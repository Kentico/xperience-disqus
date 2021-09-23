using Newtonsoft.Json.Linq;

namespace Disqus.Models
{
    public class DisqusThread
    {
        public string Id { get; set; }

        public JArray Identifiers { get; set; }

        public string Feed { get; set; }

        public string Clean_Title { get; set; }

        public string SignedLink { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public bool RatingsEnabled { get; set; }

        public bool IsClosed { get; set; }

        public int Posts { get; set; }

        public bool ValidateAllPosts { get; set; }

        public string GetIdentifier()
        {
            return Identifiers[0].ToString();
        }
    }
}
