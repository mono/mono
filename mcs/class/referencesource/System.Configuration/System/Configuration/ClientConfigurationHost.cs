//------------------------------------------------------------------------------
// <copyright file="ClientConfigurationHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.IO;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Threading;
    using System.Security;
    using System.Net;
    using System.Security.Principal;
    using System.Diagnostics.CodeAnalysis;

    internal sealed class ClientConfigurationHost : DelegatingConfigHost, IInternalConfigClientHost {
        internal const string MachineConfigName = "MACHINE";
        internal const string ExeConfigName = "EXE";
        internal const string RoamingUserConfigName = "ROAMING_USER";
        internal const string LocalUserConfigName = "LOCAL_USER";

        internal const string MachineConfigPath = MachineConfigName;
        internal const string ExeConfigPath = MachineConfigPath + "/" + ExeConfigName;
        internal const string RoamingUserConfigPath = ExeConfigPath + "/" + RoamingUserConfigName;
        internal const string LocalUserConfigPath = RoamingUserConfigPath + "/" + LocalUserConfigName;

        private const string ConfigExtension = ".config";
        private const string MachineConfigFilename = "machine.config";
        private const string MachineConfigSubdirectory = "Config";

        private static object                   s_init = new object();
        private static object                   s_version = new object();
        private static volatile string          s_machineConfigFilePath;

        private string                          _exePath;       // the physical path to the exe being configured
        private ClientConfigPaths               _configPaths;   // physical paths to client config files
        private ExeConfigurationFileMap         _fileMap;       // optional file map
        private bool                            _initComplete;

        internal ClientConfigurationHost() {
            Host = new InternalConfigHost();
        }

        internal ClientConfigPaths ConfigPaths {
            get {
                if (_configPaths == null) {
                    _configPaths = ClientConfigPaths.GetPaths(_exePath, _initComplete);
                }

                return _configPaths;
            }
        }

        internal void RefreshConfigPaths() {
            // Refresh current config paths.
            if (_configPaths != null && !_configPaths.HasEntryAssembly && _exePath == null) {
                ClientConfigPaths.RefreshCurrent();
                _configPaths = null;
            }
        }

        static internal string MachineConfigFilePath {
            [FileIOPermissionAttribute(SecurityAction.Assert, AllFiles = FileIOPermissionAccess.PathDiscovery)]
            [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "The callers do not expose this information without performing the appropriate demands themselves.")]
            get {
                if (s_machineConfigFilePath == null) {
                    string directory = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
                    s_machineConfigFilePath = Path.Combine(Path.Combine(directory, MachineConfigSubdirectory), MachineConfigFilename);
                }

                return s_machineConfigFilePath;
            }
        }

        internal bool HasRoamingConfig {
            get {
                if (_fileMap != null) {
                    return !String.IsNullOrEmpty(_fileMap.RoamingUserConfigFilename);
                }
                else {
                    return ConfigPaths.HasRoamingConfig;
                }
            }
        }

        internal bool HasLocalConfig {
            get {
                if (_fileMap != null) {
                    return !String.IsNullOrEmpty(_fileMap.LocalUserConfigFilename);
                }
                else {
                    return ConfigPaths.HasLocalConfig;
                }
            }
        }

        internal bool IsAppConfigHttp {
            get {
                return !IsFile(GetStreamName(ExeConfigPath));
            }
        }

        // IInternalConfigClientHost methods are used by Venus and Whitehorse 
        // so as not to require explicit knowledge of the contents of the
        // config path.

        // return true if the config path is for an exe config, false otherwise.
        bool IInternalConfigClientHost.IsExeConfig(string configPath) {
            return StringUtil.EqualsIgnoreCase(configPath, ExeConfigPath);
        }

        bool IInternalConfigClientHost.IsRoamingUserConfig(string configPath) {
            return StringUtil.EqualsIgnoreCase(configPath, RoamingUserConfigPath);
        }

        bool IInternalConfigClientHost.IsLocalUserConfig(string configPath) {
            return StringUtil.EqualsIgnoreCase(configPath, LocalUserConfigPath);
        }

        // Return true if the config path is for a user.config file, false otherwise.
        private bool IsUserConfig(string configPath) {
            return StringUtil.EqualsIgnoreCase(configPath, RoamingUserConfigPath) ||
                   StringUtil.EqualsIgnoreCase(configPath, LocalUserConfigPath);
        }

        string IInternalConfigClientHost.GetExeConfigPath() {
            return ExeConfigPath;
        }

        string IInternalConfigClientHost.GetRoamingUserConfigPath() {
            return RoamingUserConfigPath;
        }

        string IInternalConfigClientHost.GetLocalUserConfigPath() {
            return LocalUserConfigPath;
        }

        public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams) {
            try {
                ConfigurationFileMap fileMap = (ConfigurationFileMap)   hostInitParams[0];
                _exePath = (string)                                     hostInitParams[1];

                Host.Init(configRoot, hostInitParams);

                // Do not complete initialization in runtime config, to avoid expense of 
                // loading user.config files that may not be required.
                _initComplete = configRoot.IsDesignTime;

                if (fileMap != null && !String.IsNullOrEmpty(_exePath)) {
                    throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::Init");
                }

                if (String.IsNullOrEmpty(_exePath)) {
                    _exePath = null;
                }

                // Initialize the fileMap, if provided.
                if (fileMap != null) {
                    _fileMap = new ExeConfigurationFileMap();
                    if (!String.IsNullOrEmpty(fileMap.MachineConfigFilename)) {
                        _fileMap.MachineConfigFilename = Path.GetFullPath(fileMap.MachineConfigFilename);
                    }

                    ExeConfigurationFileMap exeFileMap = fileMap as ExeConfigurationFileMap;
                    if (exeFileMap != null) {
                        if (!String.IsNullOrEmpty(exeFileMap.ExeConfigFilename)) {
                            _fileMap.ExeConfigFilename = Path.GetFullPath(exeFileMap.ExeConfigFilename);
                        }

                        if (!String.IsNullOrEmpty(exeFileMap.RoamingUserConfigFilename)) {
                            _fileMap.RoamingUserConfigFilename = Path.GetFullPath(exeFileMap.RoamingUserConfigFilename);
                        }

                        if (!String.IsNullOrEmpty(exeFileMap.LocalUserConfigFilename)) {
                            _fileMap.LocalUserConfigFilename = Path.GetFullPath(exeFileMap.LocalUserConfigFilename); 
                        }
                    }
                }
            }
            catch (SecurityException) {
                // Lets try to give them some information telling them 
                // they don't have enough security privileges
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Config_client_config_init_security));
            }
            catch {
                throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::Init");
            }
        }

        public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, 
                IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams) {
            
            locationSubPath = null;
            configPath = (string) hostInitConfigurationParams[2];
            locationConfigPath = null;

            Init(configRoot, hostInitConfigurationParams);
        }

        // Delay init if we have not been asked to complete init, and it is a user.config file.
        public override bool IsInitDelayed(IInternalConfigRecord configRecord) {
            return !_initComplete && IsUserConfig(configRecord.ConfigPath);
        }

        public override void RequireCompleteInit(IInternalConfigRecord record) {
            // Loading information about user.config files is expensive, 
            // so do it just once by locking.
            lock (this) {
                if (!_initComplete) {
                    // Note that all future requests for config must be complete.
                    _initComplete = true;

                    // Throw out the ConfigPath for this exe.
                    ClientConfigPaths.RefreshCurrent();

                    // Throw out our cached copy.
                    _configPaths = null;

                    // Force loading of user.config file information under lock.
                    ClientConfigPaths configPaths = ConfigPaths;
                }
            }
        }

        // config path support
        public override bool IsConfigRecordRequired(string configPath) {
            string configName = ConfigPathUtility.GetName(configPath);
            switch (configName) {
                default:
                    // should never get here
                    return false;

                case MachineConfigName:
                case ExeConfigName:
                    return true;

                case RoamingUserConfigName:
                    // Makes the design easier even if we only have an empty Roaming config record.
                    return HasRoamingConfig || HasLocalConfig;

                case LocalUserConfigName:
                    return HasLocalConfig;
            }
        }

        // stream support
        public override string GetStreamName(string configPath) {
            string configName = ConfigPathUtility.GetName(configPath);
            if (_fileMap != null) {
                switch (configName) {
                    default:
                        // should never get here
                        goto case MachineConfigName;

                    case MachineConfigName:
                        return _fileMap.MachineConfigFilename;

                    case ExeConfigName:
                        return _fileMap.ExeConfigFilename;

                    case RoamingUserConfigName:
                        return _fileMap.RoamingUserConfigFilename;

                    case LocalUserConfigName:
                        return _fileMap.LocalUserConfigFilename;
                }
            }
            else {
                switch (configName) {
                    default:
                        // should never get here
                        goto case MachineConfigName;

                    case MachineConfigName:
                        return MachineConfigFilePath;

                    case ExeConfigName:
                        return ConfigPaths.ApplicationConfigUri;

                    case RoamingUserConfigName:
                        return ConfigPaths.RoamingConfigFilename;

                    case LocalUserConfigName:
                        return ConfigPaths.LocalConfigFilename;
                }
            }
        }

        public override string GetStreamNameForConfigSource(string streamName, string configSource) {
            if (IsFile(streamName)) {
                return Host.GetStreamNameForConfigSource(streamName, configSource);
            }

            int index = streamName.LastIndexOf('/');
            if (index < 0)
                return null;

            string parentUri = streamName.Substring(0, index + 1);
            string result = parentUri + configSource.Replace('\\', '/');

            return result;
        }

        public override object GetStreamVersion(string streamName) {
            if (IsFile(streamName)) {
                return Host.GetStreamVersion(streamName);
            }

            // assume it is the same
            return s_version;
        }


        // default impl treats name as a file name
        // null means stream doesn't exist for this name
        public override Stream OpenStreamForRead(string streamName) {
            // the streamName can either be a file name, or a URI
            if (IsFile(streamName)) {
                return Host.OpenStreamForRead(streamName);
            }

            if (streamName == null) {
                return null;
            }
            
            // scheme is http
            WebClient client = new WebClient();

            // Try using default credentials
            try {
                client.Credentials = CredentialCache.DefaultCredentials;
            }
            catch {
            }

            byte[] fileData = null;
            try {
                fileData = client.DownloadData(streamName);
            }
            catch {
            }

            if (fileData == null) {
                return null;
            }

            MemoryStream stream = new MemoryStream(fileData);
            return stream;
        }

        public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext) {
            // only support files, not URIs
            if (!IsFile(streamName)) {
                throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::OpenStreamForWrite");
            }

            return Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
        }

        public override void DeleteStream(string streamName) {
            // only support files, not URIs
            if (!IsFile(streamName)) {
                throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::Delete");
            }

            Host.DeleteStream(streamName);
        }

        // RefreshConfig support - runtime only
        public override bool SupportsRefresh {
            get {return true;}
        }

        // path support
        public override bool SupportsPath {
            get {return false;}
        }

        // Do we support location tags?
        public override bool SupportsLocation {
            get {return false;}
        }

        public override bool IsDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition) {
            string allowedConfigPath;

            switch (allowExeDefinition) {
                case ConfigurationAllowExeDefinition.MachineOnly:
                    allowedConfigPath = MachineConfigPath;
                    break;

                case ConfigurationAllowExeDefinition.MachineToApplication:
                    allowedConfigPath = ExeConfigPath;
                    break;

                case ConfigurationAllowExeDefinition.MachineToRoamingUser:
                    allowedConfigPath = RoamingUserConfigPath;
                    break;

                // MachineToLocalUser does not current have any definition restrictions
                case ConfigurationAllowExeDefinition.MachineToLocalUser:
                    return true;

                default:
                    // If we have extended ConfigurationAllowExeDefinition
                    // make sure to update this switch accordingly
                    throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::IsDefinitionAllowed");
            }

            return configPath.Length <= allowedConfigPath.Length;
        }

        public override void VerifyDefinitionAllowed(string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo) {
            if (!IsDefinitionAllowed(configPath, allowDefinition, allowExeDefinition)) {
                switch (allowExeDefinition) {
                    case ConfigurationAllowExeDefinition.MachineOnly:
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_allow_exedefinition_error_machine), errorInfo);

                    case ConfigurationAllowExeDefinition.MachineToApplication:
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_allow_exedefinition_error_application), errorInfo);

                    case ConfigurationAllowExeDefinition.MachineToRoamingUser:
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_allow_exedefinition_error_roaminguser), errorInfo);

                    default:
                        // If we have extended ConfigurationAllowExeDefinition
                        // make sure to update this switch accordingly
                        throw ExceptionUtil.UnexpectedError("ClientConfigurationHost::VerifyDefinitionAllowed");
                }
            }
        }

        // prefetch support
        public override bool PrefetchAll(string configPath, string streamName) {
            // If it's a file, we don't need to.  Otherwise (e.g. it's from the web), we'll prefetch everything.
            return !IsFile(streamName);
        }

        public override bool PrefetchSection(string sectionGroupName, string sectionName) {
            return sectionGroupName == "system.net";
        }

        // we trust machine.config - admins settings do not have security restrictions.
        public override bool IsTrustedConfigPath(string configPath) {
            return configPath == MachineConfigPath;
        }

        [SecurityPermission(SecurityAction.Assert, ControlEvidence=true)]
        public override void GetRestrictedPermissions(IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady) {
            // Get the stream name as a URL
            string url;
            bool isFile = IsFile(configRecord.StreamName);
            if (isFile) {
                url = UrlPath.ConvertFileNameToUrl(configRecord.StreamName);
            }
            else {
                url = configRecord.StreamName;
            }

            Evidence evidence = new Evidence();
            
            // Add Url evidence, which is simply the URL.
            evidence.AddHostEvidence(new Url(url));

            // Add Zone evidence - My Computer, Intranet, Internet, etc.
            evidence.AddHostEvidence(Zone.CreateFromUrl(url));

            // Add Site evidence if the url is http.
            if (!isFile) {
                evidence.AddHostEvidence(Site.CreateFromUrl(url));
            }

            // Get the resulting permission set.
            permissionSet = SecurityManager.GetStandardSandbox(evidence);

            // Client host is always ready to return permissions.
            isHostReady = true;
        }

        //
	// Impersonate for Client Config
        // Use the process identity
        //
        [SecurityPermissionAttribute(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.UnmanagedCode)]
        public override IDisposable Impersonate() {
            // Use the process identity
            return WindowsIdentity.Impersonate(IntPtr.Zero);
        }

	// context support
        public override object CreateDeprecatedConfigContext(string configPath) {
            return null;
        }

        // CreateConfigurationContext
        //
        // Create the new context
        //
        public override object 
        CreateConfigurationContext( string configPath,
                                    string locationSubPath )
        {
            return new ExeContext(GetUserLevel(configPath), ConfigPaths.ApplicationUri);
        }
        
        // GetUserLevel
        //
        // Given a configPath, determine what the user level is?
        //
        private ConfigurationUserLevel GetUserLevel(string configPath)
        {
            ConfigurationUserLevel level;
            
            switch (ConfigPathUtility.GetName(configPath)) {
                case MachineConfigName:
                    // Machine Level
                    level = ConfigurationUserLevel.None;
                    break;

                case ExeConfigName:
                    // Exe Level
                    level = ConfigurationUserLevel.None;
                    break;

                case LocalUserConfigName:
                    // User Level
                    level = ConfigurationUserLevel.PerUserRoamingAndLocal;
                    break;
                    
                case RoamingUserConfigName:
                    // Roaming Level
                    level = ConfigurationUserLevel.PerUserRoaming;
                    break;

                default:
                    Debug.Fail("unrecognized configPath " + configPath);
                    level = ConfigurationUserLevel.None;
                    break;
            }

            return level;
        }

        //
        // Create a Configuration object.
        //
        static internal Configuration OpenExeConfiguration(ConfigurationFileMap fileMap, bool isMachine, ConfigurationUserLevel userLevel, string exePath) {
            // validate userLevel argument
            switch (userLevel) {
                default:
                    throw ExceptionUtil.ParameterInvalid("userLevel");

                case ConfigurationUserLevel.None:
                case ConfigurationUserLevel.PerUserRoaming:
                case ConfigurationUserLevel.PerUserRoamingAndLocal:
                    break;
            }

            // validate fileMap arguments
            if (fileMap != null) {
                if (String.IsNullOrEmpty(fileMap.MachineConfigFilename)) {
                    throw ExceptionUtil.ParameterNullOrEmpty("fileMap.MachineConfigFilename");
                }

                ExeConfigurationFileMap exeFileMap = fileMap as ExeConfigurationFileMap;
                if (exeFileMap != null) {
                    switch (userLevel) {
                        case ConfigurationUserLevel.None:
                            if (String.IsNullOrEmpty(exeFileMap.ExeConfigFilename)) {
                                throw ExceptionUtil.ParameterNullOrEmpty("fileMap.ExeConfigFilename");
                            }

                            break;

                        case ConfigurationUserLevel.PerUserRoaming:
                            if (String.IsNullOrEmpty(exeFileMap.RoamingUserConfigFilename)) {
                                throw ExceptionUtil.ParameterNullOrEmpty("fileMap.RoamingUserConfigFilename");
                            }

                            goto case ConfigurationUserLevel.None;

                        case ConfigurationUserLevel.PerUserRoamingAndLocal:
                            if (String.IsNullOrEmpty(exeFileMap.LocalUserConfigFilename)) {
                                throw ExceptionUtil.ParameterNullOrEmpty("fileMap.LocalUserConfigFilename");
                            }

                            goto case ConfigurationUserLevel.PerUserRoaming;
                    }
                }
            }

            string configPath = null;
            if (isMachine) {
                configPath = MachineConfigPath;
            }
            else {
                switch (userLevel) {
                    case ConfigurationUserLevel.None:
                        configPath = ExeConfigPath;
                        break;

                    case ConfigurationUserLevel.PerUserRoaming:
                        configPath = RoamingUserConfigPath;
                        break;

                    case ConfigurationUserLevel.PerUserRoamingAndLocal:
                        configPath = LocalUserConfigPath;
                        break;
                }
            }

            Configuration configuration = new Configuration(null, typeof(ClientConfigurationHost), fileMap, exePath, configPath);

            return configuration;
        }
    }
}
