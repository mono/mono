//------------------------------------------------------------------------------
// <copyright file="InternalConfigSettingsFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {

    using System.Configuration;

    //
    // Called by System.Web.HttpConfigurationSystem to set the configuration system 
    // in the ConfigurationSettings class.
    //
    internal sealed class InternalConfigSettingsFactory : IInternalConfigSettingsFactory {
        private InternalConfigSettingsFactory() {}

        void IInternalConfigSettingsFactory.SetConfigurationSystem(IInternalConfigSystem configSystem, bool initComplete) {
            ConfigurationManager.SetConfigurationSystem(configSystem, initComplete);
        }

        void IInternalConfigSettingsFactory.CompleteInit() {
            ConfigurationManager.CompleteConfigInit();
        }
    }
}
