//---------------------------------------------------------------------
// <copyright file="CustomAssemblyResolver.cs" company="Microsoft">
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

    internal class CustomAssemblyResolver : MetadataArtifactAssemblyResolver
    {
        private Func<AssemblyName, Assembly> _referenceResolver;
        private Func<IEnumerable<Assembly>> _wildcardAssemblyEnumerator;

        internal CustomAssemblyResolver(Func<IEnumerable<Assembly>> wildcardAssemblyEnumerator, Func<AssemblyName, Assembly> referenceResolver)
        {
            Debug.Assert(wildcardAssemblyEnumerator != null);
            Debug.Assert(referenceResolver != null);
            _wildcardAssemblyEnumerator = wildcardAssemblyEnumerator;
            _referenceResolver = referenceResolver;
        }

        internal override bool TryResolveAssemblyReference(AssemblyName refernceName, out Assembly assembly)
        {
            assembly = _referenceResolver(refernceName);
            return assembly != null;
        }

        internal override IEnumerable<Assembly> GetWildcardAssemblies()
        {
            IEnumerable<Assembly> wildcardAssemblies = _wildcardAssemblyEnumerator();
            if (wildcardAssemblies == null)
            {
                throw EntityUtil.InvalidOperation(Strings.WildcardEnumeratorReturnedNull);
            }
            return wildcardAssemblies;

        }
    }
}
