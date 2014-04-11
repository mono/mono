//
// StreamWebSocket.cs
//
// Authors:
//	  Jérémie Laval <jeremie dot laval at xamarin dot com>
//    INADA Naoki <songofacandy at gmail dot com>
//
// Copyright 2013-2014 Xamarin Inc (http://www.xamarin.com).
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace System.Net.WebSockets {
	internal class StreamWebSocket : WebSocket, IDisposable {
		Stream rstream;
		Stream wstream;
		Socket socket;
		// Data received while handshake.
		ArraySegment<byte> preloaded;

		WebSocketState state;
		WebSocketCloseStatus closeStatus = WebSocketCloseStatus.Empty;
		Stream writeStream;
		Stream readStream;
		Random random;
		string subProtocol;

		bool maskSend;
		const int HeaderMaxLength = 14;
		byte[] headerBuffer;
		byte[] sendBuffer;

		public StreamWebSocket (Stream rstream, Stream wstream, Socket socket, string subProtocol, bool maskSend, ArraySegment<byte> preloaded)
		{
			this.rstream = rstream;
			this.wstream = wstream;
			// Necessary when both of rstream and wstream doesn't close socket.
			this.socket = socket;
			this.subProtocol = subProtocol;
			this.maskSend = maskSend;
			if (maskSend) {
				random = new Random ();
			}
			this.preloaded = preloaded;
			state = WebSocketState.Open;
			headerBuffer = new byte[HeaderMaxLength];
		}

		public override void Dispose ()
		{
			if (state != WebSocketState.Closed) {
				state = WebSocketState.Aborted;
			}
			if (readStream != null) {
				readStream.Dispose ();
				if (writeStream != null && ! Object.ReferenceEquals (readStream, writeStream)) {
					writeStream.Dispose ();
				}
			}
			readStream = writeStream = null;
			if (socket != null) {
				socket.Dispose ();
				socket = null;
			}
		}

		public override WebSocketState State {
			get { return state; }
		}

		public override string SubProtocol {
			get { return subProtocol; }
		}

		public override WebSocketCloseStatus? CloseStatus
		{
			get {
				if (state != WebSocketState.Closed)
					return null;
				return closeStatus;
			}
		}

		[MonoTODO]
		public override string CloseStatusDescription
		{
			get { return null; }
		}

		public override void Abort ()
		{
			Dispose ();
		}

		public override Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			EnsureWebSocketState (WebSocketState.Open, WebSocketState.CloseReceived);
			ValidateArraySegment (buffer);
			if (wstream == null)
				throw new WebSocketException (WebSocketError.Faulted);

			var count = Math.Max (16 * 1024, buffer.Count) + HeaderMaxLength;
			if (sendBuffer == null || sendBuffer.Length < count)
				sendBuffer = new byte[count];

			int headerSize = WriteHeader (messageType, buffer.Count, endOfMessage);
			int frameSize;
			if (maskSend) {
				MaskData (headerSize, buffer);
				frameSize = headerSize + buffer.Count + 4;
			} else {
				Array.Copy (buffer.Array, buffer.Offset, sendBuffer, headerSize, buffer.Count);
				frameSize = headerSize + buffer.Count;
			}
			try {
				return wstream.WriteAsync (sendBuffer, 0, frameSize, cancellationToken);
			} catch (IOException e) {
				InnerClose ();
				closeStatus = WebSocketCloseStatus.EndpointUnavailable;
				throw e;
			}
		}

		private bool isLast;
		private bool isMasked;
		private WebSocketMessageType receivedType;
		private int remainOffset;
		private int remainBytes = 0;
		private byte[] mask = new byte[4];

		public override async Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			ValidateArraySegment (buffer);
			EnsureWebSocketState (WebSocketState.Open, WebSocketState.CloseSent);

			if (remainBytes > 0) {
				var readLength = (int)(buffer.Count < remainBytes ? buffer.Count : remainBytes);
				await InternalReadAsync (buffer.Array, buffer.Offset, readLength);
				if (isMasked)
					UnmaskInPlace (mask, buffer.Array, buffer.Offset, readLength, remainOffset);
				remainOffset += readLength;
				remainBytes -= readLength;
				return new WebSocketReceiveResult (readLength, receivedType, isLast && remainBytes == 0);
			}

			try {
				// First read the two first bytes to know what we are doing next
				await InternalReadAsync (headerBuffer, 0, 2);
				isLast = (headerBuffer[0] >> 7) > 0;
				isMasked = (headerBuffer[1] >> 7) > 0;
				receivedType = (WebSocketMessageType)(headerBuffer[0] & 0xF);
				long length = headerBuffer[1] & 0x7F;
				int offset = 0;
				if (length == 126) {
					offset = 2;
					await InternalReadAsync (headerBuffer, 2, offset);
					length = (headerBuffer[2] << 8) | headerBuffer[3];
				} else if (length == 127) {
					offset = 8;
					await InternalReadAsync (headerBuffer, 2, offset);
					length = 0;
					for (int i = 2; i <= 9; i++)
						length = (length << 8) | headerBuffer[i];
				}
				if (isMasked) {
					await InternalReadAsync (mask, 0, 4);
				}

				if (receivedType == WebSocketMessageType.Close) {
					var tmpBuffer = new byte[length];
					await InternalReadAsync (tmpBuffer, 0, tmpBuffer.Length);
					if (isMasked)
						UnmaskInPlace (mask, tmpBuffer, 0, (int)length);
					var closeStatus = (WebSocketCloseStatus)(tmpBuffer[0] << 8 | tmpBuffer[1]);
					var closeDesc = tmpBuffer.Length > 2 ? Encoding.UTF8.GetString (tmpBuffer, 2, tmpBuffer.Length - 2) : string.Empty;
					if (state == WebSocketState.CloseSent) {
						await SendCloseFrame (WebSocketCloseStatus.NormalClosure, "Received Close", cancellationToken).ConfigureAwait (false);
						InnerClose ();
					} else {
						state = WebSocketState.CloseReceived;
					}
					return new WebSocketReceiveResult ((int)length, receivedType, isLast, closeStatus, closeDesc);
				} else {
					var readLength = (int)(buffer.Count < length ? buffer.Count : length);
					await InternalReadAsync (buffer.Array, buffer.Offset, readLength);
					remainBytes = (int)length - readLength;
					remainOffset = readLength;
					if (isMasked)
						UnmaskInPlace (mask, buffer.Array, buffer.Offset, readLength);
					return new WebSocketReceiveResult (readLength, receivedType, isLast && remainBytes == 0);
				}
			} catch (IOException e) {
				InnerClose ();
				closeStatus = WebSocketCloseStatus.EndpointUnavailable;
				return new WebSocketReceiveResult ((int)0, WebSocketMessageType.Close, true, closeStatus, e.Message);
			}
		}

		// The damn difference between those two methods is that CloseAsync will wait for server acknowledgement before completing
		// while CloseOutputAsync will send the close packet and simply complete.

		public override async Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			await SendCloseFrame (closeStatus, statusDescription, cancellationToken).ConfigureAwait (false);
			if (state == WebSocketState.Open) {
				this.closeStatus = closeStatus;
				state = WebSocketState.CloseSent;
				do {
					// TODO: figure what's exceptions are thrown if the server returns something faulty here
					var result = await ReceiveAsync (new ArraySegment<byte> (new byte[0]), cancellationToken).ConfigureAwait (false);
					if (result.MessageType == WebSocketMessageType.Close) {
						break;
					}
				} while (true);
			}
			InnerClose ();
		}

		public override async Task CloseOutputAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			await SendCloseFrame (closeStatus, statusDescription, cancellationToken).ConfigureAwait (false);
			if (state == WebSocketState.CloseReceived) {
				InnerClose ();
			} else {
				state = WebSocketState.CloseSent;
			}
		}

		async Task InternalReadAsync (byte[] buffer, int offset, int length)
		{
			if (preloaded.Count > 0) {
				if (preloaded.Count > length) {
					Array.Copy (preloaded.Array, preloaded.Offset, buffer, offset, length);
					preloaded = new ArraySegment<byte> (preloaded.Array, preloaded.Offset + length, preloaded.Count - length);
					return;
				}
				Array.Copy (preloaded.Array, preloaded.Offset, buffer, offset, preloaded.Count);
				offset += preloaded.Count;
				length -= preloaded.Count;
				preloaded = new ArraySegment<byte>(new byte[0]);
			}
			if (length == 0)
				return;
			await rstream.ReadAsync (buffer, offset, length);
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

		void InnerClose()
		{
			if (readStream == null)
				return;
			readStream.Dispose ();
			if (! Object.ReferenceEquals (readStream, writeStream))
				writeStream.Dispose ();
			readStream = writeStream = null;
			if (socket != null)
				socket.Close ();
			socket = null;
			state = WebSocketState.Closed;
		}

		public static string CreateAcceptKey(string secKey)
		{
			const string Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
			return Convert.ToBase64String (SHA1.Create ().ComputeHash (Encoding.ASCII.GetBytes (secKey + Magic)));
		}

		int WriteHeader (WebSocketMessageType type, int length, bool endOfMessage)
		{
			var opCode = (byte)type;
			int headerLength;

			sendBuffer[0] = (byte)(opCode | (endOfMessage ? 0x80 : 0x0));
			if (length < 126) {
				sendBuffer[1] = (byte)length;
				headerLength = 2;
			} else if (length <= ushort.MaxValue) {
				sendBuffer[1] = (byte)126;
				sendBuffer[2] = (byte)(length / 256);
				sendBuffer[3] = (byte)(length % 256);
				headerLength = 4;
			} else {
				sendBuffer[1] = (byte)127;
				headerLength = 6;

				int left = length;
				int unit = 256;
				for (int i = 9; i > 1; i--) {
					sendBuffer[i] = (byte)(left % unit);
					left = left / unit;
				}
			}
			if (maskSend) {
				sendBuffer[1] |= 0x80;
			}
			return headerLength;
		}

		void GenerateMask (byte[] mask, int offset)
		{
			mask[offset + 0] = (byte)random.Next (0, 255);
			mask[offset + 1] = (byte)random.Next (0, 255);
			mask[offset + 2] = (byte)random.Next (0, 255);
			mask[offset + 3] = (byte)random.Next (0, 255);
		}

		void MaskData (int offset, ArraySegment<byte> data)
		{
			// Client MUST use fresh mask for each frame.
			// https://tools.ietf.org/html/rfc6455#section-5.3
			GenerateMask (sendBuffer, offset);
			int dataOffset = offset + 4;
			for (var i = 0; i < data.Count; i++)
				sendBuffer[dataOffset + i] = (byte)(data.Array[data.Offset + i] ^ sendBuffer[offset + (i % 4)]);
		}

		static void UnmaskInPlace(byte[] mask, byte[] data, int offset, int length, int frameOffset=0)
		{
			for (int i = 0; i < length; i++)
				data[offset + i] ^= mask[(frameOffset + i) % 4];
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

		static internal void ValidateArraySegment (ArraySegment<byte> segment)
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

#endif
