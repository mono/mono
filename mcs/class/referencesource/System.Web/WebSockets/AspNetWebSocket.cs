//------------------------------------------------------------------------------
// <copyright file="AspNetWebSocket.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Util;

    // Used to send and receive messages over a WebSocket connection
    //
    // SYNCHRONIZATION AND THREAD SAFETY:
    //
    // This class is not generally thread-safe, but the following exceptions are made:
    //
    // - We'll detect multiple calls to SendAsync (e.g. a second call takes place while the first
    //   call isn't yet complete) and throw an appropriate exception. CloseOutputAsync is
    //   an equivalent of SendAsync for this purpose.
    //
    // - There can be a pending call to SendAsync (or CloseOutputAsync) at the same time as a
    //   call to ReceiveAsync, and we'll handle synchronization properly.
    //
    // - CloseAsync is the equivalent of starting parallel sends and receives. If there's already
    //   a send (SendAsync / CloseOutputAsync) or receive (ReceiveAsync) in progress, we'll
    //   detect this and throw an appropriate exception.
    //
    // - Any thread can call Abort at any time. Any pending sends / receives will result in
    //   failure. Continuations hanging off the failed tasks will execute, but they may do
    //   so before or after the call to Abort returns.
    //
    // - Dispose is *not* thread-safe and should not be called while the socket has a
    //   pending operation.
    //
    // TAP asks that if exceptions are thrown due to precondition violations (e.g. bad parameters
    // or the object is in an invalid state), these be thrown synchronously from the Async method
    // rather than shoved into the resulting Task. Hence in the design below, the AsyncImpl methods
    // themselves are synchronous and perform any necessary initialization, then they return
    // delegates that can be used to kick off the asynchronous operations. This allows the
    // AsyncImpl callers to provide coarser locking around multiple methods if necessary. In all
    // other cases, the locks are held for the absolute minimum time necessary to guarantee thread-
    // safety. Importantly, locks must never be held while awaiting an asynchronous method.
    //
    // VISIBILITY:
    //
    // Aside from the ctor, internal members of this type are not meant to be called from outside
    // the type itself. Any internal members are only internal to ease unit testing. We have the
    // goal of allowing third-party developers to create the same higher-level abstractions over
    // this type that we can, and having ASP.NET call internal members runs counter to that goal.
    //
    // TERMINOLOGY:
    //
    // The WebSocket protocol is defined at:
    // http://tools.ietf.org/html/rfc6455
    //
    // "WSPC" is the WebSocket Protocol Component (websocket.dll), which provides WebSocket-
    // parsing services to WCF, IIS, IE, and others. ASP.NET doesn't call into WSPC directly;
    // rather, IIS calls into WSPC on our behalf.

    public sealed class AspNetWebSocket : WebSocket, IAsyncAbortableWebSocket {

        // RFC 6455, Sec. 5.5:
        // All control frames MUST be 125 bytes or less in length and MUST NOT be fragmented.
        // 
        // When a CLOSE frame is sent, it has an optional payload that consists of a 2-byte status code followed by a
        // UTF8-encoded string. This string must therefore be no greater than 123 bytes (after UTF8-encoding) in length.
        private const int _maxCloseMessageByteCount = 123;

        // represents no-op tasks or asynchronous operations
        private static readonly Task _completedTask = Task.FromResult<object>(null);
        private static readonly Func<Task> _completedTaskFunc = () => _completedTask;

        // machine-readable / human-readable reasons this WebSocket was closed
        private const WebSocketCloseStatus CLOSE_STATUS_NOT_SET = (WebSocketCloseStatus)(-1); // used as a 'null' placeholder for _closeStatus since Nullable<T> access not guaranteed atomic
        private WebSocketCloseStatus _closeStatus = CLOSE_STATUS_NOT_SET;
        private string _closeStatusDescription;

        // for determining whether the socket has been disposed of
        private bool _disposed;

        // the pipe used for low-level communication with the remote endpoint
        private readonly IWebSocketPipe _pipe;
        private readonly string _subProtocol;

        // the current state of the RECEIVE (remote to local) and SEND (local to remote) channels
        private int _abortAsyncCalled;
        private CountdownTask _pendingOperationCounter = new CountdownTask(1);
        internal ChannelState _receiveState;
        internal ChannelState _sendState;

        // the current state (observable externally) of this socket
        internal WebSocketState _state = WebSocketState.Open;

        // for synchronization around controlling state transitions
        private readonly object _stateLockObj = new object();

        internal AspNetWebSocket(IWebSocketPipe pipe, string subProtocol) {
            _pipe = pipe;
            _subProtocol = subProtocol;

            // It is possible that a pipe couldn't be established, e.g. if the client disconnects or there is
            // a server error before the handshake is complete. If this is the case, we just immediately
            // mark this AspNetWebSocket instance as aborted.
            if (_pipe == null) {
                Abort();
            }
        }

        public override WebSocketCloseStatus? CloseStatus {
            get {
                ThrowIfDisposed();

                WebSocketCloseStatus closeStatus = _closeStatus;
                if (closeStatus == CLOSE_STATUS_NOT_SET) {
                    // no close status (not even Unspecified) to report to the caller; most likely reason is connection hasn't been closed
                    return null;
                }
                else {
                    return closeStatus;
                }
            }
        }

        public override string CloseStatusDescription {
            get {
                ThrowIfDisposed();
                return _closeStatusDescription;
            }
        }

        public override WebSocketState State {
            get {
                ThrowIfDisposed();
                return _state;
            }
        }

        public override string SubProtocol {
            get {
                ThrowIfDisposed();
                return _subProtocol;
            }
        }

        // for unit testing
        internal CountdownTask PendingOperationCounter {
            get {
                return _pendingOperationCounter;
            }
        }

        // Rudely closes a connection and cancels all pending I/O.
        // This is a fire-and-forget method which never blocks.
        // Both this and AbortAsync() are thread-safe.
        public override void Abort() {
            Abort(throwIfDisposed: true); // throws if disposed, per spec
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CloseTcpConnection() is not a dangerous method.")]
        private void Abort(bool throwIfDisposed, bool isDisposing = false) {
            lock (_stateLockObj) {
                // State validation
                if (throwIfDisposed) {
                    ThrowIfDisposed();
                }

                try {
                    if (IsStateTerminal(_state)) {
                        // If we're already in a terminal state, do nothing.
                        return;
                    }
                    else {
                        // Allowed transitions:
                        // - Open -> Aborted
                        // - CloseSent -> Aborted
                        // - CloseReceived -> Aborted
                        _state = WebSocketState.Aborted;
                    }
                }
                finally {
                    // Is this being called from Dispose()?
                    if (isDisposing) {
                        _disposed = true;
                    }
                }
            }

            // -- CORE LOGIC --
            // Everything after this point will be executed only once.

            if (_pipe != null) {
                _pipe.CloseTcpConnection();
            }
        }

        // Similar to Abort(), but returns a Task which allows a caller to know when
        // the Abort operation is complete. When the returned Task is complete, this
        // means that there will be no more calls to or from the _pipe object.
        // This method is always safe to call, even if the socket has been disposed.
        // This method must be called at the end of WebSocket processing in order
        // to release resources associated with this socket.
        internal Task AbortAsync() {
            Abort(throwIfDisposed: false);

            if (Interlocked.Exchange(ref _abortAsyncCalled, 1) == 0) {
                // The CountdownTask was seeded with an initial value of 1 so that the
                // associated Task wouldn't be marked as complete until this call to
                // MarkOperationCompleted. If there is pending IO, the current value
                // might still be > 1.
                _pendingOperationCounter.MarkOperationCompleted();
            }
            return _pendingOperationCounter.Task;
        }

        // Asynchronously sends a CLOSE frame and waits for the remote endpoint to send a CLOSE/ACK.
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            return CloseAsyncImpl(closeStatus, statusDescription, cancellationToken)();
        }

        private Func<Task> CloseAsyncImpl(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken, bool performValidation = true) {
            Func<Task> sendCloseTaskFunc = null;
            Func<Task<WebSocketReceiveResult>> receiveCloseTaskFunc = null;

            // -- PARAMETER VALIDATION --
            // Performed outside lock since doesn't affect state

            if (performValidation) {
                ValidateCloseStatusCodeAndDescription(closeStatus, ref statusDescription);
            }

            lock (_stateLockObj) {
                // -- STATE VALIDATION --
                // Performed within lock

                if (performValidation) {
                    ThrowIfDisposed();
                    ThrowIfAborted();
                    ThrowIfSendUnavailable(allowClosed: true);
                    ThrowIfReceiveUnavailable(allowClosed: true);
                }

                // -- STATE INITIALIZATION --
                // Performed within lock

                // State transitions are handled automatically by the CloseOutputAsyncImpl / ReceiveAsyncImpl methods
                // These methods don't need to perform validation since this method has already performed it (if necessary)
                if (_sendState != ChannelState.Closed) {
                    sendCloseTaskFunc = CloseOutputAsyncImpl(closeStatus, statusDescription, cancellationToken, performValidation: false);
                }
                if (_receiveState != ChannelState.Closed) {
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[_maxCloseMessageByteCount]);
                    receiveCloseTaskFunc = ReceiveAsyncImpl(buffer, cancellationToken, performValidation: false);
                }

                // special-case no outstanding work to perform
                if (sendCloseTaskFunc == null && receiveCloseTaskFunc == null) {
                    return _completedTaskFunc;
                }
            }

            // once initialization is complete, the implementation can run truly asynchronously
            return async () => {
                // By kicking off both tasks in parallel before awaiting either one, we have full-duplex communication
                Task sendCloseTask = (sendCloseTaskFunc != null) ? sendCloseTaskFunc() : null;
                Task<WebSocketReceiveResult> receiveCloseTask = (receiveCloseTaskFunc != null) ? receiveCloseTaskFunc() : null;

                if (sendCloseTask != null) {
                    await sendCloseTask.ConfigureAwait(continueOnCapturedContext: false);

                    // -- ASYNC POINT --
                    // Any code after this which requires synchronization must reacquire the lock
                }

                if (receiveCloseTask != null) {
                    WebSocketReceiveResult result = await receiveCloseTask.ConfigureAwait(continueOnCapturedContext: false);

                    // -- ASYNC POINT --
                    // Any code after this which requires synchronization must reacquire the lock

                    // Throw if this wasn't a CLOSE frame
                    if (result.MessageType != WebSocketMessageType.Close) {
                        Abort(throwIfDisposed: false);
                        throw new WebSocketException(WebSocketError.InvalidMessageType);
                    }
                }
            };
        }

        // Sends a CLOSE frame asynchronously.
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken) {
            return CloseOutputAsyncImpl(closeStatus, statusDescription, cancellationToken)();
        }

        private Func<Task> CloseOutputAsyncImpl(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken, bool performValidation = true) {
            // -- PARAMETER VALIDATION --
            // Performed outside lock since doesn't affect state

            if (performValidation) {
                ValidateCloseStatusCodeAndDescription(closeStatus, ref statusDescription);
            }

            lock (_stateLockObj) {
                // -- STATE VALIDATION --
                // Performed within lock

                if (performValidation) {
                    ThrowIfDisposed();
                    ThrowIfAborted();
                    ThrowIfSendUnavailable(allowClosed: true);
                }

                // -- STATE INITIALIZATION --
                // Performed within lock

                if (_sendState == ChannelState.Closed) {
                    // already closed; nothing to do
                    return _completedTaskFunc;
                }

                // Mark channel as in use
                _sendState = ChannelState.Busy;
                _pendingOperationCounter.MarkOperationPending();
            }

            // once initialization is complete, the implementation can run truly asynchronously
            return async () => {
                // Send the fragment
                await DoWork(() => _pipe.WriteCloseFragmentAsync(closeStatus, statusDescription), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                // -- ASYNC POINT --
                // Any code after this which requires synchronization must reacquire the lock

                lock (_stateLockObj) {
                    _sendState = ChannelState.Closed;

                    // State transition:
                    // - Open -> CloseSent
                    // - CloseReceived -> Closed
                    switch (_state) {
                        case WebSocketState.Open:
                            _state = WebSocketState.CloseSent;
                            break;

                        case WebSocketState.CloseReceived:
                            _state = WebSocketState.Closed;
                            break;
                    }
                }
            };
        }

        // Releases resources associated with this object; similar to calling Abort
        public override void Dispose() {
            throw new NotSupportedException(SR.GetString(SR.AspNetWebSocket_DisposeNotSupported));
        }

        internal void DisposeInternal() {
            Abort(throwIfDisposed: false, isDisposing: true);
        }


        // Receives a single fragment from the remote endpoint
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) {
            return ReceiveAsyncImpl(buffer, cancellationToken)();
        }

        private Func<Task<WebSocketReceiveResult>> ReceiveAsyncImpl(ArraySegment<byte> buffer, CancellationToken cancellationToken, bool performValidation = true) {
            lock (_stateLockObj) {
                // -- STATE VALIDATION --
                // Performed within lock

                if (performValidation) {
                    ThrowIfDisposed();
                    ThrowIfAborted();
                    ThrowIfReceiveUnavailable();
                }

                // -- STATE INITIALIZATION --
                // Performed within lock

                // Mark channel as in use
                _receiveState = ChannelState.Busy;
                _pendingOperationCounter.MarkOperationPending();
            }

            // once initialization is complete, the implementation can run truly asynchronously
            return async () => {
                // Receive a single fragment
                WebSocketReceiveResult result = await DoWork(() => _pipe.ReadFragmentAsync(buffer), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                // -- ASYNC POINT --
                // Any code after this which requires synchronization must reacquire the lock

                lock (_stateLockObj) {
                    if (result.MessageType == WebSocketMessageType.Close) {
                        // received a CLOSE frame
                        _receiveState = ChannelState.Closed;
                        Debug.Assert(result.CloseStatus.HasValue, "The CloseStatus property should be non-null when a CLOSE frame is received.");
                        _closeStatus = result.CloseStatus.Value;
                        _closeStatusDescription = result.CloseStatusDescription;

                        // State transition:
                        // - Open -> CloseReceived
                        // - CloseSent -> Closed
                        switch (_state) {
                            case WebSocketState.Open:
                                _state = WebSocketState.CloseReceived;
                                break;

                            case WebSocketState.CloseSent:
                                _state = WebSocketState.Closed;
                                break;
                        }
                    }
                    else {
                        _receiveState = ChannelState.Ready;
                    }
                }

                return result;
            };
        }

        // Sends a single fragment to the remote endpoint
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken) {
            return SendAsyncImpl(buffer, messageType, endOfMessage, cancellationToken)();
        }

        private Func<Task> SendAsyncImpl(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken, bool performValidation = true) {
            // -- PARAMETER VALIDATION --
            // Performed outside lock since doesn't affect state

            if (performValidation) {
                ValidateSendMessageType(messageType);
            }

            lock (_stateLockObj) {
                // -- STATE VALIDATION --
                // Performed within lock

                if (performValidation) {
                    ThrowIfDisposed();
                    ThrowIfAborted();
                    ThrowIfSendUnavailable();
                }

                // -- STATE INITIALIZATION --
                // Performed within lock

                // Mark channel as in use
                _sendState = ChannelState.Busy;
                _pendingOperationCounter.MarkOperationPending();
            }

            // once initialization is complete, the implementation can run truly asynchronously
            return async () => {
                // Send the fragment
                await DoWork(() => _pipe.WriteFragmentAsync(buffer, (messageType == WebSocketMessageType.Text), endOfMessage), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

                // -- ASYNC POINT --
                // Any code after this which requires synchronization must reacquire the lock      

                lock (_stateLockObj) {
                    _sendState = ChannelState.Ready;
                }
            };
        }

        #region Validation and helper methods
        // Validates that the provided 'closeStatus' and 'statusDescription' parameters are sane.
        // The 'statusDescription' parameter is taken by reference since it might be normalized.
        internal static void ValidateCloseStatusCodeAndDescription(WebSocketCloseStatus closeStatus, ref string statusDescription) {
            // We do three checks here:
            // - RFC 6455, Sec. 5.5.1: Close status is 16-bit unsigned integer, so we need to make sure it is within the range 0x0000 - 0xffff.
            // - RFC 6455, Sec. 7.4.2: 0 - 999 is an invalid value for the close status.
            // - RFC 6455, Sec. 7.4.1: 1004, 1006, 1010, 1015 are invalid status codes for the server to send to the client.
            if (closeStatus < (WebSocketCloseStatus)1000
                || closeStatus > (WebSocketCloseStatus)UInt16.MaxValue
                || closeStatus == (WebSocketCloseStatus)1004
                || closeStatus == (WebSocketCloseStatus)1006
                || closeStatus == (WebSocketCloseStatus)1010
                || closeStatus == (WebSocketCloseStatus)1015) {
                throw new ArgumentOutOfRangeException("closeStatus");
            }

            if (closeStatus == WebSocketCloseStatus.Empty) {
                // Fix Bug : 312472, we would like to allow empty strings to be passed to our APIs when status code is 1005.
                // Since WSPC requires the statusDescription to be null, we convert.
                if (statusDescription == String.Empty) {
                    statusDescription = null;
                }
                // If the status code is 1005 (Empty), the statusDescription string MUST be null.
                // This behavior is required by WSPC and matches WCF.
                if (statusDescription != null) {
                    throw new ArgumentException(SR.GetString(SR.AspNetWebSocket_CloseStatusEmptyButCloseDescriptionNonNull), "statusDescription");
                }
            }
            else if (statusDescription != null) {
                // Need to make sure the provided status description fits within a single WebSocket control frame.
                int byteCount = Encoding.UTF8.GetByteCount(statusDescription);
                if (byteCount > _maxCloseMessageByteCount) {
                    throw new ArgumentException(SR.GetString(SR.AspNetWebSocket_CloseDescriptionTooLong, _maxCloseMessageByteCount), "statusDescription");
                }
            }
            else {
                // WSPC requires that null status descriptions be normalized to empty strings.
                statusDescription = String.Empty;
            }
        }

        private static void ValidateSendMessageType(WebSocketMessageType messageType) {
            switch (messageType) {
                case WebSocketMessageType.Text:
                case WebSocketMessageType.Binary:
                    return; // these are OK

                default:
                    throw new ArgumentException(SR.GetString(SR.AspNetWebSocket_SendMessageTypeInvalid), "messageType");
            }
        }

        private void ThrowIfDisposed() {
            if (_disposed) {
                throw new ObjectDisposedException(objectName: GetType().FullName);
            }
        }

        private void ThrowIfAborted() {
            if (_state == WebSocketState.Aborted) {
                throw new WebSocketException(WebSocketError.InvalidState);
            }
        }

        private void ThrowIfSendUnavailable(bool allowClosed = false) {
            switch (_sendState) {
                case ChannelState.Busy:
                    throw new InvalidOperationException(SR.GetString(SR.AspNetWebSocket_SendInProgress));

                case ChannelState.Closed:
                    if (allowClosed) { break; }
                    throw new InvalidOperationException(SR.GetString(SR.AspNetWebSocket_CloseAlreadySent));
            }
        }

        private void ThrowIfReceiveUnavailable(bool allowClosed = false) {
            switch (_receiveState) {
                case ChannelState.Busy:
                    throw new InvalidOperationException(SR.GetString(SR.AspNetWebSocket_ReceiveInProgress));

                case ChannelState.Closed:
                    if (allowClosed) { break; }
                    throw new InvalidOperationException(SR.GetString(SR.AspNetWebSocket_CloseAlreadyReceived));
            }
        }

        // Helper method that wraps performing some work and aborting on exceptions
        private async Task<T> DoWork<T>(Func<Task<T>> taskDelegate, CancellationToken cancellationToken) {
            // Used for timing out operations
            IDisposable cancellationTokenRegistration = null;

            try {
                try {
                    // If cancellation is requested, honor it immediately. This is the same kind of optimization done throughout
                    // the rest of the framework, e.g. as in FileStream.ReadAsync. Our finally block will be responsible for
                    // throwing the actual exception which results in the Task being canceled.
                    if (cancellationToken.IsCancellationRequested) {
                        return default(T);
                    }

                    Task<T> task = taskDelegate();

                    if (!task.IsCompleted && cancellationToken.CanBeCanceled) {
                        // If the task didn't complete synchronously, let the CancellationToken abort the operation
                        // The callback may complete inline, so we need to make sure it doesn't throw
                        cancellationTokenRegistration = cancellationToken.Register(() => Abort(throwIfDisposed: false));
                    }

                    // The 'await' keyword may cause an exception to be observed (rethrown)
                    // We call only thread-safe methods so don't need to spend the time to come back to the SynchronizationContext
                    return await task.ConfigureAwait(continueOnCapturedContext: false);
                }
                finally {
                    // called for both normal and exceptional completion
                    _pendingOperationCounter.MarkOperationCompleted();

                    // failed due to cancellation?
                    if (cancellationTokenRegistration != null) {
                        cancellationTokenRegistration.Dispose();
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            catch {
                // Something went wrong while communicating on the pipe - mark faulted and observe exception.
                // Benign ---- - Abort might be called both by the CancellationTokenRegistration and by the line below.
                Abort(throwIfDisposed: false);
                throw;
            }
        }

        // Helper method that wraps performing some work and aborting on exceptions
        internal Task DoWork(Func<Task> taskDelegate, CancellationToken cancellationToken) {
            return DoWork<object>(async () => { await taskDelegate().ConfigureAwait(continueOnCapturedContext: false); return null; }, cancellationToken);
        }
        #endregion

        Task IAsyncAbortableWebSocket.AbortAsync() {
            return AbortAsync();
        }

        // Represents the state of a single channel; send and receive have their own states
        internal enum ChannelState {
            Ready, // this channel is available for transmitting new frames
            Busy, // the channel is already busy transmitting frames
            Closed // this channel has been closed
        }

    }
}
