using System.Threading;
using System.Diagnostics;
using System.Net.Internals;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace System.Net.Sockets
{
#if MONO
    using Internals = System.Net;
#endif

    partial class Socket
    {
#region CoreFX Code

        // _rightEndPoint is null if the socket has not been bound.  Otherwise, it is any EndPoint of the
        // correct type (IPEndPoint, etc).
        internal EndPoint _rightEndPoint;
        internal EndPoint _remoteEndPoint;

        // These flags monitor if the socket was ever connected at any time and if it still is.
        private bool _isConnected;
        private bool _isDisconnected;

        // When the socket is created it will be in blocking mode. We'll only be able to Accept or Connect,
        // so we need to handle one of these cases at a time.
        private bool _willBlock = true; // Desired state of the socket from the user.
        private bool _willBlockInternal = true; // Actual win32 state of the socket.
        private bool _isListening = false;

        // Our internal state doesn't automatically get updated after a non-blocking connect
        // completes.  Keep track of whether we're doing a non-blocking connect, and make sure
        // to poll for the real state until we're done connecting.
        private bool _nonBlockingConnectInProgress;

        // Keep track of the kind of endpoint used to do a non-blocking connect, so we can set
        // it to _rightEndPoint when we discover we're connected.
        private EndPoint _nonBlockingConnectRightEndPoint;

        // These are constants initialized by constructor.
        private AddressFamily _addressFamily;
        private SocketType _socketType;
        private ProtocolType _protocolType;

        // These caches are one degree off of Socket since they're not used in the sync case/when disabled in config.
        private CacheSet _caches;

        private class CacheSet
        {
            internal CallbackClosure ConnectClosureCache;
            internal CallbackClosure AcceptClosureCache;
            internal CallbackClosure SendClosureCache;
            internal CallbackClosure ReceiveClosureCache;
        }

        // Bool marked true if the native socket option IP_PKTINFO or IPV6_PKTINFO has been set.
        private bool _receivingPacketInformation;

        private int _closeTimeout = Socket.DefaultCloseTimeout;

        // Called by the class to create a socket to accept an incoming request.
        private Socket(SafeCloseSocket fd)
        {
            // NOTE: If this ctor is ever made public/protected, this check will need
            // to be converted into a runtime exception.
            Debug.Assert(fd != null && !fd.IsInvalid);

            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);
            InitializeSockets();

#if MONO
            _handle = (SafeSocketHandle)fd;
#else
            _handle = fd;
#endif

            _addressFamily = Sockets.AddressFamily.Unknown;
            _socketType = Sockets.SocketType.Unknown;
            _protocolType = Sockets.ProtocolType.Unknown;
            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        // Gets the local end point.
        public EndPoint LocalEndPoint
        {
            get
            {
                if (CleanedUp)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                if (_nonBlockingConnectInProgress && Poll(0, SelectMode.SelectWrite))
                {
                    // Update the state if we've become connected after a non-blocking connect.
                    _isConnected = true;
                    _rightEndPoint = _nonBlockingConnectRightEndPoint;
                    _nonBlockingConnectInProgress = false;
                }

                if (_rightEndPoint == null)
                {
                    return null;
                }

                Internals.SocketAddress socketAddress = IPEndPointExtensions.Serialize(_rightEndPoint);

                // This may throw ObjectDisposedException.
                SocketError errorCode = SocketPal.GetSockName(
                    _handle,
                    socketAddress.Buffer,
                    ref socketAddress.InternalSize);

                if (errorCode != SocketError.Success)
                {
                    UpdateStatusAfterSocketErrorAndThrowException(errorCode);
                }

                return _rightEndPoint.Create(socketAddress);
            }
        }

        // Gets the remote end point.
        public EndPoint RemoteEndPoint
        {
            get
            {
                if (CleanedUp)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                if (_remoteEndPoint == null)
                {
                    if (_nonBlockingConnectInProgress && Poll(0, SelectMode.SelectWrite))
                    {
                        // Update the state if we've become connected after a non-blocking connect.
                        _isConnected = true;
                        _rightEndPoint = _nonBlockingConnectRightEndPoint;
                        _nonBlockingConnectInProgress = false;
                    }

                    if (_rightEndPoint == null)
                    {
                        return null;
                    }

                    Internals.SocketAddress socketAddress = IPEndPointExtensions.Serialize(_rightEndPoint);

                    // This may throw ObjectDisposedException.
                    SocketError errorCode = SocketPal.GetPeerName(
                        _handle,
                        socketAddress.Buffer,
                        ref socketAddress.InternalSize);

                    if (errorCode != SocketError.Success)
                    {
                        UpdateStatusAfterSocketErrorAndThrowException(errorCode);
                    }

                    try
                    {
                        _remoteEndPoint = _rightEndPoint.Create(socketAddress);
                    }
                    catch
                    {
                    }
                }

                return _remoteEndPoint;
            }
        }

        // Gets and sets the blocking mode of a socket.
        public bool Blocking
        {
            get
            {
                // Return the user's desired blocking behaviour (not the actual win32 state).
                return _willBlock;
            }
            set
            {
                if (CleanedUp)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"value:{value} willBlock:{_willBlock} willBlockInternal:{_willBlockInternal}");

                bool current;

                SocketError errorCode = InternalSetBlocking(value, out current);

                if (errorCode != SocketError.Success)
                {
                    UpdateStatusAfterSocketErrorAndThrowException(errorCode);
                }

                // The native call succeeded, update the user's desired state.
                _willBlock = current;
            }
        }

        // Associates a socket with an end point.
        public void Bind(EndPoint localEP)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, localEP);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // Validate input parameters.
            if (localEP == null)
            {
                throw new ArgumentNullException(nameof(localEP));
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"localEP:{localEP}");

            // Ask the EndPoint to generate a SocketAddress that we can pass down to native code.
            EndPoint endPointSnapshot = localEP;
            Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref endPointSnapshot);

            DoBind(endPointSnapshot, socketAddress);

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        internal void InternalBind(EndPoint localEP)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, localEP);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"localEP:{localEP}");

            if (localEP is DnsEndPoint)
            {
                NetEventSource.Fail(this, "Calling InternalBind with a DnsEndPoint, about to get NotImplementedException");
            }

            // Ask the EndPoint to generate a SocketAddress that we can pass down to native code.
            EndPoint endPointSnapshot = localEP;
            Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref endPointSnapshot);
            DoBind(endPointSnapshot, socketAddress);

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        private void DoBind(EndPoint endPointSnapshot, Internals.SocketAddress socketAddress)
        {
            // Mitigation for Blue Screen of Death (Win7, maybe others).
            IPEndPoint ipEndPoint = endPointSnapshot as IPEndPoint;
            if (!OSSupportsIPv4 && ipEndPoint != null && ipEndPoint.Address.IsIPv4MappedToIPv6)
            {
                UpdateStatusAfterSocketErrorAndThrowException(SocketError.InvalidArgument);
            }

            // This may throw ObjectDisposedException.
            SocketError errorCode = SocketPal.Bind(
                _handle,
                _protocolType,
                socketAddress.Buffer,
                socketAddress.Size);

#if TRACE_VERBOSE
            if (NetEventSource.IsEnabled)
            {
                try
                {
                    if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} Interop.Winsock.bind returns errorCode:{errorCode}");
                }
                catch (ObjectDisposedException) { }
            }
