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
    /// applied the PlatformSpecific attribute
    /// </summary>
    public class PlatformSpecificDiscoverer : ITraitDiscoverer
    {
        /// <summary>
        /// Gets the trait values from the Category attribute.
        /// </summary>
        /// <param name="traitAttribute">The trait attribute containing the trait values.</param>
        /// <returns>The trait values.</returns>
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            TestPlatforms platforms = (TestPlatforms)traitAttribute.GetConstructorArguments().First();
            if (!platforms.HasFlag(TestPlatforms.Windows))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonWindowsTest);
            if (!platforms.HasFlag(TestPlatforms.Linux))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonLinuxTest);
            if (!platforms.HasFlag(TestPlatforms.OSX))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonOSXTest);
            if (!platforms.HasFlag(TestPlatforms.FreeBSD))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonFreeBSDTest);
            if (!platforms.HasFlag(TestPlatforms.NetBSD))
                yield return new KeyValuePair<string, string>(XunitConstants.Category, XunitConstants.NonNetBSDTest);
        }
    }
}
