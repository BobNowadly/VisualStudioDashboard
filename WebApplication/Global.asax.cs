using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using WebApplication.App_Start;

namespace WebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            UnityConfig.RegisterComponents(new UnityContainer());
        }
    }
}
