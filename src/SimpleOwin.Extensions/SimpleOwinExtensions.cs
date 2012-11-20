
// VERSION: 
// https://github.com/prabirshrestha/simple-owin

namespace SimpleOwin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using StartupEnv = System.Collections.Generic.IDictionary<string, object>;
    using Env = System.Collections.Generic.IDictionary<string, object>;
    using WsEnv = System.Collections.Generic.IDictionary<string, object>;

    using Headers = System.Collections.Generic.IDictionary<string, string[]>;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    using WebSocketAccept = System.Action<
                    System.Collections.Generic.IDictionary<string, object>, // WebSocket Accept parameters
                    System.Func< // WebSocketFunc callback
                        System.Collections.Generic.IDictionary<string, object>, // WebSocket environment
                        System.Threading.Tasks.Task>>;

    using WebSocketCloseAsync = System.Func<
                    int, // closeStatus
                    string, // closeDescription
                    System.Threading.CancellationToken, // cancel
                    System.Threading.Tasks.Task>; // closeStatusDescription

    using WebSocketReceiveAsync = System.Func<
                System.ArraySegment<byte>, // data
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task<
                    System.Tuple< // WebSocketReceiveTuple
                        int, // messageType
                        bool, // endOfMessage
                        int?, // count
                        int?, // closeStatus
                        string>>>; // closeStatusDescription

    using WebSocketSendAsync = System.Func<
                System.ArraySegment<byte>, // data
                int, // message type
                bool, // end of message
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task>;

    public static class OwinExtensions
    {
        public static readonly Task NoopTask;

        static OwinExtensions()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            NoopTask = tcs.Task;
        }

        public static T GetStartupValue<T>(this StartupEnv startup, string name, T defaultValue = default(T))
        {
            object value;
            return startup.TryGetValue(name, out value) && value is T ? (T)value : defaultValue;
        }

        public static StartupEnv SetStartupValue(this StartupEnv startup, string name, object value)
        {
            startup[name] = value;
            return startup;
        }

        public static T GetEnvironmentValue<T>(this Env env, string name, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(name, out value) && value is T ? (T)value : defaultValue;
        }

        public static Env SetEnvironmentValue(this Env env, string name, object value)
        {
            env[name] = value;
            return env;
        }

        public static string GetRequestMethod(this Env env)
        {
            return env.GetEnvironmentValue<string>("owin.RequestMethod");
        }

        public static string GetRequestScheme(this Env env)
        {
            return env.GetEnvironmentValue<string>("owin.RequestScheme");
        }

        public static string GetRequestPathBase(this Env env)
        {
            return env.GetEnvironmentValue<string>("owin.RequestPathBase");
        }

        public static string GetRequestPath(this Env env)
        {
            return env.GetEnvironmentValue<string>("owin.RequestPath");
        }

        public static string GetRequestQueryString(this Env env)
        {
            return env.GetEnvironmentValue<string>("owin.RequestQueryString");
        }

        public static System.IO.Stream GetRequestBody(this Env env)
        {
            return env.GetEnvironmentValue<System.IO.Stream>("owin.RequestBody");
        }

        public static string GetCallCancelled(this Env env)
        {
            return env.GetEnvironmentValue<string>("owin.CallCancelled");
        }

        public static Headers GetRequestHeaders(this Env env)
        {
            return env.GetEnvironmentValue<Headers>("owin.RequestHeaders");
        }

        public static Headers GetResponseHeaders(this Env env)
        {
            return env.GetEnvironmentValue<Headers>("owin.ResponseHeaders");
        }

        public static System.IO.Stream GetResponseBody(this Env env)
        {
            return env.GetEnvironmentValue<System.IO.Stream>("owin.ResponseBody");
        }

        public static string[] GetHeaderValues(this Headers headers, string name, string[] defaultValue = null)
        {
            string[] values;
            return headers.TryGetValue(name, out values) ? values : defaultValue;
        }

        public static string GetOwinHeaderValue(this Headers headers, string name)
        {
            var values = headers.GetHeaderValues(name);

            if (values == null)
                return null;

            switch (values.Length)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return values[0];
                default:
                    return string.Join(",", values);
            }
        }

        public static Env SetRequestMethod(this Env env, string method)
        {
            return env.SetEnvironmentValue("owin.RequestMethod", method);
        }

        public static Env SetRequestScheme(this Env env, string scheme)
        {
            return env.SetEnvironmentValue("owin.RequestScheme", scheme);
        }

        public static Env SetRequestPathBase(this Env env, string pathBase)
        {
            return env.SetEnvironmentValue("owin.RequestPathBase", pathBase);
        }

        public static Env SetRequestPath(this Env env, string path)
        {
            return env.SetEnvironmentValue("owin.RequestPath", path);
        }

        public static Env SetRequestQueryString(this Env env, string queryString)
        {
            return env.SetEnvironmentValue("owin.RequestQueryString", queryString);
        }

        public static Env SetRequestBody(this Env env, System.IO.Stream stream)
        {
            return env.SetEnvironmentValue("owin.RequestBody", stream);
        }

        public static Env SetCallCancelled(this Env env, CancellationToken cancellationToken)
        {
            return env.SetEnvironmentValue("owin.CallCancelled", cancellationToken);
        }

        public static Env SetResponseStatusCode(this Env env, int statusCode)
        {
            return env.SetEnvironmentValue("owin.ResponseStatusCode", statusCode);
        }

        public static Headers SetHeader(this Headers headers, string name, string[] value)
        {
            headers[name] = value;
            return headers;
        }

        public static Headers SetHeader(this Headers headers, string name, string value)
        {
            headers[name] = new[] { value };
            return headers;
        }

        public static Headers RemoveHeader(this Headers headers, string name)
        {
            if (headers.ContainsKey(name))
                headers.Remove(name);
            return headers;
        }

        public static string GetWebSocketVersion(this WsEnv wsEnv)
        {
            return wsEnv.GetEnvironmentValue<string>("websocket.Version");
        }

        public static WebSocketSendAsync GetWebSocketSendAsync(this WsEnv wsEnv)
        {
            return wsEnv.GetEnvironmentValue<WebSocketSendAsync>("websocket.SendAsync");
        }

        public static WebSocketReceiveAsync GetWebSocketReceiveAsync(this WsEnv wsEnv)
        {
            return wsEnv.GetEnvironmentValue<WebSocketReceiveAsync>("websocket.ReceiveAsync");
        }

        public static WebSocketCloseAsync GetWebSocketCloseAsync(this WsEnv wsEnv)
        {
            return wsEnv.GetEnvironmentValue<WebSocketCloseAsync>("websocket.CloseAsync");
        }

        public static CancellationToken GetWebSocketCallCancelled(this WsEnv wsEnv)
        {
            return wsEnv.GetEnvironmentValue<CancellationToken>("websocket.CallCancelled");
        }

        public static WsEnv SetWebSocketVersion(this WsEnv wsEnv, string version)
        {
            return wsEnv.SetEnvironmentValue("websocket.Version", version);
        }

        public static WsEnv SetWebSocketSendAsync(this WsEnv wsEnv, WebSocketSendAsync webSocketSendAsync)
        {
            return wsEnv.SetEnvironmentValue("websocket.SendAsync", webSocketSendAsync);
        }

        public static WsEnv SetWebSocketReceiveAsync(this WsEnv wsEnv, WebSocketReceiveAsync webSocketReceiveAsync)
        {
            return wsEnv.SetEnvironmentValue("websocket.ReceiveAsync", webSocketReceiveAsync);
        }

        public static WsEnv SetWebSocketCloseAsync(this WsEnv wsEnv, WebSocketCloseAsync webSocketCloseAsync)
        {
            return wsEnv.SetEnvironmentValue("websocket.CloseAsync", webSocketCloseAsync);
        }

        public static WsEnv SetWebSocketCallCancelled(this WsEnv wsEnv, CancellationToken cancellationToken)
        {
            return wsEnv.SetEnvironmentValue("websocket.CallCancelled", cancellationToken);
        }

        public static T Use<T>(this T app, Func<AppFunc, AppFunc> middleware)
            where T : ICollection<Func<AppFunc, AppFunc>>
        {
            app.Add(middleware);
            return app;
        }

        public static T Use<T>(this T app, Func<Env, bool> condition, Func<AppFunc, AppFunc> middleware)
          where T : ICollection<Func<AppFunc, AppFunc>>
        {
            app.Add(next => env => condition(env) ? middleware(next)(env) : next(env));
            return app;
        }

        public static AppFunc ToOwinApp(this IEnumerable<Func<AppFunc, AppFunc>> app)
        {
            return
                env =>
                {
                    var enumerator = app.GetEnumerator();
                    AppFunc next = null;
                    next = env2 => enumerator.MoveNext() ? enumerator.Current(env3 => next(env3))(env2) : NoopTask;
                    return next(env);
                };
        }

        public static AppFunc ToOwinApp(this Func<AppFunc, AppFunc> app)
        {
            return env => app(env2 => NoopTask)(env);
        }

    }

    namespace Stream
    {
        using System.Text;

        public static class StreamExtensions
        {
            public static void WriteString(this System.IO.Stream stream, string str, Encoding encoding)
            {
                var data = encoding.GetBytes(str);
                stream.Write(data, 0, data.Length);
            }

            public static void WriteString(this System.IO.Stream stream, string str)
            {
                stream.WriteString(str, Encoding.UTF8);
            }

            public static Task WriteStringAsync(this System.IO.Stream stream, string str, Encoding encoding)
            {
                var data = encoding.GetBytes(str);
                return stream.WriteAsync(data, 0, data.Length, null);
            }

            public static Task WriteStringAsync(this System.IO.Stream stream, string str)
            {
                var data = Encoding.UTF8.GetBytes(str);
                return stream.WriteAsync(data, 0, data.Length, null);
            }

            private static Task WriteAsync(this  System.IO.Stream stream, byte[] data, int offset, int count, object state)
            {
                return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, data, offset, count, state);
            }
        }
    }
}