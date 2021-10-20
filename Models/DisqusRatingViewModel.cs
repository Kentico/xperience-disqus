namespace Disqus.Models
{
    public class DisqusRatingViewModel
    {
        public string StarId { get; set; }

        public double Rating { get; set; } = 0;

        public bool Disabled { get; set; }

        public string Classes { get; set; }

        public bool DisplaySummary { get; set; } = false;

        public string ThreadId { get; set; }
    }
}
