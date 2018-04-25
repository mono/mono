//---------------------------------------------------------------------
// <copyright file="StoreGeneratedPattern.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner anpete
//---------------------------------------------------------------------

using System;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// The pattern for Server Generated Properties.
    /// </summary>
    public enum StoreGeneratedPattern
    {
        /// <summary>
        /// Not a Server Generated Property. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// A value is generated on INSERT, and remains unchanged on update.
        /// </summary>
        Identity = 1,

        /// <summary>
        /// A value is generated on both INSERT and UPDATE.
        /// </summary>
        Computed = 2,
    }
}