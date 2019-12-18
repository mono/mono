// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Xunit
{
    [Flags]
    public enum TargetFrameworkMonikers
    {
        Net45 = 0x1,
        Net451 = 0x2,
        Net452 = 0x4,
        Net46 = 0x8,
        Net461 = 0x10,
        Net462 = 0x20,
        Net463 = 0x40,
        Net472 = 0x60,
        Netcore50 = 0x80,
        Netcore50aot = 0x100,
        Netcoreapp1_0 = 0x200,
        Netcoreapp1_1 = 0x400,
        NetFramework = 0x800,
        Netcoreapp = 0x1000,
        UapNotUapAot = 0x2000,
        UapAot = 0x4000,
        Uap = UapAot | UapNotUapAot,
        // unused = 0x8000,
        Mono = 0x10000
    }
}
