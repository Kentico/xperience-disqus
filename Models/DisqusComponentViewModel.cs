using System.Collections.Generic;
using System.Linq;

namespace Disqus.Models
{
    public class DisqusComponentViewModel
    {
        /// <summary>
        /// If the Disqus API encountered an exception during retrieval of threads, posts, etc.
        /// we can use this to display a nice message in the widget instead of throwing.
        /// </summary>
        public DisqusException Exception { get; set; }

        public string Header { get; set; }

        public DisqusThread Thread { get; set; }

        public IEnumerable<DisqusPost> Posts { get; set; } = Enumerable.Empty<DisqusPost>();
    }
}
