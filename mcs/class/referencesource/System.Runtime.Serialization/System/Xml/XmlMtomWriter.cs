//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xml
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using System.Runtime.Serialization;

    public interface IXmlMtomWriterInitializer
    {
        void SetOutput(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream);
    }

    class XmlMtomWriter : XmlDictionaryWriter, IXmlMtomWriterInitializer
    {
        // Maximum number of bytes that are inlined as base64 data without being MTOM-optimized as xop:Include
        const int MaxInlinedBytes = 767;  // 768 will be the first MIMEd length

        int maxSizeInBytes;
        XmlDictionaryWriter writer;
        XmlDictionaryWriter infosetWriter;
        MimeWriter mimeWriter;
        Encoding encoding;
        bool isUTF8;
        string contentID;
        string contentType;
        string initialContentTypeForRootPart;
        string initialContentTypeForMimeMessage;
        MemoryStream contentTypeStream;
        List<MimePart> mimeParts;
        IList<MtomBinaryData> binaryDataChunks;
        int depth;
        int totalSizeOfMimeParts;
        int sizeOfBufferedBinaryData;
        char[] chars;
        byte[] bytes;
        bool isClosed;
        bool ownsStream;

        public XmlMtomWriter()
        {
        }

        public void SetOutput(Stream stream, Encoding encoding, int maxSizeInBytes, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            if (encoding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            if (maxSizeInBytes < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxSizeInBytes", SR.GetString(SR.ValueMustBeNonNegative)));
            this.maxSizeInBytes = maxSizeInBytes;
            this.encoding = encoding;
            this.isUTF8 = IsUTF8Encoding(encoding);
            Initialize(stream, startInfo, boundary, startUri, writeMessageHeaders, ownsStream);
        }

        XmlDictionaryWriter Writer
        {
            get
            {
                if (!IsInitialized)
                {
                    Initialize();
                }
                return writer;
            }
        }

        bool IsInitialized
        {
            get { return (initialContentTypeForRootPart == null); }
        }

        void Initialize(Stream stream, string startInfo, string boundary, string startUri, bool writeMessageHeaders, bool ownsStream)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            if (startInfo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("startInfo");
            if (boundary == null)
                boundary = GetBoundaryString();
            if (startUri == null)
                startUri = GenerateUriForMimePart(0);
            if (!MailBnfHelper.IsValidMimeBoundary(boundary))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MtomBoundaryInvalid, boundary), "boundary"));

            this.ownsStream = ownsStream;
            this.isClosed = false;
            this.depth = 0;
            this.totalSizeOfMimeParts = 0;
            this.sizeOfBufferedBinaryData = 0;
            this.binaryDataChunks = null;
            this.contentType = null;
            this.contentTypeStream = null;
            this.contentID = startUri;
            if (this.mimeParts != null)
                this.mimeParts.Clear();
            this.mimeWriter = new MimeWriter(stream, boundary);
            this.initialContentTypeForRootPart = GetContentTypeForRootMimePart(this.encoding, startInfo);
            if (writeMessageHeaders)
                this.initialContentTypeForMimeMessage = GetContentTypeForMimeMessage(boundary, startUri, startInfo);
        }

        void Initialize()
        {
            if (this.isClosed)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlWriterClosed)));

            if (this.initialContentTypeForRootPart != null)
            {
                if (this.initialContentTypeForMimeMessage != null)
                {
                    mimeWriter.StartPreface();
                    mimeWriter.WriteHeader(MimeGlobals.MimeVersionHeader, MimeGlobals.DefaultVersion);
                    mimeWriter.WriteHeader(MimeGlobals.ContentTypeHeader, this.initialContentTypeForMimeMessage);
                    this.initialContentTypeForMimeMessage = null;
                }

                WriteMimeHeaders(this.contentID, this.initialContentTypeForRootPart, this.isUTF8 ? MimeGlobals.Encoding8bit : MimeGlobals.EncodingBinary);

                Stream infosetContentStream = this.mimeWriter.GetContentStream();
                IXmlTextWriterInitializer initializer = writer as IXmlTextWriterInitializer;
                if (initializer == null)
                    writer = XmlDictionaryWriter.CreateTextWriter(infosetContentStream, this.encoding, this.ownsStream);
                else
                    initializer.SetOutput(infosetContentStream, this.encoding, this.ownsStream);

                this.contentID = null;
                this.initialContentTypeForRootPart = null;
            }
        }

        static string GetBoundaryString()
        {
            return MimeBoundaryGenerator.Next();
        }

        internal static bool IsUTF8Encoding(Encoding encoding)
        {
            return encoding.WebName == "utf-8";
        }

        static string GetContentTypeForMimeMessage(string boundary, string startUri, string startInfo)
        {
            StringBuilder contentTypeBuilder = new StringBuilder(
                String.Format(CultureInfo.InvariantCulture, "{0}/{1};{2}=\"{3}\";{4}=\"{5}\"",
                    MtomGlobals.MediaType, MtomGlobals.MediaSubtype,
                    MtomGlobals.TypeParam, MtomGlobals.XopType,
                    MtomGlobals.BoundaryParam, boundary));

            if (startUri != null && startUri.Length > 0)
                contentTypeBuilder.AppendFormat(CultureInfo.InvariantCulture, ";{0}=\"<{1}>\"", MtomGlobals.StartParam, startUri);

            if (startInfo != null && startInfo.Length > 0)
                contentTypeBuilder.AppendFormat(CultureInfo.InvariantCulture, ";{0}=\"{1}\"", MtomGlobals.StartInfoParam, startInfo);

            return contentTypeBuilder.ToString();
        }

        static string GetContentTypeForRootMimePart(Encoding encoding, string startInfo)
        {
            string contentType = String.Format(CultureInfo.InvariantCulture, "{0};{1}={2}", MtomGlobals.XopType, MtomGlobals.CharsetParam, CharSet(encoding));

            if (startInfo != null)
                contentType = String.Format(CultureInfo.InvariantCulture, "{0};{1}=\"{2}\"", contentType, MtomGlobals.TypeParam, startInfo);

            return contentType;
        }

        static string CharSet(Encoding enc)
        {
            string name = enc.WebName;
            if (String.Compare(name, Encoding.UTF8.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                return name;
            if (String.Compare(name, Encoding.Unicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                return "utf-16LE";
            if (String.Compare(name, Encoding.BigEndianUnicode.WebName, StringComparison.OrdinalIgnoreCase) == 0)
                return "utf-16BE";
            return name;
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            WriteBase64InlineIfPresent();
            ThrowIfElementIsXOPInclude(prefix, localName, ns);
            Writer.WriteStartElement(prefix, localName, ns);
            depth++;
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (localName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");

            WriteBase64InlineIfPresent();
            ThrowIfElementIsXOPInclude(prefix, localName.Value, ns == null ? null : ns.Value);
            Writer.WriteStartElement(prefix, localName, ns);
            depth++;
        }

        void ThrowIfElementIsXOPInclude(string prefix, string localName, string ns)
        {
            if (ns == null)
            {
                XmlBaseWriter w = this.Writer as XmlBaseWriter;
                if (w != null)
                    ns = w.LookupNamespace(prefix);
            }

            if (localName == MtomGlobals.XopIncludeLocalName && ns == MtomGlobals.XopIncludeNamespace)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomDataMustNotContainXopInclude, MtomGlobals.XopIncludeLocalName, MtomGlobals.XopIncludeNamespace)));
        }

        public override void WriteEndElement()
        {
            WriteXOPInclude();
            Writer.WriteEndElement();
            depth--;
            WriteXOPBinaryParts();
        }

        public override void WriteFullEndElement()
        {
            WriteXOPInclude();
            Writer.WriteFullEndElement();
            depth--;
            WriteXOPBinaryParts();
        }

        public override void WriteValue(IStreamProvider value)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

            if (Writer.WriteState == WriteState.Element)
            {
                if (binaryDataChunks == null)
                {
                    binaryDataChunks = new List<MtomBinaryData>();
                    contentID = GenerateUriForMimePart((mimeParts == null) ? 1 : mimeParts.Count + 1);
                }
                binaryDataChunks.Add(new MtomBinaryData(value));
            }
            else
                Writer.WriteValue(value);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            if (Writer.WriteState == WriteState.Element)
            {
                if (buffer == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("buffer"));

                // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
                if (index < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeNonNegative)));

                if (count < 0)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
                if (count > buffer.Length - index)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, buffer.Length - index)));

                if (binaryDataChunks == null)
                {
                    binaryDataChunks = new List<MtomBinaryData>();
                    contentID = GenerateUriForMimePart((mimeParts == null) ? 1 : mimeParts.Count + 1);
                }

                int totalSize = ValidateSizeOfMessage(maxSizeInBytes, 0, totalSizeOfMimeParts);
                totalSize += ValidateSizeOfMessage(maxSizeInBytes, totalSize, sizeOfBufferedBinaryData);
                totalSize += ValidateSizeOfMessage(maxSizeInBytes, totalSize, count);
                sizeOfBufferedBinaryData += count;
                binaryDataChunks.Add(new MtomBinaryData(buffer, index, count));
            }
            else
                Writer.WriteBase64(buffer, index, count);
        }

        internal static int ValidateSizeOfMessage(int maxSize, int offset, int size)
        {
            if (size > (maxSize - offset))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.MtomExceededMaxSizeInBytes, maxSize)));
            return size;
        }

        void WriteBase64InlineIfPresent()
        {
            if (binaryDataChunks != null)
            {
                WriteBase64Inline();
            }
        }

        void WriteBase64Inline()
        {
            foreach (MtomBinaryData data in binaryDataChunks)
            {
                if (data.type == MtomBinaryDataType.Provider)
                {
                    Writer.WriteValue(data.provider);
                }
                else
                {
                    Writer.WriteBase64(data.chunk, 0, data.chunk.Length);
                }
            }
            this.sizeOfBufferedBinaryData = 0;
            binaryDataChunks = null;
            contentType = null;
            contentID = null;
        }

        void WriteXOPInclude()
        {
            if (binaryDataChunks == null)
                return;

            bool inline = true;
            long size = 0;
            foreach (MtomBinaryData data in binaryDataChunks)
            {
                long len = data.Length;
                if (len < 0 || len > (MaxInlinedBytes - size))
                {
                    inline = false;
                    break;
                }
                size += len;
            }

            if (inline)
                WriteBase64Inline();
            else
            {
                if (mimeParts == null)
                    mimeParts = new List<MimePart>();

                MimePart mimePart = new MimePart(binaryDataChunks, contentID, contentType, MimeGlobals.EncodingBinary, sizeOfBufferedBinaryData, maxSizeInBytes);
                mimeParts.Add(mimePart);

                totalSizeOfMimeParts += ValidateSizeOfMessage(maxSizeInBytes, totalSizeOfMimeParts, mimePart.sizeInBytes);
                totalSizeOfMimeParts += ValidateSizeOfMessage(maxSizeInBytes, totalSizeOfMimeParts, mimeWriter.GetBoundarySize());

                Writer.WriteStartElement(MtomGlobals.XopIncludePrefix, MtomGlobals.XopIncludeLocalName, MtomGlobals.XopIncludeNamespace);
                Writer.WriteStartAttribute(MtomGlobals.XopIncludeHrefLocalName, MtomGlobals.XopIncludeHrefNamespace);
                Writer.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0}{1}", MimeGlobals.ContentIDScheme, contentID));
                Writer.WriteEndAttribute();
                Writer.WriteEndElement();
                binaryDataChunks = null;
                sizeOfBufferedBinaryData = 0;
                contentType = null;
                contentID = null;
            }
        }

        public static string GenerateUriForMimePart(int index)
        {
            return String.Format(CultureInfo.InvariantCulture, "http://tempuri.org/{0}/{1}", index, DateTime.Now.Ticks);
        }

        void WriteXOPBinaryParts()
        {
            if (depth > 0 || mimeWriter.WriteState == MimeWriterState.Closed)
                return;

            if (Writer.WriteState != WriteState.Closed)
                Writer.Flush();

            if (mimeParts != null)
            {
                foreach (MimePart part in mimeParts)
                {
                    WriteMimeHeaders(part.contentID, part.contentType, part.contentTransferEncoding);
                    Stream s = mimeWriter.GetContentStream();
                    int blockSize = 256;
                    int bytesRead = 0;
                    byte[] block = new byte[blockSize];
                    Stream stream = null;
                    foreach (MtomBinaryData data in part.binaryData)
                    {
                        if (data.type == MtomBinaryDataType.Provider)
                        {
                            stream = data.provider.GetStream();
                            if (stream == null)
                                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidStream)));
                            while (true)
                            {
                                bytesRead = stream.Read(block, 0, blockSize);
                                if (bytesRead > 0)
                                    s.Write(block, 0, bytesRead);
                                else
                                    break;
                                if (blockSize < 65536 && bytesRead == blockSize)
                                {
                                    blockSize = blockSize * 16;
                                    block = new byte[blockSize];
                                }
                            }

                            data.provider.ReleaseStream(stream);
                        }
                        else
                        {
                            s.Write(data.chunk, 0, data.chunk.Length);
                        }
                    }
                }
                mimeParts.Clear();
            }
            mimeWriter.Close();
        }

        void WriteMimeHeaders(string contentID, string contentType, string contentTransferEncoding)
        {
            mimeWriter.StartPart();
            if (contentID != null)
                mimeWriter.WriteHeader(MimeGlobals.ContentIDHeader, String.Format(CultureInfo.InvariantCulture, "<{0}>", contentID));
            if (contentTransferEncoding != null)
                mimeWriter.WriteHeader(MimeGlobals.ContentTransferEncodingHeader, contentTransferEncoding);
            if (contentType != null)
                mimeWriter.WriteHeader(MimeGlobals.ContentTypeHeader, contentType);
        }
