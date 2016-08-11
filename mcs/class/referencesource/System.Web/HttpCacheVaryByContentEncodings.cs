//------------------------------------------------------------------------------
// <copyright file="HttpCacheVaryByContentEncodings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HttpCacheVaryByContentEncodings
 * 
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Web.Util;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>Provides a type-safe way to vary by Content-Encoding.</para>
    /// </devdoc>
    public sealed class HttpCacheVaryByContentEncodings {
        String[]        _contentEncodings;
        bool            _isModified;

        public HttpCacheVaryByContentEncodings() {
            Reset();
        }

        internal void Reset() {
            _isModified = false;
            _contentEncodings = null;
        }
  
        /// <summary>
        /// Set the Content Encodings in Cache Vary
        /// </summary>
        /// <param name="contentEncodings"></param>
        public void SetContentEncodings(string[] contentEncodings) {

            Reset();
            if (contentEncodings != null) {
                _isModified = true;
                _contentEncodings = new String[contentEncodings.Length];
                for (int i = 0; i < contentEncodings.Length; i++) {
                    _contentEncodings[i] = contentEncodings[i];
                }
            }
        }

        // the response is not cacheable if we're varying by content encoding
        // and the content-encoding header is not one of the encodings that we're
        // varying by
        internal bool IsCacheableEncoding(string coding) {
            // return true if we are not varying by content encoding.
            if (_contentEncodings == null) {
                return true;
            }

            // return true if there is no Content-Encoding header
            if (coding == null) {
                return true;
            }

            // return true if the Content-Encoding header is listed
            for (int i = 0; i < _contentEncodings.Length; i++) {
                if (_contentEncodings[i] == coding) {
                    return true;
                }
            }

            // return false if the Content-Encoding header is not listed
            return false;
        }

        internal bool IsModified() {
            return _isModified;
        }
               
        /// <summary>
        /// Get the Content Encodings in Cache Vary
        /// </summary>
        /// <returns></returns>
        public string[] GetContentEncodings() {
            if (_contentEncodings != null) {
                string[] contentEncodings = new string[_contentEncodings.Length];
                _contentEncodings.CopyTo(contentEncodings, 0);
                return contentEncodings;
            }
            return null;
        }

        //
        // Public methods and properties
        //


        /// <devdoc>
        ///    <para> Default property.
        ///       Indexed property indicating that a cache should (or should not) vary according
        ///       to a Content-Encoding.</para>
        /// </devdoc>
        public bool this[String contentEncoding]
        {
            get {
                if (String.IsNullOrEmpty(contentEncoding)) {
                    throw new ArgumentNullException(SR.GetString(SR.Parameter_NullOrEmpty, "contentEncoding"));
                }
                if (_contentEncodings == null) {
                    return false;
                }
                for(int i = 0; i < _contentEncodings.Length; i++) {
                    if (_contentEncodings[i] == contentEncoding) {
                        return true;
                    }
                }
                return false;
            }

            set {
                if (String.IsNullOrEmpty(contentEncoding)) {
                    throw new ArgumentNullException(SR.GetString(SR.Parameter_NullOrEmpty, "contentEncoding"));
                }

                // if someone enabled it, don't allow someone else to disable it.
                if (!value) {
                    return;
                }

                _isModified = true;
                if (_contentEncodings != null) {
                    string[] contentEncodings = new String[_contentEncodings.Length + 1];
                    for (int i = 0; i < _contentEncodings.Length; i++) {
                        contentEncodings[i] = _contentEncodings[i];
                    }
                    contentEncodings[contentEncodings.Length - 1] = contentEncoding;
                    _contentEncodings = contentEncodings;
                    return;
                }
                _contentEncodings = new String[1];
                _contentEncodings[0] = contentEncoding;
            }
        }
    }
}
