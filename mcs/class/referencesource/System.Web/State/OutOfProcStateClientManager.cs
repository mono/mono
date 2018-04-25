//------------------------------------------------------------------------------
// <copyright file="OutOfProcSessionStateStore.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.SessionState {
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Security.Cryptography;
    using System.Web.Util;

    internal sealed class OutOfProcSessionStateStore : SessionStateStoreProviderBase {
        internal static readonly IntPtr INVALID_SOCKET = UnsafeNativeMethods.INVALID_HANDLE_VALUE;
        internal static readonly int    WHIDBEY_MAJOR_VERSION = 2;
        internal const int              STATE_NETWORK_TIMEOUT_DEFAULT = 10; // in sec

        static string       s_uribase;
        static int          s_networkTimeout;
        #pragma warning disable 0649
        static ReadWriteSpinLock            s_lock;
        #pragma warning restore 0649
        static bool         s_oneTimeInited;
        static StateServerPartitionInfo s_singlePartitionInfo;
        static PartitionManager s_partitionManager;
        static bool         s_usePartition;
        static EventHandler s_onAppDomainUnload;

        // We keep these info because we don't want to hold on to the config object.
        static string       s_configPartitionResolverType;
        static string       s_configStateConnectionString;
        static string       s_configStateConnectionStringFileName;
        static int          s_configStateConnectionStringLineNumber;
        static bool         s_configCompressionEnabled;

        // Per request info
        IPartitionResolver  _partitionResolver;
        StateServerPartitionInfo    _partitionInfo;


        internal override void Initialize(string name, NameValueCollection config, IPartitionResolver partitionResolver) {
            _partitionResolver = partitionResolver;
            Initialize(name, config);
        }

        public override void Initialize(string name, NameValueCollection config) {
            if (String.IsNullOrEmpty(name))
                name = "State Server Session State Provider";
            base.Initialize(name, config);

            if (!s_oneTimeInited) {
                s_lock.AcquireWriterLock();
                try {
                    if (!s_oneTimeInited) {
                        OneTimeInit();
                    }
                }
                finally {
                    s_lock.ReleaseWriterLock();
                }
            }

            if (!s_usePartition) {
                // For single partition, the connection info won't change from request to request
                Debug.Assert(s_partitionManager == null);
                _partitionInfo = s_singlePartitionInfo;
            }
        }

        void OneTimeInit() {
            SessionStateSection config = RuntimeConfig.GetAppConfig().SessionState;

            s_configPartitionResolverType = config.PartitionResolverType;
            s_configStateConnectionString = config.StateConnectionString;
            s_configStateConnectionStringFileName = config.ElementInformation.Properties["stateConnectionString"].Source;
            s_configStateConnectionStringLineNumber = config.ElementInformation.Properties["stateConnectionString"].LineNumber;
            s_configCompressionEnabled = config.CompressionEnabled;

            if (_partitionResolver == null) {
                String stateConnectionString = config.StateConnectionString;

                SessionStateModule.ReadConnectionString(config, ref stateConnectionString, "stateConnectionString");

                s_singlePartitionInfo = (StateServerPartitionInfo)CreatePartitionInfo(stateConnectionString);
            }
            else {
                s_usePartition = true;
                s_partitionManager = new PartitionManager(new CreatePartitionInfo(CreatePartitionInfo));
            }

            s_networkTimeout = (int)config.StateNetworkTimeout.TotalSeconds;

            string appId = HttpRuntime.AppDomainAppId;
            string idHash = Convert.ToBase64String(CryptoUtil.ComputeSHA256Hash(Encoding.UTF8.GetBytes(appId)));

            // Make sure that we have a absolute URI, some hosts(Cassini) don't provide this.
            if (appId.StartsWith("/", StringComparison.Ordinal)) {
                s_uribase = appId + "(" + idHash + ")/";
            }
            else {
                s_uribase = "/" + appId + "(" + idHash + ")/";
            }

            // We only need to do this in one instance
            s_onAppDomainUnload = new EventHandler(OnAppDomainUnload);
            Thread.GetDomain().DomainUnload += s_onAppDomainUnload;

            s_oneTimeInited = true;
        }

        void OnAppDomainUnload(Object unusedObject, EventArgs unusedEventArgs) {
            Debug.Trace("OutOfProcSessionStateStore", "OnAppDomainUnload called");

            Thread.GetDomain().DomainUnload -= s_onAppDomainUnload;

            if (_partitionResolver == null) {
                if (s_singlePartitionInfo != null) {
                    s_singlePartitionInfo.Dispose();
                }
            }
            else {
                if (s_partitionManager != null) {
                    s_partitionManager.Dispose();
                }
            }
        }

        internal IPartitionInfo CreatePartitionInfo(string stateConnectionString) {
            string  server;
            bool    serverIsIpv6NumericAddress;
            int     port;
            int     hr;

            try {
                ParseStateConnectionString(stateConnectionString, out server, out serverIsIpv6NumericAddress, out port);

                // At v1, we won't accept server name that has non-ascii characters
                for (int i = 0; i < server.Length; ++i) {
                    if (server[i] > 0x7F) {
                        throw new ArgumentException("stateConnectionString");
                    }
                }

            }
            catch {
                if (s_usePartition) {
                    throw new HttpException(
                           SR.GetString(SR.Error_parsing_state_server_partition_resolver_string, s_configPartitionResolverType));
                }
                else {
                    throw new ConfigurationErrorsException(
                            SR.GetString(SR.Invalid_value_for_sessionstate_stateConnectionString, s_configStateConnectionString),
                            s_configStateConnectionStringFileName, s_configStateConnectionStringLineNumber);
                }
            }

            hr = UnsafeNativeMethods.SessionNDConnectToService(server);
            if (hr != 0) {
                throw CreateConnectionException(server, port, hr);
            }

            return new StateServerPartitionInfo(
                new ResourcePool(new TimeSpan(0, 0, 5), int.MaxValue),
                server: server,
                serverIsIPv6NumericAddress: serverIsIpv6NumericAddress,
                port: port);

        }

        private static Regex _ipv6ConnectionStringFormat = new Regex(@"^\[(?<ipv6Address>.*)\]:(?<port>\d*)$");

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = @"The exception is never bubbled up to the user.")]
        internal static void ParseStateConnectionString(string stateConnectionString, out string server, out bool serverIsIPv6NumericAddress, out int port) {
            /*
            * stateConnection string has the following format:
            *
            *     "tcpip=<server>:<port>"
            *     "tcpip=[IPv6-address]:port", per RFC 3986, Sec. 3.2.2
            */

            // chop off the "tcpip=" part
            if (!stateConnectionString.StartsWith("tcpip=", StringComparison.Ordinal)) {
                throw new ArgumentException("stateConnectionString");
            }
            stateConnectionString = stateConnectionString.Substring("tcpip=".Length);

            // is this an IPv6 address?
            Match ipv6RegexMatch = _ipv6ConnectionStringFormat.Match(stateConnectionString);
            if (ipv6RegexMatch != null && ipv6RegexMatch.Success) {
                string ipv6AddressString = ipv6RegexMatch.Groups["ipv6Address"].Value;
                IPAddress ipv6Address = IPAddress.Parse(ipv6AddressString);
                if (ipv6Address.AddressFamily != AddressFamily.InterNetworkV6) {
                    throw new ArgumentException("stateConnectionString");
                }

                server = ipv6AddressString;
                serverIsIPv6NumericAddress = true;
                port = UInt16.Parse(ipv6RegexMatch.Groups["port"].Value, CultureInfo.InvariantCulture);
                return;
            }

            // not an IPv6 address; assume "host:port"
            string[] parts = stateConnectionString.Split(':');
            if (parts.Length != 2) {
                throw new ArgumentException("stateConnectionString");
            }
            server = parts[0];
            serverIsIPv6NumericAddress = false;
            port = UInt16.Parse(parts[1], CultureInfo.InvariantCulture);
        }

        internal static HttpException CreateConnectionException(string server, int port, int hr) {
            if (s_usePartition) {
                return new HttpException(
                        SR.GetString(SR.Cant_make_session_request_partition_resolver,
                                    s_configPartitionResolverType, server, port.ToString(CultureInfo.InvariantCulture)), hr);
            }
            else {
                return new HttpException(
                    SR.GetString(SR.Cant_make_session_request), hr);
            }
        }


        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback) {
            return false;
        }

        public override void Dispose() {
        }

        public override void InitializeRequest(HttpContext context) {
            if (s_usePartition) {
                // For multiple partition case, the connection info can change from request to request
                Debug.Assert(_partitionResolver != null);
                _partitionInfo = null;
            }
        }

        void MakeRequest(
                UnsafeNativeMethods.StateProtocolVerb   verb,
                String                                  id,
                UnsafeNativeMethods.StateProtocolExclusive    exclusiveAccess,
                int                                     extraFlags,
                int                                     timeout,
                int                                     lockCookie,
                byte[]                                  buf,
                int                                     cb,
                int                                     networkTimeout,
                out UnsafeNativeMethods.SessionNDMakeRequestResults results) {

            int                         hr;
            string                      uri;
            OutOfProcConnection         conn = null;
            HandleRef                   socketHandle;
            bool                        checkVersion = false;

            Debug.Assert(timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES, "item.Timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES");

            SessionIDManager.CheckIdLength(id, true /* throwOnFail */);

            if (_partitionInfo == null) {
                Debug.Assert(s_partitionManager != null);
                Debug.Assert(_partitionResolver != null);

                _partitionInfo = (StateServerPartitionInfo)s_partitionManager.GetPartition(_partitionResolver, id);

                // If its still null, we give up
                if (_partitionInfo == null) {
                    throw new HttpException(SR.GetString(SR.Bad_partition_resolver_connection_string, "PartitionManager"));
                }
            }

            // Need to make sure we dispose the connection if anything goes wrong
            try {
                conn = (OutOfProcConnection)_partitionInfo.RetrieveResource();
                if (conn != null) {
                    socketHandle = new HandleRef(this, conn._socketHandle.Handle);
                }
                else {
                    socketHandle = new HandleRef(this, INVALID_SOCKET);
                }

                if (_partitionInfo.StateServerVersion == -1) {
                    // We don't need locking here because it's okay to have two
                    // requests initializing s_stateServerVersion.
                    checkVersion = true;
                }

                Debug.Trace("OutOfProcSessionStateStoreMakeRequest",
                            "Calling MakeRequest, " +
                            "socket=" + (IntPtr)socketHandle.Handle +
                            "verb=" + verb +
                            " id=" + id +
                            " exclusiveAccess=" + exclusiveAccess +
                            " timeout=" + timeout +
                            " buf=" + ((buf != null) ? "non-null" : "null") +
                            " cb=" + cb +
                            " checkVersion=" + checkVersion +
                            " extraFlags=" + extraFlags);

                // Have to UrlEncode id because it may contain non-URL-safe characters
                uri = HttpUtility.UrlEncode(s_uribase + id);

                hr = UnsafeNativeMethods.SessionNDMakeRequest(
                        socketHandle, _partitionInfo.Server, _partitionInfo.Port, _partitionInfo.ServerIsIPv6NumericAddress /* forceIPv6 */, networkTimeout, verb, uri,
                        exclusiveAccess, extraFlags, timeout, lockCookie,
                        buf, cb, checkVersion, out results);

                Debug.Trace("OutOfProcSessionStateStoreMakeRequest", "MakeRequest returned: " +
                            "hr=" + hr +
                            " socket=" + (IntPtr)results.socket +
                            " httpstatus=" + results.httpStatus +
                            " timeout=" + results.timeout +
                            " contentlength=" + results.contentLength +
                            " uri=" + (IntPtr)results.content +
                            " lockCookie=" + results.lockCookie +
                            " lockDate=" + string.Format("{0:x}", results.lockDate) +
                            " lockAge=" + results.lockAge +
                            " stateServerMajVer=" + results.stateServerMajVer +
                            " actionFlags=" + results.actionFlags);

                if (conn != null) {
                    if (results.socket == INVALID_SOCKET) {
                        conn.Detach();
                        conn = null;
                    }
                    else if (results.socket != socketHandle.Handle) {
                        // The original socket is no good.  We've got a new one.
                        // Pleae note that EnsureConnected has closed the bad
                        // one already.
                        conn._socketHandle = new HandleRef(this, results.socket);
                    }
                }
                else if (results.socket != INVALID_SOCKET) {
                    conn = new OutOfProcConnection(results.socket);
                }

                if (conn != null) {
                    _partitionInfo.StoreResource(conn);
                }
            }
            catch {
                // We just need to dispose the connection if anything bad happened
                if (conn != null) {
                    conn.Dispose();
                }

                throw;
            }

            if (hr != 0) {
                HttpException e = CreateConnectionException(_partitionInfo.Server, _partitionInfo.Port, hr);

                string phase = null;

                switch (results.lastPhase) {
                case (int)UnsafeNativeMethods.SessionNDMakeRequestPhase.Initialization:
                    phase = SR.GetString(SR.State_Server_detailed_error_phase0);
                    break;

                case (int)UnsafeNativeMethods.SessionNDMakeRequestPhase.Connecting:
                    phase = SR.GetString(SR.State_Server_detailed_error_phase1);
                    break;

                case (int)UnsafeNativeMethods.SessionNDMakeRequestPhase.SendingRequest:
                    phase = SR.GetString(SR.State_Server_detailed_error_phase2);
                    break;

                case (int)UnsafeNativeMethods.SessionNDMakeRequestPhase.ReadingResponse:
                    phase = SR.GetString(SR.State_Server_detailed_error_phase3);
                    break;

                default:
                    Debug.Assert(false, "Unknown results.lastPhase: " + results.lastPhase);
                    break;
                }

                WebBaseEvent.RaiseSystemEvent(SR.GetString(SR.State_Server_detailed_error,
                            phase,
                            "0x" + hr.ToString("X08", CultureInfo.InvariantCulture),
                            cb.ToString(CultureInfo.InvariantCulture)),
                            this, WebEventCodes.WebErrorOtherError, WebEventCodes.StateServerConnectionError, e);

                throw e;
            }

            if (results.httpStatus == 400) {
                if (s_usePartition) {
                    throw new HttpException(
                        SR.GetString(SR.Bad_state_server_request_partition_resolver,
                                    s_configPartitionResolverType, _partitionInfo.Server, _partitionInfo.Port.ToString(CultureInfo.InvariantCulture)));
                }
                else {
                    throw new HttpException(
                        SR.GetString(SR.Bad_state_server_request));
                }
            }

            if (checkVersion) {
                _partitionInfo.StateServerVersion = results.stateServerMajVer;
                if (_partitionInfo.StateServerVersion < WHIDBEY_MAJOR_VERSION) {
                    // We won't work with versions lower than Whidbey
                    if (s_usePartition) {
                        throw new HttpException(
                            SR.GetString(SR.Need_v2_State_Server_partition_resolver,
                                        s_configPartitionResolverType, _partitionInfo.Server, _partitionInfo.Port.ToString(CultureInfo.InvariantCulture)));
                    }
                    else {
                        throw new HttpException(
                            SR.GetString(SR.Need_v2_State_Server));
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
        internal SessionStateStoreData DoGet(HttpContext context,
                                            String id,
                                            UnsafeNativeMethods.StateProtocolExclusive exclusiveAccess,
                                            out bool locked,
                                            out TimeSpan lockAge,
                                            out object lockId,
                                            out SessionStateActions actionFlags) {
            SessionStateStoreData   item = null;
            UnmanagedMemoryStream   stream = null;
            int                     contentLength;
            UnsafeNativeMethods.SessionNDMakeRequestResults results;

            // Set default return values
            locked = false;
            lockId = null;
            lockAge = TimeSpan.Zero;
            actionFlags = 0;
            results.content = IntPtr.Zero;

            try {
                MakeRequest(UnsafeNativeMethods.StateProtocolVerb.GET,
                            id, exclusiveAccess, 0, 0, 0,
                            null, 0, s_networkTimeout, out results);

                switch (results.httpStatus) {
                    case 200:
                        /* item found, deserialize it */
                        contentLength = results.contentLength;
                        if (contentLength > 0) {
                            try {
                                unsafe {
                                    stream = new UnmanagedMemoryStream((byte*)results.content, contentLength);
                                }
                                item = SessionStateUtility.DeserializeStoreData(context, stream, s_configCompressionEnabled);
                            }
                            finally {
                                if(stream != null) {
                                    stream.Close();
                                }
                            }

                            lockId = results.lockCookie;
                            actionFlags = (SessionStateActions) results.actionFlags;
                        }

                        break;

                    case 423:
                        /* state locked, return lock information */

                        if (0 <= results.lockAge) {
                            if (results.lockAge < Sec.ONE_YEAR) {
                                lockAge = new TimeSpan(0, 0, results.lockAge);
                            }
                            else {
                                lockAge = TimeSpan.Zero;
                            }
                        }
                        else {
                            DateTime now = DateTime.Now;
                            if (0 < results.lockDate && results.lockDate < now.Ticks) {
                                lockAge = now - new DateTime(results.lockDate);
                            }
                            else {
                                lockAge = TimeSpan.Zero;
                            }
                        }

                        locked = true;
                        lockId = results.lockCookie;

                        Debug.Assert((results.actionFlags & (int)SessionStateActions.InitializeItem) == 0,
                            "(results.actionFlags & (int)SessionStateActions.InitializeItem) == 0; uninitialized item cannot be locked");
                        break;
                }
            }
            finally {
                if (results.content != IntPtr.Zero) {
                    UnsafeNativeMethods.SessionNDFreeBody(new HandleRef(this, results.content));
                }
            }

            return item;
        }

        public override SessionStateStoreData  GetItem(HttpContext context,
                                                            String id,
                                                            out bool locked,
                                                            out TimeSpan lockAge,
                                                            out object lockId,
                                                            out SessionStateActions actionFlags) {
            Debug.Trace("OutOfProcSessionStateStore", "Calling Get, id=" + id);

            return DoGet(context, id, UnsafeNativeMethods.StateProtocolExclusive.NONE,
                        out locked, out lockAge, out lockId, out actionFlags);
        }


        public override SessionStateStoreData  GetItemExclusive(HttpContext context,
                                                String id,
                                                out bool locked,
                                                out TimeSpan lockAge,
                                                out object lockId,
                                                out SessionStateActions actionFlags) {
            Debug.Trace("OutOfProcSessionStateStore", "Calling GetExlusive, id=" + id);

            return DoGet(context, id, UnsafeNativeMethods.StateProtocolExclusive.ACQUIRE,
                        out locked, out lockAge, out lockId, out actionFlags);
        }

        public override void ReleaseItemExclusive(HttpContext context,
                                String id,
                                object lockId) {
            Debug.Assert(lockId != null, "lockId != null");

            UnsafeNativeMethods.SessionNDMakeRequestResults results;
            int lockCookie = (int)lockId;

            Debug.Trace("OutOfProcSessionStateStore", "Calling ReleaseExclusive, id=" + id);
            MakeRequest(UnsafeNativeMethods.StateProtocolVerb.GET, id,
                        UnsafeNativeMethods.StateProtocolExclusive.RELEASE, 0, 0,
                        lockCookie, null, 0, s_networkTimeout, out results);

        }

        public override void SetAndReleaseItemExclusive(HttpContext context,
                                    String id,
                                    SessionStateStoreData item,
                                    object lockId,
                                    bool newItem) {
            UnsafeNativeMethods.SessionNDMakeRequestResults results;
            byte[]          buf;
            int             length;
            int             lockCookie;

            Debug.Assert(item.Items != null, "item.Items != null");
            Debug.Assert(item.StaticObjects != null, "item.StaticObjects != null");

            Debug.Trace("OutOfProcSessionStateStore", "Calling Set, id=" + id + " sessionItems=" + item.Items + " timeout=" + item.Timeout);

            try {
                SessionStateUtility.SerializeStoreData(item, 0, out buf, out length, s_configCompressionEnabled);
            }
            catch {
                if (!newItem) {
                    ((SessionStateStoreProviderBase)this).ReleaseItemExclusive(context, id, lockId);
                }
                throw;
            }

            // Save it to the store

            if (lockId == null) {
                lockCookie = 0;
            }
            else {
                lockCookie = (int)lockId;
            }

            MakeRequest(UnsafeNativeMethods.StateProtocolVerb.PUT, id,
                        UnsafeNativeMethods.StateProtocolExclusive.NONE, 0, item.Timeout, lockCookie,
                        buf, length, s_networkTimeout, out results);
        }

        public override void RemoveItem(HttpContext context,
                                        String id,
                                        object lockId,
                                        SessionStateStoreData item) {
            Debug.Assert(lockId != null, "lockId != null");
            Debug.Trace("OutOfProcSessionStateStore", "Calling Remove, id=" + id);

            UnsafeNativeMethods.SessionNDMakeRequestResults results;
            int             lockCookie = (int)lockId;

            MakeRequest(UnsafeNativeMethods.StateProtocolVerb.DELETE, id,
                        UnsafeNativeMethods.StateProtocolExclusive.NONE, 0, 0, lockCookie,
                        null, 0, s_networkTimeout, out results);

        }

        public override void ResetItemTimeout(HttpContext context, String id) {
            UnsafeNativeMethods.SessionNDMakeRequestResults results;

            Debug.Trace("OutOfProcSessionStateStore", "Calling ResetTimeout, id=" + id);
            MakeRequest(UnsafeNativeMethods.StateProtocolVerb.HEAD, id,
                        UnsafeNativeMethods.StateProtocolExclusive.NONE, 0, 0, 0,
                        null, 0, s_networkTimeout, out results);
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            Debug.Assert(timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES, "item.Timeout <= SessionStateModule.MAX_CACHE_BASED_TIMEOUT_MINUTES");

            return SessionStateUtility.CreateLegitStoreData(context, null, null, timeout);
        }

        public override void CreateUninitializedItem(HttpContext context, String id, int timeout) {
            UnsafeNativeMethods.SessionNDMakeRequestResults results;
            byte[]          buf;
            int             length;

            Debug.Trace("OutOfProcSessionStateStore", "Calling CreateUninitializedItem, id=" + id + " timeout=" + timeout);

            // Create an empty item
            SessionStateUtility.SerializeStoreData(CreateNewStoreData(context, timeout), 0, out buf, out length, s_configCompressionEnabled);

            // Save it to the store
            MakeRequest(UnsafeNativeMethods.StateProtocolVerb.PUT, id,
                        UnsafeNativeMethods.StateProtocolExclusive.NONE,
                        (int)SessionStateItemFlags.Uninitialized, timeout, 0,
                        buf, length, s_networkTimeout, out results);
        }

        // Called during EndRequest event
        public override void EndRequest(HttpContext context) {
        }

        class StateServerPartitionInfo : PartitionInfo {
            string  _server;
            bool    _serverIsIPv6NumericAddress;
            int     _port;
            int     _stateServerVersion;

            internal StateServerPartitionInfo(ResourcePool rpool, string server, bool serverIsIPv6NumericAddress, int port) : base(rpool) {
                _server = server;
                _serverIsIPv6NumericAddress = serverIsIPv6NumericAddress;
                _port = port;
                _stateServerVersion = -1;
                Debug.Trace("PartitionInfo", "Created a new info, server=" + server + ", port=" + port);
            }

            internal string Server {
                get { return _server; }
            }

            internal bool ServerIsIPv6NumericAddress {
                get { return _serverIsIPv6NumericAddress; }
            }

            internal int Port {
                get { return _port; }
            }

            internal int StateServerVersion {
                get { return _stateServerVersion; }
                set { _stateServerVersion = value; }
            }

            protected override string TracingPartitionString {
                get {
                    // only add the brackets if the server is an IPv6 address, per the URI specification
                    string formatString = (ServerIsIPv6NumericAddress) ? "[{0}]:{1}" : "{0}:{1}";
                    return String.Format(CultureInfo.InvariantCulture, formatString, Server, Port);
                }
            }
        }


        class OutOfProcConnection : IDisposable {
            internal HandleRef _socketHandle;

            internal OutOfProcConnection(IntPtr socket) {
                Debug.Assert(socket != OutOfProcSessionStateStore.INVALID_SOCKET,
                             "socket != OutOfProcSessionStateStore.INVALID_SOCKET");

                _socketHandle = new HandleRef(this, socket);
                PerfCounters.IncrementCounter(AppPerfCounter.SESSION_STATE_SERVER_CONNECTIONS);
            }

            ~OutOfProcConnection() {
                Dispose(false);
            }

            public void Dispose() {
                Debug.Trace("ResourcePool", "Disposing OutOfProcConnection");

                Dispose(true);
                System.GC.SuppressFinalize(this);
            }

            private void Dispose(bool dummy) {
                if (_socketHandle.Handle != OutOfProcSessionStateStore.INVALID_SOCKET) {
                    UnsafeNativeMethods.SessionNDCloseConnection(_socketHandle);
                    _socketHandle = new HandleRef(this, OutOfProcSessionStateStore.INVALID_SOCKET);
                    PerfCounters.DecrementCounter(AppPerfCounter.SESSION_STATE_SERVER_CONNECTIONS);
                }
            }

            internal void Detach() {
                _socketHandle = new HandleRef(this, OutOfProcSessionStateStore.INVALID_SOCKET);
            }
        }
    }
}
