using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SnWebApplication
{
    public class MvcApplication : SenseNet.Services.SenseNetGlobal
    {
        protected override void Application_Start(object sender, EventArgs e, HttpApplication application)
        {
            base.Application_Start(sender, e, application);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
