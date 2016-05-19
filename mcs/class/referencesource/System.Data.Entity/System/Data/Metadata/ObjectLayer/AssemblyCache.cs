//---------------------------------------------------------------------
// <copyright file="AssemblyCache.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Diagnostics;
using System.Reflection;

namespace System.Data.Metadata.Edm
{
    internal static class AssemblyCache
    {
        // Global Assembly Cache
        private readonly static Dictionary<Assembly, ImmutableAssemblyCacheEntry> s_globalAssemblyCache = new Dictionary<Assembly, ImmutableAssemblyCacheEntry>();
        private static object _assemblyCacheLock = new object();

        //List of assemblies having view gen attribute. We cache these things if we discover
        //these assemblies while looking for O-space metadata.
        private static IList<Assembly> s_viewGenAssemblies = new ThreadSafeList<Assembly>();

        internal static LockedAssemblyCache AquireLockedAssemblyCache()
        {
            return new LockedAssemblyCache(_assemblyCacheLock, s_globalAssemblyCache);
        }
        
        internal static void LoadAssembly(Assembly assembly, bool loadReferencedAssemblies,
            KnownAssembliesSet knownAssemblies, out Dictionary<string, EdmType> typesInLoading, out List<EdmItemError> errors)
        {
            object loaderCookie = null;
            LoadAssembly(assembly, loadReferencedAssemblies, knownAssemblies, null, null, ref loaderCookie, out typesInLoading, out errors);
        }

        internal static void LoadAssembly(Assembly assembly, bool loadReferencedAssemblies,
            KnownAssembliesSet knownAssemblies, EdmItemCollection edmItemCollection, Action<String> logLoadMessage, ref object loaderCookie, out Dictionary<string, EdmType> typesInLoading, out List<EdmItemError> errors)
        {
            Debug.Assert(loaderCookie == null || loaderCookie is Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>, "This is a bad loader cookie");
            typesInLoading = null;
            errors = null;

            using (LockedAssemblyCache lockedAssemblyCache = AssemblyCache.AquireLockedAssemblyCache())
            {
                ObjectItemLoadingSessionData loadingData = new ObjectItemLoadingSessionData(knownAssemblies, lockedAssemblyCache, edmItemCollection, logLoadMessage, loaderCookie);

                LoadAssembly(assembly, loadReferencedAssemblies, loadingData);
                loaderCookie = loadingData.LoaderCookie;
                // resolve references to top level types (base types, navigation properties returns and associations, and complex type properties)
                loadingData.CompleteSession();

                if (loadingData.EdmItemErrors.Count == 0)
                {
                    // do the validation for the all the new types
                    // Now, perform validation on all the new types
                    EdmValidator validator = new EdmValidator();
                    validator.SkipReadOnlyItems = true;
                    validator.Validate(loadingData.TypesInLoading.Values, loadingData.EdmItemErrors);
                    // Update the global cache if there are no errors
                    if (loadingData.EdmItemErrors.Count == 0)
                    {
                        if (ObjectItemAssemblyLoader.IsAttributeLoader(loadingData.ObjectItemAssemblyLoaderFactory))
                        {
                            // we only cache items from the attribute loader globally, the 
                            // items loaded by convention will change depending on the cspace 
                            // provided.  cspace will have a cache of it's own for assemblies
                            UpdateCache(lockedAssemblyCache, loadingData.AssembliesLoaded);
                        }
                        else if (loadingData.EdmItemCollection != null && 
                                ObjectItemAssemblyLoader.IsConventionLoader(loadingData.ObjectItemAssemblyLoaderFactory))
                        {
                            UpdateCache(loadingData.EdmItemCollection, loadingData.AssembliesLoaded);
                        }
                    }
                }

                if (loadingData.TypesInLoading.Count > 0)
                {
                    foreach (EdmType edmType in loadingData.TypesInLoading.Values)
                    {
                        edmType.SetReadOnly();
                    }
                }

                // Update the out parameters once you are done with loading
                typesInLoading = loadingData.TypesInLoading;
                errors = loadingData.EdmItemErrors;
            }
        }

        private static void LoadAssembly(Assembly assembly, bool loadReferencedAssemblies, ObjectItemLoadingSessionData loadingData)
        {
            // Check if the assembly is already loaded
            KnownAssemblyEntry entry;
            bool shouldLoadReferences = false;
            if (loadingData.KnownAssemblies.TryGetKnownAssembly(assembly, loadingData.ObjectItemAssemblyLoaderFactory, loadingData.EdmItemCollection, out entry))
            {
                shouldLoadReferences = !entry.ReferencedAssembliesAreLoaded && loadReferencedAssemblies;
            }
            else
            {
                ObjectItemAssemblyLoader loader = ObjectItemAssemblyLoader.CreateLoader(assembly, loadingData);
                loader.Load();
                shouldLoadReferences = loadReferencedAssemblies;
            }

            if (shouldLoadReferences)
            {
                if (entry == null && loadingData.KnownAssemblies.TryGetKnownAssembly(assembly, loadingData.ObjectItemAssemblyLoaderFactory, loadingData.EdmItemCollection, out entry) ||
                    entry != null)
                {
                    entry.ReferencedAssembliesAreLoaded = true;
                }
                Debug.Assert(entry != null, "we should always have an entry, why don't we?");

                // We will traverse through all the statically linked assemblies and their dependencies.
                // Only assemblies with the EdmSchemaAttribute will be loaded and rest will be ignored

                // Even if the schema attribute is missing, we should still check all the dependent assemblies
                // any of the dependent assemblies can have the schema attribute

                // After the given assembly has been loaded, check on the flag in _knownAssemblies to see if it has already
                // been recursively loaded. The flag can be true if it was already loaded before this function was called
                foreach (Assembly referencedAssembly in MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(assembly))
                {
                    // filter out "known" assemblies to prevent unnecessary loading
                    // recursive call
                    LoadAssembly(referencedAssembly, loadReferencedAssemblies, loadingData);
                }
            }
        }


        private static void UpdateCache(EdmItemCollection edmItemCollection, Dictionary<Assembly, MutableAssemblyCacheEntry> assemblies)
        {
            foreach (var entry in assemblies)
            {
                edmItemCollection.ConventionalOcCache.AddAssemblyToOcCacheFromAssemblyCache(
                    entry.Key, new ImmutableAssemblyCacheEntry(entry.Value));
            }
        }
        
        private static void UpdateCache(LockedAssemblyCache lockedAssemblyCache, Dictionary<Assembly, MutableAssemblyCacheEntry> assemblies)
        {
            
            foreach (KeyValuePair<Assembly, MutableAssemblyCacheEntry> entry in assemblies)
            {
                // Add all the assemblies from the loading context to the global cache
                lockedAssemblyCache.Add(entry.Key, new ImmutableAssemblyCacheEntry(entry.Value));
            }
        }

        internal static IList<Assembly> ViewGenerationAssemblies
        {
            get
            {
                return s_viewGenAssemblies;
            }
        }
    }
}
