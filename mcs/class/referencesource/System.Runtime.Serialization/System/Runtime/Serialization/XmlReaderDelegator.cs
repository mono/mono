//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Reflection;
    using System.Globalization;
    using System.Collections.Generic;

#if USE_REFEMIT
    public class XmlReaderDelegator 
#else
    internal class XmlReaderDelegator
#endif
    {
        protected XmlReader reader;
        protected XmlDictionaryReader dictionaryReader;
        protected bool isEndOfEmptyElement = false;

        public XmlReaderDelegator(XmlReader reader)
        {
            XmlObjectSerializer.CheckNull(reader, "reader");
            this.reader = reader;
            this.dictionaryReader = reader as XmlDictionaryReader;
        }

        internal XmlReader UnderlyingReader
        {
            get { return reader; }
        }

        internal ExtensionDataReader UnderlyingExtensionDataReader
        {
            get { return reader as ExtensionDataReader; }
        }

        internal int AttributeCount
        {
            get { return isEndOfEmptyElement ? 0 : reader.AttributeCount; }
        }

        internal string GetAttribute(string name)
        {
            return isEndOfEmptyElement ? null : reader.GetAttribute(name);
        }

        internal string GetAttribute(string name, string namespaceUri)
        {
            return isEndOfEmptyElement ? null : reader.GetAttribute(name, namespaceUri);
        }

        internal string GetAttribute(int i)
        {
            if (isEndOfEmptyElement)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("i", SR.GetString(SR.XmlElementAttributes)));
            return reader.GetAttribute(i);
        }

        internal bool IsEmptyElement
        {
            get { return false; }
        }

        internal bool IsNamespaceURI(string ns)
        {
            if (dictionaryReader == null)
                return ns == reader.NamespaceURI;
            else
                return dictionaryReader.IsNamespaceUri(ns);
        }

        internal bool IsLocalName(string localName)
        {
            if (dictionaryReader == null)
                return localName == reader.LocalName;
            else
                return dictionaryReader.IsLocalName(localName);
        }

        internal bool IsNamespaceUri(XmlDictionaryString ns)
        {
            if (dictionaryReader == null)
                return ns.Value == reader.NamespaceURI;
            else
                return dictionaryReader.IsNamespaceUri(ns);
        }

        internal bool IsLocalName(XmlDictionaryString localName)
        {
            if (dictionaryReader == null)
                return localName.Value == reader.LocalName;
            else
                return dictionaryReader.IsLocalName(localName);
        }

        internal int IndexOfLocalName(XmlDictionaryString[] localNames, XmlDictionaryString ns)
        {
            if (dictionaryReader != null)
                return dictionaryReader.IndexOfLocalName(localNames, ns);

            if (reader.NamespaceURI == ns.Value)
            {
                string localName = this.LocalName;
                for (int i = 0; i < localNames.Length; i++)
                {
                    if (localName == localNames[i].Value)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public bool IsStartElement()
        {
            return !isEndOfEmptyElement && reader.IsStartElement();
        }

        internal bool IsStartElement(string localname, string ns)
        {
            return !isEndOfEmptyElement && reader.IsStartElement(localname, ns);
        }

        public bool IsStartElement(XmlDictionaryString localname, XmlDictionaryString ns)
        {
            if (dictionaryReader == null)
                return !isEndOfEmptyElement && reader.IsStartElement(localname.Value, ns.Value);
            else
                return !isEndOfEmptyElement && dictionaryReader.IsStartElement(localname, ns);
        }

        internal bool MoveToAttribute(string name)
        {
            return isEndOfEmptyElement ? false : reader.MoveToAttribute(name);
        }

        internal bool MoveToAttribute(string name, string ns)
        {
            return isEndOfEmptyElement ? false : reader.MoveToAttribute(name, ns);
        }

        internal void MoveToAttribute(int i)
        {
            if (isEndOfEmptyElement)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("i", SR.GetString(SR.XmlElementAttributes)));
            reader.MoveToAttribute(i);
        }

        internal bool MoveToElement()
        {
            return isEndOfEmptyElement ? false : reader.MoveToElement();
        }

        internal bool MoveToFirstAttribute()
        {
            return isEndOfEmptyElement ? false : reader.MoveToFirstAttribute();
        }

        internal bool MoveToNextAttribute()
        {
            return isEndOfEmptyElement ? false : reader.MoveToNextAttribute();
        }

        public XmlNodeType NodeType
        {
            get { return isEndOfEmptyElement ? XmlNodeType.EndElement : reader.NodeType; }
        }

        internal bool Read()
        {
            //reader.MoveToFirstAttribute();
            //if (NodeType == XmlNodeType.Attribute)
            reader.MoveToElement();
            if (!reader.IsEmptyElement)
                return reader.Read();
            if (isEndOfEmptyElement)
            {
                isEndOfEmptyElement = false;
                return reader.Read();
            }
            isEndOfEmptyElement = true;
            return true;
        }

#if USE_REFEMIT
        public XmlNodeType MoveToContent()
#else
        internal XmlNodeType MoveToContent()
#endif
        {
            if (isEndOfEmptyElement)
                return XmlNodeType.EndElement;

            return reader.MoveToContent();
        }

        internal bool ReadAttributeValue()
        {
            return isEndOfEmptyElement ? false : reader.ReadAttributeValue();
        }

        public void ReadEndElement()
        {
            if (isEndOfEmptyElement)
                Read();
            else
                reader.ReadEndElement();
        }

        Exception CreateInvalidPrimitiveTypeException(Type type)
        {
            return new InvalidDataContractException(SR.GetString(
                type.IsInterface ? SR.InterfaceTypeCannotBeCreated : SR.InvalidPrimitiveType,
                DataContract.GetClrTypeFullName(type)));
        }

        public object ReadElementContentAsAnyType(Type valueType)
        {
            Read();
            object o = ReadContentAsAnyType(valueType);
            ReadEndElement();
            return o;
        }

        internal object ReadContentAsAnyType(Type valueType)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    return ReadContentAsBoolean();
                case TypeCode.Char:
                    return ReadContentAsChar();
                case TypeCode.Byte:
                    return ReadContentAsUnsignedByte();
                case TypeCode.Int16:
                    return ReadContentAsShort();
                case TypeCode.Int32:
                    return ReadContentAsInt();
                case TypeCode.Int64:
                    return ReadContentAsLong();
                case TypeCode.Single:
                    return ReadContentAsSingle();
                case TypeCode.Double:
                    return ReadContentAsDouble();
                case TypeCode.Decimal:
                    return ReadContentAsDecimal();
                case TypeCode.DateTime:
                    return ReadContentAsDateTime();
                case TypeCode.String:
                    return ReadContentAsString();

                case TypeCode.SByte:
                    return ReadContentAsSignedByte();
                case TypeCode.UInt16:
                    return ReadContentAsUnsignedShort();
                case TypeCode.UInt32:
                    return ReadContentAsUnsignedInt();
                case TypeCode.UInt64:
                    return ReadContentAsUnsignedLong();
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.Object:
                default:
                    if (valueType == Globals.TypeOfByteArray)
                        return ReadContentAsBase64();
                    else if (valueType == Globals.TypeOfObject)
                        return new object();
                    else if (valueType == Globals.TypeOfTimeSpan)
                        return ReadContentAsTimeSpan();
                    else if (valueType == Globals.TypeOfGuid)
                        return ReadContentAsGuid();
                    else if (valueType == Globals.TypeOfUri)
                        return ReadContentAsUri();
                    else if (valueType == Globals.TypeOfXmlQualifiedName)
                        return ReadContentAsQName();
                    break;
            }
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidPrimitiveTypeException(valueType));
        }

        internal IDataNode ReadExtensionData(Type valueType)
        {
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    return new DataNode<bool>(ReadContentAsBoolean());
                case TypeCode.Char:
                    return new DataNode<char>(ReadContentAsChar());
                case TypeCode.Byte:
                    return new DataNode<byte>(ReadContentAsUnsignedByte());
                case TypeCode.Int16:
                    return new DataNode<short>(ReadContentAsShort());
                case TypeCode.Int32:
                    return new DataNode<int>(ReadContentAsInt());
                case TypeCode.Int64:
                    return new DataNode<long>(ReadContentAsLong());
                case TypeCode.Single:
                    return new DataNode<float>(ReadContentAsSingle());
                case TypeCode.Double:
                    return new DataNode<double>(ReadContentAsDouble());
                case TypeCode.Decimal:
                    return new DataNode<decimal>(ReadContentAsDecimal());
                case TypeCode.DateTime:
                    return new DataNode<DateTime>(ReadContentAsDateTime());
                case TypeCode.String:
                    return new DataNode<string>(ReadContentAsString());
                case TypeCode.SByte:
                    return new DataNode<sbyte>(ReadContentAsSignedByte());
                case TypeCode.UInt16:
                    return new DataNode<ushort>(ReadContentAsUnsignedShort());
                case TypeCode.UInt32:
                    return new DataNode<uint>(ReadContentAsUnsignedInt());
                case TypeCode.UInt64:
                    return new DataNode<ulong>(ReadContentAsUnsignedLong());
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.Object:
                default:
                    if (valueType == Globals.TypeOfByteArray)
                        return new DataNode<byte[]>(ReadContentAsBase64());
                    else if (valueType == Globals.TypeOfObject)
                        return new DataNode<object>(new object());
                    else if (valueType == Globals.TypeOfTimeSpan)
                        return new DataNode<TimeSpan>(ReadContentAsTimeSpan());
                    else if (valueType == Globals.TypeOfGuid)
                        return new DataNode<Guid>(ReadContentAsGuid());
                    else if (valueType == Globals.TypeOfUri)
                        return new DataNode<Uri>(ReadContentAsUri());
                    else if (valueType == Globals.TypeOfXmlQualifiedName)
                        return new DataNode<XmlQualifiedName>(ReadContentAsQName());
                    break;
            }
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidPrimitiveTypeException(valueType));
        }

        void ThrowConversionException(string value, string type)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(XmlObjectSerializer.TryAddLineInfo(this, SR.GetString(SR.XmlInvalidConversion, value, type))));
        }

        void ThrowNotAtElement()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlStartElementExpected, "EndElement")));
        }

