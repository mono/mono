//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;

    class TransportReplyChannelAcceptor : ReplyChannelAcceptor
    {
        TransportManagerContainer transportManagerContainer;
        TransportChannelListener listener;

        public TransportReplyChannelAcceptor(TransportChannelListener listener)
            : base(listener)
        {
            this.listener = listener;
        }

        protected override ReplyChannel OnCreateChannel()
        {
            return new TransportReplyChannel(this.ChannelManager, null);
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.transportManagerContainer = this.listener.GetTransportManagers();
            this.listener = null;
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.transportManagerContainer != null && !TransferTransportManagers())
            {
                this.transportManagerContainer.Abort();
            }
        }

        IAsyncResult DummyBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        void DummyEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ChainedBeginHandler begin1 = DummyBeginClose;
            ChainedEndHandler end1 = DummyEndClose;
            if (this.transportManagerContainer != null && !TransferTransportManagers())
            {
                begin1 = this.transportManagerContainer.BeginClose;
                end1 = this.transportManagerContainer.EndClose;
            }

            return new ChainedAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, begin1, end1);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnClose(timeoutHelper.RemainingTime());
            if (this.transportManagerContainer != null && !TransferTransportManagers())
            {
                this.transportManagerContainer.Close(timeoutHelper.RemainingTime());
            }
        }

        // used to decouple our channel and listener lifetimes
        bool TransferTransportManagers()
        {
            TransportReplyChannel singletonChannel = (TransportReplyChannel)base.GetCurrentChannel();
            if (singletonChannel == null)
            {
                return false;
            }
            else
            {
                return singletonChannel.TransferTransportManagers(this.transportManagerContainer);
            }
        }

        // tracks TransportManager so that the channel can outlive the Listener
        protected class TransportReplyChannel : ReplyChannel
        {
            TransportManagerContainer transportManagerContainer;

            public TransportReplyChannel(ChannelManagerBase channelManager, EndpointAddress localAddress)
                : base(channelManager, localAddress)
            {
            }

            public bool TransferTransportManagers(TransportManagerContainer transportManagerContainer)
            {
                lock (ThisLock)
                {
                    if (this.State != CommunicationState.Opened)
                    {
                        return false;
                    }

                    this.transportManagerContainer = transportManagerContainer;
                    return true;
                }
            }

            protected override void OnAbort()
            {
                if (this.transportManagerContainer != null)
                {
                    this.transportManagerContainer.Abort();
                }
                base.OnAbort();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (this.transportManagerContainer != null)
                {
                    this.transportManagerContainer.Close(timeoutHelper.RemainingTime());
                }
                base.OnClose(timeoutHelper.RemainingTime());
            }

            IAsyncResult DummyBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            void DummyEndClose(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                ChainedBeginHandler begin1 = DummyBeginClose;
                ChainedEndHandler end1 = DummyEndClose;
                if (this.transportManagerContainer != null)
                {
                    begin1 = this.transportManagerContainer.BeginClose;
                    end1 = this.transportManagerContainer.EndClose;
                }

                return new ChainedAsyncResult(timeout, callback, state, begin1, end1,
                        base.OnBeginClose, base.OnEndClose);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }
        }
    }
}
