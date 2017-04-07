//------------------------------------------------------------------------------
// <copyright file="EntityCommandDefinition.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.EntityClient {

    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Mapping;
    using System.Data.Metadata.Edm;
    using System.Data.Query.InternalTrees;
    using System.Data.Query.PlanCompiler;
    using System.Data.Query.ResultAssembly;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// An aggregate Command Definition used by the EntityClient layers.  This is an aggregator
    /// object that represent information from multiple underlying provider commands.
    /// </summary>
    sealed internal class EntityCommandDefinition : DbCommandDefinition {

        #region internal state

        /// <summary>
        /// nested store command definitions
        /// </summary>
        private readonly List<DbCommandDefinition> _mappedCommandDefinitions;

        /// <summary>
        /// generates column map for the store result reader
        /// </summary>
        private readonly IColumnMapGenerator[] _columnMapGenerators;

        /// <summary>
        /// list of the parameters that the resulting command should have
        /// </summary>
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<EntityParameter> _parameters;

        /// <summary>
        /// Set of entity sets exposed in the command.
        /// </summary>
        private readonly Set<EntitySet> _entitySets;

        #endregion

        #region constructors
        /// <summary>
        /// don't let this be constructed publicly;
        /// </summary>
        /// <exception cref="EntityCommandCompilationException">Cannot prepare the command definition for execution; consult the InnerException for more information.</exception>
        /// <exception cref="NotSupportedException">The ADO.NET Data Provider you are using does not support CommandTrees.</exception>
        internal EntityCommandDefinition(DbProviderFactory storeProviderFactory, DbCommandTree commandTree) {
            EntityUtil.CheckArgumentNull(storeProviderFactory, "storeProviderFactory");
            EntityUtil.CheckArgumentNull(commandTree, "commandTree");

            DbProviderServices storeProviderServices = DbProviderServices.GetProviderServices(storeProviderFactory);

            try {
                if (DbCommandTreeKind.Query == commandTree.CommandTreeKind) {
                    // Next compile the plan for the command tree
                    List<ProviderCommandInfo> mappedCommandList = new List<ProviderCommandInfo>();
                    ColumnMap columnMap;
                    int columnCount;
                    PlanCompiler.Compile(commandTree, out mappedCommandList, out columnMap, out columnCount, out _entitySets);
                    _columnMapGenerators = new IColumnMapGenerator[] {new ConstantColumnMapGenerator(columnMap, columnCount)};
                    // Note: we presume that the first item in the ProviderCommandInfo is the root node;
                    Debug.Assert(mappedCommandList.Count > 0, "empty providerCommandInfo collection and no exception?"); // this shouldn't ever happen.

                    // Then, generate the store commands from the resulting command tree(s)
                    _mappedCommandDefinitions = new List<DbCommandDefinition>(mappedCommandList.Count);

                    foreach (ProviderCommandInfo providerCommandInfo in mappedCommandList) {
                        DbCommandDefinition providerCommandDefinition = storeProviderServices.CreateCommandDefinition(providerCommandInfo.CommandTree);

                        if (null == providerCommandDefinition) {
                            throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.ProviderReturnedNullForCreateCommandDefinition);
                        }
                        _mappedCommandDefinitions.Add(providerCommandDefinition);
                    }
                }
                else {
                    Debug.Assert(DbCommandTreeKind.Function == commandTree.CommandTreeKind, "only query and function command trees are supported");
                    DbFunctionCommandTree entityCommandTree = (DbFunctionCommandTree)commandTree;

                    // Retrieve mapping and metadata information for the function import.
                    FunctionImportMappingNonComposable mapping = GetTargetFunctionMapping(entityCommandTree);
                    IList<FunctionParameter> returnParameters = entityCommandTree.EdmFunction.ReturnParameters;
                    int resultSetCount = returnParameters.Count > 1 ? returnParameters.Count : 1;
                    _columnMapGenerators = new IColumnMapGenerator[resultSetCount];
                    TypeUsage storeResultType = DetermineStoreResultType(entityCommandTree.MetadataWorkspace, mapping, 0, out _columnMapGenerators[0]);
                    for (int i = 1; i < resultSetCount; i++)
                    {
                        DetermineStoreResultType(entityCommandTree.MetadataWorkspace, mapping, i, out _columnMapGenerators[i]);
                    }
                    // Copy over parameters (this happens through a more indirect route in the plan compiler, but
                    // it happens nonetheless)
                    List<KeyValuePair<string, TypeUsage>> providerParameters = new List<KeyValuePair<string, TypeUsage>>();
                    foreach (KeyValuePair<string, TypeUsage> parameter in entityCommandTree.Parameters)
                    {
                        providerParameters.Add(parameter);
                    }

                    // Construct store command tree usage.
                    DbFunctionCommandTree providerCommandTree = new DbFunctionCommandTree(entityCommandTree.MetadataWorkspace, DataSpace.SSpace,
                        mapping.TargetFunction, storeResultType, providerParameters);
                                        
                    DbCommandDefinition storeCommandDefinition = storeProviderServices.CreateCommandDefinition(providerCommandTree);
                    _mappedCommandDefinitions = new List<DbCommandDefinition>(1) { storeCommandDefinition };

                    EntitySet firstResultEntitySet = mapping.FunctionImport.EntitySets.FirstOrDefault();
                    if (firstResultEntitySet != null)
                    {
                        _entitySets = new Set<EntitySet>();
                        _entitySets.Add(mapping.FunctionImport.EntitySets.FirstOrDefault());
                        _entitySets.MakeReadOnly();
                    }
                }

                // Finally, build a list of the parameters that the resulting command should have;
                List<EntityParameter> parameterList = new List<EntityParameter>();

                foreach (KeyValuePair<string, TypeUsage> queryParameter in commandTree.Parameters) {
                    EntityParameter parameter = CreateEntityParameterFromQueryParameter(queryParameter);
                    parameterList.Add(parameter);
                }

                _parameters = new System.Collections.ObjectModel.ReadOnlyCollection<EntityParameter>(parameterList);
            }
            catch (EntityCommandCompilationException) {
                // No need to re-wrap EntityCommandCompilationException
                throw;
            }
            catch (Exception e) {
                // we should not be wrapping all exceptions
                if (EntityUtil.IsCatchableExceptionType(e)) {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw EntityUtil.CommandCompilation(System.Data.Entity.Strings.EntityClient_CommandDefinitionPreparationFailed, e);
                }
                throw;
            }
        }

        /// <summary>
        /// Determines the store type for a function import.
        /// </summary>
        private TypeUsage DetermineStoreResultType(MetadataWorkspace workspace, FunctionImportMappingNonComposable mapping, int resultSetIndex, out IColumnMapGenerator columnMapGenerator) {
            // Determine column maps and infer result types for the mapped function. There are four varieties:
            // Collection(Entity)
            // Collection(PrimitiveType)
            // Collection(ComplexType)
            // No result type
            TypeUsage storeResultType; 
            {
                StructuralType baseStructuralType;
                EdmFunction functionImport = mapping.FunctionImport;

                // Collection(Entity) or Collection(ComplexType)
                if (MetadataHelper.TryGetFunctionImportReturnType<StructuralType>(functionImport, resultSetIndex, out baseStructuralType))
                {
                    ValidateEdmResultType(baseStructuralType, functionImport);

                    //Note: Defensive check for historic reasons, we expect functionImport.EntitySets.Count > resultSetIndex 
                    EntitySet entitySet = functionImport.EntitySets.Count > resultSetIndex ? functionImport.EntitySets[resultSetIndex] : null;

                    columnMapGenerator = new FunctionColumnMapGenerator(mapping, resultSetIndex, entitySet, baseStructuralType);

                    // We don't actually know the return type for the stored procedure, but we can infer
                    // one based on the mapping (i.e.: a column for every property of the mapped types
                    // and for all discriminator columns)
                    storeResultType = mapping.GetExpectedTargetResultType(workspace, resultSetIndex);
                }

                // Collection(PrimitiveType)
                else
                {
                    FunctionParameter returnParameter = MetadataHelper.GetReturnParameter(functionImport, resultSetIndex);
                    if (returnParameter != null && returnParameter.TypeUsage != null)
                    {
                        // Get metadata description of the return type 
                        storeResultType = returnParameter.TypeUsage;
                        Debug.Assert(storeResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType, "FunctionImport currently supports only collection result type");
                        TypeUsage elementType = ((CollectionType)storeResultType.EdmType).TypeUsage;
                        Debug.Assert(Helper.IsScalarType(elementType.EdmType) 
                            , "FunctionImport supports only Collection(Entity), Collection(Enum) and Collection(Primitive)");

                        // Build collection column map where the first column of the store result is assumed
                        // to contain the primitive type values.
                        ScalarColumnMap scalarColumnMap = new ScalarColumnMap(elementType, string.Empty, 0, 0);
                        SimpleCollectionColumnMap collectionColumnMap = new SimpleCollectionColumnMap(storeResultType,
                            string.Empty, scalarColumnMap, null, null);
                        columnMapGenerator = new ConstantColumnMapGenerator(collectionColumnMap, 1);
                    }

                    // No result type
                    else
                    {
                        storeResultType = null;
                        columnMapGenerator = new ConstantColumnMapGenerator(null, 0);
                    }
                }
            }
            return storeResultType;
        }

        /// <summary>
        /// Handles the following negative scenarios
        /// Nested ComplexType Property in ComplexType
        /// </summary>
        /// <param name="resultType"></param>
        private void ValidateEdmResultType(EdmType resultType, EdmFunction functionImport)
        {
            if (Helper.IsComplexType(resultType))
            {
                ComplexType complexType = resultType as ComplexType;
                Debug.Assert(null != complexType, "we should have a complex type here");

                foreach (var property in complexType.Properties)
                {
                    if (property.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
                    {
                        throw new NotSupportedException(System.Data.Entity.Strings.ComplexTypeAsReturnTypeAndNestedComplexProperty(property.Name, complexType.Name, functionImport.FullName));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves mapping for the given C-Space functionCommandTree
        /// </summary>
        private static FunctionImportMappingNonComposable GetTargetFunctionMapping(DbFunctionCommandTree functionCommandTree)
        {
            Debug.Assert(functionCommandTree.DataSpace == DataSpace.CSpace, "map from CSpace->SSpace function");
            Debug.Assert(functionCommandTree != null, "null functionCommandTree");
            Debug.Assert(!functionCommandTree.EdmFunction.IsComposableAttribute, "functionCommandTree.EdmFunction must be non-composable.");

            // Find mapped store function.
            FunctionImportMapping targetFunctionMapping;
            if (!functionCommandTree.MetadataWorkspace.TryGetFunctionImportMapping(functionCommandTree.EdmFunction, out targetFunctionMapping))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_UnmappedFunctionImport(functionCommandTree.EdmFunction.FullName));
            }
            return (FunctionImportMappingNonComposable)targetFunctionMapping;
        }

        #endregion

        #region public API
        /// <summary>
        /// Create a DbCommand object from the definition, that can be executed
        /// </summary>
        /// <returns></returns>
        public override DbCommand CreateCommand() {
            return new EntityCommand(this);
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Get a list of commands to be executed by the provider
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal IEnumerable<string> MappedCommands {
            get {
                // Build up the list of command texts, if we haven't done so yet
                List<string> mappedCommandTexts = new List<string>();
                foreach (DbCommandDefinition commandDefinition in _mappedCommandDefinitions) {
                    DbCommand mappedCommand = commandDefinition.CreateCommand();
                    mappedCommandTexts.Add(mappedCommand.CommandText);
                }
                return mappedCommandTexts;
            }
        }

        /// <summary>
        /// Creates ColumnMap for result assembly using the given reader.
        /// </summary>
        internal ColumnMap CreateColumnMap(DbDataReader storeDataReader) 
        {
            return CreateColumnMap(storeDataReader, 0);
        }

        /// <summary>
        /// Creates ColumnMap for result assembly using the given reader's resultSetIndexth result set.
        /// </summary>
        internal ColumnMap CreateColumnMap(DbDataReader storeDataReader, int resultSetIndex)
        {
            return _columnMapGenerators[resultSetIndex].CreateColumnMap(storeDataReader);
        }

        /// <summary>
        /// Property to expose the known parameters for the query, so the Command objects 
        /// constructor can poplulate it's parameter collection from.
        /// </summary>
        internal IEnumerable<EntityParameter> Parameters {
            get {
                return _parameters;
            }
        }

        /// <summary>
        /// Set of entity sets exposed in the command.
        /// </summary>
        internal Set<EntitySet> EntitySets {
            get { 
                return _entitySets; 
            }
        }

        /// <summary>
        /// Constructs a EntityParameter from a CQT parameter.
        /// </summary>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        private static EntityParameter CreateEntityParameterFromQueryParameter(KeyValuePair<string, TypeUsage> queryParameter) {
            // We really can't have a parameter here that isn't a scalar type...
            Debug.Assert(TypeSemantics.IsScalarType(queryParameter.Value), "Non-scalar type used as query parameter type");

            EntityParameter result = new EntityParameter();
            result.ParameterName = queryParameter.Key;

            EntityCommandDefinition.PopulateParameterFromTypeUsage(result, queryParameter.Value, isOutParam: false);

            return result;
        }

        internal static void PopulateParameterFromTypeUsage(EntityParameter parameter, TypeUsage type, bool isOutParam)
        {
            // type can be null here if the type provided by the user is not a known model type
            if (type != null)
            {
                PrimitiveTypeKind primitiveTypeKind;

                if (Helper.IsEnumType(type.EdmType))
                {
                    type = TypeUsage.Create(Helper.GetUnderlyingEdmTypeForEnumType(type.EdmType));
                }
                else if (Helper.IsSpatialType(type, out primitiveTypeKind))
                {
                    parameter.EdmType = EdmProviderManifest.Instance.GetPrimitiveType(primitiveTypeKind);
                }
            }
            
            DbCommandDefinition.PopulateParameterFromTypeUsage(parameter, type, isOutParam);            
        }

        /// <summary>
        /// Internal execute method -- copies command information from the map command 
        /// to the command objects, executes them, and builds the result assembly 
        /// structures needed to return the data reader
        /// </summary>
        /// <param name="entityCommand"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">behavior must specify CommandBehavior.SequentialAccess</exception>
        /// <exception cref="InvalidOperationException">input parameters in the entityCommand.Parameters collection must have non-null values.</exception>
        internal DbDataReader Execute(EntityCommand entityCommand, CommandBehavior behavior) {
            if (CommandBehavior.SequentialAccess != (behavior & CommandBehavior.SequentialAccess)) {
                throw EntityUtil.MustUseSequentialAccess();
            }

            DbDataReader storeDataReader = ExecuteStoreCommands(entityCommand, behavior);
            DbDataReader result = null;

            // If we actually executed something, then go ahead and construct a bridge
            // data reader for it.
            if (null != storeDataReader) {
                try {
                    ColumnMap columnMap = this.CreateColumnMap(storeDataReader, 0);
                    if (null == columnMap) {
                        // For a query with no result type (and therefore no column map), consume the reader.
                        // When the user requests Metadata for this reader, we return nothing.
                        CommandHelper.ConsumeReader(storeDataReader);
                        result = storeDataReader;
                    }
                    else {
                        result = BridgeDataReader.Create(storeDataReader, columnMap, entityCommand.Connection.GetMetadataWorkspace(), GetNextResultColumnMaps(storeDataReader));
                    }
                }
                catch {
                    // dispose of store reader if there is an error creating the BridgeDataReader
                    storeDataReader.Dispose();
                    throw;
                }
            }
            return result;
        }

        private IEnumerable<ColumnMap> GetNextResultColumnMaps(DbDataReader storeDataReader)
        {
            for (int i = 1; i < _columnMapGenerators.Length; ++i)
            {
                yield return this.CreateColumnMap(storeDataReader, i);
            }
        }

        /// <summary>
        /// Execute the store commands, and return IteratorSources for each one
        /// </summary>
        /// <param name="entityCommand"></param>
        /// <param name="behavior"></param>
        internal DbDataReader ExecuteStoreCommands(EntityCommand entityCommand, CommandBehavior behavior)
        {
            // SQLPT #120007433 is the work item to implement MARS support, which we
            //                  need to do here, but since the PlanCompiler doesn't 
            //                  have it yet, neither do we...
            if (1 != _mappedCommandDefinitions.Count) {
                throw EntityUtil.NotSupported("MARS");
            }

            EntityTransaction entityTransaction = CommandHelper.GetEntityTransaction(entityCommand);

            DbCommandDefinition definition = _mappedCommandDefinitions[0];
            DbCommand storeProviderCommand = definition.CreateCommand();

            CommandHelper.SetStoreProviderCommandState(entityCommand, entityTransaction, storeProviderCommand);
                        
            // Copy over the values from the map command to the store command; we 
            // assume that they were not renamed by either the plan compiler or SQL 
            // Generation.
            //
            // Note that this pretty much presumes that named parameters are supported
            // by the store provider, but it might work if we don't reorder/reuse
            // parameters.
            //
            // Note also that the store provider may choose to add parameters to thier
            // command object for some things; we'll only copy over the values for
            // parameters that we find in the EntityCommands parameters collection, so 
            // we won't damage anything the store provider did.

            bool hasOutputParameters = false;
            if (storeProviderCommand.Parameters != null)    // SQLBUDT 519066
            {
                DbProviderServices storeProviderServices = DbProviderServices.GetProviderServices(entityCommand.Connection.StoreProviderFactory);

                foreach (DbParameter storeParameter in storeProviderCommand.Parameters) {
                    // I could just use the string indexer, but then if I didn't find it the
                    // consumer would get some ParameterNotFound exeception message and that
                    // wouldn't be very meaningful.  Instead, I use the IndexOf method and
                    // if I don't find it, it's not a big deal (The store provider must
                    // have added it).
                    int parameterOrdinal = entityCommand.Parameters.IndexOf(storeParameter.ParameterName);
                    if (-1 != parameterOrdinal) {
                        EntityParameter entityParameter = entityCommand.Parameters[parameterOrdinal];

                        SyncParameterProperties(entityParameter, storeParameter, storeProviderServices);

                        if (storeParameter.Direction != ParameterDirection.Input) {
                            hasOutputParameters = true;
                        }
                    }
                }
            }

            // If the EntityCommand has output parameters, we must synchronize parameter values when
            // the reader is closed. Tell the EntityCommand about the store command so that it knows
            // where to pull those values from.
            if (hasOutputParameters) {
                entityCommand.SetStoreProviderCommand(storeProviderCommand);
            }

            DbDataReader reader = null;
            try {
                reader = storeProviderCommand.ExecuteReader(behavior & ~CommandBehavior.SequentialAccess);
            }
            catch (Exception e) {
                // we should not be wrapping all exceptions
                if (EntityUtil.IsCatchableExceptionType(e)) {
                    // we don't wan't folks to have to know all the various types of exceptions that can 
                    // occur, so we just rethrow a CommandDefinitionException and make whatever we caught  
                    // the inner exception of it.
                    throw EntityUtil.CommandExecution(System.Data.Entity.Strings.EntityClient_CommandDefinitionExecutionFailed, e);
                }
                throw;
            }
            return reader;
        }

        /// <summary>
        /// Updates storeParameter size, precision and scale properties from user provided parameter properties.
        /// </summary>
        /// <param name="entityParameter"></param>
        /// <param name="storeParameter"></param>
        private static void SyncParameterProperties(EntityParameter entityParameter, DbParameter storeParameter, DbProviderServices storeProviderServices) {
            IDbDataParameter dbDataParameter = (IDbDataParameter)storeParameter;

            // DBType is not currently syncable; it's part of the cache key anyway; this is because we can't guarantee
            // that the store provider will honor it -- (SqlClient doesn't...)
            //if (entityParameter.IsDbTypeSpecified)
            //{
            //    storeParameter.DbType = entityParameter.DbType;
            //}

            // Give the store provider the opportunity to set the value before any parameter state has been copied from
            // the EntityParameter.
            TypeUsage parameterTypeUsage = TypeHelpers.GetPrimitiveTypeUsageForScalar(entityParameter.GetTypeUsage());
            storeProviderServices.SetParameterValue(storeParameter, parameterTypeUsage, entityParameter.Value);

            // Override the store provider parameter state with any explicitly specified values from the EntityParameter.
            if (entityParameter.IsDirectionSpecified)
            {
                storeParameter.Direction = entityParameter.Direction;
            }
            if (entityParameter.IsIsNullableSpecified)
            {
                storeParameter.IsNullable = entityParameter.IsNullable;
            }
            if (entityParameter.IsSizeSpecified)
            {
                storeParameter.Size = entityParameter.Size;
            }
            if (entityParameter.IsPrecisionSpecified)
            {
                dbDataParameter.Precision = entityParameter.Precision;
            }
            if (entityParameter.IsScaleSpecified)
            {
                dbDataParameter.Scale = entityParameter.Scale;
            }
        }

        /// <summary>
        /// Return the string used by EntityCommand and ObjectQuery<T> ToTraceString"/>
        /// </summary>
        /// <returns></returns>
        internal string ToTraceString() {
            if (_mappedCommandDefinitions != null) {
                if (_mappedCommandDefinitions.Count == 1) {
                    // Gosh it sure would be nice if I could just get the inner commandText, but
                    // that would require more public surface area on DbCommandDefinition, or
                    // me to know about the inner object...
                    return _mappedCommandDefinitions[0].CreateCommand().CommandText;
                }
                else {
                    StringBuilder sb = new StringBuilder();
                    foreach (DbCommandDefinition commandDefinition in _mappedCommandDefinitions) {
                        DbCommand mappedCommand = commandDefinition.CreateCommand();
                        sb.Append(mappedCommand.CommandText);
                    }
                    return sb.ToString();
                }
            }
            return string.Empty;
        }

        #endregion

        #region nested types
        /// <summary>
        /// Generates a column map given a data reader.
        /// </summary>
        private interface IColumnMapGenerator {
            /// <summary>
            /// Given a data reader, returns column map.
            /// </summary>
            /// <param name="reader">Data reader.</param>
            /// <returns>Column map.</returns>
            ColumnMap CreateColumnMap(DbDataReader reader);
        }

        /// <summary>
        /// IColumnMapGenerator wrapping a constant instance of a column map (invariant with respect
        /// to the given DbDataReader)
        /// </summary>
        private sealed class ConstantColumnMapGenerator : IColumnMapGenerator {
            private readonly ColumnMap _columnMap;
            private readonly int _fieldsRequired;

            internal ConstantColumnMapGenerator(ColumnMap columnMap, int fieldsRequired) {
                _columnMap = columnMap;
                _fieldsRequired = fieldsRequired;
            }

            ColumnMap IColumnMapGenerator.CreateColumnMap(DbDataReader reader) {
                if (null != reader && reader.FieldCount < _fieldsRequired) {
                    throw EntityUtil.CommandExecution(System.Data.Entity.Strings.EntityClient_TooFewColumns);
                }
                return _columnMap;
            }
        }

        /// <summary>
        /// Generates column maps for a non-composable function mapping.
        /// </summary>
        private sealed class FunctionColumnMapGenerator : IColumnMapGenerator {
            private readonly FunctionImportMappingNonComposable _mapping;
            private readonly EntitySet _entitySet;
            private readonly StructuralType _baseStructuralType;
            private readonly int _resultSetIndex;

            internal FunctionColumnMapGenerator(FunctionImportMappingNonComposable mapping, int resultSetIndex, EntitySet entitySet, StructuralType baseStructuralType)
            {
                _mapping = mapping;
                _entitySet = entitySet;
                _baseStructuralType = baseStructuralType;
                _resultSetIndex = resultSetIndex;
            }

            ColumnMap IColumnMapGenerator.CreateColumnMap(DbDataReader reader)
            {
                return ColumnMapFactory.CreateFunctionImportStructuralTypeColumnMap(reader, _mapping, _resultSetIndex, _entitySet, _baseStructuralType);
            }
        }
        #endregion
    }
}
