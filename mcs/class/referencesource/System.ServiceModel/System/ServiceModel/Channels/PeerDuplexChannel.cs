//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    class PeerDuplexChannel : DuplexChannel
    {
        EndpointAddress to;
        Uri via;
        PeerNode peerNode;
        bool released = false;
        SecurityProtocol securityProtocol;
        ChannelManagerBase channelManager;
        PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> messageDispatcher;

        public PeerDuplexChannel(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager,
                                 EndpointAddress localAddress, Uri via)
            : base(channelManager, localAddress)
        {
            PeerNodeImplementation.ValidateVia(via);
            if (registration != null)
            {
                peerNode = PeerNodeImplementation.Get(via, registration);
            }
            this.peerNode = new PeerNode(peerNode);
            this.to = localAddress;
            this.via = via;
            this.channelManager = channelManager;
        }

        public override EndpointAddress RemoteAddress
        {
            get { return this.to; }
        }

        public override Uri Via
        {
            get { return this.via; }
        }

        public PeerNodeImplementation InnerNode
        {
            get { return this.peerNode.InnerNode; }
        }

        internal PeerMessageDispatcher<IDuplexChannel, PeerDuplexChannel> Dispatcher
        {
            get
            {
                return this.messageDispatcher;
            }
            set
            {
                Fx.Assert(this.State < CommunicationState.Opened, "Can not change the dispatcher on DuplexChannel after Open.");
                this.messageDispatcher = value;
            }
        }

        // add headers to an outgoing message
        protected override void AddHeadersTo(Message message)
        {
            base.AddHeadersTo(message);

            if (this.to != null)
            {
                this.to.ApplyTo(message);
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(PeerNode))
            {
                return (T)(object)this.peerNode;
            }
            else if (typeof(T) == typeof(PeerNodeImplementation))
            {
                return (T)(object)this.peerNode.InnerNode;
            }
            else if (typeof(T) == typeof(IOnlineStatus))
            {
                return (T)(object)this.peerNode;
            }
            else if (typeof(T) == typeof(FaultConverter))
            {
                return (T)(object)FaultConverter.GetDefaultFaultConverter(MessageVersion.Soap12WSAddressing10);
            }
            return base.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.State < CommunicationState.Closed)
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

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.peerNode.InnerNode.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result = this.peerNode.InnerNode.BeginOpen(timeout, callback, state, true);
            return result;
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.peerNode.InnerNode.Close(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            ReleaseNode();
        }

        void ReleaseNode()
        {
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
                if (release && (this.peerNode != null))
                {
                    if (this.messageDispatcher != null)
                        this.messageDispatcher.Unregister(false);
                    this.peerNode.InnerNode.Release();
                }
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            PeerNodeImplementation.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            PeerNodeImplementation.EndOpen(result);
        }

        protected override void OnEnqueueItem(Message message)
        {
            // set the message's via to the uri on which it was received
            message.Properties.Via = via;

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerChannelMessageReceived,
                    SR.GetString(SR.TraceCodePeerChannelMessageReceived), this, message);
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.peerNode.OnOpen();
            this.peerNode.InnerNode.Open(timeout, true);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            ReleaseNode();
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            EndSend(BeginSend(message, timeout, null, null));
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Fx.Assert(message.Headers.Action != message.Version.Addressing.DefaultFaultAction, "fault action in duplex.send");

            if (this.securityProtocol == null)
            {
                lock (ThisLock)
                {
                    if (this.securityProtocol == null)
                    {
                        this.securityProtocol = ((IPeerFactory)channelManager).SecurityManager.CreateSecurityProtocol<IDuplexChannel>(this.to, timeoutHelper.RemainingTime());
                    }
                }
            }
            return this.peerNode.InnerNode.BeginSend(this, message, this.via, (ITransportFactorySettings)Manager,
                timeoutHelper.RemainingTime(), callback, state, this.securityProtocol);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            PeerNodeImplementation.EndSend(result);
        }
    }
}
