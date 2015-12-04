//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security.AccessControl;
    using System.ComponentModel;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Text;
    using System.Threading;
    using SafeCloseHandle = System.ServiceModel.Activation.SafeCloseHandle;

    sealed class PipeConnection : IConnection
    {
        // common state
        PipeHandle pipe;
        CloseState closeState;
        bool aborted;
        bool isBoundToCompletionPort;
        bool autoBindToCompletionPort;
        TraceEventType exceptionEventType;
        static byte[] zeroBuffer;

        // read state
        object readLock = new object();
        bool inReadingState;     // This keeps track of the state machine (IConnection interface).
        bool isReadOutstanding;  // This tracks whether an actual I/O is pending.
        OverlappedContext readOverlapped;
        byte[] asyncReadBuffer;
        int readBufferSize;
        ManualResetEvent atEOFEvent;
        bool isAtEOF;
        OverlappedIOCompleteCallback onAsyncReadComplete;
        Exception asyncReadException;
        WaitCallback asyncReadCallback;
        object asyncReadCallbackState;
        int asyncBytesRead;

        // write state
        object writeLock = new object();
        bool inWritingState;      // This keeps track of the state machine (IConnection interface).
        bool isWriteOutstanding;  // This tracks whether an actual I/O is pending.
        OverlappedContext writeOverlapped;
        Exception asyncWriteException;
        WaitCallback asyncWriteCallback;
        object asyncWriteCallbackState;
        int asyncBytesToWrite;
        bool isShutdownWritten;
        int syncWriteSize;
        byte[] pendingWriteBuffer;
        BufferManager pendingWriteBufferManager;
        OverlappedIOCompleteCallback onAsyncWriteComplete;
        int writeBufferSize;

        // timeout support
        TimeSpan readTimeout;
        IOThreadTimer readTimer;
        static Action<object> onReadTimeout;
        string timeoutErrorString;
        TransferOperation timeoutErrorTransferOperation;
        TimeSpan writeTimeout;
        IOThreadTimer writeTimer;
        static Action<object> onWriteTimeout;

        public PipeConnection(PipeHandle pipe, int connectionBufferSize, bool isBoundToCompletionPort, bool autoBindToCompletionPort)
        {
            if (pipe == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pipe");
            if (pipe.IsInvalid)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pipe");

            this.closeState = CloseState.Open;
            this.exceptionEventType = TraceEventType.Error;
            this.isBoundToCompletionPort = isBoundToCompletionPort;
            this.autoBindToCompletionPort = autoBindToCompletionPort;
            this.pipe = pipe;
            this.readBufferSize = connectionBufferSize;
            this.writeBufferSize = connectionBufferSize;
            this.readOverlapped = new OverlappedContext();
            this.asyncReadBuffer = DiagnosticUtility.Utility.AllocateByteArray(connectionBufferSize);
            this.writeOverlapped = new OverlappedContext();
            this.atEOFEvent = new ManualResetEvent(false);
            this.onAsyncReadComplete = new OverlappedIOCompleteCallback(OnAsyncReadComplete);
            this.onAsyncWriteComplete = new OverlappedIOCompleteCallback(OnAsyncWriteComplete);
        }

        public int AsyncReadBufferSize
        {
            get
            {
                return this.readBufferSize;
            }
        }

        public byte[] AsyncReadBuffer
        {
            get
            {
                return this.asyncReadBuffer;
            }
        }

        static byte[] ZeroBuffer
        {
            get
            {
                if (PipeConnection.zeroBuffer == null)
                {
                    PipeConnection.zeroBuffer = new byte[1];
                }
                return PipeConnection.zeroBuffer;
            }
        }

        public TraceEventType ExceptionEventType
        {
            get { return this.exceptionEventType; }
            set { this.exceptionEventType = value; }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get { return null; }
        }

        IOThreadTimer ReadTimer
        {
            get
            {
                if (this.readTimer == null)
                {
                    if (onReadTimeout == null)
                    {
                        onReadTimeout = new Action<object>(OnReadTimeout);
                    }

                    this.readTimer = new IOThreadTimer(onReadTimeout, this, false);
                }

                return this.readTimer;
            }
        }
        IOThreadTimer WriteTimer
        {
            get
            {
                if (this.writeTimer == null)
                {
                    if (onWriteTimeout == null)
                    {
                        onWriteTimeout = new Action<object>(OnWriteTimeout);
                    }

                    this.writeTimer = new IOThreadTimer(onWriteTimeout, this, false);
                }

                return this.writeTimer;
            }
        }

        static void OnReadTimeout(object state)
        {
            PipeConnection thisPtr = (PipeConnection)state;
            thisPtr.Abort(SR.GetString(SR.PipeConnectionAbortedReadTimedOut, thisPtr.readTimeout), TransferOperation.Read);
        }

        static void OnWriteTimeout(object state)
        {
            PipeConnection thisPtr = (PipeConnection)state;
            thisPtr.Abort(SR.GetString(SR.PipeConnectionAbortedWriteTimedOut, thisPtr.writeTimeout), TransferOperation.Write);
        }

        public void Abort()
        {
            Abort(null, TransferOperation.Undefined);
        }

        void Abort(string timeoutErrorString, TransferOperation transferOperation)
        {
            CloseHandle(true, timeoutErrorString, transferOperation);
        }

        Exception ConvertPipeException(PipeException pipeException, TransferOperation transferOperation)
        {
            return ConvertPipeException(pipeException.Message, pipeException, transferOperation);
        }

        Exception ConvertPipeException(string exceptionMessage, PipeException pipeException, TransferOperation transferOperation)
        {
            if (this.timeoutErrorString != null)
            {
                if (transferOperation == this.timeoutErrorTransferOperation)
                {
                    return new TimeoutException(this.timeoutErrorString, pipeException);
                }
                else
                {
                    return new CommunicationException(this.timeoutErrorString, pipeException);
                }
            }
            else if (this.aborted)
            {
                return new CommunicationObjectAbortedException(exceptionMessage, pipeException);
            }
            else
            {
                return new CommunicationException(exceptionMessage, pipeException);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public unsafe AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(AsyncReadBuffer, offset, size);

            lock (readLock)
            {
                try
                {
                    ValidateEnterReadingState(true);

                    if (isAtEOF)
                    {
                        asyncBytesRead = 0;
                        asyncReadException = null;
                        return AsyncCompletionResult.Completed;
                    }

                    if (autoBindToCompletionPort)
                    {
                        if (!isBoundToCompletionPort)
                        {
                            lock (writeLock)
                            {
                                // readLock, writeLock acquired in order to prevent deadlock
                                EnsureBoundToCompletionPort();
                            }
                        }
                    }

                    if (this.isReadOutstanding)
                    {
                        throw Fx.AssertAndThrow("Read I/O already pending when BeginRead called.");
                    }
                    try
                    {
                        this.readTimeout = timeout;

                        if (this.readTimeout != TimeSpan.MaxValue)
                        {
                            this.ReadTimer.Set(this.readTimeout);
                        }

                        this.asyncReadCallback = callback;
                        this.asyncReadCallbackState = state;

                        this.isReadOutstanding = true;
                        this.readOverlapped.StartAsyncOperation(AsyncReadBuffer, this.onAsyncReadComplete, this.isBoundToCompletionPort);
                        if (UnsafeNativeMethods.ReadFile(this.pipe.DangerousGetHandle(), this.readOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.readOverlapped.NativeOverlapped) == 0)
                        {
                            int error = Marshal.GetLastWin32Error();
                            if (error != UnsafeNativeMethods.ERROR_IO_PENDING && error != UnsafeNativeMethods.ERROR_MORE_DATA)
                            {
                                this.isReadOutstanding = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateReadException(error));
                            }
                        }
                    }
                    finally
                    {
                        if (!this.isReadOutstanding)
                        {
                            // Unbind the buffer.
                            this.readOverlapped.CancelAsyncOperation();

                            this.asyncReadCallback = null;
                            this.asyncReadCallbackState = null;
                            this.ReadTimer.Cancel();
                        }
                    }

                    if (!this.isReadOutstanding)
                    {
                        int bytesRead;
                        Exception readException = Exceptions.GetOverlappedReadException(this.pipe, this.readOverlapped.NativeOverlapped, out bytesRead);
                        if (readException != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(readException);
                        }
                        asyncBytesRead = bytesRead;
                        HandleReadComplete(asyncBytesRead);
                    }
                    else
                    {
                        EnterReadingState();
                    }

                    return this.isReadOutstanding ? AsyncCompletionResult.Queued : AsyncCompletionResult.Completed;
                }
                catch (PipeException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Read), ExceptionEventType);
                }
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public unsafe AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            FinishPendingWrite(timeout);

            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);

            if (autoBindToCompletionPort && !isBoundToCompletionPort)
            {
                // Locks must be both taken, and in this order.
                lock (readLock)
                {
                    lock (writeLock)
                    {
                        ValidateEnterWritingState(true);

                        EnsureBoundToCompletionPort();
                    }
                }
            }

            lock (writeLock)
            {
                try
                {
                    ValidateEnterWritingState(true);

                    if (this.isWriteOutstanding)
                    {
                        throw Fx.AssertAndThrow("Write I/O already pending when BeginWrite called.");
                    }

                    try
                    {
                        this.writeTimeout = timeout;
                        this.WriteTimer.Set(timeoutHelper.RemainingTime());

                        this.asyncBytesToWrite = size;
                        this.asyncWriteException = null;
                        this.asyncWriteCallback = callback;
                        this.asyncWriteCallbackState = state;

                        this.isWriteOutstanding = true;
                        this.writeOverlapped.StartAsyncOperation(buffer, this.onAsyncWriteComplete, this.isBoundToCompletionPort);
                        if (UnsafeNativeMethods.WriteFile(this.pipe.DangerousGetHandle(), this.writeOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.writeOverlapped.NativeOverlapped) == 0)
                        {
                            int error = Marshal.GetLastWin32Error();
                            if (error != UnsafeNativeMethods.ERROR_IO_PENDING)
                            {
                                this.isWriteOutstanding = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateWriteException(error));
                            }
                        }
                    }
                    finally
                    {
                        if (!this.isWriteOutstanding)
                        {
                            // Unbind the buffer.
                            this.writeOverlapped.CancelAsyncOperation();

                            this.ResetWriteState();
                            this.WriteTimer.Cancel();
                        }
                    }

                    if (!this.isWriteOutstanding)
                    {
                        int bytesWritten;
                        Exception writeException = Exceptions.GetOverlappedWriteException(this.pipe, this.writeOverlapped.NativeOverlapped, out bytesWritten);
                        if (writeException == null && bytesWritten != size)
                        {
                            writeException = new PipeException(SR.GetString(SR.PipeWriteIncomplete));
                        }
                        if (writeException != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(writeException);
                        }
                    }
                    else
                    {
                        EnterWritingState();
                    }

                    return this.isWriteOutstanding ? AsyncCompletionResult.Queued : AsyncCompletionResult.Completed;
                }
                catch (PipeException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Write), ExceptionEventType);
                }
            }
        }

        // CSDMain 112188: Note asyncAndLinger has no effect here. Async pooling for Tcp was
        // added and NamedPipes currently doesn't obey the async model.
        public void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            FinishPendingWrite(timeout);

            bool shouldCloseHandle = false;
            try
            {
                bool existingReadIsPending = false;
                bool shouldReadEOF = false;
                bool shouldWriteEOF = false;

                lock (readLock)
                {
                    lock (writeLock)
                    {
                        if (!isShutdownWritten && inWritingState)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                                new PipeException(SR.GetString(SR.PipeCantCloseWithPendingWrite)), ExceptionEventType);
                        }

                        if (closeState == CloseState.Closing || closeState == CloseState.HandleClosed)
                        {
                            // already closing or closed, so just return
                            return;
                        }

                        closeState = CloseState.Closing;

                        shouldCloseHandle = true;

                        if (!isAtEOF)
                        {
                            if (inReadingState)
                            {
                                existingReadIsPending = true;
                            }
                            else
                            {
                                shouldReadEOF = true;
                            }
                        }

                        if (!isShutdownWritten)
                        {
                            shouldWriteEOF = true;
                            isShutdownWritten = true;
                        }
                    }
                }

                if (shouldWriteEOF)
                {
                    StartWriteZero(timeoutHelper.RemainingTime());
                }

                if (shouldReadEOF)
                {
                    StartReadZero();
                }

                // wait for shutdown write to complete
                try
                {
                    WaitForWriteZero(timeoutHelper.RemainingTime(), true);
                }
                catch (TimeoutException e)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        new TimeoutException(SR.GetString(SR.PipeShutdownWriteError), e), ExceptionEventType);
                }

                // ensure we have received EOF signal
                if (shouldReadEOF)
                {
                    try
                    {
                        WaitForReadZero(timeoutHelper.RemainingTime(), true);
                    }
                    catch (TimeoutException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                            new TimeoutException(SR.GetString(SR.PipeShutdownReadError), e), ExceptionEventType);
                    }
                }
                else if (existingReadIsPending)
                {
                    if (!TimeoutHelper.WaitOne(atEOFEvent, timeoutHelper.RemainingTime()))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                            new TimeoutException(SR.GetString(SR.PipeShutdownReadError)), ExceptionEventType);
                    }
                }
                // else we had already seen EOF.

                // at this point, we may get exceptions if the other side closes the handle first
                try
                {
                    // write an ack for eof
                    StartWriteZero(timeoutHelper.RemainingTime());

                    // read an ack for eof
                    StartReadZero();

                    // wait for write to complete/fail
                    WaitForWriteZero(timeoutHelper.RemainingTime(), false);

                    // wait for read to complete/fail
                    WaitForReadZero(timeoutHelper.RemainingTime(), false);
                }
                catch (PipeException e)
                {
                    if (!IsBrokenPipeError(e.ErrorCode))
                    {
                        throw;
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (TimeoutException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
            }
            catch (TimeoutException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    new TimeoutException(SR.GetString(SR.PipeCloseFailed), e), ExceptionEventType);
            }
            catch (PipeException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertPipeException(SR.GetString(SR.PipeCloseFailed), e, TransferOperation.Undefined), ExceptionEventType);
            }
            finally
            {
                if (shouldCloseHandle)
                {
                    CloseHandle(false, null, TransferOperation.Undefined);
                }
            }
        }

        void CloseHandle(bool abort, string timeoutErrorString, TransferOperation transferOperation)
        {
            lock (readLock)
            {
                lock (writeLock)
                {
                    if (this.closeState == CloseState.HandleClosed)
                    {
                        return;
                    }

                    this.timeoutErrorString = timeoutErrorString;
                    this.timeoutErrorTransferOperation = transferOperation;
                    this.aborted = abort;
                    this.closeState = CloseState.HandleClosed;
                    this.pipe.Close();
                    this.readOverlapped.FreeOrDefer();
                    this.writeOverlapped.FreeOrDefer();

                    if (this.atEOFEvent != null)
                    {
                        this.atEOFEvent.Close();
                    }

                    // This should only do anything in the abort case.
                    try
                    {
                        FinishPendingWrite(TimeSpan.Zero);
                    }
                    catch (TimeoutException exception)
                    {
                        if (TD.CloseTimeoutIsEnabled())
                        {
                            TD.CloseTimeout(exception.Message);
                        }
                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                    catch (CommunicationException exception)
                    {
                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
            }

            if (abort)
            {
                TraceEventType traceEventType = TraceEventType.Warning;

                // we could be timing out a cached connection
                if (this.ExceptionEventType == TraceEventType.Information)
                {
                    traceEventType = this.ExceptionEventType;
                }

                if (DiagnosticUtility.ShouldTrace(traceEventType))
                {
                    TraceUtility.TraceEvent(traceEventType, TraceCode.PipeConnectionAbort, SR.GetString(SR.TraceCodePipeConnectionAbort), this);
                }
            }
        }

        CommunicationException CreatePipeDuplicationFailedException(int win32Error)
        {
            Exception innerException = new PipeException(SR.GetString(SR.PipeDuplicationFailed), win32Error);
            return new CommunicationException(innerException.Message, innerException);
        }

        public object DuplicateAndClose(int targetProcessId)
        {
            SafeCloseHandle targetProcessHandle = ListenerUnsafeNativeMethods.OpenProcess(ListenerUnsafeNativeMethods.PROCESS_DUP_HANDLE, false, targetProcessId);
            if (targetProcessHandle.IsInvalid)
            {
                targetProcessHandle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), ExceptionEventType);
            }
            try
            {
                // no need to close this handle, it's a pseudo handle. expected value is -1.
                IntPtr sourceProcessHandle = ListenerUnsafeNativeMethods.GetCurrentProcess();
                if (sourceProcessHandle == IntPtr.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), ExceptionEventType);
                }
                IntPtr duplicatedHandle;
                bool success = UnsafeNativeMethods.DuplicateHandle(sourceProcessHandle, this.pipe, targetProcessHandle, out duplicatedHandle, 0, false, UnsafeNativeMethods.DUPLICATE_SAME_ACCESS);
                if (!success)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), ExceptionEventType);
                }
                this.Abort();
                return duplicatedHandle;
            }
            finally
            {
                targetProcessHandle.Close();
            }
        }

        public object GetCoreTransport()
        {
            return pipe;
        }

        void EnsureBoundToCompletionPort()
        {
            // Both read and write locks must be acquired before doing this
            if (!isBoundToCompletionPort)
            {
                ThreadPool.BindHandle(this.pipe);
                isBoundToCompletionPort = true;
            }
        }

        public int EndRead()
        {
            if (asyncReadException != null)
            {
                Exception exceptionToThrow = asyncReadException;
                asyncReadException = null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
            }
            return asyncBytesRead;
        }

        public void EndWrite()
        {
            if (this.asyncWriteException != null)
            {
                Exception exceptionToThrow = this.asyncWriteException;
                this.asyncWriteException = null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
            }
        }

        void EnterReadingState()
        {
            inReadingState = true;
        }

        void EnterWritingState()
        {
            inWritingState = true;
        }

        void ExitReadingState()
        {
            inReadingState = false;
        }

        void ExitWritingState()
        {
            inWritingState = false;
        }

        void ReadIOCompleted()
        {
            this.readOverlapped.FreeIfDeferred();
        }

        void WriteIOCompleted()
        {
            this.writeOverlapped.FreeIfDeferred();
        }

        void FinishPendingWrite(TimeSpan timeout)
        {
            if (this.pendingWriteBuffer == null)
            {
                return;
            }

            byte[] buffer;
            BufferManager bufferManager;
            lock (this.writeLock)
            {
                if (this.pendingWriteBuffer == null)
                {
                    return;
                }

                buffer = this.pendingWriteBuffer;
                this.pendingWriteBuffer = null;

                bufferManager = this.pendingWriteBufferManager;
                this.pendingWriteBufferManager = null;
            }

            try
            {
                bool success = false;
                try
                {
                    WaitForSyncWrite(timeout, true);
                    success = true;
                }
                finally
                {
                    lock (this.writeLock)
                    {
                        try
                        {
                            if (success)
                            {
                                FinishSyncWrite(true);
                            }
                        }
                        finally
                        {
                            ExitWritingState();
                            if (!this.isWriteOutstanding)
                            {
                                bufferManager.ReturnBuffer(buffer);
                                WriteIOCompleted();
                            }
                        }
                    }
                }
            }
            catch (PipeException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Write), ExceptionEventType);
            }
        }

