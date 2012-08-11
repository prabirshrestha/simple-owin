using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;

namespace SimpleOwinAspNetHost
{
    /// <summary>
    /// Summary description for WebSocketEchoServerPureHttpHandler
    /// </summary>
    /// <remarks>http://evolpin.wordpress.com/2012/02/17/html5-websockets-revolution/</remarks>
    public class WebSocketEchoServerPureHttpHandler : IHttpHandler
    {
        // list of client WebSockets that are open
        private static readonly IList<WebSocket> Clients = new List<WebSocket>();

        // ensure thread-safety of the WebSocket clients
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        public void ProcessRequest(HttpContext context)
        {
            ProcessRequest(new HttpContextWrapper(context));
        }

        public void ProcessRequest(HttpContextBase context)
        {
            if (context.IsWebSocketRequest)
                context.AcceptWebSocketRequest(ProcessSocketRequest);
        }

        private async Task ProcessSocketRequest(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;

            // add socket to socket list
            Locker.EnterWriteLock();
            try
            {
                Clients.Add(socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }

            var buffer = new ArraySegment<byte>(new byte[1024]);

            // maintain socket
            while (true)
            {
                WebSocketReceiveResult result;

                try
                {
                    // async wait for a change in the socket
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    // client is no longer available - delete from list
                    RemoveClient(socket);
                    break;
                }

                if (socket.State == WebSocketState.Open)
                {
                    if (result.Count == 0)
                        continue;

                    var bufferToSend = new ArraySegment<byte>(buffer.Array, 0, result.Count);

                    // echo to all clients
                    foreach (var client in Clients)
                    {
                        try
                        {
                            await client.SendAsync(bufferToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                            // client is no longer available - delete from list
                            RemoveClient(client);
                            continue;  // try sending to the next client
                        }
                    }
                }
                else
                {
                    // client is no longer available - delete from list
                    RemoveClient(socket);
                    break;
                }
            }
        }

        private void RemoveClient(WebSocket socket)
        {
            Locker.EnterWriteLock();
            try
            {
                Clients.Remove(socket);
            }
            finally
            {
                Locker.ExitWriteLock();
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}