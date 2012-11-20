
namespace SimpleOwinAspNetHost.Samples.WebSockets.Helloworld
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    using WebSocketAccept = System.Action<
            System.Collections.Generic.IDictionary<string, object>, // WebSocket Accept parameters
            System.Func< // WebSocketFunc callback
                System.Collections.Generic.IDictionary<string, object>, // WebSocket environment
                System.Threading.Tasks.Task>>;

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

    public class HelloWorldWebSocket
    {
        private static readonly Task CachedCompletedResultTupleTask;

        static HelloWorldWebSocket()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(0);
            CachedCompletedResultTupleTask = tcs.Task;
        }

        public static AppFunc OwinApp()
        {
            return env =>
            {
                var responseBody = (Stream)env["owin.ResponseBody"];

                object temp;
                if (env.TryGetValue("websocket.Accept", out temp) && temp != null)
                {
                    var wsAccept = (WebSocketAccept)temp;
                    var requestHeaders = Get<IDictionary<string, string[]>>(env, "owin.RequestHeaders");

                    Dictionary<string, object> acceptOptions = null;
                    string[] subProtocols;
                    if (requestHeaders.TryGetValue("Sec-WebSocket-Protocol", out subProtocols) && subProtocols.Length > 0)
                    {
                        acceptOptions = new Dictionary<string, object>();
                        // Select the first one from the client
                        acceptOptions.Add("websocket.SubProtocol", subProtocols[0].Split(',').First().Trim());
                    }

                    wsAccept(acceptOptions, async wsEnv =>
                                                {
                                                    var wsSendAsync = (WebSocketSendAsync)wsEnv["websocket.SendAsync"];
                                                    var wsRecieveAsync = (WebSocketReceiveAsync)wsEnv["websocket.ReceiveAsync"];
                                                    var wsCloseAsync = (WebSocketCloseAsync)wsEnv["websocket.CloseAsync"];
                                                    var wsVersion = (string)wsEnv["websocket.Version"];
                                                    var wsCallCancelled = (CancellationToken)wsEnv["websocket.CallCancelled"];

                                                    // note: make sure to catch errors when calling sendAsync, receiveAsync and closeAsync
                                                    // for simiplicity this code does not handle errors
                                                    var buffer = new ArraySegment<byte>(new byte[6]);
                                                    while (true)
                                                    {
                                                        var webSocketResultTuple = await wsRecieveAsync(buffer, wsCallCancelled);
                                                        int wsMessageType = webSocketResultTuple.Item1;
                                                        bool wsEndOfMessge = webSocketResultTuple.Item2;
                                                        int? count = webSocketResultTuple.Item3;
                                                        int? closeStatus = webSocketResultTuple.Item4;
                                                        string closeStatusDescription = webSocketResultTuple.Item5;

                                                        Debug.Write(Encoding.UTF8.GetString(buffer.Array, 0, count.Value));

                                                        await wsSendAsync(new ArraySegment<byte>(buffer.ToArray(), 0, count.Value), 1, wsEndOfMessge, wsCallCancelled);

                                                        if (wsEndOfMessge)
                                                            break;
                                                    }

                                                    await wsCloseAsync((int)WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                                                });
                }
                else
                {
                    env["owin.ResponseStatusCode"] = 200;
                    // do the actual copying to response stream here
                    var message = Encoding.UTF8.GetBytes("hello from owin");
                    responseBody.Write(message, 0, message.Length);
                }

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