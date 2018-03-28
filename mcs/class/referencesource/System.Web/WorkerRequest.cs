//------------------------------------------------------------------------------
// <copyright file="WorkerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*++

   Copyright    (c)    1999    Microsoft Corporation

   Module  Name :

        HttpWorkerRequest.cs

   Abstract:

        This module defines the base worker class used by ASP.NET Managed
        code for request processing.
 
--*/

namespace System.Web {
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Management; // for webevent tracing
    using System.Web.Util;

    //
    // ****************************************************************************
    //


    /// <devdoc>
    ///    <para>This abstract class defines the base worker methods and enumerations used by ASP.NET managed code for request processing.</para>
    /// </devdoc>
    [ComVisible(false)]
    public abstract class HttpWorkerRequest {
        private DateTime _startTime;
        private volatile bool _isInReadEntitySync;

        //it is up to the derived classes to implement a real id
        #pragma warning disable 0649
        private Guid _traceId;
        #pragma warning restore 0649


        protected HttpWorkerRequest()
        {
            _startTime = DateTime.UtcNow;
        }

        // ************************************************************************

        //
        // Indexed Headers. All headers that are defined by HTTP/1.1. These 
        // values are used as offsets into arrays and as token values.
        //  
        // IMPORTANT : Notice request + response values overlap. Make sure you 
        // know which type of header array you are indexing.
        //

        //
        // general-headers [section 4.5]
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderCacheControl          = 0;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderConnection            = 1;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderDate                  = 2;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderKeepAlive             = 3;   // not in rfc

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderPragma                = 4;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderTrailer               = 5;     

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderTransferEncoding      = 6;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderUpgrade               = 7;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderVia                   = 8;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderWarning               = 9;

        //
        // entity-headers  [section 7.1]
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAllow                 = 10;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentLength         = 11;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentType           = 12;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentEncoding       = 13;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentLanguage       = 14;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentLocation       = 15;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentMd5            = 16;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderContentRange          = 17;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderExpires               = 18;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderLastModified          = 19;

        //
        // request-headers [section 5.3]
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAccept                = 20;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAcceptCharset         = 21;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAcceptEncoding        = 22;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAcceptLanguage        = 23;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAuthorization         = 24;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderCookie                = 25;   // not in rfc

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderExpect                = 26;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderFrom                  = 27;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderHost                  = 28;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderIfMatch               = 29;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderIfModifiedSince       = 30;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderIfNoneMatch           = 31;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderIfRange               = 32;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderIfUnmodifiedSince     = 33;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderMaxForwards           = 34;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderProxyAuthorization    = 35;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderReferer               = 36;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderRange                 = 37;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderTe                    = 38;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderUserAgent             = 39;

        //
        // Request headers end here
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int RequestHeaderMaximum        = 40;

        //
        // response-headers [section 6.2]
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAcceptRanges          = 20;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderAge                   = 21;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderEtag                  = 22;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderLocation              = 23;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderProxyAuthenticate     = 24;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderRetryAfter            = 25;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderServer                = 26;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderSetCookie             = 27;   // not in rfc

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderVary                  = 28;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int HeaderWwwAuthenticate       = 29;

        //
        // Response headers end here
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const int ResponseHeaderMaximum       = 30;

        // ************************************************************************

        //
        // Request reasons
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public const int ReasonResponseCacheMiss     = 0;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public const int ReasonFileHandleCacheMiss   = 1;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public const int ReasonCachePolicy           = 2;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public const int ReasonCacheSecurity         = 3;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public const int ReasonClientDisconnect      = 4;


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public const int ReasonDefault               = ReasonResponseCacheMiss;


        // ************************************************************************

        //
        // Access to request related members
        //

        // required members


        /// <devdoc>
        ///    <para> Returns the virtual path to the requested Uri, including PathInfo.</para>
        /// </devdoc>
        public abstract String  GetUriPath();           // "/foo/page.aspx/tail"

        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract String  GetQueryString();       // "param=bar"

