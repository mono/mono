//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Transactions;
    using SR = System.ServiceModel.SR;
    using System.Threading;

    sealed class MsmqInputSessionChannel : InputChannel, IInputSessionChannel
    {
        IInputSession session;
        Transaction associatedTx;
        ReceiveContext sessiongramReceiveContext;
        bool receiveContextEnabled;
        bool sessiongramDoomed;

        // count of messages that have been pulled out of the base queue but Complete has not been called on them
        int incompleteMessageCount;

        // count of messages that have been completed but the transaction has not been committed
        int uncommittedMessageCount;

        public MsmqInputSessionChannel(MsmqInputSessionChannelListener listener, Transaction associatedTx, ReceiveContext sessiongramReceiveContext)
            : base(listener, new EndpointAddress(listener.Uri))
        {
            this.session = new InputSession();
            this.incompleteMessageCount = 0;

            if (sessiongramReceiveContext == null)
            {
                this.receiveContextEnabled = false;

                // only enlist if we are running in a non-receive context mode
                this.associatedTx = associatedTx;
                this.associatedTx.EnlistVolatile(new TransactionEnlistment(this, this.associatedTx), EnlistmentOptions.None);
            }
            else
            {
                //ignore the ambient transaction if any
                this.receiveContextEnabled = true;
                this.sessiongramReceiveContext = sessiongramReceiveContext;
                this.sessiongramDoomed = false;
            }
        }

        public IInputSession Session
        {
            get { return this.session; }
        }

        int TotalPendingItems
        {
            get
            {
                return this.InternalPendingItems + this.incompleteMessageCount;
            }
        }

        void DetachTransaction(bool aborted)
        {
            // disassociate the session channel from the current transaction and enlistment
            this.associatedTx = null;
            if (aborted)
            {
                this.incompleteMessageCount += this.uncommittedMessageCount;
            }
            this.uncommittedMessageCount = 0;
        }

        void AbandonMessage(TimeSpan timeout)
        {
            ThrowIfFaulted();
            this.sessiongramDoomed = true;
        }

        void CompleteMessage(TimeSpan timeout)
        {
            ThrowIfFaulted();
            EnsureReceiveContextTransaction();

            // the message is now off to transaction land
            Interlocked.Increment(ref uncommittedMessageCount);
            Interlocked.Decrement(ref incompleteMessageCount);
        }

        public override Message Receive()
        {
            return this.Receive(this.DefaultReceiveTimeout);
        }

        public override Message Receive(TimeSpan timeout)
        {
            return InputChannel.HelpReceive(this, timeout);
        }

        public override IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.DefaultReceiveTimeout, callback, state);
        }

        public override IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InputChannel.HelpBeginReceive(this, timeout, callback, state);
        }

        public override bool TryReceive(TimeSpan timeout, out Message message)
        {
            ThrowIfFaulted();
            if (CommunicationState.Closed == this.State || CommunicationState.Closing == this.State)
            {
                message = null;
                return true;
            }

            // we don't look at the transaction in the receive if receive context is enabled
            if (!this.receiveContextEnabled)
            {
                VerifyTransaction();
            }

            bool receiveSuccessful = base.TryReceive(timeout, out message);

            if (receiveSuccessful && message != null && this.receiveContextEnabled)
            {
                message.Properties[ReceiveContext.Name] = new MsmqSessionReceiveContext(this);
                Interlocked.Increment(ref incompleteMessageCount);
            }

            return receiveSuccessful;
        }

        public override IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfFaulted();
            if (CommunicationState.Closed == this.State || CommunicationState.Closing == this.State)
            {
                return new CompletedAsyncResult<bool, Message>(true, null, callback, state);
            }
            // we don't look at the transaction in the receive if receive context is enabled
            if (!this.receiveContextEnabled)
            {
                VerifyTransaction();
            }
            return base.BeginTryReceive(timeout, callback, state);
        }

        public override bool EndTryReceive(IAsyncResult result, out Message message)
        {
            CompletedAsyncResult<bool, Message> completedResult = result as CompletedAsyncResult<bool, Message>;

            if (null != completedResult)
            {
                return CompletedAsyncResult<bool, Message>.End(result, out message);
            }
            else
            {
                bool receiveSuccessful = base.EndTryReceive(result, out message);
                if (receiveSuccessful && message != null && this.receiveContextEnabled)
                {
                    message.Properties[ReceiveContext.Name] = new MsmqSessionReceiveContext(this);
                    Interlocked.Increment(ref incompleteMessageCount);
                }

                return receiveSuccessful;
            }
        }

        public void FaultChannel()
        {
            this.Fault();
        }

        void OnCloseReceiveContext(bool isAborting)
        {
            if (isAborting)
            {
                // can't do much on Channel.Abort if the transaction had already committed
                if (this.associatedTx != null)
                {
                    // Channel.Abort called within the associated transaction
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqSessionChannelAbort)));
                    RollbackTransaction(e);
                }
                this.sessiongramReceiveContext.Abandon(TimeSpan.MaxValue);
            }
            else
            {
                if (this.TotalPendingItems > 0)
                {
                    // no need for rollback, it will happen automatically when this condition is hit in the Prepare() call
                    this.Fault();
                    this.sessiongramReceiveContext.Abandon(TimeSpan.MaxValue);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqSessionPrematureClose)));
                }
            }
        }

        void OnCloseTransactional(bool isAborting)
        {
            if (isAborting)
            {
                RollbackTransaction(null);
            }
            else
            {
                VerifyTransaction();
                if (this.InternalPendingItems > 0)
                {
                    RollbackTransaction(null);
                    this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqSessionMessagesNotConsumed)));
                }
            }
        }

        void OnCloseCore(bool isAborting)
        {
            if (this.receiveContextEnabled)
            {
                OnCloseReceiveContext(isAborting);
            }
            else
            {
                OnCloseTransactional(isAborting);
            }
        }

        protected override void OnAbort()
        {
            OnCloseCore(true);
            base.OnAbort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseCore(false);
            base.OnClose(timeout);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnCloseCore(false);
            return base.OnBeginClose(timeout, callback, state);
        }

        void RollbackTransaction(Exception exception)
        {
            try
            {
                if (TransactionStatus.Active == this.associatedTx.TransactionInformation.Status)
                    this.associatedTx.Rollback(exception);
            }
            catch (TransactionAbortedException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }
            catch (ObjectDisposedException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }
        }

        void EnsureReceiveContextTransaction()
        {
            // if this is the first time we are seeing this transaction in receivecontext enabled mode then enlist and 
            // associate the session channel with this transaction
            if (Transaction.Current == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqTransactionRequired)));
            }

            if (this.associatedTx == null)
            {
                this.associatedTx = Transaction.Current;
                this.associatedTx.EnlistVolatile(new ReceiveContextTransactionEnlistment(this, this.associatedTx, this.sessiongramReceiveContext),
                    EnlistmentOptions.EnlistDuringPrepareRequired);
            }
            else
            {
                if (this.associatedTx != Transaction.Current)
                {
                    RollbackTransaction(null);
                    this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqSameTransactionExpected)));
                }

                if (TransactionStatus.Active != Transaction.Current.TransactionInformation.Status)
                {
                    this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqTransactionNotActive)));
                }
            }
        }

        void VerifyTransaction()
        {
            if (this.InternalPendingItems > 0)
            {
                if (this.associatedTx != Transaction.Current)
                {
                    RollbackTransaction(null);
                    this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqSameTransactionExpected)));
                }

                if (TransactionStatus.Active != Transaction.Current.TransactionInformation.Status)
                {
                    RollbackTransaction(null);
                    this.Fault();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new InvalidOperationException(SR.GetString(SR.MsmqTransactionNotActive)));
                }
            }
        }

        class InputSession : IInputSession
        {
            string id = "uuid://session-gram/" + Guid.NewGuid().ToString();

            public string Id
            {
                get { return this.id; }
            }
        }

        class MsmqSessionReceiveContext : ReceiveContext
        {
            MsmqInputSessionChannel channel;

            public MsmqSessionReceiveContext(MsmqInputSessionChannel channel)
            {
                this.channel = channel;
            }

            protected override void OnAbandon(TimeSpan timeout)
            {
                this.channel.AbandonMessage(timeout);
            }

            protected override IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return SessionReceiveContextAsyncResult.CreateAbandon(this, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return SessionReceiveContextAsyncResult.CreateComplete(this, timeout, callback, state);
            }

            protected override void OnComplete(TimeSpan timeout)
            {
                this.channel.CompleteMessage(timeout);
            }

            protected override void OnEndAbandon(IAsyncResult result)
            {
                SessionReceiveContextAsyncResult.End(result);
            }

            protected override void OnEndComplete(IAsyncResult result)
            {
                SessionReceiveContextAsyncResult.End(result);
            }

            class SessionReceiveContextAsyncResult : AsyncResult
            {
                MsmqSessionReceiveContext receiveContext;
                Transaction completionTransaction;

                TimeoutHelper timeoutHelper;
                static Action<object> onComplete;
                static Action<object> onAbandon;

                SessionReceiveContextAsyncResult(MsmqSessionReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state, Action<object> target)
                    : base(callback, state)
                {
                    this.completionTransaction = Transaction.Current;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.receiveContext = receiveContext;
                    ActionItem.Schedule(target, this);
                }

                public static IAsyncResult CreateComplete(MsmqSessionReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state)
                {
                    if (onComplete == null)
                    {
                        onComplete = new Action<object>(OnComplete);
                    }
                    return new SessionReceiveContextAsyncResult(receiveContext, timeout, callback, state, onComplete);
                }

                public static IAsyncResult CreateAbandon(MsmqSessionReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state)
                {
                    if (onAbandon == null)
                    {
                        onAbandon = new Action<object>(OnAbandon);
                    }
                    return new SessionReceiveContextAsyncResult(receiveContext, timeout, callback, state, onAbandon);
                }

                static void OnComplete(object parameter)
                {
                    SessionReceiveContextAsyncResult result = parameter as SessionReceiveContextAsyncResult;
                    Transaction savedTransaction = Transaction.Current;
                    Transaction.Current = result.completionTransaction;

                    try
                    {
                        Exception completionException = null;
                        try
                        {
                            result.receiveContext.OnComplete(result.timeoutHelper.RemainingTime());
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            completionException = e;
                        }
                        result.Complete(false, completionException);
                    }
                    finally
                    {
                        Transaction.Current = savedTransaction;
                    }
                }

                static void OnAbandon(object parameter)
                {
                    SessionReceiveContextAsyncResult result = parameter as SessionReceiveContextAsyncResult;
                    Exception completionException = null;
                    try
                    {
                        result.receiveContext.OnAbandon(result.timeoutHelper.RemainingTime());
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;
                        completionException = e;
                    }
                    result.Complete(false, completionException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SessionReceiveContextAsyncResult>(result);
                }
            }
        }

        class ReceiveContextTransactionEnlistment : IEnlistmentNotification
        {
            MsmqInputSessionChannel channel;
            Transaction transaction;
            ReceiveContext sessiongramReceiveContext;

            public ReceiveContextTransactionEnlistment(MsmqInputSessionChannel channel, Transaction transaction, ReceiveContext receiveContext)
            {
                this.channel = channel;
                this.transaction = transaction;
                this.sessiongramReceiveContext = receiveContext;
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                // Abort if this happens before all messges are consumed
                // Note that we are not placing any restriction on the channel state
                if (this.channel.TotalPendingItems > 0 || this.channel.sessiongramDoomed)
                {
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqSessionChannelHasPendingItems)));
                    this.sessiongramReceiveContext.Abandon(TimeSpan.MaxValue);
                    preparingEnlistment.ForceRollback(e);
                    this.channel.Fault();
                }
                else
                {
                    Transaction savedTransaction = Transaction.Current;
                    // complete the sessiongram message within this transaction
                    try
                    {
                        Transaction.Current = this.transaction;

                        try
                        {
                            this.sessiongramReceiveContext.Complete(TimeSpan.MaxValue);
                            preparingEnlistment.Done();
                        }
                        catch (MsmqException msmqex)
                        {
                            preparingEnlistment.ForceRollback(msmqex);
                            this.channel.Fault();
                        }
                    }
                    finally
                    {
                        Transaction.Current = savedTransaction;
                    }
                }
            }

            public void Commit(Enlistment enlistment)
            {
                this.channel.DetachTransaction(false);
                enlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.channel.DetachTransaction(true);
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }
        }

        class TransactionEnlistment : IEnlistmentNotification
        {
            MsmqInputSessionChannel channel;
            Transaction transaction;

            public TransactionEnlistment(MsmqInputSessionChannel channel, Transaction transaction)
            {
                this.channel = channel;
                this.transaction = transaction;
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                // Abort if this happens before all messges are consumed
                if (this.channel.State == CommunicationState.Opened && this.channel.InternalPendingItems > 0)
                {
                    Exception e = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MsmqSessionChannelsMustBeClosed)));
                    preparingEnlistment.ForceRollback(e);
                    this.channel.Fault();
                }
                else
                {
                    preparingEnlistment.Done();
                }
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
                channel.Fault();
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }
        }
    }
}
