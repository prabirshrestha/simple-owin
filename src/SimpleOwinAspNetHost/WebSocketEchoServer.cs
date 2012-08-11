
// note: actually you don't need #if ASPNET_WEBSOCKETS in this file
// I added this so users trying in .net 4 or IIS7 can compile this project
// I will need to create a new project. yes i know i'm lazy.

namespace SimpleOwinAspNetHost
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.WebSockets;

    using AppAction = System.Func< // Call
        System.Collections.Generic.IDictionary<string, object>, // Environment
        System.Collections.Generic.IDictionary<string, string[]>, // Headers
        System.IO.Stream, // Body
        System.Threading.Tasks.Task<System.Tuple< // Result
            System.Collections.Generic.IDictionary<string, object>, // Properties
            int, // Status
            System.Collections.Generic.IDictionary<string, string[]>, // Headers
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

    using BodyAction = System.Func< // CopyToB
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

    public class WebSocketEchoServer
    {
        private static Task<ResultTuple> CachedCompletedResultTupleTask;

        static WebSocketEchoServer()
        {
            var tcs = new TaskCompletionSource<ResultTuple>();
            tcs.TrySetResult(null);
            CachedCompletedResultTupleTask = tcs.Task;
        }

        public Task<ResultTuple> OwinApp(IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            object version;
            environment.TryGetValue("owin.Version", out version);

            if (version == null || !string.Equals(version.ToString(), "1.0"))
                throw new InvalidOperationException("An OWIN v1.0 host is required");

            var owinRequestMethod = Get<string>(environment, "owin.RequestMethod");
            var owinRequestScheme = Get<string>(environment, "owin.RequestScheme");
            var owinRequestHeaders = headers;
            var owinRequestPathBase = Get<string>(environment, "owin.RequestPathBase");
            var owinRequestPath = Get<string>(environment, "owin.RequestPath");
            var owinRequestQueryString = Get<string>(environment, "owin.RequestQueryString");
            var serverClientIp = Get<string>(environment, "server.CLIENT_IP");
            var callCompleted = Get<Task>(environment, "owin.CallCompleted");

            var uriHostName = GetHeader(owinRequestHeaders, "Host");
            var uri = string.Format("{0}://{1}{2}{3}{4}{5}", owinRequestScheme, uriHostName, owinRequestPathBase, owinRequestPath, owinRequestQueryString == "" ? "" : "?", owinRequestQueryString);

            var tcs = new TaskCompletionSource<ResultTuple>();

            var owinResponseProperties = new Dictionary<string, object>();
            int owinResponseStatus;
            var owinResponseHeaders = new Dictionary<string, string[]>();
            BodyAction bodyAction = null;

#if ASPNET_WEBSOCKETS

            var webSocketSupport = Get<string[]>(environment, "websocket.Support");

            if (webSocketSupport != null && webSocketSupport.Contains("WebSocket"))
            {
                // supports web sockets
                owinResponseStatus = 101;
                WebSocketAction webSocketBody = async (sendAsync, receiveAsync, closeAsync) =>
                    {
                        var buffer = new ArraySegment<byte>(new byte[100]);

                        while (true)
                        {
                            WebSocketReceiveResultTuple webSocketResultTuple;
                            try
                            {
                                webSocketResultTuple = await receiveAsync(buffer, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                // call closeAsync?
                                break;
                            }

                            int wsMessageType = webSocketResultTuple.Item1;
                            bool wsEndOfMessge = webSocketResultTuple.Item2;
                            int? count = webSocketResultTuple.Item3;
                            int? closeStatus = webSocketResultTuple.Item4;
                            string closeStatusDescription = webSocketResultTuple.Item5;

                            Debug.Write(Encoding.UTF8.GetString(buffer.Array, 0, count.Value));

                            try
                            {
                                await sendAsync(new ArraySegment<byte>(buffer.Array, 0, count.Value),
                                        wsMessageType, wsEndOfMessge, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                                // call closeAsync?
                                break;
                            }

                            
                        }
                    };

                owinResponseProperties["websocket.BodyFunc"] = webSocketBody;
            }
            else
#endif
            {
                owinResponseStatus = 200;
                bodyAction = output =>
                    {
                        // callCompleted.Finally(context.Dispose);

                        // do the actual copying to response stream here
                        var message = Encoding.UTF8.GetBytes("hello from owin");
                        output.Write(message, 0, message.Length);

                        return CachedCompletedResultTupleTask;
                    };
            }

            var resultTuple = new ResultTuple(
                    owinResponseProperties,
                    owinResponseStatus,
                    owinResponseHeaders,
                    bodyAction);

            tcs.TrySetResult(resultTuple);

            return tcs.Task;
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

        private static long ExpectedLength(IDictionary<string, string[]> headers)
        {
            var header = GetHeader(headers, "Content-Length");
            if (string.IsNullOrWhiteSpace(header))
                return 0;

            int contentLength;
            return int.TryParse(header, NumberStyles.Any, CultureInfo.InvariantCulture, out contentLength) ? contentLength : 0;
        }

    }
}