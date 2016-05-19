//------------------------------------------------------------------------------
// <copyright file="SoapExtensionReflector.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;
    using System.Security.Permissions;

    /// <include file='doc\SoapExtensionReflector.uex' path='docs/doc[@for="SoapExtensionReflector"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public abstract class SoapExtensionReflector {
        ProtocolReflector protocolReflector;

        /// <include file='doc\SoapExtensionReflector.uex' path='docs/doc[@for="SoapExtensionReflector.ReflectMethod"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public abstract void ReflectMethod();

        public virtual void ReflectDescription() {
        }

        /// <include file='doc\SoapExtensionReflector.uex' path='docs/doc[@for="SoapExtensionReflector.ReflectionContext"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ProtocolReflector ReflectionContext {
            get { return protocolReflector; }
            set { protocolReflector = value; }
        }
    }
}
