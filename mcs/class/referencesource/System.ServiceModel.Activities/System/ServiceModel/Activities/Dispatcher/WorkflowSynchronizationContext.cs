//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Threading;

    class WorkflowSynchronizationContext : SynchronizationContext
    {
        static WorkflowSynchronizationContext singletonInstance;

        WorkflowSynchronizationContext()
            : base()
        {

        }

        public static WorkflowSynchronizationContext Instance
        {
            get
            {
                if (singletonInstance == null)
                {
                    singletonInstance = new WorkflowSynchronizationContext();
                }
                return singletonInstance;
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Send(d, state);
        }
        public override void Send(SendOrPostCallback d, object state)
        {            
            base.Send(d, state);
        }     
    }
}
