//---------------------------------------------------------------------
// <copyright file="MetadataPropertyAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Attribute used to mark up properties that should appear in the MetadataItem.MetadataProperties collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class MetadataPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new attribute with built in type kind
        /// </summary>
        /// <param name="builtInTypeKind">Built in type setting Type property</param>
        /// <param name="isCollectionType">Sets IsCollectionType property</param>
        internal MetadataPropertyAttribute(BuiltInTypeKind builtInTypeKind, bool isCollectionType)
            : this(MetadataItem.GetBuiltInType(builtInTypeKind), isCollectionType)
        {
        }

        /// <summary>
        /// Initializes a new attribute with primitive type kind
        /// </summary>
        /// <param name="primitiveTypeKind">Primitive type setting Type property</param>
        /// <param name="isCollectionType">Sets IsCollectionType property</param>
        internal MetadataPropertyAttribute(PrimitiveTypeKind primitiveTypeKind, bool isCollectionType)
            : this(MetadataItem.EdmProviderManifest.GetPrimitiveType(primitiveTypeKind), isCollectionType)
        {
        }

        /// <summary>
        /// Initialize a new attribute with complex type kind (corresponding the the CLR type)
        /// </summary>
        /// <param name="type">CLR type setting Type property</param>
        /// <param name="isCollection">Sets IsCollectionType property</param>
        internal MetadataPropertyAttribute(Type type, bool isCollection)
            : this(ClrComplexType.CreateReadonlyClrComplexType(type, type.Namespace ?? string.Empty, type.Name), isCollection)
        {
        }

        /// <summary>
        /// Initialize a new attribute
        /// </summary>
        /// <param name="type">Sets Type property</param>
        /// <param name="isCollectionType">Sets IsCollectionType property</param>
        private MetadataPropertyAttribute(EdmType type, bool isCollectionType)
        {
            Debug.Assert(null != type);
            _type = type;
            _isCollectionType = isCollectionType;
        }

        private readonly EdmType _type;
        private readonly bool _isCollectionType;

        /// <summary>
        /// Gets EDM type for values stored in property.
        /// </summary>
        internal EdmType Type { get { return _type; } }

        /// <summary>
        /// Gets bool indicating whether this is a collection type.
        /// </summary>
        internal bool IsCollectionType { get { return _isCollectionType; } }
    }
}
