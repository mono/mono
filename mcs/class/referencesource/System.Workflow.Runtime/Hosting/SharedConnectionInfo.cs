//------------------------------------------------------------------------------
// <copyright file="SharedConnectionInfo.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region Using directives

using System;
using System.Diagnostics;
using System.Transactions;
using System.Data.Common;
using System.Threading;

#endregion

namespace System.Workflow.Runtime.Hosting
{
    /// <summary>
    /// This class keeps the following associated with a Transaction
    /// - a connection that participates in the transaction.
    /// - an optional local transaction (DbTransaction) generated from the single-phase-committed Transaction.
    /// The connection and the local transaction are passed around to different host components to 
    /// do transacted DB work using the shared connection.
    /// </summary>
    internal sealed class SharedConnectionInfo : IDisposable
    {
        readonly DbConnection connection;
        readonly DbTransaction localTransaction;
        private bool disposed;
        private ManualResetEvent handle;

        #region Constructor

        /// <summary>
        /// Instantiate an opened connection enlisted to the Transaction
        /// if promotable is false, the Transaction wraps a local 
        /// transaction inside and can never be promoted
        /// </summary>
        /// <param name="dbResourceAllocator"></param>
        /// <param name="transaction"></param>
        /// <param name="wantPromotable"></param>
        internal SharedConnectionInfo(
            DbResourceAllocator dbResourceAllocator,
            Transaction transaction,
            bool wantPromotable,
            ManualResetEvent handle)
        {
            Debug.Assert((transaction != null), "Null Transaction!");

            if (null == handle)
                throw new ArgumentNullException("handle");

            this.handle = handle;

            if (wantPromotable)
            {
                // Enlist a newly opened connection to this regular Transaction
                this.connection = dbResourceAllocator.OpenNewConnection();
                this.connection.EnlistTransaction(transaction);
            }
            else
            {
                // Make this transaction no longer promotable by attaching our 
                // IPromotableSinglePhaseNotification implementation (LocalTranscaction)
                // and track the DbConnection and DbTransaction associated with the LocalTranscaction
                LocalTransaction localTransaction = new LocalTransaction(dbResourceAllocator, handle);
                transaction.EnlistPromotableSinglePhase(localTransaction);
                this.connection = localTransaction.Connection;
                this.localTransaction = localTransaction.Transaction;
            }
        }

        #endregion Constructor

        #region Accessors

        internal DbConnection DBConnection
        {
            get { return this.connection; }
        }

        internal DbTransaction DBTransaction
        {
            get { return this.localTransaction; }
        }

        #endregion Accessors

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                //
                // If we're using a LocalTransaction it will close the connection
                // in it's IPromotableSinglePhaseNotification methods
                if ((this.localTransaction == null) && (null != connection))
                    this.connection.Dispose();
            }
            this.disposed = true;
        }

        #endregion
    }
}
