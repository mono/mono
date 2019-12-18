// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit;

namespace Microsoft.DotNet.XUnitExtensions
{
    /// <summary>
    /// This class discovers all of the tests and test classes that have
    /// applied the TestOnTargetFrameworkDiscoverer attribute
    /// </summary>
    public class SkipOnTargetFrameworkDiscoverer : ITraitDiscoverer
    {
        /// <summary>
        /// Gets the trait values from the Category attribute.
        /// </summary>
        /// <param name="traitAttribute">The trait attribute containing the trait values.</param>
        /// <returns>The trait values.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            TargetFrameworkMonikers frameworks = (TargetFrameworkMonikers)traitAttribute.GetConstructorArguments().First();
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net45))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet45Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net451))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet451Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net452))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet452Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net46))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet46Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net461))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet461Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net472))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet472Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net462))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet462Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Net463))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNet463Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Netcore50))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetcore50Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Netcore50aot))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetcore50aotTest);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Netcoreapp1_0))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetcoreapp1_0Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Netcoreapp1_1))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetcoreapp1_1Test);
            if (frameworks.HasFlag(TargetFrameworkMonikers.NetFramework))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetfxTest);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Mono))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonMonoTest);
            if (frameworks.HasFlag(TargetFrameworkMonikers.Netcoreapp))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetcoreappTest);
            if (frameworks.HasFlag(TargetFrameworkMonikers.UapNotUapAot))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonUapTest);
            if (frameworks.HasFlag(TargetFrameworkMonikers.UapAot))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonUapAotTest);
        }
    }
}
