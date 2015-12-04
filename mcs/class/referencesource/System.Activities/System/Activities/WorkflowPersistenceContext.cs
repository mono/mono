//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Collections.Generic;
    using System.Transactions;
    using System.Runtime;

    class WorkflowPersistenceContext
    {
        CommittableTransaction contextOwnedTransaction;
        Transaction clonedTransaction;        

        public WorkflowPersistenceContext(bool transactionRequired, TimeSpan transactionTimeout)
            : this(transactionRequired, CloneAmbientTransaction(), transactionTimeout)
        {
        }

        public WorkflowPersistenceContext(bool transactionRequired, Transaction transactionToUse, TimeSpan transactionTimeout)
        {
            if (transactionToUse != null)
            {
                this.clonedTransaction = transactionToUse;
            }
            else if (transactionRequired)
            {
                this.contextOwnedTransaction = new CommittableTransaction(transactionTimeout);
                // Clone it so that we don't pass a CommittableTransaction to the participants
                this.clonedTransaction = this.contextOwnedTransaction.Clone();
            }
        }

        public Transaction PublicTransaction
        {
            get
            {
                return this.clonedTransaction;
            }
        }       

        public void Abort()
        {
            if (this.contextOwnedTransaction != null)
            {
                try
                {
                    this.contextOwnedTransaction.Rollback();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    // ---- these exceptions as we are already on the error path
                }
            }
        }

        public void Complete()
        {            
            if (this.contextOwnedTransaction != null)
            {
                this.contextOwnedTransaction.Commit();
            }
        }        

        // Returns true if end needs to be called
        // Note: this is side effecting even if it returns false
        public bool TryBeginComplete(AsyncCallback callback, object state, out IAsyncResult result)
        {
            // In the interest of allocating less objects we don't implement
            // the full async pattern here.  Instead, we've flattened it to
            // do the [....] part and then optionally delegate down to the inner
            // BeginCommit.            

            if (this.contextOwnedTransaction != null)
            {
                result = this.contextOwnedTransaction.BeginCommit(callback, state);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public void EndComplete(IAsyncResult result)
        {
            Fx.Assert(this.contextOwnedTransaction != null, "We must have a contextOwnedTransaction if we are calling End");

            this.contextOwnedTransaction.EndCommit(result);
        }

        // We might as well clone the ambient transaction so that PersistenceParticipants
        // can't cast to a CommittableTransaction.
        static Transaction CloneAmbientTransaction()
        {
            Transaction ambientTransaction = Transaction.Current;
            return ambientTransaction == null ? null : ambientTransaction.Clone();
        }
    }
}
