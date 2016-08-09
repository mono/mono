#region Imports

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Globalization;
using System.IO;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Configuration;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Tracking;
using System.Workflow.ComponentModel.Compiler;
using System.Xml;
using System.Workflow.Runtime.DebugEngine;
using System.Workflow.ComponentModel.Serialization;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;

#endregion

namespace System.Workflow.Runtime
{
    #region Class WorkflowRuntimeEventArgs

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowRuntimeEventArgs : EventArgs
    {
        private bool _isStarted;

        internal WorkflowRuntimeEventArgs(bool isStarted)
        {
            _isStarted = isStarted;
        }

        public bool IsStarted { get { return _isStarted; } }
    }

    #endregion

    internal class FanOutOnKeyDictionary<K, V> : IEnumerable<Dictionary<K, V>>
    {
        Dictionary<int, Dictionary<K, V>> dictionaryDictionary;

        public FanOutOnKeyDictionary(int fanDegree)
        {
            dictionaryDictionary = new Dictionary<int, Dictionary<K, V>>(fanDegree);
            for (int i = 0; i < fanDegree; ++i)
            {
                dictionaryDictionary.Add(i, new Dictionary<K, V>());
            }
        }

        public Dictionary<K, V> this[K key]
        {
            get
            {
                return dictionaryDictionary[Math.Abs(key.GetHashCode() % dictionaryDictionary.Count)];
            }
        }

        public bool SafeTryGetValue(K key, out V value)
        {
            Dictionary<K, V> dict = this[key];
            lock (dict)
            {
                return dict.TryGetValue(key, out value);
            }
        }

        #region IEnumerable<Dictionary<K,V>> Members

        public IEnumerator<Dictionary<K, V>> GetEnumerator()
        {
            return dictionaryDictionary.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionaryDictionary.Values.GetEnumerator();
        }

        #endregion
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class WorkflowRuntime : IServiceProvider, IDisposable
    {
        #region Private members

        internal const string DefaultName = "WorkflowRuntime";
        // Instances aggregation
        private FanOutOnKeyDictionary<Guid, WorkflowExecutor> workflowExecutors;
        private WorkflowDefinitionDispenser _workflowDefinitionDispenser;

        private PerformanceCounterManager _performanceCounterManager;

        private bool _disposed = false;
        //This is Instance Specific Flag to mark the given instance of
        //Instance Service is started or not.
        private bool isInstanceStarted;
        private DebugController debugController;
        private object _servicesLock = new object();        // protects integrity or the services collection
        private object _startStopLock = new object();       // serializes calls to start and stop        
        private Guid _uid = Guid.NewGuid();

        private BooleanSwitch disableWorkflowDebugging = new BooleanSwitch("DisableWorkflowDebugging", "Disables workflow debugging in host");

        private TrackingListenerFactory _trackingFactory = new TrackingListenerFactory();
        private static Dictionary<Guid, WeakReference> _runtimes = new Dictionary<Guid, WeakReference>();
        private static object _runtimesLock = new object(); // protects the collection of runtime objects

        #endregion

        #region Constructors and Configure methods

        static WorkflowRuntime()
        {
            // listen to activity definition resolve events
            Activity.ActivityResolve += OnActivityDefinitionResolve;
            Activity.WorkflowChangeActionsResolve += OnWorkflowChangeActionsResolve;
            
            try
            {
                using (TelemetryEventSource eventSource = new TelemetryEventSource())
                {
                    eventSource.V1Runtime();
                }
            }
            catch
            {
            }
        }

        public WorkflowRuntime()
        {
            this.PrivateInitialize(null);
        }

        public WorkflowRuntime(string configSectionName)
        {
            if (configSectionName == null)
                throw new ArgumentNullException("configSectionName");

            WorkflowRuntimeSection settings = ConfigurationManager.GetSection(configSectionName) as WorkflowRuntimeSection;
            if (settings == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                    ExecutionStringManager.ConfigurationSectionNotFound, configSectionName), "configSectionName");

            this.PrivateInitialize(settings);
        }
        /// <summary> Creates a WorkflowRuntime from settings. </summary>
        /// <param name="configuration"> The settings for this container </param>
        public WorkflowRuntime(WorkflowRuntimeSection settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            this.PrivateInitialize(settings);

        }

        private void VerifyInternalState()
        {
            if (_disposed)
                throw new ObjectDisposedException("WorkflowRuntime");
        }
        /// <summary>Initializes this container with the provided settings.</summary>
        /// <param name="settings"></param>
        private void PrivateInitialize(WorkflowRuntimeSection settings)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Created WorkflowRuntime {0}", _uid);

            _workflowDefinitionDispenser = new WorkflowDefinitionDispenser(this, (settings != null) ? settings.ValidateOnCreate : true, (settings != null) ? settings.WorkflowDefinitionCacheCapacity : 0);
            workflowExecutors = new FanOutOnKeyDictionary<Guid, WorkflowExecutor>((Environment.ProcessorCount * 4) - 1);
            _name = DefaultName;

            if (settings == null || settings.EnablePerformanceCounters) // on by default
                this.PerformanceCounterManager = new PerformanceCounterManager();

            if (settings != null)
            {
                _name = settings.Name;
                _configurationParameters = settings.CommonParameters;

                foreach (WorkflowRuntimeServiceElement service in settings.Services)
                {
                    AddServiceFromSettings(service);
                }
            }

            // create controller
            if (!disableWorkflowDebugging.Enabled)
            {
                DebugController.InitializeProcessSecurity();
                this.debugController = new DebugController(this, _name);
            }

            lock (_runtimesLock)
            {
                if (!_runtimes.ContainsKey(_uid))
                    _runtimes.Add(_uid, new WeakReference(this));
            }
        }

        public void Dispose()
        {
            lock (_startStopLock)
            {
                if (!_disposed)
                {
                    if (this.debugController != null)
                    {
                        this.debugController.Close();
                    }
                    _workflowDefinitionDispenser.Dispose();
                    _startedServices = false;
                    _disposed = true;
                }
            }
            lock (_runtimesLock)
            {
                //
                // Clean up our weakref entries
                if (_runtimes.ContainsKey(_uid))
                    _runtimes.Remove(_uid);
            }
        }

