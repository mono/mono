//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.ServiceModel;

    sealed class ExclusiveTcpTransportManager : TcpTransportManager, ISocketListenerSettings
    {
        bool closed;
        ConnectionDemuxer connectionDemuxer;
        IConnectionListener connectionListener;
        IPAddress ipAddress;
        int listenBacklog;
        Socket listenSocket;
        ExclusiveTcpTransportManagerRegistration registration;

        public ExclusiveTcpTransportManager(ExclusiveTcpTransportManagerRegistration registration,
            TcpChannelListener channelListener, IPAddress ipAddressAny, UriHostNameType ipHostNameType)
        {
            ApplyListenerSettings(channelListener);

            this.listenSocket = channelListener.GetListenSocket(ipHostNameType);
            if (this.listenSocket != null)
            {
                this.ipAddress = ((IPEndPoint)this.listenSocket.LocalEndPoint).Address;
            }
            else if (channelListener.Uri.HostNameType == ipHostNameType)
            {
                this.ipAddress = IPAddress.Parse(channelListener.Uri.DnsSafeHost);
            }
            else
            {
                this.ipAddress = ipAddressAny;
            }

            this.listenBacklog = channelListener.ListenBacklog;
            this.registration = registration;
        }

        public IPAddress IPAddress
        {
            get
            {
                return this.ipAddress;
            }
        }

        public int ListenBacklog
        {
            get
            {
                return this.listenBacklog;
            }
        }

        int ISocketListenerSettings.BufferSize
        {
            get { return ConnectionBufferSize; }
        }

        bool ISocketListenerSettings.TeredoEnabled
        {
            get { return registration.TeredoEnabled; }
        }

        int ISocketListenerSettings.ListenBacklog
        {
            get { return ListenBacklog; }
        }

        internal override void OnOpen()
        {
            SocketConnectionListener socketListener = null;

            if (this.listenSocket != null)
            {
                socketListener = new SocketConnectionListener(this.listenSocket, this, false);
                this.listenSocket = null;
            }
            else
            {
                int port = this.registration.ListenUri.Port;
                if (port == -1)
                    port = TcpUri.DefaultPort;

                socketListener = new SocketConnectionListener(new IPEndPoint(ipAddress, port), this, false);
            }

            connectionListener = new BufferedConnectionListener(socketListener, MaxOutputDelay, ConnectionBufferSize);
            if (DiagnosticUtility.ShouldUseActivity)
            {
                connectionListener = new TracingConnectionListener(connectionListener, this.registration.ListenUri.ToString(), false);
            }
            connectionDemuxer = new ConnectionDemuxer(connectionListener,
                MaxPendingAccepts, MaxPendingConnections, ChannelInitializationTimeout,
                IdleTimeout, MaxPooledConnections,
                OnGetTransportFactorySettings,
                OnGetSingletonMessageHandler,
                OnHandleServerSessionPreamble,
                OnDemuxerError);

            bool startedDemuxing = false;
            try
            {
                connectionDemuxer.StartDemuxing();
                startedDemuxing = true;
            }
            finally
            {
                if (!startedDemuxing)
                {
                    connectionDemuxer.Dispose();
                }
            }
        }

        internal override void OnClose(TimeSpan timeout)
        {
            Cleanup();
        }

        internal override void OnAbort()
        {
            Cleanup();
            base.OnAbort();
        }

        void Cleanup()
        {
            lock (this.ThisLock)
            {
                if (this.closed)
                {
                    return;
                }

                this.closed = true;
            }

            if (connectionDemuxer != null)
            {
                connectionDemuxer.Dispose();
            }

            if (connectionListener != null)
            {
                connectionListener.Dispose();
            }

            this.registration.OnClose(this);
        }
    }

    class ExclusiveTcpTransportManagerRegistration : TransportManagerRegistration
    {
        int connectionBufferSize;
        TimeSpan channelInitializationTimeout;
        TimeSpan idleTimeout;
        int maxPooledConnections;
        bool teredoEnabled;
        int listenBacklog;
        TimeSpan maxOutputDelay;
        int maxPendingConnections;
        int maxPendingAccepts;

        ExclusiveTcpTransportManager ipv4TransportManager;
        ExclusiveTcpTransportManager ipv6TransportManager;

        public ExclusiveTcpTransportManagerRegistration(Uri listenUri, TcpChannelListener channelListener)
            : base(listenUri, channelListener.HostNameComparisonMode)
        {
            this.connectionBufferSize = channelListener.ConnectionBufferSize;
            this.channelInitializationTimeout = channelListener.ChannelInitializationTimeout;
            this.teredoEnabled = channelListener.TeredoEnabled;
            this.listenBacklog = channelListener.ListenBacklog;
            this.maxOutputDelay = channelListener.MaxOutputDelay;
            this.maxPendingConnections = channelListener.MaxPendingConnections;
            this.maxPendingAccepts = channelListener.MaxPendingAccepts;
            this.idleTimeout = channelListener.IdleTimeout;
            this.maxPooledConnections = channelListener.MaxPooledConnections;
        }

        public bool TeredoEnabled
        {
            get { return this.teredoEnabled; }
        }

        public void OnClose(TcpTransportManager manager)
        {
            if (manager == this.ipv4TransportManager)
            {
                this.ipv4TransportManager = null;
            }
            else if (manager == this.ipv6TransportManager)
            {
                this.ipv6TransportManager = null;
            }
            else
            {
                Fx.Assert("Unknown transport manager passed to OnClose().");
            }

            if ((this.ipv4TransportManager == null) && (this.ipv6TransportManager == null))
            {
                TcpChannelListener.StaticTransportManagerTable.UnregisterUri(this.ListenUri, this.HostNameComparisonMode);
            }
        }

        bool IsCompatible(TcpChannelListener channelListener, bool useIPv4, bool useIPv6)
        {
            if (channelListener.InheritBaseAddressSettings)
                return true;

            if (useIPv6)
            {
                if (!channelListener.IsScopeIdCompatible(HostNameComparisonMode, this.ListenUri))
                {
                    return false;
                }
            }

            return (!channelListener.PortSharingEnabled
                && (useIPv4 || useIPv6)
                && (this.channelInitializationTimeout == channelListener.ChannelInitializationTimeout)
                && (this.idleTimeout == channelListener.IdleTimeout)
                && (this.maxPooledConnections == channelListener.MaxPooledConnections)
                && (this.connectionBufferSize == channelListener.ConnectionBufferSize)
                && (!useIPv6 || (this.teredoEnabled == channelListener.TeredoEnabled))
                && (this.listenBacklog == channelListener.ListenBacklog)
                && (this.maxPendingConnections == channelListener.MaxPendingConnections)
                && (this.maxOutputDelay == channelListener.MaxOutputDelay)
                && (this.maxPendingAccepts == channelListener.MaxPendingAccepts));
        }

        void ProcessSelection(TcpChannelListener channelListener, IPAddress ipAddressAny, UriHostNameType ipHostNameType,
            ref ExclusiveTcpTransportManager transportManager, IList<TransportManager> result)
        {
            if (transportManager == null)
            {
                transportManager = new ExclusiveTcpTransportManager(this, channelListener, ipAddressAny, ipHostNameType);
            }
            result.Add(transportManager);
        }

        public override IList<TransportManager> Select(TransportChannelListener channelListener)
        {
            bool useIPv4 = (this.ListenUri.HostNameType != UriHostNameType.IPv6) && Socket.OSSupportsIPv4;
            bool useIPv6 = (this.ListenUri.HostNameType != UriHostNameType.IPv4) && Socket.OSSupportsIPv6;

            TcpChannelListener tcpListener = (TcpChannelListener)channelListener;
            if (!this.IsCompatible(tcpListener, useIPv4, useIPv6))
            {
                return null;
            }

            IList<TransportManager> result = new List<TransportManager>();
            if (useIPv4)
            {
                this.ProcessSelection(tcpListener, IPAddress.Any, UriHostNameType.IPv4,
                    ref this.ipv4TransportManager, result);
            }
            if (useIPv6)
            {
                this.ProcessSelection(tcpListener, IPAddress.IPv6Any, UriHostNameType.IPv6,
                    ref this.ipv6TransportManager, result);
            }
            return result;
        }
    }
}
