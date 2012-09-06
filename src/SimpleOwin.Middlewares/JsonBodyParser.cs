namespace SimpleOwin.Middlewares
{
    using SimpleOwin.Extensions;
    using System;
    using System.IO;
    using System.Linq;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class JsonBodyParser
    {
        public static Func<AppFunc, AppFunc> Middleware(Func<string, object> jsonDeserializer = null)
        {
            return
                next =>
                env =>
                {
                    var contentType = env.GetOwinRequestHeaderValues("content-type");
                    if (contentType == null)
                        return next(env);

                    if (contentType.Any(t => t.Equals("application/json", StringComparison.OrdinalIgnoreCase)))
                    {
                        env["simpleOwin.body"] = new Lazy<object>(
                            () =>
                            {
                                var json = ParseJson(env.GetOwinRequestBody(), jsonDeserializer);
                                return json;
                            });
                    }

                    return next(env);
                };
        }

        private static object ParseJson(Stream stream, Func<string, object> jsonDeserializer)
        {
            if (jsonDeserializer == null)
                jsonDeserializer = SimpleJson.DeserializeObject;

            using (var reader = new StreamReader(stream))
            {
                var jsonString = reader.ReadToEnd();
                return jsonDeserializer(jsonString);
            }
        }
    }
}