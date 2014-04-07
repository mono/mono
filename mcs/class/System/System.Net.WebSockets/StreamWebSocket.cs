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
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace System.Net.WebSockets {
	internal class StreamWebSocket : WebSocket, IDisposable {
		const string Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		WebSocketState state;
		Stream writeStream;
		Stream readStream;
		Random random;
		string subProtocol;

		bool maskSend;
		const int HeaderMaxLength = 14;
		byte[] headerBuffer;
		int sendBufferSize;
		byte[] sendBuffer;

		public StreamWebSocket (Stream readStream, Stream writeStream, int sendBufferSize, bool maskSend, string subProtocol)
		{
			this.readStream = readStream;
			this.writeStream = writeStream;
			this.maskSend = maskSend;
			if (maskSend) {
				random = new Random ();
			}
			this.sendBufferSize = sendBufferSize;
			state = WebSocketState.Open;
			headerBuffer = new byte[HeaderMaxLength];
			this.subProtocol = subProtocol;
		}

		public override void Dispose ()
		{
			if (readStream != null) {
				readStream.Dispose ();
				readStream = null;
			}
			if (writeStream != null) {
				writeStream.Dispose ();
				writeStream = null;
			}
			if (state != WebSocketState.Closed) {
				state = WebSocketState.Aborted;
			}
		}

		public override WebSocketState State {
			get { return state; }
		}

		public override string SubProtocol {
			get { return subProtocol; }
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

		public override void Abort ()
		{
			this.Dispose ();
		}

		public override Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			ValidateArraySegment (buffer);
			if (writeStream == null)
				throw new WebSocketException (WebSocketError.Faulted);
			var count = Math.Max (sendBufferSize, buffer.Count) + HeaderMaxLength;
			if (sendBuffer == null || sendBuffer.Length < count)
				sendBuffer = new byte[count];
			EnsureWebSocketState (WebSocketState.Open, WebSocketState.CloseReceived);
			var maskOffset = WriteHeader (messageType, buffer, endOfMessage);
			int headerLength = maskOffset;
			if (buffer.Count > 0) {
				if (maskSend) {
					headerLength += 4;
					MaskData (buffer, maskOffset);
				} else {
					Array.Copy (buffer.Array, buffer.Offset, sendBuffer, maskOffset, buffer.Count);
				}
			}
			Array.Copy (headerBuffer, sendBuffer, headerLength);
			return writeStream.WriteAsync (sendBuffer, 0, headerLength + buffer.Count, cancellationToken);
		}

		public override async Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			ValidateArraySegment (buffer);
			EnsureWebSocketState (WebSocketState.Open, WebSocketState.CloseSent);

			// First read the two first bytes to know what we are doing next
			await readStream.ReadAsync (headerBuffer, 0, 2, cancellationToken);
			var isLast = (headerBuffer[0] >> 7) > 0;
			var isMasked = (headerBuffer[1] >> 7) > 0;
			byte[] mask = new byte[4];
			var type = (WebSocketMessageType)(headerBuffer[0] & 0xF);
			long length = headerBuffer[1] & 0x7F;
			int offset = 0;
			if (length == 126) {
				offset = 2;
				await readStream.ReadAsync (headerBuffer, 2, offset, cancellationToken);
				length = (headerBuffer[2] << 8) | headerBuffer[3];
			} else if (length == 127) {
				offset = 8;
				await readStream.ReadAsync (headerBuffer, 2, offset, cancellationToken);
				length = 0;
				for (int i = 2; i <= 9; i++)
					length = (length << 8) | headerBuffer[i];
			}

			if (isMasked) {
				await readStream.ReadAsync (headerBuffer, 2 + offset, 4, cancellationToken);
				for (int i = 0; i < 4; i++) {
					var pos = i + offset + 2;
					mask[i] = headerBuffer[pos];
				}
			}

			if (type == WebSocketMessageType.Close) {
				var tmpBuffer = new byte[length];
				await readStream.ReadAsync (tmpBuffer, 0, tmpBuffer.Length, cancellationToken);
				UnmaskInPlace (mask, tmpBuffer, 0, (int)length);
				var closeStatus = (WebSocketCloseStatus)(tmpBuffer[0] << 8 | tmpBuffer[1]);
				var closeDesc = tmpBuffer.Length > 2 ? Encoding.UTF8.GetString (tmpBuffer, 2, tmpBuffer.Length - 2) : string.Empty;
				if (state == WebSocketState.CloseSent) {
					await SendCloseFrame (WebSocketCloseStatus.NormalClosure, "Received Close", cancellationToken).ConfigureAwait (false);
					InnerClose ();
				} else {
					state = WebSocketState.CloseReceived;
				}
				return new WebSocketReceiveResult ((int)length, type, isLast, closeStatus, closeDesc);
			} else {
				var readLength = (int)(buffer.Count < length ? buffer.Count : length);
				await readStream.ReadAsync (buffer.Array, buffer.Offset, readLength);
				UnmaskInPlace (mask, buffer.Array, buffer.Offset, readLength);
				// TODO: Skip remains if buffer is smaller than frame?
				return new WebSocketReceiveResult ((int)length, type, isLast);
			}
		}

		// The damn difference between those two methods is that CloseAsync will wait for server acknowledgement before completing
		// while CloseOutputAsync will send the close packet and simply complete.

		public override async Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			EnsureWebSocketConnected ();
			await SendCloseFrame (closeStatus, statusDescription, cancellationToken).ConfigureAwait (false);
			if (state == WebSocketState.Open) {
				state = WebSocketState.CloseSent;
				while (true) {
					// TODO: figure what's exceptions are thrown if the server returns something faulty here
					var result = await ReceiveAsync (new ArraySegment<byte> (new byte[0]), cancellationToken).ConfigureAwait (false);
					if (result.MessageType == WebSocketMessageType.Close) {
						break;
					}
				}
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
			readStream.Dispose ();
			readStream = null;
			writeStream.Dispose ();
			writeStream = null;
			state = WebSocketState.Closed;
		}

		public static string CreateAcceptKey(string secKey)
		{
			return Convert.ToBase64String (SHA1.Create ().ComputeHash (Encoding.ASCII.GetBytes (secKey + Magic)));
		}

		int WriteHeader (WebSocketMessageType type, ArraySegment<byte> buffer, bool endOfMessage)
		{
			var opCode = (byte)type;
			var length = buffer.Count;

			headerBuffer[0] = (byte)(opCode | (endOfMessage ? 0x80 : 0x0));
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
			if (maskSend) {
				GenerateMask (headerBuffer, maskOffset);
				headerBuffer[1] |= 0x80;
			}
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

		void UnmaskInPlace(byte[] mask, byte[] data, int offset, int length)
		{
			for (int i = 0; i < length; i++) {
				data[i + offset] ^= mask[i % 4];
			}
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

#endif
