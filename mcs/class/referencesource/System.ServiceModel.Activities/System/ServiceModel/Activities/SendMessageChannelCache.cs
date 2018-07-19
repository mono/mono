//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.Collections;

    public sealed class SendMessageChannelCache : IDisposable, ICancelable
    {
        static Func<SendMessageChannelCache> defaultExtensionProvider = new Func<SendMessageChannelCache>(CreateDefaultExtension);

        ChannelCacheSettings channelCacheSettings;
        ChannelCacheSettings factoryCacheSettings;
        bool isReadOnly;
        bool allowUnsafeCaching;
        bool isDisposed;
        ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> factoryCache;
        object thisLock;
        
        public SendMessageChannelCache()
            : this(null, null, ChannelCacheDefaults.DefaultAllowUnsafeSharing)
        {
        }

        public SendMessageChannelCache(ChannelCacheSettings factorySettings, ChannelCacheSettings channelSettings) :
            this(factorySettings, channelSettings, ChannelCacheDefaults.DefaultAllowUnsafeSharing)
        {
        }

        // use the default settings if null is specified for FactoryCacheSettings or ChannelCacheSettings
        public SendMessageChannelCache(ChannelCacheSettings factorySettings, ChannelCacheSettings channelSettings, bool allowUnsafeCaching)
        {
            this.allowUnsafeCaching = allowUnsafeCaching;
            this.FactorySettings = factorySettings;
            this.ChannelSettings = channelSettings;
            this.thisLock = new Object();
        }

        internal static Func<SendMessageChannelCache> DefaultExtensionProvider
        {
            get
            {
                return defaultExtensionProvider;
            }
        }

        public bool AllowUnsafeCaching
        {
            get 
            {
                return this.allowUnsafeCaching;
            }
            set 
            {
                ThrowIfReadOnly();
                this.allowUnsafeCaching = value;
            }
        }

        public ChannelCacheSettings ChannelSettings
        {
            get
            {
                return this.channelCacheSettings;
            }
            set
            {
                ThrowIfReadOnly();
                
                if (value == null)
                {
                    this.channelCacheSettings = new ChannelCacheSettings { LeaseTimeout = ChannelCacheDefaults.DefaultChannelLeaseTimeout };
                }
                else
                {
                    this.channelCacheSettings = value;
                }
            }
        }

        public ChannelCacheSettings FactorySettings
        {
            get
            {
                return this.factoryCacheSettings;
            }
            set
            {
                ThrowIfReadOnly();
                if (value == null)
                {
                    this.factoryCacheSettings = new ChannelCacheSettings { LeaseTimeout = ChannelCacheDefaults.DefaultFactoryLeaseTimeout };
                }
                else
                {
                    this.factoryCacheSettings = value;
                }
            }
        }

        static SendMessageChannelCache CreateDefaultExtension()
        {
            SendMessageChannelCache defaultExtension = new SendMessageChannelCache();
            defaultExtension.FactorySettings.LeaseTimeout = ChannelCacheDefaults.DefaultFactoryLeaseTimeout;
            defaultExtension.ChannelSettings.LeaseTimeout = ChannelCacheDefaults.DefaultChannelLeaseTimeout;
            return defaultExtension;
        }
               
        // factory cache will be created on first usage after which the settings are immutable
        internal ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> GetFactoryCache()
        {
            if (this.factoryCache == null)
            {
                this.isReadOnly = true;
                lock (thisLock)
                {
                    ThrowIfDisposed();
           
                    if (this.factoryCache == null)
                    {
                        // we don't need to set DisposeItemCallback since InternalSendMessage.ChannelFactoryReference is IDisposable
                        ObjectCacheSettings objectCacheSettings = new ObjectCacheSettings
                        {
                            CacheLimit = this.FactorySettings.MaxItemsInCache,
                            IdleTimeout = this.FactorySettings.IdleTimeout,
                            LeaseTimeout = this.FactorySettings.LeaseTimeout
                        };
                        this.factoryCache = new ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference>(objectCacheSettings);
                    }
                }
            }
            return this.factoryCache;
        }

        void ThrowIfReadOnly()
        {
            if (this.isReadOnly)
            {
                // cache has already been created, settings cannot be changed now
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CacheSettingsLocked));
            }
        }

        public void Dispose()
        {
            // Close the ChannelFactory and the Channels
            // SendMessageChannelCache cannot be used after Dispose is called
            if (!this.isDisposed)
            {
                lock (thisLock)
                {
                    if (!this.isDisposed)
                    {
                        if (this.factoryCache != null)
                        {
                            this.factoryCache.Dispose();

                        }
                        this.isDisposed = true;
                    }
                }
            }
        }

        void ICancelable.Cancel()
        {
            Dispose();
        }

        void ThrowIfDisposed()
        {
            if (this.isDisposed == true)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(typeof(SendMessageChannelCache).ToString()));
            }
        }
   }
}
