namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.CodeDom;
    using System.Drawing;
    using System.Collections;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime.DebugEngine;
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.ConstrainedGroupActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(ConditionedActivityGroupDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(ConditionedActivityGroup), "Resources.cag.png")]
    [ActivityValidator(typeof(ConditionedActivityGroupValidator))]
    [SRCategory(SR.Standard)]
    [WorkflowDebuggerSteppingAttribute(WorkflowDebuggerSteppingOption.Concurrent)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ConditionedActivityGroup : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        //Attached properties provided to the children
        public static readonly DependencyProperty WhenConditionProperty = DependencyProperty.RegisterAttached("WhenCondition", typeof(ActivityCondition), typeof(ConditionedActivityGroup), new PropertyMetadata(DependencyPropertyOptions.Metadata), typeof(WhenUnlessConditionDynamicPropertyValidator));

        // metadata properties go here
        public static readonly DependencyProperty UntilConditionProperty = DependencyProperty.Register("UntilCondition", typeof(ActivityCondition), typeof(ConditionedActivityGroup), new PropertyMetadata(DependencyPropertyOptions.Metadata));

        #region Constructors

        public ConditionedActivityGroup()
        {
        }

        public ConditionedActivityGroup(string name)
            : base(name)
        {
        }

        #endregion

        // WhenConditionProperty Get and Set Accessors
        public static object GetWhenCondition(object dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");
            if (!(dependencyObject is DependencyObject))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(DependencyObject).FullName), "dependencyObject");

            return (dependencyObject as DependencyObject).GetValue(WhenConditionProperty);
        }

        public static void SetWhenCondition(object dependencyObject, object value)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");
            if (!(dependencyObject is DependencyObject))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(DependencyObject).FullName), "dependencyObject");

            (dependencyObject as DependencyObject).SetValue(WhenConditionProperty, value);
        }

        [SRCategory(SR.Conditions)]
        [SRDescription(SR.UntilConditionDescr)]
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
        private Activity GetDynamicActivity(Activity childActivity)
        {
            if (childActivity == null)
                throw new ArgumentNullException("childActivity");

            if (!this.EnabledActivities.Contains(childActivity))
                throw new ArgumentException(SR.GetString(SR.Error_CAGChildNotFound, childActivity.QualifiedName, this.QualifiedName), "childActivity");
            else
            {
                Activity[] dynamicChildActivity = this.GetDynamicActivities(childActivity);

                if (dynamicChildActivity.Length != 0)
                    return dynamicChildActivity[0];
                else
                    return null;
            }
        }

        public Activity GetDynamicActivity(String childActivityName)
        {
            if (childActivityName == null)
                throw new ArgumentNullException("childActivityName");

            Activity childActivity = null;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                if (this.EnabledActivities[i].QualifiedName.Equals(childActivityName))
                {
                    childActivity = this.EnabledActivities[i];
                    break;
                }
            }

            if (childActivity != null)
                return GetDynamicActivity(childActivity);

            throw new ArgumentException(SR.GetString(SR.Error_CAGChildNotFound, childActivityName, this.QualifiedName), "childActivityName");
        }

        public int GetChildActivityExecutedCount(Activity child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            ConditionedActivityGroupStateInfo conditionedInfo = this.CAGState;

            if (conditionedInfo == null)
            {
                throw new InvalidOperationException(SR.GetString(SR.Error_CAGNotExecuting, this.QualifiedName));
            }

            if (!conditionedInfo.ChildrenStats.ContainsKey(child.QualifiedName))
            {
                throw new ArgumentException(SR.GetString(SR.Error_CAGChildNotFound, child.QualifiedName, this.QualifiedName), "child");
            }
            else
            {
                return conditionedInfo.ChildrenStats[child.QualifiedName].ExecutedCount;
            }
        }

        private sealed class WhenUnlessConditionDynamicPropertyValidator : Validator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection validationErrors = ValidationHelpers.ValidateObject(manager, obj);

                if (validationErrors.Count == 0)
                {
                    Activity activity = manager.Context[typeof(Activity)] as Activity;
                    if (activity == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ContextStackItemMissing, typeof(Activity).Name));

                    CodeCondition codeCondition = obj as CodeCondition;
                    if (codeCondition != null && codeCondition.IsBindingSet(CodeCondition.ConditionEvent))
                    {
                        ActivityBind activityBind = codeCondition.GetBinding(CodeCondition.ConditionEvent) as ActivityBind;
                        if (activityBind != null)
                        {
                            Activity contextActivity = Helpers.ParseActivityForBind(activity, activityBind.Name);
                            if (contextActivity != null && Helpers.IsChildActivity(activity.Parent, contextActivity))
                            {
                                string propertyName = GetFullPropertyName(manager);
                                ValidationError error = new ValidationError(SR.GetString(SR.Error_NestedConstrainedGroupConditions, propertyName), ErrorNumbers.Error_NestedConstrainedGroupConditions);
                                error.PropertyName = propertyName;
                                validationErrors.Add(error);
                            }
                        }
                    }
                }

                return validationErrors;
            }
        }

        #region Runtime Internal Dependency Property
        static DependencyProperty CAGStateProperty = DependencyProperty.Register("CAGState", typeof(ConditionedActivityGroupStateInfo), typeof(ConditionedActivityGroup));

        internal ConditionedActivityGroupStateInfo CAGState
        {
            get
            {
                return (ConditionedActivityGroupStateInfo)base.GetValue(CAGStateProperty);
            }
            set
            {
                base.SetValue(CAGStateProperty, value);
            }
        }
        #endregion

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(ConditionedActivityGroup.CAGStateProperty);
        }

        #region Workflow Changes Overrides
        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            if (!addedActivity.Enabled)
                return;

            ConditionedActivityGroup currentActivity = executionContext.Activity as ConditionedActivityGroup;
            Debug.Assert(currentActivity != null);

            ConditionedActivityGroupStateInfo state = currentActivity.CAGState;
            if (currentActivity.ExecutionStatus == ActivityExecutionStatus.Executing && state != null)
            {
                Debug.Assert(currentActivity == addedActivity.Parent, "Attempting to add wrong activity to CAG");
                state.ChildrenStats[addedActivity.QualifiedName] = new CAGChildStats();
            }
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (removedActivity == null)
                throw new ArgumentNullException("removedActivity");

            if (!removedActivity.Enabled)
                return;

            ConditionedActivityGroup cag = executionContext.Activity as ConditionedActivityGroup;
            Debug.Assert(cag != null);

            // find out the status of the cag

            ConditionedActivityGroupStateInfo state = cag.CAGState;
            if ((cag.ExecutionStatus == ActivityExecutionStatus.Executing) && (state != null))
            {
                state.ChildrenStats.Remove(removedActivity.QualifiedName);
            }
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            // find out the status of the cag
            ConditionedActivityGroup currentActivity = executionContext.Activity as ConditionedActivityGroup;

            // if CAG is executing... fire the conditions on the net result
            if (currentActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                // but hold on, a derived cag could be applying model changes before it
                // "really" starts executing the activities. In that case it will evaluate
                // the conditions later, at the appropriate time.
                ConditionedActivityGroupStateInfo state = currentActivity.CAGState;
                if ((state != null) && (!state.Testing))
                {
                    // fire away...  fire away... said the CAG
                    if (this.EvaluateConditions(currentActivity, executionContext))
                    {
                        // CAG until indicates we are done, so no children execute
                        this.Cleanup(currentActivity, executionContext);
                    }
                    else
                    {
                        // start any pending activity required
                        this.TriggerChildren(currentActivity, executionContext);
                    }
                }
            }
        }

        #endregion

        #region Execution Implementation
