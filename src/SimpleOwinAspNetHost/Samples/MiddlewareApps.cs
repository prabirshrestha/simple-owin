
namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class MiddlewareApps
    {
        private static readonly Task CachedCompletedResultTupleTask;

        static MiddlewareApps()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            CachedCompletedResultTupleTask = tcs.Task;
        }

        public static AppFunc Middleware1(AppFunc app)
        {
            return env =>
                       {
                           var responseBody = (Stream)env["owin.ResponseBody"];
                           var msg = Encoding.UTF8.GetBytes("response from middleware 1<br/>");
                           responseBody.Write(msg, 0, msg.Length);

                           return app(env);
                       };
        }

        public static AppFunc Middleware2(AppFunc app)
        {
            return env =>
                       {
                           var responseBody = (Stream)env["owin.ResponseBody"];
                           var msg = Encoding.UTF8.GetBytes("response from middleware 2<br/>");
                           responseBody.Write(msg, 0, msg.Length);

                           // don't call the next app. this middleware ends the response
                           return CachedCompletedResultTupleTask;
                       };
        }

        public static AppFunc Middleware3(AppFunc app)
        {
            return env =>
            {
                var responseBody = (Stream)env["owin.ResponseBody"];
                var msg = Encoding.UTF8.GetBytes("response from middleware 3. this should not execute.<br/>");
                responseBody.Write(msg, 0, msg.Length);

                return app(env);
            };
        }

        public static AppFunc OwinApp()
        {
            var app = new List<Func<AppFunc, AppFunc>>();
            app.Add(Middleware1);
            app.Add(Middleware2);
            app.Add(Middleware3);

            return
                env =>
                {
                    AppFunc next = null;
                    int index = 0;

                    next = env2 =>
                               {
                                   if (index == app.Count)
                                       return CachedCompletedResultTupleTask; // we are done

                                   Func<AppFunc, AppFunc> other = app[index++];
                                   return other(env3 => next(env3))(env2);
                               };

                    return next(env);
                };
        }
    }
}