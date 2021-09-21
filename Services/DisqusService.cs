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

        public string UserToken {
            get
            {
                return CookieHelper.GetValue(IDisqusService.AUTH_COOKIE_TOKEN);
            }

            set
            {
                CookieHelper.SetValue(IDisqusService.AUTH_COOKIE_TOKEN, value, DateTime.Now.AddDays(90), sameSiteMode: CMS.Base.SameSiteMode.None, secure: true);
            }
        }

        public string UserName
        {
            get
            {
                return CookieHelper.GetValue(IDisqusService.AUTH_COOKIE_NAME);
            }

            set
            {
                CookieHelper.SetValue(IDisqusService.AUTH_COOKIE_NAME, value, DateTime.Now.AddDays(90), sameSiteMode: CMS.Base.SameSiteMode.None, secure: true);
            }
        }

        public DisqusService(IConfiguration config, IPageUrlRetriever pageUrlRetriever)
        {
            mSite = config.GetValue<string>("Disqus:Site");
            mSecret = config.GetValue<string>("Disqus:SecretKey");
            mPublicKey = config.GetValue<string>("Disqus:PublicKey");
            mAuthRedirect = config.GetValue<string>("Disqus:AuthenticationRedirect");

            this.pageUrlRetriever = pageUrlRetriever;
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(UserToken);
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
            var result = await MakeRequest(url);
            if (result.Value<int>("code") == 0)
            {
                // Success
                var foundThread = result.SelectTokens($"$.response[?(@.identifiers[0] == '{identifier}')].id");
                if (foundThread.Count() > 0)
                {
                    return foundThread.FirstOrDefault().Value<string>();
                }
                else
                {
                    // Thread with identifier doesn't exist yet
                    var pageUrl = pageUrlRetriever.Retrieve(node).AbsoluteUrl;
                    var response = await CreateThread(identifier, node.DocumentName, pageUrl);
                    if (response.Value<int>("code") == 0)
                    {
                        // Thread created
                        return response.SelectToken("$.response.id").ToString();
                    }
                    else
                    {
                        throw new Exception(response.Value<string>("response"));
                    }
                }
            }
            else
            {
                // Failure
                throw new Exception(result.Value<string>("response"));
            }
        }

        public async Task<JObject> CreateThread(string identifier, string title, string pageUrl)
        {
            var url = string.Format(IDisqusService.THREAD_CREATE, mSite, title, identifier, mSecret, pageUrl);
            return await MakePost(url, null);          
        }

        public async Task<IEnumerable<DisqusPost>> GetThreadPosts(string threadId)
        {
            var url = string.Format(IDisqusService.POSTS_BY_THREAD, threadId, mSecret);
            var result = await MakeRequest(url);
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
                throw new Exception(result.Value<string>("response"));
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

        public async Task<string> GetUserDetails()
        {
            var url = string.Format(IDisqusService.USER_DETAILS, mSecret);
            var result = await MakeRequest(url);
            if (result.Value<int>("code") == 0)
            {
                // Success
                var user = result.Value<JArray>("response");

                return user.ToString();
            }
            else
            {
                // Failure
                throw new Exception(result.Value<string>("response"));
            }
        }

        public async Task<JObject> MakeRequest(string url)
        {
            if(!string.IsNullOrEmpty(UserToken))
            {
                url += $"&access_token={UserToken}";
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.GetStringAsync(url);
                client.Dispose();

                return JObject.Parse(response);
            }
        }

        public async Task<JObject> MakePost(string url, HttpContent data)
        {
            if (!string.IsNullOrEmpty(UserToken))
            {
                url += $"&access_token={UserToken}";
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.PostAsync(url, data);
                var content = await response.Content.ReadAsStringAsync();
                client.Dispose();

                return JObject.Parse(content);
            }
        }
    }
}