#if	LOG
        private static void Log(string message)
        {
            Trace.WriteLine(message);
        }
#endif

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
#if	LOG
            Log("Execute on " + this.QualifiedName);
#endif

            // go figure out what the CAG needs to do
            this.CAGState = new ConditionedActivityGroupStateInfo(this);

            if (EvaluateConditions(this, executionContext))
            {
                // CAG until indicates we are done, so no children execute
                return ActivityExecutionStatus.Closed;
            }

            // start any pending activity required
            TriggerChildren(this, executionContext);
            return this.ExecutionStatus;
        }

        /// <summary>
        /// Evaluate the conditions on the CAG
        /// </summary>
        /// <param name="cag"></param>
        /// <param name="context"></param>
        /// <returns>True if CAG is complete (UNTIL == true, or no UNTIL and no children execute), false otherwise</returns>
        internal bool EvaluateConditions(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            Debug.Assert(cag != null);
            Debug.Assert(context != null);
#if	LOG
            Log("EvaluateConditions on " + cag.QualifiedName);
            cag.CAGState.DumpState("Before EvaluateConditions");
#endif
            // if we've already decided to quit this CAG, don't do anything
            if (cag.CAGState.Completed)
                return false;

            // if the cag has an UNTIL condition, execute it
            if ((cag.UntilCondition != null) && cag.UntilCondition.Evaluate(cag, context))
            {
                // UNTIL condition says we're done, no need to look at children
#if	LOG
                Log("Until condition is true");
#endif
                return true;
            }

            // until condition is false, so let's look at all children
            int childExecuting = 0; // keep track of children executing
            Dictionary<string, CAGChildStats> childrenStats = cag.CAGState.ChildrenStats;
            foreach (Activity act in cag.EnabledActivities)
            {
                // if we think the child is executing, do nothing
                if (childrenStats[act.QualifiedName].State == CAGChildState.Excuting)
                {
                    ++childExecuting;
                    continue;
                }

                // find the run-time activity
                Activity activity = GetRuntimeInitializedActivity(context, act);
                // should it execute?
                if (EvaluateChildConditions(cag, activity, context))
                {
                    ++childExecuting;
                    childrenStats[act.QualifiedName].State = CAGChildState.Pending;
                }
            }
#if	LOG
            cag.CAGState.DumpState("After EvaluateConditions");
#endif

            // if any work to do, CAG not yet done
            if (childExecuting > 0)
                return false;

            // CAG is quiet (nothing more to do)
            // if specified an UNTIL condition but we have nothing to do
            if (cag.UntilCondition != null)
            {
#if	LOG
                Log("CAG quiet, but UNTIL condition is false, so error time");
#endif
                throw new InvalidOperationException(SR.GetString(SR.Error_CAGQuiet, cag.QualifiedName));
            }
#if	LOG
            Log("CAG quiet");
#endif
            return true;
        }

        /// <summary>
        /// Evaluate the While condition for a particular child of the CAG
        /// If no While condition, it becomes "execute once"
        /// </summary>
        /// <param name="cag"></param>
        /// <param name="child"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool EvaluateChildConditions(ConditionedActivityGroup cag, Activity child, ActivityExecutionContext context)
        {
#if	LOG
            Log("EvaluateChildConditions on activity " + child.QualifiedName + " inside " + cag.QualifiedName);
#endif
            // determine the result of the when condition (evaluate once if not specified)
            ConditionedActivityGroupStateInfo state = cag.CAGState;
            try
            {
                state.Testing = true;
                ActivityCondition whenCondition = (ActivityCondition)child.GetValue(ConditionedActivityGroup.WhenConditionProperty);
                return (whenCondition != null)
                    ? whenCondition.Evaluate(child, context)
                    : (state.ChildrenStats[child.QualifiedName].ExecutedCount == 0);
            }
            finally
            {
                state.Testing = false;
            }
        }

        /// <summary>
        /// Start any child activities that need to be run
        /// </summary>
        /// <param name="cag"></param>
        /// <param name="context"></param>
        internal void TriggerChildren(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            Debug.Assert(cag != null);
            Debug.Assert(context != null);
#if	LOG
            Log("TriggerChildren on " + cag.QualifiedName);
            cag.CAGState.DumpState("Before TriggerChildren");
#endif

            Dictionary<string, CAGChildStats> childrenStats = cag.CAGState.ChildrenStats;
            // until condition is false, so let's look at all children
            foreach (Activity act in cag.EnabledActivities)
            {
                // do we think this child needs to run?
                if (childrenStats[act.QualifiedName].State != CAGChildState.Pending)
                    continue;

                // find the run-time activity
                Activity activity = GetRuntimeInitializedActivity(context, act);
                if (activity.ExecutionStatus == ActivityExecutionStatus.Initialized)
                    ExecuteChild(cag, activity, context);
            }
#if	LOG
            cag.CAGState.DumpState("After TriggerChildren");
#endif
        }

        private void ExecuteChild(ConditionedActivityGroup cag, Activity childActivity, ActivityExecutionContext context)
        {
            Debug.Assert(cag != null);
            Debug.Assert(childActivity != null);
            Debug.Assert(context != null);
            Debug.Assert(childActivity.ExecutionStatus == ActivityExecutionStatus.Initialized);
#if	LOG
            Log("ExecuteChild " + childActivity.QualifiedName + " inside " + cag.QualifiedName);
#endif
            ActivityExecutionContext childContext = GetChildExecutionContext(context, childActivity, true);
            cag.CAGState.ChildrenStats[childActivity.QualifiedName].State = CAGChildState.Excuting;

            // subscribe for child closure
            childContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);

            // execute child in inner context
            childContext.ExecuteActivity(childContext.Activity);
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            // child activities are cancelled and could complete asynchronously.
            // if there was no asynchronous stuff then lets do the cag level cleanup

            // if we are already done (or never started), then we are already closed
            if (this.CAGState == null)
                return ActivityExecutionStatus.Closed;

            return Cleanup(this, executionContext) ? ActivityExecutionStatus.Closed : ActivityExecutionStatus.Canceling;
        }

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members
        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            this.HandleEvent(sender as ActivityExecutionContext, new SubscriptionEventArg(e, EventType.StatusChange));
        }
        #endregion

        internal void HandleEvent(ActivityExecutionContext context, SubscriptionEventArg e)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (e == null)
                throw new ArgumentNullException("e");

            ConditionedActivityGroup cag = context.Activity as ConditionedActivityGroup;
            if (cag == null)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidCAGActivityType), "activity");

            // Already done the cleanup from another child's signalling
            if (cag.ExecutionStatus == ActivityExecutionStatus.Closed)
                return;

            if (e.SubscriptionType != EventType.StatusChange)
            {
                // split into seperate test to keep FxCop happy (only place SubscriptionType used)
                Debug.Assert(false, "This CAG activity handler does not handle this event");
            }
            ActivityExecutionStatusChangedEventArgs args1 = (ActivityExecutionStatusChangedEventArgs)e.Args;