#if NO
        public override bool CanSubsetElements
        {
            get { return Writer.CanSubsetElements; }
        }
#endif
        public override void Close()
        {
            if (!this.isClosed)
            {
                this.isClosed = true;
                if (IsInitialized)
                {
                    WriteXOPInclude();
                    if (Writer.WriteState == WriteState.Element ||
                        Writer.WriteState == WriteState.Attribute ||
                        Writer.WriteState == WriteState.Content)
                    {
                        Writer.WriteEndDocument();
                    }
                    Writer.Flush();
                    depth = 0;
                    WriteXOPBinaryParts();
                    Writer.Close();
                }
            }
        }

        void CheckIfStartContentTypeAttribute(string localName, string ns)
        {
            if (localName != null && localName == MtomGlobals.MimeContentTypeLocalName
                && ns != null && (ns == MtomGlobals.MimeContentTypeNamespace200406 || ns == MtomGlobals.MimeContentTypeNamespace200505))
            {
                contentTypeStream = new MemoryStream();
                this.infosetWriter = Writer;
                this.writer = XmlDictionaryWriter.CreateBinaryWriter(contentTypeStream);
                Writer.WriteStartElement("Wrapper");
                Writer.WriteStartAttribute(localName, ns);
            }
        }

        void CheckIfEndContentTypeAttribute()
        {
            if (contentTypeStream != null)
            {
                Writer.WriteEndAttribute();
                Writer.WriteEndElement();
                Writer.Flush();
                contentTypeStream.Position = 0;
                XmlReader contentTypeReader = XmlDictionaryReader.CreateBinaryReader(contentTypeStream, null, XmlDictionaryReaderQuotas.Max, null, null);
                while (contentTypeReader.Read())
                {
                    if (contentTypeReader.IsStartElement("Wrapper"))
                    {
                        contentType = contentTypeReader.GetAttribute(MtomGlobals.MimeContentTypeLocalName, MtomGlobals.MimeContentTypeNamespace200406);
                        if (contentType == null)
                        {
                            contentType = contentTypeReader.GetAttribute(MtomGlobals.MimeContentTypeLocalName, MtomGlobals.MimeContentTypeNamespace200505);
                        }
                        break;
                    }
                }
                this.writer = infosetWriter;
                this.infosetWriter = null;
                contentTypeStream = null;
                if (contentType != null)
                    Writer.WriteString(contentType);
            }
        }

