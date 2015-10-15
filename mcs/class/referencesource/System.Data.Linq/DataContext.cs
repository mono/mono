using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Transactions;
using System.Xml;
using System.Runtime.CompilerServices;

namespace System.Data.Linq {
    using System.Data.Linq.Mapping;
    using System.Data.Linq.Provider;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Used to specify how a submit should behave when one
    /// or more updates fail due to optimistic concurrency
    /// conflicts.
    /// </summary>
    public enum ConflictMode {
        /// <summary>
        /// Fail immediately when the first change conflict is encountered.
        /// </summary>
        FailOnFirstConflict,
        /// <summary>
        /// Only fail after all changes have been attempted.
        /// </summary>
        ContinueOnConflict
    }

    /// <summary>
    /// Used to specify a value synchronization strategy. 
    /// </summary>
    public enum RefreshMode {
        /// <summary>
        /// Keep the current values.
        /// </summary>
        KeepCurrentValues,
        /// <summary>
        /// Current values that have been changed are not modified, but
        /// any unchanged values are updated with the current database
        /// values.  No changes are lost in this merge.
        /// </summary>
        KeepChanges,
        /// <summary>
        /// All current values are overwritten with current database values,
        /// regardless of whether they have been changed.
        /// </summary>
        OverwriteCurrentValues
    }

    /// <summary>
    /// The DataContext is the source of all entities mapped over a database connection.
    /// It tracks changes made to all retrieved entities and maintains an 'identity cache' 
    /// that guarantees that entities retrieved more than once are represented using the 
    /// same object instance.
    /// </summary>
    public class DataContext : IDisposable {
        CommonDataServices services;
        IProvider provider;
        Dictionary<MetaTable, ITable> tables;
        bool objectTrackingEnabled = true;
        bool deferredLoadingEnabled = true;
        bool disposed;
        bool isInSubmitChanges;
        DataLoadOptions loadOptions;
        ChangeConflictCollection conflicts;

        private DataContext() {
        }

        public DataContext(string fileOrServerOrConnection) {
            if (fileOrServerOrConnection == null) {
                throw Error.ArgumentNull("fileOrServerOrConnection");
            }
            this.InitWithDefaultMapping(fileOrServerOrConnection);
        }

        public DataContext(string fileOrServerOrConnection, MappingSource mapping) {
            if (fileOrServerOrConnection == null) {
                throw Error.ArgumentNull("fileOrServerOrConnection");
            }
            if (mapping == null) {
                throw Error.ArgumentNull("mapping");
            }
            this.Init(fileOrServerOrConnection, mapping);
        }

        public DataContext(IDbConnection connection) {
            if (connection == null) {
                throw Error.ArgumentNull("connection");
            }
            this.InitWithDefaultMapping(connection);
        }

        public DataContext(IDbConnection connection, MappingSource mapping) {
            if (connection == null) {
                throw Error.ArgumentNull("connection");
            }
            if (mapping == null) {
                throw Error.ArgumentNull("mapping");
            }
            this.Init(connection, mapping);
        }

        internal DataContext(DataContext context) {
            if (context == null) {
                throw Error.ArgumentNull("context");
            }
            this.Init(context.Connection, context.Mapping.MappingSource);
            this.LoadOptions = context.LoadOptions;
            this.Transaction = context.Transaction;
            this.Log = context.Log;
            this.CommandTimeout = context.CommandTimeout;
        }

        #region Dispose\Finalize
        public void Dispose() {            
            this.disposed = true;
            Dispose(true);
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
        }
        // Not implementing finalizer here because there are no unmanaged resources
        // to release. See http://msdnwiki.microsoft.com/en-us/mtpswiki/12afb1ea-3a17-4a3f-a1f0-fcdb853e2359.aspx

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing) {
            // Implemented but empty so that derived contexts can implement
            // a finalizer that potentially cleans up unmanaged resources.
            if (disposing) {
                if (this.provider != null) {
                    this.provider.Dispose();
                    this.provider = null;
                }
                this.services = null;
                this.tables = null;
                this.loadOptions = null;
            }
        }

        internal void CheckDispose() {
            if (this.disposed) {
                throw Error.DataContextCannotBeUsedAfterDispose();
            }
        }
        #endregion

        private void InitWithDefaultMapping(object connection) {
            this.Init(connection, new AttributeMappingSource());
        }

        internal object Clone() {
            CheckDispose();
            return Activator.CreateInstance(this.GetType(), new object[] { this.Connection, this.Mapping.MappingSource });
        }

        private void Init(object connection, MappingSource mapping) {
            MetaModel model = mapping.GetModel(this.GetType());
            this.services = new CommonDataServices(this, model);
            this.conflicts = new ChangeConflictCollection();

            // determine provider
            Type providerType;
            if (model.ProviderType != null) {
                providerType = model.ProviderType;
            }
            else {
                throw Error.ProviderTypeNull();
            }

            if (!typeof(IProvider).IsAssignableFrom(providerType)) {
                throw Error.ProviderDoesNotImplementRequiredInterface(providerType, typeof(IProvider));
            }

            this.provider = (IProvider)Activator.CreateInstance(providerType);
            this.provider.Initialize(this.services, connection);

            this.tables = new Dictionary<MetaTable, ITable>();
            this.InitTables(this);
        }

        internal void ClearCache() {
            CheckDispose();
            this.services.ResetServices();
        }

        internal CommonDataServices Services {
            get { 
                CheckDispose();
                return this.services; 
            }
        }

        /// <summary>
        /// The connection object used by this DataContext when executing queries and commands.
        /// </summary>
        public DbConnection Connection {
            get {
                CheckDispose();
                return this.provider.Connection;
            }
        }

        /// <summary>
        /// The transaction object used by this DataContext when executing queries and commands.
        /// </summary>
        public DbTransaction Transaction {
            get {
                CheckDispose();
                return this.provider.Transaction;
            }
            set {
                CheckDispose();
                this.provider.Transaction = value;
            }
        }

        /// <summary>
        /// The command timeout to use when executing commands.
        /// </summary>
        public int CommandTimeout {
            get {
                CheckDispose();
                return this.provider.CommandTimeout;
            }
            set {
                CheckDispose();
                this.provider.CommandTimeout = value;
            }
        }

