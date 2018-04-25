//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Text;
    using System.Diagnostics;
    using System.Reflection;
    using System.Globalization;

#if USE_REFEMIT
    public class XmlWriterDelegator
#else
    internal class XmlWriterDelegator
#endif
    {
        protected XmlWriter writer;
        protected XmlDictionaryWriter dictionaryWriter;
        internal int depth;
        int prefixes;

        public XmlWriterDelegator(XmlWriter writer)
        {
            XmlObjectSerializer.CheckNull(writer, "writer");
            this.writer = writer;
            this.dictionaryWriter = writer as XmlDictionaryWriter;
        }

        internal XmlWriter Writer
        {
            get { return writer; }
        }

        internal void Flush()
        {
            writer.Flush();
        }

        internal string LookupPrefix(string ns)
        {
            return writer.LookupPrefix(ns);
        }

        void WriteEndAttribute()
        {
            writer.WriteEndAttribute();
        }

        public void WriteEndElement()
        {
            writer.WriteEndElement();
            depth--;
        }

        internal void WriteRaw(char[] buffer, int index, int count)
        {
            writer.WriteRaw(buffer, index, count);
        }

        internal void WriteRaw(string data)
        {
            writer.WriteRaw(data);
        }


        internal void WriteXmlnsAttribute(XmlDictionaryString ns)
        {
            if (dictionaryWriter != null)
            {
                if (ns != null)
                    dictionaryWriter.WriteXmlnsAttribute(null, ns);
            }
            else
                WriteXmlnsAttribute(ns.Value);
        }

        internal void WriteXmlnsAttribute(string ns)
        {
            if (ns != null)
            {
                if (ns.Length == 0)
                    writer.WriteAttributeString("xmlns", String.Empty, null, ns);
                else
                {
                    if (dictionaryWriter != null)
                        dictionaryWriter.WriteXmlnsAttribute(null, ns);
                    else
                    {
                        string prefix = writer.LookupPrefix(ns);
                        if (prefix == null)
                        {
                            prefix = String.Format(CultureInfo.InvariantCulture, "d{0}p{1}", depth, prefixes);
                            prefixes++;
                            writer.WriteAttributeString("xmlns", prefix, null, ns);
                        }
                    }
                }
            }
        }

        internal void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            if (dictionaryWriter != null)
            {
                dictionaryWriter.WriteXmlnsAttribute(prefix, ns);
            }
            else
            {
                writer.WriteAttributeString("xmlns", prefix, null, ns.Value);
            }
        }

        void WriteStartAttribute(string prefix, string localName, string ns)
        {
            writer.WriteStartAttribute(prefix, localName, ns);
        }

        void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (dictionaryWriter != null)
                dictionaryWriter.WriteStartAttribute(prefix, localName, namespaceUri);
            else
                writer.WriteStartAttribute(prefix,
                    (localName == null ? null : localName.Value),
                    (namespaceUri == null ? null : namespaceUri.Value));
        }

        internal void WriteAttributeString(string prefix, string localName, string ns, string value)
        {
            WriteStartAttribute(prefix, localName, ns);
            WriteAttributeStringValue(value);
            WriteEndAttribute();
        }

        internal void WriteAttributeString(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, string value)
        {
            WriteStartAttribute(prefix, attrName, attrNs);
            WriteAttributeStringValue(value);
            WriteEndAttribute();
        }

        void WriteAttributeStringValue(string value)
        {
            writer.WriteValue(value);
        }

        internal void WriteAttributeString(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, XmlDictionaryString value)
        {
            WriteStartAttribute(prefix, attrName, attrNs);
            WriteAttributeStringValue(value);
            WriteEndAttribute();
        }

        void WriteAttributeStringValue(XmlDictionaryString value)
        {
            if (dictionaryWriter == null)
                writer.WriteString(value.Value);
            else
                dictionaryWriter.WriteString(value);
        }

        internal void WriteAttributeInt(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, int value)
        {
            WriteStartAttribute(prefix, attrName, attrNs);
            WriteAttributeIntValue(value);
            WriteEndAttribute();
        }

        void WriteAttributeIntValue(int value)
        {
            writer.WriteValue(value);
        }

        internal void WriteAttributeBool(string prefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, bool value)
        {
            WriteStartAttribute(prefix, attrName, attrNs);
            WriteAttributeBoolValue(value);
            WriteEndAttribute();
        }

        void WriteAttributeBoolValue(bool value)
        {
            writer.WriteValue(value);
        }

        internal void WriteAttributeQualifiedName(string attrPrefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, string name, string ns)
        {
            WriteXmlnsAttribute(ns);
            WriteStartAttribute(attrPrefix, attrName, attrNs);
            WriteAttributeQualifiedNameValue(name, ns);
            WriteEndAttribute();
        }

        void WriteAttributeQualifiedNameValue(string name, string ns)
        {
            writer.WriteQualifiedName(name, ns);
        }

        internal void WriteAttributeQualifiedName(string attrPrefix, XmlDictionaryString attrName, XmlDictionaryString attrNs, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteXmlnsAttribute(ns);
            WriteStartAttribute(attrPrefix, attrName, attrNs);
            WriteAttributeQualifiedNameValue(name, ns);
            WriteEndAttribute();
        }

        void WriteAttributeQualifiedNameValue(XmlDictionaryString name, XmlDictionaryString ns)
        {
            if (dictionaryWriter == null)
                writer.WriteQualifiedName(name.Value, ns.Value);
            else
                dictionaryWriter.WriteQualifiedName(name, ns);
        }

        internal void WriteStartElement(string localName, string ns)
        {
            WriteStartElement(null, localName, ns);
        }

        internal virtual void WriteStartElement(string prefix, string localName, string ns)
        {
            writer.WriteStartElement(prefix, localName, ns);
            depth++;
            prefixes = 1;
        }

        public void WriteStartElement(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            WriteStartElement(null, localName, namespaceUri);
        }

        internal void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (dictionaryWriter != null)
                dictionaryWriter.WriteStartElement(prefix, localName, namespaceUri);
            else
                writer.WriteStartElement(prefix, (localName == null ? null : localName.Value), (namespaceUri == null ? null : namespaceUri.Value));
            depth++;
            prefixes = 1;
        }

        internal void WriteStartElementPrimitive(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            if (dictionaryWriter != null)
                dictionaryWriter.WriteStartElement(null, localName, namespaceUri);
            else
                writer.WriteStartElement(null, (localName == null ? null : localName.Value), (namespaceUri == null ? null : namespaceUri.Value));
        }

        internal void WriteEndElementPrimitive()
        {
            writer.WriteEndElement();
        }

        internal WriteState WriteState
        {
            get { return writer.WriteState; }
        }

        internal string XmlLang
        {
            get { return writer.XmlLang; }
        }

        internal XmlSpace XmlSpace
        {
            get { return writer.XmlSpace; }
        }

        public void WriteNamespaceDecl(XmlDictionaryString ns)
        {
            WriteXmlnsAttribute(ns);
        }

        Exception CreateInvalidPrimitiveTypeException(Type type)
        {
            return new InvalidDataContractException(SR.GetString(SR.InvalidPrimitiveType, DataContract.GetClrTypeFullName(type)));
        }

        internal void WriteAnyType(object value)
        {
            WriteAnyType(value, value.GetType());
        }

        internal void WriteAnyType(object value, Type valueType)
        {
            bool handled = true;
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    WriteBoolean((bool)value);
                    break;
                case TypeCode.Char:
                    WriteChar((char)value);
                    break;
                case TypeCode.Byte:
                    WriteUnsignedByte((byte)value);
                    break;
                case TypeCode.Int16:
                    WriteShort((short)value);
                    break;
                case TypeCode.Int32:
                    WriteInt((int)value);
                    break;
                case TypeCode.Int64:
                    WriteLong((long)value);
                    break;
                case TypeCode.Single:
                    WriteFloat((float)value);
                    break;
                case TypeCode.Double:
                    WriteDouble((double)value);
                    break;
                case TypeCode.Decimal:
                    WriteDecimal((decimal)value);
                    break;
                case TypeCode.DateTime:
                    WriteDateTime((DateTime)value);
                    break;
                case TypeCode.String:
                    WriteString((string)value);
                    break;
                case TypeCode.SByte:
                    WriteSignedByte((sbyte)value);
                    break;
                case TypeCode.UInt16:
                    WriteUnsignedShort((ushort)value);
                    break;
                case TypeCode.UInt32:
                    WriteUnsignedInt((uint)value);
                    break;
                case TypeCode.UInt64:
                    WriteUnsignedLong((ulong)value);
                    break;
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.Object:
                default:
                    if (valueType == Globals.TypeOfByteArray)
                        WriteBase64((byte[])value);
                    else if (valueType == Globals.TypeOfObject)
                    {
                        //Write Nothing
                    }
                    else if (valueType == Globals.TypeOfTimeSpan)
                        WriteTimeSpan((TimeSpan)value);
                    else if (valueType == Globals.TypeOfGuid)
                        WriteGuid((Guid)value);
                    else if (valueType == Globals.TypeOfUri)
                        WriteUri((Uri)value);
                    else if (valueType == Globals.TypeOfXmlQualifiedName)
                        WriteQName((XmlQualifiedName)value);
                    else
                        handled = false;
                    break;
            }
            if (!handled)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidPrimitiveTypeException(valueType));
        }

        internal void WriteExtensionData(IDataNode dataNode)
        {
            bool handled = true;
            Type valueType = dataNode.DataType;
            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    WriteBoolean(((DataNode<bool>)dataNode).GetValue());
                    break;
                case TypeCode.Char:
                    WriteChar(((DataNode<char>)dataNode).GetValue());
                    break;
                case TypeCode.Byte:
                    WriteUnsignedByte(((DataNode<byte>)dataNode).GetValue());
                    break;
                case TypeCode.Int16:
                    WriteShort(((DataNode<short>)dataNode).GetValue());
                    break;
                case TypeCode.Int32:
                    WriteInt(((DataNode<int>)dataNode).GetValue());
                    break;
                case TypeCode.Int64:
                    WriteLong(((DataNode<long>)dataNode).GetValue());
                    break;
                case TypeCode.Single:
                    WriteFloat(((DataNode<float>)dataNode).GetValue());
                    break;
                case TypeCode.Double:
                    WriteDouble(((DataNode<double>)dataNode).GetValue());
                    break;
                case TypeCode.Decimal:
                    WriteDecimal(((DataNode<decimal>)dataNode).GetValue());
                    break;
                case TypeCode.DateTime:
                    WriteDateTime(((DataNode<DateTime>)dataNode).GetValue());
                    break;
                case TypeCode.String:
                    WriteString(((DataNode<string>)dataNode).GetValue());
                    break;
                case TypeCode.SByte:
                    WriteSignedByte(((DataNode<sbyte>)dataNode).GetValue());
                    break;
                case TypeCode.UInt16:
                    WriteUnsignedShort(((DataNode<ushort>)dataNode).GetValue());
                    break;
                case TypeCode.UInt32:
                    WriteUnsignedInt(((DataNode<uint>)dataNode).GetValue());
                    break;
                case TypeCode.UInt64:
                    WriteUnsignedLong(((DataNode<ulong>)dataNode).GetValue());
                    break;
                case TypeCode.Empty:
                case TypeCode.DBNull:
                case TypeCode.Object:
                default:
                    if (valueType == Globals.TypeOfByteArray)
                        WriteBase64(((DataNode<byte[]>)dataNode).GetValue());
                    else if (valueType == Globals.TypeOfObject)
                    {
                        object obj = dataNode.Value;
                        if (obj != null)
                            WriteAnyType(obj);
                    }
                    else if (valueType == Globals.TypeOfTimeSpan)
                        WriteTimeSpan(((DataNode<TimeSpan>)dataNode).GetValue());
                    else if (valueType == Globals.TypeOfGuid)
                        WriteGuid(((DataNode<Guid>)dataNode).GetValue());
                    else if (valueType == Globals.TypeOfUri)
                        WriteUri(((DataNode<Uri>)dataNode).GetValue());
                    else if (valueType == Globals.TypeOfXmlQualifiedName)
                        WriteQName(((DataNode<XmlQualifiedName>)dataNode).GetValue());
                    else
                        handled = false;
                    break;
            }
            if (!handled)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateInvalidPrimitiveTypeException(valueType));
        }

        internal void WriteString(string value)
        {
            writer.WriteValue(value);
        }

        internal virtual void WriteBoolean(bool value)
        {
            writer.WriteValue(value);
        }
        public void WriteBoolean(bool value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteBoolean(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteDateTime(DateTime value)
        {
            writer.WriteValue(value);
        }

        public void WriteDateTime(DateTime value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteDateTime(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteDecimal(decimal value)
        {
            writer.WriteValue(value);
        }
        public void WriteDecimal(decimal value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteDecimal(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteDouble(double value)
        {
            writer.WriteValue(value);
        }
        public void WriteDouble(double value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteDouble(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteInt(int value)
        {
            writer.WriteValue(value);
        }
        public void WriteInt(int value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteInt(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteLong(long value)
        {
            writer.WriteValue(value);
        }
        public void WriteLong(long value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteLong(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteFloat(float value)
        {
            writer.WriteValue(value);
        }
        public void WriteFloat(float value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteFloat(value);
            WriteEndElementPrimitive();
        }

        private const int CharChunkSize = 76;
        private const int ByteChunkSize = CharChunkSize / 4 * 3;

        internal virtual void WriteBase64(byte[] bytes)
        {
            if (bytes == null)
                return;

            writer.WriteBase64(bytes, 0, bytes.Length);
        }

        internal virtual void WriteShort(short value)
        {
            writer.WriteValue(value);
        }
        public void WriteShort(short value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteShort(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedByte(byte value)
        {
            writer.WriteValue(value);
        }
        public void WriteUnsignedByte(byte value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteUnsignedByte(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteSignedByte(sbyte value)
        {
            writer.WriteValue(value);
        }
#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public void WriteSignedByte(sbyte value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteSignedByte(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedInt(uint value)
        {
            writer.WriteValue(value);
        }
#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public void WriteUnsignedInt(uint value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteUnsignedInt(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedLong(ulong value)
        {
            writer.WriteRaw(XmlConvert.ToString(value));
        }
#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public void WriteUnsignedLong(ulong value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteUnsignedLong(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteUnsignedShort(ushort value)
        {
            writer.WriteValue(value);
        }
#if USE_REFEMIT
        [CLSCompliant(false)]
#endif
        public void WriteUnsignedShort(ushort value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteUnsignedShort(value);
            WriteEndElementPrimitive();
        }

        internal virtual void WriteChar(char value)
        {
            writer.WriteValue((int)value);
        }
        public void WriteChar(char value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteChar(value);
            WriteEndElementPrimitive();
        }

        internal void WriteTimeSpan(TimeSpan value)
        {
            writer.WriteRaw(XmlConvert.ToString(value));
        }
        public void WriteTimeSpan(TimeSpan value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteTimeSpan(value);
            WriteEndElementPrimitive();
        }

        internal void WriteGuid(Guid value)
        {
            writer.WriteRaw(value.ToString());
        }
        public void WriteGuid(Guid value, XmlDictionaryString name, XmlDictionaryString ns)
        {
            WriteStartElementPrimitive(name, ns);
            WriteGuid(value);
            WriteEndElementPrimitive();
        }

        internal void WriteUri(Uri value)
        {
            writer.WriteString(value.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped));
        }

        internal virtual void WriteQName(XmlQualifiedName value)
        {
            if (value != XmlQualifiedName.Empty)
            {
                WriteXmlnsAttribute(value.Namespace);
                WriteQualifiedName(value.Name, value.Namespace);
            }
        }

        internal void WriteQualifiedName(string localName, string ns)
        {
            writer.WriteQualifiedName(localName, ns);
        }

        internal void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString ns)
        {
            if (dictionaryWriter == null)
                writer.WriteQualifiedName(localName.Value, ns.Value);
            else
                dictionaryWriter.WriteQualifiedName(localName, ns);
        }

        public void WriteBooleanArray(bool[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteBoolean(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteDateTimeArray(DateTime[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteDateTime(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteDecimalArray(decimal[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteDecimal(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteInt32Array(int[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteInt(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteInt64Array(long[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteLong(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteSingleArray(float[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteFloat(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

        public void WriteDoubleArray(double[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
        {
            if (dictionaryWriter == null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    WriteDouble(value[i], itemName, itemNamespace);
                }
            }
            else
            {
                dictionaryWriter.WriteArray(null, itemName, itemNamespace, value, 0, value.Length);
            }
        }

    }
}

