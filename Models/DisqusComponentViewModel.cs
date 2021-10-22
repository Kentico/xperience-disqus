using System.Collections.Generic;
using System.Linq;

namespace Disqus.Models
{
    public class DisqusComponentViewModel
    {
        public string Header { get; set; }

        public DisqusThread Thread { get; set; }

        public IEnumerable<DisqusPost> Posts { get; set; } = Enumerable.Empty<DisqusPost>();
    }
}
