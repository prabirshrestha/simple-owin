// https://github.com/prabirshrestha/simple-owin

namespace SimpleOwin.Middlewares.AspNetWebSocket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.WebSockets;
    using System.Reflection;
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

    public class AspNetWebSocketMiddleware
    {
        private readonly Func<object, bool> _isWebSocketRequest;
        private readonly Action<object, object[]> _acceptWebSocketRequest;
        private readonly Func<object, WebSocket> _getWebSocketFromWebSocketContext;

        public AspNetWebSocketMiddleware(bool autoDetect = true)
        {
            if (autoDetect)
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT ||
                    Environment.OSVersion.Version < new Version(6, 2))
                {
                    return;
                }
            }

            var systemWebDll = AppDomain.CurrentDomain.GetAssemblies()
                   .FirstOrDefault(assembly => assembly.FullName.StartsWith("System.Web,"));

            if (systemWebDll == null)
                return;

            var httpContextBaseType = systemWebDll.GetExportedTypes().FirstOrDefault(type => type.Name == "HttpContextBase");
            if (httpContextBaseType == null)
                return;

            var isWebSocketRequestPropertyInfo = httpContextBaseType.GetProperties()
                .FirstOrDefault(property => property.Name == "IsWebSocketRequest" && property.CanRead);

            if (isWebSocketRequestPropertyInfo == null)
                return;

            _isWebSocketRequest = GetGetMethodByExpression<bool>(isWebSocketRequestPropertyInfo);
            if (_isWebSocketRequest == null)
                return;

            var acceptWebSocketRequestMethodInfo = httpContextBaseType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(methodInfo => methodInfo.Name == "AcceptWebSocketRequest");

            if (acceptWebSocketRequestMethodInfo == null)
                return;

            _acceptWebSocketRequest = Create(acceptWebSocketRequestMethodInfo);
            if (_acceptWebSocketRequest == null)
                return;

            var aspnetWebSocketContextType =
                systemWebDll.GetExportedTypes().FirstOrDefault(type => type.Name == "AspNetWebSocketContext");
            if (aspnetWebSocketContextType == null)
                return;

            var getWebSocketFromWebSocketContextPropertyInfo = aspnetWebSocketContextType.GetProperties()
                .FirstOrDefault(property => property.Name == "WebSocket" && property.CanRead);

            if (getWebSocketFromWebSocketContextPropertyInfo == null)
                return;

            _getWebSocketFromWebSocketContext = GetGetMethodByExpression<WebSocket>(getWebSocketFromWebSocketContextPropertyInfo);
        }

        public bool SupportsWebSockets
        {
            get { return _getWebSocketFromWebSocketContext != null; }
        }

        private static Action<object, object[]> Create(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(
              call,
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
              Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }

        private static Func<object, T> GetGetMethodByExpression<T>(PropertyInfo propertyInfo)
        {
            MethodInfo getMethodInfo = propertyInfo.GetGetMethod();
            ParameterExpression instance = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = (!propertyInfo.DeclaringType.IsValueType) ? Expression.TypeAs(instance, propertyInfo.DeclaringType) : Expression.Convert(instance, propertyInfo.DeclaringType);
            Func<object, object> compiled = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(instanceCast, getMethodInfo), typeof(object)), instance).Compile();
            return source => (T)compiled(source);
        }

        public static Func<AppFunc, AppFunc> Middleware(bool autodetect = true, bool replace = false, string httpContextBaseKey = "aspnet.HttpContextBase")
        {
            var ws = new AspNetWebSocketMiddleware(autodetect);

            if (!ws.SupportsWebSockets)
            {
                return app => app;
            }

            return app =>
                async env =>
                {
                    bool containsWebSocketSupport = env.ContainsKey("websocket.Support");
                    if (!replace && containsWebSocketSupport)
                    {
                        await app(env);
                        return;
                    }

                    var httpWebContext = Get<object>(env, httpContextBaseKey, null);
                    if (httpWebContext == null)
                    {
                        await app(env);
                        return;
                    }

                    if (ws._isWebSocketRequest(httpWebContext))
                        env["websocket.Support"] = "WebSocketFunc";

                    await app(env);

                    int responseStatusCode = Get<int>(env, "owin.ResponseStatusCode", 200);

                    object tempWsBodyDelegate;
                    if (responseStatusCode == 101 &&
                        env.TryGetValue("websocket.Func", out tempWsBodyDelegate) &&
                        tempWsBodyDelegate != null)
                    {
                        var wsBodyDelegate = (WebSocketFunc)tempWsBodyDelegate;

                        ws._acceptWebSocketRequest(httpWebContext,
                              new object[]
                                   {
                                       (Func<object, Task>)(async websocketContext =>
                                                                {
                                                                    var webSocket = ws._getWebSocketFromWebSocketContext(websocketContext);

                                                                    var wsEnv = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                                                                    wsEnv["websocket.SendAsyncFunc"] = WebSocketSendAsync(webSocket);
                                                                    wsEnv["websocket.ReceiveAsyncFunc"] = WebSocketReceiveAsync(webSocket);
                                                                    wsEnv["websocket.CloseAsyncFunc"] = WebSocketCloseAsync(webSocket);
                                                                    wsEnv["websocket.Version"] = "1.0";
                                                                    wsEnv["websocket.CallCancelled"] = CancellationToken.None;
                                                                    wsEnv["System.Web.WebSockets.AspNetWebSocketContext"] = websocketContext;

                                                                    await wsBodyDelegate(wsEnv);

                                                                    switch (webSocket.State)
                                                                    {
                                                                        case WebSocketState.Closed:  // closed gracefully, no action needed
                                                                        case WebSocketState.Aborted: // closed abortively, no action needed
                                                                            break;
                                                                        case WebSocketState.CloseReceived:
                                                                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                                                            break;
                                                                        case WebSocketState.Open:
                                                                        case WebSocketState.CloseSent: // No close received, abort so we don't have to drain the pipe.
                                                                            webSocket.Abort();
                                                                            break;
                                                                        default:
                                                                            throw new ArgumentOutOfRangeException("state", webSocket.State, string.Empty);
                                                                    }

                                                                    // todo close response
                                                                    //response.Close();
                                                                })
                                   });

                        if (containsWebSocketSupport)
                            env.Remove("websocket.Func");
                    }
                };
        }

        private static T Get<T>(IDictionary<string, object> env, string key, T defaultValue)
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : defaultValue;
        }

        private static WebSocketSendAsync WebSocketSendAsync(WebSocket webSocket)
        {
            return (buffer, messageType, endOfMessage, cancel) =>
                webSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        private static WebSocketReceiveAsync WebSocketReceiveAsync(WebSocket webSocket)
        {
            return async (buffer, cancel) =>
            {
                var nativeResult = await webSocket.ReceiveAsync(buffer, cancel);
                return new WebSocketReceiveTuple(
                    EnumToOpCode(nativeResult.MessageType),
                    nativeResult.EndOfMessage,
                    (nativeResult.MessageType == WebSocketMessageType.Close ? null : (int?)nativeResult.Count),
                    (int?)nativeResult.CloseStatus,
                    nativeResult.CloseStatusDescription);
            };
        }

        private static WebSocketCloseAsync WebSocketCloseAsync(WebSocket webSocket)
        {
            return (status, description, cancel) =>
                webSocket.CloseOutputAsync((WebSocketCloseStatus)status, description, cancel);
        }

        private static WebSocketMessageType OpCodeToEnum(int messageType)
        {
            switch (messageType)
            {
                case 0x1: return WebSocketMessageType.Text;
                case 0x2: return WebSocketMessageType.Binary;
                case 0x8: return WebSocketMessageType.Close;
                default:
                    throw new ArgumentOutOfRangeException("messageType", messageType, string.Empty);
            }
        }

        private static int EnumToOpCode(WebSocketMessageType webSocketMessageType)
        {
            switch (webSocketMessageType)
            {
                case WebSocketMessageType.Text: return 0x1;
                case WebSocketMessageType.Binary: return 0x2;
                case WebSocketMessageType.Close: return 0x8;
                default:
                    throw new ArgumentOutOfRangeException("webSocketMessageType", webSocketMessageType, string.Empty);
            }
        }
    }
}