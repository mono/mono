//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel.Diagnostics;

    sealed class InternalDuplexChannelListener : DelegatingChannelListener<IDuplexChannel>
    {
        IChannelFactory<IOutputChannel> innerChannelFactory;
        bool providesCorrelation;

        internal InternalDuplexChannelListener(InternalDuplexBindingElement bindingElement, BindingContext context)
            : base(context.Binding, context.Clone().BuildInnerChannelListener<IInputChannel>())
        {
            this.innerChannelFactory = context.BuildInnerChannelFactory<IOutputChannel>();
            this.providesCorrelation = bindingElement.ProvidesCorrelation;
        }

        IOutputChannel GetOutputChannel(Uri to, TimeoutHelper timeoutHelper)
        {
            IOutputChannel channel = this.innerChannelFactory.CreateChannel(new EndpointAddress(to));
            channel.Open(timeoutHelper.RemainingTime());
            return channel;
        }

        protected override void OnAbort()
        {
            try
            {
                this.innerChannelFactory.Abort();
            }
            finally
            {
                base.OnAbort();
            }
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

        protected override void OnOpening()
        {
            base.OnOpening();
            this.Acceptor = (IChannelAcceptor<IDuplexChannel>)(object)new CompositeDuplexChannelAcceptor(this, (IChannelListener<IInputChannel>)this.InnerChannelListener);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.innerChannelFactory);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            this.innerChannelFactory.Open(timeoutHelper.RemainingTime());
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IChannelFactory))
            {
                return (T)(object)innerChannelFactory;
            }

            if (typeof(T) == typeof(ISecurityCapabilities) && !this.providesCorrelation)
            {
                return InternalDuplexBindingElement.GetSecurityCapabilities<T>(base.GetProperty<ISecurityCapabilities>());
            }

            T baseProperty = base.GetProperty<T>();
            if (baseProperty != null)
            {
                return baseProperty;
            }

            return this.innerChannelFactory.GetProperty<T>();
        }

        sealed class CompositeDuplexChannelAcceptor : LayeredChannelAcceptor<IDuplexChannel, IInputChannel>
        {
            public CompositeDuplexChannelAcceptor(InternalDuplexChannelListener listener, IChannelListener<IInputChannel> innerListener)
                : base(listener, innerListener)
            {
            }

            protected override IDuplexChannel OnAcceptChannel(IInputChannel innerChannel)
            {
                return new ServerCompositeDuplexChannel((InternalDuplexChannelListener)ChannelManager, innerChannel);
            }
        }

        sealed class ServerCompositeDuplexChannel : ChannelBase, IDuplexChannel
        {
            IInputChannel innerInputChannel;
            TimeSpan sendTimeout;

            public ServerCompositeDuplexChannel(InternalDuplexChannelListener listener, IInputChannel innerInputChannel)
                : base(listener)
            {
                this.innerInputChannel = innerInputChannel;
                this.sendTimeout = listener.DefaultSendTimeout;
            }

            InternalDuplexChannelListener Listener
            {
                get { return (InternalDuplexChannelListener)base.Manager; }
            }

            public EndpointAddress LocalAddress
            {
                get { return this.innerInputChannel.LocalAddress; }
            }

            public EndpointAddress RemoteAddress
            {
                get { return null; }
            }

            public Uri Via
            {
                get { return null; }
            }

            public Message Receive()
            {
                return this.Receive(this.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return InputChannel.HelpReceive(this, timeout);
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return InputChannel.HelpEndReceive(result);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginTryReceive(timeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendAsyncResult(this, message, timeout, callback, state);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return this.innerInputChannel.EndTryReceive(result, out message);
            }

            public void EndSend(IAsyncResult result)
            {
                SendAsyncResult.End(result);
            }

            protected override void OnAbort()
            {
                this.innerInputChannel.Abort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginClose(timeout, callback, state);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                this.innerInputChannel.EndClose(result);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                if (this.innerInputChannel.State == CommunicationState.Opened)
                    this.innerInputChannel.Close(timeout);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginOpen(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerInputChannel.EndOpen(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerInputChannel.Open(timeout);
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                return this.innerInputChannel.TryReceive(timeout, out message);
            }

            public void Send(Message message)
            {
                this.Send(message, this.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                IOutputChannel outputChannel = ValidateStateAndGetOutputChannel(message, timeoutHelper);
                try
                {
                    outputChannel.Send(message, timeoutHelper.RemainingTime());
                    outputChannel.Close(timeoutHelper.RemainingTime());
                }
                finally
                {
                    outputChannel.Abort();
                }
            }

            IOutputChannel ValidateStateAndGetOutputChannel(Message message, TimeoutHelper timeoutHelper)
            {
                ThrowIfDisposedOrNotOpen();
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                Uri to = message.Properties.Via;
                if (to == null)
                {
                    to = message.Headers.To;
                    if (to == null)
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MessageMustHaveViaOrToSetForSendingOnServerSideCompositeDuplexChannels)), message);
                    }
                    //Check for EndpointAddress.AnonymousUri is just redundant
                    else if (to.Equals(EndpointAddress.AnonymousUri) || to.Equals(message.Version.Addressing.AnonymousUri))
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MessageToCannotBeAddressedToAnonymousOnServerSideCompositeDuplexChannels, to)), message);
                    }
                }
                //Check for EndpointAddress.AnonymousUri is just redundant
                else if (to.Equals(EndpointAddress.AnonymousUri) || to.Equals(message.Version.Addressing.AnonymousUri))
                {
                    throw TraceUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.MessageViaCannotBeAddressedToAnonymousOnServerSideCompositeDuplexChannels, to)), message);
                }
                return this.Listener.GetOutputChannel(to, timeoutHelper);
            }

            class SendAsyncResult : AsyncResult
            {
                IOutputChannel outputChannel;
                static AsyncCallback sendCompleteCallback = Fx.ThunkCallback(new AsyncCallback(SendCompleteCallback));
                TimeoutHelper timeoutHelper;

                public SendAsyncResult(ServerCompositeDuplexChannel outer, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.outputChannel = outer.ValidateStateAndGetOutputChannel(message, timeoutHelper);

                    bool success = false;
                    try
                    {
                        IAsyncResult result = outputChannel.BeginSend(message, timeoutHelper.RemainingTime(), sendCompleteCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            CompleteSend(result);
                            this.Complete(true);
                        }
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                            this.outputChannel.Abort();
                    }
                }

                void CompleteSend(IAsyncResult result)
                {
                    try
                    {
                        outputChannel.EndSend(result);
                        outputChannel.Close();
                    }
                    finally
                    {
                        outputChannel.Abort();
                    }
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<SendAsyncResult>(result);
                }

                static void SendCompleteCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.CompleteSend(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    thisPtr.Complete(false, completionException);
                }
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return innerInputChannel.WaitForMessage(timeout);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return innerInputChannel.BeginWaitForMessage(timeout, callback, state);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return innerInputChannel.EndWaitForMessage(result);
            }
        }
    }
}
