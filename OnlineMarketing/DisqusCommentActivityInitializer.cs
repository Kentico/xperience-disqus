using Azure.AI.TextAnalytics;
using CMS.Activities;
using Disqus.Models;

namespace Disqus.OnlineMarketing
{
    public class DisqusCommentActivityInitializer : CustomActivityInitializerBase
    {
        private readonly string message;
        private readonly int nodeId;
        private readonly TextSentiment sentiment;

        public override string ActivityType
        {
            get
            {
                return "disquscomment";
            }
        }

        public DisqusCommentActivityInitializer(string message, int nodeId, TextSentiment sentiment)
        {
            this.message = message;
            this.nodeId = nodeId;
            this.sentiment = sentiment;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Posted {sentiment.ToString().ToLower()} comment";
            activity.ActivityValue = sentiment.ToString().ToLower();
            activity.ActivityComment = message;
            activity.ActivityNodeID = nodeId;
        }
    }
}
