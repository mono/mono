//------------------------------------------------------------------------------
// <copyright file="Shaper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Data.Objects.Internal;
    using System.Data.Spatial;
    using System.Diagnostics;
    using System.Reflection;

    /// <summary>
    /// Shapes store reader values into EntityClient/ObjectQuery results. Also maintains 
    /// state used by materializer delegates.
    /// </summary>
    internal abstract class Shaper
    {
        #region constructor

        internal Shaper(DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption, int stateCount)
        {
            Debug.Assert(context == null || workspace == context.MetadataWorkspace, "workspace must match context's workspace");

            this.Reader = reader;
            this.MergeOption = mergeOption;
            this.State = new object[stateCount];
            this.Context = context;
            this.Workspace = workspace;
            this.AssociationSpaceMap = new Dictionary<AssociationType, AssociationType>();
            this.spatialReader = new Singleton<DbSpatialDataReader>(CreateSpatialDataReader);
        }
        
        #endregion

        #region OnMaterialized storage

        /// <summary>
        /// Keeps track of the entities that have been materialized so that we can fire an OnMaterialized
        /// for them before returning control to the caller.
        /// </summary>
        private IList<IEntityWrapper> _materializedEntities;

        #endregion

        #region runtime callable/accessible code

        // Code in this section is called from the delegates produced by the Translator.  It   
        // may not show up if you search using Find All References...use Find in Files instead.
        // 
        // Many items on this class are public, simply to make the job of producing the
        // expressions that use them simpler.  If you have a hankering to make them private,
        // you will need to modify the code in the Translator that does the GetMethod/GetField
        // to use BindingFlags.NonPublic | BindingFlags.Instance as well.
        //
        // Debug.Asserts that fire from the code in this region will probably create a 
        // SecurityException in the Coordinator's Read method since those are restricted when 
        // running the Shaper.

        /// <summary>
        /// The store data reader we're pulling data from
        /// </summary>
        public readonly DbDataReader Reader;

        /// <summary>
        /// The state slots we use in the coordinator expression.
        /// </summary>
        public readonly object[] State;

        /// <summary>
        /// The context the shaper is performing for.
        /// </summary>
        public readonly ObjectContext Context;

        /// <summary>
        /// The workspace we are performing for; yes we could get it from the context, but
        /// it's much easier to just have it handy.
        /// </summary>
        public readonly MetadataWorkspace Workspace;

        /// <summary>
        /// The merge option this shaper is performing under/for.
        /// </summary>
        public readonly MergeOption MergeOption;

        /// <summary>
        /// A mapping of CSpace AssociationTypes to OSpace AssociationTypes
        /// Used for faster lookup/retrieval of AssociationTypes during materialization
        /// </summary>
        private readonly Dictionary<AssociationType, AssociationType> AssociationSpaceMap;

        /// <summary>
        /// Caches Tuples of EntitySet, AssociationType, and source member name for which RelatedEnds exist.
        /// </summary>
        private HashSet<Tuple<string, string, string>> _relatedEndCache;

        /// <summary>
        /// Utility method used to evaluate a multi-discriminator column map. Takes
        /// discriminator values and determines the appropriate entity type, then looks up 
        /// the appropriate handler and invokes it.
        /// </summary>
        public TElement Discriminate<TElement>(object[] discriminatorValues, Func<object[], EntityType> discriminate, KeyValuePair<EntityType, Func<Shaper, TElement>>[] elementDelegates)
        {
            EntityType entityType = discriminate(discriminatorValues);
            Func<Shaper, TElement> elementDelegate = null;
            foreach (KeyValuePair<EntityType, Func<Shaper, TElement>> typeDelegatePair in elementDelegates)
            {
                if (typeDelegatePair.Key == entityType)
                {
                    elementDelegate = typeDelegatePair.Value;
                }
            }
            return elementDelegate(this);
        }

        public IEntityWrapper HandleEntityNoTracking<TEntity>(IEntityWrapper wrappedEntity)
        {
            Debug.Assert(null != wrappedEntity, "wrapped entity is null");
            RegisterMaterializedEntityForEvent(wrappedEntity);
            return wrappedEntity;
        }

        /// <summary>
        /// REQUIRES:: entity is not null and MergeOption is OverwriteChanges or PreserveChanges
        /// Handles state management for an entity returned by a query. Where an existing entry
        /// exists, updates that entry and returns the existing entity. Otherwise, the entity
        /// passed in is returned.
        /// </summary>
        public IEntityWrapper HandleEntity<TEntity>(IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySet)
        {
            Debug.Assert(MergeOption.NoTracking != this.MergeOption, "no need to HandleEntity if there's no tracking");
            Debug.Assert(MergeOption.AppendOnly != this.MergeOption, "use HandleEntityAppendOnly instead...");
            Debug.Assert(null != wrappedEntity, "wrapped entity is null");
            Debug.Assert(null != wrappedEntity.Entity, "if HandleEntity is called, there must be an entity");

            IEntityWrapper result = wrappedEntity;

            // no entity set, so no tracking is required for this entity
            if (null != (object)entityKey)
            {
                Debug.Assert(null != entitySet, "if there is an entity key, there must also be an entity set");

                // check for an existing entity with the same key
                EntityEntry existingEntry = this.Context.ObjectStateManager.FindEntityEntry(entityKey);
                if (null != existingEntry && !existingEntry.IsKeyEntry)
                {
                    Debug.Assert(existingEntry.EntityKey.Equals(entityKey), "Found ObjectStateEntry with wrong EntityKey");
                    UpdateEntry<TEntity>(wrappedEntity, existingEntry);
                    result = existingEntry.WrappedEntity;
                }
                else
                {
                    RegisterMaterializedEntityForEvent(result);
                    if (null == existingEntry)
                    {
                        Context.ObjectStateManager.AddEntry(wrappedEntity, entityKey, entitySet, "HandleEntity", false);
                    }
                    else
                    {
                        Context.ObjectStateManager.PromoteKeyEntry(existingEntry, wrappedEntity, (IExtendedDataRecord)null, false, /*setIsLoaded*/ true, /*keyEntryInitialized*/ false, "HandleEntity");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// REQUIRES:: entity exists; MergeOption is AppendOnly
        /// Handles state management for an entity with the given key. When the entity already exists  
        /// in the state manager, it is returned directly. Otherwise, the entityDelegate is invoked and  
        /// the resulting entity is returned.
        /// </summary>
        public IEntityWrapper HandleEntityAppendOnly<TEntity>(Func<Shaper, IEntityWrapper> constructEntityDelegate, EntityKey entityKey, EntitySet entitySet)
        {
            Debug.Assert(this.MergeOption == MergeOption.AppendOnly, "only use HandleEntityAppendOnly when MergeOption is AppendOnly");
            Debug.Assert(null != constructEntityDelegate, "must provide delegate to construct the entity");

            IEntityWrapper result;

            if (null == (object)entityKey)
            {
                // no entity set, so no tracking is required for this entity, just
                // call the delegate to "materialize" it.
                result = constructEntityDelegate(this);
                RegisterMaterializedEntityForEvent(result);
            }
            else
            {
                Debug.Assert(null != entitySet, "if there is an entity key, there must also be an entity set");

                // check for an existing entity with the same key
                EntityEntry existingEntry = this.Context.ObjectStateManager.FindEntityEntry(entityKey);
                if (null != existingEntry && !existingEntry.IsKeyEntry)
                {
                    Debug.Assert(existingEntry.EntityKey.Equals(entityKey), "Found ObjectStateEntry with wrong EntityKey");
                    if (typeof(TEntity) != existingEntry.WrappedEntity.IdentityType)
                    {
                        throw EntityUtil.RecyclingEntity(existingEntry.EntityKey, typeof(TEntity), existingEntry.WrappedEntity.IdentityType);
                    }

                    if (EntityState.Added == existingEntry.State)
                    {
                        throw EntityUtil.AddedEntityAlreadyExists(existingEntry.EntityKey);
                    }
                    result = existingEntry.WrappedEntity;
                }
                else
                {
                    // We don't already have the entity, so construct it
                    result = constructEntityDelegate(this);
                    RegisterMaterializedEntityForEvent(result);
                    if (null == existingEntry)
                    {
                        Context.ObjectStateManager.AddEntry(result, entityKey, entitySet, "HandleEntity", false);
                    }
                    else
                    {
                        Context.ObjectStateManager.PromoteKeyEntry(existingEntry, result, (IExtendedDataRecord)null, false, /*setIsLoaded*/ true, /*keyEntryInitialized*/ false, "HandleEntity");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Call to ensure a collection of full-spanned elements are added
        /// into the state manager properly.  We registers an action to be called
        /// when the collection is closed that pulls the collection of full spanned 
        /// objects into the state manager.
        /// </summary>
        public IEntityWrapper HandleFullSpanCollection<T_SourceEntity, T_TargetEntity>(IEntityWrapper wrappedEntity, Coordinator<T_TargetEntity> coordinator, AssociationEndMember targetMember)
        {
            Debug.Assert(null != wrappedEntity, "wrapped entity is null");
            if (null != wrappedEntity.Entity)
            {
                coordinator.RegisterCloseHandler((state, spannedEntities) => FullSpanAction(wrappedEntity, spannedEntities, targetMember));
            }
            return wrappedEntity;
        }

        /// <summary>
        /// Call to ensure a single full-spanned element is added into 
        /// the state manager properly.
        /// </summary>
        public IEntityWrapper HandleFullSpanElement<T_SourceEntity, T_TargetEntity>(IEntityWrapper wrappedSource, IEntityWrapper wrappedSpannedEntity, AssociationEndMember targetMember)
        {
            Debug.Assert(null != wrappedSource, "wrapped entity is null");
            if (wrappedSource.Entity == null)
            {
                return wrappedSource;
            }
            List<IEntityWrapper> spannedEntities = null;
            if (wrappedSpannedEntity.Entity != null)
            {
                // There was a single entity in the column
                // Create a list so we can perform the same logic as a collection of entities
                spannedEntities = new List<IEntityWrapper>(1);
                spannedEntities.Add(wrappedSpannedEntity);
            }
            else
            {
                EntityKey sourceKey = wrappedSource.EntityKey;
                CheckClearedEntryOnSpan(null, wrappedSource, sourceKey, targetMember);
            }
            FullSpanAction(wrappedSource, spannedEntities, targetMember);
            return wrappedSource;
        }

        /// <summary>
        /// Call to ensure a target entities key is added into the state manager 
        /// properly
        /// </summary>
        public IEntityWrapper HandleRelationshipSpan<T_SourceEntity>(IEntityWrapper wrappedEntity, EntityKey targetKey, AssociationEndMember targetMember)
        {
            if (null == wrappedEntity.Entity)
            {
                return wrappedEntity;
            }
            Debug.Assert(targetMember != null);
            Debug.Assert(targetMember.RelationshipMultiplicity == RelationshipMultiplicity.One || targetMember.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne);

            EntityKey sourceKey = wrappedEntity.EntityKey;
            AssociationEndMember sourceMember = MetadataHelper.GetOtherAssociationEnd(targetMember);
            CheckClearedEntryOnSpan(targetKey, wrappedEntity, sourceKey, targetMember);

            if (null != (object)targetKey)
            {
                EntitySet targetEntitySet;

                EntityContainer entityContainer = this.Context.MetadataWorkspace.GetEntityContainer(
                    targetKey.EntityContainerName, DataSpace.CSpace);

                // find the correct AssociationSet
                AssociationSet associationSet = MetadataHelper.GetAssociationsForEntitySetAndAssociationType(entityContainer,
                    targetKey.EntitySetName, (AssociationType)(targetMember.DeclaringType), targetMember.Name, out targetEntitySet);
                Debug.Assert(associationSet != null, "associationSet should not be null");

                ObjectStateManager manager = Context.ObjectStateManager;
                EntityState newEntryState;
                // If there is an existing relationship entry, update it based on its current state and the MergeOption, otherwise add a new one            
                if (!ObjectStateManager.TryUpdateExistingRelationships(this.Context, this.MergeOption, associationSet, sourceMember, sourceKey, wrappedEntity, targetMember, targetKey, /*setIsLoaded*/ true, out newEntryState))
                {
                    // Try to find a state entry for the target key
                    EntityEntry targetEntry = null;
                    if (!manager.TryGetEntityEntry(targetKey, out targetEntry))
                    {
                        // no entry exists for the target key
                        // create a key entry for the target
                        targetEntry = manager.AddKeyEntry(targetKey, targetEntitySet);
                    }

                    // SQLBU 557105. For 1-1 relationships we have to take care of the relationships of targetEntity
                    bool needNewRelationship = true;
                    switch (sourceMember.RelationshipMultiplicity)
                    {
                        case RelationshipMultiplicity.ZeroOrOne:
                        case RelationshipMultiplicity.One:
                            // devnote: targetEntry can be a key entry (targetEntry.Entity == null), 
                            // but it that case this parameter won't be used in TryUpdateExistingRelationships
                            needNewRelationship = !ObjectStateManager.TryUpdateExistingRelationships(this.Context,
                                this.MergeOption,
                                associationSet,
                                targetMember,
                                targetKey,
                                targetEntry.WrappedEntity,
                                sourceMember,
                                sourceKey,
                                /*setIsLoaded*/ true,
                                out newEntryState);

                            // It is possible that as part of removing existing relationships, the key entry was deleted
                            // If that is the case, recreate the key entry
                            if (targetEntry.State == EntityState.Detached)
                            {
                                targetEntry = manager.AddKeyEntry(targetKey, targetEntitySet);
                            }
                            break;
                        case RelationshipMultiplicity.Many:
                            // we always need a new relationship with Many-To-Many, if there was no exact match between these two entities, so do nothing                                
                            break;
                        default:
                            Debug.Assert(false, "Unexpected sourceMember.RelationshipMultiplicity");
                            break;
                    }

                    if (needNewRelationship)
                    {

                        // If the target entry is a key entry, then we need to add a relation 
                        //   between the source and target entries
                        // If we are in a state where we just need to add a new Deleted relation, we
                        //   only need to do that and not touch the related ends
                        // If the target entry is a full entity entry, then we need to add 
                        //   the target entity to the source collection or reference
                        if (targetEntry.IsKeyEntry || newEntryState == EntityState.Deleted)
                        {
                            // Add a relationship between the source entity and the target key entry
                            RelationshipWrapper wrapper = new RelationshipWrapper(associationSet, sourceMember.Name, sourceKey, targetMember.Name, targetKey);
                            manager.AddNewRelation(wrapper, newEntryState);
                        }
                        else
                        {
                            Debug.Assert(!targetEntry.IsRelationship, "how IsRelationship?");
                            if (targetEntry.State != EntityState.Deleted)
                            {
                                // The entry contains an entity, do collection or reference fixup
                                // This will also try to create a new relationship entry or will revert the delete on an existing deleted relationship
                                ObjectStateManager.AddEntityToCollectionOrReference(
                                    this.MergeOption, wrappedEntity, sourceMember,
                                    targetEntry.WrappedEntity,
                                    targetMember,
                                    /*setIsLoaded*/ true,
                                    /*relationshipAlreadyExists*/ false,
                                    /* inKeyEntryPromotion */ false);
                            }
                            else
                            {
                                // if the target entry is deleted, then the materializer needs to create a deleted relationship
                                // between the entity and the target entry so that if the entity is deleted, the update
                                // pipeline can find the relationship (even though it is deleted)
                                RelationshipWrapper wrapper = new RelationshipWrapper(associationSet, sourceMember.Name, sourceKey, targetMember.Name, targetKey);
                                manager.AddNewRelation(wrapper, EntityState.Deleted);
                            }
                        }
                    }
                }
            }
            else
            {
                RelatedEnd relatedEnd;
                if(TryGetRelatedEnd(wrappedEntity, (AssociationType)targetMember.DeclaringType, sourceMember.Name, targetMember.Name, out relatedEnd))
                {
                    SetIsLoadedForSpan(relatedEnd, false);
                }
            }

            // else there is nothing else for us to do, the relationship has been handled already   
            return wrappedEntity;
        }

        private bool TryGetRelatedEnd(IEntityWrapper wrappedEntity, AssociationType associationType, string sourceEndName, string targetEndName, out RelatedEnd relatedEnd)
        {
            Debug.Assert(associationType.DataSpace == DataSpace.CSpace);

            // Get the OSpace AssociationType
            AssociationType oSpaceAssociation;
            if (!AssociationSpaceMap.TryGetValue((AssociationType)associationType, out oSpaceAssociation))
            {
                oSpaceAssociation = this.Workspace.GetItemCollection(DataSpace.OSpace).GetItem<AssociationType>(associationType.FullName);
                AssociationSpaceMap[(AssociationType)associationType] = oSpaceAssociation;
            }

            AssociationEndMember sourceEnd = null;
            AssociationEndMember targetEnd = null;
            foreach (var end in oSpaceAssociation.AssociationEndMembers)
            {
                if (end.Name == sourceEndName)
                {
                    sourceEnd = end;
                }
                else if (end.Name == targetEndName)
                {
                    targetEnd = end;
                }
            }

            if (sourceEnd != null && targetEnd != null)
            {
                bool createRelatedEnd = false;
                if (wrappedEntity.EntityKey == null)
                {
                    // Free-floating entity--key is null, so don't have EntitySet for validation, so always create RelatedEnd
                    createRelatedEnd = true;
                }
                else
                {
                    // It is possible, because of MEST, that we're trying to load a relationship that is valid for this EntityType
                    // in metadata, but is not valid in this case because the specific entity is part of an EntitySet that is not
                    // mapped in any AssociationSet for this association type.                  
                    // The metadata structure makes checking for this somewhat time consuming because of the loop required.
                    // Because the whole reason for this method is perf, we try to reduce the
                    // impact of this check by caching positive hits in a HashSet so we don't have to do this for
                    // every entity in a query.  (We could also cache misses, but since these only happen in MEST, which
                    // is not common, we decided not to slow down the normal non-MEST case anymore by doing this.)
                    var entitySet = wrappedEntity.EntityKey.GetEntitySet(this.Workspace);
                    var relatedEndKey = Tuple.Create<string, string, string>(entitySet.Identity, associationType.Identity, sourceEndName);

                    if (_relatedEndCache == null)
                    {
                        _relatedEndCache = new HashSet<Tuple<string, string, string>>();
                    }

                    if (_relatedEndCache.Contains(relatedEndKey))
                    {
                        createRelatedEnd = true;
                    }
                    else
                    {
                        foreach (var entitySetBase in entitySet.EntityContainer.BaseEntitySets)
                        {
                            if ((EdmType)entitySetBase.ElementType == associationType)
                            {
                                if (((AssociationSet)entitySetBase).AssociationSetEnds[sourceEndName].EntitySet == entitySet)
                                {
                                    createRelatedEnd = true;
                                    _relatedEndCache.Add(relatedEndKey);
                                    break;
                                }
                            }
                        }
                    }
                }
                if (createRelatedEnd)
                {
                    relatedEnd = LightweightCodeGenerator.GetRelatedEnd(wrappedEntity.RelationshipManager, sourceEnd, targetEnd, null);
                    return true;
                }
            }

            relatedEnd = null;
            return false;
        }
        
        /// <summary>
        /// Sets the IsLoaded flag to "true"
        /// There are also rules for when this can be set based on MergeOption and the current value(s) in the related end.
        /// </summary>
        private void SetIsLoadedForSpan(RelatedEnd relatedEnd, bool forceToTrue)
        {
            Debug.Assert(relatedEnd != null, "RelatedEnd should not be null");
            
            // We can now say this related end is "Loaded" 
            // The cases where we should set this to true are:
            // AppendOnly: the related end is empty and does not point to a stub
            // PreserveChanges: the related end is empty and does not point to a stub (otherwise, an Added item exists and IsLoaded should not change)
            // OverwriteChanges: always
            // NoTracking: always
            if (!forceToTrue)
            {
                // Detect the empty value state of the relatedEnd
                forceToTrue = relatedEnd.IsEmpty();
                EntityReference reference = relatedEnd as EntityReference;
                if (reference != null)
                {
                    forceToTrue &= reference.EntityKey == null;
                }
            }
            if (forceToTrue || this.MergeOption == MergeOption.OverwriteChanges)
            {
                relatedEnd.SetIsLoaded(true);
            }
        }

        /// <summary>
        /// REQUIRES:: entity is not null and MergeOption is OverwriteChanges or PreserveChanges
        /// Calls through to HandleEntity after retrieving the EntityKey from the given entity.
        /// Still need this so that the correct key will be used for iPOCOs that implement IEntityWithKey
        /// in a non-default manner.
        /// </summary>
        public IEntityWrapper HandleIEntityWithKey<TEntity>(IEntityWrapper wrappedEntity, EntitySet entitySet)
        {
            Debug.Assert(null != wrappedEntity, "wrapped entity is null");
            return HandleEntity<TEntity>(wrappedEntity, wrappedEntity.EntityKey, entitySet);
        }

        /// <summary>
        /// Calls through to the specified RecordState to set the value for the specified column ordinal.
        /// </summary>
        public bool SetColumnValue(int recordStateSlotNumber, int ordinal, object value)
        {
            RecordState recordState = (RecordState)this.State[recordStateSlotNumber];
            recordState.SetColumnValue(ordinal, value);
            return true;  // TRICKY: return true so we can use BitwiseOr expressions to string these guys together.
        }

        /// <summary>
        /// Calls through to the specified RecordState to set the value for the EntityRecordInfo.
        /// </summary>
        public bool SetEntityRecordInfo(int recordStateSlotNumber, EntityKey entityKey, EntitySet entitySet)
        {
            RecordState recordState = (RecordState)this.State[recordStateSlotNumber];
            recordState.SetEntityRecordInfo(entityKey, entitySet);
            return true;  // TRICKY: return true so we can use BitwiseOr expressions to string these guys together.
        }

        /// <summary>
        /// REQUIRES:: should be called only by delegate allocating this state.
        /// Utility method assigning a value to a state slot. Returns an arbitrary value
        /// allowing the method call to be composed in a ShapeEmitter Expression delegate.
        /// </summary>
        public bool SetState<T>(int ordinal, T value)
        {
            this.State[ordinal] = value;
            return true;  // TRICKY: return true so we can use BitwiseOr expressions to string these guys together.
        }

        /// <summary>
        /// REQUIRES:: should be called only by delegate allocating this state.
        /// Utility method assigning a value to a state slot and return the value, allowing
        /// the value to be accessed/set in a ShapeEmitter Expression delegate and later 
        /// retrieved.
        /// </summary>
        public T SetStatePassthrough<T>(int ordinal, T value)
        {
            this.State[ordinal] = value;
            return value;
        }

        /// <summary>
        /// Used to retrieve a property value with exception handling. Normally compiled
        /// delegates directly call typed methods on the DbDataReader (e.g. GetInt32)
        /// but when an exception occurs we retry using this method to potentially get
        /// a more useful error message to the user.
        /// </summary>
        public TProperty GetPropertyValueWithErrorHandling<TProperty>(int ordinal, string propertyName, string typeName)
        {
            TProperty result = new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName).GetValue(this.Reader, ordinal);
            return result;
        }

        /// <summary>
        /// Used to retrieve a column value with exception handling. Normally compiled
        /// delegates directly call typed methods on the DbDataReader (e.g. GetInt32)
        /// but when an exception occurs we retry using this method to potentially get
        /// a more useful error message to the user.
        /// </summary>
        public TColumn GetColumnValueWithErrorHandling<TColumn>(int ordinal)
        {
            TColumn result = new ColumnErrorHandlingValueReader<TColumn>().GetValue(this.Reader, ordinal);
            return result;
        }

        private DbSpatialDataReader CreateSpatialDataReader()
        {
            return SpatialHelpers.CreateSpatialDataReader(this.Workspace, this.Reader);
        }
        private readonly Singleton<DbSpatialDataReader> spatialReader;
                
        public DbGeography GetGeographyColumnValue(int ordinal)
        {
            return this.spatialReader.Value.GetGeography(ordinal);
        }
                
        public DbGeometry GetGeometryColumnValue(int ordinal)
        {
            return this.spatialReader.Value.GetGeometry(ordinal);
        }

        public TColumn GetSpatialColumnValueWithErrorHandling<TColumn>(int ordinal, PrimitiveTypeKind spatialTypeKind)
        {
            Debug.Assert(spatialTypeKind == PrimitiveTypeKind.Geography || spatialTypeKind == PrimitiveTypeKind.Geometry, "Spatial primitive type kind is not geography or geometry?");

            TColumn result;
            if (spatialTypeKind == PrimitiveTypeKind.Geography)
            {
                result = new ColumnErrorHandlingValueReader<TColumn>(
                                (reader, column) => (TColumn)(object)this.spatialReader.Value.GetGeography(column),
                                (reader, column) => this.spatialReader.Value.GetGeography(column)
                         ).GetValue(this.Reader, ordinal);
            }
            else
            {
                result = new ColumnErrorHandlingValueReader<TColumn>(
                                (reader, column) => (TColumn)(object)this.spatialReader.Value.GetGeometry(column),
                                (reader, column) => this.spatialReader.Value.GetGeometry(column)
                         ).GetValue(this.Reader, ordinal);
            }
            return result;
        }

        public TProperty GetSpatialPropertyValueWithErrorHandling<TProperty>(int ordinal, string propertyName, string typeName, PrimitiveTypeKind spatialTypeKind)
        {
            TProperty result;
            if (Helper.IsGeographicTypeKind(spatialTypeKind))
            {
                result = new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName,
                                (reader, column) => (TProperty)(object)this.spatialReader.Value.GetGeography(column),
                                (reader, column) => this.spatialReader.Value.GetGeography(column)
                         ).GetValue(this.Reader, ordinal);
            }
            else
            {
                Debug.Assert(Helper.IsGeometricTypeKind(spatialTypeKind));
                result = new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName,
                            (reader, column) => (TProperty)(object)this.spatialReader.Value.GetGeometry(column),
                            (reader, column) => this.spatialReader.Value.GetGeometry(column)
                     ).GetValue(this.Reader, ordinal);
            }

            return result;
        }

        #endregion

        #region helper methods (used by runtime callable code)

        private void CheckClearedEntryOnSpan(object targetValue, IEntityWrapper wrappedSource, EntityKey sourceKey, AssociationEndMember targetMember)
        {
            // If a relationship does not exist on the server but does exist on the client,
            // we may need to remove it, depending on the current state and the MergeOption
            if ((null != (object)sourceKey) && (null == targetValue) &&
                (this.MergeOption == MergeOption.PreserveChanges ||
                 this.MergeOption == MergeOption.OverwriteChanges))
            {
                // When the spanned value is null, it may be because the spanned association applies to a
                // subtype of the entity's type, and the entity is not actually an instance of that type.
                AssociationEndMember sourceEnd = MetadataHelper.GetOtherAssociationEnd(targetMember);
                EdmType expectedSourceType = ((RefType)sourceEnd.TypeUsage.EdmType).ElementType;
                TypeUsage entityTypeUsage;
                if (!this.Context.Perspective.TryGetType(wrappedSource.IdentityType, out entityTypeUsage) ||
                    entityTypeUsage.EdmType.EdmEquals(expectedSourceType) ||
                    TypeSemantics.IsSubTypeOf(entityTypeUsage.EdmType, expectedSourceType))
                {
                    // Otherwise, the source entity is the correct type (exactly or a subtype) for the source
                    // end of the spanned association, so validate that the relationhip that was spanned is
                    // part of the Container owning the EntitySet of the root entity.
                    // This can be done by comparing the EntitySet  of the row's entity to the relationships
                    // in the Container and their AssociationSetEnd's type
                    CheckClearedEntryOnSpan(sourceKey, wrappedSource, targetMember);
                }
            }
        }

        private void CheckClearedEntryOnSpan(EntityKey sourceKey, IEntityWrapper wrappedSource, AssociationEndMember targetMember)
        {
            Debug.Assert(null != (object)sourceKey);
            Debug.Assert(wrappedSource != null);
            Debug.Assert(wrappedSource.Entity != null);
            Debug.Assert(targetMember != null);
            Debug.Assert(this.Context != null);

            AssociationEndMember sourceMember = MetadataHelper.GetOtherAssociationEnd(targetMember);

            EntityContainer entityContainer = this.Context.MetadataWorkspace.GetEntityContainer(sourceKey.EntityContainerName,
                DataSpace.CSpace);
            EntitySet sourceEntitySet;
            AssociationSet associationSet = MetadataHelper.GetAssociationsForEntitySetAndAssociationType(entityContainer, sourceKey.EntitySetName,
                (AssociationType)sourceMember.DeclaringType, sourceMember.Name, out sourceEntitySet);

            if (associationSet != null)
            {
                Debug.Assert(associationSet.AssociationSetEnds[sourceMember.Name].EntitySet == sourceEntitySet);
                ObjectStateManager.RemoveRelationships(Context, MergeOption, associationSet, sourceKey, sourceMember);
            }
        }

        /// <summary>
        /// Wire's one or more full-spanned entities into the state manager; used by
        /// both full-spanned collections and full-spanned entities.
        /// </summary>
        private void FullSpanAction<T_TargetEntity>(IEntityWrapper wrappedSource, IList<T_TargetEntity> spannedEntities, AssociationEndMember targetMember)
        {
            Debug.Assert(null != wrappedSource, "wrapped entity is null");

            if (wrappedSource.Entity != null)
            {
                EntityKey sourceKey = wrappedSource.EntityKey;
                AssociationEndMember sourceMember = MetadataHelper.GetOtherAssociationEnd(targetMember);

                RelatedEnd relatedEnd;
                if (TryGetRelatedEnd(wrappedSource, (AssociationType)targetMember.DeclaringType, sourceMember.Name, targetMember.Name, out relatedEnd))
                {
                    // Add members of the list to the source entity (item in column 0)
                    int count = ObjectStateManager.UpdateRelationships(this.Context, this.MergeOption, (AssociationSet)relatedEnd.RelationshipSet, sourceMember, sourceKey, wrappedSource, targetMember, (List<T_TargetEntity>)spannedEntities, true);

                    SetIsLoadedForSpan(relatedEnd, count > 0);
                }
            }
        }

        #region update existing ObjectStateEntry

        private void UpdateEntry<TEntity>(IEntityWrapper wrappedEntity, EntityEntry existingEntry)
        {
            Debug.Assert(null != wrappedEntity, "wrapped entity is null");
            Debug.Assert(null != wrappedEntity.Entity, "null entity");
            Debug.Assert(null != existingEntry, "null ObjectStateEntry");
            Debug.Assert(null != existingEntry.Entity, "ObjectStateEntry without Entity");

            Type clrType = typeof(TEntity);
            if (clrType != existingEntry.WrappedEntity.IdentityType)
            {
                throw EntityUtil.RecyclingEntity(existingEntry.EntityKey, clrType, existingEntry.WrappedEntity.IdentityType);
            }

            if (EntityState.Added == existingEntry.State)
            {
                throw EntityUtil.AddedEntityAlreadyExists(existingEntry.EntityKey);
            }

            if (MergeOption.AppendOnly != MergeOption)
            {   // existing entity, update CSpace values in place
                Debug.Assert(EntityState.Added != existingEntry.State, "entry in State=Added");
                Debug.Assert(EntityState.Detached != existingEntry.State, "entry in State=Detached");

                if (MergeOption.OverwriteChanges == MergeOption)
                {
                    if (EntityState.Deleted == existingEntry.State)
                    {
                        existingEntry.RevertDelete();
                    }
                    existingEntry.UpdateCurrentValueRecord(wrappedEntity.Entity);
                    Context.ObjectStateManager.ForgetEntryWithConceptualNull(existingEntry, resetAllKeys: true);
                    existingEntry.AcceptChanges();
                    Context.ObjectStateManager.FixupReferencesByForeignKeys(existingEntry, replaceAddedRefs: true);
                }
                else
                {
                    Debug.Assert(MergeOption.PreserveChanges == MergeOption, "not MergeOption.PreserveChanges");
                    if (EntityState.Unchanged == existingEntry.State)
                    {
                        // same behavior as MergeOption.OverwriteChanges
                        existingEntry.UpdateCurrentValueRecord(wrappedEntity.Entity);
                        Context.ObjectStateManager.ForgetEntryWithConceptualNull(existingEntry, resetAllKeys: true);
                        existingEntry.AcceptChanges();
                        Context.ObjectStateManager.FixupReferencesByForeignKeys(existingEntry, replaceAddedRefs: true);
                    }
                    else
                    {
                        if (Context.ContextOptions.UseLegacyPreserveChangesBehavior)
                        {
                            // Do not mark properties as modified if they differ from the entity.
                            existingEntry.UpdateRecordWithoutSetModified(wrappedEntity.Entity, existingEntry.EditableOriginalValues);
                        }
                        else
                        {
                            // Mark properties as modified if they differ from the entity
                            existingEntry.UpdateRecordWithSetModified(wrappedEntity.Entity, existingEntry.EditableOriginalValues);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region nested types
        private abstract class ErrorHandlingValueReader<T>
        {
            private readonly Func<DbDataReader, int, T> getTypedValue;
            private readonly Func<DbDataReader, int, object> getUntypedValue;

            protected ErrorHandlingValueReader(Func<DbDataReader, int, T> typedValueAccessor, Func<DbDataReader, int, object> untypedValueAccessor)
            {
                this.getTypedValue = typedValueAccessor;
                this.getUntypedValue = untypedValueAccessor;
            }

            protected ErrorHandlingValueReader()
                : this(GetTypedValueDefault, GetUntypedValueDefault)
            {
            }

            private static T GetTypedValueDefault(DbDataReader reader, int ordinal)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));
                // The value read from the reader is of a primitive type. Such a value cannot be cast to a nullable enum type directly
                // but first needs to be cast to the non-nullable enum type. Therefore we will call this method for non-nullable
                // underlying enum type and cast to the target type. 
                if (underlyingType != null && underlyingType.IsEnum)
                {
                    var type = typeof(ErrorHandlingValueReader<>).MakeGenericType(underlyingType);
                    return (T)type.GetMethod(MethodBase.GetCurrentMethod().Name, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { reader, ordinal });
                }

                // use the specific reader.GetXXX method
                bool isNullable;
                MethodInfo readerMethod = Translator.GetReaderMethod(typeof(T), out isNullable);
                T result = (T)readerMethod.Invoke(reader, new object[] { ordinal });
                return result;
            }

            private static object GetUntypedValueDefault(DbDataReader reader, int ordinal)
            {
                return reader.GetValue(ordinal);
            }

            /// <summary>
            /// Gets value from reader using the same pattern as the materializer delegate. Avoids
            /// the need to compile multiple delegates for error handling. If there is a failure
            /// reading a value
            /// </summary>
            internal T GetValue(DbDataReader reader, int ordinal)
            {
                T result;
                if (reader.IsDBNull(ordinal))
                {
                    try
                    {
                        result = (T)(object)null;
                    }
                    catch (NullReferenceException)
                    {
                        // NullReferenceException is thrown when casting null to a value type.
                        // We don't use isNullable here because of an issue with GetReaderMethod
                        // 
                        throw CreateNullValueException();
                    }
                }
                else
                {
                    try
                    {
                        result = this.getTypedValue(reader, ordinal);
                    }
                    catch (Exception e)
                    {
                        if (EntityUtil.IsCatchableExceptionType(e))
                        {
                            // determine if the problem is with the result type
                            // (note that if we throw on this call, it's ok
                            // for it to percolate up -- we only intercept type
                            // and null mismatches)
                            object untypedResult = this.getUntypedValue(reader, ordinal);
                            Type resultType = null == untypedResult ? null : untypedResult.GetType();
                            if (!typeof(T).IsAssignableFrom(resultType))
                            {
                                throw CreateWrongTypeException(resultType);
                            }
                        }
                        throw;
                    }
                }
                return result;
            }

            /// <summary>
            /// Creates the exception thrown when the reader returns a null value
            /// for a non nullable property/column.
            /// </summary>
            protected abstract Exception CreateNullValueException();

            /// <summary>
            /// Creates the exception thrown when the reader returns a value with
            /// an incompatible type.
            /// </summary>
            protected abstract Exception CreateWrongTypeException(Type resultType);
        }

        private class ColumnErrorHandlingValueReader<TColumn> : ErrorHandlingValueReader<TColumn>
        {
            internal ColumnErrorHandlingValueReader()
            {
            }

            internal ColumnErrorHandlingValueReader(Func<DbDataReader, int, TColumn> typedAccessor, Func<DbDataReader, int, object> untypedAccessor)
                : base(typedAccessor, untypedAccessor)
            {
            }

            protected override Exception CreateNullValueException()
            {
                return EntityUtil.ValueNullReferenceCast(typeof(TColumn));
            }

            protected override Exception  CreateWrongTypeException(Type resultType)
            {
                return EntityUtil.ValueInvalidCast(resultType, typeof(TColumn));
            }
        }

        private class PropertyErrorHandlingValueReader<TProperty> : ErrorHandlingValueReader<TProperty>
        {
            private readonly string _propertyName;
            private readonly string _typeName;

            internal PropertyErrorHandlingValueReader(string propertyName, string typeName)
                : base()
            {
                _propertyName = propertyName;
                _typeName = typeName;
            }

            internal PropertyErrorHandlingValueReader(string propertyName, string typeName, Func<DbDataReader, int, TProperty> typedAccessor, Func<DbDataReader, int, object> untypedAccessor)
                : base(typedAccessor, untypedAccessor)
            {
                _propertyName = propertyName;
                _typeName = typeName;
            }

            protected override Exception CreateNullValueException()
            {
                return EntityUtil.Constraint(
                                        System.Data.Entity.Strings.Materializer_SetInvalidValue(
                                        (Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty)).Name,
                                        _typeName, _propertyName, "null"));
            }

            protected override Exception CreateWrongTypeException(Type resultType)
            {
                return EntityUtil.InvalidOperation(
                                        System.Data.Entity.Strings.Materializer_SetInvalidValue(
                                        (Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty)).Name,
                                        _typeName, _propertyName, resultType.Name));
            }
        }
        #endregion

        #region OnMaterialized helpers

        public void RaiseMaterializedEvents()
        {
            if (_materializedEntities != null)
            {
                foreach (var wrappedEntity in _materializedEntities)
                {
                    Context.OnObjectMaterialized(wrappedEntity.Entity);
                }
                _materializedEntities.Clear();
            }
        }

        public void InitializeForOnMaterialize()
        {
            if (Context.OnMaterializedHasHandlers)
            {
                if (_materializedEntities == null)
                {
                    _materializedEntities = new List<IEntityWrapper>();
                }
            }
            else if (_materializedEntities != null)
            {
                _materializedEntities = null;
            }
        }

        protected void RegisterMaterializedEntityForEvent(IEntityWrapper wrappedEntity)
        {
            if (_materializedEntities != null)
            {
                _materializedEntities.Add(wrappedEntity);
            }
        }

        #endregion
    }

    /// <summary>
    /// Typed Shaper. Includes logic to enumerate results and wraps the _rootCoordinator,
    /// which includes materializer delegates for the root query collection.
    /// </summary>
    internal sealed class Shaper<T> : Shaper
    {
        #region private state

        /// <summary>
        /// Shapers and Coordinators work together in harmony to materialize the data
        /// from the store; the shaper contains the state, the coordinator contains the
        /// code.
        /// </summary>
        internal readonly Coordinator<T> RootCoordinator;

        /// <summary>
        /// Which type of query is this, object layer (true) or value layer (false)
        /// </summary>
        private readonly bool IsObjectQuery;

        /// <summary>
        /// Keeps track of whether we've completed processing or not.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// The enumerator we're using to read data; really only populated for value
        /// layer queries.
        /// </summary>
        private IEnumerator<T> _rootEnumerator;

        /// <summary>
        /// Whether the current value of _rootEnumerator has been returned by a bridge
        /// data reader.
        /// </summary>
        private bool _dataWaiting;

        /// <summary>
        /// Is the reader owned by the EF or was it supplied by the user?
        /// </summary>
        private bool _readerOwned;

        #endregion

        #region constructor

        internal Shaper(DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption, int stateCount, CoordinatorFactory<T> rootCoordinatorFactory, Action checkPermissions, bool readerOwned)
            : base(reader, context, workspace, mergeOption, stateCount)
        {
            RootCoordinator = new Coordinator<T>(rootCoordinatorFactory, /*parent*/ null, /*next*/ null);
            if (null != checkPermissions)
            {
                checkPermissions();
            }
            IsObjectQuery = !(typeof(T) == typeof(RecordState));
            _isActive = true;
            RootCoordinator.Initialize(this);
            _readerOwned = readerOwned;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// Events raised when the shaper has finished enumerating results. Useful for callback 
        /// to set parameter values.
        /// </summary>
        internal event EventHandler OnDone;

        /// <summary>
        /// Used to handle the read-ahead requirements of value-layer queries.  This
        /// field indicates the status of the current value of the _rootEnumerator; when
        /// a bridge data reader "accepts responsibility" for the current value, it sets
        /// this to false.
        /// </summary>
        internal bool DataWaiting
        {
            get { return _dataWaiting; }
            set { _dataWaiting = value; }
        }

        /// <summary>
        /// The enumerator that the value-layer bridge will use to read data; all nested
        /// data readers need to use the same enumerator, so we put it on the Shaper, since
        /// that is something that all the nested data readers (and data records) have access
        /// to -- it prevents us from having to pass two objects around.
        /// </summary>
        internal IEnumerator<T> RootEnumerator
        {
            get
            {
                if (_rootEnumerator == null)
                {
                    InitializeRecordStates(RootCoordinator.CoordinatorFactory);
                    _rootEnumerator = GetEnumerator();
                }
                return _rootEnumerator;
            }
        }

        /// <summary>
        /// Initialize the RecordStateFactory objects in their StateSlots.
        /// </summary>
        private void InitializeRecordStates(CoordinatorFactory coordinatorFactory)
        {
            foreach (RecordStateFactory recordStateFactory in coordinatorFactory.RecordStateFactories)
            {
                State[recordStateFactory.StateSlotNumber] = recordStateFactory.Create(coordinatorFactory);
            }

            foreach (CoordinatorFactory nestedCoordinatorFactory in coordinatorFactory.NestedCoordinators)
            {
                InitializeRecordStates(nestedCoordinatorFactory);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            // we can use a simple enumerator if there are no nested results, no keys and no "has data"
            // discriminator
            if (RootCoordinator.CoordinatorFactory.IsSimple)
            {
                return new SimpleEnumerator(this);
            }
            else
            {
                RowNestedResultEnumerator rowEnumerator = new Shaper<T>.RowNestedResultEnumerator(this);

                if (this.IsObjectQuery)
                {
                    return new ObjectQueryNestedEnumerator(rowEnumerator);
                }
                else
                {
                    return (IEnumerator<T>)(object)(new RecordStateEnumerator(rowEnumerator));
                }
            }
        }

        #endregion

        #region enumerator helpers

        /// <summary>
        /// Called when enumeration of results has completed.
        /// </summary>
        private void Finally()
        {
            if (_isActive)
            {
                _isActive = false;

                if (_readerOwned)
                {
                    // I'd prefer not to special case this, but value-layer behavior is that you
                    // must explicitly close the data reader; if we automatically dispose of the
                    // reader here, we won't have that behavior.
                    if (IsObjectQuery)
                    {
                        this.Reader.Dispose();
                    }

                    // This case includes when the ObjectResult is disposed before it 
                    // created an ObjectQueryEnumeration; at this time, the connection can be released
                    if (this.Context != null)
                    {
                        this.Context.ReleaseConnection();
                    }
                }

                if (null != this.OnDone)
                {
                    this.OnDone(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Reads the next row from the store. If there is a failure, throws an exception message
        /// in some scenarios (note that we respond to failure rather than anticipate failure,
        /// avoiding repeated checks in the inner materialization loop)
        /// </summary>
        private bool StoreRead()
        {
            bool readSucceeded;
            try
            {
                readSucceeded = this.Reader.Read();
            }
            catch (Exception e)
            {
                // check if the reader is closed; if so, throw friendlier exception
                if (this.Reader.IsClosed)
                {
                    const string operation = "Read";
                    throw EntityUtil.DataReaderClosed(operation);
                }

                // wrap exception if necessary
                if (EntityUtil.IsCatchableEntityExceptionType(e))
                {
                    throw EntityUtil.CommandExecution(System.Data.Entity.Strings.EntityClient_StoreReaderFailed, e);
                }
                throw;
            }
            return readSucceeded;
        }

        /// <summary>
        /// Notify ObjectContext that we are about to start materializing an element
        /// </summary>
        private void StartMaterializingElement()
        {
            if (Context != null)
            {
                Context.InMaterialization = true;
                InitializeForOnMaterialize();
            }
        }

        /// <summary>
        /// Notify ObjectContext that we are finished materializing the element
        /// </summary>        
        private void StopMaterializingElement()
        {
            if (Context != null)
            {
                Context.InMaterialization = false;
                RaiseMaterializedEvents();
            }
        }

        #endregion

        #region simple enumerator

        /// <summary>
        /// Optimized enumerator for queries not including nested results.
        /// </summary>
        private class SimpleEnumerator : IEnumerator<T>
        {
            private readonly Shaper<T> _shaper;

            internal SimpleEnumerator(Shaper<T> shaper)
            {
                _shaper = shaper;
            }

            public T Current
            {
                get { return _shaper.RootCoordinator.Current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _shaper.RootCoordinator.Current; }
            }

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                // For backwards compatibility, we set the current value to the
                // default value, so you can still call Current.
                _shaper.RootCoordinator.SetCurrentToDefault();
                _shaper.Finally();
            }

            public bool MoveNext()
            {
                if (!_shaper._isActive)
                {
                    return false;
                }
                if (_shaper.StoreRead())
                {
                    try
                    {
                        _shaper.StartMaterializingElement();
                        _shaper.RootCoordinator.ReadNextElement(_shaper);
                    }
                    finally
                    {
                        _shaper.StopMaterializingElement();
                    }                    
                    return true;
                }
                this.Dispose();
                return false;
            }

            public void Reset()
            {
                throw EntityUtil.NotSupported();
            }
        }

        #endregion

        #region nested enumerator

        /// <summary>
        /// Enumerates (for each row in the input) an array of all coordinators producing new elements. The array
        /// contains a position for each 'depth' in the result. A null value in any position indicates that no new
        /// results were produced for the given row at the given depth. It is possible for a row to contain no
        /// results for any row.
        /// </summary>
        private class RowNestedResultEnumerator : IEnumerator<Coordinator[]>
        {
            private readonly Shaper<T> _shaper;
            private readonly Coordinator[] _current;

            internal RowNestedResultEnumerator(Shaper<T> shaper)
            {
                _shaper = shaper;
                _current = new Coordinator[_shaper.RootCoordinator.MaxDistanceToLeaf() + 1];
            }

            public Coordinator[] Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                _shaper.Finally();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                Coordinator currentCoordinator = _shaper.RootCoordinator;

                try
                {
                    _shaper.StartMaterializingElement();
                    
                    if (!_shaper.StoreRead())
                    {
                        // Reset all collections
                        this.RootCoordinator.ResetCollection(_shaper);
                        return false;
                    }

                    int depth = 0;
                    bool haveInitializedChildren = false;
                    for (; depth < _current.Length; depth++)
                    {
                        // find a coordinator at this depth that currently has data (if any)
                        while (currentCoordinator != null && !currentCoordinator.CoordinatorFactory.HasData(_shaper))
                        {
                            currentCoordinator = currentCoordinator.Next;
                        }
                        if (null == currentCoordinator)
                        {
                            break;
                        }

                        // check if this row contains a new element for this coordinator
                        if (currentCoordinator.HasNextElement(_shaper))
                        {
                            // if we have children and haven't initialized them yet, do so now
                            if (!haveInitializedChildren && null != currentCoordinator.Child)
                            {
                                currentCoordinator.Child.ResetCollection(_shaper);
                            }
                            haveInitializedChildren = true;

                            // read the next element
                            currentCoordinator.ReadNextElement(_shaper);

                            // place the coordinator in the result array to indicate there is a new
                            // element at this depth
                            _current[depth] = currentCoordinator;
                        }
                        else
                        {
                            // clear out the coordinator in result array to indicate there is no new
                            // element at this depth
                            _current[depth] = null;
                        }

                        // move to child (in the next iteration we deal with depth + 1
                        currentCoordinator = currentCoordinator.Child;
                    }

                    // clear out all positions below the depth we reached before we ran out of data
                    for (; depth < _current.Length; depth++)
                    {
                        _current[depth] = null;
                    }
                }
                finally
                {
                    _shaper.StopMaterializingElement();
                }

                return true;
            }

            public void Reset()
            {
                throw EntityUtil.NotSupported();
            }

            internal Coordinator<T> RootCoordinator
            {
                get { return _shaper.RootCoordinator; }
            }
        }

        /// <summary>
        /// Wraps RowNestedResultEnumerator and yields results appropriate to an ObjectQuery instance. In particular,
        /// root level elements (T) are returned only after aggregating all child elements.
        /// </summary>
        private class ObjectQueryNestedEnumerator : IEnumerator<T>
        {
            private readonly RowNestedResultEnumerator _rowEnumerator;
            private T _previousElement;
            private State _state;

            internal ObjectQueryNestedEnumerator(RowNestedResultEnumerator rowEnumerator)
            {
                _rowEnumerator = rowEnumerator;
                _previousElement = default(T);
                _state = State.Start;
            }

            public T Current { get { return _previousElement; } }

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                _rowEnumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current { get { return this.Current; } }

            public bool MoveNext()
            {
                // See the documentation for enum State to understand the behaviors and requirements
                // for each state.
                switch (_state)
                {
                    case State.Start:
                        {
                            if (TryReadToNextElement())
                            {
                                // if there's an element in the reader...
                                ReadElement();
                            }
                            else
                            {
                                // no data at all...
                                _state = State.NoRows;
                            }
                        };
                        break;
                    case State.Reading:
                        {
                            ReadElement();
                        };
                        break;
                    case State.NoRowsLastElementPending:
                        {
                            // nothing to do but move to the next state...
                            _state = State.NoRows;
                        };
                        break;
                }

                bool result;
                if (_state == State.NoRows)
                {
                    _previousElement = default(T);
                    result = false;
                }
                else
                {
                    result = true;
                }

                return result;
            }

            /// <summary>
            /// Requires: the row is currently positioned at the start of an element.
            /// 
            /// Reads all rows in the element and sets up state for the next element (if any).
            /// </summary>
            private void ReadElement()
            {
                // remember the element we're currently reading
                _previousElement = _rowEnumerator.RootCoordinator.Current;

                // now we need to read to the next element (or the end of the
                // reader) so that we can return the first element
                if (TryReadToNextElement())
                {
                    // we're positioned at the start of the next element (which
                    // corresponds to the 'reading' state)
                    _state = State.Reading;
                }
                else
                {
                    // we're positioned at the end of the reader
                    _state = State.NoRowsLastElementPending;
                }
            }

            /// <summary>
            /// Reads rows until the start of a new element is found. If no element
            /// is found before all rows are consumed, returns false.
            /// </summary>
            private bool TryReadToNextElement()
            {
                while (_rowEnumerator.MoveNext())
                {
                    // if we hit a new element, return true
                    if (_rowEnumerator.Current[0] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                _rowEnumerator.Reset();
            }

            /// <summary>
            /// Describes the state of this enumerator with respect to the _rowEnumerator
            /// it wraps.
            /// </summary>
            private enum State
            {
                /// <summary>
                /// No rows have been read yet
                /// </summary>
                Start,

                /// <summary>
                /// Positioned at the start of a new root element. The previous element must
                /// be stored in _previousElement. We read ahead in this manner so that
                /// the previous element is fully populated (all of its children loaded)
                /// before returning.
                /// </summary>
                Reading,

                /// <summary>
                /// Positioned past the end of the rows. The last element in the enumeration
                /// has not yet been returned to the user however, and is stored in _previousElement.
                /// </summary>
                NoRowsLastElementPending,

                /// <summary>
                /// Positioned past the end of the rows. The last element has been returned to
                /// the user.
                /// </summary>
                NoRows,
            }
        }

        /// <summary>
        /// Wraps RowNestedResultEnumerator and yields results appropriate to an EntityReader instance. In particular,
        /// yields RecordState whenever a new element becomes available at any depth in the result hierarchy.
        /// </summary>
        private class RecordStateEnumerator : IEnumerator<RecordState>
        {
            private readonly RowNestedResultEnumerator _rowEnumerator;
            private RecordState _current;

            /// <summary>
            /// Gets depth of coordinator we're currently consuming. If _depth == -1, it means we haven't started
            /// to consume the next row yet.
            /// </summary>
            private int _depth;
            private bool _readerConsumed;

            internal RecordStateEnumerator(RowNestedResultEnumerator rowEnumerator)
            {
                _rowEnumerator = rowEnumerator;
                _current = null;
                _depth = -1;
                _readerConsumed = false;
            }

            public RecordState Current
            {
                get { return _current; }
            }

            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                _rowEnumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                if (!_readerConsumed)
                {
                    while (true)
                    {
                        // keep on cycling until we find a result
                        if (-1 == _depth || _rowEnumerator.Current.Length == _depth)
                        {
                            // time to move to the next row...
                            if (!_rowEnumerator.MoveNext())
                            {
                                // no more rows...
                                _current = null;
                                _readerConsumed = true;
                                break;
                            }

                            _depth = 0;
                        }

                        // check for results at the current depth
                        Coordinator currentCoordinator = _rowEnumerator.Current[_depth];
                        if (null != currentCoordinator)
                        {
                            _current = ((Coordinator<RecordState>)currentCoordinator).Current;
                            _depth++;
                            break;
                        }

                        _depth++;
                    }
                }

                return !_readerConsumed;
            }

            public void Reset()
            {
                _rowEnumerator.Reset();
            }
        }

        #endregion

    }
}
