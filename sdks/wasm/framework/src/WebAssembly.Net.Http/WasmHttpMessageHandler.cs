using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using WebAssembly.Core;
using WebAssembly.Host;

namespace WebAssembly.Net.Http.HttpClient {
	public class WasmHttpMessageHandler : HttpMessageHandler {
		static JSObject fetch;
		static JSObject window;

		/// <summary>
		/// Gets or sets the default value of the 'credentials' option on outbound HTTP requests.
		/// Defaults to <see cref="FetchCredentialsOption.SameOrigin"/>.
		/// </summary>
		public static FetchCredentialsOption DefaultCredentials { get; set; }
		    = FetchCredentialsOption.SameOrigin;

		public static RequestCache Cache { get; set; }
		    = RequestCache.Default;

		public static RequestMode Mode { get; set; }
		    = RequestMode.Cors;


		/// <summary>
		/// Gets whether the current Browser supports streaming responses
		/// </summary>
		public static bool StreamingSupported { get; }

		/// <summary>
		/// Gets or sets whether responses should be streamed if supported
		/// </summary>
		public static bool StreamingEnabled { get; set; } = true;

		static WasmHttpMessageHandler ()
		{
			using (var streamingSupported = new Function ("return 'body' in Response.prototype && typeof ReadableStream === 'function'"))
				StreamingSupported = (bool)streamingSupported.Call ();
		}

		public WasmHttpMessageHandler ()
		{
			handlerInit ();
		}

		private static WasmHttpMessageHandler GetHttpMessageHandler ()
		{
			return new WasmHttpMessageHandler ();
		}

		private void handlerInit ()
		{
			window = (JSObject)WebAssembly.Runtime.GetGlobalObject ("window");
			fetch = (JSObject)WebAssembly.Runtime.GetGlobalObject ("fetch");
		}

		protected override async Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<HttpResponseMessage> ();
			using (cancellationToken.Register (() => tcs.TrySetCanceled ())) {
#pragma warning disable 4014
				doFetch (tcs, request, cancellationToken).ConfigureAwait (false);
#pragma warning restore 4014

				return await tcs.Task;
			}
		}

