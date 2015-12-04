//---------------------------------------------------------------------
// <copyright file="ObjectItemAssemblyLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Reflection;

    internal abstract class ObjectItemAssemblyLoader
    {
        protected const BindingFlags PropertyReflectionBindingFlags = BindingFlags.DeclaredOnly |
                                                                     BindingFlags.Instance |
                                                                     BindingFlags.Public |
                                                                     BindingFlags.NonPublic;

        private readonly ObjectItemLoadingSessionData _sessionData;
        private Assembly _assembly;
        private AssemblyCacheEntry _cacheEntry;

        protected ObjectItemAssemblyLoader(Assembly assembly, AssemblyCacheEntry cacheEntry, ObjectItemLoadingSessionData sessionData)
        {
            _assembly = assembly;
            _cacheEntry = cacheEntry;
            _sessionData = sessionData;
        }

        internal virtual void Load()
        {
            AddToAssembliesLoaded();

            LoadTypesFromAssembly();

            AddToKnownAssemblies();

            LoadClosureAssemblies();
        }

        protected abstract void AddToAssembliesLoaded();
        protected abstract void LoadTypesFromAssembly();
        protected virtual void LoadClosureAssemblies()
        {
            LoadAssemblies(CacheEntry.ClosureAssemblies, SessionData);
        }

        internal virtual void OnLevel1SessionProcessing() { }
        internal virtual void OnLevel2SessionProcessing() { }

        internal static ObjectItemAssemblyLoader CreateLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
        {
            ImmutableAssemblyCacheEntry cacheEntry;

            // KnownAssembly -> NoOp
            // Inside the LockedAssemblyCache means it is an attribute based assembly -> Cachedassembly
            // Inside the OcCache on EdmItemCollection -> cachedassembly
            // If none of above, setup the LoaderFactory based on the current assembly and EdmItemCollection
            if (sessionData.KnownAssemblies.Contains(assembly, sessionData.ObjectItemAssemblyLoaderFactory, sessionData.EdmItemCollection))
            {
                return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
            }
            else if (sessionData.LockedAssemblyCache.TryGetValue(assembly, out cacheEntry))
            {
                if (sessionData.ObjectItemAssemblyLoaderFactory == null)
                {
                    if (cacheEntry.TypesInAssembly.Count != 0)
                    {
                        // we are loading based on attributes now
                        sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemAttributeAssemblyLoader.Create;
                    }
                    // if types in assembly are 0, don't commit to any loader yet
                }
                else if (sessionData.ObjectItemAssemblyLoaderFactory != ObjectItemAttributeAssemblyLoader.Create)
                {
                    // we were loading in convention mode, and ran into an assembly that can't be loaded by convention
                    // we know this because all cached assemblies are attribute based at the moment.
                    sessionData.EdmItemErrors.Add(new EdmItemError(Strings.Validator_OSpace_Convention_AttributeAssemblyReferenced(assembly.FullName), null));
                }
                return new ObjectItemCachedAssemblyLoader(assembly, cacheEntry, sessionData);
            }
            else if (sessionData.EdmItemCollection != null &&
                    sessionData.EdmItemCollection.ConventionalOcCache.TryGetConventionalOcCacheFromAssemblyCache(
                        assembly, out cacheEntry))
            {
                sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemConventionAssemblyLoader.Create;
                return new ObjectItemCachedAssemblyLoader(assembly, cacheEntry, sessionData);
            }
            else if (sessionData.ObjectItemAssemblyLoaderFactory == null)
            {
                if (ObjectItemAttributeAssemblyLoader.IsSchemaAttributePresent(assembly))
                {
                    sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemAttributeAssemblyLoader.Create;
                }
                else if (ObjectItemConventionAssemblyLoader.SessionContainsConventionParameters(sessionData))
                {
                    sessionData.ObjectItemAssemblyLoaderFactory = ObjectItemConventionAssemblyLoader.Create;
                }
            }

            if (sessionData.ObjectItemAssemblyLoaderFactory != null)
            {
                return sessionData.ObjectItemAssemblyLoaderFactory(assembly, sessionData);
            }

            return new ObjectItemNoOpAssemblyLoader(assembly, sessionData);
            
        }

        internal static bool IsAttributeLoader(object loaderCookie)
        {
            Debug.Assert(loaderCookie == null || loaderCookie is Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>, "Non loader cookie passed in");
            return IsAttributeLoader(loaderCookie as Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>);
        }

        internal static bool IsAttributeLoader(Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> loaderFactory)
        {
            if (loaderFactory == null)
            {
                return false;
            }

            return loaderFactory == ObjectItemAttributeAssemblyLoader.Create; 
        }

        internal static bool IsConventionLoader(Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> loaderFactory)
        {
            if (loaderFactory == null)
            {
                return false;
            }

            return loaderFactory == ObjectItemConventionAssemblyLoader.Create;
        }

        protected virtual void AddToKnownAssemblies()
        {
            Debug.Assert(!_sessionData.KnownAssemblies.Contains(_assembly, SessionData.ObjectItemAssemblyLoaderFactory, _sessionData.EdmItemCollection), "This assembly must not be present in the list of known assemblies");
            _sessionData.KnownAssemblies.Add(_assembly, new KnownAssemblyEntry(CacheEntry, SessionData.EdmItemCollection != null));
        }

        protected static void LoadAssemblies(IEnumerable<Assembly> assemblies, ObjectItemLoadingSessionData sessionData)
        {
            foreach (Assembly assembly in assemblies)
            {
                ObjectItemAssemblyLoader loader = ObjectItemAssemblyLoader.CreateLoader(assembly, sessionData);
                loader.Load();
            }
        }

        protected bool TryGetPrimitiveType(Type type, out PrimitiveType primitiveType)
        {
            return ClrProviderManifest.Instance.TryGetPrimitiveType(Nullable.GetUnderlyingType(type) ?? type, out primitiveType);
        }



        protected ObjectItemLoadingSessionData SessionData { get { return _sessionData; } }
        protected Assembly SourceAssembly { get { return _assembly; } }
        protected AssemblyCacheEntry CacheEntry { get { return _cacheEntry; } }

    }
}
