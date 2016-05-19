//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

#pragma warning disable 1634 // Stops compiler from warning about unknown warnings (for Presharp)

namespace System.Runtime.Serialization.Json
{
    using System.IO;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;
    using System.Security;

    // This wrapper does not support seek.
    // Supports: UTF-8, Unicode, BigEndianUnicode
    // ASSUMPTION ([....]): This class will only be used for EITHER reading OR writing.  It can be done, it would just mean more buffers.
    class JsonEncodingStreamWrapper : Stream
    {
        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        static readonly UnicodeEncoding SafeBEUTF16 = new UnicodeEncoding(true, false, false);

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        static readonly UnicodeEncoding SafeUTF16 = new UnicodeEncoding(false, false, false);

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        static readonly UTF8Encoding SafeUTF8 = new UTF8Encoding(false, false);

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        static readonly UnicodeEncoding ValidatingBEUTF16 = new UnicodeEncoding(true, false, true);

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        static readonly UnicodeEncoding ValidatingUTF16 = new UnicodeEncoding(false, false, true);

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        static readonly UTF8Encoding ValidatingUTF8 = new UTF8Encoding(false, true);
        const int BufferLength = 128;

        byte[] byteBuffer = new byte[1];
        int byteCount;
        int byteOffset;
        byte[] bytes;
        char[] chars;
        Decoder dec;
        Encoder enc;
        Encoding encoding;

        SupportedEncoding encodingCode;
        bool isReading;

        Stream stream;

        public JsonEncodingStreamWrapper(Stream stream, Encoding encoding, bool isReader)
        {
            this.isReading = isReader;
            if (isReader)
            {
                InitForReading(stream, encoding);
            }
            else
            {
                if (encoding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
                }

                InitForWriting(stream, encoding);
            }
        }

        enum SupportedEncoding
        {
            UTF8,
            UTF16LE,
            UTF16BE,
            None
        }

        // This stream wrapper does not support duplex
        public override bool CanRead
        {
            get
            {
                if (!isReading)
                {
                    return false;
                }

                return this.stream.CanRead;
            }
        }

        // The encoding conversion and buffering breaks seeking.
        public override bool CanSeek
        {
            get { return false; }
        }

        // Delegate properties
        public override bool CanTimeout
        {
            get { return this.stream.CanTimeout; }
        }

        // This stream wrapper does not support duplex
        public override bool CanWrite
        {
            get
            {
                if (isReading)
                {
                    return false;
                }

                return this.stream.CanWrite;
            }
        }

        public override long Length
        {
            get { return this.stream.Length; }
        }


        // The encoding conversion and buffering breaks seeking.
        public override long Position
        {
            get
            {
#pragma warning suppress 56503 // The contract for non seekable stream is to throw exception
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            set { throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException()); }
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

        public static ArraySegment<byte> ProcessBuffer(byte[] buffer, int offset, int count, Encoding encoding)
        {
            try
            {
                SupportedEncoding expectedEnc = GetSupportedEncoding(encoding);
                SupportedEncoding dataEnc;
                if (count < 2)
                {
                    dataEnc = SupportedEncoding.UTF8;
                }
                else
                {
                    dataEnc = ReadEncoding(buffer[offset], buffer[offset + 1]);
                }
                if ((expectedEnc != SupportedEncoding.None) && (expectedEnc != dataEnc))
                {
                    ThrowExpectedEncodingMismatch(expectedEnc, dataEnc);
                }

                // Fastpath: UTF-8
                if (dataEnc == SupportedEncoding.UTF8)
                {
                    return new ArraySegment<byte>(buffer, offset, count);
                }

                // Convert to UTF-8
                return
                    new ArraySegment<byte>(ValidatingUTF8.GetBytes(GetEncoding(dataEnc).GetChars(buffer, offset, count)));
            }
            catch (DecoderFallbackException e)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonInvalidBytes), e));
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (byteCount == 0)
                {
                    if (encodingCode == SupportedEncoding.UTF8)
                    {
                        return this.stream.Read(buffer, offset, count);
                    }

                    // No more bytes than can be turned into characters
                    byteOffset = 0;
                    byteCount = this.stream.Read(bytes, byteCount, (chars.Length - 1) * 2);

                    // Check for end of stream
                    if (byteCount == 0)
                    {
                        return 0;
                    }

                    // Fix up incomplete chars
                    CleanupCharBreak();

                    // Change encoding
                    int charCount = this.encoding.GetChars(bytes, 0, byteCount, chars, 0);
                    byteCount = Encoding.UTF8.GetBytes(chars, 0, charCount, bytes, 0);
                }

