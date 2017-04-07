//------------------------------------------------------------------------------
// <copyright file="StateWorkerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * StateHttpWorkerRequest
 * 
 * Copyright (c) 1998-1999, Microsoft Corporation
 * 
 */

namespace System.Web.SessionState {

    using System.Text;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;   
    using System.Collections;    
    using System.Web;
    using System.Web.Util;
    using System.Globalization;

    class StateHttpWorkerRequest : HttpWorkerRequest {

        /* long enough to hold the string representation of an IPv4 or IPv6 address; keep in sync with tracker.cxx */
        private const int ADDRESS_LENGTH_MAX = 64;

        IntPtr                                  _tracker;
        string                                  _uri;              
        UnsafeNativeMethods.StateProtocolExclusive    _exclusive;        
        int                                     _extraFlags;
        int                                     _timeout;           
        int                                     _lockCookie;        
        bool                                    _lockCookieExists;  
        int                                     _contentLength;     
        byte[]                                  _content;           


        UnsafeNativeMethods.StateProtocolVerb   _methodIndex;
        string                                  _method;         
                                                                 
        string                                  _remoteAddress;  
        int                                     _remotePort;     
        string                                  _localAddress;   
        int                                     _localPort;      
                                                                 
        StringBuilder                           _status;         
        int                                     _statusCode;     
        StringBuilder                           _headers;        
        IntPtr                           _unmanagedState; 
        bool                                    _sent;           

        internal StateHttpWorkerRequest(
                   IntPtr tracker,
                   UnsafeNativeMethods.StateProtocolVerb methodIndex,
                   string uri,
                   UnsafeNativeMethods.StateProtocolExclusive exclusive,
                   int extraFlags,
                   int timeout,
                   int lockCookieExists,
                   int lockCookie,
                   int contentLength,
                   IntPtr content
                   ) {
            _tracker = tracker;
            _methodIndex = methodIndex;
            switch (_methodIndex) {
                case UnsafeNativeMethods.StateProtocolVerb.GET:
                    _method = "GET";
                    break;

                case UnsafeNativeMethods.StateProtocolVerb.PUT:
                    _method = "PUT";
                    break;

                case UnsafeNativeMethods.StateProtocolVerb.HEAD:
                    _method = "HEAD";
                    break;

                case UnsafeNativeMethods.StateProtocolVerb.DELETE:
                    _method = "DELETE";
                    break;

                default:
                    Debug.Assert(false, "Shouldn't get here!");
                    break;
            }

            _uri = uri;
            // Handle the ASP1.1 case which prepends an extra / to the URI
            if (_uri.StartsWith("//", StringComparison.Ordinal)) {
                _uri = _uri.Substring(1);
            }
            _exclusive = exclusive;
            _extraFlags = extraFlags;
            _timeout = timeout;
            _lockCookie = lockCookie;
            _lockCookieExists = lockCookieExists != 0;
            _contentLength = contentLength;
            if (contentLength != 0) {
                Debug.Assert(_contentLength == IntPtr.Size);
                // Need to convert 'content', which is a ptr to native StateItem,
                // into a byte array because that's what GetPreloadedEntityBody
                // must return, and GetPreloadedEntityBody is what the pipeline uses
                // to read the body of the request, which in our case is just a pointer
                // to a native StateItem object.
#if WIN64
                ulong p = (ulong) content; 
                _content = new byte[8] 
                {
                    (byte) ((p & 0x00000000000000ff)),
                    (byte) ((p & 0x000000000000ff00) >> 8),
                    (byte) ((p & 0x0000000000ff0000) >> 16),
                    (byte) ((p & 0x00000000ff000000) >> 24),
                    (byte) ((p & 0x000000ff00000000) >> 32),
                    (byte) ((p & 0x0000ff0000000000) >> 40),
                    (byte) ((p & 0x00ff000000000000) >> 48),
                    (byte) ((p & 0xff00000000000000) >> 56),
                };
#else
                uint p = (uint) content; 
                _content = new byte[4] 
                {
                    (byte) ((p & 0x000000ff)),
                    (byte) ((p & 0x0000ff00) >> 8),
                    (byte) ((p & 0x00ff0000) >> 16),
                    (byte) ((p & 0xff000000) >> 24),
                };
#endif                
            }

            _status  = new StringBuilder(256);
            _headers = new StringBuilder(256);
        }

        public override string GetUriPath() {
            return HttpUtility.UrlDecode(_uri);
        }

