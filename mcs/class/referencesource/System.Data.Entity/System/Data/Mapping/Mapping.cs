//---------------------------------------------------------------------
// <copyright file="Mapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System.Data.Metadata.Edm;
using System.Globalization;

namespace System.Data.Mapping
{
    /// <summary>
    /// Represents the base item class for all the mapping metadata
    /// </summary>
    internal abstract class Map : GlobalItem
    {
        protected Map() 
            : base(MetadataFlags.Readonly) 
        {
        }
        #region Properties
        /// <summary>
        /// Returns the Item that is being mapped either for ES or OE spaces.
        /// The EDM type will be an EntityContainer type in ES mapping case.
        /// In the OE mapping case it could be any type.
        /// </summary>
        internal abstract MetadataItem EdmItem { get; }
        #endregion
    }
}
