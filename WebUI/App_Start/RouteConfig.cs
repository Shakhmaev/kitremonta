using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Store.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            /*routes.MapRoute(
                name: null,
                url: "Page_{page}",
                defaults: new { controller = "Item", action = "List", category = (string)null },
                constraints: new { page = @"\d+"}
            );*/

            routes.MapRoute(null,
                "account/{id}",
                new { controller = "Account", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(null,
                "Search/{search}",
                new { controller = "Item", action = "Search", search = UrlParameter.Optional }
            );

            routes.MapRoute(null,
                "catalogue/Item/{id}",
                new { controller = "Item", action = "ItemDetails", id = UrlParameter.Optional}
            );


            routes.MapRoute("cat",
                "catalogue/{category}",
                new { controller = "Item", action = "List", category = UrlParameter.Optional }
            );

            /*routes.MapRoute(
                name: null,
                url: "{category}/Page_{page}",
                defaults: new { controller = "Item", action = "List"},
                constraints: new { page = @"\d+" }
            );
            */


           routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Item", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