#if USE_REFEMIT
        public virtual char ReadElementContentAsChar()
#else
        internal virtual char ReadElementContentAsChar()
#endif
        {
            return ToChar(ReadElementContentAsInt());
        }

        internal virtual char ReadContentAsChar()
        {
            return ToChar(ReadContentAsInt());
        }

        char ToChar(int value)
        {
            if (value < char.MinValue || value > char.MaxValue)
            {
                ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Char");
            }
            return (char)value;
        }

        public string ReadElementContentAsString()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsString();
        }

        internal string ReadContentAsString()
        {
            return isEndOfEmptyElement ? String.Empty : reader.ReadContentAsString();
        }

        public bool ReadElementContentAsBoolean()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsBoolean();
        }

        internal bool ReadContentAsBoolean()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "Boolean");

            return reader.ReadContentAsBoolean();
        }

        public float ReadElementContentAsFloat()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsFloat();
        }

        internal float ReadContentAsSingle()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "Float");

            return reader.ReadContentAsFloat();
        }

        public double ReadElementContentAsDouble()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsDouble();
        }

        internal double ReadContentAsDouble()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "Double");

            return reader.ReadContentAsDouble();
        }

        public decimal ReadElementContentAsDecimal()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsDecimal();
        }

        internal decimal ReadContentAsDecimal()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "Decimal");

            return reader.ReadContentAsDecimal();
        }

