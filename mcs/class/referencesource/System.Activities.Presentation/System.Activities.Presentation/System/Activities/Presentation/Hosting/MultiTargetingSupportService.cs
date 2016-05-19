//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Hosting
{
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime;

    /// <summary>
    /// abstract class for multi-targeting support service
    /// </summary>
    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly,
                     Justification = "MultiTargetingSupportService is the correct name")]
    public abstract class MultiTargetingSupportService : IMultiTargetingSupportService
    {
        /// <summary>
        /// Get reflection assembly given an assembly name
        /// </summary>
        /// <param name="targetAssemblyName">target assembly name</param>
        /// <returns>reflection assembly if target assembly name could be resolved</returns>
        public abstract Assembly GetReflectionAssembly(AssemblyName targetAssemblyName);

        /// <summary>
        /// Get runtime type given a reflection type
        /// </summary>
        /// <param name="reflectionType">reflection type</param>
        /// <returns>runtime type associated with the reflection type</returns>
        public abstract Type GetRuntimeType(Type reflectionType);

        /// <summary>
        /// Check if a given type is supported by target framework
        /// </summary>
        /// <param name="type">type to be checkec</param>
        /// <returns>true is type is supported by target framework</returns>
        public abstract bool IsSupportedType(Type type);

        /// <summary>
        /// Get the reflection type give an object type
        /// </summary>
        /// <param name="objectType">object type</param>
        /// <returns>reflection type</returns>
        public abstract Type GetReflectionType(Type objectType);
    }
}
