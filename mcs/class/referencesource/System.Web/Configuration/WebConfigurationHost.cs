//------------------------------------------------------------------------------
// <copyright file="WebConfigurationHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#define YES

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration.Internal;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration.Internal;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Xml;
    using System.Text;
    using System.Runtime.InteropServices;
    using Microsoft.Build.Utilities;

    //
    // Configuration host for web applications.
    //
    internal sealed class WebConfigurationHost : DelegatingConfigHost, IInternalConfigWebHost {
        const string InternalHostTypeName = "System.Configuration.Internal.InternalConfigHost, " + AssemblyRef.SystemConfiguration;
        const string InternalConfigConfigurationFactoryTypeName = "System.Configuration.Internal.InternalConfigConfigurationFactory, " + AssemblyRef.SystemConfiguration;

        internal const string           MachineConfigName = "machine";
        internal const string           MachineConfigPath = "machine";
        internal const string           RootWebConfigName = "webroot";
        internal const string           RootWebConfigPath = "machine/webroot";

        internal const char             PathSeparator = '/';
        internal const string           DefaultSiteID = "1";
        private static readonly string  RootWebConfigPathAndPathSeparator = RootWebConfigPath + PathSeparator;
        private static readonly string  RootWebConfigPathAndDefaultSiteID = RootWebConfigPathAndPathSeparator + DefaultSiteID;

        internal static readonly char[]                     s_slashSplit;
        private static IInternalConfigConfigurationFactory  s_configurationFactory;
        private static string                               s_defaultSiteName;

        private Hashtable       _fileChangeCallbacks;   // filename -> arraylist of filechangecallbacks
        private IConfigMapPath  _configMapPath;         // mappath implementation
        private IConfigMapPath2 _configMapPath2;        // mappath implementation that supports VirtualPath
        private VirtualPath     _appPath;               // the application's path
        private string          _appSiteName;           // the application's site name
        private string          _appSiteID;             // the application's site ID
        private string          _appConfigPath;         // the application's configPath
        private string          _machineConfigFile;
        private string          _rootWebConfigFile;


#if DBG
        private bool            _inited;
#endif

         static WebConfigurationHost() {
            s_slashSplit = new char[PathSeparator];
        }

        static internal string DefaultSiteName {
            get {
                if (s_defaultSiteName == null) {
                    s_defaultSiteName = SR.GetString(SR.DefaultSiteName);
                }

                return s_defaultSiteName;
            }
        }

        internal WebConfigurationHost() {
            Type type = Type.GetType(InternalHostTypeName, true);
            Host = (IInternalConfigHost) Activator.CreateInstance(type, true);
        }

        // Not used in runtime because in runtime we have all the siteid, appPath, etc. already.
        static internal void GetConfigPaths(IConfigMapPath configMapPath, WebLevel webLevel, VirtualPath virtualPath, string site, string locationSubPath,
                out VirtualPath appPath, out string appSiteName, out string appSiteID, out string configPath, out string locationConfigPath) {

            appPath = null;
            appSiteName = null;
            appSiteID = null;

            if (webLevel == WebLevel.Machine || virtualPath == null) {
                // Site is meaningless at machine and root web.config level
                // However, we allow a site parameter if the caller is opening
                // a location tag.  See VSWhidbey 548361.
                if (!String.IsNullOrEmpty(site) && String.IsNullOrEmpty(locationSubPath)) {
                    throw ExceptionUtil.ParameterInvalid("site");
                }

                if (webLevel == WebLevel.Machine) {
                    configPath = MachineConfigPath;
                }
                else {
                    configPath = RootWebConfigPath;
                }
            }
            else {
                // Get the site name and ID
                if (!String.IsNullOrEmpty(site)) {
                    configMapPath.ResolveSiteArgument(site, out appSiteName, out appSiteID);

                    if (String.IsNullOrEmpty(appSiteID)) {
                        throw new InvalidOperationException(SR.GetString(SR.Config_failed_to_resolve_site_id, site));
                    }
                }
                else {
                    // If site not supplied, try hosting environment first
                    if (HostingEnvironment.IsHosted) {
                        appSiteName = HostingEnvironment.SiteNameNoDemand;
                        appSiteID = HostingEnvironment.SiteID;
                    }

                    // Rely on defaults if not provided in hosting environment
                    if (String.IsNullOrEmpty(appSiteID)) {
                        configMapPath.GetDefaultSiteNameAndID(out appSiteName, out appSiteID);
                    }

                    Debug.Assert(!String.IsNullOrEmpty(appSiteID), "No appSiteID found when site argument is null");
                }

                configPath = GetConfigPathFromSiteIDAndVPath(appSiteID, virtualPath);
            }

            // get locationConfigPath
            locationConfigPath = null;
            string locationSite = null;
            VirtualPath locationVPath = null;
            if (locationSubPath != null) {
                locationConfigPath = GetConfigPathFromLocationSubPathBasic(configPath, locationSubPath);
                GetSiteIDAndVPathFromConfigPath(locationConfigPath, out locationSite, out locationVPath);

                // If we're at machine or root web.config level and a location path is given,
                // handle the site part of the location path.
                if (String.IsNullOrEmpty(appSiteID) && !String.IsNullOrEmpty(locationSite)) {
                    configMapPath.ResolveSiteArgument(locationSite, out appSiteName, out appSiteID);
                    if (!String.IsNullOrEmpty(appSiteID)) {
                        // Recompose the location config path based on new appSiteID
                        locationConfigPath = GetConfigPathFromSiteIDAndVPath(appSiteID, locationVPath);
                    }
                    else {
                        // If there is no path, then allow the location to be edited,
                        // as we don't need to map elements of the path.
                        if (locationVPath == null || locationVPath.VirtualPathString == "/") {
                            appSiteName = locationSite;
                            appSiteID = locationSite;
                        }
                        // Otherwise, the site argument is ambiguous.
                        else {
                            // 

                            appSiteName = null;
                            appSiteID = null;
                        }
                    }
                }
            }

            // get appPath
            string appPathString = null;
            if (locationVPath != null) {
                appPathString = configMapPath.GetAppPathForPath(appSiteID, locationVPath.VirtualPathString);
            }
            else if (virtualPath != null) {
                appPathString = configMapPath.GetAppPathForPath(appSiteID, virtualPath.VirtualPathString);
            }

            if (appPathString != null) {
                appPath = VirtualPath.Create(appPathString);
            }
        }

        // Choose the implementation of IConfigMapPath to use
        // in cases where it is not provided to us.
        void ChooseAndInitConfigMapPath(bool useConfigMapPath, IConfigMapPath configMapPath, ConfigurationFileMap fileMap) {
            if (useConfigMapPath) {
                _configMapPath = configMapPath;
            }
            else if (fileMap != null) {
                _configMapPath = new UserMapPath(fileMap);
            }
            else if (HostingEnvironment.IsHosted) {
                _configMapPath = HostingPreferredMapPath.GetInstance();
            }
            else {
                _configMapPath = IISMapPath.GetInstance();
            }

            // see if it supports IConfigMapPath2
            _configMapPath2 = _configMapPath as IConfigMapPath2;
        }

        public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams) {
            bool                    useConfigMapPath = (bool)           hostInitParams[0];
            IConfigMapPath          configMapPath = (IConfigMapPath)    hostInitParams[1];
            ConfigurationFileMap    fileMap = (ConfigurationFileMap)    hostInitParams[2];
            string                  appPath = (string)                  hostInitParams[3];
            string                  appSiteName = (string)              hostInitParams[4];
            string                  appSiteID = (string)                hostInitParams[5];

            if (hostInitParams.Length > 6) {
                // If VS sent a 7th param, it is the .Net Framwework Target version moniker
                string fxMoniker = hostInitParams[6] as string;
                _machineConfigFile = GetMachineConfigPathFromTargetFrameworkMoniker(fxMoniker);
                if (!string.IsNullOrEmpty(_machineConfigFile)) {
                    _rootWebConfigFile = Path.Combine(Path.GetDirectoryName(_machineConfigFile), "web.config");
                }
            }

            Debug.Assert(configMapPath == null || useConfigMapPath, "non-null configMapPath without useConfigMapPath == true");

            Host.Init(configRoot, hostInitParams);

            ChooseAndInitConfigMapPath(useConfigMapPath, configMapPath, fileMap);

            appPath = UrlPath.RemoveSlashFromPathIfNeeded(appPath);
            _appPath = VirtualPath.CreateAbsoluteAllowNull(appPath);
            _appSiteName = appSiteName;
            _appSiteID = appSiteID;

            if (!String.IsNullOrEmpty(_appSiteID) && _appPath != null) {
                _appConfigPath = GetConfigPathFromSiteIDAndVPath(_appSiteID, _appPath);
            }

#if DBG
            _inited = true;
#endif
        }

        public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath,
                        IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams) {


            WebLevel                webLevel = (WebLevel)               hostInitConfigurationParams[0];
            ConfigurationFileMap    fileMap = (ConfigurationFileMap)    hostInitConfigurationParams[1];
            VirtualPath             path = VirtualPath.CreateAbsoluteAllowNull((string)hostInitConfigurationParams[2]);
            string                  site = (string)                     hostInitConfigurationParams[3];

            if (locationSubPath == null) {
                locationSubPath = (string)                              hostInitConfigurationParams[4];
            }

            Host.Init(configRoot, hostInitConfigurationParams);

             // Choose the implementation of IConfigMapPath.
            ChooseAndInitConfigMapPath(false, null, fileMap);

            // Get the configuration paths and application information
            GetConfigPaths(_configMapPath, webLevel, path, site, locationSubPath, out _appPath, out _appSiteName, out _appSiteID, out configPath, out locationConfigPath);
            _appConfigPath = GetConfigPathFromSiteIDAndVPath(_appSiteID, _appPath);

            // Verify that the site and path arguments represent a valid name
            // For example, in Cassini app, which is running on \myApp, a page can
            // ask for the config for "\", which can't map to anything.  We want
            // to catch this kind of error.  Another example is if we're given
            // an invalid site id.
            if (IsVirtualPathConfigPath(configPath)) {
                string finalSiteID;
                VirtualPath finalPath;
                GetSiteIDAndVPathFromConfigPath(configPath, out finalSiteID, out finalPath);

                string physicalPath;

                // attempt to use IConfigMapPath2 if provider supports it
                if (null != _configMapPath2) {
                    physicalPath = _configMapPath2.MapPath(finalSiteID, finalPath);
                }
                else {
                    physicalPath = _configMapPath.MapPath(finalSiteID, finalPath.VirtualPathString);
                }

                if (String.IsNullOrEmpty(physicalPath)) {
                    throw new ArgumentOutOfRangeException("site");
                }
            }

#if DBG
            _inited = true;
#endif
        }


        //
        // Utilities to parse config path
        //
        // In both WHIDBEY and ORCAS, Path has the format:
        //          MACHINE/WEBROOT/[Site ID]/[Path Component]/[Path Component]/...
        //

        static internal bool IsMachineConfigPath(string configPath) {
            return configPath.Length == MachineConfigPath.Length;
        }

        static internal bool IsRootWebConfigPath(string configPath) {
            return configPath.Length == RootWebConfigPath.Length;
        }

        // Does the configPath represent a virtual path?
        static internal bool IsVirtualPathConfigPath(string configPath) {
            return configPath.Length > RootWebConfigPath.Length;
        }

        // A site argument that begins or ends in slashes will prevent
        // us from using it in a configPath
        static internal bool IsValidSiteArgument(string site) {
            if (!String.IsNullOrEmpty(site)) {
                char first = site[0];
                char last = site[site.Length - 1];

                if (first == '/' || first == '\\' || last == '/' || last == '\\') {
                    return false;
                }
            }

            return true;
        }

        // Return the virtual path from the configPath.
        static internal string VPathFromConfigPath(string configPath) {
            if (!IsVirtualPathConfigPath(configPath))
                return null;

            // Return the path part after [SiteName]
            int indexStart = RootWebConfigPath.Length + 1;
            int indexVPath = configPath.IndexOf(PathSeparator, indexStart);
            if (indexVPath == -1) {
                return "/";
            }

            return configPath.Substring(indexVPath);
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        void IInternalConfigWebHost.GetSiteIDAndVPathFromConfigPath(string configPath, out string siteID, out string vpath) {
            VirtualPath virtualPath;
            WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out virtualPath);
            vpath = VirtualPath.GetVirtualPathString(virtualPath);
        }

        static internal void GetSiteIDAndVPathFromConfigPath(string configPath, out string siteID, out VirtualPath vpath) {
            if (!IsVirtualPathConfigPath(configPath)) {
                siteID = null;
                vpath = null;
                return;
            }

            int indexStart = RootWebConfigPath.Length + 1;
            int indexVPath = configPath.IndexOf(PathSeparator, indexStart);
            int length;
            if (indexVPath == -1) {
                length = configPath.Length - indexStart;
            }
            else {
                length = indexVPath - indexStart;
            }

            siteID = configPath.Substring(indexStart, length);
            if (indexVPath == -1) {
                vpath = VirtualPath.RootVirtualPath;
            }
            else {
                vpath = VirtualPath.CreateAbsolute(configPath.Substring(indexVPath));
            }
        }

        [AspNetHostingPermission(SecurityAction.Demand, Level=AspNetHostingPermissionLevel.Medium)]
        string IInternalConfigWebHost.GetConfigPathFromSiteIDAndVPath(string siteID, string vpath) {
            return WebConfigurationHost.GetConfigPathFromSiteIDAndVPath(
                siteID, VirtualPath.CreateAbsoluteAllowNull(vpath));
        }

        static internal string GetConfigPathFromSiteIDAndVPath(string siteID, VirtualPath vpath) {
#if DBG
            // Do not inadverte expand app-relative paths using appdomain
            Debug.Assert(vpath == null || vpath.VirtualPathStringIfAvailable != null || vpath.AppRelativeVirtualPathStringIfAvailable == null,
                        "vpath == null || vpath.VirtualPathStringIfAvailable != null || vpath.AppRelativeVirtualPathStringIfAvailable == null");
#endif

            if (vpath == null || string.IsNullOrEmpty(siteID)) {
                return RootWebConfigPath;
            }

            string virtualPath = vpath.VirtualPathStringNoTrailingSlash.ToLower(CultureInfo.InvariantCulture);
            string configPath = (siteID == DefaultSiteID) ? RootWebConfigPathAndDefaultSiteID : RootWebConfigPathAndPathSeparator + siteID;
            if (virtualPath.Length > 1) {
                configPath += virtualPath;
            }
            return configPath;
        }

        static internal string CombineConfigPath(string parentConfigPath, string childConfigPath) {
            if (String.IsNullOrEmpty(parentConfigPath)) {
                return childConfigPath;
            }

            if (String.IsNullOrEmpty(childConfigPath)) {
                return parentConfigPath;
            }

            return parentConfigPath + PathSeparator + childConfigPath;
        }

        public override bool IsConfigRecordRequired(string configPath) {
            // machine.config and root web.config are required records
            if (!IsVirtualPathConfigPath(configPath))
                return true;

            // find the physical translation
            string siteID;
            VirtualPath path;
            GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out path);

            string physicalPath;

            // attempt to use fast path VirtualPath interface
            if (null != _configMapPath2) {
                physicalPath = _configMapPath2.MapPath(siteID, path);
            }
            else {
                physicalPath = _configMapPath.MapPath(siteID, path.VirtualPathString);
            }

            // If the mapping doesn't exist, it may contain children.
            // For example, a Cassini server running application
            // "/myApp" will not have a mapping for "/".
            if (physicalPath == null)
                return true;

            // vpaths that correspond to directories are required
            return FileUtil.DirectoryExists(physicalPath, true);
        }

        // stream support
        public override string GetStreamName(string configPath) {
            if (IsMachineConfigPath(configPath)) {
                return !string.IsNullOrEmpty(_machineConfigFile) ? _machineConfigFile : _configMapPath.GetMachineConfigFilename();
            }
            if (IsRootWebConfigPath(configPath)) {
                return !string.IsNullOrEmpty(_rootWebConfigFile) ? _rootWebConfigFile : _configMapPath.GetRootWebConfigFilename();
            }
            else {
                string siteID;
                VirtualPath path;
                GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out path);

                string directory, baseName;

                // attempt to use fast path interface that takes a VirtualPath
                // to avoid conversion
                if (null != _configMapPath2) {
                    _configMapPath2.GetPathConfigFilename(siteID, path, out directory, out baseName);
                }
                else {
                    _configMapPath.GetPathConfigFilename(siteID, path.VirtualPathString, out directory, out baseName);
                }
                if (directory == null)
                    return null;

                bool exists, isDirectory;
                FileUtil.PhysicalPathStatus(directory, true, false, out exists, out isDirectory);
                if (exists && isDirectory) {
                    // DevDiv Bugs 152256:  Illegal characters {",|} in path prevent configuration system from working.
                    // We need to catch this exception and return null as System.IO.Path.Combine fails to combine paths
                    // containing these characters.
                    // We return null when we have an ArgumentException because we have characters in the parameters
                    // that cannot be part of a filename in Windows. So it is impossible to get a stream name for paths
                    // with these characters. We fallback to the default GetStreamName failure behavior which is to
                    // return null.

                    // Dev10 


                    return CombineAndValidatePath(directory, baseName);
                }

                return null;
            }
        }

        [FileIOPermissionAttribute(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private string CombineAndValidatePath(string directory, string baseName) {
            try {
                string path = Path.Combine(directory, baseName);
                // validate path by calling GetFullPath, but return the result of Path.Combine so as
                // not to change what was being returned previously (Dev10 835901).
                Path.GetFullPath(path);
                return path;
            }
            catch (PathTooLongException) {
                return null;
            }
            catch (NotSupportedException) {
                return null;
            }
            catch (ArgumentException) {
                return null;
            }
        }

        // change notification support - runtime only
        public override bool SupportsChangeNotifications {
            get {return true;}
        }

        private Hashtable FileChangeCallbacks {
            get {
                if (_fileChangeCallbacks == null) {
                    _fileChangeCallbacks = new Hashtable(StringComparer.OrdinalIgnoreCase);
                }

                return _fileChangeCallbacks;
            }
        }

        public override object StartMonitoringStreamForChanges(string streamName, StreamChangeCallback callback) {
            WebConfigurationHostFileChange wrapper;

            // Note: in theory it's possible for multiple config records to monitor the same stream.
            // That's why we use the arraylist to store the callbacks.
            lock (this) {
                wrapper = new WebConfigurationHostFileChange(callback);
                ArrayList list = (ArrayList) FileChangeCallbacks[streamName];
                if (list == null) {
                    list = new ArrayList(1);
                    FileChangeCallbacks.Add(streamName, list);
                }

                list.Add(wrapper);
            }

            HttpRuntime.FileChangesMonitor.StartMonitoringFile(
                    streamName, new FileChangeEventHandler(wrapper.OnFileChanged));

            return wrapper;
        }

        public override void StopMonitoringStreamForChanges(string streamName, StreamChangeCallback callback) {
            WebConfigurationHostFileChange wrapper = null;
            lock (this) {
                ArrayList list = (ArrayList) FileChangeCallbacks[streamName];
                for (int i = 0; i < list.Count; i++) {
                    WebConfigurationHostFileChange item = (WebConfigurationHostFileChange) list[i];
                    if (object.ReferenceEquals(item.Callback, callback)) {
                        wrapper = item;
                        list.RemoveAt(i);
                        if (list.Count == 0) {
                            FileChangeCallbacks.Remove(streamName);
                        }
                        break;
                    }
                }
            }

            HttpRuntime.FileChangesMonitor.StopMonitoringFile(streamName, wrapper);
        }

        public override bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition) {
            switch (allowDefinition) {
                case ConfigurationAllowDefinition.MachineOnly:
                    return configPath.Length <= MachineConfigPath.Length;

                case ConfigurationAllowDefinition.MachineToWebRoot:
                    return configPath.Length <= RootWebConfigPath.Length;

                case ConfigurationAllowDefinition.MachineToApplication:
                    // In some scenarios the host does not have an application path.
                    // Allow all definitions in this case.
                    return  String.IsNullOrEmpty(_appConfigPath) ||
                            (configPath.Length <= _appConfigPath.Length) ||
                            IsApplication(configPath);

                // MachineToLocalUser does not current have any definition restrictions
                case ConfigurationAllowDefinition.Everywhere:
                    return true;

                default:
                    // If we have extended ConfigurationAllowDefinition
                    // make sure to update this switch accordingly
                    throw ExceptionUtil.UnexpectedError("WebConfigurationHost::IsDefinitionAllowed");
            }
        }

        public override void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo) {
            if (!IsDefinitionAllowed(configPath, allowDefinition, allowExeDefinition)) {
                switch (allowDefinition) {
                    case ConfigurationAllowDefinition.MachineOnly:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_allow_definition_error_machine), errorInfo.Filename, errorInfo.LineNumber);

                    case ConfigurationAllowDefinition.MachineToWebRoot:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_allow_definition_error_webroot), errorInfo.Filename, errorInfo.LineNumber);

                    case ConfigurationAllowDefinition.MachineToApplication:
                        throw new ConfigurationErrorsException(SR.GetString(SR.Config_allow_definition_error_application), errorInfo.Filename, errorInfo.LineNumber);

                    default:
                        // If we have extended ConfigurationAllowDefinition
                        // make sure to update this switch accordingly
                        throw ExceptionUtil.UnexpectedError("WebConfigurationHost::VerifyDefinitionAllowed");
                }
            }
        }

        private WebApplicationLevel GetPathLevel(string configPath) {
            if (!IsVirtualPathConfigPath(configPath))
                return WebApplicationLevel.AboveApplication;

#if DBG
            Debug.Assert(_inited, "_inited");
#endif

            // Disable handling of path level when we don't have an application path.
            if (_appPath == null)
                return WebApplicationLevel.AboveApplication;

            string siteID;
            VirtualPath path;
            GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out path);
            if (!StringUtil.EqualsIgnoreCase(_appSiteID, siteID))
                return WebApplicationLevel.AboveApplication;

            if (_appPath == path)
                return WebApplicationLevel.AtApplication;

            if (UrlPath.IsEqualOrSubpath(_appPath.VirtualPathString, path.VirtualPathString))
                return WebApplicationLevel.BelowApplication;

            return WebApplicationLevel.AboveApplication;
        }

        // path support
        public override bool SupportsPath {
            get {
                return true;
            }
        }

        public override bool SupportsLocation {
            get {
                return true;
            }
        }

        public override bool IsAboveApplication(string configPath) {
            return GetPathLevel(configPath) == WebApplicationLevel.AboveApplication;
        }

        static internal string GetConfigPathFromLocationSubPathBasic(string configPath, string locationSubPath) {
            string locationConfigPath;

            if (IsVirtualPathConfigPath(configPath)) {
                locationConfigPath = CombineConfigPath(configPath, locationSubPath);
            }
            else {
                // Location subpaths only apply to virtual paths, not config file roots.
                locationConfigPath = CombineConfigPath(RootWebConfigPath, locationSubPath);
            }

            return locationConfigPath;
        }

        public override string GetConfigPathFromLocationSubPath(string configPath, string locationSubPath) {
#if DBG
            Debug.Assert(_inited, "_inited");
#endif

            string locationConfigPath;

            if (IsVirtualPathConfigPath(configPath)) {
                locationConfigPath = CombineConfigPath(configPath, locationSubPath);
            }
            else {
                string siteID = null;

                // First part of path is the site.
                // If it matches this app's site, use the proper site id,
                // otherwise just use the site as it is given, which will
                // result in it being ignored as desired.
                string site;
                VirtualPath virtualPath;
                int firstSlash = locationSubPath.IndexOf(PathSeparator);
                if (firstSlash < 0) {
                    site = locationSubPath;
                    virtualPath = VirtualPath.RootVirtualPath;
                }
                else {
                    site = locationSubPath.Substring(0, firstSlash);
                    virtualPath = VirtualPath.CreateAbsolute(locationSubPath.Substring(firstSlash));
                }

                if (StringUtil.EqualsIgnoreCase(site, _appSiteID) || StringUtil.EqualsIgnoreCase(site, _appSiteName)) {
                    siteID = _appSiteID;
                }
                else {
                    siteID = site;
                }

                locationConfigPath = GetConfigPathFromSiteIDAndVPath(siteID, virtualPath);
            }

            return locationConfigPath;
        }

        public override bool IsLocationApplicable(string configPath) {
            return IsVirtualPathConfigPath(configPath);
        }

        internal static void StaticGetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady) {
            isHostReady = HttpRuntime.IsTrustLevelInitialized;
            permissionSet = null;
            if (isHostReady && IsVirtualPathConfigPath(configRecord.ConfigPath)) {
                permissionSet = HttpRuntime.NamedPermissionSet;
            }
        }

        // we trust root config files - admins settings do not have security restrictions.
        public override bool IsTrustedConfigPath(string configPath) {
            return !IsVirtualPathConfigPath(configPath);
        }

        public override bool IsFullTrustSectionWithoutAptcaAllowed(IInternalConfigRecord configRecord) {
            if (HostingEnvironment.IsHosted) {
                return HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Unrestricted);
            }
            else {
                return Host.IsFullTrustSectionWithoutAptcaAllowed(configRecord);
            }
        }

        public override void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady) {
            StaticGetRestrictedPermissions(configRecord, out permissionSet, out isHostReady);
        }

        public override IDisposable Impersonate() {
            return new ApplicationImpersonationContext();
        }

        // prefetch support
        public override bool PrefetchAll(string configPath, string streamName) {
            return !IsMachineConfigPath(configPath);
        }

        const string SysWebName = "system.web";
        public override bool PrefetchSection(string sectionGroupName, string sectionName) {
            if (    StringUtil.StringStartsWith(sectionGroupName, SysWebName)
                && (sectionGroupName.Length == SysWebName.Length || sectionGroupName[SysWebName.Length] == '/')) {

                return true;
            }

            if (String.IsNullOrEmpty(sectionGroupName) && sectionName == "system.codedom")
                return true;

            return false;
        }

        // context support
        public override object CreateDeprecatedConfigContext(string configPath) {
            return new HttpConfigurationContext(VPathFromConfigPath(configPath));
        }

        // CreateConfigurationContext
        //
        // Create the ConfigurationContext for ConfigurationElement's
        //
        // Parameters:
        //   configPath      - Config Path we are quering for
        //   locationSubPath - Location SubPath
        //
        public override object CreateConfigurationContext(string configPath, string locationSubPath)
        {
            string              path;
            WebApplicationLevel pathLevel;

            path      = VPathFromConfigPath(configPath);
            pathLevel = GetPathLevel(configPath);

            return new WebContext( pathLevel,         // PathLevel
                                   _appSiteName,      // Site
                                   VirtualPath.GetVirtualPathString(_appPath),          // AppPath
                                   path,              // Path
                                   locationSubPath,   // LocationSubPath
                                   _appConfigPath);   // e.g., "machine/webroot/2/approot"
        }

        // Type name support
        public override Type GetConfigType(string typeName, bool throwOnError) {
            // Go through BuildManager to allow simple references to types in the
            // code directory (VSWhidbey 284498)
            return BuildManager.GetType(typeName, throwOnError);
        }

        public override string GetConfigTypeName(Type t) {
            return BuildManager.GetNormalizedTypeName(t);
        }

        // IsApplication
        //
        // Given a config Path, is it the Path for an application?
        //
        private bool IsApplication(string configPath) {
            VirtualPath appPath;
            string siteID;
            VirtualPath vpath;

            // Break up into siteID and vpath
            GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out vpath);

            // Retrieve appPath for this
            if (null != _configMapPath2) {
                appPath = _configMapPath2.GetAppPathForPath(siteID, vpath);
            }
            else {
                appPath = VirtualPath.CreateAllowNull(_configMapPath.GetAppPathForPath(siteID, vpath.VirtualPathString));
            }

            return (appPath == vpath);
        }

        // Get the factory used to create and initialize Configuration objects.
        static internal IInternalConfigConfigurationFactory ConfigurationFactory {
            [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
            get {
                if (s_configurationFactory == null) {
                    Type type = Type.GetType(InternalConfigConfigurationFactoryTypeName, true);
                    s_configurationFactory = (IInternalConfigConfigurationFactory) Activator.CreateInstance(type, true);
                }

                return s_configurationFactory;
            }
        }

         // Create an instance of a Configuration object.
        // Used by design-time API to open a Configuration object.
        static internal Configuration OpenConfiguration(
                WebLevel webLevel, ConfigurationFileMap fileMap, VirtualPath path, string site, string locationSubPath,
                string server, string userName, string password, IntPtr tokenHandle) {

            Configuration configuration;

            if (!IsValidSiteArgument(site)) {
                throw ExceptionUtil.ParameterInvalid("site");
            }

            locationSubPath = ConfigurationFactory.NormalizeLocationSubPath(locationSubPath, null);

            bool isRemote = !String.IsNullOrEmpty(server)
                && server != "."
                && !StringUtil.EqualsIgnoreCase(server, "127.0.0.1")
                && !StringUtil.EqualsIgnoreCase(server, "::1")
                && !StringUtil.EqualsIgnoreCase(server, "localhost")
                && !StringUtil.EqualsIgnoreCase(server, Environment.MachineName);


            if (isRemote) {
                configuration = ConfigurationFactory.Create(typeof(RemoteWebConfigurationHost),
                    webLevel, null, VirtualPath.GetVirtualPathString(path), site, locationSubPath, server, userName, password, tokenHandle);
            }
            else {
                 if (String.IsNullOrEmpty(server)) {
                    if (!String.IsNullOrEmpty(userName))
                        throw ExceptionUtil.ParameterInvalid("userName");

                    if (!String.IsNullOrEmpty(password))
                        throw ExceptionUtil.ParameterInvalid("password");

                    if (tokenHandle != (IntPtr) 0)
                        throw ExceptionUtil.ParameterInvalid("tokenHandle");
                }

                // Create a copy of the fileMap, so that it cannot be altered by
                // its creator once we start using it.
                if (fileMap != null) {
                    fileMap = (ConfigurationFileMap) fileMap.Clone();
                }

                WebConfigurationFileMap webFileMap = fileMap as WebConfigurationFileMap;
                if (webFileMap != null && !String.IsNullOrEmpty(site)) {
                    webFileMap.Site = site;
                }

                configuration = ConfigurationFactory.Create(typeof(WebConfigurationHost),
                    webLevel, fileMap, VirtualPath.GetVirtualPathString(path), site, locationSubPath );
             }

            return configuration;
        }


        private static string GetMachineConfigPathFromTargetFrameworkMoniker(string moniker) {
            TargetDotNetFrameworkVersion ver = GetTargetFrameworkVersionEnumFromMoniker(moniker);
            if (ver == TargetDotNetFrameworkVersion.VersionLatest)
                return null;

            string machineConfig = ToolLocationHelper.GetPathToDotNetFrameworkFile(@"config\machine.config", ver);
            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, machineConfig).Demand();
            return machineConfig;
        }

        private static TargetDotNetFrameworkVersion GetTargetFrameworkVersionEnumFromMoniker(string moniker)
        {
            // 

            if (moniker.Contains("3.5") || moniker.Contains("3.0") || moniker.Contains("2.0") ) {
                return TargetDotNetFrameworkVersion.Version20;
            }
            return TargetDotNetFrameworkVersion.VersionLatest;
        }
    }
}
