//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Transactions;
    
    [Fx.Tag.XamlVisible(false)]
    public sealed class NativeActivityTransactionContext : NativeActivityContext
    {
        ActivityExecutor executor;
        RuntimeTransactionHandle transactionHandle;

        internal NativeActivityTransactionContext(ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarks, RuntimeTransactionHandle handle)
            : base(instance, executor, bookmarks)
        {
            this.executor = executor;
            this.transactionHandle = handle;
        }

        public void SetRuntimeTransaction(Transaction transaction)
        {
            ThrowIfDisposed();

            if (transaction == null)
            {
                throw FxTrace.Exception.ArgumentNull("transaction");
            }

            this.executor.SetTransaction(this.transactionHandle, transaction, transactionHandle.Owner, this.CurrentInstance);
        }
    }
}
