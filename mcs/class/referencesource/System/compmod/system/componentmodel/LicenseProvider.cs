//------------------------------------------------------------------------------
// <copyright file="LicenseProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting;
    using System.Security.Permissions;

    /// <include file='doc\LicenseProvider.uex' path='docs/doc[@for="LicenseProvider"]/*' />
    /// <devdoc>
    /// <para>Provides the <see langword='abstract'/> base class for implementing a <see cref='System.ComponentModel.LicenseProvider'/>.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class LicenseProvider
    {

        /// <include file='doc\LicenseProvider.uex' path='docs/doc[@for="LicenseProvider.GetLicense"]/*' />
        /// <devdoc>
        ///    <para>When overridden in a derived class, gets a license for an <paramref name="instance "/>or <paramref name="type "/>
        ///       of component.</para>
        /// </devdoc>
        public abstract License GetLicense(LicenseContext context, Type type, object instance, bool allowExceptions);
    }
}
