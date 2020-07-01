using Arity.Data;
using Arity.Service;
using Arity.Service.Contract;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ArityApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<AccountService>().As<IAccountService>();
            builder.RegisterType<DocumentService>().As<IDocumentService>();
            builder.RegisterType<InvoiceService>().As<IInvoiceService>();
            builder.RegisterType<MasterService>().As<IMasterService>();
            builder.RegisterType<ParticularServices>().As<IParticularServices>();
            builder.RegisterType<PaymentService>().As<IPaymentService>();
            builder.RegisterType<LoggerService>().As<ILoggerService>();
            builder.RegisterType<TaskService>().As<ITaskService>();
            builder.RegisterType<RMNEntities>().InstancePerLifetimeScope();


            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver =
                     new AutofacWebApiDependencyResolver(container);
        }
    }
}
