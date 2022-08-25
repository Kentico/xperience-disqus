using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Kentico.Xperience.Disqus.Widget.KX13
{
    /// <summary>
    /// Application startup extension methods.
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Maps Disqus activity tracking route into the system.
        /// </summary>
        public static void MapDisqusActivityTracking(this IEndpointRouteBuilder endpoints)
        {
            DisqusHelper.CommentActivityTrackingEnabled = true;

            endpoints.MapControllerRoute(
                name: "Kentico.Xperience.Disqus",
                pattern: "Kentico.Xperience.Disqus/LogCommentActivity",
                defaults: new
                {
                    controller = "KenticoDisqusLog",
                    action = nameof(KenticoDisqusLogController.LogCommentActivity)
                });
        }
    }
}
