/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.Scripting.Utils {
    public static class StringUtils {

        public static Encoding DefaultEncoding {
            get {
#if FEATURE_ENCODING
                return Encoding.Default;
#else
                return Encoding.UTF8;
#endif
            }
        }

        public static string GetSuffix(string str, char separator, bool includeSeparator) {
            ContractUtils.RequiresNotNull(str, "str");
            int last = str.LastIndexOf(separator);
            return (last != -1) ? str.Substring(includeSeparator ? last : last + 1) : null;
        }

        public static string GetLongestPrefix(string str, char separator, bool includeSeparator) {
            ContractUtils.RequiresNotNull(str, "str");
            int last = str.LastIndexOf(separator);
            return (last != -1) ? str.Substring(0, (includeSeparator || last == 0) ? last : last - 1) : null;
        }

        public static int CountOf(string str, char c) {
            if (System.String.IsNullOrEmpty(str)) return 0;

            int result = 0;
            for (int i = 0; i < str.Length; i++) {
                if (c == str[i]) {
                    result++;
                }
            }
            return result;
        }

        public static string[] Split(string str, string separator, int maxComponents, StringSplitOptions options) {
            ContractUtils.RequiresNotNull(str, "str");
#if SILVERLIGHT || WP75
            if (string.IsNullOrEmpty(separator)) throw new ArgumentNullException("separator");

            bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

            List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

            int i = 0;
            int next;
            while (maxComponents > 1 && i < str.Length && (next = str.IndexOf(separator, i)) != -1) {

                if (next > i || keep_empty) {
                    result.Add(str.Substring(i, next - i));
                    maxComponents--;
                }

                i = next + separator.Length;
            }

            if (i < str.Length || keep_empty) {
                result.Add(str.Substring(i));
            }

            return result.ToArray();
#else
            return str.Split(new string[] { separator }, maxComponents, options);
#endif
        }

        public static string[] Split(string str, char[] separators, int maxComponents, StringSplitOptions options) {
            ContractUtils.RequiresNotNull(str, "str");
#if SILVERLIGHT || WP75
            if (separators == null) return SplitOnWhiteSpace(str, maxComponents, options);

            bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

            List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

            int i = 0;
            int next;
            while (maxComponents > 1 && i < str.Length && (next = str.IndexOfAny(separators, i)) != -1) {

                if (next > i || keep_empty) {
                    result.Add(str.Substring(i, next - i));
                    maxComponents--;
                }

                i = next + 1;
            }

            if (i < str.Length || keep_empty) {
                result.Add(str.Substring(i));
            }

            return result.ToArray();
#else
            return str.Split(separators, maxComponents, options);
#endif
        }

#if SILVERLIGHT|| WP75
        public static string[] SplitOnWhiteSpace(string str, int maxComponents, StringSplitOptions options) {
            ContractUtils.RequiresNotNull(str, "str");

            bool keep_empty = (options & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries;

            List<string> result = new List<string>(maxComponents == Int32.MaxValue ? 1 : maxComponents + 1);

            int i = 0;
            int next;
            while (maxComponents > 1 && i < str.Length && (next = IndexOfWhiteSpace(str, i)) != -1) {

                if (next > i || keep_empty) {
                    result.Add(str.Substring(i, next - i));
                    maxComponents--;
                }

                i = next + 1;
            }

            if (i < str.Length || keep_empty) {
                result.Add(str.Substring(i));
            }

            return result.ToArray();
        }

        public static int IndexOfWhiteSpace(string str, int start) {
            ContractUtils.RequiresNotNull(str, "str");
            if (start < 0 || start > str.Length) throw new ArgumentOutOfRangeException("start");

            while (start < str.Length && !Char.IsWhiteSpace(str[start])) start++;

            return (start == str.Length) ? -1 : start;
        }
#endif

        /// <summary>
        /// Splits text and optionally indents first lines - breaks along words, not characters.
        /// </summary>
        public static string SplitWords(string text, bool indentFirst, int lineWidth) {
            ContractUtils.RequiresNotNull(text, "text");

            const string indent = "    ";

            if (text.Length <= lineWidth || lineWidth <= 0) {
                if (indentFirst) return indent + text;
                return text;
            }

            StringBuilder res = new StringBuilder();
            int start = 0, len = lineWidth;
            while (start != text.Length) {
                if (len >= lineWidth) {
                    // find last space to break on
                    while (len != 0 && !Char.IsWhiteSpace(text[start + len - 1]))
                        len--;
                }

                if (res.Length != 0) res.Append(' ');
                if (indentFirst || res.Length != 0) res.Append(indent);

                if (len == 0) {
                    int copying = System.Math.Min(lineWidth, text.Length - start);
                    res.Append(text, start, copying);
                    start += copying;
                } else {
                    res.Append(text, start, len);
                    start += len;
                }
                res.AppendLine();
                len = System.Math.Min(lineWidth, text.Length - start);
            }
            return res.ToString();
        }

        public static string AddSlashes(string str) {
            ContractUtils.RequiresNotNull(str, "str");

            // TODO: optimize
            StringBuilder result = new StringBuilder(str.Length);
            for (int i = 0; i < str.Length; i++) {
                switch (str[i]) {
                    case '\a': result.Append("\\a"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    case '\v': result.Append("\\v"); break;
                    default: result.Append(str[i]); break;
                }
            }

            return result.ToString();
        }

        public static bool TryParseDouble(string s, NumberStyles style, IFormatProvider provider, out double result) {
            return Double.TryParse(s, style, provider, out result);
        }

        public static bool TryParseInt32(string s, out int result) {
            return Int32.TryParse(s, out result);
        }

        public static bool TryParseDateTimeExact(string s, string format, IFormatProvider provider, DateTimeStyles style, out DateTime result) {
            return DateTime.TryParseExact(s, format, provider, style, out result);
        }

        public static bool TryParseDateTimeExact(string s, string[] formats, IFormatProvider provider, DateTimeStyles style, out DateTime result) {
            return DateTime.TryParseExact(s, formats, provider, style, out result);
        }

        public static bool TryParseDate(string s, IFormatProvider provider, DateTimeStyles style, out DateTime result) {
            return DateTime.TryParse(s, provider, style, out result);
        }

#if !WIN8
#if SILVERLIGHT || WP75
        private static Dictionary<string, CultureInfo> _cultureInfoCache = new Dictionary<string, CultureInfo>();
#endif

        // Aims to be equivalent to Culture.GetCultureInfo for Silverlight
        public static CultureInfo GetCultureInfo(string name) {
#if SILVERLIGHT || WP75
            lock (_cultureInfoCache) {
                CultureInfo result;
                if (_cultureInfoCache.TryGetValue(name, out result)) {
                    return result;
                }
                _cultureInfoCache[name] = result = new CultureInfo(name);
                return result;
            }
#else
            return CultureInfo.GetCultureInfo(name);
#endif
        }
#endif
        // Like string.Split, but enumerates
        public static IEnumerable<string> Split(string str, string sep) {
            int start = 0, end;
            while ((end = str.IndexOf(sep, start)) != -1) {
                yield return str.Substring(start, end - start);

                start = end + sep.Length;
            }
            yield return str.Substring(start);
        }
    }
}
