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
using System.Threading.Tasks;
using System.IO;

namespace System.Net.Http
{
	public partial class HttpClient : HttpMessageInvoker
	{
		static readonly TimeSpan TimeoutDefault = TimeSpan.FromSeconds (100);

		Uri base_address;
		CancellationTokenSource cts;
		bool disposed;
		HttpRequestHeaders headers;
		long buffer_size;
		TimeSpan timeout;

#if !XAMARIN_MODERN && !WASM
		public HttpClient ()
			: this (new HttpClientHandler (), true)
		{
		}
#endif
		
		public HttpClient (HttpMessageHandler handler)
			: this (handler, true)
		{
		}

		public HttpClient (HttpMessageHandler handler, bool disposeHandler)
			: base (handler, disposeHandler)
		{
			buffer_size = int.MaxValue;
			timeout = TimeoutDefault;
			cts = new CancellationTokenSource ();
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

		public long MaxResponseContentBufferSize {
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
				if (value != System.Threading.Timeout.InfiniteTimeSpan && (value <= TimeSpan.Zero || value.TotalMilliseconds > int.MaxValue))
					throw new ArgumentOutOfRangeException ();

				timeout = value;
			}
		}

		public void CancelPendingRequests ()
		{
			// Cancel only any already running requests not any new request after this cancellation
			using (var c = Interlocked.Exchange (ref cts, new CancellationTokenSource ()))
				c.Cancel ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				disposed = true;

				//We don't use CancelPendingRequests() because we don't want to create new CancellationTokenSource
				cts.Cancel ();
				cts.Dispose ();
			}
			
			base.Dispose (disposing);
		}

