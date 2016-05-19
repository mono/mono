//------------------------------------------------------------------------------
// <copyright file="HttpListenerResponse.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.ComponentModel;

    public sealed unsafe class HttpListenerResponse : /* BaseHttpResponse, */ IDisposable {

        enum ResponseState {
            Created,
            ComputedHeaders,
            SentHeaders,
            Closed,
        }
        
        private Encoding m_ContentEncoding;
        private CookieCollection m_Cookies;

        private string m_StatusDescription;
        private bool m_KeepAlive;
        private ResponseState m_ResponseState;
        private WebHeaderCollection m_WebHeaders;
        private HttpResponseStream m_ResponseStream;
        private long m_ContentLength;
        private BoundaryType m_BoundaryType;
        private UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE m_NativeResponse;

        private HttpListenerContext m_HttpContext;

        internal HttpListenerResponse() {
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, ".ctor", "");
            m_NativeResponse = new UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE();
            m_WebHeaders = new WebHeaderCollection(WebHeaderCollectionType.HttpListenerResponse);
            m_BoundaryType = BoundaryType.None;
            m_NativeResponse.StatusCode = (ushort)HttpStatusCode.OK;
            m_NativeResponse.Version.MajorVersion = 1;
            m_NativeResponse.Version.MinorVersion = 1;
            m_KeepAlive = true;
            m_ResponseState = ResponseState.Created;
        }

        internal HttpListenerResponse(HttpListenerContext httpContext) : this() {
            if(Logging.On)Logging.Associate(Logging.HttpListener, this, httpContext);
            m_HttpContext = httpContext;
        }

        private HttpListenerContext HttpListenerContext {
            get {
                return m_HttpContext;
            }
        }

        private HttpListenerRequest HttpListenerRequest {
            get {
                return HttpListenerContext.Request;
            }
        }

        public /* override */ Encoding ContentEncoding {
            get {
                return m_ContentEncoding;
            }
            set {
                m_ContentEncoding = value;
            }
        }

        public /* override */ string ContentType {
            get {
                return Headers[HttpResponseHeader.ContentType];
            }
            set {
                CheckDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    Headers.Remove(HttpResponseHeader.ContentType);
                }
                else
                {
                    Headers.Set(HttpResponseHeader.ContentType, value);
                }
            }
        }

        public /* override */ Stream OutputStream {
            get {
                CheckDisposed();
                EnsureResponseStream();
                return m_ResponseStream;
            }
        }

        public /* override */ string RedirectLocation {
            get {
                return Headers[HttpResponseHeader.Location];
            }
            set {
                // note that this doesn't set the status code to a redirect one
                CheckDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    Headers.Remove(HttpResponseHeader.Location);
                }
                else
                {
                    Headers.Set(HttpResponseHeader.Location, value);
                }
            }
        }

        public /* override */ int StatusCode {
            get {
                return (int)m_NativeResponse.StatusCode;
            }
            set {
                CheckDisposed();
                if (value<100 || value>999) {
                    throw new ProtocolViolationException(SR.GetString(SR.net_invalidstatus));
                }
                m_NativeResponse.StatusCode = (ushort)value;
            }
        }

        public /* override */ string StatusDescription {
            get {
                if (m_StatusDescription==null) {
                    // if the user hasn't set this, generated on the fly, if possible.
                    // We know this one is safe, no need to verify it as in the setter.
                    m_StatusDescription = HttpStatusDescription.Get(StatusCode);
                }
                if (m_StatusDescription==null) {
                    m_StatusDescription = string.Empty;
                }
                return m_StatusDescription;
            }
            set {
                CheckDisposed();
                if (value==null) {
                    throw new ArgumentNullException("value");
                }

                // Need to verify the status description doesn't contain any control characters except HT.  We mask off the high
                // byte since that's how it's encoded.
                for (int i = 0; i < value.Length; i++)
                {
                    char c = (char) (0x000000ff & (uint) value[i]);
                    if ((c <= 31 && c != (byte) '\t') || c == 127)
                    {
                        throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidControlChars), "name");
                    }
                }

                m_StatusDescription = value;
            }
        }

        public CookieCollection Cookies {
            get {
                if (m_Cookies==null) {
                    m_Cookies = new CookieCollection(false);
                }
                return m_Cookies;
            }
            set {
                m_Cookies = value;
            }
        }

        public void CopyFrom(HttpListenerResponse templateResponse) {
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "CopyFrom", "templateResponse#"+ValidationHelper.HashString(templateResponse));
            m_NativeResponse = new UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE();
            m_ResponseState = ResponseState.Created;
            m_WebHeaders = templateResponse.m_WebHeaders;
            m_BoundaryType = templateResponse.m_BoundaryType;
            m_ContentLength = templateResponse.m_ContentLength;
            m_NativeResponse.StatusCode = templateResponse.m_NativeResponse.StatusCode;
            m_NativeResponse.Version.MajorVersion = templateResponse.m_NativeResponse.Version.MajorVersion;
            m_NativeResponse.Version.MinorVersion = templateResponse.m_NativeResponse.Version.MinorVersion;
            m_StatusDescription = templateResponse.m_StatusDescription;
            m_KeepAlive = templateResponse.m_KeepAlive;
        }

        public bool SendChunked{
            get{
                return (EntitySendFormat == EntitySendFormat.Chunked);
            }
            set{
                if(value)
                {
                    EntitySendFormat = EntitySendFormat.Chunked;
                }
                else{
                    EntitySendFormat = EntitySendFormat.ContentLength;
                }
            }
        }

        // We MUST NOT send message-body when we send responses with these Status codes
        private static readonly int[] s_NoResponseBody = { 100, 101, 204, 205, 304 };

        private bool CanSendResponseBody(int responseCode) {
            for (int i = 0; i < s_NoResponseBody.Length; i++) {
                if (responseCode == s_NoResponseBody[i]) {
                    return false;                    
                }
            }
            return true;         
        }
       
        internal EntitySendFormat EntitySendFormat {
            get {
                return (EntitySendFormat)m_BoundaryType;
            }
            set {
                CheckDisposed();
                if (m_ResponseState>=ResponseState.SentHeaders) {
                    throw new InvalidOperationException(SR.GetString(SR.net_rspsubmitted));
                }
                if (value==EntitySendFormat.Chunked && HttpListenerRequest.ProtocolVersion.Minor==0) {
                    throw new ProtocolViolationException(SR.GetString(SR.net_nochunkuploadonhttp10));
                }
                m_BoundaryType = (BoundaryType)value;
                if (value!=EntitySendFormat.ContentLength) {
                    m_ContentLength = -1;
                }
            }
        }

        public /* override */ bool KeepAlive {
            get {
                return m_KeepAlive;
            }
            set {
                CheckDisposed();
                m_KeepAlive = value;
            }
        }

        public WebHeaderCollection Headers {
            get {
                return m_WebHeaders;
            }
            set {
                m_WebHeaders.Clear();
                foreach (string headerName in value.AllKeys)
                {
                    m_WebHeaders.Add(headerName, value[headerName]);
                }
            }
        }

        public /* override */ void AddHeader(string name, string value) {
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "AddHeader", " name="+name+" value="+value);
            Headers.SetInternal(name, value);
        }

        public /* override */ void AppendHeader(string name, string value) {
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "AppendHeader", " name="+name+" value="+value);
            Headers.Add(name, value);
        }

        public /* override */ void Redirect(string url) {
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "Redirect", " url="+url);
            Headers.SetInternal(HttpResponseHeader.Location, url);
            StatusCode = (int)HttpStatusCode.Redirect;
            StatusDescription = HttpStatusDescription.Get(StatusCode);
        }

        public void AppendCookie(Cookie cookie) {
            if (cookie==null) {
                throw new ArgumentNullException("cookie");
            }
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "AppendCookie", " cookie#"+ValidationHelper.HashString(cookie));
            Cookies.Add(cookie);
        }

        public void SetCookie(Cookie cookie) {
            if (cookie==null) {
                throw new ArgumentNullException("cookie");
            }
            Cookie new_cookie = cookie.Clone();
            int added = Cookies.InternalAdd(new_cookie, true);
            if(Logging.On)Logging.PrintInfo(Logging.HttpListener, this, "SetCookie", " cookie#"+ValidationHelper.HashString(cookie));
            if (added!=1) {
                // cookie already existed and couldn't be replaced
                throw new ArgumentException(SR.GetString(SR.net_cookie_exists), "cookie");
            }
        }

        public long ContentLength64 {
            get {
                return m_ContentLength;
            }
            set {
                CheckDisposed();
                if (m_ResponseState>=ResponseState.SentHeaders) {
                    throw new InvalidOperationException(SR.GetString(SR.net_rspsubmitted));
                }
                if (value>=0) {
                    m_ContentLength = value;
                    m_BoundaryType = BoundaryType.ContentLength;
                }
                else {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_clsmall));
                }
            }
        }

        public Version ProtocolVersion {
            get {
                return new Version(m_NativeResponse.Version.MajorVersion, m_NativeResponse.Version.MinorVersion);
            }
            set {
                CheckDisposed();
                if (value==null) {
                    throw new ArgumentNullException("value");
                }
                if (value.Major!=1 || (value.Minor!=0 && value.Minor!=1)) {
                    throw new ArgumentException(SR.GetString(SR.net_wrongversion), "value");
                }
                m_NativeResponse.Version.MajorVersion = (ushort)value.Major;
                m_NativeResponse.Version.MinorVersion = (ushort)value.Minor;
            }
        }

        public void Abort() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "abort", "");
            try {
                if (m_ResponseState>=ResponseState.Closed) {
                    return;
                }

                m_ResponseState = ResponseState.Closed;
                HttpListenerContext.Abort();
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "abort", "");
            }
        }

        public void Close(byte[] responseEntity, bool willBlock) {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Close", " responseEntity="+ValidationHelper.HashString(responseEntity)+" willBlock="+willBlock);
            try {
                CheckDisposed();
                if (responseEntity==null) {
                    throw new ArgumentNullException("responseEntity");
                }
                GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::Close() ResponseState:" + m_ResponseState + " BoundaryType:" + m_BoundaryType + " ContentLength:" + m_ContentLength);
                if (m_ResponseState<ResponseState.SentHeaders && m_BoundaryType!=BoundaryType.Chunked) {
                    ContentLength64 = responseEntity.Length;
                }
                EnsureResponseStream();
                if (willBlock) {
                    try {
                        m_ResponseStream.Write(responseEntity, 0, responseEntity.Length);
                    }
                    catch (Win32Exception) {
                    }
                    finally {
                        m_ResponseStream.Close();
                        m_ResponseState = ResponseState.Closed;
                        HttpListenerContext.Close();
                    }
                }
                else {
                    // <


                    m_ResponseStream.BeginWrite(responseEntity, 0, responseEntity.Length, new AsyncCallback(NonBlockingCloseCallback), null);
                }
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Close", "");
            }
        }

        public /* override */ void Close() {
            if(Logging.On)Logging.Enter(Logging.HttpListener, this, "Close", "");
            try {
                GlobalLog.Print("HttpListenerResponse::Close()");
                ((IDisposable)this).Dispose();
            } finally {
                if(Logging.On)Logging.Exit(Logging.HttpListener, this, "Close", "");
            }
        }

        private void Dispose(bool disposing) {
            if (m_ResponseState>=ResponseState.Closed) {
                return;
            }
            EnsureResponseStream();
            m_ResponseStream.Close();
            m_ResponseState = ResponseState.Closed;
            
            HttpListenerContext.Close();
        }

        void IDisposable.Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // old API, now private, and helper methods

        internal BoundaryType BoundaryType {
            get {
                return m_BoundaryType;
            }
        }

        internal bool SentHeaders {
            get {
                return m_ResponseState>=ResponseState.SentHeaders;
            }
        }

        internal bool ComputedHeaders {
            get {
                return m_ResponseState>=ResponseState.ComputedHeaders;
            }
        }

        private void EnsureResponseStream() {
            if (m_ResponseStream==null) {
                m_ResponseStream = new HttpResponseStream(HttpListenerContext);
            }
        }

        private void NonBlockingCloseCallback(IAsyncResult asyncResult) {
            try {
                m_ResponseStream.EndWrite(asyncResult);
            }
            catch (Win32Exception) {
            }
            finally {
                m_ResponseStream.Close();
                HttpListenerContext.Close();
                m_ResponseState = ResponseState.Closed;
            }
        }

