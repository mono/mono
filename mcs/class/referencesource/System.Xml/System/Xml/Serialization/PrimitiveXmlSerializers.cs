//------------------------------------------------------------------------------
// <copyright file="PrimitiveXmlSerializers.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {
    internal class XmlSerializationPrimitiveWriter : System.Xml.Serialization.XmlSerializationWriter {

        internal void Write_string(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"string", @"");
                return;
            }
            TopLevelElement();
            WriteNullableStringLiteral(@"string", @"", ((System.String)o));
        }

        internal void Write_int(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"int", @"");
                return;
            }
            WriteElementStringRaw(@"int", @"", System.Xml.XmlConvert.ToString((System.Int32)((System.Int32)o)));
        }

        internal void Write_boolean(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"boolean", @"");
                return;
            }
            WriteElementStringRaw(@"boolean", @"", System.Xml.XmlConvert.ToString((System.Boolean)((System.Boolean)o)));
        }

        internal void Write_short(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"short", @"");
                return;
            }
            WriteElementStringRaw(@"short", @"", System.Xml.XmlConvert.ToString((System.Int16)((System.Int16)o)));
        }

        internal void Write_long(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"long", @"");
                return;
            }
            WriteElementStringRaw(@"long", @"", System.Xml.XmlConvert.ToString((System.Int64)((System.Int64)o)));
        }

        internal void Write_float(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"float", @"");
                return;
            }
            WriteElementStringRaw(@"float", @"", System.Xml.XmlConvert.ToString((System.Single)((System.Single)o)));
        }

        internal void Write_double(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"double", @"");
                return;
            }
            WriteElementStringRaw(@"double", @"", System.Xml.XmlConvert.ToString((System.Double)((System.Double)o)));
        }

        internal void Write_decimal(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"decimal", @"");
                return;
            }
            WriteElementStringRaw(@"decimal", @"", System.Xml.XmlConvert.ToString((System.Decimal)((System.Decimal)o)));
        }

        internal void Write_dateTime(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"dateTime", @"");
                return;
            }
            WriteElementStringRaw(@"dateTime", @"", FromDateTime(((System.DateTime)o)));
        }

        internal void Write_unsignedByte(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"unsignedByte", @"");
                return;
            }
            WriteElementStringRaw(@"unsignedByte", @"", System.Xml.XmlConvert.ToString((System.Byte)((System.Byte)o)));
        }

        internal void Write_byte(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"byte", @"");
                return;
            }
            WriteElementStringRaw(@"byte", @"", System.Xml.XmlConvert.ToString((System.SByte)((System.SByte)o)));
        }

        internal void Write_unsignedShort(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"unsignedShort", @"");
                return;
            }
            WriteElementStringRaw(@"unsignedShort", @"", System.Xml.XmlConvert.ToString((System.UInt16)((System.UInt16)o)));
        }

        internal void Write_unsignedInt(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"unsignedInt", @"");
                return;
            }
            WriteElementStringRaw(@"unsignedInt", @"", System.Xml.XmlConvert.ToString((System.UInt32)((System.UInt32)o)));
        }

        internal void Write_unsignedLong(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"unsignedLong", @"");
                return;
            }
            WriteElementStringRaw(@"unsignedLong", @"", System.Xml.XmlConvert.ToString((System.UInt64)((System.UInt64)o)));
        }

        internal void Write_base64Binary(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"base64Binary", @"");
                return;
            }
            TopLevelElement();
            WriteNullableStringLiteralRaw(@"base64Binary", @"", FromByteArrayBase64(((System.Byte[])o)));
        }

        internal void Write_guid(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"guid", @"");
                return;
            }
            WriteElementStringRaw(@"guid", @"", System.Xml.XmlConvert.ToString((System.Guid)((System.Guid)o)));
        }

        internal void Write_char(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteEmptyTag(@"char", @"");
                return;
            }
            WriteElementString(@"char", @"", FromChar(((System.Char)o)));
        }

        internal void Write_QName(object o) {
            WriteStartDocument();
            if (o == null) {
                WriteNullTagLiteral(@"QName", @"");
                return;
            }
            TopLevelElement();
            WriteNullableQualifiedNameLiteral(@"QName", @"", ((global::System.Xml.XmlQualifiedName)o));
        }

        protected override void InitCallbacks() {
        }
    }

    internal class XmlSerializationPrimitiveReader : System.Xml.Serialization.XmlSerializationReader {

        internal object Read_string() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_string && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    if (ReadNull()) {
                        o = null;
                    }
                    else {
                        o = Reader.ReadElementString();
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_int() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id3_int && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToInt32(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_boolean() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id4_boolean && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToBoolean(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_short() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id5_short && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToInt16(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_long() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id6_long && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToInt64(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_float() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id7_float && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToSingle(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_double() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id8_double && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToDouble(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_decimal() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id9_decimal && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToDecimal(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_dateTime() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id10_dateTime && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = ToDateTime(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_unsignedByte() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id11_unsignedByte && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToByte(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_byte() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id12_byte && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToSByte(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_unsignedShort() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id13_unsignedShort && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToUInt16(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_unsignedInt() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id14_unsignedInt && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToUInt32(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_unsignedLong() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id15_unsignedLong && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToUInt64(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_base64Binary() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id16_base64Binary && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    if (ReadNull()) {
                        o = null;
                    }
                    else {
                        o = ToByteArrayBase64(false);
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_guid() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id17_guid && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = System.Xml.XmlConvert.ToGuid(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_char() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id18_char && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    {
                        o = ToChar(Reader.ReadElementString());
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        internal object Read_QName() {
            object o = null;
            Reader.MoveToContent();
            if (Reader.NodeType == System.Xml.XmlNodeType.Element) {
                if (((object) Reader.LocalName == (object)id1_QName && (object) Reader.NamespaceURI == (object)id2_Item)) {
                    if (ReadNull()) {
                        o = null;
                    }
                    else {
                        o = ReadElementQualifiedName();
                    }
                }
                else {
                    throw CreateUnknownNodeException();
                }
            }
            else {
                UnknownNode(null);
            }
            return (object)o;
        }

        protected override void InitCallbacks() {
        }

        System.String id4_boolean;
        System.String id14_unsignedInt;
        System.String id15_unsignedLong;
        System.String id7_float;
        System.String id10_dateTime;
        System.String id6_long;
        System.String id9_decimal;
        System.String id8_double;
        System.String id17_guid;
        System.String id2_Item;
        System.String id13_unsignedShort;
        System.String id18_char;
        System.String id3_int;
        System.String id12_byte;
        System.String id16_base64Binary;
        System.String id11_unsignedByte;
        System.String id5_short;
        System.String id1_string;
        System.String id1_QName;

        protected override void InitIDs() {
            id4_boolean = Reader.NameTable.Add(@"boolean");
            id14_unsignedInt = Reader.NameTable.Add(@"unsignedInt");
            id15_unsignedLong = Reader.NameTable.Add(@"unsignedLong");
            id7_float = Reader.NameTable.Add(@"float");
            id10_dateTime = Reader.NameTable.Add(@"dateTime");
            id6_long = Reader.NameTable.Add(@"long");
            id9_decimal = Reader.NameTable.Add(@"decimal");
            id8_double = Reader.NameTable.Add(@"double");
            id17_guid = Reader.NameTable.Add(@"guid");
            id2_Item = Reader.NameTable.Add(@"");
            id13_unsignedShort = Reader.NameTable.Add(@"unsignedShort");
            id18_char = Reader.NameTable.Add(@"char");
            id3_int = Reader.NameTable.Add(@"int");
            id12_byte = Reader.NameTable.Add(@"byte");
            id16_base64Binary = Reader.NameTable.Add(@"base64Binary");
            id11_unsignedByte = Reader.NameTable.Add(@"unsignedByte");
            id5_short = Reader.NameTable.Add(@"short");
            id1_string = Reader.NameTable.Add(@"string");
            id1_QName = Reader.NameTable.Add(@"QName");
        }
    }
}
