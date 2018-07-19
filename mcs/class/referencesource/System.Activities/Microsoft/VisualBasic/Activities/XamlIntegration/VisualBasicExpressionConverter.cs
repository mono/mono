//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace Microsoft.VisualBasic.Activities.XamlIntegration
{
    using System;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Reflection;
    using System.Xml.Linq;
    using System.ComponentModel;
    using System.Xaml;
    using System.Windows.Markup;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime;
    using System.Threading;

    static class VisualBasicExpressionConverter
    {
        static readonly Regex assemblyQualifiedNamespaceRegex = new Regex(
            "clr-namespace:(?<namespace>[^;]*);assembly=(?<assembly>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static VisualBasicSettings CollectXmlNamespacesAndAssemblies(ITypeDescriptorContext context)
        {
            // access XamlSchemaContext.ReferenceAssemblies 
            // for the Compiled Xaml scenario
            IList<Assembly> xsCtxReferenceAssemblies = null;
            IXamlSchemaContextProvider xamlSchemaContextProvider = context.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;
            if (xamlSchemaContextProvider != null && xamlSchemaContextProvider.SchemaContext != null)
            {
                xsCtxReferenceAssemblies = xamlSchemaContextProvider.SchemaContext.ReferenceAssemblies;
                if (xsCtxReferenceAssemblies != null && xsCtxReferenceAssemblies.Count == 0)
                {
                    xsCtxReferenceAssemblies = null;
                }
            } 

            VisualBasicSettings settings = null;
            IXamlNamespaceResolver namespaceResolver = (IXamlNamespaceResolver)context.GetService(typeof(IXamlNamespaceResolver));

            if (namespaceResolver == null)
            {
                return null;
            }

            lock (AssemblyCache.XmlnsMappingsLockObject)
            {
                // Fetch xmlnsMappings for the prefixes returned by the namespaceResolver service

                foreach (NamespaceDeclaration prefix in namespaceResolver.GetNamespacePrefixes())
                {
                    ReadOnlyXmlnsMapping mapping;
                    WrapCachedMapping(prefix, out mapping);
                    if (!mapping.IsEmpty)
                    {
                        if (settings == null)
                        {
                            settings = new VisualBasicSettings();
                        }

                        if (!mapping.IsEmpty)
                        {
                            foreach (ReadOnlyVisualBasicImportReference importReference in mapping.ImportReferences)
                            {
                                if (xsCtxReferenceAssemblies != null)
                                {
                                    // this is "compiled Xaml" 
                                    VisualBasicImportReference newImportReference;

                                    if (importReference.EarlyBoundAssembly != null)
                                    {
                                        if (xsCtxReferenceAssemblies.Contains(importReference.EarlyBoundAssembly))
                                        {
                                            newImportReference = importReference.Clone();
                                            newImportReference.EarlyBoundAssembly = importReference.EarlyBoundAssembly;
                                            settings.ImportReferences.Add(newImportReference);
                                        }
                                        continue;
                                    }

                                    for (int i = 0; i < xsCtxReferenceAssemblies.Count; i++)
                                    {
                                        AssemblyName xsCtxAssemblyName = VisualBasicHelper.GetFastAssemblyName(xsCtxReferenceAssemblies[i]);
                                        if (importReference.AssemblySatisfiesReference(xsCtxAssemblyName))
                                        {
                                            // bind this assembly early to the importReference
                                            // so later AssemblyName resolution can be skipped
                                            newImportReference = importReference.Clone();
                                            newImportReference.EarlyBoundAssembly = xsCtxReferenceAssemblies[i];
                                            settings.ImportReferences.Add(newImportReference);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    // this is "loose Xaml"
                                    VisualBasicImportReference newImportReference = importReference.Clone();
                                    if (importReference.EarlyBoundAssembly != null)
                                    {
                                        // VBImportReference.Clone() method deliberately doesn't copy 
                                        // its EarlyBoundAssembly to the cloned instance.
                                        // we need to explicitly copy the original's EarlyBoundAssembly
                                        newImportReference.EarlyBoundAssembly = importReference.EarlyBoundAssembly;
                                    }
                                    settings.ImportReferences.Add(newImportReference);
                                }
                            }
                        }
                    }
                }
            }
            return settings;
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing critical member AssemblyCache.XmlnsMappings.",
            Safe = "Safe because we prevent partial trusted code from manipulating the cache directly by creating a read-only wrapper around the cached XmlnsMapping.")]
        [SecuritySafeCritical]
        private static void WrapCachedMapping(NamespaceDeclaration prefix, out ReadOnlyXmlnsMapping readOnlyMapping)
        {
            XmlnsMapping mapping = new XmlnsMapping();
            XNamespace xmlns = XNamespace.Get(prefix.Namespace);

            if (!AssemblyCache.XmlnsMappings.TryGetValue(xmlns, out mapping))
            {
                // Match a namespace of the form "clr-namespace:<namespace-name>;assembly=<assembly-name>"

                Match match = assemblyQualifiedNamespaceRegex.Match(prefix.Namespace);

                if (match.Success)
                {
                    mapping.ImportReferences = new HashSet<VisualBasicImportReference>();
                    mapping.ImportReferences.Add(
                        new VisualBasicImportReference
                        {
                            Assembly = match.Groups["assembly"].Value,
                            Import = match.Groups["namespace"].Value,
                            Xmlns = xmlns
                        });
                }
                else
                {
                    mapping.ImportReferences = new HashSet<VisualBasicImportReference>();
                }
                AssemblyCache.XmlnsMappings[xmlns] = mapping;
            }

            // ReadOnlyXmlnsMapping constructor tolerates an empty mapping being passed in.
            readOnlyMapping = new ReadOnlyXmlnsMapping(mapping);
        }

        /// <summary>
        /// Static class used to cache assembly metadata.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>
        ///         XmlnsMappings for static assemblies are not GC'd. In v4.0 we can assume that all static assemblies 
        ///         containing XmlnsDefinition attributes are non-collectible. The CLR will provide no public mechanism 
        ///         for unloading a static assembly or specifying that a static assembly is collectible. While there 
        ///         may be some small number of assemblies identified by the CLR as collectible, none will contain 
        ///         XmlnsDefinition attributes. Should the CLR provide a public mechanism for unloading a static assembly
        ///         or specifying that a static assembly is collectible, we should revisit this decision based on scenarios
        ///         that flow from these mechanisms.
        ///         </description></item>
        ///         <item><description>
        ///         XmlnsMappings for dynamic assemblies are not created. This is because the hosted Visual Basic compiler
        ///         does not support dynamic assembly references. Should support for dynamic assembly references be 
        ///         added to the Visual Basic compiler, we should strip away Assembly.IsDynamic checks from this class and
        ///         update the code ensure that VisualBasicImportReference instances are removed in a timely manner.
        ///         </description></item>
        ///     </list>
        /// </remarks>
        static class AssemblyCache
        {
            static bool initialized = false;

            // This is here so that obtaining the lock is not required to be SecurityCritical.
            public static object XmlnsMappingsLockObject = new object();

            [Fx.Tag.SecurityNote(Critical = "Critical because we are storing assembly references and if we alloed PT access, they could mess with that.")]
            [SecurityCritical]
            static Dictionary<XNamespace, XmlnsMapping> xmlnsMappings;

            public static Dictionary<XNamespace, XmlnsMapping> XmlnsMappings
            {
                [Fx.Tag.SecurityNote(Critical = "Critical because providing access to the critical xmlnsMappings dictionary.")]
                [SecurityCritical]
                get
                {
                    EnsureInitialized();
                    return xmlnsMappings;
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing critical member xmlnsMappings and CacheLoadedAssembly. Only called from CLR.")]
            [SecurityCritical]
            static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
            {
                Assembly assembly = args.LoadedAssembly;

                if (assembly.IsDefined(typeof(XmlnsDefinitionAttribute), false) && !assembly.IsDynamic)
                {
                    lock (XmlnsMappingsLockObject)
                    {
                        CacheLoadedAssembly(assembly);
                    }
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing AppDomain.AssemblyLoaded and we are accessing critical member xmlnsMappings.")]
            [SecurityCritical]
            static void EnsureInitialized()            
            {
                if (initialized)
                {
                    return;
                }

                if (xmlnsMappings == null)
                {
                    Interlocked.CompareExchange(ref xmlnsMappings,
                        new Dictionary<XNamespace, XmlnsMapping>(new XNamespaceEqualityComparer()),
                        null);
                }

                lock (XmlnsMappingsLockObject)
                {
                    if (AssemblyCache.initialized)
                    {
                        return;
                    }

                    AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    for (int i = 0; i < assemblies.Length; ++i)
                    {
                        Assembly assembly = assemblies[i];

                        if (assembly.IsDefined(typeof(XmlnsDefinitionAttribute), false) && ! assembly.IsDynamic)
                        {
                            CacheLoadedAssembly(assembly);
                        }
                    }
                    
                    initialized = true;
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing critical member xmlnsMappings.")]
            [SecurityCritical]
            static void CacheLoadedAssembly(Assembly assembly)
            {
                // this VBImportReference is only used as an entry to the xmlnsMappings cache
                // and is never meant to be Xaml serialized.
                // those VBImportReferences that are to be Xaml serialized are created by Clone() method.
                XmlnsDefinitionAttribute[] attributes = (XmlnsDefinitionAttribute[])assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false);
                string assemblyName = assembly.FullName;
                XmlnsMapping mapping;

                for (int i = 0; i < attributes.Length; ++i)
                {
                    XNamespace xmlns = XNamespace.Get(attributes[i].XmlNamespace);

                    if (!xmlnsMappings.TryGetValue(xmlns, out mapping))
                    {
                        mapping.ImportReferences = new HashSet<VisualBasicImportReference>();
                        xmlnsMappings[xmlns] = mapping;
                    }

                    VisualBasicImportReference newImportReference = new VisualBasicImportReference
                    {
                        Assembly = assemblyName,
                        Import = attributes[i].ClrNamespace,
                        Xmlns = xmlns,
                    };
                    // early binding the assembly
                    // this leads to the short-cut, skipping the normal assembly resolution routine
                    newImportReference.EarlyBoundAssembly = assembly;
                    mapping.ImportReferences.Add(newImportReference);
                }
            }

            class XNamespaceEqualityComparer : IEqualityComparer<XNamespace>
            {
                public XNamespaceEqualityComparer()
                { }

                bool IEqualityComparer<XNamespace>.Equals(XNamespace x, XNamespace y)
                {
                    return x == y;
                }

                int IEqualityComparer<XNamespace>.GetHashCode(XNamespace x)
                {
                    return x.GetHashCode();
                }
            }
        }

        /// <summary>
        /// Struct used to cache XML Namespace mappings. 
        /// </summary>
        struct XmlnsMapping
        {
            public HashSet<VisualBasicImportReference> ImportReferences;

            public bool IsEmpty
            {
                get
                {
                    return this.ImportReferences == null || this.ImportReferences.Count == 0;
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing a XmlnsMapping that is stored in the XmlnsMappings cache, which is SecurityCritical.",
            Safe = "Safe because we are wrapping the XmlnsMapping and not allowing unsafe code to modify it.")]
        [SecuritySafeCritical]
        struct ReadOnlyXmlnsMapping
        {
            XmlnsMapping wrappedMapping;

            internal ReadOnlyXmlnsMapping(XmlnsMapping mapping)
            {
                this.wrappedMapping = mapping;
            }

            internal bool IsEmpty
            {
                get
                {
                    return this.wrappedMapping.IsEmpty;
                }
            }

            internal IEnumerable<ReadOnlyVisualBasicImportReference> ImportReferences
            {
                get
                {
                    foreach (VisualBasicImportReference wrappedReference in this.wrappedMapping.ImportReferences)
                    {
                        yield return new ReadOnlyVisualBasicImportReference(wrappedReference);
                    }
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing a VisualBasicImportReference that is stored in the XmlnsMappings cache, which is SecurityCritical.",
            Safe = "Safe because we are wrapping the VisualBasicImportReference and not allowing unsafe code to modify it.")]
        [SecuritySafeCritical]
        struct ReadOnlyVisualBasicImportReference
        {
            readonly VisualBasicImportReference wrappedReference;

            internal ReadOnlyVisualBasicImportReference(VisualBasicImportReference referenceToWrap)
            {
                this.wrappedReference = referenceToWrap;
            }

            // If this is ever needed, uncomment this. It is commented out now to avoid FxCop violation because it is not called.
            //internal string Assembly
            //{
            //    get
            //    {
            //        return this.wrappedReference.Assembly;
            //    }
            //}

            // If this is ever needed, uncomment this. It is commented out now to avoid FxCop violation because it is not called.
            //internal string Import
            //{
            //    get
            //    {
            //        return this.wrappedReference.Import;
            //    }
            //}

            internal Assembly EarlyBoundAssembly
            {
                get { return this.wrappedReference.EarlyBoundAssembly; }
            }

            internal VisualBasicImportReference Clone()
            {
                return this.wrappedReference.Clone();
            }

            // this code is borrowed from XamlSchemaContext
            internal bool AssemblySatisfiesReference(AssemblyName assemblyName)
            {
                if (this.wrappedReference.AssemblyName.Name != assemblyName.Name)
                {
                    return false;
                }
                if (this.wrappedReference.AssemblyName.Version != null && !this.wrappedReference.AssemblyName.Version.Equals(assemblyName.Version))
                {
                    return false;
                }
                if (this.wrappedReference.AssemblyName.CultureInfo != null && !this.wrappedReference.AssemblyName.CultureInfo.Equals(assemblyName.CultureInfo))
                {
                    return false;
                }
                byte[] requiredToken = this.wrappedReference.AssemblyName.GetPublicKeyToken();
                if (requiredToken != null)
                {
                    byte[] actualToken = assemblyName.GetPublicKeyToken();
                    if (!AssemblyNameEqualityComparer.IsSameKeyToken(requiredToken, actualToken))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return this.wrappedReference.GetHashCode();
            }

            // If this is ever needed, uncomment this. It is commented out now to avoid FxCop violation because it is not called.
            //public bool Equals(VisualBasicImportReference other)
            //{
            //    return this.wrappedReference.Equals(other);
            //}
        }
    }
}
