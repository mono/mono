//------------------------------------------------------------------------------
// <copyright file="JavaScriptObjectDeserializer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Script.Serialization {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Resources;
    
    using AppSettings = System.Web.Util.AppSettings;
    using Debug = System.Web.Util.Debug;
    using Utf16StringValidator = System.Web.Util.Utf16StringValidator;

    internal class JavaScriptObjectDeserializer {
        private const string DateTimePrefix = @"""\/Date(";
        private const int DateTimePrefixLength = 8;

        private const string DateTimeSuffix = @"\/""";
        private const int DateTimeSuffixLength = 3;

        internal JavaScriptString _s;
        private JavaScriptSerializer _serializer;
        private int _depthLimit;

        internal static object BasicDeserialize(string input, int depthLimit, JavaScriptSerializer serializer) {
            JavaScriptObjectDeserializer jsod = new JavaScriptObjectDeserializer(input, depthLimit, serializer);
            object result = jsod.DeserializeInternal(0);
            if (jsod._s.GetNextNonEmptyChar() != null) {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_IllegalPrimitive, jsod._s.ToString()));
            }
            return result;
        }

        private JavaScriptObjectDeserializer(string input, int depthLimit, JavaScriptSerializer serializer) {
            _s = new JavaScriptString(input);
            _depthLimit = depthLimit;
            _serializer = serializer;
        }

        private object DeserializeInternal(int depth) {
            if (++depth > _depthLimit) {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_DepthLimitExceeded));
            }

            Nullable<Char> c = _s.GetNextNonEmptyChar();
            if (c == null) {
                return null;
            }

            _s.MovePrev();

            if (IsNextElementDateTime()) {
                return DeserializeStringIntoDateTime();
            }

            if (IsNextElementObject(c)) {
                IDictionary<string, object> dict = DeserializeDictionary(depth);
                // Try to coerce objects to the right type if they have the __serverType
                if (dict.ContainsKey(JavaScriptSerializer.ServerTypeFieldName)) {
                    return ObjectConverter.ConvertObjectToType(dict, null, _serializer);
                }
                return dict;
            }

            if (IsNextElementArray(c)) {
                return DeserializeList(depth);
            }

            if (IsNextElementString(c)) {
                return DeserializeString();
            }

            return DeserializePrimitiveObject();
        }

        private IList DeserializeList(int depth) {
            IList list = new ArrayList();
            Nullable<Char> c = _s.MoveNext();
            if (c != '[') {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidArrayStart));
            }

            bool expectMore = false;
            while ((c = _s.GetNextNonEmptyChar()) != null && c != ']') {
                _s.MovePrev();
                object o = DeserializeInternal(depth);
                list.Add(o);

                expectMore = false;
                // we might be done here.
                c = _s.GetNextNonEmptyChar();
                if (c == ']') {
                    break;
                }

                expectMore = true;
                if (c != ',') {
                    throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidArrayExpectComma));
                }
            }
            if (expectMore) {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidArrayExtraComma));
            }
            if (c != ']') {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidArrayEnd));
            }
            return list;
        }

        private IDictionary<string, object> DeserializeDictionary(int depth) {
            IDictionary<string, object> dictionary = null;
            Nullable<Char> c = _s.MoveNext();
            if (c != '{') {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_ExpectedOpenBrace));
            }

            // Loop through each JSON entry in the input object
            while ((c = _s.GetNextNonEmptyChar()) != null) {
                _s.MovePrev();

                if (c == ':') {
                    throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidMemberName));
                }

                string memberName = null;
                if (c != '}') {
                    // Find the member name
                    memberName = DeserializeMemberName();
                    c = _s.GetNextNonEmptyChar();
                    if (c != ':') {
                        throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidObject));
                    }
                }

                if (dictionary == null) {
                    dictionary = new Dictionary<string, object>();

                    // If the object contains nothing (i.e. {}), we're done
                    if (memberName == null) {
                        // Move the cursor to the '}' character.
                        c = _s.GetNextNonEmptyChar();
                        Debug.Assert(c == '}');
                        break;
                    }
                }

                ThrowIfMaxJsonDeserializerMembersExceeded(dictionary.Count);

                // Deserialize the property value.  Here, we don't know its type
                object propVal = DeserializeInternal(depth);
                dictionary[memberName] = propVal;
                c = _s.GetNextNonEmptyChar();
                if (c == '}') {
                    break;
                }

                if (c != ',') {
                    throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidObject));
                }
            }

            if (c != '}') {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_InvalidObject));
            }

            return dictionary;
        }

        // MSRC 12038: limit the maximum number of entries that can be added to a Json deserialized dictionary,
        // as a large number of entries potentially can result in too many hash collisions that may cause DoS
        private void ThrowIfMaxJsonDeserializerMembersExceeded(int count) {
            if (count >= AppSettings.MaxJsonDeserializerMembers) {
                throw new InvalidOperationException(SR.GetString(SR.CollectionCountExceeded_JavaScriptObjectDeserializer, AppSettings.MaxJsonDeserializerMembers));
            }
        }

        // Deserialize a member name.
        // e.g. { MemberName: ... }
        // e.g. { 'MemberName': ... }
        // e.g. { "MemberName": ... }
        private string DeserializeMemberName() {

            // It could be double quoted, single quoted, or not quoted at all
            Nullable<Char> c = _s.GetNextNonEmptyChar();
            if (c == null) {
                return null;
            }

            _s.MovePrev();

            // If it's quoted, treat it as a string
            if (IsNextElementString(c)) {
                return DeserializeString();
            }

            // Non-quoted token
            return DeserializePrimitiveToken();
        }

        private object DeserializePrimitiveObject() {
            string input = DeserializePrimitiveToken();
            if (input.Equals("null")) {
                return null;
            }

            if (input.Equals("true")) {
                return true;
            }

            if (input.Equals("false")) {
                return false;
            }

            // Is it a floating point value
            bool hasDecimalPoint = input.IndexOf('.') >= 0;
            // DevDiv 56892: don't try to parse to Int32/64/Decimal if it has an exponent sign
            bool hasExponent = input.LastIndexOf("e", StringComparison.OrdinalIgnoreCase) >= 0;
            // [Last]IndexOf(char, StringComparison) overload doesn't exist, so search for "e" as a string not a char
            // Use 'Last'IndexOf since if there is an exponent it would be more quickly found starting from the end of the string
            // since 'e' is always toward the end of the number. e.g. 1.238907598768972987E82

            if (!hasExponent) {
                // when no exponent, could be Int32, Int64, Decimal, and may fall back to Double
                // otherwise it must be Double

                if (!hasDecimalPoint) {
                    // No decimal or exponent. All Int32 and Int64s fall into this category, so try them first
                    // First try int
                    int n;
                    if (Int32.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out n)) {
                        // NumberStyles.Integer: AllowLeadingWhite, AllowTrailingWhite, AllowLeadingSign
                        return n;
                    }

                    // Then try a long
                    long l;
                    if (Int64.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out l)) {
                        // NumberStyles.Integer: AllowLeadingWhite, AllowTrailingWhite, AllowLeadingSign
                        return l;
                    }
                }

                // No exponent, may or may not have a decimal (if it doesn't it couldn't be parsed into Int32/64)
                decimal dec;
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out dec)) {
                    // NumberStyles.Number: AllowLeadingWhite, AllowTrailingWhite, AllowLeadingSign,
                    //                      AllowTrailingSign, AllowDecimalPoint, AllowThousands
                    return dec;
                }
            }

            // either we have an exponent or the number couldn't be parsed into any previous type. 
            Double d;
            if (Double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) {
                // NumberStyles.Float: AllowLeadingWhite, AllowTrailingWhite, AllowLeadingSign, AllowDecimalPoint, AllowExponent
                return d;
            }

            // must be an illegal primitive
            throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.JSON_IllegalPrimitive, input));
        }

        private string DeserializePrimitiveToken() {
            StringBuilder sb = new StringBuilder();
            Nullable<Char> c = null;
            while ((c = _s.MoveNext()) != null) {
                if (Char.IsLetterOrDigit(c.Value) || c.Value == '.' ||
                    c.Value == '-' || c.Value == '_' || c.Value == '+') {

                    sb.Append(c.Value);
                }
                else {
                    _s.MovePrev();
                    break;
                }
            }

            return sb.ToString();
        }

        private string DeserializeString() {
            StringBuilder sb = new StringBuilder();
            bool escapedChar = false;

            Nullable<Char> c = _s.MoveNext();

            // First determine which quote is used by the string.
            Char quoteChar = CheckQuoteChar(c);
            while ((c = _s.MoveNext()) != null) {
                if (c == '\\') {
                    if (escapedChar) {
                        sb.Append('\\');
                        escapedChar = false;
                    }
                    else {
                        escapedChar = true;
                    }

                    continue;
                }

                if (escapedChar) {
                    AppendCharToBuilder(c, sb);
                    escapedChar = false;
                }
                else {
                    if (c == quoteChar) {
                        return Utf16StringValidator.ValidateString(sb.ToString());
                    }

                    sb.Append(c.Value);
                }
            }

            throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_UnterminatedString));
        }

        private void AppendCharToBuilder(char? c, StringBuilder sb) {
            if (c == '"' || c == '\'' || c == '/') {
                sb.Append(c.Value);
            }
            else if (c == 'b') {
                sb.Append('\b');
            }
            else if (c == 'f') {
                sb.Append('\f');
            }
            else if (c == 'n') {
                sb.Append('\n');
            }
            else if (c == 'r') {
                sb.Append('\r');
            }
            else if (c == 't') {
                sb.Append('\t');
            }
            else if (c == 'u') {
                sb.Append((char)int.Parse(_s.MoveNext(4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
            else {
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_BadEscape));
            }
        }

        private char CheckQuoteChar(char? c) {
            Char quoteChar = '"';
            if (c == '\'') {
                quoteChar = c.Value;
            }
            else if (c != '"') {
                // Fail if the string is not quoted.
                throw new ArgumentException(_s.GetDebugString(AtlasWeb.JSON_StringNotQuoted));
            }

            return quoteChar;
        }

        private object DeserializeStringIntoDateTime() {
            // DivDiv 41127: Never confuse atlas serialized strings with dates.
            // DevDiv 74430: JavasciptSerializer will need to handle date time offset - following WCF design
            // serialized dates look like: "\/Date(123)\/" or "\/Date(123A)" or "Date(123+4567)" or Date(123-4567)"
            // the A, +14567, -4567 portion in the above example is ignored 
            int pos = _s.IndexOf(DateTimeSuffix);
            Match match = Regex.Match(_s.Substring(pos + DateTimeSuffixLength),
                @"^""\\/Date\((?<ticks>-?[0-9]+)(?:[a-zA-Z]|(?:\+|-)[0-9]{4})?\)\\/""");
            string ticksStr = match.Groups["ticks"].Value;

            long ticks;
            if (long.TryParse(ticksStr, out ticks)) {
                _s.MoveNext(match.Length);

                // The javascript ticks start from 1/1/1970 but FX DateTime ticks start from 1/1/0001
                DateTime dt = new DateTime(ticks * 10000 + JavaScriptSerializer.DatetimeMinTimeTicks, DateTimeKind.Utc);
                return dt;
            }
            else {
                // If we failed to get a DateTime, treat it as a string
                return DeserializeString();
            }
        }

        private static bool IsNextElementArray(Nullable<Char> c) {
            return c == '[';
        }

        private bool IsNextElementDateTime() {
            String next = _s.MoveNext(DateTimePrefixLength);
            if (next != null) {
                _s.MovePrev(DateTimePrefixLength);
                return String.Equals(next, DateTimePrefix, StringComparison.Ordinal);
            }

            return false;
        }

        private static bool IsNextElementObject(Nullable<Char> c) {
            return c == '{';
        }

        private static bool IsNextElementString(Nullable<Char> c) {
            return c == '"' || c == '\'';
        }
    }
}
