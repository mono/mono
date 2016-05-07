//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;

    sealed class ExtendLockCommand : InstancePersistenceCommand
    {
        public ExtendLockCommand() :
            base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("ExtendLock"))
        {
        }
    }
}
