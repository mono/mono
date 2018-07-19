//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics.Application;

    sealed class ChannelFactoryRef<TChannel>
        where TChannel : class
    {
        ChannelFactory<TChannel> channelFactory;
        int refCount = 1;

        public ChannelFactoryRef(ChannelFactory<TChannel> channelFactory)
        {
            this.channelFactory = channelFactory;
        }

        public void AddRef()
        {
            this.refCount++;
        }

        // The caller should call Close/Abort when the return value is true.
        public bool Release()
        {
            --this.refCount;
            Fx.Assert(this.refCount >= 0, "RefCount should not be less than zero.");

            if (this.refCount == 0)
            {
                return true;
            }

            return false;
        }

        public void Close(TimeSpan timeout)
        {
            this.channelFactory.Close(timeout);
        }

        public void Abort()
        {
            this.channelFactory.Abort();
        }

        public ChannelFactory<TChannel> ChannelFactory
        {
            get
            {
                return this.channelFactory;
            }
        }
    }

    class ChannelFactoryRefCache<TChannel> : MruCache<EndpointTrait<TChannel>, ChannelFactoryRef<TChannel>>
       where TChannel : class
    {
        static EndpointTraitComparer DefaultEndpointTraitComparer = new EndpointTraitComparer();
        readonly int watermark;

        class EndpointTraitComparer : IEqualityComparer<EndpointTrait<TChannel>>
        {
            public bool Equals(EndpointTrait<TChannel> x, EndpointTrait<TChannel> y)
            {
                if (x != null)
                {
                    if (y != null)
                        return x.Equals(y);

                    return false;
                }

                if (y != null)
                    return false;

                return true;
            }

            public int GetHashCode(EndpointTrait<TChannel> obj)
            {
                if (obj == null)
                    return 0;

                return obj.GetHashCode();
            }
        }

        public ChannelFactoryRefCache(int watermark)
            : base(watermark * 4 / 5, watermark, DefaultEndpointTraitComparer)
        {
            this.watermark = watermark;
        }

        protected override void OnSingleItemRemoved(ChannelFactoryRef<TChannel> item)
        {
            // Remove from cache.
            if (item.Release())
            {
                item.Abort();
            }

            if (TD.ClientBaseCachedChannelFactoryCountIsEnabled())
            {
                TD.ClientBaseCachedChannelFactoryCount(this.Count, this.watermark, this);
            }
        }

        protected override void OnItemAgedOutOfCache(ChannelFactoryRef<TChannel> item)
        {
            if (TD.ClientBaseChannelFactoryAgedOutofCacheIsEnabled())
            {
                TD.ClientBaseChannelFactoryAgedOutofCache(this.watermark, this);
            }
        }
    }
}
