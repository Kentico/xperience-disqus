using CMS.Activities;
using Disqus.Models;

namespace Disqus.OnlineMarketing
{
    public class DisqusActivityInitializer : CustomActivityInitializerBase
    {
        private readonly DisqusThread thread;
        private readonly bool isNegative;

        public override string ActivityType
        {
            get
            {
                return isNegative ? "DisqusNegativeCommentActivity" : "DisqusPositiveCommentActivity";
            }
        }

        public DisqusActivityInitializer(DisqusThread thread, bool isNegative)
        {
            this.thread = thread;
            this.isNegative = isNegative;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Comment on thread '{thread.GetIdentifier()}'";
            activity.ActivityValue = thread.Id;
        }
    }
}
