//------------------------------------------------------------------------------
// <copyright file="SpatialHelpers.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner [....]
//------------------------------------------------------------------------------
using System.Diagnostics;
using System.Data.Metadata.Edm;
using System.Data.Common;

namespace System.Data.Spatial
{
    internal static class SpatialHelpers
    {
        internal static object GetSpatialValue(MetadataWorkspace workspace, DbDataReader reader, TypeUsage columnType, int columnOrdinal)
        {
            Debug.Assert(Helper.IsSpatialType(columnType));
            DbSpatialDataReader spatialReader = CreateSpatialDataReader(workspace, reader);
            if (Helper.IsGeographicType((PrimitiveType)columnType.EdmType))
            {
                return spatialReader.GetGeography(columnOrdinal);
            }
            else
            {
                return spatialReader.GetGeometry(columnOrdinal);
            }
        }

        internal static DbSpatialDataReader CreateSpatialDataReader(MetadataWorkspace workspace, DbDataReader reader)
        {
            StoreItemCollection storeItemCollection = (StoreItemCollection)workspace.GetItemCollection(DataSpace.SSpace);
            DbProviderFactory providerFactory = storeItemCollection.StoreProviderFactory;
            Debug.Assert(providerFactory != null, "GetProviderSpatialServices requires provider factory to have been initialized");

            DbProviderServices providerServices = DbProviderServices.GetProviderServices(providerFactory);
            DbSpatialDataReader result = providerServices.GetSpatialDataReader(reader, storeItemCollection.StoreProviderManifestToken);
            Debug.Assert(result != null, "DbProviderServices did not throw ProviderIncompatibleException for null IDbSpatialDataReader");

            return result;
        }
    }
}
