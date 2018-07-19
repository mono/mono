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
    internal class StateMachineSubscriptionManager
    {
        private SetStateSubscription _setStateSubscription;
        private List<StateMachineSubscription> _eventQueue = new List<StateMachineSubscription>();
        private Dictionary<IComparable, StateMachineSubscription> _subscriptions = new Dictionary<IComparable, StateMachineSubscription>();
        private StateMachineExecutionState _executionState;

        internal StateMachineSubscriptionManager(StateMachineExecutionState executionState, Guid instanceId)
        {
            _executionState = executionState;
            _setStateSubscription = new SetStateSubscription(instanceId);
        }

        #region Properties
        private List<StateMachineSubscription> EventQueue
        {
            get
            {
                return this._eventQueue;
            }
        }

        internal StateMachineExecutionState ExecutionState
        {
            get
            {
                return _executionState;
            }
        }

        internal Dictionary<IComparable, StateMachineSubscription> Subscriptions
        {
            get
            {
                return this._subscriptions;
            }
        }

        internal SetStateSubscription SetStateSubscription
        {
            get
            {
                return _setStateSubscription;
            }
        }

        #endregion Properties

        internal void UnsubscribeState(ActivityExecutionContext context)
        {
            StateActivity state = (StateActivity)context.Activity;
            foreach (Activity childActivity in state.EnabledActivities)
            {
                EventDrivenActivity eventDriven = childActivity as EventDrivenActivity;
                if (eventDriven != null)
                {
                    if (IsEventDrivenSubscribed(eventDriven))
                        UnsubscribeEventDriven(context, eventDriven);
                }
            }
        }

        internal void ReevaluateSubscriptions(ActivityExecutionContext context)
        {
            Dictionary<IComparable, StateMachineSubscription> subscriptions = this.GetSubscriptionsShallowCopy();
            List<IComparable> subscribed = new List<IComparable>();

            StateActivity state = StateMachineHelpers.GetCurrentState(context);
            while (state != null)
            {

                foreach (Activity activity in state.EnabledActivities)
                {
                    EventDrivenActivity eventDriven = activity as EventDrivenActivity;
                    if (eventDriven == null)
                        continue;

                    IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
                    IComparable queueName = eventActivity.QueueName;
                    if (queueName == null)
                        continue;

                    StateMachineSubscription subscription;
                    subscriptions.TryGetValue(queueName, out subscription);
                    EventActivitySubscription eventActivitySubscription = subscription as EventActivitySubscription;
                    if (eventActivitySubscription != null)
                    {
                        if (eventActivitySubscription.EventDrivenName.Equals(eventDriven.QualifiedName))
                        {
                            // this EventDriven is already subscribed
                            subscribed.Add(queueName);
                            continue;
                        }
                        else
                        {
                            // Check if this state already subscribe to this event
                            // if so, throws, since it is not valid to subscribe to the
                            // same event twice
                            if (eventActivitySubscription.StateName.Equals(state.QualifiedName))
                                throw new InvalidOperationException(SR.GetStateAlreadySubscribesToThisEvent(state.QualifiedName, queueName));

                            // some other EventDriven is subscribed, so we need to unsubscribe if 
                            // the event driven belongs to one of our parents
                            if (IsParentState(state, eventActivitySubscription.StateName))
                            {
                                UnsubscribeAction unsubscribe = new UnsubscribeAction(eventActivitySubscription.StateName, eventActivitySubscription.EventDrivenName);
                                this.ExecutionState.EnqueueAction(unsubscribe);
                                subscriptions.Remove(queueName);
                            }
                        }
                    }

                    // Tests if a child state already subscribes to this event
                    // is so, skip, since the child takes precedence
                    if (subscribed.Contains(queueName))
                        continue;

                    SubscribeAction subscribe = new SubscribeAction(state.QualifiedName, eventDriven.QualifiedName);
                    this.ExecutionState.EnqueueAction(subscribe);
                    subscribed.Add(queueName);
                }

                state = state.Parent as StateActivity;
            }

            StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
            DisableQueuesAction disableQueues = new DisableQueuesAction(currentState.QualifiedName);
            this.ExecutionState.EnqueueAction(disableQueues);
        }

        private bool IsParentState(StateActivity state, string stateName)
        {
            StateActivity parentState = state.Parent as StateActivity;
            while (parentState != null)
            {
                if (parentState.QualifiedName.Equals(stateName))
                    return true;
                parentState = parentState.Parent as StateActivity;
            }
            return false;
        }

        internal void SubscribeEventDriven(ActivityExecutionContext context, EventDrivenActivity eventDriven)
        {
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            Activity activity = (Activity)eventActivity;
            IComparable queueName = GetQueueName(eventActivity);
            Debug.Assert(!this.Subscriptions.ContainsKey(queueName));
            SubscribeEventActivity(context, eventActivity);
        }

        internal void UnsubscribeEventDriven(ActivityExecutionContext context, EventDrivenActivity eventDriven)
        {
            Debug.Assert(IsEventDrivenSubscribed(eventDriven));
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            UnsubscribeEventActivity(context, eventActivity);
        }

        private StateMachineSubscription SubscribeEventActivity(ActivityExecutionContext context,
            IEventActivity eventActivity)
        {
            EventActivitySubscription subscription = new EventActivitySubscription();
            StateActivity state = (StateActivity)context.Activity;
            subscription.Subscribe(context, state, eventActivity);
            WorkflowQueue workflowQueue = GetWorkflowQueue(context, subscription.QueueName);
            if (workflowQueue != null)
                workflowQueue.Enabled = true;

            Debug.Assert(subscription.QueueName != null);
            this.Subscriptions[subscription.QueueName] = subscription;

            return subscription;
        }

        private void UnsubscribeEventActivity(ActivityExecutionContext context,
            IEventActivity eventActivity)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (eventActivity == null)
                throw new ArgumentNullException("eventActivity");

            EventActivitySubscription subscription = GetSubscription(eventActivity);
            WorkflowQueue workflowQueue = GetWorkflowQueue(context, subscription.QueueName);
            if (workflowQueue != null)
                workflowQueue.Enabled = false;
            UnsubscribeEventActivity(context, eventActivity, subscription);
        }

        private void UnsubscribeEventActivity(ActivityExecutionContext context,
            IEventActivity eventActivity,
            EventActivitySubscription subscription)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (eventActivity == null)
                throw new ArgumentNullException("eventActivity");
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            subscription.Unsubscribe(context, eventActivity);
            RemoveFromQueue(subscription.SubscriptionId);

            Debug.Assert(subscription.QueueName != null);
            this.Subscriptions.Remove(subscription.QueueName);
        }

        internal void CreateSetStateEventQueue(ActivityExecutionContext context)
        {
            this.SetStateSubscription.CreateQueue(context);
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = this.SetStateSubscription;
        }

        internal void DeleteSetStateEventQueue(ActivityExecutionContext context)
        {
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = null;
            this.SetStateSubscription.DeleteQueue(context);
        }

        internal void SubscribeToSetStateEvent(ActivityExecutionContext context)
        {
            this.SetStateSubscription.Subscribe(context);
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = this.SetStateSubscription;
        }

        internal void UnsubscribeToSetStateEvent(ActivityExecutionContext context)
        {
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = null;
            this.SetStateSubscription.Unsubscribe(context);
        }

        private bool IsEventDrivenSubscribed(EventDrivenActivity eventDriven)
        {
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            EventActivitySubscription subscription = GetSubscription(eventActivity);
            return (subscription != null);
        }

        private EventActivitySubscription GetSubscription(IEventActivity eventActivity)
        {
            IComparable queueName = GetQueueName(eventActivity);
            if ((queueName == null) || (!this.Subscriptions.ContainsKey(queueName)))
                return null;

            EventActivitySubscription subscription = this.Subscriptions[queueName] as EventActivitySubscription;

            Activity activity = (Activity)eventActivity;
            if (subscription == null ||
                subscription.EventActivityName != activity.QualifiedName)
                return null;

            return subscription;
        }

        private StateMachineSubscription GetSubscription(IComparable queueName)
        {
            StateMachineSubscription subscription;
            this.Subscriptions.TryGetValue(queueName, out subscription);
            return subscription;
        }

        /* Currently not used, left here for completeness
        internal static void EnableStateWorkflowQueues(ActivityExecutionContext context, StateActivity state)
        {
            ChangeStateWorkflowQueuesState(context, state, true);
        }
        */

        internal static void DisableStateWorkflowQueues(ActivityExecutionContext context, StateActivity state)
        {
            ChangeStateWorkflowQueuesState(context, state, false);
        }

        private static void ChangeStateWorkflowQueuesState(ActivityExecutionContext context, StateActivity state, bool enabled)
        {
            foreach (Activity activity in state.EnabledActivities)
            {
                EventDrivenActivity eventDriven = activity as EventDrivenActivity;
                if (eventDriven != null)
                    ChangeEventDrivenQueueState(context, eventDriven, enabled);
            }
        }

        internal static void ChangeEventDrivenQueueState(ActivityExecutionContext context, EventDrivenActivity eventDriven, bool enabled)
        {
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            IComparable queueName = GetQueueName(eventActivity);
            if (queueName == null)
                return; // skip unitialized follower
            WorkflowQueue workflowQueue = GetWorkflowQueue(context, queueName);
            if (workflowQueue != null)
                workflowQueue.Enabled = enabled;
        }

        internal static WorkflowQueue GetWorkflowQueue(ActivityExecutionContext context, IComparable queueName)
        {
            WorkflowQueuingService workflowQueuingService = context.GetService<WorkflowQueuingService>();
            if (workflowQueuingService.Exists(queueName))
            {
                WorkflowQueue workflowQueue = workflowQueuingService.GetWorkflowQueue(queueName);
                return workflowQueue;
            }
            return null;
        }

        private static IComparable GetQueueName(IEventActivity eventActivity)
        {
            IComparable queueName = eventActivity.QueueName;
            return queueName;
        }

        private Dictionary<IComparable, StateMachineSubscription> GetSubscriptionsShallowCopy()
        {
            Dictionary<IComparable, StateMachineSubscription> subscriptions = new Dictionary<IComparable, StateMachineSubscription>();
            foreach (KeyValuePair<IComparable, StateMachineSubscription> dictionaryEntry in this.Subscriptions)
            {
                subscriptions.Add(dictionaryEntry.Key, dictionaryEntry.Value);
            }
            return subscriptions;
        }

        #region Event Queue Methods

        internal void Enqueue(ActivityExecutionContext context, Guid subscriptionId)
        {
            StateMachineSubscription subscription = GetSubscription(subscriptionId);
            if (subscription != null)
            {
                // subscription can be null if we already unsubscribed to 
                // this event
                this.EventQueue.Add(subscription);
            }
            ProcessQueue(context);
        }

        internal void Enqueue(ActivityExecutionContext context, IComparable queueName)
        {
            StateMachineSubscription subscription = GetSubscription(queueName);
            if (subscription != null)
            {
                // subscription can be null if we already unsubscribed to 
                // this event
                this.EventQueue.Add(subscription);
            }
            ProcessQueue(context);
        }

        internal StateMachineSubscription Dequeue()
        {
            StateMachineSubscription subscription = this.EventQueue[0];
            this.EventQueue.RemoveAt(0);
            return subscription;
        }

        private void RemoveFromQueue(Guid subscriptionId)
        {
            this.EventQueue.RemoveAll(delegate(StateMachineSubscription subscription) { return subscription.SubscriptionId.Equals(subscriptionId); });
        }

        internal void ProcessQueue(ActivityExecutionContext context)
        {
            StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
            if (this.EventQueue.Count == 0 ||
                this.ExecutionState.HasEnqueuedActions ||
                this.ExecutionState.SchedulerBusy ||
                currentState == null)
                return;

            StateMachineSubscription subscription = Dequeue();
            subscription.ProcessEvent(context);
        }

        #endregion Event Queue Methods

    }
}
