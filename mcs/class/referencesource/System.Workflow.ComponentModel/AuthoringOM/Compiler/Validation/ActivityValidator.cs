namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;

    #region Class ActivityValidator
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ActivityValidator : DependencyObjectValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            Activity activity = obj as Activity;
            if (activity == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(Activity).FullName), "obj");

            if (manager.Context == null)
                throw new ArgumentException("manager", SR.GetString(SR.Error_MissingContextProperty));

            manager.Context.Push(activity);

            ValidationErrorCollection errors = new ValidationErrorCollection();
            errors.AddRange(base.Validate(manager, obj));

            if (activity.Parent == null)
            {
                errors.AddRange(ValidationHelpers.ValidateUniqueIdentifiers(activity));

                if (activity.Enabled == false)
                {
                    ValidationError error = new ValidationError(SR.GetString(SR.Error_RootIsNotEnabled), ErrorNumbers.Error_RootIsNotEnabled);
                    error.PropertyName = "Enabled";
                    errors.Add(error);
                }
            }

            // validate ID property, only if it is not root activity
            Activity rootActivity = Helpers.GetRootActivity(activity);
            if (activity != rootActivity)
            {
                ValidationError identifierError = ValidationHelpers.ValidateNameProperty("Name", manager, activity.Name);
                if (identifierError != null)
                    errors.Add(identifierError);
            }

            try
            {
                errors.AddRange(ValidateProperties(manager, obj));
            }
            finally
            {
                System.Diagnostics.Debug.Assert(manager.Context.Current == activity, "Unwinding contextStack: the item that is about to be popped is not the one we pushed.");
                manager.Context.Pop();
            }

            return errors;
        }
    }
    #endregion
}
