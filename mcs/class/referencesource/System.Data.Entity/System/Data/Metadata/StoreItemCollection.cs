//---------------------------------------------------------------------
// <copyright file="StoreItemCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    /// Class for representing a collection of items in Store space.
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class StoreItemCollection : ItemCollection
    {
        #region Fields

        double _schemaVersion = XmlConstants.UndefinedVersion;

        // Cache for primitive type maps for Edm to provider
        private readonly CacheForPrimitiveTypes _primitiveTypeMaps = new CacheForPrimitiveTypes();
        private readonly Memoizer<EdmFunction, EdmFunction> _cachedCTypeFunction;

        private readonly DbProviderManifest _providerManifest;
        private readonly string _providerManifestToken;
        private readonly DbProviderFactory _providerFactory;
        
        // Storing the query cache manager in the store item collection since all queries are currently bound to the
        // store. So storing it in StoreItemCollection makes sense. Also, since query cache requires version and other
        // stuff of the provider, we can assume that the connection is always open and we have the store metadata.
        // Also we can use the same cache manager both for Entity Client and Object Query, since query cache has
        // no reference to any metadata in OSpace. Also we assume that ObjectMaterializer loads the assembly
        // before it tries to do object materialization, since we might not have loaded an assembly in another workspace
        // where this store item collection is getting reused
        private readonly System.Data.Common.QueryCache.QueryCacheManager _queryCacheManager = System.Data.Common.QueryCache.QueryCacheManager.Create();
        #endregion

        #region Constructors

        // used by EntityStoreSchemaGenerator to start with an empty (primitive types only) StoreItemCollection and 
        // add types discovered from the database
        internal StoreItemCollection(DbProviderFactory factory, DbProviderManifest manifest, string providerManifestToken)
            : base(DataSpace.SSpace)
        {
            Debug.Assert(factory != null, "factory is null");
            Debug.Assert(manifest != null, "manifest is null");

            _providerFactory = factory;
            _providerManifest = manifest;
            _providerManifestToken = providerManifestToken;
            _cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);
            LoadProviderManifest(_providerManifest, true /*checkForSystemNamespace*/);
        }

        /// <summary>
        /// constructor that loads the metadata files from the specified xmlReaders, and returns the list of errors
        /// encountered during load as the out parameter errors.
        /// 
        /// Publicly available from System.Data.Entity.Desgin.dll
        /// </summary>
        /// <param name="xmlReaders">xmlReaders where the CDM schemas are loaded</param>
        /// <param name="filePaths">the paths where the files can be found that match the xml readers collection</param>
        /// <param name="errors">An out parameter to return the collection of errors encountered while loading</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced by System.Data.Entity.Design.dll
        internal StoreItemCollection(IEnumerable<XmlReader> xmlReaders,
                                     System.Collections.ObjectModel.ReadOnlyCollection<string> filePaths,
                                     out IList<EdmSchemaError> errors)
            : base(DataSpace.SSpace)
        {
            // we will check the parameters for this internal ctor becuase
            // it is pretty much publicly exposed through the MetadataItemCollectionFactory
            // in System.Data.Entity.Design
            EntityUtil.CheckArgumentNull(xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");
            // filePaths is allowed to be null

            errors = this.Init(xmlReaders, filePaths, false,
                out _providerManifest,
                out _providerFactory,
                out _providerManifestToken,
                out _cachedCTypeFunction);
        }

        /// <summary>
        /// constructor that loads the metadata files from the specified xmlReaders, and returns the list of errors
        /// encountered during load as the out parameter errors.
        /// 
        /// Publicly available from System.Data.Entity.Desgin.dll
        /// </summary>
        /// <param name="xmlReaders">xmlReaders where the CDM schemas are loaded</param>
        /// <param name="filePaths">the paths where the files can be found that match the xml readers collection</param>
        internal StoreItemCollection(IEnumerable<XmlReader> xmlReaders,
                                     IEnumerable<string> filePaths)
            : base(DataSpace.SSpace)
        {
            EntityUtil.CheckArgumentNull(filePaths, "filePaths");
            EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");

            this.Init(xmlReaders, filePaths, true,
                out _providerManifest,
                out _providerFactory,
                out _providerManifestToken,
                out _cachedCTypeFunction);
        }

        /// <summary>
        /// Public constructor that loads the metadata files from the specified xmlReaders.
        /// Throws when encounter errors.
        /// </summary>
        /// <param name="xmlReaders">xmlReaders where the CDM schemas are loaded</param>
        public StoreItemCollection(IEnumerable<XmlReader> xmlReaders)
            : base(DataSpace.SSpace)
        {
            EntityUtil.CheckArgumentNull(xmlReaders, "xmlReaders");
            EntityUtil.CheckArgumentEmpty(ref xmlReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "xmlReader");

            MetadataArtifactLoader composite = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);
            this.Init(composite.GetReaders(),
                      composite.GetPaths(), true,
                out _providerManifest,
                out _providerFactory,
                out _providerManifestToken,
                out _cachedCTypeFunction);

        }

        /// <summary>
        /// Constructs the new instance of StoreItemCollection
        /// with the list of CDM files provided.
        /// </summary>
        /// <param name="filePaths">paths where the CDM schemas are loaded</param>
        /// <exception cref="ArgumentException"> Thrown if path name is not valid</exception>
        /// <exception cref="System.ArgumentNullException">thrown if paths argument is null</exception>
        /// <exception cref="System.Data.MetadataException">For errors related to invalid schemas.</exception>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path names which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For MetadataArtifactLoader.CreateCompositeFromFilePaths method call but we do not create the file paths in this method 
        public StoreItemCollection(params string[] filePaths)
            : base(DataSpace.SSpace)
        {
            EntityUtil.CheckArgumentNull(filePaths, "filePaths");
            IEnumerable<string> enumerableFilePaths = filePaths;
            EntityUtil.CheckArgumentEmpty(ref enumerableFilePaths, Strings.StoreItemCollectionMustHaveOneArtifact, "filePaths");

            // Wrap the file paths in instances of the MetadataArtifactLoader class, which provides
            // an abstraction and a uniform interface over a diverse set of metadata artifacts.
            //
            MetadataArtifactLoader composite = null;
            List<XmlReader> readers = null;
            try
            {
                composite = MetadataArtifactLoader.CreateCompositeFromFilePaths(enumerableFilePaths, XmlConstants.SSpaceSchemaExtension);
                readers = composite.CreateReaders(DataSpace.SSpace);
                IEnumerable<XmlReader> ieReaders = readers.AsEnumerable();
                EntityUtil.CheckArgumentEmpty(ref ieReaders, Strings.StoreItemCollectionMustHaveOneArtifact, "filePaths");

                this.Init(readers, 
                          composite.GetPaths(DataSpace.SSpace), true,
                    out _providerManifest,
                    out _providerFactory,
                    out _providerManifestToken,
                    out _cachedCTypeFunction);
            }
            finally
            {
                if (readers != null)
                {
                    Helper.DisposeXmlReaders(readers);
                }
            }
        }


        

        private IList<EdmSchemaError> Init(IEnumerable<XmlReader> xmlReaders,
                                           IEnumerable<string> filePaths, bool throwOnError,
                                           out DbProviderManifest providerManifest,
                                           out DbProviderFactory providerFactory,
                                           out string providerManifestToken,
                                           out Memoizer<EdmFunction, EdmFunction> cachedCTypeFunction)
        {
            EntityUtil.CheckArgumentNull(xmlReaders, "xmlReaders");
            // 'filePaths' can be null

            cachedCTypeFunction = new Memoizer<EdmFunction, EdmFunction>(ConvertFunctionSignatureToCType, null);

            Loader loader = new Loader(xmlReaders, filePaths, throwOnError);
            providerFactory = loader.ProviderFactory;
            providerManifest = loader.ProviderManifest;
            providerManifestToken = loader.ProviderManifestToken;

            // load the items into the colleciton
            if (!loader.HasNonWarningErrors)
            {
                LoadProviderManifest(loader.ProviderManifest, true /* check for system namespace */);
                List<EdmSchemaError> errorList = EdmItemCollection.LoadItems(_providerManifest, loader.Schemas, this);
                foreach (var error in errorList)
                {
                    loader.Errors.Add(error);
                }
                
                if (throwOnError && errorList.Count != 0)
                    loader.ThrowOnNonWarningErrors();
            }
            
            return loader.Errors;
        }
                
        #endregion

        #region Properties
        /// <summary>
        /// Returns the query cache manager
        /// </summary>
        internal System.Data.Common.QueryCache.QueryCacheManager QueryCacheManager
        {
            get { return _queryCacheManager; }
        }

        internal DbProviderFactory StoreProviderFactory
        {
            get
            {
                return _providerFactory;
            }
        }

        internal DbProviderManifest StoreProviderManifest
        {
            get
            {
                return _providerManifest;
            }
        }

        internal string StoreProviderManifestToken
        {
            get
            {
                return _providerManifestToken;
            }
        }

        /// <summary>
        /// Version of this StoreItemCollection represents.
        /// </summary>
        public Double StoreSchemaVersion
        {
            get
            {
                return _schemaVersion;
            }
            internal set
            {
                _schemaVersion = value;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Get the list of primitive types for the given space
        /// </summary>
        /// <returns></returns>
        public System.Collections.ObjectModel.ReadOnlyCollection<PrimitiveType> GetPrimitiveTypes()
        {
            return _primitiveTypeMaps.GetTypes();
        }

        /// <summary>
        /// Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind">canonical primitive type</param>
        /// <returns>The mapped scalar type</returns>
        internal override PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            PrimitiveType type = null;
            _primitiveTypeMaps.TryGetType(primitiveTypeKind, null, out type);
            return type;
        }
               
        /// <summary>
        /// checks if the schemaKey refers to the provider manifest schema key 
        /// and if true, loads the provider manifest
        /// </summary>
        /// <param name="connection">The connection where the store manifest is loaded from</param>
        /// <param name="checkForSystemNamespace">Check for System namespace</param>
        /// <returns>The provider manifest object that was loaded</returns>
        private void LoadProviderManifest(DbProviderManifest storeManifest,
                                                      bool checkForSystemNamespace)
        {

            foreach (PrimitiveType primitiveType in storeManifest.GetStoreTypes())
            {
                //Add it to the collection and the primitive type maps
                this.AddInternal(primitiveType);
                _primitiveTypeMaps.Add(primitiveType);
            }

            foreach (EdmFunction function in storeManifest.GetStoreFunctions())
            {
                AddInternal(function);
            }
        }

        #endregion

        /// <summary>
        /// Get all the overloads of the function with the given name, this method is used for internal perspective
        /// </summary>
        /// <param name="functionName">The full name of the function</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if functionaName argument passed in is null</exception>
        internal System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetCTypeFunctions(string functionName, bool ignoreCase)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> functionOverloads;

            if (this.FunctionLookUpTable.TryGetValue(functionName, out functionOverloads))
            {
                functionOverloads = ConvertToCTypeFunctions(functionOverloads);
                if (ignoreCase)
                {
                    return functionOverloads;
                }

                return GetCaseSensitiveFunctions(functionOverloads, functionName);
            }

            return Helper.EmptyEdmFunctionReadOnlyCollection;
        }

        private System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> ConvertToCTypeFunctions(
            System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> functionOverloads)
        {
            List<EdmFunction> cTypeFunctions = new List<EdmFunction>();
            foreach (var sTypeFunction in functionOverloads)
            {
                cTypeFunctions.Add(ConvertToCTypeFunction(sTypeFunction));
            }
            return cTypeFunctions.AsReadOnly();
        }

        internal EdmFunction ConvertToCTypeFunction(EdmFunction sTypeFunction)
        {
            return this._cachedCTypeFunction.Evaluate(sTypeFunction);
        }

        /// <summary>
        /// Convert the S type function parameters and returnType to C types.
        /// </summary>
        private EdmFunction ConvertFunctionSignatureToCType(EdmFunction sTypeFunction)
        {
            Debug.Assert(sTypeFunction.DataSpace == Edm.DataSpace.SSpace, "sTypeFunction.DataSpace == Edm.DataSpace.SSpace");

            if (sTypeFunction.IsFromProviderManifest)
            {
                return sTypeFunction;
            }
            
            FunctionParameter returnParameter = null;
            if (sTypeFunction.ReturnParameter != null)
            {
                TypeUsage edmTypeUsageReturnParameter =
                    MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(sTypeFunction.ReturnParameter.TypeUsage);

                returnParameter =
                    new FunctionParameter(
                        sTypeFunction.ReturnParameter.Name,
                        edmTypeUsageReturnParameter,
                        sTypeFunction.ReturnParameter.GetParameterMode());
            }

            List<FunctionParameter> parameters = new List<FunctionParameter>();
            if (sTypeFunction.Parameters.Count > 0)
            {
                
                foreach (var parameter in sTypeFunction.Parameters)
                {
                    TypeUsage edmTypeUsage = MetadataHelper.ConvertStoreTypeUsageToEdmTypeUsage(parameter.TypeUsage);

                    FunctionParameter edmTypeParameter = new FunctionParameter(parameter.Name, edmTypeUsage, parameter.GetParameterMode());
                    parameters.Add(edmTypeParameter);
                }
            }

            FunctionParameter[] returnParameters = 
                returnParameter == null ? new FunctionParameter[0] : new FunctionParameter[] { returnParameter };
            EdmFunction edmFunction = new EdmFunction(sTypeFunction.Name, 
                sTypeFunction.NamespaceName,
                DataSpace.CSpace,
                new EdmFunctionPayload
                {
                    Schema = sTypeFunction.Schema,
                    StoreFunctionName = sTypeFunction.StoreFunctionNameAttribute,
                    CommandText = sTypeFunction.CommandTextAttribute,
                    IsAggregate = sTypeFunction.AggregateAttribute,
                    IsBuiltIn = sTypeFunction.BuiltInAttribute,
                    IsNiladic = sTypeFunction.NiladicFunctionAttribute,
                    IsComposable = sTypeFunction.IsComposableAttribute,
                    IsFromProviderManifest = sTypeFunction.IsFromProviderManifest,
                    IsCachedStoreFunction = true,
                    IsFunctionImport = sTypeFunction.IsFunctionImport,
                    ReturnParameters = returnParameters,
                    Parameters = parameters.ToArray(),
                    ParameterTypeSemantics = sTypeFunction.ParameterTypeSemanticsAttribute,
                });

            edmFunction.SetReadOnly();

            return edmFunction;
        }

    }//---- ItemCollection

}//---- 
