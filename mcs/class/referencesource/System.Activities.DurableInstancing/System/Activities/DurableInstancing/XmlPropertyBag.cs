//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    class XmlPropertyBag : Dictionary<XName, object>, IXmlSerializable
    {
        public XmlPropertyBag()
        {
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.ReadToDescendant("Property"))
            {
                do
                {
                    reader.MoveToFirstAttribute();
                    XName propertyName = XName.Get(reader.Value);

                    reader.MoveToNextAttribute();
                    PrimitiveType type = (PrimitiveType)Int32.Parse(reader.Value, CultureInfo.InvariantCulture);

                    reader.MoveToNextAttribute();
                    object value = ConvertStringToNativeType(reader.Value, type);

                    this.Add(propertyName, value);
                }

                while (reader.ReadToNextSibling("Property"));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Properties");

            foreach (KeyValuePair<XName, object> property in this)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("XName", property.Key.ToString());
                writer.WriteAttributeString("Type", ((int)GetPrimitiveType(property.Value)).ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Value", ConvertNativeValueToString(property.Value));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public static PrimitiveType GetPrimitiveType(object value)
        {
            if (value == null)
            {
                return PrimitiveType.Null;
            }
            else if (value is bool)
            {
                return PrimitiveType.Bool;
            }
            else if (value is byte)
            {
                return PrimitiveType.Byte;
            }
            else if (value is char)
            {
                return PrimitiveType.Char;
            }
            else if (value is DateTime)
            {
                return PrimitiveType.DateTime;
            }
            else if (value is DateTimeOffset)
            {
                return PrimitiveType.DateTimeOffset;
            }
            else if (value is decimal)
            {
                return PrimitiveType.Decimal;
            }
            else if (value is double)
            {
                return PrimitiveType.Double;
            }
            else if (value is float)
            {
                return PrimitiveType.Float;
            }
            else if (value is Guid)
            {
                return PrimitiveType.Guid;
            }
            else if (value is int)
            {
                return PrimitiveType.Int;
            }
            else if (value is long)
            {
                return PrimitiveType.Long;
            }
            else if (value is sbyte)
            {
                return PrimitiveType.SByte;
            }
            else if (value is short)
            {
                return PrimitiveType.Short;
            }
            else if (value is string)
            {
                return PrimitiveType.String;
            }
            else if (value is TimeSpan)
            {
                return PrimitiveType.TimeSpan;
            }
            else if (value is Type)
            {
                return PrimitiveType.Type;
            }
            else if (value is uint)
            {
                return PrimitiveType.UInt;
            }
            else if (value is ulong)
            {
                return PrimitiveType.ULong;
            }
            else if (value is Uri)
            {
                return PrimitiveType.Uri;
            }
            else if (value is ushort)
            {
                return PrimitiveType.UShort;
            }
            else if (value is XmlQualifiedName)
            {
                return PrimitiveType.XmlQualifiedName;
            }
            else
            {
                return PrimitiveType.Unavailable;
            }
        }

        public static object ConvertStringToNativeType(string value, PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool:
                    return XmlConvert.ToBoolean(value);
                case PrimitiveType.Byte:
                    return XmlConvert.ToByte(value);
                case PrimitiveType.Char:
                    return XmlConvert.ToChar(value);
                case PrimitiveType.DateTime:
                    return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
                case PrimitiveType.DateTimeOffset:
                    return XmlConvert.ToDateTimeOffset(value);
                case PrimitiveType.Decimal:
                    return XmlConvert.ToDecimal(value);
                case PrimitiveType.Double:
                    return XmlConvert.ToDouble(value);
                case PrimitiveType.Float:
                    return float.Parse(value, CultureInfo.InvariantCulture);
                case PrimitiveType.Guid:
                    return XmlConvert.ToGuid(value);
                case PrimitiveType.Int:
                    return XmlConvert.ToInt32(value);
                case PrimitiveType.Long:
                    return XmlConvert.ToInt64(value);
                case PrimitiveType.SByte:
                    return XmlConvert.ToSByte(value);
                case PrimitiveType.Short:
                    return XmlConvert.ToInt16(value);
                case PrimitiveType.String:
                    return value;
                case PrimitiveType.TimeSpan:
                    return XmlConvert.ToTimeSpan(value);
                case PrimitiveType.Type:
                    return Type.GetType(value);
                case PrimitiveType.UInt:
                    return XmlConvert.ToUInt32(value);
                case PrimitiveType.ULong:
                    return XmlConvert.ToUInt64(value);
                case PrimitiveType.Uri:
                    return new Uri(value);
                case PrimitiveType.UShort:
                    return XmlConvert.ToUInt16(value);
                case PrimitiveType.XmlQualifiedName:
                    return new XmlQualifiedName(value);
                case PrimitiveType.Null:
                case PrimitiveType.Unavailable:
                default:
                    return null;
            }
        }

        public static string ConvertNativeValueToString(object value)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is bool)
            {
                return XmlConvert.ToString((bool)value);
            }
            else if (value is byte)
            {
                return XmlConvert.ToString((byte)value);
            }
            else if (value is char)
            {
                return XmlConvert.ToString((char)value);
            }
            else if (value is DateTime)
            {
                return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
            }
            else if (value is DateTimeOffset)
            {
                return XmlConvert.ToString((DateTimeOffset)value);
            }
            else if (value is decimal)
            {
                return XmlConvert.ToString((decimal)value);
            }
            else if (value is double)
            {
                return XmlConvert.ToString((double)value);
            }
            else if (value is float)
            {
                return ((float)value).ToString("r", CultureInfo.InvariantCulture);
            }
            else if (value is Guid)
            {
                return XmlConvert.ToString((Guid)value);
            }
            else if (value is int)
            {
                return XmlConvert.ToString((int)value);
            }
            else if (value is long)
            {
                return XmlConvert.ToString((long)value);
            }
            else if (value is sbyte)
            {
                return XmlConvert.ToString((sbyte)value);
            }
            else if (value is short)
            {
                return XmlConvert.ToString((short)value);
            }
            else if (value is string)
            {
                return (string)value;
            }
            else if (value is TimeSpan)
            {
                return XmlConvert.ToString((TimeSpan)value);
            }
            else if (value is Type)
            {
                return value.ToString();
            }
            else if (value is uint)
            {
                return XmlConvert.ToString((uint)value);
            }
            else if (value is ulong)
            {
                return XmlConvert.ToString((ulong)value);
            }
            else if (value is Uri)
            {
                return ((Uri)value).ToString();
            }
            else if (value is ushort)
            {
                return XmlConvert.ToString((ushort)value);
            }
            else if (value is XmlQualifiedName)
            {
                return ((XmlQualifiedName)value).ToString();
            }
            else
            {
                Fx.AssertAndThrow("Should never reach here");
                return null;
            }
        }
    }
}
