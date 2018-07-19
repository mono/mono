//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    class XmlBufferReader
    {
        XmlDictionaryReader reader;
        Stream stream;
        byte[] streamBuffer;
        byte[] buffer;
        int offsetMin;
        int offsetMax;
        IXmlDictionary dictionary;
        XmlBinaryReaderSession session;
        byte[] guid;
        int offset;
        const int maxBytesPerChar = 3;
        char[] chars;
        int windowOffset;
        int windowOffsetMax;
        ValueHandle listValue;
        static byte[] emptyByteArray = new byte[0];
        static XmlBufferReader empty = new XmlBufferReader(emptyByteArray);

        public XmlBufferReader(XmlDictionaryReader reader)
        {
            this.reader = reader;
        }

        public XmlBufferReader(byte[] buffer)
        {
            this.reader = null;
            this.buffer = buffer;
        }

        static public XmlBufferReader Empty
        {
            get
            {
                return empty;
            }
        }

        public byte[] Buffer
        {
            get
            {
                return buffer;
            }
        }

        public bool IsStreamed
        {
            get
            {
                return stream != null;
            }
        }

        public void SetBuffer(Stream stream, IXmlDictionary dictionary, XmlBinaryReaderSession session)
        {
            if (streamBuffer == null)
            {
                streamBuffer = new byte[128];
            }
            SetBuffer(stream, streamBuffer, 0, 0, dictionary, session);
            this.windowOffset = 0;
            this.windowOffsetMax = streamBuffer.Length;
        }

        public void SetBuffer(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlBinaryReaderSession session)
        {
            SetBuffer(null, buffer, offset, count, dictionary, session);
        }

        void SetBuffer(Stream stream, byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlBinaryReaderSession session)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.offsetMin = offset;
            this.offset = offset;
            this.offsetMax = offset + count;
            this.dictionary = dictionary;
            this.session = session;
        }

        public void Close()
        {
            if (streamBuffer != null && streamBuffer.Length > 4096)
            {
                streamBuffer = null;
            }
            if (stream != null)
            {
                stream.Close();
                this.stream = null;
            }
            this.buffer = emptyByteArray;
            this.offset = 0;
            this.offsetMax = 0;
            this.windowOffset = 0;
            this.windowOffsetMax = 0;
            this.dictionary = null;
            this.session = null;
        }

        public bool EndOfFile
        {
            get
            {
                return offset == offsetMax && !TryEnsureByte();
            }
        }

        public byte GetByte()
        {
            int offset = this.offset;
            if (offset < offsetMax)
                return buffer[offset];
            else
                return GetByteHard();
        }

        public void SkipByte()
        {
            Advance(1);
        }

        byte GetByteHard()
        {
            EnsureByte();
            return buffer[offset];
        }

        public byte[] GetBuffer(int count, out int offset)
        {
            offset = this.offset;
            if (offset <= this.offsetMax - count)
                return buffer;
            return GetBufferHard(count, out offset);
        }

        public byte[] GetBuffer(int count, out int offset, out int offsetMax)
        {
            offset = this.offset;
            if (offset <= this.offsetMax - count)
            {
                offsetMax = this.offset + count;
            }
            else
            {
                TryEnsureBytes(Math.Min(count, windowOffsetMax - offset));
                offsetMax = this.offsetMax;
            }
            return buffer;
        }

        public byte[] GetBuffer(out int offset, out int offsetMax)
        {
            offset = this.offset;
            offsetMax = this.offsetMax;
            return buffer;
        }

        byte[] GetBufferHard(int count, out int offset)
        {
            offset = this.offset;
            EnsureBytes(count);
            return buffer;
        }

        void EnsureByte()
        {
            if (!TryEnsureByte())
                XmlExceptionHelper.ThrowUnexpectedEndOfFile(reader);
        }

        bool TryEnsureByte()
        {
            if (stream == null)
                return false;
            if (offsetMax >= windowOffsetMax)
                XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(reader, windowOffsetMax - windowOffset);
            if (offsetMax >= buffer.Length)
                return TryEnsureBytes(1);
            int b = stream.ReadByte();
            if (b == -1)
                return false;
            buffer[offsetMax++] = (byte)b;
            return true;
        }

        void EnsureBytes(int count)
        {
            if (!TryEnsureBytes(count))
                XmlExceptionHelper.ThrowUnexpectedEndOfFile(reader);
        }

        bool TryEnsureBytes(int count)
        {
            if (stream == null)
                return false;
            if (offset > int.MaxValue - count)
                XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(reader, windowOffsetMax - windowOffset);
            int newOffsetMax = offset + count;
            if (newOffsetMax < offsetMax)
                return true;
            if (newOffsetMax > windowOffsetMax)
                XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(reader, windowOffsetMax - windowOffset);
            if (newOffsetMax > buffer.Length)
            {
                byte[] newBuffer = new byte[Math.Max(newOffsetMax, buffer.Length * 2)];
                System.Buffer.BlockCopy(this.buffer, 0, newBuffer, 0, offsetMax);
                buffer = newBuffer;
                streamBuffer = newBuffer;
            }
            int needed = newOffsetMax - offsetMax;
            while (needed > 0)
            {
                int actual = stream.Read(buffer, offsetMax, needed);
                if (actual == 0)
                    return false;
                offsetMax += actual;
                needed -= actual;
            }
            return true;
        }

