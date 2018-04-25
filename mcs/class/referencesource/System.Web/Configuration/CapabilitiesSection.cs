//------------------------------------------------------------------------------
// <copyright file="CapabilitiesSection.cs" company="Microsoft">
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
    // Implementation of <filter match="Mozilla/\d+\.\d+" with="${something}" />
    // expand the "with" pattern and match against the "match" expression.
    //
    internal class CapabilitiesSection : CapabilitiesRule {
        internal CapabilitiesPattern _expr;
        internal DelayedRegex _regex;
        internal CapabilitiesRule[] _rules;

        internal CapabilitiesSection(int type, DelayedRegex regex, CapabilitiesPattern expr, ArrayList rulelist) {
            _type = type;
            _regex = regex;
            _expr = expr;
            _rules = (CapabilitiesRule[])rulelist.ToArray(typeof(CapabilitiesRule));
        }

        internal override void Evaluate(CapabilitiesState state) {
            Match match;

            state.Exit = false;

            if (_regex != null) {
                match = _regex.Match(_expr.Expand(state));

                if (!match.Success)
                    return;

                state.AddMatch(_regex, match);
            }

            for (int i = 0; i < _rules.Length; i++) {
                _rules[i].Evaluate(state);

                if (state.Exit)
                    break;
            }

            if (_regex != null) {
                state.PopMatch();
            }

            state.Exit = (Type == Case);
        }
    }
}
