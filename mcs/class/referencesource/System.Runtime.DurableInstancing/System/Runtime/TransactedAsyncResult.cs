//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Transactions;

    // AsyncResult starts acquired; Complete releases.
    [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.ManualResetEvent, SupportsAsync = true, ReleaseMethod = "Complete")]
    abstract class TransactedAsyncResult : AsyncResult
    {
        IAsyncResult deferredTransactionalResult;
        TransactionSignalScope transactionContext;

        protected TransactedAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            SetBeforePrepareAsyncCompletionAction(BeforePrepareAsyncCompletion);
            SetCheckSyncValidationFunc(CheckSyncValidation);
        }

        protected override bool OnContinueAsyncCompletion(IAsyncResult result)
        {
            if (this.transactionContext != null && !this.transactionContext.Signal(result))
            {
                // The TransactionScope isn't cleaned up yet and can't be done on this thread.  Must defer
                // the callback (which is likely to attempt to commit the transaction) until later.
                return false;
            }

            this.transactionContext = null;
            return true;
        }

        bool CheckSyncValidation(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                // Once we pass the check, we know that we own forward progress, so transactionContext is correct. Verify its state.
                if (this.transactionContext != null)
                {
                    if (this.transactionContext.State != TransactionSignalState.Completed)
                    {
                        ThrowInvalidAsyncResult("Check/SyncContinue cannot be called from within the PrepareTransactionalCall using block.");
                    }
                    else if (this.transactionContext.IsSignalled)
                    {
                        // This is most likely to happen when result.CompletedSynchronously registers differently here and in the callback, which
                        // is the fault of 'result'.
                        ThrowInvalidAsyncResult(result);
                    }
                }
            }
            else if (object.ReferenceEquals(result, this.deferredTransactionalResult))
            {
                // The transactionContext may not be current if forward progress has been made via the callback. Instead,
                // use deferredTransactionalResult to see if we are supposed to execute a post-transaction callback.
                //
                // Once we pass the check, we know that we own forward progress, so transactionContext is correct. Verify its state.
                if (this.transactionContext == null || !this.transactionContext.IsSignalled)
                {
                    ThrowInvalidAsyncResult(result);
                }
                this.deferredTransactionalResult = null;
            }
            else
            {
                return false;
            }

            this.transactionContext = null;
            return true;
        }

        void BeforePrepareAsyncCompletion()
        {
            if (this.transactionContext != null)
            {
                // It might be an old, leftover one, if an exception was thrown within the last using (PrepareTransactionalCall()) block.
                if (this.transactionContext.IsPotentiallyAbandoned)
                {
                    this.transactionContext = null;
                }
                else
                {
                    this.transactionContext.Prepared();
                }
            }
        }

        protected IDisposable PrepareTransactionalCall(Transaction transaction)
        {
            if (this.transactionContext != null && !this.transactionContext.IsPotentiallyAbandoned)
            {
                ThrowInvalidAsyncResult("PrepareTransactionalCall should only be called as the object of non-nested using statements. If the Begin succeeds, Check/SyncContinue must be called before another PrepareTransactionalCall.");
            }

            return this.transactionContext = transaction == null ? null : new TransactionSignalScope(this, transaction);
        }

        enum TransactionSignalState
        {
            Ready = 0,
            Prepared,
            Completed,
            Abandoned,
        }

        class TransactionSignalScope : SignalGate<IAsyncResult>, IDisposable
        {
            TransactionScope transactionScope;
            TransactedAsyncResult parent;

            public TransactionSignalScope(TransactedAsyncResult result, Transaction transaction)
            {
                Fx.Assert(transaction != null, "Null Transaction provided to AsyncResult.TransactionSignalScope.");
                this.parent = result;
                this.transactionScope = TransactionHelper.CreateTransactionScope(transaction);
            }

            public TransactionSignalState State { get; private set; }

            public bool IsPotentiallyAbandoned
            {
                get
                {
                    return State == TransactionSignalState.Abandoned || (State == TransactionSignalState.Completed && !IsSignalled);
                }
            }

            public void Prepared()
            {
                if (State != TransactionSignalState.Ready)
                {
                    AsyncResult.ThrowInvalidAsyncResult("PrepareAsyncCompletion should only be called once per PrepareTransactionalCall.");
                }
                State = TransactionSignalState.Prepared;
            }

            void IDisposable.Dispose()
            {
                if (State == TransactionSignalState.Ready)
                {
                    State = TransactionSignalState.Abandoned;
                }
                else if (State == TransactionSignalState.Prepared)
                {
                    State = TransactionSignalState.Completed;
                }
                else
                {
                    AsyncResult.ThrowInvalidAsyncResult("PrepareTransactionalCall should only be called in a using. Dispose called multiple times.");
                }

                try
                {
                    TransactionHelper.CompleteTransactionScope(ref this.transactionScope);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    // Complete and Dispose are not expected to throw.  If they do it can mess up the AsyncResult state machine.
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.AsyncTransactionException));
                }

                // This will release the callback to run, or tell us that we need to defer the callback to Check/SyncContinue.
                //
                // It's possible to avoid this Interlocked when CompletedSynchronously is true, but we have no way of knowing that
                // from here, and adding a way would add complexity to the AsyncResult transactional calling pattern. This
                // unnecessary Interlocked only happens when: PrepareTransactionalCall is called with a non-null transaction,
                // PrepareAsyncCompletion is reached, and the operation completes synchronously or with an exception.
                IAsyncResult result;
                if (State == TransactionSignalState.Completed && Unlock(out result))
                {
                    if (this.parent.deferredTransactionalResult != null)
                    {
                        AsyncResult.ThrowInvalidAsyncResult(this.parent.deferredTransactionalResult);
                    }
                    this.parent.deferredTransactionalResult = result;
                }
            }
        }
    }
}
