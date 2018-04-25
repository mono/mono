//------------------------------------------------------------------------------
// <copyright file="HttpConfigurationSystem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Configuration.Internal;
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Xml;
    using System.Web.Util;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    using CultureInfo = System.Globalization.CultureInfo;
    using Debug = System.Web.Util.Debug;
    using UnicodeEncoding = System.Text.UnicodeEncoding;
    using UrlPath = System.Web.Util.UrlPath;

    internal class HttpConfigurationSystem : IInternalConfigSystem {
        private const string InternalConfigSettingsFactoryTypeString = "System.Configuration.Internal.InternalConfigSettingsFactory, " + AssemblyRef.SystemConfiguration;
        internal const string ConfigSystemTypeString = "System.Configuration.Internal.ConfigSystem, " + AssemblyRef.SystemConfiguration;

#if !PLATFORM_UNIX // File system paths must be lowercased in UNIX
        internal const string MachineConfigSubdirectory = "Config";
#else // !PLATFORM_UNIX
        internal const string MachineConfigSubdirectory = "config";
#endif // !PLATFORM_UNIX
        internal const string MachineConfigFilename = "machine.config";
        internal const string RootWebConfigFilename         = "web.config";
        internal const string WebConfigFileName             = "web.config";
        internal const string InetsrvDirectoryName          = "inetsrv";
        internal const string ApplicationHostConfigFileName = "applicationHost.config";

        static private object                           s_initLock;
        static private volatile bool                   s_inited;

        // Supports IInternalConfigSystem and the file change dependency delegate
        static private HttpConfigurationSystem          s_httpConfigSystem;
        static private IConfigSystem                    s_configSystem;

        static private IConfigMapPath                   s_configMapPath;
        static private WebConfigurationHost             s_configHost;
        static private FileChangeEventHandler           s_fileChangeEventHandler;
        static private string                           s_MsCorLibDirectory;
        static private string                           s_MachineConfigurationDirectory;
        static private string                           s_MachineConfigurationFilePath;
        static private string                           s_RootWebConfigurationFilePath;

        static private IInternalConfigRoot              s_configRoot;
        static private IInternalConfigSettingsFactory   s_configSettingsFactory;
        static private bool                             s_initComplete;

        static HttpConfigurationSystem() {
            s_initLock = new object();
        }

        HttpConfigurationSystem() {}

        //
        // Set this configuration system to the default for requests ConfigurationManager.GetSection
        //
        static internal void EnsureInit(IConfigMapPath configMapPath, bool listenToFileChanges, bool initComplete) {
            if (!s_inited) {
                lock (s_initLock) {
                    if (!s_inited) {
                        s_initComplete = initComplete;

                        // Use the IIS map path if one is not explicitly provided
                        if (configMapPath == null) {
                            configMapPath = IISMapPath.GetInstance();
                        }

                        s_configMapPath = configMapPath;

                        Type typeConfigSystem = Type.GetType(ConfigSystemTypeString, true);
                        s_configSystem = (IConfigSystem) Activator.CreateInstance(typeConfigSystem, true);
                        s_configSystem.Init(
                                typeof(WebConfigurationHost),               // The config host we'll create and use
                                // The remaining parameters are passed to the config host:
                                true,                                       // Use the supplied configMapPath
                                s_configMapPath,                            // the configMapPath to use
                                null,                                       // ConfigurationFileMap
                                HostingEnvironment.ApplicationVirtualPath,  // app path
                                HostingEnvironment.SiteNameNoDemand,        // app site name
                                HostingEnvironment.SiteID);                 // app site ID

                        s_configRoot = s_configSystem.Root;
                        s_configHost = (WebConfigurationHost) s_configSystem.Host;

                        // Register for config changed notifications
                        HttpConfigurationSystem configSystem = new HttpConfigurationSystem();

                        if (listenToFileChanges) {
                            s_configRoot.ConfigChanged += new InternalConfigEventHandler(configSystem.OnConfigurationChanged);
                        }

                        // Set the configSystem into the ConfigurationManager class.
                        // Please note that factory.SetConfigurationSystem will end up calling
                        // ConfigurationManager.SetConfigurationSystem, which is an internal static method
                        // in System.Configuration.dll.  If we want to call that here, we have to use
                        // reflection and that's what we want to avoid.
                        Type typeFactory = Type.GetType(InternalConfigSettingsFactoryTypeString, true);
                        s_configSettingsFactory = (IInternalConfigSettingsFactory) Activator.CreateInstance(typeFactory, true);
                        s_configSettingsFactory.SetConfigurationSystem(configSystem, initComplete);

                        // The system has been successfully set, so mark that we should use it.
                        s_httpConfigSystem = configSystem;

                        // Mark as having completed initialization after s_httpConfigSystem has been set.
                        // s_inited is coordinated with s_httpConfigSystem in UseHttpConfigurationSystem.
                        s_inited = true;
                    }
                }
            }

            Debug.Assert(s_httpConfigSystem != null, "s_httpConfigSystem != null - The appdomain is using the client configuration system.");
        }

        static internal void CompleteInit() {
            Debug.Assert(!s_initComplete, "!s_initComplete");
            s_configSettingsFactory.CompleteInit();
            s_configSettingsFactory = null;
        }

        // Return true if the HttpConfigurationSystem is being used
        // by ConfigurationManager.
        static internal bool UseHttpConfigurationSystem {
            get {
                if (!s_inited) {
                    lock (s_initLock) {
                        if (!s_inited) {
                            //
                            // If we ask whether the HttpConfigurationSystem is in use, and it has not
                            // been initialized, then this caller is going to end up using the client
                            // configuration system. So prevent initialization of the HttpConfigurationSystem
                            // by setting s_inited = true.
                            //
                            s_inited = true;
                            Debug.Assert(s_httpConfigSystem == null, "s_httpConfigSystem == null");
                        }
                    }
                }

                return s_httpConfigSystem != null;
            }
        }

        // Return true if the HttpConfigurationSystem is already loaded
        // by ConfigurationManager.
        static internal bool IsSet {
            get {
                return s_httpConfigSystem != null;
            }
        }


        //
        // Return the config object for the current context.
        // If the HttpContext is not available, get the config object
        // for the web application path.
        //
        object IInternalConfigSystem.GetSection(string configKey) {
            return HttpConfigurationSystem.GetSection(configKey);
        }

        // Does not support refresh - the appdomain will restart if
        // a file changes.
        void IInternalConfigSystem.RefreshConfig(string sectionName) {}

        // Supports user config
        bool IInternalConfigSystem.SupportsUserConfig {
            get {
                return false;
            }
        }

        // GetSection
        //
        // Get the Config for the current context
        //
        static internal object GetSection(string sectionName) {
            HttpContext context = HttpContext.Current;
            if (context != null) {
                return context.GetSection(sectionName);
            }
            else {
                // If there is no context, then lets get the config for
                // the application we are hosted in.
                return GetApplicationSection(sectionName);
            }
        }

        // GetSection
        //
        // Get the Config for a specific path
        //
        static internal object GetSection(string sectionName, VirtualPath path) {
            Debug.Assert(UseHttpConfigurationSystem, "UseHttpConfigurationSystem");

            CachedPathData pathData;

            pathData = CachedPathData.GetVirtualPathData(path, true);

            return pathData.ConfigRecord.GetSection(sectionName);
        }

        static internal object GetSection(string sectionName, string path) {
            return GetSection(sectionName, VirtualPath.CreateNonRelativeAllowNull(path));
        }

        // GetAppSection
        //
        // Get the Config for a specific path
        //
        static internal object GetApplicationSection(string sectionName) {
            Debug.Assert(UseHttpConfigurationSystem, "UseHttpConfigurationSystem");

            CachedPathData pathData;

            pathData = CachedPathData.GetApplicationPathData();

            return pathData.ConfigRecord.GetSection(sectionName);
        }


        //
        // Return the unique configuration record for a config path.
        // Used by CachedPathData to retreive config records.
        //
        static internal IInternalConfigRecord GetUniqueConfigRecord(string configPath) {
            if (!UseHttpConfigurationSystem)
                return null;

            IInternalConfigRecord configRecord = s_configRoot.GetUniqueConfigRecord(configPath);
            return configRecord;
        }

        static internal void AddFileDependency(String file) {
            if (String.IsNullOrEmpty(file))
                return;
#if !FEATURE_PAL // No file change notification in Coriolis
            if (UseHttpConfigurationSystem) {
                if (s_fileChangeEventHandler == null) {
                    s_fileChangeEventHandler = new FileChangeEventHandler(s_httpConfigSystem.OnConfigFileChanged);
                }

                HttpRuntime.FileChangesMonitor.StartMonitoringFile(file, s_fileChangeEventHandler);
            }
#endif // !FEATURE_PAL
        }

        internal void OnConfigurationChanged(Object sender, InternalConfigEventArgs e) {
            HttpRuntime.OnConfigChange(message: null);
        }

        internal void OnConfigFileChanged(Object sender, FileChangeEvent e) {
            string message = FileChangesMonitor.GenerateErrorMessage(e.Action, e.FileName);
            HttpRuntime.OnConfigChange(message);
        }

        static internal String MsCorLibDirectory {
            [FileIOPermissionAttribute(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
            get {
                if (s_MsCorLibDirectory == null) {
                    s_MsCorLibDirectory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
                }

                return s_MsCorLibDirectory;
            }
        }

        static internal string MachineConfigurationDirectory {
            get {
                if (s_MachineConfigurationDirectory == null) {
#if !FEATURE_PAL
                    s_MachineConfigurationDirectory = Path.Combine(MsCorLibDirectory, MachineConfigSubdirectory);
#else // !FEATURE_PAL
                    System.UInt32 length = 0;

                    // Get the required size
                    if (!UnsafeNativeMethods.GetMachineConfigurationDirectory(null, ref length)) {
                        throw new System.ComponentModel.Win32Exception();
                    }

                    // Now, create the string and call again
                    System.Text.StringBuilder sb = new System.Text.StringBuilder((int)length);

                    if (!UnsafeNativeMethods.GetMachineConfigurationDirectory(sb, ref length)) {
                        throw new System.ComponentModel.Win32Exception();
                    }

                    s_MachineConfigurationDirectory = sb.ToString();
#endif // !FEATURE_PAL
                }

                return s_MachineConfigurationDirectory;
            }
        }

        static internal string MachineConfigurationFilePath {
            get {
                if (s_MachineConfigurationFilePath == null) {
                    s_MachineConfigurationFilePath = Path.Combine(MachineConfigurationDirectory, MachineConfigFilename);
                }

                return s_MachineConfigurationFilePath;
            }
        }

        static internal string RootWebConfigurationFilePath {
            get {
                if (s_RootWebConfigurationFilePath == null) {
                    s_RootWebConfigurationFilePath = Path.Combine(MachineConfigurationDirectory, RootWebConfigFilename);
                }

                return s_RootWebConfigurationFilePath;
            }
            // for IIS 7, support setting this file path to support the hostable
            // web core and admin scenarios
            set {
                s_RootWebConfigurationFilePath = value;
                if (null == s_RootWebConfigurationFilePath) {
                    s_RootWebConfigurationFilePath = Path.Combine(MachineConfigurationDirectory, RootWebConfigFilename);
                }
            }
        }
    }
}
