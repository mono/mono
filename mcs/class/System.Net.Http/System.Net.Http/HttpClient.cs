//
// HttpClient.cs
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
using System.Net.Http.Headers;

namespace System.Net.Http
{
	public class HttpClient : IDisposable
	{
		static readonly TimeSpan TimeoutDefault = TimeSpan.FromSeconds (100);

		Uri base_address;
		CancellationTokenSource cancellation_token;
		bool disposed;
		readonly HttpMessageHandler handler;
		HttpRequestHeaders headers;
		int buffer_size;
		TimeSpan timeout;

		public HttpClient ()
			: this (null)
		{
		}

		public HttpClient (HttpMessageHandler handler)
		{
			this.handler = handler ?? new HttpClientHandler ();
			buffer_size = 0x10000;
			timeout = TimeoutDefault;
		}

		public Uri BaseAddress {
			get {
				return base_address;
			}
			set {
				base_address = value;
			}
		}

		public HttpRequestHeaders DefaultRequestHeaders {
			get {
				return headers ?? (headers = new HttpRequestHeaders ());
			}
		}

		public int MaxResponseContentBufferSize {
			get {
				return buffer_size;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

				buffer_size = value;
			}
		}

		public TimeSpan Timeout {
			get {
				return timeout;
			}
			set {
				if (value != System.Threading.Timeout.InfiniteTimeSpan && value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException ();

				timeout = value;
			}
		}

		public void CancelPendingRequests ()
		{
			if (cancellation_token != null)
				cancellation_token.Cancel ();

			cancellation_token = new CancellationTokenSource ();
		}
 
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;

				if (cancellation_token != null)
					cancellation_token.Dispose ();
			}
		}

		public HttpResponseMessage Delete (string requestUri)
		{
			return Send (new HttpRequestMessage (HttpMethod.Delete, requestUri));
		}

		public HttpResponseMessage Delete (Uri requestUri)
		{
			return Send (new HttpRequestMessage (HttpMethod.Delete, requestUri));
		}

		public HttpResponseMessage Get (string requestUri)
		{
			return Send (new HttpRequestMessage (HttpMethod.Get, requestUri));
		}

		public HttpResponseMessage Get (Uri requestUri)
		{
			return Send (new HttpRequestMessage (HttpMethod.Get, requestUri));
		}

		public HttpResponseMessage Post (string requestUri, HttpContent content)
		{
			return Send (new HttpRequestMessage (HttpMethod.Post, requestUri) { Content = content });
		}

		public HttpResponseMessage Put (Uri requestUri, HttpContent content)
		{
			return Send (new HttpRequestMessage (HttpMethod.Put, requestUri) { Content = content });
		}

		public HttpResponseMessage Put (string requestUri, HttpContent content)
		{
			return Send (new HttpRequestMessage (HttpMethod.Put, requestUri) { Content = content });
		}

		public HttpResponseMessage Post (Uri requestUri, HttpContent content)
		{
			return Send (new HttpRequestMessage (HttpMethod.Post, requestUri) { Content = content });
		}

		public HttpResponseMessage Send (HttpRequestMessage request)
		{
			return Send (request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
		}

		public HttpResponseMessage Send (HttpRequestMessage request, HttpCompletionOption completionOption)
		{
			return Send (request, completionOption, CancellationToken.None);
		}

		public HttpResponseMessage Send (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return Send (request, HttpCompletionOption.ResponseContentRead, cancellationToken);
		}

		public HttpResponseMessage Send (HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			if (request.SetIsUsed ())
				throw new InvalidOperationException ("Cannot send the same request message multiple times");

			if (request.RequestUri == null) {
				if (base_address == null)
					throw new InvalidOperationException ("The request URI must either be an absolute URI or BaseAddress must be set");

				request.RequestUri = base_address;
			}

			try {
				if (cancellation_token == null)
					cancellation_token = new CancellationTokenSource ();

				using (var cts = CancellationTokenSource.CreateLinkedTokenSource (cancellation_token.Token, cancellationToken)) {
					cts.CancelAfter (timeout);

					var response = handler.Send (request, cts.Token);
					if (response == null)
						throw new InvalidOperationException ("Handler failed to return a response");

					return response;
				}
			} finally {
				cancellation_token.Dispose ();
				cancellation_token = null;
			}
		}
	}
}
