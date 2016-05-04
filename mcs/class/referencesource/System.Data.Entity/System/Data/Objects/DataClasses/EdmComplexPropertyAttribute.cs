//---------------------------------------------------------------------
// <copyright file="EdmComplexPropertyAttribute.cs" company="Microsoft">
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
    /// Attribute for complex properties
    /// Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    /// The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EdmComplexPropertyAttribute : EdmPropertyAttribute
    {
        /// <summary>
        /// Attribute for complex properties
        /// </summary>
        public EdmComplexPropertyAttribute()
        {
        }
    }
}
