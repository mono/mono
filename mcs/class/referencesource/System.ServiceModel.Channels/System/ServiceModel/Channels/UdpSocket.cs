//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Threading;
#if DEBUG
    using System.Diagnostics;
#endif

    class UdpSocket
    {
        int openCount;
        int timeToLive;
        int pendingReceiveCount;
        Socket socket;
#if DEBUG
        StackTrace disposeStack;
#endif
        
        public UdpSocket(Socket socket, int interfaceIndex)
        {
            Fx.Assert(socket != null, "Socket argument cannot be null");

            this.openCount = 0;
            this.pendingReceiveCount = 0;

            this.ThisLock = new object();

            this.socket = socket;
            this.InterfaceIndex = interfaceIndex;
            this.timeToLive = socket.Ttl;
        }

        enum TransferDirection : byte
        {
            Send,
            Receive,
        }

        public AddressFamily AddressFamily
        {
            get
            {
                return this.socket.AddressFamily;
            }
        }

        public int PendingReceiveCount
        {
            get
            {
                return this.pendingReceiveCount;
            }
        }

        //value of UdpConstants.Defaults.InterfaceIndex (-1) if not a multicast socket
        internal int InterfaceIndex
        {
            get;
            private set;
        }    

        internal bool IsDisposed
        { get { return this.openCount < 0; } }

        internal object ThisLock
        {
            get;
            private set;
        }

        //must be called under a lock in receive loop
        public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, ref EndPoint remoteEndPoint, AsyncCallback callback, object state)
        {
            UdpUtility.ValidateBufferBounds(buffer, offset, size);

            ThrowIfNotOpen();

            bool success = false;
            IAsyncResult asyncResult = null;
            try
            {
                this.pendingReceiveCount++;

                asyncResult = new ReceiveFromAsyncResult(
                    this.socket,
                    new ArraySegment<byte>(buffer, offset, size),
                    remoteEndPoint,
                    size - offset,
                    this.timeToLive,
                    callback,
                    state);
                success = true;

                return asyncResult;
            }
            finally
            {
                if (!success)
                {
                    this.pendingReceiveCount--;
                }
            }
        }

        public void Close()
        {
            bool cleanup = false;
            lock (this.ThisLock)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                //UdpUtility.CreateListenSocketsOnUniquePort can create a socket and then close it without ever calling
                //UdpSocket.Open() if it fails to bind on both IPv4 and IPv6.  If this happens, then openCount will still be at zero.
                if (this.openCount > 0)
                {
                    this.openCount--;
                }
                
                if (this.openCount == 0)
                {
                    cleanup = true;
                    this.openCount = -1;
                }
            }

            if (cleanup)
            {
#if DEBUG
                if (!Fx.FastDebug)
                {
                    disposeStack = new StackTrace();
                }
#endif
                //non-zero sendTimeout causes the socket to block on a receive while looking for an EOF, which will never come
                this.socket.Close(0);
            }
        }

        //must be called under a lock in receive loop
        public ArraySegment<byte> EndReceiveFrom(IAsyncResult result, ref EndPoint remoteEndPoint)
        {
            this.pendingReceiveCount--;
            return ReceiveFromAsyncResult.End(result, ref remoteEndPoint);
        }

        internal EndPoint CreateIPAnyEndPoint()
        {
            if (this.AddressFamily == AddressFamily.InterNetwork)
            {
                return new IPEndPoint(IPAddress.Any, 0);
            }
            else
            {
                return new IPEndPoint(IPAddress.IPv6Any, 0);
            }
        }

        public void Open()
        {
            lock (this.ThisLock)
            {
                if (this.IsDisposed)
                {
                    throw FxTrace.Exception.AsError(new ObjectDisposedException("UdpSocket"));
                }
                this.openCount++;
            }
        }

        public int SendTo(byte[] buffer, int offset, int size, EndPoint remoteEndPoint)
        {
            ThrowIfNotOpen();
            UdpUtility.ValidateBufferBounds(buffer, offset, size);
            
            try
            {
                int count = this.socket.SendTo(buffer, offset, size, SocketFlags.None, remoteEndPoint);
                Fx.Assert(count == size, "Bytes sent on the wire should be the same as the bytes specified");

                return count;
            }
            catch (SocketException socketException)
            {
                throw FxTrace.Exception.AsError(ConvertNetworkError(socketException, size - offset, TransferDirection.Send, this.timeToLive));
            }
        }

        public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, EndPoint remoteEndPoint, AsyncCallback callback, object state)
        {
            ThrowIfNotOpen();
            UdpUtility.ValidateBufferBounds(buffer, offset, size);

            return new SendToAsyncResult(this.socket, buffer, offset, size, remoteEndPoint, this.timeToLive, callback, state);
        }

        public int EndSendTo(IAsyncResult result)
        {
            return SendToAsyncResult.End(result);
        }

        static Exception ConvertNetworkError(SocketException socketException, ReceiveFromAsyncResult result)
        {
            return ConvertNetworkError(socketException, result.MessageSize, TransferDirection.Receive, result.TimeToLive);
        }

        //  size:   sending => the size of the data being sent
        //          Receiving => the max message size we can receive
        //  remoteEndPoint: remote endpoint reported when error occured
        static Exception ConvertNetworkError(SocketException socketException, int size, TransferDirection direction, int timeToLive)
        {
            Exception result = null;

            if (socketException.ErrorCode == UnsafeNativeMethods.ERROR_INVALID_HANDLE)
            {
                //This would likely indicate a bug in our ref-counting 
                //for instance, a channel is closing the socket multiple times...
                Fx.Assert("The socket appears to have been closed unexpectedly.  This probably indicates incorrect ref counting (i.e. a channel is closing the socket multiple times)");
                result = new CommunicationObjectAbortedException(socketException.Message, socketException);
            }
            else
            {
                string errorMessage;
                switch (socketException.SocketErrorCode)
                {
                    case SocketError.MessageSize: //10040
                        errorMessage = (direction == TransferDirection.Send ? SR.UdpMaxMessageSendSizeExceeded(size) : SR.MaxReceivedMessageSizeExceeded(size));
                        Exception inner = new QuotaExceededException(errorMessage, socketException);
                        result = new ProtocolException(errorMessage, inner);
                        break;
                    case SocketError.NetworkReset: //10052
                        //ICMP: Time Exceeded (TTL expired)
                        //see http://tools.ietf.org/html/rfc792
                        result = new CommunicationException(SR.IcmpTimeExpired(timeToLive), socketException);
                        break;
                    case SocketError.ConnectionReset: //10054
                        //ICMP: Destination Unreachable (target host/port/etc not reachable)
                        //see http://tools.ietf.org/html/rfc792
                        result = new CommunicationException(SR.IcmpDestinationUnreachable, socketException);
                        break;
                    default:
                        errorMessage = (direction == TransferDirection.Send ? SR.UdpSendException : SR.UdpReceiveException);
                        result = new CommunicationException(errorMessage, socketException);
                        break;
                }
            }

            Fx.Assert(result != null, "we should never return null");
            return result;
        }

        void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(this.GetType().ToString()));
            }
        }

        void ThrowIfNotOpen()
        {
            ThrowIfDisposed();

            if (this.openCount == 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ObjectNotOpen));
            }
        }

        class SendToAsyncResult : TypedAsyncResult<int>
        {
            Socket socket;
            int offset;
            int size;
            int timeToLive;

            static AsyncCallback onSendToComplete = Fx.ThunkCallback(OnSendToComplete);            

            public SendToAsyncResult(Socket socket, byte[] buffer, int offset, int size, EndPoint remoteEndPoint, int timeToLive, AsyncCallback callback, object state) 
                : base(callback, state)
            {
                this.socket = socket;
                this.offset = offset;
                this.size = size;
                this.timeToLive = timeToLive;
                int count = 0;

                try
                {
                    IAsyncResult socketAsyncResult = this.socket.BeginSendTo(buffer, offset, size, SocketFlags.None, remoteEndPoint, onSendToComplete, this);

                    if (!socketAsyncResult.CompletedSynchronously)
                    {
                        return;
                    }

                    count = this.socket.EndSendTo(socketAsyncResult);
                }
                catch (SocketException socketException)
                {
                    throw FxTrace.Exception.AsError(ConvertNetworkError(socketException, this.size - this.offset, TransferDirection.Send, this.timeToLive));
                }

                this.Complete(count, true);
            }
           
            static void OnSendToComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendToAsyncResult thisPtr = (SendToAsyncResult)result.AsyncState; 
                Exception completionException = null;
                int count = 0;
                
                try
                {
                    count = thisPtr.socket.EndSendTo(result);      
                }
                catch (SocketException socketException)
                {
                    completionException = ConvertNetworkError(socketException, thisPtr.size - thisPtr.offset, TransferDirection.Send, thisPtr.timeToLive);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    completionException = ex;
                }

                if (completionException != null)
                {
                    thisPtr.Complete(false, completionException);
                }
                else
                {
                    thisPtr.Complete(count, false);
                }   
            }
        }

        class ReceiveFromAsyncResult : TypedAsyncResult<ArraySegment<byte>>
        {
            static AsyncCallback onReceiveMessageFromCallback = Fx.ThunkCallback(new AsyncCallback(OnReceiveMessageFrom));
            Socket socket;
   
            public ReceiveFromAsyncResult(Socket socket, ArraySegment<byte> buffer, EndPoint remoteEndPoint, int messageSize, int timeToLive, AsyncCallback userCallback, object userState) :
                base(userCallback, userState)
            {
                this.RemoteEndPoint = remoteEndPoint;
                this.MessageSize = messageSize;
                this.socket = socket;
                this.Buffer = buffer;
                this.TimeToLive = timeToLive;

                ArraySegment<byte> data = default(ArraySegment<byte>);

                try
                {
                    IAsyncResult socketAsyncResult = this.socket.BeginReceiveFrom(this.Buffer.Array,
                        this.Buffer.Offset,
                        this.Buffer.Count,
                        SocketFlags.None,
                        ref remoteEndPoint,
                        onReceiveMessageFromCallback,
                        this);

                    if (!socketAsyncResult.CompletedSynchronously)
                    {
                        return;
                    }

                    data = EndReceiveFrom(socketAsyncResult);
                }
                catch (SocketException socketException)
                {
                    throw FxTrace.Exception.AsError(UdpSocket.ConvertNetworkError(socketException, this));
                }

                Complete(data, true);                
            }

            public EndPoint RemoteEndPoint
            {
                get;
                private set;
            }

            public int TimeToLive
            {
                get;
                private set;
            }

            //used when generating error messages for the user...
            internal int MessageSize
            {
                get;
                private set;
            }

            ArraySegment<byte> Buffer
            {
                get;
                set;
            }

            public static ArraySegment<byte> End(IAsyncResult result, ref EndPoint remoteEndPoint)
            {
                ArraySegment<byte> data = TypedAsyncResult<ArraySegment<byte>>.End(result);
                ReceiveFromAsyncResult receiveFromResult = (ReceiveFromAsyncResult)result;
                remoteEndPoint = receiveFromResult.RemoteEndPoint;
                return data;
            }

            static void OnReceiveMessageFrom(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ReceiveFromAsyncResult asyncResult = (ReceiveFromAsyncResult)result.AsyncState;

                Exception completionException = null;
                ArraySegment<byte> data = default(ArraySegment<byte>);

                try
                {
                    data = asyncResult.EndReceiveFrom(result);
                }
                catch (SocketException socketException)
                {
                    completionException = UdpSocket.ConvertNetworkError(socketException, asyncResult);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                }

                if (completionException != null)
                {
                    asyncResult.Complete(false, completionException);
                }
                else
                {
                    asyncResult.Complete(data, false);
                }
            }

            ArraySegment<byte> EndReceiveFrom(IAsyncResult result)
            {
                EndPoint remoteEndPoint = this.RemoteEndPoint;
                int count = this.socket.EndReceiveFrom(result, ref remoteEndPoint);
                this.RemoteEndPoint = remoteEndPoint;
                return new ArraySegment<byte>(this.Buffer.Array, this.Buffer.Offset, count);
            }
        }
    }
}