#if NO
        public override bool ElementSubsetting
        {
            get
            {
                return Writer.ElementSubsetting;
            }
            set
            {
                Writer.ElementSubsetting = value;
            }
        }
#endif
        public override void Flush()
        {
            if (IsInitialized)
                Writer.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return Writer.LookupPrefix(ns);
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                return Writer.Settings;
            }
        }

        public override void WriteAttributes(XmlReader reader, bool defattr)
        {
            Writer.WriteAttributes(reader, defattr);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            // Don't write comments after the document element
            if (depth == 0 && mimeWriter.WriteState == MimeWriterState.Closed)
                return;

            WriteBase64InlineIfPresent();
            Writer.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteDocType(name, pubid, sysid, subset);
        }
#if NO
        public override void WriteElementSubset(ArraySegment<byte> buffer)
        {
            Writer.WriteElementSubset(buffer);
        }
#endif
        public override void WriteEndAttribute()
        {
            CheckIfEndContentTypeAttribute();
            Writer.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            WriteXOPInclude();
            Writer.WriteEndDocument();
            depth = 0;
            WriteXOPBinaryParts();
        }

        public override void WriteEntityRef(string name)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteEntityRef(name);
        }

        public override void WriteName(string name)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteNmToken(name);
        }

        protected override void WriteTextNode(XmlDictionaryReader reader, bool attribute)
        {
            Type type = reader.ValueType;
            if (type == typeof(string))
            {
                if (reader.CanReadValueChunk)
                {
                    if (chars == null)
                    {
                        chars = new char[256];
                    }
                    int count;
                    while ((count = reader.ReadValueChunk(chars, 0, chars.Length)) > 0)
                    {
                        this.WriteChars(chars, 0, count);
                    }
                }
                else
                {
                    WriteString(reader.Value);
                }
                if (!attribute)
                {
                    reader.Read();
                }
            }
            else if (type == typeof(byte[]))
            {
                if (reader.CanReadBinaryContent)
                {
                    // Its best to read in buffers that are a multiple of 3 so we don't break base64 boundaries when converting text
                    if (bytes == null)
                    {
                        bytes = new byte[384];
                    }
                    int count;
                    while ((count = reader.ReadValueAsBase64(bytes, 0, bytes.Length)) > 0)
                    {
                        this.WriteBase64(bytes, 0, count);
                    }
                }
                else
                {
                    WriteString(reader.Value);
                }
                if (!attribute)
                {
                    reader.Read();
                }
            }
            else
            {
                base.WriteTextNode(reader, attribute);
            }
        }

        public override void WriteNode(XPathNavigator navigator, bool defattr)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteNode(navigator, defattr);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string namespaceUri)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteQualifiedName(localName, namespaceUri);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteRaw(buffer, index, count);
        }

        public override void WriteRaw(string data)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteRaw(data);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            Writer.WriteStartAttribute(prefix, localName, ns);
            CheckIfStartContentTypeAttribute(localName, ns);
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString ns)
        {
            Writer.WriteStartAttribute(prefix, localName, ns);
            if (localName != null && ns != null)
                CheckIfStartContentTypeAttribute(localName.Value, ns.Value);
        }

        public override void WriteStartDocument()
        {
            Writer.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            Writer.WriteStartDocument(standalone);
        }

        public override WriteState WriteState
        {
            get
            {
                return Writer.WriteState;
            }
        }

        public override void WriteString(string text)
        {
            // Don't write whitespace after the document element
            if (depth == 0 && mimeWriter.WriteState == MimeWriterState.Closed && XmlConverter.IsWhitespace(text))
                return;

            WriteBase64InlineIfPresent();
            Writer.WriteString(text);
        }

        public override void WriteString(XmlDictionaryString value)
        {
            // Don't write whitespace after the document element
            if (depth == 0 && mimeWriter.WriteState == MimeWriterState.Closed && XmlConverter.IsWhitespace(value.Value))
                return;

            WriteBase64InlineIfPresent();
            Writer.WriteString(value);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string whitespace)
        {
            // Don't write whitespace after the document element
            if (depth == 0 && mimeWriter.WriteState == MimeWriterState.Closed)
                return;

            WriteBase64InlineIfPresent();
            Writer.WriteWhitespace(whitespace);
        }

        public override void WriteValue(object value)
        {
            IStreamProvider sp = value as IStreamProvider;
            if (sp != null)
            {
                WriteValue(sp);
            }
            else
            {
                WriteBase64InlineIfPresent();
                Writer.WriteValue(value);
            }
        }

        public override void WriteValue(string value)
        {
            // Don't write whitespace after the document element
            if (depth == 0 && mimeWriter.WriteState == MimeWriterState.Closed && XmlConverter.IsWhitespace(value))
                return;

            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteValue(bool value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteValue(DateTime value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteValue(double value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteValue(int value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteValue(long value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

#if DECIMAL_FLOAT_API
        public override void WriteValue(decimal value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteValue(float value)
        {
            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }
#endif
        public override void WriteValue(XmlDictionaryString value)
        {
            // Don't write whitespace after the document element
            if (depth == 0 && mimeWriter.WriteState == MimeWriterState.Closed && XmlConverter.IsWhitespace(value.Value))
                return;

            WriteBase64InlineIfPresent();
            Writer.WriteValue(value);
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            Writer.WriteXmlnsAttribute(prefix, ns);
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            Writer.WriteXmlnsAttribute(prefix, ns);
        }

        public override string XmlLang
        {
            get
            {
                return Writer.XmlLang;
            }
        }

        public override XmlSpace XmlSpace
        {
            get
            {
                return Writer.XmlSpace;
            }
        }

        static class MimeBoundaryGenerator
        {
            static long id;
            static string prefix;

            static MimeBoundaryGenerator()
            {
                prefix = string.Concat(Guid.NewGuid().ToString(), "+id=");
            }

            internal static string Next()
            {
                long nextId = Interlocked.Increment(ref id);
                return String.Format(CultureInfo.InvariantCulture, "{0}{1}", prefix, nextId);
            }
        }

        class MimePart
        {
            internal IList<MtomBinaryData> binaryData;
            internal string contentID;
            internal string contentType;
            internal string contentTransferEncoding;
            internal int sizeInBytes;

            internal MimePart(IList<MtomBinaryData> binaryData, string contentID, string contentType, string contentTransferEncoding, int sizeOfBufferedBinaryData, int maxSizeInBytes)
            {
                this.binaryData = binaryData;
                this.contentID = contentID;
                this.contentType = contentType ?? MtomGlobals.DefaultContentTypeForBinary;
                this.contentTransferEncoding = contentTransferEncoding;
                this.sizeInBytes = GetSize(contentID, contentType, contentTransferEncoding, sizeOfBufferedBinaryData, maxSizeInBytes);
            }

            static int GetSize(string contentID, string contentType, string contentTransferEncoding, int sizeOfBufferedBinaryData, int maxSizeInBytes)
            {
                int size = XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, 0, MimeGlobals.CRLF.Length * 3);
                if (contentTransferEncoding != null)
                    size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, MimeWriter.GetHeaderSize(MimeGlobals.ContentTransferEncodingHeader, contentTransferEncoding, maxSizeInBytes));
                if (contentType != null)
                    size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, MimeWriter.GetHeaderSize(MimeGlobals.ContentTypeHeader, contentType, maxSizeInBytes));
                if (contentID != null)
                {
                    size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, MimeWriter.GetHeaderSize(MimeGlobals.ContentIDHeader, contentID, maxSizeInBytes));
                    size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, 2); // include '<' and '>'
                }
                size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, sizeOfBufferedBinaryData);
                return size;
            }
        }
    }


    internal static class MtomGlobals
    {
        internal static string XopIncludeLocalName = "Include";
        internal static string XopIncludeNamespace = "http://www.w3.org/2004/08/xop/include";
        internal static string XopIncludePrefix = "xop";
        internal static string XopIncludeHrefLocalName = "href";
        internal static string XopIncludeHrefNamespace = String.Empty;
        internal static string MediaType = "multipart";
        internal static string MediaSubtype = "related";
        internal static string BoundaryParam = "boundary";
        internal static string TypeParam = "type";
        internal static string XopMediaType = "application";
        internal static string XopMediaSubtype = "xop+xml";
        internal static string XopType = "application/xop+xml";
        internal static string StartParam = "start";
        internal static string StartInfoParam = "start-info";
        internal static string ActionParam = "action";
        internal static string CharsetParam = "charset";
        internal static string MimeContentTypeLocalName = "contentType";
        internal static string MimeContentTypeNamespace200406 = "http://www.w3.org/2004/06/xmlmime";
        internal static string MimeContentTypeNamespace200505 = "http://www.w3.org/2005/05/xmlmime";
        internal static string DefaultContentTypeForBinary = "application/octet-stream";
    }

    internal static class MimeGlobals
    {
        internal static string MimeVersionHeader = "MIME-Version";
        internal static string DefaultVersion = "1.0";
        internal static string ContentIDScheme = "cid:";
        internal static string ContentIDHeader = "Content-ID";
        internal static string ContentTypeHeader = "Content-Type";
        internal static string ContentTransferEncodingHeader = "Content-Transfer-Encoding";
        internal static string EncodingBinary = "binary";
        internal static string Encoding8bit = "8bit";
        internal static byte[] COLONSPACE = new byte[] { (byte)':', (byte)' ' };
        internal static byte[] DASHDASH = new byte[] { (byte)'-', (byte)'-' };
        internal static byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };
        // Per RFC2045, preceding CRLF sequence is part of the boundary. MIME boundary tags begin with --
        internal static byte[] BoundaryPrefix = new byte[] { (byte)'\r', (byte)'\n', (byte)'-', (byte)'-' };
    }

    enum MimeWriterState
    {
        Start,
        StartPreface,
        StartPart,
        Header,
        Content,
        Closed,
    }

    internal class MimeWriter
    {
        Stream stream;
        byte[] boundaryBytes;
        MimeWriterState state;
        BufferedWrite bufferedWrite;
        Stream contentStream;

        internal MimeWriter(Stream stream, string boundary)
        {
            if (stream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            if (boundary == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("boundary");

            this.stream = stream;
            this.boundaryBytes = MimeWriter.GetBoundaryBytes(boundary);
            this.state = MimeWriterState.Start;
            this.bufferedWrite = new BufferedWrite();
        }

        internal static int GetHeaderSize(string name, string value, int maxSizeInBytes)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            if (value == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            int size = XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, 0, MimeGlobals.COLONSPACE.Length + MimeGlobals.CRLF.Length);
            size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, name.Length);
            size += XmlMtomWriter.ValidateSizeOfMessage(maxSizeInBytes, size, value.Length);
            return size;
        }

        internal static byte[] GetBoundaryBytes(string boundary)
        {
            byte[] boundaryBytes = new byte[boundary.Length + MimeGlobals.BoundaryPrefix.Length];
            for (int i = 0; i < MimeGlobals.BoundaryPrefix.Length; i++)
                boundaryBytes[i] = MimeGlobals.BoundaryPrefix[i];
            Encoding.ASCII.GetBytes(boundary, 0, boundary.Length, boundaryBytes, MimeGlobals.BoundaryPrefix.Length);
            return boundaryBytes;
        }

        internal MimeWriterState WriteState
        {
            get
            {
                return state;
            }
        }

        internal int GetBoundarySize()
        {
            return this.boundaryBytes.Length;
        }

        internal void StartPreface()
        {
            if (state != MimeWriterState.Start)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeWriterInvalidStateForStartPreface, state.ToString())));

            state = MimeWriterState.StartPreface;
        }

        internal void StartPart()
        {
            switch (state)
            {
                case MimeWriterState.StartPart:
                case MimeWriterState.Closed:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeWriterInvalidStateForStartPart, state.ToString())));
                default:
                    break;
            }

            state = MimeWriterState.StartPart;

            if (contentStream != null)
            {
                contentStream.Flush();
                contentStream = null;
            }

            bufferedWrite.Write(boundaryBytes);
            bufferedWrite.Write(MimeGlobals.CRLF);
        }

        internal void Close()
        {
            switch (state)
            {
                case MimeWriterState.Closed:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeWriterInvalidStateForClose, state.ToString())));
                default:
                    break;
            }

            state = MimeWriterState.Closed;

            if (contentStream != null)
            {
                contentStream.Flush();
                contentStream = null;
            }

            bufferedWrite.Write(boundaryBytes);
            bufferedWrite.Write(MimeGlobals.DASHDASH);
            bufferedWrite.Write(MimeGlobals.CRLF);

            Flush();
        }

        void Flush()
        {
            if (bufferedWrite.Length > 0)
            {
                stream.Write(bufferedWrite.GetBuffer(), 0, bufferedWrite.Length);
                bufferedWrite.Reset();
            }
        }

        internal void WriteHeader(string name, string value)
        {
            if (name == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            if (value == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");

            switch (state)
            {
                case MimeWriterState.Start:
                case MimeWriterState.Content:
                case MimeWriterState.Closed:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeWriterInvalidStateForHeader, state.ToString())));
                default:
                    break;
            }

            state = MimeWriterState.Header;

            bufferedWrite.Write(name);
            bufferedWrite.Write(MimeGlobals.COLONSPACE);
            bufferedWrite.Write(value);
            bufferedWrite.Write(MimeGlobals.CRLF);
        }

        internal Stream GetContentStream()
        {
            switch (state)
            {
                case MimeWriterState.Start:
                case MimeWriterState.Content:
                case MimeWriterState.Closed:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MimeWriterInvalidStateForContent, state.ToString())));
                default:
                    break;
            }

            state = MimeWriterState.Content;

            bufferedWrite.Write(MimeGlobals.CRLF);

            Flush();

            contentStream = stream;

            return contentStream;
        }
    }

    internal class BufferedWrite
    {
        byte[] buffer;
        int offset;

        internal BufferedWrite()
            : this(256)
        {
        }

        internal BufferedWrite(int initialSize)
        {
            buffer = new byte[initialSize];
        }

        void EnsureBuffer(int count)
        {
            int currSize = buffer.Length;
            if (count > currSize - offset)
            {
                int newSize = currSize;
                do
                {
                    if (newSize == Int32.MaxValue)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.WriteBufferOverflow)));
                    newSize = (newSize < Int32.MaxValue / 2) ? newSize * 2 : Int32.MaxValue;
                }
                while (count > newSize - offset);
                byte[] newBuffer = new byte[newSize];
                Buffer.BlockCopy(buffer, 0, newBuffer, 0, offset);
                buffer = newBuffer;
            }
        }

        internal int Length
        {
            get
            {
                return offset;
            }
        }

        internal byte[] GetBuffer()
        {
            return buffer;
        }

        internal void Reset()
        {
            offset = 0;
        }

        internal void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        internal void Write(byte[] value, int index, int count)
        {
            EnsureBuffer(count);
            Buffer.BlockCopy(value, index, buffer, offset, count);
            offset += count;
        }

        internal void Write(string value)
        {
            Write(value, 0, value.Length);
        }

        internal void Write(string value, int index, int count)
        {
            EnsureBuffer(count);
            for (int i = 0; i < count; i++)
            {
                char c = value[index + i];
                if ((ushort)c > 0xFF)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, c, ((int)c).ToString("X", CultureInfo.InvariantCulture))));
                buffer[offset + i] = (byte)c;
            }
            offset += count;
        }

