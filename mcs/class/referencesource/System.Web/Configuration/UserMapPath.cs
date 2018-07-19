//------------------------------------------------------------------------------
// <copyright file="UserMapPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Xml;
    using System.Text;
    using System.Web.Util;
    using System.Web.UI;
    using System.IO;
    using System.Web.Hosting;

    //
    // IConfigMapPath that uses information from a ConfigurationFileMap.
    //
    public class UserMapPath : IConfigMapPath {
        string                  _machineConfigFilename;
        string                  _rootWebConfigFilename;
        string                  _siteName;
        string                  _siteID;
        WebConfigurationFileMap _webFileMap;
        bool                    _pathsAreLocal;

        public UserMapPath(ConfigurationFileMap fileMap)
            : this(fileMap, true) {
        }

        internal UserMapPath(ConfigurationFileMap fileMap, bool pathsAreLocal) {
            _pathsAreLocal = pathsAreLocal;


            if (!String.IsNullOrEmpty(fileMap.MachineConfigFilename)) {
                if (_pathsAreLocal) {
                    _machineConfigFilename = Path.GetFullPath(fileMap.MachineConfigFilename);
                }
                else {
                    _machineConfigFilename = fileMap.MachineConfigFilename;
                }
            }

            if (string.IsNullOrEmpty(_machineConfigFilename)) {
                // Defaults for machine.config and root web.config if not supplied by user
                _machineConfigFilename = HttpConfigurationSystem.MachineConfigurationFilePath;
                _rootWebConfigFilename = HttpConfigurationSystem.RootWebConfigurationFilePath;
            } else {
                _rootWebConfigFilename = Path.Combine(Path.GetDirectoryName(_machineConfigFilename), "web.config");
            }

            _webFileMap = fileMap as WebConfigurationFileMap;
            if (_webFileMap != null) {

                // Use the site if supplied, otherwise use the default.
                if (!String.IsNullOrEmpty(_webFileMap.Site)) {
                    _siteName = _webFileMap.Site;
                    _siteID = _webFileMap.Site;
                }
                else {
                    _siteName = WebConfigurationHost.DefaultSiteName;
                    _siteID = WebConfigurationHost.DefaultSiteID;
                }

                if (_pathsAreLocal) {
                    // validate mappings
                    foreach (string virtualDirectory in _webFileMap.VirtualDirectories) {
                        VirtualDirectoryMapping mapping = _webFileMap.VirtualDirectories[virtualDirectory];
                        mapping.Validate();
                    }
                }

                // Get the root web.config path
                VirtualDirectoryMapping rootWebMapping = _webFileMap.VirtualDirectories[null];
                if (rootWebMapping != null) {
                    _rootWebConfigFilename = Path.Combine(rootWebMapping.PhysicalDirectory, rootWebMapping.ConfigFileBaseName);
                    _webFileMap.VirtualDirectories.Remove(null);
                }

            }
        }

        bool IsSiteMatch(string site) {
            return  String.IsNullOrEmpty(site) ||
                StringUtil.EqualsIgnoreCase(site, _siteName) ||
                StringUtil.EqualsIgnoreCase(site, _siteID);
        }

        // Get the VirtualDirectoryMapping for a path by walking the parent hierarchy
        // until found. If onlyApps == true, then only return mappings for appliation roots.
        VirtualDirectoryMapping GetPathMapping(VirtualPath path, bool onlyApps) {
            if (_webFileMap == null) {
                return null;
            }

            string matchPath = path.VirtualPathStringNoTrailingSlash;
            for (;;) {
                VirtualDirectoryMapping mapping = _webFileMap.VirtualDirectories[matchPath];
                if (mapping != null && (!onlyApps || mapping.IsAppRoot)) {
                    return mapping;
                }

                // "/" is the root of the path hierarchy, so it is not found
                if (matchPath == "/") {
                    return null;
                }

                int index = matchPath.LastIndexOf('/');
                if (index == 0) {
                    matchPath = "/";
                }
                else {
                    matchPath = matchPath.Substring(0, index);
                }
            }
        }

        // Given a path and a VirtualDirectoryMapping, return the corresponding
        // physical path.
        string GetPhysicalPathForPath(string path, VirtualDirectoryMapping mapping) {
            string physicalPath;

            int l = mapping.VirtualDirectory.Length;
            if (path.Length == l) {
                physicalPath = mapping.PhysicalDirectory;
            }
            else {
                string childPart;
                if (path[l] == '/') {
                    childPart = path.Substring(l+1);
                }
                else {
                    childPart = path.Substring(l);
                }

                childPart = childPart.Replace('/', '\\');
                physicalPath = Path.Combine(mapping.PhysicalDirectory, childPart);
            }

            // Throw if the resulting physical path is not canonical, to prevent potential
            // security issues (VSWhidbey 418125)
            if (_pathsAreLocal && FileUtil.IsSuspiciousPhysicalPath(physicalPath)) {
                throw new HttpException(SR.GetString(SR.Cannot_map_path, path));
            }

            return physicalPath;
        }

        public string GetMachineConfigFilename() {
            return _machineConfigFilename;
        }

        public string GetRootWebConfigFilename() {
            return _rootWebConfigFilename;
        }

        public void GetPathConfigFilename(
                string siteID, string path, out string directory, out string baseName) {
            GetPathConfigFilename(siteID, VirtualPath.Create(path), out directory, out baseName);
        }

        private void GetPathConfigFilename(
                string siteID, VirtualPath path, out string directory, out string baseName) {

            directory = null;
            baseName = null;

            if (!IsSiteMatch(siteID))
                return;

            VirtualDirectoryMapping mapping = GetPathMapping(path, false);
            if (mapping == null)
                return;

            directory = GetPhysicalPathForPath(path.VirtualPathString, mapping);
            if (directory == null)
                return;

            baseName = mapping.ConfigFileBaseName;
        }

        public void GetDefaultSiteNameAndID(out string siteName, out string siteID) {
            siteName = _siteName;
            siteID = _siteID;
        }

        public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID) {
            if (IsSiteMatch(siteArgument)) {
                siteName = _siteName;
                siteID = _siteID;
            }
            else {
                siteName = siteArgument;
                siteID = null;
            }
        }

        public string MapPath(string siteID, string path) {
            return MapPath(siteID, VirtualPath.Create(path));
        }

        private string MapPath(string siteID, VirtualPath path) {
            string directory, baseName;
            GetPathConfigFilename(siteID, path, out directory, out baseName);
            return directory;
        }

        public string GetAppPathForPath(string siteID, string path) {
            VirtualPath resolved = GetAppPathForPath(siteID, VirtualPath.Create(path));
            if (resolved == null) {
                return null;
            }

            return resolved.VirtualPathString;
        }

        private VirtualPath GetAppPathForPath(string siteID, VirtualPath path) {
            if (!IsSiteMatch(siteID)) {
                return null;
            }

            VirtualDirectoryMapping mapping = GetPathMapping(path, true);
            if (mapping == null) {
                return null;
            }

            return mapping.VirtualDirectoryObject;
        }
    }
}
