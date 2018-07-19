//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Security;
    using System.Security.Permissions;

    public interface IXmlMtomReaderInitializer
    {
        void SetInput(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose);
        void SetInput(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose);
    }

    class XmlMtomReader : XmlDictionaryReader, IXmlLineInfo, IXmlMtomReaderInitializer
    {
        Encoding[] encodings;
        XmlDictionaryReader xmlReader;
        XmlDictionaryReader infosetReader;
        MimeReader mimeReader;
        Dictionary<string, MimePart> mimeParts;
        OnXmlDictionaryReaderClose onClose;
        bool readingBinaryElement;
        int maxBufferSize;
        int bufferRemaining;
        MimePart part;

        public XmlMtomReader()
        {
        }

        internal static void DecrementBufferQuota(int maxBuffer, ref int remaining, int size)
        {
            if (remaining - size <= 0)
            {
                remaining = 0;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomBufferQuotaExceeded, maxBuffer)));
            }
            else
            {
                remaining -= size;
            }
        }

        void SetReadEncodings(Encoding[] encodings)
        {
            if (encodings == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encodings");

            for (int i = 0; i < encodings.Length; i++)
            {
                if (encodings[i] == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(String.Format(CultureInfo.InvariantCulture, "encodings[{0}]", i));
            }

            this.encodings = new Encoding[encodings.Length];
            encodings.CopyTo(this.encodings, 0);
        }

        void CheckContentType(string contentType)
        {
            if (contentType != null && contentType.Length == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MtomContentTypeInvalid), "contentType"));
        }

        public void SetInput(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            SetInput(new MemoryStream(buffer, offset, count), encodings, contentType, quotas, maxBufferSize, onClose);
        }

        public void SetInput(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            SetReadEncodings(encodings);
            CheckContentType(contentType);
            Initialize(stream, contentType, quotas, maxBufferSize);
            this.onClose = onClose;
        }

        void Initialize(Stream stream, string contentType, XmlDictionaryReaderQuotas quotas, int maxBufferSize)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");

            this.maxBufferSize = maxBufferSize;
            this.bufferRemaining = maxBufferSize;

            string boundary, start, startInfo;

            if (contentType == null)
            {
                MimeMessageReader messageReader = new MimeMessageReader(stream);
                MimeHeaders messageHeaders = messageReader.ReadHeaders(this.maxBufferSize, ref this.bufferRemaining);
                ReadMessageMimeVersionHeader(messageHeaders.MimeVersion);
                ReadMessageContentTypeHeader(messageHeaders.ContentType, out boundary, out start, out startInfo);
                stream = messageReader.GetContentStream();
                if (stream == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageInvalidContent)));
            }
            else
            {
                ReadMessageContentTypeHeader(new ContentTypeHeader(contentType), out boundary, out start, out startInfo);
            }

            this.mimeReader = new MimeReader(stream, boundary);
            this.mimeParts = null;
            this.readingBinaryElement = false;

            MimePart infosetPart = (start == null) ? ReadRootMimePart() : ReadMimePart(GetStartUri(start));
            byte[] infosetBytes = infosetPart.GetBuffer(this.maxBufferSize, ref this.bufferRemaining);
            int infosetByteCount = (int)infosetPart.Length;

            Encoding encoding = ReadRootContentTypeHeader(infosetPart.Headers.ContentType, this.encodings, startInfo);
            CheckContentTransferEncodingOnRoot(infosetPart.Headers.ContentTransferEncoding);

            IXmlTextReaderInitializer initializer = xmlReader as IXmlTextReaderInitializer;

            if (initializer != null)
                initializer.SetInput(infosetBytes, 0, infosetByteCount, encoding, quotas, null);
            else
                xmlReader = XmlDictionaryReader.CreateTextReader(infosetBytes, 0, infosetByteCount, encoding, quotas, null);
        }

        public override XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return this.xmlReader.Quotas;
            }
        }

        void ReadMessageMimeVersionHeader(MimeVersionHeader header)
        {
            if (header != null && header.Version != MimeVersionHeader.Default.Version)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageInvalidMimeVersion, header.Version, MimeVersionHeader.Default.Version)));
        }

        void ReadMessageContentTypeHeader(ContentTypeHeader header, out string boundary, out string start, out string startInfo)
        {
            if (header == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageContentTypeNotFound)));

            if (String.Compare(MtomGlobals.MediaType, header.MediaType, StringComparison.OrdinalIgnoreCase) != 0
                || String.Compare(MtomGlobals.MediaSubtype, header.MediaSubtype, StringComparison.OrdinalIgnoreCase) != 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageNotMultipart, MtomGlobals.MediaType, MtomGlobals.MediaSubtype)));

            string type;
            if (!header.Parameters.TryGetValue(MtomGlobals.TypeParam, out type) || MtomGlobals.XopType != type)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageNotApplicationXopXml, MtomGlobals.XopType)));

            if (!header.Parameters.TryGetValue(MtomGlobals.BoundaryParam, out boundary))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageRequiredParamNotSpecified, MtomGlobals.BoundaryParam)));
            if (!MailBnfHelper.IsValidMimeBoundary(boundary))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomBoundaryInvalid, boundary)));

            if (!header.Parameters.TryGetValue(MtomGlobals.StartParam, out start))
                start = null;

            if (!header.Parameters.TryGetValue(MtomGlobals.StartInfoParam, out startInfo))
                startInfo = null;
        }

        Encoding ReadRootContentTypeHeader(ContentTypeHeader header, Encoding[] expectedEncodings, string expectedType)
        {
            if (header == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootContentTypeNotFound)));

            if (String.Compare(MtomGlobals.XopMediaType, header.MediaType, StringComparison.OrdinalIgnoreCase) != 0
                || String.Compare(MtomGlobals.XopMediaSubtype, header.MediaSubtype, StringComparison.OrdinalIgnoreCase) != 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootNotApplicationXopXml, MtomGlobals.XopMediaType, MtomGlobals.XopMediaSubtype)));

            string charset;
            if (!header.Parameters.TryGetValue(MtomGlobals.CharsetParam, out charset)
                || charset == null || charset.Length == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootRequiredParamNotSpecified, MtomGlobals.CharsetParam)));
            Encoding encoding = null;
            for (int i = 0; i < encodings.Length; i++)
            {
                if (String.Compare(charset, expectedEncodings[i].WebName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    encoding = expectedEncodings[i];
                    break;
                }
            }
            if (encoding == null)
            {
                // Check for alternate names
                if (String.Compare(charset, "utf-16LE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int i = 0; i < encodings.Length; i++)
                    {
                        if (String.Compare(expectedEncodings[i].WebName, Encoding.Unicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            encoding = expectedEncodings[i];
                            break;
                        }
                    }
                }
                else if (String.Compare(charset, "utf-16BE", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    for (int i = 0; i < encodings.Length; i++)
                    {
                        if (String.Compare(expectedEncodings[i].WebName, Encoding.BigEndianUnicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            encoding = expectedEncodings[i];
                            break;
                        }
                    }
                }

                if (encoding == null)
                {
                    StringBuilder expectedCharSetStr = new StringBuilder();
                    for (int i = 0; i < encodings.Length; i++)
                    {
                        if (expectedCharSetStr.Length != 0)
                            expectedCharSetStr.Append(" | ");
                        expectedCharSetStr.Append(encodings[i].WebName);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootUnexpectedCharset, charset, expectedCharSetStr.ToString())));
                }
            }

            if (expectedType != null)
            {
                string rootType;
                if (!header.Parameters.TryGetValue(MtomGlobals.TypeParam, out rootType)
                    || rootType == null || rootType.Length == 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootRequiredParamNotSpecified, MtomGlobals.TypeParam)));
                if (rootType != expectedType)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootUnexpectedType, rootType, expectedType)));
            }

            return encoding;
        }

        // 7bit is default encoding in the absence of content-transfer-encoding header 
        void CheckContentTransferEncodingOnRoot(ContentTransferEncodingHeader header)
        {
            if (header != null && header.ContentTransferEncoding == ContentTransferEncoding.Other)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomContentTransferEncodingNotSupported,
                                                                                      header.Value,
                                                                                      ContentTransferEncodingHeader.SevenBit.ContentTransferEncodingValue,
                                                                                      ContentTransferEncodingHeader.EightBit.ContentTransferEncodingValue,
                                                                                      ContentTransferEncodingHeader.Binary.ContentTransferEncodingValue)));
        }

        void CheckContentTransferEncodingOnBinaryPart(ContentTransferEncodingHeader header)
        {
            if (header == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomContentTransferEncodingNotPresent,
                    ContentTransferEncodingHeader.Binary.ContentTransferEncodingValue)));
            else if (header.ContentTransferEncoding != ContentTransferEncoding.Binary)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomInvalidTransferEncodingForMimePart,
                    header.Value, ContentTransferEncodingHeader.Binary.ContentTransferEncodingValue)));
        }

        string GetStartUri(string startUri)
        {
            if (startUri.StartsWith("<", StringComparison.Ordinal))
            {
                if (startUri.EndsWith(">", StringComparison.Ordinal))
                    return startUri;
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomInvalidStartUri, startUri)));
            }
            else
                return String.Format(CultureInfo.InvariantCulture, "<{0}>", startUri);
        }

        public override bool Read()
        {
            bool retVal = xmlReader.Read();

            if (xmlReader.NodeType == XmlNodeType.Element)
            {
                XopIncludeReader binaryDataReader = null;
                if (xmlReader.IsStartElement(MtomGlobals.XopIncludeLocalName, MtomGlobals.XopIncludeNamespace))
                {
                    string uri = null;
                    while (xmlReader.MoveToNextAttribute())
                    {
                        if (xmlReader.LocalName == MtomGlobals.XopIncludeHrefLocalName && xmlReader.NamespaceURI == MtomGlobals.XopIncludeHrefNamespace)
                            uri = xmlReader.Value;
                        else if (xmlReader.NamespaceURI == MtomGlobals.XopIncludeNamespace)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomXopIncludeInvalidXopAttributes, xmlReader.LocalName, MtomGlobals.XopIncludeNamespace)));
                    }
                    if (uri == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomXopIncludeHrefNotSpecified, MtomGlobals.XopIncludeHrefLocalName)));

                    MimePart mimePart = ReadMimePart(uri);

                    CheckContentTransferEncodingOnBinaryPart(mimePart.Headers.ContentTransferEncoding);

                    this.part = mimePart;
                    binaryDataReader = new XopIncludeReader(mimePart, xmlReader);
                    binaryDataReader.Read();

                    xmlReader.MoveToElement();
                    if (xmlReader.IsEmptyElement)
                    {
                        xmlReader.Read();
                    }
                    else
                    {
                        int xopDepth = xmlReader.Depth;
                        xmlReader.ReadStartElement();

                        while (xmlReader.Depth > xopDepth)
                        {
                            if (xmlReader.IsStartElement() && xmlReader.NamespaceURI == MtomGlobals.XopIncludeNamespace)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomXopIncludeInvalidXopElement, xmlReader.LocalName, MtomGlobals.XopIncludeNamespace)));

                            xmlReader.Skip();
                        }

                        xmlReader.ReadEndElement();
                    }
                }

                if (binaryDataReader != null)
                {
                    this.xmlReader.MoveToContent();
                    this.infosetReader = this.xmlReader;
                    this.xmlReader = binaryDataReader;
                    binaryDataReader = null;
                }
            }

            if (xmlReader.ReadState == ReadState.EndOfFile && infosetReader != null)
            {
                // Read past the containing EndElement if necessary
                if (!retVal)
                    retVal = infosetReader.Read();

                this.part.Release(this.maxBufferSize, ref this.bufferRemaining);
                xmlReader = infosetReader;
                infosetReader = null;
            }

            return retVal;
        }

        MimePart ReadMimePart(string uri)
        {
            MimePart part = null;

            if (uri == null || uri.Length == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomInvalidEmptyURI)));

            string contentID = null;
            if (uri.StartsWith(MimeGlobals.ContentIDScheme, StringComparison.Ordinal))
                contentID = String.Format(CultureInfo.InvariantCulture, "<{0}>", Uri.UnescapeDataString(uri.Substring(MimeGlobals.ContentIDScheme.Length)));
            else if (uri.StartsWith("<", StringComparison.Ordinal))
                contentID = uri;

            if (contentID == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomInvalidCIDUri, uri)));

            if (mimeParts != null && mimeParts.TryGetValue(contentID, out part))
            {
                if (part.ReferencedFromInfoset)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMimePartReferencedMoreThanOnce, contentID)));
            }
            else
            {
                int maxMimeParts = AppSettings.MaxMimeParts;
                while (part == null && mimeReader.ReadNextPart())
                {
                    MimeHeaders headers = mimeReader.ReadHeaders(this.maxBufferSize, ref this.bufferRemaining);
                    Stream contentStream = mimeReader.GetContentStream();
                    if (contentStream == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageInvalidContentInMimePart)));

                    ContentIDHeader contentIDHeader = (headers == null) ? null : headers.ContentID;
                    if (contentIDHeader == null || contentIDHeader.Value == null)
                    {
                        // Skip content if Content-ID header is not present
                        int size = 256;
                        byte[] bytes = new byte[size];

                        int read = 0;
                        do
                        {
                            read = contentStream.Read(bytes, 0, size);
                        }
                        while (read > 0);
                        continue;
                    }

                    string currentContentID = headers.ContentID.Value;
                    MimePart currentPart = new MimePart(contentStream, headers);
                    if (mimeParts == null)
                        mimeParts = new Dictionary<string, MimePart>();

                    mimeParts.Add(currentContentID, currentPart);

                    if (mimeParts.Count > maxMimeParts)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MaxMimePartsExceeded, maxMimeParts, AppSettings.MaxMimePartsAppSettingsString)));

                    if (currentContentID.Equals(contentID))
                        part = currentPart;
                    else
                        currentPart.GetBuffer(this.maxBufferSize, ref this.bufferRemaining);
                }

                if (part == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomPartNotFound, uri)));
            }

            part.ReferencedFromInfoset = true;
            return part;
        }

        MimePart ReadRootMimePart()
        {
            MimePart part = null;

            if (!mimeReader.ReadNextPart())
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomRootPartNotFound)));

            MimeHeaders headers = mimeReader.ReadHeaders(this.maxBufferSize, ref this.bufferRemaining);
            Stream contentStream = mimeReader.GetContentStream();
            if (contentStream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomMessageInvalidContentInMimePart)));
            part = new MimePart(contentStream, headers);

            return part;
        }

        void AdvanceToContentOnElement()
        {
            if (NodeType != XmlNodeType.Attribute)
            {
                MoveToContent();
            }
        }

        public override int AttributeCount
        {
            get
            {
                return xmlReader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return xmlReader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return xmlReader.CanReadBinaryContent;
            }
        }

        public override bool CanReadValueChunk
        {
            get
            {
                return xmlReader.CanReadValueChunk;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return xmlReader.CanResolveEntity;
            }
        }

        public override void Close()
        {
            xmlReader.Close();
            mimeReader.Close();
            OnXmlDictionaryReaderClose onClose = this.onClose;
            this.onClose = null;
            if (onClose != null)
            {
                try
                {
                    onClose(this);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(e);
                }
            }
        }

        public override int Depth
        {
            get
            {
                return xmlReader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return xmlReader.EOF;
            }
        }

        public override string GetAttribute(int index)
        {
            return xmlReader.GetAttribute(index);
        }

        public override string GetAttribute(string name)
        {
            return xmlReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string ns)
        {
            return xmlReader.GetAttribute(name, ns);
        }

        public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString ns)
        {
            return xmlReader.GetAttribute(localName, ns);
        }
