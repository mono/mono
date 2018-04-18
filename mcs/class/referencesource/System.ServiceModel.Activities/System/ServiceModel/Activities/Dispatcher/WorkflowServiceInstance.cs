//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Activities;
    using System.Activities.DynamicUpdate;
    using System.Activities.Hosting;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Interop;
    using System.Runtime.DurableInstancing;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Diagnostics;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;
    
    // WorkflowServiceInstance is free-threaded. It is responsible for the correct locking and usage of the underlying WorkflowInstance.
    // Given that there are two simultaneous users of WorkflowInstance (WorkflowServiceInstance and Activities),
    // it is imperative that WorkflowServiceInstance only calls into WorkflowInstance when there are no activities executing
    // (and thus no worries about colliding with AEC calls).

    // LOCKING SCHEME DESCRIPTION
    // AcquireLock* - These are the only locks that should call Enter on the WorkflowExecutionLock.
    // ReleaseLock  - This is the only method that should call Exit on the WorkflowExecutionLock.
    // Lock Handoff - The lock is often handed off from one thread to another.  This is handled by
    //   WorkflowExecutionLock itself.  If there is a waiter (someone called Enter) then the Exit call
    //   will simply notify the first waiter.  The waiter is now responsible for the lock.
    //   NOTE: There is a small period of time where no one things they own the lock.  Exit has "handed
    //   off the lock by calling Set on the waiter, but the waiter has not yet executed the code
    //   which sets ownsLock to true.
    // Sync Handoff - During sync handoff the ref bool ownsLock will be set accordingly by the
    //   Acquire* method.  These methods should always be called in a try block with a finally
    //   which calls ReleaseLock.
    // Async Handoff - During async handoff the callback can assume it has the lock if either
    //   there was no exception (FastAsyncCallback) or the call to End sets the ref bool ownsLock
    //   to true.  Note that in cases of async handoff there should always be a guarding ReleaseLock
    //   which releases the lock if the async call does not state that it has gone async.
    // Scheduler Interactions - The scheduler's state MUST ONLY be changed with the activeOperationsLock
    //   held.  This is to guarantee that a Pause (Acquire) is not clobbered by a concurrently executing
    //   Resume (Release) resulting in an instance operation which times out when it shouldn't have.
    // ActiveOperations RefCount - The activeOperations ref count MUST be incremented before calling
    //   any of the Enter variations and must be decremented after leaving the Enter.  ActiveOperations
    //   is how ReleaseLock determines whether to hand the lock off to a waiting operation or to continue
    //   execution workflow when the workflow is in a runnable state.
    // Future Innovation - If necessary we can consider iterating on the current code to provide
    //   better guarantees around async handoff.  For example, at the risk of starvation we could
    //   actually exit the lock before notifying waiters rather than doing a direct handoff.
    [Fx.Tag.XamlVisible(false)]
    class WorkflowServiceInstance : WorkflowInstance
    {
        static AsyncCallback handleEndReleaseInstance;
        static FastAsyncCallback lockAcquiredAsyncCallback = new FastAsyncCallback(OnLockAcquiredAsync);
        static AsyncCallback trackCompleteDoneCallback;
        static AsyncCallback trackIdleDoneCallback;
        static AsyncCallback trackUnhandledExceptionDoneCallback;
        static ReadOnlyCollection<BookmarkInfo> emptyBookmarkInfoCollection = new ReadOnlyCollection<BookmarkInfo>(new List<BookmarkInfo>());

        WorkflowExecutionLock executorLock;

        PersistenceContext persistenceContext;
        PersistencePipeline persistencePipelineInUse;
        bool abortingExtensions;

        int activeOperations;
        object activeOperationsLock;
        int handlerThreadId;
        bool isInHandler;
        List<AsyncWaitHandle> idleWaiters;
        List<AsyncWaitHandle> nextIdleWaiters;
        List<WaitForCanPersistAsyncResult> checkCanPersistWaiters;

        // Used for synchronizing ResumeBookmark calls on the the load path from extensions (e.g DurableTimerExtension)
        AsyncWaitHandle workflowServiceInstanceReadyWaitHandle;
        bool isWorkflowServiceInstanceReady;

        // Tracking for one-time actions per instance lifetime (these end up being persisted)
        bool hasRaisedCompleted;
        bool hasPersistedDeleted;

        bool isRunnable;
        BufferedReceiveManager bufferedReceiveManager;
        State state;
        object thisLock;
        TransactionContext transactionContext;
        bool isInTransaction;
        bool isTransactedCancelled;
        Dictionary<string, List<PendingOperationAsyncResult>> pendingOperations;
        int pendingOperationCount;
        Guid instanceId;

        // Used for synchronizing unload with persist
        // This is to mark that the instance has made progress but has not been persisted by idle policy yet
        bool hasDataToPersist;

        // tracks the outstanding requests.  This contributes to idle calculations, and the list is notified
        // if workflow completes in any way (including unhandled exception)
        List<WorkflowOperationContext> pendingRequests;

        // Various Policies
        UnloadInstancePolicyHelper unloadInstancePolicy;
        UnhandledExceptionPolicyHelper unhandledExceptionPolicy;
        int referenceCount;
        ThreadNeutralSemaphore acquireReferenceSemaphore;

        WorkflowServiceHost serviceHost;
        WorkflowCreationContext creationContext;
        bool creationContextAborted;
        IDictionary<string, object> workflowOutputs;
        Exception terminationException;
        ActivityInstanceState completionState;
        TimeSpan persistTimeout;
        TimeSpan trackTimeout;
        TimeSpan acquireLockTimeout;
        
        //Tracking for increment of ASP.NET busy count
        bool hasIncrementedBusyCount;

        // dummy ctor only used to calculate IsLoadTransactionRequired
        WorkflowServiceInstance(WorkflowServiceHost serviceHost)
            : base(serviceHost.Activity)
        {
        }

        WorkflowServiceInstance(Activity workflowDefinition, WorkflowIdentity definitionIdentity, Guid instanceId, WorkflowServiceHost serviceHost, PersistenceContext persistenceContext)
            : base(workflowDefinition, definitionIdentity)
        {
            this.serviceHost = serviceHost;
            this.instanceId = instanceId;
            this.persistTimeout = serviceHost.PersistTimeout;
            this.trackTimeout = serviceHost.TrackTimeout;
            this.bufferedReceiveManager = serviceHost.Extensions.Find<BufferedReceiveManager>();

            if (persistenceContext != null)
            {
                this.persistenceContext = persistenceContext;
                this.persistenceContext.Closed += this.OnPersistenceContextClosed;
            }

            this.thisLock = new object();
            this.pendingRequests = new List<WorkflowOperationContext>();
            this.executorLock = new WorkflowExecutionLock(this);
            this.activeOperationsLock = new object();
            this.acquireReferenceSemaphore = new ThreadNeutralSemaphore(1);
            this.acquireLockTimeout = TimeSpan.MaxValue;

            // Two initial references are held:
            // The first referenceCount is owned by UnloadInstancePolicy (ReleaseInstance)
            this.referenceCount = 1;
            // The second referenceCount is owned by the loader / creator of the instance.
            this.TryAddReference();
        }

        static AsyncCallback TrackIdleDoneCallback
        {
            get
            {
                if (trackIdleDoneCallback == null)
                {
                    trackIdleDoneCallback = Fx.ThunkCallback(new AsyncCallback(OnTrackIdleDone));
                }

                return trackIdleDoneCallback;
            }
        }

        static AsyncCallback TrackUnhandledExceptionDoneCallback
        {
            get
            {
                if (trackUnhandledExceptionDoneCallback == null)
                {
                    trackUnhandledExceptionDoneCallback = Fx.ThunkCallback(new AsyncCallback(OnTrackUnhandledExceptionDone));
                }

                return trackUnhandledExceptionDoneCallback;
            }
        }

        static AsyncCallback TrackCompleteDoneCallback
        {
            get
            {
                if (trackCompleteDoneCallback == null)
                {
                    trackCompleteDoneCallback = Fx.ThunkCallback(new AsyncCallback(OnTrackCompleteDone));
                }

                return trackCompleteDoneCallback;
            }
        }

        // cache the results for perf from the extension container
        internal List<IPersistencePipelineModule> PipelineModules
        {
            get;
            private set;
        }
        
        public BufferedReceiveManager BufferedReceiveManager
        {
            get
            {
                return this.bufferedReceiveManager;
            }
        }

        public override Guid Id
        {
            get
            {
                return this.instanceId;
            }
        }

        public bool IsActive
        {
            get
            {
                return this.state == State.Active;
            }
        }

        public bool HasBeenUpdated
        {
            get;
            private set;
        }

        protected override bool SupportsInstanceKeys
        {
            get
            {
                return true;
            }
        }

        bool IsIdle
        {
            get
            {
                return this.Controller.State == WorkflowInstanceState.Idle;
            }
        }

        bool ShouldRaiseComplete
        {
            get
            {
                return this.Controller.State == WorkflowInstanceState.Complete && !this.hasRaisedCompleted;
            }
        }

        bool ShouldRaiseIdle
        {
            get
            {
                return this.IsIdle && !this.hasRaisedCompleted && this.state != State.Aborted;
            }
        }

        bool IsHandlerThread
        {
            get
            {
                return this.isInHandler && this.handlerThreadId == Thread.CurrentThread.ManagedThreadId;
            }
        }

        UnloadInstancePolicyHelper UnloadInstancePolicy
        {
            get
            {
                if (this.unloadInstancePolicy == null)
                {
                    this.unloadInstancePolicy = new UnloadInstancePolicyHelper(this, this.serviceHost.IdleTimeToPersist, this.serviceHost.IdleTimeToUnload);
                }
                return this.unloadInstancePolicy;
            }
        }

        UnhandledExceptionPolicyHelper UnhandledExceptionPolicy
        {
            get
            {
                if (this.unhandledExceptionPolicy == null)
                {
                    this.unhandledExceptionPolicy = new UnhandledExceptionPolicyHelper(this, this.serviceHost.UnhandledExceptionAction);
                }
                return this.unhandledExceptionPolicy;
            }
        }

        // create a dummy instance to configure extensions and determine if a load-time transaction is required
        public static bool IsLoadTransactionRequired(WorkflowServiceHost host)
        {
            WorkflowServiceInstance instance = new WorkflowServiceInstance(host);
            instance.RegisterExtensionManager(host.WorkflowExtensions);
            return instance.GetExtensions<IPersistencePipelineModule>().Any(module => module.IsLoadTransactionRequired);
        }

        public static WorkflowServiceInstance InitializeInstance(PersistenceContext persistenceContext, Guid instanceId, Activity workflowDefinition, WorkflowIdentity definitionIdentity, IDictionary<XName, InstanceValue> loadedObject, WorkflowCreationContext creationContext,
            SynchronizationContext synchronizationContext, WorkflowServiceHost serviceHost, DynamicUpdateMap updateMap = null)
        {
            Fx.Assert(workflowDefinition != null, "workflowDefinition cannot be null.");
            Fx.Assert(serviceHost != null, "serviceHost cannot be null!");
            Fx.Assert(instanceId != Guid.Empty, "instanceId cannot be empty.");

            WorkflowServiceInstance workflowInstance = new WorkflowServiceInstance(workflowDefinition, definitionIdentity, instanceId, serviceHost, persistenceContext)
            {
                SynchronizationContext = synchronizationContext
            };

            // let us initalize the instance level extensions here
            workflowInstance.SetupExtensions(serviceHost.WorkflowExtensions);

            if (loadedObject != null)
            {
                InstanceValue stateValue;
                object deserializedRuntimeState;

                if (!loadedObject.TryGetValue(WorkflowNamespace.Workflow, out stateValue) || stateValue.Value == null)
                {
                    throw FxTrace.Exception.AsError(
                        new InstancePersistenceException(SR.WorkflowInstanceNotFoundInStore(instanceId)));
                }
                deserializedRuntimeState = stateValue.Value;

                if (loadedObject.TryGetValue(WorkflowServiceNamespace.CreationContext, out stateValue))
                {
                    workflowInstance.creationContext = (WorkflowCreationContext)stateValue.Value;
                }

                if (persistenceContext.IsSuspended)
                {
                    workflowInstance.state = State.Suspended;
                }
                try
                {
                    workflowInstance.Initialize(deserializedRuntimeState, updateMap);
                }
                catch (InstanceUpdateException)
                {
                    // Need to flush the tracking record for the update failure
                    workflowInstance.ScheduleAbortTracking(true);
                    throw;
                }

                if (updateMap != null)
                {
                    workflowInstance.HasBeenUpdated = true;
                }
            }
            else
            {
                IList<Handle> rootExecutionProperties = null;
                IDictionary<string, object> workflowArguments = null;
                // Provide default CorrelationScope if root activity is not CorrelationScope
                if (!(workflowDefinition is CorrelationScope))
                {
                    rootExecutionProperties = new List<Handle>(1)
                    {
                        new CorrelationHandle()
                    };
                }

                if (creationContext != null)
                {
                    workflowArguments = creationContext.RawWorkflowArguments;
                    workflowInstance.creationContext = creationContext;
                }
                workflowInstance.Initialize(workflowArguments, rootExecutionProperties);
            }

            return workflowInstance;
        }

        void SetupExtensions(WorkflowInstanceExtensionManager extensionManager)
        {
            base.RegisterExtensionManager(extensionManager);

            // cache IPersistencePipelineModules
            IEnumerable<IPersistencePipelineModule> modules = base.GetExtensions<IPersistencePipelineModule>();
            int modulesCount = modules.Count<IPersistencePipelineModule>();
            if (modulesCount > 0)
            {
                this.PipelineModules = new List<IPersistencePipelineModule>(modulesCount);
                this.PipelineModules.AddRange(modules);
            }
        }

        void OnPersistenceContextClosed(object sender, EventArgs e)
        {
            if (this.persistenceContext.Aborted && !this.abortingExtensions)
            {
                AbortInstance(new FaultException(OperationExecutionFault.CreateAbortedFault(SR.DefaultAbortReason)), false);
            }
        }

        // Call when GetInstance to perform operation
        bool TryAddReference()
        {
            bool success = false;
            lock (this.thisLock)
            {
                if (this.referenceCount > 0)
                {
                    ++this.referenceCount;
                    success = true;
                }
            }
            if (success)
            {
                this.UnloadInstancePolicy.Cancel();
            }
            return success;
        }

        // Called by unload via unload policy
        bool TryReleaseLastReference()
        {
            lock (this.thisLock)
            {
                if (this.referenceCount == 1)
                {
                    this.referenceCount = 0;
                    return true;
                }
            }
            return false;
        }

        // Called when terminating ongoing unload
        void RecoverLastReference()
        {
            lock (this.thisLock)
            {
                Fx.Assert(this.referenceCount == 0, "referenceCount must be 0 during unload");
                this.referenceCount = 1;
            }
        }

        // Release after operation done
        public int ReleaseReference()
        {
            int refCount;
            lock (this.thisLock)
            {
                Fx.AssertAndThrow(this.referenceCount > 1, "referenceCount must be greater than 1");
                refCount = --this.referenceCount;
            }
            StartUnloadInstancePolicyIfNecessary();
            return refCount;
        }

        void StartUnloadInstancePolicyIfNecessary()
        {
            // The conditions to start unload policy.
            // - referenceCount is 1.  Like COM, This is the last reference count hold by WorkflowServiceInstance itself.
            //   It is incremented per command (control/resumebookmark) and decremented when command is done. 
            // - No lock pending.  In general, when referenceCount is 1, the executor lock is freed and WF is idled.  
            //   There is, however, one narrow case for Persist activity.  When it goes async (executing Sql command), 
            //   the referenceCount is decremented to 1 but WF sheduler still busy.  In this case, we will let
            //   the lock release to initiate the policy.
            // - Not in transaction (TxCommit will take care of this).
            // - Must not be in completed or unloaded or aborted states.
            // Note: it is okay to dirty read referenceCount and isLocked.  If the UnloadInstancePolicy starts before
            //   increment, the increment will correct and cancel it.  If the increment happens before, ReleaseReference
            //   will have a chance to start the policy.  Same applies to isLocked.
            if (this.referenceCount == 1 && !this.executorLock.IsLocked && !this.isInTransaction && 
                this.state != State.Completed && this.state != State.Unloaded && this.state != State.Aborted)
            {
                this.UnloadInstancePolicy.Begin();
            }
        }

        void AcquireLock(TimeSpan timeout, ref bool ownsLock)
        {
            Fx.Assert(!ownsLock, "We should never call acquire if we already think we own the lock.");

            if (this.IsHandlerThread)
            {
                // We're in a handler, on the handler thread, and doing work synchronously so we already have the lock
                return;
            }

            if (!this.executorLock.TryEnter(ref ownsLock))
            {
                Fx.Assert(!ownsLock, "This should always match the return of TryEnter and is only useful in light of exceptions");

                bool incrementedActiveOperations = false;
                object lockToken = null;

                try
                {
                    lock (this.activeOperationsLock)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.activeOperations++;
                            incrementedActiveOperations = true;
                        }

                        // An exception occuring before we call PauseScheduler causes no issues/----s since
                        // we'll just cleanup activeOperations and be in the same state as when AcquireLock
                        // was called.

                        this.Controller.RequestPause();

                        this.executorLock.SetupWaiter(ref lockToken);
                    }

                    // There is a ---- here which is solved by code in ReleaseLock.  In short, if we fail
                    // to acquire the lock here but before we decrement activeOperations the workflow pauses
                    // then nothing will ever restart the workflow.  To that end, ReleaseLock does some
                    // special handling when it exits the lock and no one is waiting.

                    this.executorLock.Enter(timeout, ref lockToken, ref ownsLock);
                }
                finally
                {
                    if (incrementedActiveOperations)
                    {
                        lock (this.activeOperationsLock)
                        {
                            this.activeOperations--;
                        }
                    }

                    this.executorLock.CleanupWaiter(lockToken, ref ownsLock);
                }
            }
        }

        bool AcquireLockAsync(TimeSpan timeout, ref bool ownsLock, FastAsyncCallback callback, object state)
        {
            return AcquireLockAsync(timeout, false, false, ref ownsLock, callback, state);
        }

        bool AcquireLockAsync(TimeSpan timeout, bool isAbortPriority, bool skipPause, ref bool ownsLock, FastAsyncCallback callback, object state)
        {
            Fx.Assert(!ownsLock, "We should never call acquire if we already think we own the lock.");

            // We cannot just hand off the lock if we are in a handler thread
            // because this might eventually go async (during the operation)
            // and we could have multiple operations occurring concurrently.

            if (!this.executorLock.TryEnter(ref ownsLock))
            {
                Fx.Assert(!ownsLock, "This should always match the return of TryEnter and is only useful in light of exceptions");

                bool incrementedActiveOperations = false;
                bool decrementActiveOperations = true;
                object lockToken = null;

                try
                {
                    lock (this.activeOperationsLock)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.activeOperations++;
                            incrementedActiveOperations = true;
                        }

                        // An exception occuring before we call PauseScheduler causes no issues/----s since
                        // we'll just cleanup activeOperations and be in the same state as when AcquireLock
                        // was called.

                        if (!skipPause)
                        {
                            this.Controller.RequestPause();
                        }

                        this.executorLock.SetupWaiter(isAbortPriority, ref lockToken);
                    }

                    // If we get the lock here then we should decrement, otherwise
                    // it is up to the lock acquired callback
                    decrementActiveOperations = this.executorLock.EnterAsync(timeout, ref lockToken, ref ownsLock, lockAcquiredAsyncCallback, new AcquireLockAsyncData(this, callback, state));
                    return decrementActiveOperations;
                }
                finally
                {
                    if (incrementedActiveOperations && decrementActiveOperations)
                    {
                        lock (this.activeOperationsLock)
                        {
                            this.activeOperations--;
                        }
                    }

                    this.executorLock.CleanupWaiter(lockToken, ref ownsLock);
                }
            }
            else
            {
                return true;
            }
        }

        static void OnLockAcquiredAsync(object state, Exception asyncException)
        {
            AcquireLockAsyncData data = (AcquireLockAsyncData)state;

            lock (data.Instance.activeOperationsLock)
            {
                data.Instance.activeOperations--;
            }

            data.Callback(data.State, asyncException);
        }

        AsyncWaitHandle SetupIdleWaiter(ref bool ownsLock)
        {
            AsyncWaitHandle idleEvent = new AsyncWaitHandle(EventResetMode.ManualReset);

            lock (this.activeOperationsLock)
            {
                if (this.idleWaiters == null)
                {
                    this.idleWaiters = new List<AsyncWaitHandle>();
                }

                this.idleWaiters.Add(idleEvent);
            }

            ReleaseLock(ref ownsLock);

            return idleEvent;
        }

        bool CleanupIdleWaiter(AsyncWaitHandle idleEvent, Exception waitException, ref bool ownsLock)
        {
            lock (this.activeOperationsLock)
            {
                if (!this.idleWaiters.Remove(idleEvent))
                {
                    // If it wasn't in the list that means we raced between throwing from Wait
                    // and setting the event.  This thread now is responsible for the lock.
                    if (waitException is TimeoutException)
                    {
                        // In the case of Timeout we let setting the event win and signal to
                        // swallow the exception

                        ownsLock = true;
                        return false;
                    }
                }
            }

            return true;
        }

        // Called with the executor lock
        // Returns true if someone was notified (this thread no longer owns the lock) or false if
        // no one was notified.
        bool NotifyNextIdleWaiter(ref bool ownsLock)
        {
            // If we are no longer active, flush all idle waiters (next + current) because we will
            // not enter Idle state again.  For Suspended, even we could ---- to unsuspend and become idle,
            // the desirable behavior while suspending is to reject pending as well as new requests.
            if (this.state != State.Active)
            {
                PrepareNextIdleWaiter();
            }

            if (this.idleWaiters != null && this.idleWaiters.Count > 0)
            {
                // We need to be careful about setting this event because if there is an async
                // waiter then this thread will be used for some execution.  Therefore we shouldn't
                // call set with the activeOperationsLock held.
                AsyncWaitHandle idleEvent = null;

                // We need to lock this because a waiter might have timed out (or thrown another exception) and
                // could be trying to remove itself from the list without the executor lock.
                lock (this.activeOperationsLock)
                {
                    if (this.idleWaiters.Count > 0)
                    {
                        idleEvent = this.idleWaiters[0];
                        this.idleWaiters.RemoveAt(0);
                    }
                }

                if (idleEvent != null)
                {
                    idleEvent.Set();
                    ownsLock = false;
                    return true;
                }
            }

            return false;
        }

        void PrepareNextIdleWaiter()
        {
            if (this.nextIdleWaiters != null && this.nextIdleWaiters.Count > 0)
            {
                lock (this.activeOperationsLock)
                {
                    if (this.idleWaiters == null)
                    {
                        this.idleWaiters = new List<AsyncWaitHandle>();
                    }

                    for (int i = 0; i < this.nextIdleWaiters.Count; i++)
                    {
                        this.idleWaiters.Add(this.nextIdleWaiters[i]);
                    }
                }

                this.nextIdleWaiters.Clear();
            }
        }

        IAsyncResult BeginAcquireLockOnIdle(TimeSpan timeout, ref bool ownsLock, AsyncCallback callback, object state)
        {
            return new AcquireLockOnIdleAsyncResult(this, timeout, ref ownsLock, callback, state);
        }

        void EndAcquireLockOnIdle(IAsyncResult result)
        {
            Fx.Assert(result.CompletedSynchronously, "This overload should only be called when completed synchronously.");
            AcquireLockOnIdleAsyncResult.End(result);
        }

        void EndAcquireLockOnIdle(IAsyncResult result, ref bool ownsLock)
        {
            Fx.Assert(!result.CompletedSynchronously, "This overload should only be called when completed asynchronously.");
            AcquireLockOnIdleAsyncResult.End(result, ref ownsLock);
        }

        void ReleaseLock(ref bool ownsLock)
        {
            ReleaseLock(ref ownsLock, false);
        }

        void ReleaseLock(ref bool ownsLock, bool hasBeenPersistedByIdlePolicy)
        {
            // The hasBeenPersistedByIdlePolicy flag is only true when this is part of the idle policy initiated persist.

            if (!ownsLock)
            {
                return;
            }

            Fx.Assert(!this.IsHandlerThread, "We never set ownsLock if we are on the handler thread and therefore should have shortcut out earlier.");

            bool resumeScheduler = false;

            bool needToSignalWorkflowServiceInstanceReadyWaitHandle = false;
            lock (this.thisLock)
            {
                this.isWorkflowServiceInstanceReady = true;
                if (this.workflowServiceInstanceReadyWaitHandle != null)
                {
                    needToSignalWorkflowServiceInstanceReadyWaitHandle = true; 
                }

                // Signal that workflow has made progress and this progress has not been persisted by idle policy,
                // we need to supress the abort initiated by unload when TimeToPersist < TimeToUnload.
                // If ReleaseLock is done by anyone other than idle policy persist, we mark the instance dirty.
                // Conversely, if idle policy completed a persist, we mark the instance clean.
                this.hasDataToPersist = !hasBeenPersistedByIdlePolicy;
            }

            if (needToSignalWorkflowServiceInstanceReadyWaitHandle)
            {
                this.workflowServiceInstanceReadyWaitHandle.Set();
            }

            lock (this.activeOperationsLock)
            {
                // We don't check for completion here because we need to make sure we always
                // drain the scheduler queue.  Note that the OnIdle handler only raises events
                // if the workflow is truly idle.  Therefore, if we are completed but not idle
                // then we won't raise the events.
                // Terminate capitalizes on this by assuring that there is at least one more
                // work item in the queue.  This provides a simple mechanism for getting a
                // scheduler thread to raise the completed event.
                bool isRunnable = this.state == State.Active && this.isRunnable && !this.IsIdle;
                if (isRunnable && this.activeOperations == 0)
                {
                    ownsLock = false;
                    resumeScheduler = true;
                }
                else if ((this.IsIdle || this.state != State.Active) && NotifyNextIdleWaiter(ref ownsLock))
                {
                }
                else
                {
                    // If we are runnable then we want to hang onto the lock if Exit finds no one waiting.
                    if (!this.executorLock.Exit(isRunnable, ref ownsLock))
                    {
                        // No one was waiting, but we had activeOperations (otherwise we would not have gotten
                        // to this branch of the if).  This means that we raced with a timeout and should resume
                        // the workflow's execution.  If we don't resume execution we'll just hang ... no one
                        // has the lock, the workflow is ready to execute, but it is not.
                        Fx.Assert(this.activeOperations > 0, "We should always have active operations otherwise we should have taken a different code path.");

                        // We no longer "own" the lock because the scheduler has taken control
                        ownsLock = false;

                        resumeScheduler = true;
                    }
                }
            }

            if (resumeScheduler)
            {
                IncrementBusyCount();
                this.persistenceContext.Bookmarks = null;
                this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowExecuting(true);
                if (this.Controller.State == WorkflowInstanceState.Complete)
                {
                    OnNotifyPaused();
                }
                else
                {
                    this.Controller.Run();
                    
                }
            }
        }

        public IAsyncResult BeginAbandon(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(reason != null, "reason must not be null!");
            return BeginAbandon(reason, true, timeout, callback, state);
        }

        //used by UnloadPolicy when TimeToUnload > TimeToPersist to prevent an Abort tracking record.
        IAsyncResult BeginAbandon(Exception reason, bool shouldTrackAbort, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return AbandonAsyncResult.Create(this, reason, shouldTrackAbort, timeout, callback, state);
        }

        public void EndAbandon(IAsyncResult result)
        {
            AbandonAsyncResult.End(result);
        }

        IAsyncResult BeginAbandonAndSuspend(Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(reason != null, "reason must not be null!");
            return AbandonAndSuspendAsyncResult.Create(this, reason, timeout, callback, state);
        }

        void EndAbandonAndSuspend(IAsyncResult result)
        {
            AbandonAndSuspendAsyncResult.End(result);
        }

        void AbortInstance(Exception reason, bool isWorkflowThread)
        {
            AbortInstance(reason, isWorkflowThread, true);
        }

        void AbortInstance(Exception reason, bool isWorkflowThread, bool shouldTrackAbort)
        {
            bool completeSelf = false;

            if (shouldTrackAbort)
            {
                FxTrace.Exception.AsWarning(reason); 
            }

            FaultPendingRequests(reason);

            AbortExtensions();

            try
            {
                if (this.creationContext != null && !this.creationContextAborted)
                {
                    this.creationContextAborted = true;
                    this.creationContext.OnAbort();
                }

                if (isWorkflowThread)
                {
                    completeSelf = true;
                    if (ValidateStateForAbort())
                    {
                        this.state = State.Aborted;
                         if (shouldTrackAbort)
                        {
                            this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowAborted();
                            this.Controller.Abort(reason);
                        }
                        else
                        {
                            // this ensures that reason is null when WorkflowInstance.Abort is called
                            // and prevents an Abort tracking record.
                            this.Controller.Abort();
                        }
                        DecrementBusyCount();

                        // We should get off this thread because we're unsure of its state
                        ScheduleAbortTracking(false);
                    }
                }
                else
                {
                    bool ownsLock = false;

                    try
                    {
                        if (AcquireLockAsync(this.acquireLockTimeout, true, false, ref ownsLock, new FastAsyncCallback(OnAbortLockAcquired),
                            new AbortInstanceState(reason, shouldTrackAbort)))
                        {
                            completeSelf = true;
                            if (ValidateStateForAbort())
                            {
                                this.state = State.Aborted;
                                if (shouldTrackAbort)
                                {
                                    this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowAborted();
                                    this.Controller.Abort(reason);
                                }
                                else
                                {
                                    // this ensures that reason is null when WorkflowInstance.Abort is called
                                    // and prevents an Abort tracking record.
                                    this.Controller.Abort();
                                }
                                DecrementBusyCount();

                                // We need to get off this thread so we don't block the caller
                                // of abort
                                ScheduleAbortTracking(false);
                            }
                        }
                    }
                    finally
                    {
                        if (completeSelf)
                        {
                            ReleaseLock(ref ownsLock);
                        }
                    }
                }
            }
            finally
            {
                this.serviceHost.FaultServiceHostIfNecessary(reason);
            }
        }

        void AbortExtensions()
        {
            this.abortingExtensions = true;

            // Need to ensure that either components see the Aborted state, this method sees the components, or both.
            Thread.MemoryBarrier();

            if (this.persistenceContext != null)
            {
                this.persistenceContext.Abort();
            }

            PersistencePipeline currentPersistencePipeline = this.persistencePipelineInUse;
            if (currentPersistencePipeline != null)
            {
                currentPersistencePipeline.Abort();
            }

            // We abandon buffered Receives only in the complete code path, not in abort code path.
            if (this.hasRaisedCompleted && this.bufferedReceiveManager != null)
            {
                this.bufferedReceiveManager.AbandonBufferedReceives(this.persistenceContext.AssociatedKeys);
            }

        }

        void Dispose()
        {
            this.DisposeExtensions();

            // We abandon buffered Receives only in the complete code path, not in abort code path.
            if (this.hasRaisedCompleted && this.bufferedReceiveManager != null)
            {
                this.bufferedReceiveManager.AbandonBufferedReceives(this.persistenceContext.AssociatedKeys);
            }
        }
        
        void OnAbortLockAcquired(object state, Exception exception)
        {
            if (exception != null)
            {
                // We ---- this exception because we were simply doing our
                // best to get the lock.  Note that we won't proceed without
                // the lock because we may have already succeeded on another
                // thread.  Technically this abort call has failed.

                FxTrace.Exception.AsWarning(exception);
                return;
            }

            bool ownsLock = true;
            bool shouldRaise = false;
            AbortInstanceState abortInstanceState = (AbortInstanceState)state;

            try
            {
                if (ValidateStateForAbort())
                {
                    shouldRaise = true;
                    this.state = State.Aborted;
                    if (abortInstanceState.ShouldTrackAbort)
                    {
                        this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowAborted();
                        this.Controller.Abort(abortInstanceState.Reason);
                    }
                    else
                    {
                        // this ensures that reason is null when WorkflowInstance.Abort is called
                        // and prevents an Abort tracking record.
                        this.Controller.Abort();
                    }
                    DecrementBusyCount();
                }
            }
            finally
            {
                ReleaseLock(ref ownsLock);
            }

            if (shouldRaise)
            {
                // We call this from this thread because we've already
                // had a thread switch
                TrackAbort(false);
            }
        }

        void ScheduleAbortTracking(bool isUpdateFailure)
        {
            ActionItem.Schedule(new Action<object>(TrackAbort), isUpdateFailure);
        }

        // This is only ever called from an appropriate thread (not the thread
        // that called abort unless it was an internal abort).
        // This method is called without the lock.  We still provide single threaded
        // guarantees to the WorkflowInstance because:
        //    * No other call can ever enter the executor again once the state has
        //      switched to Aborted
        //    * If this was an internal abort then the thread was fast pathing its
        //      way out of the runtime and won't conflict
        // Or, in the case of a DynamicUpdate failure, the WorkflowInstance is
        // never returned from the factory method, and so will never be acessed by
        // another thread.
        void TrackAbort(object state)
        {
            bool isUpdateFailure = (bool)state;

            if (isUpdateFailure || this.Controller.HasPendingTrackingRecords)
            {
                try
                {
                    IAsyncResult result = this.BeginFlushTrackingRecords(this.trackTimeout, Fx.ThunkCallback(new AsyncCallback(OnAbortTrackingComplete)), isUpdateFailure);

                    if (result.CompletedSynchronously)
                    {
                        this.Controller.EndFlushTrackingRecords(result);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    // We ---- any exception here because we are on the abort path
                    // and are doing a best effort to track this record.
                    FxTrace.Exception.AsWarning(e);
                }
            }

            if (!isUpdateFailure)
            {
                RaiseAborted();
            }
        }

        void OnAbortTrackingComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            bool isUpdateFailure = (bool)result.AsyncState;

            try
            {
                this.EndFlushTrackingRecords(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                // We ---- any exception here because we are on the abort path
                // and are doing a best effort to track this record.
                FxTrace.Exception.AsWarning(e);
            }

            if (!isUpdateFailure)
            {
                RaiseAborted();
            }
        }

        void RaiseAborted()
        {
            this.UnloadInstancePolicy.Cancel();
            CompletePendingOperations();
        }

        public IAsyncResult BeginTerminate(string reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(!String.IsNullOrEmpty(reason), "reason string must not be null or empty!");

            // the FaultException below is created using the FaultException(FaultReason, FaultCode) ctor instead of the FaultException(MessageFault) ctor
            // because the latter ctor saves the fault in its fault member.  Saving the fault is problematic because faultException would serialize its 
            // fault member and operationExecutionFault is not serializable.  The faultException might need to be serialized if the workflowServiceInstance
            // is ever persisted since the faultException below ultimately becomes the terminationException saved with the workflowServiceInstance.
            OperationExecutionFault fault = OperationExecutionFault.CreateTerminatedFault(reason);
            return BeginTerminate(new FaultException(fault.Reason, fault.Code), transaction, timeout, callback, state);
        }

        IAsyncResult BeginTerminate(Exception reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(reason != null, "reason must not be null!");
            return TerminateAsyncResult.Create(this, reason, transaction, timeout, callback, state);
        }

        public void EndTerminate(IAsyncResult result)
        {
            this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowTerminated();
            TerminateAsyncResult.End(result);
        }

        public IAsyncResult BeginCancel(Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return CancelAsyncResult.Create(this, transaction, timeout, callback, state);
        }

        public void EndCancel(IAsyncResult result)
        {
            CancelAsyncResult.End(result);
        }

        void RunCore()
        {
            this.isRunnable = true;
            this.state = State.Active;
        }

        public IAsyncResult BeginRun(Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginRun(transaction, null, timeout, callback, state);
        }

        public IAsyncResult BeginRun(Transaction transaction, string operationName, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return RunAsyncResult.Create(this, transaction, operationName, timeout, callback, state);
        }

        public void EndRun(IAsyncResult result)
        {
            RunAsyncResult.End(result);
        }

        protected override void OnNotifyPaused()
        {
            bool ownsLock = true;
            bool keepLock = false;

            try
            {
                this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowExecuting(false);
                if (ShouldRaiseComplete)
                {
                    PrepareNextIdleWaiter();
                    
                    Exception abortException = null;

                    try
                    {
                        // We're about to notify the world that this instance is completed
                        // so let's make it official.
                        this.hasRaisedCompleted = true;
                        this.state = State.Completed;
                        GetCompletionState();
                        if (this.completionState == ActivityInstanceState.Closed)
                        {
                            this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowCompleted();
                        }
                        
                        if (this.Controller.HasPendingTrackingRecords)
                        {
                            IAsyncResult result = this.Controller.BeginFlushTrackingRecords(this.trackTimeout, TrackCompleteDoneCallback, this);

                            if (result.CompletedSynchronously)
                            {
                                this.Controller.EndFlushTrackingRecords(result);
                            }
                            else
                            {
                                keepLock = true;
                                return;
                            }
                        }

                        this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;

                        try
                        {
                            this.isInHandler = true;
                            OnCompleted();
                        }
                        finally
                        {
                            this.isInHandler = false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        abortException = e;
                    }

                    if (abortException != null)
                    {
                        AbortInstance(abortException, true);
                    }
                }
                else if (this.Controller.State == WorkflowInstanceState.Aborted)
                {
                    Exception abortReason = this.Controller.GetAbortReason();
                    this.AbortInstance(abortReason, true);
                }
                else if (ShouldRaiseIdle)
                {
                    this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowIdle();

                    PrepareNextIdleWaiter();

                    if (this.Controller.TrackingEnabled)
                    {
                        this.Controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Idle, this.DefinitionIdentity));
                        IAsyncResult result = this.Controller.BeginFlushTrackingRecords(this.trackTimeout, TrackIdleDoneCallback, this);

                        if (result.CompletedSynchronously)
                        {
                            this.Controller.EndFlushTrackingRecords(result);
                        }
                        else
                        {
                            keepLock = true;
                            return;
                        }
                    }

                    this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;

                    try
                    {
                        this.isInHandler = true;
                        OnIdle();
                    }
                    finally
                    {
                        this.isInHandler = false;
                    }
                }
                else
                {
                    NotifyCheckCanPersistWaiters(ref ownsLock);
                }
            }
            finally
            {
                if (!keepLock)
                {
                    ReleaseLock(ref ownsLock);
                }
            }
        }

        // Note: this is runtime generated Abort such as Transaction failure
        protected override void OnRequestAbort(Exception reason)
        {
            AbortInstance(reason, false);
        }

        static void OnTrackCompleteDone(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            WorkflowServiceInstance thisPtr = (WorkflowServiceInstance)result.AsyncState;
            bool ownsLock = true;

            try
            {
                thisPtr.Controller.EndFlushTrackingRecords(result);

                thisPtr.handlerThreadId = Thread.CurrentThread.ManagedThreadId;

                try
                {
                    thisPtr.isInHandler = true;
                    thisPtr.OnCompleted();
                }
                finally
                {
                    thisPtr.isInHandler = false;
                }
            }
            finally
            {
                thisPtr.ReleaseLock(ref ownsLock);
            }
        }

        static void OnTrackIdleDone(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            WorkflowServiceInstance thisPtr = (WorkflowServiceInstance)result.AsyncState;
            bool ownsLock = true;

            try
            {
                thisPtr.Controller.EndFlushTrackingRecords(result);

                thisPtr.handlerThreadId = Thread.CurrentThread.ManagedThreadId;

                try
                {
                    thisPtr.isInHandler = true;
                    thisPtr.OnIdle();
                }
                finally
                {
                    thisPtr.isInHandler = false;
                }
            }
            finally
            {
                thisPtr.ReleaseLock(ref ownsLock);
            }
        }

        protected override void OnNotifyUnhandledException(Exception exception, Activity exceptionSource, 
            string exceptionSourceInstanceId)
        {
            bool ownsLock = true;
            bool keepLock = false;
            UnhandledExceptionAsyncData data = new UnhandledExceptionAsyncData(this, exception, exceptionSource);

            try
            {
                if (this.Controller.HasPendingTrackingRecords)
                {
                    IAsyncResult result = this.Controller.BeginFlushTrackingRecords(this.trackTimeout, TrackUnhandledExceptionDoneCallback, data);

                    if (result.CompletedSynchronously)
                    {
                        this.Controller.EndFlushTrackingRecords(result);
                    }
                    else
                    {
                        keepLock = true;
                        return;
                    }
                }

                this.handlerThreadId = Thread.CurrentThread.ManagedThreadId;

                try
                {
                    this.isInHandler = true;
                    OnUnhandledException(data);
                }
                finally
                {
                    this.isInHandler = false;
                }
            }
            finally
            {
                if (!keepLock)
                {
                    ReleaseLock(ref ownsLock);
                }
            }
        }

        static void OnTrackUnhandledExceptionDone(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            UnhandledExceptionAsyncData data = (UnhandledExceptionAsyncData)result.AsyncState;
            WorkflowServiceInstance thisPtr = data.Instance;

            bool ownsLock = true;

            try
            {
                thisPtr.Controller.EndFlushTrackingRecords(result);

                thisPtr.handlerThreadId = Thread.CurrentThread.ManagedThreadId;

                try
                {
                    thisPtr.isInHandler = true;
                    thisPtr.OnUnhandledException(data);
                }
                finally
                {
                    thisPtr.isInHandler = false;
                }
            }
            finally
            {
                thisPtr.ReleaseLock(ref ownsLock);
            }
        }

        public IAsyncResult BeginSuspend(bool isUnlocked, string reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return SuspendAsyncResult.Create(this, isUnlocked, reason, transaction, timeout, callback, state);
        }

        public void EndSuspend(IAsyncResult result)
        {
            SuspendAsyncResult.End(result);
        }

        public IAsyncResult BeginUnsuspend(Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return UnsuspendAsyncResult.Create(this, transaction, timeout, callback, state);
        }

        public void EndUnsuspend(IAsyncResult result)
        {
            UnsuspendAsyncResult.End(result);
        }

        void GetCompletionState()
        {
            this.completionState = this.Controller.GetCompletionState(out this.workflowOutputs, out this.terminationException);
        }

        void TrackPersistence(PersistenceOperation operation)
        {
            if (this.Controller.TrackingEnabled)
            {
                if (operation == PersistenceOperation.Delete)
                {
                    this.Controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Deleted, this.DefinitionIdentity));
                }
                else if (operation == PersistenceOperation.Unload)
                {
                    this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowUnloaded();
                    this.Controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Unloaded, this.DefinitionIdentity));
                }
                else
                {
                    this.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowPersisted();
                    this.Controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Persisted, this.DefinitionIdentity));
                }
            }
        }

        Dictionary<XName, InstanceValue> GeneratePersistenceData()
        {
            Dictionary<XName, InstanceValue> data = new Dictionary<XName, InstanceValue>(10);
            data[WorkflowNamespace.Bookmarks] = new InstanceValue(Controller.GetBookmarks(), InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            data[WorkflowNamespace.LastUpdate] = new InstanceValue(DateTime.UtcNow, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);

            foreach (KeyValuePair<string, LocationInfo> mappedVariable in Controller.GetMappedVariables())
            {
                data[WorkflowNamespace.VariablesPath.GetName(mappedVariable.Key)] = new InstanceValue(mappedVariable.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
            }           

            Fx.AssertAndThrow(Controller.State != WorkflowInstanceState.Aborted, "Cannot generate data for an aborted service instance.");
            if (Controller.State != WorkflowInstanceState.Complete)
            {
                data[WorkflowNamespace.Workflow] = new InstanceValue(Controller.PrepareForSerialization());

                if (this.creationContext != null)
                {
                    data[WorkflowServiceNamespace.CreationContext] = new InstanceValue(this.creationContext);
                }

                data[WorkflowNamespace.Status] = new InstanceValue(Controller.State == WorkflowInstanceState.Idle ? "Idle" : "Executing", InstanceValueOptions.WriteOnly);
            }
            else
            {
                data[WorkflowNamespace.Workflow] = new InstanceValue(Controller.PrepareForSerialization(), InstanceValueOptions.Optional);

                this.GetCompletionState();

                if (this.completionState == ActivityInstanceState.Faulted)
                {
                    data[WorkflowNamespace.Status] = new InstanceValue("Faulted", InstanceValueOptions.WriteOnly);
                    data[WorkflowNamespace.Exception] = new InstanceValue(this.terminationException, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                }
                else if (this.completionState == ActivityInstanceState.Closed)
                {
                    data[WorkflowNamespace.Status] = new InstanceValue("Closed", InstanceValueOptions.WriteOnly);
                    if (this.workflowOutputs != null)
                    {
                        foreach (KeyValuePair<string, object> output in this.workflowOutputs)
                        {
                            data[WorkflowNamespace.OutputPath.GetName(output.Key)] = new InstanceValue(output.Value, InstanceValueOptions.WriteOnly | InstanceValueOptions.Optional);
                        }
                    }
                }
                else
                {
                    Fx.AssertAndThrow(this.completionState == ActivityInstanceState.Canceled, "Cannot be executing a service instance when WorkflowState was completed.");
                    data[WorkflowNamespace.Status] = new InstanceValue("Canceled", InstanceValueOptions.WriteOnly);
                }
            }
            return data;
        }

        public IAsyncResult BeginPersist(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginPersist(false, timeout, callback, state);
        }

        IAsyncResult BeginPersist(bool isTry, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new UnloadOrPersistAsyncResult(this, this.Controller.State == WorkflowInstanceState.Complete ? PersistenceOperation.Delete : PersistenceOperation.Save, false, isTry, 
                timeout, callback, state);
        }

        public bool EndPersist(IAsyncResult result)
        {
            return UnloadOrPersistAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginFlushTrackingRecords(AsyncCallback callback, object state)
        {
            return this.Controller.BeginFlushTrackingRecords(this.trackTimeout, callback, state);
        }

        protected override void OnEndFlushTrackingRecords(IAsyncResult result)
        {
            this.Controller.EndFlushTrackingRecords(result);
        }

        protected override IAsyncResult OnBeginPersist(AsyncCallback callback, object state)
        {
            return new UnloadOrPersistAsyncResult(this, PersistenceOperation.Save, true, false, TimeSpan.MaxValue, callback, state);
        }

        protected override void OnEndPersist(IAsyncResult result)
        {
            UnloadOrPersistAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginAssociateKeys(ICollection<InstanceKey> keys, AsyncCallback callback, object state)
        {
            if (this.persistenceContext == null)
            {
                return new CompletedAsyncResult(callback, state);
            }
            else
            {
                return this.persistenceContext.BeginAssociateKeys(keys, this.persistTimeout, callback, state);
            }
        }

        protected override void OnEndAssociateKeys(IAsyncResult result)
        {
            if (this.persistenceContext == null)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.persistenceContext.EndAssociateKeys(result);
            }
        }

        protected override void OnDisassociateKeys(ICollection<InstanceKey> keys)
        {
            if (persistenceContext != null)
            {
                this.persistenceContext.DisassociateKeys(keys);
            }
        }

        BookmarkResumptionResult ResumeProtocolBookmarkCore(Bookmark bookmark, object value, BookmarkScope bookmarkScope, bool bufferedReceiveEnabled, ref AsyncWaitHandle waitHandle, ref bool ownsLock)
        {
            Fx.Assert(this.state == State.Active, "WorkflowServiceInstance.State should be State.Active at this point.");

            BookmarkResumptionResult result;
            if (bookmarkScope == null)
            {
                result = this.Controller.ScheduleBookmarkResumption(bookmark, value);
            }
            else
            {
                result = this.Controller.ScheduleBookmarkResumption(bookmark, value, bookmarkScope);
            }

            if (result == BookmarkResumptionResult.NotReady && !bufferedReceiveEnabled && (this.serviceHost.FilterResumeTimeout.TotalSeconds > 0))
                {
                if (waitHandle == null)
                {
                    waitHandle = new AsyncWaitHandle();
                }
                else
                {
                    waitHandle.Reset();
                }

                // Creation doesn't require the lock since it is guarded
                // by the executor lock.
                if (this.nextIdleWaiters == null)
                {
                    this.nextIdleWaiters = new List<AsyncWaitHandle>();
                }

                lock (this.activeOperationsLock)
                {
                    this.nextIdleWaiters.Add(waitHandle);
                }

                // We release the lock here so that the workflow will continue to process
                // until the NextIdle waiters get notified
                ReleaseLock(ref ownsLock);
            }

            return result;
        }

        [Fx.Tag.Throws(typeof(TimeoutException), "Either the execution lock could not be acquired or the target sub-instance did not become stable in the allotted time.")]
        public IAsyncResult BeginResumeProtocolBookmark(Bookmark bookmark, BookmarkScope bookmarkScope, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(bookmark != null, "bookmark must not be null!");

            object bookmarkValue = value;
            WorkflowOperationContext context = value as WorkflowOperationContext;
            if (context != null)
            {
                if (!context.HasResponse)
                {
                    lock (this.thisLock)
                    {
                        this.pendingRequests.Add(context);
                    }
                }
                bookmarkValue = context.BookmarkValue;
            }

            return new ResumeProtocolBookmarkAsyncResult(this, bookmark, bookmarkValue, bookmarkScope, true, timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "ResumeProtocolBookmark")]
        public BookmarkResumptionResult EndResumeProtocolBookmark(IAsyncResult result)
        {
            return ResumeProtocolBookmarkAsyncResult.End(result);
        }

        protected override IAsyncResult OnBeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ResumeProtocolBookmarkAsyncResult(this, bookmark, value, null, false, timeout, callback, state);
        }

        protected override BookmarkResumptionResult OnEndResumeBookmark(IAsyncResult result)
        {
            return ResumeProtocolBookmarkAsyncResult.End(result);
        }

        void MarkUnloaded()
        {
            this.state = State.Unloaded;

            // don't abort completed instances
            if (this.Controller.State != WorkflowInstanceState.Complete)
            {
                this.Controller.Abort();
            }

            DecrementBusyCount();
        }

        // This always happens under executor lock
        void AddCheckCanPersistWaiter(WaitForCanPersistAsyncResult result)
        {
            // Creation doesn't require the lock since it is guarded
            // by the executor lock.
            if (this.checkCanPersistWaiters == null)
            {
                this.checkCanPersistWaiters = new List<WaitForCanPersistAsyncResult>();
            }
            this.checkCanPersistWaiters.Add(result);
        }

        // This always happens under executor lock
        void NotifyCheckCanPersistWaiters(ref bool ownsLock)
        {
            // Always guarded by the executor lock.
            if (this.checkCanPersistWaiters != null && this.checkCanPersistWaiters.Count > 0 && this.Controller.IsPersistable)
            {
                List<WaitForCanPersistAsyncResult> waiters = this.checkCanPersistWaiters;
                this.checkCanPersistWaiters = null;
                foreach (WaitForCanPersistAsyncResult waiter in waiters)
                {
                    waiter.SetEvent(ref ownsLock);
                }
            }
        }

        IAsyncResult BeginWaitForCanPersist(ref bool ownsLock, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForCanPersistAsyncResult(this, ref ownsLock, timeout, callback, state);  
        }

        void EndWaitForCanPersist(IAsyncResult result, ref bool ownsLock)
        {
            WaitForCanPersistAsyncResult.End(result, ref ownsLock);
        }

        void ThrowIfAborted()
        {
            if (this.state == State.Aborted)
            {
                throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateAbortedFault(SR.WorkflowInstanceAborted(this.Id))));
            }
        }

        void ThrowIfTerminatedOrCompleted()
        {
            if (this.hasRaisedCompleted)
            {
                if (this.terminationException != null)
                {
                    throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateTerminatedFault(SR.WorkflowInstanceTerminated(this.Id))));
                }
                else
                {
                    throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateCompletedFault(SR.WorkflowInstanceCompleted(this.Id))));
                }
            }
        }

        void ThrowIfUnloaded()
        {
            if (this.state == State.Unloaded)
            {
                throw FxTrace.Exception.AsError(new FaultException(OperationExecutionFault.CreateInstanceUnloadedFault(SR.WorkflowInstanceUnloaded(this.Id))));
            }
        }

        void ThrowIfSuspended()
        {
            if (this.state == State.Suspended)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InstanceMustNotBeSuspended));
            }
        }

        void ThrowIfNoPersistenceProvider()
        {
            if (this.persistenceContext == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PersistenceProviderRequiredToPersist));
            }
        }

        bool ValidateStateForSuspend(Transaction transaction)
        {
            // Note: we allow suspend even when suspended to update Suspended reason.

            Validate(transaction == null ? XD2.WorkflowInstanceManagementService.Suspend : XD2.WorkflowInstanceManagementService.TransactedSuspend, transaction, true);

            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfTerminatedOrCompleted();
            ThrowIfUnloaded();

            return true;
        }

        bool ValidateStateForUnsuspend(Transaction transaction)
        {
            if (this.state == State.Active)
            {
                return false;
            }

            Validate(transaction == null ? XD2.WorkflowInstanceManagementService.Unsuspend : XD2.WorkflowInstanceManagementService.TransactedUnsuspend, transaction, true);

            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfTerminatedOrCompleted();
            ThrowIfUnloaded();

            return true;
        }

        bool ValidateStateForRun(Transaction transaction, string operationName)
        {
            if (this.hasRaisedCompleted || (this.state == State.Active && this.isRunnable) || this.isInTransaction)
            {
                return false;
            }

            Validate(operationName ?? (transaction == null ? XD2.WorkflowInstanceManagementService.Run : XD2.WorkflowInstanceManagementService.TransactedRun), transaction, true);

            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfUnloaded();
            ThrowIfSuspended();

            return true;
        }

        void ValidateStateForResumeProtocolBookmark()
        {
            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfTerminatedOrCompleted();
            ThrowIfUnloaded();
            ThrowIfSuspended();
        }

        void ValidateStateForAssociateKeys()
        {
            // WorkflowInstanceException validations
            ThrowIfSuspended();
        }

        bool AreBookmarksInvalid(out BookmarkResumptionResult result)
        {
            if (this.hasRaisedCompleted)
            {
                result = BookmarkResumptionResult.NotFound;
                return true;
            }
            else if (this.state == State.Unloaded || this.state == State.Aborted || this.state == State.Suspended)
            {
                result = BookmarkResumptionResult.NotReady;
                return true;
            }

            result = BookmarkResumptionResult.Success;
            return false;
        }

        bool ValidateStateForAbort()
        {
            if (this.state == State.Aborted)
            {
                return false;
            }

            return true;
        }

        bool ValidateStateForCancel(Transaction transaction)
        {
            if (this.hasRaisedCompleted)
            {
                return false;
            }

            Validate(transaction == null ? XD2.WorkflowInstanceManagementService.Cancel : XD2.WorkflowInstanceManagementService.TransactedCancel, transaction, true);

            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfUnloaded();

            return true;
        }

        void ValidateStateForPersist()
        {
            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfUnloaded();

            // Other validations
            ThrowIfNoPersistenceProvider();
        }

        bool ValidateStateForUnload()
        {
            if (this.state == State.Unloaded)
            {
                return false;
            }

            // WorkflowInstanceException validations
            ThrowIfAborted();

            // Other validations
            if (this.Controller.State != WorkflowInstanceState.Complete)
            {
                ThrowIfNoPersistenceProvider();
            }

            return true;
        }

        bool ValidateStateForTerminate(Transaction transaction)
        {
            Validate(transaction == null ? XD2.WorkflowInstanceManagementService.Terminate : XD2.WorkflowInstanceManagementService.TransactedTerminate, transaction, true);

            // WorkflowInstanceException validations
            ThrowIfAborted();
            ThrowIfTerminatedOrCompleted();
            ThrowIfUnloaded();

            return true;
        }

        delegate void InvokeCompletedCallback();

        enum PersistenceOperation : byte
        {
            Delete,
            Save,
            Unload
        }

        struct AcquireLockAsyncData
        {
            WorkflowServiceInstance instance;
            FastAsyncCallback callback;
            object state;

            public AcquireLockAsyncData(WorkflowServiceInstance instance, FastAsyncCallback callback, object state)
            {
                this.instance = instance;
                this.callback = callback;
                this.state = state;
            }

            public WorkflowServiceInstance Instance
            {
                get
                {
                    return instance;
                }
            }

            public FastAsyncCallback Callback
            {
                get
                {
                    return this.callback;
                }
            }

            public object State
            {
                get
                {
                    return this.state;
                }
            }
        }

        class AbortInstanceState
        {
            public AbortInstanceState(Exception reason, bool shouldTrackAbort)
            {
                this.Reason = reason;
                this.ShouldTrackAbort = shouldTrackAbort;
            }

            public Exception Reason
            {
                get;
                private set;
            }

            public bool ShouldTrackAbort
            {
                get;
                private set;
            }
        }

        public IAsyncResult BeginTryAcquireReference(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryAcquireReferenceAsyncResult(this, timeout, callback, state);
        }

        public bool EndTryAcquireReference(IAsyncResult result)
        {
            return TryAcquireReferenceAsyncResult.End(result);
        }

        public IAsyncResult BeginReleaseInstance(bool isTryUnload, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReleaseInstanceAsyncResult(this, isTryUnload, timeout, callback, state);
        }

        public void EndReleaseInstance(IAsyncResult result)
        {
            ReleaseInstanceAsyncResult.End(result);
        }

        public static void EndReleaseInstanceForClose(IAsyncResult result)
        {
            ReleaseInstanceAsyncResult.End(result);
        }

        public IAsyncResult BeginAssociateInfrastructureKeys(ICollection<InstanceKey> associatedKeys, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new AssociateKeysAsyncResult(this, associatedKeys, transaction, timeout, callback, state);
        }

        public void EndAssociateInfrastructureKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        public void ReleaseContext(WorkflowOperationContext context)
        {
            lock (this.thisLock)
            {
                this.pendingRequests.Remove(context);
            }
        }

        public IAsyncResult BeginWaitForPendingOperations(string sessionId, TimeSpan timeout, AsyncCallback callback, object state)
        {
            PendingOperationAsyncResult result = null;
            lock (this.thisLock)
            {
                if (this.pendingOperations == null)
                {
                    this.pendingOperations = new Dictionary<string, List<PendingOperationAsyncResult>>();
                }
                List<PendingOperationAsyncResult> pendingList;
                if (!this.pendingOperations.TryGetValue(sessionId, out pendingList))
                {
                    pendingList = new List<PendingOperationAsyncResult>();
                    this.pendingOperations.Add(sessionId, pendingList);
                }
                bool isFirstRequest = (pendingList.Count == 0);
                result = new PendingOperationAsyncResult(isFirstRequest, timeout, callback, state);
                pendingList.Add(result);
                ++this.pendingOperationCount;
            }
            result.Start();
            return result;
        }

        public void EndWaitForPendingOperations(IAsyncResult result)
        {
            PendingOperationAsyncResult.End(result);
        }


        public void RemovePendingOperation(string sessionId, IAsyncResult result)
        {
            // remove the async result from the queue. The result could represent the operation currently being processed for the session
            // or could be an operation that had timed out waiting to get to the head of the queue.
            // Also, note that if the instance has already completed/aborted etc all pending operations would call OnWorkflowOperationCompleted
            // simultaneously and this.pendingOperations would be null.
            lock (this.thisLock)
            {
                List<PendingOperationAsyncResult> pendingList;
                if (this.pendingOperations != null && this.pendingOperations.TryGetValue(sessionId, out pendingList))
                {
                    if (pendingList.Count > 0)
                    {
                        // In the happy path, RemovePendingOperation might get called more than more than once(HandleEndResume & ProcessReply)
                        // wasInProcess would be false the second time. When wasInProcess is false, we do not unblock the next item in the list
                        bool wasInProcess = pendingList[0] == result;
                        
                        if (pendingList.Remove((PendingOperationAsyncResult)result))
                        {
                            --this.pendingOperationCount;
                        }
                        if (pendingList.Count == 0)
                        {
                            this.pendingOperations.Remove(sessionId);
                        }
                        // signal the next request to resume bookmark
                        else if (wasInProcess)
                        {
                            pendingList[0].Unblock();
                        }
                    }
                }
            }
        }

        void CompletePendingOperations()
        {
            lock (this.thisLock)
            {
                if (this.pendingOperations != null)
                {
                    foreach (List<PendingOperationAsyncResult> pendingList in this.pendingOperations.Values)
                    {
                        foreach (PendingOperationAsyncResult result in pendingList)
                        {
                            result.Unblock();
                        }
                    }
                }
                this.pendingOperations = null;
                this.pendingOperationCount = 0;
            }
        }

        void OnIdle()
        {
            if (this.BufferedReceiveManager != null)
            {
                this.persistenceContext.Bookmarks = this.Controller.GetBookmarks();
                this.BufferedReceiveManager.Retry(this.persistenceContext.AssociatedKeys, this.persistenceContext.Bookmarks);
            }
        }

        void OnCompleted()
        {
            if (this.terminationException != null)
            {
                FaultPendingRequests(new FaultException(OperationExecutionFault.CreateTerminatedFault(SR.WorkflowInstanceTerminated(this.Id))));
            }
            else
            {
                FaultPendingRequests(new FaultException(OperationExecutionFault.CreateCompletedFault(SR.WorkflowInstanceCompleted(this.Id))));
            }

            if (handleEndReleaseInstance == null)
            {
                handleEndReleaseInstance = Fx.ThunkCallback(new AsyncCallback(HandleEndReleaseInstance));
            }
            IAsyncResult result = this.BeginReleaseInstance(false, TimeSpan.MaxValue, handleEndReleaseInstance, this);
            if (result.CompletedSynchronously)
            {
                OnReleaseInstance(result);
            }

            CompletePendingOperations();
        }

        static void HandleEndReleaseInstance(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            WorkflowServiceInstance thisPtr = (WorkflowServiceInstance)result.AsyncState;
            thisPtr.OnReleaseInstance(result);
        }

        void OnReleaseInstance(IAsyncResult result)
        {
            try
            {
                this.EndReleaseInstance(result);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                // 
                this.AbortInstance(e, false);
            }
        }

        void OnUnhandledException(UnhandledExceptionAsyncData data)
        {
            Fx.Assert(data != null, "data must not be null!");
            Fx.Assert(data.Exception != null, "data.Exception must not be null!");

            FaultPendingRequests(data.Exception);
            this.UnhandledExceptionPolicy.OnUnhandledException(data);
        }

        // notify pending requests so that clients don't hang
        void FaultPendingRequests(Exception e)
        {
            WorkflowOperationContext[] requestsToFault = null;

            lock (this.thisLock)
            {
                if (this.pendingRequests.Count == 0)
                {
                    return;
                }

                requestsToFault = this.pendingRequests.ToArray();
                this.pendingRequests.Clear();
            }

            for (int i = 0; i < requestsToFault.Length; i++)
            {
                requestsToFault[i].SendFault(e);
            }
        }

        //Attached Transaction outcome Signals from IEnlistmentNotification.
        public void TransactionCommitted() //Signal from TransactionContext on attached transaction commit.
        {
            if (this.TryAddReference())
            {
                try
                {
                    if ((this.state == State.Suspended && this.isTransactedCancelled) || this.state == State.Active)
                    {
                        bool ownsLock = false;
                        // this could ---- with other commands and may throw exception.
                        // treat it as best effort pulse of workflow.
                        try
                        {

                            AcquireLock(this.acquireLockTimeout, ref ownsLock);

                            if ((this.state == State.Suspended && this.isTransactedCancelled) || ValidateStateForRun(null, null))
                            {
                                this.isRunnable = true;
                                this.state = State.Active;
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            FxTrace.Exception.AsWarning(exception);
                        }
                        finally
                        {
                            ReleaseLock(ref ownsLock);
                        }
                    }
                    // the workflow has completed thru transacted Terminate 
                    else if (this.state == State.Unloaded && this.completionState == ActivityInstanceState.Faulted)
                    {
                        try
                        {
                            OnCompleted();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }

                            this.AbortInstance(exception, false);
                        }
                    }
                }
                finally
                {
                    this.ReleaseReference();
                }
            }
        }

        public void OnTransactionPrepared()
        {
            // Transaction has been prepared.
            // As far as WorkflowServiceInstance is concerned, no longer in transaction.
            this.transactionContext = null;
            this.isInTransaction = false;
        }

        public void OnTransactionAbortOrInDoubt(TransactionException exception)
        {
            Fx.Assert(exception != null, "Need a valid TransactionException to call this");
            this.AbortInstance(exception, false);
        }

        // Called under the lock.
        void Validate(string operationName, Transaction ambientTransaction, bool controlEndpoint)
        {
            ValidateHelper(operationName, ambientTransaction, false, controlEndpoint);
        }

        void ValidateHelper(string operationName, Transaction ambientTransaction, bool useThreadTransaction, bool controlEndpoint)
        {
            TransactionContext attachedTransaction = this.transactionContext;

            //Ensure Instance is usable.
            if (attachedTransaction != null &&
                attachedTransaction.CurrentTransaction != (useThreadTransaction ? Transaction.Current : ambientTransaction))
            {
                throw FxTrace.Exception.AsError(new FaultException(
                    OperationExecutionFault.CreateTransactedLockException(this.persistenceContext.InstanceId, operationName)));
            }

            if (controlEndpoint)
            {
                Fx.AssertAndThrow(this.state != State.Unloaded, "Cannot be unloaded");
            }

            if (this.state == State.Unloaded)
            {
                throw FxTrace.Exception.AsError(new FaultException(
                    OperationExecutionFault.CreateInstanceUnloadedFault(SR.ServiceInstanceUnloaded(this.persistenceContext.InstanceId))));
            }

            //Do a fast check to fail fast.
            if (this.state == State.Completed || this.state == State.Aborted)
            {
                throw FxTrace.Exception.AsError(new FaultException(
                    OperationExecutionFault.CreateInstanceNotFoundFault(SR.ServiceInstanceTerminated(this.persistenceContext.InstanceId))));
            }

            if (this.state == State.Suspended &&
                !(operationName == XD2.WorkflowInstanceManagementService.Suspend
                || operationName == XD2.WorkflowInstanceManagementService.TransactedSuspend
                || operationName == XD2.WorkflowInstanceManagementService.Unsuspend
                || operationName == XD2.WorkflowInstanceManagementService.TransactedUnsuspend
                || operationName == XD2.WorkflowInstanceManagementService.Terminate
                || operationName == XD2.WorkflowInstanceManagementService.TransactedTerminate
                || operationName == XD2.WorkflowInstanceManagementService.Cancel
                || operationName == XD2.WorkflowInstanceManagementService.TransactedCancel))
            {
                throw FxTrace.Exception.AsError(new FaultException(
                    OperationExecutionFault.CreateSuspendedFault(this.Id, operationName)));
            }
        }
        //already done under the scope of a lock.No additional locking needed here
        void DecrementBusyCount()
        {
            if (this.hasIncrementedBusyCount)
            {
                this.serviceHost.DecrementBusyCount();
                if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceDecrementBusyCount(SR.BusyCountTraceFormatString(this.Id));
                }
                this.hasIncrementedBusyCount = false;
            }
        }
        //already done under the scope of a lock.No additional locking needed here
        void IncrementBusyCount()
        {
            if (!this.hasIncrementedBusyCount)
            {
                this.serviceHost.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(SR.BusyCountTraceFormatString(this.Id));
                }
                this.hasIncrementedBusyCount = true;
            }
        }


        enum State
        {
            Active, //Default.
            Aborted,
            Suspended,
            Completed,
            Unloaded
        };

        class ReleaseInstanceAsyncResult : AsyncResult
        {
            static AsyncCompletion handleEndUnload;
            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);
            static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(OnLockAcquired);
            static FastAsyncCallback acquireCompletedCallback = new FastAsyncCallback(AcquireCompletedCallback);
            static AsyncCompletion onReleasePersistenceContext;
            static AsyncCompletion onClosePersistenceContext;
            WorkflowServiceInstance workflowInstance;
            TimeoutHelper timeoutHelper;
            bool isTryUnload;
            bool ownsLock;
            bool referenceAcquired;

            public ReleaseInstanceAsyncResult(WorkflowServiceInstance workflowServiceInstance,
                bool isTryUnload, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.workflowInstance = workflowServiceInstance;
                this.isTryUnload = isTryUnload;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.OnCompleting = onCompleting;

                bool completeSelf = false;
                Exception completionException = null;
                try
                {
                    completeSelf = TryAcquire();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                    throw;
                }
                finally
                {
                    if (completionException != null)
                    {
                        Finally(this, completionException);
                    }
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReleaseInstanceAsyncResult>(result);
            }

            bool TryAcquire()
            {
                if (this.workflowInstance.acquireReferenceSemaphore.EnterAsync(timeoutHelper.RemainingTime(), acquireCompletedCallback, this))
                {
                    return this.HandleEndAcquireReference();
                }
                else
                {
                    return false;
                }
            }

            bool HandleEndAcquireReference()
            {
                this.referenceAcquired = true;
                if (this.workflowInstance.hasPersistedDeleted)
                {
                    return this.LockAndReleasePersistenceContext();
                }
                else
                {
                    return this.ReleaseInstance();
                }
            }

            static void AcquireCompletedCallback(object state, Exception completionException)
            {
                ReleaseInstanceAsyncResult thisPtr = (ReleaseInstanceAsyncResult)state;

                bool completeSelf = true;
                if (completionException == null)
                {
                    try
                    {
                        completeSelf = thisPtr.HandleEndAcquireReference();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            bool ReleaseInstance()
            {
                if (handleEndUnload == null)
                {
                    handleEndUnload = new AsyncCompletion(HandleEndUnload);
                }

                IAsyncResult result = null;
                try
                {
                    if (this.isTryUnload)
                    {
                        result = this.BeginTryUnload(timeoutHelper.RemainingTime(),
                            this.PrepareAsyncCompletion(handleEndUnload), this);
                    }
                    else
                    {
                        result = this.BeginUnload(timeoutHelper.RemainingTime(),
                            this.PrepareAsyncCompletion(handleEndUnload), this);
                    }
                }
                catch (FaultException exception)
                {
                    if (OperationExecutionFault.IsAbortedFaultException(exception))
                    {
                        FxTrace.Exception.AsWarning(exception);
                        return true;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (result.CompletedSynchronously)
                {
                    return HandleEndUnload(result);
                }
                else
                {
                    return false;
                }
            }

            IAsyncResult BeginUnload(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new UnloadOrPersistAsyncResult(this.workflowInstance, PersistenceOperation.Unload, false, false, timeout, callback, state);
            }

            void EndUnload(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult.End(result);
            }

            IAsyncResult BeginTryUnload(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new UnloadOrPersistAsyncResult(this.workflowInstance, PersistenceOperation.Unload, false, true, timeout, callback, state);
            }

            bool EndTryUnload(IAsyncResult result)
            {
                return UnloadOrPersistAsyncResult.End(result);
            }

            static bool HandleEndUnload(IAsyncResult result)
            {
                ReleaseInstanceAsyncResult thisPtr = (ReleaseInstanceAsyncResult)result.AsyncState;
                bool successfulUnload = false;
                try
                {
                    if (thisPtr.isTryUnload)
                    {
                        // if EndTryUnload returns false, then we need to revert our changes
                        successfulUnload = thisPtr.EndTryUnload(result);
                    }
                    else
                    {
                        thisPtr.EndUnload(result);
                        successfulUnload = true;
                    }
                }
                catch (FaultException exception)
                {
                    if (OperationExecutionFault.IsAbortedFaultException(exception))
                    {
                        FxTrace.Exception.AsWarning(exception);
                    }
                    else
                    {
                        throw;
                    }
                }

                if (successfulUnload)
                {
                    return thisPtr.LockAndReleasePersistenceContext();
                }
                else
                {
                    return true;
                }
            }

            bool LockAndReleasePersistenceContext()
            {
                if (this.workflowInstance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), ref this.ownsLock, lockAcquiredCallback, this))
                {
                    bool completeSelf = true;
                    try
                    {
                        completeSelf = this.ReleasePersistenceContext();
                    }
                    finally
                    {
                        if (completeSelf)
                        {
                            this.workflowInstance.ReleaseLock(ref this.ownsLock);
                        }
                    }
                    return completeSelf;
                }
                else
                {
                    return false;
                }
            }

            static void OnLockAcquired(object state, Exception asyncException)
            {
                ReleaseInstanceAsyncResult thisPtr = (ReleaseInstanceAsyncResult)state;

                if (asyncException != null)
                {
                    thisPtr.Complete(false, asyncException);
                    return;
                }

                thisPtr.ownsLock = true;

                bool completeSelf = true;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.ReleasePersistenceContext();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    completionException = exception;
                }
                finally
                {
                    if (completeSelf)
                    {
                        thisPtr.workflowInstance.ReleaseLock(ref thisPtr.ownsLock);
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            bool ReleasePersistenceContext()
            {
                if (this.workflowInstance.persistenceContext.State != CommunicationState.Opened)
                {
                    return true;
                }

                if (onReleasePersistenceContext == null)
                {
                    onReleasePersistenceContext = new AsyncCompletion(OnReleasePersistenceContext);
                }

                IAsyncResult result = this.workflowInstance.persistenceContext.BeginRelease(this.workflowInstance.persistTimeout,
                    PrepareAsyncCompletion(onReleasePersistenceContext), this);

                return SyncContinue(result);
            }

            static bool OnReleasePersistenceContext(IAsyncResult result)
            {
                ReleaseInstanceAsyncResult thisPtr = (ReleaseInstanceAsyncResult)result.AsyncState;
                thisPtr.workflowInstance.persistenceContext.EndRelease(result);
                if (onClosePersistenceContext == null)
                {
                    onClosePersistenceContext = new AsyncCompletion(OnClosePersistenceContext);
                }

                IAsyncResult closeResult = thisPtr.workflowInstance.persistenceContext.BeginClose(thisPtr.timeoutHelper.RemainingTime(),
                    thisPtr.PrepareAsyncCompletion(onClosePersistenceContext), thisPtr);
                return thisPtr.SyncContinue(closeResult);
            }

            static bool OnClosePersistenceContext(IAsyncResult result)
            {
                ReleaseInstanceAsyncResult thisPtr = (ReleaseInstanceAsyncResult)result.AsyncState;
                thisPtr.workflowInstance.persistenceContext.EndClose(result);
                thisPtr.workflowInstance.Dispose();
                return true;
            }

            static void Finally(AsyncResult result, Exception completionException)
            {
                ReleaseInstanceAsyncResult thisPtr = (ReleaseInstanceAsyncResult)result;
                try
                {
                    try
                    {
                        if (completionException != null && !Fx.IsFatal(completionException))
                        {
                            thisPtr.workflowInstance.AbortInstance(completionException, thisPtr.ownsLock);
                        }
                    }
                    finally
                    {
                        if (thisPtr.ownsLock)
                        {
                            thisPtr.workflowInstance.ReleaseLock(ref thisPtr.ownsLock);
                        }
                    }
                }
                finally
                {
                    if (thisPtr.referenceAcquired)
                    {
                        thisPtr.workflowInstance.acquireReferenceSemaphore.Exit();
                        thisPtr.referenceAcquired = false;
                    }
                }
            }
        }

        class TryAcquireReferenceAsyncResult : AsyncResult
        {
            static FastAsyncCallback acquireCompletedCallback = new FastAsyncCallback(AcquireCompletedCallback);
            WorkflowServiceInstance instance;
            TimeoutHelper timeoutHelper;
            bool result;

            public TryAcquireReferenceAsyncResult(WorkflowServiceInstance instance, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);

                if (TryAcquire())
                {
                    this.Complete(true);
                }
            }

            public static bool End(IAsyncResult result)
            {
                return AsyncResult.End<TryAcquireReferenceAsyncResult>(result).result;
            }

            bool TryAcquire()
            {
                if (this.instance.acquireReferenceSemaphore.EnterAsync(timeoutHelper.RemainingTime(), acquireCompletedCallback, this))
                {
                    this.HandleEndAcquireReference();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            void HandleEndAcquireReference()
            {
                try
                {
                    this.result = this.instance.TryAddReference();
                }
                finally
                {
                    this.instance.acquireReferenceSemaphore.Exit();
                }
            }

            static void AcquireCompletedCallback(object state, Exception completionException)
            {
                TryAcquireReferenceAsyncResult thisPtr = (TryAcquireReferenceAsyncResult)state;

                if (completionException == null)
                {
                    try
                    {
                        thisPtr.HandleEndAcquireReference();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                }

                thisPtr.Complete(false, completionException);
            }
        }

        class PendingOperationAsyncResult : AsyncResult
        {
            static Action<object, TimeoutException> handleEndWait = new Action<object, TimeoutException>(HandleEndWait);
            AsyncWaitHandle waitHandle;
            bool isFirstRequest;
            TimeSpan timeout;
            
            public PendingOperationAsyncResult(bool isFirstRequest, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.isFirstRequest = isFirstRequest;
                this.timeout = timeout;

                if (!this.isFirstRequest)
                {
                    this.waitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                }
            }

            public void Start()
            {
                if (this.isFirstRequest)
                {
                    Complete(true);
                    return;
                }

                Fx.Assert(this.waitHandle != null, "waitHandle should not be null if the request is not the first");
                if (this.waitHandle.WaitAsync(handleEndWait, this, this.timeout))
                {
                    Complete(true);
                }
            }

            static void HandleEndWait(object state, TimeoutException e)
            {
                PendingOperationAsyncResult thisPtr = (PendingOperationAsyncResult)state;
                thisPtr.Complete(false, e);
            }

            public void Unblock()
            {
                if (this.waitHandle != null)
                {
                    this.waitHandle.Set();
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PendingOperationAsyncResult>(result);
            }

        }

        class AssociateKeysAsyncResult : TransactedAsyncResult
        {
            static AsyncCompletion handleLockAcquired = new AsyncCompletion(HandleLockAcquired);
            static AsyncCompletion handleAssociateInfrastructureKeys = new AsyncCompletion(HandleAssociateInfrastructureKeys);
            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);

            readonly WorkflowServiceInstance workflow;
            readonly ICollection<InstanceKey> associatedKeys;
            readonly TimeoutHelper timeoutHelper;
            readonly Transaction transaction;
            bool ownsLock;

            public AssociateKeysAsyncResult(WorkflowServiceInstance workflow, ICollection<InstanceKey> associatedKeys, Transaction transaction,
                TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(associatedKeys != null && associatedKeys.Count > 0, "Must have associatedKeys!");
                this.workflow = workflow;
                this.associatedKeys = associatedKeys;
                this.transaction = transaction;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.OnCompleting = onCompleting;

                IAsyncResult result = this.workflow.BeginAcquireLockOnIdle(this.timeoutHelper.RemainingTime(), ref this.ownsLock, 
                    PrepareAsyncCompletion(handleLockAcquired), this);
                if (SyncContinue(result))
                {
                    Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AssociateKeysAsyncResult>(result);
            }

            static bool HandleLockAcquired(IAsyncResult result)
            {
                AssociateKeysAsyncResult thisPtr = (AssociateKeysAsyncResult)result.AsyncState;

                if (result.CompletedSynchronously)
                {
                    thisPtr.workflow.EndAcquireLockOnIdle(result);
                }
                else
                {
                    thisPtr.workflow.EndAcquireLockOnIdle(result, ref thisPtr.ownsLock);
                }

                thisPtr.workflow.ValidateStateForAssociateKeys();
                return thisPtr.AssociateKeys();
            }

            bool AssociateKeys()
            {
                IAsyncResult result;
                using (PrepareTransactionalCall(this.transaction))
                {
                    result = this.workflow.persistenceContext.BeginAssociateInfrastructureKeys(this.associatedKeys, this.workflow.persistTimeout,
                        PrepareAsyncCompletion(handleAssociateInfrastructureKeys), this);
                }
                return SyncContinue(result);
            }

            static bool HandleAssociateInfrastructureKeys(IAsyncResult result)
            {
                AssociateKeysAsyncResult thisPtr = (AssociateKeysAsyncResult)result.AsyncState;
                thisPtr.workflow.persistenceContext.EndAssociateInfrastructureKeys(result);
                // Proper relase lock
                thisPtr.workflow.ReleaseLock(ref thisPtr.ownsLock);
                return true;
            }

            static void Finally(AsyncResult result, Exception completionException)
            {
                AssociateKeysAsyncResult thisPtr = (AssociateKeysAsyncResult)result;
                // Fallback for exception
                if (thisPtr.ownsLock)
                {
                    thisPtr.workflow.ReleaseLock(ref thisPtr.ownsLock);
                }
            }
        }

        class ResumeProtocolBookmarkAsyncResult : AsyncResult
        {
            static Action<object, TimeoutException> nextIdleCallback;
            static Action<object, TimeoutException> workflowServiceInstanceReadyCallback;

            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);
            static AsyncCompletion handleEndTrack = new AsyncCompletion(HandleEndTrack);
            static AsyncCompletion handleEndLockAcquired = new AsyncCompletion(HandleEndLockAcquired);
            static AsyncCompletion handleEndReferenceAcquired = new AsyncCompletion(HandleEndReferenceAcquired);

            WorkflowServiceInstance instance;
            Bookmark bookmark;
            object value;
            BookmarkScope bookmarkScope;
            TimeoutHelper timeoutHelper;
            TimeoutHelper nextIdleTimeoutHelper;
            AsyncWaitHandle waitHandle;
            bool ownsLock;
            BookmarkResumptionResult resumptionResult;
            bool isResumeProtocolBookmark;
            bool referenceAcquired;

            public ResumeProtocolBookmarkAsyncResult(WorkflowServiceInstance instance, Bookmark bookmark, object value, BookmarkScope bookmarkScope, bool isResumeProtocolBookmark, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instance = instance;
                this.bookmark = bookmark;
                this.value = value;
                this.bookmarkScope = bookmarkScope;
                this.timeoutHelper = new TimeoutHelper(timeout);
                // The value for WorkflowServiceHost.FilterResumeTimeout comes from the AppSetting
                // "microsoft:WorkflowServices:FilterResumeTimeoutInSeconds"
                this.nextIdleTimeoutHelper = new TimeoutHelper(instance.serviceHost.FilterResumeTimeout);
                this.isResumeProtocolBookmark = isResumeProtocolBookmark;
                this.OnCompleting = onCompleting;
                
                Exception completionException = null;
                bool completeSelf = true;

                try
                {
                    if (this.isResumeProtocolBookmark)
                    {
                        completeSelf = DoResumeBookmark();
                    }
                    else
                    {
                        completeSelf = WaitForInstanceToBeReady();
                    }

                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                if (completeSelf)
                {
                    this.Complete(true, completionException);
                }
            }

            bool DoResumeBookmark()
            {
                IAsyncResult result = this.instance.BeginAcquireLockOnIdle(timeoutHelper.RemainingTime(), ref this.ownsLock, PrepareAsyncCompletion(handleEndLockAcquired), this);
                return SyncContinue(result);
            }

            bool WaitForInstanceToBeReady()
            {
                IAsyncResult result = this.instance.BeginTryAcquireReference(timeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleEndReferenceAcquired), this);
                return SyncContinue(result);
            }

            static bool HandleEndReferenceAcquired(IAsyncResult result)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = (ResumeProtocolBookmarkAsyncResult)result.AsyncState;
                thisPtr.referenceAcquired = thisPtr.instance.EndTryAcquireReference(result);
                if (thisPtr.referenceAcquired)
                {
                    return thisPtr.WaitToBeSignaled();
                }
                else
                {
                    thisPtr.resumptionResult = BookmarkResumptionResult.NotReady;
                    return true;
                }
            }

            bool WaitToBeSignaled()
            {
                bool needToWait = false;

                lock (this.instance.thisLock)
                {
                    if (!this.instance.isWorkflowServiceInstanceReady)
                    {
                        needToWait = true;
                        if (this.instance.workflowServiceInstanceReadyWaitHandle == null)
                        {
                            this.instance.workflowServiceInstanceReadyWaitHandle = new AsyncWaitHandle(EventResetMode.ManualReset);
                        }
                    }
                }

                if (needToWait)
                {
                    if (workflowServiceInstanceReadyCallback == null)
                    {
                        workflowServiceInstanceReadyCallback = new Action<object, TimeoutException>(OnSignaled);
                    }

                    if (this.instance.workflowServiceInstanceReadyWaitHandle.WaitAsync(workflowServiceInstanceReadyCallback, this, this.timeoutHelper.RemainingTime()))
                    {
                        return DoResumeBookmark();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return DoResumeBookmark();
                }
            }

            static void OnSignaled(object state, TimeoutException exception)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = (ResumeProtocolBookmarkAsyncResult)state;
                if (exception != null)
                {
                    thisPtr.Complete(false, exception);
                    return;
                }

                bool completeSelf = false;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.DoResumeBookmark();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    completionException = e;
                }
                finally
                {
                    if (completionException != null)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false);
                }
            }

            public static BookmarkResumptionResult End(IAsyncResult result)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = AsyncResult.End<ResumeProtocolBookmarkAsyncResult>(result);
                return thisPtr.resumptionResult;
            }

            static bool HandleEndLockAcquired(IAsyncResult result)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = (ResumeProtocolBookmarkAsyncResult)result.AsyncState;
                if (result.CompletedSynchronously)
                {
                    thisPtr.instance.EndAcquireLockOnIdle(result);
                }
                else
                {
                    thisPtr.instance.EndAcquireLockOnIdle(result, ref thisPtr.ownsLock);
                }
                return thisPtr.PerformResumption();
            }

            bool PerformResumption()
            {
                // We always have the lock when entering this method

                bool waitFinishedSynchronously;
                bool completeSelf = false;

                // For ProtocolBookmark without Out-Of-Order messaging support, we will throw and 
                // propagate Fault to client in case of invalid state (similar to management commands).
                // Otherwise, the result consistent with WorkflowApplication will be return and 
                // the caller (eg. delay extension or OOM) needs to handle them accordingly.
                if (this.isResumeProtocolBookmark && this.instance.BufferedReceiveManager == null)
                {
                    this.instance.ValidateStateForResumeProtocolBookmark();
                }
                else
                {
                    if (this.instance.AreBookmarksInvalid(out this.resumptionResult))
                    {
                        return TrackPerformResumption(true);
                    }
                }

                do
                {
                    waitFinishedSynchronously = false;

                    bool bufferedReceiveEnabled = this.isResumeProtocolBookmark && this.instance.BufferedReceiveManager != null;
                    this.resumptionResult = this.instance.ResumeProtocolBookmarkCore(this.bookmark, this.value, this.bookmarkScope, bufferedReceiveEnabled, ref this.waitHandle, ref this.ownsLock);
                    if (this.resumptionResult == BookmarkResumptionResult.NotReady && !bufferedReceiveEnabled && (this.instance.serviceHost.FilterResumeTimeout.TotalSeconds > 0))
                        {
                        if (nextIdleCallback == null)
                        {
                            nextIdleCallback = new Action<object, TimeoutException>(OnNextIdle);
                        }
                        
                        if (this.waitHandle.WaitAsync(nextIdleCallback, this, !this.isResumeProtocolBookmark ? this.timeoutHelper.RemainingTime() : this.nextIdleTimeoutHelper.RemainingTime()))
                        {
                            // We now have the lock
                            this.ownsLock = true;

                            // We should retry the resumption synchronously
                            waitFinishedSynchronously = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        completeSelf = true;
                        break;
                    }

                }
                while (waitFinishedSynchronously);

                return TrackPerformResumption(completeSelf);
            }

            bool TrackPerformResumption(bool completeSelf)
            {
                if (this.instance.Controller.HasPendingTrackingRecords)
                {
                    Fx.Assert(completeSelf, "CompleteSelf should be true at this point.");

                    IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.instance.trackTimeout, PrepareAsyncCompletion(handleEndTrack), this);
                    completeSelf = SyncContinue(result);
                }

                return completeSelf;
            }

            static bool HandleEndTrack(IAsyncResult result)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = (ResumeProtocolBookmarkAsyncResult)result.AsyncState;
                thisPtr.instance.Controller.EndFlushTrackingRecords(result);

                if (thisPtr.ownsLock)
                {
                    thisPtr.instance.ReleaseLock(ref thisPtr.ownsLock);
                }
                if (thisPtr.referenceAcquired)
                {
                    thisPtr.instance.ReleaseReference();
                    thisPtr.referenceAcquired = false;
                }
                return true;
            }

            static void OnNextIdle(object state, TimeoutException asyncException)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = (ResumeProtocolBookmarkAsyncResult)state;

                if (asyncException != null)
                {
                    lock (thisPtr.instance.activeOperationsLock)
                    {
                        // If the waitHandle is not in either of these lists then it must have
                        // been removed by the Set() path - that means we've got the lock, so let's
                        // just run with it (IE - swallow the exception).
                        if (thisPtr.instance.nextIdleWaiters.Remove(thisPtr.waitHandle) || thisPtr.instance.idleWaiters.Remove(thisPtr.waitHandle))
                        {
                            thisPtr.Complete(false, asyncException);
                            return;
                        }
                    }
                }

                thisPtr.ownsLock = true;

                bool completeSelf = true;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.PerformResumption();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                finally
                {
                    if (completeSelf)
                    {
                        thisPtr.instance.ReleaseLock(ref thisPtr.ownsLock);
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            static void Finally(AsyncResult result, Exception completionException)
            {
                ResumeProtocolBookmarkAsyncResult thisPtr = (ResumeProtocolBookmarkAsyncResult)result;
                try
                {
                    if (thisPtr.ownsLock)
                    {
                        thisPtr.instance.ReleaseLock(ref thisPtr.ownsLock);
                    }
                }
                finally
                {
                    if (thisPtr.referenceAcquired)
                    {
                        thisPtr.instance.ReleaseReference();
                        thisPtr.referenceAcquired = false;
                    }
                }
            }
        }

        class UnloadOrPersistAsyncResult : TransactedAsyncResult
        {
            static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(OnLockAcquired);
            static AsyncCompletion persistedCallback = new AsyncCompletion(OnPersisted);
            static AsyncCompletion savedCallback = new AsyncCompletion(OnSaved);
            static AsyncCompletion waitForCanPersistCallback = new AsyncCompletion(OnWaitForCanPersist);
            static AsyncCompletion providerOpenedCallback = new AsyncCompletion(OnProviderOpened);
            static AsyncCompletion outermostCallback = new AsyncCompletion(OutermostCallback);
            static AsyncCompletion trackingCompleteCallback = new AsyncCompletion(OnTrackingComplete);
            static AsyncCompletion completeContextCallback = new AsyncCompletion(OnCompleteContext);
            static AsyncCompletion notifyCompletionCallback = new AsyncCompletion(OnNotifyCompletion);
            static Action<AsyncResult, Exception> completeCallback = new Action<AsyncResult, Exception>(OnComplete);

            WorkflowServiceInstance instance;
            bool isUnloaded;
            SaveStatus saveStatus;
            TimeoutHelper timeoutHelper;
            PersistenceOperation operation;
            WorkflowPersistenceContext context;
            AsyncCompletion nextInnerAsyncCompletion;
            IDictionary<XName, InstanceValue> data;
            PersistencePipeline pipeline;
            bool ownsLock;
            bool isWorkflowThread;
            bool isTry;
            bool tryResult;
            bool updateState;
            bool isCompletionTransactionRequired;
            DependentTransaction dependentTransaction;
            bool isIdlePolicyPersist;
            long startTime;

            public UnloadOrPersistAsyncResult(WorkflowServiceInstance instance, PersistenceOperation operation,
                bool isWorkflowThread, bool isTry, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                // The isTry flag is only true when this is an idle policy initiated persist/unload.
                
                Fx.Assert((isWorkflowThread && !isTry) || !isWorkflowThread, "Either we're the workflow thread and NOT a try or we're not a workflow thread.");

                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.operation = operation;
                this.isWorkflowThread = isWorkflowThread;
                this.isTry = isTry;
                this.tryResult = true;
                this.isUnloaded = (operation == PersistenceOperation.Unload || operation == PersistenceOperation.Delete);
                this.saveStatus = SaveStatus.Locked;
                this.isCompletionTransactionRequired = this.isUnloaded && instance.Controller.State == WorkflowInstanceState.Complete && 
                    instance.creationContext != null && instance.creationContext.IsCompletionTransactionRequired;
                this.isIdlePolicyPersist = isTry && operation == PersistenceOperation.Save;

                if (operation == PersistenceOperation.Unload)
                {
                    this.saveStatus = SaveStatus.Unlocked;
                }
                else if (operation == PersistenceOperation.Delete)
                {
                    this.saveStatus = SaveStatus.Completed;
                }
                else if (operation == PersistenceOperation.Save)
                {
                    SetStartTime();
                }
                
                // Save off the current transaction in case we have an async operation before we end up creating
                // the WorkflowPersistenceContext and create it on another thread. Do a simple clone here to prevent
                // the object referenced by Transaction.Current from disposing before we get around to referencing it
                // when we create the WorkflowPersistenceContext.
                //
                // This will throw TransactionAbortedException by design, if the transaction is already rolled back.
                Transaction currentTransaction = Transaction.Current;
                if (currentTransaction != null)
                {
                    OnCompleting = UnloadOrPersistAsyncResult.completeCallback;
                    this.dependentTransaction = currentTransaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                }

                bool completeSelf = true;
                bool success = false;
                try
                {
                    if (this.isWorkflowThread)
                    {
                        Fx.Assert(this.instance.Controller.IsPersistable, "The runtime won't schedule this work item unless we've passed the guard");

                        // We're an internal persistence on the workflow thread which means
                        // that we are passed the guard already, we have the lock, and we know
                        // we aren't detached.

                        completeSelf = OpenProvider();
                    }
                    else
                    {
                        try
                        {
                            completeSelf = LockAndPassGuard();
                        }
                        finally
                        {
                            if (completeSelf)
                            {
                                Fx.Assert(!this.isWorkflowThread, "We should never be calling ReleaseLock if this is the workflow thread.");

                                this.instance.ReleaseLock(ref this.ownsLock, this.isIdlePolicyPersist && this.tryResult);
                            }
                        }
                    }
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        if (this.dependentTransaction != null)
                        {
                            this.dependentTransaction.Complete();
                        }
                    }
                }

                if (completeSelf)
                {
                    Complete(true);
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because it accesses UnsafeNativeMethods.QueryPerformanceCounter.",
                Safe = "Safe because we only make the call if PartialTrustHelper.AppDomainFullyTrusted is true.")]
            [SecuritySafeCritical]
            void SetStartTime()
            {
                if (PartialTrustHelpers.AppDomainFullyTrusted && UnsafeNativeMethods.QueryPerformanceCounter(out this.startTime) == 0)
                {
                    this.startTime = -1;
                }
            }

            bool LockAndPassGuard()
            {
                if (this.instance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), ref this.ownsLock, lockAcquiredCallback, this))
                {
                    return PassGuard();
                }

                return false;
            }

            bool PassGuard()
            {
                if (this.operation == PersistenceOperation.Unload)
                {
                    if (!this.instance.ValidateStateForUnload())
                    {
                        return true;
                    }
                }
                else
                {
                    this.instance.ValidateStateForPersist();
                }

                if (this.instance.Controller.IsPersistable)
                {
                    return OpenProvider();
                }
                else
                {
                    if (this.isTry)
                    {
                        this.tryResult = false;
                        return true;
                    }

                    IAsyncResult result = this.instance.BeginWaitForCanPersist(ref this.ownsLock, this.timeoutHelper.RemainingTime(),
                        PrepareInnerAsyncCompletion(waitForCanPersistCallback), this);
                    if (result.CompletedSynchronously)
                    {
                        return OnWaitForCanPersist(result);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            [Fx.Tag.SecurityNote(Critical = "Critical because it accesses UnsafeNativeMethods.QueryPerformanceCounter.",
                Safe = "Safe because we only make the call if PartialTrustHelper.AppDomainFullyTrusted is true.")]
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

            static void OnLockAcquired(object state, Exception asyncException)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)state;

                if (asyncException != null)
                {
                    // AcquireLock does not return an exception unless it doesn't have the lock
                    thisPtr.Complete(false, asyncException);

                    return;
                }

                thisPtr.ownsLock = true;

                bool completeSelf = true;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.PassGuard();
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }
                finally
                {
                    if (completeSelf)
                    {
                        Fx.Assert(!thisPtr.isWorkflowThread, "We should never be calling ReleaseLock if this is the workflow thread.");

                        thisPtr.instance.ReleaseLock(ref thisPtr.ownsLock, thisPtr.isIdlePolicyPersist && thisPtr.tryResult);
                    }
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            bool OpenProvider()
            {
                if (this.operation == PersistenceOperation.Unload)
                {
                    if (this.instance.state != State.Suspended && !this.instance.IsIdle)
                    {
                        if (this.isTry)
                        {
                            this.tryResult = false;
                            return true;
                        }
                        // Force unload
                    }

                    // Release the last referenceCount
                    if (!this.instance.TryReleaseLastReference())
                    {
                        if (this.isTry)
                        {
                            this.tryResult = false;
                            return true;
                        }
                        // Force unload
                    }
                }

                // We finally have the lock and are passed the guard.  Let's update our operation if this is an Unload.
                if (this.operation == PersistenceOperation.Unload && this.instance.Controller.State == WorkflowInstanceState.Complete)
                {
                    this.operation = PersistenceOperation.Delete;
                }

                bool completedSync = false;

                if (this.instance.persistenceContext != null && this.instance.persistenceContext.State == CommunicationState.Created)
                {
                    IAsyncResult result = this.instance.persistenceContext.BeginOpen(timeoutHelper.RemainingTime(),
                        PrepareInnerAsyncCompletion(providerOpenedCallback), this);

                    if (result.CompletedSynchronously)
                    {
                        completedSync = OnProviderOpened(result);
                    }
                }
                else
                {
                    completedSync = Track();
                }

                return completedSync;
            }

            public static bool End(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = AsyncResult.End<UnloadOrPersistAsyncResult>(result);

                return thisPtr.tryResult;
            }

            static bool OutermostCallback(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;

                bool completeSelf = true;
                AsyncCompletion innerCallback = thisPtr.GetNextInnerAsyncCompletion();

                try
                {
                    completeSelf = innerCallback(result);
                }
                finally
                {
                    // We're exiting either on purpose or because of an exception
                    if (completeSelf)
                    {
                        if (thisPtr.updateState)
                        {
                            if (thisPtr.saveStatus != SaveStatus.Locked)
                            {
                                // Stop execution if we've given up the instance lock
                                thisPtr.instance.isRunnable = false;
                            }

                            if (thisPtr.isUnloaded)
                            {
                                thisPtr.instance.MarkUnloaded();
                            }
                            if (thisPtr.isIdlePolicyPersist && thisPtr.tryResult)
                            {
                                thisPtr.instance.DecrementBusyCount();
                            }
                        }

                        // We don't want to release the lock if we're the workflow thread
                        if (!thisPtr.isWorkflowThread)
                        {
                            thisPtr.instance.ReleaseLock(ref thisPtr.ownsLock, thisPtr.isIdlePolicyPersist && thisPtr.tryResult);
                        }
                    }
                }

                return completeSelf;
            }

            AsyncCompletion GetNextInnerAsyncCompletion()
            {
                AsyncCompletion next = this.nextInnerAsyncCompletion;

                Fx.Assert(this.nextInnerAsyncCompletion != null, "Must have had one if we are calling GetNext");
                this.nextInnerAsyncCompletion = null;

                return next;
            }

            AsyncCallback PrepareInnerAsyncCompletion(AsyncCompletion innerCallback)
            {
                this.nextInnerAsyncCompletion = innerCallback;

                return PrepareAsyncCompletion(outermostCallback);
            }

            static bool OnWaitForCanPersist(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;

                thisPtr.instance.EndWaitForCanPersist(result, ref thisPtr.ownsLock);

                return thisPtr.OpenProvider();
            }

            static bool OnProviderOpened(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;

                thisPtr.instance.persistenceContext.EndOpen(result);

                return thisPtr.Track();
            }

            bool Track()
            {
                // Do the tracking before preparing in case the tracking data is being pushed into
                // an extension and persisted transactionally with the instance state.

                if (this.instance.persistenceContext != null)
                {
                    // We only track the persistence operation if we actually
                    // are persisting (and not just hitting PersistenceParticipants)
                    this.instance.TrackPersistence(this.operation);
                }

                if (this.instance.Controller.HasPendingTrackingRecords)
                {
                    IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.instance.trackTimeout, PrepareInnerAsyncCompletion(trackingCompleteCallback), this);
                    return SyncContinue(result);
                }
                else
                {
                    return CollectAndMap();
                }
            }

            static bool OnTrackingComplete(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;

                thisPtr.instance.Controller.EndFlushTrackingRecords(result);

                return thisPtr.CollectAndMap();
            }

            bool CollectAndMap()
            {
                // From this point forward we'll update the state unless we get a persistence exception
                this.updateState = true;

                Dictionary<XName, InstanceValue> initialPersistenceData = this.instance.GeneratePersistenceData();

                bool success = false;
                try
                {
                    List<IPersistencePipelineModule> modules = this.instance.PipelineModules;
                    if (modules != null)
                    {
                        Fx.Assert(modules.Count > 0, "should only setup modules if we have some");
                        this.pipeline = new PersistencePipeline(modules, initialPersistenceData);
                        this.pipeline.Collect();
                        this.pipeline.Map();
                        this.data = this.pipeline.Values;
                    }
                    else
                    {
                        this.data = initialPersistenceData;
                    }
                    success = true;
                }
                finally
                {
                    if (!success && this.context != null)
                    {
                        this.context.Abort();
                    }
                }

                if (this.instance.persistenceContext != null)
                {
                    return Persist();
                }
                else
                {
                    return Save();
                }
            }

            bool Persist()
            {
                IAsyncResult result = null;
                try
                {
                    if (this.operation == PersistenceOperation.Delete)
                    {
                        this.saveStatus = SaveStatus.Completed;
                    }

                    if (this.context == null)
                    {
                        this.context = new WorkflowPersistenceContext(this.instance, (this.pipeline != null && this.pipeline.IsSaveTransactionRequired) || this.isCompletionTransactionRequired,
                            this.dependentTransaction, this.instance.persistTimeout);
                    }

                    using (PrepareTransactionalCall(this.context.PublicTransaction))
                    {
                        result = this.instance.persistenceContext.BeginSave(this.data, this.saveStatus, this.instance.persistTimeout, PrepareInnerAsyncCompletion(persistedCallback), this);
                    }
                }
                catch (InstancePersistenceException)
                {
                    this.updateState = false;
                    throw;
                }
                finally
                {
                    if (result == null && this.context != null)
                    {
                        this.context.Abort();
                    }
                }

                return SyncContinue(result);
            }

            static bool OnPersisted(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;
                bool success = false;
                try
                {
                    thisPtr.instance.persistenceContext.EndSave(result);
                    success = true;
                }
                catch (InstancePersistenceException)
                {
                    thisPtr.updateState = false;
                    throw;
                }
                finally
                {
                    if (!success)
                    {
                        thisPtr.context.Abort();
                    }
                }

                return thisPtr.Save();
            }

            bool Save()
            {
                if (this.pipeline != null)
                {
                    IAsyncResult result = null;
                    try
                    {
                        if (this.context == null)
                        {
                            this.context = new WorkflowPersistenceContext(this.instance, this.pipeline.IsSaveTransactionRequired || this.isCompletionTransactionRequired,
                                this.dependentTransaction, this.instance.persistTimeout);
                        }

                        this.instance.persistencePipelineInUse = this.pipeline;
                        Thread.MemoryBarrier();
                        if (this.instance.abortingExtensions)
                        {
                            throw FxTrace.Exception.AsError(new OperationCanceledException(SR.DefaultAbortReason));
                        }

                        using (PrepareTransactionalCall(this.context.PublicTransaction))
                        {
                            result = this.pipeline.BeginSave(this.timeoutHelper.RemainingTime(), PrepareInnerAsyncCompletion(savedCallback), this);
                        }
                    }
                    finally
                    {
                        if (result == null)
                        {
                            this.instance.persistencePipelineInUse = null;
                            if (this.context != null)
                            {
                                this.context.Abort();
                            }
                        }
                    }
                    return SyncContinue(result);
                }
                else
                {
                    return NotifyCompletion();
                }
            }

            static bool OnSaved(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;

                bool success = false;
                try
                {
                    thisPtr.pipeline.EndSave(result);
                    success = true;
                }
                finally
                {
                    thisPtr.instance.persistencePipelineInUse = null;
                    if (!success)
                    {
                        thisPtr.context.Abort();
                    }
                }

                return thisPtr.NotifyCompletion();
            }

            bool NotifyCompletion()
            {
                if (this.isUnloaded && this.instance.Controller.State == WorkflowInstanceState.Complete && this.instance.creationContext != null)
                {
                    IAsyncResult result = null;
                    try
                    {
                        if (this.context == null)
                        {
                            this.context = new WorkflowPersistenceContext(this.instance, this.isCompletionTransactionRequired,
                                this.dependentTransaction, this.instance.persistTimeout);
                        }

                        using (PrepareTransactionalCall(this.context.PublicTransaction))
                        {
                            result = this.instance.creationContext.OnBeginWorkflowCompleted(this.instance.completionState, this.instance.workflowOutputs, this.instance.terminationException,
                                this.timeoutHelper.RemainingTime(), PrepareInnerAsyncCompletion(notifyCompletionCallback), this);
                            if (result == null)
                            {
                                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowCompletionAsyncResultCannotBeNull));
                            }
                        }
                    }
                    finally
                    {
                        if (result == null && this.context != null)
                        {
                            this.context.Abort();
                        }
                    }
                    return SyncContinue(result);
                }
                else
                {
                    return CompleteContext();
                }
            }

            static bool OnNotifyCompletion(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;

                bool success = false;
                try
                {
                    thisPtr.instance.creationContext.OnEndWorkflowCompleted(result);
                    success = true;
                }
                finally
                {
                    if (!success)
                    {
                        thisPtr.context.Abort();
                    }
                }

                return thisPtr.CompleteContext();
            }

            bool CompleteContext()
            {
                bool wentAsync = false;
                IAsyncResult completeResult = null;

                // Computing Persist Duration. 
                if (this.operation == PersistenceOperation.Save)
                {
                    this.instance.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowPersistDuration(GetDuration());
                }

                if (this.context != null)
                {
                    wentAsync = this.context.TryBeginComplete(this.PrepareInnerAsyncCompletion(completeContextCallback), this, out completeResult);
                }

                // we have persisted deleted state.  this is to address TransactedTerminate avoiding 
                // multiple deleted persistence.
                this.instance.hasPersistedDeleted = this.operation == PersistenceOperation.Delete;

                if (wentAsync)
                {
                    Fx.Assert(completeResult != null, "We shouldn't have null here because we would have rethrown or gotten false for went async.");
                    return SyncContinue(completeResult);
                }
                else
                {
                    // We completed synchronously if we didn't get an async result out of
                    // TryBeginComplete
                    return true;
                }
                                
            }

            static bool OnCompleteContext(IAsyncResult result)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result.AsyncState;
                thisPtr.context.EndComplete(result);
                return true;
            }

            static void OnComplete(AsyncResult result, Exception exception)
            {
                UnloadOrPersistAsyncResult thisPtr = (UnloadOrPersistAsyncResult)result;
                if (thisPtr.dependentTransaction != null)
                {
                    thisPtr.dependentTransaction.Complete();
                }
            }
        }

        abstract class SimpleOperationAsyncResult : AsyncResult
        {
            static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(OnLockAcquired);
            static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);
            static AsyncCompletion handleEndPerformOperation;
            static AsyncCompletion handleEndTrack;

            protected WorkflowServiceInstance instance;
            protected TimeoutHelper timeoutHelper;
            protected bool ownsLock;

            protected SimpleOperationAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instance = instance;
                this.OperationTransaction = transaction;
                this.OnCompleting = onCompleting;
            }

            protected WorkflowServiceInstance Instance
            {
                get
                {
                    return this.instance;
                }
            }

            protected Transaction OperationTransaction
            {
                get;
                private set;
            }

            protected virtual bool IsSynchronousOperation
            {
                get
                {
                    return true;
                }
            }

            protected void Run(TimeSpan timeout)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);

                Exception completionException = null;
                bool completeSelf = true;

                if (this.instance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), ref this.ownsLock, lockAcquiredCallback, this))
                {
                    try
                    {
                        completeSelf = HandleLockAcquired();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completionException = exception;
                    }
                }
                else
                {
                    completeSelf = false;
                }

                if (completeSelf)
                {
                    Complete(true, completionException);
                }
            }

            static void OnLockAcquired(object state, Exception asyncException)
            {
                SimpleOperationAsyncResult thisPtr = (SimpleOperationAsyncResult)state;

                if (asyncException != null)
                {
                    thisPtr.Complete(false, asyncException);
                }
                else
                {
                    thisPtr.ownsLock = true;

                    Exception completionException = null;
                    bool completeSelf = true;

                    try
                    {
                        completeSelf = thisPtr.HandleLockAcquired();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        completionException = exception;
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }
            }

            bool HandleLockAcquired()
            {
                if (ValidateState())
                {
                    return AttachTransaction();
                }
                else
                {
                    return true;
                }
            }

            bool AttachTransaction()
            {
                if (this.OperationTransaction != null && this.Instance.transactionContext == null)
                {
                    this.Instance.transactionContext = new TransactionContext(this.Instance, this.OperationTransaction);
                    this.Instance.isInTransaction = true;
                    this.Instance.isRunnable = false;
                }

                if (this.IsSynchronousOperation)
                {
                    PerformOperation();
                    return Track();
                }
                else
                {
                    if (handleEndPerformOperation == null)
                    {
                        handleEndPerformOperation = new AsyncCompletion(HandleEndPerformOperation);
                    }

                    IAsyncResult result = BeginPerformOperation(PrepareAsyncCompletion(handleEndPerformOperation), this);
                    if (result.CompletedSynchronously)
                    {
                        return HandleEndPerformOperation(result);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            static bool HandleEndPerformOperation(IAsyncResult result)
            {
                SimpleOperationAsyncResult thisPtr = (SimpleOperationAsyncResult)result.AsyncState;
                thisPtr.EndPerformOperation(result);
                return thisPtr.Track();
            }

            bool Track()
            {
                // For aborted, the AbortInstance will handle tracking.
                if (this.instance.state != State.Aborted && this.instance.Controller.HasPendingTrackingRecords)
                {
                    if (handleEndTrack == null)
                    {
                        handleEndTrack = new AsyncCompletion(HandleEndTrack);
                    }

                    IAsyncResult result = this.instance.Controller.BeginFlushTrackingRecords(this.instance.trackTimeout, PrepareAsyncCompletion(handleEndTrack), this);
                    if (result.CompletedSynchronously)
                    {
                        return HandleEndTrack(result);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return ReleaseLock();
                }
            }

            static bool HandleEndTrack(IAsyncResult result)
            {
                SimpleOperationAsyncResult thisPtr = (SimpleOperationAsyncResult)result.AsyncState;
                thisPtr.instance.Controller.EndFlushTrackingRecords(result);
                return thisPtr.ReleaseLock();
            }

            bool ReleaseLock()
            {
                this.instance.ReleaseLock(ref this.ownsLock);
                PostOperation();
                return true;
            }

            static void Finally(AsyncResult result, Exception completionException)
            {
                SimpleOperationAsyncResult thisPtr = (SimpleOperationAsyncResult)result;
                if (thisPtr.ownsLock)
                {
                    thisPtr.instance.ReleaseLock(ref thisPtr.ownsLock);
                }
            }

            protected abstract bool ValidateState();
            protected abstract void PerformOperation();
            protected virtual IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }
            protected virtual void EndPerformOperation(IAsyncResult result)
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }
            protected abstract void PostOperation();
        }

        class TerminateAsyncResult : SimpleOperationAsyncResult
        {
            Exception reason;

            TerminateAsyncResult(WorkflowServiceInstance instance, Exception reason, Transaction transaction, AsyncCallback callback, object state)
                : base(instance, transaction, callback, state)
            {
                this.reason = reason;
            }

            public static TerminateAsyncResult Create(WorkflowServiceInstance instance, Exception reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                TerminateAsyncResult result = new TerminateAsyncResult(instance, reason, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TerminateAsyncResult>(result);
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForTerminate(this.OperationTransaction);
            }

            protected override void PerformOperation()
            {
                this.Instance.Controller.Terminate(reason);

                // Reset suspended reason for Cancel and Terminate
                if (this.Instance.persistenceContext.IsSuspended)
                {
                    this.Instance.persistenceContext.IsSuspended = false;
                    this.Instance.persistenceContext.SuspendedReason = null;
                }

                // For non-transacted, we used the normal pulse to complete/unload the workflow.
                if (!this.Instance.isInTransaction)
                {
                    this.Instance.isRunnable = true;
                    this.Instance.state = State.Active;
                }
                // For transacted, the unload will happen at Tx committed time.
                else 
                {
                    this.Instance.GetCompletionState();
                }
            }

            protected override void PostOperation()
            {
                this.Instance.CompletePendingOperations();
            }
        }

        class AbandonAsyncResult : SimpleOperationAsyncResult
        {
            Exception reason;

            // The shouldTrackAbort flag is only false when idle policy has TimeToPersist < TimeToUnload.
            bool shouldTrackAbort;

            AbandonAsyncResult(WorkflowServiceInstance instance, Exception reason, bool shouldTrackAbort, AsyncCallback callback, object state)
                : base(instance, null, callback, state)
            {
                this.reason = reason;
                this.shouldTrackAbort = shouldTrackAbort;
            }

            public static AbandonAsyncResult Create(WorkflowServiceInstance instance, Exception reason, bool shouldTrackAbort, TimeSpan timeout, AsyncCallback callback, object state)
            {
                AbandonAsyncResult result = new AbandonAsyncResult(instance, reason, shouldTrackAbort, callback, state);
                result.Run(timeout);
                return result;
            }

            protected override bool IsSynchronousOperation
            {
                get
                {
                    // We go through the synchronous code path only when we want to terminate the unload.
                    // We want to terminate the unload only when
                    // TimeToPersist < TimeToUnload AND instance is dirty and waiting to be persisted by idle policy.

                    // The hasDataToPersist flag should only be read under the executor lock.
                    if (!this.shouldTrackAbort && this.Instance.hasDataToPersist)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AbandonAsyncResult>(result);
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForAbort();
            }

            protected override void PerformOperation()
            {
                // This is the synchronous code path. This path terminates the unload and leaves the instance intact.
                Fx.Assert(!this.shouldTrackAbort && this.Instance.hasDataToPersist, "We should only get here when we need to terminate the unload.");

                // Since reference count has already been decremented to 0 by now, we should set it back to 1.
                this.Instance.RecoverLastReference();
            }

            protected override IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                try
                {
                    return this.Instance.persistenceContext.BeginRelease(this.Instance.persistTimeout, callback, state);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    this.Instance.AbortInstance(this.reason, true);
                    throw;
                }
            }

            protected override void EndPerformOperation(IAsyncResult result)
            {
                try
                {
                    this.Instance.persistenceContext.EndRelease(result);
                    if (!this.shouldTrackAbort && this.Instance.Controller.TrackingEnabled)
                    {
                        this.Instance.Controller.Track(new WorkflowInstanceRecord(this.Instance.Id, this.Instance.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Unloaded, this.Instance.DefinitionIdentity));
                    }
                    
                    if (!this.shouldTrackAbort)
                    {
                        this.instance.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowUnloaded();
                    }
                    
                    this.Instance.AbortInstance(this.reason, true, this.shouldTrackAbort);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    this.Instance.AbortInstance(this.reason, true);
                    throw;
                }
            }

            protected override void PostOperation()
            {
            }
        }

        class AbandonAndSuspendAsyncResult : SimpleOperationAsyncResult
        {
            Exception reason;

            AbandonAndSuspendAsyncResult(WorkflowServiceInstance instance, Exception reason, AsyncCallback callback, object state)
                : base(instance, null, callback, state)
            {
                this.reason = reason;
            }

            public static AbandonAndSuspendAsyncResult Create(WorkflowServiceInstance instance, Exception reason, TimeSpan timeout, AsyncCallback callback, object state)
            {
                AbandonAndSuspendAsyncResult result = new AbandonAndSuspendAsyncResult(instance, reason, callback, state);
                result.Run(timeout);
                return result;
            }

            protected override bool IsSynchronousOperation
            {
                get
                {
                    return false;
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AbandonAndSuspendAsyncResult>(result);
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForAbort();
            }

            protected override void PerformOperation()
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }

            protected override IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                try
                {
                    return this.Instance.persistenceContext.BeginUpdateSuspendMetadata(this.reason, this.Instance.persistTimeout, callback, state);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    this.Instance.AbortInstance(this.reason, true);
                    throw;
                }
            }

            protected override void EndPerformOperation(IAsyncResult result)
            {
                try
                {
                    this.Instance.persistenceContext.EndUpdateSuspendMetadata(result);
                    AbandonAndSuspendAsyncResult data = (AbandonAndSuspendAsyncResult)result.AsyncState;
                    if (this.Instance.Controller.TrackingEnabled)
                    {
                        this.Instance.Controller.Track(new WorkflowInstanceSuspendedRecord(this.Instance.Id, this.Instance.WorkflowDefinition.DisplayName, data.reason.Message, this.Instance.DefinitionIdentity));
                    }
                    
                    this.Instance.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowSuspended();
                }
                finally
                {
                    this.Instance.AbortInstance(this.reason, true);
                }
            }

            protected override void PostOperation()
            {
            }
        }

        class CancelAsyncResult : SimpleOperationAsyncResult
        {
            CancelAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state)
                : base(instance, transaction, callback, state)
            {
            }

            public static CancelAsyncResult Create(WorkflowServiceInstance instance, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                CancelAsyncResult result = new CancelAsyncResult(instance, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CancelAsyncResult>(result);
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForCancel(this.OperationTransaction);
            }

            protected override void PerformOperation()
            {
                this.Instance.Controller.ScheduleCancel();

                // Reset suspended reason for Cancel and Terminate
                if (this.Instance.persistenceContext.IsSuspended)
                {
                    this.Instance.persistenceContext.IsSuspended = false;
                    this.Instance.persistenceContext.SuspendedReason = null;
                }

                // Cancel implies a state change to runnable.
                if (!this.Instance.isInTransaction)
                {
                    this.Instance.isRunnable = true;
                    this.Instance.state = State.Active;
                }
                // For transacted, the unload will happen at Tx committed time.
                else
                {
                    this.Instance.isTransactedCancelled = true;
                }
            }

            protected override void PostOperation()
            {
                this.Instance.CompletePendingOperations();
            }
        }

        class RunAsyncResult : SimpleOperationAsyncResult
        {
            string operationName;

            RunAsyncResult(WorkflowServiceInstance instance, Transaction transaction, string operationName, AsyncCallback callback, object state)
                : base(instance, transaction, callback, state)
            {
                this.operationName = operationName;
            }

            public static RunAsyncResult Create(WorkflowServiceInstance instance, Transaction transaction, string operationName, TimeSpan timeout, AsyncCallback callback, object state)
            {
                RunAsyncResult result = new RunAsyncResult(instance, transaction, operationName, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<RunAsyncResult>(result);
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForRun(this.OperationTransaction, this.operationName);
            }

            protected override void PerformOperation()
            {
                if (!this.Instance.isInTransaction)
                {
                    this.Instance.RunCore();
                }
            }

            protected override void PostOperation()
            {
            }
        }

        class SuspendAsyncResult : SimpleOperationAsyncResult
        {
            bool isUnlocked;
            string reason;

            SuspendAsyncResult(WorkflowServiceInstance instance, bool isUnlocked, string reason, Transaction transaction, AsyncCallback callback, object state)
                : base(instance, transaction, callback, state)
            {
                this.isUnlocked = isUnlocked;
                this.reason = reason;
            }

            public static SuspendAsyncResult Create(WorkflowServiceInstance instance, bool isUnlocked, string reason, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                SuspendAsyncResult result = new SuspendAsyncResult(instance, isUnlocked, reason, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SuspendAsyncResult>(result);
            }

            protected override bool IsSynchronousOperation
            {
                get
                {
                    return false;
                }
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForSuspend(this.OperationTransaction);
            }

            protected override void PerformOperation()
            {
                throw Fx.AssertAndThrow("Should not reach here!");
            }

            protected override IAsyncResult BeginPerformOperation(AsyncCallback callback, object state)
            {
                return new SuspendCoreAsyncResult(this, callback, state);
            }

            protected override void EndPerformOperation(IAsyncResult result)
            {
                SuspendCoreAsyncResult.End(result);
            }

            protected override void PostOperation()
            {
                this.Instance.CompletePendingOperations();
            }

            class SuspendCoreAsyncResult : AsyncResult
            {
                static AsyncCompletion handleEndWaitForCanPersist = new AsyncCompletion(HandleEndWaitForCanPersist);

                SuspendAsyncResult parent;

                public SuspendCoreAsyncResult(SuspendAsyncResult parent, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.parent = parent;

                    IAsyncResult result = this.parent.Instance.BeginWaitForCanPersist(ref this.parent.ownsLock, this.parent.timeoutHelper.RemainingTime(),
                        PrepareAsyncCompletion(handleEndWaitForCanPersist), this);
                    if (SyncContinue(result))
                    {
                        this.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SuspendCoreAsyncResult>(result);
                }

                static bool HandleEndWaitForCanPersist(IAsyncResult result)
                {
                    SuspendCoreAsyncResult thisPtr = (SuspendCoreAsyncResult)result.AsyncState;
                    thisPtr.parent.Instance.EndWaitForCanPersist(result, ref thisPtr.parent.ownsLock);

                    thisPtr.parent.Instance.persistenceContext.IsSuspended = true;
                    thisPtr.parent.Instance.persistenceContext.SuspendedReason = thisPtr.parent.reason;
                    thisPtr.parent.Instance.state = State.Suspended;

                    if (thisPtr.parent.Instance.Controller.TrackingEnabled)
                    {
                        thisPtr.parent.Instance.Controller.Track(new WorkflowInstanceSuspendedRecord(thisPtr.parent.Instance.Id, thisPtr.parent.Instance.WorkflowDefinition.DisplayName, thisPtr.parent.reason, thisPtr.parent.Instance.DefinitionIdentity));
                    }

                    thisPtr.parent.instance.serviceHost.WorkflowServiceHostPerformanceCounters.WorkflowSuspended();

                    // This is to handle a corner case where Pause is called
                    // from an event handler:
                    //    Case 1: Called while executing - pauses the scheduler
                    //       in order to obtain the lock and ReleaseLock never
                    //       calls resume.
                    //    Case 2: Called while not executing - no need to pause
                    //       the scheduler because ReleaseLock makes sure never
                    //       to tell it to post.
                    //    Case 3: Called from UnhandledException handler - the
                    //       scheduler is unpaused and ReleaseLock doesn't
                    //       control the fate of this thread.  Instead, this
                    //       thread will return to the scheduler unless we
                    //       tell it to Pause here.
                    thisPtr.parent.Instance.Controller.RequestPause();

                    return true;
                }
            }
        }

        class UnsuspendAsyncResult : SimpleOperationAsyncResult
        {
            UnsuspendAsyncResult(WorkflowServiceInstance instance, Transaction transaction, AsyncCallback callback, object state)
                : base(instance, transaction, callback, state)
            {
            }

            public static UnsuspendAsyncResult Create(WorkflowServiceInstance instance, Transaction transaction, TimeSpan timeout, AsyncCallback callback, object state)
            {
                UnsuspendAsyncResult result = new UnsuspendAsyncResult(instance, transaction, callback, state);
                result.Run(timeout);
                return result;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<UnsuspendAsyncResult>(result);
            }

            protected override bool ValidateState()
            {
                return this.Instance.ValidateStateForUnsuspend(this.OperationTransaction);
            }

            protected override void PerformOperation()
            {
                if (!this.Instance.isInTransaction)
                {
                    this.Instance.isRunnable = true;
                }
                this.Instance.persistenceContext.IsSuspended = false;
                this.Instance.persistenceContext.SuspendedReason = null;
                this.Instance.state = State.Active;

                if (this.Instance.Controller.TrackingEnabled)
                {
                    this.Instance.Controller.Track(new WorkflowInstanceRecord(this.Instance.Id, this.Instance.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Unsuspended, this.Instance.DefinitionIdentity));
                }
            }

            protected override void PostOperation()
            {
            }
        }

        class AcquireLockOnIdleAsyncResult : AsyncResult
        {
            static FastAsyncCallback lockAcquiredCallback = new FastAsyncCallback(OnLockAcquired);
            static Action<object, TimeoutException> idleReceivedCallback = new Action<object, TimeoutException>(OnIdleReceived);

            AsyncWaitHandle idleEvent;
            WorkflowServiceInstance instance;
            TimeoutHelper timeoutHelper;
            bool acquiredLockAsynchronously;

            public AcquireLockOnIdleAsyncResult(WorkflowServiceInstance instance, TimeSpan timeout, ref bool ownsLock, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(!ownsLock, "We should never call acquire if we already think we own the lock.");

                // We cannot just hand off the lock if we are in a handler thread
                // because this might eventually go async (during the operation)
                // and we could have multiple operations occurring concurrently.

                this.instance = instance;
                this.timeoutHelper = new TimeoutHelper(timeout);

                bool incrementedActiveOperations = false;
                bool decrementActiveOperations = true;
                bool completeSelf = true;
                object lockToken = null;

                try
                {
                    lock (this.instance.activeOperationsLock)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.instance.activeOperations++;
                            incrementedActiveOperations = true;
                        }

                        this.instance.executorLock.SetupWaiter(ref lockToken);
                    }

                    completeSelf = this.instance.executorLock.EnterAsync(this.timeoutHelper.RemainingTime(), ref lockToken, ref ownsLock, lockAcquiredCallback, this);

                    // We don't want to decrement the count if we went async
                    // because the async callback will do the decrement
                    decrementActiveOperations = completeSelf;
                }
                finally
                {
                    if (incrementedActiveOperations && decrementActiveOperations)
                    {
                        lock (this.instance.activeOperationsLock)
                        {
                            this.instance.activeOperations--;
                        }
                    }

                    this.instance.executorLock.CleanupWaiter(lockToken, ref ownsLock);
                }

                if (completeSelf)
                {
                    if (CheckState(ref ownsLock))
                    {
                        Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AcquireLockOnIdleAsyncResult>(result);
            }

            public static void End(IAsyncResult result, ref bool ownsLock)
            {
                // We don't care about validating type because worst
                // case scenario we skip this section and validation
                // occurs in the base AsyncResult call.
                AcquireLockOnIdleAsyncResult thisPtr = result as AcquireLockOnIdleAsyncResult;

                if (thisPtr != null)
                {
                    ownsLock = thisPtr.acquiredLockAsynchronously;
                }

                AsyncResult.End<AcquireLockOnIdleAsyncResult>(result);
            }

            static void OnLockAcquired(object state, Exception asyncException)
            {
                AcquireLockOnIdleAsyncResult thisPtr = (AcquireLockOnIdleAsyncResult)state;

                lock (thisPtr.instance.activeOperationsLock)
                {
                    thisPtr.instance.activeOperations--;
                }

                if (asyncException != null)
                {
                    thisPtr.Complete(false, asyncException);
                    return;
                }

                bool completeSelf = true;
                Exception completionException = null;

                try
                {
                    thisPtr.acquiredLockAsynchronously = true;
                    completeSelf = thisPtr.CheckState(ref thisPtr.acquiredLockAsynchronously);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException = e;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            bool CheckState(ref bool ownsLock)
            {
                if (this.instance.state == State.Active && !this.instance.isRunnable)
                {
                    this.instance.RunCore();
                }

                // If instance state is non-Active, the AcquireOnIdle will succeed (WSI is doing nothing),
                // the caller is responsible for dealing with state vs. operation.
                // For instance, ResumeBookmark will call ValidateStateForResumeProtocolBookmark.
                if (this.instance.state == State.Active && this.instance.Controller.State == WorkflowInstanceState.Runnable)
                {
                    this.idleEvent = this.instance.SetupIdleWaiter(ref ownsLock);

                    try
                    {
                        if (this.idleEvent.WaitAsync(idleReceivedCallback, this, this.timeoutHelper.RemainingTime()))
                        {
                            ownsLock = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        if (this.instance.CleanupIdleWaiter(this.idleEvent, e, ref ownsLock))
                        {
                            throw;
                        }
                    }
                }

                return true;
            }

            static void OnIdleReceived(object state, TimeoutException asyncException)
            {
                AcquireLockOnIdleAsyncResult thisPtr = (AcquireLockOnIdleAsyncResult)state;

                if (asyncException != null)
                {
                    if (thisPtr.instance.CleanupIdleWaiter(thisPtr.idleEvent, asyncException, ref thisPtr.acquiredLockAsynchronously))
                    {
                        Fx.Assert(!thisPtr.acquiredLockAsynchronously, "We shouldn't own the lock if we're rethrowing");
                        thisPtr.Complete(false, asyncException);
                        return;
                    }

                    Fx.Assert(thisPtr.acquiredLockAsynchronously, "We should own the lock if we're ----ing");
                }

                thisPtr.acquiredLockAsynchronously = true;

                thisPtr.Complete(false, null);
            }
        }

        class WaitForCanPersistAsyncResult : AsyncResult
        {
            static Action<object, TimeoutException> onWaitEvent;
            static FastAsyncCallback onLockAcquired;

            WorkflowServiceInstance instance;
            TimeoutHelper timeoutHelper;
            bool ownsLock;
            bool mustWait;
            AsyncWaitHandle checkCanPersistEvent;

            public WaitForCanPersistAsyncResult(WorkflowServiceInstance instance, ref bool ownsLock, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.instance = instance;
                this.ownsLock = ownsLock;
                this.timeoutHelper = new TimeoutHelper(timeout);

                Fx.Assert(ownsLock, "Must be called under locked!");

                if (WaitForCanPersist())
                {
                    Complete(true);
                }
            }

            public static void End(IAsyncResult result, ref bool ownsLock)
            {
                // We don't care about validating type because worst
                // case scenario we skip this section and validation
                // occurs in the base AsyncResult call.
                WaitForCanPersistAsyncResult thisPtr = result as WaitForCanPersistAsyncResult;

                if (thisPtr != null)
                {
                    ownsLock = thisPtr.ownsLock;
                }

                AsyncResult.End<WaitForCanPersistAsyncResult>(result);
            }

            bool WaitForCanPersist()
            {
                if (this.instance.Controller.IsPersistable)
                {
                    return true;
                }

                this.instance.Controller.PauseWhenPersistable();

                this.mustWait = false;
                if (this.instance.IsIdle)
                {
                    if (this.checkCanPersistEvent == null)
                    {
                        this.checkCanPersistEvent = new AsyncWaitHandle(EventResetMode.AutoReset);
                    }

                    // Will be signaled when WF is paused.
                    this.instance.AddCheckCanPersistWaiter(this);
                    this.mustWait = true;
                }

                this.instance.ReleaseLock(ref this.ownsLock);

                if (this.mustWait)
                {
                    if (onWaitEvent == null)
                    {
                        onWaitEvent = new Action<object, TimeoutException>(OnWaitEvent);
                    }

                    if (this.checkCanPersistEvent.WaitAsync(onWaitEvent, this, this.timeoutHelper.RemainingTime()))
                    {
                        return HandleWaitEvent();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return HandleWaitEvent();
                }
            }

            static void OnWaitEvent(object state, TimeoutException asyncException)
            {
                WaitForCanPersistAsyncResult thisPtr = (WaitForCanPersistAsyncResult)state;

                if (asyncException != null)
                {
                    thisPtr.Complete(false, asyncException);
                    return;
                }

                bool completeSelf = true;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.HandleWaitEvent();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    completionException = exception;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            public void SetEvent(ref bool ownsLock)
            {
                this.ownsLock = ownsLock;
                ownsLock = false;
                this.checkCanPersistEvent.Set();
            }

            bool HandleWaitEvent()
            {
                return AcquireLockWithoutPause();
            }

            bool AcquireLockWithoutPause()
            {
                if (!this.instance.IsHandlerThread && !this.ownsLock)
                {
                    if (onLockAcquired == null)
                    {
                        onLockAcquired = new FastAsyncCallback(OnLockAcquired);
                    }

                    if (this.instance.AcquireLockAsync(this.timeoutHelper.RemainingTime(), false, true, ref this.ownsLock, onLockAcquired, this))
                    {
                        return HandleLockAcquired();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return HandleLockAcquired();
                }
            }

            static void OnLockAcquired(object state, Exception asyncException)
            {
                WaitForCanPersistAsyncResult thisPtr = (WaitForCanPersistAsyncResult)state;

                if (asyncException != null)
                {
                    thisPtr.Complete(false, asyncException);
                    return;
                }

                thisPtr.ownsLock = true;

                bool completeSelf = true;
                Exception completionException = null;

                try
                {
                    completeSelf = thisPtr.HandleLockAcquired();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    completionException = exception;
                }

                if (completeSelf)
                {
                    thisPtr.Complete(false, completionException);
                }
            }

            bool HandleLockAcquired()
            {
                this.instance.ValidateStateForPersist();
                return WaitForCanPersist();
            }
        }

        [Fx.Tag.SynchronizationPrimitive(Fx.Tag.BlocksUsing.PrivatePrimitive, SupportsAsync = true, ReleaseMethod = "Exit")]
        class WorkflowExecutionLock
        {
            static Action<object, TimeoutException> asyncWaiterSignaledCallback = new Action<object, TimeoutException>(OnAsyncWaiterSignaled);

            bool owned;
            WorkflowServiceInstance instance;

            [Fx.Tag.SynchronizationObject(Blocking = false)]
            object ThisLock = new object();

            [Fx.Tag.SynchronizationObject]
            List<object> waiters;

#if DEBUG
            StackTrace exitStack;
#endif

            public WorkflowExecutionLock(WorkflowServiceInstance instance)
            {
                this.instance = instance;
            }

            public bool IsLocked
            {
                get { return this.owned; }
            }

            List<object> Waiters
            {
                get
                {
                    if (waiters == null)
                    {
                        waiters = new List<object>();
                    }

                    return waiters;
                }
            }

            public void SetupWaiter(ref object token)
            {
                SetupWaiter(false, ref token);
            }

            // The token returned here must be fed to all Enter calls
            // and finally to CleanupWaiter by the thread that calls
            // SetupWaiter.  If the enter goes async (such as EnterAsync
            // might) then the caller should NOT call cleanup in the async
            // callback.
            public void SetupWaiter(bool isAbortPriority, ref object token)
            {
                lock (ThisLock)
                {
                    try
                    {
                    }
                    finally
                    {
                        token = new AsyncWaitHandle();

                        if (isAbortPriority)
                        {
                            this.Waiters.Insert(0, token);
                        }
                        else
                        {
                            this.Waiters.Add(token);
                        }
                    }
                }
            }

            public void CleanupWaiter(object token, ref bool ownsLock)
            {
                if (token != null)
                {
                    lock (ThisLock)
                    {
                        if (!this.waiters.Remove(token))
                        {
                            // If it is not in the list that means we've been
                            // signaled and now own the lock.

                            ownsLock = true;
                        }
                    }
                }
            }

            public void Enter(TimeSpan timeout, ref object token, ref bool ownsLock)
            {
                Fx.Assert(!ownsLock, "We should never attempt to get the lock if we think we own it.");

                if (!TryEnter(timeout, ref token, ref ownsLock))
                {
                    throw FxTrace.Exception.AsError(new TimeoutException(SR.TimeoutOnOperation(timeout)));
                }
            }

            public bool EnterAsync(TimeSpan timeout, ref object token, ref bool ownsLock, FastAsyncCallback callback, object state)
            {
                Fx.Assert(!ownsLock, "We should never attempt to get the lock if we think we own it.");
                Fx.Assert(callback != null, "must have a non-null call back for async purposes");
                Fx.Assert(token is AsyncWaitHandle, "The token must be an AsyncWaitHandle.");

                AsyncWaitHandle waitHandle = null;

                lock (ThisLock)
                {
                    if (!this.owned)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.owned = true;
                            ownsLock = true;
                        }

                        return true;
                    }

                    waitHandle = (AsyncWaitHandle)token;
                }

                bool result = false;

                if (waitHandle.WaitAsync(asyncWaiterSignaledCallback, new AsyncWaiterData(this, callback, state, waitHandle), timeout))
                {
                    Fx.Assert(!this.Waiters.Contains(waitHandle), "We should not have this wait handle in the list.");

                    // Since the waiter is only signaled when they own the lock we won't have
                    // to set owned to true if this returns true.  owned was never set to false
                    // by Exit in this case.

                    ownsLock = true;
                    result = true;
                }

                token = null;
                return result;
            }

            static void OnAsyncWaiterSignaled(object state, TimeoutException asyncException)
            {
                AsyncWaiterData asyncWaiter = (AsyncWaiterData)state;

                Exception completionException = asyncException;

                if (asyncException != null)
                {
                    lock (asyncWaiter.Owner.ThisLock)
                    {
                        if (!asyncWaiter.Owner.waiters.Remove(asyncWaiter.Token))
                        {
                            // We raced between timing out and getting signaled.
                            // We'll take the signal which means we now own the lock

                            completionException = null;
                        }
                    }
                }

                // Callers of EnterAsync take a null value for the exception to mean
                // that they own the lock.  Either we were signaled (asyncException was
                // null), we got the lock in a ----y way (we nulled the exception when
                // we found we weren't in the list), or we don't have the lock (asyncException
                // is non-null and we are passing it along).
                asyncWaiter.Callback(asyncWaiter.State, completionException);
            }

            public bool TryEnter(ref bool ownsLock)
            {
                Fx.Assert(!ownsLock, "We should never attempt to get the lock if we think we own it.");

                lock (ThisLock)
                {
                    if (!this.owned)
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.owned = true;
                            ownsLock = true;
                        }

                        return true;
                    }

                    return false;
                }
            }

            public bool TryEnter(TimeSpan timeout, ref object token, ref bool ownsLock)
            {
                Fx.Assert(!ownsLock, "We should never attempt to get the lock if we think we own it.");

                AsyncWaitHandle waiter = EnterCore(ref token, ref ownsLock);

                if (waiter != null)
                {
                    Fx.Assert(!ownsLock, "We should not have gotten a waiter if EnterCore gave us the lock.");

                    if (waiter.Wait(timeout))
                    {
                        ownsLock = true;
                        token = null;
                        return true;
                    }
                    else
                    {
                        // The waiter will be cleaned up by the caller
                        return false;
                    }
                }
                else
                {
                    Fx.Assert(ownsLock, "We didn't have a waiter which means we got the lock.");
                    return true;
                }
            }

            AsyncWaitHandle EnterCore(ref object token, ref bool ownsLock)
            {
                AsyncWaitHandle waiter = null;

                lock (ThisLock)
                {
                    if (this.owned)
                    {
                        if (token == null)
                        {
                            waiter = new AsyncWaitHandle();
                            this.Waiters.Add(waiter);
                        }
                        else
                        {
                            waiter = (AsyncWaitHandle)token;
                        }
                    }
                    else
                    {
                        try
                        {
                        }
                        finally
                        {
                            this.owned = true;
                            ownsLock = true;
                        }
                    }
                }

                return waiter;
            }

            // Returns false if the lock was not released, returns true if released.
            public bool Exit(bool keepLockIfNoWaiters, ref bool ownsLock)
            {
                Fx.Assert(ownsLock, "We shouldn't call Exit unless we think we own the lock.");

                AsyncWaitHandle waiter = null;

                lock (ThisLock)
                {
                    if (!this.owned)
                    {
                        string message = InternalSR.InvalidSemaphoreExit;

#if DEBUG
                        if (!Fx.FastDebug && exitStack != null)
                        {
                            string originalStack = exitStack.ToString().Replace("\r\n", "\r\n    ");
                            message = string.Format(CultureInfo.InvariantCulture,
                                "Object synchronization method was called from an unsynchronized block of code. Previous Exit(): {0}", originalStack);
                        }
#endif

                        throw FxTrace.Exception.AsError(new SynchronizationLockException(message));
                    }

                    if (this.waiters == null || this.waiters.Count == 0)
                    {
                        if (keepLockIfNoWaiters)
                        {
                            return false;
                        }
                        else
                        {
                            try
                            {
                            }
                            finally
                            {
                                this.owned = false;
                                ownsLock = false;
                                this.instance.StartUnloadInstancePolicyIfNecessary();
                            }

#if DEBUG
                            if (!Fx.FastDebug)
                            {
                                exitStack = new StackTrace();
                            }
#endif

                            return true;
                        }
                    }

                    waiter = (AsyncWaitHandle)this.waiters[0];
                    this.waiters.RemoveAt(0);
                }

                // We're giving up the lock to another thread which now has to
                // take care of releasing it
                waiter.Set();
                ownsLock = false;

                // This counts as a successful exit from the point of view
                // of callers of Exit.
                return true;
            }

            class AsyncWaiterData
            {
                public AsyncWaiterData(WorkflowExecutionLock owner, FastAsyncCallback callback, object state, object token)
                {
                    this.Owner = owner;
                    this.Callback = callback;
                    this.State = state;
                    this.Token = token;
                }

                public WorkflowExecutionLock Owner
                {
                    get;
                    private set;
                }

                public FastAsyncCallback Callback
                {
                    get;
                    private set;
                }

                public object State
                {
                    get;
                    private set;
                }

                public object Token
                {
                    get;
                    private set;
                }
            }
        }

        class UnhandledExceptionAsyncData
        {
            public UnhandledExceptionAsyncData(WorkflowServiceInstance instance, Exception exception, Activity exceptionSource)
            {
                this.Instance = instance;
                this.Exception = exception;
                this.ExceptionSource = exceptionSource;
            }

            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Tracking team is considering to provide the exception source as part of the WorkflowInstanceUnhandledException record")]
            public Activity ExceptionSource
            {
                get;
                private set;
            }

            public WorkflowServiceInstance Instance
            {
                get;
                private set;
            }

            public Exception Exception
            {
                get;
                private set;
            }
        }

        class WorkflowPersistenceContext
        {
            WorkflowServiceInstance instance;
            CommittableTransaction contextOwnedTransaction;
            Transaction clonedTransaction;

            public WorkflowPersistenceContext(WorkflowServiceInstance instance, bool transactionRequired, Transaction transactionToUse, TimeSpan transactionTimeout)
            {
                this.instance = instance;

                if (transactionToUse != null)
                {
                    this.clonedTransaction = transactionToUse;
                }
                else if (transactionRequired)
                {
                    this.contextOwnedTransaction = new CommittableTransaction(transactionTimeout);
                    // Clone it so that we don't pass a CommittableTransaction to the participants
                    this.clonedTransaction = this.contextOwnedTransaction.Clone();
                }
            }

            public Transaction PublicTransaction
            {
                get
                {
                    return this.clonedTransaction;
                }
            }

            public void Abort()
            {
                if (this.contextOwnedTransaction != null)
                {
                    try
                    {
                        this.contextOwnedTransaction.Rollback();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        // ---- these exceptions as we are already on the error path
                    }
                }
            }

            // Returns true if end needs to be called
            // Note: this is side effecting even if it returns false
            public bool TryBeginComplete(AsyncCallback callback, object state, out IAsyncResult result)
            {
                // In the interest of allocating less objects we don't implement
                // the full async pattern here.  Instead, we've flattened it to
                // do the sync part and then optionally delegate down to the inner
                // BeginCommit.
                if (this.contextOwnedTransaction != null)
                {
                    result = this.contextOwnedTransaction.BeginCommit(callback, state);
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            public void EndComplete(IAsyncResult result)
            {
                Fx.Assert(this.contextOwnedTransaction != null, "We must have a contextOwnedTransaction if we are calling End");

                this.contextOwnedTransaction.EndCommit(result);
            }
        }

        class UnloadInstancePolicyHelper
        {
            static Action<object> onTimerCallback = new Action<object>(OnTimerCallback);
            static AsyncCallback onPersistCallback = Fx.ThunkCallback(new AsyncCallback(PersistCallback));
            static AsyncCallback onUnloadCallback = Fx.ThunkCallback(new AsyncCallback(UnloadCallback));
            static AsyncCallback onUnlockAndAbortCallback = Fx.ThunkCallback(new AsyncCallback(UnlockAndAbortCallback));

            WorkflowServiceInstance instance;
            TimeSpan timeToPersist;
            TimeSpan timeToUnload;
            IOThreadTimer persistTimer;
            IOThreadTimer unloadTimer;
            bool cancelled;
            bool persistEnabled;
            bool unloadEnabled;

            public UnloadInstancePolicyHelper(WorkflowServiceInstance instance, TimeSpan timeToPersist, TimeSpan timeToUnload)
            {
                Fx.Assert(instance != null, String.Empty);

                this.instance = instance;
                this.timeToPersist = timeToPersist;
                this.timeToUnload = timeToUnload;
                this.persistEnabled = this.instance.persistenceContext.CanPersist && this.timeToPersist < this.timeToUnload;
                this.unloadEnabled = this.instance.persistenceContext.CanPersist && this.timeToUnload < TimeSpan.MaxValue;

                if (this.persistEnabled)
                {
                    this.persistTimer = new IOThreadTimer(onTimerCallback, new Action(Persist), true);
                }
                if (this.unloadEnabled)
                {
                    this.unloadTimer = new IOThreadTimer(onTimerCallback, new Action(Unload), true);
                }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Exceptions", "DoNotCatchGeneralExceptionTypes", MessageId = "System.ServiceModel.Activities.WorkflowServiceInstance+UnloadInstancePolicyHelper.OnTimerCallback(System.Object):System.Void", Justification = "The non-fatal exceptions will be traced")]
            static void OnTimerCallback(object state)
            {
                try
                {
                    ((Action)state).Invoke();
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    FxTrace.Exception.AsWarning(ex);
                }
            }

            public void Begin()
            {
                if (this.cancelled)
                {
                    this.cancelled = false;
                    if (this.persistEnabled)
                    {
                        Fx.Assert(this.persistTimer != null, "persistTimer cannot be null if persist is enabled");
                        SetTimer(this.persistTimer, this.timeToPersist);
                    }
                    else
                    {
                        if (this.instance.persistenceContext.CanPersist)
                        {
                            if (this.unloadEnabled)
                            {
                                Fx.Assert(this.unloadTimer != null, "unloadTimer cannot be null if unload is enabled");
                                SetTimer(this.unloadTimer, this.timeToUnload);
                            }
                        }
                    }
                }
            }

            public void Cancel()
            {
                this.cancelled = true;
                if (this.persistTimer != null)
                {
                    this.persistTimer.Cancel();
                }
                if (this.unloadTimer != null)
                {
                    this.unloadTimer.Cancel();
                }
            }

            void Persist()
            {
                try
                {
                    IAsyncResult result = this.instance.BeginPersist(true, TimeSpan.MaxValue, onPersistCallback, this);
                    if (result.CompletedSynchronously)
                    {
                        HandleEndPersist(result);
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    this.instance.AbortInstance(ex, false);
                }
            }

            static void PersistCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                UnloadInstancePolicyHelper thisPtr = (UnloadInstancePolicyHelper)result.AsyncState;
                try
                {
                    thisPtr.HandleEndPersist(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    thisPtr.instance.AbortInstance(ex, false);
                }
            }

            void HandleEndPersist(IAsyncResult result)
            {
                bool persistSucceeded = this.instance.EndPersist(result);

                if (!this.cancelled)
                {
                    if (this.instance.persistenceContext.CanPersist)
                    {
                        if (this.unloadEnabled)
                        {
                            Fx.Assert(this.unloadTimer != null, "unloadTimer cannot be null if unload is enabled");

                            if (persistSucceeded)
                            {
                                Fx.Assert(this.timeToUnload > this.timeToPersist, String.Empty);
                                SetTimer(this.unloadTimer, this.timeToUnload - this.timeToPersist);
                            }
                        }
                    }
                }
            }

            void SetTimer(IOThreadTimer timer, TimeSpan ts)
            {
                Fx.Assert(timer != null && ts >= TimeSpan.Zero, String.Empty);

                // It is ok to dirty read the state, the consistency will be ensured by persis/unload itself.
                if (this.instance.state == State.Suspended)
                {
                    // Unload/Persist immediately when suspended 
                    timer.Set(TimeSpan.Zero);
                }
                else
                {
                    timer.Set(ts);
                }
            }

            void Unload()
            {
                try
                {
                    if (this.persistEnabled)
                    {
                        // This is an optimization to avoid expensive redundant persist (already persisted).
                        // We will simply Unlock and Abort an instance.
                        IAsyncResult result = BeginUnlockAndAbort(TimeSpan.MaxValue, onUnlockAndAbortCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            EndUnlockAndAbort(result);
                        }
                    }
                    else
                    {
                        IAsyncResult result = this.instance.BeginReleaseInstance(true, TimeSpan.MaxValue, onUnloadCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            HandleEndUnload(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    this.instance.AbortInstance(ex, false);
                }
            }

            static void UnloadCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                UnloadInstancePolicyHelper thisPtr = (UnloadInstancePolicyHelper)result.AsyncState;
                try
                {
                    thisPtr.HandleEndUnload(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    // 
                    thisPtr.instance.AbortInstance(ex, false);
                }
            }

            void HandleEndUnload(IAsyncResult result)
            {
                this.instance.EndReleaseInstance(result);
            }

            IAsyncResult BeginUnlockAndAbort(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new UnlockAndAbortAsyncResult(this.instance, timeout, callback, state);
            }

            void EndUnlockAndAbort(IAsyncResult result)
            {
                UnlockAndAbortAsyncResult.End(result);
            }

            static void UnlockAndAbortCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                UnloadInstancePolicyHelper thisPtr = (UnloadInstancePolicyHelper)result.AsyncState;
                try
                {
                    thisPtr.EndUnlockAndAbort(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    thisPtr.instance.AbortInstance(ex, false);
                }
            }

            // This class provides a safe unlock and abort of the instance without persisting.
            // The synchronized mechanism is the same as ReleaseAsyncResult.
            class UnlockAndAbortAsyncResult : AsyncResult
            {
                static Action<AsyncResult, Exception> onCompleting = new Action<AsyncResult, Exception>(Finally);
                static FastAsyncCallback acquireCompletedCallback = new FastAsyncCallback(AcquireCompletedCallback);
                static AsyncCompletion handleEndAbandon;

                WorkflowServiceInstance instance;
                TimeoutHelper timeoutHelper;
                bool referenceAcquired;

                public UnlockAndAbortAsyncResult(WorkflowServiceInstance instance, TimeSpan timeout, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.instance = instance;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.OnCompleting = onCompleting;

                    Exception completionException = null;
                    bool completeSelf = true;

                    if (this.instance.acquireReferenceSemaphore.EnterAsync(this.timeoutHelper.RemainingTime(), acquireCompletedCallback, this))
                    {
                        try
                        {
                            completeSelf = this.HandleEndAcquireReference();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            completionException = exception;
                        }
                    }
                    else
                    {
                        completeSelf = false;
                    }

                    if (completeSelf)
                    {
                        Complete(true, completionException);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<UnlockAndAbortAsyncResult>(result);
                }

                static void AcquireCompletedCallback(object state, Exception completionException)
                {
                    UnlockAndAbortAsyncResult thisPtr = (UnlockAndAbortAsyncResult)state;

                    bool completeSelf = true;
                    if (completionException == null)
                    {
                        try
                        {
                            completeSelf = thisPtr.HandleEndAcquireReference();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            completionException = exception;
                        }
                    }

                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                bool HandleEndAcquireReference()
                {
                    this.referenceAcquired = true;

                    if (this.instance.TryReleaseLastReference())
                    {
                        if (handleEndAbandon == null)
                        {
                            handleEndAbandon = new AsyncCompletion(HandleEndAbandon);
                        }

                        IAsyncResult result = this.instance.BeginAbandon(new FaultException(OperationExecutionFault.CreateAbortedFault(SR.DefaultAbortReason)), false,
                            this.timeoutHelper.RemainingTime(), PrepareAsyncCompletion(handleEndAbandon), this);
                        return SyncContinue(result);
                    }
                    else
                    {
                        return true;
                    }
                }

                static bool HandleEndAbandon(IAsyncResult result)
                {
                    UnlockAndAbortAsyncResult thisPtr = (UnlockAndAbortAsyncResult)result.AsyncState;
                    thisPtr.instance.EndAbandon(result);

                    return thisPtr.ReleaseAcquiredReference();
                }

                bool ReleaseAcquiredReference()
                {
                    this.instance.acquireReferenceSemaphore.Exit();
                    this.referenceAcquired = false;
                    return true;
                }

                static void Finally(AsyncResult result, Exception completionException)
                {
                    UnlockAndAbortAsyncResult thisPtr = (UnlockAndAbortAsyncResult)result;
                    if (thisPtr.referenceAcquired)
                    {
                        thisPtr.ReleaseAcquiredReference();
                    }
                }
            }
        }

        class UnhandledExceptionPolicyHelper
        {
            static AsyncCallback operationCallback = Fx.ThunkCallback(new AsyncCallback(OperationCallback));

            WorkflowServiceInstance instance;
            WorkflowUnhandledExceptionAction action;

            public UnhandledExceptionPolicyHelper(WorkflowServiceInstance instance, WorkflowUnhandledExceptionAction action)
            {
                Fx.Assert(instance != null, "instance must not be null!");
                Fx.Assert(WorkflowUnhandledExceptionActionHelper.IsDefined(action), action + " is invalid!");
                this.instance = instance;
                this.action = action;
            }

            public void OnUnhandledException(UnhandledExceptionAsyncData data)
            {
                Fx.Assert(data != null, "data must not be null!");
                Fx.Assert(data.Exception != null, "data.Exception must not be null!");

                FxTrace.Exception.AsWarning(data.Exception);

                try
                {                   
                    IAsyncResult result;
                    if (this.action == WorkflowUnhandledExceptionAction.Cancel)
                    {
                        result = this.instance.BeginCancel(null, TimeSpan.MaxValue, operationCallback, data);
                    }
                    else if (this.action == WorkflowUnhandledExceptionAction.Terminate)
                    {
                        result = this.instance.BeginTerminate(data.Exception, null, TimeSpan.MaxValue, operationCallback, data);
                    }
                    else if (this.action == WorkflowUnhandledExceptionAction.AbandonAndSuspend)
                    {
                        this.instance.isRunnable = false;
                        // For non-durable WF, simply abandon.
                        if (this.instance.persistenceContext.CanPersist)
                        {
                            result = this.instance.BeginAbandonAndSuspend(data.Exception, TimeSpan.MaxValue, operationCallback, data);
                        }
                        else
                        {
                            result = this.instance.BeginAbandon(data.Exception, TimeSpan.MaxValue, operationCallback, data);
                        }
                    }
                    else
                    {
                        this.instance.isRunnable = false;
                        result = this.instance.BeginAbandon(data.Exception, TimeSpan.MaxValue, operationCallback, data);
                    }

                    if (result.CompletedSynchronously)
                    {
                        HandleEndOperation(result);
                    }
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    this.instance.AbortInstance(ex, true);
                }
            }

            static void OperationCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                UnhandledExceptionAsyncData data = (UnhandledExceptionAsyncData)result.AsyncState;
                UnhandledExceptionPolicyHelper thisPtr = data.Instance.UnhandledExceptionPolicy;
                try
                {
                    thisPtr.HandleEndOperation(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }
                    thisPtr.instance.AbortInstance(ex, false);
                }
            }

            void HandleEndOperation(IAsyncResult result)
            {
                if (this.action == WorkflowUnhandledExceptionAction.Cancel)
                {
                    this.instance.EndCancel(result);
                }
                else if (this.action == WorkflowUnhandledExceptionAction.Terminate)
                {
                    this.instance.EndTerminate(result);
                }
                else if (this.action == WorkflowUnhandledExceptionAction.AbandonAndSuspend)
                {
                    if (this.instance.persistenceContext.CanPersist)
                    {
                        this.instance.EndAbandonAndSuspend(result);
                    }
                    else
                    {
                        this.instance.EndAbandon(result);
                    }
                }
                else
                {
                    this.instance.EndAbandon(result);
                }
            }
        }
    }
}
