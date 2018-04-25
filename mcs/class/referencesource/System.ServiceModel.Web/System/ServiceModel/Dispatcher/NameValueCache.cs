//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Dispatcher
{
    class NameValueCache<T>
    {
        // The NameValueCache implements a structure that uses a dictionary to map objects to
        // indices of an array of cache entries.  This allows us to store the cache entries in 
        // the order in which they were added to the cache, and yet still lookup any cache entry.
        // The eviction policy of the cache is to evict the least-recently-added cache entry.  
        // Using a pointer to the next available cache entry in the array, we can always be sure 
        // that the given entry is the oldest entry. 
        Hashtable cache;
        string[] currentKeys;
        int nextAvailableCacheIndex;
        object cachelock;
        internal const int maxNumberofEntriesInCache = 16;

        public NameValueCache()
            : this(maxNumberofEntriesInCache)
        {
        }

        public NameValueCache(int maxCacheEntries)
        {
            cache = new Hashtable();
            currentKeys = new string[maxCacheEntries];
            cachelock = new object();
        }

        public T Lookup(string key)
        {
            return (T)cache[key];
        }

        public void AddOrUpdate(string key, T value)
        {
            lock (cache)
            {
                if (!cache.ContainsKey(key))
                {
                    if (!String.IsNullOrEmpty(currentKeys[nextAvailableCacheIndex]))
                    {
                        cache.Remove(currentKeys[nextAvailableCacheIndex]);
                    }
                    currentKeys[nextAvailableCacheIndex] = key;
                    nextAvailableCacheIndex = ++nextAvailableCacheIndex % currentKeys.Length;
                }
                cache[key] = value;
            }
        }
    }
}
