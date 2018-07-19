//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public sealed class InstanceKeyView
    {
        static readonly ReadOnlyDictionaryInternal<XName, InstanceValue> emptyProperties = new ReadOnlyDictionaryInternal<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0));

        IDictionary<XName, InstanceValue> metadata;
        Dictionary<XName, InstanceValue> accumulatedMetadataWrites;

        internal InstanceKeyView(Guid key)
        {
            InstanceKey = key;
            InstanceKeyMetadataConsistency = InstanceValueConsistency.InDoubt | InstanceValueConsistency.Partial;
        }

        InstanceKeyView(InstanceKeyView source)
        {
            InstanceKey = source.InstanceKey;
            InstanceKeyState = source.InstanceKeyState;

            InstanceKeyMetadata = source.InstanceKeyMetadata;
            InstanceKeyMetadataConsistency = source.InstanceKeyMetadataConsistency;
        }

        public Guid InstanceKey { get; private set; }
        public InstanceKeyState InstanceKeyState { get; internal set; }

        public InstanceValueConsistency InstanceKeyMetadataConsistency { get; internal set; }
        public IDictionary<XName, InstanceValue> InstanceKeyMetadata
        {
            get
            {
                IDictionary<XName, InstanceValue> pendingWrites = this.accumulatedMetadataWrites;
                this.accumulatedMetadataWrites = null;
                this.metadata = pendingWrites.ReadOnlyMergeInto(this.metadata ?? InstanceKeyView.emptyProperties, true);
                return this.metadata;
            }
            internal set
            {
                this.accumulatedMetadataWrites = null;
                this.metadata = value;
            }
        }
        internal Dictionary<XName, InstanceValue> AccumulatedMetadataWrites
        {
            get
            {
                if (this.accumulatedMetadataWrites == null)
                {
                    this.accumulatedMetadataWrites = new Dictionary<XName, InstanceValue>();
                }
                return this.accumulatedMetadataWrites;
            }
        }

        internal InstanceKeyView Clone()
        {
            return new InstanceKeyView(this);
        }
    }
}
