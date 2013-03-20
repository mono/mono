//
// ClientWebSocketOptions.cs
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
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace System.Net.WebSockets
{
	public sealed class ClientWebSocketOptions
	{
		public X509CertificateCollection ClientCertificates { get; set; }

		public CookieContainer Cookies { get; set; }

		public ICredentials Credentials { get; set; }

		public TimeSpan KeepAliveInterval { get; set; }

		public IWebProxy Proxy { get; set; }

		public bool UseDefaultCredentials { get; set; }

		[MonoTODO]
		public void AddSubProtocol (string subProtocol)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetBuffer (int receiveBufferSize, int sendBufferSize)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetBuffer (int receiveBufferSize, int sendBufferSize, ArraySegment<byte> buffer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetRequestHeader (string headerName, string headerValue)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
