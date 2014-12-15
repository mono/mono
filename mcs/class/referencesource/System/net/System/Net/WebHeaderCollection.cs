//------------------------------------------------------------------------------
// <copyright file="WebHeaderCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Net.Cache;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    internal enum WebHeaderCollectionType : ushort {
        Unknown,
        WebRequest,
        WebResponse,
        HttpWebRequest,
        HttpWebResponse,
        HttpListenerRequest,
        HttpListenerResponse,
        FtpWebRequest,
        FtpWebResponse,
        FileWebRequest,
        FileWebResponse,
    }

    //
    // HttpHeaders - this is our main HttpHeaders object,
    //  which is a simple collection of name-value pairs,
    //  along with additional methods that provide HTTP parsing
    //  collection to sendable buffer capablities and other enhansments
    //  We also provide validation of what headers are allowed to be added.
    //

    /// <devdoc>
    ///    <para>
    ///       Contains protocol headers associated with a
    ///       request or response.
    ///    </para>
    /// </devdoc>
    [ComVisible(true), Serializable]
    public class WebHeaderCollection : NameValueCollection, ISerializable {
        //
        // Data and Constants
        //
        private const int ApproxAveHeaderLineSize = 30;
        private const int ApproxHighAvgNumHeaders = 16;
        private static readonly HeaderInfoTable HInfo = new HeaderInfoTable();

        //
        // Common Headers - used only when receiving a response, and internally.  If the user ever requests a header,
        // all the common headers are moved into the hashtable.
        //
        private string[] m_CommonHeaders;
        private int m_NumCommonHeaders;

        // Grouped by first character, so lookup is faster.  The table s_CommonHeaderHints maps first letters to indexes in this array.
        // After first character, sort by decreasing length.  It's ok if two headers have the same first character and length.
        private static readonly string[] s_CommonHeaderNames = new string[] {
            HttpKnownHeaderNames.AcceptRanges,      // "Accept-Ranges"       13
            HttpKnownHeaderNames.ContentLength,     // "Content-Length"      14
            HttpKnownHeaderNames.CacheControl,      // "Cache-Control"       13
            HttpKnownHeaderNames.ContentType,       // "Content-Type"        12
            HttpKnownHeaderNames.Date,              // "Date"                 4 
            HttpKnownHeaderNames.Expires,           // "Expires"              7
            HttpKnownHeaderNames.ETag,              // "ETag"                 4
            HttpKnownHeaderNames.LastModified,      // "Last-Modified"       13
            HttpKnownHeaderNames.Location,          // "Location"             8
            HttpKnownHeaderNames.ProxyAuthenticate, // "Proxy-Authenticate"  18
            HttpKnownHeaderNames.P3P,               // "P3P"                  3
            HttpKnownHeaderNames.SetCookie2,        // "Set-Cookie2"         11
            HttpKnownHeaderNames.SetCookie,         // "Set-Cookie"          10
            HttpKnownHeaderNames.Server,            // "Server"               6
            HttpKnownHeaderNames.Via,               // "Via"                  3
            HttpKnownHeaderNames.WWWAuthenticate,   // "WWW-Authenticate"    16
            HttpKnownHeaderNames.XAspNetVersion,    // "X-AspNet-Version"    16
            HttpKnownHeaderNames.XPoweredBy,        // "X-Powered-By"        12
            "[" };  // This sentinel will never match.  (This character isn't in the hint table.)

        // Mask off all but the bottom five bits, and look up in this array.
        private static readonly sbyte[] s_CommonHeaderHints = new sbyte[] {
            -1,  0, -1,  1,  4,  5, -1, -1,   // - a b c d e f g
            -1, -1, -1, -1,  7, -1, -1, -1,   // h i j k l m n o
             9, -1, -1, 11, -1, -1, 14, 15,   // p q r s t u v w
            16, -1, -1, -1, -1, -1, -1, -1 }; // x y z [ - - - -

        private const int c_AcceptRanges      =  0;
        private const int c_ContentLength     =  1;
        private const int c_CacheControl      =  2;
        private const int c_ContentType       =  3;
        private const int c_Date              =  4;
        private const int c_Expires           =  5;
        private const int c_ETag              =  6;
        private const int c_LastModified      =  7;
        private const int c_Location          =  8;
        private const int c_ProxyAuthenticate =  9;
        private const int c_P3P               = 10;
        private const int c_SetCookie2        = 11;
        private const int c_SetCookie         = 12;
        private const int c_Server            = 13;
        private const int c_Via               = 14;
        private const int c_WwwAuthenticate   = 15;
        private const int c_XAspNetVersion    = 16;
        private const int c_XPoweredBy        = 17;

        // Easy fast lookups for common headers.  More can be added.
        internal string ContentLength
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_ContentLength] : Get(s_CommonHeaderNames[c_ContentLength]);
            }
        }

        internal string CacheControl
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_CacheControl] : Get(s_CommonHeaderNames[c_CacheControl]);
            }
        }

        internal string ContentType
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_ContentType] : Get(s_CommonHeaderNames[c_ContentType]);
            }
        }

        internal string Date
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_Date] : Get(s_CommonHeaderNames[c_Date]);
            }
        }

        internal string Expires
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_Expires] : Get(s_CommonHeaderNames[c_Expires]);
            }
        }

        internal string ETag
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_ETag] : Get(s_CommonHeaderNames[c_ETag]);
            }
        }

        internal string LastModified
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_LastModified] : Get(s_CommonHeaderNames[c_LastModified]);
            }
        }

        internal string Location
        {
            get
            {
                string location = m_CommonHeaders != null
                    ? m_CommonHeaders[c_Location] : Get(s_CommonHeaderNames[c_Location]);
                // The normal header parser just casts bytes to chars. Check if there is a UTF8 host name.
                return HeaderEncoding.DecodeUtf8FromString(location);
            }
        }

        internal string ProxyAuthenticate
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_ProxyAuthenticate] : Get(s_CommonHeaderNames[c_ProxyAuthenticate]);
            }
        }

        internal string SetCookie2
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_SetCookie2] : Get(s_CommonHeaderNames[c_SetCookie2]);
            }
        }

        internal string SetCookie
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_SetCookie] : Get(s_CommonHeaderNames[c_SetCookie]);
            }
        }

        internal string Server
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_Server] : Get(s_CommonHeaderNames[c_Server]);
            }
        }

        internal string Via
        {
            get
            {
                return m_CommonHeaders != null ? m_CommonHeaders[c_Via] : Get(s_CommonHeaderNames[c_Via]);
            }
        }

        private void NormalizeCommonHeaders()
        {
            if (m_CommonHeaders == null)
                return;
            for (int i = 0; i < m_CommonHeaders.Length; i++)
                if (m_CommonHeaders[i] != null)
                    InnerCollection.Add(s_CommonHeaderNames[i], m_CommonHeaders[i]);

            m_CommonHeaders = null;
            m_NumCommonHeaders = 0;
        }

        //
        // To ensure C++ and IL callers can't pollute the underlying collection by calling overridden base members directly, we
        // will use a member collection instead.
        private NameValueCollection m_InnerCollection;

        private NameValueCollection InnerCollection
        {
            get
            {
                if (m_InnerCollection == null)
                    m_InnerCollection = new NameValueCollection(ApproxHighAvgNumHeaders, CaseInsensitiveAscii.StaticInstance);
                return m_InnerCollection;
            }
        }

        // this is the object that created the header collection.
        private WebHeaderCollectionType m_Type;

