//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    abstract class PeerChannelListenerBase : TransportChannelListener, IPeerFactory
    {
        // settings passed to PeerNode
        IPAddress listenIPAddress;
        int port;
        PeerResolver resolver;
        PeerNode peerNode;
        PeerNodeImplementation privatePeerNode;
        PeerNodeImplementation.Registration registration;
        bool released = false;
        PeerSecurityManager securityManager;
        SecurityProtocol securityProtocol;
        XmlDictionaryReaderQuotas readerQuotas;
        ISecurityCapabilities securityCapabilities;

        static UriPrefixTable<ITransportManagerRegistration> transportManagerTable =
            new UriPrefixTable<ITransportManagerRegistration>(true);

        internal PeerChannelListenerBase(PeerTransportBindingElement bindingElement, BindingContext context,
            PeerResolver peerResolver)
            : base(bindingElement, context)
        {
            this.listenIPAddress = bindingElement.ListenIPAddress;
            this.port = bindingElement.Port;
            this.resolver = peerResolver;
            this.readerQuotas = new XmlDictionaryReaderQuotas();
            BinaryMessageEncodingBindingElement encoder = context.Binding.Elements.Find<BinaryMessageEncodingBindingElement>();
            if (encoder != null)
                encoder.ReaderQuotas.CopyTo(this.readerQuotas);
            else
                EncoderDefaults.ReaderQuotas.CopyTo(this.readerQuotas);
            securityManager = PeerSecurityManager.Create(bindingElement.Security, context, this.readerQuotas);
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
        }

        public IPAddress ListenIPAddress
        {
            get { return listenIPAddress; }
        }

        internal PeerNodeImplementation InnerNode
        {
            get
            {
                return peerNode != null ? peerNode.InnerNode : null;
            }
        }

        internal PeerNodeImplementation.Registration Registration
        {
            get { return registration; }
        }

        public PeerNodeImplementation PrivatePeerNode
        {
            get { return privatePeerNode; }
            set { privatePeerNode = value; }
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

        public PeerSecurityManager SecurityManager
        {
            get { return this.securityManager; }
            set { this.securityManager = value; }
        }

        protected SecurityProtocol SecurityProtocol
        {
            get { return this.securityProtocol; }
            set { this.securityProtocol = value; }
        }

        public override string Scheme
        {
            get { return PeerStrings.Scheme; }
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

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(PeerNode))
            {
                return peerNode as T;
            }
            else if (typeof(T) == typeof(IOnlineStatus))
            {
                return peerNode as T;
            }
            else if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }

            return base.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.State < CommunicationState.Closed && this.peerNode != null)
            {
                try
                {
                    this.peerNode.InnerNode.Abort();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
        }

        void OnCloseCore(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            peerNode.OnClose();
            peerNode.InnerNode.Close(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseCore(timeout);
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            if (!this.released)
            {
                bool release = false;
                lock (ThisLock)
                {
                    if (!this.released)
                    {
                        release = this.released = true;
                    }
                }
                if (release && this.peerNode != null)
                {
                    this.peerNode.InnerNode.Release();
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            return new CompletedAsyncResult<TimeoutHelper>(timeoutHelper, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            TimeoutHelper timeoutHelper = CompletedAsyncResult<TimeoutHelper>.End(result);
            OnCloseCore(timeoutHelper.RemainingTime());
        }

        void OnOpenCore(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            peerNode.OnOpen();
            peerNode.InnerNode.Open(timeoutHelper.RemainingTime(), false);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            OnOpenCore(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            return new CompletedAsyncResult<TimeoutHelper>(timeoutHelper, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            TimeoutHelper timeoutHelper = CompletedAsyncResult<TimeoutHelper>.End(result);
            OnOpenCore(timeoutHelper.RemainingTime());
        }

        protected override void OnFaulted()
        {
            OnAbort();              // Fault aborts the PeerNode
        }

        internal override IList<TransportManager> SelectTransportManagers()
        {
            //test override
            if (peerNode == null)
            {
                PeerNodeImplementation foundPeerNode = null;

                // use the private InnerNode if it has been configured and matches the channel
                if (privatePeerNode != null && this.Uri.Host == privatePeerNode.MeshId)
                {
                    foundPeerNode = privatePeerNode;
                    this.registration = null;
                }
                else
                {
                    // find or create a InnerNode for the given Uri
                    this.registration = new PeerNodeImplementation.Registration(this.Uri, this);
                    foundPeerNode = PeerNodeImplementation.Get(this.Uri, registration);
                }

                // ensure that the max message size is compatible
                if (foundPeerNode.MaxReceivedMessageSize < MaxReceivedMessageSize)
                {
                    foundPeerNode.Release();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR.GetString(SR.PeerMaxReceivedMessageSizeConflict, MaxReceivedMessageSize, foundPeerNode.MaxReceivedMessageSize, this.Uri)));
                }

                // associate with the PeerNode and open it
                peerNode = new PeerNode(foundPeerNode);
            }

            return null;
        }
    }

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    internal abstract class PeerChannelListener<TChannel, TChannelAcceptor> : PeerChannelListenerBase, IChannelListener<TChannel>
        where TChannel : class, IChannel
        where TChannelAcceptor : ChannelAcceptor<TChannel>
    {
        public PeerChannelListener(PeerTransportBindingElement bindingElement, BindingContext context, PeerResolver peerResolver)
            : base(bindingElement, context, peerResolver)
        {
        }

        protected abstract TChannelAcceptor ChannelAcceptor { get; }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return null;
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
            return ChannelAcceptor.EndAcceptChannel(result);
        }

        void OnCloseCore(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.ChannelAcceptor.Close(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseCore(timeout);
        }

        protected override void OnAbort()
        {
            if (this.ChannelAcceptor != null)
                this.ChannelAcceptor.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            return new CompletedAsyncResult<TimeoutHelper>(timeoutHelper, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            TimeoutHelper timeoutHelper = CompletedAsyncResult<TimeoutHelper>.End(result);
            OnCloseCore(timeoutHelper.RemainingTime());
        }

        void OnOpenCore(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            CreateAcceptor();
            ChannelAcceptor.Open(timeoutHelper.RemainingTime());
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            OnOpenCore(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            return new CompletedAsyncResult<TimeoutHelper>(timeoutHelper, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            TimeoutHelper timeoutHelper = CompletedAsyncResult<TimeoutHelper>.End(result);
            OnOpenCore(timeoutHelper.RemainingTime());
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

        protected abstract void CreateAcceptor();
    }
}
