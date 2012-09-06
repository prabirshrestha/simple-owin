namespace SimpleOwin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class SimpleOwinExtensions
    {
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
    }
}