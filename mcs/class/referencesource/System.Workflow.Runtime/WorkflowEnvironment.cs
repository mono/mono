#pragma warning disable 1634, 1691
using System;
using System.Collections;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Diagnostics;
using System.Transactions;

namespace System.Workflow.Runtime
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public static class WorkflowEnvironment /*: IDisposable*/
    {
        public static IWorkBatch WorkBatch
        {
            get
            {
                IWorkBatch currentBatch = ServiceEnvironment.WorkBatch;

#pragma warning disable 56503
                if (currentBatch == null)
                    throw new System.InvalidOperationException(ExecutionStringManager.WorkBatchNotFound);
#pragma warning restore 56503

                return currentBatch;
            }
        }

        public static Guid WorkflowInstanceId
        {
            get
            {
#pragma warning disable 56503
                Guid currentInstanceId = ServiceEnvironment.WorkflowInstanceId;

                if (currentInstanceId == Guid.Empty)
                    throw new System.InvalidOperationException(ExecutionStringManager.InstanceIDNotFound);
#pragma warning restore 56503

                return currentInstanceId;
            }
        }
    }
}
