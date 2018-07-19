//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    class BufferedConnection : DelegatingConnection
    {
        byte[] writeBuffer;
        int writeBufferSize;
        int pendingWriteSize;
        Exception pendingWriteException;
        IOThreadTimer flushTimer;
        long flushTimeout;
        TimeSpan pendingTimeout;
        const int maxFlushSkew = 100;

        public BufferedConnection(IConnection connection, TimeSpan flushTimeout, int writeBufferSize)
            : base(connection)
        {
            this.flushTimeout = Ticks.FromTimeSpan(flushTimeout);
            this.writeBufferSize = writeBufferSize;
        }

        object ThisLock
        {
            get { return this; }
        }

        public override void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Flush(timeoutHelper.RemainingTime());
            base.Close(timeoutHelper.RemainingTime(), asyncAndLinger);
        }

        void CancelFlushTimer()
        {
            if (flushTimer != null)
            {
                flushTimer.Cancel();
                pendingTimeout = TimeSpan.Zero;
            }
        }

        void Flush(TimeSpan timeout)
        {
            ThrowPendingWriteException();

            lock (ThisLock)
            {
                FlushCore(timeout);
            }
        }

        void FlushCore(TimeSpan timeout)
        {
            if (pendingWriteSize > 0)
            {
                ThreadTrace.Trace("BC:Flush");
                Connection.Write(writeBuffer, 0, pendingWriteSize, false, timeout);
                pendingWriteSize = 0;
            }
        }

        void OnFlushTimer(object state)
        {
            ThreadTrace.Trace("BC:Flush timer");
            lock (ThisLock)
            {
                try
                {
                    FlushCore(pendingTimeout);
                    pendingTimeout = TimeSpan.Zero;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    pendingWriteException = e;
                    CancelFlushTimer();
                }
            }
        }

        void SetFlushTimer()
        {
            if (this.flushTimer == null)
            {
                int flushSkew = Ticks.ToMilliseconds(Math.Min(this.flushTimeout / 10, Ticks.FromMilliseconds(maxFlushSkew)));
                this.flushTimer = new IOThreadTimer(new Action<object>(OnFlushTimer), null, true, flushSkew);
            }
            this.flushTimer.Set(Ticks.ToTimeSpan(this.flushTimeout));
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, SR.GetString(
                    SR.ValueMustBePositive)));
            }

            ThrowPendingWriteException();

            if (immediate || flushTimeout == 0)
            {
                ThreadTrace.Trace("BC:Write now");
                WriteNow(buffer, offset, size, timeout, bufferManager);
            }
            else
            {
                ThreadTrace.Trace("BC:Write later");
                WriteLater(buffer, offset, size, timeout);
                bufferManager.ReturnBuffer(buffer);
            }

            ThreadTrace.Trace("BC:Write done");
        }

        public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, SR.GetString(
                    SR.ValueMustBePositive)));
            }

            ThrowPendingWriteException();

            if (immediate || flushTimeout == 0)
            {
                ThreadTrace.Trace("BC:Write now");
                WriteNow(buffer, offset, size, timeout);
            }
            else
            {
                ThreadTrace.Trace("BC:Write later");
                WriteLater(buffer, offset, size, timeout);
            }

            ThreadTrace.Trace("BC:Write done");
        }

        void WriteNow(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            WriteNow(buffer, offset, size, timeout, null);
        }

        void WriteNow(byte[] buffer, int offset, int size, TimeSpan timeout, BufferManager bufferManager)
        {
            lock (ThisLock)
            {
                if (pendingWriteSize > 0)
                {
                    int remainingSize = writeBufferSize - pendingWriteSize;
                    CancelFlushTimer();
                    if (size <= remainingSize)
                    {
                        Buffer.BlockCopy(buffer, offset, writeBuffer, pendingWriteSize, size);
                        if (bufferManager != null)
                        {
                            bufferManager.ReturnBuffer(buffer);
                        }
                        pendingWriteSize += size;
                        FlushCore(timeout);
                        return;
                    }
                    else
                    {
                        TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                        FlushCore(timeoutHelper.RemainingTime());
                        timeout = timeoutHelper.RemainingTime();
                    }
                }

                if (bufferManager == null)
                {
                    Connection.Write(buffer, offset, size, true, timeout);
                }
                else
                {
                    Connection.Write(buffer, offset, size, true, timeout, bufferManager);
                }
            }
        }

        void WriteLater(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            lock (ThisLock)
            {
                bool setTimer = (pendingWriteSize == 0);
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                while (size > 0)
                {
                    if (size >= writeBufferSize && pendingWriteSize == 0)
                    {
                        Connection.Write(buffer, offset, size, false, timeoutHelper.RemainingTime());
                        size = 0;
                    }
                    else
                    {
                        if (writeBuffer == null)
                        {
                            writeBuffer = DiagnosticUtility.Utility.AllocateByteArray(writeBufferSize);
                        }

                        int remainingSize = writeBufferSize - pendingWriteSize;
                        int copySize = size;
                        if (copySize > remainingSize)
                        {
                            copySize = remainingSize;
                        }

                        Buffer.BlockCopy(buffer, offset, writeBuffer, pendingWriteSize, copySize);
                        pendingWriteSize += copySize;
                        if (pendingWriteSize == writeBufferSize)
                        {
                            FlushCore(timeoutHelper.RemainingTime());
                            setTimer = true;
                        }
                        size -= copySize;
                        offset += copySize;
                    }
                }
                if (pendingWriteSize > 0)
                {
                    if (setTimer)
                    {
                        SetFlushTimer();
                        pendingTimeout = TimeoutHelper.Add(pendingTimeout, timeoutHelper.RemainingTime());
                    }
                }
                else
                {
                    CancelFlushTimer();
                }
            }
        }

        public override AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            ThreadTrace.Trace("BC:BeginWrite");
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Flush(timeoutHelper.RemainingTime());
            return base.BeginWrite(buffer, offset, size, immediate, timeoutHelper.RemainingTime(), callback, state);
        }

        public override void EndWrite()
        {
            ThreadTrace.Trace("BC:EndWrite");
            base.EndWrite();
        }

        public override void Shutdown(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Flush(timeoutHelper.RemainingTime());
            base.Shutdown(timeoutHelper.RemainingTime());
        }

        void ThrowPendingWriteException()
        {
            if (pendingWriteException != null)
            {
                lock (ThisLock)
                {
                    if (pendingWriteException != null)
                    {
                        Exception exceptionTothrow = pendingWriteException;
                        pendingWriteException = null;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exceptionTothrow);
                    }
                }
            }
        }
    }

    class BufferedConnectionInitiator : IConnectionInitiator
    {
        int writeBufferSize;
        TimeSpan flushTimeout;
        IConnectionInitiator connectionInitiator;

        public BufferedConnectionInitiator(IConnectionInitiator connectionInitiator, TimeSpan flushTimeout, int writeBufferSize)
        {
            this.connectionInitiator = connectionInitiator;
            this.flushTimeout = flushTimeout;
            this.writeBufferSize = writeBufferSize;
        }

        protected TimeSpan FlushTimeout
        {
            get
            {
                return this.flushTimeout;
            }
        }

        protected int WriteBufferSize
        {
            get
            {
                return this.writeBufferSize;
            }
        }

        public IConnection Connect(Uri uri, TimeSpan timeout)
        {
            return new BufferedConnection(connectionInitiator.Connect(uri, timeout), flushTimeout, writeBufferSize);
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return connectionInitiator.BeginConnect(uri, timeout, callback, state);
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            return new BufferedConnection(connectionInitiator.EndConnect(result), flushTimeout, writeBufferSize);
        }
    }

    class BufferedConnectionListener : IConnectionListener
    {
        int writeBufferSize;
        TimeSpan flushTimeout;
        IConnectionListener connectionListener;

        public BufferedConnectionListener(IConnectionListener connectionListener, TimeSpan flushTimeout, int writeBufferSize)
        {
            this.connectionListener = connectionListener;
            this.flushTimeout = flushTimeout;
            this.writeBufferSize = writeBufferSize;
        }

        public void Dispose()
        {
            connectionListener.Dispose();
        }

        public void Listen()
        {
            connectionListener.Listen();
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return connectionListener.BeginAccept(callback, state);

        }

        public IConnection EndAccept(IAsyncResult result)
        {
            IConnection connection = connectionListener.EndAccept(result);
            if (connection == null)
            {
                return connection;
            }

            return new BufferedConnection(connection, flushTimeout, writeBufferSize);
        }
    }
}
