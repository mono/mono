//---------------------------------------------------------------------
// <copyright file="DbSpatialServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.Utils;

namespace System.Data.Spatial
{
    /// <summary>
    /// A provider-independent service API for geospatial (Geometry/Geography) type support.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    [Serializable]
    public abstract class DbSpatialServices
    {
        private static readonly Singleton<DbSpatialServices> defaultServices = new Singleton<DbSpatialServices>(LoadDefaultServices);

        public static DbSpatialServices Default
        {
            get
            {
                return defaultServices.Value;
            }
        }

        protected DbSpatialServices()
        {
        }

        // For CTP1 use the SQL types whenever they are available.   
        // in future we will have to consider providing a more pluggable 
        // story here so that users can specify what spatial services they want to use by default.
        static DbSpatialServices LoadDefaultServices()
        {
            if (System.Data.SqlClient.SqlProviderServices.SqlTypesAssemblyIsAvailable)
            {
                return System.Data.SqlClient.SqlSpatialServices.Instance;
            }
            else
            {
                return DefaultSpatialServices.Instance;
            }
        }

        #region Geography API

        /// <summary>
        /// This method is intended for use by derived implementations of <see cref="GeographyFromProviderValue"/> after suitable validation of the specified provider value to ensure it is suitable for use with the derived implementation.
        /// </summary>
        /// <param name="spatialServices">The spatial services instance that the returned <see cref="DbGeography"/> value will depend on for its implementation of spatial functionality.</param>
        /// <param name="providerValue"></param>
        /// <returns>A new <see cref="DbGeography"/> instance that contains the specified <paramref name="providerValue"/> and uses the specified <paramref name="spatialServices"/> as its spatial implementation</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialServices"/> or <paramref name="providerValue"/> is null.</exception>
        protected static DbGeography CreateGeography(DbSpatialServices spatialServices, object providerValue)
        {
            spatialServices.CheckNull("spatialServices");
            providerValue.CheckNull("providerValue");
            return new DbGeography(spatialServices, providerValue);
        }

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on a provider-specific value that is compatible with this spatial services implementation.
        /// </summary>
        /// <param name="providerValue">A provider-specific value that this spatial services implementation is capable of interpreting as a geography value.</param>
        /// <returns>A new DbGeography value backed by this spatial services implementation and the specified provider value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="providerValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="providerValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography GeographyFromProviderValue(object providerValue);

        /// <summary>
        /// Creates a provider-specific value compatible with this spatial services implementation based on the specified well known <see cref="DbGeography"/> representation.
        /// </summary>
        /// <param name="wellKnownValue">An instance of <see cref="DbGeographyWellKnownValue"/> that contains the well known representation of a geography value.</param>
        /// <returns>A provider-specific value that encodes the information contained in <paramref name="wellKnownValue"/> in a fashion compatible with this spatial services implementation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownValue"/> is null.</exception>
        public abstract object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue);

        /// <summary>
        /// Creates an instance of <see cref="DbGeographyWellKnownValue"/> that represents the specified <see cref="DbGeography"/> value using one or both of the standard well known spatial formats.
        /// </summary>
        /// <param name="geographyValue"></param>
        /// <returns>The well known representation of <paramref name="geographyValue"/>, as a new <see cref="DbGeographyWellKnownValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue);

