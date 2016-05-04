//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.Transactions;

    class TransactionBehavior
    {
        bool isConcurrent;
        IsolationLevel isolation = ServiceBehaviorAttribute.DefaultIsolationLevel;
        DispatchRuntime dispatch;
        TimeSpan timeout = TimeSpan.Zero;
        bool isTransactedReceiveChannelDispatcher = false;

        internal TransactionBehavior()
        {
        }

        internal TransactionBehavior(DispatchRuntime dispatch)
        {
            this.isConcurrent = (dispatch.ConcurrencyMode == ConcurrencyMode.Multiple ||
                dispatch.ConcurrencyMode == ConcurrencyMode.Reentrant);

            this.dispatch = dispatch;
            this.isTransactedReceiveChannelDispatcher = dispatch.ChannelDispatcher.IsTransactedReceive;

            // Don't pull in System.Transactions.dll if we don't need it
            if (dispatch.ChannelDispatcher.TransactionIsolationLevelSet)
            {
                this.InitializeIsolationLevel(dispatch);
            }

            this.timeout = TransactionBehavior.NormalizeTimeout(dispatch.ChannelDispatcher.TransactionTimeout);
        }

        internal static Exception CreateFault(string reasonText, string codeString, bool isNetDispatcherFault)
        {
            string faultCodeNamespace, action;

            // 'Transactions' action should be used only when we expect to have a TransactionChannel in the channel stack
            // otherwise one should use the NetDispatch action.
            if (isNetDispatcherFault)
            {
                faultCodeNamespace = FaultCodeConstants.Namespaces.NetDispatch;
                action = FaultCodeConstants.Actions.NetDispatcher;
            }
            else
            {
                faultCodeNamespace = FaultCodeConstants.Namespaces.Transactions;
                action = FaultCodeConstants.Actions.Transactions;
            }

            FaultReason reason = new FaultReason(reasonText, CultureInfo.CurrentCulture);
            FaultCode code = FaultCode.CreateSenderFaultCode(codeString, faultCodeNamespace);
            return new FaultException(reason, code, action);
        }

        internal static TransactionBehavior CreateIfNeeded(DispatchRuntime dispatch)
        {
            if (TransactionBehavior.NeedsTransactionBehavior(dispatch))
            {
                return new TransactionBehavior(dispatch);
            }
            else
            {
                return null;
            }
        }

        internal static TimeSpan NormalizeTimeout(TimeSpan timeout)
        {
            if (TimeSpan.Zero == timeout)
            {
                timeout = TransactionManager.DefaultTimeout;
            }
            else if (TimeSpan.Zero != TransactionManager.MaximumTimeout && timeout > TransactionManager.MaximumTimeout)
            {
                timeout = TransactionManager.MaximumTimeout;
            }
            return timeout;
        }

        internal static CommittableTransaction CreateTransaction(IsolationLevel isolation, TimeSpan timeout)
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = isolation;
            options.Timeout = timeout;

            return new CommittableTransaction(options);
        }

        internal void SetCurrent(ref MessageRpc rpc)
        {
            if (!this.isConcurrent)
            {
                rpc.InstanceContext.Transaction.SetCurrent(ref rpc);
            }
        }

        internal void ResolveOutcome(ref MessageRpc rpc)
        {
            if ((rpc.InstanceContext != null) && (rpc.transaction != null))
            {
                TransactionInstanceContextFacet context = rpc.InstanceContext.Transaction;

                if (context != null)
                {
                    context.CheckIfTxCompletedAndUpdateAttached(ref rpc, this.isConcurrent);
                }

                rpc.Transaction.Complete(rpc.Error);
            }
        }

        Transaction GetInstanceContextTransaction(ref MessageRpc rpc)
        {
            return rpc.InstanceContext.Transaction.Attached;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InitializeIsolationLevel(DispatchRuntime dispatch)
        {
            this.isolation = dispatch.ChannelDispatcher.TransactionIsolationLevel;
        }

        static bool NeedsTransactionBehavior(DispatchRuntime dispatch)
        {
            DispatchOperation unhandled = dispatch.UnhandledDispatchOperation;
            if ((unhandled != null) && (unhandled.TransactionRequired))
            {
                return true;
            }

            if (dispatch.ChannelDispatcher.IsTransactedReceive) //check if we have transacted receive
            {
                return true;
            }

            for (int i = 0; i < dispatch.Operations.Count; i++)
            {
                DispatchOperation operation = dispatch.Operations[i];
                if (operation.TransactionRequired)
                {
                    return true;
                }
            }

            return false;
        }

        internal void ResolveTransaction(ref MessageRpc rpc)
        {
            if (rpc.Operation.HasDefaultUnhandledActionInvoker)
            {
                // we ignore unhandled operations 
                return;
            }

            Transaction contextTransaction = null;
            //If we are inside a TransactedReceiveScope in workflow, then we need to look into the PPD and not the InstanceContext
            //to get the contextTransaction
            if (rpc.Operation.IsInsideTransactedReceiveScope)
            {
                // We may want to use an existing transaction for the instance.
                IInstanceTransaction instanceTransaction = rpc.Operation.Invoker as IInstanceTransaction;
                if (instanceTransaction != null)
                {
                    contextTransaction = instanceTransaction.GetTransactionForInstance(rpc.OperationContext);
                }

                if (contextTransaction != null)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.TxSourceTxScopeRequiredUsingExistingTransaction,
                            SR.GetString(SR.TraceCodeTxSourceTxScopeRequiredUsingExistingTransaction,
                            contextTransaction.TransactionInformation.LocalIdentifier,
                            rpc.Operation.Name)
                            );
                    }
                }
            }
            else
            {
                contextTransaction = this.GetInstanceContextTransaction(ref rpc);
            }

            Transaction transaction = null;

            try
            {
                transaction = TransactionMessageProperty.TryGetTransaction(rpc.Request);
            }
            catch (TransactionException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionUnmarshalFailed, e.Message), FaultCodeConstants.Codes.TransactionUnmarshalingFailed, false));
            }

            if (rpc.Operation.TransactionRequired)
            {
                if (transaction != null)
                {
                    if (this.isTransactedReceiveChannelDispatcher)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxSourceTxScopeRequiredIsTransactedTransport,
                                SR.GetString(SR.TraceCodeTxSourceTxScopeRequiredIsTransactedTransport,
                                transaction.TransactionInformation.LocalIdentifier,
                                rpc.Operation.Name)
                                );
                        }
                    }
                    else
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxSourceTxScopeRequiredIsTransactionFlow,
                                SR.GetString(SR.TraceCodeTxSourceTxScopeRequiredIsTransactionFlow,
                                transaction.TransactionInformation.LocalIdentifier,
                                rpc.Operation.Name)
                                );
                        }

                        if (PerformanceCounters.PerformanceCountersEnabled)
                        {
                            PerformanceCounters.TxFlowed(PerformanceCounters.GetEndpointDispatcher(), rpc.Operation.Name);
                        }

                        bool sameTransaction = false;
                        if (rpc.Operation.IsInsideTransactedReceiveScope)
                        {
                            sameTransaction = transaction.Equals(contextTransaction);
                        }
                        else
                        {
                            sameTransaction = transaction == contextTransaction;
                        }

                        if (!sameTransaction)
                        {
                            try
                            {
                                transaction = transaction.DependentClone(DependentCloneOption.RollbackIfNotComplete);
                            }
                            catch (TransactionException e)
                            {
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true));
                            }
                        }
                    }
                }
            }
            else
            {
                // We got a transaction from the ChannelHandler.
                // Transport is transacted.
                // But operation doesn't require the transaction, so no one ever will commit it.
                // Because of that we have to commit it here.
                if (transaction != null && this.isTransactedReceiveChannelDispatcher)
                {
                    try
                    {
                        if (null != rpc.TransactedBatchContext)
                        {
                            rpc.TransactedBatchContext.ForceCommit();
                            rpc.TransactedBatchContext = null;
                        }
                        else
                        {
                            TransactionInstanceContextFacet.Complete(transaction, null);
                        }
                    }
                    finally
                    {
                        transaction.Dispose();
                        transaction = null;
                    }
                }
            }

            InstanceContext context = rpc.InstanceContext;

            if (context.Transaction.ShouldReleaseInstance && !this.isConcurrent)
            {
                if (context.Behavior.ReleaseServiceInstanceOnTransactionComplete)
                {
                    context.ReleaseServiceInstance();
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information,
                            TraceCode.TxReleaseServiceInstanceOnCompletion,
                            SR.GetString(SR.TraceCodeTxReleaseServiceInstanceOnCompletion,
                            contextTransaction.TransactionInformation.LocalIdentifier)
                            );
                    }
                }

                context.Transaction.ShouldReleaseInstance = false;

                if (transaction == null || transaction == contextTransaction)
                {
                    rpc.Transaction.Current = contextTransaction;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true));
                }
                else
                {
                    contextTransaction = null;
                }
            }

            if (rpc.Operation.TransactionRequired)
            {
                if (transaction == null)
                {
                    if (contextTransaction != null)
                    {
                        transaction = contextTransaction;
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxSourceTxScopeRequiredIsAttachedTransaction,
                                SR.GetString(SR.TraceCodeTxSourceTxScopeRequiredIsAttachedTransaction,
                                transaction.TransactionInformation.LocalIdentifier,
                                rpc.Operation.Name)
                                );
                        }
                    }
                    else
                    {
                        transaction = TransactionBehavior.CreateTransaction(this.isolation, this.timeout);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxSourceTxScopeRequiredIsCreateNewTransaction,
                                SR.GetString(SR.TraceCodeTxSourceTxScopeRequiredIsCreateNewTransaction,
                                transaction.TransactionInformation.LocalIdentifier,
                                rpc.Operation.Name)
                                );
                        }
                    }
                }

                if ((this.isolation != IsolationLevel.Unspecified) && (transaction.IsolationLevel != this.isolation))
                {
                    throw TraceUtility.ThrowHelperError(TransactionBehavior.CreateFault
                        (SR.GetString(SR.IsolationLevelMismatch2, transaction.IsolationLevel, this.isolation), FaultCodeConstants.Codes.TransactionIsolationLevelMismatch, false), rpc.Request);
                }

                rpc.Transaction.Current = transaction;
                rpc.InstanceContext.Transaction.AddReference(ref rpc, rpc.Transaction.Current, true);

                try
                {
                    rpc.Transaction.Clone = transaction.Clone();
                    if (rpc.Operation.IsInsideTransactedReceiveScope)
                    {
                        //It is because we want to synchronize the dispatcher processing of messages with the commit 
                        //processing that is started by the completion of a TransactedReceiveScope. We need to make sure 
                        //that all the dispatcher processing is done and we can do that by creating a blocking dependent clone and only 
                        //completing it after all of the message processing is done for a given TransactionRpcFacet
                        rpc.Transaction.CreateDependentClone();
                    }
                }
                catch (ObjectDisposedException e)//transaction may be async aborted
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true));
                }

                rpc.InstanceContext.Transaction.AddReference(ref rpc, rpc.Transaction.Clone, false);

                rpc.OperationContext.TransactionFacet = rpc.Transaction;

                if (!rpc.Operation.TransactionAutoComplete)
                {
                    rpc.Transaction.SetIncomplete();
                }
            }
        }

        internal void InitializeCallContext(ref MessageRpc rpc)
        {
            if (rpc.Operation.TransactionRequired)
            {
                rpc.Transaction.ThreadEnter(ref rpc.Error);
            }
        }

        internal void ClearCallContext(ref MessageRpc rpc)
        {
            if (rpc.Operation.TransactionRequired)
            {
                rpc.Transaction.ThreadLeave();
            }
        }
    }

    internal class TransactionRpcFacet
    {

        //internal members
        // Current is the original transaction that we created/flowed/whatever.  This is
        // the "current" transaction used by the operation, and we keep it around so we
        // can commit it, complete it, etc.
        //
        // Clone is a clone of Current.  We keep it around to pass into TransactionScope
        // so that System.Transactions.Transaction.Current is not CommittableTransaction
        // or anything dangerous like that.
        internal Transaction Current;
        internal Transaction Clone;
        internal DependentTransaction dependentClone;
        internal bool IsCompleted = true;
        internal MessageRpc rpc;

        TransactionScope scope;
        bool transactionSetComplete = false; // To track if user has called SetTransactionComplete()

        internal TransactionRpcFacet()
        {
        }

        internal TransactionRpcFacet(ref MessageRpc rpc)
        {
            this.rpc = rpc;
        }

        // Calling Complete will Commit or Abort the transaction based on, 
        // error - If any user error is propagated to the service we abort the transaction unless SetTransactionComplete was successful.
        // transactionDoomed - If internal error occurred and this error may or may not be propagated 
        //                                 by the user to the service. Abort the Tx if transactionDoomed is set true. 
        //
        // If the user violates the following rules, the transaction is doomed.
        // User cannot call TransactionSetComplete() when TransactionAutoComplete is true.
        // User cannot call TransactionSetComplete() multiple times.

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void Complete(Exception error)
        {
            if (!object.ReferenceEquals(this.Current, null))
            {
                TransactedBatchContext batchContext = this.rpc.TransactedBatchContext;
                if (null != batchContext)
                {
                    if (null == error)
                    {
                        batchContext.Complete();
                    }
                    else
                    {
                        batchContext.ForceRollback();
                    }
                    batchContext.InDispatch = false;
                }
                else
                {
                    if (this.transactionSetComplete)
                    {
                        // Commit the transaction when TransactionSetComplete() is called and 
                        // even when an exception(non transactional) happens after this call. 
                        rpc.InstanceContext.Transaction.CompletePendingTransaction(this.Current, null);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxCompletionStatusCompletedForSetComplete,
                                SR.GetString(SR.TraceCodeTxCompletionStatusCompletedForSetComplete,
                                this.Current.TransactionInformation.LocalIdentifier,
                                this.rpc.Operation.Name)
                                );
                        }
                    }
                    else if (this.IsCompleted || (error != null))
                    {
                        rpc.InstanceContext.Transaction.CompletePendingTransaction(this.Current, error);
                    }
                }
                if (this.rpc.Operation.IsInsideTransactedReceiveScope)
                {
                    //We are done with the message processing associated with this TransactionRpcFacet so a commit that may have 
                    //been started by a TransactedReceiveScope can move forward.
                    this.CompleteDependentClone();
                }
                this.Current = null;
            }
        }

        internal void SetIncomplete()
        {
            this.IsCompleted = false;
        }

        internal void Completed()
        {
            if (this.scope == null)
            {
                return;
            }

            // Prohibit user from calling SetTransactionComplete() when TransactionAutoComplete is set to true.
            // Transaction will be aborted.
            if (this.rpc.Operation.TransactionAutoComplete)
            {
                try
                {
                    this.Current.Rollback();
                }
                catch (ObjectDisposedException e)
                {
                    //we don't want to mask the real error here 
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);

                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxTransactionInvalidSetTransactionComplete, rpc.Operation.Name, rpc.Host.Description.Name)));

            }
            // Prohibit user from calling SetTransactionComplete() multiple times.
            // Transaction will be aborted.
            else if (this.transactionSetComplete)
            {
                try
                {
                    this.Current.Rollback();
                }
                catch (ObjectDisposedException e)
                {
                    //we don't want to mask the real error here
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxMultiSetTransactionComplete, rpc.Operation.Name, rpc.Host.Description.Name)));

            }

            this.transactionSetComplete = true;
            this.IsCompleted = true;
            this.scope.Complete();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ThreadEnter(ref Exception error)
        {
            Transaction clone = this.Clone;

            if ((clone != null) && (error == null))
            {

                if (TD.TransactionScopeCreateIsEnabled())
                {
                    if (clone != null && clone.TransactionInformation != null)
                    {
                        TD.TransactionScopeCreate(rpc.EventTraceActivity,
                            clone.TransactionInformation.LocalIdentifier,
                            clone.TransactionInformation.DistributedIdentifier);
                    }
                }

                this.scope = this.rpc.InstanceContext.Transaction.CreateTransactionScope(clone);
                this.transactionSetComplete = false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ThreadLeave()
        {
            if (this.scope != null)
            {
                if (!this.transactionSetComplete)
                {
                    this.scope.Complete();
                }

                try
                {
                    this.scope.Dispose();
                    this.scope = null;
                }
                catch (TransactionException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true));
                }
            }
        }
        internal void CreateDependentClone()
        {
            if ((this.dependentClone == null) && (this.Clone != null))
            {
                this.dependentClone = this.Clone.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
            }
        }

        internal void CompleteDependentClone()
        {
            if (this.dependentClone != null)
            {
                this.dependentClone.Complete();
            }
        }
    }

    internal sealed class TransactionInstanceContextFacet
    {
        internal Transaction waiting; // waiting to become Single because Single is on his way out.
        internal Transaction Attached;

        IResumeMessageRpc paused; // the IResumeMessageRpc for this.waiting.
        object mutex;
        Transaction current; // the one true transaction when Concurrency=false.
        InstanceContext instanceContext;
        Dictionary<Transaction, RemoveReferenceRM> pending; // When Concurrency=true, all the still pending guys.
        bool shouldReleaseInstance = false;

        internal TransactionInstanceContextFacet(InstanceContext instanceContext)
        {
            this.instanceContext = instanceContext;
            this.mutex = instanceContext.ThisLock;
        }

        // ........................................................................................................
        // no need to lock the following property because it's used only if Concurrency = false
        internal bool ShouldReleaseInstance
        {
            get
            {
                return this.shouldReleaseInstance;
            }
            set
            {
                this.shouldReleaseInstance = value;
            }
        }


        // ........................................................................................................
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void CheckIfTxCompletedAndUpdateAttached(ref MessageRpc rpc, bool isConcurrent)
        {
            if (rpc.Transaction.Current == null)
            {
                return;
            }

            lock (this.mutex)
            {
                if (!isConcurrent)
                {
                    if (this.shouldReleaseInstance)
                    {
                        this.shouldReleaseInstance = false;
                        if (rpc.Error == null) //we don't want to mask the initial error
                        {
                            rpc.Error = TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true);
                            DiagnosticUtility.TraceHandledException(rpc.Error, TraceEventType.Error);
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information,
                                    TraceCode.TxCompletionStatusCompletedForAsyncAbort,
                                    SR.GetString(SR.TraceCodeTxCompletionStatusCompletedForAsyncAbort,
                                    rpc.Transaction.Current.TransactionInformation.LocalIdentifier,
                                    rpc.Operation.Name)
                                    );
                            }
                        }
                    }

                    if (rpc.Transaction.IsCompleted || (rpc.Error != null))
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            if (rpc.Error != null)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information,
                                    TraceCode.TxCompletionStatusCompletedForError,
                                    SR.GetString(SR.TraceCodeTxCompletionStatusCompletedForError,
                                    rpc.Transaction.Current.TransactionInformation.LocalIdentifier,
                                    rpc.Operation.Name)
                                    );
                            }
                            else
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information,
                                    TraceCode.TxCompletionStatusCompletedForAutocomplete,
                                    SR.GetString(SR.TraceCodeTxCompletionStatusCompletedForAutocomplete,
                                    rpc.Transaction.Current.TransactionInformation.LocalIdentifier,
                                    rpc.Operation.Name)
                                    );
                            }

                        }

                        this.Attached = null;

                        if (!(waiting == null))
                        {
                            // tx processing requires failfast when state is inconsistent
                            DiagnosticUtility.FailFast("waiting should be null when resetting current");
                        }

                        this.current = null;
                    }
                    else
                    {
                        this.Attached = rpc.Transaction.Current;
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxCompletionStatusRemainsAttached,
                                SR.GetString(SR.TraceCodeTxCompletionStatusRemainsAttached,
                                rpc.Transaction.Current.TransactionInformation.LocalIdentifier,
                                rpc.Operation.Name)
                                );
                        }
                    }
                }
                else if (!this.pending.ContainsKey(rpc.Transaction.Current))
                {
                    //transaction has been asynchronously aborted
                    if (rpc.Error == null) //we don't want to mask the initial error
                    {
                        rpc.Error = TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true);
                        DiagnosticUtility.TraceHandledException(rpc.Error, TraceEventType.Error);
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information,
                                TraceCode.TxCompletionStatusCompletedForAsyncAbort,
                                SR.GetString(SR.TraceCodeTxCompletionStatusCompletedForAsyncAbort,
                                rpc.Transaction.Current.TransactionInformation.LocalIdentifier,
                                rpc.Operation.Name)
                                );
                        }
                    }
                }
            }
        }


        // ........................................................................................................
        internal void CompletePendingTransaction(Transaction transaction, Exception error)
        {
            lock (this.mutex)
            {
                if (this.pending.ContainsKey(transaction))
                {
                    Complete(transaction, error);
                }
            }
        }

        // ........................................................................................................
        internal static void Complete(Transaction transaction, Exception error)
        {
            try
            {
                if (error == null)
                {
                    CommittableTransaction commit = (transaction as CommittableTransaction);
                    if (commit != null)
                    {
                        commit.Commit();
                    }
                    else
                    {
                        DependentTransaction complete = (transaction as DependentTransaction);
                        if (complete != null)
                        {
                            complete.Complete();
                        }
                    }
                }
                else
                {
                    transaction.Rollback();
                }

            }
            catch (TransactionException e)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true));
            }
        }

        // ........................................................................................................
        internal TransactionScope CreateTransactionScope(Transaction transaction)
        {

            lock (this.mutex)
            {
                if (this.pending.ContainsKey(transaction))
                {
                    try
                    {
                        return new TransactionScope(transaction);
                    }
                    catch (TransactionException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Error);
                        //we'll rethrow below
                    }
                }
            }

            //the transaction was asynchronously aborted
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(TransactionBehavior.CreateFault(SR.GetString(SR.SFxTransactionAsyncAborted), FaultCodeConstants.Codes.TransactionAborted, true));
        }

        // ........................................................................................................

        internal void SetCurrent(ref MessageRpc rpc)
        {
            Transaction requestTransaction = rpc.Transaction.Current;

            if (!(requestTransaction != null))
            {
                // tx processing requires failfast when state is inconsistent
                DiagnosticUtility.FailFast("we should never get here with a requestTransaction null");
            }

            lock (this.mutex)
            {
                if (this.current == null)
                {
                    this.current = requestTransaction;
                }
                else if (this.current != requestTransaction)
                {
                    this.waiting = requestTransaction;
                    this.paused = rpc.Pause();
                }
                else
                {
                    rpc.Transaction.Current = this.current; //rpc.Transaction.Current should get the dependent clone
                }
            }
        }

        // ........................................................................................................

        internal void AddReference(ref MessageRpc rpc, Transaction tx, bool updateCallCount)
        {
            lock (this.mutex)
            {
                if (this.pending == null)
                {
                    this.pending = new Dictionary<Transaction, RemoveReferenceRM>();
                }

                if (tx != null)
                {
                    if (this.pending == null)
                    {
                        this.pending = new Dictionary<Transaction, RemoveReferenceRM>();
                    }

                    RemoveReferenceRM rm;
                    if (!this.pending.TryGetValue(tx, out rm))
                    {
                        RemoveReferenceRM rrm = new RemoveReferenceRM(this.instanceContext, tx, rpc.Operation.Name);
                        rrm.CallCount = 1;
                        this.pending.Add(tx, rrm);
                    }
                    else if (updateCallCount)
                    {
                        rm.CallCount += 1;
                    }
                }
            }
        }

        internal void RemoveReference(Transaction tx)
        {
            lock (this.mutex)
            {
                if (tx.Equals(this.current))
                {
                    if (this.waiting != null)
                    {
                        this.current = waiting;
                        this.waiting = null;

                        if (instanceContext.Behavior.ReleaseServiceInstanceOnTransactionComplete)
                        {
                            instanceContext.ReleaseServiceInstance();
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information,
                                    TraceCode.TxReleaseServiceInstanceOnCompletion,
                                    SR.GetString(SR.TraceCodeTxReleaseServiceInstanceOnCompletion,
                                    tx.TransactionInformation.LocalIdentifier)
                                    );
                            }
                        }

                        bool alreadyResumedNoLock;
                        this.paused.Resume(out alreadyResumedNoLock);
                        if (alreadyResumedNoLock)
                        {
                            Fx.Assert("TransactionBehavior resumed more than once for same call.");
                        }

                    }
                    else
                    {
                        this.shouldReleaseInstance = true;
                        this.current = null;
                    }

                }

                if (this.pending != null)
                {
                    if (this.pending.ContainsKey(tx))
                    {
                        this.pending.Remove(tx);
                    }
                }
            }
        }

        // ........................................................................................................



        abstract class VolatileBase : ISinglePhaseNotification
        {
            protected InstanceContext InstanceContext;
            protected Transaction Transaction;

            protected VolatileBase(InstanceContext instanceContext, Transaction transaction)
            {
                this.InstanceContext = instanceContext;
                this.Transaction = transaction;
                this.Transaction.EnlistVolatile(this, EnlistmentOptions.None);
            }

            protected abstract void Completed();

            public virtual void Commit(Enlistment enlistment)
            {
                this.Completed();
            }

            public virtual void InDoubt(Enlistment enlistment)
            {
                this.Completed();
            }

            public virtual void Rollback(Enlistment enlistment)
            {
                this.Completed();
            }

            public virtual void SinglePhaseCommit(SinglePhaseEnlistment enlistment)
            {
                enlistment.Committed();
                this.Completed();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }
        }

        sealed class RemoveReferenceRM : VolatileBase
        {
            string operation;
            long callCount = 0;
            EndpointDispatcher endpointDispatcher;

            internal RemoveReferenceRM(InstanceContext instanceContext, Transaction tx, string operation)
                : base(instanceContext, tx)
            {
                this.operation = operation;
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    this.endpointDispatcher = PerformanceCounters.GetEndpointDispatcher();
                }
                AspNetEnvironment.Current.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(this.GetType().FullName);
                }
            }

            internal long CallCount
            {
                get { return this.callCount; }
                set { this.callCount = value; }
            }

            protected override void Completed()
            {
                this.InstanceContext.Transaction.RemoveReference(this.Transaction);
                AspNetEnvironment.Current.DecrementBusyCount();
                if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceDecrementBusyCount(this.GetType().FullName);
                }
            }

            public override void SinglePhaseCommit(SinglePhaseEnlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxCommitted(this.endpointDispatcher, CallCount);
                }
                base.SinglePhaseCommit(enlistment);
            }


            public override void Commit(Enlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxCommitted(this.endpointDispatcher, CallCount);
                }
                base.Commit(enlistment);
            }

            public override void Rollback(Enlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxAborted(this.endpointDispatcher, CallCount);
                }

                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.TxAsyncAbort,
                        SR.GetString(SR.TraceCodeTxAsyncAbort,
                        this.Transaction.TransactionInformation.LocalIdentifier)
                        );
                }
                base.Rollback(enlistment);
            }

            public override void InDoubt(Enlistment enlistment)
            {
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.TxInDoubt(this.endpointDispatcher, CallCount);
                }
                base.InDoubt(enlistment);
            }
        }
    }

    internal enum ExclusiveInstanceContextTransactionResult
    {
        Acquired, Wait, Fault
    };
}
