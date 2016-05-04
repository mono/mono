using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq.Mapping;
using System.Data.Linq.Provider;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Data.Linq.SqlClient {

    internal static class SqlIdentifier
    {
        private static SqlCommandBuilder builder = new SqlCommandBuilder();

        const string ParameterPrefix = "@";
        const string QuotePrefix = "[";
        const string QuoteSuffix = "]";
        const string SchemaSeparator = ".";
        const char SchemaSeparatorChar = '.';

        private static bool IsQuoted(string s) {
            if (s == null) {
                throw Error.ArgumentNull("s");
            }

            if (s.Length < 2) {
                return false;
            }

            return s.StartsWith(QuotePrefix, StringComparison.Ordinal)
                && s.EndsWith(QuoteSuffix, StringComparison.Ordinal);
        }

        // This is MSSQL-specific quoting.
        // If the string begins and ends with [ and ], it will be assumed to already be quoted.
        // Otherwise periods are assumed to be namespace delimiters, and the string is split on each.
        // Each string from the split is then check to see if it is already quoted, and if
        // not, it is replaced with the result of SqlCommandBuilder.QuoteIdentifier.
        // Then the set of strings is rejoined with periods.
        internal static string QuoteCompoundIdentifier(string s) {
            if (s == null) {
                throw Error.ArgumentNull("s");
            }

            // if it starts with @, then return unprocessed
            if (s.StartsWith(ParameterPrefix, StringComparison.Ordinal)) {
                return s;
            } else if (IsQuoted(s)) {
                return s;
            }
            else if (!s.StartsWith(QuotePrefix, StringComparison.Ordinal) && s.EndsWith(QuoteSuffix, StringComparison.Ordinal)) {
                //A.[B] => [A].[B]
                int splitPosition = s.IndexOf(SchemaSeparatorChar);
                if (splitPosition < 0){ //no . in the string
                    return builder.QuoteIdentifier(s);
                }
                string left = s.Substring(0, splitPosition);
                string right = s.Substring(splitPosition + 1, s.Length - splitPosition - 1);
                if (!IsQuoted(right)) {
                    right = builder.QuoteIdentifier(right);
                }
                return String.Concat(QuoteCompoundIdentifier(left), SchemaSeparatorChar + right);
            }
            else if (s.StartsWith(QuotePrefix, StringComparison.Ordinal) && !s.EndsWith(QuoteSuffix, StringComparison.Ordinal)) {
                //[A].B => [A].[B]
                int splitPosition = s.LastIndexOf(SchemaSeparatorChar);
                if (splitPosition < 0){ //no . in the string
                    return builder.QuoteIdentifier(s);
                }
                string left = s.Substring(0, splitPosition);
                if (!IsQuoted(left)) {
                    left = builder.QuoteIdentifier(left);
                }
                string right = s.Substring(splitPosition + 1, s.Length - splitPosition - 1);
                return String.Concat(left + SchemaSeparatorChar, QuoteCompoundIdentifier(right));
            }
            else {
                int splitPosition = s.IndexOf(SchemaSeparatorChar);
                if (splitPosition < 0) { //no . in the string
                    //A => [A]
                    return builder.QuoteIdentifier(s);
                }
                string left = s.Substring(0, splitPosition);
                string right = s.Substring(splitPosition + 1, s.Length - splitPosition - 1);
                return String.Concat(QuoteCompoundIdentifier(left) + SchemaSeparatorChar, QuoteCompoundIdentifier(right));
            }
        }

        // This is MSSQL-specific quoting.
        // This is the same as above, but it doesn't consider anything compound.
        internal static string QuoteIdentifier(string s) {
            if (s == null) {
                throw Error.ArgumentNull("s");
            }

            // if it starts with @, then return unprocessed
            if (s.StartsWith(ParameterPrefix, StringComparison.Ordinal)) {
                return s;
            } else if (IsQuoted(s)) {
                return s;
            } else {
                return builder.QuoteIdentifier(s);
            }
        }

        // turns "[ABC].[PQR].[XYZ]" into {"[ABC]", "[PQR]", "[XYZ]"}
        internal static IEnumerable<string> GetCompoundIdentifierParts(string s) {
            if (s == null) {
                throw Error.ArgumentNull("s");
            }

            // can't do this to parameters
            if (s.StartsWith(ParameterPrefix, StringComparison.Ordinal)) {
                throw Error.ArgumentWrongValue("s");
            }

            string quotedS = QuoteCompoundIdentifier(s);
            string pattern = @"^(?<component>\[([^\]]|\]\])*\])(\.(?<component>\[([^\]]|\]\])*\]))*$";

            // This pattern matches "."-delimited quoted SQL identifiers. Here's how:
            //
            // 1. It is wrapped in "^" and "$", which match the begining and end of text, so it will match
            //    only the entire text and not any sub-part.
            // 2. The group "(?<component>\[([^\]]|\]\])*\])" captures a single quoted segment of the text.
            //    It's a literal "[" followed by any number of non-"]" characters or "]]" strings, followed
            //    by a literal "]". The "?<component>" bit names the capture so we can refer to it.
            // 3. After the first component, we will allow any number of groups which consist of a literal
            //    "." followed by a component (and the component part is a repeat of the part described in 2).
 
            Match m = Regex.Match(quotedS, pattern);
            if (!m.Success)
            {
                throw Error.ArgumentWrongValue("s");
            }

            foreach (Capture cap in m.Groups["component"].Captures)
            {
                yield return cap.Value;
            }
        }
    }
}
