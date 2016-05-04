using System;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using System.Security.Permissions;
using System.Security.Cryptography;

using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Workflow.Runtime.Tracking;

namespace System.Workflow.Runtime
{
    /// <summary>
    /// Creates TrackingListener instances
    /// </summary>
    internal class TrackingListenerFactory
    {
        private List<TrackingService> _services = null;
        private bool _initialized = false;
        private Dictionary<Guid, WeakReference> _listeners = new Dictionary<Guid, WeakReference>();
        private volatile object _listenerLock = new object();

        private System.Timers.Timer _timer = null;
        private double _interval = 60000;

        private TrackingProfileManager _profileManager = new TrackingProfileManager();

        internal TrackingListenerFactory()
        {
        }

        internal TrackingProfileManager TrackingProfileManager
        {
            get { return _profileManager; }
        }
        /// <summary>
        /// Must be called 
        /// </summary>
        /// <param name="skedExec"></param>
        internal void Initialize(WorkflowRuntime runtime)
        {
            lock (this)
            {
                _services = runtime.TrackingServices;
                _profileManager.Initialize(runtime);
                runtime.WorkflowExecutorInitializing += WorkflowExecutorInitializing;

                _timer = new System.Timers.Timer();
                _timer.Interval = _interval;
                _timer.AutoReset = false; // ensure that only one timer thread at a time
                _timer.Elapsed += new ElapsedEventHandler(Cleanup);
                _timer.Start();

                _initialized = true;
            }
        }

        /// <summary>
        /// Clean up static state created in Initialize
        /// </summary>
        internal void Uninitialize(WorkflowRuntime runtime)
        {
            lock (this)
            {
                _profileManager.Uninitialize();
                runtime.WorkflowExecutorInitializing -= WorkflowExecutorInitializing;
                _timer.Elapsed -= new ElapsedEventHandler(Cleanup);
                _timer.Stop();
                _services = null;
                _initialized = false;

                _timer.Dispose();
                _timer = null;
            }
        }
        /// <summary>
        /// Callback for associating tracking listeners to in memory instances.  Fires for new and loading instances.
        /// </summary>
        /// <param name="sender">WorkflowExecutor</param>
        /// <param name="e"></param>
        void WorkflowExecutorInitializing(object sender, WorkflowRuntime.WorkflowExecutorInitializingEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (null == e)
                throw new ArgumentNullException("e");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            WorkflowExecutor exec = (WorkflowExecutor)sender;
            //
            // Add an event to clean up the WeakRef entry
            exec.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(WorkflowExecutionEvent);
            TrackingCallingState trackingCallingState = exec.TrackingCallingState;
            TrackingListenerBroker listenerBroker = (TrackingListenerBroker)exec.RootActivity.GetValue(WorkflowExecutor.TrackingListenerBrokerProperty);
            if (listenerBroker != null)
            {
                listenerBroker.ReplaceServices(exec.WorkflowRuntime.TrackingServiceReplacement);
            }
            TrackingListener listener = null;
            //
            // Check if we still have a weakref to the listener for this instance
            WeakReference weakref = null;
            if (e.Loading)
            {
                bool found = false;
                lock (_listenerLock)
                {
                    found = _listeners.TryGetValue(exec.InstanceId, out weakref);
                }
                if (found)
                {
                    try
                    {
                        // 
                        // Instead of checking IsAlive take a ref to the Target
                        //  so that it isn't GC'd underneath us.
                        listener = weakref.Target as TrackingListener;
                    }
                    catch (InvalidOperationException)
                    {
                        //
                        // This seems weird but according to msdn 
                        // accessing Target can throw ???
                        // Ignore because it's the same as a null target.
                    }
                }
                //
                // If listener is null because we didn't find the wr in the cache 
                // or because the Target has been GC'd create a new listener
                if (null != listener)
                {
                    listener.Broker = listenerBroker;
                }
                else
                {
                    Debug.Assert(null != listenerBroker, "TrackingListenerBroker should not be null during loading");
                    listener = GetTrackingListener(exec.WorkflowDefinition, exec, listenerBroker);
                    if (null != listener)
                    {
                        if (null != weakref)
                            weakref.Target = listener;
                        else
                        {
                            lock (_listenerLock)
                            {
                                _listeners.Add(exec.ID, new WeakReference(listener));
                            }
                        }
                    }
                }
            }
            else
            {
                //
                // New instance is being created
                listener = GetTrackingListener(exec.WorkflowDefinition, exec);

                if (null != listener)
                {
                    exec.RootActivity.SetValue(WorkflowExecutor.TrackingListenerBrokerProperty, listener.Broker);
                    lock (_listenerLock)
                    {
                        _listeners.Add(exec.ID, new WeakReference(listener));
                    }
                }
                else
                    exec.RootActivity.SetValue(WorkflowExecutor.TrackingListenerBrokerProperty, new TrackingListenerBroker());
            }

            if (null != listener)
            {
                exec.WorkflowExecutionEvent += new EventHandler<WorkflowExecutor.WorkflowExecutionEventArgs>(listener.WorkflowExecutionEvent);
            }

        }

        void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            switch (e.EventType)
            {
                case WorkflowEventInternal.Aborted:
                case WorkflowEventInternal.Completed:
                case WorkflowEventInternal.Terminated:
                    //
                    // The instance is done - remove 
                    // the WeakRef from our list
                    WorkflowExecutor exec = (WorkflowExecutor)sender;
                    lock (_listenerLock)
                    {
                        _listeners.Remove(exec.ID);
                    }
                    break;
                default:
                    return;
            }
        }

        void Cleanup(object sender, ElapsedEventArgs e)
        {
            List<Guid> _toRemove = new List<Guid>();
            if ((null != _listeners) || (_listeners.Count > 0))
            {
                lock (_listenerLock)
                {
                    foreach (KeyValuePair<Guid, WeakReference> kvp in _listeners)
                    {
                        if (null == kvp.Value.Target)
                            _toRemove.Add(kvp.Key);
                    }
                    if (_toRemove.Count > 0)
                    {
                        foreach (Guid g in _toRemove)
                            _listeners.Remove(g);
                    }
                }
            }

            lock (this)
            {
                if (_timer != null)
                {
                    _timer.Start();
                }
            }
        }

        /// <summary>
        /// Return a tracking listener for a new instance
        /// </summary>
        /// <param name="sked">SequentialWorkflow for which the tracking listener will be associated</param>
        /// <param name="skedExec">ScheduleExecutor for the schedule instance</param>
        /// <returns>New TrackingListener instance</returns>
        internal TrackingListener GetTrackingListener(Activity sked, WorkflowExecutor skedExec)
        {
            if (!_initialized)
                Initialize(skedExec.WorkflowRuntime);

            return GetListener(sked, skedExec, null);
        }