#if NO
        void ReadByte(byte b)
        {
            if (BufferReader.GetByte() != b)
                XmlExceptionHelper.ThrowTokenExpected(this, ((char)b).ToString(), (char)BufferReader.GetByte());
        }
#endif
        public void Advance(int count)
        {
            Fx.Assert(this.offset + count <= offsetMax, "");
            this.offset += count;
        }

        public void InsertBytes(byte[] buffer, int offset, int count)
        {
            Fx.Assert(stream != null, "");
            if (offsetMax > buffer.Length - count)
            {
                byte[] newBuffer = new byte[offsetMax + count];
                System.Buffer.BlockCopy(this.buffer, 0, newBuffer, 0, this.offsetMax);
                this.buffer = newBuffer;
                this.streamBuffer = newBuffer;
            }
            System.Buffer.BlockCopy(this.buffer, this.offset, this.buffer, this.offset + count, this.offsetMax - this.offset);
            offsetMax += count;
            System.Buffer.BlockCopy(buffer, offset, this.buffer, this.offset, count);
        }

        public void SetWindow(int windowOffset, int windowLength)
        {
            // [0...elementOffset-1][elementOffset..offset][offset..offsetMax-1][offsetMax..buffer.Length]
            // ^--Elements, Attributes in scope
            //                      ^-- The node just consumed
            //                                             ^--Data buffered, not consumed
            //                                                                  ^--Unused space
            if (windowOffset > int.MaxValue - windowLength)
                windowLength = int.MaxValue - windowOffset;

            if (offset != windowOffset)
            {
                System.Buffer.BlockCopy(buffer, offset, buffer, windowOffset, offsetMax - offset);
                offsetMax = windowOffset + (offsetMax - offset);
                offset = windowOffset;
            }
            this.windowOffset = windowOffset;
            this.windowOffsetMax = Math.Max(windowOffset + windowLength, offsetMax);
        }

        public int Offset
        {
            get
            {
                return offset;
            }
            set
            {
                Fx.Assert(value >= offsetMin && value <= offsetMax, "");
                this.offset = value;
            }
        }

        public int ReadBytes(int count)
        {
            Fx.Assert(count >= 0, "");
            int offset = this.offset;
            if (offset > offsetMax - count)
                EnsureBytes(count);
            this.offset += count;
            return offset;
        }

        public int ReadMultiByteUInt31()
        {
            int i = GetByte();
            Advance(1);
            if ((i & 0x80) == 0)
                return i;
            i &= 0x7F;

            int j = GetByte();
            Advance(1);
            i |= ((j & 0x7F) << 7);
            if ((j & 0x80) == 0)
                return i;

            int k = GetByte();
            Advance(1);
            i |= ((k & 0x7F) << 14);
            if ((k & 0x80) == 0)
                return i;

            int l = GetByte();
            Advance(1);
            i |= ((l & 0x7F) << 21);
            if ((l & 0x80) == 0)
                return i;

            int m = GetByte();
            Advance(1);
            i |= (m << 28);
            if ((m & 0xF8) != 0)
                XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);

            return i;
        }

        public int ReadUInt8()
        {
            byte b = GetByte();
            Advance(1);
            return b;
        }

        public int ReadInt8()
        {
            return (sbyte)ReadUInt8();
        }

        public int ReadUInt16()
        {
            int offset;
            byte[] buffer = GetBuffer(2, out offset);
            int i = buffer[offset + 0] + (buffer[offset + 1] << 8);
            Advance(2);
            return i;
        }

        public int ReadInt16()
        {
            return (Int16)ReadUInt16();
        }

        public int ReadInt32()
        {
            int offset;
            byte[] buffer = GetBuffer(4, out offset);
            byte b1 = buffer[offset + 0];
            byte b2 = buffer[offset + 1];
            byte b3 = buffer[offset + 2];
            byte b4 = buffer[offset + 3];
            Advance(4);
            return (((((b4 << 8) + b3) << 8) + b2) << 8) + b1;
        }

        public int ReadUInt31()
        {
            int i = ReadInt32();
            if (i < 0)
                XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
            return i;
        }

        public long ReadInt64()
        {
            Int64 lo = (UInt32)ReadInt32();
            Int64 hi = (UInt32)ReadInt32();
            return (hi << 32) + lo;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public float ReadSingle()
        {
            int offset;
            byte[] buffer = GetBuffer(ValueHandleLength.Single, out offset);
            float value;
            byte* pb = (byte*)&value;
            Fx.Assert(sizeof(float) == 4, "");
            pb[0] = buffer[offset + 0];
            pb[1] = buffer[offset + 1];
            pb[2] = buffer[offset + 2];
            pb[3] = buffer[offset + 3];
            Advance(ValueHandleLength.Single);
            return value;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public double ReadDouble()
        {
            int offset;
            byte[] buffer = GetBuffer(ValueHandleLength.Double, out offset);
            double value;
            byte* pb = (byte*)&value;
            Fx.Assert(sizeof(double) == 8, "");
            pb[0] = buffer[offset + 0];
            pb[1] = buffer[offset + 1];
            pb[2] = buffer[offset + 2];
            pb[3] = buffer[offset + 3];
            pb[4] = buffer[offset + 4];
            pb[5] = buffer[offset + 5];
            pb[6] = buffer[offset + 6];
            pb[7] = buffer[offset + 7];
            Advance(ValueHandleLength.Double);
            return value;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public decimal ReadDecimal()
        {
            const int SignMask = unchecked((int)0x80000000);
            const int ScaleMask = 0x00FF0000;

            int offset;
            byte[] buffer = GetBuffer(ValueHandleLength.Decimal, out offset);
            byte b1 = buffer[offset + 0];
            byte b2 = buffer[offset + 1];
            byte b3 = buffer[offset + 2];
            byte b4 = buffer[offset + 3];
            int flags = (((((b4 << 8) + b3) << 8) + b2) << 8) + b1;

            //this logic mirrors the code in Decimal(int []) ctor.
            if ((flags & ~(SignMask | ScaleMask)) == 0 && (flags & ScaleMask) <= (28 << 16))
            {
                decimal value;
                byte* pb = (byte*)&value;
                for (int i = 0; i < sizeof(decimal); i++)
                    pb[i] = buffer[offset + i];

                Advance(ValueHandleLength.Decimal);
                return value;
            }
            else
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }

            //compiler doesn't know that XmlExceptionHelper.ThrowInvalidBinaryFormat always throws, 
            //so we have to have a return statement here even though we shouldn't hit this code path...
            Fx.Assert("A decimal value should have been returned or an exception should have been thrown.");
            return default(decimal);
        }

        public UniqueId ReadUniqueId()
        {
            int offset;
            byte[] buffer = GetBuffer(ValueHandleLength.UniqueId, out offset);
            UniqueId uniqueId = new UniqueId(buffer, offset);
            Advance(ValueHandleLength.UniqueId);
            return uniqueId;
        }

        public DateTime ReadDateTime()
        {
            long value = 0;
            try
            {
                value = ReadInt64();
                return DateTime.FromBinary(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "DateTime", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "DateTime", exception));
            }
            catch (OverflowException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "DateTime", exception));
            }
        }

        public TimeSpan ReadTimeSpan()
        {
            long value = 0;
            try
            {
                value = ReadInt64();
                return TimeSpan.FromTicks(value);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "TimeSpan", exception));
            }
            catch (FormatException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "TimeSpan", exception));
            }
            catch (OverflowException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(value.ToString(CultureInfo.InvariantCulture), "TimeSpan", exception));
            }
        }

        public Guid ReadGuid()
        {
            int offset;
            byte[] buffer = GetBuffer(ValueHandleLength.Guid, out offset);
            Guid guid = GetGuid(offset);
            Advance(ValueHandleLength.Guid);
            return guid;
        }

        public string ReadUTF8String(int length)
        {
            int offset;
            byte[] buffer = GetBuffer(length, out offset);
            char[] chars = GetCharBuffer(length);
            int charCount = GetChars(offset, length, chars);
            string value = new string(chars, 0, charCount);
            Advance(length);
            return value;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe public void UnsafeReadArray(byte* dst, byte* dstMax)
        {
            UnsafeReadArray(dst, (int)(dstMax - dst));
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe void UnsafeReadArray(byte* dst, int length)
        {
            if (stream != null)
            {
                const int chunk = 256;
                while (length >= chunk)
                {
                    byte[] _buffer = GetBuffer(chunk, out offset);
                    for (int i = 0; i < chunk; i++)
                    {
                        *dst++ = _buffer[offset + i];
                    }
                    Advance(chunk);
                    length -= chunk;
                }
            }

            if (length > 0)
            {
                byte[] buffer = GetBuffer(length, out offset);
                fixed (byte* _src = &buffer[offset])
                {
                    byte* src = _src;
                    byte* dstMax = dst + length;
                    while (dst < dstMax)
                    {
                        *dst = *src;
                        dst++;
                        src++;
                    }
                }
                Advance(length);
            }
        }

        char[] GetCharBuffer(int count)
        {
            if (count > 1024)
                return new char[count];
            if (chars == null || chars.Length < count)
                chars = new char[count];
            return chars;
        }

        int GetChars(int offset, int length, char[] chars)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i++)
            {
                byte b = buffer[offset + i];
                if (b >= 0x80)
                    return i + XmlConverter.ToChars(buffer, offset + i, length - i, chars, i);
                chars[i] = (char)b;
            }
            return length;
        }

        int GetChars(int offset, int length, char[] chars, int charOffset)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i++)
            {
                byte b = buffer[offset + i];
                if (b >= 0x80)
                    return i + XmlConverter.ToChars(buffer, offset + i, length - i, chars, charOffset + i);
                chars[charOffset + i] = (char)b;
            }
            return length;
        }

        public string GetString(int offset, int length)
        {
            char[] chars = GetCharBuffer(length);
            int charCount = GetChars(offset, length, chars);
            return new string(chars, 0, charCount);
        }

        public string GetUnicodeString(int offset, int length)
        {
            return XmlConverter.ToStringUnicode(buffer, offset, length);
        }

        public string GetString(int offset, int length, XmlNameTable nameTable)
        {
            char[] chars = GetCharBuffer(length);
            int charCount = GetChars(offset, length, chars);
            return nameTable.Add(chars, 0, charCount);
        }

        public int GetEscapedChars(int offset, int length, char[] chars)
        {
            byte[] buffer = this.buffer;
            int charCount = 0;
            int textOffset = offset;
            int offsetMax = offset + length;
            while (true)
            {
                while (offset < offsetMax && IsAttrChar(buffer[offset]))
                    offset++;
                charCount += GetChars(textOffset, offset - textOffset, chars, charCount);
                if (offset == offsetMax)
                    break;
                textOffset = offset;
                if (buffer[offset] == '&')
                {
                    while (offset < offsetMax && buffer[offset] != ';')
                        offset++;
                    offset++;
                    int ch = GetCharEntity(textOffset, offset - textOffset);
                    textOffset = offset;
                    if (ch > char.MaxValue)
                    {
                        SurrogateChar surrogate = new SurrogateChar(ch);
                        chars[charCount++] = surrogate.HighChar;
                        chars[charCount++] = surrogate.LowChar;
                    }
                    else
                    {
                        chars[charCount++] = (char)ch;
                    }
                }
                else if (buffer[offset] == '\n' || buffer[offset] == '\t')
                {
                    chars[charCount++] = ' ';
                    offset++;
                    textOffset = offset;
                }
                else // '\r'
                {
                    chars[charCount++] = ' ';
                    offset++;

                    if (offset < offsetMax && buffer[offset] == '\n')
                        offset++;

                    textOffset = offset;
                }

            }
            return charCount;
        }

        bool IsAttrChar(int ch)
        {
            switch (ch)
            {
                case '&':
                case '\r':
                case '\t':
                case '\n':
                    return false;

                default:
                    return true;
            }
        }

        public string GetEscapedString(int offset, int length)
        {
            char[] chars = GetCharBuffer(length);
            int charCount = GetEscapedChars(offset, length, chars);
            return new string(chars, 0, charCount);
        }

        public string GetEscapedString(int offset, int length, XmlNameTable nameTable)
        {
            char[] chars = GetCharBuffer(length);
            int charCount = GetEscapedChars(offset, length, chars);
            return nameTable.Add(chars, 0, charCount);
        }

        int GetLessThanCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (length != 4 ||
                buffer[offset + 1] != (byte)'l' ||
                buffer[offset + 2] != (byte)'t')
            {
                XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return (int)'<';
        }

        int GetGreaterThanCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (length != 4 ||
                buffer[offset + 1] != (byte)'g' ||
                buffer[offset + 2] != (byte)'t')
            {
                XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return (int)'>';
        }

        int GetQuoteCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (length != 6 ||
                buffer[offset + 1] != (byte)'q' ||
                buffer[offset + 2] != (byte)'u' ||
                buffer[offset + 3] != (byte)'o' ||
                buffer[offset + 4] != (byte)'t')
            {
                XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return (int)'"';
        }

        int GetAmpersandCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (length != 5 ||
                buffer[offset + 1] != (byte)'a' ||
                buffer[offset + 2] != (byte)'m' ||
                buffer[offset + 3] != (byte)'p')
            {
                XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return (int)'&';
        }

        int GetApostropheCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (length != 6 ||
                buffer[offset + 1] != (byte)'a' ||
                buffer[offset + 2] != (byte)'p' ||
                buffer[offset + 3] != (byte)'o' ||
                buffer[offset + 4] != (byte)'s')
            {
                XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return (int)'\'';
        }

        int GetDecimalCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            Fx.Assert(buffer[offset + 0] == '&', "");
            Fx.Assert(buffer[offset + 1] == '#', "");
            Fx.Assert(buffer[offset + length - 1] == ';', "");
            int value = 0;
            for (int i = 2; i < length - 1; i++)
            {
                byte ch = buffer[offset + i];
                if (ch < (byte)'0' || ch > (byte)'9')
                    XmlExceptionHelper.ThrowInvalidCharRef(reader);
                value = value * 10 + (ch - '0');
                if (value > SurrogateChar.MaxValue)
                    XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return value;
        }

        int GetHexCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            Fx.Assert(buffer[offset + 0] == '&', "");
            Fx.Assert(buffer[offset + 1] == '#', "");
            Fx.Assert(buffer[offset + 2] == 'x', "");
            Fx.Assert(buffer[offset + length - 1] == ';', "");
            int value = 0;
            for (int i = 3; i < length - 1; i++)
            {
                byte ch = buffer[offset + i];
                int digit = 0;
                if (ch >= '0' && ch <= '9')
                    digit = (ch - '0');
                else if (ch >= 'a' && ch <= 'f')
                    digit = 10 + (ch - 'a');
                else if (ch >= 'A' && ch <= 'F')
                    digit = 10 + (ch - 'A');
                else
                    XmlExceptionHelper.ThrowInvalidCharRef(reader);
                Fx.Assert(digit >= 0 && digit < 16, "");
                value = value * 16 + digit;
                if (value > SurrogateChar.MaxValue)
                    XmlExceptionHelper.ThrowInvalidCharRef(reader);
            }
            return value;
        }

        public int GetCharEntity(int offset, int length)
        {
            if (length < 3)
                XmlExceptionHelper.ThrowInvalidCharRef(reader);
            byte[] buffer = this.buffer;
            Fx.Assert(buffer[offset] == '&', "");
            Fx.Assert(buffer[offset + length - 1] == ';', "");
            switch (buffer[offset + 1])
            {
                case (byte)'l':
                    return GetLessThanCharEntity(offset, length);
                case (byte)'g':
                    return GetGreaterThanCharEntity(offset, length);
                case (byte)'a':
                    if (buffer[offset + 2] == (byte)'m')
                        return GetAmpersandCharEntity(offset, length);
                    else
                        return GetApostropheCharEntity(offset, length);
                case (byte)'q':
                    return GetQuoteCharEntity(offset, length);
                case (byte)'#':
                    if (buffer[offset + 2] == (byte)'x')
                        return GetHexCharEntity(offset, length);
                    else
                        return GetDecimalCharEntity(offset, length);
                default:
                    XmlExceptionHelper.ThrowInvalidCharRef(reader);
                    return 0;
            }
        }

        public bool IsWhitespaceKey(int key)
        {
            string s = GetDictionaryString(key).Value;
            for (int i = 0; i < s.Length; i++)
            {
                if (!XmlConverter.IsWhitespace(s[i]))
                    return false;
            }
            return true;
        }

        public bool IsWhitespaceUTF8(int offset, int length)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i++)
            {
                if (!XmlConverter.IsWhitespace((char)buffer[offset + i]))
                    return false;
            }
            return true;
        }

        public bool IsWhitespaceUnicode(int offset, int length)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i += sizeof(char))
            {
                char ch = (char)GetInt16(offset + i);
                if (!XmlConverter.IsWhitespace(ch))
                    return false;
            }
            return true;
        }

        public bool Equals2(int key1, int key2, XmlBufferReader bufferReader2)
        {
            // If the keys aren't from the same dictionary, they still might be the same
            if (key1 == key2)
                return true;
            else
                return GetDictionaryString(key1).Value == bufferReader2.GetDictionaryString(key2).Value;
        }

        public bool Equals2(int key1, XmlDictionaryString xmlString2)
        {
            if ((key1 & 1) == 0 && xmlString2.Dictionary == dictionary)
                return xmlString2.Key == (key1 >> 1);
            else
                return GetDictionaryString(key1).Value == xmlString2.Value;
        }

        public bool Equals2(int offset1, int length1, byte[] buffer2)
        {
            int length2 = buffer2.Length;
            if (length1 != length2)
                return false;
            byte[] buffer1 = this.buffer;
            for (int i = 0; i < length1; i++)
            {
                if (buffer1[offset1 + i] != buffer2[i])
                    return false;
            }
            return true;
        }

        public bool Equals2(int offset1, int length1, XmlBufferReader bufferReader2, int offset2, int length2)
        {
            if (length1 != length2)
                return false;
            byte[] buffer1 = this.buffer;
            byte[] buffer2 = bufferReader2.buffer;
            for (int i = 0; i < length1; i++)
            {
                if (buffer1[offset1 + i] != buffer2[offset2 + i])
                    return false;
            }
            return true;
        }

        public bool Equals2(int offset1, int length1, int offset2, int length2)
        {
            if (length1 != length2)
                return false;
            if (offset1 == offset2)
                return true;
            byte[] buffer = this.buffer;
            for (int i = 0; i < length1; i++)
            {
                if (buffer[offset1 + i] != buffer[offset2 + i])
                    return false;
            }
            return true;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public bool Equals2(int offset1, int length1, string s2)
        {
            int byteLength = length1;
            int charLength = s2.Length;

            // N unicode chars will be represented in at least N bytes, but
            // no more than N * 3 bytes.  If the byte count falls outside of this
            // range, then the strings cannot be equal.
            if (byteLength < charLength || byteLength > charLength * maxBytesPerChar)
                return false;

            byte[] buffer = this.buffer;
            if (length1 < 8)
            {
                int length = Math.Min(byteLength, charLength);
                int offset = offset1;
                for (int i = 0; i < length; i++)
                {
                    byte b = buffer[offset + i];
                    if (b >= 0x80)
                        return XmlConverter.ToString(buffer, offset1, length1) == s2;
                    if (s2[i] != (char)b)
                        return false;
                }
                return byteLength == charLength;
            }
            else
            {
                int length = Math.Min(byteLength, charLength);
                fixed (byte* _pb = &buffer[offset1])
                {
                    byte* pb = _pb;
                    byte* pbMax = pb + length;
                    fixed (char* _pch = s2)
                    {
                        char* pch = _pch;
                        // Try to do the fast comparison in ascii space
                        int t = 0;
                        while (pb < pbMax && *pb < 0x80)
                        {
                            t = *pb - (byte)(*pch);
                            // The code generated is better if we break out then return
                            if (t != 0)
                                break;
                            pb++;
                            pch++;
                        }
                        if (t != 0)
                            return false;
                        if (pb == pbMax)
                            return (byteLength == charLength);
                    }
                }
                return XmlConverter.ToString(buffer, offset1, length1) == s2;
            }
        }

        public int Compare(int offset1, int length1, int offset2, int length2)
        {
            byte[] buffer = this.buffer;
            int length = Math.Min(length1, length2);
            for (int i = 0; i < length; i++)
            {
                int s = buffer[offset1 + i] - buffer[offset2 + i];
                if (s != 0)
                    return s;
            }
            return length1 - length2;
        }

        public byte GetByte(int offset)
        {
            return buffer[offset];
        }

        public int GetInt8(int offset)
        {
            return (sbyte)GetByte(offset);
        }

        public int GetInt16(int offset)
        {
            byte[] buffer = this.buffer;
            return (Int16)(buffer[offset] + (buffer[offset + 1] << 8));
        }

        public int GetInt32(int offset)
        {
            byte[] buffer = this.buffer;
            byte b1 = buffer[offset + 0];
            byte b2 = buffer[offset + 1];
            byte b3 = buffer[offset + 2];
            byte b4 = buffer[offset + 3];
            return (((((b4 << 8) + b3) << 8) + b2) << 8) + b1;
        }

        public long GetInt64(int offset)
        {
            byte[] buffer = this.buffer;
            byte b1, b2, b3, b4;
            b1 = buffer[offset + 0];
            b2 = buffer[offset + 1];
            b3 = buffer[offset + 2];
            b4 = buffer[offset + 3];
            Int64 lo = (UInt32)(((((b4 << 8) + b3) << 8) + b2) << 8) + b1;
            b1 = buffer[offset + 4];
            b2 = buffer[offset + 5];
            b3 = buffer[offset + 6];
            b4 = buffer[offset + 7];
            Int64 hi = (UInt32)(((((b4 << 8) + b3) << 8) + b2) << 8) + b1;
            return (hi << 32) + lo;
        }

        public ulong GetUInt64(int offset)
        {
            return (ulong)GetInt64(offset);
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public float GetSingle(int offset)
        {
            byte[] buffer = this.buffer;
            float value;
            byte* pb = (byte*)&value;
            Fx.Assert(sizeof(float) == 4, "");
            pb[0] = buffer[offset + 0];
            pb[1] = buffer[offset + 1];
            pb[2] = buffer[offset + 2];
            pb[3] = buffer[offset + 3];
            return value;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public double GetDouble(int offset)
        {
            byte[] buffer = this.buffer;
            double value;
            byte* pb = (byte*)&value;
            Fx.Assert(sizeof(double) == 8, "");
            pb[0] = buffer[offset + 0];
            pb[1] = buffer[offset + 1];
            pb[2] = buffer[offset + 2];
            pb[3] = buffer[offset + 3];
            pb[4] = buffer[offset + 4];
            pb[5] = buffer[offset + 5];
            pb[6] = buffer[offset + 6];
            pb[7] = buffer[offset + 7];
            return value;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        public unsafe decimal GetDecimal(int offset)
        {
            const int SignMask = unchecked((int)0x80000000);
            const int ScaleMask = 0x00FF0000;

            byte[] buffer = this.buffer;
            byte b1 = buffer[offset + 0];
            byte b2 = buffer[offset + 1];
            byte b3 = buffer[offset + 2];
            byte b4 = buffer[offset + 3];
            int flags = (((((b4 << 8) + b3) << 8) + b2) << 8) + b1;

            //this logic mirrors the code in Decimal(int []) ctor.
            if ((flags & ~(SignMask | ScaleMask)) == 0 && (flags & ScaleMask) <= (28 << 16))
            {
                decimal value;
                byte* pb = (byte*)&value;
                for (int i = 0; i < sizeof(decimal); i++)
                    pb[i] = buffer[offset + i];
                return value;
            }
            else
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }

            //compiler doesn't know that XmlExceptionHelper.ThrowInvalidBinaryFormat always throws, 
            //so we have to have a return statement here even though we shouldn't hit this code path...
            Fx.Assert("A decimal value should have been returned or an exception should have been thrown.");
            return default(decimal);
        }

        public UniqueId GetUniqueId(int offset)
        {
            return new UniqueId(this.buffer, offset);
        }

        public Guid GetGuid(int offset)
        {
            if (guid == null)
                guid = new byte[16];
            System.Buffer.BlockCopy(buffer, offset, guid, 0, guid.Length);
            return new Guid(guid);
        }

        public void GetBase64(int srcOffset, byte[] buffer, int dstOffset, int count)
        {
            System.Buffer.BlockCopy(this.buffer, srcOffset, buffer, dstOffset, count);
        }

        public XmlBinaryNodeType GetNodeType()
        {
            return (XmlBinaryNodeType)GetByte();
        }

        public void SkipNodeType()
        {
            SkipByte();
        }

        public object[] GetList(int offset, int count)
        {
            int bufferOffset = this.Offset;
            this.Offset = offset;
            try
            {
                object[] objects = new object[count];
                for (int i = 0; i < count; i++)
                {
                    XmlBinaryNodeType nodeType = GetNodeType();
                    SkipNodeType();
                    Fx.Assert(nodeType != XmlBinaryNodeType.StartListText, "");
                    ReadValue(nodeType, listValue);
                    objects[i] = listValue.ToObject();
                }
                return objects;
            }
            finally
            {
                this.Offset = bufferOffset;
            }
        }

        public XmlDictionaryString GetDictionaryString(int key)
        {
            IXmlDictionary keyDictionary;
            if ((key & 1) != 0)
            {
                keyDictionary = session;
            }
            else
            {
                keyDictionary = dictionary;
            }
            XmlDictionaryString s;
            if (!keyDictionary.TryLookup(key >> 1, out s))
                XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
            return s;
        }

        public int ReadDictionaryKey()
        {
            int key = ReadMultiByteUInt31();
            if ((key & 1) != 0)
            {
                if (session == null)
                    XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
                int sessionKey = (key >> 1);
                XmlDictionaryString xmlString;
                if (!session.TryLookup(sessionKey, out xmlString))
                {
                    if (sessionKey < XmlDictionaryString.MinKey || sessionKey > XmlDictionaryString.MaxKey)
                        XmlExceptionHelper.ThrowXmlDictionaryStringIDOutOfRange(this.reader);
                    XmlExceptionHelper.ThrowXmlDictionaryStringIDUndefinedSession(this.reader, sessionKey);
                }
            }
            else
            {
                if (dictionary == null)
                    XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
                int staticKey = (key >> 1);
                XmlDictionaryString xmlString;
                if (!dictionary.TryLookup(staticKey, out xmlString))
                {
                    if (staticKey < XmlDictionaryString.MinKey || staticKey > XmlDictionaryString.MaxKey)
                        XmlExceptionHelper.ThrowXmlDictionaryStringIDOutOfRange(this.reader);
                    XmlExceptionHelper.ThrowXmlDictionaryStringIDUndefinedStatic(this.reader, staticKey);
                }
            }

            return key;
        }

        public void ReadValue(XmlBinaryNodeType nodeType, ValueHandle value)
        {
            switch (nodeType)
            {
                case XmlBinaryNodeType.EmptyText:
                    value.SetValue(ValueHandleType.Empty);
                    break;
                case XmlBinaryNodeType.ZeroText:
                    value.SetValue(ValueHandleType.Zero);
                    break;
                case XmlBinaryNodeType.OneText:
                    value.SetValue(ValueHandleType.One);
                    break;
                case XmlBinaryNodeType.TrueText:
                    value.SetValue(ValueHandleType.True);
                    break;
                case XmlBinaryNodeType.FalseText:
                    value.SetValue(ValueHandleType.False);
                    break;
                case XmlBinaryNodeType.BoolText:
                    value.SetValue(ReadUInt8() != 0 ? ValueHandleType.True : ValueHandleType.False);
                    break;
                case XmlBinaryNodeType.Chars8Text:
                    ReadValue(value, ValueHandleType.UTF8, ReadUInt8());
                    break;
                case XmlBinaryNodeType.Chars16Text:
                    ReadValue(value, ValueHandleType.UTF8, ReadUInt16());
                    break;
                case XmlBinaryNodeType.Chars32Text:
                    ReadValue(value, ValueHandleType.UTF8, ReadUInt31());
                    break;
                case XmlBinaryNodeType.UnicodeChars8Text:
                    ReadUnicodeValue(value, ReadUInt8());
                    break;
                case XmlBinaryNodeType.UnicodeChars16Text:
                    ReadUnicodeValue(value, ReadUInt16());
                    break;
                case XmlBinaryNodeType.UnicodeChars32Text:
                    ReadUnicodeValue(value, ReadUInt31());
                    break;
                case XmlBinaryNodeType.Bytes8Text:
                    ReadValue(value, ValueHandleType.Base64, ReadUInt8());
                    break;
                case XmlBinaryNodeType.Bytes16Text:
                    ReadValue(value, ValueHandleType.Base64, ReadUInt16());
                    break;
                case XmlBinaryNodeType.Bytes32Text:
                    ReadValue(value, ValueHandleType.Base64, ReadUInt31());
                    break;
                case XmlBinaryNodeType.DictionaryText:
                    value.SetDictionaryValue(ReadDictionaryKey());
                    break;
                case XmlBinaryNodeType.UniqueIdText:
                    ReadValue(value, ValueHandleType.UniqueId, ValueHandleLength.UniqueId);
                    break;
                case XmlBinaryNodeType.GuidText:
                    ReadValue(value, ValueHandleType.Guid, ValueHandleLength.Guid);
                    break;
                case XmlBinaryNodeType.DecimalText:
                    ReadValue(value, ValueHandleType.Decimal, ValueHandleLength.Decimal);
                    break;
                case XmlBinaryNodeType.Int8Text:
                    ReadValue(value, ValueHandleType.Int8, ValueHandleLength.Int8);
                    break;
                case XmlBinaryNodeType.Int16Text:
                    ReadValue(value, ValueHandleType.Int16, ValueHandleLength.Int16);
                    break;
                case XmlBinaryNodeType.Int32Text:
                    ReadValue(value, ValueHandleType.Int32, ValueHandleLength.Int32);
                    break;
                case XmlBinaryNodeType.Int64Text:
                    ReadValue(value, ValueHandleType.Int64, ValueHandleLength.Int64);
                    break;
                case XmlBinaryNodeType.UInt64Text:
                    ReadValue(value, ValueHandleType.UInt64, ValueHandleLength.UInt64);
                    break;
                case XmlBinaryNodeType.FloatText:
                    ReadValue(value, ValueHandleType.Single, ValueHandleLength.Single);
                    break;
                case XmlBinaryNodeType.DoubleText:
                    ReadValue(value, ValueHandleType.Double, ValueHandleLength.Double);
                    break;
                case XmlBinaryNodeType.TimeSpanText:
                    ReadValue(value, ValueHandleType.TimeSpan, ValueHandleLength.TimeSpan);
                    break;
                case XmlBinaryNodeType.DateTimeText:
                    ReadValue(value, ValueHandleType.DateTime, ValueHandleLength.DateTime);
                    break;
                case XmlBinaryNodeType.StartListText:
                    ReadList(value);
                    break;
                case XmlBinaryNodeType.QNameDictionaryText:
                    ReadQName(value);
                    break;
                default:
                    XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
                    break;
            }
        }

        void ReadValue(ValueHandle value, ValueHandleType type, int length)
        {
            int offset = ReadBytes(length);
            value.SetValue(type, offset, length);
        }

        void ReadUnicodeValue(ValueHandle value, int length)
        {
            if ((length & 1) != 0)
                XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
            ReadValue(value, ValueHandleType.Unicode, length);
        }

        void ReadList(ValueHandle value)
        {
            if (listValue == null)
            {
                listValue = new ValueHandle(this);
            }
            int count = 0;
            int offset = this.Offset;
            while (true)
            {
                XmlBinaryNodeType nodeType = GetNodeType();
                SkipNodeType();
                if (nodeType == XmlBinaryNodeType.StartListText)
                    XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
                if (nodeType == XmlBinaryNodeType.EndListText)
                    break;
                ReadValue(nodeType, listValue);
                count++;
            }
            value.SetValue(ValueHandleType.List, offset, count);
        }

        public void ReadQName(ValueHandle value)
        {
            int prefix = ReadUInt8();
            if (prefix >= 26)
                XmlExceptionHelper.ThrowInvalidBinaryFormat(reader);
            int key = ReadDictionaryKey();
            value.SetQNameValue(prefix, key);
        }

        public int[] GetRows()
        {
            if (buffer == null)
            {
                return new int[1] { 0 };
            }

            ArrayList list = new ArrayList();
            list.Add(offsetMin);
            for (int i = offsetMin; i < offsetMax; i++)
            {
                if (buffer[i] == (byte)13 || buffer[i] == (byte)10)
                {
                    if (i + 1 < offsetMax && buffer[i + 1] == (byte)10)
                        i++;
                    list.Add(i + 1);
                }
            }
            return (int[])list.ToArray(typeof(int));
        }
    }
}
