// <copyright file="CacheEntryRemovedArguments.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Runtime.Caching {
    public class CacheEntryRemovedArguments {
        private CacheItem _cacheItem;
        private ObjectCache _source;
        private CacheEntryRemovedReason _reason;

        public CacheItem CacheItem { 
            get { return _cacheItem; }
        }

        public CacheEntryRemovedReason RemovedReason { 
            get { return _reason; } 
        }

        public ObjectCache Source { 
            get { return _source; }
        }

        public CacheEntryRemovedArguments(ObjectCache source, CacheEntryRemovedReason reason, CacheItem cacheItem) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }
            if (cacheItem == null) {
                throw new ArgumentNullException("cacheItem");
            }
            _source = source;
            _reason = reason;
            _cacheItem = cacheItem;
        }
    }
}