#if FUTURE
        ulong GetServerPid()
        {
            ulong id;
#pragma warning suppress 56523 // [....], Win32Exception ctor calls Marshal.GetLastWin32Error()
            if (!UnsafeNativeMethods.GetNamedPipeServerProcessId(pipe, out id))
            {
                Win32Exception e = new Win32Exception();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(e.Message, e));
            }
            return id;
        }

        ulong GetClientPid()
        {
            ulong id;
#pragma warning suppress 56523 // [....], Win32Exception ctor calls Marshal.GetLastWin32Error()
            if (!UnsafeNativeMethods.GetNamedPipeServerProcessId(pipe, out id))
            {
                Win32Exception e = new Win32Exception();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(e.Message, e));
            }
            return id;
        }
#endif

        void HandleReadComplete(int bytesRead)
        {
            if (bytesRead == 0)
            {
                isAtEOF = true;
                atEOFEvent.Set();
            }
        }

        bool IsBrokenPipeError(int error)
        {
            return error == UnsafeNativeMethods.ERROR_NO_DATA ||
                error == UnsafeNativeMethods.ERROR_BROKEN_PIPE;
        }

        Exception CreatePipeClosedException(TransferOperation transferOperation)
        {
            return ConvertPipeException(new PipeException(SR.GetString(SR.PipeClosed)), transferOperation);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void OnAsyncReadComplete(bool haveResult, int error, int numBytes)
        {
            WaitCallback callback;
            object state;

            lock (readLock)
            {
                try
                {
                    try
                    {
                        if (this.readTimeout != TimeSpan.MaxValue && !this.ReadTimer.Cancel())
                        {
                            this.Abort(SR.GetString(SR.PipeConnectionAbortedReadTimedOut, this.readTimeout), TransferOperation.Read);
                        }

                        if (this.closeState == CloseState.HandleClosed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeClosedException(TransferOperation.Read));
                        }
                        if (!haveResult)
                        {
                            if (UnsafeNativeMethods.GetOverlappedResult(this.pipe.DangerousGetHandle(), this.readOverlapped.NativeOverlapped, out numBytes, 0) == 0)
                            {
                                error = Marshal.GetLastWin32Error();
                            }
                            else
                            {
                                error = 0;
                            }
                        }

                        if (error != 0 && error != UnsafeNativeMethods.ERROR_MORE_DATA)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateReadException((int)error));
                        }
                        this.asyncBytesRead = numBytes;
                        HandleReadComplete(this.asyncBytesRead);
                    }
                    catch (PipeException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ConvertPipeException(e, TransferOperation.Read));
                    }
                }
