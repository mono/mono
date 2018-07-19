//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    class PeerOutputChannel : TransportOutputChannel
    {
        PeerNode peerNode;
        Uri via;
        EndpointAddress to;
        SecurityProtocol securityProtocol;
        bool released;
        ChannelManagerBase channelManager;

        public PeerOutputChannel(PeerNodeImplementation peerNode, PeerNodeImplementation.Registration registration, ChannelManagerBase channelManager,
            EndpointAddress localAddress, Uri via, MessageVersion messageVersion)
            : base(channelManager, localAddress, via, false, messageVersion)
        {
            PeerNodeImplementation.ValidateVia(via);
            if (registration != null)
            {
                peerNode = PeerNodeImplementation.Get(via, registration);
            }
            this.peerNode = new PeerNode(peerNode);
            this.via = via;
            this.channelManager = channelManager;
            this.to = localAddress;
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
            this.peerNode.InnerNode.Close(timeout);
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
                    this.peerNode.InnerNode.Release();
                }
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

        protected override void OnEndClose(IAsyncResult result)
        {
            PeerNodeImplementation.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            PeerNodeImplementation.EndOpen(result);
        }

        protected override void OnSend(Message message, TimeSpan timeout)
        {
            EndSend(BeginSend(message, timeout, null, null));
        }

        protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            if (this.securityProtocol == null)
            {
                lock (ThisLock)
                {
                    if (this.securityProtocol == null)
                    {
                        this.securityProtocol = ((IPeerFactory)channelManager).SecurityManager.CreateSecurityProtocol<IOutputChannel>(this.to, timeoutHelper.RemainingTime());
                    }
                }
            }
            return this.peerNode.InnerNode.BeginSend(this, message, this.via, (ITransportFactorySettings)Manager, timeoutHelper.RemainingTime(), callback, state, this.securityProtocol);
        }

        protected override void OnEndSend(IAsyncResult result)
        {
            PeerNodeImplementation.EndSend(result);
        }

        protected override void AddHeadersTo(Message message)
        {
            this.RemoteAddress.ApplyTo(message);
        }
    }
}
