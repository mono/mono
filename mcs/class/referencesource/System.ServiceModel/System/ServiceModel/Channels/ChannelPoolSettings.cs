//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;

    public class ChannelPoolSettings
    {
        TimeSpan idleTimeout;
        TimeSpan leaseTimeout;
        int maxOutboundChannelsPerEndpoint;

        public ChannelPoolSettings()
        {
            this.idleTimeout = OneWayDefaults.IdleTimeout;
            this.leaseTimeout = OneWayDefaults.LeaseTimeout;
            this.maxOutboundChannelsPerEndpoint = OneWayDefaults.MaxOutboundChannelsPerEndpoint;
        }

        ChannelPoolSettings(ChannelPoolSettings poolToBeCloned)
        {
            this.idleTimeout = poolToBeCloned.idleTimeout;
            this.leaseTimeout = poolToBeCloned.leaseTimeout;
            this.maxOutboundChannelsPerEndpoint = poolToBeCloned.maxOutboundChannelsPerEndpoint;
        }

        [DefaultValue(typeof(TimeSpan), OneWayDefaults.IdleTimeoutString)]
        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
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

                this.idleTimeout = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), OneWayDefaults.LeaseTimeoutString)]
        public TimeSpan LeaseTimeout
        {
            get
            {
                return leaseTimeout;
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

                this.leaseTimeout = value;
            }
        }

        [DefaultValue(OneWayDefaults.MaxOutboundChannelsPerEndpoint)]
        public int MaxOutboundChannelsPerEndpoint
        {
            get
            {
                return this.maxOutboundChannelsPerEndpoint;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));
                }

                this.maxOutboundChannelsPerEndpoint = value;
            }
        }

        internal ChannelPoolSettings Clone()
        {
            return new ChannelPoolSettings(this);
        }

        internal bool IsMatch(ChannelPoolSettings channelPoolSettings)
        {
            if (channelPoolSettings == null)
            {
                return false;
            }

            if (this.idleTimeout != channelPoolSettings.idleTimeout)
            {
                return false;
            }

            if (this.leaseTimeout != channelPoolSettings.leaseTimeout)
            {
                return false;
            }

            if (this.maxOutboundChannelsPerEndpoint != channelPoolSettings.maxOutboundChannelsPerEndpoint)
            {
                return false;
            }

            return true;
        }

        internal bool InternalShouldSerialize()
        {
            return (this.maxOutboundChannelsPerEndpoint != OneWayDefaults.MaxOutboundChannelsPerEndpoint
                    || this.idleTimeout != OneWayDefaults.IdleTimeout
                    || this.leaseTimeout != OneWayDefaults.LeaseTimeout);
        }
    }
}
