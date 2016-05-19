//------------------------------------------------------------------------------
// <copyright file="CapabilitiesAssignment.cs" company="Microsoft">
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
    // Implementation of the foo = ${bar}-something-%{que}
    // expand the pattern on the right and store it in the %{foo} variable
    //
    internal class CapabilitiesAssignment : CapabilitiesRule {
        internal String _var;
        internal CapabilitiesPattern _pat;

        internal CapabilitiesAssignment(String var, CapabilitiesPattern pat) {
            _type = Assign;
            _var = var;
            _pat = pat;
        }

        internal override void Evaluate(CapabilitiesState state) {
            state.SetVariable(_var, _pat.Expand(state));
            state.Exit = false;
        }
    }
}
