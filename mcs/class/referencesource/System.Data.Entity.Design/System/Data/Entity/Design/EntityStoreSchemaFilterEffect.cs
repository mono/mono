//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaFilterEffect.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// The effect that the filter entry should have on the results
    /// 
    /// When a database object matchs the pattern for both an allow and exclude EntityStoreSchemaFilterEntry,
    /// the database object will be excluded.
    /// </summary>
    public enum EntityStoreSchemaFilterEffect
    {
        /// <summary>Allow the entries that match the specified pattern.</summary>
        Allow = 0,
        /// <summary>Exclude the entries that match the specified pattern.</summary>
        Exclude = 1,
    }
}
