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

    sealed class UnlockInstanceAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        static string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[UnlockInstance]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        public UnlockInstanceAsyncResult
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
            UnlockInstanceCommand unlockCommand = (UnlockInstanceCommand)(base.InstancePersistenceCommand);

            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@instanceId", SqlDbType = SqlDbType.UniqueIdentifier, Value = unlockCommand.InstanceId });
            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = unlockCommand.SurrogateOwnerId });
            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@handleInstanceVersion", SqlDbType = SqlDbType.BigInt, Value = unlockCommand.InstanceVersion });
        }

        protected override string GetSqlCommandText()
        {
            return UnlockInstanceAsyncResult.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            return StoreUtilities.CheckRemainingResultSetForErrors(base.InstancePersistenceCommand.Name, reader);
        }
    }
}
