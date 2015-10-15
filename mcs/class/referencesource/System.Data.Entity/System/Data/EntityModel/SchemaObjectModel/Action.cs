//---------------------------------------------------------------------
// <copyright file="Action.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       jeffreed
// @backupOwner anpete
//---------------------------------------------------------------------
using System;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Valid actions in an On&lt;Operation&gt; element
    /// </summary>
    enum Action
    {
        /// <summary>
        /// no action
        /// </summary>
        None,

        /// <summary>
        /// Cascade to other ends
        /// </summary>
        Cascade,

    }
}

