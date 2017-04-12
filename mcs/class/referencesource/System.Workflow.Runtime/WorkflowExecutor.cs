#pragma warning disable 1634, 1691
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Transactions;
using SES = System.EnterpriseServices;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.DebugEngine;

namespace System.Workflow.Runtime
{
    /// <remarks>
    /// The runtime object that represents the schedule.
    /// </remarks>
    internal sealed class WorkflowExecutor : IWorkflowCoreRuntime, IServiceProvider, ISupportInterop
    {
        internal readonly static DependencyProperty WorkflowExecutorProperty = DependencyProperty.RegisterAttached("WorkflowExecutor", typeof(IWorkflowCoreRuntime), typeof(WorkflowExecutor), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        // The static method GetTransientBatch is used by this property to retrieve the WorkBatch.
        // GetTransientBatch is defined in this class but if the workflow is running under a V2.0 Interop environment,
        // it forwards the call to the Interop activity.
        internal readonly static DependencyProperty TransientBatchProperty = DependencyProperty.RegisterAttached("TransientBatch", typeof(IWorkBatch), typeof(WorkflowExecutor), new PropertyMetadata(null, DependencyPropertyOptions.NonSerialized, new GetValueOverride(GetTransientBatch), null));
        internal readonly static DependencyProperty TransactionalPropertiesProperty = DependencyProperty.RegisterAttached("TransactionalProperties", typeof(TransactionalProperties), typeof(WorkflowExecutor), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal readonly static DependencyProperty WorkflowInstanceIdProperty = DependencyProperty.RegisterAttached("WorkflowInstanceId", typeof(Guid), typeof(WorkflowExecutor), new PropertyMetadata(Guid.NewGuid()));
        internal readonly static DependencyProperty IsBlockedProperty = DependencyProperty.RegisterAttached("IsBlocked", typeof(bool), typeof(WorkflowExecutor), new PropertyMetadata(false));
        internal readonly static DependencyProperty WorkflowStatusProperty = DependencyProperty.RegisterAttached("WorkflowStatus", typeof(WorkflowStatus), typeof(WorkflowExecutor), new PropertyMetadata(WorkflowStatus.Created));
        internal readonly static DependencyProperty SuspendOrTerminateInfoProperty = DependencyProperty.RegisterAttached("SuspendOrTerminateInfo", typeof(string), typeof(WorkflowExecutor));

        // Persisted state properties
        private static DependencyProperty ContextIdProperty = DependencyProperty.RegisterAttached("ContextId", typeof(int), typeof(WorkflowExecutor), new PropertyMetadata(new Int32()));
        private static DependencyProperty TrackingCallingStateProperty = DependencyProperty.RegisterAttached("TrackingCallingState", typeof(TrackingCallingState), typeof(WorkflowExecutor));
        internal static DependencyProperty TrackingListenerBrokerProperty = DependencyProperty.RegisterAttached("TrackingListenerBroker", typeof(TrackingListenerBroker), typeof(WorkflowExecutor));
        private static DependencyProperty IsSuspensionRequestedProperty = DependencyProperty.RegisterAttached("IsSuspensionRequested", typeof(bool), typeof(WorkflowExecutor), new PropertyMetadata(false));
        private static DependencyProperty IsIdleProperty = DependencyProperty.RegisterAttached("IsIdle", typeof(bool), typeof(WorkflowExecutor), new PropertyMetadata(false));

        #region Data Members - Please keep all the data here

        internal Activity currentAtomicActivity;
        private ManualResetEvent atomicActivityEvent;

        private Hashtable completedContextActivities = new Hashtable();

        Activity rootActivity;

        WorkflowRuntime _runtime;                            // hosting environment

        private VolatileResourceManager _resourceManager = new VolatileResourceManager();
        bool _isInstanceValid;
        private bool isInstanceIdle;
        Activity _lastExecutingActivity;

        private Scheduler schedulingContext;
        private WorkflowQueuingService qService;

        private Exception thrownException;
        private string activityThrowingException;

        private List<SchedulerLockGuardInfo> eventsToFireList = new List<SchedulerLockGuardInfo>();

        internal bool stateChangedSincePersistence;

        private WorkflowInstance _workflowInstance;
        private Guid workflowInstanceId;
        private string workflowIdString = null;

        WorkflowStateRollbackService workflowStateRollbackService;

        private InstanceLock _executorLock;
        private InstanceLock _msgDeliveryLock;
        private InstanceLock _schedulerLock;
        private TimerEventSubscriptionCollection _timerQueue;
        private volatile Activity _workflowDefinition;   // dependency property cache

        private static BooleanSwitch disableWorkflowDebugging = new BooleanSwitch("DisableWorkflowDebugging", "Disables workflow debugging in host");
        private static bool workflowDebuggingDisabled;
        private WorkflowDebuggerService _workflowDebuggerService;
        #endregion Data Members

        #region Ctors

        static WorkflowExecutor()
        {
            // registered by workflow executor
            DependencyProperty.RegisterAsKnown(ContextIdProperty, (byte)51, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(IsSuspensionRequestedProperty, (byte)52, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(TrackingCallingStateProperty, (byte)53, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(TrackingListenerBrokerProperty, (byte)54, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(IsIdleProperty, (byte)56, DependencyProperty.PropertyValidity.Uninitialize);

            // registered by Scheduler
            DependencyProperty.RegisterAsKnown(Scheduler.NormalPriorityEntriesQueueProperty, (byte)61, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(Scheduler.HighPriorityEntriesQueueProperty, (byte)62, DependencyProperty.PropertyValidity.Uninitialize);

            // registered by other services
            DependencyProperty.RegisterAsKnown(WorkflowQueuingService.LocalPersistedQueueStatesProperty, (byte)63, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowQueuingService.RootPersistedQueueStatesProperty, (byte)64, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(CorrelationTokenCollection.CorrelationTokenCollectionProperty, (byte)65, DependencyProperty.PropertyValidity.Always);
            DependencyProperty.RegisterAsKnown(CorrelationToken.NameProperty, (byte)67, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.OwnerActivityNameProperty, (byte)68, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.PropertiesProperty, (byte)69, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.SubscriptionsProperty, (byte)70, DependencyProperty.PropertyValidity.Uninitialize);
            DependencyProperty.RegisterAsKnown(CorrelationToken.InitializedProperty, (byte)71, DependencyProperty.PropertyValidity.Uninitialize);

            //registered by the definition dispenser
            DependencyProperty.RegisterAsKnown(WorkflowDefinitionDispenser.WorkflowDefinitionHashCodeProperty, (byte)80, DependencyProperty.PropertyValidity.Reexecute);


            // registered by workflow instance
            DependencyProperty.RegisterAsKnown(WorkflowInstanceIdProperty, (byte)102, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(IsBlockedProperty, (byte)103, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(WorkflowStatusProperty, (byte)104, DependencyProperty.PropertyValidity.Reexecute);
            DependencyProperty.RegisterAsKnown(SuspendOrTerminateInfoProperty, (byte)105, DependencyProperty.PropertyValidity.Reexecute);

            workflowDebuggingDisabled = disableWorkflowDebugging.Enabled;
        }

        internal WorkflowExecutor(Guid instanceId)
        {
            this._isInstanceValid = false;
            this._executorLock = LockFactory.CreateWorkflowExecutorLock(instanceId);
            this._msgDeliveryLock = LockFactory.CreateWorkflowMessageDeliveryLock(instanceId);
            this.stateChangedSincePersistence = true;

            // If DisableWorkflowDebugging switch is turned off create WorkflowDebuggerService
            if (!workflowDebuggingDisabled)
                this._workflowDebuggerService = new WorkflowDebuggerService(this);
        }

        // Initialize for the root schedule
        internal void Initialize(Activity rootActivity, WorkflowExecutor invokerExec, string invokeActivityID, Guid instanceId, IDictionary<string, object> namedArguments, WorkflowInstance workflowInstance)
        {
            this.rootActivity = rootActivity;
            this.InstanceId = instanceId;

            // Set the persisted State properties
            this.rootActivity.SetValue(WorkflowExecutor.ContextIdProperty, 0);
            this.rootActivity.SetValue(WorkflowInstanceIdProperty, instanceId);
            this.WorkflowStatus = WorkflowStatus.Created;
            this.rootActivity.SetValue(Activity.ActivityExecutionContextInfoProperty, new ActivityExecutionContextInfo(this.rootActivity.QualifiedName, GetNewContextId(), instanceId, -1));
            this.rootActivity.SetValue(Activity.ActivityContextGuidProperty, instanceId);
            this.rootActivity.SetValue(WorkflowExecutor.IsIdleProperty, true);
            this.isInstanceIdle = true;

            // set workflow executor
            this.rootActivity.SetValue(WorkflowExecutor.WorkflowExecutorProperty, this);

            // initialize the root activity
            RefreshWorkflowDefinition();
            Activity workflowDefinition = this.WorkflowDefinition;
            if (workflowDefinition == null)
                throw new InvalidOperationException("workflowDefinition");

            ((IDependencyObjectAccessor)this.rootActivity).InitializeActivatingInstanceForRuntime(null, this);
            this.rootActivity.FixUpMetaProperties(workflowDefinition);
            _runtime = workflowInstance.WorkflowRuntime;

            if (invokerExec != null)
            {
                List<string> calleeBase = new List<string>();
                TrackingCallingState parentTCS = (TrackingCallingState)invokerExec.rootActivity.GetValue(WorkflowExecutor.TrackingCallingStateProperty);
                if ((parentTCS != null) && (parentTCS.CallerActivityPathProxy != null))
                {
                    foreach (string qualifiedID in parentTCS.CallerActivityPathProxy)
                        calleeBase.Add(qualifiedID);
                }
                calleeBase.Add(invokeActivityID);

                //
                // This has been exec'd by another instance
                // Set up tracking info to allow linking instances
                Debug.Assert(invokeActivityID != null && invokeActivityID.Length > 0);
                TrackingCallingState trackingCallingState = new TrackingCallingState();
                trackingCallingState.CallerActivityPathProxy = calleeBase;
                trackingCallingState.CallerWorkflowInstanceId = invokerExec.InstanceId;
                trackingCallingState.CallerContextGuid = ((ActivityExecutionContextInfo)ContextActivityUtils.ContextActivity(invokerExec.CurrentActivity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
                if (null == invokerExec.CurrentActivity.Parent)
                    trackingCallingState.CallerParentContextGuid = trackingCallingState.CallerContextGuid;
                else
                    trackingCallingState.CallerParentContextGuid = ((ActivityExecutionContextInfo)ContextActivityUtils.ContextActivity(invokerExec.CurrentActivity.Parent).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
                this.rootActivity.SetValue(WorkflowExecutor.TrackingCallingStateProperty, trackingCallingState);
            }

            _setInArgsOnCompanion(namedArguments);

            this.schedulingContext = new Scheduler(this, true);
            this._schedulerLock = LockFactory.CreateWorkflowSchedulerLock(this.InstanceId);

            qService = new WorkflowQueuingService(this);

            _workflowInstance = workflowInstance;

            TimerQueue = new TimerEventSubscriptionCollection(this, this.InstanceId);

            // register the dynamic activity
            using (new ServiceEnvironment(this.rootActivity))
            {
                using (SetCurrentActivity(this.rootActivity))
                {
                    this.RegisterDynamicActivity(this.rootActivity, false);
                }
            }
        }

        internal void RegisterWithRuntime(WorkflowRuntime workflowRuntime)
        {
            _isInstanceValid = true;
            _runtime = workflowRuntime;
            using (new ServiceEnvironment(this.rootActivity))
            {
                using (SetCurrentActivity(this.rootActivity))
                {
                    using (ActivityExecutionContext executionContext = new ActivityExecutionContext(this.rootActivity, true))
                        executionContext.InitializeActivity(this.rootActivity);
                }

                //
                // Tell the runtime that the instance is ready 
                // so that internal components can set up event subscriptions
                this._runtime.WorkflowExecutorCreated(this, false);

                //
                // Fire first events
                FireWorkflowExecutionEvent(this, WorkflowEventInternal.Creating);
            }
        }

        // Used to recreate the root schedule executor from its persisted state
        internal void Reload(Activity rootActivity, WorkflowInstance workflowInstance)
        {
            _workflowInstance = workflowInstance;
            ReloadHelper(rootActivity);
        }

        internal void ReRegisterWithRuntime(WorkflowRuntime workflowRuntime)
        {
            using (new SchedulerLockGuard(this._schedulerLock, this))
            {
                _isInstanceValid = true;
                _runtime = workflowRuntime;
                using (new ServiceEnvironment(this.rootActivity))
                {
                    this._runtime.WorkflowExecutorCreated(this, true);

                    TimerQueue.Executor = this;
                    TimerQueue.ResumeDelivery();

                    // This will get the instance running so do it last otherwise we can end up
                    // with ----s between the running workflow and deliverying timers, etc.
                    if (this.WorkflowStatus == WorkflowStatus.Running)
                        this.Scheduler.CanRun = true;

                    FireWorkflowExecutionEvent(this, WorkflowEventInternal.Loading);
                }
            }
        }

        internal void Registered(bool isActivation)
        {
            using (ScheduleWork work = new ScheduleWork(this))
            {
                this.Scheduler.ResumeIfRunnable();
            }
            if (isActivation)
                FireWorkflowExecutionEvent(this, WorkflowEventInternal.Created);
            else
                FireWorkflowExecutionEvent(this, WorkflowEventInternal.Loaded);
        }

        // Used when replacing a workflow executor. Basically we move
        // the locks from the previous executor so we guarantee that
        // everything stays locks as it is supposed to be.
        internal void Initialize(Activity rootActivity, WorkflowRuntime runtime, WorkflowExecutor previousWorkflowExecutor)
        {
            _workflowInstance = previousWorkflowExecutor.WorkflowInstance;
            ReloadHelper(rootActivity);
            // mark instance as valid now
            IsInstanceValid = true;
            _runtime = runtime;
            this._runtime.WorkflowExecutorCreated(this, true);

            TimerQueue.Executor = this;
            TimerQueue.ResumeDelivery();

            _executorLock = previousWorkflowExecutor._executorLock;
            _msgDeliveryLock = previousWorkflowExecutor._msgDeliveryLock;
            _schedulerLock = previousWorkflowExecutor._schedulerLock;
            ScheduleWork.Executor = this;
        }

        // Used to recreate the root schedule executor from its persisted state
        private void ReloadHelper(Activity rootActivity)
        {
            // assign activity state
            this.rootActivity = rootActivity;
            this.InstanceId = (Guid)rootActivity.GetValue(WorkflowInstanceIdProperty);

            // set workflow executor
            this.rootActivity.SetValue(WorkflowExecutor.WorkflowExecutorProperty, this);

            this._schedulerLock = LockFactory.CreateWorkflowSchedulerLock(this.InstanceId);

            this.schedulingContext = new Scheduler(this, false);
            this.qService = new WorkflowQueuingService(this);

            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Loading instance {0}", this.InstanceIdString);
            DiagnosticStackTrace("load request");

            using (new ServiceEnvironment(this.rootActivity))
            {

                // check if this instance can be loaded
                switch (this.WorkflowStatus)
                {
                    case WorkflowStatus.Completed:
                    case WorkflowStatus.Terminated:
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: attempt to load a completed/terminated instance: {0}", this.InstanceIdString);
                        throw new InvalidOperationException(
                            ExecutionStringManager.InvalidAttemptToLoad);

                    default:
                        break;
                }

                // new nonSerialized members
                _resourceManager = new VolatileResourceManager();
                _runtime = _workflowInstance.WorkflowRuntime;

                // register all dynamic activities for loading
                Queue<Activity> dynamicActivitiesQueue = new Queue<Activity>();
                dynamicActivitiesQueue.Enqueue(this.rootActivity);
                while (dynamicActivitiesQueue.Count > 0)
                {
                    Activity dynamicActivity = dynamicActivitiesQueue.Dequeue();
                    ((IDependencyObjectAccessor)dynamicActivity).InitializeInstanceForRuntime(this);
                    this.RegisterDynamicActivity(dynamicActivity, true);

                    IList<Activity> nestedDynamicActivities = (IList<Activity>)dynamicActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                    if (nestedDynamicActivities != null)
                    {
                        foreach (Activity nestedDynamicActivity in nestedDynamicActivities)
                            dynamicActivitiesQueue.Enqueue(nestedDynamicActivity);
                    }
                }
            }

            this.isInstanceIdle = (bool)this.rootActivity.GetValue(IsIdleProperty);
            RefreshWorkflowDefinition();
        }

        private void _setInArgsOnCompanion(IDictionary<string, object> namedInArguments)
        {
            // Do parameter property assignments.
            if (namedInArguments != null)
            {
                foreach (string arg in namedInArguments.Keys)
                {

                    PropertyInfo propertyInfo = this.WorkflowDefinition.GetType().GetProperty(arg);

                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        try
                        {
                            propertyInfo.SetValue(this.rootActivity, namedInArguments[arg], null);
                        }
                        catch (ArgumentException e)
                        {
                            throw new ArgumentException(ExecutionStringManager.InvalidWorkflowParameterValue, arg, e);
                        }
                    }
                    else
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.SemanticErrorInvalidNamedParameter, ((Activity)this.WorkflowDefinition).Name, arg));
                }
            }
        }
        #endregion Ctors

        #region Misc properties and methods

        internal TrackingCallingState TrackingCallingState
        {
            get
            {
                return (TrackingCallingState)this.rootActivity.GetValue(WorkflowExecutor.TrackingCallingStateProperty);
            }
        }

        internal WorkflowRuntime WorkflowRuntime
        {
            get
            {
                return _runtime;
            }
        }
        internal bool IsInstanceValid
        {
            get { return _isInstanceValid; }
            set
            {
                if (!value)
                {
                    this.ResourceManager.ClearAllBatchedWork();
                    InstanceLock.AssertIsLocked(this._schedulerLock);
                    InstanceLock.AssertIsLocked(this._msgDeliveryLock);
                }
                _isInstanceValid = value;
            }
        }

        internal bool IsIdle
        {
            get
            {
                return this.isInstanceIdle;
            }
            set
            {
                using (InstanceLock.InstanceLockGuard messageDeliveryLockGuard = this.MessageDeliveryLock.Enter())
                {
                    try
                    {
                        this.isInstanceIdle = value;
                        this.RootActivity.SetValue(WorkflowExecutor.IsIdleProperty, value);
                    }
                    finally
                    {
                        // Playing it safe here. If the try block throws, 
                        // we test what was the resulting value of the
                        // property to see if we need to signal the
                        // waiting threads
                        if (this.IsIdle)
                            messageDeliveryLockGuard.Pulse();
                    }
                }
            }
        }

        internal string AdditionalInformation
        {
            get { return (string)this.rootActivity.GetValue(SuspendOrTerminateInfoProperty); }
        }

        public WorkBatchCollection BatchCollection
        {
            get
            {
                return _resourceManager.BatchCollection;
            }
        }

        internal VolatileResourceManager ResourceManager
        {
            get
            {
                return _resourceManager;
            }
        }

        internal Activity WorkflowDefinition
        {
            get
            {
                Debug.Assert(_workflowDefinition != null, "WorkflowDefinition cannot be null.");
                return _workflowDefinition;
            }
        }

        private void RefreshWorkflowDefinition()
        {
            Activity tempDefinition = (Activity)this.rootActivity.GetValue(Activity.WorkflowDefinitionProperty);
            Debug.Assert(tempDefinition != null, "WorkflowDefinition cannot be null.");

            // Workflow definitions needs to have a locking object
            // on them for use when cloning for public consumption
            // (WorkflowInstance.GetWorkflowDefinition and 
            // WorkflowCompletedEventArgs.WorkflowDefinition).
            WorkflowDefinitionLock.SetWorkflowDefinitionLockObject(tempDefinition, new object());

            _workflowDefinition = tempDefinition;
        }

        internal Activity RootActivity
        {
            get
            {
                return this.rootActivity;
            }
        }

        internal Guid InstanceId
        {
            get
            {
                return workflowInstanceId;
            }
            private set
            {
                workflowInstanceId = value;
            }
        }

        internal string InstanceIdString
        {
            get
            {
                if (workflowIdString == null)
                    workflowIdString = this.InstanceId.ToString();
                return workflowIdString;
            }
        }


        internal InstanceLock MessageDeliveryLock
        {
            get
            {
                return _msgDeliveryLock;
            }
        }

        internal InstanceLock ExecutorLock
        {
            get
            {
                return _executorLock;
            }
        }

        internal WorkflowStateRollbackService WorkflowStateRollbackService
        {
            get
            {
                if (this.workflowStateRollbackService == null)
                    this.workflowStateRollbackService = new WorkflowStateRollbackService(this);
                return this.workflowStateRollbackService;
            }
        }

        internal WorkflowInstance WorkflowInstance
        {
            get
            {
                System.Diagnostics.Debug.Assert(this._workflowInstance != null, "WorkflowInstance property should not be called before the proxy is initialized.");
                return this._workflowInstance;
            }
        }

        internal void Start()
        {
            using (ScheduleWork work = new ScheduleWork(this))
            {
                using (this.ExecutorLock.Enter())
                {
                    if (this.WorkflowStatus != WorkflowStatus.Created)
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CannotStartInstanceTwice, this.InstanceId));

                    // Set a new ServiceEnvironment to establish a current batch in TLS
                    // This is needed for synchronous status change notification at start
                    // (status init->executing) when there is no batch in TLS yet
                    // and there are subscribers like tracking
                    this.WorkflowStatus = WorkflowStatus.Running;
                    using (new ServiceEnvironment(this.rootActivity))
                    {
                        FireWorkflowExecutionEvent(this, WorkflowEventInternal.Starting);
                        try
                        {
                            using (ActivityExecutionContext executionContext = new ActivityExecutionContext(this.rootActivity, true))
                            {
                                // make sure the scheduler is able to run
                                this.schedulingContext.CanRun = true;

                                // Since we are actually scheduling work at this point, we should grab
                                // the scheduler lock. This will avoid ----s some operations we schedule
                                // start executing before we are done scheduling all operations.
                                using (new SchedulerLockGuard(this._schedulerLock, this))
                                {
                                    executionContext.ExecuteActivity(this.rootActivity);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Terminate(e.Message);
                            throw;
                        }
                        FireWorkflowExecutionEvent(this, WorkflowEventInternal.Started);

                    }
                }
            }
        }

        internal Activity CurrentActivity
        {
            get { return _lastExecutingActivity; }
            set { _lastExecutingActivity = value; }
        }

        internal Hashtable CompletedContextActivities
        {
            get { return this.completedContextActivities; }
            set { this.completedContextActivities = value; }
        }


        private int GetNewContextId()
        {
            int conextId = (int)this.rootActivity.GetValue(WorkflowExecutor.ContextIdProperty) + 1;
            this.rootActivity.SetValue(WorkflowExecutor.ContextIdProperty, conextId);
            return conextId;
        }

        internal List<SchedulerLockGuardInfo> EventsToFireList
        {
            get
            {
                return eventsToFireList;
            }
        }

        private void FireEventAfterSchedulerLockDrop(WorkflowEventInternal workflowEventInternal, object eventInfo)
        {
            eventsToFireList.Add(new SchedulerLockGuardInfo(this, workflowEventInternal, eventInfo));
        }

        private void FireEventAfterSchedulerLockDrop(WorkflowEventInternal workflowEventInternal)
        {
            eventsToFireList.Add(new SchedulerLockGuardInfo(this, workflowEventInternal));
        }

        #endregion Misc properties and methods

        #region Scheduler Related

        // asks the hosting env threadProvider for a thread
        internal void ScheduleForWork()
        {
            this.IsIdle = false;

            if (this.IsInstanceValid)
                FireWorkflowExecutionEvent(this, WorkflowEventInternal.Runnable);

            ScheduleWork.NeedsService = true;
        }

        internal void RequestHostingService()
        {
            WorkflowRuntime.SchedulerService.Schedule(this.RunSome, this.InstanceId);
        }

        internal void DeliverTimerSubscriptions()
        {
            using (ScheduleWork work = new ScheduleWork(this))
            {
                using (this._executorLock.Enter())
                {
                    if (this.IsInstanceValid)
                    {
                        using (this.MessageDeliveryLock.Enter())
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                if (!this.IsInstanceValid)
                                    return;

                                TimerEventSubscriptionCollection queue = TimerQueue;
                                bool done = false;
                                while (!done)
                                {
                                    lock (queue.SyncRoot)
                                    {
                                        TimerEventSubscription sub = queue.Peek();
                                        if (sub == null || sub.ExpiresAt > DateTime.UtcNow)
                                        {
                                            done = true;
                                        }
                                        else
                                        {
                                            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Delivering timer subscription for instance {0}", this.InstanceIdString);
                                            stateChangedSincePersistence = true;
                                            lock (qService.SyncRoot)
                                            {
                                                if (qService.Exists(sub.QueueName))
                                                {
                                                    qService.EnqueueEvent(sub.QueueName, sub.SubscriptionId);
                                                }
                                            }
                                            queue.Dequeue();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // call from the threadProvider about the availability of a thread.
        internal void RunSome(object ignored)
        {
            using (ScheduleWork work = new ScheduleWork(this))
            {
                using (new WorkflowTraceTransfer(this.InstanceId))
                {
                    using (new SchedulerLockGuard(this._schedulerLock, this))
                    {
                        using (new ServiceEnvironment(this.rootActivity))
                        {
                            // check if this is a valid in-memory instance
                            if (!this.IsInstanceValid)
                                return;

                            // check if instance already done
                            if ((this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Closed) || (WorkflowStatus.Completed == this.WorkflowStatus) || (WorkflowStatus.Terminated == this.WorkflowStatus))
                                return;

                            bool ignoreFinallyBlock = false;

                            //
                            // For V1 we don't support flow through transaction on the service thread
                            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                            {
                                try
                                {
                                    FireWorkflowExecutionEvent(this, WorkflowEventInternal.Executing);
                                    // run away ... run away...
                                    this.RunScheduler();
                                }
                                catch (Exception e)
                                {
                                    if (WorkflowExecutor.IsIrrecoverableException(e))
                                    {
                                        ignoreFinallyBlock = true;
                                        throw;
                                    }
                                    else
                                    {
                                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Fatal exception thrown in the scheduler. Terminating the workflow instance '{0}'. Exception:{1}\n{2}", this.InstanceIdString, e.Message, e.StackTrace);
                                        this.TerminateOnIdle(WorkflowExecutor.GetNestedExceptionMessage(e));
                                        this.ThrownException = e;
                                    }
                                }
                                finally
                                {
                                    if (!ignoreFinallyBlock)
                                    {
                                        FireWorkflowExecutionEvent(this, WorkflowEventInternal.NotExecuting);
                                    }
                                }
                                scope.Complete();
                            }
                        }
                    }
                }
            }
        }

        // this method is called with the scheduler lock held
        private void RunScheduler()
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);

            // run away ... run away...
            try
            {
                this.Scheduler.Run();
            }
            finally
            {
                this.IsIdle = true;
            }

            if (!this.IsInstanceValid)
                return;

            if (this.WorkflowStateRollbackService.IsInstanceStateRevertRequested)
            {
                //
                // Protect against message delivery while reverting
                using (MessageDeliveryLock.Enter())
                {
                    this.WorkflowStateRollbackService.RevertToCheckpointState();
                    return;
                }
            }

            if (this.Scheduler.IsStalledNow)
            {
                // the instance has no ready work

                // Protect against the host accessing DPs.
                using (this.MessageDeliveryLock.Enter())
                {
                    if (this.rootActivity.ExecutionStatus != ActivityExecutionStatus.Closed)
                    {
                        this.ProcessQueuedEvents(); // deliver any outstanding queued events before persisting
                        if (this.Scheduler.IsStalledNow)
                        {
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: workflow instance '{0}' has no work.", this.InstanceIdString);
                            FireWorkflowExecutionEvent(this, WorkflowEventInternal.SchedulerEmpty);

                            FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Idle);

                            WorkflowPersistenceService persistence = this.WorkflowRuntime.WorkflowPersistenceService;

                            // instance is not done.. must be idle
                            // can potentially dehydrate now..
                            if ((persistence != null) && persistence.UnloadOnIdle(this.rootActivity))
                            {
                                if (!this.IsInstanceValid)
                                    return;

                                // Do not unload if we are not unloadable and if a persistence exception
                                // was thrown the last time.
                                if (this.IsUnloadableNow && !(this.ThrownException is PersistenceException))
                                {
                                    PerformUnloading(true);
                                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "WorkflowExecutor: unloaded workflow instance '{0}'.  IsInstanceValid={1}", this.InstanceIdString, IsInstanceValid);
                                }
                            }
                            else
                            {
                                if (this.ResourceManager.IsBatchDirty && this.currentAtomicActivity == null)
                                {
                                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: workflow instance '{0}' has no work and the batch is dirty. Persisting state and commiting batch.", this.InstanceIdString);
                                    this.Persist(this.rootActivity, false, false);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // the instance has ready work but was told to stop
                //

                // if suspension was requested, suspend now.

                if ((bool)this.rootActivity.GetValue(WorkflowExecutor.IsSuspensionRequestedProperty))
                {
                    this.SuspendOnIdle(this.AdditionalInformation);
                    this.rootActivity.SetValue(WorkflowExecutor.IsSuspensionRequestedProperty, false);
                }
            }

            if (this.currentAtomicActivity != null)
            {
                // Leave TransactionScope before giving up the thread                
                TransactionalProperties transactionalProperties = (TransactionalProperties)this.currentAtomicActivity.GetValue(TransactionalPropertiesProperty);
                DisposeTransactionScope(transactionalProperties);
            }
        }

        private bool attemptedRootAECUnload = false;
        private bool attemptedRootDispose = false;

        private void DisposeRootActivity(bool aborting)
        {
            try
            {
                if (!attemptedRootAECUnload)
                {
                    attemptedRootAECUnload = true;
                    this.RootActivity.OnActivityExecutionContextUnload(this);
                }
                if (!attemptedRootDispose)
                {
                    attemptedRootDispose = true;
                    this.RootActivity.Dispose();
                }
            }
            catch (Exception)
            {
                if (!aborting)
                {
                    using (_msgDeliveryLock.Enter())
                    {
                        this.AbortOnIdle();
                        throw;
                    }
                }
            }
        }


        internal Scheduler Scheduler
        {
            get
            {
                return this.schedulingContext;
            }
        }

        #endregion Scheduler Related

        #region IInstanceState

        /// <summary>
        /// Instance Id
        /// </summary>
        /// <value></value>
        internal Guid ID
        {
            get { return InstanceId; }
        }

        /// <summary>
        /// Completed status for instances clean up
        /// </summary>
        /// <value></value>        
        internal WorkflowStatus WorkflowStatus
        {
            get { return (WorkflowStatus)this.rootActivity.GetValue(WorkflowStatusProperty); }
            private set { this.rootActivity.SetValue(WorkflowStatusProperty, value); }
        }

        internal TimerEventSubscriptionCollection TimerQueue
        {
            get
            {
                if (_timerQueue == null)
                {
                    _timerQueue = (TimerEventSubscriptionCollection)this.rootActivity.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
                    Debug.Assert(_timerQueue != null, "TimerEventSubscriptionCollection on root activity should never be null, but it was");
                }
                return _timerQueue;
            }
            private set
            {
                _timerQueue = value;
                this.rootActivity.SetValue(TimerEventSubscriptionCollection.TimerCollectionProperty, _timerQueue);
            }
        }


        #endregion

        #region Persistence

        private bool ProtectedPersist(bool unlock)
        {
            try
            {
                // persist
                this.Persist(this.rootActivity, unlock, false);
            }
            catch (Exception e)
            {
                if (WorkflowExecutor.IsIrrecoverableException(e))
                {
                    throw;
                } //@@undone: for Microsoft:- we should not be running exception handler, when we are unlocking.
                else if (this.WorkflowStatus != WorkflowStatus.Suspended && this.IsInstanceValid)
                {
                    // the persistence attempt threw an exception
                    // lets give this exception to a scope
                    Activity activity = FindExecutorToHandleException();

                    this.Scheduler.CanRun = true;
                    this.ExceptionOccured(e, activity, null);
                }
                else
                {
                    if (this.TerminateOnIdle(WorkflowExecutor.GetNestedExceptionMessage(e)))
                    {
                        this.stateChangedSincePersistence = true;
                        this.WorkflowStatus = WorkflowStatus.Terminated;
                    }
                }
                return false;
            }
            return true;
        }

        private Activity FindExecutorToHandleException()
        {
            Activity lastExecutingActivity = this.CurrentActivity;
            if (lastExecutingActivity == null)
                lastExecutingActivity = this.rootActivity;
            return lastExecutingActivity;
        }

        // called by core runtime to persist the instance.
        // 'exec' is the executor requesting the persistence
        internal void Persist(Activity dynamicActivity, bool unlock, bool needsCompensation)
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);
            Activity currentActivity = (this.CurrentActivity == null) ? dynamicActivity : this.CurrentActivity;
            //
            // Save the current status.  The status may change in PrePersist
            // and we need to reset if the commit fails for any reason.
            WorkflowStatus oldStatus = this.WorkflowStatus;

            // New a ServiceEnvironment to set the current batch to be of the exec to be persisted 
            using (new ServiceEnvironment(currentActivity))
            {
                try
                {
                    // prevent the message delivery from outside
                    using (this.MessageDeliveryLock.Enter())
                    {
                        this.ProcessQueuedEvents(); // Must always process this queue before persisting state!
                        // check what has changed since last persist
                        //
                        if (this.ResourceManager.IsBatchDirty)
                        {
                            // if there is work in the batch, persist the state to be consistent
                            this.stateChangedSincePersistence = true;
                        }
                        else
                        {
                            // no work in the batch...
                            if (!this.stateChangedSincePersistence && !unlock)
                            {
                                // the instance state is not dirty
                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: NOT Persisting Instance '{0}' since the batch is NOT dirty and the instance state is NOT dirty", this.InstanceIdString);
                                return;
                            }
                        }

                        // prepare the state for persistence
                        //
                        this.PrePersist();

                        if (WorkflowStatus.Completed == WorkflowStatus)
                        {
                            // Any remaining messages in queues are zombie messages so move all to the pending queue
                            this.qService.MoveAllMessagesToPendingQueue();
                        }
                        // give the state to the persistence provider
                        WorkflowPersistenceService persistence = this.WorkflowRuntime.WorkflowPersistenceService;

                        // Create a transient batch for Persistence Service.
                        currentActivity.SetValue(TransientBatchProperty, _resourceManager.BatchCollection.GetTransientBatch());

                        bool firedPersistingEvent = false;

                        if (persistence != null)
                        {
                            foreach (Activity completedContextActivity in this.completedContextActivities.Values)
                            {
                                // Save the committing activity 
                                completedContextActivity.SetValue(WorkflowInstanceIdProperty, this.InstanceId);

                                if (!firedPersistingEvent)
                                {
                                    FireWorkflowExecutionEvent(this, WorkflowEventInternal.Persisting);
                                    firedPersistingEvent = true;
                                }

                                persistence.SaveCompletedContextActivity(completedContextActivity);
                                completedContextActivity.Dispose();
                            }

                            if (this.stateChangedSincePersistence)
                            {
                                if (!firedPersistingEvent)
                                {
                                    FireWorkflowExecutionEvent(this, WorkflowEventInternal.Persisting);
                                    firedPersistingEvent = true;
                                }

                                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Calling SaveWorkflowInstanceState for instance {0} hc {1}", this.InstanceIdString, this.GetHashCode());
                                persistence.SaveWorkflowInstanceState(this.rootActivity, unlock);
                            }
                            else if (unlock)
                            {
                                persistence.UnlockWorkflowInstanceState(this.rootActivity);
                            }
                        }

                        if (unlock)
                        {
                            DisposeRootActivity(false);
                        }

                        // commit 
                        // check batch again, since the persistence provider may have added something.
                        // If we are unlocking (unloading/dehydrating) commit the batch
                        // regardless of whether the batch items signal that they need a commit
                        if (this.currentAtomicActivity != null || this.ResourceManager.IsBatchDirty || (unlock && HasNonEmptyWorkBatch()))
                        {

                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Calling CommitTransaction for instance {0} hc {1}", this.InstanceIdString, this.GetHashCode());
                            this.CommitTransaction(currentActivity);
                        }

                        if (firedPersistingEvent)
                            FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Persisted);

                        // post-persist
                        //
                        this.stateChangedSincePersistence = false;
                        this.PostPersist();
                        //
                        // Must do this after all persist related work has successfully finished
                        // If we weren't successful we aren't actually completed
                        if (WorkflowStatus.Completed == WorkflowStatus)
                        {
                            FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Completed);
                            this.IsInstanceValid = false;
                        }
                    }
                }
                catch (PersistenceException e)
                {
                    this.Rollback(oldStatus);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Persist attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                    throw;
                }
                catch (Exception e)
                {
                    if (WorkflowExecutor.IsIrrecoverableException(e))
                    {
                        throw;
                    }
                    this.Rollback(oldStatus);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Persist attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                    throw new PersistenceException(e.Message, e);
                }
                finally
                {
                    //Flush the transient Batch
                    currentActivity.SetValue(TransientBatchProperty, null);
                }
            }
        }

        /// <summary>
        /// There is always at least 1 BatchCollection (at root),
        /// check if any batch contains any work item
        /// </summary>
        /// <returns></returns>
        private bool HasNonEmptyWorkBatch()
        {
            foreach (WorkBatch workBatch in ResourceManager.BatchCollection.Values)
            {
                if (workBatch.Count > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// PrePersist
        /// 
        /// Signal to prepare the state for persistence.
        /// </summary>
        private void PrePersist()
        {
            //
            // This is our hook to set the workflowstatus to Completed
            // so that it is correctly written to persistence
            WorkflowStatus workflowStatus = this.WorkflowStatus;
            if ((ActivityExecutionStatus.Closed == this.rootActivity.ExecutionStatus) && (WorkflowStatus.Terminated != workflowStatus))
            {
                FireWorkflowExecutionEvent(this, WorkflowEventInternal.Completing);
                this.WorkflowStatus = WorkflowStatus.Completed;
            }

            switch (this.WorkflowStatus)
            {
                case WorkflowStatus.Running:
                    this.rootActivity.SetValue(IsBlockedProperty, this.Scheduler.IsStalledNow);
                    break;
                case WorkflowStatus.Suspended:
                case WorkflowStatus.Completed:
                case WorkflowStatus.Terminated:
                case WorkflowStatus.Created:
                    this.rootActivity.SetValue(IsBlockedProperty, false);
                    break;
                default:
                    Debug.Assert(false, "Unknown WorkflowStatus");
                    break;
            }

            qService.PrePersist();
        }

        private void PostPersist()
        {
            qService.PostPersist(true);
            if (this.Scheduler != null)
                this.Scheduler.PostPersist();
            this.completedContextActivities.Clear();
        }

        private void Rollback(WorkflowStatus oldStatus)
        {
            this.WorkflowStatus = oldStatus;

            if (this.Scheduler != null)
                this.Scheduler.Rollback();
        }

        #endregion

        #region MessageArrival and Query

        internal void ProcessQueuedEvents()
        {
            using (MessageDeliveryLock.Enter())
            {
                qService.ProcessesQueuedAsynchronousEvents();
            }
        }

        internal void EnqueueItem(IComparable queueName, object item, IPendingWork pendingWork, Object workItem)
        {
            using (ScheduleWork work = new ScheduleWork(this))
            {
                bool lockedScheduler = false;
                if (!ServiceEnvironment.IsInServiceThread(InstanceId))
                    lockedScheduler = _schedulerLock.TryEnter();
                try
                {
                    // take the msg delivery lock to make sure the instance
                    // doesn't persist while the message is being delivered.
                    using (this.MessageDeliveryLock.Enter())
                    {
                        if (!this.IsInstanceValid)
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                        if (lockedScheduler || ServiceEnvironment.IsInServiceThread(InstanceId))
                        {
                            using (new ServiceEnvironment(this.RootActivity))
                            {
                                qService.EnqueueEvent(queueName, item);
                            }
                        }
                        else
                        {
                            if (qService.SafeEnqueueEvent(queueName, item))
                            {
                                ScheduleWork.NeedsService = true;
                            }
                        }

                        // add work items to the current batch if exists
                        if (pendingWork != null)
                        {
                            IWorkBatch batch = _resourceManager.BatchCollection.GetBatch(this.rootActivity);
                            batch.Add(pendingWork, workItem);
                        }

                        stateChangedSincePersistence = true;
                    }
                }
                finally
                {
                    if (lockedScheduler)
                        _schedulerLock.Exit();
                }
            }
        }

        internal void EnqueueItemOnIdle(IComparable queueName, object item, IPendingWork pendingWork, Object workItem)
        {
            using (ScheduleWork work = new ScheduleWork(this))
            {
                // prevent other control operations from outside
                using (this._executorLock.Enter())
                {
                    if (!this.IsInstanceValid)
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                    // take the msg delivery lock to make sure the instance
                    // doesn't persist while the message is being delivered.
                    using (InstanceLock.InstanceLockGuard messageDeliveryLockGuard = this.MessageDeliveryLock.Enter())
                    {
                        using (new ServiceEnvironment(this.rootActivity))
                        {

                            if (!this.IsInstanceValid)
                                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                            // Wait until the Scheduler is idle.
                            while (!this.IsIdle)
                            {
                                messageDeliveryLockGuard.Wait();
                                if (!this.IsInstanceValid)
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                            }

                            // At this point the scheduler is not running and it is 
                            // EnqueueItemOnIdle is not valid for suspended workflows
                            if ((this.WorkflowStatus == WorkflowStatus.Suspended) || (!this.Scheduler.CanRun))
                                throw new InvalidOperationException(ExecutionStringManager.InvalidWaitForIdleOnSuspendedWorkflow);

                            try
                            {
                                // add work items to the current batch if exists
                                if (pendingWork != null)
                                {
                                    IWorkBatch batch = (IWorkBatch)this.rootActivity.GetValue(WorkflowExecutor.TransientBatchProperty);
                                    batch.Add(pendingWork, workItem);
                                }

                                stateChangedSincePersistence = true;
                                qService.EnqueueEvent(queueName, item);
                            }
                            finally
                            {
                                if (this.IsIdle)
                                    messageDeliveryLockGuard.Pulse();
                            }
                        }
                    }
                }
            }
        }

        internal ReadOnlyCollection<WorkflowQueueInfo> GetWorkflowQueueInfos()
        {
            List<WorkflowQueueInfo> queuedItems = new List<WorkflowQueueInfo>();
            // take the msg delivery lock to make sure the queues don't
            // change during the list assembly.
            using (this.MessageDeliveryLock.Enter())
            {
                using (new ServiceEnvironment(this.rootActivity))
                {
                    lock (qService.SyncRoot)
                    {
                        IEnumerable<IComparable> names = qService.QueueNames;
                        foreach (IComparable name in names)
                        {
                            try
                            {
                                WorkflowQueue queue = qService.GetWorkflowQueue(name);
                                if (!queue.Enabled)
                                    continue;
                                Queue items = qService.GetQueue(name).Messages;
                                List<ActivityExecutorDelegateInfo<QueueEventArgs>> listeners = qService.GetQueue(name).AsynchronousListeners;
                                List<string> subscribedActivities = new List<string>();
                                foreach (ActivityExecutorDelegateInfo<QueueEventArgs> l in listeners)
                                {
                                    string activity = (l.SubscribedActivityQualifiedName == null) ? l.ActivityQualifiedName : l.SubscribedActivityQualifiedName;
                                    subscribedActivities.Add(activity);
                                }
                                queuedItems.Add(new WorkflowQueueInfo(name, items, subscribedActivities.AsReadOnly()));
                            }
                            catch (InvalidOperationException)
                            {
                                // ignore this queue if it has disappeared
                            }
                        }
                    }
                }
            }
            return queuedItems.AsReadOnly();
        }

        internal DateTime GetWorkflowNextTimerExpiration()
        {
            using (this._executorLock.Enter())
            {
                using (this.MessageDeliveryLock.Enter())
                {
                    TimerEventSubscriptionCollection timers = TimerQueue;
                    TimerEventSubscription sub = timers.Peek();
                    return sub == null ? DateTime.MaxValue : sub.ExpiresAt;
                }
            }
        }

        #endregion MessageArrival and Query

        #region executor to execution context mappings

        //This list is populated at loading time.
        //a map of SubState Tracking Context - SubState.
        [NonSerialized]
        private Dictionary<int, Activity> subStateMap = new Dictionary<int, Activity>();

        internal void RegisterDynamicActivity(Activity dynamicActivity, bool load)
        {
            int contextId = ContextActivityUtils.ContextId(dynamicActivity);
            this.subStateMap.Add(contextId, dynamicActivity);

            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(
                TraceEventType.Information, 0, "Adding context {0}:{1}",
                contextId, dynamicActivity.QualifiedName + (load ? " for load" : ""));

            dynamicActivity.OnActivityExecutionContextLoad(this);
        }

        internal void UnregisterDynamicActivity(Activity dynamicActivity)
        {
            int contextId = ContextActivityUtils.ContextId(dynamicActivity);
            this.subStateMap.Remove(contextId);

            System.Workflow.Runtime.WorkflowTrace.Runtime.TraceEvent(
                TraceEventType.Information, 0, "Removing context {0}:{1}",
                contextId, dynamicActivity.QualifiedName);

            dynamicActivity.OnActivityExecutionContextUnload(this);
        }

        internal Activity GetContextActivityForId(int stateId)
        {
            if (this.subStateMap.ContainsKey(stateId))
                return this.subStateMap[stateId];
            return null;
        }

        #endregion

        #region Unloading
        // indicates whether an this schedule instance can be unloaded right now
        internal bool IsUnloadableNow
        {
            // Called by hosting environment
            get { return ((this.currentAtomicActivity == null) && (this.Scheduler.IsStalledNow || this.WorkflowStatus == WorkflowStatus.Suspended)); }
        }

        /// <summary>
        /// Synchronously unload if currently idle
        /// </summary>
        /// <returns>true if successful</returns>
        internal bool TryUnload()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a TryUnload request for instance {0}", this.InstanceIdString);
            DiagnosticStackTrace("try unload request");

            try
            {
                // check if this is a valid in-memory instance
                if (!this.IsInstanceValid)
                    return false;

                // check if there is a persistence service
                if (this.WorkflowRuntime.WorkflowPersistenceService == null)
                {
                    string errMsg = String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, this.InstanceId);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, errMsg);
                    throw new InvalidOperationException(errMsg);
                }
                using (new ScheduleWork(this, true))
                {
                    // Stop threads from outside - message delivery and control operations
                    if (this._executorLock.TryEnter())
                    {
                        try
                        {
                            // we need to take these locks to make sure that we have a fixed picture of the 
                            // unloadability state of the workflow.
                            if (this._schedulerLock.TryEnter())
                            {
                                try
                                {
                                    if (this._msgDeliveryLock.TryEnter())
                                    {
                                        using (new ServiceEnvironment(this.rootActivity))
                                        {
                                            try
                                            {
                                                if (!this.IsInstanceValid)
                                                    return false;

                                                this.ProcessQueuedEvents(); // deliver any outstanding queued events before persisting
                                                if (this.IsUnloadableNow)
                                                {
                                                    // can unload now
                                                    return PerformUnloading(false);
                                                }
                                                else
                                                    return false;
                                            }
                                            finally
                                            {
                                                this._msgDeliveryLock.Exit();
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    SchedulerLockGuard.Exit(this._schedulerLock, this);
                                }
                            }
                        }
                        finally
                        {
                            this._executorLock.Exit();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: TryUnloading attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
            return false;
        }

        // this unloads the instance by assuming that it can be unloaded.
        private bool PerformUnloading(bool handleExceptions)
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);

            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Unloading instance {0}", this.InstanceIdString);
            DiagnosticStackTrace("unload request");

            FireWorkflowExecutionEvent(this, WorkflowEventInternal.Unloading);
            //
            // Block message delivery for duration of persist and marking as invalid
            using (_msgDeliveryLock.Enter())
            {
                TimerQueue.SuspendDelivery();

                bool persisted;
                if (handleExceptions)
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, InstanceId + ": Calling PerformUnloading(false): InstanceId {0}, hc: {1}", InstanceIdString, this.GetHashCode());
                    persisted = this.ProtectedPersist(true);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, InstanceId + ": Returning from ProtectedPersist: InstanceId {0}, hc: {1}, ret={2}", InstanceIdString, this.GetHashCode(), persisted);
                }
                else
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, InstanceId + ": Calling Persist");
                    this.Persist(this.rootActivity, true, false);
                    persisted = true;
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, InstanceId + ": Returning from Persist: InstanceId {0}, hc: {1}, IsInstanceValid={2}", InstanceIdString, this.GetHashCode(), IsInstanceValid);
                }
                if (persisted)
                {
                    // mark instance as invalid
                    this.IsInstanceValid = false;

                    FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Unloaded);
                    return true;
                }
                else
                    return false;
            }
        }

        // shutsdown the schedule instance sync
        internal void Unload()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got an unload request for instance {0}", this.InstanceIdString);
            DiagnosticStackTrace("unload request");

            try
            {
                using (new ScheduleWork(this, true))
                {
                    // Stop threads from outside - message delivery and control operations
                    using (this._executorLock.Enter())
                    {
                        if (this.WorkflowRuntime.WorkflowPersistenceService == null)
                        {
                            string errMsg = String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, this.InstanceId);
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, errMsg);
                            throw new InvalidOperationException(errMsg);
                        }

                        // tell the scheduler to stop running
                        this.Scheduler.CanRun = false;
                        // If there were some thread executing the instance, then setting up
                        // the callback, the thread getting done and the notification coming back
                        // is racy... so we lock the scheduler
                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                // check if this is a valid in-memory instance
                                if (!this.IsInstanceValid)
                                {
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                                }

                                // the scheduler must be idle now
                                if (this.currentAtomicActivity == null)
                                {
                                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, InstanceId + ": Calling PerformUnloading(false) on instance {0} hc {1}", InstanceIdString, this.GetHashCode());
                                    // unload
                                    PerformUnloading(false);
                                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, InstanceId + ": Returning from PerformUnloading(false): IsInstanceValue: " + IsInstanceValid);
                                }
                                else
                                {
                                    this.Scheduler.CanRun = true;
                                    throw new ExecutorLocksHeldException(atomicActivityEvent);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Unload attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
        }

        #endregion

        #region Terminate

        // terminates the schedule instance sync
        // must be called only from outside the instance... the thread running the instance must
        // never call this method... it should call TerminateOnIdle instead.
        internal void Terminate(string error)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor::Terminate : Got a terminate request for instance {0}", this.InstanceIdString);

            try
            {
                using (new ScheduleWork(this, true))
                {
                    // Stop threads from outside - message delivery and control operations
                    using (this._executorLock.Enter())
                    {
                        // tell the scheduler to stop returnig items from its queue (ref: 16534)
                        this.Scheduler.AbortOrTerminateRequested = true;
                        // tell the scheduler to stop running
                        this.Scheduler.CanRun = false;

                        // If there were some thread executing the instance, then setting up
                        // the callback, the thread getting done and the notification coming back
                        // is racy... so we lock the scheduler
                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {

                                // check if this is a valid in-memory instance
                                if (!this.IsInstanceValid)
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                                this.TerminateOnIdle(error);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Terminate attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
        }

        // this method must be called with the scheduler lock held
        internal bool TerminateOnIdle(string error)
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);

            // check if the instance can be terminated
            if (!this.IsInstanceValid)
                return false;

            // tell the scheduler to stop running
            this.Scheduler.CanRun = false;

            try
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Terminating instance {0}", this.InstanceIdString);

                if (null != ThrownException)
                    FireWorkflowTerminating(ThrownException);
                else
                    FireWorkflowTerminating(error);


                // mark instance as canceled
                this.stateChangedSincePersistence = true;
                WorkflowStatus oldStatus = this.WorkflowStatus;
                this.rootActivity.SetValue(SuspendOrTerminateInfoProperty, error);
                this.WorkflowStatus = WorkflowStatus.Terminated;
                //
                // Block message delivery for duration of persistence and marking as invalid instance
                using (_msgDeliveryLock.Enter())
                {
                    TimerQueue.SuspendDelivery();
                    this.rootActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Canceled);
                    try
                    {
                        // persist the instance state
                        this.Persist(this.rootActivity, true, false);
                    }
                    catch (Exception e)
                    {
                        // the persistence at terminate threw an exception.
                        this.WorkflowStatus = oldStatus;
                        this.rootActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.None);
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Persistence attempt at instance '{0}' termination threw an exception. Aborting the instance. The termination event would be raised. The instance would execute from the last persisted point whenever started by the host explicitly. Exception:{1}\n{2}", this.InstanceIdString, e.Message, e.StackTrace);
                        this.AbortOnIdle();
                        return false;
                    }

                    // Any remaining messages in queues are zombie messages so move all to the pending queue
                    this.qService.MoveAllMessagesToPendingQueue();

                    if (null != ThrownException)
                        FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Terminated, ThrownException);
                    else
                        FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Terminated, error);

                    // unsubscribe for model changes
                    Debug.Assert(this.IsInstanceValid);
                    // mark instance as invalid
                    this.IsInstanceValid = false;
                }

                if (currentAtomicActivity != null)
                {
                    atomicActivityEvent.Set();
                    atomicActivityEvent.Close();
                }
            }
            catch (Exception)
            {
                if ((this.rootActivity == this.CurrentActivity) && this.rootActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    using (_msgDeliveryLock.Enter())
                    {
                        this.AbortOnIdle();
                        return false;
                    }
                }
                else
                {
                    this.Scheduler.CanRun = true;
                    throw;
                }
            }

            return true;
        }

        #endregion

        #region Abort

        // aborts the schedule instance sync
        // must be called only from outside the instance... the thread running the instance must
        // never call this method... it should call AbortOnIdle instead.
        internal void Abort()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor::Abort : Got a abort request for instance {0}", this.InstanceIdString);

            try
            {
                // Stop threads from outside - message delivery and control operations
                using (this._executorLock.Enter())
                {
                    // tell the scheduler to stop returnig items from its queue (ref: 16534)
                    this.Scheduler.AbortOrTerminateRequested = true;
                    // tell the scheduler to stop running
                    this.Scheduler.CanRun = false;

                    // If there were some thread executing the instance, then setting up
                    // the callback, the thread getting done and the notification coming back
                    // is racy... so we lock the scheduler
                    using (new SchedulerLockGuard(this._schedulerLock, this))
                    {
                        using (this._msgDeliveryLock.Enter())
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {

                                // check if this is a valid in-memory instance
                                if (!this.IsInstanceValid)
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);


                                this.AbortOnIdle();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Abort attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
        }

        // this method must be called with the scheduler lock held
        internal void AbortOnIdle()
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);
            InstanceLock.AssertIsLocked(this._msgDeliveryLock);

            // check if the instance can be aborted
            if (!this.IsInstanceValid)
                return;

            FireWorkflowExecutionEvent(this, WorkflowEventInternal.Aborting);

            TimerQueue.SuspendDelivery();

            // tell the scheduler to stop running
            this.Scheduler.CanRun = false;

            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Aborting instance {0}", this.InstanceIdString);

            try
            {
                // abort any transaction in progress
                if (this.currentAtomicActivity != null)
                {
                    this.RollbackTransaction(null, this.currentAtomicActivity);
                    this.currentAtomicActivity = null;
                }

                // clear the batched work
                this.ResourceManager.ClearAllBatchedWork();

                // unlock instance state w/o saving it
                WorkflowPersistenceService persistenceSvc = this.WorkflowRuntime.WorkflowPersistenceService;
                if (persistenceSvc != null)
                {
                    persistenceSvc.UnlockWorkflowInstanceState(attemptedRootDispose ? null : this.rootActivity);
                    if (HasNonEmptyWorkBatch())
                    {
                        this.CommitTransaction(this.rootActivity);
                    }
                }
            }
            catch (Exception e)
            {
                if (WorkflowExecutor.IsIrrecoverableException(e))
                {
                    throw;
                }
            }
            finally
            {
                // mark instance as invalid
                this.IsInstanceValid = false;
                DisposeRootActivity(true);
                if (currentAtomicActivity != null)
                {
                    atomicActivityEvent.Set();
                    atomicActivityEvent.Close();
                }
                FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Aborted);
            }
        }

        #endregion

        #region Suspend

        // suspends the schedule instance sync
        // must be called only from outside the instance... the thread running the instance must
        // never call this method... it should call SuspendOnIdle instead.
        internal bool Suspend(string error)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a suspend request for instance {0}", this.InstanceIdString);

            try
            {
                // check if this is a valid in-memory instance
                if (!this.IsInstanceValid)
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);


                // Stop threads from outside - message delivery and control operations
                using (this._executorLock.Enter())
                {
                    // check if this is a valid in-memory instance
                    if (!this.IsInstanceValid)
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                    // tell the scheduler to stop running
                    this.Scheduler.CanRun = false;

                    // If there were some thread executing the instance, then setting up
                    // the callback, the thread getting done and the notification coming back
                    // is racy... so we lock the scheduler
                    using (new SchedulerLockGuard(this._schedulerLock, this))
                    {
                        using (new ServiceEnvironment(this.rootActivity))
                        {
                            // check if this is a valid in-memory instance
                            if (!this.IsInstanceValid)
                                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                            return this.SuspendOnIdle(error);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Suspend attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
        }

        // this method must be called with the scheduler lock held
        internal bool SuspendOnIdle(string error)
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);

            // check if the instance can be suspended
            if (!this.IsInstanceValid)
                return false;

            // if atomic activity in progress, then throw
            if (this.currentAtomicActivity != null)
            {
                this.Scheduler.CanRun = true;
                throw new ExecutorLocksHeldException(atomicActivityEvent);
            }
            else
            {
                // if already suspended or if just created, then do nothing
                WorkflowStatus status = this.WorkflowStatus;
                if (status == WorkflowStatus.Suspended || status == WorkflowStatus.Created)
                    return false;

                FireWorkflowSuspending(error);

                // tell the scheduler to stop running
                this.Scheduler.CanRun = false;

                switch (this.rootActivity.ExecutionStatus)
                {
                    case ActivityExecutionStatus.Initialized:
                    case ActivityExecutionStatus.Executing:
                    case ActivityExecutionStatus.Canceling:
                    case ActivityExecutionStatus.Faulting:
                    case ActivityExecutionStatus.Compensating:
                        break;

                    case ActivityExecutionStatus.Closed:
                        return false;
                    default:
                        return false;
                }

                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Suspending instance {0}", this.InstanceIdString);

                // mark it as suspended
                this.stateChangedSincePersistence = true;
                this.WorkflowStatus = WorkflowStatus.Suspended;
                this.rootActivity.SetValue(SuspendOrTerminateInfoProperty, error);

                // note: don't persist the instance and don't mark it as invalid.
                // The suspended instances must be explicitly unloaded, if required.
                FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Suspended, error);
                return true;
            }
        }
        #endregion

        #region Resume

        // resumes the schedule instance sync
        // must be called only from outside the instance... the thread running the instance must
        // never call this method... it should call ResumeOnIdle instead.
        internal void Resume()
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a resume request for instance {0}", this.InstanceIdString);

            try
            {
                // check if this is a valid in-memory instance
                if (!this.IsInstanceValid)
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                using (ScheduleWork work = new ScheduleWork(this))
                {
                    // Stop threads from outside - message delivery and control operations
                    using (this._executorLock.Enter())
                    {
                        // check if this is a valid in-memory instance
                        if (!this.IsInstanceValid)
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                        if ((this.WorkflowStatus != WorkflowStatus.Suspended))
                            return;

                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            //@@Undone-- bmalhi there is one test in bat
                            //which fails here. This check is right thing but im 
                            //commenting it out for bat.
                            // Microsoft:  this fails because when we load an instance into memory it grabs
                            // the scheduler lock and starts running.  By the time the user Resume request
                            // gets the scheduler lock the instance is often done (the AbortBat test case scenario)
                            // Balinder is attempting a fix to separate rehydration from resuming execution.
                            /*if (!this.IsInstanceValid)
                                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);
                             */
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                this.ResumeOnIdle(true);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: Resume attempt on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
        }

        // this method must be called with the scheduler lock held
        internal bool ResumeOnIdle(bool outsideThread)
        {
            InstanceLock.AssertIsLocked(this._schedulerLock);

            // check if this is a valid in-memory instance
            if (!this.IsInstanceValid)
                return false;

            // if not suspended and CanRun is true, then nothing to resume
            if ((this.WorkflowStatus != WorkflowStatus.Suspended) && (!this.Scheduler.CanRun))
                return false;

            FireWorkflowExecutionEvent(this, WorkflowEventInternal.Resuming);

            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Resuming instance {0}", this.InstanceIdString);

            this.stateChangedSincePersistence = true;
            this.WorkflowStatus = WorkflowStatus.Running;
            this.rootActivity.SetValue(SuspendOrTerminateInfoProperty, string.Empty);

            FireEventAfterSchedulerLockDrop(WorkflowEventInternal.Resumed, ThrownException);

            using (this._msgDeliveryLock.Enter())
            {
                TimerQueue.ResumeDelivery();
            }

            // resume the instance
            if (outsideThread)
                this.Scheduler.Resume();
            else
                // being called from within the scheduler thread, so just allow the 
                // scheduler to run without requesting a new thread
                this.Scheduler.CanRun = true;

            return true;
        }

        #endregion

        #region Transaction Management

        internal bool IsActivityInAtomicContext(Activity activity, out Activity atomicActivity)
        {
            Debug.Assert(activity != null);

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

        private void CreateTransaction(Activity atomicActivity)
        {
            Debug.Assert(this.currentAtomicActivity == null, "There is already a transacted activity running");

            TransactionalProperties transactionalProperties = new TransactionalProperties();

            TransactionOptions tranOpts = new TransactionOptions();
            WorkflowTransactionOptions atomicTxn = TransactedContextFilter.GetTransactionOptions(atomicActivity);
            Debug.Assert(atomicTxn != null, "null atomicTxn");

            // 
            tranOpts.IsolationLevel = atomicTxn.IsolationLevel;
            if (tranOpts.IsolationLevel == IsolationLevel.Unspecified)
                tranOpts.IsolationLevel = IsolationLevel.Serializable;

            tranOpts.Timeout = atomicTxn.TimeoutDuration;

            // Create a promotable transaction (can be promoted to DTC when necessary)
            // as COM+ user code may want to participate in the transaction
            // Enlist to the transaction for abort notification
            System.Transactions.CommittableTransaction transaction = new CommittableTransaction(tranOpts);
            // Can switch back to using TransactionCompletionHandler once VS562627 is fixed
            // transaction.TransactionCompleted += new TransactionCompletedEventHandler(TransactionCompletionHandler);
            //transaction.EnlistVolatile(new TransactionNotificationEnlistment(this, transaction, atomicActivity), EnlistmentOptions.None);
            transactionalProperties.Transaction = transaction;
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                " .Created enlistable transaction " + ((System.Transactions.Transaction)transaction).GetHashCode() +
                " with timeout " + tranOpts.Timeout + ", isolation " + tranOpts.IsolationLevel);

            // create a local queuing service per atomic context
            transactionalProperties.LocalQueuingService = new WorkflowQueuingService(this.qService);

            // Store the transaction properties onto the activity
            atomicActivity.SetValue(TransactionalPropertiesProperty, transactionalProperties);

            // Set current atomic activity
            this.currentAtomicActivity = atomicActivity;
            atomicActivityEvent = new ManualResetEvent(false);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString + " .Set CurrentAtomicActivity to " + atomicActivity.Name);
        }

        private void DisposeTransaction(Activity atomicActivity)
        {
            // Validates the assumption that only one atomic activity in execution at a time
            //Debug.Assert((atomicActivity == this.currentAtomicActivity),
            //    "Activity context " + atomicActivity.Name + " different from currentAtomicActivity " + this.currentAtomicActivity.Name);

            // Cleanup work following a transaction commit or Rollback
            TransactionalProperties transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(TransactionalPropertiesProperty);

            // release transaction
            transactionalProperties.Transaction.Dispose();
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                " .Disposed enlistable transaction " +
                ((System.Transactions.Transaction)transactionalProperties.Transaction).GetHashCode());

            // cleanup properties
            transactionalProperties.Transaction = null;
            transactionalProperties.LocalQueuingService = null;
            transactionalProperties.Transaction = null;

            // We no longer clear the currentAtomicActivity member here
            // but only in the callers of this method (CommitTransaction and RollbackTransaction).
            // However, we do this only in CommitTransaction but omit resetting it in RollbackTransaction
            // because a complete reversal of a TransactionScopeActivity will restore the 
            // workflow instance state to a prior checkpointed state.
            atomicActivityEvent.Set();
            atomicActivityEvent.Close();

        }

        private void CommitTransaction(Activity activityContext)
        {
            if (null == Transaction.Current)
            {
                //
                // No TxScopeActivity or external tx
                // Ask the TxService to commit
                // In this scenario retries are OK as it owns the tx
                try
                {
                    //
                    // Pass a delegate that does the batch commit 
                    // so that it can do retries
                    this.WorkflowRuntime.TransactionService.CommitWorkBatch(DoResourceManagerCommit);
                    this.ResourceManager.Complete();
                }
                catch
                {
                    this.ResourceManager.HandleFault();
                    throw;
                }
            }
            else
            {
                Debug.Assert(activityContext != null, "null activityContext");

                TransactionalProperties transactionalProperties = null;
                bool inTxScope = (activityContext == this.currentAtomicActivity);
                //
                // Tx is either from TxScopeActivity or it is external                
                if (inTxScope)
                {
                    transactionalProperties = (TransactionalProperties)activityContext.GetValue(TransactionalPropertiesProperty);
                    if (CheckAndProcessTransactionAborted(transactionalProperties))
                        return;
                }
                //
                // Commit the batches and rely on the enlistment to do completion/rollback work for the batches
                // TxService must use the ambient transaction directly or do a dependent clone.
                try
                {
                    this.WorkflowRuntime.TransactionService.CommitWorkBatch(DoResourceManagerCommit);
                }
                catch
                {
                    //
                    // This tx is doomed, clean up batches
                    ResourceManager.HandleFault();
                    throw;
                }
                finally
                {
                    if (inTxScope)
                    {
                        // DTC transaction commit needs to be done after TransactionScope Complete
                        // because the Commit Voting needs to happen on the the original thread 
                        // that created the transaction.  Otherwise the transaction will abort after timing out.
                        Debug.Assert(null != transactionalProperties, "TransactionProperties from TransactionScopeActivity should not be null.");
                        DisposeTransactionScope(transactionalProperties);
                    }
                }
                //
                // If we are in a tx scope we need to commit our tx
                if (inTxScope)
                {
                    //
                    // The tx will be Committable if there was not ambient tx when the scope started
                    // It will be Dependent if there was an ambient tx when the scope started
                    // (The external case is explicitly disabled for V1)
                    try
                    {
                        CommittableTransaction ctx = transactionalProperties.Transaction as CommittableTransaction;
                        if (null != ctx)
                        {
                            try
                            {
                                ctx.Commit();
                            }
                            catch
                            {
                                qService.PostPersist(false);
                                throw;
                            }

                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                                "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                                " .Committed CommittableTransaction " +
                                ((System.Transactions.Transaction)transactionalProperties.Transaction).GetHashCode());
                        }

                        DependentTransaction dtx = transactionalProperties.Transaction as DependentTransaction;
                        if (null != dtx)
                        {
                            try
                            {
                                dtx.Complete();
                            }
                            catch
                            {
                                qService.PostPersist(false);
                                throw;
                            }
                            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                                "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                                " .Completed DependentTransaction " +
                                ((System.Transactions.Transaction)transactionalProperties.Transaction).GetHashCode());
                        }
                    }
                    catch
                    {
                        //
                        // This tx (scope activity or external) is doomed, clean up batches
                        ResourceManager.HandleFault();
                        throw;
                    }

                    //
                    // If commit throws we'll do this call in RollbackTransaction.
                    // However, the currentAtomicActivity member is not reset in RollbackTransaction
                    // because a complete reversal of a TransactionScopeActivity will restore the 
                    // workflow instance state to a prior checkpointed state.                    
                    DisposeTransaction(activityContext);
                    this.currentAtomicActivity = null;

                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                        "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                        "Reset CurrentAtomicActivity to null");

                }
                //
                // Tell the batches that we committed successfully
                ResourceManager.Complete();
            }
        }
        /// <summary>
        /// Call commit on the VolatileResourceManager to commit all work in the batch.
        /// Transaction.Current must be non-null.
        /// </summary>
        private void DoResourceManagerCommit()
        {
            if (null == Transaction.Current)
                throw new Exception(ExecutionStringManager.NullAmbientTransaction);

            this.ResourceManager.Commit();
        }

        private void RollbackTransaction(Exception exp, Activity activityContext)
        {
            if (activityContext == this.currentAtomicActivity)
            {
                Debug.Assert((activityContext == this.currentAtomicActivity),
                    "Activity context " + activityContext.Name + " different from currentAtomicActivity " + this.currentAtomicActivity.Name);

                TransactionalProperties transactionalProperties = (TransactionalProperties)activityContext.GetValue(TransactionalPropertiesProperty);
                if (transactionalProperties.TransactionState != TransactionProcessState.AbortProcessed)
                {
                    // If TransactionState is not already AbortProcessed, Set it to AbortProcessed as we have raised exception for it already
                    // Possible call paths for which it's not already AbortProcessed:
                    // TransactionState == Aborted if due to transaction failure notified through TransactionCompletionHandler
                    // TransactionState == Ok if Called from external exception raising (e.g. a throw activity in Atomic context)
                    transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                }

                Debug.Assert((transactionalProperties.Transaction != null), "Null Transaction while transaction is present");
                Debug.Assert((transactionalProperties.LocalQueuingService != null), "Null LocalQueuingService while transaction is present");

                try
                {
                    DisposeTransactionScope(transactionalProperties);

                    // roll back transaction
                    System.Transactions.Transaction transaction = transactionalProperties.Transaction;
                    if (System.Transactions.TransactionStatus.Aborted != transaction.TransactionInformation.Status)
                        transaction.Rollback();
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                        "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                        " .Aborted enlistable transaction " +
                        ((System.Transactions.Transaction)transaction).GetHashCode());
                }
                finally
                {
                    // roolback queuing service state
                    WorkflowQueuingService queuingService = transactionalProperties.LocalQueuingService;
                    queuingService.Complete(false);

                    // dispose transaction. However, do not reset the currentAtomicActivity member here
                    // because a complete reversal of a TransactionScopeActivity will restore the 
                    // workflow instance state to a prior checkpointed state.                    
                    DisposeTransaction(this.currentAtomicActivity);
                }
            }
        }

        #region VolatileEnlistment for Transaction Completion Notification
        /*
         * Leaving this class in place as we will need it for the flow through tx story in V2
        class TransactionNotificationEnlistment : IEnlistmentNotification, IActivityEventListener<EventArgs>
        {
            WorkflowExecutor workflowExecutor;
            Transaction transaction;
            Activity atomicActivity;
            internal TransactionNotificationEnlistment(WorkflowExecutor exec, Transaction tx, Activity atomicActivity)
            {
                this.workflowExecutor = exec;
                this.transaction = tx;
                this.atomicActivity = atomicActivity;
            }

            #region IEnlistmentNotification Members

            void IEnlistmentNotification.Commit(Enlistment enlistment)
            {
                enlistment.Done();
            }

            void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }

            void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            void IEnlistmentNotification.Rollback(Enlistment enlistment)
            {
                //
                // Currently this method isn't used.  
                // The problem is that we must acquire the sched lock in order to schedule
                // an item.  While we wait trying to acquire the lock the transaction is held open.
                // If the instance is idle we acquire the lock right away and this works fine.
                // However is we have items to run we'll check the transaction, find that it is aborted
                // and start exception handling.  During the entire exception handling process the transaction
                // and the associated connections will be held open.  This is not good.
                // Post V1 we need scheduler changes to allow us to safely asynchronously schedule work
                // without taking the scheduler lock.
                enlistment.Done();
                //
                // ensure transaction timeout/abort is processed in case of a
                // blocked activity inside a transactional scope
                ScheduleTransactionTimeout();
            }

            private void ScheduleTransactionTimeout()
            {
                try
                {
                    //
                    // We're going to check executor state and possibly enqueue a workitem
                    // Must take the scheduleExecutor lock
                    using (this.workflowExecutor._schedulerLock.Enter())
                    {
                        if (!this.workflowExecutor.IsInstanceValid)
                            return;

                        // If the exception has already been taken care of, ignore this abort notification
                        Activity curAtomicActivity = this.workflowExecutor.currentAtomicActivity;
                        if ((curAtomicActivity != null)&&(curAtomicActivity==atomicActivity))
                        {
                            TransactionalProperties transactionalProperties = (TransactionalProperties)curAtomicActivity.GetValue(TransactionalPropertiesProperty);
                            if ((transactionalProperties.Transaction == this.transaction) &&
                                (transactionalProperties.TransactionState != TransactionProcessState.AbortProcessed))
                            {
                                transactionalProperties.TransactionState = TransactionProcessState.Aborted;

                                using (this.workflowExecutor.MessageDeliveryLock.Enter())
                                {
                                    using (new ServiceEnvironment(this.workflowExecutor.RootActivity))
                                    {
                                        using (this.workflowExecutor.SetCurrentActivity(curAtomicActivity))
                                        {
                                            //
                                            // This will schedule (async) a work item to cancel the tx scope activity
                                            // However this item will never get run - we always check if the 
                                            // tx has aborted prior to running any items so this is really 
                                            // just a "wake up" notification to the scheduler.
                                            Activity contextActivity = ContextActivityUtils.ContextActivity(curAtomicActivity);
                                            ActivityExecutorDelegateInfo<EventArgs> dummyCallback = new ActivityExecutorDelegateInfo<EventArgs>(this, contextActivity, true);
                                            dummyCallback.InvokeDelegate(contextActivity, EventArgs.Empty, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "AbortNotificationEnlistment: instanceId: {0} failed to process ScheduleTransactionTimeout with exception {1} ", this.workflowExecutor.this.InstanceIdString, e.Message);
                }
            }

            void IActivityEventListener<EventArgs>.OnEvent(object sender, EventArgs e)
            {
                // this will never be invoked since Scheduler will process the transaction aborted request
            }

            #endregion
        }*/
        #endregion VolatileEnlistment for AbortNotification

        internal static bool CheckAndProcessTransactionAborted(TransactionalProperties transactionalProperties)
        {
            if (transactionalProperties.Transaction != null && transactionalProperties.Transaction.TransactionInformation.Status != TransactionStatus.Aborted)
                return false;

            // If transaction aborted but not processed, 
            // process it (i.e. throw to invoke Exception handling)
            // otherwise return if transaction aborted
            switch (transactionalProperties.TransactionState)
            {
                case TransactionProcessState.Ok:
                case TransactionProcessState.Aborted:
                    transactionalProperties.TransactionState = TransactionProcessState.AbortProcessed;
                    throw new TransactionAbortedException();

                case TransactionProcessState.AbortProcessed:
                    return true;

                default:
                    return false;
            }
        }

        private void DisposeTransactionScope(TransactionalProperties transactionalProperties)
        {
            if (transactionalProperties.TransactionScope != null)
            {
                // Need to call Complete othwise the transaction will be aborted
                transactionalProperties.TransactionScope.Complete();
                transactionalProperties.TransactionScope.Dispose();
                transactionalProperties.TransactionScope = null;
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                    "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                    "Left TransactionScope, Current atomic acitivity was " +
                    ((this.currentAtomicActivity == null) ? null : this.currentAtomicActivity.Name));
            }
        }

        #region delay scheduling of items for ACID purposes

        private void AddItemToBeScheduledLater(Activity atomicActivity, SchedulableItem item)
        {
            if (atomicActivity == null)
                return;

            // Activity may not be atomic and is an activity which is not
            // yet scheduled for execution (typically receive case)
            if (!atomicActivity.SupportsTransaction)
                return;

            TransactionalProperties transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(TransactionalPropertiesProperty);
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

        private void ScheduleDelayedItems(Activity atomicActivity)
        {
            List<SchedulableItem> items = null;
            TransactionalProperties transactionalProperties = (TransactionalProperties)atomicActivity.GetValue(TransactionalPropertiesProperty);

            if (transactionalProperties == null)
                return;

            lock (transactionalProperties)
            {
                items = transactionalProperties.ItemsToBeScheduledAtCompletion;
                if (items == null)
                    return;

                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0,
                    "Workflow Runtime: WorkflowExecutor: instanceId: " + this.InstanceIdString +
                    " .Scheduling delayed " + items.Count + " number of items");

                foreach (SchedulableItem item in items)
                {
                    this.Scheduler.ScheduleItem(item, false, true);
                }
                items.Clear();

                transactionalProperties.ItemsToBeScheduledAtCompletion = null;
            }
        }

        #endregion delay scheduling of items for ACID purposes

        #endregion Transaction Management

        #region Exception Management

        internal void ExceptionOccured(Exception exp, Activity currentActivity, string originalActivityId)
        {
            Debug.Assert(exp != null, "null exp");
            Debug.Assert(currentActivity != null, "null currentActivity");
            // exception tracking work
            //
            if (this.ThrownException != exp)
            {
                // first time exception
                this.ThrownException = exp;
                this.activityThrowingException = currentActivity.QualifiedName;
                originalActivityId = currentActivity.QualifiedName;
            }
            else
            {
                // rethrown exception
                originalActivityId = this.activityThrowingException;
            }
            Guid contextGuid = ((ActivityExecutionContextInfo)ContextActivityUtils.ContextActivity(currentActivity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
            Guid parentContextGuid = Guid.Empty;
            if (null != currentActivity.Parent)
                parentContextGuid = ((ActivityExecutionContextInfo)ContextActivityUtils.ContextActivity(currentActivity.Parent).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
            this.FireExceptionOccured(exp, currentActivity.QualifiedName, originalActivityId, contextGuid, parentContextGuid);

            // notify the activity.
            //
            using (new ServiceEnvironment(currentActivity))
            {
                using (SetCurrentActivity(currentActivity))
                {
                    using (ActivityExecutionContext executionContext = new ActivityExecutionContext(currentActivity, true))
                        executionContext.FaultActivity(exp);
                }
            }

            // transaction and batching clean-up on the activity that handles the exception
            this.RollbackTransaction(exp, currentActivity);
            if ((currentActivity is TransactionScopeActivity) || (exp is PersistenceException))
                this.BatchCollection.RollbackBatch(currentActivity);
        }

        internal Exception ThrownException
        {
            get { return thrownException; }
            set { thrownException = value; }
        }

        internal static bool IsIrrecoverableException(Exception e)
        {
            return ((e is OutOfMemoryException) ||
                    (e is StackOverflowException) ||
                    (e is ThreadInterruptedException) ||
                    (e is ThreadAbortException));
        }

        #endregion Exception Management

        #region Tracking Management

        internal void Track(Activity activity, string key, object args)
        {
            FireUserTrackPoint(activity, key, args);
        }

        internal void FireExceptionOccured(Exception e, string currentActivityPath, string originalActivityPath, Guid contextGuid, Guid parentContextGuid)
        {
            FireWorkflowException(e, currentActivityPath, originalActivityPath, contextGuid, parentContextGuid);
        }

        #endregion

        #region Dynamic Update Management

        #region Dynamic Update From Outside the instance
        internal Activity GetWorkflowDefinition(string workflowContext)
        {
            if (workflowContext == null)
                throw new ArgumentNullException("workflowContext");

            return this.WorkflowDefinition;
        }

        internal Activity GetWorkflowDefinitionClone(string workflowContext)
        {
            if (workflowContext == null)
                throw new ArgumentNullException("workflowContext");

            Activity definition = this.WorkflowDefinition;

            using (new WorkflowDefinitionLock(definition))
            {
                return definition.Clone();
            }
        }

        internal void ApplyWorkflowChanges(WorkflowChanges workflowChanges)
        {
            // Accessing InstanceId is not thread safe here!
            //WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a dynamic update request from outside for instance {0}", this.InstanceIdString);
            DiagnosticStackTrace("dynamic update request");

            // check arguments
            if (workflowChanges == null)
                throw new ArgumentNullException("workflowChanges");

            // check if this is a valid in-memory instance
            if (!this.IsInstanceValid)
                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

            if (this.currentAtomicActivity != null)
                throw new InvalidOperationException(ExecutionStringManager.Error_InsideAtomicScope);

            try
            {
                using (ScheduleWork work = new ScheduleWork(this))
                {
                    // block other instance operations from outside
                    using (this._executorLock.Enter())
                    {
                        // check if this is a valid in-memory instance
                        if (!this.IsInstanceValid)
                            throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                        // get the instance to stop running
                        this.Scheduler.CanRun = false;
                        using (new SchedulerLockGuard(this._schedulerLock, this))
                        {
                            using (new ServiceEnvironment(this.rootActivity))
                            {
                                bool localSuspend = false;

                                // check if this is a valid in-memory instance
                                if (!this.IsInstanceValid)
                                    throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                                try
                                {
                                    // check the status of the schedule
                                    switch (this.WorkflowStatus)
                                    {
                                        ////case ActivityExecutionStatus.Completed:
                                        // 
                                        case WorkflowStatus.Completed:
                                        case WorkflowStatus.Terminated:
                                            throw new InvalidOperationException(
                                                ExecutionStringManager.InvalidOperationRequest);
                                        case WorkflowStatus.Suspended:
                                            // instance already suspended
                                            localSuspend = false;
                                            break;
                                        default:
                                            // suspend the instance
                                            this.SuspendOnIdle(null);
                                            localSuspend = true;
                                            break;
                                    }

                                    // apply the changes
                                    workflowChanges.ApplyTo(this.rootActivity);
                                }
                                finally
                                {
                                    if (localSuspend)
                                    {
                                        // @undone: for now this will not return till the instance is done
                                        // Once Kumar has fixed 4335, we can enable this.
                                        this.ResumeOnIdle(true);
                                    }
                                }
                            }
                        } // release lock on scheduler
                    }
                }
            }
            catch (Exception e)
            {
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, "Workflow Runtime: WorkflowExecutor: dynamic update attempt from outside on instance '{0}' threw an exception '{1}' at {2}", this.InstanceIdString, e.Message, e.StackTrace);
                throw;
            }
        }
        #endregion Dynamic Update From Outside the instance
        internal bool OnBeforeDynamicChange(IList<WorkflowChangeAction> changes)
        {
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Got a dynamic update request for instance {0}", this.InstanceIdString);

            if (!this.IsInstanceValid)
                throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Found a match for the schedule in updating instance {0}", this.InstanceIdString);

            FireDynamicUpdateBegin(changes);

            return true;
        }

        internal void OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes)
        {
            if (updateSucceeded)
            {
                RefreshWorkflowDefinition();
                //Commit temporary work
                FireDynamicUpdateCommit(changes);
                FireWorkflowExecutionEvent(this, WorkflowEventInternal.Changed);
            }
            else
            {
                // Rollback
                FireDynamicUpdateRollback(changes);
            }

            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: Done updating a schedule in instance {0}", this.InstanceIdString);

        }

        bool IWorkflowCoreRuntime.IsDynamicallyUpdated
        {
            get
            {
                return ((Activity)this.WorkflowDefinition).GetValue(WorkflowChanges.WorkflowChangeActionsProperty) != null;
            }
        }
        #endregion

        #region Diagnostic tracing

        [System.Diagnostics.Conditional("DEBUG")]
        void DiagnosticStackTrace(string reason)
        {
            StackTrace st = new StackTrace(true);
            WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "Workflow Runtime: WorkflowExecutor: InstanceId: {0} : {1} stack trace: {2}", this.InstanceIdString, reason, st.ToString());
        }

        #endregion

        #region Timer event support

        WaitCallback IWorkflowCoreRuntime.ProcessTimersCallback
        {
            get
            {
                return new WaitCallback(this.WorkflowInstance.ProcessTimers);
            }
        }

        #endregion

        #region IServiceProvider members

        object IServiceProvider.GetService(Type serviceType)
        {
            return ((IWorkflowCoreRuntime)this).GetService(this.rootActivity, serviceType);
        }
        #endregion

        #region IWorkflowCoreRuntime Members

        Activity IWorkflowCoreRuntime.CurrentActivity
        {
            get
            {
#pragma warning disable 56503
                if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                    throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
#pragma warning restore 56503
                return this.CurrentActivity;
            }
        }
        Activity IWorkflowCoreRuntime.CurrentAtomicActivity
        {
            get
            {
                return this.currentAtomicActivity;
            }
        }
        Guid IWorkflowCoreRuntime.StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);

