//
// MonoWebRequestHandler.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//      Martin Baulig  <mabaul@microsoft.com>
//
// Copyright (C) 2011-2018 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Cache;
using System.Net.Security;
using System.Linq;

namespace System.Net.Http
{
	class MonoWebRequestHandler : IMonoHttpClientHandler
	{
		static long groupCounter;

		bool allowAutoRedirect;
		DecompressionMethods automaticDecompression;
		CookieContainer cookieContainer;
		ICredentials credentials;
		int maxAutomaticRedirections;
		long maxRequestContentBufferSize;
		bool preAuthenticate;
		IWebProxy proxy;
		bool useCookies;
		bool useProxy;
		SslClientAuthenticationOptions sslOptions;
		bool allowPipelining;
		RequestCachePolicy cachePolicy;
		AuthenticationLevel authenticationLevel;
		TimeSpan continueTimeout;
		TokenImpersonationLevel impersonationLevel;
		int maxResponseHeadersLength;
		int readWriteTimeout;
		RemoteCertificateValidationCallback serverCertificateValidationCallback;
		bool unsafeAuthenticatedConnectionSharing;
		bool sentRequest;
		string connectionGroupName;
		TimeSpan? timeout;
		bool disposed;

		public MonoWebRequestHandler ()
		{
			allowAutoRedirect = true;
			maxAutomaticRedirections = 50;
			maxRequestContentBufferSize = int.MaxValue;
			useCookies = true;
			useProxy = true;
			allowPipelining = true;
			authenticationLevel = AuthenticationLevel.MutualAuthRequested;
			cachePolicy = System.Net.WebRequest.DefaultCachePolicy;
			continueTimeout = TimeSpan.FromMilliseconds (350);
			impersonationLevel = TokenImpersonationLevel.Delegation;
			maxResponseHeadersLength = HttpWebRequest.DefaultMaximumResponseHeadersLength;
			readWriteTimeout = 300000;
			serverCertificateValidationCallback = null;
			unsafeAuthenticatedConnectionSharing = false;
			connectionGroupName = "HttpClientHandler" + Interlocked.Increment (ref groupCounter);
		}

		internal void EnsureModifiability ()
		{
			if (sentRequest)
				throw new InvalidOperationException (
					"This instance has already started one or more requests. " +
					"Properties can only be modified before sending the first request.");
		}

		public bool AllowAutoRedirect {
			get {
				return allowAutoRedirect;
			}
			set {
				EnsureModifiability ();
				allowAutoRedirect = value;
			}
		}

		public DecompressionMethods AutomaticDecompression {
			get {
				return automaticDecompression;
			}
			set {
				EnsureModifiability ();
				automaticDecompression = value;
			}
		}

		public CookieContainer CookieContainer {
			get {
				return cookieContainer ?? (cookieContainer = new CookieContainer ());
			}
			set {
				EnsureModifiability ();
				cookieContainer = value;
			}
		}

		public ICredentials Credentials {
			get {
				return credentials;
			}
			set {
				EnsureModifiability ();
				credentials = value;
			}
		}

		public int MaxAutomaticRedirections {
			get {
				return maxAutomaticRedirections;
			}
			set {
				EnsureModifiability ();
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

				maxAutomaticRedirections = value;
			}
		}

		public long MaxRequestContentBufferSize {
			get {
				return maxRequestContentBufferSize;
			}
			set {
				EnsureModifiability ();
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				maxRequestContentBufferSize = value;
			}
		}

		public bool PreAuthenticate {
			get {
				return preAuthenticate;
			}
			set {
				EnsureModifiability ();
				preAuthenticate = value;
			}
		}

		public IWebProxy Proxy {
			get {
				return proxy;
			}
			set {
				EnsureModifiability ();
				if (!UseProxy)
					throw new InvalidOperationException ();

				proxy = value;
			}
		}

		public virtual bool SupportsAutomaticDecompression {
			get {
				return true;
			}
		}

		public virtual bool SupportsProxy {
			get {
				return true;
			}
		}

		public virtual bool SupportsRedirectConfiguration {
			get {
				return true;
			}
		}

		public bool UseCookies {
			get {
				return useCookies;
			}
			set {
				EnsureModifiability ();
				useCookies = value;
			}
		}

