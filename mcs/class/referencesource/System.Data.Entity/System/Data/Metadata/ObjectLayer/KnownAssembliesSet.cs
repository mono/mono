//---------------------------------------------------------------------
// <copyright file="KnownAssembliesSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// This class is responsible for keeping track of which assemblies we have already 
    /// considered so we don't reconsider them again. 
    /// 
    /// The current rules for an assembly to be "seen" is
    ///     1. It is already in our dictionary
    ///     AND
    ///         1.  We are in attribute loading mode
    ///         OR
    ///         2. We have seen it already with a non null EdmItemCollection
    ///         OR
    ///         3. We are seeing it with a null EdmItemCollection this time
    /// </summary>
    internal class KnownAssembliesSet 
    {
        private Dictionary<Assembly, KnownAssemblyEntry> _assemblies;
        internal KnownAssembliesSet()
        {
            _assemblies = new Dictionary<Assembly, KnownAssemblyEntry>();
        }

        internal KnownAssembliesSet(KnownAssembliesSet set)
        {
            _assemblies = new Dictionary<Assembly, KnownAssemblyEntry>(set._assemblies);
        }

        internal bool TryGetKnownAssembly(Assembly assembly, object loaderCookie, EdmItemCollection itemCollection, out KnownAssemblyEntry entry)
        {
            if (!_assemblies.TryGetValue(assembly, out entry))
            {
                return false;
            }

            if (!entry.HaveSeenInCompatibleContext(loaderCookie, itemCollection))
            {
                return false;
            }

            return true;
        }

        internal IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies.Keys; }
        }

        public IEnumerable<KnownAssemblyEntry> GetEntries(object loaderCookie, EdmItemCollection itemCollection)
        {
            return _assemblies.Values.Where(e => e.HaveSeenInCompatibleContext(loaderCookie, itemCollection));
        }

        internal bool Contains(Assembly assembly, object loaderCookie, EdmItemCollection itemCollection)
        {
            KnownAssemblyEntry entry;
            return TryGetKnownAssembly(assembly, loaderCookie, itemCollection, out entry);
        }

        internal void Add(Assembly assembly, KnownAssemblyEntry knownAssemblyEntry)
        {
            KnownAssemblyEntry current;
            if (_assemblies.TryGetValue(assembly, out current))
            {
                Debug.Assert(current.SeenWithEdmItemCollection != knownAssemblyEntry.SeenWithEdmItemCollection &&
                    knownAssemblyEntry.SeenWithEdmItemCollection, "should only be updating if we haven't seen it with an edmItemCollection yet.");
                _assemblies[assembly] = knownAssemblyEntry;
            }
            else
            {
                _assemblies.Add(assembly, knownAssemblyEntry);
            }
        }
    }
}
