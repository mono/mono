//------------------------------------------------------------------------------
// <copyright file="DiscoveryExceptionDictionary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;
    using System.Collections;
    using System.Security.Permissions;
    using System.Globalization;

    /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DiscoveryExceptionDictionary : DictionaryBase {

        /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Exception this[string url] {
            get {
                return (Exception) Dictionary[url];
            }
            set {
                Dictionary[url] = value;
            }
        }

        /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary.Keys"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Keys {
            get { return Dictionary.Keys; }
        }

        /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary.Values"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Values {
            get {
                return Dictionary.Values;
            }
        }

        /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(string url, Exception value) {
            Dictionary.Add(url, value);
        }

        /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(string url) {
            return Dictionary.Contains(url);
        }

        /// <include file='doc\DiscoveryExceptionDictionary.uex' path='docs/doc[@for="DiscoveryExceptionDictionary.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(string url) {
            Dictionary.Remove(url);
        }

    }

}