#pragma warning suppress 56500 // [....], transferring exception to caller
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.asyncReadException = e;
                }
                finally
                {
                    this.isReadOutstanding = false;
                    ReadIOCompleted();
                    ExitReadingState();
                    callback = this.asyncReadCallback;
                    this.asyncReadCallback = null;
                    state = this.asyncReadCallbackState;
                    this.asyncReadCallbackState = null;
                }
            }

            callback(state);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void OnAsyncWriteComplete(bool haveResult, int error, int numBytes)
        {
            WaitCallback callback;
            object state;

            Exception writeException = null;

            this.WriteTimer.Cancel();
            lock (writeLock)
            {
                try
                {
                    try
                    {
                        if (this.closeState == CloseState.HandleClosed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeClosedException(TransferOperation.Write));
                        }
                        if (!haveResult)
                        {
                            if (UnsafeNativeMethods.GetOverlappedResult(this.pipe.DangerousGetHandle(), this.writeOverlapped.NativeOverlapped, out numBytes, 0) == 0)
                            {
                                error = Marshal.GetLastWin32Error();
                            }
                            else
                            {
                                error = 0;
                            }
                        }

                        if (error != 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateWriteException(error));
                        }
                        else if (numBytes != this.asyncBytesToWrite)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PipeException(SR.GetString(SR.PipeWriteIncomplete)));
                        }
                    }
                    catch (PipeException e)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Write), ExceptionEventType);
                    }
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    writeException = e;
                }
                finally
                {
                    this.isWriteOutstanding = false;
                    WriteIOCompleted();
                    ExitWritingState();
                    this.asyncWriteException = writeException;
                    callback = this.asyncWriteCallback;
                    state = this.asyncWriteCallbackState;
                    this.ResetWriteState();
                }
            }

            if (callback != null)
            {
                callback(state);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe public int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);

            try
            {
                lock (readLock)
                {
                    ValidateEnterReadingState(true);
                    if (isAtEOF)
                    {
                        return 0;
                    }

                    StartSyncRead(buffer, offset, size);
                    EnterReadingState();
                }

                int bytesRead = -1;
                bool success = false;
                try
                {
                    WaitForSyncRead(timeout, true);
                    success = true;
                }
                finally
                {
                    lock (this.readLock)
                    {
                        try
                        {
                            if (success)
                            {
                                bytesRead = FinishSyncRead(true);
                                HandleReadComplete(bytesRead);
                            }
                        }
                        finally
                        {
                            ExitReadingState();
                            if (!this.isReadOutstanding)
                            {
                                ReadIOCompleted();
                            }
                        }
                    }
                }

                Fx.Assert(bytesRead >= 0, "Logic error in Read - bytesRead not set.");
                return bytesRead;
            }
            catch (PipeException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Read), ExceptionEventType);
            }
        }

        public void Shutdown(TimeSpan timeout)
        {
            try
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                FinishPendingWrite(timeoutHelper.RemainingTime());

                lock (writeLock)
                {
                    ValidateEnterWritingState(true);
                    StartWriteZero(timeoutHelper.RemainingTime());
                    isShutdownWritten = true;
                }
            }
            catch (PipeException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Undefined), ExceptionEventType);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void StartReadZero()
        {
            lock (this.readLock)
            {
                ValidateEnterReadingState(false);
                StartSyncRead(ZeroBuffer, 0, 1);
                EnterReadingState();
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void StartWriteZero(TimeSpan timeout)
        {
            FinishPendingWrite(timeout);

            lock (this.writeLock)
            {
                ValidateEnterWritingState(false);
                StartSyncWrite(ZeroBuffer, 0, 0);
                EnterWritingState();
            }
        }

        void ResetWriteState()
        {
            this.asyncBytesToWrite = -1;
            this.asyncWriteCallback = null;
            this.asyncWriteCallbackState = null;
        }

        public IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        public bool EndValidate(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        void WaitForReadZero(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            bool success = false;
            try
            {
                WaitForSyncRead(timeout, traceExceptionsAsErrors);
                success = true;
            }
            finally
            {
                lock (this.readLock)
                {
                    try
                    {
                        if (success)
                        {
                            if (FinishSyncRead(traceExceptionsAsErrors) != 0)
                            {
                                Exception exception = ConvertPipeException(new PipeException(SR.GetString(SR.PipeSignalExpected)), TransferOperation.Read);
                                TraceEventType traceEventType = TraceEventType.Information;
                                if (traceExceptionsAsErrors)
                                {
                                    traceEventType = TraceEventType.Error;
                                }
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, traceEventType);
                            }
                        }
                    }
                    finally
                    {
                        ExitReadingState();
                        if (!this.isReadOutstanding)
                        {
                            ReadIOCompleted();
                        }
                    }
                }
            }
        }

        void WaitForWriteZero(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            bool success = false;
            try
            {
                WaitForSyncWrite(timeout, traceExceptionsAsErrors);
                success = true;
            }
            finally
            {
                lock (this.writeLock)
                {
                    try
                    {
                        if (success)
                        {
                            FinishSyncWrite(traceExceptionsAsErrors);
                        }
                    }
                    finally
                    {
                        ExitWritingState();
                        if (!this.isWriteOutstanding)
                        {
                            WriteIOCompleted();
                        }
                    }
                }
            }
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            WriteHelper(buffer, offset, size, immediate, timeout, ref this.writeOverlapped.Holder[0]);
        }

        // The holder is a perf optimization that lets us avoid repeatedly indexing into the array.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void WriteHelper(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, ref object holder)
        {
            try
            {
                FinishPendingWrite(timeout);

                ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);

                int bytesToWrite = size;
                if (size > this.writeBufferSize)
                {
                    size = this.writeBufferSize;
                }

                while (bytesToWrite > 0)
                {
                    lock (this.writeLock)
                    {
                        ValidateEnterWritingState(true);

                        StartSyncWrite(buffer, offset, size, ref holder);
                        EnterWritingState();
                    }

                    bool success = false;
                    try
                    {
                        WaitForSyncWrite(timeout, true, ref holder);
                        success = true;
                    }
                    finally
                    {
                        lock (this.writeLock)
                        {
                            try
                            {
                                if (success)
                                {
                                    FinishSyncWrite(true);
                                }
                            }
                            finally
                            {
                                ExitWritingState();
                                if (!this.isWriteOutstanding)
                                {
                                    WriteIOCompleted();
                                }
                            }
                        }
                    }

                    bytesToWrite -= size;
                    offset += size;
                    if (size > bytesToWrite)
                    {
                        size = bytesToWrite;
                    }
                }
            }
            catch (PipeException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Write), ExceptionEventType);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public unsafe void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            bool shouldReturnBuffer = true;

            try
            {
                if (size > this.writeBufferSize)
                {
                    WriteHelper(buffer, offset, size, immediate, timeout, ref this.writeOverlapped.Holder[0]);
                    return;
                }

                FinishPendingWrite(timeout);

                ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);

                lock (this.writeLock)
                {
                    ValidateEnterWritingState(true);

                    // This method avoids the call to GetOverlappedResult for synchronous completions.  Perf?
                    bool success = false;
                    try
                    {
                        shouldReturnBuffer = false;
                        StartSyncWrite(buffer, offset, size);
                        success = true;
                    }
                    finally
                    {
                        if (!this.isWriteOutstanding)
                        {
                            shouldReturnBuffer = true;
                        }
                        else
                        {
                            if (success)
                            {
                                EnterWritingState();

                                Fx.Assert(this.pendingWriteBuffer == null, "Need to pend a write but one's already pending.");
                                this.pendingWriteBuffer = buffer;
                                this.pendingWriteBufferManager = bufferManager;
                            }
                        }
                    }
                }
            }
            catch (PipeException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertPipeException(e, TransferOperation.Write), ExceptionEventType);
            }
            finally
            {
                if (shouldReturnBuffer)
                {
                    bufferManager.ReturnBuffer(buffer);
                }
            }
        }

        void ValidateEnterReadingState(bool checkEOF)
        {
            if (checkEOF)
            {
                if (closeState == CloseState.Closing)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeAlreadyClosing)), ExceptionEventType);
                }
            }

            if (inReadingState)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeReadPending)), ExceptionEventType);
            }

            if (closeState == CloseState.HandleClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeClosed)), ExceptionEventType);
            }
        }

        void ValidateEnterWritingState(bool checkShutdown)
        {
            if (checkShutdown)
            {
                if (isShutdownWritten)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeAlreadyShuttingDown)), ExceptionEventType);
                }

                if (closeState == CloseState.Closing)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeAlreadyClosing)), ExceptionEventType);
                }
            }

            if (inWritingState)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeWritePending)), ExceptionEventType);
            }

            if (closeState == CloseState.HandleClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(SR.GetString(SR.PipeClosed)), ExceptionEventType);
            }
        }

        void StartSyncRead(byte[] buffer, int offset, int size)
        {
            StartSyncRead(buffer, offset, size, ref this.readOverlapped.Holder[0]);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void StartSyncRead(byte[] buffer, int offset, int size, ref object holder)
        {
            if (this.isReadOutstanding)
            {
                throw Fx.AssertAndThrow("StartSyncRead called when read I/O was already pending.");
            }

            try
            {
                this.isReadOutstanding = true;
                this.readOverlapped.StartSyncOperation(buffer, ref holder);
                if (UnsafeNativeMethods.ReadFile(this.pipe.DangerousGetHandle(), this.readOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.readOverlapped.NativeOverlapped) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != UnsafeNativeMethods.ERROR_IO_PENDING)
                    {
                        this.isReadOutstanding = false;
                        if (error != UnsafeNativeMethods.ERROR_MORE_DATA)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateReadException(error));
                        }
                    }
                }
                else
                {
                    this.isReadOutstanding = false;
                }
            }
            finally
            {
                if (!this.isReadOutstanding)
                {
                    this.readOverlapped.CancelSyncOperation(ref holder);
                }
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void WaitForSyncRead(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            if (this.isReadOutstanding)
            {
                if (!this.readOverlapped.WaitForSyncOperation(timeout))
                {
                    Abort(SR.GetString(SR.PipeConnectionAbortedReadTimedOut, this.readTimeout), TransferOperation.Read);

                    Exception timeoutException = new TimeoutException(SR.GetString(SR.PipeReadTimedOut, timeout));
                    TraceEventType traceEventType = TraceEventType.Information;
                    if (traceExceptionsAsErrors)
                    {
                        traceEventType = TraceEventType.Error;
                    }

                    // This intentionally doesn't reset isReadOutstanding, because technically it still is, and we need to not free the buffer.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(timeoutException, traceEventType);
                }
                else
                {
                    this.isReadOutstanding = false;
                }
            }
        }

        // Must be called in a lock.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe int FinishSyncRead(bool traceExceptionsAsErrors)
        {
            int bytesRead = -1;
            Exception readException;

            if (this.closeState == CloseState.HandleClosed)
            {
                readException = CreatePipeClosedException(TransferOperation.Read);
            }
            else
            {
                readException = Exceptions.GetOverlappedReadException(this.pipe, this.readOverlapped.NativeOverlapped, out bytesRead);
            }
            if (readException != null)
            {
                TraceEventType traceEventType = TraceEventType.Information;
                if (traceExceptionsAsErrors)
                {
                    traceEventType = TraceEventType.Error;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(readException, traceEventType);
            }

            return bytesRead;
        }

        void StartSyncWrite(byte[] buffer, int offset, int size)
        {
            StartSyncWrite(buffer, offset, size, ref this.writeOverlapped.Holder[0]);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void StartSyncWrite(byte[] buffer, int offset, int size, ref object holder)
        {
            if (this.isWriteOutstanding)
            {
                throw Fx.AssertAndThrow("StartSyncWrite called when write I/O was already pending.");
            }

            try
            {
                this.syncWriteSize = size;
                this.isWriteOutstanding = true;
                this.writeOverlapped.StartSyncOperation(buffer, ref holder);
                if (UnsafeNativeMethods.WriteFile(this.pipe.DangerousGetHandle(), this.writeOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.writeOverlapped.NativeOverlapped) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != UnsafeNativeMethods.ERROR_IO_PENDING)
                    {
                        this.isWriteOutstanding = false;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateWriteException(error));
                    }
                }
                else
                {
                    this.isWriteOutstanding = false;
                }
            }
            finally
            {
                if (!this.isWriteOutstanding)
                {
                    this.writeOverlapped.CancelSyncOperation(ref holder);
                }
            }
        }

        void WaitForSyncWrite(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            WaitForSyncWrite(timeout, traceExceptionsAsErrors, ref this.writeOverlapped.Holder[0]);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void WaitForSyncWrite(TimeSpan timeout, bool traceExceptionsAsErrors, ref object holder)
        {
            if (this.isWriteOutstanding)
            {
                if (!this.writeOverlapped.WaitForSyncOperation(timeout, ref holder))
                {
                    Abort(SR.GetString(SR.PipeConnectionAbortedWriteTimedOut, this.writeTimeout), TransferOperation.Write);

                    Exception timeoutException = new TimeoutException(SR.GetString(SR.PipeWriteTimedOut, timeout));
                    TraceEventType traceEventType = TraceEventType.Information;
                    if (traceExceptionsAsErrors)
                    {
                        traceEventType = TraceEventType.Error;
                    }

                    // This intentionally doesn't reset isWriteOutstanding, because technically it still is, and we need to not free the buffer.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(timeoutException, traceEventType);
                }
                else
                {
                    this.isWriteOutstanding = false;
                }
            }
        }

        // Must be called in a lock.
        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        unsafe void FinishSyncWrite(bool traceExceptionsAsErrors)
        {
            int bytesWritten;
            Exception writeException;

            if (this.closeState == CloseState.HandleClosed)
            {
                writeException = CreatePipeClosedException(TransferOperation.Write);
            }
            else
            {
                writeException = Exceptions.GetOverlappedWriteException(this.pipe, this.writeOverlapped.NativeOverlapped, out bytesWritten);
                if (writeException == null && bytesWritten != this.syncWriteSize)
                {
                    writeException = new PipeException(SR.GetString(SR.PipeWriteIncomplete));
                }
            }

            if (writeException != null)
            {
                TraceEventType traceEventType = TraceEventType.Information;
                if (traceExceptionsAsErrors)
                {
                    traceEventType = TraceEventType.Error;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(writeException, traceEventType);
            }
        }

        enum CloseState
        {
            Open,
            Closing,
            HandleClosed,
        }

        enum TransferOperation
        {
            Write,
            Read,
            Undefined,
        }

        static class Exceptions
        {
            static PipeException CreateException(string resourceString, int error)
            {
                return new PipeException(SR.GetString(resourceString, PipeError.GetErrorString(error)), error);
            }

            public static PipeException CreateReadException(int error)
            {
                return CreateException(SR.PipeReadError, error);
            }

            public static PipeException CreateWriteException(int error)
            {
                return CreateException(SR.PipeWriteError, error);
            }

            // Must be called in a lock, after checking for HandleClosed.
            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            public static unsafe PipeException GetOverlappedWriteException(PipeHandle pipe,
                NativeOverlapped* nativeOverlapped, out int bytesWritten)
            {
                if (UnsafeNativeMethods.GetOverlappedResult(pipe.DangerousGetHandle(), nativeOverlapped, out bytesWritten, 0) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    return Exceptions.CreateWriteException(error);
                }
                else
                {
                    return null;
                }
            }

            // Must be called in a lock, after checking for HandleClosed.
            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            public static unsafe PipeException GetOverlappedReadException(PipeHandle pipe,
                NativeOverlapped* nativeOverlapped, out int bytesRead)
            {
                if (UnsafeNativeMethods.GetOverlappedResult(pipe.DangerousGetHandle(), nativeOverlapped, out bytesRead, 0) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == UnsafeNativeMethods.ERROR_MORE_DATA)
                    {
                        return null;
                    }

                    else
                    {
                        return Exceptions.CreateReadException(error);
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }


    class PipeConnectionInitiator : IConnectionInitiator
    {
        int bufferSize;
        IPipeTransportFactorySettings pipeSettings;

        public PipeConnectionInitiator(int bufferSize, IPipeTransportFactorySettings pipeSettings)
        {
            this.bufferSize = bufferSize;
            this.pipeSettings = pipeSettings;
        }

        Exception CreateConnectFailedException(Uri remoteUri, PipeException innerException)
        {
            return new CommunicationException(
                SR.GetString(SR.PipeConnectFailed, remoteUri.AbsoluteUri), innerException);
        }

        public IConnection Connect(Uri remoteUri, TimeSpan timeout)
        {
            string resolvedAddress;
            BackoffTimeoutHelper backoffHelper;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.PrepareConnect(remoteUri, timeoutHelper.RemainingTime(), out resolvedAddress, out backoffHelper);

            IConnection connection = null;
            while (connection == null)
            {
                connection = this.TryConnect(remoteUri, resolvedAddress, backoffHelper);
                if (connection == null)
                {
                    backoffHelper.WaitAndBackoff();

                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(
                            TraceEventType.Information,
                            TraceCode.FailedPipeConnect,
                            SR.GetString(
                                SR.TraceCodeFailedPipeConnect,
                                timeoutHelper.RemainingTime(),
                                remoteUri));
                    }
                }
            }
            return connection;
        }

        internal static string GetPipeName(Uri uri, IPipeTransportFactorySettings transportFactorySettings)
        {
            AppContainerInfo appInfo = GetAppContainerInfo(transportFactorySettings);

            // for wildcard hostName support, we first try and connect to the StrongWildcard,
            // then the Exact HostName, and lastly the WeakWildcard
            string[] hostChoices = new string[] { "+", uri.Host, "*" };
            bool[] globalChoices = new bool[] { true, false };
            for (int i = 0; i < hostChoices.Length; i++)
            {
                for (int iGlobal = 0; iGlobal < globalChoices.Length; iGlobal++)
                {

                    if (appInfo != null && globalChoices[iGlobal])
                    {
                        // Don't look at shared memory to acces pipes 
                        // that are created in the local NamedObjectPath
                        continue;
                    }

                    // walk up the path hierarchy, looking for first match
                    string path = PipeUri.GetPath(uri);

                    while (path.Length > 0)
                    {

                        string sharedMemoryName = PipeUri.BuildSharedMemoryName(hostChoices[i], path, globalChoices[iGlobal], appInfo);
                        try
                        {
                            PipeSharedMemory sharedMemory = PipeSharedMemory.Open(sharedMemoryName, uri);
                            if (sharedMemory != null)
                            {
                                try
                                {
                                    string pipeName = sharedMemory.GetPipeName(appInfo);
                                    if (pipeName != null)
                                    {
                                        return pipeName;
                                    }
                                }
                                finally
                                {
                                    sharedMemory.Dispose();
                                }
                            }
                        }
                        catch (AddressAccessDeniedException exception)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(
                                SR.EndpointNotFound, uri.AbsoluteUri), exception));
                        }

                        path = PipeUri.GetParentPath(path);
                    }
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, uri.AbsoluteUri),
                new PipeException(SR.GetString(SR.PipeEndpointNotFound, uri.AbsoluteUri))));
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ConnectAsyncResult(this, uri, timeout, callback, state);
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            return ConnectAsyncResult.End(result);
        }

        void PrepareConnect(Uri remoteUri, TimeSpan timeout, out string resolvedAddress, out BackoffTimeoutHelper backoffHelper)
        {
            PipeUri.Validate(remoteUri);
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(System.Diagnostics.TraceEventType.Information, TraceCode.InitiatingNamedPipeConnection,
                    SR.GetString(SR.TraceCodeInitiatingNamedPipeConnection),
                    new StringTraceRecord("Uri", remoteUri.ToString()), this, null);
            }
            resolvedAddress = GetPipeName(remoteUri, this.pipeSettings);

            const int backoffBufferMilliseconds = 150;
            TimeSpan backoffTimeout;
            if (timeout >= TimeSpan.FromMilliseconds(backoffBufferMilliseconds * 2))
            {
                backoffTimeout = TimeoutHelper.Add(timeout, TimeSpan.Zero - TimeSpan.FromMilliseconds(backoffBufferMilliseconds));
            }
            else
            {
                backoffTimeout = Ticks.ToTimeSpan((Ticks.FromMilliseconds(backoffBufferMilliseconds) / 2) + 1);
            }

            backoffHelper = new BackoffTimeoutHelper(backoffTimeout, TimeSpan.FromMinutes(5));
        }

        [ResourceConsumption(ResourceScope.Machine)]
        IConnection TryConnect(Uri remoteUri, string resolvedAddress, BackoffTimeoutHelper backoffHelper)
        {
            const int access = UnsafeNativeMethods.GENERIC_READ | UnsafeNativeMethods.GENERIC_WRITE;
            bool lastAttempt = backoffHelper.IsExpired();

            int flags = UnsafeNativeMethods.FILE_FLAG_OVERLAPPED;

            // By default Windows named pipe connection is created with impersonation, but we want
            // to create it with anonymous and let WCF take care of impersonation/identification.
            flags |= UnsafeNativeMethods.SECURITY_QOS_PRESENT | UnsafeNativeMethods.SECURITY_ANONYMOUS;

            PipeHandle pipeHandle = UnsafeNativeMethods.CreateFile(resolvedAddress, access, 0, IntPtr.Zero,
                UnsafeNativeMethods.OPEN_EXISTING, flags, IntPtr.Zero);
            int error = Marshal.GetLastWin32Error();
            if (pipeHandle.IsInvalid)
            {
                pipeHandle.SetHandleAsInvalid();
            }
            else
            {
                int mode = UnsafeNativeMethods.PIPE_READMODE_MESSAGE;
                if (UnsafeNativeMethods.SetNamedPipeHandleState(pipeHandle, ref mode, IntPtr.Zero, IntPtr.Zero) == 0)
                {
                    error = Marshal.GetLastWin32Error();
                    pipeHandle.Close();
                    PipeException innerException = new PipeException(SR.GetString(SR.PipeModeChangeFailed,
                        PipeError.GetErrorString(error)), error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        CreateConnectFailedException(remoteUri, innerException));
                }
                return new PipeConnection(pipeHandle, bufferSize, false, true);
            }

            if (error == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND || error == UnsafeNativeMethods.ERROR_PIPE_BUSY)
            {
                if (lastAttempt)
                {
                    Exception innerException = new PipeException(SR.GetString(SR.PipeConnectAddressFailed,
                        resolvedAddress, PipeError.GetErrorString(error)), error);

                    TimeoutException timeoutException;
                    string endpoint = remoteUri.AbsoluteUri;

                    if (error == UnsafeNativeMethods.ERROR_PIPE_BUSY)
                    {
                        timeoutException = new TimeoutException(SR.GetString(SR.PipeConnectTimedOutServerTooBusy,
                            endpoint, backoffHelper.OriginalTimeout), innerException);
                    }
                    else
                    {
                        timeoutException = new TimeoutException(SR.GetString(SR.PipeConnectTimedOut,
                            endpoint, backoffHelper.OriginalTimeout), innerException);
                    }

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(timeoutException);
                }

                return null;
            }
            else
            {
                PipeException innerException = new PipeException(SR.GetString(SR.PipeConnectAddressFailed,
                    resolvedAddress, PipeError.GetErrorString(error)), error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    CreateConnectFailedException(remoteUri, innerException));
            }
        }

        static AppContainerInfo GetAppContainerInfo(IPipeTransportFactorySettings transportFactorySettings)
        {
            if (AppContainerInfo.IsAppContainerSupported &&
                transportFactorySettings != null &&
                transportFactorySettings.PipeSettings != null)
            {
                ApplicationContainerSettings appSettings = transportFactorySettings.PipeSettings.ApplicationContainerSettings;
                if (appSettings != null && appSettings.TargetingAppContainer)
                {
                    return AppContainerInfo.CreateAppContainerInfo(appSettings.PackageFullName, appSettings.SessionId);
                }
            }

            return null;
        }

        class ConnectAsyncResult : AsyncResult
        {
            PipeConnectionInitiator parent;
            Uri remoteUri;
            string resolvedAddress;
            BackoffTimeoutHelper backoffHelper;
            TimeoutHelper timeoutHelper;
            IConnection connection;
            static Action<object> waitCompleteCallback;

            public ConnectAsyncResult(PipeConnectionInitiator parent, Uri remoteUri, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.remoteUri = remoteUri;
                this.timeoutHelper = new TimeoutHelper(timeout);
                parent.PrepareConnect(remoteUri, this.timeoutHelper.RemainingTime(), out this.resolvedAddress, out this.backoffHelper);

                if (this.ConnectAndWait())
                {
                    this.Complete(true);
                }
            }

            bool ConnectAndWait()
            {
                this.connection = this.parent.TryConnect(this.remoteUri, this.resolvedAddress, this.backoffHelper);
                bool completed = (this.connection != null);
                if (!completed)
                {
                    if (waitCompleteCallback == null)
                    {
                        waitCompleteCallback = new Action<object>(OnWaitComplete);
                    }
                    this.backoffHelper.WaitAndBackoff(waitCompleteCallback, this);
                }
                return completed;
            }

            public static IConnection End(IAsyncResult result)
            {
                ConnectAsyncResult thisPtr = AsyncResult.End<ConnectAsyncResult>(result);
                return thisPtr.connection;
            }

            static void OnWaitComplete(object state)
            {
                Exception exception = null;
                ConnectAsyncResult thisPtr = (ConnectAsyncResult)state;

                bool completeSelf = true;
                try
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(
                            TraceEventType.Information,
                            TraceCode.FailedPipeConnect,
                            SR.GetString(
                                SR.TraceCodeFailedPipeConnect,
                                thisPtr.timeoutHelper.RemainingTime(),
                                thisPtr.remoteUri));
                    }

                    completeSelf = thisPtr.ConnectAndWait();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    exception = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, exception);
                }
            }
        }
    }

    class PipeConnectionListener : IConnectionListener
    {
        Uri pipeUri;
        int bufferSize;
        HostNameComparisonMode hostNameComparisonMode;
        bool isDisposed;
        bool isListening;
        List<PendingAccept> pendingAccepts;
        bool anyPipesCreated;
        PipeSharedMemory sharedMemory;
        List<SecurityIdentifier> allowedSids;
        bool useCompletionPort;
        int maxInstances;

        public PipeConnectionListener(Uri pipeUri, HostNameComparisonMode hostNameComparisonMode, int bufferSize,
            List<SecurityIdentifier> allowedSids, bool useCompletionPort, int maxConnections)
        {
            PipeUri.Validate(pipeUri);
            this.pipeUri = pipeUri;
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.allowedSids = allowedSids;
            this.bufferSize = bufferSize;
            pendingAccepts = new List<PendingAccept>();
            this.useCompletionPort = useCompletionPort;
            this.maxInstances = Math.Min(maxConnections, UnsafeNativeMethods.PIPE_UNLIMITED_INSTANCES);
        }

        object ThisLock
        {
            get { return this; }
        }

        public string PipeName { get { return sharedMemory.PipeName; } }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            lock (ThisLock)
            {
                if (isDisposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException("", SR.GetString(SR.PipeListenerDisposed)));
                }

                if (!isListening)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.PipeListenerNotListening)));
                }

                PipeHandle pipeHandle = CreatePipe();
                PendingAccept pendingAccept = new PendingAccept(this, pipeHandle, useCompletionPort, callback, state);
                if (!pendingAccept.CompletedSynchronously)
                {
                    this.pendingAccepts.Add(pendingAccept);
                }
                return pendingAccept;
            }
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            PendingAccept pendingAccept = result as PendingAccept;
            if (pendingAccept == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", SR.GetString(SR.InvalidAsyncResult));
            }

            PipeHandle acceptedPipe = pendingAccept.End();

            if (acceptedPipe == null)
            {
                return null;
            }
            else
            {
                return new PipeConnection(acceptedPipe, bufferSize,
                    pendingAccept.IsBoundToCompletionPort, pendingAccept.IsBoundToCompletionPort);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [ResourceConsumption(ResourceScope.Machine)]
        unsafe PipeHandle CreatePipe()
        {
            int openMode = UnsafeNativeMethods.PIPE_ACCESS_DUPLEX | UnsafeNativeMethods.FILE_FLAG_OVERLAPPED;
            if (!anyPipesCreated)
            {
                openMode |= UnsafeNativeMethods.FILE_FLAG_FIRST_PIPE_INSTANCE;
            }

            byte[] binarySecurityDescriptor;

            try
            {
                binarySecurityDescriptor = SecurityDescriptorHelper.FromSecurityIdentifiers(allowedSids, UnsafeNativeMethods.GENERIC_READ | UnsafeNativeMethods.GENERIC_WRITE);
            }
            catch (Win32Exception e)
            {
                // While Win32exceptions are not expected, if they do occur we need to obey the pipe/communication exception model.
                Exception innerException = new PipeException(e.Message, e);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException));
            }

            PipeHandle pipeHandle;
            int error;
            string pipeName = null;
            fixed (byte* pinnedSecurityDescriptor = binarySecurityDescriptor)
            {
                UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
                securityAttributes.lpSecurityDescriptor = (IntPtr)pinnedSecurityDescriptor;

                pipeName = this.sharedMemory.PipeName;
                pipeHandle = UnsafeNativeMethods.CreateNamedPipe(
                                                    pipeName,
                                                    openMode,
                                                    UnsafeNativeMethods.PIPE_TYPE_MESSAGE | UnsafeNativeMethods.PIPE_READMODE_MESSAGE,
                                                    maxInstances, bufferSize, bufferSize, 0, securityAttributes);
                error = Marshal.GetLastWin32Error();
            }

            if (pipeHandle.IsInvalid)
            {
                pipeHandle.SetHandleAsInvalid();

                Exception innerException = new PipeException(SR.GetString(SR.PipeListenFailed,
                    pipeUri.AbsoluteUri, PipeError.GetErrorString(error)), error);

                if (error == UnsafeNativeMethods.ERROR_ACCESS_DENIED)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAccessDeniedException(innerException.Message, innerException));
                }
                else if (error == UnsafeNativeMethods.ERROR_ALREADY_EXISTS)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAlreadyInUseException(innerException.Message, innerException));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException));
                }
            }
            else
            {
                if (TD.NamedPipeCreatedIsEnabled())
                {
                    TD.NamedPipeCreated(pipeName);
                }
            }

            bool closePipe = true;
            try
            {
                if (useCompletionPort)
                {
                    ThreadPool.BindHandle(pipeHandle);
                }
                anyPipesCreated = true;
                closePipe = false;
                return pipeHandle;
            }
            finally
            {
                if (closePipe)
                {
                    pipeHandle.Close();
                }
            }
        }

        public void Dispose()
        {
            lock (ThisLock)
            {
                if (!isDisposed)
                {
                    if (sharedMemory != null)
                    {
                        sharedMemory.Dispose();
                    }
                    for (int i = 0; i < pendingAccepts.Count; i++)
                    {
                        pendingAccepts[i].Abort();
                    }
                    isDisposed = true;
                }
            }
        }

        public void Listen()
        {
            lock (ThisLock)
            {
                if (!isListening)
                {
                    string sharedMemoryName = PipeUri.BuildSharedMemoryName(pipeUri, hostNameComparisonMode, true);
                    if (!PipeSharedMemory.TryCreate(allowedSids, pipeUri, sharedMemoryName, out this.sharedMemory))
                    {
                        PipeSharedMemory tempSharedMemory = null;

                        // first see if we're in RANU by creating a unique Uri in the global namespace
                        Uri tempUri = new Uri(pipeUri, Guid.NewGuid().ToString());
                        string tempSharedMemoryName = PipeUri.BuildSharedMemoryName(tempUri, hostNameComparisonMode, true);
                        if (PipeSharedMemory.TryCreate(allowedSids, tempUri, tempSharedMemoryName, out tempSharedMemory))
                        {
                            // we're not RANU, throw PipeNameInUse
                            tempSharedMemory.Dispose();
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                PipeSharedMemory.CreatePipeNameInUseException(UnsafeNativeMethods.ERROR_ACCESS_DENIED, pipeUri));
                        }
                        else
                        {
                            // try the session namespace since we're RANU
                            sharedMemoryName = PipeUri.BuildSharedMemoryName(pipeUri, hostNameComparisonMode, false);
                            this.sharedMemory = PipeSharedMemory.Create(allowedSids, pipeUri, sharedMemoryName);
                        }
                    }

                    isListening = true;
                }
            }
        }

        void RemovePendingAccept(PendingAccept pendingAccept)
        {
            lock (ThisLock)
            {
                Fx.Assert(this.pendingAccepts.Contains(pendingAccept), "An unknown PendingAccept is removing itself.");
                this.pendingAccepts.Remove(pendingAccept);
            }
        }

        class PendingAccept : AsyncResult
        {
            PipeHandle pipeHandle;
            PipeHandle result;
            OverlappedIOCompleteCallback onAcceptComplete;
            static Action<object> onStartAccept;
            OverlappedContext overlapped;
            bool isBoundToCompletionPort;
            PipeConnectionListener listener;
            EventTraceActivity eventTraceActivity;

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            public unsafe PendingAccept(PipeConnectionListener listener, PipeHandle pipeHandle, bool isBoundToCompletionPort,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.pipeHandle = pipeHandle;
                this.result = pipeHandle;
                this.listener = listener;
                onAcceptComplete = new OverlappedIOCompleteCallback(OnAcceptComplete);
                overlapped = new OverlappedContext();
                this.isBoundToCompletionPort = isBoundToCompletionPort;

                if (TD.PipeConnectionAcceptStartIsEnabled())
                {
                    this.eventTraceActivity = new EventTraceActivity();
                    TD.PipeConnectionAcceptStart(this.eventTraceActivity, this.listener.pipeUri != null ? this.listener.pipeUri.ToString() : string.Empty);
                }

                if (!Thread.CurrentThread.IsThreadPoolThread)
                {
                    if (onStartAccept == null)
                    {
                        onStartAccept = new Action<object>(OnStartAccept);
                    }
                    ActionItem.Schedule(onStartAccept, this);
                }
                else
                {
                    StartAccept(true);
                }
            }

            public bool IsBoundToCompletionPort
            {
                get { return this.isBoundToCompletionPort; }
            }

            static void OnStartAccept(object state)
            {
                PendingAccept pendingAccept = (PendingAccept)state;
                pendingAccept.StartAccept(false);
            }

            Exception CreatePipeAcceptFailedException(int errorCode)
            {
                Exception innerException = new PipeException(SR.GetString(SR.PipeAcceptFailed,
                    PipeError.GetErrorString(errorCode)), errorCode);
                return new CommunicationException(innerException.Message, innerException);
            }

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            unsafe void StartAccept(bool synchronous)
            {
                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    try
                    {
                        this.overlapped.StartAsyncOperation(null, onAcceptComplete, this.isBoundToCompletionPort);
                        while (true)
                        {
                            if (UnsafeNativeMethods.ConnectNamedPipe(pipeHandle, overlapped.NativeOverlapped) == 0)
                            {
                                int error = Marshal.GetLastWin32Error();
                                switch (error)
                                {
                                    case UnsafeNativeMethods.ERROR_NO_DATA:
                                        if (UnsafeNativeMethods.DisconnectNamedPipe(pipeHandle) != 0)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            completeSelf = true;
                                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeAcceptFailedException(error));
                                        }
                                    case UnsafeNativeMethods.ERROR_PIPE_CONNECTED:
                                        completeSelf = true;
                                        break;
                                    case UnsafeNativeMethods.ERROR_IO_PENDING:
                                        break;
                                    default:
                                        completeSelf = true;
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeAcceptFailedException(error));
                                }
                            }
                            else
                            {
                                completeSelf = true;
                            }

                            break;
                        }
                    }
                    catch (ObjectDisposedException exception)
                    {
                        // A ---- with Abort can cause PipeHandle to throw this.
                        Fx.Assert(this.result == null, "Got an ObjectDisposedException but not an Abort!");
                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);
                        completeSelf = true;
                    }
                    finally
                    {
                        if (completeSelf)
                        {
                            this.overlapped.CancelAsyncOperation();
                            this.overlapped.Free();
                        }
                    }
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completeSelf = true;
                    completionException = e;
                }
                if (completeSelf)
                {
                    if (!synchronous)
                    {
                        this.listener.RemovePendingAccept(this);
                    }
                    base.Complete(synchronous, completionException);
                }
            }

            // Must be called in PipeConnectionListener's lock.
            public void Abort()
            {
                this.result = null; // we need to return null after an abort
                pipeHandle.Close();
            }

            public PipeHandle End()
            {
                AsyncResult.End<PendingAccept>(this);
                return this.result;
            }

            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            unsafe void OnAcceptComplete(bool haveResult, int error, int numBytes)
            {
                this.listener.RemovePendingAccept(this);

                if (!haveResult)
                {
                    // No ---- with Abort here since Abort can't be called once RemovePendingAccept happens.
                    if (this.result != null && UnsafeNativeMethods.GetOverlappedResult(this.pipeHandle,
                        this.overlapped.NativeOverlapped, out numBytes, 0) == 0)
                    {
                        error = Marshal.GetLastWin32Error();
                    }
                    else
                    {
                        error = 0;
                    }
                }

                this.overlapped.Free();

                if (TD.PipeConnectionAcceptStopIsEnabled())
                {
                    TD.PipeConnectionAcceptStop(this.eventTraceActivity);
                }

                if (error != 0)
                {
                    this.pipeHandle.Close();
                    base.Complete(false, CreatePipeAcceptFailedException(error));
                }
                else
                {
                    base.Complete(false);
                }
            }
        }
    }

    static class SecurityDescriptorHelper
    {
        static byte[] worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork;
        static byte[] worldCreatorOwnerWithReadDescriptorDenyNetwork;

        static SecurityDescriptorHelper()
        {
            worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork = FromSecurityIdentifiersFull(null, UnsafeNativeMethods.GENERIC_READ | UnsafeNativeMethods.GENERIC_WRITE);
            worldCreatorOwnerWithReadDescriptorDenyNetwork = FromSecurityIdentifiersFull(null, UnsafeNativeMethods.GENERIC_READ);
        }

        internal static byte[] FromSecurityIdentifiers(List<SecurityIdentifier> allowedSids, int accessRights)
        {
            if (allowedSids == null)
            {
                if (accessRights == (UnsafeNativeMethods.GENERIC_READ | UnsafeNativeMethods.GENERIC_WRITE))
                {
                    return worldCreatorOwnerWithReadAndWriteDescriptorDenyNetwork;
                }

                if (accessRights == UnsafeNativeMethods.GENERIC_READ)
                {
                    return worldCreatorOwnerWithReadDescriptorDenyNetwork;
                }
            }

            return FromSecurityIdentifiersFull(allowedSids, accessRights);
        }

        static byte[] FromSecurityIdentifiersFull(List<SecurityIdentifier> allowedSids, int accessRights)
        {
            int capacity = allowedSids == null ? 3 : 2 + allowedSids.Count;
            DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, capacity);

            // add deny ACE first so that we don't get short circuited
            dacl.AddAccess(AccessControlType.Deny, new SecurityIdentifier(WellKnownSidType.NetworkSid, null),
                UnsafeNativeMethods.GENERIC_ALL, InheritanceFlags.None, PropagationFlags.None);

            // clients get different rights, since they shouldn't be able to listen
            int clientAccessRights = GenerateClientAccessRights(accessRights);

            if (allowedSids == null)
            {
                dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    clientAccessRights, InheritanceFlags.None, PropagationFlags.None);
            }
            else
            {
                for (int i = 0; i < allowedSids.Count; i++)
                {
                    SecurityIdentifier allowedSid = allowedSids[i];
                    dacl.AddAccess(AccessControlType.Allow, allowedSid,
                        clientAccessRights, InheritanceFlags.None, PropagationFlags.None);
                }
            }

            dacl.AddAccess(AccessControlType.Allow, GetProcessLogonSid(), accessRights, InheritanceFlags.None, PropagationFlags.None);


            if (AppContainerInfo.IsRunningInAppContainer)
            {
                // NamedPipeBinding requires dacl with current AppContainer SID
                // to setup multiple NamedPipes in the BeginAccept loop.                
                dacl.AddAccess(
                            AccessControlType.Allow,
                            AppContainerInfo.GetCurrentAppContainerSid(),
                            accessRights,
                            InheritanceFlags.None,
                            PropagationFlags.None);
            }

            CommonSecurityDescriptor securityDescriptor =
                new CommonSecurityDescriptor(false, false, ControlFlags.None, null, null, null, dacl);
            byte[] binarySecurityDescriptor = new byte[securityDescriptor.BinaryLength];
            securityDescriptor.GetBinaryForm(binarySecurityDescriptor, 0);
            return binarySecurityDescriptor;
        }

        // Security: We cannot grant rights for FILE_CREATE_PIPE_INSTANCE to clients, otherwise other apps can intercept server side pipes.
        // FILE_CREATE_PIPE_INSTANCE is granted in 2 ways, via GENERIC_WRITE or directly specified. Remove both.
        static int GenerateClientAccessRights(int accessRights)
        {
            int everyoneAccessRights = accessRights;

            if ((everyoneAccessRights & UnsafeNativeMethods.GENERIC_WRITE) != 0)
            {
                everyoneAccessRights &= ~UnsafeNativeMethods.GENERIC_WRITE;

                // Since GENERIC_WRITE grants the permissions to write to a file, we need to add it back.
                const int clientWriteAccess = UnsafeNativeMethods.FILE_WRITE_ATTRIBUTES | UnsafeNativeMethods.FILE_WRITE_DATA | UnsafeNativeMethods.FILE_WRITE_EA;
                everyoneAccessRights |= clientWriteAccess;
            }

            // Future proofing: FILE_CREATE_PIPE_INSTANCE isn't used currently but we need to ensure it is not granted.
            everyoneAccessRights &= ~UnsafeNativeMethods.FILE_CREATE_PIPE_INSTANCE;

            return everyoneAccessRights;
        }

        // The logon sid is generated on process start up so it is unique to this process.
        static SecurityIdentifier GetProcessLogonSid()
        {
            int pid = Process.GetCurrentProcess().Id;
            return System.ServiceModel.Activation.Utility.GetLogonSidForPid(pid);
        }
    }

    unsafe class PipeSharedMemory : IDisposable
    {
        internal const string PipePrefix = @"\\.\pipe\";
        internal const string PipeLocalPrefix = @"\\.\pipe\Local\";
        SafeFileMappingHandle fileMapping;
        string pipeName;
        string pipeNameGuidPart;
        Uri pipeUri;

        PipeSharedMemory(SafeFileMappingHandle fileMapping, Uri pipeUri)
            : this(fileMapping, pipeUri, null)
        {
        }

        PipeSharedMemory(SafeFileMappingHandle fileMapping, Uri pipeUri, string pipeName)
        {
            this.pipeName = pipeName;
            this.fileMapping = fileMapping;
            this.pipeUri = pipeUri;
        }

        public static PipeSharedMemory Create(List<SecurityIdentifier> allowedSids, Uri pipeUri, string sharedMemoryName)
        {
            PipeSharedMemory result;
            if (TryCreate(allowedSids, pipeUri, sharedMemoryName, out result))
            {
                return result;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameInUseException(UnsafeNativeMethods.ERROR_ACCESS_DENIED, pipeUri));
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public unsafe static bool TryCreate(List<SecurityIdentifier> allowedSids, Uri pipeUri, string sharedMemoryName, out PipeSharedMemory result)
        {
            Guid pipeGuid = Guid.NewGuid();
            string pipeName = BuildPipeName(pipeGuid.ToString());
            byte[] binarySecurityDescriptor;
            try
            {
                binarySecurityDescriptor = SecurityDescriptorHelper.FromSecurityIdentifiers(allowedSids, UnsafeNativeMethods.GENERIC_READ);
            }
            catch (Win32Exception e)
            {
                // While Win32exceptions are not expected, if they do occur we need to obey the pipe/communication exception model.
                Exception innerException = new PipeException(e.Message, e);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(innerException.Message, innerException));
            }

            SafeFileMappingHandle fileMapping;
            int error;
            result = null;
            fixed (byte* pinnedSecurityDescriptor = binarySecurityDescriptor)
            {
                UnsafeNativeMethods.SECURITY_ATTRIBUTES securityAttributes = new UnsafeNativeMethods.SECURITY_ATTRIBUTES();
                securityAttributes.lpSecurityDescriptor = (IntPtr)pinnedSecurityDescriptor;

                fileMapping = UnsafeNativeMethods.CreateFileMapping((IntPtr)(-1), securityAttributes,
                    UnsafeNativeMethods.PAGE_READWRITE, 0, sizeof(SharedMemoryContents), sharedMemoryName);
                error = Marshal.GetLastWin32Error();
            }

            if (fileMapping.IsInvalid)
            {
                fileMapping.SetHandleAsInvalid();
                if (error == UnsafeNativeMethods.ERROR_ACCESS_DENIED)
                {
                    return false;
                }
                else
                {
                    Exception innerException = new PipeException(SR.GetString(SR.PipeNameCantBeReserved,
                        pipeUri.AbsoluteUri, PipeError.GetErrorString(error)), error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new AddressAccessDeniedException(innerException.Message, innerException));
                }
            }

            // now we have a valid file mapping handle
            if (error == UnsafeNativeMethods.ERROR_ALREADY_EXISTS)
            {
                fileMapping.Close();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameInUseException(error, pipeUri));
            }
            PipeSharedMemory pipeSharedMemory = new PipeSharedMemory(fileMapping, pipeUri, pipeName);
            bool disposeSharedMemory = true;
            try
            {
                pipeSharedMemory.InitializeContents(pipeGuid);
                disposeSharedMemory = false;
                result = pipeSharedMemory;

                if (TD.PipeSharedMemoryCreatedIsEnabled())
                {
                    TD.PipeSharedMemoryCreated(sharedMemoryName);
                }
                return true;
            }
            finally
            {
                if (disposeSharedMemory)
                {
                    pipeSharedMemory.Dispose();
                }
            }
        }

        [ResourceConsumption(ResourceScope.Machine)]
        public static PipeSharedMemory Open(string sharedMemoryName, Uri pipeUri)
        {
            SafeFileMappingHandle fileMapping = UnsafeNativeMethods.OpenFileMapping(
                UnsafeNativeMethods.FILE_MAP_READ, false, sharedMemoryName);
            if (fileMapping.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                fileMapping.SetHandleAsInvalid();
                if (error == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND)
                {
                    fileMapping = UnsafeNativeMethods.OpenFileMapping(
                        UnsafeNativeMethods.FILE_MAP_READ, false, "Global\\" + sharedMemoryName);
                    if (fileMapping.IsInvalid)
                    {
                        error = Marshal.GetLastWin32Error();
                        fileMapping.SetHandleAsInvalid();
                        if (error == UnsafeNativeMethods.ERROR_FILE_NOT_FOUND)
                        {
                            return null;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameCannotBeAccessedException(error, pipeUri));
                    }
                    return new PipeSharedMemory(fileMapping, pipeUri);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameCannotBeAccessedException(error, pipeUri));
            }
            return new PipeSharedMemory(fileMapping, pipeUri);
        }

        public void Dispose()
        {
            if (fileMapping != null)
            {
                fileMapping.Close();
                fileMapping = null;
            }
        }

        public string PipeName
        {
            [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
            get
            {
                if (pipeName == null)
                {
                    SafeViewOfFileHandle view = GetView(false);
                    try
                    {
                        SharedMemoryContents* contents = (SharedMemoryContents*)view.DangerousGetHandle();
                        if (contents->isInitialized)
                        {
                            Thread.MemoryBarrier();
                            this.pipeNameGuidPart = contents->pipeGuid.ToString();
                            this.pipeName = BuildPipeName(this.pipeNameGuidPart);
                        }
                    }
                    finally
                    {
                        view.Close();
                    }
                }
                return pipeName;
            }
        }

        internal string GetPipeName(AppContainerInfo appInfo)
        {
            if (appInfo == null)
            {
                return this.PipeName;
            }
            else if (this.PipeName != null)
            {
                // Build the PipeName for a pipe inside an AppContainer as follows
                // \\.\pipe\Sessions\<SessionId>\<NamedObjectPath>\<PipeGuid>
                return string.Format(
                            CultureInfo.InvariantCulture,
                            @"\\.\pipe\Sessions\{0}\{1}\{2}",
                            appInfo.SessionId,
                            appInfo.NamedObjectPath,
                            this.pipeNameGuidPart);
            }

            return null;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        void InitializeContents(Guid pipeGuid)
        {
            SafeViewOfFileHandle view = GetView(true);
            try
            {
                SharedMemoryContents* contents = (SharedMemoryContents*)view.DangerousGetHandle();
                contents->pipeGuid = pipeGuid;
                Thread.MemoryBarrier();
                contents->isInitialized = true;
            }
            finally
            {
                view.Close();
            }
        }

        public static Exception CreatePipeNameInUseException(int error, Uri pipeUri)
        {
            Exception innerException = new PipeException(SR.GetString(SR.PipeNameInUse, pipeUri.AbsoluteUri), error);
            return new AddressAlreadyInUseException(innerException.Message, innerException);
        }

        static Exception CreatePipeNameCannotBeAccessedException(int error, Uri pipeUri)
        {
            Exception innerException = new PipeException(SR.GetString(SR.PipeNameCanNotBeAccessed,
                PipeError.GetErrorString(error)), error);
            return new AddressAccessDeniedException(SR.GetString(SR.PipeNameCanNotBeAccessed2, pipeUri.AbsoluteUri), innerException);
        }

        SafeViewOfFileHandle GetView(bool writable)
        {
            SafeViewOfFileHandle handle = UnsafeNativeMethods.MapViewOfFile(fileMapping,
                writable ? UnsafeNativeMethods.FILE_MAP_WRITE : UnsafeNativeMethods.FILE_MAP_READ,
                0, 0, (IntPtr)sizeof(SharedMemoryContents));
            if (handle.IsInvalid)
            {
                int error = Marshal.GetLastWin32Error();
                handle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreatePipeNameCannotBeAccessedException(error, pipeUri));
            }
            return handle;
        }

        static string BuildPipeName(string pipeGuid)
        {
            return (AppContainerInfo.IsRunningInAppContainer ? PipeLocalPrefix : PipePrefix) + pipeGuid;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SharedMemoryContents
        {
            public bool isInitialized;
            public Guid pipeGuid;
        }
    }

    static class PipeUri
    {
        public static string BuildSharedMemoryName(Uri uri, HostNameComparisonMode hostNameComparisonMode, bool global)
        {
            string path = PipeUri.GetPath(uri);
            string host = null;

            switch (hostNameComparisonMode)
            {
                case HostNameComparisonMode.StrongWildcard:
                    host = "+";
                    break;
                case HostNameComparisonMode.Exact:
                    host = uri.Host;
                    break;
                case HostNameComparisonMode.WeakWildcard:
                    host = "*";
                    break;
            }

            return PipeUri.BuildSharedMemoryName(host, path, global);
        }

        internal static string BuildSharedMemoryName(string hostName, string path, bool global, AppContainerInfo appContainerInfo)
        {
            if (appContainerInfo == null)
            {
                return BuildSharedMemoryName(hostName, path, global);
            }
            else
            {
                Fx.Assert(appContainerInfo.SessionId != ApplicationContainerSettingsDefaults.CurrentSession, "Session has not yet been initialized.");
                Fx.Assert(!String.IsNullOrEmpty(appContainerInfo.NamedObjectPath),
                    "NamedObjectPath cannot be empty when creating the SharedMemoryName when running in an AppContainer.");

                //We need to use a session symlink for the lowbox appcontainer.
                // Session\{0}\{1}\{2}\<SharedMemoryName>                
                return string.Format(
                            CultureInfo.InvariantCulture,
                            @"Session\{0}\{1}\{2}",
                            appContainerInfo.SessionId,
                            appContainerInfo.NamedObjectPath,
                            BuildSharedMemoryName(hostName, path, global));
            }
        }

        static string BuildSharedMemoryName(string hostName, string path, bool global)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Uri.UriSchemeNetPipe);
            builder.Append("://");
            builder.Append(hostName.ToUpperInvariant());
            builder.Append(path);
            string canonicalName = builder.ToString();

            byte[] canonicalBytes = Encoding.UTF8.GetBytes(canonicalName);
            byte[] hashedBytes;
            string separator;

            if (canonicalBytes.Length >= 128)
            {
                using (HashAlgorithm hash = GetHashAlgorithm())
                {
                    hashedBytes = hash.ComputeHash(canonicalBytes);
                }
                separator = ":H";
            }
            else
            {
                hashedBytes = canonicalBytes;
                separator = ":E";
            }

            builder = new StringBuilder();
            if (global)
            {
                // we may need to create the shared memory in the global namespace so we work with terminal services+admin 
                builder.Append("Global\\");
            }
            else
            {
                builder.Append("Local\\");
            }
            builder.Append(Uri.UriSchemeNetPipe);
            builder.Append(separator);
            builder.Append(Convert.ToBase64String(hashedBytes));
            return builder.ToString();
        }

        static HashAlgorithm GetHashAlgorithm()
        {
            if (SecurityUtilsEx.RequiresFipsCompliance)
                return new SHA1CryptoServiceProvider();
            else
                return new SHA1Managed();
        }

        public static string GetPath(Uri uri)
        {
            string path = uri.LocalPath.ToUpperInvariant();
            if (!path.EndsWith("/", StringComparison.Ordinal))
                path = path + "/";
            return path;
        }

        public static string GetParentPath(string path)
        {
            if (path.EndsWith("/", StringComparison.Ordinal))
                path = path.Substring(0, path.Length - 1);
            if (path.Length == 0)
                return path;
            return path.Substring(0, path.LastIndexOf('/') + 1);
        }

        public static void Validate(Uri uri)
        {
            if (uri.Scheme != Uri.UriSchemeNetPipe)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("uri", SR.GetString(SR.PipeUriSchemeWrong));
        }
    }

    static class PipeError
    {
        public static string GetErrorString(int error)
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            if (UnsafeNativeMethods.FormatMessage(UnsafeNativeMethods.FORMAT_MESSAGE_IGNORE_INSERTS |
                UnsafeNativeMethods.FORMAT_MESSAGE_FROM_SYSTEM | UnsafeNativeMethods.FORMAT_MESSAGE_ARGUMENT_ARRAY,
                IntPtr.Zero, error, CultureInfo.CurrentCulture.LCID, stringBuilder, stringBuilder.Capacity, IntPtr.Zero) != 0)
            {
                stringBuilder = stringBuilder.Replace("\n", "");
                stringBuilder = stringBuilder.Replace("\r", "");
                return SR.GetString(
                    SR.PipeKnownWin32Error,
                    stringBuilder.ToString(),
                    error.ToString(CultureInfo.InvariantCulture),
                    Convert.ToString(error, 16));
            }
            else
            {
                return SR.GetString(
                    SR.PipeUnknownWin32Error,
                    error.ToString(CultureInfo.InvariantCulture),
                    Convert.ToString(error, 16));
            }
        }
    }
}