		private async Task doFetch (TaskCompletionSource<HttpResponseMessage> tcs, HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try {
				var requestObject = new JSObject ();
				requestObject.SetObjectProperty ("method", request.Method.Method);

				// See https://developer.mozilla.org/en-US/docs/Web/API/Request/credentials for
				// standard values and meanings
				requestObject.SetObjectProperty ("credentials", DefaultCredentials);

				// See https://developer.mozilla.org/en-US/docs/Web/API/Request/cache for
				// standard values and meanings
				requestObject.SetObjectProperty ("cache", Cache);

				// See https://developer.mozilla.org/en-US/docs/Web/API/Request/mode for
				// standard values and meanings
				requestObject.SetObjectProperty ("mode", Mode);

				// We need to check for body content
				if (request.Content != null) {
					if (request.Content is StringContent) {
						requestObject.SetObjectProperty ("body", await request.Content.ReadAsStringAsync ());
					} else {
						using (var uint8Buffer = Uint8Array.From(await request.Content.ReadAsByteArrayAsync ()))
						{
							requestObject.SetObjectProperty ("body", uint8Buffer);
						}
					}
				}

				// Process headers
				// Cors has it's own restrictions on headers.
				// https://developer.mozilla.org/en-US/docs/Web/API/Headers
				using (var jsHeaders = new HostObject ("Headers")) {
					if (request.Headers != null) {
						foreach (var header in request.Headers) {
							foreach (var value in header.Value) {
								jsHeaders.Invoke ("append", header.Key, value);
							}
						}
					}
					if (request.Content?.Headers != null) {
						foreach (var header in request.Content.Headers) {
							foreach (var value in header.Value) {
								jsHeaders.Invoke ("append", header.Key, value);
							}
						}
					}
					requestObject.SetObjectProperty ("headers", jsHeaders);
				}

				JSObject abortController = null;
				JSObject signal = null;
				WasmHttpReadStream wasmHttpReadStream = null;

				CancellationTokenRegistration abortRegistration = default (CancellationTokenRegistration);
				if (cancellationToken.CanBeCanceled) {

					abortController = new HostObject ("AbortController");

					signal = (JSObject)abortController.GetObjectProperty ("signal");
					requestObject.SetObjectProperty ("signal", signal);
					abortRegistration = cancellationToken.Register ((Action)(() => {
						if (abortController.JSHandle != -1) {
							abortController.Invoke ((string)"abort");
							abortController?.Dispose ();
						}
						wasmHttpReadStream?.Dispose ();
					}));
				}

				var args = new Core.Array();
				args.Push (request.RequestUri.ToString ());
				args.Push (requestObject);

				requestObject.Dispose ();

				var response = (Task<object>)fetch.Invoke ("apply", window, args);
				args.Dispose ();

				var t = await response;

				var status = new WasmFetchResponse ((JSObject)t, abortController, abortRegistration);

				//Console.WriteLine($"bodyUsed: {status.IsBodyUsed}");
				//Console.WriteLine($"ok: {status.IsOK}");
				//Console.WriteLine($"redirected: {status.IsRedirected}");
				//Console.WriteLine($"status: {status.Status}");
				//Console.WriteLine($"statusText: {status.StatusText}");
				//Console.WriteLine($"type: {status.ResponseType}");
				//Console.WriteLine($"url: {status.Url}");

				HttpResponseMessage httpresponse = new HttpResponseMessage ((HttpStatusCode)Enum.Parse (typeof (HttpStatusCode), status.Status.ToString ()));

				httpresponse.Content = StreamingSupported && StreamingEnabled
				    ? new StreamContent (wasmHttpReadStream = new WasmHttpReadStream (status))
				    : (HttpContent)new WasmHttpContent (status);

				// Fill the response headers
				// CORS will only allow access to certain headers.
				// If a request is made for a resource on another origin which returns the CORs headers, then the type is cors.
				// cors and basic responses are almost identical except that a cors response restricts the headers you can view to
				// `Cache-Control`, `Content-Language`, `Content-Type`, `Expires`, `Last-Modified`, and `Pragma`.
				// View more information https://developers.google.com/web/updates/2015/03/introduction-to-fetch#response_types
				//
				// Note: Some of the headers may not even be valid header types in .NET thus we use TryAddWithoutValidation
				using (var respHeaders = new Headers(status.Headers)) {
					foreach (DictionaryEntry entry in respHeaders) {
						var name = (string)entry.Key;
						var value = (string)entry.Value;
						if (!httpresponse.Headers.TryAddWithoutValidation (name, value))
							if (httpresponse.Content != null)
								if (!httpresponse.Content.Headers.TryAddWithoutValidation (name, value))
									Console.WriteLine ($"Warning: Can not add response header for name: {name} value: {value}");
					}
				}

				tcs.SetResult (httpresponse);

				signal?.Dispose ();
			} catch (Exception exception) {
				tcs.SetException (exception);
			}
		}

		class WasmFetchResponse : IDisposable {
			private JSObject fetchResponse;
			private JSObject abortController;
			private readonly CancellationTokenRegistration abortRegistration;

			public WasmFetchResponse (JSObject fetchResponse, JSObject abortController, CancellationTokenRegistration abortRegistration)
			{
				this.fetchResponse = fetchResponse;
				this.abortController = abortController;
				this.abortRegistration = abortRegistration;
			}

			public bool IsOK => (bool)fetchResponse.GetObjectProperty ("ok");
			public bool IsRedirected => (bool)fetchResponse.GetObjectProperty ("redirected");
			public int Status => (int)fetchResponse.GetObjectProperty ("status");
			public string StatusText => (string)fetchResponse.GetObjectProperty ("statusText");
			public string ResponseType => (string)fetchResponse.GetObjectProperty ("type");
			public string Url => (string)fetchResponse.GetObjectProperty ("url");
			//public bool IsUseFinalURL => (bool)managedJSObject.GetObjectProperty("useFinalUrl");
			public bool IsBodyUsed => (bool)fetchResponse.GetObjectProperty ("bodyUsed");
			public JSObject Headers => (JSObject)fetchResponse.GetObjectProperty ("headers");
			public JSObject Body => (JSObject)fetchResponse.GetObjectProperty ("body");

			public Task<object> ArrayBuffer () => (Task<object>)fetchResponse.Invoke ("arrayBuffer");
			public Task<object> Text () => (Task<object>)fetchResponse.Invoke ("text");
			public Task<object> JSON () => (Task<object>)fetchResponse.Invoke ("json");

			public void Dispose ()
			{
				// Dispose of unmanaged resources.
				Dispose (true);
				// Suppress finalization.
				GC.SuppressFinalize (this);
			}

			// Protected implementation of Dispose pattern.
			protected virtual void Dispose (bool disposing)
			{
				if (disposing) {
					// Free any other managed objects here.
					//
					abortRegistration.Dispose ();
				}

				// Free any unmanaged objects here.
				//
				fetchResponse?.Dispose ();
				fetchResponse = null;

				abortController?.Dispose ();
				abortController = null;
			}

		}

