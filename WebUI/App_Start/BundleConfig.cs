using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Store.WebUI.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Scripts/jquery").Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery-ui-{version}.js",
                    "~/Scripts/jquery.unobtrusive-ajax.js",
                    "~/Scripts/jquery.elevateZoom-{version}.min.js",
                    "~/Scripts/jquery.signalR-{version}.js"
                ));

            bundles.Add(new ScriptBundle("~/Scripts/custom").Include(
                    "~/Scripts/bootstrap.min.js",
                    "~/Scripts/common.js",
                    "~/Scripts/megamenu.js",
                    "~/Scripts/lity.min.js"
                ));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                    "~/Content/themes/base/base.css",
                    "~/Content/themes/base/theme.css"
                ));

            bundles.Add(new StyleBundle("~/Content/Styles/custom").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-theme.css",
                    "~/Content/font-awesome.min.css",
                    "~/Content/Styles/ionicons.min.css",
                    "~/Content/Styles/style.css",
                    "~/Content/Styles/lity.min.css",
                    "~/Content/Styles/Styles1.css",
                    "~/Content/Styles/ErrorStyles.css",
                    "~/Content/Styles/jCarouselStyles.css"
                ));

            bundles.Add(new ScriptBundle("~/Content/slick/slickJs").Include(
                    "~/Content/slick/slick.min.js"
                ));
            bundles.Add(new StyleBundle("~/Content/slick/slickStyle").Include(
                    "~/Content/slick/slick.css",
                    "~/Content/slick/slick-theme.css"
                ));

            bundles.Add(new StyleBundle("~/Scripts/jquery.validate").Include(
                    "~/Scripts/jquery.validate.min.js",
                    "~/Scripts/jquery.validate.unobtrusive.min.js"
                ));

            bundles.UseCdn = true;
            bundles.Add(new StyleBundle("~/Content/Styles/font", 
                "https://fonts.googleapis.com/css?family=Play:400,700&subset=latin,cyrillic"));
        }
    }
}