#if NO
        public override ArraySegment<byte> GetSubset(bool advance) 
        { 
            return xmlReader.GetSubset(advance); 
        }
#endif
        public override bool HasAttributes
        {
            get
            {
                return xmlReader.HasAttributes;
            }
        }

        public override bool HasValue
        {
            get
            {
                return xmlReader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return xmlReader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return xmlReader.IsEmptyElement;
            }
        }

        public override bool IsLocalName(string localName)
        {
            return xmlReader.IsLocalName(localName);
        }

        public override bool IsLocalName(XmlDictionaryString localName)
        {
            return xmlReader.IsLocalName(localName);
        }

        public override bool IsNamespaceUri(string ns)
        {
            return xmlReader.IsNamespaceUri(ns);
        }

        public override bool IsNamespaceUri(XmlDictionaryString ns)
        {
            return xmlReader.IsNamespaceUri(ns);
        }

        public override bool IsStartElement()
        {
            return xmlReader.IsStartElement();
        }

        public override bool IsStartElement(string localName)
        {
            return xmlReader.IsStartElement(localName);
        }

        public override bool IsStartElement(string localName, string ns)
        {
            return xmlReader.IsStartElement(localName, ns);
        }

        public override bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString ns)
        {
            return xmlReader.IsStartElement(localName, ns);
        }
#if NO
        public override bool IsStartSubsetElement()
        {
            return xmlReader.IsStartSubsetElement();
        }
