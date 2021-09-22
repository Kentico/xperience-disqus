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
                var data = CookieHelper.GetValue(IDisqusService.AUTH_COOKIE_DATA);
                if(!string.IsNullOrEmpty(data))
                {
                    return JsonConvert.DeserializeObject<DisqusCurrentUser>(data);
                }

                return null;
            }

            set
            {
                var json = JsonConvert.SerializeObject(value);
                CookieHelper.SetValue(IDisqusService.AUTH_COOKIE_DATA, json, DateTime.Now.AddDays(90), sameSiteMode: CMS.Base.SameSiteMode.None, secure: true);
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
            return string.Format(IDisqusService.AUTH_URL, mPublicKey, mAuthRedirect);
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

        public async Task<string> GetThreadByIdentifier(string identifier, TreeNode node)
        {
            var url = string.Format(IDisqusService.THREAD_LISTING, mSite, mSecret);
            var getThreadsResponse = await MakeGetRequest(url);
            if (getThreadsResponse.Value<int>("code") == 0)
            {
                // Success
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
                    if (createResponse.Value<int>("code") == 0)
                    {
                        // Thread created
                        return createResponse.SelectToken("$.response.id").ToString();
                    }
                    else
                    {
                        throw new DisqusException(createResponse.Value<int>("code"), createResponse.Value<string>("response"));
                    }
                }
            }
            else
            {
                // Failure
                throw new DisqusException(getThreadsResponse.Value<int>("code"), getThreadsResponse.Value<string>("response"));
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
            return await MakePostRequest(IDisqusService.THREAD_CREATE, data);          
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

            return await MakePostRequest(IDisqusService.POST_CREATE, data);
        }

        public async Task<IEnumerable<DisqusPost>> GetThreadPosts(string threadId)
        {
            var url = string.Format(IDisqusService.POSTS_BY_THREAD, threadId, mSecret);
            var result = await MakeGetRequest(url);
            if (result.Value<int>("code") == 0)
            {
                // Success
                var posts = result.Value<JArray>("response");
                var allPosts = posts.Select(o => JsonConvert.DeserializeObject<DisqusPost>(o.ToString())).ToList();

                // Get all posts at top level (not replies to other posts)
                var topLevelPosts = allPosts.Where(p => string.IsNullOrEmpty(p.Parent)).ToList();
                var hierarchicalPosts = topLevelPosts.Select(p =>
                {
                    p.ChildPosts = GetPostChildren(p, allPosts);
                    return p;
                }).ToList();

                return hierarchicalPosts;
            }
            else
            {
                // Failure
                throw new DisqusException(result.Value<int>("code"), result.Value<string>("response"));
            }
        }

        public List<DisqusPost> GetPostChildren(DisqusPost post, List<DisqusPost> allPosts)
        {
            var directChildren = allPosts.Where(p => p.Parent == post.Id).ToList();
            return directChildren.Select(p =>
            {
                p.ChildPosts = GetPostChildren(p, allPosts);
                return p;
            }).ToList();
        }

        public async Task<JObject> GetUserDetails(int userId)
        {
            var url = string.Format(IDisqusService.USER_DETAILS, mSecret, userId);
            var result = await MakeGetRequest(url);
            if (result.Value<int>("code") == 0)
            {
                // Success
                return result;
            }
            else
            {
                // Failure
                throw new DisqusException(result.Value<int>("code"), result.Value<string>("response"));
            }
        }

        public async Task<JObject> SubmitVote(string postId, int value)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("post", postId),
                new KeyValuePair<string, string>("vote", value.ToString()),
                new KeyValuePair<string, string>("api_secret", mSecret)
            };

            return await MakePostRequest(IDisqusService.POST_VOTE, data);
        }

        public async Task<JObject> MakeGetRequest(string url)
        {
            if(IsAuthenticated())
            {
                url += $"&access_token={CurrentUser.Token}";
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.GetStringAsync(url);
                client.Dispose();

                return JObject.Parse(response);
            }
        }

        public async Task<JObject> MakePostRequest(string url, List<KeyValuePair<string, string>> data)
        {
            if (IsAuthenticated())
            {
                data.Add(new KeyValuePair<string, string>("access_token", CurrentUser.Token));
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.PostAsync(url, new FormUrlEncodedContent(data));
                var content = await response.Content.ReadAsStringAsync();
                client.Dispose();

                return JObject.Parse(content);
            }
        }
    }
}
