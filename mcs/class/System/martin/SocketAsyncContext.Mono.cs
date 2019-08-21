using Microsoft.Win32.SafeHandles;

using System.Diagnostics;
using System.Threading;

namespace System.Net.Sockets
{
    sealed partial class SocketAsyncContext
    {
        public void SetNonBlocking()
        {
            Socket.Blocking_internal((SafeSocketHandle)_socket, false, out var error);
            if (error != 0)
                throw new SocketException(error);
        }

#if FIXME
        public SocketError ConnectAsync(byte[] socketAddress, int socketAddressLen, Action<SocketError> callback)
        {
            Debug.Assert(socketAddress != null, "Expected non-null socketAddress");
            Debug.Assert(socketAddressLen > 0, $"Unexpected socketAddressLen: {socketAddressLen}");
            Debug.Assert(callback != null, "Expected non-null callback");

            SetNonBlocking();

            // Connect is different than the usual "readiness" pattern of other operations.
            // We need to initiate the connect before we try to complete it. 
            // Thus, always call TryStartConnect regardless of readiness.
            SocketError errorCode;
            if (SocketPal.TryStartConnect(_socket, socketAddress, socketAddressLen, out errorCode))
            {
                _socket.RegisterConnectResult(errorCode);
                return errorCode;
            }

            var operation = new ConnectOperation(this)
            {
                Callback = callback,
                SocketAddress = socketAddress,
                SocketAddressLen = socketAddressLen
            };

            IOSelector.Add (_socket.DangerousGetHandle (), new IOSelectorJob (IOOperation.Write, AsyncOperation.CompletionCallback, operation));
            return SocketError.IOPending;
        }
#endif

        abstract partial class AsyncOperation : IOAsyncResult
        {
            internal sealed override void CompleteDisposed()
            {
                Abort();
            }

            internal static void CompletionCallback(IOAsyncResult ioares)
            {
                var operation = (AsyncOperation)ioares;
                if (operation.TryComplete(operation.AssociatedContext))
                {
                    operation.InvokeCallback(true);
                }
            }
        }

#if FIXME
        abstract class AsyncOperation : IOAsyncResult
        {
            public readonly SocketAsyncContext AssociatedContext;
            protected object CallbackOrEvent;
            public SocketError ErrorCode;
            public byte[] SocketAddress;
            public int SocketAddressLen;

            internal override void CompleteDisposed()
            {
                Abort();
            }

            protected AsyncOperation(SocketAsyncContext context)
            {
                AssociatedContext = context;
            }

            internal static void CompletionCallback(IOAsyncResult ioares)
            {
                var operation = (AsyncOperation)ioares;
                if (operation.TryComplete(operation.AssociatedContext))
                {
                    operation.InvokeCallback(true);
                }
            }

            public bool TryComplete(SocketAsyncContext context)
            {
                return DoTryComplete(context);
            }

            protected abstract void Abort();

            protected abstract bool DoTryComplete(SocketAsyncContext context);

            public abstract void InvokeCallback(bool allowPooling);
        }

        class ConnectOperation : AsyncOperation
        {
            public Action<SocketError> Callback
            {
                set => CallbackOrEvent = value;
            }

            public ConnectOperation(SocketAsyncContext context) : base(context)
            {
            }

            protected override void Abort() { }

            protected override bool DoTryComplete(SocketAsyncContext context)
            {
                bool result = SocketPal.TryCompleteConnect(context._socket, SocketAddressLen, out ErrorCode);
                context._socket.RegisterConnectResult(ErrorCode);
                return result;
            }

            public override void InvokeCallback(bool allowPooling) =>
                ((Action<SocketError>)CallbackOrEvent)(ErrorCode);
        }
#endif
    }
}
