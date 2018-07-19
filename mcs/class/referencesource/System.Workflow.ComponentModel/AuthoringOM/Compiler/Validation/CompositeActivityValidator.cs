namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Workflow.ComponentModel.Design;

    #region Class CompositeActivityValidator
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CompositeActivityValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            CompositeActivity compositeActivity = obj as CompositeActivity;
            if (compositeActivity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(CompositeActivity).FullName), "obj");
            if (Helpers.IsActivityLocked(compositeActivity))
                return new ValidationErrorCollection();

            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            // check if more than one cancellation handler or compensation or fault handlers are specified
            int cancelHandlerCount = 0;
            int exceptionHandlersCount = 0;
            int compensationHandlerCount = 0;
            foreach (Activity activity in ((ISupportAlternateFlow)compositeActivity).AlternateFlowActivities)
            {
                cancelHandlerCount += (activity is CancellationHandlerActivity) ? 1 : 0;
                exceptionHandlersCount += (activity is FaultHandlersActivity) ? 1 : 0;
                compensationHandlerCount += (activity is CompensationHandlerActivity) ? 1 : 0;
            }
            // check cancellation handlers
            if (cancelHandlerCount > 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MoreThanOneCancelHandler, compositeActivity.GetType().Name), ErrorNumbers.Error_ScopeMoreThanOneEventHandlersDecl));

            // check exception handlers
            if (exceptionHandlersCount > 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MoreThanOneFaultHandlersActivityDecl, compositeActivity.GetType().Name), ErrorNumbers.Error_ScopeMoreThanOneFaultHandlersActivityDecl));

            // check compensation handlers
            if (compensationHandlerCount > 1)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_MoreThanOneCompensationDecl, compositeActivity.GetType().Name), ErrorNumbers.Error_ScopeMoreThanOneCompensationDecl));


            if (manager.ValidateChildActivities)
            {
                foreach (Activity childActivity in Helpers.GetAllEnabledActivities(compositeActivity))
                    validationErrors.AddRange(ValidationHelpers.ValidateActivity(manager, childActivity));
            }
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
                return new ValidationError(SR.GetString(SR.Error_DynamicActivity, activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus)), ErrorNumbers.Error_DynamicActivity);
            }

            return null;
        }

    }
    #endregion
}
