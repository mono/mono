//------------------------------------------------------------------------------
// <copyright file="IConfigurationSystem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    // obsolete
    [System.Runtime.InteropServices.ComVisible(false)]
    public interface IConfigurationSystem {
        // Returns the config object for the specified key.
        object GetConfig(string configKey);

        // Initializes the configuration system.
        void Init();
    }
}
