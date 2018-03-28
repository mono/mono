//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Threading;

    sealed class UdpSocketReceiveManager
    {
        BufferManager bufferManager;
        Action<object> continueReceivingCallback;
        int maxPendingReceivesPerSocket;
        AsyncCallback onReceiveFrom;
        Action<object> onStartReceiving;
        int openCount;
        IUdpReceiveHandler receiveHandler;
        UdpSocket[] receiveSockets;
        Action onMessageDequeued;
        object thisLock;
        int messageBufferSize;
        ConnectionBufferPool receiveBufferPool;

        internal UdpSocketReceiveManager(UdpSocket[] receiveSockets, int maxPendingReceivesPerSocket, BufferManager bufferManager, IUdpReceiveHandler receiveHandler)
        {
            Fx.Assert(receiveSockets != null, "receiveSockets parameter is null");
            Fx.Assert(receiveSockets.Length > 0, "receiveSockets parameter is empty");
            Fx.Assert(maxPendingReceivesPerSocket > 0, "maxPendingReceivesPerSocket can't be <= 0");
            Fx.Assert(receiveHandler.MaxReceivedMessageSize > 0, "maxReceivedMessageSize must be > 0");
            Fx.Assert(bufferManager != null, "bufferManager argument should not be null");
            Fx.Assert(receiveHandler != null, "receiveHandler should not be null");

            this.receiveHandler = receiveHandler;
            this.thisLock = new object();
            this.bufferManager = bufferManager;
            this.receiveSockets = receiveSockets;
            this.maxPendingReceivesPerSocket = maxPendingReceivesPerSocket;
            this.messageBufferSize = UdpUtility.ComputeMessageBufferSize(receiveHandler.MaxReceivedMessageSize);

            int maxPendingReceives = maxPendingReceivesPerSocket * receiveSockets.Length;
            this.receiveBufferPool = new ConnectionBufferPool(this.messageBufferSize, maxPendingReceives);
        }

        bool IsDisposed
        {
            get
            {
                return this.openCount < 0;
            }
        }

        public void SetReceiveHandler(IUdpReceiveHandler handler)
        {
            Fx.Assert(handler != null, "IUdpReceiveHandler can't be null");
            Fx.Assert(handler.MaxReceivedMessageSize == this.receiveHandler.MaxReceivedMessageSize, "new receive handler's max message size doesn't match");
            Fx.Assert(this.openCount > 0, "SetReceiveHandler called on a closed UdpSocketReceiveManager");
            this.receiveHandler = handler;
        }

        public void Close()
        {
            lock (this.thisLock)
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.openCount--;

                if (this.openCount == 0)
                {
                    this.openCount = -1;
                    this.receiveBufferPool.Close();
                    this.bufferManager.Clear();

                    for (int i = 0; i < this.receiveSockets.Length; i++)
                    {
                        this.receiveSockets[i].Close();
                    }
                }
            }
        }

        public void Open()
        {
            lock (this.thisLock)
            {
                ThrowIfDisposed();

                this.openCount++;

                if (this.openCount == 1)
                {
                    for (int i = 0; i < this.receiveSockets.Length; i++)
                    {
                        this.receiveSockets[i].Open();
                    }

                    this.onMessageDequeued = new Action(OnMessageDequeued);
                    this.onReceiveFrom = Fx.ThunkCallback(new AsyncCallback(OnReceiveFrom));
                    this.continueReceivingCallback = new Action<object>(ContinueReceiving);
                }
            }


            try
            {
                if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    EnsureReceiving();
                }
                else
                {
                    if (this.onStartReceiving == null)
                    {
                        this.onStartReceiving = new Action<object>(OnStartReceiving);
                    }

                    ActionItem.Schedule(this.onStartReceiving, this);
                }
            }
            catch (Exception ex)
            {
                if (!TryHandleException(ex))
                {
                    throw;
                }
            }
        }

        static void OnStartReceiving(object state)
        {
            UdpSocketReceiveManager thisPtr = (UdpSocketReceiveManager)state;

            try
            {
                if (thisPtr.IsDisposed)
                {
                    return;
                }

                thisPtr.EnsureReceiving();
            }
            catch (Exception ex)
            {
                if (!thisPtr.TryHandleException(ex))
                {
                    throw;
                }
            }
        }

        void OnMessageDequeued()
        {
            try
            {
                EnsureReceiving();
            }
            catch (Exception ex)
            {
                if (!TryHandleException(ex))
                {
                    throw;
                }
            }
        }

        void ContinueReceiving(object socket)
        {            
            try
            {
                while (StartAsyncReceive(socket as UdpSocket))
                {
                    Fx.Assert(Thread.CurrentThread.IsThreadPoolThread, "Receive loop is running on a non-threadpool thread.  If this thread disappears while a completion port operation is outstanding, then the operation will get canceled.");
                }
            }
            catch (Exception ex)
            {
                if (!TryHandleException(ex))
                {
                    throw;
                }
            }
        }

        void OnReceiveFrom(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            UdpSocketReceiveState state = (UdpSocketReceiveState)result.AsyncState;

            ArraySegment<byte> messageBytes;
            bool continueReceiving = true;

            try
            {
                lock (this.thisLock)
                {
                    if (this.IsDisposed)
                    {
                        return;
                    }

                    messageBytes = EndReceiveFrom(result, state);
                }
                messageBytes = this.CopyMessageIntoBufferManager(messageBytes);

                //when receiveHandler.HandleDataReceived is called, it will return the buffer to the buffer manager.
                continueReceiving = this.receiveHandler.HandleDataReceived(messageBytes, state.RemoteEndPoint, state.Socket.InterfaceIndex, this.onMessageDequeued);
            }
            catch (Exception ex)
            {
                if (!TryHandleException(ex))
                {
                    throw;
                }
            }
            finally
            {
                if (!this.IsDisposed && continueReceiving)
                {
                    ContinueReceiving(state.Socket);
                }
            }
        }

        //returns true if receive completed synchronously, false otherwise
        bool StartAsyncReceive(UdpSocket socket)
        {
            Fx.Assert(socket != null, "UdpSocketReceiveManager.StartAsyncReceive: Socket should never be null");                         
            bool completedSync = false;

            ArraySegment<byte> messageBytes = default(ArraySegment<byte>);
            UdpSocketReceiveState state = null;

            lock (this.thisLock)
            {
                if (!this.IsDisposed && socket.PendingReceiveCount < this.maxPendingReceivesPerSocket)
                {
                    IAsyncResult result = null;
                    byte[] receiveBuffer = this.receiveBufferPool.Take();
                    try
                    {
                        state = new UdpSocketReceiveState(socket, receiveBuffer);
                        EndPoint remoteEndpoint = socket.CreateIPAnyEndPoint();

                        result = socket.BeginReceiveFrom(receiveBuffer, 0, receiveBuffer.Length, ref remoteEndpoint, onReceiveFrom, state);
                    }
                    catch (Exception e)
                    {
                        if (!Fx.IsFatal(e))
                        {
                            this.receiveBufferPool.Return(receiveBuffer);
                        }
                        throw;
                    }

                    if (result.CompletedSynchronously)
                    {
                        completedSync = true;
                        messageBytes = EndReceiveFrom(result, state);
                    }
                }
            }

            if (completedSync)
            {
                messageBytes = this.CopyMessageIntoBufferManager(messageBytes);
                //if HandleDataReceived returns false, it means that the max pending message count was hit.
                //when receiveHandler.HandleDataReceived is called (whether now or later), it will return the buffer to the buffer manager.
                return this.receiveHandler.HandleDataReceived(messageBytes, state.RemoteEndPoint, state.Socket.InterfaceIndex, this.onMessageDequeued);
            }

            return false;
        }

        ArraySegment<byte> CopyMessageIntoBufferManager(ArraySegment<byte> receiveBuffer)
        {
            int dataLength = receiveBuffer.Count;
            byte[] dataBuffer = this.bufferManager.TakeBuffer(dataLength);
            Array.Copy(receiveBuffer.Array, receiveBuffer.Offset, dataBuffer, 0, dataLength);
            this.receiveBufferPool.Return(receiveBuffer.Array);
            return new ArraySegment<byte>(dataBuffer, 0, dataLength);
        }

        void EnsureReceiving()
        {
            for (int i = 0; i < this.receiveSockets.Length; i++)
            {
                UdpSocket socket = this.receiveSockets[i];
                                
                while (!this.IsDisposed && socket.PendingReceiveCount < this.maxPendingReceivesPerSocket)
                {
                    bool jumpThreads = false;
                    try
                    {
                        if (StartAsyncReceive(socket) && !Thread.CurrentThread.IsThreadPoolThread)
                        {
                            jumpThreads = true;
                        }
                    }
                    catch (CommunicationException ex)
                    {
                        //message too big, ICMP errors, etc, are translated by the socket into a CommunicationException derived exception.
                        //These should not be fatal to the receive loop, so we need to continue receiving.
                        this.receiveHandler.HandleAsyncException(ex);
                        jumpThreads = !Thread.CurrentThread.IsThreadPoolThread;
                    }

                    if (jumpThreads)
                    {
                        ActionItem.Schedule(this.continueReceivingCallback, socket);
                        break; //while loop.
                    }
                }
            }
        }

        void ThrowIfDisposed()
        {
            if (this.IsDisposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException("SocketReceiveManager"));
            }
        }

        bool TryHandleException(Exception ex)
        {
            if (Fx.IsFatal(ex))
            {
                return false;
            }

            this.receiveHandler.HandleAsyncException(ex);
            return true;
        }

        //call under a lock
        ArraySegment<byte> EndReceiveFrom(IAsyncResult result, UdpSocketReceiveState state)
        {
            try
            {
                EndPoint remoteEndpoint = null;
                ArraySegment<byte> messageBytes = state.Socket.EndReceiveFrom(result, ref remoteEndpoint);
                state.RemoteEndPoint = remoteEndpoint;
                Fx.Assert(messageBytes.Array == state.ReceiveBuffer, "Array returned by Socket.EndReceiveFrom must match the array passed in through the UdpSocketReceiveState");
                return messageBytes;
            }
            catch (Exception e)
            {
                if (!Fx.IsFatal(e))
                {
                    this.receiveBufferPool.Return(state.ReceiveBuffer);
                }
                throw;
            }
        }

        internal class UdpSocketReceiveState
        {
            public UdpSocketReceiveState(UdpSocket socket, byte[] receiveBuffer)
            {
                Fx.Assert(socket != null, "UdpSocketReceiveState.ctor: socket should not be null");

                this.Socket = socket;
                this.ReceiveBuffer = receiveBuffer;
            }

            public EndPoint RemoteEndPoint
            {
                get;
                set;
            }

            internal UdpSocket Socket
            {
                get;
                private set;
            }

            internal byte[] ReceiveBuffer
            {
                get;
                private set;
            }
        }
    }
}
