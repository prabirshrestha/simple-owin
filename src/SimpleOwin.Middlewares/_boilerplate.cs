namespace SimpleOwin.Middlewares
{
    using System;
    using SimpleOwin.Extensions;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class SimpleOwinMiddlewareBoilerplate
    {
         public static Func<AppFunc, AppFunc> Middleware()
         {
             return
                 next =>
                 env =>
                 {

                     return next(env);
                 };
         }
    }
}