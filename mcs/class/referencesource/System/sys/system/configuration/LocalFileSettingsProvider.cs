//------------------------------------------------------------------------------
// <copyright file="LocalFileSettingsProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace System.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Runtime.Versioning;
    
    /// <devdoc>
    ///    <para>
    ///         This is a provider used to store configuration settings locally for client applications.
    ///    </para>
    /// </devdoc>
    [
     PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"),
     PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")
    ]
    public class LocalFileSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        private string              _appName                    = String.Empty;
        private ClientSettingsStore  _store                     = null;
        private string              _prevLocalConfigFileName    = null;
        private string              _prevRoamingConfigFileName  = null;
        private XmlEscaper          _escaper                    = null;
        
        /// <devdoc>
        ///     Abstract SettingsProvider property.
        /// </devdoc>
        public override string ApplicationName { 
            get { 
                return _appName;
            } 
            set {
                _appName = value;
            }
        }

        private XmlEscaper Escaper {
            get {
                if (_escaper == null) {
                    _escaper = new XmlEscaper();
                }

                return _escaper;
            }
        }

        /// <devdoc>
        ///     We maintain a single instance of the ClientSettingsStore per instance of provider.
        /// </devdoc>
        private ClientSettingsStore Store {
            get {
                if (_store == null) {
                    _store = new ClientSettingsStore();
                }

                return _store;
            }
        }

        /// <devdoc>
        ///     Abstract ProviderBase method.
        /// </devdoc>
        public override void Initialize(string name, NameValueCollection values) {
            if (String.IsNullOrEmpty(name)) {
                name = "LocalFileSettingsProvider";
            }

            base.Initialize(name, values);
        }

        /// <devdoc>
        ///     Abstract SettingsProvider method
        /// </devdoc>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties) {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            string sectionName = GetSectionName(context);

            //<--Look for this section in both applicationSettingsGroup and userSettingsGroup-->
            IDictionary appSettings = Store.ReadSettings(sectionName, false);
            IDictionary userSettings = Store.ReadSettings(sectionName, true);
            ConnectionStringSettingsCollection connStrings = Store.ReadConnectionStrings();

            //<--Now map each SettingProperty to the right StoredSetting and deserialize the value if found.-->
            foreach (SettingsProperty setting in properties) {
                string settingName = setting.Name;
                SettingsPropertyValue value = new SettingsPropertyValue(setting);
                
                // First look for and handle "special" settings
                SpecialSettingAttribute attr = setting.Attributes[typeof(SpecialSettingAttribute)] as SpecialSettingAttribute;
                bool isConnString =  (attr != null) ? (attr.SpecialSetting == SpecialSetting.ConnectionString) : false;
                
                if (isConnString) { 
                    string connStringName = sectionName + "." + settingName; 
                    if (connStrings != null && connStrings[connStringName] != null) {
                        value.PropertyValue = connStrings[connStringName].ConnectionString;
                    }
                    else if (setting.DefaultValue != null && setting.DefaultValue is string) {
                        value.PropertyValue = setting.DefaultValue;
                    }
                    else {
                        //No value found and no default specified 
                        value.PropertyValue = String.Empty;
                    }

                    value.IsDirty = false; //reset IsDirty so that it is correct when SetPropertyValues is called 
                    values.Add(value);
                    continue;
                }

                // Not a "special" setting
                bool isUserSetting = IsUserSetting(setting); 

                if (isUserSetting && !ConfigurationManagerInternalFactory.Instance.SupportsUserConfig) {
                    // We encountered a user setting, but the current configuration system does not support
                    // user settings.
                   throw new ConfigurationErrorsException(SR.GetString(SR.UserSettingsNotSupported));
                }

                IDictionary settings = isUserSetting ? userSettings : appSettings;
                
                if (settings.Contains(settingName)) {
                    StoredSetting ss = (StoredSetting) settings[settingName];
                    string valueString = ss.Value.InnerXml;

                    // We need to un-escape string serialized values
                    if (ss.SerializeAs == SettingsSerializeAs.String) {
                        valueString = Escaper.Unescape(valueString);
                    }

                    value.SerializedValue = valueString;
                }
                else if (setting.DefaultValue != null) {
                    value.SerializedValue = setting.DefaultValue;
                }
                else {
                    //No value found and no default specified 
                    value.PropertyValue = null;
                }

                value.IsDirty = false; //reset IsDirty so that it is correct when SetPropertyValues is called 
                values.Add(value);
            }

            return values;
        }

        /// <devdoc>
        ///     Abstract SettingsProvider method
        /// </devdoc>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values) {
            string sectionName = GetSectionName(context);
            IDictionary roamingUserSettings = new Hashtable();
            IDictionary localUserSettings = new Hashtable();
            
            foreach (SettingsPropertyValue value in values) {
                SettingsProperty setting = value.Property;
                bool isUserSetting = IsUserSetting(setting);

                if (value.IsDirty) {
                    if (isUserSetting) {
                        bool isRoaming = IsRoamingSetting(setting);
                        StoredSetting ss = new StoredSetting(setting.SerializeAs, SerializeToXmlElement(setting, value));

                        if (isRoaming) {
                            roamingUserSettings[setting.Name] = ss;
                        }
                        else {
                            localUserSettings[setting.Name] = ss;
                        }
                        
                        value.IsDirty = false; //reset IsDirty
                    }
                    else {
                        // This is an app-scoped or connection string setting that has been written to. 
                        // We don't support saving these.
                    }
                }
            }
            
            // Semi-hack: If there are roamable settings, let's write them before local settings so if a handler 
            // declaration is necessary, it goes in the roaming config file in preference to the local config file.
            if (roamingUserSettings.Count > 0) {
                Store.WriteSettings(sectionName, true, roamingUserSettings);
            }

            if (localUserSettings.Count > 0) {
                Store.WriteSettings(sectionName, false, localUserSettings);
            }
        }

        /// <devdoc>
        ///     Implementation of IClientSettingsProvider.Reset. Resets user scoped settings to the values 
        ///     in app.exe.config, does nothing for app scoped settings.
        /// </devdoc>
        public void Reset(SettingsContext context) {
            string sectionName = GetSectionName(context);

            // First revert roaming, then local
            Store.RevertToParent(sectionName, true);
            Store.RevertToParent(sectionName, false);
        }

        /// <devdoc>
        ///    Implementation of IClientSettingsProvider.Upgrade.
        ///    Tries to locate a previous version of the user.config file. If found, it migrates matching settings.
        ///    If not, it does nothing.
        /// </devdoc>
        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties) {
            // Separate the local and roaming settings and upgrade them separately.
            
            SettingsPropertyCollection local = new SettingsPropertyCollection();
            SettingsPropertyCollection roaming = new SettingsPropertyCollection();
            
            foreach (SettingsProperty sp in properties) {
                bool isRoaming = IsRoamingSetting(sp);

                if (isRoaming) {
                    roaming.Add(sp);
                }
                else {
                    local.Add(sp);
                }
            }

            if (roaming.Count > 0) {
                Upgrade(context, roaming, true);
            }

            if (local.Count > 0) {
                Upgrade(context, local, false);
            }
        }

        /// <devdoc>
        ///     Encapsulates the Version constructor so that we can return null when an exception is thrown.
        /// </devdoc>
        private Version CreateVersion(string name) {
            Version ver = null;

            try {
                ver = new Version(name);
            }
            catch (ArgumentException) { 
                ver = null;
            }
            catch (OverflowException) {
                ver = null;
            }
            catch (FormatException) {
                ver = null;
            }

            return ver;
        }

        /// <devdoc>
        ///    Implementation of IClientSettingsProvider.GetPreviousVersion.
        /// </devdoc>
        //  Security Note: Like Upgrade, GetPreviousVersion involves finding a previous version user.config file and 
        //  reading settings from it. To support this in partial trust, we need to assert file i/o here. We believe 
        //  this to be safe, since the user does not have a way to specify the file or control where we look for it. 
        //  So it is no different than reading from the default user.config file, which we already allow in partial trust.
        //  BTW, the Link/Inheritance demand pair here is just a copy of what's at the class level, and is needed since
        //  we are overriding security at method level.
        [
         FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read),
         PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"),
         PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")
        ]
        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property) {
            bool isRoaming = IsRoamingSetting(property);
            string prevConfig = GetPreviousConfigFileName(isRoaming);

            if (!String.IsNullOrEmpty(prevConfig)) {
                SettingsPropertyCollection properties = new SettingsPropertyCollection();
                properties.Add(property);
                SettingsPropertyValueCollection values = GetSettingValuesFromFile(prevConfig, GetSectionName(context), true, properties);
                return values[property.Name];
            }
            else {
                SettingsPropertyValue value = new SettingsPropertyValue(property);
                value.PropertyValue = null;
                return value;
            }
        }

        /// <devdoc>
        ///     Locates the previous version of user.config, if present. The previous version is determined
        ///     by walking up one directory level in the *UserConfigPath and searching for the highest version
        ///     number less than the current version.
        ///     SECURITY NOTE: Config path information is privileged - do not directly pass this on to untrusted callers.
        ///     Note this is meant to be used at installation time to help migrate 
        ///     config settings from a previous version of the app.
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private string GetPreviousConfigFileName(bool isRoaming) {
            if (!ConfigurationManagerInternalFactory.Instance.SupportsUserConfig) {
                throw new ConfigurationErrorsException(SR.GetString(SR.UserSettingsNotSupported));
            }

            string prevConfigFile = isRoaming ? _prevRoamingConfigFileName : _prevLocalConfigFileName;

            if (String.IsNullOrEmpty(prevConfigFile)) {
                string userConfigPath = isRoaming ? ConfigurationManagerInternalFactory.Instance.ExeRoamingConfigDirectory : ConfigurationManagerInternalFactory.Instance.ExeLocalConfigDirectory;
                Version curVer = CreateVersion(ConfigurationManagerInternalFactory.Instance.ExeProductVersion);
                Version prevVer = null;
                DirectoryInfo prevDir = null;
                string file = null;
    
                if (curVer == null) {
                    return null;
                }
    
                DirectoryInfo parentDir = Directory.GetParent(userConfigPath);
    
                if (parentDir.Exists) {
                    foreach (DirectoryInfo dir in parentDir.GetDirectories()) {
                        Version tempVer = CreateVersion(dir.Name);
        
                        if (tempVer != null && tempVer < curVer) {
                            if (prevVer == null) {
                                prevVer = tempVer;
                                prevDir = dir;
                            }
                            else if (tempVer > prevVer) {
                                prevVer = tempVer;
                                prevDir = dir;
                            }
                        }
                    }
        
                    if (prevDir != null) {
                        file = Path.Combine(prevDir.FullName, ConfigurationManagerInternalFactory.Instance.UserConfigFilename);
                    }
        
                    if (File.Exists(file)) {
                        prevConfigFile = file;
                    }
                }

                //Cache for future use.
                if (isRoaming) {
                    _prevRoamingConfigFileName = prevConfigFile;
                }
                else {
                    _prevLocalConfigFileName = prevConfigFile;
                }
            }

            return prevConfigFile;
        }

        /// <devdoc>
        ///     Gleans information from the SettingsContext and determines the name of the config section.
        /// </devdoc>
        private string GetSectionName(SettingsContext context) {
            string groupName = (string) context["GroupName"];
            string key = (string) context["SettingsKey"];
            
            Debug.Assert(groupName != null, "SettingsContext did not have a GroupName!");

            string sectionName = groupName;

            if (!String.IsNullOrEmpty(key)) {
                sectionName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", sectionName, key);
            }

            return XmlConvert.EncodeLocalName(sectionName);
        }

        /// <devdoc>
        ///     Retrieves the values of settings from the given config file (as opposed to using 
        ///     the configuration for the current context)
        /// </devdoc>
        private SettingsPropertyValueCollection GetSettingValuesFromFile(string configFileName, string sectionName, bool userScoped, SettingsPropertyCollection properties) {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            IDictionary settings = ClientSettingsStore.ReadSettingsFromFile(configFileName, sectionName, userScoped);

            // Map each SettingProperty to the right StoredSetting and deserialize the value if found.
            foreach (SettingsProperty setting in properties) {
                string settingName = setting.Name;
                SettingsPropertyValue value = new SettingsPropertyValue(setting);
                
                if (settings.Contains(settingName)) {
                    StoredSetting ss = (StoredSetting) settings[settingName];
                    string valueString = ss.Value.InnerXml;

                    // We need to un-escape string serialized values
                    if (ss.SerializeAs == SettingsSerializeAs.String) {
                        valueString = Escaper.Unescape(valueString);
                    }

                    value.SerializedValue = valueString;
                    value.IsDirty = true;
                    values.Add(value);
                }
            }
            
            return values;
        }

        /// <devdoc>
        ///     Indicates whether a setting is roaming or not.
        /// </devdoc>
        private static bool IsRoamingSetting(SettingsProperty setting) {
            // Roaming is not supported in Clickonce deployed apps, since they don't have roaming config files.
            bool roamingSupported = !ApplicationSettingsBase.IsClickOnceDeployed(AppDomain.CurrentDomain);
            bool isRoaming = false;

            if (roamingSupported) {
                SettingsManageabilityAttribute manageAttr = setting.Attributes[typeof(SettingsManageabilityAttribute)] as SettingsManageabilityAttribute;
                isRoaming = manageAttr != null && ((manageAttr.Manageability & SettingsManageability.Roaming) == SettingsManageability.Roaming);
            }

            return isRoaming;
        }

        /// <devdoc>
        ///     This provider needs settings to be marked with either the UserScopedSettingAttribute or the
        ///     ApplicationScopedSettingAttribute. This method determines whether this setting is user-scoped
        ///     or not. It will throw if none or both of the attributes are present.
        /// </devdoc>
        private bool IsUserSetting(SettingsProperty setting) {
            bool isUser = setting.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute;
            bool isApp  = setting.Attributes[typeof(ApplicationScopedSettingAttribute)] is ApplicationScopedSettingAttribute;

            if (isUser && isApp) {
                throw new ConfigurationErrorsException(SR.GetString(SR.BothScopeAttributes));
            }
            else if (!(isUser || isApp)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.NoScopeAttributes));
            }

            return isUser;
        }

        private XmlNode SerializeToXmlElement(SettingsProperty setting, SettingsPropertyValue value) {
            XmlDocument doc = new XmlDocument();
            XmlElement valueXml = doc.CreateElement("value");

            string serializedValue = value.SerializedValue as string;
            
            if (serializedValue == null && setting.SerializeAs == SettingsSerializeAs.Binary) {
                // SettingsPropertyValue returns a byte[] in the binary serialization case. We need to
                // encode this - we use base64 since SettingsPropertyValue understands it and we won't have
                // to special case while deserializing.
                byte[] buf = value.SerializedValue as byte[];
                if (buf != null) {
                    serializedValue = Convert.ToBase64String(buf);
                }
            }

            if (serializedValue == null) {
                serializedValue = String.Empty;
            }
            
            // We need to escape string serialized values
            if (setting.SerializeAs == SettingsSerializeAs.String) {
                serializedValue = Escaper.Escape(serializedValue);
            }

            valueXml.InnerXml = serializedValue; 
            
            // Hack to remove the XmlDeclaration that the XmlSerializer adds. 
            XmlNode unwanted = null;
            foreach (XmlNode child in valueXml.ChildNodes) {
                if (child.NodeType == XmlNodeType.XmlDeclaration) {
                    unwanted = child;
                    break;
                }
            }
            if (unwanted != null) {
                valueXml.RemoveChild(unwanted);
            }
            
            return valueXml;
        }

        /// <devdoc>
        ///    Private version of upgrade that uses isRoaming to determine which config file to use.
        /// </devdoc> 
        // Security Note: Upgrade involves finding a previous version user.config file and reading settings from it. To
        // support this in partial trust, we need to assert file i/o here. We believe this to be safe, since the user
        // does not have a way to specify the file or control where we look for it. As such, it is no different than
        // reading from the default user.config file, which we already allow in partial trust.
        // The following suppress is okay, since the Link/Inheritance demand pair at the class level are not needed for
        // this method, since it is private.
        [SuppressMessage("Microsoft.Security", "CA2114:MethodSecurityShouldBeASupersetOfType")]
        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
        private void Upgrade(SettingsContext context, SettingsPropertyCollection properties, bool isRoaming) {
            string prevConfig = GetPreviousConfigFileName(isRoaming); 
            
            if (!String.IsNullOrEmpty(prevConfig)) {
                //Filter the settings properties to exclude those that have a NoSettingsVersionUpgradeAttribute on them.
                SettingsPropertyCollection upgradeProperties = new SettingsPropertyCollection();
                foreach (SettingsProperty sp in properties) {
                    if (!(sp.Attributes[typeof(NoSettingsVersionUpgradeAttribute)] is NoSettingsVersionUpgradeAttribute)) {
                        upgradeProperties.Add(sp);
                    }
                }
                
                SettingsPropertyValueCollection values = GetSettingValuesFromFile(prevConfig, GetSectionName(context), true, upgradeProperties);
                SetPropertyValues(context, values);
            }
        }

        private class XmlEscaper {
            private XmlDocument doc;
            private XmlElement temp;
    
            internal XmlEscaper() {
                doc = new XmlDocument();
                temp = doc.CreateElement("temp");
            }
    
            internal string Escape(string xmlString) {
                if (String.IsNullOrEmpty(xmlString)) {
                    return xmlString;
                }
    
                temp.InnerText = xmlString;
                return temp.InnerXml;
            }
    
            internal string Unescape(string escapedString) {
                if (String.IsNullOrEmpty(escapedString)) {
                    return escapedString;
                }
    
                temp.InnerXml = escapedString;
                return temp.InnerText;
            }
        }
    }
}
