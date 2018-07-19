//------------------------------------------------------------------------------
// <copyright file="WebUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// Don't entity encode high chars (160 to 256), to fix bugs VSWhidbey 85857/111927
// 
#define ENTITY_ENCODE_HIGH_ASCII_CHARS

namespace System.Net {
    using System;
    using System.Collections.Generic;
#if !FEATURE_NETCORE
    using System.Configuration;
#endif
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net.Configuration;
    using System.Runtime.Versioning;
    using System.Text;
#if FEATURE_NETCORE
    using System.Security;
#endif

    public static class WebUtility
    {
        // some consts copied from Char / CharUnicodeInfo since we don't have friend access to those types
        private const char HIGH_SURROGATE_START = '\uD800';
        private const char LOW_SURROGATE_START = '\uDC00';
        private const char LOW_SURROGATE_END = '\uDFFF';
        private const int UNICODE_PLANE00_END = 0x00FFFF;
        private const int UNICODE_PLANE01_START = 0x10000;
        private const int UNICODE_PLANE16_END = 0x10FFFF;

        private const int UnicodeReplacementChar = '\uFFFD';

        private static readonly char[] _htmlEntityEndingChars = new char[] { ';', '&' };

        private static volatile UnicodeDecodingConformance _htmlDecodeConformance = UnicodeDecodingConformance.Auto;
        private static volatile UnicodeEncodingConformance _htmlEncodeConformance = UnicodeEncodingConformance.Auto;

        #region HtmlEncode / HtmlDecode methods

        public static string HtmlEncode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return value;
            }

            // Don't create string writer if we don't have nothing to encode
            int index = IndexOfHtmlEncodingChars(value, 0);
            if (index == -1) {
                return value;
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlEncode(value, writer);
            return writer.ToString();
        }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        public static unsafe void HtmlEncode(string value, TextWriter output) {
            if (value == null) {
                return;
            }
            if (output == null) {
                throw new ArgumentNullException("output");
            }

            int index = IndexOfHtmlEncodingChars(value, 0);
            if (index == -1) {
                output.Write(value);
                return;
            }

            Debug.Assert(0 <= index && index <= value.Length, "0 <= index && index <= value.Length");

            UnicodeEncodingConformance encodeConformance = HtmlEncodeConformance;
            int cch = value.Length - index;
            fixed (char* str = value) {
                char* pch = str;
                while (index-- > 0) {
                    output.Write(*pch++);
                }

                for (; cch > 0; cch--, pch++) {
                    char ch = *pch;
                    if (ch <= '>') {
                        switch (ch) {
                            case '<':
                                output.Write("&lt;");
                                break;
                            case '>':
                                output.Write("&gt;");
                                break;
                            case '"':
                                output.Write("&quot;");
                                break;
                            case '\'':
                                output.Write("&#39;");
                                break;
                            case '&':
                                output.Write("&amp;");
                                break;
                            default:
                                output.Write(ch);
                                break;
                        }
                    }
                    else {
                        int valueToEncode = -1; // set to >= 0 if needs to be encoded

#if ENTITY_ENCODE_HIGH_ASCII_CHARS

#if MONO
                        // MS starts encoding with &# from 160 and stops at 255.
                        // We don't do that. One reason is the 65308/65310 unicode
                        // characters that look like '<' and '>'.
                        if (ch >= 160 && !char.IsSurrogate (ch)) {
                            valueToEncode = ch;
#else
                        if (ch >= 160 && ch < 256) {
                            // The seemingly arbitrary 160 comes from RFC
                            valueToEncode = ch;
#endif
                        } else
#endif // ENTITY_ENCODE_HIGH_ASCII_CHARS
                        if (encodeConformance == UnicodeEncodingConformance.Strict && Char.IsSurrogate(ch)) {
                            int scalarValue = GetNextUnicodeScalarValueFromUtf16Surrogate(ref pch, ref cch);
                            if (scalarValue >= UNICODE_PLANE01_START) {
                                valueToEncode = scalarValue;
                            }
                            else {
                                // Don't encode BMP characters (like U+FFFD) since they wouldn't have
                                // been encoded if explicitly present in the string anyway.
                                ch = (char)scalarValue;
                            }
                        }

                        if (valueToEncode >= 0) {
                            // value needs to be encoded
                            output.Write("&#");
                            output.Write(valueToEncode.ToString(NumberFormatInfo.InvariantInfo));
                            output.Write(';');
                        }
                        else {
                            // write out the character directly
                            output.Write(ch);
                        }
                    }
                }
            }
        }

        public static string HtmlDecode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return value;
            }

