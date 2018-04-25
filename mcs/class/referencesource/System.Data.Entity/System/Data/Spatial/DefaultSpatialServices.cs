//------------------------------------------------------------------------------
// <copyright file="DefaultSpatialServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.Common.Internal;
using System.Diagnostics;
using System.Data.Spatial.Internal;

namespace System.Data.Spatial
{
    [Serializable]
    internal sealed class DefaultSpatialServices : DbSpatialServices
    {
        #region Provider Value Type

        [Serializable]
        private sealed class ReadOnlySpatialValues
        {
            private readonly int srid;
            private readonly byte[] wkb;
            private readonly string wkt;
            private readonly string gml;

            internal ReadOnlySpatialValues(int spatialRefSysId, string textValue, byte[] binaryValue, string gmlValue)
            {
                this.srid = spatialRefSysId;
                this.wkb = (binaryValue == null ? null : (byte[])binaryValue.Clone());
                this.wkt = textValue;
                this.gml = gmlValue;
            }

            internal int CoordinateSystemId { get { return this.srid; } }
            internal byte[] CloneBinary() { return (this.wkb == null ? null : (byte[])this.wkb.Clone()); }
            internal string Text { get { return this.wkt; } }
            internal string GML { get { return this.gml; } }
        }

        #endregion

        internal static readonly DefaultSpatialServices Instance = new DefaultSpatialServices();

        private DefaultSpatialServices()
            : base()
        {
        }

        private static Exception SpatialServicesUnavailable()
        {
            // 
            return new NotImplementedException();
        }
                
        private static ReadOnlySpatialValues CheckProviderValue(object providerValue)
        {
            ReadOnlySpatialValues expectedValue = providerValue as ReadOnlySpatialValues;
            if (expectedValue == null)
            {
                throw SpatialExceptions.ProviderValueNotCompatibleWithSpatialServices();
            }
            return expectedValue;
        }

        private static ReadOnlySpatialValues CheckCompatible(DbGeography geographyValue)
        {
            Debug.Assert(geographyValue != null, "Validate geographyValue is non-null before calling CheckCompatible");
            if (geographyValue != null)
            {
                ReadOnlySpatialValues expectedValue = geographyValue.ProviderValue as ReadOnlySpatialValues;
                if (expectedValue != null)
                {
                    return expectedValue;
                }
            }
            throw SpatialExceptions.GeographyValueNotCompatibleWithSpatialServices("geographyValue");
        }

        private static ReadOnlySpatialValues CheckCompatible(DbGeometry geometryValue)
        {
            Debug.Assert(geometryValue != null, "Validate geometryValue is non-null before calling CheckCompatible");
            if (geometryValue != null)
            {
                ReadOnlySpatialValues expectedValue = geometryValue.ProviderValue as ReadOnlySpatialValues;
                if (expectedValue != null)
                {
                    return expectedValue;
                }
            }
            throw SpatialExceptions.GeometryValueNotCompatibleWithSpatialServices("geometryValue");
        }
                
        #region Geography API
                
        public override DbGeography GeographyFromProviderValue(object providerValue)
        {
            providerValue.CheckNull("providerValue");
            ReadOnlySpatialValues expectedValue = CheckProviderValue(providerValue);
            return CreateGeography(this, expectedValue);
        }

        public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
        {
            wellKnownValue.CheckNull("wellKnownValue");
            return new ReadOnlySpatialValues(wellKnownValue.CoordinateSystemId, wellKnownValue.WellKnownText, wellKnownValue.WellKnownBinary, gmlValue: null);
        }

        public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
        {
            geographyValue.CheckNull("geographyValue");
            ReadOnlySpatialValues backingValue = CheckCompatible(geographyValue);
            return new DbGeographyWellKnownValue() { CoordinateSystemId = backingValue.CoordinateSystemId, WellKnownBinary = backingValue.CloneBinary(), WellKnownText = backingValue.Text };
        }

        #region Static Constructors - Well Known Binary (WKB)

