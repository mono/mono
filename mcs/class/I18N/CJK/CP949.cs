//
// I18N.CJK.CP949
//
// Author:
//   Hye-Shik Chang (perky@FreeBSD.org)
//

using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
    internal class CP949 : DbcsEncoding
    {
        // Magic number used by Windows for the UHC code page.
        private const int UHC_CODE_PAGE = 949;

        // Constructor.
        public CP949() : base(UHC_CODE_PAGE) {
            convert = KSConvert.Convert;
        }

        // Get the bytes that result from encoding a character buffer.
        public override int GetBytes(char[] chars, int charIndex, int charCount,
                         byte[] bytes, int byteIndex)
        {
            // 00 00 - FF FF
            base.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
            int origIndex = byteIndex;
            while (charCount-- > 0) {
                char c = chars[charIndex++];
                if (c <= 0x80 || c == 0xFF) { // ASCII
                    bytes[byteIndex++] = (byte)c;
                    continue;
                }
                byte b1 = convert.u2n[((int)c) * 2];
                byte b2 = convert.u2n[((int)c) * 2 + 1];
                if (b1 == 0 && b2 == 0) {
                    bytes[byteIndex++] = (byte)'?';
                } else {
                    bytes[byteIndex++] = b1;
                    bytes[byteIndex++] = b2;
                }
            }
            return byteIndex - origIndex;
        }

        // Get the characters that result from decoding a byte buffer.
        public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
                         char[] chars, int charIndex)
        {
            base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
            int origIndex = charIndex;
            int lastByte = 0;

            while (byteCount-- > 0) {
                int b = bytes[byteIndex++];
                if (lastByte == 0) {
                    if (b <= 0x80 || b == 0xFF) { // ASCII
                        chars[charIndex++] = (char)b;
                        continue;
                    } else {
                        lastByte = b;
                        continue;
                    }
                }

                char c1;
                if (lastByte < 0xa1) { // UHC Level 1
                    int ord = 8836 + (lastByte - 0x81) * 178;

                    if (b >= 0x41 && b <= 0x5A)
                        ord += b - 0x41;
                    else if (b >= 0x61 && b <= 0x7A)
                        ord += b - 0x61 + 26;
                    else if (b >= 0x81 && b <= 0xFE)
                        ord += b - 0x81 + 52;
                    else
                        ord = -1;

                    if (ord >= 0)
                        c1 = (char)(convert.n2u[ord*2] +
                                    convert.n2u[ord*2 + 1] * 256);
                    else
                        c1 = (char)0;
                } else if (lastByte <= 0xC6 && b < 0xa1) { // UHC Level 2
                    int ord = 14532 + (lastByte - 0x81) * 84;

                    if (b >= 0x41 && b <= 0x5A)
                        ord += b - 0x41;
                    else if (b >= 0x61 && b <= 0x7A)
                        ord += b - 0x61 + 26;
                    else if (b >= 0x81 && b <= 0xA0)
                        ord += b - 0x81 + 52;
                    else
                        ord = -1;

                    if (ord >= 0)
                        c1 = (char)(convert.n2u[ord*2] +
                                    convert.n2u[ord*2 + 1] * 256);
                    else
                        c1 = (char)0;
                } else if (b >= 0xA1 && b <= 0xFE) { // KS X 1001
                    int ord = ((lastByte - 0xA1) * 94 + b - 0xA1) * 2;

                    c1 = (char)(convert.n2u[ord] +
                                convert.n2u[ord + 1] * 256);
                } else
                    c1 = (char)0;

                if (c1 == 0)
                    chars[charIndex++] = '?';
                else
                    chars[charIndex++] = c1;
                lastByte = 0;
            }
            return charIndex - origIndex;
        }

        // Get a decoder that handles a rolling UHC state.
        public override Decoder GetDecoder()
        {
            return new CP949Decoder(convert);
        }

        // Get the mail body name for this encoding.
        public override String BodyName
        {
            get { return "ks_c_5601-1987"; }
        }

        // Get the human-readable name for this encoding.
        public override String EncodingName
        {
            get { return "Korean (UHC)"; }
        }

        // Get the mail agent header name for this encoding.
        public override String HeaderName
        {
            get { return "ks_c_5601-1987"; }
        }

        // Get the IANA-preferred Web name for this encoding.
        public override String WebName
        {
            get { return "euc-kr"; }
        }

        /*
        // Get the Windows code page represented by this object.
        public override int WindowsCodePage
        {
            get { return UHC_PAGE; }
        }
        */

        // Decoder that handles a rolling UHC state.
        private sealed class CP949Decoder : DbcsDecoder
        {
            // Constructor.
            public CP949Decoder(DbcsConvert convert) : base(convert) {}

            public override int GetChars(byte[] bytes, int byteIndex,
                                int byteCount, char[] chars, int charIndex)
            {
                base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
                int origIndex = charIndex;
                while (byteCount-- > 0) {
                    int b = bytes[byteIndex++];
                    if (lastByte == 0) {
                        if (b <= 0x80 || b == 0xFF) { // ASCII
                            chars[charIndex++] = (char)b;
                            continue;
                        } else {
                            lastByte = b;
                            continue;
                        }
                    }

                    char c1;
                    if (lastByte < 0xa1) { // UHC Level 1
                        int ord = 8836 + (lastByte - 0x81) * 178;

                        if (b >= 0x41 && b <= 0x5A)
                            ord += b - 0x41;
                        else if (b >= 0x61 && b <= 0x7A)
                            ord += b - 0x61 + 26;
                        else if (b >= 0x81 && b <= 0xFE)
                            ord += b - 0x81 + 52;
                        else
                            ord = -1;

                        if (ord >= 0)
                            c1 = (char)(convert.n2u[ord*2] +
                                        convert.n2u[ord*2 + 1] * 256);
                        else
                            c1 = (char)0;
                    } else if (lastByte <= 0xC6 && b < 0xA1) { // UHC Level 2
                        int ord = 14532 + (lastByte - 0xA1) * 84;

                        if (b >= 0x41 && b <= 0x5A)
                            ord += b - 0x41;
                        else if (b >= 0x61 && b <= 0x7A)
                            ord += b - 0x61 + 26;
                        else if (b >= 0x81 && b <= 0xA0)
                            ord += b - 0x81 + 52;
                        else
                            ord = -1;

                        if (ord >= 0)
                            c1 = (char)(convert.n2u[ord*2] +
                                        convert.n2u[ord*2 + 1] * 256);
                        else
                            c1 = (char)0;
                    } else if (b >= 0xA1 && b <= 0xFE) { // KS X 1001
                        int ord = ((lastByte - 0xA1) * 94 + b - 0xA1) * 2;

                        c1 = (char)(convert.n2u[ord] +
                                    convert.n2u[ord + 1] * 256);
                    } else
                        c1 = (char)0;

                    if (c1 == 0)
                        chars[charIndex++] = '?';
                    else
                        chars[charIndex++] = c1;
                    lastByte = 0;
                }
                return charIndex - origIndex;
            }
        }
    }

    internal class ENCuhc : CP949
    {
        public ENCuhc() {}
    }
}

// ex: ts=8 sts=4 et
