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

            app.Add(QueryParser.Middleware());
            app.Add(JsonBodyParser.Middleware());
            app.Add(MethodOverride.Middleware());

            var router = new Router();
            app.Add(router.Middleware());

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