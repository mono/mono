using System;
using System.Workflow.ComponentModel;

namespace System.Workflow.Runtime.DebugEngine
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IWorkflowDebuggerService
    {
        void NotifyHandlerInvoking(Delegate delegateHandler);
        void NotifyHandlerInvoked();
    }

    internal sealed class WorkflowDebuggerService : IWorkflowDebuggerService
    {
        private IWorkflowCoreRuntime coreRuntime;

        internal WorkflowDebuggerService(IWorkflowCoreRuntime coreRuntime)
        {
            if (coreRuntime == null)
                throw new ArgumentNullException("coreRuntime");

            this.coreRuntime = coreRuntime;
        }

        void IWorkflowDebuggerService.NotifyHandlerInvoking(Delegate delegateHandler)
        {
            this.coreRuntime.RaiseHandlerInvoking(delegateHandler);
        }

        void IWorkflowDebuggerService.NotifyHandlerInvoked()
        {
            this.coreRuntime.RaiseHandlerInvoked();
        }
    }
}