        /// <devdoc>
        ///    <para>Gets the URI requsted by the client, which will include PathInfo and QueryString if it exists.
        ///    This value is unaffected by any URL rewriting or routing that may occur on the server.</para>
        /// </devdoc>
        public abstract String  GetRawUrl();            // "/foo/page.aspx/tail?param=bar"

        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract String  GetHttpVerbName();      // "GET" 

        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract String  GetHttpVersion();       // "HTTP/1.1"


        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract String  GetRemoteAddress();     // client's ip address

        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract int     GetRemotePort();        // client's port

        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract String  GetLocalAddress();      // server's ip address

        /// <devdoc>
        ///    <para>Provides Access to the specified member of the request header.</para>
        /// </devdoc>
        public abstract int     GetLocalPort();         // server's port

        internal virtual String GetLocalPortAsString() {
            return GetLocalPort().ToString(NumberFormatInfo.InvariantInfo);
        }

        /*
         * Internal property to determine if request is local
         */

        internal bool IsLocal() {
            String remoteAddress = GetRemoteAddress();

            // if unknown, assume not local
            if (String.IsNullOrEmpty(remoteAddress))
                return false;

            // check if localhost
            if (remoteAddress == "127.0.0.1" || remoteAddress == "::1")
                return true;

            // compare with local address
            if (remoteAddress == GetLocalAddress())
                return true;

            return false;
        }

        // Attempt to derive RawUrl from the "CACHE_URL" server variable.
        internal static String GetRawUrlHelper(String cacheUrl) {
            // cacheUrl has format "[http|https]://[server]:[port][uri]", including query string and path-info, if they exist.
            if (cacheUrl != null) {
                // the URI begins at the 3rd slash
                int count = 0;
                for(int index = 0; index < cacheUrl.Length; index++) {
                    if (cacheUrl[index] == '/') {
                        if (++count == 3) {
                            return cacheUrl.Substring(index);
                        }
                    }
                }
            }
            
            // someone must have modified CACHE_URL, it is not valid
            throw new HttpException(SR.GetString(SR.Cache_url_invalid));
        } 

        // Mark a blocking call
        // It allows RequestTimeoutManager to eventualy to close the connection and unblock the caller
        // and handle request timeout properly (if in cancelable state)
        internal bool IsInReadEntitySync {
            get {
                return _isInReadEntitySync;
            }
            set {
                _isInReadEntitySync = value;
            }
        }

