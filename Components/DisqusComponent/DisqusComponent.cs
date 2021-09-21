using Disqus.Models;
using Disqus.Services;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Disqus.Components.DisqusComponent
{
    public class DisqusComponent : ViewComponent
    {
        public const string IDENTIFIER = "Xperience.DisqusComponent";
        private readonly IDisqusService disqusService;

        public DisqusComponent(IDisqusService disqusService)
        {
            this.disqusService = disqusService;
        }

        public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<DisqusComponentProperties> widgetProperties)
        {
            if (widgetProperties is null)
            {
                throw new ArgumentNullException(nameof(widgetProperties));
            }

            var identifier = string.IsNullOrEmpty(widgetProperties.Properties.ThreadIdentifier) ?
                widgetProperties.Page.DocumentGUID.ToString() : widgetProperties.Properties.ThreadIdentifier;
            var threadId = await disqusService.GetThreadByIdentifier(identifier, widgetProperties.Page);
            var posts = await disqusService.GetThreadPosts(threadId);

            return View("~/Views/Shared/Components/_DisqusComponent.cshtml", new DisqusComponentViewModel() {
                Posts = posts,
                ThreadID = threadId,
                Header = widgetProperties.Properties.Header
            });
        }
    }
}
