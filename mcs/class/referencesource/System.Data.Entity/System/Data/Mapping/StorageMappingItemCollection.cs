//---------------------------------------------------------------------
// <copyright file="StorageMappingItemCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping.Update.Internal;
using System.Data.Mapping.ViewGeneration;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml;
using som = System.Data.EntityModel.SchemaObjectModel;

namespace System.Data.Mapping
{
    using OfTypeQVCacheKey = Pair<EntitySetBase, Pair<EntityTypeBase, bool>>;

    /// <summary>
    /// Class for representing a collection of items in Storage Mapping( CS Mapping) space.
    /// </summary>
    [CLSCompliant(false)]
    public partial class StorageMappingItemCollection : MappingItemCollection
    {
        #region Fields
        //EdmItemCollection that is associated with the MSL Loader.
        private EdmItemCollection m_edmCollection;

        //StoreItemCollection that is associated with the MSL Loader.
        private StoreItemCollection m_storeItemCollection;
        private ViewDictionary m_viewDictionary;
        private double m_mappingVersion = XmlConstants.UndefinedVersion;

        private MetadataWorkspace m_workspace;

        // In this version, we won't allow same types in CSpace to map to different types in store. If the same type
        // need to be reused, the store type must be the same. To keep track of this, we need to keep track of the member 
        // mapping across maps to make sure they are mapped to the same store side.
        // The first TypeUsage in the KeyValuePair stores the store equivalent type for the cspace member type and the second
        // one store the actual store type to which the member is mapped to.
        // For e.g. If the CSpace member of type Edm.Int32 maps to a sspace member of type SqlServer.bigint, then the KeyValuePair
        // for the cspace member will contain SqlServer.int (store equivalent for Edm.Int32) and SqlServer.bigint (Actual store type
        // to which the member was mapped to)
        private Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> m_memberMappings = new Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>>();
        private ViewLoader _viewLoader;

        internal enum InterestingMembersKind
        { 
            RequiredOriginalValueMembers,   // legacy - used by the obsolete GetRequiredOriginalValueMembers
            FullUpdate,                     // Interesting members in case of full update scenario
            PartialUpdate                   // Interesting members in case of partial update scenario
        };

        private ConcurrentDictionary<Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>, ReadOnlyCollection<EdmMember>> _cachedInterestingMembers =
            new ConcurrentDictionary<Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>, ReadOnlyCollection<EdmMember>>();

        #endregion

        #region Constructors
        /// <summary>
        /// constructor that takes in a list of folder or files or a mix of both and
        /// creates metadata for mapping in all the files.
        /// </summary>
        /// <param name="edmCollection"></param>
        /// <param name="storeCollection"></param>
        /// <param name="filePaths"></param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but we do not create the file paths in this method
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public StorageMappingItemCollection(EdmItemCollection edmCollection, StoreItemCollection storeCollection,
            params string[] filePaths)
            : base(DataSpace.CSSpace)
        {
            EntityUtil.CheckArgumentNull(edmCollection, "edmCollection");
            EntityUtil.CheckArgumentNull(storeCollection, "storeCollection");
            EntityUtil.CheckArgumentNull(filePaths, "filePaths");

            this.m_edmCollection = edmCollection;
            this.m_storeItemCollection = storeCollection;

            // Wrap the file paths in instances of the MetadataArtifactLoader class, which provides
            // an abstraction and a uniform interface over a diverse set of metadata artifacts.
            //
            MetadataArtifactLoader composite = null;
            List<XmlReader> readers = null;
            try
            {
                composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(filePaths, XmlConstants.CSSpaceSchemaExtension);
                readers = composite.CreateReaders(DataSpace.CSSpace);

                this.Init(edmCollection, storeCollection, readers,
                          composite.GetPaths(DataSpace.CSSpace), true /*throwOnError*/);
            }
            finally
            {
                if (readers != null)
                {
                    Helper.DisposeXmlReaders(readers);
                }
            }
        }

        /// <summary>
        /// constructor that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public StorageMappingItemCollection(EdmItemCollection edmCollection,
                                            StoreItemCollection storeCollection,
                                            IEnumerable<XmlReader> xmlReaders)
            : base(DataSpace.CSSpace)
        {
            EntityUtil.CheckArgumentNull(xmlReaders, "xmlReaders");

            MetadataArtifactLoader composite = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);

