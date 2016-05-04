//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Text;
    using System.Threading;

    class SocketConnection : IConnection
    {
        static AsyncCallback onReceiveCompleted;
        static EventHandler<SocketAsyncEventArgs> onReceiveAsyncCompleted;
        static EventHandler<SocketAsyncEventArgs> onSocketSendCompleted;

        // common state
        Socket socket;
        TimeSpan sendTimeout;
        TimeSpan readFinTimeout;
        TimeSpan receiveTimeout;
        CloseState closeState;
        bool isShutdown;
        bool noDelay = false;
        bool aborted;
        TraceEventType exceptionEventType;

        // close state
        TimeoutHelper closeTimeoutHelper;
        static WaitCallback onWaitForFinComplete = new WaitCallback(OnWaitForFinComplete);

        // read state
        int asyncReadSize;
        SocketAsyncEventArgs asyncReadEventArgs;
        byte[] readBuffer;
        int asyncReadBufferSize;
        object asyncReadState;
        WaitCallback asyncReadCallback;
        Exception asyncReadException;
        bool asyncReadPending;

        // write state
        SocketAsyncEventArgs asyncWriteEventArgs;
        object asyncWriteState;
        WaitCallback asyncWriteCallback;
        Exception asyncWriteException;
        bool asyncWritePending;

        IOThreadTimer receiveTimer;
        static Action<object> onReceiveTimeout;
        IOThreadTimer sendTimer;
        static Action<object> onSendTimeout;
        string timeoutErrorString;
        TransferOperation timeoutErrorTransferOperation;
        IPEndPoint remoteEndpoint;
        ConnectionBufferPool connectionBufferPool;
        string remoteEndpointAddress;

        public SocketConnection(Socket socket, ConnectionBufferPool connectionBufferPool, bool autoBindToCompletionPort)
        {
            if (socket == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("socket");
            }

            Fx.Assert(connectionBufferPool != null, "Argument connectionBufferPool cannot be null");

            this.closeState = CloseState.Open;
            this.exceptionEventType = TraceEventType.Error;
            this.socket = socket;
            this.connectionBufferPool = connectionBufferPool;
            this.readBuffer = this.connectionBufferPool.Take();
            this.asyncReadBufferSize = this.readBuffer.Length;
            this.socket.SendBufferSize = this.socket.ReceiveBufferSize = this.asyncReadBufferSize;
            this.sendTimeout = this.receiveTimeout = TimeSpan.MaxValue;

            this.remoteEndpoint = null;

            if (autoBindToCompletionPort)
            {
                this.socket.UseOnlyOverlappedIO = false;
            }

            // In SMSvcHost, sockets must be duplicated to the target process. Binding a handle to a completion port
            // prevents any duplicated handle from ever binding to a completion port. The target process is where we
            // want to use completion ports for performance. This means that in SMSvcHost, socket.UseOnlyOverlappedIO
            // must be set to true to prevent completion port use.
            if (this.socket.UseOnlyOverlappedIO)
            {
                // Init BeginRead state
                if (onReceiveCompleted == null)
                {
                    onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(OnReceiveCompleted));
                }
            }

            this.TraceSocketInfo(socket, TraceCode.SocketConnectionCreate, SR.TraceCodeSocketConnectionCreate, null);
        }
        public int AsyncReadBufferSize
        {
            get { return asyncReadBufferSize; }
        }

        public byte[] AsyncReadBuffer
        {
            get
            {
                return readBuffer;
            }
        }

        object ThisLock
        {
            get { return this; }
        }

        public TraceEventType ExceptionEventType
        {
            get { return this.exceptionEventType; }
            set { this.exceptionEventType = value; }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get
            {
                // this property should only be called on the receive path
                if (remoteEndpoint == null && this.closeState == CloseState.Open)
                {
                    try
                    {
                        remoteEndpoint = (IPEndPoint)socket.RemoteEndPoint;
                    }
                    catch (SocketException socketException)
                    {
                        // will never be a timeout error, so TimeSpan.Zero is ok
#pragma warning suppress 56503 // Called from Receive path, SocketConnection cannot allow a SocketException to escape.
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                            ConvertReceiveException(socketException, TimeSpan.Zero), ExceptionEventType);
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        Exception exceptionToThrow = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Undefined);
                        if (object.ReferenceEquals(exceptionToThrow, objectDisposedException))
                        {
#pragma warning suppress 56503 // rethrow
                            throw;
                        }
                        else
                        {
#pragma warning suppress 56503 // Called from Receive path, SocketConnection must convert ObjectDisposedException properly.
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
                        }
                    }
                }

                return remoteEndpoint;
            }
        }

        IOThreadTimer SendTimer
        {
            get
            {
                if (this.sendTimer == null)
                {
                    if (onSendTimeout == null)
                    {
                        onSendTimeout = new Action<object>(OnSendTimeout);
                    }

                    this.sendTimer = new IOThreadTimer(onSendTimeout, this, false);
                }

                return this.sendTimer;
            }
        }

        IOThreadTimer ReceiveTimer
        {
            get
            {
                if (this.receiveTimer == null)
                {
                    if (onReceiveTimeout == null)
                    {
                        onReceiveTimeout = new Action<object>(OnReceiveTimeout);
                    }

                    this.receiveTimer = new IOThreadTimer(onReceiveTimeout, this, false);
                }

                return this.receiveTimer;
            }
        }


        string RemoteEndpointAddress
        {
            get
            {
                if (remoteEndpointAddress == null)
                {
                    try
                    {
                        IPEndPoint local, remote;
                        if (TryGetEndpoints(out local, out remote))
                        {
                            this.remoteEndpointAddress = TraceUtility.GetRemoteEndpointAddressPort(remote);
                        }
                        else
                        {
                            //null indicates not initialized.
                            remoteEndpointAddress = string.Empty;
                        }
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }

                    }
                }
                return remoteEndpointAddress;
            }
        }

        static void OnReceiveTimeout(object state)
        {
            SocketConnection thisPtr = (SocketConnection)state;
            thisPtr.Abort(SR.GetString(SR.SocketAbortedReceiveTimedOut, thisPtr.receiveTimeout), TransferOperation.Read);
        }

        static void OnSendTimeout(object state)
        {
            SocketConnection thisPtr = (SocketConnection)state;
            thisPtr.Abort(TraceEventType.Warning,
                SR.GetString(SR.SocketAbortedSendTimedOut, thisPtr.sendTimeout), TransferOperation.Write);
        }

        static void OnReceiveCompleted(IAsyncResult result)
        {
            ((SocketConnection)result.AsyncState).OnReceive(result);
        }

        static void OnReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ((SocketConnection)e.UserToken).OnReceiveAsync(sender, e);
        }

        static void OnSendAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            ((SocketConnection)e.UserToken).OnSendAsync(sender, e);
        }

        public void Abort()
        {
            Abort(null, TransferOperation.Undefined);
        }

        void Abort(string timeoutErrorString, TransferOperation transferOperation)
        {
            TraceEventType traceEventType = TraceEventType.Warning;

            // we could be timing out a cached connection
            if (this.ExceptionEventType == TraceEventType.Information)
            {
                traceEventType = this.ExceptionEventType;
            }

            Abort(traceEventType, timeoutErrorString, transferOperation);
        }

        void Abort(TraceEventType traceEventType)
        {
            Abort(traceEventType, null, TransferOperation.Undefined);
        }

        void Abort(TraceEventType traceEventType, string timeoutErrorString, TransferOperation transferOperation)
        {
            if (TD.SocketConnectionAbortIsEnabled())
            {
                TD.SocketConnectionAbort(this.socket.GetHashCode());
            }
            lock (ThisLock)
            {
                if (closeState == CloseState.Closed)
                {
                    return;
                }

                this.timeoutErrorString = timeoutErrorString;
                this.timeoutErrorTransferOperation = transferOperation;
                aborted = true;
                closeState = CloseState.Closed;

                if (this.asyncReadPending)
                {
                    CancelReceiveTimer();
                }
                else
                {
                    this.DisposeReadEventArgs();
                }

                if (this.asyncWritePending)
                {
                    CancelSendTimer();
                }
                else
                {
                    this.DisposeWriteEventArgs();
                }
            }

            if (DiagnosticUtility.ShouldTrace(traceEventType))
            {
                TraceUtility.TraceEvent(traceEventType, TraceCode.SocketConnectionAbort,
                    SR.GetString(SR.TraceCodeSocketConnectionAbort), this);
            }

            socket.Close(0);
        }

        void AbortRead()
        {
            lock (ThisLock)
            {
                if (this.asyncReadPending)
                {
                    if (closeState != CloseState.Closed)
                    {
                        this.SetUserToken(this.asyncReadEventArgs, null);
                        this.asyncReadPending = false;
                        CancelReceiveTimer();
                    }
                    else
                    {
                        this.DisposeReadEventArgs();
                    }
                }
            }
        }

        void CancelReceiveTimer()
        {
            // CSDMain 34539: Snapshot the timer so that we don't null ref if there is a ----
            // between calls to CancelReceiveTimer (e.g., Abort, AsyncReadCallback)

            IOThreadTimer receiveTimerSnapshot = this.receiveTimer;
            this.receiveTimer = null;

            if (receiveTimerSnapshot != null)
            {
                receiveTimerSnapshot.Cancel();
            }
        }

        void CancelSendTimer()
        {
            IOThreadTimer sendTimerSnapshot = this.sendTimer;
            this.sendTimer = null;

            if (sendTimerSnapshot != null)
            {
                sendTimerSnapshot.Cancel();
            }
        }

        void CloseAsyncAndLinger()
        {
            readFinTimeout = closeTimeoutHelper.RemainingTime();

            try
            {
                if (BeginReadCore(0, 1, readFinTimeout, onWaitForFinComplete, this) == AsyncCompletionResult.Queued)
                {
                    return;
                }

                int bytesRead = EndRead();

                if (bytesRead > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        new CommunicationException(SR.GetString(SR.SocketCloseReadReceivedData, socket.RemoteEndPoint)),
                        ExceptionEventType);
                }
            }
            catch (TimeoutException timeoutException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(
                    SR.GetString(SR.SocketCloseReadTimeout, socket.RemoteEndPoint, readFinTimeout), timeoutException),
                    ExceptionEventType);
            }

            ContinueClose(closeTimeoutHelper.RemainingTime());
        }

        static void OnWaitForFinComplete(object state)
        {
            SocketConnection thisPtr = (SocketConnection)state;

            try
            {
                int bytesRead;

                try
                {
                    bytesRead = thisPtr.EndRead();

                    if (bytesRead > 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                            new CommunicationException(SR.GetString(SR.SocketCloseReadReceivedData, thisPtr.socket.RemoteEndPoint)),
                            thisPtr.ExceptionEventType);
                    }
                }
                catch (TimeoutException timeoutException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(
                        SR.GetString(SR.SocketCloseReadTimeout, thisPtr.socket.RemoteEndPoint, thisPtr.readFinTimeout),
                        timeoutException), thisPtr.ExceptionEventType);
                }

                thisPtr.ContinueClose(thisPtr.closeTimeoutHelper.RemainingTime());
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);

                // The user has no opportunity to clean up the connection in the async and linger
                // code path, ensure cleanup finishes.
                thisPtr.Abort();
            }
        }

        public void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            lock (ThisLock)
            {
                if (closeState == CloseState.Closing || closeState == CloseState.Closed)
                {
                    // already closing or closed, so just return
                    return;
                }
                this.TraceSocketInfo(this.socket, TraceCode.SocketConnectionClose, SR.TraceCodeSocketConnectionClose, timeout.ToString());
                closeState = CloseState.Closing;
            }

            // first we shutdown our send-side
            closeTimeoutHelper = new TimeoutHelper(timeout);
            Shutdown(closeTimeoutHelper.RemainingTime());

            if (asyncAndLinger)
            {
                CloseAsyncAndLinger();
            }
            else
            {
                CloseSync();
            }
        }

        void CloseSync()
        {
            byte[] dummy = new byte[1];

            // then we check for a FIN from the other side (i.e. read zero)
            int bytesRead;
            readFinTimeout = closeTimeoutHelper.RemainingTime();

            try
            {
                bytesRead = ReadCore(dummy, 0, 1, readFinTimeout, true);

                if (bytesRead > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        new CommunicationException(SR.GetString(SR.SocketCloseReadReceivedData, socket.RemoteEndPoint)), ExceptionEventType);
                }
            }
            catch (TimeoutException timeoutException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(
                    SR.GetString(SR.SocketCloseReadTimeout, socket.RemoteEndPoint, readFinTimeout), timeoutException), ExceptionEventType);
            }

            // finally we call Close with whatever time is remaining
            ContinueClose(closeTimeoutHelper.RemainingTime());
        }

        public void ContinueClose(TimeSpan timeout)
        {
            // trace if we're effectively aborting
            if (timeout <= TimeSpan.Zero && DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.SocketConnectionAbortClose,
                    SR.GetString(SR.TraceCodeSocketConnectionAbortClose), this);
            }

            socket.Close(TimeoutHelper.ToMilliseconds(timeout));

            lock (ThisLock)
            {
                // Abort could have been called on a separate thread and cleaned up 
                // our buffers/completion here
                if (this.closeState != CloseState.Closed)
                {
                    if (!this.asyncReadPending)
                    {
                        this.DisposeReadEventArgs();
                    }

                    if (!this.asyncWritePending)
                    {
                        this.DisposeWriteEventArgs();
                    }
                }

                closeState = CloseState.Closed;
            }
        }

        public void Shutdown(TimeSpan timeout)
        {
            lock (ThisLock)
            {
                if (isShutdown)
                {
                    return;
                }

                isShutdown = true;
            }

            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertSendException(socketException, TimeSpan.MaxValue), ExceptionEventType);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Exception exceptionToThrow = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Undefined);
                if (object.ReferenceEquals(exceptionToThrow, objectDisposedException))
                {
                    throw;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
                }
            }
        }

        void ThrowIfNotOpen()
        {
            if (closeState == CloseState.Closing || closeState == CloseState.Closed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertObjectDisposedException(new ObjectDisposedException(
                    this.GetType().ToString(), SR.GetString(SR.SocketConnectionDisposed)), TransferOperation.Undefined), ExceptionEventType);
            }
        }

        void ThrowIfClosed()
        {
            if (closeState == CloseState.Closed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertObjectDisposedException(new ObjectDisposedException(
                    this.GetType().ToString(), SR.GetString(SR.SocketConnectionDisposed)), TransferOperation.Undefined), ExceptionEventType);
            }
        }

        void TraceSocketInfo(Socket socket, int traceCode, string srString, string timeoutString)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> values = new Dictionary<string, string>(4);
                values["State"] = this.closeState.ToString();

                if (timeoutString != null)
                {
                    values["Timeout"] = timeoutString;
                }

                if (socket != null && this.closeState != CloseState.Closing)
                {
                    if (socket.LocalEndPoint != null)
                    {
                        values["LocalEndpoint"] = socket.LocalEndPoint.ToString();
                    }
                    if (socket.RemoteEndPoint != null)
                    {
                        values["RemoteEndPoint"] = socket.RemoteEndPoint.ToString();
                    }
                }
                TraceUtility.TraceEvent(TraceEventType.Information, traceCode, SR.GetString(srString), new DictionaryTraceRecord(values), this, null);
            }
        }

        bool TryGetEndpoints(out IPEndPoint localIPEndpoint, out IPEndPoint remoteIPEndpoint)
        {
            localIPEndpoint = null;
            remoteIPEndpoint = null;

            if (this.closeState == CloseState.Open)
            {
                try
                {
                    remoteIPEndpoint = this.remoteEndpoint ?? (IPEndPoint)this.socket.RemoteEndPoint;
                    localIPEndpoint = (IPEndPoint)this.socket.LocalEndPoint;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
            }

            return localIPEndpoint != null && remoteIPEndpoint != null;
        }

        public object DuplicateAndClose(int targetProcessId)
        {
            object result = socket.DuplicateAndClose(targetProcessId);
            this.Abort(TraceEventType.Information);
            return result;
        }

        public object GetCoreTransport()
        {
            return socket;
        }

        public IAsyncResult BeginValidate(Uri uri, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<bool>(true, callback, state);
        }

        public bool EndValidate(IAsyncResult result)
        {
            return CompletedAsyncResult<bool>.End(result);
        }

        Exception ConvertSendException(SocketException socketException, TimeSpan remainingTime)
        {
            return ConvertTransferException(socketException, this.sendTimeout, socketException,
                TransferOperation.Write, this.aborted, this.timeoutErrorString, this.timeoutErrorTransferOperation, this, remainingTime);
        }

        Exception ConvertReceiveException(SocketException socketException, TimeSpan remainingTime)
        {
            return ConvertTransferException(socketException, this.receiveTimeout, socketException,
                TransferOperation.Read, this.aborted, this.timeoutErrorString, this.timeoutErrorTransferOperation, this, remainingTime);
        }

        internal static Exception ConvertTransferException(SocketException socketException, TimeSpan timeout, Exception originalException)
        {
            return ConvertTransferException(socketException, timeout, originalException,
                TransferOperation.Undefined, false, null, TransferOperation.Undefined, null, TimeSpan.MaxValue);
        }

        Exception ConvertObjectDisposedException(ObjectDisposedException originalException, TransferOperation transferOperation)
        {
            if (this.timeoutErrorString != null)
            {
                return ConvertTimeoutErrorException(originalException, transferOperation, this.timeoutErrorString, this.timeoutErrorTransferOperation);
            }
            else if (this.aborted)
            {
                return new CommunicationObjectAbortedException(SR.GetString(SR.SocketConnectionDisposed), originalException);
            }
            else
            {
                return originalException;
            }
        }

        static Exception ConvertTransferException(SocketException socketException, TimeSpan timeout, Exception originalException,
            TransferOperation transferOperation, bool aborted, string timeoutErrorString, TransferOperation timeoutErrorTransferOperation,
            SocketConnection socketConnection, TimeSpan remainingTime)
        {
            if (socketException.ErrorCode == UnsafeNativeMethods.ERROR_INVALID_HANDLE)
            {
                return new CommunicationObjectAbortedException(socketException.Message, socketException);
            }

            if (timeoutErrorString != null)
            {
                return ConvertTimeoutErrorException(originalException, transferOperation, timeoutErrorString, timeoutErrorTransferOperation);
            }

            TraceEventType exceptionEventType = socketConnection == null ? TraceEventType.Error : socketConnection.ExceptionEventType;

            // 10053 can occur due to our timeout sockopt firing, so map to TimeoutException in that case
            if (socketException.ErrorCode == UnsafeNativeMethods.WSAECONNABORTED &&
                remainingTime <= TimeSpan.Zero)
            {
                TimeoutException timeoutException = new TimeoutException(SR.GetString(SR.TcpConnectionTimedOut, timeout), originalException);
                if (TD.TcpConnectionTimedOutIsEnabled())
                {
                    if (socketConnection != null)
                    {
                        int socketid = (socketConnection != null && socketConnection.socket != null) ? socketConnection.socket.GetHashCode() : -1;
                        TD.TcpConnectionTimedOut(socketid, socketConnection.RemoteEndpointAddress);
                    }
                }
                if (DiagnosticUtility.ShouldTrace(exceptionEventType))
                {
                    TraceUtility.TraceEvent(exceptionEventType, TraceCode.TcpConnectionTimedOut, GetEndpointString(SR.TcpConnectionTimedOut, timeout, null, socketConnection), timeoutException, null);
                }
                return timeoutException;
            }

            if (socketException.ErrorCode == UnsafeNativeMethods.WSAENETRESET ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAECONNABORTED ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAECONNRESET)
            {
                if (aborted)
                {
                    return new CommunicationObjectAbortedException(SR.GetString(SR.TcpLocalConnectionAborted), originalException);
                }
                else
                {
                    CommunicationException communicationException = new CommunicationException(SR.GetString(SR.TcpConnectionResetError, timeout), originalException);
                    if (TD.TcpConnectionResetErrorIsEnabled())
                    {
                        if (socketConnection != null)
                        {
                            int socketId = (socketConnection.socket != null) ? socketConnection.socket.GetHashCode() : -1;
                            TD.TcpConnectionResetError(socketId, socketConnection.RemoteEndpointAddress);
                        }
                    }
                    if (DiagnosticUtility.ShouldTrace(exceptionEventType))
                    {
                        TraceUtility.TraceEvent(exceptionEventType, TraceCode.TcpConnectionResetError, GetEndpointString(SR.TcpConnectionResetError, timeout, null, socketConnection), communicationException, null);
                    }
                    return communicationException;
                }
            }
            else if (socketException.ErrorCode == UnsafeNativeMethods.WSAETIMEDOUT)
            {
                TimeoutException timeoutException = new TimeoutException(SR.GetString(SR.TcpConnectionTimedOut, timeout), originalException);
                if (DiagnosticUtility.ShouldTrace(exceptionEventType))
                {
                    TraceUtility.TraceEvent(exceptionEventType, TraceCode.TcpConnectionTimedOut, GetEndpointString(SR.TcpConnectionTimedOut, timeout, null, socketConnection), timeoutException, null);
                }
                return timeoutException;
            }
            else
            {
                if (aborted)
                {
                    return new CommunicationObjectAbortedException(SR.GetString(SR.TcpTransferError, socketException.ErrorCode, socketException.Message), originalException);
                }
                else
                {
                    CommunicationException communicationException = new CommunicationException(SR.GetString(SR.TcpTransferError, socketException.ErrorCode, socketException.Message), originalException);
                    if (DiagnosticUtility.ShouldTrace(exceptionEventType))
                    {
                        TraceUtility.TraceEvent(exceptionEventType, TraceCode.TcpTransferError, GetEndpointString(SR.TcpTransferError, TimeSpan.MinValue, socketException, socketConnection), communicationException, null);
                    }
                    return communicationException;
                }
            }
        }

        static Exception ConvertTimeoutErrorException(Exception originalException,
            TransferOperation transferOperation, string timeoutErrorString, TransferOperation timeoutErrorTransferOperation)
        {
            if (timeoutErrorString == null)
            {
                Fx.Assert("Argument timeoutErrorString must not be null.");
            }

            if (transferOperation == timeoutErrorTransferOperation)
            {
                return new TimeoutException(timeoutErrorString, originalException);
            }
            else
            {
                return new CommunicationException(timeoutErrorString, originalException);
            }
        }

        static string GetEndpointString(string sr, TimeSpan timeout, SocketException socketException, SocketConnection socketConnection)
        {
            IPEndPoint remoteEndpoint = null;
            IPEndPoint localEndpoint = null;
            bool haveEndpoints = socketConnection != null && socketConnection.TryGetEndpoints(out localEndpoint, out remoteEndpoint);

            if (string.Compare(sr, SR.TcpConnectionTimedOut, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return haveEndpoints
                    ? SR.GetString(SR.TcpConnectionTimedOutWithIP, timeout, localEndpoint, remoteEndpoint)
                    : SR.GetString(SR.TcpConnectionTimedOut, timeout);
            }
            else if (string.Compare(sr, SR.TcpConnectionResetError, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return haveEndpoints
                    ? SR.GetString(SR.TcpConnectionResetErrorWithIP, timeout, localEndpoint, remoteEndpoint)
                    : SR.GetString(SR.TcpConnectionResetError, timeout);
            }
            else
            {
                // sr == SR.TcpTransferError
                return haveEndpoints
                    ? SR.GetString(SR.TcpTransferErrorWithIP, socketException.ErrorCode, socketException.Message, localEndpoint, remoteEndpoint)
                    : SR.GetString(SR.TcpTransferError, socketException.ErrorCode, socketException.Message);
            }
        }

        public AsyncCompletionResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            bool abortWrite = true;

            try
            {
                if (TD.SocketAsyncWriteStartIsEnabled())
                {
                    TraceWriteStart(size, true);
                }

                lock (ThisLock)
                {
                    Fx.Assert(!this.asyncWritePending, "Called BeginWrite twice.");
                    this.ThrowIfClosed();
                    this.EnsureWriteEventArgs();
                    SetImmediate(immediate);
                    SetWriteTimeout(timeout, false);
                    this.SetUserToken(this.asyncWriteEventArgs, this);
                    this.asyncWritePending = true;
                    this.asyncWriteCallback = callback;
                    this.asyncWriteState = state;
                }

                this.asyncWriteEventArgs.SetBuffer(buffer, offset, size);

                if (socket.SendAsync(this.asyncWriteEventArgs))
                {
                    abortWrite = false;
                    return AsyncCompletionResult.Queued;
                }

                this.HandleSendAsyncCompleted();
                abortWrite = false;
                return AsyncCompletionResult.Completed;
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertSendException(socketException, TimeSpan.MaxValue), ExceptionEventType);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Exception exceptionToThrow = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Write);
                if (object.ReferenceEquals(exceptionToThrow, objectDisposedException))
                {
                    throw;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
                }
            }
            finally
            {
                if (abortWrite)
                {
                    this.AbortWrite();
                }
            }
        }

        public void EndWrite()
        {
            if (this.asyncWriteException != null)
            {
                this.AbortWrite();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.asyncWriteException, ExceptionEventType);
            }

            lock (ThisLock)
            {
                if (!this.asyncWritePending)
                {
                    throw Fx.AssertAndThrow("SocketConnection.EndWrite called with no write pending.");
                }

                this.SetUserToken(this.asyncWriteEventArgs, null);
                this.asyncWritePending = false;                

                if (this.closeState == CloseState.Closed)
                {
                    this.DisposeWriteEventArgs();
                }
            }
        }

        void OnSendAsync(object sender, SocketAsyncEventArgs eventArgs)
        {
            Fx.Assert(eventArgs != null, "Argument 'eventArgs' cannot be NULL.");
            this.CancelSendTimer();

            try
            {
                this.HandleSendAsyncCompleted();
                Fx.Assert(eventArgs.BytesTransferred == this.asyncWriteEventArgs.Count, "The socket SendAsync did not send all the bytes.");
            }
            catch (SocketException socketException)
            {
                this.asyncWriteException = ConvertSendException(socketException, TimeSpan.MaxValue);
            }
#pragma warning suppress 56500 // [....], transferring exception to caller
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                this.asyncWriteException = exception;
            }

            this.FinishWrite();
        }

        void HandleSendAsyncCompleted()
        {
            if (this.asyncWriteEventArgs.SocketError == SocketError.Success)
            {
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException((int)this.asyncWriteEventArgs.SocketError));
        }

        // This method should be called inside ThisLock
        void DisposeWriteEventArgs()
        {
            if (this.asyncWriteEventArgs != null)
            {
                this.asyncWriteEventArgs.Completed -= onSocketSendCompleted;
                this.asyncWriteEventArgs.Dispose();
            }
        }

        void AbortWrite()
        {
            lock (ThisLock)
            {
                if (this.asyncWritePending)
                {
                    if (this.closeState != CloseState.Closed)
                    {
                        this.SetUserToken(this.asyncWriteEventArgs, null);
                        this.asyncWritePending = false;
                        this.CancelSendTimer();
                    }
                    else
                    {
                        this.DisposeWriteEventArgs();
                    }
                }
            }
        }

        void FinishWrite()
        {
            WaitCallback asyncWriteCallback = this.asyncWriteCallback;
            object asyncWriteState = this.asyncWriteState;

            this.asyncWriteState = null;
            this.asyncWriteCallback = null;

            asyncWriteCallback(asyncWriteState);
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            // as per http://support.microsoft.com/default.aspx?scid=kb%3ben-us%3b201213
            // we shouldn't write more than 64K synchronously to a socket
            const int maxSocketWrite = 64 * 1024;

            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);

            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            try
            {
                if (TD.SocketWriteStartIsEnabled())
                {
                    TraceWriteStart(size, false);
                }

                SetImmediate(immediate);
                int bytesToWrite = size;

                while (bytesToWrite > 0)
                {
                    SetWriteTimeout(timeoutHelper.RemainingTime(), true);
                    size = Math.Min(bytesToWrite, maxSocketWrite);
                    socket.Send(buffer, offset, size, SocketFlags.None);
                    bytesToWrite -= size;
                    offset += size;
                    timeout = timeoutHelper.RemainingTime();
                }
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertSendException(socketException, timeoutHelper.RemainingTime()), ExceptionEventType);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Exception exceptionToThrow = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Write);
                if (object.ReferenceEquals(exceptionToThrow, objectDisposedException))
                {
                    throw;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
                }
            }
        }

        void TraceWriteStart(int size, bool async)
        {
            if (!async)
            {
                TD.SocketWriteStart(this.socket.GetHashCode(), size, this.RemoteEndpointAddress);
            }
            else
            {
                TD.SocketAsyncWriteStart(this.socket.GetHashCode(), size, this.RemoteEndpointAddress);
            }
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            try
            {
                Write(buffer, offset, size, immediate, timeout);
            }
            finally
            {
                bufferManager.ReturnBuffer(buffer);
            }
        }

        public int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            ThrowIfNotOpen();
            return ReadCore(buffer, offset, size, timeout, false);
        }

        int ReadCore(byte[] buffer, int offset, int size, TimeSpan timeout, bool closing)
        {
            int bytesRead = 0;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            try
            {
                SetReadTimeout(timeoutHelper.RemainingTime(), true, closing);
                bytesRead = socket.Receive(buffer, offset, size, SocketFlags.None);

                if (TD.SocketReadStopIsEnabled())
                {
                    TraceSocketReadStop(bytesRead, false);
                }
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                    ConvertReceiveException(socketException, timeoutHelper.RemainingTime()), ExceptionEventType);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Exception exceptionToThrow = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Read);
                if (object.ReferenceEquals(exceptionToThrow, objectDisposedException))
                {
                    throw;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
                }
            }

            return bytesRead;
        }

        private void TraceSocketReadStop(int bytesRead, bool async)
        {
            if (!async)
            {
                TD.SocketReadStop((this.socket != null) ? this.socket.GetHashCode() : -1, bytesRead, this.RemoteEndpointAddress);
            }
            else
            {
                TD.SocketAsyncReadStop((this.socket != null) ? this.socket.GetHashCode() : -1, bytesRead, this.RemoteEndpointAddress);
            }
        }

        public virtual AsyncCompletionResult BeginRead(int offset, int size, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            ConnectionUtilities.ValidateBufferBounds(AsyncReadBufferSize, offset, size);
            this.ThrowIfNotOpen();
            return this.BeginReadCore(offset, size, timeout, callback, state);
        }

        AsyncCompletionResult BeginReadCore(int offset, int size, TimeSpan timeout,
            WaitCallback callback, object state)
        {
            bool abortRead = true;

            lock (ThisLock)
            {
                this.ThrowIfClosed();
                this.EnsureReadEventArgs();
                this.asyncReadState = state;
                this.asyncReadCallback = callback;
                this.SetUserToken(this.asyncReadEventArgs, this);
                this.asyncReadPending = true;
                this.SetReadTimeout(timeout, false, false);
            }

            try
            {
                if (socket.UseOnlyOverlappedIO)
                {
                    // ReceiveAsync does not respect UseOnlyOverlappedIO but BeginReceive does.
                    IAsyncResult result = socket.BeginReceive(AsyncReadBuffer, offset, size, SocketFlags.None, onReceiveCompleted, this);

                    if (!result.CompletedSynchronously)
                    {
                        abortRead = false;
                        return AsyncCompletionResult.Queued;
                    }

                    asyncReadSize = socket.EndReceive(result);
                }
                else
                {
                    if (offset != this.asyncReadEventArgs.Offset ||
                        size != this.asyncReadEventArgs.Count)
                    {
                        this.asyncReadEventArgs.SetBuffer(offset, size);
                    }

                    if (this.ReceiveAsync())
                    {
                        abortRead = false;
                        return AsyncCompletionResult.Queued;
                    }

                    this.HandleReceiveAsyncCompleted();
                    this.asyncReadSize = this.asyncReadEventArgs.BytesTransferred;
                }

                if (TD.SocketReadStopIsEnabled())
                {
                    TraceSocketReadStop(asyncReadSize, true);
                }

                abortRead = false;
                return AsyncCompletionResult.Completed;
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(ConvertReceiveException(socketException, TimeSpan.MaxValue), ExceptionEventType);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Exception exceptionToThrow = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Read);
                if (object.ReferenceEquals(exceptionToThrow, objectDisposedException))
                {
                    throw;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exceptionToThrow, ExceptionEventType);
                }
            }
            finally
            {
                if (abortRead)
                {
                    AbortRead();
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses a SecurityCritical method to suppress ExecutionContext flow when running in fullTrust.",
            Safe = "Safe because we're only suppressing the ExecutionContext if we're already in full trust.")]
        [SecuritySafeCritical]
        bool ReceiveAsync()
        {
            if (!PartialTrustHelpers.ShouldFlowSecurityContext)
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    return ReceiveAsyncNoFlow();
                }
            }

            return this.socket.ReceiveAsync(this.asyncReadEventArgs);
        }

        [Fx.Tag.SecurityNote(Critical = "Suppresses execution context flow and restores it after invocation. Fulltrust async callbacks " +
            "will not have an ExecutionContext, LogicalCallcontext or SecurityContext and should not take dependency on them.")]
        [SecurityCritical]
        bool ReceiveAsyncNoFlow()
        {
            using (ExecutionContext.SuppressFlow())
            {
                return this.socket.ReceiveAsync(this.asyncReadEventArgs);
            }
        }

        void OnReceive(IAsyncResult result)
        {
            this.CancelReceiveTimer();
            if (result.CompletedSynchronously)
            {
                return;
            }

            try
            {
                this.asyncReadSize = socket.EndReceive(result);

                if (TD.SocketReadStopIsEnabled())
                {
                    TraceSocketReadStop(this.asyncReadSize, true);
                }
            }
            catch (SocketException socketException)
            {
                this.asyncReadException = ConvertReceiveException(socketException, TimeSpan.MaxValue);
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                this.asyncReadException = ConvertObjectDisposedException(objectDisposedException, TransferOperation.Read);
            }
#pragma warning suppress 56500 // [....], transferring exception to caller
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.asyncReadException = exception;
            }

            this.FinishRead();
        }

        void OnReceiveAsync(object sender, SocketAsyncEventArgs eventArgs)
        {
            Fx.Assert(eventArgs != null, "Argument 'eventArgs' cannot be NULL.");
            this.CancelReceiveTimer();

            try
            {
                this.HandleReceiveAsyncCompleted();
                this.asyncReadSize = eventArgs.BytesTransferred;

                if (TD.SocketReadStopIsEnabled())
                {
                    TraceSocketReadStop(asyncReadSize, true);
                }
            }
            catch (SocketException socketException)
            {
                asyncReadException = ConvertReceiveException(socketException, TimeSpan.MaxValue);
            }
#pragma warning suppress 56500 // [....], transferring exception to caller
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                asyncReadException = exception;
            }

            FinishRead();
        }

        void HandleReceiveAsyncCompleted()
        {
            if (this.asyncReadEventArgs.SocketError == SocketError.Success)
            {
                return;
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException((int)this.asyncReadEventArgs.SocketError));
        }

        void FinishRead()
        {
            WaitCallback asyncReadCallback = this.asyncReadCallback;
            object asyncReadState = this.asyncReadState;

            this.asyncReadState = null;
            this.asyncReadCallback = null;

            asyncReadCallback(asyncReadState);
        }

        // Both BeginRead/ReadAsync paths completed themselves. EndRead's only job is to deliver the result.
        public int EndRead()
        {
            if (this.asyncReadException != null)
            {
                AbortRead();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.asyncReadException, ExceptionEventType);
            }

            lock (ThisLock)
            {
                if (!this.asyncReadPending)
                {
                    throw Fx.AssertAndThrow("SocketConnection.EndRead called with no read pending.");
                }

                this.SetUserToken(this.asyncReadEventArgs, null);
                this.asyncReadPending = false;
                
                if (closeState == CloseState.Closed)
                {
                    this.DisposeReadEventArgs();
                }
            }

            return this.asyncReadSize;
        }

        // This method should be called inside ThisLock
        void DisposeReadEventArgs()
        {
            if (this.asyncReadEventArgs != null)
            {
                this.asyncReadEventArgs.Completed -= onReceiveAsyncCompleted;
                this.asyncReadEventArgs.Dispose();
            }

            // We release the buffer only if there is no outstanding I/O
            this.TryReturnReadBuffer();
        }

        void TryReturnReadBuffer()
        {
            // The buffer must not be returned and nulled when an abort occurs. Since the buffer
            // is also accessed by higher layers, code that has not yet realized the stack is
            // aborted may be attempting to read from the buffer.
            if (this.readBuffer != null && !this.aborted)
            {
                this.connectionBufferPool.Return(this.readBuffer);
                this.readBuffer = null;
            }
        }

        void SetUserToken(SocketAsyncEventArgs args, object userToken)
        {
            // The socket args can be pinned by the overlapped callback. Ensure SocketConnection is
            // only pinned when there is outstanding IO.
            if (args != null)
            {
                args.UserToken = userToken;
            }
        }

        void SetImmediate(bool immediate)
        {
            if (immediate != this.noDelay)
            {
                lock (ThisLock)
                {
                    ThrowIfNotOpen();
                    socket.NoDelay = immediate;
                }
                this.noDelay = immediate;
            }
        }

        void SetReadTimeout(TimeSpan timeout, bool synchronous, bool closing)
        {
            if (synchronous)
            {
                CancelReceiveTimer();

                // 0 == infinite for winsock timeouts, so we should preempt and throw
                if (timeout <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        new TimeoutException(SR.GetString(SR.TcpConnectionTimedOut, timeout)), ExceptionEventType);
                }

                if (UpdateTimeout(this.receiveTimeout, timeout))
                {
                    lock (ThisLock)
                    {
                        if (!closing || this.closeState != CloseState.Closing)
                        {
                            ThrowIfNotOpen();
                        }
                        this.socket.ReceiveTimeout = TimeoutHelper.ToMilliseconds(timeout);
                    }
                    this.receiveTimeout = timeout;
                }
            }
            else
            {
                this.receiveTimeout = timeout;
                if (timeout == TimeSpan.MaxValue)
                {
                    CancelReceiveTimer();
                }
                else
                {
                    ReceiveTimer.Set(timeout);
                }
            }
        }

        void SetWriteTimeout(TimeSpan timeout, bool synchronous)
        {
            if (synchronous)
            {
                CancelSendTimer();

                // 0 == infinite for winsock timeouts, so we should preempt and throw
                if (timeout <= TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(
                        new TimeoutException(SR.GetString(SR.TcpConnectionTimedOut, timeout)), ExceptionEventType);
                }

                if (UpdateTimeout(this.sendTimeout, timeout))
                {
                    lock (ThisLock)
                    {
                        ThrowIfNotOpen();
                        this.socket.SendTimeout = TimeoutHelper.ToMilliseconds(timeout);
                    }
                    this.sendTimeout = timeout;
                }
            }
            else
            {
                this.sendTimeout = timeout;
                if (timeout == TimeSpan.MaxValue)
                {
                    CancelSendTimer();
                }
                else
                {
                    SendTimer.Set(timeout);
                }
            }
        }

        bool UpdateTimeout(TimeSpan oldTimeout, TimeSpan newTimeout)
        {
            if (oldTimeout == newTimeout)
            {
                return false;
            }

            long threshold = oldTimeout.Ticks / 10;
            long delta = Math.Max(oldTimeout.Ticks, newTimeout.Ticks) - Math.Min(oldTimeout.Ticks, newTimeout.Ticks);

            return delta > threshold;
        }

        // This method should be called inside ThisLock
        void EnsureReadEventArgs()
        {
            if (this.asyncReadEventArgs == null)
            {
                // Init ReadAsync state
                if (onReceiveAsyncCompleted == null)
                {
                    onReceiveAsyncCompleted = new EventHandler<SocketAsyncEventArgs>(OnReceiveAsyncCompleted);
                }

                this.asyncReadEventArgs = new SocketAsyncEventArgs();
                this.asyncReadEventArgs.SetBuffer(this.readBuffer, 0, this.readBuffer.Length);
                this.asyncReadEventArgs.Completed += onReceiveAsyncCompleted;
            }
        }

        // This method should be called inside ThisLock
        void EnsureWriteEventArgs()
        {
            if (this.asyncWriteEventArgs == null)
            {
                // Init SendAsync state
                if (onSocketSendCompleted == null)
                {
                    onSocketSendCompleted = new EventHandler<SocketAsyncEventArgs>(OnSendAsyncCompleted);
                }

                this.asyncWriteEventArgs = new SocketAsyncEventArgs();
                this.asyncWriteEventArgs.Completed += onSocketSendCompleted;
            }
        }

        enum CloseState
        {
            Open,
            Closing,
            Closed,
        }

        enum TransferOperation
        {
            Write,
            Read,
            Undefined,
        }
    }

    class SocketConnectionInitiator : IConnectionInitiator
    {
        int bufferSize;
        ConnectionBufferPool connectionBufferPool;

        public SocketConnectionInitiator(int bufferSize)
        {
            this.bufferSize = bufferSize;
            this.connectionBufferPool = new ConnectionBufferPool(bufferSize);
        }

        IConnection CreateConnection(Socket socket)
        {
            return new SocketConnection(socket, this.connectionBufferPool, false);
        }

        public static Exception ConvertConnectException(SocketException socketException, Uri remoteUri, TimeSpan timeSpent, Exception innerException)
        {
            if (socketException.ErrorCode == UnsafeNativeMethods.ERROR_INVALID_HANDLE)
            {
                return new CommunicationObjectAbortedException(socketException.Message, socketException);
            }

            if (socketException.ErrorCode == UnsafeNativeMethods.WSAEADDRNOTAVAIL ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAECONNREFUSED ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAENETDOWN ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAENETUNREACH ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAEHOSTDOWN ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAEHOSTUNREACH ||
                socketException.ErrorCode == UnsafeNativeMethods.WSAETIMEDOUT)
            {
                if (timeSpent == TimeSpan.MaxValue)
                {
                    return new EndpointNotFoundException(SR.GetString(SR.TcpConnectError, remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message), innerException);
                }
                else
                {
                    return new EndpointNotFoundException(SR.GetString(SR.TcpConnectErrorWithTimeSpan, remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message, timeSpent), innerException);
                }
            }
            else if (socketException.ErrorCode == UnsafeNativeMethods.WSAENOBUFS)
            {
                return new InsufficientMemoryException(SR.GetString(SR.TcpConnectNoBufs), innerException);
            }
            else if (socketException.ErrorCode == UnsafeNativeMethods.ERROR_NOT_ENOUGH_MEMORY ||
                socketException.ErrorCode == UnsafeNativeMethods.ERROR_NO_SYSTEM_RESOURCES ||
                socketException.ErrorCode == UnsafeNativeMethods.ERROR_OUTOFMEMORY)
            {
                return new InsufficientMemoryException(SR.GetString(SR.InsufficentMemory), socketException);
            }
            else
            {
                if (timeSpent == TimeSpan.MaxValue)
                {
                    return new CommunicationException(SR.GetString(SR.TcpConnectError, remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message), innerException);
                }
                else
                {
                    return new CommunicationException(SR.GetString(SR.TcpConnectErrorWithTimeSpan, remoteUri.AbsoluteUri, socketException.ErrorCode, socketException.Message, timeSpent), innerException);
                }
            }
        }

        static IPAddress[] GetIPAddresses(Uri uri)
        {
            if (uri.HostNameType == UriHostNameType.IPv4 ||
                uri.HostNameType == UriHostNameType.IPv6)
            {
                IPAddress ipAddress = IPAddress.Parse(uri.DnsSafeHost);
                return new IPAddress[] { ipAddress };
            }

            IPHostEntry hostEntry = null;

            try
            {
                hostEntry = DnsCache.Resolve(uri);
            }
            catch (SocketException socketException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new EndpointNotFoundException(SR.GetString(SR.UnableToResolveHost, uri.Host), socketException));
            }

            if (hostEntry.AddressList.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new EndpointNotFoundException(SR.GetString(SR.UnableToResolveHost, uri.Host)));
            }

            return hostEntry.AddressList;
        }

        static TimeoutException CreateTimeoutException(Uri uri, TimeSpan timeout, IPAddress[] addresses, int invalidAddressCount,
            SocketException innerException)
        {
            StringBuilder addressStringBuilder = new StringBuilder();
            for (int i = 0; i < invalidAddressCount; i++)
            {
                if (addresses[i] == null)
                {
                    continue;
                }

                if (addressStringBuilder.Length > 0)
                {
                    addressStringBuilder.Append(", ");
                }
                addressStringBuilder.Append(addresses[i].ToString());
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(
                SR.GetString(SR.TcpConnectingToViaTimedOut, uri.AbsoluteUri, timeout.ToString(),
                invalidAddressCount, addresses.Length, addressStringBuilder.ToString()), innerException));
        }

        public IConnection Connect(Uri uri, TimeSpan timeout)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.InitiatingTcpConnection,
                    SR.GetString(SR.TraceCodeInitiatingTcpConnection),
                    new StringTraceRecord("Uri", uri.ToString()), this, null);
            }

            int port = uri.Port;
            IPAddress[] addresses = SocketConnectionInitiator.GetIPAddresses(uri);
            Socket socket = null;
            SocketException lastException = null;

            if (port == -1)
            {
                port = TcpUri.DefaultPort;
            }

            int invalidAddressCount = 0;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            for (int i = 0; i < addresses.Length; i++)
            {
                if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        CreateTimeoutException(uri, timeoutHelper.OriginalTimeout, addresses, invalidAddressCount, lastException));
                }

                AddressFamily addressFamily = addresses[i].AddressFamily;

                if (addressFamily == AddressFamily.InterNetworkV6 && !Socket.OSSupportsIPv6)
                {
                    addresses[i] = null; // disregard for exception attempt purposes
                    continue;
                }

                DateTime connectStartTime = DateTime.UtcNow;
                try
                {
                    socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.Connect(new IPEndPoint(addresses[i], port));
                    lastException = null;
                    break;
                }
                catch (SocketException socketException)
                {
                    invalidAddressCount++;
                    SocketConnectionInitiator.TraceConnectFailure(socket, socketException, uri, DateTime.UtcNow - connectStartTime);
                    lastException = socketException;
                    socket.Close();
                }
            }

            if (socket == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new EndpointNotFoundException(SR.GetString(SR.NoIPEndpointsFoundForHost, uri.Host)));
            }

            if (lastException != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    SocketConnectionInitiator.ConvertConnectException(lastException, uri,
                    timeoutHelper.ElapsedTime(), lastException));
            }

            return CreateConnection(socket);
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.InitiatingTcpConnection,
                    SR.GetString(SR.TraceCodeInitiatingTcpConnection),
                    new StringTraceRecord("Uri", uri.ToString()), this, null);
            }
            return new ConnectAsyncResult(uri, timeout, callback, state);
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            Socket socket = ConnectAsyncResult.End(result);
            return CreateConnection(socket);
        }

        public static void TraceConnectFailure(Socket socket, SocketException socketException, Uri remoteUri,
            TimeSpan timeSpentInConnect)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Exception traceException = ConvertConnectException(socketException, remoteUri, timeSpentInConnect, socketException);
                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.TcpConnectError,
                    SR.GetString(SR.TraceCodeTcpConnectError), socket, traceException);
            }
        }

        class ConnectAsyncResult : AsyncResult
        {
            IPAddress[] addresses;
            int currentIndex;
            int port;
            SocketException lastException;
            TimeSpan timeout;
            TimeoutHelper timeoutHelper;
            int invalidAddressCount;
            DateTime connectStartTime;
            Socket socket;
            Uri uri;
            static Action<object> startConnectCallback;
            static AsyncCallback onConnect = Fx.ThunkCallback(new AsyncCallback(OnConnect));

            public ConnectAsyncResult(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.uri = uri;
                addresses = SocketConnectionInitiator.GetIPAddresses(uri);
                port = uri.Port;
                if (port == -1)
                {
                    port = TcpUri.DefaultPort;
                }

                currentIndex = 0;
                this.timeout = timeout;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (Thread.CurrentThread.IsThreadPoolThread)
                {
                    if (StartConnect())
                    {
                        base.Complete(true);
                    }
                }
                else
                {
                    // If we're not on a threadpool thread, then we need to post a callback to start our accepting loop
                    // Otherwise if the calling thread aborts then the async I/O will get inadvertantly cancelled
                    if (startConnectCallback == null)
                    {
                        startConnectCallback = StartConnectCallback;
                    }

                    ActionItem.Schedule(startConnectCallback, this);
                }
            }

            static void StartConnectCallback(object state)
            {
                ConnectAsyncResult connectAsyncResult = (ConnectAsyncResult)state;
                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = connectAsyncResult.StartConnect();
                }
#pragma warning suppress 56500 // covered by FxCOP
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
                    connectAsyncResult.Complete(false, completionException);
                }
            }

            bool StartConnect()
            {
                while (currentIndex < addresses.Length)
                {
                    if (timeoutHelper.RemainingTime() == TimeSpan.Zero)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            CreateTimeoutException(uri, timeoutHelper.OriginalTimeout, addresses, invalidAddressCount, lastException));
                    }

                    AddressFamily addressFamily = addresses[currentIndex].AddressFamily;

                    if (addressFamily == AddressFamily.InterNetworkV6 && !Socket.OSSupportsIPv6)
                    {
                        addresses[currentIndex++] = null; // disregard for exception attempt purposes
                        continue;
                    }

                    this.connectStartTime = DateTime.UtcNow;
                    try
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(addresses[currentIndex], port);
                        this.socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
                        IAsyncResult result = socket.BeginConnect(ipEndPoint, onConnect, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }

                        socket.EndConnect(result);
                        return true;
                    }
                    catch (SocketException socketException)
                    {
                        invalidAddressCount++;
                        this.TraceConnectFailure(socketException);
                        lastException = socketException;
                        currentIndex++;
                    }
                }

                if (socket == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new EndpointNotFoundException(SR.GetString(SR.NoIPEndpointsFoundForHost, uri.Host)));
                }

                Fx.Assert(lastException != null, "StartConnect: Can't get here without an exception.");
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    SocketConnectionInitiator.ConvertConnectException(lastException, uri,
                    timeoutHelper.ElapsedTime(), lastException));
            }

            void TraceConnectFailure(SocketException exception)
            {
                SocketConnectionInitiator.TraceConnectFailure(this.socket, exception, uri, DateTime.UtcNow - connectStartTime);
                this.socket.Close();
            }

            static void OnConnect(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                bool completeSelf = false;
                Exception completionException = null;
                ConnectAsyncResult thisPtr = (ConnectAsyncResult)result.AsyncState;
                try
                {
                    thisPtr.socket.EndConnect(result);
                    completeSelf = true;
                }
                catch (SocketException socketException)
                {
                    thisPtr.TraceConnectFailure(socketException);
                    thisPtr.lastException = socketException;
                    thisPtr.currentIndex++;
                    try
                    {
                        completeSelf = thisPtr.StartConnect();
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
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            public static Socket End(IAsyncResult result)
            {
                ConnectAsyncResult thisPtr = AsyncResult.End<ConnectAsyncResult>(result);
                return thisPtr.socket;
            }
        }
    }

    internal interface ISocketListenerSettings
    {
        int BufferSize { get; }
        bool TeredoEnabled { get; }
        int ListenBacklog { get; }
    }

    class SocketConnectionListener : IConnectionListener
    {
        IPEndPoint localEndpoint;
        bool isDisposed;
        bool isListening;
        Socket listenSocket;
        ISocketListenerSettings settings;
        bool useOnlyOverlappedIO;
        ConnectionBufferPool connectionBufferPool;
        SocketAsyncEventArgsPool socketAsyncEventArgsPool;

        public SocketConnectionListener(Socket listenSocket, ISocketListenerSettings settings, bool useOnlyOverlappedIO)
            : this(settings, useOnlyOverlappedIO)
        {
            this.listenSocket = listenSocket;
        }

        public SocketConnectionListener(IPEndPoint localEndpoint, ISocketListenerSettings settings, bool useOnlyOverlappedIO)
            : this(settings, useOnlyOverlappedIO)
        {
            this.localEndpoint = localEndpoint;
        }

        SocketConnectionListener(ISocketListenerSettings settings, bool useOnlyOverlappedIO)
        {
            Fx.Assert(settings != null, "Input settings should not be null");
            this.settings = settings;
            this.useOnlyOverlappedIO = useOnlyOverlappedIO;
            this.connectionBufferPool = new ConnectionBufferPool(settings.BufferSize);
        }

        object ThisLock
        {
            get { return this; }
        }

        public IAsyncResult BeginAccept(AsyncCallback callback, object state)
        {
            return new AcceptAsyncResult(this, callback, state);
        }

        SocketAsyncEventArgs TakeSocketAsyncEventArgs()
        {
            return this.socketAsyncEventArgsPool.Take();
        }

        void ReturnSocketAsyncEventArgs(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            Fx.Assert(socketAsyncEventArgsPool != null, "The socketAsyncEventArgsPool should not be null");
            this.socketAsyncEventArgsPool.Return(socketAsyncEventArgs);
        }

        // This is the buffer size that is used by the System.Net for accepting new connections
        static int GetAcceptBufferSize(Socket listenSocket)
        {
            return (listenSocket.LocalEndPoint.Serialize().Size + 16) * 2;
        }

        bool InternalBeginAccept(Func<Socket, bool> acceptAsyncFunc)
        {
            lock (ThisLock)
            {
                if (isDisposed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString(), SR.GetString(SR.SocketListenerDisposed)));
                }

                if (!isListening)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SocketListenerNotListening)));
                }

                return acceptAsyncFunc(listenSocket);
            }
        }

        public IConnection EndAccept(IAsyncResult result)
        {
            Socket socket = AcceptAsyncResult.End(result);

            if (socket == null)
                return null;

            if (useOnlyOverlappedIO)
            {
                socket.UseOnlyOverlappedIO = true;
            }
            return new SocketConnection(socket, this.connectionBufferPool, false);
        }

        public void Dispose()
        {
            lock (ThisLock)
            {
                if (!isDisposed)
                {
                    if (listenSocket != null)
                    {
                        listenSocket.Close();
                    }

                    if (this.socketAsyncEventArgsPool != null)
                    {
                        this.socketAsyncEventArgsPool.Close();
                    }

                    isDisposed = true;
                }
            }
        }


        public void Listen()
        {
            // If you call listen() on a port, then kill the process, then immediately start a new process and 
            // try to listen() on the same port, you sometimes get WSAEADDRINUSE.  Even if nothing was accepted.  
            // Ports don't immediately free themselves on process shutdown.  We call listen() in a loop on a delay 
            // for a few iterations for this reason. 
            //
            TimeSpan listenTimeout = TimeSpan.FromSeconds(1);
            BackoffTimeoutHelper backoffHelper = new BackoffTimeoutHelper(listenTimeout);

            lock (ThisLock)
            {
                if (this.listenSocket != null)
                {
                    this.listenSocket.Listen(settings.ListenBacklog);
                    isListening = true;
                }

                while (!isListening)
                {
                    try
                    {
                        this.listenSocket = new Socket(localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        if (localEndpoint.AddressFamily == AddressFamily.InterNetworkV6 && settings.TeredoEnabled)
                        {
                            this.listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)23, 10);
                        }

                        this.listenSocket.Bind(localEndpoint);
                        this.listenSocket.Listen(settings.ListenBacklog);
                        isListening = true;
                    }
                    catch (SocketException socketException)
                    {
                        bool retry = false;

                        if (socketException.ErrorCode == UnsafeNativeMethods.WSAEADDRINUSE)
                        {
                            if (!backoffHelper.IsExpired())
                            {
                                backoffHelper.WaitAndBackoff();
                                retry = true;
                            }
                        }

                        if (!retry)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                SocketConnectionListener.ConvertListenException(socketException, this.localEndpoint));
                        }
                    }
                }

                this.socketAsyncEventArgsPool = new SocketAsyncEventArgsPool(GetAcceptBufferSize(this.listenSocket));
            }
        }

        public static Exception ConvertListenException(SocketException socketException, IPEndPoint localEndpoint)
        {
            if (socketException.ErrorCode == UnsafeNativeMethods.ERROR_INVALID_HANDLE)
            {
                return new CommunicationObjectAbortedException(socketException.Message, socketException);
            }
            if (socketException.ErrorCode == UnsafeNativeMethods.WSAEADDRINUSE)
            {
                return new AddressAlreadyInUseException(SR.GetString(SR.TcpAddressInUse, localEndpoint.ToString()), socketException);
            }
            else
            {
                return new CommunicationException(
                    SR.GetString(SR.TcpListenError, socketException.ErrorCode, socketException.Message, localEndpoint.ToString()),
                    socketException);
            }
        }

        class AcceptAsyncResult : AsyncResult
        {
            SocketConnectionListener listener;
            Socket socket;
            SocketAsyncEventArgs socketAsyncEventArgs;
            static Action<object> startAccept;
            EventTraceActivity eventTraceActivity;

            // 
            static EventHandler<SocketAsyncEventArgs> acceptAsyncCompleted = new EventHandler<SocketAsyncEventArgs>(AcceptAsyncCompleted);
            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(OnInternalCompleting);

            public AcceptAsyncResult(SocketConnectionListener listener, AsyncCallback callback, object state)
                : base(callback, state)
            {

                if (TD.SocketAcceptEnqueuedIsEnabled())
                {
                    TD.SocketAcceptEnqueued(this.EventTraceActivity);
                }

                Fx.Assert(listener != null, "listener should not be null");
                this.listener = listener;
                this.socketAsyncEventArgs = listener.TakeSocketAsyncEventArgs();
                this.socketAsyncEventArgs.UserToken = this;
                this.socketAsyncEventArgs.Completed += acceptAsyncCompleted;
                this.OnCompleting = onCompleting;

                // If we're going to start up the thread pool eventually anyway, avoid using RegisterWaitForSingleObject
                if (!Thread.CurrentThread.IsThreadPoolThread)
                {
                    if (startAccept == null)
                    {
                        startAccept = new Action<object>(StartAccept);
                    }

                    ActionItem.Schedule(startAccept, this);
                }
                else
                {
                    bool completeSelf;
                    bool success = false;
                    try
                    {
                        completeSelf = StartAccept();
                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            // Return the args when an exception is thrown
                            ReturnSocketAsyncEventArgs();
                        }
                    }

                    if (completeSelf)
                    {
                        base.Complete(true);
                    }
                }
            }

            public EventTraceActivity EventTraceActivity
            {
                get
                {
                    if (this.eventTraceActivity == null)
                    {
                        this.eventTraceActivity = new EventTraceActivity();
                    }

                    return this.eventTraceActivity;
                }
            }

            static void StartAccept(object state)
            {
                AcceptAsyncResult thisPtr = (AcceptAsyncResult)state;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    completeSelf = thisPtr.StartAccept();
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
                    thisPtr.Complete(false, completionException);
                }
            }

            bool StartAccept()
            {
                while (true)
                {
                    try
                    {
                        return listener.InternalBeginAccept(DoAcceptAsync);
                    }
                    catch (SocketException socketException)
                    {
                        if (ShouldAcceptRecover(socketException))
                        {
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            static bool ShouldAcceptRecover(SocketException exception)
            {
                return (
                    (exception.ErrorCode == UnsafeNativeMethods.WSAECONNRESET) ||
                    (exception.ErrorCode == UnsafeNativeMethods.WSAEMFILE) ||
                    (exception.ErrorCode == UnsafeNativeMethods.WSAENOBUFS) ||
                    (exception.ErrorCode == UnsafeNativeMethods.WSAETIMEDOUT)
                );
            }

            // Return true means completed synchronously
            bool DoAcceptAsync(Socket listenSocket)
            {
                SocketAsyncEventArgsPool.CleanupAcceptSocket(this.socketAsyncEventArgs);

                if (listenSocket.AcceptAsync(this.socketAsyncEventArgs))
                {
                    // AcceptAsync returns true to indicate that the I/O operation is pending (asynchronous)
                    return false;
                }

                Exception exception = HandleAcceptAsyncCompleted();
                if (exception != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }

                return true;
            }

            static void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
            {
                AcceptAsyncResult thisPtr = (AcceptAsyncResult)e.UserToken;
                Fx.Assert(thisPtr.socketAsyncEventArgs == e, "Got wrong socketAsyncEventArgs");
                Exception completionException = thisPtr.HandleAcceptAsyncCompleted();
                if (completionException != null && ShouldAcceptRecover((SocketException)completionException))
                {
                    DiagnosticUtility.TraceHandledException(completionException, TraceEventType.Warning);

                    StartAccept(thisPtr);
                    return;
                }

                thisPtr.Complete(false, completionException);
            }

            static void OnInternalCompleting(AsyncResult result, Exception exception)
            {
                AcceptAsyncResult thisPtr = result as AcceptAsyncResult;

                if (TD.SocketAcceptedIsEnabled())
                {
                    int hashCode = thisPtr.socket != null ? thisPtr.socket.GetHashCode() : -1;
                    if (hashCode != -1)
                    {
                        TD.SocketAccepted(
                            thisPtr.EventTraceActivity,
                            thisPtr.listener != null ? thisPtr.listener.GetHashCode() : -1,
                            hashCode);
                    }
                    else
                    {
                        TD.SocketAcceptClosed(thisPtr.EventTraceActivity);
                    }
                }

                Fx.Assert(result != null, "Wrong async result has been passed in to OnInternalCompleting");
                thisPtr.ReturnSocketAsyncEventArgs();
            }

            void ReturnSocketAsyncEventArgs()
            {
                if (this.socketAsyncEventArgs != null)
                {
                    this.socketAsyncEventArgs.UserToken = null;
                    this.socketAsyncEventArgs.Completed -= acceptAsyncCompleted;
                    this.listener.ReturnSocketAsyncEventArgs(this.socketAsyncEventArgs);
                    this.socketAsyncEventArgs = null;
                }
            }

            Exception HandleAcceptAsyncCompleted()
            {
                Exception completionException = null;
                if (this.socketAsyncEventArgs.SocketError == SocketError.Success)
                {
                    this.socket = this.socketAsyncEventArgs.AcceptSocket;
                    this.socketAsyncEventArgs.AcceptSocket = null;
                }
                else
                {
                    completionException = new SocketException((int)this.socketAsyncEventArgs.SocketError);
                }

                return completionException;
            }

            public static Socket End(IAsyncResult result)
            {
                AcceptAsyncResult thisPtr = AsyncResult.End<AcceptAsyncResult>(result);
                return thisPtr.socket;
            }
        }
    }
}
