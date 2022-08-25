using CMS.Base;
using CMS.Core;
using CMS.Helpers;

using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Disqus.Widget;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System;

[assembly: CMS.AssemblyDiscoverable]
[assembly: RegisterWidget(DisqusComponent.IDENTIFIER,
    typeof(DisqusComponent),
    "Disqus comments",
    typeof(DisqusComponentProperties),
    Description = "Enables commenting, ratings, and reactions on Xperience pages.",
    IconClass = "icon-bubbles")]

namespace Kentico.Xperience.Disqus.Widget
{
    /// <summary>
    /// Class which constructs the <see cref="DisqusComponentViewModel"/> and renders the widget.
    /// </summary>
    public class DisqusComponent : ViewComponent
    {
        /// <summary>
        /// The internal identifier of the Disqus widget.
        /// </summary>
        public const string IDENTIFIER = "Kentico.Xperience.Disqus.Widget.DisqusComponent";


        private readonly IPageUrlRetriever pageUrlRetriever;
        private readonly IConfiguration configuration;
        private readonly IEventLogService eventLogService;
        private readonly ISiteService siteService;


        /// <summary>
        /// Initializes a new instance of the <see cref="DisqusComponent"/> class.
        /// </summary>
        public DisqusComponent(IPageUrlRetriever pageUrlRetriever,
            IConfiguration configuration,
            IEventLogService eventLogService,
            ISiteService siteService)
        {
            this.pageUrlRetriever = pageUrlRetriever;
            this.eventLogService = eventLogService;
            this.siteService = siteService;
            this.configuration = configuration;
        }


        /// <summary>
        /// Populates the <see cref="DisqusComponentViewModel"/> and returns the appropriate view.
        /// </summary>
        /// <param name="widgetProperties">User populated properties from the page builder or view.</param>
        public IViewComponentResult Invoke(ComponentViewModel<DisqusComponentProperties> widgetProperties)
        {
            if (widgetProperties == null)
            {
                LogWidgetLoadError("Widget properties were not provided.");
                return Content(String.Empty);
            }

            var title = widgetProperties.Properties.Title;
            var identifier = String.IsNullOrEmpty(widgetProperties.Properties.PageIdentifier) ?
                widgetProperties.Page?.DocumentGUID.ToString() : widgetProperties.Properties.PageIdentifier;

            var options = configuration.GetSection(DisqusOptions.SECTION_NAME).Get<DisqusOptions>();
            if (String.IsNullOrEmpty(options?.SiteShortName))
            {
                LogWidgetLoadError($"{nameof(DisqusOptions.SiteShortName)} is null or empty. Please set the siteShortName option under the xperience.disqus section in your appsettings.json file.");
                return Content(String.Empty);
            }

            if (String.IsNullOrEmpty(identifier))
            {
                LogWidgetLoadError("A page identifier used for the Disqus thread must be specified for non-Xperience pages.");
                return Content(String.Empty);
            }

            string pageUrl;
            if (widgetProperties.Page == null)
            {
                pageUrl = HttpContext.Request.GetDisplayUrl();
                pageUrl = URLHelper.RemoveQuery(pageUrl);
            }
            else
            {
                pageUrl = pageUrlRetriever.Retrieve(widgetProperties.Page).AbsoluteUrl;
                if (String.IsNullOrEmpty(title))
                {
                    title = widgetProperties.Page.DocumentName;
                }
            }

            return View(new DisqusComponentViewModel()
            {
                Identifier = identifier,
                Site = options.SiteShortName,
                Url = pageUrl,
                Title = title,
                Node = widgetProperties.Page,
                CssClass = widgetProperties.Properties.CssClass
            });
        }


        private void LogWidgetLoadError(string description)
        {
            eventLogService.LogError(nameof(DisqusComponent),
                    nameof(Invoke),
                    description,
                    siteService.CurrentSite?.SiteID ?? 0,
                    new LoggingPolicy(TimeSpan.FromMinutes(1)));
        }
    }
}