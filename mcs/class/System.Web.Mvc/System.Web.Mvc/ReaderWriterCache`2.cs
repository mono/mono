/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal abstract class ReaderWriterCache<TKey, TValue> {

        private readonly Dictionary<TKey, TValue> _cache;
        private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();

        protected ReaderWriterCache()
            : this(null) {
        }

        protected ReaderWriterCache(IEqualityComparer<TKey> comparer) {
            _cache = new Dictionary<TKey, TValue>(comparer);
        }

        protected Dictionary<TKey, TValue> Cache {
            get {
                return _cache;
            }
        }

        protected TValue FetchOrCreateItem(TKey key, Func<TValue> creator) {
            // first, see if the item already exists in the cache
            _rwLock.AcquireReaderLock(Timeout.Infinite);
            try {
                TValue existingEntry;
                if (_cache.TryGetValue(key, out existingEntry)) {
                    return existingEntry;
                }
            }
            finally {
                _rwLock.ReleaseReaderLock();
            }

            // insert the new item into the cache
            TValue newEntry = creator();
            _rwLock.AcquireWriterLock(Timeout.Infinite);
            try {
                TValue existingEntry;
                if (_cache.TryGetValue(key, out existingEntry)) {
                    // another thread already inserted an item, so use that one
                    return existingEntry;
                }

                _cache[key] = newEntry;
                return newEntry;
            }
            finally {
                _rwLock.ReleaseWriterLock();
            }
        }

    }
}
