namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;
    using System.Workflow.ComponentModel.Compiler;
    using System.Diagnostics;
    using System.Globalization;
    using System.Workflow.Activities.Common;

    [SRDescription(SR.DelayActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(DelayDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(DelayActivity), "Resources.Delay.png")]
    [DefaultEvent("InitializeTimeoutDuration")]
    [ActivityValidator(typeof(DelayActivityValidator))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class DelayActivity : Activity, IEventActivity, IActivityEventListener<QueueEventArgs>
    {
        #region Public Dependency Properties
        public static readonly DependencyProperty InitializeTimeoutDurationEvent = DependencyProperty.Register("InitializeTimeoutDuration", typeof(EventHandler), typeof(DelayActivity));
        public static readonly DependencyProperty TimeoutDurationProperty = DependencyProperty.Register("TimeoutDuration", typeof(TimeSpan), typeof(DelayActivity), new PropertyMetadata(new TimeSpan(0, 0, 0)));
        #endregion
        #region Private Dependency Properties
        private static readonly DependencyProperty QueueNameProperty = DependencyProperty.Register("QueueName", typeof(IComparable), typeof(DelayActivity));
        #endregion

        #region Constructors

        public DelayActivity()
        {
        }

        public DelayActivity(string name)
            : base(name)
        {
        }

        #endregion

        #region Public Handlers
        [SRCategory(SR.Handlers)]
        [SRDescription(SR.TimeoutInitializerDescription)]
        [MergableProperty(false)]
        public event EventHandler InitializeTimeoutDuration
        {
            add
            {
                base.AddHandler(InitializeTimeoutDurationEvent, value);
            }
            remove
            {
                base.RemoveHandler(InitializeTimeoutDurationEvent, value);
            }
        }
        #endregion

        #region Public Properties
        [SRDescription(SR.TimeoutDurationDescription)]
        [MergableProperty(false)]
        [TypeConverter(typeof(TimeoutDurationConverter))]
        public TimeSpan TimeoutDuration
        {
            get
            {
                return (TimeSpan)base.GetValue(TimeoutDurationProperty);
            }
            set
            {
                base.SetValue(TimeoutDurationProperty, value);
            }
        }
        #endregion

        #region Protected Methods

        protected override void Initialize(IServiceProvider provider)
        {
            base.Initialize(provider);

            // Define the queue name for this Delay
            this.SetValue(QueueNameProperty, Guid.NewGuid());
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (this.IsInEventActivityMode)
            {
                return ActivityExecutionStatus.Closed;
            }
            else
            {
                ((IEventActivity)this).Subscribe(executionContext, this);
                this.IsInEventActivityMode = false;
                return ActivityExecutionStatus.Executing;
            }
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (!this.IsInEventActivityMode)
            {
                if (this.SubscriptionID != Guid.Empty)
                {
                    ((IEventActivity)this).Unsubscribe(executionContext, this);
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        protected sealed override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (exception == null)
                throw new ArgumentNullException("exception");

            ActivityExecutionStatus newStatus = this.Cancel(executionContext);
            if (newStatus == ActivityExecutionStatus.Canceling)
                return ActivityExecutionStatus.Faulting;

            return newStatus;
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(DelayActivity.SubscriptionIDProperty);
            base.RemoveProperty(DelayActivity.IsInEventActivityModeProperty);
        }

        private class DelayActivityValidator : ActivityValidator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();

                DelayActivity delay = obj as DelayActivity;
                if (delay == null)
                    throw new InvalidOperationException();

                if (delay.TimeoutDuration.Ticks < 0)
                {
                    errors.Add(new ValidationError(SR.GetString(SR.Error_NegativeValue, new object[] { delay.TimeoutDuration.ToString(), "TimeoutDuration" }), ErrorNumbers.Error_NegativeValue));
                }

                errors.AddRange(base.Validate(manager, obj));
                return errors;
            }
        }
        #endregion

        #region Private Implementation
        #region Runtime Data / Properties
        static DependencyProperty SubscriptionIDProperty = DependencyProperty.Register("SubscriptionID", typeof(Guid), typeof(DelayActivity), new PropertyMetadata(Guid.NewGuid()));
        static DependencyProperty IsInEventActivityModeProperty = DependencyProperty.Register("IsInEventActivityMode", typeof(bool), typeof(DelayActivity), new PropertyMetadata(false));

        private Guid SubscriptionID
        {
            get
            {
                return (Guid)base.GetValue(DelayActivity.SubscriptionIDProperty);
            }
            set
            {
                base.SetValue(DelayActivity.SubscriptionIDProperty, value);
            }
        }
        private bool IsInEventActivityMode
        {
            get
            {
                return (bool)base.GetValue(DelayActivity.IsInEventActivityModeProperty);
            }
            set
            {
                base.SetValue(DelayActivity.IsInEventActivityModeProperty, value);
            }
        }
        #endregion

        #region Timeout Duration Conversion Helper
        private sealed class TimeoutDurationConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string) && value is TimeSpan)
                {
                    TimeSpan timespan = (TimeSpan)value;
                    return timespan.ToString();
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                object parsedTimespan = TimeSpan.Zero;
                string timeSpan = value as string;
                if (!String.IsNullOrEmpty(timeSpan))
                {
                    //If this fails then an exception is thrown and the property set would fail
                    try
                    {
                        parsedTimespan = TimeSpan.Parse(timeSpan, CultureInfo.InvariantCulture);
                        if (parsedTimespan != null && ((TimeSpan)parsedTimespan).Ticks < 0)
                            throw new Exception(string.Format(System.Globalization.CultureInfo.CurrentCulture, SR.GetString(SR.Error_NegativeValue), value.ToString(), "TimeoutDuration"));
                    }
                    catch
                    {
                        throw new Exception(SR.GetString(SR.InvalidTimespanFormat, timeSpan));
                    }
                }

                return parsedTimespan;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                ArrayList standardValuesCollection = new ArrayList();
                standardValuesCollection.Add(new TimeSpan(0, 0, 0));
                standardValuesCollection.Add(new TimeSpan(0, 1, 0));
                standardValuesCollection.Add(new TimeSpan(0, 30, 0));
                standardValuesCollection.Add(new TimeSpan(1, 0, 0));
                standardValuesCollection.Add(new TimeSpan(12, 0, 0));
                standardValuesCollection.Add(new TimeSpan(1, 0, 0, 0));

                return new StandardValuesCollection(standardValuesCollection);
            }
        }
        #endregion
        #endregion

        #region IActivityEventListener<QueueEventArgs> Implementation
        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;

            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            if (this.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                System.Diagnostics.Debug.Assert(this.SubscriptionID != Guid.Empty);

                WorkflowQueuingService qService = context.GetService<WorkflowQueuingService>();
                qService.GetWorkflowQueue(e.QueueName).Dequeue();
                qService.DeleteWorkflowQueue(e.QueueName);
                context.CloseActivity();
            }
        }
        #endregion

        #region IEventActivity implementation
        void IEventActivity.Subscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (parentEventHandler == null)
                throw new ArgumentNullException("parentEventHandler");

            this.IsInEventActivityMode = true;

            base.RaiseEvent(DelayActivity.InitializeTimeoutDurationEvent, this, EventArgs.Empty);
            TimeSpan timeSpan = this.TimeoutDuration;
            DateTime timeOut = DateTime.UtcNow + timeSpan;

            WorkflowQueuingService qService = parentContext.GetService<WorkflowQueuingService>();

            IComparable queueName = ((IEventActivity)this).QueueName;
            TimerEventSubscription timerSub = new TimerEventSubscription((Guid)queueName, this.WorkflowInstanceId, timeOut);
            WorkflowQueue queue = qService.CreateWorkflowQueue(queueName, false);

            queue.RegisterForQueueItemAvailable(parentEventHandler, this.QualifiedName);
            this.SubscriptionID = timerSub.SubscriptionId;

            Activity root = this;
            while (root.Parent != null)
                root = root.Parent;

            TimerEventSubscriptionCollection timers = (TimerEventSubscriptionCollection)root.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
            Debug.Assert(timers != null, "TimerEventSubscriptionCollection on root activity should never be null, but it was");
            timers.Add(timerSub);
        }

        void IEventActivity.Unsubscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
                throw new ArgumentNullException("parentContext");
            if (parentEventHandler == null)
                throw new ArgumentNullException("parentEventHandler");

            System.Diagnostics.Debug.Assert(this.SubscriptionID != Guid.Empty);
            WorkflowQueuingService qService = parentContext.GetService<WorkflowQueuingService>();
            WorkflowQueue wfQueue = null;

            try
            {
                wfQueue = qService.GetWorkflowQueue(this.SubscriptionID);
            }
            catch
            {
                // If the queue no longer exists, eat the exception, we clear the subscription id later.
            }

            if (wfQueue != null && wfQueue.Count != 0)
                wfQueue.Dequeue();

            // WinOE 

            Activity root = parentContext.Activity;
            while (root.Parent != null)
                root = root.Parent;
            TimerEventSubscriptionCollection timers = (TimerEventSubscriptionCollection)root.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty);
            Debug.Assert(timers != null, "TimerEventSubscriptionCollection on root activity should never be null, but it was");
            timers.Remove(this.SubscriptionID);

            if (wfQueue != null)
            {
                wfQueue.UnregisterForQueueItemAvailable(parentEventHandler);
                qService.DeleteWorkflowQueue(this.SubscriptionID);
            }

            this.SubscriptionID = Guid.Empty;
        }

        IComparable IEventActivity.QueueName
        {
            get
            {
                return (IComparable)this.GetValue(QueueNameProperty);
            }
        }

        #endregion
    }
}
