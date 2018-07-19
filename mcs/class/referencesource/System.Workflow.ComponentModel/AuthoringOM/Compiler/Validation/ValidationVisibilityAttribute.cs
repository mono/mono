namespace System.Workflow.ComponentModel.Compiler
{
    using System;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum ValidationOption
    {
        None,
        Optional,
        Required
    }

    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ValidationOptionAttribute : Attribute
    {
        private ValidationOption validationOption;

        public ValidationOptionAttribute(ValidationOption validationOption)
        {
            this.validationOption = validationOption;
        }

        public ValidationOption ValidationOption
        {
            get
            {
                return this.validationOption;
            }
        }
    }
}
