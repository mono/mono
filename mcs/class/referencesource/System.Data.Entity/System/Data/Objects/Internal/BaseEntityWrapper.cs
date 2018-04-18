//---------------------------------------------------------------------
// <copyright file="BaseEntityWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Reflection;
using System.Data.Metadata.Edm;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Base class containing common code for different implementations of the IEntityWrapper
    /// interface.  Generally speaking, operations involving the ObjectContext, RelationshipManager
    /// and raw Entity are handled through this class.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity wrapped</typeparam>
    internal abstract class BaseEntityWrapper<TEntity> : IEntityWrapper
    {
        // This enum allows boolean flags to be added to the wrapper without introducing a new field
        // for each one.  This helps keep the wrapper memory footprint small, which is important
        // in some high-performance NoTracking cases.
        [Flags]
        private enum WrapperFlags
        {
            None = 0,
            NoTracking = 1,
            InitializingRelatedEnds = 2,
        }

        private readonly RelationshipManager _relationshipManager;
        private Type _identityType;
        private WrapperFlags _flags;

        /// <summary>
        /// Constructs a wrapper for the given entity and its associated RelationshipManager.
        /// </summary>
        /// <param name="entity">The entity to be wrapped</param>
        /// <param name="relationshipManager">the RelationshipManager associated with this entity</param>
        protected BaseEntityWrapper(TEntity entity, RelationshipManager relationshipManager)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(entity != null, "Factory should ensure wrapped entity here is never null.");
            if (relationshipManager == null)
            {
                throw EntityUtil.UnexpectedNullRelationshipManager();
            }
            _relationshipManager = relationshipManager;
        }

        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="relationshipManager">The RelationshipManager associated with this entity</param>
        /// <param name="entitySet">The entity set, or null if none is known</param>
        /// <param name="context">The context to which the entity should be attached</param>
        /// <param name="mergeOption">NoTracking for non-tracked entities, AppendOnly otherwise</param>
        /// <param name="identityType">The type of the entity ignoring any possible proxy type</param>
        protected BaseEntityWrapper(TEntity entity, RelationshipManager relationshipManager, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType)
        {
            Debug.Assert(!(entity is IEntityWrapper), "Object is an IEntityWrapper instance instead of the raw entity.");
            Debug.Assert(entity != null, "Factory should ensure wrapped entity here is never null.");
            if (relationshipManager == null)
            {
                throw EntityUtil.UnexpectedNullRelationshipManager();
            }
            _identityType = identityType;
            _relationshipManager = relationshipManager;
            RelationshipManager.SetWrappedOwner(this, entity);
            if (entitySet != null)
            {
                Context = context;
                MergeOption = mergeOption;
                RelationshipManager.AttachContextToRelatedEnds(context, entitySet, mergeOption);
            }
            
        }

        // See IEntityWrapper documentation
        public RelationshipManager RelationshipManager
        {
            get
            {
                return _relationshipManager;
            }
        }

        // See IEntityWrapper documentation
        public ObjectContext Context
        {
            get;
            set;
        }

        // See IEntityWrapper documentation
        public MergeOption MergeOption
        {
            get
            {
                return (_flags & WrapperFlags.NoTracking) != 0 ? MergeOption.NoTracking : MergeOption.AppendOnly;
            }
            private set
            {
                Debug.Assert(value == MergeOption.AppendOnly || value == MergeOption.NoTracking, "Merge option must be one of NoTracking or AppendOnly.");
                if (value == MergeOption.NoTracking)
                {
                    _flags |= WrapperFlags.NoTracking;
                }
                else
                {
                    _flags &= ~WrapperFlags.NoTracking;
                }
            }
        }

        // See IEntityWrapper documentation
        public bool InitializingProxyRelatedEnds
        {
            get
            {
                return (_flags & WrapperFlags.InitializingRelatedEnds) != 0;
            }
            set
            {
                if (value)
                {
                    _flags |= WrapperFlags.InitializingRelatedEnds;
                }
                else
                {
                    _flags &= ~WrapperFlags.InitializingRelatedEnds;
                }
            }
        }

        // See IEntityWrapper documentation
        public void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
        {
            Debug.Assert(null != context, "context");
            Context = context;
            MergeOption = mergeOption;
            if (entitySet != null)
            {
                RelationshipManager.AttachContextToRelatedEnds(context, entitySet, mergeOption);
            }
        }

        // See IEntityWrapper documentation
        public void ResetContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
        {
            Debug.Assert(null != entitySet, "entitySet should not be null");
            Debug.Assert(null != context, "context");
            Debug.Assert(MergeOption.NoTracking == mergeOption ||
                         MergeOption.AppendOnly == mergeOption,
                         "mergeOption");

            if (!object.ReferenceEquals(Context, context))
            {
                Context = context;
                MergeOption = mergeOption;
                RelationshipManager.ResetContextOnRelatedEnds(context, entitySet, mergeOption);
            }
        }

        // See IEntityWrapper documentation
        public void DetachContext()
        {
            if (Context != null &&
                Context.ObjectStateManager.TransactionManager.IsAttachTracking &&
                Context.ObjectStateManager.TransactionManager.OriginalMergeOption == MergeOption.NoTracking)
            {
                // If AttachTo() failed while attaching graph retrieved with NoTracking option,
                // we don't want to clear the Context property of the wrapped entity
                MergeOption = MergeOption.NoTracking;
            }
            else
            {
                Context = null;
            }

            RelationshipManager.DetachContextFromRelatedEnds();
        }

        // See IEntityWrapper documentation
        public EntityEntry ObjectStateEntry
        {
            get;
            set;
        }

        // See IEntityWrapper documentation
        public Type IdentityType
        {
            get
            {
                if (_identityType == null)
                {
                    _identityType = EntityUtil.GetEntityIdentityType(typeof(TEntity));
                }
                return _identityType;
            }
        }

        // All these methods defined by IEntityWrapper
        public abstract void EnsureCollectionNotNull(RelatedEnd relatedEnd);
        public abstract EntityKey EntityKey { get; set; }
        public abstract bool OwnsRelationshipManager
        {
            get;
        }
        public abstract EntityKey GetEntityKeyFromEntity();
        public abstract void SetChangeTracker(IEntityChangeTracker changeTracker);
        public abstract void TakeSnapshot(EntityEntry entry);
        public abstract void TakeSnapshotOfRelationships(EntityEntry entry);
        public abstract object GetNavigationPropertyValue(RelatedEnd relatedEnd);
        public abstract void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value);
        public abstract void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value);
        public abstract void CollectionAdd(RelatedEnd relatedEnd, object value);
        public abstract bool CollectionRemove(RelatedEnd relatedEnd, object value);
        public abstract object Entity { get; }
        public abstract TEntity TypedEntity { get; }
        public abstract void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value);
        public abstract void UpdateCurrentValueRecord(object value, EntityEntry entry);
        public abstract bool RequiresRelationshipChangeTracking { get; }
    }
}
