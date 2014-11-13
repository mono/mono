//------------------------------------------------------------------------------
// <copyright file="ClientProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Net;
    using System.Net.Cache;
    using System.Threading;
    using System.Text;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Web.Services.Diagnostics;

    internal class ClientTypeCache {
        Hashtable cache = new Hashtable();

        internal object this[Type key] {
            get { return cache[key]; }
        }

        internal void Add(Type key, object value) {
            lock (this) {
                if (cache[key] == value) return;
                Hashtable clone = new Hashtable();
                foreach (object k in cache.Keys) {
                    clone.Add(k, cache[k]);
                }
                cache = clone;
                cache[key] = value;
            }
        }
    }

    /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol"]/*' />
    /// <devdoc>
    ///    <para>
    ///       Specifies the base class for all web service protocol client proxies that
    ///       use the HTTP protocol.
    ///    </para>
    /// </devdoc>
    [ComVisible(true)]
    public abstract class WebClientProtocol : Component {
        static AsyncCallback getRequestStreamAsyncCallback;
        static AsyncCallback getResponseAsyncCallback;

        // Double-checked locking pattern requires volatile for read/write synchronization
        static volatile AsyncCallback readResponseAsyncCallback;
        private static ClientTypeCache cache;
        private static RequestCachePolicy bypassCache;
        private ICredentials credentials;
        private bool preAuthenticate;
        private Uri uri;
        private int timeout;
        private string connectionGroupName;
        private Encoding requestEncoding;
        private RemoteDebugger debugger;
        private WebRequest pendingSyncRequest;
        object nullToken = new object();
        Hashtable asyncInvokes = Hashtable.Synchronized(new Hashtable());

        private static Object s_InternalSyncObject;
        internal static Object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        static WebClientProtocol() {
            cache = new ClientTypeCache();
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.WebClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected WebClientProtocol() {
            this.timeout = 100000; // should be kept in [....] with HttpWebRequest.Timeout default (see private WebRequest.DefaultTimeout)
        }

        internal WebClientProtocol(WebClientProtocol protocol) {
            this.credentials = protocol.credentials;
            this.uri = protocol.uri;
            this.timeout = protocol.timeout;
            this.connectionGroupName = protocol.connectionGroupName;
            this.requestEncoding = protocol.requestEncoding;
        }

        internal static RequestCachePolicy BypassCache {
            get {
                if (bypassCache == null) {
                    bypassCache = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                }
                return bypassCache;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.Credentials"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICredentials Credentials {
            get {
                return credentials;
            }
            set {
                credentials = value;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.UseDefaultCredentials"]/*' />
        /// <devdoc>
        ///    <para>Sets Credentials to CredentialCache.DefaultCredentials</para>
        /// </devdoc>
        public bool UseDefaultCredentials {
            get {
                return (credentials == CredentialCache.DefaultCredentials) ? true : false;
            }
            set {
                credentials = value ? CredentialCache.DefaultCredentials : null;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.ConnectionGroupName"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating the name of the connection group to use when making a request.
        ///    </para>
        /// </devdoc>
        [DefaultValue("")]
        public string ConnectionGroupName {
            get { return (connectionGroupName == null) ? string.Empty : connectionGroupName; }
            set { connectionGroupName = value; }
        }

        internal WebRequest PendingSyncRequest {
            get { return pendingSyncRequest; }
            set { pendingSyncRequest = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.PreAuthenticate"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether pre-authentication is enabled.
        ///    </para>
        /// </devdoc>
        [DefaultValue(false), WebServicesDescription(Res.ClientProtocolPreAuthenticate)]
        public bool PreAuthenticate {
            get { return preAuthenticate; }
            set { preAuthenticate = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.Url"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the base Uri to the server to use for requests.
        ///    </para>
        /// </devdoc>
        [DefaultValue(""), SettingsBindable(true), WebServicesDescription(Res.ClientProtocolUrl)]
        public string Url {
            get { return uri == null ? string.Empty : uri.ToString(); }
            set { uri = new Uri(value); }
        }

        internal Hashtable AsyncInvokes {
            get { return asyncInvokes; }
        }

        internal object NullToken {
            get { return nullToken; }
        }

        internal Uri Uri {
            get { return uri; }
            set { uri = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.RequestEncoding"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the encoding used for making the request.
        ///    </para>
        /// </devdoc>
        [DefaultValue(null), SettingsBindable(true), WebServicesDescription(Res.ClientProtocolEncoding)]
        public Encoding RequestEncoding {
            get { return requestEncoding; }
            set { requestEncoding = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.Timeout"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the timeout (in milliseconds) used for synchronous calls.
        ///    </para>
        /// </devdoc>
        [DefaultValue(100000), SettingsBindable(true), WebServicesDescription(Res.ClientProtocolTimeout)]
        public int Timeout {
            get { return timeout; }
            set { timeout = (value < Threading.Timeout.Infinite) ? Threading.Timeout.Infinite : value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.Abort"]/*' />
        public virtual void Abort() {
            WebRequest request = PendingSyncRequest;
            if (request != null)
                request.Abort();
        }

        /// <devdoc>
        ///    <para>
        ///     Starts async request processing including async retrieval of the request stream and response.
        ///     Derived classes can use BeginSend
        ///     to help implement their own higher level async methods like BeginInvoke. Derived
        ///     classes can add custom behavior by overriding GetWebRequest, GetWebResponse,
        ///     InitializeAsyncRequest and WriteAsyncRequest methods.
        ///    </para>
        /// </devdoc>
        internal IAsyncResult BeginSend(Uri requestUri, WebClientAsyncResult asyncResult, bool callWriteAsyncRequest) {
            if (readResponseAsyncCallback == null) {
                lock (InternalSyncObject) {
                    if (readResponseAsyncCallback == null) {
                        getRequestStreamAsyncCallback = new AsyncCallback(GetRequestStreamAsyncCallback);
                        getResponseAsyncCallback = new AsyncCallback(GetResponseAsyncCallback);
                        readResponseAsyncCallback = new AsyncCallback(ReadResponseAsyncCallback);
                    }
                }
            }
            Debug.Assert(asyncResult.Request == null, "calling GetWebRequest twice for the same WebClientAsyncResult");
            WebRequest request = GetWebRequest(requestUri);
            asyncResult.Request = request;
            InitializeAsyncRequest(request, asyncResult.InternalAsyncState);
            if (callWriteAsyncRequest)
                request.BeginGetRequestStream(getRequestStreamAsyncCallback, asyncResult);
            else
                request.BeginGetResponse(getResponseAsyncCallback, asyncResult);

            if (!asyncResult.IsCompleted)
                asyncResult.CombineCompletedSynchronously(false);
            return asyncResult;
        }

        static private void ProcessAsyncException(WebClientAsyncResult client, Exception e, string method) {
            if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, typeof(WebClientProtocol), method, e);
            WebException webException = e as WebException;
            if (webException != null && webException.Response != null) {
                client.Response = webException.Response;
            }
            else {
                // If we've already completed the call then the exception must have come
                // out of the user callback in which case we need to rethrow it here
                // so that it bubbles up to the AppDomain unhandled exception event.
                if (client.IsCompleted)
                    throw new InvalidOperationException(Res.GetString(Res.ThereWasAnErrorDuringAsyncProcessing), e);
                else
                    client.Complete(e);
            }
        }

        static private void GetRequestStreamAsyncCallback(IAsyncResult asyncResult) {
            WebClientAsyncResult client = (WebClientAsyncResult)asyncResult.AsyncState;
            client.CombineCompletedSynchronously(asyncResult.CompletedSynchronously);
            bool processingRequest = true;
            try {
                Stream requestStream = client.Request.EndGetRequestStream(asyncResult);
                processingRequest = false;
                try {
                    client.ClientProtocol.AsyncBufferedSerialize(client.Request, requestStream, client.InternalAsyncState);
                }
                finally {
                    requestStream.Close();
                }
                client.Request.BeginGetResponse(getResponseAsyncCallback, client);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                ProcessAsyncException(client, e, "GetRequestStreamAsyncCallback");
                if (processingRequest)
                {
                    WebException we = e as WebException;
                    if (we != null && we.Response != null)
                    {
                        // ProcessAsyncExcption doesn't call client.Complete() if there's a response,
                        // because it expects us to read the response. However, in certain cases
                        // (e.g. 502 errors), the exception thrown from Request can have a response.
                        // We don't process it, so call Complete() now.
                        client.Complete(e);
                    }
                }
            }
        }

        static private void GetResponseAsyncCallback(IAsyncResult asyncResult) {
            WebClientAsyncResult client = (WebClientAsyncResult)asyncResult.AsyncState;
            client.CombineCompletedSynchronously(asyncResult.CompletedSynchronously);
            try {
                client.Response = client.ClientProtocol.GetWebResponse(client.Request, asyncResult);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                ProcessAsyncException(client, e, "GetResponseAsyncCallback");
                if (client.Response == null)
                    return;
            }

            ReadAsyncResponse(client);
        }

        static private void ReadAsyncResponse(WebClientAsyncResult client) {
            if (client.Response.ContentLength == 0) {
                client.Complete();
                return;
            }
            try {
                client.ResponseStream = client.Response.GetResponseStream();
                ReadAsyncResponseStream(client);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                ProcessAsyncException(client, e, "ReadAsyncResponse");
            }
        }

        static private void ReadAsyncResponseStream(WebClientAsyncResult client) {
            IAsyncResult asyncResult;
            do {
                byte[] buffer = client.Buffer;
                long contentLength = client.Response.ContentLength;
                if (buffer == null)
                    buffer = client.Buffer = new byte[(contentLength == -1) ? 1024 : contentLength];
                else if (contentLength != -1 && contentLength > buffer.Length)
                    buffer = client.Buffer = new byte[contentLength];
                asyncResult = client.ResponseStream.BeginRead(buffer, 0, buffer.Length, readResponseAsyncCallback, client);
                if (!asyncResult.CompletedSynchronously)
                    return;
            }
            while (!ProcessAsyncResponseStreamResult(client, asyncResult));
        }

        static private bool ProcessAsyncResponseStreamResult(WebClientAsyncResult client, IAsyncResult asyncResult) {
            bool complete;
            int bytesRead = client.ResponseStream.EndRead(asyncResult);
            long contentLength = client.Response.ContentLength;
            if (contentLength > 0 && bytesRead == contentLength) {
                // the non-chunked response finished in a single read
                client.ResponseBufferedStream = new MemoryStream(client.Buffer);
                complete = true;
            }
            else if (bytesRead > 0) {
                if (client.ResponseBufferedStream == null) {
                    int capacity = (int)((contentLength == -1) ? client.Buffer.Length : contentLength);
                    client.ResponseBufferedStream = new MemoryStream(capacity);
                }
                client.ResponseBufferedStream.Write(client.Buffer, 0, bytesRead);
                complete = false;
            }
            else
                complete = true;

            if (complete)
                client.Complete();
            return complete;
        }

        static private void ReadResponseAsyncCallback(IAsyncResult asyncResult) {
            WebClientAsyncResult client = (WebClientAsyncResult)asyncResult.AsyncState;
            client.CombineCompletedSynchronously(asyncResult.CompletedSynchronously);
            if (asyncResult.CompletedSynchronously)
                return;
            try {
                bool complete = ProcessAsyncResponseStreamResult(client, asyncResult);
                if (!complete)
                    ReadAsyncResponseStream(client);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                ProcessAsyncException(client, e, "ReadResponseAsyncCallback");
            }
        }

        internal void NotifyClientCallOut(WebRequest request) {
            if (RemoteDebugger.IsClientCallOutEnabled()) {
                debugger = new RemoteDebugger();
                debugger.NotifyClientCallOut(request);
            }
            else {
                debugger = null;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.GetWebRequest"]/*' />
        /// <devdoc>
        ///    <para>
        ///     Creates a new <see cref='System.Net.WebRequest'/> instance for the given url. The base implementation creates a new
        ///     instance using the WebRequest.Create() and then sets request related properties from
        ///     the WebClientProtocol instance. Derived classes can override this method if additional
        ///     properties need to be set on the web request instance.
        ///    </para>
        /// </devdoc>
        protected virtual WebRequest GetWebRequest(Uri uri) {
            if (uri == null)
                throw new InvalidOperationException(Res.GetString(Res.WebMissingPath));
            WebRequest request = (WebRequest)WebRequest.Create(uri);
            PendingSyncRequest = request;
            request.Timeout = this.timeout;
            request.ConnectionGroupName = connectionGroupName;
            request.Credentials = Credentials;
            request.PreAuthenticate = PreAuthenticate;
            request.CachePolicy = BypassCache;
            return request;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.GetWebResponse"]/*' />
        /// <devdoc>
        ///    <para>
        ///     Gets the <see cref='System.Net.WebResponse'/> from the given request by calling
        ///     GetResponse(). Derived classes can override this method to do additional
        ///     processing on the response instance.
        ///    </para>
        /// </devdoc>
        protected virtual WebResponse GetWebResponse(WebRequest request) {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "GetWebResponse") : null;
            WebResponse response = null;
            try {
                if (Tracing.On) Tracing.Enter("WebRequest.GetResponse", caller, new TraceMethod(request, "GetResponse"));
                response = request.GetResponse();
                if (Tracing.On) Tracing.Exit("WebRequest.GetResponse", caller);
            }
            catch (WebException e) {
                if (e.Response == null)
                    throw e;
                else {
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "GetWebResponse", e);
                    response = e.Response;
                }
            }
            finally {
                if (debugger != null)
                    debugger.NotifyClientCallReturn(response);
            }
            return response;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.GetWebResponse1"]/*' />
        /// <devdoc>
        ///    <para>
        ///     Gets the <see cref='System.Net.WebResponse'/> from the given request by calling
        ///     EndGetResponse(). Derived classes can override this method to do additional
        ///     processing on the response instance. This method is only called during
        ///     async request processing.
        ///    </para>
        /// </devdoc>
        protected virtual WebResponse GetWebResponse(WebRequest request, IAsyncResult result) {
            WebResponse response = request.EndGetResponse(result);
            if (response != null && debugger != null)
                debugger.NotifyClientCallReturn(response);
            return response;
        }

        /// <devdoc>
        ///    <para>
        ///     Called during async request processing to give the derived class an opportunity
        ///     to modify the web request instance before the request stream is retrieved at which
        ///     point the request headers are sent and can no longer be modified. The base implementation
        ///     does nothing.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
        internal virtual void InitializeAsyncRequest(WebRequest request, object internalAsyncState) {
            return;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
        internal virtual void AsyncBufferedSerialize(WebRequest request, Stream requestStream, object internalAsyncState) {
            throw new NotSupportedException(Res.GetString(Res.ProtocolDoesNotAsyncSerialize));
        }

        internal WebResponse EndSend(IAsyncResult asyncResult, ref object internalAsyncState, ref Stream responseStream) {
            if (asyncResult == null) throw new ArgumentNullException(Res.GetString(Res.WebNullAsyncResultInEnd));

            WebClientAsyncResult client = (WebClientAsyncResult)asyncResult;
            if (client.EndSendCalled)
                throw new InvalidOperationException(Res.GetString(Res.CanTCallTheEndMethodOfAnAsyncCallMoreThan));
            client.EndSendCalled = true;
            WebResponse response = client.WaitForResponse();
            internalAsyncState = client.InternalAsyncState;
            responseStream = client.ResponseBufferedStream;
            return response;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.GetFromCache"]/*' />
        /// <devdoc>
        /// Returns an instance of a client protocol handler from the cache.
        /// </devdoc>
        protected static object GetFromCache(Type type) {
            return cache[type];
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientProtocol.AddToCache"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Add an instance of the client protocol handler to the cache.
        ///    </para>
        /// </devdoc>
        protected static void AddToCache(Type type, object value) {
            cache.Add(type, value);
        }

    }

    /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientAsyncResult"]/*' />
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class WebClientAsyncResult : IAsyncResult {
        private object userAsyncState;
        private bool completedSynchronously;
        private bool isCompleted;

        // Double-checked locking pattern requires volatile for read/write synchronization
        private volatile ManualResetEvent manualResetEvent;
        private AsyncCallback userCallback;

        internal WebClientProtocol ClientProtocol;
        internal object InternalAsyncState;
        internal Exception Exception;
        internal WebResponse Response;
        internal WebRequest Request;
        internal Stream ResponseStream;
        internal Stream ResponseBufferedStream;
        internal byte[] Buffer;
        internal bool EndSendCalled;

        internal WebClientAsyncResult(WebClientProtocol clientProtocol,
            object internalAsyncState,
            WebRequest request,
            AsyncCallback userCallback,
            object userAsyncState) 
        {
            this.ClientProtocol = clientProtocol;
            this.InternalAsyncState = internalAsyncState;
            this.userAsyncState = userAsyncState;
            this.userCallback = userCallback;
            this.Request = request;
            this.completedSynchronously = true;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientAsyncResult.AsyncState"]/*' />
        public object AsyncState { get { return userAsyncState; } }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientAsyncResult.AsyncWaitHandle"]/*' />
        public WaitHandle AsyncWaitHandle {
            get {
                bool savedIsCompleted = isCompleted;
                if (manualResetEvent == null) {
                    lock (this) {
                        if (manualResetEvent == null)
                            manualResetEvent = new ManualResetEvent(savedIsCompleted);
                    }
                }
                if (!savedIsCompleted && isCompleted)
                    manualResetEvent.Set();
                return (WaitHandle)manualResetEvent;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientAsyncResult.CompletedSynchronously"]/*' />
        public bool CompletedSynchronously {
            get { return completedSynchronously; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientAsyncResult.IsCompleted"]/*' />
        public bool IsCompleted { get { return isCompleted; } }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="WebClientAsyncResult.Abort"]/*' />
        public void Abort() {
            WebRequest req = Request;
            if (req != null)
                req.Abort();
        }


        internal void Complete() {
            Debug.Assert(!isCompleted, "Complete called more than once.");

            try {
                if (ResponseStream != null) {
                    ResponseStream.Close();
                    ResponseStream = null;
                }

                if (ResponseBufferedStream != null)
                    ResponseBufferedStream.Position = 0;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (this.Exception == null)
                    this.Exception = e;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "Complete", e);
            }

            isCompleted = true;

            try {
                if (manualResetEvent != null)
                    manualResetEvent.Set();
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (this.Exception == null)
                    this.Exception = e;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "Complete", e);
            }

            // We want to let exceptions in user callback to bubble up to
            // threadpool so that AppDomain.UnhandledExceptionEventHandler
            // will get it if one is registered
            if (userCallback != null)
                userCallback(this);
        }

        internal void Complete(Exception e) {
            this.Exception = e;
            Complete();
        }

        internal WebResponse WaitForResponse() {
            if (!isCompleted)
                AsyncWaitHandle.WaitOne();

            if (this.Exception != null)
                throw this.Exception;

            return Response;
        }

        internal void CombineCompletedSynchronously(bool innerCompletedSynchronously) {
            completedSynchronously = completedSynchronously && innerCompletedSynchronously;
        }
    }

    /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="InvokeCompletedEventHandler"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public delegate void InvokeCompletedEventHandler(object sender, InvokeCompletedEventArgs e);

    /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="InvokeCompletedEventArgs"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class InvokeCompletedEventArgs : AsyncCompletedEventArgs {
        object[] results;

        internal InvokeCompletedEventArgs(object[] results, Exception exception, bool cancelled, object userState) :
            base(exception, cancelled, userState) {
            this.results = results;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="InvokeCompletedEventArgs.Results"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the client should automatically follow server redirects.
        ///    </para>
        /// </devdoc>
        public object[] Results {
            get {
                return results;
            }
        }
    }


    internal class UserToken {
        SendOrPostCallback callback;
        object userState;

        internal UserToken(SendOrPostCallback callback, object userState) {
            this.callback = callback;
            this.userState = userState;
        }
        internal SendOrPostCallback Callback { get { return callback; } }
        internal object UserState { get { return userState; } }
    }

    /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [ComVisible(true)]
    public abstract class HttpWebClientProtocol : WebClientProtocol {
        private bool allowAutoRedirect;
        private bool enableDecompression = false;
        private CookieContainer cookieJar = null;
        private X509CertificateCollection clientCertificates;
        private IWebProxy proxy;
        private static string UserAgentDefault = "Mozilla/4.0 (compatible; MSIE 6.0; MS Web Services Client Protocol " + System.Environment.Version.ToString() + ")";
        private string userAgent;
        private bool unsafeAuthenticatedConnectionSharing;

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.HttpWebClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected HttpWebClientProtocol() : base() {
            this.allowAutoRedirect = false;
            this.userAgent = UserAgentDefault;
            // the right thing to do, for NetClasses to pick up the default
            // GlobalProxySelection settings, is to leave proxy to null
            // (which is the default initialization value)
            // rather than picking up GlobalProxySelection.Select
            // which will never change.
        }

        // used by SoapHttpClientProtocol.Discover
        internal HttpWebClientProtocol(HttpWebClientProtocol protocol)
            : base(protocol) {
            this.allowAutoRedirect  = protocol.allowAutoRedirect;
            this.enableDecompression  = protocol.enableDecompression;
            this.cookieJar          = protocol.cookieJar;
            this.clientCertificates = protocol.clientCertificates;
            this.proxy              = protocol.proxy;
            this.userAgent          = protocol.userAgent;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.AllowAutoRedirect"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the client should automatically follow server redirects.
        ///    </para>
        /// </devdoc>
        [DefaultValue(false), WebServicesDescription(Res.ClientProtocolAllowAutoRedirect)]
        public bool AllowAutoRedirect {
            get {
                return allowAutoRedirect;
            }

            set {
                allowAutoRedirect = value;
            }
        }


        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.CookieContainer"]/*' />
        [DefaultValue(null), WebServicesDescription(Res.ClientProtocolCookieContainer)]
        public CookieContainer CookieContainer {
            get {
                return cookieJar;
            }
            set {
                cookieJar = value;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.ClientCertificates"]/*' />
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.ClientProtocolClientCertificates)]
        public X509CertificateCollection ClientCertificates {
            get {
                if (clientCertificates == null) {
                    clientCertificates = new X509CertificateCollection();
                }
                return clientCertificates;
            }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.EnableDecompression"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the client should automatically follow server redirects.
        ///    </para>
        /// </devdoc>
        [DefaultValue(false), WebServicesDescription(Res.ClientProtocolEnableDecompression)]
        public bool EnableDecompression {
            get {
                return enableDecompression;
            }

            set {
                enableDecompression = value;
            }
        }

         /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.UserAgent"]/*' />
         /// <devdoc>
         ///    <para>
         ///       Gets or sets the value for the user agent header that is
         ///       sent with each request.
         ///    </para>
         /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), WebServicesDescription(Res.ClientProtocolUserAgent)]
        public string UserAgent {
            get { return (userAgent == null) ? string.Empty : userAgent; }
            set { userAgent = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.Proxy"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets or sets the name of the proxy server to use for requests.
        ///    </para>
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IWebProxy Proxy {
            get { return proxy; }
            set { proxy = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.GetWebRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override WebRequest GetWebRequest(Uri uri) {
            WebRequest request = base.GetWebRequest(uri);
            HttpWebRequest httpRequest = request as HttpWebRequest;
            if (httpRequest != null) {
                httpRequest.UserAgent = UserAgent;
                httpRequest.AllowAutoRedirect = allowAutoRedirect;
                httpRequest.AutomaticDecompression = enableDecompression ? DecompressionMethods.GZip : DecompressionMethods.None;
                httpRequest.AllowWriteStreamBuffering = true;
                httpRequest.SendChunked = false;
                if (unsafeAuthenticatedConnectionSharing != httpRequest.UnsafeAuthenticatedConnectionSharing)
                    httpRequest.UnsafeAuthenticatedConnectionSharing = unsafeAuthenticatedConnectionSharing;
                // if the user has set a proxy explictly then we need to
                // propagate that to the WebRequest, otherwise we'll let NetClasses
                // use their global setting (GlobalProxySelection.Select).
                if (proxy != null) {
                    httpRequest.Proxy = proxy;
                }
                if (clientCertificates != null && clientCertificates.Count > 0) {
                    httpRequest.ClientCertificates.AddRange(clientCertificates);
                }
                httpRequest.CookieContainer = cookieJar;
            }
            return request;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.GetWebResponse"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override WebResponse GetWebResponse(WebRequest request) {
            WebResponse response = base.GetWebResponse(request);
            return response;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.GetWebResponse1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result) {
            WebResponse response = base.GetWebResponse(request, result);
            return response;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.UnsafeAuthenticatedConnectionSharing"]/*' />
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UnsafeAuthenticatedConnectionSharing {
            get { return unsafeAuthenticatedConnectionSharing; }
            set { unsafeAuthenticatedConnectionSharing = value; }
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="ClientProtocol.CancelInvokeAsync"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void CancelAsync(object userState) {
            if (userState == null)
                userState = NullToken;
            WebClientAsyncResult result = OperationCompleted(userState, new object[] { null }, null, true);
            if (result != null) {
                result.Abort();
            }
        }

        internal WebClientAsyncResult OperationCompleted(object userState, object[] parameters, Exception e, bool canceled) {
            Debug.Assert(userState != null, "We should not call OperationCompleted with null user token.");
            WebClientAsyncResult result = (WebClientAsyncResult)AsyncInvokes[userState];
            if (result != null) {
                AsyncOperation asyncOp = (AsyncOperation)result.AsyncState;
                UserToken token = (UserToken)asyncOp.UserSuppliedState;
                InvokeCompletedEventArgs eventArgs = new InvokeCompletedEventArgs(parameters, e, canceled, userState);
                AsyncInvokes.Remove(userState);
                asyncOp.PostOperationCompleted(token.Callback, eventArgs);
            }
            return result;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.GenerateXmlMappings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool GenerateXmlMappings(Type type, ArrayList mappings) {
            if (typeof(SoapHttpClientProtocol).IsAssignableFrom(type)) {
                WebServiceBindingAttribute binding = WebServiceBindingReflector.GetAttribute(type);
                if (binding == null)
                    throw new InvalidOperationException(Res.GetString(Res.WebClientBindingAttributeRequired));
                // Note: Service namespace is taken from WebserviceBindingAttribute and not WebserviceAttribute because
                // the generated proxy does not have a WebServiceAttribute; however all have a WebServiceBindingAttribute. 
                string serviceNamespace = binding.Namespace;
                bool serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(type);
                ArrayList soapMethodList = new ArrayList();
                SoapClientType.GenerateXmlMappings(type, soapMethodList, serviceNamespace, serviceDefaultIsEncoded, mappings);
                return true;
            }
            return false;
        }

        /// <include file='doc\ClientProtocol.uex' path='docs/doc[@for="HttpWebClientProtocol.GenerateXmlMappings1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static Hashtable GenerateXmlMappings(Type[] types, ArrayList mappings) {
            if (types == null)
                throw new ArgumentNullException("types");

            Hashtable mappedTypes = new Hashtable();
            foreach (Type type in types) {
                ArrayList typeMappings = new ArrayList();
                if (GenerateXmlMappings(type, mappings)) {
                    mappedTypes.Add(type, typeMappings);
                    mappings.Add(typeMappings);
                }
            }
            return mappedTypes;
        }
    }
}
