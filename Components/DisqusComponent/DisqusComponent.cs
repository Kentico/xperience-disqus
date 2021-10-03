using Disqus.Models;
using Disqus.Services;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
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

            var model = new DisqusComponentViewModel()
            {
                Header = widgetProperties.Properties.Header
            };
            var identifier = string.IsNullOrEmpty(widgetProperties.Properties.ThreadIdentifier) ?
                widgetProperties.Page.DocumentGUID.ToString() : widgetProperties.Properties.ThreadIdentifier;
            
            try
            {
                var threadId = await disqusService.GetThreadIdByIdentifier(identifier, widgetProperties.Page);
                model.Thread = await disqusService.GetThread(threadId);
                model.Posts = await disqusService.GetThreadPosts(threadId);
            }
            catch(DisqusException e)
            {
                model.Exception = e;
            }

            return View("~/Views/Shared/Components/DisqusComponent/_DisqusComponent.cshtml", model);
        }
    }
}
