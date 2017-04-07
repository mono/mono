//---------------------------------------------------------------------
// <copyright file="RelationshipManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.Objects.DataClasses
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Common.Utils;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.Internal;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Container for the lazily created relationship navigation
    /// property objects (collections and refs).
    /// </summary>
    [Serializable]
    public class RelationshipManager
    {
        // ------------
        // Constructors
        // ------------

        // This method is private in order to force all creation of this
        // object to occur through the public static Create method.
        // See comments on that method for more details.
        private RelationshipManager()
        {
        }

        // ------
        // Fields
        // ------

        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.

        // Note that this field should no longer be used directly.  Instead, use the _wrappedOwner
        // field.  This field is retained only for compatibility with the serialization format introduced in v1.
        private IEntityWithRelationships _owner;

        private List<RelatedEnd> _relationships;

        [NonSerialized]
        private bool _nodeVisited;

        [NonSerialized]
        private IEntityWrapper _wrappedOwner;

        // ----------
        // Properties
        // ----------

        /// <summary>
        /// Returns a defensive copy of all the known relationships.  The copy is defensive because
        /// new items may get added to the collection while the caller is iterating over it.  Without
        /// the copy this would cause an exception for concurrently modifying the collection.
        /// </summary>
        internal IEnumerable<RelatedEnd> Relationships
        {
            get
            {
                EnsureRelationshipsInitialized();
                return _relationships.ToArray();
            }
        }

        /// <summary>
        /// Lazy initialization of the _relationships collection.
        /// </summary>
        private void EnsureRelationshipsInitialized()
        {
            if (null == _relationships)
            {
                _relationships = new List<RelatedEnd>();
            }
        }

        /// <summary>
        /// this flag is used to keep track of nodes which have 
        /// been visited. Currently used for Exclude operation.
        /// </summary>
        internal bool NodeVisited
        {
            get
            {
                return _nodeVisited;
            }
            set
            {
                _nodeVisited = value;
            }
        }

        /// <summary>
        /// Provides access to the entity that owns this manager in its wrapped form.
        /// </summary>
        internal IEntityWrapper WrappedOwner
        {
            get
            {
                if (_wrappedOwner == null)
                {
                    _wrappedOwner = EntityWrapperFactory.CreateNewWrapper(_owner, null);
                }
                return _wrappedOwner;
            }
        }

        // -------
        // Methods
        // -------

        /// <summary>
        /// Factory method to create a new RelationshipManager object.
        /// 
        /// Used by data classes that support relationships. If the change tracker
        /// requests the RelationshipManager property and the data class does not
        /// already have a reference to one of these objects, it calls this method
        /// to create one, then saves a reference to that object. On subsequent accesses
        /// to that property, the data class should return the saved reference.
        /// 
        /// The reason for using a factory method instead of a public constructor is to 
        /// emphasize that this is not something you would normally call outside of a data class.
        /// By requiring that these objects are created via this method, developers should
        /// give more thought to the operation, and will generally only use it when
        /// they explicitly need to get an object of this type. It helps define the intended usage.  
        /// </summary>
        /// <param name="owner">Reference to the entity that is calling this method</param>
        /// <exception cref="ArgumentNullException"><paramref name="owner"/> is null</exception>
        /// <returns>A new or existing RelationshipManager for the given entity</returns>
        public static RelationshipManager Create(IEntityWithRelationships owner)
        {
            EntityUtil.CheckArgumentNull(owner, "owner");
            RelationshipManager rm = new RelationshipManager();
            rm._owner = owner;
            return rm;
        }

        /// <summary>
        /// Factory method that creates a new, uninitialized RelationshipManager.  This should only be
        /// used to create a RelationshipManager for an IEntityWrapper for an entity that does not
        /// implement IEntityWithRelationships.  For entities that implement IEntityWithRelationships,
        /// the Create(IEntityWithRelationships) method should be used instead.
        /// </summary>
        /// <returns>The new RelationshipManager</returns>
        internal static RelationshipManager Create()
        {
            return new RelationshipManager();
        }

        /// <summary>
        /// Replaces the existing wrapped owner with one that potentially contains more information,
        /// such as an entity key.  Both must wrap the same entity.
        /// </summary>
        internal void SetWrappedOwner(IEntityWrapper wrappedOwner, object expectedOwner)
        {
            _wrappedOwner = wrappedOwner;
            Debug.Assert(_owner != null || !(wrappedOwner.Entity is IEntityWithRelationships), "_owner should only be null if entity is not IEntityWithRelationships");
            // We need to check that the RelationshipManager created by the entity has the correct owner set,
            // since the entity can pass any value into RelationshipManager.Create().
            if (_owner != null && !Object.ReferenceEquals(expectedOwner, _owner))
            {
                throw EntityUtil.InvalidRelationshipManagerOwner();
            }

            if (null != _relationships)
            {
                // Not using defensive copy here since SetWrappedOwner should not cause change in underlying
                // _relationships collection.
                foreach (RelatedEnd relatedEnd in _relationships)
                {
                    relatedEnd.SetWrappedOwner(wrappedOwner);
                }
            }
        }

        /// <summary>
        /// Get the collection of entities related to the current entity using the specified
        /// combination of relationship name, source role name, and target role name
        /// </summary>
        /// <typeparam name="TSourceEntity">Type of the entity in the source role (same as the type of this)</typeparam>
        /// <typeparam name="TTargetEntity">Type of the entity in the target role</typeparam>
        /// <param name="relationshipName">CSpace-qualified name of the relationship to navigate</param>
        /// <param name="sourceRoleName">Name of the source role for the navigation. Indicates the direction of navigation across the relationship.</param>
        /// <param name="targetRoleName">Name of the target role for the navigation. Indicates the direction of navigation across the relationship.</param>
        /// <param name="sourcePropertyName">Name of the property on the source of the navigation.</param>
        /// <param name="targetPropertyName">Name of the property on the target of the navigation.</param>
        /// <param name="sourceRoleMultiplicity">Multiplicity of the source role. RelationshipMultiplicity.OneToOne and RelationshipMultiplicity.Zero are both
        /// accepted for a reference end, and RelationshipMultiplicity.Many is accepted for a collection</param>
        /// <returns>Collection of related entities of type TTargetEntity</returns>
        internal EntityCollection<TTargetEntity> GetRelatedCollection<TSourceEntity, TTargetEntity>(string relationshipName,
            string sourceRoleName, string targetRoleName, NavigationPropertyAccessor sourceAccessor, NavigationPropertyAccessor targetAccessor,
            RelationshipMultiplicity sourceRoleMultiplicity, RelatedEnd existingRelatedEnd)
            where TSourceEntity : class
            where TTargetEntity : class
        {
            EntityCollection<TTargetEntity> collection;
            RelatedEnd relatedEnd;
            TryGetCachedRelatedEnd(relationshipName, targetRoleName, out relatedEnd);

            if (existingRelatedEnd == null)
            {
                if (relatedEnd != null)
                {
                    collection = relatedEnd as EntityCollection<TTargetEntity>;
                    // Because this is a private method that will only be called for target roles that actually have a
                    // multiplicity that works with EntityReference, this should never be null. If the user requests
                    // a collection or reference and it doesn't match the target role multiplicity, it will be detected
                    // in the public GetRelatedCollection<T> or GetRelatedReference<T>
                    Debug.Assert(collection != null, "should never receive anything but an EntityCollection here");
                    return collection;
                }
                else
                {
                    RelationshipNavigation navigation = new RelationshipNavigation(relationshipName, sourceRoleName, targetRoleName, sourceAccessor, targetAccessor);
                    return CreateRelatedEnd<TSourceEntity, TTargetEntity>(navigation, sourceRoleMultiplicity, RelationshipMultiplicity.Many, existingRelatedEnd) as EntityCollection<TTargetEntity>;
                }
            }
            else
            {
                // There is no need to supress events on the existingRelatedEnd because setting events on a disconnected
                // EntityCollection is an InvalidOperation
                Debug.Assert(existingRelatedEnd._onAssociationChanged == null, "Disconnected RelatedEnd had events");

                if (relatedEnd != null)
                {
                    Debug.Assert(_relationships != null, "Expected _relationships to be non-null.");
                    _relationships.Remove(relatedEnd);
                }

                RelationshipNavigation navigation = new RelationshipNavigation(relationshipName, sourceRoleName, targetRoleName, sourceAccessor, targetAccessor);
                collection = CreateRelatedEnd<TSourceEntity, TTargetEntity>(navigation, sourceRoleMultiplicity, RelationshipMultiplicity.Many, existingRelatedEnd) as EntityCollection<TTargetEntity>;

                if (collection != null)
                {
                    bool doCleanup = true;
                    try
                    {
                        RemergeCollections(relatedEnd as EntityCollection<TTargetEntity>, collection);
                        doCleanup = false;
                    }
                    finally
                    {
                        // An error occured so we need to put the previous relatedEnd back into the RelationshipManager
                        if (doCleanup && relatedEnd != null)
                        {
                            Debug.Assert(_relationships != null, "Expected _relationships to be non-null.");
                            _relationships.Remove(collection);
                            _relationships.Add(relatedEnd);
                        }
                    }
                }
                return collection;
            }
        }

        /// <summary>
        /// Re-merge items from collection so that relationship fixup is performed.
        /// Ensure that any items in previous collection are excluded from the re-merge
        /// </summary>
        /// <typeparam name="TTargetEntity"></typeparam>
        /// <param name="previousCollection">The previous EntityCollection containing items that have already had fixup performed</param>
        /// <param name="collection">The new EntityCollection</param>
        private void RemergeCollections<TTargetEntity>(EntityCollection<TTargetEntity> previousCollection,
            EntityCollection<TTargetEntity> collection)
                where TTargetEntity : class
        {
            Debug.Assert(collection != null, "collection is null");
            // If there is a previousCollection, we only need to merge the items that are 
            // in the collection but not in the previousCollection
            // Ensure that all of the items in the previousCollection are already in the new collection

            int relatedEntityCount = 0;

            // We will be modifing the collection's enumerator, so we need to make a copy of it
            List<IEntityWrapper> tempEntities = new List<IEntityWrapper>(collection.CountInternal);
            foreach (IEntityWrapper wrappedEntity in collection.GetWrappedEntities())
            {
                tempEntities.Add(wrappedEntity);
            }

            // Iterate through the entities that require merging
            // If the previousCollection already contained the entity, no additional work is needed
            // If the previousCollection did not contain the entity,
            //   then remove it from the collection and re-add it to force relationship fixup
            foreach (IEntityWrapper wrappedEntity in tempEntities)
            {
                bool requiresMerge = true;
                if (previousCollection != null)
                {
                    // There is no need to merge and do fixup if the entity was already in the previousCollection because
                    // fixup would have already taken place when it was added to the previousCollection
                    if (previousCollection.ContainsEntity(wrappedEntity))
                    {
                        relatedEntityCount++;
                        requiresMerge = false;
                    }
                }

                if (requiresMerge)
                {
                    // Remove and re-add the item to the collections to force fixup
                    collection.Remove(wrappedEntity, false);
                    collection.Add(wrappedEntity);
                }
            }

            // Ensure that all of the items in the previousCollection are already in the new collection
            if (previousCollection != null && relatedEntityCount != previousCollection.CountInternal)
            {
                throw EntityUtil.CannotRemergeCollections();
            }
        }

        /// <summary>
        /// Get the entity reference of a related entity using the specified
        /// combination of relationship name, source role name, and target role name
        /// </summary>
        /// <param name="relationshipName">CSpace-qualified name of the relationship to navigate</param>
        /// <param name="sourceRoleName">Name of the source role for the navigation. Indicates the direction of navigation across the relationship.</param>
        /// <param name="targetRoleName">Name of the target role for the navigation. Indicates the direction of navigation across the relationship.</param>
        /// <param name="sourcePropertyName">Name of the property on the source of the navigation.</param>
        /// <param name="targetPropertyName">Name of the property on the target of the navigation.</param>
        /// <param name="sourceRoleMultiplicity">Multiplicity of the source role. RelationshipMultiplicity.OneToOne and RelationshipMultiplicity.Zero are both
        /// accepted for a reference end, and RelationshipMultiplicity.Many is accepted for a collection</param>
        /// <returns>Reference for related entity of type TTargetEntity</returns>
        internal EntityReference<TTargetEntity> GetRelatedReference<TSourceEntity, TTargetEntity>(string relationshipName,
            string sourceRoleName, string targetRoleName, NavigationPropertyAccessor sourceAccessor, NavigationPropertyAccessor targetAccessor,
            RelationshipMultiplicity sourceRoleMultiplicity, RelatedEnd existingRelatedEnd)
            where TSourceEntity : class
            where TTargetEntity : class
        {
            EntityReference<TTargetEntity> entityRef;
            RelatedEnd relatedEnd;

            if (TryGetCachedRelatedEnd(relationshipName, targetRoleName, out relatedEnd))
            {
                entityRef = relatedEnd as EntityReference<TTargetEntity>;
                // Because this is a private method that will only be called for target roles that actually have a
                // multiplicity that works with EntityReference, this should never be null. If the user requests
                // a collection or reference and it doesn't match the target role multiplicity, it will be detected
                // in the public GetRelatedCollection<T> or GetRelatedReference<T>
                Debug.Assert(entityRef != null, "should never receive anything but an EntityReference here");
                return entityRef;
            }
            else
            {
                RelationshipNavigation navigation = new RelationshipNavigation(relationshipName, sourceRoleName, targetRoleName, sourceAccessor, targetAccessor);
                return CreateRelatedEnd<TSourceEntity, TTargetEntity>(navigation, sourceRoleMultiplicity, RelationshipMultiplicity.One, existingRelatedEnd) as EntityReference<TTargetEntity>;
            }
        }

        /// <summary>
        /// Internal version of GetRelatedEnd that works with the o-space navigation property
        /// name rather than the c-space relationship name and end name.
        /// </summary>
        /// <param name="navigationProperty">the name of the property to lookup</param>
        /// <returns>the related end for the given property</returns>
        internal RelatedEnd GetRelatedEnd(string navigationProperty, bool throwArgumentException = false)
        {
            IEntityWrapper wrappedOwner = WrappedOwner;
            Debug.Assert(wrappedOwner.Entity != null, "Entity is null");
            Debug.Assert(wrappedOwner.Context != null, "Context is null");
            Debug.Assert(wrappedOwner.Context.MetadataWorkspace != null, "MetadataWorkspace is null");
            Debug.Assert(wrappedOwner.Context.Perspective != null, "Perspective is null");

            EntityType entityType = wrappedOwner.Context.MetadataWorkspace.GetItem<EntityType>(wrappedOwner.IdentityType.FullName, DataSpace.OSpace);
            EdmMember member;
            if (!wrappedOwner.Context.Perspective.TryGetMember(entityType, navigationProperty, false, out member) ||
                !(member is NavigationProperty))
            {
                var message = System.Data.Entity.Strings.RelationshipManager_NavigationPropertyNotFound(navigationProperty);
                throw throwArgumentException ? (Exception)new ArgumentException(message) : (Exception)new InvalidOperationException(message);
            }
            NavigationProperty navProp = (NavigationProperty)member;
            return GetRelatedEndInternal(navProp.RelationshipType.FullName, navProp.ToEndMember.Name);
        }

        /// <summary>
        /// Returns either an EntityCollection or EntityReference of the correct type for the specified target role in a relationship
        /// This is intended to be used in scenarios where the user doesn't have full metadata, including the static type
        /// information for both ends of the relationship. This metadata is specified in the EdmRelationshipRoleAttribute
        /// on each entity type in the relationship, so the metadata system can retrieve it based on the supplied relationship
        /// name and target role name.
        /// </summary>
        /// <param name="relationshipName">Name of the relationship in which targetRoleName is defined. Can be CSpace-qualified or not.</param>
        /// <param name="targetRoleName">Target role to use to retrieve the other end of relationshipName</param>
        /// <returns>IRelatedEnd representing the EntityCollection or EntityReference that was retrieved</returns>
        public IRelatedEnd GetRelatedEnd(string relationshipName, string targetRoleName)
        {
            return GetRelatedEndInternal(PrependNamespaceToRelationshipName(relationshipName), targetRoleName);
        }

        // Internal version of GetRelatedEnd which returns the RelatedEnd as a RelatedEnd rather than an IRelatedEnd
        internal RelatedEnd GetRelatedEndInternal(string relationshipName, string targetRoleName)
        {
            EntityUtil.CheckArgumentNull(relationshipName, "relationshipName");
            EntityUtil.CheckArgumentNull(targetRoleName, "targetRoleName");

            IEntityWrapper wrappedOwner = WrappedOwner;
            if (wrappedOwner.Context == null && wrappedOwner.RequiresRelationshipChangeTracking)
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.RelationshipManager_CannotGetRelatEndForDetachedPocoEntity);
            }

            RelatedEnd relatedEnd = null;

            // Try to get the AssociationType from metadata. This will contain all of the ospace metadata for this relationship            
            AssociationType associationType = null;
            if (!TryGetRelationshipType(wrappedOwner, wrappedOwner.IdentityType, relationshipName, out associationType))
            {
                if (_relationships != null)
                {
                    // Look for the RelatedEnd in the list that has already been retrieved
                    relatedEnd = (from RelatedEnd end in _relationships
                                  where end.RelationshipName == relationshipName &&
                                        end.TargetRoleName == targetRoleName
                                  select end).FirstOrDefault();
                }

                if (relatedEnd == null && !EntityProxyFactory.TryGetAssociationTypeFromProxyInfo(wrappedOwner, relationshipName, targetRoleName, out associationType))
                {
                    // If the end still cannot be found, throw an exception
                    throw UnableToGetMetadata(WrappedOwner, relationshipName);
                }
            }

            if (relatedEnd == null)
            {
                Debug.Assert(associationType != null, "associationType is null");
                relatedEnd = GetRelatedEndInternal(relationshipName, targetRoleName, /*existingRelatedEnd*/ null, associationType);
            }

            return relatedEnd;
        }

        private RelatedEnd GetRelatedEndInternal(string relationshipName, string targetRoleName, RelatedEnd existingRelatedEnd, AssociationType relationship)
        {
            return GetRelatedEndInternal(relationshipName, targetRoleName, existingRelatedEnd, relationship, true);
        }

        private RelatedEnd GetRelatedEndInternal(string relationshipName, string targetRoleName, RelatedEnd existingRelatedEnd, AssociationType relationship, bool throwOnError)
        {
            Debug.Assert(relationshipName != null, "null relationshipNameFromUser");
            Debug.Assert(targetRoleName != null, "null targetRoleName");
            // existingRelatedEnd can be null if we are not trying to initialize an existing end
            Debug.Assert(relationship != null, "null relationshipType");

            AssociationEndMember sourceEnd;
            AssociationEndMember targetEnd;
            Debug.Assert(relationship.AssociationEndMembers.Count == 2, "Only 2-way relationships are currently supported");

            RelatedEnd result = null;

            // There can only be two ends because we don't support n-way relationships -- figure out which end is the target and which is the source
            // If we want to support n-way relationships in the future, we will need a different overload of GetRelatedEnd that takes the source role name as well
            targetEnd = relationship.AssociationEndMembers[1];
            if (targetEnd.Identity != targetRoleName)
            {
                sourceEnd = targetEnd;
                targetEnd = relationship.AssociationEndMembers[0];
                if (targetEnd.Identity != targetRoleName)
                {
                    if (throwOnError)
                    {
                        throw EntityUtil.InvalidTargetRole(relationshipName, targetRoleName, "targetRoleName");
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            else
            {
                sourceEnd = relationship.AssociationEndMembers[0];
            }

            // Validate that the source type matches the type of the owner
            EntityType sourceEntityType = MetadataHelper.GetEntityTypeForEnd(sourceEnd);
            Debug.Assert(sourceEntityType.DataSpace == DataSpace.OSpace && sourceEntityType.ClrType != null, "sourceEntityType must contain an ospace type");
            Type sourceType = sourceEntityType.ClrType;
            IEntityWrapper wrappedOwner = WrappedOwner;
            if (!(sourceType.IsAssignableFrom(wrappedOwner.IdentityType)))
            {
                if (throwOnError)
                {
                    throw EntityUtil.OwnerIsNotSourceType(wrappedOwner.IdentityType.FullName, sourceType.FullName, sourceEnd.Name, relationshipName);
                }
            }
            else if (VerifyRelationship(relationship, sourceEnd.Name, throwOnError))
            {
                // Call a dynamic method that will call either GetRelatedCollection<T, T> or GetRelatedReference<T, T> for this relationship
                result = LightweightCodeGenerator.GetRelatedEnd(this, sourceEnd, targetEnd, existingRelatedEnd);
            }
            return result;
        }

        /// <summary>
        /// Takes an existing EntityReference that was created with the default constructor and initializes it using the provided relationship and target role names.
        /// This method is designed to be used during deserialization only, and will throw an exception if the provided EntityReference has already been initialized, 
        /// if the relationship manager already contains a relationship with this name and target role, or if the relationship manager is already attached to a ObjectContext.
        /// </summary>
        /// <typeparam name="TTargetEntity">Type of the entity represented by targetRoleName</typeparam>
        /// <param name="relationshipName"></param>
        /// <param name="targetRoleName"></param>
        /// <param name="entityReference"></param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InitializeRelatedReference<TTargetEntity>(string relationshipName, string targetRoleName, EntityReference<TTargetEntity> entityReference)
            where TTargetEntity : class
        {
            EntityUtil.CheckArgumentNull(relationshipName, "relationshipName");
            EntityUtil.CheckArgumentNull(targetRoleName, "targetRoleName");
            EntityUtil.CheckArgumentNull(entityReference, "entityReference");

            if (entityReference.WrappedOwner.Entity != null)
            {
                throw EntityUtil.ReferenceAlreadyInitialized();
            }

            IEntityWrapper wrappedOwner = WrappedOwner;
            if (wrappedOwner.Context != null && wrappedOwner.MergeOption != MergeOption.NoTracking)
            {
                throw EntityUtil.RelationshipManagerAttached();
            }

            // We need the CSpace-qualified name in order to determine if this relationship already exists, so look it up.
            // If the relationship doesn't exist, we will use this type information to determine how to initialize the reference
            relationshipName = PrependNamespaceToRelationshipName(relationshipName);
            AssociationType relationship = GetRelationshipType(wrappedOwner.IdentityType, relationshipName);

            RelatedEnd relatedEnd;
            if (TryGetCachedRelatedEnd(relationshipName, targetRoleName, out relatedEnd))
            {
                // For some serialization scenarios, we have to allow replacing a related end that we already know about, but in those scenarios 
                // the end is always empty, so we can further restrict the user calling method method directly by doing this extra validation
                if (!relatedEnd.IsEmpty())
                {
                    entityReference.InitializeWithValue(relatedEnd);
                }
                Debug.Assert(_relationships != null, "Expected _relationships to be non-null.");
                _relationships.Remove(relatedEnd);
            }

            EntityReference<TTargetEntity> reference = GetRelatedEndInternal(relationshipName, targetRoleName, entityReference, relationship) as EntityReference<TTargetEntity>;
            if (reference == null)
            {
                throw EntityUtil.ExpectedReferenceGotCollection(typeof(TTargetEntity).Name, targetRoleName, relationshipName);
            }
        }

        /// <summary>
        /// Takes an existing EntityCollection that was created with the default constructor and initializes it using the provided relationship and target role names.
        /// This method is designed to be used during deserialization only, and will throw an exception if the provided EntityCollection has already been initialized, 
        /// or if the relationship manager is already attached to a ObjectContext.
        /// </summary>
        /// <typeparam name="TTargetEntity">Type of the entity represented by targetRoleName</typeparam>
        /// <param name="relationshipName"></param>
        /// <param name="targetRoleName"></param>
        /// <param name="entityCollection"></param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void InitializeRelatedCollection<TTargetEntity>(string relationshipName, string targetRoleName, EntityCollection<TTargetEntity> entityCollection)
            where TTargetEntity : class
        {
            EntityUtil.CheckArgumentNull(relationshipName, "relationshipName");
            EntityUtil.CheckArgumentNull(targetRoleName, "targetRoleName");
            EntityUtil.CheckArgumentNull(entityCollection, "entityCollection");

            if (entityCollection.WrappedOwner.Entity != null)
            {
                throw EntityUtil.CollectionAlreadyInitialized();
            }

            IEntityWrapper wrappedOwner = WrappedOwner;
            if (wrappedOwner.Context != null && wrappedOwner.MergeOption != MergeOption.NoTracking)
            {
                throw EntityUtil.CollectionRelationshipManagerAttached();
            }

            // We need the CSpace-qualified name in order to determine if this relationship already exists, so look it up.
            // If the relationship doesn't exist, we will use this type information to determine how to initialize the reference
            relationshipName = PrependNamespaceToRelationshipName(relationshipName);
            AssociationType relationship = GetRelationshipType(wrappedOwner.IdentityType, relationshipName);

            EntityCollection<TTargetEntity> collection = GetRelatedEndInternal(relationshipName, targetRoleName, entityCollection, relationship) as EntityCollection<TTargetEntity>;
            if (collection == null)
            {
                throw EntityUtil.ExpectedCollectionGotReference(typeof(TTargetEntity).Name, targetRoleName, relationshipName);
            }
        }

        /// <summary>
        /// Given a relationship name that may or may not be qualified with a namespace name, this method
        /// attempts to lookup a namespace using the entity type that owns this RelationshipManager as a
        /// source and adds that namespace to the front of the relationship name.  If the namespace
        /// can't be found, then the relationshipName is returned untouched and the expectation is that
        /// other validations will fail later in the code paths that use this.
        /// This method should only be used at the imediate top-level public surface since all internal
        /// calls are expected to use fully qualified names already.
        /// </summary>
        private string PrependNamespaceToRelationshipName(string relationshipName)
        {
            EntityUtil.CheckArgumentNull(relationshipName, "relationshipName");

            if (!relationshipName.Contains('.'))
            {
                string identityName = WrappedOwner.IdentityType.FullName;
                ObjectItemCollection objectItemCollection = GetObjectItemCollection(WrappedOwner);
                EdmType entityType = null;
                if (objectItemCollection != null)
                {
                    objectItemCollection.TryGetItem<EdmType>(identityName, out entityType);
                }
                else
                {
                    Dictionary<string, EdmType> types = ObjectItemCollection.LoadTypesExpensiveWay(WrappedOwner.IdentityType.Assembly);
                    if (types != null)
                    {
                        types.TryGetValue(identityName, out entityType);
                    }

                }
                ClrEntityType clrEntityType = entityType as ClrEntityType;
                if (clrEntityType != null)
                {
                    string ns = clrEntityType.CSpaceNamespaceName;
                    Debug.Assert(!string.IsNullOrEmpty(ns), "Expected non-empty namespace for type.");

                    return ns + "." + relationshipName;
                }
            }
            return relationshipName;
        }

        /// <summary>
        /// Trys to get an ObjectItemCollection and returns null if it can;t be found.
        /// </summary>
        private static ObjectItemCollection GetObjectItemCollection(IEntityWrapper wrappedOwner)
        {
            if (wrappedOwner.Context != null && wrappedOwner.Context.MetadataWorkspace != null)
            {
                return (ObjectItemCollection)wrappedOwner.Context.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);
            }
            return null;
        }

        /// <summary>
        /// Trys to get the EntityType metadata and returns false if it can't be found.
        /// </summary>
        private bool TryGetOwnerEntityType(out EntityType entityType)
        {
            DefaultObjectMappingItemCollection mappings;
            Map map;
            if (TryGetObjectMappingItemCollection(WrappedOwner, out mappings) &&
                mappings.TryGetMap(WrappedOwner.IdentityType.FullName, DataSpace.OSpace, out map))
            {
                ObjectTypeMapping objectMap = (ObjectTypeMapping)map;
                if (Helper.IsEntityType(objectMap.EdmType))
                {
                    entityType = (EntityType)objectMap.EdmType;
                    return true;
                }
            }

            entityType = null;
            return false;
        }

        /// <summary>
        /// Trys to get an DefaultObjectMappingItemCollection and returns false if it can't be found.
        /// </summary>
        private static bool TryGetObjectMappingItemCollection(IEntityWrapper wrappedOwner, out DefaultObjectMappingItemCollection collection)
        {
            if (wrappedOwner.Context != null && wrappedOwner.Context.MetadataWorkspace != null)
            {
                collection = (DefaultObjectMappingItemCollection)wrappedOwner.Context.MetadataWorkspace.GetItemCollection(DataSpace.OCSpace);
                return collection != null;
            }

            collection = null;
            return false;
        }



        internal static bool TryGetRelationshipType(IEntityWrapper wrappedOwner, Type entityClrType, string relationshipName, out AssociationType associationType)
        {
            ObjectItemCollection objectItemCollection = GetObjectItemCollection(wrappedOwner);
            if (objectItemCollection != null)
            {
                associationType = objectItemCollection.GetRelationshipType(entityClrType, relationshipName);
            }
            else
            {
                associationType = ObjectItemCollection.GetRelationshipTypeExpensiveWay(entityClrType, relationshipName);
            }

            return (associationType != null);
        }

        private AssociationType GetRelationshipType(Type entityClrType, string relationshipName)
        {
            AssociationType associationType = null;
            if (!TryGetRelationshipType(WrappedOwner, entityClrType, relationshipName, out associationType))
            {
                throw UnableToGetMetadata(WrappedOwner, relationshipName);
            }
            return associationType;
        }

        internal static Exception UnableToGetMetadata(IEntityWrapper wrappedOwner, string relationshipName)
        {
            ArgumentException argException = EntityUtil.UnableToFindRelationshipTypeInMetadata(relationshipName, "relationshipName");
            if (EntityProxyFactory.IsProxyType(wrappedOwner.Entity.GetType()))
            {
                return EntityUtil.ProxyMetadataIsUnavailable(wrappedOwner.IdentityType, argException);
            }
            else
            {
                return argException;
            }
        }

        private IEnumerable<AssociationEndMember> GetAllTargetEnds(EntityType ownerEntityType, EntitySet ownerEntitySet)
        {
            foreach (AssociationSet assocSet in MetadataHelper.GetAssociationsForEntitySet(ownerEntitySet))
            {
                EntityType end2EntityType = ((AssociationType)assocSet.ElementType).AssociationEndMembers[1].GetEntityType();
                if (end2EntityType.IsAssignableFrom(ownerEntityType))
                {
                    yield return ((AssociationType)assocSet.ElementType).AssociationEndMembers[0];
                }
                // not "else" because of associations between the same entity sets
                EntityType end1EntityType = ((AssociationType)assocSet.ElementType).AssociationEndMembers[0].GetEntityType();
                if (end1EntityType.IsAssignableFrom(ownerEntityType))
                {
                    yield return ((AssociationType)assocSet.ElementType).AssociationEndMembers[1];
                }
            }
        }

        /// <summary>
        /// Retrieves the AssociationEndMembers that corespond to the target end of a relationship
        /// given a specific CLR type that exists on the source end of a relationship
        /// Note: this method can be very expensive if this RelationshipManager is not attached to an 
        /// ObjectContext because no OSpace Metadata is available
        /// </summary>
        /// <param name="entityClrType">A CLR type that is on the source role of the relationship</param>
        /// <returns>The OSpace EntityType that represents this CLR type</returns>
        private IEnumerable<AssociationEndMember> GetAllTargetEnds(Type entityClrType)
        {
            ObjectItemCollection objectItemCollection = GetObjectItemCollection(WrappedOwner);

            IEnumerable<AssociationType> associations = null;
            if (objectItemCollection != null)
            {
                // Metadata is available
                associations = objectItemCollection.GetItems<AssociationType>();
            }
            else
            {
                // No metadata is available, attempt to load the metadata on the fly to retrieve the AssociationTypes
                associations = ObjectItemCollection.GetAllRelationshipTypesExpensiveWay(entityClrType.Assembly);
            }

            foreach (AssociationType association in associations)
            {
                // Check both ends for the presence of the source CLR type
                RefType referenceType = association.AssociationEndMembers[0].TypeUsage.EdmType as RefType;
                if (referenceType != null && referenceType.ElementType.ClrType.IsAssignableFrom(entityClrType))
                {
                    // Return the target end
                    yield return association.AssociationEndMembers[1];
                }

                referenceType = association.AssociationEndMembers[1].TypeUsage.EdmType as RefType;
                if (referenceType != null && referenceType.ElementType.ClrType.IsAssignableFrom(entityClrType))
                {
                    // Return the target end
                    yield return association.AssociationEndMembers[0];
                }
            }
            yield break;
        }


        private bool VerifyRelationship(AssociationType relationship, string sourceEndName, bool throwOnError)
        {
            IEntityWrapper wrappedOwner = WrappedOwner;
            if (wrappedOwner.Context == null)
            {
                return true;// if not added to cache, can not decide- for now
            }

            EntityKey ownerKey = null;
            ownerKey = wrappedOwner.EntityKey;

            if (null == (object)ownerKey)
            {
                return true; // if not added to cache, can not decide- for now
            }

            TypeUsage associationTypeUsage;
            AssociationSet association = null;
            bool isVerified = true;

            // First, get the CSpace association type from the relationship name, since the helper method looks up
            // association set in the CSpace, since there is no Entity Container in the OSpace
            if (wrappedOwner.Context.Perspective.TryGetTypeByName(relationship.FullName, false/*ignoreCase*/, out associationTypeUsage))
            {
                //Get the entity container first
                EntityContainer entityContainer = wrappedOwner.Context.MetadataWorkspace.GetEntityContainer(
                    ownerKey.EntityContainerName, DataSpace.CSpace);
                EntitySet entitySet;

                // Get the association set from the entity container, given the association type it refers to, and the entity set
                // name that the source end refers to
                association = MetadataHelper.GetAssociationsForEntitySetAndAssociationType(entityContainer, ownerKey.EntitySetName,
                    (AssociationType)associationTypeUsage.EdmType, sourceEndName, out entitySet);

                if (association == null)
                {
                    if (throwOnError)
                    {
                        throw EntityUtil.NoRelationshipSetMatched(relationship.FullName);
                    }
                    else
                    {
                        isVerified = false;
                    }
                }
                else
                {
                    Debug.Assert(association.AssociationSetEnds[sourceEndName].EntitySet == entitySet, "AssociationSetEnd does have the matching EntitySet");
                }
            }
            return isVerified;
        }

        /// <summary>
        /// Get the collection of a related entity using the specified
        /// combination of relationship name, and target role name.
        /// Only supports 2-way relationships.
        /// </summary>
        /// <param name="relationshipName">Name of the relationship in which targetRoleName is defined. Can be CSpace-qualified or not.</param>        
        /// <param name="targetRoleName">Name of the target role for the navigation. Indicates the direction of navigation across the relationship.</param>
        /// <returns>Collection of entities of type TTargetEntity</returns>
        public EntityCollection<TTargetEntity> GetRelatedCollection<TTargetEntity>(string relationshipName, string targetRoleName)
            where TTargetEntity : class
        {
            EntityCollection<TTargetEntity> collection = GetRelatedEndInternal(PrependNamespaceToRelationshipName(relationshipName), targetRoleName) as EntityCollection<TTargetEntity>;
            if (collection == null)
            {
                throw EntityUtil.ExpectedCollectionGotReference(typeof(TTargetEntity).Name, targetRoleName, relationshipName);
            }
            return collection;
        }

        /// <summary>
        /// Get the entity reference of a related entity using the specified
        /// combination of relationship name, and target role name.
        /// Only supports 2-way relationships.
        /// </summary>
        /// <param name="relationshipName">Name of the relationship in which targetRoleName is defined. Can be CSpace-qualified or not.</param>        
        /// <param name="targetRoleName">Name of the target role for the navigation. Indicates the direction of navigation across the relationship.</param>
        /// <returns>Reference for related entity of type TTargetEntity</returns>
        public EntityReference<TTargetEntity> GetRelatedReference<TTargetEntity>(string relationshipName, string targetRoleName)
            where TTargetEntity : class
        {
            EntityReference<TTargetEntity> reference = GetRelatedEndInternal(PrependNamespaceToRelationshipName(relationshipName), targetRoleName) as EntityReference<TTargetEntity>;
            if (reference == null)
            {
                throw EntityUtil.ExpectedReferenceGotCollection(typeof(TTargetEntity).Name, targetRoleName, relationshipName);
            }
            return reference;
        }

        /// <summary>
        /// Gets collection or ref of related entity for a particular navigation.
        /// </summary>
        /// <param name="navigation">
        /// Describes the relationship and navigation direction
        /// </param>
        /// <param name="relationshipFixer">
        /// Encapsulates information about the other end's type and cardinality,
        /// and knows how to create the other end
        /// </param>
        /// <returns></returns>
        internal RelatedEnd GetRelatedEnd(RelationshipNavigation navigation, IRelationshipFixer relationshipFixer)
        {
            RelatedEnd relatedEnd;

            if (TryGetCachedRelatedEnd(navigation.RelationshipName, navigation.To, out relatedEnd))
            {
                return relatedEnd;
            }
            else
            {
                relatedEnd = relationshipFixer.CreateSourceEnd(navigation, this);
                Debug.Assert(null != relatedEnd, "CreateSourceEnd should always return a valid RelatedEnd");

                return relatedEnd;
            }
        }

        /// <summary>
        /// Factory method for creating new related ends
        /// </summary>
        /// <typeparam name="TSourceEntity">Type of the source end</typeparam>
        /// <typeparam name="TTargetEntity">Type of the target end</typeparam>
        /// <param name="navigation">RelationshipNavigation to be set on the new RelatedEnd</param>
        /// <param name="sourceRoleMultiplicity">Multiplicity of the source role</param>
        /// <param name="targetRoleMultiplicity">Multiplicity of the target role</param>
        /// <param name="existingRelatedEnd">An existing related end to initialize instead of creating a new one</param>
        /// <returns>new EntityCollection or EntityReference, depending on the specified target multiplicity</returns>
        internal RelatedEnd CreateRelatedEnd<TSourceEntity, TTargetEntity>(RelationshipNavigation navigation, RelationshipMultiplicity sourceRoleMultiplicity, RelationshipMultiplicity targetRoleMultiplicity, RelatedEnd existingRelatedEnd)
            where TSourceEntity : class
            where TTargetEntity : class
        {
            IRelationshipFixer relationshipFixer = new RelationshipFixer<TSourceEntity, TTargetEntity>(sourceRoleMultiplicity, targetRoleMultiplicity);
            RelatedEnd relatedEnd = null;
            IEntityWrapper wrappedOwner = WrappedOwner;
            switch (targetRoleMultiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:
                case RelationshipMultiplicity.One:
                    if (existingRelatedEnd != null)
                    {
                        Debug.Assert(wrappedOwner.Context == null || wrappedOwner.MergeOption == MergeOption.NoTracking, "Expected null context when initializing an existing related end");
                        existingRelatedEnd.InitializeRelatedEnd(wrappedOwner, navigation, relationshipFixer);
                        relatedEnd = existingRelatedEnd;
                    }
                    else
                    {
                        relatedEnd = new EntityReference<TTargetEntity>(wrappedOwner, navigation, relationshipFixer);
                    }
                    break;
                case RelationshipMultiplicity.Many:
                    if (existingRelatedEnd != null)
                    {
                        Debug.Assert(wrappedOwner.Context == null || wrappedOwner.MergeOption == MergeOption.NoTracking, "Expected null context or NoTracking when initializing an existing related end");
                        existingRelatedEnd.InitializeRelatedEnd(wrappedOwner, navigation, relationshipFixer);
                        relatedEnd = existingRelatedEnd;
                    }
                    else
                    {
                        relatedEnd = new EntityCollection<TTargetEntity>(wrappedOwner, navigation, relationshipFixer);
                    }
                    break;
                default:
                    throw EntityUtil.InvalidEnumerationValue(typeof(RelationshipMultiplicity), (int)targetRoleMultiplicity);
            }

            // Verify that we can attach the context successfully before adding to our list of relationships
            if (wrappedOwner.Context != null)
            {
                relatedEnd.AttachContext(wrappedOwner.Context, wrappedOwner.MergeOption);
            }

            EnsureRelationshipsInitialized();
            _relationships.Add(relatedEnd);

            return relatedEnd;
        }

        /// <summary>
        /// Returns an enumeration of all the related ends.  The enumeration 
        /// will be empty if the relationships have not been populated.
        /// </summary>
        public IEnumerable<IRelatedEnd> GetAllRelatedEnds()
        {
            IEntityWrapper wrappedOwner = WrappedOwner;

            EntityType entityType;
            if (wrappedOwner.Context != null && wrappedOwner.Context.MetadataWorkspace != null && TryGetOwnerEntityType(out entityType))
            {
                // For attached scenario:
                // MEST: This returns RelatedEnds representing AssociationTypes which belongs to AssociationSets 
                // which have one end of EntitySet of wrappedOwner.Entity's EntitySet
                Debug.Assert(wrappedOwner.EntityKey != null, "null entityKey on a attached entity");
                EntitySet entitySet = wrappedOwner.Context.GetEntitySet(wrappedOwner.EntityKey.EntitySetName, wrappedOwner.EntityKey.EntityContainerName);
                foreach (AssociationEndMember endMember in GetAllTargetEnds(entityType, entitySet))
                {
                    yield return GetRelatedEnd(endMember.DeclaringType.FullName, endMember.Name);
                }
            }
            else
            {
                // Disconnected scenario
                // MEST: this returns RelatedEnds representing all AssociationTypes which have one end of type of wrappedOwner.Entity's type.
                // The returned collection of RelatedEnds is a superset of RelatedEnds which can make sense for a single entity, because
                // an entity can belong only to one EntitySet.  Note that the ideal would be to return the same collection as for attached scenario,
                // but it's not possible because we don't know to which EntitySet the wrappedOwner.Entity belongs.
                if (wrappedOwner.Entity != null)
                {
                    foreach (AssociationEndMember endMember in GetAllTargetEnds(wrappedOwner.IdentityType))
                    {
                        yield return GetRelatedEnd(endMember.DeclaringType.FullName, endMember.Name);
                    }
                }
            }
            yield break;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [OnSerializingAttribute]
        public void OnSerializing(StreamingContext context)
        {
            IEntityWrapper wrappedOwner = WrappedOwner;
            if (!(wrappedOwner.Entity is IEntityWithRelationships))
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.RelatedEnd_CannotSerialize("RelationshipManager"));
            }
            // If we are attached to a context we need to go fixup the detached entity key on any EntityReferences
            if (wrappedOwner.Context != null && wrappedOwner.MergeOption != MergeOption.NoTracking)
            {
                foreach (RelatedEnd relatedEnd in GetAllRelatedEnds())
                {
                    EntityReference reference = relatedEnd as EntityReference;
                    if (reference != null && reference.EntityKey != null)
                    {
                        reference.DetachedEntityKey = reference.EntityKey;
                    }
                }
            }
        }

        // ----------------
        // Internal Methods
        // ----------------

        internal bool HasRelationships
        {
            get { return _relationships != null; }
        }

        /// <summary>
        /// Add the rest of the graph, attached to this owner, to ObjectStateManager
        /// </summary>
        /// <param name="doAttach">if TRUE, the rest of the graph is attached directly as Unchanged 
        /// without calling AcceptChanges()</param>
        internal void AddRelatedEntitiesToObjectStateManager(bool doAttach)
        {
            if (null != _relationships)
            {
                bool doCleanup = true;
                try
                {
                    // Create a copy of this list because with self references, the set of relationships can change
                    foreach (RelatedEnd relatedEnd in Relationships)
                    {
                        relatedEnd.Include(/*addRelationshipAsUnchanged*/false, doAttach);
                    }
                    doCleanup = false;
                }
                finally
                {
                    // If error happens, while attaching entity graph to context, clean-up
                    // is done on the Owner entity and all its relating entities.
                    if (doCleanup)
                    {
                        IEntityWrapper wrappedOwner = WrappedOwner;
                        Debug.Assert(wrappedOwner.Context != null && wrappedOwner.Context.ObjectStateManager != null, "Null context or ObjectStateManager");

                        TransactionManager transManager = wrappedOwner.Context.ObjectStateManager.TransactionManager;

                        // The graph being attached is connected to graph already existing in the OSM only through "promoted" relationships
                        // (relationships which originally existed only in OSM between key entries and entity entries but later were
                        // "promoted" to normal relationships in EntityRef/Collection when the key entries were promoted).
                        // The cleanup code traverse all the graph being added to the OSM, so we have to disconnect it from the graph already
                        // existing in the OSM by degrading promoted relationships.
                        wrappedOwner.Context.ObjectStateManager.DegradePromotedRelationships();

                        NodeVisited = true;
                        RemoveRelatedEntitiesFromObjectStateManager(wrappedOwner);

                        EntityEntry entry;

                        Debug.Assert(doAttach == (transManager.IsAttachTracking), "In attach the recovery collection should be not null");

                        if (transManager.IsAttachTracking &&
                            transManager.PromotedKeyEntries.TryGetValue(wrappedOwner.Entity, out entry))
                        {
                            // This is executed only in the cleanup code from ObjectContext.AttachTo()
                            // If the entry was promoted in AttachTo(), it has to be degraded now instead of being deleted.
                            entry.DegradeEntry();
                        }
                        else
                        {
                            RelatedEnd.RemoveEntityFromObjectStateManager(wrappedOwner);
                        }
                    }
                }
            }
        }

        // Method is used to remove all entities and relationships, of a given entity
        // graph, from ObjectStateManager. This method is used when adding entity graph,
        // or a portion of it, raise exception. 
        internal static void RemoveRelatedEntitiesFromObjectStateManager(IEntityWrapper wrappedEntity)
        {
            Debug.Assert(wrappedEntity != null, "IEntityWrapper instance is null.");
            foreach (RelatedEnd relatedEnd in wrappedEntity.RelationshipManager.Relationships)
            {
                // only some of the related ends may have gotten attached, so just skip the ones that weren't
                if (relatedEnd.ObjectContext != null)
                {
                    Debug.Assert(!relatedEnd.UsingNoTracking, "Shouldn't be touching the state manager with entities that were retrieved with NoTracking");
                    relatedEnd.Exclude();
                    relatedEnd.DetachContext();
                }
            }
        }

        // Remove entity from its relationships and do cascade delete if required.
        // All removed relationships are marked for deletion and all cascade deleted 
        // entitites are also marked for deletion.
        internal void RemoveEntityFromRelationships()
        {
            if (null != _relationships)
            {
                foreach (RelatedEnd relatedEnd in Relationships)
                {
                    relatedEnd.RemoveAll();
                }
            }
        }

        /// <summary>
        /// Traverse the relationships and find all the dependent ends that contain FKs, then attempt
        /// to null all of those FKs.
        /// </summary>
        internal void NullAllFKsInDependentsForWhichThisIsThePrincipal()
        {
            if (_relationships != null)
            {
                // Build a list of the dependent RelatedEnds because with overlapping FKs we could
                // end up removing a relationship before we have suceeded in nulling all the FK values
                // for that relationship.
                var dependentEndsToProcess = new List<EntityReference>();
                foreach (RelatedEnd relatedEnd in Relationships)
                {
                    if (relatedEnd.IsForeignKey)
                    {
                        foreach (IEntityWrapper dependent in relatedEnd.GetWrappedEntities())
                        {
                            var dependentEnd = relatedEnd.GetOtherEndOfRelationship(dependent);
                            if (dependentEnd.IsDependentEndOfReferentialConstraint(checkIdentifying: false))
                            {
                                Debug.Assert(dependentEnd is EntityReference, "Dependent end in FK relationship should always be a reference.");
                                dependentEndsToProcess.Add((EntityReference)dependentEnd);
                            }
                        }
                    }
                }
                foreach (EntityReference dependentEnd in dependentEndsToProcess)
                {
                    dependentEnd.NullAllForeignKeys();
                }
            }
        }

        // Removes entity from its relationships.
        // Relationship entries are removed from ObjectStateManager if owner is in Added state 
        // or when owner is "many" end of the relationship
        internal void DetachEntityFromRelationships(EntityState ownerEntityState)
        {
            if (null != _relationships)
            {
                foreach (RelatedEnd relatedEnd in Relationships)
                {
                    relatedEnd.DetachAll(ownerEntityState);
                }
            }
        }

        //For a given relationship removes passed in entity from owners relationship
        internal void RemoveEntity(string toRole, string relationshipName, IEntityWrapper wrappedEntity)
        {
            Debug.Assert(wrappedEntity != null, "IEntityWrapper instance is null.");
            RelatedEnd relatedEnd;
            if (TryGetCachedRelatedEnd(relationshipName, toRole, out relatedEnd))
            {
                relatedEnd.Remove(wrappedEntity, false);
            }
        }

        internal void ClearRelatedEndWrappers()
        {
            if (_relationships != null)
            {
                foreach (IRelatedEnd relatedEnd in Relationships)
                {
                    ((RelatedEnd)relatedEnd).ClearWrappedValues();
                }
            }
        }

        // Method used to retrieve properties from principal entities.
        // Parameter includeOwnValues means that values from current entity should be also added to "properties"
        // includeOwnValues is false only when this method is called from ObjectStateEntry.AcceptChanges()
        // Parmeter "visited" is a set containig entities which were already visited during traversing the graph.
        // If _owner already exists in the set, it means that there is a cycle in the graph of relationships with RI Constraints.
        internal void RetrieveReferentialConstraintProperties(out Dictionary<string, KeyValuePair<object, IntBox>> properties, HashSet<object> visited, bool includeOwnValues)
        {
            IEntityWrapper wrappedOwner = WrappedOwner;
            Debug.Assert(wrappedOwner.Entity != null);
            Debug.Assert(visited != null);

            // Dictionary< propertyName, <propertyValue, counter>>
            properties = new Dictionary<string, KeyValuePair<object, IntBox>>();

            EntityKey ownerKey = wrappedOwner.EntityKey;
            Debug.Assert((object)ownerKey != null);

            // If the key is temporary, get values of referential constraint properties from principal entities
            if (ownerKey.IsTemporary)
            {
                // Find property names which should be retrieved
                List<string> propertiesToRetrieve;
                bool propertiesToPropagateExist; // not used

                this.FindNamesOfReferentialConstraintProperties(out propertiesToRetrieve, out propertiesToPropagateExist, skipFK: false);

                if (propertiesToRetrieve != null)
                {
                    // At first try to retrieve properties from entities which are in collections or references.
                    // This is the most common scenario.
                    // Only if properties couldn't be retrieved this way, try to retrieve properties from related stubs.

                    if (_relationships != null)
                    {
                        // Not using defensive copy here since RetrieveReferentialConstraintProperties should not cause change in underlying
                        // _relationships collection.
                        foreach (RelatedEnd relatedEnd in _relationships)
                        {
                            // NOTE: If the following call throws UnableToRetrieveReferentialConstraintProperties,
                            //       it means that properties couldn't be found in indirectly related entities,
                            //       so it doesn't make sense to search for properties in directly related stubs,
                            //       so exception is not being caught here.
                            relatedEnd.RetrieveReferentialConstraintProperties(properties, visited);
                        }
                    }

                    // Check if all properties were retrieved.
                    // There are 3 scenarios in which not every expected property can be retrieved:
                    // 1. There is no related entity from which the property is supposed to be retrieved.
                    // 2. Related entity which supposed to contains the property doesn't have fixed entity key.
                    // 3. Property should be retrieved from related key entry

                    if (!CheckIfAllPropertiesWereRetrieved(properties, propertiesToRetrieve))
                    {
                        // Properties couldn't be found in entities in collections or refrences.
                        // Try to find missing properties in related key entries.
                        // This process is slow but it is not a common case.
                        EntityEntry entry = wrappedOwner.Context.ObjectStateManager.FindEntityEntry(ownerKey);
                        Debug.Assert(entry != null, "Owner entry not found in the object state manager");
                        entry.RetrieveReferentialConstraintPropertiesFromKeyEntries(properties);

                        // Check again if all properties were retrieved.
                        if (!CheckIfAllPropertiesWereRetrieved(properties, propertiesToRetrieve))
                        {
                            throw EntityUtil.UnableToRetrieveReferentialConstraintProperties();
                        }
                    }
                }
            }

            // 1. If key is temporary, properties from principal entities were retrieved above. 
            //    The other key properties are properties which are not Dependent end of some Referential Constraint.
            // 2. If key is not temporary and this method was not called from AcceptChanges() - all key values
            //    of the current entity are added to 'properties'.
            if (!ownerKey.IsTemporary || includeOwnValues)
            {
                // NOTE this part is never executed when the method is called from ObjectStateManager.AcceptChanges(),
                //      so we don't try to "retrieve" properties from the the same (callers) entity.
                EntityEntry entry = wrappedOwner.Context.ObjectStateManager.FindEntityEntry(ownerKey);
                Debug.Assert(entry != null, "Owner entry not found in the object state manager");
                entry.GetOtherKeyProperties(properties);
            }
        }

        // properties dictionary contains name of property, its value and coutner saying how many times this property was retrieved from principal entities
        private static bool CheckIfAllPropertiesWereRetrieved(Dictionary<string, KeyValuePair<object, IntBox>> properties, List<string> propertiesToRetrieve)
        {
            Debug.Assert(properties != null);
            Debug.Assert(propertiesToRetrieve != null);

            bool isSuccess = true;

            List<int> countersCopy = new List<int>();
            ICollection<KeyValuePair<object, IntBox>> values = properties.Values;

            // Create copy of counters (needed in case of failure)
            foreach (KeyValuePair<object, IntBox> valueCounterPair in values)
            {
                countersCopy.Add(valueCounterPair.Value.Value);
            }

            foreach (string name in propertiesToRetrieve)
            {
                if (!properties.ContainsKey(name))
                {
                    isSuccess = false;
                    break;
                }

                KeyValuePair<object, IntBox> valueCounterPair = properties[name];
                valueCounterPair.Value.Value = valueCounterPair.Value.Value - 1;
                if (valueCounterPair.Value.Value < 0)
                {
                    isSuccess = false;
                    break;
                }
            }

            // Check if all the coutners equal 0
            if (isSuccess)
            {
                foreach (KeyValuePair<object, IntBox> valueCounterPair in values)
                {
                    if (valueCounterPair.Value.Value != 0)
                    {
                        isSuccess = false;
                        break;
                    }
                }
            }

            // Restore counters in case of failure
            if (!isSuccess)
            {
                IEnumerator<int> enumerator = countersCopy.GetEnumerator();
                foreach (KeyValuePair<object, IntBox> valueCounterPair in values)
                {
                    enumerator.MoveNext();
                    valueCounterPair.Value.Value = enumerator.Current;
                }
            }

            return isSuccess;
        }


        // Check consistency between properties of current entity and Principal entities
        // If some of Principal entities don't exist or some property cannot be checked - this is violation of RI Constraints
        internal void CheckReferentialConstraintProperties(EntityEntry ownerEntry)
        {
            Debug.Assert(ownerEntry != null);

            List<string> propertiesToRetrieve; // used to check if the owner is a dependent end of some RI Constraint
            bool propertiesToPropagateExist;   // used to check if the owner is a principal end of some RI Constraint
            this.FindNamesOfReferentialConstraintProperties(out propertiesToRetrieve, out propertiesToPropagateExist, skipFK: false);

            if ((propertiesToRetrieve != null || propertiesToPropagateExist) &&
                _relationships != null)
            {
                // Not using defensive copy here since CheckReferentialConstraintProperties should not cause change in underlying
                // _relationships collection.
                foreach (RelatedEnd relatedEnd in _relationships)
                {
                    if (!relatedEnd.CheckReferentialConstraintProperties(ownerEntry))
                    {
                        throw EntityUtil.InconsistentReferentialConstraintProperties();
                    }
                }
            }
        }

        // ----------------
        // Private Methods
        // ----------------

        // This method is required to maintain compatibility with the v1 binary serialization format. 
        // In particular, it recreates a entity wrapper from the serialized owner.
        // Note that this is only expected to work for non-POCO entities, since serialization of POCO
        // entities will not result in serialization of the RelationshipManager or its related objects.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [OnDeserialized()]
        public void OnDeserialized(StreamingContext context)
        {
            // Note that when deserializing, the context is always null since we never serialize
            // the context with the entity.
            _wrappedOwner = EntityWrapperFactory.WrapEntityUsingContext(_owner, null);
        }

        /// <summary>
        /// Searches the list of relationships for an entry with the specified relationship name and role names
        /// </summary>
        /// <param name="relationshipName">CSpace-qualified name of the relationship</param>        
        /// <param name="targetRoleName">name of the target role</param>
        /// <param name="relatedEnd">the RelatedEnd if found, otherwise null</param>
        /// <returns>true if the entry found, false otherwise</returns>
        private bool TryGetCachedRelatedEnd(string relationshipName, string targetRoleName, out RelatedEnd relatedEnd)
        {
            relatedEnd = null;
            if (null != _relationships)
            {
                // Not using defensive copy here since loop should not cause change in underlying
                // _relationships collection.
                foreach (RelatedEnd end in _relationships)
                {
                    RelationshipNavigation relNav = end.RelationshipNavigation;
                    if (relNav.RelationshipName == relationshipName && relNav.To == targetRoleName)
                    {
                        relatedEnd = end;
                        return true;
                    }
                }
            }
            return false;
        }

        // Find properties which are Dependent/Principal ends of some referential constraint
        // Returned lists are never null.
        // NOTE This method will be removed when bug 505935 is solved
        // Returns true if any FK relationships were skipped so that they can be checked again after fixup
        internal bool FindNamesOfReferentialConstraintProperties(out List<string> propertiesToRetrieve, out bool propertiesToPropagateExist, bool skipFK)
        {
            IEntityWrapper wrappedOwner = WrappedOwner;
            Debug.Assert(wrappedOwner.Entity != null);
            EntityKey ownerKey = wrappedOwner.EntityKey;
            EntityUtil.CheckEntityKeyNull(ownerKey);

            propertiesToRetrieve = null;
            propertiesToPropagateExist = false;

            EntityUtil.CheckContextNull(wrappedOwner.Context);
            EntitySet entitySet = ownerKey.GetEntitySet(wrappedOwner.Context.MetadataWorkspace);
            Debug.Assert(entitySet != null, "Unable to find entity set");

            // Get association types in which current entity's type is one of the ends.
            List<AssociationSet> associations = MetadataHelper.GetAssociationsForEntitySet(entitySet);

            bool skippedFK = false;
            // Find key property names which are part of referential integrity constraints
            foreach (AssociationSet association in associations)
            {
                // NOTE ReferentialConstraints collection currently can contain 0 or 1 element
                if (skipFK && association.ElementType.IsForeignKey)
                {
                    skippedFK = true;
                }
                else
                {
                    foreach (ReferentialConstraint constraint in association.ElementType.ReferentialConstraints)
                    {
                        if (constraint.ToRole.TypeUsage.EdmType == entitySet.ElementType.GetReferenceType())
                        {
                            // lazy creation of the list
                            propertiesToRetrieve = propertiesToRetrieve ?? new List<string>();
                            foreach (EdmProperty property in constraint.ToProperties)
                            {
                                propertiesToRetrieve.Add(property.Name);
                            }
                        }
                        // There are schemas, in which relationship has the same entitySet on both ends
                        // that is why following 'if' statement is not inside of 'else' of previous 'if' statement
                        if (constraint.FromRole.TypeUsage.EdmType == entitySet.ElementType.GetReferenceType())
                        {
                            propertiesToPropagateExist = true;
                        }
                    }
                }
            }
            return skippedFK;
        }

        /// <summary>
        /// Helper method to validate consistency of RelationshipManager instances
        /// </summary>
        /// <param name="entity">entity to compare against</param>
        /// <returns>True if entity is the owner of this RelationshipManager, otherwise false</returns>
        internal bool IsOwner(IEntityWrapper wrappedEntity)
        {
            IEntityWrapper wrappedOwner = WrappedOwner;
            Debug.Assert(wrappedEntity != null, "IEntityWrapper instance is null.");
            return Object.ReferenceEquals(wrappedEntity.Entity, wrappedOwner.Entity);
        }

        /// <summary>
        /// Calls AttachContext on each RelatedEnd referenced by this manager.
        /// </summary>
        internal void AttachContextToRelatedEnds(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
        {
            Debug.Assert(null != context, "context");
            Debug.Assert(null != entitySet, "entitySet");
            if (null != _relationships)
            {
                // If GetAllRelatedEnds was called while the entity was not attached to the context
                // then _relationships may contain RelatedEnds that do not belong in based on the
                // entity set that the owner ultimately was attached to.  This means that when attaching
                // we need to trim the list to get rid of those RelatedEnds.
                // It is possible that the RelatedEnds may have been obtained explicitly rather than through
                // GetAllRelatedEnds.  If this is the case, then we prune anyway unless the RelatedEnd actually
                // has something attached to it, in which case we try to attach the context which will cause
                // an exception to be thrown.  This is all a bit messy, but it's the best we could do given that
                // GetAllRelatedEnds was implemented in 3.5sp1 without taking MEST into account.
                // Note that the Relationships property makes a copy so we can modify the list while iterating
                foreach (RelatedEnd relatedEnd in Relationships)
                {
                    EdmType relationshipType;
                    RelationshipSet relationshipSet;
                    relatedEnd.FindRelationshipSet(context, entitySet, out relationshipType, out relationshipSet);
                    if (relationshipSet != null || !relatedEnd.IsEmpty())
                    {
                        relatedEnd.AttachContext(context, entitySet, mergeOption);
                    }
                    else
                    {
                        _relationships.Remove(relatedEnd);
                    }
                }
            }
        }

        /// <summary>
        /// Calls AttachContext on each RelatedEnd referenced by this manager and also on all the enties
        /// referenced by that related end.
        /// </summary>
        internal void ResetContextOnRelatedEnds(ObjectContext context, EntitySet entitySet, MergeOption mergeOption)
        {
            Debug.Assert(null != context, "context");
            Debug.Assert(null != entitySet, "entitySet");
            if (null != _relationships)
            {
                foreach (RelatedEnd relatedEnd in Relationships)
                {
                    relatedEnd.AttachContext(context, entitySet, mergeOption);
                    foreach (IEntityWrapper wrappedEntity in relatedEnd.GetWrappedEntities())
                    {
                        wrappedEntity.ResetContext(context, relatedEnd.GetTargetEntitySetFromRelationshipSet(), mergeOption);
                    }
                }
            }
        }

        /// <summary>
        /// Calls DetachContext on each RelatedEnd referenced by this manager.
        /// </summary>
        internal void DetachContextFromRelatedEnds()
        {
            if (null != _relationships)
            {
                // Not using defensive copy here since DetachContext should not cause change in underlying
                // _relationships collection.
                foreach (RelatedEnd relatedEnd in _relationships)
                {
                    relatedEnd.DetachContext();
                }
            }
        }

        // --------------------
        // Internal definitions
        // --------------------

        [Conditional("DEBUG")]
        internal void VerifyIsNotRelated()
        {
            if (this._relationships != null)
            {
                foreach (var r in this._relationships)
                {
                    if (!r.IsEmpty())
                    {
                        Debug.Assert(false, "Cannot change a state of a Deleted entity if the entity has other than deleted relationships with other entities.");
                    }
                }
            }
        }
    }
}
