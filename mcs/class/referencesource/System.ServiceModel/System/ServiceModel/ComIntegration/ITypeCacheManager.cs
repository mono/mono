//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
     using System;
     using System.Reflection;
     
     internal interface ITypeCacheManager 
     {
         void FindOrCreateType (Guid riid, out Type interfaceType, bool noAssemblyGeneration, bool isServer);
         void FindOrCreateType (Type serverType, Guid riid, out Type interfaceType, bool noAssemblyGeneration, bool isServer);
         void FindOrCreateType (Guid typeLibId, string typeLibVersion, Guid typeDefId, out Type userDefinedType, bool noAssemblyGeneration);
         Assembly ResolveAssembly(Guid assembly);
     }
}

