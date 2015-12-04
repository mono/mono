//---------------------------------------------------------------------
// <copyright file="AssemblyCacheEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;

namespace System.Data.Metadata.Edm
{
    internal abstract class AssemblyCacheEntry
    {
        internal abstract IList<EdmType> TypesInAssembly { get; }
        internal abstract IList<Assembly> ClosureAssemblies { get; }

        internal bool TryGetEdmType(string typeName, out EdmType edmType)
        {
            edmType = null;
            foreach (EdmType loadedEdmType in this.TypesInAssembly)
            {
                if (loadedEdmType.Identity == typeName)
                {
                    edmType = loadedEdmType;
                    break;
                }
            }
            return (edmType != null);
        }

        internal bool ContainsType(string typeName)
        {
            EdmType edmType = null;
            return TryGetEdmType(typeName, out edmType);
        }
    }
}