#if USE_REFEMIT
        public virtual byte[] ReadElementContentAsBase64()
#else
        internal virtual byte[] ReadElementContentAsBase64()
#endif
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            if (dictionaryReader == null)
            {
                return ReadContentAsBase64(reader.ReadElementContentAsString());
            }
            else
            {
                return dictionaryReader.ReadElementContentAsBase64();
            }
        }

#if USE_REFEMIT
        public virtual byte[] ReadContentAsBase64()
#else
        internal virtual byte[] ReadContentAsBase64()
#endif
        {
            if (isEndOfEmptyElement)
                return new byte[0];

            if (dictionaryReader == null)
            {
                return ReadContentAsBase64(reader.ReadContentAsString());
            }
            else
            {
                return dictionaryReader.ReadContentAsBase64();
            }
        }

        internal byte[] ReadContentAsBase64(string str)
        {
            if (str == null)
                return null;
            str = str.Trim();
            if (str.Length == 0)
                return new byte[0];

            try
            {
                return Convert.FromBase64String(str);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "byte[]", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "byte[]", exception));
            }
        }

#if USE_REFEMIT
        public virtual DateTime ReadElementContentAsDateTime()
#else
        internal virtual DateTime ReadElementContentAsDateTime()
#endif
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsDateTime();
        }

        internal virtual DateTime ReadContentAsDateTime()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "DateTime");

            return reader.ReadContentAsDateTime();
        }

        public int ReadElementContentAsInt()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsInt();
        }

        internal int ReadContentAsInt()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "Int32");

            return reader.ReadContentAsInt();
        }

        public long ReadElementContentAsLong()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            return reader.ReadElementContentAsLong();
        }

        internal long ReadContentAsLong()
        {
            if (isEndOfEmptyElement)
                ThrowConversionException(string.Empty, "Int64");

            return reader.ReadContentAsLong();
        }

        public short ReadElementContentAsShort()
        {
            return ToShort(ReadElementContentAsInt());
        }

        internal short ReadContentAsShort()
        {
            return ToShort(ReadContentAsInt());
        }

        short ToShort(int value)
        {
            if (value < short.MinValue || value > short.MaxValue)
            {
                ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Int16");
            }
            return (short)value;
        }

        public byte ReadElementContentAsUnsignedByte()
        {
            return ToByte(ReadElementContentAsInt());
        }

        internal byte ReadContentAsUnsignedByte()
        {
            return ToByte(ReadContentAsInt());
        }

        byte ToByte(int value)
        {
            if (value < byte.MinValue || value > byte.MaxValue)
            {
                ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Byte");
            }
            return (byte)value;
        }

