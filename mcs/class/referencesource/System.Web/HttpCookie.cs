//------------------------------------------------------------------------------
// <copyright file="HttpCookie.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HttpCookie - collection + name + path
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System.Text;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Configuration;
    using System.Web.Management;
    using Util;


    /// <devdoc>
    ///    <para>
    ///       Provides a type-safe way
    ///       to access multiple HTTP cookies.
    ///    </para>
    /// </devdoc>
    public sealed class HttpCookie {
        private String _name;
        private String _path = "/";
        private bool _secure;
        private bool _httpOnly;
        private String _domain;
        private bool _expirationSet;
        private DateTime _expires;
        private String _stringValue;
        private HttpValueCollection _multiValue;
        private bool _changed;
        private bool _added;

        internal HttpCookie() {
            _changed = true;
        }

        /*
         * Constructor - empty cookie with name
         */

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.HttpCookie'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public HttpCookie(String name) {
            _name = name;

            SetDefaultsFromConfig();
            _changed = true;
        }

        /*
         * Constructor - cookie with name and value
         */

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.HttpCookie'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        public HttpCookie(String name, String value) {
            _name = name;
            _stringValue = value;

            SetDefaultsFromConfig();
            _changed = true;
        }

        private void SetDefaultsFromConfig() {
            HttpCookiesSection config = RuntimeConfig.GetConfig().HttpCookies;
            _secure = config.RequireSSL;
            _httpOnly = config.HttpOnlyCookies;
            
            if (config.Domain != null && config.Domain.Length > 0)
                _domain = config.Domain;
        }

        /*
         * Whether the cookie contents have changed
         */
        internal bool Changed {
            get { return _changed; }
            set { _changed = value; }
        }

        /*
         * Whether the cookie has been added
         */
        internal bool Added {
            get { return _added; }
            set { _added = value; }
        }

        // DevID 251951	Cookie is getting duplicated by ASP.NET when they are added via a native module
        // This flag is used to remember that this cookie came from an IIS Set-Header flag, 
        // so we don't duplicate it and send it back to IIS
        internal bool IsInResponseHeader {
            get;
            set;
        }

        /*
         * Cookie name
         */

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or sets the name of cookie.
        ///    </para>
        /// </devdoc>
        public String Name {
            get { return _name;}
            set { 
                _name = value;
                _changed = true;
            }
        }

        /*
         * Cookie path
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the URL prefix to transmit with the
        ///       current cookie.
        ///    </para>
        /// </devdoc>
        public String Path {
            get { return _path;}
            set { 
                _path = value;
                _changed = true;
            }
        }

        /*
         * 'Secure' flag
         */

        /// <devdoc>
        ///    <para>
        ///       Indicates whether the cookie should be transmitted only over HTTPS.
        ///    </para>
        /// </devdoc>
        public bool Secure {
            get { return _secure;}
            set { 
                _secure = value;
                _changed = true;
            }
        }

        /// <summary>
        /// Determines whether this cookie is allowed to participate in output caching.
        /// </summary>
        /// <remarks>
        /// If a given HttpResponse contains one or more outbound cookies with Shareable = false (the default value),
        /// output caching will be suppressed for that response. This prevents cookies that contain potentially
        /// sensitive information, e.g. FormsAuth cookies, from being cached in the response and sent to multiple
        /// clients. If a developer wants to allow a response containing cookies to be cached, he should configure
        /// caching as normal for the response, e.g. via the OutputCache directive, MVC's [OutputCache] attribute,
        /// etc., and he should make sure that all outbound cookies are marked Shareable = true.
        /// </remarks>
        public bool Shareable {
            get;
            set; // don't need to set _changed flag since Set-Cookie header isn't affected by value of Shareable
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates whether the cookie should have HttpOnly attribute
        ///    </para>
        /// </devdoc>
        public bool HttpOnly {
            get { return _httpOnly;}
            set { 
                _httpOnly = value;
                _changed = true;
            }
        }

        /*
         * Cookie domain
         */

        /// <devdoc>
        ///    <para>
        ///       Restricts domain cookie is to be used with.
        ///    </para>
        /// </devdoc>
        public String Domain {
            get { return _domain;}
            set { 
                _domain = value;
                _changed = true;
            }
        }

        /*
         * Cookie expiration
         */

        /// <devdoc>
        ///    <para>
        ///       Expiration time for cookie (in minutes).
        ///    </para>
        /// </devdoc>
        public DateTime Expires {
            get {
                return(_expirationSet ? _expires : DateTime.MinValue);
            }

            set {
                _expires = value;
                _expirationSet = true;
                _changed = true;
            }
        }

        /*
         * Cookie value as string
         */

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or
        ///       sets an individual cookie value.
        ///    </para>
        /// </devdoc>
        public String Value {
            get {
                if (_multiValue != null)
                    return _multiValue.ToString(false);
                else
                    return _stringValue;
            }

            set {
                if (_multiValue != null) {
                    // reset multivalue collection to contain
                    // single keyless value
                    _multiValue.Reset();
                    _multiValue.Add(null, value);
                }
                else {
                    // remember as string
                    _stringValue = value;
                }
                _changed = true;
            }
        }

        /*
         * Checks is cookie has sub-keys
         */

        /// <devdoc>
        ///    <para>Gets a
        ///       value indicating whether the cookie has sub-keys.</para>
        /// </devdoc>
        public bool HasKeys {
            get { return Values.HasKeys();}
        }

        private bool SupportsHttpOnly(HttpContext context) {
            if (context != null && context.Request != null) {
                HttpBrowserCapabilities browser = context.Request.Browser;
                return (browser != null && (browser.Type != "IE5" || browser.Platform != "MacPPC"));
            }
            return false;
        }

        /*
         * Cookie values as multivalue collection
         */

        /// <devdoc>
        ///    <para>Gets individual key:value pairs within a single cookie object.</para>
        /// </devdoc>
        public NameValueCollection Values {
            get {
                if (_multiValue == null) {
                    // create collection on demand
                    _multiValue = new HttpValueCollection();

                    // convert existing string value into multivalue
                    if (_stringValue != null) {
                        if (_stringValue.IndexOf('&') >= 0 || _stringValue.IndexOf('=') >= 0)
                            _multiValue.FillFromString(_stringValue);
                        else
                            _multiValue.Add(null, _stringValue);

                        _stringValue = null;
                    }
                }

                _changed = true;

                return _multiValue;
            }
        }

        /*
         * Default indexed property -- lookup the multivalue collection
         */

        /// <devdoc>
        ///    <para>
        ///       Shortcut for HttpCookie$Values[key]. Required for ASP compatibility.
        ///    </para>
        /// </devdoc>
        public String this[String key]
        {
            get {
                return Values[key];
            }

            set {
                Values[key] = value;
                _changed = true;
            }
        }

        /// <summary>
        /// Converts the specified string representation of an HTTP cookie to HttpCookie
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string input, out HttpCookie result) {
            result = null;

            if (string.IsNullOrEmpty(input)) {
                return false;
            }

            // The substring before the first ';' is cookie-pair, with format of cookiename[=key1=val2&key2=val2&...]
            int dividerIndex = input.IndexOf(';');
            string cookiePair = dividerIndex >= 0 ? input.Substring(0, dividerIndex) : input;

            HttpCookie cookie = HttpRequest.CreateCookieFromString(cookiePair.Trim());

            // If there was no cookie name being created, stop parsing and return
            if (string.IsNullOrEmpty(cookie.Name)) {
                return false;
            }

            //
            // Parse the collections of cookie-av 
            // cookie-av = expires-av/max-age-av/domain-av/path-av/secure-av/httponly-av/extension-av
            // https://tools.ietf.org/html/rfc6265 

            while (dividerIndex >= 0 && dividerIndex < input.Length - 1) {
                int cookieAvStartIndex = dividerIndex + 1;
                dividerIndex = input.IndexOf(';', cookieAvStartIndex);
                string cookieAv = dividerIndex >= 0 ? input.Substring(cookieAvStartIndex, dividerIndex - cookieAvStartIndex).Trim() : input.Substring(cookieAvStartIndex).Trim();

                int assignmentIndex = cookieAv.IndexOf('=');
                string attributeName = assignmentIndex >= 0 ? cookieAv.Substring(0, assignmentIndex).Trim() : cookieAv;
                string attributeValue = assignmentIndex >= 0 && assignmentIndex < cookieAv.Length - 1 ? cookieAv.Substring(assignmentIndex + 1).Trim() : null;

                //
                // Parse supported cookie-av Attribute

                //
                // Expires
                if (StringUtil.EqualsIgnoreCase(attributeName, "Expires")) {
                    DateTime dt;
                    if (DateTime.TryParse(attributeValue, out dt)) {
                        cookie.Expires = dt;
                    }
                }
                //
                // Domain
                else if (attributeValue != null && StringUtil.EqualsIgnoreCase(attributeName, "Domain")) {
                    cookie.Domain = attributeValue;
                }
                //
                // Path
                else if (attributeValue != null && StringUtil.EqualsIgnoreCase(attributeName, "Path")) {
                    cookie.Path = attributeValue;
                }
                //
                // Secure
                else if (StringUtil.EqualsIgnoreCase(attributeName, "Secure")) {
                    cookie.Secure = true;
                }
                //
                // HttpOnly
                else if (StringUtil.EqualsIgnoreCase(attributeName, "HttpOnly")) {
                    cookie.HttpOnly = true;
                }
            }

            result = cookie;

            return true;
        }

        /*
         * Construct set-cookie header
         */
        internal HttpResponseHeader GetSetCookieHeader(HttpContext context) {
            StringBuilder s = new StringBuilder();

            // cookiename=
            if (!String.IsNullOrEmpty(_name)) {
                s.Append(_name);
                s.Append('=');
            }

            // key=value&...
            if (_multiValue != null)
                s.Append(_multiValue.ToString(false));
            else if (_stringValue != null)
                s.Append(_stringValue);

            // domain
            if (!String.IsNullOrEmpty(_domain)) {
                s.Append("; domain=");
                s.Append(_domain);
            }

            // expiration
            if (_expirationSet && _expires != DateTime.MinValue) {
                s.Append("; expires=");
                s.Append(HttpUtility.FormatHttpCookieDateTime(_expires));
            }

            // path
            if (!String.IsNullOrEmpty(_path)) {
                s.Append("; path=");
                s.Append(_path);
            }

            // secure
            if (_secure)
                s.Append("; secure");

            // httponly, Note: IE5 on the Mac doesn't support this
            if (_httpOnly && SupportsHttpOnly(context)) {
                s.Append("; HttpOnly");
            }

            // return as HttpResponseHeader
            return new HttpResponseHeader(HttpWorkerRequest.HeaderSetCookie, s.ToString());
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////

    public enum HttpCookieMode {

        UseUri,          // cookieless=true

        UseCookies,      // cookieless=false

        AutoDetect,      // cookieless=AutoDetect; Probe if device is cookied

        UseDeviceProfile // cookieless=UseDeviceProfile; Base decision on caps
    }
}
