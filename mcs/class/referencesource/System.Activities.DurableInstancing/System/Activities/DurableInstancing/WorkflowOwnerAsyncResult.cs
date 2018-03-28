//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Text;
    using System.Transactions;

    abstract class WorkflowOwnerAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        public WorkflowOwnerAsyncResult
            (
            InstancePersistenceContext context, 
            InstancePersistenceCommand command, 
            SqlWorkflowInstanceStore store,
            SqlWorkflowInstanceStoreLock storeLock,
            Transaction currentTransaction,
            TimeSpan timeout, 
            AsyncCallback callback, 
            object state
            ) :
            base(context, command, store, storeLock, currentTransaction, timeout, callback, state)
        {
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            base.StoreLock.TakeModificationLock();
        }

        protected override void OnCommandCompletion()
        {
            base.StoreLock.ReturnModificationLock();
        }
    }
}