        public override DbGeography GeographyFromBinary(byte[] geographyBinary)
        {
            geographyBinary.CheckNull("geographyBinary");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(DbGeography.DefaultCoordinateSystemId, textValue: null, binaryValue: geographyBinary, gmlValue: null);
            return DbSpatialServices.CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            geographyBinary.CheckNull("geographyBinary");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(spatialReferenceSystemId, textValue: null, binaryValue: geographyBinary, gmlValue: null);
            return DbSpatialServices.CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyLineFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPointFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPolygonFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

       public override DbGeography GeographyMultiLineFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

         public override DbGeography GeographyMultiPointFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

         [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon", Justification = "Match MultiPoint, MultiLine")]
         public override DbGeography GeographyMultiPolygonFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyCollectionFromBinary(byte[] geographyBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - Well Known Text (WKT)

        public override DbGeography GeographyFromText(string geographyText)
        {
            geographyText.CheckNull("geographyText");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(DbGeography.DefaultCoordinateSystemId, textValue: geographyText, binaryValue: null, gmlValue: null);
            return DbSpatialServices.CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyFromText(string geographyText, int spatialReferenceSystemId)
        {
            geographyText.CheckNull("geographyText");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(spatialReferenceSystemId, textValue: geographyText, binaryValue: null, gmlValue: null);
            return DbSpatialServices.CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyLineFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPointFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyPolygonFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyMultiLineFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyMultiPointFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon", Justification = "Match MultiPoint, MultiLine")]
        public override DbGeography GeographyMultiPolygonFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GeographyCollectionFromText(string geographyText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - GML

        public override DbGeography GeographyFromGml(string geographyMarkup)
        {
            geographyMarkup.CheckNull("geographyMarkup");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(DbGeography.DefaultCoordinateSystemId, textValue: null, binaryValue: null, gmlValue: geographyMarkup);
            return DbSpatialServices.CreateGeography(this, backingValue);
        }

        public override DbGeography GeographyFromGml(string geographyMarkup, int spatialReferenceSystemId)
        {
            geographyMarkup.CheckNull("geographyMarkup");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(spatialReferenceSystemId, textValue: null, binaryValue: null, gmlValue: geographyMarkup);
            return DbSpatialServices.CreateGeography(this, backingValue);
        }

        #endregion

        #region Geography Instance Property Accessors

        public override int GetCoordinateSystemId(DbGeography geographyValue)
        {
            geographyValue.CheckNull("geographyValue");
            ReadOnlySpatialValues backingValue = CheckCompatible(geographyValue);
            return backingValue.CoordinateSystemId;
        }

        public override int GetDimension(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override string GetSpatialTypeName(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsEmpty(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geography Well Known Format Conversion

        public override string AsText(DbGeography geographyValue)
        {
           geographyValue.CheckNull("geographyValue");
            ReadOnlySpatialValues expectedValue = CheckCompatible(geographyValue);
            return expectedValue.Text;
        }

        public override byte[] AsBinary(DbGeography geographyValue)
        {
            geographyValue.CheckNull("geographyValue");
            ReadOnlySpatialValues expectedValue = CheckCompatible(geographyValue);
            return expectedValue.CloneBinary();
        }

        public override string AsGml(DbGeography geographyValue)
        {
            geographyValue.CheckNull("geographyValue");
            ReadOnlySpatialValues expectedValue = CheckCompatible(geographyValue);
            return expectedValue.GML;
        }

        #endregion

        #region Geography Instance Methods - Spatial Relation

        public override bool SpatialEquals(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Disjoint(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Intersects(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geography Instance Methods - Spatial Analysis

        public override DbGeography Buffer(DbGeography geographyValue, double distance)
        {
            throw SpatialServicesUnavailable();
        }

        public override double Distance(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography Intersection(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography Union(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography Difference(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography SymmetricDifference(DbGeography geographyValue, DbGeography otherGeography)
        {
            throw SpatialServicesUnavailable();
        }


        #endregion

        #region Geography Collection

        public override int? GetElementCount(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography ElementAt(DbGeography geographyValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Point

        public override double? GetLatitude(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetLongitude(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetElevation(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetMeasure(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Curve

        public override double? GetLength(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GetEndPoint(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography GetStartPoint(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool? GetIsClosed(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region LineString, Line, LinearRing

        public override int? GetPointCount(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeography PointAt(DbGeography geographyValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Surface

        public override double? GetArea(DbGeography geographyValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #endregion

        #region Geometry API
                
        public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
        {
            wellKnownValue.CheckNull("wellKnownValue");
            return new ReadOnlySpatialValues(wellKnownValue.CoordinateSystemId, wellKnownValue.WellKnownText, wellKnownValue.WellKnownBinary, gmlValue: null);
        }

        public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            ReadOnlySpatialValues backingValue = CheckCompatible(geometryValue);
            return new DbGeometryWellKnownValue() { CoordinateSystemId = backingValue.CoordinateSystemId, WellKnownBinary = backingValue.CloneBinary(), WellKnownText = backingValue.Text };
        }

        public override DbGeometry GeometryFromProviderValue(object providerValue)
        {
            providerValue.CheckNull("providerValue");
            ReadOnlySpatialValues expectedValue = CheckProviderValue(providerValue);
            return CreateGeometry(this, expectedValue);
        }

        #region Static Constructors - Well Known Binary (WKB)

        public override DbGeometry GeometryFromBinary(byte[] geometryBinary)
        {
            geometryBinary.CheckNull("geometryBinary");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(DbGeometry.DefaultCoordinateSystemId, textValue: null, binaryValue: geometryBinary, gmlValue: null);
            return DbSpatialServices.CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            geometryBinary.CheckNull("geometryBinary");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(spatialReferenceSystemId, textValue: null, binaryValue: geometryBinary, gmlValue: null);
            return DbSpatialServices.CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryLineFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPointFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPolygonFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

       public override DbGeometry GeometryMultiLineFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiPointFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon", Justification = "Match MultiPoint, MultiLine")]
        public override DbGeometry GeometryMultiPolygonFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryCollectionFromBinary(byte[] geometryBinary, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - Well Known Text (WKT)

        public override DbGeometry GeometryFromText(string geometryText)
        {
            geometryText.CheckNull("geometryText");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(DbGeometry.DefaultCoordinateSystemId, textValue: geometryText, binaryValue: null, gmlValue: null);
            return DbSpatialServices.CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryFromText(string geometryText, int spatialReferenceSystemId)
        {
            geometryText.CheckNull("geometryText");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(spatialReferenceSystemId, textValue: geometryText, binaryValue: null, gmlValue: null);
            return DbSpatialServices.CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryLineFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPointFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryPolygonFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiLineFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryMultiPointFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "MultiPolygon", Justification = "Match MultiPoint, MultiLine")]
        public override DbGeometry GeometryMultiPolygonFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GeometryCollectionFromText(string geometryText, int spatialReferenceSystemId)
        {
            // Without a backing implementation, this method cannot enforce the requirement that the result be of the specified geometry type
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Static Constructors - GML

        public override DbGeometry GeometryFromGml(string geometryMarkup)
        {
            geometryMarkup.CheckNull("geometryMarkup");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(DbGeometry.DefaultCoordinateSystemId, textValue: null, binaryValue: null, gmlValue: geometryMarkup);
            return DbSpatialServices.CreateGeometry(this, backingValue);
        }

        public override DbGeometry GeometryFromGml(string geometryMarkup, int spatialReferenceSystemId)
        {
            geometryMarkup.CheckNull("geometryMarkup");
            ReadOnlySpatialValues backingValue = new ReadOnlySpatialValues(spatialReferenceSystemId, textValue: null, binaryValue: null, gmlValue: geometryMarkup);
            return DbSpatialServices.CreateGeometry(this, backingValue);
        }

        #endregion

        #region Geometry Instance Property Accessors

        public override int GetCoordinateSystemId(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            ReadOnlySpatialValues backingValue = CheckCompatible(geometryValue);
            return backingValue.CoordinateSystemId;
        }

        public override DbGeometry GetBoundary(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override int GetDimension(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetEnvelope(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override string GetSpatialTypeName(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsEmpty(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsSimple(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool GetIsValid(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Well Known Format Conversion

        public override string AsText(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            ReadOnlySpatialValues expectedValue = CheckCompatible(geometryValue);
            return expectedValue.Text;
        }

        public override byte[] AsBinary(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            ReadOnlySpatialValues expectedValue = CheckCompatible(geometryValue);
            return expectedValue.CloneBinary();
        }

        public override string AsGml(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            ReadOnlySpatialValues expectedValue = CheckCompatible(geometryValue);
            return expectedValue.GML;
        }

        #endregion

        #region Geometry Instance Methods - Spatial Relation

        public override bool SpatialEquals(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }
        
        public override bool Disjoint(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Intersects(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Touches(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Crosses(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Within(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Contains(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Overlaps(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool Relate(DbGeometry geometryValue, DbGeometry otherGeometry, string matrix)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Instance Methods - Spatial Analysis

        public override DbGeometry Buffer(DbGeometry geometryValue, double distance)
        {
            throw SpatialServicesUnavailable();
        }

        public override double Distance(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetConvexHull(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry Intersection(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry Union(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry Difference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry SymmetricDifference(DbGeometry geometryValue, DbGeometry otherGeometry)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Instance Methods - Geometry Collection

        public override int? GetElementCount(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry ElementAt(DbGeometry geometryValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Geometry Instance Methods - Geometry Collection

        public override double? GetXCoordinate(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetYCoordinate(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetElevation(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override double? GetMeasure(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Curve

        public override double? GetLength(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetEndPoint(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetStartPoint(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool? GetIsClosed(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override bool? GetIsRing(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region LineString, Line, LinearRing

        public override int? GetPointCount(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry PointAt(DbGeometry geometryValue, int index)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Surface

        public override double? GetArea(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetCentroid(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry GetPointOnSurface(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        #endregion

        #region Polygon

        public override DbGeometry GetExteriorRing(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override int? GetInteriorRingCount(DbGeometry geometryValue)
        {
            throw SpatialServicesUnavailable();
        }

        public override DbGeometry InteriorRingAt(DbGeometry geometryValue, int index)
        {
            throw SpatialServicesUnavailable();
        }
        #endregion

        #endregion
    }
}
