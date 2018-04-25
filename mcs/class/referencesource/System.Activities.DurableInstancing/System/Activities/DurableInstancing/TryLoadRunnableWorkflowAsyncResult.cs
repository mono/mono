//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.DurableInstancing;
    using System.Transactions;
    using System.Globalization;

    sealed class TryLoadRunnableWorkflowAsyncResult : LoadWorkflowAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[TryLoadRunnableInstance]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        public TryLoadRunnableWorkflowAsyncResult
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
            if (base.Store.WorkflowHostType == Guid.Empty)
            {
                throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(command.Name, SR.TryLoadRequiresWorkflowType, null));
            }
        }

        protected override void GenerateSqlCommand(SqlCommand command)
        {
            double operationTimeout = this.TimeoutHelper.RemainingTime().TotalMilliseconds;

            command.Parameters.Add(new SqlParameter { ParameterName = "@surrogateLockOwnerId", SqlDbType = SqlDbType.BigInt, Value = base.StoreLock.SurrogateLockOwnerId });
            command.Parameters.Add(new SqlParameter { ParameterName = "@workflowHostType", SqlDbType = SqlDbType.UniqueIdentifier, Value = base.Store.WorkflowHostType });
            command.Parameters.Add(new SqlParameter { ParameterName = "@operationType", SqlDbType = SqlDbType.TinyInt, Value = LoadType.LoadByInstance });
            command.Parameters.Add(new SqlParameter { ParameterName = "@handleInstanceVersion", SqlDbType = SqlDbType.BigInt, Value = base.InstancePersistenceContext.InstanceVersion });
            command.Parameters.Add(new SqlParameter { ParameterName = "@handleIsBoundToLock", SqlDbType = SqlDbType.Bit, Value = base.InstancePersistenceContext.InstanceView.IsBoundToLock });
            command.Parameters.Add(new SqlParameter { ParameterName = "@encodingOption", SqlDbType = SqlDbType.TinyInt, Value = base.Store.InstanceEncodingOption });
            command.Parameters.Add(new SqlParameter { ParameterName = "@operationTimeout", SqlDbType = SqlDbType.Int, Value = (operationTimeout < Int32.MaxValue) ? Convert.ToInt32(operationTimeout) : Int32.MaxValue });            
        }

        protected override string GetSqlCommandText()
        {
            return TryLoadRunnableWorkflowAsyncResult.commandText;
        }

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = StoreUtilities.GetNextResultSet(this.InstancePersistenceCommand.Name, reader);

            if (exception == null)
            {
                bool runnableInstanceFound = reader.GetBoolean(1);
                if (!runnableInstanceFound || base.ProcessSqlResult(reader) != null)
                {
                    base.Store.UpdateEventStatus(false, HasRunnableWorkflowEvent.Value);
                    base.StoreLock.InstanceDetectionTask.ResetTimer(false);
                }
            }

            return exception;
        }
    }
}
