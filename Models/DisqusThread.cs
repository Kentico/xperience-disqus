using CMS.Core;
using CMS.DocumentEngine;
using Disqus.Services;
using Kentico.Content.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;

namespace Disqus.Models
{
    public class DisqusThread
    {
        public string Id { get; set; }

        public JArray Identifiers { get; set; }

        public string Feed { get; set; }

        public string Clean_Title { get; set; }

        public string Link { get; set; }

        public string SignedLink { get; set; }

        public string Message { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public bool RatingsEnabled { get; set; }

        public bool IsClosed { get; set; }

        public int Posts { get; set; }

        public bool ValidateAllPosts { get; set; }

        public string Forum { get; set; }

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

        /// <summary>
        /// Gets the URL of the page this thread was created on by parsing the identifier with <see cref="GetNodeId"/>
        /// </summary>
        /// <returns></returns>
        public string GetThreadUrl()
        {
            var urlService = Service.Resolve<IPageUrlRetriever>();
            var pageRetriever = Service.Resolve<IPageRetriever>();
            var nodeId = GetNodeId();
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            var nodes = pageRetriever.Retrieve<TreeNode>(query =>
                query.WhereEquals("NodeID", nodeId)
                    .Culture(culture)
                    .Published()
            );
            if (nodes.Count() > 0)
            {
                return urlService.Retrieve(nodes.FirstOrDefault()).AbsoluteUrl;
            }

            return string.Empty;
        }
    }
}
