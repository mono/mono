//------------------------------------------------------------------------------
// <copyright file="CapabilitiesUse.cs" company="Microsoft">
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
    // Implementation of <use var="HTTP_ACCEPT_LANGUAGE" as="language" />: grab
    // the server variable and stuff it into the %{language} variable
    //
    internal class CapabilitiesUse : CapabilitiesRule {
        internal String _var;
        internal String _as;

        internal CapabilitiesUse(String var, String asParam) {
            _var = var;
            _as = asParam;
        }

        internal override void Evaluate(CapabilitiesState state) {
            state.SetVariable(_as, state.ResolveServerVariable(_var));
            state.Exit = false;
        }
    }
}
