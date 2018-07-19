// <copyright file="PerfCounterName.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;

namespace System.Runtime.Caching {
    internal enum PerfCounterName {
        Entries = 0,
        Hits,
        HitRatio,
        HitRatioBase,
        Misses,
        Trims,
        Turnover
    }
}
