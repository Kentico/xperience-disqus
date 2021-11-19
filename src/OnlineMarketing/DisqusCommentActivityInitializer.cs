using CMS.Activities;
using System;

namespace Kentico.Xperience.Disqus.OnlineMarketing
{
    /// <summary>
    /// Initializes the required variables for logging an activity after a comment is created.
    /// </summary>
    internal class DisqusCommentActivityInitializer : CustomActivityInitializerBase
    {
        private readonly int nodeId;
        private readonly string culture;
        private readonly DisqusTextSentiment sentiment;

        /// <summary>
        /// The identifier of the activity type.
        /// </summary>
        public override string ActivityType
        {
            get
            {
                return "disquscomment";
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sentiment">The result of Sentiment Analysis, or <see cref="TextSentiment.Neutral"/> if not enabled.</param>
        /// <param name="nodeId">The page on which the comment was submitted.</param>
        /// <param name="culture">The culture of the page.</param>
        public DisqusCommentActivityInitializer(DisqusTextSentiment sentiment, int nodeId, string culture)
        {
            this.nodeId = nodeId;
            this.culture = culture;
            this.sentiment = sentiment;
        }


        public override void Initialize(IActivityInfo activity)
        {
            var activitySentiment = sentiment != DisqusTextSentiment.Uknown ? sentiment.ToString().ToLower() : String.Empty;
            activity.ActivityTitle = $"Posted {activitySentiment} comment";
            activity.ActivityValue = sentiment.ToString().ToLower();
            

            if (!String.IsNullOrEmpty(culture))
            {
                activity.ActivityCulture = culture;
            }

            if (nodeId > 0)
            {
                activity.ActivityNodeID = nodeId;
            }
        }
    }
}