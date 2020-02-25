using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebAssembly.Net.Debugging {

	// This type is the public entrypoint that allows external code to attach the debugger proxy
	// to a given websocket listener. Everything else in this package can be internal.

	public class DebuggerProxy : IDisposable {
		private readonly ILoggerFactory loggerFactory;
		private readonly MonoProxy proxy;

		public DebuggerProxy () {
			loggerFactory = LoggerFactory.Create(
				builder => builder.AddConsole().AddFilter(null, LogLevel.Trace));
			proxy = new MonoProxy(loggerFactory);
		}

		public Task Run (Uri browserUri, WebSocket ideSocket) {
			return proxy.Run (browserUri, ideSocket);
		}

		void IDisposable.Dispose() {
			loggerFactory.Dispose();
		}
	}
}
