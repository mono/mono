//------------------------------------------------------------------------------
// <copyright file="WebResourceUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Web.Util;

    internal static class WebResourceUtil {
        // Maps Tuple<string, Assembly>(resourceName, assembly) to bool
        private static readonly Hashtable _assemblyContainsWebResourceCache = Hashtable.Synchronized(new Hashtable());
        private static readonly FastStringLookupTable _systemWebExtensionsCache = CreateSystemWebExtensionsCache();

        // Returns true if the assembly contains a Web resource and an embedded resource with
        // the sepecified name.  Throws exception if assembly contains Web resource but no
        // embedded resource, since this is always an error.
        public static bool AssemblyContainsWebResource(Assembly assembly, string resourceName) {
            // PERF: Special-case known resources in our own assembly
            if (assembly == AssemblyCache.SystemWebExtensions) {
                return _systemWebExtensionsCache.Contains(resourceName);
            }

            // Getting and checking the custom attributes is expensive, so we cache the result
            // of the lookup.
            Tuple<string, Assembly> key = new Tuple<string, Assembly>(resourceName, assembly);
            object assemblyContainsWebResource = _assemblyContainsWebResourceCache[key];

            if (assemblyContainsWebResource == null) {
                assemblyContainsWebResource = false;

                object[] attrs = assembly.GetCustomAttributes(typeof(WebResourceAttribute), false);
                foreach (WebResourceAttribute attr in attrs) {
                    // Resource names are always case-sensitive
                    if (String.Equals(attr.WebResource, resourceName, StringComparison.Ordinal)) {
                        if (assembly.GetManifestResourceStream(resourceName) != null) {
                            assemblyContainsWebResource = true;
                            break;
                        }
                        else {
                            // Always an error to contain Web resource but not embedded resource.
                            throw new InvalidOperationException(String.Format(
                                CultureInfo.CurrentUICulture,
                                AtlasWeb.WebResourceUtil_AssemblyDoesNotContainEmbeddedResource,
                                assembly, resourceName));
                        }
                    }
                }

                _assemblyContainsWebResourceCache[key] = assemblyContainsWebResource;
            }

            return (bool)assemblyContainsWebResource;
        }

        private static FastStringLookupTable CreateSystemWebExtensionsCache() {
            Assembly assembly = AssemblyCache.SystemWebExtensions;
            object[] attrs = assembly.GetCustomAttributes(typeof(WebResourceAttribute), false);

            var resourceNames = from WebResourceAttribute attr in attrs
                                select attr.WebResource;
            return new FastStringLookupTable(resourceNames);
        }

        // Throws exception if the assembly does not contain a Web resource and an embedded resource
        // with the specified name.
        public static void VerifyAssemblyContainsReleaseWebResource(Assembly assembly, string releaseResourceName,
            Assembly currentAjaxAssembly) {
            if (!AssemblyContainsWebResource(assembly, releaseResourceName)) {
                string errorMessage;
                if (assembly == AssemblyCache.SystemWebExtensions) {
                    errorMessage = String.Format(
                        CultureInfo.CurrentUICulture,
                        AtlasWeb.WebResourceUtil_SystemWebExtensionsDoesNotContainReleaseWebResource,
                        currentAjaxAssembly ?? assembly, releaseResourceName);
                }
                else {
                    errorMessage = String.Format(
                        CultureInfo.CurrentUICulture,
                        AtlasWeb.WebResourceUtil_AssemblyDoesNotContainReleaseWebResource,
                        assembly, releaseResourceName);
                }
                throw new InvalidOperationException(errorMessage);
            }
        }

        // Throws exception if the assembly does not contain a Web resource and an embedded resource
        // with the specified name.
        public static void VerifyAssemblyContainsDebugWebResource(Assembly assembly, string debugResourceName) {
            if (!AssemblyContainsWebResource(assembly, debugResourceName)) {
                throw new InvalidOperationException(String.Format(
                    CultureInfo.CurrentUICulture,
                    AtlasWeb.WebResourceUtil_AssemblyDoesNotContainDebugWebResource,
                   assembly, debugResourceName));
            }
        }

        private class FastStringLookupTable {
            // PERF: Switching over the length is more performant than switching over the string itself
            // or checking equality against each string.  When switching over the string itself, the switch
            // is compiled to a lookup in a static Dictionary<string, int>, which is 5-10 times slower than
            // switching over the length.  Checking equality against each string ranges from equal performance
            // to 10 times slower, depending on how early a match is found.
 
            private readonly string[][] _table;

            public FastStringLookupTable(IEnumerable<string> strings) {
                int longest = (from s in strings
                               orderby s.Length descending
                               select s.Length).First();

                _table = new string[longest + 1][];

                var groups = from s in strings
                             group s by s.Length into g
                             select g;

                foreach (var g in groups) {
                    _table[g.Key] = g.ToArray();
                }
            }

            public bool Contains(string s) {
                if (String.IsNullOrEmpty(s)) {
                    return false;
                }

                if (s.Length >= _table.Length) {
                    return false;
                }

                string[] strings = _table[s.Length];
                if (strings == null) {
                    return false;
                }

                for (int i = 0; i < strings.Length; i++) {
                    if (s == strings[i]) {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
