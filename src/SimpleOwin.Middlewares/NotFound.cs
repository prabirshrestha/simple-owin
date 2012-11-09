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
                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        var headers = env
                            .SetResponseStatusCode(404)
                            .GetResponseHeaders();
                        headers.SetHeader("content-type", contentType);
                    }

                    env
                        .GetResponseBody()
                        .Write(data, 0, data.Length);

                    return next(env);
                };
        }
    }
}