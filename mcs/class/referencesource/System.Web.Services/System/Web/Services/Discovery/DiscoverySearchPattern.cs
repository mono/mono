//------------------------------------------------------------------------------
// <copyright file="DiscoverySearchPattern.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.Security.Permissions;

    /// <include file='doc\DiscoverySearchPattern.uex' path='docs/doc[@for="DiscoverySearchPattern"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class DiscoverySearchPattern {
        /// <include file='doc\DiscoverySearchPattern.uex' path='docs/doc[@for="DiscoverySearchPattern.Pattern"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract string Pattern {
            get;
        }

        /// <include file='doc\DiscoverySearchPattern.uex' path='docs/doc[@for="DiscoverySearchPattern.GetDiscoveryReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract DiscoveryReference GetDiscoveryReference(string filename);
    }
}
