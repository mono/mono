//---------------------------------------------------------------------
// <copyright file="CollectionKind.cs" company="Microsoft">
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
    /// Kind of collection (applied to Properties)
    /// </summary>
    public enum CollectionKind
    {
        /// <summary>
        /// Property is not a Collection
        /// </summary>
        None,

        /// <summary>
        /// Collection has Bag semantics( unordered and duplicates ok)
        /// </summary>
        Bag,

        /// <summary>
        /// Collection has List semantics
        /// (Order is deterministic and duplciates ok)
        /// </summary>
        List,
    }
}