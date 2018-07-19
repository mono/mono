//------------------------------------------------------------------------------
// <copyright file="ConfigurationManagerHelperFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;

    //
    // class ConfigurationManagerHelperFactory manages access to a 
    // single instance of ConfigurationManagerHelper.
    //
    static internal class ConfigurationManagerHelperFactory {
        private const string ConfigurationManagerHelperTypeString = "System.Configuration.Internal.ConfigurationManagerHelper, " + AssemblyRef.System;

        static private volatile IConfigurationManagerHelper s_instance;

        static internal IConfigurationManagerHelper Instance {
            get {
                if (s_instance == null) {
                    s_instance = CreateConfigurationManagerHelper();
                }

                return s_instance;
            }
        }

        [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "Hard-coded to create an instance of a specific type.")]
        private static IConfigurationManagerHelper CreateConfigurationManagerHelper() {
            return TypeUtil.CreateInstance<IConfigurationManagerHelper>(ConfigurationManagerHelperTypeString);
        }
    }
}
