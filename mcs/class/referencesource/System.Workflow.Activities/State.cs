namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Diagnostics;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Serialization;
    using System.Xml.Serialization;
    using System.Workflow.ComponentModel.Compiler;

    #endregion

    [SRDescription(SR.StateActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(StateDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(StateActivity), "Resources.StateActivity.png")]
    [ActivityValidator(typeof(StateActivityValidator))]
    [SRCategory(SR.Standard)]
    [System.Runtime.InteropServices.ComVisible(false)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class StateActivity : CompositeActivity
    {
        #region Fields

        public const string StateChangeTrackingDataKey = "StateActivity.StateChange";
        internal static DependencyProperty StateMachineExecutionStateProperty = DependencyProperty.Register(StateMachineExecutionState.StateMachineExecutionStateKey, typeof(StateMachineExecutionState), typeof(StateActivity), new PropertyMetadata());

        #endregion Fields

        #region Constructor

        public StateActivity()
        {
        }

        public StateActivity(string name)
            : base(name)
        {
        }

        #endregion Constructor

        #region Methods

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(StateActivity.StateMachineExecutionStateProperty);
        }

        #region Public Methods

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Activity GetDynamicActivity(Activity childActivity)
        {
            if (childActivity == null)
                throw new ArgumentNullException("childActivity");
            if (!this.EnabledActivities.Contains(childActivity))
                throw new ArgumentException(SR.GetString(SR.Error_StateChildNotFound), "childActivity");
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

            throw new ArgumentException(SR.GetString(SR.Error_StateChildNotFound), "childActivityName");
        }

        #endregion Public Methods

        protected override void Initialize(IServiceProvider provider)
        {
            base.Initialize(provider);

            ActivityExecutionContext context = (ActivityExecutionContext)provider;

            StateActivity rootState = StateMachineHelpers.GetRootState(this);
            if (!StateMachineHelpers.IsStateMachine(rootState))
                throw new InvalidOperationException(SR.GetError_StateActivityMustBeContainedInAStateMachine());

            string initialStateName = StateMachineHelpers.GetInitialStateName(this);
            if (String.IsNullOrEmpty(initialStateName))
                throw new InvalidOperationException(SR.GetError_CannotExecuteStateMachineWithoutInitialState());

            // 


            if (this.QualifiedName != initialStateName)
                StateMachineSubscriptionManager.DisableStateWorkflowQueues(context, this);
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (StateMachineHelpers.IsRootState(this))
            {
                ExecuteRootState(executionContext);
            }
            else
            {
                if (StateMachineHelpers.IsLeafState(this))
                {
                    ExecuteLeafState(executionContext);
                }
                else
                {
                    ExecuteState(executionContext);
                }
            }

            return this.ExecutionStatus;
        }

        private void ExecuteRootState(ActivityExecutionContext context)
        {
            StateActivity state = (StateActivity)context.Activity;
            StateMachineExecutionState executionState = new StateMachineExecutionState(this.WorkflowInstanceId);
            executionState.SchedulerBusy = false;
            state.SetValue(StateActivity.StateMachineExecutionStateProperty, executionState);
            executionState.SubscriptionManager.CreateSetStateEventQueue(context);

            string initialStateName = StateMachineHelpers.GetInitialStateName(state);
            executionState.CalculateStateTransition(this, initialStateName);
            executionState.ProcessActions(context);
        }

        private static void ExecuteState(ActivityExecutionContext context)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            executionState.SchedulerBusy = false;
            executionState.ProcessActions(context);
        }

        private static void ExecuteLeafState(ActivityExecutionContext context)
        {
            StateActivity state = (StateActivity)context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(state);
            executionState.SchedulerBusy = false;

            executionState.CurrentStateName = state.QualifiedName;

            StateInitializationActivity stateInitialization = GetStateInitialization(context);
            if (stateInitialization != null)
            {
                ExecuteStateInitialization(context, stateInitialization);
            }
            else
            {
                EnteringLeafState(context);
            }
        }

        private static void EnteringLeafState(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            StateActivity state = (StateActivity)context.Activity;
            Debug.Assert(StateMachineHelpers.IsLeafState(state));

            StateMachineExecutionState executionState = GetExecutionState(state);
            executionState.SubscriptionManager.SubscribeToSetStateEvent(context);

            string completedStateName = StateMachineHelpers.GetCompletedStateName(state);
            if (StateMachineHelpers.IsCompletedState(state))
            {
                // make sure that we track that we entered the completed state
                EnteringStateAction enteringState = new EnteringStateAction(state.QualifiedName);
                executionState.EnqueueAction(enteringState);
                executionState.ProcessActions(context);

                // this is the final state, so we start completing this tree
                executionState.Completed = true;
                LeavingState(context);
            }
            else
            {
                if (String.IsNullOrEmpty(executionState.NextStateName))
                {
                    executionState.SubscriptionManager.ReevaluateSubscriptions(context);
                    EnteringStateAction enteringState = new EnteringStateAction(state.QualifiedName);
                    executionState.EnqueueAction(enteringState);
                    executionState.LockQueue();
                }
                else
                {
                    // The StateInitialization requested a state transtion
                    EnteringStateAction enteringState = new EnteringStateAction(state.QualifiedName);
                    executionState.EnqueueAction(enteringState);
                    executionState.ProcessTransitionRequest(context);
                }
                executionState.ProcessActions(context);
            }
        }

        internal static void LeavingState(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            StateActivity state = (StateActivity)context.Activity;
            if (StateMachineHelpers.IsLeafState(state))
            {
                StateFinalizationActivity stateFinalization = GetStateFinalization(context);
                if (stateFinalization == null)
                    Complete(context);
                else
                    ExecuteStateFinalization(context, stateFinalization);
            }
            else
                Complete(context);
        }

        private static void CleanUp(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            StateActivity state = (StateActivity)context.Activity;

            if (state.ExecutionStatus == ActivityExecutionStatus.Faulting)
                return; // if we're faulting, then we're already in a bad state, so we don't try to unsubscribe

            StateMachineExecutionState executionState = GetExecutionState(state);
            StateMachineSubscriptionManager subscriptionManager = executionState.SubscriptionManager;
            subscriptionManager.UnsubscribeState(context);

            if (StateMachineHelpers.IsRootState(state))
                subscriptionManager.DeleteSetStateEventQueue(context);
            else if (StateMachineHelpers.IsLeafState(state))
                subscriptionManager.UnsubscribeToSetStateEvent(context);
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            CleanUp(executionContext);

            Debug.Assert(executionContext.Activity == this);

            bool canCloseNow = true;
            ActivityExecutionContextManager contextManager = executionContext.ExecutionContextManager;

            foreach (ActivityExecutionContext existingContext in contextManager.ExecutionContexts)
            {
                if (existingContext.Activity.Parent == this)
                {
                    canCloseNow = false;

                    if (existingContext.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                        existingContext.CancelActivity(existingContext.Activity);
                }
            }
            return canCloseNow ? ActivityExecutionStatus.Closed : this.ExecutionStatus;
        }

        private static void Complete(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            StateActivity state = (StateActivity)context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(state);
            if (StateMachineHelpers.IsLeafState(state))
            {
                executionState.PreviousStateName = state.Name;
            }

            CleanUp(context);
            executionState.SchedulerBusy = true;
            context.CloseActivity();
        }

        private static void ExecuteChild(ActivityExecutionContext context, Activity childActivity)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (childActivity == null)
                throw new ArgumentNullException("childActivity");
            StateActivity state = (StateActivity)context.Activity;

            StateMachineExecutionState executionState = GetExecutionState(state);
            Debug.Assert(!executionState.SchedulerBusy);
            executionState.SchedulerBusy = true;
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            ActivityExecutionContext childContext = contextManager.CreateExecutionContext(childActivity);
            childContext.Activity.Closed += state.HandleChildActivityClosed;
            childContext.ExecuteActivity(childContext.Activity);
        }

        private static void CleanupChildAtClosure(ActivityExecutionContext context, Activity childActivity)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (childActivity == null)
                throw new ArgumentNullException("childActivity");
            StateActivity state = (StateActivity)context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(state);

            childActivity.Closed -= state.HandleChildActivityClosed;

            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            ActivityExecutionContext childContext = contextManager.GetExecutionContext(childActivity);
            contextManager.CompleteExecutionContext(childContext);
        }

        internal void RaiseProcessActionEvent(ActivityExecutionContext context)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            Debug.Assert(!executionState.SchedulerBusy);
            executionState.SchedulerBusy = true;
            base.Invoke<EventArgs>(this.HandleProcessActionEvent, new EventArgs());
        }

        private void HandleProcessActionEvent(object sender,
            EventArgs eventArgs)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            StateMachineExecutionState executionState = GetExecutionState(context);
            executionState.SchedulerBusy = false;
            executionState.ProcessActions(context);
        }

        #region HandleStatusChange

        private void HandleChildActivityClosed(object sender, ActivityExecutionStatusChangedEventArgs eventArgs)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            if (eventArgs == null)
                throw new ArgumentNullException("eventArgs");

            Activity completedChildActivity = eventArgs.Activity;
            StateActivity state = (StateActivity)context.Activity;

            StateMachineExecutionState executionState = GetExecutionState(context);
            executionState.SchedulerBusy = false;

            CleanupChildAtClosure(context, completedChildActivity);
            switch (state.ExecutionStatus)
            {
                case ActivityExecutionStatus.Canceling:
                case ActivityExecutionStatus.Faulting:
                    context.CloseActivity();
                    return;

                case ActivityExecutionStatus.Executing:
                    if (completedChildActivity is EventDrivenActivity)
                    {
                        HandleEventDrivenCompleted(context);
                        return;
                    }

                    StateInitializationActivity stateInitialization = completedChildActivity as StateInitializationActivity;
                    if (stateInitialization != null)
                    {
                        HandleStateInitializationCompleted(context, stateInitialization);
                        return;
                    }

                    if (completedChildActivity is StateFinalizationActivity)
                    {
                        HandleStateFinalizationCompleted(context);
                        return;
                    }

                    if (completedChildActivity is StateActivity)
                    {
                        HandleSubStateCompleted(context);
                        return;
                    }

                    InvalidChildActivity(state);
                    break;

                default:
                    throw new InvalidOperationException(SR.GetInvalidActivityStatus(context.Activity));
            }
        }

        private static void InvalidChildActivity(StateActivity state)
        {
            if (StateMachineHelpers.IsLeafState(state))
                throw new InvalidOperationException(SR.GetError_InvalidLeafStateChild());
            else
                throw new InvalidOperationException(SR.GetError_InvalidCompositeStateChild());
        }

        internal static void ExecuteEventDriven(ActivityExecutionContext context, EventDrivenActivity eventDriven)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            Debug.Assert(!executionState.HasEnqueuedActions);
            ExecuteChild(context, eventDriven);
        }

        private static void HandleEventDrivenCompleted(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            StateActivity state = (StateActivity)context.Activity;

            StateMachineExecutionState executionState = GetExecutionState(context);
            if (String.IsNullOrEmpty(executionState.NextStateName))
            {
                executionState.SubscriptionManager.ReevaluateSubscriptions(context);
                executionState.LockQueue();
            }
            else
                executionState.ProcessTransitionRequest(context);
            executionState.ProcessActions(context);
        }

        private static void ExecuteStateInitialization(ActivityExecutionContext context, StateInitializationActivity stateInitialization)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            Debug.Assert(!executionState.HasEnqueuedActions);
            ExecuteChild(context, stateInitialization);
        }

        private static void HandleStateInitializationCompleted(ActivityExecutionContext context, StateInitializationActivity stateInitialization)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (stateInitialization == null)
                throw new ArgumentNullException("stateInitialization");

            StateActivity state = (StateActivity)context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(state);

            if (!String.IsNullOrEmpty(executionState.NextStateName) && executionState.NextStateName.Equals(state.QualifiedName))
                throw new InvalidOperationException(SR.GetInvalidSetStateInStateInitialization());

            EnteringLeafState(context);
        }

        private static void ExecuteStateFinalization(ActivityExecutionContext context, StateFinalizationActivity stateFinalization)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            ExecuteChild(context, stateFinalization);
        }

        private static void HandleStateFinalizationCompleted(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            StateMachineExecutionState executionState = GetExecutionState(context);
            Complete(context);
        }

        internal static void ExecuteState(ActivityExecutionContext context, StateActivity state)
        {
            StateMachineExecutionState executionState = GetExecutionState(context);
            ExecuteChild(context, state);
        }

        private static void HandleSubStateCompleted(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            StateMachineExecutionState executionState = GetExecutionState(context);
            if (executionState.Completed)
            {
                // We're closing the state machine
                LeavingState(context);
            }
            else
            {
                executionState.ProcessActions(context);
            }
        }

        #endregion

        #region Helper methods

        private static StateInitializationActivity GetStateInitialization(ActivityExecutionContext context)
        {
            StateActivity state = (StateActivity)context.Activity;
            Debug.Assert(StateMachineHelpers.IsLeafState(state),
                "GetStateInitialization: StateInitialization is only allowed in a leaf node state");
            return GetHandlerActivity<StateInitializationActivity>(context);
        }

        private static StateFinalizationActivity GetStateFinalization(ActivityExecutionContext context)
        {
            StateActivity state = (StateActivity)context.Activity;
            Debug.Assert(StateMachineHelpers.IsLeafState(state),
                "GetStateFinalization: StateFinalization is only allowed in a leaf node state");
            return GetHandlerActivity<StateFinalizationActivity>(context);
        }

        private static T GetHandlerActivity<T>(ActivityExecutionContext context) where T : class
        {
            StateActivity state = (StateActivity)context.Activity;
            foreach (Activity activity in state.EnabledActivities)
            {
                T handler = activity as T;
                if (handler != null)
                {
                    return handler;
                }
            }
            return null;
        }

        private static StateMachineExecutionState GetExecutionState(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            StateActivity state = (StateActivity)context.Activity;
            StateMachineExecutionState executionState = GetExecutionState(state);
            return executionState;
        }


        private static StateMachineExecutionState GetExecutionState(StateActivity state)
        {
            if (state == null)
                throw new ArgumentNullException("state");

            StateActivity rootState = StateMachineHelpers.GetRootState(state);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            return executionState;
        }

        #endregion

        #endregion Methods

        #region Dynamic Update Functions

        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            if (!addedActivity.Enabled)
                return;

            if (executionContext.Activity.ExecutionStatus != ActivityExecutionStatus.Executing)
                return; // activity is not executing

            EventDrivenActivity eventDriven = addedActivity as EventDrivenActivity;
            if (eventDriven == null)
                return;

            // Activity we added is an EventDrivenActivity

            // First we disable the queue
            StateMachineSubscriptionManager.ChangeEventDrivenQueueState(executionContext, eventDriven, false);
            StateActivity rootState = StateMachineHelpers.GetRootState(executionContext.Activity as StateActivity);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            StateActivity currentState = StateMachineHelpers.GetCurrentState(executionContext);
            if (currentState == null)
                return; // Dynamic update happened before we entered the initial state

            StateMachineSubscriptionManager subscriptionManager = executionState.SubscriptionManager;
            subscriptionManager.ReevaluateSubscriptions(executionContext);
            executionState.LockQueue();
            executionState.ProcessActions(executionContext);
        }

        #endregion
    }
}