        /// <summary>
        /// Return a tracking listener for an existing instance (normally used during loading)
        /// </summary>
        /// <param name="sked">SequentialWorkflow for which the tracking listener will be associated</param>
        /// <param name="skedExec">ScheduleExecutor for the schedule instance</param>
        /// <param name="broker">TrackingListenerBroker</param>
        /// <returns>New TrackingListener instance</returns>
        internal TrackingListener GetTrackingListener(Activity sked, WorkflowExecutor skedExec, TrackingListenerBroker broker)
        {
            if (!_initialized)
                Initialize(skedExec.WorkflowRuntime);

            if (null == broker)
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullTrackingBroker);
                return null;
            }

            return GetListener(sked, skedExec, broker);
        }

        private TrackingListener GetListenerFromWRCache(Guid instanceId)
        {
            WeakReference wr = null;
            TrackingListener listener = null;
            lock (_listenerLock)
            {
                if (!_listeners.TryGetValue(instanceId, out wr))
                    throw new InvalidOperationException(string.Format(System.Globalization.CultureInfo.InvariantCulture, ExecutionStringManager.ListenerNotInCache, instanceId));

                listener = wr.Target as TrackingListener;

                if (null == listener)
                    throw new ObjectDisposedException(string.Format(System.Globalization.CultureInfo.InvariantCulture, ExecutionStringManager.ListenerNotInCacheDisposed, instanceId));
            }

            return listener;
        }

        internal void ReloadProfiles(WorkflowExecutor exec)
        {
            // Keep control events from other threads out
            using (new ServiceEnvironment(exec.RootActivity))
            {
                using (exec.ExecutorLock.Enter())
                {
                    // check if this is a valid in-memory instance
                    if (!exec.IsInstanceValid)
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowNotValid);

                    // suspend the instance
                    bool localSuspend = exec.Suspend(ExecutionStringManager.TrackingProfileUpdate);
                    try
                    {
                        //
                        // Get new profiles
                        TrackingListener listener = GetListenerFromWRCache(exec.InstanceId);
                        listener.ReloadProfiles(exec, exec.InstanceId);
                    }
                    finally
                    {
                        if (localSuspend)
                        {
                            // @undone: for now this will not return till the instance is done
                            // Once Kumar has fixed 4335, we can enable this.
                            exec.Resume();
                        }
                    }
                }
            }
        }

        internal void ReloadProfiles(WorkflowExecutor exec, Guid instanceId, ref TrackingListenerBroker broker, ref List<TrackingChannelWrapper> channels)
        {
            Type workflowType = exec.WorkflowDefinition.GetType();
            //
            // Ask every tracking service if they want to reload
            // even if they originally returned null for a profile
            foreach (TrackingService service in _services)
            {
                TrackingProfile profile = null;
                TrackingChannelWrapper w = null;
                //
                // Check if the service wants to reload a profile
                if (service.TryReloadProfile(workflowType, instanceId, out profile))
                {
                    bool found = false;
                    int i;
                    for (i = 0; i < channels.Count; i++)
                    {
                        if (service.GetType() == channels[i].TrackingServiceType)
                        {
                            w = channels[i];
                            found = true;
                            break;
                        }
                    }
                    //
                    // If we don't have a profile, remove what we had for this service type (if anything)
                    if (null == profile)
                    {
                        if (found)
                        {
                            broker.RemoveService(w.TrackingServiceType);
                            channels.RemoveAt(i);
                        }
                        continue;
                    }
                    //
                    // Parse the new profile - instance only, the cache is not involved
                    RTTrackingProfile rtp = new RTTrackingProfile(profile, exec.WorkflowDefinition, workflowType);
                    rtp.IsPrivate = true;

                    if (!found)
                    {
                        //
                        // This is a new profile, create new channel, channelwrapper and broker item
                        List<string> activityCallPath = null;
                        Guid callerInstanceId = Guid.Empty;
                        TrackingCallingState trackingCallingState = exec.TrackingCallingState;
                        Debug.Assert((null != trackingCallingState), "WorkflowState is null");
                        IList<string> path = null;
                        Guid context = GetContext(exec.RootActivity), callerContext = Guid.Empty, callerParentContext = Guid.Empty;
                        //
                        // Use CallerActivityPathProxy to determine if this is an invoked instance
                        if (trackingCallingState != null)
                        {
                            path = trackingCallingState.CallerActivityPathProxy;
                            if ((null != path) && (path.Count > 0))
                            {
                                activityCallPath = new List<string>(path);

                                Debug.Assert(Guid.Empty != trackingCallingState.CallerWorkflowInstanceId, "Instance has an ActivityCallPath but CallerInstanceId is empty");
                                callerInstanceId = trackingCallingState.CallerWorkflowInstanceId;

                                callerContext = trackingCallingState.CallerContextGuid;
                                callerParentContext = trackingCallingState.CallerParentContextGuid;
                            }
                        }

                        TrackingParameters tp = new TrackingParameters(instanceId, workflowType, exec.WorkflowDefinition, activityCallPath, callerInstanceId, context, callerContext, callerParentContext);
                        TrackingChannel channel = service.GetTrackingChannel(tp);

                        TrackingChannelWrapper wrapper = new TrackingChannelWrapper(channel, service.GetType(), workflowType, rtp);
                        channels.Add(wrapper);

                        Type t = service.GetType();
                        broker.AddService(t, rtp.Version);
                        broker.MakeProfileInstance(t);
                    }
                    else
                    {
                        //
                        // Don't need to call MakeProfilePrivate on the wrapper
                        // because we've already marked it as private and we already
                        // have a private copy of it.
                        //w.MakeProfilePrivate( exec );
                        w.SetTrackingProfile(rtp);
                        broker.MakeProfileInstance(w.TrackingServiceType);
                    }
                }
            }
        }

        internal Guid GetContext(Activity activity)
        {
            return ((ActivityExecutionContextInfo)ContextActivityUtils.ContextActivity(activity).GetValue(Activity.ActivityExecutionContextInfoProperty)).ContextGuid;
        }

        private TrackingListener GetListener(Activity sked, WorkflowExecutor skedExec, TrackingListenerBroker broker)
        {
            if ((null == sked) || (null == skedExec))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullParameters);
                return null;
            }

            if ((null == _services) || (_services.Count <= 0))
                return null;

            bool load = (null != broker);

            List<TrackingChannelWrapper> channels = GetChannels(sked, skedExec, skedExec.InstanceId, sked.GetType(), ref broker);

            if ((null == channels) || (0 == channels.Count))
                return null;

            return new TrackingListener(this, sked, skedExec, channels, broker, load);
        }

        private List<TrackingChannelWrapper> GetChannels(Activity schedule, WorkflowExecutor exec, Guid instanceID, Type workflowType, ref TrackingListenerBroker broker)
        {
            if (null == _services)
                return null;

            bool initBroker = false;
            if (null == broker)
            {
                broker = new TrackingListenerBroker();
                initBroker = true;
            }

            List<TrackingChannelWrapper> channels = new List<TrackingChannelWrapper>();

            List<string> activityCallPath = null;
            Guid callerInstanceId = Guid.Empty;
            Guid context = GetContext(exec.RootActivity), callerContext = Guid.Empty, callerParentContext = Guid.Empty;

            Debug.Assert(exec is WorkflowExecutor, "Executor is not WorkflowExecutor");
            TrackingCallingState trackingCallingState = exec.TrackingCallingState;
            TrackingListenerBroker trackingListenerBroker = (TrackingListenerBroker)exec.RootActivity.GetValue(WorkflowExecutor.TrackingListenerBrokerProperty);
            IList<string> path = trackingCallingState != null ? trackingCallingState.CallerActivityPathProxy : null;
            //
            // Use CallerActivityPathProxy to determine if this is an invoked instance
            if ((null != path) && (path.Count > 0))
            {
                activityCallPath = new List<string>(path);

                Debug.Assert(Guid.Empty != trackingCallingState.CallerWorkflowInstanceId, "Instance has an ActivityCallPath but CallerInstanceId is empty");
                callerInstanceId = trackingCallingState.CallerWorkflowInstanceId;

                callerContext = trackingCallingState.CallerContextGuid;
                callerParentContext = trackingCallingState.CallerParentContextGuid;
            }

            TrackingParameters parameters = new TrackingParameters(instanceID, workflowType, exec.WorkflowDefinition, activityCallPath, callerInstanceId, context, callerContext, callerParentContext);

            for (int i = 0; i < _services.Count; i++)
            {
                TrackingChannel channel = null;
                Type serviceType = _services[i].GetType();

                //
                // See if the service has a profile for this schedule type
                // If not we don't do any tracking for the service
                // 
                RTTrackingProfile profile = null;

                //
                // If we've created the broker get the current version of the profile
                if (initBroker)
                {
                    profile = _profileManager.GetProfile(_services[i], schedule);

                    if (null == profile)
                        continue;

                    broker.AddService(serviceType, profile.Version);
                }
                else
                {
                    //
                    // Only reload the services that are in the broker
                    // If services that weren't originally associated to an instance
                    // wish to join that instance they should call ReloadTrackingProfiles
                    if (!broker.ContainsService(serviceType))
                        continue;

                    if (broker.IsProfileInstance(serviceType))
                    {
                        profile = _profileManager.GetProfile(_services[i], schedule, instanceID);

                        if (null == profile)
                            throw new InvalidOperationException(ExecutionStringManager.MissingProfileForService + serviceType.ToString());

                        profile.IsPrivate = true;
                    }
                    else
                    {
                        Version versionId;
                        if (broker.TryGetProfileVersionId(serviceType, out versionId))
                        {
                            profile = _profileManager.GetProfile(_services[i], schedule, versionId);

                            if (null == profile)
                                throw new InvalidOperationException(ExecutionStringManager.MissingProfileForService + serviceType.ToString() + ExecutionStringManager.MissingProfileForVersion + versionId.ToString());
                            //
                            // If the profile is marked as private clone the instance we got from the cache
                            // The cloned instance is marked as private during the cloning
                            if (broker.IsProfilePrivate(serviceType))
                            {
                                profile = profile.Clone();
                                profile.IsPrivate = true;
                            }
                        }
                        else
                            continue;
                    }
                }

                //
                // If profile is not null get a channel
                channel = _services[i].GetTrackingChannel(parameters);

                if (null == channel)
                    throw new InvalidOperationException(ExecutionStringManager.NullChannel);

                channels.Add(new TrackingChannelWrapper(channel, _services[i].GetType(), workflowType, profile));
            }

            return channels;
        }
    }

    /// <summary>
    /// Handles subscribing to status change events and receiving event notifications.
    /// </summary>
    internal class TrackingListener
    {
        private List<TrackingChannelWrapper> _channels = null;
        private TrackingListenerBroker _broker = null;
        private TrackingListenerFactory _factory = null;

        protected TrackingListener()
        {
        }

        internal TrackingListener(TrackingListenerFactory factory, Activity sked, WorkflowExecutor exec, List<TrackingChannelWrapper> channels, TrackingListenerBroker broker, bool load)
        {
            if ((null == sked) || (null == broker))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullParameters);
                return;
            }
            _factory = factory;
            _channels = channels;
            //
            // Keep a reference to the broker so that we can hand it out when adding subscriptions
            _broker = broker;
            //
            // Give the broker our reference so that it can call us back on behalf of subscriptions
            _broker.TrackingListener = this;
        }

        internal TrackingListenerBroker Broker
        {
            get { return _broker; }
            set { _broker = value; }
        }

        internal void ReloadProfiles(WorkflowExecutor exec, Guid instanceId)
        {
            //
            // Ask the factory to redo the channels and broker
            _factory.ReloadProfiles(exec, instanceId, ref _broker, ref _channels);
        }


        #region Event Handlers

        internal void ActivityStatusChange(object sender, WorkflowExecutor.ActivityStatusChangeEventArgs e)
        {
            WorkflowTrace.Tracking.TraceInformation("TrackingListener::ActivityStatusChange - Received Activity Status Change Event for activity {0}", e.Activity.QualifiedName);

            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            if (null == e)
                throw new ArgumentNullException("e");

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            if ((null == _channels) || (_channels.Count <= 0))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NoChannels);
                return;
            }

            Activity activity = e.Activity;

            if (!SubscriptionRequired(activity, exec))
                return;
            //
            // Get the shared data that is the same for each tracking channel that gets a record
            Guid parentContextGuid = Guid.Empty, contextGuid = Guid.Empty;
            GetContext(activity, exec, out contextGuid, out parentContextGuid);

            DateTime dt = DateTime.UtcNow;
            int eventOrderId = _broker.GetNextEventOrderId();

            foreach (TrackingChannelWrapper wrapper in _channels)
            {
                //
                // Create a record for each tracking channel
                // Each channel gets a distinct record because extract data will almost always be different.
                ActivityTrackingRecord record = new ActivityTrackingRecord(activity.GetType(), activity.QualifiedName, contextGuid, parentContextGuid, activity.ExecutionStatus, dt, eventOrderId, null);

                bool extracted = wrapper.GetTrackingProfile(exec).TryTrackActivityEvent(activity, activity.ExecutionStatus, exec, record);
                //
                // Only send the record to the channel if the profile indicates that it is interested
                // This doesn't mean that the Body will always have data in it, 
                // it may be an empty extraction (just header info)
                if (extracted)
                    wrapper.TrackingChannel.Send(record);
            }
        }

        internal void UserTrackPoint(object sender, WorkflowExecutor.UserTrackPointEventArgs e)
        {
            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender is not WorkflowExecutor");

            WorkflowExecutor exec = (WorkflowExecutor)sender;
            Activity activity = e.Activity;

            DateTime dt = DateTime.UtcNow;
            int eventOrderId = _broker.GetNextEventOrderId();

            Guid parentContextGuid, contextGuid;
            GetContext(activity, exec, out contextGuid, out parentContextGuid);

            foreach (TrackingChannelWrapper wrapper in _channels)
            {
                UserTrackingRecord record = new UserTrackingRecord(activity.GetType(), activity.QualifiedName, contextGuid, parentContextGuid, dt, eventOrderId, e.Key, e.Args);

                if (wrapper.GetTrackingProfile(exec).TryTrackUserEvent(activity, e.Key, e.Args, exec, record))
                    wrapper.TrackingChannel.Send(record);
            }
        }

        internal void WorkflowExecutionEvent(object sender, WorkflowExecutor.WorkflowExecutionEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            WorkflowExecutor exec = sender as WorkflowExecutor;
            if (null == exec)
                throw new ArgumentException(ExecutionStringManager.InvalidSenderWorkflowExecutor);
            //
            // Many events are mapped "forward" and sent to tracking services 
            // (Persisting->Persisted, SchedulerEmpty->Idle)
            // This is so that a batch is always available when a tracking service gets an event.  
            // Without this tracking data could be inconsistent with the state of the instance. 
            switch (e.EventType)
            {
                case WorkflowEventInternal.Creating:
                    NotifyChannels(TrackingWorkflowEvent.Created, e, exec);
                    return;
                case WorkflowEventInternal.Starting:
                    NotifyChannels(TrackingWorkflowEvent.Started, e, exec);
                    return;
                case WorkflowEventInternal.Suspending:
                    NotifyChannels(TrackingWorkflowEvent.Suspended, e, exec);
                    return;
                case WorkflowEventInternal.Resuming:
                    NotifyChannels(TrackingWorkflowEvent.Resumed, e, exec);
                    return;
                case WorkflowEventInternal.Persisting:
                    NotifyChannels(TrackingWorkflowEvent.Persisted, e, exec);
                    return;
                case WorkflowEventInternal.Unloading:
                    NotifyChannels(TrackingWorkflowEvent.Unloaded, e, exec);
                    return;
                case WorkflowEventInternal.Loading:
                    NotifyChannels(TrackingWorkflowEvent.Loaded, e, exec);
                    return;
                case WorkflowEventInternal.Completing:
                    NotifyChannels(TrackingWorkflowEvent.Completed, e, exec);
                    NotifyChannelsOfCompletionOrTermination();
                    return;
                case WorkflowEventInternal.Aborting:
                    NotifyChannels(TrackingWorkflowEvent.Aborted, e, exec);
                    return;
                case WorkflowEventInternal.Terminating:
                    NotifyChannels(TrackingWorkflowEvent.Terminated, e, exec);
                    NotifyChannelsOfCompletionOrTermination();
                    return;
                case WorkflowEventInternal.Exception:
                    NotifyChannels(TrackingWorkflowEvent.Exception, e, exec);
                    return;
                case WorkflowEventInternal.SchedulerEmpty:
                    NotifyChannels(TrackingWorkflowEvent.Idle, e, exec);
                    return;
                case WorkflowEventInternal.UserTrackPoint:
                    UserTrackPoint(exec, (WorkflowExecutor.UserTrackPointEventArgs)e);
                    return;
                case WorkflowEventInternal.ActivityStatusChange:
                    ActivityStatusChange(exec, (WorkflowExecutor.ActivityStatusChangeEventArgs)e);
                    return;
                case WorkflowEventInternal.DynamicChangeBegin:
                    DynamicUpdateBegin(exec, (WorkflowExecutor.DynamicUpdateEventArgs)e);
                    return;
                case WorkflowEventInternal.DynamicChangeRollback:
                    DynamicUpdateRollback(exec, (WorkflowExecutor.DynamicUpdateEventArgs)e);
                    return;
                case WorkflowEventInternal.DynamicChangeCommit:
                    DynamicUpdateCommit(exec, (WorkflowExecutor.DynamicUpdateEventArgs)e);
                    return;
                default:
                    return;
            }
        }

        internal void DynamicUpdateBegin(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            WorkflowExecutor exec = (WorkflowExecutor)sender;
            //
            // WorkflowChangeEventArgs may be null or the WorkflowChanges may be null or empty
            // If so there's no work to do here
            if (null == e.ChangeActions)
                return;
            //
            // Clone the profiles to create instance specific copies (if they aren't already)
            MakeProfilesPrivate(exec);
            //
            // Give the profiles the changes.  At this point we are in a volatile state.
            // Profiles must act as if the changes will succeed but roll back any internal changes if they do not.
            foreach (TrackingChannelWrapper channel in _channels)
            {
                channel.GetTrackingProfile(exec).WorkflowChangeBegin(e.ChangeActions);
            }
        }

        internal void DynamicUpdateRollback(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            foreach (TrackingChannelWrapper channel in _channels)
            {
                channel.GetTrackingProfile(exec).WorkflowChangeRollback();
            }
        }

        internal void DynamicUpdateCommit(object sender, WorkflowExecutor.DynamicUpdateEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (!typeof(WorkflowExecutor).IsInstanceOfType(sender))
                throw new ArgumentException("sender");

            WorkflowExecutor exec = (WorkflowExecutor)sender;

            DateTime dt = DateTime.UtcNow;
            foreach (TrackingChannelWrapper channel in _channels)
            {
                channel.GetTrackingProfile(exec).WorkflowChangeCommit();
            }
            //
            // Notify tracking channels of changes
            int eventOrderId = _broker.GetNextEventOrderId();

            foreach (TrackingChannelWrapper wrapper in _channels)
            {
                WorkflowTrackingRecord rec = new WorkflowTrackingRecord(TrackingWorkflowEvent.Changed, dt, eventOrderId, new TrackingWorkflowChangedEventArgs(e.ChangeActions, exec.WorkflowDefinition));
                if (wrapper.GetTrackingProfile(exec).TryTrackInstanceEvent(TrackingWorkflowEvent.Changed, rec))
                    wrapper.TrackingChannel.Send(rec);
            }
        }

        #endregion Event Handlers

        #region Private Methods

        private void NotifyChannels(TrackingWorkflowEvent evt, WorkflowExecutor.WorkflowExecutionEventArgs e, WorkflowExecutor exec)
        {
            DateTime dt = DateTime.UtcNow;
            int eventOrderId = _broker.GetNextEventOrderId();

            foreach (TrackingChannelWrapper wrapper in _channels)
            {
                EventArgs args = null;
                switch (evt)
                {
                    case TrackingWorkflowEvent.Suspended:
                        args = new TrackingWorkflowSuspendedEventArgs(((WorkflowExecutor.WorkflowExecutionSuspendingEventArgs)e).Error);
                        break;
                    case TrackingWorkflowEvent.Terminated:
                        WorkflowExecutor.WorkflowExecutionTerminatingEventArgs wtea = (WorkflowExecutor.WorkflowExecutionTerminatingEventArgs)e;
                        if (null != wtea.Exception)
                            args = new TrackingWorkflowTerminatedEventArgs(wtea.Exception);
                        else
                            args = new TrackingWorkflowTerminatedEventArgs(wtea.Error);
                        break;
                    case TrackingWorkflowEvent.Exception:
                        WorkflowExecutor.WorkflowExecutionExceptionEventArgs weea = (WorkflowExecutor.WorkflowExecutionExceptionEventArgs)e;
                        args = new TrackingWorkflowExceptionEventArgs(weea.Exception, weea.CurrentPath, weea.OriginalPath, weea.ContextGuid, weea.ParentContextGuid);
                        break;
                }
                WorkflowTrackingRecord rec = new WorkflowTrackingRecord(evt, dt, eventOrderId, args);
                if (wrapper.GetTrackingProfile(exec).TryTrackInstanceEvent(evt, rec))
                    wrapper.TrackingChannel.Send(rec);
            }
        }

        private void NotifyChannelsOfCompletionOrTermination()
        {
            foreach (TrackingChannelWrapper wrapper in _channels)
                wrapper.TrackingChannel.InstanceCompletedOrTerminated();
        }

        private void GetContext(Activity activity, WorkflowExecutor exec, out Guid contextGuid, out Guid parentContextGuid)
        {
            contextGuid = _factory.GetContext(activity);

            if (null != activity.Parent)
                parentContextGuid = _factory.GetContext(activity.Parent);
            else
                parentContextGuid = contextGuid;

            Debug.Assert(contextGuid != Guid.Empty, "TrackingContext is empty");
            Debug.Assert(parentContextGuid != Guid.Empty, "Parent TrackingContext is empty");
        }

        /// <summary>
        /// Clone all profiles to create private versions in order to hold subscriptions for dynamic changes
        /// </summary>
        private void MakeProfilesPrivate(WorkflowExecutor exec)
        {
            foreach (TrackingChannelWrapper channel in _channels)
            {
                channel.MakeProfilePrivate(exec);
                _broker.MakeProfilePrivate(channel.TrackingServiceType);
            }
        }
        /// <summary>
        /// Determine if subscriptions are needed
        /// </summary>
        /// <param name="activity">Activity for which to check subscription needs</param>
        /// <returns></returns>
        private bool SubscriptionRequired(Activity activity, WorkflowExecutor exec)
        {
            //
            // Give each channel a chance to prep itself
            bool needed = false;

            foreach (TrackingChannelWrapper channel in _channels)
            {
                if ((channel.GetTrackingProfile(exec).ActivitySubscriptionNeeded(activity)) && (!needed))
                    needed = true;
            }

            return needed;
        }

        #endregion
    }

    /// <summary>
    /// This is a lightweight class that is serialized so that the TrackingListener doesn't have to be.
    /// Every subscription that the listener adds holds a reference to this class.  
    /// When an instance is loaded the broker is given to the listener factory and the listener factory
    /// gives the broker the new listener.  This saves us from having to persist the listener itself which
    /// means that while we do need to persist a list of service types and their profile version we don't
    /// have to persist the channels themselves (and we can't control how heavy channels get as they are host defined).
    /// </summary>
    [Serializable]
    internal class TrackingListenerBroker : System.Runtime.Serialization.ISerializable
    {
        [NonSerialized]
        private TrackingListener _listener = null;
        private int _eventOrderId = 0;
        private Dictionary<Guid, ServiceProfileContainer> _services = new Dictionary<Guid, ServiceProfileContainer>();

        internal TrackingListenerBroker()
        {
        }

        internal TrackingListenerBroker(TrackingListener listener)
        {
            _listener = listener;
        }

        internal TrackingListener TrackingListener
        {
            //
            // FxCops minbar complains because this isn't used.  
            // The Setter is required; seems weird not to have a getter.
            //get { return _listener; } 
            set { _listener = value; }
        }

        internal bool ContainsService(Type trackingServiceType)
        {
            return _services.ContainsKey(HashHelper.HashServiceType(trackingServiceType));
        }

        internal void AddService(Type trackingServiceType, Version profileVersionId)
        {
            _services.Add(HashHelper.HashServiceType(trackingServiceType), new ServiceProfileContainer(profileVersionId));
        }

        internal void ReplaceServices(Dictionary<string, Type> replacements)
        {
            if (replacements != null && replacements.Count > 0)
            {
                ServiceProfileContainer item;
                foreach (KeyValuePair<string, Type> replacement in replacements)
                {
                    Guid previous = HashHelper.HashServiceType(replacement.Key);
                    if (_services.TryGetValue(previous, out item))
                    {
                        _services.Remove(previous);
                        Guid current = HashHelper.HashServiceType(replacement.Value);
                        if (!_services.ContainsKey(current))
                        {
                            _services.Add(current, item);
                        }
                    }
                }
            }
        }

        internal void RemoveService(Type trackingServiceType)
        {
            _services.Remove(HashHelper.HashServiceType(trackingServiceType));
        }

        internal bool TryGetProfileVersionId(Type trackingServiceType, out Version profileVersionId)
        {
            profileVersionId = new Version(0, 0);

            ServiceProfileContainer service = null;
            if (_services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out service))
            {
                profileVersionId = service.ProfileVersionId;
                return true;
            }
            return false;
        }

        internal void MakeProfilePrivate(Type trackingServiceType)
        {
            ServiceProfileContainer service = null;
            if (!_services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out service))
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);

            service.IsPrivate = true;
        }

        internal bool IsProfilePrivate(Type trackingServiceType)
        {
            ServiceProfileContainer service = null;
            if (!_services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out service))
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);

            return service.IsPrivate;
        }

        internal void MakeProfileInstance(Type trackingServiceType)
        {
            ServiceProfileContainer service = null;
            if (!_services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out service))
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);
            //
            // Can't be instance without being private
            service.IsPrivate = true;
            service.IsInstance = true;
        }

        internal bool IsProfileInstance(Type trackingServiceType)
        {
            ServiceProfileContainer service = null;
            if (!_services.TryGetValue(HashHelper.HashServiceType(trackingServiceType), out service))
                throw new ArgumentException(ExecutionStringManager.InvalidTrackingService);

            return service.IsInstance;
        }

        internal int GetNextEventOrderId()
        {
            checked
            {
                return ++_eventOrderId;
            }
        }

        [Serializable]
        internal class ServiceProfileContainer
        {
            Version _profileVersionId = new Version(0, 0);
            bool _isPrivate = false;
            bool _isInstance = false;

            protected ServiceProfileContainer() { }

            internal ServiceProfileContainer(Version profileVersionId)
            {
                _profileVersionId = profileVersionId;
            }

            internal Version ProfileVersionId
            {
                get { return _profileVersionId; }
            }

            internal bool IsPrivate
            {
                get { return _isPrivate; }
                set { _isPrivate = value; }
            }

            internal bool IsInstance
            {
                get { return _isInstance; }
                set { _isInstance = value; }
            }
        }

        #region ISerializable Members

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            info.AddValue("eventOrderId", this._eventOrderId);
            info.AddValue("services", this._services.Count == 0 ? null : this._services);
        }
        private TrackingListenerBroker(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            this._eventOrderId = info.GetInt32("eventOrderId");
            this._services = (Dictionary<Guid, ServiceProfileContainer>)info.GetValue("services", typeof(Dictionary<Guid, ServiceProfileContainer>));
            if (this._services == null)
                this._services = new Dictionary<Guid, ServiceProfileContainer>();
        }

        #endregion
    }

    /// <summary>
    /// Manages profile requests, caching profiles and creating RTTrackingProfile instances.
    /// </summary>
    internal class TrackingProfileManager
    {
        //
        // This is a dictionary keyed by tracking service type 
        // that returns a dictionary that is key by schedule type
        // that returns a Set of profile versions for that schedule type
        // The set is constrained by VersionId
        private Dictionary<Type, Dictionary<Type, ProfileList>> _cacheLookup;
        //
        // Protects _cacheLookup
        private object _cacheLock = new object();
        //
        // Values assigned in Initialize
        private bool _init = false;
        private List<TrackingService> _services = null;
        private WorkflowRuntime _runtime = null;

        internal TrackingProfileManager()
        {
        }
        /// <summary>
        /// Clears all entries from the cache by reinitializing the member
        /// </summary>
        public static void ClearCache()
        {
            WorkflowRuntime.ClearTrackingProfileCache();
        }

        internal void ClearCacheImpl()
        {
            lock (_cacheLock)
            {
                _cacheLookup = new Dictionary<Type, Dictionary<Type, ProfileList>>();
            }
        }
        /// <summary>
        /// Create static state
        /// </summary>
        /// <param name="runtime"></param>
        internal void Initialize(WorkflowRuntime runtime)
        {
            lock (_cacheLock)
            {
                if (null == runtime)
                    throw new ArgumentException(ExecutionStringManager.NullEngine);

                _runtime = runtime;
                //
                // Initialize the cache
                // Do this every time the runtime starts/stops to make life easier 
                // for IProfileNotification tracking services that might have updated
                // profiles while we were stopped - we'll go get new versions since nothing is cached
                // without them having to fire updated events.
                _cacheLookup = new Dictionary<Type, Dictionary<Type, ProfileList>>();
                if (null != runtime.TrackingServices)
                {
                    _services = runtime.TrackingServices;
                    foreach (TrackingService service in _services)
                    {
                        if (service is IProfileNotification)
                        {
                            ((IProfileNotification)service).ProfileUpdated += new EventHandler<ProfileUpdatedEventArgs>(ProfileUpdated);
                            ((IProfileNotification)service).ProfileRemoved += new EventHandler<ProfileRemovedEventArgs>(ProfileRemoved);
                        }
                    }
                }
                _init = true;
            }
        }
        /// <summary>
        /// Clean up static state
        /// </summary>
        internal void Uninitialize()
        {
            lock (_cacheLock)
            {
                if (null != _runtime)
                {
                    foreach (TrackingService service in _services)
                    {
                        if (service is IProfileNotification)
                        {
                            ((IProfileNotification)service).ProfileUpdated -= new EventHandler<ProfileUpdatedEventArgs>(ProfileUpdated);
                            ((IProfileNotification)service).ProfileRemoved -= new EventHandler<ProfileRemovedEventArgs>(ProfileRemoved);
                        }
                    }
                }
                _runtime = null;
                _services = null;
                _init = false;
            }
        }
        /// <summary>
        /// Retrieves the current version of a profile from the specified service
        /// </summary>
        internal RTTrackingProfile GetProfile(TrackingService service, Activity schedule)
        {
            if (!_init)
                throw new ApplicationException(ExecutionStringManager.TrackingProfileManagerNotInitialized);

            if ((null == service) || (null == schedule))
            {
                WorkflowTrace.Tracking.TraceEvent(TraceEventType.Error, 0, ExecutionStringManager.NullParameters);
                return null;
            }

            Type workflowType = schedule.GetType();
            RTTrackingProfile tp = null;
            if (service is IProfileNotification)
            {
                //
                // If we found the profile in the cache return it, it may be null, this is OK 
                // (no profile for this service type/schedule type combination)
                if (TryGetFromCache(service.GetType(), workflowType, out tp))
                    return tp;
            }
            //
            // Either we don't have anything in the cache for this schedule/service combination
            // or this is a base TrackingService that doesn't notify of profile updates
            // Get the profile from the service
            TrackingProfile profile = null;

            if (!service.TryGetProfile(workflowType, out profile))
            {
                //
                // No profile for this schedule from this service
                // RemoveProfile will just mark this service/schedule as not currently having a profile in the cache
                RemoveProfile(workflowType, service.GetType());
                return null;
            }
            //
            // Check the cache to see if we already have this version
            // For TrackingService types this is necessary.
            // For IProfileNotification types this is a bit redundant 
            // but another threadcould have inserted the profile into the cache 
            // so check again before acquiring the writer lock
            if (TryGetFromCache(service.GetType(), workflowType, profile.Version, out tp))
                return tp;
            //
            // No profile, create it
            string xaml = schedule.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
            if (null != xaml && xaml.Length > 0)
            {
                //
                // Never add xaml only workflows to the cache
                // Each one must be handled distinctly
                return CreateProfile(profile, schedule, service.GetType());
            }
            else
            {
                tp = CreateProfile(profile, workflowType, service.GetType());
            }

            lock (_cacheLock)
            {
                //
                // Recheck the cache with exclusive access
                RTTrackingProfile tmp = null;
                if (TryGetFromCache(service.GetType(), workflowType, profile.Version, out tmp))
                    return tmp;
                //
                // Add it to the cache
                if (!AddToCache(tp, service.GetType()))
                    throw new ApplicationException(ExecutionStringManager.ProfileCacheInsertFailure);

                return tp;
            }
        }
        /// <summary>
        /// Retrieves the specified version of a profile from the specified service
        /// </summary>
        internal RTTrackingProfile GetProfile(TrackingService service, Activity workflow, Version versionId)
        {
            if (null == service)
                throw new ArgumentNullException("service");
            if (null == workflow)
                throw new ArgumentNullException("workflow");

            if (!_init)
                throw new InvalidOperationException(ExecutionStringManager.TrackingProfileManagerNotInitialized);

            Type workflowType = workflow.GetType();
            RTTrackingProfile tp = null;
            //
            // Looking for a specific version, see if it is in the cache
            if (TryGetFromCache(service.GetType(), workflowType, versionId, out tp))
                return tp;

            TrackingProfile profile = service.GetProfile(workflowType, versionId);
            //
            // No profile, create it
            string xaml = workflow.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
            if (null != xaml && xaml.Length > 0)
            {
                //
                // Never add xaml only workflows to the cache
                // Each one must be handled distinctly
                return CreateProfile(profile, workflow, service.GetType());
            }
            else
            {
                tp = CreateProfile(profile, workflowType, service.GetType());
            }

            lock (_cacheLock)
            {
                //
                // Recheck the cache with exclusive access
                RTTrackingProfile tmp = null;
                if (TryGetFromCache(service.GetType(), workflowType, versionId, out tmp))
                    return tmp;
                //
                // Add it to the cache
                if (!AddToCache(tp, service.GetType()))
                    throw new ApplicationException(ExecutionStringManager.ProfileCacheInsertFailure);

                return tp;
            }
        }

        internal RTTrackingProfile GetProfile(TrackingService service, Activity workflow, Guid instanceId)
        {
            //
            // An instance based profile will never be in the cache
            TrackingProfile profile = service.GetProfile(instanceId);

            if (null == profile)
                return null;

            return new RTTrackingProfile(profile, workflow, service.GetType());
        }

        #region Private Methods

        private RTTrackingProfile CreateProfile(TrackingProfile profile, Type workflowType, Type serviceType)
        {
            //
            // Can't use the activity definition that we have here, it may have been updated
            // Get the base definition and use it to create the profile.
            Activity tmpSchedule = _runtime.GetWorkflowDefinition(workflowType);
            return new RTTrackingProfile(profile, tmpSchedule, serviceType);
        }
        private RTTrackingProfile CreateProfile(TrackingProfile profile, Activity schedule, Type serviceType)
        {
            //
            // This is called for Xaml only workflows
            return new RTTrackingProfile(profile, schedule, serviceType);
        }
        /// <summary>
        /// Add a profile to the cache but do not reset the NoProfiles flag for the schedule type
        /// </summary>
        /// <param name="profile">RTTrackingProfile to add</param>
        /// <param name="serviceType">TrackingService type</param>
        /// <returns>True if the profile was successfully added; false if not</returns>
        private bool AddToCache(RTTrackingProfile profile, Type serviceType)
        {
            return AddToCache(profile, serviceType, false);
        }
        /// <summary>
        /// Adds a profile to the cache and optionally resets the NoProfiles flag for the schedule type
        /// </summary>
        /// <param name="profile">RTTrackingProfile to add</param>
        /// <param name="serviceType">TrackingService type</param>
        /// <param name="resetNoProfiles">true will reset NoProfiles (to false); false will leave NoProfiles as is</param>
        /// <returns>True if the profile was successfully added; false if not</returns>
        private bool AddToCache(RTTrackingProfile profile, Type serviceType, bool resetNoProfiles)
        {
            //
            // Profile may be null, serviceType may not
            if (null == serviceType)
                return false;

            lock (_cacheLock)
            {
                Dictionary<Type, ProfileList> schedules = null;
                //
                // Get the dictionary for the service type,
                // create it if it doesn't exist
                if (!_cacheLookup.TryGetValue(serviceType, out schedules))
                {
                    schedules = new Dictionary<Type, ProfileList>();
                    _cacheLookup.Add(serviceType, schedules);
                }
                //
                // The the ProfileList for the schedule type,
                // create it if it doesn't exist
                ProfileList profiles = null;
                if (!schedules.TryGetValue(profile.WorkflowType, out profiles))
                {
                    profiles = new ProfileList();
                    schedules.Add(profile.WorkflowType, profiles);
                }
                if (resetNoProfiles)
                    profiles.NoProfile = false;
                return profiles.Profiles.TryAdd(new CacheItem(profile));
            }
        }
        /// <summary>
        /// Gets a profile from the cache
        /// </summary>
        private bool TryGetFromCache(Type serviceType, Type workflowType, out RTTrackingProfile profile)
        {
            return TryGetFromCache(serviceType, workflowType, new Version(0, 0), out profile); // 0 is an internal signal to get the most current
        }
        /// <summary>
        /// Gets a profile from the cache
        /// </summary>
        private bool TryGetFromCache(Type serviceType, Type workflowType, Version versionId, out RTTrackingProfile profile)
        {
            profile = null;
            CacheItem item = null;
            lock (_cacheLock)
            {
                Dictionary<Type, ProfileList> schedules = null;

                if (!_cacheLookup.TryGetValue(serviceType, out schedules))
                    return false;

                ProfileList profiles = null;
                if (!schedules.TryGetValue(workflowType, out profiles))
                    return false;

                //
                // 0 means get the current version
                if (0 == versionId.Major)
                {
                    //
                    // Currently the schedule type doesn't have a profile associated to it
                    if (profiles.NoProfile)
                        return true;

                    if ((null == profiles.Profiles) || (0 == profiles.Profiles.Count))
                        return false;

                    //
                    // Current version is highest versionId
                    // which means it is at the end of the Set
                    int endPos = profiles.Profiles.Count - 1;

                    if (null == profiles.Profiles[endPos])
                        return false;

                    profile = profiles.Profiles[endPos].TrackingProfile;
                    return true;
                }
                else
                {
                    if ((null == profiles.Profiles) || (0 == profiles.Profiles.Count))
                        return false;

                    if (profiles.Profiles.TryGetValue(new CacheItem(workflowType, versionId), out item))
                    {
                        profile = item.TrackingProfile;
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        #endregion

        #region Event Handlers
        /// <summary>
        /// Listens on ProfileUpdated events from IProfileNotification services
        /// </summary>
        /// <param name="sender">Type of the tracking service sending the update</param>
        /// <param name="e">ProfileUpdatedEventArgs containing the new profile and the schedule type</param>
        private void ProfileUpdated(object sender, ProfileUpdatedEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            Type t = sender.GetType();

            if (null == e.WorkflowType)
                throw new ArgumentNullException("e");

            if (null == e.TrackingProfile)
            {
                RemoveProfile(e.WorkflowType, t);
                return;
            }

            RTTrackingProfile profile = CreateProfile(e.TrackingProfile, e.WorkflowType, t);
            //
            // If AddToCache fails this version is already in the cache and we don't care
            AddToCache(profile, t, true);
        }

        private void ProfileRemoved(object sender, ProfileRemovedEventArgs e)
        {
            if (null == sender)
                throw new ArgumentNullException("sender");

            if (null == e.WorkflowType)
                throw new ArgumentNullException("e");

            RemoveProfile(e.WorkflowType, sender.GetType());
        }

        private void RemoveProfile(Type workflowType, Type serviceType)
        {
            lock (_cacheLock)
            {
                Dictionary<Type, ProfileList> schedules = null;

                if (!_cacheLookup.TryGetValue(serviceType, out schedules))
                {
                    schedules = new Dictionary<Type, ProfileList>();
                    _cacheLookup.Add(serviceType, schedules);
                }
                ProfileList profiles = null;
                if (!schedules.TryGetValue(workflowType, out profiles))
                {
                    profiles = new ProfileList();
                    schedules.Add(workflowType, profiles);
                }
                //
                // Finally indicate that there isn't a profile for this schedule type
                // Calling UpdateProfile for this type will result in resetting this field
                // regardless of whether the version of the profile passed is in the cache or not
                profiles.NoProfile = true;
            }
        }
        #endregion

        #region Private Classes
        private class ProfileList
        {
            internal bool NoProfile = false;
            internal Set<CacheItem> Profiles = new Set<CacheItem>(5);
        }

        private class CacheItem : IComparable
        {
            internal RTTrackingProfile TrackingProfile = null;
            internal DateTime LastAccess = DateTime.UtcNow;
            //
            // VersionId and ScheduleType are stored separately from the profile so that they
            // can be used to identify the profile if it has been pushed from the cache.
            internal Version VersionId = new Version(0, 0);
            internal Type ScheduleType = null;

            internal CacheItem()
            {
            }

            internal CacheItem(RTTrackingProfile profile)
            {
                if (null == profile)
                    throw new ArgumentNullException("profile");

                ScheduleType = profile.WorkflowType;

                this.TrackingProfile = profile;
                VersionId = profile.Version;
            }

            internal CacheItem(Type workflowType, Version versionId)
            {
                VersionId = versionId;
                ScheduleType = workflowType;
            }

            #region IComparable Members

            public int CompareTo(object obj)
            {
                if (!(obj is CacheItem))
                    throw new ArgumentException(ExecutionStringManager.InvalidCacheItem);

                CacheItem item = (CacheItem)obj;
                if ((VersionId == item.VersionId) && (ScheduleType == item.ScheduleType))
                    return 0;
                else
                    return (VersionId > item.VersionId) ? 1 : -1;
            }

            #endregion
        }
        #endregion

    }
    /// <summary>
    /// Represents a wrapper around a channel and its artifacts, such as its tracking service type and profile
    /// </summary>
    internal class TrackingChannelWrapper
    {
        private Type _serviceType = null, _scheduleType = null;
        private TrackingChannel _channel = null;
        [NonSerialized]
        private RTTrackingProfile _profile = null;
        private Version _profileVersionId;

        private TrackingChannelWrapper() { }

        public TrackingChannelWrapper(TrackingChannel channel, Type serviceType, Type workflowType, RTTrackingProfile profile)
        {
            _serviceType = serviceType;
            _scheduleType = workflowType;
            _channel = channel;
            _profile = profile;
            _profileVersionId = profile.Version;
        }

        internal Type TrackingServiceType
        {
            get { return _serviceType; }
        }

        internal TrackingChannel TrackingChannel
        {
            get { return _channel; }
        }
        /// <summary>
        /// Get the tracking profile for the channel
        /// </summary>
        /// <param name="exec">BaseExecutor</param>
        /// <returns>RTTrackingProfile</returns>
        internal RTTrackingProfile GetTrackingProfile(WorkflowExecutor skedExec)
        {
            if (null != _profile)
                return _profile;
            else
                throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.CurrentCulture, ExecutionStringManager.NullProfileForChannel, this._scheduleType.AssemblyQualifiedName));
        }

        internal void SetTrackingProfile(RTTrackingProfile profile)
        {
            _profile = profile;
        }

        /// <summary>
        /// Clone the tracking profile stored in the cache and 
        /// </summary>
        /// <param name="exec"></param>
        internal void MakeProfilePrivate(WorkflowExecutor exec)
        {
            if (null != _profile)
            {
                //
                // If the profile is not already a private copy make it so
                if (!_profile.IsPrivate)
                {
                    _profile = _profile.Clone();
                    _profile.IsPrivate = true;
                }
            }
            else
            {
                //
                // We're not holding a reference to a profile
                // so get it from the cache and clone it into a private copy
                RTTrackingProfile tmp = GetTrackingProfile(exec);
                _profile = tmp.Clone();
                _profile.IsPrivate = true;
            }
        }
    }
    internal class Set<T> : IEnumerable<T> where T : IComparable
    {
        List<T> list = null;

        public Set()
        {
            list = new List<T>();
        }

        public Set(int capacity)
        {
            list = new List<T>(capacity);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public void Add(T item)
        {
            int pos = -1;
            if (!Search(item, out pos))
                list.Insert(pos, item);
            else
                throw new ArgumentException(ExecutionStringManager.ItemAlreadyExist);
        }

        public bool TryAdd(T item)
        {
            int pos = -1;
            if (!Search(item, out pos))
            {
                list.Insert(pos, item);
                return true;
            }
            else
                return false;
        }

        public bool Contains(T item)
        {
            int pos = -1;
            return Search(item, out pos);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool TryGetValue(T item, out T value)
        {
            int pos = -1;
            if (Search(item, out pos))
            {
                value = list[pos];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public T this[int index]
        {
            get { return list[index]; }
        }

        private bool Search(T item, out int insertPos)
        {
            insertPos = -1;

            int pos = 0,
                high = list.Count,
                low = -1,
                diff = 0;

            while (high - low > 1)
            {
                pos = (high + low) / 2;

                diff = list[pos].CompareTo(item);

                if (0 == diff)
                {
                    insertPos = pos;
                    return true;
                }
                else if (diff > 0)
                    high = pos;
                else
                    low = pos;
            }

            if (low == -1)
            {
                insertPos = 0;
                return false;
            }

            if (0 != diff)
            {
                insertPos = (diff < 0) ? pos + 1 : pos;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Persisted tracking State pertaining to workflow invoking for an individual schedule.  There could be multiple called schedules under 
    /// an instance.
    /// </summary>
    [Serializable]
    internal class TrackingCallingState
    {
        #region data members
        private IList<string> callerActivityPathProxy;
        private Guid callerInstanceId;
        private Guid callerContextGuid;
        private Guid callerParentContextGuid;

        #endregion data members

        #region Property accessors

        /// <summary>
        /// Activity proxy of the caller/execer activity, if any
        /// //@@Undone for Ashishmi: Hold on to ActivityPath Proxy  in one of your class impl
        /// </summary>
        /// <value></value>
        internal IList<string> CallerActivityPathProxy
        {
            get { return callerActivityPathProxy; }
            set { callerActivityPathProxy = value; }
        }

        /// <summary>
        /// Instance ID of the caller/exec'er schedule, if any
        /// </summary>
        /// <value></value>
        public Guid CallerWorkflowInstanceId
        {
            get { return callerInstanceId; }
            set { callerInstanceId = value; }
        }
        /// <summary>
        /// Context of the caller's invoke activity
        /// </summary>
        /// <value>int</value>
        public Guid CallerContextGuid
        {
            get { return callerContextGuid; }
            set { callerContextGuid = value; }
        }
        /// <summary>
        /// ParentContext of the caller's invoke activity
        /// </summary>
        /// <value>int</value>
        public Guid CallerParentContextGuid
        {
            get { return callerParentContextGuid; }
            set { callerParentContextGuid = value; }
        }

        #endregion Property accessors

    }

    internal static class HashHelper
    {
        internal static Guid HashServiceType(Type serviceType)
        {
            return HashServiceType(serviceType.AssemblyQualifiedName);
        }

        [SuppressMessage("Microsoft.Cryptographic.Standard", "CA5350:MD5CannotBeUsed", 
            Justification = "Design has been approved.  We are not using MD5 for any security or cryptography purposes but rather as a hash.")]
        internal static Guid HashServiceType(String serviceFullTypeName)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data;
            byte[] result;

            UnicodeEncoding ue = new UnicodeEncoding();
            data = ue.GetBytes(serviceFullTypeName);

            result = md5.ComputeHash(data);

            return new Guid(result);
        }
    }
}
