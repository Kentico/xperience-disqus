using CMS.Helpers;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Disqus.Components;
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
namespace Kentico.Xperience.Disqus.Components
{
    /// <summary>
    /// Class which constructs the <see cref="DisqusComponentViewModel"/> and renders the widget.
    /// </summary>
    public class DisqusComponent : ViewComponent
    {
        public const string IDENTIFIER = "Xperience.DisqusComponent";
        private readonly IPageUrlRetriever pageUrlRetriever;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        public DisqusComponent(IPageUrlRetriever pageUrlRetriever,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration config)
        {
            this.pageUrlRetriever = pageUrlRetriever;
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = config;
        }

        /// <summary>
        /// Populates the <see cref="DisqusComponentViewModel"/> and returns the appropriate view.
        /// </summary>
        /// <param name="widgetProperties">User populated properties from the page builder or view.</param>
        /// <exception cref="ArgumentNullException">Thrown if the widget properties aren't provided, the
        /// "DisqusShortName" application setting isn't set, or the widget was placed on a non-Xperience page and
        /// <see cref="DisqusComponentProperties.PageIdentifier"/> isn't set.</exception>
        public IViewComponentResult Invoke(ComponentViewModel<DisqusComponentProperties> widgetProperties)
        {
            if (widgetProperties is null)
            {
                throw new ArgumentNullException(nameof(widgetProperties));
            }

            string pageUrl;
            var title = widgetProperties.Properties.Title;
            var site = configuration.GetValue<string>("DisqusShortName");
            var guid = widgetProperties.Page == null ? "" : widgetProperties.Page.DocumentGUID.ToString();
            var identifier = String.IsNullOrEmpty(widgetProperties.Properties.PageIdentifier) ?
                guid : widgetProperties.Properties.PageIdentifier;
            
            if (String.IsNullOrEmpty(site))
            {
                throw new ArgumentNullException("DisqusShortName", "Please set the DisqusShortName setting in your appsettings.json.");
            }

            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(DisqusComponentProperties.PageIdentifier), "An indentifier must be specified for non-Xperience pages.");
            }

            if (widgetProperties.Page == null)
            {
                pageUrl = httpContextAccessor.HttpContext.Request.GetDisplayUrl();
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
                Site = site,
                Url = pageUrl,
                Title = title,
                Node = widgetProperties.Page,
                CssClass = widgetProperties.Properties.CssClass
            });
        }
    }
}