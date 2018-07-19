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
    using System.Xml.Linq;
    using System.Runtime;

    class DetectActivatableWorkflowsAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[GetActivatableWorkflowsActivationParameters]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        public DetectActivatableWorkflowsAsyncResult
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

        protected override string ConnectionString
        {
            get
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(base.Store.CachedConnectionString);
                builder.ApplicationName = SqlWorkflowInstanceStore.CommonConnectionPoolName;
                return builder.ToString();
            }
        }

        protected override void GenerateSqlCommand(SqlCommand sqlCommand)
        {
            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@machineName", SqlDbType = SqlDbType.NVarChar, Value = SqlWorkflowInstanceStoreConstants.MachineName });
        }

        protected override string GetSqlCommandText()
        {
            return DetectActivatableWorkflowsAsyncResult.commandText;
        }

        protected override CommandType GetSqlCommandType()
        {
            return CommandType.StoredProcedure;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if (exception == null)
            {
                bool signalEvent = false;
                reader.NextResult();
                signalEvent = reader.Read(); //The result set contains activatable workflows

                if (signalEvent)
                {
                    base.Store.UpdateEventStatus(true, HasActivatableWorkflowEvent.Value);
                }
                else
                {
                    base.Store.UpdateEventStatus(false, HasActivatableWorkflowEvent.Value);
                    base.StoreLock.InstanceDetectionTask.ResetTimer(false);
                }
            }
            return exception;
        }
    }
}
