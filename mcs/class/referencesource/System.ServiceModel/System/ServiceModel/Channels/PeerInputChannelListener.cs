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

    sealed class PeerInputChannelAcceptor : SingletonChannelAcceptor<IInputChannel, PeerInputChannel, Message>
    {
        PeerNodeImplementation peerNode;
        PeerNodeImplementation.Registration registration;
        EndpointAddress localAddress;
        Uri via;
        PeerMessageDispatcher<IInputChannel, PeerInputChannel> dispatcher = null;

        public PeerInputChannelAcceptor(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager, EndpointAddress localAddress, Uri via)
            : base(channelManager)
        {
            this.registration = registration;
            this.peerNode = peerNode;
            this.localAddress = localAddress;
            this.via = via;
            PeerMessageDispatcher<IInputChannel, PeerInputChannel>.PeerMessageQueueAdapter queueHandler = new PeerMessageDispatcher<IInputChannel, PeerInputChannel>.PeerMessageQueueAdapter(this);
            dispatcher = new PeerMessageDispatcher<IInputChannel, PeerInputChannel>(queueHandler, peerNode, ChannelManager, localAddress, via);
        }

        protected override PeerInputChannel OnCreateChannel()
        {
            return new PeerInputChannel(peerNode, registration, ChannelManager, localAddress, via);
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
    }
    
    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    sealed class PeerInputChannelListener : PeerChannelListener<IInputChannel, PeerInputChannelAcceptor>
    {
        PeerInputChannelAcceptor inputAcceptor;

        public PeerInputChannelListener(PeerTransportBindingElement bindingElement, BindingContext context, PeerResolver peerResolver)
            : base(bindingElement, context, peerResolver)
        {
        }

        protected override PeerInputChannelAcceptor ChannelAcceptor
        {
            get { return this.inputAcceptor; }
        }

        protected override void CreateAcceptor()
        {
            this.inputAcceptor = new PeerInputChannelAcceptor(this.InnerNode, this.Registration, this, new EndpointAddress(this.Uri), this.Uri);
        }
    }
}
