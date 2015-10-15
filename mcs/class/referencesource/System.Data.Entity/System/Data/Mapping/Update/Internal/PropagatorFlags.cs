//---------------------------------------------------------------------
// <copyright file="PropagatorFlags.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner cmeek
// @backupOwner pratikp
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// Tracks roles played by a record as it propagates
    /// w.r.t. an update mapping view.
    /// </summary>
    [Flags]
    internal enum PropagatorFlags : byte
    {
        /// <summary>
        /// No role.
        /// </summary>
        NoFlags = 0,
        /// <summary>
        /// Value is unchanged. Used only for attributes that appear in updates (in other words,
        /// in both delete and insert set).
        /// </summary>
        Preserve = 1,
        /// <summary>
        /// Value is a concurrency token. Placeholder for post Beta 2 work.
        /// </summary>
        ConcurrencyValue = 2,
        /// <summary>
        /// Value is unknown. Used only for attributes that appear in updates (in other words,
        /// in both delete and insert set).
        /// </summary>
        Unknown = 8,
        /// <summary>
        /// Value is a key, and therefore a concurrency value, but it is shared so it
        /// only needs to be checked in a single table (in the case of entity splitting)
        /// </summary>
        Key = 16,
        /// <summary>
        /// Value is a foreign key.
        /// </summary>
        ForeignKey = 32,
    }
}