#if !FEATURE_PAL
        private bool AllowHttpRequestHeader {
            get {
                if (m_Type==WebHeaderCollectionType.Unknown) {
                    m_Type = WebHeaderCollectionType.WebRequest;
                }
                return m_Type==WebHeaderCollectionType.WebRequest || m_Type==WebHeaderCollectionType.HttpWebRequest || m_Type==WebHeaderCollectionType.HttpListenerRequest;
            }
        }

        internal bool AllowHttpResponseHeader {
            get {
                if (m_Type==WebHeaderCollectionType.Unknown) {
                    m_Type = WebHeaderCollectionType.WebResponse;
                }
                return m_Type==WebHeaderCollectionType.WebResponse || m_Type==WebHeaderCollectionType.HttpWebResponse || m_Type==WebHeaderCollectionType.HttpListenerResponse;
            }
        }

        public string this[HttpRequestHeader header] {
            get {
                if (!AllowHttpRequestHeader) {
                    throw new InvalidOperationException(SR.GetString(SR.net_headers_req));
                }
                return this[UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int)header)];
            }
            set {
                if (!AllowHttpRequestHeader) {
                    throw new InvalidOperationException(SR.GetString(SR.net_headers_req));
                }
                this[UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int)header)] = value;
            }
        }
        public string this[HttpResponseHeader header] {
            get {
                if (!AllowHttpResponseHeader) {
                    throw new InvalidOperationException(SR.GetString(SR.net_headers_rsp));
                }

                // Some of these can be mapped to Common Headers.  Other cases can be added as needed for perf.
                if (m_CommonHeaders != null)
                {
                    switch (header)
                    {
                        case HttpResponseHeader.ProxyAuthenticate:
                            return m_CommonHeaders[c_ProxyAuthenticate];

                        case HttpResponseHeader.WwwAuthenticate:
                            return m_CommonHeaders[c_WwwAuthenticate];
                    }
                }

                return this[UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int)header)];
            }
            set {
                if (!AllowHttpResponseHeader) {
                    throw new InvalidOperationException(SR.GetString(SR.net_headers_rsp));
                }
                if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                    if (value!=null && value.Length>ushort.MaxValue) {
                        throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                    }
                }
                this[UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int)header)] = value;
            }
        }

        public void Add(HttpRequestHeader header, string value) {
            if (!AllowHttpRequestHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_req));
            }
            this.Add(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int)header), value);
        }

        public void Add(HttpResponseHeader header, string value) {
            if (!AllowHttpResponseHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_rsp));
            }
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            this.Add(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int)header), value);
        }

        public void Set(HttpRequestHeader header, string value) {
            if (!AllowHttpRequestHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_req));
            }
            this.Set(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int)header), value);
        }

        public void Set(HttpResponseHeader header, string value) {
            if (!AllowHttpResponseHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_rsp));
            }
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            this.Set(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int)header), value);
        }


        internal void SetInternal(HttpResponseHeader header, string value) {
            if (!AllowHttpResponseHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_rsp));
            }
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            this.SetInternal(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int)header), value);
        }


        public void Remove(HttpRequestHeader header) {
            if (!AllowHttpRequestHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_req));
            }
            this.Remove(UnsafeNclNativeMethods.HttpApi.HTTP_REQUEST_HEADER_ID.ToString((int)header));
        }

        public void Remove(HttpResponseHeader header) {
            if (!AllowHttpResponseHeader) {
                throw new InvalidOperationException(SR.GetString(SR.net_headers_rsp));
            }
            this.Remove(UnsafeNclNativeMethods.HttpApi.HTTP_RESPONSE_HEADER_ID.ToString((int)header));
        }