#endif
        public override string LocalName
        {
            get
            {
                return xmlReader.LocalName;
            }
        }

        public override string LookupNamespace(string ns)
        {
            return xmlReader.LookupNamespace(ns);
        }

        public override void MoveToAttribute(int index)
        {
            xmlReader.MoveToAttribute(index);
        }

        public override bool MoveToAttribute(string name)
        {
            return xmlReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return xmlReader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            return xmlReader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return xmlReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return xmlReader.MoveToNextAttribute();
        }

        public override string Name
        {
            get
            {
                return xmlReader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return xmlReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return xmlReader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return xmlReader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return xmlReader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return xmlReader.QuoteChar;
            }
        }

        public override bool ReadAttributeValue()
        {
            return xmlReader.ReadAttributeValue();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAs(returnType, namespaceResolver);
        }

        public override byte[] ReadContentAsBase64()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsBase64();
        }

        public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadValueAsBase64(buffer, offset, count);
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsBase64(buffer, offset, count);
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
        {
            if (!readingBinaryElement)
            {
                if (IsEmptyElement)
                {
                    Read();
                    return 0;
                }

                ReadStartElement();
                readingBinaryElement = true;
            }

            int i = ReadContentAsBase64(buffer, offset, count);

            if (i == 0)
            {
                ReadEndElement();
                readingBinaryElement = false;
            }

            return i;
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
        {
            if (!readingBinaryElement)
            {
                if (IsEmptyElement)
                {
                    Read();
                    return 0;
                }

                ReadStartElement();
                readingBinaryElement = true;
            }

            int i = ReadContentAsBinHex(buffer, offset, count);

            if (i == 0)
            {
                ReadEndElement();
                readingBinaryElement = false;
            }

            return i;
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsBinHex(buffer, offset, count);
        }

        public override bool ReadContentAsBoolean()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsBoolean();
        }

        public override int ReadContentAsChars(char[] chars, int index, int count)
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsChars(chars, index, count);
        }

        public override DateTime ReadContentAsDateTime()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsDateTime();
        }

        public override decimal ReadContentAsDecimal()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsDecimal();
        }

        public override double ReadContentAsDouble()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsDouble();
        }

        public override int ReadContentAsInt()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsLong();
        }
#if NO
        public override ICollection ReadContentAsList()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsList();
        }
