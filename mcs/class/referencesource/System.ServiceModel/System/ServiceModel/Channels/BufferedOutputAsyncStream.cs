//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;


    /// <summary>
    /// 
    /// BufferedOutputAsyncStream is used for writing streamed response.
    /// For performance reasons, the behavior we want is chunk, chunk, chunk,.. terminating chunk  without a delay.
    /// We call BeginWrite,BeginWrite,BeginWrite and Close()(close sends the terminating chunk) without 
    /// waiting for all outstanding BeginWrites to complete.
    /// 
    /// BufferedOutputAsyncStream is not a general-purpose stream wrapper, it requires that the base stream
    ///     1. allow concurrent IO (for multiple BeginWrite calls)
    ///     2. support the BeginWrite,BeginWrite,BeginWrite,.. Close() calling pattern.
    /// 
    /// Currently BufferedOutputAsyncStream only used to wrap the System.Net.HttpResponseStream, which satisfy both requirements.
    /// 
    /// BufferedOutputAsyncStream can also be used when doing asynchronous operations. [....] operations are not allowed when an async
    /// operation is in-flight. If a [....] operation is in progress (i.e., data exists in our CurrentBuffer) and we issue an async operation, 
    /// we flush everything in the buffers (and block while doing so) before the async operation is allowed to proceed. 
    ///     
    /// </summary>
    class BufferedOutputAsyncStream : Stream
    {
        readonly Stream stream;
        readonly int bufferSize;
        readonly int bufferLimit;
        readonly BufferQueue buffers;
        ByteBuffer currentByteBuffer;
        int availableBufferCount;
        static AsyncEventArgsCallback onFlushComplete = new AsyncEventArgsCallback(OnFlushComplete);
        int asyncWriteCount;
        WriteAsyncState writeState;
        WriteAsyncArgs writeArgs;
        static AsyncEventArgsCallback onAsyncFlushComplete;
        static AsyncEventArgsCallback onWriteCallback;
        EventTraceActivity activity;
        bool closed;

        internal BufferedOutputAsyncStream(Stream stream, int bufferSize, int bufferLimit)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;
            this.bufferLimit = bufferLimit;
            this.buffers = new BufferQueue(this.bufferLimit);
            this.buffers.Add(new ByteBuffer(this, this.bufferSize, stream));
            this.availableBufferCount = 1;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return stream.CanWrite && (!this.closed); }
        }

        public override long Length
        {
            get
            {
#pragma warning suppress 56503 // [....], required by the Stream.Length contract
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ReadNotSupported)));
            }
        }

        public override long Position
        {
            get
            {
#pragma warning suppress 56503 // [....], required by the Stream.Position contract
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
            }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                if (TD.BufferedAsyncWriteStartIsEnabled())
                {
                    if (this.activity == null)
                    {
                        this.activity = EventTraceActivity.GetFromThreadOrCreate();
                    }
                }

                return this.activity;
            }
        }

        ByteBuffer GetCurrentBuffer()
        {
            // Dequeue will null out the buffer
            this.ThrowOnException();
            if (this.currentByteBuffer == null)
            {
                this.currentByteBuffer = this.buffers.CurrentBuffer();
            }

            return this.currentByteBuffer;
        }

        public override void Close()
        {
            this.FlushPendingBuffer();
            stream.Close();
            this.WaitForAllWritesToComplete();
            this.closed = true;
        }

        public override void Flush()
        {
            FlushPendingBuffer();
            stream.Flush();
        }

        void FlushPendingBuffer()
        {
            ByteBuffer asyncBuffer = this.buffers.CurrentBuffer();
            if (asyncBuffer != null)
            {
                this.DequeueAndFlush(asyncBuffer, onFlushComplete);
            }
        }

        void IncrementAsyncWriteCount()
        {
            if (Interlocked.Increment(ref this.asyncWriteCount) > 1)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.WriterAsyncWritePending)));
            }
        }

        void DecrementAsyncWriteCount()
        {
            if (Interlocked.Decrement(ref this.asyncWriteCount) != 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.NoAsyncWritePending)));
            }
        }

        void EnsureNoAsyncWritePending()
        {
            if (this.asyncWriteCount != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.WriterAsyncWritePending)));
            }
        }

        void EnsureOpened()
        {
            if (this.closed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StreamClosed)));
            }
        }

        ByteBuffer NextBuffer()
        {
            if (!this.AdjustBufferSize())
            {
                this.buffers.WaitForAny();
            }

            return this.GetCurrentBuffer();
        }

        bool AdjustBufferSize()
        {
            if (this.availableBufferCount < this.bufferLimit)
            {
                buffers.Add(new ByteBuffer(this, bufferSize, stream));
                this.availableBufferCount++;
                return true;
            }

            return false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ReadNotSupported)));
        }

        public override int ReadByte()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ReadNotSupported)));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
        }

        void WaitForAllWritesToComplete()
        {
            // Complete all outstanding writes 
            this.buffers.WaitForAllWritesToComplete();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.EnsureOpened();
            this.EnsureNoAsyncWritePending();

            while (count > 0)
            {
                ByteBuffer currentBuffer = this.GetCurrentBuffer();
                if (currentBuffer == null)
                {
                    currentBuffer = this.NextBuffer();
                }

                int freeBytes = currentBuffer.FreeBytes;   // space left in the CurrentBuffer
                if (freeBytes > 0)
                {
                    if (freeBytes > count)
                        freeBytes = count;

                    currentBuffer.CopyData(buffer, offset, freeBytes);
                    offset += freeBytes;
                    count -= freeBytes;
                }
                if (currentBuffer.FreeBytes == 0)
                {
                    this.DequeueAndFlush(currentBuffer, onFlushComplete);
                }
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.EnsureOpened();
            this.IncrementAsyncWriteCount();

            Fx.Assert(this.writeState == null ||
                this.writeState.Arguments == null ||
                this.writeState.Arguments.Count <= 0,
                "All data has not been written yet.");

            if (onWriteCallback == null)
            {
                onWriteCallback = new AsyncEventArgsCallback(OnWriteCallback);
                onAsyncFlushComplete = new AsyncEventArgsCallback(OnAsyncFlushComplete);
            }

            if (this.writeState == null)
            {
                this.writeState = new WriteAsyncState();
                this.writeArgs = new WriteAsyncArgs();
            }
            else
            {
                // Since writeState!= null, check if the stream has an  
                // exception as the async path has already been invoked.
                this.ThrowOnException();
            }

            this.writeArgs.Set(buffer, offset, count, callback, state);
            this.writeState.Set(onWriteCallback, this.writeArgs, this);
            if (this.WriteAsync(this.writeState) == AsyncCompletionResult.Completed)
            {
                this.writeState.Complete(true);
                if (callback != null)
                {
                    callback(this.writeState.CompletedSynchronouslyAsyncResult);
                }

                return this.writeState.CompletedSynchronouslyAsyncResult;
            }

            return this.writeState.PendingAsyncResult;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.DecrementAsyncWriteCount();
            this.ThrowOnException();
        }

        public override void WriteByte(byte value)
        {
            this.EnsureNoAsyncWritePending();
            ByteBuffer currentBuffer = this.GetCurrentBuffer();
            if (currentBuffer == null)
            {
                currentBuffer = NextBuffer();
            }

            currentBuffer.CopyData(value);
            if (currentBuffer.FreeBytes == 0)
            {
                this.DequeueAndFlush(currentBuffer, onFlushComplete);
            }
        }

        void DequeueAndFlush(ByteBuffer currentBuffer, AsyncEventArgsCallback callback)
        {
            // Dequeue does a checkout of the buffer from its slot.
            // the callback for the [....] path only enqueues the buffer. 
            // The WriteAsync callback needs to enqueue and also complete.
            this.currentByteBuffer = null;
            ByteBuffer dequeued = this.buffers.Dequeue();
            Fx.Assert(dequeued == currentBuffer, "Buffer queue in an inconsistent state.");

            WriteFlushAsyncEventArgs writeflushState = (WriteFlushAsyncEventArgs)currentBuffer.FlushAsyncArgs;
            if (writeflushState == null)
            {
                writeflushState = new WriteFlushAsyncEventArgs();
                currentBuffer.FlushAsyncArgs = writeflushState;
            }

            writeflushState.Set(callback, null, this);
            if (currentBuffer.FlushAsync() == AsyncCompletionResult.Completed)
            {
                this.buffers.Enqueue(currentBuffer);
                writeflushState.Complete(true);
            }
        }

        static void OnFlushComplete(IAsyncEventArgs state)
        {
            BufferedOutputAsyncStream thisPtr = (BufferedOutputAsyncStream)state.AsyncState;
            WriteFlushAsyncEventArgs flushState = (WriteFlushAsyncEventArgs)state;
            ByteBuffer byteBuffer = flushState.Result;
            thisPtr.buffers.Enqueue(byteBuffer);
        }

        AsyncCompletionResult WriteAsync(WriteAsyncState state)
        {
            Fx.Assert(state != null && state.Arguments != null, "Invalid WriteAsyncState parameter.");

            if (state.Arguments.Count == 0)
            {
                return AsyncCompletionResult.Completed;
            }

            byte[] buffer = state.Arguments.Buffer;
            int offset = state.Arguments.Offset;
            int count = state.Arguments.Count;

            ByteBuffer currentBuffer = this.GetCurrentBuffer();
            while (count > 0)
            {
                if (currentBuffer == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.WriteAsyncWithoutFreeBuffer)));
                }

                int freeBytes = currentBuffer.FreeBytes;   // space left in the CurrentBuffer
                if (freeBytes > 0)
                {
                    if (freeBytes > count)
                        freeBytes = count;

                    currentBuffer.CopyData(buffer, offset, freeBytes);
                    offset += freeBytes;
                    count -= freeBytes;
                }

                if (currentBuffer.FreeBytes == 0)
                {
                    this.DequeueAndFlush(currentBuffer, onAsyncFlushComplete);

                    // We might need to increase the number of buffers available
                    // if there is more data to be written or no buffer is available.
                    if (count > 0 || this.buffers.Count == 0)
                    {
                        this.AdjustBufferSize();
                    }
                }

                //Update state for any pending writes.
                state.Arguments.Offset = offset;
                state.Arguments.Count = count;

                // We can complete synchronously only 
                // if there a buffer available for writes.
                currentBuffer = this.GetCurrentBuffer();
                if (currentBuffer == null)
                {
                    if (this.buffers.TryUnlock())
                    {
                        return AsyncCompletionResult.Queued;
                    }

                    currentBuffer = this.GetCurrentBuffer();
                }
            }

            return AsyncCompletionResult.Completed;
        }

        static void OnAsyncFlushComplete(IAsyncEventArgs state)
        {
            BufferedOutputAsyncStream thisPtr = (BufferedOutputAsyncStream)state.AsyncState;
            Exception completionException = null;
            bool completeSelf = false;

            try
            {
                OnFlushComplete(state);

                if (thisPtr.buffers.TryAcquireLock())
                {
                    WriteFlushAsyncEventArgs flushState = (WriteFlushAsyncEventArgs)state;
                    if (flushState.Exception != null)
                    {
                        completeSelf = true;
                        completionException = flushState.Exception;
                    }
                    else
                    {
                        if (thisPtr.WriteAsync(thisPtr.writeState) == AsyncCompletionResult.Completed)
                        {
                            completeSelf = true;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                if (completionException == null)
                {
                    completionException = exception;
                }

                completeSelf = true;
            }

            if (completeSelf)
            {
                thisPtr.writeState.Complete(false, completionException);
            }
        }

        static void OnWriteCallback(IAsyncEventArgs state)
        {
            BufferedOutputAsyncStream thisPtr = (BufferedOutputAsyncStream)state.AsyncState;
            IAsyncResult returnResult = thisPtr.writeState.PendingAsyncResult;
            AsyncCallback callback = thisPtr.writeState.Arguments.Callback;
            thisPtr.writeState.Arguments.Callback = null;
            if (callback != null)
            {
                callback(returnResult);
            }
        }

        void ThrowOnException()
        {
            // if any of the buffers or the write state has an
            // exception the stream is not usable anymore.
            this.buffers.ThrowOnException();
            if (this.writeState != null)
            {
                this.writeState.ThrowOnException();
            }
        }

        class BufferQueue
        {
            readonly List<ByteBuffer> refBufferList;
            readonly int size;
            readonly Slot[] buffers;
            Exception completionException;
            int head;
            int count;
            bool waiting;
            bool pendingCompletion;

            internal BufferQueue(int queueSize)
            {
                this.head = 0;
                this.count = 0;
                this.size = queueSize;
                this.buffers = new Slot[size];
                this.refBufferList = new List<ByteBuffer>();
                for (int i = 0; i < queueSize; i++)
                {
                    Slot s = new Slot();
                    s.checkedOut = true; //Start with all buffers checkedout.
                    this.buffers[i] = s;
                }
            }

            object ThisLock
            {
                get
                {
                    return this.buffers;
                }
            }

            internal int Count
            {
                get
                {
                    lock (ThisLock)
                    {
                        return count;
                    }
                }
            }

            internal ByteBuffer Dequeue()
            {
                Fx.Assert(!this.pendingCompletion, "Dequeue cannot be invoked when there is a pending completion");

                lock (ThisLock)
                {
                    if (count == 0)
                    {
                        return null;
                    }

                    Slot s = buffers[head];
                    Fx.Assert(!s.checkedOut, "This buffer is already in use.");

                    this.head = (this.head + 1) % size;
                    this.count--;
                    ByteBuffer buffer = s.buffer;
                    s.buffer = null;
                    s.checkedOut = true;
                    return buffer;
                }
            }

            internal void Add(ByteBuffer buffer)
            {
                lock (ThisLock)
                {
                    Fx.Assert(this.refBufferList.Count < size, "Bufferlist is already full.");

                    if (this.refBufferList.Count < this.size)
                    {
                        this.refBufferList.Add(buffer);
                        this.Enqueue(buffer);
                    }
                }
            }

            internal void Enqueue(ByteBuffer buffer)
            {
                lock (ThisLock)
                {
                    this.completionException = this.completionException ?? buffer.CompletionException;
                    Fx.Assert(count < size, "The queue is already full.");
                    int tail = (this.head + this.count) % size;
                    Slot s = this.buffers[tail];
                    this.count++;
                    Fx.Assert(s.checkedOut, "Current buffer is still free.");
                    s.checkedOut = false;
                    s.buffer = buffer;

                    if (this.waiting)
                    {
                        Monitor.Pulse(this.ThisLock);
                    }
                }
            }

            internal ByteBuffer CurrentBuffer()
            {
                lock (ThisLock)
                {
                    ThrowOnException();
                    Slot s = this.buffers[head];
                    return s.buffer;
                }
            }

            internal void WaitForAllWritesToComplete()
            {
                for (int i = 0; i < this.refBufferList.Count; i++)
                {
                    this.refBufferList[i].WaitForWriteComplete();
                }
            }

            internal void WaitForAny()
            {
                lock (ThisLock)
                {
                    if (this.count == 0)
                    {
                        this.waiting = true;
                        Monitor.Wait(ThisLock);
                        this.waiting = false;
                    }
                }

                this.ThrowOnException();
            }

            internal void ThrowOnException()
            {
                if (this.completionException != null)
                {
                    throw FxTrace.Exception.AsError(this.completionException);
                }
            }

            internal bool TryUnlock()
            {
                // The main thread tries to indicate a pending completion 
                // if there aren't any free buffers for the next write.
                // The callback should try to complete() through TryAcquireLock.
                lock (ThisLock)
                {
                    Fx.Assert(!this.pendingCompletion, "There is already a completion pending.");

                    if (this.count == 0)
                    {
                        this.pendingCompletion = true;
                        return true;
                    }
                }

                return false;
            }

            internal bool TryAcquireLock()
            {
                // The callback tries to acquire the lock if there is a pending completion and a free buffer.
                // Buffers might get dequeued by the main writing thread as soon as they are enqueued.
                lock (ThisLock)
                {
                    if (this.pendingCompletion && this.count > 0)
                    {
                        this.pendingCompletion = false;
                        return true;
                    }
                }

                return false;
            }

            class Slot
            {
                internal bool checkedOut;
                internal ByteBuffer buffer;
            }
        }

        /// <summary>
        /// AsyncEventArgs used to invoke the FlushAsync() on the ByteBuffer.
        /// </summary>
        class WriteFlushAsyncEventArgs : AsyncEventArgs<object, ByteBuffer>
        {
        }

        class ByteBuffer
        {
            byte[] bytes;
            int position;
            Stream stream;
            bool writePending;
            bool waiting;
            Exception completionException;
            BufferedOutputAsyncStream parent;

            static AsyncCallback writeCallback = Fx.ThunkCallback(new AsyncCallback(WriteCallback));
            static AsyncCallback flushCallback;

            internal ByteBuffer(BufferedOutputAsyncStream parent, int bufferSize, Stream stream)
            {
                this.waiting = false;
                this.writePending = false;
                this.position = 0;
                this.bytes = DiagnosticUtility.Utility.AllocateByteArray(bufferSize);
                this.stream = stream;
                this.parent = parent;
            }

            object ThisLock
            {
                get { return this; }
            }

            internal Exception CompletionException
            {
                get { return this.completionException; }
            }

            internal int FreeBytes
            {
                get
                {
                    return this.bytes.Length - this.position;
                }
            }

            internal AsyncEventArgs<object, ByteBuffer> FlushAsyncArgs
            {
                get;
                set;
            }

            static void WriteCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                // Fetch our state information: ByteBuffer
                ByteBuffer buffer = (ByteBuffer)result.AsyncState;
                try
                {
                    if (TD.BufferedAsyncWriteStopIsEnabled())
                    {
                        TD.BufferedAsyncWriteStop(buffer.parent.EventTraceActivity);
                    }

                    buffer.stream.EndWrite(result);

                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    buffer.completionException = e;
                }

                // Tell the main thread we've finished.
                lock (buffer.ThisLock)
                {
                    buffer.writePending = false;

                    // Do not Pulse if no one is waiting, to avoid the overhead of Pulse
                    if (!buffer.waiting)
                        return;

                    Monitor.Pulse(buffer.ThisLock);
                }
            }

            internal void WaitForWriteComplete()
            {
                lock (ThisLock)
                {
                    if (this.writePending)
                    {
                        // Wait until the async write of this buffer is finished.
                        this.waiting = true;
                        Monitor.Wait(ThisLock);
                        this.waiting = false;
                    }
                }

                // Raise exception if necessary
                if (this.completionException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(completionException);
                }
            }

            internal void CopyData(byte[] buffer, int offset, int count)
            {
                Fx.Assert(this.position + count <= this.bytes.Length, string.Format(CultureInfo.InvariantCulture, "Chunk is too big to fit in this buffer. Chunk size={0}, free space={1}", count, this.bytes.Length - this.position));
                Fx.Assert(!this.writePending, string.Format(CultureInfo.InvariantCulture, "The buffer is in use, position={0}", this.position));

                Buffer.BlockCopy(buffer, offset, this.bytes, this.position, count);
                this.position += count;
            }

            internal void CopyData(byte value)
            {
                Fx.Assert(this.position < this.bytes.Length, "Buffer is full");
                Fx.Assert(!this.writePending, string.Format(CultureInfo.InvariantCulture, "The buffer is in use, position={0}", this.position));

                this.bytes[this.position++] = value;
            }

            /// <summary>
            /// Set the ByteBuffer's FlushAsyncArgs to invoke FlushAsync()
            /// </summary>
            /// <returns></returns>
            internal AsyncCompletionResult FlushAsync()
            {
                if (this.position <= 0)
                    return AsyncCompletionResult.Completed;

                Fx.Assert(this.FlushAsyncArgs != null, "FlushAsyncArgs not set.");

                if (flushCallback == null)
                {
                    flushCallback = new AsyncCallback(OnAsyncFlush);
                }

                int bytesToWrite = this.position;
                this.SetWritePending();
                this.position = 0;

                if (TD.BufferedAsyncWriteStartIsEnabled())
                {
                    TD.BufferedAsyncWriteStart(this.parent.EventTraceActivity, this.GetHashCode(), bytesToWrite);
                }

                IAsyncResult asyncResult = this.stream.BeginWrite(this.bytes, 0, bytesToWrite, flushCallback, this);
                if (asyncResult.CompletedSynchronously)
                {
                    if (TD.BufferedAsyncWriteStopIsEnabled())
                    {
                        TD.BufferedAsyncWriteStop(this.parent.EventTraceActivity);
                    }

                    this.stream.EndWrite(asyncResult);
                    this.ResetWritePending();
                    return AsyncCompletionResult.Completed;
                }

                return AsyncCompletionResult.Queued;
            }

            static void OnAsyncFlush(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ByteBuffer thisPtr = (ByteBuffer)result.AsyncState;
                AsyncEventArgs<object, ByteBuffer> asyncEventArgs = thisPtr.FlushAsyncArgs;

                try
                {
                    ByteBuffer.WriteCallback(result);
                    asyncEventArgs.Result = thisPtr;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    if (thisPtr.completionException == null)
                    {
                        thisPtr.completionException = exception;
                    }
                }

                asyncEventArgs.Complete(false, thisPtr.completionException);
            }

            void ResetWritePending()
            {
                lock (ThisLock)
                {
                    this.writePending = false;
                }
            }

            void SetWritePending()
            {
                lock (ThisLock)
                {
                    if (this.writePending)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.FlushBufferAlreadyInUse)));
                    }

                    this.writePending = true;
                }
            }
        }

        /// <summary>
        /// Used to hold the users callback and state and arguments when BeginWrite is invoked. 
        /// </summary>
        class WriteAsyncArgs
        {
            internal byte[] Buffer { get; set; }

            internal int Offset { get; set; }

            internal int Count { get; set; }

            internal AsyncCallback Callback { get; set; }

            internal object AsyncState { get; set; }

            internal void Set(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                this.Buffer = buffer;
                this.Offset = offset;
                this.Count = count;
                this.Callback = callback;
                this.AsyncState = state;
            }
        }

        class WriteAsyncState : AsyncEventArgs<WriteAsyncArgs, BufferedOutputAsyncStream>
        {
            PooledAsyncResult pooledAsyncResult;
            PooledAsyncResult completedSynchronouslyResult;

            internal IAsyncResult PendingAsyncResult
            {
                get
                {
                    if (this.pooledAsyncResult == null)
                    {
                        this.pooledAsyncResult = new PooledAsyncResult(this, false);
                    }

                    return this.pooledAsyncResult;
                }
            }

            internal IAsyncResult CompletedSynchronouslyAsyncResult
            {
                get
                {
                    if (this.completedSynchronouslyResult == null)
                    {
                        this.completedSynchronouslyResult = new PooledAsyncResult(this, true);
                    }

                    return completedSynchronouslyResult;
                }
            }

            internal void ThrowOnException()
            {
                if (this.Exception != null)
                {
                    throw FxTrace.Exception.AsError(this.Exception);
                }
            }

            class PooledAsyncResult : IAsyncResult
            {
                readonly WriteAsyncState writeState;
                readonly bool completedSynchronously;

                internal PooledAsyncResult(WriteAsyncState parentState, bool completedSynchronously)
                {
                    this.writeState = parentState;
                    this.completedSynchronously = completedSynchronously;
                }

                public object AsyncState
                {
                    get
                    {
                        return this.writeState.Arguments != null ? this.writeState.Arguments.AsyncState : null;
                    }
                }

                public WaitHandle AsyncWaitHandle
                {
                    get { throw FxTrace.Exception.AsError(new NotImplementedException()); }
                }

                public bool CompletedSynchronously
                {
                    get { return this.completedSynchronously; }
                }

                public bool IsCompleted
                {
                    get { throw FxTrace.Exception.AsError(new NotImplementedException()); }
                }
            }
        }
    }
}
