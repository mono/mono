//---------------------------------------------------------------------
// <copyright file="MappingItemCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping
{
    using System;
    using System.Data.Metadata.Edm;

    /// <summary>
    /// Class for representing a collection of mapping items in Edm space.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class MappingItemCollection : ItemCollection
    {
        /// <summary>
        /// The default constructor for ItemCollection
        /// </summary>
        internal MappingItemCollection(DataSpace dataSpace) : base(dataSpace)
        {
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal virtual bool TryGetMap(string identity, DataSpace typeSpace, out Map map)
        {
            //will only be implemented by Mapping Item Collections
            throw System.Data.Entity.Error.NotSupported();
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        internal virtual Map GetMap(GlobalItem item)
        {
            //will only be implemented by Mapping Item Collections
            throw System.Data.Entity.Error.NotSupported();
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal virtual bool TryGetMap(GlobalItem item, out Map map)
        {
            //will only be implemented by Mapping Item Collections
            throw System.Data.Entity.Error.NotSupported();
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal virtual Map GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
        {
            //will only be implemented by Mapping Item Collections
            throw System.Data.Entity.Error.NotSupported();
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal virtual bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out Map map)
        {
            //will only be implemented by Mapping Item Collections
            throw System.Data.Entity.Error.NotSupported();
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal virtual Map GetMap(string identity, DataSpace typeSpace)
        {
            //will only be implemented by Mapping Item Collections
            throw System.Data.Entity.Error.NotSupported();
        }
    }//---- ItemCollection

}//---- 
