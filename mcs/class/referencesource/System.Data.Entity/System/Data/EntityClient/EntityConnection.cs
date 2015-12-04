//---------------------------------------------------------------------
// <copyright file="EntityConnection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Common.EntitySql;
using System.Data.Entity;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Transactions;

namespace System.Data.EntityClient
{
    /// <summary>
    /// Class representing a connection for the conceptual layer. An entity connection may only
    /// be initialized once (by opening the connection). It is subsequently not possible to change
    /// the connection string, attach a new store connection, or change the store connection string.
    /// </summary>
    public sealed class EntityConnection : DbConnection
    {
        #region Constants

        private const string s_metadataPathSeparator = "|";
        private const string s_semicolonSeparator = ";";
        private const string s_entityClientProviderName = "System.Data.EntityClient";
        private const string s_providerInvariantName = "provider";
        private const string s_providerConnectionString = "provider connection string";
        private const string s_readerPrefix = "reader://";
        internal static readonly StateChangeEventArgs StateChangeClosed = new StateChangeEventArgs(ConnectionState.Open, ConnectionState.Closed);
        internal static readonly StateChangeEventArgs StateChangeOpen = new StateChangeEventArgs(ConnectionState.Closed, ConnectionState.Open);
        #endregion

        private readonly object _connectionStringLock = new object();
        private static readonly DbConnectionOptions s_emptyConnectionOptions = new DbConnectionOptions(String.Empty, null);

        // The connection options object having the connection settings needed by this connection
        private DbConnectionOptions _userConnectionOptions;
        private DbConnectionOptions _effectiveConnectionOptions;

        // The internal connection state of the entity client, which is distinct from that of the
        // store connection it aggregates.
        private ConnectionState _entityClientConnectionState = ConnectionState.Closed;

        private DbProviderFactory _providerFactory;
        private DbConnection _storeConnection;
        private readonly bool _userOwnsStoreConnection;
        private MetadataWorkspace _metadataWorkspace;
        // DbTransaction started using BeginDbTransaction() method
        private EntityTransaction _currentTransaction;
        // Transaction the user enlisted in using EnlistTransaction() method
        private Transaction _enlistedTransaction;
        private bool _initialized;
        // will only have a value while waiting for the ssdl to be loaded. we should 
        // never have a value for this when _initialized == true
        private MetadataArtifactLoader _artifactLoader;

        /// <summary>
        /// Constructs the EntityConnection object with a connection not yet associated to a particular store
        /// </summary>
        [ResourceExposure(ResourceScope.None)] //We are not exposing any resource
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For EntityConnection constructor. But since the connection string we pass in is an Empty String,
                                                     //we consume the resource and do not expose it any further.        
        public EntityConnection()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Constructs the EntityConnection object with a connection string
        /// </summary>
        /// <param name="connectionString">The connection string, may contain a list of settings for the connection or
        /// just the name of the connection to use</param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
        [ResourceConsumption(ResourceScope.Machine)] //For ChangeConnectionString method call. But the paths are not created in this method.        
        public EntityConnection(string connectionString)
        {
            GC.SuppressFinalize(this);
            this.ChangeConnectionString(connectionString);
        }

