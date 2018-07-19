//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Runtime.DurableInstancing;

    sealed class UnlockInstanceCommand : InstancePersistenceCommand
    {
        public UnlockInstanceCommand() :
            base(SqlWorkflowInstanceStoreConstants.DurableInstancingNamespace.GetName("UnlockInstance"))
        {
        }

        public Guid InstanceId
        {
            get;
            set;
        }

        public long InstanceVersion
        {
            get;
            set;
        }

        public long SurrogateOwnerId
        {
            get;
            set;
        }
    }
}
