//
// HttpListenerWebSocketContext.cs
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
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace System.Net.WebSockets
{
	public class HttpListenerWebSocketContext : WebSocketContext
	{
		HttpListenerRequest request;
		StreamWebSocket webSocket;
		IPrincipal user;

		internal HttpListenerWebSocketContext(RequestStream requestStream, Stream writeStream, HttpListenerRequest request, IPrincipal user, string subProtocol)
		{
			this.request = request;
			this.user = user;
			webSocket = new StreamWebSocket (requestStream, writeStream, 16 * 1024, false, subProtocol);
		}

		public override CookieCollection CookieCollection {
			get {
				return request.Cookies;
			}
		}

		public override NameValueCollection Headers {
			get {
				return request.Headers;
			}
		}

		public override bool IsAuthenticated {
			get {
				return request.IsAuthenticated;
			}
		}

		public override bool IsLocal {
			get {
				return request.IsLocal;
			}
		}

		public override bool IsSecureConnection {
			get {
				return request.IsSecureConnection;
			}
		}

		public override string Origin {
			get {
				return request.Headers ["Origin"];
			}
		}

		public override Uri RequestUri {
			get {
				return request.Url;
			}
		}

		public override string SecWebSocketKey {
			get {
				return request.Headers ["Sec-WebSocket-Key"];
			}
		}

		[MonoTODO]
		public override IEnumerable<string> SecWebSocketProtocols {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string SecWebSocketVersion {
			get {
				return request.Headers ["Sec-WebSocket-Version"];
			}
		}

		public override IPrincipal User {
			get { return user; }
		}

		public override WebSocket WebSocket {
			get {
				return webSocket;
			}
		}
	}
}

#endif
