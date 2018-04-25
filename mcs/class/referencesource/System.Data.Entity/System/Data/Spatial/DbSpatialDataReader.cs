//---------------------------------------------------------------------
// <copyright file="IDbSpatialDataReader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  willa
// @backupOwner srimand
//---------------------------------------------------------------------

namespace System.Data.Spatial
{
    /// <summary>
    /// A provider-independent service API for geospatial (Geometry/Geography) type support.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbSpatialDataReader
    {
        /// <summary>
        /// When implemented in derived types, reads an instance of <see cref="DbGeography"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the column that contains the geography value</param>
        /// <returns>The instance of DbGeography at the specified column value</returns>
        public abstract DbGeography GetGeography(int ordinal);

        /// <summary>
        /// When implemented in derived types, reads an instance of <see cref="DbGeometry"/> from the column at the specified column ordinal. 
        /// </summary>
        /// <param name="ordinal">The ordinal of the data record column that contains the provider-specific geometry data</param>
        /// <returns>The instance of DbGeometry at the specified column value</returns>
        public abstract DbGeometry GetGeometry(int ordinal);
    }
}