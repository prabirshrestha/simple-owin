namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using SimpleOwin.Extensions;
    using SimpleOwin.Middlewares;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class SimpleOwinApp
    {
        public static AppFunc OwinApp()
        {
            var app = new List<Func<AppFunc, AppFunc>>();

            app.Add(QueryParser.Middleware());
            app.Add(MethodOverride.Middleware());
            app.Add(NotFound.Middleware());

            return app.ToOwinAppFunc();
        }
    }
}