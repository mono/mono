//---------------------------------------------------------------------
// <copyright file="EntityReference_TResultType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.DataClasses
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    /// <summary>
    /// Models a relationship end with multiplicity 1.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [DataContract]
    [Serializable]
    public sealed class EntityReference<TEntity> : EntityReference
        where TEntity : class
    {
        // ------
        // Fields
        // ------

        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        // Note that this field should no longer be used directly.  Instead, use the _wrappedCachedValue
        // field.  This field is retained only for compatibility with the serialization format introduced in v1.
        private TEntity _cachedValue;

        [NonSerialized]
        private IEntityWrapper _wrappedCachedValue;

        // ------------
        // Constructors
        // ------------

        /// <summary>
        /// The default constructor is required for some serialization scenarios. It should not be used to 
        /// create new EntityReferences. Use the GetRelatedReference or GetRelatedEnd methods on the RelationshipManager
        /// class instead.
        /// </summary>
        public EntityReference()
        {
            _wrappedCachedValue = EntityWrapperFactory.NullWrapper;
        }

        internal EntityReference(IEntityWrapper wrappedOwner, RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
            : base(wrappedOwner, navigation, relationshipFixer)
        {
            _wrappedCachedValue = EntityWrapperFactory.NullWrapper;
        }

        // ----------
        // Properties
        // ----------

        /// <summary>
        /// Stub only please replace with actual implementation
        /// </summary>
        [System.Xml.Serialization.SoapIgnore]
        [System.Xml.Serialization.XmlIgnore]
        public TEntity Value
        {
            get
            {
                DeferredLoad();
                return (TEntity)ReferenceValue.Entity;
            }
            set
            {
                ReferenceValue = EntityWrapperFactory.WrapEntityUsingContext(value, ObjectContext);
            }
        }

        internal override IEntityWrapper CachedValue
        {
            get { return _wrappedCachedValue; }
        }

        internal override IEntityWrapper ReferenceValue
        {
            get
            {
                CheckOwnerNull();
                return _wrappedCachedValue;
            }
            set
            {
                CheckOwnerNull();
                //setting to same value is a no-op (SQL BU DT # 446320)
                //setting to null is a special case because then we will also clear out any Added/Unchanged relationships with key entries, so we can't no-op if Value is null
                if (value.Entity != null && value.Entity == _wrappedCachedValue.Entity)
                {
                    return;
                }

                if (null != value.Entity)
                {
                    // Note that this is only done for the case where we are not setting the ref to null because
                    // clearing a ref is okay--it will cause the dependent to become deleted/detached.
                    ValidateOwnerWithRIConstraints(value, value == EntityWrapperFactory.NullWrapper ? null : value.EntityKey, checkBothEnds: true);
                    ObjectContext context = ObjectContext ?? value.Context;
                    if (context != null)
                    {
                        context.ObjectStateManager.TransactionManager.EntityBeingReparented = GetDependentEndOfReferentialConstraint(value.Entity);
                    }
                    try
                    {
                        Add(value, /*applyConstraints*/false);
                    }
                    finally
                    {
                        if (context != null)
                        {
                            context.ObjectStateManager.TransactionManager.EntityBeingReparented = null;
                        }
                    }
                }
                else
                {
                    if (UsingNoTracking)
                    {
                        if (_wrappedCachedValue.Entity != null)
                        {
                            // The other end of relationship can be the EntityReference or EntityCollection
                            // If the other end is EntityReference, its IsLoaded property should be set to FALSE
                            RelatedEnd relatedEnd = GetOtherEndOfRelationship(_wrappedCachedValue);
                            relatedEnd.OnRelatedEndClear();
                        }

                        _isLoaded = false;
                    }
                    else
                    {
                        if (ObjectContext != null && ObjectContext.ContextOptions.UseConsistentNullReferenceBehavior)
                        {
                            AttemptToNullFKsOnRefOrKeySetToNull();
                        }
                    }

                    ClearCollectionOrRef(null, null, false);
                }
            }
        }

        // -------
        // Methods
        // -------

        /// <summary>
        /// Loads the related entity or entities into the local related end using the supplied MergeOption.        
        /// </summary>        
        public override void Load(MergeOption mergeOption)
        {
            CheckOwnerNull();

            // Validate that the Load is possible
            bool hasResults;
            ObjectQuery<TEntity> sourceQuery = ValidateLoad<TEntity>(mergeOption, "EntityReference", out hasResults);

            _suppressEvents = true; // we do not want any event during the bulk operation
            try
            {
                List<TEntity> refreshedValue = null;
                if (hasResults)
                {
                    // Only issue a query if we know it can produce results (in the case of FK, there may not be any 
                    // results).
                    refreshedValue = new List<TEntity>(GetResults<TEntity>(sourceQuery));
                }
                if (null == refreshedValue || refreshedValue.Count == 0)
                {
                    if (!((AssociationType)base.RelationMetadata).IsForeignKey && ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One)
                    {
                        //query returned zero related end; one related end was expected.
                        throw EntityUtil.LessThanExpectedRelatedEntitiesFound();
                    }
                    else if (mergeOption == MergeOption.OverwriteChanges || mergeOption == MergeOption.PreserveChanges)
                    {
                        // This entity is not related to anything in this AssociationSet and Role on the server.
                        // If there is an existing _cachedValue, we may need to clear it out, based on the MergeOption
                        EntityKey sourceKey = WrappedOwner.EntityKey;
                        EntityUtil.CheckEntityKeyNull(sourceKey);
                        ObjectStateManager.RemoveRelationships(ObjectContext, mergeOption, (AssociationSet)RelationshipSet, sourceKey, (AssociationEndMember)FromEndProperty);
                    }
                    // else this is NoTracking or AppendOnly, and no entity was retrieved by the Load, so there's nothing extra to do

                    // Since we have no value and are not doing a merge, the last step is to set IsLoaded to true
                    _isLoaded = true;
                }
                else if (refreshedValue.Count == 1)
                {
                    Merge<TEntity>(refreshedValue, mergeOption, true /*setIsLoaded*/);
                }
                else
                {
                    // More than 1 result, which is non-recoverable data inconsistency
                    throw EntityUtil.MoreThanExpectedRelatedEntitiesFound();
                }
            }
            finally
            {
                _suppressEvents = false;
            }
            // fire the AssociationChange with Refresh
            OnAssociationChanged(CollectionChangeAction.Refresh, null);
        }

        /// <summary>
        /// This operation is not allowed if the owner is null
        /// </summary>
        /// <returns></returns>
        internal override IEnumerable GetInternalEnumerable()
        {
            if (ReferenceValue.Entity != null)
            {
                yield return (TEntity)ReferenceValue.Entity;
            }
        }

        internal override IEnumerable<IEntityWrapper> GetWrappedEntities()
        {
            // 
            return _wrappedCachedValue.Entity == null ? new IEntityWrapper[0] : new IEntityWrapper[] { _wrappedCachedValue };
        }

        /// <summary>
        /// Attaches an entity to the EntityReference. The given
        /// entity is not assumed to be the complete set of related entities.
        /// 
        /// Owner and all entities passed in must be in Unchanged or Modified state. 
        /// Deleted elements are allowed only when the state manager is already tracking the relationship
        /// instance.
        /// </summary>
        /// <param name="entity">The entity to attach to the EntityCollection</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity cannot be related via the current relationship end.</exception>
        public void Attach(TEntity entity)
        {
            CheckOwnerNull();
            EntityUtil.CheckArgumentNull(entity, "entity");
            Attach(new IEntityWrapper[] { EntityWrapperFactory.WrapEntityUsingContext(entity, ObjectContext) }, false);
        }

        internal override void Include(bool addRelationshipAsUnchanged, bool doAttach)
        {
            Debug.Assert(this.ObjectContext != null, "Should not be trying to add entities to state manager if context is null");

            // If we have an actual value or a key for this reference, add it to the context
            if (null != _wrappedCachedValue.Entity)
            {
                // Sometimes with mixed POCO and IPOCO, you can get different instances of IEntityWrappers stored in the IPOCO related ends
                // These should be replaced by the IEntityWrapper that is stored in the context
                IEntityWrapper identityWrapper = EntityWrapperFactory.WrapEntityUsingContext(_wrappedCachedValue.Entity, WrappedOwner.Context);
                if (identityWrapper != _wrappedCachedValue)
                {
                    _wrappedCachedValue = identityWrapper;
                }
                IncludeEntity(_wrappedCachedValue, addRelationshipAsUnchanged, doAttach);
            }
            else if (DetachedEntityKey != null)
            {
                IncludeEntityKey(doAttach);
            }
            // else there is nothing to add for this relationship
        }

        private void IncludeEntityKey(bool doAttach)
        {
            ObjectStateManager manager = this.ObjectContext.ObjectStateManager;

            bool addNewRelationship = false;
            bool addKeyEntry = false;
            EntityEntry existingEntry = manager.FindEntityEntry(DetachedEntityKey);
            if (existingEntry == null)
            {
                // add new key entry and create a relationship with it                
                addKeyEntry = true;
                addNewRelationship = true;
            }
            else
            {
                if (existingEntry.IsKeyEntry)
                {
                    // We have an existing key entry, so just need to add a relationship with it

                    // We know the target end of this relationship is 1..1 or 0..1 since it is a reference, so if the source end is also not Many, we have a 1-to-1
                    if (FromEndProperty.RelationshipMultiplicity != RelationshipMultiplicity.Many)
                    {
                        // before we add a new relationship to this key entry, make sure it's not already related to something else
                        // We have to explicitly do this here because there are no other checks to make sure a key entry in a 1-to-1 doesn't end up in two of the same relationship
                        foreach (RelationshipEntry relationshipEntry in this.ObjectContext.ObjectStateManager.FindRelationshipsByKey(DetachedEntityKey))
                        {
                            // only care about relationships in the same AssociationSet and where the key is playing the same role that it plays in this EntityReference                            
                            if (relationshipEntry.IsSameAssociationSetAndRole((AssociationSet)RelationshipSet, (AssociationEndMember)ToEndMember, DetachedEntityKey) &&
                                relationshipEntry.State != EntityState.Deleted)
                            {
                                throw EntityUtil.EntityConflictsWithKeyEntry();
                            }
                        }
                    }

                    addNewRelationship = true;
                }
                else
                {
                    IEntityWrapper wrappedTarget = existingEntry.WrappedEntity;

                    // Verify that the target entity is in a valid state for adding a relationship
                    if (existingEntry.State == EntityState.Deleted)
                    {
                        throw EntityUtil.UnableToAddRelationshipWithDeletedEntity();
                    }

                    // We know the target end of this relationship is 1..1 or 0..1 since it is a reference, so if the source end is also not Many, we have a 1-to-1
                    RelatedEnd relatedEnd = wrappedTarget.RelationshipManager.GetRelatedEndInternal(RelationshipName, RelationshipNavigation.From);
                    if (FromEndProperty.RelationshipMultiplicity != RelationshipMultiplicity.Many && !relatedEnd.IsEmpty())
                    {
                        // Make sure the target entity is not already related to something else.
                        // devnote: The call to Add below does *not* do this check for the fixup case, so if it's not done here, no failure will occur
                        //          and existing relationships may be deleted unexpectedly. RelatedEnd.Include should not remove existing relationships, only add new ones.
                        throw EntityUtil.EntityConflictsWithKeyEntry();
                    }

                    // We have an existing entity with the same key, just hook up the related ends
                    this.Add(wrappedTarget,
                        applyConstraints: true,
                        addRelationshipAsUnchanged: doAttach,
                        relationshipAlreadyExists: false,
                        allowModifyingOtherEndOfRelationship: true,
                        forceForeignKeyChanges: true);

                    // add to the list of promoted key references so we can cleanup if a failure occurs later
                    manager.TransactionManager.PopulatedEntityReferences.Add(this);
                }
            }

            // For FKs, don't create a key entry and don't create a relationship
            if (addNewRelationship && !IsForeignKey)
            {
                // devnote: If we add any validation here, it needs to go here before adding the key entry,
                //          otherwise we have to clean up that entry if the validation fails

                if (addKeyEntry)
                {
                    EntitySet targetEntitySet = DetachedEntityKey.GetEntitySet(this.ObjectContext.MetadataWorkspace);
                    manager.AddKeyEntry(DetachedEntityKey, targetEntitySet);
                }

                EntityKey ownerKey = WrappedOwner.EntityKey;
                EntityUtil.CheckEntityKeyNull(ownerKey);
                RelationshipWrapper wrapper = new RelationshipWrapper((AssociationSet)RelationshipSet,
                    RelationshipNavigation.From, ownerKey, RelationshipNavigation.To, DetachedEntityKey);
                manager.AddNewRelation(wrapper, doAttach ? EntityState.Unchanged : EntityState.Added);
            }
        }

        internal override void Exclude()
        {
            Debug.Assert(this.ObjectContext != null, "Should not be trying to remove entities from state manager if context is null");

            if (null != _wrappedCachedValue.Entity)
            {
                // It is possible that _cachedValue was originally null in this graph, but was only set
                // while the graph was being added, if the DetachedEntityKey matched its key. In that case,
                // we only want to clear _cachedValue and delete the relationship entry, but not remove the entity
                // itself from the context.
                TransactionManager transManager = ObjectContext.ObjectStateManager.TransactionManager;
                bool doFullRemove = transManager.PopulatedEntityReferences.Contains(this);
                bool doRelatedEndRemove = transManager.AlignedEntityReferences.Contains(this);
                // For POCO, if the entity is undergoing snapshot for the first time, then in this step we actually
                // need to really exclude it rather than just disconnecting it.  If we don't, then it has the potential
                // to remain in the context at the end of the rollback process.
                if ((transManager.ProcessedEntities == null || !transManager.ProcessedEntities.Contains(_wrappedCachedValue)) &&
                    (doFullRemove || doRelatedEndRemove))
                {
                    // Retrieve the relationship entry before _cachedValue is set to null during Remove
                    RelationshipEntry relationshipEntry = IsForeignKey ? null : FindRelationshipEntryInObjectStateManager(_wrappedCachedValue);
                    Debug.Assert(IsForeignKey || relationshipEntry != null, "Should have been able to find a valid relationship since _cachedValue is non-null");

                    // Remove the related ends and mark the relationship as deleted, but don't propagate the changes to the target entity itself
                    Remove(_wrappedCachedValue,
                            doFixup: doFullRemove,
                            deleteEntity: false,
                            deleteOwner: false,
                            applyReferentialConstraints: false,
                            preserveForeignKey: true);

                    // The relationship will now either be detached (if it was previously in the Added state), or Deleted (if it was previously Unchanged)
                    // If it's Deleted, we need to AcceptChanges to get rid of it completely                    
                    if (relationshipEntry != null && relationshipEntry.State != EntityState.Detached)
                    {
                        relationshipEntry.AcceptChanges();
                    }

                    // Since this has been processed, remove it from the list
                    if (doFullRemove)
                    {
                        transManager.PopulatedEntityReferences.Remove(this);
                    }
                    else
                    {
                        transManager.AlignedEntityReferences.Remove(this);
                    }
                }
                else
                {
                    ExcludeEntity(_wrappedCachedValue);
                }
            }
            else if (DetachedEntityKey != null)
            {
                // there may still be relationship entries with stubs that need to be removed
                // this works whether we just added the key entry along with the relationship or if it was already existing
                ExcludeEntityKey();
            }
            // else there is nothing to remove for this relationship
        }

        private void ExcludeEntityKey()
        {
            EntityKey ownerKey = WrappedOwner.EntityKey;

            RelationshipEntry relationshipEntry = this.ObjectContext.ObjectStateManager.FindRelationship(RelationshipSet,
                new KeyValuePair<string, EntityKey>(RelationshipNavigation.From, ownerKey),
                new KeyValuePair<string, EntityKey>(RelationshipNavigation.To, DetachedEntityKey));

            // we may have failed in adding the graph before we actually added this relationship, so make sure we actually found one
            if (relationshipEntry != null)
            {
                relationshipEntry.Delete(/*doFixup*/ false);
                // If entry was Added before, it is now Detached, otherwise AcceptChanges to detach it
                if (relationshipEntry.State != EntityState.Detached)
                {
                    relationshipEntry.AcceptChanges();
                }
            }
        }

        internal override void ClearCollectionOrRef(IEntityWrapper wrappedEntity, RelationshipNavigation navigation, bool doCascadeDelete)
        {
            if (wrappedEntity == null)
            {
                wrappedEntity = EntityWrapperFactory.NullWrapper;
            }
            if (null != _wrappedCachedValue.Entity)
            {
                // Following condition checks if we have already visited this graph node. If its true then
                // we should not do fixup because that would cause circular loop
                if ((wrappedEntity.Entity == _wrappedCachedValue.Entity) && (navigation.Equals(this.RelationshipNavigation)))
                {
                    Remove(_wrappedCachedValue, /*fixup*/false, /*deleteEntity*/false, /*deleteOwner*/false, /*applyReferentialConstraints*/false, /*preserveForeignKey*/false);
                }
                else
                {
                    Remove(_wrappedCachedValue, /*fixup*/true, doCascadeDelete, /*deleteOwner*/false, /*applyReferentialConstraints*/true, /*preserveForeignKey*/false);
                }
            }
            else
            {
                // this entity reference could be replacing a relationship that points to a key entry
                // we need to search relationships on the Owner entity to see if this is true, and if so remove the relationship entry
                if (WrappedOwner.Entity != null && WrappedOwner.Context != null && !UsingNoTracking)
                {
                    EntityEntry ownerEntry = WrappedOwner.Context.ObjectStateManager.GetEntityEntry(WrappedOwner.Entity);
                    ownerEntry.DeleteRelationshipsThatReferenceKeys(this.RelationshipSet, this.ToEndMember);
                }
            }

            // If we have an Owner, clear the DetachedEntityKey.
            // If we do not have an owner, retain the key so that we can resolve the difference when the entity is attached to a context
            if (this.WrappedOwner.Entity != null)
            {
                // Clear the detachedEntityKey as well. In cases where we have to fix up the detachedEntityKey, we will not always be able to detect
                // if we have *only* a Deleted relationship for a given entity/relationship/role, so clearing this here will ensure that
                // even if no other relationships are added, the key value will still be correct.
                ((EntityReference)this).DetachedEntityKey = null;
            }
        }

        internal override void ClearWrappedValues()
        {
            this._cachedValue = null;
            this._wrappedCachedValue = NullEntityWrapper.NullWrapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="relationshipAlreadyExists"></param>
        /// <returns>True if the verify succeeded, False if the Add should no-op</returns>
        internal override bool VerifyEntityForAdd(IEntityWrapper wrappedEntity, bool relationshipAlreadyExists)
        {
            if (!relationshipAlreadyExists && this.ContainsEntity(wrappedEntity))
            {
                return false;
            }

            this.VerifyType(wrappedEntity);

            return true;
        }

        internal override bool CanSetEntityType(IEntityWrapper wrappedEntity)
        {
            return wrappedEntity.Entity is TEntity;
        }

        internal override void VerifyType(IEntityWrapper wrappedEntity)
        {
            if (!CanSetEntityType(wrappedEntity))
            {
                throw EntityUtil.InvalidContainedTypeReference(wrappedEntity.Entity.GetType().FullName, typeof(TEntity).FullName);
            }
        }

        /// <summary>
        /// Disconnected adds are not supported for an EntityReference so we should report this as an error.
        /// </summary>
        /// <param name="entity">The entity to add to the related end in a disconnected state.</param>
        internal override void DisconnectedAdd(IEntityWrapper wrappedEntity)
        {
            CheckOwnerNull();
        }

        /// <summary>
        /// Disconnected removes are not supported for an EntityReference so we should report this as an error.
        /// </summary>
        /// <param name="entity">The entity to remove from the related end in a disconnected state.</param>
        internal override bool DisconnectedRemove(IEntityWrapper wrappedEntity)
        {
            CheckOwnerNull();
            return false;
        }

        /// <summary>
        /// Remove from the RelatedEnd
        /// </summary>
        /// <param name="wrappedEntity"></param>
        /// <param name="resetIsLoaded"></param>
        /// <returns></returns>
        internal override bool RemoveFromLocalCache(IEntityWrapper wrappedEntity, bool resetIsLoaded, bool preserveForeignKey)
        {
            Debug.Assert(wrappedEntity != null, "IEntityWrapper instance is null.");
            Debug.Assert(null == _wrappedCachedValue.Entity || wrappedEntity.Entity == _wrappedCachedValue.Entity, "The specified object is not a part of this relationship.");

            _wrappedCachedValue = EntityWrapperFactory.NullWrapper;
            _cachedValue = null;

            if (resetIsLoaded)
            {
                _isLoaded = false;
            }

            // This code sets nullable FK properties on a dependent end to null when a relationship has been nulled.
            if (ObjectContext != null && IsForeignKey && !preserveForeignKey)
            {
                NullAllForeignKeys();
            }
            return true;
        }

        /// <summary>
        /// Remove from the POCO collection
        /// </summary>
        /// <param name="wrappedEntity"></param>
        /// <returns></returns>
        internal override bool RemoveFromObjectCache(IEntityWrapper wrappedEntity)
        {
            Debug.Assert(wrappedEntity != null, "IEntityWrapper instance is null.");

            // For POCO entities - clear the CLR reference
            if (this.TargetAccessor.HasProperty)
            {
                this.WrappedOwner.RemoveNavigationPropertyValue(this, (TEntity)wrappedEntity.Entity);
            }

            return true;
        }

        // Method used to retrieve properties from principal entities.
        // NOTE: 'properties' list is modified in this method and may already contains some properties.
        internal override void RetrieveReferentialConstraintProperties(Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited)
        {
            Debug.Assert(properties != null);

            if (this._wrappedCachedValue.Entity != null)
            {
                // Dictionary< propertyName, <propertyValue, counter>>
                Dictionary<string, KeyValuePair<object, IntBox>> retrievedProperties;

                // PERFORMANCE: ReferentialConstraints collection in typical scenario is very small (1-3 elements)
                foreach (ReferentialConstraint constraint in ((AssociationType)this.RelationMetadata).ReferentialConstraints)
                {
                    if (constraint.ToRole == FromEndProperty)
                    {
                        // Detect circular references
                        if (visited.Contains(_wrappedCachedValue))
                        {
                            throw EntityUtil.CircularRelationshipsWithReferentialConstraints();
                        }
                        visited.Add(_wrappedCachedValue);

                        _wrappedCachedValue.RelationshipManager.RetrieveReferentialConstraintProperties(out retrievedProperties, visited, includeOwnValues: true);

                        Debug.Assert(retrievedProperties != null);
                        Debug.Assert(constraint.FromProperties.Count == constraint.ToProperties.Count, "Referential constraints From/To properties list have different size");

                        // Following loop rewrites properties from "retrievedProperties" into "properties".
                        // At the same time, property's name is translated from name from principal end into name from dependent end:
                        // Example: Client<C_ID> - Order<O_ID, Client_ID>   
                        //          Client is principal end, Order is dependent end, Client.C_ID == Order.Client_ID
                        // Input : retrievedProperties = { "C_ID" = 123 }
                        // Output: properties = { "Client_ID" = 123 }

                        // NOTE order of properties in collections constraint.From/ToProperties is important
                        for (int i = 0; i < constraint.FromProperties.Count; ++i)
                        {
                            EntityEntry.AddOrIncreaseCounter(
                                    properties,
                                    constraint.ToProperties[i].Name,
                                    retrievedProperties[constraint.FromProperties[i].Name].Key);
                        }
                    }
                }
            }
        }

        internal override bool IsEmpty()
        {
            return _wrappedCachedValue.Entity == null;
        }

        internal override void VerifyMultiplicityConstraintsForAdd(bool applyConstraints)
        {
            if (applyConstraints && !this.IsEmpty())
            {
                throw EntityUtil.CannotAddMoreThanOneEntityToEntityReference(this.RelationshipNavigation.To, this.RelationshipNavigation.RelationshipName);
            }
        }

        // Update IsLoaded flag if necessary
        // This method is called when Clear() was called on the other end of relationship (if the other end is EntityCollection)
        // or when Value property of the other end was set to null (if the other end is EntityReference).
        // This method is used only when NoTracking option was used.
        internal override void OnRelatedEndClear()
        {
            // If other end of relationship was loaded, it mean that this end was also cleared.
            _isLoaded = false;
        }

        internal override bool ContainsEntity(IEntityWrapper wrappedEntity)
        {
            // Using operator 'as' instead of () allows calling ContainsEntity
            // with entity of different type than TEntity.
            return null != _wrappedCachedValue.Entity && _wrappedCachedValue.Entity == wrappedEntity.Entity;
        }

        // Identical code is in EntityCollection, but this can't be moved to the base class because it relies on the
        // knowledge of the generic type, and the base class isn't generic
        public ObjectQuery<TEntity> CreateSourceQuery()
        {
            CheckOwnerNull();
            bool hasResults;
            return CreateSourceQuery<TEntity>(DefaultMergeOption, out hasResults);
        }

        internal override IEnumerable CreateSourceQueryInternal()
        {
            return CreateSourceQuery();
        }
        //End identical code

        /// <summary>
        /// Take any values in the incoming RelatedEnd and sets them onto the values 
        /// that currently exist in this RelatedEnd
        /// </summary>
        /// <param name="rhs"></param>
        internal void InitializeWithValue(RelatedEnd relatedEnd)
        {
            Debug.Assert(this._wrappedCachedValue.Entity == null, "The EntityReference already has a value.");
            EntityReference<TEntity> reference = relatedEnd as EntityReference<TEntity>;
            if (reference != null && reference._wrappedCachedValue.Entity != null)
            {
                _wrappedCachedValue = reference._wrappedCachedValue;
                _cachedValue = (TEntity)_wrappedCachedValue.Entity;
            }
        }

        internal override bool CheckIfNavigationPropertyContainsEntity(IEntityWrapper wrapper)
        {
            Debug.Assert(this.RelationshipNavigation != null, "null RelationshipNavigation");

            // If the navigation property doesn't exist (e.g. unidirectional prop), then it can't contain the entity.
            if (!TargetAccessor.HasProperty)
            {
                return false;
            }

            object value = this.WrappedOwner.GetNavigationPropertyValue(this);

            return Object.Equals(value, wrapper.Entity);
        }

        internal override void VerifyNavigationPropertyForAdd(IEntityWrapper wrapper)
        {
            if (this.TargetAccessor.HasProperty)
            {
                object value = WrappedOwner.GetNavigationPropertyValue(this);
                if (!Object.ReferenceEquals(null, value) && !Object.Equals(value, wrapper.Entity))
                {
                    throw EntityUtil.CannotAddMoreThanOneEntityToEntityReference(
                        this.RelationshipNavigation.To, this.RelationshipNavigation.RelationshipName);
                }
            }
        }

        // This method is required to maintain compatibility with the v1 binary serialization format. 
        // In particular, it recreates a entity wrapper from the serialized cached value.
        // Note that this is only expected to work for non-POCO entities, since serialization of POCO
        // entities will not result in serialization of the RelationshipManager or its related objects.
        [OnDeserialized()]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnRefDeserialized(StreamingContext context)
        {
            _wrappedCachedValue = EntityWrapperFactory.WrapEntityUsingContext(_cachedValue, ObjectContext);
        }

        [OnSerializing()]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnSerializing(StreamingContext context)
        {
            if (!(WrappedOwner.Entity is IEntityWithRelationships))
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.RelatedEnd_CannotSerialize("EntityReference"));
            }
        }

        #region Add

        /// <summary>
        /// AddToLocalEnd is used by both APIs a) RelatedEnd.Add b) Value property setter.
        /// ApplyConstraints is true in case of RelatedEnd.Add because one cannot add entity to ref it its already set
        /// however applyConstraints is false in case of Value property setter because value can be set to a new value
        /// even if its non null.
        /// </summary>
        internal override void AddToLocalCache(IEntityWrapper wrappedEntity, bool applyConstraints)
        {
            if (wrappedEntity != _wrappedCachedValue)
            {
                TransactionManager tm = ObjectContext != null ? ObjectContext.ObjectStateManager.TransactionManager : null;
                if (applyConstraints && null != _wrappedCachedValue.Entity)
                {
                    // The idea here is that we want to throw for constraint violations in things that we are bringing in,
                    // but not when replacing references of things already in the context.  Therefore, if the the thing that
                    // we're replacing is in ProcessedEntities it means we're bringing it in and we should throw.
                    if (tm == null || tm.ProcessedEntities == null || tm.ProcessedEntities.Contains(_wrappedCachedValue))
                    {
                        throw EntityUtil.CannotAddMoreThanOneEntityToEntityReference(this.RelationshipNavigation.To, this.RelationshipNavigation.RelationshipName);
                    }
                }
                if (tm != null && wrappedEntity.Entity != null)
                {
                    // Setting this flag will prevent the FK from being temporarily set to null while changing
                    // it from one value to the next.
                    tm.BeginRelatedEndAdd();
                }
                try
                {
                    ClearCollectionOrRef(null, null, false);
                    _wrappedCachedValue = wrappedEntity;
                    _cachedValue = (TEntity)wrappedEntity.Entity;
                }
                finally
                {
                    if (tm != null && tm.IsRelatedEndAdd)
                    {
                        tm.EndRelatedEndAdd();
                    }
                }
            }
        }

        internal override void AddToObjectCache(IEntityWrapper wrappedEntity)
        {
            // For POCO entities - set the CLR reference
            if (this.TargetAccessor.HasProperty)
            {
                this.WrappedOwner.SetNavigationPropertyValue(this, wrappedEntity.Entity);
            }
        }

        #endregion
    }
}

