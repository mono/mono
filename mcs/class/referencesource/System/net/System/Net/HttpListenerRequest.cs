//------------------------------------------------------------------------------
// <copyright file="HttpListenerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net.Sockets;
    using System.Net.WebSockets;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Principal;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
  

    internal enum ListenerClientCertState {
        NotInitialized,
        InProgress,
        Completed
    }

    unsafe internal class ListenerClientCertAsyncResult : LazyAsyncResult {
        private NativeOverlapped* m_pOverlapped;
        private byte[] m_BackingBuffer;
        private UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* m_MemoryBlob;
        private uint m_Size;

        internal NativeOverlapped* NativeOverlapped
        {
            get
            {
                return m_pOverlapped;
            }
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* RequestBlob
        {
            get
            {
                return m_MemoryBlob;
            }
        }

        private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(WaitCallback);

        internal ListenerClientCertAsyncResult(object asyncObject, object userState, AsyncCallback callback, uint size) : base(asyncObject, userState, callback) {
            // we will use this overlapped structure to issue async IO to ul
            // the event handle will be put in by the BeginHttpApi2.ERROR_SUCCESS() method
            Reset(size);
        }

        internal void Reset(uint size) {
            if (size == m_Size)
            {
                return;
            }
            if (m_Size != 0)
            {
                Overlapped.Free(m_pOverlapped);
            }
            m_Size = size;
            if (size == 0)
            {
                m_pOverlapped = null;
                m_MemoryBlob = null;
                m_BackingBuffer = null;
                return;
            }
            m_BackingBuffer = new byte[checked((int) size)];
            Overlapped overlapped = new Overlapped();
            overlapped.AsyncResult = this;
            m_pOverlapped = overlapped.Pack(s_IOCallback, m_BackingBuffer);
            m_MemoryBlob = (UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO*) Marshal.UnsafeAddrOfPinnedArrayElement(m_BackingBuffer, 0);
        }

        internal unsafe void IOCompleted(uint errorCode, uint numBytes)
        {
            IOCompleted(this, errorCode, numBytes);
        }

        private static unsafe void IOCompleted(ListenerClientCertAsyncResult asyncResult, uint errorCode, uint numBytes)
        {
            HttpListenerRequest httpListenerRequest = (HttpListenerRequest) asyncResult.AsyncObject;
            object result = null;
            try {
                if (errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA)
               {
                    //There is a bug that has existed in http.sys since w2k3.  Bytesreceived will only
                    //return the size of the inital cert structure.  To get the full size,
                    //we need to add the certificate encoding size as well.

                    UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo = asyncResult.RequestBlob;
                    asyncResult.Reset(numBytes + pClientCertInfo->CertEncodedSize);

                    uint bytesReceived = 0;
                    errorCode =
                        UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(
                            httpListenerRequest.HttpListenerContext.RequestQueueHandle,
                            httpListenerRequest.m_ConnectionId,
                            (uint)UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE,
                            asyncResult.m_MemoryBlob,
                            asyncResult.m_Size,
                            &bytesReceived,
                            asyncResult.m_pOverlapped);

                    if(errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING ||
                       (errorCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS && !HttpListener.SkipIOCPCallbackOnSuccess))
                    {
                        return;
                    }
                }
                
                if (errorCode!=UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                    asyncResult.ErrorCode = (int)errorCode;
                    result = new HttpListenerException((int)errorCode);
                }
                else {
                    UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo = asyncResult.m_MemoryBlob;
                    if (pClientCertInfo!=null) {
                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(httpListenerRequest) + "::ProcessClientCertificate() pClientCertInfo:" + ValidationHelper.ToString((IntPtr)pClientCertInfo)
                            + " pClientCertInfo->CertFlags:" + ValidationHelper.ToString(pClientCertInfo->CertFlags)
                            + " pClientCertInfo->CertEncodedSize:" + ValidationHelper.ToString(pClientCertInfo->CertEncodedSize)
                            + " pClientCertInfo->pCertEncoded:" + ValidationHelper.ToString((IntPtr)pClientCertInfo->pCertEncoded)
                            + " pClientCertInfo->Token:" + ValidationHelper.ToString((IntPtr)pClientCertInfo->Token)
                            + " pClientCertInfo->CertDeniedByMapper:" + ValidationHelper.ToString(pClientCertInfo->CertDeniedByMapper));
                        if (pClientCertInfo->pCertEncoded!=null) {
                            try {
                                byte[] certEncoded = new byte[pClientCertInfo->CertEncodedSize];
                                Marshal.Copy((IntPtr)pClientCertInfo->pCertEncoded, certEncoded, 0, certEncoded.Length);
                                result = httpListenerRequest.ClientCertificate = new X509Certificate2(certEncoded);
                            }
                            catch (CryptographicException exception) {
                                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(httpListenerRequest) + "::ProcessClientCertificate() caught CryptographicException in X509Certificate2..ctor():" + ValidationHelper.ToString(exception));
                                result = exception;
                            }
                            catch (SecurityException exception) {
                                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(httpListenerRequest) + "::ProcessClientCertificate() caught SecurityException in X509Certificate2..ctor():" + ValidationHelper.ToString(exception));
                                result = exception;
                            }
                        }
                        httpListenerRequest.SetClientCertificateError((int)pClientCertInfo->CertFlags);
                    }

                }

                // complete the async IO and invoke the callback
                GlobalLog.Print("ListenerClientCertAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() calling Complete()");
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;
                result = exception;
            }
            finally {
                if(errorCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING){
                    httpListenerRequest.ClientCertState = ListenerClientCertState.Completed;
                }
            }

            asyncResult.InvokeCallback(result);
        }

        private static unsafe void WaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped) {
            // take the ListenerClientCertAsyncResult object from the state
            Overlapped callbackOverlapped = Overlapped.Unpack(nativeOverlapped);
            ListenerClientCertAsyncResult asyncResult = (ListenerClientCertAsyncResult) callbackOverlapped.AsyncResult;

            GlobalLog.Print("ListenerClientCertAsyncResult#" + ValidationHelper.HashString(asyncResult) + "::WaitCallback() errorCode:[" + errorCode.ToString() + "] numBytes:[" + numBytes.ToString() + "] nativeOverlapped:[" + ((long)nativeOverlapped).ToString() + "]");

            IOCompleted(asyncResult, errorCode, numBytes);
        }

        // Will be called from the base class upon InvokeCallback()
        protected override void Cleanup()
        {
            if (m_pOverlapped != null)
            {
                m_MemoryBlob = null;
                Overlapped.Free(m_pOverlapped);
                m_pOverlapped = null;
            }
            GC.SuppressFinalize(this);
            base.Cleanup();
        }

        ~ListenerClientCertAsyncResult()
        {
            if (m_pOverlapped != null && !NclUtilities.HasShutdownStarted)
            {
                Overlapped.Free(m_pOverlapped);
                m_pOverlapped = null;  // Must do this in case application calls GC.ReRegisterForFinalize().
            }
        }
    }


    public sealed unsafe class HttpListenerRequest/* BaseHttpRequest, */  {

        private Uri m_RequestUri;
        private ulong m_RequestId;
        internal ulong m_ConnectionId;
        private SslStatus m_SslStatus;
        private string m_RawUrl;
        private string m_CookedUrlHost;
        private string m_CookedUrlPath;
        private string m_CookedUrlQuery;
        private long m_ContentLength;
        private Stream m_RequestStream;
        private string m_HttpMethod;
        private TriState m_KeepAlive;
        private Version m_Version;
        private WebHeaderCollection m_WebHeaders;
        private IPEndPoint m_LocalEndPoint;
        private IPEndPoint m_RemoteEndPoint;
        private BoundaryType m_BoundaryType;
        private ListenerClientCertState m_ClientCertState;
        private X509Certificate2 m_ClientCertificate;
        private int m_ClientCertificateError;
        private RequestContextBase m_MemoryBlob;
        private CookieCollection m_Cookies;
        private HttpListenerContext m_HttpContext;
        private bool m_IsDisposed = false;
        internal const uint CertBoblSize = 1500;
        private string m_ServiceName;
        private object m_Lock = new object();
        private List<TokenBinding> m_TokenBindings = null;
        private int m_TokenBindingVerifyMessageStatus = 0;

        private enum SslStatus : byte
        {
            Insecure,
            NoClientCert,
            ClientCert
        }

        internal HttpListenerRequest(HttpListenerContext httpContext, RequestContextBase memoryBlob)
        {
            if (Logging.On) Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpContext#" + ValidationHelper.HashString(httpContext) + " memoryBlob# " + ValidationHelper.HashString((IntPtr) memoryBlob.RequestBlob));
            if(Logging.On)Logging.Associate(Logging.HttpListener, this, httpContext);
            m_HttpContext = httpContext;
            m_MemoryBlob = memoryBlob;
            m_BoundaryType = BoundaryType.None;

            // Set up some of these now to avoid refcounting on memory blob later.
            m_RequestId = memoryBlob.RequestBlob->RequestId;
            m_ConnectionId = memoryBlob.RequestBlob->ConnectionId;
            m_SslStatus = memoryBlob.RequestBlob->pSslInfo == null ? SslStatus.Insecure :
                memoryBlob.RequestBlob->pSslInfo->SslClientCertNegotiated == 0 ? SslStatus.NoClientCert :
                SslStatus.ClientCert;
            if (memoryBlob.RequestBlob->pRawUrl != null && memoryBlob.RequestBlob->RawUrlLength > 0) {
                m_RawUrl = Marshal.PtrToStringAnsi((IntPtr) memoryBlob.RequestBlob->pRawUrl, memoryBlob.RequestBlob->RawUrlLength);
            }
            
            UnsafeNclNativeMethods.HttpApi.HTTP_COOKED_URL cookedUrl = memoryBlob.RequestBlob->CookedUrl;
            if (cookedUrl.pHost != null && cookedUrl.HostLength > 0) {
                m_CookedUrlHost = Marshal.PtrToStringUni((IntPtr)cookedUrl.pHost, cookedUrl.HostLength / 2);
            }
            if (cookedUrl.pAbsPath != null && cookedUrl.AbsPathLength > 0) {
                m_CookedUrlPath = Marshal.PtrToStringUni((IntPtr)cookedUrl.pAbsPath, cookedUrl.AbsPathLength / 2);
            }
            if (cookedUrl.pQueryString != null && cookedUrl.QueryStringLength > 0) {
                m_CookedUrlQuery = Marshal.PtrToStringUni((IntPtr)cookedUrl.pQueryString, cookedUrl.QueryStringLength / 2);
            }
            m_Version = new Version(memoryBlob.RequestBlob->Version.MajorVersion, memoryBlob.RequestBlob->Version.MinorVersion);
            m_ClientCertState = ListenerClientCertState.NotInitialized;
            m_KeepAlive = TriState.Unspecified;
            GlobalLog.Print("HttpListenerContext#" + ValidationHelper.HashString(this) + "::.ctor() RequestId:" + RequestId + " ConnectionId:" + m_ConnectionId + " RawConnectionId:" + memoryBlob.RequestBlob->RawConnectionId + " UrlContext:" + memoryBlob.RequestBlob->UrlContext + " RawUrl:" + m_RawUrl + " Version:" + m_Version.ToString() + " Secure:" + m_SslStatus.ToString());
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "httpContext#"+ ValidationHelper.HashString(httpContext)+ " RequestUri:" + ValidationHelper.ToString(RequestUri) + " Content-Length:" + ValidationHelper.ToString(ContentLength64) + " HTTP Method:" + ValidationHelper.ToString(HttpMethod));
            // Log headers
            if(Logging.On) {
                StringBuilder sb = new StringBuilder("HttpListenerRequest Headers:\n");
                for (int i=0; i<Headers.Count; i++) {
                    sb.Append("\t");
                    sb.Append(Headers.GetKey(i));
                    sb.Append(" : ");
                    sb.Append(Headers.Get(i));
                    sb.Append("\n");
                }
                Logging.PrintInfo(Logging.HttpListener, this, ".ctor", sb.ToString());
            }
        }

        internal HttpListenerContext HttpListenerContext {
            get {
                return m_HttpContext;
            }
        }

        // Note: RequestBuffer may get moved in memory. If you dereference a pointer from inside the RequestBuffer, 
        // you must use 'OriginalBlobAddress' below to adjust the location of the pointer to match the location of
        // RequestBuffer.
        // 

        internal byte[] RequestBuffer
        {
            get
            {
                CheckDisposed();
                return m_MemoryBlob.RequestBuffer;
            }
        }

        internal IntPtr OriginalBlobAddress
        {
            get
            {
                CheckDisposed();
                return m_MemoryBlob.OriginalBlobAddress;
            }
        }

        // Use this to save the blob from dispose if this object was never used (never given to a user) and is about to be
        // disposed.
        internal void DetachBlob(RequestContextBase memoryBlob)
        {
            if (memoryBlob != null && (object) memoryBlob == (object) m_MemoryBlob)
            {
                m_MemoryBlob = null;
            }
        }

        // Finalizes ownership of the memory blob.  DetachBlob can't be called after this.
        internal void ReleasePins()
        {
            m_MemoryBlob.ReleasePins();
        }

        public Guid RequestTraceIdentifier {
            get {
                Guid guid = new Guid();
                *(1+ (ulong *) &guid) = RequestId;
                return guid;
            }
        }

        internal ulong RequestId {
            get {
                return m_RequestId;
            }
        }

        public /* override */ string[] AcceptTypes {
            get {
                return Helpers.ParseMultivalueHeader(GetKnownHeader(HttpRequestHeader.Accept));
            }
        }

        public /* override */ Encoding ContentEncoding {
            get {
                if (UserAgent!=null && CultureInfo.InvariantCulture.CompareInfo.IsPrefix(UserAgent, "UP")) {
                    string postDataCharset = Headers["x-up-devcap-post-charset"];
                    if (postDataCharset!=null && postDataCharset.Length>0) {
                        try {
                            return Encoding.GetEncoding(postDataCharset);
                        }
                        catch (ArgumentException) {
                        }
                    }
                }
                if (HasEntityBody) {
                    if (ContentType!=null) {
                        string charSet = Helpers.GetAttributeFromHeader(ContentType, "charset");
                        if (charSet!=null) {
                            try {
                                return Encoding.GetEncoding(charSet);
                            }
                            catch (ArgumentException) {
                            }
                        }
                    }
                }
                return Encoding.Default;
            }
        }

        public /* override */ long ContentLength64 {
            get {
                if (m_BoundaryType==BoundaryType.None) {
                    if (HttpWebRequest.ChunkedHeader.Equals(GetKnownHeader(HttpRequestHeader.TransferEncoding), 
                        StringComparison.OrdinalIgnoreCase)) {
                        m_BoundaryType = BoundaryType.Chunked;
                        m_ContentLength = -1;
                    }
                    else {
                        m_ContentLength = 0;
                        m_BoundaryType = BoundaryType.ContentLength;
                        string length = GetKnownHeader(HttpRequestHeader.ContentLength);
                        if (length!=null) {
                            bool success = long.TryParse(length, NumberStyles.None,  CultureInfo.InvariantCulture.NumberFormat, out m_ContentLength);
                            if (!success) {
                                m_ContentLength = 0;
                                m_BoundaryType = BoundaryType.Invalid;
                            }
                        }
                    }
                }
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ContentLength_get() returning m_ContentLength:" + m_ContentLength + " m_BoundaryType:" + m_BoundaryType);
                return m_ContentLength;
            }
        }

        public /* override */ string ContentType {
            get {
                return GetKnownHeader(HttpRequestHeader.ContentType);
            }
        }

        public /* override */ NameValueCollection Headers {
            get {
                if (m_WebHeaders==null) {
                    m_WebHeaders = UnsafeNclNativeMethods.HttpApi.GetHeaders(RequestBuffer, OriginalBlobAddress);
                }
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::Headers_get() returning#" + ValidationHelper.HashString(m_WebHeaders));
                return m_WebHeaders;
            }
        }

        public /* override */ string HttpMethod {
            get {
                if (m_HttpMethod==null) {
                    m_HttpMethod = UnsafeNclNativeMethods.HttpApi.GetVerb(RequestBuffer, OriginalBlobAddress);
                }
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::HttpMethod_get() returning m_HttpMethod:" + ValidationHelper.ToString(m_HttpMethod));
                return m_HttpMethod;
            }
        }

        public /* override */ Stream InputStream {
            get {
                if(Logging.On)Logging.Enter(Logging.HttpListener, this, "InputStream_get", "");
                if (m_RequestStream==null) {
                    m_RequestStream = HasEntityBody ? new HttpRequestStream(HttpListenerContext) : Stream.Null;
                }
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "InputStream_get", "");
                return m_RequestStream;
            }
        }

        // Requires ControlPrincipal permission if the request was authenticated with Negotiate, NTLM, or Digest.
        public /* override */ bool IsAuthenticated {
            get {
                IPrincipal user = HttpListenerContext.User;
                return user != null && user.Identity != null && user.Identity.IsAuthenticated;
            }
        }

        public /* override */ bool IsLocal {
            get {
                return LocalEndPoint.Address.Equals(RemoteEndPoint.Address);
            }
        }

        public /* override */ bool IsSecureConnection {
            get {
                return m_SslStatus != SslStatus.Insecure;
            }
        }

        public bool IsWebSocketRequest 
        {
            get
            {
                if (!WebSocketProtocolComponent.IsSupported)
                {
                    return false;
                }

                bool foundConnectionUpgradeHeader = false;
                if (string.IsNullOrEmpty(this.Headers[HttpKnownHeaderNames.Connection]) || string.IsNullOrEmpty(this.Headers[HttpKnownHeaderNames.Upgrade]))
                {
                    return false; 
                }

                foreach (string connection in this.Headers.GetValues(HttpKnownHeaderNames.Connection)) 
                {
                    if (string.Compare(connection, HttpKnownHeaderNames.Upgrade, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        foundConnectionUpgradeHeader = true;
                        break;
                    }
                }

                if (!foundConnectionUpgradeHeader)
                {
                    return false; 
                }

                foreach (string upgrade in this.Headers.GetValues(HttpKnownHeaderNames.Upgrade))
                {
                    if (string.Compare(upgrade, WebSocketHelpers.WebSocketUpgradeToken, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }

                return false; 
            }
        }

        public /* override */ NameValueCollection QueryString {
            get {
                NameValueCollection queryString = new NameValueCollection();
                Helpers.FillFromString(queryString, Url.Query, true, ContentEncoding);
                return queryString;
            }
        }

        public /* override */ string RawUrl {
            get {
                return m_RawUrl;
            }
        }

        public string ServiceName
        {
            get { return m_ServiceName; }
            internal set { m_ServiceName = value; }
        }

        public /* override */ Uri Url {
            get {
                return RequestUri;
            }
        }

        public /* override */ Uri UrlReferrer {
            get {
                string referrer = GetKnownHeader(HttpRequestHeader.Referer);
                if (referrer==null) {
                    return null;
                }
                Uri urlReferrer;
                bool success = Uri.TryCreate(referrer, UriKind.RelativeOrAbsolute, out urlReferrer);
                return success ? urlReferrer : null;
            }
        }

        public /* override */ string UserAgent {
            get {
                return GetKnownHeader(HttpRequestHeader.UserAgent);
            }
        }

        public /* override */ string UserHostAddress {
            get {
                return LocalEndPoint.ToString();
            }
        }

        public /* override */ string UserHostName {
            get {
                return GetKnownHeader(HttpRequestHeader.Host);
            }
        }

        public /* override */ string[] UserLanguages {
            get {
                return Helpers.ParseMultivalueHeader(GetKnownHeader(HttpRequestHeader.AcceptLanguage));
            }
        }

        public int ClientCertificateError {
            get {
                if (m_ClientCertState == ListenerClientCertState.NotInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.net_listener_mustcall, "GetClientCertificate()/BeginGetClientCertificate()"));
                else if (m_ClientCertState == ListenerClientCertState.InProgress)
                    throw new InvalidOperationException(SR.GetString(SR.net_listener_mustcompletecall, "GetClientCertificate()/BeginGetClientCertificate()"));

                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ClientCertificateError_get() returning ClientCertificateError:" + ValidationHelper.ToString(m_ClientCertificateError));
                return m_ClientCertificateError;
            }
        }

        internal X509Certificate2 ClientCertificate {
            set {
                m_ClientCertificate = value;
            }
        }

        internal ListenerClientCertState ClientCertState {
            set {
                m_ClientCertState = value;
            }
        }

        internal void SetClientCertificateError(int clientCertificateError)
        {
            m_ClientCertificateError = clientCertificateError;
        }

        public X509Certificate2 GetClientCertificate() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "GetClientCertificate", "");
            try {
                ProcessClientCertificate();
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::GetClientCertificate() returning m_ClientCertificate:" + ValidationHelper.ToString(m_ClientCertificate));
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "GetClientCertificate", ValidationHelper.ToString(m_ClientCertificate));
            }
            return m_ClientCertificate;
        }

        public IAsyncResult BeginGetClientCertificate(AsyncCallback requestCallback, object state) {
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "BeginGetClientCertificate", "");
            return AsyncProcessClientCertificate(requestCallback, state);
        }

        public X509Certificate2 EndGetClientCertificate(IAsyncResult asyncResult) {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "EndGetClientCertificate", "");
            X509Certificate2 clientCertificate = null;
            try {
                if (asyncResult==null) {
                    throw new ArgumentNullException("asyncResult");
                }
                ListenerClientCertAsyncResult clientCertAsyncResult = asyncResult as ListenerClientCertAsyncResult;
                if (clientCertAsyncResult==null || clientCertAsyncResult.AsyncObject!=this) {
                    throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult), "asyncResult");
                }
                if (clientCertAsyncResult.EndCalled) {
                    throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndGetClientCertificate"));
                }
                clientCertAsyncResult.EndCalled = true;
                clientCertificate = clientCertAsyncResult.InternalWaitForCompletion() as X509Certificate2;
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::EndGetClientCertificate() returning m_ClientCertificate:" + ValidationHelper.ToString(m_ClientCertificate));
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "EndGetClientCertificate", ValidationHelper.HashString(clientCertificate));
            }
            return clientCertificate;
        }

        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        public Task<X509Certificate2> GetClientCertificateAsync()
        {
            return Task<X509Certificate2>.Factory.FromAsync(BeginGetClientCertificate, EndGetClientCertificate, null);
        }


        public TransportContext TransportContext
        {
            get
            {
                return new HttpListenerRequestContext(this);
            }
        }

        private CookieCollection ParseCookies(Uri uri, string setCookieHeader) {
            GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ParseCookies() uri:" + uri + " setCookieHeader:" + setCookieHeader);
            CookieCollection cookies = new CookieCollection();
            CookieParser parser = new CookieParser(setCookieHeader);
            for (;;) {
                Cookie cookie = parser.GetServer();
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ParseCookies() CookieParser returned cookie:" + ValidationHelper.ToString(cookie));
                if (cookie==null) {
                    // EOF, done.
                    break;
                }
                if (cookie.Name.Length==0) {
                    continue;
                }
                cookies.InternalAdd(cookie, true);
            }
            return cookies;
        }

        public CookieCollection Cookies {
            get {
                if (m_Cookies==null) {
                    string cookieString = GetKnownHeader(HttpRequestHeader.Cookie);
                    if (cookieString!=null && cookieString.Length>0) {
                        m_Cookies = ParseCookies(RequestUri, cookieString);
                    }
                    if (m_Cookies==null) {
                        m_Cookies = new CookieCollection();
                    }
                    if (HttpListenerContext.PromoteCookiesToRfc2965) {
                        for (int index=0; index<m_Cookies.Count; index++) {
                            if (m_Cookies[index].Variant==CookieVariant.Rfc2109) {
                                m_Cookies[index].Variant = CookieVariant.Rfc2965;
                            }
                        }
                    }
                }
                return m_Cookies;
            }
        }

        public Version ProtocolVersion {
            get {
                return m_Version;
            }
        }

        public /* override */ bool HasEntityBody {
            get {
               // accessing the ContentLength property delay creates m_BoundaryType
                return (ContentLength64 > 0 && m_BoundaryType == BoundaryType.ContentLength) ||
                    m_BoundaryType == BoundaryType.Chunked || m_BoundaryType == BoundaryType.Multipart;
            }
        }

        public /* override */ bool KeepAlive
        {
            get
            {
                if (m_KeepAlive == TriState.Unspecified)
                {
                    string header = Headers[HttpKnownHeaderNames.ProxyConnection];
                    if (string.IsNullOrEmpty(header))
                    {
                        header = GetKnownHeader(HttpRequestHeader.Connection);
                    }
                    if (string.IsNullOrEmpty(header))
                    {
                        if (ProtocolVersion >= HttpVersion.Version11)
                        {
                            m_KeepAlive = TriState.True;
                        }
                        else
                        {
                            header = GetKnownHeader(HttpRequestHeader.KeepAlive);
                            m_KeepAlive = string.IsNullOrEmpty(header) ? TriState.False : TriState.True;
                        }
                    }
                    else
                    {
                        header = header.ToLower(CultureInfo.InvariantCulture);
                        m_KeepAlive = header.IndexOf("close") < 0 || header.IndexOf("keep-alive") >= 0 ? TriState.True : TriState.False;
                    }
                }

                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::KeepAlive_get() returning:" + m_KeepAlive);
                return m_KeepAlive == TriState.True;
            }
        }

        public /* override */ IPEndPoint RemoteEndPoint {
            get {
                if (m_RemoteEndPoint==null) {
                    m_RemoteEndPoint = UnsafeNclNativeMethods.HttpApi.GetRemoteEndPoint(RequestBuffer, OriginalBlobAddress);
                }
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::RemoteEndPoint_get() returning:" + m_RemoteEndPoint);
                return m_RemoteEndPoint;
            }
        }

        public /* override */ IPEndPoint LocalEndPoint {
            get {
                if (m_LocalEndPoint==null) {
                    m_LocalEndPoint = UnsafeNclNativeMethods.HttpApi.GetLocalEndPoint(RequestBuffer, OriginalBlobAddress);
                }
                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::LocalEndPoint_get() returning:" + m_LocalEndPoint);
                return m_LocalEndPoint;
            }
        }

        //should only be called from httplistenercontext
        internal void Close() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Close", "");
            RequestContextBase memoryBlob = m_MemoryBlob;
            if (memoryBlob != null)
            {
                memoryBlob.Close();
                m_MemoryBlob = null;
            }
            m_IsDisposed = true;
            if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Close", "");
        }


        private ListenerClientCertAsyncResult AsyncProcessClientCertificate(AsyncCallback requestCallback, object state) {
            if (m_ClientCertState == ListenerClientCertState.InProgress)
                throw new InvalidOperationException(SR.GetString(SR.net_listener_callinprogress, "GetClientCertificate()/BeginGetClientCertificate()"));
            m_ClientCertState = ListenerClientCertState.InProgress;
            HttpListenerContext.EnsureBoundHandle();

            ListenerClientCertAsyncResult asyncResult = null;
			//--------------------------------------------------------------------
			//When you configure the HTTP.SYS with a flag value 2
			//which means require client certificates, when the client makes the
			//initial SSL connection, server (HTTP.SYS) demands the client certificate
			//
			//Some apps may not want to demand the client cert at the beginning
			//perhaps server the default.htm. In this case the HTTP.SYS is configured
			//with a flag value other than 2, whcih means that the client certificate is
			//optional.So intially when SSL is established HTTP.SYS won't ask for client
			//certificate. This works fine for the default.htm in the case above
			//However, if the app wants to demand a client certficate at a later time
			//perhaps showing "YOUR ORDERS" page, then the server wans to demand
			//Client certs. this will inturn makes HTTP.SYS to do the
			//SEC_I_RENOGOTIATE through which the client cert demand is made
			//
			//THE 














            if (m_SslStatus != SslStatus.Insecure)
            {
                // at this point we know that DefaultFlags has the 2 bit set (Negotiate Client certificate)
                // the cert, though might or might not be there. try to retrieve it
                // this number is the same that IIS decided to use
                uint size = CertBoblSize;
                asyncResult = new ListenerClientCertAsyncResult(this, state, requestCallback, size);
                try {
                    while (true)
                    {
                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() calling UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate size:" + size);
                        uint bytesReceived = 0;

                        uint statusCode =
                            UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(
                                HttpListenerContext.RequestQueueHandle,
                                m_ConnectionId,
                                (uint)UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE,
                                asyncResult.RequestBlob,
                                size,
                                &bytesReceived,
                                asyncResult.NativeOverlapped);

                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() call to UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate returned:" + statusCode + " bytesReceived:" + bytesReceived);
                        if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA)
                        {
                            UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo = asyncResult.RequestBlob;
                            size = bytesReceived + pClientCertInfo->CertEncodedSize;
                            asyncResult.Reset(size);
                            continue;
                        }
                        if (statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                            statusCode != UnsafeNclNativeMethods.ErrorCodes.ERROR_IO_PENDING) {
                            // someother bad error, possible(?) return values are:
                            // ERROR_INVALID_HANDLE, ERROR_INSUFFICIENT_BUFFER, ERROR_OPERATION_ABORTED
                            // Also ERROR_BAD_DATA if we got it twice or it reported smaller size buffer required.
                            throw new HttpListenerException((int)statusCode);
                        }

                        if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                            HttpListener.SkipIOCPCallbackOnSuccess)
                        {
                            asyncResult.IOCompleted(statusCode, bytesReceived);
                        }
                        break;
                    }
                }
                catch {
                    if (asyncResult!=null) {
                        asyncResult.InternalCleanup();
                    }
                    throw;
                }
            } else {
                asyncResult = new ListenerClientCertAsyncResult(this, state, requestCallback, 0);
                asyncResult.InvokeCallback();
            }
            return asyncResult;
        }

        private void ProcessClientCertificate() {
            if (m_ClientCertState == ListenerClientCertState.InProgress)
                throw new InvalidOperationException(SR.GetString(SR.net_listener_callinprogress, "GetClientCertificate()/BeginGetClientCertificate()"));
            m_ClientCertState = ListenerClientCertState.InProgress;
            GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate()");
			//--------------------------------------------------------------------
			//When you configure the HTTP.SYS with a flag value 2
			//which means require client certificates, when the client makes the
			//initial SSL connection, server (HTTP.SYS) demands the client certificate
			//
			//Some apps may not want to demand the client cert at the beginning
			//perhaps server the default.htm. In this case the HTTP.SYS is configured
			//with a flag value other than 2, whcih means that the client certificate is
			//optional.So intially when SSL is established HTTP.SYS won't ask for client
			//certificate. This works fine for the default.htm in the case above
			//However, if the app wants to demand a client certficate at a later time
			//perhaps showing "YOUR ORDERS" page, then the server wans to demand
			//Client certs. this will inturn makes HTTP.SYS to do the
			//SEC_I_RENOGOTIATE through which the client cert demand is made
			//
			//THE 














            if (m_SslStatus != SslStatus.Insecure)
            {
                // at this point we know that DefaultFlags has the 2 bit set (Negotiate Client certificate)
                // the cert, though might or might not be there. try to retrieve it
                // this number is the same that IIS decided to use
                uint size = CertBoblSize;
                while (true)
                {
                    byte[] clientCertInfoBlob = new byte[checked((int) size)];
                    fixed (byte* pClientCertInfoBlob = clientCertInfoBlob)
                    {
                        UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO* pClientCertInfo = (UnsafeNclNativeMethods.HttpApi.HTTP_SSL_CLIENT_CERT_INFO*) pClientCertInfoBlob;

                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() calling UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate size:" + size);
                        uint bytesReceived = 0;

                        uint statusCode =
                            UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate(
                                HttpListenerContext.RequestQueueHandle,
                                m_ConnectionId,
                                (uint)UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE,
                                pClientCertInfo,
                                size,
                                &bytesReceived,
                                null);

                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() call to UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate returned:" + statusCode + " bytesReceived:" + bytesReceived);
                        if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_MORE_DATA) {
                            size = bytesReceived + pClientCertInfo->CertEncodedSize;
                            continue;
                        }
                        else if (statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS) {
                            if (pClientCertInfo!=null) {
                                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() pClientCertInfo:" + ValidationHelper.ToString((IntPtr)pClientCertInfo)
                                    + " pClientCertInfo->CertFlags:" + ValidationHelper.ToString(pClientCertInfo->CertFlags)
                                    + " pClientCertInfo->CertEncodedSize:" + ValidationHelper.ToString(pClientCertInfo->CertEncodedSize)
                                    + " pClientCertInfo->pCertEncoded:" + ValidationHelper.ToString((IntPtr)pClientCertInfo->pCertEncoded)
                                    + " pClientCertInfo->Token:" + ValidationHelper.ToString((IntPtr)pClientCertInfo->Token)
                                    + " pClientCertInfo->CertDeniedByMapper:" + ValidationHelper.ToString(pClientCertInfo->CertDeniedByMapper));
                                if (pClientCertInfo->pCertEncoded!=null) {
                                    try {
                                        byte[] certEncoded = new byte[pClientCertInfo->CertEncodedSize];
                                        Marshal.Copy((IntPtr)pClientCertInfo->pCertEncoded, certEncoded, 0, certEncoded.Length);
                                        m_ClientCertificate = new X509Certificate2(certEncoded);
                                    }
                                    catch (CryptographicException exception) {
                                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() caught CryptographicException in X509Certificate2..ctor():" + ValidationHelper.ToString(exception));
                                    }
                                    catch (SecurityException exception) {
                                        GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::ProcessClientCertificate() caught SecurityException in X509Certificate2..ctor():" + ValidationHelper.ToString(exception));
                                    }
                                }
                                m_ClientCertificateError = (int)pClientCertInfo->CertFlags;
                            }
                        }
                        else {
                            GlobalLog.Assert(statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_NOT_FOUND, "HttpListenerRequest#{0}::ProcessClientCertificate()|Call to UnsafeNclNativeMethods.HttpApi.HttpReceiveClientCertificate() failed with statusCode {1}.", ValidationHelper.HashString(this), statusCode);
                        }
                    }
                    break;
                }
            }
            m_ClientCertState = ListenerClientCertState.Completed;
        }

        private string RequestScheme {
            get {
                return IsSecureConnection ? "https" : "http";
            }
        }

        private Uri RequestUri {
            get {
                if (m_RequestUri == null) {

                    m_RequestUri = HttpListenerRequestUriBuilder.GetRequestUri(
                        m_RawUrl, RequestScheme, m_CookedUrlHost, m_CookedUrlPath, m_CookedUrlQuery);
                }

                GlobalLog.Print("HttpListenerRequest#" + ValidationHelper.HashString(this) + "::RequestUri_get() returning m_RequestUri:" + ValidationHelper.ToString(m_RequestUri));
                return m_RequestUri;
            }
        }

        /*
        private DateTime IfModifiedSince {
            get {
                string headerValue = GetKnownHeader(HttpRequestHeader.IfModifiedSince);
                if (headerValue==null) {
                    return DateTime.Now;
                }
                return DateTime.Parse(headerValue, CultureInfo.InvariantCulture);
            }
        }
        */

        private string GetKnownHeader(HttpRequestHeader header) {
            return UnsafeNclNativeMethods.HttpApi.GetKnownHeader(RequestBuffer, OriginalBlobAddress, (int) header);
        }

        internal ChannelBinding GetChannelBinding()
        {
            return HttpListenerContext.Listener.GetChannelBindingFromTls(m_ConnectionId);
        }

        internal IEnumerable<TokenBinding> GetTlsTokenBindings() {
        
            // Try to get the token binding if not created.
            if (Volatile.Read(ref m_TokenBindings) == null)
            {
                lock (m_Lock)
                {
                    if (Volatile.Read(ref m_TokenBindings) == null)
                    {
                        // If token binding is supported on the machine get it else create empty list.
                        if (UnsafeNclNativeMethods.TokenBindingOSHelper.SupportsTokenBinding)
                        {
                            ProcessTlsTokenBindings();
                        }
                        else
                        {
                            m_TokenBindings = new List<TokenBinding>();
                        }
                    }
                }
            }

            // If the cached status is not success throw exception, else return the token binding
            if (0 != m_TokenBindingVerifyMessageStatus)
            {
                throw new HttpListenerException(m_TokenBindingVerifyMessageStatus);
            }
            else
            {
                return m_TokenBindings.AsReadOnly();
            }
        }

        /// <summary>
        /// Process the token binding information in the request and populate m_TokenBindings 
        /// This method should be called once only as token binding information is cached in m_TokenBindings for further use.
        /// </summary>
        private void ProcessTlsTokenBindings() {

            Debug.Assert(m_TokenBindings == null);

            if (m_TokenBindings != null)
            {
                return;
            }

            m_TokenBindings = new List<TokenBinding>();
            UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_TOKEN_BINDING_INFO* pTokenBindingInfo = UnsafeNclNativeMethods.HttpApi.GetTlsTokenBindingRequestInfo(RequestBuffer, OriginalBlobAddress);
            UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_TOKEN_BINDING_INFO_V1* pTokenBindingInfo_V1 = null;
            bool useV1TokenBinding = false; 

            // Only try to collect the old binding information if there is no V2 binding information available
            if (pTokenBindingInfo == null)
            {
                pTokenBindingInfo_V1 = UnsafeNclNativeMethods.HttpApi.GetTlsTokenBindingRequestInfo_V1(RequestBuffer, OriginalBlobAddress);
                useV1TokenBinding = true;
            }

            if (pTokenBindingInfo == null && pTokenBindingInfo_V1 == null)
            {
                // The current request isn't over TLS or the client or server doesn't support the token binding
                // protocol. This isn't an error; just return "nothing here".
                return;
            }

            UnsafeNclNativeMethods.HttpApi.HeapAllocHandle handle = null;
            m_TokenBindingVerifyMessageStatus = -1;
                        
            fixed (byte* pMemoryBlob = RequestBuffer){
                UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_V2* request = (UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_V2*)pMemoryBlob;
                long fixup = pMemoryBlob - (byte*) OriginalBlobAddress;
                
                if (useV1TokenBinding && pTokenBindingInfo_V1 != null)
                {
                    // Old V1 Token Binding protocol is still being used, so we need to verify the binding message using the old API
                    m_TokenBindingVerifyMessageStatus = UnsafeNclNativeMethods.HttpApi.TokenBindingVerifyMessage_V1(
                        pTokenBindingInfo_V1->TokenBinding + fixup,
                        pTokenBindingInfo_V1->TokenBindingSize,
                        (IntPtr)((byte*)(pTokenBindingInfo_V1->KeyType) + fixup),
                        pTokenBindingInfo_V1->TlsUnique + fixup,
                        pTokenBindingInfo_V1->TlsUniqueSize,
                        out handle);
                }
                else
                {
                    // Use the V2 token binding behavior 
                    m_TokenBindingVerifyMessageStatus =
                        UnsafeNclNativeMethods.HttpApi.TokenBindingVerifyMessage(
                            pTokenBindingInfo->TokenBinding + fixup,
                            pTokenBindingInfo->TokenBindingSize,
                            pTokenBindingInfo->KeyType,
                            pTokenBindingInfo->TlsUnique + fixup,
                            pTokenBindingInfo->TlsUniqueSize,
                            out handle);
                }
            }

            if (m_TokenBindingVerifyMessageStatus != 0)
            {
                throw new HttpListenerException(m_TokenBindingVerifyMessageStatus);
            }
            
            Debug.Assert(handle != null);
            Debug.Assert(!handle.IsInvalid);

            using (handle)
            {
                // If we have an old binding, use the old binding behavior 
                if (useV1TokenBinding)
                {
                    GenerateTokenBindings_V1(handle);
                }
                else
                {
                    GenerateTokenBindings(handle);
                }
            }
        }

        /// <summary>
        /// Method to allow current bindings to be returned 
        /// </summary>
        /// <param name="handle"></param>
        private void GenerateTokenBindings(UnsafeNclNativeMethods.HttpApi.HeapAllocHandle handle)
        {
            UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_LIST* pResultList = (UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_LIST*)handle.DangerousGetHandle();
            for (int i = 0; i < pResultList->resultCount; i++)
            {
                UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_DATA* pThisResultData = &pResultList->resultData[i];

                if (pThisResultData != null)
                {
                    byte[] retVal = new byte[pThisResultData->identifierSize];
                    Marshal.Copy((IntPtr)(pThisResultData->identifierData), retVal, 0, retVal.Length);

                    if (pThisResultData->bindingType == UnsafeNclNativeMethods.HttpApi.TOKENBINDING_TYPE.TOKENBINDING_TYPE_PROVIDED)
                    {
                        m_TokenBindings.Add(new TokenBinding(TokenBindingType.Provided, retVal));
                    }
                    else if (pThisResultData->bindingType == UnsafeNclNativeMethods.HttpApi.TOKENBINDING_TYPE.TOKENBINDING_TYPE_REFERRED)
                    {
                        m_TokenBindings.Add(new TokenBinding(TokenBindingType.Referred, retVal));
                    }
                }
            }
        }

        /// <summary>
        /// Compat method to allow V1 bindings to be returned
        /// </summary>
        /// <param name="handle"></param>
        private void GenerateTokenBindings_V1(UnsafeNclNativeMethods.HttpApi.HeapAllocHandle handle)
        {
            UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_LIST_V1* pResultList = (UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_LIST_V1*)handle.DangerousGetHandle();
            for (int i = 0; i < pResultList->resultCount; i++)
            {
                UnsafeNclNativeMethods.HttpApi.TOKENBINDING_RESULT_DATA_V1* pThisResultData = &pResultList->resultData[i];

                if (pThisResultData != null)
                {
                    // Old V1 Token Binding protocol is still being used, so we need modify the binding message using the old behavior
                    
                    // Per http://tools.ietf.org/html/draft-ietf-tokbind-protocol-00, Sec. 4,
                    // We'll strip off the token binding type and return the remainder as an opaque blob.
                    Debug.Assert((long)(&pThisResultData->identifierData->hashAlgorithm) == (long)(pThisResultData->identifierData) + 1 );
                    byte[] retVal = new byte[pThisResultData->identifierSize - 1];
                    Marshal.Copy((IntPtr)(&pThisResultData->identifierData->hashAlgorithm), retVal, 0, retVal.Length);

                    if (pThisResultData->identifierData->bindingType == UnsafeNclNativeMethods.HttpApi.TOKENBINDING_TYPE.TOKENBINDING_TYPE_PROVIDED)
                    {
                        m_TokenBindings.Add(new TokenBinding(TokenBindingType.Provided, retVal));
                    }
                    else if (pThisResultData->identifierData->bindingType == UnsafeNclNativeMethods.HttpApi.TOKENBINDING_TYPE.TOKENBINDING_TYPE_REFERRED)
                    {
                        m_TokenBindings.Add(new TokenBinding(TokenBindingType.Referred, retVal));
                    }
                }
            }
        }

        internal void CheckDisposed() {
            if (m_IsDisposed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        // <




        static class Helpers {
            //
            // Get attribute off header value
            //
            internal static String GetAttributeFromHeader(String headerValue, String attrName) {
                if (headerValue == null)
                    return null;

                int l = headerValue.Length;
                int k = attrName.Length;

               // find properly separated attribute name
                int i = 1; // start searching from 1

                while (i < l) {
                    i = CultureInfo.InvariantCulture.CompareInfo.IndexOf(headerValue, attrName, i, CompareOptions.IgnoreCase);
                    if (i < 0)
                        break;
                    if (i+k >= l)
                        break;

                    char chPrev = headerValue[i-1];
                    char chNext = headerValue[i+k];
                    if ((chPrev == ';' || chPrev == ',' || Char.IsWhiteSpace(chPrev)) && (chNext == '=' || Char.IsWhiteSpace(chNext)))
                        break;

                    i += k;
                }

                if (i < 0 || i >= l)
                    return null;

               // skip to '=' and the following whitespaces
                i += k;
                while (i < l && Char.IsWhiteSpace(headerValue[i]))
                    i++;
                if (i >= l || headerValue[i] != '=')
                    return null;
                i++;
                while (i < l && Char.IsWhiteSpace(headerValue[i]))
                    i++;
                if (i >= l)
                    return null;

               // parse the value
                String attrValue = null;

                int j;

                if (i < l && headerValue[i] == '"') {
                    if (i == l-1)
                        return null;
                    j = headerValue.IndexOf('"', i+1);
                    if (j < 0 || j == i+1)
                        return null;

                    attrValue = headerValue.Substring(i+1, j-i-1).Trim();
                }
                else {
                    for (j = i; j < l; j++) {
                        if (headerValue[j] == ' ' || headerValue[j] == ',')
                            break;
                    }

                    if (j == i)
                        return null;

                    attrValue = headerValue.Substring(i, j-i).Trim();
                }

                return attrValue;
            }

            internal static String[] ParseMultivalueHeader(String s) {
                if (s == null)
                    return null;

                int l = s.Length;

               // collect comma-separated values into list

                ArrayList values = new ArrayList();
                int i = 0;

                while (i < l) {
                   // find next ,
                    int ci = s.IndexOf(',', i);
                    if (ci < 0)
                        ci = l;

                   // append corresponding server value
                    values.Add(s.Substring(i, ci-i));

                   // move to next
                    i = ci+1;

                   // skip leading space
                    if (i < l && s[i] == ' ')
                        i++;
                }

               // return list as array of strings

                int n = values.Count;
                String[] strings;

                // if n is 0 that means s was empty string

                if (n == 0) {
                    strings = new String[1];
                    strings[0] = String.Empty;
                }
                else {
                    strings = new String[n];
                    values.CopyTo(0, strings, 0, n);
                }
                return strings;
            }


            private static string UrlDecodeStringFromStringInternal(string s, Encoding e) {
                int count = s.Length;
                UrlDecoder helper = new UrlDecoder(count, e);

                // go through the string's chars collapsing %XX and %uXXXX and
                // appending each char as char, with exception of %XX constructs
                // that are appended as bytes

                for (int pos = 0; pos < count; pos++) {
                    char ch = s[pos];

                    if (ch == '+') {
                        ch = ' ';
                    }
                    else if (ch == '%' && pos < count-2) {
                        if (s[pos+1] == 'u' && pos < count-5) {
                            int h1 = HexToInt(s[pos+2]);
                            int h2 = HexToInt(s[pos+3]);
                            int h3 = HexToInt(s[pos+4]);
                            int h4 = HexToInt(s[pos+5]);

                            if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0) {   // valid 4 hex chars
                                ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                                pos += 5;

                                // only add as char
                                helper.AddChar(ch);
                                continue;
                            }
                        }
                        else {
                            int h1 = HexToInt(s[pos+1]);
                            int h2 = HexToInt(s[pos+2]);

                            if (h1 >= 0 && h2 >= 0) {     // valid 2 hex chars
                                byte b = (byte)((h1 << 4) | h2);
                                pos += 2;

                                // don't add as char
                                helper.AddByte(b);
                                continue;
                            }
                        }
                    }

                    if ((ch & 0xFF80) == 0)
                        helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                    else
                        helper.AddChar(ch);
                }

                return helper.GetString();
            }

            private static int HexToInt(char h) {
                return( h >= '0' && h <= '9' ) ? h - '0' :
                ( h >= 'a' && h <= 'f' ) ? h - 'a' + 10 :
                ( h >= 'A' && h <= 'F' ) ? h - 'A' + 10 :
                -1;
            }

            private class UrlDecoder {
                private int _bufferSize;

                // Accumulate characters in a special array
                private int _numChars;
                private char[] _charBuffer;

                // Accumulate bytes for decoding into characters in a special array
                private int _numBytes;
                private byte[] _byteBuffer;

                // Encoding to convert chars to bytes
                private Encoding _encoding;

                private void FlushBytes() {
                    if (_numBytes > 0) {
                        _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                        _numBytes = 0;
                    }
                }

                internal UrlDecoder(int bufferSize, Encoding encoding) {
                    _bufferSize = bufferSize;
                    _encoding = encoding;

                    _charBuffer = new char[bufferSize];
                    // byte buffer created on demand
                }

                internal void AddChar(char ch) {
                    if (_numBytes > 0)
                        FlushBytes();

                    _charBuffer[_numChars++] = ch;
                }

                internal void AddByte(byte b) {
                    // if there are no pending bytes treat 7 bit bytes as characters
                    // this optimization is temp disable as it doesn't work for some encodings
    /*
                    if (_numBytes == 0 && ((b & 0x80) == 0)) {
                        AddChar((char)b);
                    }
                    else
    */
                    {
                        if (_byteBuffer == null)
                            _byteBuffer = new byte[_bufferSize];

                        _byteBuffer[_numBytes++] = b;
                    }
                }

                internal String GetString() {
                    if (_numBytes > 0)
                        FlushBytes();

                    if (_numChars > 0)
                        return new String(_charBuffer, 0, _numChars);
                    else
                        return String.Empty;
                }
            }


            internal static void FillFromString(NameValueCollection nvc, String s, bool urlencoded, Encoding encoding) {
                int l = (s != null) ? s.Length : 0;
                int i = (s.Length>0 && s[0]=='?') ? 1 : 0;

                while (i < l) {
                    // find next & while noting first = on the way (and if there are more)

                    int si = i;
                    int ti = -1;

                    while (i < l) {
                        char ch = s[i];

                        if (ch == '=') {
                            if (ti < 0)
                                ti = i;
                        }
                        else if (ch == '&') {
                            break;
                        }

                        i++;
                    }

                    // extract the name / value pair

                    String name = null;
                    String value = null;

                    if (ti >= 0) {
                        name = s.Substring(si, ti-si);
                        value = s.Substring(ti+1, i-ti-1);
                    }
                    else {
                        value = s.Substring(si, i-si);
                    }

                    // add name / value pair to the collection

                    if (urlencoded)
                        nvc.Add(
                           name == null ? null : UrlDecodeStringFromStringInternal(name, encoding),
                           UrlDecodeStringFromStringInternal(value, encoding));
                    else
                        nvc.Add(name, value);

                    // trailing '&'

                    if (i == l-1 && s[i] == '&')
                        nvc.Add(null, "");

                    i++;
                }
            }
        }
    }
}
