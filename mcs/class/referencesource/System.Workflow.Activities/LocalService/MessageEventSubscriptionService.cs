//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Activities
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowSubscriptionService
    {
        public abstract void CreateSubscription(MessageEventSubscription subscription);
        public abstract void DeleteSubscription(Guid subscriptionId);
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class MessageEventSubscription
    {
        Type interfaceType;
        String operation;
        List<CorrelationProperty> predicates;
        Guid subscriptionId;
        Guid workflowInstanceId;
        IComparable queueName;

        protected MessageEventSubscription()
        {

        }

        public MessageEventSubscription(IComparable queueName, Guid instanceId)
            : this(queueName, instanceId, Guid.NewGuid())
        {

        }

        public MessageEventSubscription(IComparable queueName, Guid instanceId, Guid subscriptionId)
            : this(queueName, instanceId, null, null, subscriptionId)
        {

        }

        public MessageEventSubscription(IComparable queueName, Guid subscriptionId, Type interfaceType, String operation)
            : this(queueName, Guid.Empty, interfaceType, operation, subscriptionId)
        {

        }

        public MessageEventSubscription(IComparable queueName, Guid instanceId, Type interfaceType, String operation, Guid subscriptionId)
        {
            this.queueName = queueName;
            this.workflowInstanceId = instanceId;
            this.subscriptionId = subscriptionId;
            this.interfaceType = interfaceType;
            this.operation = operation;
            this.predicates = new List<CorrelationProperty>();
        }

        public virtual Type InterfaceType
        {
            get
            {
                return this.interfaceType;
            }
            set
            {
                this.interfaceType = value;
            }
        }

        public virtual String MethodName
        {
            get
            {
                return this.operation;
            }
            set
            {
                this.operation = value;
            }
        }

        public virtual ICollection<CorrelationProperty> CorrelationProperties
        {
            get
            {
                return this.predicates;
            }
        }

        //A Unique id for this subscription. It is needed because
        //QueueName is not always guaranteed to be Unique.
        //Needed in case of Multiple Subscription on Same Queue
        public virtual Guid SubscriptionId
        {
            get
            {
                return this.subscriptionId;
            }
        }

        public virtual IComparable QueueName
        {
            get
            {
                return this.queueName;
            }
            protected set
            {
                this.queueName = value;
            }
        }

        public virtual Guid WorkflowInstanceId
        {
            get
            {
                return this.workflowInstanceId;
            }
        }

    }

}