            // Don't create string writer if we don't have nothing to encode
            if (!StringRequiresHtmlDecoding(value)) {
                return value;
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlDecode(value, writer);
            return writer.ToString();
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.UInt16.TryParse(System.String,System.Globalization.NumberStyles,System.IFormatProvider,System.UInt16@)", Justification="UInt16.TryParse guarantees that result is zero if the parse fails.")]
        public static void HtmlDecode(string value, TextWriter output) {
            if (value == null) {
                return;
            }
            if (output == null) {
                throw new ArgumentNullException("output");
            }

            if (!StringRequiresHtmlDecoding(value)) {
                output.Write(value);        // good as is
                return;
            }

            UnicodeDecodingConformance decodeConformance = HtmlDecodeConformance;
            int l = value.Length;
            for (int i = 0; i < l; i++) {
                char ch = value[i];

                if (ch == '&') {
                    // We found a '&'. Now look for the next ';' or '&'. The idea is that
                    // if we find another '&' before finding a ';', then this is not an entity,
                    // and the next '&' might start a real entity (VSWhidbey 275184)
                    int index = value.IndexOfAny(_htmlEntityEndingChars, i + 1);
                    if (index > 0 && value[index] == ';') {
                        string entity = value.Substring(i + 1, index - i - 1);

                        if (entity.Length > 1 && entity[0] == '#') {
                            // The # syntax can be in decimal or hex, e.g.
                            //      &#229;  --> decimal
                            //      &#xE5;  --> same char in hex
                            // See http://www.w3.org/TR/REC-html40/charset.html#entities

                            bool parsedSuccessfully;
                            uint parsedValue;
                            if (entity[1] == 'x' || entity[1] == 'X') {
                                parsedSuccessfully = UInt32.TryParse(entity.Substring(2), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out parsedValue);
                            }
                            else {
                                parsedSuccessfully = UInt32.TryParse(entity.Substring(1), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out parsedValue);
                            }

                            if (parsedSuccessfully) {
                                switch (decodeConformance) {
                                    case UnicodeDecodingConformance.Strict:
                                        // decoded character must be U+0000 .. U+10FFFF, excluding surrogates
                                        parsedSuccessfully = ((parsedValue < HIGH_SURROGATE_START) || (LOW_SURROGATE_END < parsedValue && parsedValue <= UNICODE_PLANE16_END));
                                        break;

                                    case UnicodeDecodingConformance.Compat:
                                        // decoded character must be U+0001 .. U+FFFF
                                        // null chars disallowed for compat with 4.0
                                        parsedSuccessfully = (0 < parsedValue && parsedValue <= UNICODE_PLANE00_END);
                                        break;

                                    case UnicodeDecodingConformance.Loose:
                                        // decoded character must be U+0000 .. U+10FFFF
                                        parsedSuccessfully = (parsedValue <= UNICODE_PLANE16_END);
                                        break;

                                    default:
                                        Debug.Assert(false, "Should never get here!");
                                        parsedSuccessfully = false;
                                        break;
                                }
                            }

                            if (parsedSuccessfully) {
                                if (parsedValue <= UNICODE_PLANE00_END) {
                                    // single character
                                    output.Write((char)parsedValue);
                                }
                                else {
                                    // multi-character
                                    char leadingSurrogate, trailingSurrogate;
                                    ConvertSmpToUtf16(parsedValue, out leadingSurrogate, out trailingSurrogate);
                                    output.Write(leadingSurrogate);
                                    output.Write(trailingSurrogate);
                                }
                                
                                i = index; // already looked at everything until semicolon
                                continue;
                            }
                        }
                        else {
                            i = index; // already looked at everything until semicolon

                            char entityChar = HtmlEntities.Lookup(entity);
                            if (entityChar != (char)0) {
                                ch = entityChar;
                            }
                            else {
                                output.Write('&');
                                output.Write(entity);
                                output.Write(';');
                                continue;
                            }
                        }

                    }
                }

                output.Write(ch);
            }
        }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        private static unsafe int IndexOfHtmlEncodingChars(string s, int startPos) {
            Debug.Assert(0 <= startPos && startPos <= s.Length, "0 <= startPos && startPos <= s.Length");

            UnicodeEncodingConformance encodeConformance = HtmlEncodeConformance;
            int cch = s.Length - startPos;
            fixed (char* str = s) {
                for (char* pch = &str[startPos]; cch > 0; pch++, cch--) {
                    char ch = *pch;
                    if (ch <= '>') {
                        switch (ch) {
                            case '<':
                            case '>':
                            case '"':
                            case '\'':
                            case '&':
                                return s.Length - cch;
                        }
                    }
#if ENTITY_ENCODE_HIGH_ASCII_CHARS
                    else if (ch >= 160 
#if !MONO
						&& ch < 256
#endif
					) {
                        return s.Length - cch;
                    }
#endif // ENTITY_ENCODE_HIGH_ASCII_CHARS
                    else if (encodeConformance == UnicodeEncodingConformance.Strict && Char.IsSurrogate(ch)) {
                        return s.Length - cch;
                    }
                }
            }

            return -1;
        }

        private static UnicodeDecodingConformance HtmlDecodeConformance {
            get {
                if (_htmlDecodeConformance != UnicodeDecodingConformance.Auto) {
                    return _htmlDecodeConformance;
                }
    
                UnicodeDecodingConformance defaultDecodeConformance = (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5) ? UnicodeDecodingConformance.Strict : UnicodeDecodingConformance.Compat;
                UnicodeDecodingConformance decodingConformance = defaultDecodeConformance;

#if !FEATURE_NETCORE && !MOBILE
                try {
                    // Read from config
                    decodingConformance = SettingsSectionInternal.Section.WebUtilityUnicodeDecodingConformance;
                    // Normalize conformance settings (turn 'Auto' into the actual setting)
                    if (decodingConformance <= UnicodeDecodingConformance.Auto || decodingConformance > UnicodeDecodingConformance.Loose) {
                        decodingConformance = defaultDecodeConformance;
                    }
                }
                catch (ConfigurationException) {
                    // Continue with default values
                    // HtmlDecode and related methods can still be called and format the error page intended for the client
                    // No need to retry again to initialize from the config in case of config errors
                    decodingConformance = defaultDecodeConformance;
                }
                catch {
                    // DevDiv: 642025
                    // ASP.NET uses own ConfigurationManager which can throw in more situations than config errors (i.e. BadRequest)
                    // It's ok to swallow the exception here and continue using the default value
                    // Try to initialize again the next time
                    return defaultDecodeConformance;
                }
#endif
                _htmlDecodeConformance = decodingConformance;

                return _htmlDecodeConformance;
            }
        }

        private static UnicodeEncodingConformance HtmlEncodeConformance {
            get {
                if (_htmlEncodeConformance != UnicodeEncodingConformance.Auto) {
                    return _htmlEncodeConformance;
                }
    
                UnicodeEncodingConformance defaultEncodeConformance = (BinaryCompatibility.TargetsAtLeast_Desktop_V4_5) ? UnicodeEncodingConformance.Strict : UnicodeEncodingConformance.Compat;
                UnicodeEncodingConformance encodingConformance = defaultEncodeConformance;

#if !FEATURE_NETCORE && !MOBILE
                try {
                    // Read from config
                    encodingConformance = SettingsSectionInternal.Section.WebUtilityUnicodeEncodingConformance;

                    // Normalize conformance settings (turn 'Auto' into the actual setting)
                    if (encodingConformance <= UnicodeEncodingConformance.Auto || encodingConformance > UnicodeEncodingConformance.Compat) {
                        encodingConformance = defaultEncodeConformance;
                    }
                }
                catch (ConfigurationException) {
                    // Continue with default values
                    // HtmlEncode and related methods can still be called and format the error page intended for the client
                    // No need to retry again to initialize from the config in case of config errors
                    encodingConformance = defaultEncodeConformance;
                }
                catch {
                    // DevDiv: 642025
                    // ASP.NET uses own ConfigurationManager which can throw in more situations than config errors (i.e. BadRequest)
                    // It's ok to swallow the exception here and continue using the default value
                    // Try to initialize again the next time
                    return defaultEncodeConformance;
                }
#endif
                _htmlEncodeConformance = encodingConformance;

                return _htmlEncodeConformance;
            }
        }

        #endregion

        #region UrlEncode implementation

        // *** Source: alm/tfs_core/Framework/Common/UriUtility/HttpUtility.cs
        // This specific code was copied from above ASP.NET codebase.

        private static byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        private static byte[] UrlEncode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                    cSpaces++;
                else if (!IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0) {
                // DevDiv 912606: respect "offset" and "count"
                if (0 == offset && bytes.Length == count) {
                    return bytes;
                }
                else {
                    var subarray = new byte[count];
                    Buffer.BlockCopy(bytes, offset, subarray, 0, count);
                    return subarray;
                }
            }

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (IsUrlSafeChar(ch))
                {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ')
                {
                    expandedBytes[pos++] = (byte)'+';
                }
                else
                {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        #endregion

        #region UrlEncode public methods

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification="Already shipped public API; code moved here as part of API consolidation")]
        public static string UrlEncode(string value)
        {
            if (value == null)
                return null;

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Encoding.UTF8.GetString(UrlEncode(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */));
        }

        public static byte[] UrlEncodeToBytes(byte[] value, int offset, int count)
        {
            return UrlEncode(value, offset, count, true /* alwaysCreateNewReturnValue */);
        }

        #endregion

        #region UrlDecode implementation

        // *** Source: alm/tfs_core/Framework/Common/UriUtility/HttpUtility.cs
        // This specific code was copied from above ASP.NET codebase.
        // Changes done - Removed the logic to handle %Uxxxx as it is not standards compliant.

        private static string UrlDecodeInternal(string value, Encoding encoding)
        {
            if (value == null)
            {
                return null;
            }

            int count = value.Length;
            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the string's chars collapsing %XX and
            // appending each char as char, with exception of %XX constructs
            // that are appended as bytes

            for (int pos = 0; pos < count; pos++)
            {
                char ch = value[pos];

                if (ch == '+')
                {
                    ch = ' ';
                }
                else if (ch == '%' && pos < count - 2)
                {
                    int h1 = HexToInt(value[pos + 1]);
                    int h2 = HexToInt(value[pos + 2]);

                    if (h1 >= 0 && h2 >= 0)
                    {     // valid 2 hex chars
                        byte b = (byte)((h1 << 4) | h2);
                        pos += 2;

                        // don't add as char
                        helper.AddByte(b);
                        continue;
                    }
                }

                if ((ch & 0xFF80) == 0)
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                else
                    helper.AddChar(ch);
            }

            return helper.GetString();
        }

        private static byte[] UrlDecodeInternal(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }

            int decodedBytesCount = 0;
            byte[] decodedBytes = new byte[count];

            for (int i = 0; i < count; i++)
            {
                int pos = offset + i;
                byte b = bytes[pos];

                if (b == '+')
                {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2)
                {
                    int h1 = HexToInt((char)bytes[pos + 1]);
                    int h2 = HexToInt((char)bytes[pos + 2]);

                    if (h1 >= 0 && h2 >= 0)
                    {     // valid 2 hex chars
                        b = (byte)((h1 << 4) | h2);
                        i += 2;
                    }
                }

                decodedBytes[decodedBytesCount++] = b;
            }

            if (decodedBytesCount < decodedBytes.Length)
            {
                byte[] newDecodedBytes = new byte[decodedBytesCount];
                Array.Copy(decodedBytes, newDecodedBytes, decodedBytesCount);
                decodedBytes = newDecodedBytes;
            }

            return decodedBytes;
        }

