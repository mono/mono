//------------------------------------------------------------------------------
// <copyright file="SubProtocolUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.WebSockets {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Utility class for creating and parsing "Sec-WebSocket-Protocol" headers
    //
    // From the WebSocket protocol spec, sec. 4.1:
    // 10.  The request MAY include a header field with the name "Sec-
    //      WebSocket-Protocol".  If present, this value indicates one or
    //      more comma separated subprotocol the client wishes to speak,
    //      ordered by preference.  The elements that comprise this value
    //      MUST be non-empty strings with characters in the range U+0021 to
    //      U+007E not including separator characters as defined in
    //      [RFC2616], and MUST all be unique strings.  The ABNF for the
    //      value of this header field is 1#token, where the definitions of
    //      constructs and rules are as given in [RFC2616].
    //
    // RFC 2616, sec. 2.1:
    // #rule
    //    A construct "#" is defined, similar to "*", for defining lists of
    //    elements. The full form is "<n>#<m>element" indicating at least
    //    <n> and at most <m> elements, each separated by one or more commas
    //    (",") and OPTIONAL linear white space (LWS). This makes the usual
    //    form of lists very easy; a rule such as
    //       ( *LWS element *( *LWS "," *LWS element ))
    //    can be shown as
    //       1#element
    //    Wherever this construct is used, null elements are allowed, but do
    //    not contribute to the count of elements present. That is,
    //    "(element), , (element) " is permitted, but counts as only two
    //    elements. Therefore, where at least one element is required, at
    //    least one non-null element MUST be present. Default values are 0
    //    and infinity so that "#element" allows any number, including zero;
    //    "1#element" requires at least one; and "1#2element" allows one or
    //    two.

    internal static class SubProtocolUtil {

        // RFC 2616, sec. 2.2:
        // LWS            = [CRLF] 1*( SP | HT )
        // We use a subset: _lwsTrimChars = SP | HT
        private static readonly char[] _lwsTrimChars = new char[] { ' ', '\t' };
        private static readonly char[] _splitChars = new char[] { ',' };

        // Returns a value stating whether the specified SubProtocol is valid
        public static bool IsValidSubProtocolName(string subprotocol) {
            return (!String.IsNullOrEmpty(subprotocol) && subprotocol.All(IsValidSubProtocolChar));
        }

        private static bool IsValidSubProtocolChar(char c) {
            return ('\u0021' <= c && c <= '\u007e' && !IsSeparatorChar(c));
        }

        // RFC 2616, sec. 2.2:
        // separators     = "(" | ")" | "<" | ">" | "@"
        //                | "," | ";" | ":" | "\" | <">
        //                | "/" | "[" | "]" | "?" | "="
        //                | "{" | "}" | SP | HT
        private static bool IsSeparatorChar(char c) {
            switch (c) {
                case '(':
                case ')':
                case '<':
                case '>':
                case '@':
                case ',':
                case ';':
                case ':':
                case '\\':
                case '"':
                case '/':
                case '[':
                case ']':
                case '?':
                case '=':
                case '{':
                case '}':
                case ' ':
                case '\t':
                    return true;

                default:
                    return false;
            }
        }

        // Returns a list of preferred subprotocols by parsing an incoming header value, or null if the incoming header was improperly formatted.
        public static List<string> ParseHeader(string headerValue) {
            if (headerValue == null) {
                // No incoming values
                return null;
            }

            List<string> subprotocols = new List<string>();
            foreach (string subprotocolCandidate in headerValue.Split(_splitChars)) {
                string subprotocolCandidateTrimmed = subprotocolCandidate.Trim(_lwsTrimChars); // remove LWS according to '#' rule

                // skip LWS between commas according to '#' rule
                if (subprotocolCandidateTrimmed.Length == 0) {
                    continue;
                }

                // reject improperly formatted header values
                if (!IsValidSubProtocolName(subprotocolCandidateTrimmed)) {
                    return null;
                }

                // otherwise this subprotocol is OK
                subprotocols.Add(subprotocolCandidateTrimmed);
            }

            if (subprotocols.Count == 0) {
                // header is improperly formatted (contained no usable values)
                return null;
            }

            if (subprotocols.Distinct(StringComparer.Ordinal).Count() != subprotocols.Count) {
                // header is improperly formatted (contained duplicate values)
                return null;
            }

            return subprotocols;
        }

    }
}
