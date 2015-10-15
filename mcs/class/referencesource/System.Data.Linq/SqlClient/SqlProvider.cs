using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Me = System.Data.Linq.SqlClient;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;

namespace System.Data.Linq.SqlClient {
    public sealed class Sql2000Provider : SqlProvider {
        public Sql2000Provider()
            : base(ProviderMode.Sql2000) {
        }
    }

    public sealed class Sql2005Provider : SqlProvider {
        public Sql2005Provider()
            : base(ProviderMode.Sql2005) {
        }
    }

    public sealed class Sql2008Provider : SqlProvider {
        public Sql2008Provider()
            : base(ProviderMode.Sql2008) {
        }
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="Unknown reason.")]
    public class SqlProvider : IReaderProvider, IConnectionUser {
        private IDataServices services;
        private SqlConnectionManager conManager;
        private TypeSystemProvider typeProvider;
        private SqlFactory sqlFactory;
        private Translator translator;
        private IObjectReaderCompiler readerCompiler;
        private bool disposed;
        private int commandTimeout;

        private TextWriter log;
        string dbName = string.Empty;

        // stats and flags
        private int queryCount;
        private bool checkQueries;
        private OptimizationFlags optimizationFlags = OptimizationFlags.All;
        private bool enableCacheLookup = true;
        private ProviderMode mode = ProviderMode.NotYetDecided;
        private bool deleted = false;

#if PERFORMANCE_BUILD
        private bool collectPerfInfo;
        private bool collectPerfInfoInitialized = false;
        private bool collectQueryPerf;

        internal bool CollectPerfInfo {
            get { 
                if (!collectPerfInfoInitialized) {
                    string s = System.Environment.GetEnvironmentVariable("CollectDLinqPerfInfo");
                    collectPerfInfo = (s != null) && (s == "On");
                    collectPerfInfoInitialized = true;
                }
                return this.collectPerfInfo; 
            }
        }

        internal bool CollectQueryPerf {
            get { return this.collectQueryPerf; }
        }
#endif

        internal enum ProviderMode {
            NotYetDecided,
            Sql2000,
            Sql2005,
            Sql2008,
            SqlCE
        }

        const string SqlCeProviderInvariantName = "System.Data.SqlServerCe.3.5";
        const string SqlCeDataReaderTypeName = "System.Data.SqlServerCe.SqlCeDataReader";
        const string SqlCeConnectionTypeName = "System.Data.SqlServerCe.SqlCeConnection";
        const string SqlCeTransactionTypeName = "System.Data.SqlServerCe.SqlCeTransaction";

        internal ProviderMode Mode {
            get {
                this.CheckDispose();
                this.CheckInitialized();
                this.InitializeProviderMode();
                return this.mode; 
            }
        }
        
        private void InitializeProviderMode() {
            if (this.mode == ProviderMode.NotYetDecided) {
                if (this.IsSqlCe) {
                    this.mode = ProviderMode.SqlCE;
                } else if (this.IsServer2KOrEarlier) {
                    this.mode = ProviderMode.Sql2000;
                }
                else if (this.IsServer2005) {
                    this.mode = ProviderMode.Sql2005;
                } else {
                    this.mode = ProviderMode.Sql2008;
                }
            }
            if (this.typeProvider == null) {
                switch (this.mode) {
                    case ProviderMode.Sql2000:
                        this.typeProvider = SqlTypeSystem.Create2000Provider();
                        break;
                    case ProviderMode.Sql2005:
                        this.typeProvider = SqlTypeSystem.Create2005Provider();
                        break;
                    case ProviderMode.Sql2008:
                        this.typeProvider = SqlTypeSystem.Create2008Provider();
                        break;
                    case ProviderMode.SqlCE:
                        this.typeProvider = SqlTypeSystem.CreateCEProvider();
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }
            }
            if (this.sqlFactory == null) {
                this.sqlFactory = new SqlFactory(this.typeProvider, this.services.Model);
                this.translator = new Translator(this.services, this.sqlFactory, this.typeProvider);
            }
        }

        /// <summary>
        /// Return true if the current connection is SQLCE.
        /// </summary>
        private bool IsSqlCe {
            get {
                DbConnection con = conManager.UseConnection(this);
                try {
                    if (String.CompareOrdinal(con.GetType().FullName, SqlCeConnectionTypeName) == 0) {
                        return true;
                    }
                } finally {
                    conManager.ReleaseConnection(this);
                }
                return false;
            }
        }
        
        /// <summary>
        /// Return true if this is a 2K (or earlier) server. This may be a round trip to the server.
        /// </summary>
        private bool IsServer2KOrEarlier {
            get {
                DbConnection con = conManager.UseConnection(this);
                try {
                    string serverVersion = con.ServerVersion;
                    if (serverVersion.StartsWith("06.00.", StringComparison.Ordinal)) {
                        return true;
                    }
                    else if (serverVersion.StartsWith("06.50.", StringComparison.Ordinal)) {
                        return true;
                    }
                    else if (serverVersion.StartsWith("07.00.", StringComparison.Ordinal)) {
                        return true;
                    }
                    else if (serverVersion.StartsWith("08.00.", StringComparison.Ordinal)) {
                        return true;
                    }
                    return false;
                }
                finally {
                    conManager.ReleaseConnection(this);
                }
            }
        }

        /// <summary>
        /// Return true if this is a SQL 2005 server. This may be a round trip to the server.
        /// </summary>
        private bool IsServer2005 {
            get {
                DbConnection con = conManager.UseConnection(this);
                try {
                    string serverVersion = con.ServerVersion;
                    if (serverVersion.StartsWith("09.00.", StringComparison.Ordinal)) {
                        return true;
                    }
                    return false;
                }
                finally {
                    conManager.ReleaseConnection(this);
                }
            }
        }

        DbConnection IProvider.Connection {
            get {
                this.CheckDispose();
                this.CheckInitialized();
                return this.conManager.Connection;
            }
        }

        TextWriter IProvider.Log {
            get {
                this.CheckDispose();
                this.CheckInitialized();
                return this.log;
            }
            set {
                this.CheckDispose();
                this.CheckInitialized();
                this.log = value;
            }
        }

        DbTransaction IProvider.Transaction {
            get {
                this.CheckDispose();
                this.CheckInitialized();
                return this.conManager.Transaction;
            }
            set {
                this.CheckDispose();
                this.CheckInitialized();
                this.conManager.Transaction = value;
            }
        }

        int IProvider.CommandTimeout {
            get {
                this.CheckDispose();
                return this.commandTimeout; 
            }
            set {
                this.CheckDispose();
                this.commandTimeout = value; 
            }
        }

        /// <summary>
        /// Expose a test hook which controls which SQL optimizations are executed.
        /// </summary>
        internal OptimizationFlags OptimizationFlags {
            get { 
                CheckDispose();
                return this.optimizationFlags; 
            }
            set { 
                CheckDispose();
                this.optimizationFlags = value; 
            }
        }

        /// <summary>
        /// Validate queries as they are generated.
        /// </summary>
        internal bool CheckQueries {
            get { 
                CheckDispose();
                return checkQueries; 
            }
            set { 
                CheckDispose();
                checkQueries = value; 
            }
        }

        internal bool EnableCacheLookup {
            get { 
                CheckDispose();
                return this.enableCacheLookup; 
            }
            set { 
                CheckDispose();
                this.enableCacheLookup = value; 
            }
        }

        internal int QueryCount {
            get { 
                CheckDispose();
                return this.queryCount; 
            }
        }

        internal int MaxUsers {
            get {
                CheckDispose();
                return this.conManager.MaxUsers;
            }
        }

        IDataServices IReaderProvider.Services {
            get { return this.services; }
        }

        IConnectionManager IReaderProvider.ConnectionManager {
            get { return this.conManager; }
        }

        public SqlProvider() {
            this.mode = ProviderMode.NotYetDecided;
        }

        internal SqlProvider(ProviderMode mode) {
            this.mode = mode;
        }

        private void CheckInitialized() {
            if (this.services == null) {
                throw Error.ContextNotInitialized();
            }
        }
        private void CheckNotDeleted() {
            if (this.deleted) {
                throw Error.DatabaseDeleteThroughContext();
            }
        }

        [ResourceExposure(ResourceScope.Machine)] // connection parameter may refer to filenames.
        void IProvider.Initialize(IDataServices dataServices, object connection) {
            if (dataServices == null) {
                throw Error.ArgumentNull("dataServices");
            }
            this.services = dataServices;

            DbConnection con;
            DbTransaction tx = null;

            string fileOrServerOrConnectionString = connection as string;
            if (fileOrServerOrConnectionString != null) {
                string connectionString = this.GetConnectionString(fileOrServerOrConnectionString);
                this.dbName = this.GetDatabaseName(connectionString);
                if (this.dbName.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase)) {
                    this.mode = ProviderMode.SqlCE;
                }
                if (this.mode == ProviderMode.SqlCE) {
                    DbProviderFactory factory = SqlProvider.GetProvider(SqlCeProviderInvariantName);
                    if (factory == null) {
                        throw Error.ProviderNotInstalled(this.dbName, SqlCeProviderInvariantName);
                    }
                    con = factory.CreateConnection();
                }
                else {
                    con = new SqlConnection();
                }
                con.ConnectionString = connectionString;
            }
            else {
                // We only support SqlTransaction and SqlCeTransaction
                tx = connection as SqlTransaction;
                if (tx == null) {
                    // See if it's a SqlCeTransaction
                    if (connection.GetType().FullName == SqlCeTransactionTypeName) {
                        tx = connection as DbTransaction;
                    }
                }
                if (tx != null) {
                    connection = tx.Connection;
                }
                con = connection as DbConnection;
                if (con == null) {
                    throw Error.InvalidConnectionArgument("connection");
                }
                if (con.GetType().FullName == SqlCeConnectionTypeName) {
                    this.mode = ProviderMode.SqlCE;
                }
                this.dbName = this.GetDatabaseName(con.ConnectionString);
            }
            
            // initialize to the default command timeout
            using (DbCommand c = con.CreateCommand()) {
                this.commandTimeout = c.CommandTimeout;
            }

            int maxUsersPerConnection = 1;
            if (con.ConnectionString.IndexOf("MultipleActiveResultSets", StringComparison.OrdinalIgnoreCase) >= 0) {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder.ConnectionString = con.ConnectionString;
                if (string.Compare((string)builder["MultipleActiveResultSets"], "true", StringComparison.OrdinalIgnoreCase) == 0) {
                    maxUsersPerConnection = 10;
                }
            }

            // If fileOrServerOrConnectionString != null, that means we just created the connection instance and we have to tell
            // the SqlConnectionManager that it should dispose the connection when the context is disposed. Otherwise the user owns
            // the connection and should dispose of it themselves.
            this.conManager = new SqlConnectionManager(this, con, maxUsersPerConnection, fileOrServerOrConnectionString != null /*disposeConnection*/);
            if (tx != null) {
                this.conManager.Transaction = tx;
            }

#if DEBUG
            SqlNode.Formatter = new SqlFormatter();
#endif

#if ILGEN
            Type readerType;
            if (this.mode == ProviderMode.SqlCE) {
                readerType = con.GetType().Module.GetType(SqlCeDataReaderTypeName);
            }
            else if (con is SqlConnection) {
                readerType = typeof(SqlDataReader);
            }
            else {
                readerType = typeof(DbDataReader);
            }
            this.readerCompiler = new ObjectReaderCompiler(readerType, this.services);
#else
            this.readerCompiler = new ObjectReaderBuilder(this, this.services);
#endif
        }

        private static DbProviderFactory GetProvider(string providerName) {
            bool hasProvider = 
                DbProviderFactories.GetFactoryClasses().Rows.OfType<DataRow>()
                .Select(r => (string)r["InvariantName"])
                .Contains(providerName, StringComparer.OrdinalIgnoreCase);
            if (hasProvider) {
                return DbProviderFactories.GetFactory(providerName);
            }
            return null;
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
                this.services = null;
                if (this.conManager != null) {
                    this.conManager.DisposeConnection();
                }
                this.conManager = null;
                this.typeProvider = null;
                this.sqlFactory = null;
                this.translator = null;
                this.readerCompiler = null;
                this.log = null;
            }
        }

        internal void CheckDispose() {
            if (this.disposed) {
                throw Error.ProviderCannotBeUsedAfterDispose();
            }
        }
        #endregion

        private string GetConnectionString(string fileOrServerOrConnectionString) {
            if (fileOrServerOrConnectionString.IndexOf('=') >= 0) {
                return fileOrServerOrConnectionString;
            }
            else {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                if (fileOrServerOrConnectionString.EndsWith(".mdf", StringComparison.OrdinalIgnoreCase)) {
                    // if just a database file is specified, default to local SqlExpress instance
                    builder.Add("AttachDBFileName", fileOrServerOrConnectionString);
                    builder.Add("Server", "localhost\\sqlexpress");
                    builder.Add("Integrated Security", "SSPI");
                    builder.Add("User Instance", "true");
                    builder.Add("MultipleActiveResultSets", "true");
                }
                else if (fileOrServerOrConnectionString.EndsWith(".sdf", StringComparison.OrdinalIgnoreCase)) {
                    // A SqlCE database file has been specified
                    builder.Add("Data Source", fileOrServerOrConnectionString);
                }
                else {
                    builder.Add("Server", fileOrServerOrConnectionString);
                    builder.Add("Database", this.services.Model.DatabaseName);
                    builder.Add("Integrated Security", "SSPI");
                }
                return builder.ToString();
            }
        }

        private string GetDatabaseName(string constr) {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = constr;

            if (builder.ContainsKey("Initial Catalog")) {
                return (string)builder["Initial Catalog"];
            }
            else if (builder.ContainsKey("Database")) {
                return (string)builder["Database"];
            }
            else if (builder.ContainsKey("AttachDBFileName")) {
                return (string)builder["AttachDBFileName"];
            }
            else if (builder.ContainsKey("Data Source") 
                && ((string)builder["Data Source"]).EndsWith(".sdf", StringComparison.OrdinalIgnoreCase)) {
                return (string)builder["Data Source"];
            }
            else {
                return this.services.Model.DatabaseName;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        [ResourceExposure(ResourceScope.None)] // Exposure is via other methods that set dbName.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] // File.Exists method call.
        void IProvider.CreateDatabase() {
            this.CheckDispose();
            this.CheckInitialized();
            // Don't need to call CheckNotDeleted() here since we allow CreateDatabase after DeleteDatabase
            // Don't need to call InitializeProviderMode() here since we don't need to know the provider to do this.
            string catalog = null;
            string filename = null;

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.ConnectionString = this.conManager.Connection.ConnectionString;

            if (this.conManager.Connection.State == ConnectionState.Closed) {
                if (this.mode == ProviderMode.SqlCE) {
                    if (!File.Exists(this.dbName)) {
                        Type engineType = this.conManager.Connection.GetType().Module.GetType("System.Data.SqlServerCe.SqlCeEngine");
                        object engine = Activator.CreateInstance(engineType, new object[] { builder.ToString() });
                        try {
                            engineType.InvokeMember("CreateDatabase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, engine, new object[] { }, CultureInfo.InvariantCulture);
                        } 
                        catch (TargetInvocationException tie) {
                            throw tie.InnerException;
                        } 
                        finally {
                            IDisposable disp = engine as IDisposable;
                            if (disp != null) {
                                disp.Dispose();
                            }
                        }
                    } 
                    else {
                        throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
                    }
                } 
                else {
                    // get connection string w/o reference to new catalog
                    object val;
                    if (builder.TryGetValue("Initial Catalog", out val)) {
                        catalog = val.ToString();
                        builder.Remove("Initial Catalog");
                    }
                    if (builder.TryGetValue("Database", out val)) {
                        catalog = val.ToString();
                        builder.Remove("Database");
                    }
                    if (builder.TryGetValue("AttachDBFileName", out val)) {
                        filename = val.ToString();
                        builder.Remove("AttachDBFileName");
                    }
                }
                this.conManager.Connection.ConnectionString = builder.ToString();
            }
            else {
                if (this.mode == ProviderMode.SqlCE) {
                    if (File.Exists(this.dbName)) {
                        throw Error.CreateDatabaseFailedBecauseSqlCEDatabaseAlreadyExists(this.dbName);
                    }
                }
                object val;
                if (builder.TryGetValue("Initial Catalog", out val)) {
                    catalog = val.ToString();
                }
                if (builder.TryGetValue("Database", out val)) {
                    catalog = val.ToString();
                }
                if (builder.TryGetValue("AttachDBFileName", out val)) {
                    filename = val.ToString();
                }
            }

            if (String.IsNullOrEmpty(catalog)) {
                if (!String.IsNullOrEmpty(filename)) {
                    catalog = Path.GetFullPath(filename);
                } 
                else if (!String.IsNullOrEmpty(this.dbName)) {
                    catalog = this.dbName;
                } 
                else {
                    throw Error.CouldNotDetermineCatalogName();
                }
            }

            this.conManager.UseConnection(this);
            this.conManager.AutoClose = false;

            try {
                if (this.services.Model.GetTables().FirstOrDefault() == null) {
                    // we have no tables to create
                    throw Error.CreateDatabaseFailedBecauseOfContextWithNoTables(this.services.Model.DatabaseName);
                }

                this.deleted = false;

                // create database
                if (this.mode == ProviderMode.SqlCE) {

                    // create tables
                    foreach (MetaTable table in this.services.Model.GetTables()) {
                        string command = SqlBuilder.GetCreateTableCommand(table);
                        if (!String.IsNullOrEmpty(command)) {
                            this.ExecuteCommand(command);
                        }
                    }
                    // create all foreign keys after all tables are defined
                    foreach (MetaTable table in this.services.Model.GetTables()) {
                        foreach (string command in SqlBuilder.GetCreateForeignKeyCommands(table)) {
                            if (!String.IsNullOrEmpty(command)) {
                                this.ExecuteCommand(command);
                            }
                        }
                    }
                }
                else {
                    string createdb = SqlBuilder.GetCreateDatabaseCommand(catalog, filename, Path.ChangeExtension(filename, ".ldf"));
                    this.ExecuteCommand(createdb);
                    this.conManager.Connection.ChangeDatabase(catalog);

                    // create the schemas that our tables will need
                    // cannot be batched together with the rest of the CREATE TABLES
                    if (this.mode == ProviderMode.Sql2005 || this.mode == ProviderMode.Sql2008) {
                        HashSet<string> schemaCommands = new HashSet<string>();

                        foreach (MetaTable table in this.services.Model.GetTables()) {
                            string schemaCommand = SqlBuilder.GetCreateSchemaForTableCommand(table);
                            if (!string.IsNullOrEmpty(schemaCommand)) {
                                schemaCommands.Add(schemaCommand);
                            }
                        }

                        foreach (string schemaCommand in schemaCommands) {
                            this.ExecuteCommand(schemaCommand);
                        }
                    }

                    StringBuilder sb = new StringBuilder();

                    // create tables
                    foreach (MetaTable table in this.services.Model.GetTables()) {
                        string createTable = SqlBuilder.GetCreateTableCommand(table);
                        if (!string.IsNullOrEmpty(createTable)) {
                            sb.AppendLine(createTable);
                        }
                    }

                    // create all foreign keys after all tables are defined
                    foreach (MetaTable table in this.services.Model.GetTables()) {
                        foreach (string createFK in SqlBuilder.GetCreateForeignKeyCommands(table)) {
                            if (!string.IsNullOrEmpty(createFK)) {
                                sb.AppendLine(createFK);
                            }
                        }
                    }

                    if (sb.Length > 0) {
                        // must be on when creating indexes on computed columns
                        sb.Insert(0, "SET ARITHABORT ON" + Environment.NewLine);
                        this.ExecuteCommand(sb.ToString());
                    }
                }
            }
            finally {
                this.conManager.ReleaseConnection(this);
                if (this.conManager.Connection is SqlConnection) {
                    SqlConnection.ClearAllPools();
                }
            }
        }

        [ResourceExposure(ResourceScope.None)] // Exposure is via other methods that set dbName.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] // File.Delete method call.
        void IProvider.DeleteDatabase() {
            this.CheckDispose();
            this.CheckInitialized();
            // Don't need to call InitializeProviderMode() here since we don't need to know the provider to do this.
            if (this.deleted) {
                // 2nd delete is no-op.
                return;
            }

            if (this.mode == ProviderMode.SqlCE) {
                ((IProvider)this).ClearConnection();
                System.Diagnostics.Debug.Assert(this.conManager.Connection.State == ConnectionState.Closed);
                File.Delete(this.dbName);
                this.deleted = true;
            }
            else {
                string holdConnStr = conManager.Connection.ConnectionString;
                DbConnection con = this.conManager.UseConnection(this);
                try {
                    con.ChangeDatabase("master");
                    if (con is SqlConnection) {
                        SqlConnection.ClearAllPools();
                    }
                    if (this.log != null) {
                        this.log.WriteLine(Strings.LogAttemptingToDeleteDatabase(this.dbName));
                    }
                    this.ExecuteCommand(SqlBuilder.GetDropDatabaseCommand(this.dbName));
                    this.deleted = true;
                }
                finally {
                    this.conManager.ReleaseConnection(this);
                    if (conManager.Connection.State == ConnectionState.Closed &&
                        string.Compare(conManager.Connection.ConnectionString, holdConnStr, StringComparison.Ordinal) != 0) {
                        // Credential information may have been stripped from the connection
                        // string as a result of opening the connection. Restore the full
                        // connection string.
                        conManager.Connection.ConnectionString = holdConnStr;
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="Microsoft: Code needs to return false regarless of exception.")]
        [ResourceExposure(ResourceScope.None)] // Exposure is via other methods that set dbName.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] // File.Exists method call.
        bool IProvider.DatabaseExists() {
            this.CheckDispose();
            this.CheckInitialized();
            if (this.deleted) {
                return false;
            }
            // Don't need to call InitializeProviderMode() here since we don't need to know the provider to do this.

            bool exists = false;
            if (this.mode == ProviderMode.SqlCE) {
                exists = File.Exists(this.dbName);
            }
            else {
                string holdConnStr = conManager.Connection.ConnectionString;
                try {
                    // If no database name is explicitly specified on the connection,
                    // UseConnection will connect to 'Master', which is why after connecting
                    // we call ChangeDatabase to verify that the database actually exists.
                    this.conManager.UseConnection(this);
                    this.conManager.Connection.ChangeDatabase(this.dbName);
                    this.conManager.ReleaseConnection(this);
                    exists = true;
                } catch (Exception) {
                } finally {
                    if (conManager.Connection.State == ConnectionState.Closed &&
                        string.Compare(conManager.Connection.ConnectionString, holdConnStr, StringComparison.Ordinal) != 0) {
                        // Credential information may have been stripped from the connection
                        // string as a result of opening the connection. Restore the full
                        // connection string.
                        conManager.Connection.ConnectionString = holdConnStr;
                    }
                }
            }
            return exists;
        }

        void IConnectionUser.CompleteUse() {
        }

        void IProvider.ClearConnection() {
            this.CheckDispose();
            this.CheckInitialized();
            this.conManager.ClearConnection();
        }

        private void ExecuteCommand(string command) {
            if (this.log != null) {
                this.log.WriteLine(command);
                this.log.WriteLine();
            }
            IDbCommand cmd = this.conManager.Connection.CreateCommand();
            cmd.CommandTimeout = this.commandTimeout;
            cmd.Transaction = this.conManager.Transaction;
            cmd.CommandText = command;
            cmd.ExecuteNonQuery();
        }

        ICompiledQuery IProvider.Compile(Expression query) {
            this.CheckDispose();
            this.CheckInitialized();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            this.InitializeProviderMode();

            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            QueryInfo[] qis = this.BuildQuery(query, annotations);
            CheckSqlCompatibility(qis, annotations);
            
            LambdaExpression lambda = query as LambdaExpression;
            if (lambda != null) {
                query = lambda.Body;
            }

            IObjectReaderFactory factory = null;
            ICompiledSubQuery[] subQueries = null;
            QueryInfo qi = qis[qis.Length - 1];
            if (qi.ResultShape == ResultShape.Singleton) {
                subQueries = this.CompileSubQueries(qi.Query);
                factory = this.GetReaderFactory(qi.Query, qi.ResultType);
            }
            else if (qi.ResultShape == ResultShape.Sequence) {
                subQueries = this.CompileSubQueries(qi.Query);
                factory = this.GetReaderFactory(qi.Query, TypeSystem.GetElementType(qi.ResultType));
            }

            return new CompiledQuery(this, query, qis, factory, subQueries);
        }

        private ICompiledSubQuery CompileSubQuery(SqlNode query, Type elementType, ReadOnlyCollection<Me.SqlParameter> parameters) {
            query = SqlDuplicator.Copy(query);
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();

            QueryInfo[] qis = this.BuildQuery(ResultShape.Sequence, TypeSystem.GetSequenceType(elementType), query, parameters, annotations);
            System.Diagnostics.Debug.Assert(qis.Length == 1);
            QueryInfo qi = qis[0];
            ICompiledSubQuery[] subQueries = this.CompileSubQueries(qi.Query);
            IObjectReaderFactory factory = this.GetReaderFactory(qi.Query, elementType);

            CheckSqlCompatibility(qis, annotations);

            return new CompiledSubQuery(qi, factory, parameters, subQueries);
        }

        IExecuteResult IProvider.Execute(Expression query) {
            this.CheckDispose();
            this.CheckInitialized();
            this.CheckNotDeleted();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            this.InitializeProviderMode();

#if PERFORMANCE_BUILD
            PerformanceCounter pcBuildQuery = null, bpcBuildQuery = null, pcExecQuery = null, bpcExecQuery = null,
                   pcSession = null, bpcSession = null;
            PerfTimer timerAll = null, timer = null;
            if (this.CollectPerfInfo) {
                string s = System.Environment.GetEnvironmentVariable("EnableDLinqQueryPerf");
                collectQueryPerf = (s != null && s == "On");
            }
            if (collectQueryPerf) {
                pcBuildQuery = new PerformanceCounter("DLinq", "BuildQueryElapsedTime", false);
                bpcBuildQuery = new PerformanceCounter("DLinq", "BuildQueryElapsedTimeBase", false);
                pcExecQuery = new PerformanceCounter("DLinq", "ExecuteQueryElapsedTime", false);
                bpcExecQuery = new PerformanceCounter("DLinq", "ExecuteQueryElapsedTimeBase", false);
                pcSession = new PerformanceCounter("DLinq", "SessionExecuteQueryElapsedTime", false);
                bpcSession = new PerformanceCounter("DLinq", "SessionExecuteQueryElapsedTimeBase", false);
                timerAll = new PerfTimer();
                timer = new PerfTimer();
                timerAll.Start();
            }
#endif
            query = Funcletizer.Funcletize(query);

            if (this.EnableCacheLookup) {
                IExecuteResult cached = this.GetCachedResult(query);
                if (cached != null) {
                    return cached;
                }
            }

#if PERFORMANCE_BUILD
            if (collectQueryPerf) {
                timer.Start();
            }
#endif
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            QueryInfo[] qis = this.BuildQuery(query, annotations);
            CheckSqlCompatibility(qis, annotations);

            LambdaExpression lambda = query as LambdaExpression;
            if (lambda != null) {
                query = lambda.Body;
            }

            IObjectReaderFactory factory = null;
            ICompiledSubQuery[] subQueries = null;
            QueryInfo qi = qis[qis.Length - 1];
            if (qi.ResultShape == ResultShape.Singleton) {
                subQueries = this.CompileSubQueries(qi.Query);
                factory = this.GetReaderFactory(qi.Query, qi.ResultType);
            }
            else if (qi.ResultShape == ResultShape.Sequence) {
                subQueries = this.CompileSubQueries(qi.Query);
                factory = this.GetReaderFactory(qi.Query, TypeSystem.GetElementType(qi.ResultType));
            }

#if PERFORMANCE_BUILD
                if (collectQueryPerf) {
                    timer.Stop();
                    pcBuildQuery.IncrementBy(timer.Duration);
                    bpcBuildQuery.Increment();
                }
#endif

#if PERFORMANCE_BUILD
            if (collectQueryPerf) {
                timer.Start();
            }

#endif
            IExecuteResult result = this.ExecuteAll(query, qis, factory, null, subQueries);

#if PERFORMANCE_BUILD
            if (collectQueryPerf) {
                timer.Stop();
                pcSession.IncrementBy(timer.Duration);
                bpcSession.Increment();
                timerAll.Stop();
                pcExecQuery.IncrementBy(timerAll.Duration);
                bpcExecQuery.Increment();
            }
#endif
            return result;
        }

        private ICompiledSubQuery[] CompileSubQueries(SqlNode query) {
            return new SubQueryCompiler(this).Compile(query);            
        }

        class SubQueryCompiler : SqlVisitor {
            SqlProvider provider;
            List<ICompiledSubQuery> subQueries;

            internal SubQueryCompiler(SqlProvider provider) {
                this.provider = provider;
            }

            internal ICompiledSubQuery[] Compile(SqlNode node) {
                this.subQueries = new List<ICompiledSubQuery>();
                this.Visit(node);
                return this.subQueries.ToArray();
            }

            internal override SqlSelect VisitSelect(SqlSelect select) {
                this.Visit(select.Selection);
                return select;
            }

            internal override SqlExpression VisitSubSelect(SqlSubSelect ss) {
                return ss;
            }

            internal override SqlExpression VisitClientQuery(SqlClientQuery cq) {
                Type clientElementType = cq.Query.NodeType == SqlNodeType.Multiset ? TypeSystem.GetElementType(cq.ClrType) : cq.ClrType;
                ICompiledSubQuery c = this.provider.CompileSubQuery(cq.Query.Select, clientElementType, cq.Parameters.AsReadOnly());
                cq.Ordinal = this.subQueries.Count;
                this.subQueries.Add(c);
                return cq;
            }
        }

        /// <summary>
        /// Look for compatibility annotations for the set of providers we
        /// add annotations for.
        /// </summary>
        private void CheckSqlCompatibility(QueryInfo[] queries, SqlNodeAnnotations annotations) {
            if (this.Mode == ProviderMode.Sql2000 ||
                this.Mode == ProviderMode.SqlCE) {
                for (int i = 0, n = queries.Length; i < n; i++) {
                    SqlServerCompatibilityCheck.ThrowIfUnsupported(queries[i].Query, annotations, this.Mode);
                }
            }
        }

        private IExecuteResult ExecuteAll(Expression query, QueryInfo[] queryInfos, IObjectReaderFactory factory, object[] userArguments, ICompiledSubQuery[] subQueries) {
            IExecuteResult result = null;
            object lastResult = null;
            for (int i = 0, n = queryInfos.Length; i < n; i++) {
                if (i < n - 1) {
                    result = this.Execute(query, queryInfos[i], null, null, userArguments, subQueries, lastResult);
                }
                else {
                    result = this.Execute(query, queryInfos[i], factory, null, userArguments, subQueries, lastResult);
                }
                if (queryInfos[i].ResultShape == ResultShape.Return) {
                    lastResult = result.ReturnValue;
                }
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private IExecuteResult GetCachedResult(Expression query) {
            object obj = this.services.GetCachedObject(query);
            if (obj != null) {
                switch (this.GetResultShape(query)) {
                    case ResultShape.Singleton:
                        return new ExecuteResult(null, null, null, obj);
                    case ResultShape.Sequence:
                        return new ExecuteResult(null, null, null,
                            Activator.CreateInstance(
                                typeof(SequenceOfOne<>).MakeGenericType(TypeSystem.GetElementType(this.GetResultType(query))),
                                BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { obj }, null
                                ));
                }
            }
            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="Unknown reason.")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private IExecuteResult Execute(Expression query, QueryInfo queryInfo, IObjectReaderFactory factory, object[] parentArgs, object[] userArgs, ICompiledSubQuery[] subQueries, object lastResult) {
            this.InitializeProviderMode();

            DbConnection con = this.conManager.UseConnection(this);
            try {
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = queryInfo.CommandText;
                cmd.Transaction = this.conManager.Transaction;
                cmd.CommandTimeout = this.commandTimeout;
                AssignParameters(cmd, queryInfo.Parameters, userArgs, lastResult);
                LogCommand(this.log, cmd);
                this.queryCount += 1;

                switch (queryInfo.ResultShape) {
                    default:
                    case ResultShape.Return: {
                            return new ExecuteResult(cmd, queryInfo.Parameters, null, cmd.ExecuteNonQuery(), true);
                        }
                    case ResultShape.Singleton: {
                            DbDataReader reader = cmd.ExecuteReader();                            
                            IObjectReader objReader = factory.Create(reader, true, this, parentArgs, userArgs, subQueries);                        
                            this.conManager.UseConnection(objReader.Session);
                            try {
                                IEnumerable sequence = (IEnumerable)Activator.CreateInstance(
                                    typeof(OneTimeEnumerable<>).MakeGenericType(queryInfo.ResultType),
                                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                                    new object[] { objReader }, null
                                    );
                                object value = null;
                                MethodCallExpression mce = query as MethodCallExpression;
                                MethodInfo sequenceMethod = null;
                                if (mce != null && (
                                    mce.Method.DeclaringType == typeof(Queryable) ||
                                    mce.Method.DeclaringType == typeof(Enumerable))
                                    ) {
                                    switch (mce.Method.Name) {
                                        case "First":
                                        case "FirstOrDefault":
                                        case "SingleOrDefault":
                                            sequenceMethod = TypeSystem.FindSequenceMethod(mce.Method.Name, sequence);
                                            break;
                                        case "Single":
                                        default:
                                            sequenceMethod = TypeSystem.FindSequenceMethod("Single", sequence);
                                            break;
                                    }
                                }
                                else {
                                    sequenceMethod = TypeSystem.FindSequenceMethod("SingleOrDefault", sequence);
                                }

                                // When dynamically invoking the sequence method, we want to
                                // return the inner exception if the invocation fails
                                if (sequenceMethod != null) {
                                    try {
                                        value = sequenceMethod.Invoke(null, new object[] { sequence });
                                    }
                                    catch (TargetInvocationException tie) {
                                        if (tie.InnerException != null) {
                                            throw tie.InnerException;
                                        }
                                        throw;
                                    }
                                }

                                return new ExecuteResult(cmd, queryInfo.Parameters, objReader.Session, value);
                            }
                            finally {
                                objReader.Dispose();
                            }
                        }
                    case ResultShape.Sequence: {
                            DbDataReader reader = cmd.ExecuteReader();
                            IObjectReader objReader = factory.Create(reader, true, this, parentArgs, userArgs, subQueries);
                            this.conManager.UseConnection(objReader.Session);
                            IEnumerable sequence = (IEnumerable)Activator.CreateInstance(
                                typeof(OneTimeEnumerable<>).MakeGenericType(TypeSystem.GetElementType(queryInfo.ResultType)),
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                                new object[] { objReader }, null
                                );
                            if (typeof(IQueryable).IsAssignableFrom(queryInfo.ResultType)) {
                                sequence = sequence.AsQueryable();
                            }
                            ExecuteResult result = new ExecuteResult(cmd, queryInfo.Parameters, objReader.Session);
                            MetaFunction function = this.GetFunction(query);
                            if (function != null && !function.IsComposable) {
                                sequence = (IEnumerable)Activator.CreateInstance(
                                typeof(SingleResult<>).MakeGenericType(TypeSystem.GetElementType(queryInfo.ResultType)),
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                                new object[] { sequence, result, this.services.Context }, null
                                );
                            }
                            result.ReturnValue = sequence;
                            return result;
                        }
                    case ResultShape.MultipleResults: {
                            DbDataReader reader = cmd.ExecuteReader();
                            IObjectReaderSession session = this.readerCompiler.CreateSession(reader, this, parentArgs, userArgs, subQueries);
                            this.conManager.UseConnection(session);
                            MetaFunction function = this.GetFunction(query);
                            ExecuteResult result = new ExecuteResult(cmd, queryInfo.Parameters, session);
                            result.ReturnValue = new MultipleResults(this, function, session, result);
                            return result;
                        }
                }
            }
            finally {
                this.conManager.ReleaseConnection(this);
            }
        }

        private MetaFunction GetFunction(Expression query) {
            LambdaExpression lambda = query as LambdaExpression;
            if (lambda != null) {
                query = lambda.Body;
            }
            MethodCallExpression mc = query as MethodCallExpression;
            if (mc != null && typeof(DataContext).IsAssignableFrom(mc.Method.DeclaringType)) {
                return this.services.Model.GetFunction(mc.Method);
            }
            return null;
        }

        private void LogCommand(TextWriter writer, DbCommand cmd) {
            if (writer != null) {
                writer.WriteLine(cmd.CommandText);
                foreach (DbParameter p in cmd.Parameters) {
                    int prec = 0;
                    int scale = 0;
                    PropertyInfo piPrecision = p.GetType().GetProperty("Precision");
                    if (piPrecision != null) {
                        prec = (int)Convert.ChangeType(piPrecision.GetValue(p, null), typeof(int), CultureInfo.InvariantCulture);
                    }
                    PropertyInfo piScale = p.GetType().GetProperty("Scale");
                    if (piScale != null) {
                        scale = (int)Convert.ChangeType(piScale.GetValue(p, null), typeof(int), CultureInfo.InvariantCulture);
                    }                
                    var sp = p as System.Data.SqlClient.SqlParameter;
                    writer.WriteLine("-- {0}: {1} {2} (Size = {3}; Prec = {4}; Scale = {5}) [{6}]", 
                        p.ParameterName, 
                        p.Direction, 
                        sp == null ? p.DbType.ToString() : sp.SqlDbType.ToString(),
                        p.Size.ToString(System.Globalization.CultureInfo.CurrentCulture), 
                        prec, 
                        scale, 
                        sp == null ? p.Value : sp.SqlValue);
                }
                writer.WriteLine("-- Context: {0}({1}) Model: {2} Build: {3}", this.GetType().Name, this.Mode, this.services.Model.GetType().Name, ThisAssembly.InformationalVersion);
                writer.WriteLine();
            }
        }

        private void AssignParameters(DbCommand cmd, ReadOnlyCollection<SqlParameterInfo> parms, object[] userArguments, object lastResult) {
            if (parms != null) {
                foreach (SqlParameterInfo pi in parms) {
                    DbParameter p = cmd.CreateParameter();
                    p.ParameterName = pi.Parameter.Name;
                    p.Direction = pi.Parameter.Direction;
                    if (pi.Parameter.Direction == ParameterDirection.Input ||
                        pi.Parameter.Direction == ParameterDirection.InputOutput) {
                        object value = pi.Value;
                        switch (pi.Type) {
                            case SqlParameterType.UserArgument:
                                try {
                                    value = pi.Accessor.DynamicInvoke(new object[] { userArguments });
                                } catch (System.Reflection.TargetInvocationException e) {
                                    throw e.InnerException;
                                }
                                break;
                            case SqlParameterType.PreviousResult:
                                value = lastResult;
                                break;
                        }
                        this.typeProvider.InitializeParameter(pi.Parameter.SqlType, p, value);
                    }
                    else {
                        this.typeProvider.InitializeParameter(pi.Parameter.SqlType, p, null);
                    }
                    cmd.Parameters.Add(p);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        IEnumerable IProvider.Translate(Type elementType, DbDataReader reader) {
            this.CheckDispose();
            this.CheckInitialized();
            this.InitializeProviderMode();
            if (elementType == null) {
                throw Error.ArgumentNull("elementType");
            }
            if (reader == null) {
                throw Error.ArgumentNull("reader");
            }
            MetaType rowType = services.Model.GetMetaType(elementType);
            IObjectReaderFactory factory = this.GetDefaultFactory(rowType);
            IEnumerator e = factory.Create(reader, true, this, null, null, null);
            Type enumerableType = typeof(OneTimeEnumerable<>).MakeGenericType(elementType);
            return (IEnumerable)Activator.CreateInstance(enumerableType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { e }, null);
        }

        IMultipleResults IProvider.Translate(DbDataReader reader) {
            this.CheckDispose();
            this.CheckInitialized();
            this.InitializeProviderMode();
            if (reader == null) {
                throw Error.ArgumentNull("reader");
            }
            IObjectReaderSession session = this.readerCompiler.CreateSession(reader, this, null, null, null);
            return new MultipleResults(this, null, session, null);
        }

         string IProvider.GetQueryText(Expression query) {
            this.CheckDispose();
            this.CheckInitialized();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            this.InitializeProviderMode();
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            QueryInfo[] qis = this.BuildQuery(query, annotations);

            StringBuilder sb = new StringBuilder();
            for (int i = 0, n = qis.Length; i < n; i++) {
                QueryInfo qi = qis[i];
#if DEBUG
                StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
                DbCommand cmd = this.conManager.Connection.CreateCommand();
                cmd.CommandText = qi.CommandText;
                AssignParameters(cmd, qi.Parameters, null, null);
                LogCommand(writer, cmd);
                sb.Append(writer.ToString());
#else
                sb.Append(qi.CommandText);
                sb.AppendLine();
#endif
            }
            return sb.ToString();
        }

        DbCommand IProvider.GetCommand(Expression query) {
            this.CheckDispose();
            this.CheckInitialized();
            if (query == null) {
                throw Error.ArgumentNull("query");
            }
            this.InitializeProviderMode();
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            QueryInfo[] qis = this.BuildQuery(query, annotations);
            QueryInfo qi = qis[qis.Length - 1];
            DbCommand cmd = this.conManager.Connection.CreateCommand();
            cmd.CommandText = qi.CommandText;
            cmd.Transaction = this.conManager.Transaction;
            cmd.CommandTimeout = this.commandTimeout;
            AssignParameters(cmd, qi.Parameters, null, null);
            return cmd;
        }

        internal class QueryInfo {
            SqlNode query;
            string commandText;
            ReadOnlyCollection<SqlParameterInfo> parameters;
            ResultShape resultShape;
            Type resultType;

            internal QueryInfo(SqlNode query, string commandText, ReadOnlyCollection<SqlParameterInfo> parameters, ResultShape resultShape, Type resultType) {
                this.query = query;
                this.commandText = commandText;
                this.parameters = parameters;
                this.resultShape = resultShape;
                this.resultType = resultType;
            }
            internal SqlNode Query {
                get { return this.query; }
            }
            internal string CommandText {
                get { return this.commandText; }
            }
            internal ReadOnlyCollection<SqlParameterInfo> Parameters {
                get { return this.parameters; }
            }
            internal ResultShape ResultShape {
                get { return this.resultShape; }
            }
            internal Type ResultType {
                get { return this.resultType; }
            }
        }

        internal enum ResultShape {
            Return,
            Singleton,
            Sequence,
            MultipleResults
        }

        private ResultShape GetResultShape(Expression query) {
            LambdaExpression lambda = query as LambdaExpression;
            if (lambda != null) {
                query = lambda.Body;
            }

            if (query.Type == typeof(void)) {
                return ResultShape.Return;
            }
            else if (query.Type == typeof(IMultipleResults)) {
                return ResultShape.MultipleResults;
            }

            bool isSequence = typeof(IEnumerable).IsAssignableFrom(query.Type);
            ProviderType pt = this.typeProvider.From(query.Type);
            bool isScalar = !pt.IsRuntimeOnlyType && !pt.IsApplicationType;
            bool isSingleton = isScalar || !isSequence;

            MethodCallExpression mce = query as MethodCallExpression;
            if (mce != null) {
                // query operators
                if (mce.Method.DeclaringType == typeof(Queryable) ||
                    mce.Method.DeclaringType == typeof(Enumerable)) {
                    switch (mce.Method.Name) {
                        // methods known to produce singletons
                        case "First":
                        case "FirstOrDefault":
                        case "Single":
                        case "SingleOrDefault":
                            isSingleton = true;
                            break;
                    }
                }
                else if (mce.Method.DeclaringType == typeof(DataContext)) {
                    if (mce.Method.Name == "ExecuteCommand") {
                        return ResultShape.Return;
                    }
                }
                else if (mce.Method.DeclaringType.IsSubclassOf(typeof(DataContext))) {
                    MetaFunction f = this.GetFunction(query);
                    if (f != null) {
                        if (!f.IsComposable) {
                            isSingleton = false;
                        }
                        else if (isScalar) {
                            isSingleton = true;
                        }
                    }
                }
                else if (mce.Method.DeclaringType == typeof(DataManipulation) && mce.Method.ReturnType == typeof(int)) {
                    return ResultShape.Return;
                }
            }

            if (isSingleton) {
                return ResultShape.Singleton;
            }
            else if (isScalar) {
                return ResultShape.Return;
            }
            else {
                return ResultShape.Sequence;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification="Unknown reason.")]
        private Type GetResultType(Expression query) {
            LambdaExpression lambda = query as LambdaExpression;
            if (lambda != null) {
                query = lambda.Body;
            }
            return query.Type;
        }

        internal QueryInfo[] BuildQuery(Expression query, SqlNodeAnnotations annotations) {
            this.CheckDispose();

            // apply maximal funcletization
            query = Funcletizer.Funcletize(query);

            // convert query nodes into sql nodes
            QueryConverter converter = new QueryConverter(this.services, this.typeProvider, this.translator, this.sqlFactory);
            switch (this.Mode) {
                case ProviderMode.Sql2000:
                    converter.ConverterStrategy =
                        ConverterStrategy.CanUseScopeIdentity |
                        ConverterStrategy.CanUseJoinOn |
                        ConverterStrategy.CanUseRowStatus;
                    break;
                case ProviderMode.Sql2005:
                case ProviderMode.Sql2008:
                    converter.ConverterStrategy =
                        ConverterStrategy.CanUseScopeIdentity |
                        ConverterStrategy.SkipWithRowNumber |
                        ConverterStrategy.CanUseRowStatus |
                        ConverterStrategy.CanUseJoinOn |
                        ConverterStrategy.CanUseOuterApply |
                        ConverterStrategy.CanOutputFromInsert;
                    break;
                case ProviderMode.SqlCE:
                    converter.ConverterStrategy = ConverterStrategy.CanUseOuterApply;
                    // Can't set ConverterStrategy.CanUseJoinOn because scalar subqueries in the ON clause
                    // can't be converted into anything.
                    break;
            }
            SqlNode node = converter.ConvertOuter(query);

            return this.BuildQuery(this.GetResultShape(query), this.GetResultType(query), node, null, annotations);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification="These issues are related to our use of if-then and case statements for node types, which adds to the complexity count however when reviewed they are easy to navigate and understand.")]
        private QueryInfo[] BuildQuery(ResultShape resultShape, Type resultType, SqlNode node, ReadOnlyCollection<Me.SqlParameter> parentParameters, SqlNodeAnnotations annotations) {
            System.Diagnostics.Debug.Assert(resultType != null);
            System.Diagnostics.Debug.Assert(node != null);

            SqlSupersetValidator validator = new SqlSupersetValidator();

            // These are the rules that apply to every SQL tree.
            if (this.checkQueries) {
                validator.AddValidator(new ColumnTypeValidator()); /* Column CLR Type must agree with its Expressions CLR Type */
                validator.AddValidator(new LiteralValidator()); /* Constrain literal Types */
            }

            validator.Validate(node);

            SqlColumnizer columnizer = new SqlColumnizer();

            // resolve member references
            bool canUseOuterApply = (this.Mode == ProviderMode.Sql2005 || this.Mode == ProviderMode.Sql2008 || this.Mode == ProviderMode.SqlCE);
            SqlBinder binder = new SqlBinder(this.translator, this.sqlFactory, this.services.Model, this.services.Context.LoadOptions, columnizer, canUseOuterApply);
            binder.OptimizeLinkExpansions = (optimizationFlags & OptimizationFlags.OptimizeLinkExpansions) != 0;
            binder.SimplifyCaseStatements = (optimizationFlags & OptimizationFlags.SimplifyCaseStatements) != 0;
            binder.PreBinder = delegate(SqlNode n) {
                // convert methods into known reversable operators
                return PreBindDotNetConverter.Convert(n, this.sqlFactory, this.services.Model);
            };
            node = binder.Bind(node);
            if (this.checkQueries) {
                validator.AddValidator(new ExpectNoAliasRefs());
                validator.AddValidator(new ExpectNoSharedExpressions());
            }
            validator.Validate(node);

            node = PostBindDotNetConverter.Convert(node, this.sqlFactory, this.Mode);

            // identify true flow of sql data types 
            SqlRetyper retyper = new SqlRetyper(this.typeProvider, this.services.Model);
            node = retyper.Retype(node);
            validator.Validate(node);

            // change CONVERT to special conversions like UNICODE,CHAR,...
            SqlTypeConverter converter = new SqlTypeConverter(this.sqlFactory);
            node = converter.Visit(node);
            validator.Validate(node);

            // transform type-sensitive methods such as LEN (to DATALENGTH), ...
            SqlMethodTransformer methodTransformer = new SqlMethodTransformer(this.sqlFactory);
            node = methodTransformer.Visit(node);
            validator.Validate(node);

            // convert multisets into separate queries
            SqlMultiplexer.Options options = (this.Mode == ProviderMode.Sql2008 || 
                                              this.Mode == ProviderMode.Sql2005 ||
                                              this.Mode == ProviderMode.SqlCE) 
                ? SqlMultiplexer.Options.EnableBigJoin : SqlMultiplexer.Options.None;
            SqlMultiplexer mux = new SqlMultiplexer(options, parentParameters, this.sqlFactory);
            node = mux.Multiplex(node);
            validator.Validate(node);

            // convert object construction expressions into flat row projections
            SqlFlattener flattener = new SqlFlattener(this.sqlFactory, columnizer);
            node = flattener.Flatten(node);
            validator.Validate(node);

            if (this.mode == ProviderMode.SqlCE) {
                SqlRewriteScalarSubqueries rss = new SqlRewriteScalarSubqueries(this.sqlFactory);
                node = rss.Rewrite(node);
            }

            // Simplify case statements where all alternatives map to the same thing.
            // Doing this before deflator because the simplified results may lead to
            // more deflation opportunities.
            // Doing this before booleanizer because it may convert CASE statements (non-predicates) into
            // predicate expressions.
            // Doing this before reorderer because it may reduce some orders to constant nodes which should not
            // be passed onto ROW_NUMBER.
            node = SqlCaseSimplifier.Simplify(node, this.sqlFactory);

            // Rewrite order-by clauses so that they only occur at the top-most select 
            // or in selects with TOP
            SqlReorderer reorderer = new SqlReorderer(this.typeProvider, this.sqlFactory);
            node = reorderer.Reorder(node);
            validator.Validate(node);

            // Inject code to turn predicates into bits, and bits into predicates where necessary
            node = SqlBooleanizer.Rationalize(node, this.typeProvider, this.services.Model);
            if (this.checkQueries) {
                validator.AddValidator(new ExpectRationalizedBooleans()); /* From now on all boolean expressions should remain rationalized. */
            }
            validator.Validate(node);

            if (this.checkQueries) {
                validator.AddValidator(new ExpectNoFloatingColumns());
            }

            // turning predicates into bits/ints can change Sql types, propagate changes            
            node = retyper.Retype(node);
            validator.Validate(node);

            // assign aliases to columns
            // we need to do this now so that the sql2k lifters will work
            SqlAliaser aliaser = new SqlAliaser();
            node = aliaser.AssociateColumnsWithAliases(node);
            validator.Validate(node);

            // SQL2K enablers.
            node = SqlLiftWhereClauses.Lift(node, this.typeProvider, this.services.Model);
            node = SqlLiftIndependentRowExpressions.Lift(node);
            node = SqlOuterApplyReducer.Reduce(node, this.sqlFactory, annotations);
            node = SqlTopReducer.Reduce(node, annotations, this.sqlFactory);

            // resolve references to columns in other scopes by adding them
            // to the intermediate selects
            SqlResolver resolver = new SqlResolver();
            node = resolver.Resolve(node);
            validator.Validate(node);

            // re-assign aliases after resolving (new columns may have been added)
            node = aliaser.AssociateColumnsWithAliases(node);
            validator.Validate(node);

            // fixup union projections
            node = SqlUnionizer.Unionize(node);

            // remove order-by of literals
            node = SqlRemoveConstantOrderBy.Remove(node);

            // throw out unused columns and redundant sub-queries...
            SqlDeflator deflator = new SqlDeflator();
            node = deflator.Deflate(node);
            validator.Validate(node);

            // Positioning after deflator because it may remove unnecessary columns
            // from SELECT projection lists and allow more CROSS APPLYs to be reduced
            // to CROSS JOINs.
            node = SqlCrossApplyToCrossJoin.Reduce(node, annotations);

            // fixup names for aliases, columns, locals, etc..
            SqlNamer namer = new SqlNamer();
            node = namer.AssignNames(node);
            validator.Validate(node);

            // Convert [N]Text,Image to [N]VarChar(MAX),VarBinary(MAX) where necessary.
            // These new types do not exist on SQL2k, so add annotations.
            LongTypeConverter longTypeConverter = new LongTypeConverter(this.sqlFactory);
            node = longTypeConverter.AddConversions(node, annotations);
   
            // final validation            
            validator.AddValidator(new ExpectNoMethodCalls());
            validator.AddValidator(new ValidateNoInvalidComparison());
            validator.Validate(node);

            SqlParameterizer parameterizer = new SqlParameterizer(this.typeProvider, annotations);
            SqlFormatter formatter = new SqlFormatter();
            if (this.mode == ProviderMode.SqlCE ||
                this.mode == ProviderMode.Sql2005 ||
                this.mode == ProviderMode.Sql2008) {
                formatter.ParenthesizeTop = true;
            }

            SqlBlock block = node as SqlBlock;
            if (block != null && this.mode == ProviderMode.SqlCE) {
                // SQLCE cannot batch multiple statements.
                ReadOnlyCollection<ReadOnlyCollection<SqlParameterInfo>> parameters = parameterizer.ParameterizeBlock(block);
                string[] commands = formatter.FormatBlock(block, false);
                QueryInfo[] queries = new QueryInfo[commands.Length];
                for (int i = 0, n = commands.Length; i < n; i++) {
                    queries[i] = new QueryInfo(
                        block.Statements[i],
                        commands[i],
                        parameters[i],
                        (i < n - 1) ? ResultShape.Return : resultShape,
                        (i < n - 1) ? typeof(int) : resultType
                        );
                }
                return queries;
            }
            else {
                // build only one result
                ReadOnlyCollection<SqlParameterInfo> parameters = parameterizer.Parameterize(node);
                string commandText = formatter.Format(node);
                return new QueryInfo[] {
                    new QueryInfo(node, commandText, parameters, resultShape, resultType)
                    };
            }
        }

        private SqlSelect GetFinalSelect(SqlNode node) {
            switch (node.NodeType) {
                case SqlNodeType.Select:
                    return (SqlSelect)node;
                case SqlNodeType.Block: {
                        SqlBlock b = (SqlBlock)node;
                        return GetFinalSelect(b.Statements[b.Statements.Count - 1]);
                    }
            }
            return null;
        }

        private IObjectReaderFactory GetReaderFactory(SqlNode node, Type elemType) {
            SqlSelect sel = node as SqlSelect;
            SqlExpression projection = null;
            if (sel == null && node.NodeType == SqlNodeType.Block) {
                sel = this.GetFinalSelect(node);
            }
            if (sel != null) {
                projection = sel.Selection;
            }
            else {
                SqlUserQuery suq = node as SqlUserQuery;
                if (suq != null && suq.Projection != null) {
                    projection = suq.Projection;
                }
            }
            IObjectReaderFactory factory;
            if (projection != null) {
                factory = this.readerCompiler.Compile(projection, elemType);
            }
            else {
                return this.GetDefaultFactory(services.Model.GetMetaType(elemType));
            }
            return factory;
        }

        private IObjectReaderFactory GetDefaultFactory(MetaType rowType) {
            if (rowType == null) {
                throw Error.ArgumentNull("rowType");
            }
            SqlNodeAnnotations annotations = new SqlNodeAnnotations();
            Expression tmp = Expression.Constant(null);
            SqlUserQuery suq = new SqlUserQuery(string.Empty, null, null, tmp);
            if (TypeSystem.IsSimpleType(rowType.Type)) {
                // if the element type is a simple type (int, bool, etc.) we create
                // a single column binding
                SqlUserColumn col = new SqlUserColumn(rowType.Type, typeProvider.From(rowType.Type), suq, "", false, suq.SourceExpression);
                suq.Columns.Add(col);
                suq.Projection = col;
            }
            else {
                // ... otherwise we generate a default projection
                SqlUserRow rowExp = new SqlUserRow(rowType.InheritanceRoot, this.typeProvider.GetApplicationType((int)ConverterSpecialTypes.Row), suq, tmp);
                suq.Projection = this.translator.BuildProjection(rowExp, rowType, true, null, tmp);
            }
            Type resultType = TypeSystem.GetSequenceType(rowType.Type);
            QueryInfo[] qis = this.BuildQuery(ResultShape.Sequence, resultType, suq, null, annotations);
            return this.GetReaderFactory(qis[qis.Length - 1].Query, rowType.Type);
        }

        class CompiledQuery : ICompiledQuery {
            DataLoadOptions originalShape;
            Expression query;
            QueryInfo[] queryInfos;
            IObjectReaderFactory factory;
            ICompiledSubQuery[] subQueries;

            internal CompiledQuery(SqlProvider provider, Expression query, QueryInfo[] queryInfos, IObjectReaderFactory factory, ICompiledSubQuery[] subQueries) {
                this.originalShape = provider.services.Context.LoadOptions;
                this.query = query;
                this.queryInfos = queryInfos;
                this.factory = factory;
                this.subQueries = subQueries;
            }

            public IExecuteResult Execute(IProvider provider, object[] arguments) {
                if (provider == null) {
                    throw Error.ArgumentNull("provider");
                }

                SqlProvider sqlProvider = provider as SqlProvider;
                if (sqlProvider == null) {
                    throw Error.ArgumentTypeMismatch("provider");
                }

                // verify shape is compatibile with original.
                if (!AreEquivalentShapes(this.originalShape, sqlProvider.services.Context.LoadOptions)) {
                    throw Error.CompiledQueryAgainstMultipleShapesNotSupported();
                }

                // execute query (only last query produces results)
                return sqlProvider.ExecuteAll(this.query, this.queryInfos, this.factory, arguments, subQueries);
            }

            private static bool AreEquivalentShapes(DataLoadOptions shape1, DataLoadOptions shape2) {
                if (shape1 == shape2) {
                    return true;
                }
                else if (shape1 == null) {
                    return shape2.IsEmpty;
                }
                else if (shape2 == null) {
                    return shape1.IsEmpty;
                }
                else if (shape1.IsEmpty && shape2.IsEmpty) {
                    return true;
                }
                return false;
            }
        }

        class CompiledSubQuery : ICompiledSubQuery {
            QueryInfo queryInfo;
            IObjectReaderFactory factory;
            ReadOnlyCollection<Me.SqlParameter> parameters;
            ICompiledSubQuery[] subQueries;

            internal CompiledSubQuery(QueryInfo queryInfo, IObjectReaderFactory factory, ReadOnlyCollection<Me.SqlParameter> parameters, ICompiledSubQuery[] subQueries) {
                this.queryInfo = queryInfo;
                this.factory = factory;
                this.parameters = parameters;
                this.subQueries = subQueries;
            }

            public IExecuteResult Execute(IProvider provider, object[] parentArgs, object[] userArgs) {
                if (parentArgs == null && !(this.parameters == null || this.parameters.Count == 0)) {
                    throw Error.ArgumentNull("arguments");
                }

                SqlProvider sqlProvider = provider as SqlProvider;
                if (sqlProvider == null) {
                    throw Error.ArgumentTypeMismatch("provider");
                }

                // construct new copy of query info
                List<SqlParameterInfo> spis = new List<SqlParameterInfo>(this.queryInfo.Parameters);

                // add call arguments
                for (int i = 0, n = this.parameters.Count; i < n; i++) {
                    spis.Add(new SqlParameterInfo(this.parameters[i], parentArgs[i]));
                }

                QueryInfo qi = new QueryInfo(
                    this.queryInfo.Query,
                    this.queryInfo.CommandText,
                    spis.AsReadOnly(),
                    this.queryInfo.ResultShape,
                    this.queryInfo.ResultType
                    );

                // execute query
                return sqlProvider.Execute(null, qi, this.factory, parentArgs, userArgs, subQueries, null);
            }
        }

        class ExecuteResult : IExecuteResult, IDisposable {
            DbCommand command;
            ReadOnlyCollection<SqlParameterInfo> parameters;
            IObjectReaderSession session;
            int iReturnParameter = -1;
            object value;
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Microsoft: used in an assert in ReturnValue.set")]
            bool useReturnValue;
            bool isDisposed;

            internal ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value, bool useReturnValue)
                : this(command, parameters, session) {
                this.value = value;
                this.useReturnValue = useReturnValue;
                if (this.command != null && this.parameters != null && useReturnValue) {
                    iReturnParameter = GetParameterIndex("@RETURN_VALUE");
                }
            }

            internal ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session) {
                this.command = command;
                this.parameters = parameters;
                this.session = session;
            }

            internal ExecuteResult(DbCommand command, ReadOnlyCollection<SqlParameterInfo> parameters, IObjectReaderSession session, object value)
                : this(command, parameters, session, value, false) {
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1500:VariableNamesShouldNotMatchFieldNames", MessageId = "value", Justification="FxCop Error -- False positive during code analysis")]
            public object ReturnValue {
                get {
                    if (this.iReturnParameter >= 0) {
                        return this.GetParameterValue(this.iReturnParameter);
                    }
                    return this.value;
                }
                internal set {
                    Debug.Assert(!useReturnValue);
                    this.value = value;
                }
            }

            private int GetParameterIndex(string paramName) {
                int idx = -1;
                for (int i = 0, n = this.parameters.Count; i < n; i++) {
                    if (string.Compare(parameters[i].Parameter.Name, paramName, StringComparison.OrdinalIgnoreCase) == 0) {
                        idx = i;
                        break;
                    }
                }
                return idx;
            }

            internal object GetParameterValue(string paramName) {
                int idx = GetParameterIndex(paramName);
                if (idx >= 0) {
                    return GetParameterValue(idx);
                }
                return null;
            }

            public object GetParameterValue(int parameterIndex) {
                if (this.parameters == null || parameterIndex < 0 || parameterIndex > this.parameters.Count) {
                    throw Error.ArgumentOutOfRange("parameterIndex");
                }

                // SQL server requires all results to be read before output parameters are visible
                if (this.session != null && !this.session.IsBuffered) {
                    this.session.Buffer();
                }

                SqlParameterInfo pi = this.parameters[parameterIndex];
                object parameterValue = this.command.Parameters[parameterIndex].Value;
                if (parameterValue == DBNull.Value) parameterValue = null;
                if (parameterValue != null && parameterValue.GetType() != pi.Parameter.ClrType) {
                    return DBConvert.ChangeType(parameterValue, pi.Parameter.ClrType);
                }

                return parameterValue;
            }

            public void Dispose() {
                if (!this.isDisposed) {
                    // Technically, calling GC.SuppressFinalize is not required because the class does not
                    // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                    // in the future, and prevents an FxCop warning.
                    GC.SuppressFinalize(this);
                    this.isDisposed = true;
                    if (this.session!=null) {
                        this.session.Dispose();
                    }
                }
            }
        }

        class SequenceOfOne<T> : IEnumerable<T>, IEnumerable {
            T[] sequence;
            internal SequenceOfOne(T value) {
                this.sequence = new T[] { value };
            }
            public IEnumerator<T> GetEnumerator() {
                return ((IEnumerable<T>)this.sequence).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }

        class OneTimeEnumerable<T> : IEnumerable<T>, IEnumerable {
            IEnumerator<T> enumerator;

            internal OneTimeEnumerable(IEnumerator<T> enumerator) {
                System.Diagnostics.Debug.Assert(enumerator != null);
                this.enumerator = enumerator;
            }

            public IEnumerator<T> GetEnumerator() {
                if (this.enumerator == null) {
                    throw Error.CannotEnumerateResultsMoreThanOnce();
                }
                IEnumerator<T> e = this.enumerator;
                this.enumerator = null;
                return e;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }
        }

        /// <summary>
        /// Result type for single rowset returning stored procedures.
        /// </summary>
        class SingleResult<T> : ISingleResult<T>, IDisposable, IListSource {
            private IEnumerable<T> enumerable;
            private ExecuteResult executeResult;
            private DataContext context;
            private IBindingList cachedList;

            internal SingleResult(IEnumerable<T> enumerable, ExecuteResult executeResult, DataContext context) {
                System.Diagnostics.Debug.Assert(enumerable != null);
                System.Diagnostics.Debug.Assert(executeResult != null);
                this.enumerable = enumerable;
                this.executeResult = executeResult;
                this.context = context;
            }

            public IEnumerator<T> GetEnumerator() {
                return enumerable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            public object ReturnValue {
                get {
                    return executeResult.GetParameterValue("@RETURN_VALUE");
                }
            }

            public void Dispose() {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                this.executeResult.Dispose();
            }

            IList IListSource.GetList() {
                if (this.cachedList == null) {
                    this.cachedList = BindingList.Create<T>(this.context, this);
                }
                return this.cachedList;
            }

            bool IListSource.ContainsListCollection {
                get { return false; }
            }
        }

        class MultipleResults : IMultipleResults, IDisposable {
            SqlProvider provider;
            MetaFunction function;
            IObjectReaderSession session;
            bool isDisposed;
            private ExecuteResult executeResult;

            internal MultipleResults(SqlProvider provider, MetaFunction function, IObjectReaderSession session, ExecuteResult executeResult) {
                this.provider = provider;
                this.function = function;
                this.session = session;
                this.executeResult = executeResult;
            }

            public IEnumerable<T> GetResult<T>() {
                MetaType metaType = null;
                // Check the inheritance hierarchy of each mapped result row type
                // for the function.
                if (this.function != null) {
                    foreach (MetaType mt in function.ResultRowTypes) {
                        metaType = mt.InheritanceTypes.SingleOrDefault(it => it.Type == typeof(T));
                        if (metaType != null) {
                            break;
                        }
                    }
                }
                if (metaType == null) {
                    metaType = this.provider.services.Model.GetMetaType(typeof(T));
                }
                IObjectReaderFactory factory = this.provider.GetDefaultFactory(metaType);
                IObjectReader objReader = factory.GetNextResult(this.session, false);
                if (objReader == null) {
                    this.Dispose();
                    return null;
                }
                return new SingleResult<T>(new OneTimeEnumerable<T>((IEnumerator<T>)objReader), this.executeResult, this.provider.services.Context);
            }

            public void Dispose() {
                if (!this.isDisposed) {
                    // Technically, calling GC.SuppressFinalize is not required because the class does not
                    // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                    // in the future, and prevents an FxCop warning.
                    GC.SuppressFinalize(this);
                    this.isDisposed = true;
                    if (this.executeResult != null) {
                        this.executeResult.Dispose();
                    }
                    else {
                        this.session.Dispose();
                    }
                }
            }

            public object ReturnValue {
                get {
                    if (this.executeResult != null) {
                        return executeResult.GetParameterValue("@RETURN_VALUE");
                    } else {
                        return null;
                    }
                }
            }
        }
    }
}
