
using System.Threading;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;

namespace System.Net.Http
{
	public class HttpClient : HttpMessageInvoker
	{
		const string EXCEPTION_MESSAGE = "System.Net.Http.HttpClientHandler is not supported on the current platform.";

		public HttpClient ()
			: base (CreateDefaultHandler ())
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		static HttpMessageHandler CreateDefaultHandler () => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);

		public HttpClient (HttpMessageHandler handler)
			: this (handler, true)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public HttpClient (HttpMessageHandler handler, bool disposeHandler)
			: base (handler, disposeHandler)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Uri BaseAddress {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public HttpRequestHeaders DefaultRequestHeaders {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public long MaxResponseContentBufferSize {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TimeSpan Timeout {
			get => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			set => throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void CancelPendingRequests ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> DeleteAsync (string requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> DeleteAsync (string requestUri, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> DeleteAsync (Uri requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> DeleteAsync (Uri requestUri, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri, HttpCompletionOption completionOption)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri, HttpCompletionOption completionOption)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> GetAsync (Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PostAsync (string requestUri, HttpContent content)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PostAsync (string requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PostAsync (Uri requestUri, HttpContent content)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PostAsync (Uri requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PutAsync (Uri requestUri, HttpContent content)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PutAsync (Uri requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PutAsync (string requestUri, HttpContent content)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> PutAsync (string requestUri, HttpContent content, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> SendAsync (HttpRequestMessage request)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, HttpCompletionOption completionOption)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<byte[]> GetByteArrayAsync (string requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<byte[]> GetByteArrayAsync (Uri requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<Stream> GetStreamAsync (string requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<Stream> GetStreamAsync (Uri requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<string> GetStringAsync (string requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<string> GetStringAsync (Uri requestUri)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		// NS2.1 methods, added here while CoreFX HttpClient PR is not merged
		public Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content) => throw new PlatformNotSupportedException();
		public Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content, CancellationToken cancellationToken) => throw new PlatformNotSupportedException();
		public Task<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent content) => throw new PlatformNotSupportedException();
		public Task<HttpResponseMessage> PatchAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken) => throw new PlatformNotSupportedException();
	}
}
