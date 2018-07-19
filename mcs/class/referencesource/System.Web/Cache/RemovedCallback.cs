// <copyright file="RemovedCallback.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
#if USE_MEMORY_CACHE
using System;
using System.Runtime.Caching;
using System.Web.Util;

namespace System.Web.Caching {
    internal sealed class RemovedCallback {
        CacheItemRemovedCallback _callback;
        internal RemovedCallback(CacheItemRemovedCallback callback) {
            _callback = callback;
        }

        internal void CacheEntryRemovedCallback(CacheEntryRemovedArguments arguments) {
            string key = arguments.CacheItem.Key;
            object value = arguments.CacheItem.Value;
            CacheItemRemovedReason reason;
            switch (arguments.RemovedReason) {
                case CacheEntryRemovedReason.Removed :
                    reason = CacheItemRemovedReason.Removed;
                    break;
                case CacheEntryRemovedReason.Expired :
                    reason = CacheItemRemovedReason.Expired;
                    break;
                case CacheEntryRemovedReason.Evicted :
                    reason = CacheItemRemovedReason.Underused;
                    break;
                case CacheEntryRemovedReason.ChangeMonitorChanged :
                    reason = CacheItemRemovedReason.DependencyChanged;
                    break;
                default :
                    reason = CacheItemRemovedReason.Removed;
                    break;
            }
            _callback(key, value, reason);
        }
    }
}
#endif
