//------------------------------------------------------------------------------
// <copyright file="SqlCommand.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration.Assemblies;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SysTx = System.Transactions;
    using System.Xml;

    using Microsoft.SqlServer.Server;

    [
    DefaultEvent("RecordsAffected"),
    ToolboxItem(true),
    Designer("Microsoft.VSDesigner.Data.VS.SqlCommandDesigner, " + AssemblyRef.MicrosoftVSDesigner)
    ]
    public sealed class SqlCommand : DbCommand, ICloneable {

        private  static int     _objectTypeCount; // Bid counter
        internal readonly int   ObjectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        private string          _commandText;
        private CommandType     _commandType;
        private int             _commandTimeout = ADP.DefaultCommandTimeout;
        private UpdateRowSource _updatedRowSource = UpdateRowSource.Both;
        private bool            _designTimeInvisible;

        /// <summary>
        /// Indicates if the column encryption setting was set at-least once in the batch rpc mode, when using AddBatchCommand.
        /// </summary>
        private bool            _wasBatchModeColumnEncryptionSettingSetOnce;

        /// <summary>
        /// Column Encryption Override. Defaults to SqlConnectionSetting, in which case
        /// it will be Enabled if SqlConnectionOptions.IsColumnEncryptionSettingEnabled = true, Disabled if false.
        /// This may also be used to set other behavior which overrides connection level setting.
        /// </summary>
        private SqlCommandColumnEncryptionSetting _columnEncryptionSetting = SqlCommandColumnEncryptionSetting.UseConnectionSetting;

        internal SqlDependency  _sqlDep;

#if DEBUG
        /// <summary>
        /// Force the client to sleep during sp_describe_parameter_encryption in the function TryFetchInputParameterEncryptionInfo.
        /// </summary>
        private static bool _sleepDuringTryFetchInputParameterEncryptionInfo = false;

        /// <summary>
        /// Force the client to sleep during sp_describe_parameter_encryption in the function RunExecuteReaderTds.
        /// </summary>
        private static bool _sleepDuringRunExecuteReaderTdsForSpDescribeParameterEncryption = false;

        /// <summary>
        /// Force the client to sleep during sp_describe_parameter_encryption after ReadDescribeEncryptionParameterResults.
        /// </summary>
        private static bool _sleepAfterReadDescribeEncryptionParameterResults = false;
#endif

        // devnote: Prepare
        // Against 7.0 Server (Sphinx) a prepare/unprepare requires an extra roundtrip to the server.
        //
        // From 8.0 (Shiloh) and above (Yukon) the preparation can be done as part of the command execution.
        //
        private enum EXECTYPE {
            UNPREPARED,         // execute unprepared commands, all server versions (results in sp_execsql call)
            PREPAREPENDING,     // prepare and execute command, 8.0 and above only  (results in sp_prepexec call)
            PREPARED,           // execute prepared commands, all server versions   (results in sp_exec call)
        }

        // devnotes
        //
        // _hiddenPrepare
        // On 8.0 and above the Prepared state cannot be left. Once a command is prepared it will always be prepared.
        // A change in parameters, commandtext etc (IsDirty) automatically causes a hidden prepare
        //
        // _inPrepare will be set immediately before the actual prepare is done.
        // The OnReturnValue function will test this flag to determine whether the returned value is a _prepareHandle or something else.
        //
        // _prepareHandle - the handle of a prepared command. Apparently there can be multiple prepared commands at a time - a feature that we do not support yet.

        private bool _inPrepare         = false;
        private int  _prepareHandle     = -1;
        private bool _hiddenPrepare     = false;
        private int _preparedConnectionCloseCount = -1;
        private int _preparedConnectionReconnectCount = -1;

        private SqlParameterCollection _parameters;
        private SqlConnection          _activeConnection;
        private bool                   _dirty            = false;               // true if the user changes the commandtext or number of parameters after the command is already prepared
        private EXECTYPE               _execType         = EXECTYPE.UNPREPARED; // by default, assume the user is not sharing a connection so the command has not been prepared
        private _SqlRPC[]              _rpcArrayOf1      = null;                // Used for RPC executes
        private _SqlRPC                _rpcForEncryption = null;                // Used for sp_describe_parameter_encryption RPC executes

        // cut down on object creation and cache all these
        // cached metadata
        private _SqlMetaDataSet _cachedMetaData;
        
        // Last TaskCompletionSource for reconnect task - use for cancellation only
        TaskCompletionSource<object> _reconnectionCompletionSource = null;

#if DEBUG
        static internal int DebugForceAsyncWriteDelay { get; set; }
#endif
        internal bool InPrepare {
            get {
                return _inPrepare;
            }
        }

        /// <summary>
        /// Return if column encryption setting is enabled.
        /// The order in the below if is important since _activeConnection.Parser can throw if the 
        /// underlying tds connection is closed and we don't want to change the behavior for folks
        /// not trying to use transparent parameter encryption i.e. who don't use (SqlCommandColumnEncryptionSetting.Enabled or _activeConnection.IsColumnEncryptionSettingEnabled) here.
        /// </summary>
        internal bool IsColumnEncryptionEnabled {
            get {
                return (_columnEncryptionSetting == SqlCommandColumnEncryptionSetting.Enabled
                                 || (_columnEncryptionSetting == SqlCommandColumnEncryptionSetting.UseConnectionSetting && _activeConnection.IsColumnEncryptionSettingEnabled))
                                 && _activeConnection.Parser != null
                                 && _activeConnection.Parser.IsColumnEncryptionSupported;
            }
        }

        // Cached info for async executions
        private class CachedAsyncState {
            private int           _cachedAsyncCloseCount = -1;    // value of the connection's CloseCount property when the asyncResult was set; tracks when connections are closed after an async operation
            private TaskCompletionSource<object> _cachedAsyncResult     = null;
            private SqlConnection _cachedAsyncConnection = null;  // Used to validate that the connection hasn't changed when end the connection;
            private SqlDataReader _cachedAsyncReader     = null;
            private RunBehavior   _cachedRunBehavior     = RunBehavior.ReturnImmediately;
            private string        _cachedSetOptions      = null;
            private string        _cachedEndMethod       = null;

            internal CachedAsyncState () {
            }

            internal SqlDataReader CachedAsyncReader {
                get {return _cachedAsyncReader;}
            }
            internal  RunBehavior CachedRunBehavior {
                get {return _cachedRunBehavior;}
            }
            internal  string CachedSetOptions {
                get {return _cachedSetOptions;}
            }
            internal bool PendingAsyncOperation {
                get {return (null != _cachedAsyncResult);}
            }
            internal string EndMethodName { 
                get { return _cachedEndMethod; } 
            }

            internal bool IsActiveConnectionValid(SqlConnection activeConnection) {
                return (_cachedAsyncConnection == activeConnection && _cachedAsyncCloseCount == activeConnection.CloseCount);
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            internal void ResetAsyncState() {
                _cachedAsyncCloseCount = -1;
                _cachedAsyncResult     = null;
                if (_cachedAsyncConnection != null) {
                    _cachedAsyncConnection.AsyncCommandInProgress = false;
                    _cachedAsyncConnection = null;
                }
                _cachedAsyncReader     = null;
                _cachedRunBehavior     = RunBehavior.ReturnImmediately;
                _cachedSetOptions      = null;
                _cachedEndMethod       = null;
            }

            internal void SetActiveConnectionAndResult(TaskCompletionSource<object> completion, string endMethod, SqlConnection activeConnection) {
                Debug.Assert(activeConnection != null, "Unexpected null connection argument on SetActiveConnectionAndResult!");
                TdsParser parser = activeConnection.Parser;
                if ((parser == null) || (parser.State == TdsParserState.Closed) || (parser.State == TdsParserState.Broken)) {
                    throw ADP.ClosedConnectionError();
                }

                _cachedAsyncCloseCount = activeConnection.CloseCount;
                _cachedAsyncResult     = completion;
                if (null != activeConnection && !parser.MARSOn) {
                    if (activeConnection.AsyncCommandInProgress)
                        throw SQL.MARSUnspportedOnConnection();
                }
                _cachedAsyncConnection = activeConnection;

                // Should only be needed for non-MARS, but set anyways.
                _cachedAsyncConnection.AsyncCommandInProgress = true;
                _cachedEndMethod = endMethod;
            }

            internal void SetAsyncReaderState (SqlDataReader ds, RunBehavior runBehavior, string optionSettings) {
                _cachedAsyncReader  = ds;
                _cachedRunBehavior  = runBehavior;
                _cachedSetOptions   = optionSettings;
            }
        }

        CachedAsyncState _cachedAsyncState = null;

        private CachedAsyncState cachedAsyncState {
            get {
                if (_cachedAsyncState == null) {
                    _cachedAsyncState = new CachedAsyncState ();
                }
                return  _cachedAsyncState;
            }
        }

        // sql reader will pull this value out for each NextResult call.  It is not cumulative
        // _rowsAffected is cumulative for ExecuteNonQuery across all rpc batches
        internal int _rowsAffected = -1; // rows affected by the command

        // number of rows affected by sp_describe_parameter_encryption.
        // The below line is used only for debug asserts and not exposed publicly or impacts functionality otherwise.
        private int _rowsAffectedBySpDescribeParameterEncryption = -1;

        private SqlNotificationRequest _notification;
        private bool _notificationAutoEnlist = true;            // Notifications auto enlistment is turned on by default

        // transaction support
        private SqlTransaction _transaction;

        private StatementCompletedEventHandler _statementCompletedEventHandler;

        private TdsParserStateObject _stateObj; // this is the TDS session we're using.

        // Volatile bool used to synchronize with cancel thread the state change of an executing
        // command going from pre-processing to obtaining a stateObject.  The cancel synchronization
        // we require in the command is only from entering an Execute* API to obtaining a 
        // stateObj.  Once a stateObj is successfully obtained, cancel synchronization is handled
        // by the stateObject.
        private volatile bool _pendingCancel;

        private bool _batchRPCMode;
        private List<_SqlRPC> _RPCList;
        private _SqlRPC[] _SqlRPCBatchArray;
        private _SqlRPC[] _sqlRPCParameterEncryptionReqArray;
        private List<SqlParameterCollection>  _parameterCollectionList;
        private int     _currentlyExecutingBatch;

        /// <summary>
        /// This variable is used to keep track of which RPC batch's results are being read when reading the results of
        /// describe parameter encryption RPC requests in BatchRPCMode.
        /// </summary>
        private int _currentlyExecutingDescribeParameterEncryptionRPC;

        /// <summary>
        /// A flag to indicate if we have in-progress describe parameter encryption RPC requests.
        /// Reset to false when completed.
        /// </summary>
        private bool _isDescribeParameterEncryptionRPCCurrentlyInProgress;

        /// <summary>
        /// Return the flag that indicates if describe parameter encryption RPC requests are in-progress.
        /// </summary>
        internal bool IsDescribeParameterEncryptionRPCCurrentlyInProgress {
            get {
                return _isDescribeParameterEncryptionRPCCurrentlyInProgress;
            }
        }

        //
        //  Smi execution-specific stuff
        //
        sealed private class CommandEventSink : SmiEventSink_Default {
            private SqlCommand _command;

            internal CommandEventSink( SqlCommand command ) : base( ) {
                _command = command;
            }

            internal override void StatementCompleted( int rowsAffected ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.StatementCompleted|ADV> %d#, rowsAffected=%d.\n", _command.ObjectID, rowsAffected);
                }
                _command.InternalRecordsAffected = rowsAffected;

// 




            }

            internal override void BatchCompleted() {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.BatchCompleted|ADV> %d#.\n", _command.ObjectID);
                }
            }

            internal override void ParametersAvailable( SmiParameterMetaData[] metaData, ITypedGettersV3 parameterValues ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlCommand.CommandEventSink.ParametersAvailable|ADV> %d# metaData.Length=%d.\n", _command.ObjectID, (null!=metaData)?metaData.Length:-1);

                    if (null != metaData) {
                        for (int i=0; i < metaData.Length; i++) {
                            Bid.Trace("<sc.SqlCommand.CommandEventSink.ParametersAvailable|ADV> %d#, metaData[%d] is %ls%ls\n", 
                                        _command.ObjectID, i, metaData[i].GetType().ToString(), metaData[i].TraceString());
                        }
                    }
                }
                Debug.Assert(SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.YukonVersion);
                _command.OnParametersAvailableSmi( metaData, parameterValues );
            }

            internal override void ParameterAvailable(SmiParameterMetaData metaData, SmiTypedGetterSetter parameterValues, int ordinal)
            {
                if (Bid.AdvancedOn) {
                    if (null != metaData) {
                        Bid.Trace("<sc.SqlCommand.CommandEventSink.ParameterAvailable|ADV> %d#, metaData[%d] is %ls%ls\n", 
                                    _command.ObjectID, ordinal, metaData.GetType().ToString(), metaData.TraceString());
                    }
                }
                Debug.Assert(SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion);
                _command.OnParameterAvailableSmi(metaData, parameterValues, ordinal);
            }
        }

        private SmiContext              _smiRequestContext; // context that _smiRequest came from
        private CommandEventSink _smiEventSink;
        private SmiEventSink_DeferedProcessing _outParamEventSink;

        private CommandEventSink EventSink {
            get {
                if ( null == _smiEventSink ) {
                    _smiEventSink = new CommandEventSink( this );
                }

                _smiEventSink.Parent = InternalSmiConnection.CurrentEventSink;
                return _smiEventSink;
            }
        }

        private SmiEventSink_DeferedProcessing OutParamEventSink {
            get {
                if (null == _outParamEventSink) {
                    _outParamEventSink = new SmiEventSink_DeferedProcessing(EventSink);
                }
                else {
                    _outParamEventSink.Parent = EventSink;
                }

                return _outParamEventSink;
            }
        }


        public SqlCommand() : base() {
            GC.SuppressFinalize(this);
        }

        public SqlCommand(string cmdText) : this() {
            CommandText = cmdText;
        }

        public SqlCommand(string cmdText, SqlConnection connection) : this() {
            CommandText = cmdText;
            Connection = connection;
        }

        public SqlCommand(string cmdText, SqlConnection connection, SqlTransaction transaction) : this() {
            CommandText = cmdText;
            Connection = connection;
            Transaction = transaction;
        }

        public SqlCommand(string cmdText, SqlConnection connection, SqlTransaction transaction, SqlCommandColumnEncryptionSetting columnEncryptionSetting) : this() {
            CommandText = cmdText;
            Connection = connection;
            Transaction = transaction;
            _columnEncryptionSetting = columnEncryptionSetting;
        }

        private SqlCommand(SqlCommand from) : this() { // Clone
            CommandText = from.CommandText;
            CommandTimeout = from.CommandTimeout;
            CommandType = from.CommandType;
            Connection = from.Connection;
            DesignTimeVisible = from.DesignTimeVisible;
            Transaction = from.Transaction;
            UpdatedRowSource = from.UpdatedRowSource;
            _columnEncryptionSetting = from.ColumnEncryptionSetting;

            SqlParameterCollection parameters = Parameters;
            foreach(object parameter in from.Parameters) {
                parameters.Add((parameter is ICloneable) ? (parameter as ICloneable).Clone() : parameter);
            }
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DbCommand_Connection),
        ]
        new public SqlConnection Connection {
            get {
                return _activeConnection;
            }
            set {                
                // Don't allow the connection to be changed while in a async opperation.
                if (_activeConnection != value && _activeConnection != null) { // If new value...
                    if (cachedAsyncState.PendingAsyncOperation) { // If in pending async state, throw.
                        throw SQL.CannotModifyPropertyAsyncOperationInProgress(SQL.Connection);
                    }
                }

                // Check to see if the currently set transaction has completed.  If so,
                // null out our local reference.
                if (null != _transaction && _transaction.Connection == null) {
                    _transaction = null;
                }

                // If the connection has changes, then the request context may have changed as well
                _smiRequestContext = null;

                // Command is no longer prepared on new connection, cleanup prepare status
                if (IsPrepared) {
                    if (_activeConnection != value && _activeConnection != null) {
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try {
#if DEBUG
                            TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try {
                                tdsReliabilitySection.Start();
#endif //DEBUG
                                // cleanup
                                Unprepare();
#if DEBUG
                            }
                            finally {
                                tdsReliabilitySection.Stop();
                            }
#endif //DEBUG
                        }
                        catch (System.OutOfMemoryException) {
                            _activeConnection.InnerConnection.DoomThisConnection();
                            throw;
                        }
                        catch (System.StackOverflowException) {
                            _activeConnection.InnerConnection.DoomThisConnection();
                            throw;
                        }
                        catch (System.Threading.ThreadAbortException) {
                            _activeConnection.InnerConnection.DoomThisConnection();
                            throw;
                        }
                        catch (Exception) {
                            // we do not really care about errors in unprepare (may be the old connection went bad)                                        
                        }
                        finally {
                            // clean prepare status (even successfull Unprepare does not do that)
                            _prepareHandle = -1;
                            _execType = EXECTYPE.UNPREPARED;
                        }
                    }
                }

                _activeConnection = value; // UNDONE: Designers need this setter.  Should we block other scenarios?

                Bid.Trace("<sc.SqlCommand.set_Connection|API> %d#, %d#\n", ObjectID, ((null != value) ? value.ObjectID : -1));
            }
        }

        override protected DbConnection DbConnection { // V1.2.3300
            get {
                return Connection;
            }
            set {
                Connection = (SqlConnection)value;
            }
        }

        private SqlInternalConnectionSmi InternalSmiConnection {
            get {
                return (SqlInternalConnectionSmi)_activeConnection.InnerConnection;
            }
        }

        private SqlInternalConnectionTds InternalTdsConnection {
            get {
                return (SqlInternalConnectionTds)_activeConnection.InnerConnection;
            }
        }

        private bool IsShiloh {
            get {
                Debug.Assert(_activeConnection != null, "The active connection is null!");
                if (_activeConnection == null)
                    return false;
                return _activeConnection.IsShiloh;
            }
        }

        [
        DefaultValue(true),
        ResCategoryAttribute(Res.DataCategory_Notification),
        ResDescriptionAttribute(Res.SqlCommand_NotificationAutoEnlist),
        ]
        public bool NotificationAutoEnlist {
            get {
                return _notificationAutoEnlist;
            }
            set {
                _notificationAutoEnlist = value;
            }
         }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), // MDAC 90471
        ResCategoryAttribute(Res.DataCategory_Notification),
        ResDescriptionAttribute(Res.SqlCommand_Notification),
        ]
        public SqlNotificationRequest Notification {
            get {
                return _notification;
            }
            set {
                Bid.Trace("<sc.SqlCommand.set_Notification|API> %d#\n", ObjectID);
                _sqlDep = null;
                _notification = value;
            }
        }


        internal SqlStatistics Statistics {
            get {
                if (null != _activeConnection) {
                    if (_activeConnection.StatisticsEnabled) {
                        return _activeConnection.Statistics;
                    }
                }
                return null;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DbCommand_Transaction),
        ]
        new public SqlTransaction Transaction {
            get {
                // if the transaction object has been zombied, just return null
                if ((null != _transaction) && (null == _transaction.Connection)) { // MDAC 72720
                    _transaction = null;
                }
                return _transaction;
            }
            set {
                // Don't allow the transaction to be changed while in a async opperation.
                if (_transaction != value && _activeConnection != null) { // If new value...
                    if (cachedAsyncState.PendingAsyncOperation) { // If in pending async state, throw
                        throw SQL.CannotModifyPropertyAsyncOperationInProgress(SQL.Transaction);
                    }
                }

                // 
                Bid.Trace("<sc.SqlCommand.set_Transaction|API> %d#\n", ObjectID);
                _transaction = value;
            }
        }

        override protected DbTransaction DbTransaction { // V1.2.3300
            get {
                return Transaction;
            }
            set {
                Transaction = (SqlTransaction)value;
            }
        }

        [
        DefaultValue(""),
        Editor("Microsoft.VSDesigner.Data.SQL.Design.SqlCommandTextEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        RefreshProperties(RefreshProperties.All), // MDAC 67707
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DbCommand_CommandText),
        ]
        override public string CommandText { // V1.2.3300, XXXCommand V1.0.5000
            get {
                string value = _commandText;
                return ((null != value) ? value : ADP.StrEmpty);
            }
            set {
                if (Bid.TraceOn) {
                    Bid.Trace("<sc.SqlCommand.set_CommandText|API> %d#, '", ObjectID);
                    Bid.PutStr(value); // Use PutStr to write out entire string
                    Bid.Trace("'\n");
                }
                if (0 != ADP.SrcCompare(_commandText, value)) {
                    PropertyChanging();
                    _commandText = value;
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.TCE_SqlCommand_ColumnEncryptionSetting),
        ]
        public SqlCommandColumnEncryptionSetting ColumnEncryptionSetting {
            get {
                return _columnEncryptionSetting;
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DbCommand_CommandTimeout),
        ]
        override public int CommandTimeout { // V1.2.3300, XXXCommand V1.0.5000
            get {
                return _commandTimeout;
            }
            set {
                Bid.Trace("<sc.SqlCommand.set_CommandTimeout|API> %d#, %d\n", ObjectID, value);
                if (value < 0) {
                    throw ADP.InvalidCommandTimeout(value);
                }
                if (value != _commandTimeout) {
                    PropertyChanging();
                    _commandTimeout = value;
                }
            }
        }

        public void ResetCommandTimeout() { // V1.2.3300
            if (ADP.DefaultCommandTimeout != _commandTimeout) {
                PropertyChanging();
                _commandTimeout = ADP.DefaultCommandTimeout;
            }
        }

        private bool ShouldSerializeCommandTimeout() { // V1.2.3300
            return (ADP.DefaultCommandTimeout != _commandTimeout);
        }

        [
        DefaultValue(System.Data.CommandType.Text),
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DbCommand_CommandType),
        ]
        override public CommandType CommandType { // V1.2.3300, XXXCommand V1.0.5000
            get {
                CommandType cmdType = _commandType;
                return ((0 != cmdType) ? cmdType : CommandType.Text);
            }
            set {
                Bid.Trace("<sc.SqlCommand.set_CommandType|API> %d#, %d{ds.CommandType}\n", ObjectID, (int)value);
                if (_commandType != value) {
                    switch(value) { // @perfnote: Enum.IsDefined
                    case CommandType.Text:
                    case CommandType.StoredProcedure:
                        PropertyChanging();
                        _commandType = value;
                        break;
                    case System.Data.CommandType.TableDirect:
                        throw SQL.NotSupportedCommandType(value);
                    default:
                        throw ADP.InvalidCommandType(value);
                    }
                }
            }
        }

        // @devnote: By default, the cmd object is visible on the design surface (i.e. VS7 Server Tray)
        // to limit the number of components that clutter the design surface,
        // when the DataAdapter design wizard generates the insert/update/delete commands it will
        // set the DesignTimeVisible property to false so that cmds won't appear as individual objects
        [
        DefaultValue(true),
        DesignOnly(true),
        Browsable(false),
        EditorBrowsableAttribute(EditorBrowsableState.Never),
        ]
        public override bool DesignTimeVisible { // V1.2.3300, XXXCommand V1.0.5000
            get {
                return !_designTimeInvisible;
            }
            set {
                _designTimeInvisible = !value;
                TypeDescriptor.Refresh(this); // VS7 208845
            }
        }

        [
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        ResCategoryAttribute(Res.DataCategory_Data),
        ResDescriptionAttribute(Res.DbCommand_Parameters),
        ]
        new public SqlParameterCollection Parameters {
            get {
                if (null == this._parameters) {
                    // delay the creation of the SqlParameterCollection
                    // until user actually uses the Parameters property
                    this._parameters = new SqlParameterCollection();
                }
                return this._parameters;
            }
        }

        override protected DbParameterCollection DbParameterCollection { // V1.2.3300
            get {
                return Parameters;
            }
        }

        [
        DefaultValue(System.Data.UpdateRowSource.Both),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbCommand_UpdatedRowSource),
        ]
        override public UpdateRowSource UpdatedRowSource { // V1.2.3300, XXXCommand V1.0.5000
            get {
                return _updatedRowSource;
            }
            set {
                switch(value) { // @perfnote: Enum.IsDefined
                case UpdateRowSource.None:
                case UpdateRowSource.OutputParameters:
                case UpdateRowSource.FirstReturnedRecord:
                case UpdateRowSource.Both:
                    _updatedRowSource = value;
                    break;
                default:
                    throw ADP.InvalidUpdateRowSource(value);
                }
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_StatementCompleted),
        ResDescriptionAttribute(Res.DbCommand_StatementCompleted),
        ]
        public event StatementCompletedEventHandler StatementCompleted {
            add {
                _statementCompletedEventHandler += value;
            }
            remove {
                _statementCompletedEventHandler -= value;
            }
        }

        internal void OnStatementCompleted(int recordCount) { // V1.2.3300
             if (0 <= recordCount) {
                StatementCompletedEventHandler handler = _statementCompletedEventHandler;
                if (null != handler) {
                    try {
                        Bid.Trace("<sc.SqlCommand.OnStatementCompleted|INFO> %d#, recordCount=%d\n", ObjectID, recordCount);
                        handler(this, new StatementCompletedEventArgs(recordCount));
                    }
                    catch(Exception e) {
                        // 
                        if (!ADP.IsCatchableOrSecurityExceptionType(e)) {
                            throw;
                        }

                        ADP.TraceExceptionWithoutRethrow(e);
                    }
                }
            }
        }

        private void PropertyChanging() { // also called from SqlParameterCollection
            this.IsDirty = true;
        }

        override public void Prepare() {
            SqlConnection.ExecutePermission.Demand();

            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            // Context connection's prepare is a no-op
            if (null != _activeConnection && _activeConnection.IsContextConnection) {
                return;
            }

            SqlStatistics statistics = null;
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.Prepare|API> %d#", ObjectID);
            Bid.CorrelationTrace("<sc.SqlCommand.Prepare|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            statistics = SqlStatistics.StartTimer(Statistics);

            // only prepare if batch with parameters
            // MDAC 
            if (
                this.IsPrepared && !this.IsDirty
                || (this.CommandType == CommandType.StoredProcedure)
                ||  (
                        (System.Data.CommandType.Text == this.CommandType)
                        && (0 == GetParameterCount (_parameters))
                    )

            ) {
                if (null != Statistics) {
                    Statistics.SafeIncrement (ref Statistics._prepares);
                }
                _hiddenPrepare = false;
            }
            else {
                // Validate the command outside of the try\catch to avoid putting the _stateObj on error
                ValidateCommand(ADP.Prepare, false /*not async*/);

                bool processFinallyBlock = true;
                TdsParser bestEffortCleanupTarget = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);

                    // NOTE: The state object isn't actually needed for this, but it is still here for back-compat (since it does a bunch of checks)
                    GetStateObject();

                    // Loop through parameters ensuring that we do not have unspecified types, sizes, scales, or precisions
                    if (null != _parameters) {
                        int count = _parameters.Count;
                        for (int i = 0; i < count; ++i) {
                            _parameters[i].Prepare(this); // MDAC 67063
                        }
                    }

#if DEBUG
                    TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {
                        tdsReliabilitySection.Start();
#else
                    {
#endif //DEBUG
                        InternalPrepare();
                    }
#if DEBUG
                    finally {
                        tdsReliabilitySection.Stop();
                    }
#endif //DEBUG
                }
                catch (System.OutOfMemoryException e) {
                    processFinallyBlock = false;
                    _activeConnection.Abort(e);
                    throw;
                }
                catch (System.StackOverflowException e) {
                    processFinallyBlock = false;
                    _activeConnection.Abort(e);
                    throw;
                }
                catch (System.Threading.ThreadAbortException e)  {
                    processFinallyBlock = false;
                    _activeConnection.Abort(e);

                    SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                    throw;
                }
                catch (Exception e) {
                    processFinallyBlock = ADP.IsCatchableExceptionType(e);
                    throw;
                }
                finally {
                    if (processFinallyBlock) {
                        _hiddenPrepare = false; // The command is now officially prepared
    
                        ReliablePutStateObject();
                    }
                }
            }

            SqlStatistics.StopTimer(statistics);
            Bid.ScopeLeave(ref hscp);
        }

        private void InternalPrepare() {
            if (this.IsDirty) {
                Debug.Assert(_cachedMetaData == null || !_dirty, "dirty query should not have cached metadata!"); // can have cached metadata if dirty because of parameters
                //
                // someone changed the command text or the parameter schema so we must unprepare the command
                //
                this.Unprepare();
                this.IsDirty = false;
            }
            Debug.Assert(_execType != EXECTYPE.PREPARED, "Invalid attempt to Prepare already Prepared command!");
            Debug.Assert(_activeConnection != null, "must have an open connection to Prepare");
            Debug.Assert(null != _stateObj, "TdsParserStateObject should not be null");
            Debug.Assert(null != _stateObj.Parser, "TdsParser class should not be null in Command.Execute!");
            Debug.Assert(_stateObj.Parser == _activeConnection.Parser, "stateobject parser not same as connection parser");
            Debug.Assert(false == _inPrepare, "Already in Prepare cycle, this.inPrepare should be false!");

            // remember that the user wants to do a prepare but don't actually do an rpc
            _execType = EXECTYPE.PREPAREPENDING;
            // Note the current close count of the connection - this will tell us if the connection has been closed between calls to Prepare() and Execute
            _preparedConnectionCloseCount = _activeConnection.CloseCount;
            _preparedConnectionReconnectCount = _activeConnection.ReconnectCount;

            if (null != Statistics) {
                Statistics.SafeIncrement(ref Statistics._prepares);
            }
        }

        // SqlInternalConnectionTds needs to be able to unprepare a statement
        internal void Unprepare() {
            // Context connection's prepare is a no-op
            if (_activeConnection.IsContextConnection) {
                return;
            }

            Debug.Assert(true == IsPrepared, "Invalid attempt to Unprepare a non-prepared command!");
            Debug.Assert(_activeConnection != null, "must have an open connection to UnPrepare");
            Debug.Assert(false == _inPrepare, "_inPrepare should be false!");

            // @devnote: we're always falling back to Prepare pending
            // @devnote: This seems broken because once the command is prepared it will - always - be a
            // @devnote: prepared execution.
            // @devnote: Even replacing the parameterlist with something completely different or
            // @devnote: changing the commandtext to a non-parameterized query will result in prepared execution
            // @devnote:
            // @devnote: We need to keep the behavior for backward compatibility though (non-breaking change)
            //
            _execType = EXECTYPE.PREPAREPENDING;
            // Don't zero out the handle because we'll pass it in to sp_prepexec on the next prepare
            // Unless the close count isn't the same as when we last prepared
            if ((_activeConnection.CloseCount != _preparedConnectionCloseCount) || (_activeConnection.ReconnectCount != _preparedConnectionReconnectCount)) {
                // reset our handle
                _prepareHandle = -1;
            }

            _cachedMetaData = null;
            Bid.Trace("<sc.SqlCommand.Prepare|INFO> %d#, Command unprepared.\n", ObjectID);
        }


        // Cancel is supposed to be multi-thread safe.
        // It doesn't make sense to verify the connection exists or that it is open during cancel
        // because immediately after checkin the connection can be closed or removed via another thread.
        //
        override public void Cancel() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.Cancel|API> %d#", ObjectID);
            
            Bid.CorrelationTrace("<sc.SqlCommand.Cancel|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);

            SqlStatistics statistics = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);

                // If we are in reconnect phase simply cancel the waiting task
                var reconnectCompletionSource = _reconnectionCompletionSource;
                if (reconnectCompletionSource != null) {
                    if (reconnectCompletionSource.TrySetCanceled()) {                        
                        return;
                    }
                }
                
                // the pending data flag means that we are awaiting a response or are in the middle of proccessing a response
                // if we have no pending data, then there is nothing to cancel
                // if we have pending data, but it is not a result of this command, then we don't cancel either.  Note that
                // this model is implementable because we only allow one active command at any one time.  This code
                // will have to change we allow multiple outstanding batches

                // 
                if (null == _activeConnection) {
                    return;
                }
                SqlInternalConnectionTds connection = (_activeConnection.InnerConnection as SqlInternalConnectionTds);
                if (null == connection) {  // Fail with out locking
                     return;
                }

                // The lock here is to protect against the command.cancel / connection.close race condition
                // The SqlInternalConnectionTds is set to OpenBusy during close, once this happens the cast below will fail and 
                // the command will no longer be cancelable.  It might be desirable to be able to cancel the close opperation, but this is
                // outside of the scope of Whidbey RTM.  See (SqlConnection::Close) for other lock.
                lock (connection) {                                              
                    if (connection != (_activeConnection.InnerConnection as SqlInternalConnectionTds)) { // make sure the connection held on the active connection is what we have stored in our temp connection variable, if not between getting "connection" and takeing the lock, the connection has been closed
                        return;
                    }
                    
                    TdsParser parser = connection.Parser;
                    if (null == parser) {
                        return;
                    }

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
                            bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);

                            if (!_pendingCancel) { // Do nothing if aleady pending.
                                // Before attempting actual cancel, set the _pendingCancel flag to false.
                                // This denotes to other thread before obtaining stateObject from the
                                // session pool that there is another thread wishing to cancel.
                                // The period in question is between entering the ExecuteAPI and obtaining 
                                // a stateObject.
                                _pendingCancel = true;

                                TdsParserStateObject stateObj = _stateObj;
                                if (null != stateObj) {
                                    stateObj.Cancel(ObjectID);
                                }
                                else {
                                    SqlDataReader reader = connection.FindLiveReader(this);
                                    if (reader != null) {
                                        reader.Cancel(ObjectID);
                                    }
                                }
                            }
                        }
