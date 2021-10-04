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
        /// Data representing the currently authenticated Disqus user which is serialized into a cookie
        /// </summary>
        public DisqusCurrentUser CurrentUser { get; set; }

        /// <summary>
        /// Returns the thread ID of an existing thread, or creates a new thread if one doesn't exist
        /// </summary>
        /// <param name="identifier">Thread identifier</param>
        /// <param name="node">Current page</param>
        /// <returns>A thread ID</returns>
        public abstract Task<string> GetThreadIdByIdentifier(string identifier, TreeNode node);

        /// <summary>
        /// Gets a <see cref="DisqusThread"/> object based on Disqus thread ID
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
        /// Returns shallow post details from the Disqus API
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract Task<DisqusPost> GetPostShallow(string id);

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
        public abstract Task<JObject> SubmitVote(string postId, int value);

        /// <summary>
        /// Flags a post for moderator review. See https://help.disqus.com/en/articles/1717148-flagging-comments
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public abstract Task<JObject> ReportPost(string postId, int reason);

        /// <summary>
        /// Returns a shallow list of a thread's posts from the Disqus API.
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public abstract Task<IEnumerable<DisqusPost>> GetThreadPostsShallow(string threadId);

        /// <summary>
        /// Gets full user details based on Disqus user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<JObject> GetUserDetails(string userId);

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
        /// Returns true if the user has authenticated with Disqus
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAuthenticated();

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
