//------------------------------------------------------------------------------
// <copyright file="TdsParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using MSS = Microsoft.SqlServer.Server;

    // The TdsParser Object controls reading/writing to the netlib, parsing the tds,
    // and surfacing objects to the user.
    sealed internal class TdsParser {
        private static int _objectTypeCount; // Bid counter
        internal readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        static Task completedTask;
        static Task CompletedTask {
            get {
                if (completedTask == null) {
                    completedTask = Task.FromResult<object>(null);
                }
                return completedTask;
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }


        // ReliabilitySection Usage:
        //
        // #if DEBUG
        //        TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();
        //
        //        RuntimeHelpers.PrepareConstrainedRegions();
        //        try {
        //            tdsReliabilitySection.Start();
        // #else
        //        {
        // #endif //DEBUG
        //
        //        // code that requires reliability
        //
        //        }
        // #if DEBUG
        //        finally {
        //            tdsReliabilitySection.Stop();
        //        }
        //  #endif //DEBUG

        internal struct ReliabilitySection {
#if DEBUG
            // do not allocate TLS data in RETAIL bits
            [ThreadStatic]
            private static int s_reliabilityCount; // initialized to 0 by CLR

            private bool m_started;  // initialized to false (not started) by CLR
#endif //DEBUG

            [Conditional("DEBUG")]
            internal void Start() {
#if DEBUG
                Debug.Assert(!m_started);
                
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                }
                finally {
                    ++s_reliabilityCount;
                    m_started = true;
                }
#endif //DEBUG
            }

            [Conditional("DEBUG")]
            internal void Stop() {
#if DEBUG
                // cannot assert m_started - ThreadAbortException can be raised before Start is called
                
                if (m_started) {
                    Debug.Assert(s_reliabilityCount > 0);
                    
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try {
                    }
                    finally {
                        --s_reliabilityCount;
                        m_started = false;
                    }
                }
#endif //DEBUG
            }

            // you need to setup for a thread abort somewhere before you call this method
            [Conditional("DEBUG")]
            internal static void Assert(string message) {
#if DEBUG
                Debug.Assert(s_reliabilityCount > 0, message);
#endif //DEBUG
            }
        }

        // Default state object for parser
        internal TdsParserStateObject _physicalStateObj = null; // Default stateObj and connection for Dbnetlib and non-MARS SNI.

        // Also, default logical stateObj and connection for MARS over SNI.
        internal TdsParserStateObject _pMarsPhysicalConObj = null; // With MARS enabled, cached physical stateObj and connection.

        // Must keep this around - especially for callbacks on pre-MARS
        // ReadAsync which will return if physical connection broken!
        //
        // Per Instance TDS Parser variables
        //

        // Constants
        const int constBinBufferSize = 4096; // Size of the buffer used to read input parameter of type Stream
        const int constTextBufferSize = 4096; // Size of the buffer (in chars) user to read input parameter of type TextReader

        // State variables
        internal TdsParserState _state = TdsParserState.Closed; // status flag for connection

        private string _server = "";                            // name of server that the parser connects to

        internal volatile bool _fResetConnection = false;                 // flag to denote whether we are needing to call sp_reset
        internal volatile bool _fPreserveTransaction = false;             // flag to denote whether we need to preserve the transaction when reseting

        private SqlCollation   _defaultCollation;                         // default collation from the server

        private int _defaultCodePage;

        private int _defaultLCID;

        internal Encoding _defaultEncoding = null;                  // for sql character data

        private static EncryptionOptions    _sniSupportedEncryptionOption = SNILoadHandle.SingletonInstance.Options;

        private EncryptionOptions           _encryptionOption             = _sniSupportedEncryptionOption;

        private SqlInternalTransaction      _currentTransaction;
        private SqlInternalTransaction      _pendingTransaction;    // pending transaction for Yukon and beyond.
        // SQLHOT 483
        //  need to hold on to the transaction id if distributed transaction merely rolls back without defecting.
        private long                       _retainedTransactionId        = SqlInternalTransaction.NullTransactionId;

        // This counter is used for the entire connection to track the open result count for all
        // operations not under a transaction.
        private int _nonTransactedOpenResultCount = 0;

        // Connection reference
        private SqlInternalConnectionTds    _connHandler;

        // Async/Mars variables
        private bool _fMARS = false;

        internal bool _loginWithFailover = false; // set to true while connect in failover mode so parser state object can adjust its logic

        internal AutoResetEvent         _resetConnectionEvent = null;  // Used to serialize executes and call reset on first execute only.

        internal TdsParserSessionPool _sessionPool = null;  // initialized only when we're a MARS parser.

        // Version variables
        private bool _isShiloh = false; // set to true if we connect to a 8.0 server (SQL 2000) or later

        private bool _isShilohSP1 = false; // set to true if speaking to Shiloh SP1 or later

        private bool _isYukon = false; // set to true if speaking to Yukon or later

        private bool _isKatmai = false;

        private bool _isDenali = false;

        private byte[] _sniSpnBuffer = null;

        // 
        
        // SqlStatistics
        private SqlStatistics _statistics = null;

        private bool _statisticsIsInTransaction = false;

        //
        // STATIC TDS Parser variables
        //
        
        // NIC address caching
        private static byte[] s_nicAddress;             // cache the NIC address from the registry

        // SSPI variables
        private static bool s_fSSPILoaded = false; // bool to indicate whether library has been loaded

        private volatile static UInt32 s_maxSSPILength = 0;     // variable to hold max SSPI data size, keep for token from server

        // ADAL variables
        private static bool s_fADALLoaded = false; // bool to indicate whether ADAL library has been loaded

        // textptr sequence
        private static readonly byte[] s_longDataHeader = { 0x10, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };

        private static object s_tdsParserLock = new object();

        // Various other statics
        private const int ATTENTION_TIMEOUT = 5000;  // internal attention timeout, in ticks

        // XML metadata substitue sequence
        private static readonly byte[] s_xmlMetadataSubstituteSequence = { 0xe7, 0xff, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00 };

        // size of Guid  (e.g. _clientConnectionId, ActivityId.Id)
        private const int GUID_SIZE = 16;

        // NOTE: You must take the internal connection's _parserLock before modifying this
        internal bool _asyncWrite = false;

        // TCE supported flag, used to determine if new TDS fields are present. This is 
        // useful when talking to downlevel/uplevel server.
        private bool _serverSupportsColumnEncryption = false;

        /// <summary>
        /// Get or set if column encryption is supported by the server.
        /// </summary>
        internal bool IsColumnEncryptionSupported {
            get {
                return _serverSupportsColumnEncryption;
            }
            set {
                _serverSupportsColumnEncryption = value;
            }
        }

        internal TdsParser(bool MARS, bool fAsynchronous) {
            _fMARS = MARS; // may change during Connect to pre Yukon servers
            _physicalStateObj = new TdsParserStateObject(this);
        }

        internal SqlInternalConnectionTds Connection {
            get {
                return _connHandler;
            }
        }
      
        internal SqlInternalTransaction CurrentTransaction {
            get {
                return _currentTransaction;
            }
            set {
                Debug.Assert(value == _currentTransaction
                          || null == _currentTransaction
                          || null == value
                          || (null != _currentTransaction && !_currentTransaction.IsLocal), "attempting to change current transaction?");

                // If there is currently a transaction active, we don't want to
                // change it; this can occur when there is a delegated transaction
                // and the user attempts to do an API begin transaction; in these
                // cases, it's safe to ignore the set.
                if ((null == _currentTransaction && null != value)
                  ||(null != _currentTransaction && null == value)) {
                    _currentTransaction = value;
                }
            }
        }

        internal int DefaultLCID {
            get {
                return _defaultLCID;
            }
        }

        internal EncryptionOptions EncryptionOptions {
            get {
                return _encryptionOption;
            }
            set {
                _encryptionOption = value;
            }
        }
        
        internal bool IsYukonOrNewer {
            get {
                return _isYukon;
            }
        }

        internal bool IsKatmaiOrNewer {
            get {
                return _isKatmai;
            }
        }

        internal bool MARSOn {
            get {
                return _fMARS;
            }
        }

        internal SqlInternalTransaction PendingTransaction {
            get {
                return _pendingTransaction;
            }
            set {
                Debug.Assert (null != value, "setting a non-null PendingTransaction?");
                _pendingTransaction = value;
            }
        }

        internal string Server {
            get {
                return _server;
            }
        }

        internal TdsParserState State {
            get {
                return _state;
            }
            set {
                _state = value;
            }
        }

        internal SqlStatistics Statistics {
            get {
                return _statistics;
            }
            set {
                _statistics = value;
            }
        }

        private bool IncludeTraceHeader {
            get {
                return (_isDenali && Bid.TraceOn && Bid.IsOn(ActivityCorrelator.CorrelationTracePoints));
            }

        }


        internal int IncrementNonTransactedOpenResultCount() {
            // IMPORTANT - this increments the connection wide open result count for all
            // operations not under a transaction!  Do not call if you intend to modify the 
            // count for a transaction!
            Debug.Assert(_nonTransactedOpenResultCount >= 0, "Unexpected result count state");
            int result = Interlocked.Increment(ref _nonTransactedOpenResultCount);
            return result;
        }

        internal void DecrementNonTransactedOpenResultCount() {
            // IMPORTANT - this decrements the connection wide open result count for all
            // operations not under a transaction!  Do not call if you intend to modify the 
            // count for a transaction!
            Interlocked.Decrement(ref _nonTransactedOpenResultCount);
            Debug.Assert(_nonTransactedOpenResultCount >= 0, "Unexpected result count state");
        }

        internal void ProcessPendingAck(TdsParserStateObject stateObj) {
            if (stateObj._attentionSent) {
                ProcessAttention(stateObj);
            }
        }

        internal void Connect(ServerInfo serverInfo,
                              SqlInternalConnectionTds connHandler,
                              bool ignoreSniOpenTimeout,
                              long timerExpire,
                              bool encrypt,
                              bool trustServerCert,
                              bool integratedSecurity,
                              bool withFailover,
                              bool isFirstTransparentAttempt,
                              SqlAuthenticationMethod authType,
                              bool disableTnir) {
            if (_state != TdsParserState.Closed) {
                Debug.Assert(false, "TdsParser.Connect called on non-closed connection!");
                return;
            }

            _connHandler = connHandler;
            _loginWithFailover = withFailover;

            UInt32 sniStatus = SNILoadHandle.SingletonInstance.SNIStatus;
            if (sniStatus != TdsEnums.SNI_SUCCESS) {
                _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                _physicalStateObj.Dispose();
                ThrowExceptionAndWarning(_physicalStateObj);
                Debug.Assert(false, "SNI returned status != success, but no error thrown?");
            }

            //Create LocalDB instance if necessary             
            if (connHandler.ConnectionOptions.LocalDBInstance != null)
                LocalDBAPI.CreateLocalDBInstance(connHandler.ConnectionOptions.LocalDBInstance);

            if (integratedSecurity || authType == SqlAuthenticationMethod.ActiveDirectoryIntegrated) {
                LoadSSPILibrary();
                // now allocate proper length of buffer
                _sniSpnBuffer = new byte[SNINativeMethodWrapper.SniMaxComposedSpnLength];
                Bid.Trace("<sc.TdsParser.Connect|SEC> SSPI or Active Directory Authentication Library for SQL Server based integrated authentication\n");
            }
            else {
                _sniSpnBuffer = null;
                if (authType == SqlAuthenticationMethod.ActiveDirectoryPassword) {
                    Bid.Trace("<sc.TdsParser.Connect|SEC> Active Directory Password authentication\n");
                }
                else if (authType == SqlAuthenticationMethod.SqlPassword) {
                    Bid.Trace("<sc.TdsParser.Connect|SEC> SQL Password authentication\n");
                }
                else{
                    Bid.Trace("<sc.TdsParser.Connect|SEC> SQL authentication\n");
                }
            }

            byte[] instanceName = null;

            Debug.Assert(_connHandler != null, "SqlConnectionInternalTds handler can not be null at this point.");
            _connHandler.TimeoutErrorInternal.EndPhase(SqlConnectionTimeoutErrorPhase.PreLoginBegin);
            _connHandler.TimeoutErrorInternal.SetAndBeginPhase(SqlConnectionTimeoutErrorPhase.InitializeConnection);

            bool fParallel = _connHandler.ConnectionOptions.MultiSubnetFailover;

            TransparentNetworkResolutionState transparentNetworkResolutionState;
            if (_connHandler.ConnectionOptions.TransparentNetworkIPResolution && !disableTnir)
            {
                if(isFirstTransparentAttempt)
                    transparentNetworkResolutionState = TransparentNetworkResolutionState.SequentialMode;
                else
                    transparentNetworkResolutionState = TransparentNetworkResolutionState.ParallelMode;
            }
            else 
                transparentNetworkResolutionState = TransparentNetworkResolutionState.DisabledMode;

            int  totalTimeout = _connHandler.ConnectionOptions.ConnectTimeout;

            _physicalStateObj.CreatePhysicalSNIHandle(serverInfo.ExtendedServerName, ignoreSniOpenTimeout, timerExpire,
                        out instanceName, _sniSpnBuffer, false, true, fParallel, transparentNetworkResolutionState, totalTimeout);

            if (TdsEnums.SNI_SUCCESS != _physicalStateObj.Status) {
                _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));

                // Since connect failed, free the unmanaged connection memory.
                // HOWEVER - only free this after the netlib error was processed - if you
                // don't, the memory for the connection object might not be accurate and thus
                // a bad error could be returned (as it was when it was freed to early for me).
                _physicalStateObj.Dispose();
                Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Login failure\n");
                ThrowExceptionAndWarning(_physicalStateObj);
                Debug.Assert(false, "SNI returned status != success, but no error thrown?");
            }

            _server = serverInfo.ResolvedServerName;

            if (null != connHandler.PoolGroupProviderInfo) {
                // If we are pooling, check to see if we were processing an
                // alias which has changed, which means we need to clean out
                // the pool. See Webdata 104293.
                // This should not apply to routing, as it is not an alias change, routed connection 
                // should still use VNN of AlwaysOn cluster as server for pooling purposes.
                connHandler.PoolGroupProviderInfo.AliasCheck(serverInfo.PreRoutingServerName==null ?
                    serverInfo.ResolvedServerName: serverInfo.PreRoutingServerName);
            }
            _state = TdsParserState.OpenNotLoggedIn;
            _physicalStateObj.SniContext = SniContext.Snix_PreLoginBeforeSuccessfullWrite; // SQL BU DT 376766
            _physicalStateObj.TimeoutTime = timerExpire;

            bool marsCapable = false;

            _connHandler.TimeoutErrorInternal.EndPhase(SqlConnectionTimeoutErrorPhase.InitializeConnection);
            _connHandler.TimeoutErrorInternal.SetAndBeginPhase(SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake);

            UInt32 result = SNINativeMethodWrapper.SniGetConnectionId(_physicalStateObj.Handle, ref _connHandler._clientConnectionId);
            Debug.Assert(result == TdsEnums.SNI_SUCCESS, "Unexpected failure state upon calling SniGetConnectionId");

            // 
            Bid.Trace("<sc.TdsParser.Connect|SEC> Sending prelogin handshake\n");
            SendPreLoginHandshake(instanceName, encrypt);

            _connHandler.TimeoutErrorInternal.EndPhase(SqlConnectionTimeoutErrorPhase.SendPreLoginHandshake);
            _connHandler.TimeoutErrorInternal.SetAndBeginPhase(SqlConnectionTimeoutErrorPhase.ConsumePreLoginHandshake);

            _physicalStateObj.SniContext = SniContext.Snix_PreLogin;

            Bid.Trace("<sc.TdsParser.Connect|SEC> Consuming prelogin handshake\n");
            PreLoginHandshakeStatus status = ConsumePreLoginHandshake(authType, encrypt, trustServerCert, integratedSecurity, out marsCapable,
                                                                      out _connHandler._fedAuthRequired);

            if (status == PreLoginHandshakeStatus.InstanceFailure) {
                Bid.Trace("<sc.TdsParser.Connect|SEC> Prelogin handshake unsuccessful. Reattempting prelogin handshake\n");
                _physicalStateObj.Dispose(); // Close previous connection

                // On Instance failure re-connect and flush SNI named instance cache.
                _physicalStateObj.SniContext=SniContext.Snix_Connect;
                _physicalStateObj.CreatePhysicalSNIHandle(serverInfo.ExtendedServerName, ignoreSniOpenTimeout, timerExpire, out instanceName, _sniSpnBuffer, true, true, fParallel, transparentNetworkResolutionState, totalTimeout);

                if (TdsEnums.SNI_SUCCESS != _physicalStateObj.Status) {
                    _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                    Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Login failure\n");
                    ThrowExceptionAndWarning(_physicalStateObj);
                }

                UInt32 retCode = SNINativeMethodWrapper.SniGetConnectionId(_physicalStateObj.Handle, ref _connHandler._clientConnectionId);
                Debug.Assert(retCode == TdsEnums.SNI_SUCCESS, "Unexpected failure state upon calling SniGetConnectionId");
                
                Bid.Trace("<sc.TdsParser.Connect|SEC> Sending prelogin handshake\n");
                SendPreLoginHandshake(instanceName, encrypt);
                status = ConsumePreLoginHandshake(authType, encrypt, trustServerCert, integratedSecurity, out marsCapable,
                                                  out _connHandler._fedAuthRequired);

                // Don't need to check for Sphinx failure, since we've already consumed
                // one pre-login packet and know we are connecting to Shiloh.
                if (status == PreLoginHandshakeStatus.InstanceFailure) {
                    Bid.Trace("<sc.TdsParser.Connect|ERR|SEC> Prelogin handshake unsuccessful. Login failure\n");
                    throw SQL.InstanceFailure();
                }
            }
            Bid.Trace("<sc.TdsParser.Connect|SEC> Prelogin handshake successful\n");

            if (_fMARS && marsCapable) {
                // if user explictly disables mars or mars not supported, don't create the session pool
                _sessionPool = new TdsParserSessionPool(this);
            }
            else {
                _fMARS = false;
            }

            if (authType == SqlAuthenticationMethod.ActiveDirectoryPassword || (authType == SqlAuthenticationMethod.ActiveDirectoryIntegrated && _connHandler._fedAuthRequired)) {
                Debug.Assert(!integratedSecurity, "The legacy Integrated Security connection string option cannot be true when using Active Directory Authentication Library for SQL Server Based workflows.");

                LoadADALLibrary();
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.TdsParser.Connect|SEC> Active directory authentication.Loaded Active Directory Authentication Library for SQL Server\n");
                }
            }

            return;
        }

        internal void RemoveEncryption() {
            Debug.Assert(_encryptionOption == EncryptionOptions.LOGIN, "Invalid encryption option state");

            UInt32 error = 0;

            // Remove SSL (Encryption) SNI provider since we only wanted to encrypt login.
            error = SNINativeMethodWrapper.SNIRemoveProvider(_physicalStateObj.Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV);
            if (error != TdsEnums.SNI_SUCCESS) {
                _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                ThrowExceptionAndWarning(_physicalStateObj);
            }
            // create a new packet encryption changes the internal packet size Bug# 228403
            try {}   // EmptyTry/Finally to avoid FXCop violation
            finally {
                _physicalStateObj.ClearAllWritePackets();
            }
        }

        internal void EnableMars() {
            if (_fMARS) {
                // Cache physical stateObj and connection.
                _pMarsPhysicalConObj = _physicalStateObj;

                UInt32 error = 0;
                UInt32 info = 0;

                // Add SMUX (MARS) SNI provider.
                error = SNINativeMethodWrapper.SNIAddProvider(_pMarsPhysicalConObj.Handle, SNINativeMethodWrapper.ProviderEnum.SMUX_PROV, ref info);

                if (error != TdsEnums.SNI_SUCCESS) {
                    _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                    ThrowExceptionAndWarning(_physicalStateObj);
                }

                // HACK HACK HACK - for Async only
                // Have to post read to intialize MARS - will get callback on this when connection goes
                // down or is closed.

                IntPtr temp = IntPtr.Zero;

                RuntimeHelpers.PrepareConstrainedRegions();
                try {} finally {
                    _pMarsPhysicalConObj.IncrementPendingCallbacks();

                    error = SNINativeMethodWrapper.SNIReadAsync(_pMarsPhysicalConObj.Handle, ref temp);

                    if (temp != IntPtr.Zero) {
                        // Be sure to release packet, otherwise it will be leaked by native.
                        SNINativeMethodWrapper.SNIPacketRelease(temp);
                    }    
                }
                Debug.Assert(IntPtr.Zero == temp, "unexpected syncReadPacket without corresponding SNIPacketRelease");
                if (TdsEnums.SNI_SUCCESS_IO_PENDING != error) {
                    Debug.Assert(TdsEnums.SNI_SUCCESS != error, "Unexpected successfull read async on physical connection before enabling MARS!");
                    _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                    ThrowExceptionAndWarning(_physicalStateObj);
                }

                _physicalStateObj = CreateSession(); // Create and open default MARS stateObj and connection.
            }
        }

        internal TdsParserStateObject CreateSession() {
            TdsParserStateObject session = new TdsParserStateObject(this, (SNIHandle)_pMarsPhysicalConObj.Handle, true);
            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParser.CreateSession|ADV> %d# created session %d\n", ObjectID, session.ObjectID);
            }
            return session;
        }

        internal TdsParserStateObject GetSession(object owner) {
            TdsParserStateObject session = null;

            // 

            if (MARSOn) {
                session = _sessionPool.GetSession(owner);

                Debug.Assert(!session._pendingData, "pending data on a pooled MARS session");
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.TdsParser.GetSession|ADV> %d# getting session %d from pool\n", ObjectID, session.ObjectID);
                }
            }
            else {
                session = _physicalStateObj;
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.TdsParser.GetSession|ADV> %d# getting physical session %d\n", ObjectID, session.ObjectID);
                }
            }
            Debug.Assert(session._outputPacketNumber==1, "The packet number is expected to be 1");
            return session;
        }

        internal void PutSession(TdsParserStateObject session) {
            session.AssertStateIsClean();

            if (MARSOn) {
                // This will take care of disposing if the parser is closed
                _sessionPool.PutSession(session);
            }
            else if ((_state == TdsParserState.Closed) || (_state == TdsParserState.Broken)) {
                // Parser is closed\broken - dispose the stateObj
                Debug.Assert(session == _physicalStateObj, "MARS is off, but session to close is not the _physicalStateObj");
                _physicalStateObj.SniContext = SniContext.Snix_Close;
#if DEBUG
                _physicalStateObj.InvalidateDebugOnlyCopyOfSniContext();
#endif
                _physicalStateObj.Dispose();
            }
            else {
                // Non-MARS, and session is ok - remove its owner
                _physicalStateObj.Owner = null;
            }
        }

        // This is called from a ThreadAbort - ensure that it can be run from a CER Catch
        internal void BestEffortCleanup() {
            _state = TdsParserState.Broken;

            var stateObj = _physicalStateObj;
            if (stateObj != null) {
                var stateObjHandle = stateObj.Handle;
                if (stateObjHandle != null) {
                    stateObjHandle.Dispose();
                }
            }

            if (_fMARS) {
                var sessionPool = _sessionPool;
                if (sessionPool != null) {
                    sessionPool.BestEffortCleanup();
                }

                var marsStateObj = _pMarsPhysicalConObj;
                if (marsStateObj != null) {
                    var marsStateObjHandle = marsStateObj.Handle;
                    if (marsStateObjHandle != null) {
                        marsStateObjHandle.Dispose();
                    }
                }
            }
        }

        private void SendPreLoginHandshake(byte[] instanceName, bool encrypt) {
            // PreLoginHandshake buffer consists of:
            // 1) Standard header, with type = MT_PRELOGIN
            // 2) Consecutive 5 bytes for each option, (1 byte length, 2 byte offset, 2 byte payload length)
            // 3) Consecutive data blocks for each option

            // NOTE: packet data needs to be big endian - not the standard little endian used by
            // the rest of the parser.

            _physicalStateObj._outputMessageType = TdsEnums.MT_PRELOGIN;

            // Initialize option offset into payload buffer
            // 5 bytes for each option (1 byte length, 2 byte offset, 2 byte payload length)
            int offset = (int)PreLoginOptions.NUMOPT * 5 + 1;

            byte[] payload = new byte[(int)PreLoginOptions.NUMOPT * 5 + TdsEnums.MAX_PRELOGIN_PAYLOAD_LENGTH];
            int payloadLength = 0;

            // 


            for (int option = (int)PreLoginOptions.VERSION; option < (int)PreLoginOptions.NUMOPT; option++) {
                int optionDataSize = 0;

                // Fill in the option
                _physicalStateObj.WriteByte((byte)option);

                // Fill in the offset of the option data
                _physicalStateObj.WriteByte((byte)((offset & 0xff00) >> 8)); // send upper order byte
                _physicalStateObj.WriteByte((byte)(offset & 0x00ff)); // send lower order byte

                switch (option) {
                    case (int)PreLoginOptions.VERSION:
                        Version systemDataVersion = ADP.GetAssemblyVersion();

                        // Major and minor
                        payload[payloadLength++] = (byte)(systemDataVersion.Major & 0xff);
                        payload[payloadLength++] = (byte)(systemDataVersion.Minor & 0xff);

                        // Build (Big Endian)
                        payload[payloadLength++] = (byte)((systemDataVersion.Build & 0xff00) >> 8);
                        payload[payloadLength++] = (byte)(systemDataVersion.Build & 0xff);

                        // Sub-build (Little Endian)
                        payload[payloadLength++] = (byte)(systemDataVersion.Revision & 0xff);
                        payload[payloadLength++] = (byte)((systemDataVersion.Revision & 0xff00) >> 8);
                        offset += 6;
                        optionDataSize = 6;
                        break;

                    case (int)PreLoginOptions.ENCRYPT:
                        if (_encryptionOption == EncryptionOptions.NOT_SUP) {
                            // If OS doesn't support encryption, inform server not supported.
                            payload[payloadLength] = (byte)EncryptionOptions.NOT_SUP;
                        }
                        else {
                            // Else, inform server of user request.
                            if (encrypt) {
                                payload[payloadLength] = (byte)EncryptionOptions.ON;
                                _encryptionOption = EncryptionOptions.ON;
                            }
                            else {
                                payload[payloadLength] = (byte)EncryptionOptions.OFF;
                                _encryptionOption = EncryptionOptions.OFF;
                            }
                        }

                        payloadLength += 1;
                        offset += 1;
                        optionDataSize = 1;
                        break;

                    case (int)PreLoginOptions.INSTANCE:
                        int i = 0;

                        while (instanceName[i] != 0) {
                            payload[payloadLength] = instanceName[i];
                            payloadLength++;
                            i++;
                        }

                        payload[payloadLength] = 0; // null terminate
                        payloadLength++;
                        i++;

                        offset += i;
                        optionDataSize = i;
                        break;

                    case (int)PreLoginOptions.THREADID:
                        Int32 threadID = TdsParserStaticMethods.GetCurrentThreadIdForTdsLoginOnly();

                        payload[payloadLength++] = (byte)((0xff000000 & threadID) >> 24);
                        payload[payloadLength++] = (byte)((0x00ff0000 & threadID) >> 16);
                        payload[payloadLength++] = (byte)((0x0000ff00 & threadID) >> 8);
                        payload[payloadLength++] = (byte)(0x000000ff & threadID);
                        offset += 4;
                        optionDataSize = 4;
                        break;

                    case (int)PreLoginOptions.MARS:
                        payload[payloadLength++] = (byte)(_fMARS ? 1 : 0);
                        offset += 1;
                        optionDataSize += 1;
                        break;

                    case (int)PreLoginOptions.TRACEID:
                        byte[] connectionIdBytes = _connHandler._clientConnectionId.ToByteArray();
                        Debug.Assert(GUID_SIZE == connectionIdBytes.Length);
                        Buffer.BlockCopy(connectionIdBytes, 0, payload, payloadLength, GUID_SIZE);
                        payloadLength += GUID_SIZE;
                        offset += GUID_SIZE;
                        optionDataSize = GUID_SIZE;
                       
                        ActivityCorrelator.ActivityId actId = ActivityCorrelator.Next();
                        connectionIdBytes = actId.Id.ToByteArray();
                        Buffer.BlockCopy(connectionIdBytes, 0, payload, payloadLength, GUID_SIZE);
                        payloadLength += GUID_SIZE;
                        payload[payloadLength++] = (byte)(0x000000ff & actId.Sequence); 
                        payload[payloadLength++] = (byte)((0x0000ff00 & actId.Sequence) >> 8);
                        payload[payloadLength++] = (byte)((0x00ff0000 & actId.Sequence) >> 16);
                        payload[payloadLength++] = (byte)((0xff000000 & actId.Sequence) >> 24);
                        int actIdSize = GUID_SIZE + sizeof(UInt32);
                        offset += actIdSize;
                        optionDataSize += actIdSize;
                        Bid.Trace("<sc.TdsParser.SendPreLoginHandshake|INFO> ClientConnectionID %ls, ActivityID %ls\n", _connHandler._clientConnectionId.ToString(), actId.ToString());
                        break;

                    case (int)PreLoginOptions.FEDAUTHREQUIRED:
                        payload[payloadLength++] = 0x01;
                        offset += 1;
                        optionDataSize += 1;
                        break;

                    default:
                        Debug.Assert(false, "UNKNOWN option in SendPreLoginHandshake");
                        break;
                }

                // Write data length
                _physicalStateObj.WriteByte((byte)((optionDataSize & 0xff00) >> 8));
                _physicalStateObj.WriteByte((byte)(optionDataSize & 0x00ff));
            }

            // Write out last option - to let server know the second part of packet completed
            _physicalStateObj.WriteByte((byte)PreLoginOptions.LASTOPT);

            // Write out payload
            _physicalStateObj.WriteByteArray(payload, payloadLength, 0);

            // Flush packet
            _physicalStateObj.WritePacket(TdsEnums.HARDFLUSH);
        }

        private PreLoginHandshakeStatus ConsumePreLoginHandshake(SqlAuthenticationMethod authType, bool encrypt, bool trustServerCert, bool integratedSecurity, out bool marsCapable, out bool fedAuthRequired) {

            // Assign default values
            marsCapable = _fMARS;
            fedAuthRequired = false;

            bool isYukonOrLater = false;

            Debug.Assert(_physicalStateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
            bool result = _physicalStateObj.TryReadNetworkPacket();
            if (!result) { throw SQL.SynchronousCallMayNotPend(); }

            if (_physicalStateObj._inBytesRead == 0) {
                // If the server did not respond then something has gone wrong and we need to close the connection
                _physicalStateObj.AddError(new SqlError(0, (byte)0x00, TdsEnums.FATAL_ERROR_CLASS, _server, SQLMessage.PreloginError(), "", 0));
                _physicalStateObj.Dispose();
                ThrowExceptionAndWarning(_physicalStateObj);
            }

            if (!_physicalStateObj.TryProcessHeader()) { throw SQL.SynchronousCallMayNotPend(); }

            if (_physicalStateObj._inBytesPacket > TdsEnums.MAX_PACKET_SIZE || _physicalStateObj._inBytesPacket <= 0) {
                throw SQL.ParsingError(ParsingErrorState.CorruptedTdsStream);
            }
            byte[] payload = new byte[_physicalStateObj._inBytesPacket];

            Debug.Assert(_physicalStateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
            result = _physicalStateObj.TryReadByteArray(payload, 0, payload.Length);
            if (!result) { throw SQL.SynchronousCallMayNotPend(); }

            if (payload[0] == 0xaa) {
                // If the first byte is 0xAA, we are connecting to a 6.5 or earlier server, which
                // is not supported.  SQL BU DT 296425
                throw SQL.InvalidSQLServerVersionUnknown();
            }

            int offset = 0;
            int payloadOffset = 0;
            int payloadLength = 0;
            int option = payload[offset++];

            while (option != (byte)PreLoginOptions.LASTOPT) {
                switch (option) {
                    case (int)PreLoginOptions.VERSION:
                        payloadOffset = payload[offset++] << 8 | payload[offset++];
                        payloadLength = payload[offset++] << 8 | payload[offset++];

                        byte majorVersion = payload[payloadOffset];
                        byte minorVersion = payload[payloadOffset + 1];
                        int level = (payload[payloadOffset + 2] << 8) |
                                             payload[payloadOffset + 3];

                        isYukonOrLater = majorVersion >= 9;
                        if (!isYukonOrLater) {
                            marsCapable = false;            // If pre-Yukon, MARS not supported.
                        }

                        break;

                    case (int)PreLoginOptions.ENCRYPT:
                        payloadOffset = payload[offset++] << 8 | payload[offset++];
                        payloadLength = payload[offset++] << 8 | payload[offset++];

                        EncryptionOptions serverOption = (EncryptionOptions)payload[payloadOffset];

                        /* internal enum EncryptionOptions {
                            OFF,
                            ON,
                            NOT_SUP,
                            REQ,
                            LOGIN
                        } */

                        switch (_encryptionOption) {
                            case (EncryptionOptions.ON):
                                if (serverOption == EncryptionOptions.NOT_SUP) {
                                    _physicalStateObj.AddError(new SqlError(TdsEnums.ENCRYPTION_NOT_SUPPORTED, (byte)0x00, TdsEnums.FATAL_ERROR_CLASS, _server, SQLMessage.EncryptionNotSupportedByServer(), "", 0));
                                    _physicalStateObj.Dispose();
                                    ThrowExceptionAndWarning(_physicalStateObj);
                                }

                                break;

                            case (EncryptionOptions.OFF):
                                if (serverOption == EncryptionOptions.OFF) {
                                    // Only encrypt login.
                                    _encryptionOption = EncryptionOptions.LOGIN;
                                }
                                else if (serverOption == EncryptionOptions.REQ) {
                                    // Encrypt all.
                                    _encryptionOption = EncryptionOptions.ON;
                                }

                                break;

                            case (EncryptionOptions.NOT_SUP):
                                if (serverOption == EncryptionOptions.REQ) {
                                    _physicalStateObj.AddError(new SqlError(TdsEnums.ENCRYPTION_NOT_SUPPORTED, (byte)0x00, TdsEnums.FATAL_ERROR_CLASS, _server, SQLMessage.EncryptionNotSupportedByClient(), "", 0));
                                    _physicalStateObj.Dispose();
                                    ThrowExceptionAndWarning(_physicalStateObj);
                                }

                                break;

                            default:
                                Debug.Assert(false, "Invalid client encryption option detected");
                                break;
                        }

                        if (_encryptionOption == EncryptionOptions.ON ||
                            _encryptionOption == EncryptionOptions.LOGIN) {
                            UInt32 error = 0;
                            
                            // If we're using legacy server certificate validation behavior (Authentication keyword not provided and not using access token), then validate if
                            //     Encrypt=true and Trust Sever Certificate = false.
                            // If using Authentication keyword or access token, validate if Trust Server Certificate=false.
                            bool shouldValidateServerCert = (encrypt && !trustServerCert) || ((authType != SqlAuthenticationMethod.NotSpecified || _connHandler._accessTokenInBytes != null) && !trustServerCert);

                            UInt32 info = (shouldValidateServerCert ? TdsEnums.SNI_SSL_VALIDATE_CERTIFICATE : 0)
                                | (isYukonOrLater ? TdsEnums.SNI_SSL_USE_SCHANNEL_CACHE : 0);

                            if (encrypt && !integratedSecurity) {
                                // optimization: in case of SQL Authentication and encryption, set SNI_SSL_IGNORE_CHANNEL_BINDINGS to let SNI 
                                // know that it does not need to allocate/retrieve the Channel Bindings from the SSL context.
                                info |= TdsEnums.SNI_SSL_IGNORE_CHANNEL_BINDINGS;
                            }

                            // Add SSL (Encryption) SNI provider.
                            error = SNINativeMethodWrapper.SNIAddProvider(_physicalStateObj.Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV, ref info);

                            if (error != TdsEnums.SNI_SUCCESS) {
                                _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                                ThrowExceptionAndWarning(_physicalStateObj);
                            }

                            // in the case where an async connection is made, encryption is used and Windows Authentication is used, 
                            // wait for SSL handshake to complete, so that the SSL context is fully negotiated before we try to use its 
                            // Channel Bindings as part of the Windows Authentication context build (SSL handshake must complete 
                            // before calling SNISecGenClientContext).
                            error = SNINativeMethodWrapper.SNIWaitForSSLHandshakeToComplete(_physicalStateObj.Handle, _physicalStateObj.GetTimeoutRemaining());
                            if (error != TdsEnums.SNI_SUCCESS)
                            {
                                _physicalStateObj.AddError(ProcessSNIError(_physicalStateObj));
                                ThrowExceptionAndWarning(_physicalStateObj);
                            }

                            // create a new packet encryption changes the internal packet size Bug# 228403
                            try {} // EmptyTry/Finally to avoid FXCop violation
                            finally {
                                _physicalStateObj.ClearAllWritePackets();
                             }
                        }

                        break;

                    case (int)PreLoginOptions.INSTANCE:
                        payloadOffset = payload[offset++] << 8 | payload[offset++];
                        payloadLength = payload[offset++] << 8 | payload[offset++];

                        byte ERROR_INST = 0x1;
                        byte instanceResult = payload[payloadOffset];

                        if (instanceResult == ERROR_INST) {
                            // Check if server says ERROR_INST. That either means the cached info
                            // we used to connect is not valid or we connected to a named instance
                            // listening on default params.
                            return PreLoginHandshakeStatus.InstanceFailure;
                        }

                        break;

                    case (int)PreLoginOptions.THREADID:
                        // DO NOTHING FOR THREADID
                        offset += 4;
                        break;

                    case (int)PreLoginOptions.MARS:
                        payloadOffset = payload[offset++] << 8 | payload[offset++];
                        payloadLength = payload[offset++] << 8 | payload[offset++];

                        marsCapable = (payload[payloadOffset] == 0 ? false : true);

                        Debug.Assert(payload[payloadOffset] == 0 || payload[payloadOffset] == 1, "Value for Mars PreLoginHandshake option not equal to 1 or 0!");
                        break;

                    case (int)PreLoginOptions.TRACEID:
                        // DO NOTHING FOR TRACEID
                        offset += 4;
                        break;

                    case (int)PreLoginOptions.FEDAUTHREQUIRED:
                        payloadOffset = payload[offset++] << 8 | payload[offset++];
                        payloadLength = payload[offset++] << 8 | payload[offset++];

                        // Only 0x00 and 0x01 are accepted values from the server.
                        if (payload[payloadOffset] != 0x00 && payload[payloadOffset] != 0x01) {
                            Bid.Trace("<sc.TdsParser.ConsumePreLoginHandshake|ERR> %d#, Server sent an unexpected value for FedAuthRequired PreLogin Option. Value was %d.\n", ObjectID, (int)payload[payloadOffset]);
                            throw SQL.ParsingErrorValue(ParsingErrorState.FedAuthRequiredPreLoginResponseInvalidValue, (int)payload[payloadOffset]);
                        }

                        // We must NOT use the response for the FEDAUTHREQUIRED PreLogin option, if the connection string option
                        // was not using the new Authentication keyword or in other words, if Authentication=NotSpecified
                        // Or AccessToken is not null, mean token based authentication is used.
                        if ((_connHandler.ConnectionOptions != null
                            && _connHandler.ConnectionOptions.Authentication != SqlAuthenticationMethod.NotSpecified)
                            || _connHandler._accessTokenInBytes != null) 
                        {
                            fedAuthRequired = payload[payloadOffset] == 0x01 ? true : false;
                        }
                        break;

                    default:
                        Debug.Assert(false, "UNKNOWN option in ConsumePreLoginHandshake, option:" + option);

                        // DO NOTHING FOR THESE UNKNOWN OPTIONS
                        offset += 4;

                        break;
                }

                if (offset < payload.Length) {
                    option = payload[offset++];
                }
                else {
                    break;
                }
            }

            return PreLoginHandshakeStatus.Successful;
        }

        internal void Deactivate(bool connectionIsDoomed) {
            // Called when the connection that owns us is deactivated.

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParser.Deactivate|ADV> %d# deactivating\n", ObjectID);
            }

            if (Bid.IsOn(Bid.ApiGroup.StateDump)) {
                Bid.Trace("<sc.TdsParser.Deactivate|STATE> %d#, %ls\n", ObjectID, TraceString());
            }

            if (MARSOn) {
                _sessionPool.Deactivate();
            }

            Debug.Assert(connectionIsDoomed || null == _pendingTransaction, "pending transaction at disconnect?");

            if (!connectionIsDoomed && null != _physicalStateObj) {
                if (_physicalStateObj._pendingData) {
                    DrainData(_physicalStateObj);
                }

                if (_physicalStateObj.HasOpenResult) { // SQL BU DT 383773 - need to decrement openResultCount for all pending operations.
                    _physicalStateObj.DecrementOpenResultCount();
                }
            }

            // Any active, non-distributed transaction must be rolled back.  We
            // need to wait for distributed transactions to be completed by the
            // transaction manager -- we don't want to automatically roll them
            // back.
            //
            // Note that when there is a transaction delegated to this connection,
            // we will defer the deactivation of this connection until the
            // transaction manager completes the transaction.
            SqlInternalTransaction currentTransaction = CurrentTransaction;

            if (null != currentTransaction && currentTransaction.HasParentTransaction) {
                currentTransaction.CloseFromConnection();
                Debug.Assert(null == CurrentTransaction, "rollback didn't clear current transaction?");
            }

            Statistics = null; // must come after CleanWire or we won't count the stuff that happens there...
        }

        // Used to close the connection and then free the memory allocated for the netlib connection.
        internal void Disconnect() {
            if (null != _sessionPool) {
                // MARSOn may be true, but _sessionPool not yet created
                _sessionPool.Dispose();
            }

            // Can close the connection if its open or broken
            if (_state != TdsParserState.Closed) {
                //benign assert - the user could close the connection before consuming all the data
                //Debug.Assert(_physicalStateObj._inBytesUsed == _physicalStateObj._inBytesRead && _physicalStateObj._outBytesUsed == _physicalStateObj._inputHeaderLen, "TDSParser closed with data not fully sent or consumed.");

                _state = TdsParserState.Closed;

                try {
                    // If the _physicalStateObj has an owner, we will delay the disposal until the owner is finished with it
                    if (!_physicalStateObj.HasOwner) {
                        _physicalStateObj.SniContext = SniContext.Snix_Close;
#if DEBUG
                        _physicalStateObj.InvalidateDebugOnlyCopyOfSniContext();
#endif
                        _physicalStateObj.Dispose();
                    }
                    else {
                        // Remove the "initial" callback (this will allow the stateObj to be GC collected if need be)
                        _physicalStateObj.DecrementPendingCallbacks(false);
                    }

                    // Not allocated until MARS is actually enabled in SNI.
                    if (null != _pMarsPhysicalConObj) {
                        _pMarsPhysicalConObj.Dispose();
                    }
                }
                finally {
                    _pMarsPhysicalConObj = null;
                }
            }
        }

        // Fires a single InfoMessageEvent
        private void FireInfoMessageEvent(SqlConnection connection, TdsParserStateObject stateObj, SqlError error) {

            string serverVersion = null;

            Debug.Assert(connection != null && _connHandler.Connection == connection);

            if (_state == TdsParserState.OpenLoggedIn) {
                serverVersion = _connHandler.ServerVersion;
            }

            SqlErrorCollection sqlErs = new SqlErrorCollection();

            sqlErs.Add(error);

            SqlException exc = SqlException.CreateException(sqlErs, serverVersion, _connHandler);

            bool notified;
            connection.OnInfoMessage(new SqlInfoMessageEventArgs(exc), out notified);
            if (notified) {
                // observable side-effects, no retry
                stateObj._syncOverAsync = true;
            }
            return;
        }

        internal void DisconnectTransaction(SqlInternalTransaction internalTransaction) {
            Debug.Assert(_currentTransaction != null && _currentTransaction == internalTransaction, "disconnecting different transaction");

            if (_currentTransaction != null && _currentTransaction == internalTransaction) {
                _currentTransaction = null;
            }
        }

        internal void RollbackOrphanedAPITransactions() {
            // Any active, non-distributed transaction must be rolled back.
            SqlInternalTransaction currentTransaction = CurrentTransaction;

            if (null != currentTransaction && currentTransaction.HasParentTransaction && currentTransaction.IsOrphaned) {
                currentTransaction.CloseFromConnection();
                Debug.Assert(null == CurrentTransaction, "rollback didn't clear current transaction?");
            }
        }

        internal void ThrowExceptionAndWarning(TdsParserStateObject stateObj, bool callerHasConnectionLock = false, bool asyncClose = false) {
            Debug.Assert(!callerHasConnectionLock || _connHandler._parserLock.ThreadMayHaveLock(), "Caller claims to have lock, but connection lock is not taken");

            SqlException exception = null;
            bool breakConnection;

            // This function should only be called when there was an error or warning.  If there aren't any
            // errors, the handler will be called for the warning(s).  If there was an error, the warning(s) will
            // be copied to the end of the error collection so that the user may see all the errors and also the
            // warnings that occurred.
            // can be deleted)
            SqlErrorCollection temp = stateObj.GetFullErrorAndWarningCollection(out breakConnection);

            Debug.Assert(temp.Count > 0, "TdsParser::ThrowExceptionAndWarning called with no exceptions or warnings!");
            if (temp.Count == 0)
            {
                Bid.Trace("<sc.TdsParser.ThrowExceptionAndWarning|ERR> Potential multi-threaded misuse of connection, unexpectedly empty warnings/errors under lock %d#\n", ObjectID);
            }
            Debug.Assert(_connHandler != null, "TdsParser::ThrowExceptionAndWarning called with null connectionHandler!");
            
            // Don't break the connection if it is already closed
            breakConnection &= (TdsParserState.Closed != _state);
            if (breakConnection) {
                if ((_state == TdsParserState.OpenNotLoggedIn) && (_connHandler.ConnectionOptions.TransparentNetworkIPResolution || _connHandler.ConnectionOptions.MultiSubnetFailover || _loginWithFailover) && (temp.Count == 1) && ((temp[0].Number == TdsEnums.TIMEOUT_EXPIRED) || (temp[0].Number == TdsEnums.SNI_WAIT_TIMEOUT))) {
                    // DevDiv2 Bug 459546: With "MultiSubnetFailover=yes" in the Connection String, SQLClient incorrectly throws a Timeout using shorter time slice (3-4 seconds), not honoring the actual 'Connect Timeout'
                    // http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/459546
                    // For Multisubnet Failover we slice the timeout to make reconnecting faster (with the assumption that the server will not failover instantaneously)
                    // However, when timeout occurs we need to not doom the internal connection and also to mark the TdsParser as closed such that the login will be will retried
                    breakConnection = false;
                    Disconnect();
                }
                else {
                    _state = TdsParserState.Broken;
                }
            }
            
            Debug.Assert(temp != null, "TdsParser::ThrowExceptionAndWarning: 0 errors in collection");
            if (temp != null && temp.Count > 0) {
                // Construct the exception now that we've collected all the errors
                string serverVersion = null;
                if (_state == TdsParserState.OpenLoggedIn) {
                    serverVersion = _connHandler.ServerVersion;
                }
                exception = SqlException.CreateException(temp, serverVersion, _connHandler);
                if (exception.Procedure == TdsEnums.INIT_ADAL_PACKAGE || exception.Procedure == TdsEnums.INIT_SSPI_PACKAGE) {
                    exception._doNotReconnect = true;
                }
            }

            // call OnError outside of _ErrorCollectionLock to avoid deadlock
            if (exception != null) {
                if (breakConnection) {
                    // report exception to pending async operation
                    // before OnConnectionClosed overrides the exception
                    // due to connection close notification through references
                    var taskSource = stateObj._networkPacketTaskSource;
                    if (taskSource != null) {
                        taskSource.TrySetException(ADP.ExceptionWithStackTrace(exception));
                    }
                }

                if (asyncClose) {
                    // Wait until we have the parser lock, then try to close
                    var connHandler = _connHandler;
                    Action<Action> wrapCloseAction = closeAction => {
                        Task.Factory.StartNew(() => {
                            connHandler._parserLock.Wait(canReleaseFromAnyThread: false);
                            connHandler.ThreadHasParserLockForClose = true;
                            try {
                                closeAction();
                            }
                            finally {
                                connHandler.ThreadHasParserLockForClose = false;
                                connHandler._parserLock.Release();
                            }
                        });
                    };

                    _connHandler.OnError(exception, breakConnection, wrapCloseAction);
                }
                else {
                    // Let close know that we already have the _parserLock
                    bool threadAlreadyHadParserLockForClose = _connHandler.ThreadHasParserLockForClose;
                    if (callerHasConnectionLock) {
                        _connHandler.ThreadHasParserLockForClose = true;
                    }
                    try {
                        // the following handler will throw an exception or generate a warning event   
                        _connHandler.OnError(exception, breakConnection);
                    }
                    finally {
                        if (callerHasConnectionLock) {
                            _connHandler.ThreadHasParserLockForClose = threadAlreadyHadParserLockForClose;
                        }
                    }
                }
            }
        }
        
        internal SqlError ProcessSNIError(TdsParserStateObject stateObj) {
#if DEBUG
            // There is an exception here for MARS as its possible that another thread has closed the connection just as we see an error
            Debug.Assert(SniContext.Undefined!=stateObj.DebugOnlyCopyOfSniContext || ((_fMARS) && ((_state == TdsParserState.Closed) || (_state == TdsParserState.Broken))), "SniContext must not be None");
#endif
            SNINativeMethodWrapper.SNI_Error sniError = new SNINativeMethodWrapper.SNI_Error();
            SNINativeMethodWrapper.SNIGetLastError(sniError);

            if (sniError.sniError != 0) {

                // handle special SNI error codes that are converted into exception which is not a SqlException.
                switch (sniError.sniError) {
                    case (int)SNINativeMethodWrapper.SniSpecialErrors.MultiSubnetFailoverWithMoreThan64IPs:
                        // Connecting with the MultiSubnetFailover connection option to a SQL Server instance configured with more than 64 IP addresses is not supported.
                        throw SQL.MultiSubnetFailoverWithMoreThan64IPs();
                    
                    case (int)SNINativeMethodWrapper.SniSpecialErrors.MultiSubnetFailoverWithInstanceSpecified:
                        // Connecting to a named SQL Server instance using the MultiSubnetFailover connection option is not supported.
                        throw SQL.MultiSubnetFailoverWithInstanceSpecified();

                    case (int)SNINativeMethodWrapper.SniSpecialErrors.MultiSubnetFailoverWithNonTcpProtocol:
                        // Connecting to a SQL Server instance using the MultiSubnetFailover connection option is only supported when using the TCP protocol.
                        throw SQL.MultiSubnetFailoverWithNonTcpProtocol();

                    // continue building SqlError instance
                }
            }

            // error.errorMessage is null terminated with garbage beyond that, since fixed length
            string errorMessage;
            int MessageLength = Array.IndexOf(sniError.errorMessage, '\0');
            if (MessageLength == -1) {
                errorMessage = String.Empty;  // If we don't see the expected null return nothing.
            } else {            
                errorMessage = new String(sniError.errorMessage, 0, MessageLength);
            }

            //  Format SNI errors and add Context Information
            //
            //  General syntax is:
            //  <sqlclient message>
            //  (provider:<SNIx provider>, error: <SNIx error code> - <SNIx error message>)
            //
            // errorMessage | sniError |
            // -------------------------------------------
            // ==null       | x        | must never happen
            // !=null       | != 0     | retrieve corresponding errorMessage from resources
            // !=null       | == 0     | replace text left of errorMessage
            //

            Debug.Assert(!ADP.IsEmpty(errorMessage),"Empty error message received from SNI");

            string sqlContextInfo = Res.GetString(Enum.GetName(typeof(SniContext), stateObj.SniContext));

            string providerRid = String.Format((IFormatProvider)null,"SNI_PN{0}", (int)sniError.provider);
            string providerName = Res.GetString(providerRid);
            Debug.Assert(!ADP.IsEmpty(providerName), String.Format((IFormatProvider)null,"invalid providerResourceId '{0}'", providerRid));
            uint win32ErrorCode = sniError.nativeError;

            if (sniError.sniError == 0) {
                // Provider error. The message from provider is preceeded with non-localizable info from SNI
                // strip provider info from SNI
                //
                int iColon = errorMessage.IndexOf(':');
                Debug.Assert(0<=iColon, "':' character missing in sni errorMessage");
                Debug.Assert(errorMessage.Length>iColon+1 && errorMessage[iColon+1]==' ', "Expecting a space after the ':' character");

                // extract the message excluding the colon and trailing cr/lf chars
                if (0<=iColon) {
                    int len = errorMessage.Length;
                    len -=2;    // exclude "\r\n" sequence
                    iColon+=2;  // skip over ": " sequence
                    len-=iColon;
                    /*
                        The error message should come back in the following format: "TCP Provider: MESSAGE TEXT"
                        Fix Bug 370686, if the message is recieved on a Win9x OS, the error message will not contain MESSAGE TEXT 
                        per Bug: 269574.  If we get a errormessage with no message text, just return the entire message otherwise 
                        return just the message text.
                    */
                    if (len > 0) { 
                        errorMessage = errorMessage.Substring(iColon, len); 
                    }
                }
            }
            else {
                // SNI error. Replace the entire message
                //
                errorMessage = SQL.GetSNIErrorMessage((int)sniError.sniError);

                // If its a LocalDB error, then nativeError actually contains a LocalDB-specific error code, not a win32 error code
                if (sniError.sniError == (int)SNINativeMethodWrapper.SniSpecialErrors.LocalDBErrorCode) {
                    errorMessage += LocalDBAPI.GetLocalDBMessage((int)sniError.nativeError);
                    win32ErrorCode = 0;
                }
            }
            errorMessage = String.Format((IFormatProvider)null, "{0} (provider: {1}, error: {2} - {3})",
                sqlContextInfo, providerName, (int)sniError.sniError, errorMessage);

            return new SqlError((int)sniError.nativeError, 0x00, TdsEnums.FATAL_ERROR_CLASS,
                                _server, errorMessage, sniError.function, (int)sniError.lineNumber, win32ErrorCode);
        }

        internal void CheckResetConnection(TdsParserStateObject stateObj) {
            if (_fResetConnection && !stateObj._fResetConnectionSent) {
                Debug.Assert(stateObj._outputPacketNumber == 1 || stateObj._outputPacketNumber == 2, "In ResetConnection logic unexpectedly!");
                try {
                    if (_fMARS && !stateObj._fResetEventOwned) {
                        // If using Async & MARS and we do not own ResetEvent - grab it.  We need to not grab lock here
                        // for case where multiple packets are sent to server from one execute.
                        stateObj._fResetEventOwned = _resetConnectionEvent.WaitOne(stateObj.GetTimeoutRemaining(), false);

                        if (stateObj._fResetEventOwned) {
                            if (stateObj.TimeoutHasExpired) {
                                // We didn't timeout on the WaitOne, but we timed out by the time we decremented stateObj._timeRemaining.
                                stateObj._fResetEventOwned = !_resetConnectionEvent.Set();
                                stateObj.TimeoutTime = 0;
                            }
                        }

                        if (!stateObj._fResetEventOwned) {
                            // We timed out waiting for ResetEvent.  Throw timeout exception and reset
                            // the buffer.  Nothing else to do since we did not actually send anything
                            // to the server.
                            stateObj.ResetBuffer();
                            Debug.Assert(_connHandler != null, "SqlConnectionInternalTds handler can not be null at this point.");
                            stateObj.AddError(new SqlError(TdsEnums.TIMEOUT_EXPIRED, (byte)0x00, TdsEnums.MIN_ERROR_CLASS, _server, _connHandler.TimeoutErrorInternal.GetErrorMessage(), "", 0, TdsEnums.SNI_WAIT_TIMEOUT));
                            Debug.Assert(_connHandler._parserLock.ThreadMayHaveLock(), "Thread is writing without taking the connection lock");
                            ThrowExceptionAndWarning(stateObj, callerHasConnectionLock: true);
                        }
                    }

                    if (_fResetConnection) {
                        // Check again to see if we need to send reset.

                        Debug.Assert(!stateObj._fResetConnectionSent, "Unexpected state for sending reset connection");
                        Debug.Assert(_isShiloh, "TdsParser.cs: Error!  _fResetConnection true when not going against Shiloh or later!");

                        if (_fPreserveTransaction) {
                            // if we are reseting, set bit in header by or'ing with other value
                            stateObj._outBuff[1] = (Byte)(stateObj._outBuff[1] | TdsEnums.ST_RESET_CONNECTION_PRESERVE_TRANSACTION);
                        }
                        else {
                            // if we are reseting, set bit in header by or'ing with other value
                            stateObj._outBuff[1] = (Byte)(stateObj._outBuff[1] | TdsEnums.ST_RESET_CONNECTION);
                        }

                        if (!_fMARS) {
                            _fResetConnection = false; // If not MARS, can turn off flag now.
                            _fPreserveTransaction = false;
                        }
                        else {
                            stateObj._fResetConnectionSent = true; // Otherwise set flag so we don't resend on multiple packet execute.
                        }
                    }
                    else if (_fMARS && stateObj._fResetEventOwned) {
                        Debug.Assert(!stateObj._fResetConnectionSent, "Unexpected state on WritePacket ResetConnection");

                        // Otherwise if Yukon and we grabbed the event, free it.  Another execute grabbed the event and
                        // took care of sending the reset.
                        stateObj._fResetEventOwned = !_resetConnectionEvent.Set();
                        Debug.Assert(!stateObj._fResetEventOwned, "Invalid AutoResetEvent state!");
                    }
                }
                catch (Exception) {
                    if (_fMARS && stateObj._fResetEventOwned) {
                        // If exception thrown, and we are on Yukon and own the event, release it!
                        stateObj._fResetConnectionSent = false;
                        stateObj._fResetEventOwned = !_resetConnectionEvent.Set();
                        Debug.Assert(!stateObj._fResetEventOwned, "Invalid AutoResetEvent state!");
                    }

                    throw;
                }
            }
#if DEBUG
            else {
                Debug.Assert(!_fResetConnection ||
                             (_fResetConnection && stateObj._fResetConnectionSent && stateObj._fResetEventOwned),
                             "Unexpected state on else ResetConnection block in WritePacket");
            }
#endif
        }

        //
        // Takes a 16 bit short and writes it.
        //
        internal byte[] SerializeShort(int v, TdsParserStateObject stateObj) {
            if (null == stateObj._bShortBytes) {
                stateObj._bShortBytes = new byte[2];
            }
            else {
                Debug.Assert(2 == stateObj._bShortBytes.Length);
                }

            byte[] bytes = stateObj._bShortBytes;
            int current = 0;
            bytes[current++] = (byte)(v & 0xff);
            bytes[current++] = (byte)((v >> 8) & 0xff);
            return bytes;
        }

        internal void WriteShort(int v, TdsParserStateObject stateObj) {
            ReliabilitySection.Assert("unreliable call to WriteShort");  // you need to setup for a thread abort somewhere before you call this method

            if ((stateObj._outBytesUsed + 2) > stateObj._outBuff.Length) {
                // if all of the short doesn't fit into the buffer
                stateObj.WriteByte((byte)(v & 0xff));
                stateObj.WriteByte((byte)((v >> 8) & 0xff));
            }
            else {
                // all of the short fits into the buffer
                stateObj._outBuff[stateObj._outBytesUsed] = (byte)(v & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 1] = (byte)((v >> 8) & 0xff);
                stateObj._outBytesUsed += 2;
            }
        }

        internal void WriteUnsignedShort(ushort us, TdsParserStateObject stateObj) {
            WriteShort((short)us, stateObj);
        }

        //
        // Takes a long and writes out an unsigned int
        //
        internal byte[] SerializeUnsignedInt(uint i, TdsParserStateObject stateObj) {
            return SerializeInt((int)i, stateObj);
        }

        internal void WriteUnsignedInt(uint i, TdsParserStateObject stateObj) {
            WriteInt((int)i, stateObj);
        }

        //
        // Takes an int and writes it as an int.
        //
        internal byte[] SerializeInt(int v, TdsParserStateObject stateObj) {
            if (null == stateObj._bIntBytes) {
                stateObj._bIntBytes = new byte[4];
            }
            else {
                Debug.Assert (4 == stateObj._bIntBytes.Length);
            }

            int current = 0;
            byte[] bytes = stateObj._bIntBytes;
            bytes[current++] = (byte)(v & 0xff);
            bytes[current++] = (byte)((v >> 8) & 0xff);
            bytes[current++] = (byte)((v >> 16) & 0xff);
            bytes[current++] = (byte)((v >> 24) & 0xff);
            return bytes;
        }

        internal void WriteInt(int v, TdsParserStateObject stateObj) {
            ReliabilitySection.Assert("unreliable call to WriteInt");  // you need to setup for a thread abort somewhere before you call this method

            if ((stateObj._outBytesUsed + 4) > stateObj._outBuff.Length) {
                // if all of the int doesn't fit into the buffer
                for (int shiftValue = 0; shiftValue < sizeof(int) * 8; shiftValue += 8) {
                    stateObj.WriteByte((byte)((v >> shiftValue) & 0xff));
                }
            }
            else {
                // all of the int fits into the buffer
                // NOTE: We don't use a loop here for performance
                stateObj._outBuff[stateObj._outBytesUsed] = (byte)(v & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 1] = (byte)((v >> 8) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 2] = (byte)((v >> 16) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 3] = (byte)((v >> 24) & 0xff);
                stateObj._outBytesUsed += 4;
            }
        }

        //
        // Takes a float and writes it as a 32 bit float.
        //
        internal byte[] SerializeFloat(float v) {
            if (Single.IsInfinity(v) || Single.IsNaN(v)) { 
                throw ADP.ParameterValueOutOfRange(v.ToString());
            }

            return BitConverter.GetBytes(v);
        }

        internal void WriteFloat(float v, TdsParserStateObject stateObj) {
            byte[] bytes = BitConverter.GetBytes(v);

            stateObj.WriteByteArray(bytes, bytes.Length, 0);
        }

        //
        // Takes a long and writes it as a long.
        //
        internal byte[] SerializeLong(long v, TdsParserStateObject stateObj) {
            int current = 0;
            if (null == stateObj._bLongBytes) {
                stateObj._bLongBytes = new byte[8];
            }

            byte[] bytes = stateObj._bLongBytes;
            Debug.Assert (8 == bytes.Length, "Cached buffer has wrong size");

            bytes[current++] = (byte)(v & 0xff);
            bytes[current++] = (byte)((v >> 8) & 0xff);
            bytes[current++] = (byte)((v >> 16) & 0xff);
            bytes[current++] = (byte)((v >> 24) & 0xff);
            bytes[current++] = (byte)((v >> 32) & 0xff);
            bytes[current++] = (byte)((v >> 40) & 0xff);
            bytes[current++] = (byte)((v >> 48) & 0xff);
            bytes[current++] = (byte)((v >> 56) & 0xff);

            return bytes;
        }

        internal void WriteLong(long v, TdsParserStateObject stateObj) {
            ReliabilitySection.Assert("unreliable call to WriteLong");  // you need to setup for a thread abort somewhere before you call this method

            if ((stateObj._outBytesUsed + 8) > stateObj._outBuff.Length) {
                // if all of the long doesn't fit into the buffer
                for (int shiftValue = 0; shiftValue < sizeof(long) * 8; shiftValue += 8) {
                    stateObj.WriteByte((byte)((v >> shiftValue) & 0xff));
                }
            }
            else {
                // all of the long fits into the buffer
                // NOTE: We don't use a loop here for performance
                stateObj._outBuff[stateObj._outBytesUsed] = (byte)(v & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 1] = (byte)((v >> 8) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 2] = (byte)((v >> 16) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 3] = (byte)((v >> 24) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 4] = (byte)((v >> 32) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 5] = (byte)((v >> 40) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 6] = (byte)((v >> 48) & 0xff);
                stateObj._outBuff[stateObj._outBytesUsed + 7] = (byte)((v >> 56) & 0xff);
                stateObj._outBytesUsed += 8;
            }
        }

        //
        // Takes a long and writes part of it
        //
        internal byte[] SerializePartialLong(long v, int length) {
            Debug.Assert(length <= 8, "Length specified is longer than the size of a long");
            Debug.Assert(length >= 0, "Length should not be negative");

            byte[] bytes = new byte[length];

            // all of the long fits into the buffer
            for (int index = 0; index < length; index++) {
                bytes[index] = (byte)((v >> (index * 8)) & 0xff);
            }

            return bytes;
        }

        internal void WritePartialLong(long v, int length, TdsParserStateObject stateObj) {
            ReliabilitySection.Assert("unreliable call to WritePartialLong");  // you need to setup for a thread abort somewhere before you call this method
            Debug.Assert(length <= 8, "Length specified is longer than the size of a long");
            Debug.Assert(length >= 0, "Length should not be negative");

            if ((stateObj._outBytesUsed + length) > stateObj._outBuff.Length) {
                // if all of the long doesn't fit into the buffer
                for (int shiftValue = 0; shiftValue < length * 8; shiftValue += 8) {
                    stateObj.WriteByte((byte)((v >> shiftValue) & 0xff));
                }
            }
            else {
                // all of the long fits into the buffer
                for (int index = 0; index < length; index++) {
                    stateObj._outBuff[stateObj._outBytesUsed + index] = (byte)((v >> (index * 8)) & 0xff);
                }
                stateObj._outBytesUsed += length;
            }
        }

        //
        // Takes a ulong and writes it as a ulong.
        //
        internal void WriteUnsignedLong(ulong uv, TdsParserStateObject stateObj) {
            WriteLong((long)uv, stateObj);
        }

        //
        // Takes a double and writes it as a 64 bit double.
        //
        internal byte[] SerializeDouble(double v) {
            if (Double.IsInfinity(v) || Double.IsNaN(v)) {
                throw ADP.ParameterValueOutOfRange(v.ToString());
            }

            return BitConverter.GetBytes(v);
        }

        internal void WriteDouble(double v, TdsParserStateObject stateObj) {
            byte[] bytes = BitConverter.GetBytes(v);

            stateObj.WriteByteArray(bytes, bytes.Length, 0);
        }

        internal void PrepareResetConnection(bool preserveTransaction) {
            // Set flag to reset connection upon next use - only for use on shiloh!
            _fResetConnection = true;
            _fPreserveTransaction = preserveTransaction;
        }

        internal bool RunReliably(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj) {
            RuntimeHelpers.PrepareConstrainedRegions();
            try {
#if DEBUG
                TdsParser.ReliabilitySection tdsReliabilitySection = new TdsParser.ReliabilitySection();
                RuntimeHelpers.PrepareConstrainedRegions();
                try {
                    tdsReliabilitySection.Start();
#endif //DEBUG
                    return Run(runBehavior, cmdHandler, dataStream, bulkCopyHandler, stateObj);
#if DEBUG
                }
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (OutOfMemoryException) {
                _connHandler.DoomThisConnection();
                throw;
            }
            catch (StackOverflowException) {
                _connHandler.DoomThisConnection();
                throw;
            }
            catch (ThreadAbortException) {
                _connHandler.DoomThisConnection();
                throw;
            }
        }
        

        internal bool Run(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj) {
            bool syncOverAsync = stateObj._syncOverAsync;
            try
            {
                stateObj._syncOverAsync = true;

                bool dataReady;
                bool result = TryRun(runBehavior, cmdHandler, dataStream, bulkCopyHandler, stateObj, out dataReady);
                Debug.Assert(result == true, "Should never return false when _syncOverAsync is set");
                return dataReady;
            }
            finally
            {
                stateObj._syncOverAsync = syncOverAsync;
            }
        }

        /// <summary>
        /// Checks if the given token is a valid TDS token
        /// </summary>
        /// <param name="token">Token to check</param>
        /// <returns>True if the token is a valid TDS token, otherwise false</returns>
        internal static bool IsValidTdsToken(byte token) {
            return (
                token == TdsEnums.SQLERROR ||
                token == TdsEnums.SQLINFO ||
                token == TdsEnums.SQLLOGINACK ||
                token == TdsEnums.SQLENVCHANGE ||
                token == TdsEnums.SQLRETURNVALUE ||
                token == TdsEnums.SQLRETURNSTATUS ||
                token == TdsEnums.SQLCOLNAME ||
                token == TdsEnums.SQLCOLFMT ||
                token == TdsEnums.SQLCOLMETADATA ||
                token == TdsEnums.SQLALTMETADATA ||
                token == TdsEnums.SQLTABNAME ||
                token == TdsEnums.SQLCOLINFO ||
                token == TdsEnums.SQLORDER ||
                token == TdsEnums.SQLALTROW ||
                token == TdsEnums.SQLROW ||
                token == TdsEnums.SQLNBCROW ||
                token == TdsEnums.SQLDONE ||
                token == TdsEnums.SQLDONEPROC ||
                token == TdsEnums.SQLDONEINPROC ||
                token == TdsEnums.SQLROWCRC ||
                token == TdsEnums.SQLSECLEVEL ||
                token == TdsEnums.SQLPROCID ||
                token == TdsEnums.SQLOFFSET ||
                token == TdsEnums.SQLSSPI ||
                token == TdsEnums.SQLFEATUREEXTACK ||
                token == TdsEnums.SQLSESSIONSTATE ||
                token == TdsEnums.SQLFEDAUTHINFO);
        }

        // Main parse loop for the top-level tds tokens, calls back into the I*Handler interfaces
        internal bool TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, out bool dataReady) {
            ReliabilitySection.Assert("unreliable call to Run");  // you need to setup for a thread abort somewhere before you call this method
            Debug.Assert((SniContext.Undefined != stateObj.SniContext) &&       // SniContext must not be Undefined
                ((stateObj._attentionSent) || ((SniContext.Snix_Execute != stateObj.SniContext) && (SniContext.Snix_SendRows != stateObj.SniContext))),  // SniContext should not be Execute or SendRows unless attention was sent (and, therefore, we are looking for an ACK)
                        String.Format("Unexpected SniContext on call to TryRun; SniContext={0}", stateObj.SniContext));

            if (TdsParserState.Broken == State || TdsParserState.Closed == State ) {
                dataReady = true;
                return true; // Just in case this is called in a loop, expecting data to be returned.
            }
            
            dataReady = false;
            
            do {
                // If there is data ready, but we didn't exit the loop, then something is wrong
                Debug.Assert(!dataReady, "dataReady not expected - did we forget to skip the row?");

                if (stateObj._internalTimeout) {
                    runBehavior = RunBehavior.Attention;
                }

                if (TdsParserState.Broken == State || TdsParserState.Closed == State)
                    break; // jump out of the loop if the state is already broken or closed.

                if (!stateObj._accumulateInfoEvents && (stateObj._pendingInfoEvents != null)) {
                    if (RunBehavior.Clean != (RunBehavior.Clean & runBehavior)) {
                        SqlConnection connection = null;
                        if (_connHandler != null)
                            connection = _connHandler.Connection; // SqlInternalConnection holds the user connection object as a weak ref
                        // We are omitting checks for error.Class in the code below (see processing of INFO) since we know (and assert) that error class
                        // error.Class < TdsEnums.MIN_ERROR_CLASS for info message. 
                        // Also we know that TdsEnums.MIN_ERROR_CLASS<TdsEnums.MAX_USER_CORRECTABLE_ERROR_CLASS
                        if ((connection != null) && connection.FireInfoMessageEventOnUserErrors)
                        {
                            foreach (SqlError error in stateObj._pendingInfoEvents)
                                FireInfoMessageEvent(connection, stateObj, error);
                        }
                        else
                            foreach (SqlError error in stateObj._pendingInfoEvents)
                                stateObj.AddWarning(error);
                            
                    }
                    stateObj._pendingInfoEvents=null;
                }

                byte token;
                if (!stateObj.TryReadByte(out token)) {
                    return false;
                }

                if (!IsValidTdsToken(token)) {
                    Debug.Assert(false, String.Format((IFormatProvider)null, "unexpected token; token = {0,-2:X2}", token));
                    _state = TdsParserState.Broken;
                    _connHandler.BreakConnection();
                    Bid.Trace("<sc.TdsParser.Run|ERR> Potential multi-threaded misuse of connection, unexpected TDS token found %d#\n", ObjectID);
                    throw SQL.ParsingErrorToken(ParsingErrorState.InvalidTdsTokenReceived, token); // MDAC 82443
                }

                int tokenLength;
                if (!TryGetTokenLength(token, stateObj, out tokenLength)) {
                    return false;
                }

                switch (token) {
                case TdsEnums.SQLERROR:
                case TdsEnums.SQLINFO:
                {
                    if (token == TdsEnums.SQLERROR) {
                        stateObj._errorTokenReceived = true; // Keep track of the fact error token was received - for Done processing.
                    }

                    SqlError error;
                    if (!TryProcessError(token, stateObj, out error)) {
                        return false;
                    }

                    if (token == TdsEnums.SQLINFO && stateObj._accumulateInfoEvents)
                    {
                        Debug.Assert(error.Class < TdsEnums.MIN_ERROR_CLASS, "INFO with class > TdsEnums.MIN_ERROR_CLASS");

                        if (stateObj._pendingInfoEvents == null)
                            stateObj._pendingInfoEvents = new List<SqlError>();
                        stateObj._pendingInfoEvents.Add(error);
                        stateObj._syncOverAsync = true;
                        break;
                    }

                    if (RunBehavior.Clean != (RunBehavior.Clean & runBehavior)) {
                        // If FireInfoMessageEventOnUserErrors is true, we have to fire event without waiting.
                        // Otherwise we can go ahead and add it to errors/warnings collection.
                        SqlConnection connection = null;
                        if (_connHandler != null)
                            connection = _connHandler.Connection; // SqlInternalConnection holds the user connection object as a weak ref

                        if ((connection != null) && 
                            (connection.FireInfoMessageEventOnUserErrors == true) &&
                            (error.Class <= TdsEnums.MAX_USER_CORRECTABLE_ERROR_CLASS)) {
                            // Fire SqlInfoMessage here
                            FireInfoMessageEvent(connection, stateObj, error);
                        }
                        else {
                            // insert error/info into the appropriate exception - warning if info, exception if error
                            if (error.Class < TdsEnums.MIN_ERROR_CLASS) {
                                stateObj.AddWarning(error);
                            }
                            else if (error.Class < TdsEnums.FATAL_ERROR_CLASS) {
                                // VSTFDEVDIV 479643: continue results processing for all non-fatal errors (<20)

                                stateObj.AddError(error);

                                // Add it to collection - but do NOT change run behavior UNLESS
                                // we are in an ExecuteReader call - at which time we will be throwing
                                // anyways so we need to consume all errors.  This is not the case
                                // if we have already given out a reader.  If we have already given out
                                // a reader we need to throw the error but not halt further processing.  We used to
                                // halt processing and that was a bug preventing the user from
                                // processing subsequent results.

                                if (null != dataStream) { // Webdata 104560
                                    if (!dataStream.IsInitialized) {
                                        runBehavior = RunBehavior.UntilDone;
                                    }
                                }
                            }
                            else {
                                stateObj.AddError(error);

                                // Else we have a fatal error and we need to change the behavior
                                // since we want the complete error information in the exception.
                                // Besides - no further results will be received.
                                runBehavior = RunBehavior.UntilDone;
                            }
                        }
                    }
                    else if (error.Class >= TdsEnums.FATAL_ERROR_CLASS) {
                        stateObj.AddError(error);
                    }
                    break;
                }

                case TdsEnums.SQLCOLINFO:
                {
                    if (null != dataStream) {
                        _SqlMetaDataSet metaDataSet;
                        if (!TryProcessColInfo(dataStream.MetaData, dataStream, stateObj, out metaDataSet)) {
                            return false;
                        }
                        if (!dataStream.TrySetMetaData(metaDataSet, false)) {
                            return false;
                        }
                        dataStream.BrowseModeInfoConsumed = true;
                    }
                    else { // no dataStream
                        if (!stateObj.TrySkipBytes(tokenLength)) {
                            return false;
                        }
                    }
                    break;
                }

                case TdsEnums.SQLDONE:
                case TdsEnums.SQLDONEPROC:
                case TdsEnums.SQLDONEINPROC:
                {
                    // RunBehavior can be modified - see SQL BU DT 269516 & 290090
                    if (!TryProcessDone(cmdHandler, dataStream, ref runBehavior, stateObj)) {
                        return false;
                    }
                    if ((token == TdsEnums.SQLDONEPROC) &&  (cmdHandler != null)) {
                        // If the current parse/read is for the results of describe parameter encryption RPC requests,
                        // call a different handler which will update the describe parameter encryption RPC structures
                        // with the results, instead of the actual user RPC requests.
                        if (cmdHandler.IsDescribeParameterEncryptionRPCCurrentlyInProgress) {
                            cmdHandler.OnDoneDescribeParameterEncryptionProc(stateObj);
                        }
                        else {
                            cmdHandler.OnDoneProc();
                        }
                    }

                    break;
                }

                case TdsEnums.SQLORDER:
                {
                    // don't do anything with the order token so read off the pipe
                    if (!stateObj.TrySkipBytes(tokenLength)) {
                        return false;
                    }
                    break;
                }

                case TdsEnums.SQLALTMETADATA:
                {
                    stateObj.CloneCleanupAltMetaDataSetArray();

                    if (stateObj._cleanupAltMetaDataSetArray == null) {  
                        // create object on demand (lazy creation)
                        stateObj._cleanupAltMetaDataSetArray = new _SqlMetaDataSetCollection();
                    }

                    _SqlMetaDataSet cleanupAltMetaDataSet;
                    if (!TryProcessAltMetaData(tokenLength, stateObj, out cleanupAltMetaDataSet)) {
                        return false;
                    }

                    stateObj._cleanupAltMetaDataSetArray.SetAltMetaData(cleanupAltMetaDataSet);
                    if (null != dataStream) {
                        byte metadataConsumedByte;
                        if (!stateObj.TryPeekByte(out metadataConsumedByte)) {
                            return false;
                        }
                        if (!dataStream.TrySetAltMetaDataSet(cleanupAltMetaDataSet, (TdsEnums.SQLALTMETADATA != metadataConsumedByte))) {
                            return false;
                        }
                    }

                    break;
                }

                case TdsEnums.SQLALTROW:
                {
                    if (!stateObj.TryStartNewRow(isNullCompressed:false)) { // altrows are not currently null compressed
                        return false;
                    }

                    // read will call run until dataReady. Must not read any data if returnimmetiately set
                    if (RunBehavior.ReturnImmediately != (RunBehavior.ReturnImmediately & runBehavior)) {
                        ushort altRowId;
                        if (!stateObj.TryReadUInt16(out altRowId)) { // get altRowId
                            return false;
                        }

                        if (!TrySkipRow(stateObj._cleanupAltMetaDataSetArray.GetAltMetaData(altRowId), stateObj)) { // skip altRow
                            return false;
                        }
                    }
                    else {
                        dataReady = true;
                    }

                    break;
                }

                case TdsEnums.SQLENVCHANGE:
                {
                    // ENVCHANGE must be processed synchronously (since it can modify the state of many objects)
                    stateObj._syncOverAsync = true;

                    SqlEnvChange[] env;
                    if (!TryProcessEnvChange(tokenLength, stateObj, out env)) {
                        return false;
                    }

                    for (int ii = 0; ii < env.Length; ii++) {
                        if (env[ii] != null && !this.Connection.IgnoreEnvChange) {
                            switch (env[ii].type) {
                            case TdsEnums.ENV_BEGINTRAN:
                            case TdsEnums.ENV_ENLISTDTC:
                                // When we get notification from the server of a new
                                // transaction, we move any pending transaction over to
                                // the current transaction, then we store the token in it.
                                // if there isn't a pending transaction, then it's either
                                // a TSQL transaction or a distributed transaction.
                                Debug.Assert(null == _currentTransaction, "non-null current transaction with an ENV Change");
                                _currentTransaction = _pendingTransaction;
                                _pendingTransaction = null;

                                if (null != _currentTransaction) {
                                    _currentTransaction.TransactionId = env[ii].newLongValue;   // this is defined as a ULongLong in the server and in the TDS Spec.
                                }
                                else {
                                    TransactionType transactionType = (TdsEnums.ENV_BEGINTRAN == env[ii].type) ? TransactionType.LocalFromTSQL : TransactionType.Distributed;
                                    _currentTransaction = new SqlInternalTransaction(_connHandler, transactionType, null, env[ii].newLongValue);
                                }
                                if (null != _statistics && !_statisticsIsInTransaction) {
                                    _statistics.SafeIncrement(ref _statistics._transactions);
                                }
                                _statisticsIsInTransaction = true;
                                _retainedTransactionId = SqlInternalTransaction.NullTransactionId;
                                break;
                            case TdsEnums.ENV_DEFECTDTC:
                            case TdsEnums.ENV_TRANSACTIONENDED:
                            case TdsEnums.ENV_COMMITTRAN:
                                // SQLHOT 483
                                //  Must clear the retain id if the server-side transaction ends by anything other
                                //  than rollback.
                                _retainedTransactionId = SqlInternalTransaction.NullTransactionId;
                                goto case TdsEnums.ENV_ROLLBACKTRAN;
                            case TdsEnums.ENV_ROLLBACKTRAN:
                                // When we get notification of a completed transaction
                                // we null out the current transaction.
                                if (null != _currentTransaction) {
#if DEBUG                                    
                                    // Check null for case where Begin and Rollback obtained in the same message.
                                    if (SqlInternalTransaction.NullTransactionId != _currentTransaction.TransactionId) {
                                        Debug.Assert(_currentTransaction.TransactionId != env[ii].newLongValue, "transaction id's are not equal!");
                                    }
#endif

                                    if (TdsEnums.ENV_COMMITTRAN == env[ii].type) {
                                        _currentTransaction.Completed(TransactionState.Committed);
                                    }
                                    else if (TdsEnums.ENV_ROLLBACKTRAN == env[ii].type) {
                                        //  Hold onto transaction id if distributed tran is rolled back.  This must
                                        //  be sent to the server on subsequent executions even though the transaction
                                        //  is considered to be rolled back.
                                        if (_currentTransaction.IsDistributed && _currentTransaction.IsActive) {
                                            _retainedTransactionId = env[ii].oldLongValue;
                                        }
                                        _currentTransaction.Completed(TransactionState.Aborted);
                                    }
                                    else {
                                        // 
                                        _currentTransaction.Completed(TransactionState.Unknown);
                                    }
                                    _currentTransaction = null;
                                }
                                _statisticsIsInTransaction = false;
                                break;
                            default:
                                _connHandler.OnEnvChange(env[ii]);
                                break;
                            }
                        }
                    }
                    break;
                }
                case TdsEnums.SQLLOGINACK:
                {
                    Bid.Trace("<sc.TdsParser.TryRun|SEC> Received login acknowledgement token\n");
                    SqlLoginAck ack;
                    if (!TryProcessLoginAck(stateObj, out ack)) {
                        return false;
                    }

                    _connHandler.OnLoginAck(ack);
                    break;
                }
                case TdsEnums.SQLFEATUREEXTACK: 
                {                    
                    if (!TryProcessFeatureExtAck(stateObj)) {
                        return false;
                    }
                    break;
                }
                case TdsEnums.SQLFEDAUTHINFO:
                {
                    _connHandler._federatedAuthenticationInfoReceived = true;
                    SqlFedAuthInfo info;
                    Bid.Trace("<sc.TdsParser.TryRun|SEC> Received federated authentication info token\n");
                    if (!TryProcessFedAuthInfo(stateObj, tokenLength, out info)) {
                        return false;
                    }
                    _connHandler.OnFedAuthInfo(info);
                    break;
                }
                case TdsEnums.SQLSESSIONSTATE: 
                {
                    if (!TryProcessSessionState(stateObj, tokenLength,  _connHandler._currentSessionData)) {
                        return false;
                    }
                    break;
                }
                case TdsEnums.SQLCOLMETADATA:
                {
                    if (tokenLength != TdsEnums.VARNULL) {
                        _SqlMetaDataSet metadata;
                        if (!TryProcessMetaData(tokenLength, stateObj, out metadata, 
                                                cmdHandler != null ? cmdHandler.ColumnEncryptionSetting : SqlCommandColumnEncryptionSetting.UseConnectionSetting)) {
                            return false;
                        }
                        stateObj._cleanupMetaData = metadata;
                    }
                    else {
                        if (cmdHandler != null) {
                            stateObj._cleanupMetaData = cmdHandler.MetaData;
                        }
                    }

                    if (null != dataStream) {
                        byte peekedToken;
                        if (!stateObj.TryPeekByte(out peekedToken)) { // temporarily cache next byte
                            return false;
                        }

                        if (!dataStream.TrySetMetaData(stateObj._cleanupMetaData, (TdsEnums.SQLTABNAME == peekedToken || TdsEnums.SQLCOLINFO == peekedToken))) {
                            return false;
                        }
                    }
                    else if (null != bulkCopyHandler) {
                        bulkCopyHandler.SetMetaData(stateObj._cleanupMetaData);
                    }
                    break;
                }
                case TdsEnums.SQLROW:
                case TdsEnums.SQLNBCROW:
                {
                    Debug.Assert(stateObj._cleanupMetaData != null, "Reading a row, but the metadata is null");

                    if (token == TdsEnums.SQLNBCROW) {
                        if (!stateObj.TryStartNewRow(isNullCompressed: true, nullBitmapColumnsCount: stateObj._cleanupMetaData.Length)) {
                            return false;
                        }
                    }
                    else {
                        if (!stateObj.TryStartNewRow(isNullCompressed:false)) {
                            return false;
                        }
                    }

                    if (null != bulkCopyHandler) {
                        // 
                        if (!TryProcessRow(stateObj._cleanupMetaData, bulkCopyHandler.CreateRowBuffer(), bulkCopyHandler.CreateIndexMap(), stateObj)) {
                            return false;
                        }
                    }
                    else if (RunBehavior.ReturnImmediately != (RunBehavior.ReturnImmediately & runBehavior)) {
                        if (!TrySkipRow(stateObj._cleanupMetaData, stateObj)) { // skip rows
                            return false;
                        }
                    }
                    else {
                        dataReady = true;
                    }

                    if (_statistics != null) {
                        _statistics.WaitForDoneAfterRow = true;
                    }
                    break;
                }
                case TdsEnums.SQLRETURNSTATUS:
                    int status;
                    if (!stateObj.TryReadInt32(out status)) {
                        return false;
                    }
                    if (cmdHandler != null) {
                        cmdHandler.OnReturnStatus(status);
                    }
                    break;
                case TdsEnums.SQLRETURNVALUE:
                {
                    SqlReturnValue returnValue;
                    if (!TryProcessReturnValue(tokenLength, stateObj, out returnValue, 
                                               cmdHandler != null ? cmdHandler.ColumnEncryptionSetting : SqlCommandColumnEncryptionSetting.UseConnectionSetting)) {
                        return false;
                    }
                    if (cmdHandler != null) {
                        cmdHandler.OnReturnValue(returnValue, stateObj);
                    }
                    break;
                }
                case TdsEnums.SQLSSPI:
                {
                    // token length is length of SSPI data - call ProcessSSPI with it

                    Debug.Assert(stateObj._syncOverAsync, "ProcessSSPI does not support retry, do not attempt asynchronously");
                    stateObj._syncOverAsync = true;
                    
                    ProcessSSPI(tokenLength);
                    break;
                }
                case TdsEnums.SQLTABNAME:
                {
                    if (null != dataStream) {
                        MultiPartTableName[] tableNames;
                        if (!TryProcessTableName(tokenLength, stateObj, out tableNames)) {
                            return false;
                        }
                        dataStream.TableNames = tableNames;
                    }
                    else {
                        if (!stateObj.TrySkipBytes(tokenLength)) {
                            return false;
                        }
                    }
                    break;
                }

                default:
                    Debug.Assert(false, "Unhandled token:  " + token.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            Debug.Assert(stateObj._pendingData || !dataReady, "dataReady is set, but there is no pending data");
            }

            // Loop while data pending & runbehavior not return immediately, OR
            // if in attention case, loop while no more pending data & attention has not yet been
            // received.
            while ((stateObj._pendingData &&
                    (RunBehavior.ReturnImmediately != (RunBehavior.ReturnImmediately & runBehavior))) ||
                (!stateObj._pendingData && stateObj._attentionSent && !stateObj._attentionReceived));
            
#if DEBUG
            if ((stateObj._pendingData) && (!dataReady)) {
                byte token;
                if (!stateObj.TryPeekByte(out token)) {
                    return false;
                }
                Debug.Assert(IsValidTdsToken(token), string.Format("DataReady is false, but next token is not valid: {0,-2:X2}", token));
            }
#endif

            if (!stateObj._pendingData) {
                if (null != CurrentTransaction) {
                    CurrentTransaction.Activate();
                }
            }

            // if we recieved an attention (but this thread didn't send it) then
            // we throw an Operation Cancelled error
            if (stateObj._attentionReceived) {
                // Dev11 #344723: SqlClient stress hang System_Data!Tcp::ReadSync via a call to SqlDataReader::Close
                // Spin until SendAttention has cleared _attentionSending, this prevents a race condition between receiving the attention ACK and setting _attentionSent
                SpinWait.SpinUntil(() => !stateObj._attentionSending);

                Debug.Assert(stateObj._attentionSent, "Attention ACK has been received without attention sent");
                if (stateObj._attentionSent) {
                    // Reset attention state.
                    stateObj._attentionSent = false;
                    stateObj._attentionReceived = false;

                    if (RunBehavior.Clean != (RunBehavior.Clean & runBehavior) && !stateObj._internalTimeout) {
                        // Add attention error to collection - if not RunBehavior.Clean!
                        stateObj.AddError(new SqlError(0, 0, TdsEnums.MIN_ERROR_CLASS, _server, SQLMessage.OperationCancelled(), "", 0));
                    }
                }
            }

            if (stateObj.HasErrorOrWarning) {
                ThrowExceptionAndWarning(stateObj);
            }
            return true;
        }

        private bool TryProcessEnvChange(int tokenLength, TdsParserStateObject stateObj, out SqlEnvChange[] sqlEnvChange) {
            // There could be multiple environment change messages following this token.
            byte byteLength;
            int processedLength = 0;
            int nvalues = 0;
            SqlEnvChange[] envarray = new SqlEnvChange[3];  // Why is this hardcoded to 3?

            sqlEnvChange = null;

            while (tokenLength > processedLength) {

                if (nvalues >= envarray.Length) {
                    // This is a rare path. Most of the time we will have 1 or 2 envchange data streams.
                    SqlEnvChange[] newenvarray = new SqlEnvChange[envarray.Length + 3];

                    for (int ii = 0; ii < envarray.Length; ii++)
                        newenvarray[ii] = envarray[ii];

                    envarray = newenvarray;
                }

                SqlEnvChange env = new SqlEnvChange();

                if (!stateObj.TryReadByte(out env.type)) {
                    return false;
                }

                envarray[nvalues] = env;
                nvalues++;

                switch (env.type) {
                    case TdsEnums.ENV_DATABASE:
                    case TdsEnums.ENV_LANG:
                        if (!TryReadTwoStringFields(env, stateObj)) {
                            return false;
                        }
                        break;

                    case TdsEnums.ENV_CHARSET:
                        // we copied this behavior directly from luxor - see charset envchange
                        // section from sqlctokn.c
                        Debug.Assert(!_isShiloh, "Received ENV_CHARSET on non 7.0 server!");
                        if (!TryReadTwoStringFields(env, stateObj)) {
                            return false;
                        }
                        if (env.newValue == TdsEnums.DEFAULT_ENGLISH_CODE_PAGE_STRING) {
                            _defaultCodePage = TdsEnums.DEFAULT_ENGLISH_CODE_PAGE_VALUE;
                            _defaultEncoding = System.Text.Encoding.GetEncoding(_defaultCodePage);
                        }
                        else {
                            Debug.Assert(env.newValue.Length > TdsEnums.CHARSET_CODE_PAGE_OFFSET, "TdsParser.ProcessEnvChange(): charset value received with length <=10");

                            string stringCodePage = env.newValue.Substring(TdsEnums.CHARSET_CODE_PAGE_OFFSET);

                            _defaultCodePage = Int32.Parse(stringCodePage, NumberStyles.Integer, CultureInfo.InvariantCulture);
                            _defaultEncoding = System.Text.Encoding.GetEncoding(_defaultCodePage);
                        }

                        break;

                    case TdsEnums.ENV_PACKETSIZE:
                        // take care of packet size right here
                        Debug.Assert(stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
                        if (!TryReadTwoStringFields(env, stateObj)) {
                            // Changing packet size does not support retry, should not pend"
                            throw SQL.SynchronousCallMayNotPend(); 
                        }
                        // Only set on physical state object - this should only occur on LoginAck prior
                        // to MARS initialization!
                        Int32 packetSize = Int32.Parse(env.newValue, NumberStyles.Integer, CultureInfo.InvariantCulture);

                        if (_physicalStateObj.SetPacketSize(packetSize)) {
                            // If packet size changed, we need to release our SNIPackets since
                            // those are tied to packet size of connection.
                            _physicalStateObj.ClearAllWritePackets();

                            // Update SNI ConsumerInfo value to be resulting packet size
                            UInt32 unsignedPacketSize = (UInt32) packetSize;
                            UInt32 result = SNINativeMethodWrapper.SNISetInfo(_physicalStateObj.Handle, SNINativeMethodWrapper.QTypes.SNI_QUERY_CONN_BUFSIZE, ref unsignedPacketSize);

                            Debug.Assert(result == TdsEnums.SNI_SUCCESS, "Unexpected failure state upon calling SNISetInfo");
                        }

                        break;

                    case TdsEnums.ENV_LOCALEID:
                        // 

                        if (!TryReadTwoStringFields(env, stateObj)) {
                            return false;
                        }
                        _defaultLCID = Int32.Parse(env.newValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
                        break;

                    case TdsEnums.ENV_COMPFLAGS:
                        if (!TryReadTwoStringFields(env, stateObj)) {
                            return false;
                        }
                        break;

                    case TdsEnums.ENV_COLLATION:
                        Debug.Assert(env.newLength == 5 || env.newLength == 0, "Improper length in new collation!");
                        if (!stateObj.TryReadByte(out byteLength)) {
                            return false;
                        }
                        env.newLength = byteLength;
                        if (env.newLength == 5) {
                            if (!TryProcessCollation(stateObj, out env.newCollation)) {
                                return false;
                            }

                            // give the parser the new collation values in case parameters don't specify one
                            _defaultCollation = env.newCollation;
                            int newCodePage = GetCodePage(env.newCollation, stateObj);
                            if (newCodePage != _defaultCodePage) {
                                _defaultCodePage = newCodePage;
                                _defaultEncoding = System.Text.Encoding.GetEncoding(_defaultCodePage);
                            }
                            _defaultLCID = env.newCollation.LCID;
                        }

                        if (!stateObj.TryReadByte(out byteLength)) {
                            return false;
                        }
                        env.oldLength = byteLength;
                        Debug.Assert(env.oldLength == 5 || env.oldLength == 0, "Improper length in old collation!");
                        if (env.oldLength == 5) {
                            if (!TryProcessCollation(stateObj, out env.oldCollation)) {
                                return false;
                            }
                        }

                        env.length = 3 + env.newLength + env.oldLength;
                        break;

                    case TdsEnums.ENV_BEGINTRAN:
                    case TdsEnums.ENV_COMMITTRAN:
                    case TdsEnums.ENV_ROLLBACKTRAN:
                    case TdsEnums.ENV_ENLISTDTC:
                    case TdsEnums.ENV_DEFECTDTC:
                    case TdsEnums.ENV_TRANSACTIONENDED:
                        Debug.Assert(_isYukon, "Received new ENVCHANGE transaction/DTC token on pre 9.0 server!");

                        if (!stateObj.TryReadByte(out byteLength)) {
                            return false;
                        }
                        env.newLength = byteLength;
                        Debug.Assert(env.newLength == 0 || env.newLength == 8, "Improper length for new transaction id!");

                        if (env.newLength > 0) {
                            if (!stateObj.TryReadInt64(out env.newLongValue)) {
                                return false;
                            }
                            Debug.Assert(env.newLongValue != SqlInternalTransaction.NullTransactionId, "New transaction id is null?"); // the server guarantees that zero is an invalid transaction id.
                        }
                        else {
                            env.newLongValue = SqlInternalTransaction.NullTransactionId; // the server guarantees that zero is an invalid transaction id.
                        }
                        
                        if (!stateObj.TryReadByte(out byteLength)) {
                            return false;
                        }
                        env.oldLength = byteLength;
                        Debug.Assert(env.oldLength == 0 || env.oldLength == 8, "Improper length for old transaction id!");

                        if (env.oldLength > 0) {
                            if (!stateObj.TryReadInt64(out env.oldLongValue)) {
                                return false;
                            }
                            Debug.Assert(env.oldLongValue != SqlInternalTransaction.NullTransactionId, "Old transaction id is null?"); // the server guarantees that zero is an invalid transaction id.
                        }
                        else {
                            env.oldLongValue = SqlInternalTransaction.NullTransactionId; // the server guarantees that zero is an invalid transaction id.
                        }

                        // env.length includes 1 byte type token
                        env.length = 3 + env.newLength + env.oldLength;
                        break;

                    case TdsEnums.ENV_LOGSHIPNODE:
                        // env.newBinValue is secondary node, env.oldBinValue is witness node
                        // comes before LoginAck so we can't assert this
                        if (!TryReadTwoStringFields(env, stateObj)) {
                            return false;
                        }
                        break;

                    case TdsEnums.ENV_PROMOTETRANSACTION:
                        Debug.Assert(_isYukon, "Received new ENVCHANGE tokens on pre 9.0 server!");

                        if (!stateObj.TryReadInt32(out env.newLength)) { // new value has 4 byte length
                            return false;
                        }
                        env.newBinValue = new byte[env.newLength];
                        if (!stateObj.TryReadByteArray(env.newBinValue, 0, env.newLength)) { // read new value with 4 byte length
                            return false;
                        }

                        if (!stateObj.TryReadByte(out byteLength)) {
                            return false;
                        }
                        env.oldLength = byteLength;
                        Debug.Assert(0 == env.oldLength, "old length should be zero");

                        // env.length includes 1 byte for type token
                        env.length = 5 + env.newLength;
                        break;

                    case TdsEnums.ENV_TRANSACTIONMANAGERADDRESS:
                    case TdsEnums.ENV_SPRESETCONNECTIONACK:
                        // 
                        Debug.Assert(_isYukon, "Received new ENVCHANGE tokens on pre 9.0 server!");
                        if (!TryReadTwoBinaryFields(env, stateObj)) {
                            return false;
                        }
                        break;
                        
                    case TdsEnums.ENV_USERINSTANCE:
                        Debug.Assert(!_isYukon, "Received ENV_USERINSTANCE on non 9.0 server!");
                        if (!TryReadTwoStringFields(env, stateObj)) {
                            return false;
                        }
                        break;
                        
                    case TdsEnums.ENV_ROUTING:
                        ushort newLength;
                        if (!stateObj.TryReadUInt16(out newLength)) {
                            return false;
                        }
                        env.newLength = newLength;
                        byte protocol;
                        if (!stateObj.TryReadByte(out protocol)) {
                            return false;
                        }
                        ushort port;
                        if (!stateObj.TryReadUInt16(out port)) {
                            return false;
                        }
                        UInt16 serverLen;
                        if (!stateObj.TryReadUInt16(out serverLen)) {
                            return false;
                        }
                        string serverName;
                        if (!stateObj.TryReadString(serverLen, out serverName)) {
                            return false;
                        }
                        env.newRoutingInfo = new RoutingInfo(protocol, port, serverName);
                        UInt16 oldLength;
                        if (!stateObj.TryReadUInt16(out oldLength)) {
                            return false;
                        }
                        if (!stateObj.TrySkipBytes(oldLength)) {
                            return false;
                        }
                        env.length = env.newLength + oldLength + 5; // 5=2*sizeof(UInt16)+sizeof(byte) [token+newLength+oldLength]
                        break;                        

                    default:
                        Debug.Assert(false, "Unknown environment change token: " + env.type);
                        break;
                }
                processedLength += env.length;
            }

            sqlEnvChange = envarray;
            return true;
        }

        private bool TryReadTwoBinaryFields(SqlEnvChange env, TdsParserStateObject stateObj) {
            // Used by ProcessEnvChangeToken
            byte byteLength;
            if (!stateObj.TryReadByte(out byteLength)) {
                return false;
            }
            env.newLength = byteLength;
            env.newBinValue = new byte[env.newLength];
            if (!stateObj.TryReadByteArray(env.newBinValue, 0, env.newLength)) {
                return false;
            }
            if (!stateObj.TryReadByte(out byteLength)) {
                return false;
            }
            env.oldLength = byteLength;
            env.oldBinValue = new byte[env.oldLength];
            if (!stateObj.TryReadByteArray(env.oldBinValue, 0, env.oldLength)) {
                return false;
            }

            // env.length includes 1 byte type token
            env.length = 3 + env.newLength + env.oldLength;
            return true;
        }

        private bool TryReadTwoStringFields(SqlEnvChange env, TdsParserStateObject stateObj) {
            // Used by ProcessEnvChangeToken
            byte newLength, oldLength;
            string newValue, oldValue;
            if (!stateObj.TryReadByte(out newLength)) {
                return false;
            }
            if (!stateObj.TryReadString(newLength, out newValue)) {
                return false;
            }
            if (!stateObj.TryReadByte(out oldLength)) {
                return false;
            }
            if (!stateObj.TryReadString(oldLength, out oldValue)) {
                return false;
            }
            
            env.newLength = newLength;
            env.newValue = newValue;
            env.oldLength = oldLength;
            env.oldValue = oldValue;

            // env.length includes 1 byte type token
            env.length = 3 + env.newLength * 2 + env.oldLength * 2;
            return true;
        }

        private bool TryProcessDone(SqlCommand cmd, SqlDataReader reader, ref RunBehavior run, TdsParserStateObject stateObj) {
            ushort curCmd;
            ushort status;
            int count;

            // Can't retry TryProcessDone
            stateObj._syncOverAsync = true;

            // status
            // command
            // rowcount (valid only if DONE_COUNT bit is set)

            if (!stateObj.TryReadUInt16(out status)) {
                return false;
            }
            if (!stateObj.TryReadUInt16(out curCmd)) {
                return false;
            }

            if (_isYukon) {
                long longCount;
                if (!stateObj.TryReadInt64(out longCount)) {
                    return false;
                }
                count = (int) longCount;
            }
            else {
                if (!stateObj.TryReadInt32(out count)) {
                    return false;
                }
                // If we haven't yet completed processing login token stream yet, we may be talking to a Yukon server
                // In that case we still have to read another 4 bytes
                // But don't try to read beyond the TDS stream in this case, because it generates errors if login failed.
                if ( _state == TdsParserState.OpenNotLoggedIn) {
                    // Login incomplete, if we are reading from Yukon we need to read another int
                    if (stateObj._inBytesRead > stateObj._inBytesUsed) {
                        byte b;
                        if (!stateObj.TryPeekByte(out b)) {
                            return false;
                        }
                        if (b == 0) {
                            // This is an invalid token value
                            if (!stateObj.TryReadInt32(out count)) {
                                return false;
                            }
                        }
                    }
                }
            }

            // We get a done token with the attention bit set
            if (TdsEnums.DONE_ATTN == (status & TdsEnums.DONE_ATTN)) {
                Debug.Assert(TdsEnums.DONE_MORE != (status & TdsEnums.DONE_MORE),"Not expecting DONE_MORE when receiving DONE_ATTN");
                Debug.Assert(stateObj._attentionSent, "Received attention done without sending one!");
                stateObj._attentionReceived = true;
                Debug.Assert(stateObj._inBytesUsed == stateObj._inBytesRead && stateObj._inBytesPacket == 0, "DONE_ATTN received with more data left on wire");
            }
            if ((null != cmd) && (TdsEnums.DONE_COUNT == (status & TdsEnums.DONE_COUNT))) {
                if (curCmd != TdsEnums.SELECT) {
                    if (cmd.IsDescribeParameterEncryptionRPCCurrentlyInProgress) {
                        // The below line is used only for debug asserts and not exposed publicly or impacts functionality otherwise.
                        cmd.RowsAffectedByDescribeParameterEncryption = count;
                    }
                    else {
                        cmd.InternalRecordsAffected = count;
                    }
                }
                // Skip the bogus DONE counts sent by the server
                if (stateObj._receivedColMetaData || (curCmd != TdsEnums.SELECT)) {
                    cmd.OnStatementCompleted(count);
                }
            }

            stateObj._receivedColMetaData = false;

            // Surface exception for DONE_ERROR in the case we did not receive an error token
            // in the stream, but an error occurred.  In these cases, we throw a general server error.  The
            // situations where this can occur are: an invalid buffer received from client, login error
            // and the server refused our connection, and the case where we are trying to log in but
            // the server has reached its max connection limit.  Bottom line, we need to throw general
            // error in the cases where we did not receive a error token along with the DONE_ERROR.
            if ((TdsEnums.DONE_ERROR == (TdsEnums.DONE_ERROR & status)) && stateObj.ErrorCount == 0 &&
                  stateObj._errorTokenReceived == false && (RunBehavior.Clean != (RunBehavior.Clean & run))) {
                stateObj.AddError(new SqlError(0, 0, TdsEnums.MIN_ERROR_CLASS, _server, SQLMessage.SevereError(), "", 0));

                if (null != reader) { // SQL BU DT 269516
                    if (!reader.IsInitialized) {
                        run = RunBehavior.UntilDone;
                    }
                }
             }

            // Similar to above, only with a more severe error.  In this case, if we received
            // the done_srverror, this exception will be added to the collection regardless.
            // MDAC #93896.  Also, per Ashwin, the server will always break the connection in this case.
            if ((TdsEnums.DONE_SRVERROR == (TdsEnums.DONE_SRVERROR & status)) && (RunBehavior.Clean != (RunBehavior.Clean & run))) {
                stateObj.AddError(new SqlError(0, 0, TdsEnums.FATAL_ERROR_CLASS, _server, SQLMessage.SevereError(), "", 0));

                if (null != reader) { // SQL BU DT 269516
                    if (!reader.IsInitialized) {
                        run = RunBehavior.UntilDone;
                    }
                }
            }

            ProcessSqlStatistics(curCmd, status, count);

            // stop if the DONE_MORE bit isn't set (see above for attention handling)
            if (TdsEnums.DONE_MORE != (status & TdsEnums.DONE_MORE)) {
                stateObj._errorTokenReceived = false;
                if (stateObj._inBytesUsed >= stateObj._inBytesRead) {
                    stateObj._pendingData = false;
                }
            }

            // _pendingData set by e.g. 'TdsExecuteSQLBatch'
            // _hasOpenResult always set to true by 'WriteMarsHeader'
            //
            if (!stateObj._pendingData && stateObj._hasOpenResult) {
/*
                Debug.Assert(!((sqlTransaction != null               && _distributedTransaction != null) ||
                               (_userStartedLocalTransaction != null && _distributedTransaction != null))
                              , "ProcessDone - have both distributed and local transactions not null!");
*/ // WebData 112722

                stateObj.DecrementOpenResultCount();
            }

            return true;
        }

        private void ProcessSqlStatistics(ushort curCmd, ushort status, int count) {
            // SqlStatistics bookkeeping stuff
            //
            if (null != _statistics) {
                // any done after row(s) counts as a resultset
                if (_statistics.WaitForDoneAfterRow) {
                    _statistics.SafeIncrement(ref _statistics._sumResultSets);
                    _statistics.WaitForDoneAfterRow = false;
                }

                // clear row count DONE_COUNT flag is not set
                if (!(TdsEnums.DONE_COUNT == (status & TdsEnums.DONE_COUNT))) {
                    count = 0;
                }

                switch (curCmd) {
                    case TdsEnums.INSERT:
                    case TdsEnums.DELETE:
                    case TdsEnums.UPDATE:
                    case TdsEnums.MERGE:
                        _statistics.SafeIncrement(ref _statistics._iduCount);
                        _statistics.SafeAdd(ref _statistics._iduRows, count);
                        if (!_statisticsIsInTransaction) {
                            _statistics.SafeIncrement(ref _statistics._transactions);
                        }

                        break;

                    case TdsEnums.SELECT:
                        _statistics.SafeIncrement(ref _statistics._selectCount);
                        _statistics.SafeAdd(ref _statistics._selectRows, count);
                        break;

                    case TdsEnums.BEGINXACT:
                        if (!_statisticsIsInTransaction) {
                            _statistics.SafeIncrement(ref _statistics._transactions);
                        }
                        _statisticsIsInTransaction = true;
                        break;

                    case TdsEnums.OPENCURSOR:
                        _statistics.SafeIncrement(ref _statistics._cursorOpens);
                        break;

                    case TdsEnums.ABORT:
                        _statisticsIsInTransaction = false;
                        break;

                    case TdsEnums.ENDXACT:
                        _statisticsIsInTransaction = false;
                        break;
                } // switch
            }
            else {
                switch (curCmd) {
                    case TdsEnums.BEGINXACT:
                        _statisticsIsInTransaction = true;
                        break;

                    case TdsEnums.ABORT:
                    case TdsEnums.ENDXACT:
                        _statisticsIsInTransaction = false;
                        break;
                }
            }
        }

        private bool TryProcessFeatureExtAck(TdsParserStateObject stateObj) {
            // read feature ID
            byte featureId;
            do {
                if (!stateObj.TryReadByte(out featureId)) {
                    return false;
                }
                if (featureId != TdsEnums.FEATUREEXT_TERMINATOR) {
                    UInt32 dataLen;
                    if (!stateObj.TryReadUInt32(out dataLen)) {
                        return false;
                    }                    
                    byte[] data = new byte[dataLen];
                    if (dataLen > 0) {
                        if (!stateObj.TryReadByteArray(data, 0, checked ((int)dataLen))) {
                            return false;
                        }
                    }
                    _connHandler.OnFeatureExtAck(featureId, data);
                }
            } while (featureId != TdsEnums.FEATUREEXT_TERMINATOR);

            // Check if column encryption was on and feature wasn't acknowledged.
            if (_connHandler.ConnectionOptions.ColumnEncryptionSetting == SqlConnectionColumnEncryptionSetting.Enabled && !IsColumnEncryptionSupported) {
                throw SQL.TceNotSupported ();
            }

            return true;
        }

        private bool TryProcessSessionState(TdsParserStateObject stateObj, int length, SessionData sdata) {
            if (length < 5) {
                throw SQL.ParsingErrorLength(ParsingErrorState.SessionStateLengthTooShort, length);
            }
            UInt32 seqNum;
            if (!stateObj.TryReadUInt32(out seqNum)) {
                return false;
            }
            if (seqNum == UInt32.MaxValue) {
                _connHandler.DoNotPoolThisConnection();
            }
            byte status;
            if (!stateObj.TryReadByte(out status)) {
                return false;
            }
            if (status > 1) {
                throw SQL.ParsingErrorStatus(ParsingErrorState.SessionStateInvalidStatus, status);
            }
            bool recoverable = status != 0;
            length -= 5;           
            while (length > 0) {
                byte stateId;
                if (!stateObj.TryReadByte(out stateId)) {
                    return false;
                }
                int stateLen;
                byte stateLenByte;
                if (!stateObj.TryReadByte(out stateLenByte)) {
                    return false;
                }
                if (stateLenByte < 0xFF) {
                    stateLen = stateLenByte;
                }
                else {
                    if (!stateObj.TryReadInt32(out stateLen)) {
                        return false;
                    }
                }
                byte[] buffer = null;
                lock (sdata._delta) {
                    if (sdata._delta[stateId] == null) {
                        buffer = new byte[stateLen];
                        sdata._delta[stateId] = new SessionStateRecord { _version = seqNum, _dataLength = stateLen, _data = buffer, _recoverable = recoverable };
                        sdata._deltaDirty = true;
                        if (!recoverable) {
                            checked { sdata._unrecoverableStatesCount++; }
                        }
                    }
                    else {
                        if (sdata._delta[stateId]._version <= seqNum) {
                            SessionStateRecord sv = sdata._delta[stateId];
                            sv._version = seqNum;
                            sv._dataLength = stateLen;
                            if (sv._recoverable != recoverable) {
                                if (recoverable) {
                                    Debug.Assert(sdata._unrecoverableStatesCount > 0, "Unrecoverable states count >0");
                                    sdata._unrecoverableStatesCount--;
                                }
                                else {
                                    checked { sdata._unrecoverableStatesCount++; }
                                }
                                sv._recoverable = recoverable;
                            }
                            buffer = sv._data;
                            if (buffer.Length < stateLen) {
                                buffer = new byte[stateLen];
                                sv._data = buffer;
                            }
                        }
                    }
                }
                if (buffer != null) {
                    if (!stateObj.TryReadByteArray(buffer, 0, stateLen)) {
                        return false;
                    }
                }
                else {
                    if (!stateObj.TrySkipBytes(stateLen))
                        return false;
                }

                if (stateLenByte < 0xFF) {
                    length -= 2 + stateLen;
                }
                else {
                    length -= 6 + stateLen;
                }
            }
            sdata.AssertUnrecoverableStateCountIsCorrect();

            return true;
        }

        private bool TryProcessLoginAck(TdsParserStateObject stateObj, out SqlLoginAck sqlLoginAck) {
            SqlLoginAck a = new SqlLoginAck();

            sqlLoginAck = null;

            // read past interface type and version
            if (!stateObj.TrySkipBytes(1)) {
                return false;
            }

            byte[] b = new byte[TdsEnums.VERSION_SIZE];
            if (!stateObj.TryReadByteArray(b, 0, b.Length)) {
                return false;
            }
            a.tdsVersion = (UInt32)((((((b[0]<<8)|b[1])<<8)|b[2])<<8)|b[3]); // bytes are in motorola order (high byte first)
            UInt32 majorMinor = a.tdsVersion & 0xff00ffff;
            UInt32 increment  = (a.tdsVersion >> 16) & 0xff;

            // Server responds:
            // 0x07000000 -> Sphinx         // Notice server response format is different for bwd compat
            // 0x07010000 -> Shiloh RTM     // Notice server response format is different for bwd compat
            // 0x71000001 -> Shiloh SP1
            // 0x72xx0002 -> Yukon RTM
            // information provided by S. Ashwin

            switch (majorMinor) {
                case TdsEnums.SPHINXORSHILOH_MAJOR<<24|TdsEnums.DEFAULT_MINOR:    // Sphinx & Shiloh RTM
                    // note that sphinx and shiloh_rtm can only be distinguished by the increment
                    switch (increment) {
                        case TdsEnums.SHILOH_INCREMENT:
                            _isShiloh = true;
                            break;
                        case TdsEnums.SPHINX_INCREMENT:
                            // no flag will be set
                            break;
                        default:
                            throw SQL.InvalidTDSVersion();
                    }
                    break;
                case TdsEnums.SHILOHSP1_MAJOR<<24|TdsEnums.SHILOHSP1_MINOR: // Shiloh SP1
                    if (increment != TdsEnums.SHILOHSP1_INCREMENT) { throw SQL.InvalidTDSVersion(); }
                    _isShilohSP1 = true;
                    break;
                case TdsEnums.YUKON_MAJOR<<24|TdsEnums.YUKON_RTM_MINOR:     // Yukon
                    if (increment != TdsEnums.YUKON_INCREMENT) { throw SQL.InvalidTDSVersion(); }
                    _isYukon = true;
                    break;
                case TdsEnums.KATMAI_MAJOR<<24|TdsEnums.KATMAI_MINOR:
                    if (increment != TdsEnums.KATMAI_INCREMENT) { throw SQL.InvalidTDSVersion(); }
                    _isKatmai = true;
                    break;
                case TdsEnums.DENALI_MAJOR << 24|TdsEnums.DENALI_MINOR:
                    if (increment != TdsEnums.DENALI_INCREMENT) { throw SQL.InvalidTDSVersion(); }
                    _isDenali = true;
                    break;
                default:
                    throw SQL.InvalidTDSVersion();
            }

            _isKatmai       |= _isDenali;
            _isYukon        |= _isKatmai;
            _isShilohSP1    |= _isYukon;            // includes all lower versions
            _isShiloh       |= _isShilohSP1;        //

            a.isVersion8 = _isShiloh;

            stateObj._outBytesUsed = stateObj._outputHeaderLen;
            byte len;
            if (!stateObj.TryReadByte(out len)) {
                return false;
            }

            if (!stateObj.TryReadString(len, out a.programName)) {
                return false;
            }
            if (!stateObj.TryReadByte(out a.majorVersion)) {
                return false;
            }
            if (!stateObj.TryReadByte(out a.minorVersion)) {
                return false;
            }
            byte buildNumHi, buildNumLo;
            if (!stateObj.TryReadByte(out buildNumHi)) {
                return false;
            }
            if (!stateObj.TryReadByte(out buildNumLo)) {
                return false;
            }

            a.buildNum = (short)((buildNumHi << 8) + buildNumLo);

            Debug.Assert(_state == TdsParserState.OpenNotLoggedIn, "ProcessLoginAck called with state not TdsParserState.OpenNotLoggedIn");
            _state = TdsParserState.OpenLoggedIn;

            if (_isYukon) {
                if (_fMARS) {
                    _resetConnectionEvent = new AutoResetEvent(true);
                }
            }

            // Fail if SSE UserInstance and we have not received this info.
            if ( _connHandler.ConnectionOptions.UserInstance && 
                ADP.IsEmpty(_connHandler.InstanceName)) {
                stateObj.AddError(new SqlError(0, 0, TdsEnums.FATAL_ERROR_CLASS, Server, SQLMessage.UserInstanceFailure(), "", 0));
                ThrowExceptionAndWarning(stateObj);
            }

            sqlLoginAck = a;
            return true;
        }

        private bool TryProcessFedAuthInfo(TdsParserStateObject stateObj, int tokenLen, out SqlFedAuthInfo sqlFedAuthInfo) {
            sqlFedAuthInfo = null;
            SqlFedAuthInfo tempFedAuthInfo = new SqlFedAuthInfo();

            // Skip reading token length, since it has already been read in caller

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> FEDAUTHINFO token stream length = {0}\n", tokenLen);
            }

            if (tokenLen < sizeof(uint)) {
                // the token must at least contain a DWORD indicating the number of info IDs
                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> FEDAUTHINFO token stream length too short for CountOfInfoIDs.\n");
                throw SQL.ParsingErrorLength(ParsingErrorState.FedAuthInfoLengthTooShortForCountOfInfoIds, tokenLen);
            }

            // read how many FedAuthInfo options there are
            uint optionsCount;
            if (!stateObj.TryReadUInt32(out optionsCount)) {
                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> Failed to read CountOfInfoIDs in FEDAUTHINFO token stream.\n");
                throw SQL.ParsingError(ParsingErrorState.FedAuthInfoFailedToReadCountOfInfoIds);
            }
            tokenLen -= sizeof(uint); // remaining length is shortened since we read optCount

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> CountOfInfoIDs = {0}\n", optionsCount.ToString(CultureInfo.InvariantCulture));
            }

            if (tokenLen > 0) {
                // read the rest of the token
                byte[] tokenData = new byte[tokenLen];
                int totalRead = 0;
                bool successfulRead = stateObj.TryReadByteArray(tokenData, 0, tokenLen, out totalRead);

                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> Read rest of FEDAUTHINFO token stream: {0}\n", BitConverter.ToString(tokenData, 0, totalRead));
                }

                if (!successfulRead || totalRead != tokenLen) {
                    Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> Failed to read FEDAUTHINFO token stream. Attempted to read {0} bytes, actually read {1}\n", tokenLen, totalRead);
                    throw SQL.ParsingError(ParsingErrorState.FedAuthInfoFailedToReadTokenStream);
                }

                // each FedAuthInfoOpt is 9 bytes:
                //    1 byte for FedAuthInfoID
                //    4 bytes for FedAuthInfoDataLen
                //    4 bytes for FedAuthInfoDataOffset
                // So this is the index in tokenData for the i-th option
                const uint optionSize = 9;

                // the total number of bytes for all FedAuthInfoOpts together
                uint totalOptionsSize = checked(optionsCount * optionSize);

                for (uint i = 0; i < optionsCount; i++) {
                    uint currentOptionOffset = checked(i * optionSize);

                    byte id = tokenData[currentOptionOffset];
                    uint dataLen = BitConverter.ToUInt32(tokenData, checked((int)(currentOptionOffset + 1)));
                    uint dataOffset = BitConverter.ToUInt32(tokenData, checked((int)(currentOptionOffset + 5)));

                    if (Bid.AdvancedOn) {
                        Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> FedAuthInfoOpt: ID={0}, DataLen={1}, Offset={2}\n", id, dataLen.ToString(CultureInfo.InvariantCulture), dataOffset.ToString(CultureInfo.InvariantCulture));
                    }

                    // offset is measured from optCount, so subtract to make offset measured
                    // from the beginning of tokenData
                    checked {
                        dataOffset -= sizeof(uint);
                    }

                    // if dataOffset points to a region within FedAuthInfoOpt or after the end of the token, throw
                    if (dataOffset < totalOptionsSize || dataOffset >= tokenLen) {
                        Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> FedAuthInfoDataOffset points to an invalid location.\n");
                        throw SQL.ParsingErrorOffset(ParsingErrorState.FedAuthInfoInvalidOffset, unchecked((int)dataOffset));
                    }

                    // try to read data and throw if the arguments are bad, meaning the server sent us a bad token
                    string data;
                    try {
                        data = System.Text.Encoding.Unicode.GetString(tokenData, checked((int)dataOffset), checked((int)dataLen));
                    }
                    catch (ArgumentOutOfRangeException e) {
                        Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> Failed to read FedAuthInfoData.\n");
                        throw SQL.ParsingError(ParsingErrorState.FedAuthInfoFailedToReadData, e);
                    }
                    catch (ArgumentException e) {
                        Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> FedAuthInfoData is not in unicode format.\n");
                        throw SQL.ParsingError(ParsingErrorState.FedAuthInfoDataNotUnicode, e);
                    }

                    if (Bid.AdvancedOn) {
                        Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> FedAuthInfoData: {0}\n", data);
                    }

                    // store data in tempFedAuthInfo
                    switch ((TdsEnums.FedAuthInfoId)id) {
                        case TdsEnums.FedAuthInfoId.Spn:
                            tempFedAuthInfo.spn = data;
                            break;
                        case TdsEnums.FedAuthInfoId.Stsurl:
                            tempFedAuthInfo.stsurl = data;
                            break;
                        default:
                            if (Bid.AdvancedOn) {
                                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> Ignoring unknown federated authentication info option: {0}\n", id);
                            }
                            break;
                    }
                }
            }
            else {
                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> FEDAUTHINFO token stream is not long enough to contain the data it claims to.\n");
                throw SQL.ParsingErrorLength(ParsingErrorState.FedAuthInfoLengthTooShortForData, tokenLen);
            }

            Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo> Processed FEDAUTHINFO token stream: {0}\n", tempFedAuthInfo.ToString());

            if (String.IsNullOrWhiteSpace(tempFedAuthInfo.stsurl) || String.IsNullOrWhiteSpace(tempFedAuthInfo.spn)) {
                // We should be receiving both stsurl and spn
                Bid.Trace("<sc.TdsParser.TryProcessFedAuthInfo|ERR> FEDAUTHINFO token stream does not contain both STSURL and SPN.\n");
                throw SQL.ParsingError(ParsingErrorState.FedAuthInfoDoesNotContainStsurlAndSpn);
            }

            sqlFedAuthInfo = tempFedAuthInfo;
            return true;
        }

        internal bool TryProcessError(byte token, TdsParserStateObject stateObj, out SqlError error) {
            ushort shortLen;
            byte byteLen;
            int number;
            byte state;
            byte errorClass;

            error = null;

            if (!stateObj.TryReadInt32(out number)) {
                return false;
            }
            if (!stateObj.TryReadByte(out state)) {
                return false;
            }
            if (!stateObj.TryReadByte(out errorClass)) {
                return false;
            }

            Debug.Assert(((errorClass >= TdsEnums.MIN_ERROR_CLASS) && token == TdsEnums.SQLERROR) ||
                          ((errorClass < TdsEnums.MIN_ERROR_CLASS) && token == TdsEnums.SQLINFO), "class and token don't match!");

            if (!stateObj.TryReadUInt16(out shortLen)) {
                return false;
            }
            string message;
            if (!stateObj.TryReadString(shortLen, out message)) {
                return false;
            }

            if (!stateObj.TryReadByte(out byteLen)) {
                return false;
            }

            string server;

            // MDAC bug #49307 - server sometimes does not send over server field! In those cases
            // we will use our locally cached value.
            if (byteLen == 0) {
                server = _server;
            }
            else {
                if (!stateObj.TryReadString(byteLen, out server)) {
                    return false;
                }
            }

            if (!stateObj.TryReadByte(out byteLen)) {
                return false;
            }
            string procedure;
            if (!stateObj.TryReadString(byteLen, out procedure)) {
                return false;
            }

            int line;
            if (_isYukon) {
                if (!stateObj.TryReadInt32(out line)) {
                    return false;
                }
            }
            else {
                ushort shortLine;
                if (!stateObj.TryReadUInt16(out shortLine)) {
                    return false;
                }
                line = shortLine;
                // If we haven't yet completed processing login token stream yet, we may be talking to a Yukon server
                // In that case we still have to read another 2 bytes
                if ( _state == TdsParserState.OpenNotLoggedIn) {
                    // Login incomplete
                    byte b;
                    if (!stateObj.TryPeekByte(out b)) {
                        return false;
                    }
                    if (b == 0) {
                        // This is an invalid token value
                        ushort value;
                        if (!stateObj.TryReadUInt16(out value)) {
                            return false;
                        }
                        line = (line << 16) + value;
                    }
                }
            }

            error = new SqlError(number, state, errorClass, _server, message, procedure, line);
            return true;
        }


        internal bool TryProcessReturnValue(int length,
                                            TdsParserStateObject stateObj,
                                            out SqlReturnValue returnValue,
                                            SqlCommandColumnEncryptionSetting columnEncryptionSetting) {
            returnValue = null;
            SqlReturnValue rec = new SqlReturnValue();
            rec.length = length;        // In Yukon this length is -1
            if (_isYukon) {
                if (!stateObj.TryReadUInt16(out rec.parmIndex)) {
                    return false;
                }
            }
            byte len;
            if (!stateObj.TryReadByte(out len)) { // Length of parameter name
                return false;
            }

            rec.parameter = null;
            if (len > 0) {
                if (!stateObj.TryReadString(len, out rec.parameter)) {
                    return false;
                }
            }

            // read status and ignore
            byte ignored;
            if (!stateObj.TryReadByte(out ignored)) {
                return false;
            }

            UInt32 userType;

            // read user type - 4 bytes Yukon, 2 backwards
            if (IsYukonOrNewer) {
                if (!stateObj.TryReadUInt32(out userType)) {
                    return false;
                }
            }
            else {
                ushort userTypeShort;
                if (!stateObj.TryReadUInt16(out userTypeShort)) {
                    return false;
                }
                userType = userTypeShort;
            }

            // Read off the flags.
            // The first byte is ignored since it doesn't contain any interesting information.
            byte flags;
            if (!stateObj.TryReadByte(out flags)) {
                return false;
            }

            if (!stateObj.TryReadByte(out flags)) {
                return false;
            }

            // Check if the column is encrypted.
            if (_serverSupportsColumnEncryption) {
                rec.isEncrypted = (TdsEnums.IsEncrypted == (flags & TdsEnums.IsEncrypted));
            }

            // read the type
            byte tdsType;
            if (!stateObj.TryReadByte(out tdsType)) {
                return false;
            }

            // read the MaxLen
            // For xml datatpyes, there is no tokenLength
            int tdsLen;

            if (tdsType == TdsEnums.SQLXMLTYPE) {
                tdsLen = TdsEnums.SQL_USHORTVARMAXLEN;
            }
            else if (IsVarTimeTds(tdsType))
                tdsLen = 0;  // placeholder until we read the scale, just make sure it's not SQL_USHORTVARMAXLEN
            else if (tdsType == TdsEnums.SQLDATE) {
                tdsLen = 3;
            }
            else {
                if (!TryGetTokenLength(tdsType, stateObj, out tdsLen)) {
                    return false;
                }
            }

            rec.metaType = MetaType.GetSqlDataType(tdsType, userType, tdsLen);
            rec.type = rec.metaType.SqlDbType;

            // always use the nullable type for parameters if Shiloh or later
            // Sphinx sometimes sends fixed length return values
            if (_isShiloh) {
                rec.tdsType = rec.metaType.NullableType;
                rec.isNullable = true;
                if (tdsLen == TdsEnums.SQL_USHORTVARMAXLEN) {
                    Debug.Assert(_isYukon, "plp data from pre-Yukon server");
                    rec.metaType = MetaType.GetMaxMetaTypeFromMetaType(rec.metaType);
                }
            }
            else {      // For sphinx, keep the fixed type if that is what is returned
                if (rec.metaType.NullableType == tdsType)
                    rec.isNullable = true;

                rec.tdsType = (byte)tdsType;
            }

            if (rec.type == SqlDbType.Decimal) {
                if (!stateObj.TryReadByte(out rec.precision)) {
                    return false;
                }
                if (!stateObj.TryReadByte(out rec.scale)) {
                    return false;
                }
            }

            if (rec.metaType.IsVarTime) {
                if (!stateObj.TryReadByte(out rec.scale)) {
                    return false;
                }
            }

            if (tdsType == TdsEnums.SQLUDT) {
                if (!TryProcessUDTMetaData((SqlMetaDataPriv) rec, stateObj)) {
                    return false;
                }
            }

            if (rec.type == SqlDbType.Xml) {
                // Read schema info
                byte schemapresent;
                if (!stateObj.TryReadByte(out schemapresent)) {
                    return false;
                }

                if ((schemapresent & 1) != 0) {
                    if (!stateObj.TryReadByte(out len)) {
                        return false;
                    }
                    if (len != 0) {
                        if (!stateObj.TryReadString(len, out rec.xmlSchemaCollectionDatabase)) {
                            return false;
                        }
                    }

                    if (!stateObj.TryReadByte(out len)) {
                        return false;
                    }
                    if (len != 0) {
                        if (!stateObj.TryReadString(len, out rec.xmlSchemaCollectionOwningSchema)) {
                            return false;
                        }
                    }

                    short slen;
                    if (!stateObj.TryReadInt16(out slen)) {
                        return false;
                    }

                    if (slen != 0) {
                        if (!stateObj.TryReadString(slen, out rec.xmlSchemaCollectionName)) {
                            return false;
                        }
                    }

                }
            }
            else if (_isShiloh && rec.metaType.IsCharType) {
                // read the collation for 8.x servers
                if (!TryProcessCollation(stateObj, out rec.collation)) {
                    return false;
                }

                int codePage = GetCodePage(rec.collation, stateObj);

                // if the column lcid is the same as the default, use the default encoder
                if (codePage == _defaultCodePage) {
                    rec.codePage = _defaultCodePage;
                    rec.encoding = _defaultEncoding;
                }
                else {
                    rec.codePage = codePage;
                    rec.encoding = System.Text.Encoding.GetEncoding(rec.codePage);
                }
            }

            // For encrypted parameters, read the unencrypted type and encryption information.
            if (_serverSupportsColumnEncryption && rec.isEncrypted) {
                if (!TryProcessTceCryptoMetadata(stateObj, rec, cipherTable: null, columnEncryptionSetting: columnEncryptionSetting, isReturnValue: true)) {
                    return false;
                }
            }

            // for now we coerce return values into a SQLVariant, not good...
            bool isNull = false;
            ulong valLen;
            if (!TryProcessColumnHeaderNoNBC(rec, stateObj, out isNull, out valLen)) {
                return false;
            }

            // always read as sql types
            Debug.Assert(valLen < (ulong)(Int32.MaxValue), "ProcessReturnValue received data size > 2Gb");

            int intlen = valLen > (ulong)(Int32.MaxValue) ? Int32.MaxValue : (int)valLen;

            if (rec.metaType.IsPlp) {
                intlen = Int32.MaxValue;    // If plp data, read it all
            }

            if (isNull) {
                GetNullSqlValue(rec.value, rec, SqlCommandColumnEncryptionSetting.Disabled, _connHandler);
            }
            else {
                // We should never do any decryption here, so pass disabled as the command encryption override.
                // We only read the binary value and decryption will be performed by OnReturnValue().
                if (!TryReadSqlValue(rec.value, rec, intlen, stateObj, SqlCommandColumnEncryptionSetting.Disabled, columnName:null /*Not used*/)) {
                    return false;
                }
            }

            returnValue = rec;
            return true;
        }

        internal bool TryProcessTceCryptoMetadata (TdsParserStateObject stateObj, 
            SqlMetaDataPriv col, 
            SqlTceCipherInfoTable? cipherTable, 
            SqlCommandColumnEncryptionSetting columnEncryptionSetting,
            bool isReturnValue) {
            Debug.Assert(isReturnValue == (cipherTable == null), "Ciphertable is not set iff this is a return value");

            // Read the ordinal into cipher table
            ushort index = 0;
            UInt32 userType;

            // For return values there is not cipher table and no ordinal.
            if (cipherTable.HasValue) {
                if (!stateObj.TryReadUInt16(out index)) {
                    return false;
                }

                // validate the index (ordinal passed)
                if (index >= cipherTable.Value.Size) {
                    Bid.Trace("<sc.TdsParser.TryProcessTceCryptoMetadata|TCE> Incorrect ordinal received %d, max tab size: %d\n", index, cipherTable.Value.Size);
                    throw SQL.ParsingErrorValue(ParsingErrorState.TceInvalidOrdinalIntoCipherInfoTable, index);
                }
            }

            // Read the user type
            if (!stateObj.TryReadUInt32(out userType)) {
                return false;
            }

            // Read the base TypeInfo
            col.baseTI = new SqlMetaDataPriv();
            if (!TryProcessTypeInfo(stateObj, col.baseTI, userType)) {
                return false;
            }

            // Read the cipher algorithm Id
            byte cipherAlgorithmId;
            if (!stateObj.TryReadByte(out cipherAlgorithmId)) {
                return false;
            }

            string cipherAlgorithmName = null;
            if (TdsEnums.CustomCipherAlgorithmId == cipherAlgorithmId) {
                // Custom encryption algorithm, read the name
                byte nameSize;
                if (!stateObj.TryReadByte(out nameSize)) {
                    return false;
                }

                if (!stateObj.TryReadString(nameSize, out cipherAlgorithmName)) {
                    return false;
                }
            }

            // Read Encryption Type. 
            byte encryptionType;
            if (!stateObj.TryReadByte(out encryptionType)) {
                return false;
            }

            // Read Normalization Rule Version.
            byte normalizationRuleVersion;
            if (!stateObj.TryReadByte(out normalizationRuleVersion)) {
                return false;
            }

            Debug.Assert(col.cipherMD == null, "col.cipherMD should be null in TryProcessTceCryptoMetadata.");

            // Check if TCE is enable and if it is set the crypto MD for the column.
            // TCE is enabled if the command is set to enabled or to resultset only and this is not a return value
            // or if it is set to use connection setting and the connection has TCE enabled.
            if ((columnEncryptionSetting == SqlCommandColumnEncryptionSetting.Enabled ||
                (columnEncryptionSetting == SqlCommandColumnEncryptionSetting.ResultSetOnly && !isReturnValue)) ||
                (columnEncryptionSetting == SqlCommandColumnEncryptionSetting.UseConnectionSetting &&
                _connHandler != null && _connHandler.ConnectionOptions != null &&
                _connHandler.ConnectionOptions.ColumnEncryptionSetting == SqlConnectionColumnEncryptionSetting.Enabled)) {
                col.cipherMD = new SqlCipherMetadata(cipherTable.HasValue ? (SqlTceCipherInfoEntry?)cipherTable.Value[index] : null,
                                                        index,
                                                        cipherAlgorithmId: cipherAlgorithmId,
                                                        cipherAlgorithmName: cipherAlgorithmName,
                                                        encryptionType: encryptionType,
                                                        normalizationRuleVersion: normalizationRuleVersion);
            }
            else {
                // If TCE is disabled mark the MD as not encrypted.
                col.isEncrypted = false;
            }

            return true;
        }

        internal bool TryProcessCollation(TdsParserStateObject stateObj, out SqlCollation collation) {
            SqlCollation newCollation = new SqlCollation();

            if (!stateObj.TryReadUInt32(out newCollation.info)) {
                collation = null;
                return false;
            }
            if (!stateObj.TryReadByte(out newCollation.sortId)) {
                collation = null;
                return false;
            }

            collation = newCollation;
            return true;
        }

        private void WriteCollation(SqlCollation collation, TdsParserStateObject stateObj) {
            if (collation == null) {
                _physicalStateObj.WriteByte(0);
            }
            else {
                _physicalStateObj.WriteByte(sizeof(UInt32)+sizeof(byte));
                WriteUnsignedInt(collation.info, _physicalStateObj);
                _physicalStateObj.WriteByte(collation.sortId);
            }

        }

        internal int GetCodePage(SqlCollation collation, TdsParserStateObject stateObj) {
            int codePage = 0;

            if (0 != collation.sortId) {
                codePage = TdsEnums.CODE_PAGE_FROM_SORT_ID[collation.sortId];
                Debug.Assert(0 != codePage, "GetCodePage accessed codepage array and produced 0!, sortID =" + ((Byte)(collation.sortId)).ToString((IFormatProvider)null));
            }
            else {
                int cultureId = collation.LCID;
                bool success = false;

                try {
                    codePage = CultureInfo.GetCultureInfo(cultureId).TextInfo.ANSICodePage;

                    // SqlHot 50001398: CodePage can be zero, but we should defer such errors until
                    //  we actually MUST use the code page (i.e. don't error if no ANSI data is sent).
                    success = true;
                }
                catch (ArgumentException e) {
                    ADP.TraceExceptionWithoutRethrow(e);
                }

                // If we failed, it is quite possible this is because certain culture id's
                // were removed in Win2k and beyond, however Sql Server still supports them.
                // There is a workaround for the culture id's listed below, which is to mask
                // off the sort id (the leading 1). If that fails, or we have a culture id
                // other than the special cases below, we throw an error and throw away the
                // rest of the results. For additional info, see MDAC 65963.

                // SqlHot 50001398: Sometimes GetCultureInfo will return CodePage 0 instead of throwing.
                //  treat this as an error also, and switch into the special-case logic.
                if (!success || codePage == 0) {
                    CultureInfo ci = null;
                    switch (cultureId) {
                        case 0x10404: // zh-TW
                        case 0x10804: // zh-CN
                        case 0x10c04: // zh-HK
                        case 0x11004: // zh-SG
                        case 0x11404: // zh-MO
                        case 0x10411: // ja-JP
                        case 0x10412: // ko-KR
                            // If one of the following special cases, mask out sortId and
                            // retry.
                            cultureId = cultureId & 0x03fff;

                            try {
                                ci = new CultureInfo(cultureId);
                                success = true;
                            }
                            catch (ArgumentException e) {
                                ADP.TraceExceptionWithoutRethrow(e);
                            }
                            break;
                        case 0x827:     // Non-supported Lithuanian code page, map it to supported Lithuanian.
                            try {
                                ci = new CultureInfo(0x427);
                                success = true;
                            }
                            catch (ArgumentException e) {
                                ADP.TraceExceptionWithoutRethrow(e);
                            }
                            break;
                        default:
                            break;
                    }

                    // I don't believe we should still be in failure case, but just in case.
                    if (!success) {
                        ThrowUnsupportedCollationEncountered(stateObj);
                    }

                    if (null != ci) {
                        codePage = ci.TextInfo.ANSICodePage;
                    }
                }
            }

            return codePage;
        }


        internal void DrainData(TdsParserStateObject stateObj) {
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
                    try {
                        SqlDataReader.SharedState sharedState = stateObj._readerState;
                        if (sharedState != null && sharedState._dataReady) {
                            var metadata = stateObj._cleanupMetaData;
                            if (stateObj._partialHeaderBytesRead > 0) {
                                if (!stateObj.TryProcessHeader()) {
                                    throw SQL.SynchronousCallMayNotPend();
                                }
                            }
                            if (0 == sharedState._nextColumnHeaderToRead) {
                                // i. user called read but didn't fetch anything
                                if (!stateObj.Parser.TrySkipRow(stateObj._cleanupMetaData, stateObj)) {
                                    throw SQL.SynchronousCallMayNotPend();
                                }
                            }
                            else {
                                // iia.  if we still have bytes left from a partially read column, skip
                                if (sharedState._nextColumnDataToRead < sharedState._nextColumnHeaderToRead) {
                                    if ((sharedState._nextColumnHeaderToRead > 0) && (metadata[sharedState._nextColumnHeaderToRead - 1].metaType.IsPlp)) {
                                        if (stateObj._longlen != 0) {
                                            ulong ignored;
                                            if (!TrySkipPlpValue(UInt64.MaxValue, stateObj, out ignored)) {
                                                throw SQL.SynchronousCallMayNotPend();
                                            }
                                        }
                                    }

                                    else if (0 < sharedState._columnDataBytesRemaining) {
                                        if (!stateObj.TrySkipLongBytes(sharedState._columnDataBytesRemaining)) {
                                            throw SQL.SynchronousCallMayNotPend();
                                        }
                                    }

                                }


                                // iib.
                                // now read the remaining values off the wire for this row
                                if (!stateObj.Parser.TrySkipRow(metadata, sharedState._nextColumnHeaderToRead, stateObj)) {
                                    throw SQL.SynchronousCallMayNotPend();
                                }

                            }
                        }
                        Run(RunBehavior.Clean, null, null, null, stateObj);
                    }
                    catch {
                        _connHandler.DoomThisConnection();
                        throw;
                    }
                }
#if DEBUG
                finally {
                    tdsReliabilitySection.Stop();
                }
#endif //DEBUG
            }
            catch (System.OutOfMemoryException) {
                _connHandler.DoomThisConnection();
                throw;
            }
            catch (System.StackOverflowException) {
                _connHandler.DoomThisConnection();
                throw;
            }
            catch (System.Threading.ThreadAbortException) {
                _connHandler.DoomThisConnection();
                throw;
            }
        }
        

        internal void ThrowUnsupportedCollationEncountered(TdsParserStateObject stateObj) {
            stateObj.AddError(new SqlError(0, 0, TdsEnums.MIN_ERROR_CLASS, _server, SQLMessage.CultureIdError(), "", 0));

            if (null != stateObj) {
                DrainData(stateObj);

                stateObj._pendingData = false;
            }

            ThrowExceptionAndWarning(stateObj);
        }



        internal bool TryProcessAltMetaData(int cColumns, TdsParserStateObject stateObj, out _SqlMetaDataSet metaData) {
            Debug.Assert(cColumns > 0, "should have at least 1 column in altMetaData!");

            metaData = null;
            _SqlMetaDataSet altMetaDataSet = new _SqlMetaDataSet(cColumns, null);
            int[] indexMap = new int[cColumns];

            if (!stateObj.TryReadUInt16(out altMetaDataSet.id)) {
                return false;
            }

            byte byCols;
            if (!stateObj.TryReadByte(out byCols)) {
                return false;
            }

            while (byCols > 0) {
                if (!stateObj.TrySkipBytes(2)) { // ignore ColNum ...
                    return false;
                }
                byCols--;
            }

            // pass 1, read the meta data off the wire
            for (int i = 0; i < cColumns; i++) {
                // internal meta data class
                _SqlMetaData col = altMetaDataSet[i];

                if (!stateObj.TryReadByte(out col.op)) {
                    return false;
                }
                if (!stateObj.TryReadUInt16(out col.operand)) {
                    return false;
                }

                // TCE is not applicable to AltMetadata.
                if (!TryCommonProcessMetaData(stateObj, col, null, fColMD: false, columnEncryptionSetting: SqlCommandColumnEncryptionSetting.Disabled)) {
                    return false;
                }

                if (ADP.IsEmpty(col.column)) {
                    // create column name from op
                    switch (col.op) {
                        case TdsEnums.AOPAVG:
                            col.column = "avg";
                            break;

                        case TdsEnums.AOPCNT:
                            col.column = "cnt";
                            break;

                        case TdsEnums.AOPCNTB:
                            col.column = "cntb";
                            break;

                        case TdsEnums.AOPMAX:
                            col.column = "max";
                            break;

                        case TdsEnums.AOPMIN:
                            col.column = "min";
                            break;

                        case TdsEnums.AOPSUM:
                            col.column = "sum";
                            break;

                        case TdsEnums.AOPANY:
                            col.column = "any";
                            break;

                        case TdsEnums.AOPNOOP:
                            col.column = "noop";
                            break;

                        case TdsEnums.AOPSTDEV:
                            col.column = "stdev";
                            break;

                        case TdsEnums.AOPSTDEVP:
                            col.column = "stdevp";
                            break;

                        case TdsEnums.AOPVAR:
                            col.column = "var";
                            break;

                        case TdsEnums.AOPVARP:
                            col.column = "varp";
                            break;
                    }
                }
                indexMap[i] = i;
            }

            altMetaDataSet.indexMap = indexMap;
            altMetaDataSet.visibleColumns = cColumns;

            metaData = altMetaDataSet;
            return true;
        }

        /// <summary>
        /// <para> Parses the TDS message to read single CIPHER_INFO entry.</para>
        /// </summary>
        internal bool TryReadCipherInfoEntry (TdsParserStateObject stateObj, out SqlTceCipherInfoEntry entry) {
            byte cekValueCount = 0;
            entry = new SqlTceCipherInfoEntry(ordinal: 0);

            // Read the DB ID
            int dbId;
            if (!stateObj.TryReadInt32(out dbId)) {
                return false;
            }

            // Read the keyID
            int keyId;
            if (!stateObj.TryReadInt32(out keyId)) {
                return false;
            }

            // Read the key version
            int keyVersion;
            if (!stateObj.TryReadInt32(out keyVersion)) {
                return false;
            }

            // Read the key MD Version
            byte[] keyMDVersion = new byte[8];
            if (!stateObj.TryReadByteArray(keyMDVersion, 0, 8)) {
                return false;
            }

            // Read the value count
            if (!stateObj.TryReadByte (out cekValueCount)) {
                return false;
            }

            for (int i = 0; i < cekValueCount; i++) {
                // Read individual CEK values
                byte[] encryptedCek;
                string keyPath;
                string keyStoreName;
                byte algorithmLength;
                string algorithmName;
                ushort shortValue;
                byte byteValue;
                int length;

                // Read the length of encrypted CEK 
                if (!stateObj.TryReadUInt16 (out shortValue)) {
                    return false; 
                }

                length = shortValue;
                encryptedCek = new byte[length];

                // Read the actual encrypted CEK
                if (!stateObj.TryReadByteArray (encryptedCek, 0, length)) {
                    return false;
                }

                // Read the length of key store name
                if (!stateObj.TryReadByte (out byteValue)) {
                    return false;
                }

                length = byteValue;

                // And read the key store name now
                if (!stateObj.TryReadString(length, out keyStoreName)) {
                    return false;
                }

                // Read the length of key Path
                if (!stateObj.TryReadUInt16 (out shortValue)) {
                    return false;
                }

                length = shortValue;

                // Read the key path string
                if (!stateObj.TryReadString(length, out keyPath)) {
                    return false;
                }

                // Read the length of the string carrying the encryption algo
                if (!stateObj.TryReadByte(out algorithmLength)) {
                    return false;
                }

                length = (int)algorithmLength;

                // Read the string carrying the encryption algo  (eg. RSA_PKCS_OAEP)
                if (!stateObj.TryReadString(length, out algorithmName)) {
                    return false;
                }

                // Add this encrypted CEK blob to our list of encrypted values for the CEK
                entry.Add(encryptedCek, 
                    databaseId: dbId, 
                    cekId: keyId, 
                    cekVersion: keyVersion, 
                    cekMdVersion: keyMDVersion, 
                    keyPath: keyPath, 
                    keyStoreName: keyStoreName, 
                    algorithmName: algorithmName);
            }

            return true;
        }

        /// <summary>
        /// <para> Parses the TDS message to read a single CIPHER_INFO table.</para>
        /// </summary>
        internal bool TryProcessCipherInfoTable (TdsParserStateObject stateObj, out SqlTceCipherInfoTable? cipherTable) {
            // Read count
            short tableSize = 0;
            cipherTable = null;
            if (!stateObj.TryReadInt16(out tableSize)) {
                return false;
            }

            if (0 != tableSize) {
                SqlTceCipherInfoTable tempTable = new SqlTceCipherInfoTable(tableSize);

                // Read individual entries
                for (int i = 0; i < tableSize; i++) {
                    SqlTceCipherInfoEntry entry;
                    if (!TryReadCipherInfoEntry (stateObj, out entry)) {
                        return false;
                    }

                    tempTable[i] = entry;
                }

                cipherTable = tempTable;
            }

            return true;
        }

        internal bool TryProcessMetaData(int cColumns, TdsParserStateObject stateObj, out _SqlMetaDataSet metaData, SqlCommandColumnEncryptionSetting columnEncryptionSetting) {
            Debug.Assert(cColumns > 0, "should have at least 1 column in metadata!");

            // Read the cipher info table first 
            SqlTceCipherInfoTable? cipherTable = null;
            if (_serverSupportsColumnEncryption) {
                if (!TryProcessCipherInfoTable (stateObj, out cipherTable)) {
                    metaData = null;
                    return false;
                }
            }

            // Read the ColumnData fields
            _SqlMetaDataSet newMetaData = new _SqlMetaDataSet(cColumns, cipherTable);
            for (int i = 0; i < cColumns; i++) {
                if (!TryCommonProcessMetaData(stateObj, newMetaData[i], cipherTable, fColMD: true, columnEncryptionSetting: columnEncryptionSetting)) {
                    metaData = null;
                    return false;
                }
            }

            // DEVNOTE: cipherTable is discarded at this point since its no longer needed.
            metaData = newMetaData;
            return true;
        }

        private bool IsVarTimeTds(byte tdsType) {
            return tdsType == TdsEnums.SQLTIME || tdsType == TdsEnums.SQLDATETIME2 || tdsType == TdsEnums.SQLDATETIMEOFFSET;
        }

        private bool TryProcessTypeInfo (TdsParserStateObject stateObj, SqlMetaDataPriv col, UInt32 userType) {
            byte byteLen;
            byte tdsType;
            if (!stateObj.TryReadByte(out tdsType)) {
                return false;
            }

            if (tdsType == TdsEnums.SQLXMLTYPE)
                col.length = TdsEnums.SQL_USHORTVARMAXLEN;  //Use the same length as other plp datatypes
            else if (IsVarTimeTds(tdsType))
                col.length = 0;  // placeholder until we read the scale, just make sure it's not SQL_USHORTVARMAXLEN
            else if (tdsType == TdsEnums.SQLDATE) {
                col.length = 3;
            }
            else {
                if (!TryGetTokenLength(tdsType, stateObj, out col.length)) {
                    return false;
                }
            }

            col.metaType = MetaType.GetSqlDataType(tdsType, userType, col.length);
            col.type = col.metaType.SqlDbType;

            // If sphinx, do not change to nullable type
            if (_isShiloh)
                col.tdsType = (col.isNullable ? col.metaType.NullableType : col.metaType.TDSType);
            else
                col.tdsType = tdsType;

            if (_isYukon) {
                if (TdsEnums.SQLUDT == tdsType) {
                    if (!TryProcessUDTMetaData((SqlMetaDataPriv) col, stateObj)) {
                        return false;
                    }
                }

                if (col.length == TdsEnums.SQL_USHORTVARMAXLEN) {
                    Debug.Assert(tdsType == TdsEnums.SQLXMLTYPE ||
                                 tdsType == TdsEnums.SQLBIGVARCHAR ||
                                 tdsType == TdsEnums.SQLBIGVARBINARY ||
                                 tdsType == TdsEnums.SQLNVARCHAR ||
                                 tdsType == TdsEnums.SQLUDT,
                                 "Invalid streaming datatype");
                    col.metaType = MetaType.GetMaxMetaTypeFromMetaType(col.metaType);
                    Debug.Assert(col.metaType.IsLong, "Max datatype not IsLong");
                    col.length = Int32.MaxValue;
                    if (tdsType == TdsEnums.SQLXMLTYPE) {
                        byte schemapresent;
                        if (!stateObj.TryReadByte(out schemapresent)) {
                            return false;
                        }

                        if ((schemapresent & 1) != 0) {
                            if (!stateObj.TryReadByte(out byteLen)) {
                                return false;
                            }
                            if (byteLen != 0) {
                                if (!stateObj.TryReadString(byteLen, out col.xmlSchemaCollectionDatabase)) {
                                    return false;
                                }
                            }

                            if (!stateObj.TryReadByte(out byteLen)) {
                                return false;
                            }
                            if (byteLen != 0) {
                                if (!stateObj.TryReadString(byteLen, out col.xmlSchemaCollectionOwningSchema)) {
                                    return false;
                                }
                            }

                            short shortLen;
                            if (!stateObj.TryReadInt16(out shortLen)) {
                                return false;
                            }
                            if (byteLen != 0) {
                                if (!stateObj.TryReadString(shortLen, out col.xmlSchemaCollectionName)) {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            if (col.type == SqlDbType.Decimal) {
                if (!stateObj.TryReadByte(out col.precision)) {
                    return false;
                }
                if (!stateObj.TryReadByte(out col.scale)) {
                    return false;
                }
            }

            if (col.metaType.IsVarTime) {
                if (!stateObj.TryReadByte(out col.scale)) {
                    return false;
                }

                Debug.Assert(0 <= col.scale && col.scale <= 7);

                // calculate actual column length here
                // 
                switch (col.metaType.SqlDbType)
                {
                    case SqlDbType.Time:
                        col.length = MetaType.GetTimeSizeFromScale(col.scale);
                        break;
                    case SqlDbType.DateTime2:
                        // Date in number of days (3 bytes) + time
                        col.length = 3 + MetaType.GetTimeSizeFromScale(col.scale);
                        break;
                    case SqlDbType.DateTimeOffset:
                        // Date in days (3 bytes) + offset in minutes (2 bytes) + time
                        col.length = 5 + MetaType.GetTimeSizeFromScale(col.scale);
                        break;

                    default:
                        Debug.Assert(false, "Unknown VariableTime type!");
                        break;
                }
            }

            // read the collation for 7.x servers
            if (_isShiloh && col.metaType.IsCharType && (tdsType != TdsEnums.SQLXMLTYPE)) {
                if (!TryProcessCollation(stateObj, out col.collation)) {
                    return false;
                }

                int codePage = GetCodePage(col.collation, stateObj);

                if (codePage == _defaultCodePage) {
                    col.codePage = _defaultCodePage;
                    col.encoding = _defaultEncoding;
                }
                else {
                    col.codePage = codePage;
                    col.encoding = System.Text.Encoding.GetEncoding(col.codePage);
                }
            }

            return true;
        }

        private bool TryCommonProcessMetaData(TdsParserStateObject stateObj, _SqlMetaData col, SqlTceCipherInfoTable? cipherTable, bool fColMD, SqlCommandColumnEncryptionSetting columnEncryptionSetting) {
            byte byteLen;
            UInt32 userType;

            // read user type - 4 bytes Yukon, 2 backwards
            if (IsYukonOrNewer) {
                if (!stateObj.TryReadUInt32(out userType)) {
                    return false;
                }
            }
            else {
                ushort userTypeShort;
                if (!stateObj.TryReadUInt16(out userTypeShort)) {
                    return false;
                }
                userType = userTypeShort;
            }

            // read flags and set appropriate flags in structure
            byte flags;
            if (!stateObj.TryReadByte(out flags)) {
                return false;
            }

            col.updatability = (byte)((flags & TdsEnums.Updatability) >> 2);
            col.isNullable = (TdsEnums.Nullable == (flags & TdsEnums.Nullable));
            col.isIdentity = (TdsEnums.Identity == (flags & TdsEnums.Identity));

            // read second byte of column metadata flags
            if (!stateObj.TryReadByte(out flags)) {
                return false;
            }
            
            col.isColumnSet = (TdsEnums.IsColumnSet == (flags & TdsEnums.IsColumnSet));
            if (fColMD && _serverSupportsColumnEncryption) {
                col.isEncrypted = (TdsEnums.IsEncrypted == (flags & TdsEnums.IsEncrypted));
            }

            // Read TypeInfo
            if (!TryProcessTypeInfo (stateObj, col, userType)) {
                return false;
            }

            // Read tablename if present
            if (col.metaType.IsLong && !col.metaType.IsPlp) {
                if (_isYukon) {
                    int  unusedLen = 0xFFFF;      //We ignore this value
                    if (!TryProcessOneTable(stateObj, ref unusedLen, out col.multiPartTableName)) {
                        return false;
                    }
                } else {
                    ushort shortLen;
                    if (!stateObj.TryReadUInt16(out shortLen)) {
                        return false;
                    }
                    string tableName;
                    if (!stateObj.TryReadString(shortLen, out tableName)) {
                        return false;
                    }
                    // with Sql2000 this is returned as an unquoted mix of catalog.owner.table
                    // all of which may contain "." and unable to parse correctly from the string alone
                    // example "select * from pubs..[A.B.C.D.E]" AND only when * will contain a image/text/ntext column
                    // by delay parsing from execute to SqlDataReader.GetSchemaTable to enable more scenarios
                    col.multiPartTableName = new MultiPartTableName(tableName);
                }
            }

            // Read the TCE column cryptoinfo
            if (fColMD && _serverSupportsColumnEncryption && col.isEncrypted) {
                // If the column is encrypted, we should have a valid cipherTable
                if (cipherTable.HasValue && !TryProcessTceCryptoMetadata (stateObj, col, cipherTable.Value, columnEncryptionSetting, isReturnValue: false)) {
                    return false;
                }
            }

            // Read the column name 
            if (!stateObj.TryReadByte(out byteLen)) {
                return false;
            }
            if (!stateObj.TryReadString(byteLen, out col.column)) {
                return false;
            }

            // We get too many DONE COUNTs from the server, causing too meany StatementCompleted event firings.
            // We only need to fire this event when we actually have a meta data stream with 0 or more rows.
            stateObj._receivedColMetaData = true;
            return true;
        }

        private bool TryProcessUDTMetaData(SqlMetaDataPriv metaData, TdsParserStateObject stateObj) {
            ushort shortLength;
            byte byteLength;

            if (!stateObj.TryReadUInt16(out shortLength)) { // max byte size
                return false;
            }
            metaData.length = shortLength;

            // database name
            if (!stateObj.TryReadByte(out byteLength)) {
                return false;
            }
            if (byteLength != 0) {
                if (!stateObj.TryReadString(byteLength, out metaData.udtDatabaseName)) {
                    return false;
                }
            }

            // schema name
            if (!stateObj.TryReadByte(out byteLength)) {
                return false;
            }
            if (byteLength != 0) {
                if (!stateObj.TryReadString(byteLength, out metaData.udtSchemaName)) {
                    return false;
                }
            }

            // type name
            if (!stateObj.TryReadByte(out byteLength)) {
                return false;
            }
            if (byteLength != 0) {
                if (!stateObj.TryReadString(byteLength, out metaData.udtTypeName)) {
                    return false;
                }
            }

            if (!stateObj.TryReadUInt16(out shortLength)) {
                return false;
            }
            if (shortLength != 0) {
                if (!stateObj.TryReadString(shortLength, out metaData.udtAssemblyQualifiedName)) {
                    return false;
                }
            }

            return true;
        }

        private void WriteUDTMetaData(object value, string database, string schema, string type,
                                        TdsParserStateObject stateObj) {
            // database
            if (ADP.IsEmpty(database)) {
                stateObj.WriteByte(0);
            }
            else {
                stateObj.WriteByte((byte)database.Length);
                WriteString(database, stateObj);
            }

            // schema
            if (ADP.IsEmpty(schema)) {
                stateObj.WriteByte(0);
            }
            else {
                stateObj.WriteByte((byte)schema.Length);
                WriteString(schema, stateObj);
            }

            // type
            if (ADP.IsEmpty(type)) {
                stateObj.WriteByte(0);
            }
            else {
                stateObj.WriteByte((byte)type.Length);
                WriteString(type, stateObj);
            }
        }

        internal bool TryProcessTableName(int length, TdsParserStateObject stateObj, out MultiPartTableName[] multiPartTableNames) {
            int tablesAdded = 0;

            MultiPartTableName[] tables = new MultiPartTableName[1];
            MultiPartTableName mpt;
            while (length > 0) {
                // 


                if (!TryProcessOneTable(stateObj, ref length, out mpt)) {
                    multiPartTableNames = null;
                    return false;
                }
                if (tablesAdded == 0) {
                    tables[tablesAdded] = mpt;
                }
                else {
                    MultiPartTableName[] newTables = new MultiPartTableName[tables.Length + 1];
                    Array.Copy(tables, 0, newTables, 0, tables.Length);
                    newTables[tables.Length] = mpt;
                    tables = newTables;
                }

                tablesAdded++;
            }

            multiPartTableNames = tables;
            return true;
        }

        private bool TryProcessOneTable(TdsParserStateObject stateObj, ref int length, out MultiPartTableName multiPartTableName) {
            ushort tableLen;
            MultiPartTableName mpt;
            string value;

            multiPartTableName = default(MultiPartTableName);

            if (_isShilohSP1) {

                mpt = new MultiPartTableName();
                byte nParts;

                // Find out how many parts in the TDS stream
                if (!stateObj.TryReadByte(out nParts)) {
                    return false;
                }
                length--;
                if (nParts == 4) {
                    if (!stateObj.TryReadUInt16(out tableLen)) {
                        return false;
                    }
                    length -= 2;
                    if (!stateObj.TryReadString(tableLen, out value)) {
                        return false;
                    }
                    mpt.ServerName = value;
                    nParts--;
                    length -= (tableLen * 2); // wide bytes
                }
                if (nParts == 3) {
                    if (!stateObj.TryReadUInt16(out tableLen)) {
                        return false;
                    }
                    length -= 2;
                    if (!stateObj.TryReadString(tableLen, out value)) {
                        return false;
                    }
                    mpt.CatalogName = value;
                    length -= (tableLen * 2); // wide bytes
                    nParts--;
                }
                if (nParts == 2) {
                    if (!stateObj.TryReadUInt16(out tableLen)) {
                        return false;
                    }
                    length -= 2;
                    if (!stateObj.TryReadString(tableLen, out value)) {
                        return false;
                    }
                    mpt.SchemaName = value;
                    length -= (tableLen * 2); // wide bytes
                    nParts--;
                }
                if (nParts == 1) {
                    if (!stateObj.TryReadUInt16(out tableLen)) {
                        return false;
                    }
                    length -= 2;
                    if (!stateObj.TryReadString(tableLen, out value)) {
                        return false;
                    }
                    mpt.TableName = value;
                    length -= (tableLen * 2); // wide bytes
                    nParts--;
                }
                Debug.Assert(nParts == 0 , "ProcessTableName:Unidentified parts in the table name token stream!");

            }
            else {
                if (!stateObj.TryReadUInt16(out tableLen)) {
                    return false;
                }
                length -= 2;
                if (!stateObj.TryReadString(tableLen, out value)) {
                    return false;
                }
                string tableName = value;
                length -= (tableLen * 2); // wide bytes
                mpt = new MultiPartTableName(MultipartIdentifier.ParseMultipartIdentifier(tableName, "[\"", "]\"", Res.SQL_TDSParserTableName, false));
            }

            multiPartTableName = mpt;
            return true;
        }

        // augments current metadata with table and key information
        private bool TryProcessColInfo(_SqlMetaDataSet columns, SqlDataReader reader, TdsParserStateObject stateObj, out _SqlMetaDataSet metaData) {
            Debug.Assert(columns != null && columns.Length > 0, "no metadata available!");

            metaData = null;

            for (int i = 0; i < columns.Length; i++) {
                _SqlMetaData col = columns[i];

                byte ignored;
                if (!stateObj.TryReadByte(out ignored)) { // colnum, ignore
                    return false;
                }
                if (!stateObj.TryReadByte(out col.tableNum)) {
                    return false;
                }

                // interpret status
                byte status;
                if (!stateObj.TryReadByte(out status)) {
                    return false;
                }

                col.isDifferentName = (TdsEnums.SQLDifferentName == (status & TdsEnums.SQLDifferentName));
                col.isExpression = (TdsEnums.SQLExpression == (status & TdsEnums.SQLExpression));
                col.isKey = (TdsEnums.SQLKey == (status & TdsEnums.SQLKey));
                col.isHidden = (TdsEnums.SQLHidden == (status & TdsEnums.SQLHidden));

                // read off the base table name if it is different than the select list column name
                if (col.isDifferentName) {
                    byte len;
                    if (!stateObj.TryReadByte(out len)) {
                        return false;
                    }
                    if (!stateObj.TryReadString(len, out col.baseColumn)) {
                        return false;
                    }
                }

                // Fixup column name - only if result of a table - that is if it was not the result of
                // an expression.
                if ((reader.TableNames != null) && (col.tableNum > 0)) {
                    Debug.Assert(reader.TableNames.Length >= col.tableNum, "invalid tableNames array!");
                    col.multiPartTableName = reader.TableNames[col.tableNum - 1];
                }

                // MDAC 60109: expressions are readonly
                if (col.isExpression) {
                    col.updatability = 0;
                }
            }

            // set the metadata so that the stream knows some metadata info has changed
            metaData = columns;
            return true;
        }

        // takes care of any per data header information:
        // for long columns, reads off textptrs, reads length, check nullability
        // for other columns, reads length, checks nullability
        // returns length and nullability
        internal bool TryProcessColumnHeader(SqlMetaDataPriv col, TdsParserStateObject stateObj, int columnOrdinal, out bool isNull, out ulong length) {
            // query NBC row information first
            if (stateObj.IsNullCompressionBitSet(columnOrdinal)) {
                isNull = true;
                // column information is not present in TDS if null compression bit is set, return now
                length = 0;
                return true;
            }

            return TryProcessColumnHeaderNoNBC(col, stateObj, out isNull, out length);
        }

        private bool TryProcessColumnHeaderNoNBC(SqlMetaDataPriv col, TdsParserStateObject stateObj, out bool isNull, out ulong length) {
            if (col.metaType.IsLong && !col.metaType.IsPlp) {
                //
                // we don't care about TextPtrs, simply go after the data after it
                //
                byte textPtrLen;
                if (!stateObj.TryReadByte(out textPtrLen)) {
                    isNull = false;
                    length = 0;
                    return false;
                }

                if (0 != textPtrLen) {
                    // read past text pointer
                    if (!stateObj.TrySkipBytes(textPtrLen)) {
                        isNull = false;
                        length = 0;
                        return false;
                    }

                    // read past timestamp
                    if (!stateObj.TrySkipBytes(TdsEnums.TEXT_TIME_STAMP_LEN)) {
                        isNull = false;
                        length = 0;
                        return false;
                    }

                    isNull = false;
                    return TryGetDataLength(col, stateObj, out length);
                }
                else {
                    isNull = true;
                    length = 0;
                    return true;

                }
            }
            else {
                // non-blob columns
                ulong longlen;
                if (!TryGetDataLength(col, stateObj, out longlen)) {
                    isNull = false;
                    length = 0;
                    return false;
                }
                isNull = IsNull(col.metaType, longlen);
                length = (isNull ? 0 : longlen);
                return true;
            }
        }

        // assumes that the current position is at the start of an altrow!
        internal bool TryGetAltRowId(TdsParserStateObject stateObj, out int id) {
            byte token;
            if (!stateObj.TryReadByte(out token)) { // skip over ALTROW token
                id = 0;
                return false;
            }
            Debug.Assert((token == TdsEnums.SQLALTROW), "");

            // Start a fresh row - disable NBC since Alt Rows are never compressed
            if (!stateObj.TryStartNewRow(isNullCompressed: false)) {
                id = 0;
                return false;
            }

            ushort shortId;
            if (!stateObj.TryReadUInt16(out shortId)) {
                id = 0;
                return false;
            }

            id = shortId;
            return true;
        }

        // Used internally by BulkCopy only
        private bool TryProcessRow(_SqlMetaDataSet columns, object[] buffer, int[] map, TdsParserStateObject stateObj) {
            SqlBuffer data = new SqlBuffer();

            for (int i = 0; i < columns.Length; i++) {
                _SqlMetaData md = columns[i];
                Debug.Assert(md != null, "_SqlMetaData should not be null for column " + i.ToString(CultureInfo.InvariantCulture));

                bool isNull;
                ulong len;
                if (!TryProcessColumnHeader(md, stateObj, i, out isNull, out len)) {
                    return false;
                }

                if (isNull) {
                    GetNullSqlValue(data, md, SqlCommandColumnEncryptionSetting.Disabled /*Column Encryption Disabled for Bulk Copy*/, _connHandler);
                    buffer[map[i]] = data.SqlValue;
                }
                else {
                    // We only read up to 2Gb. Throw if data is larger. Very large data
                    // should be read in chunks in sequential read mode
                    // For Plp columns, we may have gotten only the length of the first chunk
                    if (!TryReadSqlValue(data, md, md.metaType.IsPlp ? (Int32.MaxValue) : (int)len, stateObj,
                                         SqlCommandColumnEncryptionSetting.Disabled /*Column Encryption Disabled for Bulk Copy*/,
                                         md.column)) {
                        return false;
                    }
                    buffer[map[i]] = data.SqlValue;
                    if (stateObj._longlen != 0) {
                        throw new SqlTruncateException(Res.GetString(Res.SqlMisc_TruncationMaxDataMessage));
                    }
                }
                data.Clear();
            }

            return true;
        }

        /// <summary>
        /// Determines if a column value should be transparently decrypted (based on SqlCommand and Connection String settings).
        /// </summary>
        /// <returns>true if the value should be transparently decrypted, false otherwise</returns>
        internal static bool ShouldHonorTceForRead (SqlCommandColumnEncryptionSetting columnEncryptionSetting, 
            SqlInternalConnectionTds connection) {

            // Command leve setting trumps all
            switch (columnEncryptionSetting) {
                case SqlCommandColumnEncryptionSetting.Disabled:
                    return false;
                case SqlCommandColumnEncryptionSetting.Enabled:
                    return true;
                case SqlCommandColumnEncryptionSetting.ResultSetOnly:
                    return true;
                default:
                    // Check connection level setting!
                    Debug.Assert(SqlCommandColumnEncryptionSetting.UseConnectionSetting == columnEncryptionSetting,
                        "Unexpected value for command level override");
                    return (connection != null && connection.ConnectionOptions != null &&
                        connection.ConnectionOptions.ColumnEncryptionSetting == SqlConnectionColumnEncryptionSetting.Enabled);
            }
        }

        internal static object GetNullSqlValue(
                SqlBuffer nullVal, 
                SqlMetaDataPriv md, 
                SqlCommandColumnEncryptionSetting columnEncryptionSetting,
                SqlInternalConnectionTds connection) {
            SqlDbType type = md.type;

            if (type == SqlDbType.VarBinary && // if its a varbinary
                md.isEncrypted &&// and encrypted
                ShouldHonorTceForRead(columnEncryptionSetting, connection)){
                type = md.baseTI.type; // the use the actual (plaintext) type
            }

            switch (type) {
                case SqlDbType.Real:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Single);
                    break;

                case SqlDbType.Float:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Double);
                    break;

                case SqlDbType.Udt:
                case SqlDbType.Binary:
                case SqlDbType.VarBinary:
                case SqlDbType.Image:
                    nullVal.SqlBinary = SqlBinary.Null;
                    break;

                case SqlDbType.UniqueIdentifier:
                    nullVal.SqlGuid = SqlGuid.Null;
                    break;

                case SqlDbType.Bit:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Boolean);
                    break;

                case SqlDbType.TinyInt:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Byte);
                    break;

                case SqlDbType.SmallInt:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Int16);
                    break;

                case SqlDbType.Int:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Int32);
                    break;

                case SqlDbType.BigInt:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Int64);
                    break;

                case SqlDbType.Char:
                case SqlDbType.VarChar:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.NText:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.String);
                    break;

                case SqlDbType.Decimal:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Decimal);
                    break;

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.DateTime);
                    break;

                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Money);
                    break;

                case SqlDbType.Variant:
                    // DBNull.Value will have to work here
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Empty);
                    break;

                case SqlDbType.Xml:
                    nullVal.SqlCachedBuffer = SqlCachedBuffer.Null;
                    break;

                case SqlDbType.Date:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Date);
                    break;

                case SqlDbType.Time:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.Time);
                    break;
                    
                case SqlDbType.DateTime2:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.DateTime2);
                    break;
                    
                case SqlDbType.DateTimeOffset:
                    nullVal.SetToNullOfType(SqlBuffer.StorageType.DateTimeOffset);
                    break;

                case SqlDbType.Timestamp:
                    // Dev10 Bug #479607 - this should have been the same as SqlDbType.Binary, but it's a rejected breaking change
                    // Dev10 Bug #752790 - don't assert when it does happen
                    break;

                default:
                    Debug.Assert(false, "unknown null sqlType!" + md.type.ToString());
                    break;
            }

            return nullVal;
        }

        internal bool TrySkipRow(_SqlMetaDataSet columns, TdsParserStateObject stateObj) {
            return TrySkipRow(columns, 0, stateObj);
        }

        internal bool TrySkipRow(_SqlMetaDataSet columns, int startCol, TdsParserStateObject stateObj) {
            for (int i = startCol; i < columns.Length; i++) {
                _SqlMetaData md = columns[i];

                if (!TrySkipValue(md, i, stateObj)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// This method skips bytes of a single column value from the media. It supports NBCROW and handles all types of values, including PLP and long
        /// </summary>
        internal bool TrySkipValue(SqlMetaDataPriv md, int columnOrdinal, TdsParserStateObject stateObj) {
            if (stateObj.IsNullCompressionBitSet(columnOrdinal)) {
                return true;
            }

            if (md.metaType.IsPlp) {
                ulong ignored;
                if (!TrySkipPlpValue(UInt64.MaxValue, stateObj, out ignored)) {
                    return false;
                }
            }
            else if (md.metaType.IsLong) {
                
                Debug.Assert(!md.metaType.IsPlp, "Plp types must be handled using SkipPlpValue");

                byte textPtrLen;
                if (!stateObj.TryReadByte(out textPtrLen)) {
                    return false;
                }

                if (0 != textPtrLen) {
                    if (!stateObj.TrySkipBytes(textPtrLen + TdsEnums.TEXT_TIME_STAMP_LEN)) {
                        return false;
                    }

                    int length;
                    if (!TryGetTokenLength(md.tdsType, stateObj, out length)) {
                        return false;
                    }
                    if (!stateObj.TrySkipBytes(length)) {
                        return false;
                    }
                }
            }
            else {
                int length;
                if (!TryGetTokenLength(md.tdsType, stateObj, out length)) {
                    return false;
                }

                // if false, no value to skip - it's null
                if (!IsNull(md.metaType, (ulong)length)) {
                    if (!stateObj.TrySkipBytes(length)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsNull(MetaType mt, ulong length) {
            // null bin and char types have a length of -1 to represent null
            if (mt.IsPlp) {
                return (TdsEnums.SQL_PLP_NULL == length);
            }

            // HOTFIX #50000415: for image/text, 0xFFFF is the length, not representing null
            if ((TdsEnums.VARNULL == length) && !mt.IsLong) {
                return true;
            }

            // other types have a length of 0 to represent null
            // long and non-PLP types will always return false because these types are either char or binary
            // this is expected since for long and non-plp types isnull is checked based on textptr field and not the length
            return ((TdsEnums.FIXEDNULL == length) && !mt.IsCharType && !mt.IsBinType);
        }

        private bool TryReadSqlStringValue(SqlBuffer value, byte type, int length, Encoding encoding, bool isPlp, TdsParserStateObject stateObj) {
            switch (type) {
                case TdsEnums.SQLCHAR:
                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLVARCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT:
                    // If bigvarchar(max), we only read the first chunk here,
                    // expecting the caller to read the rest
                    if (encoding == null) {
                        // if hitting 7.0 server, encoding will be null in metadata for columns or return values since
                        // 7.0 has no support for multiple code pages in data - single code page support only
                        encoding = _defaultEncoding;
                    }
                    string stringValue;
                    if (!stateObj.TryReadStringWithEncoding(length, encoding, isPlp, out stringValue)) {
                        return false;
                    }
                    value.SetToString(stringValue);
                    break;

                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                {
                        String s = null;

                        if (isPlp) {
                            char[] cc = null;

                            if (!TryReadPlpUnicodeChars(ref cc, 0, length >> 1, stateObj, out length)) {
                                return false;
                            }
                            if (length > 0) {
                                s = new String(cc, 0, length);
                            }
                            else {
                                s = ADP.StrEmpty;
                            }
                        }
                        else {
                            if (!stateObj.TryReadString(length >> 1, out s)) {
                                return false;
                            }
                        }

                        value.SetToString(s);
                        break;
                    }

                default:
                    Debug.Assert(false, "Unknown tds type for SqlString!" + type.ToString(CultureInfo.InvariantCulture));
                    break;
            }

            return true;
        }

        /// <summary>
        /// Deserializes the unencrypted bytes into a value based on the target type info.
        /// </summary>
        internal bool DeserializeUnencryptedValue (SqlBuffer value, byte[] unencryptedBytes, SqlMetaDataPriv md, TdsParserStateObject stateObj, byte normalizationVersion) {
            if (normalizationVersion != 0x01) {
                throw SQL.UnsupportedNormalizationVersion(normalizationVersion);
            }

            byte tdsType = md.baseTI.tdsType;
            int length = unencryptedBytes.Length;

            // For normalized types, the length and scale of the actual type might be different than the value's.
            int denormalizedLength = md.baseTI.length;
            byte denormalizedScale = md.baseTI.scale;

            Debug.Assert (false == md.baseTI.isEncrypted, "Double encryption detected");
            switch (tdsType) {
                // We normalize to allow conversion across data types. All data types below are serialized into a BIGINT.
                case TdsEnums.SQLBIT:
                case TdsEnums.SQLBITN:
                case TdsEnums.SQLINTN:
                case TdsEnums.SQLINT1:
                case TdsEnums.SQLINT2:
                case TdsEnums.SQLINT4:
                case TdsEnums.SQLINT8:
                    Debug.Assert(length == 8, "invalid length for SqlInt64 type!");
                    byte byteValue;
                    long longValue;

                    if (unencryptedBytes.Length != 8) {
                        return false;
                    }

                    longValue = BitConverter.ToInt64(unencryptedBytes, 0);

                    if (tdsType == TdsEnums.SQLBIT ||
                        tdsType == TdsEnums.SQLBITN) {
                        value.Boolean = (longValue != 0);
                        break;
                    }

                    if (tdsType == TdsEnums.SQLINT1 || denormalizedLength == 1)
                        value.Byte = (byte)longValue;
                    else if (tdsType == TdsEnums.SQLINT2 || denormalizedLength == 2)
                        value.Int16 = (Int16)longValue;
                    else if (tdsType == TdsEnums.SQLINT4 || denormalizedLength == 4)
                        value.Int32 = (Int32)longValue;
                    else
                        value.Int64 = longValue;

                    break;

                case TdsEnums.SQLFLTN:
                    if (length == 4) {
                        goto case TdsEnums.SQLFLT4;
                    }
                    else {
                        goto case TdsEnums.SQLFLT8;
                    }

                case TdsEnums.SQLFLT4:
                    Debug.Assert(length == 4, "invalid length for SqlSingle type!");
                    float singleValue;
                    if (unencryptedBytes.Length != 4) {
                        return false;
                    }

                    singleValue = BitConverter.ToSingle(unencryptedBytes, 0);
                    value.Single = singleValue;
                    break;

                case TdsEnums.SQLFLT8:
                    double doubleValue;
                    if (unencryptedBytes.Length != 8) {
                        return false;
                    }

                    doubleValue = BitConverter.ToDouble(unencryptedBytes, 0);
                    value.Double = doubleValue;
                    break;

                // We normalize to allow conversion across data types. SMALLMONEY is serialized into a MONEY.
                case TdsEnums.SQLMONEYN:
                case TdsEnums.SQLMONEY4:
                case TdsEnums.SQLMONEY:
                {
                    int mid;
                    uint lo;

                    if (unencryptedBytes.Length != 8) {
                        return false;
                    }

                    mid = BitConverter.ToInt32(unencryptedBytes, 0);
                    lo = BitConverter.ToUInt32(unencryptedBytes, 4);

                    long l = (((long)mid) << 0x20) + ((long)lo);
                    value.SetToMoney(l);
                    break;
                }

                case TdsEnums.SQLDATETIMN:
                    if (length == 4) {
                        goto case TdsEnums.SQLDATETIM4;
                    }
                    else {
                        goto case TdsEnums.SQLDATETIME;
                    }

                case TdsEnums.SQLDATETIM4:
                    ushort daypartShort, timepartShort;
                    if (unencryptedBytes.Length != 4) {
                        return false;
                    }

                    daypartShort = (UInt16)((unencryptedBytes[1] << 8) + unencryptedBytes[0]);
                    timepartShort = (UInt16)((unencryptedBytes[3] << 8) + unencryptedBytes[2]);
                    value.SetToDateTime(daypartShort, timepartShort * SqlDateTime.SQLTicksPerMinute);
                    break;

                case TdsEnums.SQLDATETIME:
                    int daypart;
                    uint timepart;
                    if (unencryptedBytes.Length != 8) {
                        return false;
                    }

                    daypart = BitConverter.ToInt32(unencryptedBytes, 0);
                    timepart = BitConverter.ToUInt32(unencryptedBytes, 4);
                    value.SetToDateTime(daypart, (int)timepart);
                    break;

                case TdsEnums.SQLUNIQUEID:
                {
                    Debug.Assert(length == 16, "invalid length for SqlGuid type!");
                    value.SqlGuid = new SqlGuid(unencryptedBytes, true);   // doesn't copy the byte array
                    break;
                }

                case TdsEnums.SQLBINARY:
                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLVARBINARY:
                case TdsEnums.SQLIMAGE:
                {
                    // Note: Better not come here with plp data!!
                    Debug.Assert(length <= TdsEnums.MAXSIZE, "Plp data decryption attempted");

                    // If this is a fixed length type, pad with zeros to get to the fixed length size.
                    if (tdsType == TdsEnums.SQLBINARY || tdsType == TdsEnums.SQLBIGBINARY) {
                        byte[] bytes = new byte[md.baseTI.length];
                        Buffer.BlockCopy(unencryptedBytes, 0, bytes, 0, unencryptedBytes.Length);
                        unencryptedBytes = bytes;
                    }

                    value.SqlBinary = new SqlBinary(unencryptedBytes, true);   // doesn't copy the byte array
                    break;
                }

                case TdsEnums.SQLDECIMALN:
                case TdsEnums.SQLNUMERICN:
                    // Check the sign from the first byte.
                    int index = 0;
                    byteValue = unencryptedBytes[index++];
                    bool fPositive = (1 == byteValue);

                    // Now read the 4 next integers which contain the actual value.
                    length = checked((int)length-1);
                    int[] bits = new int[4];
                    int decLength = length>>2;
                    for (int i = 0; i < decLength; i++) {
                        // up to 16 bytes of data following the sign byte
                        bits[i] = BitConverter.ToInt32(unencryptedBytes, index);
                        index += 4;
                    }
                    value.SetToDecimal (md.baseTI.precision, md.baseTI.scale, fPositive, bits);
                    break;

                case TdsEnums.SQLCHAR:
                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLVARCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT:
                    {
                        System.Text.Encoding encoding = md.baseTI.encoding;

                        if (null == encoding) {
                            encoding = _defaultEncoding;
                        }

                        if (null == encoding) {
                            ThrowUnsupportedCollationEncountered(stateObj);
                        }

                        string strValue = encoding.GetString(unencryptedBytes, 0, length);

                        // If this is a fixed length type, pad with spaces to get to the fixed length size.
                        if (tdsType == TdsEnums.SQLCHAR || tdsType == TdsEnums.SQLBIGCHAR) {
                            strValue = strValue.PadRight(md.baseTI.length);
                        }

                        value.SetToString(strValue);
                        break;
                    }

                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                    {
                        string strValue = System.Text.Encoding.Unicode.GetString(unencryptedBytes, 0, length);

                        // If this is a fixed length type, pad with spaces to get to the fixed length size.
                        if (tdsType == TdsEnums.SQLNCHAR) {
                            strValue = strValue.PadRight(md.baseTI.length / ADP.CharSize);
                        }

                        value.SetToString(strValue);
                        break;
                    }

                case TdsEnums.SQLDATE:
                    Debug.Assert(length == 3, "invalid length for date type!");
                    value.SetToDate(unencryptedBytes);
                    break;

                case TdsEnums.SQLTIME:
                    // We normalize to maximum precision to allow conversion across different precisions.
                    Debug.Assert(length == 5, "invalid length for time type!");
                    value.SetToTime(unencryptedBytes, length, TdsEnums.MAX_TIME_SCALE, denormalizedScale);
                    break;

                case TdsEnums.SQLDATETIME2:
                    // We normalize to maximum precision to allow conversion across different precisions.
                    Debug.Assert(length == 8, "invalid length for datetime2 type!");
                    value.SetToDateTime2(unencryptedBytes, length, TdsEnums.MAX_TIME_SCALE, denormalizedScale);
                    break;

                case TdsEnums.SQLDATETIMEOFFSET:
                    // We normalize to maximum precision to allow conversion across different precisions.
                    Debug.Assert(length == 10, "invalid length for datetimeoffset type!");
                    value.SetToDateTimeOffset(unencryptedBytes, length, TdsEnums.MAX_TIME_SCALE, denormalizedScale);
                    break;

                default:
                    MetaType metaType = md.baseTI.metaType;

                    // If we don't have a metatype already, construct one to get the proper type name.
                    if (metaType == null) {
                        metaType = MetaType.GetSqlDataType(tdsType, userType:0, length:length);
                    }

                    throw SQL.UnsupportedDatatypeEncryption(metaType.TypeName);
            }

            return true;
        }

        internal bool TryReadSqlValue(SqlBuffer value,
                                      SqlMetaDataPriv md,
                                      int length,
                                      TdsParserStateObject stateObj,
                                      SqlCommandColumnEncryptionSetting columnEncryptionOverride, 
                                      string columnName) {
            bool isPlp = md.metaType.IsPlp;
            byte tdsType = md.tdsType;

            Debug.Assert(isPlp || !IsNull(md.metaType, (ulong)length), "null value should not get here!");
            if (isPlp) {
                // We must read the column value completely, no matter what length is passed in
                length = Int32.MaxValue;
            }

            //DEVNOTE: When modifying the following routines (for deserialization) please pay attention to 
            // deserialization code in DecryptWithKey () method and modify it accordingly.
            switch (tdsType) {
                case TdsEnums.SQLDECIMALN:
                case TdsEnums.SQLNUMERICN:
                    if (!TryReadSqlDecimal(value, length, md.precision, md.scale, stateObj)) {
                        return false;
                    }
                    break;

                case TdsEnums.SQLUDT:
                case TdsEnums.SQLBINARY:
                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLVARBINARY:
                case TdsEnums.SQLIMAGE:
                    byte[] b = null;

                    // If varbinary(max), we only read the first chunk here, expecting the caller to read the rest
                    if (isPlp) {
                        // If we are given -1 for length, then we read the entire value,
                        // otherwise only the requested amount, usually first chunk.
                        int ignored;
                        if (!stateObj.TryReadPlpBytes(ref b, 0, length, out ignored)) {
                            return false;
                        }
                    }
                    else {
                        //Debug.Assert(length > 0 && length < (long)(Int32.MaxValue), "Bad length for column");
                        b = new byte[length];
                        if (!stateObj.TryReadByteArray(b, 0, length)) {
                            return false;
                        }
                    }

                    if (md.isEncrypted 
                        && ((columnEncryptionOverride == SqlCommandColumnEncryptionSetting.Enabled
                            || columnEncryptionOverride == SqlCommandColumnEncryptionSetting.ResultSetOnly)
                            || (columnEncryptionOverride == SqlCommandColumnEncryptionSetting.UseConnectionSetting
                                && _connHandler != null && _connHandler.ConnectionOptions != null
                                && _connHandler.ConnectionOptions.ColumnEncryptionSetting == SqlConnectionColumnEncryptionSetting.Enabled))) {
                        try {
                            // CipherInfo is present, decrypt and read
                            byte[] unencryptedBytes = SqlSecurityUtility.DecryptWithKey(b, md.cipherMD, _connHandler.ConnectionOptions.DataSource);

                            if (unencryptedBytes != null) {
                                DeserializeUnencryptedValue(value, unencryptedBytes, md, stateObj, md.NormalizationRuleVersion);
                            }
                        }
                        catch (Exception e) {
                            throw SQL.ColumnDecryptionFailed(columnName, null, e);
                        }
                    }
                    else {
                        value.SqlBinary = new SqlBinary(b, true);   // doesn't copy the byte array
                    }
                    break;

                case TdsEnums.SQLCHAR:
                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLVARCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT:
                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                    if (!TryReadSqlStringValue(value, tdsType, length, md.encoding, isPlp, stateObj)) {
                        return false;
                    }
                    break;

                case TdsEnums.SQLXMLTYPE:
                    // We store SqlCachedBuffer here, so that we can return either SqlBinary, SqlString or SqlXmlReader.
                    SqlCachedBuffer sqlBuf;
                    if (!SqlCachedBuffer.TryCreate(md, this, stateObj, out sqlBuf)) {
                        return false;
                    }

                    value.SqlCachedBuffer = sqlBuf;
                    break;

                case TdsEnums.SQLDATE:
                case TdsEnums.SQLTIME: 
                case TdsEnums.SQLDATETIME2:
                case TdsEnums.SQLDATETIMEOFFSET:
                    if (!TryReadSqlDateTime(value, tdsType, length, md.scale, stateObj)) {
                        return false;
                    }
                    break;
                    
                default:
                    Debug.Assert(!isPlp, "ReadSqlValue calling ReadSqlValueInternal with plp data");
                    if (!TryReadSqlValueInternal(value, tdsType, length, stateObj)) {
                        return false;
                    }
                    break;
            }

            Debug.Assert((stateObj._longlen == 0) && (stateObj._longlenleft == 0), "ReadSqlValue did not read plp field completely, longlen =" + stateObj._longlen.ToString((IFormatProvider)null) + ",longlenleft=" + stateObj._longlenleft.ToString((IFormatProvider)null));
            return true;
        }

        private bool TryReadSqlDateTime(SqlBuffer value, byte tdsType, int length, byte scale, TdsParserStateObject stateObj) {
            byte[] datetimeBuffer = new byte[length];

            if (!stateObj.TryReadByteArray(datetimeBuffer, 0, length)) {
                return false;
            }

            switch (tdsType) {
                case TdsEnums.SQLDATE:
                    Debug.Assert(length == 3, "invalid length for date type!");
                    value.SetToDate(datetimeBuffer);
                    break;

                case TdsEnums.SQLTIME:
                    Debug.Assert(3 <= length && length <= 5, "invalid length for time type!");
                    value.SetToTime(datetimeBuffer, length, scale, scale);
                    break;

                case TdsEnums.SQLDATETIME2:
                    Debug.Assert(6 <= length && length <= 8, "invalid length for datetime2 type!");
                    value.SetToDateTime2(datetimeBuffer, length, scale, scale);
                    break;

                case TdsEnums.SQLDATETIMEOFFSET:
                    Debug.Assert(8 <= length && length <= 10, "invalid length for datetimeoffset type!");
                    value.SetToDateTimeOffset(datetimeBuffer, length, scale, scale);
                    break;

                default:
                    Debug.Assert(false, "ReadSqlDateTime is called with the wrong tdsType");
                    break;
            }

            return true;
        }

        internal bool TryReadSqlValueInternal(SqlBuffer value, byte tdsType, int length, TdsParserStateObject stateObj) {
            switch (tdsType) {
                case TdsEnums.SQLBIT:
                case TdsEnums.SQLBITN:
                    Debug.Assert(length == 1, "invalid length for SqlBoolean type!");
                    byte byteValue;
                    if (!stateObj.TryReadByte(out byteValue)) {
                        return false;
                    }
                    value.Boolean = (byteValue != 0);
                    break;

                case TdsEnums.SQLINTN:
                    if (length == 1) {
                        goto case TdsEnums.SQLINT1;
                    }
                    else if (length == 2) {
                        goto case TdsEnums.SQLINT2;
                    }
                    else if (length == 4) {
                        goto case TdsEnums.SQLINT4;
                    }
                    else {
                        goto case TdsEnums.SQLINT8;
                    }

                case TdsEnums.SQLINT1:
                    Debug.Assert(length == 1, "invalid length for SqlByte type!");
                    if (!stateObj.TryReadByte(out byteValue)) {
                        return false;
                    }
                    value.Byte = byteValue;
                    break;

                case TdsEnums.SQLINT2:
                    Debug.Assert(length == 2, "invalid length for SqlInt16 type!");
                    short shortValue;
                    if (!stateObj.TryReadInt16(out shortValue)) {
                        return false;
                    }
                    value.Int16 = shortValue;
                    break;

                case TdsEnums.SQLINT4:
                    Debug.Assert(length == 4, "invalid length for SqlInt32 type!");
                    int intValue;
                    if (!stateObj.TryReadInt32(out intValue)) {
                        return false;
                    }
                    value.Int32 = intValue;
                    break;

                case TdsEnums.SQLINT8:
                    Debug.Assert(length == 8, "invalid length for SqlInt64 type!");
                    long longValue;
                    if (!stateObj.TryReadInt64(out longValue)) {
                        return false;
                    }
                    value.Int64 = longValue;
                    break;

                case TdsEnums.SQLFLTN:
                    if (length == 4) {
                        goto case TdsEnums.SQLFLT4;
                    }
                    else {
                        goto case TdsEnums.SQLFLT8;
                    }

                case TdsEnums.SQLFLT4:
                    Debug.Assert(length == 4, "invalid length for SqlSingle type!");
                    float singleValue;
                    if (!stateObj.TryReadSingle(out singleValue)) {
                        return false;
                    }
                    value.Single = singleValue;
                    break;

                case TdsEnums.SQLFLT8:
                    Debug.Assert(length == 8, "invalid length for SqlDouble type!");
                    double doubleValue;
                    if (!stateObj.TryReadDouble(out doubleValue)) {
                        return false;
                    }
                    value.Double = doubleValue;
                    break;

                case TdsEnums.SQLMONEYN:
                    if (length == 4) {
                        goto case TdsEnums.SQLMONEY4;
                    }
                    else {
                        goto case TdsEnums.SQLMONEY;
                    }

                case TdsEnums.SQLMONEY:
                {
                    int mid;
                    uint lo;

                    if (!stateObj.TryReadInt32(out mid)) {
                        return false;
                    }
                    if (!stateObj.TryReadUInt32(out lo)) {
                        return false;
                    }

                    long l = (((long)mid) << 0x20) + ((long)lo);

                    value.SetToMoney(l);
                    break;
                }

                case TdsEnums.SQLMONEY4:
                    if (!stateObj.TryReadInt32(out intValue)) {
                        return false;
                    }
                    value.SetToMoney(intValue);
                    break;

                case TdsEnums.SQLDATETIMN:
                    if (length == 4) {
                        goto case TdsEnums.SQLDATETIM4;
                    }
                    else {
                        goto case TdsEnums.SQLDATETIME;
                    }

                case TdsEnums.SQLDATETIM4:
                    ushort daypartShort, timepartShort;
                    if (!stateObj.TryReadUInt16(out daypartShort)) {
                        return false;
                    }
                    if (!stateObj.TryReadUInt16(out timepartShort)) {
                        return false;
                    }
                    value.SetToDateTime(daypartShort, timepartShort * SqlDateTime.SQLTicksPerMinute);
                    break;

                case TdsEnums.SQLDATETIME:
                    int daypart;
                    uint timepart;
                    if (!stateObj.TryReadInt32(out daypart)) {
                        return false;
                    }
                    if (!stateObj.TryReadUInt32(out timepart)) {
                        return false;
                    }
                    value.SetToDateTime(daypart, (int)timepart);
                    break;

                case TdsEnums.SQLUNIQUEID:
                {
                    Debug.Assert(length == 16, "invalid length for SqlGuid type!");

                    byte[] b = new byte[length];

                    if (!stateObj.TryReadByteArray(b, 0, length)) {
                        return false;
                    }
                    value.SqlGuid = new SqlGuid(b, true);   // doesn't copy the byte array
                    break;
                }

                case TdsEnums.SQLBINARY:
                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLVARBINARY:
                case TdsEnums.SQLIMAGE:
                {
                    // Note: Better not come here with plp data!!
                    Debug.Assert(length <= TdsEnums.MAXSIZE);
                    byte[] b = new byte[length];
                    if (!stateObj.TryReadByteArray(b, 0, length)) {
                        return false;
                    }
                    value.SqlBinary = new SqlBinary(b, true);   // doesn't copy the byte array

                    break;
                }

                case TdsEnums.SQLVARIANT:
                    if (!TryReadSqlVariant(value, length, stateObj)) {
                        return false;
                    }
                    break;

                default:
                    Debug.Assert(false, "Unknown SqlType!" + tdsType.ToString(CultureInfo.InvariantCulture));
                    break;
            } // switch

            return true;
        }

        //
        // Read in a SQLVariant
        //
        // SQLVariant looks like:
        // struct
        // {
        //      BYTE TypeTag
        //      BYTE cbPropBytes
        //      BYTE[] Properties
        //      BYTE[] DataVal
        // }
        internal bool TryReadSqlVariant(SqlBuffer value, int lenTotal, TdsParserStateObject stateObj) {
            Debug.Assert(_isShiloh == true, "Shouldn't be dealing with sql_variaint in pre-SQL2000 server!");
            // get the SQLVariant type
            byte type;
            if (!stateObj.TryReadByte(out type)) {
                return false;
            }
            ushort lenMax = 0; // maximum lenData of value inside variant

            // read cbPropBytes
            byte cbPropsActual;
            if (!stateObj.TryReadByte(out cbPropsActual)) {
                return false;
            }
            MetaType mt = MetaType.GetSqlDataType(type, 0 /*no user datatype*/, 0 /* no lenData, non-nullable type */);
            byte cbPropsExpected = mt.PropBytes;

            int lenConsumed = TdsEnums.SQLVARIANT_SIZE + cbPropsActual; // type, count of propBytes, and actual propBytes
            int lenData = lenTotal - lenConsumed; // length of actual data

            // read known properties and skip unknown properties
            Debug.Assert(cbPropsActual >= cbPropsExpected, "cbPropsActual is less that cbPropsExpected!");

            //
            // now read the value
            //
            switch (type) {
                case TdsEnums.SQLBIT:
                case TdsEnums.SQLINT1:
                case TdsEnums.SQLINT2:
                case TdsEnums.SQLINT4:
                case TdsEnums.SQLINT8:
                case TdsEnums.SQLFLT4:
                case TdsEnums.SQLFLT8:
                case TdsEnums.SQLMONEY:
                case TdsEnums.SQLMONEY4:
                case TdsEnums.SQLDATETIME:
                case TdsEnums.SQLDATETIM4:
                case TdsEnums.SQLUNIQUEID:
                    if (!TryReadSqlValueInternal(value, type, lenData, stateObj)) {
                        return false;
                    }
                    break;

                case TdsEnums.SQLDECIMALN:
                case TdsEnums.SQLNUMERICN:
                {
                    Debug.Assert(cbPropsExpected == 2, "SqlVariant: invalid PropBytes for decimal/numeric type!");

                    byte precision;
                    if (!stateObj.TryReadByte(out precision)) {
                        return false;
                    }
                    byte scale;
                    if (!stateObj.TryReadByte(out scale)) {
                        return false;
                    }

                    // skip over unknown properties
                    if (cbPropsActual > cbPropsExpected) {
                        if (!stateObj.TrySkipBytes(cbPropsActual - cbPropsExpected)) {
                            return false;
                        }
                    }

                    if (!TryReadSqlDecimal(value, TdsEnums.MAX_NUMERIC_LEN, precision, scale, stateObj)) {
                        return false;
                    }
                    break;
                }

                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                    //Debug.Assert(TdsEnums.VARNULL == lenData, "SqlVariant: data length for Binary indicates null?");
                    Debug.Assert(cbPropsExpected == 2, "SqlVariant: invalid PropBytes for binary type!");

                    if (!stateObj.TryReadUInt16(out lenMax)) {
                        return false;
                    }
                    Debug.Assert(lenMax != TdsEnums.SQL_USHORTVARMAXLEN, "bigvarbinary(max) in a sqlvariant");

                    // skip over unknown properties
                    if (cbPropsActual > cbPropsExpected) {
                        if (!stateObj.TrySkipBytes(cbPropsActual - cbPropsExpected)) {
                            return false;
                        }
                    }

                    goto case TdsEnums.SQLBIT;

                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                {
                    Debug.Assert(cbPropsExpected == 7, "SqlVariant: invalid PropBytes for character type!");

                    SqlCollation collation;
                    if (!TryProcessCollation(stateObj, out collation)) {
                        return false;
                    }

                    if (!stateObj.TryReadUInt16(out lenMax)) {
                        return false;
                    }
                    Debug.Assert(lenMax != TdsEnums.SQL_USHORTVARMAXLEN, "bigvarchar(max) or nvarchar(max) in a sqlvariant");

                    // skip over unknown properties
                    if (cbPropsActual > cbPropsExpected) {
                        if (!stateObj.TrySkipBytes(cbPropsActual - cbPropsExpected)) {
                            return false;
                        }
                    }

                    Encoding encoding = Encoding.GetEncoding(GetCodePage(collation, stateObj));
                    if (!TryReadSqlStringValue(value, type, lenData, encoding, false, stateObj)) {
                        return false;
                    }
                    break;
                }
                case TdsEnums.SQLDATE:
                    if (!TryReadSqlDateTime(value, type, lenData, 0, stateObj)) {
                        return false;
                    }
                    break;

                case TdsEnums.SQLTIME:
                case TdsEnums.SQLDATETIME2:
                case TdsEnums.SQLDATETIMEOFFSET:
                {
                    Debug.Assert(cbPropsExpected == 1, "SqlVariant: invalid PropBytes for time/datetime2/datetimeoffset type!");

                    byte scale;
                    if (!stateObj.TryReadByte(out scale)) {
                        return false;
                    }

                    // skip over unknown properties
                    if (cbPropsActual > cbPropsExpected) {
                        if (!stateObj.TrySkipBytes(cbPropsActual - cbPropsExpected)) {
                            return false;
                        }
                    }

                    if (!TryReadSqlDateTime(value, type, lenData, scale, stateObj)) {
                        return false;
                    }
                    break;
                }

                default:
                    Debug.Assert(false, "Unknown tds type in SqlVariant!" + type.ToString(CultureInfo.InvariantCulture));
                    break;
            } // switch

            return true;
        }

        //
        // Translates a com+ object -> SqlVariant
        // when the type is ambiguous, we always convert to the bigger type
        // note that we also write out the maxlen and actuallen members (4 bytes each)
        // in addition to the SQLVariant structure
        //
        internal Task WriteSqlVariantValue(object value, int length, int offset, TdsParserStateObject stateObj,  bool canAccumulate = true) {
            Debug.Assert(_isShiloh == true, "Shouldn't be dealing with sql_variant in pre-SQL2000 server!");

            // handle null values
            if (ADP.IsNull(value)) {
                WriteInt(TdsEnums.FIXEDNULL, stateObj); //maxlen
                WriteInt(TdsEnums.FIXEDNULL, stateObj); //actuallen
                return null;
            }

            MetaType mt = MetaType.GetMetaTypeFromValue(value);

            // Special case data type correction for SqlMoney inside a SqlVariant.
            if ((TdsEnums.SQLNUMERICN == mt.TDSType) && (8 == length)) {
                // The caller will coerce all SqlTypes to native CLR types, which means SqlMoney will 
                // coerce to decimal/SQLNUMERICN (via SqlMoney.Value call).  In the case where the original 
                // value was SqlMoney the caller will also pass in the metadata length for the SqlMoney type 
                // which is 8 bytes.  To honor the intent of the caller here we coerce this special case 
                // input back to SqlMoney from decimal/SQLNUMERICN.
                mt = MetaType.GetMetaTypeFromValue(new SqlMoney((decimal)value));
            }

            if (mt.IsAnsiType) {
                length = GetEncodingCharLength((string)value, length, 0, _defaultEncoding);
            }

            // max and actual len are equal to
            // SQLVARIANTSIZE {type (1 byte) + cbPropBytes (1 byte)} + cbPropBytes + length (actual length of data in bytes)
            WriteInt(TdsEnums.SQLVARIANT_SIZE + mt.PropBytes + length, stateObj); // maxLen
            WriteInt(TdsEnums.SQLVARIANT_SIZE + mt.PropBytes + length, stateObj); // actualLen

            // write the SQLVariant header (type and cbPropBytes)
            stateObj.WriteByte(mt.TDSType);
            stateObj.WriteByte(mt.PropBytes);

            // now write the actual PropBytes and data
            switch (mt.TDSType) {
                case TdsEnums.SQLFLT4:
                    WriteFloat((Single)value, stateObj);
                    break;

                case TdsEnums.SQLFLT8:
                    WriteDouble((Double)value, stateObj);
                    break;

                case TdsEnums.SQLINT8:
                    WriteLong((Int64)value, stateObj);
                    break;

                case TdsEnums.SQLINT4:
                    WriteInt((Int32)value, stateObj);
                    break;

                case TdsEnums.SQLINT2:
                    WriteShort((Int16)value, stateObj);
                    break;

                case TdsEnums.SQLINT1:
                    stateObj.WriteByte((byte)value);
                    break;

                case TdsEnums.SQLBIT:
                    if ((bool)value == true)
                        stateObj.WriteByte(1);
                    else
                        stateObj.WriteByte(0);

                    break;

                case TdsEnums.SQLBIGVARBINARY:
                    {
                        byte[] b = (byte[])value;

                        WriteShort(length, stateObj); // propbytes: varlen
                        return stateObj.WriteByteArray(b, length, offset, canAccumulate);
                    }

                case TdsEnums.SQLBIGVARCHAR:
                    {
                        string s = (string)value;

                        WriteUnsignedInt(_defaultCollation.info, stateObj); // propbytes: collation.Info
                        stateObj.WriteByte(_defaultCollation.sortId); // propbytes: collation.SortId
                        WriteShort(length, stateObj); // propbyte: varlen
                        return WriteEncodingChar(s, _defaultEncoding, stateObj,  canAccumulate);
                    }

                case TdsEnums.SQLUNIQUEID: 
                    {
                        System.Guid guid = (System.Guid)value;
                        byte[] b = guid.ToByteArray();

                        Debug.Assert((length == b.Length) && (length == 16), "Invalid length for guid type in com+ object");
                        stateObj.WriteByteArray(b, length, 0);
                        break;
                    }

                case TdsEnums.SQLNVARCHAR:
                    {
                        string s = (string)value;

                        WriteUnsignedInt(_defaultCollation.info, stateObj); // propbytes: collation.Info
                        stateObj.WriteByte(_defaultCollation.sortId); // propbytes: collation.SortId
                        WriteShort(length, stateObj); // propbyte: varlen

                        // string takes cchar, not cbyte so convert
                        length >>= 1;
                        return WriteString(s, length, offset, stateObj,  canAccumulate);
                    }

                case TdsEnums.SQLDATETIME: 
                    {
                        TdsDateTime dt = MetaType.FromDateTime((DateTime)value, 8);

                        WriteInt(dt.days, stateObj);
                        WriteInt(dt.time, stateObj);
                        break;
                    }

                case TdsEnums.SQLMONEY: 
                    {
                        WriteCurrency((Decimal)value, 8, stateObj);
                        break;
                    }

                case TdsEnums.SQLNUMERICN: 
                    {
                        stateObj.WriteByte(mt.Precision); //propbytes: precision
                        stateObj.WriteByte((byte)((Decimal.GetBits((Decimal)value)[3] & 0x00ff0000) >> 0x10)); // propbytes: scale
                        WriteDecimal((Decimal)value, stateObj);
                        break;
                    }

                case TdsEnums.SQLTIME:
                    stateObj.WriteByte(mt.Scale); //propbytes: scale
                    WriteTime((TimeSpan)value, mt.Scale, length, stateObj);
                    break;

                case TdsEnums.SQLDATETIMEOFFSET:
                    stateObj.WriteByte(mt.Scale); //propbytes: scale
                    WriteDateTimeOffset((DateTimeOffset)value, mt.Scale, length, stateObj);
                    break;

                default:
                    Debug.Assert(false, "unknown tds type for sqlvariant!");
                    break;
            } // switch
            // return point for accumulated writes, note: non-accumulated writes returned from their case statements
            return null;
        }

// todo: since we now know the difference between SqlWriteVariantValue and SqlWriteRowDataVariant we should consider
// combining these tow methods.

        //
        // Translates a com+ object -> SqlVariant
        // when the type is ambiguous, we always convert to the bigger type
        // note that we also write out the maxlen and actuallen members (4 bytes each)
        // in addition to the SQLVariant structure
        //
        // Devnote: DataRows are preceeded by Metadata. The Metadata includes the MaxLen value.
        // Therefore the sql_variant value must not include the MaxLength. This is the major difference
        // between this method and WriteSqlVariantValue above.
        //
        internal Task WriteSqlVariantDataRowValue(object value, TdsParserStateObject stateObj, bool canAccumulate = true) {
            Debug.Assert(_isShiloh == true, "Shouldn't be dealing with sql_variant in pre-SQL2000 server!");

            // handle null values
            if ((null == value) || (DBNull.Value == value)) {
                WriteInt(TdsEnums.FIXEDNULL, stateObj);
                return null;
            }

            MetaType metatype = MetaType.GetMetaTypeFromValue(value);
            int length = 0;

            if (metatype.IsAnsiType) {
                length = GetEncodingCharLength((string)value, length, 0, _defaultEncoding);
            }

            switch (metatype.TDSType) {
                case TdsEnums.SQLFLT4:
                    WriteSqlVariantHeader(6, metatype.TDSType, metatype.PropBytes, stateObj);
                    WriteFloat((Single)value, stateObj);
                    break;

                case TdsEnums.SQLFLT8:
                    WriteSqlVariantHeader(10, metatype.TDSType, metatype.PropBytes, stateObj);
                    WriteDouble((Double)value, stateObj);
                    break;

                case TdsEnums.SQLINT8:
                    WriteSqlVariantHeader(10, metatype.TDSType, metatype.PropBytes, stateObj);
                    WriteLong((Int64)value, stateObj);
                    break;

                case TdsEnums.SQLINT4:
                    WriteSqlVariantHeader(6, metatype.TDSType, metatype.PropBytes, stateObj);
                    WriteInt((Int32)value, stateObj);
                    break;

                case TdsEnums.SQLINT2:
                    WriteSqlVariantHeader(4, metatype.TDSType, metatype.PropBytes, stateObj);
                    WriteShort((Int16)value, stateObj);
                    break;

                case TdsEnums.SQLINT1:
                    WriteSqlVariantHeader(3, metatype.TDSType, metatype.PropBytes, stateObj);
                    stateObj.WriteByte((byte)value);
                    break;

                case TdsEnums.SQLBIT:
                    WriteSqlVariantHeader(3, metatype.TDSType, metatype.PropBytes, stateObj);
                    if ((bool)value == true)
                        stateObj.WriteByte(1);
                    else
                        stateObj.WriteByte(0);

                    break;

                case TdsEnums.SQLBIGVARBINARY:
                    {
                        byte[] b = (byte[])value;

                        length = b.Length;
                        WriteSqlVariantHeader(4 + length, metatype.TDSType, metatype.PropBytes, stateObj);
                        WriteShort(length, stateObj); // propbytes: varlen
                        return stateObj.WriteByteArray(b, length, 0,  canAccumulate);
                    }

                case TdsEnums.SQLBIGVARCHAR:
                    {
                        string s = (string)value;

                        length = s.Length;
                        WriteSqlVariantHeader(9 + length, metatype.TDSType, metatype.PropBytes, stateObj);
                        WriteUnsignedInt(_defaultCollation.info, stateObj); // propbytes: collation.Info
                        stateObj.WriteByte(_defaultCollation.sortId); // propbytes: collation.SortId
                        WriteShort(length, stateObj);
                        return WriteEncodingChar(s, _defaultEncoding, stateObj, canAccumulate);
                    }

                case TdsEnums.SQLUNIQUEID: 
                    {
                        System.Guid guid = (System.Guid)value;
                        byte[] b = guid.ToByteArray();

                        length = b.Length;
                        Debug.Assert(length == 16, "Invalid length for guid type in com+ object");
                        WriteSqlVariantHeader(18, metatype.TDSType, metatype.PropBytes, stateObj);
                        stateObj.WriteByteArray(b, length, 0);
                        break;
                    }

                case TdsEnums.SQLNVARCHAR:
                    {
                        string s = (string)value;

                        length = s.Length * 2;
                        WriteSqlVariantHeader(9 + length, metatype.TDSType, metatype.PropBytes, stateObj);
                        WriteUnsignedInt(_defaultCollation.info, stateObj); // propbytes: collation.Info
                        stateObj.WriteByte(_defaultCollation.sortId); // propbytes: collation.SortId
                        WriteShort(length, stateObj); // propbyte: varlen

                        // string takes cchar, not cbyte so convert
                        length >>= 1;
                        return WriteString(s, length, 0, stateObj, canAccumulate);
                    }

                case TdsEnums.SQLDATETIME: 
                    {
                        TdsDateTime dt = MetaType.FromDateTime((DateTime)value, 8);

                        WriteSqlVariantHeader(10, metatype.TDSType, metatype.PropBytes, stateObj);
                        WriteInt(dt.days, stateObj);
                        WriteInt(dt.time, stateObj);
                        break;
                    }

                case TdsEnums.SQLMONEY: 
                    {
                        WriteSqlVariantHeader(10, metatype.TDSType, metatype.PropBytes, stateObj);
                        WriteCurrency((Decimal)value, 8, stateObj);
                        break;
                    }

                case TdsEnums.SQLNUMERICN: 
                    {
                        WriteSqlVariantHeader(21, metatype.TDSType, metatype.PropBytes, stateObj);
                        stateObj.WriteByte(metatype.Precision); //propbytes: precision
                        stateObj.WriteByte((byte)((Decimal.GetBits((Decimal)value)[3] & 0x00ff0000) >> 0x10)); // propbytes: scale
                        WriteDecimal((Decimal)value, stateObj);
                        break;
                    }

                case TdsEnums.SQLTIME:
                    WriteSqlVariantHeader(8, metatype.TDSType, metatype.PropBytes, stateObj);
                    stateObj.WriteByte(metatype.Scale); //propbytes: scale
                    WriteTime((TimeSpan)value, metatype.Scale, 5, stateObj);
                    break;

                case TdsEnums.SQLDATETIMEOFFSET:
                    WriteSqlVariantHeader(13, metatype.TDSType, metatype.PropBytes, stateObj);
                    stateObj.WriteByte(metatype.Scale); //propbytes: scale
                    WriteDateTimeOffset((DateTimeOffset)value, metatype.Scale, 10, stateObj);
                    break;

                default:
                    Debug.Assert(false, "unknown tds type for sqlvariant!");
                    break;
            } // switch
            // return point for accumualated writes, note: non-accumulated writes returned from their case statements
            return null;
        }

        internal void WriteSqlVariantHeader(int length, byte tdstype, byte propbytes, TdsParserStateObject stateObj) {
            WriteInt(length, stateObj);
            stateObj.WriteByte(tdstype);
            stateObj.WriteByte(propbytes);
        }

        internal void WriteSqlVariantDateTime2(DateTime value, TdsParserStateObject stateObj) 
        {
            MSS.SmiMetaData dateTime2MetaData = MSS.SmiMetaData.DefaultDateTime2;
            // NOTE: 3 bytes added here to support additional header information for variant - internal type, scale prop, scale
            WriteSqlVariantHeader((int)(dateTime2MetaData.MaxLength + 3), TdsEnums.SQLDATETIME2, 1 /* one scale prop */, stateObj);
            stateObj.WriteByte(dateTime2MetaData.Scale); //scale property
            WriteDateTime2(value, dateTime2MetaData.Scale, (int)(dateTime2MetaData.MaxLength), stateObj);
        }

        internal void WriteSqlVariantDate(DateTime value, TdsParserStateObject stateObj) 
        {
            MSS.SmiMetaData dateMetaData = MSS.SmiMetaData.DefaultDate;
            // NOTE: 2 bytes added here to support additional header information for variant - internal type, scale prop (ignoring scale here)
            WriteSqlVariantHeader((int)(dateMetaData.MaxLength + 2), TdsEnums.SQLDATE, 0 /* one scale prop */, stateObj);
            WriteDate(value, stateObj);
        }

        private byte[] SerializeSqlMoney(SqlMoney value, int length, TdsParserStateObject stateObj) {
            return SerializeCurrency(value.Value, length, stateObj);
        }

        private void WriteSqlMoney(SqlMoney value, int length, TdsParserStateObject stateObj) {
            // 
            int[] bits = Decimal.GetBits(value.Value);

            // this decimal should be scaled by 10000 (regardless of what the incoming decimal was scaled by)
            bool isNeg = (0 != (bits[3] & unchecked((int)0x80000000)));
            long l = ((long)(uint)bits[1]) << 0x20 | (uint)bits[0];

            if (isNeg)
                l = -l;

            if (length == 4) {
                Decimal decimalValue = value.Value;

                // validate the value can be represented as a small money
                if (decimalValue < TdsEnums.SQL_SMALL_MONEY_MIN || decimalValue > TdsEnums.SQL_SMALL_MONEY_MAX) {
                    throw SQL.MoneyOverflow(decimalValue.ToString(CultureInfo.InvariantCulture));
                }

                WriteInt((int)l, stateObj);
            }
            else {
                WriteInt((int)(l >> 0x20), stateObj);
                WriteInt((int)l, stateObj);
            }
        }

        private byte[] SerializeCurrency(Decimal value, int length, TdsParserStateObject stateObj) {
            SqlMoney m = new SqlMoney(value);
            int[] bits = Decimal.GetBits(m.Value);

            // this decimal should be scaled by 10000 (regardless of what the incoming decimal was scaled by)
            bool isNeg = (0 != (bits[3] & unchecked((int)0x80000000)));
            long l = ((long)(uint)bits[1]) << 0x20 | (uint)bits[0];

            if (isNeg)
                l = -l;

            if (length == 4) {
                // validate the value can be represented as a small money
                if (value < TdsEnums.SQL_SMALL_MONEY_MIN || value > TdsEnums.SQL_SMALL_MONEY_MAX) {
                    throw SQL.MoneyOverflow(value.ToString(CultureInfo.InvariantCulture));
                }

                // We normalize to allow conversion across data types. SMALLMONEY is serialized into a MONEY.
                length = 8;
            }

            Debug.Assert (8 == length, "invalid length in SerializeCurrency");
            if (null == stateObj._bLongBytes) {
                stateObj._bLongBytes = new byte[8];
            }

            byte[] bytes = stateObj._bLongBytes;
            int current = 0;

            byte[] bytesPart = SerializeInt((int)(l >> 0x20), stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;

            bytesPart = SerializeInt((int)l, stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);

            return bytes;
        }

        private void WriteCurrency(Decimal value, int length, TdsParserStateObject stateObj) {
            SqlMoney m = new SqlMoney(value);
            int[] bits = Decimal.GetBits(m.Value);

            // this decimal should be scaled by 10000 (regardless of what the incoming decimal was scaled by)
            bool isNeg = (0 != (bits[3] & unchecked((int)0x80000000)));
            long l = ((long)(uint)bits[1]) << 0x20 | (uint)bits[0];

            if (isNeg)
                l = -l;

            if (length == 4) {
                // validate the value can be represented as a small money
                if (value < TdsEnums.SQL_SMALL_MONEY_MIN || value > TdsEnums.SQL_SMALL_MONEY_MAX) {
                    throw SQL.MoneyOverflow(value.ToString(CultureInfo.InvariantCulture));
                }

                WriteInt((int)l, stateObj);
            }
            else {
                WriteInt((int)(l >> 0x20), stateObj);
                WriteInt((int)l, stateObj);
            }
        }

        private byte[] SerializeDate(DateTime value) {
            long days = value.Subtract(DateTime.MinValue).Days;
            return SerializePartialLong(days, 3);
        }

        private void WriteDate(DateTime value, TdsParserStateObject stateObj) {
            long days = value.Subtract(DateTime.MinValue).Days;
            WritePartialLong(days, 3, stateObj);
        }

        private byte[] SerializeTime(TimeSpan value, byte scale, int length) {
            if (0 > value.Ticks || value.Ticks >= TimeSpan.TicksPerDay) {
                throw SQL.TimeOverflow(value.ToString());
            }

            long time = value.Ticks / TdsEnums.TICKS_FROM_SCALE[scale];

            // We normalize to maximum precision to allow conversion across different precisions.
            time = time * TdsEnums.TICKS_FROM_SCALE[scale];
            length = TdsEnums.MAX_TIME_LENGTH;

            return SerializePartialLong(time, length);
        }

        private void WriteTime(TimeSpan value, byte scale, int length, TdsParserStateObject stateObj) {
            if (0 > value.Ticks || value.Ticks >= TimeSpan.TicksPerDay) {
                throw SQL.TimeOverflow(value.ToString());
            }
            long time = value.Ticks / TdsEnums.TICKS_FROM_SCALE[scale];
            WritePartialLong(time, length, stateObj);
        }

        private byte[] SerializeDateTime2(DateTime value, byte scale, int length) {
            long time = value.TimeOfDay.Ticks / TdsEnums.TICKS_FROM_SCALE[scale]; // DateTime.TimeOfDay always returns a valid TimeSpan for Time

            // We normalize to maximum precision to allow conversion across different precisions.
            time = time * TdsEnums.TICKS_FROM_SCALE[scale];
            length = TdsEnums.MAX_DATETIME2_LENGTH;

            byte[] bytes = new byte[length];
            byte[] bytesPart;
            int current = 0;

            bytesPart = SerializePartialLong(time, length - 3);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, length - 3);
            current += length - 3;

            bytesPart = SerializeDate(value);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 3);

            return bytes;
        }

        private void WriteDateTime2(DateTime value, byte scale, int length, TdsParserStateObject stateObj) {
            long time = value.TimeOfDay.Ticks / TdsEnums.TICKS_FROM_SCALE[scale]; // DateTime.TimeOfDay always returns a valid TimeSpan for Time
            WritePartialLong(time, length - 3, stateObj);
            WriteDate(value, stateObj);
        }

        private byte[] SerializeDateTimeOffset(DateTimeOffset value, byte scale, int length) {
            byte[] bytesPart;
            int current = 0;

            bytesPart = SerializeDateTime2(value.UtcDateTime, scale, length - 2);
            
            // We need to allocate the array after we have received the length of the serialized value
            // since it might be higher due to normalization.
            length = bytesPart.Length + 2;
            byte[] bytes = new byte[length];

            Buffer.BlockCopy(bytesPart, 0, bytes, current, length - 2);
            current += length - 2;

            Int16 offset = (Int16)value.Offset.TotalMinutes;
            bytes[current++] = (byte)(offset & 0xff);
            bytes[current++] = (byte)((offset >> 8) & 0xff);

            return bytes;
        }

        private void WriteDateTimeOffset(DateTimeOffset value, byte scale, int length, TdsParserStateObject stateObj) {
            WriteDateTime2(value.UtcDateTime, scale, length - 2, stateObj);
            Int16 offset = (Int16)value.Offset.TotalMinutes;
            stateObj.WriteByte((byte)(offset & 0xff));
            stateObj.WriteByte((byte)((offset >> 8) & 0xff));
        }

        private bool TryReadSqlDecimal(SqlBuffer value, int length, byte precision, byte scale, TdsParserStateObject stateObj) {
            byte byteValue;
            if (!stateObj.TryReadByte(out byteValue)) {
                return false;
            }
            bool fPositive = (1 == byteValue);

            length = checked((int)length-1);

            int[] bits;
            if (!TryReadDecimalBits(length, stateObj, out bits)) {
                return false;
            }

            value.SetToDecimal(precision, scale, fPositive, bits);
            return true;
        }

        // @devnote: length should be size of decimal without the sign
        // @devnote: sign should have already been read off the wire
        private bool TryReadDecimalBits(int length, TdsParserStateObject stateObj, out int[] bits) {
            bits = stateObj._decimalBits; // used alloc'd array if we have one already
            int i;

            if (null == bits)
                bits = new int[4];
            else {
                for (i = 0; i < bits.Length; i++)
                    bits[i] = 0;
            }

            Debug.Assert((length > 0) &&
                         (length <= TdsEnums.MAX_NUMERIC_LEN - 1) &&
                         (length % 4 == 0), "decimal should have 4, 8, 12, or 16 bytes of data");

            int decLength = length >> 2;

            for (i = 0; i < decLength; i++) {
                // up to 16 bytes of data following the sign byte
                if (!stateObj.TryReadInt32(out bits[i])) {
                    return false;
                }
            }

            return true;
        }

        static internal SqlDecimal AdjustSqlDecimalScale(SqlDecimal d, int newScale) {
            if (d.Scale != newScale) {
                return SqlDecimal.AdjustScale(d, newScale - d.Scale, false /* Don't round, truncate.  MDAC 69229 */);
            }

            return d;
        }

        static internal decimal AdjustDecimalScale(decimal value, int newScale) {
            int oldScale = (Decimal.GetBits(value)[3] & 0x00ff0000) >> 0x10;

            if (newScale != oldScale) {
                SqlDecimal num = new SqlDecimal(value);

                num = SqlDecimal.AdjustScale(num, newScale - oldScale, false /* Don't round, truncate.  MDAC 69229 */);
                return num.Value;
            }

            return value;
        }

        internal byte[] SerializeSqlDecimal(SqlDecimal d, TdsParserStateObject stateObj) {
            if (null == stateObj._bDecimalBytes) {
                stateObj._bDecimalBytes = new byte[17];
            }

            byte[] bytes = stateObj._bDecimalBytes;
            int current = 0;

            // sign
            if (d.IsPositive)
                bytes[current++] = 1;
            else
                bytes[current++] = 0;

            byte[] bytesPart = SerializeUnsignedInt(d.m_data1, stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;
            bytesPart = SerializeUnsignedInt(d.m_data2, stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;
            bytesPart = SerializeUnsignedInt(d.m_data3, stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;
            bytesPart = SerializeUnsignedInt(d.m_data4, stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            
            return bytes;
        }

        internal void WriteSqlDecimal(SqlDecimal d, TdsParserStateObject stateObj) {
            // sign
            if (d.IsPositive)
                stateObj.WriteByte(1);
            else
                stateObj.WriteByte(0);

            WriteUnsignedInt(d.m_data1, stateObj);
            WriteUnsignedInt(d.m_data2, stateObj);
            WriteUnsignedInt(d.m_data3, stateObj);
            WriteUnsignedInt(d.m_data4, stateObj);
        }

        private byte[] SerializeDecimal(decimal value, TdsParserStateObject stateObj) {
            int[] decimalBits = Decimal.GetBits(value);
            if (null == stateObj._bDecimalBytes) {
                stateObj._bDecimalBytes = new byte[17];
            }

            byte[] bytes = stateObj._bDecimalBytes;
            int current = 0;

            /*
             Returns a binary representation of a Decimal. The return value is an integer
             array with four elements. Elements 0, 1, and 2 contain the low, middle, and
             high 32 bits of the 96-bit integer part of the Decimal. Element 3 contains
             the scale factor and sign of the Decimal: bits 0-15 (the lower word) are
             unused; bits 16-23 contain a value between 0 and 28, indicating the power of
             10 to divide the 96-bit integer part by to produce the Decimal value; bits 24-
             30 are unused; and finally bit 31 indicates the sign of the Decimal value, 0
             meaning positive and 1 meaning negative.

             SQLDECIMAL/SQLNUMERIC has a byte stream of:
             struct {
                 BYTE sign; // 1 if positive, 0 if negative
                 BYTE data[];
             }

             For TDS 7.0 and above, there are always 17 bytes of data
            */

            // write the sign (note that COM and SQL are opposite)
            if (0x80000000 == (decimalBits[3] & 0x80000000))
                bytes[current++] = 0;
            else
                bytes[current++] = 1;

            byte[] bytesPart = SerializeInt(decimalBits[0], stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;
            bytesPart = SerializeInt(decimalBits[1], stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;
            bytesPart = SerializeInt(decimalBits[2], stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);
            current += 4;
            bytesPart = SerializeInt(0, stateObj);
            Buffer.BlockCopy(bytesPart, 0, bytes, current, 4);

            return bytes;
        }

        private void WriteDecimal(decimal value, TdsParserStateObject stateObj) {
            stateObj._decimalBits = Decimal.GetBits(value);
            Debug.Assert(null != stateObj._decimalBits, "decimalBits should be filled in at TdsExecuteRPC time");

            /*
             Returns a binary representation of a Decimal. The return value is an integer
             array with four elements. Elements 0, 1, and 2 contain the low, middle, and
             high 32 bits of the 96-bit integer part of the Decimal. Element 3 contains
             the scale factor and sign of the Decimal: bits 0-15 (the lower word) are
             unused; bits 16-23 contain a value between 0 and 28, indicating the power of
             10 to divide the 96-bit integer part by to produce the Decimal value; bits 24-
             30 are unused; and finally bit 31 indicates the sign of the Decimal value, 0
             meaning positive and 1 meaning negative.

             SQLDECIMAL/SQLNUMERIC has a byte stream of:
             struct {
                 BYTE sign; // 1 if positive, 0 if negative
                 BYTE data[];
             }

             For TDS 7.0 and above, there are always 17 bytes of data
            */

            // write the sign (note that COM and SQL are opposite)
            if (0x80000000 == (stateObj._decimalBits[3] & 0x80000000))
                stateObj.WriteByte(0);
            else
                stateObj.WriteByte(1);

            WriteInt(stateObj._decimalBits[0], stateObj);
            WriteInt(stateObj._decimalBits[1], stateObj);
            WriteInt(stateObj._decimalBits[2], stateObj);
            WriteInt(0, stateObj);
        }

        private void WriteIdentifier(string s, TdsParserStateObject stateObj) {
            if (null != s) {
                stateObj.WriteByte(checked((byte)s.Length));
                WriteString(s, stateObj);
            }
            else {
                stateObj.WriteByte((byte)0);
            }
        }

        private void WriteIdentifierWithShortLength(string s, TdsParserStateObject stateObj) {
            if (null != s) {
                WriteShort(checked((short)s.Length), stateObj);
                WriteString(s, stateObj);
            }
            else {
                WriteShort(0, stateObj);
            }
        }

        private Task WriteString(string s, TdsParserStateObject stateObj,  bool canAccumulate = true) {
            return WriteString(s, s.Length, 0, stateObj, canAccumulate);
        }

        internal byte[] SerializeCharArray(char[] carr, int length, int offset) {
            int cBytes = ADP.CharSize * length;
            byte[] bytes = new byte[cBytes];

            CopyCharsToBytes(carr, offset, bytes, 0, length);
            return bytes;
        }

        internal Task WriteCharArray(char[] carr, int length, int offset, TdsParserStateObject stateObj,  bool canAccumulate = true) {
            int cBytes = ADP.CharSize * length;

            // Perf shortcut: If it fits, write directly to the outBuff
            if(cBytes < (stateObj._outBuff.Length - stateObj._outBytesUsed)) {
                CopyCharsToBytes(carr, offset, stateObj._outBuff, stateObj._outBytesUsed, length);
                stateObj._outBytesUsed += cBytes;
                return null;
            }
            else {
                if (stateObj._bTmp == null || stateObj._bTmp.Length < cBytes) {
                    stateObj._bTmp = new byte[cBytes];
                }

                CopyCharsToBytes(carr, offset, stateObj._bTmp, 0, length);
                return stateObj.WriteByteArray(stateObj._bTmp, cBytes, 0,  canAccumulate);
            }
        }

        internal byte[] SerializeString(string s, int length, int offset) {
            int cBytes = ADP.CharSize * length;
            byte[] bytes = new byte[cBytes];

            CopyStringToBytes(s, offset, bytes, 0, length);
            return bytes;
        }

        internal Task WriteString(string s, int length, int offset, TdsParserStateObject stateObj, bool canAccumulate = true) {
            int cBytes = ADP.CharSize * length;

            // Perf shortcut: If it fits, write directly to the outBuff
            if(cBytes < (stateObj._outBuff.Length - stateObj._outBytesUsed)) {
                CopyStringToBytes(s, offset, stateObj._outBuff, stateObj._outBytesUsed, length);
                stateObj._outBytesUsed += cBytes;
                return null;
            }
            else {
                if (stateObj._bTmp == null || stateObj._bTmp.Length < cBytes) {
                    stateObj._bTmp = new byte[cBytes];
                }

                CopyStringToBytes(s, offset, stateObj._bTmp, 0, length);
                return stateObj.WriteByteArray(stateObj._bTmp, cBytes, 0,  canAccumulate);
            }
        }


        private unsafe static void CopyCharsToBytes(char[] source, int sourceOffset, byte[] dest, int destOffset, int charLength) {
            // DEVNOTE: BE EXTREMELY CAREFULL in this method.  Since it pins the buffers and copies the memory
            //  directly, it bypasses all of the CLR's usual array-bounds checking.  For maintainability, the checks
            //  here should NOT be removed just because some other code will check them ahead of time.
            if (charLength < 0) {
                throw ADP.InvalidDataLength(charLength);
            }

            if (checked(sourceOffset + charLength) > source.Length || sourceOffset < 0) {
                throw ADP.IndexOutOfRange(sourceOffset);
            }

            // charLength >= 0 & checked conversion implies byteLength >= 0
            int byteLength = checked(charLength * ADP.CharSize);

            if (checked(destOffset + byteLength) > dest.Length || destOffset < 0) {
                throw ADP.IndexOutOfRange(destOffset);
            }

            fixed (char* sourcePtr = source) {
                char* srcPtr = sourcePtr;  // Can't increment the target of a Fixed statement
                srcPtr += sourceOffset;  // char* increments by sizeof(char)
                fixed (byte* destinationPtr = dest) {
                    byte* destPtr = destinationPtr;
                    destPtr += destOffset;
                    NativeOledbWrapper.MemoryCopy((IntPtr)destPtr, (IntPtr)srcPtr, byteLength);
                }
            }
        }

        private unsafe static void CopyStringToBytes(string source, int sourceOffset, byte[] dest, int destOffset, int charLength) {
            // DEVNOTE: BE EXTREMELY CAREFULL in this method.  Since it pins the buffers and copies the memory
            //  directly, it bypasses all of the CLR's usual array-bounds checking.  For maintainability, the checks
            //  here should NOT be removed just because some other code will check them ahead of time.
            if (charLength < 0) {
                throw ADP.InvalidDataLength(charLength);
            }

            if (checked(sourceOffset + charLength) > source.Length || sourceOffset < 0) {
                throw ADP.IndexOutOfRange(sourceOffset);
            }

            // charLength >= 0 & checked conversion implies byteLength >= 0
            int byteLength = checked(charLength * ADP.CharSize);

            if (checked(destOffset + byteLength) > dest.Length || destOffset < 0) { 
                throw ADP.IndexOutOfRange(destOffset);
            }

            fixed (char* sourcePtr = source) {
                char* srcPtr = sourcePtr;  // Can't increment the target of a Fixed statement
                srcPtr += sourceOffset;  // char* increments by sizeof(char)
                fixed (byte* destinationPtr = dest) {
                    byte* destPtr = destinationPtr;
                    destPtr += destOffset;
                    NativeOledbWrapper.MemoryCopy((IntPtr)destPtr, (IntPtr)srcPtr, byteLength);
                }
            }
        }

        private Task WriteEncodingChar(string s, Encoding encoding, TdsParserStateObject stateObj,  bool canAccumulate = true) {
            return WriteEncodingChar(s, s.Length, 0, encoding, stateObj,  canAccumulate);
        }

        private byte[] SerializeEncodingChar(string s, int numChars, int offset, Encoding encoding) {
            char[] charData;
            byte[] byteData = null;

            // if hitting 7.0 server, encoding will be null in metadata for columns or return values since
            // 7.0 has no support for multiple code pages in data - single code page support only
            if (encoding == null)
                encoding = _defaultEncoding;
            
            charData = s.ToCharArray(offset, numChars);

            byteData = new byte[encoding.GetByteCount(charData, 0, charData.Length)];
            encoding.GetBytes(charData, 0, charData.Length, byteData, 0);

            return byteData;
        }

        private Task WriteEncodingChar(string s, int numChars, int offset, Encoding encoding, TdsParserStateObject stateObj,  bool canAccumulate = true) {
            char[] charData;
            byte[] byteData;

            // if hitting 7.0 server, encoding will be null in metadata for columns or return values since
            // 7.0 has no support for multiple code pages in data - single code page support only
            if (encoding == null)
                encoding = _defaultEncoding;

            charData = s.ToCharArray(offset, numChars);

            // Optimization: if the entire string fits in the current buffer, then copy it directly
            int bytesLeft = stateObj._outBuff.Length - stateObj._outBytesUsed;
            if ((numChars <= bytesLeft) && (encoding.GetMaxByteCount(charData.Length) <= bytesLeft)) {
                int bytesWritten = encoding.GetBytes(charData, 0, charData.Length, stateObj._outBuff, stateObj._outBytesUsed);
                stateObj._outBytesUsed += bytesWritten;
                return null;
            }
            else {
                byteData = encoding.GetBytes(charData, 0, numChars);
                Debug.Assert(byteData != null, "no data from encoding");
                return stateObj.WriteByteArray(byteData, byteData.Length, 0,  canAccumulate);
            }
        }

        internal int GetEncodingCharLength(string value, int numChars, int charOffset, Encoding encoding) {
            // 

            if (value == null || value == ADP.StrEmpty) {
                return 0;
            }

            // if hitting 7.0 server, encoding will be null in metadata for columns or return values since
            // 7.0 has no support for multiple code pages in data - single code page support only
            if (encoding == null) {
                if (null == _defaultEncoding) {
                    ThrowUnsupportedCollationEncountered(null);
                }

                encoding = _defaultEncoding;
            }

            char[] charData = value.ToCharArray(charOffset, numChars);

            return encoding.GetByteCount(charData, 0, numChars);
        }

        //
        // Returns the data stream length of the data identified by tds type or SqlMetaData returns
        // Returns either the total size or the size of the first chunk for partially length prefixed types.
        //
        internal bool TryGetDataLength(SqlMetaDataPriv colmeta, TdsParserStateObject stateObj, out ulong length) {
            // Handle Yukon specific tokens
            if (_isYukon && colmeta.metaType.IsPlp) {
                Debug.Assert(colmeta.tdsType == TdsEnums.SQLXMLTYPE ||
                             colmeta.tdsType == TdsEnums.SQLBIGVARCHAR ||
                             colmeta.tdsType == TdsEnums.SQLBIGVARBINARY ||
                             colmeta.tdsType == TdsEnums.SQLNVARCHAR ||
                             // Large UDTs is WinFS-only
                             colmeta.tdsType == TdsEnums.SQLUDT,
                             "GetDataLength:Invalid streaming datatype");
                return stateObj.TryReadPlpLength(true, out length);
            }
            else {
                int intLength;
                if (!TryGetTokenLength(colmeta.tdsType, stateObj, out intLength)) {
                    length = 0;
                    return false;
                }
                length = (ulong)intLength;
                return true;
            }
        }

        //
        // returns the token length of the token or tds type
        // Returns -1 for partially length prefixed (plp) types for metadata info.
        // DOES NOT handle plp data streams correctly!!!
        // Plp data streams length information should be obtained from GetDataLength
        //
        internal bool TryGetTokenLength(byte token, TdsParserStateObject stateObj, out int tokenLength) {
            Debug.Assert(token != 0, "0 length token!");

            switch (token) { // rules about SQLLenMask no longer apply to new tokens (as of 7.4)
                case TdsEnums.SQLFEATUREEXTACK: 
                    tokenLength = -1;
                    return true;
                case TdsEnums.SQLSESSIONSTATE:
                case TdsEnums.SQLFEDAUTHINFO:
                    return stateObj.TryReadInt32(out tokenLength);
            }

            if (_isYukon) {     // Handle Yukon specific exceptions
                if (token == TdsEnums.SQLUDT) { // special case for UDTs
                    tokenLength = -1; // Should we return -1 or not call GetTokenLength for UDTs?
                    return true;
                }
                else if (token == TdsEnums.SQLRETURNVALUE) {
                    tokenLength = -1; // In Yukon, the RETURNVALUE token stream no longer has length
                    return true;
                }
                else if (token == TdsEnums.SQLXMLTYPE) {
                    ushort value;
                    if (!stateObj.TryReadUInt16(out value)) {
                        tokenLength = 0;
                        return false;
                    }
                    tokenLength = (int)value;
                    Debug.Assert(tokenLength == TdsEnums.SQL_USHORTVARMAXLEN, "Invalid token stream for xml datatype");
                    return true;
                }
            }

            switch (token & TdsEnums.SQLLenMask) {
                case TdsEnums.SQLFixedLen:
                    tokenLength = ((0x01 << ((token & 0x0c) >> 2))) & 0xff;
                    return true;
                case TdsEnums.SQLZeroLen:
                    tokenLength = 0;
                    return true;
                case TdsEnums.SQLVarLen:
                case TdsEnums.SQLVarCnt:
                    if (0 != (token & 0x80)) {
                        ushort value;
                        if (!stateObj.TryReadUInt16(out value)) {
                            tokenLength = 0;
                            return false;
                        }
                        tokenLength = value;
                        return true;
                    }
                    else if (0 == (token & 0x0c)) {
                        // 
                        if (!stateObj.TryReadInt32(out tokenLength)) {
                            return false;
                        }
                        return true;
                    }
                    else {
                        byte value;
                        if (!stateObj.TryReadByte(out value)) {
                            tokenLength = 0;
                            return false;
                        }
                        tokenLength = value;
                        return true;
                    }
                default:
                    Debug.Assert(false, "Unknown token length!");
                    tokenLength = 0;
                    return true;
            }
        }

        private void ProcessAttention(TdsParserStateObject stateObj) {
            if (_state == TdsParserState.Closed || _state == TdsParserState.Broken){
                return;
            }
            Debug.Assert(stateObj._attentionSent, "invalid attempt to ProcessAttention, attentionSent == false!");

            // Attention processing scenarios:
            // 1) EOM packet with header ST_AACK bit plus DONE with status DONE_ATTN
            // 2) Packet without ST_AACK header bit but has DONE with status DONE_ATTN
            // 3) Secondary timeout occurs while reading, break connection

            // Since errors can occur and we need to cancel prior to throwing those errors, we
            // cache away error state and then process TDS for the attention.  We restore those
            // errors after processing.
            stateObj.StoreErrorAndWarningForAttention();

            try {
                // Call run loop to process looking for attention ack.
                Run(RunBehavior.Attention, null, null, null, stateObj);
            }
            catch (Exception e) {
                // 
                if (!ADP.IsCatchableExceptionType(e)) {
                    throw;
                }

                // If an exception occurs - break the connection.
                // Attention error will not be thrown in this case by Run(), but other failures may.
                ADP.TraceExceptionWithoutRethrow(e);
                _state = TdsParserState.Broken;
                _connHandler.BreakConnection();

                throw;
            }

            stateObj.RestoreErrorAndWarningAfterAttention();                        
                
            Debug.Assert(!stateObj._attentionSent, "Invalid attentionSent state at end of ProcessAttention");
        }
       
        static private int StateValueLength(int dataLen) {
            return dataLen < 0xFF ? (dataLen + 1) : (dataLen + 5);
        }
                
        internal int WriteSessionRecoveryFeatureRequest(SessionData reconnectData, bool write /* if false just calculates the length */) {
            int len = 1;
            if (write) {
                _physicalStateObj.WriteByte(TdsEnums.FEATUREEXT_SRECOVERY);
            }
            if (reconnectData == null) {
                if (write) {
                    WriteInt(0, _physicalStateObj);
                }
                len += 4;
            }
            else {
                Debug.Assert(reconnectData._unrecoverableStatesCount == 0, "Unrecoverable state count should be 0");
                int initialLength = 0; // sizeof(DWORD) - length itself
                initialLength += 1 + 2 * TdsParserStaticMethods.NullAwareStringLength(reconnectData._initialDatabase);
                initialLength += 1 + 2 * TdsParserStaticMethods.NullAwareStringLength(reconnectData._initialLanguage);
                initialLength += (reconnectData._initialCollation == null) ? 1 : 6;
                for (int i = 0; i < SessionData._maxNumberOfSessionStates; i++) {
                    if (reconnectData._initialState[i] != null) {
                        initialLength += 1 /* StateId*/ + StateValueLength(reconnectData._initialState[i].Length);
                    }
                }
                int currentLength = 0; // sizeof(DWORD) - length itself                
                currentLength += 1 + 2 * (reconnectData._initialDatabase == reconnectData._database ? 0 : TdsParserStaticMethods.NullAwareStringLength(reconnectData._database));
                currentLength += 1 + 2 * (reconnectData._initialLanguage == reconnectData._language ? 0 : TdsParserStaticMethods.NullAwareStringLength(reconnectData._language));
                currentLength += (reconnectData._collation != null && !SqlCollation.AreSame(reconnectData._collation, reconnectData._initialCollation)) ? 6 : 1;
                bool[] writeState = new bool[SessionData._maxNumberOfSessionStates];
                for (int i = 0; i < SessionData._maxNumberOfSessionStates; i++) {
                    if (reconnectData._delta[i] != null) {
                        Debug.Assert(reconnectData._delta[i]._recoverable, "State should be recoverable");
                        writeState[i] = true;
                        if (reconnectData._initialState[i] != null && reconnectData._initialState[i].Length == reconnectData._delta[i]._dataLength) {
                            writeState[i] = false;
                            for (int j = 0; j < reconnectData._delta[i]._dataLength; j++) {
                                if (reconnectData._initialState[i][j] != reconnectData._delta[i]._data[j]) {
                                    writeState[i] = true;
                                    break;
                                }
                            }
                        }
                        if (writeState[i]) {
                            currentLength += 1 /* StateId*/ + StateValueLength(reconnectData._delta[i]._dataLength);
                        }
                    }
                }                
                if (write) {
                    WriteInt(8 + initialLength + currentLength, _physicalStateObj); // length of data w/o total length (initil+current+2*sizeof(DWORD))
                    WriteInt(initialLength, _physicalStateObj);
                    WriteIdentifier(reconnectData._initialDatabase, _physicalStateObj);
                    WriteCollation(reconnectData._initialCollation, _physicalStateObj);
                    WriteIdentifier(reconnectData._initialLanguage, _physicalStateObj);
                    for (int i = 0; i < SessionData._maxNumberOfSessionStates; i++) {
                        if (reconnectData._initialState[i] != null) {
                            _physicalStateObj.WriteByte((byte)i);
                            if (reconnectData._initialState[i].Length < 0xFF) {
                                _physicalStateObj.WriteByte((byte)reconnectData._initialState[i].Length);
                            }
                            else {
                                _physicalStateObj.WriteByte(0xFF);
                                WriteInt(reconnectData._initialState[i].Length, _physicalStateObj);
                            }
                            _physicalStateObj.WriteByteArray(reconnectData._initialState[i], reconnectData._initialState[i].Length, 0);
                        }
                    }
                    WriteInt(currentLength, _physicalStateObj);
                    WriteIdentifier(reconnectData._database != reconnectData._initialDatabase ? reconnectData._database : null, _physicalStateObj);
                    WriteCollation(SqlCollation.AreSame(reconnectData._initialCollation, reconnectData._collation) ? null : reconnectData._collation, _physicalStateObj);
                    WriteIdentifier(reconnectData._language != reconnectData._initialLanguage ? reconnectData._language : null, _physicalStateObj);
                    for (int i = 0; i < SessionData._maxNumberOfSessionStates; i++) {
                        if (writeState[i]) {
                            _physicalStateObj.WriteByte((byte)i);
                            if (reconnectData._delta[i]._dataLength < 0xFF) {
                                _physicalStateObj.WriteByte((byte)reconnectData._delta[i]._dataLength);
                            }
                            else {
                                _physicalStateObj.WriteByte(0xFF);
                                WriteInt(reconnectData._delta[i]._dataLength, _physicalStateObj);
                            }
                            _physicalStateObj.WriteByteArray(reconnectData._delta[i]._data, reconnectData._delta[i]._dataLength, 0);
                        }
                    }
                }
                len += initialLength + currentLength + 12 /* length fields (initial, current, total) */;
            }
            return len;
        }

        internal int WriteFedAuthFeatureRequest(FederatedAuthenticationFeatureExtensionData fedAuthFeatureData,
                                                bool write /* if false just calculates the length */) {
            Debug.Assert(fedAuthFeatureData.libraryType == TdsEnums.FedAuthLibrary.ADAL || fedAuthFeatureData.libraryType == TdsEnums.FedAuthLibrary.SecurityToken,
                "only fed auth library type ADAL and Security Token are supported in writing feature request");

            int dataLen = 0;
            int totalLen = 0;

            // set dataLen and totalLen
            switch (fedAuthFeatureData.libraryType) {
                case TdsEnums.FedAuthLibrary.ADAL:
                    dataLen = 2;  // length of feature data = 1 byte for library and echo + 1 byte for workflow
                    break;
                case TdsEnums.FedAuthLibrary.SecurityToken:
                    Debug.Assert(fedAuthFeatureData.accessToken != null, "AccessToken should not be null.");
                    dataLen = 1 + sizeof(int) + fedAuthFeatureData.accessToken.Length; // length of feature data = 1 byte for library and echo, security token length and sizeof(int) for token lengh itself
                    break;
               default:
                    Debug.Assert(false, "Unrecognized library type for fedauth feature extension request");
                    break;
            }

            totalLen = dataLen + 5; // length of feature id (1 byte), data length field (4 bytes), and feature data (dataLen)

            // write feature id
            if (write) {
                _physicalStateObj.WriteByte(TdsEnums.FEATUREEXT_FEDAUTH);

                // set options
                byte options = 0x00;

                // set upper 7 bits of options to indicate fed auth library type
                switch (fedAuthFeatureData.libraryType) {
                    case TdsEnums.FedAuthLibrary.ADAL:
                        Debug.Assert(_connHandler._federatedAuthenticationInfoRequested == true, "_federatedAuthenticationInfoRequested field should be true");
                        options |= TdsEnums.FEDAUTHLIB_ADAL << 1;
                        break;
                    case TdsEnums.FedAuthLibrary.SecurityToken:
                        Debug.Assert(_connHandler._federatedAuthenticationRequested == true, "_federatedAuthenticationRequested field should be true");
                        options |= TdsEnums.FEDAUTHLIB_SECURITYTOKEN << 1;
                        break;
                    default:
                        Debug.Assert(false, "Unrecognized FedAuthLibrary type for feature extension request");
                        break;
                }

                options |= (byte)(fedAuthFeatureData.fedAuthRequiredPreLoginResponse == true ? 0x01 : 0x00);

                // write dataLen and options
                WriteInt(dataLen, _physicalStateObj);
                _physicalStateObj.WriteByte(options);

                // write workflow for FedAuthLibrary.ADAL
                // write accessToken for FedAuthLibrary.SecurityToken
                switch (fedAuthFeatureData.libraryType) {
                    case TdsEnums.FedAuthLibrary.ADAL:
                        byte workflow = 0x00;
                        switch (fedAuthFeatureData.authentication) {
                            case SqlAuthenticationMethod.ActiveDirectoryPassword:
                                workflow = TdsEnums.ADALWORKFLOW_ACTIVEDIRECTORYPASSWORD;
                                break;
                            case SqlAuthenticationMethod.ActiveDirectoryIntegrated:
                                workflow = TdsEnums.ADALWORKFLOW_ACTIVEDIRECTORYINTEGRATED;
                                break;
                            default:
                                Debug.Assert(false, "Unrecognized Authentication type for fedauth ADAL request");
                                break;
                        }

                        _physicalStateObj.WriteByte(workflow);
                        break;
                    case TdsEnums.FedAuthLibrary.SecurityToken:
                        WriteInt(fedAuthFeatureData.accessToken.Length, _physicalStateObj);
                        _physicalStateObj.WriteByteArray(fedAuthFeatureData.accessToken, fedAuthFeatureData.accessToken.Length, 0);
                        break;
                    default:
                        Debug.Assert(false, "Unrecognized FedAuthLibrary type for feature extension request");
                        break;
                 }
            }
            return totalLen;
        }

        internal int WriteTceFeatureRequest (bool write /* if false just calculates the length */) {
            int len = 6; // (1byte = featureID, 4bytes = featureData length, 1 bytes = Version

            if (write) {
                // Write Feature ID, legth of the version# field and TCE Version#
                _physicalStateObj.WriteByte(TdsEnums.FEATUREEXT_TCE);
                WriteInt (1, _physicalStateObj);
                _physicalStateObj.WriteByte(TdsEnums.MAX_SUPPORTED_TCE_VERSION);
            }

            return len; // size of data written
        }

        internal int WriteGlobalTransactionsFeatureRequest(bool write /* if false just calculates the length */) {
            int len = 5; // 1byte = featureID, 4bytes = featureData length

            if (write) {
                // Write Feature ID
                _physicalStateObj.WriteByte(TdsEnums.FEATUREEXT_GLOBALTRANSACTIONS);
                WriteInt(0, _physicalStateObj); // we don't send any data
            }

            return len;
        }

        internal void TdsLogin(SqlLogin rec,
                               TdsEnums.FeatureExtension requestedFeatures,
                               SessionData recoverySessionData,
                               FederatedAuthenticationFeatureExtensionData? fedAuthFeatureExtensionData) {
            _physicalStateObj.SetTimeoutSeconds(rec.timeout);

            Debug.Assert(recoverySessionData == null || (requestedFeatures & TdsEnums.FeatureExtension.SessionRecovery) != 0, "Recovery session data without session recovery feature request");
            Debug.Assert(TdsEnums.MAXLEN_HOSTNAME>=rec.hostName.Length, "_workstationId.Length exceeds the max length for this value");

            Debug.Assert(!(rec.useSSPI && _connHandler._fedAuthRequired), "Cannot use SSPI when server has responded 0x01 for FedAuthRequired PreLogin Option.");
            Debug.Assert(!rec.useSSPI || (requestedFeatures & TdsEnums.FeatureExtension.FedAuth) == 0, "Cannot use both SSPI and FedAuth");
            Debug.Assert(fedAuthFeatureExtensionData == null || (requestedFeatures & TdsEnums.FeatureExtension.FedAuth) != 0, "fedAuthFeatureExtensionData provided without fed auth feature request");
            Debug.Assert(fedAuthFeatureExtensionData != null || (requestedFeatures & TdsEnums.FeatureExtension.FedAuth) == 0, "Fed Auth feature requested without specifying fedAuthFeatureExtensionData.");

            Debug.Assert(rec.userName == null || (rec.userName != null && TdsEnums.MAXLEN_USERNAME >= rec.userName.Length), "_userID.Length exceeds the max length for this value");
            Debug.Assert(rec.credential == null || (rec.credential != null && TdsEnums.MAXLEN_USERNAME >= rec.credential.UserId.Length), "_credential.UserId.Length exceeds the max length for this value");

            Debug.Assert(rec.password == null || (rec.password != null && TdsEnums.MAXLEN_PASSWORD>=rec.password.Length), "_password.Length exceeds the max length for this value");
            Debug.Assert(rec.credential == null || (rec.credential != null && TdsEnums.MAXLEN_PASSWORD >= rec.credential.Password.Length), "_credential.Password.Length exceeds the max length for this value");

            Debug.Assert(rec.credential != null || rec.userName != null || rec.password != null, "cannot mix the new secure password system and the connection string based password");
            Debug.Assert(rec.newSecurePassword != null || rec.newPassword != null, "cannot have both new secure change password and string based change password");
            Debug.Assert(TdsEnums.MAXLEN_APPNAME>=rec.applicationName.Length, "_applicationName.Length exceeds the max length for this value");
            Debug.Assert(TdsEnums.MAXLEN_SERVERNAME>=rec.serverName.Length, "_dataSource.Length exceeds the max length for this value");
            Debug.Assert(TdsEnums.MAXLEN_LANGUAGE>=rec.language.Length, "_currentLanguage .Length exceeds the max length for this value");
            Debug.Assert(TdsEnums.MAXLEN_DATABASE>=rec.database.Length, "_initialCatalog.Length exceeds the max length for this value");
            Debug.Assert(TdsEnums.MAXLEN_ATTACHDBFILE>=rec.attachDBFilename.Length, "_attachDBFileName.Length exceeds the max length for this value");

            Debug.Assert(_connHandler != null, "SqlConnectionInternalTds handler can not be null at this point.");
            _connHandler.TimeoutErrorInternal.EndPhase(SqlConnectionTimeoutErrorPhase.LoginBegin);
            _connHandler.TimeoutErrorInternal.SetAndBeginPhase(SqlConnectionTimeoutErrorPhase.ProcessConnectionAuth);

            // get the password up front to use in sspi logic below
            byte[] encryptedPassword = null;
            byte[] encryptedChangePassword = null;
            int encryptedPasswordLengthInBytes;
            int encryptedChangePasswordLengthInBytes;
            bool useFeatureExt = (requestedFeatures != TdsEnums.FeatureExtension.None);

            string userName;

            if (rec.credential != null) {
                userName = rec.credential.UserId;
                encryptedPasswordLengthInBytes = rec.credential.Password.Length * 2;
            }
            else {
                userName = rec.userName;
                encryptedPassword = TdsParserStaticMethods.EncryptPassword(rec.password);
                encryptedPasswordLengthInBytes = encryptedPassword.Length;  // password in clear text is already encrypted and its length is in byte
            }

            if (rec.newSecurePassword != null) {
                encryptedChangePasswordLengthInBytes = rec.newSecurePassword.Length * 2;
            }
            else {
                encryptedChangePassword = TdsParserStaticMethods.EncryptPassword(rec.newPassword);
                encryptedChangePasswordLengthInBytes = encryptedChangePassword.Length;
            }


            // set the message type
            _physicalStateObj._outputMessageType = TdsEnums.MT_LOGIN7;

            // length in bytes
            int length = TdsEnums.YUKON_LOG_REC_FIXED_LEN;

            string clientInterfaceName = TdsEnums.SQL_PROVIDER_NAME;
            Debug.Assert(TdsEnums.MAXLEN_CLIENTINTERFACE >= clientInterfaceName.Length, "cchCltIntName can specify at most 128 unicode characters. See Tds spec");

            // add up variable-len portions (multiply by 2 for byte len of char strings)
            //
            checked {
                length += (rec.hostName.Length + rec.applicationName.Length +
                            rec.serverName.Length + clientInterfaceName.Length +
                            rec.language.Length + rec.database.Length +
                            rec.attachDBFilename.Length) * 2;
                if (useFeatureExt) {
                    length += 4;
                }
            }

            // allocate memory for SSPI variables
            byte[] outSSPIBuff = null;
            UInt32 outSSPILength = 0;

            // only add lengths of password and username if not using SSPI or requesting federated authentication info
            if (!rec.useSSPI && !(_connHandler._federatedAuthenticationInfoRequested || _connHandler._federatedAuthenticationRequested)) {
                checked {
                    length += (userName.Length * 2) + encryptedPasswordLengthInBytes
                    + encryptedChangePasswordLengthInBytes;
                }
            }
            else {
                if (rec.useSSPI) {
                    // now allocate proper length of buffer, and set length
                    outSSPIBuff = new byte[s_maxSSPILength];
                    outSSPILength = s_maxSSPILength;

                    // Call helper function for SSPI data and actual length.
                    // Since we don't have SSPI data from the server, send null for the
                    // byte[] buffer and 0 for the int length.
                    Debug.Assert(SniContext.Snix_Login==_physicalStateObj.SniContext, String.Format((IFormatProvider)null, "Unexpected SniContext. Expecting Snix_Login, actual value is '{0}'", _physicalStateObj.SniContext));
                    _physicalStateObj.SniContext = SniContext.Snix_LoginSspi;
                    SSPIData(null, 0, outSSPIBuff, ref outSSPILength);
                    if (outSSPILength > Int32.MaxValue) {
                        throw SQL.InvalidSSPIPacketSize();  // SqlBu 332503
                    }
                    _physicalStateObj.SniContext=SniContext.Snix_Login;

                    checked {
                        length += (Int32)outSSPILength;
                    }
                }
            }

            int feOffset = length;

            if (useFeatureExt) {
                checked {
                    if ((requestedFeatures & TdsEnums.FeatureExtension.SessionRecovery) != 0) {
                        length += WriteSessionRecoveryFeatureRequest(recoverySessionData, false);
                    };
                    if ((requestedFeatures & TdsEnums.FeatureExtension.FedAuth) != 0) {
                        Debug.Assert(fedAuthFeatureExtensionData != null, "fedAuthFeatureExtensionData should not null.");
                        length += WriteFedAuthFeatureRequest(fedAuthFeatureExtensionData.Value, write:false);
                    }
                    if ((requestedFeatures & TdsEnums.FeatureExtension.Tce) != 0) {
                        length += WriteTceFeatureRequest (false);
                    }
                    if ((requestedFeatures & TdsEnums.FeatureExtension.GlobalTransactions) != 0) {
                        length += WriteGlobalTransactionsFeatureRequest(false);
                    }
                    length++; // for terminator
                }
            }

            try {
                WriteInt(length, _physicalStateObj);
                if (recoverySessionData == null) {
                    WriteInt((TdsEnums.DENALI_MAJOR << 24) | (TdsEnums.DENALI_INCREMENT << 16) | TdsEnums.DENALI_MINOR, _physicalStateObj);
                }
                else {
                    WriteUnsignedInt(recoverySessionData._tdsVersion, _physicalStateObj);
                }
                WriteInt(rec.packetSize, _physicalStateObj);
                WriteInt(TdsEnums.CLIENT_PROG_VER, _physicalStateObj);
                WriteInt(TdsParserStaticMethods.GetCurrentProcessIdForTdsLoginOnly(), _physicalStateObj); //MDAC 84718
                WriteInt(0, _physicalStateObj); // connectionID is unused

                // Log7Flags (DWORD)
                int log7Flags = 0;

                /*
                 Current snapshot from TDS spec with the offsets added:
                    0) fByteOrder:1,                // byte order of numeric data types on client
                    1) fCharSet:1,                  // character set on client
                    2) fFloat:2,                    // Type of floating point on client
                    4) fDumpLoad:1,                 // Dump/Load and BCP enable
                    5) fUseDb:1,                    // USE notification
                    6) fDatabase:1,                 // Initial database fatal flag
                    7) fSetLang:1,                  // SET LANGUAGE notification
                    8) fLanguage:1,                 // Initial language fatal flag
                    9) fODBC:1,                     // Set if client is ODBC driver
                   10) fTranBoundary:1,             // Transaction boundary notification
                   11) fDelegatedSec:1,             // Security with delegation is available
                   12) fUserType:3,                 // Type of user
                   15) fIntegratedSecurity:1,       // Set if client is using integrated security
                   16) fSQLType:4,                  // Type of SQL sent from client
                   20) fOLEDB:1,                    // Set if client is OLEDB driver
                   21) fSpare1:3,                   // first bit used for read-only intent, rest unused
                   24) fResetPassword:1,            // set if client wants to reset password
                   25) fNoNBCAndSparse:1,           // set if client does not support NBC and Sparse column
                   26) fUserInstance:1,             // This connection wants to connect to a SQL "user instance"
                   27) fUnknownCollationHandling:1, // This connection can handle unknown collation correctly.
                   28) fExtension:1                 // Extensions are used                 
                   32 - total
                */

                // first byte
                log7Flags |= TdsEnums.USE_DB_ON << 5;
                log7Flags |= TdsEnums.INIT_DB_FATAL << 6;
                log7Flags |= TdsEnums.SET_LANG_ON << 7;

                // second byte
                log7Flags |= TdsEnums.INIT_LANG_FATAL << 8;
                log7Flags |= TdsEnums.ODBC_ON << 9;
                if (rec.useReplication) {
                    log7Flags |=  TdsEnums.REPL_ON << 12;
                }
                if (rec.useSSPI) {
                    log7Flags |=  TdsEnums.SSPI_ON << 15;
                }

                // third byte
                if (rec.readOnlyIntent) {
                    log7Flags |=  TdsEnums.READONLY_INTENT_ON << 21; // read-only intent flag is a first bit of fSpare1
                }

                // 4th one
                if (!ADP.IsEmpty(rec.newPassword) || (rec.newSecurePassword != null && rec.newSecurePassword.Length != 0)) {
                    log7Flags |=  1 << 24;
                }
                if (rec.userInstance) {
                    log7Flags |=  1 << 26;
                }

                if (useFeatureExt) {
                    log7Flags |=  1 << 28;
                }

                WriteInt(log7Flags, _physicalStateObj);
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.TdsParser.TdsLogin|ADV> %d#, TDS Login7 flags = %d:\n", ObjectID, log7Flags);
                }

                WriteInt(0, _physicalStateObj);  // ClientTimeZone is not used
                WriteInt(0, _physicalStateObj);  // LCID is unused by server

                // Start writing offset and length of variable length portions
                int offset = TdsEnums.YUKON_LOG_REC_FIXED_LEN;

                // write offset/length pairs

                // note that you must always set ibHostName since it indicaters the beginning of the variable length section of the login record
                WriteShort(offset, _physicalStateObj); // host name offset
                WriteShort(rec.hostName.Length, _physicalStateObj);
                offset += rec.hostName.Length * 2;

                // Only send user/password over if not fSSPI or fed auth ADAL...  If both user/password and SSPI are in login
                // rec, only SSPI is used.  Confirmed same bahavior as in luxor.
                if (!rec.useSSPI && !(_connHandler._federatedAuthenticationInfoRequested || _connHandler._federatedAuthenticationRequested)) {
                    WriteShort(offset, _physicalStateObj);  // userName offset
                    WriteShort(userName.Length, _physicalStateObj);
                    offset += userName.Length * 2;

                    // the encrypted password is a byte array - so length computations different than strings
                    WriteShort(offset, _physicalStateObj); // password offset
                    WriteShort(encryptedPasswordLengthInBytes / 2, _physicalStateObj);
                    offset += encryptedPasswordLengthInBytes;
                }
                else {
                    // case where user/password data is not used, send over zeros
                    WriteShort(0, _physicalStateObj);  // userName offset
                    WriteShort(0, _physicalStateObj);
                    WriteShort(0, _physicalStateObj);  // password offset
                    WriteShort(0, _physicalStateObj);
                }

                WriteShort(offset, _physicalStateObj); // app name offset
                WriteShort(rec.applicationName.Length, _physicalStateObj);
                offset += rec.applicationName.Length * 2;

                WriteShort(offset, _physicalStateObj); // server name offset
                WriteShort(rec.serverName.Length, _physicalStateObj);
                offset += rec.serverName.Length * 2;

                WriteShort(offset, _physicalStateObj);
                if (useFeatureExt) {
                    WriteShort(4, _physicalStateObj); // length of ibFeatgureExtLong (which is a DWORD)
                    offset += 4;
                }
                else {
                    WriteShort(0, _physicalStateObj); // unused (was remote password ?)
                }

                WriteShort(offset, _physicalStateObj); // client interface name offset
                WriteShort(clientInterfaceName.Length, _physicalStateObj);
                offset += clientInterfaceName.Length * 2;

                WriteShort(offset, _physicalStateObj); // language name offset
                WriteShort(rec.language.Length, _physicalStateObj);
                offset += rec.language.Length * 2;

                WriteShort(offset, _physicalStateObj); // database name offset
                WriteShort(rec.database.Length, _physicalStateObj);
                offset += rec.database.Length * 2;

                // 

                if (null == s_nicAddress)
                    s_nicAddress = TdsParserStaticMethods.GetNetworkPhysicalAddressForTdsLoginOnly();

                _physicalStateObj.WriteByteArray(s_nicAddress, s_nicAddress.Length, 0);

                WriteShort(offset, _physicalStateObj); // ibSSPI offset
                if (rec.useSSPI) {
                    WriteShort((int)outSSPILength, _physicalStateObj);
                    offset += (int)outSSPILength;
                }
                else {
                    WriteShort(0, _physicalStateObj);
                }

                WriteShort(offset, _physicalStateObj); // DB filename offset
                WriteShort(rec.attachDBFilename.Length, _physicalStateObj);
                offset += rec.attachDBFilename.Length * 2;

                WriteShort(offset, _physicalStateObj); // reset password offset
                WriteShort(encryptedChangePasswordLengthInBytes / 2, _physicalStateObj);

                WriteInt(0, _physicalStateObj);        // reserved for chSSPI

                // write variable length portion
                WriteString(rec.hostName, _physicalStateObj);

                // if we are using SSPI or fed auth ADAL, do not send over username/password, since we will use SSPI instead
                // same behavior as Luxor
                if (!rec.useSSPI && !(_connHandler._federatedAuthenticationInfoRequested || _connHandler._federatedAuthenticationRequested)) {
                    WriteString(userName, _physicalStateObj);

                    // Cache offset in packet for tracing.
                    _physicalStateObj._tracePasswordOffset = _physicalStateObj._outBytesUsed;
                    _physicalStateObj._tracePasswordLength = encryptedPasswordLengthInBytes;

                    if (rec.credential != null) {
                        _physicalStateObj.WriteSecureString(rec.credential.Password);
                    }
                    else {
                        _physicalStateObj.WriteByteArray(encryptedPassword, encryptedPasswordLengthInBytes, 0);
                    }
                }

                WriteString(rec.applicationName, _physicalStateObj);
                WriteString(rec.serverName, _physicalStateObj);

                // write ibFeatureExtLong
                if (useFeatureExt) {
                    WriteInt(feOffset, _physicalStateObj);
                }

                WriteString(clientInterfaceName, _physicalStateObj);
                WriteString(rec.language, _physicalStateObj);
                WriteString(rec.database, _physicalStateObj);

                // send over SSPI data if we are using SSPI
                if (rec.useSSPI)
                    _physicalStateObj.WriteByteArray(outSSPIBuff, (int)outSSPILength, 0);

                WriteString(rec.attachDBFilename, _physicalStateObj);
                if (!rec.useSSPI && !(_connHandler._federatedAuthenticationInfoRequested || _connHandler._federatedAuthenticationRequested)) {
                    // Cache offset in packet for tracing.
                    _physicalStateObj._traceChangePasswordOffset = _physicalStateObj._outBytesUsed;
                    _physicalStateObj._traceChangePasswordLength = encryptedChangePasswordLengthInBytes;
                    if (rec.newSecurePassword != null) {
                        _physicalStateObj.WriteSecureString(rec.newSecurePassword);
                    }
                    else {
                        _physicalStateObj.WriteByteArray(encryptedChangePassword, encryptedChangePasswordLengthInBytes, 0);
                    }
                }

                if (useFeatureExt) {
                    if ((requestedFeatures & TdsEnums.FeatureExtension.SessionRecovery) != 0) {
                        WriteSessionRecoveryFeatureRequest(recoverySessionData, true);
                    };
                    if ((requestedFeatures & TdsEnums.FeatureExtension.FedAuth) != 0) {
                        Bid.Trace("<sc.TdsParser.TdsLogin|SEC> Sending federated authentication feature request\n");
                        Debug.Assert(fedAuthFeatureExtensionData != null, "fedAuthFeatureExtensionData should not null.");
                        WriteFedAuthFeatureRequest(fedAuthFeatureExtensionData.Value, write: true);
                    };
                    if ((requestedFeatures & TdsEnums.FeatureExtension.Tce) != 0) {
                        WriteTceFeatureRequest (true);
                    };
                    if ((requestedFeatures & TdsEnums.FeatureExtension.GlobalTransactions) != 0) {
                        WriteGlobalTransactionsFeatureRequest(true);
                    };
                    _physicalStateObj.WriteByte(0xFF); // terminator
                }
            } // try
            catch (Exception e) {
                // 
                if (ADP.IsCatchableExceptionType(e)) {
                    // be sure to wipe out our buffer if we started sending stuff
                    _physicalStateObj._outputPacketNumber = 1;  // end of message - reset to 1 - per ramas
                    _physicalStateObj.ResetBuffer();
                }

                throw;
            }

            _physicalStateObj.WritePacket(TdsEnums.HARDFLUSH);
            _physicalStateObj.ResetSecurePasswordsInfomation();     // Password information is needed only from Login process; done with writing login packet and should clear information
            _physicalStateObj._pendingData = true;
            _physicalStateObj._messageStatus = 0;
        }// tdsLogin

        /// <summary>
        /// Send the access token to the server.
        /// </summary>
        /// <param name="fedAuthToken">Type encapuslating a Federated Authentication access token.</param>
        internal void SendFedAuthToken(SqlFedAuthToken fedAuthToken) {
            Debug.Assert(fedAuthToken != null, "fedAuthToken cannot be null");
            Debug.Assert(fedAuthToken.accessToken != null, "fedAuthToken.accessToken cannot be null");


            Bid.Trace("<sc.TdsParser.SendFedAuthToken|SEC> Sending federated authentication token\n");

            _physicalStateObj._outputMessageType = TdsEnums.MT_FEDAUTH;

            byte[] accessToken = fedAuthToken.accessToken;

            // Send total length (length of token plus 4 bytes for the token length field)
            // If we were sending a nonce, this would include that length as well
            WriteUnsignedInt((uint)accessToken.Length + sizeof(uint), _physicalStateObj);

            // Send length of token
            WriteUnsignedInt((uint)accessToken.Length, _physicalStateObj);

            // Send federated authentication access token
            _physicalStateObj.WriteByteArray(accessToken, accessToken.Length, 0);

            _physicalStateObj.WritePacket(TdsEnums.HARDFLUSH);
            _physicalStateObj._pendingData = true;
            _physicalStateObj._messageStatus = 0;

            _connHandler._federatedAuthenticationRequested = true;
        }

        private void SSPIData(byte[] receivedBuff, UInt32 receivedLength, byte[] sendBuff, ref UInt32 sendLength) {
            SNISSPIData(receivedBuff, receivedLength, sendBuff, ref sendLength);
        }

        private void SNISSPIData(byte[] receivedBuff, UInt32 receivedLength, byte[] sendBuff, ref UInt32 sendLength)
        {
            if (receivedBuff == null)
            {
                // we do not have SSPI data coming from server, so send over 0's for pointer and length
                receivedLength = 0;
            }
            // we need to respond to the server's message with SSPI data
            if(0 != SNINativeMethodWrapper.SNISecGenClientContext(_physicalStateObj.Handle, receivedBuff, receivedLength, sendBuff, ref sendLength, _sniSpnBuffer))
            {
                SSPIError(SQLMessage.SSPIGenerateError(), TdsEnums.GEN_CLIENT_CONTEXT);
            }
        }


        private void ProcessSSPI(int receivedLength) {
            SniContext outerContext=_physicalStateObj.SniContext;
            _physicalStateObj.SniContext = SniContext.Snix_ProcessSspi;
            // allocate received buffer based on length from SSPI message
            byte[] receivedBuff = new byte[receivedLength];

            // read SSPI data received from server
            Debug.Assert(_physicalStateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
            bool result = _physicalStateObj.TryReadByteArray(receivedBuff, 0, receivedLength);
            if (!result) { throw SQL.SynchronousCallMayNotPend(); }

            // allocate send buffer and initialize length
            byte[] sendBuff = new byte[s_maxSSPILength];
            UInt32 sendLength = s_maxSSPILength;

            // make call for SSPI data
            SSPIData(receivedBuff, (UInt32)receivedLength, sendBuff, ref sendLength);

            // DO NOT SEND LENGTH - TDS DOC INCORRECT!  JUST SEND SSPI DATA!
            _physicalStateObj.WriteByteArray(sendBuff, (int)sendLength, 0);

            // set message type so server knows its a SSPI response
            _physicalStateObj._outputMessageType = TdsEnums.MT_SSPI;

            // send to server
            _physicalStateObj.WritePacket(TdsEnums.HARDFLUSH);
            _physicalStateObj.SniContext=outerContext;
        }

        private void SSPIError(string error, string procedure) {
            Debug.Assert(!ADP.IsEmpty(procedure), "TdsParser.SSPIError called with an empty or null procedure string");
            Debug.Assert(!ADP.IsEmpty(error), "TdsParser.SSPIError called with an empty or null error string");

            _physicalStateObj.AddError(new SqlError(0, (byte)0x00, (byte)TdsEnums.MIN_ERROR_CLASS, _server, error, procedure, 0));
            ThrowExceptionAndWarning(_physicalStateObj);
        }

        private void LoadSSPILibrary() {
            // Outer check so we don't acquire lock once once it's loaded.
            if (!s_fSSPILoaded) {
                lock (s_tdsParserLock) {
                    // re-check inside lock
                    if (!s_fSSPILoaded) {
                        // use local for ref param to defer setting s_maxSSPILength until we know the call succeeded.
                        UInt32 maxLength = 0;
                        if (0 != SNINativeMethodWrapper.SNISecInitPackage(ref maxLength))
                            SSPIError(SQLMessage.SSPIInitializeError(), TdsEnums.INIT_SSPI_PACKAGE);

                        s_maxSSPILength = maxLength;
                        s_fSSPILoaded = true;

                    }
                }
            }

            if (s_maxSSPILength > Int32.MaxValue) {
                throw SQL.InvalidSSPIPacketSize();   // SqlBu 332503
            }
        }

        private void LoadADALLibrary() {
            // Outer check so we don't acquire lock once once it's loaded.
            if (!s_fADALLoaded) {
                lock (s_tdsParserLock) {
                    // re-check inside lock
                    if (!s_fADALLoaded) {
                        int result = ADALNativeWrapper.ADALInitialize();

                        if (0 == result) {
                            s_fADALLoaded = true;
                        }
                        else {
                            s_fADALLoaded = false;

                            SqlAuthenticationMethod authentication = SqlAuthenticationMethod.NotSpecified;

                            if (_connHandler.ConnectionOptions != null)
                            {
                                authentication = _connHandler.ConnectionOptions.Authentication;

                                // Only the below connection string options should have ended up calling this function.
                                Debug.Assert(authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated || authentication == SqlAuthenticationMethod.ActiveDirectoryPassword);
                            }

                            _physicalStateObj.AddError(new SqlError(0, (byte)0x00, (byte)TdsEnums.MIN_ERROR_CLASS, _server, Res.GetString(Res.SQL_ADALInitializeError, authentication.ToString("G"), result.ToString("X")), TdsEnums.INIT_ADAL_PACKAGE, 0));

                            ThrowExceptionAndWarning(_physicalStateObj);
                        }
                    }
                }
            }
        }

        internal byte[] GetDTCAddress(int timeout, TdsParserStateObject stateObj) {
            // If this fails, the server will return a server error - Sameet Agarwal confirmed.
            // Success: DTCAddress returned.  Failure: SqlError returned.

            byte[] dtcAddr = null;

            using (SqlDataReader dtcReader = TdsExecuteTransactionManagerRequest(
                                                        null,
                                                        TdsEnums.TransactionManagerRequestType.GetDTCAddress,
                                                        null,
                                                        TdsEnums.TransactionManagerIsolationLevel.Unspecified,
                                                        timeout, null, stateObj, true)) {

                Debug.Assert(SniContext.Snix_Read==stateObj.SniContext, String.Format((IFormatProvider)null, "The SniContext should be Snix_Read but it actually is {0}", stateObj.SniContext));
                if (null != dtcReader && dtcReader.Read()) {
                    Debug.Assert(dtcReader.GetName(0) == "TM Address", "TdsParser: GetDTCAddress did not return 'TM Address'");

                    // DTCAddress is of variable size, and does not have a maximum.  So we call GetBytes
                    // to get the length of the dtcAddress, then allocate a byte array of that length,
                    // then call GetBytes again on that byte[] with the length
                    long dtcLength = dtcReader.GetBytes(0, 0, null, 0, 0);

                    //
                    if (dtcLength <= Int32.MaxValue) {
                        int cb = (int)dtcLength;

                        dtcAddr = new byte[cb];
                        dtcReader.GetBytes(0, 0, dtcAddr, 0, cb);
                    }
#if DEBUG
                    else {
                        Debug.Assert(false, "unexpected length (> Int32.MaxValue) returned from dtcReader.GetBytes");
                        // if we hit this case we'll just return a null address so that the user
                        // will get a transcaction enlistment error in the upper layers
                    }
#endif
                }
            }
            return dtcAddr;
        }

        // Propagate the dtc cookie to the server, enlisting the connection.
        internal void PropagateDistributedTransaction(byte[] buffer, int timeout, TdsParserStateObject stateObj) {
            // if this fails, the server will return a server error - Sameet Agarwal confirmed
            // Success: server will return done token.  Failure: SqlError returned.
            
            TdsExecuteTransactionManagerRequest(buffer,
                TdsEnums.TransactionManagerRequestType.Propagate, null,
                TdsEnums.TransactionManagerIsolationLevel.Unspecified, timeout, null, stateObj, true);
        }

        internal SqlDataReader TdsExecuteTransactionManagerRequest(
                    byte[] buffer,
                    TdsEnums.TransactionManagerRequestType request,
                    string transactionName,
                    TdsEnums.TransactionManagerIsolationLevel isoLevel,
                    int timeout,
                    SqlInternalTransaction transaction,
                    TdsParserStateObject stateObj,
                    bool isDelegateControlRequest) {

            Debug.Assert(this == stateObj.Parser, "different parsers");

            if (TdsParserState.Broken == State || TdsParserState.Closed == State) {
                return null;
            }

            // SQLBUDT #20010853 - Promote, Commit and Rollback requests for
            // delegated transactions often happen while there is an open result
            // set, so we need to handle them by using a different MARS session, 
            // otherwise we'll write on the physical state objects while someone
            // else is using it.  When we don't have MARS enabled, we need to 
            // lock the physical state object to syncronize it's use at least 
            // until we increment the open results count.  Once it's been 
            // incremented the delegated transaction requests will fail, so they
            // won't stomp on anything.


            Debug.Assert(!_connHandler.ThreadHasParserLockForClose || _connHandler._parserLock.ThreadMayHaveLock(), "Thread claims to have parser lock, but lock is not taken");
            bool callerHasConnectionLock = _connHandler.ThreadHasParserLockForClose;   // If the thread already claims to have the parser lock, then we will let the caller handle releasing it
            if (!callerHasConnectionLock) {
                _connHandler._parserLock.Wait(canReleaseFromAnyThread:false);
                _connHandler.ThreadHasParserLockForClose = true;
            }
            // Capture _asyncWrite (after taking lock) to restore it afterwards
            bool hadAsyncWrites = _asyncWrite;
            try {
                // Temprarily disable async writes
                _asyncWrite = false;

                // This validation step MUST be done after locking the connection to guarantee we don't 
                //  accidentally execute after the transaction has completed on a different thread.
                if (!isDelegateControlRequest) {
                    _connHandler.CheckEnlistedTransactionBinding();
                }

                stateObj._outputMessageType = TdsEnums.MT_TRANS;       // set message type
                stateObj.SetTimeoutSeconds(timeout);

                stateObj.SniContext = SniContext.Snix_Execute;

                if (_isYukon) {
                    const int marsHeaderSize = 18; // 4 + 2 + 8 + 4
                    const int totalHeaderLength = 22; // 4 + 4 + 2 + 8 + 4
                    Debug.Assert(stateObj._outBytesUsed == stateObj._outputHeaderLen, "Output bytes written before total header length");
                    // Write total header length
                    WriteInt(totalHeaderLength, stateObj);
                    // Write mars header length
                    WriteInt(marsHeaderSize, stateObj);
                    WriteMarsHeaderData(stateObj, _currentTransaction);
                }

                WriteShort((short)request, stateObj); // write TransactionManager Request type

                bool returnReader = false;

                switch (request) {
                    case TdsEnums.TransactionManagerRequestType.GetDTCAddress:
                        WriteShort(0, stateObj);

                        returnReader = true;
                        break;
                    case TdsEnums.TransactionManagerRequestType.Propagate:
                        if (null != buffer) {
                            WriteShort(buffer.Length, stateObj);
                            stateObj.WriteByteArray(buffer, buffer.Length, 0);
                        }
                        else {
                            WriteShort(0, stateObj);
                        }
                        break;
                    case TdsEnums.TransactionManagerRequestType.Begin:
                        Debug.Assert(IsYukonOrNewer, "Should not be calling TdsExecuteTransactionManagerRequest on pre-Yukon clients for BeginTransaction!");
                        Debug.Assert(null != transaction, "Should have specified an internalTransaction when doing a BeginTransaction request!");

                        // Only assign the passed in transaction if it is not equal to the current transaction.
                        // And, if it is not equal, the current actually should be null.  Anything else
                        // is a unexpected state.  The concern here is mainly for the mixed use of 
                        // T-SQL and API transactions.  See SQL BU DT 345300 for full details and repro.

                        // Expected states:
                        // 1) _pendingTransaction = null, _currentTransaction = null, non null transaction
                        // passed in on BeginTransaction API call.
                        // 2) _currentTransaction != null, _pendingTransaction = null, non null transaction
                        // passed in but equivalent to _currentTransaction.

                        // #1 will occur on standard BeginTransactionAPI call.  #2 should only occur if
                        // t-sql transaction started followed by a call to SqlConnection.BeginTransaction.
                        // Any other state is unknown.
                        if (_currentTransaction != transaction) {
                            Debug.Assert(_currentTransaction == null || true == _fResetConnection, "We should not have a current Tx at this point");
                            PendingTransaction = transaction;
                        }

                        stateObj.WriteByte((byte)isoLevel);

                        stateObj.WriteByte((byte)(transactionName.Length * 2)); // Write number of bytes (unicode string).
                        WriteString(transactionName, stateObj);
                        break;
                    case TdsEnums.TransactionManagerRequestType.Promote:
                        Debug.Assert(IsYukonOrNewer, "Should not be calling TdsExecuteTransactionManagerRequest on pre-Yukon clients for PromoteTransaction!");
                        // No payload - except current transaction in header
                        // Promote returns a DTC cookie.  However, the transaction cookie we use for the
                        // connection does not change after a promote.
                        break;
                    case TdsEnums.TransactionManagerRequestType.Commit:
                        Debug.Assert(IsYukonOrNewer, "Should not be calling TdsExecuteTransactionManagerRequest on pre-Yukon clients for CommitTransaction!");

                        Debug.Assert(transactionName.Length == 0, "Should not have a transaction name on Commit");
                        stateObj.WriteByte((byte)0); // No xact name

                        stateObj.WriteByte(0);  // No flags

                        Debug.Assert(isoLevel == TdsEnums.TransactionManagerIsolationLevel.Unspecified, "Should not have isolation level other than unspecified on Commit!");
                        // WriteByte((byte) 0, stateObj); // IsolationLevel
                        // WriteByte((byte) 0, stateObj); // No begin xact name
                        break;
                    case TdsEnums.TransactionManagerRequestType.Rollback:
                        Debug.Assert(IsYukonOrNewer, "Should not be calling TdsExecuteTransactionManagerRequest on pre-Yukon clients for RollbackTransaction!");

                        stateObj.WriteByte((byte)(transactionName.Length * 2)); // Write number of bytes (unicode string).
                        WriteString(transactionName, stateObj);

                        stateObj.WriteByte(0);  // No flags

                        Debug.Assert(isoLevel == TdsEnums.TransactionManagerIsolationLevel.Unspecified, "Should not have isolation level other than unspecified on Commit!");
                        // WriteByte((byte) 0, stateObj); // IsolationLevel
                        // WriteByte((byte) 0, stateObj); // No begin xact name
                        break;
                    case TdsEnums.TransactionManagerRequestType.Save:
                        Debug.Assert(IsYukonOrNewer, "Should not be calling TdsExecuteTransactionManagerRequest on pre-Yukon clients for SaveTransaction!");

                        stateObj.WriteByte((byte)(transactionName.Length * 2)); // Write number of bytes (unicode string).
                        WriteString(transactionName, stateObj);
                        break;
                    default:
                        Debug.Assert(false, "Unexpected TransactionManagerRequest");
                        break;
                }

                Task writeTask = stateObj.WritePacket(TdsEnums.HARDFLUSH);
                Debug.Assert(writeTask == null, "Writes should not pend when writing sync");
                stateObj._pendingData = true;
                stateObj._messageStatus = 0;

                SqlDataReader dtcReader = null;
                stateObj.SniContext = SniContext.Snix_Read;
                if (returnReader) {
                    dtcReader = new SqlDataReader(null, CommandBehavior.Default);
                    Debug.Assert(this == stateObj.Parser, "different parser");
#if DEBUG
                    // Remove the current owner of stateObj - otherwise we will hit asserts
                    stateObj.Owner = null;
#endif
                    dtcReader.Bind(stateObj);

                    // force consumption of metadata
                    _SqlMetaDataSet metaData = dtcReader.MetaData;
                }
                else {
                    Run(RunBehavior.UntilDone, null, null, null, stateObj);
                }

                // If the retained ID is no longer valid (because we are enlisting in null or a new transaction) then it should be cleared
                if (((request == TdsEnums.TransactionManagerRequestType.Begin) || (request == TdsEnums.TransactionManagerRequestType.Propagate)) && ((transaction == null) || (transaction.TransactionId != _retainedTransactionId))) {
                    _retainedTransactionId = SqlInternalTransaction.NullTransactionId;
                }

                return dtcReader;
            }
            catch (Exception e) {
                // 
                if (!ADP.IsCatchableExceptionType(e)) {
                    throw;
                }

                FailureCleanup(stateObj, e);

                throw;
            }
            finally {
                // SQLHotfix 50000518
                // make sure we don't leave temporary fields set when leaving this function
                _pendingTransaction = null;

                _asyncWrite = hadAsyncWrites;

                if (!callerHasConnectionLock) {
                    _connHandler.ThreadHasParserLockForClose = false;
                    _connHandler._parserLock.Release();
                }
            }
        }

        internal void FailureCleanup(TdsParserStateObject stateObj, Exception e) {
            int old_outputPacketNumber = stateObj._outputPacketNumber;

            if (Bid.TraceOn) {
                Bid.Trace("<sc.TdsParser.FailureCleanup|ERR> Exception caught on ExecuteXXX: '%ls' \n", e.ToString());
            }

            if (stateObj.HasOpenResult) { // SQL BU DT 383773 - need to decrement openResultCount if operation failed.
                stateObj.DecrementOpenResultCount();                
            }

            // be sure to wipe out our buffer if we started sending stuff
            stateObj.ResetBuffer();
            stateObj._outputPacketNumber = 1;  // end of message - reset to 1 - per ramas

            if (old_outputPacketNumber != 1 && _state == TdsParserState.OpenLoggedIn) {
                Debug.Assert(_connHandler._parserLock.ThreadMayHaveLock(), "Should not be calling into FailureCleanup without first taking the parser lock");

                bool originalThreadHasParserLock = _connHandler.ThreadHasParserLockForClose;
                try {
                    // Dev11 Bug 385286 : ExecuteNonQueryAsync hangs when trying to write a parameter which generates ArgumentException and while handling that exception the server disconnects the connection
                    // Need to set this to true such that if we have an error sending\processing the attention, we won't deadlock ourselves
                    _connHandler.ThreadHasParserLockForClose = true;

                    // If _outputPacketNumber prior to ResetBuffer was not equal to 1, a packet was already
                    // sent to the server and so we need to send an attention and process the attention ack.
                    stateObj.SendAttention();
                    ProcessAttention(stateObj);
                }
                finally {
                    // Reset the ThreadHasParserLock value incase our caller expects it to be set\not set
                    _connHandler.ThreadHasParserLockForClose = originalThreadHasParserLock;
                }
            }

            Bid.Trace("<sc.TdsParser.FailureCleanup|ERR> Exception rethrown. \n");
        }

        internal Task TdsExecuteSQLBatch(string text, int timeout, SqlNotificationRequest notificationRequest, TdsParserStateObject stateObj, bool sync, bool callerHasConnectionLock = false) {
            if (TdsParserState.Broken == State || TdsParserState.Closed == State) {
                return null;
            }

            if (stateObj.BcpLock) {
                throw SQL.ConnectionLockedForBcpEvent();
            }

            // SQLBUDT #20010853 - Promote, Commit and Rollback requests for
            // delegated transactions often happen while there is an open result
            // set, so we need to handle them by using a different MARS session, 
            // otherwise we'll write on the physical state objects while someone
            // else is using it.  When we don't have MARS enabled, we need to 
            // lock the physical state object to syncronize it's use at least 
            // until we increment the open results count.  Once it's been 
            // incremented the delegated transaction requests will fail, so they
            // won't stomp on anything.

            // Only need to take the lock if neither the thread nor the caller claims to already have it
            bool needToTakeParserLock = (!callerHasConnectionLock) && (!_connHandler.ThreadHasParserLockForClose);
            Debug.Assert(!_connHandler.ThreadHasParserLockForClose || sync, "Thread shouldn't claim to have the parser lock if we are doing async writes");     // Since we have the possibility of pending with async writes, make sure the thread doesn't claim to already have the lock
            Debug.Assert(needToTakeParserLock || _connHandler._parserLock.ThreadMayHaveLock(), "Thread or caller claims to have connection lock, but lock is not taken");

            bool releaseConnectionLock = false;
            if (needToTakeParserLock) {
                _connHandler._parserLock.Wait(canReleaseFromAnyThread: !sync);
                releaseConnectionLock = true;
            }

            // Switch the writing mode
            // NOTE: We are not turning off async writes when we complete since SqlBulkCopy uses this method and expects _asyncWrite to not change
            _asyncWrite = !sync;

            try {
                // Check that the connection is still alive
                if ((_state == TdsParserState.Closed) || (_state == TdsParserState.Broken)) {
                    throw ADP.ClosedConnectionError();
                }

                // This validation step MUST be done after locking the connection to guarantee we don't 
                //  accidentally execute after the transaction has completed on a different thread.
                _connHandler.CheckEnlistedTransactionBinding();

                stateObj.SetTimeoutSeconds(timeout);
                if ((!_fMARS) && (_physicalStateObj.HasOpenResult)) 
                {
                    Bid.Trace("<sc.TdsParser.TdsExecuteSQLBatch|ERR> Potential multi-threaded misuse of connection, non-MARs connection with an open result %d#\n", ObjectID);
                }
                stateObj.SniContext = SniContext.Snix_Execute;

                if (_isYukon) {

                    WriteRPCBatchHeaders(stateObj, notificationRequest);
                }

                stateObj._outputMessageType = TdsEnums.MT_SQL;

                WriteString(text, text.Length, 0, stateObj);

                Task executeTask = stateObj.ExecuteFlush();
                if (executeTask == null) {
                    stateObj.SniContext = SniContext.Snix_Read;
                }
                else {
                    Debug.Assert(!sync, "Should not have gotten a Task when writing in sync mode");

                    // Need to wait for flush - continuation will unlock the connection                    
                    bool taskReleaseConnectionLock = releaseConnectionLock;
                    releaseConnectionLock = false;
                    return executeTask.ContinueWith(t => {
                        Debug.Assert(!t.IsCanceled, "Task should not be canceled");
                        try {
                            if (t.IsFaulted) {
                                FailureCleanup(stateObj, t.Exception.InnerException);
                                throw t.Exception.InnerException;
                            }
                            else {
                                stateObj.SniContext = SniContext.Snix_Read;
                            }
                        }
                        finally {
                            if (taskReleaseConnectionLock) {
                                _connHandler._parserLock.Release();
                            }
                        }
                    }, TaskScheduler.Default);
                }

                // Finished sync
                return null;
            }
            catch (Exception e) {
                //Debug.Assert(_state == TdsParserState.Broken, "Caught exception in TdsExecuteSQLBatch but connection was not broken!");
                // 
                if (!ADP.IsCatchableExceptionType(e)) {
                    throw;
                }

                FailureCleanup(stateObj, e);

                throw;
            }
            finally {
                if (releaseConnectionLock) {
                    _connHandler._parserLock.Release();
                }
            }
        }      

      internal Task TdsExecuteRPC(SqlCommand cmd, _SqlRPC[] rpcArray, int timeout, bool inSchema, SqlNotificationRequest notificationRequest, TdsParserStateObject stateObj, bool isCommandProc, bool sync = true, 
          TaskCompletionSource<object> completion = null, int startRpc = 0, int startParam = 0) {
          bool firstCall = (completion == null);
          bool releaseConnectionLock = false;

          Debug.Assert(cmd != null, @"cmd cannot be null inside TdsExecuteRPC");
          Debug.Assert(!firstCall || startRpc == 0, "startRpc is not 0 on first call");
          Debug.Assert(!firstCall || startParam == 0, "startParam is not 0 on first call");
          Debug.Assert(!firstCall || !_connHandler.ThreadHasParserLockForClose, "Thread should not already have connection lock");
          Debug.Assert(firstCall || _connHandler._parserLock.ThreadMayHaveLock(), "Connection lock not taken after the first call");
          try {
              _SqlRPC rpcext = null;
              int tempLen;

              // SQLBUDT #20010853 - Promote, Commit and Rollback requests for
              // delegated transactions often happen while there is an open result
              // set, so we need to handle them by using a different MARS session, 
              // otherwise we'll write on the physical state objects while someone
              // else is using it.  When we don't have MARS enabled, we need to 
              // lock the physical state object to syncronize it's use at least 
              // until we increment the open results count.  Once it's been 
              // incremented the delegated transaction requests will fail, so they
              // won't stomp on anything.


              if (firstCall) {
                  _connHandler._parserLock.Wait(canReleaseFromAnyThread:!sync);
                  releaseConnectionLock = true;
              }
              try {
                  // Ensure that connection is alive
                  if ((TdsParserState.Broken == State) || (TdsParserState.Closed == State)) {
                      throw ADP.ClosedConnectionError();
                  }

                  // This validation step MUST be done after locking the connection to guarantee we don't 
                  //  accidentally execute after the transaction has completed on a different thread.
                  if (firstCall) {
                      _asyncWrite = !sync;

                      _connHandler.CheckEnlistedTransactionBinding();

                      stateObj.SetTimeoutSeconds(timeout);
                      if ((!_fMARS) && (_physicalStateObj.HasOpenResult)) 
                      {
                          Bid.Trace("<sc.TdsParser.TdsExecuteRPC|ERR> Potential multi-threaded misuse of connection, non-MARs connection with an open result %d#\n", ObjectID);
                      }
                      stateObj.SniContext = SniContext.Snix_Execute;

                      if (_isYukon) {

                          WriteRPCBatchHeaders(stateObj, notificationRequest);
                      }

                      stateObj._outputMessageType = TdsEnums.MT_RPC;
                  }

                  for (int ii = startRpc; ii < rpcArray.Length; ii++) {
                      rpcext = rpcArray[ii];

                      if (startParam == 0 || ii > startRpc) {
                          if (rpcext.ProcID != 0 && _isShiloh) {
                              // Perf optimization for Shiloh and later,
                              Debug.Assert(rpcext.ProcID < 255, "rpcExec:ProcID can't be larger than 255");
                              WriteShort(0xffff, stateObj);
                              WriteShort((short)(rpcext.ProcID), stateObj);
                          }
                          else {
                              Debug.Assert(!ADP.IsEmpty(rpcext.rpcName), "must have an RPC name");
                              tempLen = rpcext.rpcName.Length;
                              WriteShort(tempLen, stateObj);
                              WriteString(rpcext.rpcName, tempLen, 0, stateObj);
                          }

                          // Options
                          WriteShort((short)rpcext.options, stateObj);
                      }

                      // Stream out parameters
                      SqlParameter[] parameters = rpcext.parameters;

                      for (int i = (ii == startRpc) ? startParam : 0; i < parameters.Length; i++) {
                          //                Debug.WriteLine("i:  " + i.ToString(CultureInfo.InvariantCulture));
                          // parameters can be unnamed
                          SqlParameter param = parameters[i];
                          // Since we are reusing the parameters array, we cannot rely on length to indicate no of parameters.
                          if (param == null)
                              break;      // End of parameters for this execute

                          // Throw an exception if ForceColumnEncryption is set on a parameter and the ColumnEncryption is not enabled on SqlConnection or SqlCommand
                          if (param.ForceColumnEncryption && 
                              !(cmd.ColumnEncryptionSetting == SqlCommandColumnEncryptionSetting.Enabled || 
                              (cmd.ColumnEncryptionSetting == SqlCommandColumnEncryptionSetting.UseConnectionSetting && cmd.Connection.IsColumnEncryptionSettingEnabled))) {
                              throw SQL.ParamInvalidForceColumnEncryptionSetting(param.ParameterName, rpcext.GetCommandTextOrRpcName());
                          }

                          // Check if the applications wants to force column encryption to avoid sending sensitive data to server
                          if (param.ForceColumnEncryption && param.CipherMetadata == null
                              && (param.Direction == ParameterDirection.Input || param.Direction == ParameterDirection.InputOutput)) {
                              // Application wants a parameter to be encrypted before sending it to server, however server doesnt think this parameter needs encryption.
                              throw SQL.ParamUnExpectedEncryptionMetadata(param.ParameterName, rpcext.GetCommandTextOrRpcName());
                          }

                          // Validate parameters are not variable length without size and with null value.  MDAC 66522
                          param.Validate(i, isCommandProc);

                          // type (parameter record stores the MetaType class which is a helper that encapsulates all the type information we need here)
                          MetaType mt = param.InternalMetaType;

                          if (mt.IsNewKatmaiType) {
                              WriteSmiParameter(param, i, 0 != (rpcext.paramoptions[i] & TdsEnums.RPC_PARAM_DEFAULT), stateObj);
                              continue;
                          }

                          if ((!_isShiloh && !mt.Is70Supported) ||
                              (!_isYukon && !mt.Is80Supported) ||
                              (!_isKatmai && !mt.Is90Supported)) {
                              throw ADP.VersionDoesNotSupportDataType(mt.TypeName);
                          }
                          object value = null;
                          bool isNull = true;
                          bool isSqlVal = false;
                          bool isDataFeed = false;
                          // if we have an output param, set the value to null so we do not send it across to the server
                          if (param.Direction == ParameterDirection.Output) {
                              isSqlVal = param.ParamaterIsSqlType;  // We have to forward the TYPE info, we need to know what type we are returning.  Once we null the paramater we will no longer be able to distinguish what type were seeing.
                              param.Value = null;
                              param.ParamaterIsSqlType = isSqlVal;
                          }
                          else {
                              value = param.GetCoercedValue();
                              isNull = param.IsNull;
                              if (!isNull) {
                                  isSqlVal = param.CoercedValueIsSqlType;
                                  isDataFeed = param.CoercedValueIsDataFeed;
                              }
                          }

                          WriteParameterName(param.ParameterNameFixed, stateObj);

                          // Write parameter status
                          stateObj.WriteByte(rpcext.paramoptions[i]);

                          // MaxLen field is only written out for non-fixed length data types
                          // use the greater of the two sizes for maxLen
                          int actualSize;
                          int size = mt.IsSizeInCharacters ? param.GetParameterSize() * 2 : param.GetParameterSize();

                          //for UDTs, we calculate the length later when we get the bytes. This is a really expensive operation
                          if (mt.TDSType != TdsEnums.SQLUDT)
                              // getting the actualSize is expensive, cache here and use below
                              actualSize = param.GetActualSize();
                          else
                              actualSize = 0; //get this later

                          byte precision = 0;
                          byte scale = 0;

                          // scale and precision are only relevant for numeric and decimal types
                          // adjust the actual value scale and precision to match the user specified
                          if (mt.SqlDbType == SqlDbType.Decimal) {
                              precision = param.GetActualPrecision();
                              scale = param.GetActualScale();

                              if (precision > TdsEnums.MAX_NUMERIC_PRECISION) {
                                  throw SQL.PrecisionValueOutOfRange(precision);
                              }

                              // bug 49512, make sure the value matches the scale the user enters
                              if (!isNull) {
                                  if (isSqlVal) {
                                      value = AdjustSqlDecimalScale((SqlDecimal)value, scale);

                                      // If Precision is specified, verify value precision vs param precision
                                      if (precision != 0) {
                                          if (precision < ((SqlDecimal)value).Precision) {
                                              throw ADP.ParameterValueOutOfRange((SqlDecimal)value);
                                          }
                                      }
                                  }
                                  else {
                                      value = AdjustDecimalScale((Decimal)value, scale);

                                      SqlDecimal sqlValue = new SqlDecimal((Decimal)value);

                                      // If Precision is specified, verify value precision vs param precision
                                      if (precision != 0) {
                                          if (precision < sqlValue.Precision) {
                                              throw ADP.ParameterValueOutOfRange((Decimal)value);
                                          }
                                      }
                                  }
                              }
                          }

                          bool isParameterEncrypted = 0 != (rpcext.paramoptions[i] & TdsEnums.RPC_PARAM_ENCRYPTED);

                          // Additional information we need to send over wire to the server when writing encrypted parameters.
                          SqlColumnEncryptionInputParameterInfo encryptedParameterInfoToWrite = null;

                          // If the parameter is encrypted, we need to encrypt the value.
                          if (isParameterEncrypted) {
                              Debug.Assert(mt.TDSType != TdsEnums.SQLVARIANT &&
                                  mt.TDSType != TdsEnums.SQLUDT &&
                                  mt.TDSType != TdsEnums.SQLXMLTYPE &&
                                  mt.TDSType != TdsEnums.SQLIMAGE &&
                                  mt.TDSType != TdsEnums.SQLTEXT &&
                                  mt.TDSType != TdsEnums.SQLNTEXT, "Type unsupported for encryption");

                              byte[] serializedValue = null;
                              byte[] encryptedValue = null;

                              if (!isNull) {
                                  try {
                                    if (isSqlVal) {
                                        serializedValue = SerializeUnencryptedSqlValue(value, mt, actualSize, param.Offset, param.NormalizationRuleVersion, stateObj);
                                    }
                                    else {
                                        // for codePageEncoded types, WriteValue simply expects the number of characters
                                        // For plp types, we also need the encoded byte size
                                        serializedValue = SerializeUnencryptedValue(value, mt, param.GetActualScale(), actualSize, param.Offset, isDataFeed, param.NormalizationRuleVersion, stateObj);
                                    }

                                    Debug.Assert(serializedValue != null, "serializedValue should not be null in TdsExecuteRPC.");
                                    encryptedValue = SqlSecurityUtility.EncryptWithKey(serializedValue, param.CipherMetadata, _connHandler.ConnectionOptions.DataSource);
                                }
                                catch (Exception e) {
                                    throw SQL.ParamEncryptionFailed(param.ParameterName, null, e);
                                }

                                  Debug.Assert(encryptedValue != null && encryptedValue.Length > 0,
                                      "encryptedValue should not be null or empty in TdsExecuteRPC.");
                              }
                              else {
                                  encryptedValue = null;
                              }

                              // Change the datatype to varbinary(max).
                              // Since we don't know the size of the encrypted parameter on the server side, always set to (max).
                              //
                              mt = MetaType.MetaMaxVarBinary;
                              size = -1;
                              actualSize = (encryptedValue == null) ? 0 : encryptedValue.Length;

                              encryptedParameterInfoToWrite = new SqlColumnEncryptionInputParameterInfo(param.GetMetadataForTypeInfo(),
                                                                                                        param.CipherMetadata);

                              // Set the value to the encrypted value and mark isSqlVal as false for VARBINARY encrypted value.
                              value = encryptedValue;
                              isSqlVal = false;
                          }

                          Debug.Assert(isParameterEncrypted == (encryptedParameterInfoToWrite != null),
                                            "encryptedParameterInfoToWrite can be not null if and only if isParameterEncrypted is true.");

                          Debug.Assert(!isSqlVal || !isParameterEncrypted, "isParameterEncrypted can be true only if isSqlVal is false.");

                          //
                          // fixup the types by using the NullableType property of the MetaType class
                          //
                          // following rules should be followed based on feedback from the M-SQL team
                          // 1) always use the BIG* types (ex: instead of SQLCHAR use SQLBIGCHAR)
                          // 2) always use nullable types (ex: instead of SQLINT use SQLINTN)
                          // 3) DECIMALN should always be sent as NUMERICN
                          //
                          stateObj.WriteByte(mt.NullableType);

                          // handle variants here: the SQLVariant writing routine will write the maxlen and actual len columns
                          if (mt.TDSType == TdsEnums.SQLVARIANT) {
                              // devnote: Do we ever hit this codepath? Yes, when a null value is being writen out via a sql variant
                              // param.GetActualSize is not used
                              WriteSqlVariantValue(isSqlVal ? MetaType.GetComValueFromSqlVariant(value) : value, param.GetActualSize(), param.Offset, stateObj);
                              continue;
                          }

                          int codePageByteSize = 0;
                          int maxsize = 0;

                          if (mt.IsAnsiType) {
                              // Avoid the following code block if ANSI but unfilled LazyMat blob
                              if ((!isNull) && (!isDataFeed)) {
                                  string s;

                                  if (isSqlVal) {
                                      if (value is SqlString) {
                                          s = ((SqlString)value).Value;
                                      }
                                      else {
                                          Debug.Assert(value is SqlChars, "Unknown value for Ansi datatype");
                                          s = new String(((SqlChars)value).Value);
                                      }
                                  }
                                  else {
                                      s = (string)value;
                                  }

                                  codePageByteSize = GetEncodingCharLength(s, actualSize, param.Offset, _defaultEncoding);
                              }

                              if (mt.IsPlp) {
                                  WriteShort(TdsEnums.SQL_USHORTVARMAXLEN, stateObj);
                              }
                              else {
                                  maxsize = (size > codePageByteSize) ? size : codePageByteSize;
                                  if (maxsize == 0) {
                                      // Yukon doesn't like 0 as MaxSize. Change it to 2 for unicode types (SQL9 - 682322)
                                      if (mt.IsNCharType)
                                          maxsize = 2;
                                      else
                                          maxsize = 1;
                                  }

                                  WriteParameterVarLen(mt, maxsize, false/*IsNull*/, stateObj);
                              }
                          }
                          else {
                              // If type timestamp - treat as fixed type and always send over timestamp length, which is 8.
                              // For fixed types, we either send null or fixed length for type length.  We want to match that
                              // behavior for timestamps.  However, in the case of null, we still must send 8 because if we
                              // send null we will not receive a output val.  You can send null for fixed types and still
                              // receive a output value, but not for variable types.  So, always send 8 for timestamp because
                              // while the user sees it as a fixed type, we are actually representing it as a bigbinary which
                              // is variable.
                              if (mt.SqlDbType == SqlDbType.Timestamp) {
                                  WriteParameterVarLen(mt, TdsEnums.TEXT_TIME_STAMP_LEN, false, stateObj);
                              }
                              else if (mt.SqlDbType == SqlDbType.Udt) {
                                  byte[] udtVal = null;
                                  Microsoft.SqlServer.Server.Format format = Microsoft.SqlServer.Server.Format.Native;

                                  Debug.Assert(_isYukon, "Invalid DataType UDT for non-Yukon or later server!");

                                  if (!isNull) {
                                      udtVal = _connHandler.Connection.GetBytes(value, out format, out maxsize);

                                      Debug.Assert(null != udtVal, "GetBytes returned null instance. Make sure that it always returns non-null value");
                                      size = udtVal.Length;

                                      //it may be legitimate, but we dont support it yet
                                      if (size < 0 || (size >= UInt16.MaxValue && maxsize != -1))
                                          throw new IndexOutOfRangeException();
                                  }

                                  //if this is NULL value, write special null value
                                  byte[] lenBytes = BitConverter.GetBytes((Int64)size);

                                  if (ADP.IsEmpty(param.UdtTypeName))
                                      throw SQL.MustSetUdtTypeNameForUdtParams();

                                  // Split the input name. TypeName is returned as single 3 part name during DeriveParameters.
                                  // NOTE: ParseUdtTypeName throws if format is incorrect
                                  String[] names = SqlParameter.ParseTypeName(param.UdtTypeName, true /* is UdtTypeName */);
                                  if (!ADP.IsEmpty(names[0]) && TdsEnums.MAX_SERVERNAME < names[0].Length) {
                                      throw ADP.ArgumentOutOfRange("names");
                                  }
                                  if (!ADP.IsEmpty(names[1]) && TdsEnums.MAX_SERVERNAME < names[names.Length - 2].Length) {
                                      throw ADP.ArgumentOutOfRange("names");
                                  }
                                  if (TdsEnums.MAX_SERVERNAME < names[2].Length) {
                                      throw ADP.ArgumentOutOfRange("names");
                                  }

                                  WriteUDTMetaData(value, names[0], names[1], names[2], stateObj);

                                  // 
                                  if (!isNull) {
                                      WriteUnsignedLong((ulong)udtVal.Length, stateObj); // PLP length
                                      if (udtVal.Length > 0) { // Only write chunk length if its value is greater than 0
                                          WriteInt(udtVal.Length, stateObj);                  // Chunk length
                                          stateObj.WriteByteArray(udtVal, udtVal.Length, 0); // Value
                                      }
                                      WriteInt(0, stateObj);                              // Terminator
                                  }
                                  else {
                                      WriteUnsignedLong(TdsEnums.SQL_PLP_NULL, stateObj); // PLP Null.
                                  }
                                  continue; // End of UDT - continue to next parameter.
                                  // 
                              }
                              else if (mt.IsPlp) {
                                  if (mt.SqlDbType != SqlDbType.Xml)
                                      WriteShort(TdsEnums.SQL_USHORTVARMAXLEN, stateObj);
                              }
                              else if ((!mt.IsVarTime) && (mt.SqlDbType != SqlDbType.Date)) {   // Time, Date, DateTime2, DateTimeoffset do not have the size written out
                                  maxsize = (size > actualSize) ? size : actualSize;
                                  if (maxsize == 0 && IsYukonOrNewer) {
                                      // Yukon doesn't like 0 as MaxSize. Change it to 2 for unicode types (SQL9 - 682322)
                                      if (mt.IsNCharType)
                                          maxsize = 2;
                                      else
                                          maxsize = 1;
                                  }

                                  WriteParameterVarLen(mt, maxsize, false/*IsNull*/, stateObj);
                              }
                          }

                          // scale and precision are only relevant for numeric and decimal types
                          if (mt.SqlDbType == SqlDbType.Decimal) {
                              if (0 == precision) {
                                  if (_isShiloh)
                                      stateObj.WriteByte(TdsEnums.DEFAULT_NUMERIC_PRECISION);
                                  else
                                      stateObj.WriteByte(TdsEnums.SPHINX_DEFAULT_NUMERIC_PRECISION);
                              }
                              else
                                  stateObj.WriteByte(precision);

                              stateObj.WriteByte(scale);
                          }
                          else if (mt.IsVarTime) {
                              stateObj.WriteByte(param.GetActualScale());
                          }

                          // write out collation or xml metadata

                          if (_isYukon && (mt.SqlDbType == SqlDbType.Xml)) {
                              if (((param.XmlSchemaCollectionDatabase != null) && (param.XmlSchemaCollectionDatabase != ADP.StrEmpty)) ||
                                  ((param.XmlSchemaCollectionOwningSchema != null) && (param.XmlSchemaCollectionOwningSchema != ADP.StrEmpty)) ||
                                  ((param.XmlSchemaCollectionName != null) && (param.XmlSchemaCollectionName != ADP.StrEmpty))) {
                                  stateObj.WriteByte(1);   //Schema present flag

                                  if ((param.XmlSchemaCollectionDatabase != null) && (param.XmlSchemaCollectionDatabase != ADP.StrEmpty)) {
                                      tempLen = (param.XmlSchemaCollectionDatabase).Length;
                                      stateObj.WriteByte((byte)(tempLen));
                                      WriteString(param.XmlSchemaCollectionDatabase, tempLen, 0, stateObj);
                                  }
                                  else {
                                      stateObj.WriteByte(0);       // No dbname
                                  }

                                  if ((param.XmlSchemaCollectionOwningSchema != null) && (param.XmlSchemaCollectionOwningSchema != ADP.StrEmpty)) {
                                      tempLen = (param.XmlSchemaCollectionOwningSchema).Length;
                                      stateObj.WriteByte((byte)(tempLen));
                                      WriteString(param.XmlSchemaCollectionOwningSchema, tempLen, 0, stateObj);
                                  }
                                  else {
                                      stateObj.WriteByte(0);      // no xml schema name
                                  }
                                  if ((param.XmlSchemaCollectionName != null) && (param.XmlSchemaCollectionName != ADP.StrEmpty)) {
                                      tempLen = (param.XmlSchemaCollectionName).Length;
                                      WriteShort((short)(tempLen), stateObj);
                                      WriteString(param.XmlSchemaCollectionName, tempLen, 0, stateObj);
                                  }
                                  else {
                                      WriteShort(0, stateObj);       // No xml schema collection name
                                  }

                              }
                              else {
                                  stateObj.WriteByte(0);       // No schema
                              }
                          }
                          else if (_isShiloh && mt.IsCharType) {
                              // if it is not supplied, simply write out our default collation, otherwise, write out the one attached to the parameter
                              SqlCollation outCollation = (param.Collation != null) ? param.Collation : _defaultCollation;
                              Debug.Assert(_defaultCollation != null, "_defaultCollation is null!");

                              WriteUnsignedInt(outCollation.info, stateObj);
                              stateObj.WriteByte(outCollation.sortId);
                          }

                          if (0 == codePageByteSize)
                              WriteParameterVarLen(mt, actualSize, isNull, stateObj, isDataFeed);
                          else
                              WriteParameterVarLen(mt, codePageByteSize, isNull, stateObj, isDataFeed);

                          Task writeParamTask = null;
                          // write the value now
                          if (!isNull) {
                              if (isSqlVal) {
                                  writeParamTask = WriteSqlValue(value, mt, actualSize, codePageByteSize, param.Offset, stateObj);
                              }
                              else {
                                  // for codePageEncoded types, WriteValue simply expects the number of characters
                                  // For plp types, we also need the encoded byte size
                                  writeParamTask = WriteValue(value, mt, isParameterEncrypted ? (byte)0 : param.GetActualScale(), actualSize, codePageByteSize, isParameterEncrypted ? 0 : param.Offset, stateObj, isParameterEncrypted ? 0 : param.Size, isDataFeed);
                              }
                          }

                          // Send encryption metadata for encrypted parameters.
                          if (isParameterEncrypted) {
                              writeParamTask = WriteEncryptionMetadata(writeParamTask, encryptedParameterInfoToWrite, stateObj);
                          }

                          if (!sync) {
                              if (writeParamTask == null) {
                                  writeParamTask = stateObj.WaitForAccumulatedWrites();                             
                              }

                              if (writeParamTask != null) {
                                  Task task = null;
                                  if (completion == null) {
                                      completion = new TaskCompletionSource<object>();
                                      task = completion.Task;
                                  }

                                  AsyncHelper.ContinueTask(writeParamTask, completion,
                                      () => TdsExecuteRPC(cmd, rpcArray, timeout, inSchema, notificationRequest, stateObj, isCommandProc, sync, completion,
                                                            startRpc: ii, startParam: i + 1),
                                      connectionToDoom: _connHandler,
                                      onFailure: exc => TdsExecuteRPC_OnFailure(exc, stateObj));

                                  // Take care of releasing the locks
                                  if (releaseConnectionLock) {
                                      task.ContinueWith(_ => {
                                          _connHandler._parserLock.Release();
                                      }, TaskScheduler.Default);
                                      releaseConnectionLock = false;
                                  }

                                  return task;
                              }
                          }
#if DEBUG
                          else {
                              Debug.Assert(writeParamTask == null, "Should not have a task when executing sync");
                          }
#endif
                      } // parameter for loop

                      // If this is not the last RPC we are sending, add the batch flag
                      if (ii < (rpcArray.Length - 1)) {
                          if (_isYukon) {
                              stateObj.WriteByte(TdsEnums.YUKON_RPCBATCHFLAG);

                          }
                          else {
                              stateObj.WriteByte(TdsEnums.SHILOH_RPCBATCHFLAG);
                          }
                      }
                  } // rpc for loop

                  Task execFlushTask = stateObj.ExecuteFlush();
                  Debug.Assert(!sync || execFlushTask == null, "Should not get a task when executing sync");
                  if (execFlushTask != null) {
                      Task task = null;

                      if (completion == null) {
                          completion = new TaskCompletionSource<object>();
                          task = completion.Task;
                      }

                      bool taskReleaseConnectionLock = releaseConnectionLock;
                      execFlushTask.ContinueWith(tsk => ExecuteFlushTaskCallback(tsk, stateObj, completion, taskReleaseConnectionLock), TaskScheduler.Default);
                      
                      // ExecuteFlushTaskCallback will take care of the locks for us
                      releaseConnectionLock = false;
                      
                      return task;
                  }                  
              }
              catch (Exception e) {
                  // 
                  if (!ADP.IsCatchableExceptionType(e)) {
                      throw;
                  }

                  FailureCleanup(stateObj, e);

                  throw;
              }
              FinalizeExecuteRPC(stateObj);
              if (completion != null) {
                  completion.SetResult(null);
              }
              return null;
          }
          catch (Exception e) {
              FinalizeExecuteRPC(stateObj);
              if (completion != null) {
                  completion.SetException(e);
                  return null;
              }
              else {
                  throw e;
              }
          }
          finally {
              Debug.Assert(firstCall || !releaseConnectionLock, "Shouldn't be releasing locks synchronously after the first call");
              if (releaseConnectionLock) {
                  _connHandler._parserLock.Release();
              }
          }
      }
        
      private void FinalizeExecuteRPC(TdsParserStateObject stateObj) {
          stateObj.SniContext = SniContext.Snix_Read;
          _asyncWrite = false;
      }

      private void TdsExecuteRPC_OnFailure(Exception exc, TdsParserStateObject stateObj) {
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
                  FailureCleanup(stateObj, exc);
              }
#if DEBUG
              finally {
                  tdsReliabilitySection.Stop();
              }
#endif //DEBUG
          }
          catch (System.OutOfMemoryException) {
              _connHandler.DoomThisConnection();
              throw;
          }
          catch (System.StackOverflowException) {
              _connHandler.DoomThisConnection();
              throw;
          }
          catch (System.Threading.ThreadAbortException) {
              _connHandler.DoomThisConnection();
              throw;
          }
      }

     private void ExecuteFlushTaskCallback(Task tsk, TdsParserStateObject stateObj, TaskCompletionSource<object> completion, bool releaseConnectionLock) {
         try {
                FinalizeExecuteRPC(stateObj);
                if (tsk.Exception != null) {
                    Exception exc = tsk.Exception.InnerException;
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
                            FailureCleanup(stateObj, tsk.Exception);
                        }
    #if DEBUG
                        finally {
                            tdsReliabilitySection.Stop();
                        }
    #endif //DEBUG
                    }
                    catch (System.OutOfMemoryException e) {
                        _connHandler.DoomThisConnection();
                        completion.SetException(e);
                        throw;
                    }
                    catch (System.StackOverflowException e) {
                        _connHandler.DoomThisConnection();
                        completion.SetException(e);
                        throw;
                    }
                    catch (System.Threading.ThreadAbortException e) {
                        _connHandler.DoomThisConnection();
                        completion.SetException(e);
                        throw;
                    }
                    catch (Exception e) {
                        exc = e;
                    }
                    completion.SetException(exc);
                }
                else {
                    completion.SetResult(null);
                }
        }
        finally {
            if (releaseConnectionLock) {
                _connHandler._parserLock.Release();
            }
         }
      }


      private void WriteParameterName(string parameterName, TdsParserStateObject stateObj) {
          // paramLen
          // paramName
          if (!ADP.IsEmpty(parameterName)) {
              Debug.Assert(parameterName.Length <= 0xff, "parameter name can only be 255 bytes, shouldn't get to TdsParser!");
              int tempLen = parameterName.Length & 0xff;
              stateObj.WriteByte((byte)tempLen);
              WriteString(parameterName, tempLen, 0, stateObj);
          }
          else {
              stateObj.WriteByte(0);
          }
      }

        private static readonly IEnumerable<MSS.SqlDataRecord> __tvpEmptyValue = new List<MSS.SqlDataRecord>().AsReadOnly();
        private void WriteSmiParameter(SqlParameter param, int paramIndex, bool sendDefault, TdsParserStateObject stateObj) {
            //
            // Determine Metadata
            //
            ParameterPeekAheadValue peekAhead;
            MSS.SmiParameterMetaData metaData = param.MetaDataForSmi(out peekAhead);

            if (!_isKatmai) {
                MetaType mt = MetaType.GetMetaTypeFromSqlDbType(metaData.SqlDbType, metaData.IsMultiValued);
                throw ADP.VersionDoesNotSupportDataType(mt.TypeName);
            }

            //
            //  Determine value to send
            //
            object value;
            MSS.ExtendedClrTypeCode typeCode;

            // if we have an output or default param, set the value to null so we do not send it across to the server
            if (sendDefault) {
                // Value for TVP default is empty list, not NULL
                if (SqlDbType.Structured == metaData.SqlDbType && metaData.IsMultiValued) {
                    value = __tvpEmptyValue;
                    typeCode = MSS.ExtendedClrTypeCode.IEnumerableOfSqlDataRecord;
                }
                else {
                    // Need to send null value for default
                    value = null;
                    typeCode = MSS.ExtendedClrTypeCode.DBNull;
                }
            }
            else if (param.Direction == ParameterDirection.Output) {
                bool isCLRType = param.ParamaterIsSqlType;  // We have to forward the TYPE info, we need to know what type we are returning.  Once we null the paramater we will no longer be able to distinguish what type were seeing.
                param.Value = null;
                value = null;
                typeCode = MSS.ExtendedClrTypeCode.DBNull;
                param.ParamaterIsSqlType = isCLRType;
            }
            else {
                value = param.GetCoercedValue();
                typeCode = MSS.MetaDataUtilsSmi.DetermineExtendedTypeCodeForUseWithSqlDbType(
                                                    metaData.SqlDbType, metaData.IsMultiValued, value, null, MSS.SmiContextFactory.KatmaiVersion);
            }

            if (Bid.AdvancedOn) {
                Bid.Trace("<sc.TdsParser.WriteSmiParameter|ADV> %d#, Sending parameter '%ls', default flag=%d, metadata:\n", ObjectID, param.ParameterName, sendDefault?1:0);
                Bid.PutStr(metaData.TraceString(3));
                Bid.Trace("\n");
            }

            //
            // Write parameter metadata
            //
            WriteSmiParameterMetaData(metaData, sendDefault, stateObj);

            //
            // Now write the value
            //
            TdsParameterSetter paramSetter = new TdsParameterSetter(stateObj, metaData);
            MSS.ValueUtilsSmi.SetCompatibleValueV200(
                                        new MSS.SmiEventSink_Default(),  // TDS Errors/events dealt with at lower level for now, just need an object for processing
                                        paramSetter,
                                        0,          // ordinal.  TdsParameterSetter only handles one parameter at a time
                                        metaData,
                                        value,
                                        typeCode,
                                        param.Offset,
                                        0 < param.Size ? param.Size : -1,
                                        peekAhead);
        }

        // Writes metadata portion of parameter stream from an SmiParameterMetaData object.
        private void WriteSmiParameterMetaData(MSS.SmiParameterMetaData metaData, bool sendDefault, TdsParserStateObject stateObj) {
            // Determine status
            byte status = 0;
            if (ParameterDirection.Output == metaData.Direction || ParameterDirection.InputOutput == metaData.Direction) {
                status |= TdsEnums.RPC_PARAM_BYREF;
            }

            if (sendDefault) {
                status |= TdsEnums.RPC_PARAM_DEFAULT;
            }

            // Write everything out
            WriteParameterName(metaData.Name, stateObj);
            stateObj.WriteByte(status);
            WriteSmiTypeInfo(metaData, stateObj);
        }

        // Write a TypeInfo stream
        // Devnote: we remap the legacy types (text, ntext, and image) to SQLBIGVARCHAR,  SQLNVARCHAR, and SQLBIGVARBINARY
        private void WriteSmiTypeInfo(MSS.SmiExtendedMetaData metaData, TdsParserStateObject stateObj) {
            switch(metaData.SqlDbType) {
                case SqlDbType.BigInt:
                    stateObj.WriteByte(TdsEnums.SQLINTN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.Binary:
                    stateObj.WriteByte(TdsEnums.SQLBIGBINARY);
                    WriteUnsignedShort(checked((ushort)metaData.MaxLength), stateObj);
                    break;
                case SqlDbType.Bit:
                    stateObj.WriteByte(TdsEnums.SQLBITN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.Char:
                    stateObj.WriteByte(TdsEnums.SQLBIGCHAR);
                    WriteUnsignedShort(checked((ushort)(metaData.MaxLength)), stateObj);
                    WriteUnsignedInt(_defaultCollation.info, stateObj); // 
                    stateObj.WriteByte(_defaultCollation.sortId);
                    break;
                case SqlDbType.DateTime:
                    stateObj.WriteByte(TdsEnums.SQLDATETIMN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.Decimal:
                    stateObj.WriteByte(TdsEnums.SQLNUMERICN);
                    stateObj.WriteByte(checked((byte)MetaType.MetaDecimal.FixedLength));   // SmiMetaData's length and actual wire format's length are different
                    stateObj.WriteByte(0 == metaData.Precision ? (byte)1 : metaData.Precision);
                    stateObj.WriteByte(metaData.Scale);
                    break;
                case SqlDbType.Float:
                    stateObj.WriteByte(TdsEnums.SQLFLTN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.Image:
                    stateObj.WriteByte(TdsEnums.SQLBIGVARBINARY);
                    WriteUnsignedShort(unchecked((ushort)MSS.SmiMetaData.UnlimitedMaxLengthIndicator), stateObj);
                    break;
                case SqlDbType.Int:
                    stateObj.WriteByte(TdsEnums.SQLINTN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.Money:
                    stateObj.WriteByte(TdsEnums.SQLMONEYN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.NChar:
                    stateObj.WriteByte(TdsEnums.SQLNCHAR);
                    WriteUnsignedShort(checked((ushort)(metaData.MaxLength*2)), stateObj);
                    WriteUnsignedInt(_defaultCollation.info, stateObj); // 
                    stateObj.WriteByte(_defaultCollation.sortId);
                    break;
                case SqlDbType.NText:
                    stateObj.WriteByte(TdsEnums.SQLNVARCHAR);
                    WriteUnsignedShort(unchecked((ushort)MSS.SmiMetaData.UnlimitedMaxLengthIndicator), stateObj);
                    WriteUnsignedInt(_defaultCollation.info, stateObj); // 
                    stateObj.WriteByte(_defaultCollation.sortId);
                    break;
                case SqlDbType.NVarChar:
                    stateObj.WriteByte(TdsEnums.SQLNVARCHAR);
                    if (MSS.SmiMetaData.UnlimitedMaxLengthIndicator == metaData.MaxLength) {
                        WriteUnsignedShort(unchecked((ushort)MSS.SmiMetaData.UnlimitedMaxLengthIndicator), stateObj);
                    }
                    else {
                        WriteUnsignedShort(checked((ushort)(metaData.MaxLength*2)), stateObj);
                    }
                    WriteUnsignedInt(_defaultCollation.info, stateObj); // 
                    stateObj.WriteByte(_defaultCollation.sortId);
                    break;
                case SqlDbType.Real:
                    stateObj.WriteByte(TdsEnums.SQLFLTN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.UniqueIdentifier:
                    stateObj.WriteByte(TdsEnums.SQLUNIQUEID);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.SmallDateTime:
                    stateObj.WriteByte(TdsEnums.SQLDATETIMN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.SmallInt:
                    stateObj.WriteByte(TdsEnums.SQLINTN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.SmallMoney:
                    stateObj.WriteByte(TdsEnums.SQLMONEYN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.Text:
                    stateObj.WriteByte(TdsEnums.SQLBIGVARCHAR);
                    WriteUnsignedShort(unchecked((ushort)MSS.SmiMetaData.UnlimitedMaxLengthIndicator), stateObj);
                    WriteUnsignedInt(_defaultCollation.info, stateObj); // 
                    stateObj.WriteByte(_defaultCollation.sortId);
                    break;
                case SqlDbType.Timestamp:
                    stateObj.WriteByte(TdsEnums.SQLBIGBINARY);
                    WriteShort(checked((int)metaData.MaxLength), stateObj);
                    break;
                case SqlDbType.TinyInt:
                    stateObj.WriteByte(TdsEnums.SQLINTN);
                    stateObj.WriteByte(checked((byte)metaData.MaxLength));
                    break;
                case SqlDbType.VarBinary:
                    stateObj.WriteByte(TdsEnums.SQLBIGVARBINARY);
                    WriteUnsignedShort(unchecked((ushort)metaData.MaxLength), stateObj);
                    break;
                case SqlDbType.VarChar:
                    stateObj.WriteByte(TdsEnums.SQLBIGVARCHAR);
                    WriteUnsignedShort(unchecked((ushort)metaData.MaxLength), stateObj);
                    WriteUnsignedInt(_defaultCollation.info, stateObj); // 
                    stateObj.WriteByte(_defaultCollation.sortId);
                    break;
                case SqlDbType.Variant:
                    stateObj.WriteByte(TdsEnums.SQLVARIANT);
                    WriteInt(checked((int)metaData.MaxLength), stateObj);
                    break;
                case SqlDbType.Xml:
                    stateObj.WriteByte(TdsEnums.SQLXMLTYPE);
                    // Is there a schema
                    if (ADP.IsEmpty(metaData.TypeSpecificNamePart1) && ADP.IsEmpty(metaData.TypeSpecificNamePart2) &&
                            ADP.IsEmpty(metaData.TypeSpecificNamePart3)) {
                        stateObj.WriteByte(0);  // schema not present
                    }
                    else {
                        stateObj.WriteByte(1); // schema present
                        WriteIdentifier(metaData.TypeSpecificNamePart1, stateObj);
                        WriteIdentifier(metaData.TypeSpecificNamePart2, stateObj);
                        WriteIdentifierWithShortLength(metaData.TypeSpecificNamePart3, stateObj);
                    }
                    break;
                case SqlDbType.Udt:
                    stateObj.WriteByte(TdsEnums.SQLUDT);
                    WriteIdentifier(metaData.TypeSpecificNamePart1, stateObj);
                    WriteIdentifier(metaData.TypeSpecificNamePart2, stateObj);
                    WriteIdentifier(metaData.TypeSpecificNamePart3, stateObj);
                    break;
                case SqlDbType.Structured:
                    if (metaData.IsMultiValued) {
                        WriteTvpTypeInfo(metaData, stateObj);
                    }
                    else {
                        Debug.Assert(false, "SUDTs not yet supported.");
                    }
                    break;
                case SqlDbType.Date:
                    stateObj.WriteByte(TdsEnums.SQLDATE);
                    break;
                case SqlDbType.Time:
                    stateObj.WriteByte(TdsEnums.SQLTIME);
                    stateObj.WriteByte(metaData.Scale);
                    break;
                case SqlDbType.DateTime2:
                    stateObj.WriteByte(TdsEnums.SQLDATETIME2);
                    stateObj.WriteByte(metaData.Scale);
                    break;
                case SqlDbType.DateTimeOffset:
                    stateObj.WriteByte(TdsEnums.SQLDATETIMEOFFSET);
                    stateObj.WriteByte(metaData.Scale);
                    break;
                default:
                    Debug.Assert(false, "Unknown SqlDbType should have been caught earlier!");
                    break;
            }
        }

        private void WriteTvpTypeInfo(MSS.SmiExtendedMetaData metaData, TdsParserStateObject stateObj) {
            Debug.Assert(SqlDbType.Structured == metaData.SqlDbType && metaData.IsMultiValued, 
                        "Invalid metadata for TVPs. Type=" + metaData.SqlDbType);
            // Type token
            stateObj.WriteByte((byte)TdsEnums.SQLTABLE);

            // 3-part name (DB, Schema, TypeName)
            WriteIdentifier(metaData.TypeSpecificNamePart1, stateObj);
            WriteIdentifier(metaData.TypeSpecificNamePart2, stateObj);
            WriteIdentifier(metaData.TypeSpecificNamePart3, stateObj);

            // TVP_COLMETADATA
            if (0 == metaData.FieldMetaData.Count) {
                WriteUnsignedShort((ushort)TdsEnums.TVP_NOMETADATA_TOKEN, stateObj);
            }
            else {
                // COUNT of columns
                WriteUnsignedShort(checked((ushort) metaData.FieldMetaData.Count), stateObj);

                // TvpColumnMetaData for each column (look for defaults in this loop
                MSS.SmiDefaultFieldsProperty defaults = (MSS.SmiDefaultFieldsProperty) metaData.ExtendedProperties[MSS.SmiPropertySelector.DefaultFields];
                for(int i=0; i<metaData.FieldMetaData.Count; i++) {
                    WriteTvpColumnMetaData(metaData.FieldMetaData[i], defaults[i], stateObj);
                }

                // optional OrderUnique metadata
                WriteTvpOrderUnique(metaData, stateObj);

            }

            // END of optional metadata
            stateObj.WriteByte(TdsEnums.TVP_END_TOKEN);
        }

        // Write a single TvpColumnMetaData stream to the server
        private void WriteTvpColumnMetaData(MSS.SmiExtendedMetaData md, bool isDefault, TdsParserStateObject stateObj) {
            // User Type
            if (SqlDbType.Timestamp == md.SqlDbType) {
                WriteUnsignedInt(TdsEnums.SQLTIMESTAMP, stateObj);
            } else {
                WriteUnsignedInt(0, stateObj);
            }

            // Flags
            ushort status = TdsEnums.Nullable;
            if (isDefault) {
                status |= TdsEnums.TVP_DEFAULT_COLUMN;
            }
            WriteUnsignedShort(status, stateObj);

            // Type info
            WriteSmiTypeInfo(md, stateObj);

            // Column name
            // per spec, "ColName is never sent to server or client for TVP, it is required within a TVP to be zero length."
            WriteIdentifier(null, stateObj);
        }

        // temporary-results structure used only by WriteTvpOrderUnique
        //  use class to avoid List<T>'s per-struct-instantiated memory costs.
        private class TdsOrderUnique {
            internal short ColumnOrdinal;
            internal byte  Flags;

            internal TdsOrderUnique(short ordinal, byte flags) {
                ColumnOrdinal = ordinal;
                Flags = flags;
            }
        }

        private void WriteTvpOrderUnique(MSS.SmiExtendedMetaData metaData, TdsParserStateObject stateObj) {
            // TVP_ORDER_UNIQUE token (uniqueness and sort order)

            // Merge order and unique keys into a single token stream

            MSS.SmiOrderProperty orderProperty = (MSS.SmiOrderProperty) metaData.ExtendedProperties[MSS.SmiPropertySelector.SortOrder];
            MSS.SmiUniqueKeyProperty uniqueKeyProperty = (MSS.SmiUniqueKeyProperty) metaData.ExtendedProperties[MSS.SmiPropertySelector.UniqueKey];

            // Build list from
            List<TdsOrderUnique> columnList = new List<TdsOrderUnique>(metaData.FieldMetaData.Count);
            for(int i=0; i<metaData.FieldMetaData.Count; i++) {

                // Add appropriate SortOrder flag
                byte flags = 0;
                MSS.SmiOrderProperty.SmiColumnOrder columnOrder = orderProperty[i];
                if (SortOrder.Ascending == columnOrder.Order) {
                    flags = TdsEnums.TVP_ORDERASC_FLAG;
                }
                else if (SortOrder.Descending == columnOrder.Order) {
                    flags = TdsEnums.TVP_ORDERDESC_FLAG;
                }

                // Add unique key flage if appropriate
                if (uniqueKeyProperty[i]) {
                    flags |= TdsEnums.TVP_UNIQUE_FLAG;
                }

                // Remember this column if any flags were set
                if (0 != flags) {
                    columnList.Add(new TdsOrderUnique(checked((short)(i+1)), flags));
                }
            }

            // Write flagged columns to wire...
            if (0 < columnList.Count) {
                stateObj.WriteByte(TdsEnums.TVP_ORDER_UNIQUE_TOKEN);
                WriteShort(columnList.Count, stateObj);
                foreach(TdsOrderUnique column in columnList) {
                    WriteShort(column.ColumnOrdinal, stateObj);
                    stateObj.WriteByte(column.Flags);
                }
            }
        }

        internal Task WriteBulkCopyDone(TdsParserStateObject stateObj) {
            // Write DONE packet
            //
            if (!(State == TdsParserState.OpenNotLoggedIn || State == TdsParserState.OpenLoggedIn)) {
                throw ADP.ClosedConnectionError();
            }
            stateObj.WriteByte(TdsEnums.SQLDONE);
            WriteShort(0, stateObj);
            WriteShort(0, stateObj);
            WriteInt(0, stateObj);

            stateObj._pendingData = true;
            stateObj._messageStatus = 0;
            return stateObj.WritePacket(TdsEnums.HARDFLUSH);
        }

        /// <summary>
        /// Loads the column encryptions keys into cache. This will read the master key info, 
        /// decrypt the CEK and keep it ready for encryption.
        /// </summary>
        /// <returns></returns>
        internal void LoadColumnEncryptionKeys (_SqlMetaDataSet metadataCollection, string serverName) {
            if (_serverSupportsColumnEncryption && ShouldEncryptValuesForBulkCopy()) {
                for (int col = 0; col < metadataCollection.Length; col++) {
                    if (null != metadataCollection[col]) {
                        _SqlMetaData md = metadataCollection[col];
                        if (md.isEncrypted) {
                            SqlSecurityUtility.DecryptSymmetricKey(md.cipherMD, serverName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes a single entry of CEK Table into TDS Stream (for bulk copy).
        /// </summary>
        /// <returns></returns>
        internal void WriteEncryptionEntries (ref SqlTceCipherInfoTable cekTable, TdsParserStateObject stateObj) {
            for (int i =0; i < cekTable.Size; i++) {
                // Write Db ID
                WriteInt(cekTable[i].DatabaseId, stateObj);

                // Write Key ID
                WriteInt(cekTable[i].CekId, stateObj);

                // Write Key Version
                WriteInt(cekTable[i].CekVersion, stateObj);

                // Write 8 bytes of key MD Version
                Debug.Assert (8 == cekTable[i].CekMdVersion.Length);
                stateObj.WriteByteArray (cekTable[i].CekMdVersion, 8, 0);

                // We don't really need to send the keys
                stateObj.WriteByte(0x00);
            }
        }

        /// <summary>
        /// Writes a CEK Table (as part of  COLMETADATA token) for bulk copy.
        /// </summary>
        /// <returns></returns>
        internal void WriteCekTable (_SqlMetaDataSet metadataCollection, TdsParserStateObject stateObj) {
            if (!_serverSupportsColumnEncryption) {
                return;
            }

            // If no cek table is present, send a count of 0 for table size
            //     Note- Cek table (with 0 entries) will be present if TCE
            //     was enabled and server supports it!
            // OR if encryption was disabled in connection options
            if (!metadataCollection.cekTable.HasValue ||
                !ShouldEncryptValuesForBulkCopy()) {
                WriteShort(0x00, stateObj);
                return;
            }

            SqlTceCipherInfoTable cekTable = metadataCollection.cekTable.Value;
            ushort count = (ushort)cekTable.Size;

            WriteShort(count, stateObj);

            WriteEncryptionEntries(ref cekTable, stateObj);
        }

        /// <summary>
        /// Writes the UserType and TYPE_INFO values for CryptoMetadata (for bulk copy).
        /// </summary>
        /// <returns></returns>
        internal void WriteTceUserTypeAndTypeInfo(SqlMetaDataPriv mdPriv, TdsParserStateObject stateObj) {
            // Write the UserType (4 byte value)
            WriteInt(0x0, stateObj); // 

            Debug.Assert(SqlDbType.Xml != mdPriv.type);
            Debug.Assert(SqlDbType.Udt != mdPriv.type);

            stateObj.WriteByte(mdPriv.tdsType);

            switch (mdPriv.type) {
                case SqlDbType.Decimal:
                    WriteTokenLength(mdPriv.tdsType, mdPriv.length, stateObj);
                    stateObj.WriteByte(mdPriv.precision);
                    stateObj.WriteByte(mdPriv.scale);
                    break;
                case SqlDbType.Date:
                    // Nothing more to write!
                    break;
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    stateObj.WriteByte(mdPriv.scale);
                    break;
                default:
                    WriteTokenLength(mdPriv.tdsType, mdPriv.length, stateObj);
                    if (mdPriv.metaType.IsCharType && _isShiloh) {
                        WriteUnsignedInt(mdPriv.collation.info, stateObj);
                        stateObj.WriteByte(mdPriv.collation.sortId);
                    }
                    break;
            }
        }

        /// <summary>
        /// Writes the crypto metadata (as part of COLMETADATA token) for encrypted columns.
        /// </summary>
        /// <returns></returns>
        internal void WriteCryptoMetadata(_SqlMetaData md, TdsParserStateObject stateObj) {
            if (!_serverSupportsColumnEncryption || // TCE Feature supported
                !md.isEncrypted || // Column is not encrypted
                !ShouldEncryptValuesForBulkCopy()) { // TCE disabled on connection string
                return; 
            }

            // Write the ordinal
            WriteShort (md.cipherMD.CekTableOrdinal, stateObj);

            // Write UserType and TYPEINFO
            WriteTceUserTypeAndTypeInfo(md.baseTI, stateObj);

            // Write Encryption Algo
            stateObj.WriteByte(md.cipherMD.CipherAlgorithmId);

            if (TdsEnums.CustomCipherAlgorithmId == md.cipherMD.CipherAlgorithmId) {
                // Write the algorithm name
                Debug.Assert (md.cipherMD.CipherAlgorithmName.Length < 256);
                stateObj.WriteByte((byte)md.cipherMD.CipherAlgorithmName.Length);
                WriteString(md.cipherMD.CipherAlgorithmName, stateObj);
            }

            // Write Encryption Algo Type
            stateObj.WriteByte(md.cipherMD.EncryptionType);

            // Write Normalization Version
            stateObj.WriteByte(md.cipherMD.NormalizationRuleVersion);
        }

        internal void WriteBulkCopyMetaData(_SqlMetaDataSet metadataCollection, int count, TdsParserStateObject stateObj) {
            if (!(State == TdsParserState.OpenNotLoggedIn || State == TdsParserState.OpenLoggedIn)) {
                throw ADP.ClosedConnectionError();
            }

            stateObj.WriteByte(TdsEnums.SQLCOLMETADATA);
            WriteShort(count, stateObj);

            // Write CEK table - 0 count
            WriteCekTable(metadataCollection, stateObj);

            for (int i = 0; i < metadataCollection.Length; i++) {
                if (metadataCollection[i] != null) {
                    _SqlMetaData md = metadataCollection[i];

                    // read user type - 4 bytes Yukon, 2 backwards
                    if (IsYukonOrNewer) {
                        WriteInt(0x0, stateObj);
                    }
                    else {
                        WriteShort(0x0000, stateObj);
                    }

                    // Write the flags 
                    UInt16 flags;
                    flags = (UInt16)(md.updatability << 2);
                    flags |= (UInt16)(md.isNullable ? (UInt16)TdsEnums.Nullable : (UInt16)0);
                    flags |= (UInt16)(md.isIdentity ? (UInt16)TdsEnums.Identity : (UInt16)0);

                    // Write the next byte of flags
                    if (_serverSupportsColumnEncryption) { // TCE Supported
                        if (ShouldEncryptValuesForBulkCopy()) { // TCE enabled on connection options
                            flags |= (UInt16)(md.isEncrypted ? (UInt16)(TdsEnums.IsEncrypted << 8) : (UInt16)0);
                        }
                    }

                    WriteShort(flags, stateObj);// write the flags

                    // todo:
                    // for xml WriteTokenLength results in a no-op
                    // discuss this with blaine ...
                    // (Microsoft) xml datatype does not have token length in its metadata. So it should be a noop.

                    switch (md.type) {
                        case SqlDbType.Decimal:
                            stateObj.WriteByte(md.tdsType);
                            WriteTokenLength(md.tdsType, md.length, stateObj);
                            stateObj.WriteByte(md.precision);
                            stateObj.WriteByte(md.scale);
                            break;
                        case SqlDbType.Xml:
                            // 
                            stateObj.WriteByteArray(s_xmlMetadataSubstituteSequence, s_xmlMetadataSubstituteSequence.Length, 0);
                            break;
                        case SqlDbType.Udt:
                            stateObj.WriteByte(TdsEnums.SQLBIGVARBINARY);
                            WriteTokenLength(TdsEnums.SQLBIGVARBINARY, md.length, stateObj);
                            break;
                        case SqlDbType.Date:
                            stateObj.WriteByte(md.tdsType);
                            break;
                        case SqlDbType.Time:
                        case SqlDbType.DateTime2:
                        case SqlDbType.DateTimeOffset:
                            stateObj.WriteByte(md.tdsType);
                            stateObj.WriteByte(md.scale);
                            break;
                        default:
                            stateObj.WriteByte(md.tdsType);
                            WriteTokenLength(md.tdsType, md.length, stateObj);
                            if (md.metaType.IsCharType && _isShiloh) {
                                WriteUnsignedInt(md.collation.info, stateObj);
                                stateObj.WriteByte(md.collation.sortId);
                            }
                            break;
                    }

                    if (md.metaType.IsLong && !md.metaType.IsPlp) {
                        WriteShort(md.tableName.Length, stateObj);
                        WriteString(md.tableName, stateObj);
                    }

                    WriteCryptoMetadata(md, stateObj);

                    stateObj.WriteByte((byte)md.column.Length);
                    WriteString(md.column, stateObj);
                }
            } // end for loop
        }

        /// <summary>
        /// Determines if a column value should be encrypted when using BulkCopy (based on connectionstring setting).
        /// </summary>
        /// <returns></returns>
        internal bool ShouldEncryptValuesForBulkCopy () {
            if (null != _connHandler && 
                null != _connHandler.ConnectionOptions &&
                SqlConnectionColumnEncryptionSetting.Enabled == _connHandler.ConnectionOptions.ColumnEncryptionSetting) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Encrypts a column value (for SqlBulkCopy) 
        /// </summary>
        /// <returns></returns>
        internal object EncryptColumnValue (object value, SqlMetaDataPriv metadata, string column, TdsParserStateObject stateObj, bool isDataFeed, bool isSqlType) {
            Debug.Assert (_serverSupportsColumnEncryption, "Server doesn't support encryption, yet we received encryption metadata");
            Debug.Assert (ShouldEncryptValuesForBulkCopy(), "Encryption attempted when not requested");

            if (isDataFeed) { // can't encrypt a stream column
                SQL.StreamNotSupportOnEncryptedColumn(column);
            }

            int actualLengthInBytes;
            switch(metadata.baseTI.metaType.NullableType) {
                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLIMAGE:
                    // For some datatypes, engine does truncation before storing the value. (For example, when
                    // trying to insert a varbinary(7000) into a varbinary(3000) column). Since we encrypt the
                    // column values, engine has no way to tell the size of the plaintext datatype. Therefore,
                    // we truncate the values based on target column sizes here before encrypting them. This 
                    // truncation is only needed if we exceed the max column length or if the target column is
                    // not a blob type (eg. varbinary(max)). The actual work of truncating the column happens
                    // when we normalize and serialize the data buffers. The serialization routine expects us 
                    // to report the size of data to be copied out (for serialization). If we underreport the
                    // size, truncation will happen for us!
                    actualLengthInBytes = (isSqlType) ? ((SqlBinary)value).Length : ((byte[])value).Length;
                    if (metadata.baseTI.length > 0 &&
                        actualLengthInBytes > metadata.baseTI.length) { // see comments agove
                        actualLengthInBytes = metadata.baseTI.length;
                    }
                    break;

                case TdsEnums.SQLUNIQUEID:
                    actualLengthInBytes = GUID_SIZE;   // that's a constant for guid
                    break;
                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT:
                    if (null == _defaultEncoding)
                    {
                        ThrowUnsupportedCollationEncountered(null); // stateObject only when reading
                    }

                    string stringValue = (isSqlType) ? ((SqlString)value).Value : (string)value;
                    actualLengthInBytes = _defaultEncoding.GetByteCount(stringValue);

                    // If the string length is > max length, then use the max length (see comments above)
                    if (metadata.baseTI.length > 0 && 
                        actualLengthInBytes > metadata.baseTI.length) {
                        actualLengthInBytes = metadata.baseTI.length; // this ensure truncation!
                    }

                    break;
                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                    actualLengthInBytes = ((isSqlType) ? ((SqlString)value).Value.Length : ((string)value).Length) * 2;

                    if (metadata.baseTI.length > 0 &&
                        actualLengthInBytes > metadata.baseTI.length) { // see comments above
                        actualLengthInBytes = metadata.baseTI.length;
                    }

                    break;

                default:
                    actualLengthInBytes = metadata.baseTI.length;
                    break;
            }

            byte[] serializedValue;
            if (isSqlType) {
                // SqlType
                serializedValue = SerializeUnencryptedSqlValue (value, 
                                            metadata.baseTI.metaType, 
                                            actualLengthInBytes,
                                            offset : 0, 
                                            normalizationVersion: metadata.cipherMD.NormalizationRuleVersion, 
                                            stateObj: stateObj);
            }
            else {
                serializedValue = SerializeUnencryptedValue (value,
                                            metadata.baseTI.metaType, 
                                            metadata.baseTI.scale,
                                            actualLengthInBytes, 
                                            offset: 0, 
                                            isDataFeed: isDataFeed, 
                                            normalizationVersion: metadata.cipherMD.NormalizationRuleVersion, 
                                            stateObj: stateObj);
            }

            Debug.Assert(serializedValue != null, "serializedValue should not be null in TdsExecuteRPC.");
            return SqlSecurityUtility.EncryptWithKey(
                    serializedValue, 
                    metadata.cipherMD, 
                    _connHandler.ConnectionOptions.DataSource);
        }

        internal Task WriteBulkCopyValue(object value, SqlMetaDataPriv metadata, TdsParserStateObject stateObj, bool isSqlType, bool isDataFeed, bool isNull) {
            Debug.Assert(!isSqlType || value is INullable, "isSqlType is true, but value can not be type cast to an INullable");
            Debug.Assert(!isDataFeed ^ value is DataFeed, "Incorrect value for isDataFeed");

            Encoding saveEncoding = _defaultEncoding;
            SqlCollation saveCollation = _defaultCollation;
            int saveCodePage = _defaultCodePage;
            int saveLCID = _defaultLCID;
            Task resultTask = null;
            Task internalWriteTask = null;

            if (!(State == TdsParserState.OpenNotLoggedIn || State == TdsParserState.OpenLoggedIn)) {
                throw ADP.ClosedConnectionError();
            }
            try {
                if (metadata.encoding != null) {
                    _defaultEncoding = metadata.encoding;
                }
                if (metadata.collation != null) {
                    _defaultCollation = metadata.collation;
                    _defaultLCID = _defaultCollation.LCID;
                }
                _defaultCodePage = metadata.codePage;

                MetaType metatype = metadata.metaType;
                int ccb = 0;
                int ccbStringBytes = 0;

                if (isNull) {
                    // For UDT, remember we treat as binary even though it is a PLP
                    if (metatype.IsPlp && (metatype.NullableType != TdsEnums.SQLUDT || metatype.IsLong)) {
                        WriteLong(unchecked((long)TdsEnums.SQL_PLP_NULL), stateObj);
                    }
                    else if (!metatype.IsFixed && !metatype.IsLong && !metatype.IsVarTime) {
                        WriteShort(TdsEnums.VARNULL, stateObj);
                    }
                    else {
                        stateObj.WriteByte(TdsEnums.FIXEDNULL);
                    }
                    return resultTask;
                }

                if (!isDataFeed) {
                    switch (metatype.NullableType) {
                        case TdsEnums.SQLBIGBINARY:
                        case TdsEnums.SQLBIGVARBINARY:
                        case TdsEnums.SQLIMAGE:
                        case TdsEnums.SQLUDT:
                            ccb = (isSqlType) ? ((SqlBinary)value).Length : ((byte[])value).Length;
                            break;
                        case TdsEnums.SQLUNIQUEID:
                            ccb = GUID_SIZE;   // that's a constant for guid
                            break;
                        case TdsEnums.SQLBIGCHAR:
                        case TdsEnums.SQLBIGVARCHAR:
                        case TdsEnums.SQLTEXT:
                            if (null == _defaultEncoding) {
                                ThrowUnsupportedCollationEncountered(null); // stateObject only when reading
                            }

                            string stringValue = null;
                            if (isSqlType) {
                                stringValue = ((SqlString)value).Value;
                            }
                            else {
                                stringValue = (string)value;
                            }

                            ccb = stringValue.Length;
                            ccbStringBytes = _defaultEncoding.GetByteCount(stringValue);
                            break;
                        case TdsEnums.SQLNCHAR:
                        case TdsEnums.SQLNVARCHAR:
                        case TdsEnums.SQLNTEXT:
                            ccb = ((isSqlType) ? ((SqlString)value).Value.Length : ((string)value).Length) * 2;
                            break;
                        case TdsEnums.SQLXMLTYPE:
                            // Value here could be string or XmlReader
                            if (value is XmlReader) {
                                value = MetaType.GetStringFromXml((XmlReader)value);
                            }
                            ccb = ((isSqlType) ? ((SqlString)value).Value.Length : ((string)value).Length) * 2;
                            break;

                        default:
                            ccb = metadata.length;
                            break;
                    }
                }
                else {
                     Debug.Assert(metatype.IsLong &&
                         ((metatype.SqlDbType == SqlDbType.VarBinary && value is StreamDataFeed) ||
                          ((metatype.SqlDbType == SqlDbType.VarChar || metatype.SqlDbType == SqlDbType.NVarChar) && value is TextDataFeed) || 
                          (metatype.SqlDbType == SqlDbType.Xml && value is XmlDataFeed)), 
                    "Stream data feed should only be assigned to VarBinary(max), Text data feed should only be assigned to [N]VarChar(max), Xml data feed should only be assigned to XML(max)");
               }


                // Expected the text length in data stream for bulk copy of text, ntext, or image data.
                //
                if (metatype.IsLong) {
                    switch (metatype.SqlDbType) {
                        case SqlDbType.Text:
                        case SqlDbType.NText:
                        case SqlDbType.Image:
                            stateObj.WriteByteArray(s_longDataHeader, s_longDataHeader.Length, 0);
                            WriteTokenLength(metadata.tdsType, ccbStringBytes == 0 ? ccb : ccbStringBytes, stateObj);
                            break;

                        case SqlDbType.VarChar:
                        case SqlDbType.NVarChar:
                        case SqlDbType.VarBinary:
                        case SqlDbType.Xml:
                        case SqlDbType.Udt:
                            // plp data
                            WriteUnsignedLong(TdsEnums.SQL_PLP_UNKNOWNLEN, stateObj);
                            break;
                    }
                }
                else {
                    WriteTokenLength(metadata.tdsType, ccbStringBytes == 0 ? ccb : ccbStringBytes, stateObj);
                }

                if (isSqlType) {
                    internalWriteTask = WriteSqlValue(value, metatype, ccb, ccbStringBytes, 0, stateObj);
                }
                else if (metatype.SqlDbType != SqlDbType.Udt || metatype.IsLong) {
                    internalWriteTask = WriteValue(value, metatype, metadata.scale, ccb, ccbStringBytes, 0, stateObj, metadata.length, isDataFeed);
                    if ((internalWriteTask == null) && (_asyncWrite)) {
                        internalWriteTask = stateObj.WaitForAccumulatedWrites();
                    }
                    Debug.Assert(_asyncWrite || stateObj.WaitForAccumulatedWrites() == null, "Should not have accumulated writes when writing sync");
                }
                else {
                    WriteShort(ccb, stateObj);
                    internalWriteTask = stateObj.WriteByteArray((byte[])value, ccb, 0);
                }

#if DEBUG
                //In DEBUG mode, when SetAlwaysTaskOnWrite is true, we create a task. Allows us to verify async execution paths.
                if (_asyncWrite && internalWriteTask == null && SqlBulkCopy.SetAlwaysTaskOnWrite == true) {
                    internalWriteTask = Task.FromResult<object>(null);
                }
#endif
                if (internalWriteTask != null) { //i.e. the write was async.
                    resultTask = WriteBulkCopyValueSetupContinuation(internalWriteTask, saveEncoding, saveCollation, saveCodePage, saveLCID);
                }
            }
            finally {
                if (internalWriteTask == null) {
                    _defaultEncoding = saveEncoding;
                    _defaultCollation = saveCollation;
                    _defaultCodePage = saveCodePage;
                    _defaultLCID = saveLCID;
                }
            }
            return resultTask;
        }

        // This is in its own method to avoid always allocating the lambda in WriteBulkCopyValue
        private Task WriteBulkCopyValueSetupContinuation(Task internalWriteTask, Encoding saveEncoding, SqlCollation saveCollation, int saveCodePage, int saveLCID) {
            return internalWriteTask.ContinueWith<Task>(t => {
                _defaultEncoding = saveEncoding;
                _defaultCollation = saveCollation;
                _defaultCodePage = saveCodePage;
                _defaultLCID = saveLCID;
                return t;
            }, TaskScheduler.Default).Unwrap();
        }

        // Write mars header data, not including the mars header length
        private void WriteMarsHeaderData(TdsParserStateObject stateObj, SqlInternalTransaction transaction) {
            // Function to send over additional payload header data for Yukon and beyond only.
            Debug.Assert(_isYukon, "WriteMarsHeaderData called on a non-Yukon server");

            // These are not necessary - can have local started in distributed.
            // Debug.Assert(!(null != sqlTransaction && null != distributedTransaction), "Error to have local (api started) and distributed transaction at the same time!");
            // Debug.Assert(!(null != _userStartedLocalTransaction && null != distributedTransaction), "Error to have local (started outside of the api) and distributed transaction at the same time!");

            // We may need to update the mars header length if mars header is changed in the future

            WriteShort(TdsEnums.HEADERTYPE_MARS, stateObj);

            if (null != transaction && SqlInternalTransaction.NullTransactionId != transaction.TransactionId) {
                WriteLong(transaction.TransactionId, stateObj);
                WriteInt(stateObj.IncrementAndObtainOpenResultCount(transaction), stateObj);
            }
            else {
                // If no transaction, send over retained transaction descriptor (empty if none retained)
                //  and always 1 for result count.
                WriteLong(_retainedTransactionId, stateObj);
                WriteInt(stateObj.IncrementAndObtainOpenResultCount(null), stateObj);
            }
        }

        private int GetNotificationHeaderSize(SqlNotificationRequest notificationRequest) {
            if (null != notificationRequest) {
                string callbackId = notificationRequest.UserData;
                string service = notificationRequest.Options;
                int timeout = notificationRequest.Timeout;

                if (null == callbackId) {
                    throw ADP.ArgumentNull("CallbackId");
                }
                else if (UInt16.MaxValue < callbackId.Length) {
                    throw ADP.ArgumentOutOfRange("CallbackId");
                }

                if (null == service) {
                    throw ADP.ArgumentNull("Service");
                }
                else if (UInt16.MaxValue < service.Length) {
                    throw ADP.ArgumentOutOfRange("Service");
                }

                if (-1 > timeout) {
                    throw ADP.ArgumentOutOfRange("Timeout");
                }

                // Header Length (uint) (included in size) (already written to output buffer)
                // Header Type (ushort)
                // NotifyID Length (ushort)
                // NotifyID UnicodeStream (unicode text)
                // SSBDeployment Length (ushort)
                // SSBDeployment UnicodeStream (unicode text)
                // Timeout (uint) -- optional
                // WEBDATA 102263: Don't send timeout value if it is 0

                int headerLength = 4 + 2 + 2 + (callbackId.Length * 2) + 2 + (service.Length * 2);
                if (timeout > 0)
                    headerLength += 4;
                return headerLength;
            }
            else {
                return 0;
            }
        }

        // Write query notificaiton header data, not including the notificaiton header length
        private void WriteQueryNotificationHeaderData(SqlNotificationRequest notificationRequest, TdsParserStateObject stateObj) {
            Debug.Assert(_isYukon, "WriteQueryNotificationHeaderData called on a non-Yukon server");

            // We may need to update the notification header length if the header is changed in the future

            Debug.Assert (null != notificationRequest, "notificaitonRequest is null");
              
            string callbackId = notificationRequest.UserData;
            string service = notificationRequest.Options;
            int timeout = notificationRequest.Timeout;

            // we did verification in GetNotificationHeaderSize, so just assert here.
            Debug.Assert(null != callbackId, "CallbackId is null");
            Debug.Assert(UInt16.MaxValue >= callbackId.Length, "CallbackId length is out of range");
            Debug.Assert(null != service, "Service is null");
            Debug.Assert(UInt16.MaxValue >= service.Length, "Service length is out of range");
            Debug.Assert(-1 <= timeout, "Timeout");
                

            Bid.NotificationsTrace("<sc.TdsParser.WriteQueryNotificationHeader|DEP> NotificationRequest: userData: '%ls', options: '%ls', timeout: '%d'\n", notificationRequest.UserData, notificationRequest.Options, notificationRequest.Timeout);

            WriteShort(TdsEnums.HEADERTYPE_QNOTIFICATION, stateObj);      // Query notifications Type

            WriteShort(callbackId.Length * 2, stateObj); // Length in bytes
            WriteString(callbackId, stateObj);

            WriteShort(service.Length * 2, stateObj); // Length in bytes
            WriteString(service, stateObj);
            if (timeout > 0)
                WriteInt(timeout, stateObj);
        }

        // Write the trace header data, not including the trace header length
        private void WriteTraceHeaderData(TdsParserStateObject stateObj) {
            Debug.Assert(this.IncludeTraceHeader, "WriteTraceHeaderData can only be called on a Denali or higher version server and bid trace with the control bit are on");

            // We may need to update the trace header length if trace header is changed in the future

            ActivityCorrelator.ActivityId actId = ActivityCorrelator.Current;

            WriteShort(TdsEnums.HEADERTYPE_TRACE, stateObj);      // Trace Header Type

            stateObj.WriteByteArray(actId.Id.ToByteArray(), GUID_SIZE, 0); // Id (Guid)
            WriteUnsignedInt(actId.Sequence, stateObj); // sequence number

            Bid.Trace("<sc.TdsParser.WriteTraceHeaderData|INFO> ActivityID %ls\n", actId.ToString());
        }

        private void WriteRPCBatchHeaders(TdsParserStateObject stateObj, SqlNotificationRequest notificationRequest) {
            Debug.Assert(_isYukon, "WriteRPCBatchHeaders can only be called on Yukon or higher version servers");
            
            /* Header:
               TotalLength  - DWORD  - including all headers and lengths, including itself
               Each Data Session:
               {
                     HeaderLength - DWORD  - including all header length fields, including itself
                     HeaderType   - USHORT
                     HeaderData
               }
            */

            int notificationHeaderSize = GetNotificationHeaderSize(notificationRequest);

            const int marsHeaderSize = 18; // 4 + 2 + 8 + 4

            // Header Length (DWORD)
            // Header Type (ushort)
            // Trace Data Guid
            // Trace Data Sequence Number (uint)
            const int traceHeaderSize = 26;  // 4 + 2 + GUID_SIZE + sizeof(UInt32);

            // TotalLength  - DWORD  - including all headers and lengths, including itself
            int totalHeaderLength = this.IncludeTraceHeader ? (4 + marsHeaderSize + notificationHeaderSize + traceHeaderSize) : (4 + marsHeaderSize + notificationHeaderSize);
            Debug.Assert(stateObj._outBytesUsed == stateObj._outputHeaderLen, "Output bytes written before total header length");
            // Write total header length
            WriteInt(totalHeaderLength, stateObj);

            // Write Mars header length
            WriteInt(marsHeaderSize, stateObj);
            // Write Mars header data
            WriteMarsHeaderData(stateObj, CurrentTransaction);

            if (0 != notificationHeaderSize) {
                // Write Notification header length
                WriteInt(notificationHeaderSize, stateObj);
                // Write notificaiton header data
                WriteQueryNotificationHeaderData(notificationRequest, stateObj);
            }

            if (IncludeTraceHeader) {
                
                // Write trace header length
                WriteInt(traceHeaderSize, stateObj);
                // Write trace header data
                WriteTraceHeaderData(stateObj);
            }
        }


        //
        // Reverse function of GetTokenLength
        //
        private void WriteTokenLength(byte token, int length, TdsParserStateObject stateObj) {
            int tokenLength = 0;

            Debug.Assert(token != 0, "0 length token!");

            // For Plp fields, this should only be used when writing to metadata header.
            // For actual data length, WriteDataLength should be used.
            // For Xml fields, there is no token length field. For MAX fields it is 0xffff.
            if (_isYukon) {     // Handle Yukon specific exceptions
                if (TdsEnums.SQLUDT == token) {
                    tokenLength = 8;
                }
                else if (token == TdsEnums.SQLXMLTYPE) {
                    tokenLength = 8;
                }
            }

            if (tokenLength == 0) {
                switch (token & TdsEnums.SQLLenMask) {
                    case TdsEnums.SQLFixedLen:
                        Debug.Assert(length == 0x01 << ((token & 0x0c) >> 2), "length does not match encoded length in token");
                        tokenLength = 0;
                        break;

                    case TdsEnums.SQLZeroLen:
                        tokenLength = 0;
                        break;

                    case TdsEnums.SQLVarLen:
                    case TdsEnums.SQLVarCnt:
                        if (0 != (token & 0x80))
                            tokenLength = 2;
                        else if (0 == (token & 0x0c))

                            // 
                            tokenLength = 4;
                        else
                            tokenLength = 1;

                        break;

                    default:
                        Debug.Assert(false, "Unknown token length!");
                        break;
                }

                switch (tokenLength) {
                    case 1:
                        stateObj.WriteByte((byte)length);
                        break;

                    case 2:
                        WriteShort(length, stateObj);
                        break;

                    case 4:
                        WriteInt(length, stateObj);
                        break;

                    case 8:
                        // In the metadata case we write 0xffff for partial length prefixed types.
                        //  For actual data length preceding data, WriteDataLength should be used.
                        WriteShort(TdsEnums.SQL_USHORTVARMAXLEN, stateObj);
                        break;
                } // end switch
            }
        }

        // Returns true if BOM byte mark is needed for an XML value
        private bool IsBOMNeeded(MetaType type, object value) {
            if (type.NullableType == TdsEnums.SQLXMLTYPE) {
                Type currentType = value.GetType();
                
                if(currentType == typeof(SqlString)) {
                    if (!((SqlString)value).IsNull && ((((SqlString)value).Value).Length > 0)) {
                        if ((((SqlString)value).Value[0] & 0xff) != 0xff)
                            return true;
                    }
                }
                else if ((currentType == typeof(String)) && (((String)value).Length > 0)) {
                    if ((value != null) && (((String)value)[0] & 0xff) != 0xff)
                        return true;
                }
                else if (currentType == typeof(SqlXml)) {
                    if (!((SqlXml)value).IsNull) 
                        return true;
                }
                else if (currentType == typeof(XmlDataFeed)) {
                    return true;             // Values will eventually converted to unicode string here
                }
            }
            return false;
        }

        private Task GetTerminationTask(Task unterminatedWriteTask, object value, MetaType type, int actualLength, TdsParserStateObject stateObj, bool isDataFeed) {
            if (type.IsPlp && ((actualLength > 0) || isDataFeed)) {
                if (unterminatedWriteTask == null) {
                    WriteInt(0, stateObj);
                    return null;
                }
                else {
                    return AsyncHelper.CreateContinuationTask<int, TdsParserStateObject>(unterminatedWriteTask,
                        WriteInt, 0, stateObj,
                        connectionToDoom: _connHandler);
                }
            }
            else {
                return unterminatedWriteTask;
            }
        }


        private Task WriteSqlValue(object value, MetaType type, int actualLength, int codePageByteSize, int offset, TdsParserStateObject stateObj) {
            return GetTerminationTask(
                WriteUnterminatedSqlValue(value, type, actualLength, codePageByteSize, offset, stateObj),
                value, type, actualLength, stateObj, false);
        }  
        
        // For MAX types, this method can only write everything in one big chunk. If multiple
        // chunk writes needed, please use WritePlpBytes/WritePlpChars
        private Task WriteUnterminatedSqlValue(object value, MetaType type, int actualLength, int codePageByteSize, int offset, TdsParserStateObject stateObj) {
            Debug.Assert(((type.NullableType == TdsEnums.SQLXMLTYPE) ||
                   (value is INullable && !((INullable)value).IsNull)),
                   "unexpected null SqlType!");

            // parameters are always sent over as BIG or N types
            switch (type.NullableType) {
                case TdsEnums.SQLFLTN:
                    if (type.FixedLength == 4)
                        WriteFloat(((SqlSingle)value).Value, stateObj);
                    else {
                        Debug.Assert(type.FixedLength == 8, "Invalid length for SqlDouble type!");
                        WriteDouble(((SqlDouble)value).Value, stateObj);
                    }

                    break;

                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLIMAGE:
                    {
                        if (type.IsPlp) {
                            WriteInt(actualLength, stateObj);               // chunk length
                        }

                        if (value is SqlBinary) {
                            return stateObj.WriteByteArray(((SqlBinary)value).Value, actualLength, offset, canAccumulate:false);
                        }
                        else {
                            Debug.Assert(value is SqlBytes);
                            return stateObj.WriteByteArray(((SqlBytes)value).Value, actualLength, offset, canAccumulate:false);
                        }
                    }

                case TdsEnums.SQLUNIQUEID: 
                    {
                        byte[] b = ((SqlGuid)value).ToByteArray();

                        Debug.Assert((actualLength == b.Length) && (actualLength == 16), "Invalid length for guid type in com+ object");
                        stateObj.WriteByteArray(b, actualLength, 0);
                        break;
                    }

                case TdsEnums.SQLBITN: 
                    {
                        Debug.Assert(type.FixedLength == 1, "Invalid length for SqlBoolean type");
                        if (((SqlBoolean)value).Value == true)
                            stateObj.WriteByte(1);
                        else
                            stateObj.WriteByte(0);

                        break;
                    }

                case TdsEnums.SQLINTN:
                    if (type.FixedLength == 1)
                        stateObj.WriteByte(((SqlByte)value).Value);
                    else
                        if (type.FixedLength == 2)
                            WriteShort(((SqlInt16)value).Value, stateObj);
                        else
                            if (type.FixedLength == 4)
                                WriteInt(((SqlInt32)value).Value, stateObj);
                            else {
                                Debug.Assert(type.FixedLength == 8, "invalid length for SqlIntN type:  " + type.FixedLength.ToString(CultureInfo.InvariantCulture));
                                WriteLong(((SqlInt64)value).Value, stateObj);
                            }

                    break;

                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT:
                    if (type.IsPlp) {
                        WriteInt(codePageByteSize, stateObj);               // chunk length
                    }
                    if (value is SqlChars) {
                        String sch = new String(((SqlChars)value).Value);

                        return WriteEncodingChar(sch, actualLength, offset, _defaultEncoding, stateObj, canAccumulate:false);
                    }
                    else {
                        Debug.Assert(value is SqlString);
                        return WriteEncodingChar(((SqlString)value).Value, actualLength, offset, _defaultEncoding, stateObj, canAccumulate:false);
                    }


                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                case TdsEnums.SQLXMLTYPE:

                    if (type.IsPlp) {
                        if(IsBOMNeeded(type, value)) {
                            WriteInt(actualLength+2, stateObj);               // chunk length
                            WriteShort(TdsEnums.XMLUNICODEBOM , stateObj);
                        } else {
                            WriteInt(actualLength, stateObj);               // chunk length
                        }
                    }

                    // convert to cchars instead of cbytes
                    // Xml type is already converted to string through GetCoercedValue
                    if (actualLength != 0)
                        actualLength >>= 1;

                    if (value is SqlChars) {
                        return WriteCharArray(((SqlChars)value).Value, actualLength, offset, stateObj, canAccumulate:false);
                    }
                    else {
                        Debug.Assert(value is SqlString);
                        return WriteString(((SqlString)value).Value, actualLength, offset, stateObj, canAccumulate:false);
                    }

                case TdsEnums.SQLNUMERICN:
                    Debug.Assert(type.FixedLength <= 17, "Decimal length cannot be greater than 17 bytes");
                    WriteSqlDecimal((SqlDecimal)value, stateObj);
                    break;

                case TdsEnums.SQLDATETIMN:
                    SqlDateTime dt = (SqlDateTime)value;

                    if (type.FixedLength == 4) {
                        if (0 > dt.DayTicks || dt.DayTicks > UInt16.MaxValue)
                            throw SQL.SmallDateTimeOverflow(dt.ToString());

                        WriteShort(dt.DayTicks, stateObj);
                        WriteShort(dt.TimeTicks / SqlDateTime.SQLTicksPerMinute, stateObj);
                    }
                    else {
                        WriteInt(dt.DayTicks, stateObj);
                        WriteInt(dt.TimeTicks, stateObj);
                    }

                    break;

                case TdsEnums.SQLMONEYN: 
                    {
                        WriteSqlMoney((SqlMoney)value, type.FixedLength, stateObj);
                        break;
                    }

                case TdsEnums.SQLUDT:
                    Debug.Assert(false, "Called WriteSqlValue on UDT param.Should have already been handled");
                    throw SQL.UDTUnexpectedResult(value.GetType().AssemblyQualifiedName);

                default:
                    Debug.Assert(false, "Unknown TdsType!" + type.NullableType.ToString("x2", (IFormatProvider)null));
                    break;
            } // switch
            // return point for accumualated writes, note: non-accumulated writes returned from their case statements
            return null;
        }

        private class TdsOutputStream : Stream {
            TdsParser _parser;
            TdsParserStateObject _stateObj;
            byte[] _preambleToStrip;

            public TdsOutputStream(TdsParser parser, TdsParserStateObject stateObj, byte[] preambleToStrip) {
                _parser = parser;
                _stateObj = stateObj;
                _preambleToStrip = preambleToStrip;
            }

            public override bool CanRead {
                get { return false; }
            }

            public override bool CanSeek {
                get { return false; }
            }

            public override bool CanWrite {
                get { return true; }
            }

            public override void Flush() {
                // NOOP
            }

            public override long Length {
                get { throw new NotSupportedException(); }
            }

            public override long Position {
                get {
                    throw new NotSupportedException();
                }
                set {
                    throw new NotSupportedException();
                }
            }

            public override int Read(byte[] buffer, int offset, int count) {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotSupportedException();
            }

            public override void SetLength(long value) {
                throw new NotSupportedException();
            }

            private void StripPreamble(byte[] buffer, ref int offset, ref int count) {
                if (_preambleToStrip != null && count >= _preambleToStrip.Length) {

                    for (int idx = 0; idx < _preambleToStrip.Length; idx++) {
                        if (_preambleToStrip[idx] != buffer[idx]) {
                            _preambleToStrip = null;
                            return;
                        }
                    }

                    offset += _preambleToStrip.Length;
                    count -= _preambleToStrip.Length;
                }
                _preambleToStrip = null;
            }

            public override void Write(byte[] buffer, int offset, int count) {
                Debug.Assert(!_parser._asyncWrite);
                ValidateWriteParameters(buffer, offset, count);

                StripPreamble(buffer, ref offset, ref count);

                if (count > 0) {
                    _parser.WriteInt(count, _stateObj); // write length of chunk
                    _stateObj.WriteByteArray(buffer, count, offset);
                }
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
                Debug.Assert(_parser._asyncWrite);
                ValidateWriteParameters(buffer, offset, count);

                StripPreamble(buffer, ref offset, ref count);

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
                        Task task = null;
                        if (count > 0) {
                            _parser.WriteInt(count, _stateObj); // write length of chunk
                            task = _stateObj.WriteByteArray(buffer, count, offset, canAccumulate: false);
                        }
                        if (task == null) {
                            return CompletedTask;
                        }
                        else {
                            return task;
                        }
                    }
#if DEBUG
                    finally {
                        tdsReliabilitySection.Stop();
                    }
#endif //DEBUG
                }
                catch (System.OutOfMemoryException) {
                    _parser._connHandler.DoomThisConnection();
                    throw;
                }
                catch (System.StackOverflowException) {
                    _parser._connHandler.DoomThisConnection();
                    throw;
                }
                catch (System.Threading.ThreadAbortException) {
                    _parser._connHandler.DoomThisConnection();
                    throw;
                }
            }

            internal static void ValidateWriteParameters(byte[] buffer, int offset, int count) {
                if (buffer == null) {
                    throw ADP.ArgumentNull(ADP.ParameterBuffer);
                }
                if (offset < 0) {
                    throw ADP.ArgumentOutOfRange(ADP.ParameterOffset);
                }
                if (count < 0) {
                    throw ADP.ArgumentOutOfRange(ADP.ParameterCount);
                }
                try {
                    if (checked(offset + count) > buffer.Length) {
                        throw ExceptionBuilder.InvalidOffsetLength();
                    }
                }
                catch (OverflowException) {
                    // If we've overflowed when adding offset and count, then they never would have fit into buffer anyway
                    throw ExceptionBuilder.InvalidOffsetLength();
                }
            }

        }

        private class ConstrainedTextWriter : TextWriter {
            TextWriter _next;
            int _size;
            int _written;

            public ConstrainedTextWriter(TextWriter next, int size) {
                _next = next;
                _size = size;
                _written = 0;

                if (_size < 1) {
                    _size = int.MaxValue;
                }
            }

            public bool IsComplete {
                get {
                    return _size > 0 && _written >= _size;
                }
            }

            public override Encoding Encoding {
                get { return _next.Encoding; }
            }

            public override void Flush() {
                _next.Flush();
            }

            public override Task FlushAsync() {
                return _next.FlushAsync();
            }

            public override void Write(char value) {
                if (_written < _size) {
                    _next.Write(value);
                    _written++;
                }
                Debug.Assert(_size < 0 || _written <= _size, string.Format("Length of data written exceeds specified length.  Written: {0}, specified: {1}", _written, _size));
            }

            public override void Write(char[] buffer, int index, int count) {

                ValidateWriteParameters(buffer, index, count);
                
                Debug.Assert(_size >= _written);
                count = Math.Min(_size - _written, count);
                if (count > 0) {
                    _next.Write(buffer, index, count);
                }
                _written += count;
            }

            public override Task WriteAsync(char value) {

                if (_written < _size) {
                    _written++;
                    return _next.WriteAsync(value);
                }

                return CompletedTask;
            }

            public override Task WriteAsync(char[] buffer, int index, int count) {

                ValidateWriteParameters(buffer, index, count);

                Debug.Assert(_size >= _written);
                count = Math.Min(_size - _written, count);
                if (count > 0) {
                    _written += count;
                    return _next.WriteAsync(buffer, index, count);
                }

                return CompletedTask;
            }

            public override Task WriteAsync(string value) {
                return WriteAsync(value.ToCharArray());
            }

            internal static void ValidateWriteParameters(char[] buffer, int offset, int count) {
                if (buffer == null) {
                    throw ADP.ArgumentNull(ADP.ParameterBuffer);
                }
                if (offset < 0) {
                    throw ADP.ArgumentOutOfRange(ADP.ParameterOffset);
                }
                if (count < 0) {
                    throw ADP.ArgumentOutOfRange(ADP.ParameterCount);
                }
                try {
                    if (checked(offset + count) > buffer.Length) {
                        throw ExceptionBuilder.InvalidOffsetLength();
                    }
                }
                catch (OverflowException) {
                    // If we've overflowed when adding offset and count, then they never would have fit into buffer anyway
                    throw ExceptionBuilder.InvalidOffsetLength();
                }
            }

        }

        private async Task WriteXmlFeed(XmlDataFeed feed, TdsParserStateObject stateObj, bool needBom, Encoding encoding, int size) {
            byte[] preambleToSkip = null;
            if (!needBom) {
                preambleToSkip = encoding.GetPreamble();
            }
            ConstrainedTextWriter writer = new ConstrainedTextWriter(new StreamWriter(new TdsOutputStream(this, stateObj, preambleToSkip), encoding), size);

            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.CloseOutput = false;		// don't close the memory stream
            writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            if (_asyncWrite) {
                writerSettings.Async = true;
            }
            XmlWriter ww = XmlWriter.Create(writer, writerSettings);

            if (feed._source.ReadState == ReadState.Initial) {
                feed._source.Read();
            }

            while (!feed._source.EOF && !writer.IsComplete) {

                // We are copying nodes from a reader to a writer.  This will cause the
                // XmlDeclaration to be emitted despite ConformanceLevel.Fragment above.
                // Therefore, we filter out the XmlDeclaration while copying.
                if (feed._source.NodeType == XmlNodeType.XmlDeclaration) {
                    feed._source.Read();
                    continue;
                }

                if (_asyncWrite) {
                    await ww.WriteNodeAsync(feed._source, true).ConfigureAwait(false);
                }
                else {
                    ww.WriteNode(feed._source, true);
                }
            }

            if (_asyncWrite) {
                await ww.FlushAsync().ConfigureAwait(false);
            }
            else {
                ww.Flush();
            }
        }
       
        private async Task WriteTextFeed(TextDataFeed feed, Encoding encoding, bool needBom, TdsParserStateObject stateObj, int size) {
            Debug.Assert(encoding == null || !needBom);
            char[] inBuff = new char[constTextBufferSize];

            encoding = encoding ?? new UnicodeEncoding(false, false);
            ConstrainedTextWriter writer = new ConstrainedTextWriter(new StreamWriter(new TdsOutputStream(this, stateObj, null), encoding), size);

            if (needBom) {
                if (_asyncWrite) {
                    await writer.WriteAsync((char)TdsEnums.XMLUNICODEBOM).ConfigureAwait(false);
                }
                else {
                    writer.Write((char)TdsEnums.XMLUNICODEBOM);
                }
            }

            int nWritten = 0;
            do {
                int nRead = 0;

                if (_asyncWrite) {
                    nRead = await feed._source.ReadBlockAsync(inBuff, 0, constTextBufferSize).ConfigureAwait(false);
                }
                else {
                    nRead = feed._source.ReadBlock(inBuff, 0, constTextBufferSize);
                }

                if (nRead == 0) {
                    break; 
                }

                if (_asyncWrite) {
                    await writer.WriteAsync(inBuff, 0, nRead).ConfigureAwait(false);
                }
                else {
                    writer.Write(inBuff, 0, nRead);
                }

                nWritten += nRead;
            } while (!writer.IsComplete);

            if (_asyncWrite) {
                await writer.FlushAsync().ConfigureAwait(false);
            }
            else {
                writer.Flush();
            }
        }

        private async Task WriteStreamFeed(StreamDataFeed feed, TdsParserStateObject stateObj, int len) {
            TdsOutputStream output = new TdsOutputStream(this, stateObj, null);
            byte[] buff = new byte[constBinBufferSize];
            int nWritten = 0;
            do {
                int nRead = 0;
                int readSize = constBinBufferSize;
                if (len > 0 && nWritten + readSize > len) {
                    readSize = len - nWritten;
                }

                Debug.Assert(readSize >= 0);

                if (_asyncWrite) {
                    nRead = await feed._source.ReadAsync(buff, 0, readSize).ConfigureAwait(false);
                }
                else {
                    nRead = feed._source.Read(buff, 0, readSize);
                }

                if (nRead == 0) {
                    return; 
                }

                if (_asyncWrite) {
                    await output.WriteAsync(buff, 0, nRead).ConfigureAwait(false);
                }
                else {
                    output.Write(buff, 0, nRead);
                }

                nWritten += nRead;
            } while (len <= 0 || nWritten < len);
        }

        private Task NullIfCompletedWriteTask(Task task) {
            if (task == null) {
                return null;
            }
            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    return null;
                case TaskStatus.Faulted:
                    throw task.Exception.InnerException;
                case TaskStatus.Canceled:
                    throw SQL.OperationCancelled();
                default:
                    return task;
            }
        }

        private Task WriteValue(object value, MetaType type, byte scale, int actualLength, int encodingByteSize, int offset, TdsParserStateObject stateObj, int paramSize, bool isDataFeed) {
            return GetTerminationTask(WriteUnterminatedValue(value, type, scale, actualLength, encodingByteSize, offset, stateObj, paramSize, isDataFeed),
                value, type, actualLength, stateObj, isDataFeed);
        }

        // For MAX types, this method can only write everything in one big chunk. If multiple
        // chunk writes needed, please use WritePlpBytes/WritePlpChars
        private Task WriteUnterminatedValue(object value, MetaType type, byte scale, int actualLength, int encodingByteSize, int offset, TdsParserStateObject stateObj, int paramSize, bool isDataFeed)
        {
            Debug.Assert((null != value) && (DBNull.Value != value), "unexpected missing or empty object");

            // parameters are always sent over as BIG or N types
            switch (type.NullableType) {
                case TdsEnums.SQLFLTN:
                    if (type.FixedLength == 4)
                        WriteFloat((Single)value, stateObj);
                    else {
                        Debug.Assert(type.FixedLength == 8, "Invalid length for SqlDouble type!");
                        WriteDouble((Double)value, stateObj);
                    }

                    break;

                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLIMAGE:
                case TdsEnums.SQLUDT: {
                    // An array should be in the object
                    Debug.Assert(isDataFeed || value is byte[], "Value should be an array of bytes");
                    Debug.Assert(!isDataFeed || value is StreamDataFeed, "Value should be a stream");

                    if (isDataFeed) {
                        Debug.Assert(type.IsPlp,"Stream assigned to non-PLP was not converted!");
                        return NullIfCompletedWriteTask(WriteStreamFeed((StreamDataFeed)value, stateObj, paramSize));
                    }
                    else {
                        if (type.IsPlp) {
                            WriteInt(actualLength, stateObj);               // chunk length                        
                        }

                        return stateObj.WriteByteArray((byte[])value, actualLength, offset, canAccumulate: false);
                    }                           
                }

                case TdsEnums.SQLUNIQUEID: {
                        System.Guid guid = (System.Guid)value;
                        byte[] b = guid.ToByteArray();

                        Debug.Assert((actualLength == b.Length) && (actualLength == 16), "Invalid length for guid type in com+ object");
                        stateObj.WriteByteArray(b, actualLength, 0);
                        break;
                    }

                case TdsEnums.SQLBITN: {
                        Debug.Assert(type.FixedLength == 1, "Invalid length for SqlBoolean type");
                        if ((bool)value == true)
                            stateObj.WriteByte(1);
                        else
                            stateObj.WriteByte(0);

                        break;
                    }

                case TdsEnums.SQLINTN:
                    if (type.FixedLength == 1)
                        stateObj.WriteByte((byte)value);
                    else if (type.FixedLength == 2)
                        WriteShort((Int16)value, stateObj);
                    else if (type.FixedLength == 4)
                        WriteInt((Int32)value, stateObj);
                    else {
                        Debug.Assert(type.FixedLength == 8, "invalid length for SqlIntN type:  " + type.FixedLength.ToString(CultureInfo.InvariantCulture));
                        WriteLong((Int64)value, stateObj);
                    }

                    break;

                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT: {
                    Debug.Assert(!isDataFeed || (value is TextDataFeed || value is XmlDataFeed), "Value must be a TextReader or XmlReader");
                    Debug.Assert(isDataFeed || (value is string || value is byte[]), "Value is a byte array or string");

                    if (isDataFeed) {
                        Debug.Assert(type.IsPlp, "Stream assigned to non-PLP was not converted!");
                        TextDataFeed tdf = value as TextDataFeed;
                        if (tdf == null) {
                            return NullIfCompletedWriteTask(WriteXmlFeed((XmlDataFeed)value, stateObj, needBom:true, encoding:_defaultEncoding, size:paramSize));
                        }
                        else {
                            return NullIfCompletedWriteTask(WriteTextFeed(tdf, _defaultEncoding, false, stateObj, paramSize));
                        }
                    }
                    else {
                        if (type.IsPlp) {
                            WriteInt(encodingByteSize, stateObj);               // chunk length
                        }
                        if (value is byte[]) { // If LazyMat non-filled blob, send cookie rather than value
                            return stateObj.WriteByteArray((byte[])value, actualLength, 0, canAccumulate: false);
                        }
                        else {
                            return WriteEncodingChar((string)value, actualLength, offset, _defaultEncoding, stateObj, canAccumulate: false);
                        }
                    }
                }
                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                case TdsEnums.SQLXMLTYPE: {
                    Debug.Assert(!isDataFeed || (value is TextDataFeed || value is XmlDataFeed), "Value must be a TextReader or XmlReader");
                    Debug.Assert(isDataFeed || (value is string || value is byte[]), "Value is a byte array or string");

                    if (isDataFeed) {
                        Debug.Assert(type.IsPlp, "Stream assigned to non-PLP was not converted!");
                        TextDataFeed tdf = value as TextDataFeed;
                        if (tdf == null) {
                            return NullIfCompletedWriteTask(WriteXmlFeed((XmlDataFeed)value, stateObj, IsBOMNeeded(type, value), Encoding.Unicode, paramSize));
                        }
                        else {
                            return NullIfCompletedWriteTask(WriteTextFeed(tdf, null, IsBOMNeeded(type, value), stateObj, paramSize));
                        }
                    }
                    else {
                        if (type.IsPlp) {
                            if (IsBOMNeeded(type, value)) {
                                WriteInt(actualLength + 2, stateObj);               // chunk length
                                WriteShort(TdsEnums.XMLUNICODEBOM, stateObj);
                            }
                            else {
                                WriteInt(actualLength, stateObj);               // chunk length
                            }
                        }
                        if (value is byte[]) { // If LazyMat non-filled blob, send cookie rather than value
                            return stateObj.WriteByteArray((byte[])value, actualLength, 0, canAccumulate: false);
                        }
                        else {
                            // convert to cchars instead of cbytes
                            actualLength >>= 1;
                            return WriteString((string)value, actualLength, offset, stateObj, canAccumulate: false);
                        }
                    }
                }
                case TdsEnums.SQLNUMERICN:
                    Debug.Assert(type.FixedLength <= 17, "Decimal length cannot be greater than 17 bytes");
                    WriteDecimal((Decimal)value, stateObj);
                    break;

                case TdsEnums.SQLDATETIMN:
                    Debug.Assert(type.FixedLength <= 0xff, "Invalid Fixed Length");

                    TdsDateTime dt = MetaType.FromDateTime((DateTime)value, (byte)type.FixedLength);

                    if (type.FixedLength == 4) {
                        if (0 > dt.days || dt.days > UInt16.MaxValue)
                            throw SQL.SmallDateTimeOverflow(MetaType.ToDateTime(dt.days, dt.time, 4).ToString(CultureInfo.InvariantCulture));

                        WriteShort(dt.days, stateObj);
                        WriteShort(dt.time, stateObj);
                    }
                    else {
                        WriteInt(dt.days, stateObj);
                        WriteInt(dt.time, stateObj);
                    }

                    break;

                case TdsEnums.SQLMONEYN: {
                        WriteCurrency((Decimal)value, type.FixedLength, stateObj);
                        break;
                    }

                case TdsEnums.SQLDATE: {
                        WriteDate((DateTime)value, stateObj);
                        break;
                    }

                case TdsEnums.SQLTIME:
                    if (scale > TdsEnums.DEFAULT_VARTIME_SCALE) {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    WriteTime((TimeSpan)value, scale, actualLength, stateObj);
                    break;

                case TdsEnums.SQLDATETIME2:
                    if (scale > TdsEnums.DEFAULT_VARTIME_SCALE) {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    WriteDateTime2((DateTime)value, scale, actualLength, stateObj);
                    break;

                case TdsEnums.SQLDATETIMEOFFSET:
                    WriteDateTimeOffset((DateTimeOffset)value, scale, actualLength, stateObj);
                    break;

                default:
                    Debug.Assert(false, "Unknown TdsType!" + type.NullableType.ToString("x2", (IFormatProvider)null));
                    break;
            } // switch
            // return point for accumualated writes, note: non-accumulated writes returned from their case statements
            return null;
            // Debug.WriteLine("value:  " + value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Write parameter encryption metadata and returns a task if necessary.
        /// </summary>
        private Task WriteEncryptionMetadata(Task terminatedWriteTask, SqlColumnEncryptionInputParameterInfo columnEncryptionParameterInfo, TdsParserStateObject stateObj) {
            Debug.Assert(columnEncryptionParameterInfo != null, @"columnEncryptionParameterInfo cannot be null");
            Debug.Assert(stateObj != null, @"stateObj cannot be null");

            // If there is not task already, simply write the encryption metadata synchronously.
            if (terminatedWriteTask == null) {
                WriteEncryptionMetadata(columnEncryptionParameterInfo, stateObj);
                return null;
            }
            else {
                // Otherwise, create a continuation task to write the encryption metadata after the previous write completes.
                return AsyncHelper.CreateContinuationTask<SqlColumnEncryptionInputParameterInfo, TdsParserStateObject>(terminatedWriteTask,
                    WriteEncryptionMetadata, columnEncryptionParameterInfo, stateObj,
                    connectionToDoom: _connHandler);
            }
        }

        /// <summary>
        /// Write parameter encryption metadata.
        /// </summary>
        private void WriteEncryptionMetadata(SqlColumnEncryptionInputParameterInfo columnEncryptionParameterInfo, TdsParserStateObject stateObj) {
            Debug.Assert(columnEncryptionParameterInfo != null, @"columnEncryptionParameterInfo cannot be null");
            Debug.Assert(stateObj != null, @"stateObj cannot be null");

            // Write the TypeInfo.
            WriteSmiTypeInfo(columnEncryptionParameterInfo.ParameterMetadata, stateObj);

            // Write the serialized array in columnEncryptionParameterInfo.
            stateObj.WriteByteArray(columnEncryptionParameterInfo.SerializedWireFormat,
                                    columnEncryptionParameterInfo.SerializedWireFormat.Length,
                                    offsetBuffer: 0);
        }

        // For MAX types, this method can only write everything in one big chunk. If multiple
        // chunk writes needed, please use WritePlpBytes/WritePlpChars
        private byte[] SerializeUnencryptedValue(object value, MetaType type, byte scale, int actualLength, int offset, bool isDataFeed, byte normalizationVersion, TdsParserStateObject stateObj) {
            Debug.Assert((null != value) && (DBNull.Value != value), "unexpected missing or empty object");

            if (normalizationVersion != 0x01) {
                throw SQL.UnsupportedNormalizationVersion(normalizationVersion);
            }

            // parameters are always sent over as BIG or N types
            switch (type.NullableType) {
                case TdsEnums.SQLFLTN:
                    if (type.FixedLength == 4)
                        return SerializeFloat((Single)value);
                    else {
                        Debug.Assert(type.FixedLength == 8, "Invalid length for SqlDouble type!");
                        return SerializeDouble((Double)value);
                    }

                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLIMAGE:
                case TdsEnums.SQLUDT: {
                        Debug.Assert(!isDataFeed, "We cannot seriliaze streams");
                        Debug.Assert(value is byte[], "Value should be an array of bytes");

                        byte[] b = new byte[actualLength];
                        Buffer.BlockCopy((byte[])value, offset, b, 0, actualLength);
                        return b;
                    }

                case TdsEnums.SQLUNIQUEID: {
                        System.Guid guid = (System.Guid)value;
                        byte[] b = guid.ToByteArray();

                        Debug.Assert((actualLength == b.Length) && (actualLength == 16), "Invalid length for guid type in com+ object");
                        return b;
                    }

                case TdsEnums.SQLBITN: {
                        Debug.Assert(type.FixedLength == 1, "Invalid length for SqlBoolean type");

                        // We normalize to allow conversion across data types. BIT is serialized into a BIGINT.
                        return SerializeLong((bool)value == true ? 1 : 0, stateObj);
                    }

                case TdsEnums.SQLINTN:
                    if (type.FixedLength == 1)
                        return SerializeLong((byte)value, stateObj);
                    
                    if (type.FixedLength == 2)
                        return SerializeLong((Int16)value, stateObj);
                    
                    if (type.FixedLength == 4)
                        return SerializeLong((Int32)value, stateObj);
                    
                    Debug.Assert(type.FixedLength == 8, "invalid length for SqlIntN type:  " + type.FixedLength.ToString(CultureInfo.InvariantCulture));
                    return SerializeLong((Int64)value, stateObj);

                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT: {
                        Debug.Assert(!isDataFeed, "We cannot seriliaze streams");
                        Debug.Assert((value is string || value is byte[]), "Value is a byte array or string");

                        if (value is byte[]) { // If LazyMat non-filled blob, send cookie rather than value
                            byte[] b = new byte[actualLength];
                            Buffer.BlockCopy((byte[])value, 0, b, 0, actualLength);
                            return b;
                        }
                        else {
                            return SerializeEncodingChar((string)value, actualLength, offset, _defaultEncoding);
                        }
                    }
                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                case TdsEnums.SQLXMLTYPE: {
                        Debug.Assert(!isDataFeed, "We cannot seriliaze streams");
                        Debug.Assert((value is string || value is byte[]), "Value is a byte array or string");

                        if (value is byte[]) { // If LazyMat non-filled blob, send cookie rather than value
                            byte[] b = new byte[actualLength];
                            Buffer.BlockCopy((byte[])value, 0, b, 0, actualLength);
                            return b;
                        }
                        else { // convert to cchars instead of cbytes
                            actualLength >>= 1;
                            return SerializeString((string)value, actualLength, offset);
                        }
                    }
                case TdsEnums.SQLNUMERICN:
                    Debug.Assert(type.FixedLength <= 17, "Decimal length cannot be greater than 17 bytes");
                    return SerializeDecimal((Decimal)value, stateObj);

                case TdsEnums.SQLDATETIMN:
                    Debug.Assert(type.FixedLength <= 0xff, "Invalid Fixed Length");

                    TdsDateTime dt = MetaType.FromDateTime((DateTime)value, (byte)type.FixedLength);

                    if (type.FixedLength == 4) {
                        if (0 > dt.days || dt.days > UInt16.MaxValue)
                            throw SQL.SmallDateTimeOverflow(MetaType.ToDateTime(dt.days, dt.time, 4).ToString(CultureInfo.InvariantCulture));

                        if (null == stateObj._bIntBytes) {
                            stateObj._bIntBytes = new byte[4];
                        }

                        byte[] b = stateObj._bIntBytes;
                        int current = 0;

                        byte[] bPart = SerializeShort(dt.days, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 2);
                        current += 2;

                        bPart = SerializeShort(dt.time, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 2);

                        return b;
                    }
                    else {
                        if (null == stateObj._bLongBytes) {
                            stateObj._bLongBytes = new byte[8];
                        }
                        byte[] b = stateObj._bLongBytes;
                        int current = 0;

                        byte[] bPart = SerializeInt(dt.days, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 4);
                        current += 4;

                        bPart = SerializeInt(dt.time, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 4);

                        return b;
                    }

                case TdsEnums.SQLMONEYN: {
                        return SerializeCurrency((Decimal)value, type.FixedLength, stateObj);
                    }

                case TdsEnums.SQLDATE: {
                        return SerializeDate((DateTime)value);
                    }

                case TdsEnums.SQLTIME:
                    if (scale > TdsEnums.DEFAULT_VARTIME_SCALE) {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    return SerializeTime((TimeSpan)value, scale, actualLength);

                case TdsEnums.SQLDATETIME2:
                    if (scale > TdsEnums.DEFAULT_VARTIME_SCALE) {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    return SerializeDateTime2((DateTime)value, scale, actualLength);

                case TdsEnums.SQLDATETIMEOFFSET:
                    if (scale > TdsEnums.DEFAULT_VARTIME_SCALE) {
                        throw SQL.TimeScaleValueOutOfRange(scale);
                    }
                    return SerializeDateTimeOffset((DateTimeOffset)value, scale, actualLength);

                default:
                    throw SQL.UnsupportedDatatypeEncryption(type.TypeName);
            } // switch
            // Debug.WriteLine("value:  " + value.ToString(CultureInfo.InvariantCulture));
        }

        // For MAX types, this method can only write everything in one big chunk. If multiple
        // chunk writes needed, please use WritePlpBytes/WritePlpChars
        private byte[] SerializeUnencryptedSqlValue(object value, MetaType type, int actualLength, int offset, byte normalizationVersion, TdsParserStateObject stateObj) {
            Debug.Assert(((type.NullableType == TdsEnums.SQLXMLTYPE) ||
                   (value is INullable && !((INullable)value).IsNull)),
                   "unexpected null SqlType!");

            if (normalizationVersion != 0x01) {
                throw SQL.UnsupportedNormalizationVersion(normalizationVersion);
            }

            // parameters are always sent over as BIG or N types
            switch (type.NullableType) {
                case TdsEnums.SQLFLTN:
                    if (type.FixedLength == 4)
                        return SerializeFloat(((SqlSingle)value).Value);
                    else {
                        Debug.Assert(type.FixedLength == 8, "Invalid length for SqlDouble type!");
                        return SerializeDouble(((SqlDouble)value).Value);
                    }

                case TdsEnums.SQLBIGBINARY:
                case TdsEnums.SQLBIGVARBINARY:
                case TdsEnums.SQLIMAGE: {
                        byte[] b = new byte[actualLength];

                        if (value is SqlBinary) {
                            Buffer.BlockCopy(((SqlBinary)value).Value, offset, b, 0, actualLength);
                        }
                        else {
                            Debug.Assert(value is SqlBytes);
                            Buffer.BlockCopy(((SqlBytes)value).Value, offset, b, 0, actualLength);
                        }
                        return b;
                    }

                case TdsEnums.SQLUNIQUEID: {
                        byte[] b = ((SqlGuid)value).ToByteArray();

                        Debug.Assert((actualLength == b.Length) && (actualLength == 16), "Invalid length for guid type in com+ object");
                        return b;
                    }

                case TdsEnums.SQLBITN: {
                        Debug.Assert(type.FixedLength == 1, "Invalid length for SqlBoolean type");

                        // We normalize to allow conversion across data types. BIT is serialized into a BIGINT.
                        return SerializeLong(((SqlBoolean)value).Value == true ? 1 : 0, stateObj);
                    }

                case TdsEnums.SQLINTN:
                    // We normalize to allow conversion across data types. All data types below are serialized into a BIGINT.
                    if (type.FixedLength == 1)
                        return SerializeLong(((SqlByte)value).Value, stateObj);
                    
                    if (type.FixedLength == 2)
                        return SerializeLong(((SqlInt16)value).Value, stateObj);
                    
                    if (type.FixedLength == 4)
                        return SerializeLong(((SqlInt32)value).Value, stateObj);
                    else {
                        Debug.Assert(type.FixedLength == 8, "invalid length for SqlIntN type:  " + type.FixedLength.ToString(CultureInfo.InvariantCulture));
                        return SerializeLong(((SqlInt64)value).Value, stateObj);
                    }

                case TdsEnums.SQLBIGCHAR:
                case TdsEnums.SQLBIGVARCHAR:
                case TdsEnums.SQLTEXT:
                    if (value is SqlChars) {
                        String sch = new String(((SqlChars)value).Value);
                        return SerializeEncodingChar(sch, actualLength, offset, _defaultEncoding);
                    }
                    else {
                        Debug.Assert(value is SqlString);
                        return SerializeEncodingChar(((SqlString)value).Value, actualLength, offset, _defaultEncoding);
                    }


                case TdsEnums.SQLNCHAR:
                case TdsEnums.SQLNVARCHAR:
                case TdsEnums.SQLNTEXT:
                case TdsEnums.SQLXMLTYPE:
                    // convert to cchars instead of cbytes
                    // Xml type is already converted to string through GetCoercedValue
                    if (actualLength != 0)
                        actualLength >>= 1;

                    if (value is SqlChars) {
                        return SerializeCharArray(((SqlChars)value).Value, actualLength, offset);
                    }
                    else {
                        Debug.Assert(value is SqlString);
                        return SerializeString(((SqlString)value).Value, actualLength, offset);
                    }

                case TdsEnums.SQLNUMERICN:
                    Debug.Assert(type.FixedLength <= 17, "Decimal length cannot be greater than 17 bytes");
                    return SerializeSqlDecimal((SqlDecimal)value, stateObj);

                case TdsEnums.SQLDATETIMN:
                    SqlDateTime dt = (SqlDateTime)value;

                    if (type.FixedLength == 4) {
                        if (0 > dt.DayTicks || dt.DayTicks > UInt16.MaxValue)
                            throw SQL.SmallDateTimeOverflow(dt.ToString());

                        if (null == stateObj._bIntBytes) {
                            stateObj._bIntBytes = new byte[4];
                        }

                        byte[] b = stateObj._bIntBytes;
                        int current = 0;

                        byte[] bPart = SerializeShort(dt.DayTicks, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 2);
                        current += 2;

                        bPart = SerializeShort(dt.TimeTicks / SqlDateTime.SQLTicksPerMinute, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 2);

                        return b;
                    }
                    else {
                        if (null == stateObj._bLongBytes) {
                            stateObj._bLongBytes = new byte[8];
                        }

                        byte[] b = stateObj._bLongBytes;
                        int current = 0;

                        byte[] bPart = SerializeInt(dt.DayTicks, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 4);
                        current += 4;

                        bPart = SerializeInt(dt.TimeTicks, stateObj);
                        Buffer.BlockCopy(bPart, 0, b, current, 4);

                        return b;
                    }

                case TdsEnums.SQLMONEYN: {
                        return SerializeSqlMoney((SqlMoney)value, type.FixedLength, stateObj);
                    }

                default:
                    throw SQL.UnsupportedDatatypeEncryption(type.TypeName);
            } // switch
        }

        //
        // we always send over nullable types for parameters so we always write the varlen fields
        //

        internal void WriteParameterVarLen(MetaType type, int size, bool isNull, TdsParserStateObject stateObj, bool unknownLength=false) {
            if (type.IsLong) { // text/image/SQLVariant have a 4 byte length, plp datatypes have 8 byte lengths
                if (isNull) {
                    if (type.IsPlp) {
                        WriteLong(unchecked((long)TdsEnums.SQL_PLP_NULL), stateObj);
                    }
                    else {
                        WriteInt(unchecked((int)TdsEnums.VARLONGNULL), stateObj);
                    }
                }
                else if (type.NullableType == TdsEnums.SQLXMLTYPE || unknownLength) {
                    WriteUnsignedLong(TdsEnums.SQL_PLP_UNKNOWNLEN, stateObj);
                }
                else if (type.IsPlp) {
                    // Non-xml plp types
                    WriteLong((long)size, stateObj);
                }
                else {
                    WriteInt(size, stateObj);
                }
            }
            else if (type.IsVarTime) {
                if (isNull) {
                    stateObj.WriteByte(TdsEnums.FIXEDNULL);
                }
                else {
                    stateObj.WriteByte((byte)size);
                }
            }
            else if (false == type.IsFixed) { // non-long but variable length column, must be a BIG* type: 2 byte length
                if (isNull) {
                    WriteShort(TdsEnums.VARNULL, stateObj);
                }
                else {
                    WriteShort(size, stateObj);
                }
            }
            else {
                if (isNull) {
                    stateObj.WriteByte(TdsEnums.FIXEDNULL);
                }
                else {
                    Debug.Assert(type.FixedLength <= 0xff, "WriteParameterVarLen: invalid one byte length!");
                    stateObj.WriteByte((byte)(type.FixedLength & 0xff)); // 1 byte for everything else
                }
            }
        }

        // Reads the next chunk in a nvarchar(max) data stream.
        // This call must be preceeded by a call to ReadPlpLength or ReadDataLength.
        // Will not start reading into the next chunk if bytes requested is larger than
        // the current chunk length. Do another ReadPlpLength, ReadPlpUnicodeChars in that case.
        // Returns the actual chars read
        private bool TryReadPlpUnicodeCharsChunk(char[] buff, int offst, int len, TdsParserStateObject stateObj, out int charsRead) {

            Debug.Assert((buff == null && len == 0) || (buff.Length >= offst + len), "Invalid length sent to ReadPlpUnicodeChars()!");
            Debug.Assert((stateObj._longlen != 0) && (stateObj._longlen != TdsEnums.SQL_PLP_NULL),
                        "Out of sync plp read request");
            if (stateObj._longlenleft == 0) {
                Debug.Assert(false, "Out of sync read request");
                charsRead = 0;
                return true;
            }

            charsRead = len;

            // stateObj._longlenleft is in bytes
            if ((stateObj._longlenleft >> 1) < (ulong)len)
                charsRead = (int)(stateObj._longlenleft >> 1);

            for (int ii = 0; ii < charsRead; ii++) {
                if (!stateObj.TryReadChar(out buff[offst + ii])) {
                    return false;
                }
            }

            stateObj._longlenleft -= ((ulong)charsRead << 1);
            return true;
        }

        internal int ReadPlpUnicodeChars(ref char[] buff, int offst, int len, TdsParserStateObject stateObj) {
            int charsRead;
            Debug.Assert(stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
            bool result = TryReadPlpUnicodeChars(ref buff, offst, len, stateObj, out charsRead);
            if (!result) { throw SQL.SynchronousCallMayNotPend(); }
            return charsRead;
        }

        // Reads the requested number of chars from a plp data stream, or the entire data if
        // requested length is -1 or larger than the actual length of data. First call to this method
        //  should be preceeded by a call to ReadPlpLength or ReadDataLength.
        // Returns the actual chars read.
        internal bool TryReadPlpUnicodeChars(ref char[] buff, int offst, int len, TdsParserStateObject stateObj, out int totalCharsRead) {
            int charsRead = 0;
            int charsLeft = 0;
            char[] newbuf;

            if (stateObj._longlen == 0) {
                Debug.Assert(stateObj._longlenleft == 0);
                totalCharsRead = 0;
                return true;       // No data
            }

            Debug.Assert(((ulong)stateObj._longlen != TdsEnums.SQL_PLP_NULL),
                    "Out of sync plp read request");

            Debug.Assert((buff == null && offst == 0) || (buff.Length >= offst + len), "Invalid length sent to ReadPlpUnicodeChars()!");
            charsLeft = len;

            // If total length is known up front, allocate the whole buffer in one shot instead of realloc'ing and copying over each time
            if (buff == null && stateObj._longlen != TdsEnums.SQL_PLP_UNKNOWNLEN) {
                buff = new char[(int)Math.Min((int)stateObj._longlen, len)];
            }

            if (stateObj._longlenleft == 0) {
                ulong ignored;
                if (!stateObj.TryReadPlpLength(false, out ignored)) {
                    totalCharsRead = 0;
                    return false;
                }
                if (stateObj._longlenleft == 0) { // Data read complete
                    totalCharsRead = 0;
                    return true;
                }
            }

            totalCharsRead = 0;

            while (charsLeft > 0) {
                charsRead = (int)Math.Min((stateObj._longlenleft + 1) >> 1, (ulong)charsLeft);
                if ((buff == null) || (buff.Length < (offst + charsRead))) {
                    // Grow the array
                    newbuf = new char[offst + charsRead];
                    if (buff != null) {
                        Buffer.BlockCopy(buff, 0, newbuf, 0, offst*2);
                    }
                    buff = newbuf;
                }
                if (charsRead > 0) {
                    if (!TryReadPlpUnicodeCharsChunk(buff, offst, charsRead, stateObj, out charsRead)) {
                        return false;
                    }
                    charsLeft -= charsRead;
                    offst += charsRead;
                    totalCharsRead += charsRead;
                }
                // Special case single byte left
                if (stateObj._longlenleft == 1 && (charsLeft > 0)) {
                    byte b1;
                    if (!stateObj.TryReadByte(out b1)) {
                        return false;
                    }
                    stateObj._longlenleft--;
                    ulong ignored;
                    if (!stateObj.TryReadPlpLength(false, out ignored)) {
                        return false;
                    }
                    Debug.Assert((stateObj._longlenleft != 0), "ReadPlpUnicodeChars: Odd byte left at the end!");
                    byte b2;
                    if (!stateObj.TryReadByte(out b2)) {
                        return false;
                    }
                    stateObj._longlenleft--;
                    // Put it at the end of the array. At this point we know we have an extra byte.
                    buff[offst] = (char)(((b2 & 0xff) << 8) + (b1 & 0xff));
                    offst = checked((int)offst + 1);
                    charsRead++;
                    charsLeft--;
                    totalCharsRead++;
                }
                if (stateObj._longlenleft == 0) { // Read the next chunk or cleanup state if hit the end
                    ulong ignored;
                    if (!stateObj.TryReadPlpLength(false, out ignored)) {
                        return false;
                    }
                }

                if (stateObj._longlenleft == 0)   // Data read complete
                    break;
            }
            return true;
        }

        internal int ReadPlpAnsiChars(ref char[] buff, int offst, int len, SqlMetaDataPriv metadata, TdsParserStateObject stateObj) {
            int charsRead = 0;
            int charsLeft = 0;
            int bytesRead = 0;
            int totalcharsRead = 0;

            if (stateObj._longlen == 0) {
                Debug.Assert(stateObj._longlenleft == 0);
                return 0;       // No data
            }

            Debug.Assert(((ulong)stateObj._longlen != TdsEnums.SQL_PLP_NULL),
                    "Out of sync plp read request");

            Debug.Assert((buff == null && offst == 0) || (buff.Length >= offst + len), "Invalid length sent to ReadPlpAnsiChars()!");
            charsLeft = len;

            if (stateObj._longlenleft == 0) {
                stateObj.ReadPlpLength(false);
                if (stateObj._longlenleft == 0)  {// Data read complete
                    stateObj._plpdecoder = null;
                    return 0;
                }
            }

            if (stateObj._plpdecoder == null) {
                Encoding enc = metadata.encoding;

                if (enc == null)
                {
                    if (null == _defaultEncoding) {
                        ThrowUnsupportedCollationEncountered(stateObj);

                    }

                    enc = _defaultEncoding;
                }
                stateObj._plpdecoder = enc.GetDecoder();
            }

            while (charsLeft > 0) {
                bytesRead = (int)Math.Min(stateObj._longlenleft, (ulong)charsLeft);
                if ((stateObj._bTmp == null) || (stateObj._bTmp.Length < bytesRead)) {
                    // Grow the array
                    stateObj._bTmp = new byte[bytesRead];
                }

                bytesRead = stateObj.ReadPlpBytesChunk(stateObj._bTmp, 0, bytesRead);

                charsRead = stateObj._plpdecoder.GetChars(stateObj._bTmp, 0, bytesRead, buff, offst);
                charsLeft -= charsRead;
                offst += charsRead;
                totalcharsRead += charsRead;
                if (stateObj._longlenleft == 0)  // Read the next chunk or cleanup state if hit the end
                    stateObj.ReadPlpLength(false);

                if (stateObj._longlenleft == 0)  { // Data read complete
                    stateObj._plpdecoder = null;
                    break;
                }
            }
            return (totalcharsRead);
        }

        // ensure value is not null and does not have an NBC bit set for it before using this method
        internal ulong SkipPlpValue(ulong cb, TdsParserStateObject stateObj) {
            ulong skipped;
            Debug.Assert(stateObj._syncOverAsync, "Should not attempt pends in a synchronous call");
            bool result = TrySkipPlpValue(cb, stateObj, out skipped);
            if (!result) { throw SQL.SynchronousCallMayNotPend(); }
            return skipped;
        }

        internal bool TrySkipPlpValue(ulong cb, TdsParserStateObject stateObj, out ulong totalBytesSkipped) {
            // Read and skip cb bytes or until  ReadPlpLength returns 0.
            int bytesSkipped;
            totalBytesSkipped = 0;

            if (stateObj._longlenleft == 0) {
                ulong ignored;
                if (!stateObj.TryReadPlpLength(false, out ignored)) {
                    return false;
                }
            }

            while ((totalBytesSkipped < cb) &&
                    (stateObj._longlenleft > 0)) {
                if (stateObj._longlenleft > Int32.MaxValue)
                    bytesSkipped = Int32.MaxValue;
                else
                    bytesSkipped = (int)stateObj._longlenleft;
                bytesSkipped = ((cb - totalBytesSkipped) < (ulong) bytesSkipped) ? (int)(cb - totalBytesSkipped) : bytesSkipped;

                if (!stateObj.TrySkipBytes(bytesSkipped)) {
                    return false;
                }
                stateObj._longlenleft -= (ulong)bytesSkipped;
                totalBytesSkipped += (ulong)bytesSkipped;

                if (stateObj._longlenleft == 0) {
                    ulong ignored;
                    if (!stateObj.TryReadPlpLength(false, out ignored)) {
                        return false;
                    }
                }
            }

            return true;
        }

        internal ulong PlpBytesLeft(TdsParserStateObject stateObj) {
            if ((stateObj._longlen != 0) && (stateObj._longlenleft == 0))
                stateObj.ReadPlpLength(false);

            return stateObj._longlenleft;
        }

        internal bool TryPlpBytesLeft(TdsParserStateObject stateObj, out ulong left) {
            if ((stateObj._longlen != 0) && (stateObj._longlenleft == 0)) {
                if (!stateObj.TryReadPlpLength(false, out left)) {
                    return false;
                }
            }

            left = stateObj._longlenleft;
            return true;
        }

        private const ulong _indeterminateSize = 0xffffffffffffffff;        // Represents unknown size

        internal ulong PlpBytesTotalLength(TdsParserStateObject stateObj) {
            if (stateObj._longlen == TdsEnums.SQL_PLP_UNKNOWNLEN)
                return _indeterminateSize;
            else if (stateObj._longlen == TdsEnums.SQL_PLP_NULL)
                return 0;

            return stateObj._longlen;
        }

        const string StateTraceFormatString = "\n\t"
                                        + "         _physicalStateObj = {0}\n\t"
                                        + "         _pMarsPhysicalConObj = {1}\n\t"
                                        + "         _state = {2}\n\t"
                                        + "         _server = {3}\n\t"
                                        + "         _fResetConnection = {4}\n\t"
                                        + "         _defaultCollation = {5}\n\t"
                                        + "         _defaultCodePage = {6}\n\t"
                                        + "         _defaultLCID = {7}\n\t"
                                        + "         _defaultEncoding = {8}\n\t"
                                        + "         _encryptionOption = {10}\n\t"
                                        + "         _currentTransaction = {11}\n\t"
                                        + "         _pendingTransaction = {12}\n\t"
                                        + "         _retainedTransactionId = {13}\n\t"
                                        + "         _nonTransactedOpenResultCount = {14}\n\t"
                                        + "         _connHandler = {15}\n\t"
                                        + "         _fMARS = {16}\n\t"
                                        + "         _sessionPool = {17}\n\t"
                                        + "         _isShiloh = {18}\n\t"
                                        + "         _isShilohSP1 = {19}\n\t"
                                        + "         _isYukon = {20}\n\t"
                                        + "         _sniSpnBuffer = {21}\n\t"
                                        + "         _errors = {22}\n\t"
                                        + "         _warnings = {23}\n\t"
                                        + "         _attentionErrors = {24}\n\t"
                                        + "         _attentionWarnings = {25}\n\t"
                                        + "         _statistics = {26}\n\t"
                                        + "         _statisticsIsInTransaction = {27}\n\t"
                                        + "         _fPreserveTransaction = {28}"
                                        + "         _fParallel = {29}"
                                        ;
        internal string TraceString() {
            return String.Format(/*IFormatProvider*/ null,
                            StateTraceFormatString,
                            null == _physicalStateObj,
                            null == _pMarsPhysicalConObj,
                            _state,
                            _server,
                            _fResetConnection,
                            null == _defaultCollation ? "(null)" : _defaultCollation.TraceString(),
                            _defaultCodePage,
                            _defaultLCID,
                            TraceObjectClass(_defaultEncoding),
                            "",
                            _encryptionOption,
                            null == _currentTransaction ? "(null)" : _currentTransaction.TraceString(),
                            null == _pendingTransaction ? "(null)" : _pendingTransaction.TraceString(),
                            _retainedTransactionId,
                            _nonTransactedOpenResultCount,
                            null == _connHandler ? "(null)" : _connHandler.ObjectID.ToString((IFormatProvider)null),
                            _fMARS,
                            null == _sessionPool ? "(null)" : _sessionPool.TraceString(),
                            _isShiloh,
                            _isShilohSP1,
                            _isYukon,
                            null == _sniSpnBuffer ? "(null)" : _sniSpnBuffer.Length.ToString((IFormatProvider)null),
                            _physicalStateObj != null ? "(null)" : _physicalStateObj.ErrorCount.ToString((IFormatProvider)null),
                            _physicalStateObj != null ? "(null)" : _physicalStateObj.WarningCount.ToString((IFormatProvider)null),
                            _physicalStateObj != null ? "(null)" : _physicalStateObj.PreAttentionErrorCount.ToString((IFormatProvider)null),
                            _physicalStateObj != null ? "(null)" : _physicalStateObj.PreAttentionWarningCount.ToString((IFormatProvider)null),
                            null == _statistics,
                            _statisticsIsInTransaction,
                            _fPreserveTransaction,
                            null == _connHandler ? "(null)" : _connHandler.ConnectionOptions.MultiSubnetFailover.ToString((IFormatProvider)null),
                            null == _connHandler ? "(null)" : _connHandler.ConnectionOptions.TransparentNetworkIPResolution.ToString((IFormatProvider)null));
        }

        private string TraceObjectClass(object instance) {
            if (null == instance) {
                return "(null)";
            }
            else {
                return instance.GetType().ToString();
            }
        }
    }    // tdsparser
}//namespace
