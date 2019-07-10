using System;
using System.Threading.Tasks;
using System.Threading;
using ClientWebSocket = WebAssembly.Net.WebSockets.ClientWebSocket;
using System.Net.WebSockets;
using System.Text;

namespace TestSuite
{
    public class Program
    {
        static ClientWebSocket CreateWebSocket(Uri server, string protocols = "")
        {
            var cws = new ClientWebSocket();

            if (!string.IsNullOrEmpty(protocols))
            {

                foreach (var p in protocols.Split(';'))
                {
                    cws.Options.AddSubProtocol(p);
                }

            }

            return cws;

        }
        public async Task<int> ConnectWebSocketStatus(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            WebSocketCloseStatus status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;

            try
            {
                Task taskConnect = cws.ConnectAsync(server, CancellationToken.None);
                await taskConnect;
            }
            catch (Exception exc)
            {
                Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
            }
            finally
            {
                status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;
                cws = null;
            }
            return (int)status;
        }

        public async Task<int> ConnectWebSocketStatusWithToken(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            WebSocketCloseStatus status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;
            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;
                    cws = null;
                }
            }
            return (int)status;
        }

        public async Task<WebSocketState> OpenWebSocket(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            var state = cws.State;
            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    state = cws.State;
                    cws = null;
                }
            }
            return state;
        }

        public async Task<WebSocketState> CloseWebSocket(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            var state = cws.State;
            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                    if (cws.State == WebSocketState.Open)
                        await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Hic sunt Dracones!!", CancellationToken.None);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    state = cws.State;
                    cws = null;
                }
            }
            return state;
        }

        public async Task<WebSocketState> RecieveHostCloseWebSocket(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            var state = cws.State;
            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                    if (cws.State == WebSocketState.Open)
                    {
                        var sndBuffer = Encoding.UTF8.GetBytes("closeme");
                        await cws.SendAsync(new ArraySegment<byte>(sndBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        var rcvBuffer = new ArraySegment<byte>(new byte[4096]);
                        var r = await cws.ReceiveAsync(rcvBuffer, CancellationToken.None);
                    }

                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    state = cws.State;
                    cws = null;
                }
            }
            return state;
        }

        public async Task<string> CloseStatusDescCloseWebSocket(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            var description = $"{{ \"code\": \"{cws.CloseStatus}\", \"desc\": \"{cws.CloseStatusDescription}\" }}";
            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                    if (cws.State == WebSocketState.Open)
                    {
                        var sndBuffer = Encoding.UTF8.GetBytes("closeme");
                        await cws.SendAsync(new ArraySegment<byte>(sndBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        var rcvBuffer = new ArraySegment<byte>(new byte[4096]);
                        var r = await cws.ReceiveAsync(rcvBuffer, CancellationToken.None);
                    }

                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    description = $"{{ \"code\": \"{cws.CloseStatus}\", \"desc\": \"{cws.CloseStatusDescription}\" }}";
                    cws = null;
                }
            }
            return description;
        }

        public async Task<string> WebSocketSendText(Uri server, string protocols, string text)
        {
            var cws = CreateWebSocket(server, protocols);

            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                    if (cws.State == WebSocketState.Open)
                    {
                        var sndBuffer = Encoding.UTF8.GetBytes(text);
                        await cws.SendAsync(new ArraySegment<byte>(sndBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        var rcvBuffer = new ArraySegment<byte>(new byte[4096]);
                        var r = await cws.ReceiveAsync(rcvBuffer, CancellationToken.None);
                        return Encoding.UTF8.GetString(rcvBuffer.Array, rcvBuffer.Offset, r.Count);
                    }

                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    cws = null;
                }
            }
            return "SomethingWentWrong";
        }

        public async Task<string> WebSocketSendBinary(Uri server, string protocols, string text)
        {
            var cws = CreateWebSocket(server, protocols);

            using (var cts2 = new CancellationTokenSource(500))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                    if (cws.State == WebSocketState.Open)
                    {
                        var sndBuffer = Encoding.UTF8.GetBytes(text);
                        await cws.SendAsync(new ArraySegment<byte>(sndBuffer), WebSocketMessageType.Binary, true, CancellationToken.None);
                        var rcvBuffer = new ArraySegment<byte>(new byte[4096]);
                        var r = await cws.ReceiveAsync(rcvBuffer, CancellationToken.None);
                        return Encoding.UTF8.GetString(rcvBuffer.Array, rcvBuffer.Offset, r.Count);
                    }

                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    cws = null;
                }
            }
            return "SomethingWentWrong";
        }

        public async Task<WebSocketState> ConnectWebSocket(Uri server, string protocols)
        {
            var cws = CreateWebSocket(server, protocols);

            var state = cws.State;
            using (var cts2 = new CancellationTokenSource(4000))
            {

                try
                {
                    Task taskConnect = cws.ConnectAsync(server, cts2.Token);
                    await taskConnect;
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
                }
                finally
                {
                    state = cws.State;
                    cws = null;
                }
            }
            return state;
        }

    }
}
