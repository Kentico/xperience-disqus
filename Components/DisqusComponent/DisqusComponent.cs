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
                NodeID = widgetProperties.Page.NodeID
            };
            var identifier = string.IsNullOrEmpty(widgetProperties.Properties.ThreadIdentifier) ?
                widgetProperties.Page.DocumentGUID.ToString() : widgetProperties.Properties.ThreadIdentifier;

            try
            {
                var threadId = await disqusService.GetThreadIdByIdentifier(identifier, widgetProperties.Page);
                model.Thread = await disqusRepository.GetThread(threadId, false);
                model.Posts = await disqusRepository.GetPostHierarchy(threadId, model.NodeID, false);
                model.Forum = await disqusRepository.GetForum(model.Thread.Forum);

                var header = widgetProperties.Properties.Header;
                header = header.Replace("{num}", model.Thread.Posts.ToString());
                model.Header = header;
            }
            catch (DisqusException e)
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", e);
            }
            catch (Exception e)
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", new DisqusException(500, e.Message));
            }

            return View("~/Views/Shared/Components/DisqusComponent/_DisqusComponent.cshtml", model);
        }
    }
}
