// <copyright file="IMemoryCacheManager.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;

namespace System.Runtime.Caching.Hosting {
    public interface IMemoryCacheManager {
        void UpdateCacheSize(long size, MemoryCache cache);
        void ReleaseCache(MemoryCache cache);
    }
}
