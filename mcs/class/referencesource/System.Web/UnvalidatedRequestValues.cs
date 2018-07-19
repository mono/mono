//------------------------------------------------------------------------------
// <copyright file="UnvalidatedRequestValues.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections.Specialized;

    // Allows access to Form, QueryString, and other request values without going through the active
    // request validator. Useful for allowing granular access to particular inputs (like user input
    // that can contain HTML) without disabling validation for the request at large.

    public sealed class UnvalidatedRequestValues {

        private readonly HttpRequest _request;

        internal UnvalidatedRequestValues(HttpRequest request) {
            _request = request;
        }

        // Corresponds to the unvalidated version of Request.Form
        private HttpValueCollection _form;
        public NameValueCollection Form {
            get {
                if (_form == null) {
                    HttpValueCollection originalForm = _request.EnsureForm();
                    _form = new HttpValueCollection(originalForm); // copy ctor disables validation
                }
                return _form;
            }
        }

        // Forces reevaluation of the Form, e.g. as the result of Server.Execute replacing it
        internal void InvalidateForm() {
            _form = null;
        }

        // Corresponds to the unvalidated version of Request.QueryString
        private HttpValueCollection _queryString;
        public NameValueCollection QueryString {
            get {
                if (_queryString == null) {
                    HttpValueCollection originalQueryString = _request.EnsureQueryString();
                    _queryString = new HttpValueCollection(originalQueryString); // copy ctor disables validation
                }
                return _queryString;
            }
        }

        // Forces reevaluation of the QueryString, e.g. as the result of Server.Execute replacing it
        internal void InvalidateQueryString() {
            _queryString = null;
        }

        // Corresponds to the unvalidated version of Request.Headers
        private HttpHeaderCollection _headers;
        public NameValueCollection Headers {
            get {
                if (_headers == null) {
                    HttpHeaderCollection originalHeaders = _request.EnsureHeaders();
                    _headers = new HttpHeaderCollection(originalHeaders); // copy ctor disables validation
                }
                return _headers;
            }
        }

        // Corresponds to the unvalidated version of Request.Cookies
        private HttpCookieCollection _cookies;
        public HttpCookieCollection Cookies {
            get {
                if (_cookies == null) {
                    HttpCookieCollection originalCookies = _request.EnsureCookies();
                    _cookies = new HttpCookieCollection(originalCookies); // copy ctor disables validation
                }
                return _cookies;
            }
        }

        // Corresponds to the unvalidated version of Request.Files
        private HttpFileCollection _files;
        public HttpFileCollection Files {
            get {
                if (_files == null) {
                    HttpFileCollection originalFiles = _request.EnsureFiles();
                    _files = new HttpFileCollection(originalFiles); // copy ctor disables validation
                }
                return _files;
            }
        }

        public string RawUrl {
            get {
                return _request.EnsureRawUrl();
            }
        }

        public string Path {
            get {
                return _request.GetUnvalidatedPath();
            }
        }

        public string PathInfo {
            get {
                return _request.GetUnvalidatedPathInfo();
            }
        }

        public string this[string field] {
            get {
                // The original logic in HttpRequest.get_Item looked in these four collections, so we should
                // also, even though ServerVariables doesn't go through validation.

                string qsValue = QueryString[field];
                if (qsValue != null) {
                    return qsValue;
                }

                string formValue = Form[field];
                if (formValue != null) {
                    return formValue;
                }

                HttpCookie cookie = Cookies[field];
                if (cookie != null) {
                    return cookie.Value;
                }

                string svValue = _request.ServerVariables[field];
                if (svValue != null) {
                    return svValue;
                }

                return null;
            }
        }

        private Uri _url;
        public Uri Url {
            get {
                if (_url == null) {
                    _url = _request.BuildUrl(() => Path);
                }
                return _url;
            }
        }

        // Forces reevaluation of the Url, e.g. as the result of Server.Execute replacing it
        internal void InvalidateUrl() {
            _url = null;
        }
    }
}
