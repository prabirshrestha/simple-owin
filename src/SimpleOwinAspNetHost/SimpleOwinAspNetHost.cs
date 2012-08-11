//#define ASPNET_WEBSOCKETS

namespace SimpleOwinAspNetHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if ASPNET_WEBSOCKETS
    using System.Net.WebSockets;
#endif
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Routing;

    using AppAction = System.Func< // Call
            System.Collections.Generic.IDictionary<string, object>, // Environment
            System.Collections.Generic.IDictionary<string, string[]>, // Headers
            System.IO.Stream, // Body
            System.Threading.Tasks.Task<System.Tuple< // Result
                System.Collections.Generic.IDictionary<string, object>, // Properties
                int, // Status
                System.Collections.Generic.IDictionary<string, string[]>, // HeadersB
                System.Func< // CopyTo
                    System.IO.Stream, // Body
                    System.Threading.Tasks.Task>>>>; // Done

    using ResultTuple = System.Tuple< //Result
        System.Collections.Generic.IDictionary<string, object>, // Properties
        int, // Status
        System.Collections.Generic.IDictionary<string, string[]>, // Headers
        System.Func< // CopyTo
            System.IO.Stream, // Body
            System.Threading.Tasks.Task>>; // Done

    using BodyAction = System.Func< // CopyTo
        System.IO.Stream, // Body
        System.Threading.Tasks.Task>; // Done

#if ASPNET_WEBSOCKETS

    using WebSocketSendAsync = System.Func<
                System.ArraySegment<byte>, // data
                int, // message type
                bool, // end of message
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task>;

    using WebSocketReceiveResultTuple = System.Tuple<
                        int, // messageType
                        bool, // endOfMessage
                        int?, // count
                        int?, // closeStatus
                        string>; // closeStatusDescription

    using WebSocketReceiveAsync = System.Func<
                System.ArraySegment<byte> /* data */,
                System.Threading.CancellationToken /* cancel */,
                System.Threading.Tasks.Task<
                    System.Tuple<
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

#pragma warning disable 811
    using WebSocketAction = System.Func<
            System.Func< // WebSocketSendAsync 
                System.ArraySegment<byte>, // data
                int, // message type
                bool, // end of message
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task>,
            System.Func< // WebSocketReceiveAsync
                System.ArraySegment<byte> /* data */,
                System.Threading.CancellationToken /* cancel */,
                System.Threading.Tasks.Task<
                    System.Tuple<
                        int, // messageType
                        bool, // endOfMessage
                        int?, // count
                        int?, // closeStatus
                        string>>>, // closeStatusDescription
             System.Func< // WebSocketCloseAsync
                int, // closeStatus
                string, // closeDescription
                System.Threading.CancellationToken, // cancel
                System.Threading.Tasks.Task>,
            System.Threading.Tasks.Task>; // complete
#pragma warning restore 811

