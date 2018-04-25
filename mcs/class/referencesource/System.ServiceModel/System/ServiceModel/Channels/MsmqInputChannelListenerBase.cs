//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    abstract class MsmqInputChannelListenerBase
        : MsmqChannelListenerBase<IInputChannel>
    {
        InputQueueChannelAcceptor<IInputChannel> acceptor;


        internal MsmqInputChannelListenerBase(MsmqBindingElementBase bindingElement, BindingContext context, MsmqReceiveParameters receiveParameters)
            : this(bindingElement, context, receiveParameters, TransportDefaults.GetDefaultMessageEncoderFactory())
        { }

        internal MsmqInputChannelListenerBase(MsmqBindingElementBase bindingElement,
                                              BindingContext context,
                                              MsmqReceiveParameters receiveParameters,
                                              MessageEncoderFactory encoderFactory)
            : base(bindingElement, context, receiveParameters, encoderFactory)
        {
            this.acceptor = new InputQueueChannelAcceptor<IInputChannel>(this);
        }

        void OnNewChannelNeeded(object sender, EventArgs ea)
        {
            if (!this.IsDisposed && (CommunicationState.Opened == this.State || CommunicationState.Opening == this.State))
            {
                IInputChannel inputChannel = CreateInputChannel(this);
                inputChannel.Closed += OnNewChannelNeeded;
                this.acceptor.EnqueueAndDispatch(inputChannel);
            }
        }

        protected override void OnOpenCore(TimeSpan timeout)
        {
            base.OnOpenCore(timeout);
            this.acceptor.Open();
            OnNewChannelNeeded(this, EventArgs.Empty);
        }

        protected override void OnCloseCore(bool aborting)
        {
            this.acceptor.Close();
            base.OnCloseCore(aborting);
        }

        protected abstract IInputChannel CreateInputChannel(MsmqInputChannelListenerBase listener);

        // AcceptChannel
        public override IInputChannel AcceptChannel()
        {
            return AcceptChannel(this.DefaultReceiveTimeout);
        }
        //
        public override IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }
        //
        public override IInputChannel AcceptChannel(TimeSpan timeout)
        {
            return this.acceptor.AcceptChannel(timeout);
        }
        //
        public override IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.acceptor.BeginAcceptChannel(timeout, callback, state);
        }
        //
        public override IInputChannel EndAcceptChannel(IAsyncResult result)
        {
            return this.acceptor.EndAcceptChannel(result);
        }

        // WaitForChannel
        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return this.acceptor.WaitForChannel(timeout);
        }
        //
        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.acceptor.BeginWaitForChannel(timeout, callback, state);
        }
        //
        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return this.acceptor.EndWaitForChannel(result);
        }
    }
}
