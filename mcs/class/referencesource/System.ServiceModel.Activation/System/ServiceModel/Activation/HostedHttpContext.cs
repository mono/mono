//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Management;
    using System.Web.WebSockets;

    class HostedHttpContext : HttpRequestContext
    {
        HostedRequestContainer requestContainer;
        HostedHttpRequestAsyncResult result;
        TaskCompletionSource<object> webSocketWaitingTask;
        RemoteEndpointMessageProperty remoteEndpointMessageProperty;
        TaskCompletionSource<WebSocketContext> webSocketContextTaskSource;

        int impersonationReleased = 0;

        public HostedHttpContext(HttpChannelListener listener, HostedHttpRequestAsyncResult result)
            : base(listener, null, result.EventTraceActivity)
        {
            AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

            this.result = result;
            result.AddRefForImpersonation();
        }

        public override string HttpMethod
        {
            get
            {
                return result.GetHttpMethod();
            }
        }

        internal void CompleteWithException(Exception ex)
        {
            Fx.Assert(ex != null, "ex should not be null.");
            this.result.CompleteOperation(ex);
        }

        protected override SecurityMessageProperty OnProcessAuthentication()
        {
            return Listener.ProcessAuthentication(this.result);
        }

        protected override HttpStatusCode ValidateAuthentication()
        {
            return Listener.ValidateAuthentication(this.result);
        }

        // Accessing the headers of an already replied HttpRequest instance causes an Access Violation in hosted mode.
        // In one-way scenarios, reply happens before the user gets the message. That's why we are disabling all access
        // to HostedRequestContainer (CSDMain 34014).
        void CloseHostedRequestContainer()
        {
            // RequestContext.RequestMessage property can throw rather than create a message.
            // This means we never created a message and never cached the IIS properties.
            // At this point the user may return a reply, so requestContainer is allowed to be null.
            if (this.requestContainer != null)
            {
                this.requestContainer.Close();
                this.requestContainer = null;
            }
        }

        protected override Task<WebSocketContext> AcceptWebSocketCore(HttpResponseMessage response, string protocol)
        {
            this.BeforeAcceptWebSocket(response);
            this.webSocketContextTaskSource = new TaskCompletionSource<WebSocketContext>();
            this.result.Application.Context.AcceptWebSocketRequest(PostAcceptWebSocket, new AspNetWebSocketOptions() { SubProtocol = protocol });
            this.result.OnReplySent();
            return this.webSocketContextTaskSource.Task;
        }

        protected override void OnAcceptWebSocketSuccess(WebSocketContext context, HttpRequestMessage requestMessage)
        {
            // ASP.NET owns the WebSocket object and needs it during the cleanup process. We should not dispose the WebSocket in WCF layer.
            base.OnAcceptWebSocketSuccess(context, this.remoteEndpointMessageProperty, null, false, requestMessage);
        }

        void BeforeAcceptWebSocket(HttpResponseMessage response)
        {
            this.SetRequestContainer(new HostedRequestContainer(this.result));
            string address = string.Empty;
            int port = 0;

            if (this.requestContainer.TryGetAddressAndPort(out address, out port))
            {
                this.remoteEndpointMessageProperty = new RemoteEndpointMessageProperty(address, port);
            }

            this.CloseHostedRequestContainer();

            HostedHttpContext.AppendHeaderFromHttpResponseMessageToResponse(response, this.result);
        }

        Task PostAcceptWebSocket(AspNetWebSocketContext context)
        {
            this.webSocketWaitingTask = new TaskCompletionSource<object>();
            this.WebSocketChannel.Closed += new EventHandler(this.FinishWebSocketWaitingTask);
            this.webSocketContextTaskSource.SetResult(context);
            return webSocketWaitingTask.Task;
        }

        static void AppendHeaderFromHttpResponseMessageToResponse(HttpResponseMessage response, HostedHttpRequestAsyncResult result)
        {
            HostedHttpContext.AppendHeaderToResponse(response.Headers, result);
            if (response.Content != null)
            {
                HostedHttpContext.AppendHeaderToResponse(response.Content.Headers, result);
            }
        }

        static void AppendHeaderToResponse(HttpHeaders headers, HostedHttpRequestAsyncResult result)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                foreach (string value in header.Value)
                {
                    result.AppendHeader(header.Key, value);
                }
            }
        }

        void FinishWebSocketWaitingTask(object sender, EventArgs args)
        {
            this.webSocketWaitingTask.TrySetResult(null);
        }

        public override bool IsWebSocketRequest
        {
            get
            {
                return this.result.IsWebSocketRequest;
            }
        }

        protected override void OnReply(Message message, TimeSpan timeout)
        {
            this.CloseHostedRequestContainer();
            base.OnReply(message, timeout);
        }

        protected override IAsyncResult OnBeginReply(
            Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.CloseHostedRequestContainer();
            return base.OnBeginReply(message, timeout, callback, state);
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            result.Abort();
        }

        protected override void Cleanup()
        {
            base.Cleanup();

            if (Interlocked.Increment(ref this.impersonationReleased) == 1)
            {
                this.result.ReleaseImpersonation();
            }
        }

        protected override HttpInput GetHttpInput()
        {
            return new HostedHttpInput(this);
        }

        public override HttpOutput GetHttpOutput(Message message)
        {
            HttpInput httpInput = this.GetHttpInput(false);

            // work around http.sys keep alive bug with chunked requests, see MB 49676, this is fixed in Vista
            if ((httpInput != null && httpInput.ContentLength == -1 && !OSEnvironmentHelper.IsVistaOrGreater) || !this.KeepAliveEnabled)
            {
                result.SetConnectionClose();
            }

            ICompressedMessageEncoder compressedMessageEncoder = this.Listener.MessageEncoderFactory.Encoder as ICompressedMessageEncoder;
            if (compressedMessageEncoder != null && compressedMessageEncoder.CompressionEnabled)
            {
                string acceptEncoding = this.result.GetAcceptEncoding();
                compressedMessageEncoder.AddCompressedMessageProperties(message, acceptEncoding);
            }

            return new HostedRequestHttpOutput(result, Listener, message, this);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            base.OnClose(timeout);
            result.OnReplySent();
        }

        void SetRequestContainer(HostedRequestContainer requestContainer)
        {
            this.requestContainer = requestContainer;
        }

        class HostedHttpInput : HttpInput
        {
            int contentLength;
            string contentType;
            HostedHttpContext hostedHttpContext;
            byte[] preReadBuffer;

            public HostedHttpInput(HostedHttpContext hostedHttpContext)
                : base(hostedHttpContext.Listener, true, hostedHttpContext.Listener.IsChannelBindingSupportEnabled)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

                this.hostedHttpContext = hostedHttpContext;

                EnvelopeVersion envelopeVersion = hostedHttpContext.Listener.MessageEncoderFactory.Encoder.MessageVersion.Envelope;

                // MB#29602, perf optimization
                if (envelopeVersion == EnvelopeVersion.Soap11)
                {
                    // For soap 1.1, use headers collection to get content-type since we need to pull in the headers 
                    // collection for SOAP-Action anyways
                    this.contentType = hostedHttpContext.result.GetContentType();
                }
                else
                {
                    // For soap 1.2, the we pull the action header from the content-type, so don't access the headers
                    // and just use the typed property. For other versions, we shouldn't need the headers up front.
                    this.contentType = hostedHttpContext.result.GetContentTypeFast();
                }

                this.contentLength = hostedHttpContext.result.GetContentLength();

                // MB#34947: System.Web signals chunked as 0 as well so the only way we can
                // differentiate is by reading ahead
                if (this.contentLength == 0)
                {
                    preReadBuffer = hostedHttpContext.result.GetPrereadBuffer(ref this.contentLength);
                }
            }

            public override long ContentLength
            {
                get
                {
                    return this.contentLength;
                }
            }

            protected override string ContentTypeCore
            {
                get
                {
                    return this.contentType;
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
                    return hostedHttpContext.result.GetSoapAction();
                }
            }

            protected override ChannelBinding ChannelBinding
            {
                get
                {
                    return ChannelBindingUtility.DuplicateToken(hostedHttpContext.result.GetChannelBinding());
                }
            }

            protected override void AddProperties(Message message)
            {
                HostedRequestContainer requestContainer = new HostedRequestContainer(this.hostedHttpContext.result);

                HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty(requestContainer);

                requestProperty.Method = this.hostedHttpContext.HttpMethod;

                // Uri.Query always includes the '?'
                if (this.hostedHttpContext.result.RequestUri.Query.Length > 1)
                {
                    requestProperty.QueryString = this.hostedHttpContext.result.RequestUri.Query.Substring(1);
                }

                message.Properties.Add(HttpRequestMessageProperty.Name, requestProperty);

                message.Properties.Add(HostingMessageProperty.Name, CreateMessagePropertyFromHostedResult(this.hostedHttpContext.result));
                message.Properties.Via = this.hostedHttpContext.result.RequestUri;

                RemoteEndpointMessageProperty remoteEndpointProperty = new RemoteEndpointMessageProperty(requestContainer);
                message.Properties.Add(RemoteEndpointMessageProperty.Name, remoteEndpointProperty);

                this.hostedHttpContext.SetRequestContainer(requestContainer);
            }

            public override void ConfigureHttpRequestMessage(HttpRequestMessage message)
            {
                message.Method = new HttpMethod(this.hostedHttpContext.result.GetHttpMethod());
                message.RequestUri = this.hostedHttpContext.result.RequestUri;
                foreach (string webHeaderKey in this.hostedHttpContext.result.Application.Context.Request.Headers.Keys)
                {
                    message.AddHeader(webHeaderKey, this.hostedHttpContext.result.Application.Context.Request.Headers[webHeaderKey]);
                }

                HostedRequestContainer requestContainer = new HostedRequestContainer(this.hostedHttpContext.result);
                RemoteEndpointMessageProperty remoteEndpointProperty = new RemoteEndpointMessageProperty(requestContainer);
                message.Properties.Add(RemoteEndpointMessageProperty.Name, remoteEndpointProperty);
            }

            [Fx.Tag.SecurityNote(Critical = "Calls critical .ctor(HostedImpersonationContext)",
                Safe = "Only accepts the incoming context from HostedHttpRequestAsyncResult which stores the context in a critical field")]
            [SecuritySafeCritical]
            static HostingMessageProperty CreateMessagePropertyFromHostedResult(HostedHttpRequestAsyncResult result)
            {
                return new HostingMessageProperty(result);
            }

            protected override Stream GetInputStream()
            {
                if (this.preReadBuffer != null)
                {
                    return new HostedInputStream(this.hostedHttpContext, this.preReadBuffer);
                }
                else
                {
                    return new HostedInputStream(this.hostedHttpContext);
                }
            }

            class HostedInputStream : HttpDelayedAcceptStream
            {
                HostedHttpRequestAsyncResult result;

                public HostedInputStream(HostedHttpContext hostedContext)
                    : base(hostedContext.result.GetInputStream())
                {
                    AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                    this.result = hostedContext.result;
                }

                public HostedInputStream(HostedHttpContext hostedContext, byte[] preReadBuffer)
                    : base(new PreReadStream(hostedContext.result.GetInputStream(), preReadBuffer))
                {
                    AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
                    this.result = hostedContext.result;
                }

                public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    if (!this.result.TryStartStreamedRead())
                    {
                        throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.RequestContextAborted));
                    }

                    bool throwing = true;

                    try
                    {
                        IAsyncResult result = base.BeginRead(buffer, offset, count, callback, state);
                        throwing = false;
                        return result;
                    }
                    catch (HttpException hostedException)
                    {
                        throw FxTrace.Exception.AsError(CreateCommunicationException(hostedException));
                    }
                    finally
                    {
                        if (throwing)
                        {
                            this.result.SetStreamedReadFinished();
                        }
                    }
                }

                public override int EndRead(IAsyncResult result)
                {
                    try
                    {
                        return base.EndRead(result);
                    }
                    catch (HttpException hostedException)
                    {
                        throw FxTrace.Exception.AsError(CreateCommunicationException(hostedException));
                    }
                    finally
                    {
                        this.result.SetStreamedReadFinished();
                    }
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    if (!this.result.TryStartStreamedRead())
                    {
                        throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.RequestContextAborted));
                    }

                    try
                    {
                        return base.Read(buffer, offset, count);
                    }
                    catch (HttpException hostedException)
                    {
                        throw FxTrace.Exception.AsError(CreateCommunicationException(hostedException));
                    }
                    finally
                    {
                        this.result.SetStreamedReadFinished();
                    }
                }

                // Wraps HttpException as inner exception in CommunicationException or ProtocolException (which derives from CommunicationException)
                static Exception CreateCommunicationException(HttpException hostedException)
                {
                    if (hostedException.WebEventCode == WebEventCodes.RuntimeErrorPostTooLarge)
                    {
                        // This HttpException is thrown if greater than httpRuntime/maxRequestLength bytes have been read from the stream.
                        // Note that this code path can only be hit when GetBufferedInputStream() is called in HostedHttpRequestAsyncResult.GetInputStream(), which only
                        // happens when an Http Module which is executed before the WCF Http Handler has accessed the request stream via GetBufferedInputStream().
                        // This is the only case that throws because GetBufferlessInputStream(true) ignores maxRequestLength, and InputStream property throws when invoked, not when stream is read.
                        return HttpInput.CreateHttpProtocolException(SR.Hosting_MaxRequestLengthExceeded, HttpStatusCode.RequestEntityTooLarge, null, hostedException);
                    }
                    else
                    {
                        // This HttpException is thrown if client disconnects and a read operation is invoked on the stream.
                        return new CommunicationException(hostedException.Message, hostedException);
                    }

                }
            }
        }

        class HostedRequestHttpOutput : HttpOutput
        {
            HostedHttpRequestAsyncResult result;
            HostedHttpContext context;
            string mimeVersion;
            string contentType;
            int statusCode;
            bool isSettingMimeHeader = false;
            bool isSettingContentType = false;

            public HostedRequestHttpOutput(HostedHttpRequestAsyncResult result, IHttpTransportFactorySettings settings,
                Message message, HostedHttpContext context)
                : base(settings, message, false, false)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

                this.result = result;
                this.context = context;

                if (TransferModeHelper.IsResponseStreamed(settings.TransferMode))
                    result.SetTransferModeToStreaming();

                if (message.IsFault)
                {
                    this.statusCode = (int)HttpStatusCode.InternalServerError;
                }
                else
                {
                    this.statusCode = (int)HttpStatusCode.OK;
                }
            }

            protected override Stream GetOutputStream()
            {
                return new HostedResponseOutputStream(this.result, this.context);
            }

            protected override void AddHeader(string name, string value)
            {
                this.result.AppendHeader(name, value);
            }

            protected override void AddMimeVersion(string version)
            {
                if (!isSettingMimeHeader)
                {
                    this.mimeVersion = version;
                }
                else
                {
                    this.result.AppendHeader(HttpChannelUtilities.MIMEVersionHeader, this.mimeVersion);
                }
            }

            protected override void SetContentType(string contentType)
            {
                if (!this.isSettingContentType)
                {
                    this.contentType = contentType;
                }
                else
                {
                    this.result.SetContentType(contentType);
                }
            }

            protected override void SetContentEncoding(string contentEncoding)
            {
                this.result.AppendHeader(HttpChannelUtilities.ContentEncodingHeader, contentEncoding);
            }

            protected override void SetContentLength(int contentLength)
            {
                this.result.AppendHeader("content-length", contentLength.ToString(CultureInfo.InvariantCulture));
            }

            protected override void SetStatusCode(HttpStatusCode statusCode)
            {
                this.result.SetStatusCode((int)statusCode);
            }

            protected override void SetStatusDescription(string statusDescription)
            {
                this.result.SetStatusDescription(statusDescription);
            }

            protected override bool PrepareHttpSend(Message message)
            {
                bool retValue = base.PrepareHttpSend(message);
                object property;

                bool httpMethodIsHead = string.Compare(this.context.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) == 0;
                if (httpMethodIsHead)
                {
                    retValue = true;
                }

                if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out property))
                {
                    HttpResponseMessageProperty responseProperty = (HttpResponseMessageProperty)property;

                    if (responseProperty.SuppressPreamble)
                    {
                        return retValue || responseProperty.SuppressEntityBody;
                    }

                    this.SetStatusCode(responseProperty.StatusCode);
                    if (responseProperty.StatusDescription != null)
                    {
                        this.SetStatusDescription(responseProperty.StatusDescription);
                    }

                    WebHeaderCollection responseHeaders = responseProperty.Headers;
                    for (int i = 0; i < responseHeaders.Count; i++)
                    {
                        string name = responseHeaders.Keys[i];
                        string value = responseHeaders[i];
                        if (string.Compare(name, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.contentType = value;
                        }
                        else if (string.Compare(name, HttpChannelUtilities.MIMEVersionHeader, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.mimeVersion = value;
                        }
                        else if (string.Compare(name, "content-length", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            int contentLength = -1;
                            if (httpMethodIsHead &&
                                int.TryParse(value, out contentLength))
                            {
                                this.SetContentLength(contentLength);
                            }
                        }
                        else
                        {
                            this.AddHeader(name, value);
                        }
                    }
                    if (responseProperty.SuppressEntityBody)
                    {
                        contentType = null;
                        retValue = true;
                    }
                }

                else
                {
                    this.SetStatusCode((HttpStatusCode)statusCode);
                }

                if (contentType != null && contentType.Length != 0)
                {
                    if (this.CanSendCompressedResponses)
                    {
                        string contentEncoding;
                        if (HttpChannelUtilities.GetHttpResponseTypeAndEncodingForCompression(ref contentType, out contentEncoding))
                        {
                            result.SetContentEncoding(contentEncoding);
                        }
                    }

                    this.isSettingContentType = true;
                    this.SetContentType(contentType);
                }

                if (this.mimeVersion != null)
                {
                    this.isSettingMimeHeader = true;
                    this.AddMimeVersion(this.mimeVersion);
                }

                return retValue;
            }

            protected override void PrepareHttpSendCore(HttpResponseMessage message)
            {
                result.SetStatusCode((int)message.StatusCode);
                if (message.ReasonPhrase != null)
                {
                    result.SetStatusDescription(message.ReasonPhrase);
                }
                HostedHttpContext.AppendHeaderFromHttpResponseMessageToResponse(message, this.result);
            }

            class HostedResponseOutputStream : BytesReadPositionStream
            {
                HostedHttpContext context;
                HostedHttpRequestAsyncResult result;

                public HostedResponseOutputStream(HostedHttpRequestAsyncResult result, HostedHttpContext context)
                    : base(result.GetOutputStream())
                {
                    this.context = context;
                    this.result = result;
                }

                public override void Close()
                {
                    try
                    {
                        base.Close();
                    }
                    catch (Exception e)
                    {
                        CheckWrapThrow(e);
                        throw;
                    }
                    finally
                    {
                        result.OnReplySent();
                    }
                }

                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    try
                    {
                        return base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch (Exception e)
                    {
                        CheckWrapThrow(e);
                        throw;
                    }
                }

                public override void EndWrite(IAsyncResult result)
                {
                    try
                    {
                        base.EndWrite(result);
                    }
                    catch (Exception e)
                    {
                        CheckWrapThrow(e);
                        throw;
                    }
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        base.Write(buffer, offset, count);
                    }
                    catch (Exception e)
                    {
                        CheckWrapThrow(e);
                        throw;
                    }
                }

                void CheckWrapThrow(Exception e)
                {
                    if (!Fx.IsFatal(e))
                    {
                        if (e is HttpException)
                        {
                            if (this.context.Aborted)
                            {
                                throw FxTrace.Exception.AsError(
                                    new CommunicationObjectAbortedException(SR.RequestContextAborted, e));
                            }
                            else
                            {
                                throw FxTrace.Exception.AsError(new CommunicationException(e.Message, e));
                            }
                        }
                        else if (this.context.Aborted)
                        {
                            // See VsWhidbey (594450)
                            if (DiagnosticUtility.ShouldTraceError)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.RequestContextAbort, SR.TraceCodeRequestContextAbort, this, e);
                            }

                            throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.RequestContextAborted));
                        }
                    }
                }
            }
        }

        class HostedRequestContainer : RemoteEndpointMessageProperty.IRemoteEndpointProvider, HttpRequestMessageProperty.IHttpHeaderProvider
        {
            volatile bool isClosed;
            HostedHttpRequestAsyncResult result;
            object thisLock;

            public HostedRequestContainer(HostedHttpRequestAsyncResult result)
            {
                AspNetPartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();

                this.result = result;
                this.thisLock = new object();
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            // IIS properties are not valid once the reply occurs.
            // Close invalidates all access to these properties.
            public void Close()
            {
                lock (this.ThisLock)
                {
                    this.isClosed = true;
                }
            }


            [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects",
                Safe = "Does not leak control or mutable/harmful data, no potential for harm")]
            [SecuritySafeCritical]
            void HttpRequestMessageProperty.IHttpHeaderProvider.CopyHeaders(WebHeaderCollection headers)
            {
                if (!this.isClosed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isClosed)
                        {
                            HttpChannelUtilities.CopyHeadersToNameValueCollection(this.result.Application.Request.Headers, headers);                            
                        }
                    }
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects",
                Safe = "Does not leak control or mutable/harmful data, no potential for harm")]
            [SecuritySafeCritical]
            string RemoteEndpointMessageProperty.IRemoteEndpointProvider.GetAddress()
            {
                if (!this.isClosed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isClosed)
                        {
                            return this.result.Application.Request.UserHostAddress;
                        }
                    }
                }
                return string.Empty;
            }


            [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects",
                Safe = "Does not leak control or mutable/harmful data, no potential for harm")]
            [SecuritySafeCritical]
            int RemoteEndpointMessageProperty.IRemoteEndpointProvider.GetPort()
            {
                int port = 0;

                if (!this.isClosed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isClosed)
                        {
                            string remotePort = this.result.Application.Request.ServerVariables["REMOTE_PORT"];
                            if (string.IsNullOrEmpty(remotePort) || !int.TryParse(remotePort, out port))
                            {
                                port = 0;
                            }
                        }
                    }
                }

                return port;
            }

            [Fx.Tag.SecurityNote(Critical = "Calls getters with LinkDemands in ASP .NET objects",
                Safe = "Does not leak control or mutable/harmful data, no potential for harm")]
            [SecuritySafeCritical]
            public bool TryGetAddressAndPort(out string address, out int port)
            {
                address = string.Empty;
                port = 0;

                if (!this.isClosed)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.isClosed)
                        {
                            address = this.result.Application.Request.UserHostAddress;

                            IServiceProvider provider = (IServiceProvider)result.Application.Context;
                            port = GetRemotePort(provider);
                            return true;
                        }
                    }
                }
                return false;
            }

            [Fx.Tag.SecurityNote(Critical = "Asserts UnmanagedCode to get the HttpWorkerRequest.", Safe = "Only returns the remote port, doesn't leak the HttpWorkerRequest.")]
            [SecuritySafeCritical]
            [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
            static int GetRemotePort(IServiceProvider provider)
            {
                return ((HttpWorkerRequest)provider.GetService(typeof(HttpWorkerRequest))).GetRemotePort();
            }
        }
    }
}
