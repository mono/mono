//------------------------------------------------------------------------------
// <copyright file="SoapExtensionImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {
    using System.CodeDom;
    using System.Security.Permissions;

    /// <include file='doc\SoapExtensionImporter.uex' path='docs/doc[@for="SoapExtensionImporter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public abstract class SoapExtensionImporter {
        SoapProtocolImporter protocolImporter;

        /// <include file='doc\SoapExtensionImporter.uex' path='docs/doc[@for="SoapExtensionImporter.ImportMethod"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void ImportMethod(CodeAttributeDeclarationCollection metadata);

        /// <include file='doc\SoapExtensionImporter.uex' path='docs/doc[@for="SoapExtensionImporter.ImportContext"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapProtocolImporter ImportContext {
            get { return protocolImporter; }
            set { protocolImporter = value; }
        }
    }
}
