using System;
using System.Collections.Generic;
using System.Net.Http;
using WebAssembly.Net.Http.HttpClient; 
using System.Threading.Tasks;
using WebAssembly;
using System.Threading;
using System.IO;
using ClientWebSocket = WebAssembly.Net.WebSockets.ClientWebSocket;

namespace TestSuite
{
    public class Program
    {
		static ClientWebSocket cws;
		static CancellationTokenSource _cancellation;

		static async void CheckWebSocket (Uri server, string protocols = "")
		{
			if (cws == null) {

				cws = new ClientWebSocket ();

				if (!string.IsNullOrEmpty(protocols)) {

					foreach (var p in protocols.Split(';')) {
						cws.Options.AddSubProtocol (p);
					}

				}

			}

		}

		public async Task<object> ConnectWebSocket (Uri server, string protocols)
		{
			_cancellation = new CancellationTokenSource ();

			CheckWebSocket (server, protocols);
            var state = cws.State;
			using (var cts2 = new CancellationTokenSource (4000)) {

				try {
					Task taskConnect = cws.ConnectAsync (server, cts2.Token);
					await taskConnect;
				} catch (Exception exc) {
                    //return $"Close Status: [{cws.CloseStatus}] Description: [{cws.CloseStatusDescription}]";
                    //return $"{exc.Message} / {exc.InnerException.Message}";
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
