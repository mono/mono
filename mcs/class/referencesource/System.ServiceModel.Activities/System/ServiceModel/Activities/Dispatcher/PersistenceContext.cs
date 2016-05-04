//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Activities;
    using System.Activities.DurableInstancing;
    using System.Activities.Hosting;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Activities.Description;
    using System.Transactions;
    using System.Xml.Linq;    

    sealed class PersistenceContext : CommunicationObject
    {
        // Dictionary keyed by Transaction HashCode. The value is the enlistment for that transaction.
        internal static Dictionary<int, PersistenceContextEnlistment> Enlistments = new Dictionary<int, PersistenceContextEnlistment>();

        readonly PersistenceProviderDirectory directory;

        readonly InstanceStore store;
        readonly InstanceHandle handle;

        readonly HashSet<InstanceKey> keysToAssociate;
        readonly HashSet<InstanceKey> keysToDisassociate;

        static TimeSpan defaultOpenTimeout = TimeSpan.FromSeconds(90);
        static TimeSpan defaultCloseTimeout = TimeSpan.FromSeconds(90);


        bool operationInProgress;

        WorkflowServiceInstance workflowInstance;

        // The hash code of the transaction that has this particular context "locked".
        // If the value of this property is 0, then no transaction is working on this context
        // and it is available to be "locked" by a transaction. Locking for transactions is done
        // with QueueForTransactionLock. This method returns a TransactionWaitAsyncResult. If the
        // lock was obtained by the call, the resulting AsyncResult will be marked as "Completed"
        // upon return from QueueForTransactionLock. If not, the caller should wait on the 
        // AsyncResult.AsyncWaitHandle before proceeding to update any of the fields of the context.
        int lockingTransaction;
        //We are keeping a reference to both the transaction object and the hash code to avoid calling the GetHashCode multiple times
        Transaction lockingTransactionObject;

        // This is the queue of TransactionWaitAsyncResult objects that are waiting for the
        // context to become "unlocked" with respect to a transaction. DequeueTransactionWaiter
        // removes the first element from this queue and returns it. If there is no element on the
        // queue, null is returned indicating that there was no outstanding waiter.
        Queue<TransactionWaitAsyncResult> transactionWaiterQueue;

        // Used by PPD when there is no store.
        internal PersistenceContext(PersistenceProviderDirectory directory,
            Guid instanceId, InstanceKey key, IEnumerable<InstanceKey> associatedKeys)
        {
            Fx.Assert(directory != null, "Directory is null in PersistenceContext.");
            Fx.Assert(instanceId != Guid.Empty, "Cannot provide an empty instance ID.");

            this.directory = directory;

            InstanceId = instanceId;

            AssociatedKeys = associatedKeys != null ? new HashSet<InstanceKey>(associatedKeys) :
                new HashSet<InstanceKey>();
            if (key != null && !AssociatedKeys.Contains(key))
            {
                AssociatedKeys.Add(key);
            }

            this.keysToAssociate = new HashSet<InstanceKey>(AssociatedKeys);
            this.keysToDisassociate = new HashSet<InstanceKey>();

            this.lockingTransaction = 0;
            this.Detaching = false;
            this.transactionWaiterQueue = new Queue<TransactionWaitAsyncResult>();
        }

        // Used by PPD when there is a store.
        internal PersistenceContext(PersistenceProviderDirectory directory, InstanceStore store,
            InstanceHandle handle, Guid instanceId, IEnumerable<InstanceKey> associatedKeys,
            bool newInstance, bool locked, InstanceView view, WorkflowIdentityKey updatedIdentity) 
            : this(directory, instanceId, null, associatedKeys)
        {
            Fx.Assert(store != null, "Null store passed to PersistenceContext.");
            Fx.Assert(handle != null, "Null handle passed to PersistenceContext.");

            this.store = store;
            this.handle = handle;

            IsInitialized = !newInstance;
            IsLocked = locked;

            if (view != null)
            {
                ReadSuspendedInfo(view);
            }

            // If we were loaded or we locked the instance, the keys will have been [....]'d.
            if (IsInitialized || IsLocked)
            {
                RationalizeSavedKeys(false);
            }

            if (IsInitialized)
            {
                Fx.Assert(view != null, "View must be specified on an initialized instance.");
                WorkflowIdentity definitionIdentity;                

                if (!TryGetValue<WorkflowIdentity>(view.InstanceMetadata, Workflow45Namespace.DefinitionIdentity, out definitionIdentity))
                {
                    definitionIdentity = null;
                }

                this.workflowInstance = this.directory.InitializeInstance(InstanceId, this, definitionIdentity, updatedIdentity, view.InstanceData, null);
            }
        }

        public Guid InstanceId { get; private set; }

        public bool IsLocked { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsVisible { get; internal set; }

        public bool IsSuspended { get; set; }
        public string SuspendedReason { get; set; }

        // Set to true when we detach from the PPD under a transaction. When the transaction completes,
        // either commit or abort, we will finish the removal from the PPD.
        internal bool Detaching
        {
            get; set;
        }

        public bool CanPersist
        {
            get
            {
                return (this.store != null);
            }
        }

        public bool IsHandleValid
        {
            get
            {
                return this.handle == null || this.handle.IsValid;
            }
        }

        internal Transaction LockingTransaction
        {
            get
            {
                lock (ThisLock)
                {
                    ThrowIfDisposedOrNotOpen();
                    return this.lockingTransactionObject;
                }
            }
        }

        // Used only by PPD.
        internal bool IsPermanentlyRemoved { get; set; }

        // If there's a directory, only it can write to this collection as long as the isntance is locked.  Otherwise,
        // only this class can.
        internal HashSet<InstanceKey> AssociatedKeys { get; private set; }
        internal ReadOnlyCollection<BookmarkInfo> Bookmarks { get; set; }

        protected override TimeSpan DefaultCloseTimeout { get { return defaultCloseTimeout; } }
        protected override TimeSpan DefaultOpenTimeout { get { return defaultOpenTimeout; } }

        // Remove key associations.  These are never immediately propagated to the store / cache.  Succeeds
        // if the keys don't exist or are associated with a different instance (in which case they are
        // not disassociated).
        public void DisassociateKeys(ICollection<InstanceKey> expiredKeys)
        {
            ThrowIfDisposedOrNotOpen();
            Fx.Assert(expiredKeys != null, "'expiredKeys' parameter to DisassociateKeys cannot be null.");

            try
            {
                StartOperation();
                ThrowIfCompleted();
                ThrowIfNotVisible();
                Fx.Assert(!IsInitialized || IsLocked, "Should not be visible if initialized and not locked.");

                foreach (InstanceKey key in expiredKeys)
                {
                    if (AssociatedKeys.Contains(key) && !this.keysToDisassociate.Contains(key))
                    {
                        this.keysToDisassociate.Add(key);
                        this.keysToAssociate.Remove(key);
                    }
                    else
                    {
                        Fx.Assert(!this.keysToAssociate.Contains(key), "Cannot be planning to associate this key.");
                    }
                }
            }
            finally
            {
                FinishOperation();
            }
        }

        public IAsyncResult BeginSave(
            IDictionary<XName, InstanceValue> instance,
            SaveStatus saveStatus,
            TimeSpan timeout,
            AsyncCallback callback,
            object state)
        {
            ThrowIfDisposedOrNotOpen();
            Fx.AssertAndThrow(instance != null, "'instance' parameter to BeginSave cannot be null.");

            return new SaveAsyncResult(this, instance, saveStatus, timeout, callback, state);
        }

        public void EndSave(IAsyncResult result)
        {
            SaveAsyncResult.End(result);
        }

        public IAsyncResult BeginRelease(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();

            return new ReleaseAsyncResult(this, timeout, callback, state);
        }

        public void EndRelease(IAsyncResult result)
        {
            ReleaseAsyncResult.End(result);
        }

        public IAsyncResult BeginAssociateKeys(
            ICollection<InstanceKey> associatedKeys, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginAssociateKeysHelper(associatedKeys, timeout, true, callback, state);
        }

        internal IAsyncResult BeginAssociateInfrastructureKeys(
            ICollection<InstanceKey> associatedKeys, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginAssociateKeysHelper(associatedKeys, timeout, true, callback, state);
        }

        IAsyncResult BeginAssociateKeysHelper(ICollection<InstanceKey> associatedKeys,
            TimeSpan timeout, bool applicationKeys, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();
            Fx.Assert(associatedKeys != null, "'associatedKeys' parameter to BeginAssociateKeys cannot be null.");

            return new AssociateKeysAsyncResult(this, associatedKeys, timeout, applicationKeys, callback, state);
        }

        public void EndAssociateKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        internal void EndAssociateInfrastructureKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        // UpdateSuspendMetadata and Unlock instance
        public IAsyncResult BeginUpdateSuspendMetadata(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();

            return new UpdateSuspendMetadataAsyncResult(this, reason, timeout, callback, state);
        }

        public void EndUpdateSuspendMetadata(IAsyncResult result)
        {
            UpdateSuspendMetadataAsyncResult.End(result);
        }

        public WorkflowServiceInstance GetInstance(WorkflowGetInstanceContext parameters)
        {
            if (this.workflowInstance == null && parameters != null)
            {
                lock (ThisLock)
                {
                    ThrowIfDisposedOrNotOpen();

                    if (this.workflowInstance == null)
                    {
                        try
                        {
                            WorkflowServiceInstance result;
                            if (parameters.WorkflowHostingEndpoint != null)
                            {
                                WorkflowHostingResponseContext responseContext = new WorkflowHostingResponseContext();
                                WorkflowCreationContext creationContext = parameters.WorkflowHostingEndpoint.OnGetCreationContext(parameters.Inputs, parameters.OperationContext, InstanceId, responseContext);
                                if (creationContext == null)
                                {
                                    throw FxTrace.Exception.AsError(WorkflowHostingEndpoint.CreateDispatchFaultException());
                                }
                                result = this.directory.InitializeInstance(InstanceId, this, null, creationContext);

                                // Return args
                                parameters.WorkflowCreationContext = creationContext;
                                parameters.WorkflowHostingResponseContext = responseContext;
                            }
                            else
                            {
                                result = this.directory.InitializeInstance(InstanceId, this, null, null);
                            }
                            this.workflowInstance = result;
                        }
                        finally
                        {
                            if (this.workflowInstance == null)
                            {
                                Fault();
                            }
                        }
                    }
                }
            }
            return this.workflowInstance;
        }

        protected override void OnAbort()
        {
            if (this.handle != null)
            {
                this.handle.Free();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            try
            {
                StartOperation();

                if (this.store != null)
                {
                    this.handle.Free();
                }
            }
            finally
            {
                FinishOperation();
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        // PersistenceProviderDirectory calls Open in an async path.  Do not introduce blocking work to this method
        // without changing PersistenceProviderDirectory to call BeginOpen instead.
        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.directory.RemoveInstance(this, true);
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            this.directory.RemoveInstance(this, true);
        }

        void RationalizeSavedKeys(bool updateDirectory)
        {
            if (updateDirectory)
            {
                this.directory.RemoveAssociations(this, this.keysToDisassociate);
            }
            else
            {
                foreach (InstanceKey key in this.keysToDisassociate)
                {
                    AssociatedKeys.Remove(key);
                }
            }

            this.keysToAssociate.Clear();
            this.keysToDisassociate.Clear();
        }

        void ReadSuspendedInfo(InstanceView view)
        {
            string suspendedReason = null;
            if (TryGetValue<string>(view.InstanceMetadata, WorkflowServiceNamespace.SuspendReason, out suspendedReason))
            {
                IsSuspended = true;
                SuspendedReason = suspendedReason;
            }
            else
            {
                IsSuspended = false;
                SuspendedReason = null;
            }
        }

        void StartOperation()
        {
            Fx.AssertAndThrow(!this.operationInProgress, "PersistenceContext doesn't support multiple operations.");
            this.operationInProgress = true;
        }

        void FinishOperation()
        {
            this.operationInProgress = false;
        }

        void OnFinishOperationHelper(Exception exception, bool ownsThrottle)
        {
            try
            {
                if (exception is OperationCanceledException)
                {
                    throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.HandleFreedInDirectory, exception)); 
                }
                else if (exception is TimeoutException)
                {
                    Fault();
                }
            }
            finally
            {
                if (ownsThrottle)
                {
                    this.directory.ReleaseThrottle();
                }
                FinishOperation();
            }
        }

        void ThrowIfCompleted()
        {
            Fx.AssertAndThrow(!IsCompleted, "PersistenceContext operation invalid: instance already completed.");
        }

        void ThrowIfNotVisible()
        {
            // Be charitable to racy aborts.
            if (!IsVisible)
            {
                lock (ThisLock)
                {
                    Fx.AssertAndThrow(State != CommunicationState.Opened,
                        "PersistenceContext operation invalid: instance must be visible.");
                }
            }
        }

        internal static bool TryGetValue<T>(IDictionary<XName, InstanceValue> data, XName key, out T value)
        {
            InstanceValue instanceValue;
            value = default(T);
            if (data.TryGetValue(key, out instanceValue) && !instanceValue.IsDeletedValue)
            {
                if (instanceValue.Value is T)
                {
                    value = (T)instanceValue.Value;
                    return true;
                }
                else if (instanceValue.Value == null && !(value is ValueType))
                {
                    // need to check for null assignments to value types
                    return true;
                }
                else
                {
                    if (instanceValue.Value == null)
                    {
                        throw FxTrace.Exception.AsError(new InstancePersistenceException(SRCore.NullAssignedToValueType(typeof(T))));
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new InstancePersistenceException(SRCore.IncorrectValueType(typeof(T), instanceValue.Value.GetType())));
                    }
                }
            }
            else
            {
                return false;
            }
        }

        internal TransactionWaitAsyncResult BeginEnlist(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();
            // The transaction to enlist on is in Transaction.Current. The actual enlistment, if needed, will be made in 
            // TransactionWaitAsyncResult when it is notified that it has the transaction lock.
            return new TransactionWaitAsyncResult(Transaction.Current, this, timeout, callback, state);
        }

        [SuppressMessage(FxCop.Category.ReliabilityBasic, FxCop.Rule.CommunicationObjectThrowIf,
            Justification = "We are intentionally re-validating the state of the instance after having acquired the transaction lock")]
        internal void EndEnlist(IAsyncResult result)
        {
            TransactionWaitAsyncResult.End(result);
            // The PersistenceContext may have been aborted while we were waiting for the transaction lock.
            ThrowIfDisposedOrNotOpen();
        }


        // Returns true if the call was able to obtain the transaction lock; false if we had
        // to queue the request for the lock.
        internal bool QueueForTransactionLock(Transaction requestingTransaction, TransactionWaitAsyncResult txWaitAsyncResult)
        {
            lock (ThisLock)
            {
                // If the transaction "lock" is not already held, give it to this requester.
                if (0 == this.lockingTransaction)
                {
                    // It's possible that this particular request is not transacted.
                    if (null != requestingTransaction)
                    {
                        this.lockingTransaction = requestingTransaction.GetHashCode();
                        this.lockingTransactionObject = requestingTransaction.Clone();
                    }
                    // No queuing because we weren't already locked by a transaction.
                    return true;
                }
                else if ((null != requestingTransaction) && (this.lockingTransaction == requestingTransaction.GetHashCode()))
                {
                    // Same transaction as the locking transaction - no queuing.
                    return true;
                }
                else
                {
                    // Some other transaction has the lock, so add the AsyncResult to the queue.
                    this.transactionWaiterQueue.Enqueue(txWaitAsyncResult);
                    return false;
                }
            }
        }

        // Dequeue and schedule the top element on queue of waiting TransactionWaitAsyncResult objects.
        // Before returning this also makes the transaction represented by the dequeued TransactionWaitAsyncResult
        // the owner of the transaction "lock" for this context.
        internal void ScheduleNextTransactionWaiter()
        {
            TransactionWaitAsyncResult dequeuedWaiter = null;
            bool detachThis = false;

            lock (ThisLock)
            {
                // Only try Dequeue if we have entries on the queue.
                bool atLeastOneSuccessfullyCompleted = false;
                if (0 < this.transactionWaiterQueue.Count)
                {
                    while ((0 < this.transactionWaiterQueue.Count) && !atLeastOneSuccessfullyCompleted)
                    {
                        dequeuedWaiter = this.transactionWaiterQueue.Dequeue();

                        // It's possible that the waiter didn't have a transaction.
                        // If that is the case, we don't have a transaction to "lock" the context.
                        if (null != dequeuedWaiter.Transaction)
                        {
                            this.lockingTransactionObject = dequeuedWaiter.Transaction;
                            this.lockingTransaction = lockingTransactionObject.GetHashCode();
                        }
                        else
                        {
                            this.lockingTransaction = 0;
                            this.lockingTransactionObject = null;
                        }

                        atLeastOneSuccessfullyCompleted = dequeuedWaiter.Complete() || atLeastOneSuccessfullyCompleted;

                        if (this.Detaching)
                        {
                            detachThis = true;
                            this.Detaching = false;
                        }

                        // If we are doing a permanent detach, we must have received an OnClosing or
                        // OnFaulted while the PersistenceContext was locked for a transaction. In that
                        // case, we want to wake up ALL waiters.
                        if (this.IsPermanentlyRemoved)
                        {
                            this.lockingTransaction = 0;
                            this.lockingTransactionObject = null;
                            while (0 < this.transactionWaiterQueue.Count)
                            {
                                dequeuedWaiter = this.transactionWaiterQueue.Dequeue();
                                atLeastOneSuccessfullyCompleted = dequeuedWaiter.Complete() || atLeastOneSuccessfullyCompleted;
                            }
                        }

                        // Now we need to look for any adjacent waiters in the queue that are
                        // waiting for the same transaction. If we still have entries on the queue,
                        // we must have a waiterToComplete. Note that if we were doing a permanent detach,
                        // there won't be any waiters left in the queue at this point.
                        while (0 < this.transactionWaiterQueue.Count)
                        {
                            TransactionWaitAsyncResult nextWaiter = this.transactionWaiterQueue.Peek();
                            if (0 == this.lockingTransaction)
                            {
                                // We dequeue this waiter because we shouldn't block transactional waiters
                                // behind non-transactional waiters because there is nothing to wake up the
                                // transactional waiters in that case. Also set this.LockingTransaction
                                // to that of the next waiter.
                                if (null != nextWaiter.Transaction)
                                {
                                    this.lockingTransactionObject = nextWaiter.Transaction;
                                    this.lockingTransaction = this.lockingTransactionObject.GetHashCode();
                                }
                            }
                            else if (null != nextWaiter.Transaction)
                            {
                                // Stop looking if the new lockingTransaction is different than
                                // the nextWaiter's transaction. 
                                if (this.lockingTransaction != nextWaiter.Transaction.GetHashCode())
                                {
                                    break;  // out of the inner-while
                                }
                            }
                            else
                            {
                                // The nextWaiter is non-transational, so it doesn't match the current
                                // lock holder, so we are done.
                                break;  // out of the inner-while
                            }

                            dequeuedWaiter = this.transactionWaiterQueue.Dequeue();
                            atLeastOneSuccessfullyCompleted = dequeuedWaiter.Complete() || atLeastOneSuccessfullyCompleted;
                        }
                    }
                }
                if (!atLeastOneSuccessfullyCompleted)
                {
                    // There are no more waiters, so the context is no longer "locked" by a transaction.
                    this.lockingTransaction = 0;
                    this.lockingTransactionObject = null;
                }
            }

            // If we are detaching and it is NOT permanently removed, finish the detach by calling RemoveInstance non-transactionally.
            // It will be marked as permanently removed in OnClosing and OnFaulted and it will have already been removed, so we don't
            // want to try to remove it again.
            if (detachThis)
            {
                this.directory.RemoveInstance(this, false);
            }
        }

        bool ScheduleDetach()
        {
            lock (ThisLock)
            {
                if (this.lockingTransaction != 0)
                {
                    Detaching = true;
                    return true;
                }
            }
            return false;
        }

        void PopulateActivationMetadata(SaveWorkflowCommand saveCommand)
        {
            bool saveIdentity;
            if (!IsInitialized)
            {
                Fx.Assert(this.directory.InstanceMetadataChanges != null, "We should always be non-null here.");
                foreach (KeyValuePair<XName, InstanceValue> pair in this.directory.InstanceMetadataChanges)
                {
                    saveCommand.InstanceMetadataChanges.Add(pair.Key, pair.Value);
                }
                saveIdentity = this.workflowInstance.DefinitionIdentity != null;
            }
            else 
            {
                saveIdentity = this.workflowInstance.HasBeenUpdated;
            }

            if (saveIdentity)
            {
                if (this.workflowInstance.DefinitionIdentity != null)
                {
                    saveCommand.InstanceMetadataChanges.Add(Workflow45Namespace.DefinitionIdentity, new InstanceValue(this.workflowInstance.DefinitionIdentity, InstanceValueOptions.None));
                }
                else
                {
                    saveCommand.InstanceMetadataChanges.Add(Workflow45Namespace.DefinitionIdentity, InstanceValue.DeletedValue);
                }
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            PersistenceContext persistenceContext;

            public CloseAsyncResult(PersistenceContext persistenceContext, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                OnCompleting = new Action<AsyncResult, Exception>(OnFinishOperation);

                bool success = false;
                bool completeSelf = false;
                try
                {
                    this.persistenceContext.StartOperation();

                    if (this.persistenceContext.store != null)
                    {
                        Fx.Assert(this.persistenceContext.handle != null, "WorkflowInstance failed to call SetHandle - from OnBeginClose.");
                        this.persistenceContext.handle.Free();
                    }
                    completeSelf = true;
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        this.persistenceContext.FinishOperation();
                    }
                }

                if (completeSelf)
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            void OnFinishOperation(AsyncResult result, Exception exception)
            {
                this.persistenceContext.FinishOperation();
            }
        }

        class SaveAsyncResult : TransactedAsyncResult
        {
            static readonly AsyncCompletion handleEndExecute = new AsyncCompletion(HandleEndExecute);
            static readonly AsyncCompletion handleEndEnlist = new AsyncCompletion(HandleEndEnlist);

            readonly PersistenceContext persistenceContext;
            readonly SaveStatus saveStatus;
            readonly TimeoutHelper timeoutHelper;
            readonly DependentTransaction transaction;

            public SaveAsyncResult(PersistenceContext persistenceContext, IDictionary<XName, InstanceValue> instance, SaveStatus saveStatus, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                OnCompleting = new Action<AsyncResult, Exception>(OnFinishOperation);

                this.saveStatus = saveStatus;

                bool success = false;
                try
                {
                    this.persistenceContext.StartOperation();

                    this.persistenceContext.ThrowIfCompleted();
                    this.persistenceContext.ThrowIfNotVisible();
                    Fx.Assert(!this.persistenceContext.IsInitialized || this.persistenceContext.IsLocked,
                        "Should not be visible if initialized and not locked.");

                    this.timeoutHelper = new TimeoutHelper(timeout);

                    Transaction currentTransaction = Transaction.Current;
                    if (currentTransaction != null)
                    {
                        this.transaction = currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }

                    if (this.persistenceContext.store != null)
                    {
                        SaveWorkflowCommand saveCommand = new SaveWorkflowCommand();                        
                        foreach (KeyValuePair<XName, InstanceValue> value in instance)
                        {
                            saveCommand.InstanceData.Add(value);
                        }
                        this.persistenceContext.PopulateActivationMetadata(saveCommand);
                        if (this.persistenceContext.IsSuspended)
                        {
                            saveCommand.InstanceMetadataChanges.Add(WorkflowServiceNamespace.SuspendReason, new InstanceValue(this.persistenceContext.SuspendedReason));
                        }
                        else
                        {
                            saveCommand.InstanceMetadataChanges.Add(WorkflowServiceNamespace.SuspendReason, InstanceValue.DeletedValue);
                            saveCommand.InstanceMetadataChanges.Add(WorkflowServiceNamespace.SuspendException, InstanceValue.DeletedValue);
                        }
                        foreach (InstanceKey key in this.persistenceContext.keysToAssociate)
                        {
                            saveCommand.InstanceKeysToAssociate.Add(key.Value, key.Metadata);
                        }
                        foreach (InstanceKey key in this.persistenceContext.keysToDisassociate)
                        {
                            // We are going to Complete and Disassociate with the same Save command.
                            saveCommand.InstanceKeysToComplete.Add(key.Value);
                            saveCommand.InstanceKeysToFree.Add(key.Value);
                        }

                        if (this.saveStatus == SaveStatus.Completed)
                        {
                            saveCommand.CompleteInstance = true;
                            saveCommand.UnlockInstance = true;
                        }
                        else
                        {
                            saveCommand.UnlockInstance = this.saveStatus == SaveStatus.Unlocked;
                        }

                        IAsyncResult result = this.persistenceContext.store.BeginExecute(
                            this.persistenceContext.handle,
                            saveCommand,
                            this.timeoutHelper.RemainingTime(),
                            PrepareAsyncCompletion(SaveAsyncResult.handleEndExecute),
                            this);
                        if (SyncContinue(result))
                        {
                            Complete(true);
                        }
                    }
                    else
                    {
                        if (this.saveStatus == SaveStatus.Completed)
                        {
                            this.persistenceContext.IsCompleted = true;
                            this.persistenceContext.IsLocked = false;
                        }
                        else
                        {
                            this.persistenceContext.IsLocked = this.saveStatus != SaveStatus.Unlocked;
                        }
                        if (AfterSave())
                        {
                            Complete(true);
                        }
                    }
                    success = true;
                }
                catch (OperationCanceledException exception)
                {
                    throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.HandleFreedInDirectory, exception)); 
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SaveAsyncResult>(result);
            }

            static bool HandleEndExecute(IAsyncResult result)
            {
                SaveAsyncResult thisPtr = (SaveAsyncResult)result.AsyncState;
                thisPtr.persistenceContext.store.EndExecute(result);
                thisPtr.persistenceContext.IsCompleted = thisPtr.saveStatus == SaveStatus.Completed;
                thisPtr.persistenceContext.IsLocked = thisPtr.saveStatus == SaveStatus.Locked;
                return thisPtr.AfterSave();
            }

            bool AfterSave()
            {
                this.persistenceContext.IsInitialized = true;

                if (this.saveStatus != SaveStatus.Locked)
                {
                    IAsyncResult result;
                    using (PrepareTransactionalCall(this.transaction))
                    {
                        result = this.persistenceContext.BeginEnlist(this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(SaveAsyncResult.handleEndEnlist), this);
                    }
                    return SyncContinue(result);
                }

                return AfterEnlist();
            }

            bool AfterEnlist()
            {
                this.persistenceContext.RationalizeSavedKeys(this.saveStatus == SaveStatus.Locked);
                return true;
            }

            static bool HandleEndEnlist(IAsyncResult result)
            {
                SaveAsyncResult thisPtr = (SaveAsyncResult)result.AsyncState;
                thisPtr.persistenceContext.EndEnlist(result);

                if (!thisPtr.persistenceContext.ScheduleDetach())
                {
                    thisPtr.persistenceContext.directory.RemoveInstance(thisPtr.persistenceContext);
                }
                return thisPtr.AfterEnlist();
            }

            void OnFinishOperation(AsyncResult result, Exception exception)
            {
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }

        class ReleaseAsyncResult : TransactedAsyncResult
        {
            static readonly AsyncCompletion handleEndExecute = new AsyncCompletion(HandleEndExecute);
            static readonly AsyncCompletion handleEndEnlist = new AsyncCompletion(HandleEndEnlist);

            readonly PersistenceContext persistenceContext;
            readonly TimeoutHelper timeoutHelper;
            readonly DependentTransaction transaction;

            public ReleaseAsyncResult(PersistenceContext persistenceContext, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                OnCompleting = new Action<AsyncResult, Exception>(OnFinishOperation);

                bool success = false;
                try
                {
                    this.persistenceContext.StartOperation();

                    this.timeoutHelper = new TimeoutHelper(timeout);

                    Transaction currentTransaction = Transaction.Current;
                    if (currentTransaction != null)
                    {
                        this.transaction = currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }

                    if (this.persistenceContext.IsVisible)
                    {
                        if (this.persistenceContext.store != null && this.persistenceContext.IsLocked)
                        {
                            SaveWorkflowCommand saveCommand = new SaveWorkflowCommand() { UnlockInstance = true };
                            this.persistenceContext.PopulateActivationMetadata(saveCommand);
                            IAsyncResult result = this.persistenceContext.store.BeginExecute(
                                this.persistenceContext.handle,
                                saveCommand,
                                this.timeoutHelper.RemainingTime(),
                                PrepareAsyncCompletion(ReleaseAsyncResult.handleEndExecute),
                                this);
                            if (SyncContinue(result))
                            {
                                Complete(true);
                            }
                        }
                        else
                        {
                            if (AfterUnlock())
                            {
                                Complete(true);
                            }
                        }
                    }
                    else
                    {
                        // If we're not visible because we were aborted in a ----, the caller needs to know.
                        lock (this.persistenceContext.ThisLock)
                        {
                            this.persistenceContext.ThrowIfDisposedOrNotOpen();
                        }
                        Complete(true);
                    }
                    success = true;
                }
                catch (OperationCanceledException exception)
                {
                    throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.HandleFreedInDirectory, exception)); 
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReleaseAsyncResult>(result);
            }

            static bool HandleEndExecute(IAsyncResult result)
            {
                ReleaseAsyncResult thisPtr = (ReleaseAsyncResult)result.AsyncState;
                thisPtr.persistenceContext.store.EndExecute(result);
                return thisPtr.AfterUnlock();
            }

            bool AfterUnlock()
            {
                this.persistenceContext.IsLocked = false;

                IAsyncResult result;
                using (PrepareTransactionalCall(this.transaction))
                {
                    result = this.persistenceContext.BeginEnlist(this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(ReleaseAsyncResult.handleEndEnlist), this);
                }
                return SyncContinue(result);
            }

            static bool HandleEndEnlist(IAsyncResult result)
            {
                ReleaseAsyncResult thisPtr = (ReleaseAsyncResult)result.AsyncState;
                thisPtr.persistenceContext.EndEnlist(result);

                if (!thisPtr.persistenceContext.ScheduleDetach())
                {
                    thisPtr.persistenceContext.directory.RemoveInstance(thisPtr.persistenceContext);
                }

                foreach (InstanceKey key in thisPtr.persistenceContext.keysToAssociate)
                {
                    thisPtr.persistenceContext.AssociatedKeys.Remove(key);
                }
                thisPtr.persistenceContext.keysToAssociate.Clear();
                thisPtr.persistenceContext.keysToDisassociate.Clear();

                return true;
            }

            void OnFinishOperation(AsyncResult result, Exception exception)
            {
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }

        class AssociateKeysAsyncResult : TransactedAsyncResult
        {
            static readonly AsyncCompletion handleEndExecute = new AsyncCompletion(HandleEndExecute);
            static readonly AsyncCompletion handleEndEnlist = new AsyncCompletion(HandleEndEnlist);

            readonly PersistenceContext persistenceContext;
            readonly bool applicationKeys;
            readonly ICollection<InstanceKey> keysToAssociate;
            readonly TimeoutHelper timeoutHelper;
            readonly DependentTransaction transaction;

            public AssociateKeysAsyncResult(PersistenceContext persistenceContext, ICollection<InstanceKey> associatedKeys, TimeSpan timeout,
                bool applicationKeys, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                this.applicationKeys = applicationKeys;
                this.keysToAssociate = associatedKeys;
                this.timeoutHelper = new TimeoutHelper(timeout);

                OnCompleting = new Action<AsyncResult, Exception>(OnFinishOperation);

                bool success = false;
                try
                {
                    this.persistenceContext.StartOperation();

                    this.persistenceContext.ThrowIfCompleted();
                    this.persistenceContext.ThrowIfNotVisible();
                    Fx.Assert(!this.persistenceContext.IsInitialized || this.persistenceContext.IsLocked,
                        "Should not be visible if initialized and not locked.");

                    Transaction currentTransaction = Transaction.Current;
                    if (currentTransaction != null)
                    {
                        this.transaction = currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }

                    // We need to get the transaction lock and enlist on the transaction, if there is one.
                    IAsyncResult enlistResult = persistenceContext.BeginEnlist(this.timeoutHelper.RemainingTime(), 
                                      this.PrepareAsyncCompletion(handleEndEnlist), this);
                    if (SyncContinue(enlistResult))
                    {
                        Complete(true);
                    }
                    success = true;
                }
                catch (InstancePersistenceException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                catch (OperationCanceledException exception)
                {
                    throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.HandleFreedInDirectory, exception)); 
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            // We need to complete our dependent clone because OnFinishOperation will not
                            // get called in this case.
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AssociateKeysAsyncResult>(result);
            }

            static bool HandleEndExecute(IAsyncResult result)
            {
                AssociateKeysAsyncResult thisPtr = (AssociateKeysAsyncResult)result.AsyncState;
                thisPtr.persistenceContext.store.EndExecute(result);
                return thisPtr.AfterUpdate();
            }

            static bool HandleEndEnlist(IAsyncResult result)
            {
                AssociateKeysAsyncResult thisPtr = (AssociateKeysAsyncResult)result.AsyncState;
                bool returnValue = false;

                if (!thisPtr.persistenceContext.directory.TryAddAssociations(
                    thisPtr.persistenceContext,
                    thisPtr.keysToAssociate,
                    thisPtr.persistenceContext.keysToAssociate,
                    thisPtr.applicationKeys ? thisPtr.persistenceContext.keysToDisassociate : null))
                {
                    lock (thisPtr.persistenceContext.ThisLock)
                    {
                        thisPtr.persistenceContext.ThrowIfDisposedOrNotOpen();
                    }
                    throw Fx.AssertAndThrow("Should only fail to add keys in a ---- with abort.");
                }

                if (thisPtr.persistenceContext.directory.ConsistencyScope == DurableConsistencyScope.Global)
                {
                    // Only do a SetKeysToPersist or Save command if we have keys to associate or disassociate.
                    // It's possible that we got invoked with a key that was already in the
                    // AssociatedKeys collection.
                    if ((thisPtr.persistenceContext.keysToAssociate.Count != 0) ||
                        ((thisPtr.persistenceContext.keysToDisassociate.Count != 0) &&
                         (thisPtr.applicationKeys)))
                    {
                        if (thisPtr.persistenceContext.store != null)
                        {
                            SaveWorkflowCommand saveCommand = new SaveWorkflowCommand();
                            foreach (InstanceKey key in thisPtr.persistenceContext.keysToAssociate)
                            {                                
                                saveCommand.InstanceKeysToAssociate.Add(key.Value, key.Metadata);
                            }
                            if (thisPtr.applicationKeys)
                            {
                                foreach (InstanceKey key in thisPtr.persistenceContext.keysToDisassociate)
                                {
                                    // We are going to Complete and Disassociate with the same Save command.
                                    saveCommand.InstanceKeysToComplete.Add(key.Value);
                                    saveCommand.InstanceKeysToFree.Add(key.Value);
                                }
                            }
                            IAsyncResult beginExecuteResult = null;
                            using (thisPtr.PrepareTransactionalCall(thisPtr.transaction))
                            {
                                beginExecuteResult = thisPtr.persistenceContext.store.BeginExecute(
                                    thisPtr.persistenceContext.handle,
                                    saveCommand,
                                    thisPtr.timeoutHelper.RemainingTime(),
                                    thisPtr.PrepareAsyncCompletion(AssociateKeysAsyncResult.handleEndExecute),
                                    thisPtr);
                            }
                            returnValue = thisPtr.SyncContinue(beginExecuteResult);
                        }
                    }
                    else
                    {
                        returnValue = thisPtr.AfterUpdate();
                    }
                }
                else
                {
                    returnValue = thisPtr.AfterUpdate();
                }

                return returnValue;
            }

            bool AfterUpdate()
            {
                if (this.applicationKeys)
                {
                    this.persistenceContext.RationalizeSavedKeys(true);
                }
                else
                {
                    this.persistenceContext.keysToAssociate.Clear();
                }

                return true;
            }

            void OnFinishOperation(AsyncResult result, Exception exception)
            {
                if (exception is InstancePersistenceException)
                {
                    this.persistenceContext.Fault();
                }
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    // We are all done. If we have a savedTransaction, we need to complete it now.
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }

        class UpdateSuspendMetadataAsyncResult : AsyncResult
        {
            static readonly AsyncCompletion handleEndExecute = new AsyncCompletion(HandleEndExecute);

            readonly PersistenceContext persistenceContext;
            readonly TimeoutHelper timeoutHelper;
            readonly DependentTransaction transaction;

            public UpdateSuspendMetadataAsyncResult(PersistenceContext persistenceContext, Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.persistenceContext = persistenceContext;
                OnCompleting = new Action<AsyncResult, Exception>(OnFinishOperation);

                bool success = false;
                try
                {
                    this.persistenceContext.StartOperation();

                    this.timeoutHelper = new TimeoutHelper(timeout);

                    Transaction currentTransaction = Transaction.Current;
                    if (currentTransaction != null)
                    {
                        this.transaction = currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    }

                    if (this.persistenceContext.store != null)
                    {
                        SaveWorkflowCommand saveCommand = new SaveWorkflowCommand();
                        this.persistenceContext.PopulateActivationMetadata(saveCommand);
                        saveCommand.InstanceMetadataChanges[WorkflowServiceNamespace.SuspendReason] = new InstanceValue(reason.Message);
                        saveCommand.InstanceMetadataChanges[WorkflowServiceNamespace.SuspendException] = new InstanceValue(reason, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                        saveCommand.UnlockInstance = true;

                        IAsyncResult result = this.persistenceContext.store.BeginExecute(
                            this.persistenceContext.handle,
                            saveCommand,
                            this.timeoutHelper.RemainingTime(),
                            PrepareAsyncCompletion(handleEndExecute),
                            this);
                        if (SyncContinue(result))
                        {
                            Complete(true);
                        }
                    }
                    else
                    {
                        Complete(true);
                    }
                    success = true;
                }
                catch (OperationCanceledException exception)
                {
                    throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.HandleFreedInDirectory, exception));
                }
                catch (TimeoutException)
                {
                    this.persistenceContext.Fault();
                    throw;
                }
                finally
                {
                    if (!success)
                    {
                        try
                        {
                            if (this.transaction != null)
                            {
                                this.transaction.Complete();
                            }
                        }
                        finally
                        {
                            this.persistenceContext.FinishOperation();
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<UpdateSuspendMetadataAsyncResult>(result);
            }

            static bool HandleEndExecute(IAsyncResult result)
            {
                UpdateSuspendMetadataAsyncResult thisPtr = (UpdateSuspendMetadataAsyncResult)result.AsyncState;
                thisPtr.persistenceContext.store.EndExecute(result);
                return true;
            }

            void OnFinishOperation(AsyncResult result, Exception exception)
            {
                try
                {
                    this.persistenceContext.OnFinishOperationHelper(exception, false);
                }
                finally
                {
                    if (this.transaction != null)
                    {
                        this.transaction.Complete();
                    }
                }
            }
        }
    }
}
