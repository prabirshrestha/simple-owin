namespace SimpleOwin.Middlewares
{
    using System;
    using System.Text;
    using SimpleOwin.Middlewares.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class NotFound
    {
        private const string DefaultNotFoundMessage = "<h1>:( Not Found</h1>";

        public static Func<AppFunc, AppFunc> Middleware(string text = DefaultNotFoundMessage, string contentType = "text/html")
        {
            var data = Encoding.UTF8.GetBytes(text);

            return
                next =>
                async env =>
                {
                    env
                        .SetOwinResponseStatusCode(404)
                        .GetOwinResponseHeaders(headers =>
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(contentType))
                                                            headers.SetOwinHeader("Content-Type", contentType);
                                                    });

                    await env
                        .GetOwinResponseBody()
                        .WriteAsync(data, 0, data.Length);

                    await next(env);
                };
        }
    }
}