namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using SimpleOwin.Extensions;
    using SimpleOwin.Extensions.Stream;
    using SimpleOwin.Middlewares;
    using SimpleOwin.Middlewares.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class SimpleOwinApp
    {
        public static AppFunc OwinApp(IDictionary<string, object> startupEnv = null)
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
            var template = new RazorEngine.Templating.TemplateService();

            router.Get("/", next =>
                           async env =>
                           {
                               await env.GetResponseBody()
                                   .WriteStringAsync("hi");
                           });

            template.Compile("Hi @Model.name", typeof(DynamicObject), "/hi");
            router.Get(@"/hi/(?<name>((.)*))$", next =>
                            async env =>
                            {
                                var routeParameters = env.GetSimpleOwinRouteParameters();

                                string html = template.Run("/hi", new { name = routeParameters["name"] }.ToDynamicObject());

                                await env.GetResponseBody()
                                    .WriteStringAsync(html);
                            });

            router.Get("/hello", next =>
                                async env =>
                                {
                                    await env.GetResponseBody()
                                        .WriteStringAsync("Hello");
                                });

            router.All("*", NotFound.Middleware());

            return app.ToOwinApp();
        }


    }
}