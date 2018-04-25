//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Transactions;
    using System.Runtime.Diagnostics;

    public abstract class ReceiveContext
    {
        public readonly static string Name = "ReceiveContext";
        ThreadNeutralSemaphore stateLock; // protects state that may be reverted
        bool contextFaulted;
        object thisLock;
        EventTraceActivity eventTraceActivity;

        protected ReceiveContext()
        {
            this.thisLock = new object();
            this.State = ReceiveContextState.Received;
            this.stateLock = new ThreadNeutralSemaphore(1);
        }

        public ReceiveContextState State
        {
            get;
            protected set;
        }

        protected object ThisLock
        {
            get { return thisLock; }
        }

        public event EventHandler Faulted;

        public static bool TryGet(Message message, out ReceiveContext property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            bool result = TryGet(message.Properties, out property);
            if (result && FxTrace.Trace.IsEnd2EndActivityTracingEnabled && property.eventTraceActivity == null)
            {
                property.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
            }

            return result;
        }

        public static bool TryGet(MessageProperties properties, out ReceiveContext property)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            property = null;
            object foundProperty;
            if (properties.TryGetValue(Name, out foundProperty))
            {
                property = (ReceiveContext)foundProperty;
                return true;
            }
            return false;
        }

        public virtual void Abandon(TimeSpan timeout)
        {
            Abandon(null, timeout);
        }

        public virtual void Abandon(Exception exception, TimeSpan timeout)
        {
            EnsureValidTimeout(timeout);
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WaitForStateLock(timeoutHelper.RemainingTime());

            try
            {
                if (PreAbandon())
                {
                    return;
                }
            }
            finally
            {
                // Abandon can never be reverted, release the state lock.
                this.ReleaseStateLock();
            }

            bool success = false;
            try
            {
                if (exception == null)
                {
                    OnAbandon(timeoutHelper.RemainingTime());
                }
                else
                {
                    if (TD.ReceiveContextAbandonWithExceptionIsEnabled())
                    {
                        TD.ReceiveContextAbandonWithException(this.eventTraceActivity, this.GetType().ToString(), exception.GetType().ToString());
                    }
                    OnAbandon(exception, timeoutHelper.RemainingTime());
                }
                lock (ThisLock)
                {
                    ThrowIfFaulted();
                    ThrowIfNotAbandoning();
                    this.State = ReceiveContextState.Abandoned;
                }
                success = true;
            }
            finally
            {
                if (!success)
                {
                    if (TD.ReceiveContextAbandonFailedIsEnabled())
                    {
                        TD.ReceiveContextAbandonFailed(this.eventTraceActivity, this.GetType().ToString());
                    }
                    Fault();
                }
            }

        }

        public virtual IAsyncResult BeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginAbandon(null, timeout, callback, state);
        }

        public virtual IAsyncResult BeginAbandon(Exception exception, TimeSpan timeout, AsyncCallback callback, object state)
        {
            EnsureValidTimeout(timeout);
            return new AbandonAsyncResult(this, exception, timeout, callback, state);
        }

        public virtual IAsyncResult BeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
        {
            EnsureValidTimeout(timeout);
            return new CompleteAsyncResult(this, timeout, callback, state);
        }

        public virtual void Complete(TimeSpan timeout)
        {
            EnsureValidTimeout(timeout);
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.WaitForStateLock(timeoutHelper.RemainingTime());
            bool success = false;

            try
            {
                PreComplete();
                success = true;
            }
            finally
            {
                // Case 1: State validation fails, release the lock.
                // Case 2: No trasaction, the state can never be reverted, release the lock.
                // Case 3: Transaction, keep the lock until we know the transaction outcome (OnTransactionStatusNotification).
                if (!success || Transaction.Current == null)
                {
                    this.ReleaseStateLock();
                }
            }

            success = false;
            try
            {
                OnComplete(timeoutHelper.RemainingTime());
                lock (ThisLock)
                {
                    ThrowIfFaulted();
                    ThrowIfNotCompleting();
                    this.State = ReceiveContextState.Completed;
                }
                success = true;
            }
            finally
            {
                if (!success)
                {
                    if (TD.ReceiveContextCompleteFailedIsEnabled())
                    {
                        TD.ReceiveContextCompleteFailed(this.eventTraceActivity, this.GetType().ToString());
                    }
                    Fault();
                }
            }
        }

        public virtual void EndAbandon(IAsyncResult result)
        {
            AbandonAsyncResult.End(result);
        }

        public virtual void EndComplete(IAsyncResult result)
        {
            CompleteAsyncResult.End(result);
        }

        void EnsureValidTimeout(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", SR.GetString(SR.SFxTimeoutOutOfRange0)));
            }

            if (TimeoutHelper.IsTooLarge(timeout))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("timeout", timeout, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
            }
        }

        protected internal virtual void Fault()
        {
            lock (ThisLock)
            {
                if (this.State == ReceiveContextState.Completed || this.State == ReceiveContextState.Abandoned || this.State == ReceiveContextState.Faulted)
                {
                    return;
                }
                this.State = ReceiveContextState.Faulted;
            }
            OnFaulted();
        }

        protected abstract void OnAbandon(TimeSpan timeout);
        protected virtual void OnAbandon(Exception exception, TimeSpan timeout)
        {
            // default implementation: delegate to non-exception overload, ignoring reason
            OnAbandon(timeout);
        }

        protected abstract IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state);
        protected virtual IAsyncResult OnBeginAbandon(Exception exception, TimeSpan timeout, AsyncCallback callback, object state)
        {
            // default implementation: delegate to non-exception overload, ignoring reason
            return OnBeginAbandon(timeout, callback, state);
        }

        protected abstract IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state);

        protected abstract void OnComplete(TimeSpan timeout);
        protected abstract void OnEndAbandon(IAsyncResult result);
        protected abstract void OnEndComplete(IAsyncResult result);

        protected virtual void OnFaulted()
        {
            lock (ThisLock)
            {
                if (this.contextFaulted)
                {
                    return;
                }
                this.contextFaulted = true;
            }

            if (TD.ReceiveContextFaultedIsEnabled())
            {
                TD.ReceiveContextFaulted(this.eventTraceActivity, this);
            }

            EventHandler handler = this.Faulted;

            if (handler != null)
            {
                try
                {
                    handler(this, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        void OnTransactionStatusNotification(TransactionStatus status)
        {
            lock (ThisLock)
            {
                if (status == TransactionStatus.Aborted)
                {
                    if (this.State == ReceiveContextState.Completing || this.State == ReceiveContextState.Completed)
                    {
                        this.State = ReceiveContextState.Received;
                    }
                }
            }

            if (status != TransactionStatus.Active)
            {
                this.ReleaseStateLock();
            }
        }

        bool PreAbandon()
        {
            bool alreadyAbandoned = false;
            lock (ThisLock)
            {
                if (this.State == ReceiveContextState.Abandoning || this.State == ReceiveContextState.Abandoned)
                {
                    alreadyAbandoned = true;
                }
                else
                {
                    ThrowIfFaulted();
                    ThrowIfNotReceived();
                    this.State = ReceiveContextState.Abandoning;
                }
            }
            return alreadyAbandoned;
        }

        void PreComplete()
        {
            lock (ThisLock)
            {
                ThrowIfFaulted();
                ThrowIfNotReceived();
                if (Transaction.Current != null)
                {
                    Transaction.Current.EnlistVolatile(new EnlistmentNotifications(this), EnlistmentOptions.None);
                }
                this.State = ReceiveContextState.Completing;
            }
        }

        void ReleaseStateLock()
        {
            this.stateLock.Exit();
        }

        void ThrowIfFaulted()
        {

            if (State == ReceiveContextState.Faulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new CommunicationException(SR.GetString(SR.ReceiveContextFaulted, this.GetType().ToString())));
            }
        }

        void ThrowIfNotAbandoning()
        {
            if (State != ReceiveContextState.Abandoning)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ReceiveContextInInvalidState, this.GetType().ToString(), this.State.ToString())));
            }
        }

        void ThrowIfNotCompleting()
        {
            if (State != ReceiveContextState.Completing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ReceiveContextInInvalidState, this.GetType().ToString(), this.State.ToString())));
            }
        }

        void ThrowIfNotReceived()
        {
            if (State != ReceiveContextState.Received)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ReceiveContextCannotBeUsed, this.GetType().ToString(), this.State.ToString())));
            }
        }

        void WaitForStateLock(TimeSpan timeout)
        {
            try
            {
                this.stateLock.Enter(timeout);
            }
            catch (TimeoutException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WrapStateException(exception));
            }
        }

        bool WaitForStateLockAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
        {
            return this.stateLock.EnterAsync(timeout, callback, state);
        }

        Exception WrapStateException(Exception exception)
        {
            return new InvalidOperationException(SR.GetString(SR.ReceiveContextInInvalidState, this.GetType().ToString(), this.State.ToString()), exception);
        }

        sealed class AbandonAsyncResult : WaitAndContinueOperationAsyncResult
        {
            Exception exception;
            static AsyncCompletion handleOperationComplete = new AsyncCompletion(HandleOperationComplete);

            public AbandonAsyncResult(ReceiveContext receiveContext, Exception exception, TimeSpan timeout, AsyncCallback callback, object state)
                : base(receiveContext, timeout, callback, state)
            {
                this.exception = exception;
                base.Begin();
            }

            // The main Abandon logic.
            protected override bool ContinueOperation()
            {
                try
                {
                    if (this.ReceiveContext.PreAbandon())
                    {
                        return true;
                    }
                }
                finally
                {
                    // Abandon can never be reverted, release the state lock.
                    this.ReceiveContext.ReleaseStateLock();
                }

                bool success = false;
                IAsyncResult result;
                try
                {
                    if (exception == null)
                    {
                        result = this.ReceiveContext.OnBeginAbandon(this.TimeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleOperationComplete), this);
                    }
                    else
                    {
                        if (TD.ReceiveContextAbandonWithExceptionIsEnabled())
                        {
                            TD.ReceiveContextAbandonWithException(this.ReceiveContext.eventTraceActivity, this.GetType().ToString(), exception.GetType().ToString());
                        }

                        result = this.ReceiveContext.OnBeginAbandon(exception, this.TimeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleOperationComplete), this);
                    }

                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        if (TD.ReceiveContextAbandonFailedIsEnabled())
                        {
                            TD.ReceiveContextAbandonFailed((this.ReceiveContext != null) ? this.ReceiveContext.eventTraceActivity : null, 
                                                            this.GetType().ToString());
                        }

                        this.ReceiveContext.Fault();
                    }
                }

                return SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AbandonAsyncResult>(result);
            }

            void EndAbandon(IAsyncResult result)
            {
                this.ReceiveContext.OnEndAbandon(result);
                lock (this.ReceiveContext.ThisLock)
                {
                    this.ReceiveContext.ThrowIfFaulted();
                    this.ReceiveContext.ThrowIfNotAbandoning();
                    this.ReceiveContext.State = ReceiveContextState.Abandoned;
                }
            }

            static bool HandleOperationComplete(IAsyncResult result)
            {
                bool success = false;
                AbandonAsyncResult thisPtr = (AbandonAsyncResult)result.AsyncState;

                try
                {
                    thisPtr.EndAbandon(result);
                    success = true;
                    return true;
                }
                finally
                {
                    if (!success)
                    {
                        if (TD.ReceiveContextAbandonFailedIsEnabled())
                        {
                            TD.ReceiveContextAbandonFailed(thisPtr.ReceiveContext.eventTraceActivity, thisPtr.GetType().ToString());
                        }

                        thisPtr.ReceiveContext.Fault();
                    }
                }
            }
        }

        sealed class CompleteAsyncResult : WaitAndContinueOperationAsyncResult
        {
            Transaction transaction;
            static AsyncCompletion handleOperationComplete = new AsyncCompletion(HandleOperationComplete);

            public CompleteAsyncResult(ReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state)
                : base(receiveContext, timeout, callback, state)
            {
                this.transaction = Transaction.Current;
                this.Begin();
            }

            protected override bool ContinueOperation()
            {
                IAsyncResult result;

                using (PrepareTransactionalCall(this.transaction))
                {
                    bool success = false;

                    try
                    {
                        this.ReceiveContext.PreComplete();
                        success = true;
                    }
                    finally
                    {
                        // Case 1: State validation fails, release the lock.
                        // Case 2: No trasaction, the state can never be reverted, release the lock.
                        // Case 3: Transaction, keep the lock until we know the transaction outcome (OnTransactionStatusNotification).
                        if (!success || this.transaction == null)
                        {
                            this.ReceiveContext.ReleaseStateLock();
                        }
                    }

                    success = false;

                    try
                    {
                        result = this.ReceiveContext.OnBeginComplete(this.TimeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleOperationComplete), this);
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            if (TD.ReceiveContextCompleteFailedIsEnabled())
                            {
                                TD.ReceiveContextCompleteFailed(this.ReceiveContext.eventTraceActivity, this.GetType().ToString());
                            }

                            this.ReceiveContext.Fault();
                        }
                    }
                }

                return SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CompleteAsyncResult>(result);
            }

            void EndComplete(IAsyncResult result)
            {
                this.ReceiveContext.OnEndComplete(result);
                lock (this.ReceiveContext.ThisLock)
                {
                    this.ReceiveContext.ThrowIfFaulted();
                    this.ReceiveContext.ThrowIfNotCompleting();
                    this.ReceiveContext.State = ReceiveContextState.Completed;
                }
            }

            static bool HandleOperationComplete(IAsyncResult result)
            {
                CompleteAsyncResult thisPtr = (CompleteAsyncResult)result.AsyncState;
                bool success = false;

                try
                {
                    thisPtr.EndComplete(result);
                    success = true;
                    return true;
                }
                finally
                {
                    if (!success)
                    {
                        if (TD.ReceiveContextCompleteFailedIsEnabled())
                        {
                            TD.ReceiveContextCompleteFailed(thisPtr.ReceiveContext.eventTraceActivity, thisPtr.GetType().ToString());
                        }

                        thisPtr.ReceiveContext.Fault();
                    }
                }
            }
        }

        class EnlistmentNotifications : IEnlistmentNotification
        {
            ReceiveContext context;

            public EnlistmentNotifications(ReceiveContext context)
            {
                this.context = context;
            }

            public void Commit(Enlistment enlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.Committed);
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.InDoubt);
                enlistment.Done();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.Active);
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.Aborted);
                enlistment.Done();
            }
        }

        abstract class WaitAndContinueOperationAsyncResult : TransactedAsyncResult
        {
            static FastAsyncCallback onWaitForStateLockComplete = new FastAsyncCallback(OnWaitForStateLockComplete);

            public WaitAndContinueOperationAsyncResult(ReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.ReceiveContext = receiveContext;
                this.TimeoutHelper = new TimeoutHelper(timeout);
            }

            protected ReceiveContext ReceiveContext
            {
                get;
                private set;
            }

            protected TimeoutHelper TimeoutHelper
            {
                get;
                private set;
            }

            protected void Begin()
            {
                if (!this.ReceiveContext.WaitForStateLockAsync(this.TimeoutHelper.RemainingTime(), onWaitForStateLockComplete, this))
                {
                    return;
                }

                if (this.ContinueOperation())
                {
                    this.Complete(true);
                }
            }

            protected abstract bool ContinueOperation();

            static void OnWaitForStateLockComplete(object state, Exception asyncException)
            {
                WaitAndContinueOperationAsyncResult thisPtr = (WaitAndContinueOperationAsyncResult)state;
                bool completeAsyncResult = true;
                Exception completeException = null;

                if (asyncException != null)
                {
                    if (asyncException is TimeoutException)
                    {
                        asyncException = thisPtr.ReceiveContext.WrapStateException(asyncException);
                    }

                    completeException = asyncException;
                }
                else
                {
                    try
                    {
                        completeAsyncResult = thisPtr.ContinueOperation();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeException = e;
                    }
                }

                if (completeAsyncResult)
                {
                    thisPtr.Complete(false, completeException);
                }
            }
        }
    }
}
