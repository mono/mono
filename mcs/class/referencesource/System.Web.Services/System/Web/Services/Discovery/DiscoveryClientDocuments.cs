//------------------------------------------------------------------------------
// <copyright file="DiscoveryClientDocuments.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    
    using System.Collections;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Globalization;

    /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DiscoveryClientDocumentCollection : DictionaryBase {

        /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object this[string url] {
            get {
                return Dictionary[url];
            }
            set {
                Dictionary[url] = value;
            }
        }

        /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection.Keys"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Keys {
            get { return Dictionary.Keys; }
        }

        /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection.Values"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Values {
            get {
                return Dictionary.Values;
            }
        }

        /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(string url, object value) {
            Dictionary.Add(url, value);
        }

        /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(string url) {
            return Dictionary.Contains(url);
        }

        /// <include file='doc\DiscoveryClientDocuments.uex' path='docs/doc[@for="DiscoveryClientDocumentCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(string url) {
            Dictionary.Remove(url);
        }

    }

}
