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
		int maxRequestContentBufferSize;
		bool preAuthenticate;
		IWebProxy proxy;
		bool useCookies;
		bool useDefaultCredentials;
		bool useProxy;

		public HttpClientHandler ()
		{
			allowAutoRedirect = true;
			maxAutomaticRedirections = 50;
			maxRequestContentBufferSize = 0x10000;
			useCookies = true;
			useProxy = true;
		}

		public bool AllowAutoRedirect {
			get {
				return allowAutoRedirect;
			}
			set {
				allowAutoRedirect = value;
			}
		}

		public DecompressionMethods AutomaticDecompression {
			get {
				return automaticDecompression;
			}
			set {
				automaticDecompression = value;
			}
		}

		public CookieContainer CookieContainer {
			get {
				return cookieContainer ?? (cookieContainer = new CookieContainer ());
			}
			set {
				cookieContainer = value;
			}
		}

		public ICredentials Credentials {
			get {
				return credentials;
			}
			set {
				credentials = value;
			}
		}

		public int MaxAutomaticRedirections {
			get {
				return maxAutomaticRedirections;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

				maxAutomaticRedirections = value;
			}
		}

		public int MaxRequestContentBufferSize {
			get {
				return maxRequestContentBufferSize;
			}
			set {
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
				preAuthenticate = value;
			}
		}

		public IWebProxy Proxy {
			get {
				return proxy;
			}
			set {
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
				useCookies = value;
			}
		}

		public bool UseDefaultCredentials {
			get {
				return useDefaultCredentials;
			}
			set {
				useDefaultCredentials = value;
			}
		}

		public bool UseProxy {
			get {
				return useProxy;
			}
			set {
				useProxy = value;
			}
		}

		protected override void Dispose (bool disposing)
		{
			// TODO: ?
			base.Dispose (disposing);
		}

		WebRequest CreateWebRequest (HttpRequestMessage request)
		{
			var factory = Activator.CreateInstance (typeof (IWebRequestCreate).Assembly.GetType ("System.Net.HttpRequestCreator"), true) as IWebRequestCreate;
			var wr = (HttpWebRequest) factory.Create (request.RequestUri);

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
					// TODO: Have to call simpler Add
					headers.Add (header.Key, value);
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

				if (HttpHeaders.GetKnownHeaderKind (key) == Headers.HttpHeaderKind.Content)
					response.Content.Headers.AddWithoutValidation (key, value);
				else
					response.Headers.AddWithoutValidation (key, value);
			}

			return response;
		}

		protected internal override HttpResponseMessage Send (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var wrequest = CreateWebRequest (request);

			if (request.Content != null) {
				throw new NotImplementedException ();
			} else {
				// TODO:
			}

			var wresponse = (HttpWebResponse) wrequest.GetResponse ();

			return CreateResponseMessage (wresponse, request);
		}
		
		protected internal override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException ();
		}
	}
}
