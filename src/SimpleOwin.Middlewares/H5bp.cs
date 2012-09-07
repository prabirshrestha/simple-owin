namespace SimpleOwin.Middlewares
{
    using System;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class H5bp
    {
        public static Func<AppFunc, AppFunc> IeEdgeChromeFrameHeader()
        {
            return
                next =>
                env =>
                {
                    var userAgent = env.GetOwinRequestHeaderValue("user-agent");
                    if (!string.IsNullOrWhiteSpace(userAgent) && userAgent.IndexOf("MSIE", StringComparison.Ordinal) > 1)
                    {
                        env.GetOwinResponseHeaders()
                            .SetOwinHeader("X-UA-Compatible", "IE=Edge,chrome=1");
                    }

                    return next(env);
                };
        }
    }
}