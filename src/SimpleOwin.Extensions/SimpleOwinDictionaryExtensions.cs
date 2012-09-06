namespace SimpleOwin.Extensions
{
    using System.Collections.Generic;

    public static class SimpleOwinDictionaryExtensions
    {
        public static T GetOwinEnvironmentValue<T>(this IDictionary<string, object> env, string key, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
        }
    }
}