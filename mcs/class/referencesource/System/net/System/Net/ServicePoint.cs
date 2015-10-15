//------------------------------------------------------------------------------
// <copyright file="ServicePoint.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Net.Sockets;
    using System.Collections;
    using System.IO;
    using System.Threading;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Net.Security;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public delegate IPEndPoint BindIPEndPoint(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount);


    // ServicePoints are never created directly but always handed out by the
    // ServicePointManager. The ServicePointManager and the ServicePoints must be in
    // the same name space so that the ServicePointManager can call the
    // internal constructor

    /// <devdoc>
    ///    <para>Provides connection management for other classes.</para>
    /// </devdoc>
    [FriendAccessAllowed]
    public class ServicePoint {

        internal const int LoopbackConnectionLimit = Int32.MaxValue;

        private int                 m_ConnectionLeaseTimeout;
        private TimerThread.Queue   m_ConnectionLeaseTimerQueue;
        private bool                m_ProxyServicePoint;
        private bool                m_UserChangedLimit;
        private bool                m_UseNagleAlgorithm;
        private TriState            m_HostLoopbackGuess;
        private int                 m_ReceiveBufferSize;
        private bool                m_Expect100Continue;
        private bool                m_Understands100Continue;
        private HttpBehaviour       m_HttpBehaviour;
        private string              m_LookupString;
        private int                 m_ConnectionLimit;
        private Hashtable           m_ConnectionGroupList;
        private Uri                 m_Address;
        private string              m_Host;
        private int                 m_Port;
        private TimerThread.Queue   m_IdlingQueue;
        private TimerThread.Timer   m_ExpiringTimer;
        private DateTime            m_IdleSince;
        private string              m_ConnectionName;
        private int                 m_CurrentConnections;
        private bool                m_HostMode;
        private BindIPEndPoint      m_BindIPEndPointDelegate = null;
        private object              m_CachedChannelBinding;

        private static readonly AsyncCallback m_ConnectCallbackDelegate = new AsyncCallback(ConnectSocketCallback);

        private readonly TimerThread.Callback m_IdleConnectionGroupTimeoutDelegate;

#if !FEATURE_PAL
        private object m_ServerCertificateOrBytes;
        private object m_ClientCertificateOrBytes;
#endif // !FEATURE_PAL

        private bool m_UseTcpKeepAlive = false;
        private int m_TcpKeepAliveTime;
        private int m_TcpKeepAliveInterval;

        internal string LookupString {
            get {
                return m_LookupString;
            }
        }

        internal string Hostname {
            get {
                return m_HostName;
            }
        }

        internal bool IsTrustedHost {
            get {
                return m_IsTrustedHost;
            }
        }

        public BindIPEndPoint BindIPEndPointDelegate {
            get {
                return m_BindIPEndPointDelegate;
            }
            set {
                ExceptionHelper.InfrastructurePermission.Demand();
                m_BindIPEndPointDelegate = value;
            }
        }

        //
        // constructors
        //
        internal ServicePoint(Uri address, TimerThread.Queue defaultIdlingQueue, int defaultConnectionLimit, string lookupString, bool userChangedLimit, bool proxyServicePoint) {
            GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::.ctor(" + lookupString+")");
            if (Logging.On) Logging.Enter(Logging.Web, this, "ServicePoint", address.DnsSafeHost + ":" + address.Port);

            m_ProxyServicePoint     = proxyServicePoint;
            m_Address               = address;
            m_ConnectionName        = address.Scheme;
            m_Host                  = address.DnsSafeHost;
            m_Port                  = address.Port;
            m_IdlingQueue           = defaultIdlingQueue;
            m_ConnectionLimit       = defaultConnectionLimit;
            m_HostLoopbackGuess     = TriState.Unspecified;
            m_LookupString          = lookupString;
            m_UserChangedLimit      = userChangedLimit;
            m_UseNagleAlgorithm     = ServicePointManager.UseNagleAlgorithm;
            m_Expect100Continue     = ServicePointManager.Expect100Continue;
            m_ConnectionGroupList   = new Hashtable(10);
            m_ConnectionLeaseTimeout = System.Threading.Timeout.Infinite;
            m_ReceiveBufferSize     = -1;
            m_UseTcpKeepAlive       = ServicePointManager.s_UseTcpKeepAlive;
            m_TcpKeepAliveTime      = ServicePointManager.s_TcpKeepAliveTime;
            m_TcpKeepAliveInterval  = ServicePointManager.s_TcpKeepAliveInterval;

            // it would be safer to make sure the server is 1.1
            // but assume it is at the beginning, and update it later
            m_Understands100Continue = true;
            m_HttpBehaviour         = HttpBehaviour.Unknown;

            // upon creation, the service point should be idle, by default
            m_IdleSince             = DateTime.Now;
            m_ExpiringTimer         = m_IdlingQueue.CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
            m_IdleConnectionGroupTimeoutDelegate = new TimerThread.Callback(IdleConnectionGroupTimeoutCallback);
        }



        internal ServicePoint(string host, int port, TimerThread.Queue defaultIdlingQueue, int defaultConnectionLimit, string lookupString, bool userChangedLimit, bool proxyServicePoint) {
            GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::.ctor(" + lookupString+")");
            if (Logging.On) Logging.Enter(Logging.Web, this, "ServicePoint", host + ":" + port);
            
            m_ProxyServicePoint     = proxyServicePoint;
            m_ConnectionName        = "ByHost:"+host+":"+port.ToString(CultureInfo.InvariantCulture);
            m_IdlingQueue           = defaultIdlingQueue;
            m_ConnectionLimit       = defaultConnectionLimit;
            m_HostLoopbackGuess     = TriState.Unspecified;
            m_LookupString          = lookupString;
            m_UserChangedLimit      = userChangedLimit;
            m_ConnectionGroupList   = new Hashtable(10);
            m_ConnectionLeaseTimeout = System.Threading.Timeout.Infinite;
            m_ReceiveBufferSize     = -1;
            m_Host = host;
            m_Port = port;
            m_HostMode = true;

            // upon creation, the service point should be idle, by default
            m_IdleSince             = DateTime.Now;
            m_ExpiringTimer         = m_IdlingQueue.CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
            m_IdleConnectionGroupTimeoutDelegate = new TimerThread.Callback(IdleConnectionGroupTimeoutCallback);
        }



        // methods

        internal object CachedChannelBinding
        {
            get { return m_CachedChannelBinding; }
        }

        internal void SetCachedChannelBinding(Uri uri, ChannelBinding binding)
        {
            if (uri.Scheme == Uri.UriSchemeHttps)
            {
                m_CachedChannelBinding = (binding != null ? (object)binding : (object)DBNull.Value);
            }
        }

        /*++

            FindConnectionGroup       -

            Searches for the a Group object that actually holds the connections
              that we want to peak at.


            Input:
                    request                 - Request that's being submitted.
                    connName                - Connection Name if needed

            Returns:
                    ConnectionGroup

        --*/

        private ConnectionGroup FindConnectionGroup(string connName, bool dontCreate) {
            string lookupStr = ConnectionGroup.MakeQueryStr(connName);

            GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::FindConnectionGroup() lookupStr:[" + ValidationHelper.ToString(connName) + "]");

            ConnectionGroup entry = m_ConnectionGroupList[lookupStr] as ConnectionGroup;

            if (entry==null && !dontCreate) {
                entry = new ConnectionGroup(this, connName);
                GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::FindConnectionGroup() adding ConnectionGroup lookupStr:[" + lookupStr + "]");

                m_ConnectionGroupList[lookupStr] = entry;
            }
            else {
                GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::FindConnectionGroup() using existing ConnectionGroup");
            }
            GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::FindConnectionGroup() returning ConnectionGroup:" + ValidationHelper.ToString(entry) + (entry!=null ? " ConnLimit:" + entry.ConnectionLimit.ToString() : ""));
            return entry;
        }


        /// <devdoc>
        ///    <para>
        ///     Tempory for getting a new Connection for FTP client, for the time being
        ///    </para>
        /// </devdoc>
        internal Socket GetConnection(PooledStream PooledStream, object owner, bool async, out IPAddress address, ref Socket abortSocket, ref Socket abortSocket6)
        {
            Socket socket = null;
            Socket socket6 = null;
            Socket finalSocket = null;
            Exception innerException = null;
            WebExceptionStatus ws = WebExceptionStatus.ConnectFailure;
            address = null;

            //
            // if we will not create a tunnel through a proxy then create
            // and connect the socket we will use for the connection
            //

            //
            // IPv6 Support: If IPv6 is enabled, then we create a second socket that ServicePoint
            //               will use if it wants to connect via IPv6.
            //
            if ( Socket.OSSupportsIPv4 ) {
                socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            }

            if ( Socket.OSSupportsIPv6 ) {
                socket6 = new Socket(AddressFamily.InterNetworkV6,SocketType.Stream,ProtocolType.Tcp);
            }

            abortSocket = socket;
            abortSocket6 = socket6;

            //
            // Setup socket timeouts for sync requests
            //
            // 

            ConnectSocketState state = null;

            if (async) {
                state = new ConnectSocketState(this, PooledStream, owner, socket, socket6);
            }

            ws = ConnectSocket(socket, socket6, ref finalSocket, ref address, state, out innerException);

            if (ws == WebExceptionStatus.Pending) {
                return null;
            }

            if (ws != WebExceptionStatus.Success) {
                throw new WebException(
                    NetRes.GetWebStatusString(ws),
                    ws == WebExceptionStatus.ProxyNameResolutionFailure || ws == WebExceptionStatus.NameResolutionFailure ? Host : null,
                    innerException,
                    ws,
                    null, /* no response */
                    WebExceptionInternalStatus.ServicePointFatal);
            }

            //
            // There should be no means for socket to be null at this
            // point, but the damage is greater if we just carry on
            // without ensuring that it's good.
            //
            if ( finalSocket == null ) {
                throw new IOException(SR.GetString(SR.net_io_transportfailure));
            }

            CompleteGetConnection(socket, socket6, finalSocket, address);
            return finalSocket;
        }

        /// <devdoc>
        ///    <para>
        ///     Complete the GetConnection(...) call, the function was divided for async completion
        ///    </para>
        /// </devdoc>
        private void CompleteGetConnection(Socket socket, Socket socket6, Socket finalSocket, IPAddress address) {
            //
            // Decide which socket to retain
            //
            if ( finalSocket.AddressFamily == AddressFamily.InterNetwork ) {
                if ( socket6 != null ) {
                    socket6.Close();
                    socket6 = null;
                }
            }
            else {
                if (socket != null) {
                    socket.Close();
                    socket = null;
                }
            }

            // make this configurable from the user:
            if (!UseNagleAlgorithm) {
                finalSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
            }
            if (ReceiveBufferSize != -1) {
                finalSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, ReceiveBufferSize);
            }
            if (m_UseTcpKeepAlive) {
                
                // Marshal a byte array containing the params for WsaIoctl
                // struct tcp_keepalive {
                //    u_long  onoff;
                //    u_long  keepalivetime;
                //    u_long  keepaliveinterval;
                // };
                byte[] input = new byte[sizeof(int)*3];
                input[0]  = 1; 
                input[4]  = (byte)  (m_TcpKeepAliveTime & 0xff);
                input[5]  = (byte) ((m_TcpKeepAliveTime >>  8) & 0xff);
                input[6]  = (byte) ((m_TcpKeepAliveTime >> 16) & 0xff);
                input[7]  = (byte) ((m_TcpKeepAliveTime >> 24) & 0xff);
                input[8]  = (byte)  (m_TcpKeepAliveInterval & 0xff);
                input[9]  = (byte) ((m_TcpKeepAliveInterval >>  8) & 0xff);
                input[10] = (byte) ((m_TcpKeepAliveInterval >> 16) & 0xff);
                input[11] = (byte) ((m_TcpKeepAliveInterval >> 24) & 0xff);

                // do WSAIoctl call
                finalSocket.IOControl(
                                IOControlCode.KeepAliveValues,
                                input,
                                null);
            }
            
            
            //return CreateConnection(NetworkStream stream, IPAddress address);
            //return new NetworkStream(finalSocket, true);
        }


        /*++

            SubmitRequest       - Submit a request for sending.

            The service point submit handler. This is called when a request needs
            to be submitted to the network. This routine is asynchronous; the caller
            passes in an HttpSubmitDelegate that is invoked when the caller
            can use the underlying network. The delegate is invoked with the
            stream that it can write to.


            In this version, we use HttpWebRequest. In the future we use IRequest

            Input:
                    Request                 - Request that's being submitted.
                    SubmitDelegate          - Delegate to be invoked.

            Returns:
                    Nothing.

        --*/

        internal virtual void SubmitRequest(HttpWebRequest request) {
            SubmitRequest(request, null);
        }

        // userReqeustThread says whether we can post IO from this thread or not.
        internal void SubmitRequest(HttpWebRequest request, string connName)
        {
            //
            // We attempt to locate a free connection sitting on our list
            //  avoiding multiple loops of the same the list.
            //  We do this, by enumerating the list of the connections,
            //   looking for Free items, and the least busy item
            //
            Connection connToUse;
            ConnectionGroup connGroup;
            bool forcedsubmit = false;
            lock(this) {
                GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::SubmitRequest() Finding ConnectionGroup:[" + connName + "]");
                connGroup = FindConnectionGroup(connName, false);
                GlobalLog.Assert(connGroup != null, "ServicePoint#{0}::SubmitRequest()|connGroup == null", ValidationHelper.HashString(this));
            }

            do {
                connToUse = connGroup.FindConnection(request, connName, out forcedsubmit);
                // The request could be already aborted
                if (connToUse == null)
                    return;

                GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::SubmitRequest() Using Connection#" + ValidationHelper.HashString(connToUse));
                // finally sumbit delegate
                if (connToUse.SubmitRequest(request, forcedsubmit)) {
                    break;
                }
            } while (true);
        }

        // properties

        /// <devdoc>
        ///    <para>
        ///       Gets and sets timeout for when connections should be recycled.
        ///    </para>
        /// </devdoc>
        public int ConnectionLeaseTimeout {
            get {
                return m_ConnectionLeaseTimeout;
            }
            set {
                if ( !ValidationHelper.ValidateRange(value, Timeout.Infinite, Int32.MaxValue)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (value != m_ConnectionLeaseTimeout) {
                    m_ConnectionLeaseTimeout = value;
                    m_ConnectionLeaseTimerQueue = null;
                }
            }
        }

        /// <summary>
        /// <para>Returns a timer queue that can be used internally to create timers of
        /// ConnectionLeaseTimeout duration.</para>
        /// </summary>
        internal TimerThread.Queue ConnectionLeaseTimerQueue {
            get {
                TimerThread.Queue queue = m_ConnectionLeaseTimerQueue;
                if (queue == null) {
                    queue = TimerThread.GetOrCreateQueue(ConnectionLeaseTimeout);
                    m_ConnectionLeaseTimerQueue = queue;
                }
                return m_ConnectionLeaseTimerQueue;
            }
        }

        // Only the scheme and hostport, for example http://www.microsoft.com
        /// <devdoc>
        ///    <para>
        ///       Gets the Uniform Resource Identifier of the <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public Uri Address {
            get {
                if(m_HostMode){
                    throw new NotSupportedException(SR.GetString(SR.net_servicePointAddressNotSupportedInHostMode));
                }

                // Don't let low-trust apps discover the proxy information.
                if (m_ProxyServicePoint)
                {
                    ExceptionHelper.WebPermissionUnrestricted.Demand();
                }

                return m_Address;
            }
        }

        internal Uri InternalAddress
        {
            get
            {
                GlobalLog.Assert(!m_HostMode, "ServicePoint#{0}::InternalAddress|Can't be used in Host Mode.", ValidationHelper.HashString(this));
                return m_Address;
            }
        }

        internal string Host {
            get {
                if(m_HostMode){
                    return m_Host;
                }
                return m_Address.Host;
            }
        }

        internal int Port {
            get {
                return m_Port;
            }
        }


        //
        // Gets or sets the maximum idle time allowed for connections of this ServicePoint and then for ServicePoint itself
        // Default value coming in ctor is ServicePointManager.s_MaxServicePointIdleTime which 100 sec
        //
        public int MaxIdleTime {
            get {
                return m_IdlingQueue.Duration;
            }
            set {
                if ( !ValidationHelper.ValidateRange(value, Timeout.Infinite, Int32.MaxValue)) {
                    throw new ArgumentOutOfRangeException("value");
                }

                // Already set?
                if (value == m_IdlingQueue.Duration)
                    return;

                lock(this) {
                    // Make sure we can cancel the existing one.  If not, we already idled out.
                    if (m_ExpiringTimer == null || m_ExpiringTimer.Cancel())
                    {
                        m_IdlingQueue = TimerThread.GetOrCreateQueue(value);
                        if (m_ExpiringTimer != null)
                        {
                            // Need to create a one-off timer for the remaining period.
                            double elapsedDouble = (DateTime.Now - m_IdleSince).TotalMilliseconds;
                            int elapsed = elapsedDouble >= (double) Int32.MaxValue ? Int32.MaxValue : (int) elapsedDouble;
                            int timeLeft = value == Timeout.Infinite ? Timeout.Infinite : elapsed >= value ? 0 : value - elapsed;
                            m_ExpiringTimer = TimerThread.CreateQueue(timeLeft).CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
                        }
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the Nagling algorithm on the connections that are created to this <see cref='System.Net.ServicePoint'/>.
        ///       Changing this value does not affect existing connections but only to new ones that are created from that moment on.
        ///    </para>
        /// </devdoc>
        public bool UseNagleAlgorithm {
            get {
                return m_UseNagleAlgorithm;
            }
            set {
                m_UseNagleAlgorithm = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets and sets the socket's receive buffer size.
        ///    </para>
        /// </devdoc>
        public int ReceiveBufferSize {
            get {
                return m_ReceiveBufferSize;
            }
            set {
                if ( !ValidationHelper.ValidateRange(value, -1, Int32.MaxValue)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_ReceiveBufferSize = value;
            }

        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets indication whether 100-continue behaviour is desired when using this <see cref='System.Net.ServicePoint'/>.
        ///       Changing this value does not affect existing connections but only to new ones that are created from that moment on.
        ///    </para>
        /// </devdoc>
        public bool Expect100Continue {
            set {
                m_Expect100Continue = value;
            }
            get {
                return m_Expect100Continue;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the date/time that the <see cref='System.Net.ServicePoint'/> went idle.
        ///    </para>
        /// </devdoc>
        public DateTime IdleSince {
            get {
                return m_IdleSince;
            }
        }

        // HTTP Server Version
        /// <devdoc>
        ///    <para>
        ///       The version of the protocol being used on this <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public virtual Version ProtocolVersion {
            get {
                return (m_HttpBehaviour>HttpBehaviour.HTTP10 || m_HttpBehaviour == HttpBehaviour.Unknown) ? HttpVersion.Version11 : HttpVersion.Version10;
            }
        }

        // Contains set accessor for Version property. Version is a read-only
        // property at the API
        internal HttpBehaviour HttpBehaviour {
            get {
                return m_HttpBehaviour;
            }
            set {
                m_HttpBehaviour = value;
                //
                // if version is greater than HTTP/1.1, and server undesrtood
                // 100 Continue so far, keep expecting it.
                //
                m_Understands100Continue = m_Understands100Continue && (m_HttpBehaviour>HttpBehaviour.HTTP10 || m_HttpBehaviour == HttpBehaviour.Unknown);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the connection name established by the <see cref='System.Net.WebRequest'/> that created the connection.
        ///    </para>
        /// </devdoc>
        public string ConnectionName {
            get {
                return m_ConnectionName;
            }
        }

        /*
        /// <devdoc>
        ///    Gets the connection mode in use by the <see cref='System.Net.ServicePoint'/>. One of the <see cref='System.Net.ConnectionModes'/>
        ///    values.
        /// </devdoc>
        internal ConnectionModes ConnectionMode {
            get {
                return m_HttpBehaviour>=HttpBehaviour.HTTP11 ? ConnectionModes.Pipeline : ConnectionModes.Persistent;
            }
        }
        */

        /// <devdoc>
        ///     Removes the specified Connection group from the ServicePoint, destroys safe and unsafe groups, but not internal.
        /// </devdoc>

        public bool CloseConnectionGroup(string connectionGroupName) {
            GlobalLog.Enter("ServicePoint#" + ValidationHelper.HashString(this) + "::CloseConnectionGroup() lookupStr:[" + connectionGroupName + "]");
            if ( ReleaseConnectionGroup(HttpWebRequest.GenerateConnectionGroup(connectionGroupName, false, false).ToString())  ||
                 ReleaseConnectionGroup(HttpWebRequest.GenerateConnectionGroup(connectionGroupName, true, false).ToString())  ||
                 ConnectionPoolManager.RemoveConnectionPool(this, connectionGroupName)) {

                GlobalLog.Leave("ServicePoint#" + ValidationHelper.HashString(this) + "::CloseConnectionGroup()","true");
                return true;
            }
            GlobalLog.Leave("ServicePoint#" + ValidationHelper.HashString(this) + "::CloseConnectionGroup()","false");
            return false;
        }

        internal void CloseConnectionGroupInternal(string connectionGroupName) {
            
            // Release all internal connection groups (both 'safe' and 'unsafe') with the given name. We're not 
            // interested in the result value (it's OK if it is 'false', i.e. not found).
            // We don't need to call ConnectionPoolManager.RemoveConnectionPool() since this method is only used for
            // HTTP.
            string connectionGroupPrefixSafe = HttpWebRequest.GenerateConnectionGroup(connectionGroupName, false, true).ToString();
            string connectionGroupPrefixUnsafe = HttpWebRequest.GenerateConnectionGroup(connectionGroupName, true, true).ToString();
            List<string> connectionGroupNames = null;

            lock (this) {                
                // Find all connecion groups starting with the provided prefix. We just compare prefixes, since connection 
                // groups may include suffixes for client certificates, SSL over proxy, authentication IDs.
                foreach (var item in m_ConnectionGroupList.Keys) {                    
                    string current = item as string;
                    if (current.StartsWith(connectionGroupPrefixSafe, StringComparison.Ordinal) || 
                        current.StartsWith(connectionGroupPrefixUnsafe, StringComparison.Ordinal)) {
                        if (connectionGroupNames == null) {
                            connectionGroupNames = new List<string>();
                        }
                        connectionGroupNames.Add(current);
                    }
                }
            }

            // If this service point contains any connection groups with the provided prefix, remove them.
            if (connectionGroupNames != null) {
                foreach (string item in connectionGroupNames) {
                    ReleaseConnectionGroup(item);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the maximum number of connections allowed on this <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public int ConnectionLimit
        {
            get
            {
                // If there hasn't been a DNS resolution yet, make a guess based on the host name.  It might change
                // when DNS is finally done, but that's ok.  It can change anyway based on other factors like redirects.
                if (!m_UserChangedLimit && m_IPAddressInfoList == null && m_HostLoopbackGuess == TriState.Unspecified)
                {
                    // This can only happen the first time through, and before any ConnectionGroups are made.
                    lock (this)
                    {
                        if (!m_UserChangedLimit && m_IPAddressInfoList == null && m_HostLoopbackGuess == TriState.Unspecified)
                        {
                            // First check if it's just an IP address anyway.
                            IPAddress addr = null;
                            if (IPAddress.TryParse(m_Host,out addr))
                            {
                                m_HostLoopbackGuess = IsAddressListLoopback(new IPAddress[] { addr }) ? TriState.True : TriState.False;
                            }
                            else
                            {
                                m_HostLoopbackGuess = NclUtilities.GuessWhetherHostIsLoopback(m_Host) ? TriState.True : TriState.False;
                            }
                        }
                    }
                }

                return m_UserChangedLimit || (m_IPAddressInfoList == null ? m_HostLoopbackGuess != TriState.True : !m_IPAddressesAreLoopback) ? m_ConnectionLimit : LoopbackConnectionLimit;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (!m_UserChangedLimit || m_ConnectionLimit != value)
                {
                    lock (this)
                    {
                        if (!m_UserChangedLimit || m_ConnectionLimit != value)
                        {
                            m_ConnectionLimit = value;
                            m_UserChangedLimit = true;

                            // Don't want to call ResolveConnectionLimit() or ConnectionLimit before setting m_UserChangedLimit
                            // in order to avoid the 'guess' logic in ConnectionLimit.
                            ResolveConnectionLimit();
                        }
                    }
                }
            }
        }

        // Must be called under lock.
        private void ResolveConnectionLimit()
        {
            int limit = ConnectionLimit;
            foreach (ConnectionGroup cg in m_ConnectionGroupList.Values)
            {
                cg.ConnectionLimit = limit;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the current number of connections associated with this
        ///    <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public int CurrentConnections {
            get {
                int connections = 0;
                lock(this)
                {
                    foreach (ConnectionGroup group in m_ConnectionGroupList.Values)
                    {
                        connections += group.CurrentConnections;
                    }
                }
                return connections;
            }
        }

#if !FEATURE_PAL
        /// <devdoc>
        ///    <para>
        ///       Gets the certificate received for this <see cref='System.Net.ServicePoint'/>.
        ///    </para>
        /// </devdoc>
        public  X509Certificate Certificate {
            get {
                    object chkCert = m_ServerCertificateOrBytes;
                    if (chkCert != null && chkCert.GetType() == typeof(byte[]))
                        return (X509Certificate)(m_ServerCertificateOrBytes = new X509Certificate((byte[]) chkCert));
                    else
                        return chkCert as X509Certificate;
                }
        }
        internal void UpdateServerCertificate(X509Certificate certificate)
        {
            if (certificate != null)
                m_ServerCertificateOrBytes = certificate.GetRawCertData();
            else
                m_ServerCertificateOrBytes = null;
        }

        /// <devdoc>
        /// <para>
        /// Gets the Client Certificate sent by us to the Server.
        /// </para>
        /// </devdoc>
        public  X509Certificate ClientCertificate {
            get {
                object chkCert = m_ClientCertificateOrBytes;
                if (chkCert != null && chkCert.GetType() == typeof(byte[]))
                    return (X509Certificate)(m_ClientCertificateOrBytes = new X509Certificate((byte[]) chkCert));
                else
                    return chkCert as X509Certificate;
            }
        }
        internal void UpdateClientCertificate(X509Certificate certificate)
        {
            if (certificate != null)
                m_ClientCertificateOrBytes = certificate.GetRawCertData();
            else
                m_ClientCertificateOrBytes = null;
        }

#endif // !FEATURE_PAL


        /// <devdoc>
        ///    <para>
        ///       Indicates that the <see cref='System.Net.ServicePoint'/> supports pipelined connections.
        ///    </para>
        /// </devdoc>
        public bool SupportsPipelining {
            get {
                return (m_HttpBehaviour>HttpBehaviour.HTTP10 || m_HttpBehaviour==HttpBehaviour.Unknown);
            }
        }

        //
        // SetTcpKeepAlive 
        //
        // Enable/Disable the use of TCP keepalive option on ServicePoint
        // connections. This method does not affect existing ServicePoints.
        // When a ServicePoint is constructed it will inherit the current 
        // settings.
        //
        // Parameters:
        //
        // enabled - if true enables the use of the TCP keepalive option 
        // for ServicePoint connections.
        //
        // keepAliveTime - specifies the timeout, in milliseconds, with no
        // activity until the first keep-alive packet is sent. Ignored if 
        // enabled parameter is false.
        //
        // keepAliveInterval - specifies the interval, in milliseconds, between
        // when successive keep-alive packets are sent if no acknowledgement is
        // received. Ignored if enabled parameter is false.
        //
        public void SetTcpKeepAlive(
                            bool enabled, 
                            int keepAliveTime, 
                            int keepAliveInterval) {
        
            GlobalLog.Enter(
                "ServicePoint::SetTcpKeepAlive()" + 
                " enabled: " + enabled.ToString() +
                " keepAliveTime: " + keepAliveTime.ToString() +
                " keepAliveInterval: " + keepAliveInterval.ToString()
            );
            if (enabled) {
                m_UseTcpKeepAlive = true;
                if (keepAliveTime <= 0) {
                    throw new ArgumentOutOfRangeException("keepAliveTime");
                }
                if (keepAliveInterval <= 0) {
                    throw new ArgumentOutOfRangeException("keepAliveInterval");
                }
                m_TcpKeepAliveTime = keepAliveTime;
                m_TcpKeepAliveInterval = keepAliveInterval;
            } else {
                m_UseTcpKeepAlive = false;
                m_TcpKeepAliveTime = 0;
                m_TcpKeepAliveInterval =0;
            }
            GlobalLog.Leave("ServicePoint::SetTcpKeepAlive()");
        }

        //
        // Internal Properties
        //

        internal bool Understands100Continue {
            set {
                m_Understands100Continue = value;
            }
            get {
                return m_Understands100Continue;
            }
        }

        //
        // InternalProxyServicePoint
        //
        // Indicates if we are using this service point to represent
        //  a proxy connection, if so we may have to use special
        //  semantics when creating connections
        //

        internal bool InternalProxyServicePoint {
            get {
                return m_ProxyServicePoint;
            }
        }

        //
        // IncrementConnection
        //
        // call to indicate that we now are starting a new
        //  connection within this service point
        //

        internal void IncrementConnection() {
            GlobalLog.Enter("ServicePoint#" + ValidationHelper.HashString(this) + "::IncrementConnection()", m_CurrentConnections.ToString());
            // we need these to be atomic operations
            lock(this) {
                m_CurrentConnections++;
                if (m_CurrentConnections==1) {
                    GlobalLog.Assert(m_ExpiringTimer != null, "ServicePoint#{0}::IncrementConnection|First connection active, but ServicePoint wasn't idle.", ValidationHelper.HashString(this));

                    // 




                    m_ExpiringTimer.Cancel();
                    m_ExpiringTimer = null;
                }
            }
            GlobalLog.Leave("ServicePoint#" + ValidationHelper.HashString(this) + "::IncrementConnection()", m_CurrentConnections.ToString());
        }

        //
        // DecrementConnection
        //
        // call to indicate that we now are removing
        //  a connection within this connection group
        //

        internal void DecrementConnection() {
            // The timer thread is allowed to call this.  (It doesn't call user code and doesn't block.)
            GlobalLog.ThreadContract(ThreadKinds.Unknown, ThreadKinds.SafeSources | ThreadKinds.Timer, "ServicePoint#" + ValidationHelper.HashString(this) + "::DecrementConnection");
            GlobalLog.Enter("ServicePoint#" + ValidationHelper.HashString(this) + "::DecrementConnection()", m_CurrentConnections.ToString());

            // we need these to be atomic operations
            lock(this) {
                m_CurrentConnections--;
                if (m_CurrentConnections==0) {
                    GlobalLog.Assert(m_ExpiringTimer == null, "ServicePoint#{0}::DecrementConnection|Expiring timer set on non-idle ServicePoint.", ValidationHelper.HashString(this));
                    m_IdleSince = DateTime.Now;
                    m_ExpiringTimer = m_IdlingQueue.CreateTimer(ServicePointManager.IdleServicePointTimeoutDelegate, this);
                }
                else if ( m_CurrentConnections < 0 ) {
                    m_CurrentConnections = 0;
                    Diagnostics.Debug.Assert(false, "ServicePoint; Too many decrements.");
                }
            }
            GlobalLog.Leave("ServicePoint#" + ValidationHelper.HashString(this) + "::DecrementConnection()", m_CurrentConnections.ToString());
        }
        
#if !FEATURE_PAL
        internal RemoteCertValidationCallback SetupHandshakeDoneProcedure(TlsStream secureStream, Object request) {
            // Use a private adapter to connect tlsstream and this service point
            return HandshakeDoneProcedure.CreateAdapter(this, secureStream, request);
        }

        // This is an adapter class that ties a servicePoint and a TlsStream on the SSL handshake completion
        private class HandshakeDoneProcedure {
            TlsStream    m_SecureStream;
            Object       m_Request;
            ServicePoint m_ServicePoint;

            internal static RemoteCertValidationCallback CreateAdapter(ServicePoint serviePoint, TlsStream secureStream, Object request)
            {
                HandshakeDoneProcedure adapter = new HandshakeDoneProcedure(serviePoint, secureStream, request);
                return new RemoteCertValidationCallback(adapter.CertValidationCallback);
            }

            private HandshakeDoneProcedure (ServicePoint serviePoint, TlsStream secureStream, Object request) {
                m_ServicePoint  = serviePoint;
                m_SecureStream  = secureStream;
                m_Request       = request;
            }

            private bool CertValidationCallback(string hostName, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
                m_ServicePoint.UpdateServerCertificate(certificate);
                m_ServicePoint.UpdateClientCertificate(m_SecureStream.ClientCertificate);
                bool useDefault = true;

                // Request specific validator takes priority
                HttpWebRequest httpWebRequest = m_Request as HttpWebRequest;
                if (httpWebRequest != null && httpWebRequest.ServerCertValidationCallback != null)
                {
                    return httpWebRequest.ServerCertValidationCallback.
                                          Invoke(m_Request,
                                                 certificate,
                                                 chain,
                                                 sslPolicyErrors);
                }

                // If a policy is set, call the user callback inside the ExecutionContext.
                if (ServicePointManager.GetLegacyCertificatePolicy() != null && (m_Request is WebRequest))
                {
                    useDefault = false;

                    bool checkResult = ServicePointManager.CertPolicyValidationCallback.
                                                           Invoke(hostName,
                                                                  m_ServicePoint,
                                                                  certificate,
                                                                  (WebRequest) m_Request,
                                                                  chain,
                                                                  sslPolicyErrors);

                    if (checkResult == false){
                        if (!ServicePointManager.CertPolicyValidationCallback.UsesDefault
                            || ServicePointManager.ServerCertificateValidationCallback == null)
                            return checkResult;
                    }
                }

                if (ServicePointManager.ServerCertificateValidationCallback != null)
                {
                    useDefault = false;
                    return ServicePointManager.ServerCertValidationCallback.
                                               Invoke(m_Request,
                                                      certificate,
                                                      chain,
                                                      sslPolicyErrors);
                }

                if (useDefault)
                    return sslPolicyErrors == SslPolicyErrors.None;

                return true;
            }


        }

#endif // !FEATURE_PAL
        
        private void IdleConnectionGroupTimeoutCallback(TimerThread.Timer timer, int timeNoticed, object context) {
            ConnectionGroup connectionGroup = (ConnectionGroup)context;

            if (Logging.On) Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_closed_idle, 
                "ConnectionGroup", connectionGroup.GetHashCode()));

            ReleaseConnectionGroup(connectionGroup.Name);
        }
        
        internal TimerThread.Timer CreateConnectionGroupTimer(ConnectionGroup connectionGroup) {
            return m_IdlingQueue.CreateTimer(m_IdleConnectionGroupTimeoutDelegate, connectionGroup);
        }

        /// <devdoc>
        ///    <para>
        ///       Sets connections in this group to not be KeepAlive.
        ///       This is called to force cleanup of the ConnectionGroup by the
        ///       NTLM and Negotiate authentication modules.
        ///    </para>
        /// </devdoc>
        internal bool ReleaseConnectionGroup(string connName) {

            ConnectionGroup connectionGroup = null;

            //
            // look up the ConnectionGroup based on the name
            //
            lock(this) {
                
                connectionGroup = FindConnectionGroup(connName, true);
                //
                // force all connections on the ConnectionGroup to not be KeepAlive
                //
                if (connectionGroup == null) {
                    return false;
                }

                // Cancel the timer so it doesn't fire later and clean up a different 
                // connection group with the same name.
                connectionGroup.CancelIdleTimer();

                //
                // remove ConnectionGroup from our Hashtable
                //
                m_ConnectionGroupList.Remove(connName);
            }

            // Don't call the following under the lock: ConnectionGroup will call into Connection that
            // may take a lock on the Connection. ServicePoint should never call members under the lock that
            // end up taking a lock on a Connection (risk of deadlock).
            connectionGroup.DisableKeepAliveOnConnections();

            return true;
        }

        /// <devdoc>
        ///    <para>
        ///       - Sets all connections in all connections groups to not be KeepAlive.
        ///       - Causes all connections to be closed, if they are not active
        ///       - Removes all references to all connection groups and their connections
        ///       does essentially a "ReleaseConnectionGroup" of each group in this ServicePoint
        ///    </para>
        /// </devdoc>
        internal void ReleaseAllConnectionGroups()
        {
            // The timer thread is allowed to call this.  (It doesn't call user code and doesn't block.)
            GlobalLog.ThreadContract(ThreadKinds.Unknown, ThreadKinds.SafeSources | ThreadKinds.Timer, "ServicePoint#" + ValidationHelper.HashString(this) + "::ReleaseAllConnectionGroups");

            // To avoid deadlock (can't lock a ServicePoint followed by a Connection), copy out all the
            // connection groups in a lock, then release them all outside of it.
            ArrayList cgs = new ArrayList(m_ConnectionGroupList.Count);
            lock(this)
            {
                foreach (ConnectionGroup cg in m_ConnectionGroupList.Values)
                {
                    cgs.Add(cg);
                }
                m_ConnectionGroupList.Clear();
            }
            foreach (ConnectionGroup cg in cgs)
            {
                cg.DisableKeepAliveOnConnections();
            }
        }


        /// <summary>
        ///    <para>Internal class, used to store state for async Connect</para>
        /// </summary>
        private class ConnectSocketState {
            internal ConnectSocketState(ServicePoint servicePoint, PooledStream pooledStream, object owner, Socket s4, Socket s6)
            {
                this.servicePoint = servicePoint;
                this.pooledStream = pooledStream;
                this.owner = owner;
                this.s4 = s4;
                this.s6 = s6;
            }
            internal ServicePoint servicePoint;
            internal Socket s4;
            internal Socket s6;
            internal object owner;
            internal IPAddress[] addresses;
            internal int currentIndex;
            internal int i;
            internal int unsuccessfulAttempts;
            internal bool connectFailure;
            internal PooledStream pooledStream;
        }


        /// <summary>
        ///    <para>Proviates an async callback that is called when Connect completes [part of ConnectSocket(...)]</para>
        /// </summary>
        private static void ConnectSocketCallback(IAsyncResult asyncResult) {
            ConnectSocketState state = (ConnectSocketState)asyncResult.AsyncState;
            Socket socket = null;
            IPAddress address = null;
            Exception innerException = null;
            Exception exception = null;
            WebExceptionStatus ws = WebExceptionStatus.ConnectFailure;


            try {
                ws = state.servicePoint.ConnectSocketInternal(state.connectFailure, state.s4, state.s6, ref socket, ref address, state, asyncResult, out innerException);
            }
            catch (SocketException socketException) {
                exception = socketException;
            }
            catch (ObjectDisposedException socketException) {
                exception = socketException;
            }

            if (ws == WebExceptionStatus.Pending) {
                return;
            }

            if (ws == WebExceptionStatus.Success) {
                try {
                    state.servicePoint.CompleteGetConnection(state.s4, state.s6, socket, address);
                }
                catch (SocketException socketException) {
                    exception = socketException;
                }
                catch (ObjectDisposedException socketException) {
                    exception = socketException;
                }

            }
            else {
                exception = new WebException(
                        NetRes.GetWebStatusString(ws),
                        ws == WebExceptionStatus.ProxyNameResolutionFailure || ws == WebExceptionStatus.NameResolutionFailure ? state.servicePoint.Host : null,
                        innerException,
                        ws,
                        null, /* no response */
                        WebExceptionInternalStatus.ServicePointFatal);
            }
            try {
                state.pooledStream.ConnectionCallback(state.owner, exception, socket, address);
            }
            catch
            {
                if (socket != null && socket.CleanedUp)
                    return;   // The connection was aborted and requests dispatched
                throw;
            }

        }

        private void BindUsingDelegate(Socket socket, IPEndPoint remoteIPEndPoint)
        {
            IPEndPoint clonedRemoteIPEndPoint = new IPEndPoint(remoteIPEndPoint.Address, remoteIPEndPoint.Port);
            int retryCount;

            for (retryCount=0; retryCount<int.MaxValue; retryCount++) {
                IPEndPoint localIPEndPoint = BindIPEndPointDelegate(this, clonedRemoteIPEndPoint, retryCount);
                if (localIPEndPoint == null)
                    break;

                try {
                    socket.InternalBind(localIPEndPoint);
                }
                catch {
                    continue;
                }
                break;
            }
            if (retryCount == int.MaxValue)
                throw new OverflowException("Reached maximum number of BindIPEndPointDelegate retries");
        }


        /// <summary>
        ///    <para>Set SocketOptionName.ReuseUnicastPort (SO_REUSE_UNICASTPORT) socket option on the outbound connection.</para>
        /// </summary>
        private void SetUnicastReusePortForSocket(Socket socket)
        {
            bool reusePort;
         
            if (ServicePointManager.ReusePortSupported.HasValue && !ServicePointManager.ReusePortSupported.Value) {
                // We tried to set the socket option before and it isn't supported on this system.  So, we'll save some
                // time by not trying again.
                reusePort = false;
            }
            else {
                reusePort = ServicePointManager.ReusePort;
            }

            if (reusePort) {
                // This socket option is defined in Windows 10.0 or later.  It is also
                // available if an LDR servicing patch has been installed on downlevel OS.
                try {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseUnicastPort, 0x1);
                    if (Logging.On) { 
                        Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_set_socketoption_reuseport, 
                            "Socket", socket.GetHashCode()));
                    }
                    
                    ServicePointManager.ReusePortSupported = true;
                }
                catch (SocketException) {
                    // The socket option is not supported.  We will ignore this error and fail gracefully.
                    if (Logging.On) { 
                        Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_set_socketoption_reuseport_not_supported, 
                            "Socket", socket.GetHashCode()));
                    }                    
                    ServicePointManager.ReusePortSupported = false;
                }
                catch (Exception ex) {
                    // We want to preserve app compat and trap any other unusual exceptions.
                    if (Logging.On) { 
                        Logging.PrintInfo(Logging.Web, this, SR.GetString(SR.net_log_unexpected_exception, ex.Message));
                    }
                }
            }            
        }
        
        /// <summary>
        ///    <para>This is the real logic for doing the Connect with IPv4 and IPv6 addresses, see ConnectSocket for details</para>
        /// </summary>
        private WebExceptionStatus ConnectSocketInternal(bool connectFailure, Socket s4, Socket s6, ref Socket socket, 
            ref IPAddress address, ConnectSocketState state, IAsyncResult asyncResult, out Exception exception) {
            IPEndPoint remoteIPEndPoint;
            exception = null;

            //
            // so, here we are: we have the EndPoint we need to connect to, all we
            // need to do is call into winsock and try to connect to this HTTP server.
            //
            // this is how we do it:
            // we'll keep trying to Connect() until either:
            // (1) Connect() succeeds (on which we increment the number of connections opened) or
            // (2) we can't get any new address for this host.
            //
            // (1) is pretty easy, here's how we do (2):
            // If the hostinformation is every marked as failed, we will automatically refresh it
            // the next time it is read.
            // If we fail the first time using the DNS information and the DNS information is recent,
            // which mean's it either hasn't been tried or it failed, we will mark the
            // hostinformation as failed, and quit.  Otherwise we'll refresh the DNS information and
            // try one more time. If we fail the second time, then we'll mark the DNS information
            // as failed and quit.
            IPAddress[] addresses = null;
            for (int unsuccessfulAttempts = 0; unsuccessfulAttempts < 2; unsuccessfulAttempts++) {

                int currentIndex;
                int i = 0;

                // Use asyncResult to make sure it is only called at initiation time.
                if (asyncResult == null)
                {
                    // the second time, determine if the list was recent.

                    addresses = GetIPAddressInfoList(out currentIndex, addresses);

                    //the addresses were recent, or we couldn't resolve the addresses.
                    if (addresses == null || addresses.Length == 0)
                        break;
                }
                else
                {
                    GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::ConnectSocketInternal() resuming previous state");

                    addresses = state.addresses;
                    currentIndex = state.currentIndex;
                    i = state.i;
                    unsuccessfulAttempts = state.unsuccessfulAttempts;
                }

                //otherwise, try all of the addresses in the list.
                for (; i < addresses.Length; i++)
                {
                    IPAddress ipAddressInfo = addresses[currentIndex];
                    try {
                        remoteIPEndPoint = new IPEndPoint(ipAddressInfo, m_Port);
                        Socket attemptSocket;
                        if ( remoteIPEndPoint.Address.AddressFamily==AddressFamily.InterNetwork ) {
                            attemptSocket = s4;
                        }
                        else {
                            attemptSocket = s6;
                        }

                        if (state != null)
                        {
                            if (asyncResult != null)
                            {
                                IAsyncResult asyncResultCopy = asyncResult;
                                asyncResult = null;
                                attemptSocket.EndConnect(asyncResultCopy);
                            }
                            else {
                                GlobalLog.Print("ServicePoint#" + ValidationHelper.HashString(this) + "::ConnectSocketInternal() calling BeginConnect() to:" + remoteIPEndPoint.ToString());

                                // save off our state and do our async call
                                state.addresses = addresses;
                                state.currentIndex = currentIndex;
                                state.i = i;
                                state.unsuccessfulAttempts = unsuccessfulAttempts;
                                state.connectFailure = connectFailure;

                                if (!attemptSocket.IsBound) {
                                    if (ServicePointManager.ReusePort) {
                                        SetUnicastReusePortForSocket(attemptSocket);
                                    }
                                
                                    if (BindIPEndPointDelegate != null) {
                                        BindUsingDelegate(attemptSocket, remoteIPEndPoint);
                                    }
                                }

                                attemptSocket.UnsafeBeginConnect(remoteIPEndPoint, m_ConnectCallbackDelegate, state);
                                return WebExceptionStatus.Pending;
                            }
                        }
                        else {
                            if (!attemptSocket.IsBound) {
                                    if (ServicePointManager.ReusePort) {
                                        SetUnicastReusePortForSocket(attemptSocket);
                                    }
                                
                                if (BindIPEndPointDelegate != null) {
                                    BindUsingDelegate(attemptSocket, remoteIPEndPoint);
                                }
                            }

                            attemptSocket.InternalConnect(remoteIPEndPoint);
                        }
                        socket  = attemptSocket;
                        address = ipAddressInfo;
                        exception = null;
                        UpdateCurrentIndex(addresses, currentIndex);
                        return WebExceptionStatus.Success;
                    }
                    catch (ObjectDisposedException)
                    {
                        // This can happen if the request has been aborted and the attemptSocket got closed.
                        return WebExceptionStatus.RequestCanceled;
                    }
                    catch (Exception e)
                    {
                        if (NclUtilities.IsFatal(e)) throw;

                        exception = e;
                        connectFailure = true;
                    }
                    currentIndex++;
                    if (currentIndex >= addresses.Length) {
                        currentIndex = 0;
                    }
                }
            }

            Failed(addresses);

            return connectFailure ? WebExceptionStatus.ConnectFailure :
                InternalProxyServicePoint ? WebExceptionStatus.ProxyNameResolutionFailure :
                WebExceptionStatus.NameResolutionFailure;
        }

        /// <summary>
        ///    <para>private implimentation of ConnectSocket(...)</para>
        /// </summary>
        private WebExceptionStatus ConnectSocket(Socket s4, Socket s6, ref Socket socket, ref IPAddress address, 
            ConnectSocketState state, out Exception exception) {
            //
            // we need this for the call to connect()
            //
            return ConnectSocketInternal(false, s4, s6, ref socket, ref address, state, null, out exception);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal void DebugMembers(int requestHash) {
            foreach(ConnectionGroup connectGroup in  m_ConnectionGroupList.Values) {
                if (connectGroup!=null) {
                    try {
                        connectGroup.DebugMembers(requestHash);
                    }
                    catch {
                    }
                }
            }
        }


        //
        // Previously: class IPHostInformation
        //

        private string              m_HostName = String.Empty;
        private bool                m_IsTrustedHost = true; // CBT: False if the DNS resolve changed the host
        private IPAddress[]         m_IPAddressInfoList;
        private int                 m_CurrentAddressInfoIndex;
        private bool                m_ConnectedSinceDns = false;
        private bool                m_AddressListFailed = false;
        private DateTime            m_LastDnsResolve;
        private bool                m_IPAddressesAreLoopback;

        private void Failed(IPAddress[] addresses)
        {
            if (addresses == m_IPAddressInfoList){
               lock(this){
                   if (addresses == m_IPAddressInfoList){
                       m_AddressListFailed = true;
                   }
               }
           }
        }


        //if dns round robin is enabled, we don't want to update the index
        //because other connections may have skipped to the next address.
        //we need a better mechanism to handle dead connections
        private void UpdateCurrentIndex(IPAddress[] addresses, int currentIndex)
        {
            if (addresses == m_IPAddressInfoList && (m_CurrentAddressInfoIndex != currentIndex || !m_ConnectedSinceDns)){
                lock(this){
                    if (addresses == m_IPAddressInfoList){
                        if (!ServicePointManager.EnableDnsRoundRobin ) {
                            m_CurrentAddressInfoIndex = currentIndex;
                        }
                        m_ConnectedSinceDns = true;
                    }
                }
            }
        }


        private bool HasTimedOut {
            get {
                int dnsRefreshTimeout = ServicePointManager.DnsRefreshTimeout;
                return dnsRefreshTimeout != Timeout.Infinite &&
                    (m_LastDnsResolve + new TimeSpan(0, 0, 0, 0, dnsRefreshTimeout)) < DateTime.UtcNow;
            }
        }


        // If addresses is specified, we determine if the addresslist is recent
        // If the answer is yes, we return null.  Whether its recent is determined by whether
        // or not the current hostinformation has ever been marked as succeeded or failed (meaning
        // even tried). If it isn't recent, we'll refresh the addresslist.

        private IPAddress[] GetIPAddressInfoList(out int currentIndex, IPAddress[] addresses)
        {
            IPHostEntry ipHostEntry = null;
            currentIndex = 0;
            bool needDnsResolution = false;
            bool dnsResolutionFailed = false;

            // Phase 1: Decide if we need to do a DNS resolution
            lock (this) {

                // return null if the current hostinformation has never been marked as succeeded or failed
                // (the hostinformation hasn't been used) and it hasn't changed.

                if (addresses != null && !m_ConnectedSinceDns && !m_AddressListFailed && addresses == m_IPAddressInfoList)
                    return null;

                // refresh the list if its already failed, or if the addresslist isn't recent
                if (m_IPAddressInfoList == null || m_AddressListFailed || addresses == m_IPAddressInfoList || HasTimedOut) {
                    m_CurrentAddressInfoIndex = 0;
                    m_ConnectedSinceDns = false;
                    m_AddressListFailed = false;
                    m_LastDnsResolve = DateTime.UtcNow;

                    needDnsResolution = true;
                }
            }

            // Phase 2: If we have to do a DNS resolution now, then do it now
            if (needDnsResolution) {
                try {
                    dnsResolutionFailed = !Dns.TryInternalResolve(m_Host, out ipHostEntry);
                }
                catch (Exception exception)
                {
                    if (NclUtilities.IsFatal(exception)) throw;
                    dnsResolutionFailed = true;
                    GlobalLog.Print("IPHostInformation#" + ValidationHelper.HashString(this) + "::GetIPAddressInfoList() Dns.InternalResolveFast() failed with exception:\r\n" + exception.ToString());
                }
            }

            // Phase 3: If we did a DNS resolution, then deal with the results safely under a lock
            lock (this) {
                if (needDnsResolution) {

                    m_IPAddressInfoList = null;

                    if (!dnsResolutionFailed) {
                        if (ipHostEntry!=null && ipHostEntry.AddressList!=null && ipHostEntry.AddressList.Length>0) {
                            SetAddressList(ipHostEntry);
                        }
                        else {
                            GlobalLog.Print("IPHostInformation#" + ValidationHelper.HashString(this) + "::GetIPAddressInfoList() Dns.InternalResolveFast() failed with null");
                        }
                    } else {
                        GlobalLog.Print("IPHostInformation#" + ValidationHelper.HashString(this) + "::GetIPAddressInfoList() Dns.InternalResolveFast() had thrown an exception");
                    }
                }

                if (m_IPAddressInfoList!=null && m_IPAddressInfoList.Length > 0) {
                    GlobalLog.Print("IPHostInformation#" + ValidationHelper.HashString(this) + "::GetIPAddressInfoList() m_IPAddressInfoList = "+m_IPAddressInfoList);
                    currentIndex = m_CurrentAddressInfoIndex;

                    //auto increment index for next connect request if round robin is enabled
                    if (ServicePointManager.EnableDnsRoundRobin)
                    {
                        m_CurrentAddressInfoIndex++;
                        if (m_CurrentAddressInfoIndex >= m_IPAddressInfoList.Length) {
                            m_CurrentAddressInfoIndex = 0;
                        }
                    }
                    return m_IPAddressInfoList;
                }
            }
            GlobalLog.Print("IPHostInformation#" + ValidationHelper.HashString(this) + "::GetIPAddressInfoList() GetIPAddressInfoList returning null");
            return null;
        }

        //
        // Called under lock(this)
        //
        private void SetAddressList(IPHostEntry ipHostEntry)
        {
            GlobalLog.Print("IPHostInformation#" + ValidationHelper.HashString(this) + "::SetAddressList("+ipHostEntry.HostName+")");
            //
            // Create an array of IPAddress of the appropriate size, then
            // get a list of our local addresses. Walk through the input
            // address list. Copy each address in the address list into
            // our array, and if the address is a loopback address, mark it as
            // such.
            //
            // Only update the member with successfull final result.
            // In case of an exception m_IPAddressInfoList will stay as null
            //
            bool wasLoopback = m_IPAddressesAreLoopback;
            bool wasNull = m_IPAddressInfoList == null;

            m_IPAddressesAreLoopback = IsAddressListLoopback(ipHostEntry.AddressList);
            m_IPAddressInfoList = ipHostEntry.AddressList;
            m_HostName = ipHostEntry.HostName;
            m_IsTrustedHost = ipHostEntry.isTrustedHost;

            if (wasNull || wasLoopback != m_IPAddressesAreLoopback)
            {
                ResolveConnectionLimit();
            }
        }

        private static bool IsAddressListLoopback(IPAddress[] addressList)
        {
            GlobalLog.Print("IPHostInformation::CheckAddressList(" + addressList.Length + ")");

            //
            // Walk through each member of the input list, copying it into our temp array.
            //

            int i, k;
            IPAddress[] localAddresses = null;
            try {
                localAddresses = NclUtilities.LocalAddresses;
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception)) throw;

                // ATTN: If LocalAddresses has failed terribly we will treat just resolved name as a remote server.
                //       

                if (Logging.On)
                {
                    Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_retrieving_localhost_exception, exception));
                    Logging.PrintWarning(Logging.Web, SR.GetString(SR.net_log_resolved_servicepoint_may_not_be_remote_server));
                }
            }

            for (i = 0; i < addressList.Length; i++)
            {
                // First, check to see if the current address is a loopback address.
                if (IPAddress.IsLoopback(addressList[i]))
                {
                    continue;
                }

                if (localAddresses != null)
                {
                    // See if the current IP address is a local address, and if
                    // so mark it as such.
                    for (k = 0; k < localAddresses.Length; k++)
                    {
                        //
                        // IPv6 Changes: Use .Equals for this check !
                        //
                        if (addressList[i].Equals(localAddresses[k]))
                        {
                            break;
                        }
                    }
                    if (k < localAddresses.Length)
                    {
                        continue;
                    }
                }

                break;
            }

            return i == addressList.Length;
        }
    }
}
