//------------------------------------------------------------------------------
// <copyright file="DiscoveryDocumentLinksPattern.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.Security.Permissions;

    /// <include file='doc\DiscoveryDocumentLinksPattern.uex' path='docs/doc[@for="DiscoveryDocumentLinksPattern"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class DiscoveryDocumentLinksPattern : DiscoverySearchPattern {
        /// <include file='doc\DiscoveryDocumentLinksPattern.uex' path='docs/doc[@for="DiscoveryDocumentLinksPattern.Pattern"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Pattern {
            get {
                return "*.disco";
            }
        }

        /// <include file='doc\DiscoveryDocumentLinksPattern.uex' path='docs/doc[@for="DiscoveryDocumentLinksPattern.GetDiscoveryReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override DiscoveryReference GetDiscoveryReference(string filename) {
            return new DiscoveryDocumentReference(filename);
        }
    }
}
