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
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #endregion

    [SRDescription(SR.WhileActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(WhileDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(WhileValidator))]
    [ToolboxBitmap(typeof(WhileActivity), "Resources.While.png")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WhileActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        #region Dependency Properties
        //metadata properties go here
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(ActivityCondition), typeof(WhileActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        #endregion

        #region Constructors

        public WhileActivity()
        {
        }

        public WhileActivity(string name)
            : base(name)
        {
        }

        #endregion

        #region Public Properties
        [SRCategory(SR.Conditions)]
        [SRDescription(SR.WhileConditionDescr)]
        public ActivityCondition Condition
        {
            get
            {
                return base.GetValue(ConditionProperty) as ActivityCondition;
            }
            set
            {
                base.SetValue(ConditionProperty, value);
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Activity DynamicActivity
        {
            get
            {
                if (this.EnabledActivities.Count > 0)
                {
                    Activity[] dynamicChildren = this.GetDynamicActivities(this.EnabledActivities[0]);
                    if (dynamicChildren.Length != 0)
                        return dynamicChildren[0];
                }
                return null;
            }
        }
        #endregion

        #region Protected Methods
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (TryNextIteration(executionContext))
                return ActivityExecutionStatus.Executing;

            return ActivityExecutionStatus.Closed;
        }
        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (this.EnabledActivities.Count == 0)
                return ActivityExecutionStatus.Closed;

            Activity childActivity = this.EnabledActivities[0];
            ActivityExecutionContextManager contextManager = executionContext.ExecutionContextManager;
            ActivityExecutionContext childContext = contextManager.GetExecutionContext(childActivity);

            if (childContext != null)
            {
                if (childContext.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                    childContext.CancelActivity(childContext.Activity);

                return ActivityExecutionStatus.Canceling;
            }
            return ActivityExecutionStatus.Closed;
        }
        #endregion

        #region Private Implementations
        private bool TryNextIteration(ActivityExecutionContext context)
        {
            if (this.ExecutionStatus == ActivityExecutionStatus.Canceling || this.ExecutionStatus == ActivityExecutionStatus.Faulting || !this.Condition.Evaluate(this, context))
                return false;
            else
            {
                if (this.EnabledActivities.Count > 0)
                {
                    ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
                    ActivityExecutionContext innerContext = contextManager.CreateExecutionContext(this.EnabledActivities[0]);
                    innerContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                    innerContext.ExecuteActivity(innerContext.Activity);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }
                return true;
            }
        }
        #endregion

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members
        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException("e");
            if (sender == null)
                throw new ArgumentNullException("sender");

            ActivityExecutionContext context = sender as ActivityExecutionContext;

            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
            contextManager.CompleteExecutionContext(contextManager.GetExecutionContext(e.Activity));

            if (!TryNextIteration(context))
                context.CloseActivity();
        }
        #endregion
    }

    #region Validator
    internal sealed class WhileValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection(base.Validate(manager, obj));

            WhileActivity whileActivity = obj as WhileActivity;
            if (whileActivity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(WhileActivity).FullName), "obj");
            if (whileActivity.EnabledActivities.Count != 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WhileShouldHaveOneChild), ErrorNumbers.Error_WhileShouldHaveOneChild));

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
            return null;
        }
    }
    #endregion
}
