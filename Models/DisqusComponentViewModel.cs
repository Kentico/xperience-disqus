using System.Collections.Generic;
using System.Linq;

namespace Disqus.Models
{
    public class DisqusComponentViewModel
    {
        public string Header { get; set; }

        public DisqusThread Thread { get; set; }

        public IEnumerable<DisqusPost> ParentPosts { get; set; } = Enumerable.Empty<DisqusPost>();
    }
}
