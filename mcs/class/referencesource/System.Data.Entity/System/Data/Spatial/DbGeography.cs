//------------------------------------------------------------------------------
// <copyright file="DbGeography.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner [....]
//------------------------------------------------------------------------------

using System.Data.Common.Internal;
using System.ComponentModel.DataAnnotations;
using System.Data.Spatial.Internal;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.Spatial
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    [DataContract]
    [Serializable]
    [BindableType]
    public class DbGeography
    {
        private DbSpatialServices spatialSvcs;
        private object providerValue;

        internal DbGeography(DbSpatialServices spatialServices, object spatialProviderValue)
        {
            Debug.Assert(spatialServices != null, "Spatial services are required");
            Debug.Assert(spatialProviderValue != null, "Provider value is required");

            this.spatialSvcs = spatialServices;
            this.providerValue = spatialProviderValue;
        }

        /// <summary>
        /// Gets the default coordinate system id (SRID) for geography values (WGS 84)
        /// </summary>
        public static int DefaultCoordinateSystemId { get { return 4326; /* WGS 84 */ } }

        /// <summary>
        /// Gets a representation of this DbGeography value that is specific to the underlying provider that constructed it.
        /// </summary>
        public object ProviderValue { get { return this.providerValue; } }

        /// <summary>
        /// Gets or sets a data contract serializable well known representation of this DbGeography value.
        /// </summary>
        [DataMember(Name = "Geography")]
        public DbGeographyWellKnownValue WellKnownValue
        {
            get { return this.spatialSvcs.CreateWellKnownValue(this); }
            set
            {
                if (this.spatialSvcs != null)
                {
                    throw SpatialExceptions.WellKnownValueSerializationPropertyNotDirectlySettable();
                }

                DbSpatialServices resolvedServices = DbSpatialServices.Default;
                this.providerValue = resolvedServices.CreateProviderValue(value);
                this.spatialSvcs = resolvedServices;
            }
        }

        #region Well Known Binary Static Constructors

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known binary value. 
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the default geography coordinate system identifier (SRID)(<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        public static DbGeography FromBinary(byte[] wellKnownBinary)
        {
            wellKnownBinary.CheckNull("wellKnownBinary");
            return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography FromBinary(byte[] wellKnownBinary, int coordinateSystemId)
        {
            wellKnownBinary.CheckNull("wellKnownBinary");
            return DbSpatialServices.Default.GeographyFromBinary(wellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> line value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography LineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId)
        {
            lineWellKnownBinary.CheckNull("lineWellKnownBinary");
            return DbSpatialServices.Default.GeographyLineFromBinary(lineWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> point value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="pointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId)
        {
            pointWellKnownBinary.CheckNull("pointWellKnownBinary");
            return DbSpatialServices.Default.GeographyPointFromBinary(pointWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> polygon value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId)
        {
            polygonWellKnownBinary.CheckNull("polygonWellKnownBinary");
            return DbSpatialServices.Default.GeographyPolygonFromBinary(polygonWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiLine value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="multiLineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbGeography MultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId)
        {
            multiLineWellKnownBinary.CheckNull("multiLineWellKnownBinary");
            return DbSpatialServices.Default.GeographyMultiLineFromBinary(multiLineWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPoint value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbGeography MultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId)
        {
            multiPointWellKnownBinary.CheckNull("multiPointWellKnownBinary");
            return DbSpatialServices.Default.GeographyMultiPointFromBinary(multiPointWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPolygon value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="multiPolygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbGeography MultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId)
        {
            multiPolygonWellKnownBinary.CheckNull("multiPolygonWellKnownBinary");
            return DbSpatialServices.Default.GeographyMultiPolygonFromBinary(multiPolygonWellKnownBinary, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> collection value based on the specified well known binary value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="geographyCollectionWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId)
        {
            geographyCollectionWellKnownBinary.CheckNull("geographyCollectionWellKnownBinary");
            return DbSpatialServices.Default.GeographyCollectionFromBinary(geographyCollectionWellKnownBinary, coordinateSystemId);
        }

        #endregion

        #region GML Static Constructors

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <param name="geographyMarkup">A string that contains a Geography Markup Language (GML) representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the GML value with the default geography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbGeography FromGml(string geographyMarkup)
        {
            geographyMarkup.CheckNull("geographyMarkup");
            return DbSpatialServices.Default.GeographyFromGml(geographyMarkup);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="geographyMarkup">A string that contains a Geography Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the GML value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public static DbGeography FromGml(string geographyMarkup, int coordinateSystemId)
        {
            geographyMarkup.CheckNull("geographyMarkup");
            return DbSpatialServices.Default.GeographyFromGml(geographyMarkup, coordinateSystemId);
        }

        #endregion

        #region Well Known Text Static Constructors

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known text value. 
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the default geography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        public static DbGeography FromText(string wellKnownText)
        {
            wellKnownText.CheckNull("wellKnownText");
            return DbSpatialServices.Default.GeographyFromText(wellKnownText);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography FromText(string wellKnownText, int coordinateSystemId)
        {
            wellKnownText.CheckNull("wellKnownText");
            return DbSpatialServices.Default.GeographyFromText(wellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> line value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography LineFromText(string lineWellKnownText, int coordinateSystemId)
        {
            lineWellKnownText.CheckNull("lineWellKnownText");
            return DbSpatialServices.Default.GeographyLineFromText(lineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> point value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="pointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PointFromText(string pointWellKnownText, int coordinateSystemId)
        {
            pointWellKnownText.CheckNull("pointWellKnownText");
            return DbSpatialServices.Default.GeographyPointFromText(pointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> polygon value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="polygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography PolygonFromText(string polygonWellKnownText, int coordinateSystemId)
        {
            polygonWellKnownText.CheckNull("polygonWellKnownText");
            return DbSpatialServices.Default.GeographyPolygonFromText(polygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiLine value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiLineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbGeography MultiLineFromText(string multiLineWellKnownText, int coordinateSystemId)
        {
            multiLineWellKnownText.CheckNull("multiLineWellKnownText");
            return DbSpatialServices.Default.GeographyMultiLineFromText(multiLineWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPoint value based on the specified well known text value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="multiPointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbGeography MultiPointFromText(string multiPointWellKnownText, int coordinateSystemId)
        {
            multiPointWellKnownText.CheckNull("multiPointWellKnownText");
            return DbSpatialServices.Default.GeographyMultiPointFromText(multiPointWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> MultiPolygon value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPolygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public static DbGeography MultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId)
        {
            multiPolygonWellKnownText.CheckNull("multiPolygonWellKnownText");
            return DbSpatialServices.Default.GeographyMultiPolygonFromText(multiPolygonWellKnownText, coordinateSystemId);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> collection value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="geographyCollectionWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public static DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId)
        {
            geographyCollectionWellKnownText.CheckNull("geographyCollectionWellKnownText");
            return DbSpatialServices.Default.GeographyCollectionFromText(geographyCollectionWellKnownText, coordinateSystemId);
        }

        #endregion

        #region Geography Instance Properties

        /// </summary>
        /// Gets the Spatial Reference System Identifier (Coordinate System Id) of the spatial reference system used by this DbGeography value.
        /// </summary>
        public int CoordinateSystemId { get { return this.spatialSvcs.GetCoordinateSystemId(this); } }

        /// <summary>
        /// Gets the dimension of the given <see cref="DbGeography"/> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        public int Dimension { get { return this.spatialSvcs.GetDimension(this); } }

        /// </summary>
        /// Gets the spatial type name, as a string, of this DbGeography value.
        /// </summary>
        public string SpatialTypeName { get { return this.spatialSvcs.GetSpatialTypeName(this); } }

        /// </summary>
        /// Gets a Boolean value indicating whether this DbGeography value represents the empty geography.
        /// </summary>
        public bool   IsEmpty { get { return this.spatialSvcs.GetIsEmpty(this); } }
                
        #endregion

        #region Geography Well Known Format Conversion

        /// <summary>
        /// Generates the well known text representation of this DbGeography value.  Includes only Longitude and Latitude for points.
        /// </summary>
        /// <returns>A string containing the well known text representation of this DbGeography value.</returns>
        public string AsText() { return this.spatialSvcs.AsText(this); }

        /// <summary>
        /// Generates the well known text representation of this DbGeography value.  Includes Longitude, Latitude, Elevation (Z) and Measure (M) for points.
        /// </summary>
        /// <returns>A string containing the well known text representation of this DbGeography value.</returns>
        internal string AsTextIncludingElevationAndMeasure() { return this.spatialSvcs.AsTextIncludingElevationAndMeasure(this); }

        /// <summary>
        /// Generates the well known binary representation of this DbGeography value.
        /// </summary>
        /// <returns>A byte array containing the well known binary representation of this DbGeography value.</returns>
        public byte[] AsBinary() { return this.spatialSvcs.AsBinary(this); }
        
        // Non-OGC
        /// <summary>
        /// Generates the Geography Markup Language (GML) representation of this DbGeography value.
        /// </summary>
        /// <returns>A string containing the GML representation of this DbGeography value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public string AsGml() { return this.spatialSvcs.AsGml(this); }

        #endregion

        #region Geography Operations - Spatial Relation

        /// <summary>
        /// Determines whether this DbGeography is spatially equal to the specified DbGeography argument.
        /// </summary>
        /// <param name="other">The geography value that should be compared with this geography value for equality.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is spatially equal to this geography value; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool SpatialEquals(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.SpatialEquals(this, other);
        }

        /// <summary>
        /// Determines whether this DbGeography is spatially disjoint from the specified DbGeography argument.
        /// </summary>
        /// <param name="other">The geography value that should be compared with this geography value for disjointness.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is disjoint from this geography value; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool Disjoint(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.Disjoint(this, other);
        }

        /// <summary>
        /// Determines whether this DbGeography value spatially intersects the specified DbGeography argument.
        /// </summary>
        /// <param name="other">The geography value that should be compared with this geography value for intersection.</param>
        /// <returns><c>true</c> if <paramref name="other"/> intersects this geography value; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool Intersects(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.Intersects(this, other);
        }

        #endregion

        #region Geography Operations - Spatial Analysis

        /// <summary>
        /// Creates a geography value representing all points less than or equal to <paramref name="distance"/> from this DbGeography value.
        /// </summary>
        /// <param name="distance">A double value specifying how far from this geography value to buffer.</param>
        /// <returns>A new DbGeography value representing all points less than or equal to <paramref name="distance"/> from this geography value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="distance"/> is null.</exception>
        public DbGeography Buffer(double? distance)
        {
            if (!distance.HasValue)
            {
                throw EntityUtil.ArgumentNull("distance");
            } 
            return this.spatialSvcs.Buffer(this, distance.Value);
        }

        /// <summary>
        /// Computes the distance between the closest points in this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the distance from this value should be computed.</param>
        /// <returns>A double value that specifies the distance between the two closest points in this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public double? Distance(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.Distance(this, other);
        }

        /// <summary>
        /// Computes the intersection of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the intersection with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the intersection between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography Intersection(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.Intersection(this, other);
        }

        /// <summary>
        /// Computes the union of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the union with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the union between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography Union(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.Union(this, other);
        }

        /// <summary>
        /// Computes the difference of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the difference with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the difference between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography Difference(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.Difference(this, other);
        }

        /// <summary>
        /// Computes the symmetric difference of this DbGeography value and another DbGeography value.
        /// </summary>
        /// <param name="other">The geography value for which the symmetric difference with this value should be computed.</param>
        /// <returns>A new DbGeography value representing the symmetric difference between this geography value and <paramref name="other"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public DbGeography SymmetricDifference(DbGeography other)
        {
            other.CheckNull("other");
            return this.spatialSvcs.SymmetricDifference(this, other);
        }

        #endregion

        #region Geography Collection

        /// <summary>
        /// Gets the number of elements in this DbGeography value, if it represents a geography collection.
        /// <returns>The number of elements in this geography value, if it represents a collection of other geography values; otherwise <c>null</c>.</returns>
        /// </summary>
        public int? ElementCount { get { return this.spatialSvcs.GetElementCount(this); } }

        /// <summary>
        /// Returns an element of this DbGeography value from a specific position, if it represents a geography collection.
        /// <param name="index">The position within this geography value from which the element should be taken.</param>
        /// <returns>The element in this geography value at the specified position, if it represents a collection of other geography values; otherwise <c>null</c>.</returns>
        /// </summary>
        public DbGeography ElementAt(int index)
        {
            return this.spatialSvcs.ElementAt(this, index);
        }

        #endregion

        #region Point

        /// <summary>
        /// Gets the Latitude coordinate of this DbGeography value, if it represents a point.
        /// <returns>The Latitude coordinate value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        public double? Latitude { get { return this.spatialSvcs.GetLatitude(this); } }

        /// <summary>
        /// Gets the Longitude coordinate of this DbGeography value, if it represents a point.
        /// <returns>The Longitude coordinate value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        public double? Longitude { get { return this.spatialSvcs.GetLongitude(this); } }

        /// <summary>
        /// Gets the elevation (Z coordinate) of this DbGeography value, if it represents a point.
        /// <returns>The elevation (Z coordinate) value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
         public double? Elevation { get { return this.spatialSvcs.GetElevation(this); } }

        /// <summary>
        /// Gets the M (Measure) coordinate of this DbGeography value, if it represents a point.
        /// <returns>The M (Measure) coordinate value of this geography value, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        public double? Measure { get { return this.spatialSvcs.GetMeasure(this); } }

        #endregion

        #region Curve

        /// <summary>
        /// Gets a nullable double value that indicates the length of this DbGeography value, which may be null if this value does not represent a curve.
        /// </summary>
        public double? Length { get { return this.spatialSvcs.GetLength(this); } }

        /// <summary>
        /// Gets a DbGeography value representing the start point of this value, which may be null if this DbGeography value does not represent a curve.
        /// </summary>
        public DbGeography StartPoint { get { return this.spatialSvcs.GetStartPoint(this); } }

        /// <summary>
        /// Gets a DbGeography value representing the start point of this value, which may be null if this DbGeography value does not represent a curve.
        /// </summary>
        public DbGeography EndPoint { get { return this.spatialSvcs.GetEndPoint(this); } }

        /// <summary>
        /// Gets a nullable Boolean value indicating whether this DbGeography value is closed, which may be null if this value does not represent a curve.
        /// </summary>
        public bool? IsClosed { get { return this.spatialSvcs.GetIsClosed(this); } }

        #endregion

        #region LineString, Line, LinearRing

        /// <summary>
        /// Gets the number of points in this DbGeography value, if it represents a linestring or linear ring.
        /// <returns>The number of elements in this geography value, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        public int? PointCount { get { return this.spatialSvcs.GetPointCount(this); } }

        /// <summary>
        /// Returns an element of this DbGeography value from a specific position, if it represents a linestring or linear ring.
        /// <param name="index">The position within this geography value from which the element should be taken.</param>
        /// <returns>The element in this geography value at the specified position, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        public DbGeography PointAt(int index)
        {
            return this.spatialSvcs.PointAt(this, index);
        }

        #endregion

        #region Surface

        /// <summary>
        /// Gets a nullable double value that indicates the area of this DbGeography value, which may be null if this value does not represent a surface.
        /// </summary>
        public double? Area { get { return this.spatialSvcs.GetArea(this); } }

        #endregion

        #region ToString
        /// <summary>
        /// Returns a string representation of the geography value.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SRID={1};{0}", this.WellKnownValue.WellKnownText ?? base.ToString(), this.CoordinateSystemId);
        }
        #endregion
    }
}
