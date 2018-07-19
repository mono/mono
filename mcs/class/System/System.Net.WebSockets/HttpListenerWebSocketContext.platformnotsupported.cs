//
// HttpListenerWebSocketContext.platformnotsupported.cs
//
// Author:
//       Marek Safar <marek.safar@gmail.com>
//
// Copyright (c) 2017 Xamarin Inc. (http://www.xamarin.com)
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

namespace System.Net.WebSockets
{
	public partial class HttpListenerWebSocketContext : System.Net.WebSockets.WebSocketContext
	{
		const string EXCEPTION_MESSAGE = "System.Net.WebSockets.HttpListenerWebSocketContext is not supported on the current platform.";

		private HttpListenerWebSocketContext() { }
		public override System.Net.CookieCollection CookieCollection { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override System.Collections.Specialized.NameValueCollection Headers { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override bool IsAuthenticated { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override bool IsLocal { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override bool IsSecureConnection { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override string Origin { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override System.Uri RequestUri { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override string SecWebSocketKey { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override System.Collections.Generic.IEnumerable<string> SecWebSocketProtocols { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override string SecWebSocketVersion { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override System.Security.Principal.IPrincipal User { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
		public override System.Net.WebSockets.WebSocket WebSocket { get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); } }
	}
}