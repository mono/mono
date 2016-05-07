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
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #endregion


    [SRDescription(SR.ListenActivityDescription)]
    [ToolboxItem(typeof(ListenToolboxItem))]
    [Designer(typeof(ListenDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(ListenActivity), "Resources.Listen.png")]
    [ActivityValidator(typeof(ListenValidator))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ListenActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        #region Constructors

        public ListenActivity()
        {
        }

        public ListenActivity(string name)
            : base(name)
        {
        }

        #endregion

        #region Protected methods
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            List<ListenEventActivitySubscriber> eventActivitySubscribers = new List<ListenEventActivitySubscriber>();
            this.ActivityState = eventActivitySubscribers;

            //Subscribe to all EventDriven Children.
            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                EventDrivenActivity eventDriven = this.EnabledActivities[i] as EventDrivenActivity;
                ListenEventActivitySubscriber eventActivitySubscriber = new ListenEventActivitySubscriber(eventDriven);
                eventDriven.EventActivity.Subscribe(executionContext, eventActivitySubscriber);
                eventActivitySubscribers.Add(eventActivitySubscriber);
            }

            return ActivityExecutionStatus.Executing;
        }
        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (this.ActivityState == null)
                return ActivityExecutionStatus.Closed;

            try
            {
                if (this.IsListenTrigerred)
                {
                    //We need to cancel the active running branch
                    for (int i = 0; i < this.EnabledActivities.Count; ++i)
                    {
                        EventDrivenActivity eventDriven = this.EnabledActivities[i] as EventDrivenActivity;

                        if (eventDriven.ExecutionStatus == ActivityExecutionStatus.Executing)
                        {
                            executionContext.CancelActivity(eventDriven);
                            return ActivityExecutionStatus.Canceling;
                        } //If the branch is faulting let it close.
                        else if (eventDriven.ExecutionStatus == ActivityExecutionStatus.Faulting)
                        {
                            return ActivityExecutionStatus.Canceling;
                        }
                    }
                }
                else
                {
                    //Everything is passive. Lets unsubscribe all and close.
                    for (int i = 0; i < this.ActivityState.Count; ++i)
                    {
                        EventDrivenActivity eventDrivenChild = this.EnabledActivities[i] as EventDrivenActivity;
                        ListenEventActivitySubscriber eventActivitySubscriber = this.ActivityState[i];
                        eventDrivenChild.EventActivity.Unsubscribe(executionContext, eventActivitySubscriber);
                    }
                }
            }
            finally
            {
                // We null out ActivityState in the finally block to ensure that if 
                // eventDrivenChild.EventActivity.Unsubscribe above throws then the
                // Cancel method does not get called repeatedly.
                this.ActivityState = null;
            }

            return ActivityExecutionStatus.Closed;
        }
        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(ListenActivity.IsListenTrigerredProperty);
            base.RemoveProperty(ListenActivity.ActivityStateProperty);
        }
        #region Dynamic Update Methods

        [NonSerialized]
        private bool activeBranchRemoved = false;

        protected override sealed void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            Debug.Assert(addedActivity is EventDrivenActivity, "Listen only contains EventDriven activities");

            ListenActivity listen = executionContext.Activity as ListenActivity;

            if (listen.ExecutionStatus == ActivityExecutionStatus.Executing && listen.ActivityState != null && !listen.IsListenTrigerred)
            {
                EventDrivenActivity eda = addedActivity as EventDrivenActivity;
                ListenEventActivitySubscriber eventActivitySubscriber = new ListenEventActivitySubscriber(eda);
                eda.EventActivity.Subscribe(executionContext, eventActivitySubscriber);
                listen.ActivityState.Insert(listen.EnabledActivities.IndexOf(addedActivity), eventActivitySubscriber);
            }
        }
        protected override sealed void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (removedActivity == null)
                throw new ArgumentNullException("removedActivity");

            ListenActivity listen = executionContext.Activity as ListenActivity;

            if (listen.ExecutionStatus == ActivityExecutionStatus.Executing && listen.ActivityState != null && !listen.IsListenTrigerred)
            {
                EventDrivenActivity eda = removedActivity as EventDrivenActivity;

                for (int i = 0; i < listen.ActivityState.Count; ++i)
                {
                    ListenEventActivitySubscriber listenEventSubscriber = listen.ActivityState[i];

                    if (listenEventSubscriber.eventDrivenActivity.QualifiedName.Equals(eda.QualifiedName))
                    {
                        eda.EventActivity.Unsubscribe(executionContext, listenEventSubscriber);
                        listen.ActivityState.RemoveAt(i);
                        return;
                    }
                }
            }
            else if (this.IsListenTrigerred && removedActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                activeBranchRemoved = true;
            }
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            base.OnWorkflowChangesCompleted(executionContext);

            if (activeBranchRemoved)
                executionContext.CloseActivity();

            activeBranchRemoved = false;
        }
        #endregion
        #endregion

        #region Private Implementation
        #region Private Runtime State Dependency Properties
        static DependencyProperty IsListenTrigerredProperty = DependencyProperty.Register("IsListenTrigerred", typeof(bool), typeof(ListenActivity), new PropertyMetadata(false));
        static DependencyProperty ActivityStateProperty = DependencyProperty.Register("ActivityState", typeof(List<ListenEventActivitySubscriber>), typeof(ListenActivity));

        private bool IsListenTrigerred
        {
            get
            {
                return (bool)base.GetValue(IsListenTrigerredProperty);
            }
            set
            {
                base.SetValue(IsListenTrigerredProperty, value);
            }
        }
        private List<ListenEventActivitySubscriber> ActivityState
        {
            get
            {
                return (List<ListenEventActivitySubscriber>)base.GetValue(ActivityStateProperty);
            }
            set
            {
                if (value == null)
                    base.RemoveProperty(ActivityStateProperty);
                else
                    base.SetValue(ActivityStateProperty, value);
            }
        }
        #endregion

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members
        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;

            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            context.CloseActivity();
        }
        #endregion

        #region Subscription Datastructures
        [Serializable]
        private sealed class ListenEventActivitySubscriber : IActivityEventListener<QueueEventArgs>
        {
            internal EventDrivenActivity eventDrivenActivity;

            internal ListenEventActivitySubscriber(EventDrivenActivity eventDriven)
            {
                this.eventDrivenActivity = eventDriven;
            }

            void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
            {
                if (sender == null)
                    throw new ArgumentNullException("sender");
                if (e == null)
                    throw new ArgumentNullException("e");

                ActivityExecutionContext context = sender as ActivityExecutionContext;

                if (context == null)
                    throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

                ListenActivity parentActivity = context.Activity as ListenActivity;

                if (!parentActivity.IsListenTrigerred && parentActivity.ExecutionStatus != ActivityExecutionStatus.Canceling && parentActivity.ExecutionStatus != ActivityExecutionStatus.Closed)
                {
                    //Check whether it is still live in tree.
                    if (!parentActivity.EnabledActivities.Contains(eventDrivenActivity))//Activity is dynamically removed.
                        return;

                    parentActivity.IsListenTrigerred = true;

                    for (int i = 0; i < parentActivity.EnabledActivities.Count; ++i)
                    {
                        EventDrivenActivity eventDriven = parentActivity.EnabledActivities[i] as EventDrivenActivity;
                        ListenEventActivitySubscriber eventSubscriber = parentActivity.ActivityState[i];
                        eventDriven.EventActivity.Unsubscribe(context, eventSubscriber);
                    }

                    eventDrivenActivity.RegisterForStatusChange(Activity.ClosedEvent, parentActivity);
                    context.ExecuteActivity(eventDrivenActivity);
                }
            }
        }
        #endregion
        #endregion
    }

    #region Validator
    internal sealed class ListenValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection(base.Validate(manager, obj));

            ListenActivity listen = obj as ListenActivity;
            if (listen == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(ListenActivity).FullName), "obj");

            // Validate number of children
            if (listen.EnabledActivities.Count < 2)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ListenLessThanTwoChildren), ErrorNumbers.Error_ListenLessThanTwoChildren));

            bool bNotAllEventDriven = false;

            foreach (Activity activity in listen.EnabledActivities)
            {
                if (!(activity is EventDrivenActivity))
                    bNotAllEventDriven = true;
            }

            // validate that all child activities are event driven activities.
            if (bNotAllEventDriven)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ListenNotAllEventDriven), ErrorNumbers.Error_ListenNotAllEventDriven));

            return validationErrors;
        }
    }
    #endregion
}
