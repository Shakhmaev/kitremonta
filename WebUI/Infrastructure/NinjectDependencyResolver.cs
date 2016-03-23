using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using Ninject;
using Moq;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using Store.Domain.Concrete;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ninject.Web.Common;
using MvcSiteMapProvider;
using System.Data.Entity;

namespace Store.WebUI.Infrastructure
{
    public class NinjectDependencyResolver:IDependencyResolver
    {
        private IKernel kernel;

        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        private void AddBindings()
        {
            kernel.Bind<IItemRepository>().To<EFItemRepository>();

            kernel.Bind<EFDbContext>().ToSelf().InRequestScope();
            kernel.Bind<UserManager<AppUser>>().ToSelf().InRequestScope();
            kernel.Bind<IUserStore<AppUser>>()
                .To<UserStore<AppUser>>()
                .WithConstructorArgument("context", context => kernel.Get<EFDbContext>());
            kernel.Bind<IOrderProcessor>().To<OrderProcessor>();
            kernel.Bind<ICartProcessor>().To<CartProcessor>();


            /*kernel.Bind(typeof(UserStore<>)).ToSelf().InRequestScope();
            kernel.Bind(typeof(IUserStore<>)).To(typeof(UserStore<>));

            kernel.Bind<EFDbContext>().ToSelf().InRequestScope();
            kernel.Bind(typeof(UserManager<>)).ToSelf().InRequestScope();*/

        }
    }
}