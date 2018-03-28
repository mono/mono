// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;

    /// <summary>
    /// WebSocketTransportSettingsElement for WebSocketTransportSettings
    /// </summary>
    public partial class WebSocketTransportSettingsElement : ServiceModelConfigurationElement
    {
        [ConfigurationProperty(ConfigurationStrings.TransportUsage, DefaultValue = WebSocketDefaults.TransportUsage)]
        [ServiceModelEnumValidator(typeof(WebSocketTransportUsageHelper))]
        public virtual WebSocketTransportUsage TransportUsage
        {
            get { return (WebSocketTransportUsage)base[ConfigurationStrings.TransportUsage]; }
            set { base[ConfigurationStrings.TransportUsage] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.CreateNotificationOnConnection, DefaultValue = WebSocketDefaults.CreateNotificationOnConnection)]
        public bool CreateNotificationOnConnection
        {
            get { return (bool)base[ConfigurationStrings.CreateNotificationOnConnection]; }
            set { base[ConfigurationStrings.CreateNotificationOnConnection] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.KeepAliveInterval, DefaultValue = WebSocketDefaults.DefaultKeepAliveIntervalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanInfinite)]
        public TimeSpan KeepAliveInterval
        {   
            get { return (TimeSpan)base[ConfigurationStrings.KeepAliveInterval]; }
            set { base[ConfigurationStrings.KeepAliveInterval] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SubProtocol, DefaultValue = null)]
        [StringValidator(MinLength = 0)]
        public virtual string SubProtocol
        {
            get { return (string)base[ConfigurationStrings.SubProtocol]; }
            set { base[ConfigurationStrings.SubProtocol] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DisablePayloadMasking, DefaultValue = WebSocketDefaults.DisablePayloadMasking)]
        public bool DisablePayloadMasking
        {
            get { return (bool)base[ConfigurationStrings.DisablePayloadMasking]; }
            set { base[ConfigurationStrings.DisablePayloadMasking] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingConnections, DefaultValue = WebSocketDefaults.DefaultMaxPendingConnections)]
        [IntegerValidator(MinValue = 0)]
        public int MaxPendingConnections
        {
            get { return (int)base[ConfigurationStrings.MaxPendingConnections]; }
            set { base[ConfigurationStrings.MaxPendingConnections] = value; }
        }

        public void InitializeFrom(WebSocketTransportSettings settings)
        {
            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TransportUsage, settings.TransportUsage);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CreateNotificationOnConnection, settings.CreateNotificationOnConnection);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.KeepAliveInterval, settings.KeepAliveInterval);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SubProtocol, settings.SubProtocol);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.DisablePayloadMasking, settings.DisablePayloadMasking);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxPendingConnections, settings.MaxPendingConnections);
        }

        public void ApplyConfiguration(WebSocketTransportSettings settings)
        {
            if (settings == null)
            {
                throw FxTrace.Exception.ArgumentNull("settings");
            }

            settings.TransportUsage = this.TransportUsage;
            settings.CreateNotificationOnConnection = this.CreateNotificationOnConnection;
            settings.KeepAliveInterval = this.KeepAliveInterval;
            settings.SubProtocol = string.IsNullOrEmpty(this.SubProtocol) ? null : this.SubProtocol;
            settings.DisablePayloadMasking = this.DisablePayloadMasking;
            settings.MaxPendingConnections = this.MaxPendingConnections;
        }
    }
}
