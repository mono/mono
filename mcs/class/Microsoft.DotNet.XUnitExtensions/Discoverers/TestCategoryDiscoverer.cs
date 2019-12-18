// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// This is a replacement for the MSTest [TestCategoryAttribute] on xunit
/// xunit does not have the concept of Category for tests and instead, the have [TraitAttribute(string key, string value)]
/// If we replace the MSTest [TestCategoryAttribute] for the [TestCategory("BVT")], we will surely fall at some time in cases 
/// where people will typo on the "Category" key part of the Trait. 
/// On order to achieve the same behaviour as on MSTest, a custom [TestCategory] was created 
/// to mimic the MSTest one and avoid replace it on every existent test. 
/// The tests can be filtered by xunit runners by usage of "-trait" on the command line with the expression like
/// <code>-trait "Category=BVT"</code> for example that will only run the tests with [TestCategory("BVT")] on it.
/// </summary>

namespace Microsoft.DotNet.XUnitExtensions
{
    public class TestCategoryDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
            yield return new KeyValuePair<string, string>("Category", ctorArgs[0].ToString());
        }
    }
}
