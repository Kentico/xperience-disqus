using Azure.AI.TextAnalytics;
using CMS.Activities;
using Disqus.Models;

namespace Disqus.OnlineMarketing
{
    public class DisqusActivityInitializer : CustomActivityInitializerBase
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

        public DisqusActivityInitializer(DisqusPost post, TextSentiment sentiment)
        {
            this.post = post;
            this.sentiment = sentiment;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Comment on thread '{post.ThreadObject.GetIdentifier()}'";
            activity.ActivityValue = sentiment.ToString().ToLower();
            activity.ActivityComment = post.Message;
            activity.ActivityNodeID = post.ThreadObject.GetNodeId();
        }
    }
}
