namespace Disqus.Models
{
    public class DisqusRatingViewModel
    {
        public string StarId { get; set; }

        public int Rating { get; set; } = 0;

        public bool Disabled { get; set; }

        public string Classes { get; set; }
    }
}
