namespace SimpleOwin.Extensions
{
    using System.Collections.Generic;

    public static class SimpleOwinEnvironmentExtensions
    {
        public static IDictionary<string, string[]> GetOwinResponseHeaders(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }
    }
}