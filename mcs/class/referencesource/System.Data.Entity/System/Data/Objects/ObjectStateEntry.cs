//---------------------------------------------------------------------
// <copyright file="ObjectStateEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Collections;

namespace System.Data.Objects
{
    // Detached - nothing

    // Added - _entity & _currentValues only for shadowState

    // Unchanged - _entity & _currentValues only for shadowState
    // Unchanged -> Deleted - _entity & _currentValues only for shadowState

    // Modified - _currentValues & _modifiedFields + _originalValues only on change
    // Modified -> Deleted - _currentValues & _modifiedFields + _originalValues only on change

    /// <summary>
    /// Represets either a entity, entity stub or relationship
    /// </summary>
    public abstract class ObjectStateEntry : IEntityStateEntry, IEntityChangeTracker
    {
        #region common entry fields
        internal ObjectStateManager _cache;
        internal EntitySetBase _entitySet;
        internal EntityState _state;
        #endregion

        #region Constructor
        // ObjectStateEntry will not be detached and creation will be handled from ObjectStateManager
        internal ObjectStateEntry(ObjectStateManager cache, EntitySet entitySet, EntityState state)
        {
            Debug.Assert(cache != null, "cache cannot be null.");

            _cache = cache;
            _entitySet = entitySet;
            _state = state;
        }
        #endregion // Constructor

        #region Public members
        /// <summary>
        /// ObjectStateManager property of ObjectStateEntry.
        /// </summary>
        /// <param></param>
        /// <returns> ObjectStateManager </returns>
        public ObjectStateManager ObjectStateManager
        {
            get
            {
                ValidateState();
                return _cache;
            }
        }

        /// <summary> Extent property of ObjectStateEntry. </summary>
        /// <param></param>
        /// <returns> Extent </returns>
        public EntitySetBase EntitySet
        {
            get
            {
                ValidateState();
                return _entitySet;
            }
        }