#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public SByte ReadElementContentAsSignedByte()
        {
            return ToSByte(ReadElementContentAsInt());
        }

        internal SByte ReadContentAsSignedByte()
        {
            return ToSByte(ReadContentAsInt());
        }

        SByte ToSByte(int value)
        {
            if (value < SByte.MinValue || value > SByte.MaxValue)
            {
                ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "SByte");
            }
            return (SByte)value;
        }

#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public UInt32 ReadElementContentAsUnsignedInt()
        {
            return ToUInt32(ReadElementContentAsLong());
        }

        internal UInt32 ReadContentAsUnsignedInt()
        {
            return ToUInt32(ReadContentAsLong());
        }

        UInt32 ToUInt32(long value)
        {
            if (value < UInt32.MinValue || value > UInt32.MaxValue)
            {
                ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "UInt32");
            }
            return (UInt32)value;
        }

#if USE_REFEMIT
        [CLSCompliant(false)]
        public virtual UInt64 ReadElementContentAsUnsignedLong()
#else
        internal virtual UInt64 ReadElementContentAsUnsignedLong()
#endif
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            string str = reader.ReadElementContentAsString();

            if (str == null || str.Length == 0)
                ThrowConversionException(string.Empty, "UInt64");

            return XmlConverter.ToUInt64(str);
        }

        internal virtual UInt64 ReadContentAsUnsignedLong()
        {
            string str = reader.ReadContentAsString();

            if (str == null || str.Length == 0)
                ThrowConversionException(string.Empty, "UInt64");

            return XmlConverter.ToUInt64(str);
        }

