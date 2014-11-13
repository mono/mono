//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;

    abstract class ChannelAcceptor<TChannel> : CommunicationObject, IChannelAcceptor<TChannel>
        where TChannel : class, IChannel
    {
        ChannelManagerBase channelManager;

        protected ChannelAcceptor(ChannelManagerBase channelManager)
        {
            this.channelManager = channelManager;
        }

        protected ChannelManagerBase ChannelManager
        {
            get { return channelManager; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return this.channelManager.InternalCloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return this.channelManager.InternalOpenTimeout; }
        }

        public abstract TChannel AcceptChannel(TimeSpan timeout);
        public abstract IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract TChannel EndAcceptChannel(IAsyncResult result);

        public abstract bool WaitForChannel(TimeSpan timeout);
        public abstract IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract bool EndWaitForChannel(IAsyncResult result);

        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
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

        protected override void OnOpen(TimeSpan timeout)
        {
        }
    }
}
