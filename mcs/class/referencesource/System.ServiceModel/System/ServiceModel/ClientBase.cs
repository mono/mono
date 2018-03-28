//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;
    using System.Runtime.Remoting.Messaging;
    using System.ServiceModel.Dispatcher;
    using System.Security;

    public abstract class ClientBase<TChannel> : ICommunicationObject, IDisposable
        where TChannel : class
    {
        TChannel channel;
        ChannelFactoryRef<TChannel> channelFactoryRef;
        EndpointTrait<TChannel> endpointTrait;

        // Determine whether the proxy can share factory with others. It is false only if the public getters
        // are invoked.
        bool canShareFactory = true;

        // Determine whether the proxy is currently holding a cached factory
        bool useCachedFactory;

        // Determine whether we have locked down sharing for this proxy. This is turned on only when the channel
        // is created.
        bool sharingFinalized;

        // Determine whether the ChannelFactoryRef has been released. We should release it only once per proxy
        bool channelFactoryRefReleased;

        // Determine whether we have released the last ref count of the ChannelFactory so that we could abort it when it was closing.
        bool releasedLastRef;

        object syncRoot = new object();

        object finalizeLock = new object();

        // Cache at most 32 ChannelFactories
        const int maxNumChannelFactories = 32;
        static ChannelFactoryRefCache<TChannel> factoryRefCache = new ChannelFactoryRefCache<TChannel>(maxNumChannelFactories);
        static object staticLock = new object();

        static object cacheLock = new object();
        static CacheSetting cacheSetting = CacheSetting.Default;
        static bool isCacheSettingReadOnly;

        static AsyncCallback onAsyncCallCompleted = Fx.ThunkCallback(new AsyncCallback(OnAsyncCallCompleted));

        // IMPORTANT: any changes to the set of protected .ctors of this class need to be reflected
        // in ServiceContractGenerator.cs as well.

        protected ClientBase()
        {
            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>("*"));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>("*", null, null);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(string endpointConfigurationName)
        {
            if (endpointConfigurationName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(endpointConfigurationName));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>(endpointConfigurationName, null, null);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(string endpointConfigurationName, string remoteAddress)
        {
            if (endpointConfigurationName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            if (remoteAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");

            MakeCacheSettingReadOnly();
            EndpointAddress endpointAddress = new EndpointAddress(remoteAddress);

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(endpointConfigurationName, endpointAddress));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>(endpointConfigurationName, endpointAddress, null);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            if (endpointConfigurationName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            if (remoteAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(endpointConfigurationName, remoteAddress));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>(endpointConfigurationName, remoteAddress, null);
                InitializeChannelFactoryRef();
            }

        }

        protected ClientBase(Binding binding, EndpointAddress remoteAddress)
        {
            if (binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            if (remoteAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOn)
            {
                this.endpointTrait = new ProgrammaticEndpointTrait<TChannel>(binding, remoteAddress, null);
                InitializeChannelFactoryRef();
            }
            else
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(binding, remoteAddress));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
        }

        protected ClientBase(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOn)
            {
                this.endpointTrait = new ServiceEndpointTrait<TChannel>(endpoint, null);
                this.InitializeChannelFactoryRef();
            }
            else
            {
                channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(endpoint));
                channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
        }

        protected ClientBase(InstanceContext callbackInstance)
        {
            if (callbackInstance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(
                    new DuplexChannelFactory<TChannel>(callbackInstance, "*"));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>("*", null, callbackInstance);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName)
        {
            if (callbackInstance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            if (endpointConfigurationName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(
                    new DuplexChannelFactory<TChannel>(callbackInstance, endpointConfigurationName));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>(endpointConfigurationName, null, callbackInstance);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress)
        {
            if (callbackInstance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            if (endpointConfigurationName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            if (remoteAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");

            MakeCacheSettingReadOnly();
            EndpointAddress endpointAddress = new EndpointAddress(remoteAddress);

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(
                    new DuplexChannelFactory<TChannel>(callbackInstance, endpointConfigurationName, endpointAddress));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>(endpointConfigurationName, endpointAddress, callbackInstance);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            if (callbackInstance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            if (endpointConfigurationName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            if (remoteAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOff)
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(
                    new DuplexChannelFactory<TChannel>(callbackInstance, endpointConfigurationName, remoteAddress));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
            else
            {
                this.endpointTrait = new ConfigurationEndpointTrait<TChannel>(endpointConfigurationName, remoteAddress, callbackInstance);
                InitializeChannelFactoryRef();
            }
        }

        protected ClientBase(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
        {
            if (callbackInstance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            if (binding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            if (remoteAddress == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOn)
            {
                this.endpointTrait = new ProgrammaticEndpointTrait<TChannel>(binding, remoteAddress, callbackInstance);
                InitializeChannelFactoryRef();
            }
            else
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(
                    new DuplexChannelFactory<TChannel>(callbackInstance, binding, remoteAddress));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
        }

        protected ClientBase(InstanceContext callbackInstance, ServiceEndpoint endpoint)
        {
            if (callbackInstance == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            if (endpoint == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");

            MakeCacheSettingReadOnly();

            if (cacheSetting == CacheSetting.AlwaysOn)
            {
                this.endpointTrait = new ServiceEndpointTrait<TChannel>(endpoint, callbackInstance);
                InitializeChannelFactoryRef();
            }
            else
            {
                this.channelFactoryRef = new ChannelFactoryRef<TChannel>(
                    new DuplexChannelFactory<TChannel>(callbackInstance, endpoint));
                this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
                TryDisableSharing();
            }
        }

        protected T GetDefaultValueForInitialization<T>()
        {
            return default(T);
        }

        object ThisLock
        {
            get
            {
                return syncRoot;
            }
        }

        protected TChannel Channel
        {
            get
            {
                // created on demand, so that Mort can modify .Endpoint before calling methods on the client
                if (this.channel == null)
                {
                    lock (ThisLock)
                    {
                        if (this.channel == null)
                        {
                            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
                            {
                                if (DiagnosticUtility.ShouldUseActivity)
                                {
                                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityOpenClientBase, typeof(TChannel).FullName), ActivityType.OpenClient);
                                }

                                if (this.useCachedFactory)
                                {
                                    try
                                    {
                                        CreateChannelInternal();
                                    }
#pragma warning suppress 56500 // covered by FxCOP
                                    catch (Exception ex)
                                    {
                                        if (this.useCachedFactory &&
                                            (ex is CommunicationException ||
                                            ex is ObjectDisposedException ||
                                            ex is TimeoutException))
                                        {
                                            DiagnosticUtility.TraceHandledException(ex, TraceEventType.Warning);
                                            InvalidateCacheAndCreateChannel();
                                        }
                                        else
                                        {
#pragma warning suppress 56503 // Microsoft, We throw only for unknown exceptions.
                                            throw;
                                        }
                                    }
                                }
                                else
                                {
                                    CreateChannelInternal();
                                }
                            }
                        }
                    }
                }
                return channel;
            }
        }

        public static CacheSetting CacheSetting
        {
            get
            {
                return cacheSetting;
            }
            set
            {
                lock (cacheLock)
                {
                    if (isCacheSettingReadOnly && cacheSetting != value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxImmutableClientBaseCacheSetting, typeof(TChannel).ToString())));
                    }
                    else
                    {
                        cacheSetting = value;
                    }
                }
            }
        }

        public ChannelFactory<TChannel> ChannelFactory
        {
            get
            {
                if (cacheSetting == CacheSetting.Default)
                {
                    TryDisableSharing();
                }
                return GetChannelFactory();
            }
        }

        public ClientCredentials ClientCredentials
        {
            get
            {
                if (cacheSetting == CacheSetting.Default)
                {
                    TryDisableSharing();
                }
                return this.ChannelFactory.Credentials;
            }
        }

        public CommunicationState State
        {
            get
            {
                IChannel channel = (IChannel)this.channel;
                if (channel != null)
                {
                    return channel.State;
                }
                else
                {
                    // we may have failed to create the channel under open, in which case we our factory wouldn't be open
                    if (!this.useCachedFactory)
                    {
                        return GetChannelFactory().State;
                    }
                    else
                    {
                        return CommunicationState.Created;
                    }
                }
            }
        }

        public IClientChannel InnerChannel
        {
            get
            {
                return (IClientChannel)Channel;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                if (cacheSetting == CacheSetting.Default)
                {
                    TryDisableSharing();
                }
                return GetChannelFactory().Endpoint;
            }
        }

        public void Open()
        {
            ((ICommunicationObject)this).Open(GetChannelFactory().InternalOpenTimeout);
        }

        public void Abort()
        {
            IChannel channel = (IChannel)this.channel;
            if (channel != null)
            {
                channel.Abort();
            }

            if (!channelFactoryRefReleased)
            {
                lock (staticLock)
                {
                    if (!channelFactoryRefReleased)
                    {
                        if (this.channelFactoryRef.Release())
                        {
                            this.releasedLastRef = true;
                        }

                        channelFactoryRefReleased = true;
                    }
                }
            }

            // Abort the ChannelFactory if we released the last one. We should be able to abort it when another thread is closing it.
            if (this.releasedLastRef)
            {
                this.channelFactoryRef.Abort();
            }
        }

        public void Close()
        {
            ((ICommunicationObject)this).Close(GetChannelFactory().InternalCloseTimeout);
        }

        public void DisplayInitializationUI()
        {
            ((IClientChannel)this.InnerChannel).DisplayInitializationUI();
        }

        // This ensures that the cachesetting (on, off or default) cannot be modified by 
        // another ClientBase instance of matching TChannel after the first instance is created.
        void MakeCacheSettingReadOnly()
        {
            if (isCacheSettingReadOnly)
                return;

            lock (cacheLock)
            {
                isCacheSettingReadOnly = true;
            }
        }

        void CreateChannelInternal()
        {
            try
            {
                this.channel = this.CreateChannel();
                if (this.sharingFinalized)
                {
                    if (this.canShareFactory && !this.useCachedFactory)
                    {
                        // It is OK to add ChannelFactory to the cache now.
                        TryAddChannelFactoryToCache();
                    }
                }
            }
            finally
            {
                if (!this.sharingFinalized && cacheSetting == CacheSetting.Default)
                {
                    // this.CreateChannel() is not called. For safety, we disable sharing.
                    TryDisableSharing();
                }
            }
        }

        protected virtual TChannel CreateChannel()
        {
            if (this.sharingFinalized)
                return GetChannelFactory().CreateChannel();

            lock (this.finalizeLock)
            {
                this.sharingFinalized = true;
                return GetChannelFactory().CreateChannel();
            }
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        void ICommunicationObject.Open(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (!this.useCachedFactory)
            {
                GetChannelFactory().Open(timeoutHelper.RemainingTime());
            }

            this.InnerChannel.Open(timeoutHelper.RemainingTime());
        }

        void ICommunicationObject.Close(TimeSpan timeout)
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityCloseClientBase, typeof(TChannel).FullName), ActivityType.Close);
                }
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                if (this.channel != null)
                {
                    InnerChannel.Close(timeoutHelper.RemainingTime());
                }

                if (!channelFactoryRefReleased)
                {
                    lock (staticLock)
                    {
                        if (!channelFactoryRefReleased)
                        {
                            if (this.channelFactoryRef.Release())
                            {
                                this.releasedLastRef = true;
                            }

                            this.channelFactoryRefReleased = true;
                        }
                    }

                    // Close the factory outside of the lock so that we can abort from a different thread.
                    if (this.releasedLastRef)
                    {
                        if (this.useCachedFactory)
                        {
                            this.channelFactoryRef.Abort();
                        }
                        else
                        {
                            this.channelFactoryRef.Close(timeoutHelper.RemainingTime());
                        }
                    }
                }
            }
        }

        event EventHandler ICommunicationObject.Closed
        {
            add
            {
                this.InnerChannel.Closed += value;
            }
            remove
            {
                this.InnerChannel.Closed -= value;
            }
        }

        event EventHandler ICommunicationObject.Closing
        {
            add
            {
                this.InnerChannel.Closing += value;
            }
            remove
            {
                this.InnerChannel.Closing -= value;
            }
        }

        event EventHandler ICommunicationObject.Faulted
        {
            add
            {
                this.InnerChannel.Faulted += value;
            }
            remove
            {
                this.InnerChannel.Faulted -= value;
            }
        }

        event EventHandler ICommunicationObject.Opened
        {
            add
            {
                this.InnerChannel.Opened += value;
            }
            remove
            {
                this.InnerChannel.Opened -= value;
            }
        }

        event EventHandler ICommunicationObject.Opening
        {
            add
            {
                this.InnerChannel.Opening += value;
            }
            remove
            {
                this.InnerChannel.Opening -= value;
            }
        }

        IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)this).BeginClose(GetChannelFactory().InternalCloseTimeout, callback, state);
        }

        IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, BeginChannelClose, EndChannelClose, BeginFactoryClose, EndFactoryClose);
        }

        void ICommunicationObject.EndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject)this).BeginOpen(GetChannelFactory().InternalOpenTimeout, callback, state);
        }

        IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, BeginFactoryOpen, EndFactoryOpen, BeginChannelOpen, EndChannelOpen);
        }

        void ICommunicationObject.EndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        //ChainedAsyncResult methods for opening and closing ChannelFactory<T>

        internal IAsyncResult BeginFactoryOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.useCachedFactory)
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                return GetChannelFactory().BeginOpen(timeout, callback, state);
            }
        }

        internal void EndFactoryOpen(IAsyncResult result)
        {
            if (this.useCachedFactory)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                GetChannelFactory().EndOpen(result);
            }
        }

        internal IAsyncResult BeginChannelOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginOpen(timeout, callback, state);
        }

        internal void EndChannelOpen(IAsyncResult result)
        {
            this.InnerChannel.EndOpen(result);
        }

        internal IAsyncResult BeginFactoryClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.useCachedFactory)
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                return GetChannelFactory().BeginClose(timeout, callback, state);
            }
        }

        internal void EndFactoryClose(IAsyncResult result)
        {
            if (typeof(CompletedAsyncResult).IsAssignableFrom(result.GetType()))
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                GetChannelFactory().EndClose(result);
            }
        }

        internal IAsyncResult BeginChannelClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.channel != null)
            {
                return this.InnerChannel.BeginClose(timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        internal void EndChannelClose(IAsyncResult result)
        {
            if (typeof(CompletedAsyncResult).IsAssignableFrom(result.GetType()))
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.InnerChannel.EndClose(result);
            }
        }

        ChannelFactory<TChannel> GetChannelFactory()
        {
            return this.channelFactoryRef.ChannelFactory;
        }

        void InitializeChannelFactoryRef()
        {
            Fx.Assert(this.channelFactoryRef == null, "The channelFactory should have never been assigned");
            Fx.Assert(this.canShareFactory, "GetChannelFactoryFromCache can be called only when canShareFactory is true");
            lock (staticLock)
            {
                ChannelFactoryRef<TChannel> factoryRef;
                if (factoryRefCache.TryGetValue(this.endpointTrait, out factoryRef))
                {
                    if (factoryRef.ChannelFactory.State != CommunicationState.Opened)
                    {
                        // Remove the bad ChannelFactory.
                        factoryRefCache.Remove(this.endpointTrait);
                    }
                    else
                    {
                        this.channelFactoryRef = factoryRef;
                        this.channelFactoryRef.AddRef();
                        useCachedFactory = true;
                        if (TD.ClientBaseChannelFactoryCacheHitIsEnabled())
                        {
                            TD.ClientBaseChannelFactoryCacheHit(this);
                        }
                        return;
                    }
                }
            }

            if (this.channelFactoryRef == null)
            {
                // Creating the ChannelFactory at initial time to catch configuration exception earlier.
                this.channelFactoryRef = CreateChannelFactoryRef(this.endpointTrait);
            }
        }

        static ChannelFactoryRef<TChannel> CreateChannelFactoryRef(EndpointTrait<TChannel> endpointTrait)
        {
            Fx.Assert(endpointTrait != null, "The endpointTrait should not be null when the factory can be shared.");

            ChannelFactory<TChannel> channelFactory = endpointTrait.CreateChannelFactory();
            channelFactory.TraceOpenAndClose = false;
            return new ChannelFactoryRef<TChannel>(channelFactory);
        }

        // Once the channel is created, we can't disable caching.
        // This method can be called safely multiple times.  
        // this.sharingFinalized is set the first time the method is called.
        // Subsequent calls are essentially no-ops.
        void TryDisableSharing()
        {
            if (this.sharingFinalized)
                return;

            lock (this.finalizeLock)
            {
                if (this.sharingFinalized)
                    return;

                this.canShareFactory = false;
                this.sharingFinalized = true;

                if (this.useCachedFactory)
                {
                    ChannelFactoryRef<TChannel> pendingFactoryRef = this.channelFactoryRef;
                    this.channelFactoryRef = CreateChannelFactoryRef(this.endpointTrait);
                    this.useCachedFactory = false;

                    lock (staticLock)
                    {
                        if (!pendingFactoryRef.Release())
                        {
                            pendingFactoryRef = null;
                        }
                    }

                    if (pendingFactoryRef != null)
                        pendingFactoryRef.Abort();
                }
            }

            // can be done outside the lock since the lines below do not access shared data.
            // also the use of this.sharingFinalized in the lines above ensures that tracing 
            // happens only once and only when needed.
            if (TD.ClientBaseUsingLocalChannelFactoryIsEnabled())
            {
                TD.ClientBaseUsingLocalChannelFactory(this);
            }
        }

        void TryAddChannelFactoryToCache()
        {
            Fx.Assert(this.canShareFactory, "This should be called only when this proxy can share ChannelFactory.");
            Fx.Assert(this.channelFactoryRef.ChannelFactory.State == CommunicationState.Opened,
                "The ChannelFactory must be in Opened state for caching.");

            // Lock the cache and add the item to synchronize with lookup.
            lock (staticLock)
            {
                ChannelFactoryRef<TChannel> cfRef;
                if (!factoryRefCache.TryGetValue(this.endpointTrait, out cfRef))
                {
                    // Increment the ref count before adding to the cache.
                    this.channelFactoryRef.AddRef();
                    factoryRefCache.Add(this.endpointTrait, this.channelFactoryRef);
                    this.useCachedFactory = true;
                    if (TD.ClientBaseCachedChannelFactoryCountIsEnabled())
                    {
                        TD.ClientBaseCachedChannelFactoryCount(factoryRefCache.Count, maxNumChannelFactories, this);
                    }
                }
            }
        }

        // NOTE: This should be called inside ThisLock
        void InvalidateCacheAndCreateChannel()
        {
            RemoveFactoryFromCache();
            TryDisableSharing();
            CreateChannelInternal();
        }

        void RemoveFactoryFromCache()
        {
            lock (staticLock)
            {
                ChannelFactoryRef<TChannel> factoryRef;
                if (factoryRefCache.TryGetValue(this.endpointTrait, out factoryRef))
                {
                    if (object.ReferenceEquals(this.channelFactoryRef, factoryRef))
                    {
                        factoryRefCache.Remove(this.endpointTrait);
                    }
                }
            }
        }

        // WARNING: changes in the signature/name of the following delegates must be applied to the 
        // ClientClassGenerator.cs as well, otherwise the ClientClassGenerator would generate wrong code.
        protected delegate IAsyncResult BeginOperationDelegate(object[] inValues, AsyncCallback asyncCallback, object state);
        protected delegate object[] EndOperationDelegate(IAsyncResult result);

        // WARNING: Any changes in the signature/name of the following type and its ctor must be applied to the 
        // ClientClassGenerator.cs as well, otherwise the ClientClassGenerator would generate wrong code.
        protected class InvokeAsyncCompletedEventArgs : AsyncCompletedEventArgs
        {
            object[] results;

            internal InvokeAsyncCompletedEventArgs(object[] results, Exception error, bool cancelled, object userState)
                : base(error, cancelled, userState)
            {
                this.results = results;
            }

            public object[] Results
            {
                get
                {
                    return this.results;
                }
            }
        }

        // WARNING: Any changes in the signature/name of the following method ctor must be applied to the 
        // ClientClassGenerator.cs as well, otherwise the ClientClassGenerator would generate wrong code.
        protected void InvokeAsync(BeginOperationDelegate beginOperationDelegate, object[] inValues,
            EndOperationDelegate endOperationDelegate, SendOrPostCallback operationCompletedCallback, object userState)
        {
            if (beginOperationDelegate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("beginOperationDelegate");
            }
            if (endOperationDelegate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endOperationDelegate");
            }

            AsyncOperation asyncOperation = AsyncOperationManager.CreateOperation(userState);
            AsyncOperationContext context = new AsyncOperationContext(asyncOperation, endOperationDelegate, operationCompletedCallback);

            Exception error = null;
            object[] results = null;
            IAsyncResult result = null;
            try
            {
                result = beginOperationDelegate(inValues, onAsyncCallCompleted, context);
                if (result.CompletedSynchronously)
                {
                    results = endOperationDelegate(result);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                error = e;
            }

            if (error != null || result.CompletedSynchronously) /* result cannot be null if error == null */
            {
                CompleteAsyncCall(context, results, error);
            }
        }

        static void OnAsyncCallCompleted(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            AsyncOperationContext context = (AsyncOperationContext)result.AsyncState;
            Exception error = null;
            object[] results = null;
            try
            {
                results = context.EndDelegate(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                error = e;
            }

            CompleteAsyncCall(context, results, error);
        }

        static void CompleteAsyncCall(AsyncOperationContext context, object[] results, Exception error)
        {
            if (context.CompletionCallback != null)
            {
                InvokeAsyncCompletedEventArgs e = new InvokeAsyncCompletedEventArgs(results, error, false, context.AsyncOperation.UserSuppliedState);
                context.AsyncOperation.PostOperationCompleted(context.CompletionCallback, e);
            }
            else
            {
                context.AsyncOperation.OperationCompleted();
            }
        }

        class AsyncOperationContext
        {
            AsyncOperation asyncOperation;
            EndOperationDelegate endDelegate;
            SendOrPostCallback completionCallback;

            internal AsyncOperationContext(AsyncOperation asyncOperation, EndOperationDelegate endDelegate, SendOrPostCallback completionCallback)
            {
                this.asyncOperation = asyncOperation;
                this.endDelegate = endDelegate;
                this.completionCallback = completionCallback;
            }

            internal AsyncOperation AsyncOperation
            {
                get
                {
                    return this.asyncOperation;
                }
            }

            internal EndOperationDelegate EndDelegate
            {
                get
                {
                    return this.endDelegate;
                }
            }

            internal SendOrPostCallback CompletionCallback
            {
                get
                {
                    return this.completionCallback;
                }
            }
        }

        protected class ChannelBase<T> : IClientChannel, IOutputChannel, IRequestChannel, IChannelBaseProxy
            where T : class
        {
            ServiceChannel channel;
            System.ServiceModel.Dispatcher.ImmutableClientRuntime runtime;

            protected ChannelBase(ClientBase<T> client)
            {
                if (client.Endpoint.Address == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxChannelFactoryEndpointAddressUri)));
                }

                ChannelFactory<T> cf = client.ChannelFactory;
                cf.EnsureOpened();  // to prevent the NullReferenceException that is thrown if the ChannelFactory is not open when cf.ServiceChannelFactory is accessed.
                this.channel = cf.ServiceChannelFactory.CreateServiceChannel(client.Endpoint.Address, client.Endpoint.Address.Uri);
                this.channel.InstanceContext = cf.CallbackInstance;
                this.runtime = this.channel.ClientRuntime.GetRuntime();
            }

            [Fx.Tag.SecurityNote(Critical = "Accesses the critical IMethodCallMessage interface.",
                Safe = "The implementation of IMethodCallMessage is local and is created locally as weell; i.e. not passed in from Remoting.")]
            [SecuritySafeCritical]
            protected IAsyncResult BeginInvoke(string methodName, object[] args, AsyncCallback callback, object state)
            {
                object[] inArgs = new object[args.Length + 2];
                Array.Copy(args, inArgs, args.Length);
                inArgs[inArgs.Length - 2] = callback;
                inArgs[inArgs.Length - 1] = state;

                IMethodCallMessage methodCall = new MethodCallMessage(inArgs);
                ProxyOperationRuntime op = GetOperationByName(methodName);
                object[] ins = op.MapAsyncBeginInputs(methodCall, out callback, out state);
                return this.channel.BeginCall(op.Action, op.IsOneWay, op, ins, callback, state);
            }

            [Fx.Tag.SecurityNote(Critical = "Accesses the critical IMethodCallMessage interface.",
                Safe = "The implementation of IMethodCallMessage is local and is created locally as weell; i.e. not passed in from Remoting.")]
            [SecuritySafeCritical]
            protected object EndInvoke(string methodName, object[] args, IAsyncResult result)
            {
                object[] inArgs = new object[args.Length + 1];
                Array.Copy(args, inArgs, args.Length);
                inArgs[inArgs.Length - 1] = result;

                IMethodCallMessage methodCall = new MethodCallMessage(inArgs);
                ProxyOperationRuntime op = GetOperationByName(methodName);
                object[] outs;
                op.MapAsyncEndInputs(methodCall, out result, out outs);
                object ret = this.channel.EndCall(op.Action, outs, result);
                object[] retArgs = op.MapAsyncOutputs(methodCall, outs, ref ret);
                if (retArgs != null)
                {
                    Fx.Assert(retArgs.Length == inArgs.Length, "retArgs.Length should be equal to inArgs.Length");
                    Array.Copy(retArgs, args, args.Length);
                }
                return ret;
            }

            System.ServiceModel.Dispatcher.ProxyOperationRuntime GetOperationByName(string methodName)
            {
                ProxyOperationRuntime op = this.runtime.GetOperationByName(methodName);
                if (op == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SFxMethodNotSupported1, methodName)));
                }
                return op;
            }

            bool IClientChannel.AllowInitializationUI
            {
                get { return ((IClientChannel)this.channel).AllowInitializationUI; }
                set { ((IClientChannel)this.channel).AllowInitializationUI = value; }
            }

            bool IClientChannel.DidInteractiveInitialization
            {
                get { return ((IClientChannel)this.channel).DidInteractiveInitialization; }
            }

            Uri IClientChannel.Via
            {
                get { return ((IClientChannel)this.channel).Via; }
            }

            event EventHandler<UnknownMessageReceivedEventArgs> IClientChannel.UnknownMessageReceived
            {
                add { ((IClientChannel)this.channel).UnknownMessageReceived += value; }
                remove { ((IClientChannel)this.channel).UnknownMessageReceived -= value; }
            }

            void IClientChannel.DisplayInitializationUI()
            {
                ((IClientChannel)this.channel).DisplayInitializationUI();
            }

            IAsyncResult IClientChannel.BeginDisplayInitializationUI(AsyncCallback callback, object state)
            {
                return ((IClientChannel)this.channel).BeginDisplayInitializationUI(callback, state);
            }

            void IClientChannel.EndDisplayInitializationUI(IAsyncResult result)
            {
                ((IClientChannel)this.channel).EndDisplayInitializationUI(result);
            }

            bool IContextChannel.AllowOutputBatching
            {
                get { return ((IContextChannel)this.channel).AllowOutputBatching; }
                set { ((IContextChannel)this.channel).AllowOutputBatching = value; }
            }

            IInputSession IContextChannel.InputSession
            {
                get { return ((IContextChannel)this.channel).InputSession; }
            }

            EndpointAddress IContextChannel.LocalAddress
            {
                get { return ((IContextChannel)this.channel).LocalAddress; }
            }

            TimeSpan IContextChannel.OperationTimeout
            {
                get { return ((IContextChannel)this.channel).OperationTimeout; }
                set { ((IContextChannel)this.channel).OperationTimeout = value; }
            }

            IOutputSession IContextChannel.OutputSession
            {
                get { return ((IContextChannel)this.channel).OutputSession; }
            }

            EndpointAddress IContextChannel.RemoteAddress
            {
                get { return ((IContextChannel)this.channel).RemoteAddress; }
            }

            string IContextChannel.SessionId
            {
                get { return ((IContextChannel)this.channel).SessionId; }
            }

            TProperty IChannel.GetProperty<TProperty>()
            {
                return ((IChannel)this.channel).GetProperty<TProperty>();
            }

            CommunicationState ICommunicationObject.State
            {
                get { return ((ICommunicationObject)this.channel).State; }
            }

            event EventHandler ICommunicationObject.Closed
            {
                add { ((ICommunicationObject)this.channel).Closed += value; }
                remove { ((ICommunicationObject)this.channel).Closed -= value; }
            }

            event EventHandler ICommunicationObject.Closing
            {
                add { ((ICommunicationObject)this.channel).Closing += value; }
                remove { ((ICommunicationObject)this.channel).Closing -= value; }
            }

            event EventHandler ICommunicationObject.Faulted
            {
                add { ((ICommunicationObject)this.channel).Faulted += value; }
                remove { ((ICommunicationObject)this.channel).Faulted -= value; }
            }

            event EventHandler ICommunicationObject.Opened
            {
                add { ((ICommunicationObject)this.channel).Opened += value; }
                remove { ((ICommunicationObject)this.channel).Opened -= value; }
            }

            event EventHandler ICommunicationObject.Opening
            {
                add { ((ICommunicationObject)this.channel).Opening += value; }
                remove { ((ICommunicationObject)this.channel).Opening -= value; }
            }

            void ICommunicationObject.Abort()
            {
                ((ICommunicationObject)this.channel).Abort();
            }

            void ICommunicationObject.Close()
            {
                ((ICommunicationObject)this.channel).Close();
            }

            void ICommunicationObject.Close(TimeSpan timeout)
            {
                ((ICommunicationObject)this.channel).Close(timeout);
            }

            IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
            {
                return ((ICommunicationObject)this.channel).BeginClose(callback, state);
            }

            IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ((ICommunicationObject)this.channel).BeginClose(timeout, callback, state);
            }

            void ICommunicationObject.EndClose(IAsyncResult result)
            {
                ((ICommunicationObject)this.channel).EndClose(result);
            }

            void ICommunicationObject.Open()
            {
                ((ICommunicationObject)this.channel).Open();
            }

            void ICommunicationObject.Open(TimeSpan timeout)
            {
                ((ICommunicationObject)this.channel).Open(timeout);
            }

            IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
            {
                return ((ICommunicationObject)this.channel).BeginOpen(callback, state);
            }

            IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ((ICommunicationObject)this.channel).BeginOpen(timeout, callback, state);
            }

            void ICommunicationObject.EndOpen(IAsyncResult result)
            {
                ((ICommunicationObject)this.channel).EndOpen(result);
            }

            IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions
            {
                get { return ((IExtensibleObject<IContextChannel>)this.channel).Extensions; }
            }

            void IDisposable.Dispose()
            {
                ((IDisposable)this.channel).Dispose();
            }

            Uri IOutputChannel.Via
            {
                get { return ((IOutputChannel)this.channel).Via; }
            }

            EndpointAddress IOutputChannel.RemoteAddress
            {
                get { return ((IOutputChannel)this.channel).RemoteAddress; }
            }

            void IOutputChannel.Send(Message message)
            {
                ((IOutputChannel)this.channel).Send(message);
            }

            void IOutputChannel.Send(Message message, TimeSpan timeout)
            {
                ((IOutputChannel)this.channel).Send(message, timeout);
            }

            IAsyncResult IOutputChannel.BeginSend(Message message, AsyncCallback callback, object state)
            {
                return ((IOutputChannel)this.channel).BeginSend(message, callback, state);
            }

            IAsyncResult IOutputChannel.BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ((IOutputChannel)this.channel).BeginSend(message, timeout, callback, state);
            }

            void IOutputChannel.EndSend(IAsyncResult result)
            {
                ((IOutputChannel)this.channel).EndSend(result);
            }

            Uri IRequestChannel.Via
            {
                get { return ((IRequestChannel)this.channel).Via; }
            }

            EndpointAddress IRequestChannel.RemoteAddress
            {
                get { return ((IRequestChannel)this.channel).RemoteAddress; }
            }

            Message IRequestChannel.Request(Message message)
            {
                return ((IRequestChannel)this.channel).Request(message);
            }

            Message IRequestChannel.Request(Message message, TimeSpan timeout)
            {
                return ((IRequestChannel)this.channel).Request(message, timeout);
            }

            IAsyncResult IRequestChannel.BeginRequest(Message message, AsyncCallback callback, object state)
            {
                return ((IRequestChannel)this.channel).BeginRequest(message, callback, state);
            }

            IAsyncResult IRequestChannel.BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ((IRequestChannel)this.channel).BeginRequest(message, timeout, callback, state);
            }

            Message IRequestChannel.EndRequest(IAsyncResult result)
            {
                return ((IRequestChannel)this.channel).EndRequest(result);
            }

            ServiceChannel IChannelBaseProxy.GetServiceChannel()
            {
                return this.channel;
            }

            class MethodCallMessage : IMethodCallMessage
            {
                readonly object[] args;

                public MethodCallMessage(object[] args)
                {
                    this.args = args;
                }

                public object[] Args
                {
                    get { return this.args; }
                }

                public int ArgCount
                {
                    get { return this.args.Length; }
                }

                public LogicalCallContext LogicalCallContext
                {
                    get { return null; }
                }

                public object GetInArg(int argNum)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public string GetInArgName(int index)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public int InArgCount
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public object[] InArgs
                {
                    get { return this.args; }
                }


                public object GetArg(int argNum)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public string GetArgName(int index)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public bool HasVarArgs
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public Reflection.MethodBase MethodBase
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public string MethodName
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public object MethodSignature
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public string TypeName
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public string Uri
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public Collections.IDictionary Properties
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }
            }
        }
    }
}
