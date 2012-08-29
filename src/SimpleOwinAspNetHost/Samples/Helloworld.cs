
namespace SimpleOwinAspNetHost.Samples
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class Helloworld
    {
        private static readonly Task CachedCompletedResultTupleTask;

        static Helloworld()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            CachedCompletedResultTupleTask = tcs.Task;
        }

        public static AppFunc OwinApp()
        {
            return env =>
                       {
                           object version;
                           env.TryGetValue("owin.Version", out version);

                           if (version == null || !string.Equals(version.ToString(), "1.0"))
                               throw new InvalidOperationException("An OWIN v1.0 host is required");

                           var owinRequestMethod = Get<string>(env, "owin.RequestMethod");
                           var owinRequestScheme = Get<string>(env, "owin.RequestScheme");
                           var owinRequestHeaders = Get<IDictionary<string, string[]>>(env, "owin.RequestHeaders");
                           var owinRequestPathBase = Get<string>(env, "owin.RequestPathBase");
                           var owinRequestPath = Get<string>(env, "owin.RequestPath");
                           var owinRequestQueryString = Get<string>(env, "owin.RequestQueryString");
                           var serverClientIp = Get<string>(env, "server.CLIENT_IP");
                           var callCancelled = Get<Task>(env, "owin.CallCancelled");

                           var uriHostName = GetHeader(owinRequestHeaders, "Host");
                           var uri = string.Format("{0}://{1}{2}{3}{4}{5}", owinRequestScheme, uriHostName,
                                                   owinRequestPathBase, owinRequestPath,
                                                   owinRequestQueryString == "" ? "" : "?", owinRequestQueryString);

                           var owinResponseHeaders = Get<IDictionary<string, string[]>>(env, "owin.ResponseHeaders");
                           var owinResponseBody = Get<Stream>(env, "owin.ResponseBody");

                           env["owin.ResponseStatusCode"] = 200;
                           owinResponseHeaders.Add("custom header", new[] { "custom header value" });

                           var msg = Encoding.UTF8.GetBytes("hello world");
                           owinResponseBody.Write(msg, 0, msg.Length);

                           return CachedCompletedResultTupleTask;
                       };
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : default(T);
        }

        private static string GetHeader(IDictionary<string, string[]> headers, string key)
        {
            string[] value;
            return headers.TryGetValue(key, out value) && value != null ? string.Join(",", value.ToArray()) : null;
        }
    }
}