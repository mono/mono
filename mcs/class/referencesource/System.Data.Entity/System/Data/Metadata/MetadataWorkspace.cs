//---------------------------------------------------------------------
// <copyright file="MetadataWorkspace.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Data.Common.CommandTrees;
    using eSQL = System.Data.Common.EntitySql;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping;
    using System.Data.Mapping.Update.Internal;
    using System.Data.Mapping.ViewGeneration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// Runtime Metadata Workspace
    /// </summary>
    public sealed class MetadataWorkspace
    {
        #region Constructors
        /// <summary>
        /// Constructs the new instance of runtime metadata workspace
        /// </summary>
        public MetadataWorkspace()
        {
        }

        /// <summary>
        /// Create MetadataWorkspace that is populated with ItemCollections for all the spaces that the metadata artifacts provided.
        /// All res:// paths will be resolved only from the assemblies returned from the enumerable assembliesToConsider.
        /// </summary>
        /// <param name="paths">The paths where the metadata artifacts located</param>
        /// <param name="assembliesToConsider">User specified assemblies to consider</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Throw when assembliesToConsider is empty or contains null, or cannot find the corresponding assembly in it</exception>
        /// <exception cref="MetadataException"></exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For MetadataWorkspace.CreateMetadataWorkspaceWithResolver method call but we do not create the file paths in this method 
        public MetadataWorkspace(IEnumerable<string> paths, IEnumerable<Assembly> assembliesToConsider)
        {
            // we are intentionally not checking to see if the paths enumerable is empty
            EntityUtil.CheckArgumentNull(paths, "paths");
            EntityUtil.CheckArgumentContainsNull(ref paths, "paths");

            EntityUtil.CheckArgumentNull(assembliesToConsider, "assembliesToConsider");
            EntityUtil.CheckArgumentContainsNull(ref assembliesToConsider, "assembliesToConsider");

            Func<AssemblyName, Assembly> resolveReference = (AssemblyName referenceName)=>
            {
                foreach(Assembly assembly in assembliesToConsider)
                {
                    if (AssemblyName.ReferenceMatchesDefinition(referenceName, new AssemblyName(assembly.FullName)))
                    {
                        return assembly;
                    }
                }
                throw EntityUtil.Argument(Strings.AssemblyMissingFromAssembliesToConsider(referenceName.FullName), "assembliesToConsider");
            };


            CreateMetadataWorkspaceWithResolver(paths, () => assembliesToConsider, resolveReference);
        }

        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but We do not create the file paths in this method 
        private void CreateMetadataWorkspaceWithResolver(IEnumerable<string> paths, Func<IEnumerable<Assembly>> wildcardAssemblies, Func<AssemblyName, Assembly> resolveReference)
        {
            MetadataArtifactLoader composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(paths.ToArray(), "", new CustomAssemblyResolver(wildcardAssemblies, resolveReference));

            // only create the ItemCollection that has corresponding artifacts
            DataSpace dataSpace = DataSpace.CSpace;
            using (DisposableCollectionWrapper<XmlReader> cSpaceReaders = new DisposableCollectionWrapper<XmlReader>(composite.CreateReaders(dataSpace)))
            {
                if (cSpaceReaders.Any())
                {
                    this._itemsCSpace = new EdmItemCollection(cSpaceReaders, composite.GetPaths(dataSpace));
                }
            }

            dataSpace = DataSpace.SSpace;
            using (DisposableCollectionWrapper<XmlReader> sSpaceReaders = new DisposableCollectionWrapper<XmlReader>(composite.CreateReaders(dataSpace)))
            {
                if (sSpaceReaders.Any())
                {
                    this._itemsSSpace = new StoreItemCollection(sSpaceReaders, composite.GetPaths(dataSpace));
                }
            }

            dataSpace = DataSpace.CSSpace;
            using (DisposableCollectionWrapper<XmlReader> csSpaceReaders = new DisposableCollectionWrapper<XmlReader>(composite.CreateReaders(dataSpace)))
            {
                if (csSpaceReaders.Any() && null != this._itemsCSpace && null != this._itemsSSpace)
                {
                    this._itemsCSSpace = new StorageMappingItemCollection(this._itemsCSpace, this._itemsSSpace, csSpaceReaders, composite.GetPaths(dataSpace));
                }
            }
        }
        #endregion

        #region Fields
        private EdmItemCollection _itemsCSpace;
        private StoreItemCollection _itemsSSpace;
        private ObjectItemCollection _itemsOSpace;
        private StorageMappingItemCollection _itemsCSSpace;
        private DefaultObjectMappingItemCollection _itemsOCSpace;

        private List<object> _cacheTokens;
        private bool _foundAssemblyWithAttribute = false;
        private double _schemaVersion = XmlConstants.UndefinedVersion;
        private Guid _metadataWorkspaceId = Guid.Empty;
        #endregion

        #region public static Fields
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        private static IEnumerable<double> SupportedEdmVersions
        {
            get
            {
                yield return XmlConstants.UndefinedVersion;
                yield return XmlConstants.EdmVersionForV1;
                yield return XmlConstants.EdmVersionForV2;
                Debug.Assert(XmlConstants.SchemaVersionLatest == XmlConstants.EdmVersionForV3, "Did you add a new version?");
                yield return XmlConstants.EdmVersionForV3;
            }
        }
        //The Max EDM version thats going to be supported by the runtime.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public static readonly double MaximumEdmVersionSupported = SupportedEdmVersions.Last();
        #endregion

        #region Methods
        /// <summary>
        /// Create an <see cref="eSQL.EntitySqlParser"/> configured to use the <see cref="DataSpace.CSpace"/> data space.
        /// </summary>
        public eSQL.EntitySqlParser CreateEntitySqlParser()
        {
            return new eSQL.EntitySqlParser(new ModelPerspective(this));
        }

        /// <summary>
        /// Creates a new <see cref="DbQueryCommandTree"/> bound to this metadata workspace based on the specified query expression.
        /// </summary>
        /// <param name="query">A <see cref="DbExpression"/> that defines the query</param>
        /// <returns>A new <see cref="DbQueryCommandTree"/> with the specified expression as it's <see cref="DbQueryCommandTree.Query"/> property</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="query"/> is null</exception>
        /// <exception cref="ArgumentException">If <paramref name="query"/> contains metadata that cannot be resolved in this metadata workspace</exception>
        /// <exception cref="ArgumentException">If <paramref name="query"/> is not structurally valid because it contains unresolvable variable references</exception>
        public DbQueryCommandTree CreateQueryCommandTree(DbExpression query)
        {
            return new DbQueryCommandTree(this, DataSpace.CSpace, query);
        }

        /// <summary>
        /// Get item collection for the space. The ItemCollection is in read only mode as it is
        /// part of the workspace.
        /// </summary>
        /// <param name="dataSpace">The dataspace for the item colelction that should be returned</param>
        /// <returns>The item collection for the given space</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        [CLSCompliant(false)]
        public ItemCollection GetItemCollection(DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection;
        }

        /// <summary>
        /// Register the item collection for the space associated with it.
        /// This should be done only once for a space.
        /// If a space already has a registered ItemCollection InvalidOperation exception is thrown
        /// </summary>
        /// <param name="collection">The out parameter collection that needs to be filled up</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if collection argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If there is an ItemCollection that has already been registered for collection's space passed in</exception>
        [CLSCompliant(false)]
        public void RegisterItemCollection(ItemCollection collection)
        {
            EntityUtil.CheckArgumentNull(collection, "collection");

            ItemCollection existing;
            
            try
            {
                switch (collection.DataSpace)
                {
                case DataSpace.CSpace:
                    if (null == (existing = _itemsCSpace)) {
                        EdmItemCollection edmCollection = (EdmItemCollection)collection;
                        if (!MetadataWorkspace.SupportedEdmVersions.Contains(edmCollection.EdmVersion))
                        {

                            throw EntityUtil.InvalidOperation(
                                System.Data.Entity.Strings.EdmVersionNotSupportedByRuntime(
                                edmCollection.EdmVersion, 
                                Helper.GetCommaDelimitedString(
                                    SupportedEdmVersions
                                        .Where(e => e != XmlConstants.UndefinedVersion)
                                        .Select(e => e.ToString(CultureInfo.InvariantCulture)))));
                        }

                        CheckAndSetItemCollectionVersionInWorkSpace(collection);

                        _itemsCSpace = edmCollection;
                    }
                    break;
                case DataSpace.SSpace:
                    if (null == (existing = _itemsSSpace))
                    {
                        CheckAndSetItemCollectionVersionInWorkSpace(collection);
                        _itemsSSpace = (StoreItemCollection)collection;
                    }
                    break;
                case DataSpace.OSpace:
                    if (null == (existing = _itemsOSpace)) {
                        _itemsOSpace = (ObjectItemCollection)collection;
                    }
                    break;
                case DataSpace.CSSpace:
                    if (null == (existing = _itemsCSSpace)) {
                        CheckAndSetItemCollectionVersionInWorkSpace(collection);
                        _itemsCSSpace = (StorageMappingItemCollection)collection;
                    }
                    break;
                default:
                    Debug.Assert(collection.DataSpace == DataSpace.OCSpace, "Invalid DataSpace Enum value: " + collection.DataSpace);
                    
                    if (null == (existing = _itemsOCSpace)) {
                        _itemsOCSpace = (DefaultObjectMappingItemCollection)collection;
                    }
                    break;
                }
            }
            catch (InvalidCastException)
            {
                throw EntityUtil.InvalidCollectionForMapping(collection.DataSpace);
            }
            if (null != existing)
            {
                throw EntityUtil.ItemCollectionAlreadyRegistered(collection.DataSpace);
            }
            // Need to make sure that if the storage mapping Item collection was created with the 
            // same instances of item collection that are registered for CSpace and SSpace
            if (collection.DataSpace == DataSpace.CSpace)
            {
                if (_itemsCSSpace != null && !object.ReferenceEquals(_itemsCSSpace.EdmItemCollection, collection))
                {
                    throw EntityUtil.InvalidCollectionSpecified(collection.DataSpace);
                }
            }

            if (collection.DataSpace == DataSpace.SSpace)
            {
                if (_itemsCSSpace != null && !object.ReferenceEquals(_itemsCSSpace.StoreItemCollection, collection))
                {
                    throw EntityUtil.InvalidCollectionSpecified(collection.DataSpace);
                }
            }

            if (collection.DataSpace == DataSpace.CSSpace)
            {
                if (_itemsCSpace != null && !object.ReferenceEquals(_itemsCSSpace.EdmItemCollection, _itemsCSpace))
                {
                    throw EntityUtil.InvalidCollectionSpecified(collection.DataSpace);
                }

                if (_itemsSSpace != null && !object.ReferenceEquals(_itemsCSSpace.StoreItemCollection, _itemsSSpace))
                {
                    throw EntityUtil.InvalidCollectionSpecified(collection.DataSpace);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemCollectionToRegister"></param>
        private void CheckAndSetItemCollectionVersionInWorkSpace(ItemCollection itemCollectionToRegister)
        {
            double versionToRegister = XmlConstants.UndefinedVersion;
            string itemCollectionType = null;
            switch (itemCollectionToRegister.DataSpace)
            {
                case DataSpace.CSpace:
                    versionToRegister = ((EdmItemCollection)itemCollectionToRegister).EdmVersion;
                    itemCollectionType = "EdmItemCollection";
                    break;
                case DataSpace.SSpace:
                    versionToRegister = ((StoreItemCollection)itemCollectionToRegister).StoreSchemaVersion;
                    itemCollectionType = "StoreItemCollection";
                    break;
                case DataSpace.CSSpace:
                    versionToRegister = ((StorageMappingItemCollection)itemCollectionToRegister).MappingVersion;
                    itemCollectionType = "StorageMappingItemCollection";
                    break;
                default:
                    // we don't care about other spaces so keep the _versionToRegister to Undefined
                    break;
            }

            if (versionToRegister != this._schemaVersion && 
                versionToRegister != XmlConstants.UndefinedVersion && 
                this._schemaVersion != XmlConstants.UndefinedVersion)
            {
                Debug.Assert(itemCollectionType != null);
                throw EntityUtil.DifferentSchemaVersionInCollection(itemCollectionType, versionToRegister, this._schemaVersion);
            }
            else
            {
                this._schemaVersion = versionToRegister;
            }
        }
        /// <summary>
        /// Add a token for this MetadataWorkspace just so this metadata workspace holds a reference to it, this
        /// is for metadata caching to make the workspace marking a particular cache entry is still in used
        /// </summary>
        /// <param name="token"></param>
        internal void AddMetadataEntryToken(object token)
        {
            if (_cacheTokens == null)
            {
                _cacheTokens = new List<object>();
            }

            _cacheTokens.Add(token);
        }

        /// <summary>
        /// Load metadata from the given assembly
        /// </summary>
        /// <param name="assembly">The assembly from which to load metadata</param>
        /// <exception cref="System.ArgumentNullException">thrown if assembly argument is null</exception>
        public void LoadFromAssembly(Assembly assembly)
        {
            LoadFromAssembly(assembly, null);
        }

        /// <summary>
        /// Load metadata from the given assembly
        /// </summary>
        /// <param name="assembly">The assembly from which to load metadata</param>
        /// <param name="logLoadMessage">The delegate for logging the load messages</param>
        /// <exception cref="System.ArgumentNullException">thrown if assembly argument is null</exception>
        public void LoadFromAssembly(Assembly assembly, Action<string> logLoadMessage)
        {
            EntityUtil.CheckArgumentNull(assembly, "assembly");
            ObjectItemCollection collection = (ObjectItemCollection)GetItemCollection(DataSpace.OSpace);
            ExplicitLoadFromAssembly(assembly, collection, logLoadMessage);
        }

        private void ExplicitLoadFromAssembly(Assembly assembly, ObjectItemCollection collection, Action<string> logLoadMessage)
        {
            ItemCollection itemCollection;
            if (!TryGetItemCollection(DataSpace.CSpace, out itemCollection))
            {
                itemCollection = null;
            }

            collection.ExplicitLoadFromAssembly(assembly, (EdmItemCollection)itemCollection, logLoadMessage);
        }

        private void ImplicitLoadFromAssembly(Assembly assembly, ObjectItemCollection collection)
        {
            if (!MetadataAssemblyHelper.ShouldFilterAssembly(assembly))
            {
                ExplicitLoadFromAssembly(assembly, collection, null);
            }
        }

        /// <summary>
        /// Implicit loading means that we are trying to help the user find the right 
        /// assembly, but they didn't explicitly ask for it. Our Implicit rules require that
        /// we filter out assemblies with the Ecma or MicrosoftPublic PublicKeyToken on them
        /// 
        /// Load metadata from the type's assembly into the OSpace ItemCollection.
        /// If type comes from known source, has Ecma or Microsoft PublicKeyToken then the type's assembly is not
        /// loaded, but the callingAssembly and its referenced assemblies are loaded.
        /// </summary>
        /// <param name="type">The type's assembly is loaded into the OSpace ItemCollection</param>
        /// <param name="callingAssembly">The assembly and its referenced assemblies to load when type is insuffiecent</param>
        internal void ImplicitLoadAssemblyForType(Type type, Assembly callingAssembly)
        {
            // this exists separately from LoadFromAssembly so that we can handle generics, like IEnumerable<Product>
            Debug.Assert(null != type, "null type");
            ItemCollection collection;
            if (TryGetItemCollection(DataSpace.OSpace, out collection))
            {   // if OSpace is not loaded - don't register
                ObjectItemCollection objItemCollection = (ObjectItemCollection)collection;
                ItemCollection edmItemCollection;
                TryGetItemCollection(DataSpace.CSpace, out edmItemCollection);
                if (!objItemCollection.ImplicitLoadAssemblyForType(type, (EdmItemCollection)edmItemCollection) && (null != callingAssembly))
                {
                    // only load from callingAssembly if all types were filtered
                    // then loaded referenced assemblies of calling assembly

                    // attempt automatic discovery of user types
                    // interesting code paths are ObjectQuery<object>, ObjectQuery<DbDataRecord>, ObjectQuery<IExtendedDataRecord>
                    // other interesting code paths are ObjectQuery<Nullable<Int32>>, ObjectQuery<IEnumerable<object>>
                    // when assemblies is mscorlib, System.Data or System.Data.Entity
                    
		            // If the schema attribute is presented on the assembly or any referenced assemblies, then it is a V1 scenario that we should
                    // strictly follow the Get all referenced assemblies rules.
                    // If the attribute is not presented on the assembly, then we won't load the referenced ----sembly 
                    // for this callingAssembly
                    if (ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(callingAssembly) ||
                        (_foundAssemblyWithAttribute || 
                          MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(callingAssembly).Any(a => ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(a))))
                    {
                        // cache the knowledge that we found an attribute
                        // because it can be expesive to figure out
                        _foundAssemblyWithAttribute = true;
                        objItemCollection.ImplicitLoadAllReferencedAssemblies(callingAssembly, (EdmItemCollection)edmItemCollection);
                    }
                    else
                    {
                        this.ImplicitLoadFromAssembly(callingAssembly, objItemCollection);
                    }
                }
            }
        }

        /// <summary>
        /// If OSpace is not loaded for the specified EntityType
        /// the load metadata from the callingAssembly and its referenced assemblies.
        /// </summary>
        /// <param name="type">The CSPace type to verify its OSpace counterpart is loaded</param>
        /// <param name="callingAssembly">The assembly and its referenced assemblies to load when type is insuffiecent</param>
        internal void ImplicitLoadFromEntityType(EntityType type, Assembly callingAssembly)
        {
           // used by ObjectContext.*GetObjectByKey when the clr type is not available
           // so we check the OCMap to find the clr type else attempt to autoload the OSpace from callingAssembly
            Debug.Assert(null != type, "null type");
            Map map;
            if (!TryGetMap(type, DataSpace.OCSpace, out map))
            {   // an OCMap is not exist, attempt to load OSpace to retry
                ImplicitLoadAssemblyForType(typeof(System.Data.Objects.DataClasses.IEntityWithKey), callingAssembly);

                // We do a check here to see if the type was actually found in the attempted load.
                ObjectItemCollection ospaceCollection = GetItemCollection(DataSpace.OSpace) as ObjectItemCollection;
                EdmType ospaceType;
                if (ospaceCollection == null || !ospaceCollection.TryGetOSpaceType(type, out ospaceType))
                {
                    throw new InvalidOperationException(System.Data.Entity.Strings.Mapping_Object_InvalidType(type.Identity));
                }
            }
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// For example, The identity for EdmType is Namespace.Name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have an item with the given identity</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public T GetItem<T>(string identity, DataSpace dataSpace) where T:GlobalItem
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetItem<T>(identity, false /*ignoreCase*/);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="space"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if identity or space argument is null</exception>
        public bool TryGetItem<T>(string identity, DataSpace space, out T item ) where T:GlobalItem
        {
            item = null;
            ItemCollection collection = GetItemCollection(space, false);
            return (null != collection) && collection.TryGetItem<T>(identity, false /*ignoreCase*/, out item);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// For example, The identity for EdmType is Namespace.Name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have an item with the given identity</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public T GetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace) where T : GlobalItem
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetItem<T>(identity, ignoreCase);
        }

        /// <summary>
        /// Search for an item with the given identity in the given space.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ignoreCase"></param>
        /// <param name="identity"></param>
        /// <param name="dataSpace"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if identity or space argument is null</exception>
        public bool TryGetItem<T>(string identity, bool ignoreCase, DataSpace dataSpace, out T item) where T : GlobalItem
        {
            item = null;
            ItemCollection collection = GetItemCollection(dataSpace, false);
            return (null != collection) && collection.TryGetItem<T>(identity, ignoreCase, out item);
        }

        /// <summary>
        /// Returns ReadOnlyCollection of the Items of the given type
        /// in the workspace.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<T> GetItems<T>(DataSpace dataSpace) where T : GlobalItem
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetItems<T>();
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName arguments passed in are null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a type with the given name and namespaceName</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EdmType GetType(string name, string namespaceName, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetType(name, namespaceName, false /*ignoreCase*/);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <param name="type">The type that needs to be filled with the return value</param>
        /// <returns>Returns false if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if name, namespaceName or space argument is null</exception>
        public bool TryGetType(string name, string namespaceName, DataSpace dataSpace, out EdmType type)
        {
            type = null;
            ItemCollection collection = GetItemCollection(dataSpace, false);
            return (null != collection) && collection.TryGetType(name, namespaceName, false /*ignoreCase*/, out type);
        }



        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="ignoreCase"></param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName arguments passed in are null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a type with the given name and namespaceName</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EdmType GetType(string name, string namespaceName, bool ignoreCase, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetType(name, namespaceName, ignoreCase);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="dataSpace">Dataspace to search the type for</param>
        /// <param name="ignoreCase"></param>
        /// <param name="type">The type that needs to be filled with the return value</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if name, namespaceName or space argument is null</exception>
        public bool TryGetType(string name, string namespaceName, bool ignoreCase,
                               DataSpace dataSpace, out EdmType type)
        {
            type = null;
            ItemCollection collection = GetItemCollection(dataSpace, false);
            return (null != collection) && collection.TryGetType(name, namespaceName, ignoreCase, out type);
        }


        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="dataSpace"></param>
        /// <returns>The EntityContainer</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EntityContainer with the given name</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EntityContainer GetEntityContainer(string name, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetEntityContainer(name);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="dataSpace"></param>
        /// <param name="entityContainer"></param>
        /// <exception cref="System.ArgumentNullException">if either space or name arguments is null</exception>
        public bool TryGetEntityContainer(string name, DataSpace dataSpace, out EntityContainer entityContainer)
        {
            entityContainer = null;
            // null check exists in call stack, but throws for "identity" not "name"
            EntityUtil.GenericCheckArgumentNull(name, "name");
            ItemCollection collection = GetItemCollection(dataSpace, false);
            return (null != collection) && collection.TryGetEntityContainer(name, out entityContainer);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="dataSpace"></param>
        /// <returns>The EntityContainer</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if name argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EntityContainer with the given name</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public EntityContainer GetEntityContainer(string name, bool ignoreCase, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetEntityContainer(name, ignoreCase);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="dataSpace"></param>
        /// <param name="entityContainer"></param>
        /// <exception cref="System.ArgumentNullException">if name or space argument is null</exception>
        public bool TryGetEntityContainer(string name, bool ignoreCase,
                                                             DataSpace dataSpace, out EntityContainer entityContainer)
        {
            entityContainer = null;
            // null check exists in call stack, but throws for "identity" not "name"
            EntityUtil.GenericCheckArgumentNull(name, "name");
            ItemCollection collection = GetItemCollection(dataSpace, false);
            return (null != collection) && collection.TryGetEntityContainer(name, ignoreCase, out entityContainer);
        }

        /// <summary>
        /// Get all the overloads of the function with the given name
        /// </summary>
        /// <param name="name">name of the function</param>
        /// <param name="namespaceName">namespace of the function</param>
        /// <param name="dataSpace">The dataspace for which we need to get the function for</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if functionName argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EdmFunction with the given functionName</exception>
        /// <exception cref="System.ArgumentException">If the name or namespaceName is empty</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<EdmFunction> GetFunctions(string name, string namespaceName, DataSpace dataSpace)
        {
            return GetFunctions(name, namespaceName, dataSpace, false /*ignoreCase*/);
        }

        /// <summary>
        /// Get all the overloads of the function with the given name
        /// </summary>
        /// <param name="name">name of the function</param>
        /// <param name="namespaceName">namespace of the function</param>
        /// <param name="dataSpace">The dataspace for which we need to get the function for</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentNullException">if functionName argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">If the ItemCollection for this space does not have a EdmFunction with the given functionName</exception>
        /// <exception cref="System.ArgumentException">If the name or namespaceName is empty</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<EdmFunction> GetFunctions(string name,
                                                            string namespaceName,
                                                            DataSpace dataSpace,
                                                            bool ignoreCase)
        {
            EntityUtil.CheckStringArgument(name, "name");
            EntityUtil.CheckStringArgument(namespaceName, "namespaceName");
            ItemCollection collection = GetItemCollection(dataSpace, true);

            // Get the function with this full name, which is namespace name plus name
            return collection.GetFunctions(namespaceName + "." + name, ignoreCase);
        }

        /// <summary>
        /// Gets the function as specified by the function key.
        /// All parameters are assumed to be <see cref="ParameterMode.In"/>.
        /// </summary>
        /// <param name="name">name of the function</param>
        /// <param name="namespaceName">namespace of the function</param>
        /// <param name="parameterTypes">types of the parameters</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="dataSpace"></param>
        /// <param name="function">The function that needs to be returned</param>
        /// <returns> The function as specified in the function key or null</returns>
        /// <exception cref="System.ArgumentNullException">if name, namespaceName, parameterTypes or space argument is null</exception>
        internal bool TryGetFunction(string name,
                                     string namespaceName,
                                     TypeUsage[] parameterTypes,
                                     bool ignoreCase,
                                     DataSpace dataSpace,
                                     out EdmFunction function)
        {
            function = null;
            EntityUtil.GenericCheckArgumentNull(name, "name");
            EntityUtil.GenericCheckArgumentNull(namespaceName, "namespaceName");
            ItemCollection collection = GetItemCollection(dataSpace, false);

            // Get the function with this full name, which is namespace name plus name
            return (null != collection) && collection.TryGetFunction(namespaceName + "." + name, parameterTypes, ignoreCase, out function);
        }

        /// <summary>
        /// Get the list of primitive types for the given space
        /// </summary>
        /// <param name="dataSpace">dataspace for which you need the list of primitive types</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes(DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetItems<PrimitiveType>();
        }

        /// <summary>
        /// Get all the items in the data space
        /// </summary>
        /// <param name="dataSpace">dataspace for which you need the list of items</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        public ReadOnlyCollection<GlobalItem> GetItems(DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetItems<GlobalItem>();
        }

        /// <summary>
        /// Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind">primitive type kind</param>
        /// <param name="dataSpace">dataspace in which one needs to the mapping primitive types</param>
        /// <returns>The mapped scalar type</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        /// <exception cref="System.InvalidOperationException">If ItemCollection has not been registered for the space passed in</exception>
        /// <exception cref="System.ArgumentException">Thrown if the space is not a valid space. Valid space is either C, O, CS or OCSpace</exception>
        internal PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return collection.GetMappedPrimitiveType(primitiveTypeKind);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="typeIdentity">type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="mappingSpace">space for which you want to get the mapped type</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal bool TryGetMap(string typeIdentity, DataSpace typeSpace, bool ignoreCase, DataSpace mappingSpace, out Map map)
        {
            map = null;
            ItemCollection collection = GetItemCollection(mappingSpace, false);
            return (null != collection) && ((MappingItemCollection)collection).TryGetMap(typeIdentity, typeSpace, ignoreCase, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">typeIdentity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="dataSpace">space for which you want to get the mapped type</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal Map GetMap(string identity, DataSpace typeSpace, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return ((MappingItemCollection)collection).GetMap(identity, typeSpace);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dataSpace">space for which you want to get the mapped type</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal Map GetMap(GlobalItem item, DataSpace dataSpace)
        {
            ItemCollection collection = GetItemCollection(dataSpace, true);
            return ((MappingItemCollection)collection).GetMap(item);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dataSpace">space for which you want to get the mapped type</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal bool TryGetMap(GlobalItem item, DataSpace dataSpace, out Map map)
        {
            map = null;
            ItemCollection collection = GetItemCollection(dataSpace, false);
            return (null != collection) && ((MappingItemCollection)collection).TryGetMap(item, out map);
        }

        private ItemCollection RegisterDefaultObjectMappingItemCollection()
        {
            EdmItemCollection edm = _itemsCSpace as EdmItemCollection;
            ObjectItemCollection obj = _itemsOSpace as ObjectItemCollection;
            if ((null != edm) && (null != obj))
            {
                RegisterItemCollection(new DefaultObjectMappingItemCollection(edm, obj));
            }
            return _itemsOCSpace;
        }

        /// <summary>
        /// Get item collection for the space, if registered. If returned, the ItemCollection is in read only mode as it is
        /// part of the workspace.
        /// </summary>
        /// <param name="dataSpace">The dataspace for the item collection that should be returned</param>
        /// <param name="collection">The collection registered for the specified dataspace, if any</param>
        /// <returns><c>true</c> if an item collection is currently registered for the specified space; otherwise <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">if space argument is null</exception>
        [CLSCompliant(false)]
        public bool TryGetItemCollection(DataSpace dataSpace, out ItemCollection collection)
        {
            collection = GetItemCollection(dataSpace, false);
            return (null != collection);
        }

        /// <summary>
        /// Checks if the space is valid and whether the collection is registered for the given space, and if both are valid,
        /// then returns the itemcollection for the given space
        /// </summary>
        /// <param name="dataSpace"></param>
        /// <param name="required">if true, will throw</param>
        /// <exception cref="ArgumentException">Thrown if required and mapping space is not valid or registered</exception>
        internal ItemCollection GetItemCollection(DataSpace dataSpace, bool required)
        {
            ItemCollection collection;
            switch (dataSpace)
            {
            case DataSpace.CSpace:
                collection = _itemsCSpace;
                break;
            case DataSpace.OSpace:
                collection = _itemsOSpace;
                break;
            case DataSpace.OCSpace:
                collection = _itemsOCSpace ?? RegisterDefaultObjectMappingItemCollection();
                break;
            case DataSpace.CSSpace:
                collection = _itemsCSSpace;
                break;
            case DataSpace.SSpace:
                collection = _itemsSSpace;
                break;
            default:
                if (required) {
                    Debug.Fail("Invalid DataSpace Enum value: " + dataSpace);
                }
                collection = null;
                break;
            }
            if (required && (null == collection)) {
                throw EntityUtil.NoCollectionForSpace(dataSpace);
            }
            return collection;
        }

        /// <summary>
        /// The method returns the OSpace structural type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or the mapped OSpace type 
        /// cannot be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <returns>The OSpace type mapped to the supplied argument</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public StructuralType GetObjectSpaceType(StructuralType edmSpaceType)
        {
            return GetObjectSpaceType<StructuralType>(edmSpaceType);
        }

        /// <summary>
        /// This method returns the OSpace structural type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or if the mapped OSpace type 
        /// cannot be determined, the method returns false and sets the out parameter
        /// to null.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <param name="objectSpaceType">The OSpace type mapped to the supplied argument</param>
        /// <returns>true on success, false on failure</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public bool TryGetObjectSpaceType(StructuralType edmSpaceType, out StructuralType objectSpaceType)
        {
            return TryGetObjectSpaceType<StructuralType>(edmSpaceType, out objectSpaceType);
        }

        /// <summary>
        /// The method returns the OSpace enum type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or the mapped OSpace type 
        /// cannot be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <returns>The OSpace type mapped to the supplied argument</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public EnumType GetObjectSpaceType(EnumType edmSpaceType)
        {
            return GetObjectSpaceType<EnumType>(edmSpaceType);
        }

        /// <summary>
        /// This method returns the OSpace enum type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or if the mapped OSpace type 
        /// cannot be determined, the method returns false and sets the out parameter
        /// to null.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <param name="objectSpaceType">The OSpace type mapped to the supplied argument</param>
        /// <returns>true on success, false on failure</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public bool TryGetObjectSpaceType(EnumType edmSpaceType, out EnumType objectSpaceType)
        {
            return TryGetObjectSpaceType<EnumType>(edmSpaceType, out objectSpaceType);
        }

        /// <summary>
        /// Helper method returning the OSpace enum type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or the mapped OSpace type 
        /// cannot be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <returns>The OSpace type mapped to the supplied argument</returns>
        /// <typeparam name="T">Must be StructuralType or EnumType.</typeparam>
        private T GetObjectSpaceType<T>(T edmSpaceType)
            where T : EdmType
        {
            Debug.Assert(
                edmSpaceType == null || edmSpaceType is StructuralType || edmSpaceType is EnumType,
                "Only structural or enum type expected");

            T objectSpaceType;
            if (!this.TryGetObjectSpaceType(edmSpaceType, out objectSpaceType))
            {
                throw EntityUtil.Argument(Strings.FailedToFindOSpaceTypeMapping(edmSpaceType.Identity));
            }

            return objectSpaceType;
        }

        /// <summary>
        /// Helper method returning the OSpace structural or enum type mapped to the specified Edm Space Type.
        /// If the DataSpace of the argument is not CSpace, or if the mapped OSpace type 
        /// cannot be determined, the method returns false and sets the out parameter
        /// to null.
        /// </summary>
        /// <param name="edmSpaceType">The CSpace type to look up</param>
        /// <param name="objectSpaceType">The OSpace type mapped to the supplied argument</param>
        /// <returns>true on success, false on failure</returns>
        /// <typeparam name="T">Must be StructuralType or EnumType.</typeparam>
        private bool TryGetObjectSpaceType<T>(T edmSpaceType, out T objectSpaceType)
            where T : EdmType
        {
            Debug.Assert(
                edmSpaceType == null || edmSpaceType is StructuralType || edmSpaceType is EnumType,
                "Only structural or enum type expected");

            EntityUtil.CheckArgumentNull(edmSpaceType, "edmSpaceType");

            if (edmSpaceType.DataSpace != DataSpace.CSpace)
            {
                throw EntityUtil.Argument(Strings.ArgumentMustBeCSpaceType, "edmSpaceType");
            }

            objectSpaceType = null;

            Map map;
            if (this.TryGetMap(edmSpaceType, DataSpace.OCSpace, out map))
            {
                ObjectTypeMapping ocMap = map as ObjectTypeMapping;
                if (ocMap != null)
                {
                    objectSpaceType = (T)ocMap.ClrType;
                }
            }

            return objectSpaceType != null;
        }

        /// <summary>
        /// This method returns the Edm Space structural type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <returns>The CSpace type mapped to the OSpace parameter</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public StructuralType GetEdmSpaceType(StructuralType objectSpaceType)
        {
            return GetEdmSpaceType<StructuralType>(objectSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space structural type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, the method returns false and sets the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <param name="edmSpaceType">The mapped CSpace type</param>
        /// <returns>true on success, false on failure</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        public bool TryGetEdmSpaceType(StructuralType objectSpaceType, out StructuralType edmSpaceType)
        {
            return TryGetEdmSpaceType<StructuralType>(objectSpaceType, out edmSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space enum type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <returns>The CSpace type mapped to the OSpace parameter</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]        
        public EnumType GetEdmSpaceType(EnumType objectSpaceType)
        {
            return GetEdmSpaceType<EnumType>(objectSpaceType);
        }

        /// <summary>
        /// This method returns the Edm Space enum type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, the method returns false and sets the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <param name="edmSpaceType">The mapped CSpace type</param>
        /// <returns>true on success, false on failure</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]        
        public bool TryGetEdmSpaceType(EnumType objectSpaceType, out EnumType edmSpaceType)
        {
            return TryGetEdmSpaceType<EnumType>(objectSpaceType, out edmSpaceType);
        }

        /// <summary>
        /// Helper method returning the Edm Space structural or enum type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, an ArgumentException is thrown.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <returns>The CSpace type mapped to the OSpace parameter</returns>
        /// <typeparam name="T">Must be StructuralType or EnumType</typeparam>
        private T GetEdmSpaceType<T>(T objectSpaceType)
            where T : EdmType
        {
            Debug.Assert(
                objectSpaceType == null || objectSpaceType is StructuralType || objectSpaceType is EnumType,
                "Only structural or enum type expected");

            T edmSpaceType;
            if (!this.TryGetEdmSpaceType(objectSpaceType, out edmSpaceType))
            {
                throw EntityUtil.Argument(Strings.FailedToFindCSpaceTypeMapping(objectSpaceType.Identity));
            }

            return edmSpaceType;
        }

        /// <summary>
        /// Helper method returning the Edm Space structural or enum type mapped to the OSpace Type parameter. If the
        /// DataSpace of the supplied type is not OSpace, or the mapped Edm Space type cannot
        /// be determined, the method returns false and sets the out parameter to null.
        /// </summary>
        /// <param name="objectSpaceType">The OSpace type to look up</param>
        /// <param name="edmSpaceType">The mapped CSpace type</param>
        /// <returns>true on success, false on failure</returns>
        /// <typeparam name="T">Must be StructuralType or EnumType</typeparam>
        private bool TryGetEdmSpaceType<T>(T objectSpaceType, out T edmSpaceType)
            where T : EdmType
        {
            Debug.Assert(
                objectSpaceType == null || objectSpaceType is StructuralType || objectSpaceType is EnumType,
                "Only structural or enum type expected");

            EntityUtil.CheckArgumentNull(objectSpaceType, "objectSpaceType");

            if (objectSpaceType.DataSpace != DataSpace.OSpace)
            {
                throw EntityUtil.Argument(Strings.ArgumentMustBeOSpaceType, "objectSpaceType");
            }

            edmSpaceType = null;

            Map map;
            if (this.TryGetMap(objectSpaceType, DataSpace.OCSpace, out map))
            {
                ObjectTypeMapping ocMap = map as ObjectTypeMapping;
                if (ocMap != null)
                {
                    edmSpaceType = (T)ocMap.EdmType;
                }
            }

            return edmSpaceType != null;
        }

        ///// <summary>
        ///// Returns the update or query view for an Extent as a
        ///// command tree. For a given Extent, MetadataWorkspace will
        ///// have either a Query view or an Update view but not both.
        ///// </summary>
        ///// <param name="extent"></param>
        ///// <returns></returns>
        internal DbQueryCommandTree GetCqtView(EntitySetBase extent)
        {
            return GetGeneratedView(extent).GetCommandTree();
        }

        /// <summary>
        /// Returns generated update or query view for the given extent.
        /// </summary>
        internal GeneratedView GetGeneratedView(EntitySetBase extent)
        {
            ItemCollection collection = GetItemCollection(DataSpace.CSSpace, true);
            return ((StorageMappingItemCollection)collection).GetGeneratedView(extent, this);
        }

        /// <summary>
        /// Returns a TypeOf/TypeOfOnly Query for a given Extent and Type as a command tree.
        /// </summary>
        /// <param name="extent"></param>
        /// <returns></returns>
        internal bool TryGetGeneratedViewOfType(EntitySetBase extent, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
        {
            ItemCollection collection = GetItemCollection(DataSpace.CSSpace, true);
            return ((StorageMappingItemCollection)collection).TryGetGeneratedViewOfType(this, extent, type, includeSubtypes, out generatedView);
        }

        /// <summary>
        /// Returns generated function definition for the given function.
        /// Guarantees type match of declaration and generated parameters.
        /// Guarantees return type match.
        /// Throws internal error for functions without definition.
        /// Passes thru exception occured during definition generation.
        /// </summary>
        internal DbLambda GetGeneratedFunctionDefinition(EdmFunction function)
        {
            ItemCollection collection = GetItemCollection(DataSpace.CSpace, true);
            return ((EdmItemCollection)collection).GetGeneratedFunctionDefinition(function);
        }

        /// <summary>
        /// Determines if a target function exists for the given function import.
        /// </summary>
        /// <param name="functionImport">Function import (function declared in a model entity container)</param>
        /// <param name="targetFunctionMapping">Function target mapping (function to which the import is mapped in the target store)</param>
        /// <returns>true if a mapped target function exists; false otherwise</returns>
        internal bool TryGetFunctionImportMapping(EdmFunction functionImport, out FunctionImportMapping targetFunctionMapping)
        {
            Debug.Assert(null != functionImport);
            ReadOnlyCollection<StorageEntityContainerMapping> entityContainerMaps = this.GetItems<StorageEntityContainerMapping>(DataSpace.CSSpace);
            foreach (StorageEntityContainerMapping containerMapping in entityContainerMaps)
            {
                if (containerMapping.TryGetFunctionImportMapping(functionImport, out targetFunctionMapping))
                {
                    return true;
                }
            }
            targetFunctionMapping = null;
            return false;
        }

        /// <summary>
        /// Returns the view loader associated with this workspace,
        /// creating a loader if non exists. The loader includes
        /// context information used by the update pipeline when
        /// processing changes to C-space extents.
        /// </summary>
        /// <returns></returns>
        internal ViewLoader GetUpdateViewLoader()
        {
            if (_itemsCSSpace != null)
            {
                return _itemsCSSpace.GetUpdateViewLoader();
            }
            return null;
        }


        /// <summary>
        /// Takes in a Edm space type usage and converts into an
        /// equivalent O space type usage
        /// </summary>
        /// <param name="edmSpaceTypeUsage"></param>
        /// <returns></returns>
        internal TypeUsage GetOSpaceTypeUsage(TypeUsage edmSpaceTypeUsage)
        {
            EntityUtil.CheckArgumentNull(edmSpaceTypeUsage, "edmSpaceTypeUsage");
            Debug.Assert(edmSpaceTypeUsage.EdmType != null, "The TypeUsage object does not have an EDMType.");

            EdmType clrType = null;
            if (Helper.IsPrimitiveType(edmSpaceTypeUsage.EdmType))
            {
                ItemCollection collection = GetItemCollection(DataSpace.OSpace, true);
                clrType = collection.GetMappedPrimitiveType(((PrimitiveType)edmSpaceTypeUsage.EdmType).PrimitiveTypeKind);
            }
            else
            {
                // Check and throw if the OC space doesn't exist
                ItemCollection collection = GetItemCollection(DataSpace.OCSpace, true);

                // Get the OC map
                Map map = ((DefaultObjectMappingItemCollection)collection).GetMap(edmSpaceTypeUsage.EdmType);
                clrType = ((ObjectTypeMapping)map).ClrType;
            }

            Debug.Assert(!Helper.IsPrimitiveType(clrType) || 
                object.ReferenceEquals(ClrProviderManifest.Instance.GetFacetDescriptions(clrType),
                                                EdmProviderManifest.Instance.GetFacetDescriptions(clrType.BaseType)), 
                                                "these are no longer equal so we can't just use the same set of facets for the new type usage");
            
            // Transfer the facet values
            TypeUsage result = TypeUsage.Create(clrType, edmSpaceTypeUsage.Facets);

            return result;
        }

        /// <summary>
        /// Returns true if the item collection for the given space has already been registered else returns false
        /// </summary>
        /// <param name="dataSpace"></param>
        /// <returns></returns>
        internal bool IsItemCollectionAlreadyRegistered(DataSpace dataSpace)
        {
            ItemCollection itemCollection;
            return TryGetItemCollection(dataSpace, out itemCollection);
        }

        /// <summary>
        /// Requires: C, S and CS are registered in this and other
        /// Determines whether C, S and CS are equivalent. Useful in determining whether a DbCommandTree
        /// is usable within a particular entity connection.
        /// </summary>
        /// <param name="other">Other workspace.</param>
        /// <returns>true is C, S and CS collections are equivalent</returns>
        internal bool IsMetadataWorkspaceCSCompatible(MetadataWorkspace other)
        {
            Debug.Assert(this.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace) &&
                other.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace),
                "requires: C, S and CS are registered in this and other");

            bool result = this._itemsCSSpace.MetadataEquals(other._itemsCSSpace);

            Debug.Assert(!result ||
                (this._itemsCSpace.MetadataEquals(other._itemsCSpace) && this._itemsSSpace.MetadataEquals(other._itemsSSpace)),
                "constraint: this.CS == other.CS --> this.S == other.S && this.C == other.C");
            
            return result;
        }

        /// <summary>
        /// Clear all the metadata cache entries
        /// </summary>
        public static void ClearCache()
        {
            MetadataCache.Clear();
            ObjectItemCollection.ViewGenerationAssemblies.Clear();
            using (LockedAssemblyCache cache = AssemblyCache.AquireLockedAssemblyCache())
            {
                cache.Clear();
            }
        }

        /// <summary>
        /// Creates a new Metadata workspace sharing the (currently defined) item collections
        /// and tokens for caching purposes.
        /// </summary>
        /// <returns></returns>
        internal MetadataWorkspace ShallowCopy()
        {
            MetadataWorkspace copy = (MetadataWorkspace)MemberwiseClone();
            if (null != copy._cacheTokens) {
                copy._cacheTokens = new List<Object>(copy._cacheTokens);
            }
            return copy;
        }

        /// <summary>
        /// Returns the canonical Model TypeUsage for a given PrimitiveTypeKind
        /// </summary>
        /// <param name="primitiveTypeKind">PrimitiveTypeKind for which a canonical TypeUsage is expected</param>
        /// <returns>a canonical model TypeUsage</returns>
        internal TypeUsage GetCanonicalModelTypeUsage(PrimitiveTypeKind primitiveTypeKind)
        {
            return EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(primitiveTypeKind);
        }

        /// <summary>
        /// Returns the Model PrimitiveType for a given primitiveTypeKind
        /// </summary>
        /// <param name="primitiveTypeKind">a PrimitiveTypeKind for which a Model PrimitiveType is expected</param>
        /// <returns>Model PrimitiveType</returns>
        internal PrimitiveType GetModelPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            return EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
        }

        // GetRequiredOriginalValueMembers and GetRelevantMembersForUpdate return list of "interesting" members for the given EntitySet/EntityType
        // Interesting Members are a subset of the following:
        //    0. Key members
        //    1. Members with C-Side conditions (complex types can not have C-side condition at present)
        //    2. Members participating in association end
        //    3. Members with ConcurrencyMode 'Fixed'
        //      3.1 Complex Members with any child member having Concurrency mode Fixed
        //    4. Members included in Update ModificationFunction with Version='Original' (Original = Not Current)
        //      4.1 Complex Members in ModificationFunction if any sub-member is interesting
        //    5. Members included in Update ModificationFunction (mutually exclusive with 4 - required for partial update scenarios)
        //    6. Foreign keys
        //    7. All complex members - partial update scenarios only
        /// <summary>
        /// Returns members of a given EntitySet/EntityType for which original values are necessary for determining which tables to modify.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet</param>
        /// <returns>Edm Members for which original Value is required</returns>
        /// <remarks>
        /// This method returns the following groups of members: 0, 1, 2, 3, 3.1, 4, 4.1. (see group descriptions above). 
        /// This method is marked as obsolete since it does not support partial update scenarios as it does not return 
        /// members from group 5 and changing it to return these members would be a breaking change.
        /// </remarks>
        [Obsolete("Use MetadataWorkspace.GetRelevantMembersForUpdate(EntitySetBase, EntityTypeBase, bool) instead")]
        public IEnumerable<EdmMember> GetRequiredOriginalValueMembers(EntitySetBase entitySet, EntityTypeBase entityType)
        {
            return GetInterestingMembers(entitySet, entityType, StorageMappingItemCollection.InterestingMembersKind.RequiredOriginalValueMembers);
        }

        /// <summary>
        /// Returns members of a given EntitySet/EntityType for which original values are needed when modifying an entity.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet</param>
        /// <param name="partialUpdateSupported">Whether entities may be updated partially.</param>
        /// <returns>Edm Members for which original Value is required</returns>
        /// <remarks>
        /// This method returns the following groups of members:
        /// - if <paramref name="partialUpdateSupported"/> is <c>false</c>: 1, 2, 3, 3.1, 4, 4.1, 6 (see group descriptions above)
        /// - if <paramref name="partialUpdateSupported"/> is <c>true</c>: 1, 2, 3, 3.1, 5, 6, 7 (see group descriptions above)
        /// See DevDiv bugs #124460 and #272992 for more details.
        /// </remarks>
        public ReadOnlyCollection<EdmMember> GetRelevantMembersForUpdate(EntitySetBase entitySet, EntityTypeBase entityType, bool partialUpdateSupported)
        {
            return GetInterestingMembers(
                entitySet, 
                entityType, 
                partialUpdateSupported ? 
                    StorageMappingItemCollection.InterestingMembersKind.PartialUpdate : 
                    StorageMappingItemCollection.InterestingMembersKind.FullUpdate);
        }

        /// <summary>
        /// Return members for <see cref="GetRequiredOriginalValueMembers"/> and <see cref="GetRelevantMembersForUpdate"/> methods.
        /// </summary>
        /// <param name="entitySet">An EntitySet belonging to the C-Space</param>
        /// <param name="entityType">An EntityType that participates in the given EntitySet</param>
        /// <param name="interestingMembersKind">Scenario the members should be returned for.</param>
        /// <returns>ReadOnlyCollection of interesting members for the requested scenario (<paramref name="interestingMembersKind"/>).</returns>
        private ReadOnlyCollection<EdmMember> GetInterestingMembers(EntitySetBase entitySet, EntityTypeBase entityType, StorageMappingItemCollection.InterestingMembersKind interestingMembersKind)
        {
            EntityUtil.CheckArgumentNull<EntitySetBase>(entitySet, "entitySet");
            EntityUtil.CheckArgumentNull<EntityTypeBase>(entityType, "entityType");

            Debug.Assert(entitySet.EntityContainer != null);

            //Check that EntitySet is from CSpace
            if (entitySet.EntityContainer.DataSpace != DataSpace.CSpace)
            {
                AssociationSet associationSet = entitySet as AssociationSet;
                if (associationSet != null)
                {
                    throw EntityUtil.AssociationSetNotInCSpace(entitySet.Name);
                }
                else
                {
                    throw EntityUtil.EntitySetNotInCSpace(entitySet.Name);
                }
            }

            //Check that entityType belongs to entitySet
            if (!entitySet.ElementType.IsAssignableFrom(entityType))
            {
                AssociationSet associationSet = entitySet as AssociationSet;
                if (associationSet != null)
                {
                    throw EntityUtil.TypeNotInAssociationSet(entitySet.Name, entitySet.ElementType.FullName, entityType.FullName);
                }
                else
                {
                    throw EntityUtil.TypeNotInEntitySet(entitySet.Name, entitySet.ElementType.FullName, entityType.FullName);
                }
            }

            var mappingCollection = (StorageMappingItemCollection)GetItemCollection(DataSpace.CSSpace, true);
            return mappingCollection.GetInterestingMembers(entitySet, entityType, interestingMembersKind);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Returns the QueryCacheManager hosted by this metadata workspace instance
        /// </summary>
        internal System.Data.Common.QueryCache.QueryCacheManager GetQueryCacheManager()
        {
            Debug.Assert(null != _itemsSSpace, "_itemsSSpace must not be null");
            return _itemsSSpace.QueryCacheManager;
        }

        internal Guid MetadataWorkspaceId
        {
            get
            {
                if (Guid.Equals(Guid.Empty, _metadataWorkspaceId))
                {
                    _metadataWorkspaceId = Guid.NewGuid();
                }
                return _metadataWorkspaceId;
            }
        }
        #endregion
    }
}