            this.Init(edmCollection,
                      storeCollection,
                      composite.GetReaders(),   // filter out duplicates
                      composite.GetPaths(),
                      true /* throwOnError*/);

        }

        /// <summary>
        /// constructor that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="filePaths">Mapping URIs</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        /// <param name="errors">a list of errors for each file loaded</param>
        // referenced by System.Data.Entity.Design.dll
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal StorageMappingItemCollection(EdmItemCollection edmCollection,
                                              StoreItemCollection storeCollection,
                                              IEnumerable<XmlReader> xmlReaders,
                                              List<string> filePaths,
                                              out IList<EdmSchemaError> errors)
            : base(DataSpace.CSSpace)
        {
            // we will check the parameters for this internal ctor becuase
            // it is pretty much publicly exposed through the MetadataItemCollectionFactory
            // in System.Data.Entity.Design
            EntityUtil.CheckArgumentNull(xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
            // filePaths is allowed to be null

            errors = this.Init(edmCollection, storeCollection, xmlReaders, filePaths, false /*throwOnError*/);
        }

        /// <summary>
        /// constructor that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="filePaths">Mapping URIs</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal StorageMappingItemCollection(EdmItemCollection edmCollection,
                                              StoreItemCollection storeCollection,
                                              IEnumerable<XmlReader> xmlReaders,
                                              List<string> filePaths)
            : base(DataSpace.CSSpace)
        {
            this.Init(edmCollection, storeCollection, xmlReaders, filePaths, true /*throwOnError*/);
        }

        /// <summary>
        /// Initializer that takes in a list of XmlReaders and creates metadata for mapping 
        /// in all the files.  
        /// </summary>
        /// <param name="edmCollection">The edm metadata collection that this mapping is to use</param>
        /// <param name="storeCollection">The store metadata collection that this mapping is to use</param>
        /// <param name="filePaths">Mapping URIs</param>
        /// <param name="xmlReaders">The XmlReaders to load mapping from</param>
        /// <param name="errors">a list of errors for each file loaded</param>
        private IList<EdmSchemaError> Init(EdmItemCollection edmCollection,
                          StoreItemCollection storeCollection,
                          IEnumerable<XmlReader> xmlReaders,
                          List<string> filePaths,
                          bool throwOnError)
        {
            EntityUtil.CheckArgumentNull(xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentNull(edmCollection, "edmCollection");
            EntityUtil.CheckArgumentNull(storeCollection, "storeCollection");

            this.m_edmCollection = edmCollection;
            this.m_storeItemCollection = storeCollection;
            
            Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict;
            Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict;
            
            this.m_viewDictionary = new ViewDictionary(this, out userDefinedQueryViewsDict, out userDefinedQueryViewsOfTypeDict);

            List<EdmSchemaError> errors = new List<EdmSchemaError>();
            
            if(this.m_edmCollection.EdmVersion != XmlConstants.UndefinedVersion &&
                this.m_storeItemCollection.StoreSchemaVersion != XmlConstants.UndefinedVersion &&
                this.m_edmCollection.EdmVersion != this.m_storeItemCollection.StoreSchemaVersion)
            {
                errors.Add(
                    new EdmSchemaError(
                        Strings.Mapping_DifferentEdmStoreVersion, 
                        (int)StorageMappingErrorCode.MappingDifferentEdmStoreVersion, EdmSchemaErrorSeverity.Error));
            }
            else
            {
                double expectedVersion = this.m_edmCollection.EdmVersion != XmlConstants.UndefinedVersion
                    ? this.m_edmCollection.EdmVersion
                    : this.m_storeItemCollection.StoreSchemaVersion;
                errors.AddRange(LoadItems(xmlReaders, filePaths, userDefinedQueryViewsDict, userDefinedQueryViewsOfTypeDict, expectedVersion));
            }

            Debug.Assert(errors != null);

            if (errors.Count > 0 && throwOnError)
            {
                if (!System.Data.Common.Utils.MetadataHelper.CheckIfAllErrorsAreWarnings(errors))
                {
                    // NOTE: not using Strings.InvalidSchemaEncountered because it will truncate the errors list.
                    throw new MappingException(
                    String.Format(System.Globalization.CultureInfo.CurrentCulture,
                                    EntityRes.GetString(EntityRes.InvalidSchemaEncountered),
                                    Helper.CombineErrorMessage(errors)));
                }
            }

            return errors;
        }

        #endregion Constructors

        internal MetadataWorkspace Workspace
        {
            get
            {
                if (m_workspace == null)
                {
                    m_workspace = new MetadataWorkspace();
                    m_workspace.RegisterItemCollection(m_edmCollection);
                    m_workspace.RegisterItemCollection(m_storeItemCollection);
                    m_workspace.RegisterItemCollection(this);
                }
                return m_workspace;
            }
        }

        /// <summary>
        /// Return the EdmItemCollection associated with the Mapping Collection
        /// </summary>
        internal EdmItemCollection EdmItemCollection
        {
            get
            {
                return this.m_edmCollection;
            }
        }

        /// <summary>
        /// Version of this StorageMappingItemCollection represents.
        /// </summary>
        public double MappingVersion
        {
            get
            {
                return this.m_mappingVersion;
            }
        }

        /// <summary>
        /// Return the StoreItemCollection associated with the Mapping Collection
        /// </summary>
        internal StoreItemCollection StoreItemCollection
        {
            get
            {
                return this.m_storeItemCollection;
            }
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal override Map GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
        {
            EntityUtil.CheckArgumentNull(identity, "identity");
            if (typeSpace != DataSpace.CSpace)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Mapping_Storage_InvalidSpace(typeSpace));
            }
            return GetItem<Map>(identity, ignoreCase);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out Map map)
        {
            if (typeSpace != DataSpace.CSpace)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Mapping_Storage_InvalidSpace(typeSpace));
            }
            return TryGetItem<Map>(identity, ignoreCase, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal override Map GetMap(string identity, DataSpace typeSpace)
        {
            return this.GetMap(identity, typeSpace, false /*ignoreCase*/);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(string identity, DataSpace typeSpace, out Map map)
        {
            return this.TryGetMap(identity, typeSpace, false /*ignoreCase*/, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        internal override Map GetMap(GlobalItem item)
        {
            EntityUtil.CheckArgumentNull(item, "item");
            DataSpace typeSpace = item.DataSpace;
            if (typeSpace != DataSpace.CSpace)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.Mapping_Storage_InvalidSpace(typeSpace));
            }
            return this.GetMap(item.Identity, typeSpace);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(GlobalItem item, out Map map)
        {
            if (item == null)
            {
                map = null;
                return false;
            }
            DataSpace typeSpace = item.DataSpace;
            if (typeSpace != DataSpace.CSpace)
            {
                map = null;
                return false;
            }
            return this.TryGetMap(item.Identity, typeSpace, out map);
        }

        /// <summary>
        /// This method
        ///     - generates views from the mapping elements in the collection;
        ///     - does not process user defined views - these are processed during mapping collection loading;
        ///     - does not cache generated views in the mapping collection.
        /// The main purpose is design-time view validation and generation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced by System.Data.Entity.Design.dll
        internal Dictionary<EntitySetBase, string> GenerateEntitySetViews(out IList<EdmSchemaError> errors)
        {
            Dictionary<EntitySetBase, string> esqlViews = new Dictionary<EntitySetBase, string>();
            errors = new List<EdmSchemaError>();
            foreach (var mapping in GetItems<Map>())
            {
                var entityContainerMapping = mapping as StorageEntityContainerMapping;
                if (entityContainerMapping != null)
                {
                    // If there are no entity set maps, don't call the view generation process.
                    if (!entityContainerMapping.HasViews)
                    {
                        return esqlViews;
                    }

                    // If entityContainerMapping contains only query views, then add a warning to the errors and continue to next mapping.
                    if (!entityContainerMapping.HasMappingFragments())
                    {
                        Debug.Assert(2088 == (int)StorageMappingErrorCode.MappingAllQueryViewAtCompileTime, "Please change the ERRORCODE_MAPPINGALLQUERYVIEWATCOMPILETIME value as well");
                        errors.Add(new EdmSchemaError(
                            Strings.Mapping_AllQueryViewAtCompileTime(entityContainerMapping.Identity),
                            (int)StorageMappingErrorCode.MappingAllQueryViewAtCompileTime,
                            EdmSchemaErrorSeverity.Warning));
                    }
                    else
                    {
                        ViewGenResults viewGenResults = ViewgenGatekeeper.GenerateViewsFromMapping(entityContainerMapping, new ConfigViewGenerator() { GenerateEsql = true });
                        if (viewGenResults.HasErrors)
                        {
                            ((List<EdmSchemaError>)errors).AddRange(viewGenResults.Errors);
                        }
                        KeyToListMap<EntitySetBase, GeneratedView> extentMappingViews = viewGenResults.Views;
                        foreach (KeyValuePair<EntitySetBase, List<GeneratedView>> extentViewPair in extentMappingViews.KeyValuePairs)
                        {
                            List<GeneratedView> generatedViews = extentViewPair.Value;
                            // Multiple Views are returned for an extent but the first view
                            // is the only one that we will use for now. In the future,
                            // we might start using the other views which are per type within an extent.
                            esqlViews.Add(extentViewPair.Key, generatedViews[0].eSQL);
                        }
                    }
                }
            }
            return esqlViews;
        }

        #region Get interesting members

        /// <summary>
        /// Return members for MetdataWorkspace.GetRequiredOriginalValueMembers() and MetdataWorkspace.GetRelevantMembersForUpdate() methods.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space. Must not be null.</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet. Must not be null.</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <returns>ReadOnlyCollection of interesting members for the requested scenario (<paramref name="interestingMembersKind"/>).</returns>        
        internal ReadOnlyCollection<EdmMember> GetInterestingMembers(EntitySetBase entitySet, EntityTypeBase entityType, InterestingMembersKind interestingMembersKind)
        {
            Debug.Assert(entitySet != null, "entitySet != null");
            Debug.Assert(entityType != null, "entityType != null");

            var key = new Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>(entitySet, entityType, interestingMembersKind);
            return _cachedInterestingMembers.GetOrAdd(key,  FindInterestingMembers(entitySet, entityType, interestingMembersKind));
        }

        /// <summary>
        /// Finds interesting members for MetdataWorkspace.GetRequiredOriginalValueMembers() and MetdataWorkspace.GetRelevantMembersForUpdate() methods
        /// for the given <paramref name="entitySet"/> and <paramref name="entityType"/>.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space. Must not be null.</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet. Must not be null.</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <returns>ReadOnlyCollection of interesting members for the requested scenario (<paramref name="interestingMembersKind"/>).</returns>        
        private ReadOnlyCollection<EdmMember> FindInterestingMembers(EntitySetBase entitySet, EntityTypeBase entityType, InterestingMembersKind interestingMembersKind)
        {
            Debug.Assert(entitySet != null, "entitySet != null");
            Debug.Assert(entityType != null, "entityType != null");

            var interestingMembers = new List<EdmMember>();

            foreach (var storageTypeMapping in MappingMetadataHelper.GetMappingsForEntitySetAndSuperTypes(this, entitySet.EntityContainer, entitySet, entityType))
            {
                StorageAssociationTypeMapping associationTypeMapping = storageTypeMapping as StorageAssociationTypeMapping;
                if (associationTypeMapping != null)
                {
                    FindInterestingAssociationMappingMembers(associationTypeMapping, interestingMembers);
                }
                else
                {
                    Debug.Assert(storageTypeMapping is StorageEntityTypeMapping, "StorageEntityTypeMapping expected.");

                    FindInterestingEntityMappingMembers((StorageEntityTypeMapping)storageTypeMapping, interestingMembersKind, interestingMembers);
                }
            }

            // For backwards compatibility we don't return foreign keys from the obsolete MetadataWorkspace.GetRequiredOriginalValueMembers() method
            if (interestingMembersKind != InterestingMembersKind.RequiredOriginalValueMembers)
            {
                FindForeignKeyProperties(entitySet, entityType, interestingMembers);
            }

            foreach (var functionMappings in MappingMetadataHelper
                                                .GetModificationFunctionMappingsForEntitySetAndType(this, entitySet.EntityContainer, entitySet, entityType)
                                                .Where(functionMappings => functionMappings.UpdateFunctionMapping != null))
            {
                FindInterestingFunctionMappingMembers(functionMappings, interestingMembersKind, ref interestingMembers);
            }

            Debug.Assert(interestingMembers != null, "interestingMembers must never be null.");

            return new ReadOnlyCollection<EdmMember>(interestingMembers.Distinct().ToList());
        }

        /// <summary>
        /// Finds members participating in the assocciation and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="associationTypeMapping">Association type mapping. Must not be null.</param>
        /// <param name="interestingMembers">The list the interesting members (if any) will be added to. Must not be null.</param>
        private static void FindInterestingAssociationMappingMembers(StorageAssociationTypeMapping associationTypeMapping, List<EdmMember> interestingMembers)
        {
            Debug.Assert(associationTypeMapping != null, "entityTypeMapping != null");
            Debug.Assert(interestingMembers != null, "interestingMembers != null");

            //(2) Ends participating in association are "interesting"
            interestingMembers.AddRange(
                associationTypeMapping
                .MappingFragments
                .SelectMany(m => m.AllProperties)
                .OfType<StorageEndPropertyMapping>()
                .Select(epm => epm.EndMember));
        }

        /// <summary>
        /// Finds interesting entity properties - primary keys (if requested), properties (including complex properties and nested properties)
        /// with concurrency mode set to fixed and C-Side condition members and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="entityTypeMapping">Entity type mapping. Must not be null.</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <param name="interestingMembers">The list the interesting members (if any) will be added to. Must not be null.</param>
        private static void FindInterestingEntityMappingMembers(StorageEntityTypeMapping entityTypeMapping, InterestingMembersKind interestingMembersKind, List<EdmMember> interestingMembers)
        {
            Debug.Assert(entityTypeMapping != null, "entityTypeMapping != null");
            Debug.Assert(interestingMembers != null, "interestingMembers != null");

            foreach (var propertyMapping in entityTypeMapping.MappingFragments.SelectMany(mf => mf.AllProperties))
            {
                StorageScalarPropertyMapping scalarPropMapping = propertyMapping as StorageScalarPropertyMapping;
                StorageComplexPropertyMapping complexPropMapping = propertyMapping as StorageComplexPropertyMapping;
                StorageConditionPropertyMapping conditionMapping = propertyMapping as StorageConditionPropertyMapping;

                Debug.Assert(!(propertyMapping is StorageEndPropertyMapping), "association mapping properties should be handled elsewhere.");

                Debug.Assert(scalarPropMapping != null ||
                             complexPropMapping != null ||
                             conditionMapping != null, "Unimplemented property mapping");

                //scalar property
                if (scalarPropMapping != null && scalarPropMapping.EdmProperty != null)
                {
                    // (0) if a member is part of the key it is interesting
                    if (MetadataHelper.IsPartOfEntityTypeKey(scalarPropMapping.EdmProperty))
                    {
                        // For backwards compatibility we do return primary keys from the obsolete MetadataWorkspace.GetRequiredOriginalValueMembers() method
                        if (interestingMembersKind == InterestingMembersKind.RequiredOriginalValueMembers)
                        {
                            interestingMembers.Add(scalarPropMapping.EdmProperty);
                        }
                    }
                    //(3) if a scalar property has Fixed concurrency mode then it is "interesting"
                    else if (MetadataHelper.GetConcurrencyMode(scalarPropMapping.EdmProperty) == ConcurrencyMode.Fixed)
                    {
                        interestingMembers.Add(scalarPropMapping.EdmProperty);
                    }
                }
                else if (complexPropMapping != null)
                {
                    // (7) All complex members - partial update scenarios only
                    // (3.1) The complex property or its one of its children has fixed concurrency mode
                    if (interestingMembersKind == InterestingMembersKind.PartialUpdate ||
                        MetadataHelper.GetConcurrencyMode(complexPropMapping.EdmProperty) == ConcurrencyMode.Fixed || HasFixedConcurrencyModeInAnyChildProperty(complexPropMapping))
                    {
                        interestingMembers.Add(complexPropMapping.EdmProperty);
                    }
                }
                else if (conditionMapping != null)
                {
                    //(1) C-Side condition members are 'interesting'
                    if (conditionMapping.EdmProperty != null)
                    {
                        interestingMembers.Add(conditionMapping.EdmProperty);
                    }
                }
            }
        }

        /// <summary>
        /// Recurses down the complex property to find whether any of the nseted properties has concurrency mode set to "Fixed"
        /// </summary>
        /// <param name="complexMapping">Complex property mapping. Must not be null.</param>
        /// <returns><c>true</c> if any of the descendant properties has concurrency mode set to "Fixed". Otherwise <c>false</c>.</returns>
        private static bool HasFixedConcurrencyModeInAnyChildProperty(StorageComplexPropertyMapping complexMapping)
        {
            Debug.Assert(complexMapping != null, "complexMapping != null");

            foreach (StoragePropertyMapping propertyMapping in complexMapping.TypeMappings.SelectMany(m => m.AllProperties))
            {
                StorageScalarPropertyMapping childScalarPropertyMapping = propertyMapping as StorageScalarPropertyMapping;
                StorageComplexPropertyMapping childComplexPropertyMapping = propertyMapping as StorageComplexPropertyMapping;

                Debug.Assert(childScalarPropertyMapping != null ||
                             childComplexPropertyMapping != null, "Unimplemented property mapping for complex property");

                //scalar property and has Fixed CC mode
                if (childScalarPropertyMapping != null && MetadataHelper.GetConcurrencyMode(childScalarPropertyMapping.EdmProperty) == ConcurrencyMode.Fixed)
                {
                    return true;
                }
                // Complex Prop and sub-properties or itself has fixed CC mode
                else if (childComplexPropertyMapping != null &&
                    (MetadataHelper.GetConcurrencyMode(childComplexPropertyMapping.EdmProperty) == ConcurrencyMode.Fixed
                        || HasFixedConcurrencyModeInAnyChildProperty(childComplexPropertyMapping)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds foreign key properties and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="entitySetBase">Entity set <paramref name="entityType"/> relates to. Must not be null.</param>
        /// <param name="entityType">Entity type for which to find foreign key properties. Must not be null.</param>
        /// <param name="interestingMembers">The list the interesting members (if any) will be added to. Must not be null.</param>
        private void FindForeignKeyProperties(EntitySetBase entitySetBase, EntityTypeBase entityType, List<EdmMember> interestingMembers)
        {
            var entitySet = entitySetBase as EntitySet;
            if (entitySet != null && entitySet.HasForeignKeyRelationships)
            {
                // (6) Foreign keys
                // select all foreign key properties defined on the entityType and all its ancestors
                interestingMembers.AddRange(
                        MetadataHelper.GetTypeAndParentTypesOf(entityType, this.m_edmCollection, true)
                        .SelectMany(e => ((EntityType)e).Properties)
                        .Where(p => entitySet.ForeignKeyDependents.SelectMany(fk => fk.Item2.ToProperties).Contains(p)));
            }
        }
        
        /// <summary>
        /// Finds interesting members for modification functions mapped to stored procedures and adds them to the <paramref name="interestingMembers"/>.
        /// </summary>
        /// <param name="functionMappings">Modification function mapping. Must not be null.</param>
        /// <param name="interestingMembersKind">Update scenario the members will be used in (in general - partial update vs. full update).</param>
        /// <param name="interestingMembers"></param>
        private static void FindInterestingFunctionMappingMembers(StorageEntityTypeModificationFunctionMapping functionMappings, InterestingMembersKind interestingMembersKind, ref List<EdmMember> interestingMembers)
        {
            Debug.Assert(functionMappings != null && functionMappings.UpdateFunctionMapping != null, "Expected function mapping fragment with non-null update function mapping");
            Debug.Assert(interestingMembers != null, "interestingMembers != null");

            // for partial update scenarios (e.g. EntityDataSourceControl) all members are interesting otherwise the data may be corrupt. 
            // See bugs #272992 and #124460 in DevDiv database for more details. For full update scenarios and the obsolete 
            // MetadataWorkspace.GetRequiredOriginalValueMembers() metod we return only members with Version set to "Original".
            if (interestingMembersKind == InterestingMembersKind.PartialUpdate)
            {
                // (5) Members included in Update ModificationFunction
                interestingMembers.AddRange(functionMappings.UpdateFunctionMapping.ParameterBindings.Select(p => p.MemberPath.Members.Last()));
            }
            else
            {
                //(4) Members in update ModificationFunction with Version="Original" are "interesting"
                // This also works when you have complex-types (4.1)

                Debug.Assert(
                    interestingMembersKind == InterestingMembersKind.FullUpdate || interestingMembersKind == InterestingMembersKind.RequiredOriginalValueMembers,
                    "Unexpected kind of interesting members - if you changed the InterestingMembersKind enum type update this code accordingly");

                foreach (var parameterBinding in functionMappings.UpdateFunctionMapping.ParameterBindings.Where(p => !p.IsCurrent))
                {
                    //Last is the root element (with respect to the Entity)
                    //For example,  Entity1={
                    //                  S1, 
                    //                  C1{S2, 
                    //                     C2{ S3, S4 } 
                    //                     }, 
                    //                  S5}
                    // if S4 matches (i.e. C1.C2.S4), then it returns C1
                    //because internally the list is [S4][C2][C1]
                    interestingMembers.Add(parameterBinding.MemberPath.Members.Last());
                }
            }
        }

        #endregion

        /// <summary>
        /// Calls the view dictionary to load the view, see detailed comments in the view dictionary class.
        /// </summary>
        internal GeneratedView GetGeneratedView(EntitySetBase extent, MetadataWorkspace workspace)
        {
            return this.m_viewDictionary.GetGeneratedView(extent, workspace, this);
        }

        // Add to the cache. If it is already present, then throw an exception
        private void AddInternal(Map storageMap)
        {
            storageMap.DataSpace = DataSpace.CSSpace;
            try
            {
                base.AddInternal(storageMap);
            }
            catch (ArgumentException e)
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_Duplicate_Type(storageMap.EdmItem.Identity), e);
            }
        }

        // Contains whether the given StorageEntityContainerName
        internal bool ContainsStorageEntityContainer(string storageEntityContainerName)
        {
            ReadOnlyCollection<StorageEntityContainerMapping> entityContainerMaps =
                this.GetItems<StorageEntityContainerMapping>();
            return entityContainerMaps.Any(map => map.StorageEntityContainer.Name.Equals(storageEntityContainerName, StringComparison.Ordinal));
        }


        /// <summary>
        /// This helper method loads items based on contents of in-memory XmlReader instances.
        /// Assumption: This method is called only from the constructor because m_extentMappingViews is not thread safe.
        /// </summary>
        /// <param name="xmlReaders">A list of XmlReader instances</param>
        /// <param name="mappingSchemaUris">A list of URIs</param>
        /// <returns>A list of schema errors</returns>
        private List<EdmSchemaError> LoadItems(IEnumerable<XmlReader> xmlReaders,
                                               List<string> mappingSchemaUris,
                                               Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict,
                                               Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict,
                                               double expectedVersion)
        {
            Debug.Assert(m_memberMappings.Count == 0, "Assumption: This method is called only once, and from the constructor because m_extentMappingViews is not thread safe.");

            List<EdmSchemaError> errors = new List<EdmSchemaError>();

            int index = -1;
            foreach (XmlReader xmlReader in xmlReaders)
            {
                index++;
                string location = null;
                if (mappingSchemaUris == null)
                {
                    som.SchemaManager.TryGetBaseUri(xmlReader, out location);
                }
                else
                {
                    location = mappingSchemaUris[index];
                }

                StorageMappingItemLoader mapLoader = new StorageMappingItemLoader(
                                                            xmlReader,
                                                            this,
                                                            location,  // ASSUMPTION: location is only used for generating error-messages
                                                            m_memberMappings);
                errors.AddRange(mapLoader.ParsingErrors);

                CheckIsSameVersion(expectedVersion, mapLoader.MappingVersion, errors);

                // Process container mapping.
                StorageEntityContainerMapping containerMapping = mapLoader.ContainerMapping;
                if (mapLoader.HasQueryViews && containerMapping != null)
                {
                    // Compile the query views so that we can report the errors in the user specified views.
                    CompileUserDefinedQueryViews(containerMapping, userDefinedQueryViewsDict, userDefinedQueryViewsOfTypeDict, errors);
                }
                // Add container mapping if there are no errors and entity container mapping is not already present.
                if (MetadataHelper.CheckIfAllErrorsAreWarnings(errors) && !this.Contains(containerMapping))
                {
                    AddInternal(containerMapping);
                }
            }

            CheckForDuplicateItems(EdmItemCollection, StoreItemCollection, errors);

            return errors;
        }

        /// <summary>
        /// This method compiles all the user defined query views in the <paramref name="entityContainerMapping"/>.
        /// </summary>
        private static void CompileUserDefinedQueryViews(StorageEntityContainerMapping entityContainerMapping,
                                                         Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict,
                                                         Dictionary<OfTypeQVCacheKey, GeneratedView> userDefinedQueryViewsOfTypeDict,
                                                         IList<EdmSchemaError> errors)
        {
            ConfigViewGenerator config = new ConfigViewGenerator();
            foreach (StorageSetMapping setMapping in entityContainerMapping.AllSetMaps)
            {
                if (setMapping.QueryView != null)
                {
                    GeneratedView generatedView;
                    if (!userDefinedQueryViewsDict.TryGetValue(setMapping.Set, out generatedView))
                    {
                        // Parse the view so that we will get back any errors in the view.
                        if (GeneratedView.TryParseUserSpecifiedView(setMapping,
                                                                    setMapping.Set.ElementType,
                                                                    setMapping.QueryView,
                                                                    true, // includeSubtypes
                                                                    entityContainerMapping.StorageMappingItemCollection,
                                                                    config,
                                                                    /*out*/ errors,
                                                                    out generatedView))
                        {
                            // Add first QueryView
                            userDefinedQueryViewsDict.Add(setMapping.Set, generatedView);
                        }

                        // Add all type-specific QueryViews
                        foreach (OfTypeQVCacheKey key in setMapping.GetTypeSpecificQVKeys())
                        {
                            Debug.Assert(key.First.Equals(setMapping.Set));

                            if (GeneratedView.TryParseUserSpecifiedView(setMapping,
                                                                        key.Second.First, // type
                                                                        setMapping.GetTypeSpecificQueryView(key),
                                                                        key.Second.Second, // includeSubtypes
                                                                        entityContainerMapping.StorageMappingItemCollection,
                                                                        config,
                                                                        /*out*/ errors,
                                                                        out generatedView))
                            {
                                userDefinedQueryViewsOfTypeDict.Add(key, generatedView);
                            }
                        }
                    }
                }
            }
        }

        private void CheckIsSameVersion(double expectedVersion, double currentLoaderVersion, IList<EdmSchemaError> errors)
        {
            if (m_mappingVersion == XmlConstants.UndefinedVersion)
            {
                m_mappingVersion = currentLoaderVersion;
            }
            if (expectedVersion != XmlConstants.UndefinedVersion && currentLoaderVersion != XmlConstants.UndefinedVersion && currentLoaderVersion != expectedVersion)
            {
                // Check that the mapping version is the same as the storage and model version
                errors.Add(
                    new EdmSchemaError(
                        Strings.Mapping_DifferentMappingEdmStoreVersion,
                        (int)StorageMappingErrorCode.MappingDifferentMappingEdmStoreVersion, EdmSchemaErrorSeverity.Error));
            }
            if (currentLoaderVersion != m_mappingVersion && currentLoaderVersion != XmlConstants.UndefinedVersion)
            {
                // Check that the mapping versions are all consistent with each other
                errors.Add(
                   new EdmSchemaError(
                       Strings.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                       (int)StorageMappingErrorCode.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection,
                       EdmSchemaErrorSeverity.Error));
            }
        }

        /// <summary>
        /// Return the update view loader
        /// </summary>
        /// <returns></returns>
        internal ViewLoader GetUpdateViewLoader()
        {
            if (_viewLoader == null)
            {
                _viewLoader = new ViewLoader(this);
            }

            return _viewLoader;
        }

        /// <summary>
        /// this method will be called in metadatworkspace, the signature is the same as the one in ViewDictionary
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="includeSubtypes"></param>
        /// <param name="generatedView"></param>
        /// <returns></returns>
        internal bool TryGetGeneratedViewOfType(MetadataWorkspace workspace, EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
        {
            return this.m_viewDictionary.TryGetGeneratedViewOfType(workspace, entity, type, includeSubtypes, out generatedView);
        }

        // Check for duplicate items (items with same name) in edm item collection and store item collection. Mapping is the only logical place to do this. 
        // The only other place is workspace, but that is at the time of registering item collections (only when the second one gets registered) and we 
        // will have to throw exceptions at that time. If we do this check in mapping, we might throw error in a more consistent way (by adding it to error
        // collection). Also if someone is just creating item collection, and not registering it with workspace (tools), doing it in mapping makes more sense
        private static void CheckForDuplicateItems(EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, List<EdmSchemaError> errorCollection)
        {
            Debug.Assert(edmItemCollection != null && storeItemCollection != null && errorCollection != null, "The parameters must not be null in CheckForDuplicateItems");

            foreach (GlobalItem item in edmItemCollection)
            {
                if (storeItemCollection.Contains(item.Identity))
                {
                    errorCollection.Add(new EdmSchemaError(Strings.Mapping_ItemWithSameNameExistsBothInCSpaceAndSSpace(item.Identity),
                                        (int)StorageMappingErrorCode.ItemWithSameNameExistsBothInCSpaceAndSSpace, EdmSchemaErrorSeverity.Error));
                }
            }
        }
    }//---- ItemCollection

}//---- 
