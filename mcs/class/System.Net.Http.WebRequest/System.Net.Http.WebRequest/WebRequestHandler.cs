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
using System.Net.Cache;
using System.Net.Security;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;

namespace System.Net.Http
{
	public class WebRequestHandler : HttpClientHandler
	{
		bool allowPipelining;
		RequestCachePolicy cachePolicy;
		AuthenticationLevel authenticationLevel;
		TimeSpan continueTimeout;
		TokenImpersonationLevel impersonationLevel;
		int maxResponseHeadersLength;
		int readWriteTimeout;
		RemoteCertificateValidationCallback serverCertificateValidationCallback;
		bool unsafeAuthenticatedConnectionSharing;

		public WebRequestHandler ()
		{
			allowPipelining = true;
			authenticationLevel = AuthenticationLevel.MutualAuthRequested;
			cachePolicy = System.Net.WebRequest.DefaultCachePolicy;
			continueTimeout = TimeSpan.FromMilliseconds (350);
			impersonationLevel = TokenImpersonationLevel.Delegation;
			maxResponseHeadersLength = HttpWebRequest.DefaultMaximumResponseHeadersLength;
			readWriteTimeout = 300000;
			serverCertificateValidationCallback = null;
			unsafeAuthenticatedConnectionSharing = false;
		}

		public bool AllowPipelining {
			get { return allowPipelining; }
			set {
 				EnsureModifiability ();
 				allowPipelining = value;
			}
		}

		public RequestCachePolicy CachePolicy {
			get { return cachePolicy; }
			set {
				EnsureModifiability ();
				cachePolicy = value;
			}
		}

		public AuthenticationLevel AuthenticationLevel {
			get { return authenticationLevel; }
			set {
				EnsureModifiability ();
				authenticationLevel = value;
			}
		}

		[MonoTODO]
		public X509CertificateCollection ClientCertificates {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public TimeSpan ContinueTimeout {
			get { return continueTimeout; }
			set {
				EnsureModifiability ();
				continueTimeout = value;
			}
		}

		public TokenImpersonationLevel ImpersonationLevel {
			get { return impersonationLevel; }
			set {
				EnsureModifiability ();
				impersonationLevel = value;
			}
		}

		public int MaxResponseHeadersLength {
			get { return maxResponseHeadersLength; }
			set {
				EnsureModifiability ();
				maxResponseHeadersLength = value;
			}
		}

		public int ReadWriteTimeout {
			get { return readWriteTimeout; }
			set {
				EnsureModifiability ();
				readWriteTimeout = value;
			}
		}

		[MonoTODO]
		public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get { return serverCertificateValidationCallback; }
			set {
				EnsureModifiability ();
				serverCertificateValidationCallback = value;
			}
		}

		public bool UnsafeAuthenticatedConnectionSharing {
			get { return unsafeAuthenticatedConnectionSharing; }
			set {
				EnsureModifiability ();
				unsafeAuthenticatedConnectionSharing = value;
			}
		}

		internal override HttpWebRequest CreateWebRequest (HttpRequestMessage request)
		{
			HttpWebRequest wr = base.CreateWebRequest (request);

			wr.Pipelined = allowPipelining;
			wr.AuthenticationLevel = authenticationLevel;
			wr.CachePolicy = cachePolicy;
			wr.ImpersonationLevel = impersonationLevel;
			wr.MaximumResponseHeadersLength = maxResponseHeadersLength;
			wr.ReadWriteTimeout = readWriteTimeout;
			wr.UnsafeAuthenticatedConnectionSharing = unsafeAuthenticatedConnectionSharing;

			return wr;
		}
	}
}

