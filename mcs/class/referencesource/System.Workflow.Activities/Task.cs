#pragma warning disable 1634, 1691
namespace System.Workflow.Activities
{
    #region Using directives

    using System;
    using System.Xml.Serialization;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.CodeDom;
    using System.Diagnostics;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Transactions;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Reflection;
    using System.Workflow.Runtime.DebugEngine;
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.ReplicatorActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(ReplicatorActivity), "Resources.Replicator.png")]
    [Designer(typeof(ReplicatorDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(ReplicatorValidator))]
    [DefaultEvent("Initialized")]
    [WorkflowDebuggerSteppingAttribute(WorkflowDebuggerSteppingOption.Concurrent)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ReplicatorActivity : CompositeActivity
    {
        #region Dependency Properties
        public static readonly DependencyProperty UntilConditionProperty = DependencyProperty.Register("UntilCondition", typeof(ActivityCondition), typeof(ReplicatorActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ExecutionTypeProperty = DependencyProperty.Register("ExecutionType", typeof(ExecutionType), typeof(ReplicatorActivity), new PropertyMetadata(ExecutionType.Sequence));

        //events
        public static readonly DependencyProperty InitializedEvent = DependencyProperty.Register("Initialized", typeof(EventHandler), typeof(ReplicatorActivity));
        public static readonly DependencyProperty CompletedEvent = DependencyProperty.Register("Completed", typeof(EventHandler), typeof(ReplicatorActivity));
        public static readonly DependencyProperty ChildInitializedEvent = DependencyProperty.Register("ChildInitialized", typeof(EventHandler<ReplicatorChildEventArgs>), typeof(ReplicatorActivity));
        public static readonly DependencyProperty ChildCompletedEvent = DependencyProperty.Register("ChildCompleted", typeof(EventHandler<ReplicatorChildEventArgs>), typeof(ReplicatorActivity));
        public static readonly DependencyProperty InitialChildDataProperty = DependencyProperty.Register("InitialChildData", typeof(IList), typeof(ReplicatorActivity));

        #endregion

        #region Constructors

        public ReplicatorActivity()
        {
        }

        public ReplicatorActivity(string name)
            : base(name)
        {
        }

        #endregion

        #region Public Properties
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICollection<Activity> DynamicActivities
        {
            get
            {
                if (this.EnabledActivities.Count > 0)
                    return this.GetDynamicActivities(this.EnabledActivities[0]);
                else
                    return new Activity[0];
            }
        }

        [Browsable(true)]
        [SRCategory(SR.Properties)]
        [SRDescription(SR.ExecutionTypeDescr)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public ExecutionType ExecutionType
        {
            get
            {
                return (ExecutionType)base.GetValue(ReplicatorActivity.ExecutionTypeProperty);
            }
            set
            {
                if (value != ExecutionType.Sequence && value != ExecutionType.Parallel)
                    throw new ArgumentOutOfRangeException("value");
                if (this.ActivityState != null && this.ActivityState.IsChildActive)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorChildRunning));

                base.SetValue(ReplicatorActivity.ExecutionTypeProperty, value);
            }
        }

        [Editor(typeof(BindUITypeEditor), typeof(UITypeEditor))]
        [Browsable(true)]
        [SRCategory(SR.Properties)]
        [SRDescription(SR.InitialChildDataDescr)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(null)]
        public IList InitialChildData
        {
            get
            {
                return base.GetValue(InitialChildDataProperty) as IList;
            }
            set
            {
                base.SetValue(InitialChildDataProperty, value);
            }
        }

        ReplicatorChildInstanceList childDataList;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList CurrentChildData
        {
            get
            {
                if (childDataList == null)
                    childDataList = new ReplicatorChildInstanceList(this);

                return childDataList;
            }
        }


        [SRCategory(SR.Conditions)]
        [SRDescription(SR.ReplicatorUntilConditionDescr)]
        [DefaultValue(null)]
        public ActivityCondition UntilCondition
        {
            get
            {
                return base.GetValue(UntilConditionProperty) as ActivityCondition;
            }
            set
            {
                base.SetValue(UntilConditionProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllChildrenComplete
        {
            get
            {
                if (this.ActivityState != null)
                    return !this.ActivityState.IsChildActive;
                else
                    return true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentIndex
        {
            get
            {
                if (this.ActivityState != null)
                {
                    if (this.ExecutionType == ExecutionType.Sequence)
                        return this.ActivityState.CurrentIndex;
                    else
                        return this.ActivityState.AbsoluteCount - 1;
                }
                else
                    return -1;

            }
        }

        public bool IsExecuting(int index)
        {
            if (this.ActivityState != null)
            {
                if (index < 0 || index >= this.ActivityState.AbsoluteCount)
                    throw new ArgumentOutOfRangeException("index");

                ChildExecutionStateInfo childStateInfo = this.ActivityState[index, false];
                return (childStateInfo.Status == ChildRunStatus.PendingExecute || childStateInfo.Status == ChildRunStatus.Running);
            }
            return false;
        }

        #endregion

        #region Public Events
        [SRDescription(SR.OnGeneratorChildInitializedDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler<ReplicatorChildEventArgs> ChildInitialized
        {
            add
            {
                base.AddHandler(ChildInitializedEvent, value);
            }
            remove
            {
                base.RemoveHandler(ChildInitializedEvent, value);
            }
        }

        [SRDescription(SR.OnGeneratorChildCompletedDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler<ReplicatorChildEventArgs> ChildCompleted
        {
            add
            {
                base.AddHandler(ChildCompletedEvent, value);
            }
            remove
            {
                base.RemoveHandler(ChildCompletedEvent, value);
            }
        }

        [SRDescription(SR.OnCompletedDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler Completed
        {
            add
            {
                base.AddHandler(CompletedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CompletedEvent, value);
            }
        }

        [SRDescription(SR.OnInitializedDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler Initialized
        {
            add
            {
                base.AddHandler(InitializedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InitializedEvent, value);
            }
        }

        #endregion

        #region ChildList Manipulation API
        private int Add(object value)
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Executing)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

            if (this.ActivityState == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

            ChildExecutionStateInfo childStateInfo = new ChildExecutionStateInfo(value);
            this.ActivityState.Add(childStateInfo);

            int indexOfAdd = this.ActivityState.AbsoluteCount - 1;
            ScheduleExecutionIfNeeded(childStateInfo, indexOfAdd);
            return indexOfAdd;
        }

        private int IndexOf(object value)
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Executing)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

            if (this.ActivityState == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

            int absoluteIndex = 0;

            for (int i = 0; i < this.ActivityState.Count; ++i)
            {
                ChildExecutionStateInfo childStateInfo = this.ActivityState[i];

                if (!childStateInfo.MarkedForRemoval)
                {
                    if (Object.Equals(childStateInfo.InstanceData, value))
                        return absoluteIndex;
                    else
                        ++absoluteIndex;
                }
            }

            return -1;
        }
        private void Insert(int index, object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (this.ExecutionStatus != ActivityExecutionStatus.Executing)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

            if (this.ActivityState == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

            if (index < 0 || index > this.ActivityState.AbsoluteCount)
                throw new ArgumentOutOfRangeException("index");

            ChildExecutionStateInfo childStateInfo = new ChildExecutionStateInfo(value);
            this.ActivityState.Insert(index, childStateInfo, false);

            ScheduleExecutionIfNeeded(childStateInfo, index);
        }
        private void Remove(object obj)
        {
            int index = this.IndexOf(obj);

            if (index < 0)
                return;

            RemoveAt(index);
            return;
        }
        private void RemoveAt(int index)
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Executing)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

            if (this.ActivityState == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

            if (index < 0 || index >= this.ActivityState.AbsoluteCount)
                throw new ArgumentOutOfRangeException("index");

            ChildExecutionStateInfo childStateInfo = this.ActivityState[index, false];

            if (childStateInfo.Status == ChildRunStatus.Completed || childStateInfo.Status == ChildRunStatus.Created)
                this.ActivityState.Remove(childStateInfo);
            else
            {
                childStateInfo.MarkedForRemoval = true;
                base.Invoke(this.HandleChildUpdateOperation, new ReplicatorInterActivityEventArgs(childStateInfo, false));
            }
        }

        private void Clear()
        {
            if (this.ExecutionStatus != ActivityExecutionStatus.Executing)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

            if (this.ActivityState == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

            while (this.ActivityState.AbsoluteCount != 0)
                this.RemoveAt(0);
        }
        #endregion

        #region Protected Methods
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            this.ActivityState = new ReplicatorStateInfo();
            base.RaiseEvent(ReplicatorActivity.InitializedEvent, this, EventArgs.Empty);

            if (this.InitialChildData != null)
            {
                //Add the ChildData to the execution info.
                for (int i = 0; i < this.InitialChildData.Count; ++i)
                {
                    this.Add(this.InitialChildData[i]);
                }
            }

            bool bCompleteNow = (this.UntilCondition == null);

            if (this.UntilCondition != null && this.UntilCondition.Evaluate(this, executionContext))
                bCompleteNow = true;
            else if (this.ActivityState.Count != 0)
                bCompleteNow = false;

            if (bCompleteNow)
            {
                //This is needed to make sure we dont reevaluate this again.
                this.ActivityState.CompletionConditionTrueAlready = true;

                //Try cool down child. It is ok to close here immediatley
                //since we are sure we havent executed child yet.
                if (!TryCancelChildren(executionContext))
                {
                    base.RaiseEvent(ReplicatorActivity.CompletedEvent, this, EventArgs.Empty);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Executing;
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            this.TryCancelChildren(executionContext);

            if (!this.ActivityState.IsChildActive)
            {
                //Check to make sure only once we call ReplciatorCompleted when we fault.
                if (this.ExecutionStatus == ActivityExecutionStatus.Faulting)
                {
                    if (this.ActivityState.AttemptedCloseWhileFaulting)
                        return ActivityExecutionStatus.Closed;

                    this.ActivityState.AttemptedCloseWhileFaulting = true;
                }
                base.RaiseEvent(ReplicatorActivity.CompletedEvent, this, EventArgs.Empty);
                return ActivityExecutionStatus.Closed;
            }
            return this.ExecutionStatus;
        }
        protected override void OnClosed(IServiceProvider provider)
        {
            //

        }
        #endregion

        #region Private Implementation
        #region Data
        //Runtime State Properties
        static DependencyProperty ActivityStateProperty = DependencyProperty.Register("ActivityState", typeof(ReplicatorStateInfo), typeof(ReplicatorActivity));
        ReplicatorStateInfo ActivityState
        {
            get
            {
                return (ReplicatorStateInfo)base.GetValue(ActivityStateProperty);
            }
            set
            {
                base.SetValue(ActivityStateProperty, value);
            }
        }
        #endregion

        #region Replicator Interactivity Event
        private sealed class ReplicatorInterActivityEventArgs : EventArgs
        {
            #region Data
            private bool isAdd = false;
            private ChildExecutionStateInfo childStateInfo;
            #endregion

            #region Properties
            internal bool IsAdd
            {
                get
                {
                    return this.isAdd;
                }
            }
            internal ChildExecutionStateInfo ChildStateInfo
            {
                get
                {
                    return this.childStateInfo;
                }
            }
            #endregion

            internal ReplicatorInterActivityEventArgs(ChildExecutionStateInfo childStateInfo, bool isAdd)
            {
                this.childStateInfo = childStateInfo;
                this.isAdd = isAdd;
            }
        }
        void HandleChildUpdateOperation(Object sender, ReplicatorInterActivityEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");

            ActivityExecutionContext executionContext = sender as ActivityExecutionContext;

            if (executionContext == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            if (this.ExecutionStatus != ActivityExecutionStatus.Executing)
                return;

            if (!e.IsAdd)
            {
                CancelChildExecution(executionContext, e.ChildStateInfo);
            }
            else
            {
                Debug.Assert(this.ActivityState.Contains(e.ChildStateInfo));
                Debug.Assert(e.ChildStateInfo.Status == ChildRunStatus.PendingExecute);
                ExecuteTemplate(executionContext, e.ChildStateInfo);
            }
        }

        private void CancelChildExecution(ActivityExecutionContext executionContext, ChildExecutionStateInfo childStateInfo)
        {
            // Mark the Instance For Removal
            System.Diagnostics.Debug.Assert(childStateInfo.MarkedForRemoval);

            // check if the instance is currently executing
            if (childStateInfo.Status != ChildRunStatus.Running) //It is passive, then we can safely remove the State.
            {
                this.ActivityState.Remove(childStateInfo);
                return;
            }

            // schedule the child cancellation
            // once this run is cancelled, the handleEvent should remove this from execution state.
            TryCancelChild(executionContext, childStateInfo);
        }
        #endregion

        #region Execution related helpers
        private void ExecuteTemplate(ActivityExecutionContext executionContext, ChildExecutionStateInfo childStateInfo)
        {
            System.Diagnostics.Debug.Assert(childStateInfo.Status != ChildRunStatus.Running);

            ActivityExecutionContextManager contextManager = executionContext.ExecutionContextManager;
            ActivityExecutionContext templateExecutionContext = contextManager.CreateExecutionContext(this.EnabledActivities[0]);
            childStateInfo.RunId = templateExecutionContext.ContextGuid;
            childStateInfo.Status = ChildRunStatus.Running;
            try
            {
                base.RaiseGenericEvent(ReplicatorActivity.ChildInitializedEvent, this, new ReplicatorChildEventArgs(childStateInfo.InstanceData, templateExecutionContext.Activity));
            }
            catch
            {
                childStateInfo.RunId = Guid.Empty;
                childStateInfo.Status = ChildRunStatus.Completed;
                contextManager.CompleteExecutionContext(templateExecutionContext);
                throw;
            }

            templateExecutionContext.ExecuteActivity(templateExecutionContext.Activity);
            templateExecutionContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, new ReplicatorSubscriber(this, templateExecutionContext.ContextGuid));
        }
        private void HandleStatusChange(ActivityExecutionContext executionContext, ActivityExecutionStatusChangedEventArgs e, ReplicatorSubscriber subscriber)
        {
            //System.Diagnostics.Debug.Assert(this.ExecutionStatus != ActivityExecutionStatus.Closed, "Stale notification should not have reache here");
            //System.Diagnostics.Debug.Assert(e.Activity.QualifiedName.Equals(this.EnabledActivities[0].QualifiedName), "Got status change notification of non existing child");
            //System.Diagnostics.Debug.Assert(subscriber.RunIdentifier != Guid.Empty, "Got notification from non-running template instance");

            //Perform cleanup on completed run.
            int runIndex = this.ActivityState.FindIndexOfChildStateInfo(subscriber.RunIdentifier);

            if (runIndex == -1)
            {
                //This will happen when CancelChild is issued after Child Closed 
                //but before StatusChange Event raised on parent.                
                return;
            }

            ChildExecutionStateInfo childStateInfo = this.ActivityState[runIndex];
            bool isMarkedForRemoval = childStateInfo.MarkedForRemoval;

            try
            {
                try
                {
                    base.RaiseGenericEvent(ReplicatorActivity.ChildCompletedEvent, this, new ReplicatorChildEventArgs(childStateInfo.InstanceData, e.Activity));
                    e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, subscriber);
                }
                finally
                {
                    ActivityExecutionContextManager contextManager = executionContext.ExecutionContextManager;
                    ActivityExecutionContext templateExecutionContext = contextManager.GetExecutionContext(e.Activity);
                    contextManager.CompleteExecutionContext(templateExecutionContext);
                }

                //Reevaluate CompletionCondition
                if (!this.ActivityState.CompletionConditionTrueAlready)
                    this.ActivityState.CompletionConditionTrueAlready = (this.UntilCondition != null && this.UntilCondition.Evaluate(this, executionContext));
            }
            finally //Always perform cleanup of just completed child.
            {
                //This will mark child as passive.
                childStateInfo.RunId = Guid.Empty;
                childStateInfo.Status = ChildRunStatus.Completed;

                if (isMarkedForRemoval)
                {
                    //This is the case, when user issued CancelChild request on running template instance.
                    //We flush out execution state of that run when it becomes passive.
                    this.ActivityState.Remove(childStateInfo);
                    runIndex = runIndex - 1; //Needed for sequence execution type.
                }
            }

            //Next Step.
            if (!this.ActivityState.IsChildActive) //Everything is passive now.
            {
                if (this.ExecutionStatus == ActivityExecutionStatus.Canceling || this.ExecutionStatus == ActivityExecutionStatus.Faulting || this.ActivityState.CompletionConditionTrueAlready)
                {
                    base.RaiseEvent(ReplicatorActivity.CompletedEvent, this, EventArgs.Empty);
                    executionContext.CloseActivity();
                    return;
                }
            }
            else //Template is active; Valid only for parallel
            {
                System.Diagnostics.Debug.Assert(this.ExecutionType == ExecutionType.Parallel);

                if (this.ExecutionStatus != ActivityExecutionStatus.Canceling && this.ExecutionStatus != ActivityExecutionStatus.Faulting)
                {
                    if (this.ActivityState.CompletionConditionTrueAlready)
                    {
                        //Try cool down child.
                        TryCancelChildren(executionContext);
                        return;
                    }
                }
            }

            switch (this.ExecutionType)
            {
                case ExecutionType.Sequence:
                    if (runIndex < this.ActivityState.Count - 1)
                    {
                        ExecuteTemplate(executionContext, this.ActivityState[runIndex + 1]);
                        return;
                    }
                    else if (this.UntilCondition == null || this.UntilCondition.Evaluate(this, executionContext))
                    {
                        base.RaiseEvent(ReplicatorActivity.CompletedEvent, this, EventArgs.Empty);
                        executionContext.CloseActivity();
                        return;
                    }
                    break;

                case ExecutionType.Parallel:
                    if (!this.ActivityState.IsChildActive && (this.UntilCondition == null || (this.UntilCondition.Evaluate(this, executionContext))))
                    {
                        base.RaiseEvent(ReplicatorActivity.CompletedEvent, this, EventArgs.Empty);
                        executionContext.CloseActivity();
                        return;
                    }
                    break;
                default:
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorInvalidExecutionType));

            }
        }
        bool TryCancelChildren(ActivityExecutionContext executionContext)
        {
            // returns true iff scheduled cancel on one or more executions of the template
            // false if all executions are already closed
            if (this.ActivityState == null)
                return false;

            ReplicatorStateInfo stateInfo = this.ActivityState;

            bool fScheduledCancel = false;
            for (int i = 0; i < stateInfo.Count; ++i)
            {
                if (this.TryCancelChild(executionContext, stateInfo[i]))
                {
                    fScheduledCancel = true;
                }
            }

            return fScheduledCancel;
        }
        bool TryCancelChild(ActivityExecutionContext outerProvider, ChildExecutionStateInfo childStateInfo)
        {
            // schedule cancellation of the child in the inner execution context
            bool fScheduledCancel = false;

            // returns true iff scheduled cancel on one execution of the template
            // false if execution already closed

            // get the execution context for this run
            ActivityExecutionContextManager contextManager = outerProvider.ExecutionContextManager;
            ActivityExecutionContext innerProvider = GetExecutionContext(contextManager, childStateInfo.RunId);
            if (innerProvider != null)
            {
                switch (innerProvider.Activity.ExecutionStatus)
                {
                    case ActivityExecutionStatus.Executing:
                        // schedule cancellation on child
                        innerProvider.CancelActivity(innerProvider.Activity);
                        fScheduledCancel = true;
                        break;

                    case ActivityExecutionStatus.Canceling:
                    case ActivityExecutionStatus.Faulting:
                        fScheduledCancel = true;
                        break;

                    default:
                        // do nothing
                        break;
                }
            }
            else
            {
                //Finish the run if it is pending for execution.
                if (this.ExecutionStatus != ActivityExecutionStatus.Executing && childStateInfo.Status == ChildRunStatus.PendingExecute)
                    childStateInfo.Status = ChildRunStatus.Completed;
            }
            return fScheduledCancel;
        }

        private ActivityExecutionContext GetExecutionContext(ActivityExecutionContextManager contextManager, Guid contextIdGuid)
        {
            foreach (ActivityExecutionContext context in contextManager.ExecutionContexts)
                if (context.ContextGuid == contextIdGuid)
                    return context;

            return null;
        }

        //Schedules execution if mode is parallel or if the insert is at head of empty list
        //or tail of all completed list in sequence case.
        void ScheduleExecutionIfNeeded(ChildExecutionStateInfo childStateInfo, int index)
        {
            bool bShouldExecute = (this.ExecutionType == ExecutionType.Parallel);

            if (!bShouldExecute) //Sequence Case.
            {
                //Execute if its head and only node or tail and previous tail already completed.
                int totalListSize = this.ActivityState.AbsoluteCount;

                if ((index == 0 && totalListSize == 1) || ((index == totalListSize - 1) && this.ActivityState[totalListSize - 2, false].Status == ChildRunStatus.Completed))
                    bShouldExecute = true;
            }

            if (bShouldExecute)
            {
                childStateInfo.Status = ChildRunStatus.PendingExecute;
                base.Invoke(this.HandleChildUpdateOperation, new ReplicatorInterActivityEventArgs(childStateInfo, true));
            }
        }
        #endregion

        #region Execution related Data structures
        [Serializable]
        class ReplicatorSubscriber : IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
        {
            #region Data
            private Guid runId;
            internal Guid RunIdentifier
            {
                get { return this.runId; }
            }
            #endregion

            internal ReplicatorSubscriber(Activity ownerActivity, Guid runIdentifier)
                : base()
            {
                this.runId = runIdentifier;
            }

            #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members
            void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
            {
                if (sender == null)
                    throw new ArgumentNullException("sender");

                ActivityExecutionContext context = sender as ActivityExecutionContext;

                if (context == null)
                    throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

                //



                ((ReplicatorActivity)context.Activity).HandleStatusChange(context, e, this);
            }
            #endregion

            #region Object Overrides
            public override bool Equals(object obj)
            {
                ReplicatorSubscriber subscriber = obj as ReplicatorSubscriber;
                return (subscriber != null && base.Equals(obj) && (this.runId.Equals(subscriber.runId)));
            }
            public override int GetHashCode()
            {
                return base.GetHashCode() ^ this.runId.GetHashCode();
            }
            #endregion
        }

        [Serializable]
        class ReplicatorStateInfo : List<ChildExecutionStateInfo>
        {
            //Fields / Properties
            internal bool CompletionConditionTrueAlready = false;
            internal bool AttemptedCloseWhileFaulting = false;
            internal bool IsChildActive
            {
                get
                {
                    for (int i = 0; i < this.Count; ++i)
                    {
                        ChildExecutionStateInfo childStateInfo = this[i];
                        if (childStateInfo.Status == ChildRunStatus.Running || childStateInfo.Status == ChildRunStatus.PendingExecute)
                            return true;
                    }

                    return false;
                }
            }

            internal int CurrentIndex
            {
                get
                {
                    for (int i = 0; i < this.AbsoluteCount; ++i)
                    {
                        if (this[i, false].RunId != Guid.Empty)
                            return i;
                    }

                    return this.AbsoluteCount - 1;
                }
            }

            //Helper Methods
            internal int FindIndexOfChildStateInfo(Guid runId)
            {
                for (int i = 0; i < this.Count; ++i)
                {
                    ChildExecutionStateInfo childStateInfo = this[i];

                    if (childStateInfo.RunId == runId)
                        return i;
                }

                Debug.Assert(false, "Child State Info not Found for the RunID");
                throw new IndexOutOfRangeException();
            }
            internal ChildExecutionStateInfo this[int index, bool includeStaleEntries]
            {
                get
                {
                    if (includeStaleEntries)
                        return this[index];

                    for (int i = 0; i < this.Count; ++i)
                    {
                        if (!this[i].MarkedForRemoval && index-- == 0)
                            return this[i];
                    }

                    throw new IndexOutOfRangeException();
                }
            }

            internal void Insert(int index, ChildExecutionStateInfo value, bool includeStaleEntries)
            {
                if (includeStaleEntries)
                {
                    Insert(index, value);
                    return;
                }

                int indexOfInsert = 0;
                for (indexOfInsert = 0; (indexOfInsert < this.Count) && index > 0; ++indexOfInsert)
                {
                    if (!this[indexOfInsert].MarkedForRemoval)
                        --index;
                }

                if (index == 0)
                    Insert(indexOfInsert, value);
                else
                    throw new IndexOutOfRangeException();
            }

            internal int Add(ChildExecutionStateInfo value, bool includeStaleEntries)
            {
                base.Add(value);

                if (includeStaleEntries)
                    return base.Count - 1;
                else
                    return this.AbsoluteCount - 1;
            }

            internal int AbsoluteCount
            {
                get
                {
                    int absoluteCount = 0;
                    int counter = 0;

                    while (counter < this.Count)
                    {
                        if (!this[counter++].MarkedForRemoval)
                            ++absoluteCount;
                    }
                    return absoluteCount;
                }
            }
        }

        [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
        enum ChildRunStatus : byte
        {
            Created, PendingExecute, Running, Completed
        }

        [Serializable]
        class ChildExecutionStateInfo
        {
            Object data;
            Guid runId;
            bool markedForRemoval;
            ChildRunStatus status;

            internal ChildRunStatus Status
            {
                get
                {
                    return this.status;
                }
                set
                {
                    this.status = value;
                }
            }

            internal Object InstanceData
            {
                get
                {
                    return this.data;
                }
                set
                {
                    this.data = value;
                }
            }

            internal Guid RunId
            {
                get
                {
                    return this.runId;
                }
                set
                {
                    this.runId = value;
                }
            }

            internal bool MarkedForRemoval
            {
                get
                {
                    return this.markedForRemoval;
                }
                set
                {
                    this.markedForRemoval = value;
                }
            }

            internal ChildExecutionStateInfo(Object instanceData)
            {
                this.data = instanceData;
                this.markedForRemoval = false;
                this.status = ChildRunStatus.Created;
            }
        }
        #endregion


        #endregion

        #region Replicator Child List Implementation
        [Serializable]
        private sealed class ReplicatorChildInstanceList : IList
        {
            ReplicatorActivity replicatorActivity;

            internal ReplicatorChildInstanceList(ReplicatorActivity replicatorActivity)
            {
                this.replicatorActivity = replicatorActivity;
            }

            #region IList Members

            int IList.Add(object value)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                return replicatorActivity.Add(value);
            }

            void IList.Clear()
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                replicatorActivity.Clear();
            }

            bool IList.Contains(object value)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                return replicatorActivity.IndexOf(value) != -1;
            }

            int IList.IndexOf(object value)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                return replicatorActivity.IndexOf(value);
            }

            void IList.Insert(int index, object value)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                replicatorActivity.Insert(index, value);
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            void IList.Remove(object value)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                replicatorActivity.Remove(value);
            }

            void IList.RemoveAt(int index)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                replicatorActivity.RemoveAt(index);
            }

            object IList.this[int index]
            {
                get
                {
                    if (replicatorActivity == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                    if (replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

                    if (replicatorActivity.ActivityState == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

                    return replicatorActivity.ActivityState[index, false].InstanceData;
                }
                set
                {
                    if (replicatorActivity == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                    if (replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

                    if (replicatorActivity.ActivityState == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

                    replicatorActivity.ActivityState[index, false].InstanceData = value;
                }
            }

            #endregion

            #region ICollection Members

            void ICollection.CopyTo(Array array, int index)
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                if (replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

                if (replicatorActivity.ActivityState == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

                if (array == null)
                    throw new ArgumentNullException("array");

                if (array.Rank != 1)
                    throw new ArgumentException(SR.GetString(SR.Error_MultiDimensionalArray), "array");

                if (index < 0)
                    throw new ArgumentOutOfRangeException("index");

                if (array.Length - index < replicatorActivity.ActivityState.AbsoluteCount)
                    throw new ArgumentException(SR.GetString(SR.Error_InsufficientArrayPassedIn), "array");

                for (int i = 0; i < replicatorActivity.ActivityState.AbsoluteCount; ++i)
                {
                    array.SetValue(replicatorActivity.ActivityState[i, false].InstanceData, i + index);
                }
            }

            int ICollection.Count
            {
                get
                {
#pragma warning disable 56503
                    if (replicatorActivity == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                    if (replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

                    if (replicatorActivity.ActivityState == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

                    return replicatorActivity.ActivityState.AbsoluteCount;
#pragma warning restore 56503
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
#pragma warning disable 56503
                    throw new NotImplementedException();
#pragma warning restore 56503
                }
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                if (replicatorActivity == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorDisconnected));

                if (replicatorActivity.ExecutionStatus != ActivityExecutionStatus.Executing)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotExecuting));

                if (replicatorActivity.ActivityState == null)
                    throw new InvalidOperationException(SR.GetString(SR.Error_ReplicatorNotInitialized));

                for (int i = 0; i < replicatorActivity.ActivityState.AbsoluteCount; ++i)
                    yield return replicatorActivity.ActivityState[i, false].InstanceData;

            }

            #endregion
        }
        #endregion
    }

    #region ReplicatorEventArgs
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ReplicatorChildEventArgs : EventArgs
    {
        private object instanceData = null;
        private Activity activity = null;

        public ReplicatorChildEventArgs(object instanceData, Activity activity)
        {
            this.instanceData = instanceData;
            this.activity = activity;
        }

        public object InstanceData
        {
            get
            {
                return this.instanceData;
            }
        }

        public Activity Activity
        {
            get
            {
                return this.activity;
            }
        }
    }
    #endregion

    #region Execution Type Enum
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ExecutionType
    {
        Sequence = 0,
        Parallel = 1
    }
    #endregion

    #region Validator
    internal sealed class ReplicatorValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ReplicatorActivity replicator = obj as ReplicatorActivity;
            if (replicator == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(ReplicatorActivity).FullName), "obj");

            if ((replicator.EnabledActivities.Count != 1))
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_GeneratorShouldContainSingleActivity), ErrorNumbers.Error_GeneratorShouldContainSingleActivity));

            return validationErrors;
        }
    }
    #endregion
}
