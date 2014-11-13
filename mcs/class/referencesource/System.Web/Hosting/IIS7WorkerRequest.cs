//------------------------------------------------------------------------------
// <copyright file="IIS7WorkerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Security;
    using System.Web.Util;
    using System.Web.WebSockets;

    using IIS = UnsafeIISMethods;

    internal sealed class IIS7WorkerRequest : HttpWorkerRequest {

        // In http.h, Translate is 39 and User-Agent is 40, but in WorkerRequest.cs Translate is unknown and User-Agent is 39
        private const int IisHeaderTranslate = 39;
        private const string IisHeaderTranslateName = "Translate";
        private const int IisHeaderUserAgent = 40;
        private const int IisRequestHeaderMaximum = 41;

        private static readonly Version IIS_VERSION_70 = new Version(7, 0);
        private static readonly Version IIS_VERSION_75 = new Version(7, 5);
        private static readonly Version IIS_VERSION_80 = new Version(8, 0);

        // unless otherwise noted all pointers here
        // are not ref'counted and do not need cleanup
        private IntPtr _context;  // W3_MGD_HANDLER *
        private IntPtr _pCookedUrl; // HTTP_COOKED_URL *
        private Encoding _headerEncoding = Encoding.UTF8;
        private AsyncResultBase _asyncResultBase;
        private int _contentType;
        private int _contentTotalLength;
        //private int _queryStringLength;
        private string _appPath;
        private string _appPathTranslated;
        private string _path;
        private string _queryString;
        private string _filePath;
        private string _pathInfo;
        private string _pathTranslated;
        private string _httpVerb;
        private bool _rewriteNotifyDisabled;
        private bool _rebaseClientPath;
        private bool _requestHeadersAvailable;
        private String[][] _unknownRequestHeaders;
        private String[] _knownRequestHeaders;
        private int         _cachedResponseBodyLength;
        private ArrayList   _cachedResponseBodyBytes;
        #pragma warning disable 0649
        private bool _preloadedLengthRead;
        private int  _preloadedLength;
        private bool _preloadedContentRead;
        private byte[] _preloadedContent;
        #pragma warning restore 0649
        private string _cacheUrl;

        private const int CONTENT_NONE = 0;
        private const int CONTENT_FORM = 1;
        private const int CONTENT_MULTIPART = 2;
        private const int CONTENT_OTHER = 3;
        private const int MIN_ASYNC_SIZE = 2048;

        private volatile static bool s_AuthenticationChecked;
        private static bool s_AuthenticationEnabled;

        private static readonly char[] s_ColonOrNL = { ':', '\n'};

        private Guid    _traceId;   // ETW traceId
        private bool    _traceEnabled;
        private bool    _connectionClosed;
        private bool    _disconnected;
        private bool    _headersSent;
        private bool    _trySkipIisCustomErrors;

        private bool      _clientCertFetched;
        private DateTime  _clientCertValidFrom;
        private DateTime  _clientCertValidUntil;
        private byte []   _clientCert;
        private int       _clientCertEncoding;
        private byte []   _clientCertPublicKey;
        private byte []   _clientCertBinaryIssuer;
        private ChannelBinding   _channelBindingToken;

        private readonly object _disposeLockObj = new object();
        private CancellationTokenHelper _clientDisconnectTokenHelper;

        internal IIS7WorkerRequest(IntPtr requestContext, bool etwProviderEnabled) {

            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL);

            if ( IntPtr.Zero == requestContext ) {
                throw new ArgumentNullException("requestContext");
            }

            _context    = requestContext;
            _traceEnabled = etwProviderEnabled;

            if (_traceEnabled) {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.IIS7_INTEGRATED, requestContext);

                // 


                IIS.MgdGetRequestTraceGuid(_context, out _traceId);

                if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_APPDOMAIN_ENTER, this, Thread.GetDomain().FriendlyName);
            }
        }

        internal IntPtr RequestContext {
            get {
                if ( _context == IntPtr.Zero ) {
                    return IntPtr.Zero;
                }

                return _context;
            }
        }

        internal void ReadRequestBasics() {
            IntPtr pathTranslatedBuffer;
            int pathTranslatedBufferSize;
            IntPtr cacheUrl;
            int cacheUrlBufferSize;
            IntPtr httpMethodPtr;
            IntPtr pCookedUrl;
            int hr = IIS.MgdGetRequestBasics(_context, out _contentType, out _contentTotalLength, out pathTranslatedBuffer, out pathTranslatedBufferSize, out cacheUrl, out cacheUrlBufferSize, out httpMethodPtr, out pCookedUrl);
            Misc.ThrowIfFailedHr(hr);

            _cacheUrl = (cacheUrlBufferSize <= 0) ? null : StringUtil.StringFromWCharPtr(cacheUrl, cacheUrlBufferSize);
            _pathTranslated = (pathTranslatedBufferSize <= 0) ? String.Empty : StringUtil.StringFromWCharPtr(pathTranslatedBuffer, pathTranslatedBufferSize);

            // We used to get the HTTP verb lazily, but pretty much every application depends on it, so
            // we may as well just get it now and save the later p/invoke.
            if (httpMethodPtr != IntPtr.Zero) {
                unsafe { _httpVerb = new String((sbyte*)httpMethodPtr); } // don't use the Marshal class since we want to fail if we're given a bad value
            }
            _pCookedUrl = pCookedUrl;

            // path-info is the trailing part of the URL, after the script name, but before the query string (if any).
            // E.g., if the URL is "/test.aspx/Something", then path-info is "/Something"

            _path = GetUriPathInternal(true /*includePathInfo*/, false /*useParentContext*/);  // includes path-info
            _filePath = GetUriPathInternal(false /*includePathInfo*/, false /*useParentContext*/); // does not include path-info

            // set _pathInfo and adjust _pathTranslated so it does not include path-info
            int lengthDiff = _path.Length - _filePath.Length;
            if (lengthDiff > 0) {
                _pathInfo = _path.Substring(_filePath.Length);
                int pathTranslatedLength = _pathTranslated.Length - lengthDiff;
                if (pathTranslatedLength > 0) {
                    _pathTranslated = _pathTranslated.Substring(0, pathTranslatedLength);
                }
            }
            else {
                _filePath = _path;
                _pathInfo = String.Empty;
            }

            _queryString = GetQueryString();
        }

        internal static IIS7WorkerRequest CreateWorkerRequest(IntPtr requestContext, bool etwProviderEnabled) {
            IIS7WorkerRequest req = new IIS7WorkerRequest(requestContext, etwProviderEnabled);

            if ( null != req ) {
                Debug.Trace("ClientUrl", " *********** NEW REQUEST ****************");
                req.Initialize();
            }

            return req;
        }

        internal void InitAppVars() {
            IntPtr virtAddr;
            IntPtr physAddr;
            int virtLen, physLen;

            int hr = IIS.MgdGetApplicationInfo(_context,
                                               out virtAddr,
                                               out virtLen,
                                               out physAddr,
                                               out physLen);

            if ( hr < 0 ) {
                throw new HttpException(
                    SR.GetString(SR.Cannot_retrieve_request_data));
            }
            else {
                unsafe {
                    _appPath = StringUtil.StringFromWCharPtr(virtAddr, virtLen);
                    _appPathTranslated = StringUtil.StringFromWCharPtr(physAddr, physLen);
                }

                if ( _appPathTranslated != null &&
                     _appPathTranslated.Length > 2 &&
                     !StringUtil.StringEndsWith(_appPathTranslated, '\\') )
                    // IIS 6.0 doesn't add the trailing '\'
                    _appPathTranslated += "\\";
            }

        }

        internal void Initialize() {
            // setup basic values
            ReadRequestBasics();
            InitAppVars();
        }

        internal void Dispose() {
            CancellationTokenHelper clientDisconnectHelper;
            IntPtr originalContext;

            //  The native request context will be disposed separately,
            //  but we need to stop holding on to it to prevent attempts
            //  to call it
            lock (_disposeLockObj) {
                originalContext = _context;
                _context = IntPtr.Zero;
                _pCookedUrl = IntPtr.Zero;
                clientDisconnectHelper = _clientDisconnectTokenHelper;
            }

            // At this point, _clientDisconnectTokenHelper can never be set since
            // we cleared the _context field in the above lock. But we still need
            // to disable this helper's registration on the unmanaged side;
            // otherwise the unmanaged code might eventually try to call into our
            // AppDomain during or after unload, which has dire consequences.
            if (clientDisconnectHelper != null) {
                if (originalContext != IntPtr.Zero) {
                    bool dummy;
                    int hr = IIS.MgdConfigureAsyncDisconnectNotification(originalContext, false /* fEnable */, out dummy);
                    Debug.Assert(hr == 0); // also ignore the HRESULT, as we can't do anything about it right now

                    // Due to the critical section in MGD_CONNECTION_STORED_CONTEXT, we're guaranteed that
                    // at this point in program execution any async disconnect notifications received by
                    // our unmanaged code for this request will not try to enter our AppDomain.
                }
                clientDisconnectHelper.Dispose();
            }

            if (_channelBindingToken != null && !_channelBindingToken.IsInvalid)
                _channelBindingToken.Dispose();
        }

        internal override void RaiseTraceEvent(IntegratedTraceType traceType, string eventData) {
            if (_traceEnabled && _context != IntPtr.Zero) {
                // the area is derivative of the type, either page or module
                int areaFlag = (traceType < IntegratedTraceType.DiagCritical) ? EtwTraceFlags.Page : EtwTraceFlags.Module;
                if (EtwTrace.IsTraceEnabled(EtwTrace.InferVerbosity(traceType), areaFlag)) {
                    string message = String.IsNullOrEmpty(eventData) ? String.Empty : eventData;
                    IIS.MgdEmitSimpleTrace(_context, (int)traceType, message);
                }
            }
        }

        internal override void RaiseTraceEvent(WebBaseEvent webEvent) {
            if (_traceEnabled && _context != IntPtr.Zero) {
                if (EtwTrace.IsTraceEnabled(webEvent.InferEtwTraceVerbosity(), EtwTraceFlags.Infrastructure)) {
                    int fieldCount;
                    string[] fieldNames;
                    int[] fieldTypes;
                    string[] fieldData;
                    int webEventType;
                    webEvent.DeconstructWebEvent(out webEventType, out fieldCount, out fieldNames, out fieldTypes, out fieldData);
                    IIS.MgdEmitWebEventTrace(_context, webEventType, fieldCount, fieldNames, fieldTypes, fieldData);
                }
            }
        }

        internal string GetUriPathInternal(bool includePathInfo, bool useParentContext) {
            string uri = String.Empty;

            IntPtr buffer;
            int bufSize;

            int hr = IIS.MgdGetUriPath(_context,
                                       out buffer,
                                       out bufSize,
                                       includePathInfo,
                                       useParentContext);

            if (hr < 0) {
                throw new HttpException(SR.GetString(SR.Cannot_retrieve_request_data));
            }

            // is there anything to copy?  if not, just use String.Empty
            if (bufSize > 0) {
                uri = StringUtil.StringFromWCharPtr(buffer, bufSize);
            }

            return uri;
        }

        // HttpWorkerRequest overrides
        public override string GetUriPath() {
            return _path;
        }

        // Returns a pointer corresponding to the part of the query string
        // immediately following the ?. Returns a char*, length is size in chars.
        private unsafe IntPtr GetQueryStringPtr(out int length) {
            HTTP_COOKED_URL* cookedUrl = GetCookedUrl();

            if (cookedUrl->pQueryString != null) {
                int lengthInChars = checked(cookedUrl->QueryStringLength / sizeof(char));
                if (lengthInChars > 1 /* the ? char */) {
                    length = lengthInChars - 1;
                    return (IntPtr)(&cookedUrl->pQueryString[1]);
                }
            }

            // No query string or didn't contain any value after the '?'
            length = 0;
            return IntPtr.Zero;
        }

        public override string GetQueryString() {
            int len;
            IntPtr buffer = GetQueryStringPtr(out len);

            if (buffer == IntPtr.Zero) {
                return String.Empty;
            }
            else {
                return StringUtil.StringFromWCharPtr(buffer, len);
            }
        }

        internal string GetCacheUrl() {
            return _cacheUrl;
        }

        public override string GetRawUrl() {
            // CACHE_URL is the original URI, unaffected by any rewriting or routing
            // that may have occurred on the server
            string rawUrl = GetRawUrlHelper(_cacheUrl);
            Debug.Trace("ClientUrl", "*** GetRawUrl --> " + rawUrl + " ***");
            return rawUrl;
        }

        public override string GetHttpVerbName() {
            return _httpVerb;
        }

        public override string GetHttpVersion() {
            return GetServerVariable("SERVER_PROTOCOL");
        }

        public override string GetRemoteAddress() {
            return GetServerVariable("REMOTE_ADDR");
        }

        public override string GetRemoteName() {
            return GetServerVariable("REMOTE_HOST");
        }

        public override int GetRemotePort() {
            return IIS.MgdGetRemotePort(_context);
        }

        public override string GetLocalAddress() {
            return GetServerVariable("LOCAL_ADDR");
        }

        public override int GetLocalPort() {
            return IIS.MgdGetLocalPort(_context);
        }

        public override string GetServerName() {
            return GetServerVariable("SERVER_NAME");
        }

        internal override String GetLocalPortAsString()
        {
            return GetServerVariable("SERVER_PORT");
        }

        // not implemented
        public override bool IsSecure() {
            String https = GetServerVariable("HTTPS");
            return(https != null && https.Equals("on"));
        }

        public override String GetFilePath() {
            return _filePath;
        }

        public override string GetFilePathTranslated() {
            return _pathTranslated;
        }

        public override string GetPathInfo() {
            return _pathInfo;
        }

        public override string GetAppPath() {
            return _appPath;
        }

        public override string GetAppPathTranslated() {
            return _appPathTranslated;
        }

        // CODEWORK
        // if this is called before the execute state
        // it defeats reloading
        public override int GetPreloadedEntityBodyLength() {
            if( !_preloadedLengthRead ) {
                int availSize = 0;
                int hresult = IIS.MgdGetPreloadedSize(_context, out availSize);
                Misc.ThrowIfFailedHr(hresult);
                _preloadedLength = availSize;
                _preloadedLengthRead = true;
            }
            return _preloadedLength;
        }

        public override byte[] GetPreloadedEntityBody() {
            if (!_preloadedContentRead) {
                int size = GetPreloadedEntityBodyLength();
                if (size > 0) {
                    byte[] buffer = new byte[ size ];
                    
                    // dont care about size this time
                    // this will throw if it fails
                    GetPreloadedContentInternal(buffer, 0 /*offset*/, size);
                    _preloadedContent = buffer;
                }
                _preloadedContentRead = true;
            }
            return _preloadedContent;
        }

        // force read of entity body
        public override bool IsEntireEntityBodyIsPreloaded() {
            return GetTotalEntityBodyLength() ==
                   GetPreloadedEntityBodyLength();
        }

        // 

        public override int GetTotalEntityBodyLength() {
            return _contentTotalLength;
        }

        private int ReadEntityCoreSync(byte[] buffer, int offset, int size) {
            bool fAsync = false;
            int bytesRead = 0;
            IntPtr ppAsyncReceiveBuffer;
            int result = 0;

            try {
                // Acquire blocking call
                IsInReadEntitySync = true;  

                result = IIS.MgdReadEntityBody(_context,
                                               buffer,
                                               offset,
                                               size,
                                               fAsync,
                                               out bytesRead,
                                               out ppAsyncReceiveBuffer);
            }
            finally {
                // Release the blocking call
                IsInReadEntitySync = false;
            }

            // DevDiv Bugs 177323: mimic classic mode and don't throw if the client disconnected
            if (result < 0) {
                RaiseCommunicationError(result, false /*throwOnDisconnect*/);
            }

            if (bytesRead > 0) {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, bytesRead);
            }

            return bytesRead;
        }

        // synchronous read
        public override int ReadEntityBody(byte[] buffer, int size) {
            if ( size > buffer.Length ) {
                throw new ArgumentOutOfRangeException("size");
            }

            return ReadEntityCoreSync(buffer, 0, size);
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size) {
            if ( buffer.Length - offset < size ) {
                throw new ArgumentOutOfRangeException("offset");
            }

            // 
            return ReadEntityCoreSync(buffer, offset, size);
        }

        public override long GetBytesRead() {
            throw new HttpException(
                    SR.GetString(SR.Not_supported));
        }

        public override bool SupportsAsyncFlush { get { return true; } }

        // Begin an asynchronous flush of the response.  To support this, the worker request buffers
        // the status, headers, and resonse body until an asynchronous flush operation is initiated.
        public override IAsyncResult BeginFlush(AsyncCallback callback, Object state) {
            if (_context == IntPtr.Zero) {
                throw new InvalidOperationException();
            }

            FlushAsyncResult ar = new FlushAsyncResult(callback, state);

            // we only allow one async operation at a time
            if (Interlocked.CompareExchange(ref _asyncResultBase, ar, null) != null)
                throw new InvalidOperationException(SR.GetString(SR.Async_operation_pending));

            // initiate async operation here
            bool async = true;
            bool completedSynchronously;
            int hresult = IIS.MgdExplicitFlush(_context, async, out completedSynchronously);

            if (hresult < 0) {
                _asyncResultBase = null;
                // treat every error as if the client disconnected
                IncrementRequestsDisconnected();
                throw new HttpException(SR.GetString(SR.ClientDisconnected), hresult);
            }
            else if (completedSynchronously) {
                _asyncResultBase = null;
                ar.Complete(0, HResults.S_OK, IntPtr.Zero, synchronous: true);
                return ar;
            }
            else {
                return ar;
            }
        }

        // Finish an asynchronous flush.
        public override void EndFlush(IAsyncResult asyncResult) {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            FlushAsyncResult ar = asyncResult as FlushAsyncResult;
            if (ar == null)
                throw new ArgumentException(null, "asyncResult");
            ar.ReleaseWaitHandleWhenSignaled();
            if (ar.HResult < 0) {
                // treat every error as if the client disconnected
                IncrementRequestsDisconnected();
                throw new HttpException(SR.GetString(SR.ClientDisconnected), ar.HResult);
            }
            _headersSent = true;
        }

        public override bool SupportsAsyncRead { get { return true; } }

        internal void OnAsyncCompletion(int bytesCompleted, int hresult, IntPtr pAsyncCompletionContext) {
            // clear _asyncResultBase because when we call Complete the user's callback
            // may begin another async operation
            AsyncResultBase ar = _asyncResultBase;
            _asyncResultBase = null;
            ar.Complete(bytesCompleted, hresult, pAsyncCompletionContext, synchronous: false);
        }

        // Begin an asynchronous read of the request entity body.  To read the entire entity, invoke
        // repeatedly until total bytes read is equal to Request.ContentLength or EndRead indicates
        // that zero bytes were read.  If Request.ContentLength is zero and the request is chunked,
        // then invoke repeatedly until EndRead indicates that zero bytes were read.
        //
        // If an error occurs and the client is no longer connected, an HttpException will be thrown.
        //
        // This implements Stream.BeginRead, and as such, should throw
        // exceptions as described on MSDN when errors occur.
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            // Do as Stream does.
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.InvalidOffsetOrCount, "offset", "count"));

            ReadAsyncResult ar = new ReadAsyncResult(callback, state, buffer, offset, count, updatePerfCounter: true);

            if (count == 0) {
                ar.Complete(0, HResults.S_OK, IntPtr.Zero, synchronous: true);
                return ar;
            }

            // we only allow one async operation at a time
            if (Interlocked.CompareExchange(ref _asyncResultBase, ar, null) != null)
                throw new InvalidOperationException(SR.GetString(SR.Async_operation_pending));

            // initiate async operation here
            bool fAsync = true;
            int bytesRead = 0;
            IntPtr ppAsyncReceiveBuffer;
            int hresult = IIS.MgdReadEntityBody(_context,
                                                null, // for async the data is copied from a native buffer to avoid pinning
                                                0, // for async, offset is ignored
                                                count, // number of bytes to read
                                                fAsync,
                                                out bytesRead,
                                                out ppAsyncReceiveBuffer);

            if (hresult < 0) {
                _asyncResultBase = null;
                // treat every error as if the client disconnected
                IncrementRequestsDisconnected();
                throw new HttpException(SR.GetString(SR.ClientDisconnected), hresult);
            }
            // if we completed synchronously, then ppAsyncReceiveBuffer will be non-null
            else if (ppAsyncReceiveBuffer != IntPtr.Zero) {
                _asyncResultBase = null;
                ar.Complete(bytesRead, HResults.S_OK, ppAsyncReceiveBuffer, synchronous: true);
                return ar;
            }
            else {
                return ar;
            }
        }

        // Finish an asynchronous read.  When this returns zero there is no more to be read.  If Request.ContentLength is non-zero,
        // do not read more bytes then specified by ContentLength, or an error will occur.
        // This implements Stream.EndRead on HttpBufferlessInputStream, and as such, should throw
        // exceptions as described on MSDN when errors occur.
        public override int EndRead(IAsyncResult asyncResult) {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            ReadAsyncResult ar = asyncResult as ReadAsyncResult;
            if (ar == null)
                throw new ArgumentException(null, "asyncResult");
            ar.ReleaseWaitHandleWhenSignaled();
            if (ar.HResult < 0) {
                // treat every error as if the client disconnected
                IncrementRequestsDisconnected();
                throw new HttpException(SR.GetString(SR.ClientDisconnected), ar.HResult);
            }
            else {
                return ar.BytesRead;
            }
        }

        private void IncrementRequestsDisconnected() {
            if (!_disconnected) {
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_DISCONNECTED);
                _disconnected = true;
            }
        }

        // This is a performance-sensitive code path and may occur very early in the request, so we should
        // drop down to IIS to get the header directly rather than build up the entire header dictionary.
        // The p/invoke here is cheap since all arguments are blittable types.
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Guid.TryParseExact(System.String,System.String,System.Guid@)", Justification = "Guid.Empty is equivalent to no ID present.")]
        internal Guid GetRequestCorrelationId() {
            IntPtr correlationIdStringPtr;
            ushort correlationIdStringLength;
            bool isWcfFormat;

            int hr = UnsafeIISMethods.MgdGetCorrelationIdHeader(_context, out correlationIdStringPtr, out correlationIdStringLength, out isWcfFormat);
            Marshal.ThrowExceptionForHR(hr);

            try {
                if (correlationIdStringPtr != IntPtr.Zero && correlationIdStringLength > 0) {
                    string correlationIdString = StringUtil.StringFromCharPtr(correlationIdStringPtr, correlationIdStringLength);
                    if (isWcfFormat) {
                        // Per WCF guidelines (http://msdn.microsoft.com/en-us/library/hh536924.aspx),
                        // the header is a base64-encoded 16-byte binary blob.
                        byte[] binaryBlob = Convert.FromBase64String(correlationIdString); // unfortunately no TryConvert method
                        if (binaryBlob != null && binaryBlob.Length == 16) {
                            return new Guid(binaryBlob);
                        }
                    }
                    else {
                        // Per Azure REST design guidelines, the header must use the 'D' format
                        Guid guid;
                        Guid.TryParseExact(correlationIdString, "D", out guid);
                        return guid;
                    }
                }
            }
            catch {
                // Exceptions are not fatal; we just pretend there's no correlation ID.
            }

            // Either there was no correlation ID or it didn't parse correctly
            return default(Guid);
        }

        public override String GetKnownRequestHeader(int index)  {
            if ( !_requestHeadersAvailable ) {
                // special case important ones so that no
                // all headers parsing is required

                switch ( index ) {
                case HeaderCookie:
                    return GetCookieHeaderInternal();

                case HeaderContentType:
                    if ( _contentType == CONTENT_FORM )
                        return "application/x-www-form-urlencoded";
                    break;

                case HeaderContentLength:
                    if ( _contentType != CONTENT_NONE )
                        return(_contentTotalLength).ToString(CultureInfo.InvariantCulture);
                    break;

                case HeaderUserAgent:
                    return GetUserAgentInternal();
                }

                // parse all headers
                ReadRequestHeaders();
            }

            return _knownRequestHeaders[index];
        }


        public override String GetUnknownRequestHeader(String name) {
            if ( !_requestHeadersAvailable )
                ReadRequestHeaders();

            int n = _unknownRequestHeaders.Length;

            for ( int i = 0; i < n; i++ ) {
            if (StringUtil.EqualsIgnoreCase(name, _unknownRequestHeaders[i][0]))
                    return _unknownRequestHeaders[i][1];
            }

            return null;
        }

        public override String[][] GetUnknownRequestHeaders() {
            if ( !_requestHeadersAvailable )
                ReadRequestHeaders();

            return _unknownRequestHeaders;
        }

        public override String GetServerVariable(String name) {
            // fall back for headers
            // (IIS6 doesn't support them as UNICODE_XXX)
            if (StringUtil.StringStartsWith(name, "HTTP_")) {
                return GetServerVariableInternalAnsi(name);
            }
            else {
                return GetServerVariableInternal(name);
            }
        }


        internal override void SendStatus(int statusCode, int subStatusCode, string statusDescription) {
            if (null == statusDescription) {
                statusDescription = String.Empty;
            }

            int hr = IIS.MgdSetStatusW(_context,
                                       statusCode,
                                       subStatusCode,
                                       statusDescription,
                                       null /*pszErrorDescription*/,
                                       _trySkipIisCustomErrors);

            // set to false after status has been set
            _trySkipIisCustomErrors = false;

            Misc.ThrowIfFailedHr(hr);
        }

        public override void SendStatus(int statusCode,
                                        String statusDescription) {

            SendStatus(statusCode, 0, statusDescription);
        }

        internal override void SetHeaderEncoding(Encoding encoding) {
            _headerEncoding = encoding;
        }

        //
        // because we may update headers N times in IIS 7
        // we can no longer throw after headers have been sent once
        // Instead, just push headers through
        public override void SendKnownResponseHeader(int index, String value) {
            if ( index < 0 || index >= HttpWorkerRequest.ResponseHeaderMaximum ) {
                throw new ArgumentOutOfRangeException("index");
            }
            SetKnownResponseHeader(index, value, false /*replace*/);
        }

        public override void SendUnknownResponseHeader(String name,
                                                       String value) {
            if ( null == name ) {
                throw new ArgumentNullException("name");
            }

            SetUnknownResponseHeader(name, value, false /*replace*/);
        }

        public override void SendCalculatedContentLength(int contentLength) {
            SendKnownResponseHeader(HeaderContentLength,
                                    contentLength.ToString(CultureInfo.InvariantCulture));
        }

        public override bool HeadersSent() {
            return _headersSent;
        }

        public override bool IsClientConnected() {
            return (!_connectionClosed && IIS.MgdIsClientConnected(_context));
        }

        internal bool TryGetClientDisconnectedCancellationToken(out CancellationToken cancellationToken) {
            // This API is not supported on IIS versions prior to IIS 7.5.
            if (HttpRuntime.IISVersion < IIS_VERSION_75) {
                cancellationToken = CancellationToken.None;
                return false;
            }

            // The registration is done inside of the dispose lock because we don't want to
            // allow registration while the native context object is being destroyed. If we
            // didn't have this lock, there is a potential race condition where the
            // registration could take place at the same time as a call to Dispose, at which
            // point we lose our guarantees about the unmanaged code not trying to call back
            // into this AppDomain.
            //
            // If you change the below code, be judicious with how much work is performed
            // within the lock on _disposeLockObj. The p/invoke call that takes place within
            // the lock will itself try to enter a critical section in the class
            // MGD_CONNECTION_STORED_CONTEXT, and this could very easily lead to deadlock
            // scenarios. The particular usage below is safe because the _disposeLockObj
            // lock is always taken before entering the unmanaged critical section, never
            // the other way around. So code that is within the unmanaged critical section
            // is guaranteed to execute quickly without blocking.

            if (_clientDisconnectTokenHelper == null) {
                lock (_disposeLockObj) {
                    if (_clientDisconnectTokenHelper == null) {
                        if (_context == IntPtr.Zero) {
                            // A call to Dispose already took place, so we'll just return a pre-disposed helper.
                            _clientDisconnectTokenHelper = CancellationTokenHelper.StaticDisposed;
                        }
                        else {
                            bool isClientConnected;
                            int hr = IIS.MgdConfigureAsyncDisconnectNotification(_context, true /* fEnabled */, out isClientConnected);
                            Marshal.ThrowExceptionForHR(hr);

                            // Seed the helper's initial state with the known client connection status.
                            // Pay attention to the race condition mentioned in NotifyOfAsyncDisconnect.
                            LazyInitializer.EnsureInitialized(ref _clientDisconnectTokenHelper, () => new CancellationTokenHelper(canceled: !isClientConnected));
                        }
                    }
                }
            }

            cancellationToken = _clientDisconnectTokenHelper.Token;
            return true;
        }

        // called from IIS native code when client asynchronously disconnects
        internal void NotifyOfAsyncDisconnect() {
            // Race condition: it's possible that an async disconnect notification could take place
            // during GetClientDisconnectedCancellationToken but before _clientDisconnectTokenHelper
            // has been set. If this is the case, we'll just create a pre-canceled helper.

            CancellationTokenHelper helper = LazyInitializer.EnsureInitialized(ref _clientDisconnectTokenHelper, () => new CancellationTokenHelper(canceled: true));
            helper.Cancel();
        }

        internal bool IsHandlerExecutionDenied() {
            return IIS.MgdIsHandlerExecutionDenied(_context);
        }

        internal void AbortConnection() {
            // Since AbortConnection() can be called from any thread, there's a potential race condition
            // where an AbortConnection() call can come in during or after the disposal of this object.
            // The following lock solves this issue by blocking the Dispose() routine if there's an
            // outstanding call to AbortConnection() and by making this method a no-op if Dispose() has
            // already been invoked.

            lock (_disposeLockObj) {
                if (_context != IntPtr.Zero) {
                    IIS.MgdAbortConnection(_context);
                    _connectionClosed = true;
                }
            }
        }

        public override void CloseConnection() {
            IIS.MgdCloseConnection(_context);
            _connectionClosed = true;
        }

        public override IntPtr GetUserToken() {
            IntPtr token = IntPtr.Zero;
            int result = IIS.MgdGetUserToken(_context, out token );
            Misc.ThrowIfFailedHr(result);
            return token;
        }

        public override IntPtr GetVirtualPathToken() {
            IntPtr token = IntPtr.Zero;
            int result = IIS.MgdGetVirtualToken(_context, out token );
            Misc.ThrowIfFailedHr(result);
            return token;
        }


        // called directly by HttpResponse for chunked prefixes and suffixes
        // this contains managed memory and needs to be copied
        public override void SendResponseFromMemory(byte[] data, int length) {
            if (_connectionClosed)
                return;
            Debug.Trace("IIS7WorkerRequest",
                        "SendResponseFromMemory(byte[], int)");

            if ( length > 0 )
                AddBodyToCachedResponse(new MemoryBytes(data, length));
        }

        public override void SendResponseFromMemory(IntPtr data, int length) {
            if (_connectionClosed)
                return;
            Debug.Trace("IIS7WorkerRequest",
                        "SendResponseFromMemory(IntPtr, int)");
            SendResponseFromMemory(data, length, false);
        }


        internal override void SendResponseFromMemory(IntPtr data,
                                                      int length,
                                                      bool isBufferFromUnmanagedPool) {
            Debug.Trace("IIS7WorkerRequest",
                        "SendResponseFromMemory(IntPtr, int, " +
                        isBufferFromUnmanagedPool.ToString() +
                        ")");

            if (length > 0) {
                AddBodyToCachedResponse(
                        new MemoryBytes(data,
                                        length,
                                        isBufferFromUnmanagedPool ? BufferType.UnmanagedPool : BufferType.Managed));
            }
        }

        internal void SendResponseFromIISAllocatedRequestMemory(IntPtr data,
                                                                int length) {
            Debug.Trace("IIS7WorkerRequest",
                        "SendResponseFromIISAllocatedRequestMemory(IntPtr, int)");

            // WOS 1926509: ASP.NET:  WriteSubstitution in integrated mode needs to support callbacks that return String.Empty
            if (data != IntPtr.Zero && length >= 0) {
                AddBodyToCachedResponse(
                        new MemoryBytes(data,
                                        length,
                                        BufferType.IISAllocatedRequestMemory));
            }
        }

        internal override void TransmitFile(String filename, long offset, long length, bool isImpersonating) {
            if (_connectionClosed)
                return;
            Debug.Trace("IIS7WorkerRequest",
                        "TransmitFile(String, long, long, bool)");

            if (length > 0) {
                AddBodyToCachedResponse(new MemoryBytes(filename, offset, length));
            }
        }

        // VSWhidbey 555203: support 64-bit file sizes for TransmitFile on IIS6
        internal override bool SupportsLongTransmitFile {
            get { return true; }
        }

        private void SendResponseFromFileStream(FileStream f,
                                                long offset,
                                                long length) {
            long fileSize = f.Length;

            if (length == -1)
                length = fileSize - offset;

            if (offset < 0 || length > fileSize - offset) {
                throw new HttpException(
                        SR.GetString(SR.Invalid_range));
            }

            if (length > 0) {
                if (offset > 0)
                    f.Seek(offset, SeekOrigin.Begin);

                byte[] fileBytes = new byte[(int)length];
                int bytesRead = f.Read(fileBytes, 0, (int)length);
                if (bytesRead > 0) {
                    AddBodyToCachedResponse(
                            new MemoryBytes(fileBytes, bytesRead));
                }
            }

        }

        public override void SendResponseFromFile(string name,
                                                  long offset,
                                                  long length)
        {
            if (_connectionClosed)
                return;
            Debug.Trace("IIS7WorkerRequest",
                        "SendResponseFromFile(string, long, long)");


            if (length == 0) {
                return;
            }

            FileStream f = null;

            try {
                f = new FileStream(name,
                                   FileMode.Open,
                                   FileAccess.Read,
                                   FileShare.Read);

                SendResponseFromFileStream(f, offset, length);
            }
            finally {
                if (f != null)
                    f.Close();
            }
        }

        // override has signed long already
        public override void SendResponseFromFile(IntPtr handle,
                                                  long offset,
                                                  long length) {
            if (_connectionClosed)
                return;
            Debug.Trace("IIS7WorkerRequest",
                        "SendResponseFromFile(IntPtr, long, long)");

            if (length == 0) {
                return;
            }

            FileStream f = null;

            try {
                f = new FileStream(
                            new Microsoft.Win32.SafeHandles.SafeFileHandle(
                                handle,
                                false),
                            FileAccess.Read,
                            0,
                            false);

                SendResponseFromFileStream(f, offset, length);
            }
            finally {
                if (f != null)
                    f.Close();
            }
        }

        public override void FlushResponse(bool finalFlush) {
            if (_connectionClosed)
                return;
            Debug.Trace("IIS7WorkerRequest", "FlushResponse( " + finalFlush + ")");
            FlushCachedResponse(finalFlush);
        }

        public override void EndOfRequest() {
            // this work is done in HttpRuntime.FinishPipelineRequest
        }

        public override Guid RequestTraceIdentifier {
            get { return _traceId; }
        }

        internal unsafe HTTP_COOKED_URL* GetCookedUrl() {
            return (HTTP_COOKED_URL*)_pCookedUrl;
        }

        private string GetServerVariableInternalAnsi(string name) {
            IntPtr buffer;
            int bufLen;

            int retval = IIS.MgdGetServerVariableA(_context,
                                                   name,
                                                   out buffer,
                                                   out bufLen);

            Misc.ThrowIfFailedHr(retval);

            if (buffer != IntPtr.Zero) {
                return StringUtil.StringFromCharPtr(buffer, bufLen);
            }

            return null;
        }

        private string GetServerVariableInternal(string name) {
            IntPtr buffer;
            int bufLen;

            int retval = IIS.MgdGetServerVariableW(_context,
                                                   name,
                                                   out buffer,
                                                   out bufLen);

            Misc.ThrowIfFailedHr(retval);

            if (buffer != IntPtr.Zero) {
                return StringUtil.StringFromWCharPtr(buffer, bufLen);
            }

            return null;
        }

        internal string GetCurrentModuleName() {
            IntPtr buffer;
            int bufferSize;
            string moduleName = null;

            int retval = IIS.MgdGetCurrentModuleName(_context,
                                                     out buffer,
                                                     out bufferSize);

            Misc.ThrowIfFailedHr(retval);

            if (bufferSize > 0) {
                moduleName = StringUtil.StringFromWCharPtr(buffer, bufferSize);
            }

            return moduleName;
        }

        private string GetUserAgentInternal() {
            IntPtr buffer;
            int bufLen;

            int retval = IIS.MgdGetUserAgent(_context,
                                             out buffer,
                                             out bufLen);


            Misc.ThrowIfFailedHr(retval);
            // User-Agent is ASCII string
            if (buffer != IntPtr.Zero) {
                return StringUtil.StringFromCharPtr(buffer, bufLen);
            }

            return null;
        }

        private string GetCookieHeaderInternal() {
            IntPtr buffer;
            int bufLen;

            int retval = IIS.MgdGetCookieHeader(_context,
                                                out buffer,
                                                out bufLen);

            Misc.ThrowIfFailedHr(retval);
            // Cookie header is ASCII string
            if (buffer != IntPtr.Zero) {
                return StringUtil.StringFromCharPtr(buffer, bufLen);
            }

            return null;
        }


        private void ReadRequestHeaders() {
            if ( _requestHeadersAvailable )
                return;

            _knownRequestHeaders = new String[RequestHeaderMaximum];

            // construct unknown headers as array list of name1,value1,...

            ArrayList headers = new ArrayList();

            String s = GetServerVariable("ALL_RAW");
            int l = (s != null) ? s.Length : 0;
            int i = 0;

            while ( i < l ) {
                //  find next :

                int ci = s.IndexOfAny(s_ColonOrNL, i);

                if ( ci < 0 )
                    break;

                if ( s[ci] == '\n' ) {
                    // ignore header without :
                    i = ci+1;
                    continue;
                }

                if ( ci == i ) {
                    i++;
                    continue;
                }

                // add extract name
                String name = s.Substring(i, ci-i).Trim();

                //  find next \n
                int ni = s.IndexOf('\n', ci+1);
                if ( ni < 0 )
                    ni = l;

                // continuation of header (ASURT 115064)
                while ( ni < l-1 && s[ni+1] == ' ' ) {
                    ni = s.IndexOf('\n', ni+1);
                    if ( ni < 0 )
                        ni = l;
                }

                // extract value
                String value = s.Substring(ci+1, ni-ci-1).Trim();

                // remember
                int knownIndex = GetKnownRequestHeaderIndex(name);
                if ( knownIndex >= 0 ) {
                    _knownRequestHeaders[knownIndex] = value;
                }
                else {
                    headers.Add(name);
                    headers.Add(value);
                }

                i = ni+1;
            }

            // copy to array unknown headers

            int n = headers.Count / 2;
            _unknownRequestHeaders = new String[n][];
            int j = 0;

            for ( i = 0; i < n; i++ ) {
                _unknownRequestHeaders[i]    = new String[2];
                _unknownRequestHeaders[i][0] = (String)headers[j++];
                _unknownRequestHeaders[i][1] = (String)headers[j++];
            }

            _requestHeadersAvailable = true;
        }


        private void AddBodyToCachedResponse(MemoryBytes bytes) {
            if ( _cachedResponseBodyBytes == null ) {
                _cachedResponseBodyBytes = new ArrayList();
            }
            Debug.Assert(null !=bytes, "null != bytes");

            _cachedResponseBodyBytes.Add(bytes);
            _cachedResponseBodyLength += bytes.Size;
        }

        private void FlushCachedResponse(bool isFinal) {
            if (_connectionClosed)
                return;
            if ( _context == IntPtr.Zero )
                return;

            int         numFragments    = 0;
            IntPtr[]    fragments       = null;
            int[]       fragmentLengths = null;
            long        bytesOut        = 0;
            int[]       bodyFragmentTypes = null;

            try {
                // prepare body fragments as IntPtr[] of
                // pointers and int[] of lengths
                if ( _cachedResponseBodyLength > 0 ) {
                    numFragments = _cachedResponseBodyBytes.Count;

                    fragments =
                        RecyclableArrayHelper.GetIntPtrArray(numFragments);

                    fragmentLengths =
                        RecyclableArrayHelper.GetIntegerArray(numFragments);

                    bodyFragmentTypes =
                        RecyclableArrayHelper.GetIntegerArray(numFragments);

                    for ( int i = 0; i < numFragments; i++ ) {
                        MemoryBytes bytes =
                            (MemoryBytes)_cachedResponseBodyBytes[i];

                        // lock memory locks managed memory,
                        // just returns the IntPtr for native memory
                        fragments[i] = bytes.LockMemory();

                        bodyFragmentTypes[i] = (byte) bytes.BufferType;
                        fragmentLengths[i] = bytes.Size;

                        if (bytes.UseTransmitFile) {
                            bytesOut += bytes.FileSize;
                        }
                        else {
                            bytesOut += bytes.Size;
                        }
                    }
                }

                int delta = (int) bytesOut;
                if (delta > 0) {
                    PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_OUT, delta);
                }

                // send to unmanaged code
                // sends are always [....] now since they're buffered by IIS
                FlushCore(true,
                          numFragments,
                          fragments,
                          fragmentLengths,
                          bodyFragmentTypes);
            }
            finally {
                // unlock pinned memory
                UnlockCachedResponseBytes();

                // recycle buffers
                RecyclableArrayHelper.ReuseIntPtrArray(fragments);
                RecyclableArrayHelper.ReuseIntegerArray(fragmentLengths);
                RecyclableArrayHelper.ReuseIntegerArray(bodyFragmentTypes);
            }
        }

        private void FlushCore(bool       keepConnected,
                               int        numBodyFragments,
                               IntPtr[]   bodyFragments,
                               int[]      bodyFragmentLengths,
                               int[]      bodyFragmentTypes)
        {

            if (_connectionClosed)
                return;
            if ( _context == IntPtr.Zero )
                return;

            Debug.Trace("IIS7WorkerRequest", "FlushCore with " +
                    numBodyFragments.ToString(CultureInfo.InvariantCulture) +  " fragments\n");


            int rc = IIS.MgdFlushCore(
                                     _context,
                                     keepConnected,
                                     numBodyFragments,
                                     bodyFragments,
                                     bodyFragmentLengths,
                                     bodyFragmentTypes);

            // don't throw when the client disconnected to mimic classic mode
            // DevDiv Bugs 177323: mimic classic mode and don't throw if the client disconnected
            if (rc < 0) {
                RaiseCommunicationError(rc, false  /*throwOnDisconnect*/);
            }
        }

        internal void UnlockCachedResponseBytes() {
            // unlock pinned memory
            if ( _cachedResponseBodyBytes != null ) {
                int numFragments = _cachedResponseBodyBytes.Count;
                for ( int i = 0; i < numFragments; i++ ) {
                    try {
                        ((MemoryBytes)_cachedResponseBodyBytes[i]).UnlockMemory();
                    }
                    catch {
                    }
                }
            }

            // don't remember cached data anymore
            ResetCachedResponse();
        }

        private void ResetCachedResponse() {
            _cachedResponseBodyLength = 0;
            _cachedResponseBodyBytes = null;
        }

        private int GetPreloadedContentInternal(byte[] buffer, int offset, int length) {
            if (offset >= buffer.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (length + offset > buffer.Length) {
                throw new ArgumentOutOfRangeException("length");
            }

            // in theory, this should never return INSUFFICIENT_BUFFER
            // since the caller has sized it so we'll throw if it fails
            // for any reason
            int bytesReceived = 0;
            int result = IIS.MgdGetPreloadedContent(_context, buffer, offset, length, out bytesReceived);
            Misc.ThrowIfFailedHr(result);

            if (bytesReceived  > 0) {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, bytesReceived);
            }
            return bytesReceived;
        }

        // WOS 1555777: kernel cache support
        // If the response can be kernel cached, return the kernel cache key;
        // otherwise return null.  The kernel cache key is used to invalidate
        // the entry if a dependency changes or the item is flushed from the
        // managed cache for any reason.
        internal override string SetupKernelCaching(int secondsToLive, string originalCacheUrl, bool enableKernelCacheForVaryByStar) {
            string cacheUrl = _cacheUrl;

            // if we're re-inserting the response into the kernel cache, the original key must match
            if (originalCacheUrl != null && originalCacheUrl != cacheUrl) {
                return null;
            }

            // If the request contains a query string, don't kernel cache the entry
            if (String.IsNullOrEmpty(cacheUrl) || (!enableKernelCacheForVaryByStar && cacheUrl.IndexOf('?') != -1)) {
                return null;
            }

            // enable kernel caching by setting up the HTTP_CACHE_POLICY
            int result = IIS.MgdSetKernelCachePolicy(_context, secondsToLive);

            // if we failed to setup the kernel cache policy, return null to disable
            // kernel caching for this response
            if (result < 0) {
                return null;
            }

            // okay, the response will be kernel cached, here's the key
            return cacheUrl;
        }

        // WOS 1555777: kernel cache support
        internal override void DisableKernelCache() {
            // disable kernel cache for this response in IIS
            IIS.MgdDisableKernelCache(_context);
        }

        // DevDiv 255268: IIS user-mode cache support
        internal override void DisableUserCache() {
            // disable user cache for this response in IIS
            IIS.MgdDisableUserCache(_context);
        }

        // DevDiv 255268: IIS cache support
        private void DisableIISCache() {
            // disable both user mode cache and kernel mode cache for this response in IIS
            DisableKernelCache();
            DisableUserCache();
        }

        internal override bool TrySkipIisCustomErrors {
            get { return _trySkipIisCustomErrors;  }
            set { _trySkipIisCustomErrors = value; }
        }

        // This should only be used to fix up the handler mapping and paths after a call to HttpContext.RewritePath
        internal string ReMapHandlerAndGetHandlerTypeString(HttpContext httpContext, string path, out bool handlerExists) {
            IntPtr buffer;
            int bufferSize;
            string handlerTypeString = null;

            int result = IIS.MgdReMapHandler(_context,
                                             path,
                                             out buffer,
                                             out bufferSize,
                                             out handlerExists);
            Misc.ThrowIfFailedHr(result);

            if( bufferSize > 0 ) {
                handlerTypeString = StringUtil.StringFromWCharPtr(buffer, bufferSize);

            }

            if (handlerExists) {
                //DevDivBugs 187445: need to update _path, _filePath, _pathTranslated, _pathInfo, and _queryString
                ReadRequestBasics();

                // clear things that depend on path
                httpContext.ConfigurationPath = null;
                try {
                    // disable RewriteNotifyPipeline
                    _rewriteNotifyDisabled = true;
                    // rewrite path on request
                    httpContext.Request.InternalRewritePath(VirtualPath.CreateAllowNull(_filePath), VirtualPath.CreateAllowNull(_pathInfo), _queryString, _rebaseClientPath);
                }
                finally {
                    // enable RewriteNotifyPipeline
                    _rewriteNotifyDisabled = false;
                }
            }

            return handlerTypeString;
        }

        // if method is null, the method for the current request is used
        // path must be rooted (i.e., not relative)
        internal string MapHandlerAndGetHandlerTypeString(string method, string path, bool convertNativeStaticFileModule, bool ignoreWildcardMappings) {
            IntPtr buffer;
            int bufferSize;
            string handlerTypeString = null;

            int result = IIS.MgdMapHandler(_context,
                                           method,
                                           path,
                                           out buffer,
                                           out bufferSize,
                                           convertNativeStaticFileModule,
                                           ignoreWildcardMappings);
            Misc.ThrowIfFailedHr(result);

            if( bufferSize > 0 ) {
                handlerTypeString = StringUtil.StringFromWCharPtr(buffer, bufferSize);
            }

            return handlerTypeString;
        }

        internal string GetManagedHandlerType() {
            IntPtr buffer;
            int bufferSize;
            string handlerTypeString = null;

            int result = IIS.MgdGetHandlerTypeString(_context,
                                                     out buffer,
                                                     out bufferSize);
            Misc.ThrowIfFailedHr(result);

            if( bufferSize > 0 ) {
                handlerTypeString = StringUtil.StringFromWCharPtr(buffer, bufferSize);
            }

            return handlerTypeString;
        }

        internal void SetRemapHandler(string handlerType, string handlerName) {
            int result = IIS.MgdSetRemapHandler(_context,
                                                handlerName,
                                                handlerType);
            Misc.ThrowIfFailedHr(result);
        }

        internal void SetScriptMapForRemapHandler() {
            int result = IIS.MgdSetScriptMapForRemapHandler(_context);
            Misc.ThrowIfFailedHr(result);
        }

        internal void RewriteNotifyPipeline(string newPath,
                                            string newQueryString,
                                            bool rebaseClientPath) {
            if (_rewriteNotifyDisabled)
                return;

            Debug.Trace("IIS7WorkerRequest", "RewriteNotifyPipeline(" + newPath + ")");

            if(IntPtr.Zero != _context) {
                string url = newPath;

                if(null != newQueryString) {
                    url = newPath + "?" + newQueryString;
                }

                IIS.MgdRewriteUrl(_context, url, null != newQueryString);

                // recall so that we can fix-up paths after ReMapHandlerAndGetHandlerTypeString is called
                _rebaseClientPath = rebaseClientPath;
            }
        }

        internal void DisableNotifications(
                RequestNotification notifications,
                RequestNotification postNotifications) {
            IIS.MgdDisableNotifications(_context, notifications, postNotifications);
        }

        internal void SuppressSendResponseNotifications() {
            IIS.MgdSuppressSendResponseNotifications(_context);
        }

        internal void PushResponseToNative() {
            FlushCachedResponse(false);
        }

        internal void ClearResponse(bool clearEntity, bool clearHeaders) {
            IIS.MgdClearResponse(_context, clearEntity, clearHeaders);
        }

        private void GetStatusChanges(HttpContext ctx) {
            ushort status;
            ushort subStatus;
            IntPtr buffer;
            ushort bufLen;
            string description = null;

            int result = IIS.MgdGetStatusChanges( _context, out status, out subStatus, out buffer, out bufLen);
            Misc.ThrowIfFailedHr(result);

            if (buffer != IntPtr.Zero) {
                description = StringUtil.StringFromCharPtr(buffer, bufLen);
            }

            // set to false after status has been set
            _trySkipIisCustomErrors = false;

            ctx.Response.SynchronizeStatus(status, subStatus, description);
        }

        internal IntPtr AllocateRequestMemory(int size) {
            if (size > 0) {
                return IIS.MgdAllocateRequestMemory(_context, size);
            }
            return IntPtr.Zero;
        }

        internal ArrayList GetBufferedResponseChunks(bool disableRecycling, ArrayList substElements, ref bool hasSubstBlocks) {
            IntPtr[] fragments;
            int [] fragmentLengths;
            int [] fragmentTypes;

            // pick a reasonable size as a guestimate
            int numFragments = 32;
            int startFragments = numFragments;

            fragments =
                RecyclableArrayHelper.GetIntPtrArray(numFragments);

            fragmentLengths =
                RecyclableArrayHelper.GetIntegerArray(numFragments);

            fragmentTypes =
                RecyclableArrayHelper.GetIntegerArray(numFragments);

            int result = IIS.MgdGetResponseChunks(
                    _context,
                    ref numFragments,
                    fragments,
                    fragmentLengths,
                    fragmentTypes);

            //
            // did it fail?
            //
            // see if we need to reallocate or just fail
            if (result < 0) {

                // realloc
                if (result == HResults.E_INSUFFICIENT_BUFFER) {
                    // make sure we really need to realloc
                    Debug.Assert(numFragments > startFragments, "numFragments > startFragments");

                    // recycle the current stuff
                    RecyclableArrayHelper.ReuseIntPtrArray(fragments);
                    RecyclableArrayHelper.ReuseIntegerArray(fragmentLengths);
                    RecyclableArrayHelper.ReuseIntegerArray(fragmentTypes);

                    fragments =
                        RecyclableArrayHelper.GetIntPtrArray(numFragments);

                    fragmentLengths =
                        RecyclableArrayHelper.GetIntegerArray(numFragments);

                    fragmentTypes =
                        RecyclableArrayHelper.GetIntegerArray(numFragments);

                    result = IIS.MgdGetResponseChunks(
                        _context,
                        ref numFragments,
                        fragments,
                        fragmentLengths,
                        fragmentTypes);
                }

                if (result == HResults.E_INVALID_DATA) {
                    throw new InvalidOperationException(SR.GetString(SR.Invalid_http_data_chunk));
                }

                Misc.ThrowIfFailedHr(result);
            }

            // if we get here, we've acquired the data and need to copy it
            // the basic strategy is to pack as much as possible into
            // each buffer and save those
            // we need to handle file chunks by reading those into memory
            // (this is bogus but much refactoring is needed to handle them from
            // a handle or chunk them)
            ArrayList buffers = new ArrayList();
            HttpResponseUnmanagedBufferElement elem = null;
            HttpSubstBlockResponseElement[] substElemAry = null;
            if (substElements != null) {
                substElemAry = (HttpSubstBlockResponseElement[]) substElements.ToArray(typeof(HttpSubstBlockResponseElement));
            }

            int bytesLeftInChunk = 0;
            for (int i = 0; i < numFragments; i++) {

                // is it memory based?  just copy it in...
                if (fragmentTypes[i] == 0) {

                    if (substElemAry != null) {
                        int substElemAryIndex = -1;
                        for (int j = 0; j < substElemAry.Length; j++) {
                            if (substElemAry[j].PointerEquals(fragments[i])) {
                                substElemAryIndex = j;
                                break;
                            }
                        }
                        // did we find a match
                        if (substElemAryIndex != -1) {
                            if (elem != null) {
                                buffers.Add(elem);
                                elem = null;
                            }
                            buffers.Add(substElemAry[substElemAryIndex]);
                            hasSubstBlocks = true;
                            continue;
                        }
                    }

                    if (null == elem) {
                        elem = new HttpResponseUnmanagedBufferElement();
                        if (disableRecycling) {
                            elem.DisableRecycling(); // since we're holding onto it
                        }
                    }

                    bytesLeftInChunk = fragmentLengths[i];

                    // does it fit?
                    if (bytesLeftInChunk <= elem.FreeBytes) {
                        elem.Append(fragments[i], 0, bytesLeftInChunk);
                    }
                    else {

                        int offset = 0;
                        int bytesWritten;

                        do {

                            // loop, packing buffers as completely as possible
                            bytesWritten = elem.Append(
                                fragments[i],
                                offset,
                                bytesLeftInChunk);

                            // see how much we've got left
                            bytesLeftInChunk -= bytesWritten;
                            offset           += bytesWritten;

                            // if the buffer is exhausted
                            if (elem.FreeBytes == 0) {
                                buffers.Add(elem);
                                elem = new HttpResponseUnmanagedBufferElement();
                                if (disableRecycling) {
                                    elem.DisableRecycling(); // since we're holding onto it
                                }

                            }

                        } while (bytesLeftInChunk > 0);                                                                                     }

                    // handle full buffers
                    if (elem.FreeBytes == 0) {
                        buffers.Add(elem);
                        elem = null;
                    }
                } // end of MemoryBased chunk
                else if (fragmentTypes[i] == 1) {
                    // get the size info
                    long offset = 0;
                    long bytesToRead = 0;
                    result = IIS.MgdGetFileChunkInfo(_context, i, out offset, out bytesToRead);
                    Misc.ThrowIfFailedHr(result);

                    while (bytesToRead > 0 && offset >= 0) {

                        // ensure we have space in buffer
                        if (null == elem || elem.FreeBytes == 0) {
                            if (null != elem) {
                                buffers.Add(elem);
                            }
                            elem = new HttpResponseUnmanagedBufferElement();
                            if (disableRecycling) {
                                elem.DisableRecycling(); // since we're holding onto it
                            }
                        }

                        // read as much as we have space in the buffer
                        int readSize = elem.FreeBytes;

                        // or however many bytes are left, whichever is smaller
                        if (elem.FreeBytes > bytesToRead) {
                            readSize = (int)bytesToRead;
                        }

                        result = IIS.MgdReadChunkHandle( _context, fragments[i], offset, ref readSize, elem.FreeLocation);
                        Misc.ThrowIfFailedHr(result);

                        // bump the free size to account for the data we just read
                        elem.AdjustSize(readSize);

                        bytesToRead -= readSize;
                        offset += readSize;
                    }
                }
                // invalid chunk type
                else {
                    Debug.Assert(fragmentTypes[i] <= 1, "invalid fragment type");
                }
            }

            // do we need to add the last buffer?
            if (null != elem) {
                buffers.Add(elem);
            }

            // recycle the arrays
            RecyclableArrayHelper.ReuseIntPtrArray(fragments);
            RecyclableArrayHelper.ReuseIntegerArray(fragmentLengths);
            RecyclableArrayHelper.ReuseIntegerArray(fragmentTypes);

            return buffers;
        }

        internal bool IsResponseBuffered() {
            int numFragments = 0;
            int result = IIS.MgdGetResponseChunks(_context, 
                                                  ref numFragments,
                                                  null /*fragments*/,
                                                  null /*fragmentLengths*/,
                                                  null /*fragmentTypes*/);
			
            // Because no buffer supplied, don't fail
            if (result != HResults.E_INSUFFICIENT_BUFFER) {
                Misc.ThrowIfFailedHr(result);
            }
			
            return numFragments > 0;
     	}

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Web.Hosting.UnsafeIISMethods.MgdGetAnonymousUserToken(System.IntPtr,System.IntPtr@)", Justification = @"We fallback to default behavior on method failure.")]
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "This is a degenerate case that should never happen.")]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void SetPrincipal(IPrincipal user)
        {
            string userName = null;
            string authType = null;
            IntPtr token = IntPtr.Zero;

            if (user != null) {
                if (user.Identity != null) {
                    userName = user.Identity.Name;
                    authType = user.Identity.AuthenticationType;
                    WindowsIdentity wi = user.Identity as WindowsIdentity;
                    if (wi != null) {
                        token = wi.Token;
                    }
                    else {
                        // DTS#848664: Impersonate with anonymous user token instead of the process token
                        if (AppSettings.AllowAnonymousImpersonation) {
                            IIS.MgdGetAnonymousUserToken(_context, out token); // Do nothing on failure. Fallback to the default behavior
                        }
                    }
                }

                if (userName == null) {
                    userName = String.Empty;
                }
                if (authType == null) {
                    authType = String.Empty;
                }

                if (!IsValidUsername(userName)) {
                    throw new ArgumentException();
                }
            }

            int result = IIS.MgdSetRequestPrincipal(
                    _context,
                    userName,
                    authType,
                    token);

            Misc.ThrowIfFailedHr(result);
        }

        private static bool IsValidUsername(string username) {
            // MSRC 11838 (DevDiv 292994) - The username parameter is marshaled to native code as an LPCWSTR, so
            // we need to prevent embedded nulls from causing a premature termination of the string.

            if (AppSettings.AllowRelaxedHttpUserName) {
                return true; // all usernames are valid (check disabled)
            }
            else {
                return (username.IndexOf('\0') == -1); // true if no embedded null is present
            }
        }

        internal void ResponseFilterInstalled() {
            IIS.MgdSetResponseFilter(_context);
        }

        private void RaiseCommunicationError(int result, bool throwOnDisconnect) {
            if (IIS.MgdIsClientConnected(_context)) {
                throw new HttpException(SR.GetString(SR.Server_Support_Function_Error, result.ToString("X8", CultureInfo.InvariantCulture)), Marshal.GetExceptionForHR(result));
            }
            else {
                //give different error if connection was closed
                IncrementRequestsDisconnected();
                if (throwOnDisconnect) {
                    throw new HttpException(SR.GetString(SR.Server_Support_Function_Error_Disconnect, result.ToString("X8", CultureInfo.InvariantCulture)), result);
                }
            }
        }

        internal void ExplicitFlush() {
            bool async = false;
            bool completedSynchronously;
            int result = IIS.MgdExplicitFlush(_context, async, out completedSynchronously);
            
            // DevDiv Bugs 177323: mimic classic mode and throw if the client disconnected
            if (result < 0) {
                RaiseCommunicationError(result, true /*throwOnDisconnect*/);
            }
            
            _headersSent = true;
        }

        internal void SetServerVariable(string name, string value) {
            int result = IIS.MgdSetServerVariableW(_context, name, value);
            Misc.ThrowIfFailedHr(result);
        }

        internal void SetRequestHeader(string name, string value, bool replace) {
            int knownIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(name);
            if (knownIndex >= 0) {
                SetKnownRequestHeader(knownIndex, value, replace);
            }
            else {
                SetUnknownRequestHeader(name, value, replace);
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
        private void SetKnownRequestHeader(int index, string value, bool replace) {
            Debug.Assert(HttpWorkerRequest.HeaderUserAgent == 39, "HttpWorkerRequest.HeaderUserAgent == 39");
            // User-Agent is 39 in WorkerRequest.cs, but it is 40 in http.h.
            if (index == HttpWorkerRequest.HeaderUserAgent) {
                index = IisHeaderUserAgent;
            }
            byte[] valueBytes;
            int valueLength = StringUtil.GetNullTerminatedByteArray(_headerEncoding, value, out valueBytes);
            int result = IIS.MgdSetKnownHeader(_context, true /*fRequest*/, replace, (ushort)index, valueBytes, (ushort)valueLength);
            Misc.ThrowIfFailedHr(result);
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
        private void SetUnknownRequestHeader(string name, string value, bool replace) {
            byte[] valueBytes;
            int valueLength = StringUtil.GetNullTerminatedByteArray(_headerEncoding, value, out valueBytes);
            byte[] nameBytes;
            StringUtil.GetNullTerminatedByteArray(_headerEncoding, name, out nameBytes);
            int result = IIS.MgdSetUnknownHeader(_context, true /*fRequest*/, replace, nameBytes, valueBytes, (ushort)valueLength);
            Misc.ThrowIfFailedHr(result);
        }

        internal void SetResponseHeader(string name, string value, bool replace) {
            int knownIndex = HttpWorkerRequest.GetKnownResponseHeaderIndex(name);
            if (knownIndex >= 0) {
                SetKnownResponseHeader(knownIndex, value, replace);
            }
            else {
                SetUnknownResponseHeader(name, value, replace);
            }
        }

        private void SetKnownResponseHeader(int index, string value, bool replace) {
            if (index == HttpWorkerRequest.HeaderWwwAuthenticate
                || index == HttpWorkerRequest.HeaderSetCookie
                || index == HttpWorkerRequest.HeaderServer) {
                // IIS7 treats Server, WWW-Authenticate, and Set-Cookie as unknown headers
                SetUnknownResponseHeader(HttpWorkerRequest.GetKnownResponseHeaderName(index), value, replace);
            }
            else {
                byte[] valueBytes;
                int valueLength = StringUtil.GetNullTerminatedByteArray(_headerEncoding, value, out valueBytes);
                int result = IIS.MgdSetKnownHeader(_context, false /*fRequest*/, replace, (ushort)index, valueBytes, (ushort)valueLength);
                Misc.ThrowIfFailedHr(result);
            }
        }

        private void SetUnknownResponseHeader(string name, string value, bool replace) {
            // DevDiv 113142 & 255268: We should disable both the HTTP.SYS kernel cache and the IIS user-mode cache
            // for response with set-cookie header
            if (StringUtil.EqualsIgnoreCase(name, "Set-Cookie")) {
                DisableIISCache();
            }

            byte[] valueBytes;
            int valueLength = StringUtil.GetNullTerminatedByteArray(_headerEncoding, value, out valueBytes);
            byte[] nameBytes;
            StringUtil.GetNullTerminatedByteArray(_headerEncoding, name, out nameBytes);
            int result = IIS.MgdSetUnknownHeader(_context, false /*fRequest*/, replace, nameBytes, valueBytes, (ushort)valueLength);
            Misc.ThrowIfFailedHr(result);
        }

        private void GetServerVarChanges(HttpContext ctx) {
            int    count;
            IntPtr names;
            IntPtr values;
            int    diffCount;
            IntPtr diffIndices;

            int result = IIS.MgdGetServerVarChanges(_context,
                                                    out count,
                                                    out names,
                                                    out values,
                                                    out diffCount,
                                                    out diffIndices);
            Misc.ThrowIfFailedHr(result);

            if (diffCount != 0) {
                unsafe {
                    int * indices = (int*) diffIndices.ToPointer();
                    IntPtr * snapshotNames = (IntPtr *) names.ToPointer();
                    IntPtr * snapshotValues = (IntPtr *) values.ToPointer();
                    for (int i = 0; i < diffCount; i++) {
                        int index = indices[i];
                        IntPtr pName = snapshotNames[index];
                        IntPtr pValue = snapshotValues[index];
                        string name = StringUtil.StringFromCharPtr(pName, UnsafeNativeMethods.lstrlenA(pName));
                        string value = null;
                        if (pValue != IntPtr.Zero) {
                            value = StringUtil.StringFromWCharPtr(pValue, UnsafeNativeMethods.lstrlenW(pValue));
                        }
                        Debug.Trace("IIS7ServerVarChanges", "Server Variable Changed: name=" + name + ", value=" + value);
                        ctx.Request.SynchronizeServerVariable(name, value);
                    }
                }
            }
        }

        private void GetHeaderChanges(HttpContext ctx, bool forRequest) {
            IntPtr knownHeaderSnapshot;
            int    unknownHeaderSnapshotCount;
            IntPtr unknownHeaderSnapshotNames;
            IntPtr unknownHeaderSnapshotValues;
            IntPtr diffKnownIndices;
            int    diffUnknownCount;
            IntPtr diffUnknownIndices;
            int knownIndicesMaximum = forRequest ? IisRequestHeaderMaximum : HttpWorkerRequest.ResponseHeaderMaximum;
            int headerIndex = -1;

            int result = IIS.MgdGetHeaderChanges(_context,
                                                 forRequest,
                                                 out knownHeaderSnapshot,
                                                 out unknownHeaderSnapshotCount,
                                                 out unknownHeaderSnapshotNames,
                                                 out unknownHeaderSnapshotValues,
                                                 out diffKnownIndices,
                                                 out diffUnknownCount,
                                                 out diffUnknownIndices);
            Misc.ThrowIfFailedHr(result);

            unsafe {
                int * knownIndices = (int*) diffKnownIndices.ToPointer();
                IntPtr * snapshot = (IntPtr *) knownHeaderSnapshot.ToPointer();
                for (int i = 0; i < knownIndicesMaximum+1; i++) {
                    int index = knownIndices[i];

                    // the array is terminated by -1
                    if (index < 0)
                        break;

                    string name;
                    if (forRequest) {

                        // For IIS, 39 is HttpHeaderTranslate, and 40 is HttpHeaderUserAgent.
                        // ASP.NET is missing HttpHeaderTranslate, and 39 is HttpHeaderUserAgent.

                        Debug.Assert(HttpWorkerRequest.HeaderUserAgent == 39, "HttpWorkerRequest.HeaderUserAgent == 39");
                        Debug.Assert(HttpWorkerRequest.RequestHeaderMaximum == 40, "HttpWorkerRequest.RequestHeaderMaximum == 40");
                        if (index > HttpWorkerRequest.RequestHeaderMaximum) {
                            throw new NotSupportedException();
                        }

                        if (index < IisHeaderTranslate) {
                            name = HttpWorkerRequest.GetKnownRequestHeaderName(index);
                        }
                        else if (index == IisHeaderTranslate) {
                            name = IisHeaderTranslateName;
                        }
                        else {
                            name = HttpWorkerRequest.GetKnownRequestHeaderName(HttpWorkerRequest.HeaderUserAgent);
                        }
                    }
                    else {
                        if (index >= HttpWorkerRequest.ResponseHeaderMaximum) {
                            throw new NotSupportedException();
                        }
                        // Dev10 767257: IIS always sets known "Server" header to an empty string, and an unknown "Server" header with the value
                        if (index == HttpWorkerRequest.HeaderServer)
                            continue;

                        name = HttpWorkerRequest.GetKnownResponseHeaderName(index);
                        headerIndex = index;
                    }

                    IntPtr pValue = snapshot[index];
                    string value = null;
                    if (pValue != IntPtr.Zero) {
                        value = StringUtil.StringFromCharPtr(pValue, UnsafeNativeMethods.lstrlenA(pValue));
                    }

                    if (forRequest) {
                        Debug.Trace("IIS7ServerVarChanges", "Known Request Header Changed: name=" + name + ", value=" + value);
                        ctx.Request.SynchronizeHeader(name, value);
                    }
                    else {
                        Debug.Trace("IIS7ServerVarChanges", "Known Response Header Changed: name=" + name + ", value=" + value);
                        ctx.Response.SynchronizeHeader(headerIndex, name, value);
                    }
                }
            }

            if (diffUnknownCount != 0) {
                unsafe {
                    int * unknownIndices = (int*) diffUnknownIndices.ToPointer();
                    IntPtr * snapshotNames = (IntPtr *) unknownHeaderSnapshotNames.ToPointer();
                    IntPtr * snapshotValues = (IntPtr *) unknownHeaderSnapshotValues.ToPointer();
                    for (int i = 0; i < diffUnknownCount; i++) {
                        int index = unknownIndices[i];
                        IntPtr pName = snapshotNames[index];
                        IntPtr pValue = (index < unknownHeaderSnapshotCount) ? snapshotValues[index] : IntPtr.Zero; // if index >= diffCount, need to delete header
                        string name = StringUtil.StringFromCharPtr(pName, UnsafeNativeMethods.lstrlenA(pName));
                        string value = null;
                        if (pValue!=IntPtr.Zero) {
                            value = StringUtil.StringFromCharPtr(pValue, UnsafeNativeMethods.lstrlenA(pValue));
                        }

                        if (forRequest) {
                            Debug.Trace("IIS7ServerVarChanges", "Unknown Request Header Changed: name=" + name + ", value=" + value);
                            ctx.Request.SynchronizeHeader(name, value);
                        }
                        else {
                            Debug.Trace("IIS7ServerVarChanges", "Unknown Response Header Changed: name=" + name + ", value=" + value);
                            int knownHeaderIndex = -1; /*unknown*/
                            if (StringUtil.EqualsIgnoreCase(name, "Set-Cookie")) {
                                knownHeaderIndex = HttpWorkerRequest.HeaderSetCookie;
                            }
                            ctx.Response.SynchronizeHeader(knownHeaderIndex, name, value);
                        }
                    }
                }
            }
        }

        private IPrincipal GetUserPrincipal()
        {
            IntPtr pToken;
            IntPtr pAuthType;
            int cchAuthType = 0;
            IntPtr pUserName;
            int cchUserName = 0;
            IIdentity identity = null;
            IPrincipal user = null;
            IntPtr pManagedPrincipal;

            int result = IIS.MgdGetPrincipal(_context,
                                             AppDomain.CurrentDomain.Id,
                                             out pToken,
                                             out pAuthType,
                                             ref cchAuthType,
                                             out pUserName,
                                             ref cchUserName,
                                             out pManagedPrincipal );
            Misc.ThrowIfFailedHr(result);

            // Dev11 32511: The TransferRequest API allows the parent IHttpUser to be passed
            // to the child request in the call to ExecuteRequest.  If this is
            // done, the child request must be able to recreate the managed 
            // IPrincipal of the parent.  We cannot do this if all we have is 
            // the native IHttpUser; for example, we cannot recreate a custom
            // IPrincipal or a custom IIdentity.  So, the first time we call 
            // GetPrincipal, it checks the parent W3_MGD_HANDLER and if it 
            // exists and has the same user, GetPrincipal will return a pointer
            // (GCHandle) to the parent IPrincipal.
            if (pManagedPrincipal != IntPtr.Zero) {
                try {
                    return (IPrincipal)GCUtil.UnrootObject(pManagedPrincipal);
                }
                catch(Exception e) {
                    throw new HttpException(SR.GetString(SR.Failed_to_execute_child_request), e);
                }
            }

            string userName = String.Empty;
            if (pUserName != IntPtr.Zero && cchUserName > 0) {
                userName = StringUtil.StringFromWCharPtr(pUserName, cchUserName);
            }

            string authType = String.Empty;
            if (pAuthType != IntPtr.Zero && cchAuthType > 0) {
                authType = StringUtil.StringFromWCharPtr(pAuthType, cchAuthType);
            }

            if ( String.IsNullOrEmpty(userName) )
            {
                // anonymous user
                if (WindowsAuthenticationModule.IsEnabled) {
                    // the WindowsAuthenticationModule expects the user to be set by this method
                    user = WindowsAuthenticationModule.AnonymousPrincipal;
                }
                else {
                    // other authentication modules will set the user from within their OnAuthenticate methods
                    user = null;
                }
            }
            else if ( pToken != IntPtr.Zero )
            {
                // windows user
                identity = new WindowsIdentity(pToken, authType, WindowsAccountType.Normal, true);
                user = new WindowsPrincipal((WindowsIdentity)identity);
            }
            else
            {
                // generic user
                identity = new GenericIdentity(userName, authType);
                user = new IIS7UserPrincipal(this, identity);
            }

            return user;
        }

        internal bool IsUserInRole(String role) {
            bool isInRole = false;

            int result = IIS.MgdIsInRole(_context,
                                         role,
                                         out isInRole);
            Misc.ThrowIfFailedHr(result);


            return isInRole;
        }

        // Windows OS Bug 1726303 was "fixed" by changing SynchronizeVariables
        // to only synchronize the IPrincipal/IHttpUser if Windows authentication
        // was enabled.  This is incorrect behavior; for example, suppose a forms
        // authenticated request calls TransferRequest and passes the parent IPrincipal to a
        // child request.
        private static bool IsAuthenticationEnabled {
            get {
                if (!s_AuthenticationChecked) {
                    bool enabled = (AuthenticationConfig.Mode != AuthenticationMode.None);
                    s_AuthenticationEnabled = enabled;
                    s_AuthenticationChecked = true;
                }
                return s_AuthenticationEnabled;
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void SynchronizeVariables(HttpContext context) {

            if (context.IsChangeInServerVars) {
                GetServerVarChanges(context);
            }
            if (context.IsChangeInRequestHeaders) {
                GetHeaderChanges(context, true /*forRequest*/);
            }
            if (context.IsChangeInResponseHeaders) {
                GetHeaderChanges(context, false /*forRequest*/);
            }
            if (context.IsChangeInResponseStatus) {
                GetStatusChanges(context);
            }
            if (context.IsChangeInUserPrincipal && IsAuthenticationEnabled) {
                context.SetPrincipalNoDemand(GetUserPrincipal(), false /* needToUpdateNativePrincipal */);
            }
            if (context.AreResponseHeadersSent) {
                context.Response.HeadersWritten = true;
            }
        }

        internal override bool SupportsExecuteUrl {
            //  WOS 1453642:Executing the url directly is not currently supported in Integrated mode
            get { return false; }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal void ScheduleExecuteUrl(string url, string queryString, string method, bool preserveForm, byte[] entity, NameValueCollection headers, bool preserveUser) {
            // prepare headers if specified
            string[] headerNames = null;
            string[] headerValues = null;
            int headerCount = 0;
            if (headers != null && headers.Count > 0) {
                headerCount = headers.Count;
                headerNames = new string[headerCount];
                headerValues = new string[headerCount];

                for(int i = 0; i < headerCount; i++) {
                    headerNames[i] = headers.GetKey(i);
                    headerValues[i] = headers.Get(i);
                }
            }

            bool replaceQueryString = !String.IsNullOrEmpty(queryString);
            if (replaceQueryString) {
                url = url + "?" + queryString;
            }

            //  This schedules the child execute to be done when the request processing
            //  returns to native code.  It does not perform a child execution immediately.
            int result = IIS.MgdExecuteUrl(_context,
                                           url,
                                           replaceQueryString,
                                           preserveForm,
                                           entity,
                                           entity == null ? 0 : (uint) entity.Length,
                                           method,
                                           headerCount,
                                           headerNames,
                                           headerValues,
                                           preserveUser);
            if (result == HResults.ERROR_NOT_SUPPORTED) {
                throw new InvalidOperationException(SR.GetString(SR.TransferRequest_cannot_be_invoked_more_than_once));
            }

            Misc.ThrowIfFailedHr(result);
        }


        public override byte[] GetQueryStringRawBytes() {
            int len;
            IntPtr buffer = GetQueryStringPtr(out len);

            if (len == 0) {
                return null;
            }

            // DevDiv Bugs 189331: HttpRequest.QueryString incorrectly converts ASCII
            // characters above 0x7F to 0x3F ('?') in IIS7 integrated mode only.
            // HTTP.sys creates HTTP_REQUEST.CookedUrl.pQueryString by padding the
            // query string with 0x00 to concoct a UTF-16 string.  The correct way to
            // convert it to bytes is to cast each WCHAR to a CHAR.
            byte[] rawBytes = new byte[len];
            unsafe {
                char * pQueryString = (char*)buffer;
                for(int i = 0; i < rawBytes.Length; i++) {
                    rawBytes[i] = (byte)pQueryString[i];
                }
            }

            return rawBytes;
        }
        public override byte[] GetClientCertificate() {
            if (!_clientCertFetched)
                FetchClientCertificate();

            return _clientCert;
        }

        public override DateTime GetClientCertificateValidFrom() {
            if (!_clientCertFetched)
                FetchClientCertificate();

            return _clientCertValidFrom;
        }

        public override DateTime GetClientCertificateValidUntil() {
            if (!_clientCertFetched)
                FetchClientCertificate();

            return _clientCertValidUntil;
        }

        public override byte [] GetClientCertificateBinaryIssuer() {
            if (!_clientCertFetched)
                FetchClientCertificate();
            return _clientCertBinaryIssuer;
        }

        public override int GetClientCertificateEncoding() {
            if (!_clientCertFetched)
                FetchClientCertificate();
            return _clientCertEncoding;
        }

        public override byte [] GetClientCertificatePublicKey() {
            if (!_clientCertFetched)
                FetchClientCertificate();
            return _clientCertPublicKey;
        }

        private void FetchClientCertificate() {
            if (_clientCertFetched)
                return;

            _clientCertFetched = true;

            IntPtr pClientCert;
            int clientCertSize;
            IntPtr pClientCertIssuer;
            int clientCertIssuerSize;
            IntPtr pClientCertPublicKey;
            int clientCertPublicKeySize;
            uint certEncodingType;
            long notBefore;
            long notAfter;
            int hr = IIS.MgdGetClientCertificate(_context,
                                                 out pClientCert, out clientCertSize,
                                                 out pClientCertIssuer, out clientCertIssuerSize,
                                                 out pClientCertPublicKey, out clientCertPublicKeySize,
                                                 out certEncodingType,
                                                 out notBefore, out notAfter);

            Misc.ThrowIfFailedHr(hr);

            _clientCertEncoding = (int) certEncodingType;

            if (clientCertSize > 0) {
                _clientCert = new byte[clientCertSize];
                Misc.CopyMemory(pClientCert, 0, _clientCert, 0, clientCertSize);
            }

            if (clientCertIssuerSize > 0) {
                _clientCertBinaryIssuer = new byte[clientCertIssuerSize];
                Misc.CopyMemory(pClientCertIssuer, 0, _clientCertBinaryIssuer, 0, clientCertIssuerSize);
            }

            if (clientCertPublicKeySize > 0) {
                _clientCertPublicKey = new byte[clientCertPublicKeySize];
                Misc.CopyMemory(pClientCertPublicKey, 0, _clientCertPublicKey, 0, clientCertPublicKeySize);
            }

            _clientCertValidFrom = (notBefore != 0) ? DateTime.FromFileTime(notBefore) : DateTime.Now;
            _clientCertValidUntil = (notAfter != 0) ? DateTime.FromFileTime(notAfter) : DateTime.Now;
        }

        public override String MapPath(String path)
        {
            return HostingEnvironment.MapPathInternal(path);
        }

        public override String MachineConfigPath
        {
            get
            {
                return HttpConfigurationSystem.MachineConfigurationFilePath;
            }
        }

        public override String RootWebConfigPath
        {
            get
            {
                return HttpConfigurationSystem.RootWebConfigurationFilePath;
            }
        }

        public override String MachineInstallDirectory
        {
            get
            {
                return HttpRuntime.AspInstallDirectory;
            }
        }

        internal ChannelBinding HttpChannelBindingToken {
            get {
                if (_channelBindingToken == null) {
                    IntPtr token       = IntPtr.Zero;
                    int    tokenSize   = 0;
                    int    hr          = HResults.S_OK;

                    hr = IIS.MgdGetChannelBindingToken(_context, out token, out tokenSize);
                    if (hr == HResults.E_NOTIMPL)
                        throw new PlatformNotSupportedException();
                    Misc.ThrowIfFailedHr(hr);
                    _channelBindingToken = new HttpChannelBindingToken(token, tokenSize);
                }
                return _channelBindingToken;
            }
        }

        internal void InsertEntityBody(byte[] buffer, int offset, int count) {
            int hr = IIS.MgdInsertEntityBody(_context, buffer, offset, count);
            Misc.ThrowIfFailedHr(hr);
        }

        // Returns a value indicating whether iiswsock.dll is enabled at all
        internal bool IsWebSocketModuleActive() {
            return (GetServerVariableInternal("WEBSOCKET_VERSION") != null);
        }

        // Returns a value indicating whether iiswsock.dll has interpreted the current request as a WebSocket request
        internal bool IsWebSocketRequest() {
            return String.Equals(GetServerVariableInternal("IIS_WEBSOCK"), "websockets", StringComparison.Ordinal);
        }

        // Accepts a WebSocket for the current request
        internal void AcceptWebSocket() {
            int hr = IIS.MgdAcceptWebSocket(_context);
            Misc.ThrowIfFailedHr(hr);
        }

        internal UnmanagedWebSocketContext GetWebSocketContext() {
            IntPtr pWebSocketContext;
            int hr = IIS.MgdGetWebSocketContext(_context, out pWebSocketContext);

            // Failure conditions are a bad HRESULT or a null pWebSocketContext pointer
            if (hr < 0 || pWebSocketContext == IntPtr.Zero) {
                return null;
            }
            else {
                return new UnmanagedWebSocketContext(pWebSocketContext);
            }
        }

        // Returns a Boolean indicating whether the current request is a child request of a parent ASP.NET request
        internal bool GetIsChildRequest() {
            bool isChildRequest;
            int hr = IIS.MgdGetIsChildContext(_context, out isChildRequest);
            Misc.ThrowIfFailedHr(hr);

            return isChildRequest;
        }

    }
}
