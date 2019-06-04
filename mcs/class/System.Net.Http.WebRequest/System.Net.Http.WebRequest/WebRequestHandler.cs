//
// WebRequestHandler.cs
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
using System;
using System.Linq;
using System.Threading;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace System.Net.Http
{
	public class WebRequestHandler : HttpClientHandler
	{
		MonoWebRequestHandler handler;

		bool disposed;

		public WebRequestHandler ()
			: this (new MonoWebRequestHandler ())
		{
		}

		WebRequestHandler (MonoWebRequestHandler handler)
			: base (handler)
		{
			this.handler = handler;
		}

		public bool AllowPipelining {
			get => handler.AllowPipelining;
			set => handler.AllowPipelining = value;
		}

		public RequestCachePolicy CachePolicy {
			get => handler.CachePolicy;
			set => handler.CachePolicy = value;
		}

		public AuthenticationLevel AuthenticationLevel {
			get => handler.AuthenticationLevel;
			set => handler.AuthenticationLevel = value;
		}

		[MonoTODO]
		public TimeSpan ContinueTimeout {
			get => handler.ContinueTimeout;
			set => handler.ContinueTimeout = value;
		}

		public TokenImpersonationLevel ImpersonationLevel {
			get => handler.ImpersonationLevel;
			set => handler.ImpersonationLevel = value;
		}

		public int ReadWriteTimeout {
			get => handler.ReadWriteTimeout;
			set => handler.ReadWriteTimeout = value;
		}

		public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get => handler.ServerCertificateValidationCallback;
			set => handler.ServerCertificateValidationCallback = value;
		}

		public bool UnsafeAuthenticatedConnectionSharing {
			get => handler.UnsafeAuthenticatedConnectionSharing;
			set => handler.UnsafeAuthenticatedConnectionSharing = value;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				Volatile.Write (ref disposed, true);
				handler.Dispose ();
				handler = null;
			}

			base.Dispose (disposing);
		}
	}
}

