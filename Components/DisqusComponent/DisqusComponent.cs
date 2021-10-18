using CMS.Helpers;
using CMS.SiteProvider;
using Disqus.Models;
using Disqus.Services;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Disqus.Components.DisqusComponent
{
    public class DisqusComponent : ViewComponent
    {
        public const string IDENTIFIER = "Xperience.DisqusComponent";

        private readonly IDisqusService disqusService;
        private readonly DisqusRepository disqusRepository;
        private readonly IPageUrlRetriever pageUrlRetriever;
        private readonly IHttpContextAccessor httpContextAccessor;

        public DisqusComponent(IDisqusService disqusService,
            DisqusRepository disqusRepository,
            IPageUrlRetriever pageUrlRetriever,
            IHttpContextAccessor httpContextAccessor)
        {
            this.disqusService = disqusService;
            this.disqusRepository = disqusRepository;
            this.pageUrlRetriever = pageUrlRetriever;
            this.httpContextAccessor = httpContextAccessor;
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
            var title = widgetProperties.Page == null ? SiteContext.CurrentSite.DisplayName : widgetProperties.Page.DocumentName;
            var guid = widgetProperties.Page == null ? "" : widgetProperties.Page.DocumentGUID.ToString();
            var identifier = string.IsNullOrEmpty(widgetProperties.Properties.ThreadIdentifier) ?
                guid : widgetProperties.Properties.ThreadIdentifier;

            if(string.IsNullOrEmpty(identifier))
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", new DisqusExceptionViewModel() {
                    Header = model.Header,
                    Exception = new DisqusException(500, "An indentifier must be specified for non-Xperience pages.")
                });
            }

            string pageUrl;
            if(widgetProperties.Page == null)
            {
                // Check for local requests, do not create URL if local
                var context = httpContextAccessor.HttpContext;
                if (context.Connection.RemoteIpAddress.Equals(httpContextAccessor.HttpContext.Connection.LocalIpAddress) || IPAddress.IsLoopback(context.Connection.RemoteIpAddress))
                {
                    pageUrl = "";
                }
                else
                {
                    pageUrl = context.Request.GetDisplayUrl();
                    pageUrl = URLHelper.RemoveQuery(pageUrl);
                }
            }
            else
            {
                pageUrl = pageUrlRetriever.Retrieve(widgetProperties.Page).AbsoluteUrl;
            }

            try
            {
                var threadId = await disqusService.GetThreadIdByIdentifier(identifier, title, pageUrl);
                var thread = await disqusRepository.GetThread(threadId, false);
                thread.NodeID = widgetProperties.Page == null ? 0 : widgetProperties.Page.NodeID;

                model.Thread = thread;
                model.Posts = await disqusRepository.GetTopLevelPosts(threadId);
                model.Forum = await disqusRepository.GetForum(model.Thread.Forum, false);
            }
            catch (DisqusException e)
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", new DisqusExceptionViewModel() {
                    Header = model.Header,
                    Exception = e
                });
            }
            catch (Exception e)
            {
                return View("~/Views/Shared/Components/DisqusComponent/_DisqusException.cshtml", new DisqusExceptionViewModel() {
                    Header = model.Header,
                    Exception = new DisqusException(500, e.Message)
                });
            }

            return View("~/Views/Shared/Components/DisqusComponent/_DisqusComponent.cshtml", model);
        }
    }
}
