namespace System.Workflow.ComponentModel.Compiler
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class ActivityCodeGeneratorAttribute : Attribute
    {
        string codeGeneratorTypeName = null;

        public ActivityCodeGeneratorAttribute(Type codeGeneratorType)
        {
            if (codeGeneratorType == null)
                throw new ArgumentNullException("codeGeneratorType");

            if (!typeof(ActivityCodeGenerator).IsAssignableFrom(codeGeneratorType))
                throw new ArgumentException(SR.GetString(SR.Error_NotCodeGeneratorType), "codeGeneratorType");

            if (codeGeneratorType.GetConstructor(new Type[0] { }) == null)
                throw new ArgumentException(SR.GetString(SR.Error_MissingDefaultConstructor, codeGeneratorType.FullName), "codeGeneratorType");

            this.codeGeneratorTypeName = codeGeneratorType.AssemblyQualifiedName;
        }

        public ActivityCodeGeneratorAttribute(string codeGeneratorTypeName)
        {
            if (codeGeneratorTypeName == null)
                throw new ArgumentNullException("codeGeneratorTypeName");

            this.codeGeneratorTypeName = codeGeneratorTypeName;
        }

        public string CodeGeneratorTypeName
        {
            get
            {
                return this.codeGeneratorTypeName;
            }
        }
    }
}
