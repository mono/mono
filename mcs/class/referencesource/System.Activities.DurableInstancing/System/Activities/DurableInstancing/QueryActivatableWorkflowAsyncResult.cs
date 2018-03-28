//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Transactions;
    using System.Xml.Linq;

    sealed class QueryActivatableWorkflowAsyncResult : DetectActivatableWorkflowsAsyncResult
    {
        static readonly string commandText = string.Format(CultureInfo.InvariantCulture, "{0}.[GetActivatableWorkflowsActivationParameters]", SqlWorkflowInstanceStoreConstants.DefaultSchema);

        public QueryActivatableWorkflowAsyncResult
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

        protected override Exception ProcessSqlResult(SqlDataReader reader)
        {
            Exception exception = StoreUtilities.GetNextResultSet(base.InstancePersistenceCommand.Name, reader);
            if (exception == null)
            {
                reader.NextResult();
                List<IDictionary<XName, object>> activationParametersList = new List<IDictionary<XName, object>>();
                if (reader.Read())//Activatable workflows were found
                {                    
                    do
                    {
                        IDictionary<XName, object> activationParameters = new Dictionary<XName, object>();
                        activationParameters.Add(WorkflowServiceNamespace.SiteName, reader.GetString(0));
                        activationParameters.Add(WorkflowServiceNamespace.RelativeApplicationPath, reader.GetString(1));
                        activationParameters.Add(WorkflowServiceNamespace.RelativeServicePath, reader.GetString(2));

                        activationParametersList.Add(activationParameters);
                    }
                    while (reader.Read());
                }
                else
                {
                    base.Store.UpdateEventStatus(false, HasActivatableWorkflowEvent.Value);
                    base.StoreLock.InstanceDetectionTask.ResetTimer(false);
                }
                base.InstancePersistenceContext.QueriedInstanceStore(new ActivatableWorkflowsQueryResult(activationParametersList));
            }
            return exception;
        }
    }
}
