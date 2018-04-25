//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Remoting;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    abstract class ServiceChannelFactory : ChannelFactoryBase
    {
        string bindingName;
        List<IChannel> channelsList;
        ClientRuntime clientRuntime;
        RequestReplyCorrelator requestReplyCorrelator = new RequestReplyCorrelator();
        IDefaultCommunicationTimeouts timeouts;
        MessageVersion messageVersion;

        public ServiceChannelFactory(ClientRuntime clientRuntime, Binding binding)
            : base()
        {
            if (clientRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientRuntime");
            }

            this.bindingName = binding.Name;
            this.channelsList = new List<IChannel>();
            this.clientRuntime = clientRuntime;
            this.timeouts = new DefaultCommunicationTimeouts(binding);
            this.messageVersion = binding.MessageVersion;
        }

        public ClientRuntime ClientRuntime
        {
            get
            {
                this.ThrowIfDisposed();
                return this.clientRuntime;
            }
        }

        internal RequestReplyCorrelator RequestReplyCorrelator
        {
            get
            {
                ThrowIfDisposed();
                return this.requestReplyCorrelator;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.timeouts.CloseTimeout; }
        }

        protected override TimeSpan DefaultReceiveTimeout
        {
            get { return this.timeouts.ReceiveTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.timeouts.OpenTimeout; }
        }

        protected override TimeSpan DefaultSendTimeout
        {
            get { return this.timeouts.SendTimeout; }
        }

        public MessageVersion MessageVersion
        {
            get { return this.messageVersion; }
        }

        // special overload for security only
        public static ServiceChannelFactory BuildChannelFactory(ChannelBuilder channelBuilder, ClientRuntime clientRuntime)
        {
            if (channelBuilder.CanBuildChannelFactory<IDuplexChannel>())
            {
                return new ServiceChannelFactoryOverDuplex(channelBuilder.BuildChannelFactory<IDuplexChannel>(), clientRuntime,
                    channelBuilder.Binding);
            }
            else if (channelBuilder.CanBuildChannelFactory<IDuplexSessionChannel>())
            {
                return new ServiceChannelFactoryOverDuplexSession(channelBuilder.BuildChannelFactory<IDuplexSessionChannel>(), clientRuntime, channelBuilder.Binding, false);
            }
            else
            {
                return new ServiceChannelFactoryOverRequestSession(channelBuilder.BuildChannelFactory<IRequestSessionChannel>(), clientRuntime, channelBuilder.Binding, false);
            }
        }

        public static ServiceChannelFactory BuildChannelFactory(ServiceEndpoint serviceEndpoint)
        {
            return BuildChannelFactory(serviceEndpoint, false);
        }

        public static ServiceChannelFactory BuildChannelFactory(ServiceEndpoint serviceEndpoint, bool useActiveAutoClose)
        {
            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }

            serviceEndpoint.EnsureInvariants();
            serviceEndpoint.ValidateForClient();

            ChannelRequirements requirements;
            ContractDescription contractDescription = serviceEndpoint.Contract;
            ChannelRequirements.ComputeContractRequirements(contractDescription, out requirements);

            BindingParameterCollection parameters;
            ClientRuntime clientRuntime = DispatcherBuilder.BuildProxyBehavior(serviceEndpoint, out parameters);

            Binding binding = serviceEndpoint.Binding;
            Type[] requiredChannels = ChannelRequirements.ComputeRequiredChannels(ref requirements);

            CustomBinding customBinding = new CustomBinding(binding);
            BindingContext context = new BindingContext(customBinding, parameters);

            InternalDuplexBindingElement internalDuplexBindingElement = null;
            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref internalDuplexBindingElement);

            customBinding = new CustomBinding(context.RemainingBindingElements);
            customBinding.CopyTimeouts(serviceEndpoint.Binding);

            foreach (Type type in requiredChannels)
            {
                if (type == typeof(IOutputChannel) && customBinding.CanBuildChannelFactory<IOutputChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverOutput(customBinding.BuildChannelFactory<IOutputChannel>(parameters), clientRuntime, binding);
                }

                if (type == typeof(IRequestChannel) && customBinding.CanBuildChannelFactory<IRequestChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverRequest(customBinding.BuildChannelFactory<IRequestChannel>(parameters), clientRuntime, binding);
                }

                if (type == typeof(IDuplexChannel) && customBinding.CanBuildChannelFactory<IDuplexChannel>(parameters))
                {
                    if (requirements.usesReply &&
                        binding.CreateBindingElements().Find<TransportBindingElement>().ManualAddressing)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.CantCreateChannelWithManualAddressing)));
                    }

                    return new ServiceChannelFactoryOverDuplex(customBinding.BuildChannelFactory<IDuplexChannel>(parameters), clientRuntime, binding);
                }

                if (type == typeof(IOutputSessionChannel) && customBinding.CanBuildChannelFactory<IOutputSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverOutputSession(customBinding.BuildChannelFactory<IOutputSessionChannel>(parameters), clientRuntime, binding, false);
                }

                if (type == typeof(IRequestSessionChannel) && customBinding.CanBuildChannelFactory<IRequestSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverRequestSession(customBinding.BuildChannelFactory<IRequestSessionChannel>(parameters), clientRuntime, binding, false);
                }

                if (type == typeof(IDuplexSessionChannel) && customBinding.CanBuildChannelFactory<IDuplexSessionChannel>(parameters))
                {
                    if (requirements.usesReply &&
                        binding.CreateBindingElements().Find<TransportBindingElement>().ManualAddressing)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.CantCreateChannelWithManualAddressing)));
                    }

                    return new ServiceChannelFactoryOverDuplexSession(customBinding.BuildChannelFactory<IDuplexSessionChannel>(parameters), clientRuntime, binding, useActiveAutoClose);
                }
            }

            foreach (Type type in requiredChannels)
            {
                // For SessionMode.Allowed or SessionMode.NotAllowed we will accept session-ful variants as well
                if (type == typeof(IOutputChannel) && customBinding.CanBuildChannelFactory<IOutputSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverOutputSession(customBinding.BuildChannelFactory<IOutputSessionChannel>(parameters), clientRuntime, binding, true);
                }

                if (type == typeof(IRequestChannel) && customBinding.CanBuildChannelFactory<IRequestSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverRequestSession(customBinding.BuildChannelFactory<IRequestSessionChannel>(parameters), clientRuntime, binding, true);
                }

                // and for SessionMode.Required, it is possible that the InstanceContextProvider is handling the session management, so 
                // accept datagram variants if that is the case
                if (type == typeof(IRequestSessionChannel) && customBinding.CanBuildChannelFactory<IRequestChannel>(parameters)
                    && customBinding.GetProperty<IContextSessionProvider>(parameters) != null)
                {
                    return new ServiceChannelFactoryOverRequest(customBinding.BuildChannelFactory<IRequestChannel>(parameters), clientRuntime, binding);
                }
            }

            // we put a lot of work into creating a good error message, as this is a common case
            Dictionary<Type, byte> supportedChannels = new Dictionary<Type, byte>();
            if (customBinding.CanBuildChannelFactory<IOutputChannel>(parameters))
            {
                supportedChannels.Add(typeof(IOutputChannel), 0);
            }
            if (customBinding.CanBuildChannelFactory<IRequestChannel>(parameters))
            {
                supportedChannels.Add(typeof(IRequestChannel), 0);
            }
            if (customBinding.CanBuildChannelFactory<IDuplexChannel>(parameters))
            {
                supportedChannels.Add(typeof(IDuplexChannel), 0);
            }
            if (customBinding.CanBuildChannelFactory<IOutputSessionChannel>(parameters))
            {
                supportedChannels.Add(typeof(IOutputSessionChannel), 0);
            }
            if (customBinding.CanBuildChannelFactory<IRequestSessionChannel>(parameters))
            {
                supportedChannels.Add(typeof(IRequestSessionChannel), 0);
            }
            if (customBinding.CanBuildChannelFactory<IDuplexSessionChannel>(parameters))
            {
                supportedChannels.Add(typeof(IDuplexSessionChannel), 0);
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ChannelRequirements.CantCreateChannelException(
                supportedChannels.Keys, requiredChannels, binding.Name));
        }

        protected override void OnAbort()
        {
            IChannel channel = null;

            lock (ThisLock)
            {
                channel = (channelsList.Count > 0) ? channelsList[channelsList.Count - 1] : null;
            }

            while (channel != null)
            {
                channel.Abort();

                lock (ThisLock)
                {
                    channelsList.Remove(channel);
                    channel = (channelsList.Count > 0) ? channelsList[channelsList.Count - 1] : null;
                }
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            while (true)
            {
                int count;
                IChannel channel;
                lock (ThisLock)
                {
                    count = channelsList.Count;
                    if (count == 0)
                        return;
                    channel = channelsList[0];
                }
                channel.Close(timeoutHelper.RemainingTime());
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<ICommunicationObject> objectList;
            lock (ThisLock)
            {
                objectList = new List<ICommunicationObject>();
                for (int index = 0; index < channelsList.Count; index++)
                    objectList.Add(channelsList[index]);
            }
            return new CloseCollectionAsyncResult(timeout, callback, state, objectList);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseCollectionAsyncResult.End(result);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.clientRuntime.LockDownProperties();
        }

        public void ChannelCreated(IChannel channel)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.ChannelCreated,
                    SR.GetString(SR.TraceCodeChannelCreated, TraceUtility.CreateSourceString(channel)), this);
            }
            lock (ThisLock)
            {
                ThrowIfDisposed();
                channelsList.Add(channel);
            }
        }

        public void ChannelDisposed(IChannel channel)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.ChannelDisposed,
                    SR.GetString(SR.TraceCodeChannelDisposed, TraceUtility.CreateSourceString(channel)),
                    this);
            }
            lock (ThisLock)
            {
                channelsList.Remove(channel);
            }
        }

        public virtual ServiceChannel CreateServiceChannel(EndpointAddress address, Uri via)
        {
            IChannelBinder binder = this.CreateInnerChannelBinder(address, via);
            ServiceChannel serviceChannel = new ServiceChannel(this, binder);

            if (binder is DuplexChannelBinder)
            {
                DuplexChannelBinder duplexChannelBinder = binder as DuplexChannelBinder;
                duplexChannelBinder.ChannelHandler = new ChannelHandler(this.messageVersion, binder, serviceChannel);
                duplexChannelBinder.DefaultCloseTimeout = this.DefaultCloseTimeout;
                duplexChannelBinder.DefaultSendTimeout = this.DefaultSendTimeout;
                duplexChannelBinder.IdentityVerifier = this.clientRuntime.IdentityVerifier;
            }

            return serviceChannel;
        }

        public TChannel CreateChannel<TChannel>(EndpointAddress address)
        {
            return this.CreateChannel<TChannel>(address, null);
        }

        public TChannel CreateChannel<TChannel>(EndpointAddress address, Uri via)
        {
            if (!this.CanCreateChannel<TChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.CouldnTCreateChannelForChannelType2, this.bindingName, typeof(TChannel).Name)));
            }

            return (TChannel)this.CreateChannel(typeof(TChannel), address, via);
        }

        public abstract bool CanCreateChannel<TChannel>();

        public object CreateChannel(Type channelType, EndpointAddress address)
        {
            return this.CreateChannel(channelType, address, null);
        }

        public object CreateChannel(Type channelType, EndpointAddress address, Uri via)
        {
            if (via == null)
            {
                via = this.ClientRuntime.Via;

                if (via == null)
                {
                    via = address.Uri;
                }
            }

            ServiceChannel serviceChannel = this.CreateServiceChannel(address, via);

            serviceChannel.Proxy = CreateProxy(channelType, channelType, MessageDirection.Input, serviceChannel);

            serviceChannel.ClientRuntime.GetRuntime().InitializeChannel((IClientChannel)serviceChannel.Proxy);
            OperationContext current = OperationContext.Current;
            if ((current != null) && (current.InstanceContext != null))
            {
                current.InstanceContext.WmiChannels.Add((IChannel)serviceChannel.Proxy);
                serviceChannel.WmiInstanceContext = current.InstanceContext;
            }

            return serviceChannel.Proxy;
        }

        [Fx.Tag.SecurityNote(Critical = "Constructs a ServiceChannelProxy, which is Critical.",
            Safe = "Returns the TP, but does not return the RealProxy -- caller can't get from TP to RP without an elevation.")]
        [SecuritySafeCritical]
        internal static object CreateProxy(Type interfaceType, Type proxiedType, MessageDirection direction, ServiceChannel serviceChannel)
        {
            if (!proxiedType.IsInterface)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("SFxChannelFactoryTypeMustBeInterface")));
            }
            ServiceChannelProxy proxy = new ServiceChannelProxy(interfaceType, proxiedType, direction, serviceChannel);
            return proxy.GetTransparentProxy();
        }

        [Fx.Tag.SecurityNote(Critical = "Calls LinkDemand method RemotingServices.GetRealProxy and access critical class ServiceChannelProxy.",
            Safe = "Gets the ServiceChannel (which is not critical) and discards the RealProxy.")]
        [SecuritySafeCritical]
        internal static ServiceChannel GetServiceChannel(object transparentProxy)
        {
            IChannelBaseProxy cb = transparentProxy as IChannelBaseProxy;
            if (cb != null)
                return cb.GetServiceChannel();

            ServiceChannelProxy proxy = RemotingServices.GetRealProxy(transparentProxy) as ServiceChannelProxy;

            if (proxy != null)
                return proxy.GetServiceChannel();
            else
                return null;
        }

        protected abstract IChannelBinder CreateInnerChannelBinder(EndpointAddress address, Uri via);

        abstract class TypedServiceChannelFactory<TChannel> : ServiceChannelFactory
            where TChannel : class, IChannel
        {
            IChannelFactory<TChannel> innerChannelFactory;

            protected TypedServiceChannelFactory(IChannelFactory<TChannel> innerChannelFactory,
                ClientRuntime clientRuntime, Binding binding)
                : base(clientRuntime, binding)
            {
                this.innerChannelFactory = innerChannelFactory;
            }

            protected IChannelFactory<TChannel> InnerChannelFactory
            {
                get { return this.innerChannelFactory; }
            }

            protected override void OnAbort()
            {
                base.OnAbort();
                this.innerChannelFactory.Abort();
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerChannelFactory.Open(timeout);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannelFactory.BeginOpen(timeout, callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerChannelFactory.EndOpen(result);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnClose(timeoutHelper.RemainingTime());
                this.innerChannelFactory.Close(timeoutHelper.RemainingTime());
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose,
                    this.innerChannelFactory.BeginClose, this.innerChannelFactory.EndClose);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            public override T GetProperty<T>()
            {
                if (typeof(T) == typeof(TypedServiceChannelFactory<TChannel>))
                {
                    return (T)(object)this;
                }

                T baseProperty = base.GetProperty<T>();
                if (baseProperty != null)
                {
                    return baseProperty;
                }

                return this.innerChannelFactory.GetProperty<T>();
            }
        }

        class ServiceChannelFactoryOverOutput : TypedServiceChannelFactory<IOutputChannel>
        {
            public ServiceChannelFactoryOverOutput(IChannelFactory<IOutputChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding)
                : base(innerChannelFactory, clientRuntime, binding)
            {
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new OutputChannelBinder(this.InnerChannelFactory.CreateChannel(to, via));
            }

            public override bool CanCreateChannel<TChannel>()
            {
                return (typeof(TChannel) == typeof(IOutputChannel)
                    || typeof(TChannel) == typeof(IRequestChannel));
            }
        }

        class ServiceChannelFactoryOverDuplex : TypedServiceChannelFactory<IDuplexChannel>
        {
            public ServiceChannelFactoryOverDuplex(IChannelFactory<IDuplexChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding)
                : base(innerChannelFactory, clientRuntime, binding)
            {
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new DuplexChannelBinder(this.InnerChannelFactory.CreateChannel(to, via), this.RequestReplyCorrelator);
            }

            public override bool CanCreateChannel<TChannel>()
            {
                return (typeof(TChannel) == typeof(IOutputChannel)
                    || typeof(TChannel) == typeof(IRequestChannel)
                    || typeof(TChannel) == typeof(IDuplexChannel));
            }
        }

        class ServiceChannelFactoryOverRequest : TypedServiceChannelFactory<IRequestChannel>
        {
            public ServiceChannelFactoryOverRequest(IChannelFactory<IRequestChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding)
                : base(innerChannelFactory, clientRuntime, binding)
            {
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new RequestChannelBinder(this.InnerChannelFactory.CreateChannel(to, via));
            }

            public override bool CanCreateChannel<TChannel>()
            {
                return (typeof(TChannel) == typeof(IOutputChannel)
                    || typeof(TChannel) == typeof(IRequestChannel));
            }
        }

        class ServiceChannelFactoryOverOutputSession : TypedServiceChannelFactory<IOutputSessionChannel>
        {
            bool datagramAdapter;
            public ServiceChannelFactoryOverOutputSession(IChannelFactory<IOutputSessionChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding, bool datagramAdapter)
                : base(innerChannelFactory, clientRuntime, binding)
            {
                this.datagramAdapter = datagramAdapter;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                IOutputChannel channel;

                if (this.datagramAdapter)
                {
                    channel = DatagramAdapter.GetOutputChannel(
                        delegate() { return this.InnerChannelFactory.CreateChannel(to, via); },
                        timeouts);
                }
                else
                {
                    channel = this.InnerChannelFactory.CreateChannel(to, via);
                }

                return new OutputChannelBinder(channel);
            }

            public override bool CanCreateChannel<TChannel>()
            {
                return (typeof(TChannel) == typeof(IOutputChannel)
                    || typeof(TChannel) == typeof(IOutputSessionChannel)
                    || typeof(TChannel) == typeof(IRequestChannel)
                    || typeof(TChannel) == typeof(IRequestSessionChannel));
            }
        }

        class ServiceChannelFactoryOverDuplexSession : TypedServiceChannelFactory<IDuplexSessionChannel>
        {
            bool useActiveAutoClose;

            public ServiceChannelFactoryOverDuplexSession(IChannelFactory<IDuplexSessionChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding, bool useActiveAutoClose)
                : base(innerChannelFactory, clientRuntime, binding)
            {
                this.useActiveAutoClose = useActiveAutoClose;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new DuplexChannelBinder(this.InnerChannelFactory.CreateChannel(to, via), this.RequestReplyCorrelator, useActiveAutoClose);
            }

            public override bool CanCreateChannel<TChannel>()
            {
                return (typeof(TChannel) == typeof(IOutputChannel)
                    || typeof(TChannel) == typeof(IRequestChannel)
                    || typeof(TChannel) == typeof(IDuplexChannel)
                    || typeof(TChannel) == typeof(IOutputSessionChannel)
                    || typeof(TChannel) == typeof(IRequestSessionChannel)
                    || typeof(TChannel) == typeof(IDuplexSessionChannel));
            }
        }

        class ServiceChannelFactoryOverRequestSession : TypedServiceChannelFactory<IRequestSessionChannel>
        {
            bool datagramAdapter = false;

            public ServiceChannelFactoryOverRequestSession(IChannelFactory<IRequestSessionChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding, bool datagramAdapter)
                : base(innerChannelFactory, clientRuntime, binding)
            {
                this.datagramAdapter = datagramAdapter;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                IRequestChannel channel;

                if (this.datagramAdapter)
                {
                    channel = DatagramAdapter.GetRequestChannel(
                        delegate() { return this.InnerChannelFactory.CreateChannel(to, via); },
                        this.timeouts);
                }
                else
                {
                    channel = this.InnerChannelFactory.CreateChannel(to, via);
                }

                return new RequestChannelBinder(channel);
            }

            public override bool CanCreateChannel<TChannel>()
            {
                return (typeof(TChannel) == typeof(IOutputChannel)
                    || typeof(TChannel) == typeof(IOutputSessionChannel)
                    || typeof(TChannel) == typeof(IRequestChannel)
                    || typeof(TChannel) == typeof(IRequestSessionChannel));
            }
        }

        class DefaultCommunicationTimeouts : IDefaultCommunicationTimeouts
        {
            TimeSpan closeTimeout;
            TimeSpan openTimeout;
            TimeSpan receiveTimeout;
            TimeSpan sendTimeout;

            public DefaultCommunicationTimeouts(IDefaultCommunicationTimeouts timeouts)
            {
                this.closeTimeout = timeouts.CloseTimeout;
                this.openTimeout = timeouts.OpenTimeout;
                this.receiveTimeout = timeouts.ReceiveTimeout;
                this.sendTimeout = timeouts.SendTimeout;
            }

            public TimeSpan CloseTimeout
            {
                get { return this.closeTimeout; }
            }

            public TimeSpan OpenTimeout
            {
                get { return this.openTimeout; }
            }

            public TimeSpan ReceiveTimeout
            {
                get { return this.receiveTimeout; }
            }

            public TimeSpan SendTimeout
            {
                get { return this.sendTimeout; }
            }
        }
    }
}
