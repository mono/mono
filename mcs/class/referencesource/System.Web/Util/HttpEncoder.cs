//------------------------------------------------------------------------------
// <copyright file="HttpEncoder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Base class providing extensibility hooks for custom encoding / decoding
 *
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;

    public class HttpEncoder {

        private static HttpEncoder _customEncoder;
        private readonly bool _isDefaultEncoder;

        private static readonly Lazy<HttpEncoder> _customEncoderResolver =
            new Lazy<HttpEncoder>(GetCustomEncoderFromConfig);

        private static readonly HttpEncoder _defaultEncoder = new HttpEncoder();

        private static readonly string[] _headerEncodingTable = new string[] {
            "%00", "%01", "%02", "%03", "%04", "%05", "%06", "%07",
            "%08", "%09", "%0a", "%0b", "%0c", "%0d", "%0e", "%0f",
            "%10", "%11", "%12", "%13", "%14", "%15", "%16", "%17", 
            "%18", "%19", "%1a", "%1b", "%1c", "%1d", "%1e", "%1f"
        };

        public HttpEncoder() {
            _isDefaultEncoder = (GetType() == typeof(HttpEncoder));
        }

        public static HttpEncoder Current {
            get {
                // always use the fallback encoder when rendering an error page so that we can at least display *something*
                // to the user before closing the connection

                HttpContext httpContext = HttpContext.Current;
                if (httpContext != null && httpContext.DisableCustomHttpEncoder) {
                    return _defaultEncoder;
                }
                else {
                    if (_customEncoder == null) {
                        _customEncoder = _customEncoderResolver.Value;
                    }
                    return _customEncoder;
                }
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                _customEncoder = value;
            }
        }

        public static HttpEncoder Default {
            get {
                return _defaultEncoder;
            }
        }

        internal virtual bool JavaScriptEncodeAmpersand {
            get {
                return !AppSettings.JavaScriptDoNotEncodeAmpersand;
            }
        }

        private static void AppendCharAsUnicodeJavaScript(StringBuilder builder, char c) {
            builder.Append("\\u");
            builder.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
        }

        private bool CharRequiresJavaScriptEncoding(char c) {
            return c < 0x20 // control chars always have to be encoded
                || c == '\"' // chars which must be encoded per JSON spec
                || c == '\\'
                || c == '\'' // HTML-sensitive chars encoded for safety
                || c == '<'
                || c == '>'
                || (c == '&' && JavaScriptEncodeAmpersand) // Bug Dev11 #133237. Encode '&' to provide additional security for people who incorrectly call the encoding methods (unless turned off by backcompat switch)
                || c == '\u0085' // newline chars (see Unicode 6.2, Table 5-1 [http://www.unicode.org/versions/Unicode6.2.0/ch05.pdf]) have to be encoded (DevDiv #663531)
                || c == '\u2028'
                || c == '\u2029';
        }

        internal static string CollapsePercentUFromStringInternal(string s, Encoding e) {
            int count = s.Length;
            UrlDecoder helper = new UrlDecoder(count, e);

            // go thorugh the string's chars collapsing just %uXXXX and
            // appending each char as char
            int loc = s.IndexOf("%u", StringComparison.Ordinal);
            if (loc == -1) {
                return s;
            }

            for (int pos = 0; pos < count; pos++) {
                char ch = s[pos];

                if (ch == '%' && pos < count - 5) {
                    if (s[pos + 1] == 'u') {
                        int h1 = HttpEncoderUtility.HexToInt(s[pos + 2]);
                        int h2 = HttpEncoderUtility.HexToInt(s[pos + 3]);
                        int h3 = HttpEncoderUtility.HexToInt(s[pos + 4]);
                        int h4 = HttpEncoderUtility.HexToInt(s[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0) { //valid 4 hex chars
                            ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            pos += 5;

                            // add as char
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                }
                if ((ch & 0xFF80) == 0)
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                else
                    helper.AddChar(ch);
            }
            return Utf16StringValidator.ValidateString(helper.GetString());
        }

        private static HttpEncoder GetCustomEncoderFromConfig() {
            // App since this is static per AppDomain
            RuntimeConfig config = RuntimeConfig.GetAppConfig();
            HttpRuntimeSection runtimeSection = config.HttpRuntime;
            string encoderTypeName = runtimeSection.EncoderType;

            // validate the type
            Type encoderType = ConfigUtil.GetType(encoderTypeName, "encoderType", runtimeSection);
            ConfigUtil.CheckBaseType(typeof(HttpEncoder) /* expectedBaseType */, encoderType, "encoderType", runtimeSection);

            // instantiate
            HttpEncoder encoder = (HttpEncoder)HttpRuntime.CreatePublicInstance(encoderType);
            return encoder;
        }

        // Encode the header if it contains a CRLF pair
        // VSWhidbey 257154
        private static string HeaderEncodeInternal(string value) {
            string sanitizedHeader = value;
            if (HeaderValueNeedsEncoding(value)) {
                // DevDiv Bugs 146028
                // Denial Of Service scenarios involving 
                // control characters are possible.
                // We are encoding the following characters:
                // - All CTL characters except HT (horizontal tab)
                // - DEL character (\x7f)
                StringBuilder sb = new StringBuilder();
                foreach (char c in value) {
                    if (c < 32 && c != 9) {
                        sb.Append(_headerEncodingTable[c]);
                    }
                    else if (c == 127) {
                        sb.Append("%7f");
                    }
                    else {
                        sb.Append(c);
                    }
                }
                sanitizedHeader = sb.ToString();
            }

            return sanitizedHeader;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
                 Justification = "Input parameter strings are immutable, so this is an appropriate way to return multiple strings.")]
        protected internal virtual void HeaderNameValueEncode(string headerName, string headerValue, out string encodedHeaderName, out string encodedHeaderValue) {
            encodedHeaderName = (String.IsNullOrEmpty(headerName)) ? headerName : HeaderEncodeInternal(headerName);
            encodedHeaderValue = (String.IsNullOrEmpty(headerValue)) ? headerValue : HeaderEncodeInternal(headerValue);
        }

        // Returns true if the string contains a control character (other than horizontal tab) or the DEL character.
        private static bool HeaderValueNeedsEncoding(string value) {
            foreach (char c in value) {
                if ((c < 32 && c != 9) || (c == 127)) {
                    return true;
                }
            }
            return false;
        }

        internal string HtmlAttributeEncode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return value;
            }

            if(_isDefaultEncoder) {
                // Don't create string writer if we don't have nothing to encode
                int pos = IndexOfHtmlAttributeEncodingChars(value, 0);
                if (pos == -1) {
                    return value;
                }
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlAttributeEncode(value, writer);
            return writer.ToString();
        }

        protected internal virtual void HtmlAttributeEncode(string value, TextWriter output) {
            if (value == null) {
                return;
            }
            if (output == null) {
                throw new ArgumentNullException("output");
            }

            // we have a special faster path for HttpWriter
            HttpWriter httpWriter = output as HttpWriter;
            if (httpWriter != null) {
                HtmlAttributeEncodeInternal(value, httpWriter);
            }
            else {
                HtmlAttributeEncodeInternal(value, output);
            }
        }

        private static void HtmlAttributeEncodeInternal(string value, HttpWriter writer) {
            int pos = IndexOfHtmlAttributeEncodingChars(value, 0);
            if (pos == -1) {
                writer.Write(value);
                return;
            }

            int cch = value.Length;
            int startPos = 0;
            for (; ; ) {
                if (pos > startPos) {
                    writer.WriteString(value, startPos, pos - startPos);
                }

                char ch = value[pos];
                switch (ch) {
                    case '"':
                        writer.Write("&quot;");
                        break;
                    case '\'':
                        writer.Write("&#39;");
                        break;
                    case '&':
                        writer.Write("&amp;");
                        break;
                    case '<':
                        // Whidbey 32404: The character '<' is not valid in an XML attribute value.
                        // (See the W3C XML rec).
                        writer.Write("&lt;");
                        break;
                }

                startPos = pos + 1;
                if (startPos >= cch)
                    break;

                pos = IndexOfHtmlAttributeEncodingChars(value, startPos);
                if (pos == -1) {
                    writer.WriteString(value, startPos, cch - startPos);
                    break;
                }
            }
        }

        private unsafe static void HtmlAttributeEncodeInternal(String s, TextWriter output) {
            int index = IndexOfHtmlAttributeEncodingChars(s, 0);
            if (index == -1) {
                output.Write(s);
            }
            else {
                int cch = s.Length - index;
                fixed (char* str = s) {
                    char* pch = str;
                    while (index-- > 0) {
                        output.Write(*pch++);
                    }

                    while (cch-- > 0) {
                        char ch = *pch++;
                        if (ch <= '<') {
                            switch (ch) {
                                case '<':
                                    output.Write("&lt;");
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
                            output.Write(ch);
                        }
                    }
                }
            }
        }

        internal string HtmlDecode(string value) {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            if(_isDefaultEncoder) {
                return WebUtility.HtmlDecode(value);
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlDecode(value, writer);
            return writer.ToString();
        }

        protected internal virtual void HtmlDecode(string value, TextWriter output) {
            WebUtility.HtmlDecode(value, output);
        }

        internal string HtmlEncode(string value) {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            if(_isDefaultEncoder) {
                return WebUtility.HtmlEncode(value);
            }

            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            HtmlEncode(value, writer);
            return writer.ToString();
        }

        protected internal virtual void HtmlEncode(string value, TextWriter output) {
            WebUtility.HtmlEncode(value, output);
        }

        private static unsafe int IndexOfHtmlAttributeEncodingChars(string s, int startPos) {
            Debug.Assert(0 <= startPos && startPos <= s.Length, "0 <= startPos && startPos <= s.Length");
            int cch = s.Length - startPos;
            fixed (char* str = s) {
                for (char* pch = &str[startPos]; cch > 0; pch++, cch--) {
                    char ch = *pch;
                    if (ch <= '<') {
                        switch (ch) {
                            case '<':
                            case '"':
                            case '\'':
                            case '&':
                                return s.Length - cch;
                        }
                    }
                }
            }

            return -1;
        }

        internal static void InitializeOnFirstRequest() {
            // Instantiate the encoder if it hasn't already been created. Note that this isn't storing the returned encoder
            // anywhere; it's just priming the Lazy<T> so that future calls to the Value property getter will return quickly
            // without going back to config.

            HttpEncoder encoder = _customEncoderResolver.Value;
        }

        private static bool IsNonAsciiByte(byte b) {
            return (b >= 0x7F || b < 0x20);
        }

        protected internal virtual string JavaScriptStringEncode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return String.Empty;
            }

            StringBuilder b = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < value.Length; i++) {
                char c = value[i];

                // Append the unhandled characters (that do not require special treament)
                // to the string builder when special characters are detected.
                if (CharRequiresJavaScriptEncoding(c)) {
                    if (b == null) {
                        b = new StringBuilder(value.Length + 5);
                    }

                    if (count > 0) {
                        b.Append(value, startIndex, count);
                    }

                    startIndex = i + 1;
                    count = 0;
                }

                switch (c) {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    case '\b':
                        b.Append("\\b");
                        break;
                    case '\f':
                        b.Append("\\f");
                        break;
                    default:
                        if (CharRequiresJavaScriptEncoding(c)) {
                            AppendCharAsUnicodeJavaScript(b, c);
                        }
                        else {
                            count++;
                        }
                        break;
                }
            }

            if (b == null) {
                return value;
            }

            if (count > 0) {
                b.Append(value, startIndex, count);
            }

            return b.ToString();
        }
        
        internal byte[] UrlDecode(byte[] bytes, int offset, int count) {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            int decodedBytesCount = 0;
            byte[] decodedBytes = new byte[count];

            for (int i = 0; i < count; i++) {
                int pos = offset + i;
                byte b = bytes[pos];

                if (b == '+') {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2) {
                    int h1 = HttpEncoderUtility.HexToInt((char)bytes[pos + 1]);
                    int h2 = HttpEncoderUtility.HexToInt((char)bytes[pos + 2]);

                    if (h1 >= 0 && h2 >= 0) {     // valid 2 hex chars
                        b = (byte)((h1 << 4) | h2);
                        i += 2;
                    }
                }

                decodedBytes[decodedBytesCount++] = b;
            }

            if (decodedBytesCount < decodedBytes.Length) {
                byte[] newDecodedBytes = new byte[decodedBytesCount];
                Array.Copy(decodedBytes, newDecodedBytes, decodedBytesCount);
                decodedBytes = newDecodedBytes;
            }

            return decodedBytes;
        }
        
        internal string UrlDecode(byte[] bytes, int offset, int count, Encoding encoding) {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the bytes collapsing %XX and %uXXXX and appending
            // each byte as byte, with exception of %uXXXX constructs that
            // are appended as chars

            for (int i = 0; i < count; i++) {
                int pos = offset + i;
                byte b = bytes[pos];

                // The code assumes that + and % cannot be in multibyte sequence

                if (b == '+') {
                    b = (byte)' ';
                }
                else if (b == '%' && i < count - 2) {
                    if (bytes[pos + 1] == 'u' && i < count - 5) {
                        int h1 = HttpEncoderUtility.HexToInt((char)bytes[pos + 2]);
                        int h2 = HttpEncoderUtility.HexToInt((char)bytes[pos + 3]);
                        int h3 = HttpEncoderUtility.HexToInt((char)bytes[pos + 4]);
                        int h4 = HttpEncoderUtility.HexToInt((char)bytes[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0) {   // valid 4 hex chars
                            char ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            i += 5;

                            // don't add as byte
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else {
                        int h1 = HttpEncoderUtility.HexToInt((char)bytes[pos + 1]);
                        int h2 = HttpEncoderUtility.HexToInt((char)bytes[pos + 2]);

                        if (h1 >= 0 && h2 >= 0) {     // valid 2 hex chars
                            b = (byte)((h1 << 4) | h2);
                            i += 2;
                        }
                    }
                }

                helper.AddByte(b);
            }

            return Utf16StringValidator.ValidateString(helper.GetString());
        }
        
        internal string UrlDecode(string value, Encoding encoding) {
            if (value == null) {
                return null;
            }

            int count = value.Length;
            UrlDecoder helper = new UrlDecoder(count, encoding);

            // go through the string's chars collapsing %XX and %uXXXX and
            // appending each char as char, with exception of %XX constructs
            // that are appended as bytes

            for (int pos = 0; pos < count; pos++) {
                char ch = value[pos];

                if (ch == '+') {
                    ch = ' ';
                }
                else if (ch == '%' && pos < count - 2) {
                    if (value[pos + 1] == 'u' && pos < count - 5) {
                        int h1 = HttpEncoderUtility.HexToInt(value[pos + 2]);
                        int h2 = HttpEncoderUtility.HexToInt(value[pos + 3]);
                        int h3 = HttpEncoderUtility.HexToInt(value[pos + 4]);
                        int h4 = HttpEncoderUtility.HexToInt(value[pos + 5]);

                        if (h1 >= 0 && h2 >= 0 && h3 >= 0 && h4 >= 0) {   // valid 4 hex chars
                            ch = (char)((h1 << 12) | (h2 << 8) | (h3 << 4) | h4);
                            pos += 5;

                            // only add as char
                            helper.AddChar(ch);
                            continue;
                        }
                    }
                    else {
                        int h1 = HttpEncoderUtility.HexToInt(value[pos + 1]);
                        int h2 = HttpEncoderUtility.HexToInt(value[pos + 2]);

                        if (h1 >= 0 && h2 >= 0) {     // valid 2 hex chars
                            byte b = (byte)((h1 << 4) | h2);
                            pos += 2;

                            // don't add as char
                            helper.AddByte(b);
                            continue;
                        }
                    }
                }

                if ((ch & 0xFF80) == 0)
                    helper.AddByte((byte)ch); // 7 bit have to go as bytes because of Unicode
                else
                    helper.AddChar(ch);
            }

            return Utf16StringValidator.ValidateString(helper.GetString());
        }

        internal byte[] UrlEncode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue) {
            byte[] encoded = UrlEncode(bytes, offset, count);

            return (alwaysCreateNewReturnValue && (encoded != null) && (encoded == bytes))
                ? (byte[])encoded.Clone()
                : encoded;
        }

        protected internal virtual byte[] UrlEncode(byte[] bytes, int offset, int count) {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            int cSpaces = 0;
            int cUnsafe = 0;

            // count them first
            for (int i = 0; i < count; i++) {
                char ch = (char)bytes[offset + i];

                if (ch == ' ')
                    cSpaces++;
                else if (!HttpEncoderUtility.IsUrlSafeChar(ch))
                    cUnsafe++;
            }

            // nothing to expand?
            if (cSpaces == 0 && cUnsafe == 0)
                return bytes;

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cUnsafe * 2];
            int pos = 0;

            for (int i = 0; i < count; i++) {
                byte b = bytes[offset + i];
                char ch = (char)b;

                if (HttpEncoderUtility.IsUrlSafeChar(ch)) {
                    expandedBytes[pos++] = b;
                }
                else if (ch == ' ') {
                    expandedBytes[pos++] = (byte)'+';
                }
                else {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex(b & 0x0f);
                }
            }

            return expandedBytes;
        }

        //  Helper to encode the non-ASCII url characters only
        internal String UrlEncodeNonAscii(string str, Encoding e) {
            if (String.IsNullOrEmpty(str))
                return str;
            if (e == null)
                e = Encoding.UTF8;
            byte[] bytes = e.GetBytes(str);
            byte[] encodedBytes = UrlEncodeNonAscii(bytes, 0, bytes.Length, false /* alwaysCreateNewReturnValue */);
            return Encoding.ASCII.GetString(encodedBytes);
        }

        internal byte[] UrlEncodeNonAscii(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue) {
            if (!ValidateUrlEncodingParameters(bytes, offset, count)) {
                return null;
            }

            int cNonAscii = 0;

            // count them first
            for (int i = 0; i < count; i++) {
                if (IsNonAsciiByte(bytes[offset + i]))
                    cNonAscii++;
            }

            // nothing to expand?
            if (!alwaysCreateNewReturnValue && cNonAscii == 0)
                return bytes;

            // expand not 'safe' characters into %XX, spaces to +s
            byte[] expandedBytes = new byte[count + cNonAscii * 2];
            int pos = 0;

            for (int i = 0; i < count; i++) {
                byte b = bytes[offset + i];

                if (IsNonAsciiByte(b)) {
                    expandedBytes[pos++] = (byte)'%';
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex((b >> 4) & 0xf);
                    expandedBytes[pos++] = (byte)HttpEncoderUtility.IntToHex(b & 0x0f);
                }
                else {
                    expandedBytes[pos++] = b;
                }
            }

            return expandedBytes;
        }

        [Obsolete("This method produces non-standards-compliant output and has interoperability issues. The preferred alternative is UrlEncode(*).")]
        internal string UrlEncodeUnicode(string value, bool ignoreAscii) {
            if (value == null) {
                return null;
            }

            int l = value.Length;
            StringBuilder sb = new StringBuilder(l);

            for (int i = 0; i < l; i++) {
                char ch = value[i];

                if ((ch & 0xff80) == 0) {  // 7 bit?
                    if (ignoreAscii || HttpEncoderUtility.IsUrlSafeChar(ch)) {
                        sb.Append(ch);
                    }
                    else if (ch == ' ') {
                        sb.Append('+');
                    }
                    else {
                        sb.Append('%');
                        sb.Append(HttpEncoderUtility.IntToHex((ch >> 4) & 0xf));
                        sb.Append(HttpEncoderUtility.IntToHex((ch) & 0xf));
                    }
                }
                else { // arbitrary Unicode?
                    sb.Append("%u");
                    sb.Append(HttpEncoderUtility.IntToHex((ch >> 12) & 0xf));
                    sb.Append(HttpEncoderUtility.IntToHex((ch >> 8) & 0xf));
                    sb.Append(HttpEncoderUtility.IntToHex((ch >> 4) & 0xf));
                    sb.Append(HttpEncoderUtility.IntToHex((ch) & 0xf));
                }
            }

            return sb.ToString();
        }

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification = "Does not represent an entire URL, just a portion.")]
        protected internal virtual string UrlPathEncode(string value) {
            if (String.IsNullOrEmpty(value)) {
                return value;
            }

            // recurse in case there is a query string
            int i = value.IndexOf('?');
            if (i >= 0)
                return UrlPathEncode(value.Substring(0, i)) + value.Substring(i);

            // encode DBCS characters and spaces only
            return HttpEncoderUtility.UrlEncodeSpaces(UrlEncodeNonAscii(value, Encoding.UTF8));
        }

        internal byte[] UrlTokenDecode(string input) {
            if (input == null)
                throw new ArgumentNullException("input");

            int len = input.Length;
            if (len < 1)
                return new byte[0];

            ///////////////////////////////////////////////////////////////////
            // Step 1: Calculate the number of padding chars to append to this string.
            //         The number of padding chars to append is stored in the last char of the string.
            int numPadChars = (int)input[len - 1] - (int)'0';
            if (numPadChars < 0 || numPadChars > 10)
                return null;


            ///////////////////////////////////////////////////////////////////
            // Step 2: Create array to store the chars (not including the last char)
            //          and the padding chars
            char[] base64Chars = new char[len - 1 + numPadChars];


            ////////////////////////////////////////////////////////
            // Step 3: Copy in the chars. Transform the "-" to "+", and "*" to "/"
            for (int iter = 0; iter < len - 1; iter++) {
                char c = input[iter];

                switch (c) {
                    case '-':
                        base64Chars[iter] = '+';
                        break;

                    case '_':
                        base64Chars[iter] = '/';
                        break;

                    default:
                        base64Chars[iter] = c;
                        break;
                }
            }

            ////////////////////////////////////////////////////////
            // Step 4: Add padding chars
            for (int iter = len - 1; iter < base64Chars.Length; iter++) {
                base64Chars[iter] = '=';
            }

            // Do the actual conversion
            return Convert.FromBase64CharArray(base64Chars, 0, base64Chars.Length);
        }
       
        internal string UrlTokenEncode(byte[] input) {
            if (input == null)
                throw new ArgumentNullException("input");
            if (input.Length < 1)
                return String.Empty;

            string base64Str = null;
            int endPos = 0;
            char[] base64Chars = null;

            ////////////////////////////////////////////////////////
            // Step 1: Do a Base64 encoding
            base64Str = Convert.ToBase64String(input);
            if (base64Str == null)
                return null;

            ////////////////////////////////////////////////////////
            // Step 2: Find how many padding chars are present in the end
            for (endPos = base64Str.Length; endPos > 0; endPos--) {
                if (base64Str[endPos - 1] != '=') // Found a non-padding char!
                {
                    break; // Stop here
                }
            }

            ////////////////////////////////////////////////////////
            // Step 3: Create char array to store all non-padding chars,
            //      plus a char to indicate how many padding chars are needed
            base64Chars = new char[endPos + 1];
            base64Chars[endPos] = (char)((int)'0' + base64Str.Length - endPos); // Store a char at the end, to indicate how many padding chars are needed

            ////////////////////////////////////////////////////////
            // Step 3: Copy in the other chars. Transform the "+" to "-", and "/" to "_"
            for (int iter = 0; iter < endPos; iter++) {
                char c = base64Str[iter];

                switch (c) {
                    case '+':
                        base64Chars[iter] = '-';
                        break;

                    case '/':
                        base64Chars[iter] = '_';
                        break;

                    case '=':
                        Debug.Assert(false);
                        base64Chars[iter] = c;
                        break;

                    default:
                        base64Chars[iter] = c;
                        break;
                }
            }
            return new string(base64Chars);
        }

        internal static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count) {
            if (bytes == null && count == 0)
                return false;
            if (bytes == null) {
                throw new ArgumentNullException("bytes");
            }
            if (offset < 0 || offset > bytes.Length) {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count < 0 || offset + count > bytes.Length) {
                throw new ArgumentOutOfRangeException("count");
            }

            return true;
        }

        // Internal class to facilitate URL decoding -- keeps char buffer and byte buffer, allows appending of either chars or bytes
        private class UrlDecoder {
            private int _bufferSize;

            // Accumulate characters in a special array
            private int _numChars;
            private char[] _charBuffer;

            // Accumulate bytes for decoding into characters in a special array
            private int _numBytes;
            private byte[] _byteBuffer;

            // Encoding to convert chars to bytes
            private Encoding _encoding;

            private void FlushBytes() {
                if (_numBytes > 0) {
                    _numChars += _encoding.GetChars(_byteBuffer, 0, _numBytes, _charBuffer, _numChars);
                    _numBytes = 0;
                }
            }

            internal UrlDecoder(int bufferSize, Encoding encoding) {
                _bufferSize = bufferSize;
                _encoding = encoding;

                _charBuffer = new char[bufferSize];
                // byte buffer created on demand
            }

            internal void AddChar(char ch) {
                if (_numBytes > 0)
                    FlushBytes();

                _charBuffer[_numChars++] = ch;
            }

            internal void AddByte(byte b) {
                // if there are no pending bytes treat 7 bit bytes as characters
                // this optimization is temp disable as it doesn't work for some encodings
                /*
                                if (_numBytes == 0 && ((b & 0x80) == 0)) {
                                    AddChar((char)b);
                                }
                                else
                */
                {
                    if (_byteBuffer == null)
                        _byteBuffer = new byte[_bufferSize];

                    _byteBuffer[_numBytes++] = b;
                }
            }

            internal String GetString() {
                if (_numBytes > 0)
                    FlushBytes();

                if (_numChars > 0)
                    return new String(_charBuffer, 0, _numChars);
                else
                    return String.Empty;
            }
        }

    }
}
