//---------------------------------------------------------------------
// <copyright file="OperationAction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the list of possible actions for delete operation
    /// </summary>
    public enum OperationAction
    {
        /// <summary>
        /// no action
        /// </summary>
        None,

        /// <summary>
        /// Cascade to other ends
        /// </summary>
        Cascade,

        /// <summary>
        /// Do not allow if other ends are not empty 
        /// </summary>
        Restrict,
    }
}
