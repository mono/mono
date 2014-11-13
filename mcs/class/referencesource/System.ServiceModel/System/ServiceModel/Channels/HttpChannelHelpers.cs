//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mime;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    // abstract out the common functionality of an "HttpInput"
    abstract class HttpInput
    {
        const string multipartRelatedMediaType = "multipart/related";
        const string startInfoHeaderParam = "start-info";
        const string defaultContentType = "application/octet-stream";

        BufferManager bufferManager;
        bool isRequest;
        MessageEncoder messageEncoder;
        IHttpTransportFactorySettings settings;
        bool streamed;
        WebException webException;
        Stream inputStream;
        bool enableChannelBinding;
        bool errorGettingInputStream;

        protected HttpInput(IHttpTransportFactorySettings settings, bool isRequest, bool enableChannelBinding)
        {
            this.settings = settings;
            this.bufferManager = settings.BufferManager;
            this.messageEncoder = settings.MessageEncoderFactory.Encoder;
            this.webException = null;
            this.isRequest = isRequest;
            this.inputStream = null;
            this.enableChannelBinding = enableChannelBinding;

            if (isRequest)
            {
                this.streamed = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
            }
            else
            {
                this.streamed = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
            }
        }

        internal static HttpInput CreateHttpInput(HttpWebResponse httpWebResponse, IHttpTransportFactorySettings settings, ChannelBinding channelBinding)
        {
            return new WebResponseHttpInput(httpWebResponse, settings, channelBinding);
        }

        internal WebException WebException
        {
            get { return webException; }
            set { webException = value; }
        }

        // Note: This method will return null in the case where throwOnError is false, and a non-fatal error occurs.
        // Please exercice caution when passing in throwOnError = false.  This should basically only be done in error
        // code paths, or code paths where there is very good reason that you would not want this method to throw.
        // When passing in throwOnError = false, please handle the case where this method returns null.
        public Stream GetInputStream(bool throwOnError)
        {
            if (inputStream == null && (throwOnError || !this.errorGettingInputStream))
            {
                try
                {
                    inputStream = GetInputStream();
                    this.errorGettingInputStream = false;
                }
                catch (Exception e)
                {
                    this.errorGettingInputStream = true;
                    if (throwOnError || Fx.IsFatal(e))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);
                }
            }

            return inputStream;
        }

        // -1 if chunked
        public abstract long ContentLength { get; }
        protected abstract string ContentTypeCore { get; }
        protected abstract bool HasContent { get; }
        protected abstract string SoapActionHeader { get; }
        protected abstract Stream GetInputStream();
        protected virtual ChannelBinding ChannelBinding { get { return null; } }

        protected string ContentType
        {
            get
            {
                string contentType = ContentTypeCore;

                if (string.IsNullOrEmpty(contentType))
                {
                    return defaultContentType;
                }

                return contentType;
            }
        }

        void ThrowMaxReceivedMessageSizeExceeded()
        {
            if (TD.MaxReceivedMessageSizeExceededIsEnabled())
            {
                TD.MaxReceivedMessageSizeExceeded(SR.GetString(SR.MaxReceivedMessageSizeExceeded, settings.MaxReceivedMessageSize));
            }

            if (isRequest)
            {
                ThrowHttpProtocolException(SR.GetString(SR.MaxReceivedMessageSizeExceeded, settings.MaxReceivedMessageSize), HttpStatusCode.RequestEntityTooLarge);
            }
            else
            {
                string message = SR.GetString(SR.MaxReceivedMessageSizeExceeded, settings.MaxReceivedMessageSize);
                Exception inner = new QuotaExceededException(message);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(message, inner));
            }
        }

        Message DecodeBufferedMessage(ArraySegment<byte> buffer, Stream inputStream)
        {
            try
            {
                // if we're chunked, make sure we've consumed the whole body
                if (ContentLength == -1 && buffer.Count == settings.MaxReceivedMessageSize)
                {
                    byte[] extraBuffer = new byte[1];
                    int extraReceived = inputStream.Read(extraBuffer, 0, 1);
                    if (extraReceived > 0)
                    {
                        ThrowMaxReceivedMessageSizeExceeded();
                    }
                }

                try
                {
                    return messageEncoder.ReadMessage(buffer, bufferManager, ContentType);
                }
                catch (XmlException xmlException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.MessageXmlProtocolError), xmlException));
                }
            }
            finally
            {
                inputStream.Close();
            }
        }

        Message ReadBufferedMessage(Stream inputStream)
        {
            ArraySegment<byte> messageBuffer = GetMessageBuffer();
            byte[] buffer = messageBuffer.Array;
            int offset = 0;
            int count = messageBuffer.Count;

            while (count > 0)
            {
                int bytesRead = inputStream.Read(buffer, offset, count);
                if (bytesRead == 0) // EOF 
                {
                    if (ContentLength != -1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.HttpContentLengthIncorrect)));
                    }

                    break;
                }
                count -= bytesRead;
                offset += bytesRead;
            }

            return DecodeBufferedMessage(new ArraySegment<byte>(buffer, 0, offset), inputStream);
        }

        Message ReadChunkedBufferedMessage(Stream inputStream)
        {
            try
            {
                return messageEncoder.ReadMessage(inputStream, bufferManager, settings.MaxBufferSize, ContentType);
            }
            catch (XmlException xmlException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.MessageXmlProtocolError), xmlException));
            }
        }

        Message ReadStreamedMessage(Stream inputStream)
        {
            MaxMessageSizeStream maxMessageSizeStream = new MaxMessageSizeStream(inputStream, settings.MaxReceivedMessageSize);

            try
            {
                return messageEncoder.ReadMessage(maxMessageSizeStream, settings.MaxBufferSize, ContentType);
            }
            catch (XmlException xmlException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ProtocolException(SR.GetString(SR.MessageXmlProtocolError), xmlException));
            }
        }

        protected abstract void AddProperties(Message message);

        void ApplyChannelBinding(Message message)
        {
            if (this.enableChannelBinding)
            {
                ChannelBindingUtility.TryAddToMessage(this.ChannelBinding, message, true);
            }
        }

        // makes sure that appropriate HTTP level headers are included in the received Message
        Exception ProcessHttpAddressing(Message message)
        {
            Exception result = null;
            AddProperties(message);

            // check if user is receiving WS-1 messages
            if (message.Version.Addressing == AddressingVersion.None)
            {
                bool actionAbsent = false;
                try
                {
                    actionAbsent = (message.Headers.Action == null);
                }
                catch (XmlException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }

                if (!actionAbsent)
                {
                    result = new ProtocolException(SR.GetString(SR.HttpAddressingNoneHeaderOnWire,
                        XD.AddressingDictionary.Action.Value));
                }

                bool toAbsent = false;
                try
                {
                    toAbsent = (message.Headers.To == null);
                }
                catch (XmlException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                catch (CommunicationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }

                if (!toAbsent)
                {
                    result = new ProtocolException(SR.GetString(SR.HttpAddressingNoneHeaderOnWire,
                        XD.AddressingDictionary.To.Value));
                }
                message.Headers.To = message.Properties.Via;
            }

            if (isRequest)
            {
                string action = null;

                if (message.Version.Envelope == EnvelopeVersion.Soap11)
                {
                    action = SoapActionHeader;
                }
                else if (message.Version.Envelope == EnvelopeVersion.Soap12 && !String.IsNullOrEmpty(ContentType))
                {
                    ContentType parsedContentType = new ContentType(ContentType);

                    if (parsedContentType.MediaType == multipartRelatedMediaType && parsedContentType.Parameters.ContainsKey(startInfoHeaderParam))
                    {
                        // fix to grab action from start-info as stated in RFC2387
                        action = new ContentType(parsedContentType.Parameters[startInfoHeaderParam]).Parameters["action"];
                    }
                    if (action == null)
                    {
                        // only if we can't find an action inside start-info
                        action = parsedContentType.Parameters["action"];
                    }
                }

                if (action != null)
                {
                    action = UrlUtility.UrlDecode(action, Encoding.UTF8);

                    if (action.Length >= 2 && action[0] == '"' && action[action.Length - 1] == '"')
                    {
                        action = action.Substring(1, action.Length - 2);
                    }

                    if (message.Version.Addressing == AddressingVersion.None)
                    {
                        message.Headers.Action = action;
                    }

                    try
                    {

                        if (action.Length > 0 && string.Compare(message.Headers.Action, action, StringComparison.Ordinal) != 0)
                        {
                            result = new ActionMismatchAddressingException(SR.GetString(SR.HttpSoapActionMismatchFault,
                                message.Headers.Action, action), message.Headers.Action, action);
                        }

                    }
                    catch (XmlException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                    catch (CommunicationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    }
                }
            }

            ApplyChannelBinding(message);

            if (DiagnosticUtility.ShouldUseActivity)
            {
                TraceUtility.TransferFromTransport(message);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageReceived, SR.GetString(SR.TraceCodeMessageReceived),
                    MessageTransmitTraceRecord.CreateReceiveTraceRecord(message), this, null, message);
            }

            // MessageLogger doesn't log AddressingVersion.None in the encoder since we want to make sure we log 
            // as much of the message as possible. Here we log after stamping the addressing information
            if (MessageLogger.LoggingEnabled && message.Version.Addressing == AddressingVersion.None)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportReceive | MessageLoggingSource.LastChance);
            }

            return result;
        }

        void ValidateContentType()
        {
            if (!HasContent)
                return;

            if (string.IsNullOrEmpty(ContentType))
            {
                if (MessageLogger.ShouldLogMalformed)
                {
                    // We pass in throwOnError = false below so that the exception which is eventually thrown is the ProtocolException below, with Http status code 415 "UnsupportedMediaType"
                    Stream stream = this.GetInputStream(false);
                    if (stream != null)
                    {
                        MessageLogger.LogMessage(stream, MessageLoggingSource.Malformed);
                    }
                }
                ThrowHttpProtocolException(SR.GetString(SR.HttpContentTypeHeaderRequired), HttpStatusCode.UnsupportedMediaType, HttpChannelUtilities.StatusDescriptionStrings.HttpContentTypeMissing);
            }
            if (!messageEncoder.IsContentTypeSupported(ContentType))
            {
                if (MessageLogger.ShouldLogMalformed)
                {
                    // We pass in throwOnError = false below so that the exception which is eventually thrown is the ProtocolException below, with Http status code 415 "UnsupportedMediaType"
                    Stream stream = this.GetInputStream(false);
                    if (stream != null)
                    {
                        MessageLogger.LogMessage(stream, MessageLoggingSource.Malformed);
                    }
                }
                string statusDescription = string.Format(CultureInfo.InvariantCulture, HttpChannelUtilities.StatusDescriptionStrings.HttpContentTypeMismatch, ContentType, messageEncoder.ContentType);
                ThrowHttpProtocolException(SR.GetString(SR.ContentTypeMismatch, ContentType, messageEncoder.ContentType), HttpStatusCode.UnsupportedMediaType, statusDescription);
            }
        }

        public IAsyncResult BeginParseIncomingMessage(AsyncCallback callback, object state)
        {
            return this.BeginParseIncomingMessage(null, callback, state);
        }

        public IAsyncResult BeginParseIncomingMessage(HttpRequestMessage httpRequestMessage, AsyncCallback callback, object state)
        {            
            bool throwing = true;
            try
            {
                IAsyncResult result = new ParseMessageAsyncResult(httpRequestMessage, this, callback, state);
                throwing = false;
                return result;
            }
            finally
            {
                if (throwing)
                {
                    Close();
                }
            }
        }

        public Message EndParseIncomingMessage(IAsyncResult result, out Exception requestException)
        {
            bool throwing = true;
            try
            {
                Message message = ParseMessageAsyncResult.End(result, out requestException);
                throwing = false;
                return message;
            }
            finally
            {
                if (throwing)
                {
                    Close();
                }
            }
        }

        public HttpRequestMessageHttpInput CreateHttpRequestMessageInput()
        {
            HttpRequestMessage message = new HttpRequestMessage();

            if (this.HasContent)
            {
                message.Content = new StreamContent(new MaxMessageSizeStream(this.GetInputStream(true), this.settings.MaxReceivedMessageSize));
            }

            HttpChannelUtilities.EnsureHttpRequestMessageContentNotNull(message);

            this.ConfigureHttpRequestMessage(message);
            ChannelBinding channelBinding = this.enableChannelBinding ? this.ChannelBinding : null;
            return new HttpRequestMessageHttpInput(message, this.settings, this.enableChannelBinding, channelBinding);
        }

        public abstract void ConfigureHttpRequestMessage(HttpRequestMessage message);

        public Message ParseIncomingMessage(out Exception requestException)
        {
            return this.ParseIncomingMessage(null, out requestException);
        }

        public Message ParseIncomingMessage(HttpRequestMessage httpRequestMessage, out Exception requestException)
        {
            Message message = null;
            requestException = null;
            bool throwing = true;
            try
            {
                ValidateContentType();

                ServiceModelActivity activity = null;
                if (DiagnosticUtility.ShouldUseActivity &&
                    ((ServiceModelActivity.Current == null) ||
                     (ServiceModelActivity.Current.ActivityType != ActivityType.ProcessAction)))
                {
                    activity = ServiceModelActivity.CreateBoundedActivity(true);
                }
                using (activity)
                {
                    if (DiagnosticUtility.ShouldUseActivity && activity != null)
                    {
                        // Only update the Start identifier if the activity is not null.
                        ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessingMessage, TraceUtility.RetrieveMessageNumber()), ActivityType.ProcessMessage);
                    }

                    if (!this.HasContent)
                    {
                        if (this.messageEncoder.MessageVersion == MessageVersion.None)
                        {
                            message = new NullMessage();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        Stream stream = this.GetInputStream(true);
                        if (streamed)
                        {
                            message = ReadStreamedMessage(stream);
                        }
                        else if (this.ContentLength == -1)
                        {
                            message = ReadChunkedBufferedMessage(stream);
                        }
                        else
                        {
                            if (httpRequestMessage == null)
                            {
                                message = ReadBufferedMessage(stream);
                            }
                            else
                            {
                                message = ReadBufferedMessage(httpRequestMessage);
                            }
                        }
                    }

                    requestException = ProcessHttpAddressing(message);

                    throwing = false;
                    return message;
                }
            }
            finally
            {
                if (throwing)
                {
                    Close();
                }
            }
        }

        Message ReadBufferedMessage(HttpRequestMessage httpRequestMessage)
        {
            Fx.Assert(httpRequestMessage != null, "httpRequestMessage cannot be null.");

            Message message;
            using (HttpContent currentContent = httpRequestMessage.Content)
            {
                int length = (int)this.ContentLength;
                byte[] buffer = this.bufferManager.TakeBuffer(length);
                bool success = false;
                try
                {
                    MemoryStream ms = new MemoryStream(buffer);
                    currentContent.CopyToAsync(ms).Wait<CommunicationException>();
                    httpRequestMessage.Content = new ByteArrayContent(buffer, 0, length);

                    foreach (var header in currentContent.Headers)
                    {
                        httpRequestMessage.Content.Headers.Add(header.Key, header.Value);
                    }

                    // 

                    message = this.messageEncoder.ReadMessage(new ArraySegment<byte>(buffer, 0, length), this.bufferManager, this.ContentType);
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        // We don't have to return it in success case since the buffer will be returned to bufferManager when the message is disposed.
                        this.bufferManager.ReturnBuffer(buffer);
                    }
                }
            }
            return message;
        }

        void ThrowHttpProtocolException(string message, HttpStatusCode statusCode)
        {
            ThrowHttpProtocolException(message, statusCode, null);
        }

        void ThrowHttpProtocolException(string message, HttpStatusCode statusCode, string statusDescription)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateHttpProtocolException(message, statusCode, statusDescription, webException));
        }

        internal static ProtocolException CreateHttpProtocolException(string message, HttpStatusCode statusCode, string statusDescription, Exception innerException)
        {
            ProtocolException exception = new ProtocolException(message, innerException);
            exception.Data.Add(HttpChannelUtilities.HttpStatusCodeExceptionKey, statusCode);
            if (statusDescription != null && statusDescription.Length > 0)
            {
                exception.Data.Add(HttpChannelUtilities.HttpStatusDescriptionExceptionKey, statusDescription);
            }

            return exception;
        }

        protected virtual void Close()
        {
        }

        ArraySegment<byte> GetMessageBuffer()
        {
            long count = ContentLength;
            int bufferSize;

            if (count > settings.MaxReceivedMessageSize)
            {
                ThrowMaxReceivedMessageSizeExceeded();
            }

            bufferSize = (int)count;

            return new ArraySegment<byte>(bufferManager.TakeBuffer(bufferSize), 0, bufferSize);
        }

        class ParseMessageAsyncResult : TraceAsyncResult
        {
            ArraySegment<byte> buffer;
            int count;
            int offset;
            HttpInput httpInput;
            Stream inputStream;
            Message message;
            Exception requestException = null;
            HttpRequestMessage httpRequestMessage;            
            static AsyncCallback onRead = Fx.ThunkCallback(new AsyncCallback(OnRead));

            public ParseMessageAsyncResult(
                HttpRequestMessage httpRequestMessage,
                HttpInput httpInput,
                AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.httpInput = httpInput;
                this.httpRequestMessage = httpRequestMessage;
                this.BeginParse();
            }

            void BeginParse()
            {
                httpInput.ValidateContentType();
                this.inputStream = httpInput.GetInputStream(true);

                if (!httpInput.HasContent)
                {
                    if (httpInput.messageEncoder.MessageVersion == MessageVersion.None)
                    {
                        this.message = new NullMessage();
                    }
                    else
                    {
                        base.Complete(true);
                        return;
                    }
                }
                else if (httpInput.streamed || httpInput.ContentLength == -1)
                {
                    if (httpInput.streamed)
                    {
                        this.message = httpInput.ReadStreamedMessage(inputStream);
                    }
                    else
                    {
                        this.message = httpInput.ReadChunkedBufferedMessage(inputStream);
                    }
                }

                if (this.message != null)
                {
                    this.requestException = httpInput.ProcessHttpAddressing(this.message);
                    base.Complete(true);
                    return;
                }

                AsyncCompletionResult result;
                if (httpRequestMessage == null)
                {
                    result = this.DecodeBufferedMessageAsync();
                }
                else
                {
                    result = this.DecodeBufferedHttpRequestMessageAsync();
                }

                if (result == AsyncCompletionResult.Completed)
                {
                    base.Complete(true);
                }
            }

            AsyncCompletionResult DecodeBufferedMessageAsync()
            {
                this.buffer = this.httpInput.GetMessageBuffer();
                this.count = this.buffer.Count;
                this.offset = 0;

                IAsyncResult result = inputStream.BeginRead(buffer.Array, offset, count, onRead, this);
                if (result.CompletedSynchronously)
                {
                    if (ContinueReading(inputStream.EndRead(result)))
                    {
                        return AsyncCompletionResult.Completed;
                    }
                }

                return AsyncCompletionResult.Queued;
            }

            bool ContinueReading(int bytesRead)
            {
                while (true)
                {
                    if (bytesRead == 0) // EOF
                    {
                        break;
                    }
                    else
                    {
                        offset += bytesRead;
                        count -= bytesRead;
                        if (count <= 0)
                        {
                            break;
                        }
                        else
                        {
                            IAsyncResult result = inputStream.BeginRead(buffer.Array, offset, count, onRead, this);
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }

                            bytesRead = inputStream.EndRead(result);
                        }
                    }
                }

                using (DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.BoundOperation(this.CallbackActivity) : null)
                {
                    using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity(true) : null)
                    {
                        if (DiagnosticUtility.ShouldUseActivity)
                        {
                            ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityProcessingMessage, TraceUtility.RetrieveMessageNumber()), ActivityType.ProcessMessage);
                        }

                        this.message = this.httpInput.DecodeBufferedMessage(new ArraySegment<byte>(buffer.Array, 0, offset), inputStream);
                        this.requestException = this.httpInput.ProcessHttpAddressing(this.message);
                    }
                    return true;
                }
            }

            static void OnRead(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                ParseMessageAsyncResult thisPtr = (ParseMessageAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf;
                try
                {
                    completeSelf = thisPtr.ContinueReading(thisPtr.inputStream.EndRead(result));
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

            public static Message End(IAsyncResult result, out Exception requestException)
            {
                ParseMessageAsyncResult thisPtr = AsyncResult.End<ParseMessageAsyncResult>(result);
                requestException = thisPtr.requestException;
                return thisPtr.message;
            }

            AsyncCompletionResult DecodeBufferedHttpRequestMessageAsync()
            {
                // Need to consider moving this to async implemenation for HttpContent reading.(CSDMAIN: 229108)
                this.message = this.httpInput.ReadBufferedMessage(this.httpRequestMessage);
                this.requestException = this.httpInput.ProcessHttpAddressing(this.message);
                return AsyncCompletionResult.Completed;
            }
        }

        class WebResponseHttpInput : HttpInput
        {
            HttpWebResponse httpWebResponse;
            byte[] preReadBuffer;
            ChannelBinding channelBinding;
            bool hasContent;

            public WebResponseHttpInput(HttpWebResponse httpWebResponse, IHttpTransportFactorySettings settings, ChannelBinding channelBinding)
                : base(settings, false, channelBinding != null)
            {
                this.channelBinding = channelBinding;
                this.httpWebResponse = httpWebResponse;
                if (this.httpWebResponse.ContentLength == -1)
                {
                    this.preReadBuffer = new byte[1];

                    if (this.httpWebResponse.GetResponseStream().Read(preReadBuffer, 0, 1) == 0)
                    {
                        this.preReadBuffer = null;
                    }
                }

                this.hasContent = (this.preReadBuffer != null || this.httpWebResponse.ContentLength > 0);
                if (!this.hasContent)
                {
                    // Close the response stream to avoid leaking the connection.
                    this.httpWebResponse.GetResponseStream().Close();
                }
            }

            protected override ChannelBinding ChannelBinding
            {
                get
                {
                    return this.channelBinding;
                }
            }

            public override long ContentLength
            {
                get
                {
                    return httpWebResponse.ContentLength;
                }
            }

            protected override string ContentTypeCore
            {
                get
                {
                    return httpWebResponse.ContentType;
                }
            }

            protected override bool HasContent
            {
                get { return this.hasContent; }
            }

            protected override string SoapActionHeader
            {
                get
                {
                    return httpWebResponse.Headers["SOAPAction"];
                }
            }

            protected override void AddProperties(Message message)
            {
                HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty(httpWebResponse.Headers);
                responseProperty.StatusCode = httpWebResponse.StatusCode;
                responseProperty.StatusDescription = httpWebResponse.StatusDescription;
                message.Properties.Add(HttpResponseMessageProperty.Name, responseProperty);
                message.Properties.Via = message.Version.Addressing.AnonymousUri;
            }

            public override void ConfigureHttpRequestMessage(HttpRequestMessage message)
            {
                // HTTP pipeline for client side is not implemented yet
                // DCR CSDMain 216853 is tracking this
                // This API is never going to be called in current stack
                Fx.Assert(false, "HTTP pipeline for client is not implemented yet. This method should not be called.");
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }

            protected override void Close()
            {
                try
                {
                    httpWebResponse.Close();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);
                }
            }

            protected override Stream GetInputStream()
            {
                Fx.Assert(this.HasContent, "this.HasContent must be true.");
                if (this.preReadBuffer != null)
                {
                    return new WebResponseInputStream(httpWebResponse, preReadBuffer);
                }
                else
                {
                    return new WebResponseInputStream(httpWebResponse);
                }
            }

            class WebResponseInputStream : DetectEofStream
            {
                // in order to avoid ----ing kernel buffers, we throttle our reads. http.sys
                // deals with this fine, but System.Net doesn't do any such throttling.
                const int maxSocketRead = 64 * 1024;
                HttpWebResponse webResponse;
                bool responseClosed;

                public WebResponseInputStream(HttpWebResponse httpWebResponse)
                    : base(httpWebResponse.GetResponseStream())
                {
                    this.webResponse = httpWebResponse;
                }

                public WebResponseInputStream(HttpWebResponse httpWebResponse, byte[] prereadBuffer)
                    : base(new PreReadStream(httpWebResponse.GetResponseStream(), prereadBuffer))
                {
                    this.webResponse = httpWebResponse;
                }


                public override void Close()
                {
                    base.Close();
                    CloseResponse();
                }

                protected override void OnReceivedEof()
                {
                    base.OnReceivedEof();
                    CloseResponse();
                }

                void CloseResponse()
                {
                    if (responseClosed)
                    {
                        return;
                    }

                    responseClosed = true;
                    this.webResponse.Close();
                }

                public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    try
                    {
                        return BaseStream.BeginRead(buffer, offset, Math.Min(count, maxSocketRead), callback, state);
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(ioException, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(objectDisposedException.Message, objectDisposedException));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(webException, this.webResponse));
                    }
                }

                public override int EndRead(IAsyncResult result)
                {
                    try
                    {
                        return BaseStream.EndRead(result);
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(ioException, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(objectDisposedException.Message, objectDisposedException));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(webException, this.webResponse));
                    }
                }

                public override int Read(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        return BaseStream.Read(buffer, offset, Math.Min(count, maxSocketRead));
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(objectDisposedException.Message, objectDisposedException));
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(ioException, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(webException, this.webResponse));
                    }
                }


                public override int ReadByte()
                {
                    try
                    {
                        return BaseStream.ReadByte();
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(objectDisposedException.Message, objectDisposedException));
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseIOException(ioException, TimeoutHelper.FromMilliseconds(this.ReadTimeout)));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateResponseWebException(webException, this.webResponse));
                    }
                }
            }
        }
    }

    // abstract out the common functionality of an "HttpOutput"
    abstract class HttpOutput
    {
        const string DefaultMimeVersion = "1.0";

        HttpAbortReason abortReason;
        bool isDisposed;
        bool isRequest;
        Message message;
        IHttpTransportFactorySettings settings;
        byte[] bufferToRecycle;
        BufferManager bufferManager;
        MessageEncoder messageEncoder;
        bool streamed;
        static Action<object> onStreamSendTimeout;
        string mtomBoundary;
        Stream outputStream;
        bool supportsConcurrentIO;
        EventTraceActivity eventTraceActivity;
        bool canSendCompressedResponses;

        protected HttpOutput(IHttpTransportFactorySettings settings, Message message, bool isRequest, bool supportsConcurrentIO)
        {
            this.settings = settings;
            this.message = message;
            this.isRequest = isRequest;
            this.bufferManager = settings.BufferManager;
            this.messageEncoder = settings.MessageEncoderFactory.Encoder;
            ICompressedMessageEncoder compressedMessageEncoder = this.messageEncoder as ICompressedMessageEncoder;
            this.canSendCompressedResponses = compressedMessageEncoder != null && compressedMessageEncoder.CompressionEnabled;
            if (isRequest)
            {
                this.streamed = TransferModeHelper.IsRequestStreamed(settings.TransferMode);
            }
            else
            {
                this.streamed = TransferModeHelper.IsResponseStreamed(settings.TransferMode);
            }
            this.supportsConcurrentIO = supportsConcurrentIO;

            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
            }
        }

        protected virtual bool IsChannelBindingSupportEnabled { get { return false; } }
        protected virtual ChannelBinding ChannelBinding { get { return null; } }

        protected void Abort()
        {
            Abort(HttpAbortReason.Aborted);
        }

        public virtual void Abort(HttpAbortReason reason)
        {
            if (isDisposed)
            {
                return;
            }

            this.abortReason = reason;

            TraceRequestResponseAborted(reason);

            CleanupBuffer();
        }

        private void TraceRequestResponseAborted(HttpAbortReason reason)
        {
            if (isRequest)
            {
                if (TD.HttpChannelRequestAbortedIsEnabled())
                {
                    TD.HttpChannelRequestAborted(this.eventTraceActivity);
                }
            }
            else if (TD.HttpChannelResponseAbortedIsEnabled())
            {
                TD.HttpChannelResponseAborted(this.eventTraceActivity);
            }

            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning,
                                        isRequest ? TraceCode.HttpChannelRequestAborted : TraceCode.HttpChannelResponseAborted,
                                        isRequest ? SR.GetString(SR.TraceCodeHttpChannelRequestAborted) : SR.GetString(SR.TraceCodeHttpChannelResponseAborted),
                                        this.message);
            }
        }

        public void Close()
        {
            if (isDisposed)
            {
                return;
            }

            try
            {
                if (this.outputStream != null)
                {
                    outputStream.Close();
                }
            }
            finally
            {
                CleanupBuffer();
            }
        }

        void CleanupBuffer()
        {
            byte[] bufferToRecycleSnapshot = Interlocked.Exchange<byte[]>(ref this.bufferToRecycle, null);
            if (bufferToRecycleSnapshot != null)
            {
                bufferManager.ReturnBuffer(bufferToRecycleSnapshot);
            }

            isDisposed = true;
        }

        protected abstract void AddMimeVersion(string version);
        protected abstract void AddHeader(string name, string value);
        protected abstract void SetContentType(string contentType);
        protected abstract void SetContentEncoding(string contentEncoding);
        protected abstract void SetStatusCode(HttpStatusCode statusCode);
        protected abstract void SetStatusDescription(string statusDescription);
        protected virtual bool CleanupChannelBinding { get { return true; } }
        protected virtual void SetContentLength(int contentLength)
        {
        }

        protected virtual string HttpMethod { get { return null; } }

        public virtual ChannelBinding TakeChannelBinding()
        {
            return null;
        }

        private void ApplyChannelBinding()
        {
            if (this.IsChannelBindingSupportEnabled)
            {
                ChannelBindingUtility.TryAddToMessage(this.ChannelBinding, this.message, this.CleanupChannelBinding);
            }
        }

        protected abstract Stream GetOutputStream();

        protected virtual bool WillGetOutputStreamCompleteSynchronously
        {
            get { return true; }
        }

        protected bool CanSendCompressedResponses
        {
            get { return this.canSendCompressedResponses; }
        }

        protected virtual IAsyncResult BeginGetOutputStream(AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        protected virtual Stream EndGetOutputStream(IAsyncResult result)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public void ConfigureHttpResponseMessage(Message message, HttpResponseMessage httpResponseMessage, HttpResponseMessageProperty responseProperty)
        {
            HttpChannelUtilities.EnsureHttpResponseMessageContentNotNull(httpResponseMessage);

            string action = message.Headers.Action;

            if (message.Version.Addressing == AddressingVersion.None)
            {
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    message.Properties.Add(AddressingProperty.Name, new AddressingProperty(message.Headers));
                }

                message.Headers.Action = null;
                message.Headers.To = null;
            }

            bool httpResponseMessagePropertyFound = responseProperty != null;

            string contentType = null;
            if (message.Version == MessageVersion.None && httpResponseMessagePropertyFound && !string.IsNullOrEmpty(responseProperty.Headers[HttpResponseHeader.ContentType]))
            {
                contentType = responseProperty.Headers[HttpResponseHeader.ContentType];
                responseProperty.Headers.Remove(HttpResponseHeader.ContentType);
                if (!messageEncoder.IsContentTypeSupported(contentType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ProtocolException(SR.GetString(SR.ResponseContentTypeNotSupported,
                        contentType)));
                }
            }

            if (string.IsNullOrEmpty(contentType))
            {
                MtomMessageEncoder mtomMessageEncoder = messageEncoder as MtomMessageEncoder;
                if (mtomMessageEncoder == null)
                {
                    contentType = messageEncoder.ContentType;
                }
                else
                {
                    contentType = mtomMessageEncoder.GetContentType(out this.mtomBoundary);
                    // For MTOM messages, add a MIME version header
                    httpResponseMessage.Headers.Add(HttpChannelUtilities.MIMEVersionHeader, DefaultMimeVersion);
                }
            }

            if (isRequest && FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                EnsureEventTraceActivity(message);
            }

            if (this.CanSendCompressedResponses)
            {
                string contentEncoding;
                string compressionContentType = contentType;
                if (HttpChannelUtilities.GetHttpResponseTypeAndEncodingForCompression(ref compressionContentType, out contentEncoding))
                {
                    contentType = compressionContentType;
                    this.SetContentEncoding(contentEncoding);
                }
            }

            if (httpResponseMessage.Content != null && !string.IsNullOrEmpty(contentType))
            {
                MediaTypeHeaderValue mediaTypeHeaderValue;
                if (!MediaTypeHeaderValue.TryParse(contentType, out mediaTypeHeaderValue))
                {
                    throw FxTrace.Exception.Argument("contentType", SR.GetString(SR.InvalidContentTypeError, contentType));
                }
                httpResponseMessage.Content.Headers.ContentType = mediaTypeHeaderValue;
            }

            bool httpMethodIsHead = string.Compare(this.HttpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) == 0;

            if (httpMethodIsHead ||
                httpResponseMessagePropertyFound && responseProperty.SuppressEntityBody)
            {
                httpResponseMessage.Content.Headers.ContentLength = 0;
                httpResponseMessage.Content.Headers.ContentType = null;
            }

            if (httpResponseMessagePropertyFound)
            {
                httpResponseMessage.StatusCode = responseProperty.StatusCode;
                if (responseProperty.StatusDescription != null)
                {
                    responseProperty.StatusDescription = responseProperty.StatusDescription;
                }

                foreach (string key in responseProperty.Headers.AllKeys)
                {
                    httpResponseMessage.AddHeader(key, responseProperty.Headers[key]);
                }
            }

            if (!message.IsEmpty)
            {
                using (HttpContent content = httpResponseMessage.Content)
                {
                    if (this.streamed)
                    {
                        IStreamedMessageEncoder streamedMessageEncoder = this.messageEncoder as IStreamedMessageEncoder;
                        Stream stream = null;
                        if (streamedMessageEncoder != null)
                        {
                            stream = streamedMessageEncoder.GetResponseMessageStream(message);
                        }

                        if (stream != null)
                        {
                            httpResponseMessage.Content = new StreamContent(stream);
                        }
                        else
                        {
                            httpResponseMessage.Content = new OpaqueContent(this.messageEncoder, message, this.mtomBoundary);
                        }
                    }
                    else
                    {
                        // HttpOutputByteArrayContent assumes responsibility for returning the buffer to the bufferManager. 
                        ArraySegment<byte> messageBytes = this.SerializeBufferedMessage(message, false);
                        httpResponseMessage.Content = new HttpOutputByteArrayContent(messageBytes.Array, messageBytes.Offset, messageBytes.Count, this.bufferManager);
                    }

                    httpResponseMessage.Content.Headers.Clear();
                    foreach (var header in content.Headers)
                    {
                        httpResponseMessage.Content.Headers.Add(header.Key, header.Value);
                    }
                }
            }
        }

        protected virtual bool PrepareHttpSend(Message message)
        {
            string action = message.Headers.Action;

            if (message.Version.Addressing == AddressingVersion.None)
            {
                if (MessageLogger.LogMessagesAtTransportLevel)
                {
                    message.Properties.Add(AddressingProperty.Name, new AddressingProperty(message.Headers));
                }

                message.Headers.Action = null;
                message.Headers.To = null;
            }

            string contentType = null;

            if (message.Version == MessageVersion.None)
            {
                object property = null;
                if (message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out property))
                {
                    HttpResponseMessageProperty responseProperty = (HttpResponseMessageProperty)property;
                    if (!string.IsNullOrEmpty(responseProperty.Headers[HttpResponseHeader.ContentType]))
                    {
                        contentType = responseProperty.Headers[HttpResponseHeader.ContentType];
                        if (!messageEncoder.IsContentTypeSupported(contentType))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new ProtocolException(SR.GetString(SR.ResponseContentTypeNotSupported,
                                contentType)));
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(contentType))
            {
                MtomMessageEncoder mtomMessageEncoder = messageEncoder as MtomMessageEncoder;
                if (mtomMessageEncoder == null)
                {
                    contentType = messageEncoder.ContentType;
                }
                else
                {
                    contentType = mtomMessageEncoder.GetContentType(out this.mtomBoundary);
                    // For MTOM messages, add a MIME version header
                    AddMimeVersion("1.0");
                }
            }

            if (isRequest && FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
            {
                EnsureEventTraceActivity(message);
            }

            SetContentType(contentType);
            return message is NullMessage;
        }

        protected bool PrepareHttpSend(HttpResponseMessage httpResponseMessage)
        {
            this.PrepareHttpSendCore(httpResponseMessage);
            return HttpChannelUtilities.IsEmpty(httpResponseMessage);
        }

        protected abstract void PrepareHttpSendCore(HttpResponseMessage message);

        private static void EnsureEventTraceActivity(Message message)
        {
            //We need to send this only if there is no message id. 
            if (message.Headers.MessageId == null)
            {
                EventTraceActivity eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                if (eventTraceActivity == null)
                {
                    //Whoops no activity on the message yet.                         
                    eventTraceActivity = new EventTraceActivity();
                    EventTraceActivityHelper.TryAttachActivity(message, eventTraceActivity);
                }

                HttpRequestMessageProperty httpProperties;
                if (!message.Properties.TryGetValue<HttpRequestMessageProperty>(HttpRequestMessageProperty.Name, out httpProperties))
                {
                    httpProperties = new HttpRequestMessageProperty();
                    message.Properties.Add(HttpRequestMessageProperty.Name, httpProperties);
                }
                httpProperties.Headers.Add(EventTraceActivity.Name, Convert.ToBase64String(eventTraceActivity.ActivityId.ToByteArray()));
            }
        }

        ArraySegment<byte> SerializeBufferedMessage(Message message)
        {
            // by default, the HttpOutput should own the buffer and clean it up
            return SerializeBufferedMessage(message, true);
        }

        ArraySegment<byte> SerializeBufferedMessage(Message message, bool shouldRecycleBuffer)
        {
            ArraySegment<byte> result;

            MtomMessageEncoder mtomMessageEncoder = messageEncoder as MtomMessageEncoder;
            if (mtomMessageEncoder == null)
            {
                result = messageEncoder.WriteMessage(message, int.MaxValue, bufferManager);
            }
            else
            {
                result = mtomMessageEncoder.WriteMessage(message, int.MaxValue, bufferManager, 0, this.mtomBoundary);
            }

            if (shouldRecycleBuffer)
            {
                // Only set this.bufferToRecycle if the HttpOutput owns the buffer, we will clean it up upon httpOutput.Close()
                // Otherwise, caller of SerializeBufferedMessage assumes responsiblity for returning the buffer to the buffer pool
            this.bufferToRecycle = result.Array;
            }
            return result;
        }

        Stream GetWrappedOutputStream()
        {
            const int ChunkSize = 32768;    // buffer size used for synchronous writes
            const int BufferSize = 16384;   // buffer size used for asynchronous writes
            const int BufferCount = 4;      // buffer count used for asynchronous writes

            // Writing an HTTP request chunk has a high fixed cost, so use BufferedStream to avoid writing 
            // small ones. 
            return this.supportsConcurrentIO ? (Stream)new BufferedOutputAsyncStream(this.outputStream, BufferSize, BufferCount) : new BufferedStream(this.outputStream, ChunkSize);
        }

        void WriteStreamedMessage(TimeSpan timeout)
        {
            this.outputStream = GetWrappedOutputStream();

            // Since HTTP streams don't support timeouts, we can't just use TimeoutStream here. 
            // Rather, we need to run a timer to bound the overall operation
            if (onStreamSendTimeout == null)
            {
                onStreamSendTimeout = new Action<object>(OnStreamSendTimeout);
            }
            IOThreadTimer sendTimer = new IOThreadTimer(onStreamSendTimeout, this, true);
            sendTimer.Set(timeout);

            try
            {
                MtomMessageEncoder mtomMessageEncoder = messageEncoder as MtomMessageEncoder;
                if (mtomMessageEncoder == null)
                {
                    messageEncoder.WriteMessage(this.message, this.outputStream);
                }
                else
                {
                    mtomMessageEncoder.WriteMessage(this.message, this.outputStream, this.mtomBoundary);
                }
            }
            finally
            {
                sendTimer.Cancel();
            }
        }

        static void OnStreamSendTimeout(object state)
        {
            HttpOutput thisPtr = (HttpOutput)state;
            thisPtr.Abort(HttpAbortReason.TimedOut);
        }

        IAsyncResult BeginWriteStreamedMessage(HttpResponseMessage httpResponseMessage, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WriteStreamedMessageAsyncResult(timeout, this, httpResponseMessage, callback, state);
        }

        void EndWriteStreamedMessage(IAsyncResult result)
        {
            WriteStreamedMessageAsyncResult.End(result);
        }

        class HttpOutputByteArrayContent : ByteArrayContent
        {
            BufferManager bufferManager;
            volatile bool cleaned = false;
            ArraySegment<byte> content;

            public HttpOutputByteArrayContent(byte[] content, int offset, int count, BufferManager bufferManager)
                : base(content, offset, count)
            {
                Fx.Assert(bufferManager != null, "bufferManager should not be null");
                Fx.Assert(content != null, "content should not be null");
                this.content = new ArraySegment<byte>(content, offset, count);
                this.bufferManager = bufferManager;
            }

            public ArraySegment<byte> Content
            {
                get
                {
                    return this.content;
                }
            }

            protected override Task<Stream> CreateContentReadStreamAsync()
            {
                return base.CreateContentReadStreamAsync().ContinueWith<Stream>(t => 
                    new HttpOutputByteArrayContentStream(t.Result, this));
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return base.SerializeToStreamAsync(stream, context).ContinueWith(t =>
                    {
                        this.Cleanup();
                        HttpChannelUtilities.HandleContinueWithTask(t);
                    });
            }

            void Cleanup()
            {
                if (!cleaned)
                {
                    lock (this)
                    {
                        if (!cleaned)
                        {
                            cleaned = true;
                            this.bufferManager.ReturnBuffer(this.content.Array);
                        }
                    }
                }
            }

            class HttpOutputByteArrayContentStream : DelegatingStream
            {
                HttpOutputByteArrayContent content;

                public HttpOutputByteArrayContentStream(Stream innerStream, HttpOutputByteArrayContent content)
                    : base(innerStream)
                {
                    this.content = content;
                }

                public override void Close()
                {
                    base.Close();
                    this.content.Cleanup();
                }
            }
        }

        class WriteStreamedMessageAsyncResult : AsyncResult
        {
            HttpOutput httpOutput;
            IOThreadTimer sendTimer;
            static AsyncCallback onWriteStreamedMessage = Fx.ThunkCallback(OnWriteStreamedMessage);
            HttpResponseMessage httpResponseMessage;

            public WriteStreamedMessageAsyncResult(TimeSpan timeout, HttpOutput httpOutput, HttpResponseMessage httpResponseMessage, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.httpResponseMessage = httpResponseMessage;
                this.httpOutput = httpOutput;
                httpOutput.outputStream = httpOutput.GetWrappedOutputStream();

                // Since HTTP streams don't support timeouts, we can't just use TimeoutStream here. 
                // Rather, we need to run a timer to bound the overall operation
                if (onStreamSendTimeout == null)
                {
                    onStreamSendTimeout = new Action<object>(OnStreamSendTimeout);
                }
                this.SetTimer(timeout);

                bool completeSelf = false;
                bool throwing = true;

                try
                {
                    completeSelf = HandleWriteStreamedMessage(null);
                    throwing = false;
                }
                finally
                {
                    if (completeSelf || throwing)
                    {
                        this.sendTimer.Cancel();
                    }
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            bool HandleWriteStreamedMessage(IAsyncResult result)
            {
                if (this.httpResponseMessage == null)
                {
                    if (result == null)
                    {
                        MtomMessageEncoder mtomMessageEncoder = httpOutput.messageEncoder as MtomMessageEncoder;
                        if (mtomMessageEncoder == null)
                        {
                            result = httpOutput.messageEncoder.BeginWriteMessage(httpOutput.message, httpOutput.outputStream, onWriteStreamedMessage, this);
                        }
                        else
                        {
                            result = mtomMessageEncoder.BeginWriteMessage(httpOutput.message, httpOutput.outputStream, httpOutput.mtomBoundary, onWriteStreamedMessage, this);
                        }

                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                    }

                    httpOutput.messageEncoder.EndWriteMessage(result);
                    return true;
                }
                else
                {
                    OpaqueContent content = this.httpResponseMessage.Content as OpaqueContent;
                    if (result == null)
                    {
                        Fx.Assert(this.httpResponseMessage.Content != null, "httpOutput.httpResponseMessage.Content should not be null.");

                        if (content != null)
                        {
                            result = content.BeginWriteToStream(httpOutput.outputStream, onWriteStreamedMessage, this);
                        }
                        else
                        {
                            result = this.httpResponseMessage.Content.CopyToAsync(httpOutput.outputStream).AsAsyncResult(onWriteStreamedMessage, this);
                        }

                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                    }

                    if (content != null)
                    {
                        content.EndWriteToStream(result);
                    }

                    return true;
                }
            }

            static void OnWriteStreamedMessage(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                WriteStreamedMessageAsyncResult thisPtr = (WriteStreamedMessageAsyncResult)result.AsyncState;
                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    completeSelf = thisPtr.HandleWriteStreamedMessage(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = ex;
                }

                if (completeSelf)
                {
                    thisPtr.sendTimer.Cancel();
                    thisPtr.Complete(false, completionException);
                }
            }

            void SetTimer(TimeSpan timeout)
            {
                Fx.Assert(this.sendTimer == null, "SetTimer should only be called once");

                this.sendTimer = new IOThreadTimer(onStreamSendTimeout, this.httpOutput, true);
                this.sendTimer.Set(timeout);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteStreamedMessageAsyncResult>(result);
            }
        }

        public IAsyncResult BeginSend(HttpResponseMessage httpResponseMessage, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(httpResponseMessage != null, "httpResponseMessage should not be null.");
            return this.BeginSendCore(httpResponseMessage, timeout, callback, state);
        }

        public IAsyncResult BeginSend(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginSendCore(null, timeout, callback, state);
        }

        IAsyncResult BeginSendCore(HttpResponseMessage httpResponseMessage, TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool throwing = true;
            try
            {
                bool suppressEntityBody;
                if (httpResponseMessage != null)
                {
                    suppressEntityBody = this.PrepareHttpSend(httpResponseMessage);
                }
                else
                {
                    suppressEntityBody = PrepareHttpSend(message);
                }

                this.TraceHttpSendStart();
                IAsyncResult result = new SendAsyncResult(this, httpResponseMessage, suppressEntityBody, timeout, callback, state);
                throwing = false;
                return result;
            }
            finally
            {
                if (throwing)
                {
                    Abort();
                }
            }
        }

        private void TraceHttpSendStart()
        {
            if (TD.HttpSendMessageStartIsEnabled())
            {
                if (streamed)
                {
                    TD.HttpSendStreamedMessageStart(this.eventTraceActivity);
                }
                else
                {
                    TD.HttpSendMessageStart(this.eventTraceActivity);
                }
            }
        }

        public virtual void EndSend(IAsyncResult result)
        {
            bool throwing = true;
            try
            {
                SendAsyncResult.End(result);
                throwing = false;
            }
            finally
            {
                if (throwing)
                {
                    Abort();
                }
            }
        }

        void LogMessage()
        {
            if (MessageLogger.LogMessagesAtTransportLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.TransportSend);
            }
        }

        public void Send(HttpResponseMessage httpResponseMessage, TimeSpan timeout)
        {
            bool suppressEntityBody = this.PrepareHttpSend(httpResponseMessage);

            TraceHttpSendStart();

            if (suppressEntityBody)
            {
                // requests can't always support an output stream (for GET, etc)
                if (!isRequest)
                {
                    outputStream = GetOutputStream();
                }
                else
                {
                    this.SetContentLength(0);
                    LogMessage();
                }
            }
            else if (streamed)
            {
                outputStream = this.GetOutputStream();
                ApplyChannelBinding();

                OpaqueContent content = httpResponseMessage.Content as OpaqueContent;
                if (content != null)
                {
                    content.WriteToStream(this.outputStream);
                }
                else
                {
                    if (!httpResponseMessage.Content.CopyToAsync(this.outputStream).Wait<CommunicationException>(timeout))
                    {
                        throw FxTrace.Exception.AsError(new TimeoutException(SR.GetString(SR.TimeoutOnSend, timeout)));
                    }
                }
            }
            else
            {
                if (this.IsChannelBindingSupportEnabled)
                {
                    //need to get the Channel binding token (CBT), apply channel binding info to the message and then write the message                    
                    //CBT is only enabled when message security is in the stack, which also requires an HTTP entity body, so we 
                    //should be safe to always get the stream.
                    outputStream = this.GetOutputStream();

                    ApplyChannelBinding();

                    ArraySegment<byte> buffer = SerializeBufferedMessage(httpResponseMessage);

                    Fx.Assert(buffer.Count != 0, "We should always have an entity body in this case...");
                    outputStream.Write(buffer.Array, buffer.Offset, buffer.Count);
                }
                else
                {
                    ArraySegment<byte> buffer = SerializeBufferedMessage(httpResponseMessage);
                    SetContentLength(buffer.Count);

                    // requests can't always support an output stream (for GET, etc)
                    if (!isRequest || buffer.Count > 0)
                    {
                        outputStream = this.GetOutputStream();
                        outputStream.Write(buffer.Array, buffer.Offset, buffer.Count);
                    }
                }
            }

            TraceSend();
        }

        ArraySegment<byte> SerializeBufferedMessage(HttpResponseMessage httpResponseMessage)
        {
            HttpOutputByteArrayContent content = httpResponseMessage.Content as HttpOutputByteArrayContent;
            if (content == null)
            {
                byte[] byteArray = httpResponseMessage.Content.ReadAsByteArrayAsync().Result;
                return new ArraySegment<byte>(byteArray, 0, byteArray.Length);
            }
            else
            {
                return content.Content;
            }
        }

        public void Send(TimeSpan timeout)
        {
            bool suppressEntityBody = PrepareHttpSend(message);

            TraceHttpSendStart();

            if (suppressEntityBody)
            {
                // requests can't always support an output stream (for GET, etc)
                if (!isRequest)
                {
                    outputStream = GetOutputStream();
                }
                else
                {
                    this.SetContentLength(0);
                    LogMessage();
                }
            }
            else if (streamed)
            {
                outputStream = GetOutputStream();
                ApplyChannelBinding();
                WriteStreamedMessage(timeout);
            }
            else
            {
                if (this.IsChannelBindingSupportEnabled)
                {
                    //need to get the Channel binding token (CBT), apply channel binding info to the message and then write the message                    
                    //CBT is only enabled when message security is in the stack, which also requires an HTTP entity body, so we 
                    //should be safe to always get the stream.
                    outputStream = GetOutputStream();

                    ApplyChannelBinding();

                    ArraySegment<byte> buffer = SerializeBufferedMessage(message);

                    Fx.Assert(buffer.Count != 0, "We should always have an entity body in this case...");
                    outputStream.Write(buffer.Array, buffer.Offset, buffer.Count);
                }
                else
                {
                    ArraySegment<byte> buffer = SerializeBufferedMessage(message);
                    SetContentLength(buffer.Count);

                    // requests can't always support an output stream (for GET, etc)
                    if (!isRequest || buffer.Count > 0)
                    {
                        outputStream = GetOutputStream();
                        outputStream.Write(buffer.Array, buffer.Offset, buffer.Count);
                    }
                }
            }

            TraceSend();
        }

        void TraceSend()
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.MessageSent, SR.GetString(SR.TraceCodeMessageSent),
                    new MessageTraceRecord(this.message), this, null);
            }

            if (TD.HttpSendStopIsEnabled())
            {
                TD.HttpSendStop(this.eventTraceActivity);
            }
        }

        class SendAsyncResult : AsyncResult
        {
            HttpOutput httpOutput;
            static AsyncCallback onGetOutputStream;
            static Action<object> onWriteStreamedMessageLater;
            static AsyncCallback onWriteStreamedMessage;
            static AsyncCallback onWriteBody;
            bool suppressEntityBody;
            ArraySegment<byte> buffer;
            TimeoutHelper timeoutHelper;
            HttpResponseMessage httpResponseMessage;

            public SendAsyncResult(HttpOutput httpOutput, HttpResponseMessage httpResponseMessage, bool suppressEntityBody, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.httpOutput = httpOutput;
                this.httpResponseMessage = httpResponseMessage;
                this.suppressEntityBody = suppressEntityBody;

                if (suppressEntityBody)
                {
                    if (httpOutput.isRequest)
                    {
                        httpOutput.SetContentLength(0);
                        this.httpOutput.TraceSend();
                        this.httpOutput.LogMessage();
                        base.Complete(true);
                        return;
                    }
                }

                this.timeoutHelper = new TimeoutHelper(timeout);
                Send();
            }

            void Send()
            {
                if (httpOutput.IsChannelBindingSupportEnabled)
                {
                    SendWithChannelBindingToken();
                }
                else
                {
                    SendWithoutChannelBindingToken();
                }
            }

            void SendWithoutChannelBindingToken()
            {
                if (!suppressEntityBody && !httpOutput.streamed)
                {
                    if (this.httpResponseMessage != null)
                    {
                        buffer = httpOutput.SerializeBufferedMessage(this.httpResponseMessage);
                    }
                    else
                    {
                        buffer = httpOutput.SerializeBufferedMessage(httpOutput.message);
                    }

                    httpOutput.SetContentLength(buffer.Count);
                }


                if (this.httpOutput.WillGetOutputStreamCompleteSynchronously)
                {
                    httpOutput.outputStream = httpOutput.GetOutputStream();
                }
                else
                {
                    if (onGetOutputStream == null)
                    {
                        onGetOutputStream = Fx.ThunkCallback(new AsyncCallback(OnGetOutputStream));
                    }

                    IAsyncResult result = httpOutput.BeginGetOutputStream(onGetOutputStream, this);

                    if (!result.CompletedSynchronously)
                        return;

                    httpOutput.outputStream = httpOutput.EndGetOutputStream(result);
                }

                if (WriteMessage(true))
                {
                    this.httpOutput.TraceSend();
                    base.Complete(true);
                }
            }

            void SendWithChannelBindingToken()
            {
                if (this.httpOutput.WillGetOutputStreamCompleteSynchronously)
                {
                    httpOutput.outputStream = httpOutput.GetOutputStream();
                    httpOutput.ApplyChannelBinding();
                }
                else
                {
                    if (onGetOutputStream == null)
                    {
                        onGetOutputStream = Fx.ThunkCallback(new AsyncCallback(OnGetOutputStream));
                    }

                    IAsyncResult result = httpOutput.BeginGetOutputStream(onGetOutputStream, this);

                    if (!result.CompletedSynchronously)
                        return;

                    httpOutput.outputStream = httpOutput.EndGetOutputStream(result);
                    httpOutput.ApplyChannelBinding();
                }

                if (!httpOutput.streamed)
                {
                    if (this.httpResponseMessage != null)
                    {
                        buffer = httpOutput.SerializeBufferedMessage(this.httpResponseMessage);
                    }
                    else
                    {
                        buffer = httpOutput.SerializeBufferedMessage(httpOutput.message);
                    }

                    httpOutput.SetContentLength(buffer.Count);
                }

                if (WriteMessage(true))
                {
                    this.httpOutput.TraceSend();
                    base.Complete(true);
                }
            }

            bool WriteMessage(bool isStillSynchronous)
            {
                if (suppressEntityBody)
                {
                    return true;
                }
                if (httpOutput.streamed)
                {
                    if (isStillSynchronous)
                    {
                        if (onWriteStreamedMessageLater == null)
                        {
                            onWriteStreamedMessageLater = new Action<object>(OnWriteStreamedMessageLater);
                        }
                        ActionItem.Schedule(onWriteStreamedMessageLater, this);
                        return false;
                    }
                    else
                    {
                        return WriteStreamedMessage();
                    }
                }
                else
                {
                    if (onWriteBody == null)
                    {
                        onWriteBody = Fx.ThunkCallback(new AsyncCallback(OnWriteBody));
                    }

                    IAsyncResult writeResult =
                        httpOutput.outputStream.BeginWrite(buffer.Array, buffer.Offset, buffer.Count, onWriteBody, this);

                    if (!writeResult.CompletedSynchronously)
                    {
                        return false;
                    }

                    CompleteWriteBody(writeResult);
                }

                return true;
            }

            bool WriteStreamedMessage()
            {
                // return a bool to determine if we are [....]. 

                if (onWriteStreamedMessage == null)
                {
                    onWriteStreamedMessage = Fx.ThunkCallback(OnWriteStreamedMessage);
                }

                return HandleWriteStreamedMessage(null); // completed synchronously
            }

            bool HandleWriteStreamedMessage(IAsyncResult result)
            {
                if (result == null)
                {
                    result = httpOutput.BeginWriteStreamedMessage(this.httpResponseMessage, timeoutHelper.RemainingTime(), onWriteStreamedMessage, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                httpOutput.EndWriteStreamedMessage(result);
                return true;
            }

            static void OnWriteStreamedMessage(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;
                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    completeSelf = thisPtr.HandleWriteStreamedMessage(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = ex;
                }

                if (completeSelf)
                {
                    if (completionException != null)
                    {
                        thisPtr.httpOutput.TraceSend();
                    }
                    thisPtr.Complete(false, completionException);
                }
            }

            void CompleteWriteBody(IAsyncResult result)
            {
                httpOutput.outputStream.EndWrite(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SendAsyncResult>(result);
            }

            static void OnGetOutputStream(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    thisPtr.httpOutput.outputStream = thisPtr.httpOutput.EndGetOutputStream(result);
                    thisPtr.httpOutput.ApplyChannelBinding();

                    if (!thisPtr.httpOutput.streamed && thisPtr.httpOutput.IsChannelBindingSupportEnabled)
                    {
                        thisPtr.buffer = thisPtr.httpOutput.SerializeBufferedMessage(thisPtr.httpOutput.message);
                        thisPtr.httpOutput.SetContentLength(thisPtr.buffer.Count);
                    }

                    if (thisPtr.WriteMessage(false))
                    {
                        thisPtr.httpOutput.TraceSend();
                        completeSelf = true;
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
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWriteStreamedMessageLater(object state)
            {
                SendAsyncResult thisPtr = (SendAsyncResult)state;

                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.WriteStreamedMessage();
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
                    if (completionException != null)
                    {
                        thisPtr.httpOutput.TraceSend();
                    }
                    thisPtr.Complete(false, completionException);
                }
            }

            static void OnWriteBody(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                SendAsyncResult thisPtr = (SendAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.CompleteWriteBody(result);
                    thisPtr.httpOutput.TraceSend();
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
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

        internal static HttpOutput CreateHttpOutput(HttpWebRequest httpWebRequest, IHttpTransportFactorySettings settings, Message message, bool enableChannelBindingSupport)
        {
            return new WebRequestHttpOutput(httpWebRequest, settings, message, enableChannelBindingSupport);
        }

        internal static HttpOutput CreateHttpOutput(HttpListenerResponse httpListenerResponse, IHttpTransportFactorySettings settings, Message message, string httpMethod)
        {
            return new ListenerResponseHttpOutput(httpListenerResponse, settings, message, httpMethod);
        }

        class WebRequestHttpOutput : HttpOutput
        {
            HttpWebRequest httpWebRequest;
            ChannelBinding channelBindingToken;
            bool enableChannelBindingSupport;

            public WebRequestHttpOutput(HttpWebRequest httpWebRequest, IHttpTransportFactorySettings settings, Message message, bool enableChannelBindingSupport)
                : base(settings, message, true, false)
            {
                this.httpWebRequest = httpWebRequest;
                this.enableChannelBindingSupport = enableChannelBindingSupport;
            }

            public override void Abort(HttpAbortReason abortReason)
            {
                httpWebRequest.Abort();
                base.Abort(abortReason);
            }

            protected override void AddMimeVersion(string version)
            {
                httpWebRequest.Headers[HttpChannelUtilities.MIMEVersionHeader] = version;
            }

            protected override void AddHeader(string name, string value)
            {
                httpWebRequest.Headers.Add(name, value);
            }

            protected override void SetContentType(string contentType)
            {
                httpWebRequest.ContentType = contentType;
            }

            protected override void SetContentEncoding(string contentEncoding)
            {
                this.httpWebRequest.Headers.Add(HttpChannelUtilities.ContentEncodingHeader, contentEncoding);
            }

            protected override void SetContentLength(int contentLength)
            {
                if (contentLength == 0 // work around whidbey issue with setting ContentLength - (see MB36881)
                    && !this.enableChannelBindingSupport) //When ChannelBinding is enabled, content length isn't supported
                {
                    httpWebRequest.ContentLength = contentLength;
                }
            }

            protected override void SetStatusCode(HttpStatusCode statusCode)
            {
            }

            protected override void SetStatusDescription(string statusDescription)
            {
            }

            protected override bool WillGetOutputStreamCompleteSynchronously
            {
                get { return false; }
            }

            protected override bool IsChannelBindingSupportEnabled
            {
                get
                {
                    return this.enableChannelBindingSupport;
                }
            }

            protected override ChannelBinding ChannelBinding
            {
                get
                {
                    return this.channelBindingToken;
                }
            }

            protected override bool CleanupChannelBinding
            {
                get
                {
                    //client side channel binding token will be attached to the inbound response message also, so
                    //we need to not clean up the CBT object for this HttpOutput object.
                    return false;
                }
            }

            //Used to allow the channel binding object to be transferred to the 
            //WebResponseHttpInput object.
            public override ChannelBinding TakeChannelBinding()
            {
                ChannelBinding result = this.channelBindingToken;
                this.channelBindingToken = null;
                return result;
            }

            protected override Stream GetOutputStream()
            {
                try
                {
                    Stream outputStream;
                    if (this.IsChannelBindingSupportEnabled)
                    {
                        TransportContext context;
                        outputStream = httpWebRequest.GetRequestStream(out context);
                        this.channelBindingToken = ChannelBindingUtility.GetToken(context);
                    }
                    else
                    {
                        outputStream = httpWebRequest.GetRequestStream();
                    }

                    outputStream = new WebRequestOutputStream(outputStream, httpWebRequest, this);

                    return outputStream;
                }
                catch (WebException webException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, abortReason));
                }
            }

            protected override IAsyncResult BeginGetOutputStream(AsyncCallback callback, object state)
            {
                return new GetOutputStreamAsyncResult(httpWebRequest, this, callback, state);
            }

            protected override Stream EndGetOutputStream(IAsyncResult result)
            {
                return GetOutputStreamAsyncResult.End(result, out this.channelBindingToken);
            }

            protected override bool PrepareHttpSend(Message message)
            {
                bool wasContentTypeSet = false;

                string action = message.Headers.Action;

                if (action != null)
                {
                    //This code is calling UrlPathEncode due to MessageBus bug 53362.
                    //After reviewing this decision, we
                    //feel that this was probably the wrong thing to do because UrlPathEncode
                    //doesn't escape some characters like '+', '%', etc.  The real issue behind 
                    //bug 53362 may have been as simple as being encoded multiple times on the client
                    //but being decoded one time on the server.  Calling UrlEncode would correctly
                    //escape these characters, but since we don't want to break any customers and no
                    //customers have complained, we will leave this as is for now...
                    action = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", UrlUtility.UrlPathEncode(action));
                }

                bool suppressEntityBody = base.PrepareHttpSend(message);

                object property;
                if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out property))
                {
                    HttpRequestMessageProperty requestProperty = (HttpRequestMessageProperty)property;
                    httpWebRequest.Method = requestProperty.Method;
                    // Query string was applied in HttpChannelFactory.ApplyManualAddressing
                    WebHeaderCollection requestHeaders = requestProperty.Headers;
                    suppressEntityBody = suppressEntityBody || requestProperty.SuppressEntityBody;
                    for (int i = 0; i < requestHeaders.Count; i++)
                    {
                        string name = requestHeaders.Keys[i];
                        string value = requestHeaders[i];
                        if (string.Compare(name, "accept", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            httpWebRequest.Accept = value;
                        }
                        else if (string.Compare(name, "connection", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (value.IndexOf("keep-alive", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                httpWebRequest.KeepAlive = true;
                            }
                            else
                            {
                                httpWebRequest.Connection = value;
                            }
                        }
                        else if (string.Compare(name, "SOAPAction", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (action == null)
                            {
                                action = value;
                            }
                            else
                            {
                                if (value.Length > 0 && string.Compare(value, action, StringComparison.Ordinal) != 0)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        new ProtocolException(SR.GetString(SR.HttpSoapActionMismatch, action, value)));
                                }
                            }
                        }
                        else if (string.Compare(name, "content-length", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // this will be taken care of by System.Net when we write to the content
                        }
                        else if (string.Compare(name, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            httpWebRequest.ContentType = value;
                            wasContentTypeSet = true;
                        }
                        else if (string.Compare(name, "expect", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (value.ToUpperInvariant().IndexOf("100-CONTINUE", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                httpWebRequest.ServicePoint.Expect100Continue = true;
                            }
                            else
                            {
                                httpWebRequest.Expect = value;
                            }
                        }
                        else if (string.Compare(name, "host", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // this should be controlled through Via
                        }
                        else if (string.Compare(name, "referer", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // referrer is proper spelling, but referer is the what is in the protocol.
                            httpWebRequest.Referer = value;
                        }
                        else if (string.Compare(name, "transfer-encoding", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (value.ToUpperInvariant().IndexOf("CHUNKED", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                httpWebRequest.SendChunked = true;
                            }
                            else
                            {
                                httpWebRequest.TransferEncoding = value;
                            }
                        }
                        else if (string.Compare(name, "user-agent", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            httpWebRequest.UserAgent = value;
                        }
                        else if (string.Compare(name, "if-modified-since", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            DateTime modifiedSinceDate;
                            if (DateTime.TryParse(value, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out modifiedSinceDate))
                            {
                                httpWebRequest.IfModifiedSince = modifiedSinceDate;
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    new ProtocolException(SR.GetString(SR.HttpIfModifiedSinceParseError, value)));
                            }
                        }
                        else if (string.Compare(name, "date", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // this will be taken care of by System.Net when we make the request
                        }
                        else if (string.Compare(name, "proxy-connection", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // set by System.Net if using a proxy.
                        }
                        else if (string.Compare(name, "range", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            // we don't support ranges in v1.
                        }
                        else
                        {
                            httpWebRequest.Headers.Add(name, value);
                        }
                    }
                }

                if (action != null)
                {
                    if (message.Version.Envelope == EnvelopeVersion.Soap11)
                    {
                        httpWebRequest.Headers["SOAPAction"] = action;
                    }
                    else if (message.Version.Envelope == EnvelopeVersion.Soap12)
                    {
                        if (message.Version.Addressing == AddressingVersion.None)
                        {
                            bool shouldSetContentType = true;
                            if (wasContentTypeSet)
                            {
                                if (httpWebRequest.ContentType.Contains("action")
                                    || httpWebRequest.ContentType.ToUpperInvariant().IndexOf("ACTION", StringComparison.OrdinalIgnoreCase) != -1)
                                {
                                    try
                                    {
                                        ContentType parsedContentType = new ContentType(httpWebRequest.ContentType);
                                        if (parsedContentType.Parameters.ContainsKey("action"))
                                        {
                                            string value = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", parsedContentType.Parameters["action"]);
                                            if (string.Compare(value, action, StringComparison.Ordinal) != 0)
                                            {
                                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                                    new ProtocolException(SR.GetString(SR.HttpSoapActionMismatchContentType, action, value)));
                                            }
                                            shouldSetContentType = false;
                                        }
                                    }
                                    catch (FormatException formatException)
                                    {
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                            new ProtocolException(SR.GetString(SR.HttpContentTypeFormatException, formatException.Message, httpWebRequest.ContentType), formatException));
                                    }
                                }
                            }

                            if (shouldSetContentType)
                            {
                                httpWebRequest.ContentType = string.Format(CultureInfo.InvariantCulture, "{0}; action={1}", httpWebRequest.ContentType, action);
                            }
                        }
                    }
                    else if (message.Version.Envelope != EnvelopeVersion.None)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.EnvelopeVersionUnknown,
                            message.Version.Envelope.ToString())));
                    }
                }

                // since we don't get the output stream in send when retVal == true, 
                // we need to disable chunking for some verbs (DELETE/PUT)
                if (suppressEntityBody)
                {
                    httpWebRequest.SendChunked = false;
                }
                else if (this.IsChannelBindingSupportEnabled)
                {
                    //force chunked upload since the length of the message is unknown before encoding.
                    httpWebRequest.SendChunked = true;
                }

                return suppressEntityBody;
            }

            protected override void PrepareHttpSendCore(HttpResponseMessage message)
            {
                // HTTP pipeline for client side is not implemented yet
                // DCR CSDMain 216853 is tracking this
                Fx.Assert(false, "HTTP pipeline for client is not implemented yet. This method should not be called.");
            }

            class GetOutputStreamAsyncResult : AsyncResult
            {
                static AsyncCallback onGetRequestStream = Fx.ThunkCallback(new AsyncCallback(OnGetRequestStream));
                HttpOutput httpOutput;
                HttpWebRequest httpWebRequest;
                Stream outputStream;
                ChannelBinding channelBindingToken;

                public GetOutputStreamAsyncResult(HttpWebRequest httpWebRequest, HttpOutput httpOutput, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.httpWebRequest = httpWebRequest;
                    this.httpOutput = httpOutput;

                    IAsyncResult result = null;
                    try
                    {
                        result = httpWebRequest.BeginGetRequestStream(onGetRequestStream, this);
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, httpOutput.abortReason));
                    }

                    if (result.CompletedSynchronously)
                    {
                        CompleteGetRequestStream(result);
                        base.Complete(true);
                    }
                }

                void CompleteGetRequestStream(IAsyncResult result)
                {
                    try
                    {
                        TransportContext context;
                        this.outputStream = new WebRequestOutputStream(httpWebRequest.EndGetRequestStream(result, out context), httpWebRequest, this.httpOutput);
                        this.channelBindingToken = ChannelBindingUtility.GetToken(context);
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, httpOutput.abortReason));
                    }
                }

                public static Stream End(IAsyncResult result, out ChannelBinding channelBindingToken)
                {
                    GetOutputStreamAsyncResult thisPtr = AsyncResult.End<GetOutputStreamAsyncResult>(result);
                    channelBindingToken = thisPtr.channelBindingToken;
                    return thisPtr.outputStream;
                }

                static void OnGetRequestStream(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    GetOutputStreamAsyncResult thisPtr = (GetOutputStreamAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.CompleteGetRequestStream(result);
                    }
#pragma warning suppress 56500 // [....], transferring exception to another thread
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

            class WebRequestOutputStream : BytesReadPositionStream
            {
                HttpWebRequest httpWebRequest;
                HttpOutput httpOutput;
                int bytesSent = 0;

                public WebRequestOutputStream(Stream requestStream, HttpWebRequest httpWebRequest, HttpOutput httpOutput)
                    : base(requestStream)
                {
                    this.httpWebRequest = httpWebRequest;
                    this.httpOutput = httpOutput;
                }

                public override void Close()
                {
                    try
                    {
                        base.Close();
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(objectDisposedException, httpWebRequest, httpOutput.abortReason));
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(ioException, httpWebRequest));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, httpOutput.abortReason));
                    }
                }

                public override long Position
                {
                    get
                    {
                        return bytesSent;
                    }
                    set
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
                    }
                }

                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    this.bytesSent += count;
                    try
                    {
                        return base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(objectDisposedException, httpWebRequest, httpOutput.abortReason));
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(ioException, httpWebRequest));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, httpOutput.abortReason));
                    }
                }

                public override void EndWrite(IAsyncResult result)
                {
                    try
                    {
                        base.EndWrite(result);
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(objectDisposedException, httpWebRequest, httpOutput.abortReason));
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(ioException, httpWebRequest));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, httpOutput.abortReason));
                    }
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        base.Write(buffer, offset, count);
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestCanceledException(objectDisposedException, httpWebRequest, httpOutput.abortReason));
                    }
                    catch (IOException ioException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestIOException(ioException, httpWebRequest));
                    }
                    catch (WebException webException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(HttpChannelUtilities.CreateRequestWebException(webException, httpWebRequest, httpOutput.abortReason));
                    }
                    this.bytesSent += count;
                }
            }
        }

        class ListenerResponseHttpOutput : HttpOutput
        {
            HttpListenerResponse listenerResponse;
            string httpMethod;

            [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.Usage, "CA2214", Justification = "No one else is inhiriting from this class.")]
            public ListenerResponseHttpOutput(HttpListenerResponse listenerResponse, IHttpTransportFactorySettings settings, Message message, string httpMethod)
                : base(settings, message, false, true)
            {
                this.listenerResponse = listenerResponse;
                this.httpMethod = httpMethod;

                if (message.IsFault)
                {
                    this.SetStatusCode(HttpStatusCode.InternalServerError);
                }
                else
                {
                    this.SetStatusCode(HttpStatusCode.OK);
                }
            }

            protected override string HttpMethod
            {
                get { return this.httpMethod; }
            }

            public override void Abort(HttpAbortReason abortReason)
            {
                listenerResponse.Abort();
                base.Abort(abortReason);
            }

            protected override void AddMimeVersion(string version)
            {
                listenerResponse.Headers[HttpChannelUtilities.MIMEVersionHeader] = version;
            }

            protected override bool PrepareHttpSend(Message message)
            {
                bool result = base.PrepareHttpSend(message);

                if (this.CanSendCompressedResponses)
                {
                    string contentType = this.listenerResponse.ContentType;
                    string contentEncoding;
                    if (HttpChannelUtilities.GetHttpResponseTypeAndEncodingForCompression(ref contentType, out contentEncoding))
                    {
                        if (contentType != this.listenerResponse.ContentType)
                        {
                            this.SetContentType(contentType);
                        }
                        this.SetContentEncoding(contentEncoding);
                    }
                }

                HttpResponseMessageProperty responseProperty = message.Properties.GetValue<HttpResponseMessageProperty>(HttpResponseMessageProperty.Name, true);
                bool httpResponseMessagePropertyFound = responseProperty != null;
                bool httpMethodIsHead = string.Compare(this.httpMethod, "HEAD", StringComparison.OrdinalIgnoreCase) == 0;

                if (httpMethodIsHead ||
                    httpResponseMessagePropertyFound && responseProperty.SuppressEntityBody)
                {
                    result = true;
                    this.SetContentLength(0);
                    this.SetContentType(null);
                    listenerResponse.SendChunked = false;
                }

                if (httpResponseMessagePropertyFound)
                {
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
                        if (string.Compare(name, "content-length", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            int contentLength = -1;
                            if (httpMethodIsHead &&
                                int.TryParse(value, out contentLength))
                            {
                                this.SetContentLength(contentLength);
                            }
                            // else
                            //this will be taken care of by System.Net when we write to the content
                        }
                        else if (string.Compare(name, "content-type", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            if (httpMethodIsHead ||
                                !responseProperty.SuppressEntityBody)
                            {
                                this.SetContentType(value);
                            }
                        }
                        else
                        {
                            this.AddHeader(name, value);
                        }
                    }
                }

                return result;
            }

            protected override void PrepareHttpSendCore(HttpResponseMessage message)
            {
                this.listenerResponse.StatusCode = (int)message.StatusCode;
                if (message.ReasonPhrase != null)
                {
                    this.listenerResponse.StatusDescription = message.ReasonPhrase;
                }
                HttpChannelUtilities.CopyHeaders(message, AddHeader);
            }

            protected override void AddHeader(string name, string value)
            {
                if (string.Compare(name, "WWW-Authenticate", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    listenerResponse.AddHeader(name, value);
                }
                else
                {
                    listenerResponse.AppendHeader(name, value);
                }
            }

            protected override void SetContentType(string contentType)
            {
                listenerResponse.ContentType = contentType;
            }

            protected override void SetContentEncoding(string contentEncoding)
            {
                this.listenerResponse.AddHeader(HttpChannelUtilities.ContentEncodingHeader, contentEncoding);
            }

            protected override void SetContentLength(int contentLength)
            {
                listenerResponse.ContentLength64 = contentLength;
            }

            protected override void SetStatusCode(HttpStatusCode statusCode)
            {
                listenerResponse.StatusCode = (int)statusCode;
            }

            protected override void SetStatusDescription(string statusDescription)
            {
                listenerResponse.StatusDescription = statusDescription;
            }

            protected override Stream GetOutputStream()
            {
                return new ListenerResponseOutputStream(listenerResponse);
            }

            class ListenerResponseOutputStream : BytesReadPositionStream
            {
                public ListenerResponseOutputStream(HttpListenerResponse listenerResponse)
                    : base(listenerResponse.OutputStream)
                {
                }

                public override void Close()
                {
                    try
                    {
                        base.Close();
                    }
                    catch (HttpListenerException listenerException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            HttpChannelUtilities.CreateCommunicationException(listenerException));
                    }
                }

                public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
                {
                    try
                    {
                        return base.BeginWrite(buffer, offset, count, callback, state);
                    }
                    catch (HttpListenerException listenerException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            HttpChannelUtilities.CreateCommunicationException(listenerException));
                    }
                    catch (ApplicationException applicationException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new CommunicationObjectAbortedException(SR.GetString(SR.HttpResponseAborted),
                            applicationException));
                    }
                }

                public override void EndWrite(IAsyncResult result)
                {
                    try
                    {
                        base.EndWrite(result);
                    }
                    catch (HttpListenerException listenerException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            HttpChannelUtilities.CreateCommunicationException(listenerException));
                    }
                    catch (ApplicationException applicationException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new CommunicationObjectAbortedException(SR.GetString(SR.HttpResponseAborted),
                            applicationException));
                    }
                }

                public override void Write(byte[] buffer, int offset, int count)
                {
                    try
                    {
                        base.Write(buffer, offset, count);
                    }
                    catch (HttpListenerException listenerException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            HttpChannelUtilities.CreateCommunicationException(listenerException));
                    }
                    catch (ApplicationException applicationException)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new CommunicationObjectAbortedException(SR.GetString(SR.HttpResponseAborted),
                            applicationException));
                    }
                }
            }
        }
    }

    enum HttpAbortReason
    {
        None,
        Aborted,
        TimedOut
    }

    delegate void AddHeaderDelegate(string headerName, string headerValue);

    static class HttpChannelUtilities
    {
        internal static class StatusDescriptionStrings
        {
            internal const string HttpContentTypeMissing = "Missing Content Type";
            internal const string HttpContentTypeMismatch = "Cannot process the message because the content type '{0}' was not the expected type '{1}'.";
            internal const string HttpStatusServiceActivationException = "System.ServiceModel.ServiceActivationException";
        }

        internal static class ObsoleteDescriptionStrings
        {
            internal const string PropertyObsoleteUseAllowCookies = "This property is obsolete. To enable Http CookieContainer, use the AllowCookies property instead.";
            internal const string TypeObsoleteUseAllowCookies = "This type is obsolete. To enable the Http CookieContainer, use the AllowCookies property on the http binding or on the HttpTransportBindingElement.";
        }

        internal const string HttpStatusCodeKey = "HttpStatusCode";
        internal const string HttpStatusCodeExceptionKey = "System.ServiceModel.Channels.HttpInput.HttpStatusCode";
        internal const string HttpStatusDescriptionExceptionKey = "System.ServiceModel.Channels.HttpInput.HttpStatusDescription";

        internal const int ResponseStreamExcerptSize = 1024;

        internal const string MIMEVersionHeader = "MIME-Version";

        internal const string ContentEncodingHeader = "Content-Encoding";
        internal const string AcceptEncodingHeader = "Accept-Encoding";

        private const string ContentLengthHeader = "Content-Length";
        private static readonly HashSet<string> httpContentHeaders = new HashSet<string>()
            {
                "Allow", "Content-Encoding", "Content-Language", "Content-Location", "Content-MD5",
                "Content-Range", "Expires", "Last-Modified", "Content-Type", ContentLengthHeader
            };

        static bool allReferencedAssembliesLoaded = false;

        public static Exception CreateCommunicationException(HttpListenerException listenerException)
        {
            switch (listenerException.NativeErrorCode)
            {
                case UnsafeNativeMethods.ERROR_NO_TRACKING_SERVICE:
                    return new CommunicationException(SR.GetString(SR.HttpNoTrackingService, listenerException.Message), listenerException);

                case UnsafeNativeMethods.ERROR_NETNAME_DELETED:
                    return new CommunicationException(SR.GetString(SR.HttpNetnameDeleted, listenerException.Message), listenerException);

                case UnsafeNativeMethods.ERROR_INVALID_HANDLE:
                    return new CommunicationObjectAbortedException(SR.GetString(SR.HttpResponseAborted), listenerException);

                case UnsafeNativeMethods.ERROR_NOT_ENOUGH_MEMORY:
                case UnsafeNativeMethods.ERROR_OUTOFMEMORY:
                case UnsafeNativeMethods.ERROR_NO_SYSTEM_RESOURCES:
                    return new InsufficientMemoryException(SR.GetString(SR.InsufficentMemory), listenerException);

                default:
                    return new CommunicationException(listenerException.Message, listenerException);
            }
        }

        public static void EnsureHttpRequestMessageContentNotNull(HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage.Content == null)
            {
                httpRequestMessage.Content = new ByteArrayContent(EmptyArray<byte>.Instance);
            }
        }

        public static void EnsureHttpResponseMessageContentNotNull(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.Content == null)
            {
                httpResponseMessage.Content = new ByteArrayContent(EmptyArray<byte>.Instance);
            }
        }

        public static bool IsEmpty(HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.Content == null
               || (httpResponseMessage.Content.Headers.ContentLength.HasValue && httpResponseMessage.Content.Headers.ContentLength.Value == 0);
        }

        internal static void HandleContinueWithTask(Task task)
        {
            HandleContinueWithTask(task, null);
        }

        internal static void HandleContinueWithTask(Task task, Action<Exception> exceptionHandler)
        {
            if (task.IsFaulted)
            {
                if (exceptionHandler == null)
                {
                    throw FxTrace.Exception.AsError<FaultException>(task.Exception);
                }
                else
                {
                    exceptionHandler.Invoke(task.Exception);
                }
            }
            else if (task.IsCanceled)
            {
                throw FxTrace.Exception.AsError(new TimeoutException(SR.GetString(SR.TaskCancelledError)));
            }
        }

        public static void AbortRequest(HttpWebRequest request)
        {
            request.Abort();
        }

        public static void SetRequestTimeout(HttpWebRequest request, TimeSpan timeout)
        {
            int millisecondsTimeout = TimeoutHelper.ToMilliseconds(timeout);
            if (millisecondsTimeout == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(SR.GetString(
                    SR.HttpRequestTimedOut, request.RequestUri, timeout)));
            }
            request.Timeout = millisecondsTimeout;
            request.ReadWriteTimeout = millisecondsTimeout;
        }

        public static void AddReplySecurityProperty(HttpChannelFactory<IRequestChannel> factory, HttpWebRequest webRequest,
            HttpWebResponse webResponse, Message replyMessage)
        {
            SecurityMessageProperty securityProperty = factory.CreateReplySecurityProperty(webRequest, webResponse);
            if (securityProperty != null)
            {
                replyMessage.Properties.Security = securityProperty;
            }
        }

        public static void CopyHeaders(HttpRequestMessage request, AddHeaderDelegate addHeader)
        {
            HttpChannelUtilities.CopyHeaders(request.Headers, addHeader);
            if (request.Content != null)
            {
                HttpChannelUtilities.CopyHeaders(request.Content.Headers, addHeader);
            }
        }

        public static void CopyHeaders(HttpResponseMessage response, AddHeaderDelegate addHeader)
        {
            HttpChannelUtilities.CopyHeaders(response.Headers, addHeader);
            if (response.Content != null)
            {
                HttpChannelUtilities.CopyHeaders(response.Content.Headers, addHeader);
            }
        }

        static void CopyHeaders(HttpHeaders headers, AddHeaderDelegate addHeader)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                foreach (string value in header.Value)
                {
                    TryAddToCollection(addHeader, header.Key, value);                    
                }
            }
        }

        public static void CopyHeaders(NameValueCollection headers, AddHeaderDelegate addHeader)
        {
            //this nested loop logic was copied from NameValueCollection.Add(NameValueCollection)
            int count = headers.Count;
            for (int i = 0; i < count; i++)
            {
                string key = headers.GetKey(i);

                string[] values = headers.GetValues(i);
                if (values != null)
                {
                    for (int j = 0; j < values.Length; j++)
                    {
                        TryAddToCollection(addHeader, key, values[j]);
                    }
                }
                else
                {
                    addHeader(key, null);
                }
            }
        }

        public static void CopyHeadersToNameValueCollection(NameValueCollection headers, NameValueCollection destination)
        {
            CopyHeaders(headers, destination.Add); 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                            Justification = "The exceptions are traced already.")]
        static void TryAddToCollection(AddHeaderDelegate addHeader, string headerName, string value)
        {
            try
            {
                addHeader(headerName, value);
            }
            catch (ArgumentException ex)
            {
                string encodedValue = null;
                if (TryEncodeHeaderValueAsUri(headerName, value, out encodedValue))
                {
                    //note: if the hosthame of a referer header contains illegal chars, we will still throw from here
                    //because Uri will not fix this up for us, which is ok. The request will get rejected in the error code path.
                    addHeader(headerName, encodedValue);
                }
                else
                {
                    // In self-hosted scenarios, some of the headers like Content-Length cannot be added directly.
                    // It will throw ArgumentException instead.
                    FxTrace.Exception.AsInformation(ex);
                }
            }
        }

        static bool TryEncodeHeaderValueAsUri(string headerName, string value, out string result)
        {
            result = null;
            //Internet Explorer will send the referrer header on the wire in unicode without encoding it
            //this will cause errors when added to a WebHeaderCollection.  This is a workaround for sharepoint, 
            //but will only work for WebHosted Scenarios.
            if (String.Compare(headerName, "Referer", StringComparison.OrdinalIgnoreCase) == 0)
            {
                Uri uri;
                if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (uri.IsAbsoluteUri)
                    {
                        result = uri.AbsoluteUri;
                    }
                    else
                    {
                        result = uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
                    }
                    return true;
                }                
            }
            return false;
        }

        //
        internal static Type GetTypeFromAssembliesInCurrentDomain(string typeString)
        {
            Type type = Type.GetType(typeString, false);
            if (null == type)
            {
                if (!allReferencedAssembliesLoaded)
                {
                    allReferencedAssembliesLoaded = true;
                    AspNetEnvironment.Current.EnsureAllReferencedAssemblyLoaded();
                }

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    type = assemblies[i].GetType(typeString, false);
                    if (null != type)
                    {
                        break;
                    }
                }
            }

            return type;
        }

        public static NetworkCredential GetCredential(AuthenticationSchemes authenticationScheme,
            SecurityTokenProviderContainer credentialProvider, TimeSpan timeout,
            out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
        {
            impersonationLevel = TokenImpersonationLevel.None;
            authenticationLevel = AuthenticationLevel.None;

            NetworkCredential result = null;

            if (authenticationScheme != AuthenticationSchemes.Anonymous)
            {
                result = GetCredentialCore(authenticationScheme, credentialProvider, timeout, out impersonationLevel, out authenticationLevel);
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static NetworkCredential GetCredentialCore(AuthenticationSchemes authenticationScheme,
            SecurityTokenProviderContainer credentialProvider, TimeSpan timeout,
            out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
        {
            impersonationLevel = TokenImpersonationLevel.None;
            authenticationLevel = AuthenticationLevel.None;

            NetworkCredential result = null;

            switch (authenticationScheme)
            {
                case AuthenticationSchemes.Basic:
                    result = TransportSecurityHelpers.GetUserNameCredential(credentialProvider, timeout);
                    impersonationLevel = TokenImpersonationLevel.Delegation;
                    break;

                case AuthenticationSchemes.Digest:
                    result = TransportSecurityHelpers.GetSspiCredential(credentialProvider, timeout,
                        out impersonationLevel, out authenticationLevel);

                    HttpChannelUtilities.ValidateDigestCredential(ref result, impersonationLevel);
                    break;

                case AuthenticationSchemes.Negotiate:
                    result = TransportSecurityHelpers.GetSspiCredential(credentialProvider, timeout,
                        out impersonationLevel, out authenticationLevel);
                    break;

                case AuthenticationSchemes.Ntlm:
                    result = TransportSecurityHelpers.GetSspiCredential(credentialProvider, timeout,
                        out impersonationLevel, out authenticationLevel);
                    if (authenticationLevel == AuthenticationLevel.MutualAuthRequired)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.CredentialDisallowsNtlm)));
                    }
                    break;

                default:
                    // The setter for this property should prevent this.
                    throw Fx.AssertAndThrow("GetCredential: Invalid authentication scheme");
            }

            return result;
        }


        public static HttpWebResponse ProcessGetResponseWebException(WebException webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            HttpWebResponse response = null;

            if (webException.Status == WebExceptionStatus.Success ||
                webException.Status == WebExceptionStatus.ProtocolError)
            {
                response = (HttpWebResponse)webException.Response;
            }

            if (response == null)
            {
                Exception convertedException = ConvertWebException(webException, request, abortReason);

                if (convertedException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(convertedException);
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(webException.Message,
                    webException));
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, request.RequestUri.AbsoluteUri), webException));
            }

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ServerTooBusyException(SR.GetString(SR.HttpServerTooBusy, request.RequestUri.AbsoluteUri), webException));
            }

            if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                string statusDescription = response.StatusDescription;
                if (!string.IsNullOrEmpty(statusDescription))
                {
                    if (string.Compare(statusDescription, HttpChannelUtilities.StatusDescriptionStrings.HttpContentTypeMissing, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.MissingContentType, request.RequestUri), webException));
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.FramingContentTypeMismatch, request.ContentType, request.RequestUri), webException));
            }

            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(webException.Message, webException));
            }

            // if http.sys has a request queue on the TCP port, then if the path fails to match it will send
            // back "<h1>Bad Request (Invalid Hostname)</h1>" in the body of a 400 response.
            // See code at \\index1\sddnsrv\net\http\sys\httprcv.c for details
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                const string httpSysRequestQueueNotFound = "<h1>Bad Request (Invalid Hostname)</h1>";
                const string httpSysRequestQueueNotFoundVista = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\"\"http://www.w3.org/TR/html4/strict.dtd\">\r\n<HTML><HEAD><TITLE>Bad Request</TITLE>\r\n<META HTTP-EQUIV=\"Content-Type\" Content=\"text/html; charset=us-ascii\"></HEAD>\r\n<BODY><h2>Bad Request - Invalid Hostname</h2>\r\n<hr><p>HTTP Error 400. The request hostname is invalid.</p>\r\n</BODY></HTML>\r\n";
                string notFoundTestString = null;

                if (response.ContentLength == httpSysRequestQueueNotFound.Length)
                {
                    notFoundTestString = httpSysRequestQueueNotFound;
                }
                else if (response.ContentLength == httpSysRequestQueueNotFoundVista.Length)
                {
                    notFoundTestString = httpSysRequestQueueNotFoundVista;
                }

                if (notFoundTestString != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    byte[] responseBytes = new byte[notFoundTestString.Length];
                    int bytesRead = responseStream.Read(responseBytes, 0, responseBytes.Length);

                    // since the response is buffered by System.Net (it's an error response), we should have read
                    // the amount we were expecting
                    if (bytesRead == notFoundTestString.Length
                        && notFoundTestString == UTF8Encoding.ASCII.GetString(responseBytes))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, request.RequestUri.AbsoluteUri), webException));
                    }
                }
            }

            return response;
        }

        public static Exception ConvertWebException(WebException webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            switch (webException.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                    return new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, request.RequestUri.AbsoluteUri), webException);
                case WebExceptionStatus.SecureChannelFailure:
                    return new SecurityNegotiationException(SR.GetString(SR.SecureChannelFailure, request.RequestUri.Authority), webException);
                case WebExceptionStatus.TrustFailure:
                    return new SecurityNegotiationException(SR.GetString(SR.TrustFailure, request.RequestUri.Authority), webException);
                case WebExceptionStatus.Timeout:
                    return new TimeoutException(CreateRequestTimedOutMessage(request), webException);
                case WebExceptionStatus.ReceiveFailure:
                    return new CommunicationException(SR.GetString(SR.HttpReceiveFailure, request.RequestUri), webException);
                case WebExceptionStatus.SendFailure:
                    return new CommunicationException(SR.GetString(SR.HttpSendFailure, request.RequestUri), webException);
                case WebExceptionStatus.RequestCanceled:
                    return CreateRequestCanceledException(webException, request, abortReason);
                case WebExceptionStatus.ProtocolError:
                    HttpWebResponse response = (HttpWebResponse)webException.Response;
                    Fx.Assert(response != null, "'response' MUST NOT be NULL for WebExceptionStatus=='ProtocolError'.");
                    if (response.StatusCode == HttpStatusCode.InternalServerError &&
                        string.Compare(response.StatusDescription, HttpChannelUtilities.StatusDescriptionStrings.HttpStatusServiceActivationException, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return new ServiceActivationException(SR.GetString(SR.Hosting_ServiceActivationFailed, request.RequestUri));
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        public static Exception CreateResponseIOException(IOException ioException, TimeSpan receiveTimeout)
        {
            if (ioException.InnerException is SocketException)
            {
                return SocketConnection.ConvertTransferException((SocketException)ioException.InnerException, receiveTimeout, ioException);
            }

            return new CommunicationException(SR.GetString(SR.HttpTransferError, ioException.Message), ioException);
        }

        public static Exception CreateResponseWebException(WebException webException, HttpWebResponse response)
        {
            switch (webException.Status)
            {
                case WebExceptionStatus.RequestCanceled:
                    return TraceResponseException(new CommunicationObjectAbortedException(SR.GetString(SR.HttpRequestAborted, response.ResponseUri), webException));
                case WebExceptionStatus.ConnectionClosed:
                    return TraceResponseException(new CommunicationException(webException.Message, webException));
                case WebExceptionStatus.Timeout:
                    return TraceResponseException(new TimeoutException(SR.GetString(SR.HttpResponseTimedOut, response.ResponseUri,
                        TimeSpan.FromMilliseconds(response.GetResponseStream().ReadTimeout)), webException));
                default:
                    return CreateUnexpectedResponseException(webException, response);
            }
        }

        public static Exception CreateRequestCanceledException(Exception webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            switch (abortReason)
            {
                case HttpAbortReason.Aborted:
                    return new CommunicationObjectAbortedException(SR.GetString(SR.HttpRequestAborted, request.RequestUri), webException);
                case HttpAbortReason.TimedOut:
                    return new TimeoutException(CreateRequestTimedOutMessage(request), webException);
                default:
                    return new CommunicationException(SR.GetString(SR.HttpTransferError, webException.Message), webException);
            }
        }

        public static Exception CreateRequestIOException(IOException ioException, HttpWebRequest request)
        {
            return CreateRequestIOException(ioException, request, null);
        }

        public static Exception CreateRequestIOException(IOException ioException, HttpWebRequest request, Exception originalException)
        {
            Exception exception = originalException == null ? ioException : originalException;

            if (ioException.InnerException is SocketException)
            {
                return SocketConnection.ConvertTransferException((SocketException)ioException.InnerException, TimeSpan.FromMilliseconds(request.Timeout), exception);
            }

            return new CommunicationException(SR.GetString(SR.HttpTransferError, exception.Message), exception);
        }

        static string CreateRequestTimedOutMessage(HttpWebRequest request)
        {
            return SR.GetString(SR.HttpRequestTimedOut, request.RequestUri, TimeSpan.FromMilliseconds(request.Timeout));
        }

        public static Exception CreateRequestWebException(WebException webException, HttpWebRequest request, HttpAbortReason abortReason)
        {
            Exception convertedException = ConvertWebException(webException, request, abortReason);

            if (webException.Response != null)
            {
                //free the connection for use by another request
                webException.Response.Close();
            }

            if (convertedException != null)
            {
                return convertedException;
            }

            if (webException.InnerException is IOException)
            {
                return CreateRequestIOException((IOException)webException.InnerException, request, webException);
            }

            if (webException.InnerException is SocketException)
            {
                return SocketConnectionInitiator.ConvertConnectException((SocketException)webException.InnerException, request.RequestUri, TimeSpan.MaxValue, webException);
            }

            return new EndpointNotFoundException(SR.GetString(SR.EndpointNotFound, request.RequestUri.AbsoluteUri), webException);
        }

        static Exception CreateUnexpectedResponseException(WebException responseException, HttpWebResponse response)
        {
            string statusDescription = response.StatusDescription;
            if (string.IsNullOrEmpty(statusDescription))
                statusDescription = response.StatusCode.ToString();

            return TraceResponseException(
                new ProtocolException(SR.GetString(SR.UnexpectedHttpResponseCode,
                (int)response.StatusCode, statusDescription), responseException));
        }

        public static Exception CreateNullReferenceResponseException(NullReferenceException nullReferenceException)
        {
            return TraceResponseException(
                new ProtocolException(SR.GetString(SR.NullReferenceOnHttpResponse), nullReferenceException));
        }

        static string GetResponseStreamString(HttpWebResponse webResponse, out int bytesRead)
        {
            Stream responseStream = webResponse.GetResponseStream();

            long bufferSize = webResponse.ContentLength;

            if (bufferSize < 0 || bufferSize > ResponseStreamExcerptSize)
            {
                bufferSize = ResponseStreamExcerptSize;
            }

            byte[] responseBuffer = DiagnosticUtility.Utility.AllocateByteArray(checked((int)bufferSize));
            bytesRead = responseStream.Read(responseBuffer, 0, (int)bufferSize);
            responseStream.Close();

            return System.Text.Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
        }

        static Exception TraceResponseException(Exception exception)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.HttpChannelUnexpectedResponse, SR.GetString(SR.TraceCodeHttpChannelUnexpectedResponse), (object)null, exception);
            }

            return exception;
        }

        static bool ValidateEmptyContent(HttpWebResponse response)
        {
            bool responseIsEmpty = true;

            if (response.ContentLength > 0)
            {
                responseIsEmpty = false;
            }
            else if (response.ContentLength == -1) // chunked 
            {
                Stream responseStream = response.GetResponseStream();
                byte[] testBuffer = new byte[1];
                responseIsEmpty = (responseStream.Read(testBuffer, 0, 1) != 1);
            }

            return responseIsEmpty;
        }

        static void ValidateAuthentication(HttpWebRequest request, HttpWebResponse response,
            WebException responseException, HttpChannelFactory<IRequestChannel> factory)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                string message = SR.GetString(SR.HttpAuthorizationFailed, factory.AuthenticationScheme,
                    response.Headers[HttpResponseHeader.WwwAuthenticate]);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    TraceResponseException(new MessageSecurityException(message, responseException)));
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                string message = SR.GetString(SR.HttpAuthorizationForbidden, factory.AuthenticationScheme);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    TraceResponseException(new MessageSecurityException(message, responseException)));
            }

            if ((request.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired) &&
                !response.IsMutuallyAuthenticated)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    TraceResponseException(new SecurityNegotiationException(SR.GetString(SR.HttpMutualAuthNotSatisfied),
                    responseException)));
            }
        }

        public static void ValidateDigestCredential(ref NetworkCredential credential, TokenImpersonationLevel impersonationLevel)
        {
            // this is a work-around to VSWhidbey#470545 (Since the service always uses Impersonation,
            // we mitigate EOP by preemtively not allowing Identification)
            if (!SecurityUtils.IsDefaultNetworkCredential(credential))
            {
                // With a non-default credential, Digest will not honor a client impersonation constraint of 
                // TokenImpersonationLevel.Identification.
                if (!TokenImpersonationLevelHelper.IsGreaterOrEqual(impersonationLevel,
                    TokenImpersonationLevel.Impersonation))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                        SR.DigestExplicitCredsImpersonationLevel, impersonationLevel)));
                }
            }
        }

        // only valid response codes are 500 (if it's a fault) or 200 (iff it's a response message)
        public static HttpInput ValidateRequestReplyResponse(HttpWebRequest request, HttpWebResponse response,
            HttpChannelFactory<IRequestChannel> factory, WebException responseException, ChannelBinding channelBinding)
        {
            ValidateAuthentication(request, response, responseException, factory);

            HttpInput httpInput = null;

            // We will close the HttpWebResponse if we got an error code betwen 200 and 300 and 
            // 1) an exception was thrown out or 
            // 2) it's an empty message and we are using SOAP.
            // For responses with status code above 300, System.Net will close the underlying connection so we don't need to worry about that.
            if ((200 <= (int)response.StatusCode && (int)response.StatusCode < 300) || response.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError
                    && string.Compare(response.StatusDescription, HttpChannelUtilities.StatusDescriptionStrings.HttpStatusServiceActivationException, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ServiceActivationException(SR.GetString(SR.Hosting_ServiceActivationFailed, request.RequestUri)));
                }
                else
                {
                    bool throwing = true;
                    try
                    {
                        if (string.IsNullOrEmpty(response.ContentType))
                        {
                            if (!ValidateEmptyContent(response))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(
                                    new ProtocolException(
                                        SR.GetString(SR.HttpContentTypeHeaderRequired),
                                        responseException)));
                            }
                        }
                        else if (response.ContentLength != 0)
                        {
                            MessageEncoder encoder = factory.MessageEncoderFactory.Encoder;
                            if (!encoder.IsContentTypeSupported(response.ContentType))
                            {
                                int bytesRead;
                                String responseExcerpt = GetResponseStreamString(response, out bytesRead);

                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TraceResponseException(
                                    new ProtocolException(
                                        SR.GetString(
                                            SR.ResponseContentTypeMismatch,
                                            response.ContentType,
                                            encoder.ContentType,
                                            bytesRead,
                                            responseExcerpt), responseException)));

                            }

                            httpInput = HttpInput.CreateHttpInput(response, factory, channelBinding);
                            httpInput.WebException = responseException;
                        }

                        throwing = false;
                    }
                    finally
                    {
                        if (throwing)
                        {
                            response.Close();
                        }
                    }
                }

                if (httpInput == null)
                {
                    if (factory.MessageEncoderFactory.MessageVersion == MessageVersion.None)
                    {
                        httpInput = HttpInput.CreateHttpInput(response, factory, channelBinding);
                        httpInput.WebException = responseException;
                    }
                    else
                    {
                        // In this case, we got a response with
                        // 1) status code between 200 and 300
                        // 2) Non-empty Content Type string
                        // 3) Zero content length
                        // Since we are trying to use SOAP here, the message seems to be malicious and we should
                        // just close the response directly.
                        response.Close();
                    }
                }
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateUnexpectedResponseException(responseException, response));
            }

            return httpInput;
        }

        public static bool GetHttpResponseTypeAndEncodingForCompression(ref string contentType, out string contentEncoding)
        {
            contentEncoding = null;
            bool isSession = false;
            bool isDeflate = false;

            if (string.Equals(BinaryVersion.GZipVersion1.ContentType, contentType, StringComparison.OrdinalIgnoreCase) ||
                (isSession = string.Equals(BinaryVersion.GZipVersion1.SessionContentType, contentType, StringComparison.OrdinalIgnoreCase)) ||
                (isDeflate = (string.Equals(BinaryVersion.DeflateVersion1.ContentType, contentType, StringComparison.OrdinalIgnoreCase) ||
                (isSession = string.Equals(BinaryVersion.DeflateVersion1.SessionContentType, contentType, StringComparison.OrdinalIgnoreCase)))))
            {
                contentType = isSession ? BinaryVersion.Version1.SessionContentType : BinaryVersion.Version1.ContentType;
                contentEncoding = isDeflate ? MessageEncoderCompressionHandler.DeflateContentEncoding : MessageEncoderCompressionHandler.GZipContentEncoding;
                return true;
            }
            return false;
        }
    }

    abstract class HttpDelayedAcceptStream : DetectEofStream
    {
        HttpOutput httpOutput;
        bool isHttpOutputClosed;

        /// <summary>
        /// Indicates whether the HttpOutput should be closed when this stream is closed. In the streamed case, 
        /// we�ll leave the HttpOutput opened (and it will be closed by the HttpRequestContext, so we won't leak it).
        /// </summary>
        bool closeHttpOutput;

        // sometimes we can't flush the HTTP output until we're done reading the end of the 
        // incoming stream of the HTTP input
        protected HttpDelayedAcceptStream(Stream stream)
            : base(stream)
        {
        }

        public bool EnableDelayedAccept(HttpOutput output, bool closeHttpOutput)
        {
            if (IsAtEof)
            {
                return false;
            }

            this.closeHttpOutput = closeHttpOutput;
            this.httpOutput = output;
            return true;
        }

        protected override void OnReceivedEof()
        {
            if (this.closeHttpOutput)
            {
                CloseHttpOutput();
            }
        }

        public override void Close()
        {
            if (this.closeHttpOutput)
            {
                CloseHttpOutput();
            }

            base.Close();
        }

        void CloseHttpOutput()
        {
            if (this.httpOutput != null && !this.isHttpOutputClosed)
            {
                this.httpOutput.Close();
                this.isHttpOutputClosed = true;
            }
        }
    }

    abstract class BytesReadPositionStream : DelegatingStream
    {
        int bytesSent = 0;

        protected BytesReadPositionStream(Stream stream)
            : base(stream)
        {
        }

        public override long Position
        {
            get
            {
                return bytesSent;
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupported)));
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.bytesSent += count;
            return BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            this.bytesSent += count;
        }

        public override void WriteByte(byte value)
        {
            BaseStream.WriteByte(value);
            this.bytesSent++;
        }
    }

    class PreReadStream : DelegatingStream
    {
        byte[] preReadBuffer;

        public PreReadStream(Stream stream, byte[] preReadBuffer)
            : base(stream)
        {
            this.preReadBuffer = preReadBuffer;
        }

        bool ReadFromBuffer(byte[] buffer, int offset, int count, out int bytesRead)
        {
            if (this.preReadBuffer != null)
            {
                if (buffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
                }

                if (offset >= buffer.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", offset,
                        SR.GetString(SR.OffsetExceedsBufferBound, buffer.Length - 1)));
                }

                if (count < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", count,
                        SR.GetString(SR.ValueMustBeNonNegative)));
                }

                if (count == 0)
                {
                    bytesRead = 0;
                }
                else
                {
                    buffer[offset] = this.preReadBuffer[0];
                    this.preReadBuffer = null;
                    bytesRead = 1;
                }

                return true;
            }

            bytesRead = -1;
            return false;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead;
            if (ReadFromBuffer(buffer, offset, count, out bytesRead))
            {
                return bytesRead;
            }

            return base.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if (this.preReadBuffer != null)
            {
                byte[] tempBuffer = new byte[1];
                int bytesRead;
                if (ReadFromBuffer(tempBuffer, 0, 1, out bytesRead))
                {
                    return tempBuffer[0];
                }
            }
            return base.ReadByte();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            int bytesRead;
            if (ReadFromBuffer(buffer, offset, count, out bytesRead))
            {
                return new CompletedAsyncResult<int>(bytesRead, callback, state);
            }

            return base.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<int>)
            {
                return CompletedAsyncResult<int>.End(result);
            }
            else
            {
                return base.EndRead(result);
            }
        }
    }

    class HttpRequestMessageHttpInput : HttpInput, HttpRequestMessageProperty.IHttpHeaderProvider
    {
        const string SoapAction = "SOAPAction";
        HttpRequestMessage httpRequestMessage;
        ChannelBinding channelBinding;

        public HttpRequestMessageHttpInput(HttpRequestMessage httpRequestMessage, IHttpTransportFactorySettings settings, bool enableChannelBinding, ChannelBinding channelBinding)
            : base(settings, true, enableChannelBinding)
        {
            this.httpRequestMessage = httpRequestMessage;
            this.channelBinding = channelBinding;
        }

        public override long ContentLength
        {
            get
            {
                if (this.httpRequestMessage.Content.Headers.ContentLength == null)
                {
                    // Chunked transfer mode
                    return -1;
                }

                return this.httpRequestMessage.Content.Headers.ContentLength.Value;
            }
        }

        protected override ChannelBinding ChannelBinding
        {
            get
            {
                return this.channelBinding;
            }
        }

        public HttpRequestMessage HttpRequestMessage
        {
            get { return this.httpRequestMessage; }
        }

        protected override bool HasContent
        {
            get
            {
                // In Chunked transfer mode, the ContentLength header is null
                // Otherwise we just rely on the ContentLength header
                return this.httpRequestMessage.Content.Headers.ContentLength == null || this.httpRequestMessage.Content.Headers.ContentLength.Value > 0;
            }
        }

        protected override string ContentTypeCore
        {
            get
            {
                if (!this.HasContent)
                {
                    return null;
                }

                return this.httpRequestMessage.Content.Headers.ContentType == null ? null : this.httpRequestMessage.Content.Headers.ContentType.MediaType;
            }
        }

        public override void ConfigureHttpRequestMessage(HttpRequestMessage message)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }

        protected override Stream GetInputStream()
        {
            if (this.httpRequestMessage.Content == null)
            {
                return Stream.Null;
            }

            return this.httpRequestMessage.Content.ReadAsStreamAsync().Result;
        }

        protected override void AddProperties(Message message)
        {
            HttpRequestMessageProperty requestProperty = new HttpRequestMessageProperty(this.httpRequestMessage);
            message.Properties.Add(HttpRequestMessageProperty.Name, requestProperty);
            message.Properties.Via = this.httpRequestMessage.RequestUri;

            foreach (KeyValuePair<string, object> property in this.httpRequestMessage.Properties)
            {
                message.Properties.Add(property.Key, property.Value);
            }

            this.httpRequestMessage.Properties.Clear();
        }

        protected override string SoapActionHeader
        {
            get
            {
                IEnumerable<string> values;
                if (this.httpRequestMessage.Headers.TryGetValues(SoapAction, out values))
                {
                    foreach (string headerValue in values)
                    {
                        return headerValue;
                    }
                }

                return null;
            }
        }

        public void CopyHeaders(WebHeaderCollection headers)
        {
            // No special-casing for the "WWW-Authenticate" header required here,
            // because this method is only called for the incoming request
            // and the WWW-Authenticate header is a header only applied to responses.
            HttpChannelUtilities.CopyHeaders(this.httpRequestMessage, headers.Add);
        }        

        internal void SetHttpRequestMessage(HttpRequestMessage httpRequestMessage)
        {
            Fx.Assert(httpRequestMessage != null, "httpRequestMessage should not be null.");
            this.httpRequestMessage = httpRequestMessage;
        }
    }
}
