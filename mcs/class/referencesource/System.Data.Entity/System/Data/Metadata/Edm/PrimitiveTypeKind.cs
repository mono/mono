//---------------------------------------------------------------------
// <copyright file="PrimitiveTypeKind.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Primitive Types as defined by EDM
    /// </summary>
    public enum PrimitiveTypeKind
    {
        /// <summary>
        /// Binary Type Kind
        /// </summary>
        Binary = 0,

        /// <summary>
        /// Boolean Type Kind
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// Byte Type Kind
        /// </summary>
        Byte = 2,

        /// <summary>
        /// DateTime Type Kind
        /// </summary>
        DateTime = 3,

        /// <summary>
        /// Decimal Type Kind
        /// </summary>
        Decimal = 4,

        /// <summary>
        /// Double Type Kind
        /// </summary>
        Double = 5,

        /// <summary>
        /// Guid Type Kind
        /// </summary>
        Guid = 6,

        /// <summary>
        /// Single Type Kind
        /// </summary>
        Single = 7,

        /// <summary>
        /// SByte Type Kind
        /// </summary>
        SByte = 8,

        /// <summary>
        /// Int16 Type Kind
        /// </summary>
        Int16 = 9,

        /// <summary>
        /// Int32 Type Kind
        /// </summary>
        Int32 = 10,

        /// <summary>
        /// Int64 Type Kind
        /// </summary>
        Int64 = 11,

        /// <summary>
        /// String Type Kind
        /// </summary>
        String = 12,

        /// <summary>
        /// Time Type Kind
        /// </summary>
        Time = 13,

        /// <summary>
        /// DateTimeOffset Type Kind
        /// </summary>
        DateTimeOffset = 14,

        /// <summary>
        /// Geometry Type Kind
        /// </summary>
        Geometry = 15,

        /// <summary>
        /// Geography Type Kind
        /// </summary>
        Geography = 16,

        /// <summary>
        /// Geometric point type kind
        /// </summary>
        GeometryPoint = 17,

        /// <summary>
        /// Geometric linestring type kind
        /// </summary>
        GeometryLineString = 18,

        /// <summary>
        /// Geometric polygon type kind
        /// </summary>
        GeometryPolygon = 19,

        /// <summary>
        /// Geometric multi-point type kind
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702", MessageId = "MultiPoint")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi")]
        GeometryMultiPoint = 20,

        /// <summary>
        /// Geometric multi-linestring type kind
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702", MessageId = "MultiLine")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi")]
        GeometryMultiLineString = 21,

        /// <summary>
        /// Geometric multi-polygon type kind
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi")]
        GeometryMultiPolygon = 22,

        /// <summary>
        /// Geometric collection type kind
        /// </summary>
        GeometryCollection = 23,

        /// <summary>
        /// Geographic point type kind
        /// </summary>
        GeographyPoint = 24,

        /// <summary>
        /// Geographic linestring type kind
        /// </summary>
        GeographyLineString = 25,

        /// <summary>
        /// Geographic polygon type kind
        /// </summary>
        GeographyPolygon = 26,

        /// <summary>
        /// Geographic multi-point type kind
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702", MessageId = "MultiPoint")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi")]
        GeographyMultiPoint = 27,

        /// <summary>
        /// Geographic multi-linestring type kind
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702", MessageId = "MultiLine")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi")]
        GeographyMultiLineString = 28,

        /// <summary>
        /// Geographic multi-polygon type kind
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Multi")]
        GeographyMultiPolygon = 29,

        /// <summary>
        /// Geographic collection type kind
        /// </summary>
        GeographyCollection = 30,

        //
        //If you add anything below this, make sure you update the variable NumPrimitiveTypes in EdmConstants
        //
    }
}