        /// <summary>
        /// A text writer used by this DataContext to output information such as query and commands
        /// being executed.
        /// </summary>
        public TextWriter Log {
            get {
                CheckDispose();
                return this.provider.Log;
            }
            set {
                CheckDispose();
                this.provider.Log = value;
            }
        }

        /// <summary>
        /// True if object tracking is enabled, false otherwise.  Object tracking
        /// includes identity caching and change tracking.  If tracking is turned off, 
        /// SubmitChanges and related functionality is disabled.  DeferredLoading is
        /// also disabled when object tracking is disabled.
        /// </summary>
        public bool ObjectTrackingEnabled {
            get {
                CheckDispose();
                return objectTrackingEnabled;
            }
            set {
                CheckDispose();
                if (Services.HasCachedObjects) {
                    throw Error.OptionsCannotBeModifiedAfterQuery();
                }
                objectTrackingEnabled = value;
                if (!objectTrackingEnabled) {
                    deferredLoadingEnabled = false;
                }
                // force reinitialization of cache/tracking objects
                services.ResetServices();
            }
        }

        /// <summary>
        /// True if deferred loading is enabled, false otherwise.  With deferred
        /// loading disabled, association members return default values and are 
        /// not defer loaded.
        /// </summary>
        public bool DeferredLoadingEnabled {
            get {
                CheckDispose();
                return deferredLoadingEnabled;
            }
            set {
                CheckDispose();
                if (Services.HasCachedObjects) {
                    throw Error.OptionsCannotBeModifiedAfterQuery();
                }
                // can't have tracking disabled and deferred loading enabled
                if (!ObjectTrackingEnabled && value) {
                    throw Error.DeferredLoadingRequiresObjectTracking();
                }
                deferredLoadingEnabled = value;
            }
        }

        /// <summary>
        /// The mapping model used to describe the entities
        /// </summary>
        public MetaModel Mapping {
            get {
                CheckDispose();
                return this.services.Model;
            }
        }

        /// <summary>
        /// Verify that change tracking is enabled, and throw an exception
        /// if it is not.
        /// </summary>
        internal void VerifyTrackingEnabled() {
            CheckDispose();
            if (!ObjectTrackingEnabled) {
                throw Error.ObjectTrackingRequired();
            }
        }

        /// <summary>
        /// Verify that submit changes is not occurring
        /// </summary>
        internal void CheckNotInSubmitChanges() {
            CheckDispose();
            if (this.isInSubmitChanges) {
                throw Error.CannotPerformOperationDuringSubmitChanges();
            }
        }

        /// <summary>
        /// Verify that submit changes is occurring
        /// </summary>
        internal void CheckInSubmitChanges() {
            CheckDispose();
            if (!this.isInSubmitChanges) {
                throw Error.CannotPerformOperationOutsideSubmitChanges();
            }
        }

