//------------------------------------------------------------------------------
// <copyright file="WorkflowTransactionService.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


#region Using directives

using System;
using System.Transactions;

#endregion

namespace System.Workflow.Runtime.Hosting
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowCommitWorkBatchService : WorkflowRuntimeService
    {
        public delegate void CommitWorkBatchCallback();

        virtual internal protected void CommitWorkBatch(CommitWorkBatchCallback commitWorkBatchCallback)
        {
            Transaction tx = null;
            if (null == Transaction.Current)
                tx = new CommittableTransaction();
            else
                tx = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);

            try
            {
                using (TransactionScope ts = new TransactionScope(tx))
                {
                    commitWorkBatchCallback();
                    ts.Complete();
                }

                CommittableTransaction committableTransaction = tx as CommittableTransaction;
                if (committableTransaction != null)
                    committableTransaction.Commit();

                DependentTransaction dependentTransaction = tx as DependentTransaction;
                if (dependentTransaction != null)
                    dependentTransaction.Complete();
            }
            catch (Exception e)
            {
                tx.Rollback(e);
                throw;
            }
            finally
            {
                if (tx != null)
                {
                    tx.Dispose();
                }
            }
        }
    }
}

