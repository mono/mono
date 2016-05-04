using System;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime
{
    /// <summary>
    /// TimerEventSubscription
    /// Class which represents a timer subscription which a running workflow
    /// instance creates on timer service for Timer Notification.
    /// </summary>
    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TimerEventSubscription
    {
        #region Data
        DateTime expiresAt;
        Guid subscriptionId;
        Guid workflowInstanceId;
        IComparable queueName;

        #endregion
        /// <summary>
        /// Specifies the absolute timeout value in UTC format, at which
        /// workflow expects a notification from SchedulerService.
        /// </summary>
        public virtual DateTime ExpiresAt
        {
            get
            {
                return this.expiresAt;
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


        //Used only when providing specialized implementation.
        protected TimerEventSubscription()
        {

        }

        /// <summary>
        /// Creates a TimerEventSubscription for workflow Instance identified by
        /// workflowInstanceID.
        /// </summary>
        /// <param name="workflowInstanceId">WorkflowInstanceId for which this subscription is created.</param>
        /// <param name="expiresAt"> Time at which timer event should fire.</param>
        public TimerEventSubscription(Guid workflowInstanceId, DateTime expiresAt)
            : this(Guid.NewGuid(), workflowInstanceId, expiresAt)
        {

        }

        /// <summary>
        /// Constructor to create TimerEventSubscription with user provided
        /// value for timerid which will be SubscriptionId & QueueName of EventSubscription.
        /// </summary>
        /// <param name="timerId">SubscriptionId for this subscription, this will be same value as QueueName.</param>
        /// <param name="workflowInstanceId">WorkflowInstanceId for which this subscription is created.</param>
        /// <param name="expiresAt"> Time at which timer event should fire.</param>
        public TimerEventSubscription(Guid timerId, Guid workflowInstanceId, DateTime expiresAt)
        {
            this.queueName = timerId;
            this.workflowInstanceId = workflowInstanceId;
            this.subscriptionId = timerId;
            this.expiresAt = expiresAt;
        }
    }
}
