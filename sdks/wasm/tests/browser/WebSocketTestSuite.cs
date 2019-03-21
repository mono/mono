using System;
using System.Collections.Generic;
using System.Net.Http;
using WebAssembly.Net.Http.HttpClient; 
using System.Threading.Tasks;
using WebAssembly;
using System.Threading;
using System.IO;
using ClientWebSocket = WebAssembly.Net.WebSockets.ClientWebSocket;
using System.Net.WebSockets;

namespace TestSuite
{
    public class Program
    {
		static ClientWebSocket CreateWebSocket (Uri server, string protocols = "")
		{
			var cws = new ClientWebSocket ();

			if (!string.IsNullOrEmpty(protocols)) {

				foreach (var p in protocols.Split(';')) {
					cws.Options.AddSubProtocol (p);
				}

			}

			return cws;

		}
		public async Task<int> ConnectWebSocketStatus (Uri server, string protocols)
		{
			var cws = CreateWebSocket (server, protocols);

            WebSocketCloseStatus status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;

			try {
				Task taskConnect = cws.ConnectAsync (server, CancellationToken.None);
				await taskConnect;
			} catch (Exception exc) {
				Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
			}
			finally {
				status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;
				cws = null;
			}
            return (int)status;
		}

		public async Task<int> ConnectWebSocketStatusWithToken (Uri server, string protocols)
		{
			var cws = CreateWebSocket (server, protocols);

            WebSocketCloseStatus status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;
			using (var cts2 = new CancellationTokenSource (500)) {

				try {
					Task taskConnect = cws.ConnectAsync (server, cts2.Token);
					await taskConnect;
				} catch (Exception exc) {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
				}
                finally {
                    status = cws.CloseStatus ?? WebSocketCloseStatus.Empty;
                    cws = null;
                }
			}
            return (int)status;
		}

		public async Task<WebSocketState> OpenWebSocket (Uri server, string protocols)
		{
			var cws = CreateWebSocket (server, protocols);

            var state = cws.State;
			using (var cts2 = new CancellationTokenSource (500)) {

				try {
					Task taskConnect = cws.ConnectAsync (server, cts2.Token);
					await taskConnect;
				} catch (Exception exc) {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
				}
                finally {
                    state = cws.State;
                    cws = null;
                }
			}
            return state;
		}

		public async Task<WebSocketState> CloseWebSocket (Uri server, string protocols)
		{
			var cws = CreateWebSocket (server, protocols);

            var state = cws.State;
			using (var cts2 = new CancellationTokenSource (500)) {

				try {
					Task taskConnect = cws.ConnectAsync (server, cts2.Token);
					await taskConnect;
					if (cws.State == WebSocketState.Open)
						await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Hic sunt Dracones!!", CancellationToken.None);
				} catch (Exception exc) {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
				}
                finally {
                    state = cws.State;
                    cws = null;
                }
			}
            return state;
		}

		public async Task<WebSocketState> ConnectWebSocket (Uri server, string protocols)
		{
			var cws = CreateWebSocket(server, protocols);

            var state = cws.State;
			using (var cts2 = new CancellationTokenSource (4000)) {

				try {
					Task taskConnect = cws.ConnectAsync (server, cts2.Token);
					await taskConnect;
				} catch (Exception exc) {
                    Console.WriteLine($"{exc.Message} / {exc.InnerException.Message}");
				}
                finally {
                    state = cws.State;
                    cws = null;
                }
			}
            return state;
		}

    }
}