        // The file path is used as the path for configuration.
        // This path should always be null, in order to retrieve
        // the machine configuration.
        public override string GetFilePath() {
            return null;
        }

        public override string GetQueryString() {
            return null;
        }

        public override string GetRawUrl() {
            return _uri;
        }

        public override string GetHttpVerbName() {
            return _method;
        }

        public override string GetHttpVersion() {
            return "HTTP/1.0";
        }

        public override string GetRemoteAddress() {
            StringBuilder   buf;

            if (_remoteAddress == null) {
                buf = new StringBuilder(ADDRESS_LENGTH_MAX);
                UnsafeNativeMethods.STWNDGetRemoteAddress(_tracker, buf);
                _remoteAddress = buf.ToString();
            }

            return _remoteAddress;
        }

        public override int GetRemotePort() {
            if (_remotePort == 0) {
                _remotePort = UnsafeNativeMethods.STWNDGetRemotePort(_tracker);
            }

            return _remotePort;
        }

        public override string GetLocalAddress() {
            StringBuilder   buf;

            if (_localAddress == null) {
                buf = new StringBuilder(ADDRESS_LENGTH_MAX);
                UnsafeNativeMethods.STWNDGetLocalAddress(_tracker, buf);
                _localAddress = buf.ToString();
            }

            return _localAddress;
        }

        public override int GetLocalPort() {
            if (_localPort == 0) {
                _localPort = UnsafeNativeMethods.STWNDGetLocalPort(_tracker);
            }

            return _localPort;
        }

        public override byte[] GetPreloadedEntityBody() {
            return _content;
        }


        public override bool IsEntireEntityBodyIsPreloaded() {
            /* Request is always preloaded */
            return true;
        }


        public override string MapPath(string virtualPath) {
            /*
             * Physical and virtual are identical to state server.
             */
            return virtualPath;
        }

        public override int ReadEntityBody(byte[] buffer, int size) {
            /* pretend everything is preloaded */
            return 0;
        }

        public override long GetBytesRead() {
            /* State web doesn't support partial reads */
            throw new NotSupportedException(SR.GetString(SR.Not_supported));
        }

        public override string GetKnownRequestHeader(int index) {
            string s = null;

            switch (index) {
                /* special case important ones */
                case HeaderContentLength:
                    s = (_contentLength).ToString(CultureInfo.InvariantCulture);
                    break;
            }

            return s;
        }

