//------------------------------------------------------------------------------
// <copyright file="SqlInternalConnectionSmi.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using Microsoft.SqlServer.Server;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using SysTx = System.Transactions;

    sealed internal class SqlInternalConnectionSmi : SqlInternalConnection {

        private SmiContext             _smiContext;
        private SmiConnection          _smiConnection;
        private SmiEventSink_Default   _smiEventSink;
        private int                    _isInUse;            // 1 = Connected to open outer connection, 0 = not connected

        private SqlInternalTransaction _pendingTransaction; // transaction awaiting event signalling that it is active
        private SqlInternalTransaction _currentTransaction; // currently active non-context transaction.

        sealed private class EventSink : SmiEventSink_Default {

            SqlInternalConnectionSmi _connection;

            override internal string ServerVersion {
                get {
                    return SmiContextFactory.Instance.ServerVersion;
                }
            }

            override protected void DispatchMessages(bool ignoreNonFatalMessages) {
                // Override this on the Connection event sink, since we can deal
                // with info messages here.
                SqlException exception = ProcessMessages(false, ignoreNonFatalMessages);

                if (null != exception) {
                    // SQLBUVSTS 225982, query for connection once to avoid race condition between GC (that may collect the connection) and the user thread
                    SqlConnection connection = _connection.Connection;
                    if (null != connection && connection.FireInfoMessageEventOnUserErrors) {
                        connection.OnInfoMessage(new SqlInfoMessageEventArgs(exception));
                    }
                    else {
                        _connection.OnError(exception, false);    // we can't really ever break the direct connection, can we?
                    }
                }
            }

            internal EventSink(SqlInternalConnectionSmi connection) {
                Debug.Assert(null != connection, "null connection?");
                _connection = connection;
            }

            internal override void DefaultDatabaseChanged( string databaseName ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.DefaultDatabaseChanged|ADV> %d#, databaseName='%ls'.\n", _connection.ObjectID, databaseName);
                }
                _connection.CurrentDatabase = databaseName;
            }

            internal override void TransactionCommitted( long transactionId ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionCommitted|ADV> %d#, transactionId=0x%I64x.\n", _connection.ObjectID, transactionId);
                }
                _connection.TransactionEnded(transactionId, TransactionState.Committed);
            }

            internal override void TransactionDefected( long transactionId ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionDefected|ADV> %d#, transactionId=0x%I64x.\n", _connection.ObjectID, transactionId);
                }
                _connection.TransactionEnded(transactionId, TransactionState.Unknown);
            }

            internal override void TransactionEnlisted( long transactionId ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionEnlisted|ADV> %d#, transactionId=0x%I64x.\n", _connection.ObjectID, transactionId);
                }
                _connection.TransactionStarted(transactionId, true); // distributed;
            }

            internal override void TransactionEnded( long transactionId ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionEnded|ADV> %d#, transactionId=0x%I64x.\n", _connection.ObjectID, transactionId);
                }
                _connection.TransactionEndedByServer(transactionId, TransactionState.Unknown);
            }

            internal override void TransactionRolledBack( long transactionId ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionRolledBack|ADV> %d#, transactionId=0x%I64x.\n", _connection.ObjectID, transactionId);
                }

                // Dev11 1066: ensure delegated transaction is rolled back
                _connection.TransactionEndedByServer(transactionId, TransactionState.Aborted);
            }

            internal override void TransactionStarted( long transactionId ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.EventSink.TransactionStarted|ADV> %d#, transactionId=0x%I64x.\n", _connection.ObjectID, transactionId);
                }
                _connection.TransactionStarted(transactionId, false); // not distributed;
            }
        }

        internal SqlInternalConnectionSmi(SqlConnectionString connectionOptions, SmiContext smiContext) : base(connectionOptions) {
            Debug.Assert(null != smiContext, "null smiContext?");

            _smiContext = smiContext;
            _smiContext.OutOfScope += new EventHandler(OnOutOfScope);

            _smiConnection = _smiContext.ContextConnection;
            Debug.Assert(null != _smiConnection, "null SmiContext.ContextConnection?");

            _smiEventSink = new EventSink(this);

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.ctor|ADV> %d#, constructed new SMI internal connection\n", ObjectID);
            }
        }

        internal SmiContext InternalContext {
            get {
                return _smiContext;
            }
        }

        internal SmiConnection SmiConnection {
            get {
                return _smiConnection;
            }
        }

        internal SmiEventSink CurrentEventSink {
            get {
                return _smiEventSink;
            }
        }

        override internal SqlInternalTransaction CurrentTransaction {
            get {
                return _currentTransaction;
            }
        }

        override internal bool IsLockedForBulkCopy {
            get {
                return false;   // no bulk copy in the Direct connection case.
            }
        }

        override internal bool IsShiloh {
            get {
                return false;   // Can't be direct connecting to Shiloh.
            }
        }

        override internal bool IsYukonOrNewer {
            get {
                return true;    // Must be direct connecting to Yukon or newer.
            }
        }

        override internal bool IsKatmaiOrNewer {
            get {
                return SmiContextFactory.Instance.NegotiatedSmiVersion >= SmiContextFactory.KatmaiVersion;
            }
        }

        override internal SqlInternalTransaction PendingTransaction {
            get {
                return CurrentTransaction; // there are no differences between pending and current in proc.
            }
        }

        override public string ServerVersion {
            get {
                return SmiContextFactory.Instance.ServerVersion;
            }
        }

        /// <summary>
        /// Get boolean that specifies whether an enlisted transaction can be unbound from 
        /// the connection when that transaction completes.
        /// </summary>
        /// <value>
        /// True if the connection string property "TransactionBinding" is set to TransactionBindingEnum.ImplicitUnbind;
        /// otherwise, false.
        /// </value>
        protected override bool UnbindOnTransactionCompletion
        {
            get
            {
                return ConnectionOptions.TransactionBinding == SqlConnectionString.TransactionBindingEnum.ImplicitUnbind;
            }
        }

        // Workaround to access context transaction without rewriting connection pool & internalconnections properly.
        // Context transactions SHOULD be considered enlisted.
        //   This works for now only because we can't unenlist from the context transaction
        // DON'T START USING THIS ANYWHERE EXCEPT IN InternalTransaction and in InternalConnectionSmi!!!
        private SysTx.Transaction ContextTransaction
        {
            get;
            set;
        }

        private SysTx.Transaction InternalEnlistedTransaction
        {
            get
            {
                // Workaround to access context transaction without rewriting connection pool & internalconnections properly.
                // This SHOULD be a simple wrapper around EnlistedTransaction.
                //   This works for now only because we can't unenlist from the context transaction
                SysTx.Transaction tx = EnlistedTransaction;

                if (null == tx)
                {
                    tx = ContextTransaction;
                }

                return tx;
            }
        }

        override protected void Activate(SysTx.Transaction transaction)
        {
            Debug.Assert(false, "Activating an internal SMI connection?"); // we should never be activating, because that would indicate we're being pooled.
        }

        internal void Activate() {
            int wasInUse = System.Threading.Interlocked.Exchange(ref _isInUse, 1);
            if (0 != wasInUse) {
                throw SQL.ContextConnectionIsInUse();
            }

            CurrentDatabase = _smiConnection.GetCurrentDatabase(_smiEventSink);
            
            _smiEventSink.ProcessMessagesAndThrow();
        }
        
        internal void AutomaticEnlistment() {
            SysTx.Transaction currentSystemTransaction = ADP.GetCurrentTransaction();      // NOTE: Must be first to ensure _smiContext.ContextTransaction is set!
            SysTx.Transaction contextTransaction       = _smiContext.ContextTransaction; // returns the transaction that was handed to SysTx that wraps the ContextTransactionId.
            long              contextTransactionId     = _smiContext.ContextTransactionId;

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, contextTransactionId=0x%I64x, contextTransaction=%d#, currentSystemTransaction=%d#.\n", 
                                base.ObjectID, 
                                contextTransactionId, 
                                (null != contextTransaction) ? contextTransaction.GetHashCode() : 0, 
                                (null != currentSystemTransaction) ? currentSystemTransaction.GetHashCode() : 0);
            }

            if (SqlInternalTransaction.NullTransactionId != contextTransactionId) {
                if (null != currentSystemTransaction && contextTransaction != currentSystemTransaction) {
                    throw SQL.NestedTransactionScopesNotSupported();    // can't use TransactionScope(RequiresNew) inside a Sql Transaction.
                }
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, using context transaction with transactionId=0x%I64x\n", base.ObjectID, contextTransactionId);
                }
                _currentTransaction = new SqlInternalTransaction(this, TransactionType.Context, null, contextTransactionId);
                ContextTransaction = contextTransaction;
            }
            else if (null == currentSystemTransaction) {
                _currentTransaction = null;  // there really isn't a transaction.

                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, no transaction.\n", base.ObjectID);
                }
            }
            else {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.AutomaticEnlistment|ADV> %d#, using current System.Transaction.\n", base.ObjectID);
                }
                base.Enlist(currentSystemTransaction);
            }
        }

        override protected void ChangeDatabaseInternal(string database) {
            _smiConnection.SetCurrentDatabase(database, _smiEventSink);
            _smiEventSink.ProcessMessagesAndThrow();
        }

        override protected void InternalDeactivate() {
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.Deactivate|ADV> %d#, Deactivating.\n", base.ObjectID);
            }

            // When we put this to bed, we should not hold on to the transaction
            // or any activity (commit/rollback) may cause it to stop responding.
            if (!IsNonPoolableTransactionRoot) {
                base.Enlist(null);
            }

            if (null != _currentTransaction) {
                if (_currentTransaction.IsContext) {
                    _currentTransaction = null;
                }
                else if (_currentTransaction.IsLocal) {
                    _currentTransaction.CloseFromConnection();
                }
            }

            ContextTransaction = null;

            _isInUse = 0;  // don't need compare-exchange.
        }
        
        override internal void DelegatedTransactionEnded() {
            base.DelegatedTransactionEnded();
            
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.DelegatedTransactionEnded|ADV> %d#, cleaning up after Delegated Transaction Completion\n", base.ObjectID);
            }

            _currentTransaction = null;           // clean up our current transaction too
        }

        override internal void DisconnectTransaction(SqlInternalTransaction internalTransaction) {
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.DisconnectTransaction|ADV> %d#, Disconnecting Transaction %d#.\n", base.ObjectID, internalTransaction.ObjectID);
            }

            // VSTS 215465/15029: allow _currentTransaction to be null - it can be cleared before by server's callback
            Debug.Assert(_currentTransaction == null || _currentTransaction == internalTransaction, "disconnecting different transaction");

            if (_currentTransaction != null && _currentTransaction == internalTransaction) {
                _currentTransaction = null;
            }
        }

        override public void Dispose() {
            _smiContext.OutOfScope -= new EventHandler(OnOutOfScope);
            base.Dispose();
        }

        override internal void ExecuteTransaction(
                    TransactionRequest      transactionRequest, 
                    string                  transactionName, 
                    IsolationLevel          iso, 
                    SqlInternalTransaction  internalTransaction, 
                    bool                    isDelegateControlRequest) {
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.ExecuteTransaction|ADV> %d#, transactionRequest=%ls, transactionName='%ls', isolationLevel=%ls, internalTransaction=#%d transactionId=0x%I64x.\n", 
                                                        base.ObjectID, 
                                                        transactionRequest.ToString(), 
                                                        (null != transactionName) ? transactionName : "null", 
                                                        iso.ToString(), 
                                                        (null != internalTransaction) ? internalTransaction.ObjectID : 0,
                                                        (null != internalTransaction) ? internalTransaction.TransactionId : SqlInternalTransaction.NullTransactionId
                                                        );
            }
            switch (transactionRequest) {
                case TransactionRequest.Begin:
                    try {
                        _pendingTransaction = internalTransaction; // store this for the time being.

                        _smiConnection.BeginTransaction(transactionName, iso, _smiEventSink);
                    }
                    finally {
                        _pendingTransaction = null;
                    }
                    
                    Debug.Assert(_smiEventSink.HasMessages || null != _currentTransaction, "begin transaction without TransactionStarted event?");
                    break;

                case TransactionRequest.Commit:
                    Debug.Assert(null != _currentTransaction, "commit transaction without TransactionStarted event?");
                    
                    _smiConnection.CommitTransaction(_currentTransaction.TransactionId, _smiEventSink);
                    break;

                case TransactionRequest.Promote:
                    Debug.Assert(null != _currentTransaction, "promote transaction without TransactionStarted event?");
                    PromotedDTCToken = _smiConnection.PromoteTransaction(_currentTransaction.TransactionId, _smiEventSink);
                    break;

                case TransactionRequest.Rollback:
                case TransactionRequest.IfRollback:
                    Debug.Assert(null != _currentTransaction, "rollback/ifrollback transaction without TransactionStarted event?");
                    _smiConnection.RollbackTransaction(_currentTransaction.TransactionId, transactionName, _smiEventSink);
                    break;

                case TransactionRequest.Save:
                    Debug.Assert(null != _currentTransaction, "save transaction without TransactionStarted event?");
                    _smiConnection.CreateTransactionSavePoint(_currentTransaction.TransactionId, transactionName, _smiEventSink);
                    break;

                default:
                    Debug.Assert (false, "unhandled case for TransactionRequest");
                    break;
            }

            _smiEventSink.ProcessMessagesAndThrow();
        }

        override protected byte[] GetDTCAddress() {
            byte[] whereAbouts = _smiConnection.GetDTCAddress(_smiEventSink);     // might want to store this on the SmiLink because it doesn't change, but we want to be compatible with TDS which doesn't have a link yet.

            _smiEventSink.ProcessMessagesAndThrow();

            if (Bid.AdvancedOn) {
                if (null != whereAbouts) {
                    Bid.TraceBin("<sc.SqlInternalConnectionSmi.GetDTCAddress|ADV> whereAbouts", whereAbouts, (UInt16)whereAbouts.Length);
                }
                else {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.GetDTCAddress|ADV> whereAbouts=null\n");
                }
            }
            return whereAbouts;
        }

        internal void GetCurrentTransactionPair(out long transactionId, out SysTx.Transaction transaction) {
            // SQLBU 214740: Transaction state could change between obtaining tranid and transaction
            //  due to background SqlDelegatedTransaction processing. Lock the connection to prevent that.
            lock (this) {
                transactionId = (null != CurrentTransaction) ? CurrentTransaction.TransactionId : 0;
                transaction = null;
                if (0 != transactionId) {
                    transaction = InternalEnlistedTransaction;
                }
            }
        }

        private void OnOutOfScope(object s, EventArgs e) {
            // Called whenever the context goes out of scope, we need to make
            // sure that we close the connection, or the next person that uses
            // the context may appear to have the connection in use.
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.SqlInternalConnectionSmi.OutOfScope|ADV> %d# context is out of scope\n", base.ObjectID);
            }

            // 
            DelegatedTransaction = null;     // we don't want to hold this over to the next usage; it will automatically be reused as the context transaction...
            
            DbConnection owningObject = (DbConnection)Owner;

            try {
                if (null != owningObject && 1 == _isInUse) {
                        // SQLBU 369953
                        //  for various reasons, the owning object may no longer be connection to this
                        //  so call close on the owner, rather than trying to bypass to use internal close logic.
                        owningObject.Close();
                    }
            }
            finally {
                // Now make sure this object is not left in an in-use state
                // this is safe, because no user code should be accessing the connection by this time
                ContextTransaction = null;
                _isInUse = 0;
            }
        }

        override protected void PropagateTransactionCookie(byte[] transactionCookie) {
            if (Bid.AdvancedOn) {
                if (null != transactionCookie) {
                    Bid.TraceBin("<sc.SqlInternalConnectionSmi.PropagateTransactionCookie|ADV> transactionCookie", transactionCookie, (UInt16)transactionCookie.Length);
                }
                else {
                    Bid.Trace("<sc.SqlInternalConnectionSmi.PropagateTransactionCookie|ADV> null\n");
                }
            }

            // Propagate the transaction cookie to the server
            _smiConnection.EnlistTransaction(transactionCookie, _smiEventSink);
            _smiEventSink.ProcessMessagesAndThrow();
        }

        private void TransactionEndedByServer(long transactionId, TransactionState transactionState) {
            // Some extra steps required when the server initiates the ending of a transaction unilaterally
            //  as opposed to the client initiating it.
            //  Basically, we have to make the delegated transaction (if there is one) aware of the situation.

            SqlDelegatedTransaction delegatedTransaction = DelegatedTransaction;
            if (null != delegatedTransaction) {
                delegatedTransaction.Transaction.Rollback();    // just to make sure...
                DelegatedTransaction = null;   // He's dead, Jim.
            }

            // Now handle the standard transaction-ended stuff.
            TransactionEnded(transactionId, transactionState);
        }

        private void TransactionEnded(long transactionId, TransactionState transactionState) {
            // When we get notification of a completed transaction
            // we null out the current transaction.

            if (null != _currentTransaction) {
#if DEBUG
                // Check null for case where Begin and Rollback obtained in the same message.
                if (0 != _currentTransaction.TransactionId) {
                    Debug.Assert(_currentTransaction.TransactionId == transactionId, "transaction id's are not equal!");
                }
#endif
                _currentTransaction.Completed(transactionState);
                _currentTransaction = null;
            }
        }

        private void TransactionStarted(long transactionId, bool isDistributed) {
            // When we get notification from the server of a new
            // transaction, we move any pending transaction over to
            // the current transaction, then we store the token in it.
            // if there isn't a pending transaction, then it's either
            // a TSQL transaction or a distributed transaction.
            Debug.Assert(null == _currentTransaction, "non-null current transaction with an env change");
            _currentTransaction = _pendingTransaction;
            _pendingTransaction = null;

            if (null != _currentTransaction) {
                _currentTransaction.TransactionId = transactionId;   // this is defined as a ULongLong in the server and in the TDS Spec.
            }
            else {
                TransactionType transactionType = (isDistributed) ? TransactionType.Distributed : TransactionType.LocalFromTSQL;
                _currentTransaction = new SqlInternalTransaction(this, transactionType, null, transactionId);
            }
            _currentTransaction.Activate(); // SQLBUDT #376531 -- ensure this is activated to prevent asserts later.
        }

        override internal void ValidateConnectionForExecute(SqlCommand command) {
            SqlDataReader reader = FindLiveReader(null);
            if (null != reader) {
                // if MARS is on, then a datareader associated with the command exists
                // or if MARS is off, then a datareader exists
                throw ADP.OpenReaderExists(); // MDAC 66411
            }
        }
    }
}

