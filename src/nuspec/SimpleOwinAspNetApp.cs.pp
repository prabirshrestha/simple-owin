[assembly: System.Web.PreApplicationStartMethod(
    typeof($rootnamespace$.App_Start.SimpleOwinAspNetApp), "Initialize")]

namespace $rootnamespace$.App_Start
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Routing;
    using $rootnamespace$.SimpleOwin.Hosts;

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

    public static class SimpleOwinAspNetApp
    {
        public static void Initialize()
        {
            var startupEnv = SimpleOwinAspNetHandler.GetStartupProperties();

            var app = HelloWorldOwinApp.App(startupEnv);

            RouteTable.Routes.Add(new Route("{*pathInfo}", new SimpleOwinAspNetRouteHandler(app)));
        }
    }

    public class HelloWorldOwinApp
    {
        public static AppFunc App(StartupEnv startupEnv = null)
        {
            return
                env =>
                {
                    var resBody = (Stream)env["owin.ResponseBody"];

                    var data = Encoding.UTF8.GetBytes("hello world!");
                    resBody.Write(data, 0, data.Length);

                    var tcs = new TaskCompletionSource<int>();
                    tcs.TrySetResult(0);
                    return tcs.Task;
                };
        }
    }
}