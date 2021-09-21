using Disqus.Models;
using Disqus.Services;
using Kentico.Content.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
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
                model.ThreadID = await disqusService.GetThreadByIdentifier(identifier, widgetProperties.Page);
                model.Posts = await disqusService.GetThreadPosts(model.ThreadID);
            }
            catch(DisqusException e)
            {
                model.Exception = e;
            }

            return View("~/Views/Shared/Components/_DisqusComponent.cshtml", model);
        }
    }
}
