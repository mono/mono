//---------------------------------------------------------------------
// <copyright file="DefaultAssemblyResolver.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace System.Data.Metadata.Edm
{
    internal class DefaultAssemblyResolver : MetadataArtifactAssemblyResolver
    {
        internal override bool TryResolveAssemblyReference(AssemblyName refernceName, out Assembly assembly)
        {
            assembly = ResolveAssembly(refernceName);
            return assembly != null;
        }


        internal override IEnumerable<Assembly> GetWildcardAssemblies()
        {
            return GetAllDiscoverableAssemblies();
        }

        internal Assembly ResolveAssembly(AssemblyName referenceName)
        {
            Assembly assembly = null;

            // look in the already loaded assemblies
            foreach (Assembly current in GetAlreadyLoadedNonSystemAssemblies())
            {
                if (AssemblyName.ReferenceMatchesDefinition(referenceName, new AssemblyName(current.FullName)))
                {
                    return current;
                }
            }

            // try to load this one specifically
            if (assembly == null)
            {
                assembly = MetadataAssemblyHelper.SafeLoadReferencedAssembly(referenceName);
                if (assembly != null)
                {
                    return assembly;
                }
            }

            // try all the discoverable ones
            TryFindWildcardAssemblyMatch(referenceName, out assembly);

            return assembly;
        }

        private bool TryFindWildcardAssemblyMatch(AssemblyName referenceName, out Assembly assembly)
        {
            Debug.Assert(referenceName != null);

            foreach (Assembly current in GetAllDiscoverableAssemblies())
            {
                if (AssemblyName.ReferenceMatchesDefinition(referenceName, new AssemblyName(current.FullName)))
                {
                    assembly = current;
                    return true;
                }
            }

            assembly = null;
            return false;
        }


        /// <summary>
        /// Return all assemblies loaded in the current AppDomain that are not signed
        /// with the Microsoft Key.
        /// </summary>
        /// <returns>A list of assemblies</returns>
        private static IEnumerable<Assembly> GetAlreadyLoadedNonSystemAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Where(a => a != null && !MetadataAssemblyHelper.ShouldFilterAssembly(a));
        }

        /// <summary>
        /// This method returns a list of assemblies whose contents depend on whether we
        /// are running in an ASP.NET environment. If we are indeed in a Web/ASP.NET
        /// scenario, we pick up the assemblies that all page compilations need to
        /// reference. If not, then we simply get the list of assemblies referenced by
        /// the entry assembly.
        /// </summary>
        /// <returns>A list of assemblies</returns>
        private static IEnumerable<Assembly> GetAllDiscoverableAssemblies()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            HashSet<Assembly> assemblyList = new HashSet<Assembly>(
                AssemblyComparer.Instance);

            foreach (Assembly loadedAssembly in GetAlreadyLoadedNonSystemAssemblies())
            {
                assemblyList.Add(loadedAssembly);
            }

            AspProxy aspProxy = new AspProxy();
            if (!aspProxy.IsAspNetEnvironment())
            {
                if (assembly == null)
                {
                    return assemblyList;
                }

                assemblyList.Add(assembly);

                foreach (Assembly referenceAssembly in MetadataAssemblyHelper.GetNonSystemReferencedAssemblies(assembly))
                {
                    assemblyList.Add(referenceAssembly);
                }

                return assemblyList;
            }

            if (aspProxy.HasBuildManagerType())
            {
                IEnumerable<Assembly> referencedAssemblies = aspProxy.GetBuildManagerReferencedAssemblies();
                // filter out system assemblies
                if (referencedAssemblies != null)
                {
                    foreach (Assembly referencedAssembly in referencedAssemblies)
                    {
                        if (MetadataAssemblyHelper.ShouldFilterAssembly(referencedAssembly))
                        {
                            continue;
                        }

                        assemblyList.Add(referencedAssembly);
                    }
                }
            }

            return assemblyList.Where(a => a != null);
        }

        internal sealed class AssemblyComparer : IEqualityComparer<Assembly>
        {
            // use singleton
            private AssemblyComparer() { }

            private static AssemblyComparer _instance = new AssemblyComparer();
            public static AssemblyComparer Instance{ get { return _instance; } }

            /// <summary>
            /// if two assemblies have the same full name, we will consider them as the same.
            /// for example,
            /// both of x and y have the full name as "{RES, Version=3.5.0.0, Culture=neutral, PublicKeyToken=null}",
            /// although they are different instances since the ReflectionOnly field in them are different, we sitll 
            /// consider them as the same.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool Equals(Assembly x, Assembly y)
            {
                AssemblyName xname = new AssemblyName(x.FullName);
                AssemblyName yname = new AssemblyName(y.FullName);
                // return *true* when either the reference are the same 
                // *or* the Assembly names are commutative equal
                return object.ReferenceEquals(x, y)
                    || (AssemblyName.ReferenceMatchesDefinition(xname, yname)
                         && AssemblyName.ReferenceMatchesDefinition(yname, xname));
            }

            public int GetHashCode(Assembly assembly)
            {
                return assembly.FullName.GetHashCode();
            }
        }
    }
}
