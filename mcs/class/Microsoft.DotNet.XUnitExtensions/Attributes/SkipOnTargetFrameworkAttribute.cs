﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Apply this attribute to your test method to specify this is a platform specific test.
    /// </summary>
    [TraitDiscoverer("Microsoft.DotNet.XUnitExtensions.SkipOnTargetFrameworkDiscoverer", "Microsoft.DotNet.XUnitExtensions")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class SkipOnTargetFrameworkAttribute : Attribute, ITraitAttribute
    {
        public SkipOnTargetFrameworkAttribute(TargetFrameworkMonikers platform, string reason = null) { }
    }
}
