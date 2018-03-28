//------------------------------------------------------------------------------
// <copyright file="FolderLevelBuildProviderCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Web.Compilation;

    // class CompilationSection

    [ConfigurationCollection(typeof(FolderLevelBuildProvider))]
    public sealed class FolderLevelBuildProviderCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;
        private Dictionary<FolderLevelBuildProviderAppliesTo, List<Type>> _buildProviderMappings;
        private HashSet<Type> _buildProviderTypes;

        private bool _folderLevelBuildProviderTypesSet;

        static FolderLevelBuildProviderCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }
        
        public FolderLevelBuildProviderCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public new BuildProvider this[string name] {
            get {
                return (BuildProvider)BaseGet(name);
            }
        }
        public FolderLevelBuildProvider this[int index] {
            get {
                return (FolderLevelBuildProvider)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        public void Add(FolderLevelBuildProvider buildProvider) {
            BaseAdd(buildProvider);
        }
        
        public void Remove(String name) {
            BaseRemove(name);
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public void Clear() {
            BaseClear();
        }
        
        protected override ConfigurationElement CreateNewElement() {
            return new FolderLevelBuildProvider();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((FolderLevelBuildProvider)element).Name;
        }

        // Add a mapping from appliesTo to the buildProviderType
        private void AddMapping(FolderLevelBuildProviderAppliesTo appliesTo, Type buildProviderType) {
            if (_buildProviderMappings == null) {
                _buildProviderMappings = new Dictionary<FolderLevelBuildProviderAppliesTo, List<Type>>();
            }
            if (_buildProviderTypes == null) {
                _buildProviderTypes = new HashSet<Type>();
            }
            List<Type> buildProviders = null;
            if (!_buildProviderMappings.TryGetValue(appliesTo, out buildProviders)) {
                buildProviders = new List<Type>();
                _buildProviderMappings.Add(appliesTo, buildProviders);
            }
            buildProviders.Add(buildProviderType);

            _buildProviderTypes.Add(buildProviderType);
        }

        internal List<Type> GetBuildProviderTypes(FolderLevelBuildProviderAppliesTo appliesTo) {
            EnsureFolderLevelBuildProvidersInitialized();
            var buildProviders = new List<Type>();
            if (_buildProviderMappings != null) {
                foreach (var pair in _buildProviderMappings) {
                    if ((pair.Key & appliesTo) != 0) {
                        buildProviders.AddRange(pair.Value);
                    }
                }
            }
            return buildProviders;
        }

        internal bool IsFolderLevelBuildProvider(Type t) {
            EnsureFolderLevelBuildProvidersInitialized();
            if (_buildProviderTypes != null) {
                return _buildProviderTypes.Contains(t);
            }
            return false;
        }


        // Initialize the dictionary mapping appliesTo to buildProvider types
        private void EnsureFolderLevelBuildProvidersInitialized() {
            if (!_folderLevelBuildProviderTypesSet) {
                lock (this) {
                    if (!_folderLevelBuildProviderTypesSet) {
                        foreach (FolderLevelBuildProvider buildProvider in this) {
                            AddMapping(buildProvider.AppliesToInternal, buildProvider.TypeInternal);
                        }
                        _folderLevelBuildProviderTypesSet = true;
                    }
                }
            }
        }
    }
}
