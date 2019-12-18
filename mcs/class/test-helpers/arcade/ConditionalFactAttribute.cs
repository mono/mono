// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.DotNet.XUnitExtensions
{
    [XunitTestCaseDiscoverer("Microsoft.DotNet.XUnitExtensions.ConditionalFactDiscoverer", "net_4_x_System.Net.Http.FunctionalTests_xunit-test")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ConditionalFactAttribute : FactAttribute
    {
        public Type CalleeType { get; private set; }
        public string[] ConditionMemberNames { get; private set; }

        public ConditionalFactAttribute(Type calleeType, params string[] conditionMemberNames)
        {
            CalleeType = calleeType;
            ConditionMemberNames = conditionMemberNames;
        }

        public ConditionalFactAttribute(params string[] conditionMemberNames)
        {
            ConditionMemberNames = conditionMemberNames;
        }
    }
}
