//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Activities;
    using System.Activities.DurableInstancing;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Interop;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities.Description;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;
    using System.Activities.Hosting;
    using System.Security;
    using System.Security.Permissions;
    using System.Activities.DynamicUpdate;

    sealed class PersistenceProviderDirectory
    {
        readonly WorkflowServiceHost serviceHost;
        readonly InstanceStore store;
        readonly InstanceOwner owner;
        readonly WorkflowDefinitionProvider workflowDefinitionProvider;

        [Fx.Tag.SynchronizationObject(Blocking = false)]
        readonly Dictionary<Guid, PersistenceContext> keyMap;

        readonly InstanceThrottle throttle;

        [Fx.Tag.Cache(typeof(PersistenceContext), Fx.Tag.CacheAttrition.ElementOnCallback, SizeLimit = "MaxConcurrentInstances")]
        Dictionary<Guid, PersistenceContext> instanceCache;

        Dictionary<InstanceKey, AsyncWaitHandle> loadsInProgress;

        HashSet<PersistencePipeline> pipelinesInUse;
        bool aborted;

        internal PersistenceProviderDirectory(InstanceStore store, InstanceOwner owner, IDictionary<XName, InstanceValue> instanceMetadataChanges, WorkflowDefinitionProvider workflowDefinitionProvider, WorkflowServiceHost serviceHost,
            DurableConsistencyScope consistencyScope, int maxInstances)
            : this(workflowDefinitionProvider, serviceHost, consistencyScope, maxInstances)
        {
            Fx.Assert(store != null, "InstanceStore must be specified on PPD.");
            Fx.Assert(owner != null, "InstanceOwner must be specified on PPD.");

            this.store = store;
            this.owner = owner;
            this.InstanceMetadataChanges = instanceMetadataChanges;
        }

        internal PersistenceProviderDirectory(WorkflowDefinitionProvider workflowDefinitionProvider, WorkflowServiceHost serviceHost, int maxInstances)
            : this(workflowDefinitionProvider, serviceHost, DurableConsistencyScope.Local, maxInstances)
        {
        }

        PersistenceProviderDirectory(WorkflowDefinitionProvider workflowDefinitionProvider, WorkflowServiceHost serviceHost, DurableConsistencyScope consistencyScope, int maxInstances)
        {
            Fx.Assert(workflowDefinitionProvider != null, "definition provider must be specified on PPD.");
            Fx.Assert(serviceHost != null, "WorkflowServiceHost must be specified on PPD.");
            Fx.AssertAndThrow(maxInstances > 0, "MaxInstance must be greater than zero on PPD.");

            this.workflowDefinitionProvider = workflowDefinitionProvider;
            this.serviceHost = serviceHost;

            ConsistencyScope = consistencyScope;
            MaxInstances = maxInstances;

            this.throttle = new InstanceThrottle(MaxInstances, serviceHost);
            this.pipelinesInUse = new HashSet<PersistencePipeline>();

            this.keyMap = new Dictionary<Guid, PersistenceContext>();
            this.instanceCache = new Dictionary<Guid, PersistenceContext>();
            this.loadsInProgress = new Dictionary<InstanceKey, AsyncWaitHandle>();            
        }

        public IDictionary<XName, InstanceValue> InstanceMetadataChanges { get; private set; }

        public DurableConsistencyScope ConsistencyScope { get; private set; }

        public int MaxInstances { get; private set; }

        object ThisLock
        {
            get
            {
                return this.keyMap;
            }
        }

        public WorkflowServiceInstance InitializeInstance(Guid instanceId, PersistenceContext context, IDictionary<XName, InstanceValue> instance, WorkflowCreationContext creationContext)
        {
            Activity workflowDefinition = null;
            this.workflowDefinitionProvider.TryGetDefinition(this.workflowDefinitionProvider.DefaultDefinitionIdentity, out workflowDefinition);
            Fx.Assert(workflowDefinition != null, "Default definition shouldn't be null.");

            return WorkflowServiceInstance.InitializeInstance(context, instanceId, workflowDefinition, this.workflowDefinitionProvider.DefaultDefinitionIdentity, instance, creationContext,
                WorkflowSynchronizationContext.Instance, this.serviceHost);
        }

        public WorkflowServiceInstance InitializeInstance(Guid instanceId, PersistenceContext context, WorkflowIdentity definitionIdentity, WorkflowIdentityKey updatedIdentity, IDictionary<XName, InstanceValue> instance, WorkflowCreationContext creationContext)
        {
            Activity workflowDefinition = null;
            DynamicUpdateMap updateMap = null;
            if (updatedIdentity != null && !object.Equals(updatedIdentity.Identity, definitionIdentity))
            {
                if (!this.workflowDefinitionProvider.TryGetDefinitionAndMap(definitionIdentity, updatedIdentity.Identity, out workflowDefinition, out updateMap))
                {
                    if (this.workflowDefinitionProvider.TryGetDefinition(updatedIdentity.Identity, out workflowDefinition))
                    {
                        throw FxTrace.Exception.AsError(new FaultException(
                            OperationExecutionFault.CreateUpdateFailedFault(SR.UpdateMapNotFound(definitionIdentity, updatedIdentity.Identity))));
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new FaultException(
                            OperationExecutionFault.CreateUpdateFailedFault(SR.UpdateDefinitionNotFound(updatedIdentity.Identity))));
                    }
                }
            }
            else if (!this.workflowDefinitionProvider.TryGetDefinition(definitionIdentity, out workflowDefinition))
            {
                throw FxTrace.Exception.AsError(new VersionMismatchException(SR.WorkflowServiceDefinitionIdentityNotMatched(definitionIdentity), null, definitionIdentity));
            }

            WorkflowIdentity definitionToLoad = updatedIdentity == null ? definitionIdentity : updatedIdentity.Identity;
            return WorkflowServiceInstance.InitializeInstance(context, instanceId, workflowDefinition, definitionToLoad, instance, creationContext,
                WorkflowSynchronizationContext.Instance, this.serviceHost, updateMap);
        }

        // This should be called as part of the closing path. The caller should guarantee that
        // no LoadOrCreates are in progress or will be initialized after this, same with
        // AddAssociations or AddInstance.
        [Fx.Tag.Throws(typeof(OperationCanceledException), "The directory of loaded instances has been aborted. An abrupt shutdown of the service is in progress.")]
        public IEnumerable<PersistenceContext> GetContexts()
        {
            lock (ThisLock)
            {
                ThrowIfClosedOrAborted();

                // The ToList is for snapshotting within the lock.
                return this.instanceCache.Values.ToList();
            }
        }

        // All PersistenceContexts are opened before they are returned.

        [Fx.Tag.InheritThrows(From = "EndLoad")]
        public IAsyncResult BeginLoad(InstanceKey key, ICollection<InstanceKey> associatedKeys, Transaction transaction,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (key == null)
            {
                throw FxTrace.Exception.ArgumentNull("key");
            }
            if (key.Value == Guid.Empty)
            {
                throw FxTrace.Exception.Argument("key", SR.InvalidKey);
            }

            return new LoadOrCreateAsyncResult(this, key, Guid.Empty, false,
                associatedKeys, transaction, false, null, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "EndLoad")]
        public IAsyncResult BeginLoad(Guid instanceId, ICollection<InstanceKey> associatedKeys, Transaction transaction, bool loadAny, WorkflowIdentityKey updatedIdentity,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (instanceId == Guid.Empty && !loadAny)
            {
                throw FxTrace.Exception.Argument("instanceId", SR.InvalidInstanceId);
            }
            Fx.Assert(!loadAny || instanceId == Guid.Empty, "instanceId must be Empty for loadAny!");
            return new LoadOrCreateAsyncResult(this, null, instanceId, false,
                associatedKeys, transaction, loadAny, updatedIdentity, timeout, callback, state);
        }

        [Fx.Tag.Throws.Timeout("Instance may have been locked, keys may have been associated. (?!?)")]
        [Fx.Tag.Throws(typeof(InstancePersistenceException), "Instance wasn't locked, keys weren't associated.")]
        [Fx.Tag.Throws(typeof(CommunicationObjectAbortedException), "Instance store aborted")]
        [Fx.Tag.Throws(typeof(CommunicationObjectFaultedException), "Instance store faulted")]
        public PersistenceContext EndLoad(IAsyncResult result, out bool fromCache)
        {
            return LoadOrCreateAsyncResult.End(result, out fromCache);
        }

        [Fx.Tag.InheritThrows(From = "EndLoadOrCreate")]
        public IAsyncResult BeginLoadOrCreate(InstanceKey key, Guid suggestedId,
            ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            if (key == null)
            {
                throw FxTrace.Exception.ArgumentNull("key");
            }
            if (key.Value == Guid.Empty)
            {
                throw FxTrace.Exception.Argument("key", SR.InvalidKey);
            }

            return new LoadOrCreateAsyncResult(this, key, suggestedId, true,
                associatedKeys, transaction, false, null, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "EndLoadOrCreate")]
        public IAsyncResult BeginLoadOrCreate(Guid instanceId, ICollection<InstanceKey> associatedKeys, Transaction transaction,
            TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new LoadOrCreateAsyncResult(this, null, instanceId, true,
                associatedKeys, transaction, false, null, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "EndLoad")]
        public PersistenceContext EndLoadOrCreate(IAsyncResult result, out bool fromCache)
        {
            return LoadOrCreateAsyncResult.End(result, out fromCache);
        }

        public void Close()
        {
            bool needAbort = false;
            lock (ThisLock)
            {
                if (this.aborted)
                {
                    ThrowIfClosedOrAborted();
                }

                if (this.instanceCache != null)
                {
                    if (this.instanceCache.Count > 0)
                    {
                        needAbort = true;
                    }
                    else
                    {
                        this.instanceCache = null;
                    }
                }
            }
            if (needAbort)
            {
                Abort();
                ThrowIfClosedOrAborted();
                throw Fx.AssertAndThrow("Should have thrown due to abort.");
            }
        }

        public void Abort()
        {
            List<PersistenceContext> contextsToAbort = null;
            HashSet<PersistencePipeline> pipelinesToAbort = null;
            lock (ThisLock)
            {
                this.aborted = true;
                if (this.instanceCache != null)
                {
                    foreach (PersistenceContext context in this.instanceCache.Values.ToArray())
                    {
                        DetachContext(context, ref contextsToAbort);
                    }

                    Fx.Assert(this.instanceCache.Count == 0, "All instances should have been detached.");
                    Fx.Assert(this.keyMap.Count == 0, "All instances should have been removed from the key map.");

                    this.instanceCache = null;
                }
                if (this.pipelinesInUse != null)
                {
                    pipelinesToAbort = this.pipelinesInUse;
                    this.pipelinesInUse = null;
                }
            }
            AbortContexts(contextsToAbort);
            if (pipelinesToAbort != null)
            {
                foreach (PersistencePipeline pipeline in pipelinesToAbort)
                {
                    pipeline.Abort();
                }
            }

            this.throttle.Abort();
        }

        public Transaction GetTransactionForInstance(InstanceKey instanceKey)
        {
            Transaction result = null;
            PersistenceContext context;

            lock (ThisLock)
            {
                // It's okay if the instance doesn't exist. We will just return null for the transaction.
                if (this.keyMap.TryGetValue(instanceKey.Value, out context))
                {
                    result = context.LockingTransaction;
                    if (result != null)
                    {
                        // Make a clone in case the caller ends up disposing the object.
                        result = result.Clone();
                    }
                }
            }

            return result;
        }

        internal ReadOnlyCollection<BookmarkInfo> GetBookmarksForInstance(InstanceKey instanceKey)
        {
            ReadOnlyCollection<BookmarkInfo> result = null;
            PersistenceContext context;

            lock (ThisLock)
            {
                // It's okay if the instance doesn't exist. We will just return null.
                if (this.keyMap.TryGetValue(instanceKey.Value, out context))
                {
                    result = context.Bookmarks;
                }
            }

            return result;
        }

        internal bool TryAddAssociations(PersistenceContext context, IEnumerable<InstanceKey> keys, HashSet<InstanceKey> keysToAssociate, HashSet<InstanceKey> keysToDisassociate)
        {
            Fx.Assert(context != null, "TryAddAssociations cannot have a null context.");
            Fx.Assert(keys != null, "Cannot call TryAddAssociations with empty keys.");
            Fx.Assert(keysToAssociate != null, "Cannot call TryAddAssociations with null keysToAssociate.");
            // keysToDisassociate can be null if they should not be overridden by the new keys.

            List<PersistenceContext> contextsToAbort = null;
            try
            {
                lock (ThisLock)
                {
                    if (context.IsPermanentlyRemoved)
                    {
                        return false;
                    }
                    Fx.Assert(context.IsVisible, "Cannot call TryAddAssociations on an invisible context.");

                    // In the case when there is no store, if key collision is detected, the current instance will be aborted later.
                    // We should not add any of its keys to the keyMap.
                    if (this.store == null)
                    {
                        foreach (InstanceKey key in keys)
                        {
                            PersistenceContext conflictingContext;
                            if (!context.AssociatedKeys.Contains(key) && this.keyMap.TryGetValue(key.Value, out conflictingContext))
                            {
                                throw FxTrace.Exception.AsError(new InstanceKeyCollisionException(null, context.InstanceId, key, conflictingContext.InstanceId));
                            }
                        }
                    }

                    foreach (InstanceKey key in keys)
                    {
                        Fx.Assert(key.IsValid, "Cannot call TryAddAssociations with an invalid key.");

                        if (context.AssociatedKeys.Contains(key))
                        {
                            if (keysToDisassociate != null)
                            {
                                keysToDisassociate.Remove(key);
                            }
                        }
                        else
                        {
                            Fx.AssertAndThrow(this.instanceCache != null, "Since the context must be visible, it must still be in the cache.");

                            PersistenceContext contextToAbort;
                            if (this.keyMap.TryGetValue(key.Value, out contextToAbort))
                            {
                                Fx.Assert(this.store != null, "When there is no store, exception should have already been thrown before we get here.");
                                DetachContext(contextToAbort, ref contextsToAbort);
                            }
                            this.keyMap.Add(key.Value, context);
                            context.AssociatedKeys.Add(key);
                            keysToAssociate.Add(key);
                        }
                    }

                    return true;
                }
            }
            finally
            {
                AbortContexts(contextsToAbort);
            }
        }

        internal void RemoveAssociations(PersistenceContext context, IEnumerable<InstanceKey> keys)
        {
            Fx.Assert(context != null, "RemoveAssociation cannot have a null context.");
            Fx.Assert(keys != null, "Cannot call RemoveAssociation with empty keys.");

            lock (ThisLock)
            {
                if (context.IsPermanentlyRemoved)
                {
                    return;
                }
                Fx.Assert(context.IsVisible, "Cannot remove associations from a context that's not visible.");

                foreach (InstanceKey key in keys)
                {
                    if (context.AssociatedKeys.Remove(key))
                    {
                        Fx.AssertAndThrow(this.instanceCache != null, "Since the context must be visible, it must still be in the cache.");

                        Fx.Assert(this.keyMap[key.Value] == context, "Context's keys must be in the map.");
                        this.keyMap.Remove(key.Value);
                    }
                }
            }
        }

        // For transactional uses, call this method on commit.
        internal void RemoveInstance(PersistenceContext context)
        {
            RemoveInstance(context, false);
        }

        // For transactional uses, call this method on commit.
        internal void RemoveInstance(PersistenceContext context, bool permanent)
        {
            Fx.Assert(context != null, "RemoveInstance cannot have a null context.");

            lock (ThisLock)
            {
                if (permanent)
                {
                    context.IsPermanentlyRemoved = true;
                }
                DetachContext(context);
            }
        }

        internal void ReleaseThrottle()
        {
            this.throttle.Exit();
        }

        internal IAsyncResult BeginReserveThrottle(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReserveThrottleAsyncResult(this, timeout, callback, state);
        }

        internal void EndReserveThrottle(out bool ownsThrottle, IAsyncResult result)
        {
            if (result is CompletedAsyncResult)
            {
                ownsThrottle = true;
            }
            else
            {
                ownsThrottle = ReserveThrottleAsyncResult.End(result);
            }
        }

        void AbortContexts(List<PersistenceContext> contextsToAbort)
        {
            if (contextsToAbort != null)
            {
                foreach (PersistenceContext contextToAbort in contextsToAbort)
                {
                    contextToAbort.Abort();
                }
            }
        }

        // See if the instance exists in our cache
        PersistenceContext LoadFromCache(InstanceKey key, Guid suggestedIdOrId, bool canCreateInstance)
        {
            PersistenceContext foundContext = null;
            if (key != null || suggestedIdOrId != Guid.Empty)
            {
                lock (ThisLock)
                {
                    ThrowIfClosedOrAborted();

                    if (key == null)
                    {
                        this.instanceCache.TryGetValue(suggestedIdOrId, out foundContext);
                    }
                    else
                    {
                        this.keyMap.TryGetValue(key.Value, out foundContext);
                    }

                    // Done here to take advantage of the lock.
                    Fx.Assert(this.instanceCache.Count <= MaxInstances, "Too many instances in PPD.");
                }
            }
            else
            {
                Fx.Assert(canCreateInstance, "Must be able to create an instance if not addressable.");
            }

            return foundContext;
        }

        InstancePersistenceCommand CreateLoadCommandHelper(InstanceKey key, out InstanceHandle handle, bool canCreateInstance, Guid suggestedIdOrId, ICollection<InstanceKey> associatedKeys, bool loadAny)
        {
            if (loadAny)
            {
                handle = this.store.CreateInstanceHandle(this.owner);
                return new TryLoadRunnableWorkflowCommand();
            }
            else if (key != null)
            {
                LoadWorkflowByInstanceKeyCommand loadByKeyCommand;
                handle = this.store.CreateInstanceHandle(this.owner);
                if (canCreateInstance)
                {
                    loadByKeyCommand = new LoadWorkflowByInstanceKeyCommand()
                    {
                        LookupInstanceKey = key.Value,
                        AssociateInstanceKeyToInstanceId = suggestedIdOrId == Guid.Empty ? Guid.NewGuid() : suggestedIdOrId,
                        AcceptUninitializedInstance = true,
                    };
                }
                else
                {
                    loadByKeyCommand = new LoadWorkflowByInstanceKeyCommand()
                    {
                        LookupInstanceKey = key.Value,
                    };
                }
                InstanceKey lookupKeyToAdd = (canCreateInstance && key.Metadata != null && key.Metadata.Count > 0) ? key : null;
                if (associatedKeys != null)
                {
                    foreach (InstanceKey keyToAssociate in associatedKeys)
                    {
                        if (keyToAssociate == key)
                        {
                            if (!canCreateInstance)
                            {
                                continue;
                            }
                            lookupKeyToAdd = null;
                        }
                        TryAddKeyToInstanceKeysCollection(loadByKeyCommand.InstanceKeysToAssociate, keyToAssociate);
                    }
                }
                if (lookupKeyToAdd != null)
                {
                    TryAddKeyToInstanceKeysCollection(loadByKeyCommand.InstanceKeysToAssociate, lookupKeyToAdd);
                }
                return loadByKeyCommand;
            }
            else
            {
                if (associatedKeys != null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NoAdditionalKeysOnInstanceIdLoad));
                }

                handle = this.store.CreateInstanceHandle(this.owner, suggestedIdOrId == Guid.Empty ? Guid.NewGuid() : suggestedIdOrId);
                return new LoadWorkflowCommand()
                {
                    AcceptUninitializedInstance = canCreateInstance,
                };
            }
        }

        static void TryAddKeyToInstanceKeysCollection(IDictionary<Guid, IDictionary<XName, InstanceValue>> instanceKeysToAssociate, InstanceKey keyToAdd)
        {
            Fx.Assert(instanceKeysToAssociate != null, "instanceKeysToAssociate cannot be null");
            Fx.Assert(keyToAdd != null, "keyToAdd cannot be null");
            
            if (instanceKeysToAssociate.ContainsKey(keyToAdd.Value))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.DuplicateInstanceKeyExists(keyToAdd.Value)));
            }
            instanceKeysToAssociate.Add(keyToAdd.Value, keyToAdd.Metadata);
        }

        void DetachContext(PersistenceContext contextToAbort, ref List<PersistenceContext> contextsToAbort)
        {
            if (contextsToAbort == null)
            {
                contextsToAbort = new List<PersistenceContext>();
            }
            contextsToAbort.Add(contextToAbort);
            DetachContext(contextToAbort);
        }

        void DetachContext(PersistenceContext contextToAbort)
        {
            if (contextToAbort.IsVisible)
            {
                Fx.Assert(this.instanceCache != null, "All contexts should not be visible if we are closed / aborted.");

                foreach (InstanceKey key in contextToAbort.AssociatedKeys)
                {
                    Fx.Assert(this.keyMap[key.Value] == contextToAbort, "Context's key must be in the map.");
                    this.keyMap.Remove(key.Value);
                }

                try
                {
                }
                finally
                {
                    if (this.instanceCache.Remove(contextToAbort.InstanceId))
                    {
                        contextToAbort.IsVisible = false;
                        this.throttle.Exit();
                    }
                    else
                    {
                        Fx.Assert("Context must be in the cache.");
                    }
                }
            }
        }

        void ThrowIfClosedOrAborted()
        {
            if (this.instanceCache == null)
            {
                if (this.aborted)
                {
                    throw FxTrace.Exception.AsError(new OperationCanceledException(SR.DirectoryAborted));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new ObjectDisposedException(GetType().Name));
                }
            }
        }

        void RegisterPipelineInUse(PersistencePipeline pipeline)
        {
            lock (ThisLock)
            {
                if (this.aborted)
                {
                    throw FxTrace.Exception.AsError(new OperationCanceledException(SR.DirectoryAborted));
                }
                this.pipelinesInUse.Add(pipeline);
            }
        }

        void UnregisterPipelineInUse(PersistencePipeline pipeline)
        {
            lock (ThisLock)
            {
                if (!this.aborted)
                {
                    this.pipelinesInUse.Remove(pipeline);
                }
            }
        }

        AsyncWaitHandle LoadInProgressWaitHandle(InstanceKey key)
        {
            AsyncWaitHandle waitHandle = null;

            if (key != null)
            {
                lock (ThisLock)
                {
                    this.loadsInProgress.TryGetValue(key, out waitHandle);
                    if (waitHandle == null)
                    {
                        AsyncWaitHandle newWaitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                        this.loadsInProgress.Add(key, newWaitHandle);
                    }
                }
            }

            return waitHandle;
        }

        void LoadInProgressFinished(InstanceKey key)
        {
            AsyncWaitHandle waitHandle = null;

            // This may be called with a null.
            if (key != null)
            {
                lock (ThisLock)
                {
                    this.loadsInProgress.TryGetValue(key, out waitHandle);
                    if (waitHandle != null)
                    {
                        // Before we start waking up waiters, we need to remove the entry from the loadsInProgress dictionary,
                        // otherwise they would just queue up again.
                        this.loadsInProgress.Remove(key);
                    }
                }

                // Wake up any waiters outside the lock.
                if (waitHandle != null)
                {
                    waitHandle.Set();
                }
            }
        }

        class LoadOrCreateAsyncResult : TransactedAsyncResult
        {
            static Action<AsyncResult, Exception> onComplete = new Action<AsyncResult, Exception>(OnComplete);
            static AsyncCompletion handleReserveThrottle = new AsyncCompletion(HandleReserveThrottle);
            static AsyncCompletion handleExecute = new AsyncCompletion(HandleExecute);
            static Action<object> handleLoadRetry = new Action<object>(HandleLoadRetry);
            static AsyncCompletion handleLoadPipeline = new AsyncCompletion(HandleLoadPipeline);
            static AsyncCompletion handleContextEnlist = new AsyncCompletion(HandleContextEnlist);
            static Action<object, TimeoutException> handleWaitForInProgressLoad = new Action<object, TimeoutException>(HandleWaitForInProgressLoad);

            // Arguments
            readonly PersistenceProviderDirectory ppd;
            readonly InstanceKey key;
            readonly bool canCreateInstance;
            readonly ICollection<InstanceKey> associatedKeys;
            readonly TimeoutHelper timeoutHelper;
            readonly Transaction transaction;
            readonly bool loadAny;
            readonly WorkflowIdentityKey updatedIdentity;
            Guid suggestedIdOrId;

            // Global locals (the "finally" acts on their ending state)
            PersistenceContext context;
            InstanceHandle handle;
            InstanceView view;
            bool loadPending;
            List<PersistenceContext> contextsToAbort;
            PersistencePipeline pipeline;

            // Local locals (needed only to pass data through completions)
            bool isInstanceInitialized;
            bool lockInstance;

            PersistenceContext result;
            bool addedToCacheResult;

            long startTime;

            public LoadOrCreateAsyncResult(PersistenceProviderDirectory ppd, InstanceKey key, Guid suggestedIdOrId,
                bool canCreateInstance, ICollection<InstanceKey> associatedKeys, Transaction transaction, bool loadAny, WorkflowIdentityKey updatedIdentity,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Exception completionException = null;
                bool completeSelf = false;

                this.ppd = ppd;
                this.key = key;
                this.suggestedIdOrId = suggestedIdOrId;
                this.canCreateInstance = canCreateInstance;
                this.associatedKeys = associatedKeys;
                Fx.Assert(!ppd.serviceHost.IsLoadTransactionRequired || (transaction != null), "Transaction must exist!");
                this.transaction = transaction;
                this.loadAny = loadAny;
                this.updatedIdentity = updatedIdentity;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (this.associatedKeys != null && this.associatedKeys.Count == 0)
                {
                    this.associatedKeys = null;
                }

                OnCompleting = LoadOrCreateAsyncResult.onComplete;

                //
                if (this.transaction != null)
                {
                    LoadOrCreateAsyncResult.PromoteTransaction(this.transaction);
                }

                try
                {
                    if (this.LoadFromCache())
                    {
                        completeSelf = true;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    this.Complete(true, completionException);
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because we are accessing TransactionInterop.",
                Safe = "Safe because we are only accessing TransactionInterop in FullTrust.")]
            [SecuritySafeCritical]
            static void PromoteTransaction(Transaction transactionToPromote)
            {
                // TransactionInterop.GetDtcTransaction has a link demand for full trust. If we are not running in full trust, don't make the call.
                // If we are running in full trust, it is possible that we got invoked thru a cross AppDomain call from a partially trusted
                // AppDomain. So extend the demand for full trust to a full demand.
                if ((PartialTrustHelpers.AppDomainFullyTrusted) && (transactionToPromote != null))
                {
                    PermissionSet ps = new PermissionSet(PermissionState.Unrestricted);
                    ps.Demand();

                    TransactionInterop.GetDtcTransaction(transactionToPromote);
                }
            }

            public static PersistenceContext End(IAsyncResult result, out bool fromCache)
            {
                LoadOrCreateAsyncResult thisPtr = End<LoadOrCreateAsyncResult>(result);
                fromCache = !thisPtr.addedToCacheResult;

                return thisPtr.result;
            }

            static void HandleWaitForInProgressLoad(object state, TimeoutException timeoutException)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)state;
                if (timeoutException != null)
                {
                    thisPtr.Complete(false, timeoutException);
                    return;
                }

                bool completeSelf = false;
                Exception completionException = null;

                try
                {
                    if (thisPtr.LoadFromCache())
                    {
                        completeSelf = true;
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static bool HandleReserveThrottle(IAsyncResult result)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)result.AsyncState;
                thisPtr.ppd.EndReserveThrottle(out thisPtr.loadPending, result);

                thisPtr.lockInstance = thisPtr.ppd.ConsistencyScope != DurableConsistencyScope.Local || !thisPtr.canCreateInstance;

                return thisPtr.Load();
            }

            // Returns true if we found an entry in the cache, or an exception occurred. The exception is returned in the out parameter.
            bool LoadFromCache()
            {
                bool completeSelf = false;
                AsyncWaitHandle waitHandle = null;

                // We need this while loop because if we end up trying to wait for the waitHandle returned by LoadInProgressWaitHandle
                // and the call to WaitAsync returns true, then the event was signaled, but the callback was not called. So we need
                // to try the load again. But we want to avoid a potential stack overflow that might result if we just called
                // the callback routine and it called us again in a vicious cycle.
                while (true)
                {
                    // loadAny requires load from store.
                    this.result = this.loadAny ? null : this.ppd.LoadFromCache(this.key, this.suggestedIdOrId, this.canCreateInstance);

                    if (this.result != null)
                    {
                        // We found the key in the cache, so we can complete the LoadOrCreateAsyncResult.
                        completeSelf = true;
                        break;  // out of the while loop because we found the instance.
                    }
                    else if (this.ppd.store == null && !this.canCreateInstance)
                    {
                        // Fail early if the instance can't be created or loaded, no need to try to take the throttle.
                        if (this.key != null)
                        {
                            throw FxTrace.Exception.AsError(new InstanceKeyNotReadyException(null, this.key));
                        }
                        else
                        {
                            throw FxTrace.Exception.AsError(new InstanceNotReadyException(null, this.suggestedIdOrId));
                        }
                    }
                    else
                    {
                        // We didn't find it in the cache. We can't complete ourself.
                        completeSelf = false;
                        waitHandle = this.ppd.LoadInProgressWaitHandle(this.key);
                        if (waitHandle != null)   // There is another load in progress. Wait for it to complete. The waitHandle completion
                        // will do LoadFromCache again.
                        {
                            if (waitHandle.WaitAsync(handleWaitForInProgressLoad, this, this.timeoutHelper.RemainingTime()))
                            {
                                // The waitHandle is signaled. So a load must have completed between the time we called LoadInProgressWaitHandle
                                // and now. Loop back up to the top and check the cache again.
                                continue;
                            }
                        }
                        else  // there wasn't a load in progress, so we can move forward with the load.
                        {
                            IAsyncResult reserveThrottleResult = this.ppd.BeginReserveThrottle(this.timeoutHelper.RemainingTime(),
                                this.PrepareAsyncCompletion(handleReserveThrottle), this);
                            completeSelf = this.SyncContinue(reserveThrottleResult);
                        }
                    }
                    // If we get here, WaitAsync returned false, so the callback will get called later or there wasn't a load
                    // in progress, so we are moving forward to do the load. So break out of the while loop
                    break;
                }

                return completeSelf;
            }

            // If we get here, we didn't find the context in the cache.

            [Fx.Tag.SecurityNote(Critical = "Critical because it accesses UnsafeNativeMethods.QueryPerformanceCounter.",
                Safe = "Safe because we only make the call if PartialTrustHelper.AppDomainFullyTrusted is true.")]
            [SecuritySafeCritical]
            void SetStartTime()
            {
                if (PartialTrustHelpers.AppDomainFullyTrusted && (UnsafeNativeMethods.QueryPerformanceCounter(out this.startTime) == 0))
                {
                    this.startTime = -1;
                }
            }

            bool Load()
            {
                SetStartTime();

                if (this.ppd.store == null)
                {
                    Fx.Assert(this.canCreateInstance, "This case was taken care of in the constructor.");
                    Fx.Assert(!this.lockInstance, "Should not be able to try to async lock the instance if there's no factory/store.");

                    this.isInstanceInitialized = false;
                    this.context = new PersistenceContext(this.ppd, this.suggestedIdOrId == Guid.Empty ? Guid.NewGuid() : this.suggestedIdOrId, this.key, this.associatedKeys);
                    return Finish();
                }

                if (this.canCreateInstance && !this.lockInstance)
                {
                    if (this.suggestedIdOrId == Guid.Empty)
                    {
                        this.suggestedIdOrId = Guid.NewGuid();
                    }
                    this.handle = this.ppd.store.CreateInstanceHandle(this.ppd.owner, this.suggestedIdOrId);
                    this.isInstanceInitialized = false;
                    return AfterLoad();
                }

                Fx.Assert(this.lockInstance, "To get here async, lockInstance must be true.");

                InstancePersistenceCommand loadCommand = this.ppd.CreateLoadCommandHelper(this.key, out this.handle, this.canCreateInstance, this.suggestedIdOrId, this.associatedKeys, this.loadAny);
                IAsyncResult executeResult;
                try
                {
                    using (PrepareTransactionalCall(this.transaction))
                    {
                        executeResult = this.ppd.store.BeginExecute(this.handle, loadCommand, this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(LoadOrCreateAsyncResult.HandleExecute), this);
                    }
                }
                catch (InstanceHandleConflictException)
                {
                    executeResult = null;
                }
                catch (InstanceLockLostException)
                {
                    executeResult = null;
                }

                if (executeResult == null)
                {
                    return ResolveHandleConflict();
                }
                else
                {
                    return SyncContinue(executeResult);
                }
            }
            
            [Fx.Tag.SecurityNote(Critical = "Critical because it accesses UnsafeNativeMethods.QueryPerformanceCounter.",
                Safe = "Safe because we only call it if PartialTrustHelper.AppDomainFullyTrusted is true.")]
            [SecuritySafeCritical]
            long GetDuration()
            {
                long currentTime = 0;
                long duration = 0;

                if (PartialTrustHelpers.AppDomainFullyTrusted && (this.startTime >= 0) &&
                   (UnsafeNativeMethods.QueryPerformanceCounter(out currentTime) != 0))
                {
                    duration = currentTime - this.startTime;
                }
                return duration;
            }

            static bool HandleExecute(IAsyncResult result)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)result.AsyncState;

                try
                {
                    thisPtr.view = thisPtr.ppd.store.EndExecute(result);
                }
                catch (InstanceHandleConflictException)
                {
                    thisPtr.view = null;
                }
                catch (InstanceLockLostException)
                {
                    thisPtr.view = null;
                }

                if (thisPtr.view == null)
                {
                    return thisPtr.ResolveHandleConflict();
                }

                if (thisPtr.view.InstanceState == InstanceState.Unknown)
                {
                    if (thisPtr.loadAny)
                    {
                        throw FxTrace.Exception.AsError(new InstanceNotReadyException(SR.NoRunnableInstances));
                    }
                    else
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.StoreViolationNoInstanceBound));
                    }
                }
                thisPtr.isInstanceInitialized = thisPtr.view.InstanceState != InstanceState.Uninitialized;
                return thisPtr.AfterLoad();
            }

            bool ResolveHandleConflict()
            {
                Fx.Assert(this.loadPending, "How would we be here without a load pending?");

                this.result = this.loadAny ? null : this.ppd.LoadFromCache(this.key, this.suggestedIdOrId, this.canCreateInstance);
                if (this.result != null)
                {
                    return true;
                }
                else
                {
                    // A handle conflict occurred, but the instance still can't be found in the PPD's caches.  This can happen in three cases:
                    // 1.  The instance hasn't made it to the cache yet.
                    // 2.  The instance has already been removed from the cache.
                    // 3.  We're looking up by key, and a key association is in the database but not the cache because we are racing a key disassociation.
                    // All three of these cases are unstable and will resolve themselves in time, however none provide a notification that we can wait for.  So we keep
                    // trying in a loop.  This is a little scary, but should converge in all cases according to this analysis.
                    //
                    // This is issued on a new IO thread both to give the scenario some time to resolve (no use having too tight of a loop) and to avoid a stack dive.
                    ActionItem.Schedule(LoadOrCreateAsyncResult.handleLoadRetry, this);
                    return false;
                }
            }

            static void HandleLoadRetry(object state)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)state;

                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = thisPtr.Load();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    completionException = exception;
                    completeSelf = true;
                }
                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            bool AfterLoad()
            {
                if (!this.isInstanceInitialized)
                {
                    if (!this.canCreateInstance)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PersistenceViolationNoCreate));
                    }

                    if (this.view == null)
                    {
                        this.context = new PersistenceContext(this.ppd, this.ppd.store, this.handle, this.suggestedIdOrId, null, true, false, null, null);
                    }
                    else
                    {
                        this.context = new PersistenceContext(this.ppd, this.ppd.store, this.handle, this.view.InstanceId, this.view.InstanceKeys.Values.Select((keyView) => new InstanceKey(keyView.InstanceKey, keyView.InstanceKeyMetadata)), true, true, this.view, null);
                    }
                    this.handle = null;
                }
                else
                {
                    EnsureWorkflowHostType();

                    // The constructor of PersistenceContext will create the WorkflowServiceInstance in this case.
                    this.context = new PersistenceContext(this.ppd, this.ppd.store, this.handle, this.view.InstanceId, this.view.InstanceKeys.Values.Select((keyView) => new InstanceKey(keyView.InstanceKey, keyView.InstanceKeyMetadata)), false, true, this.view, this.updatedIdentity);
                    this.handle = null;

                    IEnumerable<IPersistencePipelineModule> modules = this.context.GetInstance(null).PipelineModules;
                    if (modules != null)
                    {
                        this.pipeline = new PersistencePipeline(modules);

                        this.pipeline.SetLoadedValues(this.view.InstanceData);
                        this.ppd.RegisterPipelineInUse(this.pipeline);

                        IAsyncResult loadResult;
                        using (PrepareTransactionalCall(this.transaction))
                        {
                            loadResult = this.pipeline.BeginLoad(this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(LoadOrCreateAsyncResult.handleLoadPipeline), this);
                        }
                        return SyncContinue(loadResult);
                    }
                }

                return Finish();
            }

            void EnsureWorkflowHostType()
            {
                Fx.Assert(this.view != null, "view must not be null!");
                InstanceValue instanceValue;
                if (!this.view.InstanceMetadata.TryGetValue(WorkflowNamespace.WorkflowHostType, out instanceValue))
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SRCore.NullAssignedToValueType(this.ppd.serviceHost.DurableInstancingOptions.ScopeName)));
                }

                if (!this.ppd.serviceHost.DurableInstancingOptions.ScopeName.Equals(instanceValue.Value))
                {
                    throw FxTrace.Exception.AsError(new InstancePersistenceCommandException(SRCore.IncorrectValueType(this.ppd.serviceHost.DurableInstancingOptions.ScopeName, instanceValue.Value)));
                }
            }

            static bool HandleLoadPipeline(IAsyncResult result)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)result.AsyncState;
                thisPtr.pipeline.EndLoad(result);
                return thisPtr.Finish();
            }

            [Fx.Tag.GuaranteeNonBlocking]
            bool Finish()
            {
                if (this.pipeline != null)
                {
                    this.pipeline.Publish();
                }

                // PersistenceContext.Open doesn't do anything, so it's ok to call [....].
                this.context.Open(TimeSpan.Zero);

                IAsyncResult result;
                using (PrepareTransactionalCall(this.transaction))
                {
                    result = this.context.BeginEnlist(this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(LoadOrCreateAsyncResult.handleContextEnlist), this);
                }
                return (SyncContinue(result));
            }

            static bool HandleContextEnlist(IAsyncResult result)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)result.AsyncState;
                thisPtr.context.EndEnlist(result);

                return thisPtr.AddToCache();
            }

            bool AddToCache()
            {
                Fx.Assert(!this.context.IsVisible, "Adding context which has already been added.");
                Fx.Assert(!this.context.IsPermanentlyRemoved, "Context could not already have been removed.");

                lock (this.ppd.ThisLock)
                {
                    this.ppd.ThrowIfClosedOrAborted();

                    // The InstanceStore is responsible for detecting and resolving ----s between creates.  If there is no store, we
                    // do it here, taking advantage of the lock.  We don't do it as part of the initial lookup in order to avoid
                    // holding the lock while acquiring the throttle.  Instead, we recheck here.  If we find it, we didn't need the
                    // throttle after all - it is released in cleanup (as is the PersistenceContext that got created).  If we don't
                    // find it, we add it atomically under the same lock.
                    if (this.ppd.store == null)
                    {
                        if (this.key == null)
                        {
                            this.ppd.instanceCache.TryGetValue(this.suggestedIdOrId, out this.result);
                        }
                        else
                        {
                            this.ppd.keyMap.TryGetValue(this.key.Value, out this.result);
                        }

                        if (this.result != null)
                        {
                            return true;
                        }

                        // In the case when there is no store, if key collision is detected, the current instance will be aborted later.
                        // We should not add any of its keys to the keyMap.
                        foreach (InstanceKey instanceKey in this.context.AssociatedKeys)
                        {
                            PersistenceContext conflictingContext;
                            if (this.ppd.keyMap.TryGetValue(instanceKey.Value, out conflictingContext))
                            {
                                throw FxTrace.Exception.AsError(new InstanceKeyCollisionException(null, this.context.InstanceId, instanceKey, conflictingContext.InstanceId));
                            }
                        }
                    }

                    if (!this.context.IsHandleValid)
                    {
                        // If the handle is already invalid, don't boot other instances out of the cache.
                        // If the handle is valid here, that means any PersistenceContexts in the cache under this lock
                        // must be stale - the persistence framework doesn't allow multiple valid handles.
                        throw FxTrace.Exception.AsError(new OperationCanceledException(SR.HandleFreedInDirectory));
                    }

                    this.context.IsVisible = true;

                    PersistenceContext contextToAbort;
                    if (this.ppd.instanceCache.TryGetValue(this.context.InstanceId, out contextToAbort))
                    {
                        // This is a known race condition. An instace we have loaded can get unlocked, get keys
                        // added to it, then get loaded a second time by one of the new keys.  We don't realize
                        // its happened until this point.
                        //
                        // The guarantee we give is that the old copy will be aborted before we return.  The
                        // new instance should not be processed (resumed) until after Load returns.
                        //
                        // For new instances, this could happen because the instance was deleted
                        // through a management interface or cleaned up.
                        this.ppd.DetachContext(contextToAbort, ref this.contextsToAbort);
                    }

                    foreach (InstanceKey loadedKey in this.context.AssociatedKeys)
                    {
                        if (this.ppd.keyMap.TryGetValue(loadedKey.Value, out contextToAbort))
                        {
                            Fx.Assert(this.ppd.store != null, "When there is no store, exception should have already been thrown before we get here.");
                            this.ppd.DetachContext(contextToAbort, ref this.contextsToAbort);
                        }
                        this.ppd.keyMap.Add(loadedKey.Value, this.context);
                    }

                    try
                    {
                    }
                    finally
                    {
                        this.ppd.instanceCache.Add(this.context.InstanceId, this.context);
                        this.loadPending = false;
                    }
                }

                this.addedToCacheResult = true;
                this.result = this.context;
                this.context = null;

                return true;
            }

            static void OnComplete(AsyncResult result, Exception exception)
            {
                LoadOrCreateAsyncResult thisPtr = (LoadOrCreateAsyncResult)result;

                if (thisPtr.pipeline != null)
                {
                    thisPtr.ppd.UnregisterPipelineInUse(thisPtr.pipeline);
                }
                if (thisPtr.loadPending)
                {
                    thisPtr.ppd.throttle.Exit();
                }
                if (thisPtr.context != null)
                {
                    lock (thisPtr.ppd.ThisLock)
                    {
                        thisPtr.ppd.DetachContext(thisPtr.context, ref thisPtr.contextsToAbort);
                    }
                }
                else
                {
                    if (thisPtr.handle != null)
                    {
                        thisPtr.handle.Free();
                    }
                }
                thisPtr.ppd.AbortContexts(thisPtr.contextsToAbort);

                // Wake up any LoadInProgressAsyncResult objects that may have queued up behind this load.
                thisPtr.ppd.LoadInProgressFinished(thisPtr.key);

                if (exception == null && thisPtr.addedToCacheResult)
                {
                    if (!thisPtr.isInstanceInitialized && thisPtr.canCreateInstance)
                    {
                        thisPtr.ppd.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowCreated();
                    }
                    else
                    {
                        thisPtr.ppd.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowLoaded();
                    }

                    thisPtr.ppd.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowLoadDuration(thisPtr.GetDuration());
                }

                if (exception is OperationCanceledException)
                {
                    throw FxTrace.Exception.AsError(new CommunicationObjectAbortedException(SR.LoadingAborted, exception));
                }
            }
        }

        class ReserveThrottleAsyncResult : AsyncResult
        {
            static readonly FastAsyncCallback onThrottleAcquired = new FastAsyncCallback(OnThrottleAcquired);

            readonly PersistenceProviderDirectory ppd;
            bool ownsThrottle;

            public ReserveThrottleAsyncResult(PersistenceProviderDirectory directory, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.ppd = directory;
                if (directory.throttle.EnterAsync(timeout, ReserveThrottleAsyncResult.onThrottleAcquired, this))
                {
                    this.ownsThrottle = true;
                    this.ppd.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowInMemory();
                    Complete(true);
                }
            }

            static void OnThrottleAcquired(object state, Exception asyncException)
            {
                ReserveThrottleAsyncResult thisPtr = (ReserveThrottleAsyncResult)state;
                thisPtr.ownsThrottle = asyncException == null;
                if (thisPtr.ownsThrottle)
                {
                    thisPtr.ppd.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowInMemory();
                }
                thisPtr.Complete(false, asyncException);
            }

            public static bool End(IAsyncResult result)
            {
                return End<ReserveThrottleAsyncResult>(result).ownsThrottle;
            }
        }

        class InstanceThrottle
        {
            [Fx.Tag.SynchronizationObject]
            readonly ThreadNeutralSemaphore throttle;

            int maxCount;
            int warningRestoreLimit;
            bool warningIssued;
            readonly WorkflowServiceHost serviceHost;

            public InstanceThrottle(int maxCount, WorkflowServiceHost serviceHost)
            {
                this.throttle = new ThreadNeutralSemaphore(maxCount);
                this.maxCount = maxCount;
                this.warningRestoreLimit = (int)Math.Floor(0.7 * (double)maxCount);
                this.serviceHost = serviceHost;
            }

            public bool EnterAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
            {
                bool success = this.throttle.EnterAsync(timeout, callback, state);
                if (!success)
                {
                    TraceWarning();
                }
                return success;
            }

            public void Exit()
            {
                int remainingCount = this.throttle.Exit();
                if (remainingCount < this.warningRestoreLimit)
                {
                    this.warningIssued = false;
                }
                this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowOutOfMemory();
            }

            public void Abort()
            {
                this.throttle.Abort();
            }

            void TraceWarning()
            {
                if (TD.MaxInstancesExceededIsEnabled())
                {
                    if (!this.warningIssued)
                    {
                        TD.MaxInstancesExceeded(this.maxCount);
                        this.warningIssued = true;
                    }
                }
            }
        }
    }
}
