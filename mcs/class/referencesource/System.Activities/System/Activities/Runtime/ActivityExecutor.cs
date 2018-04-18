//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Activities.Debugger;
    using System.Activities.DynamicUpdate;
    using System.Activities.Hosting;
    using System.Activities.Tracking;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.DurableInstancing;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Transactions;

    [DataContract(Name = XD.Executor.Name, Namespace = XD.Runtime.Namespace)]
    class ActivityExecutor : IEnlistmentNotification
    {
        static ReadOnlyCollection<BookmarkInfo> emptyBookmarkInfoCollection;

        BookmarkManager bookmarkManager;

        BookmarkScopeManager bookmarkScopeManager;

        DebugController debugController;
        bool hasRaisedWorkflowStarted;

        Guid instanceId;
        bool instanceIdSet;

        Activity rootElement;
        Dictionary<ActivityInstance, AsyncOperationContext> activeOperations;
        WorkflowInstance host;

        ActivityInstanceMap instanceMap;
        MappableObjectManager mappableObjectManager;

        bool hasTrackedStarted;

        long nextTrackingRecordNumber;

        ActivityInstance rootInstance;
        List<ActivityInstance> executingSecondaryRootInstances;

        Scheduler scheduler;

        Exception completionException;

        bool shouldRaiseMainBodyComplete;

        long lastInstanceId;

        LocationEnvironment rootEnvironment;

        IDictionary<string, object> workflowOutputs;

        Bookmark mainRootCompleteBookmark;

        // This field reflects our best guess at our future completion state.
        // We set it when the main root completes but might revise the value
        // depending on what actions are taken (like CancelRootActivity being
        // called).
        ActivityInstanceState executionState;

        Queue<PersistenceWaiter> persistenceWaiters;

        Quack<TransactionContextWaiter> transactionContextWaiters;
        RuntimeTransactionData runtimeTransaction;

        bool isAbortPending;
        bool isDisposed;
        bool shouldPauseOnCanPersist;

        bool isTerminatePending;
        Exception terminationPendingException;

        int noPersistCount;

        SymbolResolver symbolResolver;

        bool throwDuringSerialization;
        
        CodeActivityContext cachedResolutionContext;
        Location ignorableResultLocation;

        // work item pools (for performance)
        Pool<EmptyWorkItem> emptyWorkItemPool;
        Pool<ExecuteActivityWorkItem> executeActivityWorkItemPool;
        Pool<ExecuteSynchronousExpressionWorkItem> executeSynchronousExpressionWorkItemPool;
        Pool<CompletionCallbackWrapper.CompletionWorkItem> completionWorkItemPool;
        Pool<ResolveNextArgumentWorkItem> resolveNextArgumentWorkItemPool;

        // context pools (for performance)
        Pool<CodeActivityContext> codeActivityContextPool;
        Pool<NativeActivityContext> nativeActivityContextPool;

        // root handles (for default Tx, Correlation, etc)
        ExecutionPropertyManager rootPropertyManager;

        // This list keeps track of handles that are created and initialized.
        List<Handle> handles;

        bool persistExceptions;
        bool havePersistExceptionsValue;

        public ActivityExecutor(WorkflowInstance host)
        {
            Fx.Assert(host != null, "There must be a host.");

            this.host = host;
            this.WorkflowIdentity = host.DefinitionIdentity;

            this.bookmarkManager = new BookmarkManager();
            this.scheduler = new Scheduler(new Scheduler.Callbacks(this));
        }

        public Pool<EmptyWorkItem> EmptyWorkItemPool
        {
            get
            {
                if (this.emptyWorkItemPool == null)
                {
                    this.emptyWorkItemPool = new PoolOfEmptyWorkItems();
                }

                return this.emptyWorkItemPool;
            }
        }

        Pool<ExecuteActivityWorkItem> ExecuteActivityWorkItemPool
        {
            get
            {
                if (this.executeActivityWorkItemPool == null)
                {
                    this.executeActivityWorkItemPool = new PoolOfExecuteActivityWorkItems();
                }

                return this.executeActivityWorkItemPool;
            }
        }

        public Pool<ExecuteSynchronousExpressionWorkItem> ExecuteSynchronousExpressionWorkItemPool
        {
            get
            {
                if (this.executeSynchronousExpressionWorkItemPool == null)
                {
                    this.executeSynchronousExpressionWorkItemPool = new PoolOfExecuteSynchronousExpressionWorkItems();
                }

                return this.executeSynchronousExpressionWorkItemPool;
            }
        }

        public Pool<CompletionCallbackWrapper.CompletionWorkItem> CompletionWorkItemPool
        {
            get
            {
                if (this.completionWorkItemPool == null)
                {
                    this.completionWorkItemPool = new PoolOfCompletionWorkItems();
                }

                return this.completionWorkItemPool;
            }
        }

        public Pool<CodeActivityContext> CodeActivityContextPool
        {
            get
            {
                if (this.codeActivityContextPool == null)
                {
                    this.codeActivityContextPool = new PoolOfCodeActivityContexts();
                }

                return this.codeActivityContextPool;
            }
        }

        public Pool<NativeActivityContext> NativeActivityContextPool
        {
            get
            {
                if (this.nativeActivityContextPool == null)
                {
                    this.nativeActivityContextPool = new PoolOfNativeActivityContexts();
                }

                return this.nativeActivityContextPool;
            }
        }

        public Pool<ResolveNextArgumentWorkItem> ResolveNextArgumentWorkItemPool
        {
            get
            {
                if (this.resolveNextArgumentWorkItemPool == null)
                {
                    this.resolveNextArgumentWorkItemPool = new PoolOfResolveNextArgumentWorkItems();
                }

                return this.resolveNextArgumentWorkItemPool;
            }
        }

        public Activity RootActivity
        {
            get
            {
                return this.rootElement;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return this.host != null;
            }
        }

        public bool HasPendingTrackingRecords
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.HasPendingRecords;
            }
        }

        public bool ShouldTrack
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrack;
            }
        }

        public bool ShouldTrackBookmarkResumptionRecords
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackBookmarkResumptionRecords;
            }
        }

        public bool ShouldTrackActivityScheduledRecords
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityScheduledRecords;
            }
        }

        public bool ShouldTrackActivityStateRecords
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityStateRecords;
            }
        }

        public bool ShouldTrackActivityStateRecordsExecutingState
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityStateRecordsExecutingState;
            }
        }

        public bool ShouldTrackActivityStateRecordsClosedState
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackActivityStateRecordsClosedState;
            }
        }

        public bool ShouldTrackCancelRequestedRecords
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackCancelRequestedRecords;
            }
        }

        public bool ShouldTrackFaultPropagationRecords
        {
            get
            {
                return this.host.HasTrackingParticipant && this.host.TrackingProvider.ShouldTrackFaultPropagationRecords;
            }
        }

        public SymbolResolver SymbolResolver
        {
            get
            {
                if (this.symbolResolver == null)
                {
                    try
                    {
                        this.symbolResolver = this.host.GetExtension<SymbolResolver>();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        throw FxTrace.Exception.AsError(new CallbackException(SR.CallbackExceptionFromHostGetExtension(this.WorkflowInstanceId), e));
                    }
                }

                return this.symbolResolver;
            }
        }

        // This only gets accessed by root activities which are resolving arguments.  Since that
        // could at most be the real root and any secondary roots it doesn't seem necessary
        // to cache the empty environment.
        public LocationEnvironment EmptyEnvironment
        {
            get
            {
                return new LocationEnvironment(this, null);
            }
        }

        public ActivityInstanceState State
        {
            get
            {
                if ((this.executingSecondaryRootInstances != null && this.executingSecondaryRootInstances.Count > 0) ||
                    (this.rootInstance != null && !this.rootInstance.IsCompleted))
                {
                    // As long as some root is executing we need to return executing
                    return ActivityInstanceState.Executing;
                }
                else
                {
                    return this.executionState;
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public WorkflowIdentity WorkflowIdentity
        {
            get;
            internal set;
        }

        [DataMember]
        public Guid WorkflowInstanceId
        {
            get
            {
                if (!this.instanceIdSet)
                {
                    WorkflowInstanceId = this.host.Id;
                    if (!this.instanceIdSet)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.EmptyIdReturnedFromHost(this.host.GetType())));
                    }
                }

                return this.instanceId;
            }
            // Internal visibility for partial trust serialization purposes only.
            internal set
            {
                this.instanceId = value;
                this.instanceIdSet = value != Guid.Empty;
            }
        }

        public Exception TerminationException
        {
            get
            {
                return this.completionException;
            }
        }

        public bool IsRunning
        {
            get
            {
                return !this.isDisposed && this.scheduler.IsRunning;
            }
        }

        public bool IsPersistable
        {
            get
            {
                return this.noPersistCount == 0;
            }
        }

        public bool IsAbortPending
        {
            get
            {
                return this.isAbortPending;
            }
        }

        public bool IsIdle
        {
            get
            {
                return this.isDisposed || this.scheduler.IsIdle;
            }
        }

        public bool IsTerminatePending
        {
            get
            {
                return this.isTerminatePending;
            }
        }

        public bool KeysAllowed
        {
            get
            {
                return this.host.SupportsInstanceKeys;
            }
        }

        public IDictionary<string, object> WorkflowOutputs
        {
            get
            {
                return this.workflowOutputs;
            }
        }

        internal BookmarkScopeManager BookmarkScopeManager
        {
            get
            {
                if (this.bookmarkScopeManager == null)
                {
                    this.bookmarkScopeManager = new BookmarkScopeManager();
                }

                return this.bookmarkScopeManager;
            }
        }

        internal BookmarkScopeManager RawBookmarkScopeManager
        {
            get
            {
                return this.bookmarkScopeManager;
            }
        }

        internal BookmarkManager RawBookmarkManager
        {
            get
            {
                return this.bookmarkManager;
            }
        }

        internal MappableObjectManager MappableObjectManager
        {
            get
            {
                if (this.mappableObjectManager == null)
                {
                    this.mappableObjectManager = new MappableObjectManager();
                }

                return this.mappableObjectManager;
            }
        }

        public bool RequiresTransactionContextWaiterExists
        {
            get
            {
                return this.transactionContextWaiters != null && this.transactionContextWaiters.Count > 0 && this.transactionContextWaiters[0].IsRequires;
            }
        }

        public bool HasRuntimeTransaction
        {
            get { return this.runtimeTransaction != null; }
        }

        public Transaction CurrentTransaction
        {
            get
            {
                if (this.runtimeTransaction != null)
                {
                    return this.runtimeTransaction.ClonedTransaction;
                }
                else
                {
                    return null;
                }
            }
        }

        static ReadOnlyCollection<BookmarkInfo> EmptyBookmarkInfoCollection
        {
            get
            {
                if (emptyBookmarkInfoCollection == null)
                {
                    emptyBookmarkInfoCollection = new ReadOnlyCollection<BookmarkInfo>(new List<BookmarkInfo>(0));
                }

                return emptyBookmarkInfoCollection;
            }
        }

        [DataMember(Name = XD.Executor.BookmarkManager, EmitDefaultValue = false)]
        internal BookmarkManager SerializedBookmarkManager
        {
            get { return this.bookmarkManager; }
            set { this.bookmarkManager = value; }
        }

        [DataMember(Name = XD.Executor.BookmarkScopeManager, EmitDefaultValue = false)]
        internal BookmarkScopeManager SerializedBookmarkScopeManager
        {
            get { return this.bookmarkScopeManager; }
            set { this.bookmarkScopeManager = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "hasTrackedStarted")]
        internal bool SerializedHasTrackedStarted
        {
            get { return this.hasTrackedStarted; }
            set { this.hasTrackedStarted = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "nextTrackingRecordNumber")]
        internal long SerializedNextTrackingRecordNumber
        {
            get { return this.nextTrackingRecordNumber; }
            set { this.nextTrackingRecordNumber = value; }
        }

        [DataMember(Name = XD.Executor.RootInstance, EmitDefaultValue = false)]
        internal ActivityInstance SerializedRootInstance
        {
            get { return this.rootInstance; }
            set { this.rootInstance = value; }
        }

        [DataMember(Name = XD.Executor.SchedulerMember, EmitDefaultValue = false)]
        internal Scheduler SerializedScheduler
        {
            get { return this.scheduler; }
            set { this.scheduler = value; }
        }

        [DataMember(Name = XD.Executor.ShouldRaiseMainBodyComplete, EmitDefaultValue = false)]
        internal bool SerializedShouldRaiseMainBodyComplete
        {
            get { return this.shouldRaiseMainBodyComplete; }
            set { this.shouldRaiseMainBodyComplete = value; }
        }

        [DataMember(Name = XD.Executor.LastInstanceId, EmitDefaultValue = false)]
        internal long SerializedLastInstanceId
        {
            get { return this.lastInstanceId; }
            set { this.lastInstanceId = value; }
        }

        [DataMember(Name = XD.Executor.RootEnvironment, EmitDefaultValue = false)]
        internal LocationEnvironment SerializedRootEnvironment
        {
            get { return this.rootEnvironment; }
            set { this.rootEnvironment = value; }
        }

        [DataMember(Name = XD.Executor.WorkflowOutputs, EmitDefaultValue = false)]
        internal IDictionary<string, object> SerializedWorkflowOutputs
        {
            get { return this.workflowOutputs; }
            set { this.workflowOutputs = value; }
        }

        [DataMember(Name = XD.Executor.MainRootCompleteBookmark, EmitDefaultValue = false)]
        internal Bookmark SerializedMainRootCompleteBookmark
        {
            get { return this.mainRootCompleteBookmark; }
            set { this.mainRootCompleteBookmark = value; }
        }

        [DataMember(Name = XD.Executor.ExecutionState, EmitDefaultValue = false)]
        internal ActivityInstanceState SerializedExecutionState
        {
            get { return this.executionState; }
            set { this.executionState = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "handles")]
        internal List<Handle> SerializedHandles
        {
            get { return this.handles; }
            set { this.handles = value; }
        }

        internal bool PersistExceptions
        {
            get
            {
                if (!havePersistExceptionsValue)
                {
                    // If we have an ExceptionPersistenceExtension, set our cached "persistExceptions" value to its
                    // PersistExceptions property. If we don't have the extension, set the cached value to true.
                    ExceptionPersistenceExtension extension = this.host.GetExtension<ExceptionPersistenceExtension>();
                    if (extension != null)
                    {
                        this.persistExceptions = extension.PersistExceptions;
                    }
                    else
                    {
                        this.persistExceptions = true;
                    }

                    this.havePersistExceptionsValue = true;
                }
                return this.persistExceptions;
            }
        }

        [DataMember(Name = XD.Executor.CompletionException, EmitDefaultValue = false)]
        internal Exception SerializedCompletionException
        {
            get
            {
                if (this.PersistExceptions)
                {
                    return this.completionException;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.completionException = value;
            }
        }

        [DataMember(Name = XD.Executor.TransactionContextWaiters, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Used by serialization")]
        internal TransactionContextWaiter[] SerializedTransactionContextWaiters
        {
            get
            {
                if (this.transactionContextWaiters != null && this.transactionContextWaiters.Count > 0)
                {
                    return this.transactionContextWaiters.ToArray();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                Fx.Assert(value != null, "We don't serialize out null.");
                this.transactionContextWaiters = new Quack<TransactionContextWaiter>(value);
            }
        }

        [DataMember(Name = XD.Executor.PersistenceWaiters, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Used by serialization")]
        internal Queue<PersistenceWaiter> SerializedPersistenceWaiters
        {
            get
            {
                if (this.persistenceWaiters == null || this.persistenceWaiters.Count == 0)
                {
                    return null;
                }
                else
                {
                    return this.persistenceWaiters;
                }
            }
            set
            {
                Fx.Assert(value != null, "We don't serialize out null.");
                this.persistenceWaiters = value;
            }
        }

        [DataMember(Name = XD.Executor.SecondaryRootInstances, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Used by serialization")]
        internal List<ActivityInstance> SerializedExecutingSecondaryRootInstances
        {
            get
            {
                if (this.executingSecondaryRootInstances != null && this.executingSecondaryRootInstances.Count > 0)
                {
                    return this.executingSecondaryRootInstances;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                Fx.Assert(value != null, "We don't serialize out null.");
                this.executingSecondaryRootInstances = value;
            }
        }

        [DataMember(Name = XD.Executor.MappableObjectManager, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Used by serialization")]
        internal MappableObjectManager SerializedMappableObjectManager
        {
            get
            {
                if (this.mappableObjectManager == null || this.mappableObjectManager.Count == 0)
                {
                    return null;
                }

                return this.mappableObjectManager;
            }

            set
            {
                Fx.Assert(value != null, "value from serialization should never be null");
                this.mappableObjectManager = value;
            }
        }

        // map from activity names to (active) associated activity instances
        [DataMember(Name = XD.Executor.ActivityInstanceMap, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "called from serialization")]
        internal ActivityInstanceMap SerializedProgramMapping
        {
            get
            {
                ThrowIfNonSerializable();

                if (this.instanceMap == null && !this.isDisposed)
                {
                    this.instanceMap = new ActivityInstanceMap();

                    this.rootInstance.FillInstanceMap(this.instanceMap);
                    this.scheduler.FillInstanceMap(this.instanceMap);

                    if (this.executingSecondaryRootInstances != null && this.executingSecondaryRootInstances.Count > 0)
                    {
                        foreach (ActivityInstance secondaryRoot in this.executingSecondaryRootInstances)
                        {
                            secondaryRoot.FillInstanceMap(this.instanceMap);

                            LocationEnvironment environment = secondaryRoot.Environment;

                            if (secondaryRoot.IsEnvironmentOwner)
                            {
                                environment = environment.Parent;
                            }

                            while (environment != null)
                            {
                                if (environment.HasOwnerCompleted)
                                {
                                    this.instanceMap.AddEntry(environment, true);
                                }

                                environment = environment.Parent;
                            }
                        }
                    }
                }

                return this.instanceMap;
            }

            set
            {
                Fx.Assert(value != null, "value from serialization should never be null");
                this.instanceMap = value;
            }
        }

        // may be null
        internal ExecutionPropertyManager RootPropertyManager
        {
            get
            {
                return this.rootPropertyManager;
            }
        }

        [DataMember(Name = XD.ActivityInstance.PropertyManager, EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "Called from Serialization")]
        internal ExecutionPropertyManager SerializedPropertyManager
        {
            get
            {
                return this.rootPropertyManager;
            }
            set
            {
                Fx.Assert(value != null, "We don't emit the default value so this should never be null.");
                this.rootPropertyManager = value;
                this.rootPropertyManager.OnDeserialized(null, null, null, this);
            }
        }

        public void ThrowIfNonSerializable()
        {
            if (this.throwDuringSerialization)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.StateCannotBeSerialized(this.WorkflowInstanceId)));
            }
        }

        public void MakeNonSerializable()
        {
            this.throwDuringSerialization = true;
        }

        public IList<ActivityBlockingUpdate> GetActivitiesBlockingUpdate(DynamicUpdateMap updateMap)
        {
            Fx.Assert(updateMap != null, "UpdateMap must not be null.");
            Collection<ActivityBlockingUpdate> result = null;
            this.instanceMap.GetActivitiesBlockingUpdate(updateMap, this.executingSecondaryRootInstances, ref result);
            return result;
        }

        public void UpdateInstancePhase1(DynamicUpdateMap updateMap, Activity targetDefinition, ref Collection<ActivityBlockingUpdate> updateErrors)
        {
            Fx.Assert(updateMap != null, "UpdateMap must not be null.");
            this.instanceMap.UpdateRawInstance(updateMap, targetDefinition, this.executingSecondaryRootInstances, ref updateErrors);
        }

        public void UpdateInstancePhase2(DynamicUpdateMap updateMap, ref Collection<ActivityBlockingUpdate> updateErrors)
        {
            this.instanceMap.UpdateInstanceByActivityParticipation(this, updateMap, ref updateErrors);
        }

        internal List<Handle> Handles
        {
            get { return this.handles; }
        }

        // evaluate an argument/variable expression using fast-path optimizations
        public void ExecuteInResolutionContextUntyped(ActivityInstance parentInstance, ActivityWithResult expressionActivity, long instanceId, Location resultLocation)
        {
            if (this.cachedResolutionContext == null)
            {
                this.cachedResolutionContext = new CodeActivityContext(parentInstance, this);
            }

            this.cachedResolutionContext.Reinitialize(parentInstance, this, expressionActivity, instanceId);
            try
            {
                this.ignorableResultLocation = resultLocation;
                resultLocation.Value = expressionActivity.InternalExecuteInResolutionContextUntyped(this.cachedResolutionContext);
            }
            finally
            {
                if (!expressionActivity.UseOldFastPath)
                {
                    // The old fast path allows WorkflowDataContexts to escape up one level, because
                    // the resolution context uses the parent's ActivityInstance. We support that for
                    // back-compat, but don't allow it on new fast-path activities.
                    this.cachedResolutionContext.DisposeDataContext();
                }

                this.cachedResolutionContext.Dispose();
                this.ignorableResultLocation = null;
            }
        }

        // evaluate an argument/variable expression using fast-path optimizations
        public T ExecuteInResolutionContext<T>(ActivityInstance parentInstance, Activity<T> expressionActivity)
        {
            Fx.Assert(expressionActivity.UseOldFastPath, "New fast path should be scheduled via ExecuteSynchronousExpressionWorkItem, which calls the Untyped overload");

            if (this.cachedResolutionContext == null)
            {
                this.cachedResolutionContext = new CodeActivityContext(parentInstance, this);
            }

            this.cachedResolutionContext.Reinitialize(parentInstance, this, expressionActivity, parentInstance.InternalId);
            T result;
            try
            {
                result = expressionActivity.InternalExecuteInResolutionContext(this.cachedResolutionContext);
            }
            finally
            {
                this.cachedResolutionContext.Dispose();
            }
            return result;
        }

        internal void ExecuteSynchronousWorkItem(WorkItem workItem)
        {
            workItem.Release(this);
            try
            {
                bool result = workItem.Execute(this, bookmarkManager);
                Fx.AssertAndThrow(result, "Synchronous work item should not yield the scheduler");
            }
            finally
            {
                workItem.Dispose(this);
            }
        }

        internal void ExitNoPersistForExceptionPropagation()
        {
            if (!this.PersistExceptions)
            {
                ExitNoPersist();
            }
        }

        // This is called by RuntimeArgument.GetLocation (via ActivityContext.GetIgnorableResultLocation)
        // when the user tries to access the Result argument on an activity being run with SkipArgumentResolution.
        internal Location GetIgnorableResultLocation(RuntimeArgument resultArgument)
        {
            Fx.Assert(resultArgument.Owner == this.cachedResolutionContext.Activity, "GetIgnorableResultLocation should only be called for activity in resolution context");
            Fx.Assert(this.ignorableResultLocation != null, "ResultLocation should have been passed in to ExecuteInResolutionContext");

            return this.ignorableResultLocation;
        }

        // Whether it is being debugged.
        bool IsDebugged()
        {
            if (this.debugController == null)
            {
#if DEBUG
                if (Fx.StealthDebugger)
                {
                    return false;
                }
#endif
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    this.debugController = new DebugController(this.host);
                }
            }
            return this.debugController != null;
        }

        public void DebugActivityCompleted(ActivityInstance instance)
        {
            if (this.debugController != null)   // Don't use IsDebugged() for perf reason.
            {
                this.debugController.ActivityCompleted(instance);
            }
        }

        public void AddTrackingRecord(TrackingRecord record)
        {
            Fx.Assert(this.host.TrackingProvider != null, "We should only add records if we have a tracking provider.");

            this.host.TrackingProvider.AddRecord(record);
        }

        public bool ShouldTrackActivity(string name)
        {
            Fx.Assert(this.host.TrackingProvider != null, "We should only add records if we have a tracking provider.");
            return this.host.TrackingProvider.ShouldTrackActivity(name);
        }

        public IAsyncResult BeginTrackPendingRecords(AsyncCallback callback, object state)
        {
            Fx.Assert(this.host.TrackingProvider != null, "We should only try to track if we have a tracking provider.");
            return this.host.BeginFlushTrackingRecordsInternal(callback, state);
        }

        public void EndTrackPendingRecords(IAsyncResult result)
        {
            Fx.Assert(this.host.TrackingProvider != null, "We should only try to track if we have a tracking provider.");
            this.host.EndFlushTrackingRecordsInternal(result);
        }

        internal IDictionary<string, LocationInfo> GatherMappableVariables()
        {
            if (this.mappableObjectManager != null)
            {
                return this.MappableObjectManager.GatherMappableVariables();
            }
            return null;
        }

        internal void OnSchedulerThreadAcquired()
        {
            if (this.IsDebugged() && !this.hasRaisedWorkflowStarted)
            {
                this.hasRaisedWorkflowStarted = true;
                this.debugController.WorkflowStarted();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool aborting)
        {
            if (!this.isDisposed)
            {
                if (this.debugController != null)   // Don't use IsDebugged() because it may create debugController unnecessarily.
                {
                    this.debugController.WorkflowCompleted();
                    this.debugController = null;
                }

                if (this.activeOperations != null && this.activeOperations.Count > 0)
                {
                    Fx.Assert(aborting, "shouldn't get here in the g----ful close case");
                    Abort(new OperationCanceledException());
                }
                else
                {
                    this.scheduler.ClearAllWorkItems(this);

                    if (!aborting)
                    {
                        this.scheduler = null;
                        this.bookmarkManager = null;
                        this.lastInstanceId = 0;
                        this.rootInstance = null;
                    }

                    this.isDisposed = true;
                }
            }
        }

        // Called from an arbitrary thread
        public void PauseWhenPersistable()
        {
            this.shouldPauseOnCanPersist = true;
        }

        public void EnterNoPersist()
        {
            this.noPersistCount++;

            if (TD.EnterNoPersistBlockIsEnabled())
            {
                TD.EnterNoPersistBlock();
            }
        }

        public void ExitNoPersist()
        {
            this.noPersistCount--;

            if (TD.ExitNoPersistBlockIsEnabled())
            {
                TD.ExitNoPersistBlock();
            }

            if (this.shouldPauseOnCanPersist && this.IsPersistable)
            {
                // shouldPauseOnCanPersist is reset at the next pause
                // notification
                this.scheduler.Pause();
            }
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            // Because of ordering we might get this notification after we've already
            // determined the outcome

            // Get a local copy of this.runtimeTransaction because it is possible for
            // this.runtimeTransaction to be nulled out between the time we check for null
            // and the time we try to lock it.
            RuntimeTransactionData localRuntimeTransaction = this.runtimeTransaction;

            if (localRuntimeTransaction != null)
            {
                AsyncWaitHandle completionEvent = null;

                lock (localRuntimeTransaction)
                {
                    completionEvent = localRuntimeTransaction.CompletionEvent;

                    localRuntimeTransaction.TransactionStatus = TransactionStatus.Committed;
                }

                enlistment.Done();

                if (completionEvent != null)
                {
                    completionEvent.Set();
                }
            }
            else
            {
                enlistment.Done();
            }
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            ((IEnlistmentNotification)this).Rollback(enlistment);
        }

        //Note - There is a scenario in the TransactedReceiveScope while dealing with server side WCF dispatcher created transactions, 
        //the activity instance will end up calling BeginCommit before finishing up its execution. By this we allow the executing TransactedReceiveScope activity to 
        //complete and the executor is "free" to respond to this Prepare notification as part of the commit processing of that server side transaction
        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            // Because of ordering we might get this notification after we've already
            // determined the outcome

            // Get a local copy of this.runtimeTransaction because it is possible for
            // this.runtimeTransaction to be nulled out between the time we check for null
            // and the time we try to lock it.
            RuntimeTransactionData localRuntimeTransaction = this.runtimeTransaction;

            if (localRuntimeTransaction != null)
            {
                bool callPrepared = false;

                lock (localRuntimeTransaction)
                {
                    if (localRuntimeTransaction.HasPrepared)
                    {
                        callPrepared = true;
                    }
                    else
                    {
                        localRuntimeTransaction.PendingPreparingEnlistment = preparingEnlistment;
                    }
                }

                if (callPrepared)
                {
                    preparingEnlistment.Prepared();
                }
            }
            else
            {
                preparingEnlistment.Prepared();
            }
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            // Because of ordering we might get this notification after we've already
            // determined the outcome

            // Get a local copy of this.runtimeTransaction because it is possible for
            // this.runtimeTransaction to be nulled out between the time we check for null
            // and the time we try to lock it.
            RuntimeTransactionData localRuntimeTransaction = this.runtimeTransaction;

            if (localRuntimeTransaction != null)
            {
                AsyncWaitHandle completionEvent = null;

                lock (localRuntimeTransaction)
                {
                    completionEvent = localRuntimeTransaction.CompletionEvent;

                    localRuntimeTransaction.TransactionStatus = TransactionStatus.Aborted;
                }

                enlistment.Done();

                if (completionEvent != null)
                {
                    completionEvent.Set();
                }
            }
            else
            {
                enlistment.Done();
            }
        }

        public void RequestTransactionContext(ActivityInstance instance, bool isRequires, RuntimeTransactionHandle handle, Action<NativeActivityTransactionContext, object> callback, object state)
        {
            if (isRequires)
            {
                EnterNoPersist();
            }

            if (this.transactionContextWaiters == null)
            {
                this.transactionContextWaiters = new Quack<TransactionContextWaiter>();
            }

            TransactionContextWaiter waiter = new TransactionContextWaiter(instance, isRequires, handle, new TransactionContextWaiterCallbackWrapper(callback, instance), state);

            if (isRequires)
            {
                Fx.Assert(this.transactionContextWaiters.Count == 0 || !this.transactionContextWaiters[0].IsRequires, "Either we don't have any waiters or the first one better not be IsRequires == true");

                this.transactionContextWaiters.PushFront(waiter);
            }
            else
            {
                this.transactionContextWaiters.Enqueue(waiter);
            }

            instance.IncrementBusyCount();
            instance.WaitingForTransactionContext = true;
        }

        public void SetTransaction(RuntimeTransactionHandle handle, Transaction transaction, ActivityInstance isolationScope, ActivityInstance transactionOwner)
        {
            this.runtimeTransaction = new RuntimeTransactionData(handle, transaction, isolationScope);
            EnterNoPersist();

            // no more work to do for a host-declared transaction
            if (transactionOwner == null)
            {
                return;
            }

            Exception abortException = null;

            try
            {
                transaction.EnlistVolatile(this, EnlistmentOptions.EnlistDuringPrepareRequired);
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
                AbortWorkflowInstance(abortException);
            }
            else
            {
                if (TD.RuntimeTransactionSetIsEnabled())
                {
                    Fx.Assert(transactionOwner != null, "isolationScope and transactionOwner are either both null or both non-null");
                    TD.RuntimeTransactionSet(transactionOwner.Activity.GetType().ToString(), transactionOwner.Activity.DisplayName, transactionOwner.Id, isolationScope.Activity.GetType().ToString(), isolationScope.Activity.DisplayName, isolationScope.Id);
                }
            }
        }

        public void CompleteTransaction(RuntimeTransactionHandle handle, BookmarkCallback callback, ActivityInstance callbackOwner)
        {
            if (callback != null)
            {
                Bookmark bookmark = this.bookmarkManager.CreateBookmark(callback, callbackOwner, BookmarkOptions.None);
                ActivityExecutionWorkItem workItem;

                ActivityInstance isolationScope = null;

                if (this.runtimeTransaction != null)
                {
                    isolationScope = this.runtimeTransaction.IsolationScope;
                }

                this.bookmarkManager.TryGenerateWorkItem(this, false, ref bookmark, null, isolationScope, out workItem);
                this.scheduler.EnqueueWork(workItem);
            }

            if (this.runtimeTransaction != null && this.runtimeTransaction.TransactionHandle == handle)
            {
                this.runtimeTransaction.ShouldScheduleCompletion = true;

                if (TD.RuntimeTransactionCompletionRequestedIsEnabled())
                {
                    TD.RuntimeTransactionCompletionRequested(callbackOwner.Activity.GetType().ToString(), callbackOwner.Activity.DisplayName, callbackOwner.Id);
                }
            }
        }

        void SchedulePendingCancelation()
        {
            if (this.runtimeTransaction.IsRootCancelPending)
            {
                if (!this.rootInstance.IsCancellationRequested && !this.rootInstance.IsCompleted)
                {
                    this.rootInstance.IsCancellationRequested = true;
                    this.scheduler.PushWork(new CancelActivityWorkItem(this.rootInstance));
                }

                this.runtimeTransaction.IsRootCancelPending = false;
            }
        }

        public EmptyWorkItem CreateEmptyWorkItem(ActivityInstance instance)
        {
            EmptyWorkItem workItem = this.EmptyWorkItemPool.Acquire();
            workItem.Initialize(instance);

            return workItem;
        }

        public bool IsCompletingTransaction(ActivityInstance instance)
        {
            if (this.runtimeTransaction != null && this.runtimeTransaction.IsolationScope == instance)
            {
                // We add an empty work item to keep the instance alive
                this.scheduler.PushWork(CreateEmptyWorkItem(instance));

                // This will schedule the appopriate work item at the end of this work item
                this.runtimeTransaction.ShouldScheduleCompletion = true;

                if (TD.RuntimeTransactionCompletionRequestedIsEnabled())
                {
                    TD.RuntimeTransactionCompletionRequested(instance.Activity.GetType().ToString(), instance.Activity.DisplayName, instance.Id);
                }

                return true;
            }

            return false;
        }

        public void TerminateSpecialExecutionBlocks(ActivityInstance terminatedInstance, Exception terminationReason)
        {
            if (this.runtimeTransaction != null && this.runtimeTransaction.IsolationScope == terminatedInstance)
            {
                Exception abortException = null;

                try
                {
                    this.runtimeTransaction.Rollback(terminationReason);
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
                    // It is okay for us to call AbortWorkflowInstance even if we are already
                    // aborting the instance since it is an async call (IE - we asking the host
                    // to re-enter the instance to abandon it.
                    AbortWorkflowInstance(abortException);
                }

                SchedulePendingCancelation();

                ExitNoPersist();

                if (this.runtimeTransaction.TransactionHandle.AbortInstanceOnTransactionFailure)
                {
                    AbortWorkflowInstance(terminationReason);
                }

                this.runtimeTransaction = null;
            }
        }

        // Returns true if we actually performed the abort and false if we had already been disposed
        bool Abort(Exception terminationException, bool isTerminate)
        {
            if (!this.isDisposed)
            {
                if (!this.rootInstance.IsCompleted)
                {
                    this.rootInstance.Abort(this, this.bookmarkManager, terminationException, isTerminate);

                    // the Abort walk won't catch host-registered properties
                    if (this.rootPropertyManager != null)
                    {
                        if (isTerminate)
                        {
                            HandleInitializationContext context = new HandleInitializationContext(this, null);
                            foreach (ExecutionPropertyManager.ExecutionProperty executionProperty in this.rootPropertyManager.Properties.Values)
                            {
                                Handle handle = executionProperty.Property as Handle;
                                if (handle != null)
                                {
                                    handle.Uninitialize(context);
                                }
                            }
                            context.Dispose();
                        }

                        this.rootPropertyManager.UnregisterProperties(null, null, true);
                    }
                }

                if (this.executingSecondaryRootInstances != null)
                {
                    // We have to walk this list backwards because the abort
                    // path removes from this collection.
                    for (int i = this.executingSecondaryRootInstances.Count - 1; i >= 0; i--)
                    {
                        ActivityInstance secondaryRootInstance = this.executingSecondaryRootInstances[i];

                        Fx.Assert(!secondaryRootInstance.IsCompleted, "We should not have any complete instances in our list.");

                        secondaryRootInstance.Abort(this, this.bookmarkManager, terminationException, isTerminate);

                        Fx.Assert(this.executingSecondaryRootInstances.Count == i, "We are always working from the back and we should have removed the item we just aborted.");
                    }
                }

                // This must happen after we abort each activity.  This allows us to utilize code paths
                // which schedule work items.
                this.scheduler.ClearAllWorkItems(this);

                if (isTerminate)
                {
                    // Regardless of the previous state, a termination implies setting the
                    // completion exception and completing in the Faulted state.
                    this.completionException = terminationException;
                    this.executionState = ActivityInstanceState.Faulted;
                }

                this.Dispose();

                return true;
            }

            return false;
        }

        // Returns true if tracing was transfered
        bool TryTraceResume(out Guid oldActivityId)
        {
            if (FxTrace.Trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                oldActivityId = DiagnosticTraceBase.ActivityId;
                FxTrace.Trace.SetAndTraceTransfer(this.WorkflowInstanceId, true);

                if (TD.WorkflowActivityResumeIsEnabled())
                {
                    TD.WorkflowActivityResume(this.WorkflowInstanceId);
                }

                return true;
            }
            else
            {
                oldActivityId = Guid.Empty;
                return false;
            }
        }

        // Returns true if tracing was transfered
        bool TryTraceStart(out Guid oldActivityId)
        {
            if (FxTrace.Trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                oldActivityId = DiagnosticTraceBase.ActivityId;
                FxTrace.Trace.SetAndTraceTransfer(this.WorkflowInstanceId, true);

                if (TD.WorkflowActivityStartIsEnabled())
                {
                    TD.WorkflowActivityStart(this.WorkflowInstanceId);
                }

                return true;
            }
            else
            {
                oldActivityId = Guid.Empty;
                return false;
            }
        }

        void TraceSuspend(bool hasBeenResumed, Guid oldActivityId)
        {
            if (hasBeenResumed)
            {
                if (TD.WorkflowActivitySuspendIsEnabled())
                {
                    TD.WorkflowActivitySuspend(this.WorkflowInstanceId);
                }

                DiagnosticTraceBase.ActivityId = oldActivityId;
            }
        }

        public bool Abort(Exception reason)
        {
            Guid oldActivityId;
            bool hasTracedResume = TryTraceResume(out oldActivityId);

            bool abortResult = Abort(reason, false);

            TraceSuspend(hasTracedResume, oldActivityId);

            return abortResult;
        }

        // It must be okay for the runtime to be processing other
        // work on a different thread when this is called.  See
        // the comments in the method for justifications.
        public void AbortWorkflowInstance(Exception reason)
        {
            // 1) This flag is only ever set to true
            this.isAbortPending = true;

            // 2) This causes a couple of fields to be set
            this.host.Abort(reason);
            try
            {
                // 3) The host expects this to come from an unknown thread
                this.host.OnRequestAbort(reason);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                throw FxTrace.Exception.AsError(new CallbackException(SR.CallbackExceptionFromHostAbort(this.WorkflowInstanceId), e));
            }
        }

        public void ScheduleTerminate(Exception reason)
        {
            this.isTerminatePending = true;
            this.terminationPendingException = reason;
        }

        public void Terminate(Exception reason)
        {
            Fx.Assert(!this.isDisposed, "We should not have been able to get here if we are disposed and Abort makes choices based on isDisposed");

            Guid oldActivityId;
            bool hasTracedResume = TryTraceResume(out oldActivityId);

            Abort(reason, true);

            TraceSuspend(hasTracedResume, oldActivityId);
        }

        public void CancelRootActivity()
        {
            if (this.rootInstance.State == ActivityInstanceState.Executing)
            {
                if (!this.rootInstance.IsCancellationRequested)
                {
                    Guid oldActivityId;
                    bool hasTracedResume = TryTraceResume(out oldActivityId);

                    bool trackCancelRequested = true;

                    if (this.runtimeTransaction != null && this.runtimeTransaction.IsolationScope != null)
                    {
                        if (this.runtimeTransaction.IsRootCancelPending)
                        {
                            trackCancelRequested = false;
                        }

                        this.runtimeTransaction.IsRootCancelPending = true;
                    }
                    else
                    {
                        this.rootInstance.IsCancellationRequested = true;

                        if (this.rootInstance.HasNotExecuted)
                        {
                            this.scheduler.PushWork(CreateEmptyWorkItem(this.rootInstance));
                        }
                        else
                        {
                            this.scheduler.PushWork(new CancelActivityWorkItem(this.rootInstance));
                        }
                    }

                    if (this.ShouldTrackCancelRequestedRecords && trackCancelRequested)
                    {
                        AddTrackingRecord(new CancelRequestedRecord(this.WorkflowInstanceId, null, this.rootInstance));
                    }

                    TraceSuspend(hasTracedResume, oldActivityId);
                }
            }
            else if (this.rootInstance.State != ActivityInstanceState.Closed)
            {
                // We've been asked to cancel the instance and the root
                // completed in a canceled or faulted state.  By our rules
                // this means that the instance has been canceled.  A real
                // world example if the case of UnhandledExceptionAction.Cancel
                // on a workflow whose root activity threw an exception. The
                // expected completion state is Canceled and NOT Faulted.
                this.executionState = ActivityInstanceState.Canceled;
                this.completionException = null;
            }
        }

        public void CancelActivity(ActivityInstance activityInstance)
        {
            Fx.Assert(activityInstance != null, "The instance must not be null.");

            // Cancel is a no-op if the activity is complete or cancel has already been requested
            if (activityInstance.State != ActivityInstanceState.Executing || activityInstance.IsCancellationRequested)
            {
                return;
            }

            // Set that we have requested cancel.  This is our only guard against scheduling
            // ActivityInstance.Cancel multiple times.
            activityInstance.IsCancellationRequested = true;

            if (activityInstance.HasNotExecuted)
            {
                this.scheduler.PushWork(CreateEmptyWorkItem(activityInstance));
            }
            else
            {
                this.scheduler.PushWork(new CancelActivityWorkItem(activityInstance));
            }

            if (this.ShouldTrackCancelRequestedRecords)
            {
                AddTrackingRecord(new CancelRequestedRecord(this.WorkflowInstanceId, activityInstance.Parent, activityInstance));
            }
        }

        void PropagateException(WorkItem workItem)
        {
            ActivityInstance exceptionSource = workItem.ActivityInstance;
            Exception exception = workItem.ExceptionToPropagate;

            ActivityInstance exceptionPropagator = exceptionSource;
            FaultBookmark targetBookmark = null;

            // If we are not supposed to persist exceptions, call EnterNoPersist so that we don't persist while we are
            // propagating the exception.
            // We call ExitNoPersist when we abort an activit or when we call a fault callback. But we may end up
            // re-propagating and thus calling EnterNoPersist again.
            // We also do an exit if the workflow is aborted or the exception ends up being unhandled.
            if (!this.PersistExceptions)
            {
                EnterNoPersist();
            }
            while (exceptionPropagator != null && targetBookmark == null)
            {
                if (!exceptionPropagator.IsCompleted)
                {
                    if (this.runtimeTransaction != null && this.runtimeTransaction.IsolationScope == exceptionPropagator)
                    {
                        // We are propagating the exception across the isolation scope
                        this.scheduler.PushWork(new AbortActivityWorkItem(this, exceptionPropagator, exception, CreateActivityInstanceReference(workItem.OriginalExceptionSource, exceptionPropagator)));

                        // Because we are aborting the transaction we reset the ShouldScheduleCompletion flag
                        this.runtimeTransaction.ShouldScheduleCompletion = false;
                        workItem.ExceptionPropagated();
                        return;
                    }
                }

                if (exceptionPropagator.IsCancellationRequested)
                {
                    // Regardless of whether it is already completed or not we need
                    // to honor the workflow abort

                    this.AbortWorkflowInstance(new InvalidOperationException(SR.CannotPropagateExceptionWhileCanceling(exceptionSource.Activity.DisplayName, exceptionSource.Id), exception));
                    workItem.ExceptionPropagated();
                    ExitNoPersistForExceptionPropagation();
                    return;
                }

                if (exceptionPropagator.FaultBookmark != null)
                {
                    // This will cause us to break out of the loop
                    targetBookmark = exceptionPropagator.FaultBookmark;
                }
                else
                {
                    exceptionPropagator = exceptionPropagator.Parent;
                }
            }

            if (targetBookmark != null)
            {
                if (this.ShouldTrackFaultPropagationRecords)
                {
                    AddTrackingRecord(new FaultPropagationRecord(this.WorkflowInstanceId,
                                                                workItem.OriginalExceptionSource,
                                                                exceptionPropagator.Parent,
                                                                exceptionSource == workItem.OriginalExceptionSource,
                                                                exception));
                }

                this.scheduler.PushWork(targetBookmark.GenerateWorkItem(exception, exceptionPropagator, CreateActivityInstanceReference(workItem.OriginalExceptionSource, exceptionPropagator.Parent)));
                workItem.ExceptionPropagated();
            }
            else
            {
                if (this.ShouldTrackFaultPropagationRecords)
                {
                    AddTrackingRecord(new FaultPropagationRecord(this.WorkflowInstanceId,
                                                                workItem.OriginalExceptionSource,
                                                                null,
                                                                exceptionSource == workItem.OriginalExceptionSource,
                                                                exception));
                }
            }
        }

        internal ActivityInstanceReference CreateActivityInstanceReference(ActivityInstance toReference, ActivityInstance referenceOwner)
        {
            ActivityInstanceReference reference = new ActivityInstanceReference(toReference);

            if (this.instanceMap != null)
            {
                this.instanceMap.AddEntry(reference);
            }

            referenceOwner.AddActivityReference(reference);

            return reference;
        }

        internal void RethrowException(ActivityInstance fromInstance, FaultContext context)
        {
            this.scheduler.PushWork(new RethrowExceptionWorkItem(fromInstance, context.Exception, context.Source));
        }

        internal void OnDeserialized(Activity workflow, WorkflowInstance workflowInstance)
        {
            Fx.Assert(workflow != null, "The program must be non-null");
            Fx.Assert(workflowInstance != null, "The host must be non-null");

            if (!object.Equals(workflowInstance.DefinitionIdentity, this.WorkflowIdentity))
            {
                throw FxTrace.Exception.AsError(new VersionMismatchException(workflowInstance.DefinitionIdentity, this.WorkflowIdentity));
            }

            this.rootElement = workflow;
            this.host = workflowInstance;

            if (!this.instanceIdSet)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.EmptyGuidOnDeserializedInstance));
            }
            if (this.host.Id != this.instanceId)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.HostIdDoesNotMatchInstance(this.host.Id, this.instanceId)));
            }

            if (this.host.HasTrackingParticipant)
            {
                this.host.TrackingProvider.OnDeserialized(this.nextTrackingRecordNumber);
                this.host.OnDeserialized(this.hasTrackedStarted);
            }

            // hookup our callback to the scheduler
            if (this.scheduler != null)
            {
                this.scheduler.OnDeserialized(new Scheduler.Callbacks(this));
            }

            if (this.rootInstance != null)
            {
                Fx.Assert(this.instanceMap != null, "We always have an InstanceMap.");
                this.instanceMap.LoadActivityTree(workflow, this.rootInstance, this.executingSecondaryRootInstances, this);

                // We need to make sure that any "dangling" secondary root environments
                // get OnDeserialized called.
                if (this.executingSecondaryRootInstances != null)
                {
                    Fx.Assert(this.executingSecondaryRootInstances.Count > 0, "We don't serialize out an empty list.");

                    for (int i = 0; i < this.executingSecondaryRootInstances.Count; i++)
                    {
                        ActivityInstance secondaryRoot = this.executingSecondaryRootInstances[i];
                        LocationEnvironment environment = secondaryRoot.Environment.Parent;

                        if (environment != null)
                        {
                            environment.OnDeserialized(this, secondaryRoot);
                        }
                    }
                }
            }
            else
            {
                this.isDisposed = true;
            }
        }

        public T GetExtension<T>()
            where T : class
        {
            T extension = null;
            try
            {
                extension = this.host.GetExtension<T>();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                throw FxTrace.Exception.AsError(new CallbackException(SR.CallbackExceptionFromHostGetExtension(this.WorkflowInstanceId), e));
            }

            return extension;
        }

        internal Scheduler.RequestedAction TryExecuteNonEmptyWorkItem(WorkItem workItem)
        {
            Exception setupOrCleanupException = null;
            ActivityInstance propertyManagerOwner = workItem.PropertyManagerOwner;
            try
            {
                if (propertyManagerOwner != null && propertyManagerOwner.PropertyManager != null)
                {
                    try
                    {
                        propertyManagerOwner.PropertyManager.SetupWorkflowThread();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        setupOrCleanupException = e;
                    }
                }

                if (setupOrCleanupException == null)
                {
                    if (!workItem.Execute(this, this.bookmarkManager))
                    {
                        return Scheduler.YieldSilently;
                    }
                }
            }
            finally
            {
                // We might be multi-threaded when we execute code in
                // this finally block.  The work item might have gone
                // async and may already have called back into FinishWorkItem.
                if (propertyManagerOwner != null && propertyManagerOwner.PropertyManager != null)
                {
                    // This throws only fatal exceptions
                    propertyManagerOwner.PropertyManager.CleanupWorkflowThread(ref setupOrCleanupException);
                }

                if (setupOrCleanupException != null)
                {
                    // This API must allow the runtime to be
                    // multi-threaded when it is called.
                    AbortWorkflowInstance(new OperationCanceledException(SR.SetupOrCleanupWorkflowThreadThrew, setupOrCleanupException));
                }
            }

            if (setupOrCleanupException != null)
            {
                // We already aborted the instance in the finally block so
                // now we just need to return early.
                return Scheduler.Continue;
            }
            return null;
        }

        // callback from scheduler to process a work item
        internal Scheduler.RequestedAction OnExecuteWorkItem(WorkItem workItem)
        {
            workItem.Release(this);

            // thunk out early if the work item is no longer valid (that is, we're not in the Executing state)
            if (!workItem.IsValid)
            {
                return Scheduler.Continue;
            }

            if (!workItem.IsEmpty)
            {
                // The try/catch/finally block used in executing a workItem prevents ryujit from performing
                // some optimizations. Moving the functionality back into this method may cause a performance
                // regression.
                var result = TryExecuteNonEmptyWorkItem(workItem);
                if (result != null)
                {
                    return result;
                }
            }

            if (workItem.WorkflowAbortException != null)
            {
                AbortWorkflowInstance(new OperationCanceledException(SR.WorkItemAbortedInstance, workItem.WorkflowAbortException));
                return Scheduler.Continue;
            }

            // We only check this in the sync path because there are no ways of changing the keys collections from the work items that can
            // go async.  There's an assert to this effect in FinishWorkItem.
            if (this.bookmarkScopeManager != null && this.bookmarkScopeManager.HasKeysToUpdate)
            {
                if (!workItem.FlushBookmarkScopeKeys(this))
                {
                    return Scheduler.YieldSilently;
                }

                if (workItem.WorkflowAbortException != null)
                {
                    AbortWorkflowInstance(new OperationCanceledException(SR.WorkItemAbortedInstance, workItem.WorkflowAbortException));
                    return Scheduler.Continue;
                }
            }

            workItem.PostProcess(this);

            if (workItem.ExceptionToPropagate != null)
            {
                PropagateException(workItem);
            }

            if (this.HasPendingTrackingRecords)
            {
                if (!workItem.FlushTracking(this))
                {
                    return Scheduler.YieldSilently;
                }

                if (workItem.WorkflowAbortException != null)
                {
                    AbortWorkflowInstance(new OperationCanceledException(SR.TrackingRelatedWorkflowAbort, workItem.WorkflowAbortException));
                    return Scheduler.Continue;
                }
            }

            ScheduleRuntimeWorkItems();

            if (workItem.ExceptionToPropagate != null)
            {
                ExitNoPersistForExceptionPropagation();
                return Scheduler.CreateNotifyUnhandledExceptionAction(workItem.ExceptionToPropagate, workItem.OriginalExceptionSource);
            }

            return Scheduler.Continue;
        }

        internal IAsyncResult BeginAssociateKeys(ICollection<InstanceKey> keysToAssociate, AsyncCallback callback, object state)
        {
            return new AssociateKeysAsyncResult(this, keysToAssociate, callback, state);
        }

        internal void EndAssociateKeys(IAsyncResult result)
        {
            AssociateKeysAsyncResult.End(result);
        }

        internal void DisassociateKeys(ICollection<InstanceKey> keysToDisassociate)
        {
            this.host.OnDisassociateKeys(keysToDisassociate);
        }

        internal void FinishWorkItem(WorkItem workItem)
        {
            Scheduler.RequestedAction resumptionAction = Scheduler.Continue;

            try
            {
                Fx.Assert(this.bookmarkScopeManager == null || !this.bookmarkScopeManager.HasKeysToUpdate,
                    "FinishWorkItem should be called after FlushBookmarkScopeKeys, or by a WorkItem that could not possibly generate keys.");

                if (workItem.WorkflowAbortException != null)
                {
                    // We resume the scheduler even after abort to make sure that
                    // the proper events are raised.
                    AbortWorkflowInstance(new OperationCanceledException(SR.WorkItemAbortedInstance, workItem.WorkflowAbortException));
                }
                else
                {
                    workItem.PostProcess(this);

                    if (workItem.ExceptionToPropagate != null)
                    {
                        PropagateException(workItem);
                    }

                    if (this.HasPendingTrackingRecords)
                    {
                        if (!workItem.FlushTracking(this))
                        {
                            // We exit early here and will come back in at
                            // FinishWorkItemAfterTracking
                            resumptionAction = Scheduler.YieldSilently;
                            return;
                        }
                    }

                    if (workItem.WorkflowAbortException != null)
                    {
                        // We resume the scheduler even after abort to make sure that
                        // the proper events are raised.
                        AbortWorkflowInstance(new OperationCanceledException(SR.TrackingRelatedWorkflowAbort, workItem.WorkflowAbortException));
                    }
                    else
                    {
                        ScheduleRuntimeWorkItems();

                        if (workItem.ExceptionToPropagate != null)
                        {
                            ExitNoPersistForExceptionPropagation();
                            resumptionAction = Scheduler.CreateNotifyUnhandledExceptionAction(workItem.ExceptionToPropagate, workItem.OriginalExceptionSource);
                        }
                    }
                }
            }
            finally
            {
                if (resumptionAction != Scheduler.YieldSilently)
                {
                    workItem.Dispose(this);
                }
            }

            Fx.Assert(resumptionAction != Scheduler.YieldSilently, "should not reach this section if we've yielded earlier");
            this.scheduler.InternalResume(resumptionAction);
        }

        internal void FinishWorkItemAfterTracking(WorkItem workItem)
        {
            Scheduler.RequestedAction resumptionAction = Scheduler.Continue;

            try
            {
                if (workItem.WorkflowAbortException != null)
                {
                    // We resume the scheduler even after abort to make sure that
                    // the proper events are raised.
                    AbortWorkflowInstance(new OperationCanceledException(SR.TrackingRelatedWorkflowAbort, workItem.WorkflowAbortException));
                }
                else
                {
                    ScheduleRuntimeWorkItems();

                    if (workItem.ExceptionToPropagate != null)
                    {
                        ExitNoPersistForExceptionPropagation();
                        resumptionAction = Scheduler.CreateNotifyUnhandledExceptionAction(workItem.ExceptionToPropagate, workItem.OriginalExceptionSource);
                    }
                }
            }
            finally
            {
                workItem.Dispose(this);
            }

            this.scheduler.InternalResume(resumptionAction);
        }

        void ScheduleRuntimeWorkItems()
        {
            if (this.runtimeTransaction != null && this.runtimeTransaction.ShouldScheduleCompletion)
            {
                this.scheduler.PushWork(new CompleteTransactionWorkItem(this.runtimeTransaction.IsolationScope));
                return;
            }

            if (this.persistenceWaiters != null && this.persistenceWaiters.Count > 0 &&
                this.IsPersistable)
            {
                PersistenceWaiter waiter = this.persistenceWaiters.Dequeue();

                while (waiter != null && waiter.WaitingInstance.IsCompleted)
                {
                    // We just skip completed instance so we don't have to deal
                    // with the housekeeping are arbitrary removal from our
                    // queue

                    if (this.persistenceWaiters.Count == 0)
                    {
                        waiter = null;
                    }
                    else
                    {
                        waiter = this.persistenceWaiters.Dequeue();
                    }
                }

                if (waiter != null)
                {
                    this.scheduler.PushWork(waiter.CreateWorkItem());
                    return;
                }
            }
        }

        internal void AbortActivityInstance(ActivityInstance instance, Exception reason)
        {
            instance.Abort(this, this.bookmarkManager, reason, true);

            if (instance.CompletionBookmark != null)
            {
                instance.CompletionBookmark.CheckForCancelation();
            }
            else if (instance.Parent != null)
            {
                instance.CompletionBookmark = new CompletionBookmark();
            }

            ScheduleCompletionBookmark(instance);
        }

        internal Exception CompleteActivityInstance(ActivityInstance targetInstance)
        {
            Exception exceptionToPropagate = null;

            // 1. Handle any root related work
            HandleRootCompletion(targetInstance);

            // 2. Schedule the completion bookmark
            // We MUST schedule the completion bookmark before
            // we dispose the environment because we take this
            // opportunity to gather up any output values.
            ScheduleCompletionBookmark(targetInstance);

            if (!targetInstance.HasNotExecuted)
            {
                DebugActivityCompleted(targetInstance);
            }

            // 3. Cleanup environmental resources (properties, handles, mapped locations)
            try
            {
                if (targetInstance.PropertyManager != null)
                {
                    targetInstance.PropertyManager.UnregisterProperties(targetInstance, targetInstance.Activity.MemberOf);
                }

                if (IsSecondaryRoot(targetInstance))
                {
                    // We need to appropriately remove references, dispose
                    // environments, and remove instance map entries for
                    // all environments in this chain
                    LocationEnvironment environment = targetInstance.Environment;

                    if (targetInstance.IsEnvironmentOwner)
                    {
                        environment.RemoveReference(true);

                        if (environment.ShouldDispose)
                        {
                            // Unintialize all handles declared in this environment.  
                            environment.UninitializeHandles(targetInstance);

                            environment.Dispose();
                        }

                        environment = environment.Parent;
                    }

                    while (environment != null)
                    {
                        environment.RemoveReference(false);

                        if (environment.ShouldDispose)
                        {
                            // Unintialize all handles declared in this environment.  
                            environment.UninitializeHandles(targetInstance);

                            environment.Dispose();

                            // This also implies that the owner is complete so we should
                            // remove it from the map
                            if (this.instanceMap != null)
                            {
                                this.instanceMap.RemoveEntry(environment);
                            }
                        }

                        environment = environment.Parent;
                    }
                }
                else if (targetInstance.IsEnvironmentOwner)
                {
                    targetInstance.Environment.RemoveReference(true);

                    if (targetInstance.Environment.ShouldDispose)
                    {
                        // Unintialize all handles declared in this environment.  
                        targetInstance.Environment.UninitializeHandles(targetInstance);

                        targetInstance.Environment.Dispose();
                    }
                    else if (this.instanceMap != null)
                    {
                        // Someone else is referencing this environment
                        // Note that we don't use TryAdd since no-one else should have 
                        // added it before.
                        this.instanceMap.AddEntry(targetInstance.Environment);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                exceptionToPropagate = e;
            }

            // 4. Cleanup remaining instance related resources (bookmarks, program mapping)
            targetInstance.MarkAsComplete(this.bookmarkScopeManager, this.bookmarkManager);

            // 5. Track our final state
            targetInstance.FinalizeState(this, exceptionToPropagate != null);

            return exceptionToPropagate;
        }

        internal bool TryGetPendingOperation(ActivityInstance instance, out AsyncOperationContext asyncContext)
        {
            if (this.activeOperations != null)
            {
                return this.activeOperations.TryGetValue(instance, out asyncContext);
            }
            else
            {
                asyncContext = null;
                return false;
            }
        }

        internal void CancelPendingOperation(ActivityInstance instance)
        {
            AsyncOperationContext asyncContext;
            if (TryGetPendingOperation(instance, out asyncContext))
            {
                if (asyncContext.IsStillActive)
                {
                    asyncContext.CancelOperation();
                }
            }
        }

        internal void HandleRootCompletion(ActivityInstance completedInstance)
        {
            if (completedInstance.Parent == null)
            {
                if (completedInstance == this.rootInstance)
                {
                    this.shouldRaiseMainBodyComplete = true;

                    Fx.Assert(this.executionState == ActivityInstanceState.Executing, "We shouldn't have a guess at our completion state yet.");

                    // We start by assuming our completion state will match the root instance.
                    this.executionState = this.rootInstance.State;
                    this.rootEnvironment = this.rootInstance.Environment;
                }
                else
                {
                    Fx.Assert(this.executingSecondaryRootInstances.Contains(completedInstance), "An instance which is not the main root and doesn't have an execution parent must be an executing secondary root.");
                    this.executingSecondaryRootInstances.Remove(completedInstance);
                }

                // We just had a root complete, let's see if we're all the way done
                // and should gather outputs from the root.  Note that we wait until
                // everything completes in case the root environment was detached.
                if (this.rootInstance.IsCompleted
                    && (this.executingSecondaryRootInstances == null || this.executingSecondaryRootInstances.Count == 0))
                {
                    GatherRootOutputs();

                    // uninitialize any host-provided handles
                    if (this.rootPropertyManager != null)
                    {
                        // and uninitialize host-provided handles
                        HandleInitializationContext context = new HandleInitializationContext(this, null);
                        foreach (ExecutionPropertyManager.ExecutionProperty executionProperty in this.rootPropertyManager.Properties.Values)
                        {
                            Handle handle = executionProperty.Property as Handle;
                            if (handle != null)
                            {
                                handle.Uninitialize(context);
                            }
                        }
                        context.Dispose();

                        // unregister any properties that were registered
                        this.rootPropertyManager.UnregisterProperties(null, null);
                    }
                }
            }
        }

        bool IsSecondaryRoot(ActivityInstance instance)
        {
            return instance.Parent == null && instance != this.rootInstance;
        }

        void GatherRootOutputs()
        {
            Fx.Assert(this.workflowOutputs == null, "We should only get workflow outputs when we actually complete which should only happen once.");
            Fx.Assert(ActivityUtilities.IsCompletedState(this.rootInstance.State), "We should only gather outputs when in a completed state.");
            Fx.Assert(this.rootEnvironment != null, "We should have set the root environment");

            // We only gather outputs for Closed - not for canceled or faulted
            if (this.rootInstance.State == ActivityInstanceState.Closed)
            {
                // We use rootElement here instead of this.rootInstance.Activity
                // because we don't always reload the root instance (like if it
                // was complete when we last persisted).
                IList<RuntimeArgument> rootArguments = this.rootElement.RuntimeArguments;

                for (int i = 0; i < rootArguments.Count; i++)
                {
                    RuntimeArgument argument = rootArguments[i];

                    if (ArgumentDirectionHelper.IsOut(argument.Direction))
                    {
                        if (this.workflowOutputs == null)
                        {
                            this.workflowOutputs = new Dictionary<string, object>();
                        }

                        Location location = this.rootEnvironment.GetSpecificLocation(argument.BoundArgument.Id);
                        if (location == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NoOutputLocationWasFound(argument.Name)));
                        }
                        this.workflowOutputs.Add(argument.Name, location.Value);
                    }
                }
            }

            // GatherRootOutputs only ever gets called once so we can null it out the root environment now.
            this.rootEnvironment = null;
        }

        internal void NotifyUnhandledException(Exception exception, ActivityInstance source)
        {
            try
            {
                this.host.NotifyUnhandledException(exception, source.Activity, source.Id);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.AbortWorkflowInstance(e);
            }
        }

        internal void OnSchedulerIdle()
        {
            // If we're terminating we'll call terminate here and
            // then do the normal notification for the host.
            if (this.isTerminatePending)
            {
                Fx.Assert(this.terminationPendingException != null, "Should have set terminationPendingException at the same time that we set isTerminatePending = true");
                this.Terminate(this.terminationPendingException);
                this.isTerminatePending = false;
            }

            if (this.IsIdle)
            {
                if (this.transactionContextWaiters != null && this.transactionContextWaiters.Count > 0)
                {
                    if (this.IsPersistable || (this.transactionContextWaiters[0].IsRequires && this.noPersistCount == 1))
                    {
                        TransactionContextWaiter waiter = this.transactionContextWaiters.Dequeue();

                        waiter.WaitingInstance.DecrementBusyCount();
                        waiter.WaitingInstance.WaitingForTransactionContext = false;

                        ScheduleItem(new TransactionContextWorkItem(waiter));

                        MarkSchedulerRunning();
                        ResumeScheduler();

                        return;
                    }
                }

                if (this.shouldRaiseMainBodyComplete)
                {
                    this.shouldRaiseMainBodyComplete = false;
                    if (this.mainRootCompleteBookmark != null)
                    {
                        BookmarkResumptionResult resumptionResult = this.TryResumeUserBookmark(this.mainRootCompleteBookmark, this.rootInstance.State, false);
                        this.mainRootCompleteBookmark = null;
                        if (resumptionResult == BookmarkResumptionResult.Success)
                        {
                            this.MarkSchedulerRunning();
                            this.ResumeScheduler();
                            return;
                        }
                    }

                    if (this.executingSecondaryRootInstances == null || this.executingSecondaryRootInstances.Count == 0)
                    {
                        // if we got to this point we're completely done from the executor's point of view.
                        // outputs have been gathered, no more work is happening. Clear out some fields to shrink our 
                        // "completed instance" persistence size
                        Dispose(false);
                    }
                }
            }

            if (this.shouldPauseOnCanPersist && this.IsPersistable)
            {
                this.shouldPauseOnCanPersist = false;
            }

            try
            {
                this.host.NotifyPaused();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                this.AbortWorkflowInstance(e);
            }
        }

        public void Open(SynchronizationContext synchronizationContext)
        {
            this.scheduler.Open(synchronizationContext);
        }

        public void PauseScheduler()
        {
            // Since we don't require calls to WorkflowInstanceControl.Pause to be synchronized
            // by the caller, we need to check for null here
            Scheduler localScheduler = this.scheduler;

            if (localScheduler != null)
            {
                localScheduler.Pause();
            }
        }

        public object PrepareForSerialization()
        {
            if (this.host.HasTrackingParticipant)
            {
                this.nextTrackingRecordNumber = this.host.TrackingProvider.NextTrackingRecordNumber;
                this.hasTrackedStarted = this.host.HasTrackedStarted;
            }
            return this;
        }

        public void RequestPersist(Bookmark onPersistBookmark, ActivityInstance requestingInstance)
        {
            if (this.persistenceWaiters == null)
            {
                this.persistenceWaiters = new Queue<PersistenceWaiter>();
            }

            this.persistenceWaiters.Enqueue(new PersistenceWaiter(onPersistBookmark, requestingInstance));
        }

        void ScheduleCompletionBookmark(ActivityInstance completedInstance)
        {
            if (completedInstance.CompletionBookmark != null)
            {
                this.scheduler.PushWork(completedInstance.CompletionBookmark.GenerateWorkItem(completedInstance, this));
            }
            else if (completedInstance.Parent != null)
            {
                // Variable defaults and argument expressions always have a parent
                // and never have a CompletionBookmark
                if (completedInstance.State != ActivityInstanceState.Closed && completedInstance.Parent.HasNotExecuted)
                {
                    completedInstance.Parent.SetInitializationIncomplete();
                }

                this.scheduler.PushWork(CreateEmptyWorkItem(completedInstance.Parent));
            }
        }

        // This method is called by WorkflowInstance - these are bookmark resumptions
        // originated by the host
        internal BookmarkResumptionResult TryResumeHostBookmark(Bookmark bookmark, object value)
        {
            Guid oldActivityId;
            bool hasTracedResume = TryTraceResume(out oldActivityId);

            BookmarkResumptionResult result = TryResumeUserBookmark(bookmark, value, true);

            TraceSuspend(hasTracedResume, oldActivityId);

            return result;
        }

        internal BookmarkResumptionResult TryResumeUserBookmark(Bookmark bookmark, object value, bool isExternal)
        {
            if (this.isDisposed)
            {
                return BookmarkResumptionResult.NotFound;
            }

            ActivityInstance isolationInstance = null;

            if (this.runtimeTransaction != null)
            {
                isolationInstance = this.runtimeTransaction.IsolationScope;
            }

            ActivityExecutionWorkItem resumeExecutionWorkItem;

            BookmarkResumptionResult result = this.bookmarkManager.TryGenerateWorkItem(this, isExternal, ref bookmark, value, isolationInstance, out resumeExecutionWorkItem);

            if (result == BookmarkResumptionResult.Success)
            {
                this.scheduler.EnqueueWork(resumeExecutionWorkItem);

                if (this.ShouldTrackBookmarkResumptionRecords)
                {
                    AddTrackingRecord(new BookmarkResumptionRecord(this.WorkflowInstanceId, bookmark, resumeExecutionWorkItem.ActivityInstance, value));
                }
            }
            else if (result == BookmarkResumptionResult.NotReady)
            {
                // We had the bookmark but this is not an appropriate time to resume it
                // so we won't do anything here
            }
            else if (bookmark == Bookmark.AsyncOperationCompletionBookmark)
            {
                Fx.Assert(result == BookmarkResumptionResult.NotFound, "This BookmarkNotFound is actually a well-known bookmark.");

                AsyncOperationContext.CompleteData data = (AsyncOperationContext.CompleteData)value;

                data.CompleteOperation();

                result = BookmarkResumptionResult.Success;
            }

            return result;
        }

        internal ReadOnlyCollection<BookmarkInfo> GetAllBookmarks()
        {
            List<BookmarkInfo> bookmarks = CollectExternalBookmarks();

            if (bookmarks != null)
            {
                return new ReadOnlyCollection<BookmarkInfo>(bookmarks);
            }
            else
            {
                return EmptyBookmarkInfoCollection;
            }
        }

        List<BookmarkInfo> CollectExternalBookmarks()
        {
            List<BookmarkInfo> bookmarks = null;

            if (this.bookmarkManager != null && this.bookmarkManager.HasBookmarks)
            {
                bookmarks = new List<BookmarkInfo>();

                this.bookmarkManager.PopulateBookmarkInfo(bookmarks);
            }

            if (this.bookmarkScopeManager != null)
            {
                this.bookmarkScopeManager.PopulateBookmarkInfo(ref bookmarks);
            }

            if (bookmarks == null || bookmarks.Count == 0)
            {
                return null;
            }
            else
            {
                return bookmarks;
            }
        }

        internal ReadOnlyCollection<BookmarkInfo> GetBookmarks(BookmarkScope scope)
        {
            if (this.bookmarkScopeManager == null)
            {
                return EmptyBookmarkInfoCollection;
            }
            else
            {
                ReadOnlyCollection<BookmarkInfo> bookmarks = this.bookmarkScopeManager.GetBookmarks(scope);

                if (bookmarks == null)
                {
                    return EmptyBookmarkInfoCollection;
                }
                else
                {
                    return bookmarks;
                }
            }
        }

        internal IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.host.OnBeginResumeBookmark(bookmark, value, timeout, callback, state);
        }

        internal BookmarkResumptionResult EndResumeBookmark(IAsyncResult result)
        {
            return this.host.OnEndResumeBookmark(result);
        }

        // This is only called by WorkflowInstance so it behaves like TryResumeUserBookmark with must
        // run work item set to true
        internal BookmarkResumptionResult TryResumeBookmark(Bookmark bookmark, object value, BookmarkScope scope)
        {
            // We have to perform all of this work with tracing set up
            // since we might initialize a sub-instance while generating
            // the work item.
            Guid oldActivityId;
            bool hasTracedResume = TryTraceResume(out oldActivityId);

            ActivityInstance isolationInstance = null;

            if (this.runtimeTransaction != null)
            {
                isolationInstance = this.runtimeTransaction.IsolationScope;
            }

            bool hasOperations = this.activeOperations != null && this.activeOperations.Count > 0;

            ActivityExecutionWorkItem resumeExecutionWorkItem;
            BookmarkResumptionResult result = this.BookmarkScopeManager.TryGenerateWorkItem(this, ref bookmark, scope, value, isolationInstance, hasOperations || this.bookmarkManager.HasBookmarks, out resumeExecutionWorkItem);

            if (result == BookmarkResumptionResult.Success)
            {
                this.scheduler.EnqueueWork(resumeExecutionWorkItem);

                if (this.ShouldTrackBookmarkResumptionRecords)
                {
                    AddTrackingRecord(new BookmarkResumptionRecord(this.WorkflowInstanceId, bookmark, resumeExecutionWorkItem.ActivityInstance, value));
                }
            }

            TraceSuspend(hasTracedResume, oldActivityId);

            return result;
        }

        public void MarkSchedulerRunning()
        {
            this.scheduler.MarkRunning();
        }

        public void Run()
        {
            ResumeScheduler();
        }

        void ResumeScheduler()
        {
            this.scheduler.Resume();
        }

        internal void ScheduleItem(WorkItem workItem)
        {
            this.scheduler.PushWork(workItem);
        }

        public void ScheduleRootActivity(Activity activity, IDictionary<string, object> argumentValueOverrides, IList<Handle> hostProperties)
        {
            Fx.Assert(this.rootInstance == null, "ScheduleRootActivity should only be called once");

            if (hostProperties != null && hostProperties.Count > 0)
            {
                Dictionary<string, ExecutionPropertyManager.ExecutionProperty> rootProperties = new Dictionary<string, ExecutionPropertyManager.ExecutionProperty>(hostProperties.Count);
                HandleInitializationContext context = new HandleInitializationContext(this, null);
                for (int i = 0; i < hostProperties.Count; i++)
                {
                    Handle handle = hostProperties[i];
                    handle.Initialize(context);
                    rootProperties.Add(handle.ExecutionPropertyName, new ExecutionPropertyManager.ExecutionProperty(handle.ExecutionPropertyName, handle, null));
                }
                context.Dispose();

                this.rootPropertyManager = new ExecutionPropertyManager(null, rootProperties);
            }

            Guid oldActivityId;
            bool hasTracedStart = TryTraceStart(out oldActivityId);

            // Create and initialize the root instance
            this.rootInstance = new ActivityInstance(activity)
            {
                PropertyManager = this.rootPropertyManager
            };
            this.rootElement = activity;

            Fx.Assert(this.lastInstanceId == 0, "We should only hit this path once");
            this.lastInstanceId++;

            bool requiresSymbolResolution = this.rootInstance.Initialize(null, this.instanceMap, null, this.lastInstanceId, this);

            if (TD.ActivityScheduledIsEnabled())
            {
                TraceActivityScheduled(null, activity, this.rootInstance.Id);
            }

            // Add the work item for executing the root
            this.scheduler.PushWork(new ExecuteRootWorkItem(this.rootInstance, requiresSymbolResolution, argumentValueOverrides));

            TraceSuspend(hasTracedStart, oldActivityId);
        }

        public void RegisterMainRootCompleteCallback(Bookmark bookmark)
        {
            this.mainRootCompleteBookmark = bookmark;
        }

        public ActivityInstance ScheduleSecondaryRootActivity(Activity activity, LocationEnvironment environment)
        {
            ActivityInstance secondaryRoot = ScheduleActivity(activity, null, null, null, environment);

            while (environment != null)
            {
                environment.AddReference();
                environment = environment.Parent;
            }

            if (this.executingSecondaryRootInstances == null)
            {
                this.executingSecondaryRootInstances = new List<ActivityInstance>();
            }

            this.executingSecondaryRootInstances.Add(secondaryRoot);

            return secondaryRoot;
        }

        public ActivityInstance ScheduleActivity(Activity activity, ActivityInstance parent,
            CompletionBookmark completionBookmark, FaultBookmark faultBookmark, LocationEnvironment parentEnvironment)
        {
            return ScheduleActivity(activity, parent, completionBookmark, faultBookmark, parentEnvironment, null, null);
        }

        public ActivityInstance ScheduleDelegate(ActivityDelegate activityDelegate, IDictionary<string, object> inputParameters, ActivityInstance parent, LocationEnvironment executionEnvironment,
            CompletionBookmark completionBookmark, FaultBookmark faultBookmark)
        {
            Fx.Assert(activityDelegate.Owner != null, "activityDelegate must have an owner");
            Fx.Assert(parent != null, "activityDelegate should have a parent activity instance");

            ActivityInstance handlerInstance;

            if (activityDelegate.Handler == null)
            {
                handlerInstance = ActivityInstance.CreateCompletedInstance(new EmptyDelegateActivity());
                handlerInstance.CompletionBookmark = completionBookmark;
                ScheduleCompletionBookmark(handlerInstance);
            }
            else
            {
                handlerInstance = CreateUninitalizedActivityInstance(activityDelegate.Handler, parent, completionBookmark, faultBookmark);
                bool requiresSymbolResolution = handlerInstance.Initialize(parent, this.instanceMap, executionEnvironment, this.lastInstanceId, this, activityDelegate.RuntimeDelegateArguments.Count);

                IList<RuntimeDelegateArgument> activityDelegateParameters = activityDelegate.RuntimeDelegateArguments;
                for (int i = 0; i < activityDelegateParameters.Count; i++)
                {
                    RuntimeDelegateArgument runtimeArgument = activityDelegateParameters[i];

                    if (runtimeArgument.BoundArgument != null)
                    {
                        string delegateParameterName = runtimeArgument.Name;

                        // Populate argument location. Set it's value in the activity handler's 
                        // instance environment only if it is a DelegateInArgument.
                        Location newLocation = runtimeArgument.BoundArgument.CreateLocation();
                        handlerInstance.Environment.Declare(runtimeArgument.BoundArgument, newLocation, handlerInstance);

                        if (ArgumentDirectionHelper.IsIn(runtimeArgument.Direction))
                        {
                            if (inputParameters != null && inputParameters.Count > 0)
                            {
                                newLocation.Value = inputParameters[delegateParameterName];
                            }
                        }
                    }
                }

                if (TD.ActivityScheduledIsEnabled())
                {
                    TraceActivityScheduled(parent, activityDelegate.Handler, handlerInstance.Id);
                }

                if (this.ShouldTrackActivityScheduledRecords)
                {
                    AddTrackingRecord(new ActivityScheduledRecord(this.WorkflowInstanceId, parent, handlerInstance));
                }

                ScheduleBody(handlerInstance, requiresSymbolResolution, null, null);
            }

            return handlerInstance;
        }

        void TraceActivityScheduled(ActivityInstance parent, Activity activity, string scheduledInstanceId)
        {
            Fx.Assert(TD.ActivityScheduledIsEnabled(), "This should be checked before calling this helper.");

            if (parent != null)
            {
                TD.ActivityScheduled(parent.Activity.GetType().ToString(), parent.Activity.DisplayName, parent.Id, activity.GetType().ToString(), activity.DisplayName, scheduledInstanceId);
            }
            else
            {
                TD.ActivityScheduled(string.Empty, string.Empty, string.Empty, activity.GetType().ToString(), activity.DisplayName, scheduledInstanceId);
            }
        }

        ActivityInstance CreateUninitalizedActivityInstance(Activity activity, ActivityInstance parent, CompletionBookmark completionBookmark, FaultBookmark faultBookmark)
        {
            Fx.Assert(activity.IsMetadataCached, "Metadata must be cached for us to process this activity.");

            // 1. Create a new activity instance and setup bookmark callbacks
            ActivityInstance activityInstance = new ActivityInstance(activity);

            if (parent != null)
            {
                // add a bookmarks to complete at activity.Close/Fault time
                activityInstance.CompletionBookmark = completionBookmark;
                activityInstance.FaultBookmark = faultBookmark;
                parent.AddChild(activityInstance);
            }

            // 2. Setup parent and environment machinery, and add to instance's program mapping for persistence (if necessary)
            IncrementLastInstanceId();

            return activityInstance;
        }

        void IncrementLastInstanceId()
        {
            if (this.lastInstanceId == long.MaxValue)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(SR.OutOfInstanceIds));
            }
            this.lastInstanceId++;
        }

        ActivityInstance ScheduleActivity(Activity activity, ActivityInstance parent,
            CompletionBookmark completionBookmark, FaultBookmark faultBookmark, LocationEnvironment parentEnvironment,
            IDictionary<string, object> argumentValueOverrides, Location resultLocation)
        {
            ActivityInstance activityInstance = CreateUninitalizedActivityInstance(activity, parent, completionBookmark, faultBookmark);
            bool requiresSymbolResolution = activityInstance.Initialize(parent, this.instanceMap, parentEnvironment, this.lastInstanceId, this);

            if (TD.ActivityScheduledIsEnabled())
            {
                TraceActivityScheduled(parent, activity, activityInstance.Id);
            }

            if (this.ShouldTrackActivityScheduledRecords)
            {
                AddTrackingRecord(new ActivityScheduledRecord(this.WorkflowInstanceId, parent, activityInstance));
            }

            ScheduleBody(activityInstance, requiresSymbolResolution, argumentValueOverrides, resultLocation);

            return activityInstance;
        }

        internal void ScheduleExpression(ActivityWithResult activity, ActivityInstance parent, LocationEnvironment parentEnvironment, Location resultLocation, ResolveNextArgumentWorkItem nextArgumentWorkItem)
        {
            Fx.Assert(resultLocation != null, "We should always schedule expressions with a result location.");

            if (!activity.IsMetadataCached || activity.CacheId != parent.Activity.CacheId)
            {
                throw FxTrace.Exception.Argument("activity", SR.ActivityNotPartOfThisTree(activity.DisplayName, parent.Activity.DisplayName));
            }

            if (activity.SkipArgumentResolution)
            {
                // 

                Fx.Assert(!activity.UseOldFastPath || parent.SubState == ActivityInstance.Substate.Executing,
                    "OldFastPath activities should have been handled by the Populate methods, unless this is a dynamic update");
                
                IncrementLastInstanceId();

                ScheduleExpression(activity, parent, resultLocation, nextArgumentWorkItem, this.lastInstanceId);
            }
            else
            {
                if (nextArgumentWorkItem != null)
                {
                    ScheduleItem(nextArgumentWorkItem);
                }
                ScheduleActivity(activity, parent, null, null, parentEnvironment, null, resultLocation.CreateReference(true));
            }
        }

        void ScheduleExpression(ActivityWithResult activity, ActivityInstance parent, Location resultLocation, ResolveNextArgumentWorkItem nextArgumentWorkItem, long instanceId)
        {
            if (TD.ActivityScheduledIsEnabled())
            {
                TraceActivityScheduled(parent, activity, instanceId.ToString(CultureInfo.InvariantCulture));
            }

            if (this.ShouldTrackActivityScheduledRecords)
            {
                AddTrackingRecord(new ActivityScheduledRecord(this.WorkflowInstanceId, parent, new ActivityInfo(activity, instanceId)));
            }

            ExecuteSynchronousExpressionWorkItem workItem = this.ExecuteSynchronousExpressionWorkItemPool.Acquire();
            workItem.Initialize(parent, activity, this.lastInstanceId, resultLocation, nextArgumentWorkItem);
            if (this.instanceMap != null)
            {
                this.instanceMap.AddEntry(workItem);
            }
            ScheduleItem(workItem);
        }

        internal void ScheduleExpressionFaultPropagation(Activity activity, long instanceId, ActivityInstance parent, Exception exception)
        {
            ActivityInstance instance = new ActivityInstance(activity);
            instance.Initialize(parent, this.instanceMap, parent.Environment, instanceId, this);

            if (!parent.HasPendingWork)
            {
                // Force the parent to stay alive, and to attempt to execute its body if the fault is handled
                ScheduleItem(CreateEmptyWorkItem(parent));
            }
            PropagateExceptionWorkItem workItem = new PropagateExceptionWorkItem(exception, instance);
            ScheduleItem(workItem);

            parent.SetInitializationIncomplete();
        }

        // Argument and variables resolution for root activity is defered to execution time
        // invocation of this method means that we're ready to schedule Activity.Execute()
        internal void ScheduleBody(ActivityInstance activityInstance, bool requiresSymbolResolution,
            IDictionary<string, object> argumentValueOverrides, Location resultLocation)
        {
            if (resultLocation == null)
            {
                ExecuteActivityWorkItem workItem = this.ExecuteActivityWorkItemPool.Acquire();
                workItem.Initialize(activityInstance, requiresSymbolResolution, argumentValueOverrides);

                this.scheduler.PushWork(workItem);
            }
            else
            {
                this.scheduler.PushWork(new ExecuteExpressionWorkItem(activityInstance, requiresSymbolResolution, argumentValueOverrides, resultLocation));
            }
        }

        public NoPersistProperty CreateNoPersistProperty()
        {
            return new NoPersistProperty(this);
        }

        public AsyncOperationContext SetupAsyncOperationBlock(ActivityInstance owningActivity)
        {
            if (this.activeOperations != null && this.activeOperations.ContainsKey(owningActivity))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.OnlyOneOperationPerActivity));
            }

            this.EnterNoPersist();

            AsyncOperationContext context = new AsyncOperationContext(this, owningActivity);

            if (this.activeOperations == null)
            {
                this.activeOperations = new Dictionary<ActivityInstance, AsyncOperationContext>();
            }

            this.activeOperations.Add(owningActivity, context);

            return context;
        }

        // Must always be called from a workflow thread
        public void CompleteOperation(ActivityInstance owningInstance, BookmarkCallback callback, object state)
        {
            Fx.Assert(callback != null, "Use the other overload if callback is null.");

            CompleteAsyncOperationWorkItem workItem = new CompleteAsyncOperationWorkItem(
                new BookmarkCallbackWrapper(callback, owningInstance),
                this.bookmarkManager.GenerateTempBookmark(),
                state);
            CompleteOperation(workItem);
        }

        // Must always be called from a workflow thread
        public void CompleteOperation(WorkItem asyncCompletionWorkItem)
        {
            this.scheduler.EnqueueWork(asyncCompletionWorkItem);
            CompleteOperation(asyncCompletionWorkItem.ActivityInstance, false);
        }

        // Must always be called from a workflow thread
        public void CompleteOperation(ActivityInstance owningInstance)
        {
            CompleteOperation(owningInstance, true);
        }

        void CompleteOperation(ActivityInstance owningInstance, bool exitNoPersist)
        {
            Fx.Assert(owningInstance != null, "Cannot be called with a null instance.");
            Fx.Assert(this.activeOperations.ContainsKey(owningInstance), "The owning instance must be in the list if we've gotten here.");

            this.activeOperations.Remove(owningInstance);

            owningInstance.DecrementBusyCount();

            if (exitNoPersist)
            {
                this.ExitNoPersist();
            }
        }

        internal void AddHandle(Handle handleToAdd)
        {
            if (this.handles == null)
            {
                this.handles = new List<Handle>();
            }
            this.handles.Add(handleToAdd);
        }

        [DataContract]
        internal class PersistenceWaiter
        {
            Bookmark onPersistBookmark;
            ActivityInstance waitingInstance;

            public PersistenceWaiter(Bookmark onPersist, ActivityInstance waitingInstance)
            {
                this.OnPersistBookmark = onPersist;
                this.WaitingInstance = waitingInstance;
            }
            
            public Bookmark OnPersistBookmark
            {
                get
                {
                    return this.onPersistBookmark;
                }
                private set
                {
                    this.onPersistBookmark = value;
                }
            }
            
            public ActivityInstance WaitingInstance
            {
                get
                {
                    return this.waitingInstance;
                }
                private set
                {
                    this.waitingInstance = value;
                }
            }

            [DataMember(Name = "OnPersistBookmark")]
            internal Bookmark SerializedOnPersistBookmark
            {
                get { return this.OnPersistBookmark; }
                set { this.OnPersistBookmark = value; }
            }

            [DataMember(Name = "WaitingInstance")]
            internal ActivityInstance SerializedWaitingInstance
            {
                get { return this.WaitingInstance; }
                set { this.WaitingInstance = value; }
            }

            public WorkItem CreateWorkItem()
            {
                return new PersistWorkItem(this);
            }

            [DataContract]
            internal class PersistWorkItem : WorkItem
            {
                PersistenceWaiter waiter;

                public PersistWorkItem(PersistenceWaiter waiter)
                    : base(waiter.WaitingInstance)
                {
                    this.waiter = waiter;
                }

                public override bool IsValid
                {
                    get
                    {
                        return true;
                    }
                }

                public override ActivityInstance PropertyManagerOwner
                {
                    get
                    {
                        // Persist should not pick up user transaction / identity.
                        return null;
                    }
                }

                [DataMember(Name = "waiter")]
                internal PersistenceWaiter SerializedWaiter
                {
                    get { return this.waiter; }
                    set { this.waiter = value; }
                }

                public override void TraceCompleted()
                {
                    TraceRuntimeWorkItemCompleted();
                }

                public override void TraceScheduled()
                {
                    TraceRuntimeWorkItemScheduled();
                }

                public override void TraceStarting()
                {
                    TraceRuntimeWorkItemStarting();
                }

                public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
                {
                    if (executor.TryResumeUserBookmark(this.waiter.OnPersistBookmark, null, false) != BookmarkResumptionResult.Success)
                    {
                        Fx.Assert("This should always be resumable.");
                    }

                    IAsyncResult result = null;

                    try
                    {
                        result = executor.host.OnBeginPersist(Fx.ThunkCallback(new AsyncCallback(OnPersistComplete)), executor);

                        if (result.CompletedSynchronously)
                        {
                            executor.host.OnEndPersist(result);
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        this.workflowAbortException = e;
                    }

                    return result == null || result.CompletedSynchronously;
                }

                void OnPersistComplete(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    ActivityExecutor executor = (ActivityExecutor)result.AsyncState;

                    try
                    {
                        executor.host.OnEndPersist(result);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        this.workflowAbortException = e;
                    }

                    executor.FinishWorkItem(this);
                }

                public override void PostProcess(ActivityExecutor executor)
                {
                    if (this.ExceptionToPropagate != null)
                    {
                        executor.AbortActivityInstance(this.waiter.WaitingInstance, this.ExceptionToPropagate);
                    }
                }
            }
        }

        [DataContract]
        internal class AbortActivityWorkItem : WorkItem
        {
            Exception reason;
            ActivityInstanceReference originalSource;

            ActivityExecutor executor;

            public AbortActivityWorkItem(ActivityExecutor executor, ActivityInstance activityInstance, Exception reason, ActivityInstanceReference originalSource)
                : base(activityInstance)
            {
                this.reason = reason;
                this.originalSource = originalSource;

                this.IsEmpty = true;
                this.executor = executor;
            }

            public override ActivityInstance OriginalExceptionSource
            {
                get
                {
                    return this.originalSource.ActivityInstance;
                }
            }

            public override bool IsValid
            {
                get
                {
                    return this.ActivityInstance.State == ActivityInstanceState.Executing;
                }
            }

            public override ActivityInstance PropertyManagerOwner
            {
                get
                {
                    Fx.Assert("This is never called.");

                    return null;
                }
            }

            [DataMember(Name = "reason")]
            internal Exception SerializedReason
            {
                get { return this.reason; }
                set { this.reason = value; }
            }

            [DataMember(Name = "originalSource")]
            internal ActivityInstanceReference SerializedOriginalSource
            {
                get { return this.originalSource; }
                set { this.originalSource = value; }
            }

            public override void TraceCompleted()
            {
                TraceRuntimeWorkItemCompleted();
            }

            public override void TraceScheduled()
            {
                TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                TraceRuntimeWorkItemStarting();
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                Fx.Assert("This is never called");

                return true;
            }

            public override void PostProcess(ActivityExecutor executor)
            {
                executor.AbortActivityInstance(this.ActivityInstance, this.reason);

                // We always repropagate the exception from here
                this.ExceptionToPropagate = this.reason;

                // Tell the executor to decrement its NoPersistCount, if necessary.
                executor.ExitNoPersistForExceptionPropagation();
            }
        }

        [DataContract]
        internal class CompleteAsyncOperationWorkItem : BookmarkWorkItem
        {
            public CompleteAsyncOperationWorkItem(BookmarkCallbackWrapper wrapper, Bookmark bookmark, object value)
                : base(wrapper, bookmark, value)
            {
                this.ExitNoPersistRequired = true;
            }
        }

        [DataContract]
        internal class CancelActivityWorkItem : ActivityExecutionWorkItem
        {
            public CancelActivityWorkItem(ActivityInstance activityInstance)
                : base(activityInstance)
            {
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteCancelActivityWorkItemIsEnabled())
                {
                    TD.CompleteCancelActivityWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleCancelActivityWorkItemIsEnabled())
                {
                    TD.ScheduleCancelActivityWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartCancelActivityWorkItemIsEnabled())
                {
                    TD.StartCancelActivityWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                try
                {
                    this.ActivityInstance.Cancel(executor, bookmarkManager);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.ExceptionToPropagate = e;
                }

                return true;
            }
        }

        [DataContract]
        internal class ExecuteActivityWorkItem : ActivityExecutionWorkItem
        {
            bool requiresSymbolResolution;
            IDictionary<string, object> argumentValueOverrides;

            // Called by the pool.
            public ExecuteActivityWorkItem()
            {
                this.IsPooled = true;
            }

            // Called by non-pool subclasses.
            protected ExecuteActivityWorkItem(ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides)
                : base(activityInstance)
            {
                this.requiresSymbolResolution = requiresSymbolResolution;
                this.argumentValueOverrides = argumentValueOverrides;
            }

            [DataMember(EmitDefaultValue = false, Name = "requiresSymbolResolution")]
            internal bool SerializedRequiresSymbolResolution
            {
                get { return this.requiresSymbolResolution; }
                set { this.requiresSymbolResolution = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "argumentValueOverrides")]
            internal IDictionary<string, object> SerializedArgumentValueOverrides
            {
                get { return this.argumentValueOverrides; }
                set { this.argumentValueOverrides = value; }
            }            

            public void Initialize(ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides)
            {
                base.Reinitialize(activityInstance);
                this.requiresSymbolResolution = requiresSymbolResolution;
                this.argumentValueOverrides = argumentValueOverrides;
            }

            protected override void ReleaseToPool(ActivityExecutor executor)
            {
                base.ClearForReuse();
                this.requiresSymbolResolution = false;
                this.argumentValueOverrides = null;

                executor.ExecuteActivityWorkItemPool.Release(this);
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleExecuteActivityWorkItemIsEnabled())
                {
                    TD.ScheduleExecuteActivityWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartExecuteActivityWorkItemIsEnabled())
                {
                    TD.StartExecuteActivityWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteExecuteActivityWorkItemIsEnabled())
                {
                    TD.CompleteExecuteActivityWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                return ExecuteBody(executor, bookmarkManager, null);
            }

            protected bool ExecuteBody(ActivityExecutor executor, BookmarkManager bookmarkManager, Location resultLocation)
            {
                try
                {
                    if (this.requiresSymbolResolution)
                    {
                        if (!this.ActivityInstance.ResolveArguments(executor, this.argumentValueOverrides, resultLocation))
                        {
                            return true;
                        }

                        if (!this.ActivityInstance.ResolveVariables(executor))
                        {
                            return true;
                        }
                    }
                    // We want to do this if there was no symbol resolution or if ResolveVariables completed
                    // synchronously.
                    this.ActivityInstance.SetInitializedSubstate(executor);

                    if (executor.IsDebugged())
                    {
                        executor.debugController.ActivityStarted(this.ActivityInstance);
                    }

                    this.ActivityInstance.Execute(executor, bookmarkManager);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.ExceptionToPropagate = e;
                }

                return true;
            }
        }

        [DataContract]
        internal class ExecuteRootWorkItem : ExecuteActivityWorkItem
        {
            public ExecuteRootWorkItem(ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides)
                : base(activityInstance, requiresSymbolResolution, argumentValueOverrides)
            {
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                if (executor.ShouldTrackActivityScheduledRecords)
                {
                    executor.AddTrackingRecord(
                        new ActivityScheduledRecord(
                            executor.WorkflowInstanceId,
                            null,
                            this.ActivityInstance));
                }

                return ExecuteBody(executor, bookmarkManager, null);
            }
        }

        [DataContract]
        internal class ExecuteExpressionWorkItem : ExecuteActivityWorkItem
        {
            Location resultLocation;

            public ExecuteExpressionWorkItem(ActivityInstance activityInstance, bool requiresSymbolResolution, IDictionary<string, object> argumentValueOverrides, Location resultLocation)
                : base(activityInstance, requiresSymbolResolution, argumentValueOverrides)
            {
                Fx.Assert(resultLocation != null, "We should only use this work item when we are resolving arguments/variables and therefore have a result location.");
                this.resultLocation = resultLocation;
            }

            [DataMember(Name = "resultLocation")]
            internal Location SerializedResultLocation
            {
                get { return this.resultLocation; }
                set { this.resultLocation = value; }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                return ExecuteBody(executor, bookmarkManager, resultLocation);
            }
        }

        [DataContract]
        internal class PropagateExceptionWorkItem : ActivityExecutionWorkItem
        {
            Exception exception;

            public PropagateExceptionWorkItem(Exception exception, ActivityInstance activityInstance)
                : base(activityInstance)
            {
                Fx.Assert(exception != null, "We must not have a null exception.");

                this.exception = exception;
                this.IsEmpty = true;
            }

            [DataMember(EmitDefaultValue = false, Name = "exception")]
            internal Exception SerializedException
            {
                get { return this.exception; }
                set { this.exception = value; }
            }

            public override void TraceScheduled()
            {
                TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                TraceRuntimeWorkItemStarting();
            }

            public override void TraceCompleted()
            {
                TraceRuntimeWorkItemCompleted();
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                Fx.Assert("This shouldn't be called because we are empty.");

                return false;
            }

            public override void PostProcess(ActivityExecutor executor)
            {
                ExceptionToPropagate = this.exception;
            }
        }

        [DataContract]
        internal class RethrowExceptionWorkItem : WorkItem
        {
            Exception exception;
            ActivityInstanceReference source;

            public RethrowExceptionWorkItem(ActivityInstance activityInstance, Exception exception, ActivityInstanceReference source)
                : base(activityInstance)
            {
                this.exception = exception;
                this.source = source;
                this.IsEmpty = true;
            }

            public override bool IsValid
            {
                get
                {
                    return this.ActivityInstance.State == ActivityInstanceState.Executing;
                }
            }

            public override ActivityInstance PropertyManagerOwner
            {
                get
                {
                    Fx.Assert("This is never called.");

                    return null;
                }
            }

            public override ActivityInstance OriginalExceptionSource
            {
                get
                {
                    return this.source.ActivityInstance;
                }
            }

            [DataMember(Name = "exception")]
            internal Exception SerializedException
            {
                get { return this.exception; }
                set { this.exception = value; }
            }

            [DataMember(Name = "source")]
            internal ActivityInstanceReference SerializedSource
            {
                get { return this.source; }
                set { this.source = value; }
            }

            public override void TraceCompleted()
            {
                TraceRuntimeWorkItemCompleted();
            }

            public override void TraceScheduled()
            {
                TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                TraceRuntimeWorkItemStarting();
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                Fx.Assert("This shouldn't be called because we are IsEmpty = true.");

                return true;
            }

            public override void PostProcess(ActivityExecutor executor)
            {
                executor.AbortActivityInstance(this.ActivityInstance, this.ExceptionToPropagate);
                this.ExceptionToPropagate = this.exception;
            }
        }

        [DataContract]
        internal class TransactionContextWaiter
        {
            public TransactionContextWaiter(ActivityInstance instance, bool isRequires, RuntimeTransactionHandle handle, TransactionContextWaiterCallbackWrapper callbackWrapper, object state)
            {
                Fx.Assert(instance != null, "Must have an instance.");
                Fx.Assert(handle != null, "Must have a handle.");
                Fx.Assert(callbackWrapper != null, "Must have a callbackWrapper");

                this.WaitingInstance = instance;
                this.IsRequires = isRequires;
                this.Handle = handle;
                this.State = state;
                this.CallbackWrapper = callbackWrapper;
            }

            ActivityInstance waitingInstance;
            public ActivityInstance WaitingInstance
            {
                get
                {
                    return this.waitingInstance;
                }
                private set
                {
                    this.waitingInstance = value;
                }
            }

            bool isRequires;
            public bool IsRequires
            {
                get
                {
                    return this.isRequires;
                }
                private set
                {
                    this.isRequires = value;
                }
            }

            RuntimeTransactionHandle handle;
            public RuntimeTransactionHandle Handle
            {
                get
                {
                    return this.handle;
                }
                private set
                {
                    this.handle = value;
                }
            }

            object state;
            public object State
            {
                get
                {
                    return this.state;
                }
                private set
                {
                    this.state = value;
                }
            }

            TransactionContextWaiterCallbackWrapper callbackWrapper;
            public TransactionContextWaiterCallbackWrapper CallbackWrapper
            {
                get
                {
                    return this.callbackWrapper;
                }
                private set
                {
                    this.callbackWrapper = value;
                }
            }

            [DataMember(Name = "WaitingInstance")]
            internal ActivityInstance SerializedWaitingInstance
            {
                get { return this.WaitingInstance; }
                set { this.WaitingInstance = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "IsRequires")]
            internal bool SerializedIsRequires
            {
                get { return this.IsRequires; }
                set { this.IsRequires = value; }
            }

            [DataMember(Name = "Handle")]
            internal RuntimeTransactionHandle SerializedHandle
            {
                get { return this.Handle; }
                set { this.Handle = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "State")]
            internal object SerializedState
            {
                get { return this.State; }
                set { this.State = value; }
            }

            [DataMember(Name = "CallbackWrapper")]
            internal TransactionContextWaiterCallbackWrapper SerializedCallbackWrapper
            {
                get { return this.CallbackWrapper; }
                set { this.CallbackWrapper = value; }
            }
        }

        [DataContract]
        internal class TransactionContextWaiterCallbackWrapper : CallbackWrapper
        {
            static readonly Type callbackType = typeof(Action<NativeActivityTransactionContext, object>);
            static readonly Type[] transactionCallbackParameterTypes = new Type[] { typeof(NativeActivityTransactionContext), typeof(object) };

            public TransactionContextWaiterCallbackWrapper(Action<NativeActivityTransactionContext, object> action, ActivityInstance owningInstance)
                : base(action, owningInstance)
            {
            }

            [Fx.Tag.SecurityNote(Critical = "Because we are calling EnsureCallback",
                Safe = "Safe because the method needs to be part of an Activity and we are casting to the callback type and it has a very specific signature. The author of the callback is buying into being invoked from PT.")]
            [SecuritySafeCritical]
            public void Invoke(NativeActivityTransactionContext context, object value)
            {
                EnsureCallback(callbackType, transactionCallbackParameterTypes);
                Action<NativeActivityTransactionContext, object> callback = (Action<NativeActivityTransactionContext, object>)this.Callback;
                callback(context, value);
            }
        }

        // This is not DataContract because this is always scheduled in a no-persist zone.
        // This work items exits the no persist zone when it is released.
        class CompleteTransactionWorkItem : WorkItem
        {
            static AsyncCallback persistCompleteCallback;
            static AsyncCallback commitCompleteCallback;
            static Action<object, TimeoutException> outcomeDeterminedCallback;

            RuntimeTransactionData runtimeTransaction;
            ActivityExecutor executor;

            public CompleteTransactionWorkItem(ActivityInstance instance)
                : base(instance)
            {
                this.ExitNoPersistRequired = true;
            }

            static AsyncCallback PersistCompleteCallback
            {
                get
                {
                    if (persistCompleteCallback == null)
                    {
                        persistCompleteCallback = Fx.ThunkCallback(new AsyncCallback(OnPersistComplete));
                    }

                    return persistCompleteCallback;
                }
            }

            static AsyncCallback CommitCompleteCallback
            {
                get
                {
                    if (commitCompleteCallback == null)
                    {
                        commitCompleteCallback = Fx.ThunkCallback(new AsyncCallback(OnCommitComplete));
                    }

                    return commitCompleteCallback;
                }
            }

            static Action<object, TimeoutException> OutcomeDeterminedCallback
            {
                get
                {
                    if (outcomeDeterminedCallback == null)
                    {
                        outcomeDeterminedCallback = new Action<object, TimeoutException>(OnOutcomeDetermined);
                    }

                    return outcomeDeterminedCallback;
                }
            }

            public override bool IsValid
            {
                get
                {
                    return true;
                }
            }

            public override ActivityInstance PropertyManagerOwner
            {
                get
                {
                    return null;
                }
            }

            public override void TraceCompleted()
            {
                TraceRuntimeWorkItemCompleted();
            }

            public override void TraceScheduled()
            {
                TraceRuntimeWorkItemScheduled();
            }

            public override void TraceStarting()
            {
                TraceRuntimeWorkItemStarting();
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                this.runtimeTransaction = executor.runtimeTransaction;
                this.executor = executor;

                // We need to take care of any pending cancelation
                this.executor.SchedulePendingCancelation();

                bool completeSelf;
                try
                {
                    // If the transaction is already rolled back, skip the persistence.  This allows us to avoid aborting the instance.
                    completeSelf = CheckTransactionAborted();
                    if (!completeSelf)
                    {
                        IAsyncResult result = new TransactionalPersistAsyncResult(this.executor, PersistCompleteCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            completeSelf = FinishPersist(result);
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    HandleException(e);
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    this.executor.runtimeTransaction = null;

                    TraceTransactionOutcome();
                    return true;
                }

                return false;
            }

            void TraceTransactionOutcome()
            {
                if (TD.RuntimeTransactionCompleteIsEnabled())
                {
                    TD.RuntimeTransactionComplete(this.runtimeTransaction.TransactionStatus.ToString());
                }
            }

            void HandleException(Exception exception)
            {
                try
                {
                    this.runtimeTransaction.OriginalTransaction.Rollback(exception);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.workflowAbortException = e;
                }

                if (this.runtimeTransaction.TransactionHandle.AbortInstanceOnTransactionFailure)
                {
                    // We might be overwriting a more recent exception from above, but it is
                    // more important that we tell the user why they failed originally.
                    this.workflowAbortException = exception;
                }
                else
                {
                    this.ExceptionToPropagate = exception;
                }
            }

            static void OnPersistComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CompleteTransactionWorkItem thisPtr = (CompleteTransactionWorkItem)result.AsyncState;
                bool completeSelf = true;

                try
                {
                    completeSelf = thisPtr.FinishPersist(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    thisPtr.HandleException(e);
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.executor.runtimeTransaction = null;

                    thisPtr.TraceTransactionOutcome();

                    thisPtr.executor.FinishWorkItem(thisPtr);
                }
            }

            bool FinishPersist(IAsyncResult result)
            {
                TransactionalPersistAsyncResult.End(result);

                return CompleteTransaction();
            }

            bool CompleteTransaction()
            {
                PreparingEnlistment enlistment = null;

                lock (this.runtimeTransaction)
                {
                    if (this.runtimeTransaction.PendingPreparingEnlistment != null)
                    {
                        enlistment = this.runtimeTransaction.PendingPreparingEnlistment;
                    }

                    this.runtimeTransaction.HasPrepared = true;
                }

                if (enlistment != null)
                {
                    enlistment.Prepared();
                }

                Transaction original = this.runtimeTransaction.OriginalTransaction;

                DependentTransaction dependentTransaction = original as DependentTransaction;
                if (dependentTransaction != null)
                {
                    dependentTransaction.Complete();
                    return CheckOutcome();
                }
                else
                {
                    CommittableTransaction committableTransaction = original as CommittableTransaction;
                    if (committableTransaction != null)
                    {
                        IAsyncResult result = committableTransaction.BeginCommit(CommitCompleteCallback, this);

                        if (result.CompletedSynchronously)
                        {
                            return FinishCommit(result);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return CheckOutcome();
                    }
                }
            }

            static void OnCommitComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                CompleteTransactionWorkItem thisPtr = (CompleteTransactionWorkItem)result.AsyncState;
                bool completeSelf = true;

                try
                {
                    completeSelf = thisPtr.FinishCommit(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    thisPtr.HandleException(e);
                    completeSelf = true;
                }

                if (completeSelf)
                {
                    thisPtr.executor.runtimeTransaction = null;

                    thisPtr.TraceTransactionOutcome();

                    thisPtr.executor.FinishWorkItem(thisPtr);
                }
            }

            bool FinishCommit(IAsyncResult result)
            {
                ((CommittableTransaction)this.runtimeTransaction.OriginalTransaction).EndCommit(result);

                return CheckOutcome();
            }

            bool CheckOutcome()
            {
                AsyncWaitHandle completionEvent = null;

                lock (this.runtimeTransaction)
                {
                    TransactionStatus status = this.runtimeTransaction.TransactionStatus;

                    if (status == TransactionStatus.Active)
                    {
                        completionEvent = new AsyncWaitHandle();
                        this.runtimeTransaction.CompletionEvent = completionEvent;
                    }
                }

                if (completionEvent != null)
                {
                    if (!completionEvent.WaitAsync(OutcomeDeterminedCallback, this, ActivityDefaults.TransactionCompletionTimeout))
                    {
                        return false;
                    }
                }

                return FinishCheckOutcome();
            }

            static void OnOutcomeDetermined(object state, TimeoutException asyncException)
            {
                CompleteTransactionWorkItem thisPtr = (CompleteTransactionWorkItem)state;
                bool completeSelf = true;

                if (asyncException != null)
                {
                    thisPtr.HandleException(asyncException);
                }
                else
                {
                    try
                    {
                        completeSelf = thisPtr.FinishCheckOutcome();
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        thisPtr.HandleException(e);
                        completeSelf = true;
                    }
                }

                if (completeSelf)
                {
                    thisPtr.executor.runtimeTransaction = null;

                    thisPtr.TraceTransactionOutcome();

                    thisPtr.executor.FinishWorkItem(thisPtr);
                }
            }

            bool FinishCheckOutcome()
            {
                CheckTransactionAborted();
                return true;
            }

            bool CheckTransactionAborted()
            {
                try
                {
                    TransactionHelper.ThrowIfTransactionAbortedOrInDoubt(this.runtimeTransaction.OriginalTransaction);
                    return false;
                }
                catch (TransactionException exception)
                {
                    if (this.runtimeTransaction.TransactionHandle.AbortInstanceOnTransactionFailure)
                    {
                        this.workflowAbortException = exception;
                    }
                    else
                    {
                        this.ExceptionToPropagate = exception;
                    }
                    return true;
                }
            }

            public override void PostProcess(ActivityExecutor executor)
            {
            }

            class TransactionalPersistAsyncResult : TransactedAsyncResult
            {
                CompleteTransactionWorkItem workItem;
                static readonly AsyncCompletion onPersistComplete = new AsyncCompletion(OnPersistComplete);
                readonly ActivityExecutor executor;

                public TransactionalPersistAsyncResult(ActivityExecutor executor, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.executor = executor;
                    this.workItem = (CompleteTransactionWorkItem)state;
                    IAsyncResult result = null;
                    using (PrepareTransactionalCall(this.executor.CurrentTransaction))
                    {
                        try
                        {
                            result = this.executor.host.OnBeginPersist(PrepareAsyncCompletion(TransactionalPersistAsyncResult.onPersistComplete), this);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }
                            this.workItem.workflowAbortException = e;
                            throw;
                        }
                    }
                    if (SyncContinue(result))
                    {
                        Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<TransactionalPersistAsyncResult>(result);
                }

                static bool OnPersistComplete(IAsyncResult result)
                {
                    TransactionalPersistAsyncResult thisPtr = (TransactionalPersistAsyncResult)result.AsyncState;

                    try
                    {
                        thisPtr.executor.host.OnEndPersist(result);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        thisPtr.workItem.workflowAbortException = e;
                        throw;
                    }

                    return true;
                }
            }
        }

        [DataContract]
        internal class TransactionContextWorkItem : ActivityExecutionWorkItem
        {
            TransactionContextWaiter waiter;

            public TransactionContextWorkItem(TransactionContextWaiter waiter)
                : base(waiter.WaitingInstance)
            {
                this.waiter = waiter;

                if (this.waiter.IsRequires)
                {
                    this.ExitNoPersistRequired = true;
                }
            }

            [DataMember(Name = "waiter")]
            internal TransactionContextWaiter SerializedWaiter
            {
                get { return this.waiter; }
                set { this.waiter = value; }
            }

            public override void TraceCompleted()
            {
                if (TD.CompleteTransactionContextWorkItemIsEnabled())
                {
                    TD.CompleteTransactionContextWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override void TraceScheduled()
            {
                if (TD.ScheduleTransactionContextWorkItemIsEnabled())
                {
                    TD.ScheduleTransactionContextWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override void TraceStarting()
            {
                if (TD.StartTransactionContextWorkItemIsEnabled())
                {
                    TD.StartTransactionContextWorkItem(this.ActivityInstance.Activity.GetType().ToString(), this.ActivityInstance.Activity.DisplayName, this.ActivityInstance.Id);
                }
            }

            public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
            {
                NativeActivityTransactionContext transactionContext = null;

                try
                {
                    transactionContext = new NativeActivityTransactionContext(this.ActivityInstance, executor, bookmarkManager, this.waiter.Handle);
                    waiter.CallbackWrapper.Invoke(transactionContext, this.waiter.State);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.ExceptionToPropagate = e;
                }
                finally
                {
                    if (transactionContext != null)
                    {
                        transactionContext.Dispose();
                    }
                }

                return true;
            }
        }

        // This class is not DataContract since we only create instances of it while we
        // are in no-persist zones
        class RuntimeTransactionData
        {
            public RuntimeTransactionData(RuntimeTransactionHandle handle, Transaction transaction, ActivityInstance isolationScope)
            {
                this.TransactionHandle = handle;
                this.OriginalTransaction = transaction;
                this.ClonedTransaction = transaction.Clone();
                this.IsolationScope = isolationScope;
                this.TransactionStatus = TransactionStatus.Active;
            }

            public AsyncWaitHandle CompletionEvent
            {
                get;
                set;
            }

            public PreparingEnlistment PendingPreparingEnlistment
            {
                get;
                set;
            }

            public bool HasPrepared
            {
                get;
                set;
            }

            public bool ShouldScheduleCompletion
            {
                get;
                set;
            }

            public TransactionStatus TransactionStatus
            {
                get;
                set;
            }

            public bool IsRootCancelPending
            {
                get;
                set;
            }

            public RuntimeTransactionHandle TransactionHandle
            {
                get;
                private set;
            }

            public Transaction ClonedTransaction
            {
                get;
                private set;
            }

            public Transaction OriginalTransaction
            {
                get;
                private set;
            }

            public ActivityInstance IsolationScope
            {
                get;
                private set;
            }

            [Fx.Tag.Throws(typeof(Exception), "Doesn't handle any exceptions coming from Rollback.")]
            public void Rollback(Exception reason)
            {
                Fx.Assert(this.OriginalTransaction != null, "We always have an original transaction.");

                this.OriginalTransaction.Rollback(reason);
            }
        }

        class AssociateKeysAsyncResult : TransactedAsyncResult
        {
            static readonly AsyncCompletion associatedCallback = new AsyncCompletion(OnAssociated);

            readonly ActivityExecutor executor;

            public AssociateKeysAsyncResult(ActivityExecutor executor, ICollection<InstanceKey> keysToAssociate, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.executor = executor;

                IAsyncResult result;
                using (PrepareTransactionalCall(this.executor.CurrentTransaction))
                {
                    result = this.executor.host.OnBeginAssociateKeys(keysToAssociate, PrepareAsyncCompletion(associatedCallback), this);
                }
                if (SyncContinue(result))
                {
                    Complete(true);
                }
            }

            static bool OnAssociated(IAsyncResult result)
            {
                AssociateKeysAsyncResult thisPtr = (AssociateKeysAsyncResult)result.AsyncState;
                thisPtr.executor.host.OnEndAssociateKeys(result);
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<AssociateKeysAsyncResult>(result);
            }
        }

        class PoolOfEmptyWorkItems : Pool<EmptyWorkItem>
        {
            protected override EmptyWorkItem CreateNew()
            {
                return new EmptyWorkItem();
            }
        }

        class PoolOfExecuteActivityWorkItems : Pool<ExecuteActivityWorkItem>
        {
            protected override ExecuteActivityWorkItem CreateNew()
            {
                return new ExecuteActivityWorkItem();
            }
        }

        class PoolOfExecuteSynchronousExpressionWorkItems : Pool<ExecuteSynchronousExpressionWorkItem>
        {
            protected override ExecuteSynchronousExpressionWorkItem CreateNew()
            {
                return new ExecuteSynchronousExpressionWorkItem();
            }
        }

        class PoolOfCompletionWorkItems : Pool<CompletionCallbackWrapper.CompletionWorkItem>
        {
            protected override CompletionCallbackWrapper.CompletionWorkItem CreateNew()
            {
                return new CompletionCallbackWrapper.CompletionWorkItem();
            }
        }

        class PoolOfNativeActivityContexts : Pool<NativeActivityContext>
        {
            protected override NativeActivityContext CreateNew()
            {
                return new NativeActivityContext();
            }
        }

        class PoolOfCodeActivityContexts : Pool<CodeActivityContext>
        {
            protected override CodeActivityContext CreateNew()
            {
                return new CodeActivityContext();
            }
        }

        class PoolOfResolveNextArgumentWorkItems : Pool<ResolveNextArgumentWorkItem>
        {
            protected override ResolveNextArgumentWorkItem CreateNew()
            {
                return new ResolveNextArgumentWorkItem();
            }
        }

        //This is used in ScheduleDelegate when the handler is null. We use this dummy activity to 
        //set as the 'Activity' of the completed ActivityInstance.
        class EmptyDelegateActivity : NativeActivity
        {
            internal EmptyDelegateActivity()
            {
            }

            protected override void Execute(NativeActivityContext context)
            {
                Fx.Assert(false, "This activity should never be executed. It is a dummy activity");
            }
        }
    }
}
