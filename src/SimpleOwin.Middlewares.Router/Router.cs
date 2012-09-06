
namespace SimpleOwin.Middlewares.Router
{
    using SimpleOwin.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class Router
    {
        private readonly bool _ignoreCase;
        private readonly ICollection<Func<AppFunc, AppFunc>> _routes;
        private readonly RegexOptions _defaultRegexOptions;

        public bool IgnoreCase { get { return _ignoreCase; } }

        public Router(bool ignoreCase = true)
        {
            _ignoreCase = ignoreCase;
            _routes = new List<Func<AppFunc, AppFunc>>();
            _defaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.Compiled;

            if (ignoreCase)
                _defaultRegexOptions |= RegexOptions.IgnoreCase;
        }

        private Regex CreateRegex(string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentNullException("route");

            if (route == "*")
                route = "(.)*";

            return new Regex(route, _defaultRegexOptions);
        }

        public void All(string route, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var regex = CreateRegex(route);

            _routes.SimpleOwinAddIf(env =>
                                        {
                                            var requestPath = env.GetOwinRequestPath();
                                            return regex.IsMatch(requestPath);
                                        }, callback);
            _routes.Add(callback);
        }

        private void Method(string methodName, string route, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var regex = CreateRegex(route);

            _routes.SimpleOwinAddIf(env =>
                                        {
                                            var method = env.GetOwinRequestMethod();
                                            var requestPath = env.GetOwinRequestPath();
                                            return method.Equals(methodName, StringComparison.OrdinalIgnoreCase) && regex.IsMatch(requestPath);

                                        }, callback);
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
            return app => env => _routes.ToOwinAppFunc()(env);
        }
    }
}
