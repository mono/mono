//------------------------------------------------------------------------------
// <copyright file="DiscoveryReferences.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    
    using System.Collections;
    using System.Diagnostics;

    /// <include file='doc\DiscoveryReferences.uex' path='docs/doc[@for="DiscoveryReferenceCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DiscoveryReferenceCollection : CollectionBase {

        /// <include file='doc\DiscoveryReferences.uex' path='docs/doc[@for="DiscoveryReferenceCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DiscoveryReference this[int i] {
            get {
                return (DiscoveryReference) List[i];
            }
            set {
                List[i] = value;
            }
        }

        /// <include file='doc\DiscoveryReferences.uex' path='docs/doc[@for="DiscoveryReferenceCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(DiscoveryReference value) {
            return List.Add(value);
        }

        /// <include file='doc\DiscoveryReferences.uex' path='docs/doc[@for="DiscoveryReferenceCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(DiscoveryReference value) {
            return List.Contains(value);
        }

        /// <include file='doc\DiscoveryReferences.uex' path='docs/doc[@for="DiscoveryReferenceCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(DiscoveryReference value) {
            List.Remove(value);
        }

    }

}