#endif
        public override object ReadContentAsObject()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsObject();
        }

        public override float ReadContentAsFloat()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsFloat();
        }

        public override string ReadContentAsString()
        {
            AdvanceToContentOnElement();
            return xmlReader.ReadContentAsString();
        }

        public override string ReadInnerXml()
        {
            return xmlReader.ReadInnerXml();
        }

        public override string ReadOuterXml()
        {
            return xmlReader.ReadOuterXml();
        }

        public override ReadState ReadState
        {
            get
            {
                if (xmlReader.ReadState != ReadState.Interactive && infosetReader != null)
                    return infosetReader.ReadState;

                return xmlReader.ReadState;
            }
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            return xmlReader.ReadValueChunk(buffer, index, count);
        }

        public override void ResolveEntity()
        {
            xmlReader.ResolveEntity();
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return xmlReader.Settings;
            }
        }

        public override void Skip()
        {
            xmlReader.Skip();
        }

        public override string this[int index]
        {
            get
            {
                return xmlReader[index];
            }
        }

        public override string this[string name]
        {
            get
            {
                return xmlReader[name];
            }
        }

        public override string this[string name, string ns]
        {
            get
            {
                return xmlReader[name, ns];
            }
        }

        public override string Value
        {
            get
            {
                return xmlReader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return xmlReader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return xmlReader.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return xmlReader.XmlSpace;
            }
        }

        public bool HasLineInfo()
        {
            if (xmlReader.ReadState == ReadState.Closed)
                return false;

            IXmlLineInfo lineInfo = xmlReader as IXmlLineInfo;
            if (lineInfo == null)
                return false;
            return lineInfo.HasLineInfo();
        }

        public int LineNumber
        {
            get
            {
                if (xmlReader.ReadState == ReadState.Closed)
                    return 0;

                IXmlLineInfo lineInfo = xmlReader as IXmlLineInfo;
                if (lineInfo == null)
                    return 0;
                return lineInfo.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                if (xmlReader.ReadState == ReadState.Closed)
                    return 0;

                IXmlLineInfo lineInfo = xmlReader as IXmlLineInfo;
                if (lineInfo == null)
                    return 0;
                return lineInfo.LinePosition;
            }
        }

        internal class MimePart
        {
            Stream stream;
            MimeHeaders headers;
            byte[] buffer;
            bool isReferencedFromInfoset;

            internal MimePart(Stream stream, MimeHeaders headers)
            {
                this.stream = stream;
                this.headers = headers;
            }

            internal Stream Stream
            {
                get { return stream; }
            }

            internal MimeHeaders Headers
            {
                get { return headers; }
            }

            internal bool ReferencedFromInfoset
            {
                get { return isReferencedFromInfoset; }
                set { isReferencedFromInfoset = value; }
            }

            internal long Length
            {
                get { return stream.CanSeek ? stream.Length : 0; }
            }

            internal byte[] GetBuffer(int maxBuffer, ref int remaining)
            {
                if (buffer == null)
                {
                    MemoryStream bufferedStream = stream.CanSeek ? new MemoryStream((int)stream.Length) : new MemoryStream();
                    int size = 256;
                    byte[] bytes = new byte[size];

                    int read = 0;

                    do
                    {
                        read = stream.Read(bytes, 0, size);
                        XmlMtomReader.DecrementBufferQuota(maxBuffer, ref remaining, read);
                        if (read > 0)
                            bufferedStream.Write(bytes, 0, read);
                    }
                    while (read > 0);

                    bufferedStream.Seek(0, SeekOrigin.Begin);
                    buffer = bufferedStream.GetBuffer();
                    stream = bufferedStream;
                }
                return buffer;
            }

            internal void Release(int maxBuffer, ref int remaining)
            {
                remaining += (int)this.Length;
                this.headers.Release(ref remaining);
            }
        }

        internal class XopIncludeReader : XmlDictionaryReader, IXmlLineInfo
        {
            int chunkSize = 4096;  // Just a default.  Serves as a max chunk size.
            int bytesRemaining;

            MimePart part;
            ReadState readState;
            XmlDictionaryReader parentReader;
            string stringValue;
            int stringOffset;
            XmlNodeType nodeType;
            MemoryStream binHexStream;
            byte[] valueBuffer;
            int valueOffset;
            int valueCount;
            bool finishedStream;

            public XopIncludeReader(MimePart part, XmlDictionaryReader reader)
            {
                if (part == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("part");
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                this.part = part;
                this.parentReader = reader;
                this.readState = ReadState.Initial;
                this.nodeType = XmlNodeType.None;
                this.chunkSize = Math.Min(reader.Quotas.MaxBytesPerRead, chunkSize);
                this.bytesRemaining = this.chunkSize;
                this.finishedStream = false;
            }

            public override XmlDictionaryReaderQuotas Quotas
            {
                get
                {
                    return this.parentReader.Quotas;
                }
            }

            public override XmlNodeType NodeType
            {
                get
                {
                    return (readState == ReadState.Interactive) ? nodeType : parentReader.NodeType;
                }
            }

            public override bool Read()
            {
                bool retVal = true;
                switch (readState)
                {
                    case ReadState.Initial:
                        readState = ReadState.Interactive;
                        nodeType = XmlNodeType.Text;
                        break;
                    case ReadState.Interactive:
                        if (this.finishedStream || (this.bytesRemaining == this.chunkSize && this.stringValue == null))
                        {
                            readState = ReadState.EndOfFile;
                            nodeType = XmlNodeType.EndElement;
                        }
                        else
                        {
                            this.bytesRemaining = this.chunkSize;
                        }
                        break;
                    case ReadState.EndOfFile:
                        nodeType = XmlNodeType.None;
                        retVal = false;
                        break;
                }
                this.stringValue = null;
                this.binHexStream = null;
                this.valueOffset = 0;
                this.valueCount = 0;
                this.stringOffset = 0;
                CloseStreams();
                return retVal;
            }

            public override int ReadValueAsBase64(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");

                if (offset < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (offset > buffer.Length)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
                if (count < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
                if (count > buffer.Length - offset)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));

                if (this.stringValue != null)
                {
                    count = Math.Min(count, this.valueCount);
                    if (count > 0)
                    {
                        Buffer.BlockCopy(this.valueBuffer, this.valueOffset, buffer, offset, count);
                        this.valueOffset += count;
                        this.valueCount -= count;
                    }
                    return count;
                }

                if (this.bytesRemaining < count)
                    count = this.bytesRemaining;

                int read = 0;
                if (readState == ReadState.Interactive)
                {
                    while (read < count)
                    {
                        int actual = part.Stream.Read(buffer, offset + read, count - read);
                        if (actual == 0)
                        {
                            this.finishedStream = true;
                            break;
                        }
                        read += actual;
                    }
                }
                this.bytesRemaining -= read;
                return read;
            }

            public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");

                if (offset < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (offset > buffer.Length)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
                if (count < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
                if (count > buffer.Length - offset)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));

                if (this.valueCount > 0)
                {
                    count = Math.Min(count, this.valueCount);
                    Buffer.BlockCopy(this.valueBuffer, this.valueOffset, buffer, offset, count);
                    this.valueOffset += count;
                    this.valueCount -= count;
                    return count;
                }

                if (this.chunkSize < count)
                    count = this.chunkSize;

                int read = 0;
                if (readState == ReadState.Interactive)
                {
                    while (read < count)
                    {
                        int actual = part.Stream.Read(buffer, offset + read, count - read);
                        if (actual == 0)
                        {
                            this.finishedStream = true;
                            if (!Read())
                                break;
                        }
                        read += actual;
                    }
                }
                this.bytesRemaining = this.chunkSize;
                return read;
            }

            public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");

                if (offset < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (offset > buffer.Length)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
                if (count < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
                if (count > buffer.Length - offset)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));

                if (this.chunkSize < count)
                    count = this.chunkSize;

                int read = 0;
                int consumed = 0;
                while (read < count)
                {
                    if (binHexStream == null)
                    {
                        try
                        {
                            binHexStream = new MemoryStream(new BinHexEncoding().GetBytes(this.Value));
                        }
                        catch (FormatException e) // Wrap format exceptions from decoding document contents
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(e.Message, e));
                        }
                    }

                    int actual = binHexStream.Read(buffer, offset + read, count - read);
                    if (actual == 0)
                    {
                        this.finishedStream = true;
                        if (!Read())
                            break;

                        consumed = 0;
                    }

                    read += actual;
                    consumed += actual;
                }

                // Trim off the consumed chars
                if (this.stringValue != null && consumed > 0)
                {
                    this.stringValue = this.stringValue.Substring(consumed * 2);
                    this.stringOffset = Math.Max(0, this.stringOffset - consumed * 2);

                    this.bytesRemaining = this.chunkSize;
                }
                return read;
            }

            public override int ReadValueChunk(char[] chars, int offset, int count)
            {
                if (chars == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("chars");

                if (offset < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (offset > chars.Length)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
                if (count < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
                if (count > chars.Length - offset)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));

                if (readState != ReadState.Interactive)
                    return 0;

                // Copy characters from the Value property
                string str = this.Value;
                count = Math.Min(stringValue.Length - stringOffset, count);
                if (count > 0)
                {
                    stringValue.CopyTo(stringOffset, chars, offset, count);
                    stringOffset += count;
                }
                return count;
            }

            public override string Value
            {
                get
                {
                    if (readState != ReadState.Interactive)
                        return String.Empty;

                    if (stringValue == null)
                    {
                        // Compute the bytes to read
                        int byteCount = this.bytesRemaining;
                        byteCount -= byteCount % 3;

                        // Handle trailing bytes
                        if (this.valueCount > 0 && this.valueOffset > 0)
                        {
                            Buffer.BlockCopy(this.valueBuffer, this.valueOffset, this.valueBuffer, 0, this.valueCount);
                            this.valueOffset = 0;
                        }
                        byteCount -= this.valueCount;

                        // Resize buffer if needed
                        if (this.valueBuffer == null)
                        {
                            this.valueBuffer = new byte[byteCount];
                        }
                        else if (this.valueBuffer.Length < byteCount)
                        {
                            Array.Resize(ref this.valueBuffer, byteCount);
                        }
                        byte[] buffer = this.valueBuffer;

                        // Fill up the buffer
                        int offset = 0;
                        int read = 0;
                        while (byteCount > 0)
                        {
                            read = part.Stream.Read(buffer, offset, byteCount);
                            if (read == 0)
                            {
                                this.finishedStream = true;
                                break;
                            }

                            this.bytesRemaining -= read;
                            this.valueCount += read;
                            byteCount -= read;
                            offset += read;
                        }

                        // Convert the bytes
                        this.stringValue = Convert.ToBase64String(buffer, 0, this.valueCount);
                    }
                    return this.stringValue;
                }
            }

            public override string ReadContentAsString()
            {
                int stringContentQuota = this.Quotas.MaxStringContentLength;
                StringBuilder sb = new StringBuilder();
                do
                {
                    string val = this.Value;
                    if (val.Length > stringContentQuota)
                        XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, this.Quotas.MaxStringContentLength);
                    stringContentQuota -= val.Length;
                    sb.Append(val);
                } while (Read());
                return sb.ToString();
            }

            public override int AttributeCount
            {
                get { return 0; }
            }

            public override string BaseURI
            {
                get { return parentReader.BaseURI; }
            }

            public override bool CanReadBinaryContent
            {
                get { return true; }
            }

            public override bool CanReadValueChunk
            {
                get { return true; }
            }

            public override bool CanResolveEntity
            {
                get { return parentReader.CanResolveEntity; }
            }

            public override void Close()
            {
                CloseStreams();
                readState = ReadState.Closed;
            }

            void CloseStreams()
            {
                if (binHexStream != null)
                {
                    binHexStream.Close();
                    binHexStream = null;
                }
            }

            public override int Depth
            {
                get
                {
                    return (readState == ReadState.Interactive) ? parentReader.Depth + 1 : parentReader.Depth;
                }
            }

            public override bool EOF
            {
                get { return readState == ReadState.EndOfFile; }
            }

            public override string GetAttribute(int index)
            {
                return null;
            }

            public override string GetAttribute(string name)
            {
                return null;
            }

            public override string GetAttribute(string name, string ns)
            {
                return null;
            }

            public override string GetAttribute(XmlDictionaryString localName, XmlDictionaryString ns)
            {
                return null;
            }

            public override bool HasAttributes
            {
                get { return false; }
            }

            public override bool HasValue
            {
                get { return readState == ReadState.Interactive; }
            }

            public override bool IsDefault
            {
                get { return false; }
            }

            public override bool IsEmptyElement
            {
                get { return false; }
            }

            public override bool IsLocalName(string localName)
            {
                return false;
            }

            public override bool IsLocalName(XmlDictionaryString localName)
            {
                return false;
            }

            public override bool IsNamespaceUri(string ns)
            {
                return false;
            }

            public override bool IsNamespaceUri(XmlDictionaryString ns)
            {
                return false;
            }

            public override bool IsStartElement()
            {
                return false;
            }

            public override bool IsStartElement(string localName)
            {
                return false;
            }

            public override bool IsStartElement(string localName, string ns)
            {
                return false;
            }

            public override bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString ns)
            {
                return false;
            }
#if NO
            public override bool IsStartSubsetElement()
            {
                return false;
            }
