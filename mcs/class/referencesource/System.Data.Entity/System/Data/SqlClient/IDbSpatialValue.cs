//------------------------------------------------------------------------------
// <copyright file="IDbSpatialValue.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.Spatial;
using System.Data.Metadata.Edm;

namespace System.Data.SqlClient.Internal
{
    /// <summary>
    /// Adapter interface to make working with instances of <see cref="DbGeometry"/> or <see cref="DbGeography"/> easier.  
    /// Implementing types wrap instances of DbGeography/DbGeometry and allow them to be consumed in a common way. 
    /// This interface is implemented by wrapping types for two reasons:
    /// 1. The DbGeography/DbGeometry classes cannot directly implement internal interfaces because their members are virtual (behavior is not guaranteed).
    /// 2. The wrapping types ensure that instances of IDbSpatialValue handle the <see cref="NotImplementedException"/>s thrown
    ///    by any unimplemented members of derived DbGeography/DbGeometry types that correspond to the properties and methods declared in the interface.
    /// </summary>
    internal interface IDbSpatialValue
    {
        bool IsGeography { get; }
        PrimitiveTypeKind PrimitiveType { get; }
        object ProviderValue { get; }
        int? CoordinateSystemId { get; }
        string WellKnownText { get; }
        byte[] WellKnownBinary { get; }
        string GmlString { get; }
        
        Exception NotSqlCompatible();
    }

    internal static class IDbSpatialValueExtensionMethods
    {
        /// <summary>
        /// Returns an instance of <see cref="IDbSpatialValue"/> that wraps the specified <see cref="DbGeography"/> value.
        /// IDbSpatialValue members are guaranteed not to throw the <see cref="NotImplementedException"/>s caused by unimplemented members of their wrapped values.
        /// </summary>
        /// <param name="geographyValue">The geography instance to wrap</param>
        /// <returns>An instance of <see cref="IDbSpatialValue"/> that wraps the specified geography value</returns>
        internal static IDbSpatialValue AsSpatialValue(this DbGeography geographyValue)
        {
            if (geographyValue == null)
            {
                return null;
            }
            return new DbGeographyAdapter(geographyValue);
        }

        /// <summary>
        /// Returns an instance of <see cref="IDbSpatialValue"/> that wraps the specified <see cref="DbGeometry"/> value.
        /// IDbSpatialValue members are guaranteed not to throw the <see cref="NotImplementedException"/>s caused by unimplemented members of their wrapped values.
        /// </summary>
        /// <param name="geometryValue">The geometry instance to wrap</param>
        /// <returns>An instance of <see cref="IDbSpatialValue"/> that wraps the specified geometry value</returns>
        internal static IDbSpatialValue AsSpatialValue(this DbGeometry geometryValue)
        {
            if (geometryValue == null)
            {
                return null;
            }
            return new DbGeometryAdapter(geometryValue);
        }
    }

    internal struct DbGeographyAdapter : IDbSpatialValue
    {
        private readonly DbGeography value;

        internal DbGeographyAdapter(DbGeography geomValue)
        {
            this.value = geomValue;
        }

        private TResult NullIfNotImplemented<TResult>(Func<DbGeography, TResult> accessor)
            where TResult : class
        {
            try
            {
                return accessor(this.value);
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        private int? NullIfNotImplemented(Func<DbGeography, int> accessor)
        {
            try
            {
                return accessor(this.value);
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        public bool IsGeography { get { return true; } }

        public PrimitiveTypeKind PrimitiveType { get { return PrimitiveTypeKind.Geography; } }

        public object ProviderValue
        {
            get { return NullIfNotImplemented(geog => geog.ProviderValue); }
        }

        public int? CoordinateSystemId
        {
            get { return NullIfNotImplemented(geog => geog.CoordinateSystemId); }
        }

        public string WellKnownText
        {
            get
            {
                return NullIfNotImplemented(geog => geog.AsTextIncludingElevationAndMeasure())
                    ?? NullIfNotImplemented(geog => geog.AsText()); // better than nothing if the provider doesn't support AsTextIncludingElevationAndMeasure
            }
        }

        public byte[] WellKnownBinary
        {
            get { return NullIfNotImplemented(geog => geog.AsBinary()); }
        }

        public string GmlString
        {
            get { return NullIfNotImplemented(geog => geog.AsGml()); }
        }

        public Exception NotSqlCompatible() { return EntityUtil.GeographyValueNotSqlCompatible(); }
    }

    internal struct DbGeometryAdapter : IDbSpatialValue
    {
        private readonly DbGeometry value;

        internal DbGeometryAdapter(DbGeometry geomValue)
        {
            this.value = geomValue;
        }

        private TResult NullIfNotImplemented<TResult>(Func<DbGeometry, TResult> accessor)
            where TResult : class
        {
            try
            {
                return accessor(this.value);
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        private int? NullIfNotImplemented(Func<DbGeometry, int> accessor)
        {
            try
            {
                return accessor(this.value);
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        public bool IsGeography { get { return false; } }

        public PrimitiveTypeKind PrimitiveType { get { return PrimitiveTypeKind.Geometry; } }

        public object ProviderValue
        {
            get { return NullIfNotImplemented(geom => geom.ProviderValue); }
        }

        public int? CoordinateSystemId
        {
            get { return NullIfNotImplemented(geom => geom.CoordinateSystemId); }
        }

        public string WellKnownText
        {
            get 
            {
                return NullIfNotImplemented(geom => geom.AsTextIncludingElevationAndMeasure())
                    ?? NullIfNotImplemented(geom => geom.AsText()); // better than nothing if the provider doesn't support AsTextIncludingElevationAndMeasure
            }
        }

        public byte[] WellKnownBinary
        {
            get { return NullIfNotImplemented(geom => geom.AsBinary()); }
        }

        public string GmlString
        {
            get { return NullIfNotImplemented(geom => geom.AsGml()); }
        }

        public Exception NotSqlCompatible() { return EntityUtil.GeometryValueNotSqlCompatible(); }
    }
}
