namespace SimpleOwin.Middlewares.Extensions
{
    using System.Collections.Generic;

    public static class SimpleOwinEnvironmentExtensions
    {
        public static IDictionary<string, string[]> GetOwinResponseHeaders(this IDictionary<string, object> env)
        {
            return env.GetSimpleOwinValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static T GetSimpleOwinValue<T>(this IDictionary<string, object> env, string key, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
        }
    }
}