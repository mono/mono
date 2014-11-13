//------------------------------------------------------------------------------
// <copyright file="SqlConnection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("System.Data.DataSetExtensions, PublicKey="+AssemblyRef.EcmaPublicKeyFull)] // DevDiv Bugs 92166

namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Configuration.Assemblies;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Security;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Runtime.Versioning;
    
    using Microsoft.SqlServer.Server;
    using System.Security.Principal;
    using System.Diagnostics.CodeAnalysis;

    [DefaultEvent("InfoMessage")]
    public sealed partial class SqlConnection: DbConnection, ICloneable {

        static private readonly object EventInfoMessage = new object();

        private SqlDebugContext _sdc;   // SQL Debugging support

        private bool    _AsyncCommandInProgress;

        // SQLStatistics support
        internal SqlStatistics _statistics;
        private bool _collectstats;

        private bool _fireInfoMessageEventOnUserErrors; // False by default

        // root task associated with current async invocation
        Tuple<TaskCompletionSource<DbConnectionInternal>, Task> _currentCompletion;

        private SqlCredential _credential; // SQL authentication password stored in SecureString
        private string _connectionString;
        private int _connectRetryCount; 

        // connection resiliency
        private object _reconnectLock = new object();
        internal Task _currentReconnectionTask;
        private Task _asyncWaitingForReconnection; // current async task waiting for reconnection in non-MARS connections
        private Guid _originalConnectionId = Guid.Empty;
        private CancellationTokenSource _reconnectionCancellationSource;
        internal SessionData _recoverySessionData;
        internal WindowsIdentity _lastIdentity;
        internal WindowsIdentity _impersonateIdentity;
        private int _reconnectCount;
       
        public SqlConnection(string connectionString) : this(connectionString, null) {
        }

        public SqlConnection(string connectionString, SqlCredential credential) : this() {
            ConnectionString = connectionString;    // setting connection string first so that ConnectionOption is available
            if (credential != null)
            {
                // The following checks are necessary as setting Credential property will call CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential
                //  CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential it will throw InvalidOperationException rather than Arguemtn exception
                //  Need to call setter on Credential property rather than setting _credential directly as pool groups need to be checked
                SqlConnectionString connectionOptions = (SqlConnectionString) ConnectionOptions;
                if (UsesClearUserIdOrPassword(connectionOptions))
                {
                    throw ADP.InvalidMixedArgumentOfSecureAndClearCredential();
                }

                if (UsesIntegratedSecurity(connectionOptions))
                {
                    throw ADP.InvalidMixedArgumentOfSecureCredentialAndIntegratedSecurity();
                }

                if (UsesContextConnection(connectionOptions))
                {
                    throw ADP.InvalidMixedArgumentOfSecureCredentialAndContextConnection();
                }

                Credential = credential;
            }
            // else
            //      credential == null:  we should not set "Credential" as this will do additional validation check and
            //      checking pool groups which is not necessary. All necessary operation is already done by calling "ConnectionString = connectionString"
            CacheConnectionStringProperties();
        }

        private SqlConnection(SqlConnection connection) { // Clone
            GC.SuppressFinalize(this);
            CopyFrom(connection);
            _connectionString = connection._connectionString;
            if (connection._credential != null)
            {
                SecureString password = connection._credential.Password.Copy();
                password.MakeReadOnly();
                _credential = new SqlCredential(connection._credential.UserId, password);
            }
            CacheConnectionStringProperties();
        }

        // This method will be called once connection string is set or changed. 
        private void CacheConnectionStringProperties() {
            SqlConnectionString connString = ConnectionOptions as SqlConnectionString;
            if (connString != null) {
                _connectRetryCount = connString.ConnectRetryCount;
            }
        }

        //
        // PUBLIC PROPERTIES
        //

        // used to start/stop collection of statistics data and do verify the current state
        //
        // devnote: start/stop should not performed using a property since it requires execution of code
        //
        // start statistics
        //  set the internal flag (_statisticsEnabled) to true.
        //  Create a new SqlStatistics object if not already there.
        //  connect the parser to the object.
        //  if there is no parser at this time we need to connect it after creation.
        //

        [
        DefaultValue(false),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.SqlConnection_StatisticsEnabled),
        ]
        public bool StatisticsEnabled {
            get {
                return (_collectstats);
            }
            set {
                if (IsContextConnection) {
                    if (value) {
                        throw SQL.NotAvailableOnContextConnection();
                    }
                }
                else {
                    if (value) {
                        // start
                        if (ConnectionState.Open == State) {
                            if (null == _statistics) {
                                _statistics = new SqlStatistics();
                                ADP.TimerCurrent(out _statistics._openTimestamp);
                            }
                            // set statistics on the parser
                            // update timestamp;
                            Debug.Assert(Parser != null, "Where's the parser?");
                            Parser.Statistics = _statistics;
                        }
                    }
                    else {
                        // stop
                        if (null != _statistics) {
                            if (ConnectionState.Open == State) {
                                // remove statistics from parser
                                // update timestamp;
                                TdsParser parser = Parser;
                                Debug.Assert(parser != null, "Where's the parser?");
                                parser.Statistics = null;
                                ADP.TimerCurrent(out _statistics._closeTimestamp);
                            }
                        }
                    }
                    this._collectstats = value;
                }
            }
        }

        internal bool AsyncCommandInProgress  {
            get {
                return (_AsyncCommandInProgress);
            }
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            set {
                _AsyncCommandInProgress = value;
            }
        }

        internal bool IsContextConnection {
            get {
                SqlConnectionString opt = (SqlConnectionString)ConnectionOptions;
                return UsesContextConnection(opt);
            }
        }

        // Is this connection is a Context Connection?
        private bool UsesContextConnection(SqlConnectionString opt)
        {
            return opt != null ? opt.ContextConnection : false;
        }

        // Does this connection uses Integrated Security?
        private bool UsesIntegratedSecurity(SqlConnectionString opt) {
                return opt != null ? opt.IntegratedSecurity : false;
        }

        // Does this connection uses old style of clear userID or Password in connection string?
         private bool UsesClearUserIdOrPassword(SqlConnectionString opt) {
            bool result = false;
            if (null != opt) {
                result = (!ADP.IsEmpty(opt.UserID) || !ADP.IsEmpty(opt.Password));
            }
            return result;
        }

        internal SqlConnectionString.TransactionBindingEnum TransactionBinding {
            get {
                return ((SqlConnectionString)ConnectionOptions).TransactionBinding;
            }
        }

        internal SqlConnectionString.TypeSystem TypeSystem {
            get {
                return ((SqlConnectionString)ConnectionOptions).TypeSystemVersion;
            }
        }

        internal Version TypeSystemAssemblyVersion {
            get {
                return ((SqlConnectionString)ConnectionOptions).TypeSystemAssemblyVersion;
            }
        }        

        internal int ConnectRetryInterval {
            get {
                return ((SqlConnectionString)ConnectionOptions).ConnectRetryInterval;
            }
        }

        override protected DbProviderFactory DbProviderFactory {
            get {
                return SqlClientFactory.Instance;
            }
        }

        [
        DefaultValue(""),
#pragma warning disable 618 // ignore obsolete warning about RecommendedAsConfigurable to use SettingsBindableAttribute
        RecommendedAsConfigurable(true),
#pragma warning restore 618
        SettingsBindableAttribute(true),
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(Res.DataCategory_Data),
        Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlConnectionStringEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResDescriptionAttribute(Res.SqlConnection_ConnectionString),
        ]
        override public string ConnectionString {
            get {
                return ConnectionString_Get();
            }
            set {
                if (_credential != null)
                {
                    SqlConnectionString connectionOptions = new SqlConnectionString(value);
                    CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential(connectionOptions);
                }

                ConnectionString_Set(new SqlConnectionPoolKey(value, _credential));
                _connectionString = value;  // Change _connectionString value only after value is validated
                CacheConnectionStringProperties();
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.SqlConnection_ConnectionTimeout),
        ]
        override public int ConnectionTimeout {
            get {
                SqlConnectionString constr = (SqlConnectionString)ConnectionOptions;
                return ((null != constr) ? constr.ConnectTimeout : SqlConnectionString.DEFAULT.Connect_Timeout);
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.SqlConnection_Database),
        ]
        override public string Database {
            // if the connection is open, we need to ask the inner connection what it's
            // current catalog is because it may have gotten changed, otherwise we can
            // just return what the connection string had.
            get {
                SqlInternalConnection innerConnection = (InnerConnection as SqlInternalConnection);
                string result;

                if (null != innerConnection) {
                    result = innerConnection.CurrentDatabase;
                }
                else {
                    SqlConnectionString constr = (SqlConnectionString)ConnectionOptions;
                    result = ((null != constr) ? constr.InitialCatalog : SqlConnectionString.DEFAULT.Initial_Catalog);
                }
                return result;
            }
        }

        [
        Browsable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.SqlConnection_DataSource),
        ]
        override public string DataSource {
            get {
                SqlInternalConnection innerConnection = (InnerConnection as SqlInternalConnection);
                string result;

                if (null != innerConnection) {
                    result = innerConnection.CurrentDataSource;
                }
                else {
                    SqlConnectionString constr = (SqlConnectionString)ConnectionOptions;
                    result = ((null != constr) ? constr.DataSource : SqlConnectionString.DEFAULT.Data_Source);
                }
                return result;
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.SqlConnection_PacketSize),
        ]
        public int PacketSize {
            // if the connection is open, we need to ask the inner connection what it's
            // current packet size is because it may have gotten changed, otherwise we
            // can just return what the connection string had.
            get {
                if (IsContextConnection) {
                    throw SQL.NotAvailableOnContextConnection();
                }

                SqlInternalConnectionTds innerConnection = (InnerConnection as SqlInternalConnectionTds);
                int result;

                if (null != innerConnection) {
                    result = innerConnection.PacketSize;
                }
                else {
                    SqlConnectionString constr = (SqlConnectionString)ConnectionOptions;
                    result = ((null != constr) ? constr.PacketSize : SqlConnectionString.DEFAULT.Packet_Size);
                }
                return result;
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.SqlConnection_ClientConnectionId),
        ]
        public Guid ClientConnectionId {
            get {

                SqlInternalConnectionTds innerConnection = (InnerConnection as SqlInternalConnectionTds);

                if (null != innerConnection) {
                    return innerConnection.ClientConnectionId;
                }
                else {
                    Task reconnectTask = _currentReconnectionTask;
                    if (reconnectTask != null && !reconnectTask.IsCompleted) {
                        return _originalConnectionId;
                    }
                    return Guid.Empty;
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.SqlConnection_ServerVersion),
        ]
        override public string ServerVersion {
            get {
                return GetOpenConnection().ServerVersion;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DbConnection_State),
        ]
        override public ConnectionState State {
            get {
                Task reconnectTask=_currentReconnectionTask;
                if (reconnectTask != null && !reconnectTask.IsCompleted) {
                    return ConnectionState.Open;
                }
                return InnerConnection.State;
            }
        }


        internal SqlStatistics Statistics {
            get {
                return _statistics;
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.SqlConnection_WorkstationId),
        ]
        public string WorkstationId {
            get {
                if (IsContextConnection) {
                    throw SQL.NotAvailableOnContextConnection();
                }

                // If not supplied by the user, the default value is the MachineName
                // Note: In Longhorn you'll be able to rename a machine without
                // rebooting.  Therefore, don't cache this machine name.
                SqlConnectionString constr = (SqlConnectionString)ConnectionOptions;
                string result = ((null != constr) ? constr.WorkstationId : null);
                if (null == result) {
                    // getting machine name requires Environment.Permission
                    // user must have that permission in order to retrieve this
                    result = Environment.MachineName;
                }
                return result;
            }
        }

        // SqlCredential: Pair User Id and password in SecureString which are to be used for SQL authentication
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.SqlConnection_Credential),
        ]
        public SqlCredential Credential
        {
            get
            {
                SqlCredential result = _credential;

                // When a connection is connecting or is ever opened, make credential available only if "Persist Security Info" is set to true
                //  otherwise, return null
                SqlConnectionString connectionOptions = (SqlConnectionString) UserConnectionOptions;
                if (InnerConnection.ShouldHidePassword && connectionOptions != null && !connectionOptions.PersistSecurityInfo)
                {
                    result = null;
                }

                return result;
            }

            set
            {
                // If a connection is connecting or is ever opened, user id/password cannot be set
                if (!InnerConnection.AllowSetConnectionString)
                {
                    throw ADP.OpenConnectionPropertySet("Credential", InnerConnection.State);
                }

                // check if the usage of credential has any conflict with the keys used in connection string
                if (value != null)
                {
                    CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential((SqlConnectionString) ConnectionOptions);
                }

                _credential = value;

                // Need to call ConnectionString_Set to do proper pool group check
                ConnectionString_Set(new SqlConnectionPoolKey(_connectionString, _credential));
            }
        }

        // CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential: check if the usage of credential has any conflict
        //  with the keys used in connection string
        //  If there is any conflict, it throws InvalidOperationException
        //  This is to be used setter of ConnectionString and Credential properties
        private void CheckAndThrowOnInvalidCombinationOfConnectionStringAndSqlCredential(SqlConnectionString connectionOptions)
        {
            if (UsesClearUserIdOrPassword(connectionOptions))
            {
                throw ADP.InvalidMixedUsageOfSecureAndClearCredential();
            }

            if (UsesIntegratedSecurity(connectionOptions))
            {
                throw ADP.InvalidMixedUsageOfSecureCredentialAndIntegratedSecurity();
            }

            if (UsesContextConnection(connectionOptions))
            {
                throw ADP.InvalidMixedArgumentOfSecureCredentialAndContextConnection();
            }
        }

        //
        // PUBLIC EVENTS
        //

        [
        ResCategoryAttribute(Res.DataCategory_InfoMessage),
        ResDescriptionAttribute(Res.DbConnection_InfoMessage),
        ]
        public event SqlInfoMessageEventHandler InfoMessage {
            add {
                Events.AddHandler(EventInfoMessage, value);
            }
            remove {
                Events.RemoveHandler(EventInfoMessage, value);
            }
        }

        public bool FireInfoMessageEventOnUserErrors {
            get {
                return _fireInfoMessageEventOnUserErrors;
            }
            set {
                _fireInfoMessageEventOnUserErrors = value;
            }
        }

        // Approx. number of times that the internal connection has been reconnected
        internal int ReconnectCount {
            get {
                return _reconnectCount;
            }
        }

        //
        // PUBLIC METHODS
        //

        new public SqlTransaction BeginTransaction() {
            // this is just a delegate. The actual method tracks executiontime
            return BeginTransaction(IsolationLevel.Unspecified, null);
        }

        new public SqlTransaction BeginTransaction(IsolationLevel iso) {
            // this is just a delegate. The actual method tracks executiontime
            return BeginTransaction(iso, null);
        }

        public SqlTransaction BeginTransaction(string transactionName) {
                // Use transaction names only on the outermost pair of nested
                // BEGIN...COMMIT or BEGIN...ROLLBACK statements.  Transaction names
                // are ignored for nested BEGIN's.  The only way to rollback a nested
                // transaction is to have a save point from a SAVE TRANSACTION call.
                return BeginTransaction(IsolationLevel.Unspecified, transactionName);
        }

        // suppress this message - we cannot use SafeHandle here. Also, see notes in the code (VSTFDEVDIV# 560355)
        [SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive")]
        override protected DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
            IntPtr hscp;

            Bid.ScopeEnter(out hscp, "<prov.SqlConnection.BeginDbTransaction|API> %d#, isolationLevel=%d{ds.IsolationLevel}", ObjectID, (int)isolationLevel);
            try {

                DbTransaction transaction = BeginTransaction(isolationLevel);

                // VSTFDEVDIV# 560355 - InnerConnection doesn't maintain a ref on the outer connection (this) and 
                //   subsequently leaves open the possibility that the outer connection could be GC'ed before the SqlTransaction
                //   is fully hooked up (leaving a DbTransaction with a null connection property). Ensure that this is reachable
                //   until the completion of BeginTransaction with KeepAlive
                GC.KeepAlive(this);

                return transaction;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public SqlTransaction BeginTransaction(IsolationLevel iso, string transactionName) {
            WaitForPendingReconnection();
            SqlStatistics statistics = null;
            IntPtr hscp;
            string xactName =  ADP.IsEmpty(transactionName)? "None" : transactionName;
            Bid.ScopeEnter(out hscp, "<sc.SqlConnection.BeginTransaction|API> %d#, iso=%d{ds.IsolationLevel}, transactionName='%ls'\n", ObjectID, (int)iso,
                        xactName);

            try {
                statistics = SqlStatistics.StartTimer(Statistics);

                // NOTE: we used to throw an exception if the transaction name was empty
                // (see MDAC 50292) but that was incorrect because we have a BeginTransaction
                // method that doesn't have a transactionName argument.
                SqlTransaction transaction;
                bool isFirstAttempt = true;
                do {
                    transaction = GetOpenConnection().BeginSqlTransaction(iso, transactionName, isFirstAttempt); // do not reconnect twice
                    Debug.Assert(isFirstAttempt || !transaction.InternalTransaction.ConnectionHasBeenRestored, "Restored connection on non-first attempt");
                    isFirstAttempt = false;
                } while (transaction.InternalTransaction.ConnectionHasBeenRestored);


                // SQLBU 503873  The GetOpenConnection line above doesn't keep a ref on the outer connection (this),
                //  and it could be collected before the inner connection can hook it to the transaction, resulting in
                //  a transaction with a null connection property.  Use GC.KeepAlive to ensure this doesn't happen.
                GC.KeepAlive(this);

                return transaction;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
                SqlStatistics.StopTimer(statistics);
            }
        }

        override public void ChangeDatabase(string database) {
            SqlStatistics statistics = null;
            RepairInnerConnection();
            Bid.CorrelationTrace("<sc.SqlConnection.ChangeDatabase|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            TdsParser bestEffortCleanupTarget = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
#if DEBUG
                TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    tdsReliabilitySection.Start();
#else
                {
#endif //DEBUG
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(this);
                    statistics = SqlStatistics.StartTimer(Statistics);
                    InnerConnection.ChangeDatabase(database);
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
            }
        }

        static public void ClearAllPools() {
            (new SqlClientPermission(PermissionState.Unrestricted)).Demand();
            SqlConnectionFactory.SingletonInstance.ClearAllPools();
        }

        static public void ClearPool(SqlConnection connection) {
            ADP.CheckArgumentNull(connection, "connection");

            DbConnectionOptions connectionOptions = connection.UserConnectionOptions;
            if (null != connectionOptions) {
                connectionOptions.DemandPermission();
                if (connection.IsContextConnection) {
                    throw SQL.NotAvailableOnContextConnection();
                }
                SqlConnectionFactory.SingletonInstance.ClearPool(connection);
            }
        }

        object ICloneable.Clone() {
            SqlConnection clone = new SqlConnection(this);
            Bid.Trace("<sc.SqlConnection.Clone|API> %d#, clone=%d#\n", ObjectID, clone.ObjectID);
            return clone;
        }

        void CloseInnerConnection() {
            // CloseConnection() now handles the lock

            // The SqlInternalConnectionTds is set to OpenBusy during close, once this happens the cast below will fail and 
            // the command will no longer be cancelable.  It might be desirable to be able to cancel the close opperation, but this is
            // outside of the scope of Whidbey RTM.  See (SqlCommand::Cancel) for other lock.
            InnerConnection.CloseConnection(this, ConnectionFactory);
        }

        override public void Close() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlConnection.Close|API> %d#" , ObjectID);
            Bid.CorrelationTrace("<sc.SqlConnection.Close|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            try {
                SqlStatistics statistics = null;

                TdsParser bestEffortCleanupTarget = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
#if DEBUG
                    TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();

                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {
                        tdsReliabilitySection.Start();
#else
                    {
#endif //DEBUG
                        bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(this);
                        statistics = SqlStatistics.StartTimer(Statistics);

                        Task reconnectTask = _currentReconnectionTask;
                        if (reconnectTask != null && !reconnectTask.IsCompleted) {
                            CancellationTokenSource cts = _reconnectionCancellationSource;
                            if (cts != null) {
                                cts.Cancel();
                            }
                            AsyncHelper.WaitForCompletion(reconnectTask, 0, null, rethrowExceptions: false); // we do not need to deal with possible exceptions in reconnection
                            if (State != ConnectionState.Open) {// if we cancelled before the connection was opened 
                                OnStateChange(DbConnectionInternal.StateChangeClosed);
                            }
                        }
                        CancelOpenAndWait(); 
                        CloseInnerConnection();
                        GC.SuppressFinalize(this);

                        if (null != Statistics) {
                            ADP.TimerCurrent(out _statistics._closeTimestamp);
                        }
                    }
 #if DEBUG
                    finally {
                        tdsReliabilitySection.Stop();
                    }
#endif //DEBUG
                }
                catch (System.OutOfMemoryException e) {
                    Abort(e);
                    throw;
                }
                catch (System.StackOverflowException e) {
                    Abort(e);
                    throw;
                }
                catch (System.Threading.ThreadAbortException e) {
                    Abort(e);
                    SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                    throw;
                }
                finally {
                    SqlStatistics.StopTimer(statistics);
                }
            }
            finally {
                SqlDebugContext  sdc = _sdc;
                _sdc = null;
                Bid.ScopeLeave(ref hscp);
                if (sdc != null) {
                   sdc.Dispose();
                }
            }
        }

        new public SqlCommand CreateCommand() {
            return new SqlCommand(null, this);
        }

        private void DisposeMe(bool disposing) { // MDAC 65459
            _credential = null; // clear credential here rather than in IDisposable.Dispose as this is only specific to SqlConnection only
                                //  IDisposable.Dispose is generated code from a template and used by other providers as well

            if (!disposing) {
                // DevDiv2 Bug 457934:SQLConnection leaks when not disposed
                // http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/457934
                // For non-pooled connections we need to make sure that if the SqlConnection was not closed, then we release the GCHandle on the stateObject to allow it to be GCed
                // For pooled connections, we will rely on the pool reclaiming the connection
                var innerConnection = (InnerConnection as SqlInternalConnectionTds);
                if ((innerConnection != null) && (!innerConnection.ConnectionOptions.Pooling)) {
                    var parser = innerConnection.Parser;
                    if ((parser != null) && (parser._physicalStateObj != null)) {
                        parser._physicalStateObj.DecrementPendingCallbacks(release: false);
                    }
                }
            }
        }

        public void EnlistDistributedTransaction(System.EnterpriseServices.ITransaction transaction) {
            if (IsContextConnection) {
                throw SQL.NotAvailableOnContextConnection();
            }

            EnlistDistributedTransactionHelper(transaction);
        }

        override public void Open() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlConnection.Open|API> %d#", ObjectID) ;
            Bid.CorrelationTrace("<sc.SqlConnection.Open|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
           
            try {
                if (StatisticsEnabled) {
                    if (null == _statistics) {
                        _statistics = new SqlStatistics();
                    }
                    else {
                        _statistics.ContinueOnNewConnection();
                    }
                }

                SqlStatistics statistics = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    statistics = SqlStatistics.StartTimer(Statistics);

                    if (!TryOpen(null)) {
                        throw ADP.InternalError(ADP.InternalErrorCode.SynchronousConnectReturnedPending);
                    }
                }
                finally {
                    SqlStatistics.StopTimer(statistics);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        internal void RegisterWaitingForReconnect(Task waitingTask) {
            if (((SqlConnectionString)ConnectionOptions).MARS) {
                return;
            }
            Interlocked.CompareExchange(ref _asyncWaitingForReconnection, waitingTask, null);
            if (_asyncWaitingForReconnection != waitingTask) { // somebody else managed to register 
                throw SQL.MARSUnspportedOnConnection();
            }
        }

        private async Task ReconnectAsync(int timeout) {
            try {
                long commandTimeoutExpiration = 0;
                if (timeout > 0) {
                    commandTimeoutExpiration = ADP.TimerCurrent() + ADP.TimerFromSeconds(timeout);
                }
                CancellationTokenSource cts = new CancellationTokenSource();
                _reconnectionCancellationSource = cts;
                CancellationToken ctoken = cts.Token;
                int retryCount = _connectRetryCount; // take a snapshot: could be changed by modifying the connection string
                for (int attempt = 0; attempt < retryCount; attempt++) {                                       
                    if (ctoken.IsCancellationRequested) {
                        Bid.Trace("<sc.SqlConnection.ReconnectAsync|INFO> Orginal ClientConnectionID %ls - reconnection cancelled\n", _originalConnectionId.ToString());
                        return;
                    }
                    try {
                        _impersonateIdentity = _lastIdentity;
                        try {
                            ForceNewConnection = true;
                            await OpenAsync(ctoken).ConfigureAwait(false);
                            // On success, increment the reconnect count - we don't really care if it rolls over since it is approx.
                            _reconnectCount = unchecked(_reconnectCount + 1);
#if DEBUG
                            Debug.Assert(_recoverySessionData._debugReconnectDataApplied, "Reconnect data was not applied !");
#endif
                        }
                        finally {
                            _impersonateIdentity = null;
                            ForceNewConnection = false;
                        }
                        Bid.Trace("<sc.SqlConnection.ReconnectIfNeeded|INFO> Reconnection suceeded.  ClientConnectionID %ls -> %ls \n", _originalConnectionId.ToString(), ClientConnectionId.ToString());
                        return;
                    }
                    catch (SqlException e) {
                        Bid.Trace("<sc.SqlConnection.ReconnectAsyncINFO> Orginal ClientConnectionID %ls - reconnection attempt failed error %ls\n", _originalConnectionId.ToString(), e.Message);
                        if (attempt == retryCount - 1) {
                            Bid.Trace("<sc.SqlConnection.ReconnectAsync|INFO> Orginal ClientConnectionID %ls - give up reconnection\n", _originalConnectionId.ToString());
                            throw SQL.CR_AllAttemptsFailed(e, _originalConnectionId);
                        }
                        if (timeout > 0 && ADP.TimerRemaining(commandTimeoutExpiration) < ADP.TimerFromSeconds(ConnectRetryInterval)) {
                            throw SQL.CR_NextAttemptWillExceedQueryTimeout(e, _originalConnectionId);
                        }
                    }
                    await Task.Delay(1000 * ConnectRetryInterval, ctoken).ConfigureAwait(false);
                }
            }
            finally {               
                _recoverySessionData = null;
                _supressStateChangeForReconnection = false;
            }
            Debug.Assert(false, "Should not reach this point");
        }

        internal Task ValidateAndReconnect(Action beforeDisconnect, int timeout) {
            Task runningReconnect = _currentReconnectionTask;
            // This loop in the end will return not completed reconnect task or null
            while (runningReconnect != null && runningReconnect.IsCompleted) {
                // clean current reconnect task (if it is the same one we checked
                Interlocked.CompareExchange<Task>(ref _currentReconnectionTask, null, runningReconnect);
                // make sure nobody started new task (if which case we did not clean it)
                runningReconnect = _currentReconnectionTask;
            }
            if (runningReconnect == null) {
                if (_connectRetryCount > 0) {
                    SqlInternalConnectionTds tdsConn = GetOpenTdsConnection();                    
                    if (tdsConn._sessionRecoveryAcknowledged) {
                        TdsParserStateObject stateObj = tdsConn.Parser._physicalStateObj;     
                        if (!stateObj.ValidateSNIConnection()) {                           
                            if (tdsConn.Parser._sessionPool != null) {
                                if (tdsConn.Parser._sessionPool.ActiveSessionsCount > 0) {
                                    // >1 MARS session 
                                    if (beforeDisconnect != null) {
                                        beforeDisconnect();
                                    }
                                    OnError(SQL.CR_UnrecoverableClient(ClientConnectionId), true, null);
                                }
                            }
                            SessionData cData = tdsConn.CurrentSessionData;
                            cData.AssertUnrecoverableStateCountIsCorrect();
                            if (cData._unrecoverableStatesCount == 0) {
                                bool callDisconnect = false;
                                lock (_reconnectLock) {
                                    tdsConn.CheckEnlistedTransactionBinding();
                                    runningReconnect = _currentReconnectionTask; // double check after obtaining the lock
                                    if (runningReconnect == null) {
                                        if (cData._unrecoverableStatesCount == 0) { // could change since the first check, but now is stable since connection is know to be broken
                                            _originalConnectionId = ClientConnectionId;
                                            Bid.Trace("<sc.SqlConnection.ReconnectIfNeeded|INFO> Connection ClientConnectionID %ls is invalid, reconnecting\n", _originalConnectionId.ToString());
                                            _recoverySessionData = cData;
                                            if (beforeDisconnect != null) {
                                                beforeDisconnect();
                                            }
                                            try {
                                                _supressStateChangeForReconnection = true;
                                                tdsConn.DoomThisConnection();
                                            }
                                            catch (SqlException) {
                                            }
                                            runningReconnect = Task.Run(() => ReconnectAsync(timeout));
                                            // if current reconnect is not null, somebody already started reconnection task - some kind of race condition
                                            Debug.Assert(_currentReconnectionTask == null, "Duplicate reconnection tasks detected");                                            
                                            _currentReconnectionTask = runningReconnect;
                                        }
                                    }
                                    else {
                                        callDisconnect = true;
                                    }
                                }
                                if (callDisconnect && beforeDisconnect != null) {
                                    beforeDisconnect();
                                }
                            }
                            else {
                                if (beforeDisconnect != null) {
                                    beforeDisconnect();
                                }
                                OnError(SQL.CR_UnrecoverableServer(ClientConnectionId), true, null);
                            }
                        } // ValidateSNIConnection
                    } // sessionRecoverySupported                  
                } // connectRetryCount>0
            }
            else { // runningReconnect = null
                if (beforeDisconnect != null) {
                    beforeDisconnect();
                }
            }
            return runningReconnect;
        }

        // this is straightforward, but expensive method to do connection resiliency - it take locks and all prepartions as for TDS request
        partial void RepairInnerConnection() {
            WaitForPendingReconnection();
            if (_connectRetryCount == 0) {
                return;
            }
            SqlInternalConnectionTds tdsConn = InnerConnection as SqlInternalConnectionTds;
            if (tdsConn != null) {
                tdsConn.ValidateConnectionForExecute(null);
                tdsConn.GetSessionAndReconnectIfNeeded((SqlConnection)this);
            }
        }

        private void WaitForPendingReconnection() {
            Task reconnectTask = _currentReconnectionTask;
            if (reconnectTask != null && !reconnectTask.IsCompleted) {
                AsyncHelper.WaitForCompletion(reconnectTask, 0, null, rethrowExceptions: false);
            }
        }

        void CancelOpenAndWait()
        {
            // copy from member to avoid changes by background thread
            var completion = _currentCompletion;
            if (completion != null)
            {
                completion.Item1.TrySetCanceled();
                ((IAsyncResult)completion.Item2).AsyncWaitHandle.WaitOne();
            }
            Debug.Assert(_currentCompletion == null, "After waiting for an async call to complete, there should be no completion source");
        }

        public override Task OpenAsync(CancellationToken cancellationToken) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlConnection.OpenAsync|API> %d#", ObjectID) ;
            Bid.CorrelationTrace("<sc.SqlConnection.OpenAsync|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            try {

                if (StatisticsEnabled) {
                    if (null == _statistics) {
                        _statistics = new SqlStatistics();
                    }
                    else {
                        _statistics.ContinueOnNewConnection();
                    }
                }

                SqlStatistics statistics = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    statistics = SqlStatistics.StartTimer(Statistics);

                    System.Transactions.Transaction transaction = ADP.GetCurrentTransaction();
                    TaskCompletionSource<DbConnectionInternal> completion = new TaskCompletionSource<DbConnectionInternal>(transaction);
                    TaskCompletionSource<object> result = new TaskCompletionSource<object>();

                    if (cancellationToken.IsCancellationRequested) {
                        result.SetCanceled();
                        return result.Task;
                    }

                    if (IsContextConnection) {
                        // Async not supported on Context Connections
                        result.SetException(ADP.ExceptionWithStackTrace(SQL.NotAvailableOnContextConnection()));
                        return result.Task;
                    }

                    bool completed;
                    
                    try {
                        completed = TryOpen(completion);
                    }
                    catch (Exception e) {
                        result.SetException(e);
                        return result.Task;
                    }
                    
                    if (completed) {
                        result.SetResult(null);
                    }
                    else {
                        CancellationTokenRegistration registration = new CancellationTokenRegistration();
                        if (cancellationToken.CanBeCanceled) {
                            registration = cancellationToken.Register(() => completion.TrySetCanceled());
                        }
                        OpenAsyncRetry retry = new OpenAsyncRetry(this, completion, result, registration);
                        _currentCompletion = new Tuple<TaskCompletionSource<DbConnectionInternal>, Task>(completion, result.Task);
                        completion.Task.ContinueWith(retry.Retry, TaskScheduler.Default);
                        return result.Task;
                    }

                    return result.Task;
                }
                finally {
                    SqlStatistics.StopTimer(statistics);
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private class OpenAsyncRetry {
            SqlConnection _parent;
            TaskCompletionSource<DbConnectionInternal> _retry;
            TaskCompletionSource<object> _result;
            CancellationTokenRegistration _registration;

            public OpenAsyncRetry(SqlConnection parent, TaskCompletionSource<DbConnectionInternal> retry, TaskCompletionSource<object> result,  CancellationTokenRegistration registration) {
                _parent = parent;
                _retry = retry;
                _result = result;
                _registration = registration;
            }

            internal void Retry(Task<DbConnectionInternal> retryTask) {
                Bid.Trace("<sc.SqlConnection.OpenAsyncRetry|Info> %d#\n", _parent.ObjectID);
                _registration.Dispose();
                try {
                    SqlStatistics statistics = null;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {
                        statistics = SqlStatistics.StartTimer(_parent.Statistics);

                        if (retryTask.IsFaulted) {
                            Exception e = retryTask.Exception.InnerException;
                            _parent.CloseInnerConnection();
                            _parent._currentCompletion = null;
                            _result.SetException(retryTask.Exception.InnerException);
                        }
                        else if (retryTask.IsCanceled) {
                            _parent.CloseInnerConnection();
                            _parent._currentCompletion = null;
                            _result.SetCanceled();
                        }
                        else {
                            bool result;
                            // protect continuation from ----s with close and cancel
                            lock (_parent.InnerConnection) {
                                result = _parent.TryOpen(_retry);
                            }
                            if (result)
                            {
                                _parent._currentCompletion = null;
                                _result.SetResult(null);
                            }
                            else {
                                _parent.CloseInnerConnection();
                                _parent._currentCompletion = null;
                                _result.SetException(ADP.ExceptionWithStackTrace(ADP.InternalError(ADP.InternalErrorCode.CompletedConnectReturnedPending)));
                            }
                        }
                    }
                    finally {
                        SqlStatistics.StopTimer(statistics);
                    }
                }
                catch (Exception e) {
                    _parent.CloseInnerConnection();
                    _parent._currentCompletion = null;
                    _result.SetException(e);
                }
            }
        }

        private bool TryOpen(TaskCompletionSource<DbConnectionInternal> retry) {
            if (_impersonateIdentity != null) {
                if (_impersonateIdentity.User == DbConnectionPoolIdentity.GetCurrentWindowsIdentity().User) {
                    return TryOpenInner(retry);
                }
                else {
                    using (WindowsImpersonationContext context = _impersonateIdentity.Impersonate()) {
                        return TryOpenInner(retry);
                    }                    
                }
            }
            else {
                if (this.UsesIntegratedSecurity((SqlConnectionString)ConnectionOptions)) {
                    _lastIdentity = DbConnectionPoolIdentity.GetCurrentWindowsIdentity();
                }
                else {
                    _lastIdentity = null;
                }
                return TryOpenInner(retry);
            }
        }

        private bool TryOpenInner(TaskCompletionSource<DbConnectionInternal> retry) {
            TdsParser bestEffortCleanupTarget = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
#if DEBUG
                TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    tdsReliabilitySection.Start();
#else
                {
#endif //DEBUG
                    if (ForceNewConnection) {
                        if (!InnerConnection.TryReplaceConnection(this, ConnectionFactory, retry, UserConnectionOptions)) {
                            return false;
                        }
                    }
                    else {
                        if (!InnerConnection.TryOpenConnection(this, ConnectionFactory, retry, UserConnectionOptions)) {
                            return false;
                        }
                    }
                    // does not require GC.KeepAlive(this) because of OnStateChange

                    // GetBestEffortCleanup must happen AFTER OpenConnection to get the correct target.
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(this);

                    var tdsInnerConnection = (InnerConnection as SqlInternalConnectionTds);
                    if (tdsInnerConnection == null) {
                        SqlInternalConnectionSmi innerConnection = (InnerConnection as SqlInternalConnectionSmi);
                        innerConnection.AutomaticEnlistment();
                    }
                    else {
                        Debug.Assert(tdsInnerConnection.Parser != null, "Where's the parser?");

                        if (!tdsInnerConnection.ConnectionOptions.Pooling) {
                            // For non-pooled connections, we need to make sure that the finalizer does actually run to avoid leaking SNI handles
                            GC.ReRegisterForFinalize(this);
                        }

                        if (StatisticsEnabled) {
                            ADP.TimerCurrent(out _statistics._openTimestamp);
                            tdsInnerConnection.Parser.Statistics = _statistics;
                        }
                        else {
                            tdsInnerConnection.Parser.Statistics = null;
                            _statistics = null; // in case of previous Open/Close/reset_CollectStats sequence
                        }
                        CompleteOpen();
                    }
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }

            return true;
        }


        //
        // INTERNAL PROPERTIES
        //

        internal bool HasLocalTransaction {
            get {
                return GetOpenConnection().HasLocalTransaction;
            }
        }

        internal bool HasLocalTransactionFromAPI {
            get {
                Task reconnectTask = _currentReconnectionTask;
                if (reconnectTask != null  && !reconnectTask.IsCompleted) {
                    return false; //we will not go into reconnection if we are inside the transaction
                }
                return GetOpenConnection().HasLocalTransactionFromAPI;
            }
        }

        internal bool IsShiloh {
            get {
                if (_currentReconnectionTask != null) { // holds true even if task is completed
                    return true; // if CR is enabled, connection, if established, will be Katmai+
                }
                return GetOpenConnection().IsShiloh;
            }
        }

        internal bool IsYukonOrNewer {
            get {
                if (_currentReconnectionTask != null) { // holds true even if task is completed
                    return true; // if CR is enabled, connection, if established, will be Katmai+
                }
                return GetOpenConnection().IsYukonOrNewer;
            }
        }

        internal bool IsKatmaiOrNewer {
            get {
                if (_currentReconnectionTask != null) { // holds true even if task is completed
                    return true; // if CR is enabled, connection, if established, will be Katmai+
                }
                return GetOpenConnection().IsKatmaiOrNewer;
            }
        }

        internal TdsParser Parser {
            get {
                SqlInternalConnectionTds tdsConnection = (GetOpenConnection() as SqlInternalConnectionTds);
                if (null == tdsConnection) {
                    throw SQL.NotAvailableOnContextConnection();
                }
                return tdsConnection.Parser;
            }
        }

        internal bool Asynchronous {
            get {
                SqlConnectionString constr = (SqlConnectionString)ConnectionOptions;
                return ((null != constr) ? constr.Asynchronous : SqlConnectionString.DEFAULT.Asynchronous);
            }
        }

        //
        // INTERNAL METHODS
        //
        
        internal void ValidateConnectionForExecute(string method, SqlCommand command) {
            Task asyncWaitingForReconnection=_asyncWaitingForReconnection;
            if (asyncWaitingForReconnection!=null) {
                if (!asyncWaitingForReconnection.IsCompleted) {
                    throw SQL.MARSUnspportedOnConnection();
                }
                else {
                    Interlocked.CompareExchange(ref _asyncWaitingForReconnection, null, asyncWaitingForReconnection);
                }
            }
            if (_currentReconnectionTask != null) {
                Task currentReconnectionTask = _currentReconnectionTask;
                if (currentReconnectionTask != null && !currentReconnectionTask.IsCompleted) {
                    return; // execution will wait for this task later
                }
            }
            SqlInternalConnection innerConnection = GetOpenConnection(method);
            innerConnection.ValidateConnectionForExecute(command);
        }

        // Surround name in brackets and then escape any end bracket to protect against SQL Injection.
        // NOTE: if the user escapes it themselves it will not work, but this was the case in V1 as well
        // as native OleDb and Odbc.
        static internal string FixupDatabaseTransactionName(string name) {
            if (!ADP.IsEmpty(name)) {
                return SqlServerEscapeHelper.EscapeIdentifier(name);
            }
            else {
                return name;
            }
        }
        
        // If wrapCloseInAction is defined, then the action it defines will be run with the connection close action passed in as a parameter
        // The close action also supports being run asynchronously
        internal void OnError(SqlException exception, bool breakConnection, Action<Action> wrapCloseInAction) {
            Debug.Assert(exception != null && exception.Errors.Count != 0, "SqlConnection: OnError called with null or empty exception!");

            // Bug fix - MDAC 49022 - connection open after failure...  Problem was parser was passing
            // Open as a state - because the parser's connection to the netlib was open.  We would
            // then set the connection state to the parser's state - which is not correct.  The only
            // time the connection state should change to what is passed in to this function is if
            // the parser is broken, then we should be closed.  Changed to passing in
            // TdsParserState, not ConnectionState.
            // fixed by [....]

            if (breakConnection && (ConnectionState.Open == State)) {

                if (wrapCloseInAction != null) {
                    int capturedCloseCount = _closeCount;

                    Action closeAction = () => {
                        if (capturedCloseCount == _closeCount) {
                            Bid.Trace("<sc.SqlConnection.OnError|INFO> %d#, Connection broken.\n", ObjectID);
                            Close();
                        }
                    };

                    wrapCloseInAction(closeAction);
                }
                else {
                    Bid.Trace("<sc.SqlConnection.OnError|INFO> %d#, Connection broken.\n", ObjectID);
                    Close();
                }
            }

            if (exception.Class >= TdsEnums.MIN_ERROR_CLASS) {
                // It is an error, and should be thrown.  Class of TdsEnums.MIN_ERROR_CLASS or above is an error,
                // below TdsEnums.MIN_ERROR_CLASS denotes an info message.
                throw exception;
            }
            else {
                // If it is a class < TdsEnums.MIN_ERROR_CLASS, it is a warning collection - so pass to handler
                this.OnInfoMessage(new SqlInfoMessageEventArgs(exception));
            }
        }

        //
        // PRIVATE METHODS
        //

        // SxS: using Debugger.IsAttached
        // 
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        private void CompleteOpen() {
            Debug.Assert(ConnectionState.Open == State, "CompleteOpen not open");
            // be sure to mark as open so SqlDebugCheck can issue Query

            // check to see if we need to hook up sql-debugging if a debugger is attached
            // We only need this check for Shiloh and earlier servers.
            if (!GetOpenConnection().IsYukonOrNewer && 
                    System.Diagnostics.Debugger.IsAttached) {
                bool debugCheck = false;
                try {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand(); // MDAC 66682, 69017
                    debugCheck = true;
                }
                catch (SecurityException e) {
                    ADP.TraceExceptionWithoutRethrow(e);
                }

                if (debugCheck) {
                    // if we don't have Unmanaged code permission, don't check for debugging
                    // but let the connection be opened while under the debugger
                    CheckSQLDebugOnConnect();
                }
            }
        }
    
        internal SqlInternalConnection GetOpenConnection() {
            SqlInternalConnection innerConnection = (InnerConnection as SqlInternalConnection);
            if (null == innerConnection) {
                throw ADP.ClosedConnectionError();
            }
            return innerConnection;
        }

        internal SqlInternalConnection GetOpenConnection(string method) {
            DbConnectionInternal innerConnection = InnerConnection;
            SqlInternalConnection innerSqlConnection = (innerConnection as SqlInternalConnection);
            if (null == innerSqlConnection) {
                throw ADP.OpenConnectionRequired(method, innerConnection.State);
            }
            return innerSqlConnection;
        }
        
        internal SqlInternalConnectionTds GetOpenTdsConnection() {
            SqlInternalConnectionTds innerConnection = (InnerConnection as SqlInternalConnectionTds);
            if (null == innerConnection) {
                throw ADP.ClosedConnectionError();
            }
            return innerConnection;
        }
        
        internal SqlInternalConnectionTds GetOpenTdsConnection(string method) {
            SqlInternalConnectionTds innerConnection = (InnerConnection as SqlInternalConnectionTds);
            if (null == innerConnection) {
                throw ADP.OpenConnectionRequired(method, InnerConnection.State);
            }
            return innerConnection;
        }

        internal void OnInfoMessage(SqlInfoMessageEventArgs imevent) {
            bool notified;
            OnInfoMessage(imevent, out notified);
        }

        internal void OnInfoMessage(SqlInfoMessageEventArgs imevent, out bool notified) {
            if (Bid.TraceOn) {
                Debug.Assert(null != imevent, "null SqlInfoMessageEventArgs");
                Bid.Trace("<sc.SqlConnection.OnInfoMessage|API|INFO> %d#, Message='%ls'\n", ObjectID, ((null != imevent) ? imevent.Message : ""));
            }
            SqlInfoMessageEventHandler handler = (SqlInfoMessageEventHandler)Events[EventInfoMessage];
            if (null != handler) {
                notified = true;
                try {
                    handler(this, imevent);
                }
                catch (Exception e) { // MDAC 53175
                    if (!ADP.IsCatchableOrSecurityExceptionType(e)) {
                        throw;
                    }

                    ADP.TraceExceptionWithoutRethrow(e);
                }
            } else {
                notified = false;
            }
        }

        //
        // SQL DEBUGGING SUPPORT
        //

        // this only happens once per connection
        // SxS: using named file mapping APIs
        // 
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void CheckSQLDebugOnConnect() {
            IntPtr hFileMap;
            uint pid = (uint)SafeNativeMethods.GetCurrentProcessId();

            string mapFileName;

            // If Win2k or later, prepend "Global\\" to enable this to work through TerminalServices.
            if (ADP.IsPlatformNT5) {
                mapFileName = "Global\\" + TdsEnums.SDCI_MAPFILENAME;
            }
            else {
                mapFileName = TdsEnums.SDCI_MAPFILENAME;
            }

            mapFileName = mapFileName + pid.ToString(CultureInfo.InvariantCulture);

            hFileMap = NativeMethods.OpenFileMappingA(0x4/*FILE_MAP_READ*/, false, mapFileName);

            if (ADP.PtrZero != hFileMap) {
                IntPtr pMemMap = NativeMethods.MapViewOfFile(hFileMap, 0x4/*FILE_MAP_READ*/, 0, 0, IntPtr.Zero);
                if (ADP.PtrZero != pMemMap) {
                    SqlDebugContext sdc = new SqlDebugContext();
                    sdc.hMemMap = hFileMap;
                    sdc.pMemMap = pMemMap;
                    sdc.pid = pid;

                    // optimization: if we only have to refresh memory-mapped data at connection open time
                    // optimization: then call here instead of in CheckSQLDebug() which gets called
                    // optimization: at command execution time
                    // RefreshMemoryMappedData(sdc);

                    // delaying setting out global state until after we issue this first SQLDebug command so that
                    // we don't reentrantly call into CheckSQLDebug
                    CheckSQLDebug(sdc);
                    // now set our global state
                    _sdc = sdc;
                }
            }
        }

        // This overload is called by the Command object when executing stored procedures.  Note that
        // if SQLDebug has never been called, it is a noop.
        internal void CheckSQLDebug() {
            if (null != _sdc)
                CheckSQLDebug(_sdc);
        }

        // SxS: using GetCurrentThreadId
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Process, ResourceScope.Process)]
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)] // MDAC 66682, 69017
        private void CheckSQLDebug(SqlDebugContext sdc) {
            // check to see if debugging has been activated
            Debug.Assert(null != sdc, "SQL Debug: invalid null debugging context!");

#pragma warning disable 618
            uint tid = (uint)AppDomain.GetCurrentThreadId();    // Sql Debugging doesn't need fiber support;
#pragma warning restore 618
            RefreshMemoryMappedData(sdc);

            // 



            // If we get here, the debugger must be hooked up.
            if (!sdc.active) {
                if (sdc.fOption/*TdsEnums.SQLDEBUG_ON*/) {
                    // turn on
                    sdc.active = true;
                    sdc.tid = tid;
                    try {
                        IssueSQLDebug(TdsEnums.SQLDEBUG_ON, sdc.machineName, sdc.pid, sdc.dbgpid, sdc.sdiDllName, sdc.data);
                        sdc.tid = 0; // reset so that the first successful time through, we notify the server of the context switch
                    }
                    catch {
                        sdc.active = false;
                        throw;
                    }
                }
            }

            // be sure to pick up thread context switch, especially the first time through
            if (sdc.active) {
                if (!sdc.fOption/*TdsEnums.SQLDEBUG_OFF*/) {
                    // turn off and free the memory
                    sdc.Dispose();
                    // okay if we throw out here, no state to clean up
                    IssueSQLDebug(TdsEnums.SQLDEBUG_OFF, null, 0, 0, null, null);
                }
                else {
                    // notify server of context change
                    if (sdc.tid != tid) {
                        sdc.tid = tid;
                        try {
                            IssueSQLDebug(TdsEnums.SQLDEBUG_CONTEXT, null, sdc.pid, sdc.tid, null, null);
                        }
                        catch {
                            sdc.tid = 0;
                            throw;
                        }
                    }
                }
            }
        }

        private void IssueSQLDebug(uint option, string machineName, uint pid, uint id, string sdiDllName, byte[] data) {

            if (GetOpenConnection().IsYukonOrNewer) {
                // 
                return;
            }

            // 

            SqlCommand c = new SqlCommand(TdsEnums.SP_SDIDEBUG, this);
            c.CommandType = CommandType.StoredProcedure;

            // context param
            SqlParameter p = new SqlParameter(null, SqlDbType.VarChar, TdsEnums.SQLDEBUG_MODE_NAMES[option].Length);
            p.Value = TdsEnums.SQLDEBUG_MODE_NAMES[option];
            c.Parameters.Add(p);

            if (option == TdsEnums.SQLDEBUG_ON) {
                // debug dll name
                p = new SqlParameter(null, SqlDbType.VarChar, sdiDllName.Length);
                p.Value = sdiDllName;
                c.Parameters.Add(p);
                // debug machine name
                p = new SqlParameter(null, SqlDbType.VarChar, machineName.Length);
                p.Value = machineName;
                c.Parameters.Add(p);
            }

            if (option != TdsEnums.SQLDEBUG_OFF) {
                // client pid
                p = new SqlParameter(null, SqlDbType.Int);
                p.Value = pid;
                c.Parameters.Add(p);
                // dbgpid or tid
                p = new SqlParameter(null, SqlDbType.Int);
                p.Value = id;
                c.Parameters.Add(p);
            }

            if (option == TdsEnums.SQLDEBUG_ON) {
                // debug data
                p = new SqlParameter(null, SqlDbType.VarBinary, (null != data) ? data.Length : 0);
                p.Value = data;
                c.Parameters.Add(p);
            }

            c.ExecuteNonQuery();
        }


        public static void ChangePassword(string connectionString, string newPassword) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlConnection.ChangePassword|API>") ;
            Bid.CorrelationTrace("<sc.SqlConnection.ChangePassword|API|Correlation> ActivityID %ls\n");
            try {
                if (ADP.IsEmpty(connectionString)) {
                    throw SQL.ChangePasswordArgumentMissing("connectionString");
                }
                if (ADP.IsEmpty(newPassword)) {
                    throw SQL.ChangePasswordArgumentMissing("newPassword");
                }
                if (TdsEnums.MAXLEN_NEWPASSWORD < newPassword.Length) {
                    throw ADP.InvalidArgumentLength("newPassword", TdsEnums.MAXLEN_NEWPASSWORD);
                }

                SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, null);

                SqlConnectionString connectionOptions = SqlConnectionFactory.FindSqlConnectionOptions(key);
                if (connectionOptions.IntegratedSecurity) {
                    throw SQL.ChangePasswordConflictsWithSSPI();
                }
                if (! ADP.IsEmpty(connectionOptions.AttachDBFilename)) {
                    throw SQL.ChangePasswordUseOfUnallowedKey(SqlConnectionString.KEY.AttachDBFilename);
                }
                if (connectionOptions.ContextConnection) {
                    throw SQL.ChangePasswordUseOfUnallowedKey(SqlConnectionString.KEY.Context_Connection);
                }

                System.Security.PermissionSet permissionSet = connectionOptions.CreatePermissionSet();
                permissionSet.Demand();

                ChangePassword(connectionString, connectionOptions, null, newPassword, null);
             }
            finally {
                Bid.ScopeLeave(ref hscp) ;
            }
       }

        public static void ChangePassword(string connectionString, SqlCredential credential, SecureString newSecurePassword) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlConnection.ChangePassword|API>") ;
            Bid.CorrelationTrace("<sc.SqlConnection.ChangePassword|API|Correlation> ActivityID %ls\n");
            try {
                if (ADP.IsEmpty(connectionString)) {
                    throw SQL.ChangePasswordArgumentMissing("connectionString");
                }

                // check credential; not necessary to check the length of password in credential as the check is done by SqlCredential class
                if (credential == null) {
                    throw SQL.ChangePasswordArgumentMissing("credential");
                }

                if (newSecurePassword == null || newSecurePassword.Length == 0) {
                    throw SQL.ChangePasswordArgumentMissing("newSecurePassword");;
                }

                if (!newSecurePassword.IsReadOnly()) {
                    throw ADP.MustBeReadOnly("newSecurePassword");
                }

                if (TdsEnums.MAXLEN_NEWPASSWORD < newSecurePassword.Length) {
                    throw ADP.InvalidArgumentLength("newSecurePassword", TdsEnums.MAXLEN_NEWPASSWORD);
                }

                SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, credential);

                SqlConnectionString connectionOptions = SqlConnectionFactory.FindSqlConnectionOptions(key);

                // Check for incompatible connection string value with SqlCredential
                if (!ADP.IsEmpty(connectionOptions.UserID) || !ADP.IsEmpty(connectionOptions.Password)) {
                    throw ADP.InvalidMixedArgumentOfSecureAndClearCredential();
                }

                if (connectionOptions.IntegratedSecurity) {
                    throw SQL.ChangePasswordConflictsWithSSPI();
                }

                if (! ADP.IsEmpty(connectionOptions.AttachDBFilename)) {
                    throw SQL.ChangePasswordUseOfUnallowedKey(SqlConnectionString.KEY.AttachDBFilename);
                }

                if (connectionOptions.ContextConnection) {
                    throw SQL.ChangePasswordUseOfUnallowedKey(SqlConnectionString.KEY.Context_Connection);
                }

                System.Security.PermissionSet permissionSet = connectionOptions.CreatePermissionSet();
                permissionSet.Demand();

                ChangePassword(connectionString, connectionOptions, credential, null, newSecurePassword);
            }
            finally {
                Bid.ScopeLeave(ref hscp) ;
            }
        }

        private static void ChangePassword(string connectionString, SqlConnectionString connectionOptions, SqlCredential credential, string newPassword, SecureString newSecurePassword ) {
            // note: This is the only case where we directly construt the internal connection, passing in the new password.
            // Normally we would simply create a regular connectoin and open it but there is no other way to pass the
            // new password down to the constructor. Also it would have an unwanted impact on the connection pool
            //
            using (SqlInternalConnectionTds con = new SqlInternalConnectionTds(null, connectionOptions, credential, null, newPassword, newSecurePassword, false)) {
                if (!con.IsYukonOrNewer) {
                    throw SQL.ChangePasswordRequiresYukon();
                }
            }
            SqlConnectionPoolKey key = new SqlConnectionPoolKey(connectionString, credential);

            SqlConnectionFactory.SingletonInstance.ClearPool(key);
        }

        internal void RegisterForConnectionCloseNotification<T>(ref Task<T> outterTask, object value, int tag) {
            // Connection exists,  schedule removal, will be added to ref collection after calling ValidateAndReconnect
            outterTask = outterTask.ContinueWith(task => {
                RemoveWeakReference(value);
                return task;
            }, TaskScheduler.Default).Unwrap();
        }

        // updates our context with any changes made to the memory-mapped data by an external process
        static private void RefreshMemoryMappedData(SqlDebugContext sdc) {
            Debug.Assert(ADP.PtrZero != sdc.pMemMap, "SQL Debug: invalid null value for pMemMap!");
            // copy memory mapped file contents into managed types
            MEMMAP memMap = (MEMMAP)Marshal.PtrToStructure(sdc.pMemMap, typeof(MEMMAP));
            sdc.dbgpid = memMap.dbgpid;
            sdc.fOption = (memMap.fOption == 1) ? true : false;
            // xlate ansi byte[] -> managed strings
            Encoding cp = System.Text.Encoding.GetEncoding(TdsEnums.DEFAULT_ENGLISH_CODE_PAGE_VALUE);
            sdc.machineName = cp.GetString(memMap.rgbMachineName, 0, memMap.rgbMachineName.Length);
            sdc.sdiDllName = cp.GetString(memMap.rgbDllName, 0, memMap.rgbDllName.Length);
            // just get data reference
            sdc.data = memMap.rgbData;
        }

        public void ResetStatistics() {
            if (IsContextConnection) {
                throw SQL.NotAvailableOnContextConnection();
            }

            if (null != Statistics) {
                Statistics.Reset();
                if (ConnectionState.Open == State) {
                    // update timestamp;
                    ADP.TimerCurrent(out _statistics._openTimestamp);
                }
            }
        }

        public IDictionary RetrieveStatistics() {
            if (IsContextConnection) {
                throw SQL.NotAvailableOnContextConnection();
            }

            if (null != Statistics) {
                UpdateStatistics();
                return Statistics.GetHashtable();
            }
            else {
                return new SqlStatistics().GetHashtable();
            }
        }

        private void UpdateStatistics() {
            if (ConnectionState.Open == State) {
                // update timestamp
                ADP.TimerCurrent(out _statistics._closeTimestamp);
            }
            // delegate the rest of the work to the SqlStatistics class
            Statistics.UpdateStatistics();
        }

        //
        // UDT SUPPORT
        //

        private Assembly ResolveTypeAssembly(AssemblyName asmRef, bool throwOnError) {
            Debug.Assert(TypeSystemAssemblyVersion != null, "TypeSystemAssembly should be set !");
            if (string.Compare(asmRef.Name, "Microsoft.SqlServer.Types", StringComparison.OrdinalIgnoreCase) == 0) {
                if (Bid.TraceOn) {
                    if (asmRef.Version!=TypeSystemAssemblyVersion) {
                        Bid.Trace("<sc.SqlConnection.ResolveTypeAssembly> SQL CLR type version change: Server sent %ls, client will instantiate %ls", 
                            asmRef.Version.ToString(), TypeSystemAssemblyVersion.ToString());
                    }
                }
                asmRef.Version = TypeSystemAssemblyVersion;
            }
            try {
                return Assembly.Load(asmRef);
            }
            catch (Exception e) {
                if (throwOnError || !ADP.IsCatchableExceptionType(e)) {
                    throw;
                }
                else {
                    return null;
                };
            }
        }

        // 
        internal void CheckGetExtendedUDTInfo(SqlMetaDataPriv metaData, bool fThrow) {
            if (metaData.udtType == null) { // If null, we have not obtained extended info.
                Debug.Assert(!ADP.IsEmpty(metaData.udtAssemblyQualifiedName), "Unexpected state on GetUDTInfo");
                // Parameter throwOnError determines whether exception from Assembly.Load is thrown.
                metaData.udtType =                
                    Type.GetType(typeName:metaData.udtAssemblyQualifiedName, assemblyResolver:asmRef => ResolveTypeAssembly(asmRef, fThrow), typeResolver:null, throwOnError: fThrow); 

                if (fThrow && metaData.udtType == null) {
                    // 
                    throw SQL.UDTUnexpectedResult(metaData.udtAssemblyQualifiedName); 
                }
            }
        }

        internal object GetUdtValue(object value, SqlMetaDataPriv metaData, bool returnDBNull) {
            if (returnDBNull && ADP.IsNull(value)) {
                return DBNull.Value;
            }

            object o = null;

            // Since the serializer doesn't handle nulls...
            if (ADP.IsNull(value)) {
                Type t = metaData.udtType;
                Debug.Assert(t != null, "Unexpected null of udtType on GetUdtValue!");
                o = t.InvokeMember("Null", BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Static, null, null, new Object[]{}, CultureInfo.InvariantCulture);
                Debug.Assert(o != null);
                return o;
            }
            else {

                MemoryStream stm = new MemoryStream((byte[]) value);

                o = SerializationHelperSql9.Deserialize(stm, metaData.udtType);

                Debug.Assert(o != null, "object could NOT be created");
                return o;
            }
        }

        internal byte[] GetBytes(object o) {
            Microsoft.SqlServer.Server.Format format  = Microsoft.SqlServer.Server.Format.Native;
            int    maxSize = 0;
            return GetBytes(o, out format, out maxSize);
        }

        internal byte[] GetBytes(object o, out Microsoft.SqlServer.Server.Format format, out int maxSize) {
            SqlUdtInfo attr = AssemblyCache.GetInfoFromType(o.GetType());
            maxSize = attr.MaxByteSize;
            format  = attr.SerializationFormat;

            if (maxSize < -1 || maxSize >= UInt16.MaxValue) { // Do we need this?  Is this the right place?
                throw new InvalidOperationException(o.GetType() + ": invalid Size");
            }

            byte[] retval;

            using (MemoryStream stm = new MemoryStream(maxSize < 0 ? 0 : maxSize)) {
                SerializationHelperSql9.Serialize(stm, o);
                retval = stm.ToArray();
            }
            return retval;
        }
    } // SqlConnection

    // 





    [
    ComVisible(true),
    ClassInterface(ClassInterfaceType.None),
    Guid("afef65ad-4577-447a-a148-83acadd3d4b9"),
    ]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class SQLDebugging: ISQLDebug {

        // Security stuff
        const int STANDARD_RIGHTS_REQUIRED = (0x000F0000);
        const int DELETE = (0x00010000);
        const int READ_CONTROL = (0x00020000);
        const int WRITE_DAC = (0x00040000);
        const int WRITE_OWNER = (0x00080000);
        const int SYNCHRONIZE = (0x00100000);
        const int FILE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x000001FF);
        const uint GENERIC_READ = (0x80000000);
        const uint GENERIC_WRITE = (0x40000000);
        const uint GENERIC_EXECUTE = (0x20000000);
        const uint GENERIC_ALL = (0x10000000);

        const int SECURITY_DESCRIPTOR_REVISION = (1);
        const int ACL_REVISION = (2);

        const int SECURITY_AUTHENTICATED_USER_RID = (0x0000000B);
        const int SECURITY_LOCAL_SYSTEM_RID = (0x00000012);
        const int SECURITY_BUILTIN_DOMAIN_RID = (0x00000020);
        const int SECURITY_WORLD_RID = (0x00000000);
        const byte SECURITY_NT_AUTHORITY = 5;
        const int DOMAIN_GROUP_RID_ADMINS = (0x00000200);
        const int DOMAIN_ALIAS_RID_ADMINS = (0x00000220);

        const int sizeofSECURITY_ATTRIBUTES = 12; // sizeof(SECURITY_ATTRIBUTES);
        const int sizeofSECURITY_DESCRIPTOR = 20; // sizeof(SECURITY_DESCRIPTOR);
        const int sizeofACCESS_ALLOWED_ACE = 12; // sizeof(ACCESS_ALLOWED_ACE);
        const int sizeofACCESS_DENIED_ACE = 12; // sizeof(ACCESS_DENIED_ACE);
        const int sizeofSID_IDENTIFIER_AUTHORITY = 6; // sizeof(SID_IDENTIFIER_AUTHORITY)
        const int sizeofACL = 8; // sizeof(ACL);

        private IntPtr CreateSD(ref IntPtr pDacl) {
            IntPtr pSecurityDescriptor = IntPtr.Zero;
            IntPtr pUserSid = IntPtr.Zero;
            IntPtr pAdminSid = IntPtr.Zero;
            IntPtr pNtAuthority = IntPtr.Zero;
            int cbAcl = 0;
            bool status = false;

            pNtAuthority = Marshal.AllocHGlobal(sizeofSID_IDENTIFIER_AUTHORITY);
            if (pNtAuthority == IntPtr.Zero)
                goto cleanup;
            Marshal.WriteInt32(pNtAuthority, 0, 0);
            Marshal.WriteByte(pNtAuthority, 4, 0);
            Marshal.WriteByte(pNtAuthority, 5, SECURITY_NT_AUTHORITY);

            status =
            NativeMethods.AllocateAndInitializeSid(
            pNtAuthority,
            (byte)1,
            SECURITY_AUTHENTICATED_USER_RID,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            ref pUserSid);

            if (!status || pUserSid == IntPtr.Zero) {
                goto cleanup;
            }
            status =
            NativeMethods.AllocateAndInitializeSid(
            pNtAuthority,
            (byte)2,
            SECURITY_BUILTIN_DOMAIN_RID,
            DOMAIN_ALIAS_RID_ADMINS,
            0,
            0,
            0,
            0,
            0,
            0,
            ref pAdminSid);

            if (!status || pAdminSid == IntPtr.Zero) {
                goto cleanup;
            }
            status = false;
            pSecurityDescriptor = Marshal.AllocHGlobal(sizeofSECURITY_DESCRIPTOR);
            if (pSecurityDescriptor == IntPtr.Zero) {
                goto cleanup;
            }
            for (int i = 0; i < sizeofSECURITY_DESCRIPTOR; i++)
                Marshal.WriteByte(pSecurityDescriptor, i, (byte)0);
            cbAcl = sizeofACL
            + (2 * (sizeofACCESS_ALLOWED_ACE))
            + sizeofACCESS_DENIED_ACE
            + NativeMethods.GetLengthSid(pUserSid)
            + NativeMethods.GetLengthSid(pAdminSid);

            pDacl = Marshal.AllocHGlobal(cbAcl);
            if (pDacl == IntPtr.Zero) {
                goto cleanup;
            }
            // rights must be added in a certain order.  Namely, deny access first, then add access
            if (NativeMethods.InitializeAcl(pDacl, cbAcl, ACL_REVISION))
                if (NativeMethods.AddAccessDeniedAce(pDacl, ACL_REVISION, WRITE_DAC, pUserSid))
                    if (NativeMethods.AddAccessAllowedAce(pDacl, ACL_REVISION, GENERIC_READ, pUserSid))
                        if (NativeMethods.AddAccessAllowedAce(pDacl, ACL_REVISION, GENERIC_ALL, pAdminSid))
                            if (NativeMethods.InitializeSecurityDescriptor(pSecurityDescriptor, SECURITY_DESCRIPTOR_REVISION))
                                if (NativeMethods.SetSecurityDescriptorDacl(pSecurityDescriptor, true, pDacl, false)) {
                                    status = true;
                                }

            cleanup :
            if (pNtAuthority != IntPtr.Zero) {
                Marshal.FreeHGlobal(pNtAuthority);
            }
            if (pAdminSid != IntPtr.Zero)
                NativeMethods.FreeSid(pAdminSid);
            if (pUserSid != IntPtr.Zero)
                NativeMethods.FreeSid(pUserSid);
            if (status)
                return pSecurityDescriptor;
            else {
                if (pSecurityDescriptor != IntPtr.Zero) {
                    Marshal.FreeHGlobal(pSecurityDescriptor);
                }
            }
            return IntPtr.Zero;
        }

        // SxS: using file mapping API (CreateFileMapping)
        // 
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        bool ISQLDebug.SQLDebug(int dwpidDebugger, int dwpidDebuggee, [MarshalAs(UnmanagedType.LPStr)] string pszMachineName,
        [MarshalAs(UnmanagedType.LPStr)] string pszSDIDLLName, int dwOption, int cbData, byte[] rgbData) {
            bool result = false;
            IntPtr hFileMap = IntPtr.Zero;
            IntPtr pMemMap = IntPtr.Zero;
            IntPtr pSecurityDescriptor = IntPtr.Zero;
            IntPtr pSecurityAttributes = IntPtr.Zero;
            IntPtr pDacl = IntPtr.Zero;

            // validate the structure
            if (null == pszMachineName || null == pszSDIDLLName)
                return false;

            if (pszMachineName.Length > TdsEnums.SDCI_MAX_MACHINENAME ||
            pszSDIDLLName.Length > TdsEnums.SDCI_MAX_DLLNAME)
                return false;

            // note that these are ansi strings
            Encoding cp = System.Text.Encoding.GetEncoding(TdsEnums.DEFAULT_ENGLISH_CODE_PAGE_VALUE);
            byte[] rgbMachineName = cp.GetBytes(pszMachineName);
            byte[] rgbSDIDLLName = cp.GetBytes(pszSDIDLLName);

            if (null != rgbData && cbData > TdsEnums.SDCI_MAX_DATA)
                return false;

            string mapFileName;

            // If Win2k or later, prepend "Global\\" to enable this to work through TerminalServices.
            if (ADP.IsPlatformNT5) {
                mapFileName = "Global\\" + TdsEnums.SDCI_MAPFILENAME;
            }
            else {
                mapFileName = TdsEnums.SDCI_MAPFILENAME;
            }

            mapFileName = mapFileName + dwpidDebuggee.ToString(CultureInfo.InvariantCulture);

            // Create Security Descriptor
            pSecurityDescriptor = CreateSD(ref pDacl);
            pSecurityAttributes = Marshal.AllocHGlobal(sizeofSECURITY_ATTRIBUTES);
            if ((pSecurityDescriptor == IntPtr.Zero) || (pSecurityAttributes == IntPtr.Zero))
                return false;

            Marshal.WriteInt32(pSecurityAttributes, 0, sizeofSECURITY_ATTRIBUTES); // nLength = sizeof(SECURITY_ATTRIBUTES)
            Marshal.WriteIntPtr(pSecurityAttributes, 4, pSecurityDescriptor); // lpSecurityDescriptor = pSecurityDescriptor
            Marshal.WriteInt32(pSecurityAttributes, 8, 0); // bInheritHandle = FALSE
            hFileMap = NativeMethods.CreateFileMappingA(
            ADP.InvalidPtr/*INVALID_HANDLE_VALUE*/,
            pSecurityAttributes,
            0x4/*PAGE_READWRITE*/,
            0,
            Marshal.SizeOf(typeof(MEMMAP)),
            mapFileName);

            if (IntPtr.Zero == hFileMap) {
                goto cleanup;
            }


            pMemMap = NativeMethods.MapViewOfFile(hFileMap, 0x6/*FILE_MAP_READ|FILE_MAP_WRITE*/, 0, 0, IntPtr.Zero);

            if (IntPtr.Zero == pMemMap) {
                goto cleanup;
            }

            // copy data to memory-mapped file
            // layout of MEMMAP structure is:
            // uint dbgpid
            // uint fOption
            // byte[32] machineName
            // byte[16] sdiDllName
            // uint dbData
            // byte[255] vData
            int offset = 0;
            Marshal.WriteInt32(pMemMap, offset, (int)dwpidDebugger);
            offset += 4;
            Marshal.WriteInt32(pMemMap, offset, (int)dwOption);
            offset += 4;
            Marshal.Copy(rgbMachineName, 0, ADP.IntPtrOffset(pMemMap, offset), rgbMachineName.Length);
            offset += TdsEnums.SDCI_MAX_MACHINENAME;
            Marshal.Copy(rgbSDIDLLName, 0, ADP.IntPtrOffset(pMemMap, offset), rgbSDIDLLName.Length);
            offset += TdsEnums.SDCI_MAX_DLLNAME;
            Marshal.WriteInt32(pMemMap, offset, (int)cbData);
            offset += 4;
            if (null != rgbData) {
                Marshal.Copy(rgbData, 0, ADP.IntPtrOffset(pMemMap, offset), (int)cbData);
            }
            NativeMethods.UnmapViewOfFile(pMemMap);
            result = true;
        cleanup :
            if (result == false) {
                if (hFileMap != IntPtr.Zero)
                    NativeMethods.CloseHandle(hFileMap);
            }
            if (pSecurityAttributes != IntPtr.Zero)
                Marshal.FreeHGlobal(pSecurityAttributes);
            if (pSecurityDescriptor != IntPtr.Zero)
                Marshal.FreeHGlobal(pSecurityDescriptor);
            if (pDacl != IntPtr.Zero)
                Marshal.FreeHGlobal(pDacl);
            return result;
        }
    }

    // this is a private interface to com+ users
    // do not change this guid
    [
    ComImport,
    ComVisible(true),
    Guid("6cb925bf-c3c0-45b3-9f44-5dd67c7b7fe8"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    BestFitMapping(false, ThrowOnUnmappableChar = true),
    ]
    interface ISQLDebug {

        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name = "FullTrust")]
        bool SQLDebug(
        int dwpidDebugger,
        int dwpidDebuggee,
        [MarshalAs(UnmanagedType.LPStr)] string pszMachineName,
        [MarshalAs(UnmanagedType.LPStr)] string pszSDIDLLName,
        int dwOption,
        int cbData,
        byte[] rgbData);
    }

    sealed class SqlDebugContext: IDisposable {
        // context data
        internal uint pid = 0;
        internal uint tid = 0;
        internal bool active = false;
        // memory-mapped data
        internal IntPtr pMemMap = ADP.PtrZero;
        internal IntPtr hMemMap = ADP.PtrZero;
        internal uint dbgpid = 0;
        internal bool fOption = false;
        internal string machineName = null;
        internal string sdiDllName = null;
        internal byte[] data = null;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // using CloseHandle and UnmapViewOfFile - no exposure
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private void Dispose(bool disposing) {
            if (disposing) {
                // Nothing to do here
                ;
            }
            if (pMemMap != IntPtr.Zero) {
                NativeMethods.UnmapViewOfFile(pMemMap);
                pMemMap = IntPtr.Zero;
            }
            if (hMemMap != IntPtr.Zero) {
                NativeMethods.CloseHandle(hMemMap);
                hMemMap = IntPtr.Zero;
            }
            active = false;
        }
        
        ~SqlDebugContext() {
                Dispose(false);
        }

    }

    // native interop memory mapped structure for sdi debugging
    [StructLayoutAttribute(LayoutKind.Sequential, Pack = 1)]
    internal struct MEMMAP {
        [MarshalAs(UnmanagedType.U4)]
        internal uint dbgpid; // id of debugger
        [MarshalAs(UnmanagedType.U4)]
        internal uint fOption; // 1 - start debugging, 0 - stop debugging
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        internal byte[] rgbMachineName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] rgbDllName;
        [MarshalAs(UnmanagedType.U4)]
        internal uint cbData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
        internal byte[] rgbData;
    }
} // System.Data.SqlClient namespace


