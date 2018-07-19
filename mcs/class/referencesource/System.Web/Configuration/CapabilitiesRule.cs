//------------------------------------------------------------------------------
// <copyright file="CapabilitiesRule.cs" company="Microsoft">
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
    // CapabilitiesRule is a step in the computation of a capabilities object. It can be either
    // (1) import a string from the request object
    // (2) assign a pattern into a variable
    // (3) execute a subsequence if a regex matches
    // (4) execute a subsequence and exit the block if a regex matches
    //
    internal abstract class CapabilitiesRule {
        internal const int Use = 0;
        internal const int Assign = 1;
        internal const int Filter = 2;
        internal const int Case = 3;

        internal int _type;

        internal virtual int Type {
            get {
                return _type;
            }
        }

        internal abstract void Evaluate(CapabilitiesState state);
    }
}
