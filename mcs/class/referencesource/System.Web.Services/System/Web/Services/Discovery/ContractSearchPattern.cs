//------------------------------------------------------------------------------
// <copyright file="ContractSearchPattern.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.Security.Permissions;

    /// <include file='doc\ContractSearchPattern.uex' path='docs/doc[@for="ContractSearchPattern"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ContractSearchPattern : DiscoverySearchPattern {
        /// <include file='doc\ContractSearchPattern.uex' path='docs/doc[@for="ContractSearchPattern.Pattern"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Pattern {
            get {
                return "*.asmx";
            }
        }
        /// <include file='doc\ContractSearchPattern.uex' path='docs/doc[@for="ContractSearchPattern.GetDiscoveryReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override DiscoveryReference GetDiscoveryReference(string filename) {
            return new ContractReference(filename + "?wsdl", filename);
        }
    }
 }
