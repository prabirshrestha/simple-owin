namespace SimpleOwin.Middlewares
{
    using System;
    using System.Text;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class NotFound
    {
        private const string DefaultNotFoundMessage = "<h1>:( Not Found</h1>";

        public static Func<AppFunc, AppFunc> Middleware(string text = DefaultNotFoundMessage, string contentType = "text/html")
        {
            var data = Encoding.UTF8.GetBytes(text);

            return
                next =>
                env =>
                {
                    env
                        .SetOwinResponseStatusCode(404)
                        .GetOwinResponseHeaders(headers =>
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(contentType))
                                                            headers.SetOwinHeader("Content-Type", contentType);
                                                    });

                    env
                        .GetOwinResponseBody()
                        .Write(data, 0, data.Length);

                    return next(env);
                };
        }
    }
}