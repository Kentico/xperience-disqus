namespace Disqus
{
    public class DisqusConstants
    {
        // Endpoints
        private static string BASE_URL = "https://disqus.com/api/3.0/";
        public static string THREAD_LISTING = BASE_URL + "forums/listThreads.json?forum={0}&api_secret={1}";
        public static string THREAD_CREATE = BASE_URL + "threads/create.json";
        public static string THREAD_DETAILS = BASE_URL + "threads/details.json?thread={0}&api_secret={1}";
        public static string POSTS_BY_THREAD = BASE_URL + "threads/listPosts.json?thread={0}&api_secret={1}";
        public static string USER_DETAILS = BASE_URL + "users/details.json?api_secret={0}&user={1}";
        public static string POST_CREATE = BASE_URL + "posts/create.json";
        public static string POST_VOTE = BASE_URL + "posts/vote.json";

        // Authentication
        public static string AUTH_COOKIE_DATA = "kx_disqus_currentuser";
        public static string AUTH_URL = "https://disqus.com/api/oauth/2.0/authorize/?client_id={0}&response_type=code&redirect_uri={1}";
    }
}
