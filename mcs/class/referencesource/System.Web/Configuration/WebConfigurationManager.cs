//------------------------------------------------------------------------------
// <copyright file="WebConfigurationManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Security;
    using System.Security.Permissions;
    using System.Web.Hosting;

    public static class WebConfigurationManager {

        public static NameValueCollection AppSettings {
            get {
                return ConfigurationManager.AppSettings;
            }
        }

        public static ConnectionStringSettingsCollection ConnectionStrings { 
            get {
                return ConfigurationManager.ConnectionStrings;
            }
        }
    
        public static object GetSection(string sectionName) {
            if (HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return HttpConfigurationSystem.GetSection(sectionName);
            }
            else {
                return ConfigurationManager.GetSection(sectionName);
            }
        }

        public static object GetSection(string sectionName, string path) {
            if (HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return HttpConfigurationSystem.GetSection(sectionName, path);
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.Config_GetSectionWithPathArgInvalid));
            }
        }

        public static object GetWebApplicationSection(string sectionName) {
            if (HttpConfigurationSystem.UseHttpConfigurationSystem)  {
                return HttpConfigurationSystem.GetApplicationSection(sectionName);
            }
            else {
                return ConfigurationManager.GetSection(sectionName);
            }
        }

        //
        // *************************************************
        // ** Static Management Functions to edit config **
        // *************************************************
        //

        private static Configuration OpenWebConfigurationImpl(
                WebLevel webLevel, ConfigurationFileMap fileMap, string path, string site, string locationSubPath, 
                string server, string userName, string password, IntPtr userToken) {

            // In the hosted case, we allow app relative (~/....).  Otherwise, it must be absolute
            VirtualPath virtualPath;
            if (HostingEnvironment.IsHosted) {
                virtualPath = VirtualPath.CreateNonRelativeAllowNull(path);
            }
            else {
                virtualPath = VirtualPath.CreateAbsoluteAllowNull(path);
            }

            return WebConfigurationHost.OpenConfiguration(webLevel, fileMap, virtualPath, site, locationSubPath, 
                    server, userName, password, userToken);
        }

        //
        // API extra notes:
        //
        // OpenWebConfiguration(null)
        //  - Open root web.config
        //
        // OpenWebConfiguration("/fxtest");
        //  - If calling from a hosted app (e.g. an ASP.NET page), it will open web.config at 
        //    virtual path "/fxtest" in the current running application (which gives the site)
        //  - If calling from a non-hosted app (e.g. console app), it will do the same thing
        //    except it will use the default web site.
        //
        // OpenWebConfiguration("/", "1", "fxtest")
        //  - Open web.config at the root of site "1" and get the config that applies to location "fxtest"
        //

        //
        // OpenMachineConfiguration
        //
        public static Configuration OpenMachineConfiguration() {
            return OpenWebConfigurationImpl(WebLevel.Machine, null, null, null, null, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenMachineConfiguration(string locationSubPath) {
            return OpenWebConfigurationImpl(WebLevel.Machine, null, null, null, locationSubPath, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenMachineConfiguration(string locationSubPath, string server) {
            return OpenWebConfigurationImpl(WebLevel.Machine, null, null, null, locationSubPath, 
                server, null, null, IntPtr.Zero);
        }

        public static Configuration OpenMachineConfiguration(string locationSubPath, string server, string userName, string password) {
            return OpenWebConfigurationImpl(WebLevel.Machine, null, null, null, locationSubPath, 
                server, userName, password, IntPtr.Zero);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static Configuration OpenMachineConfiguration(string locationSubPath, string server, IntPtr userToken) {
            return OpenWebConfigurationImpl(WebLevel.Machine, null, null, null, locationSubPath, 
                server, null, null, userToken);
        }

        public static Configuration OpenMappedMachineConfiguration(ConfigurationFileMap fileMap) {
            return OpenWebConfigurationImpl(WebLevel.Machine, fileMap, null, null, null, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenMappedMachineConfiguration(ConfigurationFileMap fileMap, string locationSubPath) {  
            return OpenWebConfigurationImpl(WebLevel.Machine, fileMap, null, null, locationSubPath, 
                null, null, null, IntPtr.Zero);
        }

        //
        // OpenWebConfiguration
        //
        public static Configuration OpenWebConfiguration(string path) {
            return OpenWebConfigurationImpl(WebLevel.Path, null, path, null, null, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenWebConfiguration(string path, string site) {
            return OpenWebConfigurationImpl(WebLevel.Path, null, path, site, null, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenWebConfiguration(string path, string site, string locationSubPath) {
            return OpenWebConfigurationImpl(WebLevel.Path, null, path, site, locationSubPath, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenWebConfiguration(string path, string site, string locationSubPath, string server) {
            return OpenWebConfigurationImpl(WebLevel.Path, null, path, site, locationSubPath, 
                server, null, null, IntPtr.Zero);
        }

        public static Configuration OpenWebConfiguration(string path, string site, string locationSubPath, string server, string userName, string password) {
            return OpenWebConfigurationImpl(WebLevel.Path, null, path, site, locationSubPath, 
                server, userName, password, IntPtr.Zero);
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static Configuration OpenWebConfiguration(string path, string site, string locationSubPath, string server, IntPtr userToken) {
            return OpenWebConfigurationImpl(WebLevel.Path, null, path, site, locationSubPath, 
                server, null, null, userToken);
        }

        public static Configuration OpenMappedWebConfiguration(WebConfigurationFileMap fileMap, string path) {
            return OpenWebConfigurationImpl(WebLevel.Path, fileMap, path, null, null, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenMappedWebConfiguration(WebConfigurationFileMap fileMap, string path, string site) {
            return OpenWebConfigurationImpl(WebLevel.Path, fileMap, path, site, null, 
                null, null, null, IntPtr.Zero);
        }

        public static Configuration OpenMappedWebConfiguration(WebConfigurationFileMap fileMap, string path, string site, string locationSubPath) {
            return OpenWebConfigurationImpl(WebLevel.Path, fileMap, path, site, locationSubPath, 
                null, null, null, IntPtr.Zero);
        }
    }
}