        /// <summary>
        /// State property of ObjectStateEntry.
        /// </summary>
        /// <param></param>
        /// <returns> DataRowState </returns>
        public EntityState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                _state = value;
            }
        }

        /// <summary>
        /// Entity property of ObjectStateEntry.
        /// </summary>
        /// <param></param>
        /// <returns> The entity encapsulated by this entry. </returns>
        abstract public object Entity { get; }

        /// <summary>
        /// The EntityKey associated with the ObjectStateEntry
        /// </summary>
        abstract public EntityKey EntityKey { get; internal set; }

        /// <summary>
        /// Determines if this ObjectStateEntry represents a relationship
        /// </summary>
        abstract public bool IsRelationship { get; }

        /// <summary>
        /// Gets bit array indicating which properties are modified.
        /// </summary>
        abstract internal BitArray ModifiedProperties { get; }

        BitArray IEntityStateEntry.ModifiedProperties { get { return this.ModifiedProperties; } }

        /// <summary>
        /// Original values of entity
        /// </summary>
        /// <param></param>
        /// <returns> DbDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't have debugger view expand this
        abstract public DbDataRecord OriginalValues { get; }

        abstract public OriginalValueRecord GetUpdatableOriginalValues();

        /// <summary>
        /// Current values of entity/ DataRow
        /// </summary>
        /// <param></param>
        /// <returns> DbUpdatableDataRecord </returns>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] // don't have debugger view expand this
        abstract public CurrentValueRecord CurrentValues { get; }

        /// <summary>
        /// API to accept the current values as original values and  mark the entity as Unchanged.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        abstract public void AcceptChanges();
        
        /// <summary>
        /// API to mark the entity deleted. if entity is in added state, it will be detached
        /// </summary>
        /// <param></param>
        /// <returns> </returns>
        abstract public void Delete();

        /// <summary>
        /// API to return properties that are marked modified
        /// </summary>
        /// <param> </param>
        /// <returns> IEnumerable of modified properties names, names are in term of c-space </returns>
        abstract public IEnumerable<string> GetModifiedProperties();

        /// <summary>
        /// set the state to Modified.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        ///
        abstract public void SetModified();

        /// <summary>
        /// Marks specified property as modified.
        /// </summary>
        /// <param name="propertyName">This API recognizes the names in terms of OSpace</param>
        /// <exception cref="InvalidOperationException">If State is not Modified or Unchanged</exception>
        ///
        abstract public void SetModifiedProperty(string propertyName);

        /// <summary>
        /// Rejects any changes made to the property with the given name since the property was last loaded,
        /// attached, saved, or changes were accepted. The orginal value of the property is stored and the
        /// property will no longer be marked as modified. 
        /// </summary>
        /// <remarks>
        /// If the result is that no properties of the entity are marked as modified, then the entity will
        /// be marked as Unchanged.
        /// Changes to properties can only rejected for entities that are in the Modified or Unchanged state.
        /// Calling this method for entities in other states (Added, Deleted, or Detached) will result in
        /// an exception being thrown.
        /// Rejecting changes to properties of an Unchanged entity or unchanged properties of a Modifed
        /// is a no-op.
        /// </remarks>
        /// <param name="propertyName">The name of the property to change.</param>
        abstract public void RejectPropertyChanges(string propertyName);

        /// <summary>
        /// Uses DetectChanges to determine whether or not the current value of the property with the given
        /// name is different from its original value. Note that this may be different from the property being
        /// marked as modified since a property which has not changed can still be marked as modified.
        /// </summary>
        /// <remarks>
        /// For complex properties, a new instance of the complex object which has all the same property
        /// values as the original instance is not considered to be different by this method.
        /// </remarks>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property has changed; false otherwise.</returns>
        abstract public bool IsPropertyChanged(string propertyName);

        /// <summary>
        /// Returns the RelationshipManager for the entity represented by this ObjectStateEntry.
        /// Note that a RelationshipManager objects can only be returned if this entry represents a
        /// full entity.  Key-only entries (stubs) and entries representing relationships do not
        /// have associated RelationshipManagers.
        /// </summary>
        /// <exception cref="InvalidOperationException">The entry is a stub or represents a relationship</exception>
        abstract public RelationshipManager RelationshipManager
        {
            get;
        }

        /// <summary>
        /// Changes state of the entry to the specified <paramref name="state"/>
        /// </summary>
        /// <param name="state">The requested state</param>
        abstract public void ChangeState(EntityState state);

        /// <summary>
        /// Apply modified properties to the original object.
        /// </summary>
        /// <param name="current">object with modified properties</param>
        abstract public void ApplyCurrentValues(object currentEntity);

        /// <summary>
        /// Apply original values to the entity.
        /// </summary>
        /// <param name="original">The object with original values</param>
        abstract public void ApplyOriginalValues(object originalEntity);

        #endregion // Public members

        #region IEntityStateEntry
        IEntityStateManager IEntityStateEntry.StateManager
        {
            get
            {
                return (IEntityStateManager)this.ObjectStateManager;
            }
        }

        // must explicitly implement this because interface is internal & so is the property on the
        // class itself -- apparently the compiler won't let anything marked as internal be part of
        // an interface (even if the interface is also internal)
        bool IEntityStateEntry.IsKeyEntry
        {
            get
            {
                return this.IsKeyEntry;
            }
        }
        #endregion // IEntityStateEntry

        #region Public IEntityChangeTracker

        /// <summary>
        /// Used to report that a scalar entity property is about to change
        /// The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that is changing</param>
        void IEntityChangeTracker.EntityMemberChanging(string entityMemberName)
        {
            this.EntityMemberChanging(entityMemberName);
        }

        /// <summary>
        /// Used to report that a scalar entity property has been changed
        /// The property value that was cached during EntityMemberChanging is now
        /// added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that has changing</param>
        void IEntityChangeTracker.EntityMemberChanged(string entityMemberName)
        {
            this.EntityMemberChanged(entityMemberName);
        }

        /// <summary>
        /// Used to report that a complex property is about to change
        /// The current value of the specified property is cached when this method is called.
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that is changing</param>
        /// <param name="complexObject">The complex object that contains the property that is changing</param>
        /// <param name="complexObjectMemberName">The name of the property that is changing on complexObject</param>
        void IEntityChangeTracker.EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            this.EntityComplexMemberChanging(entityMemberName, complexObject, complexObjectMemberName);
        }

        /// <summary>
        /// Used to report that a complex property has been changed
        /// The property value that was cached during EntityMemberChanging is now added to OriginalValues
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that has changed</param>
        /// <param name="complexObject">The complex object that contains the property that changed</param>
        /// <param name="complexObjectMemberName">The name of the property that changed on complexObject</param>
        void IEntityChangeTracker.EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName)
        {
            this.EntityComplexMemberChanged(entityMemberName, complexObject, complexObjectMemberName);
        }

        /// <summary>
        /// Returns the EntityState from the ObjectStateEntry
        /// </summary>
        EntityState IEntityChangeTracker.EntityState
        {
            get
            {
                return this.State;
            }
        }

        #endregion // IEntityChangeTracker

        #region Internal members

        abstract internal bool IsKeyEntry { get; }

        abstract internal int GetFieldCount(StateManagerTypeMetadata metadata);

        abstract internal Type GetFieldType(int ordinal, StateManagerTypeMetadata metadata);

        abstract internal string GetCLayerName(int ordinal, StateManagerTypeMetadata metadata);

        abstract internal int GetOrdinalforCLayerName(string name, StateManagerTypeMetadata metadata);

        abstract internal void RevertDelete();

        abstract internal void SetModifiedAll();

        abstract internal void EntityMemberChanging(string entityMemberName);
        abstract internal void EntityMemberChanged(string entityMemberName);
        abstract internal void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName);
        abstract internal void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName);

        /// <summary>
        /// Reuse or create a new (Entity)DataRecordInfo.
        /// </summary>
        abstract internal DataRecordInfo GetDataRecordInfo(StateManagerTypeMetadata metadata, object userObject);

        virtual internal void Reset()
        {
            _cache = null;
            _entitySet = null;
            _state = EntityState.Detached;
        }

        internal void ValidateState()
        {
            if (_state == EntityState.Detached)
            {
                throw EntityUtil.ObjectStateEntryinInvalidState();
            }
            Debug.Assert(null != _cache, "null ObjectStateManager");
            Debug.Assert(null != _entitySet, "null EntitySetBase");
        }

        #endregion // Internal members
    }

    internal struct StateManagerValue
    {
        internal StateManagerMemberMetadata memberMetadata;
        internal object userObject;
        internal object originalValue;

        internal StateManagerValue(StateManagerMemberMetadata metadata, object instance, object value)
        {
            memberMetadata = metadata;
            userObject = instance;
            originalValue = value;
        }
    }

    internal enum ObjectStateValueRecord
    {
        OriginalReadonly = 0,
        CurrentUpdatable = 1,
        OriginalUpdatableInternal = 2,
        OriginalUpdatablePublic = 3,
    }


    // This class is used in Referential Integrity Constraints feature.
    // It is used to get around the problem of enumerating dictionary contents, 
    // but allowing update of the value without breaking the enumerator.
    internal sealed class IntBox
    {
        private int val;

        internal IntBox(int val)
        {
            this.val = val;
        }

        internal int Value 
        {
            get
            {
                return val;
            }

            set 
            {
                val = value;
            }
        }
    }
}
