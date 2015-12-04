//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    abstract class HttpRequestContext : RequestContextBase
    {
        HttpOutput httpOutput;
        bool errorGettingHttpInput;
        HttpChannelListener listener;
        SecurityMessageProperty securityProperty;
        EventTraceActivity eventTraceActivity;
        HttpPipeline httpPipeline;
        ServerWebSocketTransportDuplexSessionChannel webSocketChannel;

        protected HttpRequestContext(HttpChannelListener listener, Message requestMessage, EventTraceActivity eventTraceActivity)
            : base(requestMessage, listener.InternalCloseTimeout, listener.InternalSendTimeout)
        {
            this.listener = listener;
            this.eventTraceActivity = eventTraceActivity;
        }

        public bool KeepAliveEnabled
        {
            get
            {
                return listener.KeepAliveEnabled;
            }
        }

        public bool HttpMessagesSupported
        {
            get { return this.listener.HttpMessageSettings.HttpMessagesSupported; }
        }

        public abstract string HttpMethod { get; }
        public abstract bool IsWebSocketRequest { get; }

        internal ServerWebSocketTransportDuplexSessionChannel WebSocketChannel
        {
            get
            {
                return this.webSocketChannel;
            }

            set
            {
                Fx.Assert(this.webSocketChannel == null, "webSocketChannel should not be set twice.");
                this.webSocketChannel = value;
            }
        }

        internal HttpChannelListener Listener
        {
            get { return this.listener; }
        }

        internal EventTraceActivity EventTraceActivity
        {
            get
            {
                return this.eventTraceActivity;
            }
        }

        // Note: This method will return null in the case where throwOnError is false, and a non-fatal error occurs.
        // Please exercice caution when passing in throwOnError = false.  This should basically only be done in error
        // code paths, or code paths where there is very good reason that you would not want this method to throw.
        // When passing in throwOnError = false, please handle the case where this method returns null.
        public HttpInput GetHttpInput(bool throwOnError)
        {
            HttpPipeline pipeline = this.httpPipeline;
            if ((pipeline != null) && pipeline.IsHttpInputInitialized)
            {
                return pipeline.HttpInput;
            }

            HttpInput httpInput = null;
            if (throwOnError || !this.errorGettingHttpInput)
            {
                try
                {
                    httpInput = GetHttpInput();
                    this.errorGettingHttpInput = false;
                }
                catch (Exception e)
                {
                    this.errorGettingHttpInput = true;
                    if (throwOnError || Fx.IsFatal(e))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                }
            }

            return httpInput;
        }

        internal static HttpRequestContext CreateContext(HttpChannelListener listener, HttpListenerContext listenerContext, EventTraceActivity eventTraceActivity)
        {
            return new ListenerHttpContext(listener, listenerContext, eventTraceActivity);
        }

        protected abstract SecurityMessageProperty OnProcessAuthentication();
        public abstract HttpOutput GetHttpOutput(Message message);
        protected abstract HttpInput GetHttpInput();

        public HttpOutput GetHttpOutputCore(Message message)
        {
            if (this.httpOutput != null)
            {
                return this.httpOutput;
            }

            return this.GetHttpOutput(message);
        }

        protected override void OnAbort()
        {
            if (this.httpOutput != null)
            {
                this.httpOutput.Abort(HttpAbortReason.Aborted);
            }

            this.Cleanup();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            try
            {
                if (this.httpOutput != null)
                {
                    this.httpOutput.Close();
                }
            }
            finally
            {
                this.Cleanup();
            }
        }
        
        protected virtual void Cleanup()
        {
            if (this.httpPipeline != null)
            {
                this.httpPipeline.Close();
            }
        }

        public void InitializeHttpPipeline(TransportIntegrationHandler transportIntegrationHandler)
        {
            this.httpPipeline = HttpPipeline.CreateHttpPipeline(this, transportIntegrationHandler, this.IsWebSocketRequest);
        }

        internal void SetMessage(Message message, Exception requestException)
        {
            if ((message == null) && (requestException == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.MessageXmlProtocolError),
                    new XmlException(SR.GetString(SR.MessageIsEmpty))));
            }

            this.TraceHttpMessageReceived(message);

            if (requestException != null)
            {
                base.SetRequestMessage(requestException);
                message.Close();
            }
            else
            {
                message.Properties.Security = (this.securityProperty != null) ? (SecurityMessageProperty)this.securityProperty.CreateCopy() : null;
                base.SetRequestMessage(message);
            }
        }

        void TraceHttpMessageReceived(Message message)
        {
            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                bool attached = false;
                Guid relatedId = this.eventTraceActivity != null ? this.eventTraceActivity.ActivityId : Guid.Empty;
                HttpRequestMessageProperty httpProperty;

                // Encoder will always add an activity. We need to remove this and read it
                // from the web headers for http since correlation might be propogated.
                if (message.Headers.MessageId == null &&
                    message.Properties.TryGetValue<HttpRequestMessageProperty>(HttpRequestMessageProperty.Name, out httpProperty))
                {
                    try
                    {
                        string e2eId = httpProperty.Headers[EventTraceActivity.Name];
                        if (!String.IsNullOrEmpty(e2eId))
                        {
                            byte[] data = Convert.FromBase64String(e2eId);
                            if (data != null && data.Length == 16)
                            {
                                Guid id = new Guid(data);
                                this.eventTraceActivity = new EventTraceActivity(id, true);
                                message.Properties[EventTraceActivity.Name] = this.eventTraceActivity;
                                attached = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                        {
                            throw;
                        }
                    }
                }

                if (!attached)
                {
                    this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message, true);
                }

                if (TD.MessageReceivedByTransportIsEnabled())
                {
                    TD.MessageReceivedByTransport(
                        this.eventTraceActivity,
                        this.listener != null && this.listener.Uri != null ? this.listener.Uri.AbsoluteUri : string.Empty,
                        relatedId);
                }
            }
        }

        protected abstract HttpStatusCode ValidateAuthentication();

        bool PrepareReply(ref Message message)
        {
            bool closeOnReceivedEof = false;

            // null means we're done
            if (message == null)
            {
                // A null message means either a one-way request or that the service operation returned null and
                // hence we can close the HttpOutput. By default we keep the HttpOutput open to allow the writing to the output 
                // even after the HttpInput EOF is received and the HttpOutput will be closed only on close of the HttpRequestContext.
                closeOnReceivedEof = true;
                message = CreateAckMessage(HttpStatusCode.Accepted, string.Empty);
            }

            if (!listener.ManualAddressing)
            {
                if (message.Version.Addressing == AddressingVersion.WSAddressingAugust2004)
                {
                    if (message.Headers.To == null ||
                        listener.AnonymousUriPrefixMatcher == null ||
                        !listener.AnonymousUriPrefixMatcher.IsAnonymousUri(message.Headers.To))
                    {
                        message.Headers.To = message.Version.Addressing.AnonymousUri;
                    }
                }
                else if (message.Version.Addressing == AddressingVersion.WSAddressing10
                    || message.Version.Addressing == AddressingVersion.None)
                {
                    if (message.Headers.To != null &&
                        (listener.AnonymousUriPrefixMatcher == null ||
                        !listener.AnonymousUriPrefixMatcher.IsAnonymousUri(message.Headers.To)))
                    {
                        message.Headers.To = null;
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, message.Version.Addressing)));
                }
            }

            message.Properties.AllowOutputBatching = false;
            this.httpOutput = GetHttpOutputCore(message);

            // Reuse the HttpInput we got previously.
            HttpInput input = this.httpPipeline.HttpInput;
            if (input != null)
            {
                HttpDelayedAcceptStream requestStream = input.GetInputStream(false) as HttpDelayedAcceptStream;
                if (requestStream != null && TransferModeHelper.IsRequestStreamed(listener.TransferMode)
                    && requestStream.EnableDelayedAccept(this.httpOutput, closeOnReceivedEof))
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnReply(Message message, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            Message responseMessage = message;

            try
            {
                bool closeOutputAfterReply = PrepareReply(ref responseMessage);
                this.httpPipeline.SendReply(responseMessage, timeoutHelper.RemainingTime());

                if (closeOutputAfterReply)
                {
                    httpOutput.Close();
                }

                if (TD.MessageSentByTransportIsEnabled())
                {
                    TD.MessageSentByTransport(eventTraceActivity, this.Listener.Uri.AbsoluteUri);
                }
            }
            finally
            {
                if (message != null &&
                    !object.ReferenceEquals(message, responseMessage))
                {
                    responseMessage.Close();
                }
            }
        }

        protected override IAsyncResult OnBeginReply(
            Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReplyAsyncResult(this, message, timeout, callback, state);
        }

        protected override void OnEndReply(IAsyncResult result)
        {
            ReplyAsyncResult.End(result);
        }

        public bool ProcessAuthentication()
        {
            if (TD.HttpContextBeforeProcessAuthenticationIsEnabled())
            {
                TD.HttpContextBeforeProcessAuthentication(this.eventTraceActivity);
            }

            HttpStatusCode statusCode = ValidateAuthentication();

            if (statusCode == HttpStatusCode.OK)
            {
                bool authenticationSucceeded = false;
                statusCode = HttpStatusCode.Forbidden;
                try
                {
                    this.securityProperty = OnProcessAuthentication();
                    authenticationSucceeded = true;
                    return true;
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    if (e.Data.Contains(HttpChannelUtilities.HttpStatusCodeKey))
                    {
                        if (e.Data[HttpChannelUtilities.HttpStatusCodeKey] is HttpStatusCode)
                        {
                            statusCode = (HttpStatusCode)e.Data[HttpChannelUtilities.HttpStatusCodeKey];
                        }
                    }

                    throw;
                }
                finally
                {
                    if (!authenticationSucceeded)
                    {
                        SendResponseAndClose(statusCode);
                    }
                }
            }
            else
            {
                SendResponseAndClose(statusCode);
                return false;
            }
        }

        internal void SendResponseAndClose(HttpStatusCode statusCode)
        {
            SendResponseAndClose(statusCode, string.Empty);
        }

        internal void SendResponseAndClose(HttpStatusCode statusCode, string statusDescription)
        {
            if (ReplyInitiated)
            {
                this.Close();
                return;
            }

            using (Message ackMessage = CreateAckMessage(statusCode, statusDescription))
            {
                this.Reply(ackMessage);
            }

            this.Close();
        }

        internal void SendResponseAndClose(HttpResponseMessage httpResponseMessage)
        {
            if (this.TryInitiateReply())
            {
                // Send the response message.
                try
                {
                    if (this.httpOutput == null)
                    {
                        this.httpOutput = this.GetHttpOutputCore(new NullMessage());
                    }
                    this.httpOutput.Send(httpResponseMessage, this.DefaultSendTimeout);
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
            
            // Close the request context.
            try
            {
                this.Close(); // this also closes the HttpOutput
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

        Message CreateAckMessage(HttpStatusCode statusCode, string statusDescription)
        {
            Message ackMessage = new NullMessage();
            HttpResponseMessageProperty httpResponseProperty = new HttpResponseMessageProperty();
            httpResponseProperty.StatusCode = statusCode;
            httpResponseProperty.SuppressEntityBody = true;
            if (statusDescription.Length > 0)
            {
                httpResponseProperty.StatusDescription = statusDescription;
            }

            ackMessage.Properties.Add(HttpResponseMessageProperty.Name, httpResponseProperty);

            return ackMessage;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                    Justification = "The exceptions will be traced and thrown by the handling method.")]
        public void AcceptWebSocket(HttpResponseMessage response, string protocol, TimeSpan timeout)
        {
            Task<WebSocketContext> acceptTask;
            bool success = false;
            try
            {
                acceptTask = this.AcceptWebSocketCore(response, protocol);

                try
                {
                    if (!acceptTask.Wait(TimeoutHelper.ToMilliseconds(timeout)))
                    {
                        throw FxTrace.Exception.AsError(new TimeoutException(SR.GetString(SR.AcceptWebSocketTimedOutError)));
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    WebSocketHelper.ThrowCorrectException(ex);               
                }

                success = true;
            }
            finally
            {
                if (!success)
                {
                    this.OnAcceptWebSocketError();
                }
            }

            this.SetReplySent();
            this.OnAcceptWebSocketSuccess(acceptTask.Result, response.RequestMessage);
        }

        protected abstract Task<WebSocketContext> AcceptWebSocketCore(HttpResponseMessage response, string protocol);
        protected virtual void OnAcceptWebSocketError()
        {
        }

        protected abstract void OnAcceptWebSocketSuccess(WebSocketContext context, HttpRequestMessage requestMessage);

        protected void OnAcceptWebSocketSuccess(
            WebSocketContext context, 
            RemoteEndpointMessageProperty remoteEndpointMessageProperty, 
            byte[] webSocketInternalBuffer, 
            bool shouldDisposeWebSocketAfterClose, 
            HttpRequestMessage requestMessage)
        {
            this.webSocketChannel.SetWebSocketInfo(
                context, 
                remoteEndpointMessageProperty, 
                this.securityProperty, 
                webSocketInternalBuffer,
                shouldDisposeWebSocketAfterClose, 
                requestMessage);
        }

        public IAsyncResult BeginAcceptWebSocket(HttpResponseMessage response, string protocol, AsyncCallback callback, object state)
        {
            return new AcceptWebSocketAsyncResult(this, response, protocol, callback, state);
        }

        public void EndAcceptWebSocket(IAsyncResult result)
        {
            AcceptWebSocketAsyncResult.End(result);
        }

        class ReplyAsyncResult : AsyncResult
        {
            static AsyncCallback onSendCompleted;
            static Action<object, HttpResponseMessage> onHttpPipelineSend;

            bool closeOutputAfterReply;
            HttpRequestContext context;
            Message message;
            Message responseMessage;
            TimeoutHelper timeoutHelper;

            public ReplyAsyncResult(HttpRequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.context = context;
                this.message = message;
                this.responseMessage = null;
                this.timeoutHelper = new TimeoutHelper(timeout);

                ThreadTrace.Trace("Begin sending http reply");

                this.responseMessage = this.message;

                if (this.SendResponse())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReplyAsyncResult>(result);
            }

            void OnSendResponseCompleted(IAsyncResult result)
            {
                try
                {
                    context.httpOutput.EndSend(result);
                    ThreadTrace.Trace("End sending http reply");

                    if (this.closeOutputAfterReply)
                    {
                        context.httpOutput.Close();
                    }
                }
                finally
                {
                    if (this.message != null &&
                        !object.ReferenceEquals(this.message, this.responseMessage))
                    {
                        this.responseMessage.Close();
                    }
                }
            }

            static void OnSendResponseCompletedCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                ReplyAsyncResult thisPtr = (ReplyAsyncResult)result.AsyncState;
                Exception completionException = null;

                try
                {
                    thisPtr.OnSendResponseCompleted(result);
                }
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

            static void OnHttpPipelineSendCallback(object target, HttpResponseMessage httpResponseMessage)
            {
                ReplyAsyncResult thisPtr = (ReplyAsyncResult)target;

                Exception pendingException = null;
                bool completed = false;
                try
                {
                    completed = thisPtr.SendResponse(httpResponseMessage);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    pendingException = e;
                    completed = true;
                }

                if (completed)
                {
                    thisPtr.Complete(false, pendingException);
                }
            }

            public bool SendResponse(HttpResponseMessage httpResponseMessage)
            {
                if (onSendCompleted == null)
                {
                    onSendCompleted = Fx.ThunkCallback(new AsyncCallback(OnSendResponseCompletedCallback));
                }

                bool success = false;

                try
                {
                    return this.SendResponseCore(httpResponseMessage, out success);
                }
                finally
                {
                    if (!success && this.message != null &&
                        !object.ReferenceEquals(this.message, this.responseMessage))
                    {
                        this.responseMessage.Close();
                    }
                }
            }

            public bool SendResponse()
            {
                if (onSendCompleted == null)
                {
                    onSendCompleted = Fx.ThunkCallback(new AsyncCallback(OnSendResponseCompletedCallback));
                }

                bool success = false;

                try
                {
                    this.closeOutputAfterReply = context.PrepareReply(ref this.responseMessage);
                    if (onHttpPipelineSend == null)
                    {
                        onHttpPipelineSend = new Action<object, HttpResponseMessage>(OnHttpPipelineSendCallback);
                    }

                    if (context.httpPipeline.SendAsyncReply(this.responseMessage, onHttpPipelineSend, this) == AsyncCompletionResult.Queued)
                    {
                        //// In Async send + HTTP pipeline path, we will send the response back after the result coming out from the pipeline.
                        //// So we don't need to call it here.
                        success = true;
                        return false;
                    }

                    HttpResponseMessage httpResponseMessage = null;

                    if (this.context.HttpMessagesSupported)
                    {
                        httpResponseMessage = HttpResponseMessageProperty.GetHttpResponseMessageFromMessage(this.responseMessage);
                    }

                    return this.SendResponseCore(httpResponseMessage, out success);
                }
                finally
                {
                    if (!success && this.message != null &&
                        !object.ReferenceEquals(this.message, this.responseMessage))
                    {
                        this.responseMessage.Close();
                    }
                }
            }

            bool SendResponseCore(HttpResponseMessage httpResponseMessage, out bool success)
            {
                success = false;
                IAsyncResult result;
                if (httpResponseMessage == null)
                {
                    result = context.httpOutput.BeginSend(this.timeoutHelper.RemainingTime(), onSendCompleted, this);
                }
                else
                {
                    result = context.httpOutput.BeginSend(httpResponseMessage, this.timeoutHelper.RemainingTime(), onSendCompleted, this);
                }

                success = true;
                if (!result.CompletedSynchronously)
                {
                    return false;
                }

                this.OnSendResponseCompleted(result);
                return true;
            }
        }

        internal IAsyncResult BeginProcessInboundRequest(
                    ReplyChannelAcceptor replyChannelAcceptor,
                    Action acceptorCallback,
                    AsyncCallback callback,
                    object state)
        {
            return this.httpPipeline.BeginProcessInboundRequest(replyChannelAcceptor, acceptorCallback, callback, state);
        }

        internal void EndProcessInboundRequest(IAsyncResult result)
        {
            this.httpPipeline.EndProcessInboundRequest(result);
        }

        class ListenerHttpContext : HttpRequestContext, HttpRequestMessageProperty.IHttpHeaderProvider
        {
            HttpListenerContext listenerContext;
            byte[] webSocketInternalBuffer;

            public ListenerHttpContext(HttpChannelListener listener,
                HttpListenerContext listenerContext, EventTraceActivity eventTraceActivity)
                : base(listener, null, eventTraceActivity)
            {
                this.listenerContext = listenerContext;
            }

            public override string HttpMethod
            {
                get { return listenerContext.Request.HttpMethod; }
            }

            public override bool IsWebSocketRequest
            {
                get { return this.listenerContext.Request.IsWebSocketRequest; }
            }

            protected override HttpInput GetHttpInput()
            {
                return new ListenerContextHttpInput(this);
            }

            protected override Task<WebSocketContext> AcceptWebSocketCore(HttpResponseMessage response, string protocol)
            {
                // CopyHeaders would still throw when the response contains a "WWW-Authenticate"-header
                // But this is ok in this case because the "WWW-Authenticate"-header doesn't make sense
                // for a response returning 101 (Switching Protocol)
                HttpChannelUtilities.CopyHeaders(response, this.listenerContext.Response.Headers.Add);
                
                this.webSocketInternalBuffer = this.Listener.TakeWebSocketInternalBuffer();
                return this.listenerContext.AcceptWebSocketAsync(
                                                protocol,
                                                WebSocketHelper.GetReceiveBufferSize(this.listener.MaxReceivedMessageSize),
                                                this.Listener.WebSocketSettings.GetEffectiveKeepAliveInterval(),
                                                new ArraySegment<byte>(this.webSocketInternalBuffer)).Upcast<HttpListenerWebSocketContext, WebSocketContext>();
            }

            protected override void OnAcceptWebSocketError()
            {
                byte[] buffer = Interlocked.CompareExchange<byte[]>(ref this.webSocketInternalBuffer, null, this.webSocketInternalBuffer);
                if (buffer != null)
                {
                    this.Listener.ReturnWebSocketInternalBuffer(buffer);
                }
            }

            protected override void OnAcceptWebSocketSuccess(WebSocketContext context, HttpRequestMessage requestMessage)
            {
                RemoteEndpointMessageProperty remoteEndpointMessageProperty = null;
                if (this.listenerContext.Request.RemoteEndPoint != null)
                {
                    remoteEndpointMessageProperty = new RemoteEndpointMessageProperty(this.listenerContext.Request.RemoteEndPoint);
                }

                base.OnAcceptWebSocketSuccess(context, remoteEndpointMessageProperty, this.webSocketInternalBuffer, true, requestMessage);
            }

            public override HttpOutput GetHttpOutput(Message message)
            {
                // work around http.sys keep alive bug with chunked requests, see MB 49676, this is fixed in Vista
                if (listenerContext.Request.ContentLength64 == -1 && !OSEnvironmentHelper.IsVistaOrGreater)
                {
                    listenerContext.Response.KeepAlive = false;
                }
                else
                {
                    listenerContext.Response.KeepAlive = listener.KeepAliveEnabled;
                }
                ICompressedMessageEncoder compressedMessageEncoder = listener.MessageEncoderFactory.Encoder as ICompressedMessageEncoder;
                if (compressedMessageEncoder != null && compressedMessageEncoder.CompressionEnabled)
                {
                    string acceptEncoding = listenerContext.Request.Headers[HttpChannelUtilities.AcceptEncodingHeader];
                    compressedMessageEncoder.AddCompressedMessageProperties(message, acceptEncoding);
                }

                return HttpOutput.CreateHttpOutput(listenerContext.Response, Listener, message, this.HttpMethod);
            }

            protected override SecurityMessageProperty OnProcessAuthentication()
            {
                return Listener.ProcessAuthentication(listenerContext);
            }

            protected override HttpStatusCode ValidateAuthentication()
            {
                return Listener.ValidateAuthentication(listenerContext);
            }

            protected override void OnAbort()
            {
                listenerContext.Response.Abort();

                // CSDMain 259910, we should remove this and call base.OnAbort() instead to improve maintainability
                this.Cleanup();
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                base.OnClose(timeoutHelper.RemainingTime());
                try
                {
                    listenerContext.Response.Close();
                }
                catch (HttpListenerException listenerException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        HttpChannelUtilities.CreateCommunicationException(listenerException));
                }
            }

            void HttpRequestMessageProperty.IHttpHeaderProvider.CopyHeaders(WebHeaderCollection headers)
            {
                HttpListenerRequest listenerRequest = this.listenerContext.Request;
                headers.Add(listenerRequest.Headers);

                // MB 57988 - System.Net strips off user-agent from the headers collection
                if (listenerRequest.UserAgent != null && headers[HttpRequestHeader.UserAgent] == null)
                {
                    headers.Add(HttpRequestHeader.UserAgent, listenerRequest.UserAgent);
                }
            }

            class ListenerContextHttpInput : HttpInput
            {
                ListenerHttpContext listenerHttpContext;
                string cachedContentType; // accessing the header in System.Net involves a native transition
                byte[] preReadBuffer;

                public ListenerContextHttpInput(ListenerHttpContext listenerHttpContext)
                    : base(listenerHttpContext.Listener, true, listenerHttpContext.listener.IsChannelBindingSupportEnabled)
                {
                    this.listenerHttpContext = listenerHttpContext;
                    if (this.listenerHttpContext.listenerContext.Request.ContentLength64 == -1)
                    {
                        this.preReadBuffer = new byte[1];
                        if (this.listenerHttpContext.listenerContext.Request.InputStream.Read(preReadBuffer, 0, 1) == 0)
                        {
                            this.preReadBuffer = null;
                        }
                    }
                }

                public override long ContentLength
                {
                    get
                    {
                        return this.listenerHttpContext.listenerContext.Request.ContentLength64;
                    }
                }

                protected override string ContentTypeCore
                {
                    get
                    {
                        if (this.cachedContentType == null)
                        {
                            this.cachedContentType = this.listenerHttpContext.listenerContext.Request.ContentType;
                        }

                        return this.cachedContentType;
                    }
                }

                protected override bool HasContent
                {
                    get { return (this.preReadBuffer != null || this.ContentLength > 0); }
                }

                protected override string SoapActionHeader
                {
                    get
                    {
                        return this.listenerHttpContext.listenerContext.Request.Headers["SOAPAction"];
                    }
                }

                protected override ChannelBinding ChannelBinding
                {
                    get
                    {
                        return ChannelBindingUtility.GetToken(this.listenerHttpContext.listenerContext.Request.TransportContext);
                    }
                }

                protected override void AddProperties(Message message)
                {
                    HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty(this.listenerHttpContext);
                    requestProperty.Method = this.listenerHttpContext.listenerContext.Request.HttpMethod;

                    // Uri.Query always includes the '?'
                    if (this.listenerHttpContext.listenerContext.Request.Url.Query.Length > 1)
                    {
                        requestProperty.QueryString = this.listenerHttpContext.listenerContext.Request.Url.Query.Substring(1);
                    }

                    message.Properties.Add(HttpRequestMessageProperty.Name, requestProperty);
                    message.Properties.Via = this.listenerHttpContext.listenerContext.Request.Url;

                    RemoteEndpointMessageProperty remoteEndpointProperty = new RemoteEndpointMessageProperty(this.listenerHttpContext.listenerContext.Request.RemoteEndPoint);
                    message.Properties.Add(RemoteEndpointMessageProperty.Name, remoteEndpointProperty);
                }

                public override void ConfigureHttpRequestMessage(HttpRequestMessage message)
                {
                    message.Method = new HttpMethod(this.listenerHttpContext.listenerContext.Request.HttpMethod);
                    message.RequestUri = this.listenerHttpContext.listenerContext.Request.Url;
                    foreach (string webHeaderKey in this.listenerHttpContext.listenerContext.Request.Headers.Keys)
                    {
                        message.AddHeader(webHeaderKey, this.listenerHttpContext.listenerContext.Request.Headers[webHeaderKey]);
                    }
                    message.Properties.Add(RemoteEndpointMessageProperty.Name, new RemoteEndpointMessageProperty(this.listenerHttpContext.listenerContext.Request.RemoteEndPoint));
                }

                protected override Stream GetInputStream()
                {
                    if (this.preReadBuffer != null)
                    {
                        return new ListenerContextInputStream(listenerHttpContext, preReadBuffer);
                    }
                    else
                    {
                        return new ListenerContextInputStream(listenerHttpContext);
                    }
                }

                class ListenerContextInputStream : HttpDelayedAcceptStream
                {
                    public ListenerContextInputStream(ListenerHttpContext listenerHttpContext)
                        : base(listenerHttpContext.listenerContext.Request.InputStream)
                    {
                    }

                    public ListenerContextInputStream(ListenerHttpContext listenerHttpContext, byte[] preReadBuffer)
                        : base(new PreReadStream(listenerHttpContext.listenerContext.Request.InputStream, preReadBuffer))
                    {
                    }

                    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                    {
                        try
                        {
                            return base.BeginRead(buffer, offset, count, callback, state);
                        }
                        catch (HttpListenerException listenerException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                HttpChannelUtilities.CreateCommunicationException(listenerException));
                        }
                    }

                    public override int EndRead(IAsyncResult result)
                    {
                        try
                        {
                            return base.EndRead(result);
                        }
                        catch (HttpListenerException listenerException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                HttpChannelUtilities.CreateCommunicationException(listenerException));
                        }
                    }

                    public override int Read(byte[] buffer, int offset, int count)
                    {
                        try
                        {
                            return base.Read(buffer, offset, count);
                        }
                        catch (HttpListenerException listenerException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                HttpChannelUtilities.CreateCommunicationException(listenerException));
                        }
                    }

                    public override int ReadByte()
                    {
                        try
                        {
                            return base.ReadByte();
                        }
                        catch (HttpListenerException listenerException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                HttpChannelUtilities.CreateCommunicationException(listenerException));
                        }
                    }
                }
            }
        }

        class AcceptWebSocketAsyncResult : AsyncResult
        {
            static AsyncCallback onHandleAcceptWebSocketResult = Fx.ThunkCallback(new AsyncCallback(HandleAcceptWebSocketResult));

            HttpRequestContext context;
            SignalGate gate = new SignalGate();
            HttpResponseMessage response;

            public AcceptWebSocketAsyncResult(HttpRequestContext context, HttpResponseMessage response, string protocol, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(context != null, "context should not be null.");
                Fx.Assert(response != null, "response should not be null.");
                this.context = context;
                this.response = response;
                IAsyncResult result = this.context.AcceptWebSocketCore(response, protocol).AsAsyncResult<WebSocketContext>(onHandleAcceptWebSocketResult, this);

                if (this.gate.Unlock())
                {
                    this.CompleteAcceptWebSocket(result);
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AcceptWebSocketAsyncResult>(result);
            }

            static void HandleAcceptWebSocketResult(IAsyncResult result)
            {
                AcceptWebSocketAsyncResult thisPtr = (AcceptWebSocketAsyncResult)result.AsyncState;
                if (!thisPtr.gate.Signal())
                {
                    return;
                }

                Exception pendingException = null;
                try
                {
                    thisPtr.CompleteAcceptWebSocket(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    pendingException = ex;
                }

                thisPtr.Complete(false, pendingException);
            }

            void CompleteAcceptWebSocket(IAsyncResult result)
            {
                Task<WebSocketContext> acceptTask = result as Task<WebSocketContext>;
                Fx.Assert(acceptTask != null, "acceptTask should not be null.");

                if (acceptTask.IsFaulted)
                {
                    this.context.OnAcceptWebSocketError();
                    throw FxTrace.Exception.AsError<WebSocketException>(acceptTask.Exception);
                }
                else if (acceptTask.IsCanceled)
                {
                    this.context.OnAcceptWebSocketError();
                    //
                    throw FxTrace.Exception.AsError(new TimeoutException(SR.GetString(SR.AcceptWebSocketTimedOutError)));
                }


                this.context.SetReplySent();
                this.context.OnAcceptWebSocketSuccess(acceptTask.Result, response.RequestMessage);
            }
        }
    }
}
