//------------------------------------------------------------------------------
// <copyright file="CacheStoreProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 /*
 * CacheStoreProvider class
 *
 * Copyright (c) 2016 Microsoft Corporation
 */

 namespace System.Web.Caching {
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration.Provider;

     /// <devdoc>
    ///    <para>The base interface required of any cache store that wants to be plugged in as the default object
    ///       cache implementation used by HttpRuntime.Cache.</para>
    /// </devdoc>
    public abstract class CacheStoreProvider : ProviderBase, IDisposable {    // And IEnumerable-ish, but without the explicit declaration up top
        // Properties
        public abstract long ItemCount { get; }
        public abstract long SizeInBytes { get; }

         public new abstract void Initialize(string name, NameValueCollection config);

         /// <devdoc>
        ///    <para>Add will put an item into the cache, but not replace it if the key already exists.</para>
        /// </devdoc>
        public abstract object Add(string key, object item, CacheInsertOptions options);

         /// <devdoc>
        ///    <para>Get will retrieve an item from the cache if it exists.</para>
        /// </devdoc>
        public abstract object Get(string key);

         /// <devdoc>
        ///    <para>Insert will put an item into the cache and replace any pre-existing item with the same key.</para>
        /// </devdoc>
        public abstract void Insert(string key, object item, CacheInsertOptions options);

         /// <devdoc>
        ///    <para>Remove will remove an item from the cache if it exists.</para>
        /// </devdoc>
        public abstract object Remove(string key);

         /// <devdoc>
        ///    <para>Remove will remove an item from the cache if it exists.</para>
        /// </devdoc>
        public abstract object Remove(string key, CacheItemRemovedReason reason);

         /// <devdoc>
        ///    <para>Trim will remove the requested percentage of entries from the cache store.</para>
        /// </devdoc>
        public abstract long Trim(int percent);

         public abstract bool AddDependent(string key, CacheDependency dependency, out DateTime utcLastUpdated);
        public abstract void RemoveDependent(string key, CacheDependency dependency);

         public abstract void Dispose();

         // IEnumerable - Not technically the interface. But CacheStores are exposed via an
        // enumerator through System.Web.Cache, so we put the burden of creating one on each CacheStore.
        public abstract IDictionaryEnumerator GetEnumerator();
    }
}