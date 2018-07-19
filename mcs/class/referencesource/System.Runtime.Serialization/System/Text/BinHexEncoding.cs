//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Text
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization; //For SR
    using System.Security;

    class BinHexEncoding : Encoding
    {
        static byte[] char2val = new byte[128]
        {
                /*    0-15 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /*   16-31 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /*   32-47 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /*   48-63 */ 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /*   64-79 */ 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /*   80-95 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /*  96-111 */ 0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                /* 112-127 */ 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };

        static string val2char = "0123456789ABCDEF";

#if DEBUG
        static BinHexEncoding()
        {
            for (char ch = '0'; ch <= '9'; ch++)
            {
                Fx.Assert(char2val[ch] == ch - '0', "");
            }

            for (char ch = 'A'; ch <= 'F'; ch++)
            {
                Fx.Assert(char2val[ch] == ch - 'A' + 10, "");
            }

            for (char ch = 'a'; ch <= 'f'; ch++)
            {
                Fx.Assert(char2val[ch] == ch - 'a' + 10, "");
            }
        }
#endif

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charCount", SR.GetString(SR.ValueMustBeNonNegative)));
            if ((charCount % 2) != 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.XmlInvalidBinHexLength, charCount.ToString(NumberFormatInfo.CurrentInfo))));
            return charCount / 2;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return GetMaxByteCount(count);
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            if (charIndex < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", SR.GetString(SR.ValueMustBeNonNegative)));
            if (charIndex > chars.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
            if (charCount < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charCount", SR.GetString(SR.ValueMustBeNonNegative)));
            if (charCount > chars.Length - charIndex)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charCount", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - charIndex)));
            if (bytes == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bytes"));
            if (byteIndex < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", SR.GetString(SR.ValueMustBeNonNegative)));
            if (byteIndex > bytes.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", SR.GetString(SR.OffsetExceedsBufferSize, bytes.Length)));
            int byteCount = GetByteCount(chars, charIndex, charCount);
            if (byteCount < 0 || byteCount > bytes.Length - byteIndex)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlArrayTooSmall), "bytes"));
            if (charCount > 0)
            {
                fixed (byte* _char2val = char2val)
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        fixed (char* _chars = &chars[charIndex])
                        {
                            char* pch = _chars;
                            char* pchMax = _chars + charCount;
                            byte* pb = _bytes;
                            while (pch < pchMax)
                            {
                                Fx.Assert(pch + 2 <= pchMax, "");
                                char pch0 = pch[0];
                                char pch1 = pch[1];
                                if ((pch0 | pch1) >= 128)
                                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.XmlInvalidBinHexSequence, new string(pch, 0, 2), charIndex + (int)(pch - _chars))));
                                byte d1 = _char2val[pch0];
                                byte d2 = _char2val[pch1];
                                if ((d1 | d2) == 0xFF)
                                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.XmlInvalidBinHexSequence, new string(pch, 0, 2), charIndex + (int)(pch - _chars))));
                                pb[0] = (byte)((d1 << 4) + d2);
                                pch += 2;
                                pb++;
                            }
                        }
                    }
                }
            }
            return byteCount;
        }
#if NO
        public override Encoder GetEncoder()
        {
            return new BufferedEncoder(this, 2);
        }
#endif
        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0 || byteCount > int.MaxValue / 2)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", SR.GetString(SR.ValueMustBeInRange, 0, int.MaxValue / 2)));
            return byteCount * 2;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return GetMaxCharCount(count);
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if (bytes == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("bytes"));
            if (byteIndex < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", SR.GetString(SR.ValueMustBeNonNegative)));
            if (byteIndex > bytes.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteIndex", SR.GetString(SR.OffsetExceedsBufferSize, bytes.Length)));
            if (byteCount < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", SR.GetString(SR.ValueMustBeNonNegative)));
            if (byteCount > bytes.Length - byteIndex)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("byteCount", SR.GetString(SR.SizeExceedsRemainingBufferSpace, bytes.Length - byteIndex)));
            int charCount = GetCharCount(bytes, byteIndex, byteCount);
            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            if (charIndex < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", SR.GetString(SR.ValueMustBeNonNegative)));
            if (charIndex > chars.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("charIndex", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
            if (charCount < 0 || charCount > chars.Length - charIndex)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.XmlArrayTooSmall), "chars"));
            if (byteCount > 0)
            {
                fixed (char* _val2char = val2char)
                {
                    fixed (byte* _bytes = &bytes[byteIndex])
                    {
                        fixed (char* _chars = &chars[charIndex])
                        {
                            char* pch = _chars;
                            byte* pb = _bytes;
                            byte* pbMax = _bytes + byteCount;
                            while (pb < pbMax)
                            {
                                pch[0] = _val2char[pb[0] >> 4];
                                pch[1] = _val2char[pb[0] & 0x0F];
                                pb++;
                                pch += 2;
                            }
                        }
                    }
                }
            }
            return charCount;
        }
    }
}
