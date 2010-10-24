//
// HttpTransportSecurity.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System;
#if NET_4_0
using System.Security.Authentication.ExtendedProtection;
#endif
using System.ServiceModel.Security;

namespace System.ServiceModel
{
	public sealed class HttpTransportSecurity
	{
		HttpClientCredentialType client;
		HttpProxyCredentialType proxy;
		string realm = String.Empty;

		internal HttpTransportSecurity ()
		{
		}

		public HttpClientCredentialType ClientCredentialType {
			get { return client; }
			set { client = value; }
		}

		public HttpProxyCredentialType ProxyCredentialType {
			get { return proxy; }
			set { proxy = value; }
		}

		public string Realm {
			get { return realm; }
			set { realm = value; }
		}

#if NET_4_0
		[MonoTODO]
		public ExtendedProtectionPolicy ExtendedProtectionPolicy { get; set; }
#endif
	}
}
