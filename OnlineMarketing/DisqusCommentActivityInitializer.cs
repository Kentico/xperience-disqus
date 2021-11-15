using Azure.AI.TextAnalytics;
using CMS.Activities;

namespace Kentico.Xperience.Disqus.OnlineMarketing
{
    public class DisqusCommentActivityInitializer : CustomActivityInitializerBase
    {
        private readonly string message;
        private readonly TextSentiment sentiment;

        public override string ActivityType
        {
            get
            {
                return "disquscomment";
            }
        }

        public DisqusCommentActivityInitializer(string message, TextSentiment sentiment)
        {
            this.message = message;
            this.sentiment = sentiment;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Posted {sentiment.ToString().ToLower()} comment";
            activity.ActivityValue = sentiment.ToString().ToLower();
            activity.ActivityComment = message;
        }
    }
}