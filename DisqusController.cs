using Azure.AI.TextAnalytics;
using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.TextAnalytics.Azure;
using Kentico.Xperience.Disqus.OnlineMarketing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;

namespace Kentico.Xperience.Disqus
{
    public class DisqusController : Controller
    {
        private readonly IActivityLogService activityLogService;
        private readonly ISentimentAnalysisService sentimentAnalysisService;
        private readonly IEventLogService eventLogService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public DisqusController(IActivityLogService activityLogService,
            ISentimentAnalysisService sentimentAnalysisService,
            IEventLogService eventLogService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.activityLogService = activityLogService;
            this.sentimentAnalysisService = sentimentAnalysisService;
            this.eventLogService = eventLogService;
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Performs Sentiment Analysis on a comment, then logs an On-line Marketing activity
        /// with the results set to <see cref="ActivityInfo.ActivityValue"/>
        /// </summary>
        /// <param name="message">The contents of the comment</param>
        /// <param name="nodeId">The page on which the comment was submitted</param>
        /// <param name="culture">The culture of the page</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LogCommentActivity(string message, int nodeId, string culture)
        {
            if (String.IsNullOrEmpty(culture))
            {
                culture = Thread.CurrentThread.CurrentCulture.Name;
            }

            var sentiment = TextSentiment.Neutral;
            if (SettingsKeyInfoProvider.GetBoolValue("CMSEnableSentimentAnalysis") &&
                !String.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIEndpoint")) &&
                !String.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIKey")))
            {
                try
                {
                    DocumentSentiment result = sentimentAnalysisService.AnalyzeText(message, culture, SiteContext.CurrentSiteName);
                    sentiment = result.Sentiment;
                }
                catch (Exception e)
                {
                    eventLogService.LogError(nameof(DisqusController), nameof(LogCommentActivity), e.Message);
                }
            }

            var activityInitializer = new DisqusCommentActivityInitializer(message, sentiment, nodeId, culture);
            activityLogService.Log(activityInitializer);
        }
    }
}
