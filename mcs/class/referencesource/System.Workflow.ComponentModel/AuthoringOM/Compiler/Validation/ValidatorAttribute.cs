namespace System.Workflow.ComponentModel.Compiler
{
    using System;

    #region Class ValidatorAttribute
    [AttributeUsageAttribute(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityValidatorAttribute : Attribute
    {
        private string validatorTypeName = null;

        public ActivityValidatorAttribute(Type validatorType)
        {
            if (validatorType != null)
                this.validatorTypeName = validatorType.AssemblyQualifiedName;
        }

        public ActivityValidatorAttribute(string validatorTypeName)
        {
            this.validatorTypeName = validatorTypeName;
        }

        public string ValidatorTypeName
        {
            get
            {
                return this.validatorTypeName;
            }
        }
    }
    #endregion
}
