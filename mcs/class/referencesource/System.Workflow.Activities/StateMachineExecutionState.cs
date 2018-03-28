#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;

#endregion Using directives

namespace System.Workflow.Activities
{
    [Serializable]
    internal class StateMachineExecutionState
    {
        #region Member Variables

        internal const string StateMachineExecutionStateKey = "StateMachineExecutionState";
        private StateMachineSubscriptionManager _subscriptionManager;
        private Queue<StateMachineAction> _actions;
        private string _currentStateName;
        private string _previousStateName;
        private string _nextStateName;
        private bool _completed = false;
        private bool _queueLocked = false;
        private bool _schedulerBusy = false;

        #endregion Member Variables

        #region Properties

        internal StateMachineSubscriptionManager SubscriptionManager
        {
            get
            {
                return _subscriptionManager;
            }
        }

        private Queue<StateMachineAction> Actions
        {
            get
            {
                if (_actions == null)
                    _actions = new Queue<StateMachineAction>();
                return _actions;
            }
        }

        internal bool SchedulerBusy
        {
            get
            {
                return _schedulerBusy;
            }
            set
            {
                _schedulerBusy = value;
            }
        }

        internal string CurrentStateName
        {
            get
            {
                return _currentStateName;
            }
            set
            {
                _currentStateName = value;
            }
        }

        internal string PreviousStateName
        {
            get
            {
                return _previousStateName;
            }
            set
            {
                _previousStateName = value;
            }
        }

        internal string NextStateName
        {
            get
            {
                return _nextStateName;
            }
            set
            {
                _nextStateName = value;
            }
        }

        internal bool Completed
        {
            get
            {
                return _completed;
            }
            set
            {
                _completed = value;
            }
        }

        internal bool HasEnqueuedActions
        {
            get
            {
                return this.Actions.Count > 0;
            }
        }

        #endregion Properties

        #region Constructors

        internal StateMachineExecutionState(Guid instanceId)
        {
            _subscriptionManager = new StateMachineSubscriptionManager(this, instanceId);
        }

        #endregion

        internal void LockQueue()
        {
            _queueLocked = true;
        }

        internal void EnqueueAction(StateMachineAction action)
        {
            Debug.Assert(!this._queueLocked);
            this.Actions.Enqueue(action);
        }

        internal StateMachineAction DequeueAction()
        {
            StateMachineAction action = this.Actions.Dequeue();
            if (this.Actions.Count == 0)
                _queueLocked = false;
            return action;
        }

        internal void ProcessActions(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (this.SchedulerBusy)
                return;

            StateActivity state = (StateActivity)context.Activity;

            if (this.Actions.Count == 0)
            {
                this.SubscriptionManager.ProcessQueue(context);
                return;
            }

            StateMachineAction action = this.Actions.Peek();
            while (action.StateName.Equals(state.QualifiedName))
            {
                action = DequeueAction();
                action.Execute(context);

                // If the previous action just
                // requested something to the runtime
                // scheduler, then we quit, since
                // the scheduler takes precedence.
                // we'll pick up the processing of actions
                // after the scheduler return the control to us.
                if (this.SchedulerBusy)
                    return;

                if (this.Actions.Count == 0)
                    break;

                action = this.Actions.Peek();
            }

            if (this.Actions.Count > 0)
            {
                StateActivity rootState = StateMachineHelpers.GetRootState(state);
                StateActivity nextActionState = StateMachineHelpers.FindDynamicStateByName(rootState, action.StateName);
                if (nextActionState == null)
                    throw new InvalidOperationException(SR.GetInvalidStateMachineAction(action.StateName));

                nextActionState.RaiseProcessActionEvent(context);
            }
            else
            {
                this.SubscriptionManager.ProcessQueue(context);
            }
        }


        internal void ProcessTransitionRequest(ActivityExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.NextStateName))
                return;

            StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
            CalculateStateTransition(currentState, this.NextStateName);
            LockQueue();
            this.NextStateName = null;
        }

        internal void CalculateStateTransition(StateActivity currentState, string targetStateName)
        {
            if (currentState == null)
                throw new ArgumentNullException("currentState");
            if (String.IsNullOrEmpty(targetStateName))
                throw new ArgumentNullException("targetStateName");

            while (currentState != null && (currentState.QualifiedName.Equals(targetStateName) || !StateMachineHelpers.ContainsState(currentState, targetStateName)))
            {
                CloseStateAction action = new CloseStateAction(currentState.QualifiedName);
                this.Actions.Enqueue(action);
                currentState = currentState.Parent as StateActivity;
            }
            if (currentState == null)
                throw new InvalidOperationException(SR.GetUnableToTransitionToState(targetStateName));

            while (!currentState.QualifiedName.Equals(targetStateName))
            {
                foreach (Activity childActivity in currentState.EnabledActivities)
                {
                    StateActivity childState = childActivity as StateActivity;
                    if (childState != null)
                    {
                        // 
                        if (StateMachineHelpers.ContainsState(childState, targetStateName))
                        {
                            ExecuteChildStateAction action = new ExecuteChildStateAction(currentState.QualifiedName, childState.QualifiedName);
                            this.Actions.Enqueue(action);
                            currentState = childState;
                            break;
                        }
                    }
                }
            }
            if (!StateMachineHelpers.IsLeafState(currentState))
                throw new InvalidOperationException(SR.GetInvalidStateTransitionPath());
        }

        #region Static Methods

        internal static StateMachineExecutionState Get(StateActivity state)
        {
            Debug.Assert(StateMachineHelpers.IsRootState(state));
            StateMachineExecutionState executionState = (StateMachineExecutionState)state.GetValue(StateActivity.StateMachineExecutionStateProperty);
            Debug.Assert(executionState != null);
            return executionState;
        }

        #endregion
    }
}
