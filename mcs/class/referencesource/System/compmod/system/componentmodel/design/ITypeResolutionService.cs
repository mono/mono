//------------------------------------------------------------------------------
// <copyright file="ITypeResolutionService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.Reflection;

    /// <devdoc>
    ///    <para>
    ///         The type resolution service is used to load types at design time.
    ///    </para>
    /// </devdoc>
    public interface ITypeResolutionService {

        /// <devdoc>
        ///     Retrieves the requested assembly.
        /// </devdoc>    
        Assembly GetAssembly(AssemblyName name);
    
        /// <devdoc>
        ///     Retrieves the requested assembly.
        /// </devdoc>    
        Assembly GetAssembly(AssemblyName name, bool throwOnError);
    
        /// <devdoc>
        ///     Loads a type with the given name.
        /// </devdoc>
        Type GetType(string name);
    
        /// <devdoc>
        ///     Loads a type with the given name.
        /// </devdoc>
        Type GetType(string name, bool throwOnError);
    
        /// <devdoc>
        ///     Loads a type with the given name.
        /// </devdoc>
        Type GetType(string name, bool throwOnError, bool ignoreCase);
    
        /// <devdoc>
        ///     References the given assembly name.  Once an assembly has
        ///     been referenced types may be loaded from it without
        ///     qualifying them with the assembly.
        /// </devdoc>
        void ReferenceAssembly(AssemblyName name);

        /// <devdoc>
        ///    <para>
        ///       Returns the path to the file name from which the assembly was loaded.
        ///    </para>
        /// </devdoc>
        string GetPathOfAssembly(AssemblyName name);
    }
}

