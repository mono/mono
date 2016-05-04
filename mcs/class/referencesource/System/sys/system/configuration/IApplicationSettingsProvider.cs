//------------------------------------------------------------------------------
// <copyright file="IApplicationSettingsProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Configuration {

    using System.Security.Permissions;

    /// <devdoc>
    ///     This interface is an extension to SettingsProvider that a provider can implement
    ///     to support additional functionality for settings classes that derive from ApplicationSettingsBase.
    /// </devdoc>
    public interface IApplicationSettingsProvider {
        /// <devdoc>
        ///    Retrieves the previous value of a given SettingsProperty. This is used in conjunction with Upgrade.
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property);

        /// <devdoc>
        ///     Resets all settings to their "default" values.      
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Reset(SettingsContext context);

        /// <devdoc>
        ///    Indicates to the provider that the app has been upgraded. This is a chance for the provider to upgrade
        ///    its stored settings as appropriate.
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Upgrade(SettingsContext context, SettingsPropertyCollection properties);
    }
}

