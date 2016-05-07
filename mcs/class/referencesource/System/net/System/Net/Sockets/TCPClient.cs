//------------------------------------------------------------------------------
// <copyright file="TCPClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace System.Net.Sockets {
    
    using System.Threading;
    using System.Threading.Tasks;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>The <see cref='System.Net.Sockets.TcpClient'/> class provide TCP services at a higher level
    ///    of abstraction than the <see cref='System.Net.Sockets.Socket'/> class. <see cref='System.Net.Sockets.TcpClient'/>
    ///    is used to create a Client connection to a remote host.</para>
    /// </devdoc>
    public class TcpClient : IDisposable {

        Socket m_ClientSocket;
        bool m_Active;
        NetworkStream m_DataStream;

        //
        // IPv6: Maintain address family for the client
        //
        AddressFamily m_Family = AddressFamily.InterNetwork;

        // specify local IP and port
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.Sockets.TcpClient'/>
        ///       class with the specified end point.
        ///    </para>
        /// </devdoc>
        public TcpClient(IPEndPoint localEP) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "TcpClient", localEP);
            if (localEP==null) {
                throw new ArgumentNullException("localEP");
            }
            //
            // IPv6: Establish address family before creating a socket
            //
            m_Family = localEP.AddressFamily;

            initialize();
            Client.Bind(localEP);
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "TcpClient", "");
        }

        // TcpClient(IPaddress localaddr); // port is arbitrary
        // TcpClient(int outgoingPort); // local IP is arbitrary

        // address+port is arbitrary
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.Sockets.TcpClient'/> class.
        ///    </para>
        /// </devdoc>
        public TcpClient() : this(AddressFamily.InterNetwork) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "TcpClient", null);
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "TcpClient", null);
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.Sockets.TcpClient'/> class.
        ///    </para>
        /// </devdoc>
