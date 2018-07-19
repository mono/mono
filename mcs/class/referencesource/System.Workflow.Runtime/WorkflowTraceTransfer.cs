using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace System.Workflow.Runtime
{
    internal class WorkflowTraceTransfer : IDisposable
    {
        Guid oldGuid;
        bool transferBackAtClose;

        public WorkflowTraceTransfer(Guid instanceId)
        {
            this.oldGuid = Trace.CorrelationManager.ActivityId;

            if (!this.oldGuid.Equals(instanceId)) //Avoid redundant transfers.
            {
                WorkflowTrace.Runtime.TraceTransfer(
                    0,
                    null,
                    instanceId
                    );
                Trace.CorrelationManager.ActivityId = instanceId;
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Start, 0, "Workflow Trace");
                this.transferBackAtClose = true;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (this.transferBackAtClose)
            {
                WorkflowTrace.Runtime.TraceTransfer(
                     0,
                     null,
                     oldGuid
                     );
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Stop, 0, "Workflow Trace");
                Trace.CorrelationManager.ActivityId = oldGuid;
            }
        }
        #endregion
    }
}
