//------------------------------------------------------------------------------
// <copyright file="SoapTransportImporter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {
    using System.CodeDom;
    using System.Security.Permissions;

    /// <include file='doc\SoapTransportImporter.uex' path='docs/doc[@for="SoapTransportImporter"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class SoapTransportImporter {
        SoapProtocolImporter protocolImporter;

        /// <include file='doc\SoapTransportImporter.uex' path='docs/doc[@for="SoapTransportImporter.IsSupportedTransport"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract bool IsSupportedTransport(string transport);
        /// <include file='doc\SoapTransportImporter.uex' path='docs/doc[@for="SoapTransportImporter.ImportClass"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void ImportClass();

        /// <include file='doc\SoapTransportImporter.uex' path='docs/doc[@for="SoapTransportImporter.ImportContext"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SoapProtocolImporter ImportContext {
            get { return protocolImporter; }
            set { protocolImporter = value; }
        }
    }
}
