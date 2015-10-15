//---------------------------------------------------------------------
// <copyright file="ConcurrencyMode.cs" company="Microsoft">
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
    /// The concurrency mode for properties.
    /// </summary>
    public enum ConcurrencyMode
    {
        /// <summary>
        /// Default concurrency mode: the property is never validated
        /// at write time
        /// </summary>
        None,

        /// <summary>
        /// Fixed concurrency mode: the property is always validated at 
        /// write time
        /// </summary>
        Fixed,
    }
}