//------------------------------------------------------------------------------
// <copyright file="SqlSpatialServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Spatial;
using System.Data.SqlClient.Internal;
using System.Data.Spatial.Internal;
using System.Data.Common.Utils;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace System.Data.SqlClient
{
    /// <summary>
    /// SqlClient specific implementation of <see cref="DbSpatialServices"/>
    /// </summary>
    [Serializable]
    internal sealed partial class SqlSpatialServices : DbSpatialServices, ISerializable
    {
        /// <summary>
        /// Do not allow instantiation
        /// </summary>
        internal static readonly SqlSpatialServices Instance = new SqlSpatialServices(SqlProviderServices.GetSqlTypesAssembly);

        private static Dictionary<string, SqlSpatialServices> otherSpatialServices;

        [NonSerialized]
        private readonly Singleton<SqlTypesAssembly> _sqlTypesAssemblySingleton;

        private SqlSpatialServices(Func<SqlTypesAssembly> getSqlTypes)
        {
            Debug.Assert(getSqlTypes != null, "Validate SqlTypes assembly delegate before constructing SqlSpatialServiceS");
            this._sqlTypesAssemblySingleton = new Singleton<SqlTypesAssembly>(getSqlTypes);
            
            // Create Singletons that will delay-initialize the MethodInfo and PropertyInfo instances used to invoke SqlGeography/SqlGeometry methods via reflection.
            this.InitializeMemberInfo();
        }

        private SqlSpatialServices(SerializationInfo info, StreamingContext context)
        {
            SqlSpatialServices instance = Instance;
            this._sqlTypesAssemblySingleton = instance._sqlTypesAssemblySingleton;
            this.InitializeMemberInfo(instance);
        }

        // Given an assembly purportedly containing SqlServerTypes for spatial values, attempt to 
        // create a corersponding Sql spefic DbSpatialServices value backed by types from that assembly.
        // Uses a dictionary to ensure that there is at most db spatial service per assembly.   It's important that
        // this be done in a way that ensures that the underlying SqlTypesAssembly value is also atomized,
        // since that's caching compilation.
        // Relies on SqlTypesAssembly to verify that the assembly is appropriate.
        private static bool TryGetSpatialServiceFromAssembly(Assembly assembly, out SqlSpatialServices services)
        {
            if (otherSpatialServices == null || !otherSpatialServices.TryGetValue(assembly.FullName, out services))
            {
                lock (Instance)
                {
                    if (otherSpatialServices == null || !otherSpatialServices.TryGetValue(assembly.FullName, out services))
                    {
                        SqlTypesAssembly sqlAssembly;
                        if (SqlTypesAssembly.TryGetSqlTypesAssembly(assembly, out sqlAssembly))
                        {
                            if (otherSpatialServices == null)
                            {
                                otherSpatialServices = new Dictionary<string, SqlSpatialServices>(1);
                            }
                            services = new SqlSpatialServices(() => sqlAssembly);
                            otherSpatialServices.Add(assembly.FullName, services);
                        }
                        else
                        {
                            services = null;
                        }
                    }
                }
            }
            return services != null;
        }

        private SqlTypesAssembly SqlTypes { get { return this._sqlTypesAssemblySingleton.Value; } }
                
        public override object CreateProviderValue(DbGeographyWellKnownValue wellKnownValue)
        {
            wellKnownValue.CheckNull("wellKnownValue");

            object result = null;
            if (wellKnownValue.WellKnownText != null)
            {
                result = this.SqlTypes.SqlTypesGeographyFromText(wellKnownValue.WellKnownText, wellKnownValue.CoordinateSystemId);
            }
            else if (wellKnownValue.WellKnownBinary != null)
            {
                result = this.SqlTypes.SqlTypesGeographyFromBinary(wellKnownValue.WellKnownBinary, wellKnownValue.CoordinateSystemId);
            }
            else
            {
                throw SpatialExceptions.WellKnownGeographyValueNotValid("wellKnownValue");
            }

            return result;
        }

        public override DbGeography GeographyFromProviderValue(object providerValue)
        {
            providerValue.CheckNull("providerValue");
            object normalizedProviderValue = NormalizeProviderValue(providerValue, this.SqlTypes.SqlGeographyType);
            return this.SqlTypes.IsSqlGeographyNull(normalizedProviderValue) ? null: DbSpatialServices.CreateGeography(this, normalizedProviderValue);
        }

        // Ensure that provider values are from the expected version of the Sql types assembly.   If they aren't try to 
        // convert them so that they are.
        // 
        // Normally when we obtain values from the store, we try to use the appropriate SqlSpatialDataReader.   This will make sure that 
        // any spatial values are instantiated with the provider type from the appropriate SqlServerTypes assembly.   However, 
        // in one case (output parameter values) we don't have an opportunity to make this happen.    There we get whatever value 
        // the underlying SqlDataReader produces which doesn't necessarily produce values from the assembly we expect.
        private object NormalizeProviderValue(object providerValue, Type expectedSpatialType)
        {
            Debug.Assert(expectedSpatialType == this.SqlTypes.SqlGeographyType || expectedSpatialType == this.SqlTypes.SqlGeometryType);            
            Type providerValueType = providerValue.GetType();
            if (providerValueType != expectedSpatialType)
            {
                SqlSpatialServices otherServices;
                if (TryGetSpatialServiceFromAssembly(providerValue.GetType().Assembly, out otherServices))
                {
                    if (expectedSpatialType == this.SqlTypes.SqlGeographyType)
                    {
                        if (providerValueType == otherServices.SqlTypes.SqlGeographyType)
                        {
                            return ConvertToSqlValue(otherServices.GeographyFromProviderValue(providerValue), "providerValue");
                        }
                    }
                    else // expectedSpatialType == this.SqlTypes.SqlGeometryType
                    {
                        if (providerValueType == otherServices.SqlTypes.SqlGeometryType)
                        {
                            return ConvertToSqlValue(otherServices.GeometryFromProviderValue(providerValue), "providerValue");
                        }
                    }
                }

                throw SpatialExceptions.SqlSpatialServices_ProviderValueNotSqlType(expectedSpatialType);
            }

            return providerValue;
        }

        public override DbGeographyWellKnownValue CreateWellKnownValue(DbGeography geographyValue)
        {
            geographyValue.CheckNull("geographyValue");
            var spatialValue = geographyValue.AsSpatialValue();

            DbGeographyWellKnownValue result = CreateWellKnownValue(spatialValue, 
                () => SpatialExceptions.CouldNotCreateWellKnownGeographyValueNoSrid("geographyValue"),
                () => SpatialExceptions.CouldNotCreateWellKnownGeographyValueNoWkbOrWkt("geographyValue"),
                (srid, wkb, wkt) => new DbGeographyWellKnownValue() { CoordinateSystemId = srid, WellKnownBinary = wkb, WellKnownText = wkt });
            
            return result;
        }
       
        public override object CreateProviderValue(DbGeometryWellKnownValue wellKnownValue)
        {
            wellKnownValue.CheckNull("wellKnownValue");

            object result = null;
            if (wellKnownValue.WellKnownText != null)
            {
                result = this.SqlTypes.SqlTypesGeometryFromText(wellKnownValue.WellKnownText, wellKnownValue.CoordinateSystemId);
            }
            else if (wellKnownValue.WellKnownBinary != null)
            {
                result = this.SqlTypes.SqlTypesGeometryFromBinary(wellKnownValue.WellKnownBinary, wellKnownValue.CoordinateSystemId);
            }
            else
            {
                throw SpatialExceptions.WellKnownGeometryValueNotValid("wellKnownValue");
            }

            return result;
        }

        public override DbGeometry GeometryFromProviderValue(object providerValue)
        {
            providerValue.CheckNull("providerValue");
            object normalizedProviderValue = NormalizeProviderValue(providerValue, this.SqlTypes.SqlGeometryType);
            return this.SqlTypes.IsSqlGeometryNull(normalizedProviderValue) ? null : DbSpatialServices.CreateGeometry(this, normalizedProviderValue);
        }

        public override DbGeometryWellKnownValue CreateWellKnownValue(DbGeometry geometryValue)
        {
            geometryValue.CheckNull("geometryValue");
            var spatialValue = geometryValue.AsSpatialValue();

            DbGeometryWellKnownValue result = CreateWellKnownValue(spatialValue, 
                () => SpatialExceptions.CouldNotCreateWellKnownGeometryValueNoSrid("geometryValue"),
                () => SpatialExceptions.CouldNotCreateWellKnownGeometryValueNoWkbOrWkt("geometryValue"),
                (srid, wkb, wkt) => new DbGeometryWellKnownValue() { CoordinateSystemId = srid, WellKnownBinary = wkb, WellKnownText = wkt });
            
            return result;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // no need to serialize anything, on de-serialization we reinitialize all fields to match the 
            // those of Instance.
        }

        private static TValue CreateWellKnownValue<TValue>(IDbSpatialValue spatialValue, Func<Exception> onMissingSrid, Func<Exception> onMissingWkbAndWkt, Func<int, byte[], string, TValue> onValidValue)
        {
            int? srid = spatialValue.CoordinateSystemId;

            if (!srid.HasValue)
            {
                throw onMissingSrid();
            }

            string wkt = spatialValue.WellKnownText;
            if (wkt != null)
            {
                return onValidValue(srid.Value, null, wkt);
            }
            else
            {
                byte[] wkb = spatialValue.WellKnownBinary;
                if (wkb != null)
                {
                    return onValidValue(srid.Value, wkb, null);
                }
            }

            throw onMissingWkbAndWkt();
        }

        public override string AsTextIncludingElevationAndMeasure(DbGeography geographyValue)
        {
            return this.SqlTypes.GeographyAsTextZM(geographyValue);
        }
        
        public override string AsTextIncludingElevationAndMeasure(DbGeometry geometryValue)
        {
            return this.SqlTypes.GeometryAsTextZM(geometryValue);
        }


        #region API used by generated spatial implementation methods

        #region Reflection - remove if SqlSpatialServices uses compiled expressions instead of reflection to invoke SqlGeography/SqlGeometry methods
                
        private MethodInfo FindSqlGeographyMethod(string methodName, params Type[] argTypes)
        {
            return this.SqlTypes.SqlGeographyType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
        }

        private MethodInfo FindSqlGeographyStaticMethod(string methodName, params Type[] argTypes)
        {
            return this.SqlTypes.SqlGeographyType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
        }

        private PropertyInfo FindSqlGeographyProperty(string propertyName)
        {
            return this.SqlTypes.SqlGeographyType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        }

        private MethodInfo FindSqlGeometryStaticMethod(string methodName, params Type[] argTypes)
        {
            return this.SqlTypes.SqlGeometryType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
        }

        private MethodInfo FindSqlGeometryMethod(string methodName, params Type[] argTypes)
        {
            return this.SqlTypes.SqlGeometryType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
        }

        private PropertyInfo FindSqlGeometryProperty(string propertyName)
        {
            return this.SqlTypes.SqlGeometryType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        }
        
        #endregion

        // 



        #region Argument Conversion (conversion to SQL Server Types)

        private object ConvertToSqlValue(DbGeography geographyValue, string argumentName)
        {
            if (geographyValue == null)
            {
                return null;
            }

            return this.SqlTypes.ConvertToSqlTypesGeography(geographyValue);
        }

        private object ConvertToSqlValue(DbGeometry geometryValue, string argumentName)
        {
            if (geometryValue == null)
            {
                return null;
            }

            return this.SqlTypes.ConvertToSqlTypesGeometry(geometryValue);
        }

        private object ConvertToSqlBytes(byte[] binaryValue, string argumentName)
        {
            if (binaryValue == null)
            {
                return null;
            }

            return this.SqlTypes.SqlBytesFromByteArray(binaryValue);
        }

        private object ConvertToSqlChars(string stringValue, string argumentName)
        {
            if (stringValue == null)
            {
                return null;
            }

            return this.SqlTypes.SqlCharsFromString(stringValue);
        }

        private object ConvertToSqlString(string stringValue, string argumentName)
        {
            if (stringValue == null)
            {
                return null;
            }

            return this.SqlTypes.SqlStringFromString(stringValue);
        }

        private object ConvertToSqlXml(string stringValue, string argumentName)
        {
            if (stringValue == null)
            {
                return null;
            }

            return this.SqlTypes.SqlXmlFromString(stringValue);
        }

        #endregion

        #region Return Value Conversion (conversion from SQL Server types)

        private bool ConvertSqlBooleanToBoolean(object sqlBoolean)
        {
            return this.SqlTypes.SqlBooleanToBoolean(sqlBoolean);
        }

        private bool? ConvertSqlBooleanToNullableBoolean(object sqlBoolean)
        {
            return this.SqlTypes.SqlBooleanToNullableBoolean(sqlBoolean);
        }

        private byte[] ConvertSqlBytesToBinary(object sqlBytes)
        {
            return this.SqlTypes.SqlBytesToByteArray(sqlBytes);
        }

        private string ConvertSqlCharsToString(object sqlCharsValue)
        {
            return this.SqlTypes.SqlCharsToString(sqlCharsValue);
        }

        private string ConvertSqlStringToString(object sqlCharsValue)
        {
            return this.SqlTypes.SqlStringToString(sqlCharsValue);
        }

        private double ConvertSqlDoubleToDouble(object sqlDoubleValue)
        {
            return this.SqlTypes.SqlDoubleToDouble(sqlDoubleValue);
        }

        private double? ConvertSqlDoubleToNullableDouble(object sqlDoubleValue)
        {
            return this.SqlTypes.SqlDoubleToNullableDouble(sqlDoubleValue);
        }

        private int ConvertSqlInt32ToInt(object sqlInt32Value)
        {
            return this.SqlTypes.SqlInt32ToInt(sqlInt32Value);
        }

        private int? ConvertSqlInt32ToNullableInt(object sqlInt32Value)
        {
            return this.SqlTypes.SqlInt32ToNullableInt(sqlInt32Value);
        }

        private string ConvertSqlXmlToString(object sqlXmlValue)
        {
            return this.SqlTypes.SqlXmlToString(sqlXmlValue);
        }

        #endregion

        #endregion
    }
}
