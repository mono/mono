//---------------------------------------------------------------------
// <copyright file="ObjectItemCachedAssemblyLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Reflection;

    internal sealed class ObjectItemCachedAssemblyLoader : ObjectItemAssemblyLoader
    {
        private new ImmutableAssemblyCacheEntry CacheEntry { get { return (ImmutableAssemblyCacheEntry)base.CacheEntry; } }

        internal ObjectItemCachedAssemblyLoader(Assembly assembly, ImmutableAssemblyCacheEntry cacheEntry, ObjectItemLoadingSessionData sessionData)
            : base(assembly, cacheEntry, sessionData)
        {
        }

        protected override void AddToAssembliesLoaded()
        {
            // wasn't loaded, was pulled from cache instead
            // so don't load it
        }


        protected override void LoadTypesFromAssembly()
        {
            foreach (EdmType type in CacheEntry.TypesInAssembly)
            {
                if (!SessionData.TypesInLoading.ContainsKey(type.Identity))
                {
                    SessionData.TypesInLoading.Add(type.Identity, type);
                }
            }
        }


    }
}
