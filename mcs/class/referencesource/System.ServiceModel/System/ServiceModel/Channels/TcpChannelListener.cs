//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    static class TcpUri
    {
        public const int DefaultPort = 808;
    }

    abstract class TcpChannelListener<TChannel, TChannelAcceptor>
        : TcpChannelListener, IChannelListener<TChannel>
        where TChannel : class, IChannel
        where TChannelAcceptor : ChannelAcceptor<TChannel>
    {
        protected TcpChannelListener(TcpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
        }

        protected abstract TChannelAcceptor ChannelAcceptor { get; }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            ChannelAcceptor.Open(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, ChannelAcceptor);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            ChannelAcceptor.Close(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, ChannelAcceptor);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        protected override void OnAbort()
        {
            this.ChannelAcceptor.Abort();
            base.OnAbort();
        }

        public TChannel AcceptChannel()
        {
            return this.AcceptChannel(this.DefaultReceiveTimeout);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }

        public TChannel AcceptChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            return ChannelAcceptor.AcceptChannel(timeout);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return ChannelAcceptor.BeginAcceptChannel(timeout, callback, state);
        }

        public TChannel EndAcceptChannel(IAsyncResult result)
        {
            base.ThrowPending();
            return ChannelAcceptor.EndAcceptChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return ChannelAcceptor.WaitForChannel(timeout);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ChannelAcceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return ChannelAcceptor.EndWaitForChannel(result);
        }
    }

    class TcpReplyChannelListener
        : TcpChannelListener<IReplyChannel, ReplyChannelAcceptor>, ISingletonChannelListener
    {
        ReplyChannelAcceptor replyAcceptor;

        public TcpReplyChannelListener(TcpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.replyAcceptor = new ConnectionOrientedTransportReplyChannelAcceptor(this);
        }

        protected override ReplyChannelAcceptor ChannelAcceptor
        {
            get { return this.replyAcceptor; }
        }

        TimeSpan ISingletonChannelListener.ReceiveTimeout
        {
            get { return this.InternalReceiveTimeout; }
        }

        void ISingletonChannelListener.ReceiveRequest(RequestContext requestContext, Action callback, bool canDispatchOnThisThread)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.TcpChannelMessageReceived,
                    SR.GetString(SR.TraceCodeTcpChannelMessageReceived), requestContext.RequestMessage);
            }
            replyAcceptor.Enqueue(requestContext, callback, canDispatchOnThisThread);
        }
    }

    class TcpDuplexChannelListener
        : TcpChannelListener<IDuplexSessionChannel, InputQueueChannelAcceptor<IDuplexSessionChannel>>, ISessionPreambleHandler
    {
        InputQueueChannelAcceptor<IDuplexSessionChannel> duplexAcceptor;

        public TcpDuplexChannelListener(TcpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.duplexAcceptor = new InputQueueChannelAcceptor<IDuplexSessionChannel>(this);
        }

        protected override InputQueueChannelAcceptor<IDuplexSessionChannel> ChannelAcceptor
        {
            get { return this.duplexAcceptor; }
        }

        void ISessionPreambleHandler.HandleServerSessionPreamble(ServerSessionPreambleConnectionReader preambleReader,
            ConnectionDemuxer connectionDemuxer)
        {
            IDuplexSessionChannel channel = preambleReader.CreateDuplexSessionChannel(
                this, new EndpointAddress(this.Uri), ExposeConnectionProperty, connectionDemuxer);

            duplexAcceptor.EnqueueAndDispatch(channel, preambleReader.ConnectionDequeuedCallback);
        }
    }

    abstract class TcpChannelListener : ConnectionOrientedTransportChannelListener
    {
        bool teredoEnabled;
        int listenBacklog;
        bool portSharingEnabled;

        // "port 0" support
        Socket ipv4ListenSocket;
        Socket ipv6ListenSocket;
        ExtendedProtectionPolicy extendedProtectionPolicy;
        static Random randomPortGenerator = new Random(AppDomain.CurrentDomain.GetHashCode() | Environment.TickCount);

        static UriPrefixTable<ITransportManagerRegistration> transportManagerTable =
            new UriPrefixTable<ITransportManagerRegistration>(true);

        protected TcpChannelListener(TcpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.listenBacklog = bindingElement.ListenBacklog;
            this.portSharingEnabled = bindingElement.PortSharingEnabled;
            this.teredoEnabled = bindingElement.TeredoEnabled;
            this.extendedProtectionPolicy = bindingElement.ExtendedProtectionPolicy;
            SetIdleTimeout(bindingElement.ConnectionPoolSettings.IdleTimeout);
            InitializeMaxPooledConnections(bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);

            // for exclusive mode, we have "port 0" functionality
            if (!bindingElement.PortSharingEnabled && context.ListenUriMode == ListenUriMode.Unique)
            {
                SetupUniquePort(context);
            }
        }

        public bool PortSharingEnabled
        {
            get
            {
                return this.portSharingEnabled;
            }
        }

        public bool TeredoEnabled
        {
            get
            {
                return this.teredoEnabled;
            }
        }

        public int ListenBacklog
        {
            get
            {
                return this.listenBacklog;
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(ExtendedProtectionPolicy))
            {
                return (T)(object)this.extendedProtectionPolicy;
            }

            return base.GetProperty<T>();
        }

        internal Socket GetListenSocket(UriHostNameType ipHostNameType)
        {
            if (ipHostNameType == UriHostNameType.IPv4)
            {
                Socket result = this.ipv4ListenSocket;
                this.ipv4ListenSocket = null;
                return result;
            }
            else // UriHostNameType.IPv6
            {
                Socket result = this.ipv6ListenSocket;
                this.ipv6ListenSocket = null;
                return result;
            }
        }

        public override string Scheme
        {
            get { return Uri.UriSchemeNetTcp; }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        internal static void FixIpv6Hostname(UriBuilder uriBuilder, Uri originalUri)
        {
            if (originalUri.HostNameType == UriHostNameType.IPv6)
            {
                string ipv6Host = originalUri.DnsSafeHost;
                uriBuilder.Host = string.Concat("[", ipv6Host, "]");
            }
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration()
        {
            Uri listenUri = this.BaseUri;
            if (!this.PortSharingEnabled)
            {
                UriBuilder builder = new UriBuilder(listenUri.Scheme, listenUri.Host, listenUri.Port);
                TcpChannelListener.FixIpv6Hostname(builder, listenUri);
                listenUri = builder.Uri;
            }

            return this.CreateTransportManagerRegistration(listenUri);
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            if (this.PortSharingEnabled)
            {
                return new SharedTcpTransportManager(listenUri, this);
            }
            else
            {
                return new ExclusiveTcpTransportManagerRegistration(listenUri, this);
            }
        }

        Socket ListenAndBind(IPEndPoint localEndpoint)
        {
            Socket result = new Socket(localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                result.Bind(localEndpoint);
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    SocketConnectionListener.ConvertListenException(socketException, localEndpoint));
            }

            return result;
        }

        void SetupUniquePort(BindingContext context)
        {
            IPAddress ipv4Address = IPAddress.Any;
            IPAddress ipv6Address = IPAddress.IPv6Any;

            bool useIPv4 = Socket.OSSupportsIPv4;
            bool useIPv6 = Socket.OSSupportsIPv6;
            if (this.Uri.HostNameType == UriHostNameType.IPv6)
            {
                useIPv4 = false;
                ipv6Address = IPAddress.Parse(this.Uri.DnsSafeHost);
            }
            else if (this.Uri.HostNameType == UriHostNameType.IPv4)
            {
                useIPv6 = false;
                ipv4Address = IPAddress.Parse(this.Uri.DnsSafeHost);
            }

            if (!useIPv4 && !useIPv6)
            {
                if (this.Uri.HostNameType == UriHostNameType.IPv6)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                        "context",
                        SR.GetString(SR.TcpV6AddressInvalid, this.Uri));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                        "context",
                        SR.GetString(SR.TcpV4AddressInvalid, this.Uri));
                }
            }

            UriBuilder uriBuilder = new UriBuilder(context.ListenUriBaseAddress);
            int port = -1;
            if (!useIPv6) // we just want IPv4
            {
                this.ipv4ListenSocket = ListenAndBind(new IPEndPoint(ipv4Address, 0));
                port = ((IPEndPoint)this.ipv4ListenSocket.LocalEndPoint).Port;
            }
            else if (!useIPv4) // or just IPv6
            {
                this.ipv6ListenSocket = ListenAndBind(new IPEndPoint(ipv6Address, 0));
                port = ((IPEndPoint)this.ipv6ListenSocket.LocalEndPoint).Port;
            }
            else
            {
                // We need both IPv4 and IPv6 on the same port. We can't atomically bind for IPv4 and IPv6, 
                // so we try 10 times, which even with a 50% failure rate will statistically succeed 99.9% of the time.
                //
                // We look in the range of 49152-65534 for Vista default behavior parity.
                // http://www.iana.org/assignments/port-numbers
                // 
                // We also grab the 10 random numbers in a row to reduce collisions between multiple people somehow
                // colliding on the same seed.
                const int retries = 10;
                const int lowWatermark = 49152;
                const int highWatermark = 65535;

                int[] portNumbers = new int[retries];
                lock (randomPortGenerator)
                {
                    for (int i = 0; i < retries; i++)
                    {
                        portNumbers[i] = randomPortGenerator.Next(lowWatermark, highWatermark);
                    }
                }


                for (int i = 0; i < retries; i++)
                {
                    port = portNumbers[i];
                    try
                    {
                        this.ipv4ListenSocket = ListenAndBind(new IPEndPoint(ipv4Address, port));
                        this.ipv6ListenSocket = ListenAndBind(new IPEndPoint(ipv6Address, port));
                        break;
                    }
                    catch (AddressAlreadyInUseException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        if (this.ipv4ListenSocket != null)
                        {
                            this.ipv4ListenSocket.Close();
                            this.ipv4ListenSocket = null;
                        }
                        this.ipv6ListenSocket = null;
                    }
                }

                if (ipv4ListenSocket == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new AddressAlreadyInUseException(SR.GetString(SR.UniquePortNotAvailable)));
                }
            }

            uriBuilder.Port = port;
            base.SetUri(uriBuilder.Uri, context.ListenUriRelativeAddress);
        }
    }
}
