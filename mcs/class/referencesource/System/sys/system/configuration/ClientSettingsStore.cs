//------------------------------------------------------------------------------
// <copyright file="ClientSettingsStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Scope="member", Target="System.Configuration.ClientSettingsStore+QuotaEnforcedStream.Dispose(System.Boolean):System.Void")]

namespace System.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;
    

    /// <devdoc>
    ///     This class abstracts the details of config system away from the LocalFileSettingsProvider. It talks to 
    ///     the configuration API and the relevant Sections to read and write settings. 
    ///     It understands sections of type ClientSettingsSection.
    ///
    ///     NOTE: This API supports reading from app.exe.config and user.config, but writing only to 
    ///           user.config.
    /// </devdoc>
    internal sealed class ClientSettingsStore {
        private const string ApplicationSettingsGroupName   = "applicationSettings";
        private const string UserSettingsGroupName          = "userSettings";
        private const string ApplicationSettingsGroupPrefix = ApplicationSettingsGroupName + "/";
        private const string UserSettingsGroupPrefix        = UserSettingsGroupName + "/";

        private Configuration GetUserConfig(bool isRoaming) {
            ConfigurationUserLevel userLevel = isRoaming ? ConfigurationUserLevel.PerUserRoaming : 
                                                           ConfigurationUserLevel.PerUserRoamingAndLocal;

            return ClientSettingsConfigurationHost.OpenExeConfiguration(userLevel);
        }

        private ClientSettingsSection GetConfigSection(Configuration config, string sectionName, bool declare) {
            string fullSectionName = UserSettingsGroupPrefix + sectionName;
            ClientSettingsSection section = null;

            if (config != null) {
                section = config.GetSection(fullSectionName) as ClientSettingsSection;

                if (section == null && declare) {
                    // Looks like the section isn't declared - let's declare it and try again.
                    DeclareSection(config, sectionName);
                    section = config.GetSection(fullSectionName) as ClientSettingsSection;
                }
            }

            return section;
        }

        // Declares the section handler of a given section in its section group, if a declaration isn't already
        // present. 
        private void DeclareSection(Configuration config, string sectionName) {
            ConfigurationSectionGroup settingsGroup = config.GetSectionGroup(UserSettingsGroupName);
            
            if (settingsGroup == null) {
                //Declare settings group
                ConfigurationSectionGroup group = new UserSettingsGroup();
                config.SectionGroups.Add(UserSettingsGroupName, group);
            }

            settingsGroup = config.GetSectionGroup(UserSettingsGroupName);

            Debug.Assert(settingsGroup != null, "Failed to declare settings group");

            if (settingsGroup != null) {
                ConfigurationSection section = settingsGroup.Sections[sectionName];
                if (section == null) {
                    section = new ClientSettingsSection();
                    section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToLocalUser;
                    section.SectionInformation.RequirePermission = false;
                    settingsGroup.Sections.Add(sectionName, section);
                }
            }
        }
        
        internal IDictionary ReadSettings(string sectionName, bool isUserScoped) {
            IDictionary settings = new Hashtable();

            if( isUserScoped && !ConfigurationManagerInternalFactory.Instance.SupportsUserConfig) {
                return settings;
            }

            string prefix = isUserScoped ? UserSettingsGroupPrefix : ApplicationSettingsGroupPrefix;
            ConfigurationManager.RefreshSection(prefix + sectionName);
            ClientSettingsSection section = ConfigurationManager.GetSection(prefix + sectionName) as ClientSettingsSection;

            if (section != null) {
                foreach (SettingElement setting in section.Settings) {
                    settings[setting.Name] = new StoredSetting(setting.SerializeAs, setting.Value.ValueXml);
                }
            }

            return settings;
        }

        internal static IDictionary ReadSettingsFromFile(string configFileName, string sectionName, bool isUserScoped) {
            IDictionary settings = new Hashtable();

            if( isUserScoped && !ConfigurationManagerInternalFactory.Instance.SupportsUserConfig) {
                return settings;
            }

            string prefix = isUserScoped ? UserSettingsGroupPrefix : ApplicationSettingsGroupPrefix;
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();

            // NOTE: When isUserScoped is true, we don't care if configFileName represents a roaming file or
            //       a local one. All we want is three levels of configuration. So, we use the PerUserRoaming level. 
            ConfigurationUserLevel userLevel = isUserScoped ? ConfigurationUserLevel.PerUserRoaming : ConfigurationUserLevel.None;

            if (isUserScoped) {
                fileMap.ExeConfigFilename = ConfigurationManagerInternalFactory.Instance.ApplicationConfigUri;
                fileMap.RoamingUserConfigFilename = configFileName;
            }
            else {
                fileMap.ExeConfigFilename = configFileName;
            }
            
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel);
            ClientSettingsSection section = config.GetSection(prefix + sectionName) as ClientSettingsSection;

            if (section != null) {
                foreach (SettingElement setting in section.Settings) {
                    settings[setting.Name] = new StoredSetting(setting.SerializeAs, setting.Value.ValueXml);
                }
            }

            return settings;
        }

        internal ConnectionStringSettingsCollection ReadConnectionStrings() {
            return PrivilegedConfigurationManager.ConnectionStrings;
        }

        internal void RevertToParent(string sectionName, bool isRoaming) {
            if (!ConfigurationManagerInternalFactory.Instance.SupportsUserConfig) {
                throw new ConfigurationErrorsException(SR.GetString(SR.UserSettingsNotSupported));
            }

            Configuration config = GetUserConfig(isRoaming);
            ClientSettingsSection section = GetConfigSection(config, sectionName, false);

            // If the section is null, there is nothing to revert.
            if (section != null) {
                section.SectionInformation.RevertToParent();
                config.Save();
            }
        }

        internal void WriteSettings(string sectionName, bool isRoaming, IDictionary newSettings) {
            if (!ConfigurationManagerInternalFactory.Instance.SupportsUserConfig) {
                throw new ConfigurationErrorsException(SR.GetString(SR.UserSettingsNotSupported));
            }

            Configuration config = GetUserConfig(isRoaming);
            ClientSettingsSection section = GetConfigSection(config, sectionName, true);

            if (section != null) {
                SettingElementCollection sec = section.Settings;
                foreach (DictionaryEntry entry in newSettings) {
                    SettingElement se = sec.Get((string) entry.Key);

                    if (se == null) {
                        se = new SettingElement();
                        se.Name = (string) entry.Key;
                        sec.Add(se);
                    }

                    StoredSetting ss = (StoredSetting) entry.Value;
                    se.SerializeAs = ss.SerializeAs;
                    se.Value.ValueXml = ss.Value;
                }

                try {
                    config.Save();
                }
                catch (ConfigurationErrorsException ex) {
                    // We wrap this in an exception with our error message and throw again.
                    throw new ConfigurationErrorsException(SR.GetString(SR.SettingsSaveFailed, ex.Message), ex);
                }
            }
            else {
                throw new ConfigurationErrorsException(SR.GetString(SR.SettingsSaveFailedNoSection));
            }
        }

        /// <devdoc>
        ///     A private configuration host that we use to write settings to config. We need this so we
        ///     can enforce a quota on the size of stuff written out.
        /// </devdoc>
        private sealed class ClientSettingsConfigurationHost : DelegatingConfigHost {
            private const string ClientConfigurationHostTypeName = "System.Configuration.ClientConfigurationHost," + AssemblyRef.SystemConfiguration;
            private const string InternalConfigConfigurationFactoryTypeName = "System.Configuration.Internal.InternalConfigConfigurationFactory," + AssemblyRef.SystemConfiguration;
            private static volatile IInternalConfigConfigurationFactory s_configFactory;
            
            /// <devdoc>
            ///     ClientConfigurationHost implements this - a way of getting some info from it without
            ///     depending too much on its internals.
            /// </devdoc>
            private IInternalConfigClientHost ClientHost {
                get {
                    return (IInternalConfigClientHost) Host;
                }
            }
    
            internal static IInternalConfigConfigurationFactory ConfigFactory {
                get {
                    if (s_configFactory == null) {
                        s_configFactory = (IInternalConfigConfigurationFactory) 
                                            TypeUtil.CreateInstanceWithReflectionPermission(InternalConfigConfigurationFactoryTypeName);
                    }
                    return s_configFactory;
                }
            }

            private ClientSettingsConfigurationHost() {}
    
            public override void Init(IInternalConfigRoot configRoot, params object[] hostInitParams) {
                Debug.Fail("Did not expect to get called here");
            }
    
            /// <devdoc>
            ///     We delegate this to the ClientConfigurationHost. The only thing we need to do here is to 
            ///     build a configPath from the ConfigurationUserLevel we get passed in.
            /// </devdoc>
            public override void InitForConfiguration(ref string locationSubPath, out string configPath, out string locationConfigPath, 
                    IInternalConfigRoot configRoot, params object[] hostInitConfigurationParams) {
    
                ConfigurationUserLevel userLevel = (ConfigurationUserLevel) hostInitConfigurationParams[0];
                string desiredConfigPath = null;
                Host = (IInternalConfigHost) TypeUtil.CreateInstanceWithReflectionPermission(ClientConfigurationHostTypeName);

                switch (userLevel) {
                    case ConfigurationUserLevel.None:
                        desiredConfigPath = ClientHost.GetExeConfigPath();
                        break;
    
                    case ConfigurationUserLevel.PerUserRoaming:
                        desiredConfigPath = ClientHost.GetRoamingUserConfigPath();
                        break;
    
                    case ConfigurationUserLevel.PerUserRoamingAndLocal:
                        desiredConfigPath = ClientHost.GetLocalUserConfigPath();
                        break;
    
                    default: 
                        throw new ArgumentException(SR.GetString(SR.UnknownUserLevel));
                }
    
                
                Host.InitForConfiguration(ref locationSubPath, out configPath, out locationConfigPath, configRoot, null, null, desiredConfigPath);
            }

            private bool IsKnownConfigFile(string filename) {
                return 
                  String.Equals(filename, ConfigurationManagerInternalFactory.Instance.MachineConfigPath, StringComparison.OrdinalIgnoreCase) ||
                  String.Equals(filename, ConfigurationManagerInternalFactory.Instance.ApplicationConfigUri, StringComparison.OrdinalIgnoreCase) ||
                  String.Equals(filename, ConfigurationManagerInternalFactory.Instance.ExeLocalConfigPath, StringComparison.OrdinalIgnoreCase) ||
                  String.Equals(filename, ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigPath, StringComparison.OrdinalIgnoreCase);

            }

            internal static Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel) {
                return ConfigFactory.Create(typeof(ClientSettingsConfigurationHost), userLevel);
            }
    
            /// <devdoc>
            ///     If the stream we are asked for represents a config file that we know about, we ask 
            ///     the host to assert appropriate permissions.
            /// </devdoc>
            public override Stream OpenStreamForRead(string streamName) {
                if (IsKnownConfigFile(streamName)) {
                    return Host.OpenStreamForRead(streamName, true);
                }
                else {
                    return Host.OpenStreamForRead(streamName);
                }
            }
            
            /// <devdoc>
            ///     If the stream we are asked for represents a user.config file that we know about, we wrap it in a
            ///     QuotaEnforcedStream, after asking the host to assert appropriate permissions.///     
            /// </devdoc>
            public override Stream OpenStreamForWrite(string streamName, string templateStreamName, ref object writeContext) {   
                Stream stream = null;

                if (String.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeLocalConfigPath, StringComparison.OrdinalIgnoreCase)) {
                    stream = new QuotaEnforcedStream( 
                                   Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext, true),
                                   false);
                }
                else if (String.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigPath, StringComparison.OrdinalIgnoreCase)) {
                    stream = new QuotaEnforcedStream( 
                                   Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext, true),
                                   true);
                }
                else {
                    stream = Host.OpenStreamForWrite(streamName, templateStreamName, ref writeContext);
                }

                return stream;
            }

            /// <devdoc>
            ///     If this is a stream that represents a user.config file that we know about, we ask 
            ///     the host to assert appropriate permissions.
            /// </devdoc>
            public override void WriteCompleted(string streamName, bool success, object writeContext) {
                if (String.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeLocalConfigPath, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(streamName, ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigPath, StringComparison.OrdinalIgnoreCase)) {
                    
                    Host.WriteCompleted(streamName, success, writeContext, true);
                }
                else {
                    Host.WriteCompleted(streamName, success, writeContext);
                }
            }
        }

        /// <devdoc>
        ///     A private stream class that wraps a stream and enforces a quota. The quota enforcement uses
        ///     IsolatedStorageFilePermission. We override nearly all methods on the Stream class so we can
        ///     forward to the wrapped stream. In the methods that affect stream length, we verify that the
        ///     quota is respected before forwarding.
        /// </devdoc>
        private sealed class QuotaEnforcedStream : Stream {
            private Stream _originalStream;
            private bool _isRoaming;

            internal QuotaEnforcedStream(Stream originalStream, bool isRoaming) {
                _originalStream = originalStream;
                _isRoaming = isRoaming;

                Debug.Assert(_originalStream != null, "originalStream was null.");
            }

            public override bool CanRead {
                get { return _originalStream.CanRead; }
            }
    
            public override bool CanWrite {
                get { return _originalStream.CanWrite; }
            }
    
            public override bool CanSeek {
                get { return _originalStream.CanSeek; }
            }
    
            public override long Length {
                get { return _originalStream.Length; }
            }
    
            public override long Position {
    
                get { return _originalStream.Position; }
    
                set { 
                    if (value < 0) {
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.PositionOutOfRange));
                    }
    
                    Seek(value, SeekOrigin.Begin);
                }
            }

            public override void Close() {
                _originalStream.Close();
            }
    
            protected override void Dispose(bool disposing) {
                if (disposing) {
                    if (_originalStream != null) {
                        ((IDisposable)_originalStream).Dispose();
                        _originalStream = null;
                    }
                    
                }

                base.Dispose(disposing);
            }
    
            public override void Flush() {
                _originalStream.Flush();
            }
    
            public override void SetLength(long value) {
                long oldLen = _originalStream.Length;
                long newLen = value;

                EnsureQuota(Math.Max(oldLen, newLen));
                _originalStream.SetLength(value);
            }
    
            public override int Read(byte[] buffer, int offset, int count) {
                return _originalStream.Read(buffer, offset, count);
            }
    
            public override int ReadByte() {
                return _originalStream.ReadByte();
            }
    
            public override long Seek(long offset, SeekOrigin origin) {
                if (!CanSeek) {
                    throw new NotSupportedException();
                }

                long oldLen = _originalStream.Length;
                long newLen;

                switch (origin) {
                    case SeekOrigin.Begin:
                        newLen = offset;
                        break;
                    case SeekOrigin.Current:
                        newLen = _originalStream.Position + offset;
                        break;
                    case SeekOrigin.End:
                        newLen = oldLen + offset;
                        break;
                    default:
                        throw new ArgumentException(SR.GetString(SR.UnknownSeekOrigin), "origin");
                }

                EnsureQuota(Math.Max(oldLen, newLen));
                return _originalStream.Seek(offset, origin);
            }
    
            public override void Write(byte[] buffer, int offset, int count) {
                if (!CanWrite) {
                    throw new NotSupportedException();
                }

                long oldLen = _originalStream.Length;
                long newLen = _originalStream.CanSeek ? _originalStream.Position + (long)count : 
                                                        _originalStream.Length + (long)count;
                EnsureQuota(Math.Max(oldLen, newLen));
                _originalStream.Write(buffer, offset, count);
            }
    
            public override void WriteByte(byte value) {
                if (!CanWrite) {
                    throw new NotSupportedException();
                }

                long oldLen = _originalStream.Length;
                long newLen = _originalStream.CanSeek ? _originalStream.Position + 1 : _originalStream.Length + 1;
                EnsureQuota(Math.Max(oldLen, newLen));

                _originalStream.WriteByte(value);
            }
    
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int numBytes, 
                                                   AsyncCallback userCallback, Object stateObject) {
                return _originalStream.BeginRead(buffer, offset, numBytes, userCallback, stateObject);
            }
    
            public override int EndRead(IAsyncResult asyncResult) {
                return _originalStream.EndRead(asyncResult);
                    
            }
    
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int numBytes, 
                                                    AsyncCallback userCallback, Object stateObject) {
                if (!CanWrite) {
                    throw new NotSupportedException();
                }

                long oldLen = _originalStream.Length;
                long newLen = _originalStream.CanSeek ? _originalStream.Position + (long)numBytes : 
                                                        _originalStream.Length + (long)numBytes;
                EnsureQuota(Math.Max(oldLen, newLen));
                return _originalStream.BeginWrite(buffer, offset, numBytes, userCallback, stateObject);
            }
    
            public override void EndWrite(IAsyncResult asyncResult) {
                _originalStream.EndWrite(asyncResult);
            }

            // 
            private void EnsureQuota(long size) {
                IsolatedStoragePermission storagePerm = new IsolatedStorageFilePermission(PermissionState.None);
                storagePerm.UserQuota = size;
                storagePerm.UsageAllowed = _isRoaming? IsolatedStorageContainment.DomainIsolationByRoamingUser :
                                                       IsolatedStorageContainment.DomainIsolationByUser;
                storagePerm.Demand();
            }
        }
    }

    /// <devdoc>
    ///     The ClientSettingsStore talks to the LocalFileSettingsProvider through a dictionary which maps from
    ///     setting names to StoredSetting structs. This struct contains the relevant information.
    /// </devdoc>
    internal struct StoredSetting {
        internal StoredSetting(SettingsSerializeAs serializeAs, XmlNode value) {
            SerializeAs = serializeAs;
            Value = value;
        }
        internal SettingsSerializeAs SerializeAs;
        internal XmlNode Value;
    }
}
