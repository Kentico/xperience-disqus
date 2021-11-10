using CMS.Core;
using CMS.Helpers;
using Kentico.Xperience.Disqus.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kentico.Xperience.Disqus.Services
{
    public class DisqusRepository
    {
        private readonly List<DisqusThread> allThreads = new List<DisqusThread>();
        private readonly List<DisqusPost> allPosts = new List<DisqusPost>();
        private readonly List<DisqusUser> allUsers = new List<DisqusUser>();
        private readonly List<DisqusUser> allModerators = new List<DisqusUser>();

        private readonly IDisqusService disqusService;
        private readonly IEventLogService eventLogService;

        public DisqusRepository(IDisqusService disqusService, IEventLogService eventLogService)
        {
            this.disqusService = disqusService;
            this.eventLogService = eventLogService;
        }

        /// <summary>
        /// Gets the average thread rating among all post authors
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public async Task<double> GetThreadAverageRating(string threadId)
        {
            var numberOfRatings = await GetThreadNumberRatings(threadId);
            var distinctAuthors = await GetDistinctAuthors(threadId);
            var totalRating = distinctAuthors.Sum(a => a.ThreadRating);

            if (numberOfRatings == 0)
            {
                return 0;
            }
            else
            {
                return totalRating / (double)numberOfRatings;
            }
        }

        /// <summary>
        /// Returns the count of thread authors that provided a rating when posting
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public async Task<int> GetThreadNumberRatings(string threadId)
        {
            var distinctAuthors = await GetDistinctAuthors(threadId);
            return distinctAuthors.Where(a => a.ThreadRating > 0).Count();
        }

        /// <summary>
        /// Returns a distinct list of users that have posted on the thread
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DisqusUser>> GetDistinctAuthors(string threadId)
        {
            var threadPosts = await GetAllPosts(threadId);
            return threadPosts.Where(p => !p.IsDeleted).Select(p => p.Author)
                .GroupBy(u => u.Id)
                .Select(g => g.FirstOrDefault());
        }

        /// <summary>
        /// Gets all posts of the thread sorted in the order they should be rendered, and saves them to cache
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<DisqusPost>> GetAllPosts(string threadId, bool useCache = true)
        {
            if (useCache)
            {
                return allPosts.Where(p => p.Thread == threadId);
            }

            try
            {
                var threadPosts = await disqusService.GetThreadPosts(threadId);
                foreach (var post in threadPosts)
                {
                    AddPostCache(post);
                }

                var orderedPosts = new List<DisqusPost>();
                var topLevelPosts = await GetTopLevelPosts(threadId);
                foreach(var post in topLevelPosts)
                {
                    orderedPosts.Add(post);
                    orderedPosts.AddRange(GetAllPostsRecursive(post));
                }

                return orderedPosts;
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetAllPosts));
                return Enumerable.Empty<DisqusPost>();
            }
        }

        /// <summary>
        /// For a specified post, returns the all of the children in order they should be rendered.
        /// Sets the <see cref="DisqusPost.NestingLevel"/> for each child post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        private IEnumerable<DisqusPost> GetAllPostsRecursive(DisqusPost post, int currentNestingLevel = 0)
        {
            currentNestingLevel++;
            var posts = new List<DisqusPost>();
            var directChildren = GetDirectChildren(post.Id);
            foreach(var child in directChildren)
            {
                child.NestingLevel = currentNestingLevel;
                posts.Add(child);
                posts.AddRange(GetAllPostsRecursive(child, currentNestingLevel));
            }

            return posts;
        }

        /// <summary>
        /// Returns true if the specified user can moderate the current forum
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> IsModerator(string userId)
        {
            if(allModerators.Count == 0)
            {
                var moderators = await disqusService.GetForumModerators();
                allModerators.AddRange(moderators);
            }

            return allModerators.Any(u => u.Id == userId);
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
        /// Tries to locate the current user's thread rating if they have already posted on the thread
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public async Task<int> GetCurrentUserThreadRating(string threadId)
        {
            if (!disqusService.IsAuthenticated())
            {
                return 0;
            }

            var distinctAuthors = await GetDistinctAuthors(threadId);
            var foundAuthors = distinctAuthors.Where(a => a.Id == disqusService.AuthCookie.User_Id);
            if (foundAuthors.Count() > 0)
            {
                return foundAuthors.FirstOrDefault().ThreadRating;
            }

            return 0;
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

        /// <summary>
        /// Returns all thread posts that do not have a parent, ordered according to
        /// <see cref="DisqusForum.Sort"/>
        /// </summary>
        /// <param name="threadId"></param>
        /// <returns></returns>
        private async Task<IEnumerable<DisqusPost>> GetTopLevelPosts(string threadId, bool useCache = true)
        {
            try
            {
                var threadPosts = await GetAllPosts(threadId, useCache);
                var topLevelPosts = threadPosts.Where(p => string.IsNullOrEmpty(p.Parent)).ToList();

                return SortPosts(topLevelPosts);
            }
            catch (DisqusException ex)
            {
                LogError(ex, nameof(GetTopLevelPosts));
                return Enumerable.Empty<DisqusPost>();
            }
        }

        /// <summary>
        /// Returns only the direct children of a post, ordered according to
        /// <see cref="DisqusForum.Sort"/>
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        private IEnumerable<DisqusPost> GetDirectChildren(string postId)
        {
            var children = allPosts.Where(p => p.Parent == postId);
            return SortPosts(children);
        }

        /// <summary>
        /// Sorts posts according to <see cref="DisqusForum.Sort"/>
        /// </summary>
        /// <param name="posts"></param>
        /// <returns></returns>
        private IEnumerable<DisqusPost> SortPosts(IEnumerable<DisqusPost> posts)
        {
            IOrderedEnumerable<DisqusPost> orderedPosts = null;

            switch (disqusService.CurrentForum.Sort)
            {
                case (int)DisqusConstants.SortMethod.HOT:
                    orderedPosts = posts.OrderByDescending(p => p.Likes);
                    break;
                case (int)DisqusConstants.SortMethod.NEWEST:
                    orderedPosts = posts.OrderByDescending(p => p.CreatedAt);
                    break;
                case (int)DisqusConstants.SortMethod.OLDEST:
                    orderedPosts = posts.OrderBy(p => p.CreatedAt);
                    break;
            }

            return orderedPosts;
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
                    post.Author.ThreadRating = ValidationHelper.GetInteger(data.rating, 0);
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
