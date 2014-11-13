//---------------------------------------------------------------------
// <copyright file="EntityState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Describes state of an entity
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames")]
    [Flags]
    [BindableType(IsBindable=false)]
    public enum EntityState {
        /// <summary>
        /// The entity has been created but is not part of ObjectStateManager.
        /// A entity is in this state immediately after it has been created and 
        /// before it is added to a ObjectStateManager, or if it has been 
        /// removed from a ObjectStateManager.
        /// </summary>
        Detached  = 0x00000001,
        /// <summary>
        /// The entity has not changed since AcceptChanges was last called.
        /// </summary>
        Unchanged = 0x00000002,
        /// <summary>
        /// The entity was added to a ObjectStateManager, and AcceptChanges has not been called.
        /// </summary>
        Added     = 0x00000004,
        /// <summary>
        /// The entity was deleted using the Delete method of the ObjectStateManager.
        /// </summary>
        Deleted   = 0x00000008,
        /// <summary>
        /// The entity was modified and AcceptChanges has not been called.
        /// </summary>
        Modified  = 0x00000010
    }
}
