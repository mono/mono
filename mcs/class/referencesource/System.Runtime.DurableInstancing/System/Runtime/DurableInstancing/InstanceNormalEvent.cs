//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Linq;

    // InstanceStore owns the synchronization of this class.
    class InstanceNormalEvent : InstancePersistenceEvent
    {
        HashSet<InstanceHandle> boundHandles = new HashSet<InstanceHandle>();
        HashSet<InstanceHandle> pendingHandles = new HashSet<InstanceHandle>();

        internal InstanceNormalEvent(InstancePersistenceEvent persistenceEvent)
            : base(persistenceEvent.Name)
        {
        }

        internal bool IsSignaled { get; set; }

        internal HashSet<InstanceHandle> BoundHandles
        {
            get
            {
                return this.boundHandles;
            }
        }

        internal HashSet<InstanceHandle> PendingHandles
        {
            get
            {
                return this.pendingHandles;
            }
        }
    }
}