		class WasmHttpContent : HttpContent {
			byte [] _data;
			WasmFetchResponse _status;

			public WasmHttpContent (WasmFetchResponse status)
			{
				_status = status;
			}

			private async Task<byte []> GetResponseData ()
			{
				if (_data != null) {
					return _data;
				}

				using (ArrayBuffer dataBuffer = (ArrayBuffer)await _status.ArrayBuffer ()) {
					using (Uint8Array dataBinView = new Uint8Array(dataBuffer)) {
						_data = dataBinView.ToArray();
						_status.Dispose ();
						_status = null;

					}
				}

				return _data;
			}

			protected override async Task<Stream> CreateContentReadStreamAsync ()
			{
				var data = await GetResponseData ();
				return new MemoryStream (data, writable: false);
			}

			protected override async Task SerializeToStreamAsync (Stream stream, TransportContext context)
			{
				var data = await GetResponseData ();
				await stream.WriteAsync (data, 0, data.Length);
			}

			protected override bool TryComputeLength (out long length)
			{
				if (_data != null) {
					length = _data.Length;
					return true;
				}

				length = 0;
				return false;
			}

			protected override void Dispose (bool disposing)
			{
				_status?.Dispose ();
				base.Dispose (disposing);
			}
		}

		class WasmHttpReadStream : Stream {
			WasmFetchResponse _status;
			JSObject _reader;

			byte [] _bufferedBytes;
			int _position;

			public WasmHttpReadStream (WasmFetchResponse status)
			{
				_status = status;
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException ();
			public override long Position {
				get => throw new NotSupportedException ();
				set => throw new NotSupportedException ();
			}

			public override async Task<int> ReadAsync (byte [] buffer, int offset, int count, CancellationToken cancellationToken)
			{
				if (buffer == null) {
					throw new ArgumentNullException (nameof (buffer));
				}
				if (offset < 0) {
					throw new ArgumentOutOfRangeException (nameof (offset));
				}
				if (count < 0 || buffer.Length - offset < count) {
					throw new ArgumentOutOfRangeException (nameof (count));
				}

				if (_reader == null) {
					// If we've read everything, then _reader and _status will be null
					if (_status == null) {
						return 0;
					}

					using (var body = _status.Body) {
						_reader = (JSObject)body.Invoke ("getReader");
					}
				}

				if (_bufferedBytes != null && _position < _bufferedBytes.Length) {
					return ReadBuffered ();
				}

				var t = (Task<object>)_reader.Invoke ("read");
				using (var read = (JSObject)await t) {
					if ((bool)read.GetObjectProperty ("done")) {
						_reader.Dispose ();
						_reader = null;

						_status.Dispose ();
						_status = null;
						return 0;
					}

					_position = 0;
					// value for fetch streams is a Uint8Array
					using (Uint8Array binValue = (Uint8Array)read.GetObjectProperty ("value"))
						_bufferedBytes = binValue.ToArray ();
				}

				return ReadBuffered ();

				int ReadBuffered ()
				{
					int n = _bufferedBytes.Length - _position;
					if (n > count)
						n = count;
					if (n <= 0)
						return 0;

					Buffer.BlockCopy (_bufferedBytes, _position, buffer, offset, n);
					_position += n;

					return n;
				}
			}

			protected override void Dispose (bool disposing)
			{
				_reader?.Dispose ();
				_status?.Dispose ();
			}

			public override void Flush ()
			{
			}

			public override int Read (byte [] buffer, int offset, int count)
			{
				throw new PlatformNotSupportedException ("Synchronous reads are not supported, use ReadAsync instead");
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotSupportedException ();
			}

			public override void SetLength (long value)
			{
				throw new NotSupportedException ();
			}

			public override void Write (byte [] buffer, int offset, int count)
			{
				throw new NotSupportedException ();
			}
		}

		private sealed class Headers : IDictionary, IDisposable {

			public readonly JSObject HeadersObject; // Wrapped object

			internal Headers (JSObject jSObject)
			{
				HeadersObject = jSObject;
			}

			public bool IsFixedSize => false;

			public bool IsReadOnly => false;

			public ICollection Keys {
				get {
					return new HeadersKeyValueCollection (this, "keys");
				}
			}

			public ICollection Values {
				get {
					return new HeadersKeyValueCollection (this, "values");
				}

			}

			public int Count => (int)HeadersObject.GetObjectProperty ("size");