        internal bool IsZombie
        {
            get
            {
                return this._disposed;
            }
        }

        #endregion

        #region Workflow accessor methods

        public WorkflowInstance GetWorkflow(Guid instanceId)
        {
            if (instanceId == Guid.Empty)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "instanceId"));
            VerifyInternalState();
            if (!IsStarted)
                throw new InvalidOperationException(ExecutionStringManager.WorkflowRuntimeNotStarted);

            WorkflowExecutor executor = Load(instanceId, null, null);
            return executor.WorkflowInstance;
        }

        public ReadOnlyCollection<WorkflowInstance> GetLoadedWorkflows()
        {
            VerifyInternalState();
            List<WorkflowInstance> lSchedules = new List<WorkflowInstance>();
            foreach (WorkflowExecutor executor in GetWorkflowExecutors())
            {
                lSchedules.Add(executor.WorkflowInstance);
            }
            return lSchedules.AsReadOnly();
        }

        internal WorkflowDefinitionDispenser DefinitionDispenser
        {
            get
            {
                return _workflowDefinitionDispenser;
            }
        }

        #endregion

        #region Service accessors

        internal List<TrackingService> TrackingServices
        {
            get
            {
                List<TrackingService> retval = new List<TrackingService>();
                foreach (TrackingService trackingService in GetAllServices(typeof(TrackingService)))
                {
                    retval.Add(trackingService);
                }
                return retval;
            }
        }
        internal WorkflowSchedulerService SchedulerService
        {
            get
            {
                return GetService<WorkflowSchedulerService>();
            }
        }

        internal WorkflowCommitWorkBatchService TransactionService
        {
            get
            {
                return (WorkflowCommitWorkBatchService)GetService(typeof(WorkflowCommitWorkBatchService));
            }
        }

        internal WorkflowPersistenceService WorkflowPersistenceService
        {
            get
            {
                return (WorkflowPersistenceService)GetService(typeof(WorkflowPersistenceService));
            }
        }

        internal System.Workflow.Runtime.PerformanceCounterManager PerformanceCounterManager
        {
            get
            {
                return _performanceCounterManager;
            }
            private set
            {
                _performanceCounterManager = value;
            }
        }

        internal TrackingListenerFactory TrackingListenerFactory
        {
            get
            {
                return _trackingFactory;
            }
        }

        #endregion

        #region Workflow creation methods

        internal Activity GetWorkflowDefinition(Type workflowType)
        {
            if (workflowType == null)
                throw new ArgumentNullException("workflowType");
            VerifyInternalState();

            return _workflowDefinitionDispenser.GetRootActivity(workflowType, false, true);
        }

        public WorkflowInstance CreateWorkflow(Type workflowType)
        {
            if (workflowType == null)
                throw new ArgumentNullException("workflowType");
            if (!typeof(Activity).IsAssignableFrom(workflowType))
                throw new ArgumentException(ExecutionStringManager.TypeMustImplementRootActivity, "workflowType");
            VerifyInternalState();

            return InternalCreateWorkflow(new CreationContext(workflowType, null, null, null), Guid.NewGuid());
        }

        public WorkflowInstance CreateWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
        {
            return CreateWorkflow(workflowType, namedArgumentValues, Guid.NewGuid());
        }

        public WorkflowInstance CreateWorkflow(XmlReader workflowDefinitionReader)
        {
            if (workflowDefinitionReader == null)
                throw new ArgumentNullException("workflowDefinitionReader");
            VerifyInternalState();

            return CreateWorkflow(workflowDefinitionReader, null, null);
        }

        public WorkflowInstance CreateWorkflow(XmlReader workflowDefinitionReader, XmlReader rulesReader, Dictionary<string, object> namedArgumentValues)
        {
            return CreateWorkflow(workflowDefinitionReader, rulesReader, namedArgumentValues, Guid.NewGuid());
        }

        public WorkflowInstance CreateWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues, Guid instanceId)
        {
            if (workflowType == null)
                throw new ArgumentNullException("workflowType");
            if (!typeof(Activity).IsAssignableFrom(workflowType))
                throw new ArgumentException(ExecutionStringManager.TypeMustImplementRootActivity, "workflowType");
            VerifyInternalState();

            return InternalCreateWorkflow(new CreationContext(workflowType, null, null, namedArgumentValues), instanceId);
        }

        public WorkflowInstance CreateWorkflow(XmlReader workflowDefinitionReader, XmlReader rulesReader, Dictionary<string, object> namedArgumentValues, Guid instanceId)
        {
            if (workflowDefinitionReader == null)
                throw new ArgumentNullException("workflowDefinitionReader");
            VerifyInternalState();

            CreationContext context = new CreationContext(workflowDefinitionReader, rulesReader, namedArgumentValues);
            return InternalCreateWorkflow(context, instanceId);
        }

        internal WorkflowInstance InternalCreateWorkflow(CreationContext context, Guid instanceId)
        {
            using (new WorkflowTraceTransfer(instanceId))
            {
                VerifyInternalState();

                if (!IsStarted)
                    this.StartRuntime();

                WorkflowExecutor executor = GetWorkflowExecutor(instanceId, context);
                if (!context.Created)
                {
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowWithIdAlreadyExists);
                }


                return executor.WorkflowInstance;
            }
        }

        internal sealed class WorkflowExecutorInitializingEventArgs : EventArgs
        {
            private bool _loading = false;

            internal WorkflowExecutorInitializingEventArgs(bool loading)
            {
                _loading = loading;
            }

            internal bool Loading
            {
                get { return _loading; }
            }
        }

        // register for idle events here
        /// <summary>
        /// Raised whenever a WorkflowExecutor is constructed.  This signals either a new instance 
        /// or a loading (args) and gives listening components a chance to set up subscriptions.
        /// </summary>
        internal event EventHandler<WorkflowExecutorInitializingEventArgs> WorkflowExecutorInitializing;
        public event EventHandler<WorkflowEventArgs> WorkflowIdled;
        public event EventHandler<WorkflowEventArgs> WorkflowCreated;
        public event EventHandler<WorkflowEventArgs> WorkflowStarted;
        public event EventHandler<WorkflowEventArgs> WorkflowLoaded;
        public event EventHandler<WorkflowEventArgs> WorkflowUnloaded;
        public event EventHandler<WorkflowCompletedEventArgs> WorkflowCompleted;
        public event EventHandler<WorkflowTerminatedEventArgs> WorkflowTerminated;
        public event EventHandler<WorkflowEventArgs> WorkflowAborted;
        public event EventHandler<WorkflowSuspendedEventArgs> WorkflowSuspended;
        public event EventHandler<WorkflowEventArgs> WorkflowPersisted;
        public event EventHandler<WorkflowEventArgs> WorkflowResumed;
        internal event EventHandler<WorkflowEventArgs> WorkflowDynamicallyChanged;
        public event EventHandler<ServicesExceptionNotHandledEventArgs> ServicesExceptionNotHandled;
        public event EventHandler<WorkflowRuntimeEventArgs> Stopped;
        public event EventHandler<WorkflowRuntimeEventArgs> Started;

        internal WorkflowExecutor Load(WorkflowInstance instance)
        {
            return Load(instance.InstanceId, null, instance);
        }

        internal WorkflowExecutor Load(Guid key, CreationContext context, WorkflowInstance workflowInstance)
        {
            WorkflowExecutor executor;
            Dictionary<Guid, WorkflowExecutor> executors = workflowExecutors[key];


            lock (executors)
            {
                if (!IsStarted)
                    throw new InvalidOperationException(ExecutionStringManager.WorkflowRuntimeNotStarted);

                if (executors.TryGetValue(key, out executor))
                {
                    if (executor.IsInstanceValid)
                    {
                        return executor;
                    }
                }

                // If we get here, 'executor' is either null or unusable.
                // Before grabbing the lock, allocate a resource as we
                // may need to insert a new resource.
                executor = new WorkflowExecutor(key);
                if (workflowInstance == null)
                    workflowInstance = new WorkflowInstance(key, this);

                InitializeExecutor(key, context, executor, workflowInstance);
                try
                {
                    // If we get here, 'executor' is either null or has not been replaced.
                    // If it has not been replaced, we know that it is unusable

                    WorkflowTrace.Host.TraceInformation("WorkflowRuntime:: replacing unusable executor for key {0} with new one (hc: {1})", key, executor.GetHashCode());
                    executors[key] = executor;
                    RegisterExecutor(context != null && context.IsActivation, executor);
                }
                catch
                {
                    WorkflowExecutor currentRes;
                    if (executors.TryGetValue(key, out currentRes))
                    {
                        if (Object.Equals(executor, currentRes))
                        {
                            executors.Remove(key);
                        }
                    }
                    throw;
                }
            }
            executor.Registered(context != null && context.IsActivation);
            return executor;
        }

        // this should be called under scheduler lock
        // todo assert this condition
        internal void ReplaceWorkflowExecutor(Guid instanceId, WorkflowExecutor oldWorkflowExecutor, WorkflowExecutor newWorkflowExecutor)
        {
            Dictionary<Guid, WorkflowExecutor> executors = workflowExecutors[instanceId];
            lock (executors)
            {
                oldWorkflowExecutor.IsInstanceValid = false;

                WorkflowTrace.Host.TraceInformation("WorkflowRuntime:: replacing old executor for key {0} with new one", instanceId);
                executors[instanceId] = newWorkflowExecutor;
            }
        }

        private Activity InitializeExecutor(Guid instanceId, CreationContext context, WorkflowExecutor executor, WorkflowInstance workflowInstance)
        {
            Activity rootActivity = null;
            if (context != null && context.IsActivation)
            {
                Activity workflowDefinition = null;
                string xomlText = null;
                string rulesText = null;

                if (context.Type != null)
                {
                    workflowDefinition = _workflowDefinitionDispenser.GetRootActivity(context.Type, false, true);
                    //spawn a new instance
                    rootActivity = _workflowDefinitionDispenser.GetRootActivity(context.Type, true, false);
                }
                else if (context.XomlReader != null)
                {
                    try
                    {
                        context.XomlReader.MoveToContent();
                        while (!context.XomlReader.EOF && !context.XomlReader.IsStartElement())
                            context.XomlReader.Read();

                        xomlText = context.XomlReader.ReadOuterXml();

                        if (context.RulesReader != null)
                        {
                            context.RulesReader.MoveToContent();
                            while (!context.RulesReader.EOF && !context.RulesReader.IsStartElement())
                                context.RulesReader.Read();

                            rulesText = context.RulesReader.ReadOuterXml();
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException(ExecutionStringManager.InvalidXAML, e);
                    }

                    if (!string.IsNullOrEmpty(xomlText))
                    {
                        workflowDefinition = _workflowDefinitionDispenser.GetRootActivity(xomlText, rulesText, false, true);
                        //spawn a new instance
                        rootActivity = _workflowDefinitionDispenser.GetRootActivity(xomlText, rulesText, true, false);
                    }
                    else
                        throw new ArgumentException(ExecutionStringManager.InvalidXAML);
                }
                rootActivity.SetValue(Activity.WorkflowDefinitionProperty, workflowDefinition);

                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Creating instance " + instanceId.ToString());

                context.Created = true;
                executor.Initialize(rootActivity, context.InvokerExecutor, context.InvokeActivityID, instanceId, context.Args, workflowInstance);
            }
            else
            {
                if (this.WorkflowPersistenceService == null)
                {
                    string errMsg = String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MissingPersistenceService, instanceId);
                    WorkflowTrace.Runtime.TraceEvent(TraceEventType.Error, 0, errMsg);
                    throw new InvalidOperationException(errMsg);
                }

                // get the state from the persistenceService
                using (RuntimeEnvironment runtimeEnv = new RuntimeEnvironment(this))
                {
                    rootActivity = this.WorkflowPersistenceService.LoadWorkflowInstanceState(instanceId);
                }
                if (rootActivity == null)
                {
                    throw new InvalidOperationException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.InstanceNotFound, instanceId));
                }
                executor.Reload(rootActivity, workflowInstance);
            }
            return rootActivity;
        }

        private void RegisterExecutor(bool isActivation, WorkflowExecutor executor)
        {
            if (isActivation)
            {
                executor.RegisterWithRuntime(this);
            }
            else
            {
                executor.ReRegisterWithRuntime(this);
            }
        }

        /// <summary>
        /// On receipt of this call unload the instance
        /// This will be invoked by the runtime executor
        /// </summary>
        /// <param name="instanceId"></param>
        internal void OnIdle(WorkflowExecutor executor)
        {
            // raise the OnIdle event , typically handled 
            // by the hosting environment
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Received OnIdle Event for instance, {0}", executor.InstanceId);
                WorkflowInstance scheduleInstance = executor.WorkflowInstance;
                if (WorkflowIdled != null)
                {
                    WorkflowIdled(this, new WorkflowEventArgs(scheduleInstance));
                }
            }
            catch (Exception)
            {
                //
                WorkflowTrace.Host.TraceEvent(TraceEventType.Warning, 0, "OnIdle Event for instance, {0} threw an exception", executor.InstanceId);
                throw;
            }
        }

        private void _unRegister(WorkflowExecutor executor)
        {
            TryRemoveWorkflowExecutor(executor.InstanceId, executor);
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime::_removeInstance, instance:{0}, hc:{1}", executor.InstanceId, executor.GetHashCode());

            // be sure to flush all traces
            WorkflowTrace.Runtime.Flush();
            WorkflowTrace.Tracking.Flush();
            WorkflowTrace.Host.Flush();
        }

        private WorkflowExecutor GetWorkflowExecutor(Guid instanceId, CreationContext context)
        {
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime dispensing resource, instanceId: {0}", instanceId);

                WorkflowExecutor executor = this.Load(instanceId, context, null);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime dispensing resource instanceId: {0}, hc: {1}", instanceId, executor.GetHashCode());
                return executor;
            }
            catch (OutOfMemoryException)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime dispensing resource, can't create service due to OOM!(1), instance, {0}", instanceId);
                throw;
            }
            catch (Exception e)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime dispensing resource, can't create service due to unexpected exception!(2), instance, {0}, exception, {1}", instanceId, e);
                throw;
            }
        }

        #endregion

        #region Workflow event handlers

        internal void OnScheduleCompleted(WorkflowExecutor schedule, WorkflowCompletedEventArgs args)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleCompleted event raised for instance Id {0}", schedule.InstanceId);

            Debug.Assert(schedule != null);
            try
            {
                //Notify Subscribers
                if (WorkflowCompleted != null) WorkflowCompleted(this, args);
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnScheduleCompleted Event threw an exception.");
                throw;
            }
            finally
            {
                _unRegister(schedule);
            }
        }

        internal void OnScheduleSuspended(WorkflowExecutor schedule, WorkflowSuspendedEventArgs args)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleSuspension event raised for instance Id {0}", schedule.InstanceId);

            try
            {
                if (WorkflowSuspended != null) WorkflowSuspended(this, args);
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnScheduleSuspended Event threw an exception.");
                throw;
            }
        }

        internal void OnScheduleTerminated(WorkflowExecutor schedule, WorkflowTerminatedEventArgs args)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleTermination event raised for instance Id {0}", schedule.InstanceId);

            try
            {
                if (WorkflowTerminated != null) WorkflowTerminated(this, args);
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnScheduleTerminated Event threw an exception.");
                throw;
            }
            finally
            {
                _unRegister(schedule);
            }
        }

        internal void OnScheduleLoaded(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleLoaded event raised for instance Id {0}", schedule.InstanceId);

            _OnServiceEvent(schedule, false, WorkflowLoaded);
        }

        internal void OnScheduleAborted(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleAborted event raised for instance Id {0}", schedule.InstanceId);
            _OnServiceEvent(schedule, true, WorkflowAborted);
        }

        internal void OnScheduleUnloaded(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleUnloaded event raised for instance Id {0}", schedule.InstanceId);

            _OnServiceEvent(schedule, true, WorkflowUnloaded);
        }

        internal void OnScheduleResumed(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleResumed event raised for instance Id {0}", schedule.InstanceId);
            _OnServiceEvent(schedule, false, WorkflowResumed);
        }

        internal void OnScheduleDynamicallyChanged(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:ScheduleDynamicallyChanged event raised for instance Id {0}", schedule.InstanceId);
            _OnServiceEvent(schedule, false, WorkflowDynamicallyChanged);
        }

        internal void OnSchedulePersisted(WorkflowExecutor schedule)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime:SchedulePersisted event raised for instance Id {0}", schedule.InstanceId);

            _OnServiceEvent(schedule, false, WorkflowPersisted);
        }

        private void _OnServiceEvent(WorkflowExecutor sched, bool unregister, EventHandler<WorkflowEventArgs> handler)
        {
            Debug.Assert(sched != null);
            try
            {
                WorkflowEventArgs args = new WorkflowEventArgs(sched.WorkflowInstance);
                //Notify Subscribers
                if (handler != null) handler(this, args);
            }
            catch (Exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime:OnService Event threw an exception.");
                throw;
            }
            finally
            {
                if (unregister)
                {
                    _unRegister(sched);
                }
            }
        }

        internal void RaiseServicesExceptionNotHandledEvent(Exception exception, Guid instanceId)
        {
            VerifyInternalState();
            WorkflowTrace.Host.TraceEvent(TraceEventType.Critical, 0, "WorkflowRuntime:ServicesExceptionNotHandled event raised for instance Id {0} {1}", instanceId, exception.ToString());
            EventHandler<ServicesExceptionNotHandledEventArgs> handler = ServicesExceptionNotHandled;
            if (handler != null)
                handler(this, new ServicesExceptionNotHandledEventArgs(exception, instanceId));
        }

        #endregion

        #region More service accessors

        private Dictionary<Type, List<object>> _services = new Dictionary<Type, List<object>>();
        private string _name;
        private bool _startedServices;
        private NameValueConfigurationCollection _configurationParameters;
        private Dictionary<string, Type> _trackingServiceReplacement;


        /// <summary> The name of this container. </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                lock (_startStopLock)
                {
                    if (_startedServices)
                        throw new InvalidOperationException(ExecutionStringManager.CantChangeNameAfterStart);
                    VerifyInternalState();
                    _name = value;
                }
            }
        }

        /// <summary>
        /// Returns the configuration parameters that can be shared by all services
        /// </summary>
        internal NameValueConfigurationCollection CommonParameters
        {
            get
            {
                return _configurationParameters;
            }
        }

        // A previous tracking service whose type has the string as its AssemblyQualifiedName
        // will be replaced by the current tracking service of the Type. This dictionary is 
        // neede in order to replace the previous tracking service used by a persisted workflow
        // because what is persisted is the one-way hashed string of that AssemblyQualifiedName.
        internal Dictionary<string, Type> TrackingServiceReplacement
        {
            get
            {
                return _trackingServiceReplacement;
            }
        }


        /// <summary> Adds a service to this container. </summary>
        /// <param name="service"> The service to add </param>
        /// <exception cref="InvalidOperationException"/>
        public void AddService(object service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            VerifyInternalState();

            using (new WorkflowRuntime.EventContext())
            {
                lock (_startStopLock)
                {
                    AddServiceImpl(service);
                }
            }
        }

        private void AddServiceImpl(object service)
        {
            //ASSERT: _startStopLock is held

            lock (_servicesLock)
            {
                if (GetAllServices(service.GetType()).Contains(service))
                    throw new InvalidOperationException(ExecutionStringManager.CantAddServiceTwice);

                if (_startedServices && IsCoreService(service))
                    throw new InvalidOperationException(ExecutionStringManager.CantChangeImmutableContainer);

                Type basetype = service.GetType();
                if (basetype.IsSubclassOf(typeof(TrackingService)))
                {
                    AddTrackingServiceReplacementInfo(basetype);
                }


                foreach (Type t in basetype.GetInterfaces())
                {
                    List<object> al;
                    if (_services.ContainsKey(t))
                    {
                        al = _services[t];
                    }
                    else
                    {
                        al = new List<object>();
                        _services.Add(t, al);
                    }
                    al.Add(service);
                }

                while (basetype != null)
                {
                    List<object> al = null;
                    if (_services.ContainsKey(basetype))
                    {
                        al = _services[basetype];
                    }
                    else
                    {
                        al = new List<object>();
                        _services.Add(basetype, al);
                    }
                    al.Add(service);
                    basetype = basetype.BaseType;
                }
            }

            WorkflowRuntimeService wrs = service as WorkflowRuntimeService;
            if (wrs != null)
            {
                wrs.SetRuntime(this);
                if (_startedServices)
                    wrs.Start();
            }
        }


        /// <summary> Removes a service. </summary>
        /// <param name="service"> The service to remove </param>
        public void RemoveService(object service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            VerifyInternalState();

            using (new WorkflowRuntime.EventContext())
            {
                lock (_startStopLock)
                {
                    lock (_servicesLock)
                    {
                        if (_startedServices && IsCoreService(service))
                            throw new InvalidOperationException(ExecutionStringManager.CantChangeImmutableContainer);

                        if (!GetAllServices(service.GetType()).Contains(service))
                            throw new InvalidOperationException(ExecutionStringManager.CantRemoveServiceNotContained);

                        Type type = service.GetType();
                        if (type.IsSubclassOf(typeof(TrackingService)))
                        {
                            RemoveTrackingServiceReplacementInfo(type);
                        }

                        foreach (List<object> al in _services.Values)
                        {
                            if (al.Contains(service))
                            {
                                al.Remove(service);
                            }
                        }
                    }
                    WorkflowRuntimeService wrs = service as WorkflowRuntimeService;
                    if (wrs != null)
                    {
                        if (_startedServices)
                            wrs.Stop();

                        wrs.SetRuntime(null);
                    }
                }
            }
        }

        private void AddTrackingServiceReplacementInfo(Type type)
        {
            Debug.Assert(type.IsSubclassOf(typeof(TrackingService)), "Argument should be a subtype of TrackingService");
            object[] attributes = type.GetCustomAttributes(typeof(PreviousTrackingServiceAttribute), true);
            if (attributes != null && attributes.Length > 0)
            {
                foreach (object attribute in attributes)
                {
                    if (_trackingServiceReplacement == null)
                    {
                        _trackingServiceReplacement = new Dictionary<string, Type>();
                    }
                    _trackingServiceReplacement.Add(((PreviousTrackingServiceAttribute)attribute).AssemblyQualifiedName, type);
                }
            }
        }

        private void RemoveTrackingServiceReplacementInfo(Type type)
        {
            Debug.Assert(type.IsSubclassOf(typeof(TrackingService)), "Argument should be a subtype of TrackingService");
            object[] attributes = type.GetCustomAttributes(typeof(PreviousTrackingServiceAttribute), true);
            if (attributes != null && attributes.Length > 0)
            {
                foreach (object attribute in attributes)
                {
                    string previousTrackingService = ((PreviousTrackingServiceAttribute)attribute).AssemblyQualifiedName;
                    if (_trackingServiceReplacement.ContainsKey(previousTrackingService))
                    {
                        _trackingServiceReplacement.Remove(previousTrackingService);
                    }
                }
            }
        }

        private bool IsCoreService(object service)
        {
            return service is WorkflowSchedulerService
                || service is WorkflowPersistenceService
                || service is TrackingService
                || service is WorkflowCommitWorkBatchService
                || service is WorkflowLoaderService;
        }


        /// <summary> Returns a collection of all services that implement the give type. </summary>
        /// <param name="serviceType"> The type to look for </param>
        /// <returns> A collection of zero or more services </returns>
        public ReadOnlyCollection<object> GetAllServices(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            VerifyInternalState();

            lock (_servicesLock)
            {
                List<object> retval = new List<object>();
                if (_services.ContainsKey(serviceType))
                    retval.AddRange(_services[serviceType]);

                return new ReadOnlyCollection<object>(retval);
            }
        }


        public T GetService<T>()
        {
            VerifyInternalState();
            return (T)GetService(typeof(T));
        }

        public ReadOnlyCollection<T> GetAllServices<T>()
        {
            VerifyInternalState();
            List<T> l = new List<T>();
            foreach (T t in GetAllServices(typeof(T)))
                l.Add(t);
            return new ReadOnlyCollection<T>(l);
        }

        /// <summary> Looks for a service of the given type. </summary>
        /// <param name="serviceType"> The type of object to find </param>
        /// <returns> An object of the requested type, or null</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            VerifyInternalState();

            lock (_servicesLock)
            {
                object retval = null;

                if (_services.ContainsKey(serviceType))
                {
                    List<object> al = _services[serviceType];

                    if (al.Count > 1)
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                            ExecutionStringManager.MoreThanOneService, serviceType.ToString()));

                    if (al.Count == 1)
                        retval = al[0];
                }

                return retval;
            }
        }

        #endregion

        #region Other methods


        /// <summary> Raises the Starting event </summary>
        /// <remarks>
        /// </remarks>
        public void StartRuntime()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Starting WorkflowRuntime {0}", _uid);
            lock (_startStopLock)
            {
                VerifyInternalState();

                if (!_startedServices)
                {

                    if (GetAllServices(typeof(WorkflowCommitWorkBatchService)).Count == 0)
                        AddServiceImpl(new DefaultWorkflowCommitWorkBatchService());

                    if (GetAllServices(typeof(WorkflowSchedulerService)).Count == 0)
                        AddServiceImpl(new DefaultWorkflowSchedulerService());

                    if (GetAllServices(typeof(WorkflowLoaderService)).Count == 0)
                        AddServiceImpl(new DefaultWorkflowLoaderService());

                    if (GetAllServices(typeof(WorkflowCommitWorkBatchService)).Count != 1)
                        throw new InvalidOperationException(String.Format(
                        CultureInfo.CurrentCulture,
                        ExecutionStringManager.InvalidWorkflowRuntimeConfiguration,
                        typeof(WorkflowCommitWorkBatchService).Name));

                    if (GetAllServices(typeof(WorkflowSchedulerService)).Count != 1)
                        throw new InvalidOperationException(String.Format(
                        CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration,
                        typeof(WorkflowSchedulerService).Name));

                    if (GetAllServices(typeof(WorkflowLoaderService)).Count != 1)
                        throw new InvalidOperationException(String.Format(
                        CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration,
                        typeof(WorkflowLoaderService).Name));

                    if (GetAllServices(typeof(WorkflowPersistenceService)).Count > 1)
                        throw new InvalidOperationException(String.Format(
                        CultureInfo.CurrentCulture, ExecutionStringManager.InvalidWorkflowRuntimeConfiguration,
                        typeof(WorkflowPersistenceService).Name));

                    if (GetAllServices(typeof(WorkflowTimerService)).Count == 0)
                    {
                        AddServiceImpl(new WorkflowTimerService());
                    }

                    //Mark this instance has started
                    isInstanceStarted = true;

                    //Set up static tracking structures
                    _trackingFactory.Initialize(this);
                    if (this.PerformanceCounterManager != null)
                    {
                        this.PerformanceCounterManager.Initialize(this);
                        this.PerformanceCounterManager.SetInstanceName(this.Name);
                    }

                    foreach (WorkflowRuntimeService s in GetAllServices<WorkflowRuntimeService>())
                    {
                        s.Start();
                    }

                    _startedServices = true;
                    using (new WorkflowRuntime.EventContext())
                    {
                        EventHandler<WorkflowRuntimeEventArgs> ss = Started;
                        if (ss != null)
                            ss(this, new WorkflowRuntimeEventArgs(isInstanceStarted));
                    }
                }
            }
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Started WorkflowRuntime {0}", _uid);
        }

        void DynamicUpdateCommit(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InvalidArgumentType, "sender", typeof(WorkflowExecutor).ToString()));

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            OnScheduleDynamicallyChanged(exec);
        }

        internal void WorkflowExecutorCreated(WorkflowExecutor workflowExecutor, bool loaded)
        {
            //
            // Fire the event for all other components that need to register for notification of WorkflowExecutor events
            EventHandler<WorkflowExecutorInitializingEventArgs> localEvent = WorkflowExecutorInitializing;
            if (null != localEvent)
                localEvent(workflowExecutor, new WorkflowExecutorInitializingEventArgs(loaded));

            workflowExecutor.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(WorkflowExecutionEvent);
        }

        void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            switch (e.EventType)
            {
                case WorkflowEventInternal.Idle:
                    OnIdle(exec);
                    break;
                case WorkflowEventInternal.Created:
                    if (WorkflowCreated != null)
                        WorkflowCreated(this, new WorkflowEventArgs(exec.WorkflowInstance));
                    break;
                case WorkflowEventInternal.Started:
                    if (WorkflowStarted != null)
                        WorkflowStarted(this, new WorkflowEventArgs(exec.WorkflowInstance));
                    break;
                case WorkflowEventInternal.Loaded:
                    OnScheduleLoaded(exec);
                    break;
                case WorkflowEventInternal.Unloaded:
                    OnScheduleUnloaded(exec);
                    break;
                case WorkflowEventInternal.Completed:
                    OnScheduleCompleted(exec, CreateCompletedEventArgs(exec));
                    break;
                case WorkflowEventInternal.Terminated:
                    WorkflowExecutor.WorkflowExecutionTerminatedEventArgs args = (WorkflowExecutor.WorkflowExecutionTerminatedEventArgs)e;

                    if (null != args.Exception)
                        OnScheduleTerminated(exec, new WorkflowTerminatedEventArgs(exec.WorkflowInstance, args.Exception));
                    else
                        OnScheduleTerminated(exec, new WorkflowTerminatedEventArgs(exec.WorkflowInstance, args.Error));

                    break;
                case WorkflowEventInternal.Aborted:
                    OnScheduleAborted(exec);
                    break;
                case WorkflowEventInternal.Suspended:
                    WorkflowExecutor.WorkflowExecutionSuspendedEventArgs sargs = (WorkflowExecutor.WorkflowExecutionSuspendedEventArgs)e;
                    OnScheduleSuspended(exec, new WorkflowSuspendedEventArgs(exec.WorkflowInstance, sargs.Error));
                    break;
                case WorkflowEventInternal.Persisted:
                    OnSchedulePersisted(exec);
                    break;
                case WorkflowEventInternal.Resumed:
                    OnScheduleResumed(exec);
                    break;
                case WorkflowEventInternal.DynamicChangeCommit:
                    DynamicUpdateCommit(exec, (WorkflowExecutor.DynamicUpdateEventArgs)e);
                    break;
                default:
                    break;
            }
        }

        private WorkflowCompletedEventArgs CreateCompletedEventArgs(WorkflowExecutor exec)
        {
            WorkflowCompletedEventArgs args = new WorkflowCompletedEventArgs(exec.WorkflowInstance, exec.WorkflowDefinition);
            foreach (PropertyInfo property in _workflowDefinitionDispenser.GetOutputParameters(exec.RootActivity))
                args.OutputParameters.Add(property.Name, property.GetValue(exec.RootActivity, null));

            return args;
        }

        private void StopServices()
        {
            // Stop remaining services
            foreach (WorkflowRuntimeService s in GetAllServices<WorkflowRuntimeService>())
            {
                s.Stop();
            }
        }

        /// <summary> Fires the Stopping event </summary>
        public void StopRuntime()
        {
            VerifyInternalState();

            using (new WorkflowRuntime.EventContext())
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Stopping WorkflowRuntime {0}", _uid);
                lock (_startStopLock)
                {
                    if (_startedServices)
                    {
                        try
                        {
                            isInstanceStarted = false;

                            if (this.WorkflowPersistenceService != null)
                            {
                                //
                                // GetWorkflowExecutors() takes a lock on workflowExecutors
                                // and then returns a copy of the list.  As long as GetWorkflowExecutors()
                                // returns a non empty/null list we'll attempt to unload what's in it.
                                IList<WorkflowExecutor> executors = GetWorkflowExecutors();
                                while ((null != executors) && (executors.Count > 0))
                                {
                                    foreach (WorkflowExecutor executor in executors)
                                    {
                                        if (executor.IsInstanceValid)
                                        {
                                            try
                                            {
                                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Calling Unload on instance {0} executor hc {1}", executor.InstanceIdString, executor.GetHashCode());
                                                executor.Unload();
                                            }
                                            catch (ExecutorLocksHeldException)
                                            {
                                                //
                                                // This exception means that an atomic scope is ongoing
                                                // (we cannot unload/suspend during an atomic scope)
                                                // This instance will still be in the GetWorkflowExecutors list
                                                // so we'll attempt to unload it on the next outer loop
                                                // Yes, we may loop indefinitely if an atomic tx is hung
                                                // See WorkflowInstance.Unload for an example of retrying
                                                // when this exception is thrown.
                                            }
                                            catch (InvalidOperationException)
                                            {
                                                if (executor.IsInstanceValid)
                                                {
                                                    //
                                                    // Failed to stop, reset the flag
                                                    isInstanceStarted = true;
                                                    throw;
                                                }
                                            }
                                            catch
                                            {
                                                //
                                                // Failed to stop, reset the flag
                                                isInstanceStarted = true;
                                                throw;
                                            }
                                        }
                                    }
                                    //
                                    // Check if anything was added to the main list  
                                    // while we were working on the copy.
                                    // This happens if a executor reverts to a checkpoint.
                                    // There is the potential to loop indefinitely if
                                    // an instance continually reverts.
                                    executors = GetWorkflowExecutors();
                                }
                            }

                            StopServices();
                            _startedServices = false;


                            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime: Stopped WorkflowRuntime {0}", _uid);

                            //
                            // Clean up tracking
                            _trackingFactory.Uninitialize(this);
                            if (this.PerformanceCounterManager != null)
                            {
                                this.PerformanceCounterManager.Uninitialize(this);
                            }

                            EventHandler<WorkflowRuntimeEventArgs> handler = Stopped;
                            if (handler != null)
                                handler(this, new WorkflowRuntimeEventArgs(isInstanceStarted));
                        }
                        catch (Exception)
                        {
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "WorkflowRuntime::StartUnload Unexpected Exception");
                            throw;
                        }
                        finally
                        {
                            isInstanceStarted = false;
                        }
                    }
                }
            }
        }

        /// <summary> True if services have been started and not stopped </summary>
        public bool IsStarted
        {
            get
            {
                return _startedServices;
            }
        }


        private static Activity OnActivityDefinitionResolve(object sender, ActivityResolveEventArgs e)
        {
            WorkflowRuntime runtime = e.ServiceProvider as WorkflowRuntime;
            if (runtime == null)
                runtime = RuntimeEnvironment.CurrentRuntime;

            Debug.Assert(runtime != null);
            if (runtime != null)
            {
                if (e.Type != null)
                    return runtime._workflowDefinitionDispenser.GetRootActivity(e.Type, e.CreateNewDefinition, e.InitializeForRuntime);
                else
                    return runtime._workflowDefinitionDispenser.GetRootActivity(e.WorkflowMarkup, e.RulesMarkup, e.CreateNewDefinition, e.InitializeForRuntime);
            }
            return null;
        }

        internal static TypeProvider CreateTypeProvider(Activity rootActivity)
        {
            TypeProvider typeProvider = new TypeProvider(null);

            Type companionType = rootActivity.GetType();
            typeProvider.SetLocalAssembly(companionType.Assembly);
            typeProvider.AddAssembly(companionType.Assembly);

            foreach (AssemblyName assemblyName in companionType.Assembly.GetReferencedAssemblies())
            {
                Assembly referencedAssembly = null;
                try
                {
                    referencedAssembly = Assembly.Load(assemblyName);
                    if (referencedAssembly != null)
                        typeProvider.AddAssembly(referencedAssembly);
                }
                catch
                {
                }

                if (referencedAssembly == null && assemblyName.CodeBase != null)
                    typeProvider.AddAssemblyReference(assemblyName.CodeBase);
            }

            return typeProvider;
        }

        private static ArrayList OnWorkflowChangeActionsResolve(object sender, WorkflowChangeActionsResolveEventArgs e)
        {
            ArrayList changes = null;
            WorkflowRuntime runtime = RuntimeEnvironment.CurrentRuntime;
            Debug.Assert(runtime != null);
            if (runtime != null)
            {
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                ServiceContainer serviceContainer = new ServiceContainer();
                ITypeProvider typeProvider = runtime.GetService<ITypeProvider>();
                if (typeProvider != null)
                    serviceContainer.AddService(typeof(ITypeProvider), typeProvider);
                else if (sender is Activity)
                {
                    serviceContainer.AddService(typeof(ITypeProvider), CreateTypeProvider(sender as Activity));
                }

                DesignerSerializationManager manager = new DesignerSerializationManager(serviceContainer);
                using (manager.CreateSession())
                {
                    using (StringReader reader = new StringReader(e.WorkflowChangesMarkup))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(reader))
                        {
                            WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                            changes = serializer.Deserialize(xomlSerializationManager, xmlReader) as ArrayList;
                        }
                    }
                }
            }

            return changes;
        }

        /// <summary> Creates and adds a service to this container. </summary>
        /// <param name="serviceSettings"> Description of the service to add. </param>
        private void AddServiceFromSettings(WorkflowRuntimeServiceElement serviceSettings)
        {
            object service = null;

            Type t = Type.GetType(serviceSettings.Type, true);

            ConstructorInfo serviceProviderAndSettingsConstructor = null;
            ConstructorInfo serviceProviderConstructor = null;
            ConstructorInfo settingsConstructor = null;

            foreach (ConstructorInfo ci in t.GetConstructors())
            {
                ParameterInfo[] pi = ci.GetParameters();
                if (pi.Length == 1)
                {
                    if (typeof(IServiceProvider).IsAssignableFrom(pi[0].ParameterType))
                    {
                        serviceProviderConstructor = ci;
                    }
                    else if (typeof(NameValueCollection).IsAssignableFrom(pi[0].ParameterType))
                    {
                        settingsConstructor = ci;
                    }
                }
                else if (pi.Length == 2)
                {
                    if (typeof(IServiceProvider).IsAssignableFrom(pi[0].ParameterType)
                        && typeof(NameValueCollection).IsAssignableFrom(pi[1].ParameterType))
                    {
                        serviceProviderAndSettingsConstructor = ci;
                        break;
                    }
                }
            }

            if (serviceProviderAndSettingsConstructor != null)
            {
                service = serviceProviderAndSettingsConstructor.Invoke(
                    new object[] { this, serviceSettings.Parameters });
            }
            else if (serviceProviderConstructor != null)
            {
                service = serviceProviderConstructor.Invoke(new object[] { this });
            }
            else if (settingsConstructor != null)
            {
                service = settingsConstructor.Invoke(new object[] { serviceSettings.Parameters });
            }
            else
            {
                service = Activator.CreateInstance(t);
            }

            AddServiceImpl(service);
        }

        internal static void ClearTrackingProfileCache()
        {
            lock (_runtimesLock)
            {
                foreach (WeakReference wr in _runtimes.Values)
                {
                    WorkflowRuntime runtime = wr.Target as WorkflowRuntime;
                    if (null != runtime)
                    {
                        if ((null != runtime.TrackingListenerFactory) && (null != runtime.TrackingListenerFactory.TrackingProfileManager))
                            runtime.TrackingListenerFactory.TrackingProfileManager.ClearCacheImpl();
                    }
                }
            }
        }
        /// <summary>Utility class that prevents reentrance during event processing.</summary>
        /// <remarks>
        /// When created an EventContext it creates a static variable local to 
        /// a managed thread (similar to the old TLS slot), 
        /// which can detect cases when events are invoked while handling other events. 
        /// The variable is removed on dispose.
        /// </remarks>
        internal sealed class EventContext : IDisposable
        {

            /// <summary>
            /// Indicates that the value of a static field is unique for each thread
            /// CLR Perf suggests using this attribute over the slot approach.
            /// </summary>
            [ThreadStatic()]
            static object threadData;

            public EventContext(params Object[] ignored)
            {
                if (threadData != null)
                    throw new InvalidOperationException(ExecutionStringManager.CannotCauseEventInEvent);

                threadData = this;
            }

            void IDisposable.Dispose()
            {
                Debug.Assert(threadData != null, "unexpected call to EventContext::Dispose method");
                threadData = null;
            }
        }

        #endregion

        #region WorkflowExecutor utility methods
        private IList<WorkflowExecutor> GetWorkflowExecutors()
        {
            //
            // This is a safety check in to avoid returning invalid executors in the following cases:
            // 1.  We ---- between the executor going invalid and getting removed from the list.
            // 2.  We have a leak somewhere where invalid executors are not getting removed from the list.
            List<WorkflowExecutor> executorsList = new List<WorkflowExecutor>();
            foreach (Dictionary<Guid, WorkflowExecutor> executors in workflowExecutors)
            {
                lock (executors)
                {
                    foreach (WorkflowExecutor executor in executors.Values)
                    {
                        if ((null != executor) && (executor.IsInstanceValid))
                            executorsList.Add(executor);
                    }
                }
            }
            return executorsList;
        }

        private bool TryRemoveWorkflowExecutor(Guid instanceId, WorkflowExecutor executor)
        {
            Dictionary<Guid, WorkflowExecutor> executors = workflowExecutors[instanceId];
            lock (executors)
            {
                WorkflowExecutor currentRes;
                if (executors.TryGetValue(instanceId, out currentRes) && Object.Equals(executor, currentRes))
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "WorkflowRuntime::TryRemoveWorkflowExecutor, instance:{0}, hc:{1}", executor.InstanceIdString, executor.GetHashCode());
                    return executors.Remove(instanceId);
                }

                return false;
            }
        }
        #endregion
    }
}
