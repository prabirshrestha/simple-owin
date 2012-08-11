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
            RouteTable.Routes.Add(new Route("helloworld", new SimpleOwinAspNetRouteHandler(new Helloworld().OwinApp)));
            RouteTable.Routes.Add(new Route("websocket/helloworld", new SimpleOwinAspNetRouteHandler(new WebSocketHelloworld().OwinApp)));
            RouteTable.Routes.Add(new Route("websocket/echo", new SimpleOwinAspNetRouteHandler(new WebSocketEchoServer().OwinApp)));
        }
    }
}