using System;
using System.Runtime.InteropServices;

namespace System.Runtime.Caching {
    /*
     * This class is used to retrieve the size of an object graph.
     * Although Mono has not a way of computing this.
     * Known problems:
     *   - CacheMemoryMonitor does not trim the cache when it reaches its memory size limit.
     *   - IMemoryCacheManager.UpdateCacheSize is called with incorrect size.
     */
    internal class SRef {

        private Object _sizedRef;

        internal SRef (Object target) {
            _sizedRef = target;
        }

        internal long ApproximateSize {
            get { return (long) Marshal.SizeOf (_sizedRef.GetType ()); }
        }

        internal void Dispose() {

        }
    }
}