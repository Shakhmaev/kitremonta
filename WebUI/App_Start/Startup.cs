using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Ninject;
using Owin;

[assembly: OwinStartupAttribute(typeof(Store.WebUI.Startup))]
namespace Store.WebUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Auth/Login")
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.MapSignalR();

            GlobalConfiguration.Configuration.UseSqlServerStorage("EFDbContext");
            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}