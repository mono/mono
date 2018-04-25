//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Xml;

    sealed class WsrmMessageInfo
    {
        WsrmAcknowledgmentInfo acknowledgementInfo;
        WsrmAckRequestedInfo ackRequestedInfo;
        string action;
        CloseSequenceInfo closeSequenceInfo;
        CloseSequenceResponseInfo closeSequenceResponseInfo;
        CreateSequenceInfo createSequenceInfo;
        CreateSequenceResponseInfo createSequenceResponseInfo;
        Exception faultException;
        MessageFault faultInfo;
        Message faultReply;
        Message message;
        Exception parsingException;
        WsrmSequencedMessageInfo sequencedMessageInfo;
        TerminateSequenceInfo terminateSequenceInfo;
        TerminateSequenceResponseInfo terminateSequenceResponseInfo;
        WsrmUsesSequenceSSLInfo usesSequenceSSLInfo;
        WsrmUsesSequenceSTRInfo usesSequenceSTRInfo;

        public WsrmMessageInfo()
        {
        }

        public WsrmAcknowledgmentInfo AcknowledgementInfo
        {
            get
            {
                return acknowledgementInfo;
            }
        }

        public WsrmAckRequestedInfo AckRequestedInfo
        {
            get
            {
                return this.ackRequestedInfo;
            }
        }

        public string Action
        {
            get
            {
                return this.action;
            }
        }

        public CloseSequenceInfo CloseSequenceInfo
        {
            get
            {
                return this.closeSequenceInfo;
            }
        }

        public CloseSequenceResponseInfo CloseSequenceResponseInfo
        {
            get
            {
                return this.closeSequenceResponseInfo;
            }
        }

        public CreateSequenceInfo CreateSequenceInfo
        {
            get
            {
                return this.createSequenceInfo;
            }
        }

        public CreateSequenceResponseInfo CreateSequenceResponseInfo
        {
            get
            {
                return this.createSequenceResponseInfo;
            }
        }

        public Exception FaultException
        {
            get
            {
                return this.faultException;
            }
            set
            {
                if (this.faultException != null)
                {
                    throw Fx.AssertAndThrow("FaultException can only be set once.");
                }

                this.faultException = value;
            }
        }

        public MessageFault FaultInfo
        {
            get
            {
                return this.faultInfo;
            }
        }

        public Message FaultReply
        {
            get
            {
                return this.faultReply;
            }
            set
            {
                if (this.faultReply != null)
                {
                    throw Fx.AssertAndThrow("FaultReply can only be set once.");
                }

                this.faultReply = value;
            }
        }

        public Message Message
        {
            get
            {
                return this.message;
            }
        }

        public MessageFault MessageFault
        {
            get
            {
                return this.faultInfo;
            }
        }

        public Exception ParsingException
        {
            get
            {
                return this.parsingException;
            }
        }

        public WsrmSequencedMessageInfo SequencedMessageInfo
        {
            get
            {
                return sequencedMessageInfo;
            }
        }

        public TerminateSequenceInfo TerminateSequenceInfo
        {
            get
            {
                return terminateSequenceInfo;
            }
        }

        public TerminateSequenceResponseInfo TerminateSequenceResponseInfo
        {
            get
            {
                return terminateSequenceResponseInfo;
            }
        }

        public WsrmUsesSequenceSSLInfo UsesSequenceSSLInfo
        {
            get
            {
                return usesSequenceSSLInfo;
            }
        }

        public WsrmUsesSequenceSTRInfo UsesSequenceSTRInfo
        {
            get
            {
                return usesSequenceSTRInfo;
            }
        }

        public WsrmHeaderFault WsrmHeaderFault
        {
            get
            {
                return this.faultInfo as WsrmHeaderFault;
            }
        }

        public static Exception CreateInternalFaultException(Message faultReply, string message, Exception inner)
        {
            return new InternalFaultException(faultReply, SR.GetString(SR.WsrmMessageProcessingError, message), inner);
        }

        static Exception CreateWsrmRequiredException(MessageVersion messageVersion)
        {
            string exceptionReason = SR.GetString(SR.WsrmRequiredExceptionString);
            string faultReason = SR.GetString(SR.WsrmRequiredFaultString);
            Message faultReply = new WsrmRequiredFault(faultReason).CreateMessage(messageVersion,
                ReliableMessagingVersion.WSReliableMessaging11);
            return CreateInternalFaultException(faultReply, exceptionReason, new ProtocolException(exceptionReason));
        }

        // Caller should check these things:
        // FaultReply and FaultException, FaultInfo and FaultException or ParsingException
        public static WsrmMessageInfo Get(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, IChannel channel, ISession session, Message message)
        {
            return Get(messageVersion, reliableMessagingVersion, channel, session, message, false);
        }

        public static WsrmMessageInfo Get(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, IChannel channel, ISession session, Message message,
            bool csrOnly)
        {
            WsrmMessageInfo messageInfo = new WsrmMessageInfo();
            messageInfo.message = message;
            bool isFault = true;

            try
            {
                isFault = message.IsFault;
                MessageHeaders headers = message.Headers;
                string action = headers.Action;
                messageInfo.action = action;
                bool foundAction = false;
                bool wsrmFeb2005 = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;
                bool wsrm11 = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;
                bool csOnly = false;

                if (action == WsrmIndex.GetCreateSequenceResponseActionString(reliableMessagingVersion))
                {
                    messageInfo.createSequenceResponseInfo = CreateSequenceResponseInfo.ReadMessage(messageVersion,
                        reliableMessagingVersion, message, headers);
                    ValidateMustUnderstand(messageVersion, message);
                    return messageInfo;
                }

                if (csrOnly)
                    return messageInfo;

                if (action == WsrmIndex.GetTerminateSequenceActionString(reliableMessagingVersion))
                {
                    messageInfo.terminateSequenceInfo = TerminateSequenceInfo.ReadMessage(messageVersion,
                        reliableMessagingVersion, message, headers);
                    foundAction = true;
                }
                else if (action == WsrmIndex.GetCreateSequenceActionString(reliableMessagingVersion))
                {
                    messageInfo.createSequenceInfo = CreateSequenceInfo.ReadMessage(messageVersion,
                        reliableMessagingVersion, session as ISecureConversationSession, message, headers);

                    if (wsrmFeb2005)
                    {
                        ValidateMustUnderstand(messageVersion, message);
                        return messageInfo;
                    }

                    csOnly = true;
                }
                else if (wsrm11)
                {
                    if (action == Wsrm11Strings.CloseSequenceAction)
                    {
                        messageInfo.closeSequenceInfo = CloseSequenceInfo.ReadMessage(messageVersion, message,
                            headers);
                        foundAction = true;
                    }
                    else if (action == Wsrm11Strings.CloseSequenceResponseAction)
                    {
                        messageInfo.closeSequenceResponseInfo = CloseSequenceResponseInfo.ReadMessage(messageVersion,
                             message, headers);
                        foundAction = true;
                    }
                    else if (action == WsrmIndex.GetTerminateSequenceResponseActionString(reliableMessagingVersion))
                    {
                        messageInfo.terminateSequenceResponseInfo = TerminateSequenceResponseInfo.ReadMessage(messageVersion,
                             message, headers);
                        foundAction = true;
                    }
                }

                string wsrmNs = WsrmIndex.GetNamespaceString(reliableMessagingVersion);
                bool soap11 = messageVersion.Envelope == EnvelopeVersion.Soap11;
                bool foundHeader = false;
                int foundTooManyIndex = -1;
                int sequenceIndex = -1;
                int ackIndex = -1;
                int ackRequestedIndex = -1;
                int maxIndex = -1;
                int minIndex = -1;
                int sequenceFaultIndex = -1;
                int usesSequenceSSLIndex = -1;
                int usesSequenceSTRIndex = -1;

                for (int index = 0; index < headers.Count; index++)
                {
                    MessageHeaderInfo header = headers[index];

                    if (!messageVersion.Envelope.IsUltimateDestinationActor(header.Actor))
                        continue;

                    if (header.Namespace == wsrmNs)
                    {
                        bool setIndex = true;

                        if (csOnly)
                        {
                            if (wsrm11 && (header.Name == Wsrm11Strings.UsesSequenceSSL))
                            {
                                if (usesSequenceSSLIndex != -1)
                                {
                                    foundTooManyIndex = index;
                                    break;
                                }
                                usesSequenceSSLIndex = index;
                            }
                            else if (wsrm11 && (header.Name == Wsrm11Strings.UsesSequenceSTR))
                            {
                                if (usesSequenceSTRIndex != -1)
                                {
                                    foundTooManyIndex = index;
                                    break;
                                }
                                usesSequenceSTRIndex = index;
                            }
                            else
                            {
                                setIndex = false;
                            }
                        }
                        else
                        {
                            if (header.Name == WsrmFeb2005Strings.Sequence)
                            {
                                if (sequenceIndex != -1)
                                {
                                    foundTooManyIndex = index;
                                    break;
                                }
                                sequenceIndex = index;
                            }
                            else if (header.Name == WsrmFeb2005Strings.SequenceAcknowledgement)
                            {
                                if (ackIndex != -1)
                                {
                                    foundTooManyIndex = index;
                                    break;
                                }
                                ackIndex = index;
                            }
                            else if (header.Name == WsrmFeb2005Strings.AckRequested)
                            {
                                if (ackRequestedIndex != -1)
                                {
                                    foundTooManyIndex = index;
                                    break;
                                }
                                ackRequestedIndex = index;
                            }
                            else if (soap11 && (header.Name == WsrmFeb2005Strings.SequenceFault))
                            {
                                if (sequenceFaultIndex != -1)
                                {
                                    foundTooManyIndex = index;
                                    break;
                                }
                                sequenceFaultIndex = index;
                            }
                            else
                            {
                                setIndex = false;
                            }
                        }

                        if (setIndex)
                        {
                            if (index > maxIndex)
                                maxIndex = index;

                            if (minIndex == -1)
                                minIndex = index;
                        }
                    }
                }

                if (foundTooManyIndex != -1)
                {
                    Collection<MessageHeaderInfo> notUnderstoodHeaders = new Collection<MessageHeaderInfo>();
                    notUnderstoodHeaders.Add(headers[foundTooManyIndex]);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new MustUnderstandSoapException(notUnderstoodHeaders, messageVersion.Envelope));
                }

                if (maxIndex > -1)
                {
                    BufferedMessage bufferedMessage = message as BufferedMessage;

                    if (bufferedMessage != null && bufferedMessage.Headers.ContainsOnlyBufferedMessageHeaders)
                    {
                        foundHeader = true;

                        using (XmlDictionaryReader reader = headers.GetReaderAtHeader(minIndex))
                        {
                            for (int index = minIndex; index <= maxIndex; index++)
                            {
                                MessageHeaderInfo header = headers[index];

                                if (csOnly)
                                {
                                    if (wsrm11 && (index == usesSequenceSSLIndex))
                                    {
                                        messageInfo.usesSequenceSSLInfo = WsrmUsesSequenceSSLInfo.ReadHeader(
                                            reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else if (wsrm11 && (index == usesSequenceSTRIndex))
                                    {
                                        messageInfo.usesSequenceSTRInfo = WsrmUsesSequenceSTRInfo.ReadHeader(
                                            reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else
                                    {
                                        reader.Skip();
                                    }
                                }
                                else
                                {
                                    if (index == sequenceIndex)
                                    {
                                        messageInfo.sequencedMessageInfo = WsrmSequencedMessageInfo.ReadHeader(
                                            reliableMessagingVersion, reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else if (index == ackIndex)
                                    {
                                        messageInfo.acknowledgementInfo = WsrmAcknowledgmentInfo.ReadHeader(
                                            reliableMessagingVersion, reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else if (index == ackRequestedIndex)
                                    {
                                        messageInfo.ackRequestedInfo = WsrmAckRequestedInfo.ReadHeader(
                                            reliableMessagingVersion, reader, header);
                                        headers.UnderstoodHeaders.Add(header);
                                    }
                                    else
                                    {
                                        reader.Skip();
                                    }
                                }
                            }
                        }
                    }
                }

                if (maxIndex > -1 && !foundHeader)
                {
                    foundHeader = true;

                    if (csOnly)
                    {
                        if (usesSequenceSSLIndex != -1)
                        {
                            using (XmlDictionaryReader reader = headers.GetReaderAtHeader(usesSequenceSSLIndex))
                            {
                                MessageHeaderInfo header = headers[usesSequenceSSLIndex];
                                messageInfo.usesSequenceSSLInfo = WsrmUsesSequenceSSLInfo.ReadHeader(
                                    reader, header);
                                headers.UnderstoodHeaders.Add(header);
                            }
                        }

                        if (usesSequenceSTRIndex != -1)
                        {
                            using (XmlDictionaryReader reader = headers.GetReaderAtHeader(usesSequenceSTRIndex))
                            {
                                MessageHeaderInfo header = headers[usesSequenceSTRIndex];
                                messageInfo.usesSequenceSTRInfo = WsrmUsesSequenceSTRInfo.ReadHeader(
                                    reader, header);
                                headers.UnderstoodHeaders.Add(header);
                            }
                        }
                    }
                    else
                    {
                        if (sequenceIndex != -1)
                        {
                            using (XmlDictionaryReader reader = headers.GetReaderAtHeader(sequenceIndex))
                            {
                                MessageHeaderInfo header = headers[sequenceIndex];

                                messageInfo.sequencedMessageInfo = WsrmSequencedMessageInfo.ReadHeader(
                                    reliableMessagingVersion, reader, header);
                                headers.UnderstoodHeaders.Add(header);
                            }
                        }

                        if (ackIndex != -1)
                        {
                            using (XmlDictionaryReader reader = headers.GetReaderAtHeader(ackIndex))
                            {
                                MessageHeaderInfo header = headers[ackIndex];
                                messageInfo.acknowledgementInfo = WsrmAcknowledgmentInfo.ReadHeader(
                                    reliableMessagingVersion, reader, header);
                                headers.UnderstoodHeaders.Add(header);
                            }
                        }

                        if (ackRequestedIndex != -1)
                        {
                            using (XmlDictionaryReader reader = headers.GetReaderAtHeader(ackRequestedIndex))
                            {
                                MessageHeaderInfo header = headers[ackRequestedIndex];
                                messageInfo.ackRequestedInfo = WsrmAckRequestedInfo.ReadHeader(reliableMessagingVersion,
                                    reader, header);
                                headers.UnderstoodHeaders.Add(header);
                            }
                        }
                    }
                }

                if (csOnly)
                {
                    CreateSequenceInfo.ValidateCreateSequenceHeaders(messageVersion,
                        session as ISecureConversationSession, messageInfo);
                    ValidateMustUnderstand(messageVersion, message);
                    return messageInfo;
                }

                if (messageInfo.sequencedMessageInfo == null && messageInfo.action == null)
                {
                    if (wsrmFeb2005)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(SR.GetString(SR.NoActionNoSequenceHeaderReason), messageVersion.Addressing.Namespace, AddressingStrings.Action, false));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            CreateWsrmRequiredException(messageVersion));
                    }
                }

                if (messageInfo.sequencedMessageInfo == null && message.IsFault)
                {
                    messageInfo.faultInfo = MessageFault.CreateFault(message, TransportDefaults.MaxRMFaultSize);
                    WsrmHeaderFault wsrmFault;

                    if (soap11)
                    {
                        if (WsrmHeaderFault.TryCreateFault11(reliableMessagingVersion, message, messageInfo.faultInfo, sequenceFaultIndex, out wsrmFault))
                        {
                            messageInfo.faultInfo = wsrmFault;
                            messageInfo.faultException = WsrmHeaderFault.CreateException(wsrmFault);
                        }
                    }
                    else
                    {
                        if (WsrmHeaderFault.TryCreateFault12(reliableMessagingVersion, message, messageInfo.faultInfo, out wsrmFault))
                        {
                            messageInfo.faultInfo = wsrmFault;
                            messageInfo.faultException = WsrmHeaderFault.CreateException(wsrmFault);
                        }
                    }

                    // Not a wsrm fault, maybe it is another fault we should understand (i.e. addressing or soap fault).
                    if (wsrmFault == null)
                    {
                        FaultConverter faultConverter = channel.GetProperty<FaultConverter>();

                        if (faultConverter == null)
                        {
                            faultConverter = FaultConverter.GetDefaultFaultConverter(messageVersion);
                        }

                        if (!faultConverter.TryCreateException(message, messageInfo.faultInfo, out messageInfo.faultException))
                        {
                            messageInfo.faultException = new ProtocolException(SR.GetString(SR.UnrecognizedFaultReceived, messageInfo.faultInfo.Code.Namespace, messageInfo.faultInfo.Code.Name, System.ServiceModel.FaultException.GetSafeReasonText(messageInfo.faultInfo)));
                        }
                    }

                    foundAction = true;
                }

                if (!foundHeader && !foundAction)
                {
                    if (wsrmFeb2005)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ActionNotSupportedException(SR.GetString(SR.NonWsrmFeb2005ActionNotSupported, action)));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            CreateWsrmRequiredException(messageVersion));
                    }
                }

                if (foundAction || WsrmUtilities.IsWsrmAction(reliableMessagingVersion, action))
                {
                    ValidateMustUnderstand(messageVersion, message);
                }
            }
            catch (InternalFaultException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);

                messageInfo.FaultReply = exception.FaultReply;
                messageInfo.faultException = exception.InnerException;
            }
            catch (CommunicationException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);

                if (isFault)
                {
                    messageInfo.parsingException = exception;
                    return messageInfo;
                }

                FaultConverter faultConverter = channel.GetProperty<FaultConverter>();
                if (faultConverter == null)
                    faultConverter = FaultConverter.GetDefaultFaultConverter(messageVersion);

                if (faultConverter.TryCreateFaultMessage(exception, out messageInfo.faultReply))
                {
                    messageInfo.faultException = new ProtocolException(SR.GetString(SR.MessageExceptionOccurred), exception);
                }
                else
                {
                    messageInfo.parsingException = new ProtocolException(SR.GetString(SR.MessageExceptionOccurred), exception);
                }
            }
            catch (XmlException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Information);

                messageInfo.parsingException = new ProtocolException(SR.GetString(SR.MessageExceptionOccurred), exception);
            }

            return messageInfo;
        }

        static void ValidateMustUnderstand(MessageVersion version, Message message)
        {
            Collection<MessageHeaderInfo> notUnderstoodHeaders = message.Headers.GetHeadersNotUnderstood();
            if ((notUnderstoodHeaders != null) && (notUnderstoodHeaders.Count > 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MustUnderstandSoapException(notUnderstoodHeaders, version.Envelope));
            }
        }

        [Serializable]
        class InternalFaultException : ProtocolException
        {
            Message faultReply;

            public InternalFaultException()
                : base()
            {
            }

            public InternalFaultException(Message faultReply, string message, Exception inner)
                : base(message, inner)
            {
                this.faultReply = faultReply;
            }

            protected InternalFaultException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }

            public Message FaultReply
            {
                get
                {
                    return this.faultReply;
                }
            }
        }
    }

    sealed class CloseSequenceInfo : WsrmRequestInfo
    {
        UniqueId identifier;
        Int64 lastMsgNumber;

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public Int64 LastMsgNumber
        {
            get
            {
                return this.lastMsgNumber;
            }
            set
            {
                this.lastMsgNumber = value;
            }
        }

        public override string RequestName
        {
            get
            {
                return Wsrm11Strings.CloseSequence;
            }
        }

        public static CloseSequenceInfo ReadMessage(MessageVersion messageVersion, Message message,
            MessageHeaders headers)
        {
            if (message.IsEmpty)
            {
                string reason = SR.GetString(SR.NonEmptyWsrmMessageIsEmpty, Wsrm11Strings.CloseSequenceAction);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(reason));
            }

            CloseSequenceInfo info;
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CloseSequence.Create(reader);
                message.ReadFromBodyContentsToEnd(reader);
            }

            info.SetMessageId(messageVersion, headers);
            info.SetReplyTo(messageVersion, headers);

            return info;
        }
    }

    sealed class CloseSequenceResponseInfo
    {
        UniqueId identifier;
        UniqueId relatesTo;

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public UniqueId RelatesTo
        {
            get
            {
                return this.relatesTo;
            }
            set
            {
                this.relatesTo = value;
            }
        }

        public static CloseSequenceResponseInfo ReadMessage(MessageVersion messageVersion, Message message,
            MessageHeaders headers)
        {
            if (headers.RelatesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MessageHeaderException(SR.GetString(SR.MissingRelatesToOnWsrmResponseReason,
                    DXD.Wsrm11Dictionary.CloseSequenceResponse), messageVersion.Addressing.Namespace,
                    AddressingStrings.RelatesTo, false));
            }

            if (message.IsEmpty)
            {
                string reason = SR.GetString(SR.NonEmptyWsrmMessageIsEmpty,
                    Wsrm11Strings.CloseSequenceResponseAction);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(reason));
            }

            CloseSequenceResponseInfo info;
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CloseSequenceResponse.Create(reader);
                message.ReadFromBodyContentsToEnd(reader);
            }

            info.relatesTo = headers.RelatesTo;
            return info;
        }
    }

    sealed class CreateSequenceInfo : WsrmRequestInfo
    {
        EndpointAddress acksTo = EndpointAddress.AnonymousAddress;
        Nullable<TimeSpan> expires;
        Nullable<TimeSpan> offerExpires;
        UniqueId offerIdentifier;
        Uri to;

        public EndpointAddress AcksTo
        {
            get
            {
                return this.acksTo;
            }
            set
            {
                this.acksTo = value;
            }
        }

        public Nullable<TimeSpan> Expires
        {
            get
            {
                return this.expires;
            }
            set
            {
                this.expires = value;
            }
        }

        public Nullable<TimeSpan> OfferExpires
        {
            get
            {
                return this.offerExpires;
            }
            set
            {
                this.offerExpires = value;
            }
        }

        public UniqueId OfferIdentifier
        {
            get
            {
                return this.offerIdentifier;
            }
            set
            {
                this.offerIdentifier = value;
            }
        }

        public override string RequestName
        {
            get
            {
                return WsrmFeb2005Strings.CreateSequence;
            }
        }

        public Uri To
        {
            get
            {
                return this.to;
            }
        }

        public static CreateSequenceInfo ReadMessage(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, ISecureConversationSession securitySession,
            Message message, MessageHeaders headers)
        {
            if (message.IsEmpty)
            {
                string reason = SR.GetString(SR.NonEmptyWsrmMessageIsEmpty,
                    WsrmIndex.GetCreateSequenceActionString(reliableMessagingVersion));
                Message faultReply = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, reason);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(faultReply, reason, new ProtocolException(reason)));
            }

            CreateSequenceInfo info;
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CreateSequence.Create(messageVersion, reliableMessagingVersion, securitySession, reader);
                message.ReadFromBodyContentsToEnd(reader);
            }

            info.SetMessageId(messageVersion, headers);
            info.SetReplyTo(messageVersion, headers);

            if (info.AcksTo.Uri != info.ReplyTo.Uri)
            {
                string reason = SR.GetString(SR.CSRefusedAcksToMustEqualReplyTo);
                Message faultReply = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion, reliableMessagingVersion, reason);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(faultReply, reason, new ProtocolException(reason)));
            }

            info.to = message.Headers.To;
            if (info.to == null && messageVersion.Addressing == AddressingVersion.WSAddressing10)
                info.to = messageVersion.Addressing.AnonymousUri;

            return info;
        }

        public static void ValidateCreateSequenceHeaders(MessageVersion messageVersion,
            ISecureConversationSession securitySession, WsrmMessageInfo info)
        {
            string reason = null;

            if (info.UsesSequenceSSLInfo != null)
            {
                reason = SR.GetString(SR.CSRefusedSSLNotSupported);
            }
            else if ((info.UsesSequenceSTRInfo != null) && (securitySession == null))
            {
                reason = SR.GetString(SR.CSRefusedSTRNoWSSecurity);
            }
            else if ((info.UsesSequenceSTRInfo == null) && (securitySession != null))
            {
                reason = SR.GetString(SR.CSRefusedNoSTRWSSecurity);
            }

            if (reason != null)
            {
                Message faultReply = WsrmUtilities.CreateCSRefusedProtocolFault(messageVersion,
                    ReliableMessagingVersion.WSReliableMessaging11, reason);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(WsrmMessageInfo.CreateInternalFaultException(faultReply, reason, new ProtocolException(reason)));
            }
        }
    }

    sealed class CreateSequenceResponseInfo
    {
        EndpointAddress acceptAcksTo;
        UniqueId identifier;
        UniqueId relatesTo;

        public EndpointAddress AcceptAcksTo
        {
            get
            {
                return this.acceptAcksTo;
            }
            set
            {
                this.acceptAcksTo = value;
            }
        }

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public UniqueId RelatesTo
        {
            get
            {
                return this.relatesTo;
            }
            set
            {
                this.relatesTo = value;
            }
        }

        public static CreateSequenceResponseInfo ReadMessage(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, Message message, MessageHeaders headers)
        {
            if (message.IsEmpty)
            {
                string reason = SR.GetString(SR.NonEmptyWsrmMessageIsEmpty,
                    WsrmIndex.GetCreateSequenceResponseActionString(reliableMessagingVersion));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(reason));
            }

            if (headers.RelatesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MessageHeaderException(SR.GetString(SR.MissingRelatesToOnWsrmResponseReason,
                    XD.WsrmFeb2005Dictionary.CreateSequenceResponse), messageVersion.Addressing.Namespace,
                    AddressingStrings.RelatesTo, false));
            }

            CreateSequenceResponseInfo info;
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = CreateSequenceResponse.Create(messageVersion.Addressing, reliableMessagingVersion, reader);
                message.ReadFromBodyContentsToEnd(reader);
            }

            info.RelatesTo = headers.RelatesTo;
            return info;
        }
    }

    sealed class TerminateSequenceInfo : WsrmRequestInfo
    {
        UniqueId identifier;
        Int64 lastMsgNumber;

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public Int64 LastMsgNumber
        {
            get
            {
                return this.lastMsgNumber;
            }
            set
            {
                this.lastMsgNumber = value;
            }
        }

        public override string RequestName
        {
            get
            {
                return WsrmFeb2005Strings.TerminateSequence;
            }
        }

        public static TerminateSequenceInfo ReadMessage(MessageVersion messageVersion,
            ReliableMessagingVersion reliableMessagingVersion, Message message, MessageHeaders headers)
        {
            if (message.IsEmpty)
            {
                string reason = SR.GetString(SR.NonEmptyWsrmMessageIsEmpty,
                    WsrmIndex.GetTerminateSequenceActionString(reliableMessagingVersion));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(reason));
            }

            TerminateSequenceInfo info;
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = TerminateSequence.Create(reliableMessagingVersion, reader);
                message.ReadFromBodyContentsToEnd(reader);
            }

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                info.SetMessageId(messageVersion, headers);
                info.SetReplyTo(messageVersion, headers);
            }

            return info;
        }
    }

    sealed class TerminateSequenceResponseInfo
    {
        UniqueId identifier;
        UniqueId relatesTo;

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public UniqueId RelatesTo
        {
            get
            {
                return this.relatesTo;
            }
            set
            {
                this.relatesTo = value;
            }
        }

        public static TerminateSequenceResponseInfo ReadMessage(MessageVersion messageVersion, Message message,
            MessageHeaders headers)
        {
            if (headers.RelatesTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MessageHeaderException(SR.GetString(SR.MissingRelatesToOnWsrmResponseReason,
                    DXD.Wsrm11Dictionary.TerminateSequenceResponse), messageVersion.Addressing.Namespace,
                    AddressingStrings.RelatesTo, false));
            }

            if (message.IsEmpty)
            {
                string reason = SR.GetString(SR.NonEmptyWsrmMessageIsEmpty,
                    WsrmIndex.GetTerminateSequenceResponseActionString(ReliableMessagingVersion.WSReliableMessaging11));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(reason));
            }

            TerminateSequenceResponseInfo info;
            using (XmlDictionaryReader reader = message.GetReaderAtBodyContents())
            {
                info = TerminateSequenceResponse.Create(reader);
                message.ReadFromBodyContentsToEnd(reader);
            }

            info.relatesTo = headers.RelatesTo;
            return info;
        }
    }

    internal abstract class WsrmMessageHeader : DictionaryHeader, IMessageHeaderWithSharedNamespace
    {
        ReliableMessagingVersion reliableMessagingVersion;

        protected WsrmMessageHeader(ReliableMessagingVersion reliableMessagingVersion)
        {
            this.reliableMessagingVersion = reliableMessagingVersion;
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get { return XD.WsrmFeb2005Dictionary.Prefix; }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get { return WsrmIndex.GetNamespace(this.reliableMessagingVersion); }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get { return WsrmIndex.GetNamespace(this.reliableMessagingVersion); }
        }

        public override string Namespace
        {
            get { return WsrmIndex.GetNamespaceString(this.reliableMessagingVersion); }
        }

        protected ReliableMessagingVersion ReliableMessagingVersion
        {
            get { return this.reliableMessagingVersion; }
        }
    }

    abstract class WsrmHeaderInfo
    {
        MessageHeaderInfo messageHeader;

        protected WsrmHeaderInfo(MessageHeaderInfo messageHeader)
        {
            this.messageHeader = messageHeader;
        }

        public MessageHeaderInfo MessageHeader
        {
            get
            {
                return messageHeader;
            }
        }
    }

    abstract class WsrmRequestInfo
    {
        UniqueId messageId;
        EndpointAddress replyTo;

        protected WsrmRequestInfo()
        {
        }

        public UniqueId MessageId
        {
            get
            {
                return this.messageId;
            }
        }

        public EndpointAddress ReplyTo
        {
            get
            {
                return this.replyTo;
            }
        }

        public abstract string RequestName
        {
            get;
        }

        protected void SetMessageId(MessageVersion messageVersion, MessageHeaders headers)
        {
            this.messageId = headers.MessageId;

            if (this.messageId == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(
                    SR.GetString(SR.MissingMessageIdOnWsrmRequest, RequestName),
                    messageVersion.Addressing.Namespace,
                    AddressingStrings.MessageId,
                    false));
            }
        }

        protected void SetReplyTo(MessageVersion messageVersion, MessageHeaders headers)
        {
            this.replyTo = headers.ReplyTo;

            if (messageVersion.Addressing == AddressingVersion.WSAddressing10 && this.replyTo == null)
            {
                this.replyTo = EndpointAddress.AnonymousAddress;
            }

            if (this.replyTo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageHeaderException(
                    SR.GetString(SR.MissingReplyToOnWsrmRequest, RequestName),
                    messageVersion.Addressing.Namespace,
                    AddressingStrings.ReplyTo,
                    false));
            }
        }
    }

    internal sealed class WsrmSequencedMessageInfo : WsrmHeaderInfo
    {
        UniqueId sequenceID;
        Int64 sequenceNumber;
        bool lastMessage;

        WsrmSequencedMessageInfo(
            UniqueId sequenceID,
            Int64 sequenceNumber,
            bool lastMessage,
            MessageHeaderInfo header)
            : base(header)
        {
            this.sequenceID = sequenceID;
            this.sequenceNumber = sequenceNumber;
            this.lastMessage = lastMessage;
        }

        public UniqueId SequenceID
        {
            get
            {
                return sequenceID;
            }
        }

        public Int64 SequenceNumber
        {
            get
            {
                return sequenceNumber;
            }
        }

        public bool LastMessage
        {
            get
            {
                return lastMessage;
            }
        }

        public static WsrmSequencedMessageInfo ReadHeader(ReliableMessagingVersion reliableMessagingVersion,
            XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            reader.ReadStartElement();

            reader.ReadStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            UniqueId sequenceID = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            reader.ReadStartElement(wsrmFeb2005Dictionary.MessageNumber, wsrmNs);
            Int64 sequenceNumber = WsrmUtilities.ReadSequenceNumber(reader);
            reader.ReadEndElement();

            bool lastMessage = false;

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (reader.IsStartElement(wsrmFeb2005Dictionary.LastMessage, wsrmNs))
                {
                    WsrmUtilities.ReadEmptyElement(reader);
                    lastMessage = true;
                }
            }

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return new WsrmSequencedMessageInfo(sequenceID, sequenceNumber, lastMessage, header);
        }
    }

    internal sealed class WsrmSequencedMessageHeader : WsrmMessageHeader
    {
        bool lastMessage;
        UniqueId sequenceID;
        Int64 sequenceNumber;

        public WsrmSequencedMessageHeader(
            ReliableMessagingVersion reliableMessagingVersion,
            UniqueId sequenceID,
            Int64 sequenceNumber,
            bool lastMessage)
            : base(reliableMessagingVersion)
        {
            this.sequenceID = sequenceID;
            this.sequenceNumber = sequenceNumber;
            this.lastMessage = lastMessage;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.WsrmFeb2005Dictionary.Sequence; }
        }

        public override bool MustUnderstand
        {
            get { return true; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = this.DictionaryNamespace;

            writer.WriteStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.sequenceID);
            writer.WriteEndElement();

            writer.WriteStartElement(wsrmFeb2005Dictionary.MessageNumber, wsrmNs);
            writer.WriteValue(this.sequenceNumber);
            writer.WriteEndElement();

            if ((this.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                && lastMessage)
            {
                writer.WriteStartElement(wsrmFeb2005Dictionary.LastMessage, wsrmNs);
                writer.WriteEndElement();
            }
        }
    }

    internal sealed class WsrmAcknowledgmentInfo : WsrmHeaderInfo
    {
        int bufferRemaining;
        bool final;
        SequenceRangeCollection ranges;
        UniqueId sequenceID;

        WsrmAcknowledgmentInfo(
            UniqueId sequenceID,
            SequenceRangeCollection ranges,
            bool final,
            int bufferRemaining,
            MessageHeaderInfo header)
            : base(header)
        {
            this.sequenceID = sequenceID;
            this.ranges = ranges;
            this.final = final;
            this.bufferRemaining = bufferRemaining;
        }

        public int BufferRemaining
        {
            get
            {
                return this.bufferRemaining;
            }
        }

        public bool Final
        {
            get
            {
                return this.final;
            }
        }

        public SequenceRangeCollection Ranges
        {
            get
            {
                return this.ranges;
            }
        }

        public UniqueId SequenceID
        {
            get
            {
                return this.sequenceID;
            }
        }

        // February 2005 - Reads Identifier, AcknowledgementRange, Nack
        // 1.1 - Reads Identifier, AcknowledgementRange, None, Final, Nack
        internal static void ReadAck(ReliableMessagingVersion reliableMessagingVersion,
            XmlDictionaryReader reader, out UniqueId sequenceId, out SequenceRangeCollection rangeCollection,
            out bool final)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            reader.ReadStartElement(wsrmFeb2005Dictionary.SequenceAcknowledgement, wsrmNs);
            reader.ReadStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            sequenceId = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();
            bool allowZero = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005;

            rangeCollection = SequenceRangeCollection.Empty;
            while (reader.IsStartElement(wsrmFeb2005Dictionary.AcknowledgementRange, wsrmNs))
            {
                reader.MoveToAttribute(WsrmFeb2005Strings.Lower);
                Int64 lower = WsrmUtilities.ReadSequenceNumber(reader, allowZero);

                reader.MoveToAttribute(WsrmFeb2005Strings.Upper);
                Int64 upper = WsrmUtilities.ReadSequenceNumber(reader, allowZero);

                if (lower < 0 || lower > upper
                    || ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005) && (lower == 0 && upper > 0))
                    || ((reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (lower == 0)))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.InvalidSequenceRange, lower, upper)));
                }

                rangeCollection = rangeCollection.MergeWith(new SequenceRange(lower, upper));

                reader.MoveToElement();

                WsrmUtilities.ReadEmptyElement(reader);
            }

            bool validAck = rangeCollection.Count > 0;
            final = false;
            bool wsrm11 = reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11;

            if (wsrm11)
            {
                Wsrm11Dictionary wsrm11Dictionary = DXD.Wsrm11Dictionary;

                if (reader.IsStartElement(wsrm11Dictionary.None, wsrmNs))
                {
                    if (validAck)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                            SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                            wsrmFeb2005Dictionary.SequenceAcknowledgement)));
                    }

                    WsrmUtilities.ReadEmptyElement(reader);
                    validAck = true;
                }

                if (reader.IsStartElement(wsrm11Dictionary.Final, wsrmNs))
                {
                    if (!validAck)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                            SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                            wsrmFeb2005Dictionary.SequenceAcknowledgement)));
                    }

                    WsrmUtilities.ReadEmptyElement(reader);
                    final = true;
                }
            }

            bool foundNack = false;
            while (reader.IsStartElement(wsrmFeb2005Dictionary.Nack, wsrmNs))
            {
                if (validAck)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                        SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                        MessageStrings.Body)));
                }

                reader.ReadStartElement();
                WsrmUtilities.ReadSequenceNumber(reader, true);
                reader.ReadEndElement();
                foundNack = true;
            }

            if (!validAck && !foundNack)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                    SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                    MessageStrings.Body)));
            }
        }

        public static WsrmAcknowledgmentInfo ReadHeader(ReliableMessagingVersion reliableMessagingVersion,
            XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            UniqueId sequenceID;
            SequenceRangeCollection rangeCollection;
            bool final;
            ReadAck(reliableMessagingVersion, reader, out sequenceID, out rangeCollection, out final);

            int bufferRemaining = -1;

            // Parse the extensibility section.
            while (reader.IsStartElement())
            {
                if (reader.IsStartElement(wsrmFeb2005Dictionary.BufferRemaining,
                    XD.WsrmFeb2005Dictionary.NETNamespace))
                {
                    if (bufferRemaining != -1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                            SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                            MessageStrings.Body)));
                    }

                    reader.ReadStartElement();
                    bufferRemaining = reader.ReadContentAsInt();
                    reader.ReadEndElement();

                    if (bufferRemaining < 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                            SR.GetString(SR.InvalidBufferRemaining, bufferRemaining)));
                    }

                    // Found BufferRemaining, continue parsing.
                    continue;
                }

                if (reader.IsStartElement(wsrmFeb2005Dictionary.AcknowledgementRange, wsrmNs))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                        SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                        MessageStrings.Body)));
                }
                else if (reader.IsStartElement(wsrmFeb2005Dictionary.Nack, wsrmNs))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                        SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                        MessageStrings.Body)));
                }
                else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    Wsrm11Dictionary wsrm11Dictionary = DXD.Wsrm11Dictionary;

                    if (reader.IsStartElement(wsrm11Dictionary.None, wsrmNs)
                        || reader.IsStartElement(wsrm11Dictionary.Final, wsrmNs))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                            SR.GetString(SR.UnexpectedXmlChildNode, reader.Name, reader.NodeType,
                            wsrmFeb2005Dictionary.SequenceAcknowledgement)));
                    }
                }

                // Advance the reader in all cases.
                reader.Skip();
            }

            reader.ReadEndElement();

            return new WsrmAcknowledgmentInfo(sequenceID, rangeCollection, final, bufferRemaining, header);
        }
    }

    internal sealed class WsrmAcknowledgmentHeader : WsrmMessageHeader
    {
        int bufferRemaining;
        bool final;
        SequenceRangeCollection ranges;
        UniqueId sequenceID;

        public WsrmAcknowledgmentHeader(
            ReliableMessagingVersion reliableMessagingVersion,
            UniqueId sequenceID,
            SequenceRangeCollection ranges,
            bool final,
            int bufferRemaining)
            : base(reliableMessagingVersion)
        {
            this.sequenceID = sequenceID;
            this.ranges = ranges;
            this.final = final;
            this.bufferRemaining = bufferRemaining;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.WsrmFeb2005Dictionary.SequenceAcknowledgement; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = this.DictionaryNamespace;

            WriteAckRanges(writer, this.ReliableMessagingVersion, this.sequenceID, this.ranges);

            if ((this.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && this.final)
            {
                writer.WriteStartElement(DXD.Wsrm11Dictionary.Final, wsrmNs);
                writer.WriteEndElement();
            }

            if (this.bufferRemaining != -1)
            {
                writer.WriteStartElement(WsrmFeb2005Strings.NETPrefix, wsrmFeb2005Dictionary.BufferRemaining,
                    XD.WsrmFeb2005Dictionary.NETNamespace);
                writer.WriteValue(this.bufferRemaining);
                writer.WriteEndElement();
            }
        }

        // February 2005 - Writes Identifier, AcknowledgementRange
        // 1.1 - Writes Identifier, AcknowledgementRange | None
        internal static void WriteAckRanges(XmlDictionaryWriter writer,
            ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceId, SequenceRangeCollection ranges)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            writer.WriteStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(sequenceId);
            writer.WriteEndElement();

            if (ranges.Count == 0)
            {
                if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
                {
                    ranges = ranges.MergeWith(0);
                }
                else if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
                {
                    writer.WriteStartElement(DXD.Wsrm11Dictionary.None, wsrmNs);
                    writer.WriteEndElement();
                }
            }

            for (int index = 0; index < ranges.Count; index++)
            {
                writer.WriteStartElement(wsrmFeb2005Dictionary.AcknowledgementRange, wsrmNs);
                writer.WriteStartAttribute(wsrmFeb2005Dictionary.Lower, null);
                writer.WriteValue(ranges[index].Lower);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute(wsrmFeb2005Dictionary.Upper, null);
                writer.WriteValue(ranges[index].Upper);
                writer.WriteEndAttribute();
                writer.WriteEndElement();
            }
        }
    }

    internal sealed class WsrmAckRequestedInfo : WsrmHeaderInfo
    {
        UniqueId sequenceID;

        public WsrmAckRequestedInfo(UniqueId sequenceID, MessageHeaderInfo header)
            : base(header)
        {
            this.sequenceID = sequenceID;
        }

        public UniqueId SequenceID
        {
            get
            {
                return this.sequenceID;
            }
        }

        public static WsrmAckRequestedInfo ReadHeader(ReliableMessagingVersion reliableMessagingVersion,
            XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            reader.ReadStartElement();

            reader.ReadStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            UniqueId sequenceID = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if (reader.IsStartElement(wsrmFeb2005Dictionary.MessageNumber, wsrmNs))
                {
                    reader.ReadStartElement();
                    WsrmUtilities.ReadSequenceNumber(reader, true);
                    reader.ReadEndElement();
                }
            }

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return new WsrmAckRequestedInfo(sequenceID, header);
        }
    }

    internal sealed class WsrmAckRequestedHeader : WsrmMessageHeader
    {
        UniqueId sequenceID;

        public WsrmAckRequestedHeader(ReliableMessagingVersion reliableMessagingVersion, UniqueId sequenceID)
            : base(reliableMessagingVersion)
        {
            this.sequenceID = sequenceID;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.WsrmFeb2005Dictionary.AckRequested; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = this.DictionaryNamespace;

            writer.WriteStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.sequenceID);
            writer.WriteEndElement();
        }
    }

    // We do not generate the UsesSequenceSSL header. Thus, there is an info, but no header.
    internal sealed class WsrmUsesSequenceSSLInfo : WsrmHeaderInfo
    {
        WsrmUsesSequenceSSLInfo(MessageHeaderInfo header)
            : base(header)
        {
        }

        public static WsrmUsesSequenceSSLInfo ReadHeader(XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmUtilities.ReadEmptyElement(reader);
            return new WsrmUsesSequenceSSLInfo(header);
        }
    }

    internal sealed class WsrmUsesSequenceSTRHeader : WsrmMessageHeader
    {
        public WsrmUsesSequenceSTRHeader()
            : base(ReliableMessagingVersion.WSReliableMessaging11)
        {
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return DXD.Wsrm11Dictionary.UsesSequenceSTR; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
        }

        public override bool MustUnderstand
        {
            get { return true; }
        }
    }

    internal sealed class WsrmUsesSequenceSTRInfo : WsrmHeaderInfo
    {
        WsrmUsesSequenceSTRInfo(MessageHeaderInfo header)
            : base(header)
        {
        }

        public static WsrmUsesSequenceSTRInfo ReadHeader(XmlDictionaryReader reader, MessageHeaderInfo header)
        {
            WsrmUtilities.ReadEmptyElement(reader);
            return new WsrmUsesSequenceSTRInfo(header);
        }
    }
}
