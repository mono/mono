//------------------------------------------------------------------------------
// <copyright file="LicenseContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies when the licensed object can be used.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class LicenseContext : IServiceProvider
    {

        /// <devdoc>
        ///    <para>When overridden in a derived class, gets a value that specifies when a license can be used.</para>
        /// </devdoc>
        public virtual LicenseUsageMode UsageMode { 
            get {
                return LicenseUsageMode.Runtime;
            }
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class, gets a saved license 
        ///       key for the specified type, from the specified resource assembly.</para>
        /// </devdoc>
        public virtual string GetSavedLicenseKey(Type type, Assembly resourceAssembly) {
            return null;
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class, will return an object that implements the asked for service.</para>
        /// </devdoc>
        public virtual object GetService(Type type) {
            return null;
        }

        /// <devdoc>
        ///    <para>When overridden in a derived class, sets a license key for the specified type.</para>
        /// </devdoc>
        public virtual void SetSavedLicenseKey(Type type, string key) {
            // no-op;
        }
    }
}
