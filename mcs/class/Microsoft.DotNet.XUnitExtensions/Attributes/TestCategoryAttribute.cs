// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Apply this attribute to your test method to specify the test category.
    /// </summary>
    [TraitDiscoverer("Microsoft.DotNet.XUnitExtensions.TestCategoryDiscoverer", "Microsoft.DotNet.XUnitExtensions")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class TestCategoryAttribute : Attribute, ITraitAttribute
    {
        public TestCategoryAttribute(string category) { }
    }
}
