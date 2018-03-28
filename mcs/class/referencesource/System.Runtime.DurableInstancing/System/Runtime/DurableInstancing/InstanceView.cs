//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Xml.Linq;
    using System.Threading;

    [Fx.Tag.XamlVisible(false)]
    public sealed class InstanceView
    {
        static readonly ReadOnlyDictionaryInternal<XName, InstanceValue> emptyProperties = new ReadOnlyDictionaryInternal<XName, InstanceValue>(new Dictionary<XName, InstanceValue>(0));
        static readonly ReadOnlyDictionaryInternal<Guid, InstanceKeyView> emptyKeys = new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(new Dictionary<Guid, InstanceKeyView>(0));

        IDictionary<XName, InstanceValue> data;
        IDictionary<XName, InstanceValue> metadata;
        IDictionary<XName, InstanceValue> ownerMetadata;
        IDictionary<Guid, InstanceKeyView> keys;
        ReadOnlyCollection<InstanceStoreQueryResult> queryResults;

        Dictionary<XName, InstanceValue> accumulatedMetadataWrites;
        Dictionary<XName, InstanceValue> accumulatedOwnerMetadataWrites;
        Collection<InstanceStoreQueryResult> queryResultsBackingCollection;

        long instanceVersion;

        internal InstanceView(InstanceOwner owner)
            : this()
        {
            InstanceOwner = owner;
        }

        internal InstanceView(InstanceOwner owner, Guid instanceId)
            : this()
        {
            Fx.Assert(instanceId != Guid.Empty, "Null instanceId passed to InstanceView ctor.");

            InstanceOwner = owner;
            InstanceId = instanceId;
            IsBoundToInstance = true;
        }

        InstanceView()
        {
            this.instanceVersion = -1;
            InstanceDataConsistency = InstanceValueConsistency.InDoubt | InstanceValueConsistency.Partial;
            InstanceMetadataConsistency = InstanceValueConsistency.InDoubt | InstanceValueConsistency.Partial;
            InstanceOwnerMetadataConsistency = InstanceValueConsistency.InDoubt | InstanceValueConsistency.Partial;
            InstanceKeysConsistency = InstanceValueConsistency.InDoubt | InstanceValueConsistency.Partial;
        }

        InstanceView(InstanceView source)
        {
            this.instanceVersion = source.instanceVersion;

            InstanceOwner = source.InstanceOwner;
            InstanceId = source.InstanceId;
            IsBoundToInstance = source.IsBoundToInstance;

            InstanceState = source.InstanceState;

            InstanceDataConsistency = source.InstanceDataConsistency;
            InstanceMetadataConsistency = source.InstanceMetadataConsistency;
            InstanceOwnerMetadataConsistency = source.InstanceOwnerMetadataConsistency;
            InstanceKeysConsistency = source.InstanceKeysConsistency;

            InstanceData = source.InstanceData;
            InstanceMetadata = source.InstanceMetadata;
            InstanceOwnerMetadata = source.InstanceOwnerMetadata;

            InstanceStoreQueryResults = source.InstanceStoreQueryResults;

            Dictionary<Guid, InstanceKeyView> keys = null;
            if (source.InstanceKeys.Count > 0)
            {
                keys = new Dictionary<Guid, InstanceKeyView>(source.InstanceKeys.Count);
                foreach (KeyValuePair<Guid, InstanceKeyView> key in source.InstanceKeys)
                {
                    keys.Add(key.Key, key.Value.Clone());
                }
            }
            InstanceKeys = keys == null ? null : new ReadOnlyDictionaryInternal<Guid, InstanceKeyView>(keys);
        }

        public Guid InstanceId { get; private set; }
        public bool IsBoundToInstance { get; private set; }

        public InstanceOwner InstanceOwner { get; private set; }
        public bool IsBoundToInstanceOwner
        {
            get
            {
                return InstanceOwner != null;
            }
        }

        public bool IsBoundToLock
        {
            get
            {
                return this.instanceVersion >= 0;
            }
        }

        public InstanceState InstanceState { get; internal set; }

        // All dictionaries are always read-only.

        public InstanceValueConsistency InstanceDataConsistency { get; internal set; }
        public IDictionary<XName, InstanceValue> InstanceData
        {
            get
            {
                return this.data ?? InstanceView.emptyProperties;
            }
            internal set
            {
                Fx.AssertAndThrow(!IsViewFrozen, "Setting Data on frozen View.");
                this.data = value;
            }
        }

        public InstanceValueConsistency InstanceMetadataConsistency { get; internal set; }
        public IDictionary<XName, InstanceValue> InstanceMetadata
        {
            get
            {
                IDictionary<XName, InstanceValue> pendingWrites = this.accumulatedMetadataWrites;
                this.accumulatedMetadataWrites = null;
                this.metadata = pendingWrites.ReadOnlyMergeInto(this.metadata ?? InstanceView.emptyProperties, true);
                return this.metadata;
            }
            internal set
            {
                Fx.AssertAndThrow(!IsViewFrozen, "Setting Metadata on frozen View.");
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

        public InstanceValueConsistency InstanceOwnerMetadataConsistency { get; internal set; }
        public IDictionary<XName, InstanceValue> InstanceOwnerMetadata
        {
            get
            {
                IDictionary<XName, InstanceValue> pendingWrites = this.accumulatedOwnerMetadataWrites;
                this.accumulatedOwnerMetadataWrites = null;
                this.ownerMetadata = pendingWrites.ReadOnlyMergeInto(this.ownerMetadata ?? InstanceView.emptyProperties, true);
                return this.ownerMetadata;
            }
            internal set
            {
                Fx.AssertAndThrow(!IsViewFrozen, "Setting OwnerMetadata on frozen View.");
                this.accumulatedOwnerMetadataWrites = null;
                this.ownerMetadata = value;
            }
        }
        internal Dictionary<XName, InstanceValue> AccumulatedOwnerMetadataWrites
        {
            get
            {
                if (this.accumulatedOwnerMetadataWrites == null)
                {
                    this.accumulatedOwnerMetadataWrites = new Dictionary<XName, InstanceValue>();
                }
                return this.accumulatedOwnerMetadataWrites;
            }
        }

        public InstanceValueConsistency InstanceKeysConsistency { get; internal set; }
        public IDictionary<Guid, InstanceKeyView> InstanceKeys
        {
            get
            {
                return this.keys ?? InstanceView.emptyKeys;
            }
            internal set
            {
                Fx.AssertAndThrow(!IsViewFrozen, "Setting Keys on frozen View.");
                this.keys = value;
            }
        }

        // Arch prefers ReadOnlyCollection here to ICollection.   
        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.CollectionPropertiesShouldBeReadOnly,
            Justification = "property is of ReadOnlyCollection type")]
        public ReadOnlyCollection<InstanceStoreQueryResult> InstanceStoreQueryResults
        {
            get
            {
                if (this.queryResults == null)
                {
                    this.queryResults = new ReadOnlyCollection<InstanceStoreQueryResult>(QueryResultsBacking);
                }
                return this.queryResults;
            }
            internal set
            {
                Fx.AssertAndThrow(!IsViewFrozen, "Setting InstanceStoreQueryResults on frozen View.");
                this.queryResults = null;
                if (value == null)
                {
                    this.queryResultsBackingCollection = null;
                }
                else
                {
                    this.queryResultsBackingCollection = new Collection<InstanceStoreQueryResult>();
                    foreach (InstanceStoreQueryResult queryResult in value)
                    {
                        this.queryResultsBackingCollection.Add(queryResult);
                    }
                }
            }
        }
        internal Collection<InstanceStoreQueryResult> QueryResultsBacking
        {
            get
            {
                if (this.queryResultsBackingCollection == null)
                {
                    this.queryResultsBackingCollection = new Collection<InstanceStoreQueryResult>();
                }
                return this.queryResultsBackingCollection;
            }
        }

        internal void BindOwner(InstanceOwner owner)
        {
            Fx.AssertAndThrow(!IsViewFrozen, "BindOwner called on read-only InstanceView.");
            Fx.Assert(owner != null, "Null owner passed to BindOwner.");
            if (IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToOwner));
            }
            InstanceOwner = owner;
        }

        internal void BindInstance(Guid instanceId)
        {
            Fx.AssertAndThrow(!IsViewFrozen, "BindInstance called on read-only InstanceView.");
            Fx.Assert(instanceId != Guid.Empty, "Null instanceId passed to BindInstance.");
            if (IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToInstance));
            }
            InstanceId = instanceId;
            IsBoundToInstance = true;
        }

        internal void BindLock(long instanceVersion)
        {
            Fx.AssertAndThrow(!IsViewFrozen, "BindLock called on read-only InstanceView.");
            Fx.Assert(instanceVersion >= 0, "Negative instanceVersion passed to BindLock.");
            if (!IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToOwner));
            }
            if (!IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToInstance));
            }
            if (Interlocked.CompareExchange(ref this.instanceVersion, instanceVersion, -1) != -1)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToLock));
            }
        }

        // This method is called when IPC.BindReclaimedLock is called.  It reserves the lock, so that future calls to this or BindLock can be prevented.
        // We set the version to -(instanceVersion + 2) so that 0 maps to -2 (since -1 is special).
        internal void StartBindLock(long instanceVersion)
        {
            Fx.AssertAndThrow(!IsViewFrozen, "StartBindLock called on read-only InstanceView.");
            Fx.Assert(instanceVersion >= 0, "Negative instanceVersion passed to StartBindLock.");
            if (!IsBoundToInstanceOwner)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToOwner));
            }
            if (!IsBoundToInstance)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextMustBeBoundToInstance));
            }
            if (Interlocked.CompareExchange(ref this.instanceVersion, checked(-instanceVersion - 2), -1) != -1)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.ContextAlreadyBoundToLock));
            }
        }

        // This completes the bind started in StartBindLock.
        internal void FinishBindLock(long instanceVersion)
        {
            Fx.AssertAndThrow(!IsViewFrozen, "FinishBindLock called on read-only InstanceView.");
            Fx.Assert(IsBoundToInstanceOwner, "Must be bound to owner, checked in StartBindLock.");
            Fx.Assert(IsBoundToInstance, "Must be bound to instance, checked in StartBindLock.");

            long result = Interlocked.CompareExchange(ref this.instanceVersion, instanceVersion, -instanceVersion - 2);
            Fx.AssertAndThrow(result == -instanceVersion - 2, "FinishBindLock called with mismatched instance version.");
        }

        internal void MakeReadOnly()
        {
            IsViewFrozen = true;
        }

        internal InstanceView Clone()
        {
            return new InstanceView(this);
        }

        bool IsViewFrozen { get; set; }
    }
}