#if	LOG
            Log("HandleEvent for " + cag.QualifiedName);
            Log("event = " + e.ToString());
            Log("activity = " + args1.Activity.QualifiedName);
#endif

            bool timeToQuit = false;

            // is this event is for an immediate child?
            Debug.Assert(cag == args1.Activity.Parent, "Received event for non-child of CAG");
            Dictionary<string, CAGChildStats> childrenStats = cag.CAGState.ChildrenStats;

            // it is possible that dynamic update has removed the child before we get the closed event
            // if that is the case, we don't need to update it's stats since it's not there
            if (childrenStats.ContainsKey(args1.Activity.QualifiedName))
            {
                // update our state about the child
                if (args1.ExecutionStatus != ActivityExecutionStatus.Executing)
                    childrenStats[args1.Activity.QualifiedName].State = CAGChildState.Idle;

                // @undone: this will break if scopes move to "Delayed" closing after Completed.
                if (args1.ExecutionStatus == ActivityExecutionStatus.Closed)
                    childrenStats[args1.Activity.QualifiedName].ExecutedCount++;

                try
                {
                    // re-evaluate the conditions on any status change, as long as the CAG is still executing
                    if (cag.ExecutionStatus == ActivityExecutionStatus.Executing)
                        timeToQuit = EvaluateConditions(cag, context);
                }
                finally
                {
                    // get rid of the child that just completed
                    // do this in the finally so that the child is cleaned up, 
                    // even if EvaluateConditions throws beause the CAG is stalled
                    CleanupChildAtClosure(context, args1.Activity);
                }
            }
            else
            {
                // child has been removed
                // we still need to see if the CAG is done, provided we are still executing
                if (cag.ExecutionStatus == ActivityExecutionStatus.Executing)
                    timeToQuit = EvaluateConditions(cag, context);
            }

            // is the CAG just completed?
            if (timeToQuit)
            {
                Cleanup(cag, context);
            }
            else if (cag.CAGState.Completed)
            {
                // if the CAG is simply waiting for all children to complete, see if this is the last one
                if (AllChildrenQuiet(cag, context))
                {
                    // Mark the CAG as closed, if it hasn't already been marked so.
                    context.CloseActivity();
                }
            }
            else
            {
                // CAG not done, so see if any children need to start
                TriggerChildren(cag, context);
            }
        }

        internal bool Cleanup(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            // the completion condition has fired, or we are canceling
            // either way, we want to cleanup
            ConditionedActivityGroupStateInfo state = cag.CAGState;
            state.Completed = true;

            // cancel any children currently running
            bool childrenActive = false;
            Dictionary<string, CAGChildStats> childrenStats = state.ChildrenStats;
            foreach (Activity act in cag.EnabledActivities)
            {
                // reset any Pending Execution for all child activity
                if (childrenStats[act.QualifiedName].State == CAGChildState.Pending)
                    childrenStats[act.QualifiedName].State = CAGChildState.Idle;

                // find the run-time activity
                ActivityExecutionContext childContext = GetChildExecutionContext(context, act, false);
                if (childContext != null)
                {
                    // child must be running somewhere
                    Activity activity = GetRuntimeInitializedActivity(context, act);
                    switch (activity.ExecutionStatus)
                    {
                        case ActivityExecutionStatus.Executing:
                            // schedule cancellation on child
                            childContext.CancelActivity(activity);
                            childrenActive = true;
                            break;

                        case ActivityExecutionStatus.Canceling:
                        case ActivityExecutionStatus.Faulting:
                            childrenActive = true;
                            break;

                        case ActivityExecutionStatus.Closed:
                            CleanupChildAtClosure(context, activity);
                            break;
                        default:
                            // unhook our handler
                            // others will be removed when we get the complete/cancel notification
                            act.UnregisterForStatusChange(Activity.ClosedEvent, this);
                            break;
                    }
                }
            }

            // if the CAG is quiet, we are all done
            if (!childrenActive)
                context.CloseActivity();
            return !childrenActive;
        }

        private void CleanupChildAtClosure(ActivityExecutionContext context, Activity childActivity)
        {
            Debug.Assert(context != null);
            Debug.Assert(childActivity != null);
            Debug.Assert(childActivity.ExecutionStatus == ActivityExecutionStatus.Closed);

            //UnSubsribe child closure of completed activity.
            childActivity.UnregisterForStatusChange(Activity.ClosedEvent, this);

            //Dispose the execution context;
            ActivityExecutionContext childContext = GetChildExecutionContext(context, childActivity, false);
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            contextManager.CompleteExecutionContext(childContext);
        }

        private Activity GetRuntimeInitializedActivity(ActivityExecutionContext context, Activity childActivity)
        {
            ActivityExecutionContext childContext = GetChildExecutionContext(context, childActivity, false);

            if (childContext == null)
                return childActivity;

            return childContext.Activity;
        }

        private static ActivityExecutionContext GetChildExecutionContext(ActivityExecutionContext context, Activity childActivity, bool createIfNotExists)
        {
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            ActivityExecutionContext childContext = contextManager.GetExecutionContext(childActivity);
            if (childContext != null)
                return childContext;

            if (createIfNotExists)
                childContext = contextManager.CreateExecutionContext(childActivity);

            return childContext;
        }

        bool AllChildrenQuiet(ConditionedActivityGroup cag, ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // if there are any execution contexts, 1 or more children still doing something
            foreach (ActivityExecutionContext activeContext in context.ExecutionContextManager.ExecutionContexts)
            {
                if (cag.GetActivityByName(activeContext.Activity.QualifiedName, true) != null)
                {
                    return false;
                }
            }

            // no children
            return true;
        }
    }

    internal sealed class SubscriptionEventArg : EventArgs
    {
        private EventArgs _args;
        private EventType _subscriptionType;

        internal EventArgs Args
        {
            get { return _args; }
        }

        internal EventType SubscriptionType
        {
            get { return _subscriptionType; }
        }

        public override string ToString()
        {
            return "SubscriptionEventArg(" + (_args == null ? "null" : _args.ToString()) + ")";
        }

        internal SubscriptionEventArg(EventArgs args, EventType subType)
        {
            _args = args;
            _subscriptionType = subType;
        }
    }

    [Serializable]
    internal enum EventType : byte
    {
        Timer = 0,
        DataChange = 1,
        StatusChange = 2,
        MessageArrival = 3,
        LockAcquisition = 4,
        InterActivity = 6,
    }

    #region ConditionedActivityGroupStateInfo
    [Serializable]
    internal sealed class ConditionedActivityGroupStateInfo
    {
        private bool completed;
        private bool testing;
        private Dictionary<string, CAGChildStats> childActivityStats;

        #region Accessors

        internal bool Completed
        {
            get { return this.completed; }
            set { this.completed = value; }
        }

        internal bool Testing
        {
            get { return testing; }
            set { testing = value; }
        }

        internal Dictionary<string, CAGChildStats> ChildrenStats
        {
            get { return this.childActivityStats; }
        }

        #endregion Accessors

        internal ConditionedActivityGroupStateInfo(ConditionedActivityGroup cag)
        {
            int len = cag.EnabledActivities.Count;
            this.childActivityStats = new Dictionary<string, CAGChildStats>(len);
            foreach (Activity act in cag.EnabledActivities)
                this.childActivityStats[act.QualifiedName] = new CAGChildStats();
        }

#if	LOG
        internal void DumpState(string message)
        {
            Trace.WriteLine(message + " completed = " + Completed.ToString());
            foreach (string key in this.childActivityStats.Keys)
            {
                Trace.WriteLine(key + ": state = " + this.childActivityStats[key].State + ", performed = " + this.childActivityStats[key].ExecutedCount);
            }
        }
#endif

    }

    [Serializable]
    internal enum CAGChildState : byte
    {
        Idle,
        Pending,
        Excuting
    }

    [Serializable]
    internal class CAGChildStats
    {
        internal int ExecutedCount = 0;
        internal CAGChildState State = CAGChildState.Idle;
        internal CAGChildStats()
        { }
    }
    #endregion

        #endregion


    #region Validator
    internal sealed class ConditionedActivityGroupValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ConditionedActivityGroup conditionedActivityGroup = obj as ConditionedActivityGroup;
            if (conditionedActivityGroup == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(ConditionedActivityGroup).FullName), "obj");

            return validationErrors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (action == null)
                throw new ArgumentNullException("action");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized &&
                activity.ExecutionStatus != ActivityExecutionStatus.Executing &&
                activity.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                return new ValidationError(SR.GetString(SR.Error_DynamicActivity2, activity.QualifiedName, activity.ExecutionStatus, activity.GetType().FullName), ErrorNumbers.Error_DynamicActivity2);
            }

            // if we are currently executing, make sure that we are not changing something already running
            // removed since changes mean that the child activity is going to get validated anyway

            return null;
        }
    }
    #endregion
}