			public bool IsSynchronized => false;

			public object SyncRoot => false;

			public void Add (object key, object value)
			{
				HeadersObject.Invoke ("set", key, value);
			}

			public void Clear () => HeadersObject.Invoke ("clear");

			public bool Contains (object key) => (bool)HeadersObject.Invoke ("has", key);

			public IDictionaryEnumerator GetEnumerator ()
			{
				// Construct and return an enumerator.
				return new HeadersEnumerator (this);
			}

			public void Remove (object key) => HeadersObject.Invoke ("delete", key);

			public void CopyTo (System.Array array, int index) => throw new NotImplementedException ();

			IEnumerator IEnumerable.GetEnumerator ()
			{
				// Construct and return an enumerator.
				return ((IDictionary)this).GetEnumerator ();
			}

			public object this [object key] {
				get {
					return HeadersObject.Invoke ("get", key);
				}
				set {
					HeadersObject.Invoke ("set", key, value);
				}
			}

			#region IDisposable Support
			private bool disposedValue = false; // To detect redundant calls

			void Dispose (bool disposing)
			{
				if (!disposedValue) {
					if (disposing) {
						// TODO: dispose managed state (managed objects).
					}

					// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
					// TODO: set large fields to null.
					HeadersObject?.Dispose ();
					disposedValue = true;
				}
			}

			// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
			~Headers ()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose (false);
			}

			// This code added to correctly implement the disposable pattern.
			public void Dispose ()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				Dispose (true);
				// TODO: uncomment the following line if the finalizer is overridden above.
				GC.SuppressFinalize(this);
			}
			#endregion

			private sealed class HeadersEnumerator : IDictionaryEnumerator, IDisposable {
				JSObject headersIterator;
				readonly Headers headers;
				public HeadersEnumerator (Headers headers)
				{
					this.headers = headers;
				}

				// Return the current item.
				public object Current => new DictionaryEntry (Key, Value);

				// Return the current dictionary entry.
				public DictionaryEntry Entry {
					get { return (DictionaryEntry)Current; }
				}

				// Return the key of the current item.
				public object Key { get; private set; }

				// Return the value of the current item.
				public object Value { get; private set; }

				// Advance to the next item.
				public bool MoveNext ()
				{
					if (headersIterator == null)
						headersIterator = (JSObject)headers.HeadersObject.Invoke ("entries");

					using (var result = (JSObject)headersIterator.Invoke ("next")) {
						using (var resultValue = (Core.Array)result.GetObjectProperty ("value")) {
							if (resultValue != null) {
								Key = resultValue [0];
								Value = resultValue [1];
							} else {
								Key = null;
								Value = null;
							}
						}
						return !(bool)result.GetObjectProperty ("done");
					}
				}

				// Reset the index to restart the enumeration.
				public void Reset ()
				{
					headersIterator?.Dispose ();
					headersIterator = null;
				}

				#region IDisposable Support
				private bool disposedValue = false; // To detect redundant calls

				void Dispose (bool disposing)
				{
					if (!disposedValue) {
						if (disposing) {
							// TODO: dispose managed state (managed objects).
						}

						// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
						// TODO: set large fields to null.
						headersIterator?.Dispose ();
						headersIterator = null;
						disposedValue = true;
					}
				}

				//TODO: override a finalizer only if Dispose (bool disposing) above has code to free unmanaged resources.
				~HeadersEnumerator ()
				{
					// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
					Dispose (false);
				}

				// This code added to correctly implement the disposable pattern.
				public void Dispose ()
				{
					// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
					Dispose (true);
					// TODO: uncomment the following line if the finalizer is overridden above.
					GC.SuppressFinalize (this);
				}
				#endregion
			}

			private sealed class HeadersKeyValueCollection : ICollection {
				readonly Headers headers;
				readonly string iterator;  // "keys" or "values"

				public HeadersKeyValueCollection (Headers headers, string iterator)
				{
					this.headers = headers;
					this.iterator = iterator;

				}
				public int Count => headers.Count;

				public bool IsSynchronized => false;

				public object SyncRoot => this;

				public void CopyTo (System.Array array, int index)
				{
					throw new NotImplementedException ();
				}

				public IEnumerator GetEnumerator ()
				{
					// Construct and return an enumerator.
					return new KeyValueEnumerator (this);
				}

				private sealed class KeyValueEnumerator : IEnumerator {

					readonly HeadersKeyValueCollection keyValueCollection;
					JSObject keyValueIterator;

					public object Current { get; private set; }

