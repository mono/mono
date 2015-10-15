//---------------------------------------------------------------------
// <copyright file="LockedAssemblyCache.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    internal class LockedAssemblyCache : IDisposable
    {
        private object _lockObject;
        private Dictionary<Assembly, ImmutableAssemblyCacheEntry> _globalAssemblyCache;
        internal LockedAssemblyCache(object lockObject, Dictionary<Assembly, ImmutableAssemblyCacheEntry> globalAssemblyCache)
        {
            _lockObject = lockObject;
            _globalAssemblyCache = globalAssemblyCache;
#pragma warning disable 0618
            //@
            Monitor.Enter(_lockObject);
#pragma warning restore 0618
        }

        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            Monitor.Exit(_lockObject);
            _lockObject = null;
            _globalAssemblyCache = null;
        }

        [Conditional("DEBUG")]
        private void AssertLockedByThisThread()
        {
            bool entered = false;
            Monitor.TryEnter(_lockObject, ref entered);
            if (entered)
            {
                Monitor.Exit(_lockObject);
            }

            Debug.Assert(entered, "The cache is being accessed by a thread that isn't holding the lock");
        }

        internal bool TryGetValue(Assembly assembly, out ImmutableAssemblyCacheEntry cacheEntry)
        {
            AssertLockedByThisThread();
            return _globalAssemblyCache.TryGetValue(assembly, out cacheEntry);
        }

        internal void Add(Assembly assembly, ImmutableAssemblyCacheEntry assemblyCacheEntry)
        {
            AssertLockedByThisThread();
            _globalAssemblyCache.Add(assembly, assemblyCacheEntry);
        }

        internal void Clear()
        {
            AssertLockedByThisThread();
            _globalAssemblyCache.Clear();
        }
    }
}
