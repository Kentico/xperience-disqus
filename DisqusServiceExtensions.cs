using Microsoft.Extensions.DependencyInjection;
using CMS;
using Kentico.Xperience.Disqus.Components.DisqusComponent;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Disqus.Services;

[assembly: AssemblyDiscoverable]
[assembly: RegisterWidget(DisqusComponent.IDENTIFIER,
    typeof(DisqusComponent),
    "Disqus comments",
    typeof(DisqusComponentProperties),
    Description = "",
    IconClass = "icon-bubbles")]

namespace Kentico.Xperience.Disqus
{
    public static class DiscusServiceExtensions
    {
        public static void AddDisqus(this IServiceCollection services)
        {
            services.AddSingleton<IDisqusService, DisqusService>();
            services.AddSingleton<DisqusRepository>();
        }
    }
}
