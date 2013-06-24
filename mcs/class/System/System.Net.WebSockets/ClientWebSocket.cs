//
// ClientWebSocket.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSockets
{
	[MonoTODO]
	public class ClientWebSocket : WebSocket
	{
		public ClientWebSocketOptions Options {
			get { throw new NotImplementedException (); }
		}

		public Task ConnectAsync (Uri uri, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}

		#region implemented abstract members of WebSocket
		public override void Abort ()
		{
			throw new NotImplementedException ();
		}
		public override Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		public override Task CloseOutputAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		public override Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		public override Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
		public override void Dispose ()
		{
			throw new NotImplementedException ();
		}
		public override WebSocketCloseStatus? CloseStatus {
			get {
				throw new NotImplementedException ();
			}
		}
		public override string CloseStatusDescription {
			get {
				throw new NotImplementedException ();
			}
		}
		public override WebSocketState State {
			get {
				throw new NotImplementedException ();
			}
		}
		public override string SubProtocol {
			get {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}

#endif