#endif

    public class SimpleOwinAspNetRouteHandler : IRouteHandler
    {
        private readonly AppAction _app;

        public SimpleOwinAspNetRouteHandler(AppAction app)
        {
            _app = app;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new SimpleOwinAspNetHandler(_app);
        }
    }

    public class SimpleOwinAspNetHandler : IHttpAsyncHandler
    {
        private readonly AppAction _app;

#if ASPNET_WEBSOCKETS
        private System.Net.WebSockets.WebSocketContext _webSocketContext;
#endif

        public SimpleOwinAspNetHandler()
            : this(null)
        {
        }

        public SimpleOwinAspNetHandler(AppAction app)
        {
            if (app == null)
            {
                // get singleton app
                throw new NotImplementedException();
            }

            _app = app;
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new NotSupportedException();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object state)
        {
            return BeginProcessRequest(new HttpContextWrapper(context), callback, state);
        }

        public IAsyncResult BeginProcessRequest(HttpContextBase context, AsyncCallback callback, object state)
        {
            var tcs = new TaskCompletionSource<Action>(state);
            if (callback != null)
                tcs.Task.ContinueWith(task => callback(task), TaskContinuationOptions.ExecuteSynchronously);

            var request = context.Request;
            var response = context.Response;

            var pathBase = request.ApplicationPath;
            if (pathBase == "/" || pathBase == null)
                pathBase = "";

            var path = request.Path;
            if (path.StartsWith(pathBase))
                path = path.Substring(pathBase.Length);

            var serverVarsToAddToEnv = request.ServerVariables.AllKeys
                .Where(key => !key.StartsWith("HTTP_") && !string.Equals(key, "ALL_HTTP") && !string.Equals(key, "ALL_RAW"))
                .Select(key => new KeyValuePair<string, object>(key, request.ServerVariables.Get(key)));

            var env = new Dictionary<string, object>();
            env[OwinConstants.Version] = "1.0";
            env[OwinConstants.RequestMethod] = request.HttpMethod;
            env[OwinConstants.RequestScheme] = request.Url.Scheme;
            env[OwinConstants.RequestPathBase] = pathBase;
            env[OwinConstants.RequestPath] = path;
            env[OwinConstants.RequestQueryString] = request.ServerVariables["QUERY_STRING"];
            env[OwinConstants.RequestProtocol] = request.ServerVariables["SERVER_PROTOCOL"];
            env["aspnet.HttpContextBase"] = context;
            env[OwinConstants.CallCompleted] = tcs.Task;

#if ASPNET_WEBSOCKETS
            if (context.IsWebSocketRequest)
                env[OwinConstants.WebSocketSupport] = new[] { "WebSocket" };
#endif

            foreach (var kv in serverVarsToAddToEnv)
                env["server." + kv.Key] = kv.Value;

            var requestHeaders = request.Headers.AllKeys
                    .ToDictionary(x => x, x => request.Headers.GetValues(x), StringComparer.OrdinalIgnoreCase);

            var requestStream = request.InputStream;

            try
            {
                _app.Invoke(env, requestHeaders, requestStream)
                    .ContinueWith(taskResultParameters =>
                    {
                        if (taskResultParameters.IsFaulted)
                        {
                            tcs.TrySetException(taskResultParameters.Exception.InnerExceptions);
                        }
                        else if (taskResultParameters.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else
                        {
                            try
                            {
                                var resultParameters = taskResultParameters.Result;
                                var properties = resultParameters.Item1;
                                var responseStatus = resultParameters.Item2;
                                var responseHeader = resultParameters.Item3;
                                var responseCopyTo = resultParameters.Item4;

                                response.BufferOutput = false;
                                response.StatusCode = responseStatus;

                                object reasonPhrase;
                                if (properties.TryGetValue(OwinConstants.ReasonPhrase, out reasonPhrase))
                                    response.StatusDescription = Convert.ToString(reasonPhrase);

                                if (responseHeader != null)
                                {
                                    foreach (var header in responseHeader)
                                    {
                                        foreach (var headerValue in header.Value)
                                            response.AddHeader(header.Key, headerValue);
                                    }
                                }

#if ASPNET_WEBSOCKETS
                                object temp;
                                if (responseStatus == 101 &&
                                    properties.TryGetValue(OwinConstants.WebSocketBodyDelegte, out temp)
                                    && temp != null)
                                {
                                    var wsDelegate = (WebSocketAction)temp;
                                    context.AcceptWebSocketRequest(async websocketContext =>
                                    {
                                        _webSocketContext = websocketContext;
                                        env["aspnet.AspNetWebSocketContext"] = websocketContext;

                                        await wsDelegate(WebSocketSendAsync, WebSocketReceiveAsync, WebSocketCloseAsync);

                                        var webSocket = websocketContext.WebSocket;
                                        switch (webSocket.State)
                                        {
                                            case WebSocketState.Closed: // closed gracefully, no action needed
                                            case WebSocketState.Aborted: // closed abortively, no action needed
                                                break;
                                            case WebSocketState.CloseReceived:
                                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                                break;
                                            case WebSocketState.Open:
                                            case WebSocketState.CloseSent: // // No close received, abort so we don't have to drain the pipe.
                                                websocketContext.WebSocket.Abort();
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException("state", webSocket.State, string.Empty);
                                        }

                                        if (responseCopyTo != null)
                                            await responseCopyTo(response.OutputStream);

                                        response.Close();
                                    });

                                    tcs.TrySetResult(() => { });
                                }
                                else
#endif
                                    if (responseCopyTo != null)
                                    {
                                        responseCopyTo(response.OutputStream)
                                            .ContinueWith(taskCopyTo =>
                                            {
                                                if (taskResultParameters.IsFaulted)
                                                    tcs.TrySetException(taskResultParameters.Exception.InnerExceptions);
                                                else if (taskResultParameters.IsCanceled)
                                                    tcs.TrySetCanceled();
                                                else
                                                    tcs.TrySetResult(() => { });
                                            });
                                    }
                                    else
                                    {
                                        // if you reach here it means you didn't implmement AppAction correctly
                                        // tcs.TrySetResult(() => { });
                                    }
                            }
                            catch (Exception ex)
                            {
                                tcs.TrySetException(ex);
                            }
                        }
                    });
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        public void EndProcessRequest(IAsyncResult asyncResult)
        {
            var task = ((Task<Action>)asyncResult);
            if (task.IsFaulted)
            {
                var exception = task.Exception;
                exception.Handle(ex => ex is HttpException);
            }
            else if (task.IsCompleted)
            {
                task.Result.Invoke();
            }
        }

#if ASPNET_WEBSOCKETS

        private Task WebSocketSendAsync(ArraySegment<byte> buffer, int messageType, bool endOfMessage, CancellationToken cancel)
        {
            return _webSocketContext.WebSocket.SendAsync(buffer, OpCodeToEnum(messageType), endOfMessage, cancel);
        }

        private async Task<WebSocketReceiveResultTuple> WebSocketReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancel)
        {
            var nativeResult = await _webSocketContext.WebSocket.ReceiveAsync(buffer, cancel);
            return new WebSocketReceiveResultTuple(
                EnumToOpCode(nativeResult.MessageType),
                nativeResult.EndOfMessage,
                (nativeResult.MessageType == WebSocketMessageType.Close ? null : (int?)nativeResult.Count),
                (int?)nativeResult.CloseStatus,
                nativeResult.CloseStatusDescription);
        }

        private Task WebSocketCloseAsync(int status, string description, CancellationToken cancel)
        {
            return _webSocketContext.WebSocket.CloseOutputAsync((WebSocketCloseStatus)status, description, cancel);
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
#endif

        private static class OwinConstants
        {
            public const string Version = "owin.Version";
            public const string RequestScheme = "owin.RequestScheme";
            public const string RequestMethod = "owin.RequestMethod";
            public const string RequestPathBase = "owin.RequestPathBase";
            public const string RequestPath = "owin.RequestPath";
            public const string RequestQueryString = "owin.RequestQueryString";
            public const string RequestProtocol = "owin.RequestProtocol";
            public const string ReasonPhrase = "owin.ReasonPhrase";
            public const string CallCompleted = "owin.CallCompleted";

            public const string WebSocketSupport = "websocket.Support";
            public const string WebSocketBodyDelegte = "websocket.BodyFunc";
        }
    }
}