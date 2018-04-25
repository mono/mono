namespace System.Workflow.ComponentModel
{
    using System;
    using System.Drawing;
    using System.ComponentModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;

    [SRDescription(SR.CompensateActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(CompensateDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(CompensateActivity), "Resources.Compensate.png")]
    [ActivityValidator(typeof(CompensateValidator))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CompensateActivity : Activity, IPropertyValueProvider, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public CompensateActivity()
        {
        }
        public CompensateActivity(string name)
            : base(name)
        {
        }

        public static readonly DependencyProperty TargetActivityNameProperty = DependencyProperty.Register("TargetActivityName", typeof(string), typeof(CompensateActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));

        [SRCategory(SR.Activity)]
        [SRDescription(SR.CompensatableActivityDescr)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [MergableProperty(false)]
        [DefaultValue("")]
        public string TargetActivityName
        {
            get
            {
                return base.GetValue(TargetActivityNameProperty) as string;
            }
            set
            {
                base.SetValue(TargetActivityNameProperty, value);
            }
        }

        #region Protected Methods
        protected internal override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            return CompensateTargetActivity(executionContext);
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

            if (e.ExecutionStatus == ActivityExecutionStatus.Closed)
            {
                // Remove status change subscription.
                e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);

                // Do it again if there are any more.
                ActivityExecutionStatus status = CompensateTargetActivity(context);
                if (status == ActivityExecutionStatus.Closed)
                    context.CloseActivity();
            }
        }
        #endregion

        #region Execution Helpers
        private ActivityExecutionStatus CompensateTargetActivity(ActivityExecutionContext context)
        {
            Activity targetActivity = null;
            Activity commonParentActivity = context.Activity;
            do
            {
                commonParentActivity = commonParentActivity.Parent;
                targetActivity = commonParentActivity.GetActivityByName(this.TargetActivityName, true);
            } while (targetActivity == null);

            if (targetActivity is ICompensatableActivity &&
                targetActivity.ExecutionStatus == ActivityExecutionStatus.Closed &&
                targetActivity.ExecutionResult == ActivityExecutionResult.Succeeded)
            {
                // same execution context
                targetActivity.RegisterForStatusChange(Activity.ClosedEvent, this);
                context.CompensateActivity(targetActivity);
                return context.Activity.ExecutionStatus;
            }
            else if (targetActivity.ExecutionStatus == ActivityExecutionStatus.Initialized)
            {
                // Template activity

                // walk through active contexts
                ActivityExecutionContextManager contextManager = context.ExecutionContextManager;
                foreach (ActivityExecutionContext activeContext in contextManager.ExecutionContexts)
                {
                    if (targetActivity.GetActivityByName(activeContext.Activity.QualifiedName, true) != null)
                    {
                        if (activeContext.Activity.ExecutionStatus == ActivityExecutionStatus.Compensating ||
                            activeContext.Activity.ExecutionStatus == ActivityExecutionStatus.Faulting ||
                            activeContext.Activity.ExecutionStatus == ActivityExecutionStatus.Canceling
                            )
                            return context.Activity.ExecutionStatus;
                    }
                }

                // walk through all completed execution contexts
                for (int index = contextManager.CompletedExecutionContexts.Count - 1; index >= 0; index--)
                {
                    //only compensate direct child during explicit compensation 
                    ActivityExecutionContextInfo completedActivityInfo = contextManager.CompletedExecutionContexts[index];
                    if (((completedActivityInfo.Flags & PersistFlags.NeedsCompensation) != 0))
                    {
                        ActivityExecutionContext revokedExecutionContext = contextManager.DiscardPersistedExecutionContext(completedActivityInfo);
                        if (revokedExecutionContext.Activity is ICompensatableActivity)
                        {
                            revokedExecutionContext.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                            revokedExecutionContext.CompensateActivity(revokedExecutionContext.Activity);
                        }
                        return context.Activity.ExecutionStatus;
                    }
                }
            }
            else
            {
                // currently faulting, canceling, or compensating
                if (CompensationUtils.TryCompensateLastCompletedChildActivity(context, targetActivity, this))
                    return context.Activity.ExecutionStatus;
            }
            return ActivityExecutionStatus.Closed;
        }
        #endregion

        #region IPropertyValueProvider Members

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            return GetCompensatableTargets(this);
        }

        #endregion

        #region Validation Helpers
        internal static StringCollection GetCompensatableTargets(CompensateActivity compensate)
        {
            StringCollection targetList = new StringCollection();
            CompositeActivity parent = compensate.Parent;
            while (parent != null)
            {
                if ((parent is CompensationHandlerActivity) || (parent is FaultHandlersActivity) || (parent is CancellationHandlerActivity))
                {
                    parent = parent.Parent;
                    if (parent != null)
                    {
                        if (Helpers.IsCustomActivity(parent))
                            targetList.Add(parent.UserData[UserDataKeys.CustomActivityDefaultName] as string);
                        else
                            targetList.Add(parent.Name);

                        foreach (Activity activity in parent.EnabledActivities)
                        {
                            if (activity is ICompensatableActivity)
                                targetList.Add(activity.Name);
                        }
                    }
                    break;
                }
                parent = parent.Parent;
            }
            return targetList;
        }
        #endregion
    }

    #region Validator
    internal sealed class CompensateValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            CompensateActivity compensate = obj as CompensateActivity;
            if (compensate == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(CompensateActivity).FullName), "obj");

            // Compensate must be in a CompensationHandler or FaultHandler
            CompositeActivity parent = compensate.Parent;
            while (parent != null)
            {
                if (parent is CompensationHandlerActivity || parent is FaultHandlerActivity || parent is CancellationHandlerActivity)
                    break;

                parent = parent.Parent;
            }

            if (parent == null)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_CompensateBadNesting), ErrorNumbers.Error_CompensateBadNesting));

            ValidationError error = null;
            StringCollection targets = CompensateActivity.GetCompensatableTargets(compensate);
            if (String.IsNullOrEmpty(compensate.TargetActivityName))
            {
                error = ValidationError.GetNotSetValidationError("TargetActivityName");
            }
            else if (!targets.Contains(compensate.TargetActivityName))
            {
                error = new ValidationError(SR.GetString(SR.Error_CompensateBadTargetTX, "TargetActivityName", compensate.TargetActivityName, compensate.QualifiedName), ErrorNumbers.Error_CompensateBadTargetTX, false, "TargetActivityName");
            }
            if (error != null)
                validationErrors.Add(error);

            return validationErrors;
        }
    }
    #endregion
}
