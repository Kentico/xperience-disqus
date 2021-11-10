using System;
using System.Collections.Generic;

namespace Kentico.Xperience.Disqus.Models
{
    public class DisqusUserActivityListing
    {
        public DateTime Day { get; set; }

        public IEnumerable<DisqusUserActivity> Activities { get; set; }
    }
}
