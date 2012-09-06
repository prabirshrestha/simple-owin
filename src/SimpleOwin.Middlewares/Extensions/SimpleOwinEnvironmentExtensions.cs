namespace SimpleOwin.Middlewares.Extensions
{
    using System.Collections.Generic;
    using System.IO;

    public static class SimpleOwinEnvironmentExtensions
    {
        public static IDictionary<string, string[]> GetOwinResponseHeaders(this IDictionary<string, object> env)
        {
            return env.GetSimpleOwinValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static Stream GetOwinResponseBody(this IDictionary<string, object> env)
        {
            return env.GetSimpleOwinValue<Stream>("owin.ResponseBody");
        }

        public static IDictionary<string, object> SetOwinResponseStatusCode(this IDictionary<string, object> env, int statusCode)
        {
            env["owin.ResponseStatusCode"] = statusCode;
            return env;
        }

        public static T GetSimpleOwinValue<T>(this IDictionary<string, object> env, string key, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
        }
    }
}