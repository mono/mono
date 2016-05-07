//------------------------------------------------------------------------------
// <copyright file="DeploymentSectionCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Configuration;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Configuration;

    // DeploymentSection can only be defined in machine.config, so it is safe to cache statically in the application
    internal sealed class DeploymentSectionCache : IDeploymentSection {
        private static readonly DeploymentSectionCache _instance = new DeploymentSectionCache();
        // Value is cached statically, because DeploymentSectionCache is a Singleton.
        private bool? _retail;

        private DeploymentSectionCache() {
        }

        public static DeploymentSectionCache Instance {
            get {
                return _instance;
            }
        }

        public bool Retail {
            get {
                if (_retail == null) {
                    _retail = GetRetailFromConfig();
                }
                return _retail.Value;
            }
        }

        [
        ConfigurationPermission(SecurityAction.Assert, Unrestricted = true),
        SecuritySafeCritical()
        ]
        private static bool GetRetailFromConfig() {
            DeploymentSection section = (DeploymentSection)WebConfigurationManager.GetSection("system.web/deployment");
            return section.Retail;
        }
    }
}