        #region Geography Constructors - well known binary

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known binary value. 
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the default DbGeography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        public abstract DbGeography GeographyFromBinary(byte[] wellKnownBinary);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyFromBinary(byte[] wellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> line value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> point value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="pointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> polygon value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> multiline value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiLineWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> multipoint value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPointWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> multipolygon value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPolygonWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> collection value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="geographyCollectionWellKnownBinary">A byte array that contains a well known binary representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyCollectionFromBinary(byte[] geographyCollectionWellKnownBinary, int coordinateSystemId);

        #endregion

        #region Geography Constructors - well known text

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known text value. 
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the default DbGeography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        public abstract DbGeography GeographyFromText(string wellKnownText);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyFromText(string wellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> line value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyLineFromText(string lineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> point value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="pointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyPointFromText(string pointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> polygon value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="polygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyPolygonFromText(string polygonWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> multiline value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiLineWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> multipoint value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPointWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> multipolygon value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPolygonWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeography GeographyMultiPolygonFromText(string multiPolygonWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> collection value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="geographyCollectionWellKnownText">A string that contains a well known text representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyCollectionWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeography GeographyCollectionFromText(string geographyCollectionWellKnownText, int coordinateSystemId);

        #endregion

        #region Geography Constructors - Geography Markup Language (GML)

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <param name="geographyMarkup">A string that contains a Geometry Markup Language (GML) representation of the geography value.</param>
        /// <returns>A new DbGeography value as defined by the GML value with the default DbGeography coordinate system identifier (SRID) (<see cref="DbGeography.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeography GeographyFromGml(string geographyMarkup);

        /// <summary>
        /// Creates a new <see cref="DbGeography"/> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="geographyMarkup">A string that contains a Geometry Markup Language (GML) representation of the geography value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeography value should use.</param>
        /// <returns>A new DbGeography value as defined by the GML value with the specified coordinate system identifier (SRID).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyMarkup"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeography GeographyFromGml(string geographyMarkup, int coordinateSystemId);

        #endregion

        #region Geography Instance Property Accessors

        /// </summary>
        /// Gets the coordinate system identifier (SRID) of the coordinate system used by the given <see cref="DbGeography"/> value.
        /// </summary>
        /// <param name="geographyValue">The geography value from which the coordinate system id should be retrieved.</param>
        /// <returns>The integer coordinate system id value from <paramref name="geographyValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int GetCoordinateSystemId(DbGeography geographyValue);

        /// <summary>
        /// Gets the dimension of the given <see cref="DbGeography"/> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        /// <param name="geographyValue">The geography value for which the dimension value should be retrieved.</param>
        /// <returns>The dimension of <paramref name="geographyValue"/>, or the largest element dimension if <see cref="DbGeography"/> is a collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int GetDimension(DbGeography geographyValue);

        /// </summary>
        /// Gets the spatial type name, as a string, of the given <see cref="DbGeography"/> value.
        /// </summary>
        /// <param name="geographyValue">The geography value from which the spatial type name should be retrieved.</param>
        /// <returns>The string spatial type from <paramref name="geographyValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract string GetSpatialTypeName(DbGeography geographyValue);

        /// </summary>
        /// Gets a Boolean value indicating whether the given <see cref="DbGeography"/> value represents the empty geography.
        /// </summary>
        /// <param name="geographyValue">The geography value from which the IsEmpty property should be retrieved.</param>
        /// <returns><c>true</c> if <paramref name="geographyValue"/> represents the empty geography; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool GetIsEmpty(DbGeography geographyValue);

        #endregion

        #region Geography Well Known Format Conversion

        /// <summary>
        /// Gets the well known text representation of the given <see cref="DbGeography"/> value.  This value should include only the Longitude and Latitude of points.
        /// </summary>
        /// <param name="geographyValue">The geography value for which the well known text should be generated.</param>
        /// <returns>A string containing the well known text representation of <paramref name="geographyValue"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract string AsText(DbGeography geographyValue);

        /// <summary>
        /// Gets the well known text representation of the given <see cref="DbGeography"/> value, including Longitude, Latitude, Elevation (Z) and Measure (M) for points.
        /// </summary>
        /// <param name="geographyValue">The geography value for which the well known text should be generated.</param>
        /// <returns>A string containing the well known text representation of <paramref name="geographyValue"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public virtual string AsTextIncludingElevationAndMeasure(DbGeography geographyValue)
        {
            return null;
        }

        /// <summary>
        /// Gets the well known binary representation of the given <see cref="DbGeography"/> value.
        /// </summary>
        /// <param name="geographyValue">The geography value for which the well known binary should be generated.</param>
        /// <returns>A byte[] containing the well known binary representation of <paramref name="geographyValue"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract byte[] AsBinary(DbGeography geographyValue);

        // Non-OGC
        /// <summary>
        /// Generates the Geography Markup Language (GML) representation of this <see cref="DbGeography"/> value.
        /// </summary>
        /// <param name="geographyValue">The geography value for which the GML should be generated.</param>
        /// <returns>A string containing the GML representation of this DbGeography value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract string AsGml(DbGeography geographyValue);

        #endregion

        #region Geography Instance Methods - Spatial Relation

        /// <summary>
        /// Determines whether the two given <see cref="DbGeography"/> values are spatially equal.
        /// </summary>
        /// <param name="geographyValue">The first geography value to compare for equality.</param>
        /// <param name="otherGeography">The second geography value to compare for equality.</param>
        /// <returns><c>true</c> if <paramref name="geographyValue"/> is spatially equal to <paramref name="otherGeography"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeography"/> values are spatially disjoint.
        /// </summary>
        /// <param name="geographyValue">The first geography value to compare for disjointness.</param>
        /// <param name="otherGeography">The second geography value to compare for disjointness.</param>
        /// <returns><c>true</c> if <paramref name="geographyValue"/> is disjoint from <paramref name="otherGeography"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception>         
        public abstract bool Disjoint(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeography"/> values spatially intersect.
        /// </summary>
        /// <param name="geographyValue">The first geography value to compare for intersection.</param>
        /// <param name="otherGeography">The second geography value to compare for intersection.</param>
        /// <returns><c>true</c> if <paramref name="geographyValue"/> intersects <paramref name="otherGeography"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Intersects(DbGeography geographyValue, DbGeography otherGeography);

        #endregion

        #region Geography Instance Methods - Spatial Analysis

        /// <summary>
        /// Creates a geography value representing all points less than or equal to <paramref name="distance"/> from the given <see cref="DbGeography"/> value.
        /// </summary>
        /// <param name="geographyValue">The geography value.</param>
        /// <param name="distance">A double value specifying how far from <paramref name="geographyValue"/> to buffer.</param>
        /// <returns>A new DbGeography value representing all points less than or equal to <paramref name="distance"/> from <paramref name="geographyValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography Buffer(DbGeography geographyValue, double distance);

        /// <summary>
        /// Computes the distance between the closest points in two <see cref="DbGeography"/> values.
        /// </summary>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <returns>A double value that specifies the distance between the two closest points in <paramref name="geographyValue"/> and <paramref name="otherGeography"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception> 
        public abstract double Distance(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the intersection of two <see cref="DbGeography"/> values.
        /// </summary>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <returns>A new DbGeography value representing the intersection of <paramref name="geographyValue"/> and <paramref name="otherGeography"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the union of two <see cref="DbGeography"/> values.
        /// </summary>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <returns>A new DbGeography value representing the union of <paramref name="geographyValue"/> and <paramref name="otherGeography"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography Union(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the difference of two <see cref="DbGeography"/> values.
        /// </summary>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <returns>A new DbGeography value representing the difference of <paramref name="geographyValue"/> and <paramref name="otherGeography"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography);

        /// <summary>
        /// Computes the symmetric difference of two <see cref="DbGeography"/> values.
        /// </summary>
        /// <param name="geographyValue">The first geography value.</param>
        /// <param name="otherGeography">The second geography value.</param>
        /// <returns>A new DbGeography value representing the symmetric difference of <paramref name="geographyValue"/> and <paramref name="otherGeography"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> or <paramref name="otherGeography"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography);

        #endregion

        #region Geography Collection

        /// <summary>
        /// Returns the number of elements in the given <see cref="DbGeography"/> value, if it represents a geography collection.
        /// <param name="geographyValue">The geography value, which need not represent a geography collection.</param>
        /// <returns>The number of elements in <paramref name="geographyValue"/>, if it represents a collection of other geography values; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int? GetElementCount(DbGeography geographyValue);

        /// <summary>
        /// Returns an element of the given <see cref="DbGeography"/> value, if it represents a geography collection.
        /// <param name="geographyValue">The geography value, which need not represent a geography collection.</param>
        /// <param name="index">The position within the geography value from which the element should be taken.</param>
        /// <returns>The element in <paramref name="geographyValue"/> at position <paramref name="index"/>, if it represents a collection of other geography values; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography ElementAt(DbGeography geographyValue, int index);

        #endregion

        #region Point

        /// <summary>
        /// Returns the Latitude coordinate of the given <see cref="DbGeography"/> value, if it represents a point.
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <returns>The Latitude coordinate of <paramref name="geographyValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetLatitude(DbGeography geographyValue);

        /// <summary>
        /// Returns the Longitude coordinate of the given <see cref="DbGeography"/> value, if it represents a point.
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <returns>The Longitude coordinate of <paramref name="geographyValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetLongitude(DbGeography geographyValue);

        /// <summary>
        /// Returns the elevation (Z coordinate) of the given <see cref="DbGeography"/> value, if it represents a point.
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <returns>The elevation (Z coordinate) of <paramref name="geographyValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetElevation(DbGeography geographyValue);

        /// <summary>
        /// Returns the M (Measure) coordinate of the given <see cref="DbGeography"/> value, if it represents a point.
        /// <param name="geographyValue">The geography value, which need not represent a point.</param>
        /// <returns>The M (Measure) coordinate of <paramref name="geographyValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetMeasure(DbGeography geographyValue);

        #endregion

        #region Curve

        /// <summary>
        /// Returns a nullable double value that indicates the length of the given <see cref="DbGeography"/> value, which may be null if the value does not represent a curve.
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <returns>The length of <paramref name="geographyValue"/>, if it represents a curve; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetLength(DbGeography geographyValue);

        /// <summary>
        /// Returns a <see cref="DbGeography"/> value that represents the start point of the given DbGeography value, which may be null if the value does not represent a curve.
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <returns>The start point of <paramref name="geographyValue"/>, if it represents a curve; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography GetStartPoint(DbGeography geographyValue);

        /// <summary>
        /// Returns a <see cref="DbGeography"/> value that represents the end point of the given DbGeography value, which may be null if the value does not represent a curve.
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <returns>The end point of <paramref name="geographyValue"/>, if it represents a curve; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography GetEndPoint(DbGeography geographyValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="DbGeography"/> value is closed, which may be null if the value does not represent a curve.
        /// <param name="geographyValue">The geography value, which need not represent a curve.</param>
        /// <returns><c>true</c> if <paramref name="geographyValue"/> represents a closed curve; <c>false</c> if <paramref name="geographyValue"/> represents a curve that is not closed; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool? GetIsClosed(DbGeography geographyValue);

        #endregion

        #region LineString, Line, LinearRing

        /// <summary>
        /// Returns the number of points in the given <see cref="DbGeography"/> value, if it represents a linestring or linear ring.
        /// <param name="geographyValue">The geography value, which need not represent a linestring or linear ring.</param>
        /// <returns>The number of elements in <paramref name="geographyValue"/>, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int? GetPointCount(DbGeography geographyValue);

        /// <summary>
        /// Returns a point element of the given <see cref="DbGeography"/> value, if it represents a linestring or linear ring.
        /// <param name="geographyValue">The geography value, which need not represent a linestring or linear ring.</param>
        /// <param name="index">The position within the geography value from which the element should be taken.</param>
        /// <returns>The point in <paramref name="geographyValue"/> at position <paramref name="index"/>, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeography PointAt(DbGeography geographyValue, int index);

        #endregion

        #region Surface

        /// <summary>
        /// Returns a nullable double value that indicates the area of the given <see cref="DbGeography"/> value, which may be null if the value does not represent a surface.
        /// <param name="geographyValue">The geography value, which need not represent a surface.</param>
        /// <returns>The area of <paramref name="geographyValue"/>, if it represents a surface; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geographyValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geographyValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetArea(DbGeography geographyValue);

        #endregion

        #endregion

        #region Geometry API

        /// <summary>
        /// This method is intended for use by derived implementations of <see cref="GeometryFromProviderValue"/> after suitable validation of the specified provider value to ensure it is suitable for use with the derived implementation.
        /// </summary>
        /// <param name="spatialServices">The spatial services instance that the returned <see cref="DbGeometry"/> value will depend on for its implementation of spatial functionality.</param>
        /// <param name="providerValue"></param>
        /// <returns>A new <see cref="DbGeometry"/> instance that contains the specified <paramref name="providerValue"/> and uses the specified <paramref name="spatialServices"/> as its spatial implementation</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spatialServices"/> or <paramref name="providerValue"/> is null.</exception>
        protected static DbGeometry CreateGeometry(DbSpatialServices spatialServices, object providerValue)
        {
            spatialServices.CheckNull("spatialServices");
            providerValue.CheckNull("providerValue");
            return new DbGeometry(spatialServices, providerValue);
        }

        /// <summary>
        /// Creates a provider-specific value compatible with this spatial services implementation based on the specified well known <see cref="DbGeometry"/> representation.
        /// </summary>
        /// <param name="wellKnownValue">An instance of <see cref="DbGeometryWellKnownValue"/> that contains the well known representation of a geometry value.</param>
        /// <returns>A provider-specific value that encodes the information contained in <paramref name="wellKnownValue"/> in a fashion compatible with this spatial services implementation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownValue"/> is null.</exception>
        public abstract object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue);

        /// <summary>
        /// Creates an instance of <see cref="DbGeometryWellKnownValue"/> that represents the specified <see cref="DbGeometry"/> value using one or both of the standard well known spatial formats.
        /// </summary>
        /// <param name="geometryValue"></param>
        /// <returns>The well known representation of <paramref name="geometryValue"/>, as a new <see cref="DbGeometryWellKnownValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on a provider-specific value that is compatible with this spatial services implementation.
        /// </summary>
        /// <param name="providerValue">A provider-specific value that this spatial services implementation is capable of interpreting as a geometry value.</param>
        /// <returns>A new DbGeometry value backed by this spatial services implementation and the specified provider value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="providerValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="providerValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GeometryFromProviderValue(object providerValue);

        #region Geometry Constructors - well known binary

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on the specified well known binary value. 
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the default DbGeometry coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        public abstract DbGeometry GeometryFromBinary(byte[] wellKnownBinary);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="wellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryFromBinary(byte[] wellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> line value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryLineFromBinary(byte[] lineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> point value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="pointWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryPointFromBinary(byte[] pointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> polygon value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="polygonWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryPolygonFromBinary(byte[] polygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> multiline value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiLineWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiLineFromBinary(byte[] multiLineWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> multipoint value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPointWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiPointFromBinary(byte[] multiPointWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> multipolygon value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPolygonWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiPolygonFromBinary(byte[] multiPolygonWellKnownBinary, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> collection value based on the specified well known binary value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="geometryCollectionWellKnownBinary">A byte array that contains a well known binary representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known binary value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryCollectionWellKnownBinary"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryCollectionFromBinary(byte[] geometryCollectionWellKnownBinary, int coordinateSystemId);

        #endregion

        #region Geometry Constructors - well known text 

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on the specified well known text value. 
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the default DbGeometry coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        public abstract DbGeometry GeometryFromText(string wellKnownText);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="wellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="wellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryFromText(string wellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> line value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="lineWellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="lineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryLineFromText(string lineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> point value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="pointWellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="pointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryPointFromText(string pointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> polygon value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="polygonWellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="polygonWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryPolygonFromText(string polygonWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> multiline value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiLineWellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiLineWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiLine", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiLineFromText(string multiLineWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> multipoint value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPointWellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPointWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "MultiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "multiPoint", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiPointFromText(string multiPointWellKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> multipolygon value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="multiPolygonKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="multiPolygonKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multi", Justification = "Match OGC, EDM")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "multi", Justification = "Match OGC, EDM")]
        public abstract DbGeometry GeometryMultiPolygonFromText(string multiPolygonKnownText, int coordinateSystemId);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> collection value based on the specified well known text value and coordinate system identifier (SRID). 
        /// </summary>
        /// <param name="geometryCollectionWellKnownText">A string that contains a well known text representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the well known text value with the specified coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryCollectionWellKnownText"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        public abstract DbGeometry GeometryCollectionFromText(string geometryCollectionWellKnownText, int coordinateSystemId);

        #endregion

        #region Geometry Constructors - Geography Markup Language (GML)

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on the specified Geography Markup Language (GML) value.
        /// </summary>
        /// <param name="geometryMarkup">A string that contains a Geography Markup Language (GML) representation of the geometry value.</param>
        /// <returns>A new DbGeometry value as defined by the GML value with the default DbGeometry coordinate system identifier (SRID) (<see cref="DbGeometry.DefaultCoordinateSystemId"/>).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryMarkup"/> is null.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeometry GeometryFromGml(string geometryMarkup);

        /// <summary>
        /// Creates a new <see cref="DbGeometry"/> value based on the specified Geography Markup Language (GML) value and coordinate system identifier (SRID).
        /// </summary>
        /// <param name="geometryMarkup">A string that contains a Geography Markup Language (GML) representation of the geometry value.</param>
        /// <param name="coordinateSystemId">The identifier of the coordinate system that the new DbGeometry value should use.</param>
        /// <returns>A new DbGeometry value as defined by the GML value with the specified coordinate system identifier (SRID).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryMarkup"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="coordinateSystemId"/> is not valid.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract DbGeometry GeometryFromGml(string geometryMarkup, int coordinateSystemId);

        #endregion

        #region Geometry Instance Property Accessors

        /// </summary>
        /// Gets the coordinate system id (SRID) of the coordinate system used by the given <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value from which the coordinate system id should be retrieved.</param>
        /// <returns>The integer coordinate system id value from <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int GetCoordinateSystemId(DbGeometry geometryValue);

        /// </summary>
        /// Gets the boundary of the given <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value from which the Boundary value should be retrieved.</param>
        /// <returns>The boundary of <paramref name="geometryValue"/>, as a <see cref="DbGeometry"/> value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetBoundary(DbGeometry geometryValue);

        /// <summary>
        /// Gets the dimension of the given <see cref="DbGeometry"/> value or, if the value is a collections, the largest element dimension.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the dimension value should be retrieved.</param>
        /// <returns>The dimension of <paramref name="geometryValue"/>, or the largest element dimension if <see cref="DbGeometry"/> is a collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int GetDimension(DbGeometry geometryValue);

        /// <summary>
        /// Gets the envelope (minimum bounding box) of the given <see cref="DbGeometry"/> value, as a geometry value.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the envelope value should be retrieved.</param>
        /// <returns>The envelope of <paramref name="geometryValue"/>, as a <see cref="DbGeometry"/> value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetEnvelope(DbGeometry geometryValue);

        /// </summary>
        /// Gets the spatial type name, as a string, of the given <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value from which the spatial type name should be retrieved.</param>
        /// <returns>The string spatial type name from <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract string GetSpatialTypeName(DbGeometry geometryValue);

        /// </summary>
        /// Gets a Boolean value indicating whether the given <see cref="DbGeometry"/> value represents the empty geometry.
        /// </summary>
        /// <param name="geometryValue">The geometry value from which the IsEmpty property should be retrieved.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> represents the empty geometry; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool GetIsEmpty(DbGeometry geometryValue);

        /// </summary>
        /// Gets a Boolean value indicating whether the given <see cref="DbGeometry"/> value is simple, according to the conditions required for its geometry type.
        /// </summary>
        /// <param name="geometryValue">The geometry value from which the IsSimple property should be retrieved.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> satisifies the conditions required for an instance of its geometry type to be considered simple; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool GetIsSimple(DbGeometry geometryValue);

        /// </summary>
        /// Gets a Boolean value indicating whether the given <see cref="DbGeometry"/> value is valid.
        /// </summary>
        /// <param name="geometryValue">The geometry value from which the IsValid property should be retrieved.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> is considered valid; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool GetIsValid(DbGeometry geometryValue);

        #endregion

        #region Geometry Well Known Format Conversion

        /// <summary>
        /// Gets the well known text representation of the given <see cref="DbGeometry"/> value, including only X and Y coordinates for points.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the well known text should be generated.</param>
        /// <returns>A string containing the well known text representation of <paramref name="geometryValue"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract string AsText(DbGeometry geometryValue);

        /// <summary>
        /// Gets the well known text representation of the given <see cref="DbGeometry"/> value, including X coordinate, Y coordinate, Elevation (Z) and Measure (M) for points.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the well known text should be generated.</param>
        /// <returns>A string containing the well known text representation of <paramref name="geometryValue"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public virtual string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
        {
            return null;
        }

        /// <summary>
        /// Gets the well known binary representation of the given <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the well known binary should be generated.</param>
        /// <returns>A byte[] containing the well known binary representation of <paramref name="geometryValue"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract byte[] AsBinary(DbGeometry geometryValue);

        // Non-OGC
        /// <summary>
        /// Generates the Geography Markup Language (GML) representation of this <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the GML should be generated.</param>
        /// <returns>A string containing the GML representation of this DbGeometry value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception> 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gml")]
        public abstract string AsGml(DbGeometry geometryValue);

        #endregion

        #region Geometry Instance Methods - Spatial Relation

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values are spatially equal.
        /// </summary>
        /// <param name="geometryValue">The first geometry value to compare for equality.</param>
        /// <param name="otherGeometry">The second geometry value to compare for equality.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> is spatially equal to <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values are spatially disjoint.
        /// </summary>
        /// <param name="geometryValue">The first geometry value to compare for disjointness.</param>
        /// <param name="otherGeometry">The second geometry value to compare for disjointness.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> is disjoint from <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>         
        public abstract bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values spatially intersect.
        /// </summary>
        /// <param name="geometryValue">The first geometry value to compare for intersection.</param>
        /// <param name="otherGeometry">The second geometry value to compare for intersection.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> intersects <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values spatially touch.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> touches <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values spatially cross.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> crosses <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether one <see cref="DbGeometry"/> value is spatially within the other.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> is within <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool Within(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether one <see cref="DbGeometry"/> value spatially contains the other.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> contains <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values spatially overlap.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> overlaps <paramref name="otherGeometry"/>; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Determines whether the two given <see cref="DbGeometry"/> values are spatially related according to the
        /// given Dimensionally Extended Nine-Intersection Model (DE-9IM) intersection pattern.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The geometry value that should be compared with the first geometry value for relation.</param>
        /// <param name="matrix">A string that contains the text representation of the (DE-9IM) intersection pattern that defines the relation.</param>
        /// <returns><c>true</c> if this <paramref name="geometryValue"/> value relates to <paramref name="otherGeometry"/> according to the specified intersection pattern matrix; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/>, <paramref name="otherGeometry"/> or <paramref name="matrix"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix);

        #endregion

        #region Geometry Instance Methods - Spatial Analysis

        /// <summary>
        /// Creates a geometry value representing all points less than or equal to <paramref name="distance"/> from the given <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value.</param>
        /// <param name="distance">A double value specifying how far from <paramref name="geometryValue"/> to buffer.</param>
        /// <returns>A new DbGeometry value representing all points less than or equal to <paramref name="distance"/> from <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry Buffer(DbGeometry geometryValue, double distance);

        /// <summary>
        /// Computes the distance between the closest points in two <see cref="DbGeometry"/> values.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns>A double value that specifies the distance between the two closest points in <paramref name="geometryValue"/> and <paramref name="otherGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception> 
        public abstract double Distance(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// </summary>
        /// Gets the convex hull of the given <see cref="DbGeometry"/> value.
        /// </summary>
        /// <param name="geometryValue">The geometry value for which the convex hull should be computed.</param>
        /// <returns>A new DbGeometry value that contains the convex hull of <paramref name="geometryValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetConvexHull(DbGeometry geometryValue);

        /// <summary>
        /// Computes the intersection of two <see cref="DbGeometry"/> values.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns>A new DbGeometry value representing the intersection of <paramref name="geometryValue"/> and <paramref name="otherGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Computes the union of two <see cref="DbGeometry"/> values.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns>A new DbGeometry value representing the union of <paramref name="geometryValue"/> and <paramref name="otherGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Computes the difference between two <see cref="DbGeometry"/> values.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns>A new DbGeometry value representing the difference between <paramref name="geometryValue"/> and <paramref name="otherGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry);

        /// <summary>
        /// Computes the symmetric difference between two <see cref="DbGeometry"/> values.
        /// </summary>
        /// <param name="geometryValue">The first geometry value.</param>
        /// <param name="otherGeometry">The second geometry value.</param>
        /// <returns>A new DbGeometry value representing the symmetric difference between <paramref name="geometryValue"/> and <paramref name="otherGeometry"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> or <paramref name="otherGeometry"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry);

        #endregion

        #region Geometry Collection

        /// <summary>
        /// Returns the number of elements in the given <see cref="DbGeometry"/> value, if it represents a geometry collection.
        /// <param name="geometryValue">The geometry value, which need not represent a geometry collection.</param>
        /// <returns>The number of elements in <paramref name="geometryValue"/>, if it represents a collection of other geometry values; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int? GetElementCount(DbGeometry geometryValue);

        /// <summary>
        /// Returns an element of the given <see cref="DbGeometry"/> value, if it represents a geometry collection.
        /// <param name="geometryValue">The geometry value, which need not represent a geometry collection.</param>
        /// <param name="index">The position within the geometry value from which the element should be taken.</param>
        /// <returns>The element in <paramref name="geometryValue"/> at position <paramref name="index"/>, if it represents a collection of other geometry values; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry ElementAt(DbGeometry geometryValue, int index);

        #endregion

        #region Point

        /// <summary>
        /// Returns the X coordinate of the given <see cref="DbGeometry"/> value, if it represents a point.
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <returns>The X coordinate of <paramref name="geometryValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetXCoordinate(DbGeometry geometryValue);

        /// <summary>
        /// Returns the Y coordinate of the given <see cref="DbGeometry"/> value, if it represents a point.
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <returns>The Y coordinate of <paramref name="geometryValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetYCoordinate(DbGeometry geometryValue);

        /// <summary>
        /// Returns the elevation (Z) of the given <see cref="DbGeometry"/> value, if it represents a point.
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <returns>The elevation (Z) of <paramref name="geometryValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
         public abstract double? GetElevation(DbGeometry geometryValue);

        /// <summary>
        /// Returns the M (Measure) coordinate of the given <see cref="DbGeometry"/> value, if it represents a point.
        /// <param name="geometryValue">The geometry value, which need not represent a point.</param>
        /// <returns>The M (Measure) coordinate of <paramref name="geometryValue"/>, if it represents a point; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetMeasure(DbGeometry geometryValue);

        #endregion

        #region Curve

        /// <summary>
        /// Returns a nullable double value that indicates the length of the given <see cref="DbGeometry"/> value, which may be null if the value does not represent a curve.
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <returns>The length of <paramref name="geometryValue"/>, if it represents a curve; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetLength(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="DbGeometry"/> value that represents the start point of the given DbGeometry value, which may be null if the value does not represent a curve.
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <returns>The start point of <paramref name="geometryValue"/>, if it represents a curve; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetStartPoint(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="DbGeometry"/> value that represents the end point of the given DbGeometry value, which may be null if the value does not represent a curve.
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <returns>The end point of <paramref name="geometryValue"/>, if it represents a curve; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetEndPoint(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="DbGeometry"/> value is closed, which may be null if the value does not represent a curve.
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> represents a closed curve; <c>false</c> if <paramref name="geometryValue"/> represents a curve that is not closed; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool? GetIsClosed(DbGeometry geometryValue);

        /// <summary>
        /// Returns a nullable Boolean value that whether the given <see cref="DbGeometry"/> value is a ring, which may be null if the value does not represent a curve.
        /// <param name="geometryValue">The geometry value, which need not represent a curve.</param>
        /// <returns><c>true</c> if <paramref name="geometryValue"/> represents a ring; <c>false</c> if <paramref name="geometryValue"/> represents a curve that is not a ring; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract bool? GetIsRing(DbGeometry geometryValue);

        #endregion

        #region LineString, Line, LinearRing

        /// <summary>
        /// Returns the number of points in the given <see cref="DbGeometry"/> value, if it represents a linestring or linear ring.
        /// <param name="geometryValue">The geometry value, which need not represent a linestring or linear ring.</param>
        /// <returns>The number of elements in <paramref name="geometryValue"/>, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int? GetPointCount(DbGeometry geometryValue);

        /// <summary>
        /// Returns a point element of the given <see cref="DbGeometry"/> value, if it represents a linestring or linear ring.
        /// <param name="geometryValue">The geometry value, which need not represent a linestring or linear ring.</param>
        /// <param name="index">The position within the geometry value from which the element should be taken.</param>
        /// <returns>The point in <paramref name="geometryValue"/> at position <paramref name="index"/>, if it represents a linestring or linear ring; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry PointAt(DbGeometry geometryValue, int index);

        #endregion

        #region Surface

        /// <summary>
        /// Returns a nullable double value that indicates the area of the given <see cref="DbGeometry"/> value, which may be null if the value does not represent a surface.
        /// <param name="geometryValue">The geometry value, which need not represent a surface.</param>
        /// <returns>The area of <paramref name="geometryValue"/>, if it represents a surface; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract double? GetArea(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="DbGeometry"/> value that represents the centroid of the given DbGeometry value, which may be null if the value does not represent a surface.
        /// <param name="geometryValue">The geometry value, which need not represent a surface.</param>
        /// <returns>The centroid of <paramref name="geometryValue"/>, if it represents a surface; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Centroid", Justification = "Naming convention prescribed by OGC specification")]
        public abstract DbGeometry GetCentroid(DbGeometry geometryValue);

        /// <summary>
        /// Returns a <see cref="DbGeometry"/> value that represents a point on the surface of the given DbGeometry value, which may be null if the value does not represent a surface.
        /// <param name="geometryValue">The geometry value, which need not represent a surface.</param>
        /// <returns>A DbGeometry value representing a point on <paramref name="geometryValue"/>, if it represents a surface; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetPointOnSurface(DbGeometry geometryValue);

        #endregion

        #region Polygon

        /// <summary>
        /// Returns a <see cref="DbGeometry"/> value that represents the exterior ring of the given DbGeometry value, which may be null if the value does not represent a polygon.
        /// <param name="geometryValue">The geometry value, which need not represent a polygon.</param>
        /// <returns>A DbGeometry value representing the exterior ring on <paramref name="geometryValue"/>, if it represents a polygon; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry GetExteriorRing(DbGeometry geometryValue);

        /// <summary>
        /// Returns the number of interior rings in the given <see cref="DbGeometry"/> value, if it represents a polygon.
        /// <param name="geometryValue">The geometry value, which need not represent a polygon.</param>
        /// <returns>The number of elements in <paramref name="geometryValue"/>, if it represents a polygon; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract int? GetInteriorRingCount(DbGeometry geometryValue);

        /// <summary>
        /// Returns an interior ring from the the given <see cref="DbGeometry"/> value, if it represents a polygon.
        /// <param name="geometryValue">The geometry value, which need not represent a polygon.</param>
        /// <param name="index">The position within the geometry value from which the element should be taken.</param>
        /// <returns>The interior ring in <paramref name="geometryValue"/> at position <paramref name="index"/>, if it represents a polygon; otherwise <c>null</c>.</returns>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="geometryValue"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="geometryValue"/> is not compatible with this spatial services implementation.</exception>
        public abstract DbGeometry InteriorRingAt(DbGeometry geometryValue, int index);

        #endregion

        #endregion
    }
}
