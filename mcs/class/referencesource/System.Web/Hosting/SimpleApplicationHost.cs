//------------------------------------------------------------------------------
// <copyright file="SimpleApplicationHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;  
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;
    using Microsoft.Win32;
    using Debug = System.Web.Util.Debug;

    internal class SimpleApplicationHost : MarshalByRefObject, IApplicationHost {

        private VirtualPath _appVirtualPath;
        private String _appPhysicalPath;

        internal SimpleApplicationHost(VirtualPath virtualPath, string physicalPath) {

            if (String.IsNullOrEmpty(physicalPath))
                throw ExceptionUtil.ParameterNullOrEmpty("physicalPath");

            // Throw if the physical path is not canonical, to prevent potential
            // security issues (VSWhidbey 418125)
            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath)) {
                throw ExceptionUtil.ParameterInvalid(physicalPath);
            }

            _appVirtualPath = virtualPath;
            _appPhysicalPath = StringUtil.StringEndsWith(physicalPath, "\\") ? physicalPath : physicalPath + "\\";
        }

        public override Object InitializeLifetimeService() {
            return null; // never expire lease
        }

        // IApplicationHost implementation
        public string GetVirtualPath() {
            return _appVirtualPath.VirtualPathString;
        }

        String IApplicationHost.GetPhysicalPath() {
            return _appPhysicalPath;
        }

        IConfigMapPathFactory IApplicationHost.GetConfigMapPathFactory() {
            return new SimpleConfigMapPathFactory();
        }

        IntPtr IApplicationHost.GetConfigToken() {
            return IntPtr.Zero;
        }

        String IApplicationHost.GetSiteName() {
            return WebConfigurationHost.DefaultSiteName;
        }

        String IApplicationHost.GetSiteID() {
            return WebConfigurationHost.DefaultSiteID;
        }

        public void MessageReceived() {
            // nothing
        }
    }

    [Serializable()]
    internal class SimpleConfigMapPathFactory : IConfigMapPathFactory {
        IConfigMapPath IConfigMapPathFactory.Create(string virtualPath, string physicalPath) {
            WebConfigurationFileMap webFileMap = new WebConfigurationFileMap();
            VirtualPath vpath = VirtualPath.Create(virtualPath);

            // Application path
            webFileMap.VirtualDirectories.Add(vpath.VirtualPathStringNoTrailingSlash,
                new VirtualDirectoryMapping(physicalPath, true));

            // Client script file path
            webFileMap.VirtualDirectories.Add(
                    HttpRuntime.AspClientScriptVirtualPath, 
                    new VirtualDirectoryMapping(HttpRuntime.AspClientScriptPhysicalPathInternal, false));

            return new UserMapPath(webFileMap);
        }
    }
}
