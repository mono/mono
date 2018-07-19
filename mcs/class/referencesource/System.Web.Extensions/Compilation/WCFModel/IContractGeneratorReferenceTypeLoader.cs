#region Copyright (c) Microsoft Corporation
/// <copyright company='Microsoft Corporation'>
///    Copyright (c) Microsoft Corporation. All Rights Reserved.
///    Information Contained Herein is Proprietary and Confidential.
/// </copyright>
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

#if WEB_EXTENSIONS_CODE
namespace System.Web.Compilation.WCFModel
#else
namespace Microsoft.VSDesigner.WCFModel
#endif
{
    /// <summary>
    /// It loads a specific type from a referenced assembly.
    /// </summary>
    /// <remarks></remarks>
#if WEB_EXTENSIONS_CODE
    internal interface IContractGeneratorReferenceTypeLoader
#else
    [CLSCompliant(true)]
    public interface IContractGeneratorReferenceTypeLoader
#endif
    {
        // function should throw if type can't be loaded
        Type LoadType(string typeName);

        // function should throw if the assembly can't be loaded
        Assembly LoadAssembly(string assemblyName);

        void LoadAllAssemblies(out IEnumerable<Assembly> loadedAssemblies, out IEnumerable<Exception> loadingErrors);
    }

#if WEB_EXTENSIONS_CODE
    internal interface IContractGeneratorReferenceTypeLoader2
#else
    [CLSCompliant(true)]
    public interface IContractGeneratorReferenceTypeLoader2
#endif
    {
        /// <summary>
        /// Given an assembly which is supported in the target framework, loads all the exported type in it that are also supported in the target framework.
        /// </summary>
        IEnumerable<Type> LoadExportedTypes(Assembly assembly);
    }
}

