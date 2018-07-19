// <copyright file="DefaultCacheCapabilities.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;

namespace System.Runtime.Caching {
    [Flags]
    public enum DefaultCacheCapabilities {
        None = 0x0,
        InMemoryProvider = 0x1,
        OutOfProcessProvider = 0x2,
        CacheEntryChangeMonitors = 0x4,
        AbsoluteExpirations = 0x8,
        SlidingExpirations = 0x10,
        CacheEntryUpdateCallback = 0x20,
        CacheEntryRemovedCallback = 0x40,
        CacheRegions = 0x80,
    }
}
