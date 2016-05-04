//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Channels;
    using System.Runtime;

    // 
    abstract class LayeredChannel<TInnerChannel> : ChannelBase
        where TInnerChannel : class, IChannel
    {
        TInnerChannel innerChannel;
        EventHandler onInnerChannelFaulted;

        protected LayeredChannel(ChannelManagerBase channelManager, TInnerChannel innerChannel)
            : base(channelManager)
        {
            Fx.Assert(innerChannel != null, "innerChannel cannot be null");

            this.innerChannel = innerChannel;
            this.onInnerChannelFaulted = new EventHandler(OnInnerChannelFaulted);
            this.innerChannel.Faulted += this.onInnerChannelFaulted;
        }

        protected TInnerChannel InnerChannel
        {
            get { return this.innerChannel; }
        }

        public override T GetProperty<T>()
        {
            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return this.InnerChannel.GetProperty<T>();
        }

        protected override void OnClosing()
        {
            this.innerChannel.Faulted -= this.onInnerChannelFaulted;
            base.OnClosing();
        }

        protected override void OnAbort()
        {
            this.innerChannel.Abort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.innerChannel.Close(timeout);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannel.BeginClose(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.innerChannel.EndClose(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannel.Open(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannel.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannel.EndOpen(result);
        }

        void OnInnerChannelFaulted(object sender, EventArgs e)
        {
            Fault();
        }
    }
}
