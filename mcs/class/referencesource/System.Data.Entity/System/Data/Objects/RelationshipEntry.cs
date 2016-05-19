//---------------------------------------------------------------------
// <copyright file="RelationshipEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
namespace System.Data.Objects
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Data.Objects.Internal;
    using System.Diagnostics;

    internal sealed class RelationshipEntry : ObjectStateEntry
    {
        internal RelationshipWrapper _relationshipWrapper;
        internal EntityKey Key0 { get { return RelationshipWrapper.Key0; } }
        internal EntityKey Key1 { get { return RelationshipWrapper.Key1; } }
        internal override System.Collections.BitArray ModifiedProperties
        {
            get { return null; }
        }

        #region Linked list of related relationships
        private RelationshipEntry _nextKey0;
        private RelationshipEntry _nextKey1;
        #endregion

        #region Constructors
        internal RelationshipEntry(ObjectStateManager cache, EntityState state, RelationshipWrapper relationshipWrapper)
            : base(cache, null, state)
        {
            Debug.Assert(null != relationshipWrapper, "null RelationshipWrapper");
            Debug.Assert(EntityState.Added == state ||
                         EntityState.Unchanged == state ||
                         EntityState.Deleted == state,
                         "invalid EntityState");

            base._entitySet = relationshipWrapper.AssociationSet;
            _relationshipWrapper = relationshipWrapper;
        }
        #endregion

        #region Public members

        /// <summary>
        /// API to accept the current values as original values and  mark the entity as Unchanged.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        override public bool IsRelationship
        {
            get
            {
                ValidateState();
                return true;
            }
        }

        override public void AcceptChanges()
        {
            ValidateState();

            switch (State)
            {
                case EntityState.Deleted:
                    DeleteUnnecessaryKeyEntries();
                    // Current entry could be already detached if this is relationship entry and if one end of relationship was a KeyEntry
                    if (_cache != null)
                    {
                        _cache.ChangeState(this, EntityState.Deleted, EntityState.Detached);
                    }
                    break;
                case EntityState.Added:
                    _cache.ChangeState(this, EntityState.Added, EntityState.Unchanged);
                    State = EntityState.Unchanged;
                    break;
                case EntityState.Modified:
                    Debug.Assert(false, "RelationshipEntry cannot be in Modified state");
                    break;
                case EntityState.Unchanged:
                    break;
            }
        }

        override public void Delete()
        {
            // doFixup flag is used for Cache and Collection & Ref consistency
            // When some entity is deleted if "doFixup" is true then Delete method
            // calls the Collection & Ref code to do the necessary fix-ups.
            // "doFixup" equals to False is only called from EntityCollection & Ref code
            Delete(/*doFixup*/true);
        }

        override public IEnumerable<string> GetModifiedProperties()
        {
            ValidateState();
            yield break;
        }

        override public void SetModified()
        {
            ValidateState();
            throw EntityUtil.CantModifyRelationState();
        }

        override public object Entity
        {
            get
            {
                ValidateState();
                return null;
            }
        }

        override public EntityKey EntityKey
        {
            get
            {
                ValidateState();
                return null;
            }
            internal set
            {
                // no-op for entires other than EntityEntry
                Debug.Assert(false, "EntityKey setter shouldn't be called for RelationshipEntry");
            }
        }

        /// <summary>
        /// Marks specified property as modified.
        /// </summary>
        /// <param name="propertyName">This API recognizes the names in terms of OSpace</param>
        /// <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        ///
        override public void SetModifiedProperty(string propertyName)
        {
            ValidateState();

            throw EntityUtil.CantModifyRelationState();
        }

        /// <summary>
        /// Throws since the method has no meaning for relationship entries.
        /// </summary>
        override public void RejectPropertyChanges(string propertyName)
        {
            ValidateState();

            throw EntityUtil.CantModifyRelationState();
        }

        /// <summary>
        /// Throws since the method has no meaning for relationship entries.
        /// </summary>
        public override bool IsPropertyChanged(string propertyName)
        {
            ValidateState();

            throw EntityUtil.CantModifyRelationState();
        }

        /// <summary>
        /// Original values of entity
        /// </summary>
        /// <param></param>
        /// <returns> DbDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't have debugger view expand this
        override public DbDataRecord OriginalValues
        {
            get
            {
                ValidateState();
                if (this.State == EntityState.Added)
                {
                    throw EntityUtil.OriginalValuesDoesNotExist();
                }

                return new ObjectStateEntryDbDataRecord(this);
            }        
        }

        public override OriginalValueRecord GetUpdatableOriginalValues()
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        /// <summary>
        /// Current values of entity/ DataRow
        /// </summary>
        /// <param></param>
        /// <returns> DbUpdatableDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't have debugger view expand this
        override public CurrentValueRecord CurrentValues
        {
            get
            {
                ValidateState();
                if (this.State == EntityState.Deleted)
                {
                    throw EntityUtil.CurrentValuesDoesNotExist();
                }

                return new ObjectStateEntryDbUpdatableDataRecord(this);
            }
        }

        override public RelationshipManager RelationshipManager
        {
            get
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.ObjectStateEntry_RelationshipAndKeyEntriesDoNotHaveRelationshipManagers);
            }
        }

        public override void ChangeState(EntityState state)
        {
            EntityUtil.CheckValidStateForChangeRelationshipState(state, "state");

            if (this.State == EntityState.Detached && state == EntityState.Detached)
            {
                return;
            }

            ValidateState();

            if (this.RelationshipWrapper.Key0 == this.Key0)
            {
                this.ObjectStateManager.ChangeRelationshipState(
                    this.Key0, this.Key1,
                    this.RelationshipWrapper.AssociationSet.ElementType.FullName,
                    this.RelationshipWrapper.AssociationEndMembers[1].Name,
                    state);
            }
            else
            {
                Debug.Assert(this.RelationshipWrapper.Key0 == this.Key1, "invalid relationship");
                this.ObjectStateManager.ChangeRelationshipState(
                    this.Key0, this.Key1,
                    this.RelationshipWrapper.AssociationSet.ElementType.FullName,
                    this.RelationshipWrapper.AssociationEndMembers[0].Name,
                    state);
            }
        }

        public override void ApplyCurrentValues(object currentEntity)
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        public override void ApplyOriginalValues(object originalEntity)
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        #endregion

        #region ObjectStateEntry members

        override internal bool IsKeyEntry
        {
            get
            {
                return false;
            }
        }

        override internal int GetFieldCount(StateManagerTypeMetadata metadata)
        {
            return _relationshipWrapper.AssociationEndMembers.Count;
        }

        /// <summary>
        /// Reuse or create a new (Entity)DataRecordInfo.
        /// </summary>
        override internal DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject)
        {
            //Dev Note: RelationshipType always has default facets. Thus its safe to construct a TypeUsage from EdmType
            return new DataRecordInfo(TypeUsage.Create(((RelationshipSet)EntitySet).ElementType));
        }

        override internal void SetModifiedAll()
        {
            ValidateState();
            throw EntityUtil.CantModifyRelationState();
        }

        override internal Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata)
        {
            // 'metadata' is used for ComplexTypes in EntityEntry

            return typeof(EntityKey); // this is given By Design
        }

        override internal string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata)
        {
            ValidateRelationshipRange(ordinal);
            return _relationshipWrapper.AssociationEndMembers[ordinal].Name;
        }

        override internal int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata)
        {
            AssociationEndMember endMember;
            ReadOnlyMetadataCollection<AssociationEndMember> endMembers = _relationshipWrapper.AssociationEndMembers;
            if (endMembers.TryGetValue(name, false, out endMember))
            {
                return endMembers.IndexOf(endMember);
            }
            return -1;
        }

        override internal void RevertDelete()
        {
            State = EntityState.Unchanged;
            _cache.ChangeState(this, EntityState.Deleted, State);
        }

        /// <summary>
        /// Used to report that a scalar entity property is about to change
        /// The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that is changing</param>
        override internal void EntityMemberChanging(string entityMemberName)
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        /// <summary>
        /// Used to report that a scalar entity property has been changed
        /// The property value that was cached during EntityMemberChanging is now
        /// added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that has changing</param>
        override internal void EntityMemberChanged(string entityMemberName)
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        /// <summary>
        /// Used to report that a complex property is about to change
        /// The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that is changing</param>
        /// <param name="complexObject">The complex object that contains the property that is changing</param>
        /// <param name="complexObjectMemberName">The name of the property that is changing on complexObject</param>
        override internal void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        /// <summary>
        /// Used to report that a complex property has been changed
        /// The property value that was cached during EntityMemberChanging is now added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that has changed</param>
        /// <param name="complexObject">The complex object that contains the property that changed</param>
        /// <param name="complexObjectMemberName">The name of the property that changed on complexObject</param>
        override internal void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            throw EntityUtil.CantModifyRelationValues();
        }

        #endregion

        // Helper method to determine if the specified entityKey is in the given role and AssociationSet in this relationship entry
        internal bool IsSameAssociationSetAndRole(AssociationSet associationSet, AssociationEndMember associationMember, EntityKey entityKey)
        {
            Debug.Assert(associationSet.ElementType.AssociationEndMembers[0].Name == associationMember.Name ||
                         associationSet.ElementType.AssociationEndMembers[1].Name == associationMember.Name,
                         "Expected associationMember to be one of the ends of the specified associationSet.");

            if (!Object.ReferenceEquals(_entitySet, associationSet))
            {
                return false;
            }

            // Find the end of the relationship that corresponds to the associationMember and see if it matches the EntityKey we are looking for
            if (_relationshipWrapper.AssociationSet.ElementType.AssociationEndMembers[0].Name == associationMember.Name)
            {
                return entityKey == Key0;
            }
            else
            {
                return entityKey == Key1;
            }
        }

        private object GetCurrentRelationValue(int ordinal, bool throwException)
        {
            ValidateRelationshipRange(ordinal);
            ValidateState();
            if (State == EntityState.Deleted && throwException)
            {
                throw EntityUtil.CurrentValuesDoesNotExist();
            }
            return _relationshipWrapper.GetEntityKey(ordinal);
        }

        private static void ValidateRelationshipRange(int ordinal)
        {
            if (unchecked(1u < (uint)ordinal))
            {
                throw EntityUtil.ArgumentOutOfRange("ordinal");
            }
        }

        internal object GetCurrentRelationValue(int ordinal)
        {
            return GetCurrentRelationValue(ordinal, true);
        }

        internal RelationshipWrapper RelationshipWrapper
        {
            get
            {
                return _relationshipWrapper;
            }
            set
            {
                Debug.Assert(null != value, "don't set wrapper to null");
                _relationshipWrapper = value;
            }
        }

        override internal void Reset()
        {
            _relationshipWrapper = null;

            base.Reset();
        }

        /// <summary>
        /// Update one of the ends of the relationship
        /// </summary>
        internal void ChangeRelatedEnd(EntityKey oldKey, EntityKey newKey)
        {
            if (oldKey.Equals(Key0))
            {
                if (oldKey.Equals(Key1))
                {   // self-reference
                    RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper.AssociationSet, newKey);
                }
                else
                {
                    RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper, 0, newKey);
                }
            }
            else
            {
                RelationshipWrapper = new RelationshipWrapper(RelationshipWrapper, 1, newKey);
            }
        }

        internal void DeleteUnnecessaryKeyEntries()
        {
            // We need to check to see if the ends of the relationship are key entries.
            // If they are, and nothing else refers to them then the key entry should be removed.
            for (int i = 0; i < 2; i++)
            {
                EntityKey entityKey = this.GetCurrentRelationValue(i, false) as EntityKey;
                EntityEntry relatedEntry = _cache.GetEntityEntry(entityKey);
                if (relatedEntry.IsKeyEntry)
                {
                    bool foundRelationship = false;
                    // count the number of relationships this key entry is part of
                    // if there aren't any, then the relationship should be deleted
                    foreach (RelationshipEntry relationshipEntry in _cache.FindRelationshipsByKey(entityKey))
                    {
                        // only count relationships that are not the one we are currently deleting (i.e. this)
                        if (relationshipEntry != this)
                        {
                            foundRelationship = true;
                            break;
                        }
                    }
                    if (!foundRelationship)
                    {
                        // Nothing is refering to this key entry, so it should be removed from the cache
                        _cache.DeleteKeyEntry(relatedEntry);
                        // We assume that only one end of relationship can be a key entry,
                        // so we can break the loop
                        break;
                    }
                }
            }
        }

        //"doFixup" equals to False is called from EntityCollection & Ref code only
        internal void Delete(bool doFixup)
        {
            ValidateState();

            if (doFixup)
            {
                if (State != EntityState.Deleted)  //for deleted ObjectStateEntry its a no-op
                {
                    //Find two ends of the relationship
                    EntityEntry entry1 = _cache.GetEntityEntry((EntityKey)GetCurrentRelationValue(0));
                    IEntityWrapper wrappedEntity1 = entry1.WrappedEntity;
                    EntityEntry entry2 = _cache.GetEntityEntry((EntityKey)GetCurrentRelationValue(1));
                    IEntityWrapper wrappedEntity2 = entry2.WrappedEntity;

                    // If one end of the relationship is a KeyEntry, entity1 or entity2 is null.
                    // It is not possible that both ends of relationship are KeyEntries.
                    if (wrappedEntity1.Entity != null && wrappedEntity2.Entity != null)
                    {
                        // Obtain the ro role name and relationship name
                        // We don't create a full NavigationRelationship here because that would require looking up
                        // additional information like property names that we don't need.
                        ReadOnlyMetadataCollection<AssociationEndMember> endMembers = _relationshipWrapper.AssociationEndMembers;
                        string toRole = endMembers[1].Name;
                        string relationshipName = ((AssociationSet)_entitySet).ElementType.FullName;
                        wrappedEntity1.RelationshipManager.RemoveEntity(toRole, relationshipName, wrappedEntity2);
                    }
                    else
                    {
                        // One end of relationship is a KeyEntry, figure out which one is the real entity and get its RelationshipManager
                        // so we can update the DetachedEntityKey on the EntityReference associated with this relationship
                        EntityKey targetKey = null;
                        RelationshipManager relationshipManager = null;
                        if (wrappedEntity1.Entity == null)
                        {
                            targetKey = entry1.EntityKey;
                            relationshipManager = wrappedEntity2.RelationshipManager;
                        }
                        else
                        {
                            targetKey = entry2.EntityKey;
                            relationshipManager = wrappedEntity1.RelationshipManager;
                        }
                        Debug.Assert(relationshipManager != null, "Entity wrapper returned a null RelationshipManager");

                        // Clear the detachedEntityKey as well. In cases where we have to fix up the detachedEntityKey, we will not always be able to detect
                        // if we have *only* a Deleted relationship for a given entity/relationship/role, so clearing this here will ensure that
                        // even if no other relationships are added, the key value will still be correct and we won't accidentally pick up an old value.

                        // devnote: Since we know the target end of this relationship is a key entry, it has to be a reference, so just cast
                        AssociationEndMember targetMember = this.RelationshipWrapper.GetAssociationEndMember(targetKey);
                        EntityReference entityReference = (EntityReference)relationshipManager.GetRelatedEndInternal(targetMember.DeclaringType.FullName, targetMember.Name);
                        entityReference.DetachedEntityKey = null;

                        // Now update the state
                        if (this.State == EntityState.Added)
                        {
                            // Remove key entry if necessary
                            DeleteUnnecessaryKeyEntries();
                            // Remove relationship entry
                            // devnote: Using this method instead of just changing the state because the entry
                            //          may have already been detached along with the key entry above. However,
                            //          if there were other relationships using the key, it would not have been deleted.
                            DetachRelationshipEntry();
                        }
                        else
                        {
                            // Non-added entries should be deleted
                            _cache.ChangeState(this, this.State, EntityState.Deleted);
                            State = EntityState.Deleted;
                        }
                    }
                }
            }
            else
            {
                switch (State)
                {
                    case EntityState.Added:
                        // Remove key entry if necessary
                        DeleteUnnecessaryKeyEntries();
                        // Remove relationship entry
                        // devnote: Using this method instead of just changing the state because the entry
                        //          may have already been detached along with the key entry above. However,
                        //          if there were other relationships using the key, it would not have been deleted.
                        DetachRelationshipEntry();
                        break;
                    case EntityState.Modified:
                        Debug.Assert(false, "RelationshipEntry cannot be in Modified state");
                        break;
                    case EntityState.Unchanged:
                        _cache.ChangeState(this, EntityState.Unchanged, EntityState.Deleted);
                        State = EntityState.Deleted;
                        break;
                    //case DataRowState.Deleted:  no-op
                }
            }
        }

        internal object GetOriginalRelationValue(int ordinal)
        {
            return GetCurrentRelationValue(ordinal, false);
        }

        internal void DetachRelationshipEntry()
        {
            // no-op if already detached
            if (_cache != null)
            {
                _cache.ChangeState(this, this.State, EntityState.Detached);
            }
        }

        internal void ChangeRelationshipState(EntityEntry targetEntry, RelatedEnd relatedEnd, EntityState requestedState)
        {
            Debug.Assert(requestedState != EntityState.Modified, "Invalid requested state for relationsihp");
            Debug.Assert(this.State != EntityState.Modified, "Invalid initial state for relationsihp");

            EntityState initialState = this.State;

            switch (initialState)
            {
                case EntityState.Added:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            // no-op
                            break;
                        case EntityState.Unchanged:
                            this.AcceptChanges();
                            break;
                        case EntityState.Deleted:
                            this.AcceptChanges();
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            this.Delete();
                            break;
                        case EntityState.Detached:
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            this.Delete();
                            break;
                        default:
                            Debug.Assert(false, "Invalid requested state");
                            break;
                    }
                    break;
                case EntityState.Unchanged:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            this.ObjectStateManager.ChangeState(this, EntityState.Unchanged, EntityState.Added);
                            this.State = EntityState.Added;
                            break;
                        case EntityState.Unchanged:
                            //no-op
                            break;
                        case EntityState.Deleted:
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            this.Delete();
                            break;
                        case EntityState.Detached:
                            // cascade deletion is not performed because TransactionManager.IsLocalPublicAPI == true
                            this.Delete();
                            this.AcceptChanges();
                            break;
                        default:
                            Debug.Assert(false, "Invalid requested state");
                            break;
                    }
                    break;
                case EntityState.Deleted:
                    switch (requestedState)
                    {
                        case EntityState.Added:
                            relatedEnd.Add(targetEntry.WrappedEntity,
                                applyConstraints: true,   
                                addRelationshipAsUnchanged: false, 
                                relationshipAlreadyExists: true,   
                                allowModifyingOtherEndOfRelationship: false,
                                forceForeignKeyChanges: true); 
                            this.ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Added);
                            this.State = EntityState.Added;
                            break;
                        case EntityState.Unchanged:
                            relatedEnd.Add(targetEntry.WrappedEntity,
                                applyConstraints: true,
                                addRelationshipAsUnchanged: false,
                                relationshipAlreadyExists: true,
                                allowModifyingOtherEndOfRelationship: false,
                                forceForeignKeyChanges: true); 
                            this.ObjectStateManager.ChangeState(this, EntityState.Deleted, EntityState.Unchanged);
                            this.State = EntityState.Unchanged;
                            break;
                        case EntityState.Deleted:
                            // no-op
                            break;
                        case EntityState.Detached:
                            this.AcceptChanges();
                            break;
                        default:
                            Debug.Assert(false, "Invalid requested state");
                            break;
                    }
                    break;
                default:
                    Debug.Assert(false, "Invalid entry state");
                    break;
            }
        }


        #region RelationshipEnds as singly-linked list

        internal RelationshipEntry GetNextRelationshipEnd(EntityKey entityKey)
        {
            Debug.Assert(null != (object)entityKey, "null EntityKey");
            Debug.Assert(entityKey.Equals(Key0) || entityKey.Equals(Key1), "EntityKey mismatch");
            return (entityKey.Equals(Key0) ? NextKey0 : NextKey1);
        }

        internal void SetNextRelationshipEnd(EntityKey entityKey, RelationshipEntry nextEnd)
        {
            Debug.Assert(null != (object)entityKey, "null EntityKey");
            Debug.Assert(entityKey.Equals(Key0) || entityKey.Equals(Key1), "EntityKey mismatch");
            if (entityKey.Equals(Key0))
            {
                NextKey0 = nextEnd;
            }
            else
            {
                NextKey1 = nextEnd;
            }
        }

        /// <summary>
        /// Use when EntityEntry.EntityKey == this.Wrapper.Key0
        /// </summary>
        internal RelationshipEntry NextKey0
        {
            get { return _nextKey0; }
            set { _nextKey0 = value; }

        }

        /// <summary>
        /// Use when EntityEntry.EntityKey == this.Wrapper.Key1
        /// </summary>
        internal RelationshipEntry NextKey1
        {
            get { return _nextKey1; }
            set { _nextKey1 = value; }
        }
        #endregion
    }
}
