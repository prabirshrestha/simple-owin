
namespace SimpleOwin.Middlewares
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    namespace Extensions
    {
        public static class RegexRouterExtensions
        {
            public static IDictionary<string, string> GetSimpleOwinRouteParameters(this IDictionary<string, object> env)
            {
                return env.GetEnvironmentValue<IDictionary<string, string>>("SimpleOwin.Route.Parameters");
            }
        }
    }

    public class RegexRouter
    {
        private readonly bool ignoreCase;

        // method, route, regex, callback
        private readonly ICollection<Tuple<string, string, Regex, Func<AppFunc, AppFunc>>> routes;

        private readonly RegexOptions defaultRegexOptions;

        public bool IgnoreCase { get { return this.ignoreCase; } }

        public RegexRouter(ICollection<Func<AppFunc, AppFunc>> app = null, bool ignoreCase = true, Action<RegexRouter> config = null)
        {
            this.ignoreCase = ignoreCase;
            this.defaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.Compiled;

            this.routes = new List<Tuple<string, string, Regex, Func<AppFunc, AppFunc>>>();

            if (ignoreCase)
                this.defaultRegexOptions |= RegexOptions.IgnoreCase;

            if (app != null)
                app.Add(Middleware());

            if (config != null)
                config(this);
        }

        private Regex CreateRegex(string route)
        {
            if (string.IsNullOrEmpty(route))
                throw new ArgumentNullException("route");
            else if (route == "*")
                route = "(.)*";
            else if (route == "/")
                route = "^$";
            else if (route.StartsWith("/"))
                route = route.Substring(1);
			
			return new Regex(route, this.defaultRegexOptions);
        }

        public RegexRouter All(Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            this.routes.Add(new Tuple<string, string, Regex, Func<AppFunc, AppFunc>>(null, null, null, callback));
            return this;
        }

        public RegexRouter All(string route, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var regex = CreateRegex(route);

            this.routes.Add(new Tuple<string, string, Regex, Func<AppFunc, AppFunc>>(null, route, regex, callback));
            return this;
        }

        private RegexRouter Method(string methodName, string route, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var regex = CreateRegex(route);

            this.routes.Add(new Tuple<string, string, Regex, Func<AppFunc, AppFunc>>(methodName, route, regex, callback));
            return this;
        }

        public RegexRouter Get(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("GET", route, callback);
        }

        public RegexRouter Post(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("POST", route, callback);
        }

        public RegexRouter Put(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("POST", route, callback);
        }

        public RegexRouter Delete(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("DELETE", route, callback);
        }

        public RegexRouter Head(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("HEAD", route, callback);
        }

        public RegexRouter Options(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("Options", route, callback);
        }

        public RegexRouter Patch(string route, Func<AppFunc, AppFunc> callback)
        {
            return Method("PATCH", route, callback);
        }

        public Func<AppFunc, AppFunc> Middleware()
        {
            return next =>
                   env =>
                   {
                       var method = env.GetRequestMethod();
                       var path = env.GetRequestPath();

                       foreach (var routeToExectue in this.routes)
                       {
                           var routeMethod = routeToExectue.Item1;
                           //var route = routeToExectue.Item2;
                           var regex = routeToExectue.Item3;
                           var callback = routeToExectue.Item4;

                           if (routeMethod != null)
                           {
                               if (!routeMethod.Equals(method, StringComparison.OrdinalIgnoreCase))
                                   continue;
                           }

                           if (regex != null)
                           {
                               var match = regex.Match(path);
                               if (!match.Success)
                                   continue;

                               var matched = new Dictionary<string, string>();
                               for (int i = 0; i < match.Groups.Count; i++)
                                   matched[regex.GroupNameFromNumber(i)] = match.Groups[i].Value;
                               env["SimpleOwin.Route.Parameters"] = matched;
                           }

                           return callback(next)(env);
                       }

                       return next(env);
                   };
        }
    }
}
