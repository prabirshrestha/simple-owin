
namespace SimpleOwin.Middlewares.Router
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class RegexRouter : IRouter
    {
        private readonly bool _ignoreCase;
        // method, route, regex, callback
        private readonly ICollection<Tuple<string, string, Regex, Func<AppFunc, AppFunc>>> _routes;

        private readonly RegexOptions _defaultRegexOptions;

        public bool IgnoreCase { get { return _ignoreCase; } }

        public RegexRouter(ICollection<Func<AppFunc, AppFunc>> app = null, bool ignoreCase = true)
        {
            _ignoreCase = ignoreCase;
            _defaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.Compiled;

            _routes = new List<Tuple<string, string, Regex, Func<AppFunc, AppFunc>>>();

            if (ignoreCase)
                _defaultRegexOptions |= RegexOptions.IgnoreCase;

            if (app != null)
                app.Add(Middleware());
        }

        private Regex CreateRegex(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentNullException("route");

            if (route == "*")
                route = "(.)*";

            return new Regex(route, _defaultRegexOptions);
        }

        public void All(Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            _routes.Add(new Tuple<string, string, Regex, Func<AppFunc, AppFunc>>(null, null, null, callback));
        }

        public void All(string route, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var regex = CreateRegex(route);

            _routes.Add(new Tuple<string, string, Regex, Func<AppFunc, AppFunc>>(null, route, regex, callback));
        }

        private void Method(string methodName, string route, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var regex = CreateRegex(route);

            _routes.Add(new Tuple<string, string, Regex, Func<AppFunc, AppFunc>>(methodName, route, regex, callback));
        }

        public void Get(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("GET", route, callback);
        }

        public void Post(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("POST", route, callback);
        }

        public void Put(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("POST", route, callback);
        }

        public void Delete(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("DELETE", route, callback);
        }

        public void Head(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("HEAD", route, callback);
        }

        public void Options(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("Options", route, callback);
        }

        public void Patch(string route, Func<AppFunc, AppFunc> callback)
        {
            Method("PATCH", route, callback);
        }

        public Func<AppFunc, AppFunc> Middleware()
        {
            return next =>
                   env =>
                   {
                       var method = env.GetOwinRequestMethod();
                       var path = env.GetOwinRequestPath();

                       foreach (var routeToExectue in _routes)
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
                           }

                           return callback(next)(env);
                       }

                       return next(env);
                   };
        }
    }
}