        /// <summary>
        /// Returns the strongly-typed Table object representing a collection of persistent entities.  
        /// Use this collection as the starting point for queries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity objects. In case of a persistent hierarchy
        /// the entity specified must be the base type of the hierarchy.</typeparam>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public Table<TEntity> GetTable<TEntity>() where TEntity : class {
            CheckDispose();
            MetaTable metaTable = this.services.Model.GetTable(typeof(TEntity));
            if (metaTable == null) {
                throw Error.TypeIsNotMarkedAsTable(typeof(TEntity));
            }
            ITable table = this.GetTable(metaTable);
            if (table.ElementType != typeof(TEntity)) {
                throw Error.CouldNotGetTableForSubtype(typeof(TEntity), metaTable.RowType.Type);
            }
            return (Table<TEntity>)table;
        }
       
        /// <summary>
        /// Returns the weakly-typed ITable object representing a collection of persistent entities. 
        /// Use this collection as the starting point for dynamic/runtime-computed queries.
        /// </summary>
        /// <param name="type">The type of the entity objects. In case of a persistent hierarchy
        /// the entity specified must be the base type of the hierarchy.</param>
        /// <returns></returns>
        public ITable GetTable(Type type) {
            CheckDispose();
            if (type == null) {
                throw Error.ArgumentNull("type");
            }
            MetaTable metaTable = this.services.Model.GetTable(type);
            if (metaTable == null) {
                throw Error.TypeIsNotMarkedAsTable(type);
            }
            if (metaTable.RowType.Type != type) {
                throw Error.CouldNotGetTableForSubtype(type, metaTable.RowType.Type);
            }
            return this.GetTable(metaTable);
        }

        private ITable GetTable(MetaTable metaTable) {
            System.Diagnostics.Debug.Assert(metaTable != null);
            ITable tb;
            if (!this.tables.TryGetValue(metaTable, out tb)) {
                ValidateTable(metaTable);
                Type tbType = typeof(Table<>).MakeGenericType(metaTable.RowType.Type);
                tb = (ITable)Activator.CreateInstance(tbType, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic, null, new object[] { this, metaTable }, null);
                this.tables.Add(metaTable, tb);
            }
            return tb;
        }

        private static void ValidateTable(MetaTable metaTable) {
            // Associations can only be between entities - verify both that both ends of all
            // associations are entities.
            foreach(MetaAssociation assoc in metaTable.RowType.Associations) {
                if(!assoc.ThisMember.DeclaringType.IsEntity) {
                    throw Error.NonEntityAssociationMapping(assoc.ThisMember.DeclaringType.Type, assoc.ThisMember.Name, assoc.ThisMember.DeclaringType.Type);
                }
                if(!assoc.OtherType.IsEntity) {
                    throw Error.NonEntityAssociationMapping(assoc.ThisMember.DeclaringType.Type, assoc.ThisMember.Name, assoc.OtherType.Type);
                }
            }
        }

        private void InitTables(object schema) {
            FieldInfo[] fields = schema.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fi in fields) {
                Type ft = fi.FieldType;
                if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(Table<>)) {
                    ITable tb = (ITable)fi.GetValue(schema);
                    if (tb == null) {
                        Type rowType = ft.GetGenericArguments()[0];
                        tb = this.GetTable(rowType);
                        fi.SetValue(schema, tb);
                    }
                }     
            }
        }

        /// <summary>
        /// Internal method that can be accessed by tests to retrieve the provider
        /// The IProvider result can then be cast to the actual provider to call debug methods like
        ///   CheckQueries, QueryCount, EnableCacheLookup
        /// </summary>
        internal IProvider Provider {
            get { 
                CheckDispose();
                return this.provider; 
            }
        }

        /// <summary>
        /// Returns true if the database specified by the connection object exists.
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists() {
            CheckDispose();
            return this.provider.DatabaseExists();
        }

        /// <summary>
        /// Creates a new database instance (catalog or file) at the location specified by the connection
        /// using the metadata encoded within the entities or mapping file.
        /// </summary>
        public void CreateDatabase() {
            CheckDispose();
            this.provider.CreateDatabase();
        }

        /// <summary>
        /// Deletes the database instance at the location specified by the connection.
        /// </summary>
        public void DeleteDatabase() {
            CheckDispose();
            this.provider.DeleteDatabase();
        }

        /// <summary>
        /// Submits one or more commands to the database reflecting the changes made to the retreived entities.
        /// If a transaction is not already specified one will be created for the duration of this operation.
        /// If a change conflict is encountered a ChangeConflictException will be thrown.
        /// </summary>
        public void SubmitChanges() {
            CheckDispose();
            SubmitChanges(ConflictMode.FailOnFirstConflict);
        }

        /// <summary>
        /// Submits one or more commands to the database reflecting the changes made to the retreived entities.
        /// If a transaction is not already specified one will be created for the duration of this operation.
        /// If a change conflict is encountered a ChangeConflictException will be thrown.  
        /// You can override this method to implement common conflict resolution behaviors.
        /// </summary>
        /// <param name="failureMode">Determines how SubmitChanges handles conflicts.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Microsoft: In the middle of attempting to rollback a transaction, outer transaction is thrown.")]
        public virtual void SubmitChanges(ConflictMode failureMode) {
            CheckDispose();
            CheckNotInSubmitChanges();
            VerifyTrackingEnabled();
            this.conflicts.Clear();

            try {
                this.isInSubmitChanges = true;

                if (System.Transactions.Transaction.Current == null && this.provider.Transaction == null) {
                    bool openedConnection = false;
                    DbTransaction transaction = null;
                    try {
                        if (this.provider.Connection.State == ConnectionState.Open) {
                            this.provider.ClearConnection();
                        }
                        if (this.provider.Connection.State == ConnectionState.Closed) {
                            this.provider.Connection.Open();
                            openedConnection = true;
                        }
                        transaction = this.provider.Connection.BeginTransaction(IsolationLevel.ReadCommitted);
                        this.provider.Transaction = transaction;
                        new ChangeProcessor(this.services, this).SubmitChanges(failureMode);
                        this.AcceptChanges();

                        // to commit a transaction, there can be no open readers
                        // on the connection.
                        this.provider.ClearConnection();

                        transaction.Commit();
                    }
                    catch {
                        if (transaction != null) {
                            transaction.Rollback();
                        }
                        throw;
                    }
                    finally {
                        this.provider.Transaction = null;
                        if (openedConnection) {
                            this.provider.Connection.Close();
                        }
                    }
                }
                else {
                    new ChangeProcessor(services, this).SubmitChanges(failureMode);
                    this.AcceptChanges();
                }
            }
            finally {
                this.isInSubmitChanges = false;
            }
        }

        /// <summary>
        /// Refresh the specified object using the mode specified.  If the refresh
        /// cannot be performed (for example if the object no longer exists in the
        /// database) an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="mode">How the refresh should be performed.</param>
        /// <param name="entity">The object to refresh.  The object must be
        /// the result of a previous query.</param>
        public void Refresh(RefreshMode mode, object entity)
        {
            CheckDispose();
            CheckNotInSubmitChanges();
            VerifyTrackingEnabled();
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            Array items = Array.CreateInstance(entity.GetType(), 1);
            items.SetValue(entity, 0);
            this.Refresh(mode, items as IEnumerable);
        }

        /// <summary>
        /// Refresh a set of objects using the mode specified.  If the refresh
        /// cannot be performed (for example if the object no longer exists in the
        /// database) an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="mode">How the refresh should be performed.</param>
        /// <param name="entities">The objects to refresh.</param>
        public void Refresh(RefreshMode mode, params object[] entities)
        {
            CheckDispose(); // code hygeine requirement

            if (entities == null){
                throw Error.ArgumentNull("entities");
            }

            Refresh(mode, (IEnumerable)entities);
        }

        /// <summary>
        /// Refresh a collection of objects using the mode specified.  If the refresh
        /// cannot be performed (for example if the object no longer exists in the
        /// database) an InvalidOperationException is thrown.
        /// </summary>
        /// <param name="mode">How the refresh should be performed.</param>
        /// <param name="entities">The collection of objects to refresh.</param>
        public void Refresh(RefreshMode mode, IEnumerable entities)
        {
            CheckDispose();
            CheckNotInSubmitChanges();
            VerifyTrackingEnabled();

            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }

            // if the collection is a query, we need to execute and buffer,
            // since below we will be issuing additional queries and can only
            // have a single reader open.
            var list = entities.Cast<object>().ToList();

            // create a fresh context to fetch new state from
            DataContext refreshContext = this.CreateRefreshContext();

            foreach (object o in list) {
                // verify that each object in the list is an entity
                MetaType inheritanceRoot = services.Model.GetMetaType(o.GetType()).InheritanceRoot;
                GetTable(inheritanceRoot.Type);

                TrackedObject trackedObject = this.services.ChangeTracker.GetTrackedObject(o);
                if (trackedObject == null) {
                    throw Error.UnrecognizedRefreshObject();
                }

                if (trackedObject.IsNew) {
                    throw Error.RefreshOfNewObject();
                }
                
                // query to get the current database values
                object[] keyValues = CommonDataServices.GetKeyValues(trackedObject.Type, trackedObject.Original);
                object freshInstance = refreshContext.Services.GetObjectByKey(trackedObject.Type, keyValues);
                if (freshInstance == null) {
                    throw Error.RefreshOfDeletedObject();
                }

                // refresh the tracked object using the new values and
                // the mode specified.
                trackedObject.Refresh(mode, freshInstance);
            }
        }

        internal DataContext CreateRefreshContext() {
            CheckDispose();
            return new DataContext(this);
        }

        private void AcceptChanges() {
            CheckDispose();
            VerifyTrackingEnabled();
            this.services.ChangeTracker.AcceptChanges();
        }

        /// <summary>
        /// Returns the query text in the database server's native query language
        /// that would need to be executed to perform the specified query.
        /// </summary>
        /// <param name="query">The query</param>
        /// <returns></returns>
        internal string GetQueryText(IQueryable query) {
            CheckDispose();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            return this.provider.GetQueryText(query.Expression);
        }

        /// <summary>
        /// Returns an IDbCommand object representing the query in the database server's
        /// native query language.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DbCommand GetCommand(IQueryable query) {
            CheckDispose();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            return this.provider.GetCommand(query.Expression);
        }

        /// <summary>
        /// Returns the command text in the database server's native query langauge
        /// that would need to be executed in order to persist the changes made to the
        /// objects back into the database.
        /// </summary>
        /// <returns></returns>
        internal string GetChangeText() {
            CheckDispose();
            VerifyTrackingEnabled();
            return new ChangeProcessor(services, this).GetChangeText();
        }

        /// <summary>
        /// Computes the un-ordered set of objects that have changed
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ChangeSet", Justification="The capitalization was deliberately chosen.")]
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Non-trivial operations are not suitable for properties.")]
        public ChangeSet GetChangeSet() {
            CheckDispose();
            return new ChangeProcessor(this.services, this).GetChangeSet();
        }

        /// <summary>
        /// Execute a command against the database server that does not return a sequence of objects.
        /// The command is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <param name="command">The command specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>A single integer return value</returns>
        public int ExecuteCommand(string command, params object[] parameters) {
            CheckDispose();
            if (command == null) {
                throw Error.ArgumentNull("command");
            }
            if (parameters == null) {
                throw Error.ArgumentNull("parameters");
            }
            return (int)this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), command, parameters).ReturnValue;
        }

        /// <summary>
        /// Execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <typeparam name="TResult">The element type of the result sequence.</typeparam>
        /// <param name="query">The query specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns>An IEnumerable sequence of objects.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public IEnumerable<TResult> ExecuteQuery<TResult>(string query, params object[] parameters) {
            CheckDispose();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            if (parameters == null) {
                throw Error.ArgumentNull("parameters");
            }
            return (IEnumerable<TResult>)this.ExecuteMethodCall(this, ((MethodInfo)MethodInfo.GetCurrentMethod()).MakeGenericMethod(typeof(TResult)), query, parameters).ReturnValue;
        }

        /// <summary>
        /// Execute the sequence returning query against the database server. 
        /// The query is specified using the server's native query language, such as SQL.
        /// </summary>
        /// <param name="elementType">The element type of the result sequence.</param>
        /// <param name="query">The query specified in the server's native query language.</param>
        /// <param name="parameters">The parameter values to use for the query.</param>
        /// <returns></returns>
        public IEnumerable ExecuteQuery(Type elementType, string query, params object[] parameters) {
            CheckDispose();
            if (elementType == null) {
                throw Error.ArgumentNull("elementType");
            }
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            if (parameters == null) {
                throw Error.ArgumentNull("parameters");
            }
            if (_miExecuteQuery == null) {
                _miExecuteQuery = typeof(DataContext).GetMethods().Single(m => m.Name == "ExecuteQuery" && m.GetParameters().Length == 2);
            }
            return (IEnumerable)this.ExecuteMethodCall(this, _miExecuteQuery.MakeGenericMethod(elementType), query, parameters).ReturnValue;
        }
        private static MethodInfo _miExecuteQuery;


        /// <summary>
        /// Executes the equivalent of the specified method call on the database server.
        /// </summary>
        /// <param name="instance">The instance the method is being called on.</param>
        /// <param name="methodInfo">The reflection MethodInfo for the method to invoke.</param>
        /// <param name="parameters">The parameters for the method call.</param>
        /// <returns>The result of the method call. Use this type's ReturnValue property to access the actual return value.</returns>
        internal protected IExecuteResult ExecuteMethodCall(object instance, MethodInfo methodInfo, params object[] parameters) {
            CheckDispose();
            if (instance == null) {
                throw Error.ArgumentNull("instance");
            }
            if (methodInfo == null) {
                throw Error.ArgumentNull("methodInfo");
            }
            if (parameters == null) {
                throw Error.ArgumentNull("parameters");
            }
            return this.provider.Execute(this.GetMethodCall(instance, methodInfo, parameters));
        }

        /// <summary>
        /// Create a query object for the specified method call.
        /// </summary>
        /// <typeparam name="TResult">The element type of the query.</typeparam>
        /// <param name="instance">The instance the method is being called on.</param>
        /// <param name="methodInfo">The reflection MethodInfo for the method to invoke.</param>
        /// <param name="parameters">The parameters for the method call.</param>
        /// <returns>The returned query object</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        internal protected IQueryable<TResult> CreateMethodCallQuery<TResult>(object instance, MethodInfo methodInfo, params object[] parameters) {
            CheckDispose();
            if (instance == null) {
                throw Error.ArgumentNull("instance");
            }
            if (methodInfo == null) {
                throw Error.ArgumentNull("methodInfo");
            }
            if (parameters == null) {
                throw Error.ArgumentNull("parameters");
            }
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(methodInfo.ReturnType)) {
                throw Error.ExpectedQueryableArgument("methodInfo", typeof(IQueryable<TResult>));
            }
            return new DataQuery<TResult>(this, this.GetMethodCall(instance, methodInfo, parameters));
        }

        private Expression GetMethodCall(object instance, MethodInfo methodInfo, params object[] parameters) {
            CheckDispose();
            if (parameters.Length > 0) {
                ParameterInfo[] pis = methodInfo.GetParameters();
                List<Expression> args = new List<Expression>(parameters.Length);
                for (int i = 0, n = parameters.Length; i < n; i++) {
                    Type pType = pis[i].ParameterType;
                    if (pType.IsByRef) {
                        pType = pType.GetElementType();
                    }
                    args.Add(Expression.Constant(parameters[i], pType));
                }
                return Expression.Call(Expression.Constant(instance), methodInfo, args);
            }
            return Expression.Call(Expression.Constant(instance), methodInfo);
        }

        /// <summary>
        /// Execute a dynamic insert
        /// </summary>
        /// <param name="entity"></param>
        internal protected void ExecuteDynamicInsert(object entity) {
            CheckDispose();
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            this.CheckInSubmitChanges();
            TrackedObject tracked = this.services.ChangeTracker.GetTrackedObject(entity);
            if (tracked == null) {
                throw Error.CannotPerformOperationForUntrackedObject();
            }
            this.services.ChangeDirector.DynamicInsert(tracked);
        }

        /// <summary>
        /// Execute a dynamic update
        /// </summary>
        /// <param name="entity"></param>
        internal protected void ExecuteDynamicUpdate(object entity) {
            CheckDispose();
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            this.CheckInSubmitChanges();
            TrackedObject tracked = this.services.ChangeTracker.GetTrackedObject(entity);
            if (tracked == null) {
                throw Error.CannotPerformOperationForUntrackedObject();
            }
            int result = this.services.ChangeDirector.DynamicUpdate(tracked);
            if (result == 0) {
                throw new ChangeConflictException();
            }
        }

        /// <summary>
        /// Execute a dynamic delete
        /// </summary>
        /// <param name="entity"></param>
        internal protected void ExecuteDynamicDelete(object entity) {
            CheckDispose();
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            this.CheckInSubmitChanges();
            TrackedObject tracked = this.services.ChangeTracker.GetTrackedObject(entity);
            if (tracked == null) {
                throw Error.CannotPerformOperationForUntrackedObject();
            }
            int result = this.services.ChangeDirector.DynamicDelete(tracked);
            if (result == 0) {
                throw new ChangeConflictException();
            }
        }

        /// <summary>
        /// Translates the data from a DbDataReader into sequence of objects.
        /// </summary>
        /// <typeparam name="TResult">The element type of the resulting sequence</typeparam>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <returns>The translated sequence of objects</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        public IEnumerable<TResult> Translate<TResult>(DbDataReader reader) {
            CheckDispose();
            return (IEnumerable<TResult>)this.Translate(typeof(TResult), reader);
        }

        /// <summary>
        /// Translates the data from a DbDataReader into sequence of objects.
        /// </summary>
        /// <param name="elementType">The element type of the resulting sequence</param>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <returns>The translated sequence of objects</returns>
        public IEnumerable Translate(Type elementType, DbDataReader reader) {
            CheckDispose();
            if (elementType == null) {
                throw Error.ArgumentNull("elementType");
            }
            if (reader == null) {
                throw Error.ArgumentNull("reader");
            }
            return this.provider.Translate(elementType, reader);
        }

        /// <summary>
        /// Translates the data from a DbDataReader into IMultipleResults.
        /// </summary>
        /// <param name="reader">The DbDataReader to translate</param>
        /// <returns>The translated sequence of objects</returns>
        public IMultipleResults Translate(DbDataReader reader) {
            CheckDispose();
            if (reader == null) {
                throw Error.ArgumentNull("reader");
            }
            return this.provider.Translate(reader);
        }

        /// <summary>
        /// Remove all Include\Subquery LoadOptions settings.
        /// </summary>
        internal void ResetLoadOptions() {
            CheckDispose();
            this.loadOptions = null;
        }

        /// <summary>
        /// The DataLoadOptions used to define prefetch behavior for defer loaded members
        /// and membership of related collections.
        /// </summary>
        public DataLoadOptions LoadOptions {
            get {
                CheckDispose();
                return this.loadOptions;
            }
            set {
                CheckDispose();
                if (this.services.HasCachedObjects && value != this.loadOptions) {
                    throw Error.LoadOptionsChangeNotAllowedAfterQuery();
                }
                if (value != null) {
                    value.Freeze();
                }
                this.loadOptions = value;
            }
        }

        /// <summary>
        /// This list of change conflicts produced by the last call to SubmitChanges.  Use this collection
        /// to resolve conflicts after catching a ChangeConflictException and before calling SubmitChanges again.
        /// </summary>
        public ChangeConflictCollection ChangeConflicts {
            get {
                CheckDispose(); 
                return this.conflicts;
            }
        }
    }
    
    /// <summary>
    /// Defines behavior for implementations of IQueryable that allow modifications to the membership of the resulting set.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities returned from the queryable.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface ITable<TEntity> : IQueryable<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Notify the set that an object representing a new entity should be added to the set.
        /// Depending on the implementation, the change to the set may not be visible in an enumeration of the set 
        /// until changes to that set have been persisted in some manner.
        /// </summary>
        /// <param name="entity">Entity object to be added.</param>
        void InsertOnSubmit(TEntity entity);

        /// <summary>
        /// Notify the set that an object representing a new entity should be added to the set.
        /// Depending on the implementation, the change to the set may not be visible in an enumeration of the set 
        /// until changes to that set have been persisted in some manner.
        /// </summary>
        /// <param name="entity">Entity object to be attached.</param>
        void Attach(TEntity entity);

        /// <summary>
        /// Notify the set that an object representing an entity should be removed from the set.
        /// Depending on the implementation, the change to the set may not be visible in an enumeration of the set 
        /// until changes to that set have been persisted in some manner.
        /// </summary>
        /// <param name="entity">Entity object to be removed.</param>
        /// <exception cref="InvalidOperationException">Throws if the specified object is not in the set.</exception>
        void DeleteOnSubmit(TEntity entity);
    }

    /// <summary>
    /// ITable is the common interface for DataContext tables. It can be used as the source
    /// of a dynamic/runtime-generated query.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="Microsoft: Meant to represent a database table which is delayed loaded and doesn't provide collection semantics.")]
    public interface ITable : IQueryable {
        /// <summary>
        /// The DataContext containing this Table.
        /// </summary>
        DataContext Context { get; }
        /// <summary>
        /// Adds an entity in a 'pending insert' state to this table.  The added entity will not be observed
        /// in query results from this table until after SubmitChanges has been called. Any untracked
        /// objects referenced directly or transitively by the entity will also be inserted.
        /// </summary>
        /// <param name="entity"></param>
        void InsertOnSubmit(object entity);
        /// <summary>
        /// Adds all entities of a collection to the DataContext in a 'pending insert' state.
        /// The added entities will not be observed in query results until after SubmitChanges() 
        /// has been called. Any untracked objects referenced directly or transitively by the
        /// the inserted entities will also be inserted.
        /// </summary>
        /// <param name="entities"></param>
        void InsertAllOnSubmit(IEnumerable entities);
        /// <summary>
        /// Attaches an entity to the DataContext in an unmodified state, similiar to as if it had been 
        /// retrieved via a query. Other entities accessible from this entity are attached as unmodified 
        /// but may subsequently be transitioned to other states by performing table operations on them
        /// individually.
        /// </summary>
        /// <param name="entity"></param>
        void Attach(object entity);
        /// <summary>
        /// Attaches an entity to the DataContext in either a modified or unmodified state.
        /// If attaching as modified, the entity must either declare a version member or must 
        /// not participate in update conflict checking. Other entities accessible from this 
        /// entity are attached as unmodified but may subsequently be transitioned to other 
        /// states by performing table operations on them individually.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asModified"></param>
        void Attach(object entity, bool asModified);
        /// <summary>
        /// Attaches an entity to the DataContext in either a modified or unmodified state by specifying both the entity
        /// and its original state. Other entities accessible from this 
        /// entity are attached as unmodified but may subsequently be transitioned to other 
        /// states by performing table operations on them individually.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        /// <param name="original">An instance of the same entity type with data members containing
        /// the original values.</param>
        void Attach(object entity, object original);
        /// <summary>
        /// Attaches all entities of a collection to the DataContext in an unmodified state, 
        /// similiar to as if each had been retrieved via a query. Other entities accessible from these 
        /// entities are attached as unmodified but may subsequently be transitioned to other 
        /// states by performing table operations on them individually.
        /// </summary>
        /// <param name="entities"></param>
        void AttachAll(IEnumerable entities);
        /// <summary>
        /// Attaches all entities of a collection to the DataContext in either a modified or unmodified state.
        /// If attaching as modified, the entity must either declare a version member or must not participate in update conflict checking.
        /// Other entities accessible from these 
        /// entities are attached as unmodified but may subsequently be transitioned to other 
        /// states by performing table operations on them individually.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="asModified">True if the entities are to be attach as modified.</param>
        void AttachAll(IEnumerable entities, bool asModified);
        /// <summary>
        /// Puts an entity from this table into a 'pending delete' state.  The removed entity will not be observed
        /// missing from query results until after SubmitChanges() has been called.
        /// </summary>
        /// <param name="entity">The entity to remove.</param>
        void DeleteOnSubmit(object entity);
        /// <summary>
        /// Puts all entities from the collection 'entities' into a 'pending delete' state.  The removed entities will
        /// not be observed missing from the query results until after SubmitChanges() is called.
        /// </summary>
        /// <param name="entities"></param>
        void DeleteAllOnSubmit(IEnumerable entities);
        /// <summary>
        /// Returns an instance containing the original state of the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        object GetOriginalEntityState(object entity);
        /// <summary>
        /// Returns an array of modified members containing their current and original values
        /// for the entity specified.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        ModifiedMemberInfo[] GetModifiedMembers(object entity);
        /// <summary>
        /// True if the table is read-only.
        /// </summary>
        bool IsReadOnly { get; }
    }

    /// <summary>
    /// Table is a collection of persistent entities. It always contains the set of entities currently 
    /// persisted in the database. Use it as a source of queries and to add/insert and remove/delete entities.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="Microsoft: Meant to represent a database table which is delayed loaded and doesn't provide collection semantics.")]
    public sealed class Table<TEntity> : IQueryable<TEntity>, IQueryProvider, IEnumerable<TEntity>, IQueryable, IEnumerable, ITable, IListSource, ITable<TEntity> 
        where TEntity : class {
        DataContext context;
        MetaTable metaTable;

        internal Table(DataContext context, MetaTable metaTable) {
            System.Diagnostics.Debug.Assert(metaTable != null);
            this.context = context;
            this.metaTable = metaTable;
        }

        /// <summary>
        /// The DataContext containing this Table.
        /// </summary>
        public DataContext Context {
            get { return this.context; }
        }

        /// <summary>
        /// True if the table is read-only.
        /// </summary>
        public bool IsReadOnly {
            get { return !metaTable.RowType.IsEntity; }
        }

        Expression IQueryable.Expression {
            get { return Expression.Constant(this); }
        }

        Type IQueryable.ElementType {
            get { return typeof(TEntity); }
        }

        IQueryProvider IQueryable.Provider{
            get{
                return (IQueryProvider)this;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        IQueryable IQueryProvider.CreateQuery(Expression expression) {
            if (expression == null) {
                throw Error.ArgumentNull("expression");
            }
            Type eType = System.Data.Linq.SqlClient.TypeSystem.GetElementType(expression.Type);
            Type qType = typeof(IQueryable<>).MakeGenericType(eType);
            if (!qType.IsAssignableFrom(expression.Type)) {
                throw Error.ExpectedQueryableArgument("expression", qType);
            }
            Type dqType = typeof(DataQuery<>).MakeGenericType(eType);
            return (IQueryable)Activator.CreateInstance(dqType, new object[] { this.context, expression });
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        IQueryable<TResult> IQueryProvider.CreateQuery<TResult>(Expression expression) {
            if (expression == null) {
                throw Error.ArgumentNull("expression");
            }
            if (!typeof(IQueryable<TResult>).IsAssignableFrom(expression.Type)) {
                throw Error.ExpectedQueryableArgument("expression", typeof(IEnumerable<TResult>));
            }
            return new DataQuery<TResult>(this.context, expression);
        }

        object IQueryProvider.Execute(Expression expression) {
            return this.context.Provider.Execute(expression).ReturnValue;
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Microsoft: Generic parameters are required for strong-typing of the return type.")]
        TResult IQueryProvider.Execute<TResult>(Expression expression) {
            return (TResult)this.context.Provider.Execute(expression).ReturnValue;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() {
            return this.GetEnumerator();
        }

        public IEnumerator<TEntity> GetEnumerator() {
            return ((IEnumerable<TEntity>)this.context.Provider.Execute(Expression.Constant(this)).ReturnValue).GetEnumerator();
        }

        bool IListSource.ContainsListCollection {
            get { return false; }
        }

        private IBindingList cachedList;

        IList IListSource.GetList() {
            if (cachedList == null) {
                cachedList = GetNewBindingList();
            }
            return cachedList;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification="Method doesn't represent a property of the type.")]
        public IBindingList GetNewBindingList() {
            return BindingList.Create<TEntity>(this.context, this);
        }

        /// <summary>
        /// Adds an entity in a 'pending insert' state to this table.  The added entity will not be observed
        /// in query results from this table until after SubmitChanges() has been called.  Any untracked
        /// objects referenced directly or transitively by the entity will also be inserted.
        /// </summary>
        /// <param name="entity"></param>
        public void InsertOnSubmit(TEntity entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            MetaType type = this.metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!IsTrackableType(type)) {
                throw Error.TypeCouldNotBeAdded(type.Type);
            }
            TrackedObject tracked = this.context.Services.ChangeTracker.GetTrackedObject(entity);
            if (tracked == null) {
                tracked = this.context.Services.ChangeTracker.Track(entity);
                tracked.ConvertToNew();
            } else if (tracked.IsWeaklyTracked) {
                tracked.ConvertToNew();
            } else if (tracked.IsDeleted) {
                tracked.ConvertToPossiblyModified();
            } else if (tracked.IsRemoved) {
                tracked.ConvertToNew();
            } else if (!tracked.IsNew) {
                throw Error.CantAddAlreadyExistingItem();
            }
        }

        void ITable.InsertOnSubmit(object entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            this.InsertOnSubmit(tEntity);
        }

        /// <summary>
        /// Adds all entities of a collection to the DataContext in a 'pending insert' state.
        /// The added entities will not be observed in query results until after SubmitChanges() 
        /// has been called.
        /// </summary>
        /// <param name="entities"></param>
        public void InsertAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<TSubEntity> list = entities.ToList();
            foreach (TEntity entity in list) {
                this.InsertOnSubmit(entity);
            }
        }

        void ITable.InsertAllOnSubmit(IEnumerable entities) {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<object> list = entities.Cast<object>().ToList();
            ITable itable = this;
            foreach (object entity in list) {
                itable.InsertOnSubmit(entity);
            }
        }

        /// <summary>
        /// Returns true if this specific type is mapped into the database.
        /// For example, an abstract type can't be present because it can not be instantiated.
        /// </summary>
        private static bool IsTrackableType(MetaType type) {
            if (type == null) {
                return false;
            }
            if (!type.CanInstantiate) {
                return false;
            }
            if (type.HasInheritance && !type.HasInheritanceCode) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Puts an entity from this table into a 'pending delete' state.  The removed entity will not be observed
        /// missing from query results until after SubmitChanges() has been called.
        /// </summary>
        /// <param name="item"></param>
        public void DeleteOnSubmit(TEntity entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            TrackedObject tracked = this.context.Services.ChangeTracker.GetTrackedObject(entity);
            if (tracked != null) {
                if (tracked.IsNew) {
                    tracked.ConvertToRemoved();
                }
                else if (tracked.IsPossiblyModified || tracked.IsModified) {
                    tracked.ConvertToDeleted();
                }
            }
            else {
                throw Error.CannotRemoveUnattachedEntity();
            }
        }

        void ITable.DeleteOnSubmit(object entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            this.DeleteOnSubmit(tEntity);
        }

        /// <summary>
        /// Puts all entities from the collection 'entities' into a 'pending delete' state.  The removed entities will
        /// not be observed missing from the query results until after SubmitChanges() is called.
        /// </summary>
        /// <param name="entities"></param>
        public void DeleteAllOnSubmit<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<TSubEntity> list = entities.ToList();
            foreach (TEntity entity in list) {
                this.DeleteOnSubmit(entity);
            }
        }

        void ITable.DeleteAllOnSubmit(IEnumerable entities) {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<object> list = entities.Cast<object>().ToList();
            ITable itable = this;
            foreach (object entity in list) {
                itable.DeleteOnSubmit(entity);
            }
        }

        /// <summary>
        /// Attaches an entity to the DataContext in an unmodified state, similiar to as if it had been 
        /// retrieved via a query. Deferred loading is not enabled. Other entities accessible from this
        /// entity are not automatically attached.
        /// </summary>
        /// <param name="entity"></param>
        public void Attach(TEntity entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            this.Attach(entity, false);
        }

        void ITable.Attach(object entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            this.Attach(tEntity, false);
        }

        /// <summary>
        /// Attaches an entity to the DataContext in either a modified or unmodified state.
        /// If attaching as modified, the entity must either declare a version member or must not participate in update conflict checking.
        /// Deferred loading is not enabled. Other entities accessible from this entity are not automatically attached.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="asModified"></param>
        public void Attach(TEntity entity, bool asModified) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            MetaType type = this.metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!IsTrackableType(type)) {
                throw Error.TypeCouldNotBeTracked(type.Type);
            }
            if (asModified) {
                bool canAttach = type.VersionMember != null || !type.HasUpdateCheck;
                if (!canAttach) {
                    throw Error.CannotAttachAsModifiedWithoutOriginalState();
                }
            }
            TrackedObject tracked = this.Context.Services.ChangeTracker.GetTrackedObject(entity);
            if (tracked == null || tracked.IsWeaklyTracked) {
                if (tracked == null) {
                    tracked = this.context.Services.ChangeTracker.Track(entity, true);
                }
                if (asModified) {
                    tracked.ConvertToModified();
                } else {
                    tracked.ConvertToUnmodified();
                }
                if (this.Context.Services.InsertLookupCachedObject(type, entity) != entity) {
                    throw new DuplicateKeyException(entity, Strings.CantAddAlreadyExistingKey);
                }
                tracked.InitializeDeferredLoaders();
            }
            else {
                throw Error.CannotAttachAlreadyExistingEntity();
            }
        }

        void ITable.Attach(object entity, bool asModified) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            this.Attach(tEntity, asModified);
        }

        /// <summary>
        /// Attaches an entity to the DataContext in either a modified or unmodified state by specifying both the entity
        /// and its original state.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        /// <param name="original">An instance of the same entity type with data members containing
        /// the original values.</param>
        public void Attach(TEntity entity, TEntity original) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            if (original == null) {
                throw Error.ArgumentNull("original");
            }
            if (entity.GetType() != original.GetType()) {
                throw Error.OriginalEntityIsWrongType();
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            MetaType type = this.metaTable.RowType.GetInheritanceType(entity.GetType());
            if (!IsTrackableType(type)) {
                throw Error.TypeCouldNotBeTracked(type.Type);
            }
            TrackedObject tracked = this.context.Services.ChangeTracker.GetTrackedObject(entity);
            if (tracked == null || tracked.IsWeaklyTracked) {
                if (tracked == null) {
                    tracked = this.context.Services.ChangeTracker.Track(entity, true);
                }
                tracked.ConvertToPossiblyModified(original);
                if (this.Context.Services.InsertLookupCachedObject(type, entity) != entity) {
                    throw new DuplicateKeyException(entity, Strings.CantAddAlreadyExistingKey);
                }
                tracked.InitializeDeferredLoaders();
            }
            else {
                throw Error.CannotAttachAlreadyExistingEntity();
            }
        }

        void ITable.Attach(object entity, object original) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            if (original == null) {
                throw Error.ArgumentNull("original");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            if (entity.GetType() != original.GetType()) {
                throw Error.OriginalEntityIsWrongType();
            }
            this.Attach(tEntity, (TEntity)original);
        }

        /// <summary>
        /// Attaches all entities of a collection to the DataContext in an unmodified state, 
        /// similiar to as if each had been retrieved via a query. Deferred loading is not enabled. 
        /// Other entities accessible from these entities are not automatically attached.
        /// </summary>
        /// <param name="entities"></param>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities) where TSubEntity : TEntity {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            this.AttachAll(entities, false);
        }

        void ITable.AttachAll(IEnumerable entities) {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            ((ITable)this).AttachAll(entities, false);
        }

        /// <summary>
        /// Attaches all entities of a collection to the DataContext in either a modified or unmodified state.
        /// If attaching as modified, the entity must either declare a version member or must not participate in update conflict checking.
        /// Deferred loading is not enabled.  Other entities accessible from these entities are not automatically attached.
        /// </summary>
        /// <param name="entities">The collection of entities.</param>
        /// <param name="asModified">True if the entities are to be attach as modified.</param>
        public void AttachAll<TSubEntity>(IEnumerable<TSubEntity> entities, bool asModified) where TSubEntity : TEntity {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<TSubEntity> list = entities.ToList();
            foreach (TEntity entity in list) {
                this.Attach(entity, asModified);
            }
        }

        void ITable.AttachAll(IEnumerable entities, bool asModified) {
            if (entities == null) {
                throw Error.ArgumentNull("entities");
            }
            CheckReadOnly();
            context.CheckNotInSubmitChanges();
            context.VerifyTrackingEnabled();
            List<object> list = entities.Cast<object>().ToList();
            ITable itable = this;
            foreach (object entity in list) {
                itable.Attach(entity, asModified);
            }
        }

        /// <summary>
        /// Returns an instance containing the original state of the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity GetOriginalEntityState(TEntity entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            MetaType type = this.Context.Mapping.GetMetaType(entity.GetType());
            if (type == null || !type.IsEntity) {
                throw Error.EntityIsTheWrongType();
            }
            TrackedObject tracked = this.Context.Services.ChangeTracker.GetTrackedObject(entity);
            if (tracked != null) {
                if (tracked.Original != null) {
                    return (TEntity) tracked.CreateDataCopy(tracked.Original);
                }
                else {
                    return (TEntity) tracked.CreateDataCopy(tracked.Current);
                }
            }
            return null;
        }

        object ITable.GetOriginalEntityState(object entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            return this.GetOriginalEntityState(tEntity);
        }

        /// <summary>
        /// Returns an array of modified members containing their current and original values
        /// for the entity specified.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ModifiedMemberInfo[] GetModifiedMembers(TEntity entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            MetaType type = this.Context.Mapping.GetMetaType(entity.GetType());
            if (type == null || !type.IsEntity) {
                throw Error.EntityIsTheWrongType();
            }
            TrackedObject tracked = this.Context.Services.ChangeTracker.GetTrackedObject(entity);
            if (tracked != null) {
                return tracked.GetModifiedMembers().ToArray();
            }
            return new ModifiedMemberInfo[] { };
        }

        ModifiedMemberInfo[] ITable.GetModifiedMembers(object entity) {
            if (entity == null) {
                throw Error.ArgumentNull("entity");
            }
            TEntity tEntity = entity as TEntity;
            if (tEntity == null) {
                throw Error.EntityIsTheWrongType();
            }
            return this.GetModifiedMembers(tEntity);
        }

        private void CheckReadOnly() {
            if (this.IsReadOnly) {
                throw Error.CannotPerformCUDOnReadOnlyTable(ToString());
            }
        }

        public override string ToString() {
            return "Table(" + typeof(TEntity).Name + ")";
        }
    }

    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ChangeSet", Justification="The capitalization was deliberately chosen.")]
    public sealed class ChangeSet {
        ReadOnlyCollection<object> inserts;
        ReadOnlyCollection<object> deletes;
        ReadOnlyCollection<object> updates;

        internal ChangeSet(
            ReadOnlyCollection<object> inserts,
            ReadOnlyCollection<object> deletes,
            ReadOnlyCollection<object> updates
            ) {
            this.inserts = inserts;
            this.deletes = deletes;
            this.updates = updates;
        }

        public IList<object> Inserts {
            get { return this.inserts; }
        }

        public IList<object> Deletes {
            get { return this.deletes; }
        }

        public IList<object> Updates {
            get { return this.updates; }
        }

        public override string ToString() {
            return "{" +
                string.Format(
                    Globalization.CultureInfo.InvariantCulture,
                    "Inserts: {0}, Deletes: {1}, Updates: {2}",
                    this.Inserts.Count,
                    this.Deletes.Count,
                    this.Updates.Count
                    ) + "}";
        }
    }

    [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Microsoft: Types are never compared to each other.  When comparisons happen it is against the entities that are represented by these constructs.")]
    public struct ModifiedMemberInfo {
        MemberInfo member;
        object current;
        object original;

        internal ModifiedMemberInfo(MemberInfo member, object current, object original) {
            this.member = member;
            this.current = current;
            this.original = original;
        }

        public MemberInfo Member {
            get { return this.member; }
        }

        public object CurrentValue {
            get { return this.current; }
        }

        public object OriginalValue {
            get { return this.original; }
        }
    }
}
