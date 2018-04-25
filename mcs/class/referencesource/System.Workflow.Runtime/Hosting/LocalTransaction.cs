//------------------------------------------------------------------------------
// <copyright file="LocalTransaction.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region Using directives

using System;
using System.Diagnostics;
using System.Transactions;
using System.Data;
using System.Data.Common;
using System.Threading;

#endregion

namespace System.Workflow.Runtime.Hosting
{
    /// <summary>
    /// This class wraps a local transaction (DbTransaction) by implementing IPromotableSinglePhaseNotification:
    /// It instantiate a DbTransaction and never allows promotion to a DTC transaction.
    /// </summary>
    internal sealed class LocalTransaction : IPromotableSinglePhaseNotification
    {
        readonly DbTransaction transaction;
        System.Data.Common.DbConnection connection;
        ManualResetEvent handle;
        object syncRoot = new Object();

        #region Constructor

        /// <summary>
        /// Wraps a local transaction inside
        /// </summary>
        /// <param name="dbHelper"></param>
        internal LocalTransaction(DbResourceAllocator dbHelper, ManualResetEvent handle)
        {
            if (null == handle)
                throw new ArgumentNullException("handle");

            // Open a connection that specifically does not auto-enlist ("Enlist=false" in connection string)
            // to prevent auto-promotion of the transaction
            this.connection = dbHelper.OpenNewConnectionNoEnlist();
            this.transaction = this.connection.BeginTransaction();
            this.handle = handle;
        }

        #endregion Constructor


        #region Accessors

        public System.Data.Common.DbConnection Connection
        {
            get { return this.connection; }
        }

        public DbTransaction Transaction
        {
            get { return this.transaction; }
        }

        #endregion Accessors


        #region IPromotableSinglePhaseNotification Members

        public void Initialize()
        {
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment en)
        {
            if (en == null)
                throw new ArgumentNullException("en");

            //
            // Wait until IPendingWork members have completed (WinOE bugs 17580 and 13395)
            try
            {
                handle.WaitOne();
            }
            catch (ObjectDisposedException)
            {
                // If an ObjectDisposedException is thrown because
                // the WaitHandle has already closed, nothing to worry
                // about. Move on.
            }

            lock (syncRoot)
            {
                try
                {
                    this.transaction.Commit();
                    en.Committed();
                }
                catch (Exception e)
                {
                    en.Aborted(e);
                }
                finally
                {
                    if ((null != connection) && (ConnectionState.Closed != connection.State))
                    {
                        connection.Close();
                        connection = null;
                    }
                }
            }
        }

        public void Rollback(SinglePhaseEnlistment en)
        {
            if (en == null)
                throw new ArgumentNullException("en");

            //
            // Wait until IPendingWork members have completed (WinOE bugs 17580 and 13395)
            try
            {
                handle.WaitOne();
            }
            catch (ObjectDisposedException)
            {
                // If an ObjectDisposedException is thrown because
                // the WaitHandle has already closed, nothing to worry
                // about. Move on.
            }
            // DbTransaction.Dispose will call Rollback if the DbTransaction is not Zombied.
            // Not safe to call DbTransaction.Rollback here as the the underlining 
            // DbTransaction may have already been Rolled back
            lock (syncRoot)
            {
                if (null != transaction)
                    transaction.Dispose();

                if ((null != connection) && (ConnectionState.Closed != connection.State))
                {
                    connection.Close();
                    connection = null;
                }

                en.Aborted();
            }
        }

        public byte[] Promote()
        {
            throw new TransactionPromotionException(
                ExecutionStringManager.PromotionNotSupported);
        }

        #endregion IPromotableSinglePhaseNotification Members
    }
}
