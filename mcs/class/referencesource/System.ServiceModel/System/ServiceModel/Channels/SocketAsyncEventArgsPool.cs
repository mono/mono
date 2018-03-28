//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Runtime;

    class SocketAsyncEventArgsPool : QueuedObjectPool<SocketAsyncEventArgs>
    {
        const int SingleBatchSize = 128 * 1024;
        const int MaxBatchCount = 16;
        const int MaxFreeCountFactor = 4;
        int acceptBufferSize;

        public SocketAsyncEventArgsPool(int acceptBufferSize)
        {
            if (acceptBufferSize <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("acceptBufferSize"));
            }

            this.acceptBufferSize = acceptBufferSize;
            int batchCount = (SingleBatchSize + acceptBufferSize - 1) / acceptBufferSize;
            if (batchCount > MaxBatchCount)
            {
                batchCount = MaxBatchCount;
            }

            Initialize(batchCount, batchCount * MaxFreeCountFactor);
        }

        public override bool Return(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            CleanupAcceptSocket(socketAsyncEventArgs);
            
            if (!base.Return(socketAsyncEventArgs))
            {
                this.CleanupItem(socketAsyncEventArgs);
                return false;
            }

            return true;
        }

        internal static void CleanupAcceptSocket(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Fx.Assert(socketAsyncEventArgs != null, "socketAsyncEventArgs should not be null.");

            Socket socket = socketAsyncEventArgs.AcceptSocket;
            if (socket != null)
            {
                socketAsyncEventArgs.AcceptSocket = null;

                try
                {
                    socket.Close(0);
                }
                catch (SocketException ex)
                {
                    FxTrace.Exception.TraceHandledException(ex, TraceEventType.Information);
                }
                catch (ObjectDisposedException ex)
                {
                    FxTrace.Exception.TraceHandledException(ex, TraceEventType.Information);
                }
            }
        }

        protected override void CleanupItem(SocketAsyncEventArgs item)
        {
            item.Dispose();
        }

        protected override SocketAsyncEventArgs Create()
        {
            SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
            byte[] acceptBuffer = DiagnosticUtility.Utility.AllocateByteArray(this.acceptBufferSize);
            eventArgs.SetBuffer(acceptBuffer, 0, this.acceptBufferSize);
            return eventArgs;
        }
    }
}
