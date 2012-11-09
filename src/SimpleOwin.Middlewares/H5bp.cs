namespace SimpleOwin.Middlewares
{
    using System;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class H5bp
    {
        /// <summary>
        /// Send the IE=Edge and chrome=1 headers for IE browsers on html/htm requests.
        /// </summary>
        /// <returns></returns>
        public static Func<AppFunc, AppFunc> IeEdgeChromeFrameHeader()
        {
            return
                next =>
                env =>
                {
                    var userAgent = env
                        .GetRequestHeaders()
                        .GetOwinHeaderValue("user-agent");

                    if (!string.IsNullOrWhiteSpace(userAgent) && userAgent.IndexOf("MSIE", StringComparison.Ordinal) > 1)
                    {
                        // todo: only for html/htm requests
                        env.GetResponseHeaders()
                            .SetHeader("X-UA-Compatible", "IE=Edge,chrome=1");
                    }

                    return next(env);
                };
        }

        public static Func<AppFunc, AppFunc> RemovePoweredBy()
        {
            return
                next =>
                env =>
                {
                    env.GetResponseHeaders()
                        .RemoveHeader("X-Powered-By");

                    return next(env);
                };
        }

        /// <summary>
        /// Enable CORS cross domain rules, more info at http://enble-cors.org/
        /// </summary>
        public static Func<AppFunc, AppFunc> CrossDomainRules()
        {
            return
                next =>
                env =>
                {
                    env.GetResponseHeaders()
                        .SetHeader("Access-Control-Allow-Origin", "*")
                        .SetHeader("Access-Control-Allow-Headers", "X-Requested-With");

                    return next(env);
                };
        }
    }
}