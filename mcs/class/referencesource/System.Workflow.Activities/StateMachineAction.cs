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
    internal class StateMachineAction
    {
        private string _stateName;
        [NonSerialized]
        private StateActivity _state;
        [NonSerialized]
        private StateActivity _currentState;
        private StateMachineExecutionState _executionState;
        private StateMachineSubscriptionManager _subscriptionManager;

        internal string StateName
        {
            get
            {
                return _stateName;
            }
        }

        protected StateActivity State
        {
            get
            {
                return _state;
            }
        }

        protected StateActivity CurrentState
        {
            get
            {
                return _currentState;
            }
        }

        protected StateMachineExecutionState ExecutionState
        {
            get
            {
                return _executionState;
            }
        }

        protected StateMachineSubscriptionManager SubscriptionManager
        {
            get
            {
                return _subscriptionManager;
            }
        }

        internal StateMachineAction(string stateName)
        {
            _stateName = stateName;
        }

        internal virtual void Execute(ActivityExecutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            Debug.Assert(context.Activity.QualifiedName.Equals(this.StateName));
            _state = (StateActivity)context.Activity;
            _currentState = StateMachineHelpers.GetCurrentState(context);
            StateActivity rootState = StateMachineHelpers.GetRootState(_state);
            _executionState = StateMachineExecutionState.Get(rootState);
            _subscriptionManager = _executionState.SubscriptionManager;
        }
    }

    [Serializable]
    internal class CloseStateAction : StateMachineAction
    {

        internal CloseStateAction(string stateName)
            : base(stateName)
        {
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            StateActivity.LeavingState(context);
        }
    }

    [Serializable]
    internal class ExecuteChildStateAction : StateMachineAction
    {
        private string _childStateName;

        internal ExecuteChildStateAction(string stateName, string childStateName)
            : base(stateName)
        {
            _childStateName = childStateName;
        }

        internal string ChildStateName
        {
            get
            {
                return _childStateName;
            }
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            StateActivity childState = (StateActivity)this.State.Activities[this.ChildStateName];
            Debug.Assert(childState.Enabled);
            StateActivity.ExecuteState(context, childState);
        }
    }

    [Serializable]
    internal class SubscribeAction : StateMachineAction
    {
        private string _eventDrivenName;

        internal SubscribeAction(string stateName, string eventDrivenName)
            : base(stateName)
        {
            _eventDrivenName = eventDrivenName;
        }

        internal string EventDrivenName
        {
            get
            {
                return _eventDrivenName;
            }
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            EventDrivenActivity eventDriven = (EventDrivenActivity)this.State.Activities[this.EventDrivenName];
            Debug.Assert(eventDriven.Enabled);
            this.SubscriptionManager.SubscribeEventDriven(context, eventDriven);
        }
    }

    [Serializable]
    internal class UnsubscribeAction : StateMachineAction
    {
        private string _eventDrivenName;

        internal UnsubscribeAction(string stateName, string eventDrivenName)
            : base(stateName)
        {
            _eventDrivenName = eventDrivenName;
        }

        internal string EventDrivenName
        {
            get
            {
                return _eventDrivenName;
            }
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            EventDrivenActivity eventDriven = (EventDrivenActivity)this.State.Activities[this.EventDrivenName];
            Debug.Assert(eventDriven.Enabled);
            this.SubscriptionManager.UnsubscribeEventDriven(context, eventDriven);
        }
    }

    [Serializable]
    internal class ExternalEventAction : StateMachineAction
    {
        private string _eventDrivenName;

        internal ExternalEventAction(string stateName, string eventDrivenName)
            : base(stateName)
        {
            _eventDrivenName = eventDrivenName;
        }

        internal string EventDrivenName
        {
            get
            {
                return _eventDrivenName;
            }
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            EventDrivenActivity eventDriven = (EventDrivenActivity)this.State.Activities[this.EventDrivenName];
            Debug.Assert(eventDriven.Enabled);
            StateActivity.ExecuteEventDriven(context, eventDriven);
        }
    }

    [Serializable]
    internal class SetStateAction : StateMachineAction
    {
        private string _targetStateName;
        internal SetStateAction(string stateName, string targetStateName)
            : base(stateName)
        {
            _targetStateName = targetStateName;
        }

        internal string TargetStateName
        {
            get
            {
                return _targetStateName;
            }
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            this.ExecutionState.CalculateStateTransition(this.CurrentState, this.TargetStateName);
        }
    }

    [Serializable]
    internal class DisableQueuesAction : StateMachineAction
    {
        internal DisableQueuesAction(string stateName)
            : base(stateName)
        {
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);

            StateActivity state = this.State;
            StateActivity rootState = StateMachineHelpers.GetRootState(state);
            Queue<StateActivity> states = new Queue<StateActivity>();
            states.Enqueue(rootState);
            while (states.Count > 0)
            {
                state = states.Dequeue();
                foreach (Activity activity in state.EnabledActivities)
                {
                    EventDrivenActivity eventDriven = activity as EventDrivenActivity;
                    if (eventDriven != null)
                    {
                        IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
                        IComparable queueName = eventActivity.QueueName;
                        if (queueName != null)
                        {
                            WorkflowQueue queue = StateMachineSubscriptionManager.GetWorkflowQueue(context, queueName);
                            if (queue != null)
                                queue.Enabled = this.SubscriptionManager.Subscriptions.ContainsKey(queueName);
                        }
                    }
                    else
                    {
                        StateActivity childState = activity as StateActivity;
                        if (childState != null)
                            states.Enqueue(childState);
                    }
                }
            }
        }
    }


    [Serializable]
    internal class EnteringStateAction : StateMachineAction
    {
        internal EnteringStateAction(string stateName)
            : base(stateName)
        {
        }

        internal override void Execute(ActivityExecutionContext context)
        {
            base.Execute(context);
            context.TrackData(StateActivity.StateChangeTrackingDataKey, this.CurrentState.QualifiedName);
        }
    }
}
