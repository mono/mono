//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
// Enable this to dump contents of a connection a file.
//#define CONNECTIONDUMP

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;

    class TcpChannelFactory<TChannel> : ConnectionOrientedTransportChannelFactory<TChannel>,
        ITcpChannelFactorySettings
    {
        static TcpConnectionPoolRegistry connectionPoolRegistry = new TcpConnectionPoolRegistry();
        TimeSpan leaseTimeout;

        public TcpChannelFactory(TcpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context,
            bindingElement.ConnectionPoolSettings.GroupName,
            bindingElement.ConnectionPoolSettings.IdleTimeout,
            bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint,
            true)
        {
            this.leaseTimeout = bindingElement.ConnectionPoolSettings.LeaseTimeout;
        }

        public TimeSpan LeaseTimeout
        {
            get
            {
                return leaseTimeout;
            }
        }

        public override string Scheme
        {
            get { return Uri.UriSchemeNetTcp; }
        }

        internal override IConnectionInitiator GetConnectionInitiator()
        {
            IConnectionInitiator socketConnectionInitiator = new SocketConnectionInitiator(
                ConnectionBufferSize);
#if CONNECTIONDUMP
            socketConnectionInitiator = new ConnectionDumpInitiator(socketConnectionInitiator);
#endif
            return new BufferedConnectionInitiator(socketConnectionInitiator,
                MaxOutputDelay, ConnectionBufferSize);
        }

        internal override ConnectionPool GetConnectionPool()
        {
            return connectionPoolRegistry.Lookup(this);
        }

        internal override void ReleaseConnectionPool(ConnectionPool pool, TimeSpan timeout)
        {
            connectionPoolRegistry.Release(pool, timeout);
        }
    }
}
