//------------------------------------------------------------------------------
// <copyright file="Scalars.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    using System.Web.Services;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System;
    using System.Text;
    using System.Threading;
    
    internal class ScalarFormatter {
        private ScalarFormatter() { }

        internal static string ToString(object value) {
            if (value == null) 
                return string.Empty;
            else if (value is string)
                return (string)value;
            else if (value.GetType().IsEnum)
                return EnumToString(value);
            else
                return (string)Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        internal static object FromString(string value, Type type) {
            try {
                if (type == typeof(string))
                    return value;
                else if (type.IsEnum)
                    return (object)EnumFromString(value, type);
                else
                    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                throw new ArgumentException(Res.GetString(Res.WebChangeTypeFailed, value, type.FullName), "type", e);
            }
        }

        static object EnumFromString(string value, Type type) {
            return Enum.Parse(type, value);
        }

        static string EnumToString(object value) {
            return Enum.Format(value.GetType(), value, "G");
        }

        // We support: whatever Convert supports, and Enums
        internal static bool IsTypeSupported(Type type) {
            if (type.IsEnum) return true;
            return (
                type == typeof(int) ||
                type == typeof(string) ||
                type == typeof(long) ||
                type == typeof(byte) ||
                type == typeof(sbyte) ||
                type == typeof(short) ||
                type == typeof(bool) ||
                type == typeof(char) ||
                type == typeof(float) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(UInt16) ||
                type == typeof(UInt32) ||
                type == typeof(UInt64) ||
                type == typeof(double));
        }
    }

    internal class UrlEncoder {
        private UrlEncoder() { }

        private const int Max16BitUtf8SequenceLength = 4;

        internal static string EscapeString(string s, Encoding e) {
            return EscapeStringInternal(s, e == null ? new ASCIIEncoding() : e, false);
        }

        internal static string UrlEscapeString(string s, Encoding e) {
            return EscapeStringInternal(s, e == null ? new ASCIIEncoding() : e, true);
        }

        private static string EscapeStringInternal(string s, Encoding e, bool escapeUriStuff) {
            if (s == null) return null;
            
            byte[] bytes = e.GetBytes(s);
            StringBuilder sb = new StringBuilder(bytes.Length);
            for (int i = 0; i < bytes.Length; i++) {
                byte b = bytes[i];
                char c = (char)b;
                if (b > 0x7f || b < 0x20 || c == '%' || (escapeUriStuff && !IsSafe(c))) {
                    HexEscape8(sb, c);
                }
                else {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /*
        // [....]: adapted from UrlEscapeStringUnicode below
        internal static string EscapeStringUnicode(string s) {
            int l = s.Length;
            StringBuilder sb = new StringBuilder(l);

            for (int i = 0; i < l; i++) {
                char ch = s[i];
                if ((ch & 0xff80) == 0) {
                    sb.Append(ch);
                }
                else {
                    HexEscape16(sb, ch);
                }
            }

            return sb.ToString();
        }
        */

        // [....]: copied from System.Web.HttpUtility
        internal static string UrlEscapeStringUnicode(string s) {
            int l = s.Length;
            StringBuilder sb = new StringBuilder(l);

            for (int i = 0; i < l; i++) {
                char ch = s[i];

                if (IsSafe(ch)) {
                    sb.Append(ch);
                }
                else if (ch == ' ') {
                    sb.Append('+');
                }
                else if ((ch & 0xff80) == 0) {  // 7 bit?
                    HexEscape8(sb, ch);
                }
                else { // arbitrary Unicode?
                    HexEscape16(sb, ch);
                }
            }
            return sb.ToString();
        }

        private static void HexEscape8(StringBuilder sb, char c) {
            sb.Append('%');
            sb.Append(HexUpperChars[(c >> 4) & 0xf]);
            sb.Append(HexUpperChars[(c) & 0xf]);
        }

        private static void HexEscape16(StringBuilder sb, char c) {
            sb.Append("%u");
            sb.Append(HexUpperChars[(c >> 12) & 0xf]);
            sb.Append(HexUpperChars[(c >> 8) & 0xf]);
            sb.Append(HexUpperChars[(c >> 4) & 0xf]);
            sb.Append(HexUpperChars[(c) & 0xf]);
        }

        private static bool IsSafe(char ch) {
            if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')
                return true;

            switch (ch) {
                case '-':
                case '_':
                case '.':
                case '!':
                case '*':
                case '\'':
                case '(':
                case ')':
                    return true;
            }

            return false;
        }

        internal static readonly char[] HexUpperChars = {
            '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };
    }

    internal class ContentType {
        private ContentType() { }
        internal const string TextBase = "text";
        internal const string TextXml     = "text/xml";
        internal const string TextPlain   = "text/plain";
        internal const string TextHtml    = "text/html";

        internal const string ApplicationBase = "application";
        internal const string ApplicationXml = "application/xml";
        internal const string ApplicationSoap = "application/soap+xml";
        internal const string ApplicationOctetStream = "application/octet-stream";

        internal const string ContentEncoding = "Content-Encoding";

        // this returns the "base" part of the contentType/mimeType, e.g. the "text/xml" part w/o
        // the ; CharSet=isoxxx part that sometimes follows.
        internal static string GetBase(string contentType) {
            int semi = contentType.IndexOf(';');
            if (semi >= 0) return contentType.Substring(0, semi);
            return contentType;
        }

        // this returns the "type" part of the contentType/mimeType without subtyape
        internal static string GetMediaType(string contentType) {
            string baseCT = GetBase(contentType);
            int tmp = baseCT.IndexOf('/');
            if (tmp >= 0) return baseCT.Substring(0, tmp);
            return baseCT;
        }

        // grabs as follows (and case-insensitive): .*;\s*charset\s*=\s*[\s'"]*(.*)[\s'"]*
        internal static string GetCharset(string contentType) {
            return GetParameter(contentType, "charset");
        }

        internal static string GetAction(string contentType) {
            return GetParameter(contentType, "action");
        }

        private static string GetParameter(string contentType, string paramName) {
            string[] paramDecls = contentType.Split(new char[] { ';' });
            for (int i = 1; i < paramDecls.Length; i++) {
                string paramDecl = paramDecls[i].TrimStart(null);
                if (String.Compare(paramDecl, 0, paramName, 0, paramName.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                    int equals = paramDecl.IndexOf('=', paramName.Length);
                    if (equals >= 0) 
                        return paramDecl.Substring(equals + 1).Trim(new char[] { ' ', '\'', '\"', '\t' });
                }
            }
            return null;
        }

        internal static bool MatchesBase(string contentType, string baseContentType) {
            return string.Compare(GetBase(contentType), baseContentType, StringComparison.OrdinalIgnoreCase) == 0;
        }

        internal static bool IsApplication(string contentType) {
            return string.Compare(GetMediaType(contentType), ApplicationBase, StringComparison.OrdinalIgnoreCase) == 0;
        }

        internal static bool IsSoap(string contentType) {
            string type = GetBase(contentType);
            return (String.Compare(type, ContentType.TextXml, StringComparison.OrdinalIgnoreCase) == 0) ||
                   (String.Compare(type, ContentType.ApplicationSoap, StringComparison.OrdinalIgnoreCase) == 0);
        }
        
        internal static bool IsXml(string contentType) {
            string type = GetBase(contentType);
            return (String.Compare(type, ContentType.TextXml, StringComparison.OrdinalIgnoreCase) == 0) ||
                   (String.Compare(type, ContentType.ApplicationXml, StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsHtml(string contentType) {
            string type = GetBase(contentType);
            return String.Compare(type, ContentType.TextHtml, StringComparison.OrdinalIgnoreCase) == 0;
        }

        internal static string Compose(string contentType, Encoding encoding) {
            return Compose(contentType, encoding, null);
        }

        internal static string Compose(string contentType, Encoding encoding, string action) {
            if (encoding == null && action == null) return contentType;

            StringBuilder sb = new StringBuilder(contentType);
            if (encoding != null) {
                sb.Append("; charset=");
                sb.Append(encoding.WebName);
            }
            if (action != null) {
                sb.Append("; action=\"");
                sb.Append(action);
                sb.Append("\"");
            }
            return sb.ToString();
        }
    }

    internal class MemberHelper {
        private MemberHelper() { }
        static object[] emptyObjectArray = new object[0];

        internal static void SetValue(MemberInfo memberInfo, object target, object value) {
            if (memberInfo is FieldInfo ) {
                ((FieldInfo)memberInfo).SetValue(target, value);
            }
            else {
                ((PropertyInfo)memberInfo).SetValue(target, value, emptyObjectArray);
            }
        }

        internal static object GetValue(MemberInfo memberInfo, object target) {
            if (memberInfo is FieldInfo) {
                return ((FieldInfo)memberInfo).GetValue(target);
            }
            else {
                return ((PropertyInfo)memberInfo).GetValue(target, emptyObjectArray);
            }
        }

        internal static bool IsStatic(MemberInfo memberInfo) {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).IsStatic;
            else
                return false;
        }

        internal static bool CanRead(MemberInfo memberInfo) {
            if (memberInfo is FieldInfo)
                return true;
            else
                return ((PropertyInfo)memberInfo).CanRead;
        }

        internal static bool CanWrite(MemberInfo memberInfo) {
            if (memberInfo is FieldInfo)
                return true;
            else
                return ((PropertyInfo)memberInfo).CanWrite;
        }
    }
}
