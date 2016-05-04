//------------------------------------------------------------------------------
// <copyright file="WebSocketConnectionStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.WebSockets
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    internal class WebSocketConnectionStream : BufferedReadStream, WebSocketBase.IWebSocketStream
    {
        private static readonly Func<Exception, bool> s_CanHandleException = new Func<Exception, bool>(CanHandleException);
        private static readonly Action<object> s_OnCancel = new Action<object>(OnCancel);
        private static readonly Action<object> s_OnCancelWebSocketConnection = new Action<object>(WebSocketConnection.OnCancel);
        private static readonly Type s_NetworkStreamType = typeof(NetworkStream);
        private readonly ConnectStream m_ConnectStream;
        private readonly string m_ConnectionGroupName;
        private readonly bool m_IsFastPathAllowed;
        private readonly object m_CloseConnectStreamLock;
        private bool m_InOpaqueMode;
        private WebSocketConnection m_WebSocketConnection;

        public WebSocketConnectionStream(ConnectStream connectStream, string connectionGroupName)
            : base(new WebSocketConnection(connectStream.Connection), false)
        {
            Contract.Assert(connectStream != null,
                "'connectStream' MUST NOT be NULL.");
            Contract.Assert(connectStream.Connection != null,
                "'connectStream.Conection' MUST NOT be NULL.");
            Contract.Assert(connectStream.Connection.NetworkStream != null,
                "'connectStream.Conection.NetworkStream' MUST NOT be NULL.");
            Contract.Assert(!string.IsNullOrEmpty(connectionGroupName), 
                "connectionGroupName should not be null or empty.");

            m_ConnectStream = connectStream;
            m_ConnectionGroupName = connectionGroupName;
            m_CloseConnectStreamLock = new object();
            // Make sure we don't short circuit for TlsStream or custom NetworkStream implementations
            m_IsFastPathAllowed = m_ConnectStream.Connection.NetworkStream.GetType() == s_NetworkStreamType;

            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Associate(Logging.WebSockets, this, m_ConnectStream.Connection);
            }

            ConsumeConnectStreamBuffer(connectStream);
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public bool SupportsMultipleWrite
        {
            get
            {
                return ((WebSocketConnection)this.BaseStream).SupportsMultipleWrite;
            }
        }

        public async Task CloseNetworkConnectionAsync(CancellationToken cancellationToken)
        {
            // need to yield here to make sure that we don't get any exception synchronously
            await Task.Yield();
            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Enter(Logging.WebSockets, this, Methods.CloseNetworkConnectionAsync, string.Empty);
            }

            CancellationTokenSource reasonableTimeoutCancellationTokenSource = null;
            CancellationTokenSource linkedCancellationTokenSource = null;
            CancellationToken linkedCancellationToken = CancellationToken.None;

            CancellationTokenRegistration cancellationTokenRegistration = new CancellationTokenRegistration();
            
            int bytesRead = 0;
            try
            {
                reasonableTimeoutCancellationTokenSource = 
                    new CancellationTokenSource(WebSocketHelpers.ClientTcpCloseTimeout);
                linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    reasonableTimeoutCancellationTokenSource.Token,
                    cancellationToken);
                linkedCancellationToken = linkedCancellationTokenSource.Token;
                cancellationTokenRegistration = linkedCancellationToken.Register(s_OnCancel, this, false);

                WebSocketHelpers.ThrowIfConnectionAborted(m_ConnectStream.Connection, true);
                byte[] buffer = new byte[1];
                if (m_WebSocketConnection != null && m_InOpaqueMode)
                {
                    bytesRead = await m_WebSocketConnection.ReadAsyncCore(buffer, 0, 1, linkedCancellationToken, true).SuppressContextFlow<int>();
                }
                else
                {
                    bytesRead = await base.ReadAsync(buffer, 0, 1, linkedCancellationToken).SuppressContextFlow<int>();
                }

                if (bytesRead != 0)
                {
                    Contract.Assert(false, "'bytesRead' MUST be '0' at this point. Instead more payload was received ('" + buffer[0].ToString() + "')");

                    if (WebSocketBase.LoggingEnabled)
                    {
                        Logging.Dump(Logging.WebSockets, this, Methods.CloseNetworkConnectionAsync, buffer, 0, bytesRead);
                    }

                    throw new WebSocketException(WebSocketError.NotAWebSocket);
                }
            }
            catch (Exception error)
            {
                if (!s_CanHandleException(error))
                {
                    throw;
                }

                // throw OperationCancelledException when canceled by the caller
                // ignore cancellation due to the timeout
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                cancellationTokenRegistration.Dispose();
                if (linkedCancellationTokenSource != null)
                {
                    linkedCancellationTokenSource.Dispose();
                }

                if (reasonableTimeoutCancellationTokenSource != null)
                {
                    reasonableTimeoutCancellationTokenSource.Dispose();
                }

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, this, Methods.CloseNetworkConnectionAsync, bytesRead);
                }
            }
        }

        public override void Close()
        {
            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Enter(Logging.WebSockets, this, Methods.Close, string.Empty);
            }

            try
            {
                // Taking a lock to avoid a race condition between ConnectStream.CloseEx (called in OnCancel) and 
                // ServicePoint.CloseConnectionGroup (called in Close) which can result in a deadlock
                lock (m_CloseConnectStreamLock)
                {
                    Contract.Assert(this.m_ConnectStream.Connection.ServicePoint != null, "connection.ServicePoint should not be null.");
                    this.m_ConnectStream.Connection.ServicePoint.CloseConnectionGroup(this.m_ConnectionGroupName);
                }
                base.Close();
            }
            finally
            {
                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, this, Methods.Close, string.Empty);
                }
            }
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Enter(Logging.WebSockets, this, Methods.ReadAsync,
                    WebSocketHelpers.GetTraceMsgForParameters(offset, count, cancellationToken));
            }

            CancellationTokenRegistration cancellationTokenRegistration = new CancellationTokenRegistration();

            int bytesRead = 0;
            try
            {
                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(s_OnCancel, this, false);
                }

                WebSocketHelpers.ThrowIfConnectionAborted(m_ConnectStream.Connection, true);
                bytesRead = await base.ReadAsync(buffer, offset, count, cancellationToken).SuppressContextFlow<int>();

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Dump(Logging.WebSockets, this, Methods.ReadAsync, buffer, offset, bytesRead);
                }
            }
            catch (Exception error)
            {
                if (s_CanHandleException(error))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                throw;
            }
            finally
            {
                cancellationTokenRegistration.Dispose();

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, this, Methods.ReadAsync, bytesRead);
                }
            }

            return bytesRead;
        }

        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Enter(Logging.WebSockets, this, Methods.WriteAsync,
                    WebSocketHelpers.GetTraceMsgForParameters(offset, count, cancellationToken));
            }
            CancellationTokenRegistration cancellationTokenRegistration = new CancellationTokenRegistration();

            try
            {
                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(s_OnCancel, this, false);
                }

                WebSocketHelpers.ThrowIfConnectionAborted(m_ConnectStream.Connection, false);
                await base.WriteAsync(buffer, offset, count, cancellationToken).SuppressContextFlow();

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Dump(Logging.WebSockets, this, Methods.WriteAsync, buffer, offset, count);
                }
            }
            catch (Exception error)
            {
                if (s_CanHandleException(error))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                throw;
            }
            finally
            {
                cancellationTokenRegistration.Dispose();

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, this,  Methods.WriteAsync, string.Empty);
                }
            }
        }

        public void SwitchToOpaqueMode(WebSocketBase webSocket)
        {
            Contract.Assert(webSocket != null, "'webSocket' MUST NOT be NULL.");
            Contract.Assert(!m_InOpaqueMode, "SwitchToOpaqueMode MUST NOT be called multiple times.");

            if (m_InOpaqueMode)
            {
                throw new InvalidOperationException();
            }

            m_WebSocketConnection = BaseStream as WebSocketConnection;

            if (m_WebSocketConnection != null && m_IsFastPathAllowed)
            {
                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Associate(Logging.WebSockets, this, m_WebSocketConnection);
                }

                m_WebSocketConnection.SwitchToOpaqueMode(webSocket);
                m_InOpaqueMode = true;
            }
        }

        public async Task MultipleWriteAsync(IList<ArraySegment<byte>> sendBuffers, CancellationToken cancellationToken)
        {
            Contract.Assert(this.SupportsMultipleWrite, "This method MUST NOT be used for custom NetworkStream implementations.");

            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Enter(Logging.WebSockets, this, Methods.MultipleWriteAsync, string.Empty);
            }
            CancellationTokenRegistration cancellationTokenRegistration = new CancellationTokenRegistration();

            try
            {
                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.Register(s_OnCancel, this, false);
                }

                WebSocketHelpers.ThrowIfConnectionAborted(m_ConnectStream.Connection, false);
                await ((WebSocketBase.IWebSocketStream)base.BaseStream).MultipleWriteAsync(sendBuffers, cancellationToken).SuppressContextFlow();

                if (WebSocketBase.LoggingEnabled)
                {
                    foreach(ArraySegment<byte> buffer in sendBuffers)
                    {
                        Logging.Dump(Logging.WebSockets, this, Methods.MultipleWriteAsync, buffer.Array, buffer.Offset, buffer.Count);
                    }
                }
            }
            catch (Exception error)
            {
                if (s_CanHandleException(error))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }

                throw;
            }
            finally
            {
                cancellationTokenRegistration.Dispose();

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, this, Methods.MultipleWriteAsync, string.Empty);
                }
            }
        }

        private static bool CanHandleException(Exception error)
        {
            return error is SocketException ||
                error is ObjectDisposedException ||
                error is WebException ||
                error is IOException;
        }

        private static void OnCancel(object state)
        {
            Contract.Assert(state != null, "'state' MUST NOT be NULL.");
            WebSocketConnectionStream thisPtr = state as WebSocketConnectionStream;
            Contract.Assert(thisPtr != null, "'thisPtr' MUST NOT be NULL.");

            if (WebSocketBase.LoggingEnabled)
            {
                Logging.Enter(Logging.WebSockets, state, Methods.OnCancel, string.Empty);
            }

            try
            {
                // Taking a lock to avoid a race condition between ConnectStream.CloseEx (called in OnCancel) and 
                // ServicePoint.CloseConnectionGroup (called in Close) which can result in a deadlock
                lock (thisPtr.m_CloseConnectStreamLock)
                {
                    // similar code like in HttpWebResponse.Abort, but we don't need some of the validations
                    // and we want to ensure that the TCP connection is reset
                    thisPtr.m_ConnectStream.Connection.NetworkStream.InternalAbortSocket();
                    ((ICloseEx)thisPtr.m_ConnectStream).CloseEx(CloseExState.Abort);
                }
                thisPtr.CancelWebSocketConnection();
            }
            catch { }
            finally
            {
                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, state, Methods.OnCancel, string.Empty);
                }
            }
        }

        private void CancelWebSocketConnection()
        {
            if (m_InOpaqueMode)
            {
                WebSocketConnection webSocketConnection = (WebSocketConnection)base.BaseStream;
                s_OnCancelWebSocketConnection(webSocketConnection);
            }
        }

        public void Abort()
        {
            OnCancel(this);
        }

        private void ConsumeConnectStreamBuffer(ConnectStream connectStream)
        {
            if (connectStream.Eof)
            {
                return;
            }

            byte[] buffer = new byte[1024];
            int count;
            int offset = 0;
            int size = buffer.Length;

            while ((count = connectStream.FillFromBufferedData(buffer, ref offset, ref size)) > 0)
            {
                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Dump(Logging.WebSockets, this, "ConsumeConnectStreamBuffer", buffer, 0, count);
                }

                Append(buffer, 0, count);
                offset = 0;
                size = buffer.Length;
            }
        }

        private static class Methods
        {
            public const string Close = "Close";
            public const string CloseNetworkConnectionAsync = "CloseNetworkConnectionAsync";
            public const string OnCancel = "OnCancel";
            public const string ReadAsync = "ReadAsync";
            public const string WriteAsync = "WriteAsync";
            public const string MultipleWriteAsync = "MultipleWriteAsync";
        }

        private class WebSocketConnection : DelegatedStream, WebSocketBase.IWebSocketStream
        {
            private static readonly EventHandler<SocketAsyncEventArgs> s_OnReadCompleted =
                new EventHandler<SocketAsyncEventArgs>(OnReadCompleted);
            private static readonly EventHandler<SocketAsyncEventArgs> s_OnWriteCompleted =
                new EventHandler<SocketAsyncEventArgs>(OnWriteCompleted);
            private static readonly Func<IList<ArraySegment<byte>>, AsyncCallback, object, IAsyncResult> s_BeginMultipleWrite =
                new Func<IList<ArraySegment<byte>>, AsyncCallback, object, IAsyncResult>(BeginMultipleWrite);
            private static readonly Action<IAsyncResult> s_EndMultipleWrite =
                new Action<IAsyncResult>(EndMultipleWrite);

#if DEBUG
            private class OutstandingOperations
            {
                internal int m_Reads;
                internal int m_Writes;
            }

            private readonly OutstandingOperations m_OutstandingOperations = new OutstandingOperations();
#endif //DEBUG

            private readonly Connection m_InnerStream;
            private readonly bool m_SupportsMultipleWrites;
            private bool m_InOpaqueMode;
            private WebSocketBase m_WebSocket;
            private SocketAsyncEventArgs m_WriteEventArgs;
            private SocketAsyncEventArgs m_ReadEventArgs;
            private TaskCompletionSource<object> m_WriteTaskCompletionSource;
            private TaskCompletionSource<int> m_ReadTaskCompletionSource;
            private int m_CleanedUp;
            private bool m_IgnoreReadError;

            internal WebSocketConnection(Connection connection)
                : base(connection)
            {
                Contract.Assert(connection != null, "'connection' MUST NOT be NULL.");
                Contract.Assert(connection.NetworkStream != null, "'connection.NetworkStream' MUST NOT be NULL.");

                m_InnerStream = connection;
                m_InOpaqueMode = false;
                // NetworkStream.Multiplewrite is internal. So custom NetworkStream implementations might not support it.
                m_SupportsMultipleWrites = connection.NetworkStream.GetType().Assembly == s_NetworkStreamType.Assembly;
            }

            internal Socket InnerSocket
            {
                get
                {
                    return GetInnerSocket(false);
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public bool SupportsMultipleWrite
            {
                get
                {
                    return m_SupportsMultipleWrites;
                }
            }

            public Task CloseNetworkConnectionAsync(CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public override void Close()
            {
                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, this, Methods.Close, string.Empty);
                }

                try
                {
                    base.Close();

                    if (Interlocked.Increment(ref m_CleanedUp) == 1)
                    {
                        if (m_WriteEventArgs != null)
                        {
                            m_WriteEventArgs.Completed -= s_OnWriteCompleted;
                            m_WriteEventArgs.Dispose();
                        }

                        if (m_ReadEventArgs != null)
                        {
                            m_ReadEventArgs.Completed -= s_OnReadCompleted;
                            m_ReadEventArgs.Dispose();
                        }
                    }
                }
                finally
                {
                    if (WebSocketBase.LoggingEnabled)
                    {
                        Logging.Exit(Logging.WebSockets, this, Methods.Close, string.Empty);
                    }
                }
            }

            internal Socket GetInnerSocket(bool skipStateCheck)
            {
                Socket returnValue;
                if (!skipStateCheck)
                {
                    m_WebSocket.ThrowIfClosedOrAborted();
                }
                try
                {
                    Contract.Assert(m_InnerStream.NetworkStream != null, "'m_InnerStream.NetworkStream' MUST NOT be NULL.");
                    returnValue = m_InnerStream.NetworkStream.InternalSocket;
                }
                catch (ObjectDisposedException)
                {
                    m_WebSocket.ThrowIfClosedOrAborted();
                    throw;
                }

                return returnValue;
            }

            private static IAsyncResult BeginMultipleWrite(IList<ArraySegment<byte>> sendBuffers, AsyncCallback callback, object asyncState)
            {
                Contract.Assert(sendBuffers != null, "'sendBuffers' MUST NOT be NULL.");
                Contract.Assert(asyncState != null, "'asyncState' MUST NOT be NULL.");
                WebSocketConnection connection = asyncState as WebSocketConnection;
                Contract.Assert(connection != null, "'connection' MUST NOT be NULL.");

                BufferOffsetSize[] buffers = new BufferOffsetSize[sendBuffers.Count];
                
                for (int index = 0; index < sendBuffers.Count; index++)
                {
                    ArraySegment<byte> sendBuffer = sendBuffers[index];
                    buffers[index] = new BufferOffsetSize(sendBuffer.Array, sendBuffer.Offset, sendBuffer.Count, false);
                }

                WebSocketHelpers.ThrowIfConnectionAborted(connection.m_InnerStream, false);
                return connection.m_InnerStream.NetworkStream.BeginMultipleWrite(buffers, callback, asyncState);
            }

            private static void EndMultipleWrite(IAsyncResult asyncResult)
            {
                Contract.Assert(asyncResult != null, "'asyncResult' MUST NOT be NULL.");
                Contract.Assert(asyncResult.AsyncState != null, "'asyncResult.AsyncState' MUST NOT be NULL.");
                WebSocketConnection connection = asyncResult.AsyncState as WebSocketConnection;
                Contract.Assert(connection != null, "'connection' MUST NOT be NULL.");

                WebSocketHelpers.ThrowIfConnectionAborted(connection.m_InnerStream, false);
                connection.m_InnerStream.NetworkStream.EndMultipleWrite(asyncResult);
            }

            public Task MultipleWriteAsync(IList<ArraySegment<byte>> sendBuffers, 
                CancellationToken cancellationToken)
            {
                Contract.Assert(this.SupportsMultipleWrite, "This method MUST NOT be used for custom NetworkStream implementations.");

                if (!m_InOpaqueMode)
                {
                    // We can't use fast path over SSL
                    return Task.Factory.FromAsync<IList<ArraySegment<byte>>>(s_BeginMultipleWrite, s_EndMultipleWrite, 
                        sendBuffers, this);
                }

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, this, Methods.MultipleWriteAsync, string.Empty);
                }

                bool completedAsynchronously = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
#if DEBUG
                    // When using fast path only one outstanding read is permitted. By switching into opaque mode
                    // via IWebSocketStream.SwitchToOpaqueMode (see more detailed comments in interface definition)
                    // caller takes responsibility for enforcing this constraint.
                    Contract.Assert(Interlocked.Increment(ref m_OutstandingOperations.m_Writes) == 1,
                        "Only one outstanding write allowed at any given time.");
#endif
                    WebSocketHelpers.ThrowIfConnectionAborted(m_InnerStream, false);
                    m_WriteTaskCompletionSource = new TaskCompletionSource<object>();
                    m_WriteEventArgs.SetBuffer(null, 0, 0);
                    m_WriteEventArgs.BufferList = sendBuffers;
                    completedAsynchronously = InnerSocket.SendAsync(m_WriteEventArgs);
                    if (!completedAsynchronously)
                    {
                        if (m_WriteEventArgs.SocketError != SocketError.Success)
                        {
                            throw new SocketException(m_WriteEventArgs.SocketError);
                        }

                        return Task.CompletedTask;
                    }

                    return m_WriteTaskCompletionSource.Task;
                }
                finally
                {
                    if (WebSocketBase.LoggingEnabled)
                    {
                        Logging.Exit(Logging.WebSockets, this, Methods.MultipleWriteAsync, completedAsynchronously);
                    }
                }
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                WebSocketHelpers.ValidateBuffer(buffer, offset, count);

                if (!m_InOpaqueMode)
                {
                    return base.WriteAsync(buffer, offset, count, cancellationToken);
                }

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, this, Methods.WriteAsync,
                        WebSocketHelpers.GetTraceMsgForParameters(offset, count, cancellationToken));
                }

                bool completedAsynchronously = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
