// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// The NamedPipeSettingElement provides configuration support for the NamedPipeSetttings
    /// on the NamedPipeTransportBinding element. 
    /// </summary>
    public sealed partial class NamedPipeSettingsElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.ApplicationContainerSettings)]
        public ApplicationContainerSettingsElement ApplicationContainerSettings
        {
            get { return (ApplicationContainerSettingsElement)base[ConfigurationStrings.ApplicationContainerSettings]; }
            set { base[ConfigurationStrings.ApplicationContainerSettings] = value; }
        }

        internal void ApplyConfiguration(NamedPipeSettings settings)
        {
            if (null == settings)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            this.ApplicationContainerSettings.ApplyConfiguration(settings.ApplicationContainerSettings);
        }

        internal void InitializeFrom(NamedPipeSettings settings)
        {
            if (null == settings)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            this.ApplicationContainerSettings.InitializeFrom(settings.ApplicationContainerSettings);
        }

        internal void CopyFrom(NamedPipeSettingsElement source)
        {
            if (null == source)
            {
                throw FxTrace.Exception.ArgumentNull("source");
            }

            this.ApplicationContainerSettings.CopyFrom(source.ApplicationContainerSettings);
        }
    }
}
