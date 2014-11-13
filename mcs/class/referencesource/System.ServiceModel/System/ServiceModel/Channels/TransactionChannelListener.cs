//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    sealed class TransactionChannelListener<TChannel> : DelegatingChannelListener<TChannel>, ITransactionChannelManager
        where TChannel : class, IChannel
    {
        TransactionFlowOption flowIssuedTokens;
        Dictionary<DirectionalAction, TransactionFlowOption> dictionary;
        SecurityStandardsManager standardsManager;
        TransactionProtocol transactionProtocol;

        public TransactionChannelListener(TransactionProtocol transactionProtocol, IDefaultCommunicationTimeouts timeouts, Dictionary<DirectionalAction, TransactionFlowOption> dictionary, IChannelListener<TChannel> innerListener)
            : base(timeouts, innerListener)
        {
            this.dictionary = dictionary;
            this.TransactionProtocol = transactionProtocol;
            this.Acceptor = new TransactionChannelAcceptor(this, innerListener);

            this.standardsManager = SecurityStandardsHelper.CreateStandardsManager(this.TransactionProtocol);
        }

        public TransactionProtocol TransactionProtocol
        {
            get
            {
                return this.transactionProtocol;
            }
            set
            {
                if (!TransactionProtocol.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentException(SR.GetString(SR.SFxBadTransactionProtocols)));
                this.transactionProtocol = value;
            }
        }

        public TransactionFlowOption FlowIssuedTokens
        {
            get { return this.flowIssuedTokens; }
            set { this.flowIssuedTokens = value; }
        }

        public SecurityStandardsManager StandardsManager
        {
            get { return this.standardsManager; }
            set { this.standardsManager = (value != null ? value : SecurityStandardsHelper.CreateStandardsManager(this.transactionProtocol)); }
        }

        public IDictionary<DirectionalAction, TransactionFlowOption> Dictionary
        {
            get { return this.dictionary; }
        }

        public TransactionFlowOption GetTransaction(MessageDirection direction, string action)
        {
            TransactionFlowOption txFlow;
            if (dictionary.TryGetValue(new DirectionalAction(direction, action), out txFlow))
                return txFlow;

            // Look for the wildcard action
            if (dictionary.TryGetValue(new DirectionalAction(direction, MessageHeaders.WildcardAction), out txFlow))
                return txFlow;

            return TransactionFlowOption.NotAllowed;
        }

        class TransactionChannelAcceptor : LayeredChannelAcceptor<TChannel, TChannel>
        {
            TransactionChannelListener<TChannel> listener;

            public TransactionChannelAcceptor(TransactionChannelListener<TChannel> listener, IChannelListener<TChannel> innerListener)
                : base(listener, innerListener)
            {
                this.listener = listener;
            }

            override protected TChannel OnAcceptChannel(TChannel innerChannel)
            {
                if (typeof(TChannel) == typeof(IInputSessionChannel))
                {
                    return (TChannel)(object)new TransactionInputSessionChannel(this.listener, (IInputSessionChannel)innerChannel);
                }
                if (typeof(TChannel) == typeof(IDuplexSessionChannel))
                {
                    return (TChannel)(object)new TransactionDuplexSessionChannel(this.listener, (IDuplexSessionChannel)innerChannel);
                }
                else if (typeof(TChannel) == typeof(IInputChannel))
                {
                    return (TChannel)(object)new TransactionInputChannel(this.listener, (IInputChannel)innerChannel);
                }
                else if (typeof(TChannel) == typeof(IReplyChannel))
                {
                    return (TChannel)(object)new TransactionReplyChannel(this.listener, (IReplyChannel)innerChannel);
                }
                else if (typeof(TChannel) == typeof(IReplySessionChannel))
                {
                    return (TChannel)(object)new TransactionReplySessionChannel(this.listener, (IReplySessionChannel)innerChannel);
                }
                else if (typeof(TChannel) == typeof(IDuplexChannel))
                {
                    return (TChannel)(object)new TransactionDuplexChannel(this.listener, (IDuplexChannel)innerChannel);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(listener.CreateChannelTypeNotSupportedException(typeof(TChannel)));
                }
            }
        }


        //==============================================================
        //                Transaction channel classes
        //==============================================================

        sealed class TransactionInputChannel : TransactionReceiveChannelGeneric<IInputChannel>
        {
            public TransactionInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel)
                : base(channelManager, innerChannel, MessageDirection.Input)
            {
            }
        }

        sealed class TransactionReplyChannel : TransactionReplyChannelGeneric<IReplyChannel>
        {
            public TransactionReplyChannel(ChannelManagerBase channelManager, IReplyChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        sealed class TransactionDuplexChannel : TransactionInputDuplexChannelGeneric<IDuplexChannel>
        {
            public TransactionDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }
        }

        sealed class TransactionInputSessionChannel : TransactionReceiveChannelGeneric<IInputSessionChannel>, IInputSessionChannel
        {
            public TransactionInputSessionChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel)
                : base(channelManager, innerChannel, MessageDirection.Input)
            {
            }

            public IInputSession Session { get { return InnerChannel.Session; } }
        }

        sealed class TransactionReplySessionChannel : TransactionReplyChannelGeneric<IReplySessionChannel>, IReplySessionChannel
        {
            public TransactionReplySessionChannel(ChannelManagerBase channelManager, IReplySessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IInputSession Session { get { return InnerChannel.Session; } }
        }

        sealed class TransactionDuplexSessionChannel : TransactionInputDuplexChannelGeneric<IDuplexSessionChannel>, IDuplexSessionChannel
        {
            public TransactionDuplexSessionChannel(ChannelManagerBase channelManager, IDuplexSessionChannel innerChannel)
                : base(channelManager, innerChannel)
            {
            }

            public IDuplexSession Session { get { return InnerChannel.Session; } }
        }
    }


    sealed class TransactionRequestContext : RequestContextBase
    {
        ITransactionChannel transactionChannel;
        RequestContext innerContext;

        public TransactionRequestContext(ITransactionChannel transactionChannel, ChannelBase channel, RequestContext innerContext,
            TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
            : base(innerContext.RequestMessage, defaultCloseTimeout, defaultSendTimeout)
        {
            this.transactionChannel = transactionChannel;
            this.innerContext = innerContext;
        }

        protected override void OnAbort()
        {
            if (this.innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }

            this.innerContext.Abort();
        }

        protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.innerContext == null)
            {
                throw TraceUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName), message);
            }

            if (message != null)
            {
                this.transactionChannel.WriteIssuedTokens(message, MessageDirection.Output);
            }
            return this.innerContext.BeginReply(message, timeout, callback, state);
        }


        protected override void OnClose(TimeSpan timeout)
        {
            if (this.innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }

            this.innerContext.Close(timeout);
        }

        protected override void OnEndReply(IAsyncResult result)
        {
            if (this.innerContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }

            this.innerContext.EndReply(result);
        }

        protected override void OnReply(Message message, TimeSpan timeout)
        {
            if (this.innerContext == null)
            {
                throw TraceUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName), message);
            }

            if (message != null)
            {
                this.transactionChannel.WriteIssuedTokens(message, MessageDirection.Output);
            }
            this.innerContext.Reply(message, timeout);
        }
    }



    //==============================================================
    //                Transaction channel base generic classes
    //==============================================================

    class TransactionReceiveChannelGeneric<TChannel> : TransactionChannel<TChannel>, IInputChannel
        where TChannel : class, IInputChannel
    {
        MessageDirection receiveMessageDirection;

        public TransactionReceiveChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel, MessageDirection direction)
            : base(channelManager, innerChannel)
        {
            this.receiveMessageDirection = direction;
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return InnerChannel.LocalAddress;
            }
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
            return InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public virtual bool EndTryReceive(IAsyncResult asyncResult, out Message message)
        {
            if (!InnerChannel.EndTryReceive(asyncResult, out message))
            {
                return false;
            }

            if (message != null)
            {
                ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            }

            return true;
        }

        public virtual bool TryReceive(TimeSpan timeout, out Message message)
        {
            if (!InnerChannel.TryReceive(timeout, out message))
            {
                return false;
            }

            if (message != null)
            {
                ReadTransactionDataFromMessage(message, this.receiveMessageDirection);
            }

            return true;
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


    //-------------------------------------------------------------
    class TransactionReplyChannelGeneric<TChannel> : TransactionChannel<TChannel>, IReplyChannel
        where TChannel : class, IReplyChannel
    {

        public TransactionReplyChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel)
        {
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return InnerChannel.LocalAddress;
            }
        }

        public RequestContext ReceiveRequest()
        {
            return this.ReceiveRequest(this.DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            return ReplyChannel.HelpReceiveRequest(this, timeout);
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.BeginReceiveRequest(this.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return ReplyChannel.HelpEndReceiveRequest(result);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ReceiveTimeoutAsyncResult result = new ReceiveTimeoutAsyncResult(timeout, callback, state);
            result.InnerResult = this.InnerChannel.BeginTryReceiveRequest(timeout, result.InnerCallback, result.InnerState);
            return result;
        }

        RequestContext FinishReceiveRequest(RequestContext innerContext, TimeSpan timeout)
        {
            if (innerContext == null)
                return null;

            try
            {
                this.ReadTransactionDataFromMessage(innerContext.RequestMessage, MessageDirection.Input);
            }
            catch (FaultException fault)
            {
                string faultAction = fault.Action ?? innerContext.RequestMessage.Version.Addressing.DefaultFaultAction;
                Message reply = Message.CreateMessage(innerContext.RequestMessage.Version, fault.CreateMessageFault(), faultAction);
                try
                {
                    innerContext.Reply(reply, timeout);
                }
                finally
                {
                    reply.Close();
                }
                throw;
            }

            return new TransactionRequestContext(this, this, innerContext, this.DefaultCloseTimeout, this.DefaultSendTimeout);
        }


        public bool EndTryReceiveRequest(IAsyncResult asyncResult, out RequestContext requestContext)
        {
            if (asyncResult == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("asyncResult");

            ReceiveTimeoutAsyncResult result = asyncResult as ReceiveTimeoutAsyncResult;
            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.AsyncEndCalledWithAnIAsyncResult)));

            RequestContext innerContext;
            if (InnerChannel.EndTryReceiveRequest(result.InnerResult, out innerContext))
            {
                requestContext = FinishReceiveRequest(innerContext, result.TimeoutHelper.RemainingTime());
                return true;
            }
            else
            {
                requestContext = null;
                return false;
            }
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext requestContext)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            RequestContext innerContext;
            if (InnerChannel.TryReceiveRequest(timeoutHelper.RemainingTime(), out innerContext))
            {
                requestContext = FinishReceiveRequest(innerContext, timeoutHelper.RemainingTime());
                return true;
            }
            else
            {
                requestContext = null;
                return false;
            }
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return InnerChannel.WaitForRequest(timeout);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerChannel.BeginWaitForRequest(timeout, callback, state);
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return InnerChannel.EndWaitForRequest(result);
        }
    }


    //-------------------------------------------------------------
    class TransactionInputDuplexChannelGeneric<TChannel> : TransactionDuplexChannelGeneric<TChannel>
        where TChannel : class, IDuplexChannel
    {
        public TransactionInputDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel)
            : base(channelManager, innerChannel, MessageDirection.Input)
        {
        }
    }


    //-------------------------------------------------------------
    class TransactionDuplexChannelGeneric<TChannel> : TransactionReceiveChannelGeneric<TChannel>, IDuplexChannel
        where TChannel : class, IDuplexChannel
    {
        MessageDirection sendMessageDirection;

        public TransactionDuplexChannelGeneric(ChannelManagerBase channelManager, TChannel innerChannel, MessageDirection direction)
            : base(channelManager, innerChannel, direction)
        {
            if (direction == MessageDirection.Input)
            {
                this.sendMessageDirection = MessageDirection.Output;
            }
            else
            {
                this.sendMessageDirection = MessageDirection.Input;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return InnerChannel.RemoteAddress;
            }
        }

        public Uri Via
        {
            get
            {
                return InnerChannel.Via;
            }
        }

        public override void ReadTransactionDataFromMessage(Message message, MessageDirection direction)
        {
            try
            {
                base.ReadTransactionDataFromMessage(message, direction);
            }
            catch (FaultException fault)
            {
                Message reply = Message.CreateMessage(message.Version, fault.CreateMessageFault(), fault.Action);

                System.ServiceModel.Channels.RequestReplyCorrelator.AddressReply(reply, message);
                System.ServiceModel.Channels.RequestReplyCorrelator.PrepareReply(reply, message.Headers.MessageId);

                try
                {
                    this.Send(reply);
                }
                finally
                {
                    reply.Close();
                }

                throw;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.DefaultSendTimeout, callback, state);
        }

        public virtual IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback asyncCallback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, sendMessageDirection);
            return InnerChannel.BeginSend(message, timeoutHelper.RemainingTime(), asyncCallback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            InnerChannel.EndSend(result);
        }

        public void Send(Message message)
        {
            this.Send(message, this.DefaultSendTimeout);
        }

        public virtual void Send(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            WriteTransactionDataToMessage(message, sendMessageDirection);
            InnerChannel.Send(message, timeoutHelper.RemainingTime());
        }
    }


    //==============================================================
    //                async helper classes
    //==============================================================

    class ReceiveTimeoutAsyncResult : AsyncResult
    {
        TimeoutHelper timeoutHelper;
        IAsyncResult innerResult;
        static AsyncCallback innerCallback = Fx.ThunkCallback(new AsyncCallback(Callback));

        internal ReceiveTimeoutAsyncResult(TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        internal TimeoutHelper TimeoutHelper
        {
            get { return this.timeoutHelper; }
        }

        internal AsyncCallback InnerCallback
        {
            get
            {
                if (ReceiveTimeoutAsyncResult.innerCallback == null)
                    ReceiveTimeoutAsyncResult.innerCallback = Fx.ThunkCallback(new AsyncCallback(Callback));
                return ReceiveTimeoutAsyncResult.innerCallback;
            }
        }

        internal IAsyncResult InnerResult
        {
            get
            {
                if (!(this.innerResult != null))
                {
                    // tx processing requires failfast when state is inconsistent
                    DiagnosticUtility.FailFast("ReceiveTimeoutAsyncResult.InnerResult: (this.innerResult != null)");
                }
                return this.innerResult;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

                if (this.innerResult == null)
                {
                    this.innerResult = value;
                }
                else if (this.innerResult != value)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxAsyncResultsDontMatch0)));
                }
            }
        }

        internal object InnerState
        {
            get { return this; }
        }

        static void Callback(IAsyncResult result)
        {
            if (result == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");

            ReceiveTimeoutAsyncResult outerResult = (ReceiveTimeoutAsyncResult)result.AsyncState;

            outerResult.InnerResult = result;
            outerResult.Complete(result.CompletedSynchronously);
        }
    }


}
