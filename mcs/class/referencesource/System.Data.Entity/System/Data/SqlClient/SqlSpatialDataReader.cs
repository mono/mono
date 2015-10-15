//------------------------------------------------------------------------------
// <copyright file="SqlSpatialDataReader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Spatial;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.SqlClient
{
    /// <summary>
    /// SqlClient specific implementation of <see cref="DbSpatialDataReader"/>
    /// </summary>
    internal sealed partial class SqlSpatialDataReader : DbSpatialDataReader
    {
        private readonly SqlDataReader reader;
        private const string geometrySqlType = "sys.geometry";
        private const string geographySqlType = "sys.geography";

        internal SqlSpatialDataReader(SqlDataReader underlyingReader)
        {
            this.reader = underlyingReader;
        }

        public override DbGeography GetGeography(int ordinal)
        {
            EnsureGeographyColumn(ordinal);
            SqlBytes geogBytes = this.reader.GetSqlBytes(ordinal);
            object providerValue = sqlGeographyFromBinaryReader.Value(new BinaryReader(geogBytes.Stream));
            return SqlSpatialServices.Instance.GeographyFromProviderValue(providerValue);
        }

        public override DbGeometry GetGeometry(int ordinal)
        {
            EnsureGeometryColumn(ordinal);
 	        SqlBytes geomBytes = this.reader.GetSqlBytes(ordinal);
            object providerValue = sqlGeometryFromBinaryReader.Value(new BinaryReader(geomBytes.Stream));
            return SqlSpatialServices.Instance.GeometryFromProviderValue(providerValue);
        }

        private static readonly Singleton<Func<BinaryReader, object>> sqlGeographyFromBinaryReader = new Singleton<Func<BinaryReader, object>>(() => CreateBinaryReadDelegate(SqlProviderServices.GetSqlTypesAssembly().SqlGeographyType));
        private static readonly Singleton<Func<BinaryReader, object>> sqlGeometryFromBinaryReader = new Singleton<Func<BinaryReader, object>>(() => CreateBinaryReadDelegate(SqlProviderServices.GetSqlTypesAssembly().SqlGeometryType));

        // test to ensure that the SQL column has the expected SQL type.   Don't use the CLR type to avoid having to worry about differences in 
        // type versions between the client and the database.  
        private void EnsureGeographyColumn(int ordinal)
        {
            string fieldTypeName = this.reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith(geographySqlType, StringComparison.Ordinal)) // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeographyColumn(fieldTypeName));
            }
        }

        private void EnsureGeometryColumn(int ordinal)
        {
            string fieldTypeName = this.reader.GetDataTypeName(ordinal);
            if (!fieldTypeName.EndsWith(geometrySqlType, StringComparison.Ordinal)) // Use EndsWith so that we just see the schema and type name, not the database name.
            {
                throw new InvalidDataException(Strings.SqlProvider_InvalidGeometryColumn(fieldTypeName));
            }
        }

        /// <summary>
        /// Builds and compiles the Expression equivalent of the following:
        ///   
        /// (BinaryReader r) => { var result = new SpatialType(); result.Read(r); return r; }
        ///   
        /// The construct/read pattern is preferred over casting the result of calling GetValue on the DataReader,
        /// because constructing the value directly allows client code to specify the type, rather than SqlClient using
        /// the server-specified assembly qualified type name from TDS to try to locate the correct type on the client.
        /// </summary>
        /// <param name="spatialType"></param>
        /// <returns></returns>
        private static Func<BinaryReader, object> CreateBinaryReadDelegate(Type spatialType)
        {
            Debug.Assert(spatialType != null, "Ensure spatialType is non-null before calling CreateBinaryReadDelegate");

            var readerParam = Expression.Parameter(typeof(BinaryReader));
            var binarySerializable = Expression.Variable(spatialType);
            var readMethod = spatialType.GetMethod("Read", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(System.IO.BinaryReader) }, null);

            var ex = Expression.Lambda<Func<BinaryReader, object>>(
                            Expression.Block(
                                new[] { binarySerializable },
                                Expression.Assign(binarySerializable, Expression.New(spatialType)),
                                Expression.Call(binarySerializable, readMethod, readerParam),
                                binarySerializable
                            ),
                            readerParam
                        );

            Func<BinaryReader, object> result = ex.Compile();
            return result;
        }
    }
}
