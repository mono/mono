using System;
using System.Configuration.Provider;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Caching {
    // the abstract base class implemented by all output cache providers
    public abstract class OutputCacheProvider: ProviderBase {
        // Returns the specified entry, or null if it does not exist.
        public abstract Object Get(String key);
        // Inserts the specified entry into the cache if it does not already exist, otherwise returns the existing entry.
        public abstract Object Add(String key, Object entry, DateTime utcExpiry);
        // Inserts the specified entry into the cache, overwriting an existing entry if present.
        public abstract void Set(String key, Object entry, DateTime utcExpiry);
        // Removes the specified entry from the cache.
        public abstract void Remove(String key);
    }
}
