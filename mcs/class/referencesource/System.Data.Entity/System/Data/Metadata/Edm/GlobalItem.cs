//---------------------------------------------------------------------
// <copyright file="GlobalItem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the base item class for all the metadata
    /// </summary>
    public abstract class GlobalItem : MetadataItem
    {
        #region Constructors
        /// <summary>
        /// Implementing this internal constructor so that this class can't be derived
        /// outside this assembly
        /// </summary>
        internal GlobalItem()
        {
        }

        internal GlobalItem(MetadataFlags flags) 
            : base(flags) 
        { 
        }
        #endregion

        #region Properties

        /// <summary>
        /// Returns the DataSpace in which this type belongs to
        /// </summary>
        [MetadataProperty(typeof(DataSpace), false)]
        internal DataSpace DataSpace
        {
            get
            {
                // Since there can be row types that span across spaces and we can have collections to such row types, we need to exclude RowType and collection type in this assert check
                Debug.Assert(GetDataSpace() != (DataSpace)(-1) || this.BuiltInTypeKind == BuiltInTypeKind.RowType || this.BuiltInTypeKind == BuiltInTypeKind.CollectionType, "DataSpace must have some valid value");
                return GetDataSpace();
            }
            set
            {
                // Whenever you assign the data space value, it must be unassigned or re-assigned to the same value.
                // The only exception being we sometimes need to create row types that contains types from various spaces
                Debug.Assert(GetDataSpace() == (DataSpace)(-1) || GetDataSpace() == value || this.BuiltInTypeKind == BuiltInTypeKind.RowType || this.BuiltInTypeKind == BuiltInTypeKind.CollectionType, "Invalid Value being set for DataSpace");
                SetDataSpace(value);
            }
        }
        #endregion  
    }
}