#endif

            // Throw an appropriate SocketException if the native call fails.
            if (errorCode != SocketError.Success)
            {
                UpdateStatusAfterSocketErrorAndThrowException(errorCode);
            }

            if (_rightEndPoint == null)
            {
                // Save a copy of the EndPoint so we can use it for Create().
                _rightEndPoint = endPointSnapshot;
            }

#if MONO
            is_bound = true;
#endif
        }

        // Establishes a connection to a remote system.
        public void Connect(EndPoint remoteEP)
        {
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // Validate input parameters.
            if (remoteEP == null)
            {
                throw new ArgumentNullException(nameof(remoteEP));
            }

            if (_isDisconnected)
            {
                throw new InvalidOperationException(SR.net_sockets_disconnectedConnect);
            }

            if (_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustnotlisten);
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            ValidateBlockingMode();

            if (NetEventSource.IsEnabled)
            {
                if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"DST:{remoteEP}");
            }

            DnsEndPoint dnsEP = remoteEP as DnsEndPoint;
            if (dnsEP != null)
            {
                ValidateForMultiConnect(isMultiEndpoint: true); // needs to come before CanTryAddressFamily call

                if (dnsEP.AddressFamily != AddressFamily.Unspecified && !CanTryAddressFamily(dnsEP.AddressFamily))
                {
                    throw new NotSupportedException(SR.net_invalidversion);
                }

                Connect(dnsEP.Host, dnsEP.Port);
                return;
            }

            ValidateForMultiConnect(isMultiEndpoint: false);

            EndPoint endPointSnapshot = remoteEP;
            Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref endPointSnapshot);

            if (!Blocking)
            {
                _nonBlockingConnectRightEndPoint = endPointSnapshot;
                _nonBlockingConnectInProgress = true;
            }

            DoConnect(endPointSnapshot, socketAddress);
        }

        public void Connect(IPAddress address, int port)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, address);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            ValidateForMultiConnect(isMultiEndpoint: false); // needs to come before CanTryAddressFamily call

            if (!CanTryAddressFamily(address.AddressFamily))
            {
                throw new NotSupportedException(SR.net_invalidversion);
            }

            IPEndPoint remoteEP = new IPEndPoint(address, port);
            Connect(remoteEP);
            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        public void Connect(string host, int port)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, host);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (_addressFamily != AddressFamily.InterNetwork && _addressFamily != AddressFamily.InterNetworkV6)
            {
                throw new NotSupportedException(SR.net_invalidversion);
            }

            // No need to call ValidateForMultiConnect(), as the validation
            // will be handled by the delegated Connect overloads.

            IPAddress parsedAddress;
            if (IPAddress.TryParse(host, out parsedAddress))
            {
                Connect(parsedAddress, port);
            }
            else
            {
                IPAddress[] addresses = Dns.GetHostAddressesAsync(host).GetAwaiter().GetResult();
                Connect(addresses, port);
            }

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        public void Connect(IPAddress[] addresses, int port)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, addresses);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (addresses == null)
            {
                throw new ArgumentNullException(nameof(addresses));
            }
            if (addresses.Length == 0)
            {
                throw new ArgumentException(SR.net_sockets_invalid_ipaddress_length, nameof(addresses));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (_addressFamily != AddressFamily.InterNetwork && _addressFamily != AddressFamily.InterNetworkV6)
            {
                throw new NotSupportedException(SR.net_invalidversion);
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            ValidateForMultiConnect(isMultiEndpoint: true); // needs to come before CanTryAddressFamily call

            ExceptionDispatchInfo lastex = null;
            foreach (IPAddress address in addresses)
            {
                if (CanTryAddressFamily(address.AddressFamily))
                {
                    try
                    {
                        Connect(new IPEndPoint(address, port));
                        lastex = null;
                        break;
                    }
                    catch (Exception ex) when (!ExceptionCheck.IsFatal(ex))
                    {
                        lastex = ExceptionDispatchInfo.Capture(ex);
                    }
                }
            }

            lastex?.Throw();

            // If we're not connected, then we didn't get a valid ipaddress in the list.
            if (!Connected)
            {
                throw new ArgumentException(SR.net_invalidAddressList, nameof(addresses));
            }

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        public void Close()
        {
            if (NetEventSource.IsEnabled)
            {
                NetEventSource.Enter(this);
                NetEventSource.Info(this, $"timeout = {_closeTimeout}");
            }

            Dispose();

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        public void Close(int timeout)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, timeout);
            if (timeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            _closeTimeout = timeout;

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"timeout = {_closeTimeout}");

            Dispose();

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, timeout);
        }

        // Places a socket in a listening state.
        public void Listen(int backlog)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, backlog);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"backlog:{backlog}");

            // This may throw ObjectDisposedException.
            SocketError errorCode = SocketPal.Listen(_handle, backlog);

#if TRACE_VERBOSE
            if (NetEventSource.IsEnabled)
            {
                try
                {
                    if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} Interop.Winsock.listen returns errorCode:{errorCode}");
                }
                catch (ObjectDisposedException) { }
            }
