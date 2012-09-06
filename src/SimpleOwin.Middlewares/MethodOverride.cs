namespace SimpleOwin.Middlewares
{
    using System;
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

                    var method = env.GetOwinResponseHeaderValue("x-http-method-override");

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