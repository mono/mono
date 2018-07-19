//------------------------------------------------------------------------------
// <copyright file="DiscoveryDocumentSearchPattern.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.Security.Permissions;

    /// <include file='doc\DiscoveryDocumentSearchPattern.uex' path='docs/doc[@for="DiscoveryDocumentSearchPattern"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class DiscoveryDocumentSearchPattern : DiscoverySearchPattern {
        /// <include file='doc\DiscoveryDocumentSearchPattern.uex' path='docs/doc[@for="DiscoveryDocumentSearchPattern.Pattern"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Pattern {
            get {
                return "*.vsdisco";
            }
        }

        /// <include file='doc\DiscoveryDocumentSearchPattern.uex' path='docs/doc[@for="DiscoveryDocumentSearchPattern.GetDiscoveryReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override DiscoveryReference GetDiscoveryReference(string filename) {
            return new DiscoveryDocumentReference(filename);
        }
    }  
}
