//---------------------------------------------------------------------
// <copyright file="ViewLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common.CommandTrees.Internal;
using System.Data.Common.Utils;
using System.Diagnostics;
using System.Data.Common;
using System.Data.Objects;
using System.Linq;
using System.Threading;
namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// Retrieves update mapping views and dependency information for update mapping views. Acts as a wrapper around
    /// the metadata workspace (and allows direct definition of update mapping views for test purposes).
    /// </summary>
    internal class ViewLoader
    {
        #region Constructors 
        /// <summary>
        /// Constructor specifying a metadata workspace to use for mapping views.
        /// </summary>
        internal ViewLoader(StorageMappingItemCollection mappingCollection)
        {
            Debug.Assert(null != mappingCollection, "mapping collection required");
            m_mappingCollection = mappingCollection;
        }
        #endregion

        #region Fields
        private readonly StorageMappingItemCollection m_mappingCollection;
        private readonly Dictionary<AssociationSet, AssociationSetMetadata> m_associationSetMetadata = new Dictionary<AssociationSet, AssociationSetMetadata>();
        private readonly Dictionary<EntitySetBase, Set<EntitySet>> m_affectedTables = new Dictionary<EntitySetBase, Set<EntitySet>>();
        private readonly Set<EdmMember> m_serverGenProperties = new Set<EdmMember>();
        private readonly Set<EdmMember> m_isNullConditionProperties = new Set<EdmMember>();
        private readonly Dictionary<EntitySetBase, ModificationFunctionMappingTranslator> m_functionMappingTranslators = new Dictionary<EntitySetBase, ModificationFunctionMappingTranslator>(
            EqualityComparer<EntitySetBase>.Default);

        private readonly ReaderWriterLockSlim m_readerWriterLock = new ReaderWriterLockSlim();
        #endregion

        #region Methods
        /// <summary>
        /// For a given extent, returns the function mapping translator.
        /// </summary>
        /// <param name="extent">Association set or entity set for which to retrieve a translator</param>
        /// <returns>Function translator or null if none exists for this extent</returns>
        internal ModificationFunctionMappingTranslator GetFunctionMappingTranslator(EntitySetBase extent, MetadataWorkspace workspace)
        {
            return SyncGetValue(extent, workspace, m_functionMappingTranslators, extent);
        }

        /// <summary>
        /// Returns store tables affected by modifications to a particular C-layer extent. Although this
        /// information can be inferred from the update view, we want to avoid compiling or loading 
        /// views when not required. This information can be directly determined from mapping metadata.
        /// </summary>
        /// <param name="extent">C-layer extent.</param>
        /// <returns>Affected store tables.</returns>
        internal Set<EntitySet> GetAffectedTables(EntitySetBase extent, MetadataWorkspace workspace)
        {
            return SyncGetValue(extent, workspace, m_affectedTables, extent);
        }

        /// <summary>
        /// Gets information relevant to the processing of an AssociationSet in the update pipeline.
        /// Caches information on first retrieval.
        /// </summary>
        internal AssociationSetMetadata GetAssociationSetMetadata(AssociationSet associationSet, MetadataWorkspace workspace)
        {
            return SyncGetValue(associationSet, workspace, m_associationSetMetadata, associationSet);
        }

        /// <summary>
        /// Determines whether the given member maps to a server-generated column in the store.
        /// Requires: InitializeExtentInformation has been called for the extent being persisted.
        /// </summary>
        /// <param name="entitySetBase">Entity set containing member.</param>
        /// <param name="member">Member to lookup</param>
        /// <returns>Whether the member is server generated in some context</returns>
        internal bool IsServerGen(EntitySetBase entitySetBase, MetadataWorkspace workspace, EdmMember member)
        {
            return SyncContains(entitySetBase, workspace, m_serverGenProperties, member);
        }

        /// <summary>
        /// Determines whether the given member maps to a column participating in an isnull
        /// condition. Useful to determine if a nullability constraint violation is going to
        /// cause roundtripping problems (e.g. if type is based on nullability of a 'non-nullable'
        /// property of a derived entity type)
        /// </summary>
        internal bool IsNullConditionMember(EntitySetBase entitySetBase, MetadataWorkspace workspace, EdmMember member)
        {
            return SyncContains(entitySetBase, workspace, m_isNullConditionProperties, member);
        }

        /// <summary>
        /// Utility method reading value from dictionary within read lock.
        /// </summary>
        private T_Value SyncGetValue<T_Key, T_Value>(EntitySetBase entitySetBase, MetadataWorkspace workspace, Dictionary<T_Key, T_Value> dictionary, T_Key key)
        {
            return SyncInitializeEntitySet(entitySetBase, workspace, k => dictionary[k], key);
        }

        /// <summary>
        /// Utility method checking for membership of element in set within read lock.
        /// </summary>
        private bool SyncContains<T_Element>(EntitySetBase entitySetBase, MetadataWorkspace workspace, Set<T_Element> set, T_Element element)
        {
            return SyncInitializeEntitySet(entitySetBase, workspace, set.Contains, element);
        }

        /// <summary>
        /// Initializes all information relevant to the entity set.
        /// </summary>
        /// <param name="entitySetBase">Association set or entity set to load.</param>
        /// <param name="evaluate">Function to evaluate to produce a result.</param>
        private TResult SyncInitializeEntitySet<TArg, TResult>(EntitySetBase entitySetBase, MetadataWorkspace workspace, Func<TArg, TResult> evaluate, TArg arg)
        {
            m_readerWriterLock.EnterReadLock();
            try
            {
                // check if we've already done the work for this entity set
                if (m_affectedTables.ContainsKey(entitySetBase))
                {
                    return evaluate(arg);
                }
            }
            finally
            {
                m_readerWriterLock.ExitReadLock();
            }

            // acquire a write lock
            m_readerWriterLock.EnterWriteLock();
            try
            {
                // see if we've since done the work for this entity set
                if (m_affectedTables.ContainsKey(entitySetBase))
                {
                    return evaluate(arg);
                }

                InitializeEntitySet(entitySetBase, workspace);
                return evaluate(arg);
            }
            finally
            {
                m_readerWriterLock.ExitWriteLock();
            }
        }

        private void InitializeEntitySet(EntitySetBase entitySetBase, MetadataWorkspace workspace)
        {
            StorageEntityContainerMapping mapping = (StorageEntityContainerMapping)m_mappingCollection.GetMap(entitySetBase.EntityContainer);

            // make sure views have been generated for this sub-graph (trigger generation of the sub-graph
            // by retrieving a view for one of its components; not actually using the view here)
            if (mapping.HasViews)
            {
                m_mappingCollection.GetGeneratedView(entitySetBase, workspace);
            }

            Set<EntitySet> affectedTables = new Set<EntitySet>();


            if (null != mapping)
            {
                Set<EdmMember> isNullConditionColumns = new Set<EdmMember>();

                // find extent in the container mapping
                StorageSetMapping setMapping;
                if (entitySetBase.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
                {
                    setMapping = mapping.GetEntitySetMapping(entitySetBase.Name);

                    // Check for members that have result bindings in a function mapping. If a 
                    // function returns the member values, it indicates they are server-generated
                    m_serverGenProperties.Unite(GetMembersWithResultBinding((StorageEntitySetMapping)setMapping));
                }
                else if (entitySetBase.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
                {
                    setMapping = mapping.GetRelationshipSetMapping(entitySetBase.Name);
                }
                else
                {
                    Debug.Fail("unexpected extent type " + entitySetBase.BuiltInTypeKind);
                    throw EntityUtil.NotSupported();
                }

                // gather interesting tables, columns and properties from mapping fragments
                foreach (StorageMappingFragment mappingFragment in GetMappingFragments(setMapping))
                {
                    affectedTables.Add(mappingFragment.TableSet);

                    // get all property mappings to figure out if anything is server generated
                    m_serverGenProperties.AddRange(FindServerGenMembers(mappingFragment));

                    // get all columns participating in is null conditions
                    isNullConditionColumns.AddRange(FindIsNullConditionColumns(mappingFragment));
                }

                if (0 < isNullConditionColumns.Count)
                {
                    // gather is null condition properties based on is null condition columns
                    foreach (StorageMappingFragment mappingFragment in GetMappingFragments(setMapping))
                    {
                        m_isNullConditionProperties.AddRange(FindPropertiesMappedToColumns(isNullConditionColumns, mappingFragment));
                    }
                }
            }

            m_affectedTables.Add(entitySetBase, affectedTables.MakeReadOnly());

            InitializeFunctionMappingTranslators(entitySetBase, mapping);

            // for association sets, initialize AssociationSetMetadata if no function has claimed ownership
            // of the association yet
            if (entitySetBase.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
            {
                AssociationSet associationSet = (AssociationSet)entitySetBase;
                if (!m_associationSetMetadata.ContainsKey(associationSet))
                {
                    m_associationSetMetadata.Add(associationSet, new AssociationSetMetadata(
                        m_affectedTables[associationSet], associationSet, workspace));
                }
            }
        }

        /// <summary>
        /// Yields all members appearing in function mapping result bindings.
        /// </summary>
        /// <param name="entitySetMapping">Set mapping to examine</param>
        /// <returns>All result bindings</returns>
        private IEnumerable<EdmMember> GetMembersWithResultBinding(StorageEntitySetMapping entitySetMapping)
        {
            foreach (StorageEntityTypeModificationFunctionMapping typeFunctionMapping in entitySetMapping.ModificationFunctionMappings)
            {
                // look at all result bindings for insert and update commands
                if (null != typeFunctionMapping.InsertFunctionMapping && null != typeFunctionMapping.InsertFunctionMapping.ResultBindings)
                {
                    foreach (StorageModificationFunctionResultBinding binding in typeFunctionMapping.InsertFunctionMapping.ResultBindings)
                    {
                        yield return binding.Property;
                    }
                }
                if (null != typeFunctionMapping.UpdateFunctionMapping && null != typeFunctionMapping.UpdateFunctionMapping.ResultBindings)
                {
                    foreach (StorageModificationFunctionResultBinding binding in typeFunctionMapping.UpdateFunctionMapping.ResultBindings)
                    {
                        yield return binding.Property;
                    }
                }
            }
        }

        // Loads and registers any function mapping translators for the given extent (and related container)
        private void InitializeFunctionMappingTranslators(EntitySetBase entitySetBase, StorageEntityContainerMapping mapping)
        {
            KeyToListMap<AssociationSet, AssociationEndMember> requiredEnds = new KeyToListMap<AssociationSet, AssociationEndMember>(
                EqualityComparer<AssociationSet>.Default);

            // see if function mapping metadata needs to be processed
            if (!m_functionMappingTranslators.ContainsKey(entitySetBase))
            {
                // load all function mapping data from the current entity container
                foreach (StorageEntitySetMapping entitySetMapping in mapping.EntitySetMaps)
                {
                    if (0 < entitySetMapping.ModificationFunctionMappings.Count)
                    {
                        // register the function mapping
                        m_functionMappingTranslators.Add(entitySetMapping.Set, ModificationFunctionMappingTranslator.CreateEntitySetTranslator(entitySetMapping));

                        // register "null" function translators for all implicitly mapped association sets
                        foreach (AssociationSetEnd end in entitySetMapping.ImplicitlyMappedAssociationSetEnds)
                        {
                            AssociationSet associationSet = end.ParentAssociationSet;
                            if (!m_functionMappingTranslators.ContainsKey(associationSet))
                            {
                                m_functionMappingTranslators.Add(associationSet, ModificationFunctionMappingTranslator.CreateAssociationSetTranslator(null));
                            }

                            // Remember that the current entity set is required for all updates to the collocated
                            // relationship set. This entity set's end is opposite the target end for the mapping.
                            AssociationSetEnd oppositeEnd = MetadataHelper.GetOppositeEnd(end);
                            requiredEnds.Add(associationSet, oppositeEnd.CorrespondingAssociationEndMember);
                        }
                    }
                    else
                    {
                        // register null translator (so that we never attempt to process this extent again)
                        m_functionMappingTranslators.Add(entitySetMapping.Set, null);
                    }
                }

                foreach (StorageAssociationSetMapping associationSetMapping in mapping.RelationshipSetMaps)
                {
                    if (null != associationSetMapping.ModificationFunctionMapping)
                    {
                        AssociationSet set = (AssociationSet)associationSetMapping.Set;

                        // use indexer rather than Add since the association set may already have an implicit function
                        // mapping -- this explicit function mapping takes precedence in such cases
                        m_functionMappingTranslators.Add(set, 
                            ModificationFunctionMappingTranslator.CreateAssociationSetTranslator(associationSetMapping));

                        // remember that we've seen a function mapping for this association set, which overrides
                        // any other behaviors for determining required/optional ends
                        requiredEnds.AddRange(set, Enumerable.Empty<AssociationEndMember>());
                    }
                    else
                    {
                        if (!m_functionMappingTranslators.ContainsKey(associationSetMapping.Set))
                        {
                            // register null translator (so that we never attempt to process this extent again)
                            m_functionMappingTranslators.Add(associationSetMapping.Set, null);
                        }
                    }
                }
            }

            // register association metadata for all association sets encountered
            foreach (AssociationSet associationSet in requiredEnds.Keys)
            {
                m_associationSetMetadata.Add(associationSet, new AssociationSetMetadata(
                    requiredEnds.EnumerateValues(associationSet)));
            }
        }

        /// <summary>
        /// Gets all model properties mapped to server generated columns.
        /// </summary>
        private static IEnumerable<EdmMember> FindServerGenMembers(StorageMappingFragment mappingFragment)
        {
            foreach (var scalarPropertyMapping in FlattenPropertyMappings(mappingFragment.AllProperties)
                .OfType<StorageScalarPropertyMapping>())
            {
                if (StoreGeneratedPattern.None != MetadataHelper.GetStoreGeneratedPattern(scalarPropertyMapping.ColumnProperty))
                {
                    yield return scalarPropertyMapping.EdmProperty;
                }
            }
        }
        
        /// <summary>
        /// Gets all store columns participating in is null conditions.
        /// </summary>
        private static IEnumerable<EdmMember> FindIsNullConditionColumns(StorageMappingFragment mappingFragment)
        {
            foreach (var conditionPropertyMapping in FlattenPropertyMappings(mappingFragment.AllProperties)
                .OfType<StorageConditionPropertyMapping>())
            {
                if (conditionPropertyMapping.ColumnProperty != null &&
                    conditionPropertyMapping.IsNull.HasValue)
                {
                    yield return conditionPropertyMapping.ColumnProperty;
                }
            }
        }

        /// <summary>
        /// Gets all model properties mapped to given columns.
        /// </summary>
        private static IEnumerable<EdmMember> FindPropertiesMappedToColumns(Set<EdmMember> columns, StorageMappingFragment mappingFragment)
        {
            foreach (var scalarPropertyMapping in FlattenPropertyMappings(mappingFragment.AllProperties)
                .OfType<StorageScalarPropertyMapping>())
            {
                if (columns.Contains(scalarPropertyMapping.ColumnProperty))
                {
                    yield return scalarPropertyMapping.EdmProperty;
                }
            }
        }

        /// <summary>
        /// Enumerates all mapping fragments in given set mapping.
        /// </summary>
        private static IEnumerable<StorageMappingFragment> GetMappingFragments(StorageSetMapping setMapping)
        {
            // get all type mappings for the extent
            foreach (StorageTypeMapping typeMapping in setMapping.TypeMappings)
            {
                // get all table mapping fragments for the type
                foreach (StorageMappingFragment mappingFragment in typeMapping.MappingFragments)
                {
                    yield return mappingFragment;
                }
            }
        }

        /// <summary>
        /// Returns all bottom-level mappings (e.g. conditions and scalar property mappings but not complex property mappings
        /// whose components are returned)
        /// </summary>
        private static IEnumerable<StoragePropertyMapping> FlattenPropertyMappings(System.Collections.ObjectModel.ReadOnlyCollection<StoragePropertyMapping> propertyMappings)
        {
            foreach (StoragePropertyMapping propertyMapping in propertyMappings)
            {
                StorageComplexPropertyMapping complexPropertyMapping = propertyMapping as StorageComplexPropertyMapping;
                if (null != complexPropertyMapping)
                {
                    foreach (StorageComplexTypeMapping complexTypeMapping in complexPropertyMapping.TypeMappings)
                    {
                        // recursively call self with nested type
                        foreach (StoragePropertyMapping nestedPropertyMapping in FlattenPropertyMappings(complexTypeMapping.AllProperties))
                        {
                            yield return nestedPropertyMapping;
                        }
                    }
                }
                else
                {
                    yield return propertyMapping;
                }
            }
        }
        #endregion
    }
}
