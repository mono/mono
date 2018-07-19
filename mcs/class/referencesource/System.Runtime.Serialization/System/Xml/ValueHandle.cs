//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xml
{
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    enum ValueHandleConstStringType
    {
        String = 0,
        Number = 1,
        Array = 2,
        Object = 3,
        Boolean = 4,
        Null = 5,
    }

    static class ValueHandleLength
    {
        public const int Int8 = 1;
        public const int Int16 = 2;
        public const int Int32 = 4;
        public const int Int64 = 8;
        public const int UInt64 = 8;
        public const int Single = 4;
        public const int Double = 8;
        public const int Decimal = 16;
        public const int DateTime = 8;
        public const int TimeSpan = 8;
        public const int Guid = 16;
        public const int UniqueId = 16;
    }

    enum ValueHandleType
    {
        Empty,
        True,
        False,
        Zero,
        One,
        Int8,
        Int16,
        Int32,
        Int64,
        UInt64,
        Single,
        Double,
        Decimal,
        DateTime,
        TimeSpan,
        Guid,
        UniqueId,
        UTF8,
        EscapedUTF8,
        Base64,
        Dictionary,
        List,
        Char,
        Unicode,
        QName,
        ConstString
    }

    class ValueHandle
    {
        XmlBufferReader bufferReader;
        ValueHandleType type;
        int offset;
        int length;
        static Base64Encoding base64Encoding;


        static string[] constStrings = {
                                        "string",
                                        "number",
                                        "array",
                                        "object",
                                        "boolean",
                                        "null",
                                       };

        public ValueHandle(XmlBufferReader bufferReader)
        {
            this.bufferReader = bufferReader;
            this.type = ValueHandleType.Empty;
        }

        static Base64Encoding Base64Encoding
        {
            get
            {
                if (base64Encoding == null)
                    base64Encoding = new Base64Encoding();
                return base64Encoding;
            }
        }

        public void SetConstantValue(ValueHandleConstStringType constStringType)
        {
            type = ValueHandleType.ConstString;
            offset = (int)constStringType;
        }

        public void SetValue(ValueHandleType type)
        {
            this.type = type;
        }

        public void SetDictionaryValue(int key)
        {
            SetValue(ValueHandleType.Dictionary, key, 0);
        }

        public void SetCharValue(int ch)
        {
            SetValue(ValueHandleType.Char, ch, 0);
        }

        public void SetQNameValue(int prefix, int key)
        {
            SetValue(ValueHandleType.QName, key, prefix);
        }

        public void SetValue(ValueHandleType type, int offset, int length)
        {
            this.type = type;
            this.offset = offset;
            this.length = length;
        }

        public bool IsWhitespace()
        {
            switch (this.type)
            {
                case ValueHandleType.UTF8:
                    return bufferReader.IsWhitespaceUTF8(this.offset, this.length);

                case ValueHandleType.Dictionary:
                    return bufferReader.IsWhitespaceKey(this.offset);

                case ValueHandleType.Char:
                    int ch = GetChar();
                    if (ch > char.MaxValue)
                        return false;
                    return XmlConverter.IsWhitespace((char)ch);

                case ValueHandleType.EscapedUTF8:
                    return bufferReader.IsWhitespaceUTF8(this.offset, this.length);

                case ValueHandleType.Unicode:
                    return bufferReader.IsWhitespaceUnicode(this.offset, this.length);

                case ValueHandleType.True:
                case ValueHandleType.False:
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                    return false;

                case ValueHandleType.ConstString:
                    return constStrings[offset].Length == 0;

                default:
                    return this.length == 0;
            }
        }

        public Type ToType()
        {
            switch (type)
            {
                case ValueHandleType.False:
                case ValueHandleType.True:
                    return typeof(bool);
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return typeof(int);
                case ValueHandleType.Int64:
                    return typeof(long);
                case ValueHandleType.UInt64:
                    return typeof(ulong);
                case ValueHandleType.Single:
                    return typeof(float);
                case ValueHandleType.Double:
                    return typeof(double);
                case ValueHandleType.Decimal:
                    return typeof(decimal);
                case ValueHandleType.DateTime:
                    return typeof(DateTime);
                case ValueHandleType.Empty:
                case ValueHandleType.UTF8:
                case ValueHandleType.Unicode:
                case ValueHandleType.EscapedUTF8:
                case ValueHandleType.Dictionary:
                case ValueHandleType.Char:
                case ValueHandleType.QName:
                case ValueHandleType.ConstString:
                    return typeof(string);
                case ValueHandleType.Base64:
                    return typeof(byte[]);
                case ValueHandleType.List:
                    return typeof(object[]);
                case ValueHandleType.UniqueId:
                    return typeof(UniqueId);
                case ValueHandleType.Guid:
                    return typeof(Guid);
                case ValueHandleType.TimeSpan:
                    return typeof(TimeSpan);
                default:
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }

        public Boolean ToBoolean()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.False)
                return false;
            if (type == ValueHandleType.True)
                return true;
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToBoolean(bufferReader.Buffer, offset, length);
            if (type == ValueHandleType.Int8)
            {
                int value = GetInt8();
                if (value == 0)
                    return false;
                if (value == 1)
                    return true;
            }
            return XmlConverter.ToBoolean(GetString());
        }

        public int ToInt()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type == ValueHandleType.Int8)
                return GetInt8();
            if (type == ValueHandleType.Int16)
                return GetInt16();
            if (type == ValueHandleType.Int32)
                return GetInt32();
            if (type == ValueHandleType.Int64)
            {
                long value = GetInt64();
                if (value >= int.MinValue && value <= int.MaxValue)
                {
                    return (int)value;
                }
            }
            if (type == ValueHandleType.UInt64)
            {
                ulong value = GetUInt64();
                if (value <= int.MaxValue)
                {
                    return (int)value;
                }
            }
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToInt32(bufferReader.Buffer, offset, length);
            return XmlConverter.ToInt32(GetString());
        }

        public long ToLong()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type == ValueHandleType.Int8)
                return GetInt8();
            if (type == ValueHandleType.Int16)
                return GetInt16();
            if (type == ValueHandleType.Int32)
                return GetInt32();
            if (type == ValueHandleType.Int64)
                return GetInt64();
            if (type == ValueHandleType.UInt64)
            {
                ulong value = GetUInt64();
                if (value <= long.MaxValue)
                {
                    return (long)value;
                }
            }
            if (type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToInt64(bufferReader.Buffer, offset, length);
            }
            return XmlConverter.ToInt64(GetString());
        }

        public ulong ToULong()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type >= ValueHandleType.Int8 && type <= ValueHandleType.Int64)
            {
                long value = ToLong();
                if (value >= 0)
                    return (ulong)value;
            }
            if (type == ValueHandleType.UInt64)
                return GetUInt64();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToUInt64(bufferReader.Buffer, offset, length);
            return XmlConverter.ToUInt64(GetString());
        }

        public Single ToSingle()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Single)
                return GetSingle();
            if (type == ValueHandleType.Double)
            {
                double value = GetDouble();
                if ((value >= Single.MinValue && value <= Single.MaxValue) || double.IsInfinity(value) || double.IsNaN(value))
                    return (Single)value;
            }
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type == ValueHandleType.Int8)
                return GetInt8();
            if (type == ValueHandleType.Int16)
                return GetInt16();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToSingle(bufferReader.Buffer, offset, length);
            return XmlConverter.ToSingle(GetString());
        }

        public Double ToDouble()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Double)
                return GetDouble();
            if (type == ValueHandleType.Single)
                return GetSingle();
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type == ValueHandleType.Int8)
                return GetInt8();
            if (type == ValueHandleType.Int16)
                return GetInt16();
            if (type == ValueHandleType.Int32)
                return GetInt32();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToDouble(bufferReader.Buffer, offset, length);
            return XmlConverter.ToDouble(GetString());
        }

        public Decimal ToDecimal()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.Decimal)
                return GetDecimal();
            if (type == ValueHandleType.Zero)
                return 0;
            if (type == ValueHandleType.One)
                return 1;
            if (type >= ValueHandleType.Int8 && type <= ValueHandleType.Int64)
                return ToLong();
            if (type == ValueHandleType.UInt64)
                return GetUInt64();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToDecimal(bufferReader.Buffer, offset, length);
            return XmlConverter.ToDecimal(GetString());
        }

        public DateTime ToDateTime()
        {
            if (type == ValueHandleType.DateTime)
            {
                return XmlConverter.ToDateTime(GetInt64());
            }
            if (type == ValueHandleType.UTF8)
            {
                return XmlConverter.ToDateTime(bufferReader.Buffer, offset, length);
            }
            return XmlConverter.ToDateTime(GetString());
        }

        public UniqueId ToUniqueId()
        {
            if (type == ValueHandleType.UniqueId)
                return GetUniqueId();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToUniqueId(bufferReader.Buffer, offset, length);
            return XmlConverter.ToUniqueId(GetString());
        }

        public TimeSpan ToTimeSpan()
        {
            if (type == ValueHandleType.TimeSpan)
                return new TimeSpan(GetInt64());
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToTimeSpan(bufferReader.Buffer, offset, length);
            return XmlConverter.ToTimeSpan(GetString());
        }

        public Guid ToGuid()
        {
            if (type == ValueHandleType.Guid)
                return GetGuid();
            if (type == ValueHandleType.UTF8)
                return XmlConverter.ToGuid(bufferReader.Buffer, offset, length);
            return XmlConverter.ToGuid(GetString());
        }

        public override string ToString()
        {
            return GetString();
        }

        public byte[] ToByteArray()
        {
            if (type == ValueHandleType.Base64)
            {
                byte[] buffer = new byte[length];
                GetBase64(buffer, 0, length);
                return buffer;
            }
            if (type == ValueHandleType.UTF8 && (length % 4) == 0)
            {
                try
                {
                    int expectedLength = length / 4 * 3;
                    if (length > 0)
                    {
                        if (bufferReader.Buffer[offset + length - 1] == '=')
                        {
                            expectedLength--;
                            if (bufferReader.Buffer[offset + length - 2] == '=')
                                expectedLength--;
                        }
                    }
                    byte[] buffer = new byte[expectedLength];
                    int actualLength = Base64Encoding.GetBytes(bufferReader.Buffer, this.offset, this.length, buffer, 0);
                    if (actualLength != buffer.Length)
                    {
                        byte[] newBuffer = new byte[actualLength];
                        Buffer.BlockCopy(buffer, 0, newBuffer, 0, actualLength);
                        buffer = newBuffer;
                    }
                    return buffer;
                }
                catch (FormatException)
                {
                    // Something unhappy with the characters, fall back to the hard way
                }
            }
            try
            {
                return Base64Encoding.GetBytes(XmlConverter.StripWhitespace(GetString()));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(exception.Message, exception.InnerException));
            }
        }

        public string GetString()
        {
            ValueHandleType type = this.type;
            if (type == ValueHandleType.UTF8)
                return GetCharsText();

            switch (type)
            {
                case ValueHandleType.False:
                    return "false";
                case ValueHandleType.True:
                    return "true";
                case ValueHandleType.Zero:
                    return "0";
                case ValueHandleType.One:
                    return "1";
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return XmlConverter.ToString(ToInt());
                case ValueHandleType.Int64:
                    return XmlConverter.ToString(GetInt64());
                case ValueHandleType.UInt64:
                    return XmlConverter.ToString(GetUInt64());
                case ValueHandleType.Single:
                    return XmlConverter.ToString(GetSingle());
                case ValueHandleType.Double:
                    return XmlConverter.ToString(GetDouble());
                case ValueHandleType.Decimal:
                    return XmlConverter.ToString(GetDecimal());
                case ValueHandleType.DateTime:
                    return XmlConverter.ToString(ToDateTime());
                case ValueHandleType.Empty:
                    return string.Empty;
                case ValueHandleType.UTF8:
                    return GetCharsText();
                case ValueHandleType.Unicode:
                    return GetUnicodeCharsText();
                case ValueHandleType.EscapedUTF8:
                    return GetEscapedCharsText();
                case ValueHandleType.Char:
                    return GetCharText();
                case ValueHandleType.Dictionary:
                    return GetDictionaryString().Value;
                case ValueHandleType.Base64:
                    return Base64Encoding.GetString(ToByteArray());
                case ValueHandleType.List:
                    return XmlConverter.ToString(ToList());
                case ValueHandleType.UniqueId:
                    return XmlConverter.ToString(ToUniqueId());
                case ValueHandleType.Guid:
                    return XmlConverter.ToString(ToGuid());
                case ValueHandleType.TimeSpan:
                    return XmlConverter.ToString(ToTimeSpan());
                case ValueHandleType.QName:
                    return GetQNameDictionaryText();
                case ValueHandleType.ConstString:
                    return constStrings[offset];
                default:
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }

        // ASSUMPTION (Microsoft): all chars in str will be ASCII
        public bool Equals2(string str, bool checkLower)
        {
            if (this.type != ValueHandleType.UTF8)
                return GetString() == str;

            if (this.length != str.Length)
                return false;

            byte[] buffer = bufferReader.Buffer;
            for (int i = 0; i < this.length; ++i)
            {
                Fx.Assert(str[i] < 128, "");
                byte ch = buffer[i + this.offset];
                if (ch == str[i])
                    continue;

                if (checkLower && char.ToLowerInvariant((char)ch) == str[i])
                    continue;

                return false;
            }

            return true;
        }

        public void Sign(XmlSigningNodeWriter writer)
        {
            switch (type)
            {
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    writer.WriteInt32Text(ToInt());
                    break;
                case ValueHandleType.Int64:
                    writer.WriteInt64Text(GetInt64());
                    break;
                case ValueHandleType.UInt64:
                    writer.WriteUInt64Text(GetUInt64());
                    break;
                case ValueHandleType.Single:
                    writer.WriteFloatText(GetSingle());
                    break;
                case ValueHandleType.Double:
                    writer.WriteDoubleText(GetDouble());
                    break;
                case ValueHandleType.Decimal:
                    writer.WriteDecimalText(GetDecimal());
                    break;
                case ValueHandleType.DateTime:
                    writer.WriteDateTimeText(ToDateTime());
                    break;
                case ValueHandleType.Empty:
                    break;
                case ValueHandleType.UTF8:
                    writer.WriteEscapedText(bufferReader.Buffer, offset, length);
                    break;
                case ValueHandleType.Base64:
                    writer.WriteBase64Text(bufferReader.Buffer, 0, bufferReader.Buffer, offset, length);
                    break;
                case ValueHandleType.UniqueId:
                    writer.WriteUniqueIdText(ToUniqueId());
                    break;
                case ValueHandleType.Guid:
                    writer.WriteGuidText(ToGuid());
                    break;
                case ValueHandleType.TimeSpan:
                    writer.WriteTimeSpanText(ToTimeSpan());
                    break;
                default:
                    writer.WriteEscapedText(GetString());
                    break;
            }
        }

        public object[] ToList()
        {
            return bufferReader.GetList(offset, length);
        }

        public object ToObject()
        {
            switch (type)
            {
                case ValueHandleType.False:
                case ValueHandleType.True:
                    return ToBoolean();
                case ValueHandleType.Zero:
                case ValueHandleType.One:
                case ValueHandleType.Int8:
                case ValueHandleType.Int16:
                case ValueHandleType.Int32:
                    return ToInt();
                case ValueHandleType.Int64:
                    return ToLong();
                case ValueHandleType.UInt64:
                    return GetUInt64();
                case ValueHandleType.Single:
                    return ToSingle();
                case ValueHandleType.Double:
                    return ToDouble();
                case ValueHandleType.Decimal:
                    return ToDecimal();
                case ValueHandleType.DateTime:
                    return ToDateTime();
                case ValueHandleType.Empty:
                case ValueHandleType.UTF8:
                case ValueHandleType.Unicode:
                case ValueHandleType.EscapedUTF8:
                case ValueHandleType.Dictionary:
                case ValueHandleType.Char:
                case ValueHandleType.ConstString:
                    return ToString();
                case ValueHandleType.Base64:
                    return ToByteArray();
                case ValueHandleType.List:
                    return ToList();
                case ValueHandleType.UniqueId:
                    return ToUniqueId();
                case ValueHandleType.Guid:
                    return ToGuid();
                case ValueHandleType.TimeSpan:
                    return ToTimeSpan();
                default:
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }

        public bool TryReadBase64(byte[] buffer, int offset, int count, out int actual)
        {
            if (type == ValueHandleType.Base64)
            {
                actual = Math.Min(this.length, count);
                GetBase64(buffer, offset, actual);
                this.offset += actual;
                this.length -= actual;
                return true;
            }
            if (type == ValueHandleType.UTF8 && count >= 3 && (this.length % 4) == 0)
            {
                try
                {
                    int charCount = Math.Min(count / 3 * 4, this.length);
                    actual = Base64Encoding.GetBytes(bufferReader.Buffer, this.offset, charCount, buffer, offset);
                    this.offset += charCount;
                    this.length -= charCount;
                    return true;
                }
                catch (FormatException)
                {
                    // Something unhappy with the characters, fall back to the hard way
                }
            }
            actual = 0;
            return false;
        }

        public bool TryReadChars(char[] chars, int offset, int count, out int actual)
        {
            Fx.Assert(offset + count <= chars.Length, string.Format("offset '{0}' + count '{1}' MUST BE <= chars.Length '{2}'", offset, count, chars.Length)); 

            if (type == ValueHandleType.Unicode)
                return TryReadUnicodeChars(chars, offset, count, out actual);

            if (type != ValueHandleType.UTF8)
            {
                actual = 0;
                return false;
            }

            int charOffset = offset;
            int charCount = count;
            byte[] bytes = bufferReader.Buffer;
            int byteOffset = this.offset;
            int byteCount = this.length;
            bool insufficientSpaceInCharsArray = false; 

            while (true)
            {
                while (charCount > 0 && byteCount > 0)
                {
                    // fast path for codepoints U+0000 - U+007F
                    byte b = bytes[byteOffset];
                    if (b >= 0x80)
                        break;
                    chars[charOffset] = (char)b;
                    byteOffset++;
                    byteCount--;
                    charOffset++;
                    charCount--;
                }

                if (charCount == 0 || byteCount == 0 || insufficientSpaceInCharsArray)
                    break;

                int actualByteCount;
                int actualCharCount;

                UTF8Encoding encoding = new UTF8Encoding(false, true);
                try
                {
                    // If we're asking for more than are possibly available, or more than are truly available then we can return the entire thing
                    if (charCount >= encoding.GetMaxCharCount(byteCount) || charCount >= encoding.GetCharCount(bytes, byteOffset, byteCount))
                    {
                        actualCharCount = encoding.GetChars(bytes, byteOffset, byteCount, chars, charOffset);
                        actualByteCount = byteCount;
                    }
                    else
                    {
                        Decoder decoder = encoding.GetDecoder();

                        // Since x bytes can never generate more than x characters this is a safe estimate as to what will fit
                        actualByteCount = Math.Min(charCount, byteCount);

                        // We use a decoder so we don't error if we fall across a character boundary
                        actualCharCount = decoder.GetChars(bytes, byteOffset, actualByteCount, chars, charOffset);

                        // We might've gotten zero characters though if < 4 bytes were requested because
                        // codepoints from U+0000 - U+FFFF can be up to 3 bytes in UTF-8, and represented as ONE char
                        // codepoints from U+10000 - U+10FFFF (last Unicode codepoint representable in UTF-8) are represented by up to 4 bytes in UTF-8 
                        //                                    and represented as TWO chars (high+low surrogate)
                        // (e.g. 1 char requested, 1 char in the buffer represented in 3 bytes)
                        while (actualCharCount == 0)
                        {
                            // Note the by the time we arrive here, if actualByteCount == 3, the next decoder.GetChars() call will read the 4th byte
                            // if we don't bail out since the while loop will advance actualByteCount only after reading the byte. 
                            if (actualByteCount >= 3 && charCount < 2)
                            {
                                // If we reach here, it means that we're: 
                                // - trying to decode more than 3 bytes and, 
                                // - there is only one char left of charCount where we're stuffing decoded characters. 
                                // In this case, we need to back off since decoding > 3 bytes in UTF-8 means that we will get 2 16-bit chars 
                                // (a high surrogate and a low surrogate) - the Decoder will attempt to provide both at once 
                                // and an ArgumentException will be thrown complaining that there's not enough space in the output char array.  

                                // actualByteCount = 0 when the while loop is broken out of; decoder goes out of scope so its state no longer matters

                                insufficientSpaceInCharsArray = true; 
                                break; 
                            }
                            else
                            {
                                Fx.Assert(byteOffset + actualByteCount < bytes.Length, 
                                    string.Format("byteOffset {0} + actualByteCount {1} MUST BE < bytes.Length {2}", byteOffset, actualByteCount, bytes.Length));
                                
                                // Request a few more bytes to get at least one character
                                actualCharCount = decoder.GetChars(bytes, byteOffset + actualByteCount, 1, chars, charOffset);
                                actualByteCount++;
                            }
                        }

                        // Now that we actually retrieved some characters, figure out how many bytes it actually was
                        actualByteCount = encoding.GetByteCount(chars, charOffset, actualCharCount);
                    }
                }
                catch (FormatException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateEncodingException(bytes, byteOffset, byteCount, exception));
                }

                // Advance
                byteOffset += actualByteCount;
                byteCount -= actualByteCount;

                charOffset += actualCharCount;
                charCount -= actualCharCount;
            }

            this.offset = byteOffset;
            this.length = byteCount;

            actual = (count - charCount);
            return true;
        }

        bool TryReadUnicodeChars(char[] chars, int offset, int count, out int actual)
        {
            int charCount = Math.Min(count, this.length / sizeof(char));
            for (int i = 0; i < charCount; i++)
            {
                chars[offset + i] = (char)bufferReader.GetInt16(this.offset + i * sizeof(char));
            }
            this.offset += charCount * sizeof(char);
            this.length -= charCount * sizeof(char);
            actual = charCount;
            return true;
        }

        public bool TryGetDictionaryString(out XmlDictionaryString value)
        {
            if (type == ValueHandleType.Dictionary)
            {
                value = GetDictionaryString();
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool TryGetByteArrayLength(out int length)
        {
            if (type == ValueHandleType.Base64)
            {
                length = this.length;
                return true;
            }
            length = 0;
            return false;
        }

        string GetCharsText()
        {
            Fx.Assert(type == ValueHandleType.UTF8, "");
            if (length == 1 && bufferReader.GetByte(offset) == '1')
                return "1";
            return bufferReader.GetString(offset, length);
        }

        string GetUnicodeCharsText()
        {
            Fx.Assert(type == ValueHandleType.Unicode, "");
            return bufferReader.GetUnicodeString(offset, length);
        }

        string GetEscapedCharsText()
        {
            Fx.Assert(type == ValueHandleType.EscapedUTF8, "");
            return bufferReader.GetEscapedString(offset, length);
        }

        string GetCharText()
        {
            int ch = GetChar();
            if (ch > char.MaxValue)
            {
                SurrogateChar surrogate = new SurrogateChar(ch);
                char[] chars = new char[2];
                chars[0] = surrogate.HighChar;
                chars[1] = surrogate.LowChar;
                return new string(chars, 0, 2);
            }
            else
            {
                return ((char)ch).ToString();
            }
        }

        int GetChar()
        {
            Fx.Assert(type == ValueHandleType.Char, "");
            return offset;
        }

        int GetInt8()
        {
            Fx.Assert(type == ValueHandleType.Int8, "");
            return bufferReader.GetInt8(offset);
        }

        int GetInt16()
        {
            Fx.Assert(type == ValueHandleType.Int16, "");
            return bufferReader.GetInt16(offset);
        }

        int GetInt32()
        {
            Fx.Assert(type == ValueHandleType.Int32, "");
            return bufferReader.GetInt32(offset);
        }

        long GetInt64()
        {
            Fx.Assert(type == ValueHandleType.Int64 || type == ValueHandleType.TimeSpan || type == ValueHandleType.DateTime, "");
            return bufferReader.GetInt64(offset);
        }

        ulong GetUInt64()
        {
            Fx.Assert(type == ValueHandleType.UInt64, "");
            return bufferReader.GetUInt64(offset);
        }

        float GetSingle()
        {
            Fx.Assert(type == ValueHandleType.Single, "");
            return bufferReader.GetSingle(offset);
        }

        double GetDouble()
        {
            Fx.Assert(type == ValueHandleType.Double, "");
            return bufferReader.GetDouble(offset);
        }

        decimal GetDecimal()
        {
            Fx.Assert(type == ValueHandleType.Decimal, "");
            return bufferReader.GetDecimal(offset);
        }

        UniqueId GetUniqueId()
        {
            Fx.Assert(type == ValueHandleType.UniqueId, "");
            return bufferReader.GetUniqueId(offset);
        }

        Guid GetGuid()
        {
            Fx.Assert(type == ValueHandleType.Guid, "");
            return bufferReader.GetGuid(offset);
        }

        void GetBase64(byte[] buffer, int offset, int count)
        {
            Fx.Assert(type == ValueHandleType.Base64, "");
            bufferReader.GetBase64(this.offset, buffer, offset, count);
        }

        XmlDictionaryString GetDictionaryString()
        {
            Fx.Assert(type == ValueHandleType.Dictionary, "");
            return bufferReader.GetDictionaryString(offset);
        }

        string GetQNameDictionaryText()
        {
            Fx.Assert(type == ValueHandleType.QName, "");
            return string.Concat(PrefixHandle.GetString(PrefixHandle.GetAlphaPrefix(length)), ":", bufferReader.GetDictionaryString(offset));
        }
    }
}
