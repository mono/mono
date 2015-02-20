namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System.Security.Permissions;

    [Serializable()]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowValidationFailedException : Exception
    {
        private ValidationErrorCollection errors = null;

        private WorkflowValidationFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            this.errors = (ValidationErrorCollection)info.GetValue("errors", typeof(ValidationErrorCollection));

            if (this.errors == null)
                throw new SerializationException(SR.GetString(SR.Error_SerializationInsufficientState));
        }

        public WorkflowValidationFailedException()
            : base(SR.GetString(SR.Error_WorkflowLoadValidationFailed))
        {
        }

        public WorkflowValidationFailedException(string message)
            : base(message)
        {
        }

        public WorkflowValidationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WorkflowValidationFailedException(string message, ValidationErrorCollection errors)
            : base(message)
        {
            if (errors == null)
                throw new ArgumentNullException("errors");

            this.errors = XomlCompilerHelper.MorphIntoFriendlyValidationErrors(errors);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            base.GetObjectData(info, context);

            //ValidationErrorCollection is serializable
            info.AddValue("errors", this.errors, typeof(ValidationErrorCollection));
        }

        public ValidationErrorCollection Errors
        {
            get
            {
                return this.errors;
            }
        }
    }
}
