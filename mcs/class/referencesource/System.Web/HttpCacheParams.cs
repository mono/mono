//------------------------------------------------------------------------------
// <copyright file="HttpCacheParams.cs" company="Microsoft">
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
    public sealed class HttpCacheVaryByParams {
        HttpDictionary  _parameters;
        int             _ignoreParams;
        bool            _isModified;
        bool            _paramsStar;

        public HttpCacheVaryByParams() {
            Reset();
        }

        internal void Reset() {
            _isModified = false;
            _paramsStar = false;
            _parameters = null;
            _ignoreParams = -1;
        }

        /// <summary>
        /// Set the Parameters in Cache Vary 
        /// </summary>
        /// <param name="parameters"></param>
        public void SetParams(string[] parameters) {
            int i, n;

            Reset();
            if (parameters != null) {
                _isModified = true;
                if (parameters[0].Length == 0) {
                    Debug.Assert(parameters.Length == 1, "parameters.Length == 1");

                    IgnoreParams = true;
                }
                else if (parameters[0].Equals("*")) {
                    Debug.Assert(parameters.Length == 1, "parameters.Length == 1");

                    _paramsStar = true;
                }
                else {
                    _parameters = new HttpDictionary();
                    for (i = 0, n = parameters.Length; i < n; i++) {
                        _parameters.SetValue(parameters[i], parameters[i]);
                    }
                }
            }
        }

        internal bool IsModified() {
            return _isModified;
        }

        internal bool AcceptsParams() {
            return _ignoreParams == 1 || _paramsStar || _parameters != null;
        }

        /// <summary>
        /// Get the Parameters in Cache Vary
        /// </summary>
        /// <returns></returns>
        public string[] GetParams() {
            string[]    s = null;
            Object      item;
            int         i, j, c, n;

            if (_ignoreParams == 1) {
                s =  new string[1] {string.Empty};
            }
            else if (_paramsStar) {
                s =  new string[1] {"*"};
            }
            else if (_parameters != null) {
                n = _parameters.Size;
                c = 0;
                for (i = 0; i < n; i++) {
                    item = _parameters.GetValue(i);
                    if (item != null) {
                        c++;
                    }
                }

                if (c > 0) {
                    s = new string[c];
                    j = 0;
                    for (i = 0; i < n; i++) {
                        item = _parameters.GetValue(i);
                        if (item != null) {
                            s[j] = (string) item;
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

                if (header.Length == 0) {
                    return _ignoreParams == 1;
                }
                else {
                    return _paramsStar || 
                           (_parameters != null && _parameters.GetValue(header) != null);
                }
            }

            set {
                if (header == null) {
                    throw new ArgumentNullException("header");
                }

                if (header.Length == 0) {
                    IgnoreParams = value;
                }
                /*
                 * Since adding a Vary parameter is more restrictive, we don't
                 * want components to be able to set a Vary parameter to false
                 * if another component has set it to true.
                 */
                else if (value) {
                    _isModified = true;
                    _ignoreParams = 0;
                    if (header.Equals("*")) {
                        _paramsStar = true;
                        _parameters = null;
                    }
                    else {
                        // set value to header if true or null if false
                        if (!_paramsStar) {
                            if (_parameters == null) {
                                _parameters = new HttpDictionary();
                            }

                            _parameters.SetValue(header, header);
                        }
                    }
                }
            }
        }


        public bool IgnoreParams {
            get {
                return _ignoreParams == 1;
            }

            set {
                // Don't ignore if params have been added
                if (_paramsStar || _parameters != null) {
                    return;
                }

                if (_ignoreParams == -1 || _ignoreParams == 1) {
                    _ignoreParams = value ? 1 : 0;
                    _isModified = true;
                }
            }
        }

        internal bool IsVaryByStar { 
            get { 
                return _paramsStar; 
            } 
        }
    }
}
