//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;

    sealed class DetectRunnableInstancesCommand : InstancePersistenceCommand
    {
        public DetectRunnableInstancesCommand() :
            base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("DetectRunnableInstances"))
        {
        }
    }
}
