
using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.SessionState;
using System.Web.Routing;
using SimpleOwin.Hosts.AspNet;

namespace SimpleOwin.Samples.AspNetMono
{
	public class Global : System.Web.HttpApplication
	{
		protected virtual void Application_Start (Object sender, EventArgs e)
		{
			RouteTable.Routes.Add(new Route("hello", new SimpleOwinAspNetRouteHandler(HelloWorldOwinApp.OwinApp())));
		}
	}
}

