using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Net.Sockets
{
#if MONO
    using Internals = System.Net;
#endif

    static class SocketPal
    {
#region CoreFX Code

        public const bool SupportsMultipleConnectAttempts = false;
        private static readonly bool SupportsDualModeIPv4PacketInfo = GetPlatformSupportsDualModeIPv4PacketInfo();

        private static bool GetPlatformSupportsDualModeIPv4PacketInfo()
        {
            return Interop.Sys.PlatformSupportsDualModeIPv4PacketInfo();
        }

        public static void Initialize()
        {
            // nop.  No initialization required.
        }

        public static SocketError GetSocketErrorForErrorCode(Interop.Error errorCode)
        {
            return SocketErrorPal.GetSocketErrorForNativeError(errorCode);
        }

        public static void CheckDualModeReceiveSupport(Socket socket)
        {
            if (!SupportsDualModeIPv4PacketInfo && socket.AddressFamily == AddressFamily.InterNetworkV6 && socket.DualMode)
            {
                throw new PlatformNotSupportedException(SR.net_sockets_dualmode_receivefrom_notsupported);
            }
        }

        public static SocketError SetBlocking(SafeCloseSocket handle, bool shouldBlock, out bool willBlock)
        {
            handle.IsNonBlocking = !shouldBlock;
            willBlock = shouldBlock;
            return SocketError.Success;
        }

        public static SocketError Accept(SafeCloseSocket handle, byte[] buffer, ref int nameLen, out SafeCloseSocket socket)
        {
            return SafeCloseSocket.Accept(handle, buffer, ref nameLen, out socket);
        }

        public static SocketError Connect(SafeCloseSocket handle, byte[] socketAddress, int socketAddressLen)
        {
            if (!handle.IsNonBlocking)
            {
                return handle.AsyncContext.Connect(socketAddress, socketAddressLen);
            }

            SocketError errorCode;
            bool completed = TryStartConnect(handle, socketAddress, socketAddressLen, out errorCode);
            if (completed)
            {
                handle.RegisterConnectResult(errorCode);
                return errorCode;
            }
            else
            {
                return SocketError.WouldBlock;
            }
        }

        public static unsafe bool TryCompleteAccept(SafeCloseSocket socket, byte[] socketAddress, ref int socketAddressLen, out IntPtr acceptedFd, out SocketError errorCode)
        {
            IntPtr fd = IntPtr.Zero;
            Interop.Error errno;
            int sockAddrLen = socketAddressLen;
            fixed (byte* rawSocketAddress = socketAddress)
            {
                try
                {
                    errno = Interop.Sys.Accept(socket, rawSocketAddress, &sockAddrLen, &fd);
                }
                catch (ObjectDisposedException)
                {
                    // The socket was closed, or is closing.
                    errorCode = SocketError.OperationAborted;
                    acceptedFd = (IntPtr)(-1);
                    return true;
                }
            }

            if (errno == Interop.Error.SUCCESS)
            {
                Debug.Assert(fd != (IntPtr)(-1), "Expected fd != -1");

                socketAddressLen = sockAddrLen;
                errorCode = SocketError.Success;
                acceptedFd = fd;

                return true;
            }

            acceptedFd = (IntPtr)(-1);
            if (errno != Interop.Error.EAGAIN && errno != Interop.Error.EWOULDBLOCK)
            {
                errorCode = GetSocketErrorForErrorCode(errno);
                return true;
            }

            errorCode = SocketError.Success;
            return false;
        }

        public static SocketError Receive(SafeCloseSocket handle, IList<ArraySegment<byte>> buffers, ref SocketFlags socketFlags, out int bytesTransferred)
        {
            SocketError errorCode;
            if (!handle.IsNonBlocking)
            {
                errorCode = handle.AsyncContext.Receive(buffers, ref socketFlags, handle.ReceiveTimeout, out bytesTransferred);
            }
            else
            {
                int socketAddressLen = 0;
                if (!TryCompleteReceiveFrom(handle, buffers, socketFlags, null, ref socketAddressLen, out bytesTransferred, out socketFlags, out errorCode))
                {
                    errorCode = SocketError.WouldBlock;
                }
            }

            return errorCode;
        }

        public static SocketError Receive(SafeCloseSocket handle, byte[] buffer, int offset, int count, SocketFlags socketFlags, out int bytesTransferred)
        {
            if (!handle.IsNonBlocking)
            {
                return handle.AsyncContext.Receive(new Memory<byte>(buffer, offset, count), ref socketFlags, handle.ReceiveTimeout, out bytesTransferred);
            }

            int socketAddressLen = 0;
            SocketError errorCode;
            bool completed = TryCompleteReceiveFrom(handle, new Span<byte>(buffer, offset, count), socketFlags, null, ref socketAddressLen, out bytesTransferred, out socketFlags, out errorCode);
            return completed ? errorCode : SocketError.WouldBlock;
        }

        public static SocketError Receive(SafeCloseSocket handle, Span<byte> buffer, SocketFlags socketFlags, out int bytesTransferred)
        {
            if (!handle.IsNonBlocking)
            {
                return handle.AsyncContext.Receive(buffer, ref socketFlags, handle.ReceiveTimeout, out bytesTransferred);
            }

            int socketAddressLen = 0;
            SocketError errorCode;
            bool completed = TryCompleteReceiveFrom(handle, buffer, socketFlags, null, ref socketAddressLen, out bytesTransferred, out socketFlags, out errorCode);
            return completed ? errorCode : SocketError.WouldBlock;
        }

        private static unsafe int Receive(SafeCloseSocket socket, SocketFlags flags, Span<byte> buffer, byte[] socketAddress, ref int socketAddressLen, out SocketFlags receivedFlags, out Interop.Error errno)
        {
            Debug.Assert(socketAddress != null || socketAddressLen == 0, $"Unexpected values: socketAddress={socketAddress}, socketAddressLen={socketAddressLen}");

            long received = 0;
            int sockAddrLen = socketAddress != null ? socketAddressLen : 0;

            fixed (byte* sockAddr = socketAddress)
            fixed (byte* b = &MemoryMarshal.GetReference(buffer))
            {
                var iov = new Interop.Sys.IOVector {
                    Base = b,
                    Count = (UIntPtr)buffer.Length
                };

                var messageHeader = new Interop.Sys.MessageHeader {
                    SocketAddress = sockAddr,
                    SocketAddressLen = sockAddrLen,
                    IOVectors = &iov,
                    IOVectorCount = 1
                };

                errno = Interop.Sys.ReceiveMessage(
#if MONO
                    socket,
#else
                    socket.DangerousGetHandle(), // to minimize chances of handle recycling from misuse, this should use DangerousAddRef/Release, but it adds too much overhead
#endif
                    &messageHeader,
                    flags,
                    &received);
                GC.KeepAlive(socket); // small extra safe guard against handle getting collected/finalized while P/Invoke in progress

                receivedFlags = messageHeader.Flags;
                sockAddrLen = messageHeader.SocketAddressLen;
            }

            if (errno != Interop.Error.SUCCESS)
            {
                return -1;
            }

            socketAddressLen = sockAddrLen;
            return checked((int)received);
        }

        private static unsafe int Receive(SafeCloseSocket socket, SocketFlags flags, IList<ArraySegment<byte>> buffers, byte[] socketAddress, ref int socketAddressLen, out SocketFlags receivedFlags, out Interop.Error errno)
        {
            int available = 0;
            errno = Interop.Sys.GetBytesAvailable(socket, &available);
            if (errno != Interop.Error.SUCCESS)
            {
                receivedFlags = 0;
                return -1;
            }
            if (available == 0)
            {
                // Don't truncate iovecs.
                available = int.MaxValue;
            }

            // Pin buffers and set up iovecs.
            int maxBuffers = buffers.Count;
            var handles = new GCHandle[maxBuffers];
            var iovecs = new Interop.Sys.IOVector[maxBuffers];

            int sockAddrLen = 0;
            if (socketAddress != null)
            {
                sockAddrLen = socketAddressLen;
            }

            long received = 0;
            int toReceive = 0, iovCount = maxBuffers;
            try
            {
                for (int i = 0; i < maxBuffers; i++)
                {
                    ArraySegment<byte> buffer = buffers[i];
                    RangeValidationHelpers.ValidateSegment(buffer);

                    handles[i] = GCHandle.Alloc(buffer.Array, GCHandleType.Pinned);
                    iovecs[i].Base = &((byte*)handles[i].AddrOfPinnedObject())[buffer.Offset];

                    int space = buffer.Count;
                    toReceive += space;
                    if (toReceive >= available)
                    {
                        iovecs[i].Count = (UIntPtr)(space - (toReceive - available));
                        toReceive = available;
                        iovCount = i + 1;
                        for (int j = i + 1; j < maxBuffers; j++)
                        {
                            // We're not going to use these extra buffers, but validate their args
                            // to alert the dev to a mistake and to be consistent with Windows.
                            RangeValidationHelpers.ValidateSegment(buffers[j]);
                        }
                        break;
                    }

                    iovecs[i].Count = (UIntPtr)space;
                }

                // Make the call.
                fixed (byte* sockAddr = socketAddress)
                fixed (Interop.Sys.IOVector* iov = iovecs)
                {
                    var messageHeader = new Interop.Sys.MessageHeader {
                        SocketAddress = sockAddr,
                        SocketAddressLen = sockAddrLen,
                        IOVectors = iov,
                        IOVectorCount = iovCount
                    };

                    errno = Interop.Sys.ReceiveMessage(
#if MONO
                        socket,
#else
                        socket.DangerousGetHandle(), // to minimize chances of handle recycling from misuse, this should use DangerousAddRef/Release, but it adds too much overhead
#endif
                        &messageHeader,
                        flags,
                        &received);
                    GC.KeepAlive(socket); // small extra safe guard against handle getting collected/finalized while P/Invoke in progress

                    receivedFlags = messageHeader.Flags;
                    sockAddrLen = messageHeader.SocketAddressLen;
                }
            }
            finally
            {
                // Free GC handles.
                for (int i = 0; i < iovCount; i++)
                {
                    if (handles[i].IsAllocated)
                    {
                        handles[i].Free();
                    }
                }
            }

            if (errno != Interop.Error.SUCCESS)
            {
                return -1;
            }

            socketAddressLen = sockAddrLen;
            return checked((int)received);
        }

        public static bool TryCompleteReceiveFrom(SafeCloseSocket socket, Span<byte> buffer, SocketFlags flags, byte[] socketAddress, ref int socketAddressLen, out int bytesReceived, out SocketFlags receivedFlags, out SocketError errorCode) =>
            TryCompleteReceiveFrom(socket, buffer, null, flags, socketAddress, ref socketAddressLen, out bytesReceived, out receivedFlags, out errorCode);

        public static bool TryCompleteReceiveFrom(SafeCloseSocket socket, IList<ArraySegment<byte>> buffers, SocketFlags flags, byte[] socketAddress, ref int socketAddressLen, out int bytesReceived, out SocketFlags receivedFlags, out SocketError errorCode) =>
            TryCompleteReceiveFrom(socket, default(Span<byte>), buffers, flags, socketAddress, ref socketAddressLen, out bytesReceived, out receivedFlags, out errorCode);

        public static unsafe bool TryCompleteReceiveFrom(SafeCloseSocket socket, Span<byte> buffer, IList<ArraySegment<byte>> buffers, SocketFlags flags, byte[] socketAddress, ref int socketAddressLen, out int bytesReceived, out SocketFlags receivedFlags, out SocketError errorCode)
        {
            try
            {
                Interop.Error errno;
                int received;

                if (buffers != null)
                {
                    // Receive into a set of buffers
                    received = Receive(socket, flags, buffers, socketAddress, ref socketAddressLen, out receivedFlags, out errno);
                }
                else if (buffer.Length == 0)
                {
                    // Special case a receive of 0 bytes into a single buffer.  A common pattern is to ReceiveAsync 0 bytes in order
                    // to be asynchronously notified when data is available, without needing to dedicate a buffer.  Some platforms (e.g. macOS),
                    // however complete a 0-byte read successfully when data isn't available, as the request can logically be satisfied
                    // synchronously. As such, we treat 0 specially, and perform a 1-byte peek.
                    byte oneBytePeekBuffer;
                    received = Receive(socket, flags | SocketFlags.Peek, new Span<byte>(&oneBytePeekBuffer, 1), socketAddress, ref socketAddressLen, out receivedFlags, out errno);
                    if (received > 0)
                    {
                        // Peeked for 1-byte, but the actual request was for 0.
                        received = 0;
                    }
                }
                else
                {
                    // Receive > 0 bytes into a single buffer
                    received = Receive(socket, flags, buffer, socketAddress, ref socketAddressLen, out receivedFlags, out errno);
                }

                if (received != -1)
                {
                    bytesReceived = received;
                    errorCode = SocketError.Success;
                    return true;
                }

                bytesReceived = 0;

                if (errno != Interop.Error.EAGAIN && errno != Interop.Error.EWOULDBLOCK)
                {
                    errorCode = GetSocketErrorForErrorCode(errno);
                    return true;
                }

                errorCode = SocketError.Success;
                return false;
            }
            catch (ObjectDisposedException)
            {
                // The socket was closed, or is closing.
                bytesReceived = 0;
                receivedFlags = 0;
                errorCode = SocketError.OperationAborted;
                return true;
            }
        }

        public static unsafe bool TryCompleteReceiveMessageFrom(SafeCloseSocket socket, Span<byte> buffer, IList<ArraySegment<byte>> buffers, SocketFlags flags, byte[] socketAddress, ref int socketAddressLen, bool isIPv4, bool isIPv6, out int bytesReceived, out SocketFlags receivedFlags, out IPPacketInformation ipPacketInformation, out SocketError errorCode)
        {
            try
            {
                Interop.Error errno;

                int received = buffers == null ?
                    ReceiveMessageFrom(socket, flags, buffer, socketAddress, ref socketAddressLen, isIPv4, isIPv6, out receivedFlags, out ipPacketInformation, out errno) :
                    ReceiveMessageFrom(socket, flags, buffers, socketAddress, ref socketAddressLen, isIPv4, isIPv6, out receivedFlags, out ipPacketInformation, out errno);

                if (received != -1)
                {
                    bytesReceived = received;
                    errorCode = SocketError.Success;
                    return true;
                }

                bytesReceived = 0;

                if (errno != Interop.Error.EAGAIN && errno != Interop.Error.EWOULDBLOCK)
                {
                    errorCode = GetSocketErrorForErrorCode(errno);
                    return true;
                }

                errorCode = SocketError.Success;
                return false;
            }
            catch (ObjectDisposedException)
            {
                // The socket was closed, or is closing.
                bytesReceived = 0;
                receivedFlags = 0;
                ipPacketInformation = default(IPPacketInformation);
                errorCode = SocketError.OperationAborted;
                return true;
            }
        }

        private static unsafe int ReceiveMessageFrom(SafeCloseSocket socket, SocketFlags flags, Span<byte> buffer, byte[] socketAddress, ref int socketAddressLen, bool isIPv4, bool isIPv6, out SocketFlags receivedFlags, out IPPacketInformation ipPacketInformation, out Interop.Error errno)
        {
            Debug.Assert(socketAddress != null, "Expected non-null socketAddress");

            int cmsgBufferLen = Interop.Sys.GetControlMessageBufferSize(isIPv4, isIPv6);
            var cmsgBuffer = stackalloc byte[cmsgBufferLen];

            int sockAddrLen = socketAddressLen;

            Interop.Sys.MessageHeader messageHeader;

            long received = 0;
            fixed (byte* rawSocketAddress = socketAddress)
            fixed (byte* b = &MemoryMarshal.GetReference(buffer))
            {
                var iov = new Interop.Sys.IOVector {
                    Base = b,
                    Count = (UIntPtr)buffer.Length
                };

                messageHeader = new Interop.Sys.MessageHeader {
                    SocketAddress = rawSocketAddress,
                    SocketAddressLen = sockAddrLen,
                    IOVectors = &iov,
                    IOVectorCount = 1,
                    ControlBuffer = cmsgBuffer,
                    ControlBufferLen = cmsgBufferLen
                };

                errno = Interop.Sys.ReceiveMessage(
#if MONO
                    socket,
#else
                    socket.DangerousGetHandle(), // to minimize chances of handle recycling from misuse, this should use DangerousAddRef/Release, but it adds too much overhead
#endif
                    &messageHeader,
                    flags,
                    &received);
                GC.KeepAlive(socket); // small extra safe guard against handle getting collected/finalized while P/Invoke in progress

                receivedFlags = messageHeader.Flags;
                sockAddrLen = messageHeader.SocketAddressLen;
            }

            if (errno != Interop.Error.SUCCESS)
            {
                ipPacketInformation = default(IPPacketInformation);
                return -1;
            }

            ipPacketInformation = GetIPPacketInformation(&messageHeader, isIPv4, isIPv6);
            socketAddressLen = sockAddrLen;
            return checked((int)received);
        }

        private static unsafe int ReceiveMessageFrom(
            SafeCloseSocket socket, SocketFlags flags, IList<ArraySegment<byte>> buffers,
            byte[] socketAddress, ref int socketAddressLen, bool isIPv4, bool isIPv6,
            out SocketFlags receivedFlags, out IPPacketInformation ipPacketInformation, out Interop.Error errno)
        {
            Debug.Assert(socketAddress != null, "Expected non-null socketAddress");

            int buffersCount = buffers.Count;
            var handles = new GCHandle[buffersCount];
            var iovecs = new Interop.Sys.IOVector[buffersCount];
            try
            {
                // Pin buffers and set up iovecs.
                for (int i = 0; i < buffersCount; i++)
                {
                    ArraySegment<byte> buffer = buffers[i];
                    RangeValidationHelpers.ValidateSegment(buffer);

                    handles[i] = GCHandle.Alloc(buffer.Array, GCHandleType.Pinned);
                    iovecs[i].Base = &((byte*)handles[i].AddrOfPinnedObject())[buffer.Offset];
                    iovecs[i].Count = (UIntPtr)buffer.Count;
                }

                // Make the call.
                fixed (byte* sockAddr = socketAddress)
                fixed (Interop.Sys.IOVector* iov = iovecs)
                {
                    int cmsgBufferLen = Interop.Sys.GetControlMessageBufferSize(isIPv4, isIPv6);
                    var cmsgBuffer = stackalloc byte[cmsgBufferLen];

                    var messageHeader = new Interop.Sys.MessageHeader
                    {
                        SocketAddress = sockAddr,
                        SocketAddressLen = socketAddressLen,
                        IOVectors = iov,
                        IOVectorCount = buffersCount,
                        ControlBuffer = cmsgBuffer,
                        ControlBufferLen = cmsgBufferLen
                    };

                    long received = 0;
                    errno = Interop.Sys.ReceiveMessage(
#if MONO
                        socket,
#else
                        socket.DangerousGetHandle(), // to minimize chances of handle recycling from misuse, this should use DangerousAddRef/Release, but it adds too much overhead
#endif
                        &messageHeader,
                        flags,
                        &received);
                    GC.KeepAlive(socket); // small extra safe guard against handle getting collected/finalized while P/Invoke in progress

                    receivedFlags = messageHeader.Flags;
                    int sockAddrLen = messageHeader.SocketAddressLen;

                    if (errno == Interop.Error.SUCCESS)
                    {
                        ipPacketInformation = GetIPPacketInformation(&messageHeader, isIPv4, isIPv6);
                        socketAddressLen = sockAddrLen;
                        return checked((int)received);
                    }
                    else
                    {
                        ipPacketInformation = default(IPPacketInformation);
                        return -1;
                    }
                }
            }
            finally
            {
                // Free GC handles.
                for (int i = 0; i < buffersCount; i++)
                {
                    if (handles[i].IsAllocated)
                    {
                        handles[i].Free();
                    }
                }
            }
        }

        public static SocketError ReceiveMessageFrom(Socket socket, SafeCloseSocket handle, byte[] buffer, int offset, int count, ref SocketFlags socketFlags, Internals.SocketAddress socketAddress, out Internals.SocketAddress receiveAddress, out IPPacketInformation ipPacketInformation, out int bytesTransferred)
        {
            byte[] socketAddressBuffer = socketAddress.Buffer;
            int socketAddressLen = socketAddress.Size;

            bool isIPv4, isIPv6;
            Socket.GetIPProtocolInformation(socket.AddressFamily, socketAddress, out isIPv4, out isIPv6);

            SocketError errorCode;
            if (!handle.IsNonBlocking)
            {
                errorCode = handle.AsyncContext.ReceiveMessageFrom(new Memory<byte>(buffer, offset, count), null, ref socketFlags, socketAddressBuffer, ref socketAddressLen, isIPv4, isIPv6, handle.ReceiveTimeout, out ipPacketInformation, out bytesTransferred);
            }
            else
            {
                if (!TryCompleteReceiveMessageFrom(handle, new Span<byte>(buffer, offset, count), null, socketFlags, socketAddressBuffer, ref socketAddressLen, isIPv4, isIPv6, out bytesTransferred, out socketFlags, out ipPacketInformation, out errorCode))
                {
                    errorCode = SocketError.WouldBlock;
                }
            }

            socketAddress.InternalSize = socketAddressLen;
            receiveAddress = socketAddress;
            return errorCode;
        }

        public static SocketError ReceiveFrom(SafeCloseSocket handle, byte[] buffer, int offset, int count, SocketFlags socketFlags, byte[] socketAddress, ref int socketAddressLen, out int bytesTransferred)
        {
            if (!handle.IsNonBlocking)
            {
                return handle.AsyncContext.ReceiveFrom(new Memory<byte>(buffer, offset, count), ref socketFlags, socketAddress, ref socketAddressLen, handle.ReceiveTimeout, out bytesTransferred);
            }

            SocketError errorCode;
            bool completed = TryCompleteReceiveFrom(handle, new Span<byte>(buffer, offset, count), socketFlags, socketAddress, ref socketAddressLen, out bytesTransferred, out socketFlags, out errorCode);
            return completed ? errorCode : SocketError.WouldBlock;
        }

        public static void SetReceivingDualModeIPv4PacketInformation(Socket socket)
        {
            // NOTE: some platforms (e.g. OS X) do not support receiving IPv4 packet information for packets received
            //       on dual-mode sockets. On these platforms, this call is a no-op.
            if (SupportsDualModeIPv4PacketInfo)
            {
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            }
        }

#endregion

        public static SocketError CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, out SafeSocketHandle socket)
        {
            return SafeCloseSocket.CreateSocket(addressFamily, socketType, protocolType, out socket);
        }

        public static unsafe SocketError GetSockName(SafeSocketHandle handle, byte[] buffer, ref int nameLen)
        {
            fixed (byte* rawBuffer = buffer)
            {
                return Socket.GetSockName_internal(handle, rawBuffer, ref nameLen);
            }
        }

        public static unsafe SocketError GetPeerName(SafeSocketHandle handle, byte[] buffer, ref int nameLen)
        {
            fixed (byte* rawBuffer = buffer)
            {
                return Socket.GetPeerName_internal(handle, rawBuffer, ref nameLen);
            }
        }

        public static unsafe SocketError Bind(SafeCloseSocket handle, ProtocolType socketProtocolType, byte[] buffer, int nameLen)
        {
            var sa = new SocketAddress(buffer, nameLen);

            Socket.Bind_internal((SafeSocketHandle)handle, sa, out var error);
            return (SocketError)error;
        }

        public static SocketError Listen(SafeCloseSocket handle, int backlog)
        {
            Socket.Listen_internal((SafeSocketHandle)handle, backlog, out var error);
            return (SocketError)error;
        }

        public static SocketError ConnectAsync(Socket socket, SafeCloseSocket handle, byte[] socketAddress, int socketAddressLen, ConnectOverlappedAsyncResult asyncResult)
        {
            SocketError socketError = handle.AsyncContext.ConnectAsync(socketAddress, socketAddressLen, asyncResult.CompletionCallback);
            if (socketError == SocketError.Success)
            {
                asyncResult.CompletionCallback(SocketError.Success);
            }
            return socketError;
        }

        public static bool TryStartConnect(SafeCloseSocket socket, byte[] socketAddress, int socketAddressLen, out SocketError errorCode)
        {
            return TryStartConnect((SafeSocketHandle)socket, socketAddress, socketAddressLen, false, out errorCode);
        }

        private static bool TryStartConnect(SafeSocketHandle socket, byte[] socketAddress, int socketAddressLen, bool block, out SocketError errorCode)
        {
            Debug.Assert(socketAddress != null, "Expected non-null socketAddress");
            Debug.Assert(socketAddressLen > 0, $"Unexpected socketAddressLen: {socketAddressLen}");

            if (socket.IsDisconnected)
            {
                errorCode = SocketError.IsConnected;
                return true;
            }

            var sa = new SocketAddress(socketAddress, socketAddressLen);

            Socket.Connect_internal(socket, sa, out var error, block);

            if (error == 0)
            {
                errorCode = SocketError.Success;
                return true;
            }

            if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock)
            {
                errorCode = (SocketError)error;
                return true;
            }

            errorCode = SocketError.Success;
            return false;
        }

        public static bool TryCompleteConnect(SafeCloseSocket socket, int socketAddressLen, out SocketError errorCode)
        {
            try
            {
                errorCode = (SocketError)Socket.GetSocketOption((SafeSocketHandle)socket, SocketOptionLevel.Socket, SocketOptionName.Error);
            }
            catch (ObjectDisposedException)
            {
                // The socket was closed, or is closing.
                errorCode = SocketError.OperationAborted;
                return true;
            }

            if (errorCode == SocketError.Success)
            {
                return true;
            }

            if (errorCode == SocketError.InProgress)
            {
                errorCode = SocketError.Success;
                return false;
            }

            return true;
        }

        private static unsafe IPPacketInformation GetIPPacketInformation(Interop.Sys.MessageHeader* messageHeader, bool isIPv4, bool isIPv6)
        {
            return default(IPPacketInformation);
        }

    }
}
