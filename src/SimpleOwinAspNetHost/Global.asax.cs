using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace SimpleOwinAspNetHost
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new Route("{*pathInfo}", new SimpleOwinAspNetRouteHandler(new Helloworld().OwinApp)));
        }
    }
}