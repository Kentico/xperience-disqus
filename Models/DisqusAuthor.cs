using Newtonsoft.Json.Linq;
using System;

namespace Disqus.Models
{
    public class DisqusAuthor
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public bool IsPowerContributor { get; set; }

        public DateTime JoinedAt { get; set; }

        public string ProfileUrl { get; set; }

        public string Url { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsAnonymous { get; set; }

        public JToken Avatar { get; set; }

        /// <summary>
        /// Gets the absolute URL to a user's avatar by parsing the <see cref="Avatar"/> property
        /// </summary>
        /// <returns></returns>
        public string GetAvatarUrl()
        {
            return Avatar.SelectToken("$.large.cache").ToString();
        }
    }
}
