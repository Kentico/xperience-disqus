using Azure.AI.TextAnalytics;
using CMS.Activities;
using CMS.Helpers;

namespace Disqus.OnlineMarketing
{
    public class DisqusCommentActivityInitializer : CustomActivityInitializerBase
    {
        private readonly string message;
        private readonly int nodeId, rating;
        private readonly TextSentiment sentiment;

        public override string ActivityType
        {
            get
            {
                return "disquscomment";
            }
        }

        public DisqusCommentActivityInitializer(string message, int nodeId, TextSentiment sentiment, int rating)
        {
            this.message = message;
            this.nodeId = nodeId;
            this.sentiment = sentiment;
            this.rating = rating;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Posted {sentiment.ToString().ToLower()} comment";
            activity.ActivityValue = sentiment.ToString().ToLower();
            activity.ActivityComment = message;

            if(nodeId > 0)
            {
                activity.ActivityNodeID = nodeId;
            }

            if (rating > 0)
            {
                activity.ActivityItemDetailID = rating;
            }
        }
    }
}
