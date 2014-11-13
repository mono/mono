//---------------------------------------------------------------------
// <copyright file="IEntityWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections;
using System.Data.Objects.DataClasses;
using System.Runtime.CompilerServices;
using System.Data.Metadata.Edm;

namespace System.Data.Objects.Internal
{
    /// <summary>
    /// Internally, entities are wrapped in some implementation of this
    /// interface.  This allows the RelationshipManager and other classes
    /// to treat POCO entities and traditional entities in the same way
    /// where ever possible.
    /// </summary>
    internal interface IEntityWrapper
    {
        /// <summary>
        /// The Relationship Manager that is associated with the wrapped entity.
        /// </summary>
        RelationshipManager RelationshipManager { get; }

        /// <summary>
        /// Information about whether or not the entity instance actually owns and uses its RelationshipManager
        /// This is used to determine how to do relationship fixup in some cases
        /// </summary>
        bool OwnsRelationshipManager { get; }

        /// <summary>
        /// The actual entity that is wrapped by this wrapper object.
        /// </summary>
        object Entity { get; }

        /// <summary>
        /// If this IEntityWrapper is tracked, accesses the ObjectStateEntry that is used in the state manager
        /// </summary>
        EntityEntry ObjectStateEntry { get; set; }

        /// <summary>
        /// Ensures that the collection with the given name is not null by setting a new empty
        /// collection onto the property if necessary.
        /// </summary>
        /// <param name="collectionName">The name of the collection to operate on</param>
        void EnsureCollectionNotNull(RelatedEnd relatedEnd);

        /// <summary>
        /// The key associated with this entity, which may be null if no key is known.
        /// </summary>
        EntityKey EntityKey { get; set; }

        /// <summary>
        /// Retrieves the EntityKey from the entity if it implements IEntityWithKey
        /// </summary>
        /// <returns>The EntityKey on the entity</returns>
        EntityKey GetEntityKeyFromEntity();

        /// <summary>
        /// The context with which the wrapped entity is associated, or null if the entity
        /// is detached.
        /// </summary>
        ObjectContext Context { get; set; }

        /// <summary>
        /// The merge option ----oicated with the wrapped entity.
        /// </summary>
        MergeOption MergeOption { get; }

        /// <summary>
        /// Attaches the wrapped entity to the given context.
        /// </summary>
        /// <param name="context">the context with which to associate this entity</param>
        /// <param name="entitySet">the entity set to which the entity belongs</param>
        /// <param name="mergeOption">the merge option to use</param>
        void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption);

        /// <summary>
        /// Resets the context with which the wrapped entity is associated.
        /// </summary>
        /// <param name="context">the context with which to associate this entity</param>
        /// <param name="entitySet">the entity set to which the entity belongs</param>
        /// <param name="mergeOption">the merge option to use</param>
        void ResetContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption);

        /// <summary>
        /// Detaches the wrapped entity from its associated context.
        /// </summary>
        void DetachContext();

        /// <summary>
        /// Sets the entity's ObjectStateEntry as the entity's change tracker if possible.
        /// The ObjectStateEntry may be null when a change tracker is being removed from an
        /// entity.
        /// </summary>
        /// <param name="changeTracker">the object to use as a change tracker</param>
        void SetChangeTracker(IEntityChangeTracker changeTracker);

        /// <summary>
        /// Takes a snapshot of the entity state unless the entity has an associated
        /// change tracker or the given entry is null, in which case no action is taken.
        /// </summary>
        /// <param name="entry">the entity's associated state entry</param>
        void TakeSnapshot(EntityEntry entry);

        /// <summary>
        /// Takes a snapshot of the relationships of the entity stored in the entry
        /// </summary>
        /// <param name="entry"></param>
        void TakeSnapshotOfRelationships(EntityEntry entry);

        /// <summary>
        /// The Type object that should be used to identify this entity in o-space.
        /// This is normally just the type of the entity object, but if the object
        /// is a proxy that we have generated, then the type of the base class is returned instead.
        /// This ensures that both proxy entities and normal entities are treated as the
        /// same kind of entity in the metadata and places where the metadata is used.
        /// </summary>
        Type IdentityType { get; }

        /// <summary>
        /// Populates a value into a collection of values stored in a property of the entity.
        /// If the collection to be populated is actually managed by and returned from
        /// the RelationshipManager when needed, then this method is a no-op.  This is
        /// typically the case for non-POCO entities.
        /// </summary>
        void CollectionAdd(RelatedEnd relatedEnd, object value);

        /// <summary>
        /// Removes a value from a collection of values stored in a property of the entity.
        /// If the collection to be updated is actually managed by and returned from
        /// the RelationshipManager when needed, then this method is a no-op.  This is
        /// typically the case for non-POCO entities.
        /// </summary>
        bool CollectionRemove(RelatedEnd relatedEnd, object value);

        /// <summary>
        /// Returns value of the entity's property described by the navigation property.
        /// </summary>
        /// <param name="relatedEnd">navigation property to retrieve</param>
        /// <returns></returns>
        object GetNavigationPropertyValue(RelatedEnd relatedEnd);

        /// <summary>
        /// Populates a single value into a field or property of the entity.
        /// If the element to be populated is actually managed by and returned from
        /// the RelationshipManager when needed, then this method is a no-op.  This is
        /// typically the case for non-POCO entities.
        /// </summary>
        void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value);

        /// <summary>
        /// Removes a single value from a field or property of the entity.
        /// If the field or property contains reference to a different object,
        /// this method is a no-op.
        /// If the element to be populated is actually managed by and returned from
        /// the RelationshipManager when needed, then this method is a no-op.  This is
        /// typically the case for non-POCO entities.
        /// </summary>
        /// <param name="value">The value to remove</param>
        void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value);

        /// <summary>
        /// Sets the given value onto the entity with the registered change either handled by the
        /// entity itself or by using the given EntityEntry as the change tracker.
        /// </summary>
        /// <param name="entry">The state entry of the entity to for which a value should be set</param>
        /// <param name="member">State member information indicating the member to set</param>
        /// <param name="ordinal">The ordinal of the member to set</param>
        /// <param name="target">The object onto which the value should be set; may be the entity, or a contained complex value</param>
        /// <param name="value">The value to set</param>
        void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value);

        /// <summary>
        /// Set to true while the process of initalizing RelatedEnd objects for an IPOCO proxy is in process.
        /// This flag prevents the context from being set onto the related ends, which in turn means that
        /// the related ends don't need to have keys, which in turn means they don't need to be part of an EntitySet.
        /// </summary>
        bool InitializingProxyRelatedEnds { get; set; }

        /// <summary>
        /// Updates the current value records using Shaper.UpdateRecord but with additional change tracking logic
        /// added as required by POCO and proxy entities.  For the simple case of no proxy and an entity with
        /// a change tracker, this translates into a simple call to ShaperUpdateRecord.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="entry">The existing ObjectStateEntry</param>
        void UpdateCurrentValueRecord(object value, EntityEntry entry);

        /// <summary>
        /// True if the underlying entity is not capable of tracking changes to relationships such that
        /// DetectChanges is required to do this.
        /// </summary>
        bool RequiresRelationshipChangeTracking { get; }
    }
}
