//---------------------------------------------------------------------
// <copyright file="MetadataArtifactAssemblyResolver.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       leil
// @backupOwner anpete
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Reflection;

    internal abstract class MetadataArtifactAssemblyResolver
    {
        internal abstract bool TryResolveAssemblyReference(AssemblyName refernceName, out Assembly assembly);
        internal abstract IEnumerable<Assembly> GetWildcardAssemblies();
    }
}
