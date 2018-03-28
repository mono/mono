//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class LocalClientSecuritySettings
    {
        bool detectReplays;
        int replayCacheSize;
        TimeSpan replayWindow;
        TimeSpan maxClockSkew;
        bool cacheCookies;
        TimeSpan maxCookieCachingTime;
        TimeSpan sessionKeyRenewalInterval;
        TimeSpan sessionKeyRolloverInterval;
        bool reconnectTransportOnFailure;
        TimeSpan timestampValidityDuration;
        IdentityVerifier identityVerifier;
        int cookieRenewalThresholdPercentage;
        NonceCache nonceCache = null;

        LocalClientSecuritySettings(LocalClientSecuritySettings other)
        {
            this.detectReplays = other.detectReplays;
            this.replayCacheSize = other.replayCacheSize;
            this.replayWindow = other.replayWindow;
            this.maxClockSkew = other.maxClockSkew;
            this.cacheCookies = other.cacheCookies;
            this.maxCookieCachingTime = other.maxCookieCachingTime;
            this.sessionKeyRenewalInterval = other.sessionKeyRenewalInterval;
            this.sessionKeyRolloverInterval = other.sessionKeyRolloverInterval;
            this.reconnectTransportOnFailure = other.reconnectTransportOnFailure;
            this.timestampValidityDuration = other.timestampValidityDuration;
            this.identityVerifier = other.identityVerifier;
            this.cookieRenewalThresholdPercentage = other.cookieRenewalThresholdPercentage;
            this.nonceCache = other.nonceCache;
        }

        public bool DetectReplays
        {
            get
            {
                return this.detectReplays;
            }
            set
            {
                this.detectReplays = value;
            }
        }

        public int ReplayCacheSize
        {
            get
            {
                return this.replayCacheSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
                }
                this.replayCacheSize = value;
            }
        }

        public TimeSpan ReplayWindow
        {
            get
            {
                return this.replayWindow;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.replayWindow = value;
            }
        }

        public TimeSpan MaxClockSkew
        {
            get
            {
                return this.maxClockSkew;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.maxClockSkew = value;
            }
        }

        public NonceCache NonceCache
        {
            get
            {
                return this.nonceCache;
            }
            set
            {
                this.nonceCache = value;
            }
        }

        public TimeSpan TimestampValidityDuration
        {
            get
            {
                return this.timestampValidityDuration;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.timestampValidityDuration = value;
            }
        }

        public bool CacheCookies
        {
            get
            {
                return this.cacheCookies;
            }
            set
            {
                this.cacheCookies = value;
            }
        }

        public TimeSpan MaxCookieCachingTime
        {
            get
            {
                return this.maxCookieCachingTime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.maxCookieCachingTime = value;
            }
        }

        public int CookieRenewalThresholdPercentage
        {
            get
            {
                return this.cookieRenewalThresholdPercentage;
            }
            set
            {
                if (value < 0 || value > 100)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                                                    SR.GetString(SR.ValueMustBeInRange, 0, 100)));
                }
                this.cookieRenewalThresholdPercentage = value;
            }
        }

        public TimeSpan SessionKeyRenewalInterval
        {
            get
            {
                return this.sessionKeyRenewalInterval;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.sessionKeyRenewalInterval = value;
            }
        }

        public TimeSpan SessionKeyRolloverInterval
        {
            get
            {
                return this.sessionKeyRolloverInterval;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.sessionKeyRolloverInterval = value;
            }
        }

        public bool ReconnectTransportOnFailure
        {
            get
            {
                return this.reconnectTransportOnFailure;
            }
            set
            {
                this.reconnectTransportOnFailure = value;
            }
        }

        public IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
            set
            {
                this.identityVerifier = value;
            }
        }

        public LocalClientSecuritySettings()
        {
            this.DetectReplays = SecurityProtocolFactory.defaultDetectReplays;
            this.ReplayCacheSize = SecurityProtocolFactory.defaultMaxCachedNonces;
            this.ReplayWindow = SecurityProtocolFactory.defaultReplayWindow;
            this.MaxClockSkew = SecurityProtocolFactory.defaultMaxClockSkew;
            this.TimestampValidityDuration = SecurityProtocolFactory.defaultTimestampValidityDuration;
            this.CacheCookies = IssuanceTokenProviderBase<IssuanceTokenProviderState>.defaultClientCacheTokens;
            this.MaxCookieCachingTime = IssuanceTokenProviderBase<IssuanceTokenProviderState>.DefaultClientMaxTokenCachingTime;
            this.SessionKeyRenewalInterval = SecuritySessionClientSettings.defaultKeyRenewalInterval;
            this.SessionKeyRolloverInterval = SecuritySessionClientSettings.defaultKeyRolloverInterval;
            this.ReconnectTransportOnFailure = SecuritySessionClientSettings.defaultTolerateTransportFailures;
            this.CookieRenewalThresholdPercentage = SpnegoTokenProvider.defaultServiceTokenValidityThresholdPercentage;
            this.IdentityVerifier = IdentityVerifier.CreateDefault();
            this.nonceCache = null;
        }

        public LocalClientSecuritySettings Clone()
        {
            return new LocalClientSecuritySettings(this);
        }
    }
}