        #endregion

        #region UrlDecode public methods


        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification="Already shipped public API; code moved here as part of API consolidation")]
        public static string UrlDecode(string encodedValue)
        {
            if (encodedValue == null)
                return null;

            return UrlDecodeInternal(encodedValue, Encoding.UTF8);
        }

        public static byte[] UrlDecodeToBytes(byte[] encodedValue, int offset, int count)
        {
            return UrlDecodeInternal(encodedValue, offset, count);
        }

        #endregion

        #region Helper methods

        // similar to Char.ConvertFromUtf32, but doesn't check arguments or generate strings
        // input is assumed to be an SMP character
        private static void ConvertSmpToUtf16(uint smpChar, out char leadingSurrogate, out char trailingSurrogate) {
            Debug.Assert(UNICODE_PLANE01_START <= smpChar && smpChar <= UNICODE_PLANE16_END);

            int utf32 = (int)(smpChar - UNICODE_PLANE01_START);
            leadingSurrogate = (char)((utf32 / 0x400) + HIGH_SURROGATE_START);
            trailingSurrogate = (char)((utf32 % 0x400) + LOW_SURROGATE_START);
        }

#if FEATURE_NETCORE
        [SecuritySafeCritical]
#endif
        private static unsafe int GetNextUnicodeScalarValueFromUtf16Surrogate(ref char* pch, ref int charsRemaining) {
            // invariants
            Debug.Assert(charsRemaining >= 1);
            Debug.Assert(Char.IsSurrogate(*pch));

            if (charsRemaining <= 1) {
                // not enough characters remaining to resurrect the original scalar value
                return UnicodeReplacementChar;
            }

            char leadingSurrogate = pch[0];
            char trailingSurrogate = pch[1];

            if (Char.IsSurrogatePair(leadingSurrogate, trailingSurrogate)) {
                // we're going to consume an extra char
                pch++;
                charsRemaining--;

                // below code is from Char.ConvertToUtf32, but without the checks (since we just performed them)
                return (((leadingSurrogate - HIGH_SURROGATE_START) * 0x400) + (trailingSurrogate - LOW_SURROGATE_START) + UNICODE_PLANE01_START);
            }
            else {
                // unmatched surrogate
                return UnicodeReplacementChar;
            }
        }

        private static int HexToInt(char h)
        {
            return (h >= '0' && h <= '9') ? h - '0' :
            (h >= 'a' && h <= 'f') ? h - 'a' + 10 :
            (h >= 'A' && h <= 'F') ? h - 'A' + 10 :
            -1;
        }

        private static char IntToHex(int n)
        {
            Debug.Assert(n < 0x10);

            if (n <= 9)
                return (char)(n + (int)'0');
            else
                return (char)(n - 10 + (int)'A');
        }

        // Set of safe chars, from RFC 1738.4 minus '+'
        private static bool IsUrlSafeChar(char ch)
        {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        private static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if (bytes == null && count == 0)
                return false;
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            return true;
        }

        private static bool StringRequiresHtmlDecoding(string s) {
            if (HtmlDecodeConformance == UnicodeDecodingConformance.Compat) {
                // this string requires html decoding only if it contains '&'
                return (s.IndexOf('&') >= 0);
            }
            else {
                // this string requires html decoding if it contains '&' or a surrogate character
                for (int i = 0; i < s.Length; i++) {
                    char c = s[i];
                    if (c == '&' || Char.IsSurrogate(c)) {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region UrlDecoder nested class

        // *** Source: alm/tfs_core/Framework/Common/UriUtility/HttpUtility.cs
        // This specific code was copied from above ASP.NET codebase.

        // Internal class to facilitate URL decoding -- keeps char buffer and byte buffer, allows appending of either chars or bytes
        private class UrlDecoder
        {
            private int _bufferSize;

            // Accumulate characters in a special array
            private int _numChars;
            private char[] _charBuffer;

            // Accumulate bytes for decoding into characters in a special array
            private int _numBytes;
            private byte[] _byteBuffer;

            // Encoding to convert chars to bytes
            private Encoding _encoding;

            private void FlushBytes()
            {
                if (_numBytes > 0)
                {
                    _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                    _numBytes = 0;
                }
            }

            internal UrlDecoder(int bufferSize, Encoding encoding)
            {
                _bufferSize = bufferSize;
                _encoding = encoding;

                _charBuffer = new char[bufferSize];
                // byte buffer created on demand
            }

            internal void AddChar(char ch)
            {
                if (_numBytes > 0)
                    FlushBytes();

                _charBuffer[_numChars++] = ch;
            }

            internal void AddByte(byte b)
            {
                if (_byteBuffer == null)
                    _byteBuffer = new byte[_bufferSize];

                _byteBuffer[_numBytes++] = b;
            }

            internal String GetString()
            {
                if (_numBytes > 0)
                    FlushBytes();

                if (_numChars > 0)
                    return new String(_charBuffer, 0, _numChars);
                else
                    return String.Empty;
            }
        }

        #endregion

        #region HtmlEntities nested class

        // helper class for lookup of HTML encoding entities
        private static class HtmlEntities {

#if MONO
            public static char Lookup (string entity)
            {
                var token = CalculateKeyValue (entity);
                if (token == 0) {
                    return '\0';
                }

                var idx = Array.BinarySearch (entities, token);
                if (idx < 0) {
                    return '\0';
                }

                return entities_values [idx];
            }

            static long CalculateKeyValue (string s)
            {
                if (s.Length > 8)
                    return 0;

                long key = 0;
                for (int i = 0; i < s.Length; ++i) {
                    long ch = s[i];
                    if (ch > 'z' || ch < '0')
                        return 0;

                    key |= ch << ((7 - i) * 8);
                }

                return key;
            }

            // Must be sorted
            static readonly long[] entities = new long[] {
                (long)'A' << 56 | (long)'E' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24,
                (long)'A' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'A' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'A' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'A' << 56 | (long)'l' << 48 | (long)'p' << 40 | (long)'h' << 32 | (long)'a' << 24,
                (long)'A' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'g' << 24,
                (long)'A' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'A' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'B' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32,
                (long)'C' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'d' << 32 | (long)'i' << 24 | (long)'l' << 16,
                (long)'C' << 56 | (long)'h' << 48 | (long)'i' << 40,
                (long)'D' << 56 | (long)'a' << 48 | (long)'g' << 40 | (long)'g' << 32 | (long)'e' << 24 | (long)'r' << 16,
                (long)'D' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'t' << 32 | (long)'a' << 24,
                (long)'E' << 56 | (long)'T' << 48 | (long)'H' << 40,
                (long)'E' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'E' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'E' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'E' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8,
                (long)'E' << 56 | (long)'t' << 48 | (long)'a' << 40,
                (long)'E' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'G' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'m' << 32 | (long)'a' << 24,
                (long)'I' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'I' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'I' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'I' << 56 | (long)'o' << 48 | (long)'t' << 40 | (long)'a' << 32,
                (long)'I' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'K' << 56 | (long)'a' << 48 | (long)'p' << 40 | (long)'p' << 32 | (long)'a' << 24,
                (long)'L' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'b' << 32 | (long)'d' << 24 | (long)'a' << 16,
                (long)'M' << 56 | (long)'u' << 48,
                (long)'N' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'N' << 56 | (long)'u' << 48,
                (long)'O' << 56 | (long)'E' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24,
                (long)'O' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'O' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'O' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'O' << 56 | (long)'m' << 48 | (long)'e' << 40 | (long)'g' << 32 | (long)'a' << 24,
                (long)'O' << 56 | (long)'m' << 48 | (long)'i' << 40 | (long)'c' << 32 | (long)'r' << 24 | (long)'o' << 16 | (long)'n' << 8,
                (long)'O' << 56 | (long)'s' << 48 | (long)'l' << 40 | (long)'a' << 32 | (long)'s' << 24 | (long)'h' << 16,
                (long)'O' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'O' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'P' << 56 | (long)'h' << 48 | (long)'i' << 40,
                (long)'P' << 56 | (long)'i' << 48,
                (long)'P' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'m' << 32 | (long)'e' << 24,
                (long)'P' << 56 | (long)'s' << 48 | (long)'i' << 40,
                (long)'R' << 56 | (long)'h' << 48 | (long)'o' << 40,
                (long)'S' << 56 | (long)'c' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'o' << 24 | (long)'n' << 16,
                (long)'S' << 56 | (long)'i' << 48 | (long)'g' << 40 | (long)'m' << 32 | (long)'a' << 24,
                (long)'T' << 56 | (long)'H' << 48 | (long)'O' << 40 | (long)'R' << 32 | (long)'N' << 24,
                (long)'T' << 56 | (long)'a' << 48 | (long)'u' << 40,
                (long)'T' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'t' << 32 | (long)'a' << 24,
                (long)'U' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'U' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'U' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'U' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8,
                (long)'U' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'X' << 56 | (long)'i' << 48,
                (long)'Y' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'Y' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'Z' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32,
                (long)'a' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'a' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'a' << 56 | (long)'c' << 48 | (long)'u' << 40 | (long)'t' << 32 | (long)'e' << 24,
                (long)'a' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24,
                (long)'a' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'a' << 56 | (long)'l' << 48 | (long)'e' << 40 | (long)'f' << 32 | (long)'s' << 24 | (long)'y' << 16 | (long)'m' << 8,
                (long)'a' << 56 | (long)'l' << 48 | (long)'p' << 40 | (long)'h' << 32 | (long)'a' << 24,
                (long)'a' << 56 | (long)'m' << 48 | (long)'p' << 40,
                (long)'a' << 56 | (long)'n' << 48 | (long)'d' << 40,
                (long)'a' << 56 | (long)'n' << 48 | (long)'g' << 40,
                (long)'a' << 56 | (long)'p' << 48 | (long)'o' << 40 | (long)'s' << 32,
                (long)'a' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'g' << 24,
                (long)'a' << 56 | (long)'s' << 48 | (long)'y' << 40 | (long)'m' << 32 | (long)'p' << 24,
                (long)'a' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'a' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'b' << 56 | (long)'d' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'b' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32,
                (long)'b' << 56 | (long)'r' << 48 | (long)'v' << 40 | (long)'b' << 32 | (long)'a' << 24 | (long)'r' << 16,
                (long)'b' << 56 | (long)'u' << 48 | (long)'l' << 40 | (long)'l' << 32,
                (long)'c' << 56 | (long)'a' << 48 | (long)'p' << 40,
                (long)'c' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'d' << 32 | (long)'i' << 24 | (long)'l' << 16,
                (long)'c' << 56 | (long)'e' << 48 | (long)'d' << 40 | (long)'i' << 32 | (long)'l' << 24,
                (long)'c' << 56 | (long)'e' << 48 | (long)'n' << 40 | (long)'t' << 32,
                (long)'c' << 56 | (long)'h' << 48 | (long)'i' << 40,
                (long)'c' << 56 | (long)'i' << 48 | (long)'r' << 40 | (long)'c' << 32,
                (long)'c' << 56 | (long)'l' << 48 | (long)'u' << 40 | (long)'b' << 32 | (long)'s' << 24,
                (long)'c' << 56 | (long)'o' << 48 | (long)'n' << 40 | (long)'g' << 32,
                (long)'c' << 56 | (long)'o' << 48 | (long)'p' << 40 | (long)'y' << 32,
                (long)'c' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'r' << 24,
                (long)'c' << 56 | (long)'u' << 48 | (long)'p' << 40,
                (long)'c' << 56 | (long)'u' << 48 | (long)'r' << 40 | (long)'r' << 32 | (long)'e' << 24 | (long)'n' << 16,
                (long)'d' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'d' << 56 | (long)'a' << 48 | (long)'g' << 40 | (long)'g' << 32 | (long)'e' << 24 | (long)'r' << 16,
                (long)'d' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'d' << 56 | (long)'e' << 48 | (long)'g' << 40,
                (long)'d' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'t' << 32 | (long)'a' << 24,
                (long)'d' << 56 | (long)'i' << 48 | (long)'a' << 40 | (long)'m' << 32 | (long)'s' << 24,
                (long)'d' << 56 | (long)'i' << 48 | (long)'v' << 40 | (long)'i' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'e' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'e' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'e' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'e' << 56 | (long)'m' << 48 | (long)'p' << 40 | (long)'t' << 32 | (long)'y' << 24,
                (long)'e' << 56 | (long)'m' << 48 | (long)'s' << 40 | (long)'p' << 32,
                (long)'e' << 56 | (long)'n' << 48 | (long)'s' << 40 | (long)'p' << 32,
                (long)'e' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8,
                (long)'e' << 56 | (long)'q' << 48 | (long)'u' << 40 | (long)'i' << 32 | (long)'v' << 24,
                (long)'e' << 56 | (long)'t' << 48 | (long)'a' << 40,
                (long)'e' << 56 | (long)'t' << 48 | (long)'h' << 40,
                (long)'e' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'e' << 56 | (long)'u' << 48 | (long)'r' << 40 | (long)'o' << 32,
                (long)'e' << 56 | (long)'x' << 48 | (long)'i' << 40 | (long)'s' << 32 | (long)'t' << 24,
                (long)'f' << 56 | (long)'n' << 48 | (long)'o' << 40 | (long)'f' << 32,
                (long)'f' << 56 | (long)'o' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'l' << 24 | (long)'l' << 16,
                (long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'c' << 32 | (long)'1' << 24 | (long)'2' << 16,
                (long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'c' << 32 | (long)'1' << 24 | (long)'4' << 16,
                (long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'c' << 32 | (long)'3' << 24 | (long)'4' << 16,
                (long)'f' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'s' << 32 | (long)'l' << 24,
                (long)'g' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'m' << 32 | (long)'a' << 24,
                (long)'g' << 56 | (long)'e' << 48,
                (long)'g' << 56 | (long)'t' << 48,
                (long)'h' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'h' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'h' << 56 | (long)'e' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'t' << 24 | (long)'s' << 16,
                (long)'h' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'l' << 32 | (long)'i' << 24 | (long)'p' << 16,
                (long)'i' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'i' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'i' << 56 | (long)'e' << 48 | (long)'x' << 40 | (long)'c' << 32 | (long)'l' << 24,
                (long)'i' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'i' << 56 | (long)'m' << 48 | (long)'a' << 40 | (long)'g' << 32 | (long)'e' << 24,
                (long)'i' << 56 | (long)'n' << 48 | (long)'f' << 40 | (long)'i' << 32 | (long)'n' << 24,
                (long)'i' << 56 | (long)'n' << 48 | (long)'t' << 40,
                (long)'i' << 56 | (long)'o' << 48 | (long)'t' << 40 | (long)'a' << 32,
                (long)'i' << 56 | (long)'q' << 48 | (long)'u' << 40 | (long)'e' << 32 | (long)'s' << 24 | (long)'t' << 16,
                (long)'i' << 56 | (long)'s' << 48 | (long)'i' << 40 | (long)'n' << 32,
                (long)'i' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'k' << 56 | (long)'a' << 48 | (long)'p' << 40 | (long)'p' << 32 | (long)'a' << 24,
                (long)'l' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'l' << 56 | (long)'a' << 48 | (long)'m' << 40 | (long)'b' << 32 | (long)'d' << 24 | (long)'a' << 16,
                (long)'l' << 56 | (long)'a' << 48 | (long)'n' << 40 | (long)'g' << 32,
                (long)'l' << 56 | (long)'a' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'l' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'l' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'i' << 32 | (long)'l' << 24,
                (long)'l' << 56 | (long)'d' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'l' << 56 | (long)'e' << 48,
                (long)'l' << 56 | (long)'f' << 48 | (long)'l' << 40 | (long)'o' << 32 | (long)'o' << 24 | (long)'r' << 16,
                (long)'l' << 56 | (long)'o' << 48 | (long)'w' << 40 | (long)'a' << 32 | (long)'s' << 24 | (long)'t' << 16,
                (long)'l' << 56 | (long)'o' << 48 | (long)'z' << 40,
                (long)'l' << 56 | (long)'r' << 48 | (long)'m' << 40,
                (long)'l' << 56 | (long)'s' << 48 | (long)'a' << 40 | (long)'q' << 32 | (long)'u' << 24 | (long)'o' << 16,
                (long)'l' << 56 | (long)'s' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'l' << 56 | (long)'t' << 48,
                (long)'m' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'r' << 32,
                (long)'m' << 56 | (long)'d' << 48 | (long)'a' << 40 | (long)'s' << 32 | (long)'h' << 24,
                (long)'m' << 56 | (long)'i' << 48 | (long)'c' << 40 | (long)'r' << 32 | (long)'o' << 24,
                (long)'m' << 56 | (long)'i' << 48 | (long)'d' << 40 | (long)'d' << 32 | (long)'o' << 24 | (long)'t' << 16,
                (long)'m' << 56 | (long)'i' << 48 | (long)'n' << 40 | (long)'u' << 32 | (long)'s' << 24,
                (long)'m' << 56 | (long)'u' << 48,
                (long)'n' << 56 | (long)'a' << 48 | (long)'b' << 40 | (long)'l' << 32 | (long)'a' << 24,
                (long)'n' << 56 | (long)'b' << 48 | (long)'s' << 40 | (long)'p' << 32,
                (long)'n' << 56 | (long)'d' << 48 | (long)'a' << 40 | (long)'s' << 32 | (long)'h' << 24,
                (long)'n' << 56 | (long)'e' << 48,
                (long)'n' << 56 | (long)'i' << 48,
                (long)'n' << 56 | (long)'o' << 48 | (long)'t' << 40,
                (long)'n' << 56 | (long)'o' << 48 | (long)'t' << 40 | (long)'i' << 32 | (long)'n' << 24,
                (long)'n' << 56 | (long)'s' << 48 | (long)'u' << 40 | (long)'b' << 32,
                (long)'n' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'n' << 56 | (long)'u' << 48,
                (long)'o' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'o' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'o' << 56 | (long)'e' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24,
                (long)'o' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'o' << 56 | (long)'l' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'e' << 24,
                (long)'o' << 56 | (long)'m' << 48 | (long)'e' << 40 | (long)'g' << 32 | (long)'a' << 24,
                (long)'o' << 56 | (long)'m' << 48 | (long)'i' << 40 | (long)'c' << 32 | (long)'r' << 24 | (long)'o' << 16 | (long)'n' << 8,
                (long)'o' << 56 | (long)'p' << 48 | (long)'l' << 40 | (long)'u' << 32 | (long)'s' << 24,
                (long)'o' << 56 | (long)'r' << 48,
                (long)'o' << 56 | (long)'r' << 48 | (long)'d' << 40 | (long)'f' << 32,
                (long)'o' << 56 | (long)'r' << 48 | (long)'d' << 40 | (long)'m' << 32,
                (long)'o' << 56 | (long)'s' << 48 | (long)'l' << 40 | (long)'a' << 32 | (long)'s' << 24 | (long)'h' << 16,
                (long)'o' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'l' << 32 | (long)'d' << 24 | (long)'e' << 16,
                (long)'o' << 56 | (long)'t' << 48 | (long)'i' << 40 | (long)'m' << 32 | (long)'e' << 24 | (long)'s' << 16,
                (long)'o' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'p' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'a' << 32,
                (long)'p' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'t' << 32,
                (long)'p' << 56 | (long)'e' << 48 | (long)'r' << 40 | (long)'m' << 32 | (long)'i' << 24 | (long)'l' << 16,
                (long)'p' << 56 | (long)'e' << 48 | (long)'r' << 40 | (long)'p' << 32,
                (long)'p' << 56 | (long)'h' << 48 | (long)'i' << 40,
                (long)'p' << 56 | (long)'i' << 48,
                (long)'p' << 56 | (long)'i' << 48 | (long)'v' << 40,
                (long)'p' << 56 | (long)'l' << 48 | (long)'u' << 40 | (long)'s' << 32 | (long)'m' << 24 | (long)'n' << 16,
                (long)'p' << 56 | (long)'o' << 48 | (long)'u' << 40 | (long)'n' << 32 | (long)'d' << 24,
                (long)'p' << 56 | (long)'r' << 48 | (long)'i' << 40 | (long)'m' << 32 | (long)'e' << 24,
                (long)'p' << 56 | (long)'r' << 48 | (long)'o' << 40 | (long)'d' << 32,
                (long)'p' << 56 | (long)'r' << 48 | (long)'o' << 40 | (long)'p' << 32,
                (long)'p' << 56 | (long)'s' << 48 | (long)'i' << 40,
                (long)'q' << 56 | (long)'u' << 48 | (long)'o' << 40 | (long)'t' << 32,
                (long)'r' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'r' << 56 | (long)'a' << 48 | (long)'d' << 40 | (long)'i' << 32 | (long)'c' << 24,
                (long)'r' << 56 | (long)'a' << 48 | (long)'n' << 40 | (long)'g' << 32,
                (long)'r' << 56 | (long)'a' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'r' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'r' << 56 | (long)'c' << 48 | (long)'e' << 40 | (long)'i' << 32 | (long)'l' << 24,
                (long)'r' << 56 | (long)'d' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'r' << 56 | (long)'e' << 48 | (long)'a' << 40 | (long)'l' << 32,
                (long)'r' << 56 | (long)'e' << 48 | (long)'g' << 40,
                (long)'r' << 56 | (long)'f' << 48 | (long)'l' << 40 | (long)'o' << 32 | (long)'o' << 24 | (long)'r' << 16,
                (long)'r' << 56 | (long)'h' << 48 | (long)'o' << 40,
                (long)'r' << 56 | (long)'l' << 48 | (long)'m' << 40,
                (long)'r' << 56 | (long)'s' << 48 | (long)'a' << 40 | (long)'q' << 32 | (long)'u' << 24 | (long)'o' << 16,
                (long)'r' << 56 | (long)'s' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'s' << 56 | (long)'b' << 48 | (long)'q' << 40 | (long)'u' << 32 | (long)'o' << 24,
                (long)'s' << 56 | (long)'c' << 48 | (long)'a' << 40 | (long)'r' << 32 | (long)'o' << 24 | (long)'n' << 16,
                (long)'s' << 56 | (long)'d' << 48 | (long)'o' << 40 | (long)'t' << 32,
                (long)'s' << 56 | (long)'e' << 48 | (long)'c' << 40 | (long)'t' << 32,
                (long)'s' << 56 | (long)'h' << 48 | (long)'y' << 40,
                (long)'s' << 56 | (long)'i' << 48 | (long)'g' << 40 | (long)'m' << 32 | (long)'a' << 24,
                (long)'s' << 56 | (long)'i' << 48 | (long)'g' << 40 | (long)'m' << 32 | (long)'a' << 24 | (long)'f' << 16,
                (long)'s' << 56 | (long)'i' << 48 | (long)'m' << 40,
                (long)'s' << 56 | (long)'p' << 48 | (long)'a' << 40 | (long)'d' << 32 | (long)'e' << 24 | (long)'s' << 16,
                (long)'s' << 56 | (long)'u' << 48 | (long)'b' << 40,
                (long)'s' << 56 | (long)'u' << 48 | (long)'b' << 40 | (long)'e' << 32,
                (long)'s' << 56 | (long)'u' << 48 | (long)'m' << 40,
                (long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40,
                (long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'1' << 32,
                (long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'2' << 32,
                (long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'3' << 32,
                (long)'s' << 56 | (long)'u' << 48 | (long)'p' << 40 | (long)'e' << 32,
                (long)'s' << 56 | (long)'z' << 48 | (long)'l' << 40 | (long)'i' << 32 | (long)'g' << 24,
                (long)'t' << 56 | (long)'a' << 48 | (long)'u' << 40,
                (long)'t' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'r' << 32 | (long)'e' << 24 | (long)'4' << 16,
                (long)'t' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'t' << 32 | (long)'a' << 24,
                (long)'t' << 56 | (long)'h' << 48 | (long)'e' << 40 | (long)'t' << 32 | (long)'a' << 24 | (long)'s' << 16 | (long)'y' << 8 | (long)'m' << 0,
                (long)'t' << 56 | (long)'h' << 48 | (long)'i' << 40 | (long)'n' << 32 | (long)'s' << 24 | (long)'p' << 16,
                (long)'t' << 56 | (long)'h' << 48 | (long)'o' << 40 | (long)'r' << 32 | (long)'n' << 24,
                (long)'t' << 56 | (long)'i' << 48 | (long)'l' << 40 | (long)'d' << 32 | (long)'e' << 24,
                (long)'t' << 56 | (long)'i' << 48 | (long)'m' << 40 | (long)'e' << 32 | (long)'s' << 24,
                (long)'t' << 56 | (long)'r' << 48 | (long)'a' << 40 | (long)'d' << 32 | (long)'e' << 24,
                (long)'u' << 56 | (long)'A' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'u' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'u' << 56 | (long)'a' << 48 | (long)'r' << 40 | (long)'r' << 32,
                (long)'u' << 56 | (long)'c' << 48 | (long)'i' << 40 | (long)'r' << 32 | (long)'c' << 24,
                (long)'u' << 56 | (long)'g' << 48 | (long)'r' << 40 | (long)'a' << 32 | (long)'v' << 24 | (long)'e' << 16,
                (long)'u' << 56 | (long)'m' << 48 | (long)'l' << 40,
                (long)'u' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'h' << 24,
                (long)'u' << 56 | (long)'p' << 48 | (long)'s' << 40 | (long)'i' << 32 | (long)'l' << 24 | (long)'o' << 16 | (long)'n' << 8,
                (long)'u' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'w' << 56 | (long)'e' << 48 | (long)'i' << 40 | (long)'e' << 32 | (long)'r' << 24 | (long)'p' << 16,
                (long)'x' << 56 | (long)'i' << 48,
                (long)'y' << 56 | (long)'a' << 48 | (long)'c' << 40 | (long)'u' << 32 | (long)'t' << 24 | (long)'e' << 16,
                (long)'y' << 56 | (long)'e' << 48 | (long)'n' << 40,
                (long)'y' << 56 | (long)'u' << 48 | (long)'m' << 40 | (long)'l' << 32,
                (long)'z' << 56 | (long)'e' << 48 | (long)'t' << 40 | (long)'a' << 32,
                (long)'z' << 56 | (long)'w' << 48 | (long)'j' << 40,
                (long)'z' << 56 | (long)'w' << 48 | (long)'n' << 40 | (long)'j' << 32
            };

            static readonly char[] entities_values = new char[] {
                '\u00C6',
                '\u00C1',
                '\u00C2',
                '\u00C0',
                '\u0391',
                '\u00C5',
                '\u00C3',
                '\u00C4',
                '\u0392',
                '\u00C7',
                '\u03A7',
                '\u2021',
                '\u0394',
                '\u00D0',
                '\u00C9',
                '\u00CA',
                '\u00C8',
                '\u0395',
                '\u0397',
                '\u00CB',
                '\u0393',
                '\u00CD',
                '\u00CE',
                '\u00CC',
                '\u0399',
                '\u00CF',
                '\u039A',
                '\u039B',
                '\u039C',
                '\u00D1',
                '\u039D',
                '\u0152',
                '\u00D3',
                '\u00D4',
                '\u00D2',
                '\u03A9',
                '\u039F',
                '\u00D8',
                '\u00D5',
                '\u00D6',
                '\u03A6',
                '\u03A0',
                '\u2033',
                '\u03A8',
                '\u03A1',
                '\u0160',
                '\u03A3',
                '\u00DE',
                '\u03A4',
                '\u0398',
                '\u00DA',
                '\u00DB',
                '\u00D9',
                '\u03A5',
                '\u00DC',
                '\u039E',
                '\u00DD',
                '\u0178',
                '\u0396',
                '\u00E1',
                '\u00E2',
                '\u00B4',
                '\u00E6',
                '\u00E0',
                '\u2135',
                '\u03B1',
                '\u0026',
                '\u2227',
                '\u2220',
                '\u0027',
                '\u00E5',
                '\u2248',
                '\u00E3',
                '\u00E4',
                '\u201E',
                '\u03B2',
                '\u00A6',
                '\u2022',
                '\u2229',
                '\u00E7',
                '\u00B8',
                '\u00A2',
                '\u03C7',
                '\u02C6',
                '\u2663',
                '\u2245',
                '\u00A9',
                '\u21B5',
                '\u222A',
                '\u00A4',
                '\u21D3',
                '\u2020',
                '\u2193',
                '\u00B0',
                '\u03B4',
                '\u2666',
                '\u00F7',
                '\u00E9',
                '\u00EA',
                '\u00E8',
                '\u2205',
                '\u2003',
                '\u2002',
                '\u03B5',
                '\u2261',
                '\u03B7',
                '\u00F0',
                '\u00EB',
                '\u20AC',
                '\u2203',
                '\u0192',
                '\u2200',
                '\u00BD',
                '\u00BC',
                '\u00BE',
                '\u2044',
                '\u03B3',
                '\u2265',
                '\u003E',
                '\u21D4',
                '\u2194',
                '\u2665',
                '\u2026',
                '\u00ED',
                '\u00EE',
                '\u00A1',
                '\u00EC',
                '\u2111',
                '\u221E',
                '\u222B',
                '\u03B9',
                '\u00BF',
                '\u2208',
                '\u00EF',
                '\u03BA',
                '\u21D0',
                '\u03BB',
                '\u2329',
                '\u00AB',
                '\u2190',
                '\u2308',
                '\u201C',
                '\u2264',
                '\u230A',
                '\u2217',
                '\u25CA',
                '\u200E',
                '\u2039',
                '\u2018',
                '\u003C',
                '\u00AF',
                '\u2014',
                '\u00B5',
                '\u00B7',
                '\u2212',
                '\u03BC',
                '\u2207',
                '\u00A0',
                '\u2013',
                '\u2260',
                '\u220B',
                '\u00AC',
                '\u2209',
                '\u2284',
                '\u00F1',
                '\u03BD',
                '\u00F3',
                '\u00F4',
                '\u0153',
                '\u00F2',
                '\u203E',
                '\u03C9',
                '\u03BF',
                '\u2295',
                '\u2228',
                '\u00AA',
                '\u00BA',
                '\u00F8',
                '\u00F5',
                '\u2297',
                '\u00F6',
                '\u00B6',
                '\u2202',
                '\u2030',
                '\u22A5',
                '\u03C6',
                '\u03C0',
                '\u03D6',
                '\u00B1',
                '\u00A3',
                '\u2032',
                '\u220F',
                '\u221D',
                '\u03C8',
                '\u0022',
                '\u21D2',
                '\u221A',
                '\u232A',
                '\u00BB',
                '\u2192',
                '\u2309',
                '\u201D',
                '\u211C',
                '\u00AE',
                '\u230B',
                '\u03C1',
                '\u200F',
                '\u203A',
                '\u2019',
                '\u201A',
                '\u0161',
                '\u22C5',
                '\u00A7',
                '\u00AD',
                '\u03C3',
                '\u03C2',
                '\u223C',
                '\u2660',
                '\u2282',
                '\u2286',
                '\u2211',
                '\u2283',
                '\u00B9',
                '\u00B2',
                '\u00B3',
                '\u2287',
                '\u00DF',
                '\u03C4',
                '\u2234',
                '\u03B8',
                '\u03D1',
                '\u2009',
                '\u00FE',
                '\u02DC',
                '\u00D7',
                '\u2122',
                '\u21D1',
                '\u00FA',
                '\u2191',
                '\u00FB',
                '\u00F9',
                '\u00A8',
                '\u03D2',
                '\u03C5',
                '\u00FC',
                '\u2118',
                '\u03BE',
                '\u00FD',
                '\u00A5',
                '\u00FF',
                '\u03B6',
                '\u200D',
                '\u200C'
            };
#else
            // The list is from http://www.w3.org/TR/REC-html40/sgml/entities.html, except for &apos;, which
            // is defined in http://www.w3.org/TR/2008/REC-xml-20081126/#sec-predefined-ent.

            private static String[] _entitiesList = new String[] {
                "\x0022-quot",
                "\x0026-amp",
                "\x0027-apos",
                "\x003c-lt",
                "\x003e-gt",
                "\x00a0-nbsp",
                "\x00a1-iexcl",
                "\x00a2-cent",
                "\x00a3-pound",
                "\x00a4-curren",
                "\x00a5-yen",
                "\x00a6-brvbar",
                "\x00a7-sect",
                "\x00a8-uml",
                "\x00a9-copy",
                "\x00aa-ordf",
                "\x00ab-laquo",
                "\x00ac-not",
                "\x00ad-shy",
                "\x00ae-reg",
                "\x00af-macr",
                "\x00b0-deg",
                "\x00b1-plusmn",
                "\x00b2-sup2",
                "\x00b3-sup3",
                "\x00b4-acute",
                "\x00b5-micro",
                "\x00b6-para",
                "\x00b7-middot",
                "\x00b8-cedil",
                "\x00b9-sup1",
                "\x00ba-ordm",
                "\x00bb-raquo",
                "\x00bc-frac14",
                "\x00bd-frac12",
                "\x00be-frac34",
                "\x00bf-iquest",
                "\x00c0-Agrave",
                "\x00c1-Aacute",
                "\x00c2-Acirc",
                "\x00c3-Atilde",
                "\x00c4-Auml",
                "\x00c5-Aring",
                "\x00c6-AElig",
                "\x00c7-Ccedil",
                "\x00c8-Egrave",
                "\x00c9-Eacute",
                "\x00ca-Ecirc",
                "\x00cb-Euml",
                "\x00cc-Igrave",
                "\x00cd-Iacute",
                "\x00ce-Icirc",
                "\x00cf-Iuml",
                "\x00d0-ETH",
                "\x00d1-Ntilde",
                "\x00d2-Ograve",
                "\x00d3-Oacute",
                "\x00d4-Ocirc",
                "\x00d5-Otilde",
                "\x00d6-Ouml",
                "\x00d7-times",
                "\x00d8-Oslash",
                "\x00d9-Ugrave",
                "\x00da-Uacute",
                "\x00db-Ucirc",
                "\x00dc-Uuml",
                "\x00dd-Yacute",
                "\x00de-THORN",
                "\x00df-szlig",
                "\x00e0-agrave",
                "\x00e1-aacute",
                "\x00e2-acirc",
                "\x00e3-atilde",
                "\x00e4-auml",
                "\x00e5-aring",
                "\x00e6-aelig",
                "\x00e7-ccedil",
                "\x00e8-egrave",
                "\x00e9-eacute",
                "\x00ea-ecirc",
                "\x00eb-euml",
                "\x00ec-igrave",
                "\x00ed-iacute",
                "\x00ee-icirc",
                "\x00ef-iuml",
                "\x00f0-eth",
                "\x00f1-ntilde",
                "\x00f2-ograve",
                "\x00f3-oacute",
                "\x00f4-ocirc",
                "\x00f5-otilde",
                "\x00f6-ouml",
                "\x00f7-divide",
                "\x00f8-oslash",
                "\x00f9-ugrave",
                "\x00fa-uacute",
                "\x00fb-ucirc",
                "\x00fc-uuml",
                "\x00fd-yacute",
                "\x00fe-thorn",
                "\x00ff-yuml",
                "\x0152-OElig",
                "\x0153-oelig",
                "\x0160-Scaron",
                "\x0161-scaron",
                "\x0178-Yuml",
                "\x0192-fnof",
                "\x02c6-circ",
                "\x02dc-tilde",
                "\x0391-Alpha",
                "\x0392-Beta",
                "\x0393-Gamma",
                "\x0394-Delta",
                "\x0395-Epsilon",
                "\x0396-Zeta",
                "\x0397-Eta",
                "\x0398-Theta",
                "\x0399-Iota",
                "\x039a-Kappa",
                "\x039b-Lambda",
                "\x039c-Mu",
                "\x039d-Nu",
                "\x039e-Xi",
                "\x039f-Omicron",
                "\x03a0-Pi",
                "\x03a1-Rho",
                "\x03a3-Sigma",
                "\x03a4-Tau",
                "\x03a5-Upsilon",
                "\x03a6-Phi",
                "\x03a7-Chi",
                "\x03a8-Psi",
                "\x03a9-Omega",
                "\x03b1-alpha",
                "\x03b2-beta",
                "\x03b3-gamma",
                "\x03b4-delta",
                "\x03b5-epsilon",
                "\x03b6-zeta",
                "\x03b7-eta",
                "\x03b8-theta",
                "\x03b9-iota",
                "\x03ba-kappa",
                "\x03bb-lambda",
                "\x03bc-mu",
                "\x03bd-nu",
                "\x03be-xi",
                "\x03bf-omicron",
                "\x03c0-pi",
                "\x03c1-rho",
                "\x03c2-sigmaf",
                "\x03c3-sigma",
                "\x03c4-tau",
                "\x03c5-upsilon",
                "\x03c6-phi",
                "\x03c7-chi",
                "\x03c8-psi",
                "\x03c9-omega",
                "\x03d1-thetasym",
                "\x03d2-upsih",
                "\x03d6-piv",
                "\x2002-ensp",
                "\x2003-emsp",
                "\x2009-thinsp",
                "\x200c-zwnj",
                "\x200d-zwj",
                "\x200e-lrm",
                "\x200f-rlm",
                "\x2013-ndash",
                "\x2014-mdash",
                "\x2018-lsquo",
                "\x2019-rsquo",
                "\x201a-sbquo",
                "\x201c-ldquo",
                "\x201d-rdquo",
                "\x201e-bdquo",
                "\x2020-dagger",
                "\x2021-Dagger",
                "\x2022-bull",
                "\x2026-hellip",
                "\x2030-permil",
                "\x2032-prime",
                "\x2033-Prime",
                "\x2039-lsaquo",
                "\x203a-rsaquo",
                "\x203e-oline",
                "\x2044-frasl",
                "\x20ac-euro",
                "\x2111-image",
                "\x2118-weierp",
                "\x211c-real",
                "\x2122-trade",
                "\x2135-alefsym",
                "\x2190-larr",
                "\x2191-uarr",
                "\x2192-rarr",
                "\x2193-darr",
                "\x2194-harr",
                "\x21b5-crarr",
                "\x21d0-lArr",
                "\x21d1-uArr",
                "\x21d2-rArr",
                "\x21d3-dArr",
                "\x21d4-hArr",
                "\x2200-forall",
                "\x2202-part",
                "\x2203-exist",
                "\x2205-empty",
                "\x2207-nabla",
                "\x2208-isin",
                "\x2209-notin",
                "\x220b-ni",
                "\x220f-prod",
                "\x2211-sum",
                "\x2212-minus",
                "\x2217-lowast",
                "\x221a-radic",
                "\x221d-prop",
                "\x221e-infin",
                "\x2220-ang",
                "\x2227-and",
                "\x2228-or",
                "\x2229-cap",
                "\x222a-cup",
                "\x222b-int",
                "\x2234-there4",
                "\x223c-sim",
                "\x2245-cong",
                "\x2248-asymp",
                "\x2260-ne",
                "\x2261-equiv",
                "\x2264-le",
                "\x2265-ge",
                "\x2282-sub",
                "\x2283-sup",
                "\x2284-nsub",
                "\x2286-sube",
                "\x2287-supe",
                "\x2295-oplus",
                "\x2297-otimes",
                "\x22a5-perp",
                "\x22c5-sdot",
                "\x2308-lceil",
                "\x2309-rceil",
                "\x230a-lfloor",
                "\x230b-rfloor",
                "\x2329-lang",
                "\x232a-rang",
                "\x25ca-loz",
                "\x2660-spades",
                "\x2663-clubs",
                "\x2665-hearts",
                "\x2666-diams",
            };

            private static Dictionary<string, char> _lookupTable = GenerateLookupTable();

            private static Dictionary<string, char> GenerateLookupTable() {
                // e[0] is unicode char, e[1] is '-', e[2+] is entity string

                Dictionary<string, char> lookupTable = new Dictionary<string, char>(StringComparer.Ordinal);
                foreach (string e in _entitiesList) {
                    lookupTable.Add(e.Substring(2), e[0]);
                }

                return lookupTable;
            }

            public static char Lookup(string entity) {
                char theChar;
                _lookupTable.TryGetValue(entity, out theChar);
                return theChar;
            }
#endif
        }

        #endregion
    }
}
