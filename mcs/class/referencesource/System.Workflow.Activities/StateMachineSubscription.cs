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
    #region StateMachineSubscription

    [Serializable]
    internal abstract class StateMachineSubscription : IActivityEventListener<QueueEventArgs>
    {
        #region Member Variables

        private Guid _subscriptionId;

        #endregion Member Variables

        #region Properties

        internal Guid SubscriptionId
        {
            get
            {
                return _subscriptionId;
            }
            set
            {
                _subscriptionId = value;
            }
        }

        #endregion Properties

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            Enqueue(context);
        }

        protected abstract void Enqueue(ActivityExecutionContext context);
        internal abstract void ProcessEvent(ActivityExecutionContext context);
    }

    #endregion StateMachineSubscription

    #region EventActivitySubscription

    [Serializable]
    internal class EventActivitySubscription : StateMachineSubscription
    {
        #region Member Variables

        private string _eventActivityName = String.Empty;
        private string _eventDrivenName = String.Empty;
        private string _stateName = String.Empty;
        private IComparable _queueName;

        #endregion Member Variables

        #region Properties

        internal string EventActivityName
        {
            get
            {
                return _eventActivityName;
            }
        }

        internal string StateName
        {
            get
            {
                return _stateName;
            }
        }

        internal IComparable QueueName
        {
            get
            {
                return _queueName;
            }
        }

        internal string EventDrivenName
        {
            get
            {
                return _eventDrivenName;
            }
        }

        #endregion Properties

        internal void Subscribe(ActivityExecutionContext context,
            StateActivity state,
            IEventActivity eventActivity)
        {
            eventActivity.Subscribe(context, this);
            Activity activity = (Activity)eventActivity;
            this._queueName = eventActivity.QueueName;
            this._eventActivityName = activity.QualifiedName;
            this._stateName = state.QualifiedName;
            this.SubscriptionId = Guid.NewGuid();
            EventDrivenActivity eventDriven = StateMachineHelpers.GetParentEventDriven(eventActivity);
            this._eventDrivenName = eventDriven.QualifiedName;
        }

        internal void Unsubscribe(ActivityExecutionContext context,
            IEventActivity eventActivity)
        {
            eventActivity.Unsubscribe(context, this);
        }

        protected override void Enqueue(ActivityExecutionContext context)
        {
            StateActivity rootState = StateMachineHelpers.GetRootState((StateActivity)context.Activity);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            executionState.SubscriptionManager.Enqueue(context, this.QueueName);
        }

        internal override void ProcessEvent(ActivityExecutionContext context)
        {
            StateActivity rootState = StateMachineHelpers.GetRootState((StateActivity)context.Activity);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            ExternalEventAction action = new ExternalEventAction(this.StateName, this.EventDrivenName);
            Debug.Assert(!executionState.HasEnqueuedActions);
            executionState.EnqueueAction(action);
            executionState.ProcessActions(context);
        }
    }

    #endregion EventActivitySubscription

    #region SetStateSubscription

    [Serializable]
    internal class SetStateSubscription : StateMachineSubscription
    {
        private Guid _instanceId;

        internal SetStateSubscription(Guid instanceId)
        {
            this._instanceId = instanceId;
        }

        internal void CreateQueue(ActivityExecutionContext context)
        {
            if (!StateMachineHelpers.IsRootExecutionContext(context))
            {
                // we only subscribe to the set state event if
                // we're at the root level. If this instance is
                // being called, it is not possible to set the
                // state from the host side directly
                return;
            }

            WorkflowQueuingService workflowQueuingService = context.GetService<WorkflowQueuingService>();

            MessageEventSubscription subscription = new MessageEventSubscription(
                StateMachineWorkflowActivity.SetStateQueueName,
                this._instanceId);

            WorkflowQueue workflowQueue = workflowQueuingService.CreateWorkflowQueue(
                StateMachineWorkflowActivity.SetStateQueueName,
                true);
            this.SubscriptionId = subscription.SubscriptionId;
        }

        internal void DeleteQueue(ActivityExecutionContext context)
        {
            if (!StateMachineHelpers.IsRootExecutionContext(context))
            {
                // we only subscribe to the set state event if
                // we're at the root level. If this instance is
                // being called, it is not possible to set the
                // state from the host side directly
                return;
            }

            WorkflowQueuingService workflowQueuingService = context.GetService<WorkflowQueuingService>();
            WorkflowQueue workflowQueue = workflowQueuingService.GetWorkflowQueue(StateMachineWorkflowActivity.SetStateQueueName);
            workflowQueuingService.DeleteWorkflowQueue(StateMachineWorkflowActivity.SetStateQueueName);
        }

        internal void Subscribe(ActivityExecutionContext context)
        {
            WorkflowQueuingService workflowQueuingService = context.GetService<WorkflowQueuingService>();
            WorkflowQueue workflowQueue = workflowQueuingService.GetWorkflowQueue(StateMachineWorkflowActivity.SetStateQueueName);
            workflowQueue.RegisterForQueueItemAvailable(this);
        }

        internal void Unsubscribe(ActivityExecutionContext context)
        {
            WorkflowQueuingService workflowQueuingService = context.GetService<WorkflowQueuingService>();
            WorkflowQueue workflowQueue = workflowQueuingService.GetWorkflowQueue(StateMachineWorkflowActivity.SetStateQueueName);
            workflowQueue.UnregisterForQueueItemAvailable(this);
        }

        protected override void Enqueue(ActivityExecutionContext context)
        {
            StateActivity rootState = StateMachineHelpers.GetRootState((StateActivity)context.Activity);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            executionState.SubscriptionManager.Enqueue(context, this.SubscriptionId);
        }

        internal override void ProcessEvent(ActivityExecutionContext context)
        {
            WorkflowQueuingService workflowQueuingService = context.GetService<WorkflowQueuingService>();
            WorkflowQueue workflowQueue = workflowQueuingService.GetWorkflowQueue(StateMachineWorkflowActivity.SetStateQueueName);
            SetStateEventArgs eventArgs = workflowQueue.Dequeue() as SetStateEventArgs;
            StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
            if (currentState == null)
                throw new InvalidOperationException(SR.GetStateMachineWorkflowMustHaveACurrentState());

            StateActivity rootState = StateMachineHelpers.GetRootState((StateActivity)context.Activity);
            StateMachineExecutionState executionState = StateMachineExecutionState.Get(rootState);
            SetStateAction action = new SetStateAction(currentState.QualifiedName, eventArgs.TargetStateName);
            Debug.Assert(!executionState.HasEnqueuedActions);
            executionState.EnqueueAction(action);
            executionState.ProcessActions(context);
        }
    }

    #endregion SetStateSubscription
}
