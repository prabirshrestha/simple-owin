namespace SimpleOwin.Middlewares.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public static class SimpleOwinEnvironmentExtensions
    {
        public static IDictionary<string, string[]> GetOwinResponseHeaders(this IDictionary<string, object> env)
        {
            return env.GetSimpleOwinValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static IDictionary<string, object> GetOwinResponseHeaders(this IDictionary<string, object> env, Action<IDictionary<string, string[]>> callback)
        {
            var headers = env.GetSimpleOwinValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
            if (callback != null)
                callback(headers);
            return env;
        }

        public static async Task<IDictionary<string, object>> GetOwinResponseHeaders(this IDictionary<string, object> env, Func<IDictionary<string, string[]>, Task> callback)
        {
            var headers = env.GetSimpleOwinValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
            if (callback != null)
                await callback(headers);
            return env;
        }

        public static Stream GetOwinResponseBody(this IDictionary<string, object> env)
        {
            return env.GetSimpleOwinValue<Stream>("owin.ResponseBody");
        }

        public static async Task<Stream> WriteOwinStringAsync(this Stream stream, string str, Encoding encoding)
        {
            var data = encoding.GetBytes(str);
            await stream.WriteAsync(data, 0, data.Length);
            return stream;
        }

        public static async Task<Stream> WriteOwinStringAsync(this Stream stream, string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            await stream.WriteAsync(data, 0, data.Length);
            return stream;
        }

        public static T GetSimpleOwinValue<T>(this IDictionary<string, object> env, string key, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
        }

        public static IDictionary<string, object> SetOwinResponseStatusCode(this IDictionary<string, object> env, int statusCode)
        {
            env["owin.ResponseStatusCode"] = statusCode;
            return env;
        }

        public static IDictionary<string, string[]> SetOwinHeader(this IDictionary<string, string[]> headers, string key, string[] value)
        {
            headers[key] = value;
            return headers;
        }

        public static IDictionary<string, string[]> SetOwinHeader(this IDictionary<string, string[]> headers, string key, string value)
        {
            headers[key] = new[] { value };
            return headers;
        }

        public static AppFunc ToOwinAppFunc(this IEnumerable<Func<AppFunc, AppFunc>> app)
        {
            var apps = app.ToList();

            return
                env =>
                {
                    AppFunc next = null;
                    int index = 0;

                    next = env2 =>
                    {
                        if (index == apps.Count)
                            return Task.FromResult(0); // we are done

                        Func<AppFunc, AppFunc> other = apps[index++];
                        // ReSharper disable AccessToModifiedClosure
                        return other(env3 => next(env3))(env2);
                        // ReSharper restore AccessToModifiedClosure
                    };

                    return next(env);
                };
        }
    }
}