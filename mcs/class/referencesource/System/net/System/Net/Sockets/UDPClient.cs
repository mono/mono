//------------------------------------------------------------------------------
// <copyright file="UDPClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net.Sockets {

    using System.Threading;
    using System.Threading.Tasks;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    /// <devdoc>
    ///    <para>
    ///       The <see cref='System.Net.Sockets.UdpClient'/> class provides access to UDP services at a
    ///       higher abstraction level than the <see cref='System.Net.Sockets.Socket'/> class. <see cref='System.Net.Sockets.UdpClient'/>
    ///       is used to connect to a remote host and to receive connections from a remote
    ///       Client.
    ///    </para>
    /// </devdoc>
    public class UdpClient : IDisposable {

        private const int MaxUDPSize = 0x10000;  
        private Socket m_ClientSocket;
        private bool m_Active;
        private byte[] m_Buffer = new byte[MaxUDPSize];
        /// <devdoc>
        ///    <para>
        ///       Address family for the client, defaults to IPv4.
        ///    </para>
        /// </devdoc>
        private AddressFamily m_Family = AddressFamily.InterNetwork;

        // bind to arbitrary IP+Port
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.Sockets.UdpClient'/>class.
        ///    </para>
        /// </devdoc>
        public UdpClient() : this(AddressFamily.InterNetwork){
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Net.Sockets.UdpClient'/>class.
        ///    </para>
        /// </devdoc>

        public UdpClient(AddressFamily family) {
            //
            // Validate the address family
            //
            if ( family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6 ) {
                throw new ArgumentException(SR.GetString(SR.net_protocol_invalid_family, "UDP"), "family");
            }

            m_Family = family;

            createClientSocket();
        }

        // bind specific port, arbitrary IP
        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the UdpClient class that communicates on the
        ///       specified port number.
        ///    </para>
        /// </devdoc>
        /// We should obsolete this. This also breaks IPv6-only scenarios.
        /// But fixing it has many complications that we have decided not
        /// to fix it and instead obsolete it post-Orcas.
        public UdpClient(int port) : this(port,AddressFamily.InterNetwork) {
        }

        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the UdpClient class that communicates on the
        ///       specified port number.
        ///    </para>
        /// </devdoc>
        public UdpClient(int port,AddressFamily family) {
            //
            // parameter validation
            //
            if (!ValidationHelper.ValidateTcpPort(port)) {
                throw new ArgumentOutOfRangeException("port");
            }
            //
            // Validate the address family
            //
            if ( family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6 ) {
                throw new ArgumentException(SR.GetString(SR.net_protocol_invalid_family), "family");
            }

            IPEndPoint localEP;
            m_Family = family;
             
            if ( m_Family == AddressFamily.InterNetwork ) {
                localEP = new IPEndPoint(IPAddress.Any, port);
            }
            else {
                localEP = new IPEndPoint(IPAddress.IPv6Any, port);
            }

            createClientSocket();

            Client.Bind(localEP);
        }

        // bind to given local endpoint
        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the UdpClient class that communicates on the
        ///       specified end point.
        ///    </para>
        /// </devdoc>
        public UdpClient(IPEndPoint localEP) {
            //
            // parameter validation
            //
            if (localEP == null) {
                throw new ArgumentNullException("localEP");
            }
            //
            // IPv6 Changes: Set the AddressFamily of this object before
            //               creating the client socket.
            //
            m_Family = localEP.AddressFamily;

            createClientSocket();

            Client.Bind(localEP);
        }

        // bind and connect
        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Net.Sockets.UdpClient'/> class and connects to the
        ///       specified remote host on the specified port.
        ///    </para>
        /// </devdoc>
        public UdpClient(string hostname, int port) {
            //
            // parameter validation
            //
            if (hostname == null) {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port)) {
                throw new ArgumentOutOfRangeException("port");
            }
            //
            // NOTE: Need to create different kinds of sockets based on the addresses
            //       returned from DNS. As a result, we defer the creation of the 
            //       socket until the Connect method.
            //
            //createClientSocket();
            Connect(hostname, port);
        }

        /// <devdoc>
        ///    <para>
        ///       Used by the class to provide the underlying network socket.
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
        ///       Used by the class to indicate that a connection to a remote host has been
        ///       made.
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

        public int Available{
            get{
                return m_ClientSocket.Available;
            }
        }

        public short Ttl{
            get{
                return m_ClientSocket.Ttl;
            } 
            set{
                m_ClientSocket.Ttl = value;
            }
        }      //new


        public bool DontFragment{
            get{
                return m_ClientSocket.DontFragment;
            } 
            set{
                m_ClientSocket.DontFragment = value;
            }
        }      //new

            
        public bool MulticastLoopback{
            get{
                return m_ClientSocket.MulticastLoopback;
            } 
            set{
                m_ClientSocket.MulticastLoopback = value;
            }
        }      //new



        public bool EnableBroadcast{
            get{
                return m_ClientSocket.EnableBroadcast;
            } 
            set{
                m_ClientSocket.EnableBroadcast = value;
            }
        }      //new


        public bool ExclusiveAddressUse {
            get{
                return m_ClientSocket.ExclusiveAddressUse;
            } 
            set{
                m_ClientSocket.ExclusiveAddressUse = value;
            }
        }      //new

        public void AllowNatTraversal(bool allowed) {
            if (allowed) {
                m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            }
            else {
                m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
            }
        }

        public void Close() {
            Dispose(true);
        }

        private bool m_CleanedUp = false;
        private void FreeResources() {
            //
            // only resource we need to free is the network stream, since this
            // is based on the client socket, closing the stream will cause us
            // to flush the data to the network, close the stream and (in the
            // NetoworkStream code) close the socket as well.
            //
            if (m_CleanedUp) {
                return;
            }

            Socket chkClientSocket = Client;
            if (chkClientSocket!=null) {
                //
                // if the NetworkStream wasn't retrieved, the Socket might
                // still be there and needs to be closed to release the effect
                // of the Bind() call and free the bound IPEndPoint.
                //
                chkClientSocket.InternalShutdown(SocketShutdown.Both);
                chkClientSocket.Close();
                Client = null;
            }
            m_CleanedUp = true;
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GlobalLog.Print("UdpClient::Dispose()");
                FreeResources();
                GC.SuppressFinalize(this);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Establishes a connection to the specified port on the
        ///       specified host.
        ///    </para>
        /// </devdoc>
        public void Connect(string hostname, int port){
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (hostname == null){
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port)){
                throw new ArgumentOutOfRangeException("port");
            }

            //
            // IPv6 Changes: instead of just using the first address in the list,
            //               we must now look for addresses that use a compatible
            //               address family to the client socket.
            //               However, in the case of the <hostname,port> constructor
            //               we will have deferred creating the socket and will
            //               do that here instead.
            //               In addition, the redundant CheckForBroadcast call was
            //               removed here since it is called from Connect().
            //
            IPAddress[] addresses = Dns.GetHostAddresses(hostname);

            Exception lastex = null;
            Socket ipv6Socket = null;
            Socket ipv4Socket = null;

            try {
                if (m_ClientSocket == null){
                    if (Socket.OSSupportsIPv4){
                        ipv4Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    }
                    if (Socket.OSSupportsIPv6){
                        ipv6Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    }
                }


                foreach (IPAddress address in addresses)
                {
                    try {
                        if (m_ClientSocket == null)
                        {
                            //
                            // We came via the <hostname,port> constructor. Set the
                            // address family appropriately, create the socket and
                            // try to connect.
                            //
                            if (address.AddressFamily == AddressFamily.InterNetwork && ipv4Socket != null) {
                                ipv4Socket.Connect(address, port);
                                m_ClientSocket = ipv4Socket;
                                if (ipv6Socket != null)
                                    ipv6Socket.Close();
                            }
                            else if (ipv6Socket != null) {
                                ipv6Socket.Connect(address, port);
                                m_ClientSocket = ipv6Socket;
                                if (ipv4Socket != null)
                                    ipv4Socket.Close();
                            }


                            m_Family = address.AddressFamily;
                            m_Active = true;
                            break;
                        }
                        else if (address.AddressFamily == m_Family) {
                            //
                            // Only use addresses with a matching family
                            //
                            Connect(new IPEndPoint(address, port));
                            m_Active = true;
                            break;
                        }
                    }
                    catch ( Exception ex )
                    {
                        if (NclUtilities.IsFatal(ex)) {
                            throw;
                        }
                        lastex = ex;
                    }
                }
            }

            catch (Exception ex){
                if (NclUtilities.IsFatal(ex)) {
                    throw;
                }
                lastex = ex;
            }
            finally
            {
                //cleanup temp sockets if failed
                //main socket gets closed when tcpclient gets closed

                //did we connect?
                if (!m_Active) {
                    if (ipv6Socket != null){
                        ipv6Socket.Close();
                    }

                    if (ipv4Socket != null){
                        ipv4Socket.Close();
                    }


                    //
                    // The connect failed - rethrow the last error we had
                    //
                    if (lastex != null)
                        throw lastex;
                    else
                        throw new SocketException(SocketError.NotConnected);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Establishes a connection with the host at the specified address on the
        ///       specified port.
        ///    </para>
        /// </devdoc>
        public void Connect(IPAddress addr, int port) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (addr==null){
                throw new ArgumentNullException("addr");
            }
            if (!ValidationHelper.ValidateTcpPort(port)) {
                throw new ArgumentOutOfRangeException("port");
            }

            //
            // IPv6 Changes: Removed redundant call to CheckForBroadcast() since
            //               it is made in the real Connect() method.
            //
            IPEndPoint endPoint = new IPEndPoint(addr, port);

            Connect(endPoint);
        }

        /// <devdoc>
        ///    <para>
        ///       Establishes a connection to a remote end point.
        ///    </para>
        /// </devdoc>
        public void Connect(IPEndPoint endPoint) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (endPoint==null){
                throw new ArgumentNullException("endPoint");
            }
            //
            // IPv6 Changes: Actually, no changes but we might want to check for 
            //               compatible protocols here rather than push it down
            //               to WinSock.
            //
            CheckForBroadcast(endPoint.Address);
            Client.Connect(endPoint);
            m_Active = true;
        }
        
    
        private bool m_IsBroadcast;
        private void CheckForBroadcast(IPAddress ipAddress) {
            //
            // Here we check to see if the user is trying to use a Broadcast IP address
            // we only detect IPAddress.Broadcast (which is not the only Broadcast address)
            // and in that case we set SocketOptionName.Broadcast on the socket to allow its use.
            // if the user really wants complete control over Broadcast addresses he needs to
            // inherit from UdpClient and gain control over the Socket and do whatever is appropriate.
            //
            if (Client!=null && !m_IsBroadcast && ipAddress.IsBroadcast) {
                //
                // we need to set the Broadcast socket option.
                // note that, once we set the option on the Socket, we never reset it.
                //
                m_IsBroadcast = true;
                Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
        }



        /// <devdoc>
        ///    <para>
        ///       Sends a UDP datagram to the host at the remote end point.
        ///    </para>
        /// </devdoc>
        public int Send(byte[] dgram, int bytes, IPEndPoint endPoint) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (dgram==null){
                throw new ArgumentNullException("dgram");
            }
            if (m_Active && endPoint!=null) {
                //
                // Do not allow sending packets to arbitrary host when connected
                //
                throw new InvalidOperationException(SR.GetString(SR.net_udpconnected));
            }

            if (endPoint==null) {
                return Client.Send(dgram, 0, bytes, SocketFlags.None);
            }

            CheckForBroadcast(endPoint.Address);

            return Client.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
        }
        

        /// <devdoc>
        ///    <para>
        ///       Sends a UDP datagram to the specified port on the specified remote host.
        ///    </para>
        /// </devdoc>
        public int Send(byte[] dgram, int bytes, string hostname, int port) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (dgram==null){
                throw new ArgumentNullException("dgram");
            }
            if (m_Active && ((hostname != null) || (port != 0))) {
                //
                // Do not allow sending packets to arbitrary host when connected
                //
                throw new InvalidOperationException(SR.GetString(SR.net_udpconnected));
            }

            if (hostname==null || port==0) {
                return Client.Send(dgram, 0, bytes, SocketFlags.None);
            }

            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            
            int i=0;
            for (;i<addresses.Length && addresses[i].AddressFamily != m_Family; i++);

            if (addresses.Length == 0 || i == addresses.Length) {
                throw new ArgumentException(SR.GetString(SR.net_invalidAddressList), "hostname");
            }

            CheckForBroadcast(addresses[i]);
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[i], port);
            return Client.SendTo(dgram, 0, bytes, SocketFlags.None, ipEndPoint);
        }




        /// <devdoc>
        ///    <para>
        ///       Sends a UDP datagram to a
        ///       remote host.
        ///    </para>
        /// </devdoc>
        public int Send(byte[] dgram, int bytes) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (dgram==null){
                throw new ArgumentNullException("dgram");
            }
            if (!m_Active) {
                //
                // only allowed on connected socket
                //
                throw new InvalidOperationException(SR.GetString(SR.net_notconnected));
            }

            return Client.Send(dgram, 0, bytes, SocketFlags.None);

        }

    

        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (datagram==null){
                throw new ArgumentNullException("datagram");
            }

            if (bytes > datagram.Length || bytes < 0) {
                throw new ArgumentOutOfRangeException("bytes");
            }

            if (m_Active && endPoint!=null) {
                //
                // Do not allow sending packets to arbitrary host when connected
                //
                throw new InvalidOperationException(SR.GetString(SR.net_udpconnected));
            }

            if (endPoint==null) {
                return Client.BeginSend(datagram, 0, bytes, SocketFlags.None, requestCallback, state);
            }

            CheckForBroadcast(endPoint.Address);

            return Client.BeginSendTo(datagram, 0, bytes, SocketFlags.None, endPoint, requestCallback, state);
        }



        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state) {
            
            if (m_Active && ((hostname != null) || (port != 0))) {
                // Do not allow sending packets to arbitrary host when connected
                throw new InvalidOperationException(SR.GetString(SR.net_udpconnected));
            }

            IPEndPoint ipEndPoint = null;

            if (hostname!=null && port!=0) {

                IPAddress[] addresses = Dns.GetHostAddresses(hostname);

                int i=0; 
                for (;i<addresses.Length && addresses[i].AddressFamily != m_Family; i++);

                if (addresses.Length == 0 || i == addresses.Length) {
                    throw new ArgumentException(SR.GetString(SR.net_invalidAddressList), "hostname");
                }

                CheckForBroadcast(addresses[i]);
                ipEndPoint = new IPEndPoint(addresses[i], port);
            }
            return BeginSend(datagram, bytes, ipEndPoint,requestCallback, state);
        }



        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, AsyncCallback requestCallback, object state) {
            return BeginSend(datagram, bytes, null, requestCallback, state);
        }


        public int EndSend(IAsyncResult asyncResult){
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (m_Active){
                return Client.EndSend(asyncResult);
            }
            else{
                return Client.EndSendTo(asyncResult);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Returns a datagram sent by a server.
        ///    </para>
        /// </devdoc>
        public byte[] Receive(ref IPEndPoint remoteEP) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // this is a fix due to the nature of the ReceiveFrom() call and the 
            // ref parameter convention, we need to cast an IPEndPoint to it's base
            // class EndPoint and cast it back down to IPEndPoint. ugly but it works.
            //
            EndPoint tempRemoteEP;
            
            if ( m_Family == AddressFamily.InterNetwork ) {
                tempRemoteEP = IPEndPoint.Any;
            }
            else {
                tempRemoteEP = IPEndPoint.IPv6Any;
            }

            int received = Client.ReceiveFrom(m_Buffer, MaxUDPSize, 0 , ref tempRemoteEP);
            remoteEP = (IPEndPoint)tempRemoteEP;


            // because we don't return the actual length, we need to ensure the returned buffer
            // has the appropriate length.

            if (received < MaxUDPSize) {
                byte[] newBuffer = new byte[received];
                Buffer.BlockCopy(m_Buffer,0,newBuffer,0,received);
                return newBuffer;
            }
            return m_Buffer;
        }

        
        [HostProtection(ExternalThreading=true)]
        public IAsyncResult BeginReceive(AsyncCallback requestCallback, object state) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // this is a fix due to the nature of the ReceiveFrom() call and the 
            // ref parameter convention, we need to cast an IPEndPoint to it's base
            // class EndPoint and cast it back down to IPEndPoint. ugly but it works.
            //
            EndPoint tempRemoteEP;
            
            if ( m_Family == AddressFamily.InterNetwork ) {
                tempRemoteEP = IPEndPoint.Any;
            }
            else {
                tempRemoteEP = IPEndPoint.IPv6Any;
            }

            return Client.BeginReceiveFrom(m_Buffer, 0, MaxUDPSize, SocketFlags.None , ref tempRemoteEP, requestCallback, state);
        }


        public byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint remoteEP){
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            EndPoint tempRemoteEP;
            
            if ( m_Family == AddressFamily.InterNetwork ) {
                tempRemoteEP = IPEndPoint.Any;
            }
            else {
                tempRemoteEP = IPEndPoint.IPv6Any;
            }

            int received = Client.EndReceiveFrom(asyncResult,ref tempRemoteEP);
            remoteEP = (IPEndPoint)tempRemoteEP;
            
            // because we don't return the actual length, we need to ensure the returned buffer
            // has the appropriate length.
            
            if (received < MaxUDPSize) {
                byte[] newBuffer = new byte[received];
                Buffer.BlockCopy(m_Buffer,0,newBuffer,0,received);
                return newBuffer;
            }
            return m_Buffer;
        }



        ///     <devdoc>
        ///         <para>
        ///             Joins a multicast address group.
        ///         </para>
        ///     </devdoc>
        public void JoinMulticastGroup(IPAddress multicastAddr) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (multicastAddr==null){
                throw new ArgumentNullException("multicastAddr");
            }
 
            //
            // IPv6 Changes: we need to create the correct MulticastOption and
            //               must also check for address family compatibility
            //
            if ( multicastAddr.AddressFamily != m_Family ) {
                throw new ArgumentException(SR.GetString(SR.net_protocol_invalid_multicast_family, "UDP"), "multicastAddr");
            }

            if ( m_Family == AddressFamily.InterNetwork ) {
                MulticastOption mcOpt = new MulticastOption(multicastAddr);

                Client.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.AddMembership,
                    mcOpt );
            }
            else {
                IPv6MulticastOption mcOpt = new IPv6MulticastOption(multicastAddr);

                Client.SetSocketOption(
                    SocketOptionLevel.IPv6,
                    SocketOptionName.AddMembership,
                    mcOpt );
            }
        }



        public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        
            if ( m_Family != AddressFamily.InterNetwork ) {
                throw new SocketException(SocketError.OperationNotSupported);
            }

            MulticastOption mcOpt = new MulticastOption(multicastAddr,localAddress);
        
            Client.SetSocketOption(
               SocketOptionLevel.IP,
               SocketOptionName.AddMembership,
                mcOpt );
        }



        ///     <devdoc>
        ///         <para>
        ///             Joins an IPv6 multicast address group.
        ///         </para>
        ///     </devdoc>
        public void JoinMulticastGroup(int ifindex,IPAddress multicastAddr) {
            //
            // parameter validation
            //
            if ( m_CleanedUp ){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if ( multicastAddr==null ) {
                throw new ArgumentNullException("multicastAddr");
            }

            if ( ifindex < 0 ) {
                throw new ArgumentException(SR.GetString(SR.net_value_cannot_be_negative), "ifindex");
            }

            //
            // Ensure that this is an IPv6 client, otherwise throw WinSock 
            // Operation not supported socked exception.
            //
            if ( m_Family != AddressFamily.InterNetworkV6 ) {
                throw new SocketException(SocketError.OperationNotSupported);
            }

            IPv6MulticastOption mcOpt = new IPv6MulticastOption(multicastAddr,ifindex);

            Client.SetSocketOption(
                SocketOptionLevel.IPv6,
                SocketOptionName.AddMembership,
                mcOpt );
        }

        /// <devdoc>
        ///     <para>
        ///         Joins a multicast address group with the specified time to live (TTL).
        ///     </para>
        /// </devdoc>
        public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive) {
            //
            // parameter validation;
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (multicastAddr==null){
                throw new ArgumentNullException("multicastAddr");
            }
            if (!ValidationHelper.ValidateRange(timeToLive, 0, 255)) {
                throw new ArgumentOutOfRangeException("timeToLive");
            }

            //
            // join the Multicast Group
            //
            JoinMulticastGroup(multicastAddr);

            //
            // set Time To Live (TLL)
            //
            Client.SetSocketOption(
                (m_Family == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6,
                SocketOptionName.MulticastTimeToLive,
                timeToLive );
        }

        /// <devdoc>
        ///    <para>
        ///       Leaves a multicast address group.
        ///    </para>
        /// </devdoc>
        public void DropMulticastGroup(IPAddress multicastAddr) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (multicastAddr==null){
                throw new ArgumentNullException("multicastAddr");
            }

            //
            // IPv6 Changes: we need to create the correct MulticastOption and
            //               must also check for address family compatibility
            //
            if ( multicastAddr.AddressFamily != m_Family ) {
                throw new ArgumentException(SR.GetString(SR.net_protocol_invalid_multicast_family, "UDP"), "multicastAddr");
            }

            if ( m_Family == AddressFamily.InterNetwork ) {
                MulticastOption mcOpt = new MulticastOption(multicastAddr);

                Client.SetSocketOption(
                    SocketOptionLevel.IP,
                    SocketOptionName.DropMembership,
                    mcOpt );
            }
            else {
                IPv6MulticastOption mcOpt = new IPv6MulticastOption(multicastAddr);

                Client.SetSocketOption(
                    SocketOptionLevel.IPv6,
                    SocketOptionName.DropMembership,
                    mcOpt );
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Leaves an IPv6 multicast address group.
        ///    </para>
        /// </devdoc>
        public void DropMulticastGroup(IPAddress multicastAddr,int ifindex) {
            //
            // parameter validation
            //
            if (m_CleanedUp){
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if ( multicastAddr==null ) {
                throw new ArgumentNullException("multicastAddr");
            }

            if ( ifindex < 0 ) {
                throw new ArgumentException(SR.GetString(SR.net_value_cannot_be_negative), "ifindex");
            }

            //
            // Ensure that this is an IPv6 client, otherwise throw WinSock 
            // Operation not supported socked exception.
            //
            if ( m_Family != AddressFamily.InterNetworkV6 ) {
                throw new SocketException(SocketError.OperationNotSupported);
            }

            IPv6MulticastOption mcOpt = new IPv6MulticastOption(multicastAddr,ifindex);

            Client.SetSocketOption(
                SocketOptionLevel.IPv6,
                SocketOptionName.DropMembership,
                mcOpt );
        }

        //************* Task-based async public methods *************************
        [HostProtection(ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes)
        {
            return Task<int>.Factory.FromAsync(BeginSend, EndSend, datagram, bytes, null);
        }

        [HostProtection(ExternalThreading = true)]
        public Task<int> SendAsync(byte[] datagram, int bytes, IPEndPoint endPoint)
        {
            return Task<int>.Factory.FromAsync(BeginSend, EndSend, datagram, bytes, endPoint, null);
        }

        [HostProtection(ExternalThreading = true)]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "hostname",
            Justification="Original parameter spelling is preserved for consistency")]
        public Task<int> SendAsync(byte[] datagram, int bytes, string hostname, int port)
        {
            return Task<int>.Factory.FromAsync((callback, state) => BeginSend(datagram, bytes, hostname, port, callback, state), EndSend, null);
        }

        [HostProtection(ExternalThreading = true)]
        public Task<UdpReceiveResult> ReceiveAsync()
        {
            return Task<UdpReceiveResult>.Factory.FromAsync((callback, state) => BeginReceive(callback, state), (ar)=>
                {
                    IPEndPoint remoteEP = null;
                    Byte[] buffer = EndReceive(ar, ref remoteEP);
                    return new UdpReceiveResult(buffer, remoteEP);

                }, null);
        }


        private void createClientSocket() {
            //
            // common initialization code
            //
            // IPv6 Changes: Use the AddressFamily of this class rather than hardcode.
            //
            Client = new Socket(m_Family, SocketType.Dgram, ProtocolType.Udp);
        }

    } // class UdpClient



} // namespace System.Net.Sockets
