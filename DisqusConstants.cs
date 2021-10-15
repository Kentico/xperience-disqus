namespace Disqus
{
    public static class DisqusConstants
    {
        #region API
        private static string BASE_URL = "https://disqus.com/api/3.0/",
                              BASE_URL_HTTP = "http://disqus.com/api/3.0/";

        public static string USER_DETAILS = BASE_URL + "users/details.json?user={0}",
                             USER_ACTIVITY = BASE_URL + "users/listActivity.json?related=thread&user={0}&limit={1}",
                             USER_FOLLOW = BASE_URL + "users/follow.json",
                             USER_UNFOLLOW = BASE_URL + "users/unfollow.json",
                             USER_LIST_FOLLOWING = BASE_URL + "users/listFollowing.json?user={0}",
                             USER_CHECK = BASE_URL + "users/checkUsername.json";

        public static string THREAD_LISTING = BASE_URL + "forums/listThreads.json?forum={0}",
                             THREAD_CREATE = BASE_URL + "threads/create.json",
                             THREAD_DETAILS = BASE_URL + "threads/details.json?thread={0}",
                             THREAD_POSTS = BASE_URL + "threads/listPosts.json?thread={0}",
                             THREAD_SUBSCRIBE = BASE_URL + "threads/subscribe.json",
                             THREAD_UNSUBSCRIBE = BASE_URL + "threads/unsubscribe.json",
                             THREAD_VOTE = BASE_URL + "threads/vote.json";

        public static string POST_CREATE = BASE_URL + "posts/create.json",
                             POST_CREATE_ANON = BASE_URL_HTTP + "posts/create.json",
                             POST_UPDATE = BASE_URL + "posts/update.json",
                             POST_DELETE = BASE_URL + "posts/remove.json",
                             POST_VOTE = BASE_URL + "posts/vote.json",
                             POST_DETAILS = BASE_URL + "posts/details.json?post={0}",
                             POST_REPORT = BASE_URL + "posts/report.json";

        public static string FORUM_DETAILS = BASE_URL + "forums/details.json?forum={0}";
        #endregion

        #region Authentication
        public static string AUTH_COOKIE_DATA = "kx_disqus_currentuser";
        public static string AUTH_URL = "https://disqus.com/api/oauth/2.0/authorize/?client_id={0}&response_type=code&redirect_uri={1}";
        public static string TOKEN_URL = "https://disqus.com/api/oauth/2.0/access_token/";
        #endregion
    
        public enum DisqusAction
        {
            VOTE,
            UPDATE,
            CREATE,
            REPORT
        }
    }
}
