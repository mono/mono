//---------------------------------------------------------------------
// <copyright file="EdmEnumTypeAttribute.cs" company="Microsoft">
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
    /// Attribute indicating an enum type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [System.AttributeUsage(AttributeTargets.Enum)]
    public sealed class EdmEnumTypeAttribute : EdmTypeAttribute
    {
        /// <summary>
        /// Initializes a new instance of EdmEnumTypeAttribute class.
        /// </summary>
        public EdmEnumTypeAttribute()
        {
        }
    }
}
