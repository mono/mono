//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;

    abstract class LayeredChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
    {
        IChannelFactory innerChannelFactory;

        public LayeredChannelFactory(IDefaultCommunicationTimeouts timeouts, IChannelFactory innerChannelFactory)
            : base(timeouts)
        {
            this.innerChannelFactory = innerChannelFactory;
        }

        protected IChannelFactory InnerChannelFactory
        {
            get { return this.innerChannelFactory; }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IChannelFactory<TChannel>))
            {
                return (T)(object)this;
            }

            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return this.innerChannelFactory.GetProperty<T>();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannelFactory.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannelFactory.EndOpen(result);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, this.innerChannelFactory);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnClose(timeoutHelper.RemainingTime());
            this.innerChannelFactory.Close(timeoutHelper.RemainingTime());
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannelFactory.Open(timeout);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.innerChannelFactory.Abort();
        }
    }

    class LayeredInputChannel : LayeredChannel<IInputChannel>, IInputChannel
    {
        public LayeredInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }

        public virtual EndpointAddress LocalAddress
        {
            get { return InnerChannel.LocalAddress; }
        }

        void InternalOnReceive(Message message)
        {
            if (message != null)
            {
                this.OnReceive(message);
            }
        }

        protected virtual void OnReceive(Message message)
        {
        }

        public Message Receive()
        {
            Message message = InnerChannel.Receive();
            this.InternalOnReceive(message);
            return message;
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message = InnerChannel.Receive(timeout);
            this.InternalOnReceive(message);
            return message;
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return InnerChannel.BeginReceive(callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginReceive(timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            Message message = InnerChannel.EndReceive(result);
            this.InternalOnReceive(message);
            return message;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            bool retVal = InnerChannel.EndTryReceive(result, out message);
            this.InternalOnReceive(message);
            return retVal;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool retVal = InnerChannel.TryReceive(timeout, out message);
            this.InternalOnReceive(message);
            return retVal;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return InnerChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return InnerChannel.EndWaitForMessage(result);
        }
    }

    class LayeredDuplexChannel : LayeredInputChannel, IDuplexChannel
    {
        IOutputChannel innerOutputChannel;
        EndpointAddress localAddress;
        EventHandler onInnerOutputChannelFaulted;

        public LayeredDuplexChannel(ChannelManagerBase channelManager, IInputChannel innerInputChannel, EndpointAddress localAddress, IOutputChannel innerOutputChannel)
            : base(channelManager, innerInputChannel)
        {
            this.localAddress = localAddress;
            this.innerOutputChannel = innerOutputChannel;
            this.onInnerOutputChannelFaulted = new EventHandler(OnInnerOutputChannelFaulted);
            this.innerOutputChannel.Faulted += this.onInnerOutputChannelFaulted;
        }

        public override EndpointAddress LocalAddress
        {
            get { return this.localAddress; }
        }

        public EndpointAddress RemoteAddress
        {
            get { return this.innerOutputChannel.RemoteAddress; }
        }

        public Uri Via
        {
            get { return innerOutputChannel.Via; }
        }

        protected override void OnClosing()
        {
            this.innerOutputChannel.Faulted -= this.onInnerOutputChannelFaulted;
            base.OnClosing();
        }

        protected override void OnAbort()
        {
            this.innerOutputChannel.Abort();
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, base.OnBeginClose, base.OnEndClose, this.innerOutputChannel);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedCloseAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.innerOutputChannel.Close(timeoutHelper.RemainingTime());
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.innerOutputChannel);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            innerOutputChannel.Open(timeoutHelper.RemainingTime());
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.innerOutputChannel.Send(message, timeout);
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerOutputChannel.BeginSend(message, timeout, callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            this.innerOutputChannel.EndSend(result);
        }

        void OnInnerOutputChannelFaulted(object sender, EventArgs e)
        {
            this.Fault();
        }
    }
}
