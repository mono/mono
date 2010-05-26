/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal sealed class ControllerTypeCache {

        private const string _typeCacheName = "MVC-ControllerTypeCache.xml";

        private Dictionary<string, ILookup<string, Type>> _cache;
        private object _lockObj = new object();

        internal int Count {
            get {
                int count = 0;
                foreach (var lookup in _cache.Values) {
                    foreach (var grouping in lookup) {
                        count += grouping.Count();
                    }
                }
                return count;
            }
        }

        public void EnsureInitialized(IBuildManager buildManager) {
            if (_cache == null) {
                lock (_lockObj) {
                    if (_cache == null) {
                        List<Type> controllerTypes = TypeCacheUtil.GetFilteredTypesFromAssemblies(_typeCacheName, IsControllerType, buildManager);
                        var groupedByName = controllerTypes.GroupBy(
                            t => t.Name.Substring(0, t.Name.Length - "Controller".Length),
                            StringComparer.OrdinalIgnoreCase);
                        _cache = groupedByName.ToDictionary(
                            g => g.Key,
                            g => g.ToLookup(t => t.Namespace ?? String.Empty, StringComparer.OrdinalIgnoreCase),
                            StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
        }

        public ICollection<Type> GetControllerTypes(string controllerName, HashSet<string> namespaces) {
            HashSet<Type> matchingTypes = new HashSet<Type>();

            ILookup<string, Type> nsLookup;
            if (_cache.TryGetValue(controllerName, out nsLookup)) {
                // this friendly name was located in the cache, now cycle through namespaces
                if (namespaces != null) {
                    foreach (string requestedNamespace in namespaces) {
                        foreach (var targetNamespaceGrouping in nsLookup) {
                            if (IsNamespaceMatch(requestedNamespace, targetNamespaceGrouping.Key)) {
                                matchingTypes.UnionWith(targetNamespaceGrouping);
                            }
                        }
                    }
                }
                else {
                    // if the namespaces parameter is null, search *every* namespace
                    foreach (var nsGroup in nsLookup) {
                        matchingTypes.UnionWith(nsGroup);
                    }
                }
            }

            return matchingTypes;
        }

        internal static bool IsControllerType(Type t) {
            return
                t != null &&
                t.IsPublic &&
                t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                !t.IsAbstract &&
                typeof(IController).IsAssignableFrom(t);
        }

        internal static bool IsNamespaceMatch(string requestedNamespace, string targetNamespace) {
            // degenerate cases
            if (requestedNamespace == null) {
                return false;
            }
            else if (requestedNamespace.Length == 0) {
                return true;
            }

            if (!requestedNamespace.EndsWith(".*", StringComparison.OrdinalIgnoreCase)) {
                // looking for exact namespace match
                return String.Equals(requestedNamespace, targetNamespace, StringComparison.OrdinalIgnoreCase);
            }
            else {
                // looking for exact or sub-namespace match
                requestedNamespace = requestedNamespace.Substring(0, requestedNamespace.Length - ".*".Length);
                if (!targetNamespace.StartsWith(requestedNamespace, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }

                if (requestedNamespace.Length == targetNamespace.Length) {
                    // exact match
                    return true;
                }
                else if (targetNamespace[requestedNamespace.Length] == '.') {
                    // good prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar.Baz"
                    return true;
                }
                else {
                    // bad prefix match, e.g. requestedNamespace = "Foo.Bar" and targetNamespace = "Foo.Bar2"
                    return false;
                }
            }
        }

    }
}
