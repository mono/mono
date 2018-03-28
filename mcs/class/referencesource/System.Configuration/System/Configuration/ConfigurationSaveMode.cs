//------------------------------------------------------------------------------
// <copyright file="ConfigurationSaveMode.cs" company="Microsoft">
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

    // Determines how much of configuration is written out on save.
    public enum ConfigurationSaveMode {
        // If a setting is modified, it'll get written no matter it's
        // same as the parent or not.
        Modified = 0,

        // If a setting is the same as in its parent, it won't get written
        Minimal  = 1,

        // It writes out all the properties in the configurationat that level,
        // including the one from the parents.  Used for writing out the
        // full config settings at a file.
        Full     = 2,
    }
}