#endif
            public override string LocalName
            {
                get
                {
                    return (readState == ReadState.Interactive) ? String.Empty : parentReader.LocalName;
                }
            }

            public override string LookupNamespace(string ns)
            {
                return parentReader.LookupNamespace(ns);
            }

            public override void MoveToAttribute(int index)
            {
            }

            public override bool MoveToAttribute(string name)
            {
                return false;
            }

            public override bool MoveToAttribute(string name, string ns)
            {
                return false;
            }

            public override bool MoveToElement()
            {
                return false;
            }

            public override bool MoveToFirstAttribute()
            {
                return false;
            }

            public override bool MoveToNextAttribute()
            {
                return false;
            }

            public override string Name
            {
                get
                {
                    return (readState == ReadState.Interactive) ? String.Empty : parentReader.Name;
                }
            }

            public override string NamespaceURI
            {
                get
                {
                    return (readState == ReadState.Interactive) ? String.Empty : parentReader.NamespaceURI;
                }
            }

            public override XmlNameTable NameTable
            {
                get { return parentReader.NameTable; }
            }

            public override string Prefix
            {
                get
                {
                    return (readState == ReadState.Interactive) ? String.Empty : parentReader.Prefix;
                }
            }

            public override char QuoteChar
            {
                get { return parentReader.QuoteChar; }
            }

            public override bool ReadAttributeValue()
            {
                return false;
            }

            public override string ReadInnerXml()
            {
                return ReadContentAsString();
            }

            public override string ReadOuterXml()
            {
                return ReadContentAsString();
            }

            public override ReadState ReadState
            {
                get { return readState; }
            }

            public override void ResolveEntity()
            {
            }

            public override XmlReaderSettings Settings
            {
                get { return parentReader.Settings; }
            }

            public override void Skip()
            {
                Read();
            }

            public override string this[int index]
            {
                get { return null; }
            }

            public override string this[string name]
            {
                get { return null; }
            }

            public override string this[string name, string ns]
            {
                get { return null; }
            }

            public override string XmlLang
            {
                get { return parentReader.XmlLang; }
            }

            public override XmlSpace XmlSpace
            {
                get { return parentReader.XmlSpace; }
            }

            public override Type ValueType
            {
                get
                {
                    return (readState == ReadState.Interactive) ? typeof(byte[]) : parentReader.ValueType;
                }
            }

            bool IXmlLineInfo.HasLineInfo()
            {
                return ((IXmlLineInfo)parentReader).HasLineInfo();
            }

            int IXmlLineInfo.LineNumber
            {
                get
                {
                    return ((IXmlLineInfo)parentReader).LineNumber;
                }
            }

            int IXmlLineInfo.LinePosition
            {
                get
                {
                    return ((IXmlLineInfo)parentReader).LinePosition;
                }
            }
        }
    }

    internal class MimeMessageReader
    {
        static byte[] CRLFCRLF = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        bool getContentStreamCalled;
        MimeHeaderReader mimeHeaderReader;
        DelimittedStreamReader reader;

        public MimeMessageReader(Stream stream)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");

            this.reader = new DelimittedStreamReader(stream);
            this.mimeHeaderReader = new MimeHeaderReader(this.reader.GetNextStream(CRLFCRLF));
        }

        public Stream GetContentStream()
        {
            if (getContentStreamCalled)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeMessageGetContentStreamCalledAlready)));

            mimeHeaderReader.Close();

            Stream s = reader.GetNextStream(null);

            getContentStreamCalled = true;

            return s;
        }

        public MimeHeaders ReadHeaders(int maxBuffer, ref int remaining)
        {
            MimeHeaders headers = new MimeHeaders();
            while (mimeHeaderReader.Read(maxBuffer, ref remaining))
            {
                headers.Add(mimeHeaderReader.Name, mimeHeaderReader.Value, ref remaining);
            }
            return headers;
        }
    }

    internal class MimeReader
    {
        static byte[] CRLFCRLF = new byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };

        byte[] boundaryBytes;
        string content;
        Stream currentStream;
        MimeHeaderReader mimeHeaderReader;
        DelimittedStreamReader reader;
        byte[] scratch = new byte[2];

        public MimeReader(Stream stream, string boundary)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            if (boundary == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("boundary");

            this.reader = new DelimittedStreamReader(stream);
            this.boundaryBytes = MimeWriter.GetBoundaryBytes(boundary);

            // Need to ensure that the content begins with a CRLF, in case the 
            // outer construct has consumed the trailing CRLF
            this.reader.Push(this.boundaryBytes, 0, 2);
        }

        public void Close()
        {
            this.reader.Close();
        }

        /// Gets the content preceding the first part of the MIME multi-part message
        public string Preface
        {
            get
            {
                if (content == null)
                {
                    Stream s = this.reader.GetNextStream(this.boundaryBytes);
                    content = new StreamReader(s, System.Text.Encoding.ASCII, false, 256).ReadToEnd();
                    s.Close();
                    if (content == null)
                        content = string.Empty;
                }
                return content;
            }
        }

        public Stream GetContentStream()
        {
            Fx.Assert(content != null, "");

            mimeHeaderReader.Close();

            return reader.GetNextStream(this.boundaryBytes);
        }

        public bool ReadNextPart()
        {
            string content = Preface;

            if (currentStream != null)
            {
                currentStream.Close();
                currentStream = null;
            }

            Stream stream = reader.GetNextStream(CRLFCRLF);

            if (stream == null)
                return false;

            if (BlockRead(stream, scratch, 0, 2) == 2)
            {
                if (scratch[0] == '\r' && scratch[1] == '\n')
                {
                    if (mimeHeaderReader == null)
                        mimeHeaderReader = new MimeHeaderReader(stream);
                    else
                        mimeHeaderReader.Reset(stream);
                    return true;
                }
                else if (scratch[0] == '-' && scratch[1] == '-')
                {
                    int read = BlockRead(stream, scratch, 0, 2);

                    if (read < 2 || (scratch[0] == '\r' && scratch[1] == '\n'))
                        return false;
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderTruncated)));
        }

        public MimeHeaders ReadHeaders(int maxBuffer, ref int remaining)
        {
            MimeHeaders headers = new MimeHeaders();
            while (mimeHeaderReader.Read(maxBuffer, ref remaining))
            {
                headers.Add(mimeHeaderReader.Name, mimeHeaderReader.Value, ref remaining);
            }
            return headers;
        }

        int BlockRead(Stream stream, byte[] buffer, int offset, int count)
        {
            int read = 0;
            do
            {
                int r = stream.Read(buffer, offset + read, count - read);
                if (r == 0)
                    break;
                read += r;
            } while (read < count);
            return read;
        }

    }

    internal class DelimittedStreamReader
    {
        bool canGetNextStream = true;

        // used for closing the reader, and validating that only one stream can be reading at a time.
        DelimittedReadStream currentStream;

        byte[] delimitter;
        byte[] matchBuffer;
        byte[] scratch;

        BufferedReadStream stream;

        public DelimittedStreamReader(Stream stream)
        {
            this.stream = new BufferedReadStream(stream);
        }

        public void Close()
        {
            this.stream.Close();
        }

        // Closes the current stream.  If the current stream is not the same as the caller, nothing is done.
        void Close(DelimittedReadStream caller)
        {
            if (currentStream == caller)
            {
                if (delimitter == null)
                {
                    stream.Close();
                }
                else
                {
                    if (scratch == null)
                    {
                        scratch = new byte[1024];
                    }
                    while (0 != Read(caller, this.scratch, 0, this.scratch.Length));
                }

                currentStream = null;
            }
        }

        // Gets the next logical stream delimitted by the given sequence.
        public Stream GetNextStream(byte[] delimitter)
        {
            if (currentStream != null)
            {
                currentStream.Close();
                currentStream = null;
            }

            if (!canGetNextStream)
                return null;

            this.delimitter = delimitter;

            canGetNextStream = delimitter != null;

            currentStream = new DelimittedReadStream(this);

            return currentStream;
        }

        enum MatchState
        {
            True,
            False,
            InsufficientData
        }

        MatchState MatchDelimitter(byte[] buffer, int start, int end)
        {
            if (this.delimitter.Length > end - start)
            {
                for (int i = end - start - 1; i >= 1; i--)
                {
                    if (buffer[start + i] != delimitter[i])
                        return MatchState.False;
                }
                return MatchState.InsufficientData;
            }
            for (int i = delimitter.Length - 1; i >= 1; i--)
            {
                if (buffer[start + i] != delimitter[i])
                    return MatchState.False;
            }
            return MatchState.True;
        }

        int ProcessRead(byte[] buffer, int offset, int read)
        {
            // nothing to process if 0 bytes were read
            if (read == 0)
                return read;

            for (int ptr = offset, end = offset + read; ptr < end; ptr++)
            {
                if (buffer[ptr] == delimitter[0])
                {
                    switch (MatchDelimitter(buffer, ptr, end))
                    {
                        case MatchState.True:
                            {
                                int actual = ptr - offset;
                                ptr += this.delimitter.Length;
                                this.stream.Push(buffer, ptr, end - ptr);
                                this.currentStream = null;
                                return actual;
                            }
                        case MatchState.False:
                            break;
                        case MatchState.InsufficientData:
                            {
                                int actual = ptr - offset;
                                if (actual > 0)
                                {
                                    this.stream.Push(buffer, ptr, end - ptr);
                                    return actual;
                                }
                                else
                                {
                                    return -1;
                                }
                            }
                    }
                }
            }
            return read;
        }

        int Read(DelimittedReadStream caller, byte[] buffer, int offset, int count)
        {
            if (this.currentStream != caller)
                return 0;

            int read = this.stream.Read(buffer, offset, count);
            if (read == 0)
            {
                this.canGetNextStream = false;
                this.currentStream = null;
                return read;
            }

            // If delimitter is null, read until the underlying stream returns 0 bytes
            if (this.delimitter == null)
                return read;

            // Scans the read data for the delimitter. If found, the number of bytes read are adjusted
            // to account for the number of bytes up to but not including the delimitter.
            int actual = ProcessRead(buffer, offset, read);

            if (actual < 0)
            {
                if (this.matchBuffer == null || this.matchBuffer.Length < this.delimitter.Length - read)
                    this.matchBuffer = new byte[this.delimitter.Length - read];

                int matched = this.stream.ReadBlock(this.matchBuffer, 0, this.delimitter.Length - read);

                if (MatchRemainder(read, matched))
                {
                    this.currentStream = null;
                    actual = 0;
                }
                else
                {
                    this.stream.Push(this.matchBuffer, 0, matched);

                    int i = 1;
                    for (; i < read; i++)
                    {
                        if (buffer[i] == this.delimitter[0])
                            break;
                    }

                    if (i < read)
                        this.stream.Push(buffer, offset + i, read - i);

                    actual = i;
                }
            }

            return actual;
        }

        bool MatchRemainder(int start, int count)
        {
            if (start + count != this.delimitter.Length)
                return false;

            for (count--; count >= 0; count--)
            {
                if (this.delimitter[start + count] != this.matchBuffer[count])
                    return false;
            }
            return true;
        }

        internal void Push(byte[] buffer, int offset, int count)
        {
            this.stream.Push(buffer, offset, count);
        }

        class DelimittedReadStream : Stream
        {
            DelimittedStreamReader reader;

            public DelimittedReadStream(DelimittedStreamReader reader)
            {
                if (reader == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

                this.reader = reader;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
#pragma warning suppress 56503 // Microsoft, required by the XmlReader
                get { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, this.GetType().FullName))); }
            }

            public override long Position
            {
                get
                {
#pragma warning suppress 56503 // Microsoft, required by the XmlReader
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, this.GetType().FullName)));
                }
                set { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, this.GetType().FullName))); }
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, this.GetType().FullName)));
            }

            public override void Close()
            {
                reader.Close(this);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, this.GetType().FullName)));
            }

            public override void Flush()
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, this.GetType().FullName)));
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");

                if (offset < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
                if (offset > buffer.Length)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
                if (count < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
                if (count > buffer.Length - offset)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));

                return reader.Read(this, buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, this.GetType().FullName)));
            }

            public override void SetLength(long value)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, this.GetType().FullName)));
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, this.GetType().FullName)));
            }
        }

    }

    internal class MimeHeaders
    {
        static class Constants
        {
            public const string ContentTransferEncoding = "content-transfer-encoding";
            public const string ContentID = "content-id";
            public const string ContentType = "content-type";
            public const string MimeVersion = "mime-version";
        }

        Dictionary<string, MimeHeader> headers = new Dictionary<string, MimeHeader>();

        public MimeHeaders()
        {
        }

        public ContentTypeHeader ContentType
        {
            get
            {
                MimeHeader header;
                if (headers.TryGetValue(Constants.ContentType, out header))
                    return header as ContentTypeHeader;
                return null;
            }
        }

        public ContentIDHeader ContentID
        {
            get
            {
                MimeHeader header;
                if (headers.TryGetValue(Constants.ContentID, out header))
                    return header as ContentIDHeader;
                return null;
            }
        }

        public ContentTransferEncodingHeader ContentTransferEncoding
        {
            get
            {
                MimeHeader header;
                if (headers.TryGetValue(Constants.ContentTransferEncoding, out header))
                    return header as ContentTransferEncodingHeader;
                return null;
            }
        }

        public MimeVersionHeader MimeVersion
        {
            get
            {
                MimeHeader header;
                if (headers.TryGetValue(Constants.MimeVersion, out header))
                    return header as MimeVersionHeader;
                return null;
            }
        }

        public void Add(string name, string value, ref int remaining)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");

            if (value == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            switch (name)
            {
                case Constants.ContentType:
                    Add(new ContentTypeHeader(value));
                    break;
                case Constants.ContentID:
                    Add(new ContentIDHeader(name, value));
                    break;
                case Constants.ContentTransferEncoding:
                    Add(new ContentTransferEncodingHeader(value));
                    break;
                case Constants.MimeVersion:
                    Add(new MimeVersionHeader(value));
                    break;

                // Skip any fields that are not recognized
                // Content-description is currently not stored since it is not used
                default:
                    remaining += value.Length * sizeof(char);
                    break;
            }
            remaining += name.Length * sizeof(char);
        }

        public void Add(MimeHeader header)
        {
            if (header == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("header");

            MimeHeader existingHeader;
            if (headers.TryGetValue(header.Name, out existingHeader))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderHeaderAlreadyExists, header.Name)));
            else
                headers.Add(header.Name, header);
        }

        public void Release(ref int remaining)
        {
            foreach (MimeHeader header in this.headers.Values)
            {
                remaining += header.Value.Length * sizeof(char);
            }
        }

    }

    internal class MimeHeader
    {
        string name;
        string value;

        public MimeHeader(string name, string value)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");

            this.name = name;
            this.value = value;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
        }
    }

    internal class ContentTypeHeader : MimeHeader
    {
        public readonly static ContentTypeHeader Default = new ContentTypeHeader("application/octet-stream");

        public ContentTypeHeader(string value)
            : base("content-type", value)
        {
        }

        string mediaType;
        string subType;
        Dictionary<string, string> parameters;

        public string MediaType
        {
            get
            {
                if (this.mediaType == null && Value != null)
                    ParseValue();

                return this.mediaType;
            }
        }

        public string MediaSubtype
        {
            get
            {
                if (this.subType == null && Value != null)
                    ParseValue();

                return this.subType;
            }
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    if (Value != null)
                        ParseValue();
                    else
                        this.parameters = new Dictionary<string, string>();
                }
                return this.parameters;
            }
        }

        void ParseValue()
        {
            if (this.parameters == null)
            {
                int offset = 0;
                this.parameters = new Dictionary<string, string>();
                this.mediaType = MailBnfHelper.ReadToken(Value, ref offset, null);
                if (offset >= Value.Length || Value[offset++] != '/')
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeContentTypeHeaderInvalid)));
                this.subType = MailBnfHelper.ReadToken(Value, ref offset, null);

                while (MailBnfHelper.SkipCFWS(Value, ref offset))
                {
                    if (offset >= Value.Length || Value[offset++] != ';')
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeContentTypeHeaderInvalid)));

                    if (!MailBnfHelper.SkipCFWS(Value, ref offset))
                        break;

                    string paramAttribute = MailBnfHelper.ReadParameterAttribute(Value, ref offset, null);
                    if (paramAttribute == null || offset >= Value.Length || Value[offset++] != '=')
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeContentTypeHeaderInvalid)));
                    string paramValue = MailBnfHelper.ReadParameterValue(Value, ref offset, null);

                    this.parameters.Add(paramAttribute.ToLowerInvariant(), paramValue);
                }

                if (this.parameters.ContainsKey(MtomGlobals.StartInfoParam))
                {
                    // This allows us to maintain back compat with Orcas clients while allowing clients 
                    // following the spec (with action inside start-info) to interop with RFC 2387
                    string startInfo = this.parameters[MtomGlobals.StartInfoParam];

                    // we're only interested in finding the action here - skipping past the content type to the first ; 
                    int startInfoOffset = startInfo.IndexOf(';');
                    if (startInfoOffset > -1)
                    {
                        // keep going through the start-info string until we've reached the end of the stream
                        while (MailBnfHelper.SkipCFWS(startInfo, ref startInfoOffset))
                        {
                            // after having read through an attribute=value pair, we always expect to be at a ;
                            if (startInfo[startInfoOffset] == ';')
                            {
                                startInfoOffset++;
                            }
                            else
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeContentTypeHeaderInvalid)));
                            }
                            string paramAttribute = MailBnfHelper.ReadParameterAttribute(startInfo, ref startInfoOffset, null);
                            if (paramAttribute == null || startInfoOffset >= startInfo.Length || startInfo[startInfoOffset++] != '=')
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeContentTypeHeaderInvalid)));
                            string paramValue = MailBnfHelper.ReadParameterValue(startInfo, ref startInfoOffset, null);

                            if (paramAttribute == MtomGlobals.ActionParam)
                            {
                                this.parameters[MtomGlobals.ActionParam] = paramValue;
                            }
                        }
                    }
                }

            }
        }
    }

    enum ContentTransferEncoding
    {
        SevenBit,
        EightBit,
        Binary,
        Other,
        Unspecified
    }

    internal class ContentTransferEncodingHeader : MimeHeader
    {
        ContentTransferEncoding contentTransferEncoding;
        string contentTransferEncodingValue;

        public readonly static ContentTransferEncodingHeader Binary = new ContentTransferEncodingHeader(ContentTransferEncoding.Binary, "binary");
        public readonly static ContentTransferEncodingHeader EightBit = new ContentTransferEncodingHeader(ContentTransferEncoding.EightBit, "8bit");
        public readonly static ContentTransferEncodingHeader SevenBit = new ContentTransferEncodingHeader(ContentTransferEncoding.SevenBit, "7bit");

        public ContentTransferEncodingHeader(string value)
            : base("content-transfer-encoding", value.ToLowerInvariant())
        {
        }

        public ContentTransferEncodingHeader(ContentTransferEncoding contentTransferEncoding, string value)
            : base("content-transfer-encoding", null)
        {
            this.contentTransferEncoding = contentTransferEncoding;
            this.contentTransferEncodingValue = value;
        }

        public ContentTransferEncoding ContentTransferEncoding
        {
            get
            {
                ParseValue();
                return this.contentTransferEncoding;
            }
        }

        public string ContentTransferEncodingValue
        {
            get
            {
                ParseValue();
                return this.contentTransferEncodingValue;
            }
        }

        void ParseValue()
        {
            if (this.contentTransferEncodingValue == null)
            {
                int offset = 0;
                this.contentTransferEncodingValue = (Value.Length == 0) ? Value : ((Value[0] == '"') ? MailBnfHelper.ReadQuotedString(Value, ref offset, null) : MailBnfHelper.ReadToken(Value, ref offset, null));
                switch (this.contentTransferEncodingValue)
                {
                    case "7bit":
                        this.contentTransferEncoding = ContentTransferEncoding.SevenBit;
                        break;
                    case "8bit":
                        this.contentTransferEncoding = ContentTransferEncoding.EightBit;
                        break;
                    case "binary":
                        this.contentTransferEncoding = ContentTransferEncoding.Binary;
                        break;
                    default:
                        this.contentTransferEncoding = ContentTransferEncoding.Other;
                        break;
                }
            }
        }
    }

    internal class ContentIDHeader : MimeHeader
    {
        public ContentIDHeader(string name, string value)
            : base(name, value)
        {
        }
    }

    internal class MimeVersionHeader : MimeHeader
    {
        public static readonly MimeVersionHeader Default = new MimeVersionHeader("1.0");

        public MimeVersionHeader(string value)
            : base("mime-version", value)
        {
        }

        string version;

        public string Version
        {
            get
            {
                if (this.version == null && Value != null)
                    ParseValue();
                return this.version;
            }
        }

        void ParseValue()
        {
            // shortcut for the most common case.
            if (Value == "1.0")
            {
                this.version = "1.0";
            }
            else
            {
                int offset = 0;

                if (!MailBnfHelper.SkipCFWS(Value, ref offset))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeVersionHeaderInvalid)));

                StringBuilder builder = new StringBuilder();
                MailBnfHelper.ReadDigits(Value, ref offset, builder);

                if ((!MailBnfHelper.SkipCFWS(Value, ref offset) || offset >= Value.Length || Value[offset++] != '.') || !MailBnfHelper.SkipCFWS(Value, ref offset))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeVersionHeaderInvalid)));

                builder.Append('.');

                MailBnfHelper.ReadDigits(Value, ref offset, builder);

                this.version = builder.ToString();
            }
        }
    }

    internal class MimeHeaderReader
    {
        enum ReadState
        {
            ReadName,
            SkipWS,
            ReadValue,
            ReadLF,
            ReadWS,
            EOF
        }

        string value;
        byte[] buffer = new byte[1024];
        int maxOffset;
        string name;
        int offset;
        ReadState readState = ReadState.ReadName;
        Stream stream;

        public MimeHeaderReader(Stream stream)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");

            this.stream = stream;
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public void Close()
        {
            stream.Close();
            readState = ReadState.EOF;
        }

        public bool Read(int maxBuffer, ref int remaining)
        {
            name = null;
            value = null;

            while (readState != ReadState.EOF)
            {
                if (offset == maxOffset)
                {
                    maxOffset = stream.Read(this.buffer, 0, this.buffer.Length);
                    offset = 0;
                    if (BufferEnd())
                        break;
                }
                if (ProcessBuffer(maxBuffer, ref remaining))
                    break;
            }

            return value != null;
        }

        [Fx.Tag.SecurityNote(Critical = "Calls unsafe code", Safe = "Demands for FullTrust")]
        [SecuritySafeCritical]
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)]
        bool ProcessBuffer(int maxBuffer, ref int remaining)
        {
            unsafe
            {
                fixed (byte* pBuffer = this.buffer)
                {
                    byte* start = pBuffer + this.offset;
                    byte* end = pBuffer + this.maxOffset;
                    byte* ptr = start;

                    switch (readState)
                    {
                        case ReadState.ReadName:
                            for (; ptr < end; ptr++)
                            {
                                if (*ptr == ':')
                                {
                                    AppendName(new string((sbyte*)start, 0, (int)(ptr - start)), maxBuffer, ref remaining);
                                    ptr++;
                                    goto case ReadState.SkipWS;
                                }
                                else
                                {
                                    // convert to lower case up front.
                                    if (*ptr >= 'A' && *ptr <= 'Z')
                                    {
                                        *ptr += 'a' - 'A';
                                    }
                                    else if (*ptr < 33 || *ptr > 126)
                                    {
                                        if (name == null && *ptr == (byte)'\r')
                                        {
                                            ptr++;
                                            if (ptr >= end || *ptr != '\n')
                                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderMalformedHeader)));
                                            goto case ReadState.EOF;
                                        }

                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, (char)(*ptr), ((int)(*ptr)).ToString("X", CultureInfo.InvariantCulture))));
                                    }
                                }
                            }
                            AppendName(new string((sbyte*)start, 0, (int)(ptr - start)), maxBuffer, ref remaining);
                            readState = ReadState.ReadName;
                            break;
                        case ReadState.SkipWS:
                            for (; ptr < end; ptr++)
                                if (*ptr != (byte)'\t' && *ptr != ' ')
                                    goto case ReadState.ReadValue;
                            readState = ReadState.SkipWS;
                            break;
                        case ReadState.ReadValue:
                            start = ptr;
                            for (; ptr < end; ptr++)
                            {
                                if (*ptr == (byte)'\r')
                                {
                                    AppendValue(new string((sbyte*)start, 0, (int)(ptr - start)), maxBuffer, ref remaining);
                                    ptr++;
                                    goto case ReadState.ReadLF;
                                }
                                else if (*ptr == (byte)'\n')
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderMalformedHeader)));
                                }
                            }
                            AppendValue(new string((sbyte*)start, 0, (int)(ptr - start)), maxBuffer, ref remaining);
                            readState = ReadState.ReadValue;
                            break;
                        case ReadState.ReadLF:
                            if (ptr < end)
                            {
                                if (*ptr != (byte)'\n')
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderMalformedHeader)));
                                ptr++;
                                goto case ReadState.ReadWS;
                            }
                            readState = ReadState.ReadLF;
                            break;
                        case ReadState.ReadWS:
                            if (ptr < end)
                            {
                                if (*ptr != (byte)' ' && *ptr != (byte)'\t')
                                {
                                    readState = ReadState.ReadName;
                                    offset = (int)(ptr - pBuffer);
                                    return true;
                                }
                                goto case ReadState.ReadValue;
                            }
                            readState = ReadState.ReadWS;
                            break;
                        case ReadState.EOF:
                            readState = ReadState.EOF;
                            offset = (int)(ptr - pBuffer);
                            return true;
                    }
                    offset = (int)(ptr - pBuffer);
                }
            }
            return false;
        }

        bool BufferEnd()
        {
            if (maxOffset == 0)
            {
                if (readState != ReadState.ReadWS && readState != ReadState.ReadValue)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderMalformedHeader)));

                readState = ReadState.EOF;
                return true;
            }
            return false;
        }

        // Resets the mail field reader to the new stream to reuse buffers 
        public void Reset(Stream stream)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");

            if (readState != ReadState.EOF)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeReaderResetCalledBeforeEOF)));

            this.stream = stream;
            readState = ReadState.ReadName;
            maxOffset = 0;
            offset = 0;
        }

        // helper methods

        void AppendValue(string value, int maxBuffer, ref int remaining)
        {
            XmlMtomReader.DecrementBufferQuota(maxBuffer, ref remaining, value.Length * sizeof(char));
            if (this.value == null)
                this.value = value;
            else
                this.value += value;
        }

        void AppendName(string value, int maxBuffer, ref int remaining)
        {
            XmlMtomReader.DecrementBufferQuota(maxBuffer, ref remaining, value.Length * sizeof(char));
            if (this.name == null)
                this.name = value;
            else
                this.name += value;
        }

    }

    internal class BufferedReadStream : Stream
    {
        Stream stream;
        byte[] storedBuffer;
        int storedLength;
        int storedOffset;
        bool readMore;

        public BufferedReadStream(Stream stream)
            : this(stream, false)
        {
        }

        public BufferedReadStream(Stream stream, bool readMore)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");

            this.stream = stream;
            this.readMore = readMore;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override long Length
        {
            get
            {
#pragma warning suppress 56503 // Microsoft, required by the Stream contract
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, stream.GetType().FullName)));
            }
        }

        public override long Position
        {
            get
            {
#pragma warning suppress 56503 // Microsoft, required by the Stream contract
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, stream.GetType().FullName)));
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, stream.GetType().FullName)));
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!CanRead)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ReadNotSupportedOnStream, stream.GetType().FullName)));

            return stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, stream.GetType().FullName)));
        }

        public override void Close()
        {
            stream.Close();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!CanRead)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ReadNotSupportedOnStream, stream.GetType().FullName)));

            return stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, stream.GetType().FullName)));
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ReadNotSupportedOnStream, stream.GetType().FullName)));

            if (buffer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");

            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > buffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, buffer.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > buffer.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - offset)));

            int read = 0;
            if (this.storedOffset < this.storedLength)
            {
                read = Math.Min(count, this.storedLength - this.storedOffset);
                Buffer.BlockCopy(this.storedBuffer, this.storedOffset, buffer, offset, read);
                this.storedOffset += read;
                if (read == count || !this.readMore)
                    return read;
                offset += read;
                count -= read;
            }
            return read + stream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if (this.storedOffset < this.storedLength)
                return (int)this.storedBuffer[this.storedOffset++];
            else
                return base.ReadByte();
        }

        public int ReadBlock(byte[] buffer, int offset, int count)
        {
            int read;
            int total = 0;
            while (total < count && (read = Read(buffer, offset + total, count - total)) != 0)
            {
                total += read;
            }
            return total;
        }

        public void Push(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return;

            if (this.storedOffset == this.storedLength)
            {
                if (this.storedBuffer == null || this.storedBuffer.Length < count)
                    this.storedBuffer = new byte[count];
                this.storedOffset = 0;
                this.storedLength = count;
            }
            else
            {
                // if there's room to just insert before existing data
                if (count <= this.storedOffset)
                    this.storedOffset -= count;
                // if there's room in the buffer but need to shift things over
                else if (count <= this.storedBuffer.Length - this.storedLength + this.storedOffset)
                {
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, this.storedBuffer, count, this.storedLength - this.storedOffset);
                    this.storedLength += count - this.storedOffset;
                    this.storedOffset = 0;
                }
                else
                {
                    byte[] newBuffer = new byte[count + this.storedLength - this.storedOffset];
                    Buffer.BlockCopy(this.storedBuffer, this.storedOffset, newBuffer, count, this.storedLength - this.storedOffset);
                    this.storedLength += count - this.storedOffset;
                    this.storedOffset = 0;
                    this.storedBuffer = newBuffer;
                }
            }
            Buffer.BlockCopy(buffer, offset, this.storedBuffer, this.storedOffset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, stream.GetType().FullName)));
        }

        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SeekNotSupportedOnStream, stream.GetType().FullName)));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.WriteNotSupportedOnStream, stream.GetType().FullName)));
        }
    }

    internal static class MailBnfHelper
    {
        static bool[] s_fqtext = new bool[128];
        static bool[] s_ttext = new bool[128];
        static bool[] s_digits = new bool[128];
        static bool[] s_boundary = new bool[128];

        static MailBnfHelper()
        {
            // fqtext = %d1-9 / %d11 / %d12 / %d14-33 / %d35-91 / %d93-127
            for (int i = 1; i <= 9; i++) { s_fqtext[i] = true; }
            s_fqtext[11] = true;
            s_fqtext[12] = true;
            for (int i = 14; i <= 33; i++) { s_fqtext[i] = true; }
            for (int i = 35; i <= 91; i++) { s_fqtext[i] = true; }
            for (int i = 93; i <= 127; i++) { s_fqtext[i] = true; }

            // ttext = %d33-126 except '()<>@,;:\"/[]?='
            for (int i = 33; i <= 126; i++) { s_ttext[i] = true; }
            s_ttext['('] = false;
            s_ttext[')'] = false;
            s_ttext['<'] = false;
            s_ttext['>'] = false;
            s_ttext['@'] = false;
            s_ttext[','] = false;
            s_ttext[';'] = false;
            s_ttext[':'] = false;
            s_ttext['\\'] = false;
            s_ttext['"'] = false;
            s_ttext['/'] = false;
            s_ttext['['] = false;
            s_ttext[']'] = false;
            s_ttext['?'] = false;
            s_ttext['='] = false;

            // digits = %d48-57
            for (int i = 48; i <= 57; i++)
                s_digits[i] = true;

            // boundary = DIGIT / ALPHA / "'" / "(" / ")" / "+" / "_" / "," / "-" / "." / "/" / ":" / "=" / "?" / " "
            // cannot end with " "
            for (int i = '0'; i <= '9'; i++) { s_boundary[i] = true; }
            for (int i = 'A'; i <= 'Z'; i++) { s_boundary[i] = true; }
            for (int i = 'a'; i <= 'z'; i++) { s_boundary[i] = true; }
            s_boundary['\''] = true;
            s_boundary['('] = true;
            s_boundary[')'] = true;
            s_boundary['+'] = true;
            s_boundary['_'] = true;
            s_boundary[','] = true;
            s_boundary['-'] = true;
            s_boundary['.'] = true;
            s_boundary['/'] = true;
            s_boundary[':'] = true;
            s_boundary['='] = true;
            s_boundary['?'] = true;
            s_boundary[' '] = true;
        }

        public static bool SkipCFWS(string data, ref int offset)
        {
            int comments = 0;
            for (; offset < data.Length; offset++)
            {
                if (data[offset] > 127)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, data[offset], ((int)data[offset]).ToString("X", CultureInfo.InvariantCulture))));
                else if (data[offset] == '\\' && comments > 0)
                    offset += 2;
                else if (data[offset] == '(')
                    comments++;
                else if (data[offset] == ')')
                    comments--;
                else if (data[offset] != ' ' && data[offset] != '\t' && comments == 0)
                    return true;
            }
            return false;
        }

        public static string ReadQuotedString(string data, ref int offset, StringBuilder builder)
        {
            // assume first char is the opening quote
            int start = ++offset;
            StringBuilder localBuilder = (builder != null ? builder : new StringBuilder());
            for (; offset < data.Length; offset++)
            {
                if (data[offset] == '\\')
                {
                    localBuilder.Append(data, start, offset - start);
                    start = ++offset;
                    continue;
                }
                else if (data[offset] == '"')
                {
                    localBuilder.Append(data, start, offset - start);
                    offset++;
                    return (builder != null ? null : localBuilder.ToString());
                }
                else if (!(data[offset] < s_fqtext.Length && s_fqtext[data[offset]]))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, data[offset], ((int)data[offset]).ToString("X", CultureInfo.InvariantCulture))));
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeReaderMalformedHeader)));
        }

        public static string ReadParameterAttribute(string data, ref int offset, StringBuilder builder)
        {
            if (!SkipCFWS(data, ref offset))
                return null;

            return ReadToken(data, ref offset, null);
        }

        public static string ReadParameterValue(string data, ref int offset, StringBuilder builder)
        {
            if (!SkipCFWS(data, ref offset))
                return string.Empty;

            if (offset < data.Length && data[offset] == '"')
                return ReadQuotedString(data, ref offset, builder);
            else
                return ReadToken(data, ref offset, builder);
        }

        public static string ReadToken(string data, ref int offset, StringBuilder builder)
        {
            int start = offset;
            for (; offset < data.Length; offset++)
            {
                if (data[offset] > s_ttext.Length)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, data[offset], ((int)data[offset]).ToString("X", CultureInfo.InvariantCulture))));
                }
                else if (!s_ttext[data[offset]])
                {
                    break;
                }
            }
            return data.Substring(start, offset - start);
        }

        public static string ReadDigits(string data, ref int offset, StringBuilder builder)
        {
            int start = offset;
            StringBuilder localBuilder = (builder != null ? builder : new StringBuilder());
            for (; offset < data.Length && data[offset] < s_digits.Length && s_digits[data[offset]]; offset++);
            localBuilder.Append(data, start, offset - start);
            return (builder != null ? null : localBuilder.ToString());
        }

        public static bool IsValidMimeBoundary(string data)
        {
            int length = (data == null) ? 0 : data.Length;
            if (length == 0 || length > 70 || data[length - 1] == ' ')
                return false;

            for (int i = 0; i < length; i++)
            {
                if (!(data[i] < s_boundary.Length && s_boundary[data[i]]))
                    return false;
            }

            return true;
        }
    }

}

