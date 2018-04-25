//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Xml;

    sealed class MessageLogTraceRecord : TraceRecord
    {
        internal const string AddressingElementName = "Addressing";
        internal const string BodyElementName = "Body";
        internal const string HttpRequestMessagePropertyElementName = "HttpRequest";
        internal const string HttpResponseMessagePropertyElementName = "HttpResponse";
        internal const string NamespaceUri = "http://schemas.microsoft.com/2004/06/ServiceModel/Management/MessageTrace";
        internal const string NamespacePrefix = "";
        internal const string MessageHeaderElementName = "Header";
        internal const string MessageHeadersElementName = "MessageHeaders";
        internal const string MessageLogTraceRecordElementName = "MessageLogTraceRecord";
        internal const string MethodElementName = "Method";
        internal const string QueryStringElementName = "QueryString";
        internal const string StatusCodeElementName = "StatusCode";
        internal const string StatusDescriptionElementName = "StatusDescription";
        internal const string TraceTimeAttributeName = "Time";
        internal const string TypeElementName = "Type";
        internal const string WebHeadersElementName = "WebHeaders";


        Message message;
        XmlReader reader;
        string messageString;
        DateTime timestamp;
        bool logMessageBody = true;
        MessageLoggingSource source;
        Type type;

        MessageLogTraceRecord(MessageLoggingSource source)
        {
            this.source = source;
            this.timestamp = DateTime.Now;
        }

        internal MessageLogTraceRecord(ArraySegment<byte> buffer, MessageLoggingSource source)
            : this(source)
        {
            this.type = null;
            this.messageString = System.Text.Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        }

        internal MessageLogTraceRecord(string message, MessageLoggingSource source)
            : this(source)
        {
            this.type = null;
            this.messageString = message;
        }

        internal MessageLogTraceRecord(Stream stream, MessageLoggingSource source)
            : this(source)
        {
            this.type = null;

            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            StreamReader streamReader = new StreamReader(stream);

            int chunkSize = 4096;
            char[] buffer = DiagnosticUtility.Utility.AllocateCharArray(chunkSize);
            int count = MessageLogger.MaxMessageSize;

            if (-1 == count)
            {
                count = 4096; //Can't buffer potentially unbounded stream, let's get 4K hoping it will help
            }

            while (count > 0)
            {
                int charsRead = streamReader.Read(buffer, 0, chunkSize);
                if (charsRead == 0)
                {
                    break;
                }
                int charsToAppend = count < charsRead ? count : charsRead;
                stringBuilder.Append(buffer, 0, charsToAppend);
                count -= charsRead;
            }

            streamReader.Close();

            this.messageString = stringBuilder.ToString(); 
        }

        internal MessageLogTraceRecord(ref Message message, XmlReader reader, MessageLoggingSource source, bool logMessageBody)
            : this(source)
        {
            Fx.Assert(message != null, "");

            MessageBuffer buffer = null;
            
            try
            {
                this.logMessageBody = logMessageBody;
                this.message = message;
                this.reader = reader;
                this.type = message.GetType();
            }
            finally
            {
                if (buffer != null)
                {
                    buffer.Close();
                }
            }
        }

        public Message Message
        {
            get { return this.message; }
        }

        public MessageLoggingSource MessageLoggingSource
        {
            get { return this.source; }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            writer.WriteStartElement(MessageLogTraceRecord.NamespacePrefix, MessageLogTraceRecord.MessageLogTraceRecordElementName, MessageLogTraceRecord.NamespaceUri); // <MessageLogTraceRecord>
            writer.WriteAttributeString(MessageLogTraceRecord.TraceTimeAttributeName, this.timestamp.ToString("o", CultureInfo.InvariantCulture));
            writer.WriteAttributeString(DiagnosticStrings.SourceTag, this.source.ToString());
            
            if (null != this.type)
            {
                Fx.Assert(this.message != null, "");

                XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateDictionaryWriter(writer);
                dictionaryWriter.WriteAttributeString(MessageLogTraceRecord.TypeElementName, this.type.ToString());
                
#if DEBUG
                MessageProperties properties = this.message.Properties;
                dictionaryWriter.WriteStartElement("Properties");
                foreach (string key in properties.Keys)
                {
                    dictionaryWriter.WriteElementString(key, properties[key].ToString());
                }
                dictionaryWriter.WriteEndElement(); // </Properties>
#endif
                WriteAddressingProperties(dictionaryWriter);
                WriteHttpProperties(dictionaryWriter);

                if (null != this.reader) //TransportSend case: Message may miss some security data, so we use XmlReader created from serialized message
                {
                    this.reader.MoveToContent();
                }
                if (this.logMessageBody)
                {
                    if (null != this.reader)
                    {
                        dictionaryWriter.WriteNode(this.reader, true);
                    }
                    else
                    {
                        bool hasAtLeastOneItemInsideSecurityHeaderEncrypted = false;

                        if (this.message is SecurityVerifiedMessage)
                        {
                            SecurityVerifiedMessage verifiedMessage = this.message as SecurityVerifiedMessage;
                            ReceiveSecurityHeader receivedHeader = verifiedMessage.ReceivedSecurityHeader;
                            hasAtLeastOneItemInsideSecurityHeaderEncrypted = receivedHeader.HasAtLeastOneItemInsideSecurityHeaderEncrypted;
                        }

                        if (!hasAtLeastOneItemInsideSecurityHeaderEncrypted)
                        {
                            this.message.ToString(dictionaryWriter);
                        }
                        else
                        {
                            if (this.message.Version.Envelope != EnvelopeVersion.None)
                            {
                                dictionaryWriter.WriteStartElement(XD.MessageDictionary.Prefix.Value, XD.MessageDictionary.Envelope, this.message.Version.Envelope.DictionaryNamespace);
                                WriteHeader(dictionaryWriter);
                                this.message.WriteStartBody(writer);
                            }

                            this.message.BodyToString(dictionaryWriter);

                            if (this.message.Version.Envelope != EnvelopeVersion.None)
                            {
                                writer.WriteEndElement(); // </Body>
                                dictionaryWriter.WriteEndElement(); // </Envelope>
                            }
                        }
                    }
                }
                else if (this.message.Version.Envelope != EnvelopeVersion.None) //No headers for EnvelopeVersion.None
                {
                    if (null != this.reader)
                    {
                        dictionaryWriter.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                        this.reader.Read();
                        if (0 == String.CompareOrdinal(reader.LocalName, "Header"))
                        {
                            dictionaryWriter.WriteNode(this.reader, true);
                        }
                        dictionaryWriter.WriteEndElement();
                    }
                    else
                    {
                        dictionaryWriter.WriteStartElement(XD.MessageDictionary.Prefix.Value, XD.MessageDictionary.Envelope, this.message.Version.Envelope.DictionaryNamespace);
                        WriteHeader(dictionaryWriter);
                        dictionaryWriter.WriteEndElement(); // </Envelope>
                    }
                }
                if (null != this.reader)
                {
                    this.reader.Close();
                    this.reader = null;
                }
            }
            else
            {
                writer.WriteCData(this.messageString);
            }
            writer.WriteEndElement(); // </MessageLogTraceRecord>
        }

        void WriteHeader(XmlDictionaryWriter dictionaryWriter)
        {
            dictionaryWriter.WriteStartElement(XD.MessageDictionary.Prefix.Value, XD.MessageDictionary.Header, this.message.Version.Envelope.DictionaryNamespace);
            MessageHeaders headers = this.message.Headers;
            Security.ReceiveSecurityHeader receivedHeader = null;

            if (this.message is SecurityVerifiedMessage)
            {
                SecurityVerifiedMessage verifiedMessage = this.message as SecurityVerifiedMessage;
                receivedHeader = verifiedMessage.ReceivedSecurityHeader;
            }

            for (int i = 0; i < headers.Count; ++i)
            {
                if (receivedHeader != null && receivedHeader.HasAtLeastOneItemInsideSecurityHeaderEncrypted && receivedHeader.HeaderIndex == i)
                {
                    //
                    // if this is the security header and we found at least one item 
                    // was encrypted inside the security header
                    //
                    receivedHeader.WriteStartHeader(dictionaryWriter, headers.MessageVersion);
                    receivedHeader.WriteHeaderContents(dictionaryWriter, headers.MessageVersion);
                    dictionaryWriter.WriteEndElement();
                }
                else
                {
                    headers.WriteHeader(i, dictionaryWriter);
                }
            }
            dictionaryWriter.WriteEndElement(); // </Headers>
        }


        void WriteAddressingProperties(XmlWriter dictionaryWriter)
        {
            Fx.Assert(this.message != null, "");
            object property;
            if (this.message.Properties.TryGetValue(AddressingProperty.Name, out property))
            {
                AddressingProperty addressingProperty = (AddressingProperty)property;

                dictionaryWriter.WriteStartElement(MessageLogTraceRecord.AddressingElementName);

                dictionaryWriter.WriteElementString(AddressingStrings.Action, addressingProperty.Action);
                if (null != addressingProperty.ReplyTo)
                {
                    dictionaryWriter.WriteElementString(AddressingStrings.ReplyTo, addressingProperty.ReplyTo.ToString());
                }
                if (null != addressingProperty.To)
                {
                    dictionaryWriter.WriteElementString(AddressingStrings.To, addressingProperty.To.AbsoluteUri);
                }
                if (null != addressingProperty.MessageId)
                {
                    dictionaryWriter.WriteElementString(AddressingStrings.MessageId, addressingProperty.MessageId.ToString());
                }

                dictionaryWriter.WriteEndElement(); // Addressing

                message.Properties.Remove(AddressingProperty.Name);
            }
        }

        void WriteHttpProperties(XmlWriter dictionaryWriter)
        {
            Fx.Assert(this.message != null, "");
            object property;
            if (this.message.Properties.TryGetValue(HttpResponseMessageProperty.Name, out property))
            {
                HttpResponseMessageProperty responseProperty = (HttpResponseMessageProperty)property;

                dictionaryWriter.WriteStartElement(MessageLogTraceRecord.HttpResponseMessagePropertyElementName);

                dictionaryWriter.WriteElementString(MessageLogTraceRecord.StatusCodeElementName, responseProperty.StatusCode.ToString());
                if (responseProperty.StatusDescription != null)
                {
                    dictionaryWriter.WriteElementString(MessageLogTraceRecord.StatusDescriptionElementName, responseProperty.StatusDescription);
                }

                dictionaryWriter.WriteStartElement(MessageLogTraceRecord.WebHeadersElementName);
                WebHeaderCollection responseHeaders = responseProperty.Headers;
                for (int i = 0; i < responseHeaders.Count; i++)
                {
                    string name = responseHeaders.Keys[i];
                    string value = responseHeaders[i];
                    dictionaryWriter.WriteElementString(name, value);
                }
                dictionaryWriter.WriteEndElement(); // 

                dictionaryWriter.WriteEndElement(); // </HttpResponseMessageProperty>
            }

            if (this.message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out property))
            {
                HttpRequestMessageProperty requestProperty = (HttpRequestMessageProperty)property;

                dictionaryWriter.WriteStartElement(MessageLogTraceRecord.HttpRequestMessagePropertyElementName);

                dictionaryWriter.WriteElementString(MessageLogTraceRecord.MethodElementName, requestProperty.Method);
                dictionaryWriter.WriteElementString(MessageLogTraceRecord.QueryStringElementName, requestProperty.QueryString);

                dictionaryWriter.WriteStartElement(MessageLogTraceRecord.WebHeadersElementName);
                WebHeaderCollection responseHeaders = requestProperty.Headers;
                for (int i = 0; i < responseHeaders.Count; i++)
                {
                    string name = responseHeaders.Keys[i];
                    string value = responseHeaders[i];
                    dictionaryWriter.WriteElementString(name, value);
                }
                dictionaryWriter.WriteEndElement(); // 

                dictionaryWriter.WriteEndElement(); // </HttpResponseMessageProperty>
            }
        }
    }
}
