using Microsoft.Win32.SafeHandles;

using System.Diagnostics;
using System.Threading;

namespace System.Net.Sockets
{
    partial class SafeCloseSocket
    {

#region CoreFX Code

        private int _receiveTimeout = -1;
        private int _sendTimeout = -1;
        private bool _nonBlocking;
        private bool _underlyingHandleNonBlocking;
        private SocketAsyncContext _asyncContext;

        private TrackedSocketOptions _trackedOptions;
        internal bool LastConnectFailed { get; set; }
        internal bool DualMode { get; set; }
        internal bool ExposedHandleOrUntrackedConfiguration { get; private set; }

        public void RegisterConnectResult(SocketError error)
        {
            switch (error)
            {
                case SocketError.Success:
                case SocketError.WouldBlock:
                    break;
                default:
                    LastConnectFailed = true;
                    break;
            }
        }

        public void TransferTrackedState(SafeCloseSocket target)
        {
            target._trackedOptions = _trackedOptions;
            target.LastConnectFailed = LastConnectFailed;
            target.DualMode = DualMode;
            target.ExposedHandleOrUntrackedConfiguration = ExposedHandleOrUntrackedConfiguration;
        }

        public void SetExposed() => ExposedHandleOrUntrackedConfiguration = true;

        public bool IsTrackedOption(TrackedSocketOptions option) => (_trackedOptions & option) != 0;

        public void TrackOption(SocketOptionLevel level, SocketOptionName name)
        {
            // As long as only these options are set, we can support Connect{Async}(IPAddress[], ...).
            switch (level)
            {
                case SocketOptionLevel.Tcp:
                    switch (name)
                    {
                        case SocketOptionName.NoDelay: _trackedOptions |= TrackedSocketOptions.NoDelay; return;
                    }
                    break;

                case SocketOptionLevel.IP:
                    switch (name)
                    {
                        case SocketOptionName.DontFragment: _trackedOptions |= TrackedSocketOptions.DontFragment; return;
                        case SocketOptionName.IpTimeToLive: _trackedOptions |= TrackedSocketOptions.Ttl; return;
                    }
                    break;

                case SocketOptionLevel.IPv6:
                    switch (name)
                    {
                        case SocketOptionName.IPv6Only: _trackedOptions |= TrackedSocketOptions.DualMode; return;
                        case SocketOptionName.IpTimeToLive: _trackedOptions |= TrackedSocketOptions.Ttl; return;
                    }
                    break;

                case SocketOptionLevel.Socket:
                    switch (name)
                    {
                        case SocketOptionName.Broadcast: _trackedOptions |= TrackedSocketOptions.EnableBroadcast; return;
                        case SocketOptionName.Linger: _trackedOptions |= TrackedSocketOptions.LingerState; return;
                        case SocketOptionName.ReceiveBuffer: _trackedOptions |= TrackedSocketOptions.ReceiveBufferSize; return;
                        case SocketOptionName.ReceiveTimeout: _trackedOptions |= TrackedSocketOptions.ReceiveTimeout; return;
                        case SocketOptionName.SendBuffer: _trackedOptions |= TrackedSocketOptions.SendBufferSize; return;
                        case SocketOptionName.SendTimeout: _trackedOptions |= TrackedSocketOptions.SendTimeout; return;
                    }
                    break;
            }

            // For any other settings, we need to track that they were used so that we can error out
            // if a Connect{Async}(IPAddress[],...) attempt is made.
            ExposedHandleOrUntrackedConfiguration = true;
        }

        public SocketAsyncContext AsyncContext
        {
            get
            {
                if (Volatile.Read(ref _asyncContext) == null)
                {
                    Interlocked.CompareExchange(ref _asyncContext, new SocketAsyncContext(this), null);
                }

                return _asyncContext;
            }
        }

        // This will set the underlying OS handle to be nonblocking, for whatever reason --
        // performing an async operation or using a timeout will cause this to happen.
        // Once the OS handle is nonblocking, it never transitions back to blocking.
        private void SetHandleNonBlocking()
        {
            // We don't care about synchronization because this is idempotent
            if (!_underlyingHandleNonBlocking)
            {
                AsyncContext.SetNonBlocking();
                _underlyingHandleNonBlocking = true;
            }
        }

        public bool IsNonBlocking
        {
            get
            {
                return _nonBlocking;
            }
            set
            {
                _nonBlocking = value;

                //
                // If transitioning to non-blocking, we need to set the native socket to non-blocking mode.
                // If we ever transition back to blocking, we keep the native socket in non-blocking mode, and emulate
                // blocking.  This avoids problems with switching to native blocking while there are pending async
                // operations.
                //
                if (value)
                {
                    SetHandleNonBlocking();
                }
            }
        }

        public int ReceiveTimeout
        {
            get
            {
                return _receiveTimeout;
            }
            set
            {
                Debug.Assert(value == -1 || value > 0, $"Unexpected value: {value}");
                _receiveTimeout = value;
            }
        }

        public int SendTimeout
        {
            get
            {
                return _sendTimeout;
            }
            set
            {
                Debug.Assert(value == -1 || value > 0, $"Unexpected value: {value}");
                _sendTimeout = value;
            }
        }

        public bool IsDisconnected { get; private set; } = false;

        public void SetToDisconnected()
        {
            IsDisconnected = true;
        }

#endregion

        protected SafeCloseSocket(bool ownsHandle) : base(ownsHandle)
        {
        }

        protected void InnerReleaseHandle()
        {
            if (_asyncContext != null)
            {
                _asyncContext.Close();
            }
        }

        public static unsafe SafeCloseSocket CreateSocket(IntPtr fileDescriptor)
        {
            return new SafeSocketHandle(fileDescriptor, true);
        }

        public static SocketError CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, out SafeSocketHandle socket)
        {
            socket = Socket.Create_Socket_internal (addressFamily, socketType, protocolType, out var error);
            return (SocketError)error;
        }

        public static unsafe SocketError Accept(SafeCloseSocket socketHandle, byte[] socketAddress, ref int socketAddressSize, out SafeCloseSocket socket)
        {
            socket = Socket.Accept_internal ((SafeSocketHandle)socketHandle, out var error, true);
            return (SocketError)error;
        }

        partial class InnerSafeCloseSocket
        {
            private unsafe SocketError InnerReleaseHandle()
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>Flags that correspond to exposed options on Socket.</summary>
    [Flags]
    internal enum TrackedSocketOptions : short
    {
        DontFragment = 0x1,
        DualMode = 0x2,
        EnableBroadcast = 0x4,
        LingerState = 0x8,
        NoDelay = 0x10,
        ReceiveBufferSize = 0x20,
        ReceiveTimeout = 0x40,
        SendBufferSize = 0x80,
        SendTimeout = 0x100,
        Ttl = 0x200,
    }
}
