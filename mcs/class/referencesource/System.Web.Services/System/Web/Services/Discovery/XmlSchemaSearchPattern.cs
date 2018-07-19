//------------------------------------------------------------------------------
// <copyright file="XmlSchemaSearchPattern.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {
    using System;
    using System.Security.Permissions;

    /// <include file='doc\XmlSchemaSearchPattern.uex' path='docs/doc[@for="XmlSchemaSearchPattern"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class XmlSchemaSearchPattern : DiscoverySearchPattern {
        /// <include file='doc\XmlSchemaSearchPattern.uex' path='docs/doc[@for="XmlSchemaSearchPattern.Pattern"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string Pattern {
            get {
                return "*.xsd";
            }
        }

        /// <include file='doc\XmlSchemaSearchPattern.uex' path='docs/doc[@for="XmlSchemaSearchPattern.GetDiscoveryReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override DiscoveryReference GetDiscoveryReference(string filename) {
            return new SchemaReference(filename);
        }
    }
}
