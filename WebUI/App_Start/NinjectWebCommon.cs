[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Store.WebUI.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Store.WebUI.App_Start.NinjectWebCommon), "Stop")]

namespace Store.WebUI.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using DI;
    using Store.WebUI.DI.Ninject.Modules;
    using MvcSiteMapProvider.Loader;
    using MvcSiteMapProvider.Xml;
    using System.Web.Hosting;
    using MvcSiteMapProvider.Web.Mvc;
    using System.Web.Routing;
    using Store.Domain.Abstract;
    using Store.Domain.Concrete;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);

                kernel.Load(new MvcSiteMapProviderModule());

                MvcSiteMapProvider.SiteMaps.Loader = kernel.Get<ISiteMapLoader>();

                var validator = kernel.Get<ISiteMapXmlValidator>();
                validator.ValidateXml(HostingEnvironment.MapPath("~/Mvc.sitemap"));

                XmlSiteMapController.RegisterRoutes(RouteTable.Routes);

                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            System.Web.Mvc.DependencyResolver.SetResolver(new Store.WebUI.Infrastructure.NinjectDependencyResolver(kernel));
        }        
    }
}
