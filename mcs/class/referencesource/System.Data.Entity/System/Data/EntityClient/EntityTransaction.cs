//---------------------------------------------------------------------
// <copyright file="EntityTransaction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace System.Data.EntityClient
{
    using Metadata.Edm;

    /// <summary>
    /// Class representing a transaction for the conceptual layer
    /// </summary>
    public sealed class EntityTransaction : DbTransaction
    {
        private EntityConnection _connection;
        private DbTransaction _storeTransaction;

        /// <summary>
        /// Constructs the EntityTransaction object with an associated connection and the underlying store transaction
        /// </summary>
        /// <param name="connection">The EntityConnetion object owning this transaction</param>
        /// <param name="storeTransaction">The underlying transaction object</param>
        internal EntityTransaction(EntityConnection connection, DbTransaction storeTransaction)
            : base()
        {
            Debug.Assert(connection != null && storeTransaction != null);

            this._connection = connection;
            this._storeTransaction = storeTransaction;
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        public new EntityConnection Connection
        {
            get
            {   // follow the store transaction behavior
                return ((null != _storeTransaction.Connection) ? _connection : null);
            }
        }

        /// <summary>
        /// The connection object owning this transaction object
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {   // follow the store transaction behavior
                return ((null != _storeTransaction.Connection) ? _connection : null);
            }
        }

        /// <summary>
        /// The isolation level of this transaction
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get
            {
                return this._storeTransaction.IsolationLevel;
            }
        }

        /// <summary>
        /// Gets the DbTransaction for the underlying provider transaction
        /// </summary>
        internal DbTransaction StoreTransaction
        {
            get
            {
                return this._storeTransaction;
            }
        }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        public override void Commit()
        {
            try
            {
                this._storeTransaction.Commit();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.Provider(@"Commit", e);
                }
                throw;
            }

            this.ClearCurrentTransaction();
        }

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        public override void Rollback()
        {
            try
            {
                this._storeTransaction.Rollback();
            }
            catch (Exception e)
            {
                if (EntityUtil.IsCatchableExceptionType(e))
                {
                    throw EntityUtil.Provider(@"Rollback", e);
                }
                throw;
            }

            this.ClearCurrentTransaction();
        }

        /// <summary>
        /// Cleans up this transaction object
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearCurrentTransaction();
                this._storeTransaction.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Helper method to wrap EntityConnection.ClearCurrentTransaction()
        /// </summary>
        private void ClearCurrentTransaction()
        {
            if (_connection.CurrentTransaction == this)
            {
                _connection.ClearCurrentTransaction();
            }
        }
    }
}
