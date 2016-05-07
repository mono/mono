//------------------------------------------------------------------------------
// <copyright file="DefaultWorkflowTransactionService.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Transactions;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Configuration;

namespace System.Workflow.Runtime.Hosting
{
    /// <summary> A simple TransactionService that creates
    /// <c>System.Transactions.CommittableTransaction</c>.</summary> 
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class DefaultWorkflowCommitWorkBatchService : WorkflowCommitWorkBatchService
    {
        private bool _enableRetries = false;
        private bool _ignoreCommonEnableRetries = false;

        public DefaultWorkflowCommitWorkBatchService()
        {
        }

        public DefaultWorkflowCommitWorkBatchService(NameValueCollection parameters)
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

        protected internal override void Start()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "DefaultWorkflowCommitWorkBatchService: Starting");

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
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "DefaultWorkflowCommitWorkBatchService: Stopping");

            base.OnStopped();
        }

        internal protected override void CommitWorkBatch(CommitWorkBatchCallback commitWorkBatchCallback)
        {
            DbRetry dbRetry = new DbRetry(_enableRetries);
            short retryCounter = 0;

            while (true)
            {
                if (null != Transaction.Current)
                {
                    //
                    // Can't retry as we don't own the tx
                    // Set the counter to only allow one iteration
                    retryCounter = dbRetry.MaxRetries;
                }
                try
                {
                    base.CommitWorkBatch(commitWorkBatchCallback);

                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "DefaultWorkflowCommitWorkBatchService caught exception from commitWorkBatchCallback: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref retryCounter))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "DefaultWorkflowCommitWorkBatchService retrying commitWorkBatchCallback (retry attempt " + retryCounter.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")");
                        continue;
                    }
                    else
                        throw;
                }
            }
        }
    }
}
