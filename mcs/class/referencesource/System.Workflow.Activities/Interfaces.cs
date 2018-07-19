namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Transactions;
    using System.Workflow.Runtime.Hosting;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ConditionalEventArgs : EventArgs
    {
        bool result;

        public bool Result
        {
            get
            {
                return result;
            }
            set
            {
                result = value;
            }
        }

        public ConditionalEventArgs()
            : this(false)
        {

        }

        public ConditionalEventArgs(bool result)
        {
            this.result = result;
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public interface IEventActivity
    {
        void Subscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler);
        void Unsubscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler);
        IComparable QueueName { get; }
    }
}
