#region Using directives

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using System.Runtime.Remoting.Messaging;

#endregion

namespace System.Workflow.Activities
{
    [Serializable]
    internal sealed class FollowerQueueCreator : IActivityEventListener<QueueEventArgs>
    {
        string followerOperation;
        object sync = new object();

        internal FollowerQueueCreator(string operation)
        {
            this.followerOperation = operation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            FollowerQueueCreator equalsObject = obj as FollowerQueueCreator;
            if (this.followerOperation == equalsObject.followerOperation)
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return this.followerOperation.GetHashCode();
        }

        #region IActivityEventListener<QueueEventArgs> Members

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs args)
        {
            lock (sync)
            {
                WorkflowQueue queue = (WorkflowQueue)sender;

                // create the queue after extracting the correlation values from the message
                EventQueueName staticId = (EventQueueName)queue.QueueName;
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "FollowerQueueCreator: initialized on operation {0} for follower {1}", staticId.InterfaceType.Name + staticId.MethodName, this.followerOperation);

                IMethodMessage message = queue.Peek() as IMethodMessage;

                ICollection<CorrelationProperty> corrValues = CorrelationResolver.ResolveCorrelationValues(staticId.InterfaceType, staticId.MethodName, message.Args, false);

                EventQueueName queueName = new EventQueueName(staticId.InterfaceType, this.followerOperation, corrValues);
                if (!queue.QueuingService.Exists(queueName))
                {
                    WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Information, 0, "FollowerQueueCreator::CreateQueue creating q {0}", queueName.GetHashCode());
                    queue.QueuingService.CreateWorkflowQueue(queueName, true);
                }
            }
        }

        #endregion
    }
}
