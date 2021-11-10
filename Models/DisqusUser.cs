using Newtonsoft.Json.Linq;
using System;

namespace Kentico.Xperience.Disqus.Models
{
    public class DisqusUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public JToken Avatar { get; set; }

        public int NumFollowers { get; set; }

        public int NumFollowing { get; set; }

        public int NumPosts { get; set; }

        public int ThreadRating { get; set; } = 0;

        public int NumLikesReceived { get; set; }

        public string ReputationLabel { get; set; }

        public bool IsPowerContributor { get; set; }

        public DateTime JoinedAt { get; set; }

        public string ProfileUrl { get; set; }

        public string Url { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsAnonymous { get; set; }

        public string AvatarUrl
        {
            get
            {
                var url = Avatar.SelectToken("$.permalink").ToString();
                if (url.StartsWith("//"))
                {
                    url = $"https:{url}";
                }

                return url;
            }
        }
    }
}
