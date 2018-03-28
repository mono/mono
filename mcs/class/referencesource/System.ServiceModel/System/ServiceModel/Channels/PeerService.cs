//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    // What the connector interface needs to looks like
    interface IPeerConnectorContract
    {
        void Connect(IPeerNeighbor neighbor, ConnectInfo connectInfo);
        void Disconnect(IPeerNeighbor neighbor, DisconnectInfo disconnectInfo);
        void Refuse(IPeerNeighbor neighbor, RefuseInfo refuseInfo);
        void Welcome(IPeerNeighbor neighbor, WelcomeInfo welcomeInfo);
    }

    // Implemented by flooder / service uses this to delegate service invocations
    interface IPeerFlooderContract<TFloodContract, TLinkContract>
    {
        //invoked by the peerservice
        IAsyncResult OnFloodedMessage(IPeerNeighbor neighbor, TFloodContract floodedInfo, AsyncCallback callback, object state);
        void EndFloodMessage(IAsyncResult result);
        void ProcessLinkUtility(IPeerNeighbor neighbor, TLinkContract utilityInfo);
    }

    // Class that implements IPeerService contract for incoming neighbor sessions and messages.
    // WARNING: This class is not synchronized. Expects the using class to synchronize access
    [ServiceBehavior(
     ConcurrencyMode = ConcurrencyMode.Multiple,
     InstanceContextMode = InstanceContextMode.Single,
     UseSynchronizationContext = false)]
    class PeerService : IPeerService, IServiceBehavior, IChannelInitializer
    {
        public delegate bool ChannelCallback(IClientChannel channel);
        public delegate IPeerNeighbor GetNeighborCallback(IPeerProxy channel);

        Binding binding;
        PeerNodeConfig config;
        ChannelCallback newChannelCallback;
        GetNeighborCallback getNeighborCallback;
        ServiceHost serviceHost;                    // To listen for incoming neighbor sessions
        IPeerConnectorContract connector;
        IPeerFlooderContract<Message, UtilityInfo> flooder;
        IPeerNodeMessageHandling messageHandler;

        public PeerService(PeerNodeConfig config,
                            ChannelCallback channelCallback,
                            GetNeighborCallback getNeighborCallback,
                            Dictionary<Type, object> services)
            : this(config, channelCallback, getNeighborCallback, services, null) { }
        public PeerService(PeerNodeConfig config,
                            ChannelCallback channelCallback,
                            GetNeighborCallback getNeighborCallback,
                            Dictionary<Type, object> services,
                            IPeerNodeMessageHandling messageHandler)
        {
            this.config = config;
            this.newChannelCallback = channelCallback;
            Fx.Assert(getNeighborCallback != null, "getNeighborCallback must be passed to PeerService constructor");
            this.getNeighborCallback = getNeighborCallback;
            this.messageHandler = messageHandler;

            if (services != null)
            {
                object reply = null;
                services.TryGetValue(typeof(IPeerConnectorContract), out reply);
                connector = reply as IPeerConnectorContract;
                Fx.Assert(connector != null, "PeerService must be created with a connector implementation");
                reply = null;
                services.TryGetValue(typeof(IPeerFlooderContract<Message, UtilityInfo>), out reply);
                flooder = reply as IPeerFlooderContract<Message, UtilityInfo>;
                Fx.Assert(flooder != null, "PeerService must be created with a flooder implementation");
            }
            this.serviceHost = new ServiceHost(this);

            // Add throttling            
            ServiceThrottlingBehavior throttle = new ServiceThrottlingBehavior();
            throttle.MaxConcurrentCalls = this.config.MaxPendingIncomingCalls;
            throttle.MaxConcurrentSessions = this.config.MaxConcurrentSessions;
            this.serviceHost.Description.Behaviors.Add(throttle);
        }

        public void Abort()
        {
            this.serviceHost.Abort();
        }

        public Binding Binding
        {
            get { return this.binding; }
        }

        // Create the binding using user specified config. The stacking is 
        // BinaryMessageEncoder/TCP
        void CreateBinding()
        {
            Collection<BindingElement> bindingElements = new Collection<BindingElement>();
            BindingElement security = this.config.SecurityManager.GetSecurityBindingElement();
            if (security != null)
            {
                bindingElements.Add(security);
            }

            TcpTransportBindingElement transport = new TcpTransportBindingElement();
            transport.MaxReceivedMessageSize = this.config.MaxReceivedMessageSize;
            transport.MaxBufferPoolSize = this.config.MaxBufferPoolSize;
            transport.TeredoEnabled = true;

            MessageEncodingBindingElement encoder = null;
            if (messageHandler != null)
                encoder = messageHandler.EncodingBindingElement;

            if (encoder == null)
            {
                BinaryMessageEncodingBindingElement bencoder = new BinaryMessageEncodingBindingElement();
                this.config.ReaderQuotas.CopyTo(bencoder.ReaderQuotas);
                bindingElements.Add(bencoder);
            }
            else
            {
                bindingElements.Add(encoder);
            }

            bindingElements.Add(transport);

            this.binding = new CustomBinding(bindingElements);
            this.binding.ReceiveTimeout = TimeSpan.MaxValue;
        }

        // Returns the address that the serviceHost is listening on.
        public EndpointAddress GetListenAddress()
        {
            IChannelListener listener = this.serviceHost.ChannelDispatchers[0].Listener;
            return new EndpointAddress(listener.Uri, listener.GetProperty<EndpointIdentity>());
        }

        IPeerNeighbor GetNeighbor()
        {
            IPeerNeighbor neighbor = (IPeerNeighbor)getNeighborCallback(OperationContext.Current.GetCallbackChannel<IPeerProxy>());

            if (neighbor == null || neighbor.State == PeerNeighborState.Closed)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.PeerNeighborNotFound,
                        SR.GetString(SR.TraceCodePeerNeighborNotFound),
                        new PeerNodeTraceRecord(config.NodeId),
                        OperationContext.Current.IncomingMessage);
                }
                return null;
            }

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                PeerNeighborState state = neighbor.State;

                PeerNodeAddress listenAddr = null;
                IPAddress connectIPAddr = null;

                if (state >= PeerNeighborState.Opened && state <= PeerNeighborState.Connected)
                {
                    listenAddr = config.GetListenAddress(true);
                    connectIPAddr = config.ListenIPAddress;
                }

                PeerNeighborTraceRecord record = new PeerNeighborTraceRecord(neighbor.NodeId,
                    this.config.NodeId, listenAddr, connectIPAddr, neighbor.GetHashCode(),
                    neighbor.IsInitiator, state.ToString(), null, null,
                    OperationContext.Current.IncomingMessage.Headers.Action);

                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.PeerNeighborMessageReceived, SR.GetString(SR.TraceCodePeerNeighborMessageReceived), record, this, null);
            }

            return neighbor;
        }

        public void Open(TimeSpan timeout)
        {
            // Create the neighbor binding
            CreateBinding();
            this.serviceHost.Description.Endpoints.Clear();
            ServiceEndpoint endPoint = this.serviceHost.AddServiceEndpoint(typeof(IPeerService), this.binding, config.GetMeshUri());
            endPoint.ListenUri = config.GetSelfUri();
            endPoint.ListenUriMode = (this.config.Port > 0) ? ListenUriMode.Explicit : ListenUriMode.Unique;

            /*
                Uncomment this to allow the retrieval of metadata 
                using the command:
                    \binaries.x86chk\svcutil http://localhost /t:metadata

                        ServiceMetadataBehavior mex = new ServiceMetadataBehavior();
                        mex.HttpGetEnabled = true;
                        mex.HttpGetUrl = new Uri("http://localhost");
                        mex.HttpsGetEnabled = true;
                        mex.HttpsGetUrl = new Uri("https://localhost");
                        this.serviceHost.Description.Behaviors.Add(mex);
            */
            this.config.SecurityManager.ApplyServiceSecurity(this.serviceHost.Description);
            this.serviceHost.Open(timeout);

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerServiceOpened,
                    SR.GetString(SR.TraceCodePeerServiceOpened, this.GetListenAddress()), this);
            }
        }

        //
        // IContractBehavior and IChannelInitializer implementation. 
        // Used to register for incoming channel notification.
        //
        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHost)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHost, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHost)
        {
            for (int i = 0; i < serviceHost.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHost.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {

                    bool addedChannelInitializer = false;
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (!endpointDispatcher.IsSystemEndpoint)
                        {
                            if (!addedChannelInitializer)
                            {
                                channelDispatcher.ChannelInitializers.Add(this);
                                addedChannelInitializer = true;
                            }
                            endpointDispatcher.DispatchRuntime.OperationSelector = new OperationSelector(this.messageHandler);

                        }
                    }
                }
            }
        }

        void IChannelInitializer.Initialize(IClientChannel channel)
        {
            newChannelCallback(channel);
        }

        void IPeerServiceContract.Connect(ConnectInfo connectInfo)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor != null)
            {
                connector.Connect(neighbor, connectInfo);
            }
        }

        void IPeerServiceContract.Disconnect(DisconnectInfo disconnectInfo)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor != null)
            {
                connector.Disconnect(neighbor, disconnectInfo);
            }
        }

        void IPeerServiceContract.Refuse(RefuseInfo refuseInfo)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor != null)
            {
                connector.Refuse(neighbor, refuseInfo);
            }
        }

        void IPeerServiceContract.Welcome(WelcomeInfo welcomeInfo)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor != null)
            {
                connector.Welcome(neighbor, welcomeInfo);
            }
        }

        IAsyncResult IPeerServiceContract.BeginFloodMessage(Message floodedInfo, AsyncCallback callback, object state)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor != null)
            {
                return flooder.OnFloodedMessage(neighbor, floodedInfo, callback, state);
            }
            else
                return new CompletedAsyncResult(callback, state);
        }

        void IPeerServiceContract.EndFloodMessage(IAsyncResult result)
        {
            flooder.EndFloodMessage(result);
        }

        void IPeerServiceContract.LinkUtility(UtilityInfo utilityInfo)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor != null)
            {
                flooder.ProcessLinkUtility(neighbor, utilityInfo);
            }
        }

        Message IPeerServiceContract.ProcessRequestSecurityToken(Message message)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(typeof(IPeerNeighbor).ToString()));
            Message reply = this.config.SecurityManager.ProcessRequest(neighbor, message);
            if (reply == null)
            {
                OperationContext current = OperationContext.Current;
                current.RequestContext.Close();
                current.RequestContext = null;
            }
            return reply;
        }

        void IPeerServiceContract.Fault(Message message)
        {
            IPeerNeighbor neighbor = GetNeighbor();
            if (neighbor == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(typeof(IPeerNeighbor).ToString()));
            neighbor.Abort(PeerCloseReason.Faulted, PeerCloseInitiator.RemoteNode);
        }

        void IPeerServiceContract.Ping(Message message)
        {
        }


    }
}
