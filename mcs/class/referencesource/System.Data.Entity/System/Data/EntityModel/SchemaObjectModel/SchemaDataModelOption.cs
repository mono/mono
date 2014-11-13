//---------------------------------------------------------------------
// <copyright file="SchemaDataModelOption.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Which data model to target
    /// </summary>
    internal enum SchemaDataModelOption
    {
        /// <summary>
        /// Target the CDM data model
        /// </summary>
        EntityDataModel = 0,

        /// <summary>
        /// Target the data providers - SQL, Oracle, etc
        /// </summary>
        ProviderDataModel = 1,

        /// <summary>
        /// Target the data providers - SQL, Oracle, etc
        /// </summary>
        ProviderManifestModel = 2,
    }
}
