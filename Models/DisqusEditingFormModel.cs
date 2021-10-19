using Microsoft.AspNetCore.Mvc;

namespace Disqus.Models
{
    public class DisqusEditingFormModel
    {
        [HiddenInput]
        public string Message { get; set; }

        [HiddenInput]
        public string PostThread { get; set; }

        [HiddenInput]
        public string EditedPostId { get; set; }

        [HiddenInput]
        public string ReplyTo { get; set; } = "";

        [HiddenInput]
        public int ThreadRating { get; set; }

        public bool RatingsEnabled { get; set; } = false;

        public string AnonName { get; set; }

        public string AnonEmail { get; set; }

        public bool AllowAnon { get; set; } = false;
    }
}
