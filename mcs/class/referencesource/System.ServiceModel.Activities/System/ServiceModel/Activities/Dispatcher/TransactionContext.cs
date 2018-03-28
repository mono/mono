//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Runtime;
    using System.Transactions;

    //1) On Tx.Prepare
    //    Persist the instance.
    //          When Persist completes Tx.Prepared called.  
    //          When Persist fails Tx.ForceRollback called.
    //2) On Tx.Commit
    //     DurableInstance.OnTransactionCompleted().
    //3) On Tx.Abort
    //     DurableInstance.OnTransactionAborted()
    class TransactionContext : IEnlistmentNotification
    {
        static AsyncCallback handleEndPrepare = Fx.ThunkCallback(new AsyncCallback(HandleEndPrepare));
        Transaction currentTransaction;
        WorkflowServiceInstance durableInstance;

        public TransactionContext(WorkflowServiceInstance durableInstance, Transaction currentTransaction)
        {
            Fx.Assert(durableInstance != null, "Null DurableInstance passed to TransactionContext.");
            Fx.Assert(currentTransaction != null, "Null Transaction passed to TransactionContext.");

            this.currentTransaction = currentTransaction.Clone();
            this.durableInstance = durableInstance;
            this.currentTransaction.EnlistVolatile(this, EnlistmentOptions.EnlistDuringPrepareRequired);
        }

        public Transaction CurrentTransaction
        {
            get
            {
                return this.currentTransaction;
            }
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            enlistment.Done();
            this.durableInstance.TransactionCommitted();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
            Fx.Assert(this.currentTransaction.TransactionInformation.Status == TransactionStatus.InDoubt, "Transaction state should be InDoubt at this point");
            TransactionException exception = this.GetAbortedOrInDoubtTransactionException();

            Fx.Assert(exception != null, "Need a valid TransactionException at this point");
            this.durableInstance.OnTransactionAbortOrInDoubt(exception);
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            bool success = false;
            try
            {
                IAsyncResult result = new PrepareAsyncResult(this, TransactionContext.handleEndPrepare, preparingEnlistment);
                if (result.CompletedSynchronously)
                {
                    PrepareAsyncResult.End(result);
                    preparingEnlistment.Prepared();
                }
                success = true;
            }
            //we need to swollow the TransactionException as it could because another party aborting it
            catch (TransactionException) 
            {
            }
            finally
            {
                if (!success)
                {
                    preparingEnlistment.ForceRollback();
                }
            }
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            enlistment.Done();
            Fx.Assert(this.currentTransaction.TransactionInformation.Status == TransactionStatus.Aborted, "Transaction state should be Aborted at this point");
            TransactionException exception = this.GetAbortedOrInDoubtTransactionException();

            Fx.Assert(exception != null, "Need a valid TransactionException at this point");
            this.durableInstance.OnTransactionAbortOrInDoubt(exception);
        }

        TransactionException GetAbortedOrInDoubtTransactionException()
        {
            try
            {
                TransactionHelper.ThrowIfTransactionAbortedOrInDoubt(this.currentTransaction);
            }
            catch (TransactionException exception)
            {
                return exception;
            }
            return null;
        }

        static void HandleEndPrepare(IAsyncResult result)
        {
            PreparingEnlistment preparingEnlistment = (PreparingEnlistment)result.AsyncState;
            bool success = false;
            try
            {
                if (!result.CompletedSynchronously)
                {
                    PrepareAsyncResult.End(result);
                    preparingEnlistment.Prepared();
                }
                success = true;
            }
            //we need to swollow the TransactionException as it could because another party aborting it
            catch (TransactionException) 
            {
            }
            finally
            {
                if (!success)
                {
                    preparingEnlistment.ForceRollback();
                }
            }
        }

        class PrepareAsyncResult : TransactedAsyncResult
        {
            static readonly AsyncCompletion onEndPersist = new AsyncCompletion(OnEndPersist);

            readonly TransactionContext context;

            public PrepareAsyncResult(TransactionContext context, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.context = context;

                IAsyncResult result = null;
                using (PrepareTransactionalCall(this.context.currentTransaction))
                {
                    result = this.context.durableInstance.BeginPersist(TimeSpan.MaxValue, PrepareAsyncCompletion(PrepareAsyncResult.onEndPersist), this);
                }
                if (SyncContinue(result))
                {
                    Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PrepareAsyncResult>(result);
            }

            static bool OnEndPersist(IAsyncResult result)
            {
                PrepareAsyncResult thisPtr = (PrepareAsyncResult)result.AsyncState;
                thisPtr.context.durableInstance.EndPersist(result);
                thisPtr.context.durableInstance.OnTransactionPrepared();
                return true;
            }
        }
    }
}
