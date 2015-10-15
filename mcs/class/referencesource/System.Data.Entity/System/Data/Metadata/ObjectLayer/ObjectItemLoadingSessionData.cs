//---------------------------------------------------------------------
// <copyright file="ObjectItemLoadingSessionData.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    internal sealed class ObjectItemLoadingSessionData
    {
        private Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> _loaderFactory;

        // all the types that we encountered while loading - this may contain types from various assemblies
        private readonly Dictionary<string, EdmType> _typesInLoading;
        
        // 

        private bool _conventionBasedRelationshipsAreLoaded = false;

        private LoadMessageLogger _loadMessageLogger;

        // list of errors encountered during loading
        private readonly List<EdmItemError> _errors;

        // keep the list of new assemblies that got loaded in this load assembly call. The reason why we need to keep a seperate
        // list of assemblies is that we keep track of errors, and if there are no errors, only then do we add the list of assemblies
        // to the global cache. Hence global cache is never polluted with invalid assemblies
        private readonly Dictionary<Assembly, MutableAssemblyCacheEntry> _listOfAssembliesLoaded = new Dictionary<Assembly, MutableAssemblyCacheEntry>();

        // List of known assemblies - this list is initially passed by the caller and we keep adding to it, as and when we load
        // an assembly
        private readonly KnownAssembliesSet _knownAssemblies;
        private readonly LockedAssemblyCache _lockedAssemblyCache;
        private readonly HashSet<ObjectItemAssemblyLoader> _loadersThatNeedLevel1PostSessionProcessing;
        private readonly HashSet<ObjectItemAssemblyLoader> _loadersThatNeedLevel2PostSessionProcessing;

        private readonly EdmItemCollection _edmItemCollection;
        private Dictionary<string, KeyValuePair<EdmType, int>> _conventionCSpaceTypeNames;
        private Dictionary<EdmType, EdmType> _cspaceToOspace;
        private object _originalLoaderCookie;
        internal Dictionary<string, EdmType> TypesInLoading { get { return _typesInLoading; } }
        internal Dictionary<Assembly, MutableAssemblyCacheEntry> AssembliesLoaded { get { return _listOfAssembliesLoaded; } }
        internal List<EdmItemError> EdmItemErrors { get { return _errors; } }
        internal KnownAssembliesSet KnownAssemblies { get { return _knownAssemblies; } }
        internal LockedAssemblyCache LockedAssemblyCache { get { return _lockedAssemblyCache; } }
        internal EdmItemCollection EdmItemCollection { get { return _edmItemCollection; } }
        internal Dictionary<EdmType, EdmType> CspaceToOspace { get { return _cspaceToOspace; } }
        internal bool ConventionBasedRelationshipsAreLoaded 
        { 
            get { return _conventionBasedRelationshipsAreLoaded;  }
            set { _conventionBasedRelationshipsAreLoaded = value; }
        }

        internal LoadMessageLogger LoadMessageLogger
        { 
            get 
            { 
                return this._loadMessageLogger; 
            }
        }

        // dictionary of types by name (not including namespace), we also track duplicate names
        // so if one of those types is used we can log an error
        internal Dictionary<string, KeyValuePair<EdmType, int>> ConventionCSpaceTypeNames 
        { 
            get 
            {
                if (_edmItemCollection != null && _conventionCSpaceTypeNames == null)
                {
                    _conventionCSpaceTypeNames = new Dictionary<string, KeyValuePair<EdmType, int>>();

                    // create the map and cache it
                    foreach (var edmType in _edmItemCollection.GetItems<EdmType>())
                    {
                        if ((edmType is StructuralType && edmType.BuiltInTypeKind != BuiltInTypeKind.AssociationType) || Helper.IsEnumType(edmType))
                        {

                            KeyValuePair<EdmType, int> pair;
                            if (_conventionCSpaceTypeNames.TryGetValue(edmType.Name, out pair))
                            {
                                _conventionCSpaceTypeNames[edmType.Name] = new KeyValuePair<EdmType, int>(pair.Key, pair.Value + 1);
                            }
                            else
                            {
                                pair = new KeyValuePair<EdmType, int>(edmType, 1);
                                _conventionCSpaceTypeNames.Add(edmType.Name, pair);
                            }
                        }
                    }
                }
                return _conventionCSpaceTypeNames;
            } 
        }

        internal Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader> ObjectItemAssemblyLoaderFactory
        {
            get { return _loaderFactory; }
            set
            {
                if (_loaderFactory != value)
                {
                    Debug.Assert(_loaderFactory == null || _typesInLoading.Count == 0, "Only reset the factory after types have not been loaded or load from the cache");
                    _loaderFactory = value;
                }
            }
        }

        internal object LoaderCookie
        {
            get
            {
                // be sure we get the same factory/cookie as we had before... if we had one
                if (_originalLoaderCookie != null)
                {
                    Debug.Assert(_loaderFactory == null ||
                                 (object)_loaderFactory == _originalLoaderCookie, "The loader factory should determine the next loader, so we should always have the same loader factory");
                    return _originalLoaderCookie;
                }

                return _loaderFactory;
            }
        }
        internal ObjectItemLoadingSessionData(KnownAssembliesSet knownAssemblies, LockedAssemblyCache lockedAssemblyCache, EdmItemCollection edmItemCollection, Action<String> logLoadMessage, object loaderCookie)
        {
            Debug.Assert(loaderCookie == null || loaderCookie is Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>, "This is a bad loader cookie");
            
            _typesInLoading = new Dictionary<string, EdmType>(StringComparer.Ordinal);
            _errors = new List<EdmItemError>();
            _knownAssemblies = knownAssemblies;
            _lockedAssemblyCache = lockedAssemblyCache;
            _loadersThatNeedLevel1PostSessionProcessing = new HashSet<ObjectItemAssemblyLoader>();
            _loadersThatNeedLevel2PostSessionProcessing = new HashSet<ObjectItemAssemblyLoader>();
            _edmItemCollection = edmItemCollection;
            _loadMessageLogger = new LoadMessageLogger(logLoadMessage);
            _cspaceToOspace = new Dictionary<EdmType, EdmType>();
            _loaderFactory = (Func<Assembly, ObjectItemLoadingSessionData, ObjectItemAssemblyLoader>)loaderCookie;
            _originalLoaderCookie = loaderCookie;
            if (_loaderFactory == ObjectItemConventionAssemblyLoader.Create && _edmItemCollection != null)
            {
                foreach (KnownAssemblyEntry entry in _knownAssemblies.GetEntries(_loaderFactory, edmItemCollection))
                {
                    foreach (EdmType type in entry.CacheEntry.TypesInAssembly.OfType<EdmType>())
                    {
                        if (Helper.IsEntityType(type))
                        {
                            ClrEntityType entityType = (ClrEntityType)type;
                            _cspaceToOspace.Add(_edmItemCollection.GetItem<StructuralType>(entityType.CSpaceTypeName), entityType);
                        }
                        else if (Helper.IsComplexType(type))
                        {
                            ClrComplexType complexType = (ClrComplexType)type;
                            _cspaceToOspace.Add(_edmItemCollection.GetItem<StructuralType>(complexType.CSpaceTypeName), complexType);
                        }
                        else if(Helper.IsEnumType(type))
                        {
                            ClrEnumType enumType = (ClrEnumType)type;
                            _cspaceToOspace.Add(_edmItemCollection.GetItem<EnumType>(enumType.CSpaceTypeName), enumType);
                        }
                        else
                        {
                            Debug.Assert(Helper.IsAssociationType(type));
                            _cspaceToOspace.Add(_edmItemCollection.GetItem<StructuralType>(type.FullName), type);
                        }
                    }
                }
            }

        }

        internal void RegisterForLevel1PostSessionProcessing(ObjectItemAssemblyLoader loader)
        {
            _loadersThatNeedLevel1PostSessionProcessing.Add(loader);
        }

        internal void RegisterForLevel2PostSessionProcessing(ObjectItemAssemblyLoader loader)
        {
            _loadersThatNeedLevel2PostSessionProcessing.Add(loader);
        }
        
        internal void CompleteSession()
        {
            foreach (ObjectItemAssemblyLoader loader in _loadersThatNeedLevel1PostSessionProcessing)
            {
                loader.OnLevel1SessionProcessing();
            }

            foreach (ObjectItemAssemblyLoader loader in _loadersThatNeedLevel2PostSessionProcessing)
            {
                loader.OnLevel2SessionProcessing();
            }
        }
    }
}
