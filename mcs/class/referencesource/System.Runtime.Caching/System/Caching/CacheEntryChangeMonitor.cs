// <copyright file="CacheEntryChangeMonitor.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Runtime.Caching {
    public abstract class CacheEntryChangeMonitor : ChangeMonitor {
        public abstract ReadOnlyCollection<string> CacheKeys { get; }
        public abstract DateTimeOffset LastModified { get; }
        public abstract String RegionName { get; }
    }
}