#if COMNET_DISABLEIPV6
        private TcpClient(AddressFamily family) {
#else
        public TcpClient(AddressFamily family) {
#endif
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "TcpClient", family);
            //
            // Validate parameter
            //
            if ( family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6) {
                throw new ArgumentException(SR.GetString(SR.net_protocol_invalid_family, "TCP"), "family");
            }

            m_Family = family;

            initialize();
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "TcpClient", null);
        }

        // bind and connect
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Net.Sockets.TcpClient'/> class and connects to the
        ///    specified port on the specified host.</para>
        /// </devdoc>
        public TcpClient(string hostname, int port) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "TcpClient", hostname);
            if (hostname==null) {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port)) {
                throw new ArgumentOutOfRangeException("port");
            }
            //
            // IPv6: Delay creating the client socket until we have
            //       performed DNS resolution and know which address
            //       families we can use.
            //
            //initialize();

            try{
                Connect(hostname, port);
            }

            catch(Exception e){
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {                                       
                    throw;
                }
                
                if(m_ClientSocket != null){
                    m_ClientSocket.Close();  
                }
                throw e;
            }

            if(Logging.On)Logging.Exit(Logging.Sockets, this, "TcpClient", null);
        }

        //
        // used by TcpListener.Accept()
        //
        internal TcpClient(Socket acceptedSocket) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "TcpClient", acceptedSocket);
            Client = acceptedSocket;
            m_Active = true;
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "TcpClient", null);
        }

        /// <devdoc>
        ///    <para>
        ///       Used by the class to provide
        ///       the underlying network socket.
        ///    </para>
        /// </devdoc>
        public Socket Client {
            get {
                return m_ClientSocket;
            }
            set {
                m_ClientSocket = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Used by the class to indicate that a connection has been made.
        ///    </para>
        /// </devdoc>
        protected bool Active {
            get {
                return m_Active;
            }
            set {
                m_Active = value;
            }
        }

        public int Available {get {return m_ClientSocket.Available;}}      
        public bool Connected  {get {return m_ClientSocket.Connected;}} 
        public bool ExclusiveAddressUse {
            get {
                return m_ClientSocket.ExclusiveAddressUse;
            } 
            set{
                m_ClientSocket.ExclusiveAddressUse = value;
            }
        }    //new
        
        
        
        /// <devdoc>
        ///    <para>
        ///       Connects the Client to the specified port on the specified host.
        ///    </para>
        /// </devdoc>
        public void Connect(string hostname, int port) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "Connect", hostname);
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (hostname==null) {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port)) {
                throw new ArgumentOutOfRangeException("port");
            }
            //
            // Check for already connected and throw here. This check
            // is not required in the other connect methods as they
            // will throw from WinSock. Here, the situation is more
            // complex since we have to resolve a hostname so it's
            // easier to simply block the request up front.
            //
            if ( m_Active ) {
                throw new SocketException(SocketError.IsConnected);
            }

            //
            // IPv6: We need to process each of the addresses return from
            //       DNS when trying to connect. Use of AddressList[0] is
            //       bad form.
            //
            
            
            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            Exception   lastex = null;
            Socket ipv6Socket = null;
            Socket ipv4Socket = null;
            
            try{
                if (m_ClientSocket == null) {
                    if (Socket.OSSupportsIPv4){
                        ipv4Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    }
                    if (Socket.OSSupportsIPv6){
                        ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    }
                }
    
                foreach (IPAddress address in addresses)
                {
                    try {
                        if ( m_ClientSocket == null ) {
                            //
                            // We came via the <hostname,port> constructor. Set the
                            // address family appropriately, create the socket and
                            // try to connect.
                            //
                            if (address.AddressFamily == AddressFamily.InterNetwork && ipv4Socket != null) {
                                ipv4Socket.Connect(address,port);
                                m_ClientSocket = ipv4Socket;
                                if (ipv6Socket != null)
                                    ipv6Socket.Close();
                            }
                            else if (ipv6Socket != null)
                            {
                                ipv6Socket.Connect(address, port);
                                m_ClientSocket = ipv6Socket;
                                if (ipv4Socket != null)
                                    ipv4Socket.Close();
                            }
    
                            m_Family = address.AddressFamily;
                            m_Active = true;
                            break;
                        }
                        else if ( address.AddressFamily == m_Family ) {
                            //
                            // Only use addresses with a matching family
                            //
                            Connect( new IPEndPoint(address,port) );
                            m_Active = true;
                            break;
                        }
                    }

                    catch ( Exception ex )
                    {
                        if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException) {                                       
                            throw;
                        }
                        lastex = ex;
                    }
                }
            }
            
            catch(Exception ex){
                if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException) {                                       
                    throw;
                }
                lastex = ex;
            }

            finally{
                
                //cleanup temp sockets if failed
                //main socket gets closed when tcpclient gets closed

                //did we connect?
                if ( !m_Active ) {
                    if (ipv6Socket != null) {
                        ipv6Socket.Close();
                    }
                    
                    if (ipv4Socket != null) {
                        ipv4Socket.Close();
                    }


                    //
                    // The connect failed - rethrow the last error we had
                    //
                    if ( lastex != null )
                        throw lastex;
                    else
                        throw new SocketException(SocketError.NotConnected);
                }
            }

            if(Logging.On)Logging.Exit(Logging.Sockets, this, "Connect", null);
        }

        /// <devdoc>
        ///    <para>
        ///       Connects the Client to the specified port on the specified host.
        ///    </para>
        /// </devdoc>
        public void Connect(IPAddress address, int port) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "Connect", address);
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (address==null) {
                throw new ArgumentNullException("address");
            }
            if (!ValidationHelper.ValidateTcpPort(port)) {
                throw new ArgumentOutOfRangeException("port");
            }
            IPEndPoint remoteEP = new IPEndPoint(address, port);
            Connect(remoteEP);
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "Connect", null);
        }

        /// <devdoc>
        ///    <para>
        ///       Connect the Client to the specified end point.
        ///    </para>
        /// </devdoc>
        public void Connect(IPEndPoint remoteEP) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "Connect", remoteEP);
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (remoteEP==null) {
                throw new ArgumentNullException("remoteEP");
            }
            Client.Connect(remoteEP);
            m_Active = true;
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "Connect", null);
        }   



        //methods
        public void Connect(IPAddress[] ipAddresses, int port){
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "Connect", ipAddresses);
            Client.Connect(ipAddresses, port);
            m_Active = true;
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "Connect", null);
        }


        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "BeginConnect", host);
            IAsyncResult result = Client.BeginConnect(host, port, requestCallback, state);
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "BeginConnect", null);
            return result;
        }

        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "BeginConnect", address);
            IAsyncResult result = Client.BeginConnect(address, port, requestCallback, state);
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "BeginConnect", null);
            return result;
        }
        
        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "BeginConnect", addresses);
            IAsyncResult result = Client.BeginConnect(addresses, port, requestCallback, state);
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "BeginConnect", null);
            return result;
        }
        
        public void EndConnect(IAsyncResult asyncResult){
        
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "EndConnect", asyncResult);
            Client.EndConnect(asyncResult);
            m_Active = true;
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "EndConnect", null);
        }


        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        public Task ConnectAsync(IPAddress address, int port)
        {
            return Task.Factory.FromAsync(BeginConnect, EndConnect, address, port, null);
        }

        [HostProtection(ExternalThreading = true)]
        public Task ConnectAsync(string host, int port)
        {
            return Task.Factory.FromAsync(BeginConnect, EndConnect, host, port, null);
        }

        [HostProtection(ExternalThreading = true)]
        public Task ConnectAsync(IPAddress[] addresses, int port)
        {
            return Task.Factory.FromAsync(BeginConnect, EndConnect, addresses, port, null);
        }


        /// <devdoc>
        ///    <para>
        ///       Returns the stream used to read and write data to the
        ///       remote host.
        ///    </para>
        /// </devdoc>
        public NetworkStream GetStream() {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "GetStream", "");
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (!Client.Connected) {
                throw new InvalidOperationException(SR.GetString(SR.net_notconnected));
            }
            if (m_DataStream==null) {
                m_DataStream = new NetworkStream(Client, true);
            }
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "GetStream", m_DataStream);
            return m_DataStream;
        }

        /// <devdoc>
        ///    <para>
        ///       Disposes the Tcp connection.
        ///    </para>
        /// </devdoc>
        //UEUE
        public void Close() {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "Close", "");
            GlobalLog.Print("TcpClient::Close()");
            ((IDisposable)this).Dispose();
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "Close", "");
        }

        private bool m_CleanedUp = false;

        protected virtual void Dispose(bool disposing) {
            if(Logging.On)Logging.Enter(Logging.Sockets, this, "Dispose", "");
            if (m_CleanedUp) {
                if(Logging.On)Logging.Exit(Logging.Sockets, this, "Dispose", "");
                return;
            }

            if (disposing) {
                IDisposable dataStream = m_DataStream;
                if (dataStream != null)
                {
                    dataStream.Dispose();
                }
                else
                {
                    //
                    // if the NetworkStream wasn't created, the Socket might
                    // still be there and needs to be closed. In the case in which
                    // we are bound to a local IPEndPoint this will remove the
                    // binding and free up the IPEndPoint for later uses.
                    //
                    Socket chkClientSocket = Client;
                    if (chkClientSocket!= null) {
                        try {
                            chkClientSocket.InternalShutdown(SocketShutdown.Both);
                        }
                        finally {
                            chkClientSocket.Close();
                            Client = null;
                        }
                    }
                }

                GC.SuppressFinalize(this);
            }

            m_CleanedUp = true;
            if(Logging.On)Logging.Exit(Logging.Sockets, this, "Dispose", "");
        }

        public void Dispose() {
            Dispose(true);
        }

        ~TcpClient() {
#if DEBUG
            GlobalLog.SetThreadSource(ThreadKinds.Finalization);
            using (GlobalLog.SetThreadKind(ThreadKinds.System | ThreadKinds.Async)) {
#endif
            Dispose(false);
#if DEBUG
            }
#endif
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the size of the receive buffer in bytes.
        ///    </para>
        /// </devdoc>
        public int ReceiveBufferSize {
            get {
                return numericOption(SocketOptionLevel.Socket,
                                     SocketOptionName.ReceiveBuffer);
            }
            set {
                Client.SetSocketOption(SocketOptionLevel.Socket,
                                  SocketOptionName.ReceiveBuffer, value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or
        ///       sets the size of the send buffer in bytes.
        ///    </para>
        /// </devdoc>
        public int SendBufferSize {
            get {
                return numericOption(SocketOptionLevel.Socket,
                                     SocketOptionName.SendBuffer);
            }

            set {
                Client.SetSocketOption(SocketOptionLevel.Socket,
                                  SocketOptionName.SendBuffer, value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the receive time out value of the connection in seconds.
        ///    </para>
        /// </devdoc>
        public int ReceiveTimeout {
            get {
                return numericOption(SocketOptionLevel.Socket,
                                     SocketOptionName.ReceiveTimeout);
            }
            set {
                Client.SetSocketOption(SocketOptionLevel.Socket,
                                  SocketOptionName.ReceiveTimeout, value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the send time out value of the connection in seconds.
        ///    </para>
        /// </devdoc>
        public int SendTimeout {
            get {
                return numericOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
            }

            set {
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the value of the connection's linger option.
        ///    </para>
        /// </devdoc>
        public LingerOption LingerState {
            get {
                return (LingerOption)Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
            }
            set {
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Enables or disables delay when send or receive buffers are full.
        ///    </para>
        /// </devdoc>
        public bool NoDelay {
            get {
                return numericOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay) != 0 ? true : false;
            }
            set {
                Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, value ? 1 : 0);
            }
        }

        private void initialize() {
            //
            // IPv6: Use the address family from the constructor (or Connect method)
            //
            Client = new Socket(m_Family, SocketType.Stream, ProtocolType.Tcp);
            m_Active = false;
        }

        private int numericOption(SocketOptionLevel optionLevel, SocketOptionName optionName) {
            return (int)Client.GetSocketOption(optionLevel, optionName);
        }

    }; // class TCPClient


} // namespace System.Net.Sockets
