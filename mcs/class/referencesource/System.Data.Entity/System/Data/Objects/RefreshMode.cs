//------------------------------------------------------------------------------
// <copyright file="RefreshMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//------------------------------------------------------------------------------

namespace System.Data.Objects
{
    /// <summary>
    /// 
    /// </summary>
    ///
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum RefreshMode
    {
        /// <summary>
        /// For unmodified client objects, same behavior as StoreWins.  For modified client
        /// objects, Refresh original values with store value, keeping all values on client
        /// object. The next time an update happens, all the client change units will be
        /// considered modified and require updating.
        /// </summary>
        ///
        ClientWins       = MergeOption.PreserveChanges,
        
        /// <summary>
        /// Discard all changes on the client and refresh values with store values.
        /// Client original values is updated to match the store.
        /// </summary>
        ///
        StoreWins      = MergeOption.OverwriteChanges,
    }
}

