//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime.DurableInstancing;
    using System.Transactions;

    sealed class DeleteWorkflowOwnerAsyncResult : WorkflowOwnerAsyncResult
    {
        static string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[DeleteLockOwner]", SqlWorkflowInstanceStoreConstants.DefaultSchema);
        long surrogateLockOwnerId;

        public DeleteWorkflowOwnerAsyncResult
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
            base.GenerateSqlCommand(sqlCommand);
            this.surrogateLockOwnerId = base.StoreLock.SurrogateLockOwnerId;
            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = this.surrogateLockOwnerId });
        }

        protected override string GetSqlCommandText()
        {
            return DeleteWorkflowOwnerAsyncResult.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = null;

            exception = StoreUtilities.CheckRemainingResultSetForErrors(base.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                base.InstancePersistenceContext.InstanceHandle.Free();
                base.StoreLock.MarkInstanceOwnerLost(this.surrogateLockOwnerId, true);
            }

            return exception;
        }
    }
}
