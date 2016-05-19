//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    class PeerMessageDispatcher<ChannelInterfaceType, TChannel> : CommunicationObject
        where ChannelInterfaceType : class, IChannel
        where TChannel : InputQueueChannel<Message>
    {
        public class PeerMessageQueueAdapter
        {
            SingletonChannelAcceptor<ChannelInterfaceType, TChannel, Message> singletonChannelAcceptor;
            InputQueueChannel<Message> inputQueueChannel;

            public PeerMessageQueueAdapter(SingletonChannelAcceptor<ChannelInterfaceType, TChannel, Message> singletonChannelAcceptor)
            {
                this.singletonChannelAcceptor = singletonChannelAcceptor;
            }

            public PeerMessageQueueAdapter(InputQueueChannel<Message> inputQueueChannel)
            {
                this.inputQueueChannel = inputQueueChannel;
            }

            public void EnqueueAndDispatch(Message message, Action callback)
            {
                if (singletonChannelAcceptor != null)
                {
                    singletonChannelAcceptor.Enqueue(message, callback);
                }
                else if (inputQueueChannel != null)
                {
                    inputQueueChannel.EnqueueAndDispatch(message, callback);
                }
            }
        }

        Uri via;
        EndpointAddress to;
        SecurityProtocol securityProtocol;
        PeerNodeImplementation peerNode;
        PeerMessageQueueAdapter queueHandler;
        ChannelManagerBase channelManager;
        PeerQuotaHelper quotaHelper = new PeerQuotaHelper(Int32.MaxValue);
        bool registered;

        public PeerMessageDispatcher(PeerMessageQueueAdapter queueHandler, PeerNodeImplementation peerNode, ChannelManagerBase channelManager, EndpointAddress to, Uri via)
        {
            PeerNodeImplementation.ValidateVia(via);

            this.queueHandler = queueHandler;
            this.peerNode = peerNode;
            this.to = to;
            this.via = via;
            this.channelManager = channelManager;
            EndpointAddress filterTo = null;

            this.securityProtocol = ((IPeerFactory)channelManager).SecurityManager.CreateSecurityProtocol<ChannelInterfaceType>(to, ServiceDefaults.SendTimeout);

            if (typeof(IDuplexChannel).IsAssignableFrom(typeof(ChannelInterfaceType)))
                filterTo = to;

            //Register this handler
            PeerMessageFilter[] filters = new PeerMessageFilter[] { new PeerMessageFilter(via, filterTo) };
            peerNode.RegisterMessageFilter(this, this.via, filters, (ITransportFactorySettings)this.channelManager,
                                           new PeerNodeImplementation.MessageAvailableCallback(OnMessageAvailable), securityProtocol);
            registered = true;
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return channelManager.InternalCloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return channelManager.InternalOpenTimeout; }
        }

        public SecurityProtocol SecurityProtocol
        {
            get { return securityProtocol; }
        }

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnClose(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            Unregister(true);
        }

        internal void Unregister()
        {
            Unregister(false);
        }

        internal void Unregister(bool release)
        {
            PeerNodeImplementation node = this.peerNode;
            if (node != null)
            {
                if (registered)
                {
                    node.UnregisterMessageFilter(this, via);
                    registered = false;
                }
                if (release)
                    node.Release();
            }
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

        public void OnMessageAvailable(Message message)
        {
            quotaHelper.ReadyToEnqueueItem();
            queueHandler.EnqueueAndDispatch(message, quotaHelper.ItemDequeued);
        }
    }
    class PeerMessageFilter
    {
        Uri via;
        Uri actingAs;

        public PeerMessageFilter(Uri via) : this(via, null) { }
        public PeerMessageFilter(Uri via, EndpointAddress to)
        {
            Fx.Assert(via != null, "PeerMessageFilter via can not be set to null");
            this.via = via;
            if (to != null)
                this.actingAs = to.Uri;
        }

        public bool Match(Uri peerVia, Uri toCond)
        {
            bool result = false;
            if (peerVia == null)
            {
                result = false;
            }
            else if (Uri.Compare(this.via, peerVia,
               (UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path),
               UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) != 0)
            {
                result = false;
            }
            else if (this.actingAs != null)
            {
                result = Uri.Compare(this.actingAs, toCond,
               (UriComponents.Scheme | UriComponents.UserInfo | UriComponents.Host | UriComponents.Port | UriComponents.Path),
               UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase) == 0;
            }
            else
                result = true;

            return result;
        }

    }
}