            Guid instanceId = Guid.Empty;
            WorkflowInstance instance = this.WorkflowRuntime.InternalCreateWorkflow(new CreationContext(workflowType, this, this.CurrentActivity.QualifiedName, namedArgumentValues), Guid.NewGuid());
            if (instance != null)
            {
                instanceId = instance.InstanceId;
                instance.Start();
            }

            return instanceId;
        }
        void IWorkflowCoreRuntime.ScheduleItem(SchedulableItem item, bool isInAtomicTransaction, bool transacted, bool queueInTransaction)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            if (!queueInTransaction)
                this.Scheduler.ScheduleItem(item, isInAtomicTransaction, transacted);
            else
                this.AddItemToBeScheduledLater(this.CurrentActivity, item);
        }
        public IDisposable SetCurrentActivity(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            Activity oldCurrentActivity = this.CurrentActivity;
            this.CurrentActivity = activity;
            return new ResetCurrentActivity(this, oldCurrentActivity);
        }
        Guid IWorkflowCoreRuntime.InstanceID
        {
            get
            {
#pragma warning disable 56503
                if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                    throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
#pragma warning restore 56503
                return this.InstanceId;
            }
        }
        bool IWorkflowCoreRuntime.SuspendInstance(string suspendDescription)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            return this.SuspendOnIdle(suspendDescription);
        }
        void IWorkflowCoreRuntime.TerminateInstance(Exception e)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);

            this.ThrownException = e;
            this.TerminateOnIdle(WorkflowExecutor.GetNestedExceptionMessage(e));
        }
        bool IWorkflowCoreRuntime.Resume()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            return this.ResumeOnIdle(false);
        }
        void IWorkflowCoreRuntime.RaiseHandlerInvoking(Delegate handlerDelegate)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            FireWorkflowHandlerInvokingEvent(this, WorkflowEventInternal.HandlerInvoking, handlerDelegate);
        }
        void IWorkflowCoreRuntime.RaiseActivityExecuting(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            FireActivityExecuting(this, activity);
        }
        void IWorkflowCoreRuntime.RaiseHandlerInvoked()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            FireWorkflowExecutionEvent(this, WorkflowEventInternal.HandlerInvoked);
        }
        void IWorkflowCoreRuntime.CheckpointInstanceState(Activity currentActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);

            // Call CheckpointInstanceState() before CreateTransaction() because
            // creating a TX can fail and then we end up ----ing up in HandleFault().
            using (MessageDeliveryLock.Enter())
            {
                this.WorkflowStateRollbackService.CheckpointInstanceState();
            }
            this.CreateTransaction(currentActivity);
        }
        void IWorkflowCoreRuntime.RequestRevertToCheckpointState(Activity currentActivity, EventHandler<EventArgs> callbackHandler, EventArgs callbackData, bool suspendOnRevert, string suspendInfo)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.WorkflowStateRollbackService.RequestRevertToCheckpointState(currentActivity, callbackHandler, callbackData, suspendOnRevert, suspendInfo);
        }
        void IWorkflowCoreRuntime.DisposeCheckpointState()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.WorkflowStateRollbackService.DisposeCheckpointState();
        }
        int IWorkflowCoreRuntime.GetNewContextActivityId()
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            return this.GetNewContextId();
        }
        Activity IWorkflowCoreRuntime.GetContextActivityForId(int stateId)
        {
            if (this.subStateMap.ContainsKey(stateId))
                return this.subStateMap[stateId];
            return null;
        }
        void IWorkflowCoreRuntime.RaiseException(Exception e, Activity activity, string responsibleActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.ExceptionOccured(e, activity, responsibleActivity);
        }
        void IWorkflowCoreRuntime.RegisterContextActivity(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.RegisterDynamicActivity(activity, false);
        }
        void IWorkflowCoreRuntime.UnregisterContextActivity(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.UnregisterDynamicActivity(activity);
        }
        void IWorkflowCoreRuntime.ActivityStatusChanged(Activity activity, bool transacted, bool committed)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            if (!committed)
            {
                if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    bool mustPersistState = (TransactedContextFilter.GetTransactionOptions(activity) != null) ? true : false;
                    if (mustPersistState && this.WorkflowRuntime.WorkflowPersistenceService == null)
                    {
                        string errMsg = String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, this.InstanceId);
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, errMsg);
                        throw new InvalidOperationException(errMsg);
                    }
                }
                else if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    this.ScheduleDelayedItems(activity);
                }
                else if (activity.ExecutionStatus == ActivityExecutionStatus.Canceling || activity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                {
                    if (TransactedContextFilter.GetTransactionOptions(activity) != null)
                    {
                        // If the activity is transactional and is being canceled, roll back
                        // any batches associated with it.  (This does nothing if the activity
                        // had no batch.)
                        this.BatchCollection.RollbackBatch(activity);
                    }
                }
            }

            if (!committed)
            {
                FireActivityStatusChange(this, activity);
            }

            if (activity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                if (!(activity is ICompensatableActivity) || ((activity is ICompensatableActivity) && activity.CanUninitializeNow))
                    CorrelationTokenCollection.UninitializeCorrelationTokens(activity);
            }
        }

        void IWorkflowCoreRuntime.PersistInstanceState(Activity activity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);

            bool persistOnClose = false;
            if (activity.UserData.Contains(typeof(PersistOnCloseAttribute)))
            {
                persistOnClose = (bool)activity.UserData[typeof(PersistOnCloseAttribute)];
            }
            else
            {
                object[] attributes = activity.GetType().GetCustomAttributes(typeof(PersistOnCloseAttribute), true);
                if (attributes != null && attributes.Length > 0)
                    persistOnClose = true;
            }
            if (persistOnClose && this.WorkflowRuntime.GetService<WorkflowPersistenceService>() == null)
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceServiceWithPersistOnClose, activity.Name));

            this.ScheduleDelayedItems(activity);

            bool unlock = (activity.Parent == null) ? true : false;
            bool needsCompensation = false; // 
            this.Persist(activity, unlock, needsCompensation);
        }

        Activity IWorkflowCoreRuntime.LoadContextActivity(ActivityExecutionContextInfo contextInfo, Activity outerActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            Activity contextActivity = null;
            if (this.completedContextActivities.Contains(contextInfo))
            {
                contextActivity = (Activity)this.completedContextActivities[contextInfo];
                this.completedContextActivities.Remove(contextInfo);

                if (contextActivity.Parent != outerActivity.Parent)
                    contextActivity.parent = outerActivity.Parent;
            }
            else
            {
                using (RuntimeEnvironment runtimeEnv = new RuntimeEnvironment(this.WorkflowRuntime))
                {
                    contextActivity = this.WorkflowRuntime.WorkflowPersistenceService.LoadCompletedContextActivity(contextInfo.ContextGuid, outerActivity);
                    if (contextActivity == null)
                        throw new InvalidOperationException(ExecutionStringManager.LoadContextActivityFailed);
                }
            }
            return contextActivity;
        }
        void IWorkflowCoreRuntime.SaveContextActivity(Activity contextActivity)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.completedContextActivities.Add((ActivityExecutionContextInfo)contextActivity.GetValue(Activity.ActivityExecutionContextInfoProperty), contextActivity);
        }
        Activity IWorkflowCoreRuntime.RootActivity
        {
            get
            {
                return this.rootActivity;
            }
        }
        object IWorkflowCoreRuntime.GetService(Activity activity, Type serviceType)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);

            if (serviceType == typeof(IWorkflowCoreRuntime))
            {
                return this;
            }
            else if (serviceType == typeof(WorkflowRuntime))//sorry, no.
                return null;
            else if (serviceType == typeof(WorkflowQueuingService))
            {
                WorkflowQueuingService queuingService = ServiceEnvironment.QueuingService;
                if (queuingService == null)
                    queuingService = this.qService; // root Q service

                queuingService.CallingActivity = ContextActivityUtils.ContextActivity(activity);
                return queuingService;
            }
            else if (serviceType == typeof(IWorkflowDebuggerService))
            {
                return this._workflowDebuggerService as IWorkflowDebuggerService;
            }

            return this.WorkflowRuntime.GetService(serviceType);
        }
        bool IWorkflowCoreRuntime.OnBeforeDynamicChange(IList<WorkflowChangeAction> changes)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            return this.OnBeforeDynamicChange(changes);
        }
        void IWorkflowCoreRuntime.OnAfterDynamicChange(bool updateSucceeded, IList<WorkflowChangeAction> changes)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.OnAfterDynamicChange(updateSucceeded, changes);
        }
        void IWorkflowCoreRuntime.Track(string key, object args)
        {
            if (!ServiceEnvironment.IsInServiceThread(this.InstanceId))
                throw new InvalidOperationException(ExecutionStringManager.MustUseRuntimeThread);
            this.Track(this.CurrentActivity, key, args);
        }
        #endregion

        #region ResetCurrentActivity Class

        private class ResetCurrentActivity : IDisposable
        {
            private WorkflowExecutor workflowExecutor = null;
            private Activity oldCurrentActivity = null;
            internal ResetCurrentActivity(WorkflowExecutor workflowExecutor, Activity oldCurrentActivity)
            {
                this.workflowExecutor = workflowExecutor;
                this.oldCurrentActivity = oldCurrentActivity;
            }
            void IDisposable.Dispose()
            {
                this.workflowExecutor.CurrentActivity = oldCurrentActivity;
            }
        }
        #endregion

        // GetTransientBatch is defined in this class but if the workflow is running under a V2.0 Interop environment,
        // it calls the Interop activity to get the Batch collection.
        private static object GetTransientBatch(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");
            if (!(dependencyObject is Activity))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidArgumentType, "dependencyObject", typeof(Activity).ToString()));

            Activity currentActivity = (Activity)dependencyObject;

            // fetch workflow executor
            IWorkflowCoreRuntime workflowExecutor = null;
            ISupportInterop interopSupport = null;
            if (currentActivity != null)
            {
                workflowExecutor = ContextActivityUtils.RetrieveWorkflowExecutor(currentActivity);
                interopSupport = workflowExecutor as ISupportInterop;
            }

            while (currentActivity != null)
            {
                // If the current activity has a batch property, use it.
                IWorkBatch transientWorkBatch = currentActivity.GetValueBase(TransientBatchProperty) as IWorkBatch;
                if (transientWorkBatch != null)
                    return transientWorkBatch;

                // If it's a transactional activity (transactional scope), create a batch for it.
                // (If the activity is not executing, it means that it has canceled, probably
                // due to an exception.  In this case, we do not create the batch here, but keep
                // looking up until we find an appropriate scope, or the root.)
                if (TransactedContextFilter.GetTransactionOptions(currentActivity) != null && currentActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                    return interopSupport.BatchCollection.GetBatch(currentActivity);

                // if activity has a fault handler create a batch for it.
                if (currentActivity is CompositeActivity)
                {
                    foreach (Activity flowActivity in ((ISupportAlternateFlow)currentActivity).AlternateFlowActivities)
                    {
                        if (flowActivity is FaultHandlerActivity)
                            return interopSupport.BatchCollection.GetBatch(currentActivity);
                    }
                }

                // If it's the root activity, create a batch for it.  Note that we'll only
                // ever get here if the root activity is not also an exception handling activity.
                if (currentActivity == workflowExecutor.RootActivity)
                    return interopSupport.BatchCollection.GetBatch(currentActivity);

                currentActivity = currentActivity.Parent;
            }

            return null;
        }

        private static string GetNestedExceptionMessage(Exception exp)
        {
            string expMessage = "";
            while (exp != null)
            {
                if (expMessage == "")
                    expMessage = exp.Message;
                else
                    expMessage = expMessage + " " + exp.Message;
                exp = exp.InnerException;
            }
            return expMessage;
        }

        #region Internal Events

        internal class WorkflowExecutionEventArgs : EventArgs
        {
            protected WorkflowEventInternal _eventType;

            protected WorkflowExecutionEventArgs() { }

            internal WorkflowExecutionEventArgs(WorkflowEventInternal eventType)
            {
                _eventType = eventType;
            }

            internal WorkflowEventInternal EventType
            {
                get { return _eventType; }
            }
        }
        private event EventHandler<WorkflowExecutionEventArgs> _workflowExecutionEvent;

        internal class WorkflowHandlerInvokingEventArgs : WorkflowExecutionEventArgs
        {
            private Delegate _delegateHandler;

            internal WorkflowHandlerInvokingEventArgs(WorkflowEventInternal eventType, Delegate delegateHandler)
                : base(eventType)
            {
                _delegateHandler = delegateHandler;
            }

            internal Delegate DelegateMethod
            {
                get { return _delegateHandler; }
            }
        }

        /// <summary>
        /// Consolidated event for the majority of the general events.  
        /// Filter specific events by WorkflowEventEventArgs.EventType.
        /// </summary>
        internal event EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs> WorkflowExecutionEvent
        {
            add
            {
                _workflowExecutionEvent += value;
            }
            remove
            {
                _workflowExecutionEvent -= value;
            }
        }

        internal void FireWorkflowExecutionEvent(object sender, WorkflowEventInternal eventType)
        {
            if (null == sender)
                sender = this;

            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(sender, new WorkflowExecutionEventArgs(eventType));
        }

        internal void FireWorkflowHandlerInvokingEvent(object sender, WorkflowEventInternal eventType, Delegate delegateHandler)
        {
            if (null == sender)
                sender = this;

            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(sender, new WorkflowHandlerInvokingEventArgs(eventType, delegateHandler));
        }

        internal sealed class WorkflowExecutionSuspendingEventArgs : WorkflowExecutionEventArgs
        {
            private string _error;

            internal WorkflowExecutionSuspendingEventArgs(string error)
            {
                _eventType = WorkflowEventInternal.Suspending;
                _error = error;
            }

            internal string Error
            {
                get { return _error; }
            }
        }

        internal sealed class WorkflowExecutionSuspendedEventArgs : WorkflowExecutionEventArgs
        {
            private string _error;

            internal WorkflowExecutionSuspendedEventArgs(string error)
            {
                _eventType = WorkflowEventInternal.Suspended;
                _error = error;
            }

            internal string Error
            {
                get { return _error; }
            }
        }
        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Suspended and WorkflowSuspendedInternalEventArgs
        /// </summary>
        /// <param name="info">Reason for the suspension</param>
        private void FireWorkflowSuspending(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionSuspendingEventArgs(error));
        }

        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Suspended and WorkflowSuspendInternalEventArgs
        /// </summary>
        /// <param name="info">Reason for the suspension.</param>
        internal void FireWorkflowSuspended(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionSuspendedEventArgs(error));
        }


        internal class WorkflowExecutionExceptionEventArgs : WorkflowExecutionEventArgs
        {
            private System.Exception _exception;
            private string _currentPath, _originalPath;
            private Guid _contextGuid, _parentContextGuid;

            internal WorkflowExecutionExceptionEventArgs(Exception exception, string currentPath, string originalPath, Guid contextGuid, Guid parentContextGuid)
            {
                if (null == exception)
                    throw new ArgumentNullException("exception");

                _exception = exception;
                _currentPath = currentPath;
                _originalPath = originalPath;
                _eventType = WorkflowEventInternal.Exception;
                _contextGuid = contextGuid;
                _parentContextGuid = parentContextGuid;
            }

            internal Exception Exception
            {
                get { return _exception; }
            }

            internal string CurrentPath
            {
                get { return _currentPath; }
            }

            internal string OriginalPath
            {
                get { return _originalPath; }
            }

            internal Guid ContextGuid
            {
                get { return _contextGuid; }
            }

            internal Guid ParentContextGuid
            {
                get { return _parentContextGuid; }
            }
        }
        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Exception and WorkflowExceptionInternalEventArgs
        /// </summary>
        /// <param name="exception">Thrown exception</param>
        private void FireWorkflowException(Exception exception, string currentPath, string originalPath, Guid contextGuid, Guid parentContextGuid)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionExceptionEventArgs(exception, currentPath, originalPath, contextGuid, parentContextGuid));
        }


        internal sealed class WorkflowExecutionTerminatedEventArgs : WorkflowExecutionEventArgs
        {
            private System.Exception _exception;
            private string _error;

            internal WorkflowExecutionTerminatedEventArgs(string error)
            {
                _error = error;
                _eventType = WorkflowEventInternal.Terminated;
            }

            internal WorkflowExecutionTerminatedEventArgs(Exception exception)
            {
                _exception = exception;
                _eventType = WorkflowEventInternal.Terminated;
            }

            internal Exception Exception
            {
                get { return _exception; }
            }

            internal string Error
            {
                get { return _error; }
            }
        }
        internal sealed class WorkflowExecutionTerminatingEventArgs : WorkflowExecutionEventArgs
        {
            private System.Exception _exception;
            private string _error;

            internal WorkflowExecutionTerminatingEventArgs(string error)
            {
                _error = error;
                _eventType = WorkflowEventInternal.Terminating;
            }

            internal WorkflowExecutionTerminatingEventArgs(Exception exception)
            {
                if (null == exception)
                    throw new ArgumentNullException("exception");

                _exception = exception;
                _eventType = WorkflowEventInternal.Terminating;
            }

            internal Exception Exception
            {
                get { return _exception; }
            }

            internal string Error
            {
                get { return _error; }
            }
        }
        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Terminated and WorkflowTerminatedInternalEventArgs
        /// </summary>
        /// <param name="exception">Exception that caused the termination</param>
        private void FireWorkflowTerminating(Exception exception)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionTerminatingEventArgs(exception));
        }
        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Terminated and WorkflowTerminatedInternalEventArgs
        /// </summary>
        /// <param name="info">Reason for the termination</param>
        private void FireWorkflowTerminating(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionTerminatingEventArgs(error));
        }
        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Terminated and WorkflowTerminatedInternalEventArgs
        /// </summary>
        /// <param name="exception">Exception that caused the termination</param>
        internal void FireWorkflowTerminated(Exception exception)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionTerminatedEventArgs(exception));
        }
        /// <summary>
        /// Fires the WorkflowEvent with an EventType of Terminated and WorkflowTerminatedInternalEventArgs
        /// </summary>
        /// <param name="info">Reason for the termination</param>
        internal void FireWorkflowTerminated(string error)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new WorkflowExecutionTerminatedEventArgs(error));
        }



        internal class DynamicUpdateEventArgs : WorkflowExecutionEventArgs
        {
            private IList<WorkflowChangeAction> _changeActions = new List<WorkflowChangeAction>();

            internal DynamicUpdateEventArgs(IList<WorkflowChangeAction> changeActions, WorkflowEventInternal eventType)
            {
                _changeActions = changeActions;
                _eventType = eventType;
            }

            internal IList<WorkflowChangeAction> ChangeActions
            {
                get { return _changeActions; }
            }
        }
        /// <summary>
        /// Signals that a dynamic update is starting.
        /// </summary>
        private void FireDynamicUpdateBegin(IList<WorkflowChangeAction> changeActions)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new DynamicUpdateEventArgs(changeActions, WorkflowEventInternal.DynamicChangeBegin));
        }


        /// <summary>
        /// Signals that a dynamic update has errored and rolledback.
        /// </summary>
        private void FireDynamicUpdateRollback(IList<WorkflowChangeAction> changeActions)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new DynamicUpdateEventArgs(changeActions, WorkflowEventInternal.DynamicChangeRollback));
        }


        /// <summary>
        /// Signals that a dynamic update has completed successfully.
        /// </summary>
        private void FireDynamicUpdateCommit(IList<WorkflowChangeAction> changeActions)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new DynamicUpdateEventArgs(changeActions, WorkflowEventInternal.DynamicChangeCommit));
        }

        internal class ActivityStatusChangeEventArgs : WorkflowExecutionEventArgs
        {
            private Activity _activity;

            internal ActivityStatusChangeEventArgs(Activity activity)
            {
                _activity = activity;
                _eventType = WorkflowEventInternal.ActivityStatusChange;
            }

            internal Activity Activity
            {
                get { return _activity; }
            }
        }

        internal class ActivityExecutingEventArgs : WorkflowExecutionEventArgs
        {
            private Activity _activity;

            internal ActivityExecutingEventArgs(Activity activity)
            {
                _activity = activity;
                _eventType = WorkflowEventInternal.ActivityExecuting;
            }

            internal Activity Activity
            {
                get { return _activity; }
            }
        }
        /// <summary>
        /// Signals that an activity has changed status.  
        /// This event applies to all status change events 
        /// for all activities in the workflow.
        /// </summary>
        private void FireActivityStatusChange(object sender, Activity activity)
        {
            ActivityStatusChangeEventArgs args = new ActivityStatusChangeEventArgs(activity);

            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(sender, args);
        }

        private void FireActivityExecuting(object sender, Activity activity)
        {
            ActivityExecutingEventArgs args = new ActivityExecutingEventArgs(activity);

            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(sender, args);
        }

        internal class UserTrackPointEventArgs : WorkflowExecutionEventArgs
        {
            Activity _activity;
            string _key;
            object _args;

            internal UserTrackPointEventArgs(Activity activity, string key, object args)
            {
                if (null == activity)
                    throw new ArgumentNullException("activity");

                _activity = activity;
                //
                // args may be null, user code can send non null value
                _args = args;
                _eventType = WorkflowEventInternal.UserTrackPoint;
                _key = key;
            }

            internal Activity Activity
            {
                get { return _activity; }
            }

            internal string Key
            {
                get { return _key; }
            }

            internal object Args
            {
                get { return _args; }
            }
        }

        private void FireUserTrackPoint(Activity activity, string key, object args)
        {
            EventHandler<WorkflowExecutionEventArgs> localWorkflowExecutionEvent = this._workflowExecutionEvent;
            if (null != localWorkflowExecutionEvent)
                localWorkflowExecutionEvent(this, new UserTrackPointEventArgs(activity, key, args));
        }


        #endregion Internal Events
    }

    internal class ScheduleWork : IDisposable
    {
        internal class ScheduleInfo
        {
            public bool scheduleWork;
            public WorkflowExecutor executor;
            public bool suppress;
            public ScheduleInfo(WorkflowExecutor executor, bool suppress)
            {
                this.suppress = suppress;
                scheduleWork = false;
                this.executor = executor;
            }
        }
        [ThreadStatic]
        protected static ScheduleInfo scheduleInfo;
        protected ScheduleInfo oldValue;

        public ScheduleWork(WorkflowExecutor executor)
        {
            oldValue = scheduleInfo;
            scheduleInfo = new ScheduleInfo(executor, false);
        }

        public ScheduleWork(WorkflowExecutor executor, bool suppress)
        {
            oldValue = scheduleInfo;
            scheduleInfo = new ScheduleInfo(executor, suppress);
        }

        static public bool NeedsService
        {
            //           get
            //          {
            //               Debug.Assert(ScheduleWork.scheduleInfo != null);
            //               return ScheduleWork.scheduleInfo.scheduleWork;
            //           }
            set
            {
                Debug.Assert(ScheduleWork.scheduleInfo != null);
                Debug.Assert(value == true || ScheduleWork.scheduleInfo.scheduleWork == false);  // never go from true to false
                ScheduleWork.scheduleInfo.scheduleWork = value;
            }
        }
        static public WorkflowExecutor Executor
        {
            //           get
            //           {
            //               Debug.Assert(ScheduleWork.scheduleInfo != null);
            //               return ScheduleWork.scheduleInfo.executor;
            //           }
            set
            {
                Debug.Assert(ScheduleWork.scheduleInfo != null);
                ScheduleWork.scheduleInfo.executor = value;
            }
        }
        #region IDisposable Members

        public virtual void Dispose()
        {
            if ((scheduleInfo.scheduleWork) && (!scheduleInfo.suppress))
            {
                scheduleInfo.executor.RequestHostingService();
            }
            scheduleInfo = oldValue;
        }

        #endregion
    }
}
