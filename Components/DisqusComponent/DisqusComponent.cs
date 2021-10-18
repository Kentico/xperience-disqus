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
        private readonly DisqusRepository disqusRepository;

        public DisqusComponent(IDisqusService disqusService, DisqusRepository disqusRepository)
        {
            this.disqusService = disqusService;
            this.disqusRepository = disqusRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(ComponentViewModel<DisqusComponentProperties> widgetProperties)
        {
            if (widgetProperties is null)
            {
                throw new ArgumentNullException(nameof(widgetProperties));
            }

            var model = new DisqusComponentViewModel() {
                Header = widgetProperties.Properties.Header
            };
            var identifier = string.IsNullOrEmpty(widgetProperties.Properties.ThreadIdentifier) ?
                widgetProperties.Page.DocumentGUID.ToString() : widgetProperties.Properties.ThreadIdentifier;

            try
            {
                var threadId = await disqusService.GetThreadIdByIdentifier(identifier, widgetProperties.Page);
                var thread = await disqusRepository.GetThread(threadId, false);
                thread.NodeID = widgetProperties.Page.NodeID;

                model.Thread = thread;
                model.Posts = await disqusRepository.GetTopLevelPosts(threadId);
                model.Forum = await disqusRepository.GetForum(model.Thread.Forum);
            }
            catch (DisqusException e)
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", new DisqusExceptionViewModel() { Header = model.Header, Exception = e });
            }
            catch (Exception e)
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", new DisqusExceptionViewModel() { Header = model.Header, Exception = new DisqusException(500, e.Message) });
            }

            return View("~/Views/Shared/Components/DisqusComponent/_DisqusComponent.cshtml", model);
        }
    }
}
