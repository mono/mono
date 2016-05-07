//------------------------------------------------------------------------------
// <copyright file="HttpCacheVary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Cache Vary class.  Wraps Vary header
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Web.Util;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>Indicates that a cache should contain multiple 
    ///       representations for a particular Uri. This class is an encapsulation that
    ///       provides a rich, type-safe way to set the Vary header.</para>
    /// </devdoc>
    public sealed class HttpCacheVaryByHeaders {
        bool            _isModified;
        bool            _varyStar;
        HttpDictionary  _headers;

        public HttpCacheVaryByHeaders() {
            Reset();
        }

        internal void Reset() {
            _isModified = false;
            _varyStar = false;
            _headers = null;
        }

        /*
         * Reset based on the cached vary headers.
         */
        internal void ResetFromHeaders(String[] headers) {
            int i, n;

            if (headers == null) {
                _isModified = false;
                _varyStar = false;
                _headers = null;
            }
            else {
                _isModified = true;
                if (headers[0].Equals("*")) {
                    Debug.Assert(headers.Length == 1, "headers.Length == 1");

                    _varyStar = true;
                    _headers = null;
                }
                else {
                    _varyStar = false;
                    _headers = new HttpDictionary();
                    for (i = 0, n = headers.Length; i < n; i++) {
                        _headers.SetValue(headers[i], headers[i]);
                    }
                }
            }
        }

        internal bool IsModified() {
            return _isModified;
        }

        /*
         * Construct header value string
         */
        internal String ToHeaderString() {
            StringBuilder   s;
            Object          item;
            int             i, n;

            if (_varyStar) {
                return "*";
            }
            else if (_headers != null) {
                s = new StringBuilder();

                for (i = 0, n = _headers.Size; i < n; i++) {
                    item = _headers.GetValue(i);
                    if (item != null) {
                        HttpCachePolicy.AppendValueToHeader(s, (String)item);
                    }
                }

                if (s.Length > 0)
                    return s.ToString();
            }

            return null;
        }

        /*
         * Returns the headers, for package access only.
         * 
         * @return the headers.
         */
        internal String[] GetHeaders() {
            String[]    s = null;
            Object      item;
            int         i, j, c, n;

            if (_varyStar) {
                return new String[1] {"*"};
            }
            else if (_headers != null) {
                n = _headers.Size;
                c = 0;
                for (i = 0; i < n; i++) {
                    item = _headers.GetValue(i);
                    if (item != null) {
                        c++;
                    }
                }

                if (c > 0) {
                    s = new string[c];
                    j = 0;
                    for (i = 0; i < n; i++) {
                        item = _headers.GetValue(i);
                        if (item != null) {
                            s[j] = (String) item;
                            j++;
                        }
                    }

                    Debug.Assert(j == c, "j == c");
                }
            }

            return s;
        }

        //
        // Public methods and properties
        //


        /// <devdoc>
        ///    <para>Sets the "Vary: *" header and causes all other Vary:
        ///       header information to be dropped.</para>
        /// </devdoc>
        public void VaryByUnspecifiedParameters() {
            _isModified = true;
            _varyStar = true;
            _headers = null;
        }

        internal bool GetVaryByUnspecifiedParameters() {
            return _varyStar;
        }

        /*
         * Vary by accept types
         */

        /// <devdoc>
        ///    <para>Retrieves or assigns a value indicating whether the cache should vary by Accept types. This causes the
        ///       Vary: header to include an Accept field.</para>
        /// </devdoc>
        public bool AcceptTypes {
            get { 
                return this["Accept"]; 
            }

            set {         
                _isModified = true;
                this["Accept"] = value; 
            }
        }

        /*
         * Vary by accept language
         */

        /// <devdoc>
        ///    <para> Retrieves or assigns a Boolean value indicating whether
        ///       the cache should vary by user language.</para>
        /// </devdoc>
        public bool UserLanguage {
            get { 
                return this["Accept-Language"]; 
            }

            set { 
                _isModified = true;
                this["Accept-Language"] = value; 
            }
        }

        /*
         * Vary by user agent
         */

        /// <devdoc>
        ///    <para> Retrieves or assigns a Boolean value indicating whether
        ///       the cache should vary by user agent.</para>
        /// </devdoc>
        public bool UserAgent {
            get {   
                return this["User-Agent"]; 
            }

            set { 
                _isModified = true;
                this["User-Agent"] = value; 
            }
        }

        /*
         * Vary by charset
         */

        /// <devdoc>
        ///    <para> Retrieves or assigns a value indicating whether the
        ///       cache should vary by browser character set.</para>
        /// </devdoc>
        public bool UserCharSet {
            get { 
                return this["Accept-Charset"]; 
            }

            set { 
                _isModified = true;
                this["Accept-Charset"] = value; 
            }
        }

        /*
         * Vary by a given header
         */

        /// <devdoc>
        ///    <para> Default property.
        ///       Indexed property indicating that a cache should (or should not) vary according
        ///       to a custom header.</para>
        /// </devdoc>
        public bool this[String header]
        {
            get {
                if (header == null) {
                    throw new ArgumentNullException("header");
                }

                if (header.Equals("*")) {
                    return _varyStar;
                }
                else {
                    return (_headers != null && _headers.GetValue(header) != null);
                }
            }

            set {
                if (header == null) {
                    throw new ArgumentNullException("header");
                }

                /*
                 * Since adding a Vary header is more restrictive, we don't
                 * want components to be able to set a Vary header to false
                 * if another component has set it to true.
                 */
                if (value == false) {
                    return;
                }

                _isModified = true;

                if (header.Equals("*")) {
                    VaryByUnspecifiedParameters();
                }
                else {
                    // set value to header if true or null if false
                    if (!_varyStar) {
                        if (_headers == null) {
                            _headers = new HttpDictionary();
                        }

                        _headers.SetValue(header, header);
                    }
                }
            }
        }
    }
}
