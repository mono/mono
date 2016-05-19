//---------------------------------------------------------------------
// <copyright file="EntityWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Reflection;
using System.Data.Metadata.Edm;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// An extension of the EntityWrapper class for entities that are known not to implement
    /// IEntityWithRelationships.  Using this class causes the RelationshipManager to be created
    /// independently.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity wrapped</typeparam>
    internal sealed class EntityWrapperWithoutRelationships<TEntity> : EntityWrapper<TEntity>
    {
        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="key">The entity's key</param>
        /// <param name="entitySet">The entity set, or null if none is known</param>
        /// <param name="context">The context to which the entity should be attached</param>
        /// <param name="mergeOption">NoTracking for non-tracked entities, AppendOnly otherwise</param>
        /// <param name="identityType">The type of the entity ignoring any possible proxy type</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        internal EntityWrapperWithoutRelationships(TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType,
                                                   Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, RelationshipManager.Create(), key, entitySet, context, mergeOption, identityType,
                   propertyStrategy, changeTrackingStrategy, keyStrategy)
        {
        }

        /// <summary>
        /// Constructs a wrapper for the given entity.
        /// Note: use EntityWrapperFactory instead of calling this constructor directly.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        internal EntityWrapperWithoutRelationships(TEntity entity, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, RelationshipManager.Create(), propertyStrategy, changeTrackingStrategy, keyStrategy)
        {
        }

        public override bool OwnsRelationshipManager
        {
            get { return false; }
        }

        public override void TakeSnapshotOfRelationships(EntityEntry entry)
        {
            entry.TakeSnapshotOfRelationships();
        }

        // See IEntityWrapper documentation
        public override bool RequiresRelationshipChangeTracking
        {
            get { return true; }
        }
    }

    /// <summary>
    /// An extension of the EntityWrapper class for entities that implement IEntityWithRelationships.
    /// Using this class causes creation of the RelationshipManager to be defered to the entity object.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity wrapped</typeparam>
    internal sealed class EntityWrapperWithRelationships<TEntity> : EntityWrapper<TEntity>
        where TEntity : IEntityWithRelationships
    {
        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="key">The entity's key</param>
        /// <param name="entitySet">The entity set, or null if none is known</param>
        /// <param name="context">The context to which the entity should be attached</param>
        /// <param name="mergeOption">NoTracking for non-tracked entities, AppendOnly otherwise</param>
        /// <param name="identityType">The type of the entity ignoring any possible proxy type</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        internal EntityWrapperWithRelationships(TEntity entity, EntityKey key, EntitySet entitySet, ObjectContext context, MergeOption mergeOption, Type identityType,
                                                Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, entity.RelationshipManager, key, entitySet, context, mergeOption, identityType,
                   propertyStrategy, changeTrackingStrategy, keyStrategy)
        {
        }

        /// <summary>
        /// Constructs a wrapper for the given entity.
        /// Note: use EntityWrapperFactory instead of calling this constructor directly.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        internal EntityWrapperWithRelationships(TEntity entity, Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, entity.RelationshipManager, propertyStrategy, changeTrackingStrategy, keyStrategy)
        {
        }

        public override bool OwnsRelationshipManager
        {
            get { return true; }
        }

        public override void TakeSnapshotOfRelationships(EntityEntry entry)
        {
        }

        // See IEntityWrapper documentation
        public override bool RequiresRelationshipChangeTracking
        {
            get { return false; }
        }
    }

    /// <summary>
    /// Implementation of the IEntityWrapper interface that is used for non-null entities that do not implement
    /// all of our standard interfaces: IEntityWithKey, IEntityWithRelationships, and IEntityWithChangeTracker, and
    /// are not proxies.
    /// Different strategies for dealing with these entities are defined by strategy objects that are set into the
    /// wrapper at constructionn time.
    /// </summary>
    internal abstract class EntityWrapper<TEntity> : BaseEntityWrapper<TEntity>
    {
        private readonly TEntity _entity;
        private IPropertyAccessorStrategy _propertyStrategy;
        private IChangeTrackingStrategy _changeTrackingStrategy;
        private IEntityKeyStrategy _keyStrategy;

        /// <summary>
        /// Constructs a wrapper for the given entity.
        /// Note: use EntityWrapperFactory instead of calling this constructor directly.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="relationshipManager">The RelationshipManager associated with the entity</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        protected EntityWrapper(TEntity entity, RelationshipManager relationshipManager,
                               Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, relationshipManager)
        {
            if (relationshipManager == null)
            {
                throw EntityUtil.UnexpectedNullRelationshipManager();
            }
            _entity = entity;
            _propertyStrategy = propertyStrategy(entity);
            _changeTrackingStrategy = changeTrackingStrategy(entity);
            _keyStrategy = keyStrategy(entity);
            Debug.Assert(_changeTrackingStrategy != null, "Change tracking strategy cannot be null.");
            Debug.Assert(_keyStrategy != null, "Key strategy cannot be null.");
        }

        /// <summary>
        /// Constructs a wrapper as part of the materialization process.  This constructor is only used
        /// during materialization where it is known that the entity being wrapped is newly constructed.
        /// This means that some checks are not performed that might be needed when thw wrapper is
        /// created at other times, and information such as the identity type is passed in because
        /// it is readily available in the materializer.
        /// </summary>
        /// <param name="entity">The entity to wrap</param>
        /// <param name="relationshipManager">The RelationshipManager associated with the entity</param>
        /// <param name="key">The entity's key</param>
        /// <param name="entitySet">The entity set, or null if none is known</param>
        /// <param name="context">The context to which the entity should be attached</param>
        /// <param name="mergeOption">NoTracking for non-tracked entities, AppendOnly otherwise</param>
        /// <param name="identityType">The type of the entity ignoring any possible proxy type</param>
        /// <param name="propertyStrategy">A delegate to create the property accesor strategy object</param>
        /// <param name="changeTrackingStrategy">A delegate to create the change tracking strategy object</param>
        /// <param name="keyStrategy">A delegate to create the entity key strategy object</param>
        protected EntityWrapper(TEntity entity, RelationshipManager relationshipManager, EntityKey key, EntitySet set, ObjectContext context, MergeOption mergeOption, Type identityType,
                               Func<object, IPropertyAccessorStrategy> propertyStrategy, Func<object, IChangeTrackingStrategy> changeTrackingStrategy, Func<object, IEntityKeyStrategy> keyStrategy)
            : base(entity, relationshipManager, set, context, mergeOption, identityType)
        {
            if (relationshipManager == null)
            {
                throw EntityUtil.UnexpectedNullRelationshipManager();
            }
            _entity = entity;
            _propertyStrategy = propertyStrategy(entity);
            _changeTrackingStrategy = changeTrackingStrategy(entity);
            _keyStrategy = keyStrategy(entity);
            Debug.Assert(_changeTrackingStrategy != null, "Change tracking strategy cannot be null.");
            Debug.Assert(_keyStrategy != null, "Key strategy cannot be null.");
            _keyStrategy.SetEntityKey(key);
        }

        // See IEntityWrapper documentation
        public override void SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            _changeTrackingStrategy.SetChangeTracker(changeTracker);
        }

        // See IEntityWrapper documentation
        public override void TakeSnapshot(EntityEntry entry)
        {
            _changeTrackingStrategy.TakeSnapshot(entry);
        }

        // See IEntityWrapper documentation
        public override EntityKey EntityKey
        {
            // If no strategy is set, then the key maintained by the wrapper is used,
            // otherwise the request is passed to the strategy.
            get
            {
                return _keyStrategy.GetEntityKey();
            }
            set
            {
                _keyStrategy.SetEntityKey(value);
            }
        }

        public override EntityKey GetEntityKeyFromEntity()
        {
            return _keyStrategy.GetEntityKeyFromEntity();
        }

        public override void CollectionAdd(RelatedEnd relatedEnd, object value)
        {
            if (_propertyStrategy != null)
            {
                _propertyStrategy.CollectionAdd(relatedEnd, value);
            }
        }

        public override bool CollectionRemove(RelatedEnd relatedEnd, object value)
        {
            return _propertyStrategy != null ? _propertyStrategy.CollectionRemove(relatedEnd, value) : false;
        }

        // See IEntityWrapper documentation
        public override void EnsureCollectionNotNull(RelatedEnd relatedEnd)
        {
            if (_propertyStrategy != null)
            {
                object collection = _propertyStrategy.GetNavigationPropertyValue(relatedEnd);
                if (collection == null)
                {
                    collection = _propertyStrategy.CollectionCreate(relatedEnd);
                    _propertyStrategy.SetNavigationPropertyValue(relatedEnd, collection);
                }
            }
        }

        // See IEntityWrapper documentation
        public override object GetNavigationPropertyValue(RelatedEnd relatedEnd)
        {
            return _propertyStrategy != null ? _propertyStrategy.GetNavigationPropertyValue(relatedEnd) : null;
        }

        // See IEntityWrapper documentation
        public override void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            if (_propertyStrategy != null)
            {
                _propertyStrategy.SetNavigationPropertyValue(relatedEnd, value);
            }
        }

        // See IEntityWrapper documentation
        public override void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            if (_propertyStrategy != null)
            {
                object currentValue = _propertyStrategy.GetNavigationPropertyValue(relatedEnd);

                if (Object.ReferenceEquals(currentValue, value))
                {
                    _propertyStrategy.SetNavigationPropertyValue(relatedEnd, null);
                }
            }
        }

        // See IEntityWrapper documentation
        public override object Entity
        {
            get { return _entity; }
        }

        // See IEntityWrapper<TEntity> documentation
        public override TEntity TypedEntity
        {
            get { return _entity; }
        }

        // See IEntityWrapper documentation
        public override void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
        {
            _changeTrackingStrategy.SetCurrentValue(entry, member, ordinal, target, value);
        }

        // See IEntityWrapper documentation
        public override void UpdateCurrentValueRecord(object value, EntityEntry entry)
        {
            _changeTrackingStrategy.UpdateCurrentValueRecord(value, entry);
        }
    }
}