#if DEBUG
                    // When using fast path only one outstanding read is permitted. By switching into opaque mode
                    // via IWebSocketStream.SwitchToOpaqueMode (see more detailed comments in interface definition)
                    // caller takes responsibility for enforcing this constraint.
                    Contract.Assert(Interlocked.Increment(ref m_OutstandingOperations.m_Writes) == 1,
                        "Only one outstanding write allowed at any given time.");
#endif
                    WebSocketHelpers.ThrowIfConnectionAborted(m_InnerStream, false);
                    m_WriteTaskCompletionSource = new TaskCompletionSource<object>();
                    m_WriteEventArgs.BufferList = null;
                    m_WriteEventArgs.SetBuffer(buffer, offset, count);
                    completedAsynchronously = InnerSocket.SendAsync(m_WriteEventArgs);
                    if (!completedAsynchronously)
                    {
                        if (m_WriteEventArgs.SocketError != SocketError.Success)
                        {
                            throw new SocketException(m_WriteEventArgs.SocketError);
                        }

                        return Task.CompletedTask;
                    }

                    return m_WriteTaskCompletionSource.Task;
                }
                finally
                {
                    if (WebSocketBase.LoggingEnabled)
                    {
                        Logging.Exit(Logging.WebSockets, this, Methods.WriteAsync, completedAsynchronously);
                    }
                }
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                WebSocketHelpers.ValidateBuffer(buffer, offset, count);

                if (!m_InOpaqueMode)
                {
                    return base.ReadAsync(buffer, offset, count, cancellationToken);
                }

                return ReadAsyncCore(buffer, offset, count, cancellationToken, false);
            }

            internal Task<int> ReadAsyncCore(byte[] buffer, int offset, int count, CancellationToken cancellationToken, 
                bool ignoreReadError)
            {
                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, this, Methods.ReadAsyncCore,
                        WebSocketHelpers.GetTraceMsgForParameters(offset, count, cancellationToken));
                }

                bool completedAsynchronously = false;
                m_IgnoreReadError = ignoreReadError;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
