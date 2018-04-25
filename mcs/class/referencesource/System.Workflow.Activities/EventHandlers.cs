namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.CodeDom;
    using System.Drawing;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;
    using System.Workflow.Activities.Common;

    #endregion

    [ToolboxItem(false)]
    [Designer(typeof(EventHandlersDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(EventHandlersActivity), "Resources.events.png")]
    [ActivityValidator(typeof(EventHandlersValidator))]
    [SRCategory(SR.Standard)]
    [AlternateFlowActivityAttribute]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class EventHandlersActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public EventHandlersActivity()
        {
        }

        public EventHandlersActivity(string name)
            : base(name)
        {
        }

        #region Runtime State Specific Dependency Property
        static DependencyProperty ActivityStateProperty = DependencyProperty.Register("ActivityState", typeof(List<EventHandlerEventActivitySubscriber>), typeof(EventHandlersActivity));
        static DependencyProperty IsScopeCompletedProperty = DependencyProperty.Register("IsScopeCompleted", typeof(bool), typeof(EventHandlersActivity), new PropertyMetadata(false));

        private List<EventHandlerEventActivitySubscriber> ActivityState
        {
            get
            {
                return (List<EventHandlerEventActivitySubscriber>)base.GetValue(ActivityStateProperty);
            }
            set
            {
                if (value == null)
                    base.RemoveProperty(ActivityStateProperty);
                else
                    base.SetValue(ActivityStateProperty, value);
            }
        }

        private bool IsScopeCompleted
        {
            get
            {
                return (bool)base.GetValue(IsScopeCompletedProperty);
            }
            set
            {
                base.SetValue(IsScopeCompletedProperty, value);
            }
        }
        #endregion

        internal void UnsubscribeAndClose()
        {
            base.Invoke<EventArgs>(this.OnUnsubscribeAndClose, EventArgs.Empty);
        }

        #region Protected Methods
        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(EventHandlersActivity.ActivityStateProperty);
            base.RemoveProperty(EventHandlersActivity.IsScopeCompletedProperty);
        }

        protected override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            List<EventHandlerEventActivitySubscriber> eventActivitySubscribers = new List<EventHandlerEventActivitySubscriber>();
            this.ActivityState = eventActivitySubscribers;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                EventDrivenActivity childActivity = this.EnabledActivities[i] as EventDrivenActivity;
                EventHandlerEventActivitySubscriber eventDrivenSubscriber = new EventHandlerEventActivitySubscriber(childActivity);
                eventActivitySubscribers.Add(eventDrivenSubscriber);
                childActivity.EventActivity.Subscribe(executionContext, eventDrivenSubscriber);
            }
            return ActivityExecutionStatus.Executing;
        }
        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (this.ActivityState == null)
                return ActivityExecutionStatus.Closed;

            bool scopeCompleted = this.IsScopeCompleted;
            bool canCloseNow = true;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                EventDrivenActivity childActivity = this.EnabledActivities[i] as EventDrivenActivity;
                EventHandlerEventActivitySubscriber eventActivitySubscriber = this.ActivityState[i] as EventHandlerEventActivitySubscriber;

                eventActivitySubscriber.PendingExecutionCount = 0;

                ActivityExecutionContextManager contextManager = executionContext.ExecutionContextManager;
                ActivityExecutionContext childContext = contextManager.GetExecutionContext(childActivity);

                if (childContext != null)
                {
                    switch (childContext.Activity.ExecutionStatus)
                    {
                        case ActivityExecutionStatus.Canceling:
                        case ActivityExecutionStatus.Faulting:
                            canCloseNow = false;
                            break;
                        case ActivityExecutionStatus.Executing:
                            childContext.CancelActivity(childContext.Activity);
                            canCloseNow = false;
                            break;
                    }
                }

                if (!scopeCompleted) //UnSubscribe from event.
                {
                    childActivity.EventActivity.Unsubscribe(executionContext, eventActivitySubscriber);
                }
            }

            if (canCloseNow)
            {
                this.ActivityState = null;
                return ActivityExecutionStatus.Closed;
            }
            else
            {
                return this.ExecutionStatus;
            }
        }
        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (addedActivity == null)
                throw new ArgumentNullException("addedActivity");

            EventDrivenActivity eda = addedActivity as EventDrivenActivity;

            EventHandlersActivity activity = (EventHandlersActivity)executionContext.Activity as EventHandlersActivity;
            EventHandlerEventActivitySubscriber eventActivitySubscriber = new EventHandlerEventActivitySubscriber(eda);

            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing && activity.ActivityState != null && !activity.IsScopeCompleted)
            {
                eda.EventActivity.Subscribe(executionContext, eventActivitySubscriber);
                activity.ActivityState.Insert(activity.EnabledActivities.IndexOf(addedActivity), eventActivitySubscriber);
            }
        }
        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");
            if (removedActivity == null)
                throw new ArgumentNullException("removedActivity");

            EventDrivenActivity eda = removedActivity as EventDrivenActivity;

            // find out the status of the scope
            EventHandlersActivity activity = (EventHandlersActivity)executionContext.Activity as EventHandlersActivity;

            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing && activity.ActivityState != null && !activity.IsScopeCompleted)
            {
                for (int i = 0; i < activity.ActivityState.Count; ++i)
                {
                    EventHandlerEventActivitySubscriber eventSubscriber = activity.ActivityState[i];
                    if (eventSubscriber.eventDrivenActivity.QualifiedName.Equals(removedActivity.QualifiedName))
                    {
                        eda.EventActivity.Unsubscribe(executionContext, eventSubscriber);
                        activity.ActivityState.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            base.OnWorkflowChangesCompleted(executionContext);

            if (this.ActivityState != null)
            {
                switch (this.ExecutionStatus)
                {
                    case ActivityExecutionStatus.Executing:
                        if (this.IsScopeCompleted && AllHandlersAreQuiet(this, executionContext))
                            executionContext.CloseActivity();
                        break;
                    case ActivityExecutionStatus.Faulting:
                    case ActivityExecutionStatus.Canceling:
                        if (AllHandlersAreQuiet(this, executionContext))
                            executionContext.CloseActivity();
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region Private Impls

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

            EventDrivenActivity eda = e.Activity as EventDrivenActivity;
            EventHandlersActivity eventHandlers = context.Activity as EventHandlersActivity;

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            contextManager.CompleteExecutionContext(contextManager.GetExecutionContext(eda));

            switch (eventHandlers.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                    for (int i = 0; i < eventHandlers.EnabledActivities.Count; ++i)
                    {
                        if (eventHandlers.EnabledActivities[i].QualifiedName.Equals(eda.QualifiedName))
                        {
                            EventHandlerEventActivitySubscriber eventActivitySubscriber = eventHandlers.ActivityState[i];

                            if (eventActivitySubscriber.PendingExecutionCount > 0)
                            {
                                eventActivitySubscriber.PendingExecutionCount--;
                                eventActivitySubscriber.IsBlocked = false;

                                ActivityExecutionContext childContext = contextManager.CreateExecutionContext(eventHandlers.EnabledActivities[i]);
                                childContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                                childContext.ExecuteActivity(childContext.Activity);
                            }
                            else
                            {
                                eventActivitySubscriber.IsBlocked = true;
                                if (eventHandlers.IsScopeCompleted && AllHandlersAreQuiet(eventHandlers, context))
                                    context.CloseActivity();
                            }
                            break;
                        }
                    }
                    break;

                case ActivityExecutionStatus.Canceling:
                case ActivityExecutionStatus.Faulting:
                    if (AllHandlersAreQuiet(eventHandlers, context))
                        context.CloseActivity();
                    break;
            }
        }

        #endregion

        #region Helpers
        private bool AllHandlersAreQuiet(EventHandlersActivity handlers, ActivityExecutionContext context)
        {
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;

            for (int i = 0; i < handlers.EnabledActivities.Count; ++i)
            {
                EventDrivenActivity eventDriven = handlers.EnabledActivities[i] as EventDrivenActivity;
                if (contextManager.GetExecutionContext(eventDriven) != null || (handlers.ActivityState != null && handlers.ActivityState[i].PendingExecutionCount > 0))
                    return false;
            }
            return true;
        }
        private void OnUnsubscribeAndClose(object sender, EventArgs args)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (args == null)
                throw new ArgumentNullException("args");

            ActivityExecutionContext context = (ActivityExecutionContext)sender;
            if (context == null)
                throw new ArgumentException("sender");

            EventHandlersActivity handlers = context.Activity as EventHandlersActivity;
            if (context.Activity.ExecutionStatus != ActivityExecutionStatus.Executing)
                return;

            Debug.Assert(!handlers.IsScopeCompleted, "Only notified of scope body completion once");
            handlers.IsScopeCompleted = true;

            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            bool readyToClose = true;
            for (int i = 0; i < handlers.EnabledActivities.Count; ++i)
            {
                EventDrivenActivity evtDriven = handlers.EnabledActivities[i] as EventDrivenActivity;
                EventHandlerEventActivitySubscriber eventSubscriber = handlers.ActivityState[i];
                evtDriven.EventActivity.Unsubscribe(context, eventSubscriber);

                if (contextManager.GetExecutionContext(evtDriven) != null || handlers.ActivityState[i].PendingExecutionCount != 0)
                    readyToClose = false;
            }

            if (readyToClose)
            {
                handlers.ActivityState = null;
                context.CloseActivity();
            }
        }
        #endregion

        #region EventSubscriber
        [Serializable]
        private sealed class EventHandlerEventActivitySubscriber : IActivityEventListener<QueueEventArgs>
        {
            bool isBlocked;
            int numOfMsgs;

            internal EventDrivenActivity eventDrivenActivity;

            internal EventHandlerEventActivitySubscriber(EventDrivenActivity eventDriven)
            {
                isBlocked = true;
                numOfMsgs = 0;
                this.eventDrivenActivity = eventDriven;
            }

            internal bool IsBlocked
            {
                get
                {
                    return isBlocked;
                }
                set
                {
                    isBlocked = value;
                }
            }

            internal int PendingExecutionCount
            {
                get
                {
                    return numOfMsgs;
                }
                set
                {
                    numOfMsgs = value;
                }
            }

            void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
            {
                if (sender == null)
                    throw new ArgumentNullException("sender");
                if (e == null)
                    throw new ArgumentNullException("e");
                ActivityExecutionContext context = sender as ActivityExecutionContext;

                if (context == null)
                    throw new ArgumentException("sender");

                EventHandlersActivity handlers = context.Activity as EventHandlersActivity;

                if (handlers.ExecutionStatus != ActivityExecutionStatus.Executing)
                    return;

                if (!handlers.EnabledActivities.Contains(eventDrivenActivity))
                    return; //Activity is dynamically removed.

                if (IsBlocked)
                {
                    IsBlocked = false;
                    ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
                    ActivityExecutionContext childContext = contextManager.CreateExecutionContext(eventDrivenActivity);
                    childContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, handlers);
                    childContext.ExecuteActivity(childContext.Activity);
                }
                else
                {
                    PendingExecutionCount++;
                }
            }
        }
        #endregion
        #endregion

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Activity GetDynamicActivity(Activity childActivity)
        {
            if (childActivity == null)
                throw new ArgumentNullException("childActivity");

            if (!this.EnabledActivities.Contains(childActivity))
                throw new ArgumentException(SR.GetString(SR.Error_EventHandlersChildNotFound), "childActivity");
            else
            {
                Activity[] dynamicChildActivity = this.GetDynamicActivities(childActivity);

                if (dynamicChildActivity.Length != 0)
                    return dynamicChildActivity[0];
                else
                    return null;
            }
        }

        public Activity GetDynamicActivity(String childActivityName)
        {
            if (childActivityName == null)
                throw new ArgumentNullException("childActivityName");

            Activity childActivity = null;

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                if (this.EnabledActivities[i].QualifiedName.Equals(childActivityName))
                {
                    childActivity = this.EnabledActivities[i];
                    break;
                }
            }

            if (childActivity != null)
                return GetDynamicActivity(childActivity);

            throw new ArgumentException(SR.GetString(SR.Error_EventHandlersChildNotFound), "childActivityName");
        }
    }

    internal sealed class EventHandlersValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            EventHandlersActivity eventHandlers = obj as EventHandlersActivity;
            if (eventHandlers == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(EventHandlersActivity).FullName), "obj");

            if (eventHandlers.Parent == null)
            {
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MustHaveParent), ErrorNumbers.Error_EventHandlersDeclParentNotScope));
                return validationErrors;
            }

            // Parent must support event handlers
            if (!(eventHandlers.Parent is EventHandlingScopeActivity))
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_EventHandlersDeclParentNotScope, eventHandlers.Parent.QualifiedName), ErrorNumbers.Error_EventHandlersDeclParentNotScope));

            bool bNotAllEventHandler = false;
            foreach (Activity activity in eventHandlers.EnabledActivities)
            {
                if (!(activity is EventDrivenActivity))
                    bNotAllEventHandler = true;
            }

            // validate that all child activities are event driven activities.
            if (bNotAllEventHandler)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ListenNotAllEventDriven), ErrorNumbers.Error_ListenNotAllEventDriven));

            return validationErrors;
        }
    }
}
