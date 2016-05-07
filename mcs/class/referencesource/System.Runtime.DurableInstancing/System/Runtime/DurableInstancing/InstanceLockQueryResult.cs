//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class InstanceLockQueryResult : InstanceStoreQueryResult
    {
        static readonly ReadOnlyDictionaryInternal<Guid, Guid> EmptyQueryResult = new ReadOnlyDictionaryInternal<Guid, Guid>(new Dictionary<Guid, Guid>(0));

        // Zero
        public InstanceLockQueryResult()
        {
            InstanceOwnerIds = EmptyQueryResult;
        }

        // One
        public InstanceLockQueryResult(Guid instanceId, Guid instanceOwnerId)
        {
            Dictionary<Guid, Guid> owners = new Dictionary<Guid, Guid>(1);
            owners.Add(instanceId, instanceOwnerId);
            InstanceOwnerIds = new ReadOnlyDictionaryInternal<Guid, Guid>(owners);
        }

        // N
        public InstanceLockQueryResult(IDictionary<Guid, Guid> instanceOwnerIds)
        {
            Dictionary<Guid, Guid> copy = new Dictionary<Guid, Guid>(instanceOwnerIds);
            InstanceOwnerIds = new ReadOnlyDictionaryInternal<Guid, Guid>(copy);
        }

        public IDictionary<Guid, Guid> InstanceOwnerIds { get; private set; }
    }
}