#if NO
        internal void Write(byte value)
        {
            EnsureBuffer(1);
            buffer[offset++] = value;
        }

        internal void Write(char value)
        {
            EnsureBuffer(1);
            if ((ushort)value > 0xFF)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, value, ((int)value).ToString("X", CultureInfo.InvariantCulture)))));
            buffer[offset++] = (byte)value;
        }

        internal void Write(int value)
        {
            Write(value.ToString());
        }

        internal void Write(char[] value)
        {
            Write(value, 0, value.Length);
        }

        internal void Write(char[] value, int index, int count)
        {
            EnsureBuffer(count);
            for (int i = 0; i < count; i++)
            {
                char c = value[index + i];
                if ((ushort)c > 0xFF)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.MimeHeaderInvalidCharacter, c, ((int)c).ToString("X", CultureInfo.InvariantCulture)))));
                buffer[offset + i] = (byte)c;
            }
            offset += count;
        }

#endif

    }

    enum MtomBinaryDataType { Provider, Segment }

    class MtomBinaryData
    {
        internal MtomBinaryDataType type;

        internal IStreamProvider provider;
        internal byte[] chunk;

        internal MtomBinaryData(IStreamProvider provider)
        {
            this.type = MtomBinaryDataType.Provider;
            this.provider = provider;
        }

        internal MtomBinaryData(byte[] buffer, int offset, int count)
        {
            this.type = MtomBinaryDataType.Segment;
            this.chunk = new byte[count];
            Buffer.BlockCopy(buffer, offset, this.chunk, 0, count);
        }

        internal long Length
        {
            get
            {
                if (this.type == MtomBinaryDataType.Segment)
                    return this.chunk.Length;

                return -1;
            }
        }
    }
}

