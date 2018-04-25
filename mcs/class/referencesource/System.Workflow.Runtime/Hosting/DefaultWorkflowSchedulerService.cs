using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Workflow.Runtime;
using System.Globalization;

namespace System.Workflow.Runtime.Hosting
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class DefaultWorkflowSchedulerService : WorkflowSchedulerService
    {
        // next two fields controlled by locking the timerQueue
        private KeyedPriorityQueue<Guid, CallbackInfo, DateTime> timerQueue = new KeyedPriorityQueue<Guid, CallbackInfo, DateTime>();
        private Timer callbackTimer;

        private TimerCallback timerCallback;
        private const string MAX_SIMULTANEOUS_WORKFLOWS_KEY = "maxSimultaneousWorkflows";
        private const int DEFAULT_MAX_SIMULTANEOUS_WORKFLOWS = 5;
        private static TimeSpan infinite = new TimeSpan(Timeout.Infinite);
        private readonly int maxSimultaneousWorkflows;       // Maximum number of work items allowed in ThreadPool queue
        private static TimeSpan fiveMinutes = new TimeSpan(0, 5, 0);

        // next three fields controlled by locking the waitingQueue
        private int numCurrentWorkers;
        private Queue<WorkItem> waitingQueue;       // Queue for extra items waiting to be allowed into thread pool
        private volatile bool running = false;

        private IList<PerformanceCounter> queueCounters;    // expose internal queue length

        private static int DefaultThreadCount
        {
            get
            {
                return Environment.ProcessorCount == 1
                    ? DEFAULT_MAX_SIMULTANEOUS_WORKFLOWS
                    : (int)(DEFAULT_MAX_SIMULTANEOUS_WORKFLOWS * Environment.ProcessorCount * .8);
            }
        }

        public DefaultWorkflowSchedulerService()
            : this(DefaultThreadCount)
        {
        }


        public DefaultWorkflowSchedulerService(int maxSimultaneousWorkflows)
            : base()
        {
            if (maxSimultaneousWorkflows < 1)
                throw new ArgumentOutOfRangeException(MAX_SIMULTANEOUS_WORKFLOWS_KEY, maxSimultaneousWorkflows, String.Empty);
            this.maxSimultaneousWorkflows = maxSimultaneousWorkflows;
            init();
        }

        public DefaultWorkflowSchedulerService(NameValueCollection parameters)
            : base()
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            maxSimultaneousWorkflows = DefaultThreadCount;
            foreach (string key in parameters.Keys)
            {
                if (key == null)
                    throw new ArgumentException(String.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, "null"));
                string p = parameters[key];
                if (!key.Equals(MAX_SIMULTANEOUS_WORKFLOWS_KEY, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(String.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.UnknownConfigurationParameter, key));
                if (!int.TryParse(p, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out maxSimultaneousWorkflows))
                    throw new FormatException(MAX_SIMULTANEOUS_WORKFLOWS_KEY);
            }

            if (maxSimultaneousWorkflows < 1)
                throw new ArgumentOutOfRangeException(MAX_SIMULTANEOUS_WORKFLOWS_KEY, maxSimultaneousWorkflows, String.Empty);

            init();
        }

        private void init()
        {
            timerCallback = new TimerCallback(OnTimerCallback);
            timerQueue.FirstElementChanged += OnFirstElementChanged;
            waitingQueue = new Queue<WorkItem>();
        }


        public int MaxSimultaneousWorkflows
        {
            get { return maxSimultaneousWorkflows; }
        }

        internal protected override void Schedule(WaitCallback callback, Guid workflowInstanceId)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Scheduling work for instance {0}", workflowInstanceId);

            if (callback == null)
                throw new ArgumentNullException("callback");
            if (workflowInstanceId == Guid.Empty)
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, "workflowInstanceId"));

            // Add the work item to our internal queue and signal the ProcessQueue thread
            EnqueueWorkItem(new WorkItem(callback, workflowInstanceId));
        }

        internal protected override void Schedule(WaitCallback callback, Guid workflowInstanceId, DateTime whenUtc, Guid timerId)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Scheduling work for instance {0} on timer ID {1} in {2}", workflowInstanceId, timerId, (whenUtc - DateTime.UtcNow));

            if (callback == null)
                throw new ArgumentNullException("callback");
            if (timerId == Guid.Empty)
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, "timerId"));
            if (workflowInstanceId == Guid.Empty)
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, "workflowInstanceId"));

            CallbackInfo ci = new CallbackInfo(this, callback, workflowInstanceId, whenUtc);

            lock (timerQueue)
            {
                timerQueue.Enqueue(timerId, ci, whenUtc);
            }
        }

        internal protected override void Cancel(Guid timerId)
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Cancelling work with timer ID {0}", timerId);

            if (timerId == Guid.Empty)
                throw new ArgumentException(String.Format(CultureInfo.CurrentUICulture, ExecutionStringManager.CantBeEmptyGuid, "timerId"), "timerId");

            lock (timerQueue)
            {
                timerQueue.Remove(timerId);
            }
        }

        override protected void OnStarted()
        {
            lock (timerQueue)
            {
                base.OnStarted();
                CallbackInfo ci = timerQueue.Peek();
                if (ci != null)
                    callbackTimer = CreateTimerCallback(ci);
                running = true;
            }
            lock (waitingQueue)
            {
                int nToStart = Math.Min(maxSimultaneousWorkflows, waitingQueue.Count);
                for (int i = 0; i < nToStart; i++)
                {
                    if (ThreadPool.QueueUserWorkItem(QueueWorkerProcess))
                    {
                        numCurrentWorkers++;
                    }
                }
            }
            if (queueCounters == null && this.Runtime.PerformanceCounterManager != null)
            {
                queueCounters = this.Runtime.PerformanceCounterManager.CreateCounters(ExecutionStringManager.PerformanceCounterWorkflowsWaitingName);
            }
        }

        protected internal override void Stop()
        {
            lock (timerQueue)
            {
                base.Stop();
                if (callbackTimer != null)
                {
                    callbackTimer.Dispose();
                    callbackTimer = null;
                }
                running = false;
            }
            lock (waitingQueue)
            {
                while (numCurrentWorkers > 0)
                {
                    Monitor.Wait(waitingQueue);
                }
            }
        }

        private void OnFirstElementChanged(object source, KeyedPriorityQueueHeadChangedEventArgs<CallbackInfo> e)
        {
            // timerQueue must have been locked by operation that caused this event to fire

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

        private void OnTimerCallback(object ignored)
        {
            //Make sure activity ID comes out of Threadpool are initialized to null.
            Trace.CorrelationManager.ActivityId = Guid.Empty;

            CallbackInfo ci = null;
            bool fire = false;
            try
            {
                lock (timerQueue)
                {
                    if (State == WorkflowRuntimeServiceState.Started)
                    {
                        ci = timerQueue.Peek();
                        if (ci != null)
                        {
                            if (ci.IsExpired)
                            {
                                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Timeout occured for timer for instance {0}", ci.State);
                                timerQueue.Dequeue();
                                fire = true;
                            }
                            else
                            {
                                callbackTimer = CreateTimerCallback(ci);
                            }
                        }
                    }
                }
                if (fire && ci != null)
                    ci.Callback(ci.State);
            }
            // Ignore cases where the workflow has been stolen out from under us
            catch (WorkflowOwnershipException)
            { }
            catch (ThreadAbortException e)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", ci == null ? null : ci.State, e.Message);
                RaiseServicesExceptionNotHandledEvent(e, (Guid)ci.State);
                throw;
            }
            catch (Exception e)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Timeout for instance, {0} threw exception {1}", ci == null ? null : ci.State, e.Message);
                RaiseServicesExceptionNotHandledEvent(e, (Guid)ci.State);
            }
        }

        private Timer CreateTimerCallback(CallbackInfo info)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan span = (info.When > now) ? info.When - now : TimeSpan.Zero;
            if (span > fiveMinutes) // never let more than five minutes go by without checking
                span = fiveMinutes;
            return new Timer(timerCallback, info.State, span, infinite);
        }

        private void EnqueueWorkItem(WorkItem workItem)
        {
            lock (waitingQueue)
            {
                waitingQueue.Enqueue(workItem);
                if (running && numCurrentWorkers < maxSimultaneousWorkflows)
                {
                    if (ThreadPool.QueueUserWorkItem(this.QueueWorkerProcess))
                    {
                        numCurrentWorkers++;
                    }
                }
            }
            if (queueCounters != null)
            {
                foreach (PerformanceCounter p in queueCounters)
                {
                    p.RawValue = waitingQueue.Count;
                }
            }
        }

        private void QueueWorkerProcess(object state /*unused*/)
        {
            //Make sure activity ID comes out of Threadpool are initialized to null.
            Trace.CorrelationManager.ActivityId = Guid.Empty;

            while (true)
            {
                WorkItem workItem;
                lock (waitingQueue)
                {
                    if (waitingQueue.Count == 0 || !running)
                    {
                        numCurrentWorkers--;
                        Monitor.Pulse(waitingQueue);
                        return;
                    }
                    workItem = waitingQueue.Dequeue();
                }
                if (queueCounters != null)
                {
                    foreach (PerformanceCounter p in queueCounters)
                    {
                        p.RawValue = waitingQueue.Count;
                    }
                }
                workItem.Invoke(this);
            }
        }


        internal class WorkItem
        {
            private WaitCallback callback;
            private object state;

            public WorkItem(WaitCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public WaitCallback Callback
            {
                get { return callback; }
            }

            public void Invoke(WorkflowSchedulerService service)
            {
                try
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Running workflow {0}", state);
                    Callback(state);
                }
                catch (Exception e)
                {
                    if (WorkflowExecutor.IsIrrecoverableException(e))
                    {
                        throw;
                    }
                    else
                    {
                        service.RaiseExceptionNotHandledEvent(e, (Guid)state);
                    }
                }
            }
        }

        internal class CallbackInfo
        {
            WaitCallback callback;
            object state;
            DateTime when;
            WorkflowSchedulerService service;

            public CallbackInfo(WorkflowSchedulerService service, WaitCallback callback, object state, DateTime when)
            {
                this.service = service;
                this.callback = callback;
                this.state = state;
                this.when = when;
            }

            public DateTime When
            {
                get { return when; }
            }

            public bool IsExpired
            {
                get { return DateTime.UtcNow >= when; }
            }

            public object State
            {
                get { return state; }
            }

            public WaitCallback Callback
            {
                get { return callback; }
            }
        }
    }
}
