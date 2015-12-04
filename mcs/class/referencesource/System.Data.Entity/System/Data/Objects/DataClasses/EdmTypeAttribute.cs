//---------------------------------------------------------------------
// <copyright file="EdmTypeAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Objects.DataClasses
{
    using System;
    using System.Diagnostics.CodeAnalysis;

#pragma warning disable 3015 // no accessible constructors which use only CLS-compliant types

    /// <summary>
    /// Base attribute for schematized types
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public abstract class EdmTypeAttribute: System.Attribute
    {
        private string _typeName;
        private string _namespaceName;

        /// <summary>
        /// Only allow derived attributes from this assembly
        /// </summary>
        internal EdmTypeAttribute()
        {
        }

        /// <summary>
        /// Returns the name of the type that this type maps to in the CSpace
        /// </summary>
        public string Name
        {
            get
            {
                return _typeName;
            }
            set
            {
                _typeName = value;
            }
        }

        /// <summary>
        /// Returns the namespace of the type that this type maps to in the CSpace
        /// </summary>
        public string NamespaceName
        {
            get
            {
                return _namespaceName;
            }
            set
            {
                _namespaceName = value;
            }
        }
    }
}
