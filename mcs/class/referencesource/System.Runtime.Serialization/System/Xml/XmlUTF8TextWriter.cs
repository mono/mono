//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.IO;
    using System.Runtime;
    using System.Security;
    using System.Text;

    public interface IXmlTextWriterInitializer
    {
        void SetOutput(Stream stream, Encoding encoding, bool ownsStream);
    }

    class XmlUTF8TextWriter : XmlBaseWriter, IXmlTextWriterInitializer
    {
        XmlUTF8NodeWriter writer;

        //Supports FastAsync APIs
        internal override bool FastAsync
        {
            get
            {
                return true;
            }
        }

        public void SetOutput(Stream stream, Encoding encoding, bool ownsStream)
        {
            if (stream == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            if (encoding == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            if (encoding.WebName != Encoding.UTF8.WebName)
            {
                stream = new EncodingStreamWrapper(stream, encoding, true);
            }

            if (writer == null)
            {
                writer = new XmlUTF8NodeWriter();
            }
            writer.SetOutput(stream, ownsStream, encoding);
            SetOutput(writer);
        }

        public override bool CanFragment
        {
            get
            {
                // Fragmenting only works for utf8
                return writer.Encoding == null;
            }
        }

        protected override XmlSigningNodeWriter CreateSigningNodeWriter()
        {
            return new XmlSigningNodeWriter(true);
        }
    }

    class XmlUTF8NodeWriter : XmlStreamNodeWriter
    {
        byte[] entityChars;
        bool[] isEscapedAttributeChar;
        bool[] isEscapedElementChar;
        bool inAttribute;
        const int bufferLength = 512;
        const int maxEntityLength = 32;
        const int maxBytesPerChar = 3;
        Encoding encoding;
        char[] chars;
        InternalWriteBase64TextAsyncWriter internalWriteBase64TextAsyncWriter;

        static readonly byte[] startDecl = 
        {
            (byte)'<', (byte)'?', (byte)'x', (byte)'m', (byte)'l', (byte)' ',
            (byte)'v', (byte)'e', (byte)'r', (byte)'s', (byte)'i', (byte)'o', (byte)'n', (byte)'=', (byte)'"', (byte)'1', (byte)'.', (byte)'0', (byte)'"', (byte)' ',
            (byte)'e', (byte)'n', (byte)'c', (byte)'o', (byte)'d', (byte)'i', (byte)'n', (byte)'g', (byte)'=', (byte)'"',
        };
        static readonly byte[] endDecl =
        {
            (byte)'"', (byte)'?', (byte)'>'
        };
        static readonly byte[] utf8Decl =
        {
            (byte)'<', (byte)'?', (byte)'x', (byte)'m', (byte)'l', (byte)' ',
            (byte)'v', (byte)'e', (byte)'r', (byte)'s', (byte)'i', (byte)'o', (byte)'n', (byte)'=', (byte)'"', (byte)'1', (byte)'.', (byte)'0', (byte)'"', (byte)' ',
            (byte)'e', (byte)'n', (byte)'c', (byte)'o', (byte)'d', (byte)'i', (byte)'n', (byte)'g', (byte)'=', (byte)'"', (byte)'u', (byte)'t', (byte)'f', (byte)'-', (byte)'8', (byte)'"',
            (byte)'?', (byte)'>'
        };
        static readonly byte[] digits = 
        { 
            (byte) '0', (byte) '1', (byte) '2', (byte) '3', (byte) '4', (byte) '5', (byte) '6', (byte) '7',
            (byte) '8', (byte) '9', (byte) 'A', (byte) 'B', (byte) 'C', (byte) 'D', (byte) 'E', (byte) 'F'
        };
        static readonly bool[] defaultIsEscapedAttributeChar = new bool[]
        {
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, // '"', '&'
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false  // '<', '>'
        };
        static readonly bool[] defaultIsEscapedElementChar = new bool[]
        {
            true, true, true, true, true, true, true, true, true, false, false, true, true, true, true, true, // All but 0x09, 0x0A
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, // '&'
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false  // '<', '>'
        };

        public XmlUTF8NodeWriter()
            : this(defaultIsEscapedAttributeChar, defaultIsEscapedElementChar)
        {
        }

        public XmlUTF8NodeWriter(bool[] isEscapedAttributeChar, bool[] isEscapedElementChar)
        {
            this.isEscapedAttributeChar = isEscapedAttributeChar;
            this.isEscapedElementChar = isEscapedElementChar;
            this.inAttribute = false;
        }

        new public void SetOutput(Stream stream, bool ownsStream, Encoding encoding)
        {
            Encoding utf8Encoding = null;
            if (encoding != null && encoding.CodePage == Encoding.UTF8.CodePage)
            {
                utf8Encoding = encoding;
                encoding = null;
            }
            base.SetOutput(stream, ownsStream, utf8Encoding);
            this.encoding = encoding;
            this.inAttribute = false;
        }

        public Encoding Encoding
        {
            get
            {
                return encoding;
            }
        }

        byte[] GetCharEntityBuffer()
        {
            if (entityChars == null)
            {
                entityChars = new byte[maxEntityLength];
            }
            return entityChars;
        }

        char[] GetCharBuffer(int charCount)
        {
            if (charCount >= 256)
                return new char[charCount];
            if (chars == null || chars.Length < charCount)
                chars = new char[charCount];
            return chars;
        }

        public override void WriteDeclaration()
        {
            if (encoding == null)
            {
                WriteUTF8Chars(utf8Decl, 0, utf8Decl.Length);
            }
            else
            {
                WriteUTF8Chars(startDecl, 0, startDecl.Length);
                if (encoding.WebName == Encoding.BigEndianUnicode.WebName)
                    WriteUTF8Chars("utf-16BE");
                else
                    WriteUTF8Chars(encoding.WebName);
                WriteUTF8Chars(endDecl, 0, endDecl.Length);
            }
        }

        public override void WriteCData(string text)
        {
            byte[] buffer;
            int offset;

            buffer = GetBuffer(9, out offset);
            buffer[offset + 0] = (byte)'<';
            buffer[offset + 1] = (byte)'!';
            buffer[offset + 2] = (byte)'[';
            buffer[offset + 3] = (byte)'C';
            buffer[offset + 4] = (byte)'D';
            buffer[offset + 5] = (byte)'A';
            buffer[offset + 6] = (byte)'T';
            buffer[offset + 7] = (byte)'A';
            buffer[offset + 8] = (byte)'[';
            Advance(9);

            WriteUTF8Chars(text);

            buffer = GetBuffer(3, out offset);
            buffer[offset + 0] = (byte)']';
            buffer[offset + 1] = (byte)']';
            buffer[offset + 2] = (byte)'>';
            Advance(3);
        }

        void WriteStartComment()
        {
            int offset;
            byte[] buffer = GetBuffer(4, out offset);
            buffer[offset + 0] = (byte)'<';
            buffer[offset + 1] = (byte)'!';
            buffer[offset + 2] = (byte)'-';
            buffer[offset + 3] = (byte)'-';
            Advance(4);
        }

        void WriteEndComment()
        {
            int offset;
            byte[] buffer = GetBuffer(3, out offset);
            buffer[offset + 0] = (byte)'-';
            buffer[offset + 1] = (byte)'-';
            buffer[offset + 2] = (byte)'>';
            Advance(3);
        }

        public override void WriteComment(string text)
        {
            WriteStartComment();
            WriteUTF8Chars(text);
            WriteEndComment();
        }

        public override void WriteStartElement(string prefix, string localName)
        {
            WriteByte('<');
            if (prefix.Length != 0)
            {
                WritePrefix(prefix);
                WriteByte(':');
            }
            WriteLocalName(localName);
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName)
        {
            WriteStartElement(prefix, localName.Value);
        }

        public override void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteByte('<');
            if (prefixLength != 0)
            {
                WritePrefix(prefixBuffer, prefixOffset, prefixLength);
                WriteByte(':');
            }
            WriteLocalName(localNameBuffer, localNameOffset, localNameLength);
        }

        public override void WriteEndStartElement(bool isEmpty)
        {
            if (!isEmpty)
            {
                WriteByte('>');
            }
            else
            {
                WriteBytes('/', '>');
            }
        }

        public override void WriteEndElement(string prefix, string localName)
        {
            WriteBytes('<', '/');
            if (prefix.Length != 0)
            {
                WritePrefix(prefix);
                WriteByte(':');
            }
            WriteLocalName(localName);
            WriteByte('>');
        }

        public override void WriteEndElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteBytes('<', '/');
            if (prefixLength != 0)
            {
                WritePrefix(prefixBuffer, prefixOffset, prefixLength);
                WriteByte(':');
            }
            WriteLocalName(localNameBuffer, localNameOffset, localNameLength);
            WriteByte('>');
        }

        void WriteStartXmlnsAttribute()
        {
            int offset;
            byte[] buffer = GetBuffer(6, out offset);
            buffer[offset + 0] = (byte)' ';
            buffer[offset + 1] = (byte)'x';
            buffer[offset + 2] = (byte)'m';
            buffer[offset + 3] = (byte)'l';
            buffer[offset + 4] = (byte)'n';
            buffer[offset + 5] = (byte)'s';
            Advance(6);
            inAttribute = true;
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            WriteStartXmlnsAttribute();
            if (prefix.Length != 0)
            {
                WriteByte(':');
                WritePrefix(prefix);
            }
            WriteBytes('=', '"');
            WriteEscapedText(ns);
            WriteEndAttribute();
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            WriteXmlnsAttribute(prefix, ns.Value);
        }

        public override void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            WriteStartXmlnsAttribute();
            if (prefixLength != 0)
            {
                WriteByte(':');
                WritePrefix(prefixBuffer, prefixOffset, prefixLength);
            }
            WriteBytes('=', '"');
            WriteEscapedText(nsBuffer, nsOffset, nsLength);
            WriteEndAttribute();
        }

        public override void WriteStartAttribute(string prefix, string localName)
        {
            WriteByte(' ');
            if (prefix.Length != 0)
            {
                WritePrefix(prefix);
                WriteByte(':');
            }
            WriteLocalName(localName);
            WriteBytes('=', '"');
            inAttribute = true;
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName)
        {
            WriteStartAttribute(prefix, localName.Value);
        }

        public override void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteByte(' ');
            if (prefixLength != 0)
            {
                WritePrefix(prefixBuffer, prefixOffset, prefixLength);
                WriteByte(':');
            }
            WriteLocalName(localNameBuffer, localNameOffset, localNameLength);
            WriteBytes('=', '"');
            inAttribute = true;
        }

        public override void WriteEndAttribute()
        {
            WriteByte('"');
            inAttribute = false;
        }

        void WritePrefix(string prefix)
        {
            if (prefix.Length == 1)
            {
                WriteUTF8Char(prefix[0]);
            }
            else
            {
                WriteUTF8Chars(prefix);
            }
        }

        void WritePrefix(byte[] prefixBuffer, int prefixOffset, int prefixLength)
        {
            if (prefixLength == 1)
            {
                WriteUTF8Char((char)prefixBuffer[prefixOffset]);
            }
            else
            {
                WriteUTF8Chars(prefixBuffer, prefixOffset, prefixLength);
            }
        }

        void WriteLocalName(string localName)
        {
            WriteUTF8Chars(localName);
        }

        void WriteLocalName(byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteUTF8Chars(localNameBuffer, localNameOffset, localNameLength);
        }

        public override void WriteEscapedText(XmlDictionaryString s)
        {
            WriteEscapedText(s.Value);
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public override void WriteEscapedText(string s)
        {
            int count = s.Length;
            if (count > 0)
            {
                fixed (char* chars = s)
                {
                    UnsafeWriteEscapedText(chars, count);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public override void WriteEscapedText(char[] s, int offset, int count)
        {
            if (count > 0)
            {
                fixed (char* chars = &s[offset])
                {
                    UnsafeWriteEscapedText(chars, count);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe void UnsafeWriteEscapedText(char* chars, int count)
        {
            bool[] isEscapedChar = (inAttribute ? isEscapedAttributeChar : isEscapedElementChar);
            int isEscapedCharLength = isEscapedChar.Length;
            int i = 0;
            for (int j = 0; j < count; j++)
            {
                char ch = chars[j];
                if (ch < isEscapedCharLength && isEscapedChar[ch] || ch >= 0xFFFE)
                {
                    UnsafeWriteUTF8Chars(chars + i, j - i);
                    WriteCharEntity(ch);
                    i = j + 1;
                }
            }
            UnsafeWriteUTF8Chars(chars + i, count - i);
        }

        public override void WriteEscapedText(byte[] chars, int offset, int count)
        {
            bool[] isEscapedChar = (inAttribute ? isEscapedAttributeChar : isEscapedElementChar);
            int isEscapedCharLength = isEscapedChar.Length;
            int i = 0;
            for (int j = 0; j < count; j++)
            {
                byte ch = chars[offset + j];
                if (ch < isEscapedCharLength && isEscapedChar[ch])
                {
                    WriteUTF8Chars(chars, offset + i, j - i);
                    WriteCharEntity(ch);
                    i = j + 1;
                }
                else if (ch == 239 && offset + j + 2 < count)
                {
                    // 0xFFFE and 0xFFFF must be written as char entities
                    // UTF8(239, 191, 190) = (char) 0xFFFE
                    // UTF8(239, 191, 191) = (char) 0xFFFF
                    byte ch2 = chars[offset + j + 1];
                    byte ch3 = chars[offset + j + 2];
                    if (ch2 == 191 && (ch3 == 190 || ch3 == 191))
                    {
                        WriteUTF8Chars(chars, offset + i, j - i);
                        WriteCharEntity(ch3 == 190 ? (char)0xFFFE : (char)0xFFFF);
                        i = j + 3;
                    }
                }
            }
            WriteUTF8Chars(chars, offset + i, count - i);
        }

        public void WriteText(int ch)
        {
            WriteUTF8Char(ch);
        }

        public override void WriteText(byte[] chars, int offset, int count)
        {
            WriteUTF8Chars(chars, offset, count);
        }

        [SecuritySafeCritical]
        unsafe public override void WriteText(char[] chars, int offset, int count)
        {
            if (count > 0)
            {
                fixed (char* pch = &chars[offset])
                {
                    UnsafeWriteUTF8Chars(pch, count);
                }
            }
        }

        public override void WriteText(string value)
        {
            WriteUTF8Chars(value);
        }

        public override void WriteText(XmlDictionaryString value)
        {
            WriteUTF8Chars(value.Value);
        }

        public void WriteLessThanCharEntity()
        {
            int offset;
            byte[] buffer = GetBuffer(4, out offset);
            buffer[offset + 0] = (byte)'&';
            buffer[offset + 1] = (byte)'l';
            buffer[offset + 2] = (byte)'t';
            buffer[offset + 3] = (byte)';';
            Advance(4);
        }

        public void WriteGreaterThanCharEntity()
        {
            int offset;
            byte[] buffer = GetBuffer(4, out offset);
            buffer[offset + 0] = (byte)'&';
            buffer[offset + 1] = (byte)'g';
            buffer[offset + 2] = (byte)'t';
            buffer[offset + 3] = (byte)';';
            Advance(4);
        }

        public void WriteAmpersandCharEntity()
        {
            int offset;
            byte[] buffer = GetBuffer(5, out offset);
            buffer[offset + 0] = (byte)'&';
            buffer[offset + 1] = (byte)'a';
            buffer[offset + 2] = (byte)'m';
            buffer[offset + 3] = (byte)'p';
            buffer[offset + 4] = (byte)';';
            Advance(5);
        }

        public void WriteApostropheCharEntity()
        {
            int offset;
            byte[] buffer = GetBuffer(6, out offset);
            buffer[offset + 0] = (byte)'&';
            buffer[offset + 1] = (byte)'a';
            buffer[offset + 2] = (byte)'p';
            buffer[offset + 3] = (byte)'o';
            buffer[offset + 4] = (byte)'s';
            buffer[offset + 5] = (byte)';';
            Advance(6);
        }

        public void WriteQuoteCharEntity()
        {
            int offset;
            byte[] buffer = GetBuffer(6, out offset);
            buffer[offset + 0] = (byte)'&';
            buffer[offset + 1] = (byte)'q';
            buffer[offset + 2] = (byte)'u';
            buffer[offset + 3] = (byte)'o';
            buffer[offset + 4] = (byte)'t';
            buffer[offset + 5] = (byte)';';
            Advance(6);
        }

        void WriteHexCharEntity(int ch)
        {
            byte[] chars = GetCharEntityBuffer();
            int offset = maxEntityLength;
            chars[--offset] = (byte)';';
            offset -= ToBase16(chars, offset, (uint)ch);
            chars[--offset] = (byte)'x';
            chars[--offset] = (byte)'#';
            chars[--offset] = (byte)'&';
            WriteUTF8Chars(chars, offset, maxEntityLength - offset);
        }

        public override void WriteCharEntity(int ch)
        {
            switch (ch)
            {
                case '<':
                    WriteLessThanCharEntity();
                    break;
                case '>':
                    WriteGreaterThanCharEntity();
                    break;
                case '&':
                    WriteAmpersandCharEntity();
                    break;
                case '\'':
                    WriteApostropheCharEntity();
                    break;
                case '"':
                    WriteQuoteCharEntity();
                    break;
                default:
                    WriteHexCharEntity(ch);
                    break;
            }
        }

        int ToBase16(byte[] chars, int offset, uint value)
        {
            int count = 0;
            do
            {
                count++;
                chars[--offset] = digits[(int)(value & 0x0F)];
                value /= 16;
            }
            while (value != 0);
            return count;
        }

        public override void WriteBoolText(bool value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxBoolChars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteDecimalText(decimal value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxDecimalChars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteDoubleText(double value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxDoubleChars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteFloatText(float value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxFloatChars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteDateTimeText(DateTime value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxDateTimeChars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteUniqueIdText(UniqueId value)
        {
            if (value.IsGuid)
            {
                int charCount = value.CharArrayLength;
                char[] chars = GetCharBuffer(charCount);
                value.ToCharArray(chars, 0);
                WriteText(chars, 0, charCount);
            }
            else
            {
                WriteEscapedText(value.ToString());
            }
        }

        public override void WriteInt32Text(int value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxInt32Chars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteInt64Text(long value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxInt64Chars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteUInt64Text(ulong value)
        {
            int offset;
            byte[] buffer = GetBuffer(XmlConverter.MaxUInt64Chars, out offset);
            Advance(XmlConverter.ToChars(value, buffer, offset));
        }

        public override void WriteGuidText(Guid value)
        {
            WriteText(value.ToString());
        }

        public override void WriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count)
        {
            if (trailByteCount > 0)
            {
                InternalWriteBase64Text(trailBytes, 0, trailByteCount);
            }
            InternalWriteBase64Text(buffer, offset, count);
        }

        void InternalWriteBase64Text(byte[] buffer, int offset, int count)
        {
            Base64Encoding encoding = XmlConverter.Base64Encoding;
            while (count >= 3)
            {
                int byteCount = Math.Min(bufferLength / 4 * 3, count - count % 3);
                int charCount = byteCount / 3 * 4;
                int charOffset;
                byte[] chars = GetBuffer(charCount, out charOffset);
                Advance(encoding.GetChars(buffer, offset, byteCount, chars, charOffset));
                offset += byteCount;
                count -= byteCount;
            }
            if (count > 0)
            {
                int charOffset;
                byte[] chars = GetBuffer(4, out charOffset);
                Advance(encoding.GetChars(buffer, offset, count, chars, charOffset));
            }
        }

        internal override AsyncCompletionResult WriteBase64TextAsync(AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> xmlNodeWriterState)
        {
            if (internalWriteBase64TextAsyncWriter == null)
            {
                internalWriteBase64TextAsyncWriter = new InternalWriteBase64TextAsyncWriter(this);
            }

            return this.internalWriteBase64TextAsyncWriter.StartAsync(xmlNodeWriterState);
        }

        class InternalWriteBase64TextAsyncWriter
        {
            AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> nodeState;
            AsyncEventArgs<XmlWriteBase64AsyncArguments> writerState;
            XmlWriteBase64AsyncArguments writerArgs;
            XmlUTF8NodeWriter writer;
            GetBufferAsyncEventArgs getBufferState;
            GetBufferArgs getBufferArgs;
            static AsyncEventArgsCallback onTrailByteComplete = new AsyncEventArgsCallback(OnTrailBytesComplete);
            static AsyncEventArgsCallback onWriteComplete = new AsyncEventArgsCallback(OnWriteComplete);
            static AsyncEventArgsCallback onGetBufferComplete = new AsyncEventArgsCallback(OnGetBufferComplete);

            public InternalWriteBase64TextAsyncWriter(XmlUTF8NodeWriter writer)
            {
                this.writer = writer;
                this.writerState = new AsyncEventArgs<XmlWriteBase64AsyncArguments>();
                this.writerArgs = new XmlWriteBase64AsyncArguments();
            }

            internal AsyncCompletionResult StartAsync(AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> xmlNodeWriterState)
            {
                Fx.Assert(xmlNodeWriterState != null, "xmlNodeWriterState cannot be null.");
                Fx.Assert(this.nodeState == null, "nodeState is not null.");

                this.nodeState = xmlNodeWriterState;
                XmlNodeWriterWriteBase64TextArgs nodeWriterArgs = xmlNodeWriterState.Arguments;

                if (nodeWriterArgs.TrailCount > 0)
                {
                    this.writerArgs.Buffer = nodeWriterArgs.TrailBuffer;
                    this.writerArgs.Offset = 0;
                    this.writerArgs.Count = nodeWriterArgs.TrailCount;

                    this.writerState.Set(onTrailByteComplete, this.writerArgs, this);
                    if (this.InternalWriteBase64TextAsync(this.writerState) != AsyncCompletionResult.Completed)
                    {
                        return AsyncCompletionResult.Queued;
                    }
                    this.writerState.Complete(true);
                }

                if (this.WriteBufferAsync() == AsyncCompletionResult.Completed)
                {
                    this.nodeState = null;
                    return AsyncCompletionResult.Completed;
                }

                return AsyncCompletionResult.Queued;
            }

            static private void OnTrailBytesComplete(IAsyncEventArgs eventArgs)
            {
                InternalWriteBase64TextAsyncWriter thisPtr = (InternalWriteBase64TextAsyncWriter)eventArgs.AsyncState;
                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    if (eventArgs.Exception != null)
                    {
                        completionException = eventArgs.Exception;
                        completeSelf = true;
                    }
                    else if (thisPtr.WriteBufferAsync() == AsyncCompletionResult.Completed)
                    {
                        completeSelf = true;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    completionException = exception;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> state = thisPtr.nodeState;
                    thisPtr.nodeState = null;
                    state.Complete(false, eventArgs.Exception);
                }
            }

            AsyncCompletionResult WriteBufferAsync()
            {
                this.writerArgs.Buffer = nodeState.Arguments.Buffer;
                this.writerArgs.Offset = nodeState.Arguments.Offset;
                this.writerArgs.Count = nodeState.Arguments.Count;

                this.writerState.Set(onWriteComplete, this.writerArgs, this);
                if (this.InternalWriteBase64TextAsync(this.writerState) == AsyncCompletionResult.Completed)
                {
                    this.writerState.Complete(true);
                    return AsyncCompletionResult.Completed;
                }

                return AsyncCompletionResult.Queued;
            }

            static void OnWriteComplete(IAsyncEventArgs eventArgs)
            {
                InternalWriteBase64TextAsyncWriter thisPtr = (InternalWriteBase64TextAsyncWriter)eventArgs.AsyncState;
                AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> state = thisPtr.nodeState;
                thisPtr.nodeState = null;
                state.Complete(false, eventArgs.Exception);
            }

            AsyncCompletionResult InternalWriteBase64TextAsync(AsyncEventArgs<XmlWriteBase64AsyncArguments> writerState)
            {
                GetBufferAsyncEventArgs bufferState = this.getBufferState;
                GetBufferArgs bufferArgs = this.getBufferArgs;
                XmlWriteBase64AsyncArguments writerArgs = writerState.Arguments;

                if (bufferState == null)
                {
                    // Need to initialize the cached getBufferState 
                    // used to call GetBufferAsync() multiple times.
                    bufferState = new GetBufferAsyncEventArgs();
                    bufferArgs = new GetBufferArgs();
                    this.getBufferState = bufferState;
                    this.getBufferArgs = bufferArgs;
                }

                Base64Encoding encoding = XmlConverter.Base64Encoding;

                while (writerArgs.Count >= 3)
                {
                    int byteCount = Math.Min(bufferLength / 4 * 3, writerArgs.Count - writerArgs.Count % 3);
                    int charCount = byteCount / 3 * 4;

                    bufferArgs.Count = charCount;
                    bufferState.Set(onGetBufferComplete, bufferArgs, this);
                    if (writer.GetBufferAsync(bufferState) == AsyncCompletionResult.Completed)
                    {
                        GetBufferEventResult getbufferResult = bufferState.Result;
                        bufferState.Complete(true);
                        writer.Advance(encoding.GetChars(
                            writerArgs.Buffer,
                            writerArgs.Offset,
                            byteCount,
                            getbufferResult.Buffer,
                            getbufferResult.Offset));
                        writerArgs.Offset += byteCount;
                        writerArgs.Count -= byteCount;
                    }
                    else
                    {
                        return AsyncCompletionResult.Queued;
                    }
                }

                if (writerArgs.Count > 0)
                {
                    bufferArgs.Count = 4;
                    bufferState.Set(onGetBufferComplete, bufferArgs, this);
                    if (writer.GetBufferAsync(bufferState) == AsyncCompletionResult.Completed)
                    {
                        GetBufferEventResult getbufferResult = bufferState.Result;
                        bufferState.Complete(true);
                        writer.Advance(encoding.GetChars(
                            writerArgs.Buffer,
                            writerArgs.Offset,
                            writerArgs.Count,
                            getbufferResult.Buffer,
                            getbufferResult.Offset));
                    }
                    else
                    {
                        return AsyncCompletionResult.Queued;
                    }
                }

                return AsyncCompletionResult.Completed;
            }

            static void OnGetBufferComplete(IAsyncEventArgs state)
            {
                GetBufferEventResult result = ((GetBufferAsyncEventArgs)state).Result;
                InternalWriteBase64TextAsyncWriter thisPtr = (InternalWriteBase64TextAsyncWriter)state.AsyncState;
                XmlWriteBase64AsyncArguments writerArgs = thisPtr.writerState.Arguments;

                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    if (state.Exception != null)
                    {
                        completionException = state.Exception;
                        completeSelf = true;
                    }
                    else
                    {
                        byte[] chars = result.Buffer;
                        int offset = result.Offset;

                        Base64Encoding encoding = XmlConverter.Base64Encoding;
                        int byteCount = Math.Min(bufferLength / 4 * 3, writerArgs.Count - writerArgs.Count % 3);
                        int charCount = byteCount / 3 * 4;

                        thisPtr.writer.Advance(encoding.GetChars(
                                   writerArgs.Buffer,
                                   writerArgs.Offset,
                                   byteCount,
                                   chars,
                                   offset));

                        if (byteCount >= 3)
                        {
                            writerArgs.Offset += byteCount;
                            writerArgs.Count -= byteCount;
                        }

                        if (thisPtr.InternalWriteBase64TextAsync(thisPtr.writerState) == AsyncCompletionResult.Completed)
                        {
                            completeSelf = true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    completionException = exception;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.writerState.Complete(false, completionException);
                }
            }
        }

        public override IAsyncResult BeginWriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new WriteBase64TextAsyncResult(trailBytes, trailByteCount, buffer, offset, count, this, callback, state);
        }

        public override void EndWriteBase64Text(IAsyncResult result)
        {
            WriteBase64TextAsyncResult.End(result);
        }

        IAsyncResult BeginInternalWriteBase64Text(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new InternalWriteBase64TextAsyncResult(buffer, offset, count, this, callback, state);
        }

        void EndInternalWriteBase64Text(IAsyncResult result)
        {
            InternalWriteBase64TextAsyncResult.End(result);
        }

        class WriteBase64TextAsyncResult : AsyncResult
        {
            static AsyncCompletion onTrailBytesComplete = new AsyncCompletion(OnTrailBytesComplete);
            static AsyncCompletion onComplete = new AsyncCompletion(OnComplete);

            byte[] trailBytes;
            int trailByteCount;
            byte[] buffer;
            int offset;
            int count;

            XmlUTF8NodeWriter writer;

            public WriteBase64TextAsyncResult(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count, XmlUTF8NodeWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.writer = writer;
                this.trailBytes = trailBytes;
                this.trailByteCount = trailByteCount;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;

                bool completeSelf = HandleWriteTrailBytes(null);

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static bool OnTrailBytesComplete(IAsyncResult result)
            {
                WriteBase64TextAsyncResult thisPtr = (WriteBase64TextAsyncResult)result.AsyncState;
                return thisPtr.HandleWriteTrailBytes(result);
            }

            static bool OnComplete(IAsyncResult result)
            {
                WriteBase64TextAsyncResult thisPtr = (WriteBase64TextAsyncResult)result.AsyncState;
                return thisPtr.HandleWriteBase64Text(result);
            }

            bool HandleWriteTrailBytes(IAsyncResult result)
            {
                if (this.trailByteCount > 0)
                {
                    if (result == null)
                    {
                        result = writer.BeginInternalWriteBase64Text(this.trailBytes, 0, this.trailByteCount, PrepareAsyncCompletion(onTrailBytesComplete), this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                    }
                    writer.EndInternalWriteBase64Text(result);
                }

                return HandleWriteBase64Text(null);
            }

            bool HandleWriteBase64Text(IAsyncResult result)
            {
                if (result == null)
                {
                    result = writer.BeginInternalWriteBase64Text(this.buffer, this.offset, this.count, PrepareAsyncCompletion(onComplete), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                writer.EndInternalWriteBase64Text(result);
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteBase64TextAsyncResult>(result);
            }
        }

        class InternalWriteBase64TextAsyncResult : AsyncResult
        {
            byte[] buffer;
            int offset;
            int count;
            Base64Encoding encoding;

            XmlUTF8NodeWriter writer;
            static AsyncCallback onWriteCharacters = Fx.ThunkCallback(OnWriteCharacters);
            static AsyncCompletion onWriteTrailingCharacters = new AsyncCompletion(OnWriteTrailingCharacters);

            public InternalWriteBase64TextAsyncResult(byte[] buffer, int offset, int count, XmlUTF8NodeWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
                this.writer = writer;
                this.encoding = XmlConverter.Base64Encoding;

                bool completeSelf = ContinueWork();

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }


            static bool OnWriteTrailingCharacters(IAsyncResult result)
            {
                InternalWriteBase64TextAsyncResult thisPtr = (InternalWriteBase64TextAsyncResult)result.AsyncState;
                return thisPtr.HandleWriteTrailingCharacters(result);
            }

            bool ContinueWork()
            {
                while (this.count >= 3)
                {
                    if (HandleWriteCharacters(null))
                    {
                        continue;
                    }
                    else
                    {
                        // needs to jump async
                        return false;
                    }
                }

                if (count > 0)
                {
                    return HandleWriteTrailingCharacters(null);
                }
                return true;
            }

            bool HandleWriteCharacters(IAsyncResult result)
            {
                int byteCount = Math.Min(bufferLength / 4 * 3, count - count % 3);
                int charCount = byteCount / 3 * 4;
                int charOffset;

                if (result == null)
                {
                    result = writer.BeginGetBuffer(charCount, onWriteCharacters, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                byte[] chars = writer.EndGetBuffer(result, out charOffset);

                writer.Advance(encoding.GetChars(this.buffer, this.offset, byteCount, chars, charOffset));
                this.offset += byteCount;
                this.count -= byteCount;

                return true;
            }

            bool HandleWriteTrailingCharacters(IAsyncResult result)
            {
                if (result == null)
                {
                    result = writer.BeginGetBuffer(4, PrepareAsyncCompletion(onWriteTrailingCharacters), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                int charOffset;
                byte[] chars = writer.EndGetBuffer(result, out charOffset);
                writer.Advance(encoding.GetChars(this.buffer, this.offset, this.count, chars, charOffset));
                return true;
            }

            static void OnWriteCharacters(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                InternalWriteBase64TextAsyncResult thisPtr = (InternalWriteBase64TextAsyncResult)result.AsyncState;
                Exception completionException = null;
                bool completeSelf = false;

                try
                {
                    thisPtr.HandleWriteCharacters(result);
                    completeSelf = thisPtr.ContinueWork();
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    completeSelf = true;
                    completionException = ex;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<InternalWriteBase64TextAsyncResult>(result);
            }
        }

        public override void WriteTimeSpanText(TimeSpan value)
        {
            WriteText(XmlConvert.ToString(value));
        }

        public override void WriteStartListText()
        {
        }

        public override void WriteListSeparator()
        {
            WriteByte(' ');
        }

        public override void WriteEndListText()
        {
        }

        public override void WriteQualifiedName(string prefix, XmlDictionaryString localName)
        {
            if (prefix.Length != 0)
            {
                WritePrefix(prefix);
                WriteByte(':');
            }
            WriteText(localName);
        }
    }
}
