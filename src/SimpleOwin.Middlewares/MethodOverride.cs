namespace SimpleOwin.Middlewares
{
    using System;
    using System.Collections.Generic;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class MethodOverride
    {
        public static Func<AppFunc, AppFunc> Middleware(string key = "_method")
        {
            return
                next =>
                env =>
                {
                    string originalMethod = env.GetOwinRequestMethod();

                    var method = env.GetOwinRequestHeaderValue("x-http-method-override");

                    if (string.IsNullOrWhiteSpace(method))
                    {
                        // try checking querystring, requires QueryParser middleware
                        var qs = env.GetOwinEnvironmentValue<Lazy<IDictionary<string, string[]>>>("simpleOwin.query");
                        if (qs != null)
                            method = qs.Value.GetOwinHeaderValue(key);
                    }

                    if (!string.IsNullOrWhiteSpace(method))
                    {
                        env.SetOwinRequestMethod(method.ToUpperInvariant());
                        env["simpleOwin.originalRequestMethod"] = originalMethod;
                    }

                    return next(env);
                };
        }
    }
}