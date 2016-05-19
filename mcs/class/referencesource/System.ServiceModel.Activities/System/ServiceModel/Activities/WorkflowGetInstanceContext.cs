//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities
{
    using System.Collections.Generic;

    // Property bag classes for DurableInstanceManager.BeginGetInstance parameters
    class WorkflowGetInstanceContext
    {
        public WorkflowHostingEndpoint WorkflowHostingEndpoint
        {
            get;
            set;
        }

        public bool CanCreateInstance
        {
            get;
            set;
        }

        public object[] Inputs
        {
            get;
            set;
        }

        public OperationContext OperationContext
        {
            get;
            set;
        }

        // Output argument
        public WorkflowCreationContext WorkflowCreationContext
        {
            get;
            set;
        }

        // Output argument
        public WorkflowHostingResponseContext WorkflowHostingResponseContext
        {
            get;
            set;
        }
    }
}
