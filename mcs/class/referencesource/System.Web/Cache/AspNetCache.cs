//------------------------------------------------------------------------------
// <copyright file="AspNetCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 /*
 * AspNetCache class
 *
 * Copyright (c) 2016 Microsoft Corporation
 */

 using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Util;
using System.Web;
using Microsoft.Win32;
using System.Security.Permissions;
using System.Globalization;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Management;
using Debug = System.Web.Util.Debug;


namespace System.Web.Caching
{
    // CacheMultiple/CacheSingle
    internal sealed class AspNetCache : CacheStoreProvider {
        static CacheInsertOptions DefaultInsertOptions = new CacheInsertOptions();

         internal CacheInternal _cacheInternal;
        bool _isPublic = true;
        bool _isDisposed = false;

         public AspNetCache() {
            _cacheInternal = CacheInternal.Create();
            Interlocked.Exchange(ref _cacheInternal._refCount, 1);
        }

         internal AspNetCache(bool isPublic) {
            _isPublic = isPublic;
            _cacheInternal = CacheInternal.Create();
            Interlocked.Exchange(ref _cacheInternal._refCount, 1);
        }

         internal AspNetCache(AspNetCache cache, bool isPublic) {
            _isPublic = isPublic;
            _cacheInternal = cache._cacheInternal;
            Interlocked.Increment(ref _cacheInternal._refCount);
        }


         public override long ItemCount {
            get {
                if (_isPublic) {
                    return _cacheInternal.PublicCount;
                }
                return _cacheInternal.TotalCount - _cacheInternal.PublicCount;
            }
        }
        public override long SizeInBytes {
            get {
                return _cacheInternal.ApproximateSize;
            }
        }

         public override void Initialize(string name, NameValueCollection config) {
            bool isPublic = _isPublic;
            if (Boolean.TryParse(config["isPublic"], out isPublic)) {
                _isPublic = isPublic;
            }

             CacheSection cacheSection = RuntimeConfig.GetAppConfig().Cache;
            _cacheInternal.ReadCacheInternalConfig(cacheSection);
        }

         public override object Add(string key, object item, CacheInsertOptions options) {
            CacheInsertOptions opts = options ?? DefaultInsertOptions;
            return _cacheInternal.DoInsert(_isPublic, key, item, opts.Dependencies, opts.AbsoluteExpiration,
                opts.SlidingExpiration, opts.Priority, opts.OnRemovedCallback, false);
        }

         public override object Get(string key) { return _cacheInternal.DoGet(_isPublic, key, CacheGetOptions.None); }

         public override void Insert(string key, object item, CacheInsertOptions options) {
            CacheInsertOptions opts = options ?? DefaultInsertOptions;
            _cacheInternal.DoInsert(_isPublic, key, item, opts.Dependencies, opts.AbsoluteExpiration, opts.SlidingExpiration,
                opts.Priority, opts.OnRemovedCallback, true);
        }

         public override object Remove(string key) { return Remove(key, CacheItemRemovedReason.Removed); }
        public override object Remove(string key, CacheItemRemovedReason reason) {
            CacheKey cacheKey = new CacheKey(key, _isPublic);
            return _cacheInternal.Remove(cacheKey, reason);
        }

         public override long Trim(int percent) { return _cacheInternal.TrimCache(percent); }

         public override bool AddDependent(string key, CacheDependency dependency, out DateTime utcLastUpdated) {
            CacheEntry entry = (CacheEntry)_cacheInternal.DoGet(_isPublic, key, CacheGetOptions.ReturnCacheEntry);
            if (entry != null) {
               utcLastUpdated = entry.UtcCreated;
               entry.AddDependent(dependency);  // This seems better in the next if... but here is more faithful to original code

                if (entry.State == CacheEntry.EntryState.AddedToCache) {
                   return true;
               }
            }

             utcLastUpdated = DateTime.MinValue;
            return false;
        }

         public override void RemoveDependent(string key, CacheDependency dependency) {
            CacheEntry entry = (CacheEntry)_cacheInternal.DoGet(_isPublic, key, CacheGetOptions.ReturnCacheEntry);
            if (entry != null) {
                entry.RemoveDependent(dependency);
            }
        }

         public override IDictionaryEnumerator GetEnumerator() { return _cacheInternal.CreateEnumerator(!_isPublic); }

         public override bool Equals(object obj)
        {
            AspNetCache other = obj as AspNetCache;

             if (other != null)
                return (_cacheInternal == other._cacheInternal);

             return base.Equals(obj);
        }

         public override int GetHashCode()
        {
            return base.GetHashCode();
        }

         public override void Dispose() {
            if (!_isDisposed) {
                lock (this) {
                    if (!_isDisposed) {
                        _isDisposed = true;
                        Interlocked.Decrement(ref _cacheInternal._refCount);
                        _cacheInternal.Dispose();
                    }
                }
            }
        }
    }
}