#endif // !FEATURE_PAL

        // In general, HttpWebResponse headers aren't modified, so these methods don't support common headers.

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void AddWithoutValidate(string headerName, string headerValue) {
            headerName = CheckBadChars(headerName, false);
            headerValue = CheckBadChars(headerValue, true);
            GlobalLog.Print("WebHeaderCollection::AddWithoutValidate() calling InnerCollection.Add() key:[" + headerName + "], value:[" + headerValue + "]");
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (headerValue!=null && headerValue.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("headerValue", headerValue, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Add(headerName, headerValue);
        }

        internal void SetAddVerified(string name, string value) {
            if(HInfo[name].AllowMultiValues) {
                GlobalLog.Print("WebHeaderCollection::SetAddVerified() calling InnerCollection.Add() key:[" + name + "], value:[" + value + "]");
                NormalizeCommonHeaders();
                InvalidateCachedArrays();
                InnerCollection.Add(name, value);
            }
            else {
                GlobalLog.Print("WebHeaderCollection::SetAddVerified() calling InnerCollection.Set() key:[" + name + "], value:[" + value + "]");
                NormalizeCommonHeaders();
                InvalidateCachedArrays();
                InnerCollection.Set(name, value);
            }
        }

        // Below three methods are for fast headers manipulation, bypassing all the checks
        internal void AddInternal(string name, string value) {
            GlobalLog.Print("WebHeaderCollection::AddInternal() calling InnerCollection.Add() key:[" + name + "], value:[" + value + "]");
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Add(name, value);
        }

        internal void ChangeInternal(string name, string value) {
            GlobalLog.Print("WebHeaderCollection::ChangeInternal() calling InnerCollection.Set() key:[" + name + "], value:[" + value + "]");
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Set(name, value);
        }


        internal void RemoveInternal(string name) {
            GlobalLog.Print("WebHeaderCollection::RemoveInternal() calling InnerCollection.Remove() key:[" + name + "]");
            NormalizeCommonHeaders();
            if (m_InnerCollection != null)
            {
                InvalidateCachedArrays();
                m_InnerCollection.Remove(name);
            }
        }

        internal void CheckUpdate(string name, string value) {
            value = CheckBadChars(value, true);
            ChangeInternal(name, value);
        }

        // This even faster one can be used to add headers when it's known not to be a common header or that common headers aren't active.
        private void AddInternalNotCommon(string name, string value)
        {
            GlobalLog.Print("WebHeaderCollection::AddInternalNotCommon() calling InnerCollection.Add() key:[" + name + "], value:[" + value + "]");
            InvalidateCachedArrays();
            InnerCollection.Add(name, value);
        }


        private static readonly char[] HttpTrimCharacters = new char[]{(char)0x09,(char)0xA,(char)0xB,(char)0xC,(char)0xD,(char)0x20};

        //
        // CheckBadChars - throws on invalid chars to be not found in header name/value
        //
        internal static string CheckBadChars(string name, bool isHeaderValue) {

            if (name == null || name.Length == 0) {
                // emtpy name is invlaid
                if (!isHeaderValue) {
                    throw name == null ? new ArgumentNullException("name") :
                        new ArgumentException(SR.GetString(SR.net_emptystringcall, "name"), "name");
                }
                //empty value is OK
                return string.Empty;
            }

            if (isHeaderValue) {
                // VALUE check
                //Trim spaces from both ends
                name = name.Trim(HttpTrimCharacters);

                //First, check for correctly formed multi-line value
                //Second, check for absenece of CTL characters
                int crlf = 0;
                for(int i = 0; i < name.Length; ++i) {
                    char c = (char) (0x000000ff & (uint) name[i]);
                    switch (crlf)
                    {
                        case 0:
                            if (c == '\r')
                            {
                                crlf = 1;
                            }
                            else if (c == '\n')
                            {
                                // Technically this is bad HTTP.  But it would be a breaking change to throw here.
                                // Is there an exploit?
                                crlf = 2;
                            }
                            else if (c == 127 || (c < ' ' && c != '\t'))
                            {
                                throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidControlChars), "value");
                            }
                            break;

                        case 1:
                            if (c == '\n')
                            {
                                crlf = 2;
                                break;
                            }
                            throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidCRLFChars), "value");

                        case 2:
                            if (c == ' ' || c == '\t')
                            {
                                crlf = 0;
                                break;
                            }
                            throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidCRLFChars), "value");
                    }
                }
                if (crlf != 0)
                {
                    throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidCRLFChars), "value");
                }
            }
            else {
                // NAME check
                //First, check for absence of separators and spaces
                if (name.IndexOfAny(ValidationHelper.InvalidParamChars) != -1) {
                    throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidHeaderChars), "name");
                }

                //Second, check for non CTL ASCII-7 characters (32-126)
                if (ContainsNonAsciiChars(name)) {
                    throw new ArgumentException(SR.GetString(SR.net_WebHeaderInvalidNonAsciiChars), "name");
                }
            }
            return name;
        }

        internal static bool IsValidToken(string token) {
            return (token.Length > 0)
                && (token.IndexOfAny(ValidationHelper.InvalidParamChars) == -1)
                && !ContainsNonAsciiChars(token);
        }

        internal static bool ContainsNonAsciiChars(string token) {
            for (int i = 0; i < token.Length; ++i) {
                if ((token[i] < 0x20) || (token[i] > 0x7e)) {
                    return true;
                }
            }
            return false;
        }

        //
        // ThrowOnRestrictedHeader - generates an error if the user,
        //  passed in a reserved string as the header name
        //
        internal void ThrowOnRestrictedHeader(string headerName)
        {
            if (m_Type == WebHeaderCollectionType.HttpWebRequest)
            {
                if (HInfo[headerName].IsRequestRestricted)
                {
                    throw new ArgumentException(SR.GetString(SR.net_headerrestrict, headerName), "name");
                }
            }
            else if (m_Type == WebHeaderCollectionType.HttpListenerResponse)
            {
                if (HInfo[headerName].IsResponseRestricted)
                {
                    throw new ArgumentException(SR.GetString(SR.net_headerrestrict, headerName), "name");
                }
            }
        }

        //
        // Our Public METHOD set, most are inherited from NameValueCollection,
        //  not all methods from NameValueCollection are listed, even though usable -
        //
        //  this includes
        //  Add(name, value)
        //  Add(header)
        //  this[name] {set, get}
        //  Remove(name), returns bool
        //  Remove(name), returns void
        //  Set(name, value)
        //  ToString()
        //
        //  SplitValue(name, value)
        //  ToByteArray()
        //  ParseHeaders(char [], ...)
        //  ParseHeaders(byte [], ...)
        //

        // Add more headers; if "name" already exists it will
        // add concatenated value


        // Add -
        //  Routine Description:
        //      Adds headers with validation to see if they are "proper" headers.
        //      Will cause header to be concat to existing if already found.
        //      If the header is a special header, listed in RestrictedHeaders object,
        //      then this call will cause an exception indication as such.
        //  Arguments:
        //      name - header-name to add
        //      value - header-value to add, a header is already there, will concat this value
        //  Return Value:
        //      None

        /// <devdoc>
        ///    <para>
        ///       Adds a new header with the indicated name and value.
        ///    </para>
        /// </devdoc>
        public override void Add(string name, string value) {
            name = CheckBadChars(name, false);
            ThrowOnRestrictedHeader(name);
            value = CheckBadChars(value, true);
            GlobalLog.Print("WebHeaderCollection::Add() calling InnerCollection.Add() key:[" + name + "], value:[" + value + "]");
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Add(name, value);
        }


        // Add -
        // Routine Description:
        //     Adds headers with validation to see if they are "proper" headers.
        //     Assumes a combined a "Name: Value" string, and parses the two parts out.
        //     Will cause header to be concat to existing if already found.
        //     If the header is a speical header, listed in RestrictedHeaders object,
        //     then this call will cause an exception indication as such.
        // Arguments:
        //     header - header name: value pair
        // Return Value:
        //     None

        /// <devdoc>
        ///    <para>
        ///       Adds the indicated header.
        ///    </para>
        /// </devdoc>
        public void Add(string header) {
            if ( ValidationHelper.IsBlankString(header) ) {
                throw new ArgumentNullException("header");
            }
            int colpos = header.IndexOf(':');
            // check for badly formed header passed in
            if (colpos<0) {
                throw new ArgumentException(SR.GetString(SR.net_WebHeaderMissingColon), "header");
            }
            string name = header.Substring(0, colpos);
            string value = header.Substring(colpos+1);
            name = CheckBadChars(name, false);
            ThrowOnRestrictedHeader(name);
            value = CheckBadChars(value, true);
            GlobalLog.Print("WebHeaderCollection::Add(" + header + ") calling InnerCollection.Add() key:[" + name + "], value:[" + value + "]");
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Add(name, value);
        }

        // Set -
        // Routine Description:
        //     Sets headers with validation to see if they are "proper" headers.
        //     If the header is a special header, listed in RestrictedHeaders object,
        //     then this call will cause an exception indication as such.
        // Arguments:
        //     name - header-name to set
        //     value - header-value to set
        // Return Value:
        //     None

        /// <devdoc>
        ///    <para>
        ///       Sets the specified header to the specified value.
        ///    </para>
        /// </devdoc>
        public override void Set(string name, string value) {
            if (ValidationHelper.IsBlankString(name)) {
                throw new ArgumentNullException("name");
            }
            name = CheckBadChars(name, false);
            ThrowOnRestrictedHeader(name);
            value = CheckBadChars(value, true);
            GlobalLog.Print("WebHeaderCollection::Set() calling InnerCollection.Set() key:[" + name + "], value:[" + value + "]");
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Set(name, value);
        }


        internal void SetInternal(string name, string value) {
            if (ValidationHelper.IsBlankString(name)) {
                throw new ArgumentNullException("name");
            }
            name = CheckBadChars(name, false);
            value = CheckBadChars(value, true);
            GlobalLog.Print("WebHeaderCollection::Set() calling InnerCollection.Set() key:[" + name + "], value:[" + value + "]");
            if (m_Type==WebHeaderCollectionType.HttpListenerResponse) {
                if (value!=null && value.Length>ushort.MaxValue) {
                    throw new ArgumentOutOfRangeException("value", value, SR.GetString(SR.net_headers_toolong, ushort.MaxValue));
                }
            }
            NormalizeCommonHeaders();
            InvalidateCachedArrays();
            InnerCollection.Set(name, value);
        }


        // Remove -
        // Routine Description:
        //     Removes give header with validation to see if they are "proper" headers.
        //     If the header is a speical header, listed in RestrictedHeaders object,
        //     then this call will cause an exception indication as such.
        // Arguments:
        //     name - header-name to remove
        // Return Value:
        //     None

        /// <devdoc>
        ///    <para>Removes the specified header.</para>
        /// </devdoc>
        public override void Remove(string name) {
            if ( ValidationHelper.IsBlankString(name) ) {
                throw new ArgumentNullException("name");
            }
            ThrowOnRestrictedHeader(name);
            name = CheckBadChars(name,  false);
            GlobalLog.Print("WebHeaderCollection::Remove() calling InnerCollection.Remove() key:[" + name + "]");
            NormalizeCommonHeaders();
            if (m_InnerCollection != null)
            {
                InvalidateCachedArrays();
                m_InnerCollection.Remove(name);
            }
        }


        // GetValues
        // Routine Description:
        //     This method takes a header name and returns a string array representing
        //     the individual values for that headers. For example, if the headers
        //     contained the line Accept: text/plain, text/html then
        //     GetValues("Accept") would return an array of two strings: "text/plain"
        //     and "text/html".
        // Arguments:
        //     header      - Name of the header.
        // Return Value:
        //     string[] - array of parsed string objects

        /// <devdoc>
        ///    <para>
        ///       Gets an array of header values stored in a
        ///       header.
        ///    </para>
        /// </devdoc>
        public override string[] GetValues(string header) {
            // This method doesn't work with common headers.  Dump common headers into the pool.
            NormalizeCommonHeaders();

            // First get the information about the header and the values for
            // the header.
            HeaderInfo Info = HInfo[header];
            string[] Values = InnerCollection.GetValues(header);
            // If we have no information about the header or it doesn't allow
            // multiple values, just return the values.
            if (Info == null || Values == null || !Info.AllowMultiValues) {
                return Values;
            }
            // Here we have a multi value header. We need to go through
            // each entry in the multi values array, and if an entry itself
            // has multiple values we'll need to combine those in.
            //
            // We do some optimazation here, where we try not to copy the
            // values unless there really is one that have multiple values.
            string[] TempValues;
            ArrayList ValueList = null;
            int i;
            for (i = 0; i < Values.Length; i++) {
                // Parse this value header.
                TempValues = Info.Parser(Values[i]);
                // If we don't have an array list yet, see if this
                // value has multiple values.
                if (ValueList == null) {
                    // See if it has multiple values.
                    if (TempValues.Length > 1) {
                        // It does, so we need to create an array list that
                        // represents the Values, then trim out this one and
                        // the ones after it that haven't been parsed yet.
                        ValueList = new ArrayList(Values);
                        ValueList.RemoveRange(i, Values.Length - i);
                        ValueList.AddRange(TempValues);
                    }
                }
                else {
                    // We already have an ArrayList, so just add the values.
                    ValueList.AddRange(TempValues);
                }
            }
            // See if we have an ArrayList. If we don't, just return the values.
            // Otherwise convert the ArrayList to a string array and return that.
            if (ValueList != null) {
                string[] ReturnArray = new string[ValueList.Count];
                ValueList.CopyTo(ReturnArray);
                return ReturnArray;
            }
            return Values;
        }


        // ToString()  -
        // Routine Description:
        //     Generates a string representation of the headers, that is ready to be sent except for it being in string format:
        //     the format looks like:
        //
        //     Header-Name: Header-Value\r\n
        //     Header-Name2: Header-Value2\r\n
        //     ...
        //     Header-NameN: Header-ValueN\r\n
        //     \r\n
        //
        //     Uses the string builder class to Append the elements together.
        // Arguments:
        //     None.
        // Return Value:
        //     string

        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       Obsolete.
        ///    </para>
        /// </devdoc>
        public override string ToString() {
            string result = GetAsString(this, false, false);
            GlobalLog.Print("WebHeaderCollection::ToString: \r\n" + result);
            return result;
        }

        internal string ToString(bool forTrace)
        {
            return GetAsString(this, false, true);
        }
            

        //
        // if winInetCompat = true then it will not insert spaces after ':'
        // and it will output "~U" header first
        //
        internal static string GetAsString(NameValueCollection cc, 
                                           bool                winInetCompat,
                                           bool                forTrace) {
#if FEATURE_PAL
            if (winInetCompat) {
                throw new InvalidOperationException();
            }
#endif // FEATURE_PAL

            if (cc == null || cc.Count == 0) {
                return "\r\n";
            }
            StringBuilder sb = new StringBuilder(ApproxAveHeaderLineSize*cc.Count);
            string statusLine;
            statusLine = cc[string.Empty];
            if (statusLine != null) {
                sb.Append(statusLine).Append("\r\n");
            }
            for (int i = 0; i < cc.Count ; i++) {
                string key = cc.GetKey(i) as string;
                string val = cc.Get(i) as string;
                /*
                if (forTrace)
                {
                    // Put a condition here that if we are using basic auth, 
                    // we shouldn't put the authorization header. Otherwise
                    // the password will get saved in the trace.
                    if (using basic)
                        continue;
                }
                */
                if (ValidationHelper.IsBlankString(key)) {
                    continue;
                }
                sb.Append(key);
                if (winInetCompat) {
                    sb.Append(':');
                }
                else {
                    sb.Append(": ");
                }
                sb.Append(val).Append("\r\n");
            }
            if (!forTrace)
                sb.Append("\r\n");
            return sb.ToString();
        }


        // ToByteArray()  -
        // Routine Description:
        //     Generates a byte array representation of the headers, that is ready to be sent.
        //     So it Serializes our headers into a byte array suitable for sending over the net.
        //
        //     the format looks like:
        //
        //     Header-Name1: Header-Value1\r\n
        //     Header-Name2: Header-Value2\r\n
        //     ...
        //     Header-NameN: Header-ValueN\r\n
        //     \r\n
        //
        //     Uses the ToString() method to generate, and then performs conversion.
        //
        //     Performance Note:  Why are we not doing a single copy/covert run?
        //     As the code before used to know the size of the output!
        //     Because according to Demitry, its cheaper to copy the headers twice,
        //     then it is to call the UNICODE to ANSI conversion code many times.
        // Arguments:
        //     None.
        // Return Value:
        //     byte [] - array of bytes values

        /// <internalonly/>
        /// <devdoc>
        ///    <para>
        ///       Obsolete.
        ///    </para>
        /// </devdoc>
        public byte[] ToByteArray() {
            // Make sure the buffer is big enough.
            string tempStr = ToString();
            //
            // Use the string of headers, convert to Char Array,
            //  then convert to Bytes,
            //  serializing finally into the buffer, along the way.
            //
            byte[] buffer = HeaderEncoding.GetBytes(tempStr);
            return buffer;
        }

        /// <devdoc>
        ///    <para>Tests if access to the HTTP header with the provided name is accessible for setting.</para>
        /// </devdoc>
        public static bool IsRestricted(string headerName)
        {
            return IsRestricted(headerName, false);
        }

        public static bool IsRestricted(string headerName, bool response)
        {
            return response ? HInfo[CheckBadChars(headerName, false)].IsResponseRestricted : HInfo[CheckBadChars(headerName, false)].IsRequestRestricted;
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.WebHeaderCollection'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public WebHeaderCollection() : base(DBNull.Value)
        {
        }

        internal WebHeaderCollection(WebHeaderCollectionType type) : base(DBNull.Value)
        {
            m_Type = type;
            if (type == WebHeaderCollectionType.HttpWebResponse)
                m_CommonHeaders = new string[s_CommonHeaderNames.Length - 1];  // Minus one for the sentinel.
        }

        //This is for Cache
        internal WebHeaderCollection(NameValueCollection cc): base(DBNull.Value)
        {
            m_InnerCollection = new NameValueCollection(cc.Count + 2, CaseInsensitiveAscii.StaticInstance);
            int len = cc.Count;
            for (int i = 0; i < len; ++i) {
                String key = cc.GetKey(i);
                String[] values = cc.GetValues(i);
                if (values != null) {
                    for (int j = 0; j < values.Length; j++) {
                        InnerCollection.Add(key, values[j]);
                    }
                }
                else {
                    InnerCollection.Add(key, null);
                }
            }
        }

        //
        // ISerializable constructor
        //
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected WebHeaderCollection(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(DBNull.Value)
        {
            int count = serializationInfo.GetInt32("Count");
            m_InnerCollection = new NameValueCollection(count + 2, CaseInsensitiveAscii.StaticInstance);
            for (int i = 0; i < count; i++) {
                string headerName = serializationInfo.GetString(i.ToString(NumberFormatInfo.InvariantInfo));
                string headerValue = serializationInfo.GetString((i+count).ToString(NumberFormatInfo.InvariantInfo));
                GlobalLog.Print("WebHeaderCollection::.ctor(ISerializable) calling InnerCollection.Add() key:[" + headerName + "], value:[" + headerValue + "]");
                InnerCollection.Add(headerName, headerValue);
            }
        }

        public override void OnDeserialization(object sender) {
            // 


        }

        //
        // ISerializable method
        //
        /// <internalonly/>
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)] 		
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext) {
            //
            // for now disregard streamingContext.
            //
            NormalizeCommonHeaders();
            serializationInfo.AddValue("Count", Count);
            for (int i = 0; i < Count; i++)
            {
                serializationInfo.AddValue(i.ToString(NumberFormatInfo.InvariantInfo), GetKey(i));
                serializationInfo.AddValue((i + Count).ToString(NumberFormatInfo.InvariantInfo), Get(i));
            }
        }


        // we use this static class as a helper class to encode/decode HTTP headers.
        // what we need is a 1-1 correspondence between a char in the range U+0000-U+00FF
        // and a byte in the range 0x00-0xFF (which is the range that can hit the network).
        // The Latin-1 encoding (ISO-88591-1) (GetEncoding(28591)) works for byte[] to string, but is a little slow.
        // It doesn't work for string -> byte[] because of best-fit-mapping problems.
        internal static class HeaderEncoding
        {
            internal static unsafe string GetString(byte[] bytes, int byteIndex, int byteCount)
            {
                fixed(byte* pBytes = bytes)
                    return GetString(pBytes + byteIndex, byteCount);
            }

            internal static unsafe string GetString(byte* pBytes, int byteCount)
            {
                if (byteCount < 1)
                    return "";

                string s = new String('\0', byteCount);

                fixed (char* pStr = s)
                {
                    char* pString = pStr;
                    while (byteCount >= 8)
                    {
                        pString[0] = (char) pBytes[0];
                        pString[1] = (char) pBytes[1];
                        pString[2] = (char) pBytes[2];
                        pString[3] = (char) pBytes[3];
                        pString[4] = (char) pBytes[4];
                        pString[5] = (char) pBytes[5];
                        pString[6] = (char) pBytes[6];
                        pString[7] = (char) pBytes[7];
                        pString += 8;
                        pBytes += 8;
                        byteCount -= 8;
                    }
                    for (int i = 0; i < byteCount; i++)
                    {
                        pString[i] = (char) pBytes[i];
                    }
                }

                return s;
            }

            internal static int GetByteCount(string myString) {
                return myString.Length;
            }
            internal unsafe static void GetBytes(string myString, int charIndex, int charCount, byte[] bytes, int byteIndex) {
                if (myString.Length==0) {
                    return;
                }
                fixed (byte *bufferPointer = bytes) {
                    byte* newBufferPointer = bufferPointer + byteIndex;
                    int finalIndex = charIndex + charCount;
                    while (charIndex<finalIndex) {
                        *newBufferPointer++ = (byte)myString[charIndex++];
                    }
                }
            }
            internal unsafe static byte[] GetBytes(string myString) {
                byte[] bytes = new byte[myString.Length];
                if (myString.Length!=0) {
                    GetBytes(myString, 0, myString.Length, bytes, 0);
                }
                return bytes;
            }

            // The normal client header parser just casts bytes to chars (see GetString).
            // Check if those bytes were actually utf-8 instead of ASCII.
            // If not, just return the input value.
            [System.Runtime.CompilerServices.FriendAccessAllowed]
            internal static string DecodeUtf8FromString(string input) {
                if (string.IsNullOrWhiteSpace(input)) {
                    return input;
                }

                bool possibleUtf8 = false;
                for (int i = 0; i < input.Length; i++) {
                    if (input[i] > (char)255) {
                        return input; // This couldn't have come from the wire, someone assigned it directly.
                    }
                    else if (input[i] > (char)127) {
                        possibleUtf8 = true;
                        break;
                    }
                }
                if (possibleUtf8) {
                    byte[] rawBytes = new byte[input.Length];
                    for (int i = 0; i < input.Length; i++) {
                        if (input[i] > (char)255) {
                            return input; // This couldn't have come from the wire, someone assigned it directly.
                        }
                        rawBytes[i] = (byte)input[i];
                    }
                    try {
                        // We don't want '?' replacement characters, just fail.
                        Encoding decoder = Encoding.GetEncoding("utf-8", EncoderFallback.ExceptionFallback,
                            DecoderFallback.ExceptionFallback);
                        return decoder.GetString(rawBytes);
                    }
                    catch (ArgumentException) { } // Not actually Utf-8
                }
                return input;
            }
        }


        // ParseHeaders -
        // Routine Description:
        //
        //     This code is optimized for the case in which all the headers fit in the buffer.
        //     we support multiple re-entrance, but we won't save intermediate
        //     state, we will just roll back all the parsing done for the current header if we can't
        //     parse a whole one (including multiline) or decide something else ("invalid data" or "done parsing").
        //
        //     we're going to cycle through the loop until we
        //
        //     1) find an HTTP violation (in this case we return DataParseStatus.Invalid)
        //     2) we need more data (in this case we return DataParseStatus.NeedMoreData)
        //     3) we found the end of the headers and the beginning of the entity body (in this case we return DataParseStatus.Done)
        //
        //
        // Arguments:
        //
        //     buffer      - buffer containing the data to be parsed
        //     size        - size of the buffer
        //     unparsed    - offset of data yet to be parsed
        //
        // Return Value:
        //
        //     DataParseStatus - status of parsing
        //
        // Revision:
        //
        //     02/13/2001 rewrote the method from scratch.
        //
        // BreakPoint:
        //
        //     b system.dll!System.Net.WebHeaderCollection::ParseHeaders
        internal unsafe DataParseStatus ParseHeaders(
                byte[] buffer, 
                int size, 
                ref int unparsed, 
                ref int totalResponseHeadersLength, 
                int maximumResponseHeadersLength, 
                ref WebParseError parseError) {

            fixed (byte * byteBuffer = buffer) {

            char ch;

            // quick check in the boundaries (as we use unsafe pointer)
            if (buffer.Length < size) {
                return DataParseStatus.NeedMoreData;
            }

            int headerNameStartOffset = -1;
            int headerNameEndOffset = -1;
            int headerValueStartOffset = -1;
            int headerValueEndOffset = -1;
            int numberOfLf = -1;
            int index = unparsed;
            bool spaceAfterLf;
            string headerMultiLineValue;
            string headerName;
            string headerValue;

            // we need this because this method is entered multiple times.
            int localTotalResponseHeadersLength = totalResponseHeadersLength;

            WebParseErrorCode parseErrorCode = WebParseErrorCode.Generic;
            DataParseStatus parseStatus = DataParseStatus.Invalid;
#if TRAVE
            GlobalLog.Enter("WebHeaderCollection::ParseHeaders(): ANSI size:" + size.ToString() + ", unparsed:" + unparsed.ToString() + " buffer:[" + Encoding.ASCII.GetString(buffer, unparsed, Math.Min(256, size-unparsed)) + "]");
#endif

            //
            // according to RFC216 a header can have the following syntax:
            //
            // message-header = field-name ":" [ field-value ]
            // field-name     = token
            // field-value    = *( field-content | LWS )
            // field-content  = <the OCTETs making up the field-value and consisting of either *TEXT or combinations of token, separators, and quoted-string>
            // TEXT           = <any OCTET except CTLs, but including LWS>
            // CTL            = <any US-ASCII control character (octets 0 - 31) and DEL (127)>
            // SP             = <US-ASCII SP, space (32)>
            // HT             = <US-ASCII HT, horizontal-tab (9)>
            // CR             = <US-ASCII CR, carriage return (13)>
            // LF             = <US-ASCII LF, linefeed (10)>
            // LWS            = [CR LF] 1*( SP | HT )
            // CHAR           = <any US-ASCII character (octets 0 - 127)>
            // token          = 1*<any CHAR except CTLs or separators>
            // separators     = "(" | ")" | "<" | ">" | "@" | "," | ";" | ":" | "\" | <"> | "/" | "[" | "]" | "?" | "=" | "{" | "}" | SP | HT
            // quoted-string  = ( <"> *(qdtext | quoted-pair ) <"> )
            // qdtext         = <any TEXT except <">>
            // quoted-pair    = "\" CHAR
            //

            //
            // At each iteration of the following loop we expect to parse a single HTTP header entirely.
            //
            for (;;) {
                //
                // trim leading whitespaces (LWS) just for extra robustness, in fact if there are leading white spaces then:
                // 1) it could be that after the status line we might have spaces. handle this.
                // 2) this should have been detected to be a multiline header so there'll be no spaces and we'll spend some time here.
                //
                headerName = string.Empty;
                headerValue = string.Empty;
                spaceAfterLf = false;
                headerMultiLineValue = null;

                if (Count == 0) {
                    //
                    // so, restrict this extra trimming only on the first header line
                    //
                    while (index < size) {
                         ch = (char) byteBuffer[index];
                         if (ch == ' ' || ch == '\t') {
                             ++index;
                            if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                                parseStatus = DataParseStatus.DataTooBig;
                                goto quit;
                            }
                        }
                        else {
                            break;
                        }
                    }

                    if (index==size) {
                        //
                        // we reached the end of the buffer. ask for more data.
                        //
                        parseStatus = DataParseStatus.NeedMoreData;
                        goto quit;
                    }
                }

                //
                // what we have here is the beginning of a new header
                //
                headerNameStartOffset = index;

                while (index < size) {
                    ch = (char) byteBuffer[index];
                    if (ch != ':' && ch != '\n') {
                        if (ch > ' ') {
                            //
                            // if there's an illegal character we should return DataParseStatus.Invalid
                            // instead we choose to be flexible, try to trim it, but include it in the string
                            //
                            headerNameEndOffset = index;
                        }
                        ++index;
                        if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else {
                        if (ch == ':') {
                            ++index;
                            if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                                parseStatus = DataParseStatus.DataTooBig;
                                goto quit;
                            }
                        }
                        break;
                    }
                }
                if (index==size) {
                    //
                    // we reached the end of the buffer. ask for more data.
                    //
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

startOfValue:
                //
                // skip all [' ','\t','\r','\n'] characters until HeaderValue starts
                // if we didn't find any headers yet, we set numberOfLf to 1
                // so that we take the '\n' from the status line into account
                //

                numberOfLf = (Count == 0 && headerNameEndOffset < 0) ? 1 : 0;
                while (index<size && numberOfLf<2) {
                    ch = (char) byteBuffer[index];
                    if (ch <= ' ') {
                        if (ch=='\n') {
                            numberOfLf++;
                            // In this case, need to check for a space.
                            if (numberOfLf == 1)
                            {
                                if (index + 1 == size)
                                {
                                    //
                                    // we reached the end of the buffer. ask for more data.
                                    // need to be able to peek after the \n and see if there's some space.
                                    //
                                    parseStatus = DataParseStatus.NeedMoreData;
                                    goto quit;
                                }
                                spaceAfterLf = (char) byteBuffer[index + 1] == ' ' || (char) byteBuffer[index + 1] == '\t';
                            }
                        }
                        ++index;
                        if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else {
                        break;
                    }
                }
                if (numberOfLf==2 || (numberOfLf==1 && !spaceAfterLf)) {
                    //
                    // if we've counted two '\n' we got at the end of the headers even if we're past the end of the buffer
                    // if we've counted one '\n' and the first character after that was a ' ' or a '\t'
                    // no matter if we found a ':' or not, treat this as an empty header name.
                    //
                    goto addHeader;
                }
                if (index==size) {
                    //
                    // we reached the end of the buffer. ask for more data.
                    //
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

                headerValueStartOffset = index;

                while (index<size) {
                    ch = (char) byteBuffer[index];
                    if (ch != '\n') {
                        if (ch > ' ') {
                            headerValueEndOffset = index;
                        }
                        ++index;
                        if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else {
                        break;
                    }
                }
                if (index==size) {
                    //
                    // we reached the end of the buffer. ask for more data.
                    //
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

                //
                // at this point we found either a '\n' or the end of the headers
                // hence we are at the end of the Header Line. 4 options:
                // 1) need more data
                // 2) if we find two '\n' => end of headers
                // 3) if we find one '\n' and a ' ' or a '\t' => multiline header
                // 4) if we find one '\n' and a valid char => next header
                //
                numberOfLf = 0;
                while (index<size && numberOfLf<2) {
                    ch = (char) byteBuffer[index];
                    if (ch =='\r' || ch == '\n') {
                        if (ch == '\n') {
                            numberOfLf++;
                        }
                        ++index;
                        if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                    }
                    else {
                        break;
                    }
                }
                if (index==size && numberOfLf<2) {
                    //
                    // we reached the end of the buffer but not of the headers. ask for more data.
                    //
                    parseStatus = DataParseStatus.NeedMoreData;
                    goto quit;
                }

addHeader:
                if (headerValueStartOffset>=0 && headerValueStartOffset>headerNameEndOffset && headerValueEndOffset>=headerValueStartOffset) {
                    //
                    // Encoding fastest way to build the UNICODE string off the byte[]
                    //
                    headerValue = HeaderEncoding.GetString(byteBuffer + headerValueStartOffset, headerValueEndOffset - headerValueStartOffset + 1);
                }

                //
                // if we got here from the beginning of the for loop, headerMultiLineValue will be null
                // otherwise it will contain the headerValue constructed for the multiline header
                // add this line as well to it, separated by a single space
                //
                headerMultiLineValue = (headerMultiLineValue==null ? headerValue : headerMultiLineValue + " " + headerValue);

                if (index < size && numberOfLf == 1) {
                    ch = (char) byteBuffer[index];
                    if (ch == ' ' || ch == '\t') {
                        //
                        // since we found only one Lf and the next header line begins with a Lws,
                        // this is the beginning of a multiline header.
                        // parse the next line into headerValue later it will be added to headerMultiLineValue
                        //
                        ++index;
                        if (maximumResponseHeadersLength>=0 && ++localTotalResponseHeadersLength>=maximumResponseHeadersLength) {
                            parseStatus = DataParseStatus.DataTooBig;
                            goto quit;
                        }
                        goto startOfValue;
                    }
                }

                if (headerNameStartOffset>=0 && headerNameEndOffset>=headerNameStartOffset) {
                    //
                    // Encoding is the fastest way to build the UNICODE string off the byte[]
                    //
                    headerName = HeaderEncoding.GetString(byteBuffer + headerNameStartOffset, headerNameEndOffset - headerNameStartOffset + 1);
                }

                //
                // now it's finally safe to add the header if we have a name for it
                //
                if (headerName.Length>0) {
                    //
                    // the base clasee will check for pre-existing headerValue and append
                    // it using commas as indicated in the RFC
                    //
                    GlobalLog.Print("WebHeaderCollection::ParseHeaders() calling AddInternal() key:[" + headerName + "], value:[" + headerMultiLineValue + "]");
                    AddInternal(headerName, headerMultiLineValue);
                }

                //
                // and update unparsed
                //
                totalResponseHeadersLength = localTotalResponseHeadersLength;
                unparsed = index;

                if (numberOfLf==2) {
                    parseStatus = DataParseStatus.Done;
                    goto quit;
                }

            } // for (;;)

quit:
            GlobalLog.Leave("WebHeaderCollection::ParseHeaders() returning parseStatus:" + parseStatus.ToString());
            if (parseStatus == DataParseStatus.Invalid) {
                parseError.Section = WebParseErrorSection.ResponseHeader; 
                parseError.Code    = parseErrorCode;
            }

            return parseStatus;
            }
        }

        //
        // Alternative parsing that follows RFC2616.  Like the above, this trims both sides of the header value and replaces
        // folding with a single space.
        //
        private enum RfcChar : byte
        {
            High = 0,
            Reg,
            Ctl,
            CR,
            LF,
            WS,
            Colon,
            Delim
        }

        private static RfcChar[] RfcCharMap = new RfcChar[128]
        {
            RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,
            RfcChar.Ctl,   RfcChar.WS,    RfcChar.LF,    RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.CR,    RfcChar.Ctl,   RfcChar.Ctl,
            RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,
            RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,   RfcChar.Ctl,
            RfcChar.WS,    RfcChar.Reg,   RfcChar.Delim, RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Delim, RfcChar.Delim, RfcChar.Reg,   RfcChar.Reg,   RfcChar.Delim, RfcChar.Reg,   RfcChar.Reg,   RfcChar.Delim,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Colon, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, RfcChar.Delim,
            RfcChar.Delim, RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Delim, RfcChar.Delim, RfcChar.Delim, RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,
            RfcChar.Reg,   RfcChar.Reg,   RfcChar.Reg,   RfcChar.Delim, RfcChar.Reg,   RfcChar.Delim, RfcChar.Reg,   RfcChar.Ctl,
        };

        internal unsafe DataParseStatus ParseHeadersStrict(
                byte[] buffer, 
                int size, 
                ref int unparsed, 
                ref int totalResponseHeadersLength, 
                int maximumResponseHeadersLength, 
                ref WebParseError parseError)
        {
            GlobalLog.Enter("WebHeaderCollection::ParseHeadersStrict(): size:" + size.ToString() + ", unparsed:" + unparsed.ToString() + " buffer:[" + Encoding.ASCII.GetString(buffer, unparsed, Math.Min(256, size-unparsed)) + "]");

            WebParseErrorCode parseErrorCode = WebParseErrorCode.Generic;
            DataParseStatus parseStatus = DataParseStatus.Invalid;

            int i = unparsed;
            RfcChar ch;
            int effectiveSize = maximumResponseHeadersLength <= 0 ? Int32.MaxValue : maximumResponseHeadersLength - totalResponseHeadersLength + i;
            DataParseStatus sizeError = DataParseStatus.DataTooBig;
            if (size < effectiveSize)
            {
                effectiveSize = size;
                sizeError = DataParseStatus.NeedMoreData;
            }

            // Verify the size.
            if (i >= effectiveSize)
            {
                parseStatus = sizeError;
                goto quit;
            }

            fixed (byte* byteBuffer = buffer)
            {
                while (true)
                {
                    // If this is CRLF, actually we're done.
                    if (byteBuffer[i] == '\r')
                    {
                        if (++i == effectiveSize)
                        {
                            parseStatus = sizeError;
                            goto quit;
                        }

                        if (byteBuffer[i++] == '\n')
                        {
                            totalResponseHeadersLength += i - unparsed;
                            unparsed = i;
                            parseStatus = DataParseStatus.Done;
                            goto quit;
                        }

                        parseStatus = DataParseStatus.Invalid;
                        parseErrorCode = WebParseErrorCode.CrLfError;
                        goto quit;
                    }

                    // Find the header name; only regular characters allowed.
                    int iBeginName = i;
                    for (; i < effectiveSize && (ch = byteBuffer[i] > 127 ? RfcChar.High : RfcCharMap[byteBuffer[i]]) == RfcChar.Reg; i++);
                    if (i == effectiveSize)
                    {
                        parseStatus = sizeError;
                        goto quit;
                    }
                    if (i == iBeginName)
                    {
                        parseStatus = DataParseStatus.Invalid;
                        parseErrorCode = WebParseErrorCode.InvalidHeaderName;
                        goto quit;
                    }

                    // Read to a colon.
                    int iEndName = i - 1;
                    int crlf = 0;  // 1 = cr, 2 = crlf
                    for (; i < effectiveSize && (ch = byteBuffer[i] > 127 ? RfcChar.High : RfcCharMap[byteBuffer[i]]) != RfcChar.Colon; i++)
                    {
                        switch (ch)
                        {
                            case RfcChar.WS:
                                if (crlf == 1)
                                {
                                    break;
                                }
                                crlf = 0;
                                continue;

                            case RfcChar.CR:
                                if (crlf == 0)
                                {
                                    crlf = 1;
                                    continue;
                                }
                                break;

                            case RfcChar.LF:
                                if (crlf == 1)
                                {
                                    crlf = 2;
                                    continue;
                                }
                                break;
                        }
                        parseStatus = DataParseStatus.Invalid;
                        parseErrorCode = WebParseErrorCode.CrLfError;
                        goto quit;
                    }
                    if (i == effectiveSize)
                    {
                        parseStatus = sizeError;
                        goto quit;
                    }
                    if (crlf != 0)                        
                    {
                        parseStatus = DataParseStatus.Invalid;
                        parseErrorCode = WebParseErrorCode.IncompleteHeaderLine;
                        goto quit;
                    }

                    // Skip the colon.
                    if (++i == effectiveSize)
                    {
                        parseStatus = sizeError;
                        goto quit;
                    }

                    // Read the value.  crlf = 3 means in the whitespace after a CRLF
                    int iBeginValue = -1;
                    int iEndValue = -1;
                    StringBuilder valueAccumulator = null;
                    for (; i < effectiveSize && ((ch = byteBuffer[i] > 127 ? RfcChar.High : RfcCharMap[byteBuffer[i]]) == RfcChar.WS || crlf != 2); i++)
                    {
                        switch (ch)
                        {
                            case RfcChar.WS:
                                if (crlf == 1)
                                {
                                    break;
                                }
                                if (crlf == 2)
                                {
                                    crlf = 3;
                                }
                                continue;

                            case RfcChar.CR:
                                if (crlf == 0)
                                {
                                    crlf = 1;
                                    continue;
                                }
                                break;

                            case RfcChar.LF:
                                if (crlf == 1)
                                {
                                    crlf = 2;
                                    continue;
                                }
                                break;

                            case RfcChar.High:
                            case RfcChar.Colon:
                            case RfcChar.Delim:
                            case RfcChar.Reg:
                                if (crlf == 1)
                                {
                                    break;
                                }
                                if (crlf == 3)
                                {
                                    crlf = 0;
                                    if (iBeginValue != -1)
                                    {
                                        string s = HeaderEncoding.GetString(byteBuffer + iBeginValue, iEndValue - iBeginValue + 1);
                                        if (valueAccumulator == null)
                                        {
                                            valueAccumulator = new StringBuilder(s, s.Length * 5);
                                        }
                                        else
                                        {
                                            valueAccumulator.Append(" ");
                                            valueAccumulator.Append(s);
                                        }
                                    }
                                    iBeginValue = -1;
                                }
                                if (iBeginValue == -1)
                                {
                                    iBeginValue = i;
                                }
                                iEndValue = i;
                                continue;
                        }
                        parseStatus = DataParseStatus.Invalid;
                        parseErrorCode = WebParseErrorCode.CrLfError;
                        goto quit;
                    }
                    if (i == effectiveSize)
                    {
                        parseStatus = sizeError;
                        goto quit;
                    }

                    // Make the value.
                    string sValue = iBeginValue == -1 ? "" : HeaderEncoding.GetString(byteBuffer + iBeginValue, iEndValue - iBeginValue + 1);
                    if (valueAccumulator != null)
                    {
                        if (sValue.Length != 0)
                        {
                            valueAccumulator.Append(" ");
                            valueAccumulator.Append(sValue);
                        }
                        sValue = valueAccumulator.ToString();
                    }

                    // Make the name.  See if it's a common header first.
                    string sName = null;
                    int headerNameLength = iEndName - iBeginName + 1;
                    if (m_CommonHeaders != null)
                    {
                        int iHeader = s_CommonHeaderHints[byteBuffer[iBeginName] & 0x1f];
                        if (iHeader >= 0)
                        {
                            while (true)
                            {
                                string s = s_CommonHeaderNames[iHeader++];

                                // Not found if we get to a shorter header or one with a different first character.
                                if (s.Length < headerNameLength || CaseInsensitiveAscii.AsciiToLower[byteBuffer[iBeginName]] != CaseInsensitiveAscii.AsciiToLower[s[0]])
                                    break;

                                // Keep looking if the common header is too long.
                                if (s.Length > headerNameLength)
                                    continue;

                                int j;
                                byte* pBuffer = byteBuffer + iBeginName + 1;
                                for (j = 1; j < s.Length; j++)
                                {
                                    // Avoid the case-insensitive compare in the common case where they match.
                                    if (*(pBuffer++) != s[j] && CaseInsensitiveAscii.AsciiToLower[*(pBuffer - 1)] != CaseInsensitiveAscii.AsciiToLower[s[j]])
                                        break;
                                }
                                if (j == s.Length)
                                {
                                    // Set it to the appropriate index.
                                    m_NumCommonHeaders++;
                                    iHeader--;
                                    if (m_CommonHeaders[iHeader] == null)
                                    {
                                        m_CommonHeaders[iHeader] = sValue;
                                    }
                                    else
                                    {
                                        // Don't currently handle combining multiple header instances in the common header case.
                                        // Nothing to do but punt them all to the NameValueCollection.
                                        NormalizeCommonHeaders();
                                        AddInternalNotCommon(s, sValue);
                                    }

                                    sName = s;
                                    break;
                                }
                            }
                        }
                    }

                    // If it wasn't a common header, add it to the hash.
                    if (sName == null)
                    {
                        sName = HeaderEncoding.GetString(byteBuffer + iBeginName, headerNameLength);
                        AddInternalNotCommon(sName, sValue);
                    }

                    totalResponseHeadersLength += i - unparsed;
                    unparsed = i;
                }
            }

quit:
            GlobalLog.Leave("WebHeaderCollection::ParseHeadersStrict() returning parseStatus:" + parseStatus.ToString());

            if (parseStatus == DataParseStatus.Invalid) {
                parseError.Section = WebParseErrorSection.ResponseHeader; 
                parseError.Code    = parseErrorCode;
            }

            return parseStatus;
        }

        //
        // Keeping this version for backwards compatibility (mostly with reflection).  Remove some day, along with the interface
        // explicit reimplementation.
        //
        /// <internalonly/>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            GetObjectData(serializationInfo, streamingContext);
        }

        // Override Get() to check the common headers.
        public override string Get(string name)
        {
            // In this case, need to make sure name doesn't have any Unicode since it's being used as an index into tables.
            if (m_CommonHeaders != null && name != null && name.Length > 0 && name[0] < 256)
            {
                int iHeader = s_CommonHeaderHints[name[0] & 0x1f];
                if (iHeader >= 0)
                {
                    while (true)
                    {
                        string s = s_CommonHeaderNames[iHeader++];

                        // Not found if we get to a shorter header or one with a different first character.
                        if (s.Length < name.Length || CaseInsensitiveAscii.AsciiToLower[name[0]] != CaseInsensitiveAscii.AsciiToLower[s[0]])
                            break;

                        // Keep looking if the common header is too long.
                        if (s.Length > name.Length)
                            continue;

                        int j;
                        for (j = 1; j < s.Length; j++)
                        {
                            // Avoid the case-insensitive compare in the common case where they match.
                            if (name[j] != s[j] && (name[j] > 255 || CaseInsensitiveAscii.AsciiToLower[name[j]] != CaseInsensitiveAscii.AsciiToLower[s[j]]))
                                break;
                        }
                        if (j == s.Length)
                        {
                            // Get the appropriate header.
                            return m_CommonHeaders[iHeader - 1];
                        }
                    }
                }
            }

            // Fall back to normal lookup.
            if (m_InnerCollection == null)
                return null;
            return m_InnerCollection.Get(name);
        }


        //
        // Additional overrides required to fully orphan the base implementation.
        //
        public override IEnumerator GetEnumerator()
        {
            NormalizeCommonHeaders();
            return new NameObjectKeysEnumerator(InnerCollection);
        }

        public override int Count
        {
            get
            {
                return (m_InnerCollection == null ? 0 : m_InnerCollection.Count) + m_NumCommonHeaders;
            }
        }

        public override KeysCollection Keys
        {
            get
            {
                NormalizeCommonHeaders();
                return InnerCollection.Keys;
            }
        }

        internal override bool InternalHasKeys()
        {
            NormalizeCommonHeaders();
            if (m_InnerCollection == null)
                return false;
            return m_InnerCollection.HasKeys();
        }

        public override string Get(int index)
        {
            NormalizeCommonHeaders();
            return InnerCollection.Get(index);
        }

        public override string[] GetValues(int index)
        {
            NormalizeCommonHeaders();
            return InnerCollection.GetValues(index);
        }

        public override string GetKey(int index)
        {
            NormalizeCommonHeaders();
            return InnerCollection.GetKey(index);
        }

        public override string[] AllKeys
        {
            get
            {
                NormalizeCommonHeaders();
                return InnerCollection.AllKeys;
            }
        }

        public override void Clear()
        {
            m_CommonHeaders = null;
            m_NumCommonHeaders = 0;
            InvalidateCachedArrays();
            if (m_InnerCollection != null)
                m_InnerCollection.Clear();
        }
    } // class WebHeaderCollection


    internal class CaseInsensitiveAscii : IEqualityComparer, IComparer{
        // ASCII char ToLower table
        internal static readonly CaseInsensitiveAscii StaticInstance = new CaseInsensitiveAscii();
        internal static readonly byte[] AsciiToLower = new byte[] {
              0,  1,  2,  3,  4,  5,  6,  7,  8,  9,
             10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
             20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
             30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
             40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
             50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
             60, 61, 62, 63, 64, 97, 98, 99,100,101, //  60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
            102,103,104,105,106,107,108,109,110,111, //  70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
            112,113,114,115,116,117,118,119,120,121, //  80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
            122, 91, 92, 93, 94, 95, 96, 97, 98, 99, //  90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
            100,101,102,103,104,105,106,107,108,109,
            110,111,112,113,114,115,116,117,118,119,
            120,121,122,123,124,125,126,127,128,129,
            130,131,132,133,134,135,136,137,138,139,
            140,141,142,143,144,145,146,147,148,149,
            150,151,152,153,154,155,156,157,158,159,
            160,161,162,163,164,165,166,167,168,169,
            170,171,172,173,174,175,176,177,178,179,
            180,181,182,183,184,185,186,187,188,189,
            190,191,192,193,194,195,196,197,198,199,
            200,201,202,203,204,205,206,207,208,209,
            210,211,212,213,214,215,216,217,218,219,
            220,221,222,223,224,225,226,227,228,229,
            230,231,232,233,234,235,236,237,238,239,
            240,241,242,243,244,245,246,247,248,249,
            250,251,252,253,254,255
        };

        // ASCII string case insensitive hash function
        public int GetHashCode(object myObject) {
            string myString = myObject as string;
            if (myObject == null) {
                return 0;
            }
            int myHashCode = myString.Length;
            if (myHashCode == 0) {
                return 0;
            }
            myHashCode ^= AsciiToLower[(byte)myString[0]]<<24 ^ AsciiToLower[(byte)myString[myHashCode-1]]<<16;
            return myHashCode;
        }

        // ASCII string case insensitive comparer
        public int Compare(object firstObject, object secondObject) {
            string firstString = firstObject as string;
            string secondString = secondObject as string;
            if (firstString==null) {
                return secondString == null ? 0 : -1;
            }
            if (secondString == null) {
                return 1;
            }
            int result = firstString.Length - secondString.Length;
            int comparisons = result > 0 ? secondString.Length : firstString.Length;
            int difference, index = 0;
            while ( index < comparisons ) {
                difference = (int)(AsciiToLower[ firstString[index] ] - AsciiToLower[ secondString[index] ]);
                if ( difference != 0 ) {
                    result = difference;
                    break;
                }
                index++;
            }
            return result;
        }

        // ASCII string case insensitive hash function
        int FastGetHashCode(string myString) {
            int myHashCode = myString.Length;
            if (myHashCode!=0) {
                myHashCode ^= AsciiToLower[(byte)myString[0]]<<24 ^ AsciiToLower[(byte)myString[myHashCode-1]]<<16;
            }
            return myHashCode;
        }

        // ASCII string case insensitive comparer
        public new bool Equals(object firstObject, object secondObject) {
            string firstString = firstObject as string;
            string secondString = secondObject as string;
            if (firstString==null) {
                return secondString==null;
            }
            if (secondString!=null) {
                int index = firstString.Length;
                if (index==secondString.Length) {
                    if (FastGetHashCode(firstString)==FastGetHashCode(secondString)) {
                        int comparisons = firstString.Length;
                        while (index>0) {
                            index--;
                            if (AsciiToLower[firstString[index]]!=AsciiToLower[secondString[index]]) {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
    
    internal class HostHeaderString {

        private bool m_Converted;
        private string m_String;
        private byte[] m_Bytes;

        internal HostHeaderString() {
            Init(null);
        }

        internal HostHeaderString(string s) {
            Init(s);
        }

        private void Init(string s) {
            m_String = s;
            m_Converted = false;
            m_Bytes = null;
        }

        private void Convert() {
            if (m_String != null && !m_Converted) {
                m_Bytes = Encoding.Default.GetBytes(m_String);
                string copy = Encoding.Default.GetString(m_Bytes);
                if (!(string.Compare(m_String, copy, StringComparison.Ordinal) == 0)) {
                    m_Bytes = Encoding.UTF8.GetBytes(m_String);
                }
            }
        }

        internal string String {
            get { return m_String; }
            set {
                Init(value);
            }
        }

        internal int ByteCount {
            get {
                Convert();
                return m_Bytes.Length;
            }
        }

        internal byte[] Bytes {
            get {
                Convert();
                return m_Bytes;
            }
        }

        internal void Copy(byte[] destBytes, int destByteIndex) {
            Convert();
            Array.Copy(m_Bytes, 0, destBytes, destByteIndex, m_Bytes.Length);
        }

    }
}
