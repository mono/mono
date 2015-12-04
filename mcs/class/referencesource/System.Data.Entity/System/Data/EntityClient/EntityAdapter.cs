//---------------------------------------------------------------------
// <copyright file="EntityAdapter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------
namespace System.Data.EntityClient
{
    using System.Data;
    using System.Data.Common;
    using System.Data.Mapping.Update.Internal;
    using System.Data.Objects;
    using System.Diagnostics;

    /// <summary>
    /// Class representing a data adapter for the conceptual layer
    /// </summary>
    internal sealed class EntityAdapter : IEntityAdapter
    {
        private bool _acceptChangesDuringUpdate = true;
        private EntityConnection _connection;
        private Int32? _commandTimeout;

        /// <summary>
        /// Constructs the EntityAdapter object without a connection object
        /// </summary>
        public EntityAdapter()
        {
        }

        /// <summary>
        /// Gets or sets the map connection used by this adapter.
        /// </summary>
        DbConnection IEntityAdapter.Connection
        {
            get
            {
                return this.Connection;
            }
            set
            {
                this.Connection = (EntityConnection)value;
            }
        }

        /// <summary>
        /// Gets or sets the map connection used by this adapter.
        /// </summary>
        public EntityConnection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the IEntityCache.AcceptChanges should be called during a call to IEntityAdapter.Update.
        /// </summary>
        public bool AcceptChangesDuringUpdate
        {
            get
            {
                return this._acceptChangesDuringUpdate;
            }
            set
            {
                this._acceptChangesDuringUpdate = value;
            }
        }

        /// <summary>
        /// Gets of sets the command timeout for update operations. If null, indicates that the default timeout
        /// for the provider should be used.
        /// </summary>
        Int32? IEntityAdapter.CommandTimeout
        {
            get
            {
                return this._commandTimeout;
            }
            set
            {
                this._commandTimeout = value;
            }
        }

        /// <summary>
        /// Persist modifications described in the given cache.
        /// </summary>
        /// <param name="entityCache">Entity cache containing changes to persist to the store.</param>
        /// <returns>Number of cache entries affected by the udpate.</returns>
        public Int32 Update(IEntityStateManager entityCache)
        {
            EntityUtil.CheckArgumentNull(entityCache, "entityCache");
            if (!IsStateManagerDirty(entityCache)) { return 0; }

            // Check that we have a connection before we proceed
            if (_connection == null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_NoConnectionForAdapter);
            }

            // Check that the store connection is available
            if (_connection.StoreProviderFactory == null || this._connection.StoreConnection == null)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_NoStoreConnectionForUpdate);
            }

            // Check that the connection is open before we proceed
            if (ConnectionState.Open != _connection.State)
            {
                throw EntityUtil.InvalidOperation(System.Data.Entity.Strings.EntityClient_ClosedConnectionForUpdate);
            }

            return UpdateTranslator.Update(entityCache, this);
        }

        /// <summary>
        /// Determine whether the cache has changes to apply.
        /// </summary>
        /// <param name="entityCache">ObjectStateManager to check. Must not be null.</param>
        /// <returns>true if cache contains changes entries; false otherwise</returns>
        private static bool IsStateManagerDirty(IEntityStateManager entityCache)
        {
            Debug.Assert(null != entityCache);
            bool hasChanges = false;
            // this call to GetCacheEntries is constant time (the ObjectStateManager implementation
            // maintains an explicit list of entries in each state)
            foreach (ObjectStateEntry entry in entityCache.GetEntityStateEntries(
                EntityState.Added | EntityState.Deleted | EntityState.Modified))
            {
                hasChanges = true;
                break;
            }
            return hasChanges;
        }
    }
}
