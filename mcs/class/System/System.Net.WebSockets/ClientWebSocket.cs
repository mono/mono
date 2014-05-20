//
// ClientWebSocket.cs
//
// Authors:
//	  Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2013 Xamarin Inc (http://www.xamarin.com).
//
// Lightly inspired from WebSocket4Net distributed under the Apache License 2.0
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_5

using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;

namespace System.Net.WebSockets
{
	public class ClientWebSocket : WebSocket, IDisposable
	{
		const string VersionTag = "13";

		ClientWebSocketOptions options;
		WebSocketState state;
		string subProtocol;

		HttpWebRequest req;
		WebConnection connection;
		StreamWebSocket internalWebSocket;

		public ClientWebSocket ()
		{
			options = new ClientWebSocketOptions ();
			state = WebSocketState.None;
		}

		public override void Dispose ()
		{
			if (internalWebSocket != null)
				internalWebSocket.Dispose ();
		}

		public override void Abort ()
		{
			if (internalWebSocket != null)
				internalWebSocket.Abort ();
		}

		public ClientWebSocketOptions Options {
			get {
				return options;
			}
		}

		public override WebSocketState State {
			get {
				if (internalWebSocket != null)
					return internalWebSocket.State;
				return state;
			}
		}

		public override WebSocketCloseStatus? CloseStatus {
			get {
				if (internalWebSocket != null)
					return internalWebSocket.CloseStatus;
				if (state != WebSocketState.Closed)
					return (WebSocketCloseStatus?)null;
				return WebSocketCloseStatus.Empty;
			}
		}

		[MonoTODO]
		public override string CloseStatusDescription {
			get { return null; }
		}

		public override string SubProtocol {
			get { return subProtocol; }
		}

		public async Task ConnectAsync (Uri uri, CancellationToken cancellationToken)
		{
			state = WebSocketState.Connecting;
			var httpUri = new UriBuilder (uri);
			if (uri.Scheme == "wss")
				httpUri.Scheme = "https";
			else
				httpUri.Scheme = "http";
			req = (HttpWebRequest)WebRequest.Create (httpUri.Uri);
			req.ReuseConnection = true;
			if (options.Cookies != null)
				req.CookieContainer = options.Cookies;

			if (options.CustomRequestHeaders.Count > 0) {
				foreach (var header in options.CustomRequestHeaders)
					req.Headers[header.Key] = header.Value;
			}

			var secKey = Convert.ToBase64String (Encoding.ASCII.GetBytes (Guid.NewGuid ().ToString ().Substring (0, 16)));
			string expectedAccept = StreamWebSocket.CreateAcceptKey (secKey);

			req.Headers["Upgrade"] = "WebSocket";
			req.Headers["Sec-WebSocket-Version"] = VersionTag;
			req.Headers["Sec-WebSocket-Key"] = secKey;
			req.Headers["Sec-WebSocket-Origin"] = uri.Host;
			if (options.SubProtocols.Count > 0)
				req.Headers["Sec-WebSocket-Protocol"] = string.Join (",", options.SubProtocols);

			if (options.Credentials != null)
				req.Credentials = options.Credentials;
			if (options.ClientCertificates != null)
				req.ClientCertificates = options.ClientCertificates;
			if (options.Proxy != null)
				req.Proxy = options.Proxy;
			req.UseDefaultCredentials = options.UseDefaultCredentials;
			req.Connection = "Upgrade";

			HttpWebResponse resp = null;
			try {
				resp = (HttpWebResponse)(await req.GetResponseAsync ().ConfigureAwait (false));
			} catch (Exception e) {
				throw new WebSocketException (WebSocketError.Success, e);
			}

			if (resp.StatusCode != HttpStatusCode.SwitchingProtocols)
				throw new WebSocketException ("The server returned status code '" + (int)resp.StatusCode + "' when status code '101' was expected");
			if (!string.Equals (resp.Headers["Upgrade"], "WebSocket", StringComparison.OrdinalIgnoreCase)
				|| !string.Equals (resp.Headers["Connection"], "Upgrade", StringComparison.OrdinalIgnoreCase)
				|| !string.Equals (resp.Headers["Sec-WebSocket-Accept"], expectedAccept))
				throw new WebSocketException ("HTTP header error during handshake");
			if (resp.Headers["Sec-WebSocket-Protocol"] != null) {
				if (!options.SubProtocols.Contains (resp.Headers["Sec-WebSocket-Protocol"]))
					throw new WebSocketException (WebSocketError.UnsupportedProtocol);
				subProtocol = resp.Headers["Sec-WebSocket-Protocol"];
			}
			state = WebSocketState.Open;
			connection = req.StoredConnection;
			internalWebSocket = new StreamWebSocket(connection.nstream, connection.nstream, connection.socket, subProtocol, true, new ArraySegment<byte>(new byte[0]));
		}

		public override Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			return internalWebSocket.SendAsync (buffer, messageType, endOfMessage, cancellationToken);
		}

		public override Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			return internalWebSocket.ReceiveAsync (buffer, cancellationToken);
		}

		// The damn difference between those two methods is that CloseAsync will wait for server acknowledgement before completing
		// while CloseOutputAsync will send the close packet and simply complete.

		public override Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			return internalWebSocket.CloseAsync (closeStatus, statusDescription, cancellationToken);
		}

		public override Task CloseOutputAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			return internalWebSocket.CloseOutputAsync (closeStatus, statusDescription, cancellationToken);
		}
	}
}

#endif
