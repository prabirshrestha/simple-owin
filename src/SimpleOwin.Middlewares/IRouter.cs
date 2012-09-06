namespace SimpleOwin.Middlewares
{
    using System;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public interface IRouter
    {
        void All(Func<AppFunc, AppFunc> callback);
        void All(string route, Func<AppFunc, AppFunc> callback);

        void Get(string route, Func<AppFunc, AppFunc> callback);
        void Post(string route, Func<AppFunc, AppFunc> callback);
        void Put(string route, Func<AppFunc, AppFunc> callback);
        void Delete(string route, Func<AppFunc, AppFunc> callback);
        void Head(string route, Func<AppFunc, AppFunc> callback);
        void Options(string route, Func<AppFunc, AppFunc> callback);
        void Patch(string route, Func<AppFunc, AppFunc> callback);

        Func<AppFunc, AppFunc> Middleware();
    }
}