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
		const string Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		const string VersionTag = "13";

		ClientWebSocketOptions options;
		WebSocketState state;
		string subProtocol;

		HttpWebRequest req;
		WebConnection connection;
		Socket underlyingSocket;

		Random random = new Random ();

		const int HeaderMaxLength = 14;
		byte[] headerBuffer;
		byte[] sendBuffer;
		long remaining;

		public ClientWebSocket ()
		{
			options = new ClientWebSocketOptions ();
			state = WebSocketState.None;
			headerBuffer = new byte[HeaderMaxLength];
		}

		public override void Dispose ()
		{
			if (connection != null)
				connection.Close (false);
		}

		[MonoTODO]
		public override void Abort ()
		{
			throw new NotImplementedException ();
		}

		public ClientWebSocketOptions Options {
			get {
				return options;
			}
		}

		public override WebSocketState State {
			get {
				return state;
			}
		}

		public override WebSocketCloseStatus? CloseStatus {
			get {
				if (state != WebSocketState.Closed)
					return (WebSocketCloseStatus?)null;
				return WebSocketCloseStatus.Empty;
			}
		}

		public override string CloseStatusDescription {
			get {
				return null;
			}
		}

		public override string SubProtocol {
			get {
				return subProtocol;
			}
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
			string expectedAccept = Convert.ToBase64String (SHA1.Create ().ComputeHash (Encoding.ASCII.GetBytes (secKey + Magic)));

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

			connection = req.StoredConnection;
			underlyingSocket = connection.socket;

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
		}

		public override Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			ValidateArraySegment (buffer);
			if (connection == null)
				throw new WebSocketException (WebSocketError.Faulted);
			var count = Math.Max (options.SendBufferSize, buffer.Count) + HeaderMaxLength;
			if (sendBuffer == null || sendBuffer.Length != count)
				sendBuffer = new byte[count];
			return Task.Run (() => {
				EnsureWebSocketState (WebSocketState.Open, WebSocketState.CloseReceived);
				var maskOffset = WriteHeader (messageType, buffer, endOfMessage);

				if (buffer.Count > 0)
					MaskData (buffer, maskOffset);
				//underlyingSocket.Send (headerBuffer, 0, maskOffset + 4, SocketFlags.None);
				var headerLength = maskOffset + 4;
				Array.Copy (headerBuffer, sendBuffer, headerLength);
				underlyingSocket.Send (sendBuffer, 0, buffer.Count + headerLength, SocketFlags.None);
			});
		}
		
		const int messageTypeText = 1;
		const int messageTypeBinary = 2;
		const int messageTypeClose = 8;

		static WebSocketMessageType WireToMessageType (byte msgType)
		{
			
			if (msgType == messageTypeText)
				return WebSocketMessageType.Text;
			if (msgType == messageTypeBinary)
				return WebSocketMessageType.Binary;
			return WebSocketMessageType.Close;
		}

		static byte MessageTypeToWire (WebSocketMessageType type)
		{
			if (type == WebSocketMessageType.Text)
				return messageTypeText;
			if (type == WebSocketMessageType.Binary)
				return messageTypeBinary;
			return messageTypeClose;
		}
		
		public override Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			ValidateArraySegment (buffer);
			return Task.Run (() => {
				EnsureWebSocketState (WebSocketState.Open, WebSocketState.CloseSent);

				bool isLast;
				WebSocketMessageType type;
				long length;

				if (remaining == 0) {
					// First read the two first bytes to know what we are doing next
					connection.Read (req, headerBuffer, 0, 2);
					isLast = (headerBuffer[0] >> 7) > 0;
					var isMasked = (headerBuffer[1] >> 7) > 0;
					int mask = 0;
					type = WireToMessageType ((byte)(headerBuffer[0] & 0xF));
					length = headerBuffer[1] & 0x7F;
					int offset = 0;
					if (length == 126) {
						offset = 2;
						connection.Read (req, headerBuffer, 2, offset);
					length = (headerBuffer[2] << 8) | headerBuffer[3];
					} else if (length == 127) {
						offset = 8;
						connection.Read (req, headerBuffer, 2, offset);
						length = 0;
						for (int i = 2; i <= 9; i++)
							length = (length << 8) | headerBuffer[i];
					}

					if (isMasked) {
						connection.Read (req, headerBuffer, 2 + offset, 4);
						for (int i = 0; i < 4; i++) {
							var pos = i + offset + 2;
							mask = (mask << 8) | headerBuffer[pos];
						}
					}
				} else {
					isLast = (headerBuffer[0] >> 7) > 0;
					type = WireToMessageType ((byte)(headerBuffer[0] & 0xF));
					length = remaining;
				}

				if (type == WebSocketMessageType.Close) {
					state = WebSocketState.Closed;
					var tmpBuffer = new byte[length];
					connection.Read (req, tmpBuffer, 0, tmpBuffer.Length);
					var closeStatus = (WebSocketCloseStatus)(tmpBuffer[0] << 8 | tmpBuffer[1]);
					var closeDesc = tmpBuffer.Length > 2 ? Encoding.UTF8.GetString (tmpBuffer, 2, tmpBuffer.Length - 2) : string.Empty;
					return new WebSocketReceiveResult ((int)length, type, isLast, closeStatus, closeDesc);
				} else {
					var readLength = (int)(buffer.Count < length ? buffer.Count : length);
					connection.Read (req, buffer.Array, buffer.Offset, readLength);
					remaining = length - readLength;

					return new WebSocketReceiveResult ((int)readLength, type, isLast && remaining == 0);
				}
			});
		}

		// The damn difference between those two methods is that CloseAsync will wait for server acknowledgement before completing
		// while CloseOutputAsync will send the close packet and simply complete.

		public async override Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			await SendCloseFrame (closeStatus, statusDescription, cancellationToken).ConfigureAwait (false);
			state = WebSocketState.CloseSent;
			// TODO: figure what's exceptions are thrown if the server returns something faulty here
			await ReceiveAsync (new ArraySegment<byte> (new byte[0]), cancellationToken).ConfigureAwait (false);
			state = WebSocketState.Closed;
		}

		public async override Task CloseOutputAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			await SendCloseFrame (closeStatus, statusDescription, cancellationToken).ConfigureAwait (false);
			state = WebSocketState.CloseSent;
		}

		async Task SendCloseFrame (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			var statusDescBuffer = string.IsNullOrEmpty (statusDescription) ? new byte[2] : new byte[2 + Encoding.UTF8.GetByteCount (statusDescription)];
			statusDescBuffer[0] = (byte)(((ushort)closeStatus) >> 8);
			statusDescBuffer[1] = (byte)(((ushort)closeStatus) & 0xFF);
			if (!string.IsNullOrEmpty (statusDescription))
				Encoding.UTF8.GetBytes (statusDescription, 0, statusDescription.Length, statusDescBuffer, 2);
			await SendAsync (new ArraySegment<byte> (statusDescBuffer), WebSocketMessageType.Close, true, cancellationToken).ConfigureAwait (false);
		}

		int WriteHeader (WebSocketMessageType type, ArraySegment<byte> buffer, bool endOfMessage)
		{
			var opCode = MessageTypeToWire (type);
			var length = buffer.Count;

			headerBuffer[0] = (byte)(opCode | (endOfMessage ? 0x80 : 0));
			if (length < 126) {
				headerBuffer[1] = (byte)length;
			} else if (length <= ushort.MaxValue) {
				headerBuffer[1] = (byte)126;
				headerBuffer[2] = (byte)(length / 256);
				headerBuffer[3] = (byte)(length % 256);
			} else {
				headerBuffer[1] = (byte)127;

				int left = length;
				int unit = 256;

				for (int i = 9; i > 1; i--) {
					headerBuffer[i] = (byte)(left % unit);
					left = left / unit;
				}
			}

			var l = Math.Max (0, headerBuffer[1] - 125);
			var maskOffset = 2 + l * l * 2;
			GenerateMask (headerBuffer, maskOffset);

			// Since we are client only, we always mask the payload
			headerBuffer[1] |= 0x80;

			return maskOffset;
		}

		void GenerateMask (byte[] mask, int offset)
		{
			mask[offset + 0] = (byte)random.Next (0, 255);
			mask[offset + 1] = (byte)random.Next (0, 255);
			mask[offset + 2] = (byte)random.Next (0, 255);
			mask[offset + 3] = (byte)random.Next (0, 255);
		}

		void MaskData (ArraySegment<byte> buffer, int maskOffset)
		{
			var sendBufferOffset = maskOffset + 4;
			for (var i = 0; i < buffer.Count; i++)
				sendBuffer[i + sendBufferOffset] = (byte)(buffer.Array[buffer.Offset + i] ^ headerBuffer[maskOffset + (i % 4)]);
		}

		void EnsureWebSocketConnected ()
		{
			if (state < WebSocketState.Open)
				throw new InvalidOperationException ("The WebSocket is not connected");
		}

		void EnsureWebSocketState (params WebSocketState[] validStates)
		{
			foreach (var validState in validStates)
				if (state == validState)
					return;
			throw new WebSocketException ("The WebSocket is in an invalid state ('" + state + "') for this operation. Valid states are: " + string.Join (", ", validStates));
		}

		void ValidateArraySegment (ArraySegment<byte> segment)
		{
			if (segment.Array == null)
				throw new ArgumentNullException ("buffer.Array");
			if (segment.Offset < 0)
				throw new ArgumentOutOfRangeException ("buffer.Offset");
			if (segment.Offset + segment.Count > segment.Array.Length)
				throw new ArgumentOutOfRangeException ("buffer.Count");
		}
	}
}

