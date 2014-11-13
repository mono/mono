//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Runtime;
    using System.Transactions;
    using System.Threading;

    sealed class TransactionWaitAsyncResult : AsyncResult
    {
        static Action<object> timerCallback;

        DependentTransaction dependentTransaction;
        IOThreadTimer timer;

        [Fx.Tag.SynchronizationObject(Blocking = false)]
        object thisLock;

        internal TransactionWaitAsyncResult(Transaction transaction, PersistenceContext persistenceContext, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            bool completeSelf = false;
            TransactionException exception = null;
            this.PersistenceContext = persistenceContext;
            this.thisLock = new object();

            if (null != transaction)
            {
                // We want an "blocking" dependent transaction because we want to ensure the transaction
                // does not commit successfully while we are still waiting in the queue for the PC transaction
                // lock.
                this.dependentTransaction = transaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
            }
            else
            {
                this.dependentTransaction = null;
            }

            // Put a lock around this and Complete() in case the transaction we are queueing up behind
            // finishes and we end up calling Complete() before we actually finish constructing this
            // object by creating the DependentClone and setting up the IOThreadTimer.
            lock (ThisLock)
            {
                if (persistenceContext.QueueForTransactionLock(transaction, this))
                {
                    // If we were given a transaction in our constructor, we need to 
                    // create a volatile enlistment on it and complete the
                    // dependent clone that we created. This will allow the transaction to commit
                    // successfully when the time comes.
                    if (null != transaction)
                    {
                        // We are not going async, so we need to complete our dependent clone now.
                        this.dependentTransaction.Complete();

                        exception = this.CreateVolatileEnlistment(transaction);
                    }
                    completeSelf = true;
                }
                else
                {
                    // If the timeout value is not TimeSpan.MaxValue, start a timer.
                    if (timeout != TimeSpan.MaxValue)
                    {
                        this.timer = new IOThreadTimer(TimeoutCallbackAction, this, true);
                        this.timer.Set(timeout);
                    }
                }
            }

            // We didn't want to call Complete while holding the lock.
            if (completeSelf)
            {
                base.Complete(true, exception);
            }
        }

        internal PersistenceContext PersistenceContext { get; set; }

        internal Transaction Transaction
        {
            get
            {
                return this.dependentTransaction;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal static Action<object> TimeoutCallbackAction
        {
            get
            {
                if (timerCallback == null)
                {
                    timerCallback = new Action<object>(TimeoutCallback);
                }
                return timerCallback;
            }
        }

        // Returns true if this TransactionWaitAsyncResult was completed and has NOT timed out.
        // Returns false if this TransactionWaitAsyncResult has timed out.
        internal bool Complete()
        {
            Exception exception = null;

            // Lock to prevent completion while we are still in the process of constructing this object.
            lock (ThisLock)
            {
                // If we have a timer, but it has already expired, return false.
                if ((this.timer != null) && (!this.timer.Cancel()))
                {
                    return false;
                }

                // If we have a dependent transaction, complete it now.
                if (this.dependentTransaction != null)
                {
                    // If we were given a transaction in our constructor, we need to 
                    // create a volatile enlistment on it and complete the
                    // dependent clone that we created. This will allow the transaction to commit
                    // successfully when the time comes.
                    exception = this.CreateVolatileEnlistment(this.dependentTransaction);
                    this.dependentTransaction.Complete();
                }
            }

            // Indicate that we are complete.
            Complete(false, exception);

            return true;
        }

        TransactionException CreateVolatileEnlistment(Transaction transactionToEnlist)
        {
            TransactionException result = null;
            PersistenceContextEnlistment enlistment = null;
            int key = transactionToEnlist.GetHashCode();
            lock (PersistenceContext.Enlistments)
            {
                try
                {
                    if (!PersistenceContext.Enlistments.TryGetValue(key, out enlistment))
                    {
                        enlistment = new PersistenceContextEnlistment(this.PersistenceContext, transactionToEnlist);
                        transactionToEnlist.EnlistVolatile(enlistment, EnlistmentOptions.None);
                        // We don't save of the Enlistment object returned from EnlistVolatile. We don't need
                        // it here. When our PersistenceContextEnlistment object gets notified on Prepare,
                        // Commit, Rollback, or InDoubt, it is provided with the Enlistment object.
                        PersistenceContext.Enlistments.Add(key, enlistment);
                    }
                    else
                    {
                        enlistment.AddToEnlistment(this.PersistenceContext);
                    }
                }
                catch (TransactionException txException)
                {
                    result = txException;

                    // We own the lock but failed to create enlistment.  Manually wake up the next waiter.
                    // We only handle TransactionException, in case of other exception that failed to create enlistment,
                    // It will fallback to Timeout.  This is safe to avoid multiple waiters owning same lock.
                    this.PersistenceContext.ScheduleNextTransactionWaiter();
                }
            }
            return result;
        }

        static void TimeoutCallback(object state)
        {
            TransactionWaitAsyncResult thisPtr = (TransactionWaitAsyncResult)state;
            Fx.Assert(null != thisPtr, "TransactionWaitAsyncResult.TimeoutCallback called with an object that is not a TransactionWaitAsyncResult.");

            // As a general policy, we are not going to rollback the transaction because of this timeout. Instead, we are letting
            // the caller make the decision to rollback or not based on exception we are throwing. It could be that they could
            // tolerate the timeout and try something else and still commit the transaction.
            if (thisPtr.dependentTransaction != null)
            {
                thisPtr.dependentTransaction.Complete();
            }

            thisPtr.Complete(false, new TimeoutException(SR.TransactionPersistenceTimeout));
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<TransactionWaitAsyncResult>(result);
        }
    }
}
