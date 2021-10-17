using CMS.Activities;
using Disqus.Models;

namespace Disqus.OnlineMarketing
{
    public class DisqusReportActivityInitializer : CustomActivityInitializerBase
    {
        private readonly string message;
        private readonly int nodeId;
        private readonly ReportReason reason;

        public override string ActivityType
        {
            get
            {
                return "disqusreport";
            }
        }

        public DisqusReportActivityInitializer(string message, int nodeId, ReportReason reason)
        {
            this.message = message;
            this.nodeId = nodeId;
            this.reason = reason;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Reported Disqus comment";
            activity.ActivityValue = reason.ToString();
            activity.ActivityComment = $"Reported comment: {message}";
            activity.ActivityNodeID = nodeId;
        }
    }
}
