// <copyright file="CacheEntryRemovedReason.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;

namespace System.Runtime.Caching {
    public enum CacheEntryRemovedReason {
        Removed = 0, //Explicitly removed via API call
        Expired,     
        Evicted,     //Evicted to free up space
        ChangeMonitorChanged,  //An associated programmatic dependency triggered eviction
        CacheSpecificEviction  //Catch-all for custom providers
    }
}