/*
12.3
HttpSendHttpResponse() and HttpSendResponseEntityBody() Flag Values.
The following flags can be used on calls to HttpSendHttpResponse() and HttpSendResponseEntityBody() API calls:

#define HTTP_SEND_RESPONSE_FLAG_DISCONNECT          0x00000001
#define HTTP_SEND_RESPONSE_FLAG_MORE_DATA           0x00000002
#define HTTP_SEND_RESPONSE_FLAG_RAW_HEADER          0x00000004
#define HTTP_SEND_RESPONSE_FLAG_VALID               0x00000007

HTTP_SEND_RESPONSE_FLAG_DISCONNECT:
    specifies that the network connection should be disconnected immediately after
    sending the response, overriding the HTTP protocol's persistent connection features.
HTTP_SEND_RESPONSE_FLAG_MORE_DATA:
    specifies that additional entity body data will be sent by the caller. Thus,
    the last call HttpSendResponseEntityBody for a RequestId, will have this flag reset.
HTTP_SEND_RESPONSE_RAW_HEADER:
    specifies that a caller of HttpSendResponseEntityBody() is intentionally omitting
    a call to HttpSendHttpResponse() in order to bypass normal header processing. The
    actual HTTP header will be generated by the application and sent as entity body.
    This flag should be passed on the first call to HttpSendResponseEntityBody, and
    not after. Thus, flag is not applicable to HttpSendHttpResponse.
*/
        internal unsafe uint SendHeaders(UnsafeNclNativeMethods.HttpApi.HTTP_DATA_CHUNK* pDataChunk, 
            HttpResponseStreamAsyncResult asyncResult, 
            UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS flags,
            bool isWebSocketHandshake) {
            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SendHeaders() pDataChunk:" + ValidationHelper.ToString((IntPtr)pDataChunk) + " asyncResult:" + ValidationHelper.ToString(asyncResult));
            GlobalLog.Assert(!SentHeaders, "HttpListenerResponse#{0}::SendHeaders()|SentHeaders is true.", ValidationHelper.HashString(this));
            
            if (StatusCode == (int)HttpStatusCode.Unauthorized) { // User set 401
                // Using the configured Auth schemes, populate the auth challenge headers. This is for scenarios where 
                // Anonymous access is allowed for some resources, but the server later determines that authorization 
                // is required for this request.
                HttpListenerContext.SetAuthenticationHeaders();
            }
            
            // Log headers
            if(Logging.On) {
                StringBuilder sb = new StringBuilder("HttpListenerResponse Headers:\n");
                for (int i=0; i<Headers.Count; i++) {
                    sb.Append("\t");
                    sb.Append(Headers.GetKey(i));
                    sb.Append(" : ");
                    sb.Append(Headers.Get(i));
                    sb.Append("\n");
                }
                Logging.PrintInfo(Logging.HttpListener, this, ".ctor", sb.ToString());
            }
            m_ResponseState = ResponseState.SentHeaders;
            /*
            if (m_BoundaryType==BoundaryType.Raw) {
                use HTTP_SEND_RESPONSE_FLAG_RAW_HEADER;
            }
            */
            uint statusCode;
            uint bytesSent;
            List<GCHandle> pinnedHeaders = SerializeHeaders(ref m_NativeResponse.Headers, isWebSocketHandshake);
            try {
                if (pDataChunk!=null) {
                    m_NativeResponse.EntityChunkCount = 1;
                    m_NativeResponse.pEntityChunks = pDataChunk;
                }
                else if (asyncResult!=null && asyncResult.pDataChunks!=null) {
                    m_NativeResponse.EntityChunkCount = asyncResult.dataChunkCount;
                    m_NativeResponse.pEntityChunks = asyncResult.pDataChunks;
                } 
                else {
                    m_NativeResponse.EntityChunkCount = 0;
                    m_NativeResponse.pEntityChunks = null;
                }
                GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SendHeaders() calling UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse flags:" + flags);
                if (StatusDescription.Length>0) {
                    byte[] statusDescriptionBytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(StatusDescription)];
                    fixed (byte* pStatusDescription = statusDescriptionBytes) {
                        m_NativeResponse.ReasonLength = (ushort)statusDescriptionBytes.Length;
                        WebHeaderCollection.HeaderEncoding.GetBytes(StatusDescription, 0, statusDescriptionBytes.Length, statusDescriptionBytes, 0);
                        m_NativeResponse.pReason = (sbyte*)pStatusDescription;
                        fixed (UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE* pResponse = &m_NativeResponse) {
                            if (asyncResult != null)
                            {
                                HttpListenerContext.EnsureBoundHandle();
                            }
                            statusCode =
                                UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse(
                                    HttpListenerContext.RequestQueueHandle,
                                    HttpListenerRequest.RequestId,
                                    (uint)flags,
                                    pResponse,
                                    null,
                                    &bytesSent,
                                    SafeLocalFree.Zero,
                                    0,
                                    asyncResult==null ? null : asyncResult.m_pOverlapped,
                                    null );

                            if (asyncResult != null && 
                                statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                                HttpListener.SkipIOCPCallbackOnSuccess)
                            {
                                asyncResult.IOCompleted(statusCode, bytesSent);
                                // IO operation completed synchronously - callback won't be called to signal completion.
                            }
                        }
                    }
                }
                else {
                    fixed (UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE* pResponse = &m_NativeResponse) {
                        if (asyncResult != null)
                        {
                            HttpListenerContext.EnsureBoundHandle();
                        }
                        statusCode =
                            UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse(
                                HttpListenerContext.RequestQueueHandle,
                                HttpListenerRequest.RequestId,
                                (uint)flags,
                                pResponse,
                                null,
                                &bytesSent,
                                SafeLocalFree.Zero,
                                0,
                                asyncResult==null ? null : asyncResult.m_pOverlapped,
                                null );

                        if (asyncResult != null && 
                            statusCode == UnsafeNclNativeMethods.ErrorCodes.ERROR_SUCCESS &&
                            HttpListener.SkipIOCPCallbackOnSuccess)
                        {
                            asyncResult.IOCompleted(statusCode, bytesSent);
                            // IO operation completed synchronously - callback won't be called to signal completion.
                        }
                    }
                }
                GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SendHeaders() call to UnsafeNclNativeMethods.HttpApi.HttpSendHttpResponse returned:" + statusCode);
            }
            finally {
                FreePinnedHeaders(pinnedHeaders);
            }
            return statusCode;
        }

        internal void ComputeCookies() {
            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeCookies() entering Set-Cookie: " + ValidationHelper.ToString(Headers[HttpResponseHeader.SetCookie]) +" Set-Cookie2: " + ValidationHelper.ToString(Headers[HttpKnownHeaderNames.SetCookie2]));
            if (m_Cookies!=null) {
                // now go through the collection, and concatenate all the cookies in per-variant strings
                string setCookie2 = null;
                string setCookie = null;
                for (int index=0; index<m_Cookies.Count; index++) {
                    Cookie cookie = m_Cookies[index];
                    string cookieString = cookie.ToServerString();
                    if (cookieString==null || cookieString.Length==0) {
                        continue;
                    }
                    GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeCookies() now looking at index:" + index + " cookie.Variant:" + cookie.Variant + " cookie:" + cookie.ToString());
                    if (cookie.Variant==CookieVariant.Rfc2965 || (HttpListenerContext.PromoteCookiesToRfc2965 && cookie.Variant==CookieVariant.Rfc2109)) {
                        setCookie2 = setCookie2==null ? cookieString : setCookie2 + ", " + cookieString;
                    }
                    else {
                        setCookie = setCookie==null ? cookieString : setCookie + ", " + cookieString;
                    }
                }
                if (!string.IsNullOrEmpty(setCookie))
                {
                    Headers.Set(HttpResponseHeader.SetCookie, setCookie);
                    if (string.IsNullOrEmpty(setCookie2))
                    {
                        Headers.Remove(HttpKnownHeaderNames.SetCookie2);
                    }
                }
                if (!string.IsNullOrEmpty(setCookie2))
                {
                    Headers.Set(HttpKnownHeaderNames.SetCookie2, setCookie2);
                    if (string.IsNullOrEmpty(setCookie))
                    {
                        Headers.Remove(HttpKnownHeaderNames.SetCookie);
                    }
                }
            }
            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeCookies() exiting Set-Cookie: " + ValidationHelper.ToString(Headers[HttpResponseHeader.SetCookie]) +" Set-Cookie2: " + ValidationHelper.ToString(Headers[HttpKnownHeaderNames.SetCookie2]));
        }

        internal UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS ComputeHeaders() {
            UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeHeaders()");
            GlobalLog.Assert(!ComputedHeaders, "HttpListenerResponse#{0}::ComputeHeaders()|ComputedHeaders is true.", ValidationHelper.HashString(this));
            m_ResponseState = ResponseState.ComputedHeaders;
            /*
            // here we would check for BoundaryType.Raw, in this case we wouldn't need to do anything
            if (m_BoundaryType==BoundaryType.Raw) {
                return flags;
            }
            */

            ComputeCoreHeaders();

            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeHeaders() flags:" + flags + " m_BoundaryType:" + m_BoundaryType + " m_ContentLength:" + m_ContentLength + " m_KeepAlive:" + m_KeepAlive);
            if (m_BoundaryType==BoundaryType.None)
            {
                if (HttpListenerRequest.ProtocolVersion.Minor==0) {
                    // 
                    m_KeepAlive = false;
                }
                else {
                    m_BoundaryType = BoundaryType.Chunked;
                }
                if (CanSendResponseBody(m_HttpContext.Response.StatusCode)) {
                    m_ContentLength = -1;
                }
                else {
                    ContentLength64 = 0;
                }
            }

            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeHeaders() flags:" + flags + " m_BoundaryType:" + m_BoundaryType + " m_ContentLength:" + m_ContentLength + " m_KeepAlive:" + m_KeepAlive);
            if (m_BoundaryType==BoundaryType.ContentLength) {
                Headers.SetInternal(HttpResponseHeader.ContentLength, m_ContentLength.ToString("D", NumberFormatInfo.InvariantInfo));
                if (m_ContentLength==0) {
                    flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE;
                }
            }
            else if (m_BoundaryType==BoundaryType.Chunked) {
                Headers.SetInternal(HttpResponseHeader.TransferEncoding, HttpWebRequest.ChunkedHeader);
            }
            else if (m_BoundaryType==BoundaryType.None) {
                flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE; // seems like HTTP_SEND_RESPONSE_FLAG_MORE_DATA but this hangs the app;
            }
            else {
                m_KeepAlive = false;
            }
            if (!m_KeepAlive) {
                Headers.Add(HttpResponseHeader.Connection, "close");
                if (flags==UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.NONE) {
                    flags = UnsafeNclNativeMethods.HttpApi.HTTP_FLAGS.HTTP_SEND_RESPONSE_FLAG_DISCONNECT;
                }
            }
            else
            {
                if (HttpListenerRequest.ProtocolVersion.Minor == 0)
                {
                    Headers.SetInternal(HttpResponseHeader.KeepAlive, "true");
                }
            }
            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::ComputeHeaders() flags:" + flags + " m_BoundaryType:" + m_BoundaryType + " m_ContentLength:" + m_ContentLength + " m_KeepAlive:" + m_KeepAlive);
            return flags;
        }

        // This method handles the shared response header processing between normal HTTP responses and WebSocket responses.
        internal void ComputeCoreHeaders() {
            if (HttpListenerContext.MutualAuthentication != null && HttpListenerContext.MutualAuthentication.Length > 0)
            {
                Headers.SetInternal(HttpResponseHeader.WwwAuthenticate, HttpListenerContext.MutualAuthentication);
            }
            ComputeCookies();
        }

        private List<GCHandle> SerializeHeaders(ref UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADERS headers,
            bool isWebSocketHandshake) {
            UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[] unknownHeaders = null;
            List<GCHandle> pinnedHeaders;
            GCHandle gcHandle;
            /*
            // here we would check for BoundaryType.Raw, in this case we wouldn't need to do anything
            if (m_BoundaryType==BoundaryType.Raw) {
                return null;
            }
            */
            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SerializeHeaders(HTTP_RESPONSE_HEADERS)");
            if (Headers.Count==0) {
                return null;
            }
            string headerName;
            string headerValue;
            int lookup;
            byte[] bytes = null;
            pinnedHeaders = new List<GCHandle>();

            //---------------------------------------------------
            // DTS Issue: 609383:
            // The Set-Cookie headers are being merged into one. 
            // There are two issues here. 
            // 1. When Set-Cookie headers are set through SetCookie method on the ListenerResponse,
            // there is code in the SetCookie method and the methods it calls to flatten the Set-Cookie
            // values. This blindly concatenates the cookies with a comma delimiter. There could be 
            // a cookie value that contains comma, but we don't escape it with %XX value
            //  
            // As an alternative users can add the Set-Cookie header through the AddHeader method
            // like ListenerResponse.Headers.Add("name", "value")
            // That way they can add multiple headers - AND They can format the value like they want it.
            //
            // 2. Now that the header collection contains multiple Set-Cookie name, value pairs
            // you would think the problem would go away. However here is an interesting thing.
            // For NameValueCollection, when you add 
            // "Set-Cookie", "value1"
            // "Set-Cookie", "value2"
            //  The NameValueCollection.Count == 1. Because there is only one key
            //  NameValueCollection.Get("Set-Cookie") would conviniently take these two valuess
            //  concatenate them with a comma like 
            //  value1,value2. 
            //  In order to get individual values, you need to use 
            //  string[] values = NameValueCollection.GetValues("Set-Cookie");
            //
            //  -------------------------------------------------------------
            //  So here is the proposed fix here.
            //  We must first to loop through all the NameValueCollection keys
            //  and if the name is a unknown header, we must compute the number of 
            //  values it has. Then, we should allocate that many unknown header array 
            //  elements.
            //  
            //  Note that a part of the fix here is to treat Set-Cookie as an unknown header
            //
            //
            //-----------------------------------------------------------
            int numUnknownHeaders = 0;
            for (int index=0; index<Headers.Count; index++) {            
                headerName = Headers.GetKey(index) as string;
                
                //See if this is an unknown header
                lookup = UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.IndexOfKnownHeader(headerName);
                
                //Treat Set-Cookie as well as Connection header in Websocket mode as unknown
                if (lookup == (int)HttpResponseHeader.SetCookie ||
                    isWebSocketHandshake && lookup == (int)HttpResponseHeader.Connection)
                {
                    lookup = -1;
                }

                if(lookup == -1)
                {
                    string[] headerValues = Headers.GetValues(index);
                    numUnknownHeaders += headerValues.Length;
                }
            }

            try{
                fixed (UnsafeNclNativeMethods.HttpApi.HTTP_KNOWN_HEADER* pKnownHeaders = &headers.KnownHeaders) {
                    for (int index=0; index<Headers.Count; index++) {
                        headerName = Headers.GetKey(index) as string;
                        headerValue = Headers.Get(index) as string;
                        lookup = UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.IndexOfKnownHeader(headerName);
                        if (lookup == (int)HttpResponseHeader.SetCookie ||
                            isWebSocketHandshake && lookup == (int)HttpResponseHeader.Connection)
                        {
                            lookup = -1;
                        }
                        GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SerializeHeaders(" + index + "/" + Headers.Count + ") headerName:" + ValidationHelper.ToString(headerName) + " lookup:" + lookup + " headerValue:" + ValidationHelper.ToString(headerValue));
                        if (lookup==-1) {

                            if (unknownHeaders==null) {
                                //----------------------------------------
                                //*** This following comment is no longer true ***
                                // we waste some memory here (up to 32*41=1312 bytes) but we gain speed
                                //unknownHeaders = new UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[Headers.Count-index];
                                //--------------------------------------------
                                unknownHeaders = new UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER[numUnknownHeaders];                                    
                                gcHandle = GCHandle.Alloc(unknownHeaders, GCHandleType.Pinned);
                                pinnedHeaders.Add(gcHandle);
                                headers.pUnknownHeaders = (UnsafeNclNativeMethods.HttpApi.HTTP_UNKNOWN_HEADER*)gcHandle.AddrOfPinnedObject();
                            }

                            //----------------------------------------
                            //FOR UNKNOWN HEADERS
                            //ALLOW MULTIPLE HEADERS to be added 
                            //---------------------------------------
                            string[] headerValues = Headers.GetValues(index);
                            for(int headerValueIndex = 0; headerValueIndex < headerValues.Length; headerValueIndex++)
                            {
                                //Add Name
                                bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(headerName)];
                                unknownHeaders[headers.UnknownHeaderCount].NameLength = (ushort)bytes.Length;
                                WebHeaderCollection.HeaderEncoding.GetBytes(headerName, 0, bytes.Length, bytes, 0);
                                gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                pinnedHeaders.Add(gcHandle);
                                unknownHeaders[headers.UnknownHeaderCount].pName = (sbyte*)gcHandle.AddrOfPinnedObject();

                                //Add Value
                                headerValue = headerValues[headerValueIndex];
                                bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(headerValue)];
                                unknownHeaders[headers.UnknownHeaderCount].RawValueLength = (ushort)bytes.Length;
                                WebHeaderCollection.HeaderEncoding.GetBytes(headerValue, 0, bytes.Length, bytes, 0);
                                gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                pinnedHeaders.Add(gcHandle);
                                unknownHeaders[headers.UnknownHeaderCount].pRawValue = (sbyte*)gcHandle.AddrOfPinnedObject();
                                headers.UnknownHeaderCount++;
                                GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SerializeHeaders(Unknown) UnknownHeaderCount:" + headers.UnknownHeaderCount);
                            }
                            
                        }
                        else {
                            GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SerializeHeaders(Known) HttpResponseHeader[" + lookup + "]:" + ((HttpResponseHeader)lookup) + " headerValue:" + ValidationHelper.ToString(headerValue));
                            if (headerValue!=null) {
                                bytes = new byte[WebHeaderCollection.HeaderEncoding.GetByteCount(headerValue)];
                                pKnownHeaders[lookup].RawValueLength = (ushort)bytes.Length;
                                WebHeaderCollection.HeaderEncoding.GetBytes(headerValue, 0, bytes.Length, bytes, 0);
                                gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                                pinnedHeaders.Add(gcHandle);
                                pKnownHeaders[lookup].pRawValue = (sbyte*)gcHandle.AddrOfPinnedObject();
                                GlobalLog.Print("HttpListenerResponse#" + ValidationHelper.HashString(this) + "::SerializeHeaders(Known) pRawValue:" + ValidationHelper.ToString((IntPtr)(pKnownHeaders[lookup].pRawValue)) + " RawValueLength:" + pKnownHeaders[lookup].RawValueLength + " lookup:" + lookup);
                                GlobalLog.Dump((IntPtr)pKnownHeaders[lookup].pRawValue, 0, pKnownHeaders[lookup].RawValueLength);
                            }
                        }
                    }
                }
            }
            catch {
                FreePinnedHeaders(pinnedHeaders);
                throw;
            }
            return pinnedHeaders;
        }

        private void FreePinnedHeaders(List<GCHandle> pinnedHeaders) {
            if (pinnedHeaders!=null) {
                foreach (GCHandle gcHandle in pinnedHeaders) {
                    if (gcHandle.IsAllocated) {
                        gcHandle.Free();
                    }
                }
            }
        }

        private void CheckDisposed() {
            if (m_ResponseState>=ResponseState.Closed) {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        internal void CancelLastWrite(CriticalHandle requestQueueHandle)
        {
            if (m_ResponseStream != null)
            {
                m_ResponseStream.CancelLastWrite(requestQueueHandle);
            }
        }
    }

}
