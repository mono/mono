//
// WebSocket.cs
//
// Authors:
//    Jérémie Laval <jeremie dot laval at xamarin dot com>
//
// Copyright 2013 Xamarin Inc (http://www.xamarin.com).
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
//
//

#if NET_4_5

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace System.Net.WebSockets
{
	public abstract class WebSocket : IDisposable
	{
		protected WebSocket ()
		{
			
		}

		public abstract Nullable<WebSocketCloseStatus> CloseStatus { get; }
		public abstract string CloseStatusDescription { get; }
		public abstract WebSocketState State { get; }
		public abstract string SubProtocol { get; }

		[MonoTODO]
		public static TimeSpan DefaultKeepAliveInterval {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract void Abort ();
		
		public abstract Task CloseAsync (WebSocketCloseStatus closeStatus,
		                                 string statusDescription,
		                                 CancellationToken cancellationToken);

		public abstract Task CloseOutputAsync (WebSocketCloseStatus closeStatus,
		                                       string statusDescription,
		                                       CancellationToken cancellationToken);

		public abstract Task<WebSocketReceiveResult> ReceiveAsync (ArraySegment<byte> buffer,
		                                                           CancellationToken cancellationToken);

		public abstract Task SendAsync (ArraySegment<byte> buffer,
		                                WebSocketMessageType messageType,
		                                bool endOfMessage,
		                                CancellationToken cancellationToken);

		[MonoTODO]
		public static ArraySegment<byte> CreateClientBuffer (int receiveBufferSize, int sendBufferSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static WebSocket CreateClientWebSocket (Stream innerStream,
		                                               string subProtocol,
		                                               int receiveBufferSize,
		                                               int sendBufferSize,
		                                               TimeSpan keepAliveInterval,
		                                               bool useZeroMaskingKey,
		                                               ArraySegment<byte> internalBuffer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ArraySegment<byte> CreateServerBuffer (int receiveBufferSize)
		{
			throw new NotImplementedException ();
		}

		[ObsoleteAttribute, MonoTODO]
		public static bool IsApplicationTargeting45 ()
		{
			return true;
		}

		[MonoTODO]
		public static void RegisterPrefixes ()
		{
			throw new NotImplementedException ();
		}

		public abstract void Dispose ();

		protected static bool IsStateTerminal (WebSocketState state)
		{
			return state == WebSocketState.Closed || state == WebSocketState.Aborted;
		}

		[MonoTODO]
		protected static void ThrowOnInvalidState (WebSocketState state, params WebSocketState[] validStates)
		{
			foreach (var validState in validStates)
				if (validState == state)
					return;

			throw new NotImplementedException ();
		}
	}
}

#endif
