//---------------------------------------------------------------------
// <copyright file="MetadataAssemblyHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Common.Utils;

namespace System.Data.Metadata.Edm
{
    internal static class MetadataAssemblyHelper
    {
        static byte [] EcmaPublicKeyToken = System.Data.EntityModel.SchemaObjectModel.ScalarType.ConvertToByteArray(AssemblyRef.EcmaPublicKey);
        static byte [] MsPublicKeyToken = System.Data.EntityModel.SchemaObjectModel.ScalarType.ConvertToByteArray(AssemblyRef.MicrosoftPublicKey);
        private static Memoizer<Assembly, bool> _filterAssemblyCacheByAssembly = new Memoizer<Assembly, bool>(MetadataAssemblyHelper.ComputeShouldFilterAssembly, EqualityComparer<Assembly>.Default);

        internal static Assembly SafeLoadReferencedAssembly(AssemblyName assemblyName)
        {
            Assembly assembly = null;

            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (System.IO.FileNotFoundException)
            {
                // See 552932: ObjectItemCollection: fails on referenced assemblies that are not available
            }

            return assembly;
        }

        private static bool ComputeShouldFilterAssembly(Assembly assembly)
        {
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            return ShouldFilterAssembly(assemblyName);
        }
        
        internal static bool ShouldFilterAssembly(Assembly assembly)
        {
            return _filterAssemblyCacheByAssembly.Evaluate(assembly);
        }

        /// <summary>Is the assembly and its referened assemblies not expected to have any metadata</summary>
        private static bool ShouldFilterAssembly(AssemblyName assemblyName)
        {
            return (ArePublicKeyTokensEqual(assemblyName.GetPublicKeyToken(), EcmaPublicKeyToken) ||
                    ArePublicKeyTokensEqual(assemblyName.GetPublicKeyToken(), MsPublicKeyToken));
        }

        private static bool ArePublicKeyTokensEqual(byte [] left, byte [] right)
        {
            // some assemblies don't have public keys
            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static IEnumerable<Assembly> GetNonSystemReferencedAssemblies(Assembly assembly)
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (!ShouldFilterAssembly(name))
                {
                    Assembly referenceAssembly = SafeLoadReferencedAssembly(name);
                    if(referenceAssembly != null )
                    {
                        yield return referenceAssembly;
                    }
                }
            }
        }
     }
}