#endif

            // Throw an appropriate SocketException if the native call fails.
            if (errorCode != SocketError.Success)
            {
                UpdateStatusAfterSocketErrorAndThrowException(errorCode);
            }
            _isListening = true;
            if (NetEventSource.IsEnabled) NetEventSource.Exit(this);
        }

        // Creates a new Sockets.Socket instance to handle an incoming connection.
        public Socket Accept()
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);

            // Validate input parameters.

            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (_rightEndPoint == null)
            {
                throw new InvalidOperationException(SR.net_sockets_mustbind);
            }

            if (!_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustlisten);
            }

            if (_isDisconnected)
            {
                throw new InvalidOperationException(SR.net_sockets_disconnectedAccept);
            }

            ValidateBlockingMode();
            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint}");

            Internals.SocketAddress socketAddress = IPEndPointExtensions.Serialize(_rightEndPoint);

            // This may throw ObjectDisposedException.
            SafeCloseSocket acceptedSocketHandle;
            SocketError errorCode = SocketPal.Accept(
                _handle,
                socketAddress.Buffer,
                ref socketAddress.InternalSize,
                out acceptedSocketHandle);

            // Throw an appropriate SocketException if the native call fails.
            if (errorCode != SocketError.Success)
            {
                Debug.Assert(acceptedSocketHandle.IsInvalid);
                UpdateStatusAfterSocketErrorAndThrowException(errorCode);
            }

            Debug.Assert(!acceptedSocketHandle.IsInvalid);

            Socket socket = CreateAcceptSocket(acceptedSocketHandle, _rightEndPoint.Create(socketAddress));
            if (NetEventSource.IsEnabled)
            {
                NetEventSource.Accepted(socket, socket.RemoteEndPoint, socket.LocalEndPoint);
                NetEventSource.Exit(this, socket);
            }
            return socket;
        }

        // Receives data from a connected socket.
        public int Receive(byte[] buffer, int size, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, size, socketFlags);
        }

        public int Receive(byte[] buffer, SocketFlags socketFlags)
        {
            return Receive(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags);
        }

        public int Receive(byte[] buffer)
        {
            return Receive(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None);
        }

        // Receives data from a connected socket into a specific location of the receive buffer.
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            SocketError errorCode;
            int bytesTransferred = Receive(buffer, offset, size, socketFlags, out errorCode);
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((int)errorCode);
            }
            return bytesTransferred;
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // Validate input parameters.
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            ValidateBlockingMode();
            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} DST:{RemoteEndPoint} size:{size}");

            int bytesTransferred;
            errorCode = SocketPal.Receive(_handle, buffer, offset, size, socketFlags, out bytesTransferred);

            if (errorCode != SocketError.Success)
            {
                // Update the internal state of this socket according to the error before throwing.
                UpdateStatusAfterSocketError(errorCode);
                if (NetEventSource.IsEnabled)
                {
                    NetEventSource.Error(this, new SocketException((int)errorCode));
                    NetEventSource.Exit(this, 0);
                }
                return 0;
            }

            if (NetEventSource.IsEnabled)
            {
#if TRACE_VERBOSE
                try
                {
                    if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} DST:{RemoteEndPoint} bytesTransferred:{bytesTransferred}");
                }
                catch (ObjectDisposedException) { }
#endif
                NetEventSource.DumpBuffer(this, buffer, offset, bytesTransferred);
                NetEventSource.Exit(this, bytesTransferred);
            }

            return bytesTransferred;
        }

        public int Receive(Span<byte> buffer) => Receive(buffer, SocketFlags.None);

        public int Receive(Span<byte> buffer, SocketFlags socketFlags)
        {
            int bytesTransferred = Receive(buffer, socketFlags, out SocketError errorCode);
            return errorCode == SocketError.Success ?
                bytesTransferred :
                throw new SocketException((int)errorCode);
        }

        public int Receive(Span<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);
            if (CleanedUp) throw new ObjectDisposedException(GetType().FullName);
            ValidateBlockingMode();

            int bytesTransferred;
            errorCode = SocketPal.Receive(_handle, buffer, socketFlags, out bytesTransferred);

            if (errorCode != SocketError.Success)
            {
                UpdateStatusAfterSocketError(errorCode);
                if (NetEventSource.IsEnabled) NetEventSource.Error(this, new SocketException((int)errorCode));
                bytesTransferred = 0;
            }

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, bytesTransferred);
            return bytesTransferred;
        }

        public int Receive(IList<ArraySegment<byte>> buffers)
        {
            return Receive(buffers, SocketFlags.None);
        }

        public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
        {
            SocketError errorCode;
            int bytesTransferred = Receive(buffers, socketFlags, out errorCode);
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((int)errorCode);
            }
            return bytesTransferred;
        }

        public int Receive(IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (buffers == null)
            {
                throw new ArgumentNullException(nameof(buffers));
            }

            if (buffers.Count == 0)
            {
                throw new ArgumentException(SR.Format(SR.net_sockets_zerolist, nameof(buffers)), nameof(buffers));
            }


            ValidateBlockingMode();
            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} DST:{RemoteEndPoint}");

            int bytesTransferred;
            errorCode = SocketPal.Receive(_handle, buffers, ref socketFlags, out bytesTransferred);

#if TRACE_VERBOSE
            if (NetEventSource.IsEnabled)
            {
                try
                {
                    if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} DST:{RemoteEndPoint} Interop.Winsock.send returns errorCode:{errorCode} bytesTransferred:{bytesTransferred}");
                }
                catch (ObjectDisposedException) { }
            }
#endif

            if (errorCode != SocketError.Success)
            {
                // Update the internal state of this socket according to the error before throwing.
                UpdateStatusAfterSocketError(errorCode);
                if (NetEventSource.IsEnabled)
                {
                    NetEventSource.Error(this, new SocketException((int)errorCode));
                    NetEventSource.Exit(this, 0);
                }
                return 0;
            }

#if TRACE_VERBOSE
            if (NetEventSource.IsEnabled)
            {
                try
                {
                    if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} DST:{RemoteEndPoint} bytesTransferred:{bytesTransferred}");
                }
                catch (ObjectDisposedException) { }
            }
