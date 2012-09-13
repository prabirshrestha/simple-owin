namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using SimpleOwin.Extensions;
    using SimpleOwin.Extensions.Stream;
    using SimpleOwin.Middlewares;
    using SimpleOwin.Middlewares.Router;
    using SimpleOwin.Middlewares.Router.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class SimpleOwinApp
    {
        public static AppFunc OwinApp()
        {
            var app =
                new List<Func<AppFunc, AppFunc>>()
                    .Use(H5bp.IeEdgeChromeFrameHeader())
                    .Use(H5bp.RemovePoweredBy())
                    .Use(H5bp.CrossDomainRules())
                    .Use(JsonBodyParser.Middleware())
                    .Use(UrlEncoded.Middleware())
                    .Use(MethodOverride.Middleware());

            var router = new RegexRouter(app);

            router.Get("/", next =>
                           async env =>
                           {
                               await env.GetOwinResponseBody()
                                   .WriteStringAsync("hi");
                           });

            router.Get(@"/hi/(?<name>((.)*))$", next =>
                            async env =>
                            {
                                var routeParameters = env.GetSimpleOwinRouteParameters();

                                await env.GetOwinResponseBody()
                                    .WriteStringAsync("Hi " + routeParameters["name"]);
                            });

            router.Get("/hello", next =>
                                async env =>
                                {
                                    await env.GetOwinResponseBody()
                                        .WriteStringAsync("hello");
                                });

            router.All("*", NotFound.Middleware());

            return app.ToOwinApp();
        }
    }
}