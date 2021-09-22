using CMS.Helpers;
using Disqus.Models;
using Disqus.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Disqus.Components.DisqusComponent
{
    public class DisqusController : Controller
    {
        private readonly IDisqusService disqusService;

        public DisqusController(IDisqusService disqusService)
        {
            this.disqusService = disqusService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitPost(DisqusPost post)
        {
            var response = await disqusService.CreatePost(post);
            if(response.Value<int>("code") == 0)
            {
                return Content("Your comment has been posted.");
            }
            else
            {
                var ex = new DisqusException(response.Value<int>("code"), response.Value<string>("response"));
                return PartialView("_DisqusException.cshtml", ex);
            }
        }

        public async Task<ActionResult> VotePost(bool isLike, string id)
        {
            //TODO: Check if user has already voted (repeat voting doesn't work, but still should prevent it)
            var response = await disqusService.SubmitVote(id, isLike ? 1 : -1);
            if(response.Value<int>("code") == 0)
            {
                return View("~/Views/Shared/Components/_DisqusPostFooter.cshtml", new DisqusPostFooterModel()
                {
                    PostId = response.SelectToken("$.response.post.id").ToString(),
                    Likes = int.Parse(response.SelectToken("$.response.post.likes").ToString()),
                    Dislikes = int.Parse(response.SelectToken("$.response.post.dislikes").ToString())
                });
            }

            throw new DisqusException(response.Value<int>("code"), response.Value<string>("response"));
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
                    UserID = json.Value<int>("user_id")
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
