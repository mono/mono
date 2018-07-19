//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    sealed class LoadWorkflowByKeyAsyncResult : LoadWorkflowAsyncResult
    {
        public LoadWorkflowByKeyAsyncResult
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

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            LoadWorkflowByInstanceKeyCommand keyLoadCommand = base.InstancePersistenceCommand as LoadWorkflowByInstanceKeyCommand;
            LoadType loadType = keyLoadCommand.AcceptUninitializedInstance ? LoadType.LoadOrCreateByKey : LoadType.LoadByKey;
            Guid key = keyLoadCommand.LookupInstanceKey;
            List<CorrelationKey> keysToAssociate = CorrelationKey.BuildKeyList(keyLoadCommand.InstanceKeysToAssociate, base.Store.InstanceEncodingOption);
            Guid instanceId = keyLoadCommand.AssociateInstanceKeyToInstanceId;

            GenerateLoadSqlCommand(command, loadType, key, instanceId, keysToAssociate);
        }
    }
}
