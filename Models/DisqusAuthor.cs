using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Disqus.Models
{
    public class DisqusAuthor
    {
        public string UserName { get; set; }

        public string Name { get; set; }

        public bool IsPowerContributor { get; set; }

        public DateTime JoinedAt { get; set; }

        public string ProfileUrl { get; set; }

        public string Url { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsAnonymous { get; set; }

        public JToken Avatar { get; set; }
    }
}
