using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebAssembly.Net.Http.HttpClient
{
    public class WasmHttpMessageHandler : HttpMessageHandler
    {
        static JSObject json;
        static JSObject fetch;
        static JSObject window;
        static JSObject global;

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

        static WasmHttpMessageHandler()
        {
            StreamingSupported = Runtime.InvokeJS("'body' in Response.prototype && typeof ReadableStream === 'function'") == "true";
        }

        public WasmHttpMessageHandler()
        {
            handlerInit();
        }

        private void handlerInit()
        {
            window = (JSObject)WebAssembly.Runtime.GetGlobalObject("window");
            json = (JSObject)WebAssembly.Runtime.GetGlobalObject("JSON");
            fetch = (JSObject)WebAssembly.Runtime.GetGlobalObject("fetch");

            // install our global hook to create a Headers object.
            Runtime.InvokeJS(@"
                BINDING.mono_wasm_get_global()[""__mono_wasm_headers_hook__""] = function () { return new Headers(); }
                BINDING.mono_wasm_get_global()[""__mono_wasm_abortcontroller_hook__""] = function () { return new AbortController(); }
            ");

            global = (JSObject)WebAssembly.Runtime.GetGlobalObject("");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                #pragma warning disable 4014
                doFetch(tcs, request, cancellationToken).ConfigureAwait(false);
                #pragma warning restore 4014

                return await tcs.Task;
            }
        }

        private async Task doFetch(TaskCompletionSource<HttpResponseMessage> tcs, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var requestObject = (JSObject)json.Invoke("parse", "{}");
                requestObject.SetObjectProperty("method", request.Method.Method);

                // See https://developer.mozilla.org/en-US/docs/Web/API/Request/credentials for
                // standard values and meanings
                requestObject.SetObjectProperty("credentials", DefaultCredentials);

                // See https://developer.mozilla.org/en-US/docs/Web/API/Request/cache for
                // standard values and meanings
                requestObject.SetObjectProperty("cache", Cache);

                // See https://developer.mozilla.org/en-US/docs/Web/API/Request/mode for
                // standard values and meanings
                requestObject.SetObjectProperty("mode", Mode);

                // We need to check for body content
                if (request.Content != null)
                {
                    if (request.Content is StringContent)
                    {
                        requestObject.SetObjectProperty("body", await request.Content.ReadAsStringAsync());
                    }
                    else
                    {
                        requestObject.SetObjectProperty("body", await request.Content.ReadAsByteArrayAsync());
                    }
                }

                // Process headers
                // Cors has it's own restrictions on headers.
                // https://developer.mozilla.org/en-US/docs/Web/API/Headers
                var requestHeaders = GetHeadersAsStringArray(request);

                if (requestHeaders != null && requestHeaders.Length > 0)
                {
                    using (var jsHeaders = (JSObject)global.Invoke("__mono_wasm_headers_hook__"))
                    {
                        for (int i = 0; i < requestHeaders.Length; i++)
                        {
                            //Console.WriteLine($"append: {requestHeaders[i][0]} / {requestHeaders[i][1]}");
                            jsHeaders.Invoke("append", requestHeaders[i][0], requestHeaders[i][1]);
                        }
                        requestObject.SetObjectProperty("headers", jsHeaders);
                    }
                }

                JSObject abortController = null;
                JSObject signal = null;
                WasmHttpReadStream wasmHttpReadStream = null;

                CancellationTokenRegistration abortRegistration = default(CancellationTokenRegistration);
                if (cancellationToken.CanBeCanceled)
                {
                    abortController = (JSObject)global.Invoke("__mono_wasm_abortcontroller_hook__");
                    signal = (JSObject)abortController.GetObjectProperty("signal");
                    requestObject.SetObjectProperty("signal", signal);
                    abortRegistration = cancellationToken.Register(() =>
                    {
                        if (abortController.JSHandle != -1)
                        {
                            abortController.Invoke("abort");
                            abortController?.Dispose();
                        }
                        wasmHttpReadStream?.Dispose();
                    });
                }

                var args = (JSObject)json.Invoke("parse", "[]");
                args.Invoke("push", request.RequestUri.ToString());
                args.Invoke("push", requestObject);

                requestObject.Dispose();

                var response = (Task<object>)fetch.Invoke("apply", window, args);
                args.Dispose();

                var t = await response;

                var status = new WasmFetchResponse((JSObject)t, abortController, abortRegistration);

                //Console.WriteLine($"bodyUsed: {status.IsBodyUsed}");
                //Console.WriteLine($"ok: {status.IsOK}");
                //Console.WriteLine($"redirected: {status.IsRedirected}");
                //Console.WriteLine($"status: {status.Status}");
                //Console.WriteLine($"statusText: {status.StatusText}");
                //Console.WriteLine($"type: {status.ResponseType}");
                //Console.WriteLine($"url: {status.Url}");

                HttpResponseMessage httpresponse = new HttpResponseMessage((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), status.Status.ToString()));

                httpresponse.Content = StreamingSupported && StreamingEnabled
                    ? new StreamContent(wasmHttpReadStream = new WasmHttpReadStream(status))
                    : (HttpContent)new WasmHttpContent(status);

                // Fill the response headers
                // CORS will only allow access to certain headers.
                // If a request is made for a resource on another origin which returns the CORs headers, then the type is cors.
                // cors and basic responses are almost identical except that a cors response restricts the headers you can view to
                // `Cache-Control`, `Content-Language`, `Content-Type`, `Expires`, `Last-Modified`, and `Pragma`.
                // View more information https://developers.google.com/web/updates/2015/03/introduction-to-fetch#response_types
                //
                // Note: Some of the headers may not even be valid header types in .NET thus we use TryAddWithoutValidation
                using (var respHeaders = status.Headers)
                {
                    // Here we invoke the forEach on the headers object
                    // Note: the Action takes 3 objects and not two.  The other seems to be the Header object.
                    respHeaders.Invoke("forEach", new Action<object, object, object>((value, name, other) =>
                    {

                        if (!httpresponse.Headers.TryAddWithoutValidation((string)name, (string)value))
                            if (httpresponse.Content != null)
                                if (!httpresponse.Content.Headers.TryAddWithoutValidation((string)name, (string)value))
                                    Console.WriteLine($"Warning: Can not add response header for name: {name} value: {value}");
                        ((JSObject)other).Dispose();
                    }
                    ));
                }

                tcs.SetResult(httpresponse);

                // Do not remove the following line of code.  The httpresponse is used in the lambda above when parsing the Headers.
                // if a local is captured (used) by a lambda it becomes heap memory as we translate them into fields on an object.
                // If we do not null the field out it will not be GC'd
                httpresponse = null;

                signal?.Dispose();
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
            }
        }

        private string[][] GetHeadersAsStringArray(HttpRequestMessage request)
            => (from header in request.Headers.Concat(request.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                from headerValue in header.Value // There can be more than one value for each name
                select new[] { header.Key, headerValue }).ToArray();

        class WasmFetchResponse : IDisposable
        {
            private JSObject fetchResponse;
            private JSObject abortController;
            private readonly CancellationTokenRegistration abortRegistration;

            public WasmFetchResponse(JSObject fetchResponse, JSObject abortController, CancellationTokenRegistration abortRegistration)
            {
                this.fetchResponse = fetchResponse;
                this.abortController = abortController;
                this.abortRegistration = abortRegistration;
            }

            public bool IsOK => (bool)fetchResponse.GetObjectProperty("ok");
            public bool IsRedirected => (bool)fetchResponse.GetObjectProperty("redirected");
            public int Status => (int)fetchResponse.GetObjectProperty("status");
            public string StatusText => (string)fetchResponse.GetObjectProperty("statusText");
            public string ResponseType => (string)fetchResponse.GetObjectProperty("type");
            public string Url => (string)fetchResponse.GetObjectProperty("url");
            //public bool IsUseFinalURL => (bool)managedJSObject.GetObjectProperty("useFinalUrl");
            public bool IsBodyUsed => (bool)fetchResponse.GetObjectProperty("bodyUsed");
            public JSObject Headers => (JSObject)fetchResponse.GetObjectProperty("headers");
            public JSObject Body => (JSObject)fetchResponse.GetObjectProperty("body");

            public Task<object> ArrayBuffer() => (Task<object>)fetchResponse.Invoke("arrayBuffer");
            public Task<object> Text() => (Task<object>)fetchResponse.Invoke("text");
            public Task<object> JSON() => (Task<object>)fetchResponse.Invoke("json");

            public void Dispose()
            {
                // Dispose of unmanaged resources.
                Dispose(true);
                // Suppress finalization.
                GC.SuppressFinalize(this);
            }

            // Protected implementation of Dispose pattern.
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Free any other managed objects here.
                    //
                    abortRegistration.Dispose();
                }

                // Free any unmanaged objects here.
                //
                fetchResponse?.Dispose();
                fetchResponse = null;

                abortController?.Dispose();
                abortController = null;
            }

        }

        class WasmHttpContent : HttpContent
        {
            byte[] _data;
            WasmFetchResponse _status;

            public WasmHttpContent(WasmFetchResponse status)
            {
                _status = status;
            }

            private async Task<byte[]> GetResponseData()
            {
                if (_data != null)
                {
                    return _data;
                }

                _data = (byte[])await _status.ArrayBuffer();
                _status.Dispose();
                _status = null;

                return _data;
            }

            protected override async Task<Stream> CreateContentReadStreamAsync()
            {
                var data = await GetResponseData();
                return new MemoryStream(data, writable: false);
            }

            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                var data = await GetResponseData();
                await stream.WriteAsync(data, 0, data.Length);
            }

            protected override bool TryComputeLength(out long length)
            {
                if (_data != null)
                {
                    length = _data.Length;
                    return true;
                }

                length = 0;
                return false;
            }

            protected override void Dispose(bool disposing)
            {
                _status?.Dispose();
                base.Dispose(disposing);
            }
        }

        class WasmHttpReadStream : Stream
        {
            WasmFetchResponse _status;
            JSObject _reader;

            byte[] _bufferedBytes;
            int _position;

            public WasmHttpReadStream(WasmFetchResponse status)
            {
                _status = status;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }
                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                if (count < 0 || buffer.Length - offset < count)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                if (_reader == null)
                {
                    // If we've read everything, then _reader and _status will be null
                    if (_status == null)
                    {
                        return 0;
                    }

                    using (var body = _status.Body)
                    {
                        _reader = (JSObject)body.Invoke("getReader");
                    }
                }

                if (_bufferedBytes != null && _position < _bufferedBytes.Length)
                {
                    return ReadBuffered();
                }

                var t = (Task<object>)_reader.Invoke("read");
                using (var read = (JSObject)await t)
                {
                    if ((bool)read.GetObjectProperty("done"))
                    {
                        _reader.Dispose();
                        _reader = null;

                        _status.Dispose();
                        _status = null;
                        return 0;
                    }

                    _position = 0;
                    _bufferedBytes = (byte[])read.GetObjectProperty("value");
                }

                return ReadBuffered();

                int ReadBuffered()
                {
                    int n = _bufferedBytes.Length - _position;
                    if (n > count)
                        n = count;
                    if (n <= 0)
                        return 0;

                    Buffer.BlockCopy(_bufferedBytes, _position, buffer, offset, n);
                    _position += n;

                    return n;
                }
            }

            protected override void Dispose(bool disposing)
            {
                _reader?.Dispose();
                _status?.Dispose();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new PlatformNotSupportedException("Synchronous reads are not supported, use ReadAsync instead");
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }

    }

    /// <summary>
    /// Specifies a value for the 'credentials' option on outbound HTTP requests.
    /// </summary>
    public enum FetchCredentialsOption
    {
        /// <summary>
        /// Advises the browser never to send credentials (such as cookies or HTTP auth headers).
        /// </summary>
        [Export(EnumValue = ConvertEnum.ToLower)]
        Omit,

        /// <summary>
        /// Advises the browser to send credentials (such as cookies or HTTP auth headers)
        /// only if the target URL is on the same origin as the calling application.
        /// </summary>
        [Export("same-origin")]
        SameOrigin,

        /// <summary>
        /// Advises the browser to send credentials (such as cookies or HTTP auth headers)
        /// even for cross-origin requests.
        /// </summary>
        [Export(EnumValue = ConvertEnum.ToLower)]
        Include,
    }


    /// <summary>
    /// The cache mode of the request. It controls how the request will interact with the browser's HTTP cache.
    /// </summary>
    public enum RequestCache
    {
        /// <summary>
        /// The browser looks for a matching request in its HTTP cache.
        /// </summary>
        [Export(EnumValue = ConvertEnum.ToLower)]
        Default,

        /// <summary>
        /// The browser fetches the resource from the remote server without first looking in the cache,
        /// and will not update the cache with the downloaded resource.
        /// </summary>
        [Export("no-store")]
        NoStore,

        /// <summary>
        /// The browser fetches the resource from the remote server without first looking in the cache,
        /// but then will update the cache with the downloaded resource.
        /// </summary>
        [Export(EnumValue = ConvertEnum.ToLower)]
        Reload,

        /// <summary>
        /// The browser looks for a matching request in its HTTP cache.
        /// </summary>
        [Export("no-cache")]
        NoCache,

        /// <summary>
        /// The browser looks for a matching request in its HTTP cache.
        /// </summary>
        [Export("force-cache")]
        ForceCache,

        /// <summary>
        /// The browser looks for a matching request in its HTTP cache.
        /// Mode can only be used if the request's mode is "same-origin"
        /// </summary>
        [Export("only-if-cached")]
        OnlyIfCached,
    }

    /// <summary>
    /// The mode of the request. This is used to determine if cross-origin requests lead to valid responses
    /// </summary>
    public enum RequestMode
    {
        /// <summary>
        /// If a request is made to another origin with this mode set, the result is simply an error
        /// </summary>
        [Export("same-origin")]
        SameOrigin,

        /// <summary>
        /// Prevents the method from being anything other than HEAD, GET or POST, and the headers from
        /// being anything other than simple headers.
        /// </summary>
        [Export("no-cors")]
        NoCors,

        /// <summary>
        /// Allows cross-origin requests, for example to access various APIs offered by 3rd party vendors.
        /// </summary>
        [Export(EnumValue = ConvertEnum.ToLower)]
        Cors,

        /// <summary>
        /// A mode for supporting navigation.
        /// </summary>
        [Export(EnumValue = ConvertEnum.ToLower)]
        Navigate,
    }

}
