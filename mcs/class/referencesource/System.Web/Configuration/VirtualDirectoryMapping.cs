//------------------------------------------------------------------------------
// <copyright file="VirtualDirectoryMapping.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.IO;
    using System.Web.Util;
    using System.Security.Permissions;

    //
    // Maps a virtual directory to a physical directory and its config file.
    //
    public sealed class VirtualDirectoryMapping {
        VirtualPath  _virtualDirectory;
        string  _physicalDirectory;
        string  _configFileBaseName;
        bool    _isAppRoot;

        const string DEFAULT_BASE_NAME = "web.config";

        public VirtualDirectoryMapping(string physicalDirectory, bool isAppRoot) 
            : this(null, physicalDirectory, isAppRoot, DEFAULT_BASE_NAME) {

        }

        public VirtualDirectoryMapping(string physicalDirectory, bool isAppRoot, string configFileBaseName) 
            : this(null, physicalDirectory, isAppRoot, configFileBaseName) {
        }

        private VirtualDirectoryMapping(VirtualPath virtualDirectory, string physicalDirectory, bool isAppRoot, string configFileBaseName) {
            _virtualDirectory = virtualDirectory;
            _isAppRoot = isAppRoot;

            PhysicalDirectory = physicalDirectory;
            ConfigFileBaseName = configFileBaseName;
        }

        internal VirtualDirectoryMapping Clone() {
            return new VirtualDirectoryMapping(_virtualDirectory, _physicalDirectory, _isAppRoot, _configFileBaseName);
        }

        //
        // Get the virtual directory.
        // Not settable because it is set when it is added to a collection.
        //
        public string VirtualDirectory {
            get {
                return (_virtualDirectory != null) ? _virtualDirectory.VirtualPathString : string.Empty;
            }
        }

        internal VirtualPath VirtualDirectoryObject {
            get {
                return _virtualDirectory;
            }
        }

        internal void SetVirtualDirectory(VirtualPath virtualDirectory) {
            _virtualDirectory = virtualDirectory;
        }

        //
        // The physical directory.
        //
        public string PhysicalDirectory {
            get {
                return _physicalDirectory;
            }

            set {
                string physicalDirectory = value;
                if (String.IsNullOrEmpty(physicalDirectory)) {
                    physicalDirectory = null;
                }
                else {
                    // remove trailing '\' if any
                    if (UrlPath.PathEndsWithExtraSlash(physicalDirectory)) {
                        physicalDirectory = physicalDirectory.Substring(0, physicalDirectory.Length - 1);
                    }

                    // Throw if the resulting physical path is not canonical, to prevent potential
                    // security issues (VSWhidbey 418125)
                    if (FileUtil.IsSuspiciousPhysicalPath(physicalDirectory)) {
                        throw ExceptionUtil.ParameterInvalid("PhysicalDirectory");
                    }
                }

                _physicalDirectory = physicalDirectory;
            }
        }

        //
        // Indicates whether the virtual directory is the location of an application.
        //
        public bool IsAppRoot {
            get {
                return _isAppRoot;
            }

            set {
                _isAppRoot = value;
            }

        }

        //
        // The base name of the config file.
        // If not specified, "web.config" is used.
        //
        public string ConfigFileBaseName {
            get {
                return _configFileBaseName;
            }

            set {
                if (string.IsNullOrEmpty(value)) {
                    throw ExceptionUtil.PropertyInvalid("ConfigFileBaseName");
                }

                _configFileBaseName = value;
            }
        }

        internal void Validate() {
            if (_physicalDirectory != null) {
                //
                // Ensure that the caller has PathDiscovery to the resulting config file,
                // and that the web.config file does not have ".." that could lead to a 
                // different directory.
                //
                string configFilename = Path.Combine(_physicalDirectory, _configFileBaseName);
                string fullConfigFilename = Path.GetFullPath(configFilename);
                if (    Path.GetDirectoryName(fullConfigFilename) != _physicalDirectory ||
                        Path.GetFileName(fullConfigFilename) != _configFileBaseName ||
                        FileUtil.IsSuspiciousPhysicalPath(configFilename)) {

                    throw ExceptionUtil.ParameterInvalid("configFileBaseName");
                }
            }
        }
    }
}
