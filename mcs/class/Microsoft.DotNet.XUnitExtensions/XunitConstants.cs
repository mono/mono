// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit.Sdk;

namespace Microsoft.DotNet.XUnitExtensions
{
    public struct XunitConstants
    {
        internal const string NonFreeBSDTest = "nonfreebsdtests";
        internal const string NonLinuxTest = "nonlinuxtests";
        internal const string NonNetBSDTest = "nonnetbsdtests";
        internal const string NonOSXTest = "nonosxtests";
        internal const string NonWindowsTest = "nonwindowstests";

        internal const string NonNet45Test = "nonnet45tests";
        internal const string NonNet451Test = "nonnet451tests";
        internal static string NonNet452Test = "nonnet452tests";
        internal static string NonNet46Test = "nonnet46tests";
        internal static string NonNet461Test = "nonnet461tests";
        internal static string NonNet462Test = "nonnet462tests";
        internal static string NonNet463Test = "nonnet463tests";
        internal static string NonNet472Test = "nonnet472tests";
        internal static string NonNetcore50Test = "nonnetcore50tests";
        internal static string NonNetcore50aotTest = "nonnetcore50aottests";
        internal static string NonNetcoreapp1_0Test = "nonnetcoreapp1.0tests";
        internal static string NonNetcoreapp1_1Test = "nonnetcoreapp1.1tests";

        //Non version framework constants
        internal static string NonNetfxTest = "nonnetfxtests";
        internal static string NonMonoTest = "nonmonotests";
        internal static string NonUapTest = "nonuaptests";
        internal static string NonUapAotTest = "nonuapaottests";
        internal static string NonNetcoreappTest = "nonnetcoreapptests";

        internal const string Failing = "failing";
        internal const string ActiveIssue = "activeissue";
        internal const string OuterLoop = "outerloop";

        public const string Category = "category";
        public const string IgnoreForCI = "ignoreforci";
        public const string RequiresElevation = "requireselevation";
    }
}
