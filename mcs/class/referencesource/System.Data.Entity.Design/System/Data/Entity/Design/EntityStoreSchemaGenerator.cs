//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaGenerator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using SOM=System.Data.EntityModel;
using System.Globalization;
using System.Data.Metadata.Edm;
using System.Data.Entity.Design.Common;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Common;
using System.Data.EntityClient;
using System.IO;
using System.Data.Mapping;
using System.Data.Common.Utils;
using System.Collections.ObjectModel;
using System.Data.Common.CommandTrees;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Text;
using System.Linq;
using Microsoft.Build.Utilities;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// Responsible for Loading Database Schema Information
    /// </summary>
    public sealed partial class EntityStoreSchemaGenerator 
    {
        private const string CONTAINER_SUFFIX = "Container";
        private EntityStoreSchemaGeneratorDatabaseSchemaLoader _loader;
        private readonly string _provider;
        private string _providerManifestToken = string.Empty;
        private EntityContainer _entityContainer = null;
        private StoreItemCollection _storeItemCollection;
        private string _namespaceName;
        private MetadataItemSerializer.ErrorsLookup _errorsLookup;
        private List<EdmType> _invalidTypes;
        private Version _targetEntityFrameworkVersion;

        /// <summary>
        /// Creates a new EntityStoreGenerator
        /// </summary>
        /// <param name="providerInvariantName">The name of the provider to use to load the schema information.</param>
        /// <param name="connectionString">A connection string to the DB that should be loaded from.</param>
        /// <param name="namespaceName">The namespace name to use for the store metadata that is generated.</param>
        public EntityStoreSchemaGenerator(string providerInvariantName, string connectionString, string namespaceName)
        {
            EDesignUtil.CheckStringArgument(providerInvariantName, "providerInvariantName");
            EDesignUtil.CheckArgumentNull(connectionString, "connectionString"); // check for NULL string and support empty connection string
            EDesignUtil.CheckStringArgument(namespaceName, "namespaceName");

            _namespaceName = namespaceName;

            _provider = providerInvariantName;
            _loader = new EntityStoreSchemaGeneratorDatabaseSchemaLoader(providerInvariantName, connectionString);
        }

        /// <summary>
        /// Gets the EntityContainer that was created
        /// </summary>
        public EntityContainer EntityContainer
        {
            get
            {
                return _entityContainer;
            }
        }

        /// <summary>
        /// Gets the StoreItemCollection that was created
        /// </summary>
        [CLSCompliant(false)]
        public StoreItemCollection StoreItemCollection
        {
            get
            {
                return _storeItemCollection;
            }
        }

        /// <summary>
        /// Indicates whether the given storage model will be used to produce an entity model with foreign keys.
        /// </summary>
        public bool GenerateForeignKeyProperties
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a Metadata schema from the DbSchemaLoader that was passed in
        /// </summary>
        /// <returns>The new metadata for the schema that was loaded</returns>
        public IList<EdmSchemaError> GenerateStoreMetadata()
        {
            List<EntityStoreSchemaFilterEntry> filters = new List<EntityStoreSchemaFilterEntry>();
            return DoGenerateStoreMetadata(filters, EntityFrameworkVersions.Latest);
        }

        /// <summary>
        /// Creates a Metadata schema from the DbSchemaLoader that was passed in
        /// </summary>
        /// <param name="filters">The filters to be applied during generation.</param>
        /// <returns>The new metadata for the schema that was loaded</returns>
        public IList<EdmSchemaError> GenerateStoreMetadata(IEnumerable<EntityStoreSchemaFilterEntry> filters)
        {
            EDesignUtil.CheckArgumentNull(filters, "filters");
            return DoGenerateStoreMetadata(filters, EntityFrameworkVersions.Latest);
        }

        /// <summary>
        /// Creates a Metadata schema from the DbSchemaLoader that was passed in
        /// </summary>
        /// <param name="filters">The filters to be applied during generation.</param>
        /// <param name="targetFrameworkMoniker">The filters to be applied during generation.</param>
        /// <returns>The new metadata for the schema that was loaded</returns>
        public IList<EdmSchemaError> GenerateStoreMetadata(IEnumerable<EntityStoreSchemaFilterEntry> filters, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckArgumentNull(filters, "filters");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");


            // we are not going to actually use targetFrameworkMoniker at this time, but
            // we want the option to use it in the future if we change the 
            // the ssdl schema
            return DoGenerateStoreMetadata(filters, targetEntityFrameworkVersion);
        }

        private IList<EdmSchemaError> DoGenerateStoreMetadata(IEnumerable<EntityStoreSchemaFilterEntry> filters, Version targetEntityFrameworkVersion)
        {
            if (_entityContainer != null)
            {
                _entityContainer = null;
                _storeItemCollection = null;
                _errorsLookup = null;
                _invalidTypes = null;
            }

            _targetEntityFrameworkVersion = targetEntityFrameworkVersion;
            LoadMethodSessionState session = new LoadMethodSessionState(targetEntityFrameworkVersion);
            try
            {
                _loader.Open();

                DbConnection connection = _loader.InnerConnection;
                DbProviderFactory providerFactory = DbProviderServices.GetProviderFactory(_loader.ProviderInvariantName);
                DbProviderServices providerServices = DbProviderServices.GetProviderServices(providerFactory);
                _providerManifestToken = providerServices.GetProviderManifestToken(connection);
                DbProviderManifest storeManifest = providerServices.GetProviderManifest(_providerManifestToken);
                
                session.Filters = filters;
                Debug.Assert(_namespaceName != null, "_namespaceName should not be null at this point, did you add a new ctor?");

                session.ItemCollection = new StoreItemCollection(providerFactory, providerServices.GetProviderManifest(_providerManifestToken), _providerManifestToken);

                CreateTableEntityTypes(session);
                CreateViewEntityTypes(session);
                string entityContainerName = this._namespaceName.Replace(".", string.Empty) + CONTAINER_SUFFIX;

                Debug.Assert(entityContainerName != null, "We should always have a container name");
                EntityContainer entityContainer = new EntityContainer(entityContainerName, DataSpace.SSpace);

                foreach (EntityType type in session.GetAllEntities())
                {
                    Debug.Assert(type.KeyMembers.Count > 0, "Why do we have Entities without keys in our valid Entities collection");
                    session.ItemCollection.AddInternal(type);
                    EntitySet entitySet = CreateEntitySet(session, type);
                    session.EntityTypeToSet.Add(type, entitySet);
                    entityContainer.AddEntitySetBase(entitySet);
                }

                CreateAssociationTypes(session);
                foreach (AssociationType type in session.AssociationTypes)
                {
                    session.ItemCollection.AddInternal(type);
                    AssociationSet set = CreateAssociationSet(session, type);
                    entityContainer.AddEntitySetBase(set);
                }

                entityContainer.SetReadOnly();
                session.ItemCollection.AddInternal(entityContainer);
                FixupKeylessEntitySets(entityContainer, session);

                if (_targetEntityFrameworkVersion >= EntityFrameworkVersions.Version3 &&
                    _loader.StoreSchemaModelVersion >= EntityFrameworkVersions.Version3)
                {
                    CreateTvfReturnRowTypes(session);
                }
                CreateEdmFunctions(session);
                foreach (EdmFunction function in session.Functions)
                {
                    session.ItemCollection.AddInternal(function);
                }

                if (!HasErrorSeverityErrors(session.Errors))
                {
                    _entityContainer = entityContainer;
                    _storeItemCollection = session.ItemCollection;
                    _errorsLookup = session.ItemToErrorsMap;
                    _invalidTypes = new List<EdmType>(session.InvalidTypes);
                }
            }
            catch (Exception e)
            {
                if (MetadataUtil.IsCatchableExceptionType(e))
                {
                    string message = EDesignUtil.GetMessagesFromEntireExceptionChain(e);
                    session.AddErrorsForType(null,
                        new EdmSchemaError(message,
                                    (int)ModelBuilderErrorCode.UnknownError,
                                    EdmSchemaErrorSeverity.Error,
                                    e));
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                _loader.Close();
            }

            return new List<EdmSchemaError>(session.Errors);
        }

        /// <summary>
        /// Writes the Schema to xml
        /// </summary>
        /// <param name="outputFileName">The name of the file to write the xml to.</param>
        public void WriteStoreSchema(string outputFileName)
        {
            EDesignUtil.CheckStringArgument(outputFileName, "outputFileName");
            CheckValidItemCollection();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(outputFileName, settings))
            {
                WriteStoreSchema(writer);
            }
        }

        /// <summary>
        /// Writes the Schema to xml.
        /// </summary>
        /// <param name="writer">The XmlWriter to write the xml to.</param>
        public void WriteStoreSchema(XmlWriter writer)
        {
            EDesignUtil.CheckArgumentNull(writer, "writer");
            CheckValidItemCollection();
            
            // we are going to add this EntityStoreSchemaGenerator namespace at the top of 
            // the file so that when we mark the entitysets with where they came from 
            // we don't have to repeat the namespace on each node.  The VS tools use 
            // the source information to give better messages when refreshing the .ssdl from the db
            // e.g.
            // <Schema xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator ...>
            //   <EntityContainer ...>
            //      <!-- Views is the name of the StoreInformation EntitySet that this EntitySet was created from -->
            //      <EntitySet ... store:SchemaInformationSource="Views" /> 
            //   </EntityContainer>
            //   ...
            // </Schema>
            var xmlPrefixToNamespace = new KeyValuePair<string, string>("store", DesignXmlConstants.EntityStoreSchemaGeneratorNamespace);
            MetadataItemSerializer.WriteXml(writer, StoreItemCollection, _namespaceName, _errorsLookup, _invalidTypes, _provider, _providerManifestToken, _targetEntityFrameworkVersion, xmlPrefixToNamespace);
        }

        /// <summary>
        /// Creates an EntityConnection loaded with the providers metadata for the store schema.
        /// Store schema model is the one used in <see cref="EntityFrameworkVersions.Version2"/>.
        /// </summary>
        /// <param name="providerInvariantName">The provider invariant name.</param>
        /// <param name="connectionString">The connection for the providers connection.</param>
        /// <returns>An EntityConnection that can query the ConceptualSchemaDefinition for the provider.</returns>
        public static EntityConnection CreateStoreSchemaConnection(string providerInvariantName, string connectionString)
        {
            return CreateStoreSchemaConnection(providerInvariantName, connectionString, EntityFrameworkVersions.Version2);
        }

        /// <summary>
        /// Creates an EntityConnection loaded with the providers metadata for the store schema.
        /// Note that the targetEntityFrameworkVersion parameter uses internal EntityFramework version numbers as
        /// described in the <see cref="EntityFrameworkVersions"/> class.
        /// </summary>
        /// <param name="providerInvariantName">The provider invariant name.</param>
        /// <param name="connectionString">The connection for the providers connection.</param>
        /// <param name="targetEntityFrameworkVersion">The internal Entity Framework version that is being targeted.</param>
        /// <returns>An EntityConnection that can query the ConceptualSchemaDefinition for the provider.</returns>
        public static EntityConnection CreateStoreSchemaConnection(string providerInvariantName, string connectionString, Version targetEntityFrameworkVersion)
        {
            EDesignUtil.CheckArgumentNull(providerInvariantName, "providerInvariantName");
            EDesignUtil.CheckArgumentNull(connectionString, "connectionString");
            EDesignUtil.CheckTargetEntityFrameworkVersionArgument(targetEntityFrameworkVersion, "targetEntityFrameworkVersion");

            DbProviderFactory factory;
            try
            {
                factory = DbProviderFactories.GetFactory(providerInvariantName);
            }
            catch (ArgumentException e)
            {
                throw EDesignUtil.Argument(Strings.EntityClient_InvalidStoreProvider(providerInvariantName), e);
            }

            DbProviderServices providerServices = MetadataUtil.GetProviderServices(factory);

            DbConnection providerConnection = factory.CreateConnection();
            if (providerConnection == null)
            {
                throw EDesignUtil.ProviderIncompatible(Strings.ProviderFactoryReturnedNullFactory(providerInvariantName));
            }
            providerConnection.ConnectionString = connectionString;

            MetadataWorkspace workspace = GetProviderSchemaMetadataWorkspace(providerServices, providerConnection, targetEntityFrameworkVersion);

            // create the connection with the information we have
            return new EntityConnection(workspace, providerConnection);
        }

        private static MetadataWorkspace GetProviderSchemaMetadataWorkspace(DbProviderServices providerServices, DbConnection providerConnection, Version targetEntityFrameworkVersion)
        {
            XmlReader csdl = null;
            XmlReader ssdl = null;
            XmlReader msl = null;

            Debug.Assert(EntityFrameworkVersions.IsValidVersion(targetEntityFrameworkVersion), "EntityFrameworkVersions.IsValidVersion(targetEntityFrameworkVersion)");
            string csdlName;
            string ssdlName;
            string mslName;
            if (targetEntityFrameworkVersion >= EntityFrameworkVersions.Version3)
            {
                csdlName = DbProviderManifest.ConceptualSchemaDefinitionVersion3;
                ssdlName = DbProviderManifest.StoreSchemaDefinitionVersion3;
                mslName = DbProviderManifest.StoreSchemaMappingVersion3;
            }
            else
            {
                csdlName = DbProviderManifest.ConceptualSchemaDefinition;
                ssdlName = DbProviderManifest.StoreSchemaDefinition;
                mslName = DbProviderManifest.StoreSchemaMapping;
            }

            try
            {
                // create the metadata workspace
                MetadataWorkspace workspace = new MetadataWorkspace();

                string manifestToken = providerServices.GetProviderManifestToken(providerConnection);
                DbProviderManifest providerManifest = providerServices.GetProviderManifest(manifestToken);

                // create the EdmItemCollection
                IList<EdmSchemaError> errors;
                ssdl = providerManifest.GetInformation(ssdlName);
                string location = Strings.DbProviderServicesInformationLocationPath(providerConnection.GetType().Name, ssdlName);
                List<string> ssdlLocations = new List<string>(1);
                ssdlLocations.Add(location);
                StoreItemCollection storeItemCollection = new StoreItemCollection(new XmlReader[] { ssdl }, ssdlLocations.AsReadOnly(), out errors);
                ThrowOnError(errors);
                workspace.RegisterItemCollection(storeItemCollection);

                csdl = DbProviderServices.GetConceptualSchemaDefinition(csdlName);
                location = Strings.DbProviderServicesInformationLocationPath(typeof(DbProviderServices).Name, csdlName);
                List<string> csdlLocations = new List<string>(1);
                csdlLocations.Add(location);
                EdmItemCollection edmItemCollection = new EdmItemCollection(new XmlReader[] { csdl }, csdlLocations.AsReadOnly(), out errors);
                ThrowOnError(errors);
                workspace.RegisterItemCollection(edmItemCollection);

                msl = providerManifest.GetInformation(mslName);
                location = Strings.DbProviderServicesInformationLocationPath(providerConnection.GetType().Name, DbProviderManifest.StoreSchemaMapping);
                List<string> mslLocations = new List<string>(1);
                mslLocations.Add(location);
                StorageMappingItemCollection mappingItemCollection = new StorageMappingItemCollection(edmItemCollection,
                                                                         storeItemCollection,
                                                                         new XmlReader[] { msl },
                                                                         mslLocations,
                                                                         out errors);
                ThrowOnError(errors);
                workspace.RegisterItemCollection(mappingItemCollection);

                // make the views generate here so we can wrap the provider schema problems
                // in a ProviderIncompatibleException
                ForceViewGeneration(workspace);
                return workspace;
            }
            catch (ProviderIncompatibleException)
            {
                // we don't really want to catch this one, just rethrow it
                throw;
            }
            catch (Exception e)
            {
                if (MetadataUtil.IsCatchableExceptionType(e))
                {
                    throw EDesignUtil.ProviderIncompatible(Strings.ProviderSchemaErrors, e);
                }

                throw;
            }
            finally
            {
                if (csdl != null) ((IDisposable)csdl).Dispose();
                if (ssdl != null) ((IDisposable)ssdl).Dispose();
                if (msl != null) ((IDisposable)msl).Dispose();
            }
        }

        private static void ForceViewGeneration(MetadataWorkspace workspace)
        {
            ReadOnlyCollection<EntityContainer> containers = workspace.GetItems<EntityContainer>(DataSpace.SSpace);
            Debug.Assert(containers.Count != 0, "no s space containers found");
            Debug.Assert(containers[0].BaseEntitySets.Count != 0, "no entity sets in the sspace container");
            workspace.GetCqtView(containers[0].BaseEntitySets[0]);
        }

        private static void ThrowOnError(IList<EdmSchemaError> errors)
        {
            if (errors.Count != 0)
            {
                if (!MetadataUtil.CheckIfAllErrorsAreWarnings(errors))
                {
                    throw EDesignUtil.ProviderIncompatible(Strings.ProviderSchemaErrors, EntityUtil.InvalidSchemaEncountered(MetadataUtil.CombineErrorMessage(errors)));
                }
            }
        }

        private void CheckValidItemCollection()
        {
            if (_entityContainer == null)
            {
                throw EDesignUtil.EntityStoreGeneratorSchemaNotLoaded();
            }
        }

        internal static bool HasErrorSeverityErrors(IEnumerable<EdmSchemaError> errors)
        {
            foreach (EdmSchemaError error in errors)
            {
                if (error.Severity == EdmSchemaErrorSeverity.Error)
                {
                    return true;
                }
            }
            return false;
        }


        private AssociationSet CreateAssociationSet(LoadMethodSessionState session, 
            AssociationType type)
        {
            AssociationSet set = new AssociationSet(type.Name, type);

            foreach(AssociationEndMember end in type.RelationshipEndMembers)
            {
                EntitySet entitySet = session.GetEntitySet(end);
                DbObjectKey key = session.GetKey(entitySet.ElementType);
                AssociationSetEnd setEnd = new AssociationSetEnd(entitySet, set, end);
                set.AddAssociationSetEnd(setEnd);
            }

            set.SetReadOnly();
            return set;
        }

        private EntitySet CreateEntitySet( 
            LoadMethodSessionState session,
            EntityType type 
            )
        {
            DbObjectKey key = session.GetKey(type);
            string schema = key.Schema;

            string table = null;
            if (key.TableName != type.Name)
            {
                table = key.TableName;
            }

            EntitySet entitySet = new EntitySet(type.Name, 
                        schema, 
                        table,
                        null,
                        type);

            MetadataProperty property = System.Data.EntityModel.SchemaObjectModel.SchemaElement.CreateMetadataPropertyFromOtherNamespaceXmlArtifact(DesignXmlConstants.EntityStoreSchemaGeneratorNamespace, DesignXmlConstants.EntityStoreSchemaGeneratorTypeAttributeName, GetSourceNameFromObjectType(key.ObjectType));
            List<MetadataProperty> properties = new List<MetadataProperty>();
            properties.Add(property);
            entitySet.AddMetadataProperties(properties);
            entitySet.SetReadOnly();
            return entitySet;
        }

        private string GetSourceNameFromObjectType(DbObjectType dbObjectType)
        {
            switch(dbObjectType)
            {
                case DbObjectType.Table:
                    return DesignXmlConstants.TypeValueTables;
                default:
                    Debug.Assert(dbObjectType == DbObjectType.View, "did you change to a call that could have different types?");
                    return DesignXmlConstants.TypeValueViews;
            }
        }

        private void CreateEdmFunctions(LoadMethodSessionState session)
        {
            using(FunctionDetailsReader reader = _loader.LoadFunctionDetails(session.Filters))
            {
                DbObjectKey currentFunction = new DbObjectKey();
                List<FunctionDetailsReader.Memento> parameters = new List<FunctionDetailsReader.Memento>();
                while(reader.Read())
                {
                    DbObjectKey rowFunction = reader.CreateDbObjectKey();
                    if (rowFunction != currentFunction)
                    {
                        if (!currentFunction.IsEmpty)
                        {
                            CreateEdmFunction(session, currentFunction, parameters);
                            parameters.Clear();
                        }
                        currentFunction = rowFunction;
                    }
                    parameters.Add(reader.CreateMemento());
                }

                if (parameters.Count != 0)
                {
                    CreateEdmFunction(session, currentFunction, parameters);
                }
            }
        }

        private void CreateEdmFunction(LoadMethodSessionState session, DbObjectKey functionKey, List<FunctionDetailsReader.Memento> parameters)
        {
            Debug.Assert(parameters.Count != 0, "don't call the method with no data");

            FunctionDetailsReader row = parameters[0].CreateReader();

            FunctionParameter returnParameter = null;
            bool isValid = true;
            List<EdmSchemaError> errors = new List<EdmSchemaError>();
            if (row.ReturnType != null)
            {
                Debug.Assert(!row.IsTvf, "TVF can't have ReturnType (used only for scalars).");
                bool excludedForTarget;
                TypeUsage returnType = GetScalarFunctionTypeUsage(session, row.ReturnType, out excludedForTarget);
                if (returnType != null)
                {
                    returnParameter = new FunctionParameter(EdmConstants.ReturnType, returnType, ParameterMode.ReturnValue);
                }
                else
                {
                    isValid = false;
                    errors.Add(new EdmSchemaError(excludedForTarget ?
                                                  Strings.UnsupportedFunctionReturnDataTypeForTarget(row.ProcedureName, row.ReturnType) :
                                                  Strings.UnsupportedFunctionReturnDataType(row.ProcedureName, row.ReturnType),
                                                  (int)ModelBuilderErrorCode.UnsupportedType,
                                                  EdmSchemaErrorSeverity.Warning));
                }
            }
            else if (row.IsTvf)
            {
                if (_targetEntityFrameworkVersion < EntityFrameworkVersions.Version3)
                {
                    return;
                }
                RowType tvfReturnType;
                if (session.TryGetTvfReturnType(functionKey, out tvfReturnType) && !session.InvalidTypes.Contains(tvfReturnType))
                {
                    var collectionType = tvfReturnType.GetCollectionType();
                    collectionType.SetReadOnly();
                    returnParameter = new FunctionParameter(EdmConstants.ReturnType, TypeUsage.Create(collectionType), ParameterMode.ReturnValue);
                }
                else
                {
                    isValid = false;

                    // If the TVF return type exists, but it is not valid, then reassign all its errors directly to the TVF.
                    // This is needed in order to avoid the following kind of error reporting:
                    // SSDL:
                    // 
                    // <!-- Errors found while generating type:
                    //    column1 type not supported
                    //    column2 type not supported
                    //   <RowType />
                    // -->
                    // ...
                    // ...
                    // <!-- Error found while generating type:
                    //    TableReferencedByTvfWasNotFound
                    //   <Function Name="TVF" .... />
                    // -->
                    // 
                    // Instead we want something like this:
                    // 
                    // <!-- Errors found while generating type:
                    //    column1 type not supported
                    //    column2 type not supported
                    //    TableReferencedByTvfWasNotFound
                    //   <Function Name="TVF" .... />
                    // -->
                    // 

                    List<EdmSchemaError> tvfReturnTypeErrors;
                    if (tvfReturnType != null && session.ItemToErrorsMap.TryGetValue(tvfReturnType, out tvfReturnTypeErrors))
                    {
                        errors.AddRange(tvfReturnTypeErrors);
                        session.ItemToErrorsMap.Remove(tvfReturnType);
                        if (session.InvalidTypes.Contains(tvfReturnType))
                        {
                            session.InvalidTypes.Remove(tvfReturnType);
                        }
                    }
                    
                    errors.Add(new EdmSchemaError(
                        Strings.TableReferencedByTvfWasNotFound(functionKey),
                        (int)ModelBuilderErrorCode.MissingTvfReturnTable,
                        EdmSchemaErrorSeverity.Warning));
                }
            }
            
            bool caseSensitive = false;
            UniqueIdentifierService uniqueIdentifiers = new UniqueIdentifierService(caseSensitive);
            List<FunctionParameter> functionParameters = new List<FunctionParameter>();
            for (int i = 0; i < parameters.Count && !row.IsParameterNameNull; i++)
            {
                row.Attach(parameters[i]);
                TypeUsage parameterType = null;
                bool excludedForTarget = false;
                if (!row.IsParameterTypeNull)
                {
                    parameterType = GetScalarFunctionTypeUsage(session, row.ParameterType, out excludedForTarget);
                }

                if (parameterType != null)
                {
                    ParameterMode mode;
                    if (!row.TryGetParameterMode(out mode))
                    {
                        isValid = false;
                        string modeValue = "null";
                        if (!row.IsParameterModeNull)
                        {
                            modeValue = row.ProcParameterMode;
                        }
                        errors.Add(new EdmSchemaError(
                            Strings.ParameterDirectionNotValid(
                            row.ProcedureName,
                            row.ParameterName,
                            modeValue),
                            (int)ModelBuilderErrorCode.ParameterDirectionNotValid,
                            EdmSchemaErrorSeverity.Warning));
                    }

                    // the mode will get defaulted to something, so it is ok to keep creating after
                    // an error getting the mode value.
                    string parameterName = EntityModelSchemaGenerator.CreateValidEcmaName(row.ParameterName, 'p');
                    parameterName = uniqueIdentifiers.AdjustIdentifier(parameterName);
                    FunctionParameter parameter = new FunctionParameter(parameterName, parameterType, mode);
                    functionParameters.Add(parameter);
                }
                else
                {
                    isValid = false;
                    string typeValue = "null";
                    if (!row.IsParameterTypeNull)
                    {
                        typeValue = row.ParameterType;
                    }
                    errors.Add(new EdmSchemaError(excludedForTarget ?
                                                  Strings.UnsupportedFunctionParameterDataTypeForTarget(row.ProcedureName, row.ParameterName, i, typeValue) :
                                                  Strings.UnsupportedFunctionParameterDataType(row.ProcedureName, row.ParameterName, i, typeValue),
                                                  (int)ModelBuilderErrorCode.UnsupportedType,
                                                  EdmSchemaErrorSeverity.Warning));
                }
            }

            string functionName = EntityModelSchemaGenerator.CreateValidEcmaName(row.ProcedureName, 'f');
            functionName = session.UsedTypeNames.AdjustIdentifier(functionName);
            FunctionParameter[] returnParameters = 
                returnParameter == null ? new FunctionParameter[0] : new FunctionParameter[] {returnParameter};
            EdmFunction function = new EdmFunction(functionName,
                _namespaceName,
                DataSpace.SSpace,
                new EdmFunctionPayload
                {
                    Schema = row.Schema,
                    StoreFunctionName = functionName != row.ProcedureName ? row.ProcedureName : null,
                    IsAggregate = row.IsIsAggregate,
                    IsBuiltIn = row.IsBuiltIn,
                    IsNiladic = row.IsNiladic,
                    IsComposable = row.IsComposable,
                    ReturnParameters = returnParameters,
                    Parameters = functionParameters.ToArray()
                });
            function.SetReadOnly();

            session.AddErrorsForType(function, errors);
            if (isValid)
            {
                session.Functions.Add(function);
            }
            else
            {
                session.InvalidTypes.Add(function);
            }
        }

        private TypeUsage GetScalarFunctionTypeUsage(LoadMethodSessionState session, string dataType, out bool excludedForTarget)
        {
            PrimitiveType primitiveType;
            if (session.TryGetStorePrimitiveType(dataType, out primitiveType, out excludedForTarget))
            {
                TypeUsage usage = TypeUsage.Create(primitiveType, FacetValues.NullFacetValues);
                return usage;
            }
            return null;
        }

        private void CreateAssociationTypes(LoadMethodSessionState session)
        {
            string currentRelationshipId = string.Empty;
            List<RelationshipDetailsRow> columns = new List<RelationshipDetailsRow>();
            foreach (RelationshipDetailsRow row in _loader.LoadRelationships(session.Filters))
            {
                string rowRelationshipId = row.RelationshipId;
                if (rowRelationshipId != currentRelationshipId)
                {
                    if (!string.IsNullOrEmpty(currentRelationshipId))
                    {
                        CreateAssociationType(session, columns);
                        columns.Clear();
                    }
                    currentRelationshipId = rowRelationshipId;
                }

                columns.Add(row);
            }

            if (!string.IsNullOrEmpty(currentRelationshipId))
            {
                CreateAssociationType(session, columns);
            }
        }

        private void CreateAssociationType(LoadMethodSessionState session,
            List<RelationshipDetailsRow> columns)
        {
            Debug.Assert(columns.Count != 0, "should have at least one column");

            RelationshipDetailsRow firstRow = columns[0];

            // get the entity types for the ends
            EntityType pkEntityType;
            EntityType fkEntityType;
            if (!TryGetEndEntities(session, firstRow, out pkEntityType, out fkEntityType))
            {
                return;
            }

            if (!AreRelationshipColumnsTheTypesEntireKey(pkEntityType, columns, r => r.PKColumn))
            {
                session.AddErrorsForType(pkEntityType, new EdmSchemaError(Strings.UnsupportedDbRelationship(firstRow.RelationshipName), (int)ModelBuilderErrorCode.UnsupportedDbRelationship, EdmSchemaErrorSeverity.Warning));
                return;
                         
            }
            UniqueIdentifierService usedEndNames = new UniqueIdentifierService(false);
            // figure out the lower bound of the pk end
            bool someFkColmnsAreNullable;
            if (_targetEntityFrameworkVersion == EntityFrameworkVersions.Version1)
            {
                someFkColmnsAreNullable = AreAllFkKeyColumnsNullable(fkEntityType, columns);
            }
            else
            {
                someFkColmnsAreNullable = AreAnyFkKeyColumnsNullable(fkEntityType, columns);
            }

            RelationshipMultiplicity pkMultiplicity = someFkColmnsAreNullable ? RelationshipMultiplicity.ZeroOrOne : RelationshipMultiplicity.One;
            //Get the Delete Action for the end and set it.
            //The only DeleteAction we support is Cascade, ignor all others for now.
            OperationAction onDeleteAction = OperationAction.None;
            if (firstRow.RelationshipIsCascadeDelete)
            {
                onDeleteAction = OperationAction.Cascade;
            }
            
            AssociationEndMember pkEnd = CreateAssociationEnd( session,
                        pkEntityType,
                        pkMultiplicity,
                        usedEndNames, onDeleteAction);

            RelationshipMultiplicity fkMultiplicity = RelationshipMultiplicity.Many;

            if ( !someFkColmnsAreNullable &&
                 AreRelationshipColumnsTheTypesEntireKey(fkEntityType, columns, r => r.FKColumn))
            {
                // both the pk and fk side columns are the keys of their types
                // so this is a 1 to one relationship
                fkMultiplicity = RelationshipMultiplicity.ZeroOrOne;
            }

            AssociationEndMember fkEnd = CreateAssociationEnd(session, 
                        fkEntityType, 
                        fkMultiplicity,
                        usedEndNames, OperationAction.None);


            // create the type
            string typeName = session.UsedTypeNames.AdjustIdentifier(firstRow.RelationshipName);
            AssociationType type = new AssociationType(typeName, 
                _namespaceName, false, DataSpace.SSpace);
            type.AddMember(pkEnd);
            type.AddMember(fkEnd);

            List<EdmSchemaError> errors = new List<EdmSchemaError>();
            bool isValid = CreateReferentialConstraint(session,
                        type,
                        pkEnd,
                        fkEnd,
                        columns,
                        errors);


            string errorMessage;

            // We can skip most validation checks if the FKs are directly surfaced (since we can produce valid mappings in these cases).
            if (!this.GenerateForeignKeyProperties)
            {
                if (IsFkPartiallyContainedInPK(type, out errorMessage))
                {
                    errors.Add(new EdmSchemaError(
                        errorMessage,
                        (int)ModelBuilderErrorCode.UnsupportedForeinKeyPattern,
                        EdmSchemaErrorSeverity.Warning));
                    isValid = false;
                }

                if (isValid)
                {
                    //Now check if any FK (which could also be a PK) is shared among multiple Associations (ie shared via foreign key constraint).
                    // To do this we check if the Association Type being generated has any dependent property which is also a dependent in one of the association typed already added.
                    //If so, we keep one Association and throw the rest away.

                    foreach (var toPropertyOfAddedAssociation in session.AssociationTypes.SelectMany(t => t.ReferentialConstraints.SelectMany(refconst => refconst.ToProperties)))
                    {
                        foreach (var toProperty in type.ReferentialConstraints.SelectMany(refconst => refconst.ToProperties))
                        {
                            if (toProperty.DeclaringType.Equals(toPropertyOfAddedAssociation.DeclaringType) && toProperty.Equals(toPropertyOfAddedAssociation))
                            {
                                errors.Add(new EdmSchemaError(
                                    Strings.SharedForeignKey(type.Name, toProperty, toProperty.DeclaringType),
                                    (int)ModelBuilderErrorCode.SharedForeignKey,
                                    EdmSchemaErrorSeverity.Warning));

                                isValid = false;
                                break;
                            }
                        }

                        if (!isValid)
                        {
                            break;
                        }
                    }
                }
            }

            if (isValid)
            { 
                session.AssociationTypes.Add(type);
            }
            else
            {
                session.InvalidTypes.Add(type);
                session.RelationshipEndTypeLookup.Remove(pkEnd);
                session.RelationshipEndTypeLookup.Remove(fkEnd);
            }


            type.SetReadOnly();
            session.AddErrorsForType(type, errors);
        }

        private bool TryGetEndEntities(
            LoadMethodSessionState session,
            RelationshipDetailsRow row,
            out EntityType pkEntityType,
            out EntityType fkEntityType)
        {
            RelationshipDetailsCollection table = row.Table;
            DbObjectKey pkKey = new DbObjectKey(row[table.PKCatalogColumn],
                        row[table.PKSchemaColumn],
                        row[table.PKTableColumn], DbObjectType.Unknown);
            DbObjectKey fkKey = new DbObjectKey(row[table.FKCatalogColumn],
                        row[table.FKSchemaColumn],
                        row[table.FKTableColumn], DbObjectType.Unknown);
            
            bool worked = session.TryGetEntity(pkKey, out pkEntityType);
            worked &= session.TryGetEntity(fkKey, out fkEntityType);

            return worked;
        }

        private static bool AreRelationshipColumnsTheTypesEntireKey( 
            EntityType entity, 
            List<RelationshipDetailsRow> columns,
            Func<RelationshipDetailsRow, string> getColumnName)
        {
            if (entity.KeyMembers.Count != columns.Count)
            {
                // to be the entire key,
                // must have the same number of columns
                return false;
            }
            
            foreach (RelationshipDetailsRow row in columns)
            {
                if (!entity.KeyMembers.Contains(getColumnName(row)))
                {
                    // not a key
                    return false;
                }
            }
            return true;
        }

        private static bool AreAnyFkKeyColumnsNullable(
            EntityType entity,
            List<RelationshipDetailsRow> columns)
        {
            foreach (RelationshipDetailsRow row in columns)
            {
                EdmProperty property;
                if (entity.Properties.TryGetValue(row.FKColumn, false, out property))
                {
                    if (property.Nullable)
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Fail("Why didn't we find the column?");
                    return false;
                }
            }
            return false;
        }

        private static bool AreAllFkKeyColumnsNullable(
            EntityType entity,
            List<RelationshipDetailsRow> columns)
        {
            foreach (RelationshipDetailsRow row in columns)
            {
                EdmProperty property;
                if (entity.Properties.TryGetValue(row.FKColumn, false, out property))
                {
                    if (!property.Nullable)
                    {
                        return false;
                    }
                }
                else
                {
                    Debug.Fail("Why didn't we find the column?");
                    return false;
                }
            }
            return true;
        }

        private AssociationEndMember CreateAssociationEnd(LoadMethodSessionState session, 
            EntityType type, 
            RelationshipMultiplicity multiplicity,
            UniqueIdentifierService usedEndNames,
            OperationAction deleteAction
            )
        {
            string role = usedEndNames.AdjustIdentifier(type.Name);
            RefType refType = type.GetReferenceType();
            AssociationEndMember end = new AssociationEndMember(role, refType, multiplicity);
            end.DeleteBehavior = deleteAction;
            session.RelationshipEndTypeLookup.Add(end, type);
            return end;
        }

        private bool CreateReferentialConstraint(LoadMethodSessionState session,
            AssociationType association,
            AssociationEndMember pkEnd,
            AssociationEndMember fkEnd,
            List<RelationshipDetailsRow> columns,
            List<EdmSchemaError> errors)
        {
            EdmProperty[] fromProperties = new EdmProperty[columns.Count];
            EdmProperty[] toProperties = new EdmProperty[columns.Count];
            EntityType pkEntityType = session.RelationshipEndTypeLookup[pkEnd];
            EntityType fkEntityType = session.RelationshipEndTypeLookup[fkEnd];
            for (int index = 0; index < columns.Count; index++)
            {
                EdmProperty property;

                if(!pkEntityType.Properties.TryGetValue(columns[index].PKColumn, false, out property))
                {
                    errors.Add(
                        new EdmSchemaError(
                          Strings.AssociationMissingKeyColumn(
                            pkEntityType.Name,
                            fkEntityType.Name,
                            pkEntityType.Name + "." + columns[index].PKColumn),
                          (int)ModelBuilderErrorCode.AssociationMissingKeyColumn,
                          EdmSchemaErrorSeverity.Warning));
                    return false;
                }
                fromProperties[index] = property;

                if(!fkEntityType.Properties.TryGetValue(columns[index].FKColumn, false, out property))
                {
                    errors.Add(
                        new EdmSchemaError(
                        Strings.AssociationMissingKeyColumn(
                            pkEntityType.Name,
                            fkEntityType.Name,
                            fkEntityType.Name + "." + columns[index].FKColumn),
                            (int)ModelBuilderErrorCode.AssociationMissingKeyColumn,
                        EdmSchemaErrorSeverity.Warning));
                    return false;
                }
                toProperties[index] = property;
            }

            ReferentialConstraint constraint = new ReferentialConstraint(pkEnd,
                fkEnd,
                fromProperties,
                toProperties);

            association.AddReferentialConstraint(constraint);
            return true;
        }

        static internal bool IsFkPartiallyContainedInPK(AssociationType association, out string errorMessage)
        {
            ReferentialConstraint constraint = association.ReferentialConstraints[0];
            EntityType toType = (EntityType)constraint.ToProperties[0].DeclaringType;

            bool toPropertiesAreFullyContainedInPk = true;
            bool toPropertiesContainedAtLeastOnePK = false;

            foreach (EdmProperty edmProperty in constraint.ToProperties)
            {
                // check if there is at least one to property is not primary key
                toPropertiesAreFullyContainedInPk &= toType.KeyMembers.Contains(edmProperty);
                // check if there is one to property is primary key
                toPropertiesContainedAtLeastOnePK |= toType.KeyMembers.Contains(edmProperty); 
            }
            if (!toPropertiesAreFullyContainedInPk && toPropertiesContainedAtLeastOnePK)
            {
                string foreignKeys = MetadataUtil.MembersToCommaSeparatedString((System.Collections.IEnumerable)constraint.ToProperties);
                string primaryKeys = MetadataUtil.MembersToCommaSeparatedString((System.Collections.IEnumerable)toType.KeyMembers);
                errorMessage = Strings.UnsupportedForeignKeyPattern(association.Name, foreignKeys, primaryKeys, toType.Name);
                return true;
            }
            errorMessage = "";
            return false;
        }

        private void CreateViewEntityTypes(LoadMethodSessionState session)
        {
            CreateTableTypes(session, _loader.LoadViewDetails(session.Filters), CreateEntityType, DbObjectType.View);
        }

        private void CreateTableEntityTypes(LoadMethodSessionState session)
        {
            CreateTableTypes(session, _loader.LoadTableDetails(session.Filters), CreateEntityType, DbObjectType.Table);
        }

        private void CreateTvfReturnRowTypes(LoadMethodSessionState session)
        {
            CreateTableTypes(session, _loader.LoadFunctionReturnTableDetails(session.Filters), CreateTvfReturnRowType, DbObjectType.Function);
        }

        private void CreateTableTypes(
            LoadMethodSessionState session,
            IEnumerable<DataRow> tableDetailsRows,
            Action<
                LoadMethodSessionState/*session*/, 
                IList<TableDetailsRow>/*columns*/, 
                ICollection<string>/*primaryKeys*/,
                DbObjectType/*objectType*/,
                List<EdmSchemaError>/*errors*/> createType,
            DbObjectType objectType)
        {
            DbObjectKey currentKey = new DbObjectKey();
            List<TableDetailsRow> singleTableColumns = new List<TableDetailsRow>();
            List<string> primaryKeys = new List<string>();
            foreach (TableDetailsRow row in tableDetailsRows)
            {
                DbObjectKey rowKey = row.CreateDbObjectKey(objectType);
                if (rowKey != currentKey)
                {
                    if (singleTableColumns.Count != 0)
                    {
                        createType(
                            session,
                            singleTableColumns,
                            primaryKeys,
                            objectType, 
                            null);

                        singleTableColumns.Clear();
                        primaryKeys.Clear();
                    }
                    currentKey = rowKey;
                }

                singleTableColumns.Add(row);
                if (row.IsPrimaryKey)
                {
                    primaryKeys.Add(row.ColumnName);
                }
            }

            // pick up the last one
            if (singleTableColumns.Count != 0)
            {
                createType(
                    session,
                    singleTableColumns,
                    primaryKeys,
                    objectType,
                    null);

            }
        }

        private void CreateEntityType(
            LoadMethodSessionState session, 
            IList<TableDetailsRow> columns, 
            ICollection<string> primaryKeys,
            DbObjectType objectType,
            List<EdmSchemaError> errors)
        {
            Debug.Assert(columns.Count != 0, "Trying to create an EntityType with 0 properties");
            Debug.Assert(primaryKeys != null, "primaryKeys != null");

            DbObjectKey tableKey = columns[0].CreateDbObjectKey(objectType);
            if (errors == null)
            {
                errors = new List<EdmSchemaError>();
            }

            //
            // Handle Tables without explicit declaration of keys
            //
            EntityCreationStatus status = EntityCreationStatus.Normal;
            if (primaryKeys.Count == 0)
            {
                List<string> pKeys = new List<string>(columns.Count);
                session.AddTableWithoutKey(tableKey);
                if (InferKeyColumns(session, columns, pKeys, tableKey, ref primaryKeys))
                {
                    errors.Add(new EdmSchemaError(
                        Strings.NoPrimaryKeyDefined(tableKey),
                                    (int)ModelBuilderErrorCode.NoPrimaryKeyDefined,
                                     EdmSchemaErrorSeverity.Warning));
                    status = EntityCreationStatus.ReadOnly;
                }
                else
                {
                    errors.Add(new EdmSchemaError(
                        Strings.CannotCreateEntityWithNoPrimaryKeyDefined(tableKey),
                                        (int)ModelBuilderErrorCode.CannotCreateEntityWithoutPrimaryKey,
                                        EdmSchemaErrorSeverity.Warning));
                    status = EntityCreationStatus.Invalid;
                }
            }

            Debug.Assert(primaryKeys == null || primaryKeys.Count > 0,"There must be at least one key columns at this point in time");

            IList<string> excludedColumns;
            var properties = CreateEdmProperties(session, columns, tableKey, errors, out excludedColumns);

            var excludedKeyColumns = (primaryKeys != null ? primaryKeys.Intersect(excludedColumns) : new string[0]).ToArray();

            if (primaryKeys != null && excludedKeyColumns.Length == 0)
            {
                foreach (EdmMember pkColumn in properties.Where(p => primaryKeys.Contains(p.Name)))
                {
                    if (!MetadataUtil.IsValidKeyType(_targetEntityFrameworkVersion, pkColumn.TypeUsage.EdmType))
                    {
                        // make it a read-only table by calling this method recursively with no keys
                        errors = new List<EdmSchemaError>();
                        var tableColumn = columns.Where(c => c.ColumnName == pkColumn.Name).Single();
                        errors.Add(new EdmSchemaError(Strings.InvalidTypeForPrimaryKey(tableColumn.GetMostQualifiedTableName(),
                            tableColumn.ColumnName,
                            tableColumn.DataType),
                            (int)ModelBuilderErrorCode.InvalidKeyTypeFound,
                            EdmSchemaErrorSeverity.Warning));

                        string[] keyColumns = new string[0];
                        CreateEntityType(session, columns, keyColumns, objectType, errors);
                        return;
                    }
                }
            }

            if (excludedKeyColumns.Length > 0)
            {
                // see if we have any keys left
                if (primaryKeys != null && excludedKeyColumns.Length < primaryKeys.Count)
                {
                    primaryKeys = primaryKeys.Except(excludedKeyColumns).ToList();
                    status = EntityCreationStatus.ReadOnly;
                }
                else
                {
                    primaryKeys = null;
                    status = EntityCreationStatus.Invalid;
                }

                foreach (string columnName in excludedKeyColumns)
                {
                    if (status == EntityCreationStatus.ReadOnly)
                    {
                        errors.Add(new EdmSchemaError(
                            Strings.ExcludedColumnWasAKeyColumnEntityIsReadOnly(columnName, columns[0].GetMostQualifiedTableName()),
                            (int)ModelBuilderErrorCode.ExcludedColumnWasAKeyColumn,
                            EdmSchemaErrorSeverity.Warning));
                    }
                    else
                    {
                        Debug.Assert(status == EntityCreationStatus.Invalid, "Did we change some code above to make it possible to be something different?");
                        errors.Add(new EdmSchemaError(
                            Strings.ExcludedColumnWasAKeyColumnEntityIsInvalid(columnName, columns[0].GetMostQualifiedTableName()),
                            (int)ModelBuilderErrorCode.ExcludedColumnWasAKeyColumn,
                            EdmSchemaErrorSeverity.Warning));
                    }
                }
            }

            string typeName = session.UsedTypeNames.AdjustIdentifier(columns[0].TableName);
            var entityType = new EntityType(typeName, _namespaceName, DataSpace.SSpace, primaryKeys, properties);
            entityType.SetReadOnly();

            switch (status)
            {
                case EntityCreationStatus.Normal:
                    session.AddEntity(tableKey, entityType);
                    break;
                case EntityCreationStatus.ReadOnly:
                    session.AddEntity(tableKey, entityType);
                    session.ReadOnlyEntities.Add(entityType);
                    break;
                default:
                    Debug.Assert(status == EntityCreationStatus.Invalid, "did you add a new value?");
                    session.InvalidTypes.Add(entityType);
                    break;
            }

            session.AddErrorsForType(entityType, errors);
        }

        private void CreateTvfReturnRowType(
            LoadMethodSessionState session,
            IList<TableDetailsRow> columns,
            ICollection<string> primaryKeys,
            DbObjectType objectType,
            List<EdmSchemaError> errors)
        {
            Debug.Assert(columns.Count != 0, "Trying to create a RowType with 0 properties");
            Debug.Assert(primaryKeys != null, "primaryKeys != null");

            DbObjectKey tableKey = columns[0].CreateDbObjectKey(objectType);
            if (errors == null)
            {
                errors = new List<EdmSchemaError>();
            }

            IList<string> excludedColumns;
            var properties = CreateEdmProperties(session, columns, tableKey, errors, out excludedColumns);

            var rowType = new RowType(properties);
            rowType.SetReadOnly();
            session.AddTvfReturnType(tableKey, rowType);
            if (rowType.Properties.Count == 0)
            {
                session.InvalidTypes.Add(rowType);
            }
            session.AddErrorsForType(rowType, errors);
        }

        private IList<EdmProperty> CreateEdmProperties(
            LoadMethodSessionState session,
            IList<TableDetailsRow> columns,
            DbObjectKey tableKey,
            List<EdmSchemaError> errors,
            out IList<string> excludedColumns)
        {
            Debug.Assert(columns.Count != 0, "columns.Count != 0");
            Debug.Assert(errors != null, "errors != null");

            var members = new List<EdmProperty>();
            excludedColumns = new List<string>();
            foreach (TableDetailsRow row in columns)
            {
                PrimitiveType primitiveType;
                bool excludedForTarget = false;
                if (row.IsDataTypeNull() || !session.TryGetStorePrimitiveType(row.DataType, out primitiveType, out excludedForTarget))
                {
                    string message;
                    if (!row.IsDataTypeNull())
                    {
                        message = excludedForTarget ?
                            Strings.UnsupportedDataTypeForTarget(row.DataType, row.GetMostQualifiedTableName(), row.ColumnName) :
                            Strings.UnsupportedDataType(row.DataType, row.GetMostQualifiedTableName(), row.ColumnName);
                    }
                    else
                    {
                        message = Strings.UnsupportedDataTypeUnknownType(row.ColumnName, row.GetMostQualifiedTableName());
                    }

                    errors.Add(new EdmSchemaError(message, (int)ModelBuilderErrorCode.UnsupportedType, EdmSchemaErrorSeverity.Warning));
                    excludedColumns.Add(row.ColumnName);

                    continue;
                }

                Dictionary<string, Facet> facets = primitiveType.GetAssociatedFacetDescriptions().ToDictionary(fd => fd.FacetName, fd => fd.DefaultValueFacet);
                facets[DbProviderManifest.NullableFacetName] = Facet.Create(facets[DbProviderManifest.NullableFacetName].Description, row.IsNullable);

                if (primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Decimal)
                {
                    Facet precision;
                    if (facets.TryGetValue(DbProviderManifest.PrecisionFacetName, out precision))
                    {
                        if (!row.IsPrecisionNull() && !precision.Description.IsConstant)
                        {
                            if (row.Precision < precision.Description.MinValue || row.Precision > precision.Description.MaxValue)
                            {
                                DbObjectKey key = row.CreateDbObjectKey(tableKey.ObjectType);
                                errors.Add(new EdmSchemaError(
                                    Strings.ColumnFacetValueOutOfRange(
                                        DbProviderManifest.PrecisionFacetName,
                                        row.Precision,
                                        precision.Description.MinValue,
                                        precision.Description.MaxValue,
                                        row.ColumnName,
                                        key),
                                    (int)ModelBuilderErrorCode.FacetValueOutOfRange,
                                    EdmSchemaErrorSeverity.Warning));
                                excludedColumns.Add(row.ColumnName);
                                continue;
                            }
                            facets[precision.Name] = Facet.Create(precision.Description, (byte)row.Precision);
                        }
                    }
                    Facet scale;
                    if (facets.TryGetValue(DbProviderManifest.ScaleFacetName, out scale))
                    {
                        if (!row.IsScaleNull() && !scale.Description.IsConstant)
                        {
                            if (row.Scale < scale.Description.MinValue || row.Scale > scale.Description.MaxValue)
                            {
                                DbObjectKey key = row.CreateDbObjectKey(tableKey.ObjectType);
                                errors.Add(new EdmSchemaError(
                                    Strings.ColumnFacetValueOutOfRange(
                                        DbProviderManifest.ScaleFacetName,
                                        row.Scale,
                                        scale.Description.MinValue,
                                        scale.Description.MaxValue,
                                        row.ColumnName,
                                        key),
                                    (int)ModelBuilderErrorCode.FacetValueOutOfRange,
                                    EdmSchemaErrorSeverity.Warning));
                                excludedColumns.Add(row.ColumnName);
                                continue;
                            }
                            facets[scale.Name] = Facet.Create(scale.Description, (byte)row.Scale);
                        }
                    }
                }
                else if (primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.DateTime || 
                         primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Time ||
                         primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.DateTimeOffset)
                {
                    Facet datetimePrecision;
                    if (facets.TryGetValue(DbProviderManifest.PrecisionFacetName, out datetimePrecision))
                    {
                        if (!row.IsDateTimePrecisionNull() && !datetimePrecision.Description.IsConstant)
                        {
                            if (row.DateTimePrecision < datetimePrecision.Description.MinValue || row.DateTimePrecision > datetimePrecision.Description.MaxValue)
                            {
                                DbObjectKey key = row.CreateDbObjectKey(tableKey.ObjectType);
                                errors.Add(new EdmSchemaError(
                                    Strings.ColumnFacetValueOutOfRange(
                                        DbProviderManifest.PrecisionFacetName,
                                        row.DateTimePrecision,
                                        datetimePrecision.Description.MinValue,
                                        datetimePrecision.Description.MaxValue,
                                        row.ColumnName,
                                        key),
                                    (int)ModelBuilderErrorCode.FacetValueOutOfRange,
                                    EdmSchemaErrorSeverity.Warning));
                                excludedColumns.Add(row.ColumnName);
                                continue;
                            }
                            facets[datetimePrecision.Name] = Facet.Create(datetimePrecision.Description, (byte)row.DateTimePrecision);
                        }
                    }
                }
                else if (primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String ||
                         primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary)
                {
                    Facet maxLength;
                    if (facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, out maxLength))
                    {
                        if (!row.IsMaximumLengthNull() && !maxLength.Description.IsConstant)
                        {
                            if (row.MaximumLength < maxLength.Description.MinValue || row.MaximumLength > maxLength.Description.MaxValue)
                            {
                                DbObjectKey key = row.CreateDbObjectKey(tableKey.ObjectType);
                                errors.Add(new EdmSchemaError(
                                    Strings.ColumnFacetValueOutOfRange(
                                        DbProviderManifest.MaxLengthFacetName,
                                        row.MaximumLength,
                                        maxLength.Description.MinValue,
                                        maxLength.Description.MaxValue,
                                        row.ColumnName,
                                        key),
                                    (int)ModelBuilderErrorCode.FacetValueOutOfRange,
                                    EdmSchemaErrorSeverity.Warning));
                                excludedColumns.Add(row.ColumnName);
                                continue;
                            }
                            facets[maxLength.Name] = Facet.Create(maxLength.Description, row.MaximumLength);
                        }
                    }
                }

                if (!row.IsIsIdentityNull() && row.IsIdentity)
                {
                    Facet facet = Facet.Create(System.Data.Metadata.Edm.Converter.StoreGeneratedPatternFacet, StoreGeneratedPattern.Identity);
                    facets.Add(facet.Name, facet);
                }
                else if (!row.IsIsServerGeneratedNull() && row.IsServerGenerated)
                {
                    Facet facet = Facet.Create(System.Data.Metadata.Edm.Converter.StoreGeneratedPatternFacet, StoreGeneratedPattern.Computed);
                    facets.Add(facet.Name, facet);
                }

                members.Add(new EdmProperty(row.ColumnName, TypeUsage.Create(primitiveType, facets.Values)));
            }

            return members;
        }

        private bool InferKeyColumns(LoadMethodSessionState session, IList<TableDetailsRow> columns, List<string> pKeys, DbObjectKey tableKey, ref ICollection<string> primaryKeys)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (!columns[i].IsNullable)
                {
                    PrimitiveType primitiveType;
                    bool _;
                    if (session.TryGetStorePrimitiveType(columns[i].DataType, out primitiveType, out _) &&
                        MetadataUtil.IsValidKeyType(_targetEntityFrameworkVersion, primitiveType))
                    {
                        pKeys.Add(columns[i].ColumnName);
                    }
                }
            }

            // if there are valid key column candidates, make them the new key columns
            if (pKeys.Count > 0)
            {
                primaryKeys = pKeys;
            }
            else
            {
                primaryKeys = null;
            }

            return primaryKeys != null;
        }

        /// <summary>
        /// Populates DefiningQuery attribute of RO view entities
        /// </summary>
        /// <param name="viewEntitySets"></param>
        /// <param name="entityContainer"></param>
        /// <param name="session"></param>
        private void FixupKeylessEntitySets(EntityContainer entityContainer, LoadMethodSessionState session)
        {
            // if there are views to process
            if (session.ReadOnlyEntities.Count > 0)
            {
                //
                // create 'bogus' metadataworkspace
                //
                MetadataWorkspace metadataWorkspace = CreateMetadataWorkspace(entityContainer, session);

                if (null == metadataWorkspace)
                {
                    // failed to create bogus metadataworkspace
                    return;
                }

                //
                // For all tables/views that we could infer valid keys, update DefiningQuery with 
                // provider specific ReadOnly view SQL
                //
                foreach (EntityType entityType in session.ReadOnlyEntities)
                {
                    EntitySet entitySet = session.EntityTypeToSet[entityType];
                    DbObjectKey key = session.GetKey(entityType);
                    
                    // add properties that make it possible for the designer to track back these 
                    // types to their source db objects
                    List<MetadataProperty> properties = new List<MetadataProperty>();
                    if (key.Schema != null)
                    {
                        properties.Add(System.Data.EntityModel.SchemaObjectModel.SchemaElement.CreateMetadataPropertyFromOtherNamespaceXmlArtifact(DesignXmlConstants.EntityStoreSchemaGeneratorNamespace, DesignXmlConstants.EntityStoreSchemaGeneratorSchemaAttributeName, key.Schema));
                    }
                    properties.Add(System.Data.EntityModel.SchemaObjectModel.SchemaElement.CreateMetadataPropertyFromOtherNamespaceXmlArtifact(DesignXmlConstants.EntityStoreSchemaGeneratorNamespace, DesignXmlConstants.EntityStoreSchemaGeneratorNameAttributeName, key.TableName));
                    entitySet.AddMetadataProperties(properties);

                    FixupViewEntitySetDefiningQuery(entitySet, metadataWorkspace);
                }
            }
        }

        /// <summary>
        /// Creates 'transient' metadataworkspace based on store schema (EntityContainer) and trivial C-S mapping
        /// </summary>
        /// <param name="entityContainer"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        private MetadataWorkspace CreateMetadataWorkspace(EntityContainer entityContainer, LoadMethodSessionState session)
        {
            MetadataWorkspace metadataWorkspace = new MetadataWorkspace();
            
            EntityModelSchemaGenerator modelGen = new EntityModelSchemaGenerator(entityContainer);
            modelGen.GenerateForeignKeyProperties = this.GenerateForeignKeyProperties;

            IEnumerable<EdmSchemaError> errors = modelGen.GenerateMetadata();

            if (EntityStoreSchemaGenerator.HasErrorSeverityErrors(errors))
            {
                // this is a 'transient' metadataworkspace 
                // no errors from this metadataworkspace should be shown to the user
                return null;
            }

            // register edmitemcollection
            metadataWorkspace.RegisterItemCollection(modelGen.EdmItemCollection);
            
            // register StoreItemCollection
            metadataWorkspace.RegisterItemCollection(session.ItemCollection);

            // register mapping
            using (MemoryStream memStream = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(memStream))
                {
                    modelGen.WriteStorageMapping(xmlWriter);
                    xmlWriter.Close();
                }
                
                memStream.Seek(0, SeekOrigin.Begin);

                using (XmlReader xmlReader = XmlReader.Create(memStream))
                {
                    List<XmlReader> xmlReaders = new List<XmlReader>();
                    xmlReaders.Add(xmlReader);
                    metadataWorkspace.RegisterItemCollection(new StorageMappingItemCollection(modelGen.EdmItemCollection, 
                                                                                              session.ItemCollection, 
                                                                                              xmlReaders));
                }
            }

            return metadataWorkspace;
        }

        /// <summary>
        /// Generates provider specific, read only SQL and updates entitySet DefiningQuery 
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="metadataWorkspace"></param>
        private void FixupViewEntitySetDefiningQuery(EntitySet entitySet, MetadataWorkspace metadataWorkspace)
        {
            DbExpressionBinding inputBinding = DbExpressionBuilder.BindAs(DbExpressionBuilder.Scan(entitySet), entitySet.Name);
            List<KeyValuePair<string, DbExpression>> projectList = new List<KeyValuePair<string, DbExpression>>(entitySet.ElementType.Members.Count);
            foreach (EdmMember member in entitySet.ElementType.Members)
            {
                Debug.Assert(member.BuiltInTypeKind == BuiltInTypeKind.EdmProperty, "Every member must be a edmproperty");
                EdmProperty propertyInfo = (EdmProperty)member;
                projectList.Add(new KeyValuePair<string, DbExpression>(member.Name,
                                                                       DbExpressionBuilder.Property(inputBinding.Variable, propertyInfo)));
            }
            DbExpression query = inputBinding.Project(DbExpressionBuilder.NewRow(projectList));
            DbQueryCommandTree dbCommandTree = new DbQueryCommandTree(metadataWorkspace, DataSpace.SSpace, query);

            //
            // get provider SQL and set entitySet DefiningQuery
            //
            entitySet.DefiningQuery = (DbProviderServices.GetProviderServices(_loader.EntityConnection.StoreProviderFactory)
                                                            .CreateCommandDefinition(dbCommandTree))
                                                                .CreateCommand().CommandText;

            Debug.Assert(!String.IsNullOrEmpty(entitySet.DefiningQuery), "DefiningQuery must not be null or empty");
        }
    }
}
