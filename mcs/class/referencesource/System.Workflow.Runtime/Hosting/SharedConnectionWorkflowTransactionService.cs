#pragma warning disable 1634, 1691
//------------------------------------------------------------------------------
// <copyright file="SharedConnectionWorkflowTransactionService.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#region Using directives

using System;
using System.Transactions;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading;
#endregion

namespace System.Workflow.Runtime.Hosting
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SharedConnectionWorkflowCommitWorkBatchService : WorkflowCommitWorkBatchService
    {
        private DbResourceAllocator dbResourceAllocator;
        private IDictionary<Transaction, SharedConnectionInfo> transactionConnectionTable;
        private object tableSyncObject = new object();

        // Saved from constructor input to be used in service start initialization        
        private NameValueCollection configParameters;
        string unvalidatedConnectionString;
        private bool _enableRetries = false;
        private bool _ignoreCommonEnableRetries = false;

        /// <summary>
        /// Enables the adding of this service programmatically.
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="connectionString"></param>
        public SharedConnectionWorkflowCommitWorkBatchService(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);

            this.unvalidatedConnectionString = connectionString;
        }

        /// <summary>
        /// Enables adding of this service from a config file.
        /// Get the connection string from the runtime common parameter section or the particular service parameter section 
        /// of the configuration file, and instantiate a DbResourceAllocator object.
        /// </summary>
        /// <param name="runtime"></param>
        /// <param name="parameters"></param>
        public SharedConnectionWorkflowCommitWorkBatchService(NameValueCollection parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters", ExecutionStringManager.MissingParameters);

            if (parameters.Count > 0)
            {
                foreach (string key in parameters.Keys)
                {
                    if (0 == string.Compare("EnableRetries", key, StringComparison.OrdinalIgnoreCase))
                    {
                        _enableRetries = bool.Parse(parameters[key]);
                        _ignoreCommonEnableRetries = true;
                    }
                }
            }

            this.configParameters = parameters;
        }

        #region Accessors
        internal string ConnectionString
        {
            get
            {
#pragma warning disable 56503
                if (this.dbResourceAllocator == null)
                {
                    // Other hosting services may try to get the connection string during their initialization phase 
                    // (in WorkflowRuntimeService.Start) to dectect string mismatch conflict
                    if (this.Runtime == null)
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowRuntimeNotStarted);
                    this.dbResourceAllocator = new DbResourceAllocator(this.Runtime, this.configParameters, this.unvalidatedConnectionString);
                }
#pragma warning restore 56503
                return this.dbResourceAllocator.ConnectionString;
            }
        }

        public bool EnableRetries
        {
            get { return _enableRetries; }
            set
            {
                _enableRetries = value;
                _ignoreCommonEnableRetries = true;
            }
        }
        #endregion Accessors

        #region WorkflowRuntimeService
        override protected internal void Start()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SharedConnectionWorkflowCommitWorkBatchService: Starting");

            this.dbResourceAllocator = new DbResourceAllocator(this.Runtime, this.configParameters, this.unvalidatedConnectionString);            
            if (this.transactionConnectionTable == null)
                this.transactionConnectionTable = new Dictionary<Transaction, SharedConnectionInfo>();

            //
            // If we didn't find a local value for enable retries
            // check in the common section
            if ((!_ignoreCommonEnableRetries) && (null != base.Runtime))
            {
                NameValueConfigurationCollection commonConfigurationParameters = base.Runtime.CommonParameters;
                if (commonConfigurationParameters != null)
                {
                    // Then scan for connection string in the common configuration parameters section
                    foreach (string key in commonConfigurationParameters.AllKeys)
                    {
                        if (string.Compare("EnableRetries", key, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _enableRetries = bool.Parse(commonConfigurationParameters[key].Value);
                            break;
                        }
                    }
                }
            }

            base.Start();
        }

        protected override void OnStopped()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SharedConnectionWorkflowCommitWorkBatchService: Stopping");
            foreach (KeyValuePair<Transaction, SharedConnectionInfo> kvp in this.transactionConnectionTable)
            {
                kvp.Value.Dispose();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Removing transaction " + kvp.Key.GetHashCode());
            }
            this.transactionConnectionTable.Clear();
            this.dbResourceAllocator = null;
            base.OnStopped();
        }
        #endregion Public Methods

        #region Public Methods

        protected internal override void CommitWorkBatch(WorkflowCommitWorkBatchService.CommitWorkBatchCallback commitWorkBatchCallback)
        {
            //
            // Disable retries by default, reset to allow retries below if we own the tx
            DbRetry dbRetry = new DbRetry(_enableRetries);
            short retryCounter = dbRetry.MaxRetries;

            while (true)
            {
                //
                // When using LocalTransaction handle block access to the connection 
                // in the transaction event handlers until all IPendingWork members have completed
                ManualResetEvent handle = new ManualResetEvent(false);
                Transaction tx = null;
                SharedConnectionInfo connectionInfo = null;

                try
                {
                    if (null == Transaction.Current)
                    {
                        //
                        // It's OK to retry here as we own the tx
                        retryCounter = 0;
                        //
                        // Create a local, non promotable transaction that we share with our OOB services
                        tx = new CommittableTransaction();
                        connectionInfo = new SharedConnectionInfo(this.dbResourceAllocator, tx, false, handle);
                    }
                    else
                    {
                        //
                        // Can't retry as we don't own the tx
                        // Create a dependent transaction and don't restrict promotion.
                        tx = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                        connectionInfo = new SharedConnectionInfo(this.dbResourceAllocator, tx, true, handle);
                    }

                    AddToConnectionInfoTable(tx, connectionInfo);

                    using (TransactionScope ts = new TransactionScope(tx))
                    {
                        try
                        {
                            commitWorkBatchCallback();
                            ts.Complete();
                        }
                        finally
                        {
                            RemoveConnectionFromInfoTable(tx);
                            //
                            // Unblock transaction event handlers
                            handle.Set();
                        }
                    }

                    CommittableTransaction committableTransaction = tx as CommittableTransaction;
                    if (committableTransaction != null)
                        committableTransaction.Commit();

                    DependentTransaction dependentTransaction = tx as DependentTransaction;
                    if (dependentTransaction != null)
                        dependentTransaction.Complete();

                    break;
                }
                catch (Exception e)
                {
                    tx.Rollback();

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SharedConnectionWorkflowCommitWorkBatchService caught exception from commitWorkBatchCallback: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref retryCounter))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SharedConnectionWorkflowCommitWorkBatchService retrying commitWorkBatchCallback (retry attempt " + retryCounter.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                        continue;
                    }
                    else
                        throw;
                }
                finally
                {
                    handle.Close();
                    if (tx != null)
                    {
                        tx.Dispose();
                    }
                }
            }
        }
        /// <summary>
        /// Get the SharedConnectionInfo object from the hashtable keyed by the transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        internal SharedConnectionInfo GetConnectionInfo(Transaction transaction)
        {
            return LookupConnectionInfoTable(transaction);
        }

        #endregion Public Methods

        #region Private Methods

        private void RemoveConnectionFromInfoTable(Transaction transaction)
        {
            lock (this.tableSyncObject)
            {
                SharedConnectionInfo connectionInfo;
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TransactionCompleted " + transaction.GetHashCode());
                if (transactionConnectionTable.TryGetValue(transaction, out connectionInfo))
                {
                    connectionInfo.Dispose();
                    this.transactionConnectionTable.Remove(transaction);
                }
                else
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TransactionCompleted " + transaction.GetHashCode() +
                        " not found in table of count " + this.transactionConnectionTable.Count);                
                }
            } 
        }

        private void AddToConnectionInfoTable(Transaction transaction, SharedConnectionInfo connectionInfo)
        {
            lock (this.tableSyncObject)
            {
                this.transactionConnectionTable.Add(transaction, connectionInfo);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "AddToConnectionInfoTable " + transaction.GetHashCode() +
                        " in table of count " + this.transactionConnectionTable.Count); 
            }
        }

        private SharedConnectionInfo LookupConnectionInfoTable(Transaction transaction)
        {
            lock (this.tableSyncObject)
            {
                return transactionConnectionTable[transaction];
            }
        }

        #endregion Private Methods
    }
}