#if DEBUG
                        finally {
                            tdsReliabilitySection.Stop();
                        }
#endif //DEBUG
                    }
                    catch (System.OutOfMemoryException e) {
                        _activeConnection.Abort(e);
                        throw;
                    }
                    catch (System.StackOverflowException e) {
                        _activeConnection.Abort(e);
                        throw;
                    }
                    catch (System.Threading.ThreadAbortException e)  {
                        _activeConnection.Abort(e);
                        SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                        throw;
                    }
                }
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref hscp);
            }
        }

        new public SqlParameter CreateParameter() {
            return new SqlParameter();
        }

        override protected DbParameter CreateDbParameter() {
            return CreateParameter();
        }

        override protected void Dispose(bool disposing) {
            if (disposing) { // release mananged objects

                // V1.0, V1.1 did not reset the Connection, Parameters, CommandText, WebData 100524
                //_parameters = null;
                //_activeConnection = null;
                //_statistics = null;
                //CommandText = null;
                _cachedMetaData = null;
            }
            // release unmanaged objects
            base.Dispose(disposing);
        }

        override public object ExecuteScalar() {
            SqlConnection.ExecutePermission.Demand();

            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            SqlStatistics statistics = null;
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.ExecuteScalar|API> %d#", ObjectID);
            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteScalar|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);

            bool success = false;
            int? sqlExceptionNumber = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                WriteBeginExecuteEvent();
                SqlDataReader ds;
                ds = RunExecuteReader(0, RunBehavior.ReturnImmediately, true, ADP.ExecuteScalar);
                object result = CompleteExecuteScalar(ds, false);
                success = true;
                return result;
            }
            catch (SqlException ex) {
                sqlExceptionNumber = ex.Number;
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref hscp);
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous: true);
            }
        }

        private object CompleteExecuteScalar(SqlDataReader ds, bool returnSqlValue) {
            object retResult = null;

            try {
                if (ds.Read()) {
                    if (ds.FieldCount > 0) {
                        if (returnSqlValue) {
                            retResult = ds.GetSqlValue(0);
                        }
                        else {
                            retResult = ds.GetValue(0);
                        }
                    }
                }
            }
            finally {
                // clean off the wire
                ds.Close();
            }

            return retResult;
        }

        override public int ExecuteNonQuery() {
            SqlConnection.ExecutePermission.Demand();

            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            SqlStatistics statistics = null;
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.ExecuteNonQuery|API> %d#", ObjectID);
            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteNonQuery|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            bool success = false;
            int? sqlExceptionNumber = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                WriteBeginExecuteEvent();
                InternalExecuteNonQuery(null, ADP.ExecuteNonQuery, false, CommandTimeout);
                success = true;
                return _rowsAffected;
            }
            catch (SqlException ex) {
                sqlExceptionNumber = ex.Number;
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref hscp);
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous : true);
            }
        }

        // Handles in-proc execute-to-pipe functionality
        //  Identical to ExecuteNonQuery
        internal void ExecuteToPipe( SmiContext pipeContext ) {
            SqlConnection.ExecutePermission.Demand();

            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            SqlStatistics statistics = null;
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.ExecuteToPipe|INFO> %d#", ObjectID);
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                InternalExecuteNonQuery(null, ADP.ExecuteNonQuery, true, CommandTimeout);
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref hscp);
            }
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteNonQuery() {
            // BeginExecuteNonQuery will track ExecutionTime for us
            return BeginExecuteNonQuery(null, null);
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback, object stateObject) {
            Bid.CorrelationTrace("<sc.SqlCommand.BeginExecuteNonQuery|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            SqlConnection.ExecutePermission.Demand();
            return BeginExecuteNonQueryInternal(callback, stateObject, 0);
        }

        private IAsyncResult BeginExecuteNonQueryAsync(AsyncCallback callback, object stateObject) {
            return BeginExecuteNonQueryInternal(callback, stateObject, CommandTimeout, asyncWrite:true);
        }

        private IAsyncResult BeginExecuteNonQueryInternal(AsyncCallback callback, object stateObject, int timeout, bool asyncWrite = false) {
            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            ValidateAsyncCommand(); // Special case - done outside of try/catches to prevent putting a stateObj
                                    // back into pool when we should not.
            
            SqlStatistics statistics = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                WriteBeginExecuteEvent();
                TaskCompletionSource<object> completion = new TaskCompletionSource<object>(stateObject);

                try { // InternalExecuteNonQuery already has reliability block, but if failure will not put stateObj back into pool.
                    Task execNQ = InternalExecuteNonQuery(completion, ADP.BeginExecuteNonQuery, false, timeout, asyncWrite);
                    if (execNQ != null) {
                        AsyncHelper.ContinueTask(execNQ, completion, () => BeginExecuteNonQueryInternalReadStage(completion));
                    }
                    else {
                        BeginExecuteNonQueryInternalReadStage(completion);
                    }
                }
                catch (Exception e) {
                    if (!ADP.IsCatchableOrSecurityExceptionType(e)) {
                        // If not catchable - the connection has already been caught and doomed in RunExecuteReader.
                        throw;
                    }

                    // For async, RunExecuteReader will never put the stateObj back into the pool, so do so now.
                    ReliablePutStateObject(); 
                    throw;
                }

                // Add callback after work is done to avoid overlapping Begin\End methods
                if (callback != null) {
                    completion.Task.ContinueWith((t) => callback(t), TaskScheduler.Default);
                }

                return completion.Task;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
            }
        }

        private void BeginExecuteNonQueryInternalReadStage(TaskCompletionSource<object> completion) {
            // Read SNI does not have catches for async exceptions, handle here.
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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    // must finish caching information before ReadSni which can activate the callback before returning
                    cachedAsyncState.SetActiveConnectionAndResult(completion, ADP.EndExecuteNonQuery, _activeConnection);
                    _stateObj.ReadSni(completion);
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
            catch (Exception) {
                // Similarly, if an exception occurs put the stateObj back into the pool.
                // and reset async cache information to allow a second async execute
                if (null != _cachedAsyncState) {
                    _cachedAsyncState.ResetAsyncState();
                }
                ReliablePutStateObject();
                throw;
            }
        }

        private void VerifyEndExecuteState(Task completionTask, String endMethod) {
            if (null == completionTask) {
                throw ADP.ArgumentNull("asyncResult");
            }
            if (completionTask.IsCanceled) {
                if (_stateObj != null) {
                    _stateObj.Parser.State = TdsParserState.Broken; // We failed to respond to attention, we have to quit!
                    _stateObj.Parser.Connection.BreakConnection();
                    _stateObj.Parser.ThrowExceptionAndWarning(_stateObj);
                }
                else {
                    Debug.Assert(_reconnectionCompletionSource == null || _reconnectionCompletionSource.Task.IsCanceled, "ReconnectCompletionSource should be null or cancelled");
                    throw SQL.CR_ReconnectionCancelled();
                }
            }
            else if (completionTask.IsFaulted) {
                throw completionTask.Exception.InnerException;
            }

            // If transparent parameter encryption was attempted, then we need to skip other checks like those on EndMethodName
            // since we want to wait for async results before checking those fields.
            if (IsColumnEncryptionEnabled) {
                if (_activeConnection.State != ConnectionState.Open) {
                    // If the connection is not 'valid' then it was closed while we were executing
                    throw ADP.ClosedConnectionError();
                }

                return;
            }

            if (cachedAsyncState.EndMethodName == null) {
                throw ADP.MethodCalledTwice(endMethod);
            }
            if (endMethod != cachedAsyncState.EndMethodName) {
                throw ADP.MismatchedAsyncResult(cachedAsyncState.EndMethodName, endMethod);
            }
            if ((_activeConnection.State != ConnectionState.Open) || (!cachedAsyncState.IsActiveConnectionValid(_activeConnection))) {
                // If the connection is not 'valid' then it was closed while we were executing
                throw ADP.ClosedConnectionError();
            }
        }

        private void WaitForAsyncResults(IAsyncResult asyncResult) {
            Task completionTask = (Task) asyncResult;
            if (!asyncResult.IsCompleted) {
                asyncResult.AsyncWaitHandle.WaitOne();
            }
            _stateObj._networkPacketTaskSource = null;
            _activeConnection.GetOpenTdsConnection().DecrementAsyncCount();
        }

        public int EndExecuteNonQuery(IAsyncResult asyncResult) {
            try {
                return EndExecuteNonQueryInternal(asyncResult);
            } 
            finally {
                Bid.CorrelationTrace("<sc.SqlCommand.EndExecuteNonQuery|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            }
        }

        private void ThrowIfReconnectionHasBeenCanceled() {
            if (_stateObj == null) {
                var reconnectionCompletionSource = _reconnectionCompletionSource;
                if (reconnectionCompletionSource != null && reconnectionCompletionSource.Task.IsCanceled) {
                    throw SQL.CR_ReconnectionCancelled();
                }
            }
        }
        
        private int EndExecuteNonQueryAsync(IAsyncResult asyncResult) {
            Bid.CorrelationTrace("<sc.SqlCommand.EndExecuteNonQueryAsync|Info|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);

            Exception asyncException = ((Task)asyncResult).Exception;
            if (asyncException != null) {
                // Leftover exception from the Begin...InternalReadStage
                ReliablePutStateObject();
                throw asyncException.InnerException;
            }
            else {
                ThrowIfReconnectionHasBeenCanceled();
                // lock on _stateObj prevents ----s with close/cancel.
                lock (_stateObj) {
                    return EndExecuteNonQueryInternal(asyncResult);
                }
            }
        }

        private int EndExecuteNonQueryInternal(IAsyncResult asyncResult) {
            SqlStatistics statistics = null;

            TdsParser bestEffortCleanupTarget = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            bool success = false;
            int? sqlExceptionNumber = null;
            try {
#if DEBUG
                TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    tdsReliabilitySection.Start();
#else
                {
#endif //DEBUG
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    statistics = SqlStatistics.StartTimer(Statistics);
                    VerifyEndExecuteState((Task)asyncResult, ADP.EndExecuteNonQuery);
                    WaitForAsyncResults(asyncResult);

                    // If Transparent parameter encryption was attempted, then we would have skipped the below 
                    // checks in VerifyEndExecuteState since we wanted to wait for WaitForAsyncResults to complete.
                    if (IsColumnEncryptionEnabled) {
                        if (cachedAsyncState.EndMethodName == null) {
                            throw ADP.MethodCalledTwice(ADP.EndExecuteNonQuery);
                        }

                        if (ADP.EndExecuteNonQuery != cachedAsyncState.EndMethodName) {
                            throw ADP.MismatchedAsyncResult(cachedAsyncState.EndMethodName, ADP.EndExecuteNonQuery);
                        }

                        if (!cachedAsyncState.IsActiveConnectionValid(_activeConnection)) {
                            // If the connection is not 'valid' then it was closed while we were executing
                            throw ADP.ClosedConnectionError();
                        }
                    }

                    bool processFinallyBlock = true;
                    try {
                        NotifyDependency();
                        CheckThrowSNIException();

                        // only send over SQL Batch command if we are not a stored proc and have no parameters
                        if ((System.Data.CommandType.Text == this.CommandType) && (0 == GetParameterCount(_parameters))) {
                            try {
                                bool dataReady;
                                Debug.Assert(_stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
                                bool result = _stateObj.Parser.TryRun(RunBehavior.UntilDone, this, null, null, _stateObj, out dataReady);
                                if (!result) { throw SQL.SynchronousCallMayNotPend(); }
                            }
                            finally {
                                cachedAsyncState.ResetAsyncState();
                            }
                        }
                        else { // otherwise, use a full-fledged execute that can handle params and stored procs
                            SqlDataReader reader = CompleteAsyncExecuteReader();
                            if (null != reader) {
                                reader.Close();
                            }
                        }
                    }
                    catch (SqlException e) {
                        sqlExceptionNumber = e.Number;
                        throw;
                    }
                    catch (Exception e) {
                        processFinallyBlock = ADP.IsCatchableExceptionType(e);
                        throw;
                    }
                    finally {
                        if (processFinallyBlock) {
                            PutStateObject();
                        }
                    }

                    Debug.Assert(null == _stateObj, "non-null state object in EndExecuteNonQuery");
                    success = true;
                    return _rowsAffected;
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
            catch (Exception e) {
                if (cachedAsyncState != null) {
                    cachedAsyncState.ResetAsyncState();
                };
                if (ADP.IsCatchableExceptionType(e)) {
                    ReliablePutStateObject();
                };
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous: false);
            }
        }

        private Task InternalExecuteNonQuery(TaskCompletionSource<object> completion, string methodName, bool sendToPipe, int timeout, bool asyncWrite = false) {
            bool async = (null != completion);

            SqlStatistics statistics = Statistics;
            _rowsAffected = -1;

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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    // @devnote: this function may throw for an invalid connection
                    // @devnote: returns false for empty command text
                    ValidateCommand(methodName, async);
                    CheckNotificationStateAndAutoEnlist(); // Only call after validate - requires non null connection!

                    Task task = null;

                    // only send over SQL Batch command if we are not a stored proc and have no parameters and not in batch RPC mode
                    if ( _activeConnection.IsContextConnection ) {
                        if (null != statistics) {
                            statistics.SafeIncrement(ref statistics._unpreparedExecs);
                        }

                        RunExecuteNonQuerySmi( sendToPipe );
                    }
                    else if (!BatchRPCMode && (System.Data.CommandType.Text == this.CommandType) && (0 == GetParameterCount(_parameters))) {
                        Debug.Assert( !sendToPipe, "trying to send non-context command to pipe" );
                        if (null != statistics) {
                            if (!this.IsDirty && this.IsPrepared) {
                                statistics.SafeIncrement(ref statistics._preparedExecs);
                            }
                            else {
                                statistics.SafeIncrement(ref statistics._unpreparedExecs);
                            }
                        }

                        task = RunExecuteNonQueryTds(methodName, async, timeout, asyncWrite);
                    }
                    else  { // otherwise, use a full-fledged execute that can handle params and stored procs
                        Debug.Assert( !sendToPipe, "trying to send non-context command to pipe" );
                        Bid.Trace("<sc.SqlCommand.ExecuteNonQuery|INFO> %d#, Command executed as RPC.\n", ObjectID);
                        SqlDataReader reader = RunExecuteReader(0, RunBehavior.UntilDone, false, methodName, completion, timeout, out task, asyncWrite);
                        if (null!=reader) {
                            if (task != null) {
                                task = AsyncHelper.CreateContinuationTask(task, () => reader.Close());
                            }
                            else {
                                reader.Close();
                            }
                        }
                    }
                    Debug.Assert(async || null == _stateObj, "non-null state object in InternalExecuteNonQuery");
                    return task;
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e)  {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
        }

        public XmlReader ExecuteXmlReader() {
            SqlConnection.ExecutePermission.Demand();

            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            SqlStatistics statistics = null;
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.ExecuteXmlReader|API> %d#", ObjectID);
            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteXmlReader|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            bool success = false;
            int? sqlExceptionNumber = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                WriteBeginExecuteEvent();

                // use the reader to consume metadata
                SqlDataReader ds;
                ds = RunExecuteReader(CommandBehavior.SequentialAccess, RunBehavior.ReturnImmediately, true, ADP.ExecuteXmlReader);
                XmlReader result = CompleteXmlReader(ds);
                success = true;
                return result;
            }
            catch (SqlException ex) {
                sqlExceptionNumber = ex.Number;
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref hscp);
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous : true);
            }
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteXmlReader() {
            // BeginExecuteXmlReader will track executiontime
            return BeginExecuteXmlReader(null, null);
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteXmlReader(AsyncCallback callback, object stateObject) {
            Bid.CorrelationTrace("<sc.SqlCommand.BeginExecuteXmlReader|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            SqlConnection.ExecutePermission.Demand();   
            return BeginExecuteXmlReaderInternal(callback, stateObject, 0);
        }

        private IAsyncResult BeginExecuteXmlReaderAsync(AsyncCallback callback, object stateObject) {
            return BeginExecuteXmlReaderInternal(callback, stateObject, CommandTimeout, asyncWrite:true);
        }

        private IAsyncResult BeginExecuteXmlReaderInternal(AsyncCallback callback, object stateObject, int timeout, bool asyncWrite = false) {        
            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            ValidateAsyncCommand(); // Special case - done outside of try/catches to prevent putting a stateObj
                                    // back into pool when we should not.

            SqlStatistics statistics = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                WriteBeginExecuteEvent();
                TaskCompletionSource<object> completion = new TaskCompletionSource<object>(stateObject);

                Task writeTask;
                try { // InternalExecuteNonQuery already has reliability block, but if failure will not put stateObj back into pool.
                    RunExecuteReader(CommandBehavior.SequentialAccess, RunBehavior.ReturnImmediately, true, ADP.BeginExecuteXmlReader, completion, timeout, out writeTask, asyncWrite);
                }
                catch (Exception e) {
                    if (!ADP.IsCatchableOrSecurityExceptionType(e)) {
                        // If not catchable - the connection has already been caught and doomed in RunExecuteReader.
                        throw;
                    }
        
                    // For async, RunExecuteReader will never put the stateObj back into the pool, so do so now.
                    ReliablePutStateObject(); 
                    throw;
                }

                if (writeTask != null) {
                    AsyncHelper.ContinueTask(writeTask, completion, () => BeginExecuteXmlReaderInternalReadStage(completion));
                }
                else {
                    BeginExecuteXmlReaderInternalReadStage(completion);
                }

                // Add callback after work is done to avoid overlapping Begin\End methods
                if (callback != null) {
                    completion.Task.ContinueWith((t) => callback(t), TaskScheduler.Default);
                }
                return completion.Task;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
            }
        }

        private void BeginExecuteXmlReaderInternalReadStage(TaskCompletionSource<object> completion) {
            Debug.Assert(completion != null,"Completion source should not be null");
            // Read SNI does not have catches for async exceptions, handle here.
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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    // must finish caching information before ReadSni which can activate the callback before returning
                    cachedAsyncState.SetActiveConnectionAndResult(completion, ADP.EndExecuteXmlReader, _activeConnection);
                    _stateObj.ReadSni(completion);
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                completion.TrySetException(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                completion.TrySetException(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                completion.TrySetException(e);
                throw;
            }
            catch (Exception e) {
                // Similarly, if an exception occurs put the stateObj back into the pool.
                // and reset async cache information to allow a second async execute
                if (null != _cachedAsyncState) {
                    _cachedAsyncState.ResetAsyncState();
                }
                ReliablePutStateObject();
                completion.TrySetException(e);
            }
        }

        public XmlReader EndExecuteXmlReader(IAsyncResult asyncResult) {
            try {
                return EndExecuteXmlReaderInternal(asyncResult);
            }
            finally {
                Bid.CorrelationTrace("<sc.SqlCommand.EndExecuteXmlReader|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            }
        }

        private XmlReader EndExecuteXmlReaderAsync(IAsyncResult asyncResult) {
            Bid.CorrelationTrace("<sc.SqlCommand.EndExecuteXmlReaderAsync|Info|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);

            Exception asyncException = ((Task)asyncResult).Exception;
            if (asyncException != null) {
                // Leftover exception from the Begin...InternalReadStage
                ReliablePutStateObject();
                throw asyncException.InnerException;
            }
            else {
                ThrowIfReconnectionHasBeenCanceled();
                // lock on _stateObj prevents ----s with close/cancel.
                lock (_stateObj) {
                    return EndExecuteXmlReaderInternal(asyncResult);
                }
            }
        }

        private XmlReader EndExecuteXmlReaderInternal(IAsyncResult asyncResult) {
            bool success = false;
            int? sqlExceptionNumber = null;
            try {
                XmlReader result = CompleteXmlReader(InternalEndExecuteReader(asyncResult, ADP.EndExecuteXmlReader));
                success = true;
                return result;
            }
            catch (SqlException e){
                sqlExceptionNumber = e.Number;
                if (cachedAsyncState != null) {
                    cachedAsyncState.ResetAsyncState();
                };

                //  SqlException is always catchable 
                ReliablePutStateObject();
                throw;
            }
            catch (Exception e) {
                if (cachedAsyncState != null) {
                    cachedAsyncState.ResetAsyncState();
                };
                if (ADP.IsCatchableExceptionType(e)) {
                    ReliablePutStateObject();
                };
                throw;
            }
            finally {
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous : false);
            }
        }

        private XmlReader CompleteXmlReader(SqlDataReader ds) {
            XmlReader xr = null;

            SmiExtendedMetaData[] md = ds.GetInternalSmiMetaData();
            bool isXmlCapable = (null != md && md.Length == 1 && (md[0].SqlDbType == SqlDbType.NText 
                                                         || md[0].SqlDbType == SqlDbType.NVarChar 
                                                         || md[0].SqlDbType == SqlDbType.Xml));

            if (isXmlCapable) {
                try {
                    SqlStream sqlBuf = new SqlStream(ds, true /*addByteOrderMark*/, (md[0].SqlDbType == SqlDbType.Xml) ? false : true /*process all rows*/);
                    xr = sqlBuf.ToXmlReader();
                }
                catch (Exception e) {
                    if (ADP.IsCatchableExceptionType(e)) {
                        ds.Close();
                    }
                    throw;
                }
            }
            if (xr == null) {
                ds.Close();
                throw SQL.NonXmlResult();
            }
            return xr;
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader() {
            return BeginExecuteReader(null, null, CommandBehavior.Default);
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject) {
            return BeginExecuteReader(callback, stateObject, CommandBehavior.Default);
        }

        override protected DbDataReader ExecuteDbDataReader(CommandBehavior behavior) {
            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteDbDataReader|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            return ExecuteReader(behavior, ADP.ExecuteReader);
        }

        new public SqlDataReader ExecuteReader() {
            SqlStatistics statistics = null;
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.ExecuteReader|API> %d#", ObjectID);
            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteReader|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                return ExecuteReader(CommandBehavior.Default, ADP.ExecuteReader);
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                Bid.ScopeLeave(ref hscp);
            }
        }

        new public SqlDataReader ExecuteReader(CommandBehavior behavior) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlCommand.ExecuteReader|API> %d#, behavior=%d{ds.CommandBehavior}", ObjectID, (int)behavior);
            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteReader|API|Correlation> ObjectID%d#, behavior=%d{ds.CommandBehavior}, ActivityID %ls\n", ObjectID, (int)behavior);
            try {
                return ExecuteReader(behavior, ADP.ExecuteReader);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(CommandBehavior behavior) {
            return BeginExecuteReader(null, null, behavior);
        }

        [System.Security.Permissions.HostProtectionAttribute(ExternalThreading=true)]
        public IAsyncResult BeginExecuteReader(AsyncCallback callback, object stateObject, CommandBehavior behavior) {
            Bid.CorrelationTrace("<sc.SqlCommand.BeginExecuteReader|API|Correlation> ObjectID%d#, behavior=%d{ds.CommandBehavior}, ActivityID %ls\n", ObjectID, (int)behavior);
            SqlConnection.ExecutePermission.Demand();
            return BeginExecuteReaderInternal(behavior, callback, stateObject, 0);
        }

        internal SqlDataReader ExecuteReader(CommandBehavior behavior, string method) {
            SqlConnection.ExecutePermission.Demand(); // TODO: Need to move this to public methods...

            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            SqlStatistics statistics = null;

            TdsParser bestEffortCleanupTarget = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            bool success = false;
            int? sqlExceptionNumber = null;
            try {
                WriteBeginExecuteEvent();
#if DEBUG
                TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();

                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    tdsReliabilitySection.Start();
#else
                {
#endif //DEBUG
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    statistics = SqlStatistics.StartTimer(Statistics);
                    SqlDataReader result = RunExecuteReader(behavior, RunBehavior.ReturnImmediately, true, method);
                    success = true;
                    return result;
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (SqlException e) {
                sqlExceptionNumber = e.Number;
                throw;
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e)  {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous : true);
            }
        }

        public SqlDataReader EndExecuteReader(IAsyncResult asyncResult) {
            try {
                return EndExecuteReaderInternal(asyncResult);
            }          
            finally {
                Bid.CorrelationTrace("<sc.SqlCommand.EndExecuteReader|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            }
        }

        private SqlDataReader EndExecuteReaderAsync(IAsyncResult asyncResult) {
            Bid.CorrelationTrace("<sc.SqlCommand.EndExecuteReaderAsync|Info|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);

            Exception asyncException = ((Task)asyncResult).Exception;
            if (asyncException != null) {
                // Leftover exception from the Begin...InternalReadStage
                ReliablePutStateObject();
                throw asyncException.InnerException;
            }
            else {
                ThrowIfReconnectionHasBeenCanceled();
                // lock on _stateObj prevents ----s with close/cancel.
                lock (_stateObj) {
                        return EndExecuteReaderInternal(asyncResult);
                }
            }
        }

        private SqlDataReader EndExecuteReaderInternal(IAsyncResult asyncResult) {
            SqlStatistics statistics = null;
            bool success = false;
            int? sqlExceptionNumber = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                SqlDataReader result = InternalEndExecuteReader(asyncResult, ADP.EndExecuteReader);
                success = true;
                return result;
            }
            catch (SqlException e) {
                sqlExceptionNumber = e.Number;
                if (cachedAsyncState != null)
                {
                    cachedAsyncState.ResetAsyncState();
                };

                //  SqlException is always catchable 
                ReliablePutStateObject();
                throw;
            }
            catch (Exception e) {
                if (cachedAsyncState != null) {
                    cachedAsyncState.ResetAsyncState();
                };
                if (ADP.IsCatchableExceptionType(e)) {
                    ReliablePutStateObject();
                };
                throw;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
                WriteEndExecuteEvent(success, sqlExceptionNumber, synchronous : false);
            }
        }

        private IAsyncResult BeginExecuteReaderAsync(CommandBehavior behavior, AsyncCallback callback, object stateObject) {
            return BeginExecuteReaderInternal(behavior, callback, stateObject, CommandTimeout, asyncWrite:true);
        }

        private IAsyncResult BeginExecuteReaderInternal(CommandBehavior behavior, AsyncCallback callback, object stateObject, int timeout, bool asyncWrite = false) {        
            // Reset _pendingCancel upon entry into any Execute - used to synchronize state
            // between entry into Execute* API and the thread obtaining the stateObject.
            _pendingCancel = false;

            SqlStatistics statistics = null;
            try {
                statistics = SqlStatistics.StartTimer(Statistics);
                WriteBeginExecuteEvent();
                TaskCompletionSource<object> completion = new TaskCompletionSource<object>(stateObject);

                ValidateAsyncCommand(); // Special case - done outside of try/catches to prevent putting a stateObj
                                        // back into pool when we should not.

                Task writeTask = null;
                try { // InternalExecuteNonQuery already has reliability block, but if failure will not put stateObj back into pool.
                    RunExecuteReader(behavior, RunBehavior.ReturnImmediately, true, ADP.BeginExecuteReader, completion, timeout, out writeTask, asyncWrite);
                }
                catch (Exception e) {
                    if (!ADP.IsCatchableOrSecurityExceptionType(e)) {
                        // If not catchable - the connection has already been caught and doomed in RunExecuteReader.
                        throw;
                    }
    
                    // For async, RunExecuteReader will never put the stateObj back into the pool, so do so now.
                    ReliablePutStateObject(); 
                    throw;
                }

                if (writeTask != null ) {
                    AsyncHelper.ContinueTask(writeTask,completion,()=> BeginExecuteReaderInternalReadStage(completion));
                }
                else {
                    BeginExecuteReaderInternalReadStage(completion);
                }

                // Add callback after work is done to avoid overlapping Begin\End methods
                if (callback != null) {
                    completion.Task.ContinueWith((t) => callback(t), TaskScheduler.Default);
                }
                return completion.Task;
            }
            finally {
                SqlStatistics.StopTimer(statistics);
            }
        }

        private void BeginExecuteReaderInternalReadStage(TaskCompletionSource<object> completion) {
            Debug.Assert(completion != null,"CompletionSource should not be null");
            // Read SNI does not have catches for async exceptions, handle here.
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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    // must finish caching information before ReadSni which can activate the callback before returning
                    cachedAsyncState.SetActiveConnectionAndResult(completion, ADP.EndExecuteReader, _activeConnection);
                    _stateObj.ReadSni(completion);
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                completion.TrySetException(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                completion.TrySetException(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                completion.TrySetException(e);
                throw;
            }
            catch (Exception e) {
                // Similarly, if an exception occurs put the stateObj back into the pool.
                // and reset async cache information to allow a second async execute
                if (null != _cachedAsyncState) {
                    _cachedAsyncState.ResetAsyncState();
                }
                ReliablePutStateObject();
                completion.TrySetException(e);
            }
        }

        private SqlDataReader InternalEndExecuteReader(IAsyncResult asyncResult, string endMethod) {

            VerifyEndExecuteState((Task) asyncResult, endMethod);
            WaitForAsyncResults(asyncResult);

            // If Transparent parameter encryption was attempted, then we would have skipped the below 
            // checks in VerifyEndExecuteState since we wanted to wait for WaitForAsyncResults to complete.
            if (IsColumnEncryptionEnabled) {
                if (cachedAsyncState.EndMethodName == null) {
                    throw ADP.MethodCalledTwice(endMethod);
                }

                if (endMethod != cachedAsyncState.EndMethodName) {
                    throw ADP.MismatchedAsyncResult(cachedAsyncState.EndMethodName, endMethod);
                }

                if (!cachedAsyncState.IsActiveConnectionValid(_activeConnection)) {
                    // If the connection is not 'valid' then it was closed while we were executing
                    throw ADP.ClosedConnectionError();
                }
            }

            CheckThrowSNIException();

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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    SqlDataReader reader = CompleteAsyncExecuteReader();
                    Debug.Assert(null == _stateObj, "non-null state object in InternalEndExecuteReader");
                    return reader;
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e)  {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) {

            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteNonQueryAsync|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            SqlConnection.ExecutePermission.Demand();   

            TaskCompletionSource<int> source = new TaskCompletionSource<int>();

            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            if (cancellationToken.CanBeCanceled) {
                if (cancellationToken.IsCancellationRequested) {
                    source.SetCanceled();
                    return source.Task;
                }
                registration = cancellationToken.Register(CancelIgnoreFailure);
            }

            Task<int> returnedTask = source.Task;
            try {
                RegisterForConnectionCloseNotification(ref returnedTask);

                Task<int>.Factory.FromAsync(BeginExecuteNonQueryAsync, EndExecuteNonQueryAsync, null).ContinueWith((t) => {
                    registration.Dispose();
                    if (t.IsFaulted) {
                        Exception e = t.Exception.InnerException;
                        source.SetException(e);
                    }
                    else {
                        if (t.IsCanceled) {
                            source.SetCanceled();
                        }
                        else {
                            source.SetResult(t.Result);
                        }
                    }
                }, TaskScheduler.Default);
            } 
            catch (Exception e) {
                source.SetException(e);
            }

            return returnedTask;
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) {
            return ExecuteReaderAsync(behavior, cancellationToken).ContinueWith<DbDataReader>((result) => {
                if (result.IsFaulted) {
                    throw result.Exception.InnerException;
                }
                return result.Result;
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.NotOnCanceled, TaskScheduler.Default);
        }

        new public Task<SqlDataReader> ExecuteReaderAsync() {
            return ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);
        }

        new public Task<SqlDataReader> ExecuteReaderAsync(CommandBehavior behavior) {
            return ExecuteReaderAsync(behavior, CancellationToken.None);
        }

        new public Task<SqlDataReader> ExecuteReaderAsync(CancellationToken cancellationToken) {
            return ExecuteReaderAsync(CommandBehavior.Default, cancellationToken);
        }

        new public Task<SqlDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken) {

            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteReaderAsync|API|Correlation> ObjectID%d#, behavior=%d{ds.CommandBehavior}, ActivityID %ls\n", ObjectID, (int)behavior);
            SqlConnection.ExecutePermission.Demand();   

            TaskCompletionSource<SqlDataReader> source = new TaskCompletionSource<SqlDataReader>();

            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            if (cancellationToken.CanBeCanceled) {
                if (cancellationToken.IsCancellationRequested) {
                    source.SetCanceled();
                    return source.Task;
                }
                registration = cancellationToken.Register(CancelIgnoreFailure);
            }
            
            Task<SqlDataReader> returnedTask = source.Task;
            try {
                RegisterForConnectionCloseNotification(ref returnedTask);

                Task<SqlDataReader>.Factory.FromAsync(BeginExecuteReaderAsync, EndExecuteReaderAsync, behavior, null).ContinueWith((t) => {                    
                    registration.Dispose();
                    if (t.IsFaulted) {
                        Exception e = t.Exception.InnerException;
                        source.SetException(e);
                    }
                    else {
                        if (t.IsCanceled) {
                            source.SetCanceled();
                        }
                        else {
                            source.SetResult(t.Result);
                        }
                    }
                }, TaskScheduler.Default);
            } 
            catch (Exception e) {
                source.SetException(e);
            }

            return returnedTask;
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return ExecuteReaderAsync(cancellationToken).ContinueWith((executeTask) => {
                TaskCompletionSource<object> source = new TaskCompletionSource<object>();
                if (executeTask.IsCanceled) {
                    source.SetCanceled();
                }
                else if (executeTask.IsFaulted) {
                    source.SetException(executeTask.Exception.InnerException);
                }
                else {
                    SqlDataReader reader = executeTask.Result;
                    reader.ReadAsync(cancellationToken).ContinueWith((readTask) => {
                        try {
                            if (readTask.IsCanceled) {
                                reader.Dispose();
                                source.SetCanceled();
                            }
                            else if (readTask.IsFaulted) {
                                reader.Dispose();
                                source.SetException(readTask.Exception.InnerException);
                            } 
                            else {
                                Exception exception = null;
                                object result = null;
                                try {
                                    bool more = readTask.Result;
                                    if (more && reader.FieldCount > 0) {
                                        try {
                                            result = reader.GetValue(0);
                                        } 
                                        catch (Exception e) {
                                            exception = e;
                                        }
                                    }
                                }
                                finally {
                                    reader.Dispose();
                                }
                                if (exception != null) {
                                    source.SetException(exception);
                                }
                                else {
                                    source.SetResult(result);
                                }
                            }
                        }
                        catch (Exception e) {
                            // exception thrown by Dispose...
                            source.SetException(e);
                        }
                    }, TaskScheduler.Default);
                }
                return source.Task;
            }, TaskScheduler.Default).Unwrap();
        }

        public Task<XmlReader> ExecuteXmlReaderAsync() {
            return ExecuteXmlReaderAsync(CancellationToken.None);
        }

        public Task<XmlReader> ExecuteXmlReaderAsync(CancellationToken cancellationToken) {

            Bid.CorrelationTrace("<sc.SqlCommand.ExecuteXmlReaderAsync|API|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            SqlConnection.ExecutePermission.Demand();   
            
            TaskCompletionSource<XmlReader> source = new TaskCompletionSource<XmlReader>();

            CancellationTokenRegistration registration = new CancellationTokenRegistration();
            if (cancellationToken.CanBeCanceled) {
                if (cancellationToken.IsCancellationRequested) {
                    source.SetCanceled();
                    return source.Task;
                }
                registration = cancellationToken.Register(CancelIgnoreFailure);
            }
            
            Task<XmlReader> returnedTask = source.Task;
            try {
                RegisterForConnectionCloseNotification(ref returnedTask);

                Task<XmlReader>.Factory.FromAsync(BeginExecuteXmlReaderAsync, EndExecuteXmlReaderAsync, null).ContinueWith((t) => {
                    registration.Dispose();
                    if (t.IsFaulted) {
                        Exception e = t.Exception.InnerException;
                        source.SetException(e);
                    }
                    else {
                        if (t.IsCanceled) {
                            source.SetCanceled();
                        }
                        else {
                            source.SetResult(t.Result);
                        }
                    }
                }, TaskScheduler.Default);
            } 
            catch (Exception e) {
                source.SetException(e);
            }

            return returnedTask;
        }

        // If the user part is quoted, remove first and last brackets and then unquote any right square
        // brackets in the procedure.  This is a very simple parser that performs no validation.  As
        // with the function below, ideally we should have support from the server for this.
        private static string UnquoteProcedurePart(string part) {
            if ((null != part) && (2 <= part.Length)) {
                if ('[' == part[0] && ']' == part[part.Length-1]) {
                    part = part.Substring(1, part.Length-2); // strip outer '[' & ']'
                    part = part.Replace("]]", "]"); // undo quoted "]" from "]]" to "]"
                }
            }
            return part;
        }

        // User value in this format: [server].[database].[schema].[sp_foo];1
        // This function should only be passed "[sp_foo];1".
        // This function uses a pretty simple parser that doesn't do any validation.
        // Ideally, we would have support from the server rather than us having to do this.
        private static string UnquoteProcedureName(string name, out object groupNumber) {
            groupNumber  = null; // Out param - initialize value to no value.
            string sproc = name;

            if (null != sproc) {
                if (Char.IsDigit(sproc[sproc.Length-1])) { // If last char is a digit, parse.
                    int semicolon = sproc.LastIndexOf(';');
                    if (semicolon != -1) { // If we found a semicolon, obtain the integer.
                        string part   = sproc.Substring(semicolon+1);
                        int    number = 0;
                        if (Int32.TryParse(part, out number)) { // No checking, just fail if this doesn't work.
                            groupNumber = number;
                            sproc = sproc.Substring(0, semicolon);
                        }
                    }
                }
                sproc = UnquoteProcedurePart(sproc);
            }
            return sproc;
        }

        //index into indirection arrays for columns of interest to DeriveParameters
        private enum ProcParamsColIndex {
            ParameterName = 0,
            ParameterType,
            DataType,                  // obsolete in katmai, use ManagedDataType instead
            ManagedDataType,          // new in katmai
            CharacterMaximumLength,
            NumericPrecision,
            NumericScale,
            TypeCatalogName,
            TypeSchemaName,
            TypeName,
            XmlSchemaCollectionCatalogName,
            XmlSchemaCollectionSchemaName,
            XmlSchemaCollectionName,
            UdtTypeName,                // obsolete in Katmai.  Holds the actual typename if UDT, since TypeName didn't back then.
            DateTimeScale               // new in Katmai
        };

        // Yukon- column ordinals (this array indexed by ProcParamsColIndex
        static readonly internal string[] PreKatmaiProcParamsNames = new string[] {
            "PARAMETER_NAME",           // ParameterName,
            "PARAMETER_TYPE",           // ParameterType,
            "DATA_TYPE",                // DataType
            null,                       // ManagedDataType,     introduced in Katmai
            "CHARACTER_MAXIMUM_LENGTH", // CharacterMaximumLength,
            "NUMERIC_PRECISION",        // NumericPrecision,
            "NUMERIC_SCALE",            // NumericScale,
            "UDT_CATALOG",              // TypeCatalogName,
            "UDT_SCHEMA",               // TypeSchemaName,
            "TYPE_NAME",                // TypeName,
            "XML_CATALOGNAME",          // XmlSchemaCollectionCatalogName,
            "XML_SCHEMANAME",           // XmlSchemaCollectionSchemaName,
            "XML_SCHEMACOLLECTIONNAME", // XmlSchemaCollectionName
            "UDT_NAME",                 // UdtTypeName
            null,                       // Scale for datetime types with scale, introduced in Katmai
        };

        // Katmai+ column ordinals (this array indexed by ProcParamsColIndex
        static readonly internal string[] KatmaiProcParamsNames = new string[] {
            "PARAMETER_NAME",           // ParameterName,
            "PARAMETER_TYPE",           // ParameterType,
            null,                       // DataType, removed from Katmai+
            "MANAGED_DATA_TYPE",        // ManagedDataType,
            "CHARACTER_MAXIMUM_LENGTH", // CharacterMaximumLength,
            "NUMERIC_PRECISION",        // NumericPrecision,
            "NUMERIC_SCALE",            // NumericScale,
            "TYPE_CATALOG_NAME",        // TypeCatalogName,
            "TYPE_SCHEMA_NAME",         // TypeSchemaName,
            "TYPE_NAME",                // TypeName,
            "XML_CATALOGNAME",          // XmlSchemaCollectionCatalogName,
            "XML_SCHEMANAME",           // XmlSchemaCollectionSchemaName,
            "XML_SCHEMACOLLECTIONNAME", // XmlSchemaCollectionName
            null,                       // UdtTypeName, removed from Katmai+
            "SS_DATETIME_PRECISION",    // Scale for datetime types with scale
        };


        internal void DeriveParameters() {
            switch (this.CommandType) {
                case System.Data.CommandType.Text:
                    throw ADP.DeriveParametersNotSupported(this);
                case System.Data.CommandType.StoredProcedure:
                    break;
                case System.Data.CommandType.TableDirect:
                    // CommandType.TableDirect - do nothing, parameters are not supported
                    throw ADP.DeriveParametersNotSupported(this);
                default:
                    throw ADP.InvalidCommandType(this.CommandType);
            }

            // validate that we have a valid connection
            ValidateCommand(ADP.DeriveParameters, false /*not async*/);

            // Use common parser for SqlClient and OleDb - parse into 4 parts - Server, Catalog, Schema, ProcedureName
            string[] parsedSProc = MultipartIdentifier.ParseMultipartIdentifier(this.CommandText, "[\"", "]\"", Res.SQL_SqlCommandCommandText, false);
            if (null == parsedSProc[3] || ADP.IsEmpty(parsedSProc[3]))
            {
                throw ADP.NoStoredProcedureExists(this.CommandText);
            }

            Debug.Assert(parsedSProc.Length == 4, "Invalid array length result from SqlCommandBuilder.ParseProcedureName");

            SqlCommand    paramsCmd = null;
            StringBuilder cmdText   = new StringBuilder();

            // Build call for sp_procedure_params_rowset built of unquoted values from user:
            // [user server, if provided].[user catalog, else current database].[sys if Yukon, else blank].[sp_procedure_params_rowset]

            // Server - pass only if user provided.
            if (!ADP.IsEmpty(parsedSProc[0])) {
                SqlCommandSet.BuildStoredProcedureName(cmdText, parsedSProc[0]);
                cmdText.Append(".");
            }

            // Catalog - pass user provided, otherwise use current database.
            if (ADP.IsEmpty(parsedSProc[1])) {
                parsedSProc[1] = this.Connection.Database;
            }
            SqlCommandSet.BuildStoredProcedureName(cmdText, parsedSProc[1]);
            cmdText.Append(".");

            // Schema - only if Yukon, and then only pass sys.  Also - pass managed version of sproc
            // for Yukon, else older sproc.
            string[] colNames;
            bool useManagedDataType;
            if (this.Connection.IsKatmaiOrNewer) {
                // Procedure - [sp_procedure_params_managed]
                cmdText.Append("[sys].[").Append(TdsEnums.SP_PARAMS_MGD10).Append("]");

                colNames = KatmaiProcParamsNames;
                useManagedDataType = true;
            }
            else {
                if (this.Connection.IsYukonOrNewer) {
                    // Procedure - [sp_procedure_params_managed]
                    cmdText.Append("[sys].[").Append(TdsEnums.SP_PARAMS_MANAGED).Append("]");
                }
                else {
                    // Procedure - [sp_procedure_params_rowset]
                    cmdText.Append(".[").Append(TdsEnums.SP_PARAMS).Append("]");
                }

                colNames = PreKatmaiProcParamsNames;
                useManagedDataType = false;
            }


            paramsCmd = new SqlCommand(cmdText.ToString(), this.Connection, this.Transaction);
            paramsCmd.CommandType = CommandType.StoredProcedure;

            object groupNumber;

            // Prepare parameters for sp_procedure_params_rowset:
            // 1) procedure name - unquote user value
            // 2) group number - parsed at the time we unquoted procedure name
            // 3) procedure schema - unquote user value

            // 



            paramsCmd.Parameters.Add(new SqlParameter("@procedure_name", SqlDbType.NVarChar, 255));
            paramsCmd.Parameters[0].Value = UnquoteProcedureName(parsedSProc[3], out groupNumber); // ProcedureName is 4rd element in parsed array

            if (null != groupNumber) {
                SqlParameter param = paramsCmd.Parameters.Add(new SqlParameter("@group_number", SqlDbType.Int));
                param.Value = groupNumber;
            }

            if (!ADP.IsEmpty(parsedSProc[2])) { // SchemaName is 3rd element in parsed array
                SqlParameter param = paramsCmd.Parameters.Add(new SqlParameter("@procedure_schema", SqlDbType.NVarChar, 255));
                param.Value = UnquoteProcedurePart(parsedSProc[2]);
            }

            SqlDataReader r = null;

            List<SqlParameter> parameters = new List<SqlParameter>();
            bool processFinallyBlock = true;
            
            try {
                r = paramsCmd.ExecuteReader();

                SqlParameter p = null;

                while (r.Read()) {
                    // each row corresponds to a parameter of the stored proc.  Fill in all the info
            
                    p = new SqlParameter();

                    // name
                    p.ParameterName = (string) r[colNames[(int)ProcParamsColIndex.ParameterName]];

                    // type
                    if (useManagedDataType) {
                        p.SqlDbType = (SqlDbType)(short)r[colNames[(int)ProcParamsColIndex.ManagedDataType]];

                        // Yukon didn't have as accurate of information as we're getting for Katmai, so re-map a couple of
                        //  types for backward compatability.
                        switch (p.SqlDbType) {
                            case SqlDbType.Image:
                            case SqlDbType.Timestamp:
                                p.SqlDbType = SqlDbType.VarBinary;
                                break;

                            case SqlDbType.NText:
                                p.SqlDbType = SqlDbType.NVarChar;
                                break;

                            case SqlDbType.Text:
                                p.SqlDbType = SqlDbType.VarChar;
                                break;

                            default:
                                break;
                        }
                    }
                    else {
                        p.SqlDbType = MetaType.GetSqlDbTypeFromOleDbType((short)r[colNames[(int)ProcParamsColIndex.DataType]], 
                            ADP.IsNull(r[colNames[(int)ProcParamsColIndex.TypeName]]) ? 
                                ADP.StrEmpty : 
                                (string)r[colNames[(int)ProcParamsColIndex.TypeName]]);
                    }

                    // size
                    object a = r[colNames[(int)ProcParamsColIndex.CharacterMaximumLength]];
                    if (a is int) {
                        int size = (int)a;

                        // Map MAX sizes correctly.  The Katmai server-side proc sends 0 for these instead of -1.
                        //  Should be fixed on the Katmai side, but would likely hold up the RI, and is safer to fix here.
                        //  If we can get the server-side fixed before shipping Katmai, we can remove this mapping.
                        if (0 == size && 
                                (p.SqlDbType == SqlDbType.NVarChar ||
                                 p.SqlDbType == SqlDbType.VarBinary ||
                                 p.SqlDbType == SqlDbType.VarChar)) {
                            size = -1;
                        }
                        p.Size = size;
                    }

                    // direction
                    p.Direction = ParameterDirectionFromOleDbDirection((short)r[colNames[(int)ProcParamsColIndex.ParameterType]]);

                    if (p.SqlDbType == SqlDbType.Decimal) {
                        p.ScaleInternal = (byte) ((short)r[colNames[(int)ProcParamsColIndex.NumericScale]] & 0xff);
                        p.PrecisionInternal = (byte)((short)r[colNames[(int)ProcParamsColIndex.NumericPrecision]] & 0xff);
                    }

                    // type name for Udt 
                    if (SqlDbType.Udt == p.SqlDbType) {

                        Debug.Assert(this._activeConnection.IsYukonOrNewer,"Invalid datatype token received from pre-yukon server");

                        string udtTypeName;
                        if (useManagedDataType) {
                            udtTypeName = (string)r[colNames[(int)ProcParamsColIndex.TypeName]];
                        }
                        else {
                            udtTypeName = (string)r[colNames[(int)ProcParamsColIndex.UdtTypeName]];
                        }

                        //read the type name
                        p.UdtTypeName = r[colNames[(int)ProcParamsColIndex.TypeCatalogName]]+"."+
                            r[colNames[(int)ProcParamsColIndex.TypeSchemaName]]+"."+
                            udtTypeName;
                    }

                    // type name for Structured types (same as for Udt's except assign p.TypeName instead of p.UdtTypeName
                    if (SqlDbType.Structured == p.SqlDbType) {

                        Debug.Assert(this._activeConnection.IsKatmaiOrNewer,"Invalid datatype token received from pre-katmai server");

                        //read the type name
                        p.TypeName = r[colNames[(int)ProcParamsColIndex.TypeCatalogName]]+"."+
                            r[colNames[(int)ProcParamsColIndex.TypeSchemaName]]+"."+
                            r[colNames[(int)ProcParamsColIndex.TypeName]];
                    }

                    // XmlSchema name for Xml types
                    if (SqlDbType.Xml == p.SqlDbType) {
                        object value;

                        value = r[colNames[(int)ProcParamsColIndex.XmlSchemaCollectionCatalogName]];
                        p.XmlSchemaCollectionDatabase = ADP.IsNull(value) ? String.Empty : (string) value;

                        value = r[colNames[(int)ProcParamsColIndex.XmlSchemaCollectionSchemaName]];
                        p.XmlSchemaCollectionOwningSchema = ADP.IsNull(value) ? String.Empty : (string) value;

                        value = r[colNames[(int)ProcParamsColIndex.XmlSchemaCollectionName]];
                        p.XmlSchemaCollectionName = ADP.IsNull(value) ? String.Empty : (string) value;
                    }

                    if (MetaType._IsVarTime(p.SqlDbType)) {
                        object value = r[colNames[(int)ProcParamsColIndex.DateTimeScale]];
                        if (value is int) {
                            p.ScaleInternal = (byte)(((int)value) & 0xff);
                        }
                    }

                    parameters.Add(p);
                }
            }
            catch (Exception e) {
                processFinallyBlock = ADP.IsCatchableExceptionType(e);
                throw;
            }
            finally {
                TdsParser.ReliabilitySection.Assert("unreliable call to DeriveParameters");  // you need to setup for a thread abort somewhere before you call this method
                if (processFinallyBlock) {
                    if (null != r)
                        r.Close();

                    // always unhook the user's connection
                    paramsCmd.Connection = null;
                }
            }

            if (parameters.Count == 0) {
                throw ADP.NoStoredProcedureExists(this.CommandText);
            }

            this.Parameters.Clear();

            foreach (SqlParameter temp in parameters) {
                this._parameters.Add(temp);
            }
        }

        private ParameterDirection ParameterDirectionFromOleDbDirection(short oledbDirection) {
            Debug.Assert(oledbDirection >= 1 && oledbDirection <= 4, "invalid parameter direction from params_rowset!");

            switch (oledbDirection) {
                case 2:
                    return ParameterDirection.InputOutput;
                case 3:
                    return ParameterDirection.Output;
                case 4:
                    return ParameterDirection.ReturnValue;
                default:
                    return ParameterDirection.Input;
            }

        }

        // get cached metadata
        internal _SqlMetaDataSet MetaData {
            get {
                return _cachedMetaData;
            }
        }

        // Check to see if notificactions auto enlistment is turned on. Enlist if so.
        private void CheckNotificationStateAndAutoEnlist() {
            // First, if auto-enlist is on, check server version and then obtain context if
            // present.  If so, auto enlist to the dependency ID given in the context data.
            if (NotificationAutoEnlist) {
                if (_activeConnection.IsYukonOrNewer) { // Only supported for Yukon...
                    string notifyContext = SqlNotificationContext();
                    if (!ADP.IsEmpty(notifyContext)) {
                        // Map to dependency by ID set in context data.
                        SqlDependency dependency = SqlDependencyPerAppDomainDispatcher.SingletonInstance.LookupDependencyEntry(notifyContext);

                        if (null != dependency) {
                            // Add this command to the dependency.
                            dependency.AddCommandDependency(this);
                        }
                    }
                }
            }

            // If we have a notification with a dependency, setup the notification options at this time.

            // If user passes options, then we will always have option data at the time the SqlDependency
            // ctor is called.  But, if we are using default queue, then we do not have this data until
            // Start().  Due to this, we always delay setting options until execute.

            // There is a variance in order between Start(), SqlDependency(), and Execute.  This is the 
            // best way to solve that problem.
            if (null != Notification) {
                if (_sqlDep != null) {
                    if (null == _sqlDep.Options) { 
                        // If null, SqlDependency was not created with options, so we need to obtain default options now.
                        // GetDefaultOptions can and will throw under certain conditions.

                        // In order to match to the appropriate start - we need 3 pieces of info:
                        // 1) server 2) user identity (SQL Auth or Int Sec) 3) database

                        SqlDependency.IdentityUserNamePair identityUserName = null;

                        // Obtain identity from connection.
                        SqlInternalConnectionTds internalConnection = _activeConnection.InnerConnection as SqlInternalConnectionTds;
                        if (internalConnection.Identity != null) {
                            identityUserName = new SqlDependency.IdentityUserNamePair(internalConnection.Identity, null);
                        }
                        else {
                            identityUserName = new SqlDependency.IdentityUserNamePair(null, internalConnection.ConnectionOptions.UserID);
                        }

                        Notification.Options = SqlDependency.GetDefaultComposedOptions(_activeConnection.DataSource,
                                                             InternalTdsConnection.ServerProvidedFailOverPartner,
                                                             identityUserName, _activeConnection.Database);
                    }

                    // Set UserData on notifications, as well as adding to the appdomain dispatcher.  The value is
                    // computed by an algorithm on the dependency - fixed and will always produce the same value
                    // given identical commandtext + parameter values.
                    Notification.UserData = _sqlDep.ComputeHashAndAddToDispatcher(this);
                    // Maintain server list for SqlDependency.
                    _sqlDep.AddToServerList(_activeConnection.DataSource);
                }
            }   
        }

        [System.Security.Permissions.SecurityPermission(SecurityAction.Assert, Infrastructure=true)]
        static internal string SqlNotificationContext() {
            SqlConnection.VerifyExecutePermission();

            // since this information is protected, follow it so that it is not exposed to the user.
            // SQLBU 329633, SQLBU 329637
            return (System.Runtime.Remoting.Messaging.CallContext.GetData("MS.SqlDependencyCookie") as string);
        }

        // Tds-specific logic for ExecuteNonQuery run handling
        private Task RunExecuteNonQueryTds(string methodName, bool async, int timeout, bool asyncWrite ) {
            Debug.Assert(!asyncWrite || async, "AsyncWrite should be always accompanied by Async");
            bool processFinallyBlock = true;
            try {

                Task reconnectTask = _activeConnection.ValidateAndReconnect(null, timeout);

                if (reconnectTask != null) {
                    long reconnectionStart = ADP.TimerCurrent();
                    if (async) {
                        TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
                        _activeConnection.RegisterWaitingForReconnect(completion.Task);
                        _reconnectionCompletionSource = completion;
                        CancellationTokenSource timeoutCTS = new CancellationTokenSource();
                        AsyncHelper.SetTimeoutException(completion, timeout, SQL.CR_ReconnectTimeout, timeoutCTS.Token);                        
                        AsyncHelper.ContinueTask(reconnectTask, completion,
                            () => {                               
                                if (completion.Task.IsCompleted) {
                                    return;
                                }                                
                                Interlocked.CompareExchange(ref _reconnectionCompletionSource, null, completion);
                                timeoutCTS.Cancel();
                                Task subTask = RunExecuteNonQueryTds(methodName, async, TdsParserStaticMethods.GetRemainingTimeout(timeout, reconnectionStart), asyncWrite);
                                if (subTask == null) {
                                    completion.SetResult(null);
                                }
                                else {
                                    AsyncHelper.ContinueTask(subTask, completion, () => completion.SetResult(null));
                                }
                            }, connectionToAbort: _activeConnection);
                        return completion.Task;
                    }
                    else {
                        AsyncHelper.WaitForCompletion(reconnectTask, timeout, () => { throw SQL.CR_ReconnectTimeout(); });
                        timeout = TdsParserStaticMethods.GetRemainingTimeout(timeout, reconnectionStart);
                    }
                }

                if (asyncWrite) {
                    _activeConnection.AddWeakReference(this, SqlReferenceCollection.CommandTag);
                }

                GetStateObject();

                // we just send over the raw text with no annotation
                // no parameters are sent over
                // no data reader is returned
                // use this overload for "batch SQL" tds token type
                Bid.Trace("<sc.SqlCommand.ExecuteNonQuery|INFO> %d#, Command executed as SQLBATCH.\n", ObjectID);
                Task executeTask = _stateObj.Parser.TdsExecuteSQLBatch(this.CommandText, timeout, this.Notification, _stateObj, sync: true);
                Debug.Assert(executeTask == null, "Shouldn't get a task when doing sync writes");

                NotifyDependency();
                if (async) {
                    _activeConnection.GetOpenTdsConnection(methodName).IncrementAsyncCount();
                }
                else {
                    bool dataReady;
                    Debug.Assert(_stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
                    bool result = _stateObj.Parser.TryRun(RunBehavior.UntilDone, this, null, null, _stateObj, out dataReady);
                    if (!result) { throw SQL.SynchronousCallMayNotPend(); }
                }
            }
            catch (Exception e) {
                processFinallyBlock = ADP.IsCatchableExceptionType(e);
                throw;
            }
            finally {
                TdsParser.ReliabilitySection.Assert("unreliable call to RunExecuteNonQueryTds");  // you need to setup for a thread abort somewhere before you call this method
                if (processFinallyBlock && !async) {
                    // When executing Async, we need to keep the _stateObj alive...
                    PutStateObject();
                }
            }
            return null;
        }

        // Smi-specific logic for ExecuteNonQuery
        private void RunExecuteNonQuerySmi( bool sendToPipe ) {
            SqlInternalConnectionSmi innerConnection = InternalSmiConnection;

            // Set it up, process all of the events, and we're done!
            SmiRequestExecutor requestExecutor = null;
            try {
                requestExecutor = SetUpSmiRequest(innerConnection);
                SmiExecuteType execType;
                if ( sendToPipe )
                    execType = SmiExecuteType.ToPipe;
                else
                    execType = SmiExecuteType.NonQuery;


                SmiEventStream eventStream = null;
                // Don't need a CER here because caller already has one that will doom the
                //  connection if it's a finally-skipping type of problem.
                bool processFinallyBlock = true;
                try {
                    long transactionId;
                    SysTx.Transaction transaction;
                    innerConnection.GetCurrentTransactionPair(out transactionId, out transaction);

                    if (Bid.AdvancedOn) {
                        Bid.Trace("<sc.SqlCommand.RunExecuteNonQuerySmi|ADV> %d#, innerConnection=%d#, transactionId=0x%I64x, cmdBehavior=%d.\n", ObjectID, innerConnection.ObjectID, transactionId, (int)CommandBehavior.Default);
                    }

                    if (SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion) {
                        eventStream = requestExecutor.Execute(
                                                          innerConnection.SmiConnection,
                                                          transactionId,
                                                          transaction,
                                                          CommandBehavior.Default,
                                                          execType);
                    }
                    else {
                        eventStream = requestExecutor.Execute(
                                                          innerConnection.SmiConnection,
                                                          transactionId,
                                                          CommandBehavior.Default,
                                                          execType);
                    }

                    while ( eventStream.HasEvents ) {
                        eventStream.ProcessEvent( EventSink );
                    }
                }
                catch (Exception e) {
                    processFinallyBlock = ADP.IsCatchableExceptionType(e);
                    throw;
                }
                finally {
                    TdsParser.ReliabilitySection.Assert("unreliable call to RunExecuteNonQuerySmi");  // you need to setup for a thread abort somewhere before you call this method
                    if (null != eventStream && processFinallyBlock) {
                        eventStream.Close( EventSink );
                    }
                }

                EventSink.ProcessMessagesAndThrow();
            }
            finally {
                if (requestExecutor != null) {
                    requestExecutor.Close(EventSink);
                    EventSink.ProcessMessagesAndThrow(ignoreNonFatalMessages: true);
                }
            }
        }

        /// <summary>
        /// Resets the encryption related state of the command object and each of the parameters.
        /// BatchRPC doesn't need special handling to cleanup the state of each RPC object and its parameters since a new RPC object and 
        /// parameters are generated on every execution.
        /// </summary>
        private void ResetEncryptionState() {
            // First reset the command level state.
            ClearDescribeParameterEncryptionRequests();

            // Reset the state of each of the parameters.
            if (_parameters != null) {
                for (int i = 0; i < _parameters.Count; i++) {
                    _parameters[i].CipherMetadata = null;
                    _parameters[i].HasReceivedMetadata = false;
                }
            }
        }

        /// <summary>
        /// Steps to be executed in the Prepare Transparent Encryption finally block.
        /// </summary>
        private void PrepareTransparentEncryptionFinallyBlock(  bool closeDataReader,
                                                                bool clearDataStructures,
                                                                bool decrementAsyncCount,
                                                                bool wasDescribeParameterEncryptionNeeded,
                                                                ReadOnlyDictionary<_SqlRPC, _SqlRPC> describeParameterEncryptionRpcOriginalRpcMap,
                                                                SqlDataReader describeParameterEncryptionDataReader) {
            if (clearDataStructures) {
                // Clear some state variables in SqlCommand that reflect in-progress describe parameter encryption requests.
                ClearDescribeParameterEncryptionRequests();

                if (describeParameterEncryptionRpcOriginalRpcMap != null) {
                    describeParameterEncryptionRpcOriginalRpcMap = null;
                }
            }

            // Decrement the async count.
            if (decrementAsyncCount) {
                SqlInternalConnectionTds internalConnectionTds = _activeConnection.GetOpenTdsConnection();
                if (internalConnectionTds != null) {
                    internalConnectionTds.DecrementAsyncCount();
                }
            }

            if (closeDataReader) {
                // Close the data reader to reset the _stateObj
                if (null != describeParameterEncryptionDataReader) {
                    describeParameterEncryptionDataReader.Close();
                }
            }
        }

        /// <summary>
        /// Executes the reader after checking to see if we need to encrypt input parameters and then encrypting it if required.
        /// TryFetchInputParameterEncryptionInfo() -> ReadDescribeEncryptionParameterResults()-> EncryptInputParameters() ->RunExecuteReaderTds()
        /// </summary>
        /// <param name="cmdBehavior"></param>
        /// <param name="returnStream"></param>
        /// <param name="async"></param>
        /// <param name="timeout"></param>
        /// <param name="task"></param>
        /// <param name="asyncWrite"></param>
        /// <returns></returns>
        private void PrepareForTransparentEncryption(CommandBehavior cmdBehavior, bool returnStream, bool async, int timeout, TaskCompletionSource<object> completion, out Task returnTask, bool asyncWrite)
        {
            // Fetch reader with input params
            Task fetchInputParameterEncryptionInfoTask = null;
            bool describeParameterEncryptionNeeded = false;
            SqlDataReader describeParameterEncryptionDataReader = null;
            returnTask = null;

            Debug.Assert(_activeConnection != null, "_activeConnection should not be null in PrepareForTransparentEncryption.");
            Debug.Assert(_activeConnection.Parser != null, "_activeConnection.Parser should not be null in PrepareForTransparentEncryption.");
            Debug.Assert(_activeConnection.Parser.IsColumnEncryptionSupported,
                "_activeConnection.Parser.IsColumnEncryptionSupported should be true in PrepareForTransparentEncryption.");
            Debug.Assert(_columnEncryptionSetting == SqlCommandColumnEncryptionSetting.Enabled
                        || (_columnEncryptionSetting == SqlCommandColumnEncryptionSetting.UseConnectionSetting && _activeConnection.IsColumnEncryptionSettingEnabled),
                        "ColumnEncryption setting should be enabled for input parameter encryption.");
            Debug.Assert(async == (completion != null), "completion should can be null if and only if mode is async.");

            // A flag to indicate if finallyblock needs to execute.
            bool processFinallyBlock = true;

            // A flag to indicate if we need to decrement async count on the connection in finally block.
            bool decrementAsyncCountInFinallyBlock = async;

            // Flag to indicate if exception is caught during the execution, to govern clean up.
            bool exceptionCaught = false;

            // Used in BatchRPCMode to maintain a map of describe parameter encryption RPC requests (Keys) and their corresponding original RPC requests (Values).
            ReadOnlyDictionary<_SqlRPC, _SqlRPC> describeParameterEncryptionRpcOriginalRpcMap = null;

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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    try {
                        // Fetch the encryption information that applies to any of the input parameters.
                        describeParameterEncryptionDataReader = TryFetchInputParameterEncryptionInfo(timeout,
                                                                                                     async,
                                                                                                     asyncWrite,
                                                                                                     out describeParameterEncryptionNeeded,
                                                                                                     out fetchInputParameterEncryptionInfoTask,
                                                                                                     out describeParameterEncryptionRpcOriginalRpcMap);

                        Debug.Assert(describeParameterEncryptionNeeded || describeParameterEncryptionDataReader == null,
                            "describeParameterEncryptionDataReader should be null if we don't need to request describe parameter encryption request.");

                        Debug.Assert(fetchInputParameterEncryptionInfoTask == null || async,
                            "Task returned by TryFetchInputParameterEncryptionInfo, when in sync mode, in PrepareForTransparentEncryption.");

                        Debug.Assert((describeParameterEncryptionRpcOriginalRpcMap != null) == BatchRPCMode,
                            "describeParameterEncryptionRpcOriginalRpcMap can be non-null if and only if it is in BatchRPCMode.");

                        // If we didn't have parameters, we can fall back to regular code path, by simply returning.
                        if (!describeParameterEncryptionNeeded) {
                            Debug.Assert(null == fetchInputParameterEncryptionInfoTask,
                                "fetchInputParameterEncryptionInfoTask should not be set if describe parameter encryption is not needed.");

                            Debug.Assert(null == describeParameterEncryptionDataReader,
                                "SqlDataReader created for describe parameter encryption params when it is not needed.");

                            return;
                        }

                        Debug.Assert(describeParameterEncryptionDataReader != null,
                            "describeParameterEncryptionDataReader should not be null, as it is required to get results of describe parameter encryption.");

                        // Fire up another task to read the results of describe parameter encryption
                        if (fetchInputParameterEncryptionInfoTask != null) {
                            // Mark that we should not process the finally block since we have async execution pending.
                            // Note that this should be done outside the task's continuation delegate.
                            processFinallyBlock = false;
                            returnTask = AsyncHelper.CreateContinuationTask(fetchInputParameterEncryptionInfoTask, () => {
                                bool processFinallyBlockAsync = true;

                                RuntimeHelpers.PrepareConstrainedRegions();
                                try {
#if DEBUG
                                    TdsParser.ReliabilitySection tdsReliabilitySectionAsync = new TdsParser.ReliabilitySection();
                                    RuntimeHelpers.PrepareConstrainedRegions();
                                    try {
                                        tdsReliabilitySectionAsync.Start();
#endif //DEBUG
                                        // Check for any exceptions on network write, before reading.
                                        CheckThrowSNIException();

                                        // If it is async, then TryFetchInputParameterEncryptionInfo-> RunExecuteReaderTds would have incremented the async count.
                                        // Decrement it when we are about to complete async execute reader.
                                        SqlInternalConnectionTds internalConnectionTds = _activeConnection.GetOpenTdsConnection();
                                        if (internalConnectionTds != null)
                                        {
                                            internalConnectionTds.DecrementAsyncCount();
                                            decrementAsyncCountInFinallyBlock = false;
                                        }

                                        // Complete executereader.
                                        describeParameterEncryptionDataReader = CompleteAsyncExecuteReader();
                                        Debug.Assert(null == _stateObj, "non-null state object in PrepareForTransparentEncryption.");

                                        // Read the results of describe parameter encryption.
                                        ReadDescribeEncryptionParameterResults(describeParameterEncryptionDataReader, describeParameterEncryptionRpcOriginalRpcMap);

#if DEBUG
                                        // Failpoint to force the thread to halt to simulate cancellation of SqlCommand.
                                        if (_sleepAfterReadDescribeEncryptionParameterResults) {
                                            Thread.Sleep(10000);
                                        }
                                    }
                                    finally {
                                        tdsReliabilitySectionAsync.Stop();
                                    }
#endif //DEBUG
                                }
                                catch (Exception e) {
                                    processFinallyBlockAsync = ADP.IsCatchableExceptionType(e);
                                    throw;
                                }
                                finally {
                                    PrepareTransparentEncryptionFinallyBlock(   closeDataReader: processFinallyBlockAsync,
                                                                                decrementAsyncCount: decrementAsyncCountInFinallyBlock,
                                                                                clearDataStructures: processFinallyBlockAsync,
                                                                                wasDescribeParameterEncryptionNeeded: describeParameterEncryptionNeeded,
                                                                                describeParameterEncryptionRpcOriginalRpcMap: describeParameterEncryptionRpcOriginalRpcMap,
                                                                                describeParameterEncryptionDataReader: describeParameterEncryptionDataReader);
                                }
                            }, 
                            onFailure: ((exception) => {
                            if (_cachedAsyncState != null) {
                                _cachedAsyncState.ResetAsyncState();
                            }
                            if (exception != null) {
                                throw exception;
                            }}));
                        }
                        else {
                            // If it was async, ending the reader is still pending.
                            if (async) {
                                // Mark that we should not process the finally block since we have async execution pending.
                                // Note that this should be done outside the task's continuation delegate.
                                processFinallyBlock = false;
                                returnTask = Task.Run(() => {
                                        bool processFinallyBlockAsync = true;

                                        RuntimeHelpers.PrepareConstrainedRegions();
                                        try {
#if DEBUG
                                            TdsParser.ReliabilitySection tdsReliabilitySectionAsync = new TdsParser.ReliabilitySection();
                                            RuntimeHelpers.PrepareConstrainedRegions();
                                            try {
                                                tdsReliabilitySectionAsync.Start();
#endif //DEBUG

                                                // Check for any exceptions on network write, before reading.
                                                CheckThrowSNIException();

                                                // If it is async, then TryFetchInputParameterEncryptionInfo-> RunExecuteReaderTds would have incremented the async count.
                                                // Decrement it when we are about to complete async execute reader.
                                                SqlInternalConnectionTds internalConnectionTds = _activeConnection.GetOpenTdsConnection();
                                                if (internalConnectionTds != null) {
                                                    internalConnectionTds.DecrementAsyncCount();
                                                    decrementAsyncCountInFinallyBlock = false;
                                                }

                                                // Complete executereader.
                                                describeParameterEncryptionDataReader = CompleteAsyncExecuteReader();
                                                Debug.Assert(null == _stateObj, "non-null state object in PrepareForTransparentEncryption.");

                                                // Read the results of describe parameter encryption.
                                                ReadDescribeEncryptionParameterResults(describeParameterEncryptionDataReader, describeParameterEncryptionRpcOriginalRpcMap);
#if DEBUG
                                                // Failpoint to force the thread to halt to simulate cancellation of SqlCommand.
                                                if (_sleepAfterReadDescribeEncryptionParameterResults) {
                                                    Thread.Sleep(10000);
                                                }
#endif
#if DEBUG
                                            }
                                            finally {
                                                tdsReliabilitySectionAsync.Stop();
                                            }
#endif //DEBUG
                                        }
                                        catch (Exception e) {
                                            processFinallyBlockAsync = ADP.IsCatchableExceptionType(e);
                                            throw;
                                        }
                                        finally {
                                            PrepareTransparentEncryptionFinallyBlock(   closeDataReader: processFinallyBlockAsync,
                                                                                        decrementAsyncCount: decrementAsyncCountInFinallyBlock,
                                                                                        clearDataStructures: processFinallyBlockAsync,
                                                                                        wasDescribeParameterEncryptionNeeded: describeParameterEncryptionNeeded,
                                                                                        describeParameterEncryptionRpcOriginalRpcMap: describeParameterEncryptionRpcOriginalRpcMap,
                                                                                        describeParameterEncryptionDataReader: describeParameterEncryptionDataReader);
                                        }
                                    });
                            }
                            else {
                                // For synchronous execution, read the results of describe parameter encryption here.
                                ReadDescribeEncryptionParameterResults(describeParameterEncryptionDataReader, describeParameterEncryptionRpcOriginalRpcMap);
                            }

#if DEBUG
                            // Failpoint to force the thread to halt to simulate cancellation of SqlCommand.
                            if (_sleepAfterReadDescribeEncryptionParameterResults) {
                                Thread.Sleep(10000);
                            }
#endif
                        }
                    }
                    catch (Exception e) {
                        processFinallyBlock = ADP.IsCatchableExceptionType(e);
                        exceptionCaught = true;
                        throw;
                    }
                    finally {
                        // Free up the state only for synchronous execution. For asynchronous execution, free only if there was an exception.
                        PrepareTransparentEncryptionFinallyBlock(closeDataReader: (processFinallyBlock &&  !async) || exceptionCaught,
                                               decrementAsyncCount: decrementAsyncCountInFinallyBlock && exceptionCaught,
                                               clearDataStructures: (processFinallyBlock && !async) || exceptionCaught,
                                               wasDescribeParameterEncryptionNeeded: describeParameterEncryptionNeeded,
                                               describeParameterEncryptionRpcOriginalRpcMap: describeParameterEncryptionRpcOriginalRpcMap,
                                               describeParameterEncryptionDataReader: describeParameterEncryptionDataReader);
                    }
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e) {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
            catch (Exception e) {
                if (cachedAsyncState != null) {
                    cachedAsyncState.ResetAsyncState();
                }

                if (ADP.IsCatchableExceptionType(e)) {
                    ReliablePutStateObject();
                }

                throw;
            }
        }

        /// <summary>
        /// Executes an RPC to fetch param encryption info from SQL Engine. If this method is not done writing
        ///  the request to wire, it'll set the "task" parameter which can be used to create continuations.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="async"></param>
        /// <param name="asyncWrite"></param>
        /// <param name="inputParameterEncryptionNeeded"></param>
        /// <param name="task"></param>
        /// <param name="describeParameterEncryptionRpcOriginalRpcMap"></param>
        /// <returns></returns>
        private SqlDataReader TryFetchInputParameterEncryptionInfo(int timeout,
                                                                   bool async,
                                                                   bool asyncWrite,
                                                                   out bool inputParameterEncryptionNeeded,
                                                                   out Task task,
                                                                   out ReadOnlyDictionary<_SqlRPC, _SqlRPC> describeParameterEncryptionRpcOriginalRpcMap) {
            inputParameterEncryptionNeeded = false;
            task = null;
            describeParameterEncryptionRpcOriginalRpcMap = null;

            if (BatchRPCMode) {
                // Count the rpc requests that need to be transparently encrypted
                // We simply look for any parameters in a request and add the request to be queried for parameter encryption
                Dictionary<_SqlRPC, _SqlRPC> describeParameterEncryptionRpcOriginalRpcDictionary = new Dictionary<_SqlRPC, _SqlRPC>();

                for (int i = 0; i < _SqlRPCBatchArray.Length; i++) {
                    // In BatchRPCMode, the actual T-SQL query is in the first parameter and not present as the rpcName, as is the case with non-BatchRPCMode.
                    // So input parameters start at parameters[1]. parameters[0] is the actual T-SQL Statement. rpcName is sp_executesql.
                    if (_SqlRPCBatchArray[i].parameters.Length > 1) {
                        _SqlRPCBatchArray[i].needsFetchParameterEncryptionMetadata = true;

                        // Since we are going to need multiple RPC objects, allocate a new one here for each command in the batch.
                        _SqlRPC rpcDescribeParameterEncryptionRequest = new _SqlRPC();

                        // Prepare the describe parameter encryption request.
                        PrepareDescribeParameterEncryptionRequest(_SqlRPCBatchArray[i], ref rpcDescribeParameterEncryptionRequest);
                        Debug.Assert(rpcDescribeParameterEncryptionRequest != null, "rpcDescribeParameterEncryptionRequest should not be null, after call to PrepareDescribeParameterEncryptionRequest.");

                        Debug.Assert(!describeParameterEncryptionRpcOriginalRpcDictionary.ContainsKey(rpcDescribeParameterEncryptionRequest),
                            "There should not already be a key referring to the current rpcDescribeParameterEncryptionRequest, in the dictionary describeParameterEncryptionRpcOriginalRpcDictionary.");

                        // Add the describe parameter encryption RPC request as the key and its corresponding original rpc request to the dictionary.
                        describeParameterEncryptionRpcOriginalRpcDictionary.Add(rpcDescribeParameterEncryptionRequest, _SqlRPCBatchArray[i]);
                    }
                }

                describeParameterEncryptionRpcOriginalRpcMap = new ReadOnlyDictionary<_SqlRPC, _SqlRPC>(describeParameterEncryptionRpcOriginalRpcDictionary);

                if (describeParameterEncryptionRpcOriginalRpcMap.Count == 0) {
                    // If no parameters are present, nothing to do, simply return.
                    return null;
                }
                else {
                    inputParameterEncryptionNeeded = true;
                }

                _sqlRPCParameterEncryptionReqArray = describeParameterEncryptionRpcOriginalRpcMap.Keys.ToArray();

                Debug.Assert(_sqlRPCParameterEncryptionReqArray.Length > 0, "There should be at-least 1 describe parameter encryption rpc request.");
                Debug.Assert(_sqlRPCParameterEncryptionReqArray.Length <= _SqlRPCBatchArray.Length,
                                "The number of decribe parameter encryption RPC requests is more than the number of original RPC requests.");
            }
            else if (0 != GetParameterCount(_parameters)) {
                // Fetch params for a single batch
                inputParameterEncryptionNeeded = true;
                _sqlRPCParameterEncryptionReqArray = new _SqlRPC[1];

                _SqlRPC rpc = null;
                GetRPCObject(_parameters.Count, ref rpc);
                Debug.Assert(rpc != null, "GetRPCObject should not return rpc as null.");

                rpc.rpcName = CommandText;

                int i = 0;
                foreach (SqlParameter sqlParam in _parameters) {
                    rpc.parameters[i++] = sqlParam;
                }

                // Prepare the RPC request for describe parameter encryption procedure.
                PrepareDescribeParameterEncryptionRequest(rpc, ref _sqlRPCParameterEncryptionReqArray[0]);
                Debug.Assert(_sqlRPCParameterEncryptionReqArray[0] != null, "_sqlRPCParameterEncryptionReqArray[0] should not be null, after call to PrepareDescribeParameterEncryptionRequest.");
            }

            if (inputParameterEncryptionNeeded) {
                // Set the flag that indicates that parameter encryption requests are currently in-progress.
                _isDescribeParameterEncryptionRPCCurrentlyInProgress = true;

#if DEBUG
                // Failpoint to force the thread to halt to simulate cancellation of SqlCommand.
                if (_sleepDuringTryFetchInputParameterEncryptionInfo) {
                    Thread.Sleep(10000);
                }
#endif

                // Execute the RPC.
                return RunExecuteReaderTds( CommandBehavior.Default,
                                            runBehavior: RunBehavior.ReturnImmediately, // Other RunBehavior modes will skip reading rows.
                                            returnStream: true,
                                            async: async,
                                            timeout: timeout,
                                            task: out task,
                                            asyncWrite: asyncWrite,
                                            ds: null,
                                            describeParameterEncryptionRequest: true);
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// Constructs a SqlParameter with a given string value
        /// </summary>
        /// <param name="queryText"></param>
        /// <returns></returns>
        private SqlParameter GetSqlParameterWithQueryText(string queryText)
        {
            SqlParameter sqlParam = new SqlParameter(null, ((queryText.Length << 1) <= TdsEnums.TYPE_SIZE_LIMIT) ? SqlDbType.NVarChar : SqlDbType.NText, queryText.Length);
            sqlParam.Value = queryText;

            return sqlParam;
        }

        /// <summary>
        /// Constructs the sp_describe_parameter_encryption request with the values from the original RPC call.
        /// Prototype for <sp_describe_parameter_encryption> is 
        /// exec sp_describe_parameter_encryption @tsql=N'[SQL Statement]', @params=N'@p1 varbinary(256)'
        /// </summary>
        /// <param name="originalRpcRequest">Original RPC request</param>
        /// <param name="describeParameterEncryptionRequest">sp_describe_parameter_encryption request being built</param>
        private void PrepareDescribeParameterEncryptionRequest(_SqlRPC originalRpcRequest, ref _SqlRPC describeParameterEncryptionRequest) {
            Debug.Assert(originalRpcRequest != null);

            // Construct the RPC request for sp_describe_parameter_encryption
            // sp_describe_parameter_encryption always has 2 parameters (stmt, paramlist).
            GetRPCObject(2, ref describeParameterEncryptionRequest, forSpDescribeParameterEncryption:true);
            describeParameterEncryptionRequest.rpcName = "sp_describe_parameter_encryption";

            // Prepare @tsql parameter
            SqlParameter sqlParam;
            string text;

            // In BatchRPCMode, The actual T-SQL query is in the first parameter and not present as the rpcName, as is the case with non-BatchRPCMode.
            if (BatchRPCMode) {
                Debug.Assert(originalRpcRequest.parameters != null && originalRpcRequest.parameters.Length > 0,
                    "originalRpcRequest didn't have at-least 1 parameter in BatchRPCMode, in PrepareDescribeParameterEncryptionRequest.");
                text = (string)originalRpcRequest.parameters[0].Value;
                sqlParam = GetSqlParameterWithQueryText(text);
            }
            else {
                text = originalRpcRequest.rpcName;
                if (CommandType == Data.CommandType.StoredProcedure) {
                    // For stored procedures, we need to prepare @tsql in the following format
                    // N'EXEC sp_name @param1=@param1, @param1=@param2, ..., @paramN=@paramN'
                    sqlParam = BuildStoredProcedureStatementForColumnEncryption(text, originalRpcRequest.parameters);
                }
                else {
                    sqlParam = GetSqlParameterWithQueryText(text);
                }
            }

            Debug.Assert(text != null, "@tsql parameter is null in PrepareDescribeParameterEncryptionRequest.");

            describeParameterEncryptionRequest.parameters[0] = sqlParam;
            string parameterList = null;

            // In BatchRPCMode, the input parameters start at parameters[1]. parameters[0] is the T-SQL statement. rpcName is sp_executesql.
            // And it is already in the format expected out of BuildParamList, which is not the case with Non-BatchRPCMode.
            if (BatchRPCMode) {
                if (originalRpcRequest.parameters.Length > 1) {
                    parameterList = (string)originalRpcRequest.parameters[1].Value;
                }
            }
            else {
                // Prepare @params parameter
                // Need to create new parameters as we cannot have the same parameter being part of two SqlCommand objects
                SqlParameter paramCopy;
                SqlParameterCollection tempCollection = new SqlParameterCollection();

                for (int i = 0; i < _parameters.Count; i++) {
                    SqlParameter param = originalRpcRequest.parameters[i];
                    paramCopy = new SqlParameter(param.ParameterName, param.SqlDbType, param.Size, param.Direction, param.Precision, param.Scale, param.SourceColumn, param.SourceVersion,
                        param.SourceColumnNullMapping, param.Value, param.XmlSchemaCollectionDatabase, param.XmlSchemaCollectionOwningSchema, param.XmlSchemaCollectionName);
                    tempCollection.Add(paramCopy);
                }

                Debug.Assert(_stateObj == null, "_stateObj should be null at this time, in PrepareDescribeParameterEncryptionRequest.");
                Debug.Assert(_activeConnection != null, "_activeConnection should not be null at this time, in PrepareDescribeParameterEncryptionRequest.");
                TdsParser tdsParser = null;

                if (_activeConnection.Parser != null) {
                    tdsParser = _activeConnection.Parser;
                    if ((tdsParser == null) || (tdsParser.State == TdsParserState.Broken) || (tdsParser.State == TdsParserState.Closed)) {
                        // Connection's parser is null as well, therefore we must be closed
                        throw ADP.ClosedConnectionError();
                    }
                }

                parameterList = BuildParamList(tdsParser, tempCollection, includeReturnValue:true);
            }

            Debug.Assert(!string.IsNullOrWhiteSpace(parameterList), "parameterList should not be null or empty or whitespace.");

            sqlParam = new SqlParameter(null, ((parameterList.Length << 1) <= TdsEnums.TYPE_SIZE_LIMIT) ? SqlDbType.NVarChar : SqlDbType.NText, parameterList.Length);
            sqlParam.Value = parameterList;
            describeParameterEncryptionRequest.parameters[1] = sqlParam;
        }

        /// <summary>
        /// Read the output of sp_describe_parameter_encryption
        /// </summary>
        /// <param name="ds">Resultset from calling to sp_describe_parameter_encryption</param>
        /// <param name="describeParameterEncryptionRpcOriginalRpcMap"> Readonly dictionary with the map of parameter encryption rpc requests with the corresponding original rpc requests.</param>
        private void ReadDescribeEncryptionParameterResults(SqlDataReader ds, ReadOnlyDictionary<_SqlRPC, _SqlRPC> describeParameterEncryptionRpcOriginalRpcMap) {
            _SqlRPC rpc = null;
            int currentOrdinal = -1;
            SqlTceCipherInfoEntry cipherInfoEntry;
            Dictionary<int, SqlTceCipherInfoEntry> columnEncryptionKeyTable = new Dictionary<int, SqlTceCipherInfoEntry>();

            Debug.Assert((describeParameterEncryptionRpcOriginalRpcMap != null) == BatchRPCMode,
                "describeParameterEncryptionRpcOriginalRpcMap should be non-null if and only if it is BatchRPCMode.");

            // Indicates the current result set we are reading, used in BatchRPCMode, where we can have more than 1 result set.
            int resultSetSequenceNumber = 0;

#if DEBUG
            // Keep track of the number of rows in the result sets.
            int rowsAffected = 0;
#endif

            // A flag that used in BatchRPCMode, to assert the result of lookup in to the dictionary maintaining the map of describe parameter encryption requests
            // and the corresponding original rpc requests.
            bool lookupDictionaryResult;

            do {
                if (BatchRPCMode) {
                    // If we got more RPC results from the server than what was requested.
                    if (resultSetSequenceNumber >= _sqlRPCParameterEncryptionReqArray.Length) {
                        Debug.Assert(false, "Server sent back more results than what was expected for describe parameter encryption requests in BatchRPCMode.");
                        // Ignore the rest of the results from the server, if for whatever reason it sends back more than what we expect.
                        break;
                    }
                }

                // First read the column encryption key list
                while (ds.Read()) {

#if DEBUG
                    rowsAffected++;
#endif

                    // Column Encryption Key Ordinal.
                    currentOrdinal = ds.GetInt32((int)DescribeParameterEncryptionResultSet1.KeyOrdinal);
                    Debug.Assert(currentOrdinal >= 0, "currentOrdinal cannot be negative.");

                    // Try to see if there was already an entry for the current ordinal.
                    if (!columnEncryptionKeyTable.TryGetValue(currentOrdinal, out cipherInfoEntry)) {
                        // If an entry for this ordinal was not found, create an entry in the columnEncryptionKeyTable for this ordinal.
                        cipherInfoEntry = new SqlTceCipherInfoEntry(currentOrdinal);
                        columnEncryptionKeyTable.Add(currentOrdinal, cipherInfoEntry);
                    }

                    Debug.Assert(!cipherInfoEntry.Equals(default(SqlTceCipherInfoEntry)), "cipherInfoEntry should not be un-initialized.");

                    // Read the CEK.
                    byte[] encryptedKey = null;
                    int encryptedKeyLength = (int)ds.GetBytes((int)DescribeParameterEncryptionResultSet1.EncryptedKey, 0, encryptedKey, 0, 0);
                    encryptedKey = new byte[encryptedKeyLength];
                    ds.GetBytes((int)DescribeParameterEncryptionResultSet1.EncryptedKey, 0, encryptedKey, 0, encryptedKeyLength);

                    // Read the metadata version of the key.
                    // It should always be 8 bytes.
                    byte[] keyMdVersion = new byte[8];
                    ds.GetBytes((int)DescribeParameterEncryptionResultSet1.KeyMdVersion, 0, keyMdVersion, 0, keyMdVersion.Length);

                    // Validate the provider name
                    string providerName = ds.GetString((int)DescribeParameterEncryptionResultSet1.ProviderName);
                    //SqlColumnEncryptionKeyStoreProvider keyStoreProvider;
                    //if (!SqlConnection.TryGetColumnEncryptionKeyStoreProvider (providerName, out keyStoreProvider)) {
                    //    // unknown provider, skip processing this cek.
                    //    Bid.Trace("<sc.SqlCommand.ReadDescribeEncryptionParameterResults|INFO>Unknown provider name recevied %s, skipping\n", providerName);
                    //    continue;
                    //}

                    cipherInfoEntry.Add(encryptedKey: encryptedKey,
                                        databaseId: ds.GetInt32((int)DescribeParameterEncryptionResultSet1.DbId),
                                        cekId: ds.GetInt32((int)DescribeParameterEncryptionResultSet1.KeyId),
                                        cekVersion: ds.GetInt32((int)DescribeParameterEncryptionResultSet1.KeyVersion),
                                        cekMdVersion: keyMdVersion,
                                        keyPath: ds.GetString((int)DescribeParameterEncryptionResultSet1.KeyPath),
                                        keyStoreName: providerName,
                                        algorithmName: ds.GetString((int)DescribeParameterEncryptionResultSet1.KeyEncryptionAlgorithm));
                }

                if (!ds.NextResult()) {
                    throw SQL.UnexpectedDescribeParamFormat ();
                }

                // Find the RPC command that generated this tce request
                if (BatchRPCMode) {
                    Debug.Assert(_sqlRPCParameterEncryptionReqArray[resultSetSequenceNumber] != null, "_sqlRPCParameterEncryptionReqArray[resultSetSequenceNumber] should not be null.");

                    // Lookup in the dictionary to get the original rpc request corresponding to the describe parameter encryption request
                    // pointed to by _sqlRPCParameterEncryptionReqArray[resultSetSequenceNumber]
                    rpc = null;
                    lookupDictionaryResult = describeParameterEncryptionRpcOriginalRpcMap.TryGetValue(_sqlRPCParameterEncryptionReqArray[resultSetSequenceNumber++], out rpc);

                    Debug.Assert(lookupDictionaryResult,
                        "Describe Parameter Encryption RPC request key must be present in the dictionary describeParameterEncryptionRpcOriginalRpcMap");
                    Debug.Assert(rpc != null,
                        "Describe Parameter Encryption RPC request's corresponding original rpc request must not be null in the dictionary describeParameterEncryptionRpcOriginalRpcMap");
                }
                else {
                    rpc = _rpcArrayOf1[0];
                }

                Debug.Assert(rpc != null, "rpc should not be null here.");

                // This is the index in the parameters array where the actual parameters start.
                // In BatchRPCMode, parameters[0] has the t-sql, parameters[1] has the param list
                // and actual parameters of the query start at parameters[2].
                int parameterStartIndex = (BatchRPCMode ? 2 : 0);

                // Iterate over the parameter names to read the encryption type info
                int paramIdx;
                while (ds.Read()) {
#if DEBUG
                    rowsAffected++;
#endif
                    Debug.Assert(rpc != null, "Describe Parameter Encryption requested for non-tce spec proc");
                    string parameterName = ds.GetString((int)DescribeParameterEncryptionResultSet2.ParameterName);

                    // When the RPC object gets reused, the parameter array has more parameters that the valid params for the command.
                    // Null is used to indicate the end of the valid part of the array. Refer to GetRPCObject().
                    for (paramIdx = parameterStartIndex; paramIdx < rpc.parameters.Length && rpc.parameters[paramIdx] != null; paramIdx++) {
                        SqlParameter sqlParameter = rpc.parameters[paramIdx];
                        Debug.Assert(sqlParameter != null, "sqlParameter should not be null.");

                        if (sqlParameter.ParameterNameFixed.Equals(parameterName, StringComparison.Ordinal)) {
                            Debug.Assert(sqlParameter.CipherMetadata == null, "param.CipherMetadata should be null.");
                            sqlParameter.HasReceivedMetadata = true;

                            // Found the param, setup the encryption info.
                            byte columnEncryptionType = ds.GetByte((int)DescribeParameterEncryptionResultSet2.ColumnEncrytionType);
                            if ((byte)SqlClientEncryptionType.PlainText != columnEncryptionType) {
                                byte cipherAlgorithmId = ds.GetByte((int)DescribeParameterEncryptionResultSet2.ColumnEncryptionAlgorithm);
                                int columnEncryptionKeyOrdinal = ds.GetInt32((int)DescribeParameterEncryptionResultSet2.ColumnEncryptionKeyOrdinal);
                                byte columnNormalizationRuleVersion = ds.GetByte((int)DescribeParameterEncryptionResultSet2.NormalizationRuleVersion);

                                // Lookup the key, failing which throw an exception
                                if (!columnEncryptionKeyTable.TryGetValue(columnEncryptionKeyOrdinal, out cipherInfoEntry)) {
                                    throw SQL.InvalidEncryptionKeyOrdinal(columnEncryptionKeyOrdinal, columnEncryptionKeyTable.Count);
                                }

                                sqlParameter.CipherMetadata = new SqlCipherMetadata(sqlTceCipherInfoEntry: cipherInfoEntry,
                                                                                    ordinal: unchecked((ushort)-1),
                                                                                    cipherAlgorithmId: cipherAlgorithmId,
                                                                                    cipherAlgorithmName: null,
                                                                                    encryptionType: columnEncryptionType,
                                                                                    normalizationRuleVersion: columnNormalizationRuleVersion);

                                // Decrypt the symmetric key.(This will also validate and throw if needed).
                                Debug.Assert(_activeConnection != null, @"_activeConnection should not be null");
                                SqlSecurityUtility.DecryptSymmetricKey(sqlParameter.CipherMetadata, this._activeConnection.DataSource);

                                // This is effective only for BatchRPCMode even though we set it for non-BatchRPCMode also,
                                // since for non-BatchRPCMode mode, paramoptions gets thrown away and reconstructed in BuildExecuteSql.
                                rpc.paramoptions[paramIdx] |= TdsEnums.RPC_PARAM_ENCRYPTED;
                            }

                            break;
                        }
                    }
                }

                // When the RPC object gets reused, the parameter array has more parameters that the valid params for the command.
                // Null is used to indicate the end of the valid part of the array. Refer to GetRPCObject().
                for (paramIdx = parameterStartIndex; paramIdx < rpc.parameters.Length && rpc.parameters[paramIdx] != null; paramIdx++) {
                    if (!rpc.parameters[paramIdx].HasReceivedMetadata && rpc.parameters[paramIdx].Direction != ParameterDirection.ReturnValue) {
                        // Encryption MD wasn't sent by the server - we expect the metadata to be sent for all the parameters 
                        // that were sent in the original sp_describe_parameter_encryption but not necessarily for return values,
                        // since there might be multiple return values but server will only send for one of them.
                        // For parameters that don't need encryption, the encryption type is set to plaintext.
                        throw SQL.ParamEncryptionMetadataMissing(rpc.parameters[paramIdx].ParameterName, rpc.GetCommandTextOrRpcName());
                    }
                }

#if DEBUG
                Debug.Assert(rowsAffected == RowsAffectedByDescribeParameterEncryption,
                            "number of rows received for describe parameter encryption should be equal to rows affected by describe parameter encryption.");
#endif

                 // The server has responded with encryption related information for this rpc request. So clear the needsFetchParameterEncryptionMetadata flag.
                rpc.needsFetchParameterEncryptionMetadata = false;
            } while (ds.NextResult());

            // Verify that we received response for each rpc call needs tce
            if (BatchRPCMode) {
                for (int i = 0; i < _SqlRPCBatchArray.Length; i++) {
                    if (_SqlRPCBatchArray[i].needsFetchParameterEncryptionMetadata) {
                        throw SQL.ProcEncryptionMetadataMissing(_SqlRPCBatchArray[i].rpcName);
                    }
                }
            }
        }

        internal SqlDataReader RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, string method) {
            Task unused; // sync execution 
            SqlDataReader reader = RunExecuteReader(cmdBehavior, runBehavior, returnStream, method, completion:null, timeout:CommandTimeout, task:out unused);
            Debug.Assert(unused == null, "returned task during synchronous execution");
            return reader;
        }

        // task is created in case of pending asynchronous write, returned SqlDataReader should not be utilized until that task is complete 
        internal SqlDataReader RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, string method, TaskCompletionSource<object> completion, int timeout, out Task task, bool asyncWrite = false) {
            bool async = (null != completion);

            task = null;

            _rowsAffected = -1;
            _rowsAffectedBySpDescribeParameterEncryption = -1;
            
            if (0 != (CommandBehavior.SingleRow & cmdBehavior)) {
                // CommandBehavior.SingleRow implies CommandBehavior.SingleResult
                cmdBehavior |= CommandBehavior.SingleResult;
            }

            // @devnote: this function may throw for an invalid connection
            // @devnote: returns false for empty command text
            ValidateCommand(method, async);
            CheckNotificationStateAndAutoEnlist(); // Only call after validate - requires non null connection!

            TdsParser bestEffortCleanupTarget = null;
            // This section needs to occur AFTER ValidateCommand - otherwise it will AV without a connection.
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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    SqlStatistics statistics = Statistics;
                    if (null != statistics) {
                        if ((!this.IsDirty && this.IsPrepared && !_hiddenPrepare)
                            || (this.IsPrepared && _execType == EXECTYPE.PREPAREPENDING))
                        {
                            statistics.SafeIncrement(ref statistics._preparedExecs);
                        }
                        else {
                            statistics.SafeIncrement(ref statistics._unpreparedExecs);
                        }
                    }

                    // Reset the encryption related state of the command and its parameters.
                    ResetEncryptionState();

                    if ( _activeConnection.IsContextConnection ) {
                        return RunExecuteReaderSmi( cmdBehavior, runBehavior, returnStream );
                    }
                    else if (IsColumnEncryptionEnabled) {
                        Task returnTask = null;
                        PrepareForTransparentEncryption(cmdBehavior, returnStream, async, timeout, completion, out returnTask, asyncWrite && async);
                        Debug.Assert(async == (returnTask != null), @"returnTask should be null if and only if async is false.");

                        return RunExecuteReaderTdsWithTransparentParameterEncryption( cmdBehavior, runBehavior, returnStream, async, timeout, out task, asyncWrite && async, ds: null,
                            describeParameterEncryptionRequest: false, describeParameterEncryptionTask: returnTask);
                    }
                    else {
                        return RunExecuteReaderTds( cmdBehavior, runBehavior, returnStream, async, timeout, out task, asyncWrite && async);
                    }

                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e) {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e)  {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
        }

        /// <summary>
        /// RunExecuteReaderTds after Transparent Parameter Encryption is complete.
        /// </summary>
        /// <param name="cmdBehavior"></param>
        /// <param name="runBehavior"></param>
        /// <param name="returnStream"></param>
        /// <param name="async"></param>
        /// <param name="timeout"></param>
        /// <param name="task"></param>
        /// <param name="asyncWrite"></param>
        /// <param name="ds"></param>
        /// <param name="describeParameterEncryptionRequest"></param>
        /// <param name="describeParameterEncryptionTask"></param>
        /// <returns></returns>
        private SqlDataReader RunExecuteReaderTdsWithTransparentParameterEncryption(CommandBehavior cmdBehavior,
                                                                                    RunBehavior runBehavior,
                                                                                    bool returnStream,
                                                                                    bool async,
                                                                                    int timeout,
                                                                                    out Task task,
                                                                                    bool asyncWrite,
                                                                                    SqlDataReader ds=null,
                                                                                    bool describeParameterEncryptionRequest = false,
                                                                                    Task describeParameterEncryptionTask = null) {
            Debug.Assert(!asyncWrite || async, "AsyncWrite should be always accompanied by Async");
            Debug.Assert((describeParameterEncryptionTask != null) == async, @"async should be true if and only if describeParameterEncryptionTask is not null.");

            if (ds == null && returnStream) {
                ds = new SqlDataReader(this, cmdBehavior);
            }

            if (describeParameterEncryptionTask != null) {
                long parameterEncryptionStart = ADP.TimerCurrent();
                    TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
                    AsyncHelper.ContinueTask(describeParameterEncryptionTask, completion,
                        () => {
                            Task subTask = null;
                            RunExecuteReaderTds(cmdBehavior, runBehavior, returnStream, async, TdsParserStaticMethods.GetRemainingTimeout(timeout, parameterEncryptionStart), out subTask, asyncWrite, ds);
                            if (subTask == null) {
                                completion.SetResult(null);
                            }
                            else {
                                AsyncHelper.ContinueTask(subTask, completion, () => completion.SetResult(null));
                            }
                        }, connectionToDoom: null,
                        onFailure: ((exception) => {
                            if (_cachedAsyncState != null) {
                                _cachedAsyncState.ResetAsyncState();
                            }
                            if (exception != null) {
                                throw exception;
                            }}),
                        onCancellation: (() => {
                            if (_cachedAsyncState != null) {
                                _cachedAsyncState.ResetAsyncState();
                            }}),
                        connectionToAbort: _activeConnection);
                    task = completion.Task;
                    return ds;
            }
            else {
                // Synchronous execution.
                return RunExecuteReaderTds(cmdBehavior, runBehavior, returnStream, async, timeout, out task, asyncWrite, ds);
            }
        }

        private SqlDataReader RunExecuteReaderTds( CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream, bool async, int timeout, out Task task, bool asyncWrite, SqlDataReader ds=null, bool describeParameterEncryptionRequest = false) {
            Debug.Assert(!asyncWrite || async, "AsyncWrite should be always accompanied by Async");

            if (ds == null && returnStream) {
                ds = new SqlDataReader(this, cmdBehavior);
            }

            Task reconnectTask = _activeConnection.ValidateAndReconnect(null, timeout);

            if (reconnectTask != null) {
                long reconnectionStart = ADP.TimerCurrent();
                if (async) {                    
                    TaskCompletionSource<object> completion = new TaskCompletionSource<object>();
                    _activeConnection.RegisterWaitingForReconnect(completion.Task);
                    _reconnectionCompletionSource = completion;
                    CancellationTokenSource timeoutCTS = new CancellationTokenSource();
                    AsyncHelper.SetTimeoutException(completion, timeout, SQL.CR_ReconnectTimeout, timeoutCTS.Token);
                    AsyncHelper.ContinueTask(reconnectTask, completion,
                        () => {
                            if (completion.Task.IsCompleted) {
                                return;
                            }
                            Interlocked.CompareExchange(ref _reconnectionCompletionSource, null, completion);
                            timeoutCTS.Cancel();
                            Task subTask;                            
                            RunExecuteReaderTds(cmdBehavior, runBehavior, returnStream, async, TdsParserStaticMethods.GetRemainingTimeout(timeout, reconnectionStart), out subTask, asyncWrite, ds);
                            if (subTask == null) {
                                completion.SetResult(null);
                            }
                            else {
                                AsyncHelper.ContinueTask(subTask, completion, () => completion.SetResult(null));
                            }
                        }, connectionToAbort: _activeConnection);
                    task = completion.Task;
                    return ds;
                }
                else {
                    AsyncHelper.WaitForCompletion(reconnectTask, timeout, () => { throw SQL.CR_ReconnectTimeout(); });
                    timeout = TdsParserStaticMethods.GetRemainingTimeout(timeout, reconnectionStart);
                }
            }

            // make sure we have good parameter information
            // prepare the command
            // execute
            Debug.Assert(null != _activeConnection.Parser, "TdsParser class should not be null in Command.Execute!");

            bool inSchema =  (0 != (cmdBehavior & CommandBehavior.SchemaOnly));           

            // create a new RPC
            _SqlRPC rpc=null;

            task = null;

            string optionSettings = null;
            bool processFinallyBlock = true;
            bool decrementAsyncCountOnFailure = false;

            if (async) {
                _activeConnection.GetOpenTdsConnection().IncrementAsyncCount();
                decrementAsyncCountOnFailure = true;
            }

            try {
              
                if (asyncWrite) {
                    _activeConnection.AddWeakReference(this, SqlReferenceCollection.CommandTag);
                }

                GetStateObject();
                Task writeTask = null;

                if (describeParameterEncryptionRequest) {
#if DEBUG
                    if (_sleepDuringRunExecuteReaderTdsForSpDescribeParameterEncryption) {
                        Thread.Sleep(10000);
                    }
#endif

                    Debug.Assert(_sqlRPCParameterEncryptionReqArray != null, "RunExecuteReader rpc array not provided for describe parameter encryption request.");
                    writeTask = _stateObj.Parser.TdsExecuteRPC(this, _sqlRPCParameterEncryptionReqArray, timeout, inSchema, this.Notification, _stateObj, CommandType.StoredProcedure == CommandType, sync: !asyncWrite);
                }
                else if (BatchRPCMode) {
                    Debug.Assert(inSchema == false, "Batch RPC does not support schema only command beahvior");
                    Debug.Assert(!IsPrepared, "Batch RPC should not be prepared!");
                    Debug.Assert(!IsDirty, "Batch RPC should not be marked as dirty!");
                    //Currently returnStream is always false, but we may want to return a Reader later.
                    //if (returnStream) {
                    //    Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as batch RPC.\n", ObjectID);
                    //}
                    Debug.Assert(_SqlRPCBatchArray != null, "RunExecuteReader rpc array not provided");
                    writeTask = _stateObj.Parser.TdsExecuteRPC(this, _SqlRPCBatchArray, timeout, inSchema, this.Notification, _stateObj, CommandType.StoredProcedure == CommandType, sync: !asyncWrite );                    
                }
                else if ((System.Data.CommandType.Text == this.CommandType) && (0 == GetParameterCount(_parameters))) {
                    // Send over SQL Batch command if we are not a stored proc and have no parameters
                    // MDAC 
                    Debug.Assert(!IsUserPrepared, "CommandType.Text with no params should not be prepared!");
                    if (returnStream) {
                        Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as SQLBATCH.\n", ObjectID);
                    }
                    string text = GetCommandText(cmdBehavior) + GetResetOptionsString(cmdBehavior);
                    writeTask = _stateObj.Parser.TdsExecuteSQLBatch(text, timeout, this.Notification, _stateObj, sync: !asyncWrite);
                }
                else if (System.Data.CommandType.Text == this.CommandType) {
                    if (this.IsDirty) {
                        Debug.Assert(_cachedMetaData == null || !_dirty, "dirty query should not have cached metadata!"); // can have cached metadata if dirty because of parameters
                        //
                        // someone changed the command text or the parameter schema so we must unprepare the command
                        //
                        // remeber that IsDirty includes test for IsPrepared!
                        if(_execType == EXECTYPE.PREPARED) {
                            _hiddenPrepare = true;
                        }
                        Unprepare();
                        IsDirty = false;
                    }

                    if (_execType == EXECTYPE.PREPARED) {
                        Debug.Assert(this.IsPrepared && (_prepareHandle != -1), "invalid attempt to call sp_execute without a handle!");
                        rpc = BuildExecute(inSchema);
                    }
                    else if (_execType == EXECTYPE.PREPAREPENDING) {
                        Debug.Assert(_activeConnection.IsShiloh, "Invalid attempt to call sp_prepexec on non 7.x server");
                        rpc = BuildPrepExec(cmdBehavior);
                        // next time through, only do an exec
                        _execType = EXECTYPE.PREPARED;
                        _preparedConnectionCloseCount = _activeConnection.CloseCount;
                        _preparedConnectionReconnectCount = _activeConnection.ReconnectCount;
                        // mark ourselves as preparing the command
                        _inPrepare = true;
                    }
                    else {
                        Debug.Assert(_execType == EXECTYPE.UNPREPARED, "Invalid execType!");
                        BuildExecuteSql(cmdBehavior, null, _parameters, ref rpc);
                    }

                    // if shiloh, then set NOMETADATA_UNLESSCHANGED flag
                    if (_activeConnection.IsShiloh)
                        rpc.options = TdsEnums.RPC_NOMETADATA;
                    if (returnStream) {
                        Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as RPC.\n", ObjectID);
                    }

                    // 
                    Debug.Assert(_rpcArrayOf1[0] == rpc);
                    writeTask = _stateObj.Parser.TdsExecuteRPC(this, _rpcArrayOf1, timeout, inSchema, this.Notification, _stateObj, CommandType.StoredProcedure == CommandType, sync:!asyncWrite);
                }
                else {
                    Debug.Assert(this.CommandType == System.Data.CommandType.StoredProcedure, "unknown command type!");
                    // note: invalid asserts on Shiloh. On 8.0 (Shiloh) and above a command is ALWAYS prepared
                    // and IsDirty is always set if there are changes and the command is marked Prepared!
                    Debug.Assert(IsShiloh || !IsPrepared, "RPC should not be prepared!");
                    Debug.Assert(IsShiloh || !IsDirty, "RPC should not be marked as dirty!");

                    BuildRPC(inSchema, _parameters, ref rpc);

                    // if we need to augment the command because a user has changed the command behavior (e.g. FillSchema)
                    // then batch sql them over.  This is inefficient (3 round trips) but the only way we can get metadata only from
                    // a stored proc
                    optionSettings = GetSetOptionsString(cmdBehavior);
                    if (returnStream) {
                        Bid.Trace("<sc.SqlCommand.ExecuteReader|INFO> %d#, Command executed as RPC.\n", ObjectID);
                    }
                    // turn set options ON
                    if (null != optionSettings) {
                        Task executeTask = _stateObj.Parser.TdsExecuteSQLBatch(optionSettings, timeout, this.Notification, _stateObj, sync: true);
                        Debug.Assert(executeTask == null, "Shouldn't get a task when doing sync writes");
                        bool dataReady;
                        Debug.Assert(_stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
                        bool result = _stateObj.Parser.TryRun(RunBehavior.UntilDone, this, null, null, _stateObj, out dataReady);
                        if (!result) { throw SQL.SynchronousCallMayNotPend(); }
                        // and turn OFF when the ds exhausts the stream on Close()
                        optionSettings = GetResetOptionsString(cmdBehavior);
                    }

                    // turn debugging on
                    _activeConnection.CheckSQLDebug();

                    // execute sp
                    Debug.Assert(_rpcArrayOf1[0] == rpc);
                    writeTask=_stateObj.Parser.TdsExecuteRPC(this, _rpcArrayOf1, timeout, inSchema, this.Notification, _stateObj, CommandType.StoredProcedure == CommandType, sync:!asyncWrite); 
                }

                Debug.Assert(writeTask == null || async, "Returned task in sync mode");

                if (async) {
                    decrementAsyncCountOnFailure = false; 
                    if (writeTask != null) {
                        task = AsyncHelper.CreateContinuationTask(writeTask, () => {
                                     _activeConnection.GetOpenTdsConnection(); // it will throw if connection is closed
                                     cachedAsyncState.SetAsyncReaderState(ds, runBehavior, optionSettings);
                                 },
                                 onFailure: (exc) => {
                                     _activeConnection.GetOpenTdsConnection().DecrementAsyncCount(); 
                                 } );
                    }
                    else {
                        cachedAsyncState.SetAsyncReaderState(ds, runBehavior, optionSettings);
                    }
                }
                else {
                    // Always execute - even if no reader!
                    FinishExecuteReader(ds, runBehavior, optionSettings);
                }
            }
            catch (Exception e) {                
                processFinallyBlock = ADP.IsCatchableExceptionType (e);
                if (decrementAsyncCountOnFailure) {
                    SqlInternalConnectionTds innerConnectionTds = (_activeConnection.InnerConnection as SqlInternalConnectionTds);
                    if (null != innerConnectionTds) { // it may be closed 
                        innerConnectionTds.DecrementAsyncCount();
                    }
                }
                throw;
            }
            finally {
                TdsParser.ReliabilitySection.Assert("unreliable call to RunExecuteReaderTds");  // you need to setup for a thread abort somewhere before you call this method
                if (processFinallyBlock && !async) {
                    // When executing async, we need to keep the _stateObj alive...
                    PutStateObject();
                }
            }

            Debug.Assert(async || null == _stateObj, "non-null state object in RunExecuteReader");
            return ds;
        }

        private SqlDataReader RunExecuteReaderSmi( CommandBehavior cmdBehavior, RunBehavior runBehavior, bool returnStream ) {
            SqlInternalConnectionSmi innerConnection = InternalSmiConnection;

            SmiEventStream eventStream = null;
            SqlDataReader ds = null;
            SmiRequestExecutor requestExecutor = null;
            try {
                // Set it up, process all of the events, and we're done!
                requestExecutor = SetUpSmiRequest( innerConnection );

                long transactionId;
                SysTx.Transaction transaction;
                innerConnection.GetCurrentTransactionPair(out transactionId, out transaction);

                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlCommand.RunExecuteReaderSmi|ADV> %d#, innerConnection=%d#, transactionId=0x%I64x, commandBehavior=%d.\n", ObjectID, innerConnection.ObjectID, transactionId, (int)cmdBehavior);
                }

                if (SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion) {
                    eventStream = requestExecutor.Execute(
                                                    innerConnection.SmiConnection,
                                                    transactionId,
                                                    transaction,
                                                    cmdBehavior,
                                                    SmiExecuteType.Reader
                                                    );
                }
                else {
                    eventStream = requestExecutor.Execute(
                                                    innerConnection.SmiConnection,
                                                    transactionId,
                                                    cmdBehavior,
                                                    SmiExecuteType.Reader
                                                    );
                }

                if ( ( runBehavior & RunBehavior.UntilDone ) != 0 ) {

                    // Consume the results
                    while( eventStream.HasEvents ) {
                        eventStream.ProcessEvent( EventSink );
                    }
                    eventStream.Close( EventSink );
                }

                if ( returnStream ) {
                    ds = new SqlDataReaderSmi( eventStream, this, cmdBehavior, innerConnection, EventSink, requestExecutor );
                    ds.NextResult();    // Position on first set of results
                    _activeConnection.AddWeakReference(ds, SqlReferenceCollection.DataReaderTag);
                }

                EventSink.ProcessMessagesAndThrow();
            }
            catch (Exception e) {
                // VSTS 159716 - we do not want to handle ThreadAbort, OutOfMemory or similar critical exceptions
                // because the state of used objects might remain invalid in this case
                if (!ADP.IsCatchableOrSecurityExceptionType(e)) {
                    throw;
                }

                if (null != eventStream) {
                    eventStream.Close( EventSink );     // UNDONE: should cancel instead!
                }

                if (requestExecutor != null) {
                    requestExecutor.Close(EventSink);
                    EventSink.ProcessMessagesAndThrow(ignoreNonFatalMessages: true);
                }                

                throw;
            }

        return ds;
        }

        private SqlDataReader CompleteAsyncExecuteReader() {
            SqlDataReader ds = cachedAsyncState.CachedAsyncReader; // should not be null
            bool processFinallyBlock = true;
            try {
                FinishExecuteReader(ds, cachedAsyncState.CachedRunBehavior, cachedAsyncState.CachedSetOptions);
            }
            catch (Exception e) {
                processFinallyBlock = ADP.IsCatchableExceptionType(e);
                throw;
            }
            finally {
                TdsParser.ReliabilitySection.Assert("unreliable call to CompleteAsyncExecuteReader");  // you need to setup for a thread abort somewhere before you call this method
                if (processFinallyBlock) {
                    cachedAsyncState.ResetAsyncState();
                    PutStateObject();
                }
            }

            return ds;
        }

        private void FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, string resetOptionsString) {
            // always wrap with a try { FinishExecuteReader(...) } finally { PutStateObject(); }

            NotifyDependency();
            if (runBehavior == RunBehavior.UntilDone) {
                try {
                    bool dataReady;
                    Debug.Assert(_stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
                    bool result = _stateObj.Parser.TryRun(RunBehavior.UntilDone, this, ds, null, _stateObj, out dataReady);
                    if (!result) { throw SQL.SynchronousCallMayNotPend(); }
                }
                catch (Exception e) {
                    // 
                    if (ADP.IsCatchableExceptionType(e)) {
                        if (_inPrepare) {
                            // The flag is expected to be reset by OnReturnValue.  We should receive
                            // the handle unless command execution failed.  If fail, move back to pending
                            // state.
                            _inPrepare = false;                  // reset the flag
                            IsDirty = true;                      // mark command as dirty so it will be prepared next time we're comming through
                            _execType = EXECTYPE.PREPAREPENDING; // reset execution type to pending
                        }

                        if (null != ds) {
                            ds.Close();
                        }
                    }
                    throw;
                }
            }

            // bind the parser to the reader if we get this far
            if (ds != null) {
                ds.Bind(_stateObj);
                _stateObj = null;   // the reader now owns this...
                ds.ResetOptionsString = resetOptionsString;

                // 



                // bind this reader to this connection now
                _activeConnection.AddWeakReference(ds, SqlReferenceCollection.DataReaderTag);

                // force this command to start reading data off the wire.
                // this will cause an error to be reported at Execute() time instead of Read() time
                // if the command is not set.
                try {
                    _cachedMetaData = ds.MetaData;
                    ds.IsInitialized = true; // Webdata 104560
                }
                catch (Exception e) {
                    // 
                    if (ADP.IsCatchableExceptionType(e)) {
                        if (_inPrepare) {
                            // The flag is expected to be reset by OnReturnValue.  We should receive
                            // the handle unless command execution failed.  If fail, move back to pending
                            // state.
                            _inPrepare = false;                  // reset the flag
                            IsDirty = true;                      // mark command as dirty so it will be prepared next time we're comming through
                            _execType = EXECTYPE.PREPAREPENDING; // reset execution type to pending
                        }

                        ds.Close();
                    }

                    throw;
                }
            }
        }

        private void NotifyDependency() {
            if (_sqlDep != null) {
                _sqlDep.StartTimer(Notification);
            }
        }

        public SqlCommand Clone() {
            SqlCommand clone = new SqlCommand(this);
            Bid.Trace("<sc.SqlCommand.Clone|API> %d#, clone=%d#\n", ObjectID, clone.ObjectID);
            return clone;
        }

        object ICloneable.Clone() {
            return Clone();
        }

        private void RegisterForConnectionCloseNotification<T>(ref Task<T> outterTask) {
            SqlConnection connection = _activeConnection;
            if (connection == null) {
                // No connection
                throw ADP.ClosedConnectionError();
            }

            connection.RegisterForConnectionCloseNotification<T>(ref outterTask, this, SqlReferenceCollection.CommandTag);
        }

        // validates that a command has commandText and a non-busy open connection
        // throws exception for error case, returns false if the commandText is empty
        private void ValidateCommand(string method, bool async) {
            if (null == _activeConnection) {
                throw ADP.ConnectionRequired(method);
            }

            // Ensure that the connection is open and that the Parser is in the correct state
            SqlInternalConnectionTds tdsConnection = _activeConnection.InnerConnection as SqlInternalConnectionTds;

            // Ensure that if column encryption override was used then server supports its
            if (((SqlCommandColumnEncryptionSetting.UseConnectionSetting == ColumnEncryptionSetting  && _activeConnection.IsColumnEncryptionSettingEnabled)
                || (ColumnEncryptionSetting == SqlCommandColumnEncryptionSetting.Enabled || ColumnEncryptionSetting == SqlCommandColumnEncryptionSetting.ResultSetOnly))
                   && null != tdsConnection
                   && null != tdsConnection.Parser
                   && !tdsConnection.Parser.IsColumnEncryptionSupported) {
                throw SQL.TceNotSupported ();
            }

            if (tdsConnection != null) {
                var parser = tdsConnection.Parser;
                if ((parser == null) || (parser.State == TdsParserState.Closed)) {
                    throw ADP.OpenConnectionRequired(method, ConnectionState.Closed);
                }
                else if (parser.State != TdsParserState.OpenLoggedIn) {
                    throw ADP.OpenConnectionRequired(method, ConnectionState.Broken);
                }
            }
            else if (_activeConnection.State == ConnectionState.Closed) {
                throw ADP.OpenConnectionRequired(method, ConnectionState.Closed);
            }
            else if (_activeConnection.State == ConnectionState.Broken) {
                throw ADP.OpenConnectionRequired(method, ConnectionState.Broken);
            }

            ValidateAsyncCommand();

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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    // close any non MARS dead readers, if applicable, and then throw if still busy.
                    // Throw if we have a live reader on this command
                    _activeConnection.ValidateConnectionForExecute(method, this);

                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e)
            {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e)
            {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e)
            {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
            // Check to see if the currently set transaction has completed.  If so,
            // null out our local reference.
            if (null != _transaction && _transaction.Connection == null)
                _transaction = null;

            // throw if the connection is in a transaction but there is no
            // locally assigned transaction object
            if (_activeConnection.HasLocalTransactionFromAPI && (null == _transaction))
                throw ADP.TransactionRequired(method);

            // if we have a transaction, check to ensure that the active
            // connection property matches the connection associated with
            // the transaction
            if (null != _transaction && _activeConnection != _transaction.Connection)
                throw ADP.TransactionConnectionMismatch();

            if (ADP.IsEmpty(this.CommandText))
                throw ADP.CommandTextRequired(method);

            // Notification property must be null for pre-Yukon connections
            if ((Notification != null) && !_activeConnection.IsYukonOrNewer) {
                throw SQL.NotificationsRequireYukon();
            }

            if ((async) && (_activeConnection.IsContextConnection)) {
                // Async not supported on Context Connections
                throw SQL.NotAvailableOnContextConnection();
            }
        }

        private void ValidateAsyncCommand() {
            // 
            if (cachedAsyncState.PendingAsyncOperation) { // Enforce only one pending async execute at a time.
                if (cachedAsyncState.IsActiveConnectionValid(_activeConnection)) {
                    throw SQL.PendingBeginXXXExists();
                }
                else {
                    _stateObj = null; // Session was re-claimed by session pool upon connection close.
                    cachedAsyncState.ResetAsyncState();
                }
            }
        }

        private void GetStateObject(TdsParser parser = null) {
            Debug.Assert (null == _stateObj,"StateObject not null on GetStateObject");
            Debug.Assert (null != _activeConnection, "no active connection?");

            if (_pendingCancel) {
                _pendingCancel = false; // Not really needed, but we'll reset anyways.

                // If a pendingCancel exists on the object, we must have had a Cancel() call
                // between the point that we entered an Execute* API and the point in Execute* that
                // we proceeded to call this function and obtain a stateObject.  In that case,
                // we now throw a cancelled error.
                throw SQL.OperationCancelled();
            }

            if (parser == null) {
                parser = _activeConnection.Parser;
                if ((parser == null) || (parser.State == TdsParserState.Broken) || (parser.State == TdsParserState.Closed)) {
                    // Connection's parser is null as well, therefore we must be closed
                    throw ADP.ClosedConnectionError();
                }
            }

            TdsParserStateObject stateObj = parser.GetSession(this);            
            stateObj.StartSession(ObjectID);

            _stateObj = stateObj;

            if (_pendingCancel) {
                _pendingCancel = false; // Not really needed, but we'll reset anyways.

                // If a pendingCancel exists on the object, we must have had a Cancel() call
                // between the point that we entered this function and the point where we obtained
                // and actually assigned the stateObject to the local member.  It is possible
                // that the flag is set as well as a call to stateObj.Cancel - though that would
                // be a no-op.  So - throw.
                throw SQL.OperationCancelled();
            }
         }

        private void ReliablePutStateObject() {
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
                    bestEffortCleanupTarget = SqlInternalConnection.GetBestEffortCleanupTarget(_activeConnection);
                    PutStateObject();

                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException e)
            {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.StackOverflowException e)
            {
                _activeConnection.Abort(e);
                throw;
            }
            catch (System.Threading.ThreadAbortException e)
            {
                _activeConnection.Abort(e);
                SqlInternalConnection.BestEffortCleanup(bestEffortCleanupTarget);
                throw;
            }
        }

        private void PutStateObject() {
            TdsParserStateObject stateObj = _stateObj;
            _stateObj = null;

            if (null != stateObj) {
                stateObj.CloseSession();
            }
        }

        /// <summary>
        /// IMPORTANT NOTE: This is created as a copy of OnDoneProc below for Transparent Column Encryption improvement
        /// as there is not much time, to address regressions. Will revisit removing the duplication, when we have time again.
        /// </summary>
        internal void OnDoneDescribeParameterEncryptionProc(TdsParserStateObject stateObj) {
            // called per rpc batch complete
            if (BatchRPCMode) {
                // track the records affected for the just completed rpc batch
                // _rowsAffected is cumulative for ExecuteNonQuery across all rpc batches
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].cumulativeRecordsAffected = _rowsAffected;

                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].recordsAffected =
                    (((0 < _currentlyExecutingDescribeParameterEncryptionRPC) && (0 <= _rowsAffected))
                        ? (_rowsAffected - Math.Max(_sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC - 1].cumulativeRecordsAffected, 0))
                        : _rowsAffected);

                // track the error collection (not available from TdsParser after ExecuteNonQuery)
                // and the which errors are associated with the just completed rpc batch
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].errorsIndexStart =
                    ((0 < _currentlyExecutingDescribeParameterEncryptionRPC)
                        ? _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC - 1].errorsIndexEnd
                        : 0);
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].errorsIndexEnd = stateObj.ErrorCount;
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].errors = stateObj._errors;

                // track the warning collection (not available from TdsParser after ExecuteNonQuery)
                // and the which warnings are associated with the just completed rpc batch
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].warningsIndexStart =
                    ((0 < _currentlyExecutingDescribeParameterEncryptionRPC)
                        ? _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC - 1].warningsIndexEnd
                        : 0);
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].warningsIndexEnd = stateObj.WarningCount;
                _sqlRPCParameterEncryptionReqArray[_currentlyExecutingDescribeParameterEncryptionRPC].warnings = stateObj._warnings;

                _currentlyExecutingDescribeParameterEncryptionRPC++;
            }
        }

        /// <summary>
        /// IMPORTANT NOTE: There is a copy of this function above in OnDoneDescribeParameterEncryptionProc.
        /// Please consider the changes being done in this function for the above function as well.
        /// </summary>
        internal void OnDoneProc() { // called per rpc batch complete
            if (BatchRPCMode) {

                // track the records affected for the just completed rpc batch
                // _rowsAffected is cumulative for ExecuteNonQuery across all rpc batches
                _SqlRPCBatchArray[_currentlyExecutingBatch].cumulativeRecordsAffected = _rowsAffected;

                _SqlRPCBatchArray[_currentlyExecutingBatch].recordsAffected =
                    (((0 < _currentlyExecutingBatch) && (0 <= _rowsAffected))
                        ? (_rowsAffected - Math.Max(_SqlRPCBatchArray[_currentlyExecutingBatch-1].cumulativeRecordsAffected, 0))
                        : _rowsAffected);

                // track the error collection (not available from TdsParser after ExecuteNonQuery)
                // and the which errors are associated with the just completed rpc batch
                _SqlRPCBatchArray[_currentlyExecutingBatch].errorsIndexStart =
                    ((0 < _currentlyExecutingBatch)
                        ? _SqlRPCBatchArray[_currentlyExecutingBatch-1].errorsIndexEnd
                        : 0);
                _SqlRPCBatchArray[_currentlyExecutingBatch].errorsIndexEnd = _stateObj.ErrorCount;
                _SqlRPCBatchArray[_currentlyExecutingBatch].errors = _stateObj._errors;
                
                // track the warning collection (not available from TdsParser after ExecuteNonQuery)
                // and the which warnings are associated with the just completed rpc batch
                _SqlRPCBatchArray[_currentlyExecutingBatch].warningsIndexStart =
                    ((0 < _currentlyExecutingBatch)
                        ? _SqlRPCBatchArray[_currentlyExecutingBatch-1].warningsIndexEnd
                        : 0);
                _SqlRPCBatchArray[_currentlyExecutingBatch].warningsIndexEnd = _stateObj.WarningCount;
                _SqlRPCBatchArray[_currentlyExecutingBatch].warnings = _stateObj._warnings;

                _currentlyExecutingBatch++;
                Debug.Assert(_parameterCollectionList.Count >= _currentlyExecutingBatch, "OnDoneProc: Too many DONEPROC events");
            }
        }

        //
        // 


        internal void OnReturnStatus(int status) {
            if (_inPrepare)
                return;

            // Don't set the return status if this is the status for sp_describe_parameter_encryption.
            if (IsDescribeParameterEncryptionRPCCurrentlyInProgress)
                return;

            SqlParameterCollection parameters = _parameters;
            if (BatchRPCMode) {
                if (_parameterCollectionList.Count > _currentlyExecutingBatch) {
                    parameters = _parameterCollectionList[_currentlyExecutingBatch];
                }
                else {
                    Debug.Assert(false, "OnReturnStatus: SqlCommand got too many DONEPROC events");
                    parameters = null;
                }
            }
            // see if a return value is bound
            int count = GetParameterCount(parameters);
            for (int i = 0; i < count; i++) {
                SqlParameter parameter = parameters[i];
                if (parameter.Direction == ParameterDirection.ReturnValue) {
                    object v = parameter.Value;

                // if the user bound a sqlint32 (the only valid one for status, use it)
                if ( (null != v) && (v.GetType() == typeof(SqlInt32)) ) {
                        parameter.Value = new SqlInt32(status); // value type
                }
                else {
                        parameter.Value = status;

                    }
                    break;
                }
            }
        }

        //
        // Move the return value to the corresponding output parameter.
        // Return parameters are sent in the order in which they were defined in the procedure.
        // If named, match the parameter name, otherwise fill in based on ordinal position.
        // If the parameter is not bound, then ignore the return value.
        //
        internal void OnReturnValue(SqlReturnValue rec, TdsParserStateObject stateObj) {

            if (_inPrepare) {
                if (!rec.value.IsNull) {
                    _prepareHandle = rec.value.Int32;
                }
                _inPrepare = false;
                return;
            }

            SqlParameterCollection parameters = GetCurrentParameterCollection();
            int  count      = GetParameterCount(parameters);


            SqlParameter thisParam = GetParameterForOutputValueExtraction(parameters, rec.parameter, count);

            if (null != thisParam) {
                // If the parameter's direction is InputOutput, Output or ReturnValue and it needs to be transparently encrypted/decrypted
                // then simply decrypt, deserialize and set the value.
                if (rec.cipherMD != null &&
                    thisParam.CipherMetadata != null && 
                    (thisParam.Direction == ParameterDirection.Output || 
                    thisParam.Direction == ParameterDirection.InputOutput || 
                    thisParam.Direction == ParameterDirection.ReturnValue)) {
                    if(rec.tdsType != TdsEnums.SQLBIGVARBINARY) {
                        throw SQL.InvalidDataTypeForEncryptedParameter(thisParam.ParameterNameFixed, rec.tdsType, TdsEnums.SQLBIGVARBINARY);
                    }

                    // Decrypt the ciphertext
                    TdsParser parser = _activeConnection.Parser;
                    if ((parser == null) || (parser.State == TdsParserState.Closed) || (parser.State == TdsParserState.Broken)) {
                        throw ADP.ClosedConnectionError();
                    }

                    if (!rec.value.IsNull) {
                        try {
                            Debug.Assert(_activeConnection != null, @"_activeConnection should not be null");

                            // Get the key information from the parameter and decrypt the value.
                            rec.cipherMD.EncryptionInfo = thisParam.CipherMetadata.EncryptionInfo;
                            byte[] unencryptedBytes = SqlSecurityUtility.DecryptWithKey(rec.value.ByteArray, rec.cipherMD, _activeConnection.DataSource);

                            if (unencryptedBytes != null) {
                                // Denormalize the value and convert it to the parameter type.
                                SqlBuffer buffer = new SqlBuffer();
                                parser.DeserializeUnencryptedValue(buffer, unencryptedBytes, rec, stateObj, rec.NormalizationRuleVersion);
                                thisParam.SetSqlBuffer(buffer);
                            }
                        }
                        catch (Exception e) {
                            throw SQL.ParamDecryptionFailed(thisParam.ParameterNameFixed, null, e);
                        }
                    }
                    else {
                        // Create a new SqlBuffer and set it to null
                        // Note: We can't reuse the SqlBuffer in "rec" below since it's already been set (to varbinary)
                        // in previous call to TryProcessReturnValue(). 
                        // Note 2: We will be coming down this code path only if the Command Setting is set to use TCE.
                        // We pass the command setting as TCE enabled in the below call for this reason.
                        SqlBuffer buff = new SqlBuffer();
                        TdsParser.GetNullSqlValue(buff, rec, SqlCommandColumnEncryptionSetting.Enabled, parser.Connection);
                        thisParam.SetSqlBuffer(buff);
                    }
                }
                else {
                    // copy over data

                    // if the value user has supplied a SqlType class, then just copy over the SqlType, otherwise convert
                    // to the com type
                    object val = thisParam.Value;

                    //set the UDT value as typed object rather than bytes
                    if (SqlDbType.Udt == thisParam.SqlDbType) {
                        object data = null;
                        try {
                            Connection.CheckGetExtendedUDTInfo(rec, true);

                            //extract the byte array from the param value
                            if (rec.value.IsNull)
                                data = DBNull.Value;
                            else {
                                data = rec.value.ByteArray; //should work for both sql and non-sql values
                            }

                            //call the connection to instantiate the UDT object
                            thisParam.Value = Connection.GetUdtValue(data, rec, false);
                        }
                        catch (FileNotFoundException e) {
                            // SQL BU DT 329981
                            // Assign Assembly.Load failure in case where assembly not on client.
                            // This allows execution to complete and failure on SqlParameter.Value.
                            thisParam.SetUdtLoadError(e);
                        }
                        catch (FileLoadException e) {
                            // SQL BU DT 329981
                            // Assign Assembly.Load failure in case where assembly cannot be loaded on client.
                            // This allows execution to complete and failure on SqlParameter.Value.
                            thisParam.SetUdtLoadError(e);
                        }

                        return;
                    } else {
                        thisParam.SetSqlBuffer(rec.value);
                    }

                    MetaType mt = MetaType.GetMetaTypeFromSqlDbType(rec.type, rec.isMultiValued);

                    if (rec.type == SqlDbType.Decimal) {
                        thisParam.ScaleInternal = rec.scale;
                        thisParam.PrecisionInternal = rec.precision;
                    }
                    else if (mt.IsVarTime) {
                        thisParam.ScaleInternal = rec.scale;
                    }
                    else if (rec.type == SqlDbType.Xml) {
                        SqlCachedBuffer cachedBuffer = (thisParam.Value as SqlCachedBuffer);
                        if (null != cachedBuffer) {
                            thisParam.Value = cachedBuffer.ToString();
                        }
                    }

                    if (rec.collation != null) {
                        Debug.Assert(mt.IsCharType, "Invalid collation structure for non-char type");
                        thisParam.Collation = rec.collation;
                    }
                }
            }

            return;
        }

        internal void OnParametersAvailableSmi( SmiParameterMetaData[] paramMetaData, ITypedGettersV3 parameterValues ) {
            Debug.Assert(null != paramMetaData);

            for(int index=0; index < paramMetaData.Length; index++) {
                OnParameterAvailableSmi(paramMetaData[index], parameterValues, index);
            }
        }

        internal void OnParameterAvailableSmi(SmiParameterMetaData metaData, ITypedGettersV3 parameterValues, int ordinal) {
            if ( ParameterDirection.Input != metaData.Direction ) {
                string name = null;
                if (ParameterDirection.ReturnValue != metaData.Direction) {
                    name = metaData.Name;
                }

                SqlParameterCollection parameters = GetCurrentParameterCollection();
                int  count      = GetParameterCount(parameters);
                SqlParameter param = GetParameterForOutputValueExtraction(parameters, name, count);

                if ( null != param ) {
                    param.LocaleId = (int)metaData.LocaleId;
                    param.CompareInfo = metaData.CompareOptions;
                    SqlBuffer buffer = new SqlBuffer();
                    object result;
                    if (_activeConnection.IsKatmaiOrNewer) {
                        result = ValueUtilsSmi.GetOutputParameterV200Smi(
                                OutParamEventSink, (SmiTypedGetterSetter)parameterValues, ordinal, metaData, _smiRequestContext, buffer );
                    }
                    else {
                        result = ValueUtilsSmi.GetOutputParameterV3Smi( 
                                    OutParamEventSink, parameterValues, ordinal, metaData, _smiRequestContext, buffer );
                    }
                    if ( null != result ) {
                        param.Value = result;
                    }
                    else {
                        param.SetSqlBuffer( buffer );
                    }
                }
            }
        }

        private SqlParameterCollection GetCurrentParameterCollection() {
            if (BatchRPCMode) {
                if (_parameterCollectionList.Count > _currentlyExecutingBatch) {
                    return _parameterCollectionList[_currentlyExecutingBatch];
                }
                else {
                    Debug.Assert(false, "OnReturnValue: SqlCommand got too many DONEPROC events");
                    return null;
                }
            }
            else {
                return _parameters;
            }
        }

        private SqlParameter GetParameterForOutputValueExtraction( SqlParameterCollection parameters,
                        string paramName, int paramCount ) {
            SqlParameter thisParam = null;
            bool foundParam = false;

            if (null == paramName) {
                // rec.parameter should only be null for a return value from a function
                for (int i = 0; i < paramCount; i++) {
                    thisParam = parameters[i];
                    // searching for ReturnValue
                    if (thisParam.Direction == ParameterDirection.ReturnValue) {
                                foundParam = true;
                            break; // found it
                    }
                }
            }
            else {
                for (int i = 0; i < paramCount; i++) {
                    thisParam = parameters[i];
                    // searching for Output or InputOutput or ReturnValue with matching name
                    if (thisParam.Direction != ParameterDirection.Input && thisParam.Direction != ParameterDirection.ReturnValue  && paramName == thisParam.ParameterNameFixed) {
                                foundParam = true;
                            break; // found it
                        }
                    }
            }
            if (foundParam)
                return thisParam;
            else
                return null;
        }

        private void GetRPCObject(int paramCount, ref _SqlRPC rpc, bool forSpDescribeParameterEncryption = false) {
            // Designed to minimize necessary allocations
            int ii;
            if (rpc == null) {
                if (!forSpDescribeParameterEncryption) {
                    if (_rpcArrayOf1 == null) {
                        _rpcArrayOf1 = new _SqlRPC[1];
                        _rpcArrayOf1[0] = new _SqlRPC();
                    }

                    rpc = _rpcArrayOf1[0];
                }
                else {
                    if (_rpcForEncryption == null) {
                        _rpcForEncryption = new _SqlRPC();
                    }

                    rpc = _rpcForEncryption;
                }
            }

            rpc.ProcID = 0;
            rpc.rpcName = null;
            rpc.options = 0;

            rpc.recordsAffected = default(int?);
            rpc.cumulativeRecordsAffected = -1;

            rpc.errorsIndexStart = 0;
            rpc.errorsIndexEnd = 0;
            rpc.errors = null;
            
            rpc.warningsIndexStart = 0;
            rpc.warningsIndexEnd = 0;
            rpc.warnings = null;
            rpc.needsFetchParameterEncryptionMetadata = false;

            // Make sure there is enough space in the parameters and paramoptions arrays
            if(rpc.parameters == null || rpc.parameters.Length < paramCount) {
                rpc.parameters = new SqlParameter[paramCount];
            }
            else if (rpc.parameters.Length > paramCount) {
                        rpc.parameters[paramCount]=null;    // Terminator
            }
            if(rpc.paramoptions == null || (rpc.paramoptions.Length < paramCount)) {
                rpc.paramoptions = new byte[paramCount];
            }
            else {
                for (ii = 0 ; ii < paramCount ; ii++)
                    rpc.paramoptions[ii] = 0;
            }
        }

        private void SetUpRPCParameters (_SqlRPC rpc, int startCount, bool inSchema, SqlParameterCollection parameters) {
            int ii;
            int paramCount = GetParameterCount(parameters) ;
            int j = startCount;
            TdsParser parser = _activeConnection.Parser;
            bool yukonOrNewer = parser.IsYukonOrNewer;

            for (ii = 0;  ii < paramCount; ii++) {
                SqlParameter parameter = parameters[ii];
                parameter.Validate(ii, CommandType.StoredProcedure == CommandType);

                // func will change type to that with a 4 byte length if the type has a two
                // byte length and a parameter length > than that expressable in 2 bytes
                if ((!parameter.ValidateTypeLengths(yukonOrNewer).IsPlp) && (parameter.Direction != ParameterDirection.Output)) {
                    parameter.FixStreamDataForNonPLP();
                }

                if (ShouldSendParameter(parameter)) {
                    rpc.parameters[j] = parameter;

                    // set output bit
                    if (parameter.Direction == ParameterDirection.InputOutput ||
                        parameter.Direction == ParameterDirection.Output)
                        rpc.paramoptions[j] = TdsEnums.RPC_PARAM_BYREF;

                    // Set the encryped bit, if the parameter is to be encrypted.
                    if (parameter.CipherMetadata != null) {
                        rpc.paramoptions[j] |= TdsEnums.RPC_PARAM_ENCRYPTED;
                    }

                    // set default value bit
                    if (parameter.Direction != ParameterDirection.Output) {
                        // remember that null == Convert.IsEmpty, DBNull.Value is a database null!

                        // MDAC 62117, don't assume a default value exists for parameters in the case when
                        // the user is simply requesting schema
                        // SQLBUVSTS 179488 TVPs use DEFAULT and do not allow NULL, even for schema only.
                        if (null == parameter.Value && (!inSchema || SqlDbType.Structured == parameter.SqlDbType)) {
                            rpc.paramoptions[j] |= TdsEnums.RPC_PARAM_DEFAULT;
                        }
                    }

                    // Must set parameter option bit for LOB_COOKIE if unfilled LazyMat blob
                    j++;
                }
            }

        }

        //
        // 7.5
        // prototype for sp_prepexec is:
        // sp_prepexec(@handle int IN/OUT, @batch_params ntext, @batch_text ntext, param1value,param2value...)
        //
        private _SqlRPC  BuildPrepExec(CommandBehavior behavior) {
            Debug.Assert(System.Data.CommandType.Text == this.CommandType, "invalid use of sp_prepexec for stored proc invocation!");
            SqlParameter sqlParam;
            int j = 3;

            int count = CountSendableParameters(_parameters);

            _SqlRPC rpc = null;
            GetRPCObject(count + j, ref rpc);

            rpc.ProcID = TdsEnums.RPC_PROCID_PREPEXEC;
            rpc.rpcName = TdsEnums.SP_PREPEXEC;

            //@handle
            sqlParam = new SqlParameter(null, SqlDbType.Int);
            sqlParam.Direction = ParameterDirection.InputOutput;
            sqlParam.Value = _prepareHandle;
            rpc.parameters[0] = sqlParam;
            rpc.paramoptions[0] = TdsEnums.RPC_PARAM_BYREF;

            //@batch_params
            string paramList = BuildParamList(_stateObj.Parser, _parameters);
            sqlParam = new SqlParameter(null, ((paramList.Length<<1)<=TdsEnums.TYPE_SIZE_LIMIT)?SqlDbType.NVarChar:SqlDbType.NText, paramList.Length);
            sqlParam.Value = paramList;
            rpc.parameters[1] = sqlParam;

            //@batch_text
            string text = GetCommandText(behavior);
            sqlParam = new SqlParameter(null, ((text.Length<<1)<=TdsEnums.TYPE_SIZE_LIMIT)?SqlDbType.NVarChar:SqlDbType.NText, text.Length);
            sqlParam.Value = text;
            rpc.parameters[2] = sqlParam;

            SetUpRPCParameters (rpc,  j, false, _parameters);
            return rpc;
        }


        //
        // returns true if the parameter is not a return value
        // and it's value is not DBNull (for a nullable parameter)
        //
        private static bool ShouldSendParameter(SqlParameter p, bool includeReturnValue = false) {
            switch (p.Direction) {
            case ParameterDirection.ReturnValue:
                // return value parameters are not sent, except for the parameter list of sp_describe_parameter_encryption
                return includeReturnValue;
            case ParameterDirection.Output:
            case ParameterDirection.InputOutput:
            case ParameterDirection.Input:
                // InputOutput/Output parameters are aways sent
                return true;
            default:
                Debug.Assert(false, "Invalid ParameterDirection!");
                return false;
            }
        }

        private int CountSendableParameters(SqlParameterCollection parameters) {
            int cParams = 0;

            if (parameters != null) {
                int count = parameters.Count;
                for (int i = 0; i < count; i++) {
                    if (ShouldSendParameter(parameters[i]))
                        cParams++;
                }
            }
            return cParams;
        }

        // Returns total number of parameters
        private int GetParameterCount(SqlParameterCollection parameters) {
            return ((null != parameters) ? parameters.Count : 0);
        }

        //
        // build the RPC record header for this stored proc and add parameters
        //
        private void BuildRPC(bool inSchema, SqlParameterCollection parameters, ref _SqlRPC rpc) {
            Debug.Assert(this.CommandType == System.Data.CommandType.StoredProcedure, "Command must be a stored proc to execute an RPC");
            int count = CountSendableParameters(parameters);
            GetRPCObject(count, ref rpc);

            rpc.rpcName = this.CommandText; // just get the raw command text

            SetUpRPCParameters ( rpc, 0, inSchema, parameters);
        }

        //
        // build the RPC record header for sp_unprepare
        //
        // prototype for sp_unprepare is:
        // sp_unprepare(@handle)
        //
        // 
        private _SqlRPC BuildUnprepare() {
            Debug.Assert(_prepareHandle != 0, "Invalid call to sp_unprepare without a valid handle!");

            _SqlRPC rpc = null;
            GetRPCObject(1, ref rpc);
            SqlParameter sqlParam;

            rpc.ProcID = TdsEnums.RPC_PROCID_UNPREPARE;
            rpc.rpcName = TdsEnums.SP_UNPREPARE;

            //@handle
            sqlParam = new SqlParameter(null, SqlDbType.Int);
            sqlParam.Value = _prepareHandle;
            rpc.parameters[0] = sqlParam;

            return rpc;
        }

        //
        // build the RPC record header for sp_execute
        //
        // prototype for sp_execute is:
        // sp_execute(@handle int,param1value,param2value...)
        //
        private _SqlRPC BuildExecute(bool inSchema) {
            Debug.Assert(_prepareHandle != -1, "Invalid call to sp_execute without a valid handle!");
            int j = 1;

            int count = CountSendableParameters(_parameters);

            _SqlRPC rpc = null;
            GetRPCObject(count + j, ref rpc);

            SqlParameter sqlParam;

            rpc.ProcID = TdsEnums.RPC_PROCID_EXECUTE;
            rpc.rpcName = TdsEnums.SP_EXECUTE;

            //@handle
            sqlParam = new SqlParameter(null, SqlDbType.Int);
            sqlParam.Value = _prepareHandle;
            rpc.parameters[0] = sqlParam;

            SetUpRPCParameters (rpc, j, inSchema, _parameters);
            return rpc;
        }

        //
        // build the RPC record header for sp_executesql and add the parameters
        //
        // prototype for sp_executesql is:
        // sp_executesql(@batch_text nvarchar(4000),@batch_params nvarchar(4000), param1,.. paramN)
        private void BuildExecuteSql(CommandBehavior behavior, string commandText, SqlParameterCollection parameters, ref _SqlRPC rpc) {

            Debug.Assert(_prepareHandle == -1, "This command has an existing handle, use sp_execute!");
            Debug.Assert(System.Data.CommandType.Text == this.CommandType, "invalid use of sp_executesql for stored proc invocation!");
            int j;
            SqlParameter sqlParam;

            int cParams = CountSendableParameters(parameters);
            if (cParams > 0) {
                j = 2;
            }
            else {
                j =1;
            }

            GetRPCObject(cParams + j, ref rpc);
            rpc.ProcID = TdsEnums.RPC_PROCID_EXECUTESQL;
            rpc.rpcName = TdsEnums.SP_EXECUTESQL;

            // @sql
            if (commandText == null) {
                commandText = GetCommandText(behavior);
            }
            sqlParam = new SqlParameter(null, ((commandText.Length<<1)<=TdsEnums.TYPE_SIZE_LIMIT)?SqlDbType.NVarChar:SqlDbType.NText, commandText.Length);
            sqlParam.Value = commandText;
            rpc.parameters[0] = sqlParam;

            if (cParams > 0) {
                string paramList = BuildParamList(_stateObj.Parser, BatchRPCMode  ? parameters : _parameters);
                sqlParam = new SqlParameter(null, ((paramList.Length<<1)<=TdsEnums.TYPE_SIZE_LIMIT)?SqlDbType.NVarChar:SqlDbType.NText, paramList.Length);
                sqlParam.Value = paramList;
                rpc.parameters[1] = sqlParam;

                bool inSchema =  (0 != (behavior & CommandBehavior.SchemaOnly));
                SetUpRPCParameters (rpc, j,  inSchema, parameters);
            }
        }

        /// <summary>
        /// This function constructs a string parameter containing the exec statement in the following format
        /// N'EXEC sp_name @param1=@param1, @param1=@param2, ..., @paramN=@paramN'
        /// 




        private SqlParameter BuildStoredProcedureStatementForColumnEncryption(string storedProcedureName, SqlParameter[] parameters) {
            Debug.Assert(CommandType == CommandType.StoredProcedure, "BuildStoredProcedureStatementForColumnEncryption() should only be called for stored procedures");
            Debug.Assert(!string.IsNullOrWhiteSpace(storedProcedureName), "storedProcedureName cannot be null or empty in BuildStoredProcedureStatementForColumnEncryption");
            Debug.Assert(parameters != null, "parameters cannot be null in BuildStoredProcedureStatementForColumnEncryption");

            StringBuilder execStatement = new StringBuilder();
            execStatement.Append(@"EXEC ");

            // Find the return value parameter (if any).
            SqlParameter returnValueParameter = null;
            foreach (SqlParameter parameter in parameters) {
                if (parameter.Direction == ParameterDirection.ReturnValue) {
                    returnValueParameter = parameter;
                    break;
                }
            }

            // If there is a return value parameter we need to assign the result to it.
            // EXEC @returnValue = moduleName [parameters]
            if (returnValueParameter != null) {
                execStatement.AppendFormat(@"{0}=", returnValueParameter.ParameterNameFixed);
            }

            execStatement.Append(ParseAndQuoteIdentifier(storedProcedureName, false));

            // Build parameter list in the format
            // @param1=@param1, @param1=@param2, ..., @paramn=@paramn

            // Append the first parameter
            int i = 0;

            if(parameters.Count() > 0) {
                // Skip the return value parameters.
                while (i < parameters.Count() && parameters[i].Direction == ParameterDirection.ReturnValue) {
                    i++;
                }

                if (i < parameters.Count()) {
                    // Possibility of a SQL Injection issue through parameter names and how to construct valid identifier for parameters.
                    // Since the parameters comes from application itself, there should not be a security vulnerability.
                    // Also since the query is not executed, but only analyzed there is no possibility for elevation of priviledge, but only for 
                    // incorrect results which would only affect the user that attempts the injection.
                    execStatement.AppendFormat(@" {0}={0}", parameters[i].ParameterNameFixed);
                
                    // InputOutput and Output parameters need to be marked as such.
                    if (parameters[i].Direction == ParameterDirection.Output ||
                        parameters[i].Direction == ParameterDirection.InputOutput) {
                        execStatement.AppendFormat(@" OUTPUT");
                    }
                }
            }

            // Move to the next parameter.
            i++;

            // Append the rest of parameters
            for (; i < parameters.Count(); i++) {
                if (parameters[i].Direction != ParameterDirection.ReturnValue) {
                    execStatement.AppendFormat(@", {0}={0}", parameters[i].ParameterNameFixed);

                    // InputOutput and Output parameters need to be marked as such.
                    if (parameters[i].Direction == ParameterDirection.Output ||
                    parameters[i].Direction == ParameterDirection.InputOutput) {
                        execStatement.AppendFormat(@" OUTPUT");
                    }
                }
            }

            // Construct @tsql SqlParameter to be returned
            SqlParameter tsqlParameter = new SqlParameter(null, ((execStatement.Length << 1) <= TdsEnums.TYPE_SIZE_LIMIT) ? SqlDbType.NVarChar : SqlDbType.NText, execStatement.Length);
            tsqlParameter.Value = execStatement.ToString();

            return tsqlParameter;
        }

        // paramList parameter for sp_executesql, sp_prepare, and sp_prepexec
        internal string BuildParamList(TdsParser parser, SqlParameterCollection parameters, bool includeReturnValue = false) {
            StringBuilder paramList = new StringBuilder();
            bool fAddSeperator = false;

            bool yukonOrNewer = parser.IsYukonOrNewer;

            int count = 0;

            count = parameters.Count;
            for (int i = 0; i < count; i++) {
                SqlParameter sqlParam = parameters[i];
                sqlParam.Validate(i, CommandType.StoredProcedure == CommandType);
                // skip ReturnValue parameters; we never send them to the server
                if (!ShouldSendParameter(sqlParam, includeReturnValue))
                    continue;

                // add our separator for the ith parmeter
                if (fAddSeperator)
                    paramList.Append(',');

                paramList.Append(sqlParam.ParameterNameFixed);

                MetaType mt = sqlParam.InternalMetaType;

                //for UDTs, get the actual type name. Get only the typename, omitt catalog and schema names.
                //in TSQL you should only specify the unqualified type name

                // paragraph above doesn't seem to be correct. Server won't find the type
                // if we don't provide a fully qualified name
                paramList.Append(" ");
                if (mt.SqlDbType == SqlDbType.Udt) {
                    string fullTypeName = sqlParam.UdtTypeName;
                    if(ADP.IsEmpty(fullTypeName))
                        throw SQL.MustSetUdtTypeNameForUdtParams();
                    // DEVNOTE: do we need to escape the full type name?
                    paramList.Append(ParseAndQuoteIdentifier(fullTypeName, true /* is UdtTypeName */));
                }
                else if (mt.SqlDbType == SqlDbType.Structured) {
                    string typeName = sqlParam.TypeName;
                    if (ADP.IsEmpty(typeName)) {
                        throw SQL.MustSetTypeNameForParam(mt.TypeName, sqlParam.ParameterNameFixed);
                    }
                    paramList.Append(ParseAndQuoteIdentifier(typeName, false /* is not UdtTypeName*/));

                    // TVPs currently are the only Structured type and must be read only, so add that keyword
                    paramList.Append(" READONLY");
                }
                else {
                    // func will change type to that with a 4 byte length if the type has a two
                    // byte length and a parameter length > than that expressable in 2 bytes
                    mt  = sqlParam.ValidateTypeLengths(yukonOrNewer);
                    if ((!mt.IsPlp) && (sqlParam.Direction != ParameterDirection.Output)) {
                        sqlParam.FixStreamDataForNonPLP();
                    }
                    paramList.Append(mt.TypeName);
                }

                fAddSeperator = true;

                if (mt.SqlDbType == SqlDbType.Decimal) {
                    byte precision = sqlParam.GetActualPrecision();
                    byte scale = sqlParam.GetActualScale();

                    paramList.Append('(');

                    if (0 == precision) {
                        if (IsShiloh) {
                            precision = TdsEnums.DEFAULT_NUMERIC_PRECISION;
                        } else {
                            precision = TdsEnums.SPHINX_DEFAULT_NUMERIC_PRECISION;
                        }
                    }

                    paramList.Append(precision);
                    paramList.Append(',');
                    paramList.Append(scale);
                    paramList.Append(')');
                }
                else if (mt.IsVarTime) {
                    byte scale = sqlParam.GetActualScale();

                    paramList.Append('(');
                    paramList.Append(scale);
                    paramList.Append(')');
                }
                else if (false == mt.IsFixed && false == mt.IsLong && mt.SqlDbType != SqlDbType.Timestamp && mt.SqlDbType != SqlDbType.Udt && SqlDbType.Structured != mt.SqlDbType) {
                    int size = sqlParam.Size;

                    paramList.Append('(');

                    // if using non unicode types, obtain the actual byte length from the parser, with it's associated code page
                    if (mt.IsAnsiType) {
                        object val = sqlParam.GetCoercedValue();
                        string s = null;

                        // deal with the sql types
                        if ((null != val) && (DBNull.Value != val)) {
                            s = (val as string);
                            if (null == s) {
                                SqlString sval = val is SqlString ? (SqlString)val : SqlString.Null;
                                if (!sval.IsNull) {
                                    s = sval.Value;
                                }
                            }
                        }

                        if (null != s) {
                            int actualBytes = parser.GetEncodingCharLength(s, sqlParam.GetActualSize(), sqlParam.Offset, null);
                            // if actual number of bytes is greater than the user given number of chars, use actual bytes
                            if (actualBytes > size)
                                size = actualBytes;
                        }
                    }

                    // bug 49497, if the user specifies a 0-sized parameter for a variable len field
                    // pass over max size (8000 bytes or 4000 characters for wide types)
                    if (0 == size)
                        size = mt.IsSizeInCharacters ? (TdsEnums.MAXSIZE >> 1) : TdsEnums.MAXSIZE;

                    paramList.Append(size);
                    paramList.Append(')');
                }
                else if (mt.IsPlp && (mt.SqlDbType != SqlDbType.Xml) && (mt.SqlDbType != SqlDbType.Udt)) {
                    paramList.Append("(max) ");
                }

                // set the output bit for Output or InputOutput parameters
                if (sqlParam.Direction != ParameterDirection.Input)
                    paramList.Append(" " + TdsEnums.PARAM_OUTPUT);
            }

            return paramList.ToString();
        }

        // Adds quotes to each part of a SQL identifier that may be multi-part, while leaving
        //  the result as a single composite name.
        private string ParseAndQuoteIdentifier(string identifier, bool isUdtTypeName) {
            string[] strings = SqlParameter.ParseTypeName(identifier, isUdtTypeName);
            StringBuilder bld = new StringBuilder();

            // Stitching back together is a little tricky. Assume we want to build a full multi-part name
            //  with all parts except trimming separators for leading empty names (null or empty strings,
            //  but not whitespace). Separators in the middle should be added, even if the name part is 
            //  null/empty, to maintain proper location of the parts.
            for (int i = 0; i < strings.Length; i++ ) {
                if (0 < bld.Length) {
                    bld.Append('.');
                }
                if (null != strings[i] && 0 != strings[i].Length) {
                    bld.Append(ADP.BuildQuotedString("[", "]", strings[i]));
                }
            }

            return bld.ToString();
        }

        // returns set option text to turn on format only and key info on and off
        // @devnote:  When we are executing as a text command, then we never need
        // to turn off the options since they command text is executed in the scope of sp_executesql.
        // For a stored proc command, however, we must send over batch sql and then turn off
        // the set options after we read the data.  See the code in Command.Execute()
        private string GetSetOptionsString(CommandBehavior behavior) {
            string s = null;

            if ((System.Data.CommandBehavior.SchemaOnly == (behavior & CommandBehavior.SchemaOnly)) ||
               (System.Data.CommandBehavior.KeyInfo == (behavior & CommandBehavior.KeyInfo))) {

                // MDAC 56898 - SET FMTONLY ON will cause the server to ignore other SET OPTIONS, so turn
                // it off before we ask for browse mode metadata
                s = TdsEnums.FMTONLY_OFF;

                if (System.Data.CommandBehavior.KeyInfo == (behavior & CommandBehavior.KeyInfo)) {
                    s = s + TdsEnums.BROWSE_ON;
                }

                if (System.Data.CommandBehavior.SchemaOnly == (behavior & CommandBehavior.SchemaOnly)) {
                    s = s + TdsEnums.FMTONLY_ON;
                }
            }

            return s;
        }

        private string GetResetOptionsString(CommandBehavior behavior) {
            string s = null;

            // SET FMTONLY ON OFF
            if (System.Data.CommandBehavior.SchemaOnly == (behavior & CommandBehavior.SchemaOnly)) {
                s = s + TdsEnums.FMTONLY_OFF;
            }

            // SET NO_BROWSETABLE OFF
            if (System.Data.CommandBehavior.KeyInfo == (behavior & CommandBehavior.KeyInfo)) {
                s = s + TdsEnums.BROWSE_OFF;
            }

            return s;
        }

        private String GetCommandText(CommandBehavior behavior) {
            // build the batch string we send over, since we execute within a stored proc (sp_executesql), the SET options never need to be
            // turned off since they are scoped to the sproc
            Debug.Assert(System.Data.CommandType.Text == this.CommandType, "invalid call to GetCommandText for stored proc!");
            return GetSetOptionsString(behavior) + this.CommandText;
        }

        //
        // build the RPC record header for sp_executesql and add the parameters
        //
        // the prototype for sp_prepare is:
        // sp_prepare(@handle int OUTPUT, @batch_params ntext, @batch_text ntext, @options int default 0x1)
        private _SqlRPC BuildPrepare(CommandBehavior behavior) {
            Debug.Assert(System.Data.CommandType.Text == this.CommandType, "invalid use of sp_prepare for stored proc invocation!");

            _SqlRPC rpc = null;
            GetRPCObject(3, ref rpc);
            SqlParameter sqlParam;

            rpc.ProcID = TdsEnums.RPC_PROCID_PREPARE;
            rpc.rpcName = TdsEnums.SP_PREPARE;

            //@handle
            sqlParam = new SqlParameter(null, SqlDbType.Int);
            sqlParam.Direction = ParameterDirection.Output;
            rpc.parameters[0] = sqlParam;
            rpc.paramoptions[0] = TdsEnums.RPC_PARAM_BYREF;

            //@batch_params
            string paramList = BuildParamList(_stateObj.Parser, _parameters);
            sqlParam = new SqlParameter(null, ((paramList.Length<<1)<=TdsEnums.TYPE_SIZE_LIMIT)?SqlDbType.NVarChar:SqlDbType.NText, paramList.Length);
            sqlParam.Value = paramList;
            rpc.parameters[1] = sqlParam;

            //@batch_text
            string text = GetCommandText(behavior);
            sqlParam = new SqlParameter(null, ((text.Length<<1)<=TdsEnums.TYPE_SIZE_LIMIT)?SqlDbType.NVarChar:SqlDbType.NText, text.Length);
            sqlParam.Value = text;
            rpc.parameters[2] = sqlParam;

/*
            //@options
            sqlParam = new SqlParameter(null, SqlDbType.Int);
            rpc.Parameters[3] = sqlParam;
*/
            return rpc;
        }

        internal void CheckThrowSNIException() {
            var stateObj = _stateObj;
            if (stateObj != null) {
                stateObj.CheckThrowSNIException();
            }
        }

        // We're being notified that the underlying connection has closed
        internal void OnConnectionClosed() {
            
            var stateObj = _stateObj;
            if (stateObj != null) {
                stateObj.OnConnectionClosed();
            }
        }


        internal TdsParserStateObject StateObject {
            get {
                return _stateObj;
            }
        }

        private bool IsPrepared {
            get { return(_execType != EXECTYPE.UNPREPARED);}
        }

        private bool IsUserPrepared {
            get { return IsPrepared && !_hiddenPrepare && !IsDirty; }
        }

        internal bool IsDirty {
            get {
                // only dirty if prepared
                var activeConnection = _activeConnection;
                return (IsPrepared && 
                    (_dirty || 
                    ((_parameters != null) && (_parameters.IsDirty)) || 
                    ((activeConnection != null) && ((activeConnection.CloseCount != _preparedConnectionCloseCount) || (activeConnection.ReconnectCount != _preparedConnectionReconnectCount)))));
            }
            set {
                // only mark the command as dirty if it is already prepared
                // but always clear the value if it we are clearing the dirty flag
                _dirty = value ? IsPrepared : false;
                if (null != _parameters) {
                    _parameters.IsDirty = _dirty;
                }
                _cachedMetaData = null;
            }
        }

        /// <summary>
        /// Get or set the number of records affected by SpDescribeParameterEncryption.
        /// The below line is used only for debug asserts and not exposed publicly or impacts functionality otherwise.
        /// </summary>
        internal int RowsAffectedByDescribeParameterEncryption
        {
            get {
                return _rowsAffectedBySpDescribeParameterEncryption;
            }
            set {
                if (-1 == _rowsAffectedBySpDescribeParameterEncryption) {
                    _rowsAffectedBySpDescribeParameterEncryption = value;
                }
                else if (0 < value) {
                    _rowsAffectedBySpDescribeParameterEncryption += value;
                }
            }
        }

        internal int InternalRecordsAffected {
            get {
                return _rowsAffected;
            }
            set {
                if (-1 == _rowsAffected) {
                    _rowsAffected = value;
                }
                else if (0 < value) {
                    _rowsAffected += value;
                }
            }
        }

        internal bool BatchRPCMode {
            get {
                return _batchRPCMode;
            }
            set {
                _batchRPCMode = value;

                if (_batchRPCMode == false) {
                    ClearBatchCommand();
                } else {
                    if (_RPCList == null) {
                        _RPCList = new List<_SqlRPC>();
                    }
                    if (_parameterCollectionList == null) {
                        _parameterCollectionList = new List<SqlParameterCollection>();
                    }
                }
            }
        }

        /// <summary>
        /// Clear the state in sqlcommand related to describe parameter encryption RPC requests.
        /// </summary>
        private void ClearDescribeParameterEncryptionRequests() {
            _sqlRPCParameterEncryptionReqArray = null;
            _currentlyExecutingDescribeParameterEncryptionRPC = 0;
            _isDescribeParameterEncryptionRPCCurrentlyInProgress = false;
            _rowsAffectedBySpDescribeParameterEncryption = -1;
        }

        internal void ClearBatchCommand() {
            List<_SqlRPC> rpcList = _RPCList;
            if (null != rpcList) {
                rpcList.Clear();
            }
            if (null != _parameterCollectionList) {
                _parameterCollectionList.Clear();
            }
            _SqlRPCBatchArray = null;
            _currentlyExecutingBatch = 0;
        }

        /// <summary>
        /// Set the column encryption setting to the new one.
        /// Do not allow conflicting column encryption settings.
        /// </summary>
        private void SetColumnEncryptionSetting(SqlCommandColumnEncryptionSetting newColumnEncryptionSetting) {
            if (!this._wasBatchModeColumnEncryptionSettingSetOnce) {
                this._columnEncryptionSetting = newColumnEncryptionSetting;
                this._wasBatchModeColumnEncryptionSettingSetOnce = true;
            }
            else {
                if (this._columnEncryptionSetting != newColumnEncryptionSetting) {
                    throw SQL.BatchedUpdateColumnEncryptionSettingMismatch();
                }
            }
        }

            internal void AddBatchCommand(string commandText, SqlParameterCollection parameters, CommandType cmdType, SqlCommandColumnEncryptionSetting columnEncryptionSetting) {
            Debug.Assert(BatchRPCMode, "Command is not in batch RPC Mode");
            Debug.Assert(_RPCList != null);
            Debug.Assert(_parameterCollectionList != null);

            _SqlRPC  rpc = new _SqlRPC();

            this.CommandText = commandText;
            this.CommandType = cmdType;

            // Set the column encryption setting.
            SetColumnEncryptionSetting(columnEncryptionSetting);

            GetStateObject();
            if (cmdType == CommandType.StoredProcedure) {
                BuildRPC(false, parameters, ref rpc);
            }
            else {
                // All batch sql statements must be executed inside sp_executesql, including those without parameters
                BuildExecuteSql(CommandBehavior.Default, commandText, parameters, ref rpc);
            }
             _RPCList.Add(rpc);
             // Always add a parameters collection per RPC, even if there are no parameters.
             _parameterCollectionList.Add(parameters);

            ReliablePutStateObject();
        }

        internal int ExecuteBatchRPCCommand() {

            Debug.Assert(BatchRPCMode, "Command is not in batch RPC Mode");
            Debug.Assert(_RPCList != null, "No batch commands specified");
            _SqlRPCBatchArray = _RPCList.ToArray();
            _currentlyExecutingBatch = 0;
            return ExecuteNonQuery();       // Check permissions, execute, return output params

        }

        internal int? GetRecordsAffected(int commandIndex) {
            Debug.Assert(BatchRPCMode, "Command is not in batch RPC Mode");
            Debug.Assert(_SqlRPCBatchArray != null, "batch command have been cleared");
            return _SqlRPCBatchArray[commandIndex].recordsAffected;
        }

        internal SqlException GetErrors(int commandIndex) {
            SqlException result = null;
            int length = (_SqlRPCBatchArray[commandIndex].errorsIndexEnd - _SqlRPCBatchArray[commandIndex].errorsIndexStart);
            if (0 < length) {
                SqlErrorCollection errors = new SqlErrorCollection();
                for(int i = _SqlRPCBatchArray[commandIndex].errorsIndexStart; i < _SqlRPCBatchArray[commandIndex].errorsIndexEnd; ++i) {
                    errors.Add(_SqlRPCBatchArray[commandIndex].errors[i]);
                }
                for(int i = _SqlRPCBatchArray[commandIndex].warningsIndexStart; i < _SqlRPCBatchArray[commandIndex].warningsIndexEnd; ++i) {
                    errors.Add(_SqlRPCBatchArray[commandIndex].warnings[i]);
                }
                result = SqlException.CreateException(errors, Connection.ServerVersion, Connection.ClientConnectionId);
            }
            return result;
        }
        
        // Allocates and initializes a new SmiRequestExecutor based on the current command state
        private SmiRequestExecutor SetUpSmiRequest( SqlInternalConnectionSmi innerConnection ) {

            // General Approach To Ensure Security of Marshalling:
            //        Only touch each item in the command once
            //        (i.e. only grab a reference to each param once, only
            //        read the type from that param once, etc.).  The problem is
            //        that if the user changes something on the command in the
            //        middle of marshaling, it can overwrite the native buffers
            //        set up.  For example, if max length is used to allocate
            //        buffers, but then re-read from the parameter to truncate
            //        strings, the user could extend the length and overwrite
            //        the buffer.

            if (null != Notification){
                throw SQL.NotificationsNotAvailableOnContextConnection();
            }

            SmiParameterMetaData[] requestMetaData = null;
            ParameterPeekAheadValue[] peekAheadValues = null;

            //    Length of rgMetadata becomes *the* official count of parameters to use,
            //      don't rely on Parameters.Count after this point, as the user could change it.
            int count = GetParameterCount( Parameters );
            if ( 0 < count ) {
                requestMetaData = new SmiParameterMetaData[count];
                peekAheadValues = new ParameterPeekAheadValue[count];

                // set up the metadata
                for ( int index=0; index<count; index++ ) {
                    SqlParameter param = Parameters[index];
                    param.Validate(index, CommandType.StoredProcedure == CommandType);
                    requestMetaData[index] = param.MetaDataForSmi(out peekAheadValues[index]);

                    // Check for valid type for version negotiated
                    if (!innerConnection.IsKatmaiOrNewer) {
                        MetaType mt = MetaType.GetMetaTypeFromSqlDbType(requestMetaData[index].SqlDbType, requestMetaData[index].IsMultiValued);
                        if (!mt.Is90Supported) {
                            throw ADP.VersionDoesNotSupportDataType(mt.TypeName);
                        }
                    }
                }
            }

            // Allocate the new request
            CommandType cmdType = CommandType;
            _smiRequestContext = innerConnection.InternalContext;
            SmiRequestExecutor requestExecutor = _smiRequestContext.CreateRequestExecutor(
                                    CommandText,
                                    cmdType,
                                    requestMetaData,
                                    EventSink
                                );

            // deal with errors
            EventSink.ProcessMessagesAndThrow();

            // Now assign param values
            for ( int index=0; index<count; index++ ) {
                if ( ParameterDirection.Output != requestMetaData[index].Direction &&
                        ParameterDirection.ReturnValue != requestMetaData[index].Direction ) {
                    SqlParameter param = Parameters[index];
                    // going back to command for parameter is ok, since we'll only pick up values now.
                    object value = param.GetCoercedValue();
                    if (value is XmlDataFeed && requestMetaData[index].SqlDbType != SqlDbType.Xml) {
                        value = MetaType.GetStringFromXml(((XmlDataFeed)value)._source);
                    }
                    ExtendedClrTypeCode typeCode = MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(requestMetaData[index].SqlDbType, requestMetaData[index].IsMultiValued, value, null /* parameters don't use CLR Type for UDTs */, SmiContextFactory.Instance.NegotiatedSmiVersion);

                    // Handle null reference as special case for parameters
                    if ( CommandType.StoredProcedure == cmdType && 
                                ExtendedClrTypeCode.Empty == typeCode ) {
                        requestExecutor.SetDefault( index );
                    }
                    else {
                        // SQLBU 402391 & 403631: Exception to prevent Parameter.Size data corruption cases from working.
                        //  This should be temporary until changing to correct behavior can be safely implemented.
                        // initial size criteria is the same for all affected types
                        // NOTE: assumes size < -1 is handled by SqlParameter.Size setter
                        int size = param.Size;
                        if (size != 0 && size != SmiMetaData.UnlimitedMaxLengthIndicator && !param.SizeInferred) {
                            switch(requestMetaData[index].SqlDbType) {
                                case SqlDbType.Image:
                                case SqlDbType.Text:
                                    if (size != Int32.MaxValue) {
                                        throw SQL.ParameterSizeRestrictionFailure(index);
                                    }
                                    break;

                                case SqlDbType.NText:
                                    if (size != Int32.MaxValue/2) {
                                        throw SQL.ParameterSizeRestrictionFailure(index);
                                    }
                                    break;

                                case SqlDbType.VarBinary:
                                case SqlDbType.VarChar:
                                    // Allow size==Int32.MaxValue because of DeriveParameters
                                    if (size > 0 && size != Int32.MaxValue && requestMetaData[index].MaxLength == SmiMetaData.UnlimitedMaxLengthIndicator) {
                                        throw SQL.ParameterSizeRestrictionFailure(index);
                                    }
                                    break;

                                case SqlDbType.NVarChar:
                                    // Allow size==Int32.MaxValue/2 because of DeriveParameters
                                    if (size > 0 && size != Int32.MaxValue/2 && requestMetaData[index].MaxLength == SmiMetaData.UnlimitedMaxLengthIndicator) {
                                        throw SQL.ParameterSizeRestrictionFailure(index);
                                    }
                                    break;

                                case SqlDbType.Timestamp:
                                    // Size limiting for larger values will happen due to MaxLength
                                    if (size < SmiMetaData.DefaultTimestamp.MaxLength) {
                                        throw SQL.ParameterSizeRestrictionFailure(index);
                                    }
                                    break;

                                case SqlDbType.Variant:
                                    // Variant problems happen when Size is less than maximums for character and binary values
                                    // Size limiting for larger values will happen due to MaxLength
                                    // NOTE: assumes xml and udt types are handled in parameter value coercion
                                    //      since server does not allow these types in a variant
                                    if (null != value) {
                                        MetaType mt = MetaType.GetMetaTypeFromValue(value);

                                        if ((mt.IsNCharType && size < SmiMetaData.MaxUnicodeCharacters) ||
                                                (mt.IsBinType && size < SmiMetaData.MaxBinaryLength) ||
                                                (mt.IsAnsiType && size < SmiMetaData.MaxANSICharacters)) {
                                            throw SQL.ParameterSizeRestrictionFailure(index);
                                        }
                                    }
                                    break;

                                 case SqlDbType.Xml:
                                    // Xml is an issue for non-SqlXml types
                                    if (null != value && ExtendedClrTypeCode.SqlXml != typeCode) {
                                        throw SQL.ParameterSizeRestrictionFailure(index);
                                    }
                                    break;

                                 // NOTE: Char, NChar, Binary and UDT do not need restricting because they are always 8k or less, 
                                 //         so the metadata MaxLength will match the Size setting.

                                default:
                                    break;
                            }
                        }

                        if (innerConnection.IsKatmaiOrNewer) {
                            ValueUtilsSmi.SetCompatibleValueV200(EventSink, requestExecutor, index, requestMetaData[index], value, typeCode, param.Offset, param.Size, peekAheadValues[index]);
                        }
                        else {
                            ValueUtilsSmi.SetCompatibleValue( EventSink, requestExecutor, index, requestMetaData[index], value, typeCode, param.Offset );
                        }
                    }
                }
            }

            return requestExecutor;
        }

        private void WriteBeginExecuteEvent()
        {
            if (SqlEventSource.Log.IsEnabled() && Connection != null)
            {
                string commandText = CommandType == CommandType.StoredProcedure ? CommandText : string.Empty;
                SqlEventSource.Log.BeginExecute(GetHashCode(), Connection.DataSource, Connection.Database, commandText);
            }
        }

        /// <summary>
        /// Writes and end execute event in Event Source.
        /// </summary>
        /// <param name="success">True if SQL command finished successfully, otherwise false.</param>
        /// <param name="sqlExceptionNumber">Gets a number that identifies the type of error.</param>
        /// <param name="synchronous">True if SQL command was executed synchronously, otherwise false.</param>
        private void WriteEndExecuteEvent(bool success, int? sqlExceptionNumber, bool synchronous)
        {
            if (SqlEventSource.Log.IsEnabled())
            {
                // SqlEventSource.WriteEvent(int, int, int, int) is faster than provided overload SqlEventSource.WriteEvent(int, object[]).
                // that's why trying to fit several booleans in one integer value

                // success state is stored the first bit in compositeState 0x01
                int successFlag = success ? 1 : 0;

                // isSqlException is stored in the 2nd bit in compositeState 0x100
                int isSqlExceptionFlag = sqlExceptionNumber.HasValue ? 2 : 0;

                // synchronous state is stored in the second bit in compositeState 0x10
                int synchronousFlag = synchronous ? 4 : 0;

                int compositeState = successFlag | isSqlExceptionFlag | synchronousFlag;

                SqlEventSource.Log.EndExecute(GetHashCode(), compositeState, sqlExceptionNumber.GetValueOrDefault());
            }
        }

#if DEBUG
        internal void CompletePendingReadWithSuccess(bool resetForcePendingReadsToWait) {
            var stateObj = _stateObj;
            if (stateObj != null) {
                stateObj.CompletePendingReadWithSuccess(resetForcePendingReadsToWait);
            }
            else {
                var tempCachedAsyncState = cachedAsyncState;
                if (tempCachedAsyncState != null) {
                    var reader = tempCachedAsyncState.CachedAsyncReader;
                    if (reader != null) {
                        reader.CompletePendingReadWithSuccess(resetForcePendingReadsToWait);
                    }
                }
            }
        }

        internal void CompletePendingReadWithFailure(int errorCode, bool resetForcePendingReadsToWait) {
            var stateObj = _stateObj;
            if (stateObj != null) {
                stateObj.CompletePendingReadWithFailure(errorCode, resetForcePendingReadsToWait);
            }
            else {
                var tempCachedAsyncState = _cachedAsyncState;
                if (tempCachedAsyncState != null) {
                    var reader = tempCachedAsyncState.CachedAsyncReader;
                    if (reader != null) {
                        reader.CompletePendingReadWithFailure(errorCode, resetForcePendingReadsToWait);
                    }
                }
            }
        }
#endif
    }
}


