using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ArityApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "invoice",
                url: "invoice",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "particular",
                url: "particular",
                defaults: new { controller = "Particular", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "notfound",
                url: "404",
                defaults: new { controller = "Home", action = "NotFound", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "user",
                url: "user",
                defaults: new { controller = "Home", action = "Users", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
