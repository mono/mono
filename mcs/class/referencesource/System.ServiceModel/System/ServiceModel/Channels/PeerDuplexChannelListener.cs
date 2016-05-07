//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics;

    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    sealed class PeerDuplexChannelAcceptor : SingletonChannelAcceptor<IDuplexChannel, PeerDuplexChannel, Message>
    {
        PeerNodeImplementation peerNode;
        PeerNodeImplementation.Registration registration;
        EndpointAddress localAddress;
        Uri via;
        PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> dispatcher = null;

        public PeerDuplexChannelAcceptor(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via)
            : base(channelManager)
        {
            this.registration = registration;
            this.peerNode = peerNode;
            this.localAddress = localAddress;
            this.via = via;

            PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter queueHandler = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>.PeerMessageQueueAdapter(this);
            this.dispatcher = new PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel>(queueHandler, peerNode, ChannelManager, localAddress, via);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnClosing()
        {
            CloseDispatcher();
            base.OnClosing();
        }

        protected override void OnFaulted()
        {
            CloseDispatcher();
            base.OnFaulted();
        }

        void CloseDispatcher()
        {
            if (dispatcher != null)
            {
                dispatcher.Unregister(true);
                dispatcher = null;
            }

        }

        protected override PeerDuplexChannel OnCreateChannel()
        {
            return new PeerDuplexChannel(peerNode, registration, ChannelManager, localAddress, via);
        }

        protected override void OnTraceMessageReceived(Message message)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageReceived,
                    SR.GetString(SR.TraceCodeMessageReceived),
                    MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null);
            }
        }
    }
    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    sealed class PeerDuplexChannelListener : PeerChannelListener<IDuplexChannel, PeerDuplexChannelAcceptor>
    {
        PeerDuplexChannelAcceptor duplexAcceptor;

        public PeerDuplexChannelListener(PeerTransportBindingElement bindingElement, BindingContext context, PeerResolver peerResolver)
            : base(bindingElement, context, peerResolver)
        {
        }

        protected override PeerDuplexChannelAcceptor ChannelAcceptor
        {
            get { return this.duplexAcceptor; }
        }

        protected override void CreateAcceptor()
        {
            this.duplexAcceptor = new PeerDuplexChannelAcceptor(this.InnerNode, this.Registration, this, new EndpointAddress(this.Uri), this.BaseUri);
        }
    }
}