					public KeyValueEnumerator (HeadersKeyValueCollection mapCollection)
					{

						keyValueCollection = mapCollection;

					}

					public bool MoveNext ()
					{
						if (keyValueIterator == null)
							keyValueIterator = (JSObject)keyValueCollection.headers.HeadersObject.Invoke (keyValueCollection.iterator);

						var done = false;
						using (var result = (JSObject)keyValueIterator.Invoke ("next")) {
							done = (bool)result.GetObjectProperty ("done");
							if (!done)
								Current = result.GetObjectProperty ("value");
							return !done;
						}
					}

					public void Reset ()
					{
						keyValueIterator?.Dispose ();
						keyValueIterator = null;
					}

					#region IDisposable Support
					private bool disposedValue = false; // To detect redundant calls

					void Dispose (bool disposing)
					{
						if (!disposedValue) {
							if (disposing) {
								// TODO: dispose managed state (managed objects).
							}

							// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
							// TODO: set large fields to null.
							keyValueIterator?.Dispose ();
							keyValueIterator = null;
							disposedValue = true;
						}
					}

					//TODO: override a finalizer only if Dispose (bool disposing) above has code to free unmanaged resources.
					~KeyValueEnumerator ()
					{
						// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
						Dispose (false);
					}

					// This code added to correctly implement the disposable pattern.
					public void Dispose ()
					{
						// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
						Dispose (true);
						// TODO: uncomment the following line if the finalizer is overridden above.
						GC.SuppressFinalize (this);
					}
					#endregion
				}
			}

		}
	}

	/// <summary>
	/// Specifies a value for the 'credentials' option on outbound HTTP requests.
	/// </summary>
	public enum FetchCredentialsOption {
		/// <summary>
		/// Advises the browser never to send credentials (such as cookies or HTTP auth headers).
		/// </summary>
		[Export (EnumValue = ConvertEnum.ToLower)]
		Omit,

		/// <summary>
		/// Advises the browser to send credentials (such as cookies or HTTP auth headers)
		/// only if the target URL is on the same origin as the calling application.
		/// </summary>
		[Export ("same-origin")]
		SameOrigin,

		/// <summary>
		/// Advises the browser to send credentials (such as cookies or HTTP auth headers)
		/// even for cross-origin requests.
		/// </summary>
		[Export (EnumValue = ConvertEnum.ToLower)]
		Include,
	}


	/// <summary>
	/// The cache mode of the request. It controls how the request will interact with the browser's HTTP cache.
	/// </summary>
	public enum RequestCache {
		/// <summary>
		/// The browser looks for a matching request in its HTTP cache.
		/// </summary>
		[Export (EnumValue = ConvertEnum.ToLower)]
		Default,

		/// <summary>
		/// The browser fetches the resource from the remote server without first looking in the cache,
		/// and will not update the cache with the downloaded resource.
		/// </summary>
		[Export ("no-store")]
		NoStore,

		/// <summary>
		/// The browser fetches the resource from the remote server without first looking in the cache,
		/// but then will update the cache with the downloaded resource.
		/// </summary>
		[Export (EnumValue = ConvertEnum.ToLower)]
		Reload,

		/// <summary>
		/// The browser looks for a matching request in its HTTP cache.
		/// </summary>
		[Export ("no-cache")]
		NoCache,

		/// <summary>
		/// The browser looks for a matching request in its HTTP cache.
		/// </summary>
		[Export ("force-cache")]
		ForceCache,

		/// <summary>
		/// The browser looks for a matching request in its HTTP cache.
		/// Mode can only be used if the request's mode is "same-origin"
		/// </summary>
		[Export ("only-if-cached")]
		OnlyIfCached,
	}

	/// <summary>
	/// The mode of the request. This is used to determine if cross-origin requests lead to valid responses
	/// </summary>
	public enum RequestMode {
		/// <summary>
		/// If a request is made to another origin with this mode set, the result is simply an error
		/// </summary>
		[Export ("same-origin")]
		SameOrigin,

		/// <summary>
		/// Prevents the method from being anything other than HEAD, GET or POST, and the headers from
		/// being anything other than simple headers.
		/// </summary>
		[Export ("no-cors")]
		NoCors,

		/// <summary>
		/// Allows cross-origin requests, for example to access various APIs offered by 3rd party vendors.
		/// </summary>
		[Export (EnumValue = ConvertEnum.ToLower)]
		Cors,

		/// <summary>
		/// A mode for supporting navigation.
		/// </summary>
		[Export (EnumValue = ConvertEnum.ToLower)]
		Navigate,
	}
	
}
