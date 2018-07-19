namespace System.Workflow.ComponentModel.Compiler
{
    using System;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class ConditionValidator : DependencyObjectValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ActivityCondition conditionDeclaration = obj as ActivityCondition;
            if (conditionDeclaration == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(ActivityCondition).FullName), "obj");

            validationErrors.AddRange(ValidateProperties(manager, obj));

            return validationErrors;
        }
    }
}
