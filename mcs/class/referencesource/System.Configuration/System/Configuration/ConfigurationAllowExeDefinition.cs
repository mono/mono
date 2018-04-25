//------------------------------------------------------------------------------
// <copyright file="ConfigurationAllowExeDefinition.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {

    public enum ConfigurationAllowExeDefinition {
        MachineOnly          = 0,
        MachineToApplication = 100,
        MachineToRoamingUser = 200,
        MachineToLocalUser   = 300
    }
}
