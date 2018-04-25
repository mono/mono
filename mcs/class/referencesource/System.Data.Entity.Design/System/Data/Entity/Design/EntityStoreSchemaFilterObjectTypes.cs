//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaFilterObjectTypes.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner srimand
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// The type of store object to apply this filter to
    /// </summary>
    [Flags]
    public enum EntityStoreSchemaFilterObjectTypes
    {
        /// <summary>
        /// The value that this enum will initilize to.  
        /// This is not a valid value to be use.
        /// </summary>
        None = 0x0000,
        /// <summary>Apply this filter to table object types.</summary>
        Table = 0x0001,
        /// <summary>Apply this filter to view object types.</summary>
        View = 0x0002,
        /// <summary>Apply this filter to function object types.</summary>
        Function = 0x0004,
        /// <summary>Apply this filter to all possible object types.</summary>
        All = Table | View | Function,
    }
}
