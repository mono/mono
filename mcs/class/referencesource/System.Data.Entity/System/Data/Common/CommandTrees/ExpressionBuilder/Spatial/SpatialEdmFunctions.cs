
//---------------------------------------------------------------------
// <copyright file="SpatialEdmFunctions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.CommandTrees.ExpressionBuilder.Spatial
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Provides an API to construct <see cref="DbExpression"/>s that invoke spatial realted canonical EDM functions, and, where appropriate, allows that API to be accessed as extension methods on the expression type itself.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public static class SpatialEdmFunctions
    {
        #region Spatial Functions - Geometry well known text Constructors

        // Geometry ‘Static’ Functions
        // Geometry – well known text Constructors

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryFromText' function with the
        /// specified argument, which must have a string result type.
        /// The result type of the expression is Edm.Geometry.  Its value has the default coordinate system id (SRID) of the underlying provider.
        /// </summary>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geometry value.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryFromText' function accept an argument with the result type of <paramref name="wellKnownText"/>.</exception>
        public static DbFunctionExpression GeometryFromText(DbExpression wellKnownText)
        {
            EntityUtil.CheckArgumentNull(wellKnownText, "wellKnownText");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromText", wellKnownText);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryFromText' function with the
        /// specified arguments. <paramref name="wellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryFromText' function accepts arguments with the result types of <paramref name="wellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryFromText(DbExpression wellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(wellKnownText, "wellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromText", wellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryPointFromText' function with the
        /// specified arguments. <paramref name="pointWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="pointWellKnownText">An expression that provides the well known text representation of the geometry point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryPointFromText' function accepts arguments with the result types of <paramref name="pointWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryPointFromText(DbExpression pointWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(pointWellKnownText, "pointWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPointFromText", pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryLineFromText' function with the
        /// specified arguments. <paramref name="lineWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="lineWellKnownText">An expression that provides the well known text representation of the geometry line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryLineFromText' function accepts arguments with the result types of <paramref name="lineWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryLineFromText(DbExpression lineWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(lineWellKnownText, "lineWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryLineFromText", lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryPolygonFromText' function with the
        /// specified arguments. <paramref name="polygonWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="polygonWellKnownText">An expression that provides the well known text representation of the geometry polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryPolygonFromText' function accepts arguments with the result types of <paramref name="polygonWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryPolygonFromText(DbExpression polygonWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(polygonWellKnownText, "polygonWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPolygonFromText", polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryMultiPointFromText' function with the
        /// specified arguments. <paramref name="multiPointWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="multiPointWellKnownText">An expression that provides the well known text representation of the geometry multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryMultiPointFromText' function accepts arguments with the result types of <paramref name="multiPointWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPointFromText(DbExpression multiPointWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPointWellKnownText, "multiPointWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPointFromText", multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryMultiLineFromText' function with the
        /// specified arguments. <paramref name="multiLineWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="multiLineWellKnownText">An expression that provides the well known text representation of the geometry multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryMultiLineFromText' function accepts arguments with the result types of <paramref name="multiLineWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiLineFromText(DbExpression multiLineWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiLineWellKnownText, "multiLineWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiLineFromText", multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryMultiPolygonFromText' function with the
        /// specified arguments. <paramref name="multiPolygonWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="multiPolygonWellKnownText">An expression that provides the well known text representation of the geometry multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryMultiPolygonFromText' function accepts arguments with the result types of <paramref name="multiPolygonWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPolygonFromText(DbExpression multiPolygonWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPolygonFromText", multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryCollectionFromText' function with the
        /// specified arguments. <paramref name="geometryCollectionWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryCollectionWellKnownText">An expression that provides the well known text representation of the geometry collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry collection value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry collection value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryCollectionWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryCollectionFromText' function accepts arguments with the result types of <paramref name="geometryCollectionWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryCollectionFromText(DbExpression geometryCollectionWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(geometryCollectionWellKnownText, "geometryCollectionWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryCollectionFromText", geometryCollectionWellKnownText, coordinateSystemId);
        }

        #endregion 
        
        #region Spatial Functions - Geometry Well Known Binary Constructors

        // Geometry – Well Known Binary Constructors

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryFromBinary' function with the
        /// specified argument, which must have a binary result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geometry value.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified binary value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinaryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryFromBinary' function accepts an argument with the result type of <paramref name="wellKnownBinaryValue"/>.</exception>
        public static DbFunctionExpression GeometryFromBinary(DbExpression wellKnownBinaryValue)
        {
            EntityUtil.CheckArgumentNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromBinary", wellKnownBinaryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryFromBinary' function with the
        /// specified arguments. <paramref name="wellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryFromBinary' function accepts arguments with the result types of <paramref name="wellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryFromBinary(DbExpression wellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromBinary", wellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryPointFromBinary' function with the
        /// specified arguments. <paramref name="pointWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="pointWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryPointFromBinary' function accepts arguments with the result types of <paramref name="pointWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryPointFromBinary(DbExpression pointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(pointWellKnownBinaryValue, "pointWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPointFromBinary", pointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryLineFromBinary' function with the
        /// specified arguments. <paramref name="lineWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="lineWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryLineFromBinary' function accepts arguments with the result types of <paramref name="lineWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryLineFromBinary(DbExpression lineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(lineWellKnownBinaryValue, "lineWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryLineFromBinary", lineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryPolygonFromBinary' function with the
        /// specified arguments. <paramref name="polygonWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="polygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryPolygonFromBinary' function accepts arguments with the result types of <paramref name="polygonWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryPolygonFromBinary(DbExpression polygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(polygonWellKnownBinaryValue, "polygonWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryPolygonFromBinary", polygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryMultiPointFromBinary' function with the
        /// specified arguments. <paramref name="multiPointWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="multiPointWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryMultiPointFromBinary' function accepts arguments with the result types of <paramref name="multiPointWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPointFromBinary(DbExpression multiPointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPointWellKnownBinaryValue, "multiPointWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPointFromBinary", multiPointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryMultiLineFromBinary' function with the
        /// specified arguments. <paramref name="multiLineWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="multiLineWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryMultiLineFromBinary' function accepts arguments with the result types of <paramref name="multiLineWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiLineFromBinary(DbExpression multiLineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiLineWellKnownBinaryValue, "multiLineWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiLineFromBinary", multiLineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryMultiPolygonFromBinary' function with the
        /// specified arguments. <paramref name="multiPolygonWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="multiPolygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry multi-polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry multi-polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryMultiPolygonFromBinary' function accepts arguments with the result types of <paramref name="multiPolygonWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeometryMultiPolygonFromBinary(DbExpression multiPolygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPolygonWellKnownBinaryValue, "multiPolygonWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryMultiPolygonFromBinary", multiPolygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryCollectionFromBinary' function with the
        /// specified arguments. <paramref name="geometryCollectionWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryCollectionWellKnownBinaryValue">An expression that provides the well known binary representation of the geometry collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry collection value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry collection value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryCollectionWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryCollectionFromBinary' function accepts arguments with the result types of <paramref name="geometryCollectionWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeometryCollectionFromBinary(DbExpression geometryCollectionWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(geometryCollectionWellKnownBinaryValue, "geometryCollectionWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryCollectionFromBinary", geometryCollectionWellKnownBinaryValue, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geometry GML Constructors (non-OGC)

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryFromGml' function with the
        /// specified argument, which must have a string result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryMarkup">An expression that provides the Geography Markup Language (GML) representation of the geometry value.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified value with the default coordinate system id (SRID) of the underlying provider.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryMarkup"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryFromGml' function accepts an argument with the result type of <paramref name="geometryMarkup"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml", Justification = "Abbreviation more meaningful than what it stands for")]
        public static DbFunctionExpression GeometryFromGml(DbExpression geometryMarkup)
        {
            EntityUtil.CheckArgumentNull(geometryMarkup, "geometryMarkup");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromGml", geometryMarkup);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeometryFromGml' function with the
        /// specified arguments. <paramref name="geometryMarkup"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryMarkup">An expression that provides the Geography Markup Language (GML) representation of the geometry value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geometry value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geometry value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryMarkup"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeometryFromGml' function accepts arguments with the result types of <paramref name="geometryMarkup"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml", Justification = "Abbreviation more meaningful than what it stands for")]
        public static DbFunctionExpression GeometryFromGml(DbExpression geometryMarkup, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(geometryMarkup, "geometryMarkup");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeometryFromGml", geometryMarkup, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geography well known text Constructors

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyFromText' function with the
        /// specified argument, which must have a string result type.
        /// The result type of the expression is Edm.Geography.   Its value has the default coordinate system id (SRID) of the underlying provider.
        /// </summary>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geography value.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyFromText' function accepts an argument with the result type of <paramref name="wellKnownText"/>.</exception>
        public static DbFunctionExpression GeographyFromText(DbExpression wellKnownText)
        {
            EntityUtil.CheckArgumentNull(wellKnownText, "wellKnownText");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromText", wellKnownText);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyFromText' function with the
        /// specified arguments. <paramref name="wellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="wellKnownText">An expression that provides the well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyFromText' function accepts arguments with the result types of <paramref name="wellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyFromText(DbExpression wellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(wellKnownText, "wellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromText", wellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyPointFromText' function with the
        /// specified arguments. <paramref name="pointWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="pointWellKnownText">An expression that provides the well known text representation of the geography point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</excpointTexteption>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyPointFromText' function accepts arguments with the result types of <paramref name="pointWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyPointFromText(DbExpression pointWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(pointWellKnownText, "pointWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPointFromText", pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyLineFromText' function with the
        /// specified arguments. <paramref name="lineWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="lineWellKnownText">An expression that provides the well known text representation of the geography line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyLineFromText' function accepts arguments with the result types of <paramref name="lineWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyLineFromText(DbExpression lineWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(lineWellKnownText, "lineWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyLineFromText", lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyPolygonFromText' function with the
        /// specified arguments. <paramref name="polygonWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="polygonWellKnownText">An expression that provides the well known text representation of the geography polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyPolygonFromText' function accepts arguments with the result types of <paramref name="polygonWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyPolygonFromText(DbExpression polygonWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(polygonWellKnownText, "polygonWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPolygonFromText", polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyMultiPointFromText' function with the
        /// specified arguments. <paramref name="multiPointWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="multiPointWellKnownText">An expression that provides the well known text representation of the geography multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyMultiPointFromText' function accepts arguments with the result types of <paramref name="multiPointWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPointFromText(DbExpression multiPointWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPointWellKnownText, "multiPointWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPointFromText", multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyMultiLineFromText' function with the
        /// specified arguments. <paramref name="multiLineWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="multiLineWellKnownText">An expression that provides the well known text representation of the geography multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyMultiLineFromText' function accepts arguments with the result types of <paramref name="multiLineWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiLineFromText(DbExpression multiLineWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiLineWellKnownText, "multiLineWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiLineFromText", multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyMultiPolygonFromText' function with the
        /// specified arguments. <paramref name="multiPolygonWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="multiPolygonWellKnownText">An expression that provides the well known text representation of the geography multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyMultiPolygonFromText' function accepts arguments with the result types of <paramref name="multiPolygonWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPolygonFromText(DbExpression multiPolygonWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPolygonWellKnownText, "multiPolygonWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPolygonFromText", multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyCollectionFromText' function with the
        /// specified arguments. <paramref name="geographyCollectionWellKnownText"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="geographyCollectionWellKnownText">An expression that provides the well known text representation of the geography collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography collection value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography collection value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownText"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyCollectionFromText' function accepts arguments with the result types of <paramref name="geographyCollectionWellKnownText"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyCollectionFromText(DbExpression geographyCollectionWellKnownText, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(geographyCollectionWellKnownText, "geographyCollectionWellKnownText");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyCollectionFromText", geographyCollectionWellKnownText, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geography Well Known Binary Constructors

        // Geography – Well Known Binary Constructors

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyFromBinary' function with the
        /// specified argument, which must have a binary result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geography value.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified binary value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinaryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyFromBinary' function accepts an argument with the result type of <paramref name="wellKnownBinaryValue"/>.</exception>
        public static DbFunctionExpression GeographyFromBinary(DbExpression wellKnownBinaryValue)
        {
            EntityUtil.CheckArgumentNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromBinary", wellKnownBinaryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyFromBinary' function with the
        /// specified arguments. <paramref name="wellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="wellKnownBinaryValue">An expression that provides the well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyFromBinary' function accepts arguments with the result types of <paramref name="wellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyFromBinary(DbExpression wellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(wellKnownBinaryValue, "wellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromBinary", wellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyPointFromBinary' function with the
        /// specified arguments. <paramref name="pointWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="pointWellKnownBinaryValue">An expression that provides the well known binary representation of the geography point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography point value's coordinate systempointWellKnownBinaryValue.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyPointFromBinary' function accepts arguments with the result types of <paramref name="pointWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyPointFromBinary(DbExpression pointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(pointWellKnownBinaryValue, "pointWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPointFromBinary", pointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyLineFromBinary' function with the
        /// specified arguments. <paramref name="lineWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="lineWellKnownBinaryValue">An expression that provides the well known binary representation of the geography line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyLineFromBinary' function accepts arguments with the result types of <paramref name="lineWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyLineFromBinary(DbExpression lineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(lineWellKnownBinaryValue, "lineWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyLineFromBinary", lineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyPolygonFromBinary' function with the
        /// specified arguments. <paramref name="polygonWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="polygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geography polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyPolygonFromBinary' function accepts arguments with the result types of <paramref name="polygonWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyPolygonFromBinary(DbExpression polygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(polygonWellKnownBinaryValue, "polygonWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyPolygonFromBinary", polygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyMultiPointFromBinary' function with the
        /// specified arguments. <paramref name="multiPointWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="multiPointWellKnownBinaryValue">An expression that provides the well known binary representation of the geography multi-point value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-point value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-point value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyMultiPointFromBinary' function accepts arguments with the result types of <paramref name="multiPointWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPointFromBinary(DbExpression multiPointWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPointWellKnownBinaryValue, "multiPointWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPointFromBinary", multiPointWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyMultiLineFromBinary' function with the
        /// specified arguments. <paramref name="multiLineWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="multiLineWellKnownBinaryValue">An expression that provides the well known binary representation of the geography multi-line value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-line value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-line value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyMultiLineFromBinary' function accepts arguments with the result types of <paramref name="multiLineWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiLineFromBinary(DbExpression multiLineWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiLineWellKnownBinaryValue, "multiLineWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiLineFromBinary", multiLineWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyMultiPolygonFromBinary' function with the
        /// specified arguments. <paramref name="multiPolygonWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="multiPolygonWellKnownBinaryValue">An expression that provides the well known binary representation of the geography multi-polygon value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography multi-polygon value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography multi-polygon value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>polygonWellKnownBinaryValue
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyMultiPolygonFromBinary' function accepts arguments with the result types of <paramref name="multiPolygonWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbFunctionExpression GeographyMultiPolygonFromBinary(DbExpression multiPolygonWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(multiPolygonWellKnownBinaryValue, "multiPolygonWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyMultiPolygonFromBinary", multiPolygonWellKnownBinaryValue, coordinateSystemId);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyCollectionFromBinary' function with the
        /// specified arguments. <paramref name="geographyCollectionWellKnownBinaryValue"/> must have a binary result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="geographyCollectionWellKnownBinaryValue">An expression that provides the well known binary representation of the geography collection value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography collection value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography collection value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownBinaryValue"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyCollectionFromBinary' function accepts arguments with the result types of <paramref name="geographyCollectionWellKnownBinaryValue"/> and <paramref name="coordinateSystemId"/>.</exception>
        public static DbFunctionExpression GeographyCollectionFromBinary(DbExpression geographyCollectionWellKnownBinaryValue, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(geographyCollectionWellKnownBinaryValue, "geographyCollectionWellKnownBinaryValue");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyCollectionFromBinary", geographyCollectionWellKnownBinaryValue, coordinateSystemId);
        }

        #endregion

        #region Spatial Functions - Geography GML Constructors (non-OGC)

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyFromGml' function with the
        /// specified argument, which must have a string result type. The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="geographyMarkup">An expression that provides the Geography Markup Language (GML) representation of the geography value.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified value with the default coordinate system id (SRID) of the underlying provider.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyFromGml' function accepts an argument with the result type of <paramref name="geographyMarkup"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbFunctionExpression GeographyFromGml(DbExpression geographyMarkup)
        {
            EntityUtil.CheckArgumentNull(geographyMarkup, "geographyMarkup");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromGml", geographyMarkup);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'GeographyFromGml' function with the
        /// specified arguments. <paramref name="geographyMarkup"/> must have a string result type, while <paramref name="coordinateSystemId"/> must have an integer numeric result type.
        /// The result type of the expression is Edm.Geography.
        /// </summary>
        /// <param name="geographyMarkup">An expression that provides the Geography Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">An expression that provides the coordinate system id (SRID) of the geography value's coordinate system.</param>
        /// <returns>A new DbFunctionExpression that returns a new geography value based on the specified values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> or <paramref name="coordinateSystemId"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'GeographyFromGml' function accepts arguments with the result types of <paramref name="geographyMarkup"/> and <paramref name="coordinateSystemId"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbFunctionExpression GeographyFromGml(DbExpression geographyMarkup, DbExpression coordinateSystemId)
        {
            EntityUtil.CheckArgumentNull(geographyMarkup, "geographyMarkup");
            EntityUtil.CheckArgumentNull(coordinateSystemId, "coordinateSystemId");
            return EdmFunctions.InvokeCanonicalFunction("GeographyFromGml", geographyMarkup, coordinateSystemId);
        }
             
        #endregion

        #region Spatial Functions - Instance Member Access

        // Spatial ‘Instance’ Functions
        // Spatial Member Access
        
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'CoordinateSystemId' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the value from which the coordinate system id (SRID) should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the integer SRID value from <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'CoordinateSystemId' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression CoordinateSystemId(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("CoordinateSystemId", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialTypeName' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.String.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the value from which the Geometry Type name should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the string Geometry Type name from <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialTypeName' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression SpatialTypeName(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialTypeName", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialDimension' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.Int32.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the value from which the Dimension value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the Dimension value from <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialDimension' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression SpatialDimension(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialDimension", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialEnvelope' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of
        /// the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the value from which the Envelope value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the the minimum bounding box for <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialEnvelope' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression SpatialEnvelope(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialEnvelope", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AsBinary' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.Binary.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the well known binary representation should be produced.</param>
        /// <returns>A new DbFunctionExpression that returns the well known binary representation of <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AsBinary' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression AsBinary(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("AsBinary", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AsGml' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.String.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the Geography Markup Language (GML) representation should be produced.</param>
        /// <returns>A new DbFunctionExpression that returns the Geography Markup Language (GML) representation of <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AsGml' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbFunctionExpression AsGml(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("AsGml", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'AsText' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.String.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the well known text representation should be produced.</param>
        /// <returns>A new DbFunctionExpression that returns the well known text representation of <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'AsText' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression AsText(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("AsText", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'IsEmptySpatial' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of
        /// the expression is Edm.Boolean.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial value from which the IsEmptySptiaal value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="spatialValue"/> is empty.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'IsEmptySpatial' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression IsEmptySpatial(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("IsEmptySpatial", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'IsSimpleGeometry' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of
        /// the expression is Edm.Boolean.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the geometry value from which the IsSimpleGeometry value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue"/> is a simple geometry.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'IsSimpleGeometry' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression IsSimpleGeometry(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("IsSimpleGeometry", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialBoundary' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry value from which the SpatialBoundary value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the the boundary for <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialBoundary' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression SpatialBoundary(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialBoundary", geometryValue);
        }

        // Non-OGC
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'IsValidGeometry' function with the specified argument,
        /// which must have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry value which should be tested for spatial validity.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue"/> is valid.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'IsValidGeometry' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression IsValidGeometry(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("IsValidGeometry", geometryValue);
        }

        #endregion

        #region Spatial Functions - Spatial Relation

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialEquals' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value that should be compared with <paramref name="spatialValue1"/> for equality.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/> are equal.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialEquals' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialEquals(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialEquals", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialDisjoint' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value that should be compared with <paramref name="spatialValue1"/> for disjointness.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/> are spatially disjoint.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialDisjoint' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialDisjoint(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialDisjoint", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialIntersects' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value that should be compared with <paramref name="spatialValue1"/> for intersection.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/> intersect.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialIntersects' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialIntersects(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialIntersects", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialTouches' function with the specified arguments,
        /// which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with <paramref name="geometryValue1"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue1"/> touches <paramref name="geometryValue2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue1"/> or <paramref name="geometryValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialTouches' function accepts arguments with the result types of <paramref name="geometryValue1"/> and <paramref name="geometryValue2"/>.</exception>
        public static DbFunctionExpression SpatialTouches(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            EntityUtil.CheckArgumentNull(geometryValue1, "geometryValue1");
            EntityUtil.CheckArgumentNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialTouches", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialCrosses' function with the specified arguments,
        /// which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with <paramref name="geometryValue1"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue1"/> crosses <paramref name="geometryValue2"/> intersect.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue1"/> or <paramref name="geometryValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialCrosses' function accepts arguments with the result types of <paramref name="geometryValue1"/> and <paramref name="geometryValue2"/>.</exception>
        public static DbFunctionExpression SpatialCrosses(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            EntityUtil.CheckArgumentNull(geometryValue1, "geometryValue1");
            EntityUtil.CheckArgumentNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialCrosses", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialWithin' function with the specified arguments,
        /// which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with <paramref name="geometryValue1"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue1"/> is spatially within <paramref name="geometryValue2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue1"/> or <paramref name="geometryValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialWithin' function accepts arguments with the result types of <paramref name="geometryValue1"/> and <paramref name="geometryValue2"/>.</exception>
        public static DbFunctionExpression SpatialWithin(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            EntityUtil.CheckArgumentNull(geometryValue1, "geometryValue1");
            EntityUtil.CheckArgumentNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialWithin", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialContains' function with the specified arguments,
        /// which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with <paramref name="geometryValue1"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue1"/> spatially contains <paramref name="geometryValue2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue1"/> or <paramref name="geometryValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialContains' function accepts arguments with the result types of <paramref name="geometryValue1"/> and <paramref name="geometryValue2"/>.</exception>
        public static DbFunctionExpression SpatialContains(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            EntityUtil.CheckArgumentNull(geometryValue1, "geometryValue1");
            EntityUtil.CheckArgumentNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialContains", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialOverlaps' function with the specified arguments,
        /// which must each have an Edm.Geometry result type. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with <paramref name="geometryValue1"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue1"/> spatially overlaps <paramref name="geometryValue2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue1"/> or <paramref name="geometryValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialOverlaps' function accepts arguments with the result types of <paramref name="geometryValue1"/> and <paramref name="geometryValue2"/>.</exception>
        public static DbFunctionExpression SpatialOverlaps(this DbExpression geometryValue1, DbExpression geometryValue2)
        {
            EntityUtil.CheckArgumentNull(geometryValue1, "geometryValue1");
            EntityUtil.CheckArgumentNull(geometryValue2, "geometryValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialOverlaps", geometryValue1, geometryValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialRelate' function with the specified arguments,
        /// which must have Edm.Geometry and string result types. The result type of the expression is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue1">An expression that specifies the first geometry value.</param>
        /// <param name="geometryValue2">An expression that specifies the geometry value that should be compared with <paramref name="geometryValue1"/>.</param>
        /// <param name="intersectionPatternMatrix">An expression that specifies the text representation of the Dimensionally Extended Nine-Intersection Model (DE-9IM) intersection pattern used to compare <paramref name="geometryValue1"/> and <paramref name="geometryValue2"/>.</param>
        /// <returns>A new DbFunctionExpression that returns a Boolean value indicating whether <paramref name="geometryValue1"/> is spatially related to <paramref name="geometryValue2"/> according to the spatial relationship designated by <paramref name="intersectionPatternMatrix"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue1"/>, <paramref name="geometryValue2"/> or <paramref name="intersectionPatternMatrix"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialRelate' function accepts arguments with the result types of <paramref name="geometryValue1"/>, <paramref name="geometryValue2"/>, and <paramref name="intersectionPatternMatrix"/>.</exception>
        public static DbFunctionExpression SpatialRelate(this DbExpression geometryValue1, DbExpression geometryValue2, DbExpression intersectionPatternMatrix)
        {
            EntityUtil.CheckArgumentNull(geometryValue1, "geometryValue1");
            EntityUtil.CheckArgumentNull(geometryValue2, "geometryValue2");
            EntityUtil.CheckArgumentNull(intersectionPatternMatrix, "intersectionPatternMatrix");
            return EdmFunctions.InvokeCanonicalFunction("SpatialRelate", geometryValue1, geometryValue2, intersectionPatternMatrix);
        }

        #endregion

        #region Spatial Functions - Spatial Analysis

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialBuffer' function with the specified arguments,
        /// which must have a Edm.Geography or Edm.Geometry and Edm.Double result types. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial value.</param>
        /// <param name="distance">An expression that specifies the buffer distance.</param>
        /// <returns>A new DbFunctionExpression that returns a geometry value representing all points less than or equal to <paramref name="distance"/> from <paramref name="spatialValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> or <paramref name="distance"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialBuffer' function accepts arguments with the result types of <paramref name="spatialValue"/> and <paramref name="distance"/>.</exception>
        public static DbFunctionExpression SpatialBuffer(this DbExpression spatialValue, DbExpression distance)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            EntityUtil.CheckArgumentNull(distance, "distance");
            return EdmFunctions.InvokeCanonicalFunction("SpatialBuffer", spatialValue, distance);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Distance' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type. 
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value from which the distance from <paramref name="spatialValue1"/> should be measured.</param>
        /// <returns>A new DbFunctionExpression that returns the distance between the closest points in <paramref name="spatialValue1"/> and <paramref name="spatialValue1"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Distance' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression Distance(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("Distance", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialConvexHull' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry value from which the convex hull value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns the the convex hull for <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialConvexHull' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression SpatialConvexHull(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialConvexHull", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialIntersection' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is the same as the type of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the intersection with <paramref name="spatialValue1"/> should be computed.</param>
        /// <returns>A new DbFunctionExpression that returns the spatial value representing the intersection of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialIntersection' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialIntersection(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialIntersection", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialUnion' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is the same as the type of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the union with <paramref name="spatialValue1"/> should be computed.</param>
        /// <returns>A new DbFunctionExpression that returns the spatial value representing the union of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialUnion' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialUnion(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialUnion", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialDifference' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is the same as the type of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the difference with <paramref name="spatialValue1"/> should be computed.</param>
        /// <returns>A new DbFunctionExpression that returns the geometry value representing the difference of <paramref name="spatialValue2"/> with <paramref name="spatialValue1"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialDifference' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialDifference(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialDifference", spatialValue1, spatialValue2);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialSymmetricDifference' function with the specified arguments,
        /// which must each have an Edm.Geography or Edm.Geometry result type.
        /// The result type of <paramref name="spatialValue1"/> must match the result type of <paramref name="spatialValue2"/>.
        /// The result type of the expression is the same as the type of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.
        /// </summary>
        /// <param name="spatialValue1">An expression that specifies the first spatial value.</param>
        /// <param name="spatialValue2">An expression that specifies the spatial value for which the symmetric difference with <paramref name="spatialValue1"/> should be computed.</param>
        /// <returns>A new DbFunctionExpression that returns the geometry value representing the symmetric difference of <paramref name="spatialValue2"/> with <paramref name="spatialValue1"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue1"/> or <paramref name="spatialValue2"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialSymmetricDifference' function accepts arguments with the result types of <paramref name="spatialValue1"/> and <paramref name="spatialValue2"/>.</exception>
        public static DbFunctionExpression SpatialSymmetricDifference(this DbExpression spatialValue1, DbExpression spatialValue2)
        {
            EntityUtil.CheckArgumentNull(spatialValue1, "spatialValue1");
            EntityUtil.CheckArgumentNull(spatialValue2, "spatialValue2");
            return EdmFunctions.InvokeCanonicalFunction("SpatialSymmetricDifference", spatialValue1, spatialValue2);
        }

        #endregion

        #region Spatial Functions - Spatial Collection

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialElementCount' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the geography or geometry collection value from which the number of elements should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the number of elements in <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialElementCount' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression SpatialElementCount(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialElementCount", spatialValue);
        }

        
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialElementAt' function with the
        /// specified arguments.   The first argument must have an Edm.Geography or Edm.Geometry result type.   
        /// The second argument must have an integer numeric result type. The result type of the expression is the same as that of <paramref name="spatialValue"/>.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the geography or geometry collection value.</param>
        /// <param name="indexValue">An expression that specifies the position of the element to be retrieved from within the geometry or geography collection.</param>
        /// <returns>A new DbFunctionExpression that returns either the collection element at position <paramref name="indexValue"/> in <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="indexValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialElementAt' function accepts arguments with the result types of <paramref name="spatialValue"/> and <paramref name="indexValue"/>.</exception>
        public static DbFunctionExpression SpatialElementAt(this DbExpression spatialValue, DbExpression indexValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            EntityUtil.CheckArgumentNull(indexValue, "indexValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialElementAt", spatialValue, indexValue);
        }


        #endregion

        #region Spatial Functions - GeographyPoint

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'XCoordinate' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry point value from which the X co-ordinate value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the X co-ordinate value of <paramref name="geometryValue"/> or <c>null</c> if <paramref name="geometryValue"/> is not a point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'XCoordinate' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression XCoordinate(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("XCoordinate", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'YCoordinate' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry point value from which the Y co-ordinate value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the Y co-ordinate value of <paramref name="geometryValue"/> or <c>null</c> if <paramref name="geometryValue"/> is not a point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'YCoordinate' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression YCoordinate(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("YCoordinate", geometryValue);
        }
                
        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Elevation' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial point value from which the elevation (Z co-ordinate) value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the elevation value of <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Elevation' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression Elevation(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("Elevation", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Measure' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial point value from which the Measure (M) co-ordinate value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the Measure of <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Measure' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression Measure(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("Measure", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Latitude' function with the
        /// specified argument, which must have an Edm.Geography result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="geographyValue">An expression that specifies the geography point value from which the Latitude value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the Latitude value of <paramref name="geographyValue"/> or <c>null</c> if <paramref name="geographyValue"/> is not a point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Latitude' function accepts an argument with the result type of <paramref name="geographyValue"/>.</exception>
        public static DbFunctionExpression Latitude(this DbExpression geographyValue)
        {
            EntityUtil.CheckArgumentNull(geographyValue, "geographyValue");
            return EdmFunctions.InvokeCanonicalFunction("Latitude", geographyValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Longitude' function with the
        /// specified argument, which must have an Edm.Geography result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="geographyValue">An expression that specifies the geography point value from which the Longitude value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the Longitude value of <paramref name="geographyValue"/> or <c>null</c> if <paramref name="geographyValue"/> is not a point.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Longitude' function accepts an argument with the result type of <paramref name="geographyValue"/>.</exception>
        public static DbFunctionExpression Longitude(this DbExpression geographyValue)
        {
            EntityUtil.CheckArgumentNull(geographyValue, "geographyValue");
            return EdmFunctions.InvokeCanonicalFunction("Longitude", geographyValue);
        }

        #endregion

        #region Spatial Functions - Curve

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'SpatialLength' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the length should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the length of <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a curve.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'SpatialLength' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression SpatialLength(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("SpatialLength", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'StartPoint' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type is the same as that of <paramref name="spatialValue"/>.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the start point should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the start point of <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a curve.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'StartPoint' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression StartPoint(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("StartPoint", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'EndPoint' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type is the same as that of <paramref name="spatialValue"/>.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the end point should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the end point of <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a curve.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'EndPoint' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression EndPoint(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("EndPoint", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'IsClosedSpatial' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type is Edm.Boolean.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial curve value from which the IsClosedSpatial value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either a Boolean value indicating whether <paramref name="spatialValue"/> is closed, or <c>null</c> if <paramref name="spatialValue"/> is not a curve.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'IsClosedSpatial' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression IsClosedSpatial(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("IsClosedSpatial", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'IsRing' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type is Edm.Boolean.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry curve value from which the IsRing value should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either a Boolean value indicating whether <paramref name="geometryValue"/> is a ring (both closed and simple), or <c>null</c> if <paramref name="geometryValue"/> is not a curve.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'IsRing' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression IsRing(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("IsRing", geometryValue);
        }

        #endregion

        #region Spatial Functions - GeographyLineString, Line, LinearRing

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'PointCount' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial line string value from which the number of points should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the number of points in <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a line string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'PointCount' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression PointCount(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("PointCount", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'PointAt' function with the
        /// specified arguments.   The first argument must have an Edm.Geography or Edm.Geometry result type.   The second argument must have an integer numeric result type.
        /// The result type of the expression is the same as that of <paramref name="spatialValue"/>.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial line string value.</param>
        /// <param name="indexValue">An expression that specifies the position of the point to be retrieved from within the line string.</param>
        /// <returns>A new DbFunctionExpression that returns either the point at position <paramref name="indexValue"/> in <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a line string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="indexValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'PointAt' function accepts arguments with the result types of <paramref name="spatialValue"/> and <paramref name="indexValue"/>.</exception>
        public static DbFunctionExpression PointAt(this DbExpression spatialValue, DbExpression indexValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            EntityUtil.CheckArgumentNull(indexValue, "indexValue");
            return EdmFunctions.InvokeCanonicalFunction("PointAt", spatialValue, indexValue);
        }

        #endregion

        #region Spatial Functions - Surface

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Area' function with the
        /// specified argument, which must have an Edm.Geography or Edm.Geometry result type.  The result type of the expression is Edm.Double.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the spatial surface value for which the area should be calculated.</param>
        /// <returns>A new DbFunctionExpression that returns either the area of <paramref name="spatialValue"/> or <c>null</c> if <paramref name="spatialValue"/> is not a surface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Area' function accepts an argument with the result type of <paramref name="spatialValue"/>.</exception>
        public static DbFunctionExpression Area(this DbExpression spatialValue)
        {
            EntityUtil.CheckArgumentNull(spatialValue, "spatialValue");
            return EdmFunctions.InvokeCanonicalFunction("Area", spatialValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'Centroid' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry surface value from which the centroid should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the centroid point of <paramref name="geometryValue"/> (which may not be on the surface itself) or <c>null</c> if <paramref name="geometryValue"/> is not a surface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'Centroid' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Centroid", Justification = "Standard bame")]
        public static DbFunctionExpression Centroid(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("Centroid", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'PointOnSurface' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="geometryValue">An expression that specifies the geometry surface value from which the point should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either a point guaranteed to be on the surface <paramref name="geometryValue"/> or <c>null</c> if <paramref name="geometryValue"/> is not a surface.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'PointOnSurface' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression PointOnSurface(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("PointOnSurface", geometryValue);
        }

        #endregion

        #region Spatial Functions - GeographyPolygon

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'ExteriorRing' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the geometry polygon value from which the exterior ring should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the exterior ring of the polygon <paramref name="geometryValue"/> or <c>null</c> if <paramref name="geometryValue"/> is not a polygon.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'ExteriorRing' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression ExteriorRing(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("ExteriorRing", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'InteriorRingCount' function with the
        /// specified argument, which must have an Edm.Geometry result type. The result type of the expression is Edm.Int32.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the geometry polygon value from which the number of interior rings should be retrieved.</param>
        /// <returns>A new DbFunctionExpression that returns either the number of interior rings in the polygon <paramref name="geometryValue"/> or <c>null</c> if <paramref name="geometryValue"/> is not a polygon.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'InteriorRingCount' function accepts an argument with the result type of <paramref name="geometryValue"/>.</exception>
        public static DbFunctionExpression InteriorRingCount(this DbExpression geometryValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            return EdmFunctions.InvokeCanonicalFunction("InteriorRingCount", geometryValue);
        }

        /// <summary>
        /// Creates a <see cref="DbFunctionExpression"/> that invokes the canonical 'InteriorRingAt' function with the
        /// specified arguments.  The first argument must have an Edm.Geometry result type.  The second argument must have an integer numeric result types. 
        /// The result type of the expression is Edm.Geometry.
        /// </summary>
        /// <param name="spatialValue">An expression that specifies the geometry polygon value.</param>
        /// <param name="indexValue">An expression that specifies the position of the interior ring to be retrieved from within the polygon.</param>
        /// <returns>A new DbFunctionExpression that returns either the interior ring at position <paramref name="indexValue"/> in <paramref name="geometryValue"/> or <c>null</c> if <paramref name="geometryValue"/> is not a polygon.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="indexValue"/> is null.</exception>
        /// <exception cref="ArgumentException">No overload of the canonical 'InteriorRingAt' function accepts arguments with the result types of <paramref name="geometryValue"/> and <paramref name="indexValue"/>.</exception>
        public static DbFunctionExpression InteriorRingAt(this DbExpression geometryValue, DbExpression indexValue)
        {
            EntityUtil.CheckArgumentNull(geometryValue, "geometryValue");
            EntityUtil.CheckArgumentNull(indexValue, "indexValue");
            return EdmFunctions.InvokeCanonicalFunction("InteriorRingAt", geometryValue, indexValue);
        }

        #endregion
    }
}
