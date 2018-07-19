//------------------------------------------------------------------------------
// <copyright file="ConfigurationFileMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Security.Permissions;

namespace System.Configuration {

    //
    // Holds the configuration file mapping for
    // machine.config. It is the base class for
    // ExeConfigurationFileMap and WebConfigurationFileMap.
    //
    public class ConfigurationFileMap : ICloneable {
        // DevDiv #407902 - This used to be two fields: one containing the filename and the other containing
        // a Boolean dictating whether a security check needed to take place. Such a pattern wasn't thread-safe
        // and could be circumvented by malicious callers. Using a single reference field is guaranteed atomic
        // read and write across all platforms, so the race condition is eliminated.
        private Func<string> _getFilenameThunk;

        public ConfigurationFileMap() {
            _getFilenameThunk = GetFilenameFromMachineConfigFilePath;
        }

        public ConfigurationFileMap(string machineConfigFilename) {
            if (string.IsNullOrEmpty(machineConfigFilename))
                throw new ArgumentNullException("machineConfigFilename");
            if (!File.Exists(machineConfigFilename))
                throw new ArgumentException(SR.GetString(SR.Machine_config_file_not_found, machineConfigFilename), "machineConfigFilename");

            MachineConfigFilename = machineConfigFilename;
        }

        // copy ctor used only for cloning
        private ConfigurationFileMap(ConfigurationFileMap other) {
            _getFilenameThunk = other._getFilenameThunk;
        }

        public virtual object Clone() {
            return new ConfigurationFileMap(this);
        }

        private static string GetFilenameFromMachineConfigFilePath() {
            string filename = ClientConfigurationHost.MachineConfigFilePath;
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, filename).Demand();
            return filename;
        }

        //
        // The name of machine.config.
        //
        public string MachineConfigFilename {
            get {
                return _getFilenameThunk();
            }
            set {
                _getFilenameThunk = () => value;
            }
        }
    }
}
