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

    sealed class DetectRunnableInstancesAsyncResult : SqlWorkflowInstanceStoreAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[DetectRunnableInstances]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        public DetectRunnableInstancesAsyncResult
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
            sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@workflowHostType", SqlDbType = SqlDbType.UniqueIdentifier, Value = base.Store.WorkflowHostType });
            if (base.Store.DatabaseVersion >= StoreUtilities.Version45)
            {
                sqlCommand.Parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = base.StoreLock.SurrogateLockOwnerId });
            }
        }

        protected override string GetSqlCommandText()
        {
            return DetectRunnableInstancesAsyncResult.commandText;
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
                bool instancesExist = !reader.IsDBNull(1);
                TimeSpan? timeTillNextPoll = null;
                bool instancesReadyToRun = false;

                if (instancesExist)
                {
                    DateTime expirationTime = reader.GetDateTime(1);
                    DateTime utcNow = reader.GetDateTime(2);

                    if (expirationTime <= utcNow)
                    {
                        instancesReadyToRun = true;
                    }
                    else
                    {
                        timeTillNextPoll = expirationTime.Subtract(utcNow);
                    }
                }

                if (instancesReadyToRun)
                {
                    base.Store.UpdateEventStatus(true, HasRunnableWorkflowEvent.Value);
                }
                else
                {
                    base.Store.UpdateEventStatus(false, HasRunnableWorkflowEvent.Value);
                    base.StoreLock.InstanceDetectionTask.ResetTimer(false, timeTillNextPoll);
                }
            }
            return exception;
        }
    }
}
