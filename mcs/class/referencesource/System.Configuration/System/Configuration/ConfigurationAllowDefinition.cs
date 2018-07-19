//------------------------------------------------------------------------------
// <copyright file="ConfigurationAllowDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    public enum ConfigurationAllowDefinition {
        MachineOnly                = 0,
        MachineToWebRoot           = 100,
        MachineToApplication       = 200,
        Everywhere                 = 300,
    }
}
