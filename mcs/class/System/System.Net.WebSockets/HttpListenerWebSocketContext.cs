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
using System.Net;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace System.Net.WebSockets
{
	public class HttpListenerWebSocketContext : WebSocketContext
	{
		[MonoTODO]
		public override CookieCollection CookieCollection {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override NameValueCollection Headers {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool IsAuthenticated {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool IsLocal {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool IsSecureConnection {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string Origin {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Uri RequestUri {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string SecWebSocketKey {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override IEnumerable<string> SecWebSocketProtocols {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string SecWebSocketVersion {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override IPrincipal User {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override WebSocket WebSocket {
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

#endif
