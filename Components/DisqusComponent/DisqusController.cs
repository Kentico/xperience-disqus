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
        public ActionResult SubmitPost(DisqusComponentViewModel model)
        {
            /*if (string.IsNullOrEmpty(model.Comment))
            {
                ModelState.AddModelError("Comment", "Please enter your message");

                return PartialView("~/Views/Shared/Components/_DisqusComponent.cshtml", model);
            }*/

            return Content("Done");
        }

        public async Task<ActionResult> Auth()
        {
            var code = QueryHelper.GetString("code", "");
            var url = "https://disqus.com/api/oauth/2.0/access_token/";
            var data = disqusService.GetTokenPostData(code);

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, data);
                var content = await response.Content.ReadAsStringAsync();
                client.Dispose();

                var json = JObject.Parse(content);
                disqusService.UserName = json.Value<string>("username");
                disqusService.UserToken = json.Value<string>("access_token");

                return new RedirectResult("/");
            }
        }
    }
}
