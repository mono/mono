//---------------------------------------------------------------------
// <copyright file="EntityCommand.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityClient
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Common.EntitySql;
    using System.Data.Common.QueryCache;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Class representing a command for the conceptual layer
    /// </summary>
    public sealed class EntityCommand : DbCommand
    {
        #region Fields
        private const int InvalidCloseCount = -1;

        private bool _designTimeVisible;
        private string _esqlCommandText;
        private EntityConnection _connection;
        private DbCommandTree _preparedCommandTree;
        private EntityParameterCollection _parameters;
        private int? _commandTimeout;
        private CommandType _commandType;
        private EntityTransaction _transaction;
        private UpdateRowSource _updatedRowSource;
        private EntityCommandDefinition _commandDefinition;
        private bool _isCommandDefinitionBased;
        private DbCommandTree _commandTreeSetByUser;
        private DbDataReader _dataReader;
        private bool _enableQueryPlanCaching;
        private DbCommand _storeProviderCommand;
        #endregion

        /// <summary>
        /// Constructs the EntityCommand object not yet associated to a connection object
        /// </summary>
        public EntityCommand()
        {
            GC.SuppressFinalize(this);

            // Initalize the member field with proper default values
            this._designTimeVisible = true;
            this._commandType = CommandType.Text;
            this._updatedRowSource = UpdateRowSource.Both;
            this._parameters = new EntityParameterCollection();

            // Future Enhancement: (See SQLPT #300004256) At some point it would be  
            // really nice to read defaults from a global configuration, but we're not 
            // doing that today.  
            this._enableQueryPlanCaching = true;
        }

        /// <summary>
        /// Constructs the EntityCommand object with the given eSQL statement, but not yet associated to a connection object
        /// </summary>
        /// <param name="statement">The eSQL command text to execute</param>
        public EntityCommand(string statement)
            : this()
        {
            // Assign other member fields from the parameters
            this._esqlCommandText = statement;
        }

        /// <summary>
        /// Constructs the EntityCommand object with the given eSQL statement and the connection object to use
        /// </summary>
        /// <param name="statement">The eSQL command text to execute</param>
        /// <param name="connection">The connection object</param>
        public EntityCommand(string statement, EntityConnection connection)
            : this(statement)
        {
            // Assign other member fields from the parameters
            this._connection = connection;
        }

        /// <summary>
        /// Constructs the EntityCommand object with the given eSQL statement and the connection object to use
        /// </summary>
        /// <param name="statement">The eSQL command text to execute</param>
        /// <param name="connection">The connection object</param>
        /// <param name="transaction">The transaction object this command executes in</param>
        public EntityCommand(string statement, EntityConnection connection, EntityTransaction transaction)
            : this(statement, connection)
        {
            // Assign other member fields from the parameters
            this._transaction = transaction;
        }

        /// <summary>
        /// Internal constructor used by EntityCommandDefinition
        /// </summary>
        /// <param name="commandDefinition">The prepared command definition that can be executed using this EntityCommand</param>
        internal EntityCommand(EntityCommandDefinition commandDefinition)
            : this()
        {
            // Assign other member fields from the parameters
            this._commandDefinition = commandDefinition; 
            this._parameters = new EntityParameterCollection();

            // Make copies of the parameters
            foreach (EntityParameter parameter in commandDefinition.Parameters)
            {
                this._parameters.Add(parameter.Clone());
            }

            // Reset the dirty flag that was set to true when the parameters were added so that it won't say
            // it's dirty to start with
            this._parameters.ResetIsDirty();

            // Track the fact that this command was created from and represents an already prepared command definition
            this._isCommandDefinitionBased = true;
        }

        /// <summary>
        /// Constructs a new EntityCommand given a EntityConnection and an EntityCommandDefition. This 
        /// constructor is used by ObjectQueryExecution plan to execute an ObjectQuery.
        /// </summary>
        /// <param name="connection">The connection against which this EntityCommand should execute</param>
        /// <param name="commandDefinition">The prepared command definition that can be executed using this EntityCommand</param>
        internal EntityCommand(EntityConnection connection, EntityCommandDefinition entityCommandDefinition )
            : this(entityCommandDefinition)
        {
            this._connection = connection;
        }

        /// <summary>
        /// The connection object used for executing the command
        /// </summary>
        public new EntityConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                if (this._connection != value)
                {
                    if (null != this._connection)
                    {
                        Unprepare();
                    }
                    this._connection = value;

                    this._transaction = null;
                }
            }
        }

        /// <summary>
        /// The connection object used for executing the command
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                return this.Connection;
            }
            set
            {
                this.Connection = (EntityConnection)value;
            }
        }

        /// <summary>
        /// The eSQL statement to execute, only one of the command tree or the command text can be set, not both
        /// </summary>
        public override string CommandText
        {
            get
            {
                // If the user set the command tree previously, then we cannot retrieve the command text
                if (this._commandTreeSetByUser != null)
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CannotGetCommandText);

                return this._esqlCommandText ?? "";
            }
            set
            {
                ThrowIfDataReaderIsOpen();

                // If the user set the command tree previously, then we cannot set the command text
                if (this._commandTreeSetByUser != null)
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CannotSetCommandText);

                if (this._esqlCommandText != value)
                {
                    this._esqlCommandText = value;

                    // Wipe out any preparation work we have done
                    Unprepare();

                    // If the user-defined command text or tree has been set (even to null or empty),
                    // then this command can no longer be considered command definition-based
                    this._isCommandDefinitionBased = false;
                }
            }
        }

        /// <summary>
        /// The command tree to execute, only one of the command tree or the command text can be set, not both.
        /// </summary>
        public DbCommandTree CommandTree
        {
            get
            {
                // If the user set the command text previously, then we cannot retrieve the command tree
                if (!string.IsNullOrEmpty(this._esqlCommandText))
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CannotGetCommandTree);

                return this._commandTreeSetByUser;
            }
            set
            {
                ThrowIfDataReaderIsOpen();

                // If the user set the command text previously, then we cannot set the command tree
                if (!string.IsNullOrEmpty(this._esqlCommandText))
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CannotSetCommandTree);

                // If the command type is not Text, CommandTree cannot be set
                if (CommandType.Text != CommandType)
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.CommandTreeOnStoredProcedureEntityCommand);
                }

                if (this._commandTreeSetByUser != value)
                {
                    this._commandTreeSetByUser = value;

                    // Wipe out any preparation work we have done
                    Unprepare();

                    // If the user-defined command text or tree has been set (even to null or empty),
                    // then this command can no longer be considered command definition-based
                    this._isCommandDefinitionBased = false;
                }
            }
        }

        /// <summary>
        /// Get or set the time in seconds to wait for the command to execute
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                // Returns the timeout value if it has been set
                if (this._commandTimeout != null)
                {
                    return this._commandTimeout.Value;
                }
                
                // Create a provider command object just so we can ask the default timeout
                if (this._connection != null && this._connection.StoreProviderFactory != null)
                {
                    DbCommand storeCommand = this._connection.StoreProviderFactory.CreateCommand();
                    if (storeCommand != null)
                    {
                        return storeCommand.CommandTimeout;
                    }
                }

                return 0;
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                this._commandTimeout = value;
            }
        }

        /// <summary>
        /// The type of command being executed, only applicable when the command is using an eSQL statement and not the tree
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                return this._commandType;
            }
            set
            {
                ThrowIfDataReaderIsOpen();

                // For now, command type other than Text is not supported
                if (value != CommandType.Text && value != CommandType.StoredProcedure)
                {
                    throw EntityUtil.NotSupported(System.Data.Entity.Strings.EntityClient_UnsupportedCommandType);
                }

                this._commandType = value;
            }
        }

        /// <summary>
        /// The collection of parameters for this command
        /// </summary>
        public new EntityParameterCollection Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        /// <summary>
        /// The collection of parameters for this command
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return this.Parameters;
            }
        }

        /// <summary>
        /// The transaction object used for executing the command
        /// </summary>
        public new EntityTransaction Transaction
        {
            get
            {
                return this._transaction;   // SQLBU 496829
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                this._transaction = value;
            }
        }

        /// <summary>
        /// The transaction that this command executes in
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return this.Transaction;
            }
            set
            {
                this.Transaction = (EntityTransaction)value;
            }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the DataRow when used by the Update method of a DbDataAdapter
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this._updatedRowSource;
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                this._updatedRowSource = value;
            }
        }

        /// <summary>
        /// Hidden property used by the designers
        /// </summary>
        public override bool DesignTimeVisible
        {
            get
            {
                return this._designTimeVisible;
            }
            set
            {
                ThrowIfDataReaderIsOpen();
                this._designTimeVisible = value;
                TypeDescriptor.Refresh(this);
            }
        }

        /// <summary>
        /// Enables/Disables query plan caching for this EntityCommand
        /// </summary>
        public bool EnablePlanCaching
        {
            get
            {
                return this._enableQueryPlanCaching;
            }

            set
            {
                ThrowIfDataReaderIsOpen();
                this._enableQueryPlanCaching = value;
            }
        }

        /// <summary>
        /// Cancel the execution of the command
        /// </summary>
        public override void Cancel()
        {
        }

        /// <summary>
        /// Create and return a new parameter object representing a parameter in the eSQL statement
        /// </summary>
        ///
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new EntityParameter CreateParameter()
        {
            return new EntityParameter();
        }

        /// <summary>
        /// Create and return a new parameter object representing a parameter in the eSQL statement
        /// </summary>
        protected override DbParameter CreateDbParameter()
        {
            return CreateParameter();
        }

        /// <summary>
        /// Executes the command and returns a data reader for reading the results
        /// </summary>
        /// <returns>A data readerobject</returns>
        public new EntityDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        /// <summary>
        /// Executes the command and returns a data reader for reading the results. May only
        /// be called on CommandType.CommandText (otherwise, use the standard Execute* methods)
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <returns>A data readerobject</returns>
        /// <exception cref="InvalidOperationException">For stored procedure commands, if called
        /// for anything but an entity collection result</exception>
        public new EntityDataReader ExecuteReader(CommandBehavior behavior)
        {
            Prepare(); // prepare the query first

            EntityDataReader reader = new EntityDataReader(this, _commandDefinition.Execute(this, behavior), behavior);
            _dataReader = reader;
            return reader;
        }

        /// <summary>
        /// Executes the command and returns a data reader for reading the results
        /// </summary>
        /// <param name="behavior">The behavior to use when executing the command</param>
        /// <returns>A data readerobject</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        /// <summary>
        /// Executes the command and discard any results returned from the command
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public override int ExecuteNonQuery()
        {
            return ExecuteScalar<int>(reader => 
            {
                // consume reader before checking records affected
                CommandHelper.ConsumeReader(reader);
                return reader.RecordsAffected;
            });
        }

        /// <summary>
        /// Executes the command and return the first column in the first row of the result, extra results are ignored
        /// </summary>
        /// <returns>The result in the first column in the first row</returns>
        public override object ExecuteScalar()
        {
            return ExecuteScalar<object>(reader => 
            {
                object result = reader.Read() ? reader.GetValue(0) : null;
                // consume reader before retrieving parameters
                CommandHelper.ConsumeReader(reader);
                return result;
            });
        }

        /// <summary>
        /// Executes a reader and retrieves a scalar value using the given resultSelector delegate
        /// </summary>
        private T_Result ExecuteScalar<T_Result>(Func<DbDataReader, T_Result> resultSelector)
        {
            T_Result result;
            using (EntityDataReader reader = ExecuteReader(CommandBehavior.SequentialAccess))
            {
                result = resultSelector(reader);
            }
            return result;
        }

        /// <summary>
        /// Clear out any "compile" state
        /// </summary>
        internal void Unprepare()
        {
            this._commandDefinition = null;
            this._preparedCommandTree = null;

            // Clear the dirty flag on the parameters and parameter collection
            _parameters.ResetIsDirty();            
        }

        /// <summary>
        /// Creates a prepared version of this command
        /// </summary>
        public override void Prepare()
        {
            ThrowIfDataReaderIsOpen();
            CheckIfReadyToPrepare();

            InnerPrepare();
        }

        /// <summary>
        /// Creates a prepared version of this command without regard to the current connection state.
        /// Called by both <see cref="Prepare"/> and <see cref="ToTraceString"/>.
        /// </summary>
        private void InnerPrepare()
        {
            // Unprepare if the parameters have changed to force a reprepare
            if (_parameters.IsDirty)
            {
                Unprepare();
            }

            _commandDefinition = GetCommandDefinition();
            Debug.Assert(null != _commandDefinition, "_commandDefinition cannot be null");
        }

        /// <summary>
        /// Ensures we have the command tree, either the user passed us the tree, or an eSQL statement that we need to parse
        /// </summary>
        private void MakeCommandTree()
        {
            // We must have a connection before we come here
            Debug.Assert(this._connection != null);

            // Do the work only if we don't have a command tree yet
            if (this._preparedCommandTree == null)
            {
                DbCommandTree resultTree = null;
                if (this._commandTreeSetByUser != null)
                {
                    resultTree = this._commandTreeSetByUser;
                }
                else
                if (CommandType.Text == CommandType)
                {
                    if (!string.IsNullOrEmpty(this._esqlCommandText))
                    {
                        // The perspective to be used for the query compilation
                        Perspective perspective = (Perspective)new ModelPerspective(_connection.GetMetadataWorkspace());

                        // get a dictionary of names and typeusage from entity parameter collection
                        Dictionary<string, TypeUsage> queryParams = GetParameterTypeUsage();

                        resultTree = CqlQuery.Compile(
                            this._esqlCommandText, 
                            perspective, 
                            null /*parser option - use default*/, 
                            queryParams.Select(paramInfo => paramInfo.Value.Parameter(paramInfo.Key))).CommandTree;
                    }
                    else
                    {
                        // We have no command text, no command tree, so throw an exception
                        if (this._isCommandDefinitionBased)
                        {
                            // This command was based on a prepared command definition and has no command text,
                            // so reprepare is not possible. To create a new command with different parameters
                            // requires creating a new entity command definition and calling it's CreateCommand method.
                            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CannotReprepareCommandDefinitionBasedCommand);
                        }
                        else
                        {
                            throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_NoCommandText);
                        }
                    }
                }
                else if (CommandType.StoredProcedure == CommandType)
                {
                    // get a dictionary of names and typeusage from entity parameter collection
                    IEnumerable<KeyValuePair<string, TypeUsage>> queryParams = GetParameterTypeUsage();
                    EdmFunction function = DetermineFunctionImport();
                    resultTree = new DbFunctionCommandTree(this.Connection.GetMetadataWorkspace(), DataSpace.CSpace, function, null, queryParams);
                }

                // After everything is good and succeeded, assign the result to our field
                this._preparedCommandTree = resultTree;
            }
        }

        // requires: this must be a StoreProcedure command
        // effects: determines the EntityContainer function import referenced by this.CommandText
        private EdmFunction DetermineFunctionImport()
        {
            Debug.Assert(CommandType.StoredProcedure == this.CommandType);

            if (string.IsNullOrEmpty(this.CommandText) ||
                string.IsNullOrEmpty(this.CommandText.Trim()))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_FunctionImportEmptyCommandText);
            }

            MetadataWorkspace workspace = _connection.GetMetadataWorkspace();

            // parse the command text
            string containerName;
            string functionImportName;
            string defaultContainerName = null; // no default container in EntityCommand
            CommandHelper.ParseFunctionImportCommandText(this.CommandText, defaultContainerName, out containerName, out functionImportName);

            return CommandHelper.FindFunctionImport(_connection.GetMetadataWorkspace(), containerName, functionImportName);
        }

        /// <summary>
        /// Get the command definition for the command; will construct one if there is not already
        /// one constructed, which means it will prepare the command on the client.
        /// </summary>
        /// <returns>the command definition</returns>
        internal EntityCommandDefinition GetCommandDefinition()
        {
            EntityCommandDefinition entityCommandDefinition = _commandDefinition;

            // Construct the command definition using no special options;
            if (null == entityCommandDefinition)
            {
                //
                // check if the _commandDefinition is in cache
                //
                if (!TryGetEntityCommandDefinitionFromQueryCache(out entityCommandDefinition))
                {
                    //
                    // if not, construct the command definition using no special options;
                    //
                    entityCommandDefinition = CreateCommandDefinition();
                }

                _commandDefinition = entityCommandDefinition;
            }

            return entityCommandDefinition;
        }

        /// <summary>
        /// Returns the store command text.
        /// </summary>
        /// <returns></returns>
        [Browsable(false)]
        public string ToTraceString() 
        {
            CheckConnectionPresent();

            InnerPrepare();

            EntityCommandDefinition commandDefinition = _commandDefinition;
            if (null != commandDefinition) 
            {
                return commandDefinition.ToTraceString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets an entitycommanddefinition from cache if a match is found for the given cache key.
        /// </summary>
        /// <param name="entityCommandDefinition">out param. returns the entitycommanddefinition for a given cache key</param>
        /// <returns>true if a match is found in cache, false otherwise</returns>
        private bool TryGetEntityCommandDefinitionFromQueryCache( out EntityCommandDefinition entityCommandDefinition )
        {
            Debug.Assert(null != _connection, "Connection must not be null at this point");
            entityCommandDefinition = null;

            //
            // if EnableQueryCaching is false, then just return to force the CommandDefinition to be created
            //
            if (!this._enableQueryPlanCaching || string.IsNullOrEmpty(this._esqlCommandText))
            {
                return false;
            }

            //
            // Create cache key
            //
            EntityClientCacheKey queryCacheKey = new EntityClientCacheKey(this);

            //
            // Try cache lookup
            //
            QueryCacheManager queryCacheManager = _connection.GetMetadataWorkspace().GetQueryCacheManager();
            Debug.Assert(null != queryCacheManager,"QuerycacheManager instance cannot be null");
            if (!queryCacheManager.TryCacheLookup(queryCacheKey, out entityCommandDefinition))
            {
                //
                // if not, construct the command definition using no special options;
                //
                entityCommandDefinition = CreateCommandDefinition();

                //
                // add to the cache
                //
                QueryCacheEntry outQueryCacheEntry = null;
                if (queryCacheManager.TryLookupAndAdd(new QueryCacheEntry(queryCacheKey, entityCommandDefinition), out outQueryCacheEntry))
                {
                    entityCommandDefinition = (EntityCommandDefinition)outQueryCacheEntry.GetTarget();
                }
            }
            
            Debug.Assert(null != entityCommandDefinition, "out entityCommandDefinition must not be null");

            return true;
        }

        /// <summary>
        /// Creates a commandDefinition for the command, using the options specified.  
        /// 
        /// Note: This method must not be side-effecting of the command
        /// </summary>
        /// <returns>the command definition</returns>
        private EntityCommandDefinition CreateCommandDefinition() 
        {
            MakeCommandTree();
            // Always check the CQT metadata against the connection metadata (internally, CQT already
            // validates metadata consistency)
            if (!_preparedCommandTree.MetadataWorkspace.IsMetadataWorkspaceCSCompatible(this.Connection.GetMetadataWorkspace()))
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CommandTreeMetadataIncompatible);
            }
            EntityCommandDefinition result = EntityProviderServices.Instance.CreateCommandDefinition(this._connection.StoreProviderFactory, this._preparedCommandTree);
            return result;
        }

        private void CheckConnectionPresent()
        {
            if (this._connection == null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_NoConnectionForCommand);
            }
        }

        /// <summary>
        /// Checking the integrity of this command object to see if it's ready to be prepared or executed
        /// </summary>
        private void CheckIfReadyToPrepare()
        {
            // Check that we have a connection
            CheckConnectionPresent();

            if (this._connection.StoreProviderFactory == null || this._connection.StoreConnection == null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionStringNeededBeforeOperation);
            }

            // Make sure the connection is not closed or broken
            if ((this._connection.State == ConnectionState.Closed) || (this._connection.State == ConnectionState.Broken))
            {
                string message = System.Data.Entity.Strings.EntityClient_ExecutingOnClosedConnection(
                    this._connection.State == ConnectionState.Closed ?
                    System.Data.Entity.Strings.EntityClient_ConnectionStateClosed :
                    System.Data.Entity.Strings.EntityClient_ConnectionStateBroken);
                throw EntityUtil.InvalidOperation(message);
            }
        }

        /// <summary>
        /// Checking if the command is still tied to a data reader, if so, then the reader must still be open and we throw
        /// </summary>
        private void ThrowIfDataReaderIsOpen()
        {
            if (this._dataReader != null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_DataReaderIsStillOpen);
            }
        }

        /// <summary>
        /// Returns a dictionary of parameter name and parameter typeusage in s-space from the entity parameter 
        /// collection given by the user.
        /// </summary>
        /// <returns></returns>
        internal Dictionary<string, TypeUsage> GetParameterTypeUsage()
        {
            Debug.Assert(null != _parameters, "_parameters must not be null");
            // Extract type metadata objects from the parameters to be used by CqlQuery.Compile
            Dictionary<string, TypeUsage> queryParams = new Dictionary<string, TypeUsage>(_parameters.Count);
            foreach (EntityParameter parameter in this._parameters)
            {
                // Validate that the parameter name has the format: A character followed by alphanumerics or
                // underscores
                string parameterName = parameter.ParameterName;
                if (string.IsNullOrEmpty(parameterName))
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_EmptyParameterName);
                }

                // Check each parameter to make sure it's an input parameter, currently EntityCommand doesn't support
                // anything else
                if (this.CommandType == CommandType.Text && parameter.Direction != ParameterDirection.Input)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_InvalidParameterDirection(parameter.ParameterName));
                }

                // Checking that we can deduce the type from the parameter if the type is not set
                if (parameter.EdmType == null && parameter.DbType == DbType.Object && (parameter.Value == null || parameter.Value is DBNull))
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_UnknownParameterType(parameterName));
                }

                // Validate that the parameter has an appropriate type and value
                // Any failures in GetTypeUsage will be surfaced as exceptions to the user
                TypeUsage typeUsage = null;
                typeUsage = parameter.GetTypeUsage();

                // Add the query parameter, add the same time detect if this parameter has the same name of a previous parameter
                try
                {
                    queryParams.Add(parameterName, typeUsage);
                }
                catch (ArgumentException e)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_DuplicateParameterNames(parameter.ParameterName), e);
                }
            }
            
            return queryParams;
        }

        /// <summary>
        /// Call only when the reader associated with this command is closing. Copies parameter values where necessary.
        /// </summary>
        internal void NotifyDataReaderClosing()
        {
            // Disassociating the data reader with this command
            this._dataReader = null;

            if (null != _storeProviderCommand)
            {
                CommandHelper.SetEntityParameterValues(this, _storeProviderCommand, _connection);
                _storeProviderCommand = null;
            }
            if (null != this.OnDataReaderClosing)
            {
                this.OnDataReaderClosing(this, new EventArgs());
            }
        }

        /// <summary>
        /// Tells the EntityCommand about the underlying store provider command in case it needs to pull parameter values
        /// when the reader is closing.
        /// </summary>
        internal void SetStoreProviderCommand(DbCommand storeProviderCommand)
        {
            _storeProviderCommand = storeProviderCommand;
        }

        /// <summary>
        /// Event raised when the reader is closing.
        /// </summary>
        internal event EventHandler OnDataReaderClosing;
    }
}
