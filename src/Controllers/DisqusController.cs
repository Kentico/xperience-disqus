using Azure.AI.TextAnalytics;

using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.TextAnalytics.Azure;

using Kentico.Xperience.Disqus.OnlineMarketing;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;

namespace Kentico.Xperience.Disqus
{
    /// <summary>
    /// A Controller which receives AJAX requests from the Disqus widget to log On-line marketing activities.
    /// </summary>
    public class KenticoDisqusLogController : ControllerBase
    {
        private readonly IActivityLogService activityLogService;
        private readonly ISentimentAnalysisService sentimentAnalysisService;
        private readonly IEventLogService eventLogService;
        private readonly ISiteService siteService;


        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        public KenticoDisqusLogController(IActivityLogService activityLogService,
            ISentimentAnalysisService sentimentAnalysisService,
            IEventLogService eventLogService,
            ISiteService siteService)
        {
            this.activityLogService = activityLogService;
            this.sentimentAnalysisService = sentimentAnalysisService;
            this.eventLogService = eventLogService;
            this.siteService = siteService;
        }


        /// <summary>
        /// Performs Sentiment Analysis on a comment, then logs an On-line Marketing activity
        /// with the results set to <see cref="ActivityInfo.ActivityValue"/>.
        /// </summary>
        /// <param name="message">The contents of the comment.</param>
        /// <param name="nodeId">The page on which the comment was submitted.</param>
        /// <param name="culture">The culture of the page.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LogCommentActivity(string message, int nodeId, string culture)
        {
            if (!DisqusHelper.CommentActivityTrackingEnabled)
            {
                return;
            }

            var verifiedCulture = String.IsNullOrEmpty(culture) || !CultureSiteInfoProvider.IsCultureAllowed(culture, siteService.CurrentSite.SiteName) ?
                Thread.CurrentThread.CurrentCulture.Name : culture;

            try
            {
                var sentiment = DisqusTextSentiment.Uknown;
                if (IsSentimentAnalysisEnabled())
                {
                    DocumentSentiment result = sentimentAnalysisService.AnalyzeText(message, verifiedCulture, SiteContext.CurrentSiteName);
                    sentiment = TextSentimentMapper.Map(result.Sentiment);
                }

                var activityInitializer = new DisqusCommentActivityInitializer(sentiment, nodeId, verifiedCulture);
                activityLogService.Log(activityInitializer);
            }
            catch (Exception e)
            {
                eventLogService.LogError(nameof(KenticoDisqusLogController), nameof(LogCommentActivity), e.Message);
            }
        }


        /// <summary>
        /// Returns <c>true</c>, if sentiment analysis service is enabled and required keys are set.
        /// </summary>
        private static bool IsSentimentAnalysisEnabled()
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSEnableSentimentAnalysis") &&
                            !String.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIEndpoint")) &&
                            !String.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIKey"));
        }
    }
}
