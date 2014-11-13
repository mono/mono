// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;

    abstract class WebSocketTransportDuplexSessionChannel : TransportDuplexSessionChannel
    {
        static AsyncCallback streamedWriteCallback = Fx.ThunkCallback(StreamWriteCallback);
        WebSocket webSocket = null;
        WebSocketTransportSettings webSocketSettings;
        TransferMode transferMode;
        int maxBufferSize;
        WaitCallback waitCallback;
        object state;
        WebSocketStream webSocketStream;
        byte[] internalBuffer;
        ConnectionBufferPool bufferPool;
        int cleanupStatus = WebSocketHelper.OperationNotStarted;
        ITransportFactorySettings transportFactorySettings;
        WebSocketCloseDetails webSocketCloseDetails = new WebSocketCloseDetails();
        bool shouldDisposeWebSocketAfterClosed = true;
        Exception pendingWritingMessageException;
        
        public WebSocketTransportDuplexSessionChannel(HttpChannelListener channelListener, EndpointAddress localAddress, Uri localVia, ConnectionBufferPool bufferPool)
            : base(channelListener, channelListener, localAddress, localVia, EndpointAddress.AnonymousAddress, channelListener.MessageVersion.Addressing.AnonymousUri)
        {
            Fx.Assert(channelListener.WebSocketSettings != null, "channelListener.WebSocketTransportSettings should not be null.");
            this.webSocketSettings = channelListener.WebSocketSettings;
            this.transferMode = channelListener.TransferMode;
            this.maxBufferSize = channelListener.MaxBufferSize;
            this.bufferPool = bufferPool;
            this.transportFactorySettings = channelListener;
        }

        public WebSocketTransportDuplexSessionChannel(HttpChannelFactory<IDuplexSessionChannel> channelFactory, EndpointAddress remoteAddresss, Uri via, ConnectionBufferPool bufferPool)
            : base(channelFactory, channelFactory, EndpointAddress.AnonymousAddress, channelFactory.MessageVersion.Addressing.AnonymousUri, remoteAddresss, via)
        {
            Fx.Assert(channelFactory.WebSocketSettings != null, "channelFactory.WebSocketTransportSettings should not be null.");
            this.webSocketSettings = channelFactory.WebSocketSettings;
            this.transferMode = channelFactory.TransferMode;
            this.maxBufferSize = channelFactory.MaxBufferSize;
            this.bufferPool = bufferPool;
            this.transportFactorySettings = channelFactory;
        }

        protected WebSocket WebSocket
        {
            get
            {
                return this.webSocket;
            }

            set
            {
                Fx.Assert(value != null, "value should not be null.");
                Fx.Assert(this.webSocket == null, "webSocket should not be set before this set call.");
                this.webSocket = value;
            }
        }

        protected WebSocketTransportSettings WebSocketSettings
        {
            get { return this.webSocketSettings; }
        }

        protected TransferMode TransferMode
        {
            get { return this.transferMode; }
        }

        protected int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
        }

        protected ITransportFactorySettings TransportFactorySettings
        {
            get
            {
                return this.transportFactorySettings;
            }
        }

        protected byte[] InternalBuffer
        {
            get
            {
                return this.internalBuffer;
            }

            set
            {
                // We allow setting the property to null as long as we don't overwrite an existing non-null 'internalBuffer'. Because otherwise 
                // we get NullRefs in other places. So if you change/remove the assert below, make sure we still assert for this case.
                Fx.Assert(this.internalBuffer == null, "internalBuffer should not be set twice.");
                this.internalBuffer = value;
            }
        }

        protected bool ShouldDisposeWebSocketAfterClosed
        {
            set
            {
                this.shouldDisposeWebSocketAfterClosed = value;
            }
        }

        protected override void OnAbort()
        {
            if (TD.WebSocketConnectionAbortedIsEnabled())
            {
                TD.WebSocketConnectionAborted(
                    this.EventTraceActivity,
                    this.WebSocket != null ? this.WebSocket.GetHashCode() : -1);
            }

            this.Cleanup();
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IWebSocketCloseDetails))
            {
                return this.webSocketCloseDetails as T;
            }

            return base.GetProperty<T>();
        }

        protected override void CompleteClose(TimeSpan timeout)
        {
            if (TD.WebSocketCloseSentIsEnabled())
            {
                TD.WebSocketCloseSent(
                    this.WebSocket.GetHashCode(),
                    this.webSocketCloseDetails.OutputCloseStatus.ToString(),
                    this.RemoteAddress != null ? this.RemoteAddress.ToString() : string.Empty);
            }

            Task closeTask = this.CloseAsync(); 
            closeTask.Wait(timeout, WebSocketHelper.ThrowCorrectException, WebSocketHelper.CloseOperation);

            if (TD.WebSocketConnectionClosedIsEnabled())
            {
                TD.WebSocketConnectionClosed(this.WebSocket.GetHashCode());
            }
        }

        protected byte[] TakeBuffer()
        {
            Fx.Assert(this.bufferPool != null, "'bufferPool' MUST NOT be NULL.");
            return this.bufferPool.Take();
        }

        protected override void CloseOutputSessionCore(TimeSpan timeout)
        {
            if (TD.WebSocketCloseOutputSentIsEnabled())
            {
                TD.WebSocketCloseOutputSent(
                    this.WebSocket.GetHashCode(),
                    this.webSocketCloseDetails.OutputCloseStatus.ToString(),
                    this.RemoteAddress != null ? this.RemoteAddress.ToString() : string.Empty);
            }

            Task task = this.CloseOutputAsync(CancellationToken.None);
            task.Wait(timeout, WebSocketHelper.ThrowCorrectException, WebSocketHelper.CloseOperation);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            try
            {
                base.OnClose(timeout);
            }
            finally
            {
                this.Cleanup();
            }
        }

        protected override void ReturnConnectionIfNecessary(bool abort, TimeSpan timeout)
        {
        }

        protected override AsyncCompletionResult StartWritingBufferedMessage(Message message, ArraySegment<byte> messageData, bool allowOutputBatching, TimeSpan timeout, Threading.WaitCallback callback, object state)
        {
            Fx.Assert(callback != null, "callback should not be null.");

            TimeoutHelper helper = new TimeoutHelper(timeout);
            WebSocketMessageType outgoingMessageType = GetWebSocketMessageType(message);
            IOThreadCancellationTokenSource cancellationTokenSource = new IOThreadCancellationTokenSource(helper.RemainingTime());

            if (TD.WebSocketAsyncWriteStartIsEnabled())
            {
                TD.WebSocketAsyncWriteStart(
                    this.WebSocket.GetHashCode(),
                    messageData.Count,
                    this.RemoteAddress != null ? this.RemoteAddress.ToString() : string.Empty);
            }

            Task task = this.WebSocket.SendAsync(messageData, outgoingMessageType, true, cancellationTokenSource.Token);
            Fx.Assert(this.pendingWritingMessageException == null, "'pendingWritingMessageException' MUST be NULL at this point.");

            task.ContinueWith(t =>
            {
                try
                {
                    if (TD.WebSocketAsyncWriteStopIsEnabled())
                    {
                        TD.WebSocketAsyncWriteStop(this.webSocket.GetHashCode());
                    }

                    cancellationTokenSource.Dispose();
                    WebSocketHelper.ThrowExceptionOnTaskFailure(t, timeout, WebSocketHelper.SendOperation);
                }
                catch (Exception error)
                {
                    // Intentionally not following the usual pattern to rethrow fatal exceptions.
                    // Any rethrown exception would just be ----ed, because nobody awaits the
                    // Task returned from ContinueWith in this case.

                    FxTrace.Exception.TraceHandledException(error, TraceEventType.Information);
                    this.pendingWritingMessageException = error;
                }
                finally
                {
                    callback.Invoke(state);
                }
            }, CancellationToken.None);

            return AsyncCompletionResult.Queued;
        }

        protected override void FinishWritingMessage()
        {
            ThrowOnPendingException(ref this.pendingWritingMessageException);
            base.FinishWritingMessage();
        }

        protected override AsyncCompletionResult StartWritingStreamedMessage(Message message, TimeSpan timeout, WaitCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            WebSocketMessageType outgoingMessageType = GetWebSocketMessageType(message);
            WebSocketStream webSocketStream = new WebSocketStream(this.WebSocket, outgoingMessageType, helper.RemainingTime());

            this.waitCallback = callback;
            this.state = state;
            this.webSocketStream = webSocketStream;
            IAsyncResult result = this.MessageEncoder.BeginWriteMessage(message, new TimeoutStream(webSocketStream, ref helper), streamedWriteCallback, this);

            if (!result.CompletedSynchronously)
            {
                return AsyncCompletionResult.Queued;
            }

            this.MessageEncoder.EndWriteMessage(result);

            webSocketStream.WriteEndOfMessageAsync(helper.RemainingTime(), callback, state);
            return AsyncCompletionResult.Queued;
        }

        protected override AsyncCompletionResult BeginCloseOutput(TimeSpan timeout, Threading.WaitCallback callback, object state)
        {
            Fx.Assert(callback != null, "callback should not be null.");

            IOThreadCancellationTokenSource cancellationTokenSource = new IOThreadCancellationTokenSource(timeout);
            Task task = this.CloseOutputAsync(cancellationTokenSource.Token);
            Fx.Assert(this.pendingWritingMessageException == null, "'pendingWritingMessageException' MUST be NULL at this point.");

            task.ContinueWith(t =>
            {
                try
                {
                    cancellationTokenSource.Dispose();
                    WebSocketHelper.ThrowExceptionOnTaskFailure(t, timeout, WebSocketHelper.CloseOperation);
                }
                catch (Exception error)
                {
                    // Intentionally not following the usual pattern to rethrow fatal exceptions.
                    // Any rethrown exception would just be ----ed, because nobody awaits the
                    // Task returned from ContinueWith in this case.

                    FxTrace.Exception.TraceHandledException(error, TraceEventType.Information);
                    this.pendingWritingMessageException = error;
                }
                finally
                {
                    callback.Invoke(state);
                }
            });

            return AsyncCompletionResult.Queued;
        }

        protected override void OnSendCore(Message message, TimeSpan timeout)
        {
            Fx.Assert(message != null, "message should not be null.");

            TimeoutHelper helper = new TimeoutHelper(timeout);
            WebSocketMessageType outgoingMessageType = GetWebSocketMessageType(message);

            if (this.IsStreamedOutput)
            {
                WebSocketStream webSocketStream = new WebSocketStream(this.WebSocket, outgoingMessageType, helper.RemainingTime());
                TimeoutStream timeoutStream = new TimeoutStream(webSocketStream, ref helper);
                this.MessageEncoder.WriteMessage(message, timeoutStream);
                webSocketStream.WriteEndOfMessage(helper.RemainingTime());
            }
            else
            {
                ArraySegment<byte> messageData = this.EncodeMessage(message);
                bool success = false;
                try
                {
                    if (TD.WebSocketAsyncWriteStartIsEnabled())
                    {
                        TD.WebSocketAsyncWriteStart(
                            this.WebSocket.GetHashCode(),
                            messageData.Count,
                            this.RemoteAddress != null ? this.RemoteAddress.ToString() : string.Empty);
                    }

                    Task task = this.WebSocket.SendAsync(messageData, outgoingMessageType, true, CancellationToken.None);
                    task.Wait(helper.RemainingTime(), WebSocketHelper.ThrowCorrectException, WebSocketHelper.SendOperation);

                    if (TD.WebSocketAsyncWriteStopIsEnabled())
                    {
                        TD.WebSocketAsyncWriteStop(this.webSocket.GetHashCode());
                    }

                    success = true;
                }
                finally
                {
                    try
                    {
                        this.BufferManager.ReturnBuffer(messageData.Array);
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex) || success)
                        {
                            throw;
                        }

                        FxTrace.Exception.TraceUnhandledException(ex);
                    }
                }
            }
        }

        protected override ArraySegment<byte> EncodeMessage(Message message)
        {
            return MessageEncoder.WriteMessage(message, int.MaxValue, this.BufferManager, 0);
        }

        protected void Cleanup()
        {
            if (Interlocked.CompareExchange(ref this.cleanupStatus, WebSocketHelper.OperationFinished, WebSocketHelper.OperationNotStarted) == WebSocketHelper.OperationNotStarted)
            {
                this.OnCleanup();
            }
        }

        protected virtual void OnCleanup()
        {
            Fx.Assert(this.cleanupStatus == WebSocketHelper.OperationFinished, 
                "This method should only be called by this.Cleanup(). Make sure that you never call overriden OnCleanup()-methods directly in subclasses");
            if (this.shouldDisposeWebSocketAfterClosed && this.webSocket != null)
            {
                this.webSocket.Dispose();
            }

            if (this.internalBuffer != null)
            {
                this.bufferPool.Return(this.internalBuffer);
                this.internalBuffer = null;
            }
        }

        private static void ThrowOnPendingException(ref Exception pendingException)
        {
            Exception exceptionToThrow = pendingException;

            if (exceptionToThrow != null)
            {
                pendingException = null;
                throw FxTrace.Exception.AsError(exceptionToThrow);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule, Justification = "The exceptions thrown here are already wrapped.")]
        private Task CloseAsync()
        {
            try
            {
                return this.WebSocket.CloseAsync(this.webSocketCloseDetails.OutputCloseStatus, this.webSocketCloseDetails.OutputCloseStatusDescription, CancellationToken.None);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                throw WebSocketHelper.ConvertAndTraceException(e);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.WrapExceptionsRule, Justification = "The exceptions thrown here are already wrapped.")]
        private Task CloseOutputAsync(CancellationToken cancellationToken)
        {
            try
            {
                return this.WebSocket.CloseOutputAsync(this.webSocketCloseDetails.OutputCloseStatus, this.webSocketCloseDetails.OutputCloseStatusDescription, cancellationToken);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                throw WebSocketHelper.ConvertAndTraceException(e);
            }
        }

        static WebSocketMessageType GetWebSocketMessageType(Message message)
        {
            WebSocketMessageType outgoingMessageType = WebSocketDefaults.DefaultWebSocketMessageType;
            WebSocketMessageProperty webSocketMessageProperty;
            if (message.Properties.TryGetValue<WebSocketMessageProperty>(WebSocketMessageProperty.Name, out webSocketMessageProperty))
            {
                outgoingMessageType = webSocketMessageProperty.MessageType;
            }
            return outgoingMessageType;
        }

        static void StreamWriteCallback(IAsyncResult ar)
        {
            if (ar.CompletedSynchronously)
            {
                return;
            }

            WebSocketTransportDuplexSessionChannel thisPtr = (WebSocketTransportDuplexSessionChannel)ar.AsyncState;

            try
            {
                thisPtr.MessageEncoder.EndWriteMessage(ar);

                // We are goverend here by the TimeoutStream, no need to pass a CancellationToken here. 
                thisPtr.webSocketStream.WriteEndOfMessage(TimeSpan.MaxValue);

                thisPtr.waitCallback.Invoke(thisPtr.state);
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                thisPtr.AddPendingException(ex);
            }
        }

        protected class WebSocketMessageSource : IMessageSource
        {
            static readonly Action<object> onAsyncReceiveCancelled = Fx.ThunkCallback<object>(OnAsyncReceiveCancelled);
            MessageEncoder encoder;
            BufferManager bufferManager;
            EndpointAddress localAddress;
            Message pendingMessage;
            Exception pendingException;
            WebSocketContext context;
            WebSocket webSocket;
            bool closureReceived = false;
            bool useStreaming;
            int receiveBufferSize;
            int maxBufferSize;
            long maxReceivedMessageSize;
            TaskCompletionSource<object> streamWaitTask;
            IDefaultCommunicationTimeouts defaultTimeouts;
            RemoteEndpointMessageProperty remoteEndpointMessageProperty;
            SecurityMessageProperty handshakeSecurityMessageProperty;
            WebSocketCloseDetails closeDetails;
            ReadOnlyDictionary<string, object> properties;
            TimeSpan asyncReceiveTimeout;
            TaskCompletionSource<object> receiveTask;
            IOThreadTimer receiveTimer;
            int asyncReceiveState;

            public WebSocketMessageSource(WebSocketTransportDuplexSessionChannel webSocketTransportDuplexSessionChannel, WebSocket webSocket,
                    bool useStreaming, IDefaultCommunicationTimeouts defaultTimeouts)
            {
                this.Initialize(webSocketTransportDuplexSessionChannel, webSocket, useStreaming, defaultTimeouts);

                this.StartNextReceiveAsync();
            }

            public WebSocketMessageSource(WebSocketTransportDuplexSessionChannel webSocketTransportDuplexSessionChannel, WebSocketContext context,
                bool isStreamed, RemoteEndpointMessageProperty remoteEndpointMessageProperty, IDefaultCommunicationTimeouts defaultTimeouts, HttpRequestMessage requestMessage)
            {
                this.Initialize(webSocketTransportDuplexSessionChannel, context.WebSocket, isStreamed, defaultTimeouts);

                IPrincipal user = requestMessage == null ? null : requestMessage.GetUserPrincipal();
                this.context = new ServiceWebSocketContext(context, user);
                this.remoteEndpointMessageProperty = remoteEndpointMessageProperty;
                this.properties = requestMessage == null? null : new ReadOnlyDictionary<string, object>(requestMessage.Properties);

                this.StartNextReceiveAsync();
            }

            void Initialize(WebSocketTransportDuplexSessionChannel webSocketTransportDuplexSessionChannel, WebSocket webSocket, bool useStreaming, IDefaultCommunicationTimeouts defaultTimeouts)
            {
                this.webSocket = webSocket;
                this.encoder = webSocketTransportDuplexSessionChannel.MessageEncoder;
                this.bufferManager = webSocketTransportDuplexSessionChannel.BufferManager;
                this.localAddress = webSocketTransportDuplexSessionChannel.LocalAddress;
                this.maxBufferSize = webSocketTransportDuplexSessionChannel.MaxBufferSize;
                this.handshakeSecurityMessageProperty = webSocketTransportDuplexSessionChannel.RemoteSecurity;
                this.maxReceivedMessageSize = webSocketTransportDuplexSessionChannel.TransportFactorySettings.MaxReceivedMessageSize;
                this.receiveBufferSize = Math.Min(WebSocketHelper.GetReceiveBufferSize(this.maxReceivedMessageSize), this.maxBufferSize);
                this.useStreaming = useStreaming;
                this.defaultTimeouts = defaultTimeouts;
                this.closeDetails = webSocketTransportDuplexSessionChannel.webSocketCloseDetails;
                this.receiveTimer = new IOThreadTimer(onAsyncReceiveCancelled, this, true);
                this.asyncReceiveState = AsyncReceiveState.Finished;
            }

            internal RemoteEndpointMessageProperty RemoteEndpointMessageProperty
            {
                get { return this.remoteEndpointMessageProperty; }
            }

            static void OnAsyncReceiveCancelled(object target)
            {
                WebSocketMessageSource messageSource = (WebSocketMessageSource)target;
                messageSource.AsyncReceiveCancelled();
            }

            void AsyncReceiveCancelled()
            {
                if (Interlocked.CompareExchange(ref this.asyncReceiveState, AsyncReceiveState.Cancelled, AsyncReceiveState.Started) == AsyncReceiveState.Started)
                {
                    this.receiveTask.SetResult(null);
                }
            }

            public AsyncReceiveResult BeginReceive(TimeSpan timeout, WaitCallback callback, object state)
            {
                Fx.Assert(callback != null, "callback should not be null.");

                if (this.receiveTask.Task.IsCompleted)
                {
                    return AsyncReceiveResult.Completed;
                }
                else
                {
                    this.asyncReceiveTimeout = timeout;
                    this.receiveTimer.Set(timeout);
                    this.receiveTask.Task.ContinueWith(t =>
                        {
                            callback.Invoke(state);
                        });

                    return AsyncReceiveResult.Pending;
                }
            }

            public Message EndReceive()
            {
                if (this.asyncReceiveState == AsyncReceiveState.Cancelled)
                {
                    throw FxTrace.Exception.AsError(WebSocketHelper.GetTimeoutException(null, this.asyncReceiveTimeout, WebSocketHelper.ReceiveOperation));
                }
                else
                {
                    // IOThreadTimer.Cancel() will return false if we called IOThreadTimer.Set(Timespan.MaxValue) here, so we cannot reply on the return value of Cancel()
                    // call to see if Cancel() is fired or not. CSDMain 262179 filed for this.
                    this.receiveTimer.Cancel();
                    Fx.Assert(this.asyncReceiveState == AsyncReceiveState.Finished, "this.asyncReceiveState is not AsyncReceiveState.Finished: " + this.asyncReceiveState);
                    Message message = this.GetPendingMessage();

                    if (message != null)
                    {
                        // If we get any exception thrown out before that, the channel will be aborted thus no need to maintain the receive loop here.
                        this.StartNextReceiveAsync();
                    }

                    return message;
                }
            }

            public Message Receive(TimeSpan timeout)
            {
                bool waitingResult = this.receiveTask.Task.Wait(timeout);
                ThrowOnPendingException(ref this.pendingException);

                if (!waitingResult)
                {
                    throw FxTrace.Exception.AsError(new TimeoutException(
                               SR.GetString(SR.WaitForMessageTimedOut, timeout),
                               ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
                }

                Message message = this.GetPendingMessage();

                if (message != null)
                {
                    this.StartNextReceiveAsync();
                }

                return message;
            }

            public void UpdateOpenNotificationMessageProperties(MessageProperties messageProperties)
            {
                this.AddMessageProperties(messageProperties, WebSocketDefaults.DefaultWebSocketMessageType);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103",
                            Justification = "The exceptions are wrapped already.")]
            async Task ReadBufferedMessageAsync()
            {
                byte[] internalBuffer = null;
                try
                {
                    internalBuffer = this.bufferManager.TakeBuffer(this.receiveBufferSize);

                    int receivedByteCount = 0;
                    bool endOfMessage = false;
                    WebSocketReceiveResult result = null;
                    do
                    {
                        try
                        {

                            if (TD.WebSocketAsyncReadStartIsEnabled())
                            {
                                TD.WebSocketAsyncReadStart(this.webSocket.GetHashCode());
                            }

                            Task<WebSocketReceiveResult> receiveTask = this.webSocket.ReceiveAsync(
                                                            new ArraySegment<byte>(internalBuffer, receivedByteCount, internalBuffer.Length - receivedByteCount),
                                                            CancellationToken.None);

                            await receiveTask.ContinueOnCapturedContextFlow<WebSocketReceiveResult>();

                            result = receiveTask.Result;
                            this.CheckCloseStatus(result);
                            endOfMessage = result.EndOfMessage;

                            receivedByteCount += result.Count;
                            if (receivedByteCount >= internalBuffer.Length && !result.EndOfMessage)
                            {
                                if (internalBuffer.Length >= this.maxBufferSize)
                                {
                                    this.pendingException = FxTrace.Exception.AsError(new QuotaExceededException(SR.GetString(SR.MaxReceivedMessageSizeExceeded, this.maxBufferSize)));
                                    return;
                                }

                                int newSize = (int)Math.Min(((double)internalBuffer.Length) * 2, this.maxBufferSize);
                                Fx.Assert(newSize > 0, "buffer size should be larger than zero.");
                                byte[] newBuffer = this.bufferManager.TakeBuffer(newSize);
                                Buffer.BlockCopy(internalBuffer, 0, newBuffer, 0, receivedByteCount);
                                this.bufferManager.ReturnBuffer(internalBuffer);
                                internalBuffer = newBuffer;
                            }

                            if (TD.WebSocketAsyncReadStopIsEnabled())
                            {
                                TD.WebSocketAsyncReadStop(
                                    this.webSocket.GetHashCode(),
                                    receivedByteCount,
                                    TraceUtility.GetRemoteEndpointAddressPort(this.RemoteEndpointMessageProperty));
                            }
                        }
                        catch (AggregateException ex)
                        {
                            WebSocketHelper.ThrowCorrectException(ex, TimeSpan.MaxValue, WebSocketHelper.ReceiveOperation);
                        }

                    }
                    while (!endOfMessage && !this.closureReceived);

                    byte[] buffer = null;
                    bool success = false;
                    try
                    {
                        buffer = this.bufferManager.TakeBuffer(receivedByteCount);
                        Buffer.BlockCopy(internalBuffer, 0, buffer, 0, receivedByteCount);
                        Fx.Assert(result != null, "Result should not be null");
                        this.pendingMessage = this.PrepareMessage(result, buffer, receivedByteCount);
                        success = true;
                    }
                    finally
                    {
                        if (buffer != null && (!success || this.pendingMessage == null))
                        {
                            this.bufferManager.ReturnBuffer(buffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    this.pendingException = WebSocketHelper.ConvertAndTraceException(ex, TimeSpan.MaxValue, WebSocketHelper.ReceiveOperation);
                }
                finally
                {
                    if (internalBuffer != null)
                    {
                        this.bufferManager.ReturnBuffer(internalBuffer);
                    }
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103",
                            Justification = "The exceptions are wrapped already.")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                            Justification = "The exceptions are traced already.")]
            public AsyncReceiveResult BeginWaitForMessage(TimeSpan timeout, Threading.WaitCallback callback, object state)
            {
                try
                {
                    return this.BeginReceive(timeout, callback, state);
                }
                catch (TimeoutException ex)
                {
                    this.pendingException = FxTrace.Exception.AsError(ex);
                    return AsyncReceiveResult.Completed;
                }
            }

            public bool EndWaitForMessage()
            {
                try
                {
                    Message message = this.EndReceive();
                    this.pendingMessage = message;
                    return true;
                }
                catch (TimeoutException ex)
                {
                    if (TD.ReceiveTimeoutIsEnabled())
                    {
                        TD.ReceiveTimeout(ex.Message);
                    }

                    DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);

                    return false;
                }
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                try
                {
                    Message message = this.Receive(timeout);
                    this.pendingMessage = message;
                    return true;
                }
                catch (TimeoutException exception)
                {
                    if (TD.ReceiveTimeoutIsEnabled())
                    {
                        TD.ReceiveTimeout(exception.Message);
                    }

                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);

                    return false;
                }
            }

            internal void FinishUsingMessageStream(Exception ex)
            {
                //// The pattern of the task here is:
                //// 1) Only one thread can get the stream and consume the stream. A new task will be created at the moment it takes the stream
                //// 2) Only one another thread can enter the lock and wait on the task
                //// 3) The cleanup on the stream will return the stream to message source. And the cleanup call is limited to be called only once.
                if (ex != null && this.pendingException == null)
                {
                    this.pendingException = ex;
                }

                this.streamWaitTask.SetResult(null);
            }

            internal void CheckCloseStatus(WebSocketReceiveResult result)
            {
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (TD.WebSocketCloseStatusReceivedIsEnabled())
                    {
                        TD.WebSocketCloseStatusReceived(
                            this.webSocket.GetHashCode(),
                            result.CloseStatus.ToString());
                    }

                    this.closureReceived = true;
                    this.closeDetails.InputCloseStatus = result.CloseStatus;
                    this.closeDetails.InputCloseStatusDescription = result.CloseStatusDescription;
                }
            }

            async void StartNextReceiveAsync()
            {
                Fx.Assert(this.receiveTask == null || this.receiveTask.Task.IsCompleted, "this.receiveTask is not completed.");
                this.receiveTask = new TaskCompletionSource<object>();
                int currentState = Interlocked.CompareExchange(ref this.asyncReceiveState, AsyncReceiveState.Started, AsyncReceiveState.Finished);
                Fx.Assert(currentState == AsyncReceiveState.Finished, "currentState is not AsyncReceiveState.Finished: " + currentState);
                if (currentState != AsyncReceiveState.Finished)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException());
                }

                try
                {
                    if (this.useStreaming)
                    {
                        if (this.streamWaitTask != null)
                        {
                            //// Wait until the previous stream message finished.
                            await this.streamWaitTask.Task.ContinueOnCapturedContextFlow<object>();
                        }

                        this.streamWaitTask = new TaskCompletionSource<object>();
                    }

                    if (this.pendingException == null)
                    {
                        if (!this.useStreaming)
                        {
                            await this.ReadBufferedMessageAsync();
                        }
                        else
                        {
                            byte[] buffer = this.bufferManager.TakeBuffer(this.receiveBufferSize);
                            bool success = false;
                            try
                            {
                                if (TD.WebSocketAsyncReadStartIsEnabled())
                                {
                                    TD.WebSocketAsyncReadStart(this.webSocket.GetHashCode());
                                }

                                try
                                {
                                    Task<WebSocketReceiveResult> receiveTask = this.webSocket.ReceiveAsync(
                                                        new ArraySegment<byte>(buffer, 0, this.receiveBufferSize),
                                                        CancellationToken.None);
                                    await receiveTask.ContinueOnCapturedContextFlow<WebSocketReceiveResult>();

                                    WebSocketReceiveResult result = receiveTask.Result;
                                    this.CheckCloseStatus(result);
                                    this.pendingMessage = this.PrepareMessage(result, buffer, result.Count);

                                    if (TD.WebSocketAsyncReadStopIsEnabled())
                                    {
                                        TD.WebSocketAsyncReadStop(
                                            this.webSocket.GetHashCode(),
                                            result.Count,
                                            TraceUtility.GetRemoteEndpointAddressPort(this.remoteEndpointMessageProperty));
                                    }
                                }
                                catch (AggregateException ex)
                                {
                                    WebSocketHelper.ThrowCorrectException(ex, this.asyncReceiveTimeout, WebSocketHelper.ReceiveOperation);
                                }
                                success = true;
                            }
                            catch (Exception ex)
                            {
                                if (Fx.IsFatal(ex))
                                {
                                    throw;
                                }

                                this.pendingException = WebSocketHelper.ConvertAndTraceException(ex, this.asyncReceiveTimeout, WebSocketHelper.ReceiveOperation);
                            }
                            finally
                            {
                                if (!success)
                                {
                                    this.bufferManager.ReturnBuffer(buffer);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    if (Interlocked.CompareExchange(ref this.asyncReceiveState, AsyncReceiveState.Finished, AsyncReceiveState.Started) == AsyncReceiveState.Started)
                    {
                        this.receiveTask.SetResult(null);
                    }
                }
            }

            void AddMessageProperties(MessageProperties messageProperties, WebSocketMessageType incomingMessageType)
            {
                Fx.Assert(messageProperties != null, "messageProperties should not be null.");
                WebSocketMessageProperty messageProperty = new WebSocketMessageProperty(
                                                                this.context,
                                                                this.webSocket.SubProtocol,
                                                                incomingMessageType,
                                                                this.properties);
                messageProperties.Add(WebSocketMessageProperty.Name, messageProperty);

                if (this.remoteEndpointMessageProperty != null)
                {
                    messageProperties.Add(RemoteEndpointMessageProperty.Name, this.remoteEndpointMessageProperty);
                }

                if (this.handshakeSecurityMessageProperty != null)
                {
                    messageProperties.Security = (SecurityMessageProperty)this.handshakeSecurityMessageProperty.CreateCopy();
                }
            }

            Message GetPendingMessage()
            {
                ThrowOnPendingException(ref this.pendingException);

                if (this.pendingMessage != null)
                {
                    Message pendingMessage = this.pendingMessage;
                    this.pendingMessage = null;
                    return pendingMessage;
                }

                return null;
            }

            Message PrepareMessage(WebSocketReceiveResult result, byte[] buffer, int count)
            {
                if (result.MessageType != WebSocketMessageType.Close)
                {
                    Message message;
                    if (this.useStreaming)
                    {
                        TimeoutHelper readTimeoutHelper = new TimeoutHelper(this.defaultTimeouts.ReceiveTimeout);
                        message = this.encoder.ReadMessage(
                            new MaxMessageSizeStream(
                                new TimeoutStream(
                                    new WebSocketStream(
                                        this,
                                        new ArraySegment<byte>(buffer, 0, count),
                                        this.webSocket,
                                        result.EndOfMessage,
                                        this.bufferManager,
                                        this.defaultTimeouts.CloseTimeout),
                                    ref readTimeoutHelper),
                                this.maxReceivedMessageSize),
                            this.maxBufferSize);
                    }
                    else
                    {
                        ArraySegment<byte> bytes = new ArraySegment<byte>(buffer, 0, count);
                        message = this.encoder.ReadMessage(bytes, this.bufferManager);
                    }

                    if (message.Version.Addressing != AddressingVersion.None || !this.localAddress.IsAnonymous)
                    {
                        this.localAddress.ApplyTo(message);
                    }

                    if (message.Version.Addressing == AddressingVersion.None && message.Headers.Action == null)
                    {
                        if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            message.Headers.Action = WebSocketTransportSettings.BinaryMessageReceivedAction;
                        }
                        else
                        {
                            // WebSocketMesssageType should always be binary or text at this moment. The layer below us will help protect this.
                            Fx.Assert(result.MessageType == WebSocketMessageType.Text, "result.MessageType must be WebSocketMessageType.Text.");
                            message.Headers.Action = WebSocketTransportSettings.TextMessageReceivedAction;
                        }
                    }

                    if (message != null)
                    {
                        this.AddMessageProperties(message.Properties, result.MessageType);
                    }

                    return message;
                }

                return null;
            }

            static class AsyncReceiveState
            {
                internal const int Started = 0;
                internal const int Finished = 1;
                internal const int Cancelled = 2;
            }
        }

        class WebSocketStream : Stream
        {
            WebSocket webSocket;
            WebSocketMessageSource messageSource;
            TimeSpan closeTimeout;
            ArraySegment<byte> initialReadBuffer;
            bool endOfMessageReached = false;
            bool isForRead;
            bool endofMessageReceived;
            WebSocketMessageType outgoingMessageType;
            BufferManager bufferManager;
            int messageSourceCleanState;
            int endOfMessageWritten;
            int readTimeout;
            int writeTimeout;

            public WebSocketStream(
                        WebSocketMessageSource messageSource,
                        ArraySegment<byte> initialBuffer,
                        WebSocket webSocket,
                        bool endofMessageReceived,
                        BufferManager bufferManager,
                        TimeSpan closeTimeout)
                : this(webSocket, WebSocketDefaults.DefaultWebSocketMessageType, closeTimeout)
            {
                Fx.Assert(messageSource != null, "messageSource should not be null.");
                this.messageSource = messageSource;
                this.initialReadBuffer = initialBuffer;
                this.isForRead = true;
                this.endofMessageReceived = endofMessageReceived;
                this.bufferManager = bufferManager;
                this.messageSourceCleanState = WebSocketHelper.OperationNotStarted;
                this.endOfMessageWritten = WebSocketHelper.OperationNotStarted;
            }

            public WebSocketStream(
                    WebSocket webSocket,
                    WebSocketMessageType outgoingMessageType,
                    TimeSpan closeTimeout)
            {
                Fx.Assert(webSocket != null, "webSocket should not be null.");
                this.webSocket = webSocket;
                this.isForRead = false;
                this.outgoingMessageType = outgoingMessageType;
                this.messageSourceCleanState = WebSocketHelper.OperationFinished;
                this.closeTimeout = closeTimeout;
            }

            public override bool CanRead
            {
                get { return this.isForRead; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanTimeout
            {
                get
                {
                    return true;
                }
            }

            public override bool CanWrite
            {
                get { return !this.isForRead; }
            }

            public override long Length
            {
                get { throw FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(SR.SeekNotSupported))); }
            }

            public override long Position
            {
                get
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
                }

                set
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
                }
            }

            public override int ReadTimeout
            {
                get
                {
                    return this.readTimeout;
                }

                set
                {
                    Fx.Assert(value >= 0, "ReadTimeout should not be negative.");
                    this.readTimeout = value;
                }
            }

            public override int WriteTimeout
            {
                get
                {
                    return this.writeTimeout;
                }

                set
                {
                    Fx.Assert(value >= 0, "WriteTimeout should not be negative.");
                    this.writeTimeout = value;
                }
            }

            public override void Close()
            {
                TimeoutHelper helper = new TimeoutHelper(this.closeTimeout);
                base.Close();
                this.Cleanup(helper.RemainingTime());
            }

            public override void Flush()
            {
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                Fx.Assert(this.messageSource != null, "messageSource should not be null in read case.");

                if (this.ReadTimeout <= 0)
                { 
                    throw FxTrace.Exception.AsError(WebSocketHelper.GetTimeoutException(null, TimeoutHelper.FromMilliseconds(this.ReadTimeout), WebSocketHelper.ReceiveOperation));
                }

                TimeoutHelper helper = new TimeoutHelper(TimeoutHelper.FromMilliseconds(this.ReadTimeout));

                if (this.endOfMessageReached)
                {
                    return new CompletedAsyncResult<int>(0, callback, state);
                }

                if (this.initialReadBuffer.Count != 0)
                {
                    int bytesRead = this.GetBytesFromInitialReadBuffer(buffer, offset, count);
                    return new CompletedAsyncResult<int>(bytesRead, callback, state);
                }

                if (this.endofMessageReceived)
                {
                    this.endOfMessageReached = true;
                    return new CompletedAsyncResult<int>(0, callback, state);
                }

                if (TD.WebSocketAsyncReadStartIsEnabled())
                {
                    TD.WebSocketAsyncReadStart(this.webSocket.GetHashCode());
                }

                IOThreadCancellationTokenSource cancellationTokenSource = new IOThreadCancellationTokenSource(helper.RemainingTime());
                Task<int> task = this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), cancellationTokenSource.Token).ContinueWith(t =>
                {
                    cancellationTokenSource.Dispose();
                    WebSocketHelper.ThrowExceptionOnTaskFailure(t, TimeoutHelper.FromMilliseconds(this.ReadTimeout), WebSocketHelper.ReceiveOperation);
                    this.endOfMessageReached = t.Result.EndOfMessage;

                    int receivedBytes = t.Result.Count;
                    CheckResultAndEnsureNotCloseMessage(this.messageSource, t.Result);

                    if (this.endOfMessageReached)
                    {
                        this.Cleanup(helper.RemainingTime());
                    }

                    if (TD.WebSocketAsyncReadStopIsEnabled())
                    {
                        TD.WebSocketAsyncReadStop(
                            this.webSocket.GetHashCode(),
                            receivedBytes,
                            this.messageSource != null ? TraceUtility.GetRemoteEndpointAddressPort(this.messageSource.RemoteEndpointMessageProperty) : string.Empty);
                    }

                    return receivedBytes;
                }, TaskContinuationOptions.None);

                return task.AsAsyncResult<int>(callback, state);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                Task<int> task = (Task<int>)asyncResult;
                WebSocketHelper.ThrowExceptionOnTaskFailure((Task)task, TimeoutHelper.FromMilliseconds(this.ReadTimeout), WebSocketHelper.ReceiveOperation);
                return task.Result;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                    Justification = "The exceptions will be traced and thrown by the handling method.")]
            public override int Read(byte[] buffer, int offset, int count)
            {
                Fx.Assert(this.messageSource != null, "messageSource should not be null in read case.");

                if (this.ReadTimeout <= 0)
                { 
                    throw FxTrace.Exception.AsError(WebSocketHelper.GetTimeoutException(null, TimeoutHelper.FromMilliseconds(this.ReadTimeout), WebSocketHelper.ReceiveOperation));
                }

                TimeoutHelper helper = new TimeoutHelper(TimeoutHelper.FromMilliseconds(this.ReadTimeout));

                if (this.endOfMessageReached)
                {
                    return 0;
                }

                if (this.initialReadBuffer.Count != 0)
                {
                    return this.GetBytesFromInitialReadBuffer(buffer, offset, count);
                }

                int receivedBytes = 0;
                if (this.endofMessageReceived)
                {
                    this.endOfMessageReached = true;
                }
                else
                {
                    if (TD.WebSocketAsyncReadStartIsEnabled())
                    {
                        TD.WebSocketAsyncReadStart(this.webSocket.GetHashCode());
                    }

                    Task<WebSocketReceiveResult> task = this.webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, offset, count), CancellationToken.None);
                    task.Wait(helper.RemainingTime(), WebSocketHelper.ThrowCorrectException, WebSocketHelper.ReceiveOperation);
                    if (task.Result.EndOfMessage)
                    {
                        this.endofMessageReceived = true;
                        this.endOfMessageReached = true;
                    }

                    receivedBytes = task.Result.Count;
                    CheckResultAndEnsureNotCloseMessage(this.messageSource, task.Result);

                    if (TD.WebSocketAsyncReadStopIsEnabled())
                    {
                        TD.WebSocketAsyncReadStop(
                            this.webSocket.GetHashCode(),
                            receivedBytes,
                            this.messageSource != null ? TraceUtility.GetRemoteEndpointAddressPort(this.messageSource.RemoteEndpointMessageProperty) : string.Empty);
                    }
                }

                if (this.endOfMessageReached)
                {
                    this.Cleanup(helper.RemainingTime());
                }

                return receivedBytes;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            public override void SetLength(long value)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (this.endOfMessageWritten == WebSocketHelper.OperationFinished)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.WebSocketStreamWriteCalledAfterEOMSent)));
                }
                
                if (this.WriteTimeout <= 0)
                { 
                    throw FxTrace.Exception.AsError(WebSocketHelper.GetTimeoutException(null, TimeoutHelper.FromMilliseconds(this.WriteTimeout), WebSocketHelper.SendOperation));
                }

                if (TD.WebSocketAsyncWriteStartIsEnabled())
                {
                    TD.WebSocketAsyncWriteStart(
                            this.webSocket.GetHashCode(),
                            count,
                            this.messageSource != null ? TraceUtility.GetRemoteEndpointAddressPort(this.messageSource.RemoteEndpointMessageProperty) : string.Empty);
                }

                Task task = this.webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), this.outgoingMessageType, false, CancellationToken.None);
                task.Wait(TimeoutHelper.FromMilliseconds(this.WriteTimeout), WebSocketHelper.ThrowCorrectException, WebSocketHelper.SendOperation);

                if (TD.WebSocketAsyncWriteStopIsEnabled())
                {
                    TD.WebSocketAsyncWriteStop(this.webSocket.GetHashCode());
                }
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                if (this.endOfMessageWritten == WebSocketHelper.OperationFinished)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.WebSocketStreamWriteCalledAfterEOMSent)));
                }

                if (this.WriteTimeout <= 0)
                {
                    throw FxTrace.Exception.AsError(WebSocketHelper.GetTimeoutException(null, TimeoutHelper.FromMilliseconds(this.WriteTimeout), WebSocketHelper.SendOperation));
                }

                if (TD.WebSocketAsyncWriteStartIsEnabled())
                {
                    TD.WebSocketAsyncWriteStart(
                            this.webSocket.GetHashCode(),
                            count,
                            this.messageSource != null ? TraceUtility.GetRemoteEndpointAddressPort(this.messageSource.RemoteEndpointMessageProperty) : string.Empty);
                }

                IOThreadCancellationTokenSource cancellationTokenSource = new IOThreadCancellationTokenSource(this.WriteTimeout);
                Task task = this.webSocket.SendAsync(new ArraySegment<byte>(buffer, offset, count), this.outgoingMessageType, false, cancellationTokenSource.Token).ContinueWith(t =>
                {
                    if (TD.WebSocketAsyncWriteStopIsEnabled())
                    {
                        TD.WebSocketAsyncWriteStop(this.webSocket.GetHashCode());
                    }

                    cancellationTokenSource.Dispose();
                    WebSocketHelper.ThrowExceptionOnTaskFailure(t, TimeoutHelper.FromMilliseconds(this.WriteTimeout), WebSocketHelper.SendOperation);
                });

                return task.AsAsyncResult(callback, state);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                Task task = (Task)asyncResult;
                WebSocketHelper.ThrowExceptionOnTaskFailure(task, TimeoutHelper.FromMilliseconds(this.WriteTimeout), WebSocketHelper.SendOperation);
            }

            public void WriteEndOfMessage(TimeSpan timeout)
            {
                if (TD.WebSocketAsyncWriteStartIsEnabled())
                {
                    TD.WebSocketAsyncWriteStart(
                            this.webSocket.GetHashCode(),
                            0,
                            this.messageSource != null ? TraceUtility.GetRemoteEndpointAddressPort(this.messageSource.RemoteEndpointMessageProperty) : string.Empty);
                }

                if (Interlocked.CompareExchange(ref this.endOfMessageWritten, WebSocketHelper.OperationFinished, WebSocketHelper.OperationNotStarted) == WebSocketHelper.OperationNotStarted)
                {
                    Task task = this.webSocket.SendAsync(new ArraySegment<byte>(EmptyArray<byte>.Instance, 0, 0), this.outgoingMessageType, true, CancellationToken.None);
                    task.Wait(timeout, WebSocketHelper.ThrowCorrectException, WebSocketHelper.SendOperation);
                }

                if (TD.WebSocketAsyncWriteStopIsEnabled())
                {
                    TD.WebSocketAsyncWriteStop(this.webSocket.GetHashCode());
                }
            }

            public async void WriteEndOfMessageAsync(TimeSpan timeout, WaitCallback callback, object state)
            {
                if (TD.WebSocketAsyncWriteStartIsEnabled())
                {
                    TD.WebSocketAsyncWriteStart(
                            this.webSocket.GetHashCode(),
                            0,
                            this.messageSource != null ? TraceUtility.GetRemoteEndpointAddressPort(this.messageSource.RemoteEndpointMessageProperty) : string.Empty);
                }

                using (IOThreadCancellationTokenSource cancellationTokenSource = new IOThreadCancellationTokenSource(timeout))
                {
                    try
                    {
                        Task task = this.webSocket.SendAsync(new ArraySegment<byte>(EmptyArray<byte>.Instance, 0, 0), this.outgoingMessageType, true, cancellationTokenSource.Token);

                        // The callback here will only be TransportDuplexSessionChannel.OnWriteComplete. It's safe to call this callback without flowing
                        // security context here since there's no user code involved.
                        await task.SuppressContextFlow();

                        if (TD.WebSocketAsyncWriteStopIsEnabled())
                        {
                            TD.WebSocketAsyncWriteStop(this.webSocket.GetHashCode());
                        }
                    }
                    catch(AggregateException ex)
                    {
                        WebSocketHelper.ThrowCorrectException(ex, TimeoutHelper.FromMilliseconds(this.WriteTimeout), WebSocketHelper.SendOperation);
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                        {
                            throw;
                        }

                        WebSocketHelper.ThrowCorrectException(ex);
                    }
                    finally
                    {
                        callback.Invoke(state);
                    }
                }
            }

            static void CheckResultAndEnsureNotCloseMessage(WebSocketMessageSource messageSource, WebSocketReceiveResult result)
            {
                messageSource.CheckCloseStatus(result);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw FxTrace.Exception.AsError(new ProtocolException(SR.GetString(SR.WebSocketUnexpectedCloseMessageError)));
                }
            }

            int GetBytesFromInitialReadBuffer(byte[] buffer, int offset, int count)
            {
                int bytesToCopy = this.initialReadBuffer.Count > count ? count : this.initialReadBuffer.Count;
                Buffer.BlockCopy(this.initialReadBuffer.Array, this.initialReadBuffer.Offset, buffer, offset, bytesToCopy);
                this.initialReadBuffer = new ArraySegment<byte>(this.initialReadBuffer.Array, this.initialReadBuffer.Offset + bytesToCopy, this.initialReadBuffer.Count - bytesToCopy);
                return bytesToCopy;
            }

            void Cleanup(TimeSpan timeout)
            {
                if (this.isForRead)
                {
                    if (Interlocked.CompareExchange(ref this.messageSourceCleanState, WebSocketHelper.OperationFinished, WebSocketHelper.OperationNotStarted) == WebSocketHelper.OperationNotStarted)
                    {
                        Exception pendingException = null;
                        try
                        {
                            if (!this.endofMessageReceived && (this.webSocket.State == WebSocketState.Open || this.webSocket.State == WebSocketState.CloseSent))
                            {
                                // Drain the reading stream
                                TimeoutHelper helper = new TimeoutHelper(timeout);
                                do
                                {
                                    Task<WebSocketReceiveResult> receiveTask = this.webSocket.ReceiveAsync(new ArraySegment<byte>(this.initialReadBuffer.Array), CancellationToken.None);
                                    receiveTask.Wait(helper.RemainingTime(), WebSocketHelper.ThrowCorrectException, WebSocketHelper.ReceiveOperation);
                                    this.endofMessageReceived = receiveTask.Result.EndOfMessage;
                                }
                                while (!this.endofMessageReceived && (this.webSocket.State == WebSocketState.Open || this.webSocket.State == WebSocketState.CloseSent));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Fx.IsFatal(ex))
                            {
                                throw;
                            }

                            // Not throwing out this exception during stream cleanup. The exception
                            // will be thrown out when we are trying to receive the next message using the same
                            // WebSocket object.
                            pendingException = WebSocketHelper.ConvertAndTraceException(ex, timeout, WebSocketHelper.CloseOperation);
                        }

                        this.bufferManager.ReturnBuffer(this.initialReadBuffer.Array);
                        Fx.Assert(this.messageSource != null, "messageSource should not be null.");
                        this.messageSource.FinishUsingMessageStream(pendingException);
                    }
                }
                else
                {
                    if (Interlocked.CompareExchange(ref this.endOfMessageWritten, WebSocketHelper.OperationFinished, WebSocketHelper.OperationNotStarted) == WebSocketHelper.OperationNotStarted)
                    {
                        this.WriteEndOfMessage(timeout);
                    }
                }
            }
        }

        class WebSocketCloseDetails : IWebSocketCloseDetails
        {
            WebSocketCloseStatus outputCloseStatus = WebSocketCloseStatus.NormalClosure;
            string outputCloseStatusDescription;
            WebSocketCloseStatus? inputCloseStatus;
            string inputCloseStatusDescription;

            public WebSocketCloseStatus? InputCloseStatus
            {
                get
                {
                    return this.inputCloseStatus;
                }

                internal set
                {
                    this.inputCloseStatus = value;
                }
            }

            public string InputCloseStatusDescription
            {
                get
                {
                    return this.inputCloseStatusDescription;
                }

                internal set
                {
                    this.inputCloseStatusDescription = value;
                }
            }

            internal WebSocketCloseStatus OutputCloseStatus
            {
                get
                {
                    return this.outputCloseStatus;
                }
            }

            internal string OutputCloseStatusDescription
            {
                get
                {
                    return this.outputCloseStatusDescription;
                }
            }

            public void SetOutputCloseStatus(WebSocketCloseStatus closeStatus, string closeStatusDescription)
            {
                this.outputCloseStatus = closeStatus;
                this.outputCloseStatusDescription = closeStatusDescription;
            }
        }
    }
}
