using Newtonsoft.Json.Linq;

namespace Kentico.Xperience.Disqus.Models
{
    public class DisqusThread
    {
        public string Id { get; set; }

        public JArray Identifiers { get; set; }

        public string Feed { get; set; }

        public string Clean_Title { get; set; }

        public string Link { get; set; }

        public bool UserSubscription { get; set; }

        public int UserScore { get; set; }

        public string SignedLink { get; set; }

        public string Message { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public bool RatingsEnabled { get; set; }

        public bool IsClosed { get; set; }

        public int Posts { get; set; } = 0;

        public bool ValidateAllPosts { get; set; }

        public string Forum { get; set; }

        public int NodeID { get; set; }
    }
}
