//---------------------------------------------------------------------
// <copyright file="IEntityWithKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       barryfr
// @backupOwner sparra
//---------------------------------------------------------------------
using System.Data.Objects;
using System.Reflection;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// Interface that defines an entity containing a key.
    /// </summary>
    public interface IEntityWithKey
    {
        /// <summary>
        /// Returns the EntityKey for this entity.
        /// If an object is being managed by a change tracker, it is expected that
        /// IEntityChangeTracker methods EntityMemberChanging and EntityMemberChanged will be
        /// used to report changes on EntityKey. This allows the change tracker to validate the
        /// EntityKey's new value and to verify if the change tracker is in a state where it can
        /// allow updates to the EntityKey.
        /// </summary>
        EntityKey EntityKey { get; set; }
    }
}
