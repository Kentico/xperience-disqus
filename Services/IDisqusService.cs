using CMS.DocumentEngine;
using Disqus.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Disqus.Services
{
    /// <summary>
    /// A service for interfacing with the Disqus API: https://disqus.com/api/docs/
    /// </summary>
    public interface IDisqusService
    {
        /// <summary>
        /// Data for the currently authenticated user, which is stored in a cookie
        /// </summary>
        public DisqusCookie AuthCookie { get; set; }

        /// <summary>
        /// Returns true if the user is logged in to Disqus
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAuthenticated();

        /// <summary>
        /// Gets a <see cref="DisqusForum"/> based on Disqus forum ID
        /// </summary>
        /// <param name="forumId"></param>
        /// <returns></returns>
        public abstract Task<DisqusForum> GetForum(string forumId);

        /// <summary>
        /// Returns the thread ID of an existing thread, or creates a new thread if one doesn't exist
        /// </summary>
        /// <param name="identifier">Thread identifier</param>
        /// <param name="node">Current page</param>
        /// <returns>A thread ID</returns>
        public abstract Task<string> GetThreadIdByIdentifier(string identifier, TreeNode node);

        /// <summary>
        /// Gets a <see cref="DisqusThread"/> based on Disqus thread ID
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public abstract Task<DisqusThread> GetThread(string threadId);

        /// <summary>
        /// Creates a new thread with the provided identifier, and returns the thread internal ID
        /// </summary>
        /// <param name="identifier">An arbitrary identifier, which is combined with the NodeID to create a unique identifier</param>
        /// <returns>The response from the Disqus server</returns>
        public abstract Task<JObject> CreateThread(string identifier, string title, string pageUrl, int nodeId);

        /// <summary>
        /// Submits a like (recommend) or dislike to a thread
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="value">1 or -1 for like and dislike respectively</param>
        /// <returns></returns>
        public abstract Task<JObject> SubmitThreadVote(string threadId, int value);

        /// <summary>
        /// Gets a <see cref="DisqusPost"/> based on Disqus post ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task<DisqusPost> GetPost(string id);

        /// <summary>
        /// Creates a new post in Disqus
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public abstract Task<JObject> CreatePost(DisqusPost post);

        /// <summary>
        /// Updates a post in Disqus
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public abstract Task<JObject> UpdatePost(DisqusPost post);

        /// <summary>
        /// Deletes a post in Disqus
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task<JObject> DeletePost(string id);

        /// <summary>
        /// Submits a like or dislike to a post
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="value">1 or -1 for like and dislike respectively</param>
        /// <returns></returns>
        public abstract Task<JObject> SubmitPostVote(string postId, int value);

        /// <summary>
        /// Flags a post for moderator review. See https://help.disqus.com/en/articles/1717148-flagging-comments
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public abstract Task<JObject> ReportPost(string postId, int reason);

        /// <summary>
        /// Gets a list of a thread's posts based on Disqus thread ID
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<DisqusPost>> GetThreadPosts(string threadId);

        /// <summary>
        /// Gets a <see cref="DisqusUser"/> based on Disqus user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<DisqusUser> GetUserDetails(string userId);

        /// <summary>
        /// Gets the user's most recent Disqus activities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="topN">The number of activities to return</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<DisqusUserActivityListing>> GetUserActivity(string userId, int topN);

        /// <summary>
        /// Sets the currently authenticated user to follow/unfollow another user
        /// </summary>
        /// <param name="userId">The Disqus internal ID of the user to follow/unfollow</param>
        /// <param name="doFollow">true if should follow, false to unfollow</param>
        /// <returns></returns>
        public abstract Task<JObject> FollowUser(string userId, bool doFollow);

        /// <summary>
        /// Returns a list of users that the specified user is following
        /// </summary>
        /// <param name="userId">The Disqus internal ID of the user to list whom they are following</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<DisqusUser>> ListFollowing(string userId);

        /// <summary>
        /// Returns true if the currently authenticated user is following the <paramref name="userId"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<bool> IsUserFollowing(string userId);

        /// <summary>
        /// Sets the currently authenticated user to subscribe/unsubscribe to a thread
        /// </summary>
        /// <param name="threadId">The Disqus internal ID of the thread to subscribe/unsubscribe</param>
        /// <param name="doSubscribe">true if should subscribe, false to unsubscribe</param>
        public abstract Task<JObject> SubscribeToThread(string threadId, bool doSubscribe);

        /// <summary>
        /// Makes a GET request to the provided URL. Automatically adds the 'access_token' parameter if
        /// <see cref="UserToken"/> is populated, and the 'api_key' parameter
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public abstract Task<JObject> MakeGetRequest(string url);

        /// <summary>
        /// Makes a POST request to the provided URL with data. Automatically adds the 'access_token' parameter if
        /// <see cref="UserToken"/> is populated, and the 'api_secret' parameter
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Task<JObject> MakePostRequest(string url, List<KeyValuePair<string, string>> data);

        /// <summary>
        /// Returns Disqus' authentication endpoint with the required query string parameters
        /// </summary>
        /// <returns></returns>
        public abstract string GetAuthenticationUrl();

        /// <summary>
        /// Populates the required data for the POST to Disqus' token endpoint, after end-user approves access
        /// </summary>
        /// <param name="code">The code generated by Disqus for the current user</param>
        /// <returns></returns>
        public abstract HttpContent GetTokenPostData(string code);
    }
}
