using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Management;
using System.Web.UI;
using System.Web.Util;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Web.Caching {

    /*
      We currently have the following buffer types:

      HttpResponseBufferElement          - managed memory
      HttpResponseUnmanagedBufferElement - native memory
      HttpResourceResponseElement        - pointer to resource
      HttpFileResponseElement            - contains a file name, file can have 64-bit length
      HttpSubstBlockResponseElement      - subst callback

      Custom output cache providers do not support all features.  If we cannot use a custom output cache,
      provider, we will use the internal cache.  We will use a custom output cache provider only if:

        a) no instance validation callbacks (static callbacks are okay)
        b) no dependencies other than files
        c) no sliding expiration
        d) no instance substitution callbacks (HttpSubstBlockResponseElement) (static callbacks okay)

        
      INSERT/GET:
      ----------
      The custom cache provider receives an OutputCacheEntry, which contains the status, headers, response elements, 
      cache policy settings, and kernel cache key.  The cache provider can coallesce response elements.  For example,
      a disk based cache may return a single response element which is the name of a file containing the entire response.

      ISSUES:
      
      If the custom cache provider is distributed, the cache keys must be unique for two applications with the same name 
      but different domains.  For example, a request to http://company1.com/myapp/page.aspx and a request to 
      http://company2.com/myapp/page.aspx need to use output cache entry keys that include the domain name.  Today, the 
      key is just /myapp/page.aspx.  It would need to include the Host header, but if that is "localhost", "::1", etc, 
      we'd need to use %USERDOMAIN%\%COMPUTERNAME% instead.  But it is possible for a single site to have multiple host name 
      bindings, so this would result in multiple cache entries for potentially the same cached result.  We could allow this to be
      adjusted by adding a setting to the cache policy.

    */

    internal class DependencyCacheEntry {
        private           string _providerName;
        private           string _outputCacheEntryKey;
        private           string _kernelCacheEntryKey;

        internal string ProviderName        { get { return _providerName; } }
        internal string OutputCacheEntryKey { get { return _outputCacheEntryKey; } }
        internal string KernelCacheEntryKey { get { return _kernelCacheEntryKey; } }
        
        internal DependencyCacheEntry(string oceKey, string kernelCacheEntryKey, string providerName) {
            _outputCacheEntryKey = oceKey;
            _kernelCacheEntryKey = kernelCacheEntryKey;
            _providerName = providerName;
        }
    }

    public static class OutputCache {
        private  const  string                          OUTPUTCACHE_KEYPREFIX_DEPENDENCIES = CacheInternal.PrefixOutputCache + "D";
        internal const  string                          ASPNET_INTERNAL_PROVIDER_NAME = "AspNetInternalProvider";
        private  static bool                            s_inited;
        private  static object                          s_initLock = new object();
        private  static CacheItemRemovedCallback        s_entryRemovedCallback;     
        private  static CacheItemRemovedCallback        s_dependencyRemovedCallback;
        private  static CacheItemRemovedCallback        s_dependencyRemovedCallbackForFragment;
        private  static OutputCacheProvider             s_defaultProvider;
        private  static OutputCacheProviderCollection   s_providers;
        // when there are no providers being used, we'll use this count to optimize performance when the value is zero.
        private  static int                             s_cEntries;

        //
        // private helper methods
        //

        private static void AddCacheKeyToDependencies(ref CacheDependency dependencies, string cacheKey) {
            CacheDependency keyDep = new CacheDependency(0, null, new string[1] {cacheKey});
            if (dependencies == null) {
                dependencies = keyDep;
            }
            else {
                // if it's not an aggregate, we have to create one because you can't add
                // it to anything but an aggregate
                AggregateCacheDependency agg = dependencies as AggregateCacheDependency;
                if (agg != null) {
                    agg.Add(keyDep);
                }
                else {
                    agg = new AggregateCacheDependency();
                    agg.Add(keyDep, dependencies);
                    dependencies = agg;
                }
            }
        }

        private static void EnsureInitialized() {
            if (s_inited) 
                return;

            lock (s_initLock) {
                if (!s_inited) {
                    OutputCacheSection settings = RuntimeConfig.GetAppConfig().OutputCache;
                    s_providers = settings.CreateProviderCollection();
                    s_defaultProvider = settings.GetDefaultProvider(s_providers);
                    s_entryRemovedCallback = new CacheItemRemovedCallback(OutputCache.EntryRemovedCallback);
                    s_dependencyRemovedCallback = new CacheItemRemovedCallback(OutputCache.DependencyRemovedCallback);
                    s_dependencyRemovedCallbackForFragment = new CacheItemRemovedCallback(OutputCache.DependencyRemovedCallbackForFragment);
                    s_inited = true;
                }
            }
        }

        private static void DecrementCount() {
            if (Providers == null)
                Interlocked.Decrement(ref s_cEntries);
        }

        private static void IncrementCount() {
            if (Providers == null)
                Interlocked.Increment(ref s_cEntries);
        }

        private static OutputCacheProvider GetFragmentProvider(String providerName) {
            // if providerName is null, use default provider.  If default provider is null, we'll use internal cache.
            // if providerName is not null, get it from the provider collection.
            OutputCacheProvider provider = null;
            if (providerName == null) {
                provider = s_defaultProvider;
            }
            else {
                provider = s_providers[providerName];
                if (provider == null) {
                    Debug.Assert(false, "Unexpected, " + providerName + " should be a member of the collection.");
                    throw new ProviderException(SR.GetString(SR.Provider_Not_Found, providerName));
                }
            }
#if DBG
            string msg = (provider != null) ? provider.GetType().Name : "null";
            Debug.Trace("OutputCache", "GetFragmentProvider(" + providerName + ") --> " + msg);
#endif
            return provider;
        }

        private static OutputCacheProvider GetProvider(HttpContext context) {
            Debug.Assert(context != null, "context != null");
            if (context == null) {
                return null;
            }
            // Call GetOutputCacheProviderName
            // so it can determine which provider to use.
            HttpApplication app = context.ApplicationInstance;
            string name = app.GetOutputCacheProviderName(context);
            if (name == null) {
                throw new ProviderException(SR.GetString(SR.GetOutputCacheProviderName_Invalid, name));
            }
            // AspNetInternalProvider means use the internal cache
            if (name == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME) {
                return null;
            }
            OutputCacheProvider provider = (s_providers == null) ? null : s_providers[name];
            if (provider == null) {
                throw new ProviderException(SR.GetString(SR.GetOutputCacheProviderName_Invalid, name));
            }
            return provider;
        }

        private static OutputCacheEntry Convert(CachedRawResponse cachedRawResponse, string depKey, string[] fileDependencies) {
            List<HeaderElement> headerElements = null;
            ArrayList headers = cachedRawResponse._rawResponse.Headers;
            int count = (headers != null) ? headers.Count : 0;
            for (int i = 0; i < count; i++) {
                if (headerElements == null) {
                    headerElements = new List<HeaderElement>(count);
                }
                HttpResponseHeader h = (HttpResponseHeader)(headers[i]);
                headerElements.Add(new HeaderElement(h.Name, h.Value));
            }
            
            List<ResponseElement> responseElements = null;
            ArrayList buffers = cachedRawResponse._rawResponse.Buffers;
            count = (buffers != null) ? buffers.Count : 0;
            for (int i = 0; i < count; i++) {
                if (responseElements == null) {
                    responseElements = new List<ResponseElement>(count);
                }
                IHttpResponseElement elem = buffers[i] as IHttpResponseElement;
                if (elem is HttpFileResponseElement) {
                    HttpFileResponseElement fileElement = elem as HttpFileResponseElement;
                    responseElements.Add(new FileResponseElement(fileElement.FileName, fileElement.Offset, elem.GetSize()));
                }
                else if (elem is HttpSubstBlockResponseElement) {
                    HttpSubstBlockResponseElement substElement = elem as HttpSubstBlockResponseElement;
                    responseElements.Add(new SubstitutionResponseElement(substElement.Callback));
                }
                else {
                    byte[] b = elem.GetBytes();
                    long length = (b != null) ? b.Length : 0;
                    responseElements.Add(new MemoryResponseElement(b, length));
                }
            }                

            OutputCacheEntry oce = new OutputCacheEntry(
                cachedRawResponse._cachedVaryId,
                cachedRawResponse._settings,
                cachedRawResponse._kernelCacheUrl,
                depKey,
                fileDependencies,
                cachedRawResponse._rawResponse.StatusCode,
                cachedRawResponse._rawResponse.StatusDescription,
                headerElements,
                responseElements
                );

            return oce;
        }

        private static CachedRawResponse Convert(OutputCacheEntry oce) {            
            ArrayList headers = null;
            if (oce.HeaderElements != null && oce.HeaderElements.Count > 0) {
                headers = new ArrayList(oce.HeaderElements.Count);
                for (int i = 0; i < oce.HeaderElements.Count; i++) {
                    HttpResponseHeader h = new HttpResponseHeader(oce.HeaderElements[i].Name, oce.HeaderElements[i].Value);
                    headers.Add(h);
                }                
            }

            ArrayList buffers = null;
            if (oce.ResponseElements != null && oce.ResponseElements.Count > 0) {
                buffers = new ArrayList(oce.ResponseElements.Count);
                for (int i = 0; i < oce.ResponseElements.Count; i++) {
                    ResponseElement re = oce.ResponseElements[i];
                    IHttpResponseElement elem = null;
                    if (re is FileResponseElement) {
                        HttpContext context = HttpContext.Current;
                        HttpWorkerRequest wr = (context != null) ? context.WorkerRequest : null;
                        bool supportsLongTransmitFile = (wr != null && wr.SupportsLongTransmitFile);
                        bool isImpersonating = ((context != null && context.IsClientImpersonationConfigured) || HttpRuntime.IsOnUNCShareInternal);
                        FileResponseElement fre = (FileResponseElement)re;

                        // DevDiv #21203: Need to verify permission to access the requested file since handled by native code.
                        HttpRuntime.CheckFilePermission(fre.Path);

                        elem = new HttpFileResponseElement(fre.Path, fre.Offset, fre.Length, isImpersonating, supportsLongTransmitFile);
                    }
                    else if (re is MemoryResponseElement) {
                        MemoryResponseElement mre = (MemoryResponseElement)re;
                        int size = System.Convert.ToInt32(mre.Length);
                        elem = new HttpResponseBufferElement(mre.Buffer, size);
                    }
                    else if (re is SubstitutionResponseElement) {
                        SubstitutionResponseElement sre = (SubstitutionResponseElement)re;
                        elem = new HttpSubstBlockResponseElement(sre.Callback);                        
                    }
                    else {
                        throw new NotSupportedException();
                    }
                    buffers.Add(elem);
                }
            }
            else {
                buffers = new ArrayList();
            }
            
            HttpRawResponse rawResponse = new HttpRawResponse(oce.StatusCode, oce.StatusDescription, headers, buffers, false /*hasSubstBlocks*/);
            CachedRawResponse cachedRawResponse = new CachedRawResponse(rawResponse, oce.Settings, oce.KernelCacheUrl, oce.CachedVaryId);

            return cachedRawResponse;
        }


        //
        // helpers for accessing CacheInternal
        //

        // add CachedVary
        private static CachedVary UtcAdd(String key, CachedVary cachedVary) {
            return (CachedVary) HttpRuntime.CacheInternal.UtcAdd(key, 
                                                                 cachedVary, 
                                                                 null /*dependencies*/, 
                                                                 Cache.NoAbsoluteExpiration, 
                                                                 Cache.NoSlidingExpiration, 
                                                                 CacheItemPriority.Normal, 
                                                                 null /*callback*/);
        }        

        // add ControlCachedVary
        private static ControlCachedVary UtcAdd(String key, ControlCachedVary cachedVary) {
            return (ControlCachedVary) HttpRuntime.CacheInternal.UtcAdd(key, 
                                                                 cachedVary, 
                                                                 null /*dependencies*/, 
                                                                 Cache.NoAbsoluteExpiration, 
                                                                 Cache.NoSlidingExpiration, 
                                                                 CacheItemPriority.Normal, 
                                                                 null /*callback*/);
        }        

        private static bool IsSubstBlockSerializable(HttpRawResponse rawResponse) {
            if (!rawResponse.HasSubstBlocks)
                return true;
            for (int i = 0; i < rawResponse.Buffers.Count; i++) {
                HttpSubstBlockResponseElement substBlock = rawResponse.Buffers[i] as HttpSubstBlockResponseElement;
                if (substBlock == null)
                    continue;
                if (!substBlock.Callback.Method.IsStatic)
                    return false;
            }
            return true;
        }


        //
        // callbacks
        //

        private static void HandleErrorWithoutContext(Exception e) {
            HttpApplicationFactory.RaiseError(e);
            try {
                WebBaseEvent.RaiseRuntimeError(e, typeof(OutputCache));
            }
            catch {
            }
        }

        // only used by providers
        private static void DependencyRemovedCallback(string key, object value, CacheItemRemovedReason reason) {
            Debug.Trace("OutputCache", "DependencyRemovedCallback: reason=" + reason + ", key=" + key);

            DependencyCacheEntry dce = value as DependencyCacheEntry;
            if (dce.KernelCacheEntryKey != null) {
                // invalidate kernel cache entry
                if (HttpRuntime.UseIntegratedPipeline) {
                    UnsafeIISMethods.MgdFlushKernelCache(dce.KernelCacheEntryKey);
                }
                else {
                    UnsafeNativeMethods.InvalidateKernelCache(dce.KernelCacheEntryKey);
                }
            }
            if (reason == CacheItemRemovedReason.DependencyChanged) {
                if (dce.OutputCacheEntryKey != null) {
                    try {
                        OutputCache.RemoveFromProvider(dce.OutputCacheEntryKey, dce.ProviderName);
                    }
                    catch (Exception e) {
                        HandleErrorWithoutContext(e);
                    }
                }
            }
        }

        // only used by providers
        private static void DependencyRemovedCallbackForFragment(string key, object value, CacheItemRemovedReason reason) {
            Debug.Trace("OutputCache", "DependencyRemovedCallbackForFragment: reason=" + reason + ", key=" + key);

            if (reason == CacheItemRemovedReason.DependencyChanged) {
                DependencyCacheEntry dce = value as DependencyCacheEntry;
                if (dce.OutputCacheEntryKey != null) {
                    try {
                        OutputCache.RemoveFragment(dce.OutputCacheEntryKey, dce.ProviderName);
                    }
                    catch (Exception e) {
                        HandleErrorWithoutContext(e);
                    }
                }
            }
        }

        // only used by internal cache
        private static void EntryRemovedCallback(string key, object value, CacheItemRemovedReason reason) {
            Debug.Trace("OutputCache", "EntryRemovedCallback: reason=" + reason + ", key=" + key);

            DecrementCount();

            PerfCounters.DecrementCounter(AppPerfCounter.OUTPUT_CACHE_ENTRIES);
            PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_TURNOVER_RATE);

            CachedRawResponse cachedRawResponse = value as CachedRawResponse;
            if (cachedRawResponse != null) {
                String kernelCacheUrl = cachedRawResponse._kernelCacheUrl;
                // if it is kernel cached, the url will be non-null.
                // if the entry was re-inserted, don't remove kernel entry since it will be updated
                if (kernelCacheUrl != null && HttpRuntime.CacheInternal.Get(key) == null) {
                    // invalidate kernel cache entry
                    if (HttpRuntime.UseIntegratedPipeline) {
                        UnsafeIISMethods.MgdFlushKernelCache(kernelCacheUrl);
                    }
                    else {
                        UnsafeNativeMethods.InvalidateKernelCache(kernelCacheUrl);
                    }
                }
            }
        }

        //
        // public properties
        //

        public static string DefaultProviderName {
            get {
                EnsureInitialized();
                return (s_defaultProvider != null) ? s_defaultProvider.Name : OutputCache.ASPNET_INTERNAL_PROVIDER_NAME;
            }
        }

        public static OutputCacheProviderCollection Providers {
            get {
                EnsureInitialized();
                return s_providers;
            }
        }


        //
        // internal properties and methods
        //


        //
        // If we're not using a provider, we can optimize this so the OutputCacheModule
        // only need run when there are entries--return true iff count is non-zero.
        // If we're using a provider, it's not easy to keep track of the number of entries,
        // so always return true (so OutputCacheModule always runs).
        // 
        internal static bool InUse {
            get {
                return (Providers == null) ? (s_cEntries != 0) : true;
            }
        }
        
        internal static void ThrowIfProviderNotFound(String providerName) {
            // null means use default provider or internal cache
            if (providerName == null) {
                return;
            }
            OutputCacheProviderCollection providers = Providers;

            if (providers == null || providers[providerName] == null) {
                throw new ProviderException(SR.GetString(SR.Provider_Not_Found, providerName));
            }
        }

        internal static bool HasDependencyChanged(bool isFragment, string depKey, string[] fileDeps, string kernelKey, string oceKey, string providerName) {

            if (depKey == null)
            {
#if DBG
                Debug.Trace("OutputCache", "HasDependencyChanged(" + depKey + ", ..., " + oceKey + ", ...) --> false");
#endif
                return false;
            }

            // is the file dependency already in the in-memory cache?
            if (HttpRuntime.CacheInternal.Get(depKey) != null) {
#if DBG
                Debug.Trace("OutputCache", "HasDependencyChanged(" + depKey + ", ..., " + oceKey + ", ...) --> false");
#endif
                return false;
            }

            // deserialize the file dependencies
            CacheDependency dep = new CacheDependency(0, fileDeps);                

            int idStartIndex = OUTPUTCACHE_KEYPREFIX_DEPENDENCIES.Length;
            int idLength = depKey.Length - idStartIndex;

            CacheItemRemovedCallback callback = (isFragment) ? s_dependencyRemovedCallbackForFragment : s_dependencyRemovedCallback;

            // have the file dependencies changed?
            if (String.Compare(dep.GetUniqueID(), 0, depKey, idStartIndex, idLength, StringComparison.Ordinal) == 0) {
                // file dependencies have not changed--cache them with callback to remove OutputCacheEntry if they change
                HttpRuntime.CacheInternal.UtcInsert(depKey, new DependencyCacheEntry(oceKey, kernelKey, providerName), dep, 
                                                    Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, 
                                                    CacheItemPriority.Normal, callback);
#if DBG
                Debug.Trace("OutputCache", "HasDependencyChanged(" + depKey + ", ..., " + oceKey + ", ...) --> false, DEPENDENCY RE-INSERTED");
#endif
                return false;
            }
            else {
                // file dependencies have changed
                dep.Dispose();
#if DBG
                Debug.Trace("OutputCache", "HasDependencyChanged(" + depKey + ", ..., " + oceKey + ", ...) --> true, " + dep.GetUniqueID());
#endif
                return true;
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public static void Serialize(Stream stream, Object data) {
            BinaryFormatter formatter = new BinaryFormatter();
            if (data is OutputCacheEntry || data is PartialCachingCacheEntry || data is CachedVary || data is ControlCachedVary ||
                data is FileResponseElement || data is MemoryResponseElement || data is SubstitutionResponseElement) {
                formatter.Serialize(stream, data);
            }
            else {
                throw new ArgumentException(SR.GetString(SR.OutputCacheExtensibility_CantSerializeDeserializeType));
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public static object Deserialize(Stream stream) {
            BinaryFormatter formatter = new BinaryFormatter();
            Object data = formatter.Deserialize(stream);
            if (!(data is OutputCacheEntry || data is PartialCachingCacheEntry || data is CachedVary || data is ControlCachedVary ||
                  data is FileResponseElement || data is MemoryResponseElement || data is SubstitutionResponseElement)) {
                throw new ArgumentException(SR.GetString(SR.OutputCacheExtensibility_CantSerializeDeserializeType));
            }
            return data;
        }

        // lookup cached vary
        // lookup entry
        // lookup entry for content-encoding
        internal static Object Get(String key) {
            // if it's not in the provider or the default provider is undefined,
            // check the internal cache (we don't know where it is).
            Object result = null;
            OutputCacheProvider provider = GetProvider(HttpContext.Current);
            if (provider != null) {
                result = provider.Get(key);                
                OutputCacheEntry oce = result as OutputCacheEntry;
                if (oce != null) {
                    if (HasDependencyChanged(false /*isFragment*/, oce.DependenciesKey, oce.Dependencies, oce.KernelCacheUrl, key, provider.Name)) {
                        OutputCache.RemoveFromProvider(key, provider.Name);
#if DBG
                        Debug.Trace("OutputCache", "Get(" + key + ") --> null, " + provider.Name);
#endif
                        return null;

                    }
                    result = Convert(oce);
                }
            }
            if (result == null) {
                result = HttpRuntime.CacheInternal.Get(key);
#if DBG
                string typeName = (result != null) ? result.GetType().Name : "null";            
                Debug.Trace("OutputCache", "Get(" + key + ") --> " + typeName + ", CacheInternal");
            }
            else {
                Debug.Trace("OutputCache", "Get(" + key + ") --> " + result.GetType().Name + ", " + provider.Name);
#endif
            }
            return result;
        }

        // lookup fragment
        internal static Object GetFragment(String key, String providerName) {
            // if providerName is null, use default provider.
            // if providerName is not null, get it from the provider collection.
            // if it's not in the provider or the default provider is undefined,
            // check the internal cache (we don't know where it is).
            Object result = null;
            OutputCacheProvider provider = GetFragmentProvider(providerName);
            if (provider != null) {
                result = provider.Get(key);
                PartialCachingCacheEntry fragment = result as PartialCachingCacheEntry;
                if (fragment != null) {
                    if (HasDependencyChanged(true /*isFragment*/, fragment._dependenciesKey, fragment._dependencies, null /*kernelKey*/, key, provider.Name)) {
                        OutputCache.RemoveFragment(key, provider.Name);
#if DBG
                        Debug.Trace("OutputCache", "GetFragment(" + key + "," + providerName + ") --> null, " + providerName);
#endif
                        return null;
                    }
                }
            }

            if (result == null) {
                result = HttpRuntime.CacheInternal.Get(key);
#if DBG
                string typeName = (result != null) ? result.GetType().Name : "null";                
                Debug.Trace("OutputCache", "GetFragment(" + key + "," + providerName + ") --> " + typeName + ", CacheInternal");
            }
            else {
                Debug.Trace("OutputCache", "GetFragment(" + key + "," + providerName + ") --> " + result.GetType().Name + ", " + providerName);
#endif
            }
            return result;
        }

        // remove cache vary
        // remove entry
        internal static void Remove(String key, HttpContext context) {
            // we don't know if it's in the internal cache or
            // one of the providers.  If a context is given,
            // then we can narrow down to at most one provider.
            // If the context is null, then we don't know which
            // provider and we have to check all.

            HttpRuntime.CacheInternal.Remove(key);

            if (context == null) {
                // remove from all providers since we don't know which one it's in.
                OutputCacheProviderCollection providers = Providers;
                if (providers != null) {
                    foreach (OutputCacheProvider provider in providers) {
                        provider.Remove(key);
                    }
                }
            }
            else {
                OutputCacheProvider provider = GetProvider(context);
                if (provider != null) {
                    provider.Remove(key);
                }
            }
#if DBG
            Debug.Trace("OutputCache", "Remove(" + key + ", context)");
#endif
        }

        // remove cache vary
        // remove entry
        internal static void RemoveFromProvider(String key, String providerName) {
            // we know where it is.  If providerName is given,
            // then it is in that provider.  If it's not given,
            // it's in the internal cache.
            if (providerName == null) {
                throw new ArgumentNullException("providerName");
            }

            OutputCacheProviderCollection providers = Providers;
            OutputCacheProvider provider = (providers == null) ? null : providers[providerName];
            if (provider == null) {
                throw new ProviderException(SR.GetString(SR.Provider_Not_Found, providerName));
            }

            provider.Remove(key);
#if DBG
            Debug.Trace("OutputCache", "Remove(" + key + ", " + providerName + ")");
#endif
        }

        // remove fragment
        internal static void RemoveFragment(String key, String providerName) {
            // if providerName is null, use default provider.
            // if providerName is not null, get it from the provider collection.
            // remove it from the provider and the internal cache (we don't know where it is).
            OutputCacheProvider provider = GetFragmentProvider(providerName);
            if (provider != null) {
                provider.Remove(key);
            }
            HttpRuntime.CacheInternal.Remove(key);
#if DBG
            Debug.Trace("OutputCache", "RemoveFragment(" + key + "," + providerName + ")");
#endif
        }

        // insert fragment
        internal static void InsertFragment(String cachedVaryKey, ControlCachedVary cachedVary,
                                            String fragmentKey, PartialCachingCacheEntry fragment,
                                            CacheDependency dependencies,
                                            DateTime absExp, TimeSpan slidingExp,
                                            String providerName) {

            // if providerName is not null, find the provider in the collection.
            // if providerName is null, use default provider.
            // if the default provider is undefined or the fragment can't be inserted in the
            // provider, insert it in the internal cache.
            OutputCacheProvider provider = GetFragmentProvider(providerName);

            //
            // ControlCachedVary and PartialCachingCacheEntry can be serialized
            //

            bool useProvider = (provider != null);
            if (useProvider) {
                bool canUseProvider = (slidingExp == Cache.NoSlidingExpiration
                                       && (dependencies == null || dependencies.IsFileDependency()));
            
                if (useProvider && !canUseProvider) {
                    throw new ProviderException(SR.GetString(SR.Provider_does_not_support_policy_for_fragments, providerName));
                }
            }

#if DBG
            bool cachedVaryPutInCache = (cachedVary != null);
#endif
            if (cachedVary != null) {
                // Add the ControlCachedVary item so that a request will know
                // which varies are needed to issue another request.

                // Use the Add method so that we guarantee we only use
                // a single ControlCachedVary and don't overwrite existing ones.
                ControlCachedVary cachedVaryInCache;
                if (!useProvider) {
                    cachedVaryInCache = OutputCache.UtcAdd(cachedVaryKey, cachedVary);
                }
                else {
                    cachedVaryInCache = (ControlCachedVary) provider.Add(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                }
                
                if (cachedVaryInCache != null) {
                    if (!cachedVary.Equals(cachedVaryInCache)) {
                        // overwrite existing cached vary
                        if (!useProvider) {
                            HttpRuntime.CacheInternal.UtcInsert(cachedVaryKey, cachedVary);
                        }
                        else {
                            provider.Set(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                        }
                    }
                    else {
                        cachedVary = cachedVaryInCache;
#if DBG
                        cachedVaryPutInCache = false;
#endif
                    }
                }

                if (!useProvider) {
                    AddCacheKeyToDependencies(ref dependencies, cachedVaryKey);
                }

                // not all caches support cache key dependencies, but we can use a "change number" to associate
                // the ControlCachedVary and the PartialCachingCacheEntry
                fragment._cachedVaryId = cachedVary.CachedVaryId;
            }

            // Now insert into the cache (use cache provider if possible, otherwise use internal cache)
            if (!useProvider) {
                HttpRuntime.CacheInternal.UtcInsert(fragmentKey, fragment,
                                                    dependencies,
                                                    absExp, slidingExp,
                                                    CacheItemPriority.Normal,
                                                    null);
            }
            else {
                string depKey = null;
                if (dependencies != null) {
                    depKey = OUTPUTCACHE_KEYPREFIX_DEPENDENCIES + dependencies.GetUniqueID();
                    fragment._dependenciesKey = depKey;
                    fragment._dependencies = dependencies.GetFileDependencies();
                }                
                provider.Set(fragmentKey, fragment, absExp);
                if (dependencies != null) {
                    // use Add and dispose dependencies if there's already one in the cache
                    Object d = HttpRuntime.CacheInternal.UtcAdd(depKey, new DependencyCacheEntry(fragmentKey, null, provider.Name),
                                                                dependencies,
                                                                absExp, Cache.NoSlidingExpiration,
                                                                CacheItemPriority.Normal, s_dependencyRemovedCallbackForFragment);
                    if (d != null) {
                        dependencies.Dispose();
                    }
                }
            }

#if DBG
            string cachedVaryType = (cachedVaryPutInCache) ? "ControlCachedVary" : "";
            string providerUsed = (useProvider) ? provider.Name : "CacheInternal";
            Debug.Trace("OutputCache", "InsertFragment(" 
                        + cachedVaryKey + ", " 
                        + cachedVaryType + ", " 
                        + fragmentKey + ", PartialCachingCacheEntry, ...) -->"
                        + providerUsed);
#endif
        }

        // insert cached vary or output cache entry
        internal static void InsertResponse(String cachedVaryKey, CachedVary cachedVary,
                                            String rawResponseKey, CachedRawResponse rawResponse, 
                                            CacheDependency dependencies, 
                                            DateTime absExp, TimeSpan slidingExp) {

            // if the provider is undefined or the fragment can't be inserted in the
            // provider, insert it in the internal cache.
            OutputCacheProvider provider = GetProvider(HttpContext.Current);

            //
            // CachedVary can be serialized.
            // CachedRawResponse is not always serializable.
            //

            bool useProvider = (provider != null);
            if (useProvider) {
                bool canUseProvider = (IsSubstBlockSerializable(rawResponse._rawResponse)
                                       && rawResponse._settings.IsValidationCallbackSerializable()
                                       && slidingExp == Cache.NoSlidingExpiration
                                       && (dependencies == null || dependencies.IsFileDependency()));

                if (useProvider && !canUseProvider) {
                    throw new ProviderException(SR.GetString(SR.Provider_does_not_support_policy_for_responses, provider.Name));
                }
            }

#if DBG
            bool cachedVaryPutInCache = (cachedVary != null);
#endif
            if (cachedVary != null) {
                /*
                 * Add the CachedVary item so that a request will know
                 * which headers are needed to issue another request.
                 * 
                 * Use the Add method so that we guarantee we only use
                 * a single CachedVary and don't overwrite existing ones.
                 */

                CachedVary cachedVaryInCache;
                if (!useProvider) {
                    cachedVaryInCache = OutputCache.UtcAdd(cachedVaryKey, cachedVary);                    
                }
                else {
                    cachedVaryInCache = (CachedVary) provider.Add(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                }

                if (cachedVaryInCache != null) {
                    if (!cachedVary.Equals(cachedVaryInCache)) {
                        if (!useProvider) {
                            HttpRuntime.CacheInternal.UtcInsert(cachedVaryKey, cachedVary);
                        }
                        else {
                            provider.Set(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                        }
                    }
                    else {
                        cachedVary = cachedVaryInCache;
#if DBG
                        cachedVaryPutInCache = false;
#endif
                    }
                }

                if (!useProvider) {
                    AddCacheKeyToDependencies(ref dependencies, cachedVaryKey);
                }

                // not all caches support cache key dependencies, but we can use a "change number" to associate
                // the ControlCachedVary and the PartialCachingCacheEntry
                rawResponse._cachedVaryId = cachedVary.CachedVaryId;
            }

            // Now insert into the cache (use cache provider if possible, otherwise use internal cache)
            if (!useProvider) {
                HttpRuntime.CacheInternal.UtcInsert(rawResponseKey, rawResponse,
                                                    dependencies,
                                                    absExp, slidingExp,
                                                    CacheItemPriority.Normal,
                                                    s_entryRemovedCallback);

                IncrementCount();

                PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_ENTRIES);
                PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_TURNOVER_RATE);
            }
            else {
                string depKey = null;
                string[] fileDeps = null;
                if (dependencies != null) {
                    depKey = OUTPUTCACHE_KEYPREFIX_DEPENDENCIES + dependencies.GetUniqueID();
                    fileDeps = dependencies.GetFileDependencies();
                }
                OutputCacheEntry oce = Convert(rawResponse, depKey, fileDeps);
                provider.Set(rawResponseKey, oce, absExp);
                if (dependencies != null) {
                    // use Add and dispose dependencies if there's already one in the cache
                    Object d = HttpRuntime.CacheInternal.UtcAdd(depKey, new DependencyCacheEntry(rawResponseKey, oce.KernelCacheUrl, provider.Name),
                                                                dependencies,
                                                                absExp, Cache.NoSlidingExpiration,
                                                                CacheItemPriority.Normal, s_dependencyRemovedCallback);
                    if (d != null) {
                        dependencies.Dispose();
                    }
                }
            }
#if DBG
            string cachedVaryType = (cachedVaryPutInCache) ? "CachedVary" : "";
            string providerUsed = (useProvider) ? provider.Name : "CacheInternal";
            Debug.Trace("OutputCache", "InsertResposne(" 
                        + cachedVaryKey + ", " 
                        + cachedVaryType + ", " 
                        + rawResponseKey + ", CachedRawResponse, ...) -->"
                        + providerUsed);
#endif
        }
    }
}
