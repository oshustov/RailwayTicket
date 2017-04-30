﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure;
using Microsoft.AspNet.Identity;
using Ninject;
using WebUI.Identity;

namespace WebUI.App_Start
{
    public class NinjectDependencyResolver : IDependencyResolver
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
            kernel.Bind<IUserStore<IdentityUser, Guid>>().To<UserStore>();
            kernel.Bind<IRoleStore<IdentityRole, Guid>>().To<RoleStore>();

            NinjectKernelBinder binder = new NinjectKernelBinder();
            binder.AddBindings(kernel);
        }
    }
}