#endif

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, bytesTransferred);

            return bytesTransferred;
        }

        // Receives a datagram into a specific location in the data buffer and stores
        // the end point.
        public int ReceiveMessageFrom(byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException(nameof(remoteEP));
            }
            if (!CanTryAddressFamily(remoteEP.AddressFamily))
            {
                throw new ArgumentException(SR.Format(SR.net_InvalidEndPointAddressFamily, remoteEP.AddressFamily, _addressFamily), nameof(remoteEP));
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            if (_rightEndPoint == null)
            {
                throw new InvalidOperationException(SR.net_sockets_mustbind);
            }

            SocketPal.CheckDualModeReceiveSupport(this);
            ValidateBlockingMode();

            // We don't do a CAS demand here because the contents of remoteEP aren't used by
            // WSARecvMsg; all that matters is that we generate a unique-to-this-call SocketAddress
            // with the right address family.
            EndPoint endPointSnapshot = remoteEP;
            Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref endPointSnapshot);

            // Save a copy of the original EndPoint.
            Internals.SocketAddress socketAddressOriginal = IPEndPointExtensions.Serialize(endPointSnapshot);

            SetReceivingPacketInformation();

            Internals.SocketAddress receiveAddress;
            int bytesTransferred;
            SocketError errorCode = SocketPal.ReceiveMessageFrom(this, _handle, buffer, offset, size, ref socketFlags, socketAddress, out receiveAddress, out ipPacketInformation, out bytesTransferred);

            // Throw an appropriate SocketException if the native call fails.
            if (errorCode != SocketError.Success && errorCode != SocketError.MessageSize)
            {
                UpdateStatusAfterSocketErrorAndThrowException(errorCode);
            }

            if (!socketAddressOriginal.Equals(receiveAddress))
            {
                try
                {
                    remoteEP = endPointSnapshot.Create(receiveAddress);
                }
                catch
                {
                }
                if (_rightEndPoint == null)
                {
                    // Save a copy of the EndPoint so we can use it for Create().
                    _rightEndPoint = endPointSnapshot;
                }
            }

            if (NetEventSource.IsEnabled) NetEventSource.Error(this, errorCode);
            return bytesTransferred;
        }

        // Receives a datagram into a specific location in the data buffer and stores
        // the end point.
        public int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // Validate input parameters.
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (remoteEP == null)
            {
                throw new ArgumentNullException(nameof(remoteEP));
            }
            if (!CanTryAddressFamily(remoteEP.AddressFamily))
            {
                throw new ArgumentException(SR.Format(SR.net_InvalidEndPointAddressFamily,
                    remoteEP.AddressFamily, _addressFamily), nameof(remoteEP));
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (size < 0 || size > buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            if (_rightEndPoint == null)
            {
                throw new InvalidOperationException(SR.net_sockets_mustbind);
            }

            SocketPal.CheckDualModeReceiveSupport(this);

            ValidateBlockingMode();
            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC{LocalEndPoint} size:{size} remoteEP:{remoteEP}");

            // We don't do a CAS demand here because the contents of remoteEP aren't used by
            // WSARecvFrom; all that matters is that we generate a unique-to-this-call SocketAddress
            // with the right address family.
            EndPoint endPointSnapshot = remoteEP;
            Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref endPointSnapshot);
            Internals.SocketAddress socketAddressOriginal = IPEndPointExtensions.Serialize(endPointSnapshot);

            int bytesTransferred;
            SocketError errorCode = SocketPal.ReceiveFrom(_handle, buffer, offset, size, socketFlags, socketAddress.Buffer, ref socketAddress.InternalSize, out bytesTransferred);

            // If the native call fails we'll throw a SocketException.
            SocketException socketException = null;
            if (errorCode != SocketError.Success)
            {
                socketException = new SocketException((int)errorCode);
                UpdateStatusAfterSocketError(socketException);
                if (NetEventSource.IsEnabled) NetEventSource.Error(this, socketException);

                if (socketException.SocketErrorCode != SocketError.MessageSize)
                {
                    throw socketException;
                }
            }

            if (!socketAddressOriginal.Equals(socketAddress))
            {
                try
                {
                    remoteEP = endPointSnapshot.Create(socketAddress);
                }
                catch
                {
                }
                if (_rightEndPoint == null)
                {
                    // Save a copy of the EndPoint so we can use it for Create().
                    _rightEndPoint = endPointSnapshot;
                }
            }

            if (socketException != null)
            {
                throw socketException;
            }

            if (NetEventSource.IsEnabled)
            {
                NetEventSource.DumpBuffer(this, buffer, offset, size);
                NetEventSource.Exit(this, bytesTransferred);
            }
            return bytesTransferred;
        }

        // Receives a datagram and stores the source end point.
        public int ReceiveFrom(byte[] buffer, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, size, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, SocketFlags socketFlags, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, socketFlags, ref remoteEP);
        }

        public int ReceiveFrom(byte[] buffer, ref EndPoint remoteEP)
        {
            return ReceiveFrom(buffer, 0, buffer != null ? buffer.Length : 0, SocketFlags.None, ref remoteEP);
        }

        // Routine Description:
        //
        //    BeginConnect - Does an async connect.
        //
        // Arguments:
        //
        //    remoteEP - status line that we wish to parse
        //    Callback - Async Callback Delegate that is called upon Async Completion
        //    State - State used to track callback, set by caller, not required
        //
        // Return Value:
        //
        //    IAsyncResult - Async result used to retrieve result
        public IAsyncResult BeginConnect(EndPoint remoteEP, AsyncCallback callback, object state)
        {
            // Validate input parameters.
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, remoteEP);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (remoteEP == null)
            {
                throw new ArgumentNullException(nameof(remoteEP));
            }

            if (_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustnotlisten);
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }


            DnsEndPoint dnsEP = remoteEP as DnsEndPoint;
            if (dnsEP != null)
            {
                ValidateForMultiConnect(isMultiEndpoint: true); // needs to come before CanTryAddressFamily call

                if (dnsEP.AddressFamily != AddressFamily.Unspecified && !CanTryAddressFamily(dnsEP.AddressFamily))
                {
                    throw new NotSupportedException(SR.net_invalidversion);
                }

                return BeginConnect(dnsEP.Host, dnsEP.Port, callback, state);
            }

            ValidateForMultiConnect(isMultiEndpoint: false);
            return UnsafeBeginConnect(remoteEP, callback, state, flowContext:true);
        }

        private bool CanUseConnectEx(EndPoint remoteEP)
        {
            return (_socketType == SocketType.Stream) &&
                (_rightEndPoint != null || remoteEP.GetType() == typeof(IPEndPoint));
        }

        public SocketInformation DuplicateAndClose(int targetProcessId)
        {
            //
            // On Windows, we cannot duplicate a socket that is bound to an IOCP.  In this implementation, we *only*
            // support IOCPs, so this will not work.
            //
            // On Unix, duplication of a socket into an arbitrary process is not supported at all.
            //
            throw new PlatformNotSupportedException(SR.net_sockets_duplicateandclose_notsupported);
        }

        internal IAsyncResult UnsafeBeginConnect(EndPoint remoteEP, AsyncCallback callback, object state, bool flowContext = false)
        {
            if (CanUseConnectEx(remoteEP))
            {
                return BeginConnectEx(remoteEP, flowContext, callback, state);
            }

            EndPoint endPointSnapshot = remoteEP;
            var asyncResult = new ConnectAsyncResult(this, endPointSnapshot, state, callback);

            // For connectionless protocols, Connect is not an I/O call.
            Connect(remoteEP);
            asyncResult.FinishPostingAsyncOp();

            // Synchronously complete the I/O and call the user's callback.
            asyncResult.InvokeCallback();
            return asyncResult;
        }

        public IAsyncResult BeginConnect(string host, int port, AsyncCallback requestCallback, object state)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, host);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (_addressFamily != AddressFamily.InterNetwork && _addressFamily != AddressFamily.InterNetworkV6)
            {
                throw new NotSupportedException(SR.net_invalidversion);
            }

            if (_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustnotlisten);
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            IPAddress parsedAddress;
            if (IPAddress.TryParse(host, out parsedAddress))
            {
                IAsyncResult r = BeginConnect(parsedAddress, port, requestCallback, state);
                if (NetEventSource.IsEnabled) NetEventSource.Exit(this, r);
                return r;
            }

            ValidateForMultiConnect(isMultiEndpoint: true);

            // Here, want to flow the context.  No need to lock.
            MultipleAddressConnectAsyncResult result = new MultipleAddressConnectAsyncResult(null, port, this, state, requestCallback);
            result.StartPostingAsyncOp(false);

            IAsyncResult dnsResult = Dns.BeginGetHostAddresses(host, new AsyncCallback(DnsCallback), result);
            if (dnsResult.CompletedSynchronously)
            {
                if (DoDnsCallback(dnsResult, result))
                {
                    result.InvokeCallback();
                }
            }

            // Done posting.
            result.FinishPostingAsyncOp(ref Caches.ConnectClosureCache);

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, result);
            return result;
        }

        public IAsyncResult BeginConnect(IPAddress address, int port, AsyncCallback requestCallback, object state)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, address);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            ValidateForMultiConnect(isMultiEndpoint: false); // needs to be called before CanTryAddressFamily

            if (!CanTryAddressFamily(address.AddressFamily))
            {
                throw new NotSupportedException(SR.net_invalidversion);
            }

            IAsyncResult result = BeginConnect(new IPEndPoint(address, port), requestCallback, state);
            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, result);
            return result;
        }

        public IAsyncResult BeginConnect(IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, addresses);
            if (CleanedUp)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (addresses == null)
            {
                throw new ArgumentNullException(nameof(addresses));
            }
            if (addresses.Length == 0)
            {
                throw new ArgumentException(SR.net_invalidAddressList, nameof(addresses));
            }
            if (!TcpValidationHelpers.ValidatePortNumber(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            if (_addressFamily != AddressFamily.InterNetwork && _addressFamily != AddressFamily.InterNetworkV6)
            {
                throw new NotSupportedException(SR.net_invalidversion);
            }

            if (_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustnotlisten);
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            ValidateForMultiConnect(isMultiEndpoint: true);

            // Set up the result to capture the context.  No need for a lock.
            MultipleAddressConnectAsyncResult result = new MultipleAddressConnectAsyncResult(addresses, port, this, state, requestCallback);
            result.StartPostingAsyncOp(false);

            if (DoMultipleAddressConnectCallback(PostOneBeginConnect(result), result))
            {
                // If the call completes synchronously, invoke the callback from here.
                result.InvokeCallback();
            }

            // Finished posting async op.  Possibly will call callback.
            result.FinishPostingAsyncOp(ref Caches.ConnectClosureCache);

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, result);
            return result;
        }

        public bool AcceptAsync(SocketAsyncEventArgs e)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, e);

            if (CleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            if (e.HasMultipleBuffers)
            {
                throw new ArgumentException(SR.net_multibuffernotsupported, "BufferList");
            }
            if (_rightEndPoint == null)
            {
                throw new InvalidOperationException(SR.net_sockets_mustbind);
            }
            if (!_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustlisten);
            }

            // Handle AcceptSocket property.
            SafeCloseSocket acceptHandle;
            e.AcceptSocket = GetOrCreateAcceptSocket(e.AcceptSocket, true, "AcceptSocket", out acceptHandle);

            // Prepare for and make the native call.
            e.StartOperationCommon(this, SocketAsyncOperation.Accept);
            e.StartOperationAccept();
            SocketError socketError = SocketError.Success;
            try
            {
                socketError = e.DoOperationAccept(this, _handle, acceptHandle);
            }
            catch
            {
                // Clear in-use flag on event args object.
                e.Complete();
                throw;
            }

            bool pending = (socketError == SocketError.IOPending);
            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, pending);
            return pending;
        }

        public bool ConnectAsync(SocketAsyncEventArgs e)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, e);
            bool pending;

            if (CleanedUp)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            if (e.HasMultipleBuffers)
            {
                throw new ArgumentException(SR.net_multibuffernotsupported, "BufferList");
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEP");
            }
            if (_isListening)
            {
                throw new InvalidOperationException(SR.net_sockets_mustnotlisten);
            }

            if (_isConnected)
            {
                throw new SocketException((int)SocketError.IsConnected);
            }

            // Prepare SocketAddress.
            EndPoint endPointSnapshot = e.RemoteEndPoint;
            DnsEndPoint dnsEP = endPointSnapshot as DnsEndPoint;

            if (dnsEP != null)
            {
                if (NetEventSource.IsEnabled) NetEventSource.ConnectedAsyncDns(this);

                ValidateForMultiConnect(isMultiEndpoint: true); // needs to come before CanTryAddressFamily call

                if (dnsEP.AddressFamily != AddressFamily.Unspecified && !CanTryAddressFamily(dnsEP.AddressFamily))
                {
                    throw new NotSupportedException(SR.net_invalidversion);
                }

                MultipleConnectAsync multipleConnectAsync = new SingleSocketMultipleConnectAsync(this, true);

                e.StartOperationCommon(this, SocketAsyncOperation.Connect);
                e.StartOperationConnect(multipleConnectAsync);

                pending = multipleConnectAsync.StartConnectAsync(e, dnsEP);
            }
            else
            {
                ValidateForMultiConnect(isMultiEndpoint: false); // needs to come before CanTryAddressFamily call

                // Throw if remote address family doesn't match socket.
                if (!CanTryAddressFamily(e.RemoteEndPoint.AddressFamily))
                {
                    throw new NotSupportedException(SR.net_invalidversion);
                }

                e._socketAddress = SnapshotAndSerialize(ref endPointSnapshot);

                // Do wildcard bind if socket not bound.
                if (_rightEndPoint == null)
                {
                    if (endPointSnapshot.AddressFamily == AddressFamily.InterNetwork)
                    {
                        InternalBind(new IPEndPoint(IPAddress.Any, 0));
                    }
                    else if (endPointSnapshot.AddressFamily != AddressFamily.Unix)
                    {
                        InternalBind(new IPEndPoint(IPAddress.IPv6Any, 0));
                    }
                }

                // Save the old RightEndPoint and prep new RightEndPoint.
                EndPoint oldEndPoint = _rightEndPoint;
                if (_rightEndPoint == null)
                {
                    _rightEndPoint = endPointSnapshot;
                }

                // Prepare for the native call.
                e.StartOperationCommon(this, SocketAsyncOperation.Connect);
                e.StartOperationConnect();

                // Make the native call.
                SocketError socketError = SocketError.Success;
                try
                {
                    socketError = e.DoOperationConnect(this, _handle);
                }
                catch
                {
                    _rightEndPoint = oldEndPoint;

                    // Clear in-use flag on event args object.
                    e.Complete();
                    throw;
                }

                pending = (socketError == SocketError.IOPending);
            }

            if (NetEventSource.IsEnabled) NetEventSource.Exit(this, pending);
            return pending;
        }

        public static bool ConnectAsync(SocketType socketType, ProtocolType protocolType, SocketAsyncEventArgs e)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(null);
            bool pending;

            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            if (e.HasMultipleBuffers)
            {
                throw new ArgumentException(SR.net_multibuffernotsupported, "BufferList");
            }
            if (e.RemoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            EndPoint endPointSnapshot = e.RemoteEndPoint;
            DnsEndPoint dnsEP = endPointSnapshot as DnsEndPoint;

            if (dnsEP != null)
            {
                Socket attemptSocket = null;
                MultipleConnectAsync multipleConnectAsync = null;
                if (dnsEP.AddressFamily == AddressFamily.Unspecified)
                {
                    // This is the only *Connect* API that fully supports multiple endpoint attempts, as it's responsible
                    // for creating each Socket instance and can create one per attempt.
                    multipleConnectAsync = new DualSocketMultipleConnectAsync(socketType, protocolType);
#pragma warning restore
                }
                else
                {
                    attemptSocket = new Socket(dnsEP.AddressFamily, socketType, protocolType);
                    multipleConnectAsync = new SingleSocketMultipleConnectAsync(attemptSocket, false);
                }

                e.StartOperationCommon(attemptSocket, SocketAsyncOperation.Connect);
                e.StartOperationConnect(multipleConnectAsync);

                pending = multipleConnectAsync.StartConnectAsync(e, dnsEP);
            }
            else
            {
                Socket attemptSocket = new Socket(endPointSnapshot.AddressFamily, socketType, protocolType);
                pending = attemptSocket.ConnectAsync(e);
            }

            if (NetEventSource.IsEnabled) NetEventSource.Exit(null, pending);
            return pending;
        }

        public static void CancelConnectAsync(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            e.CancelConnectAsync();
        }

        // Set the socket option to begin receiving packet information if it has not been
        // set for this socket previously.
        internal void SetReceivingPacketInformation()
        {
            if (!_receivingPacketInformation)
            {
                // DualMode: When bound to IPv6Any you must enable both socket options.
                // When bound to an IPv4 mapped IPv6 address you must enable the IPv4 socket option.
                IPEndPoint ipEndPoint = _rightEndPoint as IPEndPoint;
                IPAddress boundAddress = (ipEndPoint != null ? ipEndPoint.Address : null);
                Debug.Assert(boundAddress != null, "Not Bound");
                if (_addressFamily == AddressFamily.InterNetwork)
                {
                    SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                }

                if ((boundAddress != null && IsDualMode && (boundAddress.IsIPv4MappedToIPv6 || boundAddress.Equals(IPAddress.IPv6Any))))
                {
                    SocketPal.SetReceivingDualModeIPv4PacketInformation(this);
                }

                if (_addressFamily == AddressFamily.InterNetworkV6
                    && (boundAddress == null || !boundAddress.IsIPv4MappedToIPv6))
                {
                    SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.PacketInformation, true);
                }

                _receivingPacketInformation = true;
            }
        }

        // This method will ignore failures, but returns the win32
        // error code, and will update internal state on success.
        private SocketError InternalSetBlocking(bool desired, out bool current)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, $"desired:{desired} willBlock:{_willBlock} willBlockInternal:{_willBlockInternal}");

            if (CleanedUp)
            {
                if (NetEventSource.IsEnabled) NetEventSource.Exit(this, "ObjectDisposed");
                current = _willBlock;
                return SocketError.Success;
            }

            // Can we avoid this call if willBlockInternal is already correct?
            bool willBlock = false;
            SocketError errorCode;
            try
            {
                errorCode = SocketPal.SetBlocking(_handle, desired, out willBlock);
            }
            catch (ObjectDisposedException)
            {
                errorCode = SocketError.NotSocket;
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"Interop.Winsock.ioctlsocket returns errorCode:{errorCode}");

            // We will update only internal state but only on successfull win32 call
            // so if the native call fails, the state will remain the same.
            if (errorCode == SocketError.Success)
            {
                _willBlockInternal = willBlock;
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"errorCode:{errorCode} willBlock:{_willBlock} willBlockInternal:{_willBlockInternal}");

            current = _willBlockInternal;
            return errorCode;
        }

        // This method ignores all failures.
        internal void InternalSetBlocking(bool desired)
        {
            bool current;
            InternalSetBlocking(desired, out current);
        }

        private CacheSet Caches
        {
            get
            {
                if (_caches == null)
                {
                    // It's not too bad if extra of these are created and lost.
                    _caches = new CacheSet();
                }
                return _caches;
            }
        }

        internal static void GetIPProtocolInformation(AddressFamily addressFamily, Internals.SocketAddress socketAddress, out bool isIPv4, out bool isIPv6)
        {
            bool isIPv4MappedToIPv6 = socketAddress.Family == AddressFamily.InterNetworkV6 && socketAddress.GetIPAddress().IsIPv4MappedToIPv6;
            isIPv4 = addressFamily == AddressFamily.InterNetwork || isIPv4MappedToIPv6; // DualMode
            isIPv6 = addressFamily == AddressFamily.InterNetworkV6;
        }

        internal static int GetAddressSize(EndPoint endPoint)
        {
            AddressFamily fam = endPoint.AddressFamily;
            return
                fam == AddressFamily.InterNetwork ? SocketAddressPal.IPv4AddressSize :
                fam == AddressFamily.InterNetworkV6 ? SocketAddressPal.IPv6AddressSize :
                endPoint.Serialize().Size;
        }

        private Internals.SocketAddress SnapshotAndSerialize(ref EndPoint remoteEP)
        {
            if (remoteEP is IPEndPoint ipSnapshot)
            {
                // Snapshot to avoid external tampering and malicious derivations if IPEndPoint.
                ipSnapshot = ipSnapshot.Snapshot();

                // DualMode: return an IPEndPoint mapped to an IPv6 address.
                remoteEP = RemapIPEndPoint(ipSnapshot);
            }
            else if (remoteEP is DnsEndPoint)
            {
                throw new ArgumentException(SR.Format(SR.net_sockets_invalid_dnsendpoint, nameof(remoteEP)), nameof(remoteEP));
            }

            return IPEndPointExtensions.Serialize(remoteEP);
        }


        // DualMode: automatically re-map IPv4 addresses to IPv6 addresses.
        private IPEndPoint RemapIPEndPoint(IPEndPoint input)
        {
            if (input.AddressFamily == AddressFamily.InterNetwork && IsDualMode)
            {
                return new IPEndPoint(input.Address.MapToIPv6(), input.Port);
            }
            return input;
        }

        private void DoConnect(EndPoint endPointSnapshot, Internals.SocketAddress socketAddress)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this, endPointSnapshot);

            SocketError errorCode = SocketPal.Connect(_handle, socketAddress.Buffer, socketAddress.Size);
