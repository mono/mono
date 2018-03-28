//------------------------------------------------------------------------------
// <copyright file="DiscoveryClientReferences.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DiscoveryClientReferenceCollection : DictionaryBase {

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DiscoveryReference this[string url] {
            get {
                return (DiscoveryReference) Dictionary[url];
            }
            set {
                Dictionary[url] = value;
            }
        }

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.Keys"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Keys {
            get { return Dictionary.Keys; }
        }

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.Values"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ICollection Values {
            get {
                return Dictionary.Values;
            }
        }

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(DiscoveryReference value) {
            Add(value.Url, value);
        }

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.Add1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Add(string url, DiscoveryReference value) {
            Dictionary.Add(url, value);
        }

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(string url) {
            return Dictionary.Contains(url);
        }

        /// <include file='doc\DiscoveryClientReferences.uex' path='docs/doc[@for="DiscoveryClientReferenceCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(string url) {
            Dictionary.Remove(url);
        }

    }

}
