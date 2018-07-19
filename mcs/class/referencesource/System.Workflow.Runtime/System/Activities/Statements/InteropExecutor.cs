//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [DataContract]
    class InteropExecutor : IWorkflowCoreRuntime, ISupportInterop
    {
        Dictionary<int, Activity> contextActivityMap;

        Activity currentActivity;
        Activity currentAtomicActivity;
        Activity internalCurrentActivity;

        bool trackingEnabled;
        bool hasCheckedForTrackingParticipant;

        [DataMember]
        Dictionary<Bookmark, IComparable> bookmarkQueueMap;

        [DataMember]
        int currentContextId;

        [DataMember]
        Guid instanceId;

        [DataMember]
        int eventCounter;

        IList<PropertyInfo> outputProperties;
        IDictionary<string, object> outputs;
        Exception outstandingException;

        [DataMember]
        byte[] persistedActivityData;
        Activity rootActivity;
        Scheduler scheduler;
        WorkflowQueuingService workflowQueuingService;
        TimerSchedulerService timerSchedulerSerivce;
        TimerEventSubscriptionCollection timerQueue;
        VolatileResourceManager resourceManager;
        ServiceEnvironment serviceEnvironment;

        [DataMember(EmitDefaultValue = false)]
        int atomicActivityContextId;
        [DataMember(EmitDefaultValue = false)]
        int internalCurrentActivityContextId;
        [DataMember(EmitDefaultValue = false)]
        string atomicActivityName;
        [DataMember(EmitDefaultValue = false)]
        string internalCurrentActivityName;

        Exception lastExceptionThrown;
        bool abortTransaction;

        public InteropExecutor(Guid instanceId, Activity rootActivity, IList<PropertyInfo> outputProperties, Activity activityDefinition)
        {
            this.PrivateInitialize(rootActivity, instanceId, outputProperties, activityDefinition);
        }

        public Activity CurrentActivity
        {
            get
            {
                return this.currentActivity;
            }
            set
            {
                this.currentActivity = value;
            }
        }

        public IDictionary<string, object> Outputs
        {
            get
            {
                return this.outputs;
            }
        }

        public IEnumerable<IComparable> Queues
        {
            get
            {
                return this.workflowQueuingService.QueueNames;
            }
        }

        public Dictionary<System.Activities.Bookmark, IComparable> BookmarkQueueMap
        {
            get
            {
                if (this.bookmarkQueueMap == null)
                {
                    this.bookmarkQueueMap = new Dictionary<System.Activities.Bookmark, IComparable>();
                }
                return this.bookmarkQueueMap;
            }
        }

        public InteropEnvironment ServiceProvider
        {
            get;
            set;
        }

        public Activity CurrentAtomicActivity
        {
            get
            {
                return this.currentAtomicActivity;
            }
        }

        public Guid InstanceID
        {
            get
            {
                return this.instanceId;
            }
        }

        public bool IsDynamicallyUpdated
        {
            get
            {
                return false;
            }
        }

        public Activity RootActivity
        {
            get
            {
                return this.rootActivity;
            }
        }

        TimerEventSubscriptionCollection TimerQueue
        {
            get
            {
                if (this.timerQueue == null)
                {
                    this.timerQueue = (TimerEventSubscriptionCollection)this.rootActivity.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
                    Debug.Assert(this.timerQueue != null, "TimerEventSubscriptionCollection on root activity should never be null, but it was");
                }
                return this.timerQueue;
            }
            set
            {
                this.timerQueue = value;
                this.rootActivity.SetValue(TimerEventSubscriptionCollection.TimerCollectionProperty, this.timerQueue);
            }
        }

        public WaitCallback ProcessTimersCallback
        {
            get
            {
                return new WaitCallback(this.ProcessTimers);
            }
        }

        public WorkBatchCollection BatchCollection
        {
            get { return this.resourceManager.BatchCollection; }
        }

        public bool TrackingEnabled
        {
            get
            {
                return this.trackingEnabled;
            }
            set
            {
                this.trackingEnabled = value;
            }
        }

        public bool HasCheckedForTrackingParticipant
        {
            get
            {
                return this.hasCheckedForTrackingParticipant;
            }
            set
            {
                this.hasCheckedForTrackingParticipant = value;
            }
        }

        public ActivityExecutionStatus EnqueueEvent(IComparable queueName, object item)
        {
            if (queueName == null)
            {
                throw new ArgumentNullException("queueName");
            }

            this.workflowQueuingService.EnqueueEvent(queueName, item);

            Guid timerId = Guid.Empty;
            if (Guid.TryParse(queueName.ToString(), out timerId))
            {
                // This is a no-op if this is not a timer event.
                this.TimerQueue.Remove(timerId);
            }

            scheduler.Run();
            return TranslateExecutionStatus();
        }

        public ActivityExecutionStatus Resume()
        {
            scheduler.Run();
            return TranslateExecutionStatus();
        }

        public void SetAmbientTransactionAndServiceEnvironment(Transaction transaction)
        {
            this.serviceEnvironment = new ServiceEnvironment(this.RootActivity);

            if (transaction != null && this.currentAtomicActivity != null)
            {
                TransactionalProperties transactionalProperties = (TransactionalProperties)this.currentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                Debug.Assert(transactionalProperties != null, "The current atomic activity is missing transactional properties");
                transactionalProperties.Transaction = transaction;
                transactionalProperties.TransactionScope = new System.Transactions.TransactionScope(transactionalProperties.Transaction, TimeSpan.Zero, EnterpriseServicesInteropOption.Full);
            }
        }

        public void ClearAmbientTransactionAndServiceEnvironment()
        {
            try
            {
                if (this.resourceManager.IsBatchDirty)
                {
                    this.ServiceProvider.AddResourceManager(this.resourceManager);
                }

                if (this.currentAtomicActivity != null)
                {
                    TransactionalProperties transactionalProperties = (TransactionalProperties)this.currentAtomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                    Debug.Assert(transactionalProperties != null, "The current atomic activity is missing transactional properties");
                    transactionalProperties.Transaction = null;
                    if (transactionalProperties.TransactionScope != null)
                    {
                        transactionalProperties.TransactionScope.Complete();
                        transactionalProperties.TransactionScope.Dispose();
                        transactionalProperties.TransactionScope = null;
                    }
                }
            }
            finally
            {
                ((IDisposable)this.serviceEnvironment).Dispose();
                this.serviceEnvironment = null;
            }
        }

        public bool CheckAndProcessTransactionAborted(TransactionalProperties transactionalProperties)
        {
            if (transactionalProperties.Transaction != null && transactionalProperties.Transaction.TransactionInformation.Status != TransactionStatus.Aborted)
            {
                return false;
            }

            if (transactionalProperties.TransactionState != TransactionProcessState.AbortProcessed)
            {
                // The transaction has aborted.  The WF3 runtime throws a TransactionAborted exception here, which then propagates as fault.
                // But WF4 aborts the workflow, so pause the scheduler and return.
                this.scheduler.Pause();
                transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
            }

            return true;
        }

        public bool IsActivityInAtomicContext(Activity activity, out Activity atomicActivity)
        {
            atomicActivity = null;
            while (activity != null)
            {
                if (activity == this.currentAtomicActivity)
                {
                    atomicActivity = activity;
                    return true;
                }
                activity = activity.Parent;
            }
            return false;
        }

        public ActivityExecutionStatus Execute()
        {
            using (ActivityExecutionContext activityExecutionContext = new ActivityExecutionContext(this.rootActivity, true))
            {
                activityExecutionContext.ExecuteActivity(this.rootActivity);
            }
            scheduler.Run();
            return TranslateExecutionStatus();
        }

        public ActivityExecutionStatus Cancel()
        {
            using (ActivityExecutionContext activityExecutionContext = new ActivityExecutionContext(this.rootActivity, true))
            {

                if (this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    activityExecutionContext.CancelActivity(this.rootActivity);
                }
            }
            scheduler.Run();
            return TranslateExecutionStatus();
        }

        public void Initialize(Activity definition, IDictionary<string, object> inputs, bool hasNameCollision)
        {
            this.rootActivity.SetValue(Activity.ActivityExecutionContextInfoProperty,
                new ActivityExecutionContextInfo(this.rootActivity.QualifiedName, this.GetNewContextActivityId(), instanceId, -1));
            this.rootActivity.SetValue(Activity.ActivityContextGuidProperty, instanceId);

            SetInputParameters(definition, this.rootActivity, inputs, hasNameCollision);

            ((IDependencyObjectAccessor)this.rootActivity).InitializeActivatingInstanceForRuntime(
                null,
                this);

            this.rootActivity.FixUpMetaProperties(definition);

            TimerQueue = new TimerEventSubscriptionCollection(this, this.instanceId);

            using (new ServiceEnvironment(this.rootActivity))
            {
                using (SetCurrentActivity(this.rootActivity))
                {
                    RegisterContextActivity(this.rootActivity);

                    using (ActivityExecutionContext executionContext = new ActivityExecutionContext(this.rootActivity, true))
                    {
                        executionContext.InitializeActivity(this.rootActivity);
                    }
                }
            }
        }

        internal void EnsureReload(Interop activity)
        {
            if (this.rootActivity == null)
            {
                this.Reload(
                    activity.ComponentModelActivity,
                    activity.OutputPropertyDefinitions);
            }
        }

        [OnSerializing]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "required signature for serialization")]
        void OnSerializing(StreamingContext context)
        {
            // If the Interop activity is serialized twice without a EnsureReload call in between, then the root activity is null.
            if (this.rootActivity == null)
            {
                // The root activity has already been serialized and saved in this.persistedActivityData so there is nothing to do.
                return;
            }

            using (MemoryStream stream = new MemoryStream(10240))
            {
                stream.Position = 0;
                this.rootActivity.Save(stream);
                this.persistedActivityData = stream.GetBuffer();
                Array.Resize<byte>(ref this.persistedActivityData, Convert.ToInt32(stream.Length));
            }

            if (this.internalCurrentActivity != null)
            {
                this.internalCurrentActivityContextId = this.internalCurrentActivity.ContextId;
                this.internalCurrentActivityName = this.internalCurrentActivity.QualifiedName;
            }

            if (this.CurrentAtomicActivity != null)
            {
                this.atomicActivityContextId = this.CurrentAtomicActivity.ContextId;
                this.atomicActivityName = this.CurrentAtomicActivity.QualifiedName;
            }
        }

        public void Reload(Activity definitionActivity, IList<PropertyInfo> outputProperties)
        {
            MemoryStream stream = new MemoryStream(this.persistedActivityData);
            Activity activity = null;

            stream.Position = 0;

            using (new ActivityDefinitionResolution(definitionActivity))
            {
                activity = Activity.Load(stream, null);
            }

            this.PrivateInitialize(activity, instanceId, outputProperties, definitionActivity);

            // register all dynamic activities for loading
            Queue<Activity> dynamicActivitiesQueue = new Queue<Activity>();
            dynamicActivitiesQueue.Enqueue(activity);
            while (dynamicActivitiesQueue.Count > 0)
            {
                Activity dynamicActivity = dynamicActivitiesQueue.Dequeue();
                ((IDependencyObjectAccessor)dynamicActivity).InitializeInstanceForRuntime(this);
                this.RegisterContextActivity(dynamicActivity);

                IList<Activity> nestedDynamicActivities = (IList<Activity>)dynamicActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                if (nestedDynamicActivities != null)
                {
                    foreach (Activity nestedDynamicActivity in nestedDynamicActivities)
                    {
                        dynamicActivitiesQueue.Enqueue(nestedDynamicActivity);
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.internalCurrentActivityName))
            {
                this.internalCurrentActivity = this.GetContextActivityForId(this.internalCurrentActivityContextId).GetActivityByName(this.internalCurrentActivityName);
            }

            if (!string.IsNullOrEmpty(this.atomicActivityName))
            {
                this.currentAtomicActivity = this.GetContextActivityForId(this.atomicActivityContextId).GetActivityByName(this.atomicActivityName);
            }

            this.TimerQueue.Executor = this;
        }

        void PrivateInitialize(Activity rootActivity, Guid instanceId, IList<PropertyInfo> outputProperties, Activity workflowDefinition)
        {
            this.instanceId = instanceId;
            this.rootActivity = rootActivity;
            this.contextActivityMap = new Dictionary<int, Activity>();
            this.scheduler = new Scheduler(this);
            this.workflowQueuingService = new WorkflowQueuingService(this);
            this.outputProperties = outputProperties;
            this.resourceManager = new VolatileResourceManager();

            this.rootActivity.SetValue(System.Workflow.ComponentModel.Activity.WorkflowDefinitionProperty, workflowDefinition);
            this.rootActivity.SetValue(WorkflowExecutor.WorkflowExecutorProperty, this);
        }

        static void SetInputParameters(Activity definition, Activity rootActivity, IDictionary<string, object> inputs, bool hasNameCollision)
        {
            if (inputs != null)
            {
                int suffixLength = Interop.InArgumentSuffix.Length;
                foreach (KeyValuePair<string, object> input in inputs)
                {
                    PropertyInfo propertyInfo;
                    //If there was a naming collision, we renamed the InArguments and need to strip "In" from the end of the property name
                    if (hasNameCollision)
                    {
                        string truncatedName = input.Key.Substring(0, input.Key.Length - suffixLength);
                        propertyInfo = definition.GetType().GetProperty(truncatedName);
                    }
                    else
                    {
                        propertyInfo = definition.GetType().GetProperty(input.Key);
                    }
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(rootActivity, input.Value, null);
                    }
                }
            }
        }

        ActivityExecutionStatus TranslateExecutionStatus()
        {
            if (this.abortTransaction)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1127,
                    string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropExceptionTraceMessage,
                        this.ServiceProvider.Activity.DisplayName,
                        this.lastExceptionThrown.ToString()));
                throw this.lastExceptionThrown;
            }

            if (this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                //Extract Outputs from V1Activity.
                if (this.outputProperties.Count != 0)
                {
                    this.outputs = new Dictionary<string, object>(this.outputProperties.Count);

                    foreach (PropertyInfo property in this.outputProperties)
                    {
                        //We renamed the OutArgument half of the pair. Don't attempt to populate if there is no Get method.
                        if (property.CanRead && (property.GetGetMethod() != null))
                        {
                            this.outputs.Add(property.Name + Interop.OutArgumentSuffix, property.GetValue(this.rootActivity, null));
                        }
                    }
                }
                if (this.outstandingException != null)
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 1127,
                        string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropExceptionTraceMessage,
                            this.ServiceProvider.Activity.DisplayName,
                            this.outstandingException.ToString()));
                    throw this.outstandingException;
                }
            }
            return this.rootActivity.ExecutionStatus;
        }

        public void ActivityStatusChanged(Activity activity, bool transacted, bool committed)
        {
            if (!committed)
            {
                // Forward to 4.0 tracking mechanism, AEC.Track
                if (this.trackingEnabled)
                {
                    this.ServiceProvider.TrackActivityStatusChange(activity, this.eventCounter++);
                }

                if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    this.ScheduleDelayedItems(activity);
                }
            }

            if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                if (!(activity is ICompensatableActivity) || ((activity is ICompensatableActivity) && activity.CanUninitializeNow))
                {
                    CorrelationTokenCollection.UninitializeCorrelationTokens(activity);
                }
            }
        }

        public void CheckpointInstanceState(Activity atomicActivity)
        {
            // Note that the WF4 runtime does not create checkpoints.  If the transaction aborts, the workflow aborts.  
            // We are following the WF4 behavior and not creating a checkpoint.

            TransactionOptions tranOpts = new TransactionOptions();
            WorkflowTransactionOptions atomicTxn = TransactedContextFilter.GetTransactionOptions(atomicActivity);

            tranOpts.IsolationLevel = atomicTxn.IsolationLevel;
            if (tranOpts.IsolationLevel == IsolationLevel.Unspecified)
            {
                tranOpts.IsolationLevel = IsolationLevel.Serializable;
            }

            tranOpts.Timeout = atomicTxn.TimeoutDuration;

            TransactionalProperties transactionProperties = new TransactionalProperties();
            atomicActivity.SetValue(WorkflowExecutor.TransactionalPropertiesProperty, transactionProperties);
            this.ServiceProvider.CreateTransaction(tranOpts);
            this.currentAtomicActivity = atomicActivity;
            this.scheduler.Pause();
        }

        public void DisposeCheckpointState()
        {
            // Nothing to do.  The interop activity executor doesn't create checkpoints.
        }

        public Activity GetContextActivityForId(int id)
        {
            return this.contextActivityMap[id];
        }

        public int GetNewContextActivityId()
        {
            return this.currentContextId++;
        }

        public object GetService(Activity currentActivity, Type serviceType)
        {
            if (serviceType == typeof(IWorkflowCoreRuntime))
            {
                return this;
            }
            if (serviceType == typeof(WorkflowQueuingService))
            {
                this.workflowQueuingService.CallingActivity = ContextActivity(currentActivity);
                return this.workflowQueuingService;
            }

            if (serviceType == typeof(ITimerService))
            {
                if (this.timerSchedulerSerivce == null)
                {
                    this.timerSchedulerSerivce = new TimerSchedulerService(this);
                }
                return this.timerSchedulerSerivce;
            }

            return ((IServiceProvider)this.ServiceProvider).GetService(serviceType);
        }

        public Activity LoadContextActivity(ActivityExecutionContextInfo contextInfo, Activity outerContextActivity)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        public void OnAfterDynamicChange(bool updateSucceeded, System.Collections.Generic.IList<WorkflowChangeAction> changes)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        public bool OnBeforeDynamicChange(System.Collections.Generic.IList<WorkflowChangeAction> changes)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        public void PersistInstanceState(Activity activity)
        {
            this.lastExceptionThrown = null;
            this.abortTransaction = false;

            this.ScheduleDelayedItems(activity);

            if (this.currentAtomicActivity == null)
            {
                if (activity == this.rootActivity && !activity.PersistOnClose)
                {
                    // This method is called when the root activity completes.  We shouldn't perist unless the root has [PersistOnClose]
                    return;
                }

                this.ServiceProvider.Persist();
            }
            else
            {
                TransactionalProperties transactionalProperties = null;
                transactionalProperties = (TransactionalProperties)activity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
                if (this.CheckAndProcessTransactionAborted(transactionalProperties))
                {
                    return;
                }

                // Complete and dispose transaction scope
                transactionalProperties.TransactionScope.Complete();
                transactionalProperties.TransactionScope.Dispose();
                transactionalProperties.TransactionScope = null;

                this.ServiceProvider.CommitTransaction();

                transactionalProperties.Transaction = null;
                this.currentAtomicActivity = null;
            }

            this.internalCurrentActivity = activity;
            this.scheduler.Pause();
        }

        public void RaiseActivityExecuting(Activity activity)
        {
            // No tracking needed since no tracking was done here in V1               
        }

        public void RaiseException(Exception e, Activity activity, string responsibleActivity)
        {
            // No tracking needed
            using (SetCurrentActivity(activity))
            {
                using (ActivityExecutionContext executionContext = new ActivityExecutionContext(activity, true))
                {
                    executionContext.FaultActivity(e);
                }
            }
        }

        public void RaiseHandlerInvoked()
        {
        }

        public void RaiseHandlerInvoking(Delegate delegateHandler)
        {
        }

        void ProcessTimers(object ignored)
        {
            // No-op in V2.  Timers are managed by TimerExtension.
        }

        public void RegisterContextActivity(Activity activity)
        {
            int contextId = ContextId(activity);
            this.contextActivityMap.Add(contextId, activity);

            activity.OnActivityExecutionContextLoad(this);
        }

        static int ContextId(Activity activity)
        {
            return ((ActivityExecutionContextInfo)ContextActivity(activity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextId;
        }

        static Activity ContextActivity(Activity activity)
        {
            Activity contextActivity = activity;

            while (contextActivity != null && contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty) == null)
            {
                contextActivity = contextActivity.Parent;
            }
            return contextActivity;
        }

        public void RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendReason)
        {
            if (this.lastExceptionThrown != null)
            {
                // Transaction scope activity is trying to abort the transaction due to an exception.
                // Pause the scheduler and throw the exception to the V2 runtime.
                // The V2 runtime will abort the workflow.
                this.abortTransaction = true;
                this.scheduler.Pause();
            }
        }

        bool IWorkflowCoreRuntime.Resume()
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        public void SaveContextActivity(Activity contextActivity)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
            Justification = @"The transacted parameter represents items that should not be processed if the ambient transction rolls back.
            Since the Interop activity just aborts the workflow on rollback, this parameter is not applicable here.")]
        public void ScheduleItem(SchedulableItem item, bool isInAtomicTransaction, bool transacted, bool queueInTransaction)
        {
            if (queueInTransaction)
            {
                this.AddItemToBeScheduledLater(this.CurrentActivity, item);
            }
            else
            {
                this.scheduler.ScheduleItem(item, isInAtomicTransaction);
            }
        }

        void AddItemToBeScheduledLater(Activity atomicActivity, SchedulableItem item)
        {
            if (atomicActivity == null)
            {
                return;
            }

            // Activity may not be atomic and is an activity which is not
            // yet scheduled for execution (typically receive case)
            if (!atomicActivity.SupportsTransaction)
            {
                return;
            }

            TransactionalProperties transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);
            if (transactionalProperties != null)
            {
                lock (transactionalProperties)
                {
                    List<SchedulableItem> notifications = null;
                    notifications = transactionalProperties.ItemsToBeScheduledAtCompletion;
                    if (notifications == null)
                    {
                        notifications = new List<SchedulableItem>();
                        transactionalProperties.ItemsToBeScheduledAtCompletion = notifications;
                    }
                    notifications.Add(item);
                }
            }
        }

        void ScheduleDelayedItems(Activity atomicActivity)
        {
            List<SchedulableItem> items = null;
            TransactionalProperties transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);

            if (transactionalProperties == null)
            {
                return;
            }

            lock (transactionalProperties)
            {
                items = transactionalProperties.ItemsToBeScheduledAtCompletion;
                if (items == null)
                {
                    return;
                }

                foreach (SchedulableItem item in items)
                {
                    this.scheduler.ScheduleItem(item, false);
                }
                items.Clear();

                transactionalProperties.ItemsToBeScheduledAtCompletion = null;
            }
        }

        public IDisposable SetCurrentActivity(Activity activity)
        {
            Activity oldCurrentActivity = this.CurrentActivity;
            this.CurrentActivity = activity;
            return new ResetCurrentActivity(this, oldCurrentActivity);
        }

        public Guid StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        public bool SuspendInstance(string suspendDescription)
        {
            throw new NotImplementedException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropNonSupportedBehavior, this.ServiceProvider.Activity.DisplayName));
        }

        public void TerminateInstance(Exception e)
        {
            this.outstandingException = e;
        }

        public void Track(string key, object data)
        {
            // Forward to 4.0 tracking mechanism, AEC.Track
            if (this.trackingEnabled)
            {
                this.ServiceProvider.TrackData(this.CurrentActivity, this.eventCounter++, key, data);
            }
        }

        public void UnregisterContextActivity(Activity activity)
        {
            int contextId = ContextId(activity);
            this.contextActivityMap.Remove(contextId);
            activity.OnActivityExecutionContextUnload(this);
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return this.GetService(this.rootActivity, serviceType);
        }

        class ActivityDefinitionResolution : IDisposable
        {
            [ThreadStatic]
            static Activity definitionActivity;
            static ActivityResolveEventHandler activityResolveEventHandler = new ActivityResolveEventHandler(OnActivityResolve);

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "This is a bogus validation; registration to event cannot be done using field initializers")]
            static ActivityDefinitionResolution()
            {
                Activity.ActivityResolve += ActivityDefinitionResolution.activityResolveEventHandler;
            }

            public ActivityDefinitionResolution(Activity definitionActivity)
            {
                ActivityDefinitionResolution.definitionActivity = definitionActivity;
            }

            static Activity OnActivityResolve(object sender, ActivityResolveEventArgs e)
            {
                return ActivityDefinitionResolution.definitionActivity;
            }

            void IDisposable.Dispose()
            {
                ActivityDefinitionResolution.definitionActivity = null;
            }
        }

        class ResetCurrentActivity : IDisposable
        {
            InteropExecutor activityExecutor;
            Activity oldCurrentActivity = null;

            internal ResetCurrentActivity(InteropExecutor activityExecutor, Activity oldCurrentActivity)
            {
                this.activityExecutor = activityExecutor;
                this.oldCurrentActivity = oldCurrentActivity;
            }
            void IDisposable.Dispose()
            {
                this.activityExecutor.CurrentActivity = oldCurrentActivity;
            }
        }

        class Scheduler
        {
            // State to be persisted for the scheduler
            internal static DependencyProperty SchedulerQueueProperty = DependencyProperty.RegisterAttached("SchedulerQueue", typeof(Queue<SchedulableItem>), typeof(Scheduler));
            internal static DependencyProperty AtomicActivityQueueProperty = DependencyProperty.RegisterAttached("AtomicActivityQueue", typeof(Queue<SchedulableItem>), typeof(Scheduler));

            // The atomic activity queue contains items related to the currently executing atomic activity.  
            // While the atomic activity is executing, execution of the items that are not in the atomic context is deferred until the atomic activity completes.
            // We need two queues to separate the items associated with the atomic activity from the other items.
            Queue<SchedulableItem> schedulerQueue;
            Queue<SchedulableItem> atomicActivityQueue;

            InteropExecutor owner;
            bool pause;

            public Scheduler(InteropExecutor owner)
            {
                this.owner = owner;
                this.schedulerQueue = (Queue<SchedulableItem>)owner.RootActivity.GetValue(Scheduler.SchedulerQueueProperty);
                if (this.schedulerQueue == null)
                {
                    this.schedulerQueue = new Queue<SchedulableItem>();
                    owner.RootActivity.SetValue(Scheduler.SchedulerQueueProperty, this.schedulerQueue);
                }

                this.atomicActivityQueue = (Queue<SchedulableItem>)owner.RootActivity.GetValue(Scheduler.AtomicActivityQueueProperty);
                if (this.atomicActivityQueue == null)
                {
                    this.atomicActivityQueue = new Queue<SchedulableItem>();
                    owner.RootActivity.SetValue(Scheduler.AtomicActivityQueueProperty, this.atomicActivityQueue);
                }
            }

            public void ScheduleItem(SchedulableItem item, bool isInAtomicTransaction)
            {
                Queue<SchedulableItem> queue = isInAtomicTransaction ? this.atomicActivityQueue : this.schedulerQueue;
                queue.Enqueue(item);
            }

            public void Pause()
            {
                this.pause = true;
            }

            public void Run()
            {
                this.pause = false;

                while (!this.pause)
                {
                    SchedulableItem item;

                    // atomicActivityQueue has higher priority
                    if (this.atomicActivityQueue.Count > 0)
                    {
                        item = this.atomicActivityQueue.Dequeue();
                    }
                    // The execution of the items in the scheduler queue is deferred until the atomic activity completes.
                    else if (owner.CurrentAtomicActivity == null &&
                        this.schedulerQueue.Count > 0)
                    {
                        item = schedulerQueue.Dequeue();
                    }
                    else
                    {
                        break;
                    }

                    Activity itemActivity = owner.GetContextActivityForId(item.ContextId).GetActivityByName(item.ActivityId);
                    Activity atomicActivity;
                    TransactionalProperties transactionalProperties = null;

                    if (owner.IsActivityInAtomicContext(itemActivity, out atomicActivity))
                    {
                        transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(WorkflowExecutor.TransactionalPropertiesProperty);

                        // If we've aborted for any reason stop now!
                        if (owner.CheckAndProcessTransactionAborted(transactionalProperties))
                        {
                            return;
                        }
                    }

                    try
                    {
                        item.Run(owner);
                    }
                    catch (Exception e)
                    {
                        if (WorkflowExecutor.IsIrrecoverableException(e))
                        {
                            throw;
                        }

                        if (transactionalProperties != null)
                        {
                            transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                            owner.lastExceptionThrown = e;
                        }

                        owner.RaiseException(e, itemActivity, null);

                    }
                }
            }
        }

        class TimerSchedulerService : ITimerService
        {
            IWorkflowCoreRuntime executor;

            public TimerSchedulerService(IWorkflowCoreRuntime executor)
                : base()
            {
                this.executor = executor;
            }

            public void ScheduleTimer(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
            {
                if (timerId == Guid.Empty)
                {
                    throw new ArgumentException(ExecutionStringManager.InteropTimerIdCantBeEmpty, "timerId");
                }

                TimeSpan timerDuration = whenUtc - DateTime.UtcNow;

                if (timerDuration < TimeSpan.Zero)
                {
                    timerDuration = TimeSpan.Zero;
                }

                GetTimerExtension().RegisterTimer(timerDuration, new Bookmark(timerId.ToString()));
            }

            public void CancelTimer(Guid timerId)
            {
                if (timerId == Guid.Empty)
                {
                    throw new ArgumentException(ExecutionStringManager.InteropTimerIdCantBeEmpty, "timerId");
                }

                GetTimerExtension().CancelTimer(new Bookmark(timerId.ToString()));
            }

            TimerExtension GetTimerExtension()
            {
                TimerExtension timerExtension = this.executor.GetService(typeof(TimerExtension)) as TimerExtension;
                if (timerExtension == null)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InteropCantFindTimerExtension);
                }

                return timerExtension;
            }
        }
    }
}
