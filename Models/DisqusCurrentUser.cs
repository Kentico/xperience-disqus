namespace Disqus.Models
{
    public class DisqusCurrentUser
    {
        public string Token { get; set; }

        public int UserID { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Avatar { get; set; }
    }
}
