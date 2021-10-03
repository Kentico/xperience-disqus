using CMS.Activities;
using Disqus.Models;

namespace Disqus.OnlineMarketing
{
    public class DisqusReportActivityInitializer : CustomActivityInitializerBase
    {
        private readonly DisqusPost post;
        private readonly ReportReason reason;

        public override string ActivityType
        {
            get
            {
                return "disqusreport";
            }
        }

        public DisqusReportActivityInitializer(DisqusPost post, ReportReason reason)
        {
            this.post = post;
            this.reason = reason;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Reported Disqus comment";
            activity.ActivityValue = reason.ToString();
            activity.ActivityComment = $"Reported comment: {post.Message}";
            activity.ActivityNodeID = post.ThreadObject.GetNodeId();
        }
    }
}
