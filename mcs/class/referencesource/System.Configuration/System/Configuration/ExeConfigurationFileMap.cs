//------------------------------------------------------------------------------
// <copyright file="ExeConfigurationFileMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Security;
using System.Security.Permissions;


namespace System.Configuration {

    //
    // Holds the configuration file mapping for an Exe.
    //
    public sealed class ExeConfigurationFileMap : ConfigurationFileMap {
        string  _exeConfigFilename;
        string  _roamingUserConfigFilename;
        string  _localUserConfigFilename;

        public ExeConfigurationFileMap() {
            _exeConfigFilename = String.Empty;
            _roamingUserConfigFilename = String.Empty;
            _localUserConfigFilename = String.Empty;
        }

        public ExeConfigurationFileMap(string machineConfigFileName)
                : base(machineConfigFileName) {
            _exeConfigFilename = String.Empty;
            _roamingUserConfigFilename = String.Empty;
            _localUserConfigFilename = String.Empty;
        }

        ExeConfigurationFileMap(string machineConfigFileName, string exeConfigFilename, string roamingUserConfigFilename, string localUserConfigFilename)
                : base(machineConfigFileName) {

            _exeConfigFilename = exeConfigFilename;
            _roamingUserConfigFilename = roamingUserConfigFilename;
            _localUserConfigFilename = localUserConfigFilename;
        }

        public override object Clone() {
            return new ExeConfigurationFileMap(MachineConfigFilename, _exeConfigFilename, _roamingUserConfigFilename, _localUserConfigFilename);
        }

        //
        // The name of the config file for the exe.
        //
        public string ExeConfigFilename {
            get {
                return _exeConfigFilename;
            }

            set {
                _exeConfigFilename = value;
            }
        }

        //
        // The name of the config file for the roaming user.
        //
        public string RoamingUserConfigFilename {
            get {
                return _roamingUserConfigFilename;
            }

            set {
                _roamingUserConfigFilename = value;
            }
        }

        //
        // The name of the config file for the local user.
        //
        public string LocalUserConfigFilename {
            get {
                return _localUserConfigFilename;
            }

            set {
                _localUserConfigFilename = value;
            }
        }
    }
}
