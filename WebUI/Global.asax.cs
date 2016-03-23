using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Store.WebUI.Infrastructure.Binders;
using Store.Domain.Entities;
using Store.WebUI.App_Start;
using Store.Domain.Abstract;
using Ninject;
using Store.WebUI.Models;

namespace Store.WebUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ModelBinders.Binders.Add(typeof(CartIdWrapper), new CartIdBinder());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        }

        /*private static double USDCourse = 0;

        public static double GetCourse()
        {
            var myConfiguration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            myConfiguration.AppSettings["123"] = "123";
            myConfiguration.Save();
        }*/
    }


}
