//
// HttpClientHandler.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Linq;

namespace System.Net.Http
{
	public class HttpClientHandler : HttpMessageHandler
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
		bool useDefaultCredentials;
		bool useProxy;
		ClientCertificateOption certificate;
		bool sentRequest;
		string connectionGroupName;
		bool disposed;

		public HttpClientHandler ()
		{
			allowAutoRedirect = true;
			maxAutomaticRedirections = 50;
			maxRequestContentBufferSize = int.MaxValue;
			useCookies = true;
			useProxy = true;
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

		public ClientCertificateOption ClientCertificateOptions {
			get {
				return certificate;
			}
			set {
				EnsureModifiability ();
				certificate = value;
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

		public bool UseDefaultCredentials {
			get {
				return useDefaultCredentials;
			}
			set {
				EnsureModifiability ();
				useDefaultCredentials = value;
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

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				Volatile.Write (ref disposed, true);
				ServicePointManager.CloseConnectionGroup (connectionGroupName);
			}

			base.Dispose (disposing);
		}

		internal virtual HttpWebRequest CreateWebRequest (HttpRequestMessage request)
		{
			var wr = new HttpWebRequest (request.RequestUri);
			wr.ThrowOnError = false;
			wr.AllowWriteStreamBuffering = false;

			wr.ConnectionGroupName = connectionGroupName;
			wr.Method = request.Method.Method;
			wr.ProtocolVersion = request.Version;

			if (wr.ProtocolVersion == HttpVersion.Version10) {
				wr.KeepAlive = request.Headers.ConnectionKeepAlive;
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

			if (useDefaultCredentials) {
				wr.UseDefaultCredentials = true;
			} else {
				wr.Credentials = credentials;
			}

			if (useProxy) {
				wr.Proxy = proxy;
			} else {
				// Disables default WebRequest.DefaultWebProxy value
				wr.Proxy = null;
			}

			wr.ServicePoint.Expect100Continue = request.Headers.ExpectContinue == true;

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
					// Chunked Transfer-Encoding is never set for HttpWebRequest. It's detected
					// from ContentLength by HttpWebRequest
					values = values.Where (l => l != "chunked");
				}

				var values_formated = HttpRequestHeaders.GetSingleHeaderString (header.Key, values);
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
			response.Content = new StreamContent (wr.GetResponseStream (), cancellationToken);

			var headers = wr.Headers;
			for (int i = 0; i < headers.Count; ++i) {
				var key = headers.GetKey(i);
				var value = headers.GetValues (i);

				HttpHeaders item_headers;
				if (HttpHeaders.GetKnownHeaderKind (key) == Headers.HttpHeaderKind.Content)
					item_headers = response.Content.Headers;
				else
					item_headers = response.Headers;
					
				item_headers.TryAddWithoutValidation (key, value);
			}

			requestMessage.RequestUri = wr.ResponseUri;

			return response;
		}

		protected async internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
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

						//
						// Content length has to be set because HttpWebRequest is running without buffering
						//
						var contentLength = content.Headers.ContentLength;
						if (contentLength != null) {
							wrequest.ContentLength = contentLength.Value;
						} else {
							await content.LoadIntoBufferAsync (MaxRequestContentBufferSize).ConfigureAwait (false);
							wrequest.ContentLength = content.Headers.ContentLength.Value;
						}

						wrequest.ResendContentFactory = content.CopyTo;

						var stream = await wrequest.GetRequestStreamAsync ().ConfigureAwait (false);
						await request.Content.CopyToAsync (stream).ConfigureAwait (false);
					} else if (HttpMethod.Post.Equals (request.Method) || HttpMethod.Put.Equals (request.Method) || HttpMethod.Delete.Equals (request.Method)) {
						// Explicitly set this to make sure we're sending a "Content-Length: 0" header.
						// This fixes the issue that's been reported on the forums:
						// http://forums.xamarin.com/discussion/17770/length-required-error-in-http-post-since-latest-release
						wrequest.ContentLength = 0;
					}

					wresponse = (HttpWebResponse)await wrequest.GetResponseAsync ().ConfigureAwait (false);
				}
			} catch (WebException we) {
				if (we.Status != WebExceptionStatus.RequestCanceled)
					throw;
			}

			if (cancellationToken.IsCancellationRequested) {
				var cancelled = new TaskCompletionSource<HttpResponseMessage> ();
				cancelled.SetCanceled ();
				return await cancelled.Task;
			}
			
			return CreateResponseMessage (wresponse, request, cancellationToken);
		}

#if NETSTANDARD
		public bool CheckCertificateRevocationList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public X509CertificateCollection ClientCertificates {
			get {
				throw new NotImplementedException ();
			}
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

		public int MaxResponseHeadersLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public IDictionary<string,object> Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		public Func<HttpRequestMessage,X509Certificate2,X509Chain,SslPolicyErrors,bool> ServerCertificateCustomValidationCallback {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public SslProtocols SslProtocols {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

#endif
	}
}
