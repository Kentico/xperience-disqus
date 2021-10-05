using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Disqus.Models
{
    public class DisqusPost
    {
        [HiddenInput]
        public string Id { get; set; }

        [HiddenInput]
        public string Thread { get; set; }

        [Required]
        [HiddenInput]
        public string Message { get; set; }

        public string Raw_Message { get; set; }

        public int NumReports { get; set; }

        public int Likes { get; set; }

        public int Dislikes { get; set; }

        public int Points { get; set; }

        public DateTime EditableUntil { get; set; }

        public DateTime CreatedAt { get; set; }

        public DisqusUser Author { get; set; }

        public IEnumerable<DisqusPost> ChildPosts { get; set; } = Enumerable.Empty<DisqusPost>();

        public bool IsSpam { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsApproved { get; set; }

        public bool IsHighlighted { get; set; }

        public bool IsFlagged { get; set; }

        public bool IsAtFlagLimit { get; set; }

        public bool IsEdited { get; set; }

        public bool CanVote { get; set; }

        [HiddenInput]
        public bool IsEditing { get; set; }

        [HiddenInput]
        public string Parent { get; set; } = string.Empty;

        public bool IsNewUserNeedsApproval { get; set; }

        public DisqusThread ThreadObject { get; set; }

        public string GetPermalink()
        {
            return $"{ThreadObject.GetThreadUrl()}#post_{Id}";
        }
    }
}
