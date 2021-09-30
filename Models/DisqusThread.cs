using Disqus.Services;
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

        public string Message { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public bool RatingsEnabled { get; set; }

        public bool IsClosed { get; set; }

        public int Posts { get; set; }

        public bool ValidateAllPosts { get; set; }

        /// <summary>
        /// Returns the identifier of the thread by trimming the NodeId
        /// </summary>
        /// <returns></returns>
        public string GetIdentifier()
        {
            var identifier = Identifiers[0].ToString();
            return identifier.Split(";")[0];
        }

        /// <summary>
        /// Returns the NodeID of the page the thread was created on by trimming the identifier
        /// </summary>
        /// <returns></returns>
        public int GetNodeId()
        {
            var id = 0;
            var identifier = Identifiers[0].ToString().Split(";");
            int.TryParse(identifier[1], out id);
            return id;
        }
    }
}
