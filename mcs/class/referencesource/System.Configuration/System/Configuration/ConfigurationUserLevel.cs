//------------------------------------------------------------------------------
// <copyright file="ConfigurationUserLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using ClassConfiguration = System.Configuration.Configuration;
using System.Collections;
using System.Configuration;
using System.Configuration.Internal;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Configuration {

    // Represents which user.config files are included in the configuration.
    public enum ConfigurationUserLevel {
        None                   = 0,
        PerUserRoaming         = 10,
        PerUserRoamingAndLocal = 20,
    }
}

