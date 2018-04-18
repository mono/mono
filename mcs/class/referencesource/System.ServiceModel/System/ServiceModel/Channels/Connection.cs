//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    // Low level abstraction for a socket/pipe
    interface IConnection
    {
        byte[] AsyncReadBuffer { get; }
        int AsyncReadBufferSize { get; }
        TraceEventType ExceptionEventType { get; set; }
        IPEndPoint RemoteIPEndPoint { get; }

        void Abort();
        void Close(TimeSpan timeout, bool asyncAndLinger);
        void Shutdown(TimeSpan timeout);

        AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout,
            WaitCallback callback, object state);
        void EndWrite();
        void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout);
        void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager);

        int Read(byte[] buffer, int offset, int size, TimeSpan timeout);
        AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state);
        int EndRead();

        // very ugly listener stuff
        object DuplicateAndClose(int targetProcessId);
        object GetCoreTransport();
        IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state);
        bool EndValidate(IAsyncResult result);
    }

    // Low level abstraction for connecting a socket/pipe
    interface IConnectionInitiator
    {
        IConnection Connect(Uri uri, TimeSpan timeout);
        IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state);
        IConnection EndConnect(IAsyncResult result);
    }

    // Low level abstraction for listening for sockets/pipes
    interface IConnectionListener : IDisposable
    {
        void Listen();
        IAsyncResult BeginAccept(AsyncCallback callback, object state);
        IConnection EndAccept(IAsyncResult result);
    }

    abstract class DelegatingConnection : IConnection
    {
        IConnection connection;

        protected DelegatingConnection(IConnection connection)
        {
            this.connection = connection;
        }

        public virtual byte[] AsyncReadBuffer
        {
            get { return connection.AsyncReadBuffer; }
        }

        public virtual int AsyncReadBufferSize
        {
            get { return connection.AsyncReadBufferSize; }
        }

        public TraceEventType ExceptionEventType
        {
            get { return connection.ExceptionEventType; }
            set { connection.ExceptionEventType = value; }
        }

        protected IConnection Connection
        {
            get { return connection; }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get { return connection.RemoteIPEndPoint; }
        }

        public virtual void Abort()
        {
            connection.Abort();
        }

        public virtual void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            connection.Close(timeout, asyncAndLinger);
        }

        public virtual void Shutdown(TimeSpan timeout)
        {
            connection.Shutdown(timeout);
        }

        public virtual object DuplicateAndClose(int targetProcessId)
        {
            return connection.DuplicateAndClose(targetProcessId);
        }

        public virtual object GetCoreTransport()
        {
            return connection.GetCoreTransport();
        }

        public virtual IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
        {
            return connection.BeginValidate(uri, callback, state);
        }

        public virtual bool EndValidate(IAsyncResult result)
        {
            return connection.EndValidate(result);
        }

        public virtual AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            return connection.BeginWrite(buffer, offset, size, immediate, timeout, callback, state);
        }

        public virtual void EndWrite()
        {
            connection.EndWrite();
        }

        public virtual void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            connection.Write(buffer, offset, size, immediate, timeout);
        }

        public virtual void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            connection.Write(buffer, offset, size, immediate, timeout, bufferManager);
        }

        public virtual int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            return connection.Read(buffer, offset, size, timeout);
        }

        public virtual AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            return connection.BeginRead(offset, size, timeout, callback, state);
        }

        public virtual int EndRead()
        {
            return connection.EndRead();
        }
    }

    class PreReadConnection : DelegatingConnection
    {
        int asyncBytesRead;
        byte[] preReadData;
        int preReadOffset;
        int preReadCount;

        public PreReadConnection(IConnection innerConnection, byte[] initialData)
            : this(innerConnection, initialData, 0, initialData.Length)
        {
        }

        public PreReadConnection(IConnection innerConnection, byte[] initialData, int initialOffset, int initialSize)
            : base(innerConnection)
        {
            this.preReadData = initialData;
            this.preReadOffset = initialOffset;
            this.preReadCount = initialSize;
        }

        public void AddPreReadData(byte[] initialData, int initialOffset, int initialSize)
        {
            if (this.preReadCount > 0)
            {
                byte[] tempBuffer = this.preReadData;
                this.preReadData = DiagnosticUtility.Utility.AllocateByteArray(initialSize + this.preReadCount);
                Buffer.BlockCopy(tempBuffer, this.preReadOffset, this.preReadData, 0, this.preReadCount);
                Buffer.BlockCopy(initialData, initialOffset, this.preReadData, this.preReadCount, initialSize);
                this.preReadOffset = 0;
                this.preReadCount += initialSize;
            }
            else
            {
                this.preReadData = initialData;
                this.preReadOffset = initialOffset;
                this.preReadCount = initialSize;
            }
        }

        public override int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);

            if (this.preReadCount > 0)
            {
                int bytesToCopy = Math.Min(size, this.preReadCount);
                Buffer.BlockCopy(this.preReadData, this.preReadOffset, buffer, offset, bytesToCopy);
                this.preReadOffset += bytesToCopy;
                this.preReadCount -= bytesToCopy;
                return bytesToCopy;
            }

            return base.Read(buffer, offset, size, timeout);
        }

        public override AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(AsyncReadBufferSize, offset, size);

            if (this.preReadCount > 0)
            {
                int bytesToCopy = Math.Min(size, this.preReadCount);
                Buffer.BlockCopy(this.preReadData, this.preReadOffset, AsyncReadBuffer, offset, bytesToCopy);
                this.preReadOffset += bytesToCopy;
                this.preReadCount -= bytesToCopy;
                this.asyncBytesRead = bytesToCopy;
                return AsyncCompletionResult.Completed;
            }

            return base.BeginRead(offset, size, timeout, callback, state);
        }

        public override int EndRead()
        {
            if (this.asyncBytesRead > 0)
            {
                int retValue = this.asyncBytesRead;
                this.asyncBytesRead = 0;
                return retValue;
            }

            return base.EndRead();
        }
    }

    class ConnectionStream : Stream
    {
        TimeSpan closeTimeout;
        int readTimeout;
        int writeTimeout;
        IConnection connection;
        bool immediate;

        public ConnectionStream(IConnection connection, IDefaultCommunicationTimeouts defaultTimeouts)
        {
            this.connection = connection;
            this.closeTimeout = defaultTimeouts.CloseTimeout;
            this.ReadTimeout = TimeoutHelper.ToMilliseconds(defaultTimeouts.ReceiveTimeout);
            this.WriteTimeout = TimeoutHelper.ToMilliseconds(defaultTimeouts.SendTimeout);
            immediate = true;
        }

        public IConnection Connection
        {
            get { return connection; }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanTimeout
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public TimeSpan CloseTimeout
        {
            get { return closeTimeout; }
            set { this.closeTimeout = value; }
        }

        public override int ReadTimeout
        {
            get { return this.readTimeout; }
            set
            {
                if (value < -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeInRange, -1, int.MaxValue)));
                }

                this.readTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get { return this.writeTimeout; }
            set
            {
                if (value < -1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeInRange, -1, int.MaxValue)));
                }

                this.writeTimeout = value;
            }
        }

        public bool Immediate
        {
            get { return immediate; }
            set { immediate = value; }
        }

        public override long Length
        {
            get
            {
#pragma warning suppress 56503 // Microsoft, required by the Stream.Length contract
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
            }
        }

        public override long Position
        {
            get
            {
#pragma warning suppress 56503 // Microsoft, required by the Stream.Position contract
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
            }
        }

        public TraceEventType ExceptionEventType
        {
            get { return connection.ExceptionEventType; }
            set { connection.ExceptionEventType = value; }
        }

        public void Abort()
        {
            connection.Abort();
        }

        public override void Close()
        {
            connection.Close(this.CloseTimeout, false);
        }

        public override void Flush()
        {
            // NOP
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new WriteAsyncResult(this.connection, buffer, offset, count, this.Immediate, TimeoutHelper.FromMilliseconds(this.WriteTimeout), callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            WriteAsyncResult.End(asyncResult);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            connection.Write(buffer, offset, count, this.Immediate, TimeoutHelper.FromMilliseconds(this.WriteTimeout));
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new ReadAsyncResult(connection, buffer, offset, count, TimeoutHelper.FromMilliseconds(this.ReadTimeout), callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ReadAsyncResult.End(asyncResult);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Read(buffer, offset, count, TimeoutHelper.FromMilliseconds(this.ReadTimeout));
        }

        protected int Read(byte[] buffer, int offset, int count, TimeSpan timeout)
        {
            return connection.Read(buffer, offset, count, timeout);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
        }


        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
        }

        public void Shutdown(TimeSpan timeout)
        {
            connection.Shutdown(timeout);
        }

        public IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
        {
            return this.connection.BeginValidate(uri, callback, state);
        }

        public bool EndValidate(IAsyncResult result)
        {
            return this.connection.EndValidate(result);
        }

        abstract class IOAsyncResult : AsyncResult
        {
            static WaitCallback onAsyncIOComplete;
            IConnection connection;

            protected IOAsyncResult(IConnection connection, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.connection = connection;
            }

            protected WaitCallback GetWaitCompletion()
            {
                if (onAsyncIOComplete == null)
                {
                    onAsyncIOComplete = new WaitCallback(OnAsyncIOComplete);
                }

                return onAsyncIOComplete;
            }

            protected abstract void HandleIO(IConnection connection);

            static void OnAsyncIOComplete(object state)
            {
                IOAsyncResult thisPtr = (IOAsyncResult)state;

                Exception completionException = null;
                try
                {
                    thisPtr.HandleIO(thisPtr.connection);
                }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                thisPtr.Complete(false, completionException);
            }
        }

        sealed class ReadAsyncResult : IOAsyncResult
        {
            int bytesRead;
            byte[] buffer;
            int offset;

            public ReadAsyncResult(IConnection connection, byte[] buffer, int offset, int count, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(connection, callback, state)
            {
                this.buffer = buffer;
                this.offset = offset;

                AsyncCompletionResult readResult = connection.BeginRead(0, Math.Min(count, connection.AsyncReadBufferSize),
                    timeout, GetWaitCompletion(), this);
                if (readResult == AsyncCompletionResult.Completed)
                {
                    HandleIO(connection);
                    base.Complete(true);
                }
            }

            protected override void HandleIO(IConnection connection)
            {
                bytesRead = connection.EndRead();
                Buffer.BlockCopy(connection.AsyncReadBuffer, 0, buffer, offset, bytesRead);
            }

            public static int End(IAsyncResult result)
            {
                ReadAsyncResult thisPtr = AsyncResult.End<ReadAsyncResult>(result);
                return thisPtr.bytesRead;
            }
        }

        sealed class WriteAsyncResult : IOAsyncResult
        {
            public WriteAsyncResult(IConnection connection, byte[] buffer, int offset, int count, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
                : base(connection, callback, state)
            {
                AsyncCompletionResult writeResult = connection.BeginWrite(buffer, offset, count, immediate, timeout, GetWaitCompletion(), this);
                if (writeResult == AsyncCompletionResult.Completed)
                {
                    HandleIO(connection);
                    base.Complete(true);
                }
            }

            protected override void HandleIO(IConnection connection)
            {
                connection.EndWrite();
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteAsyncResult>(result);
            }
        }
    }

    class StreamConnection : IConnection
    {
        byte[] asyncReadBuffer;
        int bytesRead;
        ConnectionStream innerStream;
        AsyncCallback onRead;
        AsyncCallback onWrite;
        IAsyncResult readResult;
        IAsyncResult writeResult;
        WaitCallback readCallback;
        WaitCallback writeCallback;
        Stream stream;

        public StreamConnection(Stream stream, ConnectionStream innerStream)
        {
            Fx.Assert(stream != null, "StreamConnection: Stream cannot be null.");
            Fx.Assert(innerStream != null, "StreamConnection: Inner stream cannot be null.");

            this.stream = stream;
            this.innerStream = innerStream;

            onRead = Fx.ThunkCallback(new AsyncCallback(OnRead));
            onWrite = Fx.ThunkCallback(new AsyncCallback(OnWrite));
        }

        public byte[] AsyncReadBuffer
        {
            get
            {
                if (this.asyncReadBuffer == null)
                {
                    lock (ThisLock)
                    {
                        if (this.asyncReadBuffer == null)
                        {
                            this.asyncReadBuffer = DiagnosticUtility.Utility.AllocateByteArray(innerStream.Connection.AsyncReadBufferSize);
                        }
                    }
                }

                return this.asyncReadBuffer;
            }
        }

        public int AsyncReadBufferSize
        {
            get { return innerStream.Connection.AsyncReadBufferSize; }
        }

        public Stream Stream
        {
            get { return this.stream; }
        }

        public object ThisLock
        {
            get { return this; }
        }

        public TraceEventType ExceptionEventType
        {
            get { return innerStream.ExceptionEventType; }
            set { innerStream.ExceptionEventType = value; }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get
            {
#pragma warning suppress 56503 // Not publicly accessible and this should never be called.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
            }
        }

        public void Abort()
        {
            innerStream.Abort();
        }

        Exception ConvertIOException(IOException ioException)
        {
            if (ioException.InnerException is TimeoutException)
            {
                return new TimeoutException(ioException.InnerException.Message, ioException);
            }
            else if (ioException.InnerException is CommunicationObjectAbortedException)
            {
                return new CommunicationObjectAbortedException(ioException.InnerException.Message, ioException);
            }
            else if (ioException.InnerException is CommunicationException)
            {
                return new CommunicationException(ioException.InnerException.Message, ioException);
            }
            else
            {
                return new CommunicationException(SR.GetString(SR.StreamError), ioException);
            }
        }

        public void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            innerStream.CloseTimeout = timeout;
            try
            {
                stream.Close();
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
            }
        }

        public void Shutdown(TimeSpan timeout)
        {
            innerStream.Shutdown(timeout);
        }

        public object DuplicateAndClose(int targetProcessId)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public virtual object GetCoreTransport()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
        {
            return this.innerStream.BeginValidate(uri, callback, state);
        }

        public bool EndValidate(IAsyncResult result)
        {
            return this.innerStream.EndValidate(result);
        }

        public AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            if (callback == null)
            {
                Fx.AssertAndThrow("Cannot call BeginWrite without a callback");
            }

            if (this.writeCallback != null)
            {
                Fx.AssertAndThrow("BeginWrite cannot be called twice");
            }

            this.writeCallback = callback;
            bool throwing = true;

            try
            {
                innerStream.Immediate = immediate;
                SetWriteTimeout(timeout);
                IAsyncResult localResult = stream.BeginWrite(buffer, offset, size, this.onWrite, state);

                if (!localResult.CompletedSynchronously)
                {
                    throwing = false;
                    return AsyncCompletionResult.Queued;
                }

                throwing = false;
                stream.EndWrite(localResult);
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
            }
            finally
            {
                if (throwing)
                {
                    this.writeCallback = null;
                }
            }

            return AsyncCompletionResult.Completed;
        }

        public void EndWrite()
        {
            IAsyncResult localResult = this.writeResult;
            this.writeResult = null;
            this.writeCallback = null;

            if (localResult != null)
            {
                try
                {
                    stream.EndWrite(localResult);
                }
                catch (IOException ioException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
                }
            }
        }

        void OnWrite(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            if (this.writeResult != null)
            {
                throw Fx.AssertAndThrow("StreamConnection: OnWrite called twice.");
            }

            this.writeResult = result;
            this.writeCallback(result.AsyncState);
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            try
            {
                innerStream.Immediate = immediate;
                SetWriteTimeout(timeout);
                stream.Write(buffer, offset, size);
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
            }
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            Write(buffer, offset, size, immediate, timeout);
            bufferManager.ReturnBuffer(buffer);
        }

        void SetReadTimeout(TimeSpan timeout)
        {
            int timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeout);
            if (stream.CanTimeout)
            {
                stream.ReadTimeout = timeoutInMilliseconds;
            }
            innerStream.ReadTimeout = timeoutInMilliseconds;
        }

        void SetWriteTimeout(TimeSpan timeout)
        {
            int timeoutInMilliseconds = TimeoutHelper.ToMilliseconds(timeout);
            if (stream.CanTimeout)
            {
                stream.WriteTimeout = timeoutInMilliseconds;
            }
            innerStream.WriteTimeout = timeoutInMilliseconds;
        }

        public int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            try
            {
                SetReadTimeout(timeout);
                return stream.Read(buffer, offset, size);
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
            }
        }

        public AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(AsyncReadBufferSize, offset, size);
            readCallback = callback;

            try
            {
                SetReadTimeout(timeout);
                IAsyncResult localResult = stream.BeginRead(AsyncReadBuffer, offset, size, onRead, state);

                if (!localResult.CompletedSynchronously)
                {
                    return AsyncCompletionResult.Queued;
                }

                bytesRead = stream.EndRead(localResult);
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
            }

            return AsyncCompletionResult.Completed;
        }

        public int EndRead()
        {
            IAsyncResult localResult = this.readResult;
            this.readResult = null;

            if (localResult != null)
            {
                try
                {
                    bytesRead = stream.EndRead(localResult);
                }
                catch (IOException ioException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertIOException(ioException));
                }
            }

            return bytesRead;
        }

        void OnRead(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            if (this.readResult != null)
            {
                throw Fx.AssertAndThrow("StreamConnection: OnRead called twice.");
            }

            this.readResult = result;
            readCallback(result.AsyncState);
        }
   }

    class ConnectionMessageProperty
    {
        IConnection connection;

        public ConnectionMessageProperty(IConnection connection)
        {
            this.connection = connection;
        }

        public static string Name
        {
            get { return "iconnection"; }
        }

        public IConnection Connection
        {
            get { return this.connection; }
        }
    }

    static class ConnectionUtilities
    {
        internal static void CloseNoThrow(IConnection connection, TimeSpan timeout)
        {
            bool success = false;
            try
            {
                connection.Close(timeout, false);
                success = true;
            }
            catch (TimeoutException e)
            {
                if (TD.CloseTimeoutIsEnabled())
                {
                    TD.CloseTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            catch (CommunicationException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            finally
            {
                if (!success)
                {
                    connection.Abort();
                }
            }
        }

        internal static void ValidateBufferBounds(ArraySegment<byte> buffer)
        {
            ValidateBufferBounds(buffer.Array, buffer.Offset, buffer.Count);
        }

        internal static void ValidateBufferBounds(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            ValidateBufferBounds(buffer.Length, offset, size);
        }

        internal static void ValidateBufferBounds(int bufferSize, int offset, int size)
        {
            if (offset < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset, SR.GetString(
                    SR.ValueMustBeNonNegative)));
            }

            if (offset > bufferSize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset, SR.GetString(
                    SR.OffsetExceedsBufferSize, bufferSize)));
            }

            if (size <= 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, SR.GetString(
                    SR.ValueMustBePositive)));
            }

            int remainingBufferSpace = bufferSize - offset;
            if (size > remainingBufferSpace)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("size", size, SR.GetString(
                    SR.SizeExceedsRemainingBufferSpace, remainingBufferSpace)));
            }
        }
    }
}
