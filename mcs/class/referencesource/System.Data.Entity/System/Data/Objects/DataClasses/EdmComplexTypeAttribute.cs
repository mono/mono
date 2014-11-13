//---------------------------------------------------------------------
// <copyright file="EdmComplexTypeAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.DataClasses
{
    using System;

    /// <summary>
    /// attribute for complex types
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [System.AttributeUsage(AttributeTargets.Class)]
    public sealed class EdmComplexTypeAttribute: EdmTypeAttribute
    {
        /// <summary>
        /// attribute for complex types
        /// </summary>
        public EdmComplexTypeAttribute()
        {
        }
    }
}
