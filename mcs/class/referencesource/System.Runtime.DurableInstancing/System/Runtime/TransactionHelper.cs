//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime
{
    using System;
    using System.Transactions;

    static class TransactionHelper
    {
        public static void ThrowIfTransactionAbortedOrInDoubt(Transaction transaction)
        {
            if (transaction == null)
            {
                return;
            }

            if (transaction.TransactionInformation.Status == TransactionStatus.Aborted || transaction.TransactionInformation.Status == TransactionStatus.InDoubt)
            {
                //This will throw TransactionAbortedException/TransactionInDoubtException with corresponding inner exception if any
                using (TransactionScope scope = new TransactionScope(transaction))
                {
                    //empty
                }
            }
        }

        // If the transaction has aborted then we switch over to a new transaction
        // which we will immediately abort after setting Transaction.Current
        public static TransactionScope CreateTransactionScope(Transaction transaction)
        {
            try
            {
                return transaction == null ? null : new TransactionScope(transaction);
            }
            catch (TransactionAbortedException)
            {
                CommittableTransaction tempTransaction = new CommittableTransaction();
                try
                {
                    return new TransactionScope(tempTransaction.Clone());
                }
                finally
                {
                    tempTransaction.Rollback();
                }
            }
        }

        public static void CompleteTransactionScope(ref TransactionScope scope)
        {
            TransactionScope localScope = scope;
            if (localScope != null)
            {
                scope = null;
                try
                {
                    localScope.Complete();
                }
                finally
                {
                    localScope.Dispose();
                }
            }
        }
    }
}
