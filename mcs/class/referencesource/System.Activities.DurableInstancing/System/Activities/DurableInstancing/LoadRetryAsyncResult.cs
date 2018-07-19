//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;

    class LoadRetryAsyncResult : AsyncResult
    {
        static AsyncCallback onTryCommandCallback = Fx.ThunkCallback(new AsyncCallback(OnTryCommandCallback));
        bool commandSuccess;
        TimeoutHelper commandTimeout;
        InstanceLockedException lastInstanceLockedException;

        int retryCount;

        public LoadRetryAsyncResult(SqlWorkflowInstanceStore store, InstancePersistenceContext context,
            InstancePersistenceCommand command, TimeSpan timeout, AsyncCallback callback, object state)
            : base(callback, state)
        {
            this.InstanceStore = store;
            this.InstancePersistenceContext = context;
            this.InstancePersistenceCommand = command;
            this.commandTimeout = new TimeoutHelper(timeout);

            InstanceStore.BeginTryCommandInternal(this.InstancePersistenceContext, this.InstancePersistenceCommand,
                this.commandTimeout.RemainingTime(), LoadRetryAsyncResult.onTryCommandCallback, this);
        }

        public SqlWorkflowInstanceStore InstanceStore 
        { 
            get; 
            private set; 
        }

        public TimeSpan RetryTimeout 
        { 
            get; 
            private set; 
        }

        InstancePersistenceCommand InstancePersistenceCommand 
        { 
            get; 
            set; 
        }

        InstancePersistenceContext InstancePersistenceContext 
        { 
            get; 
            set; 
        }

        public static bool End(IAsyncResult result)
        {
            LoadRetryAsyncResult thisPtr = AsyncResult.End<LoadRetryAsyncResult>(result);
            return thisPtr.commandSuccess;
        }

        public void AbortRetry()
        {
            Fx.Assert(this.lastInstanceLockedException != null, "no last instance lock exception");
            this.Complete(false, this.lastInstanceLockedException);
        }

        public void Retry()
        {
            InstanceStore.BeginTryCommandInternal(this.InstancePersistenceContext, this.InstancePersistenceCommand,
                this.commandTimeout.RemainingTime(), LoadRetryAsyncResult.onTryCommandCallback, this);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes, 
            Justification = "Standard AsyncResult callback pattern.")]
        static void OnTryCommandCallback(IAsyncResult result)
        {
            LoadRetryAsyncResult tryCommandAsyncResult = (LoadRetryAsyncResult)(result.AsyncState);
            Exception completeException = null;
            bool completeFlag = true;

            try
            {
                tryCommandAsyncResult.CompleteTryCommand(result);
            }
            catch (InstanceLockedException instanceLockedException)
            {
                TimeSpan retryDelay = tryCommandAsyncResult.InstanceStore.GetNextRetryDelay(++tryCommandAsyncResult.retryCount);

                if (retryDelay < tryCommandAsyncResult.commandTimeout.RemainingTime())
                {
                    tryCommandAsyncResult.RetryTimeout = retryDelay;

                    if (tryCommandAsyncResult.InstanceStore.EnqueueRetry(tryCommandAsyncResult))
                    {
                        tryCommandAsyncResult.lastInstanceLockedException = instanceLockedException;
                        completeFlag = false;                        
                    }
                }
                else if (TD.LockRetryTimeoutIsEnabled())
                {
                    TD.LockRetryTimeout(tryCommandAsyncResult.InstancePersistenceContext.EventTraceActivity, tryCommandAsyncResult.commandTimeout.OriginalTimeout.ToString());
                }

                if (completeFlag)
                {
                    completeException = instanceLockedException;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                completeException = exception;
            }

            if (completeFlag)
            {
                tryCommandAsyncResult.Complete(false, completeException);
            }
        }

        void CompleteTryCommand(IAsyncResult result)
        {
            this.commandSuccess = this.InstanceStore.EndTryCommand(result);
        }
    }

}
