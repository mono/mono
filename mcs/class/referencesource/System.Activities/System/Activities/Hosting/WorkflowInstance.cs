//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Hosting
{
    using System;
    using System.Text;
    using System.Activities.DynamicUpdate;
    using System.Activities.Runtime;
    using System.Activities.Tracking;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Threading;
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    public abstract class WorkflowInstance
    {
        static readonly IDictionary<string, LocationInfo> EmptyMappedVariablesDictionary = new ReadOnlyDictionaryInternal<string, LocationInfo>(new Dictionary<string, LocationInfo>(0));

        const int True = 1;
        const int False = 0;

        WorkflowInstanceControl controller;
        TrackingProvider trackingProvider;
        SynchronizationContext syncContext;
        LocationReferenceEnvironment hostEnvironment;
        ActivityExecutor executor;
        int isPerformingOperation;
        bool isInitialized;
        WorkflowInstanceExtensionCollection extensions;

        // Tracking for one-time actions per in-memory instance
        bool hasTrackedResumed;
        bool hasTrackedCompletion;

        bool isAborted;
        Exception abortedException;

#if DEBUG
        StackTrace abortStack;
#endif

        protected WorkflowInstance(Activity workflowDefinition)
            : this(workflowDefinition, null)
        {
        }

        protected WorkflowInstance(Activity workflowDefinition, WorkflowIdentity definitionIdentity)
        {
            if (workflowDefinition == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowDefinition");
            }

            this.WorkflowDefinition = workflowDefinition;
            this.DefinitionIdentity = definitionIdentity;
        }

        public abstract Guid Id
        {
            get;
        }

        internal bool HasTrackingParticipant
        {
            get;
            private set;
        }

        internal bool HasTrackedStarted
        {
            get;
            private set;
        }

        internal bool HasPersistenceModule
        {
            get;
            private set;
        }

        public SynchronizationContext SynchronizationContext
        {
            get
            {
                return this.syncContext;
            }
            set
            {
                ThrowIfReadOnly();
                this.syncContext = value;
            }
        }

        public LocationReferenceEnvironment HostEnvironment
        {
            get
            {
                return this.hostEnvironment;
            }
            set
            {
                ThrowIfReadOnly();
                this.hostEnvironment = value;
            }
        }

        public Activity WorkflowDefinition
        {
            get;
            private set;
        }

        public WorkflowIdentity DefinitionIdentity
        {
            get;
            private set;
        }

        protected bool IsReadOnly
        {
            get
            {
                return this.isInitialized;
            }
        }

        protected internal abstract bool SupportsInstanceKeys
        {
            get;
        }

        // this is going away
        internal TrackingProvider TrackingProvider
        {
            get
            {
                Fx.Assert(HasTrackingParticipant, "we should only be called if we have a tracking participant");
                return this.trackingProvider;
            }
        }

        protected WorkflowInstanceControl Controller
        {
            get
            {
                if (!this.isInitialized)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ControllerInvalidBeforeInitialize));
                }

                return this.controller;
            }
        }

        // host-facing access to our cascading ExtensionManager resolution
        protected internal T GetExtension<T>() where T : class
        {
            if (this.extensions != null)
            {
                return this.extensions.Find<T>();
            }
            else
            {
                return default(T);
            }
        }

        protected internal IEnumerable<T> GetExtensions<T>() where T : class
        {
            if (this.extensions != null)
            {
                return this.extensions.FindAll<T>();
            }
            else
            {
                return new T[0];
            }
        }

        // locks down the given extensions manager and runs cache metadata on the workflow definition
        protected void RegisterExtensionManager(WorkflowInstanceExtensionManager extensionManager)
        {
            ValidateWorkflow(extensionManager);
            this.extensions = WorkflowInstanceExtensionManager.CreateInstanceExtensions(this.WorkflowDefinition, extensionManager);
            if (this.extensions != null)
            {
                this.HasPersistenceModule = this.extensions.HasPersistenceModule;
            }
        }

        // dispose the extensions that implement IDisposable
        protected void DisposeExtensions()
        {
            if (this.extensions != null)
            {
                this.extensions.Dispose();
                this.extensions = null;
            }
        }

        protected static IList<ActivityBlockingUpdate> GetActivitiesBlockingUpdate(object deserializedRuntimeState, DynamicUpdateMap updateMap)
        {
            ActivityExecutor executor = deserializedRuntimeState as ActivityExecutor;
            if (executor == null)
            {
                throw FxTrace.Exception.Argument("deserializedRuntimeState", SR.InvalidRuntimeState);
            }
            if (updateMap == null)
            {
                throw FxTrace.Exception.ArgumentNull("updateMap");
            }

            DynamicUpdateMap rootMap = updateMap;
            if (updateMap.IsForImplementation)
            {
                rootMap = updateMap.AsRootMap();
            }
            IList<ActivityBlockingUpdate> result = executor.GetActivitiesBlockingUpdate(rootMap);
            if (result == null)
            {
                result = new List<ActivityBlockingUpdate>();
            }

            return result;
        }

        // used for Create scenarios where you are providing root information
        protected void Initialize(IDictionary<string, object> workflowArgumentValues, IList<Handle> workflowExecutionProperties)
        {
            ThrowIfAborted();
            ThrowIfReadOnly();
            this.executor = new ActivityExecutor(this);

            EnsureDefinitionReady();
            // workflowArgumentValues signals whether we are a new or loaded instance, so we can't pass in null.
            // workflowExecutionProperties is allowed to be null
            InitializeCore(workflowArgumentValues ?? ActivityUtilities.EmptyParameters, workflowExecutionProperties);
        }

        // used for Load scenarios where you are rehydrating a WorkflowInstance
        protected void Initialize(object deserializedRuntimeState)
        {
            Initialize(deserializedRuntimeState, null);
        }        

        protected void Initialize(object deserializedRuntimeState, DynamicUpdateMap updateMap)
        {
            ThrowIfAborted();
            ThrowIfReadOnly();
            this.executor = deserializedRuntimeState as ActivityExecutor;

            if (this.executor == null)
            {
                throw FxTrace.Exception.Argument("deserializedRuntimeState", SR.InvalidRuntimeState);
            }
            this.executor.ThrowIfNonSerializable();

            EnsureDefinitionReady();

            WorkflowIdentity originalDefinitionIdentity = this.executor.WorkflowIdentity;      
            bool success = false;
            Collection<ActivityBlockingUpdate> updateErrors = null;
            try
            {
                if (updateMap != null)
                {
                    // check if map is for implementaiton,                    
                    if (updateMap.IsForImplementation)
                    {
                        // if so, the definition root must be an activity 
                        // with no public/imported children and no public/imported delegates.
                        if (DynamicUpdateMap.CanUseImplementationMapAsRoot(this.WorkflowDefinition))
                        {
                            updateMap = updateMap.AsRootMap();
                        }
                        else
                        {
                            throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidImplementationAsWorkflowRoot));
                        }
                    }

                    updateMap.ThrowIfInvalid(this.WorkflowDefinition);

                    this.executor.WorkflowIdentity = this.DefinitionIdentity;

                    this.executor.UpdateInstancePhase1(updateMap, this.WorkflowDefinition, ref updateErrors);
                    ThrowIfDynamicUpdateErrorExists(updateErrors);
                }

                InitializeCore(null, null);

                if (updateMap != null)
                {
                    this.executor.UpdateInstancePhase2(updateMap, ref updateErrors);
                    ThrowIfDynamicUpdateErrorExists(updateErrors);
                    // Track that dynamic update is successful
                    if (this.Controller.TrackingEnabled)
                    {
                        this.Controller.Track(new WorkflowInstanceUpdatedRecord(this.Id, this.WorkflowDefinition.DisplayName, originalDefinitionIdentity, this.executor.WorkflowIdentity));
                    }
                }

                success = true;
            }
            catch (InstanceUpdateException updateException)
            {
                // Can't track through the controller because initialization failed
                if (this.HasTrackingParticipant && this.TrackingProvider.ShouldTrackWorkflowInstanceRecords)
                {
                    IList<ActivityBlockingUpdate> blockingActivities = updateException.BlockingActivities;
                    if (blockingActivities.Count == 0)
                    {
                        blockingActivities = new List<ActivityBlockingUpdate>
                        {
                            new ActivityBlockingUpdate(this.WorkflowDefinition, this.WorkflowDefinition.Id, updateException.Message)
                        }.AsReadOnly();
                    }
                    this.TrackingProvider.AddRecord(new WorkflowInstanceUpdatedRecord(this.Id, this.WorkflowDefinition.DisplayName, originalDefinitionIdentity, this.DefinitionIdentity, blockingActivities));
                }
                throw;
            }
            finally
            {
                if (updateMap != null && !success)
                {
                    executor.MakeNonSerializable();
                }
            }            
        }

        void ThrowIfDynamicUpdateErrorExists(Collection<ActivityBlockingUpdate> updateErrors)
        {
            if (updateErrors != null && updateErrors.Count > 0)
            {
                // update error found
                // exit early

                throw FxTrace.Exception.AsError(new InstanceUpdateException(updateErrors));
            }
        }

        void ValidateWorkflow(WorkflowInstanceExtensionManager extensionManager)
        {
            if (!WorkflowDefinition.IsRuntimeReady)
            {
                LocationReferenceEnvironment localEnvironment = this.hostEnvironment;
                if (localEnvironment == null)
                {
                    LocationReferenceEnvironment parentEnvironment = null;
                    if (extensionManager != null && extensionManager.SymbolResolver != null)
                    {
                        parentEnvironment = extensionManager.SymbolResolver.AsLocationReferenceEnvironment();
                    }
                    localEnvironment = new ActivityLocationReferenceEnvironment(parentEnvironment);
                }
                IList<ValidationError> validationErrors = null;
                ActivityUtilities.CacheRootMetadata(WorkflowDefinition, localEnvironment, ProcessActivityTreeOptions.FullCachingOptions, null, ref validationErrors);
                ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
            }
        }

        void EnsureDefinitionReady()
        {
            if (this.extensions != null)
            {
                this.extensions.Initialize();
                if (this.extensions.HasTrackingParticipant)
                {
                    this.HasTrackingParticipant = true;
                    if (this.trackingProvider == null)
                    {
                        this.trackingProvider = new TrackingProvider(this.WorkflowDefinition);
                    }
                    else
                    {
                        // TrackingProvider could be non-null if an earlier initialization attempt failed.
                        // This happens when WorkflowApplication calls Abort after a load failure. In this
                        // case we want to preserve any pending tracking records (e.g. DU failure).
                        this.trackingProvider.ClearParticipants();
                    }
                    foreach (TrackingParticipant trackingParticipant in GetExtensions<TrackingParticipant>())
                    {
                        this.trackingProvider.AddParticipant(trackingParticipant);
                    }
                }
            }
            else
            {
                // need to ensure the workflow has been validated since the host isn't using extensions (and so didn't register anything)
                ValidateWorkflow(null);
            }
        }

        void InitializeCore(IDictionary<string, object> workflowArgumentValues, IList<Handle> workflowExecutionProperties)
        {
            Fx.Assert(this.WorkflowDefinition.IsRuntimeReady, "EnsureDefinitionReady should have been called");
            Fx.Assert(this.executor != null, "at this point, we better have an executor");

            // Do Argument validation for root activities
            WorkflowDefinition.HasBeenAssociatedWithAnInstance = true;

            if (workflowArgumentValues != null)
            {
                IDictionary<string, object> actualInputs = workflowArgumentValues;

                if (object.ReferenceEquals(actualInputs, ActivityUtilities.EmptyParameters))
                {
                    actualInputs = null;
                }

                if (this.WorkflowDefinition.RuntimeArguments.Count > 0 || (actualInputs != null && actualInputs.Count > 0))
                {
                    ActivityValidationServices.ValidateRootInputs(this.WorkflowDefinition, actualInputs);
                }

                this.executor.ScheduleRootActivity(this.WorkflowDefinition, actualInputs, workflowExecutionProperties);
            }
            else
            {
                this.executor.OnDeserialized(this.WorkflowDefinition, this);
            }

            this.executor.Open(this.SynchronizationContext);
            this.controller = new WorkflowInstanceControl(this, this.executor);
            this.isInitialized = true;

            if (this.extensions != null && this.extensions.HasWorkflowInstanceExtensions)
            {
                WorkflowInstanceProxy proxy = new WorkflowInstanceProxy(this);

                for (int i = 0; i < this.extensions.WorkflowInstanceExtensions.Count; i++)
                {
                    IWorkflowInstanceExtension extension = this.extensions.WorkflowInstanceExtensions[i];
                    extension.SetInstance(proxy);
                }
            }
        }

        protected void ThrowIfReadOnly()
        {
            if (this.isInitialized)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowInstanceIsReadOnly(this.Id)));
            }
        }

        protected internal abstract IAsyncResult OnBeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state);
        protected internal abstract BookmarkResumptionResult OnEndResumeBookmark(IAsyncResult result);

        protected internal abstract IAsyncResult OnBeginPersist(AsyncCallback callback, object state);
        protected internal abstract void OnEndPersist(IAsyncResult result);

        protected internal abstract void OnDisassociateKeys(ICollection<InstanceKey> keys);

        protected internal abstract IAsyncResult OnBeginAssociateKeys(ICollection<InstanceKey> keys, AsyncCallback callback, object state);
        protected internal abstract void OnEndAssociateKeys(IAsyncResult result);

        internal IAsyncResult BeginFlushTrackingRecordsInternal(AsyncCallback callback, object state)
        {
            return OnBeginFlushTrackingRecords(callback, state);
        }

        internal void EndFlushTrackingRecordsInternal(IAsyncResult result)
        {
            OnEndFlushTrackingRecords(result);
        }

        protected void FlushTrackingRecords(TimeSpan timeout)
        {
            if (this.HasTrackingParticipant)
            {
                this.TrackingProvider.FlushPendingRecords(timeout);
            }
        }

        protected IAsyncResult BeginFlushTrackingRecords(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.HasTrackingParticipant)
            {
                return this.TrackingProvider.BeginFlushPendingRecords(timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected void EndFlushTrackingRecords(IAsyncResult result)
        {
            if (this.HasTrackingParticipant)
            {
                this.TrackingProvider.EndFlushPendingRecords(result);
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected virtual IAsyncResult OnBeginFlushTrackingRecords(AsyncCallback callback, object state)
        {
            return this.Controller.BeginFlushTrackingRecords(ActivityDefaults.TrackingTimeout, callback, state);
        }

        protected virtual void OnEndFlushTrackingRecords(IAsyncResult result)
        {
            this.Controller.EndFlushTrackingRecords(result);
        }

        internal void NotifyPaused()
        {
            if (this.executor.State != ActivityInstanceState.Executing)
            {
                TrackCompletion();
            }

            OnNotifyPaused();
        }

        protected abstract void OnNotifyPaused();

        internal void NotifyUnhandledException(Exception exception, Activity source, string sourceInstanceId)
        {
            if (this.controller.TrackingEnabled)
            {
                ActivityInfo faultSourceInfo = new ActivityInfo(source.DisplayName, source.Id, sourceInstanceId, source.GetType().FullName);
                this.controller.Track(new WorkflowInstanceUnhandledExceptionRecord(this.Id, this.WorkflowDefinition.DisplayName, faultSourceInfo, exception, this.DefinitionIdentity));
            }

            OnNotifyUnhandledException(exception, source, sourceInstanceId);
        }

        protected abstract void OnNotifyUnhandledException(Exception exception, Activity source, string sourceInstanceId);

        protected internal abstract void OnRequestAbort(Exception reason);

        internal void OnDeserialized(bool hasTrackedStarted)
        {
            this.HasTrackedStarted = hasTrackedStarted;
        }

        void StartOperation(ref bool resetRequired)
        {
            StartReadOnlyOperation(ref resetRequired);

            // isRunning can only flip to true by an operation and therefore
            // we don't have to worry about this changing under us
            if (this.executor.IsRunning)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RuntimeRunning));
            }
        }

        void StartReadOnlyOperation(ref bool resetRequired)
        {
            bool wasPerformingOperation = false;
            try
            {
            }
            finally
            {
                wasPerformingOperation = Interlocked.CompareExchange(ref this.isPerformingOperation, True, False) == True;

                if (!wasPerformingOperation)
                {
                    resetRequired = true;
                }
            }

            if (wasPerformingOperation)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.RuntimeOperationInProgress));
            }
        }

        void FinishOperation(ref bool resetRequired)
        {
            if (resetRequired)
            {
                this.isPerformingOperation = False;
            }
        }

        internal void Abort(Exception reason)
        {
            if (!this.isAborted)
            {
                this.isAborted = true;
                if (reason != null)
                {
                    this.abortedException = reason;
                }

                if (this.extensions != null)
                {
                    this.extensions.Cancel();
                }

                if (this.controller.TrackingEnabled)
                {
                    // During abort we only track this one record
                    if (reason != null)
                    {
                        string message = reason.Message;
                        if (reason.InnerException != null)
                        {
                            message = SR.WorkflowAbortedReason(reason.Message, reason.InnerException.Message);
                        }
                        this.controller.Track(new WorkflowInstanceAbortedRecord(this.Id, this.WorkflowDefinition.DisplayName, message, this.DefinitionIdentity));
                    }
                }
#if DEBUG
                if (!Fx.FastDebug)
                {
                    if (reason != null)
                    {
                        reason.ToString();
                    }
                    this.abortStack = new StackTrace();
                }
#endif
            }
        }

        void ValidatePrepareForSerialization()
        {
            ThrowIfAborted();
            if (!this.Controller.IsPersistable)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PrepareForSerializationRequiresPersistability));
            }
        }

        void ValidateScheduleResumeBookmark()
        {
            ThrowIfAborted();
            ThrowIfNotIdle();
        }

        void ValidateGetBookmarks()
        {
            ThrowIfAborted();
        }

        void ValidateGetMappedVariables()
        {
            ThrowIfAborted();
        }

        void ValidatePauseWhenPersistable()
        {
            ThrowIfAborted();
            if (this.Controller.IsPersistable)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.PauseWhenPersistableInvalidIfPersistable));
            }
        }

        void Terminate(Exception reason)
        {
            // validate we're in an ok state
            ThrowIfAborted();

            // terminate the runtime
            this.executor.Terminate(reason);

            // and track if necessary
            TrackCompletion();

        }

        void TrackCompletion()
        {
            if (this.controller.TrackingEnabled && !this.hasTrackedCompletion)
            {
                ActivityInstanceState completionState = this.executor.State;

                if (completionState == ActivityInstanceState.Faulted)
                {
                    Fx.Assert(this.executor.TerminationException != null, "must have a termination exception if we're faulted");
                    this.controller.Track(new WorkflowInstanceTerminatedRecord(this.Id, this.WorkflowDefinition.DisplayName, this.executor.TerminationException.Message, this.DefinitionIdentity));
                }
                else if (completionState == ActivityInstanceState.Closed)
                {
                    this.controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Completed, this.DefinitionIdentity));
                }
                else
                {
                    Fx.AssertAndThrow(completionState == ActivityInstanceState.Canceled, "Cannot be executing a workflow instance when WorkflowState was completed.");
                    this.controller.Track(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Canceled, this.DefinitionIdentity));
                }
                this.hasTrackedCompletion = true;
            }
        }

        void TrackResumed()
        {
            // track if necessary
            if (!this.hasTrackedResumed)
            {
                if (this.Controller.TrackingEnabled)
                {
                    if (!this.HasTrackedStarted)
                    {
                        this.TrackingProvider.AddRecord(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Started, this.DefinitionIdentity));
                        this.HasTrackedStarted = true;
                    }
                    else
                    {
                        this.TrackingProvider.AddRecord(new WorkflowInstanceRecord(this.Id, this.WorkflowDefinition.DisplayName, WorkflowInstanceStates.Resumed, this.DefinitionIdentity));
                    }
                }
                this.hasTrackedResumed = true;
            }
        }

        void Run()
        {
            // validate we're in an ok state
            ThrowIfAborted();

            TrackResumed();

            // and let the scheduler go
            this.executor.MarkSchedulerRunning();
        }

        void ScheduleCancel()
        {
            // validate we're in an ok state
            ThrowIfAborted();

            TrackResumed();

            this.executor.CancelRootActivity();
        }

        BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value)
        {
            // validate we're in an ok state
            ValidateScheduleResumeBookmark();

            TrackResumed();

            return this.executor.TryResumeHostBookmark(bookmark, value);
        }

        BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value, BookmarkScope scope)
        {
            // validate we're in an ok state
            ValidateScheduleResumeBookmark();

            TrackResumed();

            return this.executor.TryResumeBookmark(bookmark, value, scope);
        }


        void ThrowIfAborted()
        {
            if (this.isAborted || (this.executor != null && this.executor.IsAbortPending))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WorkflowInstanceAborted(this.Id)));
            }
        }

        void ThrowIfNotIdle()
        {
            if (!this.executor.IsIdle)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.BookmarksOnlyResumableWhileIdle));
            }
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.NestedTypesShouldNotBeVisible,
            Justification = "these are effectively protected methods, but encapsulated in a struct to avoid naming conflicts")]
        protected struct WorkflowInstanceControl
        {
            ActivityExecutor executor;
            WorkflowInstance instance;

            internal WorkflowInstanceControl(WorkflowInstance instance, ActivityExecutor executor)
            {
                this.instance = instance;
                this.executor = executor;
            }

            public bool IsPersistable
            {
                get
                {
                    return this.executor.IsPersistable;
                }
            }

            public bool HasPendingTrackingRecords
            {
                get
                {
                    return this.instance.HasTrackingParticipant && this.instance.TrackingProvider.HasPendingRecords;
                }
            }

            public bool TrackingEnabled
            {
                get
                {
                    return this.instance.HasTrackingParticipant && this.instance.TrackingProvider.ShouldTrackWorkflowInstanceRecords;
                }
            }

            public WorkflowInstanceState State
            {
                get
                {
                    WorkflowInstanceState result;

                    if (this.instance.isAborted)
                    {
                        result = WorkflowInstanceState.Aborted;
                    }
                    else if (!this.executor.IsIdle)
                    {
                        result = WorkflowInstanceState.Runnable;
                    }
                    else
                    {
                        if (this.executor.State == ActivityInstanceState.Executing)
                        {
                            result = WorkflowInstanceState.Idle;
                        }
                        else
                        {
                            result = WorkflowInstanceState.Complete;
                        }
                    }

                    return result;
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is WorkflowInstanceControl))
                {
                    return false;
                }

                WorkflowInstanceControl other = (WorkflowInstanceControl)obj;
                return other.instance == this.instance;
            }

            public override int GetHashCode()
            {
                return this.instance.GetHashCode();
            }

            public static bool operator ==(WorkflowInstanceControl left, WorkflowInstanceControl right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(WorkflowInstanceControl left, WorkflowInstanceControl right)
            {
                return !left.Equals(right);
            }

            public ReadOnlyCollection<BookmarkInfo> GetBookmarks()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);

                    this.instance.ValidateGetBookmarks();

                    return this.executor.GetAllBookmarks();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public ReadOnlyCollection<BookmarkInfo> GetBookmarks(BookmarkScope scope)
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);

                    this.instance.ValidateGetBookmarks();

                    return this.executor.GetBookmarks(scope);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public IDictionary<string, LocationInfo> GetMappedVariables()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);

                    this.instance.ValidateGetMappedVariables();

                    IDictionary<string, LocationInfo> mappedLocations = this.instance.executor.GatherMappableVariables();
                    if (mappedLocations != null)
                    {
                        mappedLocations = new ReadOnlyDictionaryInternal<string, LocationInfo>(mappedLocations);
                    }
                    else
                    {
                        mappedLocations = WorkflowInstance.EmptyMappedVariablesDictionary;
                    }
                    return mappedLocations;
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Run()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    this.instance.Run();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }

                this.executor.Run();
            }

            public void RequestPause()
            {
                // No validations for this because we do not
                // require calls to Pause to be synchronized
                // by the caller
                this.executor.PauseScheduler();
            }

            // Calls Pause when IsPersistable goes from false->true
            public void PauseWhenPersistable()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    this.instance.ValidatePauseWhenPersistable();

                    this.executor.PauseWhenPersistable();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void ScheduleCancel()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    this.instance.ScheduleCancel();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Terminate(Exception reason)
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    this.instance.Terminate(reason);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value)
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    return this.instance.ScheduleBookmarkResumption(bookmark, value);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public BookmarkResumptionResult ScheduleBookmarkResumption(Bookmark bookmark, object value, BookmarkScope scope)
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    return this.instance.ScheduleBookmarkResumption(bookmark, value, scope);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Abort()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    // No validations

                    this.executor.Dispose();

                    this.instance.Abort(null);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public void Abort(Exception reason)
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartOperation(ref resetRequired);

                    // No validations

                    this.executor.Abort(reason);

                    this.instance.Abort(reason);
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            [SuppressMessage(FxCop.Category.Design, FxCop.Rule.ConsiderPassingBaseTypesAsParameters,
                Justification = "Only want to allow WorkflowInstanceRecord subclasses for WorkflowInstance-level tracking")]
            public void Track(WorkflowInstanceRecord instanceRecord)
            {
                if (this.instance.HasTrackingParticipant)
                {
                    this.instance.TrackingProvider.AddRecord(instanceRecord);
                }
            }

            public void FlushTrackingRecords(TimeSpan timeout)
            {
                this.instance.FlushTrackingRecords(timeout);
            }

            public IAsyncResult BeginFlushTrackingRecords(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.instance.BeginFlushTrackingRecords(timeout, callback, state);
            }

            public void EndFlushTrackingRecords(IAsyncResult result)
            {
                this.instance.EndFlushTrackingRecords(result);
            }

            public object PrepareForSerialization()
            {
                bool resetRequired = false;

                try
                {
                    this.instance.StartReadOnlyOperation(ref resetRequired);

                    this.instance.ValidatePrepareForSerialization();

                    return this.executor.PrepareForSerialization();
                }
                finally
                {
                    this.instance.FinishOperation(ref resetRequired);
                }
            }

            public ActivityInstanceState GetCompletionState()
            {
                return this.executor.State;
            }

            [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters,
                Justification = "Arch approved design. Requires the out argument for extra information provided")]
            public ActivityInstanceState GetCompletionState(out Exception terminationException)
            {
                terminationException = this.executor.TerminationException;
                return this.executor.State;
            }

            [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters,
                Justification = "Arch approved design. Requires the out argument for extra information provided")]
            public ActivityInstanceState GetCompletionState(out IDictionary<string, object> outputs, out Exception terminationException)
            {
                outputs = this.executor.WorkflowOutputs;
                terminationException = this.executor.TerminationException;
                return this.executor.State;
            }

            public Exception GetAbortReason()
            {
                return this.instance.abortedException;
            }
        }
    }
}
