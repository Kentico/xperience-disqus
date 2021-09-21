using System;
using System.Collections.Generic;

namespace Disqus.Models
{
    public class DisqusPost
    {
        public string Id { get; set; }

        public string Message { get; set; }

        public int NumReports { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public int Points { get; set; }

        public DateTime CreatedAt { get; set; }

        public DisqusAuthor Author { get; set; }

        public IEnumerable<DisqusPost> ChildPosts { get; set; }

        public bool IsSpam { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsApproved { get; set; }

        public bool IsHighlighted { get; set; }

        public bool IsFlagged { get; set; }

        public bool IsAtFlagLimit { get; set; }

        public bool IsEdited { get; set; }

        public bool CanVote { get; set; }

        public string Parent { get; set; }

        public bool IsNewUserNeedsApproval { get; set; }
    }
}
