#pragma warning disable 1634, 1691

namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Collections;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Diagnostics;
    using System.Workflow.ComponentModel.Design;

    #endregion

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class QueueEventArgs : EventArgs
    {
        IComparable queueName;

        internal QueueEventArgs(IComparable queueName)
        {
            this.queueName = queueName;
        }

        public IComparable QueueName
        {
            get
            {
                return this.queueName;
            }
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityExecutionContext : IServiceProvider, IDisposable
    {
        #region Data members

        // dependency props
        public static readonly DependencyProperty CurrentExceptionProperty = DependencyProperty.RegisterAttached("CurrentException", typeof(Exception), typeof(ActivityExecutionContext), new PropertyMetadata(null, DependencyPropertyOptions.Default, null, EnforceExceptionSemantics, true));
        internal static readonly DependencyProperty GrantedLocksProperty = DependencyProperty.RegisterAttached("GrantedLocks", typeof(Dictionary<string, GrantedLock>), typeof(ActivityExecutionContext));
        internal static readonly DependencyProperty CachedGrantedLocksProperty = DependencyProperty.RegisterAttached("CachedGrantedLocks", typeof(Dictionary<string, GrantedLock>), typeof(ActivityExecutionContext), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal static readonly DependencyProperty LockAcquiredCallbackProperty = DependencyProperty.RegisterAttached("LockAcquiredCallback", typeof(ActivityExecutorDelegateInfo<EventArgs>), typeof(ActivityExecutionContext));

        private Activity currentActivity = null;
        private ActivityExecutionContextManager contextManager = null;
        private IStartWorkflow startWorkflowService = null;
        private bool allowSignalsOnCurrentActivity = false;

        private static Type schedulerServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowSchedulerService, " + AssemblyRef.RuntimeAssemblyRef);
        private static Type persistenceServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowPersistenceService, " + AssemblyRef.RuntimeAssemblyRef);
        private static Type trackingServiceType = Type.GetType("System.Workflow.Runtime.Tracking.TrackingService, " + AssemblyRef.RuntimeAssemblyRef);
        private static Type transactionServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowCommitWorkBatchService, " + AssemblyRef.RuntimeAssemblyRef);
        private static Type loaderServiceType = Type.GetType("System.Workflow.Runtime.Hosting.WorkflowLoaderService, " + AssemblyRef.RuntimeAssemblyRef);

        #endregion

        #region Members

        internal ActivityExecutionContext(Activity activity)
        {
            this.currentActivity = activity;
        }
        internal ActivityExecutionContext(Activity activity, bool allowSignalsOnCurrentActivity)
            : this(activity)
        {
            // ExecuteActivity/FaultActivity on root activity will be called by the ScheduleExecutor
            // we don't want to do child check in that case, so this flag is just to avoid those checks
            this.allowSignalsOnCurrentActivity = allowSignalsOnCurrentActivity;
        }

        public Activity Activity
        {
            get
            {
                if (this.currentActivity == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContext");

                return this.currentActivity;
            }
        }

        public ActivityExecutionContextManager ExecutionContextManager
        {
            get
            {
                if (this.currentActivity == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContext");

                if (this.contextManager == null)
                    this.contextManager = new ActivityExecutionContextManager(this);

                return this.contextManager;
            }
        }

        #endregion

        #region StartWorkflow
        internal sealed class StartWorkflow : IStartWorkflow
        {
            private ActivityExecutionContext executionContext = null;
            internal StartWorkflow(ActivityExecutionContext executionContext)
            {
                this.executionContext = executionContext;
            }
            Guid IStartWorkflow.StartWorkflow(Type workflowType, Dictionary<string, object> namedArgumentValues)
            {
                return this.executionContext.WorkflowCoreRuntime.StartWorkflow(workflowType, namedArgumentValues);
            }
        }
        #endregion

        #region IServiceProvider methods

        public T GetService<T>()
        {
            return (T)this.GetService(typeof(T));
        }

        public Object GetService(Type serviceType)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (serviceType == typeof(IStartWorkflow))
            {
                if (this.startWorkflowService == null)
                    this.startWorkflowService = new StartWorkflow(this);

                return this.startWorkflowService;
            }
            else
            {
                if (schedulerServiceType != null && schedulerServiceType.IsAssignableFrom(serviceType))
                    return null;

                if (persistenceServiceType != null && persistenceServiceType.IsAssignableFrom(serviceType))
                    return null;

                if (trackingServiceType != null && trackingServiceType.IsAssignableFrom(serviceType))
                    return null;

                if (transactionServiceType != null && transactionServiceType.IsAssignableFrom(serviceType))
                    return null;

                if (loaderServiceType != null && loaderServiceType.IsAssignableFrom(serviceType))
                    return null;
            }

            return this.currentActivity.WorkflowCoreRuntime.GetService(this.currentActivity, serviceType);
        }

        #endregion

        #region Context Information

        public Guid ContextGuid
        {
            get
            {
                if (this.currentActivity == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContext");

                return this.currentActivity.ContextActivity.ContextGuid;
            }
        }
        internal int ContextId
        {
            get
            {
                if (this.currentActivity == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContext");

                return this.currentActivity.ContextActivity.ContextId;
            }
        }

        #endregion

        #region Activity Execution Signals

        internal void InitializeActivity(Activity activity)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (activity == null)
                throw new ArgumentNullException("activity");

            if (!IsValidChild(activity, false))
                throw new ArgumentException(SR.GetString(SR.AEC_InvalidActivity), "activity");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidInitializingState));

            using (ActivityExecutionContext executionContext = new ActivityExecutionContext(activity))
            {
                using (this.currentActivity.WorkflowCoreRuntime.SetCurrentActivity(activity))
                    activity.Initialize(executionContext);
            }
        }
        public void ExecuteActivity(Activity activity)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (activity == null)
                throw new ArgumentNullException("activity");

            // if this activity is not executing, canceling, faulting OR compensating
            // then it can not execute a child.
            if (!this.allowSignalsOnCurrentActivity &&
                    (
                    this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Initialized ||
                    this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Closed
                    )
                )
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidStateToExecuteChild));

            if (!IsValidChild(activity, false))
                throw new ArgumentException(SR.GetString(SR.AEC_InvalidActivity), "activity");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized)
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidExecutionState));

            try
            {
                activity.SetStatus(ActivityExecutionStatus.Executing, false);
            }
            finally
            {
                Debug.Assert(activity.ExecutionStatus == ActivityExecutionStatus.Executing);
                this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(activity, ActivityOperationType.Execute, this.ContextId), IsInAtomicTransaction(activity), false, false);
            }
        }
        public void CancelActivity(Activity activity)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (activity == null)
                throw new ArgumentNullException("activity");

            // if this activity is not executing, canceling, faulting OR compensating
            // then it can not cancel a child.
            if (!this.allowSignalsOnCurrentActivity &&
                    (
                    this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Initialized ||
                    this.currentActivity.WorkflowCoreRuntime.CurrentActivity.ExecutionStatus == ActivityExecutionStatus.Closed
                    )
                )
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidStateToExecuteChild));

            if (!IsValidChild(activity, false))
                throw new ArgumentException(SR.GetString(SR.AEC_InvalidActivity), "activity");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Executing)
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidCancelingState));

            try
            {
                activity.SetStatus(ActivityExecutionStatus.Canceling, false);
            }
            finally
            {
                this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(activity, ActivityOperationType.Cancel, this.ContextId), IsInAtomicTransaction(activity), false, false);
            }
        }
        internal void CompensateActivity(Activity activity)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (activity == null)
                throw new ArgumentNullException("activity");

            if (!IsValidNestedChild(activity))
                throw new ArgumentException(SR.GetString(SR.AEC_InvalidNestedActivity), "activity");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Closed)
                throw new InvalidOperationException(SR.GetString(SR.Error_InvalidCompensatingState));

            try
            {
                activity.SetStatus(ActivityExecutionStatus.Compensating, false);
            }
            finally
            {
                this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(activity, ActivityOperationType.Compensate, this.ContextId), IsInAtomicTransaction(activity), false, false);
            }
        }
        internal void FaultActivity(Exception e)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            // the current activity might have closed, in that case, we would like to give the exception to parent
            if (this.currentActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                if (this.currentActivity.Parent == null)
                {
                    // this could have happened if the root activity closed, but 
                    // then it threw an exception
                    this.currentActivity.WorkflowCoreRuntime.TerminateInstance(e);
                }
                else
                {
                    this.currentActivity.WorkflowCoreRuntime.RaiseException(e, this.currentActivity.Parent, string.Empty);
                }
            }
            else
            {
                try
                {
                    this.currentActivity.SetValueCommon(CurrentExceptionProperty, e, CurrentExceptionProperty.DefaultMetadata, false);
                    this.currentActivity.SetStatus(ActivityExecutionStatus.Faulting, false);
                }
                finally
                {
                    this.currentActivity.WorkflowCoreRuntime.ScheduleItem(new ActivityExecutorOperation(this.currentActivity, ActivityOperationType.HandleFault, this.ContextId, e), IsInAtomicTransaction(this.currentActivity), false, false);
                }
            }
        }

        public void CloseActivity()
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            switch (this.currentActivity.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                    this.currentActivity.MarkCompleted();
                    break;
                case ActivityExecutionStatus.Canceling:
                    this.currentActivity.MarkCanceled();
                    break;
                case ActivityExecutionStatus.Compensating:
                    this.currentActivity.MarkCompensated();
                    break;
                case ActivityExecutionStatus.Faulting:
                    this.currentActivity.MarkFaulted();
                    break;
                case ActivityExecutionStatus.Closed:
                    break;
                default:
                    throw new InvalidOperationException(SR.GetString(SR.Error_InvalidClosingState));
            }
        }

        internal void Invoke<T>(EventHandler<T> handler, T e) where T : EventArgs
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            // let the activity handle it
            this.currentActivity.Invoke(handler, e);
        }

        #endregion

        #region Tracking Method

        // user tracking
        public void TrackData(object userData)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (null == userData)
                throw new ArgumentNullException("userData");

            this.currentActivity.WorkflowCoreRuntime.Track(null, userData);
        }

        // user tracking
        public void TrackData(string userDataKey, object userData)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (null == userData)
                throw new ArgumentNullException("userData");

            this.currentActivity.WorkflowCoreRuntime.Track(userDataKey, userData);
        }

        #endregion

        #region Locking methods

        internal bool AcquireLocks(IActivityEventListener<EventArgs> locksAcquiredCallback)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            this.Activity.SetValue(LockAcquiredCallbackProperty, new ActivityExecutorDelegateInfo<EventArgs>(true, locksAcquiredCallback, this.Activity.ContextActivity));
            return AcquireLocks(this.Activity);
        }

        private bool AcquireLocks(Activity activity)
        {
            // If this activity doesn't have any handles, we have nothing to do.
            ICollection<string> handles = GetAllSynchronizationHandles(activity);
            if (handles == null || handles.Count == 0)
                return true;

            Activity parent = activity.Parent;
            while (parent != null)
            {
                if (parent.SupportsSynchronization || parent.Parent == null)
                {
                    Dictionary<string, GrantedLock> grantedLocks = (Dictionary<string, GrantedLock>)parent.GetValue(GrantedLocksProperty);
                    if (grantedLocks == null)
                    {
                        grantedLocks = new Dictionary<string, GrantedLock>();
                        parent.SetValue(GrantedLocksProperty, grantedLocks);
                    }
                    foreach (string handle in handles)
                    {
                        bool acquiredLocks = true;
                        if (!grantedLocks.ContainsKey(handle))
                        {
                            grantedLocks[handle] = new GrantedLock(activity);
                        }
                        else if (grantedLocks[handle].Holder != activity)
                        {
                            grantedLocks[handle].WaitList.Add(activity);
                            acquiredLocks = false;
                        }
                        if (!acquiredLocks)
                            return false;
                    }
                }

                // If we reach a parent which has at least one handle, then we do not need to
                // go any further as the parent would already have acquired all our locks for
                // itself. Note that we still need to acquire our locks in the same parent if 
                // the parent ProvidesSychronization, hence, this if check is *not* after
                // "parent = parent.Parent"!
                ICollection<string> synchronizationHandlesOnParent = (ICollection<string>)parent.GetValue(Activity.SynchronizationHandlesProperty);
                if (synchronizationHandlesOnParent != null && synchronizationHandlesOnParent.Count != 0)
                    break;

                parent = parent.Parent;
            }
            return true;
        }

        internal void ReleaseLocks(bool transactional)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");


            // remove the callback.
            this.Activity.RemoveProperty(LockAcquiredCallbackProperty);

            // The assumption is that lock contentions will be few. Hence, we optimize serialization
            // size over performance, for ex. do not persist the list of locks that have already been 
            // granted.
            ICollection<string> handles = GetAllSynchronizationHandles(this.Activity);
            if (handles == null || handles.Count == 0)
                return;

            List<Activity> waitingActivities = new List<Activity>();
            Activity parent = Activity.Parent;
            while (parent != null)
            {
                if (parent.SupportsSynchronization || parent.Parent == null)
                {
                    Dictionary<string, GrantedLock> grantedLocks = (Dictionary<string, GrantedLock>)parent.GetValue(GrantedLocksProperty);

                    // if its an transactional release of locks, then release it and then keep it
                    // cached, so that in case of rollback, we can reacuire locks
                    if (transactional)
                    {
                        Dictionary<string, GrantedLock> cachedGrantedLocks = new Dictionary<string, GrantedLock>();

                        if (grantedLocks != null)
                            foreach (KeyValuePair<string, GrantedLock> grantedLockEntry in grantedLocks)
                                cachedGrantedLocks.Add(grantedLockEntry.Key, (GrantedLock)grantedLockEntry.Value.Clone());

                        parent.SetValue(CachedGrantedLocksProperty, cachedGrantedLocks);
                    }

                    if (grantedLocks != null)
                    {
                        foreach (string handle in handles)
                        {
                            if (!grantedLocks.ContainsKey(handle))
                            {
                                continue;
                            }
                            else if (grantedLocks[handle].WaitList.Count == 0)
                            {
                                grantedLocks.Remove(handle);
                            }
                            else if (grantedLocks[handle].Holder != this.Activity)
                            {
                                grantedLocks[handle].WaitList.Remove(this.Activity);
                            }
                            else
                            {
                                // Grant the lock to the next waiting activity.
                                Activity waitingActivity = grantedLocks[handle].WaitList[0];
                                grantedLocks[handle].WaitList.RemoveAt(0);
                                grantedLocks[handle].Holder = waitingActivity;
                                if (!waitingActivities.Contains(waitingActivity))
                                    waitingActivities.Add(waitingActivity);
                            }
                        }
                        if (grantedLocks.Count == 0)
                            parent.RemoveProperty(GrantedLocksProperty);
                    }
                }

                // If we reach a parent which has at least one handle, then we do not need to
                // go any further as the parent would already have acquired all our locks for
                // itself. Note that we still need to acquire our locks in the same parent if 
                // the parent ProvidesSychronization, hence, this if check is *not* after
                // "parent = parent.Parent"!
                ICollection<string> synchronizationHandlesOnParent = (ICollection<string>)parent.GetValue(Activity.SynchronizationHandlesProperty);
                if (synchronizationHandlesOnParent != null && synchronizationHandlesOnParent.Count != 0)
                    break;

                parent = parent.Parent;
            }

            // Try and acquire locks for all the waiting activities.
            foreach (Activity waitingActivity in waitingActivities)
            {
                if (AcquireLocks(waitingActivity))
                {
                    ActivityExecutorDelegateInfo<EventArgs> waitingActivityCallback = (ActivityExecutorDelegateInfo<EventArgs>)waitingActivity.GetValue(LockAcquiredCallbackProperty);
                    waitingActivityCallback.InvokeDelegate(this.Activity.ContextActivity, EventArgs.Empty, false, transactional);
                }
            }
        }

        private ICollection<string> GetAllSynchronizationHandles(Activity activity)
        {
            // If the activity doesn't have any handles, do not look at child activities.
            ICollection<string> handleCollection = (ICollection<string>)activity.GetValue(Activity.SynchronizationHandlesProperty);
            if (handleCollection == null || handleCollection.Count == 0)
                return handleCollection;

            List<string> handles = new List<string>(handleCollection);
            // Collect all child locks and normalize the list.
            if (activity is CompositeActivity)
            {
                Walker walker = new Walker();
                walker.FoundActivity += delegate(Walker w, WalkerEventArgs e)
                {
                    if (e.CurrentActivity == activity)
                        return;

                    ICollection<string> handlesOnChild = (ICollection<string>)e.CurrentActivity.GetValue(Activity.SynchronizationHandlesProperty);
                    if (handlesOnChild != null)
                        handles.AddRange(handlesOnChild);
                };
                walker.Walk(activity);
            }

            // normalize handles
            handles.Sort();
            for (int i = 1; i < handles.Count; i++)
            {
                if (handles[i] == handles[i - 1])
                    handles.RemoveAt(--i);
            }
            handles.TrimExcess();

            // return
            return handles;
        }

        #endregion

        #region Instance Operation Methods

        internal void SuspendWorkflowInstance(string suspendDescription)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            this.currentActivity.WorkflowCoreRuntime.SuspendInstance(suspendDescription);
        }
        internal void TerminateWorkflowInstance(Exception e)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (e == null)
                throw new ArgumentNullException("e");

            this.currentActivity.WorkflowCoreRuntime.TerminateInstance(e);
        }
        internal void CheckpointInstanceState()
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            this.currentActivity.WorkflowCoreRuntime.CheckpointInstanceState(this.currentActivity);
        }
        internal void RequestRevertToCheckpointState(EventHandler<EventArgs> handler, EventArgs data, bool suspendOnRevert, string suspendOnRevertInfo)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            this.currentActivity.WorkflowCoreRuntime.RequestRevertToCheckpointState(this.currentActivity, handler, data, suspendOnRevert, suspendOnRevertInfo);
        }
        internal void DisposeCheckpointState()
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            this.currentActivity.WorkflowCoreRuntime.DisposeCheckpointState();
        }
        #endregion

        #region Helper Methods

        internal bool IsValidChild(Activity activity, bool allowContextVariance)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (activity == this.currentActivity.WorkflowCoreRuntime.CurrentActivity && this.allowSignalsOnCurrentActivity)
                return true;

            if (activity.Enabled && activity.Parent == this.currentActivity.WorkflowCoreRuntime.CurrentActivity && (allowContextVariance || activity.Equals(this.Activity.GetActivityByName(activity.QualifiedName, true))))
                return true;

            return false;
        }
        internal bool IsValidNestedChild(Activity activity)
        {
            if (this.currentActivity == null)
                throw new ObjectDisposedException("ActivityExecutionContext");

            if (activity == this.currentActivity)
                return true;

            Activity parentActivity = activity;
            while (parentActivity != null && parentActivity.Enabled && parentActivity.Parent != this.currentActivity.ContextActivity)
                parentActivity = parentActivity.Parent;

            return (parentActivity != null && parentActivity.Enabled);
        }
        internal IWorkflowCoreRuntime WorkflowCoreRuntime
        {
            get
            {
                if (this.currentActivity == null)
#pragma warning suppress 56503
                    throw new ObjectDisposedException("ActivityExecutionContext");

                return this.GetService<IWorkflowCoreRuntime>();
            }
        }
        internal static bool IsInAtomicTransaction(Activity activity)
        {
            bool isInAtomicTransaction = false;
            while (activity != null)
            {
                if (activity == activity.WorkflowCoreRuntime.CurrentAtomicActivity)
                {
                    isInAtomicTransaction = true;
                    break;
                }
                activity = activity.Parent;
            }
            return isInAtomicTransaction;
        }
        #endregion

        #region CurrentExceptionProperty Guard
        static void EnforceExceptionSemantics(DependencyObject d, object value)
        {
            Activity activity = d as Activity;

            if (activity == null)
                throw new ArgumentException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_DOIsNotAnActivity));

            if (value != null)
                throw new InvalidOperationException(SR.GetString(System.Globalization.CultureInfo.CurrentCulture, SR.Error_PropertyCanBeOnlyCleared));

            d.SetValueCommon(CurrentExceptionProperty, null, CurrentExceptionProperty.DefaultMetadata, false);
        }
        #endregion

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (this.currentActivity != null)
            {
                if (this.contextManager != null)
                {
                    this.contextManager.Dispose();
                    this.contextManager = null;
                }
                this.currentActivity = null;
            }
        }

        #endregion
    }

    #region Class GrantedLock

    [Serializable]
    internal class GrantedLock : ICloneable
    {
        private Activity holder;
        private List<Activity> waitList;

        public GrantedLock(Activity holder)
        {
            this.holder = holder;
            this.waitList = new List<Activity>();
        }
        public Activity Holder
        {
            get
            {
                return this.holder;
            }
            set
            {
                this.holder = value;
            }
        }
        public IList<Activity> WaitList
        {
            get
            {
                return this.waitList;
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            GrantedLock clonedGrantedLock = new GrantedLock(this.holder);
            clonedGrantedLock.waitList.InsertRange(0, this.waitList);
            return clonedGrantedLock;
        }
        #endregion
    }

    #endregion
}