		public Task<HttpResponseMessage> DeleteAsync (string requestUri)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Delete, requestUri));
		}

		public Task<HttpResponseMessage> DeleteAsync (string requestUri, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Delete, requestUri), cancellationToken);
		}

		public Task<HttpResponseMessage> DeleteAsync (Uri requestUri)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Delete, requestUri));
		}

		public Task<HttpResponseMessage> DeleteAsync (Uri requestUri, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Delete, requestUri), cancellationToken);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri));
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri), cancellationToken);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri, HttpCompletionOption completionOption)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri), completionOption);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri), completionOption, cancellationToken);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri));
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri), cancellationToken);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri, HttpCompletionOption completionOption)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri), completionOption);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Get, requestUri), completionOption, cancellationToken);
		}

		public Task<HttpResponseMessage> PostAsync (string requestUri, HttpContent content)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Post, requestUri) { Content = content });
		}

		public Task<HttpResponseMessage> PostAsync (string requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Post, requestUri) { Content = content }, cancellationToken);
		}

		public Task<HttpResponseMessage> PostAsync (Uri requestUri, HttpContent content)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Post, requestUri) { Content = content });
		}

		public Task<HttpResponseMessage> PostAsync (Uri requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Post, requestUri) { Content = content }, cancellationToken);
		}

		public Task<HttpResponseMessage> PutAsync (Uri requestUri, HttpContent content)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Put, requestUri) { Content = content });
		}

		public Task<HttpResponseMessage> PutAsync (Uri requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Put, requestUri) { Content = content }, cancellationToken);
		}

		public Task<HttpResponseMessage> PutAsync (string requestUri, HttpContent content)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Put, requestUri) { Content = content });
		}

		public Task<HttpResponseMessage> PutAsync (string requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			return SendAsync (new HttpRequestMessage (HttpMethod.Put, requestUri) { Content = content }, cancellationToken);
		}

		public Task<HttpResponseMessage> SendAsync (HttpRequestMessage request)
		{
			return SendAsync (request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
		}

		public Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, HttpCompletionOption completionOption)
		{
			return SendAsync (request, completionOption, CancellationToken.None);
		}

		public override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			return SendAsync (request, HttpCompletionOption.ResponseContentRead, cancellationToken);
		}

		public Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			if (request == null)
				throw new ArgumentNullException ("request");

			if (request.SetIsUsed ())
				throw new InvalidOperationException ("Cannot send the same request message multiple times");

			var uri = request.RequestUri;
			if (uri == null) {
				if (base_address == null)
					throw new InvalidOperationException ("The request URI must either be an absolute URI or BaseAddress must be set");

				request.RequestUri = base_address;
			} else if (!uri.IsAbsoluteUri || uri.Scheme == Uri.UriSchemeFile && uri.OriginalString.StartsWith ("/", StringComparison.Ordinal)) {
				if (base_address == null)
					throw new InvalidOperationException ("The request URI must either be an absolute URI or BaseAddress must be set");

				request.RequestUri = new Uri (base_address, uri);
			}

			if (headers != null) {
				request.Headers.AddHeaders (headers);
			}

			return SendAsyncWorker (request, completionOption, cancellationToken);
		}

		async Task<HttpResponseMessage> SendAsyncWorker (HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			using (var lcts = CancellationTokenSource.CreateLinkedTokenSource (cts.Token, cancellationToken)) {
				// Hack to pass the timeout to the HttpWebRequest that's created by MonoWebRequestHandler; all other handlers ignore this.
				if (handler is HttpClientHandler clientHandler)
					clientHandler.SetWebRequestTimeout (timeout);
				lcts.CancelAfter (timeout);

				var task = base.SendAsync (request, lcts.Token);
				if (task == null)
					throw new InvalidOperationException ("Handler failed to return a value");
					
				var response = await task.ConfigureAwait (false);
				if (response == null)
					throw new InvalidOperationException ("Handler failed to return a response");

				//
				// Read the content when default HttpCompletionOption.ResponseContentRead is set
				//
				if (response.Content != null && (completionOption & HttpCompletionOption.ResponseHeadersRead) == 0) {
					await response.Content.LoadIntoBufferAsync (MaxResponseContentBufferSize).ConfigureAwait (false);
				}
					
				return response;
			}
		}

		public async Task<byte[]> GetByteArrayAsync (string requestUri)
		{
			using (var resp = await GetAsync (requestUri, HttpCompletionOption.ResponseContentRead).ConfigureAwait (false)) {
				resp.EnsureSuccessStatusCode ();
				return await resp.Content.ReadAsByteArrayAsync ().ConfigureAwait (false);
			}
		}

		public async Task<byte[]> GetByteArrayAsync (Uri requestUri)
		{
			using (var resp = await GetAsync (requestUri, HttpCompletionOption.ResponseContentRead).ConfigureAwait (false)) {
				resp.EnsureSuccessStatusCode ();
				return await resp.Content.ReadAsByteArrayAsync ().ConfigureAwait (false);
			}
		}

		public async Task<Stream> GetStreamAsync (string requestUri)
		{
			var resp = await GetAsync (requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait (false);
			resp.EnsureSuccessStatusCode ();
			return await resp.Content.ReadAsStreamAsync ().ConfigureAwait (false);
		}

		public async Task<Stream> GetStreamAsync (Uri requestUri)
		{
			var resp = await GetAsync (requestUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait (false);
			resp.EnsureSuccessStatusCode ();
			return await resp.Content.ReadAsStreamAsync ().ConfigureAwait (false);
		}

		public async Task<string> GetStringAsync (string requestUri)
		{
			using (var resp = await GetAsync (requestUri, HttpCompletionOption.ResponseContentRead).ConfigureAwait (false)) {
				resp.EnsureSuccessStatusCode ();
				return await resp.Content.ReadAsStringAsync ().ConfigureAwait (false);
			}
		}

		public async Task<string> GetStringAsync (Uri requestUri)
		{
			using (var resp = await GetAsync (requestUri, HttpCompletionOption.ResponseContentRead).ConfigureAwait (false)) {
				resp.EnsureSuccessStatusCode ();
				return await resp.Content.ReadAsStringAsync ().ConfigureAwait (false);
			}
		}

		// NS2.1 methods, added here while CoreFX HttpClient PR is not merged
		public Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content) => throw new PlatformNotSupportedException();
		public Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content, CancellationToken cancellationToken) => throw new PlatformNotSupportedException();
		public Task<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent content) => throw new PlatformNotSupportedException();
		public Task<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken) => throw new PlatformNotSupportedException();
	}
}
