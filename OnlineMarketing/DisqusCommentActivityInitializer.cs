using Azure.AI.TextAnalytics;
using CMS.Activities;
using Disqus.Models;

namespace Disqus.OnlineMarketing
{
    public class DisqusCommentActivityInitializer : CustomActivityInitializerBase
    {
        private readonly DisqusPost post;
        private readonly TextSentiment sentiment;

        public override string ActivityType
        {
            get
            {
                return "disquscomment";
            }
        }

        public DisqusCommentActivityInitializer(DisqusPost post, TextSentiment sentiment)
        {
            this.post = post;
            this.sentiment = sentiment;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Posted {sentiment.ToString().ToLower()} comment";
            activity.ActivityValue = sentiment.ToString().ToLower();
            activity.ActivityComment = post.Message;
            activity.ActivityNodeID = post.NodeID;
        }
    }
}
