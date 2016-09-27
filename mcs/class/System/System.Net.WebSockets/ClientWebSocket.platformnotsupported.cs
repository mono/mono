//
// ClientWebSocket.cs
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Threading;
using System.Threading.Tasks;

namespace System.Net.WebSockets
{
	public class ClientWebSocket : WebSocket, IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Net.WebSockets.ClientWebSocket is not supported on the current platform.";

		public ClientWebSocket ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Dispose ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override void Abort ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public ClientWebSocketOptions Options {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override WebSocketState State {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override WebSocketCloseStatus? CloseStatus {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string CloseStatusDescription {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public override string SubProtocol {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Task ConnectAsync (Uri uri, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task SendAsync (ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task CloseAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task CloseOutputAsync (WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
