using CMS.Core;
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
        private readonly bool debug;
        private readonly string site;
        private readonly string secret;
        private readonly string publicKey;
        private readonly string authRedirect;

        private readonly IPageUrlRetriever pageUrlRetriever;
        private readonly IEventLogService eventLogService;

        public DisqusCookie AuthCookie
        {
            get
            {
                var data = CookieHelper.GetValue(DisqusConstants.AUTH_COOKIE_DATA);
                if (!string.IsNullOrEmpty(data))
                {
                    return JsonConvert.DeserializeObject<DisqusCookie>(data);
                }

                return null;
            }

            set
            {
                var json = JsonConvert.SerializeObject(value);
                CookieHelper.SetValue(DisqusConstants.AUTH_COOKIE_DATA, json, DateTime.Now.AddDays(90), sameSiteMode: CMS.Base.SameSiteMode.None, secure: true);
            }
        }

        public DisqusService(IConfiguration config,
            IPageUrlRetriever pageUrlRetriever,
            IEventLogService eventLogService)
        {
            site = config.GetValue<string>("Disqus:Site");
            secret = config.GetValue<string>("Disqus:ApiSecret");
            publicKey = config.GetValue<string>("Disqus:ApiKey");
            authRedirect = config.GetValue<string>("Disqus:AuthenticationRedirect");
            debug = config.GetValue<bool>("Disqus:Debug", false);

            this.pageUrlRetriever = pageUrlRetriever;
            this.eventLogService = eventLogService;
        }

        public bool IsAuthenticated()
        {
            return AuthCookie != null && !string.IsNullOrEmpty(AuthCookie.Access_Token);
        }

        public string GetAuthenticationUrl()
        {
            return string.Format(DisqusConstants.AUTH_URL, publicKey, authRedirect);
        }

        public HttpContent GetTokenPostData(string code)
        {
            return new FormUrlEncodedContent(new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", publicKey),
                new KeyValuePair<string, string>("client_secret", secret),
                new KeyValuePair<string, string>("redirect_uri", authRedirect),
                new KeyValuePair<string, string>("code", code)
            });
        }

        public async Task<DisqusForum> GetForum(string forumId)
        {
            var url = string.Format(DisqusConstants.FORUM_DETAILS, forumId);
            var response = await MakeGetRequest(url);

            var forumJson = JsonConvert.SerializeObject(response.SelectToken("$.response"));
            return JsonConvert.DeserializeObject<DisqusForum>(forumJson);
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
            var url = string.Format(DisqusConstants.THREAD_LISTING, site);
            var getThreadsResponse = await MakeGetRequest(url);
            var foundThread = getThreadsResponse.SelectTokens($"$.response[?(@.identifiers[0] == '{identifier};{node.NodeID}')].id");

            if (foundThread.Count() > 0)
            {
                return foundThread.FirstOrDefault().Value<string>();
            }
            else
            {
                // Thread with identifier doesn't exist yet
                var pageUrl = pageUrlRetriever.Retrieve(node).AbsoluteUrl;
                var createResponse = await CreateThread(identifier, node.DocumentName, pageUrl, node.NodeID);
                    
                return createResponse.SelectToken("$.response.id").ToString();
            }
        }

        public async Task<JObject> CreateThread(string identifier, string title, string pageUrl, int nodeId)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("forum", site),
                new KeyValuePair<string, string>("title", title),
                // TODO: Enable url parameter when not running locally
                //new KeyValuePair<string, string>("url", pageUrl),
                new KeyValuePair<string, string>("identifier", $"{identifier};{nodeId}")
            };
            return await MakePostRequest(DisqusConstants.THREAD_CREATE, data);          
        }

        public async Task<DisqusPost> GetPost(string id)
        {
            var url = string.Format(DisqusConstants.POST_DETAILS, id);
            var postResult = await MakeGetRequest(url);
            var postJson = JsonConvert.SerializeObject(postResult.Value<JToken>("response"));
            return JsonConvert.DeserializeObject<DisqusPost>(postJson);
        }

        public async Task<IEnumerable<DisqusPost>> GetThreadPosts(string threadId)
        {
            var url = string.Format(DisqusConstants.THREAD_POSTS, threadId);
            var result = await MakeGetRequest(url);
            var posts = result.Value<JArray>("response");
            return posts.Select(o => JsonConvert.DeserializeObject<DisqusPost>(o.ToString())).ToList();
        }

        public async Task<JObject> UpdatePost(DisqusPost post)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("message", post.Message),
                new KeyValuePair<string, string>("post", post.Id)
            };

            return await MakePostRequest(DisqusConstants.POST_UPDATE, data);
        }

        public async Task<JObject> CreatePost(DisqusPost post)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("message", post.Message),
                new KeyValuePair<string, string>("thread", post.Thread)
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
                new KeyValuePair<string, string>("post", id)
            };

            return await MakePostRequest(DisqusConstants.POST_DELETE, data);
        }

        public async Task<DisqusUser> GetUserDetails(string userId)
        {
            var url = string.Format(DisqusConstants.USER_DETAILS, userId);
            var response = await MakeGetRequest(url);
            var userJson = JsonConvert.SerializeObject(response.Value<JToken>("response"));

            return JsonConvert.DeserializeObject<DisqusUser>(userJson);
        }

        public async Task<IEnumerable<DisqusUserActivityListing>> GetUserActivity(string userId, int topN)
        {
            // TODO: The Disqus API seems to only be returning "post" activities, check for other activities later
            var url = string.Format(DisqusConstants.USER_ACTIVITY, userId, topN);
            var response = await MakeGetRequest(url);
            var activityJson = JsonConvert.SerializeObject(response.Value<JToken>("response"));
            var activityArray = JsonConvert.DeserializeObject<JArray>(activityJson);

            var activities = activityArray.Select(o => JsonConvert.DeserializeObject<DisqusUserActivity>(o.ToString()));
            // TODO: Disqus seems to be returning activity from other users.. filter to current user, but check for updated API
            activities = activities.Where(a => a.Author != null && a.Author.Id == userId);
            activities.OrderByDescending(a => a.CreatedAt);

            // Sort activities into days
            var currentDate = DateTime.Now;
            var endDate = activities.Last().CreatedAt;
            var listings = new List<DisqusUserActivityListing>();
            do
            {
                var todaysActivities = activities.Where(a => a.CreatedAt.DayOfYear == currentDate.DayOfYear && a.CreatedAt.Year == currentDate.Year);
                if(todaysActivities.Count() > 0)
                {
                    listings.Add(new DisqusUserActivityListing()
                    {
                        Day = currentDate,
                        Activities = todaysActivities.ToList()
                    });
                }
                    
                currentDate = currentDate.AddDays(-1);
            }
            while (currentDate > endDate);

            return listings;
        }

        public async Task<JObject> FollowUser(string userId, bool doFollow)
        {
            var url = doFollow ? DisqusConstants.USER_FOLLOW : DisqusConstants.USER_UNFOLLOW;
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("target", userId)
            };

            return await MakePostRequest(url, data);
        }

        public async Task<JObject> SubscribeToThread(string threadId, bool doSubscribe)
        {
            var url = doSubscribe ? DisqusConstants.THREAD_SUBSCRIBE : DisqusConstants.THREAD_UNSUBSCRIBE;
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("thread", threadId)
            };

            return await MakePostRequest(url, data);
        }

        public async Task<IEnumerable<DisqusUser>> ListFollowing(string userId)
        {
            var url = string.Format(DisqusConstants.USER_LIST_FOLLOWING, userId);
            var response = await MakeGetRequest(url);
            var userJson = JsonConvert.SerializeObject(response.Value<JToken>("response"));
            var userArray = JsonConvert.DeserializeObject<JArray>(userJson);

            return userArray.Select(o => JsonConvert.DeserializeObject<DisqusUser>(o.ToString()));
        }

        public async Task<bool> IsUserFollowing(string userId)
        {
            if (IsAuthenticated())
            {
                var followingList = await ListFollowing(AuthCookie.User_Id);
                return followingList.Any(u => u.Id == userId);
            }

            return false;
        }

        public async Task<JObject> ReportPost(string postId, int reason)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("post", postId),
                new KeyValuePair<string, string>("reason", reason.ToString())
            };

            return await MakePostRequest(DisqusConstants.POST_REPORT, data);
        }

        public async Task<JObject> SubmitPostVote(string postId, int value)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("post", postId),
                new KeyValuePair<string, string>("vote", value.ToString())
            };

            return await MakePostRequest(DisqusConstants.POST_VOTE, data);
        }

        public async Task<JObject> SubmitThreadVote(string threadId, int value)
        {
            var data = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("thread", threadId),
                new KeyValuePair<string, string>("vote", value.ToString())
            };

            return await MakePostRequest(DisqusConstants.THREAD_VOTE, data);
        }

        public async Task<JObject> MakeGetRequest(string url)
        {
            if(IsAuthenticated())
            {
                url += $"&access_token={AuthCookie.Access_Token}";
            }

            url += $"&api_key={publicKey}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.GetStringAsync(url);
                client.Dispose();

                var result = JObject.Parse(response);
                if (result.Value<int>("code") == 0)
                {
                    if(debug)
                    {
                        var splitUrl = url.Split("?");
                        eventLogService.LogInformation(nameof(DisqusService), splitUrl[0], $"data:\n{splitUrl[1]}\n\nresult:\n{response}");
                    }

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
                data.Add(new KeyValuePair<string, string>("access_token", AuthCookie.Access_Token));
            }

            data.Add(new KeyValuePair<string, string>("api_secret", secret));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var response = await client.PostAsync(url, new FormUrlEncodedContent(data));
                var content = await response.Content.ReadAsStringAsync();
                var result = JObject.Parse(content);
                client.Dispose();

                if (result.Value<int>("code") == 0)
                {
                    if (debug)
                    {
                        var postedData = JsonConvert.SerializeObject(data);
                        eventLogService.LogInformation(nameof(DisqusService), url, $"data:\n{postedData}\n\nresult:\n{content}");
                    }

                    return result;
                }
                else
                {
                    throw new DisqusException(result.Value<int>("code"), result.Value<string>("response"));
                }
            }
        }
    }
}
