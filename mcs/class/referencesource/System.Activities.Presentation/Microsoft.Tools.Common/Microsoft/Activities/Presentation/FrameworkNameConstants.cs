//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation
{
    using System;
    using System.Runtime.Versioning;

    internal static class FrameworkNameConstants
    {
        public static readonly FrameworkName NetFramework40 = new FrameworkName(NetFramework, new Version(4, 0));
        public static readonly FrameworkName NetFramework45 = new FrameworkName(NetFramework, new Version(4, 5));

        internal const string NetFramework = ".NETFramework";
        internal const string NetFrameworkWithSpace = ".NET Framework";
        internal const string ClientProfileName = "Client";
    }
}
