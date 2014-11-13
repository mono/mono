//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public class ChannelCacheSettings
    {
        TimeSpan idleTimeout;
        TimeSpan leaseTimeout;
        int maxItemsInCache;
        
        internal static ChannelCacheSettings EmptyCacheSettings = new ChannelCacheSettings { MaxItemsInCache = 0 };
        
        public ChannelCacheSettings()
        {
            this.idleTimeout = ChannelCacheDefaults.DefaultIdleTimeout;
            this.leaseTimeout = ChannelCacheDefaults.DefaultLeaseTimeout;
            this.maxItemsInCache = ChannelCacheDefaults.DefaultMaxItemsPerCache;
        }

        [Fx.Tag.KnownXamlExternal]
        public TimeSpan IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }

            set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value);
                
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("IdleTimeout", value, SR.ValueTooLarge("IdleTimeout"));
                }

                this.idleTimeout = value;
            }
        }

        [Fx.Tag.KnownXamlExternal]
        public TimeSpan LeaseTimeout
        {
            get
            {
                return leaseTimeout;
            }
            set
            {
                TimeoutHelper.ThrowIfNegativeArgument(value);

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("LeaseTimeout", value, SR.ValueTooLarge("LeaseTimeout"));
                }

                this.leaseTimeout = value;
            }
        }

        public int MaxItemsInCache
        {
            get
            {
                return this.maxItemsInCache;
            }
            set
            {
                if (value < 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("MaxItemsInCache", value, SR.ValueCannotBeNegative("MaxItemsInCache"));
                }

                this.maxItemsInCache = value;
            }
        }
    }
}
