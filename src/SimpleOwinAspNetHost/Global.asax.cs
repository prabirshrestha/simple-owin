﻿
namespace SimpleOwinAspNetHost
{
    using SimpleOwinAspNetHost.Samples;
    using System;
    using System.Web.Routing;
    using SimpleOwinAspNetHost.Samples.WebSockets.Helloworld;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.Add(new Route("helloworld", new SimpleOwinAspNetRouteHandler(Helloworld.OwinApp())));

            RouteTable.Routes.Add(new Route("websocket/helloworld", new SimpleOwinAspNetRouteHandler(HelloWorldWebSocket.OwinApp())));
        }
    }
}