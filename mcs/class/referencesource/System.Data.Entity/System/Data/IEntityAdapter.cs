//------------------------------------------------------------------------------
// <copyright file="IEntityAdapter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">bfung</owner>
// <owner current="true" primary="false">spather</owner>
//------------------------------------------------------------------------------

using System.Data.Common;

namespace System.Data
{
    /// <summary>
    /// The IEntityAdapter interface allows adapters to support updates of entities stored in an IEntityCache.
    /// </summary>
    internal interface IEntityAdapter
    {
        /// <summary>
        /// Gets or sets the connection used by this adapter.
        /// </summary>
        DbConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets whether the IEntityCache.AcceptChanges should be called during a call to IEntityAdapter.Update.
        /// </summary>
        bool AcceptChangesDuringUpdate { get; set; }

        /// <summary>
        /// Gets of sets the command timeout for update operations. If null, indicates that the default timeout
        /// for the provider should be used.
        /// </summary>
        Int32? CommandTimeout { get; set; }

        /// <summary>
        /// Persists the changes made in the entity cache to the store.
        /// </summary>
        Int32 Update(IEntityStateManager cache);
    }
}
