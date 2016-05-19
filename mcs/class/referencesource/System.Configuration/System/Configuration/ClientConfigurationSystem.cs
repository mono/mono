//------------------------------------------------------------------------------
// <copyright file="ClientConfigurationSystem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Globalization;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Net;
    using Assembly = System.Reflection.Assembly;
    using StringBuilder = System.Text.StringBuilder;

    internal sealed class ClientConfigurationSystem : IInternalConfigSystem {
        private const string            SystemDiagnosticsConfigKey = "system.diagnostics"; 
        private const string            SystemNetGroupKey = "system.net/";                 

        private IConfigSystem           _configSystem;
        private IInternalConfigRoot     _configRoot;
        private ClientConfigurationHost _configHost;
        private IInternalConfigRecord   _machineConfigRecord;
        private IInternalConfigRecord   _completeConfigRecord;
        private Exception               _initError;    
        private bool                    _isInitInProgress;      
        private bool                    _isMachineConfigInited; 
        private bool                	_isUserConfigInited;
        private bool                    _isAppConfigHttp;       
        

        internal ClientConfigurationSystem() {
            _configSystem = new ConfigSystem();
            _configSystem.Init(typeof(ClientConfigurationHost), null, null);

            _configHost = (ClientConfigurationHost) _configSystem.Host;
            _configRoot = _configSystem.Root;

            _configRoot.ConfigRemoved += OnConfigRemoved;

            _isAppConfigHttp = _configHost.IsAppConfigHttp;

            // VSWhidbey 606116: Config has a dependency on Uri class which has
            // a new static constructor that calls config.  We need a dummy reference
            // to Uri class so the static constructor would be involved first to
            // initialize config.
            string dummy = System.Uri.SchemeDelimiter;
        }

        // Return true if the section might be used during initialization of the configuration system,
        // and thus lead to deadlock if appropriate measures are not taken.
        bool IsSectionUsedInInit(string configKey) {
            return configKey == SystemDiagnosticsConfigKey || (_isAppConfigHttp && configKey.StartsWith(SystemNetGroupKey, StringComparison.Ordinal));
        }
        
        // Return true if the section should only use the machine configuration and not use the
        // application configuration. This is only true for system.net sections when the configuration
        // file for the application is downloaded via http using System.Net.WebClient.
        bool DoesSectionOnlyUseMachineConfig(string configKey) {
            return _isAppConfigHttp && configKey.StartsWith(SystemNetGroupKey, StringComparison.Ordinal);
        }

        // Ensure that initialization has completed, while handling re-entrancy issues
        // for certain sections that may be used during initialization itself.
        void EnsureInit(string configKey) {
            bool doInit = false;

            lock (this) {
                // If the user config is not initialized, then we must either:
                //    a. Perform the initialization ourselves if no other thread is doing it, or
                //    b. Wait for the initialization to complete if we know the section is not used during initialization itself, or
                //    c. Ignore initialization if the section can be used during initialization. Note that GetSection()
                //       returns null is initialization has not completed.
                if (!_isUserConfigInited) {
                    if (!_isInitInProgress) {
                        _isInitInProgress = true;
                        doInit = true;
                    }
                    else if (!IsSectionUsedInInit(configKey)) {
                        // Wait for initialization to complete.
                        Monitor.Wait(this);
                    }
                }
            }

            if (doInit) {
                try {
                    try {
                        try {
                            // Initialize machine configuration.
                            _machineConfigRecord = _configRoot.GetConfigRecord(
                                    ClientConfigurationHost.MachineConfigPath);

                            _machineConfigRecord.ThrowIfInitErrors();

                            // Make machine configuration available to system.net sections
                            // when application configuration is downloaded via http.
                            _isMachineConfigInited = true;

                            //
                            // Prevent deadlocks in the networking classes by loading 
                            // networking config before making a networking request. 
                            // Any requests for sections used in initialization during 
                            // the call to EnsureConfigLoaded() will be served by 
                            // _machine.config or will return null.
                            //
                            if (_isAppConfigHttp) {
                                ConfigurationManagerHelperFactory.Instance.EnsureNetConfigLoaded();
                            }

                            //
                            // Now load the rest of configuration
                            //
                            _configHost.RefreshConfigPaths();
                            string configPath;
                            if (_configHost.HasLocalConfig) {
                                configPath = ClientConfigurationHost.LocalUserConfigPath;
                            }
                            else if (_configHost.HasRoamingConfig) {
                                configPath = ClientConfigurationHost.RoamingUserConfigPath;
                            }
                            else {
                                configPath = ClientConfigurationHost.ExeConfigPath;
                            }

                            _completeConfigRecord = _configRoot.GetConfigRecord(configPath);
                            _completeConfigRecord.ThrowIfInitErrors();

                            _isUserConfigInited = true;
                        }
                        catch (Exception e) {
                            _initError = new ConfigurationErrorsException(SR.GetString(SR.Config_client_config_init_error), e);
                            throw _initError;
                        }
                    }
                    catch {
                        ConfigurationManager.SetInitError(_initError);
                        _isMachineConfigInited = true;
                        _isUserConfigInited = true;
                        throw;
                    }
                }
                finally {
                    lock (this) {
                        try {
                            // Notify ConfigurationSettings that initialization has fully completed,
                            // even if unsuccessful.
                            ConfigurationManager.CompleteConfigInit();

                            _isInitInProgress = false;

                        }
                        finally {
                            // Wake up all threads waiting for initialization to complete.
                            Monitor.PulseAll(this);
                        }
                    }
                }
            }
        }

        private void PrepareClientConfigSystem(string sectionName) {
            // Ensure the configuration system is inited for this section.
            if (!_isUserConfigInited) {
                EnsureInit(sectionName);
            }

            // If an error occurred during initialzation, throw it.
            if (_initError != null) {
                throw _initError;
            }
        }

        //
        // If config has been removed because initialization was not complete,
        // fetch a new configuration record. The record will be created and
        // completely initialized as RequireCompleteInit() will have been called
        // on the ClientConfigurationHost before we receive this event.
        //
        private void OnConfigRemoved(object sender, InternalConfigEventArgs e) {
            try {
                IInternalConfigRecord newConfigRecord = _configRoot.GetConfigRecord(_completeConfigRecord.ConfigPath);
                _completeConfigRecord = newConfigRecord;
                _completeConfigRecord.ThrowIfInitErrors();
            }
            catch (Exception ex) {
                _initError = new ConfigurationErrorsException(SR.GetString(SR.Config_client_config_init_error), ex);
                ConfigurationManager.SetInitError(_initError);
                throw _initError;
            }
        }

        object IInternalConfigSystem.GetSection(string sectionName) {
            PrepareClientConfigSystem(sectionName);

            // Get the appropriate config record for the section.
            IInternalConfigRecord configRecord = null;
            if (DoesSectionOnlyUseMachineConfig(sectionName)) {
                if (_isMachineConfigInited) {
                    configRecord = _machineConfigRecord;
                }
            }
            else {
                if (_isUserConfigInited) {
                    configRecord = _completeConfigRecord;
                }
            }

            // Call GetSection(), or return null if no configuration is yet available.
            if (configRecord != null) {
                return configRecord.GetSection(sectionName);
            }
            else {
                return null;
            }
        }

        void IInternalConfigSystem.RefreshConfig(string sectionName) {
            PrepareClientConfigSystem(sectionName);

            if (_isMachineConfigInited) {
                _machineConfigRecord.RefreshSection(sectionName);
            }
        }

        // Supports user config
        bool IInternalConfigSystem.SupportsUserConfig {
            get {return true;}
        }
    }
}