        /// <summary>
        /// Constructs the EntityConnection from Metadata loaded in memory
        /// </summary>
        /// <param name="workspace">Workspace containing metadata information.</param>
        public EntityConnection(MetadataWorkspace workspace, DbConnection connection)
        {
            GC.SuppressFinalize(this);

            EntityUtil.CheckArgumentNull(workspace, "workspace");
            EntityUtil.CheckArgumentNull(connection, "connection");
            

            if (!workspace.IsItemCollectionAlreadyRegistered(DataSpace.CSpace))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ItemCollectionsNotRegisteredInWorkspace("EdmItemCollection"));
            }
            if(!workspace.IsItemCollectionAlreadyRegistered(DataSpace.SSpace))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ItemCollectionsNotRegisteredInWorkspace("StoreItemCollection"));
            }
            if(!workspace.IsItemCollectionAlreadyRegistered(DataSpace.CSSpace))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ItemCollectionsNotRegisteredInWorkspace("StorageMappingItemCollection"));
            }

            if (connection.State != ConnectionState.Closed)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ConnectionMustBeClosed);
            }

            // Verify that a factory can be retrieved
            if (DbProviderFactories.GetFactory(connection) == null)
            {
                throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.EntityClient_DbConnectionHasNoProvider(connection));
            }

            StoreItemCollection collection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);

            _providerFactory = collection.StoreProviderFactory;
            _storeConnection = connection;
            _userOwnsStoreConnection = true;
            _metadataWorkspace = workspace;
            _initialized = true;
        }
        
        /// <summary>
        /// Get or set the entity connection string associated with this connection object
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                //EntityConnection created using MetadataWorkspace
                // _userConnectionOptions is not null when empty Constructor is used
                // Therefore it is sufficient to identify whether EC(MW, DbConnection) is used
                if (this._userConnectionOptions == null)
                {
                    Debug.Assert(_storeConnection != null);

                    string invariantName;
                    if (!EntityUtil.TryGetProviderInvariantName(DbProviderFactories.GetFactory(_storeConnection), out invariantName))
                    {
                        Debug.Fail("Provider Invariant Name not found");
                        invariantName = "";
                    }

                    return string.Format(Globalization.CultureInfo.InvariantCulture,
                        "{0}={3}{4};{1}={5};{2}=\"{6}\";",
                        EntityConnectionStringBuilder.MetadataParameterName,
                        s_providerInvariantName,
                        s_providerConnectionString,
                        s_readerPrefix,
                        _metadataWorkspace.MetadataWorkspaceId,
                        invariantName,
                        FormatProviderString(_storeConnection.ConnectionString));
                }

                string userConnectionString = this._userConnectionOptions.UsersConnectionString;

                // In here, we ask the store connection for the connection string only if the user didn't specify a name
                // connection (meaning effective connection options == user connection options).  If the user specified a
                // named connection, then we return just that.  Otherwise, if the connection string is different from what
                // we have in the connection options, which is possible if the store connection changed the connection
                // string to hide the password, then we use the builder to reconstruct the string. The parameters will be
                // shuffled, which is unavoidable but it's ok because the connection string cannot be the same as what the
                // user originally passed in anyway.  However, if the store connection string is still the same, then we
                // simply return what the user originally passed in.
                if (object.ReferenceEquals(_userConnectionOptions, _effectiveConnectionOptions) && this._storeConnection != null)
                {
                    string storeConnectionString = null;
                    try
                    {
                        storeConnectionString = this._storeConnection.ConnectionString;
                    }
                    catch (Exception e)
                    {
                        if (EntityUtil.IsCatchableExceptionType(e))
                        {
                            throw EntityUtil.Provider(@"ConnectionString", e);
                        }
                        throw;
                    }

                    // SQLBU 514721, 515024 - Defer connection string parsing to ConnectionStringBuilder
                    // if the 'userStoreConnectionString' and 'storeConnectionString' are unequal, except
                    // when they are both null or empty (we treat null and empty as equivalent here).
                    //
                    string userStoreConnectionString = this._userConnectionOptions[EntityConnectionStringBuilder.ProviderConnectionStringParameterName];
                    if ((storeConnectionString != userStoreConnectionString)
                        && !(string.IsNullOrEmpty(storeConnectionString) && string.IsNullOrEmpty(userStoreConnectionString)))
                    {
                        // Feeds the connection string into the connection string builder, then plug in the provider connection string into
                        // the builder, and then extract the string from the builder
                        EntityConnectionStringBuilder connectionStringBuilder = new EntityConnectionStringBuilder(userConnectionString);
                        connectionStringBuilder.ProviderConnectionString = storeConnectionString;
                        return connectionStringBuilder.ConnectionString;
                    }
                }

                return userConnectionString;
            }
            [ResourceExposure(ResourceScope.Machine)] //Exposes the file names as part of ConnectionString which are a Machine resource
            [ResourceConsumption(ResourceScope.Machine)] //For ChangeConnectionString method call. But the paths are not created in this method.
            set
            {
                ValidateChangesPermitted();
                this.ChangeConnectionString(value);
            }
        }

        /// <summary>
        /// Formats provider string to replace " with \" so it can be appended within quotation marks "..."
        /// </summary>
        private static string FormatProviderString(string providerString)
        {
            return providerString.Trim().Replace("\"", "\\\"");
        }

        /// <summary>
        /// Get the time to wait when attempting to establish a connection before ending the try and generating an error
        /// </summary>
        public override int ConnectionTimeout
        {
            get
            {
                if (this._storeConnection == null)
                    return 0;

                try
                {
                    return this._storeConnection.ConnectionTimeout;
                }
                catch (Exception e)
                {
                    if (EntityUtil.IsCatchableExceptionType(e))
                    {
                        throw EntityUtil.Provider(@"ConnectionTimeout", e);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Get the name of the current database or the database that will be used after a connection is opened
        /// </summary>
        public override string Database
        {
            get
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the ConnectionState property of the EntityConnection
        /// </summary>
        public override ConnectionState State
        {
            get
            {
                try
                {
                    if (this._entityClientConnectionState == ConnectionState.Open)
                    {
                        Debug.Assert(this.StoreConnection != null);
                        if (this.StoreConnection.State != ConnectionState.Open)
                        {
                            return ConnectionState.Broken;
                        }
                    }
                    return this._entityClientConnectionState;
                }
                catch (Exception e)
                {
                    if (EntityUtil.IsCatchableExceptionType(e))
                    {
                        throw EntityUtil.Provider(@"State", e);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the name or network address of the data source to connect to
        /// </summary>
        public override string DataSource
        {
            get
            {
                if (this._storeConnection == null)
                    return String.Empty;

                try
                {
                    return this._storeConnection.DataSource;
                }
                catch (Exception e)
                {
                    if (EntityUtil.IsCatchableExceptionType(e))
                    {
                        throw EntityUtil.Provider(@"DataSource", e);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a string that contains the version of the data store to which the client is connected
        /// </summary>
        public override string ServerVersion
        {
            get
            {
                if (this._storeConnection == null)
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionStringNeededBeforeOperation);

                if (this.State != ConnectionState.Open)
                {
                    throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionNotOpen);
                }

                try
                {
                    return this._storeConnection.ServerVersion;
                }
                catch (Exception e)
                {
                    if (EntityUtil.IsCatchableExceptionType(e))
                    {
                        throw EntityUtil.Provider(@"ServerVersion", e);
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the provider factory associated with EntityConnection
        /// </summary>
        override protected DbProviderFactory DbProviderFactory
        {
            get
            {
                return EntityProviderFactory.Instance;
            }
        }

        /// <summary>
        /// Gets the DbProviderFactory for the underlying provider
        /// </summary>
        internal DbProviderFactory StoreProviderFactory
        {
            get
            {
                return this._providerFactory;
            }
        }

        /// <summary>
        /// Gets the DbConnection for the underlying provider connection
        /// </summary>
        public DbConnection StoreConnection
        {
            get
            {
                return this._storeConnection;
            }
        }

        /// <summary>
        /// Gets the metadata workspace used by this connection
        /// </summary>
        [CLSCompliant(false)]
        public MetadataWorkspace GetMetadataWorkspace()
        {
            return GetMetadataWorkspace(true /* initializeAllCollections */);
        }

        private bool ShouldRecalculateMetadataArtifactLoader(List<MetadataArtifactLoader> loaders)
        {
            if (loaders.Any(loader => loader.GetType() == typeof(MetadataArtifactLoaderCompositeFile)))
            {
                // the loaders had folders in it
                return true;
            }
            // in the case that loaders only contains resources or file name, we trust the cache
            return false;
        }

        [ResourceExposure(ResourceScope.None)] //The resource( path name) is not exposed to the callers of this method
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //Fir SplitPaths call and we pick the file names from class variable.
        internal MetadataWorkspace GetMetadataWorkspace(bool initializeAllCollections)
        {
            Debug.Assert(_metadataWorkspace != null || _effectiveConnectionOptions != null, "The effective connection options is null, which should never be");
            if (_metadataWorkspace == null ||
                (initializeAllCollections && !_metadataWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.SSpace)))
            {
                // This lock is to ensure that the connection string and the metadata workspace are in a consistent state, that is, you
                // don't get a metadata workspace not matching what's described by the connection string
                lock (_connectionStringLock)
                {
                    EdmItemCollection edmItemCollection = null;
                    if (_metadataWorkspace == null)
                    {
                        MetadataWorkspace workspace = new MetadataWorkspace();
                        List<MetadataArtifactLoader> loaders = new List<MetadataArtifactLoader>();
                        string paths = _effectiveConnectionOptions[EntityConnectionStringBuilder.MetadataParameterName];

                        if (!string.IsNullOrEmpty(paths))
                        {
                            loaders = MetadataCache.GetOrCreateMetdataArtifactLoader(paths);
                            
                            if(!ShouldRecalculateMetadataArtifactLoader(loaders))
                            {
                                _artifactLoader = MetadataArtifactLoader.Create(loaders);
                            }
                            else
                            {
                                // the loaders contains folders that might get updated during runtime, so we have to recalculate the loaders again
                                _artifactLoader = MetadataArtifactLoader.Create(MetadataCache.SplitPaths(paths));
                            }
                        }
                        else
                        {
                            _artifactLoader = MetadataArtifactLoader.Create(loaders);
                        }

                        edmItemCollection = LoadEdmItemCollection(workspace, _artifactLoader);
                        _metadataWorkspace = workspace;
                    }
                    else
                    {
                        edmItemCollection = (EdmItemCollection)_metadataWorkspace.GetItemCollection(DataSpace.CSpace);
                    }

                    if (initializeAllCollections && !_metadataWorkspace.IsItemCollectionAlreadyRegistered(DataSpace.SSpace))
                    {
                        LoadStoreItemCollections(_metadataWorkspace, _storeConnection, _providerFactory, _effectiveConnectionOptions, edmItemCollection, _artifactLoader);
                        _artifactLoader = null;
                        _initialized = true;
                    }
                }
            }

            return _metadataWorkspace;
        }

        /// <summary>
        /// Gets the current transaction that this connection is enlisted in
        /// </summary>
        internal EntityTransaction CurrentTransaction
        {
            get
            {
                // Null out the current transaction if the state is closed or zombied
                if ((null != _currentTransaction) && ((null == _currentTransaction.StoreTransaction.Connection) || (this.State == ConnectionState.Closed)))
                {
                    ClearCurrentTransaction();
                }
                return _currentTransaction;
            }
        }

        /// <summary>
        /// Whether the user has enlisted in transaction using EnlistTransaction method
        /// </summary>
        /// <remarks>
        /// To avoid threading issues the <see cref="_enlistedTransaction"/> field is not reset when the transaction is completed.
        /// Therefore it is possible that the transaction has completed but the field is not null. As a result we need to check 
        /// the actual transaction status. However it can happen that the transaction has already been disposed and trying to get
        /// transaction status will cause ObjectDisposedException. This would also mean that the transaction has completed and can be reset.
        /// </remarks>
        internal bool EnlistedInUserTransaction
        {
            get
            {
                try
                {
                    return _enlistedTransaction != null && _enlistedTransaction.TransactionInformation.Status == TransactionStatus.Active;
                }
                catch (ObjectDisposedException)
                {
                    _enlistedTransaction = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Establish a connection to the data store by calling the Open method on the underlying data provider
        /// </summary>
        public override void Open()
        {
            if (this._storeConnection == null)
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionStringNeededBeforeOperation);

            if (this.State != ConnectionState.Closed)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_CannotReopenConnection);
            }

            bool closeStoreConnectionOnFailure = false;
            OpenStoreConnectionIf(this._storeConnection.State != ConnectionState.Open,
                                  this._storeConnection,
                                  null,
                                  EntityRes.EntityClient_ProviderSpecificError,
                                  @"Open",
                                  ref closeStoreConnectionOnFailure);

            // the following guards against the case when the user closes the underlying store connection
            // in the state change event handler, as a consequence of which we are in the 'Broken' state
            if (this._storeConnection == null || this._storeConnection.State != ConnectionState.Open)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionNotOpen);
            }

            InitializeMetadata(this._storeConnection, this._storeConnection, closeStoreConnectionOnFailure);
            SetEntityClientConnectionStateToOpen();
        }

        /// <summary>
        /// Helper method that conditionally opens a specified store connection
        /// </summary>
        /// <param name="openCondition">The condition to evaluate</param>
        /// <param name="storeConnectionToOpen">The store connection to open</param>
        /// <param name="originalConnection">The original store connection associated with the entity client</param>
        /// <param name="closeStoreConnectionOnFailure">A flag that is set on if the connection is opened
        /// successfully</param>
        private void OpenStoreConnectionIf(bool openCondition, 
                                           DbConnection storeConnectionToOpen,
                                           DbConnection originalConnection,
                                           string exceptionCode,
                                           string attemptedOperation,
                                           ref bool closeStoreConnectionOnFailure)
        {
            try
            {
                if (openCondition)
                {
                    storeConnectionToOpen.Open();
                    closeStoreConnectionOnFailure = true;
                }

                ResetStoreConnection(storeConnectionToOpen, originalConnection, false);

                // With every successful open of the store connection, always null out the current db transaction and enlistedTransaction
                ClearTransactions();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    string exceptionMessage = string.IsNullOrEmpty(attemptedOperation) ?
                        EntityRes.GetString(exceptionCode) :
                        EntityRes.GetString(exceptionCode, attemptedOperation);

                    throw EntityUtil.ProviderExceptionWithMessage(exceptionMessage, e);
                }
                throw;
            }
        }

        /// <summary>
        /// Helper method to initialize the metadata workspace and reset the store connection
        /// associated with the entity client
        /// </summary>
        /// <param name="newConnection">The new connection to associate with the entity client</param>
        /// <param name="originalConnection">The original connection associated with the entity client</param>
        /// <param name="closeOriginalConnectionOnFailure">A flag to indicate whether the original
        /// store connection needs to be closed on failure</param>
        private void InitializeMetadata(DbConnection newConnection, 
                                        DbConnection originalConnection, 
                                        bool closeOriginalConnectionOnFailure)
        {
            try
            {
                // Ensure metadata is loaded and the workspace is appropriately initialized.
                GetMetadataWorkspace();
            }
            catch (Exception ex)
            {
                // Undo the open if something failed
                if (EntityUtil.IsCatchableExceptionType(ex))
                {
                    ResetStoreConnection(newConnection, originalConnection, closeOriginalConnectionOnFailure);
                }
                throw;
            }
        }
                
        /// <summary>
        /// Set the entity client connection state to Open, and raise an appropriate event
        /// </summary>
        private void SetEntityClientConnectionStateToOpen()
        {
            this._entityClientConnectionState = ConnectionState.Open;
            OnStateChange(StateChangeOpen);
        }

        /// <summary>
        /// This method sets the store connection and hooks up the event
        /// </summary>
        /// <param name="newConnection">The  DbConnection to set</param>
        /// <param name="originalConnection">The original DbConnection to be closed - this argument could be null</param>
        /// <param name="closeOriginalConnection">Indicates whether the original store connection should be closed</param>
        private void ResetStoreConnection(DbConnection newConnection, DbConnection originalConnection, bool closeOriginalConnection)
        {
            this._storeConnection = newConnection;

            if (closeOriginalConnection && (originalConnection != null))
            {
                originalConnection.Close();
            }
        }

        /// <summary>
        /// Create a new command object that uses this connection object.
        /// </summary>
        public new EntityCommand CreateCommand()
        {
            return new EntityCommand(null, this);
        }

        /// <summary>
        /// Create a new command object that uses this connection object
        /// </summary>
        protected override DbCommand CreateDbCommand()
        {
            return this.CreateCommand();
        }

        /// <summary>
        /// Close the connection to the data store
        /// </summary>
        public override void Close()
        {
            // It's a no-op if there isn't an underlying connection
            if (this._storeConnection == null)
                return;

            this.CloseHelper();
        }

        /// <summary>
        /// Changes the current database for this connection
        /// </summary>
        /// <param name="databaseName">The name of the database to change to</param>
        public override void ChangeDatabase(string databaseName)
        {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <returns>An object representing the new transaction</returns>
        public new EntityTransaction BeginTransaction()
        {
            return base.BeginTransaction() as EntityTransaction;
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction</param>
        /// <returns>An object representing the new transaction</returns>
        public new EntityTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return base.BeginTransaction(isolationLevel) as EntityTransaction;
        }

        /// <summary>
        /// Begins a database transaction
        /// </summary>
        /// <param name="isolationLevel">The isolation level of the transaction</param>
        /// <returns>An object representing the new transaction</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            if (CurrentTransaction != null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_TransactionAlreadyStarted);
            }

            if (this._storeConnection == null)
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionStringNeededBeforeOperation);

            if (this.State != ConnectionState.Open)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionNotOpen);
            }

            DbTransaction storeTransaction = null;
            try
            {
                storeTransaction = this._storeConnection.BeginTransaction(isolationLevel);
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.ProviderExceptionWithMessage(
                                            System.Data.Entity.Strings.EntityClient_ErrorInBeginningTransaction,
                                            e
                                        );
                }
                throw;
            }

            // The provider is problematic if it succeeded in beginning a transaction but returned a null
            // for the transaction object
            if (storeTransaction == null)
            {
                throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.EntityClient_ReturnedNullOnProviderMethod("BeginTransaction", _storeConnection.GetType().Name));
            }

            _currentTransaction = new EntityTransaction(this, storeTransaction);
            return _currentTransaction;
        }

        /// <summary>
        /// Enlist in the given transaction
        /// </summary>
        /// <param name="transaction">The transaction object to enlist into</param>
        public override void EnlistTransaction(Transaction transaction)
        {
            if (_storeConnection == null)
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionStringNeededBeforeOperation);

            if (this.State != ConnectionState.Open)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ConnectionNotOpen);
            }

            try
            {
                _storeConnection.EnlistTransaction(transaction);

                // null means "Unenlist transaction". It is fine if no transaction is in progress (no op). Otherwise
                // _storeConnection.EnlistTransaction should throw and we would not get here.
                Debug.Assert(transaction != null || !EnlistedInUserTransaction,
                    "DbConnection should not allow unenlist from a transaction that has not completed.");

                // It is OK to enlist in null transaction or multiple times in the same transaction. 
                // In the latter case we don't need to be called multiple times when the transaction completes
                // so subscribe only when enlisting for the first time. Note that _storeConnection.EnlistTransaction
                // will throw in invalid cases (like enlisting the connection in a transaction when another
                // transaction has not completed) so when we get here we are sure that either no transactions are
                // active or the transaction the caller tries enlisting to 
                // is the active transaction.
                if (transaction != null && !EnlistedInUserTransaction)
                {
                    transaction.TransactionCompleted += EnlistedTransactionCompleted;
                }

                _enlistedTransaction = transaction;
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.Provider(@"EnlistTransaction", e);
                }
                throw;
            }
        }

        /// <summary>
        /// Cleans up this connection object
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_currentTransaction")]
        [ResourceExposure(ResourceScope.None)] //We are not exposing any resource
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //For ChangeConnectionString method call. But since the connection string we pass in is an Empty String,
                                                     //we consume the resource and do not expose it any further.
        protected override void Dispose(bool disposing)
        {
            // It is possible for the EntityConnection to be finalized even if the object was not actually
            // created due to a "won't fix" bug in the x86 JITer--see Dev10 bug 892884.
            // Even without this bug, a stack overflow trying to allocate space to run the constructor can
            // result in effectively the same situation.  This means we can end up finalizing objects that
            // have not even been fully initialized.  In order for this to work we have to be very careful
            // what we do in Dispose and we need to stick rigidly to the "only dispose unmanaged resources
            // if disposing is false" rule.  We don't actually have any unmanaged resources--these are
            // handled by the base class or other managed classes that we have references to.  These classes
            // will dispose of their unmanaged resources on finalize, so we shouldn't try to do it here.
            if (disposing)
            {
                ClearTransactions();
                bool raiseStateChangeEvent = EntityCloseHelper(false, this.State);

                if (this._storeConnection != null)
                {
                    StoreCloseHelper(); // closes store connection
                    if (this._storeConnection != null)
                    {
                        if (!this._userOwnsStoreConnection)  // only dispose it if we didn't get it from the user...
                        {
                            this._storeConnection.Dispose();
                        }
                        this._storeConnection = null;
                    }
                }

                // Change the connection string to just an empty string, ChangeConnectionString should always succeed here,
                // it's unnecessary to pass in the connection string parameter name in the second argument, which we don't
                // have anyway
                this.ChangeConnectionString(String.Empty);

                if (raiseStateChangeEvent)  // we need to raise the event explicitly
                {
                    OnStateChange(StateChangeClosed);
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Reinitialize this connection object to use the new connection string
        /// </summary>
        /// <param name="newConnectionString">The new connection string</param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file names which are a Machine resource as part of the connection string
        private void ChangeConnectionString(string newConnectionString)
        {
            DbConnectionOptions userConnectionOptions = s_emptyConnectionOptions;
            if (!String.IsNullOrEmpty(newConnectionString))
            {
                userConnectionOptions = new DbConnectionOptions(newConnectionString, EntityConnectionStringBuilder.Synonyms);
            }

            DbProviderFactory factory = null;
            DbConnection storeConnection = null;
            DbConnectionOptions effectiveConnectionOptions = userConnectionOptions;

            if (!userConnectionOptions.IsEmpty)
            {
                // Check if we have the named connection, if yes, then use the connection string from the configuration manager settings
                string namedConnection = userConnectionOptions[EntityConnectionStringBuilder.NameParameterName];
                if (!string.IsNullOrEmpty(namedConnection))
                {
                    // There cannot be other parameters when the named connection is specified
                    if (1 < userConnectionOptions.Parsetable.Count)
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ExtraParametersWithNamedConnection);
                    }

                    // Find the named connection from the configuration, then extract the settings
                    ConnectionStringSettings setting = ConfigurationManager.ConnectionStrings[namedConnection];
                    if (setting == null || setting.ProviderName != EntityConnection.s_entityClientProviderName)
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_InvalidNamedConnection);
                    }

                    effectiveConnectionOptions = new DbConnectionOptions(setting.ConnectionString, EntityConnectionStringBuilder.Synonyms);

                    // Check for a nested Name keyword
                    string nestedNamedConnection = effectiveConnectionOptions[EntityConnectionStringBuilder.NameParameterName];
                    if (!string.IsNullOrEmpty(nestedNamedConnection))
                    {
                        throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_NestedNamedConnection(namedConnection));
                    }
                }

                //Validate the connection string has the required Keywords( Provider and Metadata)
                //We trim the values for both the Keywords, so a string value with only spaces will throw an exception
                //reporting back to the user that the Keyword was missing.
                ValidateValueForTheKeyword(effectiveConnectionOptions, EntityConnectionStringBuilder.MetadataParameterName);

                string providerName = ValidateValueForTheKeyword(effectiveConnectionOptions, EntityConnectionStringBuilder.ProviderParameterName);
                // Get the correct provider factory
                factory = GetFactory(providerName);

                // Create the underlying provider specific connection and give it the connection string from the DbConnectionOptions object
                storeConnection = GetStoreConnection(factory);

                try
                {
                    // When the value of 'Provider Connection String' is null, it means it has not been present in the entity connection string at all.
                    // Providers should still be able handle empty connection strings since those may be explicitly passed by clients.
                    string providerConnectionString = effectiveConnectionOptions[EntityConnectionStringBuilder.ProviderConnectionStringParameterName];
                    if (providerConnectionString != null)
                    {
                        storeConnection.ConnectionString = providerConnectionString;
                    }
                }
                catch (Exception e)
                {
                    if (EntityUtil.IsCatchableExceptionType(e))
                    {
                        throw EntityUtil.Provider(@"ConnectionString", e);
                    }
                    throw;
                }
            }
            
            // This lock is to ensure that the connection string matches with the provider connection and metadata workspace that's being
            // managed by this EntityConnection, so states in this connection object are not messed up.
            // It's not for security, but just to help reduce user error.
            lock (_connectionStringLock)
            {
                // Now we have sufficient information and verified the configuration string is good, use them for this connection object
                // Failure should not occur from this point to the end of this method
                this._providerFactory = factory;
                this._metadataWorkspace = null;

                ClearTransactions();
                ResetStoreConnection(storeConnection, null, false);

                // Remembers the connection options objects with the connection string set by the user
                this._userConnectionOptions = userConnectionOptions;
                this._effectiveConnectionOptions = effectiveConnectionOptions;
            }
        }

        private static string ValidateValueForTheKeyword(DbConnectionOptions effectiveConnectionOptions,
            string keywordName)
        {
            string keywordValue = effectiveConnectionOptions[keywordName];
            if (!string.IsNullOrEmpty(keywordValue))
                keywordValue = keywordValue.Trim(); // be nice to user, always trim the value

            // Check that we have a non-null and non-empty value for the keyword
            if (string.IsNullOrEmpty(keywordValue))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_ConnectionStringMissingInfo(keywordName));
            }
            return keywordValue;
        }

        private static EdmItemCollection LoadEdmItemCollection(MetadataWorkspace workspace, MetadataArtifactLoader artifactLoader)
        {
            // Build a string as the key and look up the MetadataCache for a match
            string edmCacheKey = CreateMetadataCacheKey(artifactLoader.GetOriginalPaths(DataSpace.CSpace), null, null);

            // Check the MetadataCache for an entry with this key
            object entryToken;
            EdmItemCollection edmItemCollection = MetadataCache.GetOrCreateEdmItemCollection(edmCacheKey,
                                                                                        artifactLoader,
                                                                                        out entryToken);
            workspace.RegisterItemCollection(edmItemCollection);

            // Adding the edm metadata entry token to the workspace, to make sure that this token remains alive till workspace is alive
            workspace.AddMetadataEntryToken(entryToken);

            return edmItemCollection;
        }

        private static void LoadStoreItemCollections(MetadataWorkspace workspace,
                                                     DbConnection storeConnection,
                                                     DbProviderFactory factory,
                                                     DbConnectionOptions connectionOptions,
                                                     EdmItemCollection edmItemCollection,
                                                     MetadataArtifactLoader artifactLoader)                                                     
        {
            Debug.Assert(workspace.IsItemCollectionAlreadyRegistered(DataSpace.CSpace), "C-Space must be loaded before loading S or C-S space");
                        
            // The provider connection string is optional; if it has not been specified,
            // we pick up the store's connection string.
            //
            string providerConnectionString = connectionOptions[EntityConnectionStringBuilder.ProviderConnectionStringParameterName];
            if (string.IsNullOrEmpty(providerConnectionString) && (storeConnection != null))
            {
                providerConnectionString = storeConnection.ConnectionString;
            }

            // Build a string as the key and look up the MetadataCache for a match
            string storeCacheKey = CreateMetadataCacheKey(artifactLoader.GetOriginalPaths(),
                                                          connectionOptions[EntityConnectionStringBuilder.ProviderParameterName],
                                                          providerConnectionString);

            // Load store metadata.
            object entryToken;
            StorageMappingItemCollection mappingCollection =
                MetadataCache.GetOrCreateStoreAndMappingItemCollections(storeCacheKey,
                                                                     artifactLoader,
                                                                     edmItemCollection,
                                                                     out entryToken);

            workspace.RegisterItemCollection(mappingCollection.StoreItemCollection);
            workspace.RegisterItemCollection(mappingCollection);

            // Adding the store metadata entry token to the workspace
            workspace.AddMetadataEntryToken(entryToken);
        }

        private static string GetErrorMessageWorthyProviderName(DbProviderFactory factory)
        {
            EntityUtil.CheckArgumentNull(factory, "factory");

            string providerName;
            if (!EntityUtil.TryGetProviderInvariantName(factory, out providerName))
            {
                providerName = factory.GetType().FullName;
            }
            return providerName;
        }
                
        /// <summary>
        /// Create a key to be used with the MetadataCache from a connection options object
        /// </summary>
        /// <param name="paths">A list of metadata file paths</param>
        /// <param name="providerName">The provider name</param>
        /// <param name="providerConnectionString">The provider connection string</param>
        /// <returns>The key</returns>
        private static string CreateMetadataCacheKey(IList<string> paths, string providerName, string providerConnectionString)
        {
            int resultCount = 0;
            string result;

            // Do a first pass to calculate the output size of the metadata cache key,
            // then another pass to populate a StringBuilder with the exact size and
            // get the result.
            CreateMetadataCacheKeyWithCount(paths, providerName, providerConnectionString,
                false, ref resultCount, out result);
            CreateMetadataCacheKeyWithCount(paths, providerName, providerConnectionString,
                true, ref resultCount, out result);

            return result;
        }

        /// <summary>
        /// Create a key to be used with the MetadataCache from a connection options 
        /// object.
        /// </summary>
        /// <param name="paths">A list of metadata file paths</param>
        /// <param name="providerName">The provider name</param>
        /// <param name="providerConnectionString">The provider connection string</param>
        /// <param name="buildResult">Whether the result variable should be built.</param>
        /// <param name="resultCount">
        /// On entry, the expected size of the result (unused if buildResult is false).
        /// After execution, the effective result.</param>
        /// <param name="result">The key.</param>
        /// <remarks>
        /// This method should be called once with buildResult=false, to get 
        /// the size of the resulting key, and once with buildResult=true
        /// and the size specification.
        /// </remarks>
        private static void CreateMetadataCacheKeyWithCount(IList<string> paths, 
            string providerName, string providerConnectionString,
            bool buildResult, ref int resultCount, out string result)
        {
            // Build a string as the key and look up the MetadataCache for a match
            StringBuilder keyString;
            if (buildResult)
            {
                keyString = new StringBuilder(resultCount);
            }
            else
            {
                keyString = null;
            }

            // At this point, we've already used resultCount. Reset it
            // to zero to make the final debug assertion that our computation
            // is correct.
            resultCount = 0;

            if (!string.IsNullOrEmpty(providerName))
            {
                resultCount += providerName.Length + 1;
                if (buildResult)
                {
                    keyString.Append(providerName);
                    keyString.Append(EntityConnection.s_semicolonSeparator);
                }
            }

            if (paths != null)
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    if (paths[i].Length > 0)
                    {
                        if (i > 0)
                        {
                            resultCount++;
                            if (buildResult)
                            {
                                keyString.Append(EntityConnection.s_metadataPathSeparator);
                            }
                        }
                        resultCount += paths[i].Length;
                        if (buildResult)
                        {
                            keyString.Append(paths[i]);
                        }
                    }
                }
                resultCount++;
                if (buildResult)
                {
                    keyString.Append(EntityConnection.s_semicolonSeparator);
                }
            }

            if (!string.IsNullOrEmpty(providerConnectionString))
            {
                resultCount += providerConnectionString.Length;
                if (buildResult)
                {
                    keyString.Append(providerConnectionString);
                }
            }

            if (buildResult)
            {
                result = keyString.ToString();
            }
            else
            {
                result = null;
            }

            System.Diagnostics.Debug.Assert(
                !buildResult || (result.Length == resultCount));
        }

        /// <summary>
        /// Clears the current DbTransaction and the transaction the user enlisted the connection in 
        /// with EnlistTransaction() method.
        /// </summary>
        private void ClearTransactions()
        {
            ClearCurrentTransaction();
            ClearEnlistedTransaction();
        }

        /// <summary>
        /// Clears the current DbTransaction for this connection
        /// </summary>
        internal void ClearCurrentTransaction()
        {
            _currentTransaction = null;
        }

        /// <summary>
        /// Clears the transaction the user elinsted in using EnlistTransaction() method.
        /// </summary>
        private void ClearEnlistedTransaction()
        {
            if (EnlistedInUserTransaction)
            {
                _enlistedTransaction.TransactionCompleted -= EnlistedTransactionCompleted;
            }

            _enlistedTransaction = null;
        }

        /// <summary>
        /// Event handler invoked when the transaction has completed (either by committing or rolling back).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TransactionEventArgs"/> that contains the event data.</param>
        /// <remarks>Note that to avoid threading issues we never reset the <see cref=" _enlistedTransaction"/> field here.</remarks>
        private void EnlistedTransactionCompleted(object sender, TransactionEventArgs e)
        {
            e.Transaction.TransactionCompleted -= EnlistedTransactionCompleted;
        }

        /// <summary>
        /// Helper method invoked as part of Close()/Dispose() that releases the underlying
        /// store connection and raises the appropriate event.
        /// </summary>
        private void CloseHelper()
        {
            ConnectionState previousState = this.State; // the public connection state before cleanup
            StoreCloseHelper();
            EntityCloseHelper(
                            true,   // raise the state change event
                            previousState
                        );
        }

        /// <summary>
        /// Store-specific helper method invoked as part of Close()/Dispose().
        /// </summary>
        private void StoreCloseHelper()
        {
            try
            {
                if (this._storeConnection != null && (this._storeConnection.State != ConnectionState.Closed))
                {
                    this._storeConnection.Close();
                }
                // Need to disassociate the transaction objects with this connection
                ClearTransactions();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.ProviderExceptionWithMessage(
                                        System.Data.Entity.Strings.EntityClient_ErrorInClosingConnection,
                                        e
                                    );
                }
                throw;
            }
        }

        /// <summary>
        /// Entity-specific helper method invoked as part of Close()/Dispose().
        /// </summary>
        /// <param name="fireEventOnStateChange">Indicates whether we need to raise the state change event here</param>
        /// <param name="previousState">The public state of the connection before cleanup began</param>
        /// <returns>true if the caller needs to raise the state change event</returns>
        private bool EntityCloseHelper(bool fireEventOnStateChange, ConnectionState previousState)
        {
            bool result = false;

            this._entityClientConnectionState = ConnectionState.Closed;

            if (previousState == ConnectionState.Open)
            {
                if (fireEventOnStateChange)
                {
                    OnStateChange(StateChangeClosed);
                }
                else
                {
                    result = true;  // we didn't raise the event here; the caller should do that
                }
            }

            return result;
        }

        /// <summary>
        /// Call to determine if changes to the entity object are currently permitted.
        /// </summary>
        private void ValidateChangesPermitted()
        {
            if (_initialized)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_SettingsCannotBeChangedOnOpenConnection);
            }
        }

        /// <summary>
        /// Returns the DbProviderFactory associated with specified provider string
        /// </summary>
        private DbProviderFactory GetFactory(string providerString)
        {
            try
            {
                return DbProviderFactories.GetFactory(providerString);
            }
            catch (ArgumentException e)
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityClient_InvalidStoreProvider, e);
            }
        }

        /// <summary>
        /// Uses DbProviderFactory to create a DbConnection
        /// </summary>
        private DbConnection GetStoreConnection(DbProviderFactory factory)
        {
            DbConnection storeConnection = factory.CreateConnection();
            if (storeConnection == null)
            {
                throw EntityUtil.ProviderIncompatible(System.Data.Entity.Strings.EntityClient_ReturnedNullOnProviderMethod("CreateConnection", factory.GetType().Name));
            }
            return storeConnection;
        }

    }
}
