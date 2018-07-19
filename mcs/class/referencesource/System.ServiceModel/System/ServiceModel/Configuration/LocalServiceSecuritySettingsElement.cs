//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;

    public sealed partial class LocalServiceSecuritySettingsElement : ServiceModelConfigurationElement
    {
        public LocalServiceSecuritySettingsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.DetectReplays, DefaultValue = SecurityProtocolFactory.defaultDetectReplays)]
        public bool DetectReplays
        {
            get { return (bool)base[ConfigurationStrings.DetectReplays]; }
            set { base[ConfigurationStrings.DetectReplays] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuedCookieLifetime, DefaultValue = SpnegoTokenAuthenticator.defaultServerIssuedTokenLifetimeString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan IssuedCookieLifetime
        {
            get { return (TimeSpan)base[ConfigurationStrings.IssuedCookieLifetime]; }
            set { base[ConfigurationStrings.IssuedCookieLifetime] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxStatefulNegotiations, DefaultValue = SpnegoTokenAuthenticator.defaultServerMaxActiveNegotiations)]
        [IntegerValidator(MinValue = 0)]
        public int MaxStatefulNegotiations
        {
            get { return (int)base[ConfigurationStrings.MaxStatefulNegotiations]; }
            set { base[ConfigurationStrings.MaxStatefulNegotiations] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReplayCacheSize, DefaultValue = SecurityProtocolFactory.defaultMaxCachedNonces)]
        [IntegerValidator(MinValue = 1)]
        public int ReplayCacheSize
        {
            get { return (int)base[ConfigurationStrings.ReplayCacheSize]; }
            set { base[ConfigurationStrings.ReplayCacheSize] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxClockSkew, DefaultValue = SecurityProtocolFactory.defaultMaxClockSkewString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaxClockSkew
        {
            get { return (TimeSpan)base[ConfigurationStrings.MaxClockSkew]; }
            set { base[ConfigurationStrings.MaxClockSkew] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.NegotiationTimeout, DefaultValue = SpnegoTokenAuthenticator.defaultServerMaxNegotiationLifetimeString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan NegotiationTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.NegotiationTimeout]; }
            set { base[ConfigurationStrings.NegotiationTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReplayWindow, DefaultValue = SecurityProtocolFactory.defaultReplayWindowString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan ReplayWindow
        {
            get { return (TimeSpan)base[ConfigurationStrings.ReplayWindow]; }
            set { base[ConfigurationStrings.ReplayWindow] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.InactivityTimeout, DefaultValue = SecuritySessionServerSettings.defaultInactivityTimeoutString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan InactivityTimeout
        {
            get { return (TimeSpan)base[ConfigurationStrings.InactivityTimeout]; }
            set { base[ConfigurationStrings.InactivityTimeout] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SessionKeyRenewalInterval, DefaultValue = SecuritySessionServerSettings.defaultKeyRenewalIntervalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan SessionKeyRenewalInterval
        {
            get { return (TimeSpan)base[ConfigurationStrings.SessionKeyRenewalInterval]; }
            set { base[ConfigurationStrings.SessionKeyRenewalInterval] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SessionKeyRolloverInterval, DefaultValue = SecuritySessionServerSettings.defaultKeyRolloverIntervalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan SessionKeyRolloverInterval
        {
            get { return (TimeSpan)base[ConfigurationStrings.SessionKeyRolloverInterval]; }
            set { base[ConfigurationStrings.SessionKeyRolloverInterval] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReconnectTransportOnFailure, DefaultValue = SecuritySessionServerSettings.defaultTolerateTransportFailures)]
        public bool ReconnectTransportOnFailure
        {
            get { return (bool)base[ConfigurationStrings.ReconnectTransportOnFailure]; }
            set { base[ConfigurationStrings.ReconnectTransportOnFailure] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxPendingSessions, DefaultValue = SecuritySessionServerSettings.defaultMaximumPendingSessions)]
        [IntegerValidator(MinValue = 1)]
        public int MaxPendingSessions
        {
            get { return (int)base[ConfigurationStrings.MaxPendingSessions]; }
            set { base[ConfigurationStrings.MaxPendingSessions] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.MaxCachedCookies, DefaultValue = SpnegoTokenAuthenticator.defaultServerMaxCachedTokens)]
        [IntegerValidator(MinValue = 0)]
        public int MaxCachedCookies
        {
            get { return (int)base[ConfigurationStrings.MaxCachedCookies]; }
            set { base[ConfigurationStrings.MaxCachedCookies] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TimestampValidityDuration, DefaultValue = SecurityProtocolFactory.defaultTimestampValidityDurationString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan TimestampValidityDuration
        {
            get { return (TimeSpan)base[ConfigurationStrings.TimestampValidityDuration]; }
            set { base[ConfigurationStrings.TimestampValidityDuration] = value; }
        }

        internal void ApplyConfiguration(LocalServiceSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.DetectReplays].ValueOrigin)
                settings.DetectReplays = this.DetectReplays;
            settings.IssuedCookieLifetime = this.IssuedCookieLifetime;
            settings.MaxClockSkew = this.MaxClockSkew;
            settings.MaxPendingSessions = this.MaxPendingSessions;
            settings.MaxStatefulNegotiations = this.MaxStatefulNegotiations;
            settings.NegotiationTimeout = this.NegotiationTimeout;
            settings.ReconnectTransportOnFailure = this.ReconnectTransportOnFailure;
            settings.ReplayCacheSize = this.ReplayCacheSize;
            settings.ReplayWindow = this.ReplayWindow;
            settings.SessionKeyRenewalInterval = this.SessionKeyRenewalInterval;
            settings.SessionKeyRolloverInterval = this.SessionKeyRolloverInterval;
            settings.InactivityTimeout = this.InactivityTimeout;
            settings.TimestampValidityDuration = this.TimestampValidityDuration;
            settings.MaxCachedCookies = this.MaxCachedCookies;
        }

        internal void InitializeFrom(LocalServiceSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            this.DetectReplays = settings.DetectReplays; // can't use default value optimization here because runtime default doesn't match config default
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.IssuedCookieLifetime, settings.IssuedCookieLifetime);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxClockSkew, settings.MaxClockSkew);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxPendingSessions, settings.MaxPendingSessions);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxStatefulNegotiations, settings.MaxStatefulNegotiations);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.NegotiationTimeout, settings.NegotiationTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReconnectTransportOnFailure, settings.ReconnectTransportOnFailure);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReplayCacheSize, settings.ReplayCacheSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReplayWindow, settings.ReplayWindow);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SessionKeyRenewalInterval, settings.SessionKeyRenewalInterval);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SessionKeyRolloverInterval, settings.SessionKeyRolloverInterval);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.InactivityTimeout, settings.InactivityTimeout);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TimestampValidityDuration, settings.TimestampValidityDuration);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxCachedCookies, settings.MaxCachedCookies);
        }

        internal void CopyFrom(LocalServiceSecuritySettingsElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            if (PropertyValueOrigin.Default != source.ElementInformation.Properties[ConfigurationStrings.DetectReplays].ValueOrigin)
                this.DetectReplays = source.DetectReplays;
            this.IssuedCookieLifetime = source.IssuedCookieLifetime;
            this.MaxClockSkew = source.MaxClockSkew;
            this.MaxPendingSessions = source.MaxPendingSessions;
            this.MaxStatefulNegotiations = source.MaxStatefulNegotiations;
            this.NegotiationTimeout = source.NegotiationTimeout;
            this.ReconnectTransportOnFailure = source.ReconnectTransportOnFailure;
            this.ReplayCacheSize = source.ReplayCacheSize;
            this.ReplayWindow = source.ReplayWindow;
            this.SessionKeyRenewalInterval = source.SessionKeyRenewalInterval;
            this.SessionKeyRolloverInterval = source.SessionKeyRolloverInterval;
            this.InactivityTimeout = source.InactivityTimeout;
            this.TimestampValidityDuration = source.TimestampValidityDuration;
            this.MaxCachedCookies = source.MaxCachedCookies;
        }
    }
}



