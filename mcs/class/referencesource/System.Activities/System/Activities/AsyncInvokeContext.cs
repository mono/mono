//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Xml;
    using System.Collections.Generic;
    using System.Threading;

    class AsyncInvokeContext
    {
        public AsyncInvokeContext(object userState, WorkflowInvoker invoker)
        {
            this.UserState = userState;                      
            SynchronizationContext syncContext = SynchronizationContext.Current ?? WorkflowApplication.SynchronousSynchronizationContext.Value;
            this.Operation = new AsyncInvokeOperation(syncContext);
            this.Invoker = invoker;
        }

        public object UserState
        {
            get;
            private set;
        }

        public AsyncInvokeOperation Operation
        {
            get;
            private set;
        }

        public WorkflowApplication WorkflowApplication
        {
            get;
            set;
        }

        public WorkflowInvoker Invoker
        {
            get;
            private set;
        }

        public IDictionary<string, object> Outputs
        {
            get;
            set;
        }
    }
}
