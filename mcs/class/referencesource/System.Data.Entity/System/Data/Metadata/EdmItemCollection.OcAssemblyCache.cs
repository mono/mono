//---------------------------------------------------------------------
// <copyright file="EdmItemCollection.OcAssemblyCache.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Metadata.Edm
{
    internal class OcAssemblyCache
    {
        /// <summary>
        /// cache for loaded assembly
        /// </summary>
        private Dictionary<Assembly, ImmutableAssemblyCacheEntry> _conventionalOcCache;

        internal OcAssemblyCache()
        {
            _conventionalOcCache = new Dictionary<Assembly, ImmutableAssemblyCacheEntry>();
        }

        /// <summary>
        /// Please do NOT call this method outside of AssemblyCache. Since AssemblyCache maintain the lock, 
        /// this method doesn't provide any locking mechanism.
        /// </summary>
        /// <param name="assemblyToLookup"></param>
        /// <param name="cacheEntry"></param>
        /// <returns></returns>
        internal bool TryGetConventionalOcCacheFromAssemblyCache(Assembly assemblyToLookup, out ImmutableAssemblyCacheEntry cacheEntry)
        {
            cacheEntry = null;
            return _conventionalOcCache.TryGetValue(assemblyToLookup, out cacheEntry);
        }

        /// <summary>
        /// Please do NOT call this method outside of AssemblyCache. Since AssemblyCache maintain the lock, 
        /// this method doesn't provide any locking mechanism.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="cacheEntry"></param>
        internal void AddAssemblyToOcCacheFromAssemblyCache(Assembly assembly, ImmutableAssemblyCacheEntry cacheEntry)
        {
            if (_conventionalOcCache.ContainsKey(assembly))
            {
                // we shouldn't update the cache if we already have one
                return;
            }
            _conventionalOcCache.Add(assembly, cacheEntry);
        }

    }
}
