using CMS.DocumentEngine;
using CMS.Helpers;
using Disqus.Models;
using Kentico.Content.Web.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Disqus.Services
{
    public class DisqusService : IDisqusService
    {
        private readonly string mSite;
        private readonly string mSecret;
        private readonly string mPublicKey;
        private readonly string mAuthRedirect;

        private readonly IPageUrlRetriever pageUrlRetriever;

        public DisqusCurrentUser CurrentUser
        {
            get
            {
                var data = CookieHelper.GetValue(DisqusConstants.AUTH_COOKIE_DATA);
                if(!string.IsNullOrEmpty(data))
                {
                    return JsonConvert.DeserializeObject<DisqusCurrentUser>(data);
                }

                return null;
            }

            set
            {
                var json = JsonConvert.SerializeObject(value);
                CookieHelper.SetValue(DisqusConstants.AUTH_COOKIE_DATA, json, DateTime.Now.AddDays(90), sameSiteMode: CMS.Base.SameSiteMode.None, secure: true);
            }
        }

        public DisqusService(IConfiguration config, IPageUrlRetriever pageUrlRetriever)
        {
            mSite = config.GetValue<string>("Disqus:Site");
            mSecret = config.GetValue<string>("Disqus:ApiSecret");
            mPublicKey = config.GetValue<string>("Disqus:ApiKey");
            mAuthRedirect = config.GetValue<string>("Disqus:AuthenticationRedirect");

            this.pageUrlRetriever = pageUrlRetriever;
        }

        public bool IsAuthenticated()
        {
            return CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.Token);
        }

        public string GetAuthenticationUrl()
        {
            return string.Format(DisqusConstants.AUTH_URL, mPublicKey, mAuthRedirect);
        }

        public HttpContent GetTokenPostData(string code)
        {
            return new FormUrlEncodedContent(new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", mPublicKey),
                new KeyValuePair<string, string>("client_secret", mSecret),
                new KeyValuePair<string, string>("redirect_uri", mAuthRedirect),
                new KeyValuePair<string, string>("code", code)
            });
        }

        public async Task<DisqusThread> GetThread(string threadId)
        {
            var url = string.Format(DisqusConstants.THREAD_DETAILS, threadId);
            var response = await MakeGetRequest(url);

            var threadJson = JsonConvert.SerializeObject(response.SelectToken("$.response"));
            return JsonConvert.DeserializeObject<DisqusThread>(threadJson);
        }

        public async Task<string> GetThreadIdByIdentifier(string identifier, TreeNode node)
        {
            var url = string.Format(DisqusConstants.THREAD_LISTING, mSite);
            var getThreadsResponse = await MakeGetRequest(url);
            var foundThread = getThreadsResponse.SelectTokens($"$.response[?(@.identifiers[0] == '{identifier}')].id");

            if (foundThread.Count() > 0)
            {
                return foundThread.FirstOrDefault().Value<string>();
            }
            else
            {
                // Thread with identifier doesn't exist yet
                var pageUrl = pageUrlRetriever.Retrieve(node).AbsoluteUrl;
                var createResponse = await CreateThread(identifier, node.DocumentName, pageUrl);
                    
                return createResponse.SelectToken("$.response.id").ToString();
            }
        }

        public async Task<JObject> CreateThread(string identifier, string title, string pageUrl)
        {
            // TODO: Add URL parameter to thread create
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("forum", mSite),
                new KeyValuePair<string, string>("title", title),
                new KeyValuePair<string, string>("identifier", identifier),
                new KeyValuePair<string, string>("api_secret", mSecret)
            };
            return await MakePostRequest(DisqusConstants.THREAD_CREATE, data);          
        }

        public async Task<DisqusPost> GetPost(string id)
        {
            // Get post
            var postUrl = string.Format(DisqusConstants.POST_DETAILS, id);
            var postResult = await MakeGetRequest(postUrl);
            var postJson = JsonConvert.SerializeObject(postResult.Value<JToken>("response"));
            var post = JsonConvert.DeserializeObject<DisqusPost>(postJson);

            // Get thread
            var thread = await GetThread(post.Thread);
            post.ThreadObject = thread;

            // Get all posts in thread
            var url = string.Format(DisqusConstants.THREAD_POSTS, post.Thread);
            var result = await MakeGetRequest(url);
            var posts = result.Value<JArray>("response");
            var allPosts = posts.Select(o => JsonConvert.DeserializeObject<DisqusPost>(o.ToString())).ToList();

            post.ChildPosts = GetPostChildren(post, allPosts.ToList(), thread);
            return post;
        }

        public async Task<JObject> UpdatePost(DisqusPost post)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("message", post.Message),
                new KeyValuePair<string, string>("post", post.Id),
                new KeyValuePair<string, string>("api_secret", mSecret)
            };

            return await MakePostRequest(DisqusConstants.POST_UPDATE, data);
        }

        public async Task<JObject> CreatePost(DisqusPost post)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("message", post.Message),
                new KeyValuePair<string, string>("thread", post.Thread),
                new KeyValuePair<string, string>("api_secret", mSecret)
            };
            if(!string.IsNullOrEmpty(post.Parent))
            {
                data.Add(new KeyValuePair<string, string>("parent", post.Parent));
            }

            return await MakePostRequest(DisqusConstants.POST_CREATE, data);
        }

        public async Task<JObject> DeletePost(string id)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("post", id),
                new KeyValuePair<string, string>("api_secret", mSecret)
            };

            return await MakePostRequest(DisqusConstants.POST_DELETE, data);
        }

        public async Task<IEnumerable<DisqusPost>> GetThreadPosts(string threadId)
        {
            // Get thread
            var thread = await GetThread(threadId);

            // Get posts
            var url = string.Format(DisqusConstants.THREAD_POSTS, threadId);
            var result = await MakeGetRequest(url);
            var posts = result.Value<JArray>("response");
            var allPosts = posts.Select(o => JsonConvert.DeserializeObject<DisqusPost>(o.ToString())).ToList();

            // Get all posts at top level (not replies to other posts)
            var topLevelPosts = allPosts.Where(p => string.IsNullOrEmpty(p.Parent)).ToList();
            var hierarchicalPosts = topLevelPosts.Select(p =>
            {
                p.ThreadObject = thread;
                p.ChildPosts = GetPostChildren(p, allPosts, thread);
                return p;
            }).ToList();

            return hierarchicalPosts;
        }

        public List<DisqusPost> GetPostChildren(DisqusPost post, List<DisqusPost> allPosts, DisqusThread thread)
        {
            var directChildren = allPosts.Where(p => p.Parent == post.Id).ToList();
            return directChildren.Select(p =>
            {
                p.ThreadObject = thread;
                p.ChildPosts = GetPostChildren(p, allPosts, thread);
                return p;
            }).ToList();
        }

        public async Task<JObject> GetUserDetails(string userId)
        {
            var url = string.Format(DisqusConstants.USER_DETAILS, userId);
            return await MakeGetRequest(url);
        }

        public async Task<JObject> SubmitVote(string postId, int value)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("post", postId),
                new KeyValuePair<string, string>("vote", value.ToString()),
                new KeyValuePair<string, string>("api_secret", mSecret)
            };

            return await MakePostRequest(DisqusConstants.POST_VOTE, data);
        }

        public async Task<JObject> MakeGetRequest(string url)
        {
            if(IsAuthenticated())
            {
                url += $"&access_token={CurrentUser.Token}";
            }

            url += $"&api_key={mPublicKey}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.GetStringAsync(url);
                client.Dispose();

                var result = JObject.Parse(response);
                if (result.Value<int>("code") == 0)
                {
                    return result;
                }
                else
                {
                    throw new DisqusException(result.Value<int>("code"), result.Value<string>("response"));
                }
            }
        }

        public async Task<JObject> MakePostRequest(string url, List<KeyValuePair<string, string>> data)
        {
            if (IsAuthenticated())
            {
                data.Add(new KeyValuePair<string, string>("access_token", CurrentUser.Token));
            }

            data.Add(new KeyValuePair<string, string>("api_secret", mSecret));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.PostAsync(url, new FormUrlEncodedContent(data));
                var content = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(content);
                client.Dispose();

                if (result.Value<int>("code") == 0)
                {
                    
                    return JObject.Parse(content);
                }
                else
                {
                    throw new DisqusException(result.Value<int>("code"), result.Value<string>("response"));
                }
            }
        }
    }
}
