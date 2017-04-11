//---------------------------------------------------------------------
// <copyright file="ObjectItemNoOpAssemblyLoader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Reflection;

namespace System.Data.Metadata.Edm
{
    internal class ObjectItemNoOpAssemblyLoader : ObjectItemAssemblyLoader
    {
        internal ObjectItemNoOpAssemblyLoader(Assembly assembly, ObjectItemLoadingSessionData sessionData)
            : base(assembly, new MutableAssemblyCacheEntry(), sessionData)
        { }

        internal override void Load()
        {
            // don't do anything but make sure we know we have seen this assembly
            if (!SessionData.KnownAssemblies.Contains(SourceAssembly, SessionData.ObjectItemAssemblyLoaderFactory, SessionData.EdmItemCollection))
            {
                AddToKnownAssemblies();
            }
        }
        
        protected override void AddToAssembliesLoaded()
        {
            throw new NotImplementedException();
        }

        protected override void LoadTypesFromAssembly()
        {
            throw new NotImplementedException();
        }
    }
}
