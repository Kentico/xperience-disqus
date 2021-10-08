using CMS.Core;
using Disqus.Services;
using Newtonsoft.Json.Linq;

namespace Disqus.Models
{
    public class DisqusThread
    {
        private DisqusForum mForum;

        public string Id { get; set; }

        public JArray Identifiers { get; set; }

        public string Feed { get; set; }

        public string Clean_Title { get; set; }

        public string Link { get; set; }

        public bool UserSubscription { get; set; }

        public string SignedLink { get; set; }

        public string Message { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public bool RatingsEnabled { get; set; }

        public bool IsClosed { get; set; }

        public int Posts { get; set; }

        public bool ValidateAllPosts { get; set; }

        public string Forum { get; set; }

        public DisqusForum ForumObject
        {
            get
            {
                if (mForum == null)
                {
                    var repository = Service.Resolve<DisqusRepository>();
                    mForum = repository.GetForum(Forum).Result;
                }

                return mForum;
            }

            set => mForum = value;
        }

        public string PlaceholderText
        {
            get
            {
                return Posts > 0 ? ForumObject.CommentsPlaceholderTextPopulated : ForumObject.CommentsPlaceholderTextEmpty;
            }
        }

        /// <summary>
        /// Returns the NodeID of the page the thread was created on by trimming the identifier from <see cref="Identifiers"/>
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
