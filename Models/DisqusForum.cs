using Newtonsoft.Json;
using System;

namespace Disqus.Models
{
    public class DisqusForum
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DisqusForumSettings Settings { get; set; }

        public bool DisableDisqusBranding { get; set; }

        public int DaysThreadAlive { get; set; }

        public string Url { get; set; }

        public int Sort { get; set; }

        public string Founder { get; set; }

        public int VotingType { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ModeratorBadgeText { get; set; }

        public string CommentsLinkOne { get; set; }

        public string CommentsLinkZero { get; set; }

        public string CommentsLinkMultiple { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CommentsPlaceholderTextEmpty { get; set; } = "Start the discussion...";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CommentsPlaceholderTextPopulated { get; set; } = "Join the discussion...";

        public string CommentPolicyLink { get; set; }

        public string CommentPolicyText { get; set; }
    }
}
