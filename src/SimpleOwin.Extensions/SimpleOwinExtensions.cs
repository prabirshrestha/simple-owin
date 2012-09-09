namespace SimpleOwin.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    using WebSocketFunc =
       System.Func<
           System.Collections.Generic.IDictionary<string, object>, // WebSocket Environment
           System.Threading.Tasks.Task>; // Complete

    using WebSocketSendAsync = System.Func<
                System.ArraySegment<byte>, // data
                int, // message type
                bool, // end of message
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task>;

    using WebSocketReceiveTuple = System.Tuple<
                        int, // messageType
                        bool, // endOfMessage
                        int?, // count
                        int?, // closeStatus
                        string>; // closeStatusDescription

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

    using WebSocketCloseAsync = System.Func<
                int, // closeStatus
                string, // closeDescription
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task>;

    public static class SimpleOwinExtensions
    {
        public static readonly Task CachedCompletedResultTupleTask;

        static SimpleOwinExtensions()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            CachedCompletedResultTupleTask = tcs.Task;
        }

        public static string GetOwinRequestMethod(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestMethod");
        }

        public static IDictionary<string, object> SetOwinRequestMethod(this IDictionary<string, object> env, string method)
        {
            env["owin.RequestMethod"] = method;
            return env;
        }

        public static string GetOwinRequestScheme(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestScheme");
        }

        public static string GetOwinRequestPathBase(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestPathBase");
        }

        public static string GetOwinRequestPath(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestPath");
        }

        public static string GetOwinRequestQueryString(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.RequestQueryString");
        }

        public static Stream GetOwinRequestBody(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<Stream>("owin.RequestBody");
        }

        public static string GetOwinCallCancelled(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("owin.CallCancelled");
        }

        public static string GetOwinServerClientIp(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<string>("server.CLIENT_IP");
        }

        public static IDictionary<string, string[]> GetOwinRequesteHeaders(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.RequestHeaders");
        }

        public static IDictionary<string, object> GetOwinRequesteHeaders(this IDictionary<string, object> env, Action<IDictionary<string, string[]>> callback)
        {
            var headers = env.GetOwinResponseHeaders();
            if (callback != null)
                callback(headers);
            return env;
        }

        public static IDictionary<string, string[]> GetOwinResponseHeaders(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<IDictionary<string, string[]>>("owin.ResponseHeaders");
        }

        public static IDictionary<string, object> GetOwinResponseHeaders(this IDictionary<string, object> env, Action<IDictionary<string, string[]>> callback)
        {
            var headers = env.GetOwinResponseHeaders();
            if (callback != null)
                callback(headers);
            return env;
        }

        public static Stream GetOwinResponseBody(this IDictionary<string, object> env)
        {
            return env.GetOwinEnvironmentValue<Stream>("owin.ResponseBody");
        }

        public static IDictionary<string, object> SetOwinResponseStatusCode(this IDictionary<string, object> env, int statusCode)
        {
            env["owin.ResponseStatusCode"] = statusCode;
            return env;
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

        public static string[] GetOwinRequestHeaderValues(this IDictionary<string, object> env, string name, string[] defaultValue = null)
        {
            return env
                .GetOwinRequesteHeaders()
                .GetOwinHeaderValues(name, defaultValue);
        }

        public static string GetOwinRequestHeaderValue(this IDictionary<string, object> env, string name)
        {
            return env
                .GetOwinRequesteHeaders()
                .GetOwinHeaderValue(name);
        }

        public static string[] GetOwinResponseHeaderValues(this IDictionary<string, object> env, string name, string[] defaultValue = null)
        {
            return env
                .GetOwinResponseHeaders()
                .GetOwinHeaderValues(name, defaultValue);
        }

        public static string GetOwinResponseHeaderValue(this IDictionary<string, object> env, string name)
        {
            return env
                .GetOwinResponseHeaders()
                .GetOwinHeaderValue(name);
        }

        public static T GetOwinEnvironmentValue<T>(this IDictionary<string, object> env, string name, T defaultValue = default(T))
        {
            object value;
            return env.TryGetValue(name, out value) && value is T ? (T)value : defaultValue;
        }

        public static ICollection<Func<AppFunc, AppFunc>> Use(this ICollection<Func<AppFunc, AppFunc>> app, Func<AppFunc, AppFunc> middleware)
        {
            app.Add(middleware);
            return app;
        }

        public static AppFunc ToOwinApp(this IEnumerable<Func<AppFunc, AppFunc>> app)
        {
            return
                env =>
                {
                    var enumerator = app.GetEnumerator();
                    AppFunc next = null;
                    next = env2 => enumerator.MoveNext() ? enumerator.Current(env3 => next(env3))(env2) : CachedCompletedResultTupleTask;
                    return next(env);
                };
        }

        public static ICollection<Func<AppFunc, AppFunc>> SimpleOwinAddIf(this ICollection<Func<AppFunc, AppFunc>> apps, Func<IDictionary<string, object>, bool> condition, Func<AppFunc, AppFunc> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            apps.Add(next => env => condition(env) ? callback(next)(env) : next(env));
            return apps;
        }

        public static string GetOwinWebSocketsVersion(this IDictionary<string, object> wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<string>("websocket.Version");
        }

        public static WebSocketSendAsync GetOwinWebSocketsSendAsync(this IDictionary<string, object> wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<WebSocketSendAsync>("websocket.SendAsyncFunc");
        }

        public static WebSocketReceiveAsync GetOwinWebSocketsReceiveAsync(this IDictionary<string, object> wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<WebSocketReceiveAsync>("websocket.ReceiveAsyncFunc");
        }

        public static WebSocketCloseAsync GetOwinWebSocketsCloseAsync(this IDictionary<string, object> wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<WebSocketCloseAsync>("websocket.CloseAsyncFunc");
        }

        public static CancellationToken GetOwinWebSocketCallCancelled(this IDictionary<string, object> wsEnv)
        {
            return wsEnv.GetOwinEnvironmentValue<CancellationToken>("websocket.CallCancelled");
        }
    }
}