//---------------------------------------------------------------------
// <copyright file="NullEntityWrapper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.Objects.Internal
{
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;

    /// <summary>
    /// Defines an entity wrapper that wraps an entity with a null value.
    /// This is a singleton class for which the same instance is always returned
    /// any time a wrapper around a null entity is requested.  Objects of this
    /// type are immutable and mutable to allow this behavior to work correctly.
    /// </summary>
    internal class NullEntityWrapper : IEntityWrapper
    {
        private static IEntityWrapper s_nullWrapper = new NullEntityWrapper();

        // Private constructor prevents anyone else from creating an instance
        private NullEntityWrapper()
        {
        }

        /// <summary>
        /// The single instance of this class.
        /// </summary>
        internal static IEntityWrapper NullWrapper
        {
            get { return s_nullWrapper; }
        }

        public RelationshipManager RelationshipManager
        {
            get
            {
                Debug.Fail("Cannot access RelationshipManager from null wrapper.");
                return null;
            }
        }

        public bool OwnsRelationshipManager
        {
            get
            {
                Debug.Fail("Cannot access RelationshipManager from null wrapper.");
                return false;
            }
        }

        public object Entity
        {
            get { return null; }
        }

        public EntityEntry ObjectStateEntry
        {
            get { return null; }
            set { }
        }

        public void CollectionAdd(RelatedEnd relatedEnd, object value)
        {
            Debug.Fail("Cannot modify collection from null wrapper.");
        }

        public bool CollectionRemove(RelatedEnd relatedEnd, object value)
        {
            Debug.Fail("Cannot modify collection from null wrapper.");
            return false;
        }

        public EntityKey EntityKey
        {
            get
            {
                Debug.Fail("Cannot access EntityKey from null wrapper.");
                return null;
            }
            set
            {
                Debug.Fail("Cannot access EntityKey from null wrapper.");
            }
        }

        public EntityKey GetEntityKeyFromEntity()
        {
            Debug.Assert(false, "Method on NullEntityWrapper should not be called");
            return null;
        }

        public ObjectContext Context
        {
            get
            {
                Debug.Fail("Cannot access Context from null wrapper.");
                return null;
            }
            set
            {
                Debug.Fail("Cannot access Context from null wrapper.");
            }
        }

        public MergeOption MergeOption
        {
            get
            {
                Debug.Fail("Cannot access MergeOption from null wrapper.");
                return MergeOption.NoTracking;
            }
        }

        public void AttachContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
        {
            Debug.Fail("Cannot access Context from null wrapper.");
        }

        public void ResetContext(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
        {
            Debug.Fail("Cannot access Context from null wrapper.");
        }

        public void DetachContext()
        {
            Debug.Fail("Cannot access Context from null wrapper.");
        }

        public void SetChangeTracker(IEntityChangeTracker changeTracker)
        {
            Debug.Fail("Cannot access ChangeTracker from null wrapper.");
        }

        public void TakeSnapshot(EntityEntry entry)
        {
            Debug.Fail("Cannot take snapshot of using null wrapper.");
        }

        public void TakeSnapshotOfRelationships(EntityEntry entry)
        {
            Debug.Fail("Cannot take snapshot using null wrapper.");
        }

        public Type IdentityType
        {
            get
            {
                Debug.Fail("Cannot access IdentityType from null wrapper.");
                return null;
            }
        }

        public void EnsureCollectionNotNull(RelatedEnd relatedEnd)
        {
            Debug.Fail("Cannot modify collection from null wrapper.");
        }

        public object GetNavigationPropertyValue(RelatedEnd relatedEnd)
        {
            Debug.Fail("Cannot access property using null wrapper.");
            return null;
        }

        public void SetNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            Debug.Fail("Cannot access property using null wrapper.");
        }

        public void RemoveNavigationPropertyValue(RelatedEnd relatedEnd, object value)
        {
            Debug.Fail("Cannot access property using null wrapper.");
        }

        public void SetCurrentValue(EntityEntry entry, StateManagerMemberMetadata member, int ordinal, object target, object value)
        {
            Debug.Fail("Cannot set a value onto a null entity.");
        }

        public bool InitializingProxyRelatedEnds
        {
            get
            { 
                Debug.Fail("Cannot access flag on null wrapper.");
                return false;
            }
            set
            {
                Debug.Fail("Cannot access flag on null wrapper.");
            }
        }

        public void UpdateCurrentValueRecord(object value, EntityEntry entry)
        {
            Debug.Fail("Cannot UpdateCurrentValueRecord on a null entity."); 
        }

        public bool RequiresRelationshipChangeTracking
        {
            get { return false; }
        }
    }
}
