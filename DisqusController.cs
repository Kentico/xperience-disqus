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
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LogCommentActivity(string message)
        {
            var sentiment = TextSentiment.Neutral;
            if (SettingsKeyInfoProvider.GetBoolValue("CMSEnableSentimentAnalysis") &&
                !string.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIEndpoint")) &&
                !string.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIKey")))
            {
                try
                {
                    var culture = Thread.CurrentThread.CurrentCulture.Name;
                    DocumentSentiment result = sentimentAnalysisService.AnalyzeText(message, culture, SiteContext.CurrentSiteName);
                    sentiment = result.Sentiment;
                }
                catch (Exception e)
                {
                    eventLogService.LogError(nameof(DisqusController), nameof(LogCommentActivity), e.Message);
                }
            }

            var activityInitializer = new DisqusCommentActivityInitializer(message, sentiment);
            activityLogService.Log(activityInitializer);
        }
    }
}
