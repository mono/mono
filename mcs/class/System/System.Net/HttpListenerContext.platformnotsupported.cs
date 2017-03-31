//
// System.Net.HttpListenerContext
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

using System.Security.Principal;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace System.Net {
	public sealed class HttpListenerContext {
		const string EXCEPTION_MESSAGE = "System.Net.HttpListenerContext is not supported on the current platform.";

		HttpListenerContext ()
		{
		}

		public HttpListenerRequest Request {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public HttpListenerResponse Response {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public IPrincipal User {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
#if SECURITY_DEP
		public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync (string subProtocol)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync (string subProtocol, TimeSpan keepAliveInterval)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync (string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpListenerWebSocketContext> AcceptWebSocketAsync (string subProtocol, int receiveBufferSize, TimeSpan keepAliveInterval, ArraySegment<byte> internalBuffer)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
#endif
	}
}
