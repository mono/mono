//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;

    public sealed partial class ChannelPoolSettingsElement : ServiceModelConfigurationElement
    {
        public ChannelPoolSettingsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.IdleTimeout, DefaultValue = OneWayDefaults.IdleTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan IdleTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.IdleTimeout]; }
            set { base[ConfigurationStrings.IdleTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.LeaseTimeout, DefaultValue = OneWayDefaults.LeaseTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan LeaseTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.LeaseTimeout]; }
            set { base[ConfigurationStrings.LeaseTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxOutboundChannelsPerEndpoint, DefaultValue = OneWayDefaults.MaxOutboundChannelsPerEndpoint)]
        [IntegerValidator(MinValue = 1)]
        public int MaxOutboundChannelsPerEndpoint
        {
            get { return (int)base[ConfigurationStrings.MaxOutboundChannelsPerEndpoint]; }
            set { base[ConfigurationStrings.MaxOutboundChannelsPerEndpoint] = value; }
        }

        internal void ApplyConfiguration(ChannelPoolSettings settings)
        {
            if (null == settings)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }

            settings.IdleTimeout = this.IdleTimeout;
            settings.LeaseTimeout = this.LeaseTimeout;
            settings.MaxOutboundChannelsPerEndpoint = this.MaxOutboundChannelsPerEndpoint;
        }

        internal void InitializeFrom(ChannelPoolSettings settings)
        {
            if (null == settings)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }

            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.IdleTimeout, settings.IdleTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.LeaseTimeout, settings.LeaseTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxOutboundChannelsPerEndpoint, settings.MaxOutboundChannelsPerEndpoint);
        }

        internal void CopyFrom(ChannelPoolSettingsElement source)
        {
            if (null == source)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }

            this.IdleTimeout = source.IdleTimeout;
            this.LeaseTimeout = source.LeaseTimeout;
            this.MaxOutboundChannelsPerEndpoint = source.MaxOutboundChannelsPerEndpoint;
        }
    }
}



