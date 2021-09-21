using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Disqus.Models
{
    public class DisqusComponentViewModel
    {
        public string Header { get; set; }

        public string ThreadID { get; set; }

        public IEnumerable<DisqusPost> Posts { get; set; }
    }
}
