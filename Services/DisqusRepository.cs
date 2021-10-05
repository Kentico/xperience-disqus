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

        private readonly IDisqusService disqusService;
        private readonly IEventLogService eventLogService;

        public DisqusRepository(IDisqusService disqusService, IEventLogService eventLogService)
        {
            this.disqusService = disqusService;
            this.eventLogService = eventLogService;
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
        /// Gets post details from Disqus, with <see cref="DisqusPost.ThreadObject"/> and
        /// <see cref="DisqusPost.ChildPosts"/> populated
        /// </summary>
        /// <param name="postId">The Disqus internal ID</param>
        /// <param name="useCache">If true, the post is returned from cache (if found) instead of the Disqus API</param>
        /// <returns>A Disqus post</returns>
        public async Task<DisqusPost> GetPost(string postId, bool useCache = true)
        {
            if (useCache)
            {
                var foundPosts = allPosts.Where(p => p.Id == postId);
                if (foundPosts.Count() > 0)
                {
                    var post = foundPosts.FirstOrDefault();
                    var thread = await GetThread(post.Thread, useCache);

                    post.ThreadObject = thread;
                    post.ChildPosts = GetPostChildren(post, thread);

                    return post;
                }
            }

            try
            {
                var post = await disqusService.GetPostShallow(postId);
                var thread = await GetThread(post.Thread, useCache);
                await GetPostHierarchy(thread.Id, useCache);

                post.ThreadObject = thread;
                post.ChildPosts = GetPostChildren(post, thread);

                return post;
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetPost));
                return null;
            }
        }

        /// <summary>
        /// Gets all of the thread's posts sorted into a hierarchical list
        /// </summary>
        /// <param name="threadId">The Disqus internal ID</param>
        /// <param name="useCache">If true, the post is returned from cache instead of the Disqus API</param>
        /// <returns></returns>
        public async Task<IEnumerable<DisqusPost>> GetPostHierarchy(string threadId, bool useCache = true)
        {
            if (useCache)
            {
                var thread = await GetThread(threadId, useCache);
                var topLevelPosts = allPosts.Where(p => p.Thread == threadId && string.IsNullOrEmpty(p.Parent)).ToList();
                return topLevelPosts.Select(p =>
                {
                    p.ThreadObject = thread;
                    p.ChildPosts = GetPostChildren(p, thread);
                    return p;
                }).ToList();
            }

            try
            {
                var thread = await GetThread(threadId, useCache);
                var threadPosts = await disqusService.GetThreadPostsShallow(threadId);
                foreach (var post in threadPosts)
                {
                    AddPostCache(post);
                }

                var topLevelPosts = threadPosts.Where(p => string.IsNullOrEmpty(p.Parent)).ToList();
                var hierarchicalPosts = topLevelPosts.Select(p =>
                {
                    p.ThreadObject = thread;
                    p.ChildPosts = GetPostChildren(p, thread);
                    return p;
                }).ToList();

                return hierarchicalPosts;
            }
            catch(DisqusException ex)
            {
                LogError(ex, nameof(GetPostHierarchy));
                return null;
            }
        }

        /// <summary>
        /// Gets a list of the posts direct children. The method is called recursively to also set the
        /// <see cref="DisqusPost.ChildPosts"/> of all children as well
        /// </summary>
        /// <remarks>
        /// This method always uses the cached data
        /// </remarks>
        /// <param name="post"></param>
        /// <param name="thread"></param>
        /// <returns>A list of all the posts children</returns>
        public List<DisqusPost> GetPostChildren(DisqusPost post, DisqusThread thread)
        {
            var directChildren = allPosts.Where(p => p.Parent == post.Id).ToList();
            return directChildren.Select(p =>
            {
                p.ThreadObject = thread;
                p.ChildPosts = GetPostChildren(p, thread);
                return p;
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
        }

        /// <summary>
        /// Adds a <see cref="DisqusPost"/> to the repository cache. Removes the posts from cache
        /// first, if it exists
        /// </summary>
        /// <param name="post"></param>
        public void AddPostCache(DisqusPost post)
        {
            var existingPosts = allPosts.Where(p => p.Id == post.Id);
            if(existingPosts.Count() > 0)
            {
                allPosts.Remove(existingPosts.FirstOrDefault());
            }

            allPosts.Add(post);
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
        public bool UpdatePostCache(string postId, DisqusConstants.DisqusAction action, dynamic data)
        {
            var post = allPosts.Where(p => p.Id == postId).FirstOrDefault();
            if(post == null)
            {
                return false;
            }

            switch (action)
            {
                case DisqusConstants.DisqusAction.VOTE:

                    post.Likes = data.likes;
                    post.Dislikes = data.dislikes;
                    return true;
                case DisqusConstants.DisqusAction.UPDATE:

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