#if DEBUG
                    // When using fast path only one outstanding read is permitted. By switching into opaque mode
                    // via IWebSocketStream.SwitchToOpaqueMode (see more detailed comments in interface definition)
                    // caller takes responsibility for enforcing this constraint.
                    Contract.Assert(Interlocked.Increment(ref m_OutstandingOperations.m_Reads) == 1,
                        "Only one outstanding read allowed at any given time.");
#endif
                    WebSocketHelpers.ThrowIfConnectionAborted(m_InnerStream, true);
                    m_ReadTaskCompletionSource = new TaskCompletionSource<int>();
                    Contract.Assert(m_ReadEventArgs != null, "'m_ReadEventArgs' MUST NOT be NULL.");
                    m_ReadEventArgs.SetBuffer(buffer, offset, count);
                    Socket innerSocket;
                    if (ignoreReadError)
                    {
                        // The State of the WebSocket instance is already Closed at this point
                        // Skipping call to WebSocketBase.ThrowIfClosedOrAborted
                        innerSocket = GetInnerSocket(true);
                    }
                    else
                    {
                        innerSocket = InnerSocket;
                    }
                    completedAsynchronously = innerSocket.ReceiveAsync(m_ReadEventArgs);
                    if (!completedAsynchronously)
                    {
                        if (m_ReadEventArgs.SocketError != SocketError.Success)
                        {
                            if (!m_IgnoreReadError)
                            {
                                throw new SocketException(m_ReadEventArgs.SocketError);
                            }
                            else
                            {
                                return Task.FromResult<int>(0);
                            }
                        }

                        return Task.FromResult<int>(m_ReadEventArgs.BytesTransferred);
                    }

                    return m_ReadTaskCompletionSource.Task;
                }
                finally
                {
                    if (WebSocketBase.LoggingEnabled)
                    {
                        Logging.Exit(Logging.WebSockets, this, Methods.ReadAsyncCore, completedAsynchronously);
                    }
                }
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                if (!m_InOpaqueMode)
                {
                    return base.FlushAsync(cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            public void Abort()
            {
                // No op - the abort is handled by the WebSocketConnectionStream
            }

            // According to my tests even when aborting the underlying Socket the completionEvent for 
            // SocketAsyncEventArgs is not always fired, which can result in not cancelling the underlying
            // IO. Cancelling the TaskCompletionSources below is safe, because CompletionSource.Tryxxx 
            // is handling the race condition (whoever is completing the CompletionSource first wins.)
            internal static void OnCancel(object state)
            {
                Contract.Assert(state != null, "'state' MUST NOT be NULL.");
                WebSocketConnection thisPtr = state as WebSocketConnection;
                Contract.Assert(thisPtr != null, "'thisPtr' MUST NOT be NULL.");

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, thisPtr, Methods.OnCancel, string.Empty);
                }

                try
                {
                    TaskCompletionSource<int> readTaskCompletionSourceSnapshot = thisPtr.m_ReadTaskCompletionSource;

                    if (readTaskCompletionSourceSnapshot != null)
                    {
                        readTaskCompletionSourceSnapshot.TrySetCanceled();
                    }

                    TaskCompletionSource<object> writeTaskCompletionSourceSnapshot = thisPtr.m_WriteTaskCompletionSource;

                    if (writeTaskCompletionSourceSnapshot != null)
                    {
                        writeTaskCompletionSourceSnapshot.TrySetCanceled();
                    }
                }
                finally
                {
                    if (WebSocketBase.LoggingEnabled)
                    {
                        Logging.Exit(Logging.WebSockets, thisPtr, Methods.OnCancel, string.Empty);
                    }
                }
            }

            public void SwitchToOpaqueMode(WebSocketBase webSocket)
            {
                Contract.Assert(webSocket != null, "'webSocket' MUST NOT be NULL.");
                Contract.Assert(!m_InOpaqueMode, "SwitchToOpaqueMode MUST NOT be called multiple times.");
                m_WebSocket = webSocket;
                m_InOpaqueMode = true;
                m_ReadEventArgs = new SocketAsyncEventArgs();
                m_ReadEventArgs.UserToken = this;
                m_ReadEventArgs.Completed += s_OnReadCompleted;
                m_WriteEventArgs = new SocketAsyncEventArgs();
                m_WriteEventArgs.UserToken = this;
                m_WriteEventArgs.Completed += s_OnWriteCompleted;
            }

            private static string GetIOCompletionTraceMsg(SocketAsyncEventArgs eventArgs)
            {
                Contract.Assert(eventArgs != null, "'eventArgs' MUST NOT be NULL.");
                return string.Format(CultureInfo.InvariantCulture,
                    "LastOperation: {0}, SocketError: {1}",
                    eventArgs.LastOperation,
                    eventArgs.SocketError);
            }

            private static void OnWriteCompleted(object sender, SocketAsyncEventArgs eventArgs)
            {
                Contract.Assert(eventArgs != null, "'eventArgs' MUST NOT be NULL.");
                WebSocketConnection thisPtr = eventArgs.UserToken as WebSocketConnection;
                Contract.Assert(thisPtr != null, "'thisPtr' MUST NOT be NULL.");

#if DEBUG
                Contract.Assert(Interlocked.Decrement(ref thisPtr.m_OutstandingOperations.m_Writes) >= 0,
                    "'thisPtr.m_OutstandingOperations.m_Writes' MUST NOT be negative.");
#endif

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, thisPtr, Methods.OnWriteCompleted, 
                        GetIOCompletionTraceMsg(eventArgs));
                }

                if (eventArgs.SocketError != SocketError.Success)
                {
                    thisPtr.m_WriteTaskCompletionSource.TrySetException(new SocketException(eventArgs.SocketError));
                }
                else
                {
                    thisPtr.m_WriteTaskCompletionSource.TrySetResult(null);
                }

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, thisPtr, Methods.OnWriteCompleted, string.Empty);
                }
            }

            private static void OnReadCompleted(object sender, SocketAsyncEventArgs eventArgs)
            {
                Contract.Assert(eventArgs != null, "'eventArgs' MUST NOT be NULL.");
                WebSocketConnection thisPtr = eventArgs.UserToken as WebSocketConnection;
                Contract.Assert(thisPtr != null, "'thisPtr' MUST NOT be NULL.");
#if DEBUG
                Contract.Assert(Interlocked.Decrement(ref thisPtr.m_OutstandingOperations.m_Reads) >= 0,
                    "'thisPtr.m_OutstandingOperations.m_Reads' MUST NOT be negative.");
#endif

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Enter(Logging.WebSockets, thisPtr, Methods.OnReadCompleted,
                        GetIOCompletionTraceMsg(eventArgs));
                }

                if (eventArgs.SocketError != SocketError.Success)
                {
                    if (!thisPtr.m_IgnoreReadError)
                    {
                        thisPtr.m_ReadTaskCompletionSource.TrySetException(new SocketException(eventArgs.SocketError));
                    }
                    else
                    {
                        thisPtr.m_ReadTaskCompletionSource.TrySetResult(0);
                    }
                }
                else
                {
                    thisPtr.m_ReadTaskCompletionSource.TrySetResult(eventArgs.BytesTransferred);
                }

                if (WebSocketBase.LoggingEnabled)
                {
                    Logging.Exit(Logging.WebSockets, thisPtr, Methods.OnReadCompleted, string.Empty);
                }
            }

            private static class Methods
            {
                public const string Close = "Close";
                public const string OnCancel = "OnCancel";
                public const string OnReadCompleted = "OnReadCompleted";
                public const string OnWriteCompleted = "OnWriteCompleted";
                public const string ReadAsyncCore = "ReadAsyncCore";
                public const string WriteAsync = "WriteAsync";
                public const string MultipleWriteAsync = "MultipleWriteAsync";
            }
        }
    }
}
