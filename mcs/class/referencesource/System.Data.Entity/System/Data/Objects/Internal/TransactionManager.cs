//---------------------------------------------------------------------
// <copyright file="TransactionManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
//
//  Internal class used to manage ObjectStateManager's transactions for
//  AddObject/AttachTo/DetectChanges
//
//---------------------------------------------------------------------

namespace System.Data.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Diagnostics;
    using System.Data.Objects.DataClasses;

    class TransactionManager
    {
        #region Properties
        // Dictionary used to recovery after exception in ObjectContext.AttachTo()
        internal Dictionary<RelatedEnd, IList<IEntityWrapper>> PromotedRelationships
        {
            get;
            private set;
        }

        // Dictionary used to recovery after exception in ObjectContext.AttachTo()
        internal Dictionary<object, EntityEntry> PromotedKeyEntries
        {
            get;
            private set;
        }

        // HashSet used to recover after exception in ObjectContext.Add and related methods
        internal HashSet<EntityReference> PopulatedEntityReferences
        {
            get;
            private set;
        }

        // HashSet used to recover after exception in ObjectContext.Add and related methods
        internal HashSet<EntityReference> AlignedEntityReferences
        {
            get;
            private set;
        }

        // Used in recovery after exception in ObjectContext.AttachTo()
        private MergeOption? _originalMergeOption = null;
        internal MergeOption? OriginalMergeOption
        {
            get
            {
                Debug.Assert(_originalMergeOption != null, "OriginalMergeOption used before being initialized");
                return _originalMergeOption;
            }
            set
            {
                _originalMergeOption = value;
            }
        }

        // Dictionary used to recovery after exception in ObjectContext.AttachTo() and ObjectContext.AddObject()
        internal HashSet<IEntityWrapper> ProcessedEntities
        {
            get;
            private set;
        }

        // Used in Add/Attach/DetectChanges
        internal Dictionary<object, IEntityWrapper> WrappedEntities
        {
            get;
            private set;
        }

        // Used in Add/Attach/DetectChanges
        internal bool TrackProcessedEntities
        {
            get;
            private set;
        }

        internal bool IsAddTracking
        {
            get;
            private set;
        }

        internal bool IsAttachTracking
        {
            get;
            private set;
        }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> AddedRelationshipsByGraph
        {
            get;
            private set;
        }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>> DeletedRelationshipsByGraph
        {
            get;
            private set;
        }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> AddedRelationshipsByForeignKey
        {
            get;
            private set;
        }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> AddedRelationshipsByPrincipalKey
        {
            get;
            private set;
        }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>> DeletedRelationshipsByForeignKey
        {
            get;
            private set;
        }

        // Used in DetectChanges
        internal Dictionary<IEntityWrapper, HashSet<RelatedEnd>> ChangedForeignKeys
        {
            get;
            private set;
        }

        internal bool IsDetectChanges
        {
            get;
            private set;
        }

        internal bool IsAlignChanges
        {
            get;
            private set;
        }

        internal bool IsLocalPublicAPI
        {
            get;
            private set;
        }

        internal bool IsOriginalValuesGetter
        {
            get;
            private set;
        }

        internal bool IsForeignKeyUpdate 
        { 
            get; 
            private set; 
        }

        internal bool IsRelatedEndAdd
        {
            get;
            private set;
        }

        private int _graphUpdateCount;
        internal bool IsGraphUpdate
        {
            get
            {
                return _graphUpdateCount != 0;
            }
        }

        internal object EntityBeingReparented
        {
            get;
            set;
        }

        internal bool IsDetaching
        {
            get;
            private set;
        }

        internal EntityReference RelationshipBeingUpdated
        {
            get;
            private set;
        }

        internal bool IsFixupByReference
        {
            get;
            private set;
        }

        #endregion Properties


        #region Methods

        // Methods and properties used by recovery code in ObjectContext.AddObject()
        internal void BeginAddTracking()
        {
            Debug.Assert(!this.IsAddTracking);
            Debug.Assert(this.PopulatedEntityReferences == null, "Expected promotion index to be null when begining tracking.");
            Debug.Assert(this.AlignedEntityReferences == null, "Expected promotion index to be null when begining tracking.");
            this.IsAddTracking = true;
            this.PopulatedEntityReferences = new HashSet<EntityReference>();
            this.AlignedEntityReferences = new HashSet<EntityReference>();
            this.PromotedRelationships = new Dictionary<RelatedEnd, IList<IEntityWrapper>>();

            // BeginAddTracking can be called in the middle of DetectChanges.  In this case the following flags and dictionaries should not be changed here.
            if (!this.IsDetectChanges)
            {
                this.TrackProcessedEntities = true;
                this.ProcessedEntities = new HashSet<IEntityWrapper>();
                this.WrappedEntities = new Dictionary<object, IEntityWrapper>();
            }
        }

        internal void EndAddTracking()
        {
            Debug.Assert(this.IsAddTracking);
            this.IsAddTracking = false;
            this.PopulatedEntityReferences = null;
            this.AlignedEntityReferences = null;
            this.PromotedRelationships = null;

            // Clear flags/dictionaries only if we are not in the iddle of DetectChanges.
            if (!this.IsDetectChanges)
            {
                this.TrackProcessedEntities = false;

                this.ProcessedEntities = null;
                this.WrappedEntities = null;
            }
        }

        // Methods and properties used by recovery code in ObjectContext.AttachTo()
        internal void BeginAttachTracking()
        {
            Debug.Assert(!this.IsAttachTracking);
            this.IsAttachTracking = true;

            this.PromotedRelationships = new Dictionary<RelatedEnd, IList<IEntityWrapper>>();
            this.PromotedKeyEntries = new Dictionary<object, EntityEntry>();
            this.PopulatedEntityReferences = new HashSet<EntityReference>();
            this.AlignedEntityReferences = new HashSet<EntityReference>();

            this.TrackProcessedEntities = true;
            this.ProcessedEntities = new HashSet<IEntityWrapper>();
            this.WrappedEntities = new Dictionary<object, IEntityWrapper>();

            this.OriginalMergeOption = null;  // this must be set explicitely to value!=null later when the merge option is known
        }

        internal void EndAttachTracking()
        {
            Debug.Assert(this.IsAttachTracking);
            this.IsAttachTracking = false;

            this.PromotedRelationships = null;
            this.PromotedKeyEntries = null;
            this.PopulatedEntityReferences = null;
            this.AlignedEntityReferences = null;

            this.TrackProcessedEntities = false;

            this.ProcessedEntities = null;
            this.WrappedEntities = null;

            this.OriginalMergeOption = null;
        }

        // This method should be called only when there is entity in OSM which doesn't implement IEntityWithRelationships
        internal bool BeginDetectChanges()
        {
            if (this.IsDetectChanges)
            {
                return false;
            }
            this.IsDetectChanges = true;

            this.TrackProcessedEntities = true;

            this.ProcessedEntities = new HashSet<IEntityWrapper>();
            this.WrappedEntities = new Dictionary<object, IEntityWrapper>();

            this.DeletedRelationshipsByGraph = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>>();
            this.AddedRelationshipsByGraph = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<IEntityWrapper>>>();
            this.DeletedRelationshipsByForeignKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
            this.AddedRelationshipsByForeignKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
            this.AddedRelationshipsByPrincipalKey = new Dictionary<IEntityWrapper, Dictionary<RelatedEnd, HashSet<EntityKey>>>();
            this.ChangedForeignKeys = new Dictionary<IEntityWrapper, HashSet<RelatedEnd>>();
            return true;
        }

        internal void EndDetectChanges()
        {
            Debug.Assert(this.IsDetectChanges);
            this.IsDetectChanges = false;

            this.TrackProcessedEntities = false;

            this.ProcessedEntities = null;
            this.WrappedEntities = null;

            this.DeletedRelationshipsByGraph = null;
            this.AddedRelationshipsByGraph = null;
            this.DeletedRelationshipsByForeignKey = null;
            this.AddedRelationshipsByForeignKey = null;
            this.AddedRelationshipsByPrincipalKey = null;
            this.ChangedForeignKeys = null;
        }

        internal void BeginAlignChanges()
        {
            IsAlignChanges = true;
        }

        internal void EndAlignChanges()
        {
            IsAlignChanges = false;
        }

        internal void ResetProcessedEntities()
        {
            Debug.Assert(this.ProcessedEntities != null, "ProcessedEntities should not be null");
            this.ProcessedEntities.Clear();
        }

        internal void BeginLocalPublicAPI()
        {
            Debug.Assert(!this.IsLocalPublicAPI);

            this.IsLocalPublicAPI = true;
        }

        internal void EndLocalPublicAPI()
        {
            Debug.Assert(this.IsLocalPublicAPI);

            this.IsLocalPublicAPI = false;
        }

        internal void BeginOriginalValuesGetter()
        {
            Debug.Assert(!this.IsOriginalValuesGetter);

            this.IsOriginalValuesGetter = true;
        }

        internal void EndOriginalValuesGetter()
        {
            Debug.Assert(this.IsOriginalValuesGetter);

            this.IsOriginalValuesGetter = false;
        }

        internal void BeginForeignKeyUpdate(EntityReference relationship)
        {
            Debug.Assert(!this.IsForeignKeyUpdate);

            this.RelationshipBeingUpdated = relationship;
            this.IsForeignKeyUpdate = true;
        }

        internal void EndForeignKeyUpdate()
        {
            Debug.Assert(this.IsForeignKeyUpdate);

            this.RelationshipBeingUpdated = null;
            this.IsForeignKeyUpdate = false;
        }

        internal void BeginRelatedEndAdd()
        {
            Debug.Assert(!this.IsRelatedEndAdd);
            this.IsRelatedEndAdd = true;
        }

        internal void EndRelatedEndAdd()
        {
            Debug.Assert(this.IsRelatedEndAdd);
            this.IsRelatedEndAdd = false;
        }

        internal void BeginGraphUpdate()
        {
            _graphUpdateCount++;
        }

        internal void EndGraphUpdate()
        {
            Debug.Assert(_graphUpdateCount > 0);
            _graphUpdateCount--;
        }

        internal void BeginDetaching()
        {
            Debug.Assert(!IsDetaching);
            IsDetaching = true;
        }

        internal void EndDetaching()
        {
            Debug.Assert(IsDetaching);
            IsDetaching = false;
        }

        internal void BeginFixupKeysByReference()
        {
            Debug.Assert(!IsFixupByReference);
            IsFixupByReference = true;
        }

        internal void EndFixupKeysByReference()
        {
            Debug.Assert(IsFixupByReference);
            IsFixupByReference = false;
        }

        #endregion Methods

    }
}