        public override string GetUnknownRequestHeader(string name) {
            string s = null;

            if (name.Equals(StateHeaders.EXCLUSIVE_NAME)) {
                switch (_exclusive) {
                    case UnsafeNativeMethods.StateProtocolExclusive.ACQUIRE:
                        s = StateHeaders.EXCLUSIVE_VALUE_ACQUIRE;
                        break;

                    case UnsafeNativeMethods.StateProtocolExclusive.RELEASE:
                        s = StateHeaders.EXCLUSIVE_VALUE_RELEASE;
                        break;
                }
            }
            else if (name.Equals(StateHeaders.TIMEOUT_NAME)) {
                if (_timeout != -1) {
                    s = (_timeout).ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (name.Equals(StateHeaders.LOCKCOOKIE_NAME)) {
                if (_lockCookieExists) {
                    s = (_lockCookie).ToString(CultureInfo.InvariantCulture);
                }
            }
            else if (name.Equals(StateHeaders.EXTRAFLAGS_NAME)) {
                if (_extraFlags != -1) {
                    s = (_extraFlags).ToString(CultureInfo.InvariantCulture);
                }
            }

            return s;
        }

        public override string[][] GetUnknownRequestHeaders() {
            string [][] ret;
            int         c, i;

            c = 0;
            if (_exclusive != (UnsafeNativeMethods.StateProtocolExclusive) (-1)) {
                c++;
            }

            if (_extraFlags != -1) {
                c++;
            }

            if (_timeout != -1) {
                c++;
            }

            if (_lockCookieExists) {
                c++;
            }

            if (c == 0)
                return null;

            ret = new string[c][];
            i = 0;
            if (_exclusive != (UnsafeNativeMethods.StateProtocolExclusive) (-1)) {
                ret[0] = new string[2];
                ret[0][0] = StateHeaders.EXCLUSIVE_NAME;
                if (_exclusive == UnsafeNativeMethods.StateProtocolExclusive.ACQUIRE) {
                    ret[0][1] = StateHeaders.EXCLUSIVE_VALUE_ACQUIRE;
                }
                else {
                    Debug.Assert(_exclusive == UnsafeNativeMethods.StateProtocolExclusive.RELEASE, "_exclusive == UnsafeNativeMethods.StateProtocolExclusive.RELEASE");
                    ret[0][1] = StateHeaders.EXCLUSIVE_VALUE_RELEASE;
                }

                i++;
            }

            if (_timeout != -1) {
                ret[i] = new string[2];
                ret[i][0] = StateHeaders.TIMEOUT_NAME;
                ret[i][1] = (_timeout).ToString(CultureInfo.InvariantCulture);

                i++;
            }

            if (_lockCookieExists) {
                ret[i] = new string[2];
                ret[i][0] = StateHeaders.LOCKCOOKIE_NAME;
                ret[i][1] = (_lockCookie).ToString(CultureInfo.InvariantCulture);

                i++;
            }

            if (_extraFlags != -1) {
                ret[i] = new string[2];
                ret[i][0] = StateHeaders.EXTRAFLAGS_NAME;
                ret[i][1] = (_extraFlags).ToString(CultureInfo.InvariantCulture);

                i++;
            }

            return ret;
        }

        public override void SendStatus(int statusCode, string statusDescription) {
            Debug.Assert(!_sent);
            _statusCode = statusCode;
            _status.Append((statusCode).ToString(CultureInfo.InvariantCulture) + " " + statusDescription + "\r\n");
        }

        public override void SendKnownResponseHeader(int index, string value) {
            Debug.Assert(!_sent);
            _headers.Append(GetKnownResponseHeaderName(index));
            _headers.Append(": ");
            _headers.Append(value);
            _headers.Append("\r\n");
        }

        public override void SendUnknownResponseHeader(string name, string value) {
            Debug.Assert(!_sent);
            _headers.Append(name);
            _headers.Append(": ");
            _headers.Append(value);
            _headers.Append("\r\n");
        }

        public override void SendCalculatedContentLength(int contentLength) {
            Debug.Assert(!_sent);
            /*
             * Do nothing - we append the content-length in STWNDSendResponse.
             */
        }

        public override bool HeadersSent() {
            return _sent;
        }

        public override bool IsClientConnected() {
            return UnsafeNativeMethods.STWNDIsClientConnected(_tracker);
        }

        public override void CloseConnection() {
            UnsafeNativeMethods.STWNDCloseConnection(_tracker);
        }

        private void SendResponse() {
            if (!_sent) {
                _sent = true;
                UnsafeNativeMethods.STWNDSendResponse(
                                    _tracker, 
                                    _status, 
                                    _status.Length, 
                                    _headers, 
                                    _headers.Length, 
                                    _unmanagedState);
            }
        }

        public override void SendResponseFromMemory(byte[] data, int length) {
            /*
             * The only content besides error message text is the pointer
             * to the state item in unmanaged memory.
             */
            if (_statusCode == 200) {
                Debug.Assert(_unmanagedState == IntPtr.Zero, "_unmanagedState == 0");
                Debug.Assert(length == IntPtr.Size, "length == IntPtr.Size");
                Debug.Assert(_methodIndex == UnsafeNativeMethods.StateProtocolVerb.GET, "verb == GET");
                Debug.Assert(_exclusive != UnsafeNativeMethods.StateProtocolExclusive.RELEASE,
                             "correct exclusive method");

                if (IntPtr.Size == 4) {
                    _unmanagedState = (IntPtr)
                        (((int)data[0])       |
                         ((int)data[1] << 8)  |
                         ((int)data[2] << 16) |
                         ((int)data[3] << 24));
                }
                else {
                    _unmanagedState = (IntPtr)
                        (((long)data[0])       |
                         ((long)data[1] << 8)  |
                         ((long)data[2] << 16) |
                         ((long)data[3] << 24) |
                         ((long)data[4] << 32) |
                         ((long)data[5] << 40) |
                         ((long)data[6] << 48) |
                         ((long)data[7] << 56));
                }

                Debug.Assert(_unmanagedState != IntPtr.Zero, "_unmanagedState != 0");
            }

            SendResponse();
        }

        public override void SendResponseFromFile(string filename, long offset, long length) {
            /* Not needed by state application */
            throw new NotSupportedException(SR.GetString(SR.Not_supported));
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
            /* Not needed by state application */
            throw new NotSupportedException(SR.GetString(SR.Not_supported));
        }

        public override void FlushResponse(bool finalFlush) {
            SendResponse();
        }

        public override void EndOfRequest() {
            SendResponse();
            UnsafeNativeMethods.STWNDEndOfRequest(_tracker);
        }
    }
}