#if TRACE_VERBOSE
            if (NetEventSource.IsEnabled)
            {
                try
                {
                    if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"SRC:{LocalEndPoint} DST:{RemoteEndPoint} Interop.Winsock.WSAConnect returns errorCode:{errorCode}");
                }
                catch (ObjectDisposedException) { }
            }
#endif

            // Throw an appropriate SocketException if the native call fails.
            if (errorCode != SocketError.Success)
            {
                // Update the internal state of this socket according to the error before throwing.
                SocketException socketException = SocketExceptionFactory.CreateSocketException((int)errorCode, endPointSnapshot);
                UpdateStatusAfterSocketError(socketException);
                if (NetEventSource.IsEnabled) NetEventSource.Error(this, socketException);
                throw socketException;
            }

            if (_rightEndPoint == null)
            {
                // Save a copy of the EndPoint so we can use it for Create().
                _rightEndPoint = endPointSnapshot;
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"connection to:{endPointSnapshot}");

            // Update state and performance counters.
            SetToConnected();
            if (NetEventSource.IsEnabled)
            {
                NetEventSource.Connected(this, LocalEndPoint, RemoteEndPoint);
                NetEventSource.Exit(this);
            }
        }

        // Implements ConnectEx - this provides completion port IO and support for disconnect and reconnects.
        // Since this is private, the unsafe mode is specified with a flag instead of an overload.
        private IAsyncResult BeginConnectEx(EndPoint remoteEP, bool flowContext, AsyncCallback callback, object state)
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);

            EndPoint endPointSnapshot = remoteEP;
            Internals.SocketAddress socketAddress = SnapshotAndSerialize(ref endPointSnapshot);

            // The socket must be bound first.
            // The calling method--BeginConnect--will ensure that this method is only
            // called if _rightEndPoint is not null, of that the endpoint is an IPEndPoint.
            if (_rightEndPoint == null)
            {
                if (endPointSnapshot.AddressFamily == AddressFamily.InterNetwork)
                {
                    InternalBind(new IPEndPoint(IPAddress.Any, 0));
                }
                else if (endPointSnapshot.AddressFamily != AddressFamily.Unix)
                {
                    InternalBind(new IPEndPoint(IPAddress.IPv6Any, 0));
                }
            }

            // Allocate the async result and the event we'll pass to the thread pool.
            ConnectOverlappedAsyncResult asyncResult = new ConnectOverlappedAsyncResult(this, endPointSnapshot, state, callback);

            // If context flowing is enabled, set it up here.  No need to lock since the context isn't used until the callback.
            if (flowContext)
            {
                asyncResult.StartPostingAsyncOp(false);
            }

            EndPoint oldEndPoint = _rightEndPoint;
            if (_rightEndPoint == null)
            {
                _rightEndPoint = endPointSnapshot;
            }

            SocketError errorCode;
            try
            {
                errorCode = SocketPal.ConnectAsync(this, _handle, socketAddress.Buffer, socketAddress.Size, asyncResult);
            }
            catch
            {
                // _rightEndPoint will always equal oldEndPoint.
                _rightEndPoint = oldEndPoint;
                throw;
            }

            if (NetEventSource.IsEnabled) NetEventSource.Info(this, $"Interop.Winsock.connect returns:{errorCode}");

            if (errorCode == SocketError.Success)
            {
                // Synchronous success. Indicate that we're connected.
                SetToConnected();
            }

            if (!CheckErrorAndUpdateStatus(errorCode))
            {
                // Update the internal state of this socket according to the error before throwing.
                _rightEndPoint = oldEndPoint;

                throw new SocketException((int)errorCode);
            }

            // We didn't throw, so indicate that we're returning this result to the user.  This may call the callback.
            // This is a nop if the context isn't being flowed.
            asyncResult.FinishPostingAsyncOp(ref Caches.ConnectClosureCache);

            if (NetEventSource.IsEnabled)
            {
                NetEventSource.Info(this, $"{endPointSnapshot} returning AsyncResult:{asyncResult}");
                NetEventSource.Exit(this, asyncResult);
            }
            return asyncResult;
        }

        private static void DnsCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            bool invokeCallback = false;

            MultipleAddressConnectAsyncResult context = (MultipleAddressConnectAsyncResult)result.AsyncState;
            try
            {
                invokeCallback = DoDnsCallback(result, context);
            }
            catch (Exception exception)
            {
                context.InvokeCallback(exception);
            }

            // Invoke the callback outside of the try block so we don't catch user exceptions.
            if (invokeCallback)
            {
                context.InvokeCallback();
            }
        }

        private static bool DoDnsCallback(IAsyncResult result, MultipleAddressConnectAsyncResult context)
        {
            IPAddress[] addresses = Dns.EndGetHostAddresses(result);
            context._addresses = addresses;
            return DoMultipleAddressConnectCallback(PostOneBeginConnect(context), context);
        }

        private sealed class ConnectAsyncResult : ContextAwareResult
        {
            private EndPoint _endPoint;

            internal ConnectAsyncResult(object myObject, EndPoint endPoint, object myState, AsyncCallback myCallBack) :
                base(myObject, myState, myCallBack)
            {
                _endPoint = endPoint;
            }

            internal override EndPoint RemoteEndPoint
            {
                get { return _endPoint; }
            }
        }

        private sealed class MultipleAddressConnectAsyncResult : ContextAwareResult
        {
            internal MultipleAddressConnectAsyncResult(IPAddress[] addresses, int port, Socket socket, object myState, AsyncCallback myCallBack) :
                base(socket, myState, myCallBack)
            {
                _addresses = addresses;
                _port = port;
                _socket = socket;
            }

            internal Socket _socket;   // Keep this member just to avoid all the casting.
            internal IPAddress[] _addresses;
            internal int _index;
            internal int _port;
            internal Exception _lastException;

            internal override EndPoint RemoteEndPoint
            {
                get
                {
                    if (_addresses != null && _index > 0 && _index < _addresses.Length)
                    {
                        return new IPEndPoint(_addresses[_index], _port);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private static AsyncCallback s_multipleAddressConnectCallback;
        private static AsyncCallback CachedMultipleAddressConnectCallback
        {
            get
            {
                if (s_multipleAddressConnectCallback == null)
                {
                    s_multipleAddressConnectCallback = new AsyncCallback(MultipleAddressConnectCallback);
                }
                return s_multipleAddressConnectCallback;
            }
        }

        private static object PostOneBeginConnect(MultipleAddressConnectAsyncResult context)
        {
            IPAddress currentAddressSnapshot = context._addresses[context._index];

            context._socket.ReplaceHandleIfNecessaryAfterFailedConnect();

            if (!context._socket.CanTryAddressFamily(currentAddressSnapshot.AddressFamily))
            {
                return context._lastException != null ? context._lastException : new ArgumentException(SR.net_invalidAddressList, nameof(context));
            }

            try
            {
                EndPoint endPoint = new IPEndPoint(currentAddressSnapshot, context._port);

                context._socket.SnapshotAndSerialize(ref endPoint);

                IAsyncResult connectResult = context._socket.UnsafeBeginConnect(endPoint, CachedMultipleAddressConnectCallback, context);
                if (connectResult.CompletedSynchronously)
                {
                    return connectResult;
                }
            }
            catch (Exception exception) when (!(exception is OutOfMemoryException))
            {
                return exception;
            }

            return null;
        }

        private static void MultipleAddressConnectCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            bool invokeCallback = false;

            MultipleAddressConnectAsyncResult context = (MultipleAddressConnectAsyncResult)result.AsyncState;
            try
            {
                invokeCallback = DoMultipleAddressConnectCallback(result, context);
            }
            catch (Exception exception)
            {
                context.InvokeCallback(exception);
            }

            // Invoke the callback outside of the try block so we don't catch user Exceptions.
            if (invokeCallback)
            {
                context.InvokeCallback();
            }
        }

        // This is like a regular async callback worker, except the result can be an exception.  This is a useful pattern when
        // processing should continue whether or not an async step failed.
        private static bool DoMultipleAddressConnectCallback(object result, MultipleAddressConnectAsyncResult context)
        {
            while (result != null)
            {
                Exception ex = result as Exception;
                if (ex == null)
                {
                    try
                    {
                        context._socket.EndConnect((IAsyncResult)result);
                    }
                    catch (Exception exception)
                    {
                        ex = exception;
                    }
                }

                if (ex == null)
                {
                    // Don't invoke the callback from here, because we're probably inside
                    // a catch-all block that would eat exceptions from the callback.
                    // Instead tell our caller to invoke the callback outside of its catchall.
                    return true;
                }
                else
                {
                    if (++context._index >= context._addresses.Length)
                    {
                        ExceptionDispatchInfo.Throw(ex);
                    }

                    context._lastException = ex;
                    result = PostOneBeginConnect(context);
                }
            }

            // Don't invoke the callback at all, because we've posted another async connection attempt.
            return false;
        }

        // CreateAcceptSocket - pulls unmanaged results and assembles them into a new Socket object.
        internal Socket CreateAcceptSocket(SafeCloseSocket fd, EndPoint remoteEP)
        {
            // Internal state of the socket is inherited from listener.
            Debug.Assert(fd != null && !fd.IsInvalid);
            Socket socket = new Socket(fd);
            return UpdateAcceptSocket(socket, remoteEP);
        }

        internal Socket UpdateAcceptSocket(Socket socket, EndPoint remoteEP)
        {
            // Internal state of the socket is inherited from listener.
            socket._addressFamily = _addressFamily;
            socket._socketType = _socketType;
            socket._protocolType = _protocolType;
            socket._rightEndPoint = _rightEndPoint;
            socket._remoteEndPoint = remoteEP;

            // The socket is connected.
            socket.SetToConnected();

            // if the socket is returned by an End(), the socket might have
            // inherited the WSAEventSelect() call from the accepting socket.
            // we need to cancel this otherwise the socket will be in non-blocking
            // mode and we cannot force blocking mode using the ioctlsocket() in
            // Socket.set_Blocking(), since it fails returning 10022 as documented in MSDN.
            // (note that the m_AsyncEvent event will not be created in this case.

            socket._willBlock = _willBlock;

            // We need to make sure the Socket is in the right blocking state
            // even if we don't have to call UnsetAsyncEventSelect
            socket.InternalSetBlocking(_willBlock);

            return socket;
        }

        internal void SetToConnected()
        {
            if (_isConnected)
            {
                // Socket was already connected.
                return;
            }

            // Update the status: this socket was indeed connected at
            // some point in time update the perf counter as well.
            _isConnected = true;
            _isDisconnected = false;
            if (NetEventSource.IsEnabled) NetEventSource.Info(this, "now connected");
        }

        internal void SetToDisconnected()
        {
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);

            if (!_isConnected)
            {
                // Socket was already disconnected.
                return;
            }

            // Update the status: this socket was indeed disconnected at
            // some point in time, clear any async select bits.
            _isConnected = false;
            _isDisconnected = true;

            if (!CleanedUp)
            {
                if (NetEventSource.IsEnabled) NetEventSource.Info(this, "!CleanedUp");
            }
        }

        private void UpdateStatusAfterSocketErrorAndThrowException(SocketError error, [CallerMemberName] string callerName = null)
        {
            // Update the internal state of this socket according to the error before throwing.
            var socketException = new SocketException((int)error);
            UpdateStatusAfterSocketError(socketException);
            if (NetEventSource.IsEnabled) NetEventSource.Error(this, socketException, memberName: callerName);
            throw socketException;
        }

        // UpdateStatusAfterSocketError(socketException) - updates the status of a connected socket
        // on which a failure occurred. it'll go to winsock and check if the connection
        // is still open and if it needs to update our internal state.
        internal void UpdateStatusAfterSocketError(SocketException socketException)
        {
            UpdateStatusAfterSocketError(socketException.SocketErrorCode);
        }

        internal void UpdateStatusAfterSocketError(SocketError errorCode)
        {
            // If we already know the socket is disconnected
            // we don't need to do anything else.
            if (NetEventSource.IsEnabled) NetEventSource.Enter(this);
            if (NetEventSource.IsEnabled) NetEventSource.Error(this, $"errorCode:{errorCode}");

            if (_isConnected && (_handle.IsInvalid || (errorCode != SocketError.WouldBlock &&
                    errorCode != SocketError.IOPending && errorCode != SocketError.NoBufferSpaceAvailable &&
                    errorCode != SocketError.TimedOut)))
            {
                // The socket is no longer a valid socket.
                if (NetEventSource.IsEnabled) NetEventSource.Info(this, "Invalidating socket.");
                SetToDisconnected();
            }
        }

        private bool CheckErrorAndUpdateStatus(SocketError errorCode)
        {
            if (errorCode == SocketError.Success || errorCode == SocketError.IOPending)
            {
                return true;
            }

            UpdateStatusAfterSocketError(errorCode);
            return false;
        }

        // ValidateBlockingMode - called before synchronous calls to validate
        // the fact that we are in blocking mode (not in non-blocking mode) so the
        // call will actually be synchronous.
        private void ValidateBlockingMode()
        {
            if (_willBlock && !_willBlockInternal)
            {
                throw new InvalidOperationException(SR.net_invasync);
            }
        }

        // Validates that the Socket can be used to try another Connect call, in case
        // a previous call failed and the platform does not support that.  In some cases,
        // the call may also be able to "fix" the Socket to continue working, even if the
        // platform wouldn't otherwise support it.  Windows always supports this.
        partial void ValidateForMultiConnect(bool isMultiEndpoint);

#endregion

    }
}