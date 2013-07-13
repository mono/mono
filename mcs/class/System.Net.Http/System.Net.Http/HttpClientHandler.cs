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

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace System.Net.Http
{
	public class HttpClientHandler : HttpMessageHandler
	{
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

		public HttpClientHandler ()
		{
			allowAutoRedirect = true;
			maxAutomaticRedirections = 50;
			maxRequestContentBufferSize = int.MaxValue;
			useCookies = true;
			useProxy = true;
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
			// TODO: ?
			base.Dispose (disposing);
		}

		internal virtual HttpWebRequest CreateWebRequest (HttpRequestMessage request)
		{
			var wr = new HttpWebRequest (request.RequestUri);
			wr.ThrowOnError = false;

			wr.ConnectionGroupName = "HttpClientHandler";
			wr.Method = request.Method.Method;
			wr.ProtocolVersion = request.Version;

			if (wr.ProtocolVersion == HttpVersion.Version10) {
				wr.KeepAlive = request.Headers.ConnectionKeepAlive;
			} else {
				wr.KeepAlive = request.Headers.ConnectionClose != true;
			}

			wr.ServicePoint.Expect100Continue = request.Headers.ExpectContinue == true;

			if (allowAutoRedirect) {
				wr.AllowAutoRedirect = true;
				wr.MaximumAutomaticRedirections = maxAutomaticRedirections;
			} else {
				wr.AllowAutoRedirect = false;
			}

			wr.AutomaticDecompression = automaticDecompression;
			wr.PreAuthenticate = preAuthenticate;

			if (useCookies) {
				wr.CookieContainer = cookieContainer;
			}

			if (useDefaultCredentials) {
				wr.UseDefaultCredentials = true;
			} else {
				wr.Credentials = credentials;
			}

			if (useProxy) {
				wr.Proxy = proxy;
			}

			// Add request headers
			var headers = wr.Headers;
			foreach (var header in request.Headers) {
				foreach (var value in header.Value) {
					headers.AddValue (header.Key, value);
				}
			}
			
			return wr;
		}

		HttpResponseMessage CreateResponseMessage (HttpWebResponse wr, HttpRequestMessage requestMessage)
		{
			var response = new HttpResponseMessage (wr.StatusCode);
			response.RequestMessage = requestMessage;
			response.ReasonPhrase = wr.StatusDescription;
			response.Content = new StreamContent (wr.GetResponseStream ());

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

			return response;
		}

		protected async internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			sentRequest = true;
			var wrequest = CreateWebRequest (request);

			if (request.Content != null) {
				var headers = wrequest.Headers;
				foreach (var header in request.Content.Headers) {
					foreach (var value in header.Value) {
						headers.AddValue (header.Key, value);
					}
				}

				var stream = await wrequest.GetRequestStreamAsync ().ConfigureAwait (false);
				await request.Content.CopyToAsync (stream).ConfigureAwait (false);
			}

			HttpWebResponse wresponse = null;
			using (cancellationToken.Register (l => ((HttpWebRequest) l).Abort (), wrequest)) {
				try {
					wresponse = (HttpWebResponse) await wrequest.GetResponseAsync ().ConfigureAwait (false);
				} catch (WebException we) {
					if (we.Status != WebExceptionStatus.RequestCanceled)
						throw;
				}

				if (cancellationToken.IsCancellationRequested) {
					var cancelled = new TaskCompletionSource<HttpResponseMessage> ();
					cancelled.SetCanceled ();
					return await cancelled.Task;
				}
			}
			
			return CreateResponseMessage (wresponse, request);
		}
	}
}
