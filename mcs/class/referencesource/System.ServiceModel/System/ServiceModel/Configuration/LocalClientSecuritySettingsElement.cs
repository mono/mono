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

    public sealed partial class LocalClientSecuritySettingsElement : ServiceModelConfigurationElement
    {
        public LocalClientSecuritySettingsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.CacheCookies, DefaultValue = SpnegoTokenProvider.defaultClientCacheTokens)]
        public bool CacheCookies
        {
            get { return (bool)base[ConfigurationStrings.CacheCookies]; }
            set { base[ConfigurationStrings.CacheCookies] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.DetectReplays, DefaultValue = SecurityProtocolFactory.defaultDetectReplays)]
        public bool DetectReplays
        {
            get { return (bool)base[ConfigurationStrings.DetectReplays]; }
            set { base[ConfigurationStrings.DetectReplays] = value; }
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

        [ConfigurationProperty(ConfigurationStrings.MaxCookieCachingTime, DefaultValue = SpnegoTokenProvider.defaultClientMaxTokenCachingTimeString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan MaxCookieCachingTime
        {
            get { return (TimeSpan)base[ConfigurationStrings.MaxCookieCachingTime]; }
            set { base[ConfigurationStrings.MaxCookieCachingTime] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReplayWindow, DefaultValue = SecurityProtocolFactory.defaultReplayWindowString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan ReplayWindow
        {
            get { return (TimeSpan)base[ConfigurationStrings.ReplayWindow]; }
            set { base[ConfigurationStrings.ReplayWindow] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SessionKeyRenewalInterval, DefaultValue = SecuritySessionClientSettings.defaultKeyRenewalIntervalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan SessionKeyRenewalInterval
        {
            get { return (TimeSpan)base[ConfigurationStrings.SessionKeyRenewalInterval]; }
            set { base[ConfigurationStrings.SessionKeyRenewalInterval] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SessionKeyRolloverInterval, DefaultValue = SecuritySessionClientSettings.defaultKeyRolloverIntervalString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan SessionKeyRolloverInterval
        {
            get { return (TimeSpan)base[ConfigurationStrings.SessionKeyRolloverInterval]; }
            set { base[ConfigurationStrings.SessionKeyRolloverInterval] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.ReconnectTransportOnFailure, DefaultValue = System.ServiceModel.Security.SecuritySessionClientSettings.defaultTolerateTransportFailures)]
        public bool ReconnectTransportOnFailure
        {
            get { return (bool)base[ConfigurationStrings.ReconnectTransportOnFailure]; }
            set { base[ConfigurationStrings.ReconnectTransportOnFailure] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.TimestampValidityDuration, DefaultValue = SecurityProtocolFactory.defaultTimestampValidityDurationString)]
        [TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        [ServiceModelTimeSpanValidator(MinValueString = ConfigurationStrings.TimeSpanZero)]
        public TimeSpan TimestampValidityDuration
        {
            get { return (TimeSpan)base[ConfigurationStrings.TimestampValidityDuration]; }
            set { base[ConfigurationStrings.TimestampValidityDuration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.CookieRenewalThresholdPercentage, DefaultValue = SpnegoTokenProvider.defaultServiceTokenValidityThresholdPercentage)]
        [IntegerValidator(MinValue = 0, MaxValue = 100)]
        public int CookieRenewalThresholdPercentage
        {
            get { return (int)base[ConfigurationStrings.CookieRenewalThresholdPercentage]; }
            set { base[ConfigurationStrings.CookieRenewalThresholdPercentage] = value; }
        }

        internal void ApplyConfiguration(LocalClientSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            settings.CacheCookies = this.CacheCookies;
            if (PropertyValueOrigin.Default != this.ElementInformation.Properties[ConfigurationStrings.DetectReplays].ValueOrigin)
                settings.DetectReplays = this.DetectReplays;
            settings.MaxClockSkew = this.MaxClockSkew;
            settings.MaxCookieCachingTime = this.MaxCookieCachingTime;
            settings.ReconnectTransportOnFailure = this.ReconnectTransportOnFailure;
            settings.ReplayCacheSize = this.ReplayCacheSize;
            settings.ReplayWindow = this.ReplayWindow;
            settings.SessionKeyRenewalInterval = this.SessionKeyRenewalInterval;
            settings.SessionKeyRolloverInterval = this.SessionKeyRolloverInterval;
            settings.TimestampValidityDuration = this.TimestampValidityDuration;
            settings.CookieRenewalThresholdPercentage = this.CookieRenewalThresholdPercentage;
        }

        internal void InitializeFrom(LocalClientSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CacheCookies, settings.CacheCookies);
            this.DetectReplays = settings.DetectReplays; // can't use default value optimization here because ApplyConfiguration looks at ValueOrigin
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxClockSkew, settings.MaxClockSkew);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.MaxCookieCachingTime, settings.MaxCookieCachingTime);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReconnectTransportOnFailure, settings.ReconnectTransportOnFailure);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReplayCacheSize, settings.ReplayCacheSize);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.ReplayWindow, settings.ReplayWindow);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SessionKeyRenewalInterval, settings.SessionKeyRenewalInterval);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.SessionKeyRolloverInterval, settings.SessionKeyRolloverInterval);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.TimestampValidityDuration, settings.TimestampValidityDuration);
            SetPropertyValueIfNotDefaultValue(ConfigurationStrings.CookieRenewalThresholdPercentage, settings.CookieRenewalThresholdPercentage);
        }

        internal void CopyFrom(LocalClientSecuritySettingsElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.CacheCookies = source.CacheCookies;
            if (PropertyValueOrigin.Default != source.ElementInformation.Properties[ConfigurationStrings.DetectReplays].ValueOrigin)
                this.DetectReplays = source.DetectReplays;
            this.MaxClockSkew = source.MaxClockSkew;
            this.MaxCookieCachingTime = source.MaxCookieCachingTime;
            this.ReconnectTransportOnFailure = source.ReconnectTransportOnFailure;
            this.ReplayCacheSize = source.ReplayCacheSize;
            this.ReplayWindow = source.ReplayWindow;
            this.SessionKeyRenewalInterval = source.SessionKeyRenewalInterval;
            this.SessionKeyRolloverInterval = source.SessionKeyRolloverInterval;
            this.TimestampValidityDuration = source.TimestampValidityDuration;
            this.CookieRenewalThresholdPercentage = source.CookieRenewalThresholdPercentage;
        }
    }
}



