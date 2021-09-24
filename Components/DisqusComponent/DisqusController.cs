using Azure.AI.TextAnalytics;
using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.TextAnalytics.Azure;
using Disqus.Models;
using Disqus.OnlineMarketing;
using Disqus.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Disqus.Components.DisqusComponent
{
    public class DisqusController : Controller
    {
        private readonly IDisqusService disqusService;
        private readonly IActivityLogService activityLogService;
        private readonly ISentimentAnalysisService sentimentAnalysisService;
        private readonly IEventLogService eventLogService;

        public DisqusController(IDisqusService disqusService,
            IActivityLogService activityLogService,
            ISentimentAnalysisService sentimentAnalysisService,
            IEventLogService eventLogService)
        {
            this.disqusService = disqusService;
            this.activityLogService = activityLogService;
            this.sentimentAnalysisService = sentimentAnalysisService;
            this.eventLogService = eventLogService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitPost(DisqusPost post)
        {
            //TODO: Disable submit button while processing
            //TODO: Fix nesting for new posts
            try
            {
                var response = await disqusService.CreatePost(post);
                await LogCommentActivity(post);

                // Get full post object for view
                var fullPost = await disqusService.GetPost(response.SelectToken("$.response.id").ToString());

                return PartialView("~/Views/Shared/Components/_DisqusPost.cshtml", fullPost);
            }
            catch(DisqusException ex)
            {
                return PartialView("~/Views/Shared/Components/_DisqusException.cshtml", ex);
            }
        }

        public async Task LogCommentActivity(DisqusPost post)
        {
            // Reconstruct thread object as it cannot be passed via the form
            post.ThreadObject = await disqusService.GetThread(post.Thread);

            // Perform Sentiment Analysis
            var isNegative = false;
            if (SettingsKeyInfoProvider.GetBoolValue("CMSEnableSentimentAnalysis") &&
                !string.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIEndpoint")) &&
                !string.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIKey")))
            {
                try
                {
                    DocumentSentiment result = sentimentAnalysisService.AnalyzeText(post.Message, "en-US", SiteContext.CurrentSiteName);
                    if (result.Sentiment == TextSentiment.Negative)
                    {
                        isNegative = true;
                    }
                }
                catch (Exception e)
                {
                    eventLogService.LogError(nameof(DisqusController), nameof(LogCommentActivity), e.Message);
                }
            }

            // Log OM activity
            var activityInitializer = new DisqusActivityInitializer(post.ThreadObject, isNegative);
            activityLogService.Log(activityInitializer);
        }

        public async Task<ActionResult> VotePost(bool isLike, string id)
        {
            var response = await disqusService.SubmitVote(id, isLike ? 1 : -1);
            var code = response.Value<int>("code");
            if (code == 0)
            {
                var voteChange = int.Parse(response.SelectToken("$.response.delta").ToString());
                if (voteChange == 0)
                {
                    var message = $"You cannot {(isLike ? "upvote" : "downvote")} this post.";
                    return new ContentResult() { Content = message, StatusCode = 403 };
                }

                // Get post (and children) so we can refresh view
                var post = await disqusService.GetPost(id);
                return View("~/Views/Shared/Components/_DisqusPost.cshtml", post);
            }
            else if (code == (int)DisqusException.DisqusErrorCode.AUTHENTICATION_REQUIRED)
            {
                return new ContentResult() { Content = "You must log in to vote on posts.", StatusCode = 401 };
            }

            throw new DisqusException(code, response.Value<string>("response"));
        }

        public async Task<ActionResult> Auth()
        {
            var code = QueryHelper.GetString("code", "");
            var url = "https://disqus.com/api/oauth/2.0/access_token/";
            var data = disqusService.GetTokenPostData(code);

            using (var client = new HttpClient())
            {
                var tokenResponse = await client.PostAsync(url, data);
                var content = await tokenResponse.Content.ReadAsStringAsync();
                client.Dispose();

                var json = JObject.Parse(content);
                var currentUser = new DisqusCurrentUser()
                {
                    UserName = json.Value<string>("username"),
                    Token = json.Value<string>("access_token"),
                    UserID = json.Value<string>("user_id")
                };

                // Get full user details
                var userResponse = await disqusService.GetUserDetails(currentUser.UserID);
                currentUser.FullName = userResponse.SelectToken("$.response.name").ToString();
                currentUser.Avatar = userResponse.SelectToken("$.response.avatar.cache").ToString();
                disqusService.CurrentUser = currentUser;

                return new RedirectResult("/");
            }
        }
    }
}