#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public UInt16 ReadElementContentAsUnsignedShort()
        {
            return ToUInt16(ReadElementContentAsInt());
        }

        internal UInt16 ReadContentAsUnsignedShort()
        {
            return ToUInt16(ReadContentAsInt());
        }

        UInt16 ToUInt16(int value)
        {
            if (value < UInt16.MinValue || value > UInt16.MaxValue)
            {
                ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "UInt16");
            }
            return (UInt16)value;
        }

        public TimeSpan ReadElementContentAsTimeSpan()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            string str = reader.ReadElementContentAsString();
            return XmlConverter.ToTimeSpan(str);
        }

        internal TimeSpan ReadContentAsTimeSpan()
        {
            string str = reader.ReadContentAsString();
            return XmlConverter.ToTimeSpan(str);
        }

        [SuppressMessage("Reliability", "Reliability113", Justification = "Catching expected exceptions inline instead of calling Fx.CreateGuid to minimize code change")]
        public Guid ReadElementContentAsGuid()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            string str = reader.ReadElementContentAsString();
            try
            {
                return Guid.Parse(str);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Guid", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Guid", exception));
            }
            catch (OverflowException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Guid", exception));
            }
        }

        [SuppressMessage("Reliability", "Reliability113", Justification = "Catching expected exceptions inline instead of calling Fx.CreateGuid to minimize code change")]
        internal Guid ReadContentAsGuid()
        {
            string str = reader.ReadContentAsString();
            try
            {
                return Guid.Parse(str);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Guid", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Guid", exception));
            }
            catch (OverflowException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Guid", exception));
            }
        }

        public Uri ReadElementContentAsUri()
        {
            if (isEndOfEmptyElement)
                ThrowNotAtElement();

            string str = ReadElementContentAsString();
            try
            {
                return new Uri(str, UriKind.RelativeOrAbsolute);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Uri", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Uri", exception));
            }
        }

        internal Uri ReadContentAsUri()
        {
            string str = ReadContentAsString();
            try
            {
                return new Uri(str, UriKind.RelativeOrAbsolute);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Uri", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(str, "Uri", exception));
            }
        }

        public XmlQualifiedName ReadElementContentAsQName()
        {
            Read();
            XmlQualifiedName obj = ReadContentAsQName();
            ReadEndElement();
            return obj;
        }

        internal virtual XmlQualifiedName ReadContentAsQName()
        {
            return ParseQualifiedName(ReadContentAsString());
        }

        XmlQualifiedName ParseQualifiedName(string str)
        {
            string name, ns, prefix;
            if (str == null || str.Length == 0)
                name = ns = String.Empty;
            else
                XmlObjectSerializerReadContext.ParseQualifiedName(str, this, out name, out ns, out prefix);
            return new XmlQualifiedName(name, ns);
        }

        void CheckExpectedArrayLength(XmlObjectSerializerReadContext context, int arrayLength)
        {
#if NO
            int readerArrayLength;
            if (dictionaryReader.TryGetArrayLength(out readerArrayLength))
            {
                if (readerArrayLength != arrayLength)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArraySizeXmlMismatch, arrayLength, readerArrayLength)));
            }
