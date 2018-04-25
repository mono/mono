using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Workflow.Runtime;
using System.Diagnostics;
using System.Globalization;

namespace System.Workflow.Runtime.Hosting
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ManualWorkflowSchedulerService : WorkflowSchedulerService
    {
        private class CallbackInfo
        {
            WaitCallback callback;
            Guid instanceId;
            Guid timerId;
            DateTime when;

            public CallbackInfo(WaitCallback callback, Guid instanceId, Guid timerId, DateTime when)
            {
                this.callback = callback;
                this.when = when;
                this.instanceId = instanceId;
                this.timerId = timerId;
            }

            public DateTime When
            {
                get { return when; }
            }

            public bool IsExpired
            {
                get { return DateTime.UtcNow >= when; }
            }

            public Guid InstanceId { get { return instanceId; } }

            public Guid TimerId { get { return timerId; } }

            public WaitCallback Callback
            {
                get { return callback; }
            }
        }

        private KeyedPriorityQueue<Guid, CallbackInfo, DateTime> pendingScheduleRequests = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
        private Dictionary<Guid, DefaultWorkflowSchedulerService.WorkItem> scheduleRequests = new Dictionary<Guid, DefaultWorkflowSchedulerService.WorkItem>();
        private object locker = new Object();
        private Timer callbackTimer;
        private readonly TimerCallback timerCallback;   // non-null indicates that active timers are enabled
        private volatile bool threadRunning;    // indicates that the timer thread is running
        private static TimeSpan infinite = new TimeSpan(Timeout.Infinite);
        private IList<PerformanceCounter> queueCounters;
        private static TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);

        private const string USE_ACTIVE_TIMERS_KEY = "UseActiveTimers";

        // Note that pendingScheduleRequests are keyed by instance ID under the assertion that there is at most one outstanding
        // timer for any given instance ID. To support cancellation, and additional map is kept of timerID-to-instanceID so that
        // we can find the appropriate pending  given a timer ID

        public ManualWorkflowSchedulerService()
        {
        }

        public ManualWorkflowSchedulerService(bool useActiveTimers)
        {
            if (useActiveTimers)
            {
                timerCallback = new TimerCallback(OnTimerCallback);
                pendingScheduleRequests.FirstElementChanged += OnFirstElementChanged;
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: started with active timers");
            }
        }
        public ManualWorkflowSchedulerService(NameValueCollection parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            foreach (string key in parameters.Keys)
            {
                if (key == null)
                    throw new ArgumentException(String.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, "null"));
                string p = parameters[key];
                if (!key.Equals(USE_ACTIVE_TIMERS_KEY, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(String.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, key));
                bool useActiveTimers;
                if (!bool.TryParse(p, out useActiveTimers))
                    throw new FormatException(USE_ACTIVE_TIMERS_KEY);
                if (useActiveTimers)
                {
                    timerCallback = new TimerCallback(OnTimerCallback);
                    pendingScheduleRequests.FirstElementChanged += OnFirstElementChanged;
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Started with active timers");
                }
            }
        }

        internal protected override void Schedule(WaitCallback callback, Guid workflowInstanceId)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (workflowInstanceId.Equals(Guid.Empty))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "workflowInstanceId"));

            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Schedule workflow {0}", workflowInstanceId);
                if (!scheduleRequests.ContainsKey(workflowInstanceId))
                    scheduleRequests.Add(workflowInstanceId, new DefaultWorkflowSchedulerService.WorkItem(callback, workflowInstanceId));
            }
            if (queueCounters != null)
            {
                foreach (PerformanceCounter p in queueCounters)
                {
                    p.RawValue = scheduleRequests.Count;
                }
            }
        }

        internal protected override void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (workflowInstanceId.Equals(Guid.Empty))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "workflowInstanceId"));
            if (timerId.Equals(Guid.Empty))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "timerId"));

            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Schedule timer {0} for workflow {1} at {2}", timerId, workflowInstanceId, whenUtc);
                pendingScheduleRequests.Enqueue(timerId, new CallbackInfo(callback, workflowInstanceId, timerId, whenUtc), whenUtc);
            }
        }

        internal protected override void Cancel(Guid timerId)
        {
            if (timerId.Equals(Guid.Empty))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "timerId"));

            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Cancel timer {0}", timerId);
                pendingScheduleRequests.Remove(timerId);
            }
        }

        private bool RunOne(Guid workflowInstanceId)
        {
            bool retval = false;
            DefaultWorkflowSchedulerService.WorkItem cs = null;
            lock (locker)
            {
                if (scheduleRequests.ContainsKey(workflowInstanceId))
                {
                    cs = scheduleRequests[workflowInstanceId];
                    scheduleRequests.Remove(workflowInstanceId);
                }
            }
            try
            {
                if (cs != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Executing {0}", workflowInstanceId);
                    if (queueCounters != null)
                    {
                        foreach (PerformanceCounter p in queueCounters)
                        {
                            p.RawValue = scheduleRequests.Count;
                        }
                    }
                    cs.Invoke(this);
                    retval = true;
                }
            }
            catch (Exception e)
            {
                RaiseServicesExceptionNotHandledEvent(e, workflowInstanceId);
            }
            return retval;
        }

        private bool HasExpiredTimer(Guid workflowInstanceId, out Guid timerId)
        {
            lock (locker)
            {
                CallbackInfo ci = pendingScheduleRequests.FindByPriority(DateTime.UtcNow,
                    delegate(CallbackInfo c) { return c.InstanceId == workflowInstanceId; });
                if (ci != null)
                {
                    timerId = ci.TimerId;
                    return true;
                }
            }
            timerId = Guid.Empty;
            return false;
        }
        private bool ProcessTimer(Guid workflowInstanceId)
        {
            bool retval = false;
            CallbackInfo cs = null;
            Guid timerId = Guid.Empty;
            lock (locker)
            {
                Guid expTimerId;
                if (HasExpiredTimer(workflowInstanceId, out expTimerId))
                {
                    cs = pendingScheduleRequests.Remove(expTimerId);
                }
            }
            try
            {
                if (cs != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Processing timer {0}", timerId);
                    cs.Callback(cs.InstanceId);
                    retval = true;
                }
            }
            catch (Exception e)
            {
                RaiseServicesExceptionNotHandledEvent(e, workflowInstanceId);
            }
            return retval;
        }

        private bool CanRun(Guid workflowInstanceId)
        {
            bool retval = false;
            lock (locker)
            {
                Guid timerId;
                retval = scheduleRequests.ContainsKey(workflowInstanceId) || HasExpiredTimer(workflowInstanceId, out timerId);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: CanRun is {0}", retval);
            }
            return retval;
        }

        public bool RunWorkflow(Guid workflowInstanceId)
        {
            if (workflowInstanceId.Equals(Guid.Empty))
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ExecutionStringManager.CantBeEmptyGuid, "workflowInstanceId"));

            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "ManualWorkflowSchedulerService: Running workflow {0}", workflowInstanceId);

            bool retval = false; // return true if we do any work at all
            while (CanRun(workflowInstanceId))
            {
                if (RunOne(workflowInstanceId) || ProcessTimer(workflowInstanceId))
                    retval = true;  // did some work, try again
                else
                    break;  // no work done this iteration
            }
            return retval;
        }
        private Timer CreateTimerCallback(CallbackInfo info)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan span = (info.When > now) ? info.When - now : TimeSpan.Zero;
            // never let more than five minutes go by without checking
            if (span > fiveMinutes)
            {
                span = fiveMinutes;
            }
            return new Timer(timerCallback, info.InstanceId, span, infinite);
        }
        override protected void OnStarted()
        {
            base.OnStarted();
            if (this.timerCallback != null)
            {
                lock (locker)
                {
                    CallbackInfo ci = pendingScheduleRequests.Peek();
                    if (ci != null)
                        callbackTimer = CreateTimerCallback(ci);
                }
            }
            lock (locker)
            {
                if (queueCounters == null && this.Runtime.PerformanceCounterManager != null)
                {
                    queueCounters = this.Runtime.PerformanceCounterManager.CreateCounters(ExecutionStringManager.PerformanceCounterWorkflowsWaitingName);
                }
            }
        }
        protected internal override void Stop()
        {
            base.Stop();
            if (this.timerCallback != null)
            {
                lock (locker)
                {
                    if (callbackTimer != null)
                    {
                        callbackTimer.Dispose();
                        callbackTimer = null;
                    }
                }
            }
        }
        private void OnTimerCallback(object ignored)
        {
            CallbackInfo ci = null;
            try
            {
                lock (locker)
                {
                    if (State.Equals(WorkflowRuntimeServiceState.Started))
                    {
                        ci = pendingScheduleRequests.Peek();
                        if (ci != null)
                        {
                            if (ci.IsExpired)
                            {
                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Timeout occured for timer for instance {0}", ci.InstanceId);
                                threadRunning = true;
                                pendingScheduleRequests.Dequeue();
                            }
                            else
                            {
                                callbackTimer = CreateTimerCallback(ci);
                            }
                        }
                    }
                }
                if (threadRunning)
                {
                    ci.Callback(ci.InstanceId);  // delivers the timer message
                    RunWorkflow(ci.InstanceId);
                }
            }
            catch (ThreadAbortException e)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", ci == null ? Guid.Empty : ci.InstanceId, e.Message);
                RaiseServicesExceptionNotHandledEvent(e, ci.InstanceId);
                throw;
            }
            catch (Exception e)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", ci == null ? Guid.Empty : ci.InstanceId, e.Message);
                RaiseServicesExceptionNotHandledEvent(e, ci.InstanceId);
            }
            finally
            {
                lock (locker)
                {
                    if (threadRunning)
                    {
                        threadRunning = false;
                        ci = pendingScheduleRequests.Peek();
                        if (ci != null)
                            callbackTimer = CreateTimerCallback(ci);
                    }
                }
            }
        }
        private void OnFirstElementChanged(object source, KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo> e)
        {
            lock (locker)
            {
                if (threadRunning)
                    return;     // ignore when a timer thread is already processing a timer request
                if (callbackTimer != null)
                {
                    callbackTimer.Dispose();
                    callbackTimer = null;
                }
                if (e.NewFirstElement != null && this.State == WorkflowRuntimeServiceState.Started)
                {
                    callbackTimer = CreateTimerCallback(e.NewFirstElement);
                }
            }
        }
    }
}
