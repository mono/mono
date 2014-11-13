//---------------------------------------------------------------------
// <copyright file="ImmutableAssemblyCacheEntry.cs" company="Microsoft">
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
    internal partial class ImmutableAssemblyCacheEntry : AssemblyCacheEntry
    {
        // types in "this" assembly
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<EdmType> _typesInAssembly;       
        // other assemblies referenced by types we care about in "this" assembly
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<Assembly> _closureAssemblies;
        internal ImmutableAssemblyCacheEntry(MutableAssemblyCacheEntry mutableEntry)
        {
            _typesInAssembly = new List<EdmType>(mutableEntry.TypesInAssembly).AsReadOnly();
            _closureAssemblies = new List<Assembly>(mutableEntry.ClosureAssemblies).AsReadOnly();
        }

        internal override IList<EdmType> TypesInAssembly
        {
            get { return _typesInAssembly; }
        }

        internal override IList<Assembly> ClosureAssemblies
        {
            get { return _closureAssemblies; }
        }
    }
}
