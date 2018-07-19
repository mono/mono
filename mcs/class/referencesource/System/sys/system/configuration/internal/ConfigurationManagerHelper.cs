//------------------------------------------------------------------------------
// <copyright file="ConfigurationManagerHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {

    using System.Configuration;


    [
        // FXCOP: The correct fix would be to make this class static.
        // But a class can't be static and sealed at the same time.
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")
    ]
    internal sealed class ConfigurationManagerHelper : IConfigurationManagerHelper {
        // Created only through reflection.
        private ConfigurationManagerHelper() {
        }

        void IConfigurationManagerHelper.EnsureNetConfigLoaded() {
            System.Net.Configuration.SettingsSection.EnsureConfigLoaded();
        }
    }
}
