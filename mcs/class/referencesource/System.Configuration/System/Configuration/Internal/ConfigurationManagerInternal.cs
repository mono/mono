//------------------------------------------------------------------------------
// <copyright file="ConfigurationManagerInternal.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration.Internal {

    using System.Configuration;

    internal sealed class ConfigurationManagerInternal : IConfigurationManagerInternal {

        // Created only through reflection
        private ConfigurationManagerInternal() {
        }

        bool IConfigurationManagerInternal.SupportsUserConfig {
            get {
                return ConfigurationManager.SupportsUserConfig;
            }
        }

        bool IConfigurationManagerInternal.SetConfigurationSystemInProgress {
            get {
                return ConfigurationManager.SetConfigurationSystemInProgress;
            }
        }

        string IConfigurationManagerInternal.MachineConfigPath {
            get {
                return ClientConfigurationHost.MachineConfigFilePath;
            }
        }

        string IConfigurationManagerInternal.ApplicationConfigUri {
            get {
                return ClientConfigPaths.Current.ApplicationConfigUri;
            }
        }

        string IConfigurationManagerInternal.ExeProductName {
            get {
                return ClientConfigPaths.Current.ProductName;
            }
        }

        string IConfigurationManagerInternal.ExeProductVersion {
            get {
                return ClientConfigPaths.Current.ProductVersion;
            }
        }

        string IConfigurationManagerInternal.ExeRoamingConfigDirectory {
            get {
                return ClientConfigPaths.Current.RoamingConfigDirectory;
            }
        }

        string IConfigurationManagerInternal.ExeRoamingConfigPath {
            get {
                return ClientConfigPaths.Current.RoamingConfigFilename;
            }
        }

        string IConfigurationManagerInternal.ExeLocalConfigDirectory {
            get {
                return ClientConfigPaths.Current.LocalConfigDirectory;
            }
        }

        string IConfigurationManagerInternal.ExeLocalConfigPath {
            get {
                return ClientConfigPaths.Current.LocalConfigFilename;
            }
        }

        string IConfigurationManagerInternal.UserConfigFilename {
            get {
                return ClientConfigPaths.UserConfigFilename;
            }
        }
    }
}
