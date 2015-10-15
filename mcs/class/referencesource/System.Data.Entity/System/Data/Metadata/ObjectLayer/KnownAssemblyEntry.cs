//---------------------------------------------------------------------
// <copyright file="KnownAssemblyEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    internal sealed class KnownAssemblyEntry
    {
        private readonly AssemblyCacheEntry _cacheEntry;
        private bool _referencedAssembliesAreLoaded;
        private bool _seenWithEdmItemCollection;

        internal KnownAssemblyEntry(AssemblyCacheEntry cacheEntry, bool seenWithEdmItemCollection)
        {
            Debug.Assert(cacheEntry != null, "Found a null cacheEntry");
            _cacheEntry = cacheEntry;
            _referencedAssembliesAreLoaded = false;
            _seenWithEdmItemCollection = seenWithEdmItemCollection;
        }

        internal AssemblyCacheEntry CacheEntry
        {
            get { return _cacheEntry; }
        }

        public bool ReferencedAssembliesAreLoaded
        {
            get { return _referencedAssembliesAreLoaded; }
            set { _referencedAssembliesAreLoaded = value; }
        }

        public bool SeenWithEdmItemCollection
        {
            get { return _seenWithEdmItemCollection; }
            set { _seenWithEdmItemCollection = value; }
        }

        public bool HaveSeenInCompatibleContext(object loaderCookie, EdmItemCollection itemCollection)
        {
            // a new "context" is only when we have not seen this assembly with an itemCollection that is non-null
            // and we now have a non-null itemCollection, and we are not already in AttributeLoader mode.
            return SeenWithEdmItemCollection ||
                   itemCollection == null ||
                   ObjectItemAssemblyLoader.IsAttributeLoader(loaderCookie);
        }
    }
}
