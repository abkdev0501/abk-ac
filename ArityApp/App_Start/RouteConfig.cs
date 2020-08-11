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
              name: "dashboard",
              url: "dashboard",
              defaults: new { controller = "Home", action = "Dashboard", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "receipt",
              url: "receipt",
              defaults: new { controller = "Home", action = "Payment", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "task",
              url: "task",
              defaults: new { controller = "Task", action = "Index", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "company",
              url: "company",
              defaults: new { controller = "Master", action = "Company", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "commodity",
              url: "commodity",
              defaults: new { controller = "Master", action = "CommodityMaster", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "consultant",
              url: "consultant",
              defaults: new { controller = "Master", action = "Consultants", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "group",
              url: "group",
              defaults: new { controller = "Master", action = "GroupMaster", id = UrlParameter.Optional }
          );

            routes.MapRoute(
              name: "client",
              url: "client",
              defaults: new { controller = "Master", action = "ClientMaster", id = UrlParameter.Optional }
          );

            routes.MapRoute(
             name: "login",
             url: "login",
             defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
         );

            routes.MapRoute(
             name: "notification",
             url: "notification",
             defaults: new { controller = "Master", action = "NotificationMaster", id = UrlParameter.Optional }
         );
            routes.MapRoute(
             name: "notes",
             url: "notes",
             defaults: new { controller = "Master", action = "NotesMaster", id = UrlParameter.Optional }
         );

            routes.MapRoute(
             name: "documenttype",
             url: "documenttype",
             defaults: new { controller = "Master", action = "DocumentType", id = UrlParameter.Optional }
         );

            routes.MapRoute(
             name: "businessstatus",
             url: "businessstatus",
             defaults: new { controller = "Master", action = "BusinessStatus", id = UrlParameter.Optional }
         );

            routes.MapRoute(
             name: "businesstype",
             url: "businesstype",
             defaults: new { controller = "Master", action = "BusinesStype", id = UrlParameter.Optional }
         );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Landing", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
