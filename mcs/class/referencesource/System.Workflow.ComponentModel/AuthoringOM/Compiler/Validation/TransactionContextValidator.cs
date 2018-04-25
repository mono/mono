namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;

    internal sealed class TransactionContextValidator : Validator
    {
        #region Validate Method

        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            Activity activity = obj as Activity;
            if (activity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            WorkflowTransactionOptions atomicTransaction = TransactedContextFilter.GetTransactionOptions(activity);
            if (atomicTransaction != null)
            {
                // Atomic scopes can't have exception handlers.
                CompositeActivity exceptionHandlers = FaultAndCancellationHandlingFilter.GetFaultHandlers(activity);
                if (exceptionHandlers != null)
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_AtomicScopeWithFaultHandlersActivityDecl, activity.Name), ErrorNumbers.Error_AtomicScopeWithFaultHandlersActivityDecl);
                    validationErrors.Add(error);
                }

                // Atomic scopes can't have cancel handlers.
                Activity cancellationHandler = FaultAndCancellationHandlingFilter.GetCancellationHandler(activity);
                if (cancellationHandler != null)
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_AtomicScopeWithCancellationHandlerActivity, activity.Name), ErrorNumbers.Error_AtomicScopeWithCancellationHandlerActivity);
                    validationErrors.Add(error);
                }

                // check that this transaction scope is not nested inside another transaction scope
                Activity parent = activity.Parent;
                while (parent != null)
                {
                    if (parent.SupportsTransaction)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString(SR.Error_AtomicScopeNestedInNonLRT), ErrorNumbers.Error_AtomicScopeNestedInNonLRT));
                        break;
                    }
                    parent = parent.Parent;
                }

                // check that an activity with PersistOnClose/SupportsTransaction/ICompensatableActivity attribute is not nested inside the transaction scope
                Queue<Activity> nestedEnabledActivities = new Queue<Activity>(Helpers.GetAllEnabledActivities((CompositeActivity)activity));
                while (nestedEnabledActivities.Count > 0)
                {
                    Activity nestedEnabledActivity = nestedEnabledActivities.Dequeue();
                    if (nestedEnabledActivity.PersistOnClose)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString(SR.Error_LRTScopeNestedInNonLRT), ErrorNumbers.Error_LRTScopeNestedInNonLRT));
                        break;
                    }
                    if (nestedEnabledActivity is ICompensatableActivity)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString(SR.Error_NestedCompensatableActivity, nestedEnabledActivity.QualifiedName), ErrorNumbers.Error_NestedCompensatableActivity));
                        break;
                    }

                    if (nestedEnabledActivity is CompositeActivity)
                    {
                        foreach (Activity nestedEnabledActivity2 in Helpers.GetAllEnabledActivities((CompositeActivity)nestedEnabledActivity))
                            nestedEnabledActivities.Enqueue(nestedEnabledActivity2);
                    }
                }

                // check timeout property
                if (atomicTransaction.TimeoutDuration.Ticks < 0)
                {
                    ValidationError timeoutError = new ValidationError(SR.GetString(SR.Error_NegativeValue, new object[] { atomicTransaction.TimeoutDuration.ToString(), "TimeoutDuration" }), ErrorNumbers.Error_NegativeValue);
                    timeoutError.PropertyName = "TimeoutDuration";
                    validationErrors.Add(timeoutError);
                }
            }
            return validationErrors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            if (action == null)
                throw new ArgumentNullException("action");

            AddedActivityAction addedAction = action as AddedActivityAction;

            if (addedAction != null)
            {
                //Check existence of nested PersistOnClose/ICompensatable/SupportsTransaction nested anywhere
                //in the added activity branch
                System.Collections.Generic.Queue<Activity> childrenQueue = new System.Collections.Generic.Queue<Activity>();
                childrenQueue.Enqueue(addedAction.AddedActivity);

                while (childrenQueue.Count != 0)
                {
                    Activity childActivity = childrenQueue.Dequeue();

                    if (childActivity.SupportsTransaction)
                        return new ValidationError(SR.GetString(SR.Error_AtomicScopeNestedInNonLRT), ErrorNumbers.Error_AtomicScopeNestedInNonLRT);

                    if (childActivity.PersistOnClose)
                        return new ValidationError(SR.GetString(SR.Error_NestedPersistOnClose, activity.QualifiedName), ErrorNumbers.Error_NestedPersistOnClose);

                    if (childActivity is ICompensatableActivity)
                        return new ValidationError(SR.GetString(SR.Error_NestedCompensatableActivity, activity.QualifiedName), ErrorNumbers.Error_NestedCompensatableActivity);

                    CompositeActivity compositeActivity = childActivity as CompositeActivity;

                    if (compositeActivity != null)
                    {
                        foreach (Activity grandChild in compositeActivity.EnabledActivities)
                        {
                            childrenQueue.Enqueue(grandChild);
                        }
                    }
                }
            }
            return base.ValidateActivityChange(activity, action);
        }
        #endregion
    }
}
