//---------------------------------------------------------------------
// <copyright file="EntityTypeAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// Attribute identifying the Edm base class
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EdmEntityTypeAttribute : EdmTypeAttribute
    {
        /// <summary>
        /// Attribute identifying the Edm base class
        /// </summary>
        public EdmEntityTypeAttribute()
        {
        }
    }
}
