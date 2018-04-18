//---------------------------------------------------------------------
// <copyright file="EntityObject.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// This is the class is the basis for all perscribed EntityObject classes.
    /// </summary>
    [DataContract(IsReference=true)]
    [Serializable]
    public abstract class EntityObject : StructuralObject, IEntityWithKey, IEntityWithChangeTracker, IEntityWithRelationships
    {
        #region Privates

        // The following 2 fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private RelationshipManager _relationships;
        private EntityKey _entityKey;

        [NonSerialized]
        private IEntityChangeTracker _entityChangeTracker = s_detachedEntityChangeTracker;
        [NonSerialized]
        private static readonly DetachedEntityChangeTracker s_detachedEntityChangeTracker = new DetachedEntityChangeTracker();

        /// <summary>
        /// Helper class used when we are not currently attached to a change tracker.
        /// Simplifies the code so we don't always have to check for null before using the change tracker
        /// </summary>
        private class DetachedEntityChangeTracker : IEntityChangeTracker
        {
            void IEntityChangeTracker.EntityMemberChanging(string entityMemberName) { }
            void IEntityChangeTracker.EntityMemberChanged(string entityMemberName) { }
            void IEntityChangeTracker.EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexMemberName) { }
            void IEntityChangeTracker.EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexMemberName) { }
            EntityState IEntityChangeTracker.EntityState
            {
                get
                {
                    return EntityState.Detached;
                }
            }
        }

        private IEntityChangeTracker EntityChangeTracker
        {
            get
            {
                if (_entityChangeTracker == null)
                {
                    _entityChangeTracker = s_detachedEntityChangeTracker;
                }
                return _entityChangeTracker;
            }
            set
            {
                _entityChangeTracker = value;
            }
        }

        #endregion
        #region Publics
        /// <summary>
        /// The storage state of this EntityObject 
        /// </summary>
        /// <value>
        /// This property returns a value from the EntityState enum.
        /// </value>
        [System.ComponentModel.Browsable(false)]
        [System.Xml.Serialization.XmlIgnore]
        public EntityState EntityState
        {
            get
            {
                Debug.Assert(EntityChangeTracker != null,
                    "EntityChangeTracker should never return null -- if detached should be set to s_detachedEntityChangeTracker");
                Debug.Assert(EntityChangeTracker != s_detachedEntityChangeTracker ? EntityChangeTracker.EntityState != EntityState.Detached : true,
                    "Should never get a detached state from an attached change tracker.");

                return EntityChangeTracker.EntityState;
            }
        }

        #region IEntityWithKey

        /// <summary>
        /// Returns the EntityKey for this EntityObject.
        /// </summary>
        [Browsable(false)]
        [DataMember]
        public EntityKey EntityKey
        {
            get
            {
                return _entityKey;
            }
            set
            {
                // Report the change to the change tracker
                // If we are not attached to a change tracker, we can do anything we want to the key
                // If we are attached, the change tracker should make sure the new value is valid for the current state
                Debug.Assert(EntityChangeTracker != null, "_entityChangeTracker should never be null -- if detached it should return s_detachedEntityChangeTracker");
                EntityChangeTracker.EntityMemberChanging(StructuralObject.EntityKeyPropertyName);
                _entityKey = value;
                EntityChangeTracker.EntityMemberChanged(StructuralObject.EntityKeyPropertyName);                
            }
        } 

        #endregion       
        #region IEntityWithChangeTracker


        /// <summary>
        /// Used by the ObjectStateManager to attach or detach this EntityObject to the cache.
        /// </summary>
        /// <param name="changeTracker">
        /// Reference to the ObjectStateEntry that contains this entity
        /// </param>
        void IEntityWithChangeTracker.SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            // Fail if the change tracker is already set for this EntityObject and it's being set to something different
            // If the original change tracker is associated with a disposed ObjectStateManager, then allow
            // the entity to be attached
            if (changeTracker != null && EntityChangeTracker != s_detachedEntityChangeTracker && !Object.ReferenceEquals(changeTracker, EntityChangeTracker))
            {
                EntityEntry entry = EntityChangeTracker as EntityEntry;
                if (entry == null || !entry.ObjectStateManager.IsDisposed)
                {
                    throw EntityUtil.EntityCantHaveMultipleChangeTrackers();
                }
            }
            
            EntityChangeTracker = changeTracker;
        }

        #endregion IEntityWithChangeTracker
        #region IEntityWithRelationships

        /// <summary>
        /// Returns the container for the lazily created relationship 
        /// navigation property objects, collections and refs.
        /// </summary>
        RelationshipManager IEntityWithRelationships.RelationshipManager
        {
            get
            {
                if (_relationships == null)
                {
                    _relationships = RelationshipManager.Create(this);
                }

                return _relationships;
            }
        }

        #endregion
        #endregion        
        #region Protected Change Tracking Methods

        /// <summary>
        /// This method is called whenever a change is going to be made to an EntityObject 
        /// property.
        /// </summary>
        /// <param name="property">
        /// The name of the changing property.
        /// </param>        
        /// <exception cref="System.ArgumentNullException">
        /// When parameter member is null (Nothing in Visual Basic).
        /// </exception>
        protected sealed override void ReportPropertyChanging(
            string property)
        {
            EntityUtil.CheckStringArgument(property, "property");

            Debug.Assert(EntityChangeTracker != null, "_entityChangeTracker should never be null -- if detached it should return s_detachedEntityChangeTracker");

            base.ReportPropertyChanging(property);

            EntityChangeTracker.EntityMemberChanging(property);
        }

        /// <summary>
        /// This method is called whenever a change is made to an EntityObject 
        /// property.
        /// </summary>
        /// <param name="property">
        /// The name of the changed property.
        /// </param>        
        /// <exception cref="System.ArgumentNullException">
        /// When parameter member is null (Nothing in Visual Basic).
        /// </exception>
        protected sealed override void ReportPropertyChanged(
            string property)
        {
            EntityUtil.CheckStringArgument(property, "property");

            Debug.Assert(EntityChangeTracker != null, "EntityChangeTracker should never return null -- if detached it should be return s_detachedEntityChangeTracker");
            EntityChangeTracker.EntityMemberChanged(property);           
            
            base.ReportPropertyChanged(property);
        }

        #endregion
        #region Internal ComplexObject Change Tracking Methods and Properties

        internal sealed override bool IsChangeTracked
        {
            get
            {
                return EntityState != EntityState.Detached;
            }
        }

        /// <summary>
        /// This method is called by a ComplexObject contained in this Entity 
        /// whenever a change is about to be made to a property of the  
        /// ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName">
        /// The name of the top-level entity property that contains the ComplexObject that is calling this method.
        /// </param>
        /// <param name="complexObject">
        /// The instance of the ComplexObject on which the property is changing.
        /// </param>
        /// <param name="complexMemberName">
        /// The name of the changing property on complexObject.
        /// </param>        
        internal sealed override void ReportComplexPropertyChanging(
            string entityMemberName, ComplexObject complexObject, string complexMemberName)
        {
            Debug.Assert(complexObject != null, "invalid complexObject");
            Debug.Assert(!String.IsNullOrEmpty(complexMemberName), "invalid complexMemberName");

            EntityChangeTracker.EntityComplexMemberChanging(entityMemberName, complexObject, complexMemberName);            
        }

        /// <summary>
        /// This method is called by a ComplexObject contained in this Entity 
        /// whenever a change has been made to a property of the  
        /// ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName">
        /// The name of the top-level entity property that contains the ComplexObject that is calling this method.
        /// </param>
        /// <param name="complexObject">
        /// The instance of the ComplexObject on which the property is changing.
        /// </param>
        /// <param name="complexMemberName">
        /// The name of the changing property on complexObject.
        /// </param>        
        internal sealed override void ReportComplexPropertyChanged(
            string entityMemberName, ComplexObject complexObject, string complexMemberName)
        {
            Debug.Assert(complexObject != null, "invalid complexObject");
            Debug.Assert(!String.IsNullOrEmpty(complexMemberName), "invalid complexMemberName");

            EntityChangeTracker.EntityComplexMemberChanged(entityMemberName, complexObject, complexMemberName);
        }

        #endregion      
    }
}

