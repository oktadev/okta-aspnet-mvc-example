using System.Web.Mvc;
using System.Web.Routing;

namespace Okta.Samples.OAuth.CodeFlow
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //routes.MapRoute( // this route must be declared first, before the one below it
            //     "StartBrowse",
            //     "Callbacks/Start/Here",
            //     new
            //     {
            //         controller = "Gallery",
            //         action = "StartBrowse",
            //     });

            //routes.MapRoute(
            //     "ActualBrowse",
            //     "Gallery/Browse/{searchterm}",
            //     new
            //     {
            //         controller = "Gallery",
            //         action = "Browse",
            //         searchterm = UrlParameter.Optional
            //     });
        }
    }
}
