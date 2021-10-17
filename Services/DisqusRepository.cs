using CMS.Core;
using Disqus.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Disqus.Services
{
    public class DisqusRepository
    {
        private readonly List<DisqusThread> allThreads = new List<DisqusThread>();
        private readonly List<DisqusPost> allPosts = new List<DisqusPost>();
        private readonly List<DisqusUser> allUsers = new List<DisqusUser>();
        private readonly List<DisqusForum> allForums = new List<DisqusForum>();

        private readonly IDisqusService disqusService;
        private readonly IEventLogService eventLogService;

        public DisqusRepository(IDisqusService disqusService, IEventLogService eventLogService)
        {
            this.disqusService = disqusService;
            this.eventLogService = eventLogService;
        }

        /// <summary>
        /// Get forum details from Disqus
        /// </summary>
        /// <param name="forumId">The Disqus internal ID</param>
        /// <param name="useCache">If true, the forum is returned from cache (if found) instead of the Disqus API</param>
        /// <returns>A Disqus user</returns>
        public async Task<DisqusForum> GetForum(string forumId, bool useCache = true)
        {
            if (useCache)
            {
                var foundForums = allForums.Where(f => f.Id == forumId);
                if (foundForums.Count() > 0)
                {
                    return foundForums.FirstOrDefault();
                }
            }

            try
            {
                var forum = await disqusService.GetForum(forumId);
                AddForumCache(forum);
                return forum;
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetForum));
                return null;
            }
        }

        /// <summary>
        /// Returns the full details for the currently authenticated user from cache or by requesting
        /// it from Disqus if not cached
        /// </summary>
        /// <returns></returns>
        public async Task<DisqusUser> GetCurrentUser()
        {
            if(disqusService.IsAuthenticated())
            {
                return await GetUser(disqusService.AuthCookie.User_Id);
            }

            return null;
        }

        /// <summary>
        /// Get user details from Disqus
        /// </summary>
        /// <param name="userId">The Disqus internal ID</param>
        /// <param name="useCache">If true, the user is returned from cache (if found) instead of the Disqus API</param>
        /// <returns>A Disqus user</returns>
        public async Task<DisqusUser> GetUser(string userId, bool useCache = true)
        {
            if (useCache)
            {
                var foundUsers = allUsers.Where(u => u.Id == userId);
                if (foundUsers.Count() > 0)
                {
                    return foundUsers.FirstOrDefault();
                }
            }

            try
            {
                var user = await disqusService.GetUserDetails(userId);
                AddUserCache(user);
                return user;
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetUser));
                return null;
            }
        }

        /// <summary>
        /// Gets thread details from Disqus
        /// </summary>
        /// <param name="threadId">The Disqus internal ID</param>
        /// <param name="useCache">If true, the post is returned from cache (if found) instead of the Disqus API</param>
        /// <returns>A Disqus thread</returns>
        public async Task<DisqusThread> GetThread(string threadId, bool useCache = true)
        {
            if (useCache)
            {
                var foundThreads = allThreads.Where(t => t.Id == threadId);
                if(foundThreads.Count() > 0)
                {
                    return foundThreads.FirstOrDefault();
                }
            }

            try
            {
                var thread = await disqusService.GetThread(threadId);
                AddThreadCache(thread);
                return thread;
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetThread));
                return null;
            }
        }

        /// <summary>
        /// Gets post details from repository cache
        /// </summary>
        /// <param name="postId">The Disqus internal ID</param>
        /// <returns>A Disqus post</returns>
        public DisqusPost GetPost(string postId)
        {
            var foundPosts = allPosts.Where(p => p.Id == postId);
            if (foundPosts.Count() > 0)
            {
                var post = foundPosts.FirstOrDefault();
                return post;
            }

            return null;
        }

        public async Task<IEnumerable<DisqusPost>> GetTopLevelPosts(string threadId)
        {
            try
            {
                var threadPosts = await disqusService.GetThreadPosts(threadId);
                foreach (var post in threadPosts)
                {
                    AddPostCache(post);
                }

                var topLevelPosts = threadPosts.Where(p => string.IsNullOrEmpty(p.Parent)).ToList();
                return topLevelPosts;
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetTopLevelPosts));
                return Enumerable.Empty<DisqusPost>();
            }
        }

        /// <summary>
        /// Returns only the direct children of a post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public IEnumerable<DisqusPost> GetDirectChildren(string postId)
        {
            return allPosts.Where(p => p.Parent == postId);
        }

        /// <summary>
        /// Returns all children of a post, regardless of nesting level
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public IEnumerable<DisqusPost> GetAllChildren(string postId)
        {
            var allChildren = new List<DisqusPost>();
            var directChildren = GetDirectChildren(postId);
            allChildren.AddRange(directChildren);
            foreach(var child in directChildren)
            {
                allChildren.AddRange(GetAllChildren(child.Id));
            }

            return allChildren;
        }

        /// <summary>
        /// Adds a <see cref="DisqusForum"/> to the repository cache. Removes the forum from cache
        /// first, if it exists
        /// </summary>
        /// <param name="post"></param>
        public void AddForumCache(DisqusForum forum)
        {
            var existingForums = allForums.Where(f => f.Id == forum.Id);
            if (existingForums.Count() > 0)
            {
                allForums.Remove(existingForums.FirstOrDefault());
            }
            allForums.Add(forum);
        }

        /// <summary>
        /// Adds a <see cref="DisqusPost"/> to the repository cache. Removes the post from cache
        /// first, if it exists.
        /// </summary>
        /// <param name="post"></param>
        public void AddPostCache(DisqusPost post)
        {
            RemovePostCache(post.Id);
            allPosts.Add(post);
        }

        /// <summary>
        /// Removes the <see cref="DisqusPost"/> from the cache with a matching internal ID
        /// </summary>
        /// <param name="id"></param>
        public void RemovePostCache(string id)
        {
            var existingPosts = allPosts.Where(p => p.Id == id);
            if (existingPosts.Count() > 0)
            {
                allPosts.Remove(existingPosts.FirstOrDefault());
            }
        }

        /// <summary>
        /// Adds a <see cref="DisqusThread"/> to the repository cache. Removes the thread from cache
        /// first, if it exists
        /// </summary>
        /// <param name="thread"></param>
        private void AddThreadCache(DisqusThread thread)
        {
            var existingThreads = allThreads.Where(t => t.Id == thread.Id);
            if(existingThreads.Count() > 0)
            {
                allThreads.Remove(existingThreads.FirstOrDefault());
            }

            allThreads.Add(thread);
        }

        /// <summary>
        /// Adds a <see cref="DisqusUser"/> to the repository cache. Removes the user from cache
        /// first, if it exists
        /// </summary>
        /// <param name="thread"></param>
        private void AddUserCache(DisqusUser user)
        {
            var existingUsers = allThreads.Where(u => u.Id == user.Id);
            if (existingUsers.Count() > 0)
            {
                allThreads.Remove(existingUsers.FirstOrDefault());
            }

            allUsers.Add(user);
        }

        /// <summary>
        /// Updates post data in cache depending on the performed action. Allows for refreshing of UI
        /// without querying the Discus API for updated post information
        /// </summary>
        /// <param name="postId">The Disqus internal ID</param>
        /// <param name="action">The operation performed on the post</param>
        /// <param name="data">The data that was updated</param>
        /// <returns>true if the item in cache was updated</returns>
        public bool UpdatePostCache(string postId, string action, dynamic data)
        {
            var post = allPosts.Where(p => p.Id == postId).FirstOrDefault();
            if(post == null)
            {
                return false;
            }

            switch (action)
            {
                case "vote":

                    post.Likes = data.likes;
                    post.Dislikes = data.dislikes;
                    return true;
                case "update":

                    post.Message = data.message;
                    post.Raw_Message = data.message;
                    post.IsEdited = true;
                    return true;
            }

            return false;
        }

        private void LogError(DisqusException ex, string source)
        {
            eventLogService.LogError(nameof(DisqusRepository), source, ex.Message);
        }
    }
}
