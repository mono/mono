using System;
using System.IO;
using System.Transactions;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Security.Permissions;
using System.Threading;

using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Globalization;

namespace System.Workflow.Runtime.Hosting
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SqlPersistenceWorkflowInstanceDescription
    {
        private Guid workflowInstanceId;
        private WorkflowStatus status;
        private bool isBlocked;
        private string suspendOrTerminateDescription;
        private SqlDateTime nextTimerExpiration;

        internal SqlPersistenceWorkflowInstanceDescription(Guid workflowInstanceId, WorkflowStatus status, bool isBlocked, string suspendOrTerminateDescription, SqlDateTime nextTimerExpiration)
        {
            this.workflowInstanceId = workflowInstanceId;
            this.status = status;
            this.isBlocked = isBlocked;
            this.suspendOrTerminateDescription = suspendOrTerminateDescription;
            this.nextTimerExpiration = nextTimerExpiration;
        }

        public Guid WorkflowInstanceId { get { return workflowInstanceId; } }
        public WorkflowStatus Status { get { return status; } }
        public bool IsBlocked { get { return isBlocked; } }
        public string SuspendOrTerminateDescription { get { return suspendOrTerminateDescription; } }
        public SqlDateTime NextTimerExpiration { get { return nextTimerExpiration; } }
    }
}
