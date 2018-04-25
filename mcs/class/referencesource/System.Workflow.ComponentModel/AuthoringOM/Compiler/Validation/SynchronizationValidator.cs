namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;

    #region Class SynchronizationValidator
    internal sealed class SynchronizationValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            Activity activity = obj as Activity;
            if (activity == null)
                return validationErrors;

            ICollection<string> synchronizationHandles = activity.GetValue(Activity.SynchronizationHandlesProperty) as ICollection<string>;
            if (synchronizationHandles != null)
            {
                foreach (string handle in synchronizationHandles)
                {
                    ValidationError error = ValidationHelpers.ValidateIdentifier("SynchronizationHandles", manager, handle);
                    if (error != null)
                        validationErrors.Add(error);
                }
            }
            return validationErrors;
        }
    }
    #endregion
}
