namespace SimpleOwin.Middlewares.Router
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class UriTemplateRouter : IRouter
    {
        private readonly UriTemplateTable _table;
        private readonly Uri _prefix = new Uri("http://localhost/");

        public UriTemplateRouter(ICollection<Func<AppFunc, AppFunc>> app = null)
        {
            _table = new UriTemplateTable(_prefix);
            if (app != null)
                app.Add(Middleware());
        }

        private void Method(string methodName, string route, Func<AppFunc, AppFunc> callback)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentNullException("route");

            _table.KeyValuePairs.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(route), new KeyValuePair<string, Func<AppFunc, AppFunc>>(methodName, callback)));
        }

        public void All(Func<AppFunc, AppFunc> callback)
        {
            All("*", callback);
        }

        public void All(string route, Func<AppFunc, AppFunc> callback)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentNullException("route");

            _table.KeyValuePairs.Add(new KeyValuePair<UriTemplate, object>(new UriTemplate(route), new KeyValuePair<string, Func<AppFunc, AppFunc>>(null, callback)));
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
                       var path = env.GetOwinRequestPath();
                       var matches = _table.Match(new Uri(_prefix, new Uri(path, UriKind.Relative)));

                       if (!matches.Any())
                           return next(env);

                       var method = env.GetOwinRequestMethod();
                       foreach (var uriTemplateMatch in matches)
                       {
                           var match = (KeyValuePair<string, Func<AppFunc, AppFunc>>)uriTemplateMatch.Data;
                           if (match.Key == null || method.Equals(match.Key, StringComparison.OrdinalIgnoreCase))
                           {
                               var app = match.Value;
                               return app(next)(env);
                           }
                       }

                       return next(env);
                   };
        }
    }
}