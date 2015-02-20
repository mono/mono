using System;

namespace System.Workflow.Runtime.DebugEngine
{

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum WorkflowDebuggerSteppingOption
    {
        Sequential = 0,
        Concurrent = 1
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowDebuggerSteppingAttribute : Attribute
    {
        private WorkflowDebuggerSteppingOption steppingOption;

        public WorkflowDebuggerSteppingAttribute(WorkflowDebuggerSteppingOption steppingOption)
        {
            this.steppingOption = steppingOption;
        }

        public WorkflowDebuggerSteppingOption SteppingOption
        {
            get
            {
                return this.steppingOption;
            }
        }
    }
}
