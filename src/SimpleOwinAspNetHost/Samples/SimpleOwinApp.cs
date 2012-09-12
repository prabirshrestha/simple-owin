namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using SimpleOwin.Extensions;
    using SimpleOwin.Extensions.Stream;
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
                .Use(UrlEncoded.Middleware())
                .Use(MethodOverride.Middleware());

            IRouter router = new RegexRouter(app); // this will auto call app.Add(router.Middleware());
            // you can manually add it later on if you pass nothing in Router constructor are pass it as null

            router.Get("hello", next =>
                            async env =>
                            {
                                await env.GetOwinResponseBody()
                                    .WriteStringAsync("hello");
                            });

            router.Get("*", next =>
                            async env =>
                            {
                                await env.GetOwinResponseBody()
                                    .WriteStringAsync("hi from get *");
                            });

            router.All("*", NotFound.Middleware());

            return app.ToOwinApp();
        }
    }
}