                // Give them bytes
                if (byteCount < count)
                {
                    count = byteCount;
                }
                Buffer.BlockCopy(bytes, byteOffset, buffer, offset, count);
                byteOffset += count;
                byteCount -= count;
                return count;
            }
            catch (DecoderFallbackException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonInvalidBytes), ex));
            }
        }

        public override int ReadByte()
        {
            if (byteCount == 0 && encodingCode == SupportedEncoding.UTF8)
            {
                return this.stream.ReadByte();
            }
            if (Read(byteBuffer, 0, 1) == 0)
            {
                return -1;
            }
            return byteBuffer[0];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        // Delegate methods
        public override void SetLength(long value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonEncodingNotSupported)));
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonEncodingNotSupported)));
            }
        }

        static SupportedEncoding GetSupportedEncoding(Encoding encoding)
        {
            if (encoding == null)
            {
                return SupportedEncoding.None;
            }
            if (encoding.WebName == ValidatingUTF8.WebName)
            {
                return SupportedEncoding.UTF8;
            }
            else if (encoding.WebName == ValidatingUTF16.WebName)
            {
                return SupportedEncoding.UTF16LE;
            }
            else if (encoding.WebName == ValidatingBEUTF16.WebName)
            {
                return SupportedEncoding.UTF16BE;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonEncodingNotSupported)));
            }
        }

        static SupportedEncoding ReadEncoding(byte b1, byte b2)
        {
            if (b1 == 0x00 && b2 != 0x00)
            {
                return SupportedEncoding.UTF16BE;
            }
            else if (b1 != 0x00 && b2 == 0x00)
            {
                // 857 It's possible to misdetect UTF-32LE as UTF-16LE, but that's OK.
                return SupportedEncoding.UTF16LE;
            }
            else if (b1 == 0x00 && b2 == 0x00)
            {
                // UTF-32BE not supported
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.JsonInvalidBytes)));
            }
            else
            {
                return SupportedEncoding.UTF8;
            }
        }

        static void ThrowExpectedEncodingMismatch(SupportedEncoding expEnc, SupportedEncoding actualEnc)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.JsonExpectedEncoding, GetEncodingName(expEnc), GetEncodingName(actualEnc))));
        }

        void CleanupCharBreak()
        {
            int max = byteOffset + byteCount;

            // Read on 2 byte boundaries
            if ((byteCount % 2) != 0)
            {
                int b = this.stream.ReadByte();
                if (b < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonUnexpectedEndOfFile)));
                }

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
            if ((w & 0xDC00) != 0xDC00 && w >= 0xD800 && w <= 0xDBFF) // First 16-bit number of surrogate pair
            {
                int b1 = this.stream.ReadByte();
                int b2 = this.stream.ReadByte();
                if (b2 < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonUnexpectedEndOfFile)));
                }
                bytes[max++] = (byte)b1;
                bytes[max++] = (byte)b2;
                byteCount += 2;
            }
        }

        void EnsureBuffers()
        {
            EnsureByteBuffer();
            if (chars == null)
            {
                chars = new char[BufferLength];
            }
        }

        void EnsureByteBuffer()
        {
            if (bytes != null)
            {
                return;
            }

            bytes = new byte[BufferLength * 4];
            byteOffset = 0;
            byteCount = 0;
        }

        void FillBuffer(int count)
        {
            count -= byteCount;
            while (count > 0)
            {
                int read = stream.Read(bytes, byteOffset + byteCount, count);
                if (read == 0)
                {
                    break;
                }

                byteCount += read;
                count -= read;
            }
        }

        void InitForReading(Stream inputStream, Encoding expectedEncoding)
        {
            try
            {
                this.stream = new BufferedStream(inputStream);

                SupportedEncoding expectedEnc = GetSupportedEncoding(expectedEncoding);
                SupportedEncoding dataEnc = ReadEncoding();
                if ((expectedEnc != SupportedEncoding.None) && (expectedEnc != dataEnc))
                {
                    ThrowExpectedEncodingMismatch(expectedEnc, dataEnc);
                }

                // Fastpath: UTF-8 (do nothing)
                if (dataEnc != SupportedEncoding.UTF8)
                {
                    // Convert to UTF-8
                    EnsureBuffers();
                    FillBuffer((BufferLength - 1) * 2);
                    this.encodingCode = dataEnc;
                    this.encoding = GetEncoding(dataEnc);
                    CleanupCharBreak();
                    int count = this.encoding.GetChars(bytes, byteOffset, byteCount, chars, 0);
                    byteOffset = 0;
                    byteCount = ValidatingUTF8.GetBytes(chars, 0, count, bytes, 0);
                }
            }
            catch (DecoderFallbackException ex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonInvalidBytes), ex));
            }
        }

        void InitForWriting(Stream outputStream, Encoding writeEncoding)
        {
            this.encoding = writeEncoding;
            this.stream = new BufferedStream(outputStream);

            // Set the encoding code
            this.encodingCode = GetSupportedEncoding(writeEncoding);

            if (this.encodingCode != SupportedEncoding.UTF8)
            {
                EnsureBuffers();
                dec = ValidatingUTF8.GetDecoder();
                enc = this.encoding.GetEncoder();
            }
        }

        SupportedEncoding ReadEncoding()
        {
            int b1 = this.stream.ReadByte();
            int b2 = this.stream.ReadByte();

            EnsureByteBuffer();

            SupportedEncoding e;

            if (b1 == -1)
            {
                e = SupportedEncoding.UTF8;
                byteCount = 0;
            }
            else if (b2 == -1)
            {
                e = SupportedEncoding.UTF8;
                bytes[0] = (byte)b1;
                byteCount = 1;
            }
            else
            {
                e = ReadEncoding((byte)b1, (byte)b2);
                bytes[0] = (byte)b1;
                bytes[1] = (byte)b2;
                byteCount = 2;
            }

            return e;
        }
    }
}
