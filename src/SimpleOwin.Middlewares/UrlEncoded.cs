namespace SimpleOwin.Middlewares
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class UrlEncoded
    {
        private const string UrlFormEncodedContentType = "application/x-www-form-urlencoded";

        public static Func<AppFunc, AppFunc> Middleware(Func<string, string> urlDecoder = null)
        {
            return
                next =>
                env =>
                {
                    if (env.ContainsKey("simpleOwin.body"))
                        return next(env);

                    var contentType = env
                        .GetRequestHeaders()
                        .GetHeaderValues("content-type");

                    if (contentType == null)
                        return next(env);

                    if (contentType.Any(t => t == UrlFormEncodedContentType))
                    {
                        env["simpleOwin.body"] = new Lazy<IDictionary<string, string[]>>(
                            () =>
                            {
                                return ParseUrlFormEncodedBody(env.GetRequestBody(), urlDecoder);
                            });
                    }

                    return next(env);
                };
        }

        private static IDictionary<string, string[]> ParseUrlFormEncodedBody(Stream stream, Func<string, string> urlDecoder = null)
        {
            using (var reader = new StreamReader(stream))
            {
                var formBody = reader.ReadToEnd();
                return QueryParser.ParseQuerystring(formBody, urlDecoder);
            }
        }
    }
}