        // optional members with defaults supplied


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the response query string as an array of bytes.</para>
        /// </devdoc>
        public virtual byte[] GetQueryStringRawBytes() {
            // access to raw qs for i18n
            return null;
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the client computer's name.</para>
        /// </devdoc>
        public virtual String GetRemoteName() {
            // client's name
            return GetRemoteAddress();
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the name of the local server.</para>
        /// </devdoc>
        public virtual String GetServerName() {
            // server's name
            return GetLocalAddress();
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the ID of the current connection.</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual long GetConnectionID() {
            // connection id
            return 0;
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the context ID of the current connection.</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual long GetUrlContextID() {
            // UL APPID
            return 0; 
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the application pool ID for the current URL.</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual String GetAppPoolID() {
            // UL Application pool id
            return null; 
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the reason for the request.</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual int GetRequestReason() {
            // constants Reason... above
            return ReasonDefault; 
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the client's impersonation token.</para>
        /// </devdoc>
        public virtual IntPtr GetUserToken() {
            // impersonation token
            return IntPtr.Zero;
        }

        //    Gets LOGON_USER as WindowsIdentity
        internal WindowsIdentity GetLogonUserIdentity() {
            IntPtr token = GetUserToken();

            if (token != IntPtr.Zero) {
                String logonUser = GetServerVariable("LOGON_USER");
                String authType = GetServerVariable("AUTH_TYPE");
                bool isAuthenticated = (!string.IsNullOrEmpty(logonUser) || (!string.IsNullOrEmpty(authType) && !StringUtil.EqualsIgnoreCase(authType, "basic")));
                return CreateWindowsIdentityWithAssert(token, ((authType == null) ? "" : authType), WindowsAccountType.Normal, isAuthenticated);
            }

            return null; // invalid token
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        private static WindowsIdentity CreateWindowsIdentityWithAssert(IntPtr token, string authType, WindowsAccountType accountType, bool isAuthenticated) {
            return new WindowsIdentity(token, authType, accountType, isAuthenticated);
        }


        /// <internalonly/>
        public virtual IntPtr GetVirtualPathToken() {
            // impersonation token
            return IntPtr.Zero;
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns a value indicating whether the connection is secure (using SSL).</para>
        /// </devdoc>
        public virtual bool IsSecure() {
            // is over ssl?
            return false;
        }

        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the HTTP protocol (HTTP or HTTPS).</para>
        /// </devdoc>
        public virtual String GetProtocol() {
            return IsSecure() ?  "https" : "http";
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the virtual path to the requested Uri, without PathInfo.</para>
        /// </devdoc>
        public virtual String GetFilePath() {
            // "/foo/page.aspx"
            return GetUriPath();
        }

        internal VirtualPath GetFilePathObject() {
            // Don't allow malformed paths for security reasons
            return VirtualPath.Create(GetFilePath(), VirtualPathOptions.AllowAbsolutePath |
                VirtualPathOptions.AllowNull);
        }

        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the translated file path to the requested Uri (from virtual path to 
        ///       UNC path, ie "/foo/page.aspx" to "c:\dir\page.aspx") </para>
        /// </devdoc>
        public virtual String GetFilePathTranslated() {
            // "c:\dir\page.aspx"
            return null;
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns additional 
        ///       path information for a resource with a URL extension. i.e. for the URL
        ///       /virdir/page.html/tail, the PathInfo value is /tail. </para>
        /// </devdoc>
        public virtual String GetPathInfo() {
            // "/tail"
            return String.Empty;
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the virtual path to the 
        ///       currently executing server application.</para>
        /// </devdoc>
        public virtual String GetAppPath() {
            // "/foo"
            return null;
        }


        /// <devdoc>
        ///    <para>When overriden in a derived class, returns the UNC-translated path to 
        ///       the currently executing server application.</para>
        /// </devdoc>
        public virtual String GetAppPathTranslated() {
            // "c:\dir"
            return null;
        }

        //
        // Virtual methods to read the incoming request
        //

        public virtual int GetPreloadedEntityBodyLength() {
            byte[] bytes = GetPreloadedEntityBody();
            return (bytes != null) ? bytes.Length : 0;
        }

        public virtual int GetPreloadedEntityBody(byte[] buffer, int offset) {
            int l = 0;
            byte[] bytes = GetPreloadedEntityBody();

            if (bytes != null) {
                l = bytes.Length;
                Buffer.BlockCopy(bytes, 0, buffer, offset, l);
            }

            return l;
        }

        public virtual byte[] GetPreloadedEntityBody() {
            return null;
        }

        public virtual bool IsEntireEntityBodyIsPreloaded() {
            return false;
        }

        public virtual int GetTotalEntityBodyLength() {
            int l = 0;

            String contentLength = GetKnownRequestHeader(HeaderContentLength);

            if (contentLength != null) {
                try {
                    l = Int32.Parse(contentLength, CultureInfo.InvariantCulture);
                }
                catch {
                }
            }

            return l;
        }

        public virtual int ReadEntityBody(byte[] buffer, int size) {
            return 0;
        }

        public virtual int ReadEntityBody(byte[] buffer, int offset, int size) {
            byte[] temp = new byte[size];
            int l = ReadEntityBody(temp, size);

            if (l > 0) {
                Buffer.BlockCopy(temp, 0, buffer, offset, l);
            }

            return l;
        }

        // Returns true if async flush is supported; otherwise false.
        public virtual bool SupportsAsyncFlush { get { return false; } }

        // Sends the currently buffered response to the client asynchronously.  To support this, 
        // the worker request buffers the status, headers, and resonse body until an asynchronous 
        // flush operation is initiated.
        public virtual IAsyncResult BeginFlush(AsyncCallback callback, Object state) {
            throw new NotSupportedException();
        }

        // Finish an asynchronous flush.
        public virtual void EndFlush(IAsyncResult asyncResult) {
            throw new NotSupportedException();
        }

        public virtual bool SupportsAsyncRead { get { return false; } }

        // Begin an asynchronous read of the request entity body.  To read the entire entity, invoke
        // repeatedly until total bytes read is equal to Request.ContentLength or EndRead indicates
        // that zero bytes were read.  If Request.ContentLength is zero and the request is chunked,
        // then invoke repeatedly until EndRead indicates that zero bytes were read.
        //
        // If an error occurs and the client is no longer connected, no exception will be thrown for
        // compatibility with the synchronous read method (ReadEntityBody).  Instead, EndRead will
        // report that zero bytes were read.
        //
        // This implements Stream.BeginRead, and as such, should throw
        // exceptions as described on MSDN when errors occur.
        public virtual IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            throw new NotSupportedException();
        }

        // Finish an asynchronous read.  When this returns zero there is no more to be read.  If Request.ContentLength is non-zero,
        // do not read more bytes then specified by ContentLength, or an error will occur.
        // This implements Stream.EndRead on HttpBufferlessInputStream, and as such, should throw
        // exceptions as described on MSDN when errors occur.
        public virtual int EndRead(IAsyncResult asyncResult) {
            throw new NotSupportedException();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String GetKnownRequestHeader(int index) {
            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String GetUnknownRequestHeader(String name) {
            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [CLSCompliant(false)]
        public virtual String[][] GetUnknownRequestHeaders() {
            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String GetServerVariable(String name) {
            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual long GetBytesRead() {
            return 0;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        internal virtual DateTime GetStartTime() {
            return _startTime;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        internal virtual void ResetStartTime() {
            _startTime = DateTime.UtcNow;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String MapPath(String virtualPath) {
            return null;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String MachineConfigPath {
            get {
                return null;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String RootWebConfigPath {
            get {
                return null;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String MachineInstallDirectory {
            get {
                return null;
            }
        }

        // IntegratedTraceType in EtwTrace.cs
        internal virtual void RaiseTraceEvent(IntegratedTraceType traceType, string eventData) {
            // do nothing
        }

        internal virtual void RaiseTraceEvent(WebBaseEvent webEvent) {
            // do nothing
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual Guid RequestTraceIdentifier {
            get {
                return _traceId;
            }
        }

        //
        // Abstract methods to write the response
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void SendStatus(int statusCode, String statusDescription);

        // for IIS 7, use both the status and substatus
        // this cannot be abstract 
        internal virtual void SendStatus(int statusCode, int subStatusCode, String statusDescription) {
            SendStatus(statusCode, statusDescription);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void SendKnownResponseHeader(int index, String value);


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void SendUnknownResponseHeader(String name, String value);

        // headers encoding controled via HttpResponse.HeaderEncoding
        internal virtual void SetHeaderEncoding(Encoding encoding) {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void SendResponseFromMemory(byte[] data, int length);


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void SendResponseFromMemory(IntPtr data, int length) {
            if (length > 0) {
                InternalSecurityPermissions.UnmanagedCode.Demand();
                // derived classes could have an efficient implementation
                byte[] bytes = new byte[length];
                Misc.CopyMemory(data, 0, bytes, 0, length);
                SendResponseFromMemory(bytes, length);
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        internal virtual void SendResponseFromMemory(IntPtr data, int length, bool isBufferFromUnmanagedPool) {
            // default implementation
            SendResponseFromMemory(data, length);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void SendResponseFromFile(String filename, long offset, long length);


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void SendResponseFromFile(IntPtr handle, long offset, long length);

        internal virtual void TransmitFile(String filename, long length, bool isImpersonating) {
            TransmitFile(filename, 0, length, isImpersonating);
        }

        internal virtual void TransmitFile(String filename, long offset, long length, bool isImpersonating) {
            // default implementation
            SendResponseFromFile(filename, offset, length);
        }

        // VSWhidbey 555203: support 64-bit file sizes for TransmitFile on IIS6
        internal virtual bool SupportsLongTransmitFile {
            get { return false; }
        }

        // WOS 1555777: kernel cache support
        // If the worker request can kernel cache the response, it returns the
        // kernel cache key; otherwise null.  The kernel cache key is used to invalidate
        // the entry if a dependency changes or the item is flushed from the managed
        // cache for any reason.
        internal virtual string SetupKernelCaching(int secondsToLive, string originalCacheUrl, bool enableKernelCacheForVaryByStar) {
            return null;
        }

        // WOS 1555777: kernel cache support
        internal virtual void DisableKernelCache() {
            return;
        }

        // DevDiv 255268: IIS user-mode cache support
        internal virtual void DisableUserCache() {
            return;
        }

        internal virtual bool TrySkipIisCustomErrors {
            get { return false; }
            set { }
        }

        // Execute Url

        internal virtual bool SupportsExecuteUrl {
            get { return false; }
        }

        internal virtual IAsyncResult BeginExecuteUrl(
                                            String url, String method, String headers,
                                            bool sendHeaders,
                                            bool addUserIndo, IntPtr token, String name, String authType,
                                            byte[] entity,
                                            AsyncCallback cb, Object state) {
            throw new NotSupportedException(SR.GetString(SR.ExecuteUrl_not_supported));
        }

        internal virtual void EndExecuteUrl(IAsyncResult result) {
        }

        internal virtual void UpdateInitialCounters() {
        }

        internal virtual void UpdateResponseCounters(bool finalFlush, int bytesOut) {
        }

        internal virtual void UpdateRequestCounters(int bytesIn) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void FlushResponse(bool finalFlush);


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void EndOfRequest();

        //
        // Virtual helper methods
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Design","CA1034:NestedTypesShouldNotBeVisible", Scope = "type", Target = "System.Web.HttpWorkerRequest+EndOfSendNotification",
            Justification = "Already shipped. Cannot move as would be a breaking change.")]
        public delegate void EndOfSendNotification(HttpWorkerRequest wr, Object extraData);


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void SetEndOfSendNotification(EndOfSendNotification callback, Object extraData) {
            // firing the callback helps with buffer recycling
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void SendCalculatedContentLength(int contentLength) {
            // oportunity to add Content-Length header if not added by user
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void SendCalculatedContentLength(long contentLength) {
            // default implementation is to call the int32 version
            SendCalculatedContentLength(Convert.ToInt32(contentLength));
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual bool HeadersSent() {
            return true;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual bool IsClientConnected() {
            return true;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void CloseConnection() {
        }


        /// <devdoc>
        ///    <para>Defines the base worker class used by ASP.NET Managed code for request 
        ///       processing.</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual byte [] GetClientCertificate() {
            return new byte[0];
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual DateTime GetClientCertificateValidFrom() {
            return DateTime.Now;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual DateTime GetClientCertificateValidUntil() {
            return DateTime.Now;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual byte [] GetClientCertificateBinaryIssuer() {
            return new byte[0];
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual int GetClientCertificateEncoding() {
            return 0;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        /// <internalonly/>
        public virtual byte[] GetClientCertificatePublicKey() {
            return new byte[0];
        }

        // ************************************************************************

        //
        // criteria to find out if there is posted data
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool HasEntityBody() {
            //
            // content length != 0 -> assume has content
            //

            String contentLength = GetKnownRequestHeader(HeaderContentLength);
            if (contentLength != null && !contentLength.Equals("0"))
                return true;

            //
            // any content encoding -> assume has content
            //

            if (GetKnownRequestHeader(HeaderTransferEncoding) != null)
                return true;

            //
            // preloaded -> has it
            //

            if (GetPreloadedEntityBody() != null)
                return true;

            //
            // no posted data but everything preloaded -> no content
            //

            if (IsEntireEntityBodyIsPreloaded())
                return false;

            return false;
        }

        // ************************************************************************

        //
        // Default values for Http status description strings
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String GetStatusDescription(int code) {
            if (code >= 100 && code < 600) {
                int i = code / 100;
                int j = code % 100;

                if (j < s_HTTPStatusDescriptions[i].Length)
                    return s_HTTPStatusDescriptions[i][j];
            }

            return String.Empty;
        }

        // Tables of status strings (first index is code/100, 2nd code%100)

        private static readonly String[][] s_HTTPStatusDescriptions = new String[][]
        {
            null,

            new String[]
            { 
                /* 100 */"Continue",
                /* 101 */ "Switching Protocols",
                /* 102 */ "Processing"
            },

            new String[]
            { 
                /* 200 */"OK",
                /* 201 */ "Created",
                /* 202 */ "Accepted",
                /* 203 */ "Non-Authoritative Information",
                /* 204 */ "No Content",
                /* 205 */ "Reset Content",
                /* 206 */ "Partial Content",
                /* 207 */ "Multi-Status"
            },

            new String[]
            { 
                /* 300 */"Multiple Choices",
                /* 301 */ "Moved Permanently",
                /* 302 */ "Found",
                /* 303 */ "See Other",
                /* 304 */ "Not Modified",
                /* 305 */ "Use Proxy",
                /* 306 */ String.Empty,
                /* 307 */ "Temporary Redirect"
            },

            new String[]
            { 
                /* 400 */"Bad Request",
                /* 401 */ "Unauthorized",
                /* 402 */ "Payment Required",
                /* 403 */ "Forbidden",
                /* 404 */ "Not Found",
                /* 405 */ "Method Not Allowed",
                /* 406 */ "Not Acceptable",
                /* 407 */ "Proxy Authentication Required",
                /* 408 */ "Request Timeout",
                /* 409 */ "Conflict",
                /* 410 */ "Gone",
                /* 411 */ "Length Required",
                /* 412 */ "Precondition Failed",
                /* 413 */ "Request Entity Too Large",
                /* 414 */ "Request-Uri Too Long",
                /* 415 */ "Unsupported Media Type",
                /* 416 */ "Requested Range Not Satisfiable",
                /* 417 */ "Expectation Failed",
                /* 418 */ String.Empty,
                /* 419 */ String.Empty,
                /* 420 */ String.Empty,
                /* 421 */ String.Empty,
                /* 422 */ "Unprocessable Entity",
                /* 423 */ "Locked",
                /* 424 */ "Failed Dependency"
            },

            new String[]
            { 
                /* 500 */"Internal Server Error",
                /* 501 */ "Not Implemented",
                /* 502 */ "Bad Gateway",
                /* 503 */ "Service Unavailable",
                /* 504 */ "Gateway Timeout",
                /* 505 */ "Http Version Not Supported",
                /* 506 */ String.Empty,
                /* 507 */ "Insufficient Storage"
            }
        };

        // ************************************************************************

        //
        // Header index to string conversions
        //


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static int GetKnownRequestHeaderIndex(String header) {
            Object intObj = s_requestHeadersLoookupTable[header];

            if (intObj != null)
                return(Int32)intObj;
            else
                return -1;
        }

        // ************************************************************************


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String GetKnownRequestHeaderName(int index) {
            return s_requestHeaderNames[index];
        }

        internal static String GetServerVariableNameFromKnownRequestHeaderIndex(int index) {
            return s_serverVarFromRequestHeaderNames[index];
        }

        // ************************************************************************


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static int GetKnownResponseHeaderIndex(String header) {
            Object intObj = s_responseHeadersLoookupTable[header];

            if (intObj != null)
                return(Int32)intObj;
            else
                return -1;
        }

        // ************************************************************************


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static String GetKnownResponseHeaderName(int index) {
            return s_responseHeaderNames[index];
        }

        // ************************************************************************


        //
        // Implemenation -- lookup tables for header names
        //

        static private String[] s_serverVarFromRequestHeaderNames = new String[RequestHeaderMaximum];
        static private String[] s_requestHeaderNames  = new String[RequestHeaderMaximum];
        static private String[] s_responseHeaderNames = new String[ResponseHeaderMaximum];
        static private Hashtable s_requestHeadersLoookupTable  = new Hashtable(StringComparer.OrdinalIgnoreCase);
        static private Hashtable s_responseHeadersLoookupTable = new Hashtable(StringComparer.OrdinalIgnoreCase);

        // ************************************************************************

        static private void DefineHeader(bool isRequest, 
                                         bool isResponse, 
                                         int index, 
                                         String headerName,
                                         String serverVarName) {

            Debug.Assert(serverVarName == null || serverVarName == "HTTP_" + headerName.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_'));

            Int32  i32 = new Int32();
            if (isRequest) {
                i32 = index;
                s_serverVarFromRequestHeaderNames[index] = serverVarName;
                s_requestHeaderNames[index] = headerName;
                s_requestHeadersLoookupTable.Add(headerName, i32);
            }

            if (isResponse) {
                i32 = index;
                s_responseHeaderNames[index] = headerName;
                s_responseHeadersLoookupTable.Add(headerName, i32);
            }
        }

        // ************************************************************************

        static HttpWorkerRequest() {
            //
            // common headers
            //

            DefineHeader(true,  true,  HeaderCacheControl,        "Cache-Control",         "HTTP_CACHE_CONTROL");
            DefineHeader(true,  true,  HeaderConnection,          "Connection",            "HTTP_CONNECTION");
            DefineHeader(true,  true,  HeaderDate,                "Date",                  "HTTP_DATE");
            DefineHeader(true,  true,  HeaderKeepAlive,           "Keep-Alive",            "HTTP_KEEP_ALIVE");
            DefineHeader(true,  true,  HeaderPragma,              "Pragma",                "HTTP_PRAGMA");
            DefineHeader(true,  true,  HeaderTrailer,             "Trailer",               "HTTP_TRAILER");
            DefineHeader(true,  true,  HeaderTransferEncoding,    "Transfer-Encoding",     "HTTP_TRANSFER_ENCODING");
            DefineHeader(true,  true,  HeaderUpgrade,             "Upgrade",               "HTTP_UPGRADE");
            DefineHeader(true,  true,  HeaderVia,                 "Via",                   "HTTP_VIA");
            DefineHeader(true,  true,  HeaderWarning,             "Warning",               "HTTP_WARNING");
            DefineHeader(true,  true,  HeaderAllow,               "Allow",                 "HTTP_ALLOW");
            DefineHeader(true,  true,  HeaderContentLength,       "Content-Length",        "HTTP_CONTENT_LENGTH");
            DefineHeader(true,  true,  HeaderContentType,         "Content-Type",          "HTTP_CONTENT_TYPE");
            DefineHeader(true,  true,  HeaderContentEncoding,     "Content-Encoding",      "HTTP_CONTENT_ENCODING");
            DefineHeader(true,  true,  HeaderContentLanguage,     "Content-Language",      "HTTP_CONTENT_LANGUAGE");
            DefineHeader(true,  true,  HeaderContentLocation,     "Content-Location",      "HTTP_CONTENT_LOCATION");
            DefineHeader(true,  true,  HeaderContentMd5,          "Content-MD5",           "HTTP_CONTENT_MD5");
            DefineHeader(true,  true,  HeaderContentRange,        "Content-Range",         "HTTP_CONTENT_RANGE");
            DefineHeader(true,  true,  HeaderExpires,             "Expires",               "HTTP_EXPIRES");
            DefineHeader(true,  true,  HeaderLastModified,        "Last-Modified",         "HTTP_LAST_MODIFIED");

            //
            // request only headers
            //

            DefineHeader(true,  false, HeaderAccept,              "Accept",                "HTTP_ACCEPT");
            DefineHeader(true,  false, HeaderAcceptCharset,       "Accept-Charset",        "HTTP_ACCEPT_CHARSET");
            DefineHeader(true,  false, HeaderAcceptEncoding,      "Accept-Encoding",       "HTTP_ACCEPT_ENCODING");
            DefineHeader(true,  false, HeaderAcceptLanguage,      "Accept-Language",       "HTTP_ACCEPT_LANGUAGE");
            DefineHeader(true,  false, HeaderAuthorization,       "Authorization",         "HTTP_AUTHORIZATION");
            DefineHeader(true,  false, HeaderCookie,              "Cookie",                "HTTP_COOKIE");
            DefineHeader(true,  false, HeaderExpect,              "Expect",                "HTTP_EXPECT");
            DefineHeader(true,  false, HeaderFrom,                "From",                  "HTTP_FROM");
            DefineHeader(true,  false, HeaderHost,                "Host",                  "HTTP_HOST");
            DefineHeader(true,  false, HeaderIfMatch,             "If-Match",              "HTTP_IF_MATCH");
            DefineHeader(true,  false, HeaderIfModifiedSince,     "If-Modified-Since",     "HTTP_IF_MODIFIED_SINCE");
            DefineHeader(true,  false, HeaderIfNoneMatch,         "If-None-Match",         "HTTP_IF_NONE_MATCH");
            DefineHeader(true,  false, HeaderIfRange,             "If-Range",              "HTTP_IF_RANGE");
            DefineHeader(true,  false, HeaderIfUnmodifiedSince,   "If-Unmodified-Since",   "HTTP_IF_UNMODIFIED_SINCE");
            DefineHeader(true,  false, HeaderMaxForwards,         "Max-Forwards",          "HTTP_MAX_FORWARDS");
            DefineHeader(true,  false, HeaderProxyAuthorization,  "Proxy-Authorization",   "HTTP_PROXY_AUTHORIZATION");
            DefineHeader(true,  false, HeaderReferer,             "Referer",               "HTTP_REFERER");
            DefineHeader(true,  false, HeaderRange,               "Range",                 "HTTP_RANGE");
            DefineHeader(true,  false, HeaderTe,                  "TE",                    "HTTP_TE");
            DefineHeader(true,  false, HeaderUserAgent,           "User-Agent",            "HTTP_USER_AGENT");

            //
            // response only headers
            //

            DefineHeader(false, true,  HeaderAcceptRanges,        "Accept-Ranges",         null);
            DefineHeader(false, true,  HeaderAge,                 "Age",                   null);
            DefineHeader(false, true,  HeaderEtag,                "ETag",                  null);
            DefineHeader(false, true,  HeaderLocation,            "Location",              null);
            DefineHeader(false, true,  HeaderProxyAuthenticate,   "Proxy-Authenticate",    null);
            DefineHeader(false, true,  HeaderRetryAfter,          "Retry-After",           null);
            DefineHeader(false, true,  HeaderServer,              "Server",                null);
            DefineHeader(false, true,  HeaderSetCookie,           "Set-Cookie",            null);
            DefineHeader(false, true,  HeaderVary,                "Vary",                  null);
            DefineHeader(false, true,  HeaderWwwAuthenticate,     "WWW-Authenticate",      null);
        }
    }
}
