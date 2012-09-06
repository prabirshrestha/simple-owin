namespace SimpleOwin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public static class SimpleOwinExtensions
    {
        public static readonly Task CachedCompletedResultTupleTask;

        static SimpleOwinExtensions()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            CachedCompletedResultTupleTask = tcs.Task;
        }

        public static string GetOwinRequestMethod(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestMethod");
        }

        public static string GetOwinRequestScheme(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestScheme");
        }

        public static string GetOwinRequestPathBase(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestPathBase");
        }

        public static string GetOwinRequestPath(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestPath");
        }

        public static string GetOwinRequestQueryString(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestQueryString");
        }

        public static string GetOwinCallCancelled(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.CallCancelled");
        }

        public static string GetOwinServerClientIp(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("server.CLIENT_IP");
        }

        public static IDictionary<string, string[]> GetOwinRequesteHeaders(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.RequestHeaders");
        }

        public static IDictionary<string, object> GetOwinRequesteHeaders(this IDictionary<string, object> env, Action<IDictionary<string, string[]>> callback)
        {
            var headers = env.GetOwinResponseHeaders();
            if (callback != null)
                callback(headers);
            return env;
        }

        public static IDictionary<string, string[]> GetOwinResponseHeaders(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static IDictionary<string, object> GetOwinResponseHeaders(this IDictionary<string, object> env, Action<IDictionary<string, string[]>> callback)
        {
            var headers = env.GetOwinResponseHeaders();
            if (callback != null)
                callback(headers);
            return env;
        }

        public static Stream GetOwinResponseBody(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<Stream>("owin.ResponseBody");
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

        public static T GetOwinEnvironmentValue<T>(this IDictionary<string, object> env, string key, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
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
                            return CachedCompletedResultTupleTask; // we are done

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