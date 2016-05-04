//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xml
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Diagnostics;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Globalization;

    public delegate void OnXmlDictionaryReaderClose(XmlDictionaryReader reader);

    public abstract class XmlDictionaryReader : XmlReader
    {
        internal const int MaxInitialArrayLength = 65535;

        static public XmlDictionaryReader CreateDictionaryReader(XmlReader reader)
        {
            if (reader == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");

            XmlDictionaryReader dictionaryReader = reader as XmlDictionaryReader;

            if (dictionaryReader == null)
            {
                dictionaryReader = new XmlWrappedReader(reader, null);
            }

            return dictionaryReader;
        }

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, XmlDictionaryReaderQuotas quotas)
        {
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            return CreateBinaryReader(buffer, 0, buffer.Length, quotas);
        }

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(buffer, offset, count, null, quotas);
        }

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(buffer, offset, count, dictionary, quotas, null);
        }

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session)
        {
            return CreateBinaryReader(buffer, offset, count, dictionary, quotas, session, null);
        }

        public static XmlDictionaryReader CreateBinaryReader(byte[] buffer, int offset, int count,
                                                             IXmlDictionary dictionary,
                                                             XmlDictionaryReaderQuotas quotas,
                                                             XmlBinaryReaderSession session,
                                                             OnXmlDictionaryReaderClose onClose)
        {
            XmlBinaryReader reader = new XmlBinaryReader();
            reader.SetInput(buffer, offset, count, dictionary, quotas, session, onClose);
            return reader;
        }

        public static XmlDictionaryReader CreateBinaryReader(Stream stream, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(stream, null, quotas);
        }

        public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas)
        {
            return CreateBinaryReader(stream, dictionary, quotas, null);
        }

        public static XmlDictionaryReader CreateBinaryReader(Stream stream, IXmlDictionary dictionary, XmlDictionaryReaderQuotas quotas, XmlBinaryReaderSession session)
        {
            return CreateBinaryReader(stream, dictionary, quotas, session, null);
        }

        public static XmlDictionaryReader CreateBinaryReader(Stream stream,
                                                             IXmlDictionary dictionary,
                                                             XmlDictionaryReaderQuotas quotas,
                                                             XmlBinaryReaderSession session,
                                                             OnXmlDictionaryReaderClose onClose)
        {
            XmlBinaryReader reader = new XmlBinaryReader();
            reader.SetInput(stream, dictionary, quotas, session, onClose);
            return reader;
        }

        static public XmlDictionaryReader CreateTextReader(byte[] buffer, XmlDictionaryReaderQuotas quotas)
        {
            if (buffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            return CreateTextReader(buffer, 0, buffer.Length, quotas);
        }

        static public XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count, XmlDictionaryReaderQuotas quotas)
        {
            return CreateTextReader(buffer, offset, count, null, quotas, null);
        }

        static public XmlDictionaryReader CreateTextReader(byte[] buffer, int offset, int count,
                                                           Encoding encoding,
                                                           XmlDictionaryReaderQuotas quotas,
                                                           OnXmlDictionaryReaderClose onClose)
        {
            XmlUTF8TextReader reader = new XmlUTF8TextReader();
            reader.SetInput(buffer, offset, count, encoding, quotas, onClose);
            return reader;
        }

        static public XmlDictionaryReader CreateTextReader(Stream stream, XmlDictionaryReaderQuotas quotas)
        {
            return CreateTextReader(stream, null, quotas, null);
        }

        static public XmlDictionaryReader CreateTextReader(Stream stream, Encoding encoding,
                                                           XmlDictionaryReaderQuotas quotas,
                                                           OnXmlDictionaryReaderClose onClose)
        {
            XmlUTF8TextReader reader = new XmlUTF8TextReader();
            reader.SetInput(stream, encoding, quotas, onClose);
            return reader;
        }

        static public XmlDictionaryReader CreateMtomReader(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas)
        {
            if (encoding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");

            return CreateMtomReader(stream, new Encoding[1] { encoding }, quotas);
        }

        static public XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(stream, encodings, null, quotas);
        }

        static public XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(stream, encodings, contentType, quotas, int.MaxValue, null);
        }

        static public XmlDictionaryReader CreateMtomReader(Stream stream, Encoding[] encodings, string contentType,
            XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            XmlMtomReader reader = new XmlMtomReader();
            reader.SetInput(stream, encodings, contentType, quotas, maxBufferSize, onClose);
            return reader;
        }

        static public XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas)
        {
            if (encoding == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");

            return CreateMtomReader(buffer, offset, count, new Encoding[1] { encoding }, quotas);
        }

        static public XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(buffer, offset, count, encodings, null, quotas);
        }

        static public XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType, XmlDictionaryReaderQuotas quotas)
        {
            return CreateMtomReader(buffer, offset, count, encodings, contentType, quotas, int.MaxValue, null);
        }

        static public XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding[] encodings, string contentType,
            XmlDictionaryReaderQuotas quotas, int maxBufferSize, OnXmlDictionaryReaderClose onClose)
        {
            XmlMtomReader reader = new XmlMtomReader();
            reader.SetInput(buffer, offset, count, encodings, contentType, quotas, maxBufferSize, onClose);
            return reader;
        }

