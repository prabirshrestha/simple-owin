
// VERSION: 
// https://github.com/prabirshrestha/simple-owin

namespace SimpleOwin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Env = System.Collections.Generic.IDictionary<string, object>;
    using WsEnv = System.Collections.Generic.IDictionary<string, object>;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

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

    public static class SimpleOwinExtensions
    {
        public static readonly Task NoopTask;

        static SimpleOwinExtensions()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            NoopTask = tcs.Task;
        }

        public static T GetOwinStartupValue<T>(this IDictionary<string, object> startup, string name, T defaultValue = default(T))
        {
            object value;
            return startup.TryGetValue(name, out value) && value is T ? (T)value : defaultValue;
        }

        public static T GetOwinEnvironmentValue<T>(this Env env, string name, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(name, out value) && value is T ? (T)value : defaultValue;
        }

        public static Env SetOwinStartupValue(this IDictionary<string, object> startup, string name, object value)
        {
            startup[name] = value;
            return startup;
        }

        public static IDictionary<string, object> SetOwinEnvironmentValue(this Env env, string name, object value)
        {
            env[name] = value;
            return env;
        }

        public static string GetOwinRequestMethod(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestMethod");
        }

        public static string GetOwinRequestScheme(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestScheme");
        }

        public static string GetOwinRequestPathBase(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestPathBase");
        }

        public static string GetOwinRequestPath(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestPath");
        }

        public static string GetOwinRequestQueryString(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestQueryString");
        }

        public static System.IO.Stream GetOwinRequestBody(this Env env)
        {
            return env.GetOwinEnvironmentValue<System.IO.Stream>("owin.RequestBody");
        }

        public static string GetOwinCallCancelled(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.CallCancelled");
        }

        public static IDictionary<string, string[]> GetOwinRequestHeaders(this Env env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.RequestHeaders");
        }

        public static string GetOwinServerClientIp(this Env env)
        {
            return env.GetOwinEnvironmentValue<string>("server.CLIENT_IP");
        }

        public static IDictionary<string, string[]> GetOwinResponseHeaders(this Env env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static System.IO.Stream GetOwinResponseBody(this Env env)
        {
            return env.GetOwinEnvironmentValue<System.IO.Stream>("owin.ResponseBody");
        }

        public static string[] GetOwinHeaderValues(this IDictionary<string, string[]> headers, string name, string[] defaultValue = null)
        {
            string[] values;
            return headers.TryGetValue(name, out values) ? values : defaultValue;
        }

        public static string GetOwinHeaderValue(this IDictionary<string, string[]> headers, string name)
        {
            var values = headers.GetOwinHeaderValues(name);

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

        public static Env SetOwinRequestMethod(this Env env, string method)
        {
            return env.SetOwinEnvironmentValue("owin.RequestMethod", method);
        }

        public static Env SetOwinRequestScheme(this Env env, string scheme)
        {
            return env.SetOwinEnvironmentValue("owin.RequestScheme", scheme);
        }

        public static Env SetOwinRequestPathBase(this Env env, string pathBase)
        {
            return env.SetOwinEnvironmentValue("owin.RequestPathBase", pathBase);
        }

        public static Env SetOwinRequestPath(this Env env, string path)
        {
            return env.SetOwinEnvironmentValue("owin.RequestPath", path);
        }

        public static Env SetOwinRequestQueryString(this Env env, string queryString)
        {
            return env.SetOwinEnvironmentValue("owin.RequestQueryString", queryString);
        }

        public static Env SetOwinRequestBody(this Env env, System.IO.Stream stream)
        {
            return env.SetOwinEnvironmentValue("owin.RequestBody", stream);
        }

        public static Env SetOwinCallCancelled(this Env env, CancellationToken cancellationToken)
        {
            return env.SetOwinEnvironmentValue("owin.CallCancelled", cancellationToken);
        }

        public static Env SetOwinResponseStatusCode(this Env env, int statusCode)
        {
            return env.SetOwinEnvironmentValue("owin.ResponseStatusCode", statusCode);
        }

        public static IDictionary<string, string[]> SetOwinHeader(this IDictionary<string, string[]> headers, string name, string[] value)
        {
            headers[name] = value;
            return headers;
        }

        public static IDictionary<string, string[]> SetOwinHeader(this IDictionary<string, string[]> headers, string name, string value)
        {
            headers[name] = new[] { value };
            return headers;
        }

        public static IDictionary<string, string[]> RemoveOwinHeader(this IDictionary<string, string[]> headers, string name)
        {
            if (headers.ContainsKey(name))
                headers.Remove(name);
            return headers;
        }

        public static string GetOwinWebSocketsVersion(this WsEnv wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<string>("websocket.Version");
        }

        public static WebSocketSendAsync GetOwinWebSocketsSendAsync(this WsEnv wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<WebSocketSendAsync>("websocket.SendAsyncFunc");
        }

        public static WebSocketReceiveAsync GetOwinWebSocketsReceiveAsync(this WsEnv wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<WebSocketReceiveAsync>("websocket.ReceiveAsyncFunc");
        }

        public static WebSocketCloseAsync GetOwinWebSocketsCloseAsync(this WsEnv wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<WebSocketCloseAsync>("websocket.CloseAsyncFunc");
        }

        public static CancellationToken GetOwinWebSocketCallCancelled(this WsEnv wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<CancellationToken>("websocket.CallCancelled");
        }

        public static WsEnv SetOwinWebSocketsVersion(this WsEnv wsEnv, string version)
        {
            return wsEnv.SetOwinEnvironmentValue("websocket.Version", version);
        }

        public static WsEnv SetOwinWebSocketsSendAsync(this WsEnv wsEnv, string version)
        {
            return wsEnv.SetOwinEnvironmentValue("websocket.SendAsyncFunc", version);
        }

        public static WsEnv SetOwinWebSocketsReceiveAsync(this WsEnv wsEnv, string version)
        {
            return wsEnv.SetOwinEnvironmentValue("websocket.ReceiveAsyncFunc", version);
        }

        public static WsEnv SetOwinWebSocketsCloseAsync(this WsEnv wsEnv, string version)
        {
            return wsEnv.SetOwinEnvironmentValue("websocket.CloseAsyncFunc", version);
        }

        public static WsEnv SetOwinWebSocketCallCancelled(this WsEnv wsEnv, string version)
        {
            return wsEnv.SetOwinEnvironmentValue("websocket.CallCancelled", version);
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
    }

    namespace Stream
    {
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