namespace Disqus
{
    public class DisqusConstants
    {
        // Endpoints
        private static string BASE_URL = "https://disqus.com/api/3.0/";

        public static string USER_DETAILS = BASE_URL + "users/details.json?user={0}";

        public static string THREAD_LISTING = BASE_URL + "forums/listThreads.json?forum={0}";
        public static string THREAD_CREATE = BASE_URL + "threads/create.json";
        public static string THREAD_DETAILS = BASE_URL + "threads/details.json?thread={0}";
        public static string THREAD_POSTS = BASE_URL + "threads/listPosts.json?thread={0}";

        public static string POST_CREATE = BASE_URL + "posts/create.json";
        public static string POST_UPDATE = BASE_URL + "posts/update.json";
        public static string POST_DELETE = BASE_URL + "posts/remove.json";
        public static string POST_VOTE = BASE_URL + "posts/vote.json";
        public static string POST_DETAILS = BASE_URL + "posts/details.json?post={0}";
        
        // Authentication
        public static string AUTH_COOKIE_DATA = "kx_disqus_currentuser";
        public static string AUTH_URL = "https://disqus.com/api/oauth/2.0/authorize/?client_id={0}&response_type=code&redirect_uri={1}";
    }
}
