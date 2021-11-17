using Azure.AI.TextAnalytics;
using CMS.Activities;
using System;

namespace Kentico.Xperience.Disqus.OnlineMarketing
{
    /// <summary>
    /// Initializes the required variables for logging an activity after a comment is created.
    /// </summary>
    public class DisqusCommentActivityInitializer : CustomActivityInitializerBase
    {
        private readonly int nodeId;
        private readonly string message;
        private readonly string culture;
        private readonly TextSentiment sentiment;

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
        /// <param name="message">The contents of the comment.</param>
        /// <param name="sentiment">The result of Sentiment Analysis, or <see cref="TextSentiment.Neutral"/> if not enabled.</param>
        /// <param name="nodeId">The page on which the comment was submitted.</param>
        /// <param name="culture">The culture of the page.</param>
        public DisqusCommentActivityInitializer(string message, TextSentiment sentiment, int nodeId, string culture)
        {
            this.nodeId = nodeId;
            this.message = message;
            this.culture = culture;
            this.sentiment = sentiment;
        }

        public override void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = $"Posted {sentiment.ToString().ToLower()} comment";
            activity.ActivityValue = sentiment.ToString().ToLower();
            activity.ActivityComment = message;

            if (!String.IsNullOrEmpty(culture))
            {
                activity.ActivityCulture = culture;
            }

            if(nodeId > 0)
            {
                activity.ActivityNodeID = nodeId;
            }
        }
    }
}