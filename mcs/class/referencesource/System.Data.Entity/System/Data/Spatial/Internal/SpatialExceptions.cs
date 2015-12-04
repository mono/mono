//------------------------------------------------------------------------------
// <copyright file="SqlSpatialServices.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner [....]
//------------------------------------------------------------------------------

namespace System.Data.Spatial.Internal
{
    using System;
    using System.Data;

    internal static class SpatialExceptions
    {
        internal static ArgumentNullException ArgumentNull(string argumentName)
        {
            // 
            return EntityUtil.ArgumentNull(argumentName);
        }

        internal static Exception ProviderValueNotCompatibleWithSpatialServices()
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.Spatial_ProviderValueNotCompatibleWithSpatialServices, "providerValue");
        }

        /// <summary>
        /// Thrown whenever DbGeograpy/DbGeometry.WellKnownValue is set after regular construction (not deserialization instantiation).
        /// </summary>
        /// <returns><see cref="InvalidOperationException"/></returns>
        internal static InvalidOperationException WellKnownValueSerializationPropertyNotDirectlySettable()
        {
            // 
            return EntityUtil.InvalidOperation(System.Data.Entity.Strings.Spatial_WellKnownValueSerializationPropertyNotDirectlySettable);
        }

        #region Geography-specific exceptions

        internal static Exception GeographyValueNotCompatibleWithSpatialServices(string argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.Spatial_GeographyValueNotCompatibleWithSpatialServices, argumentName);
        }

        internal static Exception WellKnownGeographyValueNotValid(string argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.Spatial_WellKnownGeographyValueNotValid, argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeographyValueNoSrid(string argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoSrid, argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeographyValueNoWkbOrWkt(string argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.SqlSpatialservices_CouldNotCreateWellKnownGeographyValueNoWkbOrWkt, argumentName);
        }

        #endregion

        #region Geometry-specific exceptions

        internal static Exception GeometryValueNotCompatibleWithSpatialServices(string argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.Spatial_GeometryValueNotCompatibleWithSpatialServices, argumentName);
        }

        internal static Exception WellKnownGeometryValueNotValid(string argumentName)
        {
            // 
            throw EntityUtil.Argument(System.Data.Entity.Strings.Spatial_WellKnownGeometryValueNotValid, argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeometryValueNoSrid(String argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoSrid, argumentName);
        }

        internal static Exception CouldNotCreateWellKnownGeometryValueNoWkbOrWkt(String argumentName)
        {
            // 
            return EntityUtil.Argument(System.Data.Entity.Strings.SqlSpatialservices_CouldNotCreateWellKnownGeometryValueNoWkbOrWkt, argumentName);
        }
               
        #endregion

        #region SqlSpatialServices-specific Exceptions

        internal static Exception SqlSpatialServices_ProviderValueNotSqlType(Type requiredType)
        {
            return EntityUtil.Argument(System.Data.Entity.Strings.SqlSpatialServices_ProviderValueNotSqlType(requiredType.AssemblyQualifiedName), "providerValue");
        }
                
        #endregion
    }
}
