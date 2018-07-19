//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    interface IPeerFactory : ITransportFactorySettings
    {
        IPAddress ListenIPAddress { get; }
        int Port { get; }
        XmlDictionaryReaderQuotas ReaderQuotas { get; }
        PeerResolver Resolver { get; }
        PeerSecurityManager SecurityManager { get; }
        PeerNodeImplementation PrivatePeerNode { get; set; }
        long MaxBufferPoolSize { get; }
    }

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    sealed class PeerChannelFactory<TChannel> : TransportChannelFactory<TChannel>, IPeerFactory
    {
        // settings passed to PeerNode
        IPAddress listenIPAddress;
        int port;
        PeerResolver resolver;
        PeerSecurityManager securityManager;
        XmlDictionaryReaderQuotas readerQuotas;
        ISecurityCapabilities securityCapabilities;

        // use a private mesh (PeerNode) rather than creating/retrieving one from the registry.
        // used as a test hook to allow multiple PeerNode instances per app domain
        PeerNodeImplementation privatePeerNode;

        internal PeerChannelFactory(PeerTransportBindingElement bindingElement, BindingContext context,
            PeerResolver peerResolver)
            : base(bindingElement, context)
        {
            this.listenIPAddress = bindingElement.ListenIPAddress;
            this.port = bindingElement.Port;
            this.resolver = peerResolver;
            readerQuotas = new XmlDictionaryReaderQuotas();
            BinaryMessageEncodingBindingElement encoder = context.Binding.Elements.Find<BinaryMessageEncodingBindingElement>();
            if (encoder != null)
                encoder.ReaderQuotas.CopyTo(this.readerQuotas);
            else
                EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            this.securityManager = PeerSecurityManager.Create(bindingElement.Security, context, this.readerQuotas);
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
        }

        public IPAddress ListenIPAddress
        {
            get { return listenIPAddress; }
        }

        public int Port
        {
            get { return port; }
        }

        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get
            {
                return this.readerQuotas;
            }
        }

        public PeerResolver Resolver
        {
            get { return resolver; }
        }

        public override string Scheme
        {
            get { return PeerStrings.Scheme; }
        }

        public PeerNodeImplementation PrivatePeerNode
        {
            get { return privatePeerNode; }
            set { privatePeerNode = value; }
        }

        public PeerSecurityManager SecurityManager
        {
            get { return this.securityManager; }
            set { this.securityManager = value; }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(PeerChannelFactory<TChannel>))
            {
                return (T)(object)this;
            }
            else if (typeof(T) == typeof(IPeerFactory))
            {
                return (T)(object)this;
            }
            else if (typeof(T) == typeof(PeerNodeImplementation))
            {
                return (T)(object)privatePeerNode;
            }
            else if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }

            return base.GetProperty<T>();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override TChannel OnCreateChannel(EndpointAddress to, Uri via)
        {
            base.ValidateScheme(via);

            PeerNodeImplementation peerNode = null;
            PeerNodeImplementation.Registration registration = null;

            // use the private PeerNode if it has been configured and matches the channel
            // otherwise have the channel look for one or create a new one
            if (privatePeerNode != null && via.Host == privatePeerNode.MeshId)
            {
                peerNode = privatePeerNode;
            }
            else
            {
                registration = new PeerNodeImplementation.Registration(via, this);
            }

            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel)(object)new PeerOutputChannel(peerNode, registration, this, to, via, this.MessageVersion);
            }

            // typeof(TChannel) == typeof(IDuplexChannel)
            // 'to' is both the remote address and the local address
            PeerDuplexChannel duplexChannel = new PeerDuplexChannel(peerNode, registration, this, to, via);
            PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter queueHandler = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter(duplexChannel);
            PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> dispatcher = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>(queueHandler, duplexChannel.InnerNode, this, to, via);
            duplexChannel.Dispatcher = dispatcher;
            return (TChannel)(object)duplexChannel;
        }
    }
}
