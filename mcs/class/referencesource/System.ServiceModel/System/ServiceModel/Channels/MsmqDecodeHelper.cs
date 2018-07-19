//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.ServiceModel.MsmqIntegration;
    using System.ServiceModel.Security;
    using System.Transactions;
    using System.Xml;
    using System.Xml.Serialization;

    static class MsmqDecodeHelper
    {
        static ActiveXSerializer activeXSerializer;
        static BinaryFormatter binaryFormatter;
        const int defaultMaxViaSize = 2048;
        const int defaultMaxContentTypeSize = 256;

        static ActiveXSerializer ActiveXSerializer
        {
            get
            {
                if (null == activeXSerializer)
                    activeXSerializer = new ActiveXSerializer();

                return activeXSerializer;
            }
        }

        static BinaryFormatter BinaryFormatter
        {
            get
            {
                if (null == binaryFormatter)
                    binaryFormatter = new BinaryFormatter();

                return binaryFormatter;
            }
        }

        static void ReadServerMode(MsmqChannelListenerBase listener, ServerModeDecoder modeDecoder, byte[] incoming, long lookupId, ref int offset, ref int size)
        {
            for (;;)
            {
                if (size <= 0)
                {
                    throw listener.NormalizePoisonException(lookupId, modeDecoder.CreatePrematureEOFException());
                }
                int decoded = modeDecoder.Decode(incoming, offset, size);
                offset += decoded;
                size -= decoded;
                if (ServerModeDecoder.State.Done == modeDecoder.CurrentState)
                    break;
            }
        }

        internal static Message DecodeTransportDatagram(MsmqInputChannelListener listener, MsmqReceiveHelper receiver, MsmqInputMessage msmqMessage, MsmqMessageProperty messageProperty)
        {
            using (MsmqDiagnostics.BoundReceiveBytesOperation())
            {
                long lookupId = msmqMessage.LookupId.Value;
                int size = msmqMessage.BodyLength.Value;
                int offset = 0;
                byte[] incoming = msmqMessage.Body.Buffer;

                ServerModeDecoder modeDecoder = new ServerModeDecoder();

                try
                {
                    ReadServerMode(listener, modeDecoder, incoming, messageProperty.LookupId, ref offset, ref size);
                }
                catch (ProtocolException ex)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                }

                if (modeDecoder.Mode != FramingMode.SingletonSized)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadFrame)));
                }

                ServerSingletonSizedDecoder decoder = new ServerSingletonSizedDecoder(0, defaultMaxViaSize, defaultMaxContentTypeSize);
                try
                {
                    for (;;)
                    {
                        if (size <= 0)
                        {
                            throw listener.NormalizePoisonException(messageProperty.LookupId, decoder.CreatePrematureEOFException());
                        }

                        int decoded = decoder.Decode(incoming, offset, size);
                        offset += decoded;
                        size -= decoded;
                        if (decoder.CurrentState == ServerSingletonSizedDecoder.State.Start)
                            break;
                    }
                }
                catch (ProtocolException ex)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                }

                if (size > listener.MaxReceivedMessageSize)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(listener.MaxReceivedMessageSize));
                }

                if (!listener.MessageEncoderFactory.Encoder.IsContentTypeSupported(decoder.ContentType))
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadContentType)));
                }

                byte[] envelopeBuffer = listener.BufferManager.TakeBuffer(size);
                Buffer.BlockCopy(incoming, offset, envelopeBuffer, 0, size);

                Message message = null;

                using (MsmqDiagnostics.BoundDecodeOperation())
                {
                    try
                    {
                        message = listener.MessageEncoderFactory.Encoder.ReadMessage(
                            new ArraySegment<byte>(envelopeBuffer, 0, size), listener.BufferManager);
                    }
                    catch (XmlException e)
                    {
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadXml), e));
                    }

                    bool closeMessage = true;
                    try
                    {
                        SecurityMessageProperty securityProperty = listener.ValidateSecurity(msmqMessage);
                        if (null != securityProperty)
                            message.Properties.Security = securityProperty;

                        closeMessage = false;
                        MsmqDiagnostics.TransferFromTransport(message);
                        return message;
                    }
                    catch (Exception ex)
                    {
                        if (Fx.IsFatal(ex))
                            throw;
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                    }
                    finally
                    {
                        if (closeMessage)
                        {
                            message.Close();
                        }
                    }
                }
            }
        }

        internal static IInputSessionChannel DecodeTransportSessiongram(
            MsmqInputSessionChannelListener listener,
            MsmqInputMessage msmqMessage,
            MsmqMessageProperty messageProperty,
            MsmqReceiveContextLockManager receiveContextManager)
        {
            using (MsmqDiagnostics.BoundReceiveBytesOperation())
            {
                long lookupId = msmqMessage.LookupId.Value;

                int size = msmqMessage.BodyLength.Value;
                int offset = 0;
                byte[] incoming = msmqMessage.Body.Buffer;
                MsmqReceiveHelper receiver = listener.MsmqReceiveHelper;

                ServerModeDecoder modeDecoder = new ServerModeDecoder();
                try
                {
                    ReadServerMode(listener, modeDecoder, incoming, messageProperty.LookupId, ref offset, ref size);
                }
                catch (ProtocolException ex)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                }

                if (modeDecoder.Mode != FramingMode.Simplex)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadFrame)));
                }

                MsmqInputSessionChannel channel = null;
                ServerSessionDecoder sessionDecoder = new ServerSessionDecoder(0, defaultMaxViaSize, defaultMaxContentTypeSize);

                try
                {
                    for (;;)
                    {
                        if (size <= 0)
                        {
                            throw listener.NormalizePoisonException(messageProperty.LookupId, sessionDecoder.CreatePrematureEOFException());
                        }

                        int decoded = sessionDecoder.Decode(incoming, offset, size);
                        offset += decoded;
                        size -= decoded;
                        if (ServerSessionDecoder.State.EnvelopeStart == sessionDecoder.CurrentState)
                            break;
                    }
                }
                catch (ProtocolException ex)
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                }

                MessageEncoder encoder = listener.MessageEncoderFactory.CreateSessionEncoder();

                if (!encoder.IsContentTypeSupported(sessionDecoder.ContentType))
                {
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadContentType)));
                }

                ReceiveContext receiveContext = null;

                // tack on the receive context property depending on the receive mode
                if (receiver.MsmqReceiveParameters.ReceiveContextSettings.Enabled)
                {
                    receiveContext = receiveContextManager.CreateMsmqReceiveContext(msmqMessage.LookupId.Value);
                }

                channel = new MsmqInputSessionChannel(listener, Transaction.Current, receiveContext);

                Message message = DecodeSessiongramMessage(listener, channel, encoder, messageProperty, incoming, offset, sessionDecoder.EnvelopeSize);

                SecurityMessageProperty securityProperty = null;
                try
                {
                    securityProperty = listener.ValidateSecurity(msmqMessage);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                        throw;
                    channel.FaultChannel();
                    receiver.FinalDisposition(messageProperty);
                    throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                }

                if (null != securityProperty)
                    message.Properties.Security = securityProperty;

                message.Properties[MsmqMessageProperty.Name] = messageProperty;
                channel.EnqueueAndDispatch(message);
                listener.RaiseMessageReceived();

                for (;;)
                {
                    int decoded;
                    try
                    {
                        if (size <= 0)
                        {
                            channel.FaultChannel();
                            receiver.FinalDisposition(messageProperty);
                            throw listener.NormalizePoisonException(messageProperty.LookupId, sessionDecoder.CreatePrematureEOFException());
                        }

                        decoded = sessionDecoder.Decode(incoming, offset, size);
                    }
                    catch (ProtocolException ex)
                    {
                        channel.FaultChannel();
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, ex);
                    }
                    offset += decoded;
                    size -= decoded;
                    if (ServerSessionDecoder.State.End == sessionDecoder.CurrentState)
                        break;
                    if (ServerSessionDecoder.State.EnvelopeStart == sessionDecoder.CurrentState)
                    {
                        message = DecodeSessiongramMessage(listener, channel, encoder, messageProperty, incoming, offset, sessionDecoder.EnvelopeSize);
                        if (null != securityProperty)
                        {
                            message.Properties.Security = (SecurityMessageProperty)securityProperty.CreateCopy();
                        }
                        message.Properties[MsmqMessageProperty.Name] = messageProperty;
                        channel.EnqueueAndDispatch(message);
                        listener.RaiseMessageReceived();
                    }
                }

                channel.Shutdown();
                MsmqDiagnostics.SessiongramReceived(channel.Session.Id, msmqMessage.MessageId, channel.InternalPendingItems);

                return channel;
            }
        }

        static Message DecodeSessiongramMessage(
            MsmqInputSessionChannelListener listener,
            MsmqInputSessionChannel channel,
            MessageEncoder encoder,
            MsmqMessageProperty messageProperty,
            byte[] buffer,
            int offset,
            int size)
        {
            if (size > listener.MaxReceivedMessageSize)
            {
                channel.FaultChannel();
                listener.MsmqReceiveHelper.FinalDisposition(messageProperty);
                throw listener.NormalizePoisonException(messageProperty.LookupId, MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(listener.MaxReceivedMessageSize));
            }

            // Fix for CSDMain bug 17842
            // size is derived from user data, check for corruption
            if ((size + offset) > buffer.Length)
            {
                listener.MsmqReceiveHelper.FinalDisposition(messageProperty);
                throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadFrame)));
            }

            byte[] envelopeBuffer = listener.BufferManager.TakeBuffer(size);
            Buffer.BlockCopy(buffer, offset, envelopeBuffer, 0, size);
            try
            {
                Message message = null;
                using (MsmqDiagnostics.BoundDecodeOperation())
                {
                    message = encoder.ReadMessage(new ArraySegment<byte>(envelopeBuffer, 0, size), listener.BufferManager);
                    MsmqDiagnostics.TransferFromTransport(message);
                }
                return message;
            }
            catch (XmlException e)
            {
                channel.FaultChannel();
                listener.MsmqReceiveHelper.FinalDisposition(messageProperty);
                throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqBadXml), e));
            }
        }

        internal static Message DecodeIntegrationDatagram(MsmqIntegrationChannelListener listener, MsmqReceiveHelper receiver, MsmqIntegrationInputMessage msmqMessage, MsmqMessageProperty messageProperty)
        {
            using (MsmqDiagnostics.BoundReceiveBytesOperation())
            {
                Message message = Message.CreateMessage(MessageVersion.None, (string)null);
                bool closeMessage = true;

                try
                {
                    SecurityMessageProperty securityProperty = listener.ValidateSecurity(msmqMessage);
                    if (null != securityProperty)
                        message.Properties.Security = securityProperty;

                    MsmqIntegrationMessageProperty integrationProperty = new MsmqIntegrationMessageProperty();
                    msmqMessage.SetMessageProperties(integrationProperty);

                    int size = msmqMessage.BodyLength.Value;

                    if (size > listener.MaxReceivedMessageSize)
                    {
                        receiver.FinalDisposition(messageProperty);
                        throw listener.NormalizePoisonException(messageProperty.LookupId, MaxMessageSizeStream.CreateMaxReceivedMessageSizeExceededException(listener.MaxReceivedMessageSize));
                    }

                    byte[] bodyBytes = msmqMessage.Body.GetBufferCopy(size);

                    MemoryStream bodyStream = new MemoryStream(bodyBytes, 0, bodyBytes.Length, false);

                    object body = null;
                    using (MsmqDiagnostics.BoundDecodeOperation())
                    {
                        try
                        {
                            body = DeserializeForIntegration(listener, bodyStream, integrationProperty, messageProperty.LookupId);
                        }
                        catch (SerializationException e)
                        {
                            receiver.FinalDisposition(messageProperty);
                            throw listener.NormalizePoisonException(messageProperty.LookupId, new ProtocolException(SR.GetString(SR.MsmqDeserializationError), e));
                        }

                        integrationProperty.Body = body;
                        message.Properties[MsmqIntegrationMessageProperty.Name] = integrationProperty;
                        bodyStream.Seek(0, SeekOrigin.Begin);
                        message.Headers.To = listener.Uri;
                        closeMessage = false;
                        MsmqDiagnostics.TransferFromTransport(message);
                    }
                    return message;
                }
                finally
                {
                    if (closeMessage)
                        message.Close();
                }
            }
        }

        static object DeserializeForIntegration(MsmqIntegrationChannelListener listener, Stream bodyStream, MsmqIntegrationMessageProperty property, long lookupId)
        {
            MsmqMessageSerializationFormat serializationFormat = (listener.ReceiveParameters as MsmqIntegrationReceiveParameters).SerializationFormat;

            switch (serializationFormat)
            {
                case MsmqMessageSerializationFormat.Xml:
                    return XmlDeserializeForIntegration(listener, bodyStream, lookupId);

                case MsmqMessageSerializationFormat.Binary:
                    return BinaryFormatter.Deserialize(bodyStream);

                case MsmqMessageSerializationFormat.ActiveX:
                    int bodyType = property.BodyType.Value;
                    return ActiveXSerializer.Deserialize(bodyStream as MemoryStream, bodyType);

                case MsmqMessageSerializationFormat.ByteArray:
                    return (bodyStream as MemoryStream).ToArray();

                case MsmqMessageSerializationFormat.Stream:
                    return bodyStream;

                default:
                    throw new SerializationException(SR.GetString(SR.MsmqUnsupportedSerializationFormat, serializationFormat));
            }
        }

        static object XmlDeserializeForIntegration(MsmqIntegrationChannelListener listener, Stream stream, long lookupId)
        {
            XmlTextReader reader = new XmlTextReader(stream);
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            reader.DtdProcessing = DtdProcessing.Prohibit;

            try
            {
                foreach (XmlSerializer serializer in listener.XmlSerializerList)
                {
                    if (serializer.CanDeserialize(reader))
                        return serializer.Deserialize(reader);
                }
            }
            catch (InvalidOperationException e)
            {
                // XmlSerializer throws InvalidOperationException on failure of Deserialize.
                // We map it to SerializationException to provide consistent interface
                throw new SerializationException(e.Message);
            }

            throw new SerializationException(SR.GetString(SR.MsmqCannotDeserializeXmlMessage));
        }
    }
}

