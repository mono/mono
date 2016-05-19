//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Xml;
    using System.Globalization;
    using System.ServiceModel;

#if USE_REFEMIT
    public class JsonWriterDelegator : XmlWriterDelegator
#else
    internal class JsonWriterDelegator : XmlWriterDelegator
#endif
    {
        DateTimeFormat dateTimeFormat;

        public JsonWriterDelegator(XmlWriter writer)
            : base(writer)
        {
        }

        public JsonWriterDelegator(XmlWriter writer, DateTimeFormat dateTimeFormat)
            : this(writer)
        {
            this.dateTimeFormat = dateTimeFormat;
        }

        internal override void WriteChar(char value)
        {
            WriteString(XmlConvert.ToString(value));
        }

        internal override void WriteBase64(byte[] bytes)
        {
            if (bytes == null)
            {
                return;
            }

            ByteArrayHelperWithString.Instance.WriteArray(Writer, bytes, 0, bytes.Length);
        }

        internal override void WriteQName(XmlQualifiedName value)
        {
            if (value != XmlQualifiedName.Empty)
            {
                writer.WriteString(value.Name);
                writer.WriteString(JsonGlobals.NameValueSeparatorString);
                writer.WriteString(value.Namespace);
            }
        }

        internal override void WriteUnsignedLong(ulong value)
        {
            WriteDecimal((decimal)value);
        }

        internal override void WriteDecimal(decimal value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteDecimal(value);
        }

        internal override void WriteDouble(double value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteDouble(value);
        }

        internal override void WriteFloat(float value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteFloat(value);
        }

        internal override void WriteLong(long value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteLong(value);
        }

        internal override void WriteSignedByte(sbyte value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteSignedByte(value);
        }

        internal override void WriteUnsignedInt(uint value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteUnsignedInt(value);
        }

        internal override void WriteUnsignedShort(ushort value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteUnsignedShort(value);
        }

        internal override void WriteUnsignedByte(byte value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteUnsignedByte(value);
        }

        internal override void WriteShort(short value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteShort(value);
        }

        internal override void WriteBoolean(bool value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.booleanString);
            base.WriteBoolean(value);
        }

        internal override void WriteInt(int value)
        {
            writer.WriteAttributeString(JsonGlobals.typeString, JsonGlobals.numberString);
            base.WriteInt(value);
        }


#if USE_REFEMIT
        public void WriteJsonBooleanArray(bool[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonBooleanArray(bool[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteBoolean(value[i], itemName, itemNamespace);
            }
        }

#if USE_REFEMIT
        public void WriteJsonDateTimeArray(DateTime[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonDateTimeArray(DateTime[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteDateTime(value[i], itemName, itemNamespace);
            }
        }

#if USE_REFEMIT
        public void WriteJsonDecimalArray(decimal[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonDecimalArray(decimal[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteDecimal(value[i], itemName, itemNamespace);
            }
        }

#if USE_REFEMIT
        public void WriteJsonInt32Array(int[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonInt32Array(int[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteInt(value[i], itemName, itemNamespace);
            }
        }

#if USE_REFEMIT
        public void WriteJsonInt64Array(long[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonInt64Array(long[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteLong(value[i], itemName, itemNamespace);
            }
        }

        internal override void WriteDateTime(DateTime value)
        {
            if (this.dateTimeFormat == null)
            {
                WriteDateTimeInDefaultFormat(value);
            }
            else
            {
                writer.WriteString(value.ToString(this.dateTimeFormat.FormatString, this.dateTimeFormat.FormatProvider));
            }
        }

        void WriteDateTimeInDefaultFormat(DateTime value)
        {
            // ToUniversalTime() truncates dates to DateTime.MaxValue or DateTime.MinValue instead of throwing
            // This will break round-tripping of these dates (see bug 9690 in CSD Developer Framework)
            if (value.Kind != DateTimeKind.Utc)
            {
                long tickCount = value.Ticks - TimeZone.CurrentTimeZone.GetUtcOffset(value).Ticks;
                if ((tickCount > DateTime.MaxValue.Ticks) || (tickCount < DateTime.MinValue.Ticks))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.JsonDateTimeOutOfRange), new ArgumentOutOfRangeException("value")));
                }
            }

            writer.WriteString(JsonGlobals.DateTimeStartGuardReader);
            writer.WriteValue((value.ToUniversalTime().Ticks - JsonGlobals.unixEpochTicks) / 10000);

            switch (value.Kind)
            {
                case DateTimeKind.Unspecified:
                case DateTimeKind.Local:
                    // +"zzzz";
                    TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset(value.ToLocalTime());
                    if (ts.Ticks < 0)
                    {
                        writer.WriteString("-");
                    }
                    else
                    {
                        writer.WriteString("+");
                    }
                    int hours = Math.Abs(ts.Hours);
                    writer.WriteString((hours < 10) ? "0" + hours : hours.ToString(CultureInfo.InvariantCulture));
                    int minutes = Math.Abs(ts.Minutes);
                    writer.WriteString((minutes < 10) ? "0" + minutes : minutes.ToString(CultureInfo.InvariantCulture));
                    break;
                case DateTimeKind.Utc:
                    break;
            }
            writer.WriteString(JsonGlobals.DateTimeEndGuardReader);
        }

#if USE_REFEMIT
        public void WriteJsonSingleArray(float[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonSingleArray(float[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteFloat(value[i], itemName, itemNamespace);
            }
        }

#if USE_REFEMIT
        public void WriteJsonDoubleArray(double[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#else
        internal void WriteJsonDoubleArray(double[] value, XmlDictionaryString itemName, XmlDictionaryString itemNamespace)
#endif
        {
            for (int i = 0; i < value.Length; i++)
            {
                WriteDouble(value[i], itemName, itemNamespace);
            }
        }

        internal override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (localName != null && localName.Length == 0)
            {                
                base.WriteStartElement(JsonGlobals.itemString, JsonGlobals.itemString);
                base.WriteAttributeString(null, JsonGlobals.itemString, null, localName);
            }
            else
            {
                base.WriteStartElement(prefix, localName, ns);
            }
        }
    }            
}
