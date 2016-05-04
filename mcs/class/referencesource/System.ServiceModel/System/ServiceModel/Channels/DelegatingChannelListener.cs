//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;

    abstract class DelegatingChannelListener<TChannel>
        : LayeredChannelListener<TChannel>
        where TChannel : class, IChannel
    {
        IChannelAcceptor<TChannel> channelAcceptor;

        protected DelegatingChannelListener(IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener)
            : base(timeouts, innerChannelListener)
        {
        }

        protected DelegatingChannelListener(bool sharedInnerListener)
            : base(sharedInnerListener)
        {
        }

        protected DelegatingChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts)
            : base(sharedInnerListener, timeouts)
        {
        }

        protected DelegatingChannelListener(bool sharedInnerListener, IDefaultCommunicationTimeouts timeouts, IChannelListener innerChannelListener)
            : base(sharedInnerListener, timeouts, innerChannelListener)
        {
        }


        public IChannelAcceptor<TChannel> Acceptor
        {
            get { return this.channelAcceptor; }
            set { this.channelAcceptor = value; }
        }

        protected override TChannel OnAcceptChannel(TimeSpan timeout)
        {
            return this.channelAcceptor.AcceptChannel(timeout);
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelAcceptor.BeginAcceptChannel(timeout, callback, state);
        }

        protected override TChannel OnEndAcceptChannel(IAsyncResult result)
        {
            return this.channelAcceptor.EndAcceptChannel(result);
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.channelAcceptor.WaitForChannel(timeout);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelAcceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.channelAcceptor.EndWaitForChannel(result);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.channelAcceptor != null)
            {
                this.channelAcceptor.Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, this.channelAcceptor);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnClose(timeoutHelper.RemainingTime());
            this.channelAcceptor.Close(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.channelAcceptor);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            this.channelAcceptor.Open(timeoutHelper.RemainingTime());
        }
    }
}
