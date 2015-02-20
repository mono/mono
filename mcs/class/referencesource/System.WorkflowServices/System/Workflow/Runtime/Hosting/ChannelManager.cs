//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Workflow.Activities;
    using System.Diagnostics;
    using System.Workflow.ComponentModel;

    class ChannelManager
    {
        static Uri defaultViaUri = new Uri("channelCache://default_address");

        PooledChannelPool channelPool;
        bool closed;
        Hashtable endpointMappings;

        Dictionary<EndpointAddress, ServiceEndpoint> endpoints;
        Dictionary<EndpointAddress, ChannelFactoryReference> factoryCache;

        List<PooledChannel> newChannels;

        public ChannelManager(ChannelPoolSettings settings, IList<ServiceEndpoint> endpoints)
        {
            this.channelPool = new PooledChannelPool(settings);
            this.factoryCache = new Dictionary<EndpointAddress, ChannelFactoryReference>();

            this.newChannels = new List<PooledChannel>();

            this.endpoints = new Dictionary<EndpointAddress, ServiceEndpoint>();
            this.endpointMappings = Hashtable.Synchronized(new Hashtable());

            if (endpoints != null)
            {
                foreach (ServiceEndpoint endpoint in endpoints)
                {
                    if (endpoint != null)
                    {
                        EndpointAddress cacheAddress = null;
                        if (endpoint.Contract.ContractType != null)
                        {
                            cacheAddress = ChannelManagerHelpers.BuildCacheAddress(endpoint.Name, endpoint.Contract.ContractType);
                        }
                        else
                        {
                            cacheAddress = ChannelManagerHelpers.BuildCacheAddress(endpoint.Name, endpoint.Contract.Name);
                        }
                        this.endpoints.Add(cacheAddress, endpoint);
                    }
                }
            }
        }

        object ThisLock
        {
            get
            {
                return this.channelPool;
            }
        }

        public void Close()
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return;
                }

                this.closed = true;

                this.newChannels.Clear();
                this.channelPool.Close(ServiceDefaults.CloseTimeout);

                foreach (ChannelFactoryReference factory in this.factoryCache.Values)
                {
                    factory.Close(ServiceDefaults.CloseTimeout);
                }
                this.factoryCache.Clear();

                this.endpointMappings.Clear();
                this.endpoints.Clear();
            }
        }

        public void ReturnChannel(ChannelPoolKey key, PooledChannel channel)
        {
            bool connectionIsStillGood = (channel.State == CommunicationState.Opened);
            bool newConnection = false;

            lock (this.ThisLock)
            {
                newConnection = this.newChannels.Remove(channel);
            }

            if (newConnection)
            {
                if (connectionIsStillGood)
                {
                    this.channelPool.AddConnection(key, channel, ServiceDefaults.OpenTimeout);
                }
                else
                {
                    channel.Abort();
                }
            }
            else
            {
                this.channelPool.ReturnConnection(key, channel, connectionIsStillGood, ServiceDefaults.CloseTimeout);
            }
        }

        public PooledChannel TakeChannel(string endpointName, Type contractType, string customAddress, out ChannelPoolKey key)
        {
            EndpointAddress cacheAddress = ChannelManagerHelpers.BuildCacheAddress(endpointName, contractType);
            Uri via = (customAddress != null) ? new Uri(customAddress) : defaultViaUri;

            if (this.closed)
            {
                key = null;
                return null;
            }

            this.endpointMappings[cacheAddress] = new KeyValuePair<string, Type>(endpointName, contractType);

            return this.TakeChannel(cacheAddress, via, out key);
        }

        public PooledChannel TakeChannel(EndpointAddress address, Uri via, out ChannelPoolKey key)
        {
            PooledChannel channel = null;
            key = null;

            if (!this.closed)
            {
                ChannelFactoryReference factory = this.TakeChannelFactory(address);
                if (factory != null)
                {
                    bool channelCreated = false;
                    try
                    {
                        EndpointAddress cacheAddress = address;
                        if (factory.SupportsClientAuthentication)
                        {
                            cacheAddress = ChannelManagerHelpers.BuildCacheAddressWithIdentity(address);
                        }

                        channel = this.channelPool.TakeConnection(cacheAddress, via, ServiceDefaults.OpenTimeout, out key);
                        while (channel != null && channel.State != CommunicationState.Opened)
                        {
                            // Loop will exit because non-opened channels are returned with 'connectionStillGood=false'
                            this.channelPool.ReturnConnection(key, channel, false, ServiceDefaults.CloseTimeout);
                            channel = this.channelPool.TakeConnection(cacheAddress, via, ServiceDefaults.OpenTimeout, out key);
                        }

                        if (channel == null)
                        {
                            channel = this.CreateChannel(factory, address, via, out channelCreated);
                        }
                    }
                    finally
                    {
                        if (!channelCreated)
                        {
                            this.ReturnChannelFactory(address, factory);
                        }
                    }
                }
            }

            return channel;
        }

        PooledChannel CreateChannel(ChannelFactoryReference factory, EndpointAddress address, Uri via, out bool channelCreated)
        {
            PooledChannel pooledChannel = null;
            channelCreated = false;

            IChannel channel = ChannelManagerHelpers.CreateChannel(factory.ContractType, factory.ChannelFactory, (via == defaultViaUri) ? null : via.ToString());
            if (channel != null)
            {
                pooledChannel = new PooledChannel(this, factory, address, channel);
                lock (this.ThisLock)
                {
                    this.newChannels.Add(pooledChannel);
                }

                channelCreated = true;
            }

            return pooledChannel;
        }

        ChannelFactoryReference CreateChannelFactory(EndpointAddress address)
        {
            KeyValuePair<string, Type> endpointData;

            if (this.endpointMappings.ContainsKey(address))
            {
                endpointData = (KeyValuePair<string, Type>) this.endpointMappings[address];
                if (endpointMappings != null)
                {
                    return new ChannelFactoryReference(
                        ChannelManagerHelpers.CreateChannelFactory(endpointData.Key, endpointData.Value, this.endpoints),
                        endpointData.Value);
                }
            }

            return null;
        }

        void ReturnChannelFactory(EndpointAddress key, ChannelFactoryReference channelFactory)
        {
            if (channelFactory == null)
            {
                return;
            }

            lock (this.ThisLock)
            {
                bool closeFactory = channelFactory.Release();
                bool invalidFactory = (channelFactory.ChannelFactory.State != CommunicationState.Opened);

                if (closeFactory || invalidFactory)
                {
                    ChannelFactoryReference currentFactory = null;
                    this.factoryCache.TryGetValue(key, out currentFactory);

                    if (currentFactory == channelFactory)
                    {
                        this.factoryCache.Remove(key);
                    }

                    if (closeFactory)
                    {
                        channelFactory.Close(ServiceDefaults.CloseTimeout);
                    }
                }
            }
        }

        ChannelFactoryReference TakeChannelFactory(EndpointAddress address)
        {
            ChannelFactoryReference factory;

            lock (ThisLock)
            {
                if (this.factoryCache.TryGetValue(address, out factory))
                {
                    if (factory == null || factory.ChannelFactory.State != CommunicationState.Opened)
                    {
                        this.factoryCache.Remove(address);
                        factory = null;
                    }
                }

                if (factory == null)
                {
                    factory = this.CreateChannelFactory(address);
                    if (factory != null)
                    {
                        this.factoryCache[address] = factory;
                    }
                }
                else
                {
                    factory.AddRef();
                }
            }

            return factory;
        }

        internal class ChannelFactoryReference
        {
            ChannelFactory channelFactory;
            Type contractType;
            int refCount;
            bool supportsClientAuthentication;

            public ChannelFactoryReference(ChannelFactory channelFactory, Type contractType)
            {
                this.refCount = 1;
                this.channelFactory = channelFactory;
                this.contractType = contractType;

                ISecurityCapabilities securityCapabilities = channelFactory.Endpoint.Binding.GetProperty<ISecurityCapabilities>(new BindingParameterCollection());
                if (securityCapabilities != null)
                {
                    this.supportsClientAuthentication = securityCapabilities.SupportsClientAuthentication;
                }
            }

            public ChannelFactory ChannelFactory
            {
                get
                {
                    return this.channelFactory;
                }
            }

            public Type ContractType
            {
                get
                {
                    return this.contractType;
                }
            }

            public bool SupportsClientAuthentication
            {
                get
                {
                    return this.supportsClientAuthentication;
                }
            }

            public void Abort()
            {
                ChannelManagerHelpers.CloseCommunicationObject(this.channelFactory);
            }

            public void Close(TimeSpan timeout)
            {
                ChannelManagerHelpers.CloseCommunicationObject(this.channelFactory, timeout);
            }

            internal void AddRef()
            {
                this.refCount++;
            }

            internal bool Release()
            {
                this.refCount--;
                return (this.refCount == 0);
            }
        }

        internal class PooledChannel
        {
            IChannel channel;
            bool closed;
            EndpointAddress factoryKey;
            ChannelFactoryReference factoryReference;
            ChannelManager owner;

            internal PooledChannel(ChannelManager owner, ChannelFactoryReference factoryReference, EndpointAddress factoryKey, IChannel channel)
            {
                this.owner = owner;
                this.factoryReference = factoryReference;
                this.factoryKey = factoryKey;
                this.channel = channel;
            }

            public IChannel InnerChannel
            {
                get
                {
                    return this.channel;
                }
            }

            public CommunicationState State
            {
                get
                {
                    return this.channel.State;
                }
            }

            public void Abort()
            {
                this.Close(ServiceDefaults.CloseTimeout);
            }

            public void Close(TimeSpan timeout)
            {
                if (this.closed)
                {
                    return;
                }

                bool flag = true;
                try
                {
                    if (this.channel.State == CommunicationState.Opened)
                    {
                        this.channel.Close(timeout);
                        flag = false;
                    }
                }
                catch (CommunicationException communicationException)
                {
                    DiagnosticUtility.TraceHandledException(communicationException, TraceEventType.Information);
                }
                catch (TimeoutException timeoutException)
                {
                    DiagnosticUtility.TraceHandledException(timeoutException, TraceEventType.Information);
                }
                finally
                {
                    if (flag)
                    {
                        this.channel.Abort();
                    }

                    this.owner.ReturnChannelFactory(this.factoryKey, this.factoryReference);

                    this.closed = true;
                }
            }
        }

        class PooledChannelPool : IdlingCommunicationPool<ChannelPoolKey, PooledChannel>
        {
            public PooledChannelPool(ChannelPoolSettings settings)
                : base(settings.MaxOutboundChannelsPerEndpoint, settings.IdleTimeout, settings.LeaseTimeout)
            {
            }

            protected override void AbortItem(PooledChannel item)
            {
                item.Abort();
            }

            protected override void CloseItem(PooledChannel item, TimeSpan timeout)
            {
                item.Close(timeout);
            }

            protected override ChannelPoolKey GetPoolKey(EndpointAddress address, Uri via)
            {
                return new ChannelPoolKey(address, via);
            }
        }
    }
}
