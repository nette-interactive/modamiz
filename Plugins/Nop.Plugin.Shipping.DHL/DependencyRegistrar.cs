using System;
using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.DHL.Data;
using Nop.Plugin.Shipping.DHL.Domain;
using Nop.Plugin.Shipping.DHL.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.DHL
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<DHLService>().As<IDHLService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<DHLObjectContext>(builder, "nop_object_context_dhl_zip");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<DHLRecord>>()
                .As<IRepository<DHLRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_dhl_zip"))
                .InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
