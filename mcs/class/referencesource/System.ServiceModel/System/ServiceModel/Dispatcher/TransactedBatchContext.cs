//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;

    sealed class TransactedBatchContext : IEnlistmentNotification
    {
        SharedTransactedBatchContext shared;
        CommittableTransaction transaction;
        DateTime commitNotLaterThan;
        int commits;
        bool batchFinished;
        bool inDispatch;

        internal TransactedBatchContext(SharedTransactedBatchContext shared)
        {
            this.shared = shared;
            this.transaction = TransactionBehavior.CreateTransaction(shared.IsolationLevel, shared.TransactionTimeout);
            this.transaction.EnlistVolatile(this, EnlistmentOptions.None);
            if (shared.TransactionTimeout <= TimeSpan.Zero)
                this.commitNotLaterThan = DateTime.MaxValue;
            else
                this.commitNotLaterThan = DateTime.UtcNow + TimeSpan.FromMilliseconds(shared.TransactionTimeout.TotalMilliseconds * 4 / 5);
            this.commits = 0;
            this.batchFinished = false;
            this.inDispatch = false;
        }

        internal bool AboutToExpire
        {
            get
            {
                return DateTime.UtcNow > this.commitNotLaterThan;
            }
        }

        internal bool IsActive
        {
            get
            {
                if (this.batchFinished)
                    return false;

                try
                {
                    return TransactionStatus.Active == this.transaction.TransactionInformation.Status;
                }
                catch (ObjectDisposedException ex)
                {
                    MsmqDiagnostics.ExpectedException(ex);
                    return false;
                }
            }
        }

        internal bool InDispatch
        {
            get { return this.inDispatch; }
            set
            {
                if (this.inDispatch == value)
                {
                    Fx.Assert("System.ServiceModel.Dispatcher.ChannelHandler.TransactedBatchContext.InDispatch: (inDispatch == value)");
                }
                this.inDispatch = value;
                if (this.inDispatch)
                    this.shared.DispatchStarted();
                else
                    this.shared.DispatchEnded();
            }
        }

        internal SharedTransactedBatchContext Shared
        {
            get { return this.shared; }
        }

        internal void ForceRollback()
        {
            try
            {
                this.transaction.Rollback();
            }
            catch (ObjectDisposedException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }
            catch (TransactionException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }

            this.batchFinished = true;
        }

        internal void ForceCommit()
        {
            try
            {
                this.transaction.Commit();
            }
            catch (ObjectDisposedException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }
            catch (TransactionException ex)
            {
                MsmqDiagnostics.ExpectedException(ex);
            }

            this.batchFinished = true;
        }

        internal void Complete()
        {
            ++this.commits;

            if (this.commits >= this.shared.CurrentBatchSize || DateTime.UtcNow >= this.commitNotLaterThan)
            {
                ForceCommit();
            }
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            this.shared.ReportCommit();
            this.shared.BatchDone();
            enlistment.Done();
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            this.shared.ReportAbort();
            this.shared.BatchDone();
            enlistment.Done();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            this.shared.ReportAbort();
            this.shared.BatchDone();
            enlistment.Done();
        }

        internal Transaction Transaction
        {
            get { return this.transaction; }
        }
    }

    sealed class SharedTransactedBatchContext
    {
        readonly int maxBatchSize;
        readonly int maxConcurrentBatches;
        readonly IsolationLevel isolationLevel;
        readonly TimeSpan txTimeout;
        int currentBatchSize;
        int currentConcurrentBatches;
        int currentConcurrentDispatches;
        int successfullCommits;
        object receiveLock = new object();
        object thisLock = new object();
        bool isBatching;
        ChannelHandler handler;

        internal SharedTransactedBatchContext(ChannelHandler handler, ChannelDispatcher dispatcher, int maxConcurrentBatches)
        {
            this.handler = handler;
            this.maxBatchSize = dispatcher.MaxTransactedBatchSize;
            this.maxConcurrentBatches = maxConcurrentBatches;
            this.currentBatchSize = dispatcher.MaxTransactedBatchSize;
            this.currentConcurrentBatches = 0;
            this.currentConcurrentDispatches = 0;
            this.successfullCommits = 0;
            this.isBatching = true;
            this.isolationLevel = dispatcher.TransactionIsolationLevel;
            this.txTimeout = TransactionBehavior.NormalizeTimeout(dispatcher.TransactionTimeout);
            BatchingStateChanged(this.isBatching);
        }

        internal TransactedBatchContext CreateTransactedBatchContext()
        {
            lock (thisLock)
            {
                TransactedBatchContext context = new TransactedBatchContext(this);
                ++this.currentConcurrentBatches;
                return context;
            }
        }

        internal void DispatchStarted()
        {
            lock (thisLock)
            {
                ++this.currentConcurrentDispatches;
                if (this.currentConcurrentDispatches == this.currentConcurrentBatches && this.currentConcurrentBatches < this.maxConcurrentBatches)
                {
                    TransactedBatchContext context = new TransactedBatchContext(this);
                    ++this.currentConcurrentBatches;
                    ChannelHandler newHandler = new ChannelHandler(this.handler, context);
                    ChannelHandler.Register(newHandler);
                }
            }
        }

        internal void DispatchEnded()
        {
            lock (thisLock)
            {
                --this.currentConcurrentDispatches;
                if (this.currentConcurrentDispatches < 0)
                {
                    Fx.Assert("System.ServiceModel.Dispatcher.ChannelHandler.SharedTransactedBatchContext.BatchDone: (currentConcurrentDispatches < 0)");
                }
            }
        }

        internal void BatchDone()
        {
            lock (thisLock)
            {
                --this.currentConcurrentBatches;
                if (this.currentConcurrentBatches < 0)
                {
                    Fx.Assert("System.ServiceModel.Dispatcher.ChannelHandler.SharedTransactedBatchContext.BatchDone: (currentConcurrentBatches < 0)");
                }
            }
        }

        internal int CurrentBatchSize
        {
            get
            {
                lock (thisLock)
                {
                    return this.currentBatchSize;
                }
            }
        }

        internal IsolationLevel IsolationLevel
        {
            get
            {
                return this.isolationLevel;
            }
        }

        internal TimeSpan TransactionTimeout
        {
            get
            {
                return this.txTimeout;
            }
        }

        internal void ReportAbort()
        {
            lock (thisLock)
            {
                if (isBatching)
                {
                    this.successfullCommits = 0;
                    this.currentBatchSize = 1;
                    this.isBatching = false;
                    BatchingStateChanged(this.isBatching);
                }
            }
        }

        internal void ReportCommit()
        {
            lock (thisLock)
            {
                if (++this.successfullCommits >= this.maxBatchSize * 2)
                {
                    this.successfullCommits = 0;
                    if (!isBatching)
                    {
                        this.currentBatchSize = this.maxBatchSize;
                        this.isBatching = true;
                        BatchingStateChanged(this.isBatching);
                    }
                }
            }
        }

        void BatchingStateChanged(bool batchingNow)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(
                    TraceEventType.Verbose,
                    batchingNow ? TraceCode.MsmqEnteredBatch : TraceCode.MsmqLeftBatch,
                    batchingNow ? SR.GetString(SR.TraceCodeMsmqEnteredBatch) : SR.GetString(SR.TraceCodeMsmqLeftBatch),
                    null,
                    null,
                    null);

            }
        }

        internal object ReceiveLock
        {
            get { return this.receiveLock; }
        }
    }
}
