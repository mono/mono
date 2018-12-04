// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xaml.Schema;
using System.Windows.Markup;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Xaml.MS.Impl
{
    class XmlNsInfo
    {
        // Thread-safety: any lazily initalized fields in this class must be assigned idempotently.
        // I.e. never assign until the result is complete; and if multiple threads are assigning
        // at the same time, the results should be equivalent.

        // This property will always be non-null, unless the assembly is a collectible dynamic
        // assembly, and gets unloaded.
        internal Assembly Assembly { get { return (Assembly)_assembly.Target; } }

        private IList<XmlNsDefinition> _nsDefs;
        internal IList<XmlNsDefinition> NsDefs
        {
            get
            {
                if (_nsDefs == null)
                {
                    _nsDefs = LoadNsDefs();
                }
                return _nsDefs;
            }
        }

        // Note this, is the only dictionary that we synchronize, because XamlSchemaContext adds to it
        private ConcurrentDictionary<string, IList<string>> _clrToXmlNs = null;
        internal ConcurrentDictionary<string, IList<string>> ClrToXmlNs
        {
            get
            {
                if (_clrToXmlNs == null)
                {
                    _clrToXmlNs = LoadClrToXmlNs();
                }
                return _clrToXmlNs;
            }
        }

        private ICollection<AssemblyName> _internalsVisibleTo;
        internal ICollection<AssemblyName> InternalsVisibleTo
        {
            get
            {
                if (_internalsVisibleTo == null)
                {
                    _internalsVisibleTo = LoadInternalsVisibleTo();
                }
                return _internalsVisibleTo;
            }
        }

        private Dictionary<string, string> _oldToNewNs = null;
        internal Dictionary<string, string> OldToNewNs
        {
            get
            {
                if (_oldToNewNs == null)
                {
                    _oldToNewNs = LoadOldToNewNs();
                }
                return _oldToNewNs;
            }
        }

        private Dictionary<string, string> _prefixes = null;
        internal Dictionary<string, string> Prefixes
        {
            get
            {
                if (_prefixes == null)
                {
                    _prefixes = LoadPrefixes();
                }
                return _prefixes;
            }
        }

        private string _rootNamespace = null;
        internal string RootNamespace
        {
            get
            {
                if (_rootNamespace == null)
                {
                    _rootNamespace = LoadRootNamespace() ?? string.Empty;
                }
                return _rootNamespace;
            }
        }

        private WeakReference _assembly;
        private IList<CustomAttributeData> _attributeData;
        private bool _fullyQualifyAssemblyName;

        internal XmlNsInfo(Assembly assembly, bool fullyQualifyAssemblyName)
        {
            _assembly = new WeakReference(assembly);
            _fullyQualifyAssemblyName = fullyQualifyAssemblyName;
        }

        void EnsureReflectionOnlyAttributeData()
        {
            if (_attributeData == null)
            {
                // We don't scoop RefOnly assemblies out of the AppDomain; they'll always be rooted
                // in XamlSchemaContext._referenceAssemblies or _xmlnsInfoForUnreferencedAssemblies.
                // So they should never be collected.
                Debug.Assert(Assembly != null, "RefOnly assemblies shouldn't be GCed");
                _attributeData = Assembly.GetCustomAttributesData();
            }
        }

        internal static string GetPreferredPrefix(string prefix1, string prefix2)
        {
            if (prefix1.Length < prefix2.Length)
            {
                return prefix1;
            }
            else if (prefix2.Length < prefix1.Length)
            {
                return prefix2;
            }
            else if (StringComparer.Ordinal.Compare(prefix1, prefix2) < 0)
            {
                return prefix1;
            }
            return prefix2;
        }

        IList<XmlNsDefinition> LoadNsDefs()
        {
            IList<XmlNsDefinition> result = new List<XmlNsDefinition>();

            Assembly assembly = Assembly;
            if (assembly == null)
            {
                return result;
            }
            if (assembly.ReflectionOnly)
            {
                EnsureReflectionOnlyAttributeData();
                foreach (var cad in _attributeData)
                {
                    if (LooseTypeExtensions.AssemblyQualifiedNameEquals(cad.Constructor.DeclaringType, typeof(XmlnsDefinitionAttribute)))
                    {
                        // WPF 3.0 ignores XmlnsDefinitionAttribute.AssemblyName, and so do we
                        string xmlns = cad.ConstructorArguments[0].Value as string;
                        string clrns = cad.ConstructorArguments[1].Value as string;
                        LoadNsDefHelper(result, xmlns, clrns, assembly);
                    }
                }
            }
            else
            {
                Attribute[] attributes;
                attributes = Attribute.GetCustomAttributes(assembly, typeof(XmlnsDefinitionAttribute));
                foreach (Attribute attr in attributes)
                {
                    XmlnsDefinitionAttribute xmlnsDefAttr = (XmlnsDefinitionAttribute)attr;

                    string xmlns = xmlnsDefAttr.XmlNamespace;
                    string clrns = xmlnsDefAttr.ClrNamespace;
                    LoadNsDefHelper(result, xmlns, clrns, assembly);
                }
            }
            return result;
        }

        void LoadNsDefHelper(IList<XmlNsDefinition> result, string xmlns, string clrns, Assembly assembly)
        {
            if (String.IsNullOrEmpty(xmlns) || clrns == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.BadXmlnsDefinition, assembly.FullName));
            }

            result.Add(new XmlNsDefinition { ClrNamespace = clrns, XmlNamespace = xmlns });
        }

        ConcurrentDictionary<string, IList<string>> LoadClrToXmlNs()
        {
            ConcurrentDictionary<string, IList<string>> result =
                XamlSchemaContext.CreateDictionary<string, IList<string>>();

            Assembly assembly = Assembly;
            if (assembly == null)
            {
                return result;
            }
            foreach (XmlNsDefinition nsDef in NsDefs)
            {
                IList<string> xmlNamespaceList;
                if (!result.TryGetValue(nsDef.ClrNamespace, out xmlNamespaceList))
                {
                    xmlNamespaceList = new List<string>();
                    result.TryAdd(nsDef.ClrNamespace, xmlNamespaceList);
                }
                xmlNamespaceList.Add(nsDef.XmlNamespace);
            }

            string assemblyName = _fullyQualifyAssemblyName ? 
                assembly.FullName : XamlSchemaContext.GetAssemblyShortName(assembly);
            foreach (KeyValuePair<string, IList<string>> clrToXmlNs in result)
            {
                // Sort namespaces in preference order
                List<string> nsList = (List<string>)clrToXmlNs.Value;
                NamespaceComparer comparer = new NamespaceComparer(this, assembly);
                nsList.Sort(comparer.CompareNamespacesByPreference);
                // Add clr-namespace form as last choice
                string clrNsUri = ClrNamespaceUriParser.GetUri(clrToXmlNs.Key, assemblyName);
                nsList.Add(clrNsUri);
            }
            // Convert to read-only lists so we can safely return these from public API
            MakeListsImmutable(result);
            return result;
        }

        ICollection<AssemblyName> LoadInternalsVisibleTo()
        {
            var result = new List<AssemblyName>();

            Assembly assembly = Assembly;
            if (assembly == null)
            {
                return result;
            }
            if (assembly.ReflectionOnly)
            {
                EnsureReflectionOnlyAttributeData();
                foreach (var cad in _attributeData)
                {
                    if (LooseTypeExtensions.AssemblyQualifiedNameEquals(cad.Constructor.DeclaringType, typeof(InternalsVisibleToAttribute)))
                    {
                        string assemblyName = cad.ConstructorArguments[0].Value as string;
                        LoadInternalsVisibleToHelper(result, assemblyName, assembly);
                    }
                }
            }
            else
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(assembly, typeof(InternalsVisibleToAttribute));
                for (int i = 0; i < attributes.Length; i++)
                {
                    InternalsVisibleToAttribute ivAttrib = (InternalsVisibleToAttribute)attributes[i];
                    LoadInternalsVisibleToHelper(result, ivAttrib.AssemblyName, assembly);
                }
            }
            return result;
        }

        void LoadInternalsVisibleToHelper(List<AssemblyName> result, string assemblyName, Assembly assembly)
        {
            if (assemblyName == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.BadInternalsVisibleTo1, assembly.FullName));
            }
            try
            {
                result.Add(new AssemblyName(assemblyName));
            }
            catch (ArgumentException ex)
            {
                throw new XamlSchemaException(SR.Get(SRID.BadInternalsVisibleTo2, assemblyName, assembly.FullName), ex);
            }
            // AssemblyName.ctor throws FLE on malformed assembly name
            catch (FileLoadException ex)
            {
                throw new XamlSchemaException(SR.Get(SRID.BadInternalsVisibleTo2, assemblyName, assembly.FullName), ex);
            }
        }

        Dictionary<string, string> LoadOldToNewNs()
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);

            Assembly assembly = Assembly;
            if (assembly == null)
            {
                return result;
            }
            if (assembly.ReflectionOnly)
            {
                EnsureReflectionOnlyAttributeData();

                foreach (var cad in _attributeData)
                {
                    if (LooseTypeExtensions.AssemblyQualifiedNameEquals(cad.Constructor.DeclaringType, typeof(XmlnsCompatibleWithAttribute)))
                    {
                        string oldns = cad.ConstructorArguments[0].Value as string;
                        string newns = cad.ConstructorArguments[1].Value as string;
                        LoadOldToNewNsHelper(result, oldns, newns, assembly);
                    }
                }
            }
            else
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(assembly, typeof(XmlnsCompatibleWithAttribute));
                foreach (Attribute attr in attributes)
                {
                    // Read in the attribute value
                    XmlnsCompatibleWithAttribute xmlnsCompatAttr = (XmlnsCompatibleWithAttribute)attr;
                    LoadOldToNewNsHelper(result, xmlnsCompatAttr.OldNamespace, xmlnsCompatAttr.NewNamespace, assembly);
                }
            }

            return result;
        }

        void LoadOldToNewNsHelper(Dictionary<string, string> result, string oldns, string newns, Assembly assembly)
        {
            if (String.IsNullOrEmpty(newns) || String.IsNullOrEmpty(oldns))
            {
                throw new XamlSchemaException(SR.Get(SRID.BadXmlnsCompat, assembly.FullName));
            }

            if (result.ContainsKey(oldns))
            {
                throw new XamlSchemaException(SR.Get(SRID.DuplicateXmlnsCompat, assembly.FullName, oldns));
            }
            result.Add(oldns, newns);
        }

        Dictionary<string, string> LoadPrefixes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);

            Assembly assembly = Assembly;
            if (assembly == null)
            {
                return result;
            }
            if (assembly.ReflectionOnly)
            {
                EnsureReflectionOnlyAttributeData();

                foreach (var cad in _attributeData)
                {
                    if (LooseTypeExtensions.AssemblyQualifiedNameEquals(cad.Constructor.DeclaringType, typeof(XmlnsPrefixAttribute)))
                    {
                        string xmlns = cad.ConstructorArguments[0].Value as string;
                        string prefix = cad.ConstructorArguments[1].Value as string;
                        LoadPrefixesHelper(result, xmlns, prefix, assembly);
                    }
                }
            }
            else
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(assembly, typeof(XmlnsPrefixAttribute));
                foreach (Attribute attr in attributes)
                {
                    XmlnsPrefixAttribute xmlnsPrefixAttr = (XmlnsPrefixAttribute)attr;
                    LoadPrefixesHelper(result, xmlnsPrefixAttr.XmlNamespace, xmlnsPrefixAttr.Prefix, assembly);
                }
            }
            return result;
        }

        void LoadPrefixesHelper(Dictionary<string, string> result, string xmlns, string prefix, Assembly assembly)
        {
            if (String.IsNullOrEmpty(prefix) || String.IsNullOrEmpty(xmlns))
            {
                throw new XamlSchemaException(SR.Get(SRID.BadXmlnsPrefix, assembly.FullName));
            }

            string oldPrefix;
            if (!result.TryGetValue(xmlns, out oldPrefix) ||
                GetPreferredPrefix(oldPrefix, prefix) == prefix)
            {
                result[xmlns] = prefix;
            }
        }

        string LoadRootNamespace()
        {
            Assembly assembly = Assembly;
            if (assembly == null)
            {
                return null;
            }
            if (assembly.ReflectionOnly)
            {
                EnsureReflectionOnlyAttributeData();

                foreach (var cad in _attributeData)
                {
                    if (LooseTypeExtensions.AssemblyQualifiedNameEquals(cad.Constructor.DeclaringType, typeof(RootNamespaceAttribute)))
                    {
                        return cad.ConstructorArguments[0].Value as string;
                    }
                }
                return null;
            }
            else
            {
                RootNamespaceAttribute rootNs = (RootNamespaceAttribute)
                    Attribute.GetCustomAttribute(assembly, typeof(RootNamespaceAttribute));
                return (rootNs == null) ? null : rootNs.Namespace;
            }
        }

        void MakeListsImmutable(IDictionary<string, IList<string>> dict)
        {
            // Need to copy the keys because we can't change a dictionary while iterating
            string[] keys = new string[dict.Count];
            dict.Keys.CopyTo(keys, 0);
            foreach (string key in keys)
            {
                dict[key] = new ReadOnlyCollection<string>(dict[key]);
            }

        }

        private class NamespaceComparer
        {
            XmlNsInfo _nsInfo;
            IDictionary<string, int> _subsumeCount;

            public NamespaceComparer(XmlNsInfo nsInfo, Assembly assembly)
            {
                _nsInfo = nsInfo;

                // Calculate the subsume count upfront, since this also serves as our cycle detection
                _subsumeCount = new Dictionary<string,int>(nsInfo.OldToNewNs.Count);

                Dictionary<string, object> visited = new Dictionary<string, object>();

                // for every XmlnsCompatAtribute
                foreach (string newNs in nsInfo.OldToNewNs.Values)
                {
                    visited.Clear();

                    // Increment the subsume count for all transitive subsumers
                    string ns = newNs;
                    do
                    {
                        if (visited.ContainsKey(ns))
                        {
                            throw new XamlSchemaException(SR.Get(SRID.XmlnsCompatCycle, assembly.FullName, ns));
                        }
                        visited.Add(ns, null);
                        IncrementSubsumeCount(ns);
                        ns = GetNewNs(ns);
                    }
                    while (ns != null);
                }
            }

            public int CompareNamespacesByPreference(string ns1, string ns2)
            {
                if (KS.Eq(ns1, ns2))
                {
                    return 0;
                }
                const int Prefer_NS1 = -1;
                const int Prefer_NS2 = 1;

                // If one namespace subsumes the other, favor the subsumer
                string newNs = GetNewNs(ns1);
                while (newNs != null)
                {
                    if (newNs == ns2)
                    {
                        return Prefer_NS2;
                    }
                    newNs = GetNewNs(newNs);
                }
                newNs = GetNewNs(ns2);
                while (newNs != null)
                {
                    if (newNs == ns1)
                    {
                        return Prefer_NS1;
                    }
                    newNs = GetNewNs(newNs);
                }

                // Favor namespaces that aren't subsumed over ones that are
                if (GetNewNs(ns1) == null)
                {
                    if (GetNewNs(ns2) != null)
                    {
                        return Prefer_NS1;
                    }
                }
                else if (GetNewNs(ns2) == null)
                {
                    return Prefer_NS2;
                }

                // Favor namespaces that subsume a greater number of other namespaces
                int ns1count = 0, ns2count = 0;
                _subsumeCount.TryGetValue(ns1, out ns1count);
                _subsumeCount.TryGetValue(ns2, out ns2count);
                if (ns1count > ns2count)
                {
                    return Prefer_NS1;
                }
                else if (ns2count > ns1count)
                {
                    return Prefer_NS2;
                }

                // Favor namespaces with prefixes over namespaces without prefixes
                // Favor namespaces with shorter prefixes over namespaces with longer ones
                string prefix1, prefix2;
                _nsInfo.Prefixes.TryGetValue(ns1, out prefix1);
                _nsInfo.Prefixes.TryGetValue(ns2, out prefix2);
                if (string.IsNullOrEmpty(prefix1))
                {
                    if (!string.IsNullOrEmpty(prefix2))
                    {
                        return Prefer_NS2;
                    }
                }
                else if (string.IsNullOrEmpty(prefix2))
                {
                    return Prefer_NS1;
                }
                else if (prefix1.Length < prefix2.Length)
                {
                    return Prefer_NS1;
                }
                else if (prefix2.Length < prefix1.Length)
                {
                    return Prefer_NS2;
                }

                // fall back to ordinal comparison
                return StringComparer.Ordinal.Compare(ns1, ns2);
            }

            private string GetNewNs(string oldNs)
            {
                string newNs;
                _nsInfo.OldToNewNs.TryGetValue(oldNs, out newNs);
                return newNs;
            }

            private void IncrementSubsumeCount(string ns)
            {
                int currentCount;
                _subsumeCount.TryGetValue(ns, out currentCount);
                currentCount++;
                _subsumeCount[ns] = currentCount;
            }
        }

        internal class XmlNsDefinition
        {
            public string ClrNamespace { get; set; }
            public string XmlNamespace { get; set; }
        }
    }
}
