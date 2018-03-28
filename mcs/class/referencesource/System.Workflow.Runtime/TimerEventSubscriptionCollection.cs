using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.Runtime.Hosting;
using System.Workflow.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Collections;

namespace System.Workflow.Runtime
{
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TimerEventSubscriptionCollection : ICollection
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "Design has been approved.  This is a false positive. DependencyProperty is an immutable type.")]
        public readonly static DependencyProperty TimerCollectionProperty = DependencyProperty.RegisterAttached("TimerCollection", typeof(TimerEventSubscriptionCollection), typeof(TimerEventSubscriptionCollection));

        private object locker = new Object();
        private KeyedPriorityQueue<Guid, TimerEventSubscription, DateTime> queue = new KeyedPriorityQueue<Guid, TimerEventSubscription, DateTime>();
#pragma warning disable 0414
        private bool suspended = false; // no longer used but required for binary compatibility of serialization format
#pragma warning restore 0414
        [NonSerialized]
        private IWorkflowCoreRuntime executor;
        private Guid instanceId;

        internal TimerEventSubscriptionCollection(IWorkflowCoreRuntime executor, Guid instanceId)
        {
            this.executor = executor;
            this.instanceId = instanceId;
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Created", instanceId);
            this.queue.FirstElementChanged += OnFirstElementChanged;
        }

        internal void Enqueue(TimerEventSubscription timerEventSubscription)
        {
            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Enqueue Timer {1} for {2} ", instanceId, timerEventSubscription.SubscriptionId, timerEventSubscription.ExpiresAt);
                queue.Enqueue(timerEventSubscription.SubscriptionId, timerEventSubscription, timerEventSubscription.ExpiresAt);
            }
        }

        internal IWorkflowCoreRuntime Executor
        {
            get { return executor; }
            set { executor = value; }
        }

        public TimerEventSubscription Peek()
        {
            lock (locker)
            {
                return queue.Peek();
            }
        }

        internal TimerEventSubscription Dequeue()
        {
            lock (locker)
            {
                TimerEventSubscription retval = queue.Dequeue();
                if (retval != null)
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Dequeue Timer {1} for {2} ", instanceId, retval.SubscriptionId, retval.ExpiresAt);
                return retval;
            }
        }

        public void Remove(Guid timerSubscriptionId)
        {
            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Remove Timer {1}", instanceId, timerSubscriptionId);
                queue.Remove(timerSubscriptionId);
            }
        }

        private void OnFirstElementChanged(object source, KeyedPriorityQueueHeadChangedEventArgs<TimerEventSubscription> e)
        {
            lock (locker)
            {
                ITimerService timerService = this.executor.GetService(typeof(ITimerService)) as ITimerService;
                if (e.NewFirstElement != null && executor != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Schedule Timer {1} for {2} ", instanceId, e.NewFirstElement.SubscriptionId, e.NewFirstElement.ExpiresAt);
                    timerService.ScheduleTimer(executor.ProcessTimersCallback, e.NewFirstElement.WorkflowInstanceId, e.NewFirstElement.ExpiresAt, e.NewFirstElement.SubscriptionId);
                }
                if (e.OldFirstElement != null)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Unschedule Timer {1} for {2} ", instanceId, e.OldFirstElement.SubscriptionId, e.OldFirstElement.ExpiresAt);
                    timerService.CancelTimer(e.OldFirstElement.SubscriptionId);
                }
            }
        }

        internal void SuspendDelivery()
        {
            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Suspend", instanceId);
                WorkflowSchedulerService schedulerService = this.executor.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService;
                TimerEventSubscription sub = queue.Peek();
                if (sub != null)
                {
                    schedulerService.Cancel(sub.SubscriptionId);
                }
            }
        }

        internal void ResumeDelivery()
        {
            lock (locker)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TimerEventSubscriptionQueue: {0} Resume", instanceId);
                WorkflowSchedulerService schedulerService = this.executor.GetService(typeof(WorkflowSchedulerService)) as WorkflowSchedulerService;
                TimerEventSubscription sub = queue.Peek();
                if (sub != null)
                {
                    schedulerService.Schedule(executor.ProcessTimersCallback, sub.WorkflowInstanceId, sub.ExpiresAt, sub.SubscriptionId);
                }
            }
        }

        public void Add(TimerEventSubscription item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            this.Enqueue(item);
        }


        public void Remove(TimerEventSubscription item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            this.Remove(item.SubscriptionId);
        }

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            TimerEventSubscription[] tes = null;
            lock (locker)
            {
                tes = new TimerEventSubscription[queue.Count];
                queue.Values.CopyTo(tes, 0);
            }
            if (tes != null)
                tes.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot
        {
            get
            {
                return locker;
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return queue.Values.GetEnumerator();
        }

        #endregion
    }
}
