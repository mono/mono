using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Activities
{
    [Serializable]
    internal sealed class CorrelationTokenInvalidatedHandler : IActivityEventListener<CorrelationTokenEventArgs>
    {
        IActivityEventListener<QueueEventArgs> eventHandler;
        EventQueueName queueName;
        Guid subscriptionId;
        Guid instanceId;

        bool queueCreator;
        Type interfaceType;
        string followerOperation;

        internal CorrelationTokenInvalidatedHandler(Type interfaceType, string operation, IActivityEventListener<QueueEventArgs> eventHandler, Guid instanceId)
        {
            this.eventHandler = eventHandler;
            this.interfaceType = interfaceType;
            this.followerOperation = operation;
            this.instanceId = instanceId;
        }

        #region IActivityEventListener<CorrelationTokenEventArgs> Members
        void IActivityEventListener<CorrelationTokenEventArgs>.OnEvent(object sender, CorrelationTokenEventArgs dataChangeEventArgs)
        {
            if (sender == null)
                throw new ArgumentException("sender");
            if (dataChangeEventArgs == null)
                throw new ArgumentException("dataChangeEventArgs");

            ActivityExecutionContext context = sender as ActivityExecutionContext;
            Activity activity = context.Activity;

            ICollection<CorrelationProperty> correlationValues = dataChangeEventArgs.CorrelationToken.Properties;
            if (dataChangeEventArgs.IsInitializing)
            {
                CreateSubscription(this.instanceId, context, correlationValues);
                return;
            }

            if (queueName != null)
            {
                if (!CorrelationResolver.IsInitializingMember(queueName.InterfaceType, queueName.MethodName,
                    correlationValues == null ? null : new object[] { correlationValues }))
                {
                    DeleteSubscription(context);
                }
            }

            dataChangeEventArgs.CorrelationToken.UnsubscribeFromCorrelationTokenInitializedEvent(activity, this);
        }
        #endregion

        private void CreateSubscription(Guid instanceId, ActivityExecutionContext context, ICollection<CorrelationProperty> correlationValues)
        {
            WorkflowQueuingService queueSvcs = context.GetService<WorkflowQueuingService>();
            EventQueueName queueId = new EventQueueName(this.interfaceType, this.followerOperation, correlationValues);

            WorkflowQueue workflowQueue = null;
            if (!queueSvcs.Exists(queueId))
            {
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "CorrelationTokenInvalidatedHandler: creating q {0} ", queueId.GetHashCode());
                workflowQueue = queueSvcs.CreateWorkflowQueue(queueId, true);
                queueCreator = true;
            }
            else
            {
                workflowQueue = queueSvcs.GetWorkflowQueue(queueId);
            }

            if (this.eventHandler != null)
            {
                workflowQueue.RegisterForQueueItemAvailable(this.eventHandler);
            }

            WorkflowSubscriptionService subscriptionService = (WorkflowSubscriptionService)context.GetService(typeof(WorkflowSubscriptionService));

            MessageEventSubscription subscription = new MessageEventSubscription(queueId, instanceId);
            this.queueName = queueId;
            this.subscriptionId = subscription.SubscriptionId;
            subscription.InterfaceType = this.interfaceType;
            subscription.MethodName = this.followerOperation;

            this.interfaceType = null;
            this.followerOperation = null;

            if (correlationValues != null)
            {
                foreach (CorrelationProperty property in correlationValues)
                {
                    subscription.CorrelationProperties.Add(property);
                }
            }

            if (this.eventHandler != null)
                return;

            if (subscriptionService == null)
                return;
            subscriptionService.CreateSubscription(subscription);
        }

        private void DeleteSubscription(ActivityExecutionContext context)
        {
            if (this.queueName == null)
                return;

            WorkflowQueuingService queueSvcs = context.GetService<WorkflowQueuingService>();
            if (queueCreator)
                queueSvcs.DeleteWorkflowQueue(this.queueName);

            if (this.eventHandler != null)
                return;

            WorkflowSubscriptionService subscriptionService = context.GetService<WorkflowSubscriptionService>();
            if (subscriptionService != null)
                subscriptionService.DeleteSubscription(this.subscriptionId);

            WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "CorrelationTokenInvalidatedHandler subscription deleted SubId {0} QueueId {1}", this.subscriptionId, this.queueName);
        }

    }
}
