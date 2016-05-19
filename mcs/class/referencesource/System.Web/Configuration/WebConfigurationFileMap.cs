//------------------------------------------------------------------------------
// <copyright file="WebConfigurationFileMap.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Web.Util;
    using System.Security.Permissions;


    //
    // Holds the configuration file mapping for a Web server.
    //
    // Note that multiple paths can be specified, and they do
    // not all have to be along the a single path.
    // This allows the class to be used in the SimpleApplicationHost
    // case, where the path to client ASP.NET files needs to be specified
    // in addition to the application path.
    //
    public sealed class WebConfigurationFileMap : ConfigurationFileMap {
        string                              _site;
        VirtualDirectoryMappingCollection   _virtualDirectoryMapping;

        public WebConfigurationFileMap() {
            _site = String.Empty;
            _virtualDirectoryMapping = new VirtualDirectoryMappingCollection();
        }

        private WebConfigurationFileMap(string machineConfigFileName, string site, VirtualDirectoryMappingCollection VirtualDirectoryMapping)
                : base(machineConfigFileName) {

            _site = site;
            _virtualDirectoryMapping = VirtualDirectoryMapping;
        }

        public WebConfigurationFileMap(string machineConfigFileName)
                : base(machineConfigFileName) {
            _site = String.Empty;
            _virtualDirectoryMapping = new VirtualDirectoryMappingCollection();
        }

        public override object Clone() {
            VirtualDirectoryMappingCollection virtualDirectoryMappingClone = _virtualDirectoryMapping.Clone();

            return new WebConfigurationFileMap(MachineConfigFilename, _site, virtualDirectoryMappingClone);
        }

        //
        // The name of the site.
        // If left unspecified, it will be supplied by the HostingEnvironment.
        // If there is no HostingEnvironment, it defaults to "Default Web Site".
        //
        internal string Site {
            get {
                return _site;
            }

            set {
                if (!WebConfigurationHost.IsValidSiteArgument(value)) {
                    throw ExceptionUtil.PropertyInvalid("Site");
                }

                _site = value;
            }
        }

        //
        // Collection of virtual directory -> physical directory mappings.
        //
        public VirtualDirectoryMappingCollection VirtualDirectories {
            get {
                return _virtualDirectoryMapping;
            }
        }
    }
}
