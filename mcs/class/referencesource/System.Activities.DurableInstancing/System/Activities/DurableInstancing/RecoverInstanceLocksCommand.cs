//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;

    sealed class RecoverInstanceLocksCommand : InstancePersistenceCommand
    {
        public RecoverInstanceLocksCommand() :
            base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("RecoverInstanceLocks"))
        {
        }
    }
}
