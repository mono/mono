//------------------------------------------------------------------------------
// <copyright file="CapabilitiesPattern.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Security.Permissions;

    //
    // Represents a single pattern to be expanded
    //
    internal class CapabilitiesPattern {
        internal String[]    _strings;
        internal int[]       _rules;

        internal const int Literal    = 0;    // literal string
        internal const int Reference  = 1;    // regex reference ${name} or $number
        internal const int Variable   = 2;    // regex reference %{name}

        internal static readonly Regex refPat = new Regex("\\G\\$(?:(?<name>\\d+)|\\{(?<name>\\w+)\\})");
        internal static readonly Regex varPat = new Regex("\\G\\%\\{(?<name>\\w+)\\}");
        internal static readonly Regex textPat = new Regex("\\G[^$%\\\\]*(?:\\.[^$%\\\\]*)*");
        internal static readonly Regex errorPat = new Regex(".{0,8}");

        internal static readonly CapabilitiesPattern Default = new CapabilitiesPattern();

        internal CapabilitiesPattern() {
            _strings = new String[1];
            _strings[0] = String.Empty;
            _rules = new int[1];
            _rules[0] = Variable;
        }

        internal CapabilitiesPattern(String text) {
            ArrayList strings = new ArrayList();
            ArrayList rules = new ArrayList();

            int textpos = 0;

            for (;;) {
                Match match = null;

                // 1: scan text

                if ((match = textPat.Match(text, textpos)).Success && match.Length > 0) {
                    rules.Add(Literal);
                    strings.Add(Regex.Unescape(match.ToString()));
                    textpos = match.Index + match.Length;
                }

                if (textpos == text.Length)
                    break;

                // 2: look for regex references

                if ((match = refPat.Match(text, textpos)).Success) {
                    rules.Add(Reference);
                    strings.Add(match.Groups["name"].Value);
                }

                // 3: look for variables

                else if ((match = varPat.Match(text, textpos)).Success) {
                    rules.Add(Variable);
                    strings.Add(match.Groups["name"].Value);
                }

                // 4: encountered a syntax error (

                else {
                    match = errorPat.Match(text, textpos);

                    throw new ArgumentException(
                                               SR.GetString(SR.Unrecognized_construct_in_pattern, match.ToString(), text));
                }

                textpos = match.Index + match.Length;
            }

            _strings = (String[])strings.ToArray(typeof(String));

            _rules = new int[rules.Count];
            for (int i = 0; i < rules.Count; i++)
                _rules[i] = (int)rules[i];
        }

        internal virtual String Expand(CapabilitiesState matchstate) {
            StringBuilder sb = null;
            String result = null;

            for (int i = 0; i < _rules.Length; i++) {
                if (sb == null && result != null)
                    sb = new StringBuilder(result);

                switch (_rules[i]) {
                    case Literal:
                        result = _strings[i];
                        break;

                    case Reference:
                        result = matchstate.ResolveReference(_strings[i]);
                        break;

                    case Variable:
                        result = matchstate.ResolveVariable(_strings[i]);
                        break;
                }

                if (sb != null && result != null)
                    sb.Append(result);
            }

            if (sb != null)
                return sb.ToString();

            if (result != null)
                return result;

            return String.Empty;
        }

#if DBG
        internal virtual String Dump() {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _rules.Length; i++) {
                switch (_rules[i]) {
                    case Literal:
                        sb.Append("\"" + _strings[i] + "\"");
                        break;
                    case Reference:
                        sb.Append("${" + _strings[i] + "}");
                        break;
                    default:
                        sb.Append("??");
                        break;
                }

                if (i < _rules.Length - 1)
                    sb.Append(" ");
            }

            return sb.ToString();
        }

        internal virtual String Dump(String indent) {
            return indent + Dump() + "\n";
        }
#endif
    }
}
