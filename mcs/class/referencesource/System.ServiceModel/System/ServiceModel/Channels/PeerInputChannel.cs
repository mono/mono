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

    class PeerInputChannel : InputChannel
    {
        EndpointAddress to;
        Uri via;
        PeerNode peerNode;
        bool released = false;

        public PeerInputChannel(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager,
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
            // first close the node, then the base
            return new ChainedAsyncResult(timeout, callback, state, OnBeginCloseNode, OnEndCloseNode,
                base.OnBeginClose, base.OnEndClose);
        }

        // fisrt step in the chained async close
        IAsyncResult OnBeginCloseNode(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.peerNode.InnerNode.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            // open the base, then the node
            return new ChainedAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen,
                OnBeginOpenNode, OnEndOpenNode);
        }

        // second step in the chained async open
        IAsyncResult OnBeginOpenNode(TimeSpan timeout, AsyncCallback callback, object state)
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
                if (release)
                {
                    this.peerNode.InnerNode.Release();
                }
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        void OnEndCloseNode(IAsyncResult result)
        {
            PeerNodeImplementation.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        void OnEndOpenNode(IAsyncResult result)
        {
            PeerNodeImplementation.EndOpen(result);
        }

        protected override void OnEnqueueItem(Message message)
        {
            // set the message's via to the uri on which it was received
            message.Properties.Via = this.via;

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerChannelMessageReceived,
                    SR.GetString(SR.TraceCodePeerChannelMessageReceived), this, message);
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            this.peerNode.OnOpen();
            this.peerNode.InnerNode.Open(timeoutHelper.RemainingTime(), true);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            ReleaseNode();
        }
    }
}
