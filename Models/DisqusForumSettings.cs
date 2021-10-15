namespace Disqus.Models
{
    public class DisqusForumSettings
    {
        public bool ThreadRatingsEnabled { get; set; }

        public bool ThreadReactionsEnabled { get; set; }

        // TODO: Return to standard property once Disqus anon commenting is fixed
        public bool AllowAnonPost { get => false; }

        public bool DisableSocialShare { get; set; }

        public bool AdultContent { get; set; }

        public bool MediaEmbedEnabled { get; set; }

        public bool ValidateAllPosts { get; set; }
    }
}
