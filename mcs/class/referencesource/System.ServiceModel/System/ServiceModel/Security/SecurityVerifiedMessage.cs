//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Security
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    sealed class SecurityVerifiedMessage : DelegatingMessage
    {
        byte[] decryptedBuffer;
        XmlDictionaryReader cachedDecryptedBodyContentReader;
        XmlAttributeHolder[] envelopeAttributes;
        XmlAttributeHolder[] headerAttributes;
        XmlAttributeHolder[] bodyAttributes;
        string envelopePrefix;
        bool bodyDecrypted;
        BodyState state = BodyState.Created;
        string bodyPrefix;
        bool isDecryptedBodyStatusDetermined;
        bool isDecryptedBodyFault;
        bool isDecryptedBodyEmpty;
        XmlDictionaryReader cachedReaderAtSecurityHeader;
        readonly ReceiveSecurityHeader securityHeader;
        XmlBuffer messageBuffer;
        bool canDelegateCreateBufferedCopyToInnerMessage;

        public SecurityVerifiedMessage(Message messageToProcess, ReceiveSecurityHeader securityHeader)
            : base(messageToProcess)
        {
            this.securityHeader = securityHeader;
            if (securityHeader.RequireMessageProtection)
            {
                XmlDictionaryReader messageReader;
                BufferedMessage bufferedMessage = this.InnerMessage as BufferedMessage;
                if (bufferedMessage != null && this.Headers.ContainsOnlyBufferedMessageHeaders)
                {
                    messageReader = bufferedMessage.GetMessageReader();
                }
                else
                {
                    this.messageBuffer = new XmlBuffer(int.MaxValue);
                    XmlDictionaryWriter writer = this.messageBuffer.OpenSection(this.securityHeader.ReaderQuotas);
                    this.InnerMessage.WriteMessage(writer);
                    this.messageBuffer.CloseSection();
                    this.messageBuffer.Close();
                    messageReader = this.messageBuffer.GetReader(0);
                }
                MoveToSecurityHeader(messageReader, securityHeader.HeaderIndex, true);
                this.cachedReaderAtSecurityHeader = messageReader;
                this.state = BodyState.Buffered;
            }
            else
            {
                this.envelopeAttributes = XmlAttributeHolder.emptyArray;
                this.headerAttributes = XmlAttributeHolder.emptyArray;
                this.bodyAttributes = XmlAttributeHolder.emptyArray;
                this.canDelegateCreateBufferedCopyToInnerMessage = true;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                if (this.IsDisposed)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                }
                if (!this.bodyDecrypted)
                {
                    return this.InnerMessage.IsEmpty;
                }
                
                EnsureDecryptedBodyStatusDetermined();

                return this.isDecryptedBodyEmpty;
            }
        }

        public override bool IsFault
        {
            get
            {
                if (this.IsDisposed)
                {
                    // PreSharp Bug: Property get methods should not throw exceptions.
                    #pragma warning suppress 56503
                    throw TraceUtility.ThrowHelperError(CreateMessageDisposedException(), this);
                }
                if (!this.bodyDecrypted)
                {
                    return this.InnerMessage.IsFault;
                }

                EnsureDecryptedBodyStatusDetermined();

                return this.isDecryptedBodyFault;
            }
        }

        internal byte[] PrimarySignatureValue
        {
            get { return this.securityHeader.PrimarySignatureValue; }
        }

        internal ReceiveSecurityHeader ReceivedSecurityHeader
        {
            get { return this.securityHeader; }
        }

        Exception CreateBadStateException(string operation)
        {
            return new InvalidOperationException(SR.GetString(SR.MessageBodyOperationNotValidInBodyState,
                operation, this.state));
        }

        public XmlDictionaryReader CreateFullBodyReader()
        {
            switch (this.state)
            {
                case BodyState.Buffered:
                    return CreateFullBodyReaderFromBufferedState();
                case BodyState.Decrypted:
                    return CreateFullBodyReaderFromDecryptedState();
                default:
                    throw TraceUtility.ThrowHelperError(CreateBadStateException("CreateFullBodyReader"), this);
            }
        }

        XmlDictionaryReader CreateFullBodyReaderFromBufferedState()
        {
            if (this.messageBuffer != null)
            {
                XmlDictionaryReader reader = this.messageBuffer.GetReader(0);
                MoveToBody(reader);
                return reader;
            }
            else
            {
                return ((BufferedMessage) this.InnerMessage).GetBufferedReaderAtBody();
            }
        }

        XmlDictionaryReader CreateFullBodyReaderFromDecryptedState()
        {
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(this.decryptedBuffer, 0, this.decryptedBuffer.Length, this.securityHeader.ReaderQuotas);
            MoveToBody(reader);
            return reader;
        }

        void EnsureDecryptedBodyStatusDetermined()
        {
            if (!this.isDecryptedBodyStatusDetermined)
            {
                XmlDictionaryReader reader = CreateFullBodyReader();
                if (Message.ReadStartBody(reader, this.InnerMessage.Version.Envelope, out this.isDecryptedBodyFault, out this.isDecryptedBodyEmpty))
                {
                    this.cachedDecryptedBodyContentReader = reader;
                }
                else
                {
                    reader.Close();
                }
                this.isDecryptedBodyStatusDetermined = true;
            }
        }

        public XmlAttributeHolder[] GetEnvelopeAttributes()
        {
            return this.envelopeAttributes;
        }

        public XmlAttributeHolder[] GetHeaderAttributes()
        {
            return this.headerAttributes;
        }

        XmlDictionaryReader GetReaderAtEnvelope()
        {
            if (this.messageBuffer != null)
            {
                return this.messageBuffer.GetReader(0);
            }
            else
            {
                return ((BufferedMessage) this.InnerMessage).GetMessageReader();
            }
        }

        public XmlDictionaryReader GetReaderAtFirstHeader()
        {
            XmlDictionaryReader reader = GetReaderAtEnvelope();
            MoveToHeaderBlock(reader, false);
            reader.ReadStartElement();
            return reader;
        }

        public XmlDictionaryReader GetReaderAtSecurityHeader()
        {
            if (this.cachedReaderAtSecurityHeader != null)
            {
                XmlDictionaryReader result = this.cachedReaderAtSecurityHeader;
                this.cachedReaderAtSecurityHeader = null;
                return result;
            }
            return this.Headers.GetReaderAtHeader(this.securityHeader.HeaderIndex);
        }

        void MoveToBody(XmlDictionaryReader reader)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            reader.ReadStartElement();
            if (reader.IsStartElement(XD.MessageDictionary.Header, this.Version.Envelope.DictionaryNamespace))
            {
                reader.Skip();
            }
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
        }

        void MoveToHeaderBlock(XmlDictionaryReader reader, bool captureAttributes)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            if (captureAttributes)
            {
                this.envelopePrefix = reader.Prefix;
                this.envelopeAttributes = XmlAttributeHolder.ReadAttributes(reader);
            }
            reader.ReadStartElement();
            reader.MoveToStartElement(XD.MessageDictionary.Header, this.Version.Envelope.DictionaryNamespace);
            if (captureAttributes)
            {
                this.headerAttributes = XmlAttributeHolder.ReadAttributes(reader);
            }
        }

        void MoveToSecurityHeader(XmlDictionaryReader reader, int headerIndex, bool captureAttributes)
        {
            MoveToHeaderBlock(reader, captureAttributes);
            reader.ReadStartElement();
            while (true)
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.MoveToContent();
                }
                if (headerIndex == 0)
                {
                    break;
                }
                reader.Skip();
                headerIndex--;
            }
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (this.state == BodyState.Created)
            {
                base.OnBodyToString(writer);
            }
            else
            {
                OnWriteBodyContents(writer);
            }
        }

        protected override void OnClose()
        {
            
            if (this.cachedDecryptedBodyContentReader != null)
            {
                try
                {
                    this.cachedDecryptedBodyContentReader.Close();
                }
                catch (System.IO.IOException exception)
                {
                    //
                    // We only want to catch and log the I/O exception here 
                    // assuming reader only throw those exceptions  
                    //
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
                finally 
                {
                    this.cachedDecryptedBodyContentReader = null;
                }
            }

            if (this.cachedReaderAtSecurityHeader != null)
            {
                try
                {
                    this.cachedReaderAtSecurityHeader.Close();
                }
                catch (System.IO.IOException exception)
                {
                    //
                    // We only want to catch and log the I/O exception here 
                    // assuming reader only throw those exceptions  
                    //
                    DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
                finally 
                {
                    this.cachedReaderAtSecurityHeader = null;
                }
            }

            this.messageBuffer = null;
            this.decryptedBuffer = null;
            this.state = BodyState.Disposed;
            this.InnerMessage.Close();  
        }

        protected override XmlDictionaryReader OnGetReaderAtBodyContents()
        {
            if (this.state == BodyState.Created)
            {
                return this.InnerMessage.GetReaderAtBodyContents();
            }
            if (this.bodyDecrypted)
            {
                EnsureDecryptedBodyStatusDetermined();
            }
            if (this.cachedDecryptedBodyContentReader != null)
            {
                XmlDictionaryReader result = this.cachedDecryptedBodyContentReader;
                this.cachedDecryptedBodyContentReader = null;
                return result;
            }
            else
            {
                XmlDictionaryReader reader = CreateFullBodyReader();
                reader.ReadStartElement();
                reader.MoveToContent();
                return reader;
            }
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            if (this.canDelegateCreateBufferedCopyToInnerMessage && this.InnerMessage is BufferedMessage)
            {
                return this.InnerMessage.CreateBufferedCopy(maxBufferSize);
            }
            else
            {
                return base.OnCreateBufferedCopy(maxBufferSize);
            }
        }

        internal void OnMessageProtectionPassComplete(bool atLeastOneHeaderOrBodyEncrypted)
        {
            this.canDelegateCreateBufferedCopyToInnerMessage = !atLeastOneHeaderOrBodyEncrypted;
        }

        internal void OnUnencryptedPart(string name, string ns)
        {
            if (ns == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.RequiredMessagePartNotEncrypted, name)), this);
            }
            else
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.RequiredMessagePartNotEncryptedNs, name, ns)), this);
            }
        }

        internal void OnUnsignedPart(string name, string ns)
        {
            if (ns == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.RequiredMessagePartNotSigned, name)), this);
            }
            else
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.RequiredMessagePartNotSignedNs, name, ns)), this);
            }
        }

        protected override void OnWriteStartBody(XmlDictionaryWriter writer)
        {
            if (this.state == BodyState.Created)
            {
                this.InnerMessage.WriteStartBody(writer);
                return;
            }

            XmlDictionaryReader reader = CreateFullBodyReader();
            reader.MoveToContent();
            writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
            writer.WriteAttributes(reader, false);
            reader.Close();
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (this.state == BodyState.Created)
            {
                this.InnerMessage.WriteBodyContents(writer);
                return;
            }

            XmlDictionaryReader reader = CreateFullBodyReader();
            reader.ReadStartElement();
            while (reader.NodeType != XmlNodeType.EndElement)
                writer.WriteNode(reader, false);
            reader.ReadEndElement();
            reader.Close();
        }

        public void SetBodyPrefixAndAttributes(XmlDictionaryReader bodyReader)
        {
            this.bodyPrefix = bodyReader.Prefix;
            this.bodyAttributes = XmlAttributeHolder.ReadAttributes(bodyReader);
        }

        public void SetDecryptedBody(byte[] decryptedBodyContent)
        {
            if (this.state != BodyState.Buffered)
            {
                throw TraceUtility.ThrowHelperError(CreateBadStateException("SetDecryptedBody"), this);
            }

            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);

            writer.WriteStartElement(this.envelopePrefix, XD.MessageDictionary.Envelope, this.Version.Envelope.DictionaryNamespace);
            XmlAttributeHolder.WriteAttributes(this.envelopeAttributes, writer);

            writer.WriteStartElement(this.bodyPrefix, XD.MessageDictionary.Body, this.Version.Envelope.DictionaryNamespace);
            XmlAttributeHolder.WriteAttributes(this.bodyAttributes, writer);
            writer.WriteString(" "); // ensure non-empty element
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();

            this.decryptedBuffer = ContextImportHelper.SpliceBuffers(decryptedBodyContent, stream.GetBuffer(), (int) stream.Length, 2);

            this.bodyDecrypted = true;
            this.state = BodyState.Decrypted;
        }

        enum BodyState
        {
            Created,
            Buffered,
            Decrypted,
            Disposed,
        }
    }

    // Adding wrapping tags using a writer is a temporary feature to
    // support interop with a partner.  Eventually, the serialization
    // team will add a feature to XmlUTF8TextReader to directly
    // support the addition of outer namespaces before creating a
    // Reader.  This roundabout way of supporting context-sensitive
    // decryption can then be removed.
    static class ContextImportHelper
    {
        internal static XmlDictionaryReader CreateSplicedReader(byte[] decryptedBuffer,
            XmlAttributeHolder[] outerContext1, XmlAttributeHolder[] outerContext2, XmlAttributeHolder[] outerContext3, XmlDictionaryReaderQuotas quotas)
        {
            const string wrapper1 = "x";
            const string wrapper2 = "y";
            const string wrapper3 = "z";
            const int wrappingDepth = 3;

            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            writer.WriteStartElement(wrapper1);
            WriteNamespaceDeclarations(outerContext1, writer);
            writer.WriteStartElement(wrapper2);
            WriteNamespaceDeclarations(outerContext2, writer);
            writer.WriteStartElement(wrapper3);
            WriteNamespaceDeclarations(outerContext3, writer);
            writer.WriteString(" "); // ensure non-empty element
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();

            byte[] splicedBuffer = SpliceBuffers(decryptedBuffer, stream.GetBuffer(), (int) stream.Length, wrappingDepth);
            XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(splicedBuffer, quotas);
            reader.ReadStartElement(wrapper1);
            reader.ReadStartElement(wrapper2);
            reader.ReadStartElement(wrapper3);
            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }
            return reader;
        }

        internal static string GetPrefixIfNamespaceDeclaration(string prefix, string localName)
        {
            if (prefix == "xmlns")
            {
                return localName;
            }
            if (prefix.Length == 0 && localName == "xmlns")
            {
                return string.Empty;
            }
            return null;
        }

        static bool IsNamespaceDeclaration(string prefix, string localName)
        {
            return GetPrefixIfNamespaceDeclaration(prefix, localName) != null;
        }

        internal static byte[] SpliceBuffers(byte[] middle, byte[] wrapper, int wrapperLength, int wrappingDepth)
        {
            const byte openChar = (byte) '<';
            int openCharsFound = 0;
            int openCharIndex;
            for (openCharIndex = wrapperLength - 1; openCharIndex >= 0; openCharIndex--)
            {
                if (wrapper[openCharIndex] == openChar)
                {
                    openCharsFound++;
                    if (openCharsFound == wrappingDepth)
                    {
                        break;
                    }
                }
            }

            Fx.Assert(openCharIndex > 0, "");

            byte[] splicedBuffer = DiagnosticUtility.Utility.AllocateByteArray(checked(middle.Length + wrapperLength - 1));
            int offset = 0;
            int count = openCharIndex - 1;
            Buffer.BlockCopy(wrapper, 0, splicedBuffer, offset, count);
            offset += count;
            count = middle.Length;
            Buffer.BlockCopy(middle, 0, splicedBuffer, offset, count);
            offset += count;
            count = wrapperLength - openCharIndex;
            Buffer.BlockCopy(wrapper, openCharIndex, splicedBuffer, offset, count);

            return splicedBuffer;
        }

        static void WriteNamespaceDeclarations(XmlAttributeHolder[] attributes, XmlWriter writer)
        {
            if (attributes != null)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    XmlAttributeHolder a = attributes[i];
                    if (IsNamespaceDeclaration(a.Prefix, a.LocalName))
                    {
                        a.WriteTo(writer);
                    }
                }
            }
        }
    }
}
