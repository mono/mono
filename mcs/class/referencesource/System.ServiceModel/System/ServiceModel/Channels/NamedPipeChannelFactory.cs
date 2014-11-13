//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
// Enable this to dump the contents of a connection to a file.
//#define CONNECTIONDUMP
namespace System.ServiceModel.Channels
{
    class NamedPipeChannelFactory<TChannel> : ConnectionOrientedTransportChannelFactory<TChannel>, IPipeTransportFactorySettings
    {
        static NamedPipeConnectionPoolRegistry connectionPoolRegistry = new NamedPipeConnectionPoolRegistry();

        public NamedPipeChannelFactory(NamedPipeTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context,
            GetConnectionGroupName(bindingElement),
            bindingElement.ConnectionPoolSettings.IdleTimeout,
            bindingElement.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint,
            false)
        {
            if (bindingElement.PipeSettings != null)
            {
                this.PipeSettings = bindingElement.PipeSettings.Clone();
            }
        }

        public override string Scheme
        {
            get { return Uri.UriSchemeNetPipe; }
        }


        public NamedPipeSettings PipeSettings
        {
            get;
            private set;
        }

        static string GetConnectionGroupName(NamedPipeTransportBindingElement bindingElement)
        {
            return bindingElement.ConnectionPoolSettings.GroupName + bindingElement.PipeSettings.ApplicationContainerSettings.GetConnectionGroupSuffix();
        }

        internal override IConnectionInitiator GetConnectionInitiator()
        {
            IConnectionInitiator pipeConnectionInitiator =
                new PipeConnectionInitiator(ConnectionBufferSize, this);
#if CONNECTIONDUMP
            pipeConnectionInitiator = new ConnectionDumpInitiator(pipeConnectionInitiator);
#endif
            return new BufferedConnectionInitiator(pipeConnectionInitiator, MaxOutputDelay, ConnectionBufferSize);
        }

        internal override ConnectionPool GetConnectionPool()
        {
            return connectionPoolRegistry.Lookup(this);
        }

        internal override void ReleaseConnectionPool(ConnectionPool pool, TimeSpan timeout)
        {
            connectionPoolRegistry.Release(pool, timeout);
        }

        protected override bool SupportsUpgrade(StreamUpgradeBindingElement upgradeBindingElement)
        {
            return !(upgradeBindingElement is SslStreamSecurityBindingElement);
        }
    }
}
