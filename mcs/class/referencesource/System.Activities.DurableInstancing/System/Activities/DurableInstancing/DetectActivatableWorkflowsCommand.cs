//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;

    sealed class DetectActivatableWorkflowsCommand : InstancePersistenceCommand
    {
        public DetectActivatableWorkflowsCommand() :
            base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("DetectActivatableWorkflows"))
        {
        }
    }
}
