namespace SimpleOwin.Middlewares
{
    using System;
    using System.Text;
    using SimpleOwin.Middlewares.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class NotFound
    {
        public static Func<AppFunc, AppFunc> Middleware()
        {
            const string responseBody = "<h1>:( Not Found</h1>";
            var data = Encoding.UTF8.GetBytes(responseBody);

            return
                next =>
                async env =>
                {
                    env.SetOwinResponseStatusCode(404);

                    var resBody = env.GetOwinResponseBody();

                    await resBody.WriteAsync(data, 0, data.Length);

                    await next(env);
                };
        }
    }
}