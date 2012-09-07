namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using SimpleOwin.Extensions;
    using SimpleOwin.Middlewares;
    using SimpleOwin.Middlewares.Router;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class SimpleOwinApp
    {
        public static AppFunc OwinApp()
        {
            var app = new List<Func<AppFunc, AppFunc>>();

            app
                .Use(H5bp.IeEdgeChromeFrameHeader())
                .Use(H5bp.RemovePoweredBy())    
                .Use(H5bp.CrossDomainRules());

            // Instead of Use extension method you can use Add.
            // Add is a available in .NET via ICollection<T> but it does not allow chaining.

            app.Add(QueryParser.Middleware());

            // and you can mix both Use and Add
            app
                .Use(JsonBodyParser.Middleware())
                .Use(MethodOverride.Middleware());

            IRouter router = new RegexRouter(app); // this will auto call app.Add(router.Middleware());
            // you can manually add it later on if you pass nothing in Router constructor are pass it as null

            router.Get("hello", next =>
                            async env =>
                            {
                                var msg = Encoding.UTF8.GetBytes("hello");
                                var responseBody = env.GetOwinResponseBody();

                                await responseBody.WriteAsync(msg, 0, msg.Length);
                            });

            router.Get("*", next =>
                            async env =>
                            {
                                var msg = Encoding.UTF8.GetBytes("hi from get *");
                                var responseBody = env.GetOwinResponseBody();

                                await responseBody.WriteAsync(msg, 0, msg.Length);
                            });

            router.All("*", NotFound.Middleware());

            return app.ToOwinAppFunc();
        }
    }
}