//------------------------------------------------------------------------------
// <copyright file="OutputCacheProviderAsync.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
/*
 * OutputCache Async Provider Abstract Class. Define interface for any Async OutputCache Providers
 * 
 * Copyright (c) 2016 Microsoft Corporation
 */
namespace System.Web.Caching {
    using System.Threading.Tasks;    
    /// <summary>
    /// the abstract base class implemented by all output cache Async providers
    /// </summary>
    public abstract class OutputCacheProviderAsync : OutputCacheProvider {

         /// <summary>
        /// Returns the specified entry, or null if it does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Task<object> GetAsync(String key);

         /// <summary>
        /// Inserts the specified entry into the cache if it does not already exist, otherwise returns the existing entry.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <param name="utcExpiry"></param>
        /// <returns></returns>
        public abstract Task<object> AddAsync(String key, Object entry, DateTime utcExpiry);

         /// <summary>
        /// Inserts the specified entry into the cache, overwriting an existing entry if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <param name="utcExpiry"></param>
        /// <returns></returns>
        public abstract Task SetAsync(String key, Object entry, DateTime utcExpiry);

         /// <summary>
        /// Removes the specified entry from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract Task RemoveAsync(String key);
    }
}