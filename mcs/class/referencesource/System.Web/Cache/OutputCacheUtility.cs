//------------------------------------------------------------------------------
// <copyright file="OutputCacheUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

 /*
 * Output Cache Ulitity Class.  Wraps methods to be used by Out of Band OutputCache Module
 * 
 * Copyright (c) 2016 Microsoft Corporation
 */

 namespace System.Web.Caching {
    using Collections;
    using Collections.Generic;

     /// <summary>
    ///  Utility Class that allows out of band OutputCache to access FX internal only methods in order to complete the outputCache functionality 
    /// </summary>

     public static class OutputCacheUtility {
        /// <summary>
        /// If the response can be kernel cached, return the kernel cache key;
        /// otherwise return null.  The kernel cache key is used to invalidate
        /// the entry if a dependency changes or the item is flushed from the
        /// managed cache for any reason.
        /// </summary>
        /// <param name="originalCacheUrl"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string SetupKernelCaching(String originalCacheUrl, HttpResponse response) {
#if FEATURE_PAL
            return response.SetupKernelCaching(originalCacheUrl);
#else
            return null;
#endif
        }

         /// <summary>
        /// Expose the mothod to flush kernel cache for Out of Band Module
        /// </summary>
        /// <param name="cacheEntryKey">The kernel cache key</param>
        public static void FlushKernelCache(string cacheKey) {
#if !FEATURE_PAL            
            Hosting.UnsafeIISMethods.MgdFlushKernelCache(cacheKey);
#endif
        }

         /// <summary>
        /// Create cache dependency for response and return the dependency
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static CacheDependency CreateCacheDependency(HttpResponse response) {
            return response.CreateCacheDependencyForResponse();
        }

         /// <summary>
        ///  Get response content buffers from the response and return as ArrayList
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ArrayList GetContentBuffers(HttpResponse response) {
            return response.GetSnapshot().Buffers;
        }

         /// <summary>
        /// Set the content buffers of a response from the given ArrayList
        /// </summary>
        /// <param name="response"></param>
        /// <param name="buffers"></param>
        public static void SetContentBuffers(HttpResponse response, ArrayList buffers) {
            response.SetResponseBuffers(buffers);
        }

         /// <summary>
        /// Get the CachePolicy ValidationCallbackInfo within the response as KeyValuePair collections
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<HttpCacheValidateHandler, object>> GetValidationCallbacks(HttpResponse response) {
            var list = new List<KeyValuePair<HttpCacheValidateHandler, object>>();

             foreach (ValidationCallbackInfo cb in response.Cache.GetValidationCallbacks()) {
                list.Add(new KeyValuePair<HttpCacheValidateHandler, object>(cb.handler, cb.data));
            }

             return list;
        }
    }
}