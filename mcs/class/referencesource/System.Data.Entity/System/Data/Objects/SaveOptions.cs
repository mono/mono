//------------------------------------------------------------------------------
// <copyright file="SaveOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       mirszy
// @backupOwner jeffders
//------------------------------------------------------------------------------

namespace System.Data.Objects
{
    /// <summary>
    /// Flags used to modify behavior of ObjectContext.SaveChanges()
    /// </summary>
    [Flags]
    public enum SaveOptions
    {
        None = 0,
        AcceptAllChangesAfterSave = 1,
        DetectChangesBeforeSave = 2
    }
}
