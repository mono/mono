//------------------------------------------------------------------------------
// <copyright file="NetSectionGroup.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Summary description for NetSectionGroup.
    /// </summary>
    public sealed class NetSectionGroup : ConfigurationSectionGroup
    {
        public NetSectionGroup() {}

        // public properties
        [ConfigurationProperty(ConfigurationStrings.AuthenticationModulesSectionName)]
        public AuthenticationModulesSection AuthenticationModules
        {
            get { return (AuthenticationModulesSection)Sections[ConfigurationStrings.AuthenticationModulesSectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ConnectionManagementSectionName)]
        public ConnectionManagementSection ConnectionManagement
        {
            get { return (ConnectionManagementSection)Sections[ConfigurationStrings.ConnectionManagementSectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultProxySectionName)]
        public DefaultProxySection DefaultProxy
        {
            get { return (DefaultProxySection)Sections[ConfigurationStrings.DefaultProxySectionName]; }
        }

#if !FEATURE_PAL
        public MailSettingsSectionGroup MailSettings
        {
            get { return (MailSettingsSectionGroup)SectionGroups[ConfigurationStrings.MailSettingsSectionName]; }
        }
#endif // !FEATURE_PAL

        static public NetSectionGroup GetSectionGroup(Configuration config)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            return config.GetSectionGroup(ConfigurationStrings.SectionGroupName) as NetSectionGroup;
        }

        [ConfigurationProperty(ConfigurationStrings.RequestCachingSectionName)]
        public RequestCachingSection RequestCaching
        {
            get { return (RequestCachingSection)Sections[ConfigurationStrings.RequestCachingSectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.SettingsSectionName)]
        public SettingsSection Settings
        {
            get { return (SettingsSection)Sections[ConfigurationStrings.SettingsSectionName]; }
        }

        [ConfigurationProperty(ConfigurationStrings.WebRequestModulesSectionName)]
        public WebRequestModulesSection WebRequestModules
        {
            get { return (WebRequestModulesSection)Sections[ConfigurationStrings.WebRequestModulesSectionName]; }
        }

    }
}
