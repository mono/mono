// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    abstract class HttpPipeline
    {
        const string HttpPipelineKey = "ServiceModel.HttpPipeline";
        HttpRequestContext httpRequestContext;
        HttpInput httpInput;

        /// <summary>
        /// Indicates wheather the pipeline is closed (or closing) and it's used to prevent the Close method to be called concurrently.
        /// 0 = the pipeline is not closed (or closing)
        /// 1 = the pipeline is closed (or closing)
        /// </summary>
        private int isClosed = 0;

        public HttpPipeline(HttpRequestContext httpRequestContext)
        {
            this.httpRequestContext = httpRequestContext;
        }

        public HttpInput HttpInput
        {
            get
            {
                if (this.httpInput == null)
                {
                    this.httpInput = this.GetHttpInput();
                }

                return this.httpInput;
            }
        }

        internal bool IsHttpInputInitialized
        {
            get
            {
                return this.httpInput != null;
            }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.httpRequestContext.EventTraceActivity;
            }
        }

        protected HttpRequestContext HttpRequestContext
        {
            get
            {
                return this.httpRequestContext;
            }
        }

        public static HttpPipeline CreateHttpPipeline(HttpRequestContext httpRequestContext, TransportIntegrationHandler transportIntegrationHandler, bool isWebSocketTransport)
        {
            if (transportIntegrationHandler == null)
            {
                Fx.Assert(!isWebSocketTransport, "isWebSocketTransport should be false if there's no HTTP message handler existing.");

                if (httpRequestContext.HttpMessagesSupported)
                {
                    return new HttpMessageSupportedHttpPipeline(httpRequestContext);
                }

                return new EmptyHttpPipeline(httpRequestContext);
            }

            return NormalHttpPipeline.CreatePipeline(httpRequestContext, transportIntegrationHandler, isWebSocketTransport);
        }

        public static HttpPipeline GetHttpPipeline(HttpRequestMessage httpRequestMessage)
        {
            Fx.Assert(httpRequestMessage != null, "httpRequestMessage should not be null.");
            object obj;
            if (!httpRequestMessage.Properties.TryGetValue(HttpPipelineKey, out obj) || obj == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.HttpPipelineMessagePropertyMissingError, HttpPipelineKey)));
            }

            HttpPipeline httpPipeline = obj as HttpPipeline;

            if (httpPipeline == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.HttpPipelineMessagePropertyTypeError, HttpPipelineKey, typeof(HttpPipeline))));
            }

            return httpPipeline;
        }

        public static void RemoveHttpPipeline(HttpRequestMessage httpRequestMessage)
        {
            Fx.Assert(httpRequestMessage != null, "httpRequestMessage should not be null.");
            httpRequestMessage.Properties.Remove(HttpPipelineKey);
        }

        public abstract Task<HttpResponseMessage> Dispatch(HttpRequestMessage httpRequestMessage);

        public abstract void SendReply(Message message, TimeSpan timeout);

        public virtual AsyncCompletionResult SendAsyncReply(Message message, Action<object, HttpResponseMessage> asyncSendCallback, object state)
        {
            this.TraceProcessResponseStop();
            return AsyncCompletionResult.Completed;
        }

        public void Close()
        {
            if (Interlocked.Exchange(ref this.isClosed, 1) == 0)
            {
                this.OnClose();
            }
        }

        public virtual void Cancel()
        {
            this.httpRequestContext.Abort();
        }

        internal abstract IAsyncResult BeginProcessInboundRequest(ReplyChannelAcceptor replyChannelAcceptor, Action dequeuedCallback, AsyncCallback callback, object state);

        internal abstract void EndProcessInboundRequest(IAsyncResult result);

        protected abstract IAsyncResult BeginParseIncomingMessage(AsyncCallback asynCallback, object state);

        protected abstract Message EndParseIncomingMesssage(IAsyncResult result, out Exception requestException);

        protected abstract void OnParseComplete(Message message, Exception requestException);

        protected virtual void OnClose()
        {
        }

        protected void TraceProcessInboundRequestStart()
        {
            if (TD.HttpPipelineProcessInboundRequestStartIsEnabled())
            {
                TD.HttpPipelineProcessInboundRequestStart(this.EventTraceActivity);
            }
        }

        protected void TraceBeginProcessInboundRequestStart()
        {
            if (TD.HttpPipelineBeginProcessInboundRequestStartIsEnabled())
            {
                TD.HttpPipelineBeginProcessInboundRequestStart(this.EventTraceActivity);
            }
        }

        protected void TraceProcessInboundRequestStop()
        {
            if (TD.HttpPipelineProcessInboundRequestStopIsEnabled())
            {
                TD.HttpPipelineProcessInboundRequestStop(this.EventTraceActivity);
            }
        }

        protected void TraceProcessResponseStart()
        {
            if (TD.HttpPipelineProcessResponseStartIsEnabled())
            {
                TD.HttpPipelineProcessResponseStart(this.EventTraceActivity);
            }
        }

        protected void TraceBeginProcessResponseStart()
        {
            if (TD.HttpPipelineBeginProcessResponseStartIsEnabled())
            {
                TD.HttpPipelineBeginProcessResponseStart(this.EventTraceActivity);
            }
        }

        protected void TraceProcessResponseStop()
        {
            if (TD.HttpPipelineProcessResponseStopIsEnabled())
            {
                TD.HttpPipelineProcessResponseStop(this.EventTraceActivity);
            }
        }

        protected virtual HttpInput GetHttpInput()
        {
            return this.httpRequestContext.GetHttpInput(true);
        }

        protected HttpOutput GetHttpOutput(Message message)
        {
            return this.httpRequestContext.GetHttpOutputCore(message);
        }

        class EmptyHttpPipeline : HttpPipeline
        {
            static Action<object> onRequestInitializationTimeout = Fx.ThunkCallback<object>(OnRequestInitializationTimeout);
            IOThreadTimer requestInitializationTimer;
            bool requestInitializationTimerCancelled;

            public EmptyHttpPipeline(HttpRequestContext httpRequestContext)
                : base(httpRequestContext)
            {
                if (this.httpRequestContext.Listener.RequestInitializationTimeout != HttpTransportDefaults.RequestInitializationTimeout)
                {
                    this.requestInitializationTimer = new IOThreadTimer(onRequestInitializationTimeout, this, false);
                    this.requestInitializationTimer.Set(this.httpRequestContext.Listener.RequestInitializationTimeout);
                }
            }

            public override void SendReply(Message message, TimeSpan timeout)
            {
                // Make sure the timer was cancelled in the case we need to send the response back due to some errors happened during the time
                // we are processing the incoming request. From here the operation will be guarded by the SendTimeout.
                this.CancelRequestInitializationTimer();
                this.SendReplyCore(message, timeout);
            }

            public override Task<HttpResponseMessage> Dispatch(HttpRequestMessage httpRequestMessage)
            {
                // This method should never be called for an EmptyPipeline.
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            internal override IAsyncResult BeginProcessInboundRequest(
                ReplyChannelAcceptor replyChannelAcceptor,
                Action dequeuedCallback,
                AsyncCallback callback,
                object state)
            {
                this.TraceBeginProcessInboundRequestStart();
                return new EnqueueMessageAsyncResult(replyChannelAcceptor, dequeuedCallback, this, callback, state);
            }

            internal override void EndProcessInboundRequest(IAsyncResult result)
            {
                EnqueueMessageAsyncResult.End(result);
                this.TraceProcessInboundRequestStop();
            }

            protected override IAsyncResult BeginParseIncomingMessage(AsyncCallback asynCallback, object state)
            {
                return this.HttpInput.BeginParseIncomingMessage(asynCallback, state);
            }

            protected override Message EndParseIncomingMesssage(IAsyncResult result, out Exception requestException)
            {
                return this.HttpInput.EndParseIncomingMessage(result, out requestException);
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103:ThrowWrappedExceptionsRule",
                    Justification = "The exceptions wrapped here will be thrown out later.")]
            protected override void OnParseComplete(Message message, Exception requestException)
            {
                if (!this.CancelRequestInitializationTimer() && requestException == null)
                {
                    requestException = FxTrace.Exception.AsError(new TimeoutException(SR.GetString(
                                                    SR.RequestInitializationTimeoutReached,
                                                    this.HttpRequestContext.Listener.RequestInitializationTimeout,
                                                    "RequestInitializationTimeout",
                                                    typeof(HttpTransportBindingElement).Name)));
                }

                this.HttpRequestContext.SetMessage(message, requestException);
            }

            protected virtual void SendReplyCore(Message message, TimeSpan timeout)
            {
                this.TraceProcessResponseStart();
                ThreadTrace.Trace("Begin sending http reply");
                HttpOutput httpOutput = this.GetHttpOutput(message);
                httpOutput.Send(timeout);
                ThreadTrace.Trace("End sending http reply");
                this.TraceProcessResponseStop();
            }

            protected bool CancelRequestInitializationTimer()
            {
                if (this.requestInitializationTimer == null)
                {
                    return true;
                }

                if (this.requestInitializationTimerCancelled)
                {
                    return false;
                }

                bool result = this.requestInitializationTimer.Cancel();
                this.requestInitializationTimerCancelled = true;

                return result;
            }

            protected override void OnClose()
            {
                this.CancelRequestInitializationTimer();
            }

            static void OnRequestInitializationTimeout(object obj)
            {
                Fx.Assert(obj != null, "obj should not be null.");
                HttpPipeline thisPtr = (HttpPipeline)obj;
                thisPtr.Cancel();
            }
        }

        class HttpMessageSupportedHttpPipeline : EmptyHttpPipeline
        {
            HttpRequestMessageHttpInput httpRequestMessageHttpInput;

            public HttpMessageSupportedHttpPipeline(HttpRequestContext httpRequestContext)
                : base(httpRequestContext)
            {
            }

            public HttpRequestMessageHttpInput HttpRequestMessageHttpInput
            {
                get
                {
                    if (this.httpRequestMessageHttpInput == null)
                    {
                        this.httpRequestMessageHttpInput = this.HttpInput as HttpRequestMessageHttpInput;
                        Fx.Assert(this.httpRequestMessageHttpInput != null, "The 'HttpInput' field should always be of type 'HttpRequestMessageHttpInput'.");
                    }

                    return this.httpRequestMessageHttpInput;
                }
            }

            public HttpRequestMessage HttpRequestMessage
            {
                get
                {
                    return this.HttpRequestMessageHttpInput.HttpRequestMessage;
                }
            }

            protected override IAsyncResult BeginParseIncomingMessage(AsyncCallback asynCallback, object state)
            {
                return this.HttpRequestMessageHttpInput.BeginParseIncomingMessage(this.HttpRequestMessage, asynCallback, state);
            }

            protected override void SendReplyCore(Message message, TimeSpan timeout)
            {
                this.TraceProcessResponseStart();
                ThreadTrace.Trace("Begin sending http reply");
                HttpOutput httpOutput = this.GetHttpOutput(message);

                HttpResponseMessage response = HttpResponseMessageProperty.GetHttpResponseMessageFromMessage(message);
                if (response != null)
                {
                    httpOutput.Send(response, timeout);
                }
                else
                {
                    httpOutput.Send(timeout);
                }

                ThreadTrace.Trace("End sending http reply");
                this.TraceProcessResponseStop();
            }

            protected override HttpInput GetHttpInput()
            {
                return base.GetHttpInput().CreateHttpRequestMessageInput();
            }
        }

        class NormalHttpPipeline : HttpPipeline
        {
            static readonly HttpResponseMessage internalServerErrorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            static Action<object> onCreateMessageAndEnqueue = Fx.ThunkCallback<object>(OnCreateMessageAndEnqueue);
            static AsyncCallback onEnqueued = Fx.ThunkCallback(OnEnqueued);

            HttpRequestMessage httpRequestMessage;
            TransportIntegrationHandler transportIntegrationHandler;
            Task<HttpResponseMessage> transportIntegrationHandlerTask;
            TaskCompletionSource<HttpResponseMessage> channelModelIntegrationHandlerTask;
            ReplyChannelAcceptor replyChannelAcceptor;
            Action dequeuedCallback;
            bool isShortCutResponse = true;
            bool wasProcessInboundRequestSuccessful;
            bool isAsyncReply = false;
            TimeSpan defaultSendTimeout;
            HttpOutput httpOutput;
            object thisLock = new object();
            HttpPipelineCancellationTokenSource cancellationTokenSource;

            Action<object, HttpResponseMessage> asyncSendCallback;
            object asyncSendState;

            public NormalHttpPipeline(HttpRequestContext httpRequestContext, TransportIntegrationHandler transportIntegrationHandler)
                : base(httpRequestContext)
            {
                this.defaultSendTimeout = httpRequestContext.DefaultSendTimeout;

                this.cancellationTokenSource = new HttpPipelineCancellationTokenSource(httpRequestContext);
                Fx.Assert(transportIntegrationHandler != null, "transportIntegrationHandler should not be null.");
                this.transportIntegrationHandler = transportIntegrationHandler;
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            public static HttpPipeline CreatePipeline(HttpRequestContext httpRequestContext, TransportIntegrationHandler transportIntegrationHandler, bool isWebSocketTransport)
            {
                NormalHttpPipeline pipeline = isWebSocketTransport ? new WebSocketHttpPipeline(httpRequestContext, transportIntegrationHandler) :
                                                                     new NormalHttpPipeline(httpRequestContext, transportIntegrationHandler);
                pipeline.SetPipelineIncomingTimeout();
                return pipeline;
            }

            public override void SendReply(Message message, TimeSpan timeout)
            {
                this.TraceProcessResponseStart();
                TimeoutHelper helper = new TimeoutHelper(timeout);

                if (!this.isShortCutResponse)
                {
                    this.CompleteChannelModelIntegrationHandlerTask(message);

                    bool lockTaken = false;
                    try
                    {
                        // We need this lock only in [....] reply case. In this case, we hopped the thread in the request side, so it's possible to send the response here
                        // before the TransportIntegrationHandler is ready on another thread (thus a race condition). So we use the lock here. In the incoming path, we won't
                        // release the lock until the TransportIntegrationHandler is ready. Once we get the lock on the outgoing path, we can then call Wait() on this handler safely.
                        Monitor.TryEnter(this.ThisLock, TimeoutHelper.ToMilliseconds(helper.RemainingTime()), ref lockTaken);
                        if (!lockTaken)
                        {
                            throw FxTrace.Exception.AsError(new TimeoutException(SR.GetString(SR.TimeoutOnSend, timeout)));
                        }

                        this.WaitTransportIntegrationHandlerTask(helper.RemainingTime());
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(this.ThisLock);
                        }
                    }

                    if (this.transportIntegrationHandlerTask.Result != null)
                    {
                        this.httpOutput.Send(this.transportIntegrationHandlerTask.Result, helper.RemainingTime());
                    }
                }

                this.TraceProcessResponseStop();
            }

            public override AsyncCompletionResult SendAsyncReply(Message message, Action<object, HttpResponseMessage> asyncSendCallback, object state)
            {
                this.TraceBeginProcessResponseStart();
                this.isAsyncReply = true;
                this.asyncSendCallback = asyncSendCallback;
                this.asyncSendState = state;

                this.CompleteChannelModelIntegrationHandlerTask(message);
                return AsyncCompletionResult.Queued;
            }

            public override Task<HttpResponseMessage> Dispatch(HttpRequestMessage httpRequestMessage)
            {
                this.httpRequestMessage = httpRequestMessage;
                ((HttpRequestMessageHttpInput)this.HttpInput).SetHttpRequestMessage(httpRequestMessage);
                Fx.Assert(this.channelModelIntegrationHandlerTask == null, "channelModelIntegrationHandlerTask should be null.");
                this.channelModelIntegrationHandlerTask = new TaskCompletionSource<HttpResponseMessage>();
                ActionItem.Schedule(NormalHttpPipeline.onCreateMessageAndEnqueue, this);
                return this.channelModelIntegrationHandlerTask.Task;
            }

            public override void Cancel()
            {
                this.cancellationTokenSource.Cancel();
            }

            internal override IAsyncResult BeginProcessInboundRequest(ReplyChannelAcceptor replyChannelAcceptor, Action dequeuedCallback, AsyncCallback callback, object state)
            {
                try
                {
                    this.wasProcessInboundRequestSuccessful = false;
                    this.TraceProcessInboundRequestStart();
                    this.replyChannelAcceptor = replyChannelAcceptor;
                    this.dequeuedCallback = dequeuedCallback;
                    HttpRequestMessageHttpInput httpRequestMessageInput = (HttpRequestMessageHttpInput)this.HttpInput;

                    this.httpRequestMessage = httpRequestMessageInput.HttpRequestMessage;
                    this.httpRequestMessage.Properties.Add(HttpPipelineKey, this);

                    lock (this.ThisLock)
                    {
                        this.transportIntegrationHandlerTask = this.transportIntegrationHandler.ProcessPipelineAsync(this.httpRequestMessage, this.cancellationTokenSource.Token);
                    }

                    this.SendHttpPipelineResponse();
                    this.TraceProcessInboundRequestStop();
                    this.wasProcessInboundRequestSuccessful = true;

                    return new CompletedAsyncResult(callback, state);
                }
                catch (OperationCanceledException)
                {
                    if (TD.HttpPipelineFaultedIsEnabled())
                    {
                        TD.HttpPipelineFaulted(this.EventTraceActivity);
                    }

                    this.cancellationTokenSource.Cancel();
                    throw;
                }
                catch (Exception ex)
                {
                    if (!Fx.IsFatal(ex))
                    {
                        if (TD.HttpPipelineFaultedIsEnabled())
                        {
                            TD.HttpPipelineFaulted(this.EventTraceActivity);
                        }

                        this.SendAndClose(internalServerErrorHttpResponseMessage);
                    }

                    throw;
                }
            }

            internal override void EndProcessInboundRequest(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override IAsyncResult BeginParseIncomingMessage(AsyncCallback asynCallback, object state)
            {
                return this.HttpInput.BeginParseIncomingMessage(this.httpRequestMessage, asynCallback, state);
            }

            protected override Message EndParseIncomingMesssage(IAsyncResult result, out Exception requestException)
            {
                return this.HttpInput.EndParseIncomingMessage(result, out requestException);
            }

            protected override void OnParseComplete(Message message, Exception requestException)
            {
                this.cancellationTokenSource.CancelAfter(Timeout.Infinite);
                this.httpRequestContext.SetMessage(message, requestException);
                this.isShortCutResponse = false;
            }

            protected virtual void SetPipelineIncomingTimeout()
            {
                if (httpRequestContext.Listener.RequestInitializationTimeout != HttpTransportDefaults.RequestInitializationTimeout)
                {
                    this.cancellationTokenSource.CancelAfter(httpRequestContext.Listener.RequestInitializationTimeout);
                }
            }

            // The Close() method from the base class makes sure that this method is only called once.
            protected override void OnClose()
            {
                this.cancellationTokenSource.Dispose();

                // In HttpPipeline shortcut scenario or WebSocket scenario, we need to call the dequeueCallback in selfhost case
                // to start another receive loop on transport. Note that this dequeue callback should not be invoked earlier, else it
                // will lead to a potential DOS attack to the system.
                // HttpPipeline.Close() will always be called by HttpRequestContext.Abort() or Close()
                // But if the ProcessInboundRequest method call was not successful, the SharedHttpTransportManager will start the receiving loop.
                if (this.isShortCutResponse && this.wasProcessInboundRequestSuccessful && this.dequeuedCallback != null)
                {
                    this.dequeuedCallback.Invoke();
                }

                base.OnClose();
            }

            protected override HttpInput GetHttpInput()
            {
                HttpInput httpInput = base.GetHttpInput();

                return httpInput.CreateHttpRequestMessageInput();
            }

            protected virtual void SendHttpPipelineResponse()
            {
                this.transportIntegrationHandlerTask.ContinueWith(
                    t =>
                    {
                        if (t.Result != null)
                        {
                            if (this.isShortCutResponse)
                            {
                                this.cancellationTokenSource.Dispose();
                                this.wasProcessInboundRequestSuccessful = true;
                                //// shortcut scenario
                                //// Currently we are always doing [....] send even async send is enabled. 
                                this.SendAndClose(t.Result);
                            }
                            else if (this.isAsyncReply)
                            {
                                this.asyncSendCallback.Invoke(this.asyncSendState, t.Result);
                            }
                        }
                    },
                    TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
            }

            protected void SendAndClose(HttpResponseMessage httpResponseMessage)
            {
                this.HttpRequestContext.SendResponseAndClose(httpResponseMessage);
            }

            static void OnCreateMessageAndEnqueue(object state)
            {
                try
                {
                    NormalHttpPipeline pipeline = (NormalHttpPipeline)state;
                    Fx.Assert(pipeline != null, "pipeline should not be null.");
                    pipeline.CreateMessageAndEnqueue();
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(ex, TraceEventType.Information);
                }
            }

            static void OnEnqueued(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                try
                {
                    EnqueueMessageAsyncResult.End(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    FxTrace.Exception.TraceHandledException(ex, TraceEventType.Error);
                }
            }

            void CreateMessageAndEnqueue()
            {
                bool success = false;
                try
                {
                    Fx.Assert(this.replyChannelAcceptor != null, "acceptor should not be null.");
                    IAsyncResult result = new EnqueueMessageAsyncResult(this.replyChannelAcceptor, this.dequeuedCallback, this, onEnqueued, this);
                    if (result.CompletedSynchronously)
                    {
                        EnqueueMessageAsyncResult.End(result);
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    FxTrace.Exception.TraceUnhandledException(ex);
                }

                if (!success)
                {
                    this.SendAndClose(internalServerErrorHttpResponseMessage);
                }
            }

            HttpResponseMessage CreateHttpResponseMessage(Message message)
            {
                HttpResponseMessage httpResponseMessage = HttpResponseMessageProperty.GetHttpResponseMessageFromMessage(message);
                if (httpResponseMessage == null)
                {
                    HttpResponseMessageProperty property = message.Properties.GetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name);
                    httpResponseMessage = new HttpResponseMessage();
                    httpResponseMessage.StatusCode = message.IsFault ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;
                    this.httpOutput.ConfigureHttpResponseMessage(message, httpResponseMessage, property);
                }

                return httpResponseMessage;
            }

            void CompleteChannelModelIntegrationHandlerTask(Message replyMessage)
            {
                if (this.channelModelIntegrationHandlerTask != null)
                {
                    // If Service Model (or service instance) sent us null then we create a 202 HTTP response
                    HttpResponseMessage httpResponseMessage = null;
                    this.httpOutput = this.GetHttpOutput(replyMessage);

                    if (replyMessage != null)
                    {
                        httpResponseMessage = this.CreateHttpResponseMessage(replyMessage);
                    }
                    else
                    {
                        httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted);
                    }

                    Fx.Assert(httpResponseMessage != null, "httpResponse should not be null.");
                    if (httpResponseMessage.RequestMessage == null)
                    {
                        httpResponseMessage.RequestMessage = this.httpRequestMessage;
                        Fx.Assert(httpResponseMessage.RequestMessage != null, "httpResponseMessage.RequestMessage should never be null.");

                        if (replyMessage != null)
                        {
                            httpResponseMessage.CopyPropertiesFromMessage(replyMessage);
                        }
                    }

                    HttpChannelUtilities.EnsureHttpResponseMessageContentNotNull(httpResponseMessage);

                    this.cancellationTokenSource.CancelAfter(TimeoutHelper.ToMilliseconds(this.defaultSendTimeout));
                    this.channelModelIntegrationHandlerTask.TrySetResult(httpResponseMessage);
                }

                this.TraceProcessResponseStop();
            }

            void WaitTransportIntegrationHandlerTask(TimeSpan timeout)
            {
                Fx.Assert(this.transportIntegrationHandlerTask != null, "transportIntegrationHandlerTask should not be null.");
                this.transportIntegrationHandlerTask.Wait(timeout, null, null);
                this.wasProcessInboundRequestSuccessful = true;
            }

            class WebSocketHttpPipeline : NormalHttpPipeline
            {
                public WebSocketHttpPipeline(HttpRequestContext httpRequestContext, TransportIntegrationHandler transportIntegrationHandler)
                    : base(httpRequestContext, transportIntegrationHandler)
                {
                }

                public override void SendReply(Message message, TimeSpan timeout)
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    this.httpOutput = this.GetHttpOutput(message);
                    this.httpOutput.Send(helper.RemainingTime());
                }

                protected override void SetPipelineIncomingTimeout()
                {
                    this.cancellationTokenSource.CancelAfter(TimeoutHelper.ToMilliseconds((httpRequestContext.Listener as IDefaultCommunicationTimeouts).OpenTimeout));
                }

                protected override void SendHttpPipelineResponse()
                {
                    this.WaitTransportIntegrationHandlerTask(this.defaultSendTimeout);

                    HttpResponseMessage response = this.transportIntegrationHandlerTask.Result;

                    // HttpResponseMessage equals to null means that the pipeline is already cancelled.
                    // We should aborte the connection immediately in this case.
                    if (response == null)
                    {
                        this.cancellationTokenSource.Cancel();
                    }
                    else
                    {
                        if (response.StatusCode == HttpStatusCode.SwitchingProtocols)
                        {
                            string protocol = null;
                            if (response.Headers.Contains(WebSocketHelper.SecWebSocketProtocol))
                            {
                                foreach (string headerValue in response.Headers.GetValues(WebSocketHelper.SecWebSocketProtocol))
                                {
                                    protocol = headerValue;
                                    break;
                                }

                                response.Headers.Remove(WebSocketHelper.SecWebSocketProtocol);
                            }

                            // Remove unnecessary properties from HttpRequestMessage
                            if (response.RequestMessage != null)
                            {
                                HttpPipeline.RemoveHttpPipeline(response.RequestMessage);
                                response.RequestMessage.Properties.Remove(RemoteEndpointMessageProperty.Name);
                            }

                            // CSDMain 255817: There's a race condition that the channel could be dequeued and pipeline could be closed before the 
                            // Listener.CreateWebSocketChannelAndEnqueue call finishes. In this case, we are actually calling BeginGetContext twice, thus
                            // cause the memory leak.
                            this.isShortCutResponse = false;
                            bool channelEnqueued;
                            try
                            {
                                channelEnqueued = this.HttpRequestContext.Listener.CreateWebSocketChannelAndEnqueue(this.HttpRequestContext, this, response, protocol, this.dequeuedCallback);
                            }
                            catch (Exception ex)
                            {
                                if (!Fx.IsFatal(ex))
                                {
                                    if (TD.WebSocketConnectionFailedIsEnabled())
                                    {
                                        TD.WebSocketConnectionFailed(this.EventTraceActivity, ex.Message);
                                    }

                                    this.HttpRequestContext.SendResponseAndClose(HttpStatusCode.InternalServerError);
                                }

                                throw;
                            }

                            this.isShortCutResponse = !channelEnqueued;
                            if (!channelEnqueued)
                            {
                                if (TD.WebSocketConnectionDeclinedIsEnabled())
                                {
                                    TD.WebSocketConnectionDeclined(this.EventTraceActivity, HttpStatusCode.ServiceUnavailable.ToString());
                                }

                                this.httpRequestContext.SendResponseAndClose(HttpStatusCode.ServiceUnavailable);
                            }
                        }
                        else
                        {
                            if (TD.WebSocketConnectionDeclinedIsEnabled())
                            {
                                TD.WebSocketConnectionDeclined(this.EventTraceActivity, response.StatusCode.ToString());
                            }

                            this.SendAndClose(response);
                        }
                    }
                }
            }
        }

        class EnqueueMessageAsyncResult : TraceAsyncResult
        {
            HttpPipeline pipeline;
            ReplyChannelAcceptor acceptor;
            Action dequeuedCallback;

            public EnqueueMessageAsyncResult(
                ReplyChannelAcceptor acceptor,
                Action dequeuedCallback,
                HttpPipeline pipeline,
                AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.pipeline = pipeline;
                this.acceptor = acceptor;
                this.dequeuedCallback = dequeuedCallback;

                AsyncCallback asynCallback = PrepareAsyncCompletion(HandleParseIncomingMessage);
                IAsyncResult result = this.pipeline.BeginParseIncomingMessage(asynCallback, this);
                this.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<EnqueueMessageAsyncResult>(result);
            }

            static bool HandleParseIncomingMessage(IAsyncResult result)
            {
                EnqueueMessageAsyncResult thisPtr = (EnqueueMessageAsyncResult)result.AsyncState;
                thisPtr.CompleteParseAndEnqueue(result);
                return true;
            }

            void CompleteParseAndEnqueue(IAsyncResult result)
            {
                using (DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.BoundOperation(this.CallbackActivity) : null)
                {
                    Exception requestException;
                    Message message = this.pipeline.EndParseIncomingMesssage(result, out requestException);
                    if ((message == null) && (requestException == null))
                    {
                        throw FxTrace.Exception.AsError(
                                new ProtocolException(
                                    SR.GetString(SR.MessageXmlProtocolError),
                                    new XmlException(SR.GetString(SR.MessageIsEmpty))));
                    }

                    this.pipeline.OnParseComplete(message, requestException);
                    this.acceptor.Enqueue(this.pipeline.HttpRequestContext, this.dequeuedCallback, true);
                }
            }
        }
    }
}
