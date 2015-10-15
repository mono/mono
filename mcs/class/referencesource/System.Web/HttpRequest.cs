//------------------------------------------------------------------------------
// <copyright file="HttpRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Request intrinsic
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Assemblies;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.SessionState;
    using System.Web.Util;

    // enumeration of dynamic server variables
    internal enum DynamicServerVariable {
        AUTH_TYPE = 1,
        AUTH_USER = 2,
        PATH_INFO = 3,
        PATH_TRANSLATED = 4,
        QUERY_STRING = 5,
        SCRIPT_NAME = 6
    };

    internal enum HttpVerb {
        Unparsed = 0,   // must be 0 so that it's zero-init value is Unparsed
        Unknown,
        GET,
        PUT,
        HEAD,
        POST,
        DEBUG,
        DELETE,
    }


    /// <devdoc>
    ///    <para>
    ///       Enables
    ///       type-safe browser to server communication. Used to gain access to HTTP request data
    ///       elements supplied by a client.
    ///    </para>
    /// </devdoc>
    public sealed class HttpRequest {
        // worker request
        [DoNotReset]
        private HttpWorkerRequest _wr;

        // context
        [DoNotReset]
        private HttpContext _context;

        // properties
        private String _httpMethod;
        private HttpVerb _httpVerb;
        private String _requestType;
        private VirtualPath _path;
        private String _rewrittenUrl;
        private bool   _computePathInfo;
        private VirtualPath _filePath;
        private VirtualPath _currentExecutionFilePath;
        private VirtualPath _pathInfo;
        private String _queryStringText;
        private bool   _queryStringOverriden;
        private byte[] _queryStringBytes;
        private String _pathTranslated;
        private String _contentType;
        private int    _contentLength = -1;
        private String _clientTarget;
        private String[] _acceptTypes;
        private String[] _userLanguages;
        private HttpBrowserCapabilities _browsercaps;
        private Uri _url;
        private Uri _referrer;
        private HttpInputStream _inputStream;
        private HttpClientCertificate _clientCertificate;
        private bool _tlsTokenBindingInfoResolved;
        private ITlsTokenBindingInfo _tlsTokenBindingInfo;
        private WindowsIdentity _logonUserIdentity;
        [DoNotReset]
        private RequestContext _requestContext;
        private string _rawUrl;
        private Stream _readEntityBodyStream;
        private ReadEntityBodyMode _readEntityBodyMode;

        // collections
        private UnvalidatedRequestValues _unvalidatedRequestValues;
        private HttpValueCollection _params;
        private HttpValueCollection _queryString;
        private HttpValueCollection _form;
        private HttpHeaderCollection _headers;
        private HttpServerVarsCollection _serverVariables;
        private HttpCookieCollection _cookies;
        [DoNotReset] // we can't reset this field when transitioning to WebSockets because it's our only remaining reference to the response cookies collection
        private HttpCookieCollection _storedResponseCookies;
        private HttpFileCollection _files;

        // content (to be read once)
        private HttpRawUploadedContent _rawContent;
        private bool _needToInsertEntityBody;
        private MultipartContentElement[] _multipartContentElements;

        // encoding (for content and query string)
        private Encoding _encoding;

        // content filtering
        private HttpInputStreamFilterSource _filterSource;
        private Stream _installedFilter;
        private bool _filterApplied;

        // Input validation
        #pragma warning disable 0649
        private SimpleBitVector32 _flags;
        #pragma warning restore 0649
        // const masks into the BitVector32
        private const int needToValidateQueryString     = 0x0001;
        private const int needToValidateForm            = 0x0002;
        private const int needToValidateCookies         = 0x0004;
        private const int needToValidateHeaders         = 0x0008;
        private const int needToValidateServerVariables = 0x0010;
        private const int contentEncodingResolved       = 0x0020;
        private const int needToValidatePostedFiles     = 0x0040;
        private const int needToValidateRawUrl          = 0x0080;
        private const int needToValidatePath            = 0x0100;
        private const int needToValidatePathInfo        = 0x0200;
        private const int hasValidateInputBeenCalled    = 0x8000;
        private const int needToValidateCookielessHeader = 0x10000;
        // True if granular request validation is enabled (validationmode >= 4.5); false if all collections validated eagerly.
        private const int granularValidationEnabled     = 0x40000000;
        // True if request validation is suppressed (validationMode == 0.0); false if validation can be enabled via a call to ValidateInput().
        private const int requestValidationSuppressed   = unchecked((int)0x80000000);


        // Browser caps one-time evaluator objects
        internal static object s_browserLock = new object();
        internal static bool s_browserCapsEvaled = false;

        /*
         * Internal constructor to create requests
         * that have associated HttpWorkerRequest
         *
         * @param wr HttpWorkerRequest
         */
        internal HttpRequest(HttpWorkerRequest wr, HttpContext context) {
            _wr = wr;
            _context = context;
        }

        /*
         * Public constructor for request that come from arbitrary place
         *
         * @param filename physical file name
         * @param queryString query string
         */

        /// <devdoc>
        ///    <para>
        ///       Initializes an HttpRequest object.
        ///    </para>
        /// </devdoc>
        public HttpRequest(String filename, String url, String queryString) {
            _wr = null;
            _pathTranslated = filename;
            _httpMethod = "GET";
            _url = new Uri(url);
            _path = VirtualPath.CreateAbsolute(_url.AbsolutePath);
            _queryStringText = queryString;
            _queryStringOverriden = true;
            _queryString = new HttpValueCollection(_queryStringText, true, true, Encoding.Default);

            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
        }

        internal HttpRequest(VirtualPath virtualPath, String queryString) {
            _wr = null;
            _pathTranslated = virtualPath.MapPath();
            _httpMethod = "GET";
            _url = new Uri("http://localhost" + virtualPath.VirtualPathString);
            _path = virtualPath;
            _queryStringText = queryString;
            _queryStringOverriden = true;
            _queryString = new HttpValueCollection(_queryStringText, true, true, Encoding.Default);

            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_EXECUTING);
        }

        internal bool NeedToInsertEntityBody {
            get { return _needToInsertEntityBody; }
            set { _needToInsertEntityBody = value; }
        }

        internal void SetRawContent(HttpRawUploadedContent rawContent) {
            Debug.Assert(rawContent != null);
            if (rawContent.Length > 0) {
                NeedToInsertEntityBody = true;
            }
            _rawContent = rawContent;
        }

        internal byte[] EntityBody { get { return NeedToInsertEntityBody ? _rawContent.GetAsByteArray() : null; } }

        internal string ClientTarget {
            get {
                return (_clientTarget == null) ? String.Empty : _clientTarget;
            }
            set {
                _clientTarget = value;
                // force re-create of browser caps
                _browsercaps = null;
            }
        }

        internal HttpContext Context {
            get { return _context; }
            set { _context = value; }
        }

        public RequestContext RequestContext {
            get {
                // Create an empty request context if we don't have one set
                if (_requestContext == null) {
                    HttpContext context = Context ?? HttpContext.Current;
                    _requestContext = new RequestContext(new HttpContextWrapper(context), new RouteData());
                }
                return _requestContext;
            }
            set {
                _requestContext = value;
            }
        }

        private bool HasTransitionedToWebSocketRequest {
            get {
                return (Context != null && Context.HasWebSocketRequestTransitionCompleted);
            }
        }

        /*
         * internal response object
         */
        internal HttpResponse Response {
            get {
                if (_context == null)
                    return null;
                return _context.Response;
            }
        }

        /*
         * Public property to determine if request is local
         */

        public bool IsLocal {
            get {
                if (_wr != null) {
                    return _wr.IsLocal();
                }
                else {
                    return false;
                }
            }
        }

        /*
         *  Cleanup code
         */
        internal void Dispose() {
            if (_serverVariables != null)
                _serverVariables.Dispose();  // disconnect from request

            if (_rawContent != null)
                _rawContent.Dispose();  // remove temp file with uploaded content

	    // 






        }

        //
        // Misc private methods to fill in collections from HttpWorkerRequest
        // properties
        //

        internal static String[] ParseMultivalueHeader(String s) {
            int l = (s != null) ? s.Length : 0;
            if (l == 0)
                return null;

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
            if (n == 0)
                return null;

            String[] strings = new String[n];
            values.CopyTo(0, strings, 0, n);
            return strings;
        }

        //
        // Query string collection support
        //

        private void FillInQueryStringCollection() {
            // try from raw bytes when available (better for globalization)

            byte[] rawQueryString = this.QueryStringBytes;

            if (rawQueryString != null) {
                if (rawQueryString.Length != 0)
                    _queryString.FillFromEncodedBytes(rawQueryString, QueryStringEncoding);
            }
            else if (!(String.IsNullOrEmpty(this.QueryStringText))) {
                _queryString.FillFromString(this.QueryStringText, true, QueryStringEncoding);
            }
        }

        //
        // Form collection support
        //

        private void FillInFormCollection() {
            if (_wr == null)
                return;

            if (!_wr.HasEntityBody())
                return;

            String contentType = this.ContentType;
            if (contentType == null)
                return;

            if (_readEntityBodyMode == ReadEntityBodyMode.Bufferless) {
                return;
            }

            if (StringUtil.StringStartsWithIgnoreCase(contentType, "application/x-www-form-urlencoded")) {
                // regular urlencoded form

                byte[] formBytes = null;
                HttpRawUploadedContent content = GetEntireRawContent();

                if (content != null)
                    formBytes = content.GetAsByteArray();

                if (formBytes != null) {
                    try {
                        _form.FillFromEncodedBytes(formBytes, ContentEncoding);
                    }
                    catch (Exception e) {
                        // could be thrown because of malformed data
                        throw new HttpException(SR.GetString(SR.Invalid_urlencoded_form_data), e);
                    }
                }
            }
            else if (StringUtil.StringStartsWithIgnoreCase(contentType, "multipart/form-data")) {
                // multipart form

                MultipartContentElement[] elements = GetMultipartContent();

                if (elements != null) {
                    for (int i = 0; i < elements.Length; i++) {
                        if (elements[i].IsFormItem) {
                            _form.ThrowIfMaxHttpCollectionKeysExceeded();
                            _form.Add(elements[i].Name, elements[i].GetAsString(ContentEncoding));
                        }
                    }
                }
            }
        }

        //
        // Headers collection support
        //

        private void FillInHeadersCollection() {
            if (_wr == null)
                return;

            // known headers

            for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++) {
                String h = _wr.GetKnownRequestHeader(i);

                if (!String.IsNullOrEmpty(h)) {
                    String name = HttpWorkerRequest.GetKnownRequestHeaderName(i);
                    _headers.SynchronizeHeader(name, h);
                }
            }

            // unknown headers

            String[][] hh = _wr.GetUnknownRequestHeaders();

            if (hh != null) {
                for (int i = 0; i < hh.Length; i++)
                    _headers.SynchronizeHeader(hh[i][0], hh[i][1]);
            }
        }

        //
        // Server variables collection support
        //

        private static String ServerVariableNameFromHeader(String header) {
            return("HTTP_" + header.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_'));
        }

        private String CombineAllHeaders(bool asRaw) {
            if (_wr == null)
                return String.Empty;

            StringBuilder sb = new StringBuilder(256);

            // known headers

            for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++) {
                String h = _wr.GetKnownRequestHeader(i);

                if (!String.IsNullOrEmpty(h)) {
                    String name;
                    if (!asRaw)
                        name = HttpWorkerRequest.GetServerVariableNameFromKnownRequestHeaderIndex(i);
                    else
                        name = HttpWorkerRequest.GetKnownRequestHeaderName(i);

                    if (name != null) {
                        sb.Append(name);
                        sb.Append(asRaw ? ": " : ":");  // for ASP compat don't add space
                        sb.Append(h);
                        sb.Append("\r\n");
                    }
                }
            }

            // unknown headers

            String[][] hh = _wr.GetUnknownRequestHeaders();

            if (hh != null) {
                for (int i = 0; i < hh.Length; i++) {
                    String name = hh[i][0];

                    if (!asRaw)
                        name = ServerVariableNameFromHeader(name);

                    sb.Append(name);
                    sb.Append(asRaw ? ": " : ":");  // for ASP compat don't add space
                    sb.Append(hh[i][1]);
                    sb.Append("\r\n");
                }
            }

            return sb.ToString();
        }

        // callback to calculate dynamic server variable
        internal String CalcDynamicServerVariable(DynamicServerVariable var) {
            String value = null;

            switch (var) {
                case DynamicServerVariable.AUTH_TYPE:
                    if (_context.User != null && _context.User.Identity.IsAuthenticated)
                        value = _context.User.Identity.AuthenticationType;
                    else
                        value = String.Empty;
                    break;
                case DynamicServerVariable.AUTH_USER:
                    if (_context.User != null && _context.User.Identity.IsAuthenticated)
                        value = _context.User.Identity.Name;
                    else
                        value = String.Empty;
                    break;
                case DynamicServerVariable.PATH_INFO:
                    value = this.Path;
                    break;
                case DynamicServerVariable.PATH_TRANSLATED:
                    value = this.PhysicalPathInternal;
                    break;
                case DynamicServerVariable.QUERY_STRING:
                    value = this.QueryStringText;
                    break;
                case DynamicServerVariable.SCRIPT_NAME:
                    value = this.FilePath;
                    break;
            }

            return value;
        }

        private void AddServerVariableToCollection(String name, DynamicServerVariable var) {
            // dynamic server var
            _serverVariables.AddDynamic(name, var);
        }

        private void AddServerVariableToCollection(String name, String value) {
            if (value == null)
                value = String.Empty;
            // static server var
            _serverVariables.AddStatic(name, value);
        }

        private void AddServerVariableToCollection(String name) {
            // static server var from worker request
            _serverVariables.AddStatic(name, _wr.GetServerVariable(name));
        }

        internal void FillInServerVariablesCollection() {
            if (_wr == null)
                return;

            //  Add from hardcoded list

            AddServerVariableToCollection("ALL_HTTP",           CombineAllHeaders(false));
            AddServerVariableToCollection("ALL_RAW",            CombineAllHeaders(true));

            AddServerVariableToCollection("APPL_MD_PATH");

            AddServerVariableToCollection("APPL_PHYSICAL_PATH", _wr.GetAppPathTranslated());

            AddServerVariableToCollection("AUTH_TYPE",          DynamicServerVariable.AUTH_TYPE);
            AddServerVariableToCollection("AUTH_USER",          DynamicServerVariable.AUTH_USER);

            AddServerVariableToCollection("AUTH_PASSWORD");

            AddServerVariableToCollection("LOGON_USER");
            AddServerVariableToCollection("REMOTE_USER",        DynamicServerVariable.AUTH_USER);

            AddServerVariableToCollection("CERT_COOKIE");
            AddServerVariableToCollection("CERT_FLAGS");
            AddServerVariableToCollection("CERT_ISSUER");
            AddServerVariableToCollection("CERT_KEYSIZE");
            AddServerVariableToCollection("CERT_SECRETKEYSIZE");
            AddServerVariableToCollection("CERT_SERIALNUMBER");
            AddServerVariableToCollection("CERT_SERVER_ISSUER");
            AddServerVariableToCollection("CERT_SERVER_SUBJECT");
            AddServerVariableToCollection("CERT_SUBJECT");

            String clString = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);
            AddServerVariableToCollection("CONTENT_LENGTH",     (clString != null) ? clString : "0");

            AddServerVariableToCollection("CONTENT_TYPE",       this.ContentType);

            AddServerVariableToCollection("GATEWAY_INTERFACE");

            AddServerVariableToCollection("HTTPS");
            AddServerVariableToCollection("HTTPS_KEYSIZE");
            AddServerVariableToCollection("HTTPS_SECRETKEYSIZE");
            AddServerVariableToCollection("HTTPS_SERVER_ISSUER");
            AddServerVariableToCollection("HTTPS_SERVER_SUBJECT");

            AddServerVariableToCollection("INSTANCE_ID");
            AddServerVariableToCollection("INSTANCE_META_PATH");

            AddServerVariableToCollection("LOCAL_ADDR",         _wr.GetLocalAddress());

            AddServerVariableToCollection("PATH_INFO",          DynamicServerVariable.PATH_INFO);
            AddServerVariableToCollection("PATH_TRANSLATED",    DynamicServerVariable.PATH_TRANSLATED);

            AddServerVariableToCollection("QUERY_STRING",       DynamicServerVariable.QUERY_STRING);

            AddServerVariableToCollection("REMOTE_ADDR",        this.UserHostAddress);
            AddServerVariableToCollection("REMOTE_HOST",        this.UserHostName);

            AddServerVariableToCollection("REMOTE_PORT");

            AddServerVariableToCollection("REQUEST_METHOD",     this.HttpMethod);

            AddServerVariableToCollection("SCRIPT_NAME",        DynamicServerVariable.SCRIPT_NAME);

            AddServerVariableToCollection("SERVER_NAME",        _wr.GetServerName());
            AddServerVariableToCollection("SERVER_PORT",        _wr.GetLocalPortAsString());

            AddServerVariableToCollection("SERVER_PORT_SECURE", _wr.IsSecure() ? "1" : "0");

            AddServerVariableToCollection("SERVER_PROTOCOL",    _wr.GetHttpVersion());
            AddServerVariableToCollection("SERVER_SOFTWARE");

            AddServerVariableToCollection("URL",                DynamicServerVariable.SCRIPT_NAME);

            // Add all headers in HTTP_XXX format

            for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++) {
                String h = _wr.GetKnownRequestHeader(i);
                if (!String.IsNullOrEmpty(h))
                    AddServerVariableToCollection(HttpWorkerRequest.GetServerVariableNameFromKnownRequestHeaderIndex(i), h);
            }

            String[][] hh = _wr.GetUnknownRequestHeaders();

            if (hh != null) {
                for (int i = 0; i < hh.Length; i++)
                    AddServerVariableToCollection(ServerVariableNameFromHeader(hh[i][0]), hh[i][1]);
            }
        }

        //
        // Cookies collection support
        //

        internal static HttpCookie CreateCookieFromString(String s) {
            HttpCookie c = new HttpCookie();

            int l = (s != null) ? s.Length : 0;
            int i = 0;
            int ai, ei;
            bool firstValue = true;
            int numValues = 1;

            // Format: cookiename[=key1=val2&key2=val2&...]

            while (i < l) {
                //  find next &
                ai = s.IndexOf('&', i);
                if (ai < 0)
                    ai = l;

                // first value might contain cookie name before =
                if (firstValue) {
                    ei = s.IndexOf('=', i);

                    if (ei >= 0 && ei < ai) {
                        c.Name = s.Substring(i, ei-i);
                        i = ei+1;
                    }
                    else if (ai == l) {
                        // the whole cookie is just a name
                        c.Name = s;
                        break;
                    }

                    firstValue = false;
                }

                // find '='
                ei = s.IndexOf('=', i);

                if (ei < 0 && ai == l && numValues == 0) {
                    // simple cookie with simple value
                    c.Value = s.Substring(i, l-i);
                }
                else if (ei >= 0 && ei < ai) {
                    // key=value
                    c.Values.Add(s.Substring(i, ei-i), s.Substring(ei+1, ai-ei-1));
                    numValues++;
                }
                else {
                    // value without key
                    c.Values.Add(null, s.Substring(i, ai-i));
                    numValues++;
                }

                i = ai+1;
            }

            return c;
        }

        internal void FillInCookiesCollection(HttpCookieCollection cookieCollection, bool includeResponse) {
            if (_wr == null)
                return;

            String s = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderCookie);

            // Parse the cookie server variable.
            // Format: c1=k1=v1&k2=v2; c2=...

            int l = (s != null) ? s.Length : 0;
            int i = 0;
            int j;
            char ch;

            HttpCookie lastCookie = null;

            while (i < l) {
                // find next ';' (don't look to ',' as per 91884)
                j = i;
                while (j < l) {
                    ch = s[j];
                    if (ch == ';')
                        break;
                    j++;
                }

                // create cookie form string
                String cookieString = s.Substring(i, j-i).Trim();
                i = j+1; // next cookie start

                if (cookieString.Length == 0)
                    continue;

                HttpCookie cookie = CreateCookieFromString(cookieString);

                // some cookies starting with '$' are really attributes of the last cookie
                if (lastCookie != null) {
                    String name = cookie.Name;

                    // add known attribute to the last cookie (if any)
                    if (name != null && name.Length > 0 && name[0] == '$') {
                        if (StringUtil.EqualsIgnoreCase(name, "$Path"))
                            lastCookie.Path = cookie.Value;
                        else if (StringUtil.EqualsIgnoreCase(name, "$Domain"))
                            lastCookie.Domain = cookie.Value;

                        continue;
                    }
                }

                // regular cookie
                cookieCollection.AddCookie(cookie, true);
                lastCookie = cookie;

                // goto next cookie
            }

            // Append response cookies
            if (includeResponse) {
                // If we have a reference to the response cookies collection, use it directly
                // rather than going through the Response object (which might not be available, e.g.
                // if we have already transitioned to a WebSockets request).
                HttpCookieCollection storedResponseCookies = _storedResponseCookies;
                if (storedResponseCookies == null && !HasTransitionedToWebSocketRequest && Response != null) {
                    storedResponseCookies = Response.GetCookiesNoCreate();
                }

                if (storedResponseCookies != null && storedResponseCookies.Count > 0) {
                    HttpCookie[] responseCookieArray = new HttpCookie[storedResponseCookies.Count];
                    storedResponseCookies.CopyTo(responseCookieArray, 0);
                    for (int iCookie = 0; iCookie < responseCookieArray.Length; iCookie++)
                        cookieCollection.AddCookie(responseCookieArray[iCookie], append: true);
                }

                // release any stored reference to the response cookie collection
                _storedResponseCookies = null;
            }
        }

        internal void StoreReferenceToResponseCookies(HttpCookieCollection responseCookies) {
            _storedResponseCookies = responseCookies;
        }

        // Params collection support
        private void FillInParamsCollection() {
            _params.Add(this.QueryString);
            _params.Add(this.Form);
            _params.Add(this.Cookies);
            _params.Add(this.ServerVariables);
        }

        //
        // Files collection support
        //

        private void FillInFilesCollection() {
            if (_wr == null)
                return;

            if (!StringUtil.StringStartsWithIgnoreCase(ContentType, "multipart/form-data"))
                return;

            MultipartContentElement[] elements = GetMultipartContent();
            if (elements == null)
                return;

            for (int i = 0; i < elements.Length; i++) {
                if (elements[i].IsFile) {
                    HttpPostedFile p = elements[i].GetAsPostedFile();
                    _files.AddFile(elements[i].Name, p);
                }
            }
        }

        //
        // Reading posted content ...
        //

        /*
         * Get attribute off header value
         */
        private static String GetAttributeFromHeader(String headerValue, String attrName) {
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

        /*
         * In case content-type header contains encoding it should override the config
         */
        private Encoding GetEncodingFromHeaders() {

            if (UserAgent != null && CultureInfo.InvariantCulture.CompareInfo.IsPrefix(UserAgent, "UP")) {
                String postDataCharset = Headers["x-up-devcap-post-charset"];
                if (!String.IsNullOrEmpty(postDataCharset)) {
                    try {
                        return Encoding.GetEncoding(postDataCharset);
                    }
                    catch {
                        // Exception may be thrown when charset is not valid.
                        // In this case, do nothing, and let the framework
                        // use the configured RequestEncoding setting.
                    }
                }
            }


            if (!_wr.HasEntityBody())
                return null;

            String contentType = this.ContentType;
            if (contentType == null)
                return null;

            String charSet = GetAttributeFromHeader(contentType, "charset");
            if (charSet == null)
                return null;

            Encoding encoding = null;

            try {
                encoding = Encoding.GetEncoding(charSet);
            }
            catch {
                // bad encoding string throws an exception that needs to be consumed
            }

            return encoding;
        }

        /*
         * Read entire raw content as byte array
         */
        private HttpRawUploadedContent GetEntireRawContent() {
            if (_wr == null)
                return null;

            if (_rawContent != null) {
                // if _rawContent was set by HttpBufferlessInputStream, then we will apply the filter here
                if (_installedFilter != null && !_filterApplied) {
                    ApplyFilter(ref _rawContent, RuntimeConfig.GetConfig(_context).HttpRuntime.RequestLengthDiskThresholdBytes);
                }
                return _rawContent;
            }

            if (_readEntityBodyMode == ReadEntityBodyMode.None) {
                _readEntityBodyMode = ReadEntityBodyMode.Classic;
            }
            else if (_readEntityBodyMode == ReadEntityBodyMode.Buffered) {
                // _rawContent should have been set already
                throw new InvalidOperationException(SR.GetString(SR.Invalid_operation_with_get_buffered_input_stream));
            }
            else if (_readEntityBodyMode == ReadEntityBodyMode.Bufferless) {
                throw new HttpException(SR.GetString(SR.Incompatible_with_get_bufferless_input_stream));
            }

            // enforce the limit
            HttpRuntimeSection cfg = RuntimeConfig.GetConfig(_context).HttpRuntime;
            int limit = cfg.MaxRequestLengthBytes;
            if (ContentLength > limit) {
                if ( !(_wr is IIS7WorkerRequest) ) {
                    Response.CloseConnectionAfterError();
                }
                throw new HttpException(SR.GetString(SR.Max_request_length_exceeded),
                                        null, WebEventCodes.RuntimeErrorPostTooLarge);
            }

            // threshold to go to file

            int fileThreshold = cfg.RequestLengthDiskThresholdBytes;

            // read the preloaded content

            HttpRawUploadedContent rawContent = new HttpRawUploadedContent(fileThreshold, ContentLength);

            byte[] preloadedContent = _wr.GetPreloadedEntityBody();

            if (preloadedContent != null) {
                _wr.UpdateRequestCounters(preloadedContent.Length);
                rawContent.AddBytes(preloadedContent, 0, preloadedContent.Length);
            }

            // read the remaing content

            if (!_wr.IsEntireEntityBodyIsPreloaded()) {
                int remainingBytes = (ContentLength > 0) ? ContentLength - rawContent.Length : Int32.MaxValue;

                HttpApplication app = _context.ApplicationInstance;
                byte[] buf = (app != null) ? app.EntityBuffer : new byte[8 * 1024];
                int numBytesRead = rawContent.Length;

                while (remainingBytes > 0) {
                    int bytesToRead = buf.Length;
                    if (bytesToRead > remainingBytes)
                        bytesToRead = remainingBytes;

                    int bytesRead = _wr.ReadEntityBody(buf, bytesToRead);
                    if (bytesRead <= 0)
                        break;

                    _wr.UpdateRequestCounters(bytesRead);

                    rawContent.AddBytes(buf, 0, bytesRead);

                    remainingBytes -= bytesRead;
                    numBytesRead += bytesRead;

                    if (numBytesRead > limit) {
                        throw new HttpException(SR.GetString(SR.Max_request_length_exceeded),
                                    null, WebEventCodes.RuntimeErrorPostTooLarge);
                    }

                    // Fail synchrously if receiving the request content takes too long
                    // RequestTimeoutManager is not efficient in case of ThreadPool starvation
                    // as the timer callback doing Thread.Abort may not trigger for a long time
                    if (remainingBytes > 0 && _context.HasTimeoutExpired) {
                        throw new HttpException(SR.GetString(SR.Request_timed_out));
                    }
                }
            }

            rawContent.DoneAddingBytes();

            // filter content
            if (_installedFilter != null) {
                ApplyFilter(ref rawContent, fileThreshold);
            }

            SetRawContent(rawContent);
            return _rawContent;
        }

        private void ApplyFilter(ref HttpRawUploadedContent rawContent, int fileThreshold) {
            if (_installedFilter != null) {
                _filterApplied = true;
                if (rawContent.Length > 0) {
                    try {
                        try {
                            _filterSource.SetContent(rawContent);
                            
                            HttpRawUploadedContent filteredRawContent = new HttpRawUploadedContent(fileThreshold, rawContent.Length);
                            HttpApplication app = _context.ApplicationInstance;
                            byte[] buf = (app != null) ? app.EntityBuffer : new byte[8 * 1024];
                            
                            for (;;) {
                                int bytesRead = _installedFilter.Read(buf, 0, buf.Length);
                                if (bytesRead == 0)
                                    break;
                                filteredRawContent.AddBytes(buf, 0, bytesRead);
                            }
                            
                            filteredRawContent.DoneAddingBytes();
                            rawContent = filteredRawContent;
                        }
                        finally {
                            _filterSource.SetContent(null);
                        }
                    }
                    catch { // Protect against exception filters
                        throw;
                    }
                }
            }
        }

        /*
         * Get multipart posted content as array of elements
         */
        private MultipartContentElement[] GetMultipartContent() {
            // already parsed
            if (_multipartContentElements != null)
                return _multipartContentElements;

            // check the boundary
            byte[] boundary = GetMultipartBoundary();
            if (boundary == null)
                return new MultipartContentElement[0];

            // read the content if not read already
            HttpRawUploadedContent content = GetEntireRawContent();
            if (content == null)
                return new MultipartContentElement[0];

            // do the parsing
            _multipartContentElements = HttpMultipartContentTemplateParser.Parse(content, content.Length, boundary, ContentEncoding);
            return _multipartContentElements;
        }

        /*
         * Get boundary for the posted multipart content as byte array
         */

        private byte[] GetMultipartBoundary() {
            // extract boundary value
            String b = GetAttributeFromHeader(ContentType, "boundary");
            if (b == null)
                return null;

            // prepend with "--" and convert to byte array
            b = "--" + b;
            return Encoding.ASCII.GetBytes(b.ToCharArray());
        }

        //
        // Request cookies sometimes are populated from Response
        // Here are helper methods to do that.
        //

        /*
         * Add response cookie to request collection (can override existing)
         */
        internal void AddResponseCookie(HttpCookie cookie) {
            // cookies collection

            if (_cookies != null)
                _cookies.AddCookie(cookie, true);

            // cookies also go to parameters collection

            if (_params != null) {
                _params.MakeReadWrite();
                _params.Add(cookie.Name, cookie.Value);
                _params.MakeReadOnly();
            }
        }

        /*
         * Clear any cookies response might've added
         */
        internal void ResetCookies() {
            // cookies collection

            if (_cookies != null) {
                _cookies.Reset();
                FillInCookiesCollection(_cookies, true /*includeResponse*/);
            }

            // cookies also go to parameters collection

            if (_params != null) {
                _params.MakeReadWrite();
                _params.Reset();
                FillInParamsCollection();
                _params.MakeReadOnly();
            }
        }

        /*
         * Http method (verb) associated with the current request
         */

        /// <devdoc>
        ///    <para>Indicates the HTTP data transfer method used by client (GET, POST). This property is read-only.</para>
        /// </devdoc>
        public String HttpMethod {
            get {
                // Directly from worker request
                if (_httpMethod == null) {
                    Debug.Assert(_wr != null);
                    _httpMethod = _wr.GetHttpVerbName();
                }

                return _httpMethod;
            }
        }

        internal HttpVerb HttpVerb {
            get {
                if (_httpVerb == HttpVerb.Unparsed) {
                    _httpVerb = HttpVerb.Unknown;
                    string method = HttpMethod;
                    if (method != null) {
                        switch (method.Length) {
                            case 3:
                                if (method == "GET") {
                                    _httpVerb = HttpVerb.GET;
                                }
                                else if (method == "PUT") {
                                    _httpVerb = HttpVerb.PUT;
                                }
                                break;

                            case 4:
                                if (method == "POST") {
                                    _httpVerb = HttpVerb.POST;
                                }
                                else if (method == "HEAD") {
                                    _httpVerb = HttpVerb.HEAD;
                                }
                                break;

                            case 5:
                                if (method == "DEBUG") {
                                    _httpVerb = HttpVerb.DEBUG;
                                }
                                break;

                            case 6:
                                if (method == "DELETE") {
                                    _httpVerb = HttpVerb.DELETE;
                                }
                                break;
                        }
                    }
                }

                return _httpVerb;
            }
        }

        // Check whether this is a DEBUG verb request
        internal bool IsDebuggingRequest {
            get {
                return (HttpVerb == HttpVerb.DEBUG);
            }
        }

        /*
         * RequestType default to verb, but can be changed
         */

        /// <devdoc>
        ///    Indicates the HTTP data transfer method used by client
        ///    (GET, POST).
        /// </devdoc>
        public String RequestType {
            get {
                return(_requestType != null) ? _requestType : this.HttpMethod;
            }

            set {
                _requestType = value;
            }
        }

        /*
          * Content-type of the content posted with the current request
          */

        /// <devdoc>
        ///    <para>Indicates the MIME content type of incoming request. This property is read-only.</para>
        /// </devdoc>
        public String ContentType {
            get {
                if (_contentType == null) {
                    if (_wr != null)
                        _contentType = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);

                    if (_contentType == null)
                        _contentType = String.Empty;
                }

                return _contentType;
            }

            set {
                _contentType = value;
            }
        }



        /// <devdoc>
        ///    <para>Indicates the content length of incoming request. This property is read-only.</para>
        /// </devdoc>
        public int ContentLength {
            get {
                if (_contentLength == -1) {
                    if (_wr != null) {
                        String s = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);

                        if (s != null) {
                            try {
                                _contentLength = Int32.Parse(s, CultureInfo.InvariantCulture);
                            }
                            catch {
                            }
                        }
                        else {
                            // no content-length header, but there is data
                            if (_wr.IsEntireEntityBodyIsPreloaded()) {
                                byte[] preloadedContent = _wr.GetPreloadedEntityBody();

                                if (preloadedContent != null)
                                    _contentLength = preloadedContent.Length;
                            }
                        }
                    }
                }

                return (_contentLength >= 0) ? _contentLength : 0;
            }
        }

        /*
         * Encoding to read posted text content
         */

        /// <devdoc>
        ///    <para>Indicates the character set of data supplied by client. This property is read-only.</para>
        /// </devdoc>
        public Encoding ContentEncoding {
            get {
                if(_flags[contentEncodingResolved] && _encoding != null) {
                    return _encoding;
                }

                _encoding = GetEncodingFromHeaders();

                // DevDiv #351560 - UTF-7 is dangerous and should be forbidden by default.
                // The application developer can choose to allow it if desired.
                if (_encoding is UTF7Encoding && !AppSettings.AllowUtf7RequestContentEncoding) {
                    _encoding = null;
                }

                if (_encoding == null) {
                    // WOS 1953542: No Event Is Logged When App Config is Corrupt
                    GlobalizationSection globConfig = RuntimeConfig.GetLKGConfig(_context).Globalization;
                    _encoding = globConfig.RequestEncoding;
                }

                _flags.Set(contentEncodingResolved);
                return _encoding;
            }

            set {
                _encoding = value;
                _flags.Set(contentEncodingResolved);
            }
        }

        internal Encoding QueryStringEncoding {
            get {
                Encoding e = ContentEncoding;
                // query string is never unicode - use utf-8 if instead
                return e.Equals(Encoding.Unicode) ? Encoding.UTF8 : e;
            }
        }

        /*
         * Parsed Accept header as array of strings
         */

        /// <devdoc>
        ///    <para>Returns a string array of client-supported MIME accept types. This property is read-only.</para>
        /// </devdoc>
        public String[] AcceptTypes {
            get {
                if (_acceptTypes == null) {
                    if (_wr != null)
                        _acceptTypes = ParseMultivalueHeader(_wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderAccept));
                }

                return _acceptTypes;
            }
        }

        // Is the request authenticated?
        public bool IsAuthenticated {
            get {
                return(_context.User != null && _context.User.Identity != null && _context.User.Identity.IsAuthenticated);
            }
        }

        // Is using HTTPS?
        //    Indicates whether the HTTP connection is secure (that is, HTTPS). This property is read-only.
        public bool IsSecureConnection {
            get {
                if (_wr != null)
                    return _wr.IsSecure();
                else
                    return false;
            }
        }


        /*
         * Virtual path corresponding to the requested Url
         */

        /// <devdoc>
        ///    <para>Indicates the virtual path of the current
        ///       request, including the path PathInfo. This property is read-only.</para>
        /// </devdoc>
        public String Path {
            get {
                string path = GetUnvalidatedPath();
                if (_flags[needToValidatePath]) {
                    _flags.Clear(needToValidatePath);
                    ValidateString(path, null, RequestValidationSource.Path);
                }
                return path;
            }
        }

        internal VirtualPath PathObject {
            get {
                if (_path == null) {
                    // Directly from worker request

                    Debug.Assert(_wr != null);

                    // Don't allow malformed paths for security reasons
                    _path = VirtualPath.Create(_wr.GetUriPath(),
                        VirtualPathOptions.AllowAbsolutePath);
                }

                return _path;
            }
        }

        // Gets the Path property but does not hook up validation.
        internal string GetUnvalidatedPath() {
            return PathObject.VirtualPathString;
        }

        [DoNotReset]
        private string _anonymousId;

        public string AnonymousID {
            get { return _anonymousId; }
            internal set { _anonymousId = value; }
        }

        internal String PathWithQueryString {
            get {
                String qs = QueryStringText;
                return (!String.IsNullOrEmpty(qs)) ? (Path + "?" + qs) : Path;
            }
        }

        // The virtual file path where the client browsers think we are.
        // However, in the case of cookieless session, ClientFilePath does *not* include the session id.
        private VirtualPath _clientFilePath;
        internal VirtualPath ClientFilePath {
            get {
                if (_clientFilePath == null) {
                    string uri = RawUrl;

                    // remove query string if it exists
                    int qsIndex = uri.IndexOf('?');
                    if (qsIndex > -1) {
                        uri = uri.Substring(0, qsIndex);
                    }
                    _clientFilePath = VirtualPath.Create(uri, VirtualPathOptions.AllowAbsolutePath);
                }

                Debug.Trace("ClientUrl", "*** ClientFilePath --> " + _clientFilePath + " ***");
                return _clientFilePath;
            }
            set {
                _clientFilePath = value;
            }
        }

        // The base dir of the client virtual file path.
        // However, in the case of cookieless session, ClientFilePath does *not* include the session id.
        private VirtualPath _clientBaseDir;

        // VSWhidbey 560283 : The ClientBaseDir represents the directory of the current
        // request to the client.
        //
        // the request is   ClientBaseDir   FilePathObject (FilePathObject.Parent)
        // 1. /app/sub/     /app/sub/       /app/
        // 2. /app/sub      /app/           /app/
        internal VirtualPath ClientBaseDir {
            get {
                if (_clientBaseDir == null) {
                    // client virtual path before the last '/'
                    if (ClientFilePath.HasTrailingSlash) {
                        _clientBaseDir = ClientFilePath;
                    }
                    else {
                        _clientBaseDir = ClientFilePath.Parent;
                    }
                }

                return _clientBaseDir;
            }
        }

        /*
         * File path corresponding to the requested Url
         */

        /// <devdoc>
        ///    <para>Indicates the virtual path of the current request, but without the PathInfo.
        ///         This property is read-only.</para>
        /// </devdoc>
        public String FilePath {
            get {
                return VirtualPath.GetVirtualPathString(FilePathObject);
            }
        }

        internal VirtualPath FilePathObject {
            get {
                if (_filePath != null) {
                    return _filePath;
                }

                if (!_computePathInfo) {
                    // Directly from worker request

                    if (_wr != null) {
                        _filePath = _wr.GetFilePathObject();
                    }
                    else {
                        _filePath = PathObject;
                    }
                }
                else if (_context != null) {
                    // From config
                    //
                    //          RAID#93378
                    //          Config system relies on FilePath for lookups so we should not
                    //          be calling it while _filePath is null or it will lead to
                    //          infinite recursion.
                    //
                    //          It is safe to set _filePath to Path as longer path would still
                    //          yield correct configuration, just a little slower.

                    _filePath = PathObject;

                    int filePathLen = _context.GetFilePathData().Path.VirtualPathStringNoTrailingSlash.Length;

                    // case could be wrong in config (_path has the correct case)
                    string path = Path;
                    int pathLength = path.Length;
                    // If path is extensionless, _filePath should be equal to path--trailing slash should not be removed.
                    if (pathLength != filePathLen
                        && (pathLength - filePathLen != 1
                            || path[pathLength-1] != '/'
                            || path.IndexOf('.') > -1)
                        )
                        _filePath = VirtualPath.CreateAbsolute(Path.Substring(0, filePathLen));
                }

                return _filePath;
            }
        }

        /*
         * Normally the same as ClientFilePath.  The difference is that when doing a
         * Server.Execute, ClientFilePath doesn't change, while this changes to the
         * currently executing virtual path
         */

        public string CurrentExecutionFilePath {
            get {
                return CurrentExecutionFilePathObject.VirtualPathString;
            }
        }

        public string CurrentExecutionFilePathExtension {
            get {
                return UrlPath.GetExtension(CurrentExecutionFilePathObject.VirtualPathString);
            }
        }

        internal VirtualPath CurrentExecutionFilePathObject {
            get {
                if (_currentExecutionFilePath != null)
                    return _currentExecutionFilePath;

                return FilePathObject;
            }
        }

        internal VirtualPath SwitchCurrentExecutionFilePath(VirtualPath path) {
            VirtualPath oldPath = _currentExecutionFilePath;
            _currentExecutionFilePath = path;
            return oldPath;
        }


        // Same as CurrentExecutionFilePath, but made relative to the application root,
        // so it is application-agnostic.
        public string AppRelativeCurrentExecutionFilePath {
            get {
                return UrlPath.MakeVirtualPathAppRelative(CurrentExecutionFilePath);
            }
        }

        // Path-info corresponding to the requested Url
        //    Indicates additional path information for a resource with a URL extension. i.e. for the URL
        //       /virdir/page.html/tail, the PathInfo value is /tail. This property is read-only.</para>
        public String PathInfo {
            get {
                string pathInfo = GetUnvalidatedPathInfo();
                if (_flags[needToValidatePathInfo]) {
                    _flags.Clear(needToValidatePathInfo);
                    ValidateString(pathInfo, null, RequestValidationSource.PathInfo);
                }
                return pathInfo;
            }
        }

        internal VirtualPath PathInfoObject {
            get {
                if (_pathInfo != null) {
                    return _pathInfo;
                }

                if (!_computePathInfo) {
                    // Directly from worker request

                    if (_wr != null) {
                        _pathInfo = VirtualPath.CreateAbsoluteAllowNull(_wr.GetPathInfo());
                    }
                }

                if (_pathInfo == null && _context != null) {
                    VirtualPath path = PathObject;
                    int pathLength = path.VirtualPathString.Length;
                    VirtualPath filePath = FilePathObject;
                    int filePathLength = filePath.VirtualPathString.Length;

                    if (filePath == null)
                        _pathInfo = path;
                    else if (path == null || pathLength <= filePathLength )
                        _pathInfo = null;
                    else {
                        string pathInfoString = path.VirtualPathString.Substring(filePathLength, pathLength - filePathLength);
                        _pathInfo = VirtualPath.CreateAbsolute(pathInfoString);
                    }
                }

                return _pathInfo;
            }
        }

        // Gets the PathInfo property but does not hook up validation.
        internal string GetUnvalidatedPathInfo() {
            VirtualPath pathInfoObject = PathInfoObject;
            return (pathInfoObject == null) ? String.Empty : pathInfoObject.VirtualPathString;
        }

        /*
         * Physical path corresponding to the requested Url
         */

        /// <devdoc>
        ///    <para>Gets the physical file system path corresponding
        ///       to
        ///       the requested URL. This property is read-only.</para>
        /// </devdoc>
        public String PhysicalPath {
            get {
                String path = PhysicalPathInternal;
                InternalSecurityPermissions.PathDiscovery(path).Demand();
                return path;
            }
        }

        internal String PhysicalPathInternal {
            get {
                if (_pathTranslated == null) {
                    if (!_computePathInfo) {
                        // Directly from worker request
                        Debug.Assert(_wr != null);
                        _pathTranslated = _wr.GetFilePathTranslated();
                        if (HttpRuntime.IsMapPathRelaxed)
                            _pathTranslated = HttpRuntime.GetRelaxedMapPathResult(_pathTranslated);
                    }

                    if (_pathTranslated == null && _wr != null) {
                        // Compute after rewrite
                        _pathTranslated = HostingEnvironment.MapPathInternal(FilePath);
                    }
                }

                return _pathTranslated;
            }
        }

        /*
         * Virtual path to the application root
         */

        /// <devdoc>
        ///    <para>Gets the
        ///       virtual path to the currently executing server application.</para>
        /// </devdoc>
        public String ApplicationPath {
            get {
                return HttpRuntime.AppDomainAppVirtualPath;
            }
        }

        internal VirtualPath ApplicationPathObject {
            get {
                return HttpRuntime.AppDomainAppVirtualPathObject;
            }
        }

        /*
         * Physical path to the application root
         */

        /// <devdoc>
        ///    <para>Gets the physical
        ///       file system path of currently executing server application.</para>
        /// </devdoc>
        public String PhysicalApplicationPath {
            get {
                InternalSecurityPermissions.AppPathDiscovery.Demand();

                if (_wr != null)
                    return _wr.GetAppPathTranslated();
                else
                    return null;
            }
        }

        /*
         * User agent string
         */

        /// <devdoc>
        ///    <para>Gets the client
        ///       browser's raw User Agent String.</para>
        /// </devdoc>
        public String UserAgent {
            get {
                if (_wr != null)
                    return _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderUserAgent);
                else
                    return null;
            }
        }

        /*
         * Accepted user languages
         */

        /// <devdoc>
        ///    <para>Gets a
        ///       sorted array of client language preferences.</para>
        /// </devdoc>
        public String[] UserLanguages {
            get {
                if (_userLanguages == null) {
                    if (_wr != null)
                        _userLanguages = ParseMultivalueHeader(_wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderAcceptLanguage));
                }

                return _userLanguages;
            }
        }

        // Browser caps
        //    Provides information about incoming client's browser capabilities.
        public HttpBrowserCapabilities Browser {
            get {
                if(_browsercaps != null) {
                    return _browsercaps;
                }

                if (! s_browserCapsEvaled) {
                    lock (s_browserLock) {
                        if (! s_browserCapsEvaled) {
                            HttpCapabilitiesBase.GetBrowserCapabilities(this);
                        }
                        s_browserCapsEvaled = true;
                    }
                }

                _browsercaps = (HttpBrowserCapabilities)HttpCapabilitiesBase.GetBrowserCapabilities(this);
                return _browsercaps;
            }

            set {
                _browsercaps = value;
            }
        }

        /*
         * Client's host name
         */

        /// <devdoc>
        ///    <para>Gets the
        ///       DNS name of remote client.</para>
        /// </devdoc>
        public String UserHostName {
            get {
                String s = (_wr != null) ? _wr.GetRemoteName() : null;
                if (String.IsNullOrEmpty(s))
                    s = UserHostAddress;
                return s;
            }
        }

        /*
         * Client's host address
         */

        /// <devdoc>
        ///    <para>Gets the
        ///       IP host address of remote client.</para>
        /// </devdoc>
        public String UserHostAddress {
            get {
                if (_wr != null)
                    return _wr.GetRemoteAddress();
                else
                    return null;
            }
        }

        /*
         * The current request's RAW Url (as supplied by worker request)
         */

        /// <devdoc>
        ///    <para>Gets the URI requsted by the client, which may include PathInfo and QueryString if it exists.
        ///    This value is unaffected by any URL rewriting or routing that may occur on the server.</para>
        /// </devdoc>
        public String RawUrl {
            get {
                EnsureRawUrl();

                if (_flags[needToValidateRawUrl]) {
                    _flags.Clear(needToValidateRawUrl);
                    ValidateString(_rawUrl, null, RequestValidationSource.RawUrl);
                }
                return _rawUrl;
            }
            internal set {
                _rawUrl = value;
            }
        }

        // Populates the RawUrl property but does not hook up validation.
        internal string EnsureRawUrl() {
            if (_rawUrl == null) {
                String url;

                if (_wr != null) {
                    url = _wr.GetRawUrl();
                }
                else {
                    String p = this.GetUnvalidatedPath();
                    String qs = this.QueryStringText;

                    if (!String.IsNullOrEmpty(qs))
                        url = p + "?" + qs;
                    else
                        url = p;
                }
                _rawUrl = url;
            }

            return _rawUrl;
        }

        // WOS 1953542: No Event Is Logged When App Config is Corrupt
        // This should never throw.  Since Uri.ctor can throw if config
        // is bad, we cannot use the public Url property.
        //
        // Security note: This property has not been sanitized and should not
        // be used for making decisions at runtime. It should only be used for
        // logging or other innocuous things.
        internal String UrlInternal {
            get {
                string q = QueryStringText;
                if (!String.IsNullOrEmpty(q))
                    q = "?" + HttpEncoder.CollapsePercentUFromStringInternal(q, QueryStringEncoding);

                // Get server name and port from Host header?  Or the old way?
                if (AppSettings.UseHostHeaderForRequestUrl) {
                    string serverAndPort = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderHost);
                    try {
                        if (!String.IsNullOrEmpty(serverAndPort)) {
                            // RFC 2732 (Section 2 for format, section 3 for update of RFC 2396 [HTTP 1.1]) mandates that
                            // IPv6 addresses in the host header be enclosed in []'s already.  Se we don't need to do the
                            // same check for a ---- IPv6 address as we do with _wr.GetServerName() below.
                            string u = _wr.GetProtocol() + "://" + serverAndPort + Path + q;
                            _url = new Uri(u);
                            return u;
                        }
                    } catch (UriFormatException) { /* Do nothing, leave _url null.  Backup plan will kick in below. */ }
                }

                // If for some reason the Host Header failed to produce a valid Url, fall back on the old way of doing it.
                String serverName = _wr.GetServerName();
                if (serverName.IndexOf(':') >= 0 && serverName[0] != '[')
                    serverName = "[" + serverName + "]"; // IPv6
                if (_wr.GetLocalPortAsString() == "80") {
                    return _wr.GetProtocol() + "://" + serverName + Path + q;
                }
                else {
                    return _wr.GetProtocol() + "://" + serverName + ":" + _wr.GetLocalPortAsString() + Path + q;
                }
            }
        }

        // The current request's Url
        //    Gets Information regarding URL of current request.
        public Uri Url {
            get {
                if (_url == null && _wr != null) {
                    // The Path is accessed in a deferred way to preserve the execution order that existed
                    // before the code in BuildUrl was factored out of this property.
                    // While evaluating the Path immediately would probably not have an impact on regular execution
                    // it might impact error cases. Consider a situation in which some method in workerRequest throws.
                    // If we evaluate Path early, then some other method might throw, thus producing a different
                    // error behavior for the same conditions. Passing in a Func preserves the old ordering.
                    _url = BuildUrl(() => Path);
                }

                return _url;
            }
        }

        internal Uri BuildUrl(Func<string> pathAccessor) {
            Uri url = null;
            string q = QueryStringText;
            if (!String.IsNullOrEmpty(q))
                q = "?" + HttpEncoder.CollapsePercentUFromStringInternal(q, QueryStringEncoding);

            // Get server name and port from Host header?  Or the old way?
            if (AppSettings.UseHostHeaderForRequestUrl) {
                string serverAndPort = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderHost);
                try {
                    if (!String.IsNullOrEmpty(serverAndPort)) {
                        // RFC 2732 (Section 2 for format, section 3 for update of RFC 2396 [HTTP 1.1]) mandates that
                        // IPv6 addresses in the host header be enclosed in []'s already.  Se we don't need to do the
                        // same check for a ---- IPv6 address as we do with _wr.GetServerName() below.
                        url = UriUtil.BuildUri(_wr.GetProtocol(), Uri.UnescapeDataString(serverAndPort), null /* port */, pathAccessor(), q);
                    }
                }
                catch (UriFormatException) { /* Do nothing, leave _url null.  Backup plan will kick in below. */ }
            }

            // If for some reason the Host Header failed to produce a valid Url, fall back on the old way of doing it.
            if (url == null) {
                String serverName = _wr.GetServerName();
                if (serverName.IndexOf(':') >= 0 && serverName[0] != '[')
                    serverName = "[" + serverName + "]"; // IPv6

                url = UriUtil.BuildUri(_wr.GetProtocol(), Uri.UnescapeDataString(serverName), _wr.GetLocalPortAsString(), pathAccessor(), q);
            }
            return url;
        }

        // Url of the Http referrer
        /// Gets information regarding the URL of the client's previous request that linked to the current URL.
        public Uri UrlReferrer {
            get {
                if (_referrer == null) {
                    if (_wr != null) {
                        String r = _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderReferer);

                        if (!String.IsNullOrEmpty(r)) {
                            try {
                                if (r.IndexOf("://", StringComparison.Ordinal) >= 0)
                                    _referrer = new Uri(r);
                                 else
                                    _referrer = new Uri(this.Url, r);
                            }
                            catch (HttpException) {
                                // malformed referrer shouldn't crash the request
                                _referrer = null;
                            }
                        }
                    }
                }

                return _referrer;
            }
        }

        // special case for perf in output cache module
        internal String IfModifiedSince {
            get {
                if (_wr == null)
                    return null;
                return _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderIfModifiedSince);
            }
        }

        // special case for perf in output cache module
        internal String IfNoneMatch {
            get {
                if (_wr == null)
                    return null;
                return _wr.GetKnownRequestHeader(HttpWorkerRequest.HeaderIfNoneMatch);
            }
        }

        // Params collection - combination of query string, form, server vars
        //    Gets a combined collection of QueryString+Form+ ServerVariable+Cookies.
        public NameValueCollection Params {
            get {
                if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low))
                    return GetParams();
                else
                    return GetParamsWithDemand();
            }
        }

        // Used in integrated pipeline mode to invalidate the params collection
        // after a change is made to the headers or server variables
        internal void InvalidateParams() {
            _params = null;
        }

        private NameValueCollection GetParams() {
            if (_params == null) {
                _params = new HttpValueCollection(64);
                FillInParamsCollection();
                _params.MakeReadOnly();
            }
            return _params;
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Low)]
        private NameValueCollection GetParamsWithDemand()
        {
            return GetParams();
        }


        // Default property that goes through the collections
        //      QueryString, Form, Cookies, ClientCertificate and ServerVariables
        public String this[String key] {
            get {
                String s;

                s = QueryString[key];
                if (s != null)
                    return s;

                s = Form[key];
                if (s != null)
                    return s;

                HttpCookie c = Cookies[key];
                if (c != null)
                    return c.Value;

                s = ServerVariables[key];
                if (s != null)
                    return s;

                return null;
            }
        }

        // Query string as String (private)
        internal String QueryStringText {
            get {
                if (_queryStringText == null) {
                    if (_wr != null) {
                        // if raw bytes available use them
                        byte[] rawQueryString = this.QueryStringBytes;

                        if (rawQueryString != null) {
                            if (rawQueryString.Length > 0)
                                _queryStringText = QueryStringEncoding.GetString(rawQueryString);
                            else
                                _queryStringText = String.Empty;
                        }
                        else {
                            _queryStringText = _wr.GetQueryString();
                        }
                    }

                    if (_queryStringText == null)
                        _queryStringText = String.Empty;

                    if (_queryStringText.Length > 0 && !AppSettings.UseLegacyRequestUrlGeneration)
                        _queryStringText = _queryStringText.Replace("#", "%23"); // we don't want consumers to perform string concatenation and end up with an unintended fragment
                }

                return _queryStringText;
            }

            set {
                // override the query string
                _queryStringText = value;
                _queryStringOverriden = true;

                if (_queryString != null) {
                    _params=null;
                    _queryString.MakeReadWrite();
                    _queryString.Reset();
                    FillInQueryStringCollection();
                    _queryString.MakeReadOnly();
                    Unvalidated.InvalidateQueryString();
                }
            }
        }

        // Query string as byte[] (private) -- for parsing
        internal byte[] QueryStringBytes {
            get {
                if (_queryStringOverriden)
                    return null;

                if (_queryStringBytes == null) {
                    if (_wr != null)
                        _queryStringBytes = _wr.GetQueryStringRawBytes();
                }

                return _queryStringBytes;
            }
        }

        // Query string collection
        //    <para>Gets the collection of QueryString variables.</para>
        //
        public NameValueCollection QueryString {
            get {
                EnsureQueryString();

                if (_flags[needToValidateQueryString]) {
                    _flags.Clear(needToValidateQueryString);
                    ValidateHttpValueCollection(_queryString, RequestValidationSource.QueryString);
                }

                return _queryString;
            }
        }

        // Populates the QueryString property but does not hook up validation.
        internal HttpValueCollection EnsureQueryString() {
            if (_queryString == null) {
                _queryString = new HttpValueCollection();

                if (_wr != null)
                    FillInQueryStringCollection();

                _queryString.MakeReadOnly();
            }

            return _queryString;
        }

        internal bool HasQueryString {
            get {
                if (_queryString != null)
                    return (_queryString.Count > 0);

                byte[] rawQueryString = this.QueryStringBytes;

                if (rawQueryString != null) {
                    return (rawQueryString.Length > 0);
                }
                else {
                    return (QueryStringText.Length > 0);
                }
            }
        }

        // Form collection
        ///    Gets a collection of Form variables.
        public NameValueCollection Form {
            get {
                EnsureForm();

                if (_flags[needToValidateForm]) {
                    _flags.Clear(needToValidateForm);
                    ValidateHttpValueCollection(_form, RequestValidationSource.Form);
                }

                return _form;
            }
        }

        // Populates the Form property but does not hook up validation.
        internal HttpValueCollection EnsureForm() {
            if (_form == null) {
                _form = new HttpValueCollection();

                if (_wr != null)
                    FillInFormCollection();

                _form.MakeReadOnly();
            }

            return _form;
        }

        internal bool HasForm {
            get {
                if (_form != null) {
                    return (_form.Count > 0);
                }
                else {
                    if (_wr != null && !_wr.HasEntityBody()) {
                        return false;
                    }
                    else {
                        return (Form.Count > 0);
                    }
                }
            }
        }


        internal HttpValueCollection SwitchForm(HttpValueCollection form) {
            HttpValueCollection oldForm = _form;
            _form = form;
            Unvalidated.InvalidateForm();
            return oldForm;
        }

        // Headers collection
        //    Gets a collection of HTTP headers.
        public NameValueCollection Headers {
            get {
                EnsureHeaders();

                if (_flags[needToValidateHeaders]) {
                    _flags.Clear(needToValidateHeaders);
                    ValidateHttpValueCollection(_headers, RequestValidationSource.Headers);
                }
                if (_flags[needToValidateCookielessHeader]) {
                    _flags.Clear(needToValidateCookielessHeader);
                    ValidateCookielessHeaderIfRequiredByConfig(_headers[CookielessHelperClass.COOKIELESS_SESSION_FILTER_HEADER]);
                }
                return _headers;
            }
        }

        // Populates the Headers property but does not hook up validation.
        internal HttpHeaderCollection EnsureHeaders() {
            if (_headers == null) {
                _headers = new HttpHeaderCollection(_wr, this, 8);

                if (_wr != null)
                    FillInHeadersCollection();

                if (!(_wr is IIS7WorkerRequest)) {
                    _headers.MakeReadOnly();
                }
            }

            return _headers;
        }

        // Allows access to request collections that have not gone through request validation
        public UnvalidatedRequestValues Unvalidated {
            get {
                if (_unvalidatedRequestValues == null) {
                    _unvalidatedRequestValues = new UnvalidatedRequestValues(this);
                }
                return _unvalidatedRequestValues;
            }
        }

        // Server vars collection
        // Gets a collection of web server variables.
        public NameValueCollection ServerVariables {
            get {
                if (HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low))
                    return GetServerVars();
                else
                    return GetServerVarsWithDemand();
            }
        }

        internal NameValueCollection GetServerVarsWithoutDemand() {
            return GetServerVars();
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Low)]
        private NameValueCollection GetServerVarsWithDemand()
        {
            return GetServerVars();
        }

        private NameValueCollection GetServerVars()
        {
            if (_serverVariables == null) {
                _serverVariables = new HttpServerVarsCollection(_wr, this);

                if ( !(_wr is IIS7WorkerRequest) ) {
                    _serverVariables.MakeReadOnly();
                }
            }
            return _serverVariables;
        }

        internal void SetSkipAuthorization(bool value) {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (wr == null) {
                return;
            }

            // If value is true, set server variable to "1".
            // If value is false, remove server variable by setting value to null.
            // Don't create server variable collection if it's not created yet.

            if (_serverVariables == null) {
                wr.SetServerVariable("IS_LOGIN_PAGE", value ? "1" : null);
            }
            else {
                _serverVariables.SetNoDemand("IS_LOGIN_PAGE", value ? "1" : null);
            }
        }

        internal void SetDynamicCompression(bool enable) {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (wr == null) {
                return;
            }

            // If "enable" is true, remove server variable by setting value to null.
            // If "enable" is false, set server variable to "0" to disable.
            // Don't create server variable collection if it's not created yet.

            if (_serverVariables == null) {
                wr.SetServerVariable("IIS_EnableDynamicCompression", enable ? null : "0");
            }
            else {
                _serverVariables.SetNoDemand("IIS_EnableDynamicCompression", enable ? null : "0");
            }
        }

        // WOS 1526602: ASP.Net v2.0: Response.AppendToLog does not work properly in integrated mode
        internal void AppendToLogQueryString(string logData) {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (wr == null ||  String.IsNullOrEmpty(logData)) {
                return;
            }

            // Don't create server variable collection if it's not created yet.
            if (_serverVariables == null) {
                string currentLogData = wr.GetServerVariable("LOG_QUERY_STRING");
                if (String.IsNullOrEmpty(currentLogData)) {
                    wr.SetServerVariable("LOG_QUERY_STRING", QueryStringText + logData);
                }
                else {
                    wr.SetServerVariable("LOG_QUERY_STRING", currentLogData + logData);
                }
            }
            else {
                string currentLogData = _serverVariables.Get("LOG_QUERY_STRING");
                if (String.IsNullOrEmpty(currentLogData)) {
                    _serverVariables.SetNoDemand("LOG_QUERY_STRING", QueryStringText + logData);
                }
                else {
                    _serverVariables.SetNoDemand("LOG_QUERY_STRING", currentLogData + logData);
                }
            }
        }

        // Cookie collection associated with current request
        //    Gets a collection of client's cookie variables.
        public HttpCookieCollection Cookies {
            get {
                EnsureCookies();

                if (_flags[needToValidateCookies]) {
                    _flags.Clear(needToValidateCookies);
                    ValidateCookieCollection(_cookies);
                }

                return _cookies;
            }
        }

        // Populates the Cookies property but does not hook up validation.
        internal HttpCookieCollection EnsureCookies() {
            if (_cookies == null) {
                _cookies = new HttpCookieCollection(null, false);

                if (_wr != null)
                    FillInCookiesCollection(_cookies, true /*includeResponse*/);

                if (HasTransitionedToWebSocketRequest) // cookies can't be modified after the WebSocket handshake is complete
                    _cookies.MakeReadOnly();
            }

            return _cookies;
        }

        // File collection associated with current request
        // Gets the collection of client-uploaded files (Multipart MIME format).
        public HttpFileCollection Files {
            get {
                EnsureFiles();

                if (_flags[needToValidatePostedFiles]) {
                    _flags.Clear(needToValidatePostedFiles);
                    ValidatePostedFileCollection(_files);
                }

                return _files;
            }
        }

        // Populates the Files property but does not hook up validation.
        internal HttpFileCollection EnsureFiles() {
            if (_files == null) {
                if (_readEntityBodyMode == ReadEntityBodyMode.Bufferless)
                    throw new HttpException(SR.GetString(SR.Incompatible_with_get_bufferless_input_stream));

                _files = new HttpFileCollection();

                if (_wr != null)
                    FillInFilesCollection();
            }

            return _files;
        }

        // Stream to read raw content
        //   Provides access to the raw contents of the incoming HTTP entity body.
        public Stream InputStream {
            get {
                if (_inputStream == null) {
                    if (_readEntityBodyMode == ReadEntityBodyMode.Bufferless)
                        throw new HttpException(SR.GetString(SR.Incompatible_with_get_bufferless_input_stream));

                    HttpRawUploadedContent rawContent = null;

                    if (_wr != null)
                        rawContent = GetEntireRawContent();

                    if (rawContent != null) {
                        _inputStream = new HttpInputStream(
                                                          rawContent,
                                                          0,
                                                          rawContent.Length
                                                          );
                    }
                    else {
                        _inputStream = new HttpInputStream(null, 0, 0);
                    }
                }

                return _inputStream;
            }
        }

        // ASP classic compat
        //       Gets the number of bytes in the current input stream.
        public int TotalBytes {
            get {
                Stream s = (_readEntityBodyStream != null) ? _readEntityBodyStream : InputStream;
                return(s != null) ? (int)s.Length : 0;
            }
        }

        // ASP classic compat
        //  Performs a binary read of a specified number of bytes from the current input stream.
        public byte[] BinaryRead(int count) {
            if (_readEntityBodyMode == ReadEntityBodyMode.Bufferless)
                throw new HttpException(SR.GetString(SR.Incompatible_with_get_bufferless_input_stream));

            if (count < 0 || count > TotalBytes)
                throw new ArgumentOutOfRangeException("count");

            if (count == 0)
                return new byte[0];

            byte[] buffer = new byte[count];
            int c = InputStream.Read(buffer, 0, count);

            if (c != count) {
                byte[] b2 = new byte[c];
                if (c > 0)
                    Array.Copy(buffer, b2, c);
                buffer = b2;
            }

            return buffer;
        }

        // Filtering of the input
        //   Gets or sets a filter to use when reading the current input stream.
        public Stream Filter {
            get {
                if (_installedFilter != null)
                    return _installedFilter;

                if (_filterSource == null)
                    _filterSource = new HttpInputStreamFilterSource();

                return _filterSource;
            }

            set {
                if (_filterSource == null)  // have to use the source -- null means source wasn't ever asked for
                    throw new HttpException(SR.GetString(SR.Invalid_request_filter));

                _installedFilter = value;
            }
        }


        // Client Certificate
        //    Gets information on the current request's client security certificate.
        public HttpClientCertificate ClientCertificate {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Low)]
            get {
                if (_clientCertificate == null) {
                    _clientCertificate = CreateHttpClientCertificateWithAssert();
                }

                return _clientCertificate;
            }
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        HttpClientCertificate CreateHttpClientCertificateWithAssert() {
            return new HttpClientCertificate(_context);
        }


        //    Gets LOGON_USER as WindowsIdentity
        public WindowsIdentity LogonUserIdentity {
            [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
            get {
                if (_logonUserIdentity == null) {
                    if (_wr != null) {
                        if (_wr is IIS7WorkerRequest && _context.NotificationContext != null &&
                            ((_context.NotificationContext.CurrentNotification == RequestNotification.AuthenticateRequest && !_context.NotificationContext.IsPostNotification)
                             || (_context.NotificationContext.CurrentNotification < RequestNotification.AuthenticateRequest))) {
                            throw new InvalidOperationException(SR.GetString(SR.Invalid_before_authentication));
                        }

                        _logonUserIdentity = _wr.GetLogonUserIdentity();
                    }
                }

                return _logonUserIdentity;
            }
        }

        private bool GranularValidationEnabled {
            get { return _flags[granularValidationEnabled]; }
        }

        private bool RequestValidationSuppressed {
            get { return _flags[requestValidationSuppressed]; }
        }

        //  Validate that the input from the browser is safe.
        public void ValidateInput() {
            // It doesn't make sense to call this multiple times per request.
            // Additionally, if validation was suppressed, no-op now.
            if (ValidateInputWasCalled || RequestValidationSuppressed) {
                return;
            }

            _flags.Set(hasValidateInputBeenCalled);

            // This is to prevent some XSS (cross site scripting) attacks (ASURT 122278)
            _flags.Set(needToValidateQueryString);
            _flags.Set(needToValidateForm);
            _flags.Set(needToValidateCookies);
            _flags.Set(needToValidatePostedFiles);
            _flags.Set(needToValidateRawUrl);
            _flags.Set(needToValidatePath);
            _flags.Set(needToValidatePathInfo);
            _flags.Set(needToValidateHeaders);
        }

        internal bool ValidateInputWasCalled {
            get {
                return _flags[hasValidateInputBeenCalled];
            }
        }

        // There are a few situations where we do not want to validate the request ever:
        private bool CanValidateRequest() {
            if (_wr == null) {
                return false;
            }

            // 1. State server requests
            if (_wr is StateHttpWorkerRequest) {
                return false;
            }

            // 2. DevDiv2 162442: When IIS has already rejected the request and we are inside logrequest or end request
            if (_wr is IIS7WorkerRequest &&
                (_context.Response.StatusCode == 404 || _context.Response.StatusCode == 400) &&
                (_context.NotificationContext != null) &&
                (_context.NotificationContext.CurrentNotification == RequestNotification.LogRequest ||
                _context.NotificationContext.CurrentNotification == RequestNotification.EndRequest)) {
                return false;
            }

            return true;
        }

        internal void ValidateInputIfRequiredByConfig() {
            // Do we need to enable request validation?
            RuntimeConfig config = RuntimeConfig.GetConfig(Context);
            HttpRuntimeSection runtimeSection = config.HttpRuntime;

            //////////////////////////////////////////////////////////////////////
            // Perform Path & QueryString validation checks for non-state_server requests
            if (CanValidateRequest()) {
                string requestUrl = Path;

                //////////////////////////////////////////////////////////////////
                // Verify the URL & QS lengths
                if (requestUrl.Length > runtimeSection.MaxUrlLength) {
                    throw new HttpException(400, SR.GetString(SR.Url_too_long));
                }
                if (QueryStringText.Length > runtimeSection.MaxQueryStringLength) {
                    throw new HttpException(400, SR.GetString(SR.QueryString_too_long));
                }

                //////////////////////////////////////////////////////////////////
                // Verify that the URL does not contain invalid chars
                char [] invalidChars = runtimeSection.RequestPathInvalidCharactersArray;
                if (invalidChars != null && invalidChars.Length > 0) {
                    int index = requestUrl.IndexOfAny(invalidChars);
                    if (index >= 0) {
                        string invalidString = new string(requestUrl[index], 1);
                        throw new HttpException(400, SR.GetString(SR.Dangerous_input_detected,
                                                                  "Request.Path", invalidString));
                    }
                    _flags.Set(needToValidateCookielessHeader);
                }
            }

            // only enable request validation for the entire pipeline in v4.0+ of the framework
            Version requestValidationMode = runtimeSection.RequestValidationMode;
            if (requestValidationMode == VersionUtil.Framework00) {
                // DevDiv #412689: <httpRuntime requestValidationMode="0.0" /> should suppress validation for
                // the entire request, even if a call to ValidateInput() takes place. The request path
                // characters and cookieless header (see 'needToValidateCookielessHeader') are still validated
                // if necessary. These can be suppressed via <httpRuntime requestPathInvalidChars="" />.
                _flags[requestValidationSuppressed] = true;
            }
            else if (requestValidationMode >= VersionUtil.Framework40) {
                ValidateInput();

                // Mode v4.5+ implies granular request validation
                if (requestValidationMode >= VersionUtil.Framework45) {
                    EnableGranularRequestValidation();
                }
            }
        }

        internal void ValidateCookielessHeaderIfRequiredByConfig(string header) {
            if (string.IsNullOrEmpty(header))
                return;
            if (!CanValidateRequest())
                return;
            // Verify that the header does not contain invalid chars
            char [] invalidChars = RuntimeConfig.GetConfig(Context).HttpRuntime.RequestPathInvalidCharactersArray;
            if (invalidChars != null && invalidChars.Length > 0) {
                int index = header.IndexOfAny(invalidChars);
                if (index >= 0) {
                    string invalidString = new string(header[index], 1);
                    throw new HttpException(400, SR.GetString(SR.Dangerous_input_detected, "Request.Path", invalidString));
                }
            }
        }

        private static string RemoveNullCharacters(string s) {
            if (s == null)
                return null;

            // Ignore null characters to prevent attacks (VSWhidbey 85016)
            if (s.IndexOf('\0') > -1)
                return s.Replace("\0", String.Empty);

            return s;
        }

        private void ValidateString(string value, string collectionKey, RequestValidationSource requestCollection) {

            value = RemoveNullCharacters(value);

            // Only provide the HttpContext if this is an actual HttpRequest; pass null
            // if this is simply a shell that exists for WebSockets.
            HttpContext contextToProvide = (HasTransitionedToWebSocketRequest) ? null : Context;

            int validationFailureIndex;
            if (!RequestValidator.Current.IsValidRequestString(contextToProvide, value, requestCollection, collectionKey, out validationFailureIndex)) {
                // Display only the piece of the string that caused the problem, padded by on each side
                string detectedString = collectionKey + "=\"";
                int startIndex = validationFailureIndex - 10;
                if (startIndex <= 0) {
                    startIndex = 0;
                }
                else {
                    // Start with "..." to show that this is not the beginning
                    detectedString += "...";
                }
                int endIndex = validationFailureIndex + 20;
                if (endIndex >= value.Length) {
                    endIndex = value.Length;
                    detectedString += value.Substring(startIndex, endIndex - startIndex) + "\"";
                }
                else {
                    detectedString += value.Substring(startIndex, endIndex - startIndex) + "...\"";
                }

                string collectionName = GetRequestValidationSourceName(requestCollection);
                throw new HttpRequestValidationException(SR.GetString(SR.Dangerous_input_detected,
                    collectionName, detectedString));
            }
        }

        internal void EnableGranularRequestValidation() {
            _flags[granularValidationEnabled] = true;
        }

        private static string GetRequestValidationSourceName(RequestValidationSource requestCollection) {
            switch (requestCollection) {
                case RequestValidationSource.Cookies: return "Request.Cookies";
                case RequestValidationSource.Files: return "Request.Files";
                case RequestValidationSource.Form: return "Request.Form";
                case RequestValidationSource.Headers: return "Request.Headers";
                case RequestValidationSource.Path: return "Request.Path";
                case RequestValidationSource.PathInfo: return "Request.PathInfo";
                case RequestValidationSource.QueryString: return "Request.QueryString";
                case RequestValidationSource.RawUrl: return "Request.RawUrl";

                default:
                    return "Request." + requestCollection.ToString();
            }
        }

        private void ValidateHttpValueCollection(HttpValueCollection collection, RequestValidationSource requestCollection) {
            if (GranularValidationEnabled) {
                // Granular request validation is enabled - validate collection entries only as they're accessed.
                collection.EnableGranularValidation((key, value) => ValidateString(value, key, requestCollection));
            }
            else {
                // Granular request validation is disabled - eagerly validate all collection entries.
                int c = collection.Count;

                for (int i = 0; i < c; i++) {
                    String key = collection.GetKey(i);

                    // Certain fields shouldn't go through validation - see comments in KeyIsCandidateForValidation for more information.
                    if (!HttpValueCollection.KeyIsCandidateForValidation(key)) {
                        continue;
                    }

                    String val = collection.Get(i);

                    if (!String.IsNullOrEmpty(val))
                        ValidateString(val, key, requestCollection);
                }
            }
        }

        private void ValidateCookieCollection(HttpCookieCollection cc) {
            if (GranularValidationEnabled) {
                // Granular request validation is enabled - validate collection entries only as they're accessed.
                cc.EnableGranularValidation((key, value) => ValidateString(value, key, RequestValidationSource.Cookies));
            }
            else {
                // Granular request validation is disabled - eagerly validate all collection entries.
                int c = cc.Count;

                for (int i = 0; i < c; i++) {
                    String key = cc.GetKey(i);
                    String val = cc.Get(i).Value;

                    if (!String.IsNullOrEmpty(val))
                        ValidateString(val, key, RequestValidationSource.Cookies);
                }
            }
        }

        private void ValidatePostedFileCollection(HttpFileCollection col) {
            if (GranularValidationEnabled) {
                // Granular request validation is enabled - validate collection entries only as they're accessed.
                col.EnableGranularValidation((key, value) => ValidateString(value, "filename", RequestValidationSource.Files));
            }
            else {
                // Granular request validation is disabled - eagerly validate all collection entries.
                for (int i = 0; i < col.Count; i++) {
                    string filename = col[i].FileName;
                    ValidateString(filename, "filename", RequestValidationSource.Files);
                }
            }
        }

        internal void ClearReferencesForWebSocketProcessing() {
            bool needToRevalidateInputs = ValidateInputWasCalled;

            // everything not marked [DoNotReset] should be eligible for garbage collection
            ReflectionUtil.Reset(this);

            if (needToRevalidateInputs) {
                ValidateInput();
            }
        }

        /*
         * Get coordinates of the clicked image send as name.x=&name.y=
         * in the form or in the query string
         * @param imageFieldName name of the image field
         * @return x,y as int[2] or null if not found
         */

        /// <devdoc>
        ///    <para>
        ///       Maps an incoming image field form parameter into appropriate x/y
        ///       coordinate values.
        ///    </para>
        /// </devdoc>
        public int[] MapImageCoordinates(String imageFieldName) {
            var coords = MapImageCoordinatatesInternal(imageFieldName, HttpVerb, QueryString, Form);
            if (coords != null) {
                return new[] { (int)coords[0], (int)coords[1] };
            }
            return null;
        }

        /*
         * Get coordinates of the clicked image send as name.x=&name.y=
         * in the form or in the query string
         * @param imageFieldName name of the image field
         * @return x,y as double[2] or null if not found
         */

        /// <devdoc>
        ///    <para>
        ///       Maps an incoming image field form parameter into appropriate x/y
        ///       coordinate values.
        ///    </para>
        /// </devdoc>
        public double[] MapRawImageCoordinates(String imageFieldName) {
            return MapImageCoordinatatesInternal(imageFieldName, HttpVerb, QueryString, Form);
        }

        internal static double[] MapImageCoordinatatesInternal(string imageFieldName, HttpVerb verb, NameValueCollection queryString, NameValueCollection form) {
            // Select collection where to look according to verb

            NameValueCollection c = null;

            switch (verb) {
                case HttpVerb.GET:
                case HttpVerb.HEAD:
                    c = queryString;
                    break;

                case HttpVerb.POST:
                    c = form;
                    break;

                default:
                    return null;
            }

            // Look for .x and .y values in the collection

            double[] ret = null;

            try {
                string x = c[imageFieldName + ".x"];
                string y = c[imageFieldName + ".y"];

                double xVal;
                double yVal;

                if (x != null && y != null && HttpUtility.TryParseCoordinates(x, out xVal) && HttpUtility.TryParseCoordinates(y, out yVal)) {
                    ret = new[] { xVal, yVal };
                }
            }
            catch {
                // eat parsing exceptions
            }

            return ret;
        }

        /*
         * Save contents of request into a file
         * @param filename where to save
         * @param includeHeaders flag to request inclusion of Http headers
         */

        /// <devdoc>
        ///    <para>Saves an HTTP request to disk.</para>
        /// </devdoc>
        public void SaveAs(String filename, bool includeHeaders) {
            // NDPWhidbey 14376
            if (!System.IO.Path.IsPathRooted(filename)) {
                HttpRuntimeSection config = RuntimeConfig.GetConfig(_context).HttpRuntime;
                if (config.RequireRootedSaveAsPath) {
                    throw new HttpException(SR.GetString(SR.SaveAs_requires_rooted_path, filename));
                }
            }

            FileStream f = new FileStream(filename, FileMode.Create);

            try {
                // headers

                if (includeHeaders) {
                    TextWriter w = new StreamWriter(f);

                    w.Write(this.HttpMethod + " " + this.Path);

                    String qs = this.QueryStringText;
                    if (!String.IsNullOrEmpty(qs))
                        w.Write("?" + qs);

                    if (_wr != null) {
                        // real request -- add protocol
                        w.Write(" " + _wr.GetHttpVersion() + "\r\n");

                        // headers
                        w.Write(CombineAllHeaders(true));
                    }
                    else {
                        // manufactured request
                        w.Write("\r\n");
                    }

                    w.Write("\r\n");
                    w.Flush();
                }

                // entity body

                HttpInputStream s = (HttpInputStream)this.InputStream;
                s.WriteTo(f);
                f.Flush();
            }
            finally {
                f.Close();
            }
        }

        /*
         * Map virtual path to physical path relative to current request
         * @param virtualPath virtual path (absolute or relative)
         * @return physical path
         */

        /// <devdoc>
        ///    <para>
        ///       Maps the given virtual path to a physical path.
        ///    </para>
        /// </devdoc>
        public String MapPath(String virtualPath) {
            return MapPath(VirtualPath.CreateAllowNull(virtualPath));
        }

        internal String MapPath(VirtualPath virtualPath) {
            if (_wr != null) {
                return MapPath(virtualPath, FilePathObject, true/*allowCrossAppMapping*/);
            }
            else {
                return virtualPath.MapPath();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Maps the given virtual path to a physical path.
        ///    </para>
        /// </devdoc>
        public String MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
            VirtualPath baseVirtualDirObject;
            if (String.IsNullOrEmpty(baseVirtualDir)) {
                // If no base is passed in, use the request's base dir (VSWhidbey 539063)
                baseVirtualDirObject = FilePathObject;
            }
            else {
                // We need to ensure a trailing slash to match v1.x behavior (VSWhidbey 539063)
                baseVirtualDirObject = VirtualPath.CreateTrailingSlash(baseVirtualDir);
            }

            return MapPath(VirtualPath.CreateAllowNull(virtualPath),
                baseVirtualDirObject, allowCrossAppMapping);
        }

        internal String MapPath(VirtualPath virtualPath, VirtualPath baseVirtualDir, bool allowCrossAppMapping) {
            if (_wr == null)
                throw new HttpException(SR.GetString(SR.Cannot_map_path_without_context));

            // treat null as "."

            // 
            if (virtualPath == null)
                virtualPath = VirtualPath.Create(".");

            VirtualPath originalVirtualPath = virtualPath; // remember for patch-up at the end

            // Combine it with the base if one was passed in
            if (baseVirtualDir != null) {
                virtualPath = baseVirtualDir.Combine(virtualPath);
            }

            if (!allowCrossAppMapping)
                virtualPath.FailIfNotWithinAppRoot();

            string realPath = virtualPath.MapPathInternal();

            // patch up the result for Everett combatibility (VSWhidbey 319826)
            if (virtualPath.VirtualPathString == "/" &&
                originalVirtualPath.VirtualPathString != "/" &&
                !originalVirtualPath.HasTrailingSlash &&
                UrlPath.PathEndsWithExtraSlash(realPath)) {
                realPath = realPath.Substring(0, realPath.Length - 1);
            }

            InternalSecurityPermissions.PathDiscovery(realPath).Demand();
            return realPath;
        }

        internal void InternalRewritePath(VirtualPath newPath, String newQueryString, bool rebaseClientPath) {
            // clear things that depend on path
            _pathTranslated = null;
            _pathInfo = null;
            _filePath = null;
            _url = null;
            Unvalidated.InvalidateUrl();

            // DevDiv 

            string temp = RawUrl;

            // remember the new path
            _path = newPath;

            if (rebaseClientPath) {
                _clientBaseDir = null;
                _clientFilePath = newPath;
            }

            // set a flag so we compute things that depend on path by hand
            _computePathInfo = true;

            // parse the new query string (might require config)
            if (newQueryString != null)
                this.QueryStringText = newQueryString;

            // remember the rewritten url
            _rewrittenUrl = _path.VirtualPathString;
            string q = QueryStringText;
            if (!String.IsNullOrEmpty(q))
                _rewrittenUrl += "?" + q;

            IIS7WorkerRequest iis7WorkerRequest = _wr as IIS7WorkerRequest;
            if (iis7WorkerRequest != null) {
                iis7WorkerRequest.RewriteNotifyPipeline(_path.VirtualPathString, newQueryString, rebaseClientPath);
            }
        }

        internal void InternalRewritePath(VirtualPath newFilePath, VirtualPath newPathInfo,
            String newQueryString, bool setClientFilePath) {
            // clear things that depend on path
            _pathTranslated = (_wr != null) ? newFilePath.MapPathInternal() : null;
            _pathInfo = newPathInfo;
            _filePath = newFilePath;
            _url = null;
            Unvalidated.InvalidateUrl();

            // DevDiv 

            string temp = RawUrl;

            if (newPathInfo == null) {
                _path = newFilePath;
            }
            else {
                // Combine the file path and the pathInfo to get the path.  Note that we can't call
                // newFilePath.Combine here, since the rules are very different here (VSWhidbey 498926, 528055)
                string newFullPathString = newFilePath.VirtualPathStringWhicheverAvailable + "/" + newPathInfo.VirtualPathString;
                _path = VirtualPath.Create(newFullPathString);
            }

            if (newQueryString != null)
                this.QueryStringText = newQueryString;

            // remember the rewritten url
            _rewrittenUrl = _path.VirtualPathString;
            string q = QueryStringText;
            if (!String.IsNullOrEmpty(q))
                _rewrittenUrl += "?" + q;

            // no need to calculate any paths
            _computePathInfo = false;

            if (setClientFilePath) {
                _clientFilePath = newFilePath;
            }

            IIS7WorkerRequest iis7WorkerRequest = _wr as IIS7WorkerRequest;
            if (iis7WorkerRequest != null) {
                String newPath = (_path != null && _path.VirtualPathString != null) ? _path.VirtualPathString : String.Empty;
                iis7WorkerRequest.RewriteNotifyPipeline(newPath, newQueryString, setClientFilePath);
            }
        }

        internal String RewrittenUrl {
            get { return _rewrittenUrl; }
        }

        internal string FetchServerVariable(string variable) {
            return _wr.GetServerVariable(variable);
        }

        // Used by IIS7WorkerRequest.SynchronizeServerVariables to update server variables in the collection
        internal void SynchronizeServerVariable(String name, String value) {
            if (name == "IS_LOGIN_PAGE") {
                bool skipAuth = (value != null && value != "0") ? true : false;
                _context.SetSkipAuthorizationNoDemand(skipAuth, true /*managedOnly*/);
            }
            // populate the server variables collection if necessary
            HttpServerVarsCollection serverVars = ServerVariables as HttpServerVarsCollection;
            if (serverVars != null) {
                serverVars.SynchronizeServerVariable(name, value);
            }
        }

        // Used by IIS7WorkerRequest.SynchronizeServerVariables to update server variables in the collection
        internal void SynchronizeHeader(String name, String value) {

            // populate the headers collection if necessary
            HttpHeaderCollection headers = Headers as HttpHeaderCollection;
            if (headers != null) {
                headers.SynchronizeHeader(name, value);
            }

            // if a header changes, the server variable also needs to be updated
            // populate the server variables collection if necessary
            HttpServerVarsCollection serverVars = ServerVariables as HttpServerVarsCollection;
            if (serverVars != null) {
                string svName = "HTTP_" + name.ToUpper(CultureInfo.InvariantCulture).Replace('-', '_');
                serverVars.SynchronizeServerVariable(svName, value);
            }
        }

        public ChannelBinding HttpChannelBinding {
            [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
            get {
                if (_wr is IIS7WorkerRequest)
                    return ((IIS7WorkerRequest)_wr).HttpChannelBindingToken;
                else if (_wr is ISAPIWorkerRequestInProc)
                    return ((ISAPIWorkerRequestInProc)_wr).HttpChannelBindingToken;
                throw new PlatformNotSupportedException();
            }
        }

        // Retrieves the TLS token bindings for the current request.
        // TLS token bindings help mitigate the risk of impersonation by an
        // attacker in the event an authenticated client's bearer tokens are
        // somehow exfiltrated from the client's machine.
        // More info: https://datatracker.ietf.org/doc/draft-popov-token-binding/
        //
        // This API requires that the server be running the IIS integrated mode
        // pipeline and that the server be Win10 or later. If these conditions
        // do not hold, this API will return null. This API could also return
        // null if the client doesn't support the TLS token binding protocol or
        // if the server has disabled support for the protocol.
        public ITlsTokenBindingInfo TlsTokenBindingInfo {
            get {
                if (!_tlsTokenBindingInfoResolved) {
                    IIS7WorkerRequest iis7wr = _wr as IIS7WorkerRequest;
                    if (iis7wr != null) {
                        _tlsTokenBindingInfo = iis7wr.GetTlsTokenBindingInfo(); // could return null
                    }
                    _tlsTokenBindingInfoResolved = true;
                }

                return _tlsTokenBindingInfo;
            }
        }

        // Only supported on IIS7 and later.
        // This is a wrapper for the IIS 7.0 IHttpRequest::InsertEntityBody method.
        // If you want to provide IIS with a copy of the request entity previously read by ASP.NET,
        // call the InsertEntityBody overload that takes no arguments.
        [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.High)]
        public void InsertEntityBody(byte[] buffer, int offset, int count) {
            EnsureHasNotTransitionedToWebSocket();

            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (wr == null)
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.InvalidOffsetOrCount, "offset", "count"));

            wr.InsertEntityBody(buffer, offset, count);
            NeedToInsertEntityBody = false;
        }

        // Only supported on IIS7 and later.  IIS does not maintain a copy of the request entity
        // after it has been read.  For this reason, it is recommended that only the handler
        // read the request entity.  This method provides IIS with a copy of the request entity that ASP.NET
        // previously read.  For example, this is useful for scenarios where a native handler may need to access the
        // request entity after it has been read by ASP.NET.
        public void InsertEntityBody() {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (wr == null)
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            byte[] buffer = EntityBody;
            if (buffer == null)
                return;
            wr.InsertEntityBody(buffer, 0, buffer.Length);
            NeedToInsertEntityBody = false;
        }

        // Determines if the request entity body has been read, and if so, how it was
        // read.  Used to avoid the exception thrown by GetBufferlessInputStream,
        // GetBufferedInputStream, Form, Files, InputStream, and BinaryRead when the
        // entity has already been read by an incompatible method.
        public ReadEntityBodyMode ReadEntityBodyMode { get { return _readEntityBodyMode; } }

        // GetBufferlessInputStream allows the caller to read the request entity bytes directly off the wire, either
        // synchronously or asynchronously.  The bytes are not buffered, and once read, neither ASP.NET nor IIS have a
        // copy of the request entity.  To read the entire entity, call Read or BeginRead/EndRead repeatedly until zero
        // bytes are returned.  The stream returned keeps track of the total bytes read, and for non-chunked entity bodies
        // it will not read more bytes than indicated by the Content-Length header.  If the client disconnects while the
        // entity is being read, the Read and BeginRead/EndRead methods will throw an exception.
        //
        // Throws HttpException if the entity has already been read and stored via Form, Files, InputStream, or GetBufferedInputStream.
        // To avoid this exception, first call Request.ReadEntityBodyMode.
        // Throws HttpException from Read/BeginRead/EndRead if the client disconnects while the entity is being read.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Bufferless", Justification = "Name is from the spec")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "This method does additional work which causes very big side-effects")]        
        public Stream GetBufferlessInputStream() {
            return GetInputStream(persistEntityBody: false);
        }

        // Same as GetBufferlessInputStream, but with the option of disabling system.web/httpRuntime/maxRequestLength.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Bufferless", Justification = "Name is from the spec")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "This method does additional work which causes very big side-effects")]        
        public Stream GetBufferlessInputStream(bool disableMaxRequestLength) {
            return GetInputStream(false, disableMaxRequestLength);
        }

        // GetBufferedInputStream is identical to GetBufferlessInputStream except that it also copies the bytes read
        // to internal storage used by ASP.NET to populate HttpRequest.Form, HttpRequest.Files, and HttpRequest.InputStream.
        //
        // Throws HttpException if the entity has already been read by Form, Files, InputStream, or GetBufferlessInputStream.
        // To avoid this exception, first call Request.ReadEntityBodyMode.
        // Throws HttpException from Read/BeginRead/EndRead if the client disconnects while the entity is being read.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Buffered", Justification = "Name is from the spec")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
          Justification = "This method does additional work which causes very big side-effects")]
        public Stream GetBufferedInputStream() {
            return GetInputStream(persistEntityBody: true);
        }
        
        private Stream GetInputStream(bool persistEntityBody, bool disableMaxRequestLength = false) {
            EnsureHasNotTransitionedToWebSocket();

            ReadEntityBodyMode requestedMode = (persistEntityBody) ? ReadEntityBodyMode.Buffered : ReadEntityBodyMode.Bufferless;
            ReadEntityBodyMode currentMode = _readEntityBodyMode;
            if (currentMode == ReadEntityBodyMode.None) {
                _readEntityBodyMode = requestedMode;
                _readEntityBodyStream = new HttpBufferlessInputStream(_context, persistEntityBody, disableMaxRequestLength);                
            }
            else if (currentMode == ReadEntityBodyMode.Classic) {
                throw new HttpException(SR.GetString(SR.Incompatible_with_input_stream));
            }
            else if (currentMode != requestedMode) {
                throw new HttpException((persistEntityBody) ? SR.GetString(SR.Incompatible_with_get_bufferless_input_stream) : SR.GetString(SR.Incompatible_with_get_buffered_input_stream));
            }
            return _readEntityBodyStream;
        }

        /// <summary>
        /// Forcibly terminates the underlying TCP connection to the client, causing any outstanding I/O to fail.
        /// </summary>
        /// <remarks>
        /// This method requires that the application be using the IIS integrated mode pipeline.
        /// 
        /// This method is thread-safe. Any thread may call it at any time.
        /// </remarks>
        public void Abort() {
            IIS7WorkerRequest wr = _wr as IIS7WorkerRequest;
            if (wr != null) {
                wr.AbortConnection();
            }
            else {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
        }

        // helper that throws an exception if we have transitioned the current request to a WebSocket request
        internal void EnsureHasNotTransitionedToWebSocket() {
            if (Context != null) {
                Context.EnsureHasNotTransitionedToWebSocket();
            }
        }

        /// <summary>
        /// Retrieves a CancellationToken that is signaled when a request times out.
        /// </summary>
        /// <remarks>
        /// The timeout period can be specified via config (httpRuntime/executionTimeout) or programmatically via
        /// Server.ScriptTimeout. The timeout period is measured from the time the request comes in. If using the
        /// default timeout of 110 seconds, the TimedOutToken will be tripped no earlier than 110 seconds after we
        /// started processing the request. The application developer can change the ScriptTimeout property if he
        /// so chooses, and as long as we haven't yet tripped this token we will respect the new timeout value.
        /// 
        /// Currently we only provide 15 second granularity on this token, so using the default timeout period of
        /// 110 seconds this means that we'll actually trip the token sometime between 110 - 125 seconds after we
        /// started processing the request. We may improve this resolution in the future.
        /// 
        /// Even though this property is thread-safe, there are restrictions on its use. Please see the remarks
        /// on HttpResponse.ClientDisconnectToken for caveats and best practices when consuming CancellationToken
        /// properties provided by ASP.NET intrinsics.
        /// 
        /// This property is meaningless once WebSockets request processing has started.
        /// </remarks>
        public CancellationToken TimedOutToken {
            get {
                EnsureHasNotTransitionedToWebSocket();

                HttpContext context = Context;
                return (context != null) ? context.TimedOutToken : default(CancellationToken);
            }
        }

    }
}
