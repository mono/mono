//------------------------------------------------------------------------------
// <copyright file="HostingPreferredMapPath.cs" company="Microsoft">
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
    // IConfigMapPath that uses the HostingEnvironment's IConfigMapPath for
    // paths that it maps, and uses the web server IConfigMapPath for
    // all other paths.
    //
    // This allows us to use mappings for an app using SimpleApplicationHost,
    // while still correctly mapping paths outside the app.
    //
    class HostingPreferredMapPath : IConfigMapPath {
        IConfigMapPath  _iisConfigMapPath;
        IConfigMapPath  _hostingConfigMapPath;

        internal static IConfigMapPath GetInstance() {
            IConfigMapPath iisConfigMapPath = IISMapPath.GetInstance();
            IConfigMapPath hostingConfigMapPath = HostingEnvironment.ConfigMapPath;

            // Only delegate if the types implementing IConfigMapPath are different.
            if (hostingConfigMapPath == null || iisConfigMapPath.GetType() == hostingConfigMapPath.GetType())
                return iisConfigMapPath;

            return new HostingPreferredMapPath(iisConfigMapPath, hostingConfigMapPath);
        }

        HostingPreferredMapPath(IConfigMapPath iisConfigMapPath, IConfigMapPath hostingConfigMapPath) {
            _iisConfigMapPath = iisConfigMapPath;
            _hostingConfigMapPath = hostingConfigMapPath;
        }


        public string GetMachineConfigFilename() {
            string filename = _hostingConfigMapPath.GetMachineConfigFilename();
            if (string.IsNullOrEmpty(filename)) {
                filename = _iisConfigMapPath.GetMachineConfigFilename();
            }

            return filename;
        }

        public string GetRootWebConfigFilename() {
            string filename = _hostingConfigMapPath.GetRootWebConfigFilename();
            if (string.IsNullOrEmpty(filename)) {
                filename = _iisConfigMapPath.GetRootWebConfigFilename();
            }

            return filename;
        }

        public void GetPathConfigFilename(
                string siteID, string path, out string directory, out string baseName) {

            _hostingConfigMapPath.GetPathConfigFilename(siteID, path, out directory, out baseName);
            if (string.IsNullOrEmpty(directory)) {
                _iisConfigMapPath.GetPathConfigFilename(siteID, path, out directory, out baseName);
            }
        }

        public void GetDefaultSiteNameAndID(out string siteName, out string siteID) {
            _hostingConfigMapPath.GetDefaultSiteNameAndID(out siteName, out siteID);
            if (string.IsNullOrEmpty(siteID)) {
                _iisConfigMapPath.GetDefaultSiteNameAndID(out siteName, out siteID);
            }
        }

        public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID) {
            _hostingConfigMapPath.ResolveSiteArgument(siteArgument, out siteName, out siteID);
            if (string.IsNullOrEmpty(siteID)) {
                _iisConfigMapPath.ResolveSiteArgument(siteArgument, out siteName, out siteID);
            }
        }

        public string MapPath(string siteID, string path) {
            string physicalPath = _hostingConfigMapPath.MapPath(siteID, path);
            if (string.IsNullOrEmpty(physicalPath)) {
                physicalPath = _iisConfigMapPath.MapPath(siteID, path);
            }

            return physicalPath;
        }

        public string GetAppPathForPath(string siteID, string path) {
            string appPath = _hostingConfigMapPath.GetAppPathForPath(siteID, path);
            if (appPath == null) {
                appPath = _iisConfigMapPath.GetAppPathForPath(siteID, path);
            }

            return appPath;
        }
    }
}