#if NO
        static XmlDictionaryReader CreateMtomReader(byte[] buffer, int offset, int count, Encoding encoding)
        {
            return XmlMtomWriter.IsUTF8Encoding(encoding) ? new XmlUTF8TextReader(buffer, offset, count)
                : CreateReader(new MemoryStream(buffer, offset, count), encoding);
        }
#endif
        public virtual bool CanCanonicalize
        {
            get
            {
                return false;
            }
        }

        public virtual XmlDictionaryReaderQuotas Quotas
        {
            get
            {
                return XmlDictionaryReaderQuotas.Max;
            }
        }

        public virtual void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void EndCanonicalization()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual void MoveToStartElement()
        {
            if (!IsStartElement())
                XmlExceptionHelper.ThrowStartElementExpected(this);
        }

        public virtual void MoveToStartElement(string name)
        {
            if (!IsStartElement(name))
                XmlExceptionHelper.ThrowStartElementExpected(this, name);
        }

        public virtual void MoveToStartElement(string localName, string namespaceUri)
        {
            if (!IsStartElement(localName, namespaceUri))
                XmlExceptionHelper.ThrowStartElementExpected(this, localName, namespaceUri);
        }

        public virtual void MoveToStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (!IsStartElement(localName, namespaceUri))
                XmlExceptionHelper.ThrowStartElementExpected(this, localName, namespaceUri);
        }

        public virtual bool IsLocalName(string localName)
        {
            return this.LocalName == localName;
        }

        public virtual bool IsLocalName(XmlDictionaryString localName)
        {
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");

            return IsLocalName(localName.Value);
        }

        public virtual bool IsNamespaceUri(string namespaceUri)
        {
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            return this.NamespaceURI == namespaceUri;
        }

        public virtual bool IsNamespaceUri(XmlDictionaryString namespaceUri)
        {
            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");
            return IsNamespaceUri(namespaceUri.Value);
        }

        public virtual void ReadFullStartElement()
        {
            MoveToStartElement();
            if (IsEmptyElement)
                XmlExceptionHelper.ThrowFullStartElementExpected(this);
            Read();
        }

        public virtual void ReadFullStartElement(string name)
        {
            MoveToStartElement(name);
            if (IsEmptyElement)
                XmlExceptionHelper.ThrowFullStartElementExpected(this, name);
            Read();
        }

        public virtual void ReadFullStartElement(string localName, string namespaceUri)
        {
            MoveToStartElement(localName, namespaceUri);
            if (IsEmptyElement)
                XmlExceptionHelper.ThrowFullStartElementExpected(this, localName, namespaceUri);
            Read();
        }

        public virtual void ReadFullStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            MoveToStartElement(localName, namespaceUri);
            if (IsEmptyElement)
                XmlExceptionHelper.ThrowFullStartElementExpected(this, localName, namespaceUri);
            Read();
        }

        public virtual void ReadStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            MoveToStartElement(localName, namespaceUri);
            Read();
        }

        public virtual bool IsStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return IsStartElement(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        public virtual int IndexOfLocalName(string[] localNames, string namespaceUri)
        {
            if (localNames == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");

            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");

            if (this.NamespaceURI == namespaceUri)
            {
                string localName = this.LocalName;
                for (int i = 0; i < localNames.Length; i++)
                {
                    string value = localNames[i];
                    if (value == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
                    if (localName == value)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public virtual int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString namespaceUri)
        {
            if (localNames == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localNames");

            if (namespaceUri == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceUri");

            if (this.NamespaceURI == namespaceUri.Value)
            {
                string localName = this.LocalName;
                for (int i = 0; i < localNames.Length; i++)
                {
                    XmlDictionaryString value = localNames[i];
                    if (value == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "localNames[{0}]", i));
                    if (localName == value.Value)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public virtual string GetAttribute(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return GetAttribute(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri));
        }

        public virtual bool TryGetBase64ContentLength(out int length)
        {
            length = 0;
            return false;
        }

        public virtual int ReadValueAsBase64(byte[] buffer, int offset, int count)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public virtual byte[] ReadContentAsBase64()
        {
            return ReadContentAsBase64(Quotas.MaxArrayLength, MaxInitialArrayLength);
        }

        internal byte[] ReadContentAsBase64(int maxByteArrayContentLength, int maxInitialCount)
        {
            int length;
            if (TryGetBase64ContentLength(out length))
            {
                if (length > maxByteArrayContentLength)
                    XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, maxByteArrayContentLength);

                if (length <= maxInitialCount)
                {
                    byte[] buffer = new byte[length];
                    int read = 0;
                    while (read < length)
                    {
                        int actual = ReadContentAsBase64(buffer, read, length - read);
                        if (actual == 0)
                            XmlExceptionHelper.ThrowBase64DataExpected(this);
                        read += actual;
                    }
                    return buffer;
                }
            }
            return ReadContentAsBytes(true, maxByteArrayContentLength);
        }

        public override string ReadContentAsString()
        {
            return ReadContentAsString(Quotas.MaxStringContentLength);
        }

        protected string ReadContentAsString(int maxStringContentLength)
        {
            StringBuilder sb = null;
            string result = string.Empty;
            bool done = false;
            while (true)
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Attribute:
                        result = this.Value;
                        break;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.CDATA:
                        // merge text content
                        string value = this.Value;
                        if (result.Length == 0)
                        {
                            result = value;
                        }
                        else
                        {
                            if (sb == null)
                                sb = new StringBuilder(result);
                            if (sb.Length > maxStringContentLength - value.Length)
                                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
                            sb.Append(value);
                        }
                        break;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        // skip comments, pis and end entity nodes
                        break;
                    case XmlNodeType.EntityReference:
                        if (this.CanResolveEntity)
                        {
                            this.ResolveEntity();
                            break;
                        }
                        goto default;
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                    default:
                        done = true;
                        break;
                }
                if (done)
                    break;
                if (this.AttributeCount != 0)
                    ReadAttributeValue();
                else
                    Read();
            }
            if (sb != null)
                result = sb.ToString();
            if (result.Length > maxStringContentLength)
                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
            return result;
        }

        public override string ReadString()
        {
            return ReadString(Quotas.MaxStringContentLength);
        }

        protected string ReadString(int maxStringContentLength)
        {
            if (this.ReadState != ReadState.Interactive)
                return string.Empty;
            if (this.NodeType != XmlNodeType.Element)
                MoveToElement();
            if (this.NodeType == XmlNodeType.Element)
            {
                if (this.IsEmptyElement)
                    return string.Empty;
                if (!Read())
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidOperation)));
                if (this.NodeType == XmlNodeType.EndElement)
                    return string.Empty;
            }
            StringBuilder sb = null;
            string result = string.Empty;
            while (IsTextNode(this.NodeType))
            {
                string value = this.Value;
                if (result.Length == 0)
                {
                    result = value;
                }
                else
                {
                    if (sb == null)
                        sb = new StringBuilder(result);
                    if (sb.Length > maxStringContentLength - value.Length)
                        XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
                    sb.Append(value);
                }
                if (!Read())
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlInvalidOperation)));
            }
            if (sb != null)
                result = sb.ToString();
            if (result.Length > maxStringContentLength)
                XmlExceptionHelper.ThrowMaxStringContentLengthExceeded(this, maxStringContentLength);
            return result;
        }

        public virtual byte[] ReadContentAsBinHex()
        {
            return ReadContentAsBinHex(Quotas.MaxArrayLength);
        }

        protected byte[] ReadContentAsBinHex(int maxByteArrayContentLength)
        {
            return ReadContentAsBytes(false, maxByteArrayContentLength);
        }

        byte[] ReadContentAsBytes(bool base64, int maxByteArrayContentLength)
        {
            byte[][] buffers = new byte[32][];
            byte[] buffer;
            // Its best to read in buffers that are a multiple of 3 so we don't break base64 boundaries when converting text
            int count = 384;
            int bufferCount = 0;
            int totalRead = 0;
            while (true)
            {
                buffer = new byte[count];
                buffers[bufferCount++] = buffer;
                int read = 0;
                while (read < buffer.Length)
                {
                    int actual;
                    if (base64)
                        actual = ReadContentAsBase64(buffer, read, buffer.Length - read);
                    else
                        actual = ReadContentAsBinHex(buffer, read, buffer.Length - read);
                    if (actual == 0)
                        break;
                    read += actual;
                }
                if (totalRead > maxByteArrayContentLength - read)
                    XmlExceptionHelper.ThrowMaxArrayLengthExceeded(this, maxByteArrayContentLength);
                totalRead += read;
                if (read < buffer.Length)
                    break;
                count = count * 2;
            }
            buffer = new byte[totalRead];
            int offset = 0;
            for (int i = 0; i < bufferCount - 1; i++)
            {
                Buffer.BlockCopy(buffers[i], 0, buffer, offset, buffers[i].Length);
                offset += buffers[i].Length;
            }
            Buffer.BlockCopy(buffers[bufferCount - 1], 0, buffer, offset, totalRead - offset);
            return buffer;
        }

        protected bool IsTextNode(XmlNodeType nodeType)
        {
            return nodeType == XmlNodeType.Text ||
                nodeType == XmlNodeType.Whitespace ||
                nodeType == XmlNodeType.SignificantWhitespace ||
                nodeType == XmlNodeType.CDATA ||
                nodeType == XmlNodeType.Attribute;
        }

        public virtual int ReadContentAsChars(char[] chars, int offset, int count)
        {
            int read = 0;
            while (true)
            {
                XmlNodeType nodeType = this.NodeType;

                if (nodeType == XmlNodeType.Element || nodeType == XmlNodeType.EndElement)
                    break;

                if (IsTextNode(nodeType))
                {
                    read = ReadValueChunk(chars, offset, count);

                    if (read > 0)
                        break;

                    if (nodeType == XmlNodeType.Attribute /* || inAttributeText */)
                        break;

                    if (!Read())
                        break;
                }
                else
                {
                    if (!Read())
                        break;
                }
            }

            return read;
        }

        public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
        {
            if (type == typeof(Guid[]))
            {
                string[] values = (string[])ReadContentAs(typeof(string[]), namespaceResolver);
                Guid[] guids = new Guid[values.Length];
                for (int i = 0; i < values.Length; i++)
                    guids[i] = XmlConverter.ToGuid(values[i]);
                return guids;
            }
            if (type == typeof(UniqueId[]))
            {
                string[] values = (string[])ReadContentAs(typeof(string[]), namespaceResolver);
                UniqueId[] uniqueIds = new UniqueId[values.Length];
                for (int i = 0; i < values.Length; i++)
                    uniqueIds[i] = XmlConverter.ToUniqueId(values[i]);
                return uniqueIds;
            }
            return base.ReadContentAs(type, namespaceResolver);
        }

        public virtual string ReadContentAsString(string[] strings, out int index)
        {
            if (strings == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("strings");
            string s = ReadContentAsString();
            index = -1;
            for (int i = 0; i < strings.Length; i++)
            {
                string value = strings[i];
                if (value == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "strings[{0}]", i));
                if (value == s)
                {
                    index = i;
                    return value;
                }
            }
            return s;
        }

        public virtual string ReadContentAsString(XmlDictionaryString[] strings, out int index)
        {
            if (strings == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("strings");
            string s = ReadContentAsString();
            index = -1;
            for (int i = 0; i < strings.Length; i++)
            {
                XmlDictionaryString value = strings[i];
                if (value == null)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(string.Format(CultureInfo.InvariantCulture, "strings[{0}]", i));
                if (value.Value == s)
                {
                    index = i;
                    return value.Value;
                }
            }
            return s;
        }

        public override decimal ReadContentAsDecimal()
        {
            return XmlConverter.ToDecimal(ReadContentAsString());
        }

        public override Single ReadContentAsFloat()
        {
            return XmlConverter.ToSingle(ReadContentAsString());
        }

        public virtual UniqueId ReadContentAsUniqueId()
        {
            return XmlConverter.ToUniqueId(ReadContentAsString());
        }

        public virtual Guid ReadContentAsGuid()
        {
            return XmlConverter.ToGuid(ReadContentAsString());
        }

        public virtual TimeSpan ReadContentAsTimeSpan()
        {
            return XmlConverter.ToTimeSpan(ReadContentAsString());
        }

        public virtual void ReadContentAsQualifiedName(out string localName, out string namespaceUri)
        {
            string prefix;
            XmlConverter.ToQualifiedName(ReadContentAsString(), out prefix, out localName);
            namespaceUri = LookupNamespace(prefix);
            if (namespaceUri == null)
                XmlExceptionHelper.ThrowUndefinedPrefix(this, prefix);
        }

        /* string, bool, int, long, float, double, decimal, DateTime, base64, binhex, uniqueID, object, list*/
        public override string ReadElementContentAsString()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            string value;

            if (isEmptyElement)
            {
                Read();
                value = string.Empty;
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsString();
                ReadEndElement();
            }

            return value;
        }

        public override bool ReadElementContentAsBoolean()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            bool value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToBoolean(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsBoolean();
                ReadEndElement();
            }

            return value;
        }

        public override int ReadElementContentAsInt()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            int value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToInt32(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsInt();
                ReadEndElement();
            }

            return value;
        }

        public override long ReadElementContentAsLong()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            long value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToInt64(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsLong();
                ReadEndElement();
            }

            return value;
        }

        public override float ReadElementContentAsFloat()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            float value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToSingle(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsFloat();
                ReadEndElement();
            }

            return value;
        }

        public override double ReadElementContentAsDouble()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            double value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToDouble(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsDouble();
                ReadEndElement();
            }

            return value;
        }

        public override decimal ReadElementContentAsDecimal()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            decimal value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToDecimal(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsDecimal();
                ReadEndElement();
            }

            return value;
        }

        public override DateTime ReadElementContentAsDateTime()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            DateTime value;

            if (isEmptyElement)
            {
                Read();
                try
                {
                    value = DateTime.Parse(string.Empty, NumberFormatInfo.InvariantInfo);
                }
                catch (ArgumentException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "DateTime", exception));
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "DateTime", exception));
                }
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsDateTime();
                ReadEndElement();
            }

            return value;
        }

        public virtual UniqueId ReadElementContentAsUniqueId()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            UniqueId value;

            if (isEmptyElement)
            {
                Read();
                try
                {
                    value = new UniqueId(string.Empty);
                }
                catch (ArgumentException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "UniqueId", exception));
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "UniqueId", exception));
                }
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsUniqueId();
                ReadEndElement();
            }

            return value;
        }

        [SuppressMessage("Reliability", "Reliability113", Justification = "Catching expected exceptions inline instead of calling Fx.CreateGuid to minimize code change")]
        public virtual Guid ReadElementContentAsGuid()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            Guid value;

            if (isEmptyElement)
            {
                Read();
                try
                {
                    value = Guid.Empty;
                }
                catch (ArgumentException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception));
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception));
                }
                catch (OverflowException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(string.Empty, "Guid", exception));
                }
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsGuid();
                ReadEndElement();
            }

            return value;
        }

        public virtual TimeSpan ReadElementContentAsTimeSpan()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            TimeSpan value;

            if (isEmptyElement)
            {
                Read();
                value = XmlConverter.ToTimeSpan(string.Empty);
            }
            else
            {
                ReadStartElement();
                value = ReadContentAsTimeSpan();
                ReadEndElement();
            }

            return value;
        }

        public virtual byte[] ReadElementContentAsBase64()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            byte[] buffer;

            if (isEmptyElement)
            {
                Read();
                buffer = new byte[0];
            }
            else
            {
                ReadStartElement();
                buffer = ReadContentAsBase64();
                ReadEndElement();
            }

            return buffer;
        }

        public virtual byte[] ReadElementContentAsBinHex()
        {
            bool isEmptyElement = IsStartElement() && IsEmptyElement;
            byte[] buffer;

            if (isEmptyElement)
            {
                Read();
                buffer = new byte[0];
            }
            else
            {
                ReadStartElement();
                buffer = ReadContentAsBinHex();
                ReadEndElement();
            }

            return buffer;
        }

        public virtual void GetNonAtomizedNames(out string localName, out string namespaceUri)
        {
            localName = this.LocalName;
            namespaceUri = this.NamespaceURI;
        }

        public virtual bool TryGetLocalNameAsDictionaryString(out XmlDictionaryString localName)
        {
            localName = null;
            return false;
        }

        public virtual bool TryGetNamespaceUriAsDictionaryString(out XmlDictionaryString namespaceUri)
        {
            namespaceUri = null;
            return false;
        }

        public virtual bool TryGetValueAsDictionaryString(out XmlDictionaryString value)
        {
            value = null;
            return false;
        }

        void CheckArray(Array array, int offset, int count)
        {
            if (array == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("array"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > array.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, array.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > array.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, array.Length - offset)));
        }

        public virtual bool IsStartArray(out Type type)
        {
            type = null;
            return false;
        }

        public virtual bool TryGetArrayLength(out int count)
        {
            count = 0;
            return false;
        }

        // Boolean
        public virtual bool[] ReadBooleanArray(string localName, string namespaceUri)
        {
            return BooleanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual bool[] ReadBooleanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return BooleanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsBoolean();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Int16
        public virtual Int16[] ReadInt16Array(string localName, string namespaceUri)
        {
            return Int16ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual Int16[] ReadInt16Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int16ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, Int16[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                int i = ReadElementContentAsInt();
                if (i < Int16.MinValue || i > Int16.MaxValue)
                    XmlExceptionHelper.ThrowConversionOverflow(this, i.ToString(NumberFormatInfo.CurrentInfo), "Int16");
                array[offset + actual] = (Int16)i;
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Int16[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Int32
        public virtual Int32[] ReadInt32Array(string localName, string namespaceUri)
        {
            return Int32ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual Int32[] ReadInt32Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int32ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, Int32[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsInt();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Int32[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Int64
        public virtual Int64[] ReadInt64Array(string localName, string namespaceUri)
        {
            return Int64ArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual Int64[] ReadInt64Array(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return Int64ArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, Int64[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsLong();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Int64[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Single
        public virtual float[] ReadSingleArray(string localName, string namespaceUri)
        {
            return SingleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual float[] ReadSingleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return SingleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, float[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsFloat();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Double
        public virtual double[] ReadDoubleArray(string localName, string namespaceUri)
        {
            return DoubleArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual double[] ReadDoubleArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DoubleArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, double[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsDouble();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Decimal
        public virtual decimal[] ReadDecimalArray(string localName, string namespaceUri)
        {
            return DecimalArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual decimal[] ReadDecimalArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DecimalArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsDecimal();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // DateTime
        public virtual DateTime[] ReadDateTimeArray(string localName, string namespaceUri)
        {
            return DateTimeArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual DateTime[] ReadDateTimeArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsDateTime();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // Guid
        public virtual Guid[] ReadGuidArray(string localName, string namespaceUri)
        {
            return GuidArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual Guid[] ReadGuidArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return GuidArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsGuid();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        // TimeSpan
        public virtual TimeSpan[] ReadTimeSpanArray(string localName, string namespaceUri)
        {
            return TimeSpanArrayHelperWithString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual TimeSpan[] ReadTimeSpanArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            return TimeSpanArrayHelperWithDictionaryString.Instance.ReadArray(this, localName, namespaceUri, Quotas.MaxArrayLength);
        }

        public virtual int ReadArray(string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            CheckArray(array, offset, count);
            int actual = 0;
            while (actual < count && IsStartElement(localName, namespaceUri))
            {
                array[offset + actual] = ReadElementContentAsTimeSpan();
                actual++;
            }
            return actual;
        }

        public virtual int ReadArray(XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            return ReadArray(XmlDictionaryString.GetString(localName), XmlDictionaryString.GetString(namespaceUri), array, offset, count);
        }

        class XmlWrappedReader : XmlDictionaryReader, IXmlLineInfo
        {
            XmlReader reader;
            XmlNamespaceManager nsMgr;

            public XmlWrappedReader(XmlReader reader, XmlNamespaceManager nsMgr)
            {
                this.reader = reader;
                this.nsMgr = nsMgr;
            }

            public override int AttributeCount
            {
                get
                {
                    return reader.AttributeCount;
                }
            }

            public override string BaseURI
            {
                get
                {
                    return reader.BaseURI;
                }
            }

            public override bool CanReadBinaryContent
            {
                get { return reader.CanReadBinaryContent; }
            }

            public override bool CanReadValueChunk
            {
                get { return reader.CanReadValueChunk; }
            }

            public override void Close()
            {
                reader.Close();
                nsMgr = null;
            }

            public override int Depth
            {
                get
                {
                    return reader.Depth;
                }
            }

            public override bool EOF
            {
                get
                {
                    return reader.EOF;
                }
            }

            public override string GetAttribute(int index)
            {
                return reader.GetAttribute(index);
            }

            public override string GetAttribute(string name)
            {
                return reader.GetAttribute(name);
            }

            public override string GetAttribute(string name, string namespaceUri)
            {
                return reader.GetAttribute(name, namespaceUri);
            }

            public override bool HasValue
            {
                get
                {
                    return reader.HasValue;
                }
            }

            public override bool IsDefault
            {
                get
                {
                    return reader.IsDefault;
                }
            }

            public override bool IsEmptyElement
            {
                get
                {
                    return reader.IsEmptyElement;
                }
            }

            public override bool IsStartElement(string name)
            {
                return reader.IsStartElement(name);
            }

            public override bool IsStartElement(string localName, string namespaceUri)
            {
                return reader.IsStartElement(localName, namespaceUri);
            }

            public override string LocalName
            {
                get
                {
                    return reader.LocalName;
                }
            }

            public override string LookupNamespace(string namespaceUri)
            {
                return reader.LookupNamespace(namespaceUri);
            }

            public override void MoveToAttribute(int index)
            {
                reader.MoveToAttribute(index);
            }

            public override bool MoveToAttribute(string name)
            {
                return reader.MoveToAttribute(name);
            }

            public override bool MoveToAttribute(string name, string namespaceUri)
            {
                return reader.MoveToAttribute(name, namespaceUri);
            }

            public override bool MoveToElement()
            {
                return reader.MoveToElement();
            }

            public override bool MoveToFirstAttribute()
            {
                return reader.MoveToFirstAttribute();
            }

            public override bool MoveToNextAttribute()
            {
                return reader.MoveToNextAttribute();
            }

            public override string Name
            {
                get
                {
                    return reader.Name;
                }
            }

            public override string NamespaceURI
            {
                get
                {
                    return reader.NamespaceURI;
                }
            }

            public override XmlNameTable NameTable
            {
                get
                {
                    return reader.NameTable;
                }
            }

            public override XmlNodeType NodeType
            {
                get
                {
                    return reader.NodeType;
                }
            }

            public override string Prefix
            {
                get
                {
                    return reader.Prefix;
                }
            }

            public override char QuoteChar
            {
                get
                {
                    return reader.QuoteChar;
                }
            }

            public override bool Read()
            {
                return reader.Read();
            }

            public override bool ReadAttributeValue()
            {
                return reader.ReadAttributeValue();
            }

            public override string ReadElementString(string name)
            {
                return reader.ReadElementString(name);
            }

            public override string ReadElementString(string localName, string namespaceUri)
            {
                return reader.ReadElementString(localName, namespaceUri);
            }

            public override string ReadInnerXml()
            {
                return reader.ReadInnerXml();
            }

            public override string ReadOuterXml()
            {
                return reader.ReadOuterXml();
            }

            public override void ReadStartElement(string name)
            {
                reader.ReadStartElement(name);
            }

            public override void ReadStartElement(string localName, string namespaceUri)
            {
                reader.ReadStartElement(localName, namespaceUri);
            }

            public override void ReadEndElement()
            {
                reader.ReadEndElement();
            }

            public override string ReadString()
            {
                return reader.ReadString();
            }

            public override ReadState ReadState
            {
                get
                {
                    return reader.ReadState;
                }
            }

            public override void ResolveEntity()
            {
                reader.ResolveEntity();
            }

            public override string this[int index]
            {
                get
                {
                    return reader[index];
                }
            }

            public override string this[string name]
            {
                get
                {
                    return reader[name];
                }
            }

            public override string this[string name, string namespaceUri]
            {
                get
                {
                    return reader[name, namespaceUri];
                }
            }

            public override string Value
            {
                get
                {
                    return reader.Value;
                }
            }

            public override string XmlLang
            {
                get
                {
                    return reader.XmlLang;
                }
            }

            public override XmlSpace XmlSpace
            {
                get
                {
                    return reader.XmlSpace;
                }
            }

            public override int ReadElementContentAsBase64(byte[] buffer, int offset, int count)
            {
                return reader.ReadElementContentAsBase64(buffer, offset, count);
            }

            public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
            {
                return reader.ReadContentAsBase64(buffer, offset, count);
            }

            public override int ReadElementContentAsBinHex(byte[] buffer, int offset, int count)
            {
                return reader.ReadElementContentAsBinHex(buffer, offset, count);
            }

            public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
            {
                return reader.ReadContentAsBinHex(buffer, offset, count);
            }

            public override int ReadValueChunk(char[] chars, int offset, int count)
            {
                return reader.ReadValueChunk(chars, offset, count);
            }

            public override Type ValueType
            {
                get
                {
                    return reader.ValueType;
                }
            }

            public override Boolean ReadContentAsBoolean()
            {
                return reader.ReadContentAsBoolean();
            }

            public override DateTime ReadContentAsDateTime()
            {
                return reader.ReadContentAsDateTime();
            }

            public override Decimal ReadContentAsDecimal()
            {
                // return reader.ReadContentAsDecimal();
                return (Decimal)reader.ReadContentAs(typeof(Decimal), null);
            }

            public override Double ReadContentAsDouble()
            {
                return reader.ReadContentAsDouble();
            }

            public override Int32 ReadContentAsInt()
            {
                return reader.ReadContentAsInt();
            }

            public override Int64 ReadContentAsLong()
            {
                return reader.ReadContentAsLong();
            }

            public override Single ReadContentAsFloat()
            {
                return reader.ReadContentAsFloat();
            }

            public override string ReadContentAsString()
            {
                return reader.ReadContentAsString();
            }

            public override object ReadContentAs(Type type, IXmlNamespaceResolver namespaceResolver)
            {
                return reader.ReadContentAs(type, namespaceResolver);
            }

            public bool HasLineInfo()
            {
                IXmlLineInfo lineInfo = reader as IXmlLineInfo;

                if (lineInfo == null)
                    return false;

                return lineInfo.HasLineInfo();
            }

            public int LineNumber
            {
                get
                {
                    IXmlLineInfo lineInfo = reader as IXmlLineInfo;

                    if (lineInfo == null)
                        return 1;

                    return lineInfo.LineNumber;
                }
            }

            public int LinePosition
            {
                get
                {
                    IXmlLineInfo lineInfo = reader as IXmlLineInfo;

                    if (lineInfo == null)
                        return 1;

                    return lineInfo.LinePosition;
                }
            }
        }
    }
}
