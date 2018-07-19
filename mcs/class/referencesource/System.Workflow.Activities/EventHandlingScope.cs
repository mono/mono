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
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.EventHandlingScopeActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [ToolboxBitmap(typeof(EventHandlingScopeActivity), "Resources.Sequence.png")]
    [ActivityValidator(typeof(EventHandlingScopeValidator))]
    [Designer(typeof(EventHandlingScopeDesigner), typeof(IDesigner))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class EventHandlingScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public EventHandlingScopeActivity()
        {
        }
        public EventHandlingScopeActivity(string name)
            : base(name)
        {
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            Activity bodyActivity = this.BodyActivity;

            if (bodyActivity == null)
                return ActivityExecutionStatus.Closed;

            // run EventHandlers
            EventHandlersActivity eventHandlers = this.EventHandlersActivity;
            if (eventHandlers != null)
            {
                eventHandlers.RegisterForStatusChange(Activity.ClosedEvent, this);
                executionContext.ExecuteActivity(eventHandlers);
            }

            // run body
            bodyActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
            executionContext.ExecuteActivity(bodyActivity);


            // return the status
            return this.ExecutionStatus;
        }
        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            Activity bodyActivity = this.BodyActivity;
            EventHandlersActivity eventHandlers = this.EventHandlersActivity;

            if (bodyActivity == null && eventHandlers == null)
                return ActivityExecutionStatus.Closed;

            bool cancelScheduled = false;

            // check the status of body
            if (bodyActivity != null && bodyActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                executionContext.CancelActivity(bodyActivity);
                cancelScheduled = true;
            }
            //Check the status of EventHandlers
            if (eventHandlers != null && eventHandlers.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                executionContext.CancelActivity(eventHandlers);
                cancelScheduled = true;
            }

            if (cancelScheduled ||
                        (bodyActivity != null && (bodyActivity.ExecutionStatus == ActivityExecutionStatus.Faulting ||
                        bodyActivity.ExecutionStatus == ActivityExecutionStatus.Canceling)) ||
                        (eventHandlers != null && (eventHandlers.ExecutionStatus == ActivityExecutionStatus.Faulting ||
                        eventHandlers.ExecutionStatus == ActivityExecutionStatus.Canceling))
                )
            {
                return this.ExecutionStatus;
            }
            return ActivityExecutionStatus.Closed;
        }

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException();

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            if (e.Activity is EventHandlersActivity)
            {
                if (this.BodyActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    context.CloseActivity();
                }
                //else Eventhandlers faulted, let exception propagate up.                                
            }
            else
            {
                EventHandlersActivity eventHandlers = this.EventHandlersActivity;
                if (eventHandlers == null || eventHandlers.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    context.CloseActivity();
                }
                else
                {
                    eventHandlers.UnsubscribeAndClose();
                }
            }
        }
        #endregion

        internal EventHandlersActivity EventHandlersActivity
        {
            get
            {
                EventHandlersActivity eventHandlers = null;
                foreach (Activity child in this.EnabledActivities)
                {
                    if (child is EventHandlersActivity)
                        eventHandlers = child as EventHandlersActivity;
                }
                return eventHandlers;
            }
        }
        internal Activity BodyActivity
        {
            get
            {
                Activity body = null;
                foreach (Activity child in this.EnabledActivities)
                {
                    if (!(child is EventHandlersActivity))
                        body = child;
                }
                return body;
            }
        }
        #region Dynamic Update Methods

        [NonSerialized]
        private bool eventHandlersRemovedInDynamicUpdate = false;
        [NonSerialized]
        private bool bodyActivityRemovedInDynamicUpdate = false;

        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            base.OnActivityChangeRemove(executionContext, removedActivity);

            if (removedActivity is EventHandlersActivity)
                eventHandlersRemovedInDynamicUpdate = true;
            else
                bodyActivityRemovedInDynamicUpdate = true;
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            base.OnWorkflowChangesCompleted(executionContext);

            switch (this.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                    if (bodyActivityRemovedInDynamicUpdate)
                    {
                        if (EventHandlersActivity == null || EventHandlersActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                            executionContext.CloseActivity();
                        else if (EventHandlersActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                            EventHandlersActivity.UnsubscribeAndClose();
                    }
                    if (eventHandlersRemovedInDynamicUpdate)
                    {
                        if (BodyActivity == null || BodyActivity.ExecutionStatus == ActivityExecutionStatus.Closed)
                            executionContext.CloseActivity();
                    }
                    break;
                default:
                    break;
            }
            eventHandlersRemovedInDynamicUpdate = false;
            bodyActivityRemovedInDynamicUpdate = false;
        }
        #endregion
    }

    #region Class EventHandlingScopeValidator
    internal sealed class EventHandlingScopeValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            EventHandlingScopeActivity compositeActivity = obj as EventHandlingScopeActivity;
            if (compositeActivity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(EventHandlingScopeActivity).FullName), "obj");

            //we only allow one activity to be inserted
            int childCount = 0;
            int eventHandlersCount = 0;
            foreach (Activity child in compositeActivity.EnabledActivities)
            {
                if (child is EventHandlersActivity)
                    eventHandlersCount++;
                else
                    childCount++;
            }

            // check if more than two activities inside the collection
            if (childCount > 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MoreThanTwoActivitiesInEventHandlingScope, compositeActivity.QualifiedName), ErrorNumbers.Error_MoreThanTwoActivitiesInEventHandlingScope));

            // check if more than one EventHandlers
            if (eventHandlersCount > 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MoreThanOneEventHandlersDecl, compositeActivity.GetType().Name), ErrorNumbers.Error_ScopeMoreThanOneEventHandlersDecl));

            return validationErrors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (action == null)
                throw new ArgumentNullException("action");

            if (activity.ExecutionStatus != ActivityExecutionStatus.Initialized &&
                activity.ExecutionStatus != ActivityExecutionStatus.Executing &&
                activity.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                return new ValidationError(SR.GetString(SR.Error_DynamicActivity2, activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus), activity.GetType().FullName), ErrorNumbers.Error_DynamicActivity2);
            }
            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing && action is AddedActivityAction)
            {
                return new ValidationError(SR.GetString(SR.Error_DynamicActivity3, activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus), activity.GetType().FullName), ErrorNumbers.Error_DynamicActivity2);
            }
            return null;
        }
    }
    #endregion

    #region Class EventHandlingScopeDesigner

    [ActivityDesignerTheme(typeof(EventHandlingScopeActivityDesignerTheme))]
    internal sealed class EventHandlingScopeDesigner : SequentialActivityDesigner
    {
        #region Properties and Methods
        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }
        #endregion

        public override bool CanInsertActivities(HitTestInfo insertLocation, System.Collections.ObjectModel.ReadOnlyCollection<Activity> activitiesToInsert)
        {
            //we only allow one activity to be inserted
            int childCount = 0;
            foreach (Activity child in ((EventHandlingScopeActivity)this.Activity).Activities)
            {
                if (!Helpers.IsFrameworkActivity(child) &&
                    !(child is EventHandlersActivity))
                    childCount++;
            }
            if (childCount > 0)
                return false;

            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }
    }
    #endregion

    #region EventHandlingScopeActivityDesignerTheme
    internal sealed class EventHandlingScopeActivityDesignerTheme : CompositeDesignerTheme
    {
        public EventHandlingScopeActivityDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            this.BackColorEnd = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
        }
    }
    #endregion
}
