
namespace SimpleOwinAspNetHost
{
    using SimpleOwinAspNetHost.Middlewares;
    using SimpleOwinAspNetHost.Samples;
    using System;
    using System.Web.Routing;
    using SimpleOwinAspNetHost.Samples.WebSockets.Helloworld;

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var aspnetWsMiddleware = new AspNetWebSocketMiddleware();

            RouteTable.Routes.Add(new Route("helloworld", new SimpleOwinAspNetRouteHandler(Helloworld.OwinApp())));

            //RouteTable.Routes.Add(new Route("middlewareapps", new SimpleOwinAspNetRouteHandler(MiddlewareApps.OwinApp())));
            // SimpleOwinAspNetRouteHandler is capable of auto handling IEnumerable<Func<AppFunc,AppFunc>>
            RouteTable.Routes.Add(new Route("middlewareapps", new SimpleOwinAspNetRouteHandler(MiddlewareApps.OwinApps())));
            
            RouteTable.Routes.Add(new Route("websocket/helloworld", new SimpleOwinAspNetRouteHandler(HelloWorldWebSocket.OwinApp())));
        }
    }
}