		public bool UseProxy {
			get {
				return useProxy;
			}
			set {
				EnsureModifiability ();
				useProxy = value;
			}
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

		public SslClientAuthenticationOptions SslOptions {
			get => sslOptions ?? (sslOptions = new SslClientAuthenticationOptions ());
			set {
				EnsureModifiability ();
				sslOptions = value;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				Volatile.Write (ref disposed, true);
				ServicePointManager.CloseConnectionGroup (connectionGroupName);
			}
		}

		bool GetConnectionKeepAlive (HttpRequestHeaders headers)
		{
			return headers.Connection.Any (l => string.Equals (l, "Keep-Alive", StringComparison.OrdinalIgnoreCase));
		}

		internal virtual HttpWebRequest CreateWebRequest (HttpRequestMessage request)
		{
			var wr = new HttpWebRequest (request.RequestUri);
			wr.ThrowOnError = false;
			wr.AllowWriteStreamBuffering = false;

			if (request.Version == HttpVersion.Version20)
				wr.ProtocolVersion = HttpVersion.Version11;
			else
				wr.ProtocolVersion = request.Version;

			wr.ConnectionGroupName = connectionGroupName;
			wr.Method = request.Method.Method;

			if (wr.ProtocolVersion == HttpVersion.Version10) {
				wr.KeepAlive = GetConnectionKeepAlive (request.Headers);
			} else {
				wr.KeepAlive = request.Headers.ConnectionClose != true;
			}

			if (allowAutoRedirect) {
				wr.AllowAutoRedirect = true;
				wr.MaximumAutomaticRedirections = maxAutomaticRedirections;
			} else {
				wr.AllowAutoRedirect = false;
			}

			wr.AutomaticDecompression = automaticDecompression;
			wr.PreAuthenticate = preAuthenticate;

			if (useCookies) {
				// It cannot be null or allowAutoRedirect won't work
				wr.CookieContainer = CookieContainer;
			}

			wr.Credentials = credentials;

			if (useProxy) {
				wr.Proxy = proxy;
			} else {
				// Disables default WebRequest.DefaultWebProxy value
				wr.Proxy = null;
			}

			wr.ServicePoint.Expect100Continue = request.Headers.ExpectContinue == true;

			if (timeout != null)
				wr.Timeout = (int)timeout.Value.TotalMilliseconds;

			// Add request headers
			var headers = wr.Headers;
			foreach (var header in request.Headers) {
				var values = header.Value;
				if (header.Key == "Host") {
					//
					// Host must be explicitly set for HttpWebRequest
					//
					wr.Host = request.Headers.Host;
					continue;
				}

				if (header.Key == "Transfer-Encoding") {
					//
					// Chunked Transfer-Encoding is set for HttpWebRequest later when Content length is checked
					//
					values = values.Where (l => l != "chunked");
				}

				var values_formated = PlatformHelper.GetSingleHeaderString (header.Key, values);
				if (values_formated == null)
					continue;

				headers.AddInternal (header.Key, values_formated);
			}

			return wr;
		}

		HttpResponseMessage CreateResponseMessage (HttpWebResponse wr, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage (wr.StatusCode);
			response.RequestMessage = requestMessage;
			response.ReasonPhrase = wr.StatusDescription;
			response.Content = PlatformHelper.CreateStreamContent (wr.GetResponseStream (), cancellationToken);

			var headers = wr.Headers;
			for (int i = 0; i < headers.Count; ++i) {
				var key = headers.GetKey (i);
				var value = headers.GetValues (i);

				HttpHeaders item_headers;
				if (PlatformHelper.IsContentHeader (key))
					item_headers = response.Content.Headers;
				else
					item_headers = response.Headers;

				item_headers.TryAddWithoutValidation (key, value);
			}

			requestMessage.RequestUri = wr.ResponseUri;

			return response;
		}

		static bool MethodHasBody (HttpMethod method)
		{
			switch (method.Method) {
			case "HEAD":
			case "GET":
			case "MKCOL":
			case "CONNECT":
			case "TRACE":
				return false;
			default:
				return true;
			}
		}

		public async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			Volatile.Write (ref sentRequest, true);
			var wrequest = CreateWebRequest (request);
			HttpWebResponse wresponse = null;

			try {
				using (cancellationToken.Register (l => ((HttpWebRequest)l).Abort (), wrequest)) {
					var content = request.Content;
					if (content != null) {
						var headers = wrequest.Headers;

						foreach (var header in content.Headers) {
							foreach (var value in header.Value) {
								headers.AddInternal (header.Key, value);
							}
						}

						if (request.Headers.TransferEncodingChunked == true) {
							wrequest.SendChunked = true;
						} else {
							//
							// Content length has to be set because HttpWebRequest is running without buffering
							//
							var contentLength = content.Headers.ContentLength;
							if (contentLength != null) {
								wrequest.ContentLength = contentLength.Value;
							} else {
								if (MaxRequestContentBufferSize == 0)
									throw new InvalidOperationException ("The content length of the request content can't be determined. Either set TransferEncodingChunked to true, load content into buffer, or set MaxRequestContentBufferSize.");

								await content.LoadIntoBufferAsync (MaxRequestContentBufferSize).ConfigureAwait (false);
								wrequest.ContentLength = content.Headers.ContentLength.Value;
							}
						}

						wrequest.ResendContentFactory = content.CopyToAsync;

						using (var stream = await wrequest.GetRequestStreamAsync ().ConfigureAwait (false)) {
							await request.Content.CopyToAsync (stream).ConfigureAwait (false);
						}
					} else if (MethodHasBody (request.Method)) {
						// Explicitly set this to make sure we're sending a "Content-Length: 0" header.
						// This fixes the issue that's been reported on the forums:
						// http://forums.xamarin.com/discussion/17770/length-required-error-in-http-post-since-latest-release
						wrequest.ContentLength = 0;
					}

					wresponse = (HttpWebResponse)await wrequest.GetResponseAsync ().ConfigureAwait (false);
				}
			} catch (WebException we) {
				if (we.Status != WebExceptionStatus.RequestCanceled)
					throw new HttpRequestException ("An error occurred while sending the request", we);
			} catch (System.IO.IOException ex) {
				throw new HttpRequestException ("An error occurred while sending the request", ex);
			}

			if (cancellationToken.IsCancellationRequested) {
				var cancelled = new TaskCompletionSource<HttpResponseMessage> ();
				cancelled.SetCanceled ();
				return await cancelled.Task;
			}

			return CreateResponseMessage (wresponse, request, cancellationToken);
		}

		public ICredentials DefaultProxyCredentials {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int MaxConnectionsPerServer {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public IDictionary<string, object> Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		void IMonoHttpClientHandler.SetWebRequestTimeout (TimeSpan timeout)
		{
			this.timeout = timeout;
		}
	}
}
