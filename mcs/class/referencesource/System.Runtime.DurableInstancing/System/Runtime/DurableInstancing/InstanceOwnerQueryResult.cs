//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class InstanceOwnerQueryResult : InstanceStoreQueryResult
    {
        static readonly ReadOnlyDictionaryInternal<Guid, IDictionary<XName, InstanceValue>> EmptyQueryResult = new ReadOnlyDictionaryInternal<Guid, IDictionary<XName, InstanceValue>>(new Dictionary<Guid, IDictionary<XName, InstanceValue>>(0));
        static readonly ReadOnlyDictionaryInternal<XName, InstanceValue> EmptyMetadata = new ReadOnlyDictionaryInternal<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0));

        // Zero
        public InstanceOwnerQueryResult()
        {
            InstanceOwners = EmptyQueryResult;
        }

        // One
        public InstanceOwnerQueryResult(Guid instanceOwnerId, IDictionary<XName, InstanceValue> metadata)
        {
            Dictionary<Guid, IDictionary<XName, InstanceValue>> owners = new Dictionary<Guid, IDictionary<XName, InstanceValue>>(1);
            IDictionary<XName, InstanceValue> safeMetadata; // if metadata is not readonly, copy it.
            if (metadata == null || metadata.IsReadOnly)
                safeMetadata = metadata;
            else
            {
                // Copy dictionary & make a readonly wrapper.
                IDictionary<XName, InstanceValue> copy = new Dictionary<XName, InstanceValue>(metadata);
                safeMetadata = new ReadOnlyDictionaryInternal<XName, InstanceValue>(copy);
            }
            owners.Add(instanceOwnerId, metadata == null ? EmptyMetadata : safeMetadata);
            InstanceOwners = new ReadOnlyDictionaryInternal<Guid, IDictionary<XName, InstanceValue>>(owners);
        }

        // N
        public InstanceOwnerQueryResult(IDictionary<Guid, IDictionary<XName, InstanceValue>> instanceOwners)
        {
            Dictionary<Guid, IDictionary<XName, InstanceValue>> owners = new Dictionary<Guid, IDictionary<XName, InstanceValue>>(instanceOwners.Count);
            foreach (KeyValuePair<Guid, IDictionary<XName, InstanceValue>> metadata in instanceOwners)
            {
                IDictionary<XName, InstanceValue> safeMetadata; // if metadata is not readonly, copy it.
                if (metadata.Value == null || metadata.Value.IsReadOnly)
                    safeMetadata = metadata.Value;
                else
                {
                    // Copy dictionary & make a readonly wrapper.
                    IDictionary<XName, InstanceValue> copy = new Dictionary<XName, InstanceValue>(metadata.Value);
                    safeMetadata = new ReadOnlyDictionaryInternal<XName, InstanceValue>(copy);
                }
                owners.Add(metadata.Key, metadata.Value == null ? EmptyMetadata : safeMetadata);
            }
            InstanceOwners = new ReadOnlyDictionaryInternal<Guid, IDictionary<XName, InstanceValue>>(owners);
        }

        public IDictionary<Guid, IDictionary<XName, InstanceValue>> InstanceOwners { get; private set; }
    }
}
