//------------------------------------------------------------------------------
// <copyright file="DynamicModuleRegistry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class DynamicModuleRegistry {

        private const string _moduleNameFormat = "__DynamicModule_{0}_{1}";

        private readonly List<DynamicModuleRegistryEntry> _entries = new List<DynamicModuleRegistryEntry>();
        private bool _entriesReadonly;
        private readonly object _lockObj = new object();

        public void Add(Type moduleType) {
            if (moduleType == null) {
                throw new ArgumentNullException("moduleType");
            }
            if (!typeof(IHttpModule).IsAssignableFrom(moduleType)) {
                string message = String.Format(CultureInfo.CurrentCulture, SR.GetString(SR.DynamicModuleRegistry_TypeIsNotIHttpModule), moduleType);
                throw new ArgumentException(message, "moduleType");
            }

            lock (_lockObj) {
                if (_entriesReadonly) {
                    // modules have already been initialized, e.g. Application_Start has already run
                    throw new InvalidOperationException(SR.GetString(SR.DynamicModuleRegistry_ModulesAlreadyInitialized));
                }

                _entries.Add(new DynamicModuleRegistryEntry(MakeUniqueModuleName(moduleType), moduleType.AssemblyQualifiedName));
            }
        }

        public ICollection<DynamicModuleRegistryEntry> LockAndFetchList() {
            lock (_lockObj) {
                // once the list has been returned, it must be immutable
                _entriesReadonly = true;
                return _entries;
            }
        }

        private static string MakeUniqueModuleName(Type moduleType) {
            // returns a unique name for this module
            return String.Format(CultureInfo.InvariantCulture, _moduleNameFormat, moduleType.AssemblyQualifiedName, Guid.NewGuid());
        }

    }

    internal struct DynamicModuleRegistryEntry {

        public readonly string Name;
        public readonly string Type;

        public DynamicModuleRegistryEntry(string name, string type) {
            Name = name;
            Type = type;
        }

    }
}
