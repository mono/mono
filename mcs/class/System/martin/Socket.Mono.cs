using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Net.Sockets
{
    partial class Socket
    {
#region CoreFX Code

        partial void ValidateForMultiConnect(bool isMultiEndpoint)
        {
            // ValidateForMultiConnect is called before any {Begin}Connect{Async} call,
            // regardless of whether it's targeting an endpoint with multiple addresses.
            // If it is targeting such an endpoint, then any exposure of the socket's handle
            // or configuration of the socket we haven't tracked would prevent us from
            // replicating the socket's file descriptor appropriately.  Similarly, if it's
            // only targeting a single address, but it already experienced a failure in a
            // previous connect call, then this is logically part of a multi endpoint connect,
            // and the same logic applies.  Either way, in such a situation we throw.
            if (_handle.ExposedHandleOrUntrackedConfiguration && (isMultiEndpoint || _handle.LastConnectFailed))
            {
                ThrowMultiConnectNotSupported();
            }

            // If the socket was already used for a failed connect attempt, replace it
            // with a fresh one, copying over all of the state we've tracked.
            ReplaceHandleIfNecessaryAfterFailedConnect();
            Debug.Assert(!_handle.LastConnectFailed);
        }

        internal void ReplaceHandleIfNecessaryAfterFailedConnect()
        {
            if (!_handle.LastConnectFailed)
            {
                return;
            }

            SocketError errorCode = ReplaceHandle();
            if (errorCode != SocketError.Success)
            {
                throw new SocketException((int) errorCode);
            }

            _handle.LastConnectFailed = false;
        }

        internal SocketError ReplaceHandle()
        {
            // Copy out values from key options. The copied values should be kept in sync with the
            // handling in SafeCloseSocket.TrackOption.  Note that we copy these values out first, before
            // we change _handle, so that we can use the helpers on Socket which internally access _handle.
            // Then once _handle is switched to the new one, we can call the setters to propagate the retrieved
            // values back out to the new underlying socket.
            bool broadcast = false, dontFragment = false, noDelay = false;
            int receiveSize = -1, receiveTimeout = -1, sendSize = -1, sendTimeout = -1;
            short ttl = -1;
            LingerOption linger = null;
            if (_handle.IsTrackedOption(TrackedSocketOptions.DontFragment)) dontFragment = DontFragment;
            if (_handle.IsTrackedOption(TrackedSocketOptions.EnableBroadcast)) broadcast = EnableBroadcast;
            if (_handle.IsTrackedOption(TrackedSocketOptions.LingerState)) linger = LingerState;
            if (_handle.IsTrackedOption(TrackedSocketOptions.NoDelay)) noDelay = NoDelay;
            if (_handle.IsTrackedOption(TrackedSocketOptions.ReceiveBufferSize)) receiveSize = ReceiveBufferSize;
            if (_handle.IsTrackedOption(TrackedSocketOptions.ReceiveTimeout)) receiveTimeout = ReceiveTimeout;
            if (_handle.IsTrackedOption(TrackedSocketOptions.SendBufferSize)) sendSize = SendBufferSize;
            if (_handle.IsTrackedOption(TrackedSocketOptions.SendTimeout)) sendTimeout = SendTimeout;
            if (_handle.IsTrackedOption(TrackedSocketOptions.Ttl)) ttl = Ttl;

            // Then replace the handle with a new one
            SafeCloseSocket oldHandle = _handle;
            SocketError errorCode = SocketPal.CreateSocket(_addressFamily, _socketType, _protocolType, out _handle);
            oldHandle.TransferTrackedState(_handle);
            oldHandle.Dispose();
            if (errorCode != SocketError.Success)
            {
                return errorCode;
            }

            // And put back the copied settings.  For DualMode, we use the value stored in the _handle
            // rather than querying the socket itself, as on Unix stacks binding a dual-mode socket to
            // an IPv6 address may cause the IPv6Only setting to revert to true.
            if (_handle.IsTrackedOption(TrackedSocketOptions.DualMode)) DualMode = _handle.DualMode;
            if (_handle.IsTrackedOption(TrackedSocketOptions.DontFragment)) DontFragment = dontFragment;
            if (_handle.IsTrackedOption(TrackedSocketOptions.EnableBroadcast)) EnableBroadcast = broadcast;
            if (_handle.IsTrackedOption(TrackedSocketOptions.LingerState)) LingerState = linger;
            if (_handle.IsTrackedOption(TrackedSocketOptions.NoDelay)) NoDelay = noDelay;
            if (_handle.IsTrackedOption(TrackedSocketOptions.ReceiveBufferSize)) ReceiveBufferSize = receiveSize;
            if (_handle.IsTrackedOption(TrackedSocketOptions.ReceiveTimeout)) ReceiveTimeout = receiveTimeout;
            if (_handle.IsTrackedOption(TrackedSocketOptions.SendBufferSize)) SendBufferSize = sendSize;
            if (_handle.IsTrackedOption(TrackedSocketOptions.SendTimeout)) SendTimeout = sendTimeout;
            if (_handle.IsTrackedOption(TrackedSocketOptions.Ttl)) Ttl = ttl;

            return SocketError.Success;
        }

        private static void ThrowMultiConnectNotSupported()
        {
            throw new PlatformNotSupportedException(SR.net_sockets_connect_multiconnect_notsupported);
        }

        private Socket GetOrCreateAcceptSocket(Socket acceptSocket, bool unused, string propertyName, out SafeCloseSocket handle)
        {
            // AcceptSocket is not supported on Unix.
            if (acceptSocket != null)
            {
                throw new PlatformNotSupportedException(SR.PlatformNotSupported_AcceptSocket);
            }

            handle = null;
            return null;
        }

#endregion

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static unsafe SocketAddress SocketAddress_from_buffer (byte *buffer, int len, out int error);

        internal static unsafe SocketAddress SocketAddress_from_buffer (byte[] buffer, int len)
        {
            Debug.Assert(len <= buffer.Length);
            fixed (byte* rawBuffer = buffer)
            {
                var result = SocketAddress_from_buffer (rawBuffer, len, out var error);
                if (error != (int)SocketError.Success)
                {
                    throw new SocketException((SocketError)error);
                }
                return result;
            }
        }

        internal static unsafe IntPtr Accept_internal (SafeSocketHandle safeHandle, byte *socketAddress, int *socketAddressLen, out int error)
        {
            try {
                safeHandle.RegisterForBlockingSyscall ();
                return Accept_internal2 (safeHandle.DangerousGetHandle (), socketAddress, socketAddressLen, out error);
            } finally {
                safeHandle.UnRegisterForBlockingSyscall ();
            }
        }

        /* Creates a new system socket, returning the handle */
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        extern static unsafe IntPtr Accept_internal2 (IntPtr sock, byte *socketAddress, int *socketAddressLen, out int error);

    }
}
