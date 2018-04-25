//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Threading;
    using System.Collections.Generic;

    class WorkflowDispatchContext : IDisposable
    {

        [ThreadStatic]
        static WorkflowDispatchContext workflowDispatchContext = null;
        bool isWorkflowStarting;
        bool synchronous;

        public WorkflowDispatchContext(bool synchronous)
            : this(synchronous, false)
        {
            // empty
        }

        public WorkflowDispatchContext(bool synchronous, bool isWorkflowStarting)
        {
            this.synchronous = synchronous;
            this.isWorkflowStarting = isWorkflowStarting;
            workflowDispatchContext = this;
        }

        public static WorkflowDispatchContext Current
        {
            get
            {
                return workflowDispatchContext;
            }
        }

        public bool IsSynchronous
        {
            get
            {
                return this.synchronous;
            }
        }

        public bool IsWorkflowStarting
        {
            get
            {
                return this.isWorkflowStarting;
            }
        }

        public void Dispose()
        {
            workflowDispatchContext = null;
        }
    }
}
