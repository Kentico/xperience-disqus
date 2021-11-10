using Azure.AI.TextAnalytics;
using CMS.Activities;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.TextAnalytics.Azure;
using Kentico.Xperience.Disqus.Models;
using Kentico.Xperience.Disqus.OnlineMarketing;
using Kentico.Xperience.Disqus.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kentico.Xperience.Disqus.Components.DisqusComponent
{
    public class DisqusController : Controller
    {
        private readonly IDisqusService disqusService;
        private readonly DisqusRepository disqusRepository;
        private readonly IActivityLogService activityLogService;
        private readonly ISentimentAnalysisService sentimentAnalysisService;
        private readonly IEventLogService eventLogService;

        public DisqusController(IDisqusService disqusService,
            DisqusRepository disqusRepository,
            IActivityLogService activityLogService,
            ISentimentAnalysisService sentimentAnalysisService,
            IEventLogService eventLogService)
        {
            this.disqusService = disqusService;
            this.disqusRepository = disqusRepository;
            this.activityLogService = activityLogService;
            this.sentimentAnalysisService = sentimentAnalysisService;
            this.eventLogService = eventLogService;
        }

        /// <summary>
        /// Returns the full HTML of the form to create a reply to the given post ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetReplyForm(string id)
        {
            var post = disqusRepository.GetPost(id);
            return PartialView("~/Views/Shared/Components/DisqusComponent/_DisqusPostForm.cshtml", new DisqusEditingFormModel()
            {
                PostThread = post.Thread,
                ReplyTo = post.Id,
                RatingsEnabled = post.ThreadObject.RatingsEnabled && disqusService.CurrentForum.Settings.ThreadRatingsEnabled,
                ThreadRating = await disqusRepository.GetCurrentUserThreadRating(post.Thread)
            });
        }

        /// <summary>
        /// Returns the full HTML of a post for use in async post updates
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPostBody(string id)
        {
            var post = disqusRepository.GetPost(id);
            return PartialView("~/Views/Shared/Components/DisqusComponent/_DisqusPost.cshtml", post);
        }

        /// <summary>
        /// Returns the full HTML for user details, for use in a modal popup
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetUserDetailBody(string id)
        {
            var user = await disqusRepository.GetUser(id);
            return PartialView("~/Views/Shared/Components/DisqusComponent/_DisqusUserDetails.cshtml", user);
        }

        /// <summary>
        /// Called from the submit button on the _DisqusPostForm.cshtml, creates or updates a post in Disqus
        /// </summary>
        /// <param name="post"></param>
        /// <returns>A JSON object indicating success, or the error message in the case of failure</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitPost(DisqusEditingFormModel model)
        {
            // This is the default "empty" text that Quill returns
            if(string.IsNullOrEmpty(model.Message) || model.Message == "<p><br></p>")
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter a message."
                });
            }

            if(string.IsNullOrEmpty(model.EditedPostId))
            {
                return await CreatePost(model);
            }
            else
            {
                return await UpdatePost(model);
            }
            
        }

        /// <summary>
        /// Called from the submit button on the _DisqusPostForm.cshtml, updates an existing post in Disqus.
        /// Logs a custom On-line Marketing activity with Sentiment Analysis results
        /// </summary>
        /// <param name="model"></param>
        /// <returns>A JSON object indicating success, or the error message in the case of failure</returns>
        private async Task<ActionResult> UpdatePost(DisqusEditingFormModel model)
        {
            try
            {
                var data = new
                {
                    message = model.Message,
                    rating = model.ThreadRating
                };
                disqusRepository.UpdatePostCache(model.EditedPostId, "update", data);
                var response = await disqusService.UpdatePost(model.EditedPostId, model.Message, model.ThreadRating);
                await LogCommentActivity(model);

                return Json(new
                {
                    success = true,
                    action = "update",
                    url = Url.Action("GetPostBody", "Disqus"),
                    id = response.SelectToken("$.response.id").ToString(),
                    parent = response.SelectToken("$.response.parent").ToString()
                });
            }
            catch (DisqusException ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a post in Disqus
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A comma-separated list of child post IDs that should be removed from the layout</returns>
        [HttpPost]
        public async Task<ActionResult> DeletePost(string id)
        {
            var allChildren = disqusRepository.GetAllChildren(id);
            var response = await disqusService.DeletePost(id);
            disqusRepository.RemovePostCache(id);

            return Content(allChildren.Select(p => p.Id).Join(","));
        }

        /// <summary>
        /// Called from the submit button on the _DisqusPostForm.cshtml, creates a new post in Disqus.
        /// Logs a custom On-line Marketing activity with Sentiment Analysis results
        /// </summary>
        /// <param name="model"></param>
        /// <returns>A JSON object indicating success, or the error message in the case of failure</returns>
        private async Task<ActionResult> CreatePost(DisqusEditingFormModel model)
        {
            try
            {
                var response = await disqusService.CreatePost(model.Message, model.PostThread, model.ReplyTo, model.ThreadRating);
                var responseJson = JsonConvert.SerializeObject(response.SelectToken("$.response"));
                var newPost = JsonConvert.DeserializeObject<DisqusPost>(responseJson);

                var nestingLevel = 0;
                if (!string.IsNullOrEmpty(model.ReplyTo))
                {
                    var parent = disqusRepository.GetPost(model.ReplyTo);
                    nestingLevel = parent.NestingLevel + 1;
                }
                newPost.NestingLevel = nestingLevel;

                disqusRepository.AddPostCache(newPost);
                await LogCommentActivity(model);

                return Json(new {
                    success = true,
                    action = "create",
                    url = Url.Action("GetPostBody", "Disqus"),
                    id = response.SelectToken("$.response.id").ToString(),
                    parent = response.SelectToken("$.response.parent").ToString()
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Flags a post for review by moderators
        /// </summary>
        /// <param name="post"></param>
        /// <returns>A JSON object indicating success, or the error message in the case of failure</returns>
        [HttpPost]
        public async Task<ActionResult> ReportPost(string id, int reason)
        {
            try
            {
                var response = await disqusService.ReportPost(id, reason);
                var post = disqusRepository.GetPost(id);
                LogReportActivity(post, (ReportReason)reason);

                return Json(new
                {
                    success = true,
                    action = "report",
                    id = id
                });
            }
            catch(DisqusException ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Logs an activity stating the contact has reported a post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public void LogReportActivity(DisqusPost post, ReportReason reason)
        {
            var activityInitializer = new DisqusReportActivityInitializer(post.Message, post.ThreadObject.NodeID, reason);
            activityLogService.Log(activityInitializer);
        }

        /// <summary>
        /// Performs Sentiment Analysis on the <see cref="DisqusPost.Message"/>, then logs an
        /// On-line Marketing activity with the results set to <see cref="ActivityInfo.ActivityValue"/>
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task LogCommentActivity(DisqusEditingFormModel model)
        {
            // Perform Sentiment Analysis
            var sentiment = TextSentiment.Neutral;
            if (SettingsKeyInfoProvider.GetBoolValue("CMSEnableSentimentAnalysis") &&
                !string.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIEndpoint")) &&
                !string.IsNullOrEmpty(SettingsKeyInfoProvider.GetValue("CMSAzureTextAnalyticsAPIKey")))
            {
                try
                {
                    var culture = Thread.CurrentThread.CurrentCulture.Name;
                    DocumentSentiment result = sentimentAnalysisService.AnalyzeText(model.Message, culture, SiteContext.CurrentSiteName);
                    sentiment = result.Sentiment;
                }
                catch (Exception e)
                {
                    eventLogService.LogError(nameof(DisqusController), nameof(LogCommentActivity), e.Message);
                }
            }

            var thread = await disqusRepository.GetThread(model.PostThread);
            var activityInitializer = new DisqusCommentActivityInitializer(model.Message, thread.NodeID, sentiment, model.ThreadRating);
            activityLogService.Log(activityInitializer);
        }

        /// <summary>
        /// Returns the editing form for the specified post
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditPost(string id)
        {
            var post = disqusRepository.GetPost(id);
            var model = new DisqusEditingFormModel()
            {
                EditedPostId = id,
                Message = post.Raw_Message,
                PostThread = post.Thread,
                ReplyTo = id,
                ThreadRating = post.Author.ThreadRating,
                RatingsEnabled = post.ThreadObject.RatingsEnabled && disqusService.CurrentForum.Settings.ThreadRatingsEnabled
            };

            return View("~/Views/Shared/Components/DisqusComponent/_DisqusPostForm.cshtml", model);
        }

        /// <summary>
        /// Upvotes or downvotes a post
        /// </summary>
        /// <param name="isLike"></param>
        /// <param name="id"></param>
        /// <returns>The full HTML of the voted post to update the DOM, or an error message</returns>
        [HttpPost]
        public async Task<ActionResult> VotePost(bool isLike, string id)
        {
            var response = await disqusService.SubmitPostVote(id, isLike ? 1 : -1);
            var code = response.Value<int>("code");
            if (code == 0)
            {
                var voteChange = int.Parse(response.SelectToken("$.response.delta").ToString());
                if (voteChange == 0)
                {
                    var message = $"You cannot {(isLike ? "upvote" : "downvote")} this post.";
                    return new ContentResult() { Content = message, StatusCode = 403 };
                }

                var data = new
                {
                    likes = int.Parse(response.SelectToken("$.response.post.likes").ToString()),
                    dislikes = int.Parse(response.SelectToken("$.response.post.dislikes").ToString())
                };
                disqusRepository.UpdatePostCache(id, "vote", data);

                var post = disqusRepository.GetPost(id);
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusPost.cshtml", post);
            }
            else if (code == (int)DisqusException.DisqusErrorCode.AUTHENTICATION_REQUIRED)
            {
                return new ContentResult() { Content = "You must log in to vote on posts.", StatusCode = 401 };
            }

            throw new DisqusException(code, response.Value<string>("response"));
        }

        /// <summary>
        /// Sets the currently authenticated user to recommend/unrecommend a thread
        /// </summary>
        /// <param name="id">The Disqus internal ID of the thread to recommend/unrecommend</param>
        /// <param name="doRecommend">true if should recommend, false to unrecommend</param>
        [HttpPost]
        public async Task<ActionResult> RecommendThread(string id, bool doRecommend)
        {
            int vote = doRecommend ? 1 : -1;
            var response = await disqusService.SubmitThreadVote(id, vote);
            var code = response.Value<int>("code");
            if (code == 0)
            {
                return Content("");
            }
            else
            {
                throw new DisqusException(code, response.Value<string>("response"));
            }
        }

        /// <summary>
        /// Sets the currently authenticated user to follow/unfollow the specified user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An empty string if successful</returns>
        [HttpPost]
        public async Task<ActionResult> FollowUser(string id, bool doFollow)
        {
            var response = await disqusService.FollowUser(id, doFollow);
            var code = response.Value<int>("code");
            if (code == 0)
            {
                return Content("");
            }
            else
            {
                throw new DisqusException(code, response.Value<string>("response"));
            }
        }

        /// <summary>
        /// Sets the currently authenticated user to subscribe/unsubscribe to a thread
        /// </summary>
        /// <param name="id">The Disqus internal ID of the thread to subscribe/unsubscribe</param>
        /// <param name="doSubscribe">true if should subscribe, false to unsubscribe</param>
        [HttpPost]
        public async Task<ActionResult> SubscribeThread(string id, bool doSubscribe)
        {
            var response = await disqusService.SubscribeToThread(id, doSubscribe);
            var code = response.Value<int>("code");
            if (code == 0)
            {
                return Content("");
            }
            else
            {
                throw new DisqusException(code, response.Value<string>("response"));
            }
        }

        /// <summary>
        /// Closes the specified thread
        /// </summary>
        /// <param name="id">The Disqus internal ID of the thread to close</param>
        [HttpPost]
        public async Task<ActionResult> CloseThread(string id)
        {
            var response = await disqusService.CloseThread(id);
            var code = response.Value<int>("code");
            if (code == 0)
            {
                return Content("");
            }
            else
            {
                throw new DisqusException(code, response.Value<string>("response"));
            }
        }

        /// <summary>
        /// The endpoint called after a user authenticates with Disqus. This action retrieves a token
        /// from Disqus' endpoint and sets the <see cref="IDisqusService.AuthCookie"/>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> Auth()
        {
            var code = QueryHelper.GetString("code", "");
            var url = DisqusConstants.TOKEN_URL;
            var data = disqusService.GetTokenPostData(code);

            using (var client = new HttpClient())
            {
                var tokenResponse = await client.PostAsync(url, data);
                var content = await tokenResponse.Content.ReadAsStringAsync();
                client.Dispose();

                var cookie = JsonConvert.DeserializeObject<DisqusCookie>(content);
                var user = await disqusRepository.GetUser(cookie.User_Id);
                disqusService.AuthCookie = cookie;

                return new RedirectResult("/");
            }
        }

        [HttpGet]
        public ActionResult LogOut()
        {
            CookieHelper.Remove(DisqusConstants.AUTH_COOKIE_DATA);

            return new RedirectResult("/");
        }
    }
}
