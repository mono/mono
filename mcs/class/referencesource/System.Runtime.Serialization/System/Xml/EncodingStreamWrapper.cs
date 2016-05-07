//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Xml
{
    using System;
    using System.IO;
    using System.Text;
    using System.Runtime.Serialization;

    // This wrapper does not support seek.
    // Constructors consume/emit byte order mark.
    // Supports: UTF-8, Unicode, BigEndianUnicode
    // ASSUMPTION ([....]): This class will only be used for EITHER reading OR writing.  It can be done, it would just mean more buffers.
    // ASSUMPTION ([....]): The byte buffer is large enough to hold the declaration
    // ASSUMPTION ([....]): The buffer manipulation methods (FillBuffer/Compare/etc.) will only be used to parse the declaration
    //                      during construction.
    class EncodingStreamWrapper : Stream
    {
        enum SupportedEncoding { UTF8, UTF16LE, UTF16BE, None }
        static readonly UTF8Encoding SafeUTF8 = new UTF8Encoding(false, false);
        static readonly UnicodeEncoding SafeUTF16 = new UnicodeEncoding(false, false, false);
        static readonly UnicodeEncoding SafeBEUTF16 = new UnicodeEncoding(true, false, false);
        static readonly UTF8Encoding ValidatingUTF8 = new UTF8Encoding(false, true);
        static readonly UnicodeEncoding ValidatingUTF16 = new UnicodeEncoding(false, false, true);
        static readonly UnicodeEncoding ValidatingBEUTF16 = new UnicodeEncoding(true, false, true);
        const int BufferLength = 128;

        // UTF-8 is fastpath, so that's how these are stored
        // Compare methods adapt to unicodes.
        static readonly byte[] encodingAttr = new byte[] { (byte)'e', (byte)'n', (byte)'c', (byte)'o', (byte)'d', (byte)'i', (byte)'n', (byte)'g' };
        static readonly byte[] encodingUTF8 = new byte[] { (byte)'u', (byte)'t', (byte)'f', (byte)'-', (byte)'8' };
        static readonly byte[] encodingUnicode = new byte[] { (byte)'u', (byte)'t', (byte)'f', (byte)'-', (byte)'1', (byte)'6' };
        static readonly byte[] encodingUnicodeLE = new byte[] { (byte)'u', (byte)'t', (byte)'f', (byte)'-', (byte)'1', (byte)'6', (byte)'l', (byte)'e' };
        static readonly byte[] encodingUnicodeBE = new byte[] { (byte)'u', (byte)'t', (byte)'f', (byte)'-', (byte)'1', (byte)'6', (byte)'b', (byte)'e' };

        SupportedEncoding encodingCode;
        Encoding encoding;
        Encoder enc;
        Decoder dec;
        bool isReading;

        Stream stream;
        char[] chars;
        byte[] bytes;
        int byteOffset;
        int byteCount;

        byte[] byteBuffer = new byte[1];

        // Reading constructor
        public EncodingStreamWrapper(Stream stream, Encoding encoding)
        {
            try
            {
                this.isReading = true;
                this.stream = new BufferedStream(stream);

                // Decode the expected encoding
                SupportedEncoding expectedEnc = GetSupportedEncoding(encoding);

                // Get the byte order mark so we can determine the encoding
                // May want to try to delay allocating everything until we know the BOM
                SupportedEncoding declEnc = ReadBOMEncoding(encoding == null);

                // Check that the expected encoding matches the decl encoding.
                if (expectedEnc != SupportedEncoding.None && expectedEnc != declEnc)
                    ThrowExpectedEncodingMismatch(expectedEnc, declEnc);

                // Fastpath: UTF-8 BOM
                if (declEnc == SupportedEncoding.UTF8)
                {
                    // Fastpath: UTF-8 BOM, No declaration
                    FillBuffer(2);
                    if (bytes[byteOffset + 1] != '?' || bytes[byteOffset] != '<')
                    {
                        return;
                    }

                    FillBuffer(BufferLength);
                    CheckUTF8DeclarationEncoding(bytes, byteOffset, byteCount, declEnc, expectedEnc);
                }
                else
                {
                    // Convert to UTF-8
                    EnsureBuffers();
                    FillBuffer((BufferLength - 1) * 2);
                    SetReadDocumentEncoding(declEnc);
                    CleanupCharBreak();
                    int count = this.encoding.GetChars(bytes, byteOffset, byteCount, chars, 0);
                    byteOffset = 0;
                    byteCount = ValidatingUTF8.GetBytes(chars, 0, count, bytes, 0);

                    // Check for declaration
                    if (bytes[1] == '?' && bytes[0] == '<')
                    {
                        CheckUTF8DeclarationEncoding(bytes, 0, byteCount, declEnc, expectedEnc);
                    }
                    else
                    {
                        // Declaration required if no out-of-band encoding
                        if (expectedEnc == SupportedEncoding.None)
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlDeclarationRequired)));
                    }
                }
            }
            catch (DecoderFallbackException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidBytes), ex));
            }
        }

        void SetReadDocumentEncoding(SupportedEncoding e)
        {
            EnsureBuffers();
            this.encodingCode = e;
            this.encoding = GetEncoding(e);
        }

        static Encoding GetEncoding(SupportedEncoding e)
        {
            switch (e)
            {
                case SupportedEncoding.UTF8:
                    return ValidatingUTF8;

                case SupportedEncoding.UTF16LE:
                    return ValidatingUTF16;

                case SupportedEncoding.UTF16BE:
                    return ValidatingBEUTF16;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlEncodingNotSupported)));
            }
        }

        static Encoding GetSafeEncoding(SupportedEncoding e)
        {
            switch (e)
            {
                case SupportedEncoding.UTF8:
                    return SafeUTF8;

                case SupportedEncoding.UTF16LE:
                    return SafeUTF16;

                case SupportedEncoding.UTF16BE:
                    return SafeBEUTF16;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlEncodingNotSupported)));
            }
        }

        static string GetEncodingName(SupportedEncoding enc)
        {
            switch (enc)
            {
                case SupportedEncoding.UTF8:
                    return "utf-8";

                case SupportedEncoding.UTF16LE:
                    return "utf-16LE";

                case SupportedEncoding.UTF16BE:
                    return "utf-16BE";

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlEncodingNotSupported)));
            }
        }

        static SupportedEncoding GetSupportedEncoding(Encoding encoding)
        {
            if (encoding == null)
                return SupportedEncoding.None;
            else if (encoding.WebName == ValidatingUTF8.WebName)
                return SupportedEncoding.UTF8;
            else if (encoding.WebName == ValidatingUTF16.WebName)
                return SupportedEncoding.UTF16LE;
            else if (encoding.WebName == ValidatingBEUTF16.WebName)
                return SupportedEncoding.UTF16BE;
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlEncodingNotSupported)));
        }

        // Writing constructor
        public EncodingStreamWrapper(Stream stream, Encoding encoding, bool emitBOM)
        {
            this.isReading = false;
            this.encoding = encoding;
            this.stream = new BufferedStream(stream);

            // Set the encoding code
            this.encodingCode = GetSupportedEncoding(encoding);

            if (encodingCode != SupportedEncoding.UTF8)
            {
                EnsureBuffers();
                dec = ValidatingUTF8.GetDecoder();
                enc = this.encoding.GetEncoder();

                // Emit BOM
                if (emitBOM)
                {
                    byte[] bom = this.encoding.GetPreamble();
                    if (bom.Length > 0)
                        this.stream.Write(bom, 0, bom.Length);
                }
            }
        }

        SupportedEncoding ReadBOMEncoding(bool notOutOfBand)
        {
            int b1 = this.stream.ReadByte();
            int b2 = this.stream.ReadByte();
            int b3 = this.stream.ReadByte();
            int b4 = this.stream.ReadByte();

            // Premature end of stream
            if (b4 == -1)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedEndOfFile)));

            int preserve;
            SupportedEncoding e = ReadBOMEncoding((byte)b1, (byte)b2, (byte)b3, (byte)b4, notOutOfBand, out preserve);

            EnsureByteBuffer();
            switch (preserve)
            {
                case 1:
                    bytes[0] = (byte)b4;
                    break;

                case 2:
                    bytes[0] = (byte)b3;
                    bytes[1] = (byte)b4;
                    break;

                case 4:
                    bytes[0] = (byte)b1;
                    bytes[1] = (byte)b2;
                    bytes[2] = (byte)b3;
                    bytes[3] = (byte)b4;
                    break;
            }
            byteCount = preserve;

            return e;
        }

        static SupportedEncoding ReadBOMEncoding(byte b1, byte b2, byte b3, byte b4, bool notOutOfBand, out int preserve)
        {
            SupportedEncoding e = SupportedEncoding.UTF8; // Default

            preserve = 0;
            if (b1 == '<' && b2 != 0x00) // UTF-8, no BOM
            {
                e = SupportedEncoding.UTF8;
                preserve = 4;
            }
            else if (b1 == 0xFF && b2 == 0xFE) // UTF-16 little endian
            {
                e = SupportedEncoding.UTF16LE;
                preserve = 2;
            }
            else if (b1 == 0xFE && b2 == 0xFF) // UTF-16 big endian
            {
                e = SupportedEncoding.UTF16BE;
                preserve = 2;
            }
            else if (b1 == 0x00 && b2 == '<') // UTF-16 big endian, no BOM
            {
                e = SupportedEncoding.UTF16BE;

                if (notOutOfBand && (b3 != 0x00 || b4 != '?'))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlDeclMissing)));
                preserve = 4;
            }
            else if (b1 == '<' && b2 == 0x00) // UTF-16 little endian, no BOM
            {
                e = SupportedEncoding.UTF16LE;

                if (notOutOfBand && (b3 != '?' || b4 != 0x00))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlDeclMissing)));
                preserve = 4;
            }
            else if (b1 == 0xEF && b2 == 0xBB) // UTF8 with BOM
            {
                // Encoding error
                if (notOutOfBand && b3 != 0xBF)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlBadBOM)));
                preserve = 1;
            }
            else  // Assume UTF8
            {
                preserve = 4;
            }

            return e;
        }

        void FillBuffer(int count)
        {
            count -= byteCount;
            while (count > 0)
            {
                int read = stream.Read(bytes, byteOffset + byteCount, count);
                if (read == 0)
                    break;

                byteCount += read;
                count -= read;
            }
        }

        void EnsureBuffers()
        {
            EnsureByteBuffer();
            if (chars == null)
                chars = new char[BufferLength];
        }

        void EnsureByteBuffer()
        {
            if (bytes != null)
                return;

            bytes = new byte[BufferLength * 4];
            byteOffset = 0;
            byteCount = 0;
        }

        static void CheckUTF8DeclarationEncoding(byte[] buffer, int offset, int count, SupportedEncoding e, SupportedEncoding expectedEnc)
        {
            byte quot = 0;
            int encEq = -1;
            int max = offset + Math.Min(count, BufferLength);

            // Encoding should be second "=", abort at first "?"
            int i = 0;
            int eq = 0;
            for (i = offset + 2; i < max; i++)  // Skip the "<?" so we don't get caught by the first "?"
            {
                if (quot != 0)
                {
                    if (buffer[i] == quot)
                    {
                        quot = 0;
                    }
                    continue;
                }

                if (buffer[i] == (byte)'\'' || buffer[i] == (byte)'"')
                {
                    quot = buffer[i];
                }
                else if (buffer[i] == (byte)'=')
                {
                    if (eq == 1)
                    {
                        encEq = i;
                        break;
                    }
                    eq++;
                }
                else if (buffer[i] == (byte)'?')  // Not legal character in a decl before second "="
                {
                    break;
                }
            }

            // No encoding found
            if (encEq == -1)
            {
                if (e != SupportedEncoding.UTF8 && expectedEnc == SupportedEncoding.None)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlDeclarationRequired)));
                return;
            }

            if (encEq < 28) // Earliest second "=" can appear
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlMalformedDecl)));

            // Back off whitespace
            for (i = encEq - 1; IsWhitespace(buffer[i]); i--);

            // Check for encoding attribute
            if (!Compare(encodingAttr, buffer, i - encodingAttr.Length + 1))
            {
                if (e != SupportedEncoding.UTF8 && expectedEnc == SupportedEncoding.None)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlDeclarationRequired)));
                return;
            }

            // Move ahead of whitespace
            for (i = encEq + 1; i < max && IsWhitespace(buffer[i]); i++);

            // Find the quotes
            if (buffer[i] != '\'' && buffer[i] != '"')
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlMalformedDecl)));
            quot = buffer[i];

            int q = i;
            for (i = q + 1; buffer[i] != quot && i < max; ++i);

            if (buffer[i] != quot)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlMalformedDecl)));

            int encStart = q + 1;
            int encCount = i - encStart;

            // lookup the encoding
            SupportedEncoding declEnc = e;
            if (encCount == encodingUTF8.Length && CompareCaseInsensitive(encodingUTF8, buffer, encStart))
            {
                declEnc = SupportedEncoding.UTF8;
            }
            else if (encCount == encodingUnicodeLE.Length && CompareCaseInsensitive(encodingUnicodeLE, buffer, encStart))
            {
                declEnc = SupportedEncoding.UTF16LE;
            }
            else if (encCount == encodingUnicodeBE.Length && CompareCaseInsensitive(encodingUnicodeBE, buffer, encStart))
            {
                declEnc = SupportedEncoding.UTF16BE;
            }
            else if (encCount == encodingUnicode.Length && CompareCaseInsensitive(encodingUnicode, buffer, encStart))
            {
                if (e == SupportedEncoding.UTF8)
                    ThrowEncodingMismatch(SafeUTF8.GetString(buffer, encStart, encCount), SafeUTF8.GetString(encodingUTF8, 0, encodingUTF8.Length));
            }
            else
            {
                ThrowEncodingMismatch(SafeUTF8.GetString(buffer, encStart, encCount), e);
            }

            if (e != declEnc)
                ThrowEncodingMismatch(SafeUTF8.GetString(buffer, encStart, encCount), e);
        }

        static bool CompareCaseInsensitive(byte[] key, byte[] buffer, int offset)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] == buffer[offset + i])
                    continue;

                if (key[i] != Char.ToLower((char)buffer[offset + i], System.Globalization.CultureInfo.InvariantCulture))
                    return false;
            }
            return true;
        }

        static bool Compare(byte[] key, byte[] buffer, int offset)
        {
            for (int i = 0; i < key.Length; i++)
            {
                if (key[i] != buffer[offset + i])
                    return false;
            }
            return true;
        }

        static bool IsWhitespace(byte ch)
        {
            return ch == (byte)' ' || ch == (byte)'\n' || ch == (byte)'\t' || ch == (byte)'\r';
        }

        internal static ArraySegment<byte> ProcessBuffer(byte[] buffer, int offset, int count, Encoding encoding)
        {
            if (count < 4)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedEndOfFile)));

            try
            {
                int preserve;
                ArraySegment<byte> seg;

                SupportedEncoding expectedEnc = GetSupportedEncoding(encoding);
                SupportedEncoding declEnc = ReadBOMEncoding(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3], encoding == null, out preserve);
                if (expectedEnc != SupportedEncoding.None && expectedEnc != declEnc)
                    ThrowExpectedEncodingMismatch(expectedEnc, declEnc);

                offset += 4 - preserve;
                count -= 4 - preserve;

                // Fastpath: UTF-8
                char[] chars;
                byte[] bytes;
                Encoding localEnc;
                if (declEnc == SupportedEncoding.UTF8)
                {
                    // Fastpath: No declaration
                    if (buffer[offset + 1] != '?' || buffer[offset] != '<')
                    {
                        seg = new ArraySegment<byte>(buffer, offset, count);
                        return seg;
                    }

                    CheckUTF8DeclarationEncoding(buffer, offset, count, declEnc, expectedEnc);
                    seg = new ArraySegment<byte>(buffer, offset, count);
                    return seg;
                }

                // Convert to UTF-8
                localEnc = GetSafeEncoding(declEnc);
                int inputCount = Math.Min(count, BufferLength * 2);
                chars = new char[localEnc.GetMaxCharCount(inputCount)];
                int ccount = localEnc.GetChars(buffer, offset, inputCount, chars, 0);
                bytes = new byte[ValidatingUTF8.GetMaxByteCount(ccount)];
                int bcount = ValidatingUTF8.GetBytes(chars, 0, ccount, bytes, 0);

                // Check for declaration
                if (bytes[1] == '?' && bytes[0] == '<')
                {
                    CheckUTF8DeclarationEncoding(bytes, 0, bcount, declEnc, expectedEnc);
                }
                else
                {
                    // Declaration required if no out-of-band encoding
                    if (expectedEnc == SupportedEncoding.None)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlDeclarationRequired)));
                }

                seg = new ArraySegment<byte>(ValidatingUTF8.GetBytes(GetEncoding(declEnc).GetChars(buffer, offset, count)));
                return seg;
            }
            catch (DecoderFallbackException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidBytes), e));
            }
        }

        static void ThrowExpectedEncodingMismatch(SupportedEncoding expEnc, SupportedEncoding actualEnc)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlExpectedEncoding, GetEncodingName(expEnc), GetEncodingName(actualEnc))));
        }

        static void ThrowEncodingMismatch(string declEnc, SupportedEncoding enc)
        {
            ThrowEncodingMismatch(declEnc, GetEncodingName(enc));
        }

        static void ThrowEncodingMismatch(string declEnc, string docEnc)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlEncodingMismatch, declEnc, docEnc)));
        }

        // This stream wrapper does not support duplex
        public override bool CanRead
        {
            get
            {
                if (!isReading)
                    return false;

                return this.stream.CanRead;
            }
        }

        // The encoding conversion and buffering breaks seeking.
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        // This stream wrapper does not support duplex
        public override bool CanWrite
        {
            get
            {
                if (isReading)
                    return false;

                return this.stream.CanWrite;
            }
        }


        // The encoding conversion and buffering breaks seeking.
        public override long Position
        {
            get
            {
#pragma warning suppress 56503 // The contract for non seekable stream is to throw exception
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
        }

        public override void Close()
        {
            Flush();
            base.Close();
            this.stream.Close();
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override int ReadByte()
        {
            if (byteCount == 0 && encodingCode == SupportedEncoding.UTF8)
                return this.stream.ReadByte();
            if (Read(byteBuffer, 0, 1) == 0)
                return -1;
            return byteBuffer[0];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (byteCount == 0)
                {
                    if (encodingCode == SupportedEncoding.UTF8)
                        return this.stream.Read(buffer, offset, count);

                    // No more bytes than can be turned into characters
                    byteOffset = 0;
                    byteCount = this.stream.Read(bytes, byteCount, (chars.Length - 1) * 2);

                    // Check for end of stream
                    if (byteCount == 0)
                        return 0;

                    // Fix up incomplete chars
                    CleanupCharBreak();

                    // Change encoding
                    int charCount = this.encoding.GetChars(bytes, 0, byteCount, chars, 0);
                    byteCount = Encoding.UTF8.GetBytes(chars, 0, charCount, bytes, 0);
                }

                // Give them bytes
                if (byteCount < count)
                    count = byteCount;
                Buffer.BlockCopy(bytes, byteOffset, buffer, offset, count);
                byteOffset += count;
                byteCount -= count;
                return count;
            }
            catch (DecoderFallbackException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.XmlInvalidBytes), ex));
            }
        }

        void CleanupCharBreak()
        {
            int max = byteOffset + byteCount;

            // Read on 2 byte boundaries
            if ((byteCount % 2) != 0)
            {
                int b = this.stream.ReadByte();
                if (b < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedEndOfFile)));

                bytes[max++] = (byte)b;
                byteCount++;
            }

            // Don't cut off a surrogate character
            int w;
            if (encodingCode == SupportedEncoding.UTF16LE)
            {
                w = bytes[max - 2] + (bytes[max - 1] << 8);
            }
            else
            {
                w = bytes[max - 1] + (bytes[max - 2] << 8);
            }
            if ((w & 0xDC00) != 0xDC00 && w >= 0xD800 && w <= 0xDBFF)  // First 16-bit number of surrogate pair
            {
                int b1 = this.stream.ReadByte();
                int b2 = this.stream.ReadByte();
                if (b2 < 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.UnexpectedEndOfFile)));
                bytes[max++] = (byte)b1;
                bytes[max++] = (byte)b2;
                byteCount += 2;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public override void WriteByte(byte b)
        {
            if (encodingCode == SupportedEncoding.UTF8)
            {
                this.stream.WriteByte(b);
                return;
            }
            byteBuffer[0] = b;
            Write(byteBuffer, 0, 1);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Optimize UTF-8 case
            if (encodingCode == SupportedEncoding.UTF8)
            {
                this.stream.Write(buffer, offset, count);
                return;
            }

            while (count > 0)
            {
                int size = chars.Length < count ? chars.Length : count;
                int charCount = dec.GetChars(buffer, offset, size, chars, 0, false);
                byteCount = enc.GetBytes(chars, 0, charCount, bytes, 0, false);
                this.stream.Write(bytes, 0, byteCount);
                offset += size;
                count -= size;
            }
        }

        // Delegate properties
        public override bool CanTimeout { get { return this.stream.CanTimeout; } }
        public override long Length { get { return this.stream.Length; } }
        public override int ReadTimeout
        {
            get { return this.stream.ReadTimeout; }
            set { this.stream.ReadTimeout = value; }
        }
        public override int WriteTimeout
        {
            get { return this.stream.WriteTimeout; }
            set { this.stream.WriteTimeout = value; }
        }

        // Delegate methods
        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }
    }

    // Add format exceptions
    // Do we need to modify the stream position/Seek to account for the buffer?
    // ASSUMPTION ([....]): This class will only be used for EITHER reading OR writing.
