//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;
    using System.Runtime.Diagnostics;

    [DataContract(Name = XD.Runtime.Scheduler, Namespace = XD.Runtime.Namespace)]
    class Scheduler
    {
        static ContinueAction continueAction = new ContinueAction();
        static YieldSilentlyAction yieldSilentlyAction = new YieldSilentlyAction();
        static AbortAction abortAction = new AbortAction();

        WorkItem firstWorkItem;

        static SendOrPostCallback onScheduledWorkCallback = Fx.ThunkCallback(new SendOrPostCallback(OnScheduledWork));

        SynchronizationContext synchronizationContext;

        bool isPausing;
        bool isRunning;

        bool resumeTraceRequired;

        Callbacks callbacks;

        Quack<WorkItem> workItemQueue;

        public Scheduler(Callbacks callbacks)
        {
            this.Initialize(callbacks);
        }

        public static RequestedAction Continue
        {
            get
            {
                return continueAction;
            }
        }

        public static RequestedAction YieldSilently
        {
            get
            {
                return yieldSilentlyAction;
            }
        }

        public static RequestedAction Abort
        {
            get
            {
                return abortAction;
            }
        }

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public bool IsIdle
        {
            get
            {
                return this.firstWorkItem == null;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "firstWorkItem")]
        internal WorkItem SerializedFirstWorkItem
        {
            get { return this.firstWorkItem; }
            set { this.firstWorkItem = value; }
        }

        [DataMember(EmitDefaultValue = false)]
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode)]
        internal WorkItem[] SerializedWorkItemQueue
        {
            get
            {
                if (this.workItemQueue != null && this.workItemQueue.Count > 0)
                {
                    return this.workItemQueue.ToArray();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                Fx.Assert(value != null, "EmitDefaultValue is false so we should never get null.");

                // this.firstWorkItem is serialized out separately, so don't use ScheduleWork() here
                this.workItemQueue = new Quack<WorkItem>(value);
            }
        }

        public void FillInstanceMap(ActivityInstanceMap instanceMap)
        {
            if (this.firstWorkItem != null)
            {
                ActivityInstanceMap.IActivityReference activityReference = this.firstWorkItem as ActivityInstanceMap.IActivityReference;
                if (activityReference != null)
                {
                    instanceMap.AddEntry(activityReference, true);
                }

                if (this.workItemQueue != null && this.workItemQueue.Count > 0)
                {
                    for (int i = 0; i < this.workItemQueue.Count; i++)
                    {
                        activityReference = this.workItemQueue[i] as ActivityInstanceMap.IActivityReference;
                        if (activityReference != null)
                        {
                            instanceMap.AddEntry(activityReference, true);
                        }
                    }
                }
            }
        }

        public static RequestedAction CreateNotifyUnhandledExceptionAction(Exception exception, ActivityInstance sourceInstance)
        {
            return new NotifyUnhandledExceptionAction(exception, sourceInstance);
        }

        public void ClearAllWorkItems(ActivityExecutor executor)
        {
            if (this.firstWorkItem != null)
            {
                this.firstWorkItem.Release(executor);
                this.firstWorkItem = null;

                if (this.workItemQueue != null)
                {
                    while (this.workItemQueue.Count > 0)
                    {
                        WorkItem item = this.workItemQueue.Dequeue();
                        item.Release(executor);
                    }
                }
            }

            Fx.Assert(this.workItemQueue == null || this.workItemQueue.Count == 0, "We either didn't have a first work item and therefore don't have anything in the queue, or we drained the queue.");

            // For consistency we set this to null even if it is empty
            this.workItemQueue = null;
        }

        public void OnDeserialized(Callbacks callbacks)
        {
            Initialize(callbacks);
            Fx.Assert(this.firstWorkItem != null || this.workItemQueue == null, "cannot have items in the queue unless we also have a firstWorkItem set");
        }

        // This method should only be called when we relinquished the thread but did not
        // complete the operation (silent yield is the current example)
        public void InternalResume(RequestedAction action)
        {
            Fx.Assert(this.isRunning, "We should still be processing work - we just don't have a thread");

            bool isTracingEnabled = FxTrace.ShouldTraceInformation;
            bool notifiedCompletion = false;
            bool isInstanceComplete = false;

            if (this.callbacks.IsAbortPending)
            {
                this.isPausing = false;
                this.isRunning = false;

                this.NotifyWorkCompletion();
                notifiedCompletion = true;

                if (isTracingEnabled)
                {
                    isInstanceComplete = this.callbacks.IsCompleted;
                }

                // After calling SchedulerIdle we no longer have the lock.  That means
                // that any subsequent processing in this method won't have the single
                // threaded guarantee.
                this.callbacks.SchedulerIdle();
            }
            else if (object.ReferenceEquals(action, continueAction))
            {
                ScheduleWork(false);
            }
            else
            {
                Fx.Assert(action is NotifyUnhandledExceptionAction, "This is the only other choice because we should never have YieldSilently here");

                NotifyUnhandledExceptionAction notifyAction = (NotifyUnhandledExceptionAction)action;

                // We only set isRunning back to false so that the host doesn't
                // have to treat this like a pause notification.  As an example,
                // a host could turn around and call run again in response to
                // UnhandledException without having to go through its operation
                // dispatch loop first (or request pause again).  If we reset
                // isPausing here then any outstanding operations wouldn't get
                // signaled with that type of host.
                this.isRunning = false;

                this.NotifyWorkCompletion();
                notifiedCompletion = true;

                if (isTracingEnabled)
                {
                    isInstanceComplete = this.callbacks.IsCompleted;
                }

                this.callbacks.NotifyUnhandledException(notifyAction.Exception, notifyAction.Source);
            }

            if (isTracingEnabled)
            {
                if (notifiedCompletion)
                {
                    Guid oldActivityId = Guid.Empty;
                    bool resetId = false;

                    if (isInstanceComplete)
                    {
                        if (TD.WorkflowActivityStopIsEnabled())
                        {
                            oldActivityId = DiagnosticTraceBase.ActivityId;
                            DiagnosticTraceBase.ActivityId = this.callbacks.WorkflowInstanceId;
                            resetId = true;

                            TD.WorkflowActivityStop(this.callbacks.WorkflowInstanceId);
                        }
                    }
                    else
                    {
                        if (TD.WorkflowActivitySuspendIsEnabled())
                        {
                            oldActivityId = DiagnosticTraceBase.ActivityId;
                            DiagnosticTraceBase.ActivityId = this.callbacks.WorkflowInstanceId;
                            resetId = true;

                            TD.WorkflowActivitySuspend(this.callbacks.WorkflowInstanceId);
                        }
                    }

                    if (resetId)
                    {
                        DiagnosticTraceBase.ActivityId = oldActivityId;
                    }
                }
            }
        }

        // called from ctor and OnDeserialized intialization paths
        void Initialize(Callbacks callbacks)
        {
            this.callbacks = callbacks;
        }

        public void Open(SynchronizationContext synchronizationContext)
        {
            Fx.Assert(this.synchronizationContext == null, "can only open when in the created state");
            if (synchronizationContext != null)
            {
                this.synchronizationContext = synchronizationContext;
            }
            else
            {
                this.synchronizationContext = SynchronizationContextHelper.GetDefaultSynchronizationContext();
            }
        }

        internal void Open(Scheduler oldScheduler)
        {
            Fx.Assert(this.synchronizationContext == null, "can only open when in the created state");
            this.synchronizationContext = SynchronizationContextHelper.CloneSynchronizationContext(oldScheduler.synchronizationContext);
        }

        void ScheduleWork(bool notifyStart)
        {
            if (notifyStart)
            {
                this.synchronizationContext.OperationStarted();
                this.resumeTraceRequired = true;
            }
            else
            {
                this.resumeTraceRequired = false;
            }
            this.synchronizationContext.Post(Scheduler.onScheduledWorkCallback, this);
        }

        void NotifyWorkCompletion()
        {
            this.synchronizationContext.OperationCompleted();
        }

        // signal the scheduler to stop processing work. If we are processing work
        // then we will catch this signal at our next iteration. Pause process completes
        // when idle is signalled. Can be called while we're processing work since
        // the worst thing that could happen in a ---- is that we pause one extra work item later
        public void Pause()
        {
            this.isPausing = true;
        }

        public void MarkRunning()
        {
            this.isRunning = true;
        }

        public void Resume()
        {
            Fx.Assert(this.isRunning, "This should only be called after we've been set to process work.");

            if (this.IsIdle || this.isPausing || this.callbacks.IsAbortPending)
            {
                this.isPausing = false;
                this.isRunning = false;
                this.callbacks.SchedulerIdle();
            }
            else
            {
                ScheduleWork(true);
            }
        }

        public void PushWork(WorkItem workItem)
        {
            if (this.firstWorkItem == null)
            {
                this.firstWorkItem = workItem;
            }
            else
            {
                if (this.workItemQueue == null)
                {
                    this.workItemQueue = new Quack<WorkItem>();
                }

                this.workItemQueue.PushFront(this.firstWorkItem);
                this.firstWorkItem = workItem;
            }

            // To avoid the virt call on EVERY work item we check
            // the Verbose flag.  All of our Schedule traces are
            // verbose.
            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                workItem.TraceScheduled();
            }
        }

        public void EnqueueWork(WorkItem workItem)
        {
            if (this.firstWorkItem == null)
            {
                this.firstWorkItem = workItem;
            }
            else
            {
                if (this.workItemQueue == null)
                {
                    this.workItemQueue = new Quack<WorkItem>();
                }

                this.workItemQueue.Enqueue(workItem);
            }

            if (FxTrace.ShouldTraceVerboseToTraceSource)
            {
                workItem.TraceScheduled();
            }
        }

        static void OnScheduledWork(object state)
        {
            Scheduler thisPtr = (Scheduler)state;

            // We snapshot these values here so that we can
            // use them after calling OnSchedulerIdle.
            bool isTracingEnabled = FxTrace.Trace.ShouldTraceToTraceSource(TraceEventLevel.Informational);
            Guid oldActivityId = Guid.Empty;
            Guid workflowInstanceId = Guid.Empty;

            if (isTracingEnabled)
            {
                oldActivityId = DiagnosticTraceBase.ActivityId;
                workflowInstanceId = thisPtr.callbacks.WorkflowInstanceId;
                FxTrace.Trace.SetAndTraceTransfer(workflowInstanceId, true);

                if (thisPtr.resumeTraceRequired)
                {
                    if (TD.WorkflowActivityResumeIsEnabled())
                    {
                        TD.WorkflowActivityResume(workflowInstanceId);
                    }
                }
            }

            thisPtr.callbacks.ThreadAcquired();

            RequestedAction nextAction = continueAction;
            bool idleOrPaused = false;

            while (object.ReferenceEquals(nextAction, continueAction))
            {
                if (thisPtr.IsIdle || thisPtr.isPausing)
                {
                    idleOrPaused = true;
                    break;
                }

                // cycle through (queue->thisPtr.firstWorkItem->currentWorkItem)
                WorkItem currentWorkItem = thisPtr.firstWorkItem;

                // promote an item out of our work queue if necessary
                if (thisPtr.workItemQueue != null && thisPtr.workItemQueue.Count > 0)
                {
                    thisPtr.firstWorkItem = thisPtr.workItemQueue.Dequeue();
                }
                else
                {
                    thisPtr.firstWorkItem = null;
                }

                if (TD.ExecuteWorkItemStartIsEnabled())
                {
                    TD.ExecuteWorkItemStart();
                }

                nextAction = thisPtr.callbacks.ExecuteWorkItem(currentWorkItem);

                if (TD.ExecuteWorkItemStopIsEnabled())
                {
                    TD.ExecuteWorkItemStop();
                }
            }

            bool notifiedCompletion = false;
            bool isInstanceComplete = false;

            if (idleOrPaused || object.ReferenceEquals(nextAction, abortAction))
            {
                thisPtr.isPausing = false;
                thisPtr.isRunning = false;

                thisPtr.NotifyWorkCompletion();
                notifiedCompletion = true;

                if (isTracingEnabled)
                {
                    isInstanceComplete = thisPtr.callbacks.IsCompleted;
                }

                // After calling SchedulerIdle we no longer have the lock.  That means
                // that any subsequent processing in this method won't have the single
                // threaded guarantee.
                thisPtr.callbacks.SchedulerIdle();
            }
            else if (!object.ReferenceEquals(nextAction, yieldSilentlyAction))
            {
                Fx.Assert(nextAction is NotifyUnhandledExceptionAction, "This is the only other option");

                NotifyUnhandledExceptionAction notifyAction = (NotifyUnhandledExceptionAction)nextAction;

                // We only set isRunning back to false so that the host doesn't
                // have to treat this like a pause notification.  As an example,
                // a host could turn around and call run again in response to
                // UnhandledException without having to go through its operation
                // dispatch loop first (or request pause again).  If we reset
                // isPausing here then any outstanding operations wouldn't get
                // signaled with that type of host.
                thisPtr.isRunning = false;

                thisPtr.NotifyWorkCompletion();
                notifiedCompletion = true;

                if (isTracingEnabled)
                {
                    isInstanceComplete = thisPtr.callbacks.IsCompleted;
                }

                thisPtr.callbacks.NotifyUnhandledException(notifyAction.Exception, notifyAction.Source);
            }

            if (isTracingEnabled)
            {
                if (notifiedCompletion)
                {
                    if (isInstanceComplete)
                    {
                        if (TD.WorkflowActivityStopIsEnabled())
                        {
                            TD.WorkflowActivityStop(workflowInstanceId);
                        }
                    }
                    else
                    {
                        if (TD.WorkflowActivitySuspendIsEnabled())
                        {
                            TD.WorkflowActivitySuspend(workflowInstanceId);
                        }
                    }
                }

                DiagnosticTraceBase.ActivityId = oldActivityId;
            }
        }

        public struct Callbacks
        {
            readonly ActivityExecutor activityExecutor;

            public Callbacks(ActivityExecutor activityExecutor)
            {
                this.activityExecutor = activityExecutor;
            }

            public Guid WorkflowInstanceId
            {
                get
                {
                    return this.activityExecutor.WorkflowInstanceId;
                }
            }

            public bool IsAbortPending
            {
                get
                {
                    return this.activityExecutor.IsAbortPending || this.activityExecutor.IsTerminatePending;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return ActivityUtilities.IsCompletedState(this.activityExecutor.State);
                }
            }

            public RequestedAction ExecuteWorkItem(WorkItem workItem)
            {
                Fx.Assert(this.activityExecutor != null, "ActivityExecutor null in ExecuteWorkItem.");

                // We check the Verbose flag to avoid the 
                // virt call if possible
                if (FxTrace.ShouldTraceVerboseToTraceSource)
                {
                    workItem.TraceStarting();
                }

                RequestedAction action = this.activityExecutor.OnExecuteWorkItem(workItem);

                if (!object.ReferenceEquals(action, Scheduler.YieldSilently))
                {
                    if (this.activityExecutor.IsAbortPending || this.activityExecutor.IsTerminatePending)
                    {
                        action = Scheduler.Abort;
                    }

                    // if the caller yields, then the work item is still active and the callback
                    // is responsible for releasing it back to the pool                    
                    workItem.Dispose(this.activityExecutor);                    
                }

                return action;
            }

            public void SchedulerIdle()
            {
                Fx.Assert(this.activityExecutor != null, "ActivityExecutor null in SchedulerIdle.");
                this.activityExecutor.OnSchedulerIdle();
            }

            public void ThreadAcquired()
            {
                Fx.Assert(this.activityExecutor != null, "ActivityExecutor null in ThreadAcquired.");
                this.activityExecutor.OnSchedulerThreadAcquired();
            }

            public void NotifyUnhandledException(Exception exception, ActivityInstance source)
            {
                Fx.Assert(this.activityExecutor != null, "ActivityExecutor null in NotifyUnhandledException.");
                this.activityExecutor.NotifyUnhandledException(exception, source);
            }
        }

        internal abstract class RequestedAction
        {
            protected RequestedAction()
            {
            }
        }

        class ContinueAction : RequestedAction
        {
            public ContinueAction()
            {
            }
        }

        class YieldSilentlyAction : RequestedAction
        {
            public YieldSilentlyAction()
            {
            }
        }

        class AbortAction : RequestedAction
        {
            public AbortAction()
            {
            }
        }

        class NotifyUnhandledExceptionAction : RequestedAction
        {
            public NotifyUnhandledExceptionAction(Exception exception, ActivityInstance source)
            {
                this.Exception = exception;
                this.Source = source;
            }

            public Exception Exception
            {
                get;
                private set;
            }

            public ActivityInstance Source
            {
                get;
                private set;
            }
        }
    }
}