#endif
            context.IncrementItemCount(arrayLength);
        }

        protected int GetArrayLengthQuota(XmlObjectSerializerReadContext context)
        {
            if (dictionaryReader.Quotas == null)
                return context.RemainingItemCount;

            return Math.Min(context.RemainingItemCount, dictionaryReader.Quotas.MaxArrayLength);
        }

        void CheckActualArrayLength(int expectedLength, int actualLength, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (expectedLength != actualLength)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArrayExceededSizeAttribute, expectedLength, itemName.Value, itemNamespace.Value)));
        }

        internal bool TryReadBooleanArray(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out bool[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new bool[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = BooleanArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadDateTimeArray(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out DateTime[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new DateTime[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = DateTimeArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadDecimalArray(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out decimal[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new decimal[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = DecimalArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadInt32Array(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out int[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new int[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = Int32ArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadInt64Array(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out long[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new long[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = Int64ArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadSingleArray(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out float[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new float[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = SingleArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal bool TryReadDoubleArray(XmlObjectSerializerReadContext context,
            XmlDictionaryString itemName, XmlDictionaryString itemNamespace,
            int arrayLength, out double[] array)
        {
            if (dictionaryReader == null)
            {
                array = null;
                return false;
            }

            if (arrayLength != -1)
            {
                CheckExpectedArrayLength(context, arrayLength);
                array = new double[arrayLength];
                int read = 0, offset = 0;
                while ((read = dictionaryReader.ReadArray(itemName, itemNamespace, array, offset, arrayLength - offset)) > 0)
                {
                    offset += read;
                }
                CheckActualArrayLength(arrayLength, offset, itemName, itemNamespace);
            }
            else
            {
                array = DoubleArrayHelperWithDictionaryString.Instance.ReadArray(
                    dictionaryReader, itemName, itemNamespace, GetArrayLengthQuota(context));
                context.IncrementItemCount(array.Length);
            }
            return true;
        }

        internal IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return (reader is IXmlNamespaceResolver) ? ((IXmlNamespaceResolver)reader).GetNamespacesInScope(scope) : null;
        }

        // IXmlLineInfo members
        internal bool HasLineInfo()
        {
            IXmlLineInfo iXmlLineInfo = reader as IXmlLineInfo;
            return (iXmlLineInfo == null) ? false : iXmlLineInfo.HasLineInfo();
        }

        internal int LineNumber
        {
            get
            {
                IXmlLineInfo iXmlLineInfo = reader as IXmlLineInfo;
                return (iXmlLineInfo == null) ? 0 : iXmlLineInfo.LineNumber;
            }
        }

        internal int LinePosition
        {
            get
            {
                IXmlLineInfo iXmlLineInfo = reader as IXmlLineInfo;
                return (iXmlLineInfo == null) ? 0 : iXmlLineInfo.LinePosition;
            }
        }

        // IXmlTextParser members
        internal bool Normalized
        {
            get
            {
                XmlTextReader xmlTextReader = reader as XmlTextReader;
                if (xmlTextReader == null)
                {
                    IXmlTextParser xmlTextParser = reader as IXmlTextParser;
                    return (xmlTextParser == null) ? false : xmlTextParser.Normalized;
                }
                else
                    return xmlTextReader.Normalization;
            }
            set
            {
                XmlTextReader xmlTextReader = reader as XmlTextReader;
                if (xmlTextReader == null)
                {
                    IXmlTextParser xmlTextParser = reader as IXmlTextParser;
                    if (xmlTextParser != null)
                        xmlTextParser.Normalized = value;
                }
                else
                    xmlTextReader.Normalization = value;
            }
        }

        internal WhitespaceHandling WhitespaceHandling
        {
            get
            {
                XmlTextReader xmlTextReader = reader as XmlTextReader;
                if (xmlTextReader == null)
                {
                    IXmlTextParser xmlTextParser = reader as IXmlTextParser;
                    return (xmlTextParser == null) ? WhitespaceHandling.None : xmlTextParser.WhitespaceHandling;
                }
                else
                    return xmlTextReader.WhitespaceHandling;
            }
            set
            {
                XmlTextReader xmlTextReader = reader as XmlTextReader;
                if (xmlTextReader == null)
                {
                    IXmlTextParser xmlTextParser = reader as IXmlTextParser;
                    if (xmlTextParser != null)
                        xmlTextParser.WhitespaceHandling = value;
                }
                else
                    xmlTextReader.WhitespaceHandling = value;
            }
        }

        // delegating properties and methods
        internal string Name { get { return reader.Name; } }

#if USE_REFEMIT
        internal string LocalName 
#else
        public string LocalName
#endif        
        { 
            get { return reader.LocalName; } 
        }

        internal string NamespaceURI { get { return reader.NamespaceURI; } }
        internal string Value { get { return reader.Value; } }
        internal Type ValueType { get { return reader.ValueType; } }
        internal int Depth { get { return reader.Depth; } }
        internal string LookupNamespace(string prefix) { return reader.LookupNamespace(prefix); }
        internal bool EOF { get { return reader.EOF; } }

        internal void Skip()
        {
            reader.Skip();
            isEndOfEmptyElement = false;
        }

#if NotUsed
        internal XmlReaderSettings Settings { get { return reader.Settings; } }
        internal string Prefix { get { return reader.Prefix; } }
        internal bool HasValue { get { return reader.HasValue; } }
        internal string BaseURI { get { return reader.BaseURI; } }
        internal bool IsDefault { get { return reader.IsDefault; } }
        internal char QuoteChar { get { return reader.QuoteChar; } }
        internal XmlSpace XmlSpace { get { return reader.XmlSpace; } }
        internal string XmlLang { get { return reader.XmlLang; } }
        internal IXmlSchemaInfo SchemaInfo { get { return reader.SchemaInfo; } }
        internal string this[int i] { get { return reader[i]; } }
        internal string this[string name] { get { return reader[name]; } }
        internal string this[string name, string namespaceURI] { get { return reader[name, namespaceURI]; } }
        internal ReadState ReadState { get { return reader.ReadState; } }
        internal XmlNameTable NameTable { get { return reader.NameTable; } }
        internal bool CanResolveEntity { get { return reader.CanResolveEntity; } }
        internal bool CanReadBinaryContent { get { return reader.CanReadBinaryContent; } }
        internal bool CanReadValueChunk { get { return reader.CanReadValueChunk; } }
        internal bool HasAttributes { get { return reader.HasAttributes; } }
        internal bool IsStartElement(string name) { return reader.IsStartElement(name); }
        internal void ResolveEntity() { reader.ResolveEntity(); }
        internal string ReadInnerXml() { return reader.ReadInnerXml(); }
        internal string ReadOuterXml() { return reader.ReadOuterXml(); }
        internal object ReadContentAsObject() { return reader.ReadContentAsObject(); }
        internal object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) { return reader.ReadContentAs(returnType, namespaceResolver); }
        internal object ReadElementContentAsObject() { return reader.ReadElementContentAsObject(); }
        internal object ReadElementContentAsObject(string localName, string namespaceURI) { return reader.ReadElementContentAsObject(localName, namespaceURI); }
        internal bool ReadElementContentAsBoolean(string localName, string namespaceURI) { return reader.ReadElementContentAsBoolean(localName, namespaceURI); }
        internal DateTime ReadElementContentAsDateTime(string localName, string namespaceURI) { return reader.ReadElementContentAsDateTime(localName, namespaceURI); }
        internal double ReadElementContentAsDouble(string localName, string namespaceURI) { return reader.ReadElementContentAsDouble(localName, namespaceURI); }
        internal int ReadElementContentAsInt(string localName, string namespaceURI) { return reader.ReadElementContentAsInt(localName, namespaceURI); }
        internal long ReadElementContentAsLong(string localName, string namespaceURI) { return reader.ReadElementContentAsLong(localName, namespaceURI); }
        internal string ReadElementContentAsString(string localName, string namespaceURI) { return reader.ReadElementContentAsString(localName, namespaceURI); }
        internal object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) { return reader.ReadElementContentAs(returnType, namespaceResolver); }
        internal object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI) { return reader.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI); }
        internal int ReadContentAsBase64(byte[] buffer, int index, int count) { return reader.ReadContentAsBase64(buffer, index, count); }
        internal int ReadElementContentAsBase64(byte[] buffer, int index, int count) { return reader.ReadElementContentAsBase64(buffer, index, count); }
        internal int ReadContentAsBinHex(byte[] buffer, int index, int count) { return reader.ReadContentAsBinHex(buffer, index, count); }
        internal int ReadElementContentAsBinHex(byte[] buffer, int index, int count) { return reader.ReadElementContentAsBinHex(buffer, index, count); }
        internal int ReadValueChunk(char[] buffer, int index, int count) { return reader.ReadValueChunk(buffer, index, count); }
        internal string ReadString() { return reader.ReadString(); }
        internal string ReadElementString() { return reader.ReadElementString(); }
        internal string ReadElementString(string name) { return reader.ReadElementString(name); }
        internal string ReadElementString(string localname, string ns) { return reader.ReadElementString(localname, ns); }
        internal bool ReadToFollowing(string name) { return ReadToFollowing(name); }
        internal bool ReadToFollowing(string localName, string namespaceURI) { return reader.ReadToFollowing(localName, namespaceURI); }
        internal bool ReadToDescendant(string name) { return reader.ReadToDescendant(name); }
        internal bool ReadToDescendant(string localName, string namespaceURI) { return reader.ReadToDescendant(localName, namespaceURI); }
        internal bool ReadToNextSibling(string name) { return reader.ReadToNextSibling(name); }
        internal bool ReadToNextSibling(string localName, string namespaceURI) { return reader.ReadToNextSibling(localName, namespaceURI); }
        internal void ReadStartElement() 
        {
            if (isEndOfEmptyElement)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidNodeType, this.NodeType.ToString())));
            if (reader.IsEmptyElement)
                isEndOfEmptyElement = true;
            else
                reader.ReadStartElement(); 
        }
        internal void ReadStartElement(String localname, String ns)
        {
            if (isEndOfEmptyElement)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidNodeType, this.NodeType.ToString())));
            if (reader.IsEmptyElement)
                isEndOfEmptyElement = true;
            else
                reader.ReadStartElement(localname, ns);
        }

        internal void ReadStartElement(string name) 
        {
            if (isEndOfEmptyElement)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.InvalidNodeType, this.NodeType.ToString())));
            if (reader.IsEmptyElement)
                isEndOfEmptyElement = true;
            else
                reader.ReadStartElement(name); 
        }
        
        internal XmlReader ReadSubtree() 
        { 
            if (this.NodeType == XmlNodeType.Element)
                return reader.ReadSubtree(); 

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.XmlFunctionRequiredNodeType, "ReadSubtree", "Element")));
        }
#endif
    }
}