#if NO
    class UTF16Stream : Stream
    {
        const int BufferLength = 128;
        
        Stream stream;
        bool bigEndian;
        byte[] streamBuffer;
        int streamOffset;
        int streamMax;
        byte[] trailBytes = new byte[4];
        int trailCount;
        
        public UTF16Stream(Stream stream, bool bigEndian)
        {
            this.stream = stream;
            this.bigEndian = bigEndian;
            this.streamBuffer = byte[BufferLength];
        }

        public override void Close()
        {
            Flush();
            base.Close();
            this.stream.Close();
        }

        public override void Flush()
        {
            this.stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Validate args
            
            // Read what we can if we aren't sure we have enough for a single character
            if (this.streamMax < 4)
                this.streamMax += this.stream.Read(this.streamBuffer, streamOffset, streamBuffer.Length - this.streamMax);

            int totalWritten = 0;
            while (streamOffset < streamMax && count > 0)
            {
                int ch;
                int read;
                
                read = ReadUTF16Char(out ch, streamBuffer, streamOffset, streamBuffer.Length - streamMax);
                if (read == 0)
                    break;

                int written = WriteUTF8Char(ch, buffer, offset, count);
                if (written == 0)
                    break;
                
                totalWritten += written;
                streamOffset += read;
                offset += written;
                count -= written;
            }
            
            // Shift down the leftover data
            if (this.streamOffset > 0 && this.streamOffset < this.streamMax)
            {
                Buffer.BlockCopy(this.streamBuffer, this.streamOffset, this.streamBuffer, 0, this.streamMax - this.streamOffset);
                this.streamMax -= this.streamOffset;
                this.streamOffset = 0;
            }

            return totalWritten;
        }

        int ReadUTF8Char(out int ch, byte[] buffer, int offset, int count)
        {
            ch = -1;
            if (buffer[offset] < 0x80)
            {
                ch = buffer[offset];
                return 1;
            }
            
            int mask = buffer[offset] & 0xF0;
            byte b1, b2, b3, b4;
            if (mask == 0xC0)
            {
                if (count < 2)
                    return 0;
                
                b1 = buffer[offset + 0];
                b2 = buffer[offset + 1];
                
                ch = ((b1 & 0x1F) << 6) + (b2 & 0x3F);
                
                return 2;
            }
            else if (mask == 0xE0)
            {
                if (count < 3)
                    return 0;
                
                b1 = buffer[offset + 0];
                b2 = buffer[offset + 1];
                b3 = buffer[offset + 2];

                ch = ((((b1 & 0x0F) << 6) + (b2 & 0x3F)) << 6) + (b3 & 0x3F);
                
                return 3;
            }
            else if (mask == 0xF0)
            {
                if (count < 4)
                    return 0;
                
                b1 = buffer[offset + 0];
                b2 = buffer[offset + 1];
                b3 = buffer[offset + 2];
                b4 = buffer[offset + 3];
                
                ch = ((((((b1 & 0x0F) << 6) + (b2 & 0x3F)) << 6) + (b3 & 0x3F)) << 6) + (b4 & 0x3F);
                
                return 4;
            }
             
            // Invalid
            return 0;
        }
        
        int ReadUTF16Char(out int ch, byte[] buffer, int offset, int count)
        {
            ch = -1;
            
            if (count < 2)
                return 0;
    
            int w1 = ReadEndian(buffer, offset);
            
            if (w1 < 0xD800 || w1 > 0xDFFF)
            {
                ch = w1;
                return 2;
            }
    
            if (count < 4)
                return 0;
    
            int w2 = ReadEndian(buffer, offset + 2);
    
            ch = ((w1 & 0x03FF) << 10) + (w2 & 0x03FF);
            return 4;
        }

        int ReadEndian(byte[] buffer, int offset)
        {
            if (bigEndian)
            {
                return (buffer[offset + 0] << 8) + buffer[offset + 1];
            }
            else
            {
                return (buffer[offset + 1] << 8) + buffer[offset + 0];
            }
        }

        int WriteUTF8Char(int ch, byte[] buffer, int offset, int count)
        {
            if (ch < 0x80)
            {
                buffer[offset] = (byte)ch;
                return 1;
            }
            else if (ch < 0x800)
            {
                if (count < 2)
                    return 0;

                buffer[offset + 1] = 0x80 | (ch & 0x3F);
                ch >>= 6;
                buffer[offset + 0] = 0xC0 | ch;
                return 2
            }
            else if (ch < 0x10000)
            {
                if (count < 3)
                    return 0;
                
                buffer[offset + 2] = 0x80 | (ch & 0x3F);
                ch >>= 6;
                buffer[offset + 1] = 0x80 | (ch & 0x3F);
                ch >>= 6;
                buffer[offset + 0] = 0xE0 | ch;
                return 3;
            }
            else if (ch <= 0x110000)
            {
                if (count < 4)
                    return 0;
                buffer[offset + 3] = 0x80 | (ch & 0x3F);
                ch >>= 6;
                buffer[offset + 2] = 0x80 | (ch & 0x3F);
                ch >>= 6;
                buffer[offset + 1] = 0x80 | (ch & 0x3F);
                ch >>= 6;
                buffer[offset + 0] = 0xF0 | ch;
                return 4;
            }
            
            // Invalid?
            return 0;
        }

        int WriteUTF16Char(int ch, byte[] buffer, int offset, int count)
        {
            if (ch < 0x10000)
            {
                if (count < 2)
                    return 0;

                WriteEndian(ch, buffer, offset);
                return 2;
            }

            if (count < 4)
                return 0;
            
            ch -= 0x10000;
            int w2 = 0xDC00 | (ch & 0x03FF);
            int w1 = 0xD800 | ch >> 10;
            WriteEndian(w1, buffer, offset);
            WriteEndian(w2, buffer, offset + 2);
            return 4;
        }

        void WriteEndian(int ch, byte[] buffer, int offset)
        {
            if (bigEndian)
            {
                buffer[offset + 1] = (byte)ch; 
                buffer[offset + 0] = ch >> 8;
            }
            else
            {
                buffer[offset + 0] = (byte)ch; 
                buffer[offset + 1] = ch >> 8;
            }
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            // Validate args
              
            // Write the trail bytes
            if (trailCount > 0)
            {
                int free = 4-trailCount;
                int total = (count < free ? count : free) + trialCount;
                Buffer.BlockCopy(buffer, offset, trailBytes, trailCount, total);
                
                int c;
                int r = ReadUTF8Char(out c, trailBuffer, 0, total);
                if (r == 0 && count < free)
                {
                    trailCount = total;
                    return;
                }

                int diff = r - trailCount;
                offset += diff;
                count -= diff;
                streamOffset = WriteUTF16Char(c, streamBuffer, 0, streamBuffer.Length - streamOffset);
            }
            
            while (count > 0)
            {
                if (streamBuffer.Length - streamOffset < 4)
                {
                    this.stream.Write(streamBuffer, 0, streamOffset);
                    streamOffset = 0;
                }

                int ch;
                int read = ReadUTF8Char(out ch, buffer, offset, count);
                if (read == 0)
                    break;

                int written = WriteUTF16Char(ch, streamBuffer, streamOffset, streamBuffer.Length - streamOffset);
                if (written == 0)
                    break;
                
                streamOffset += written;
                offset += read;
                count -= read;
            }
    
            if (streamOffset > 0)
            {
                this.stream.Write(streamBuffer, 0, streamOffset);
                streamOffset = 0;
            }
    
            // Save trailing bytes
            if (count > 0)
            {
                Buffer.BlockCopy(buffer, offset, trailBytes, 0, count);
                trailCount = count;
            }
        }

        // Delegate properties
        public override bool CanRead { get { return this.stream.CanRead; } }
        public override bool CanSeek { get { return this.stream.CanSeek; } }
        public override bool CanTimeout { get { return this.stream.CanTimeout; } }
        public override bool CanWrite { get { return this.stream.CanWrite; } }
        public override long Length { get { return this.stream.Length; } }
        public override long Position 
        { 
            get { return this.stream.Position; } 
            set { this.stream.Position = value; }
        }
        public override int ReadTimeout 
        { 
            get { return this.stream.ReadTimeout; } 
            set { this.stream.ReadTimeout = value; }
        }
        public override int WriteTimeout 
        { 
            get { return this.stream.WriteTimeout; } 
            set { this.stream.WriteTimeout = value; }
        }
    
        // Delegate methods
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.stream.Seek(offset, origin);
        }
    
        public override void SetLength(long value)
        {
            this.stream.SetLength(value);
        }
    }
#endif
}
