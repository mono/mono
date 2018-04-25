namespace System.Workflow.ComponentModel.Compiler
{
    using System;

    #region BindValidationContext

    [Flags]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public enum AccessTypes
    {
        Read = 0x01,
        Write = 0x02,
        ReadWrite = Read | Write
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class BindValidationContext
    {
        private Type targetType = null;
        private AccessTypes access = AccessTypes.Read;

        public BindValidationContext(Type targetType)
            : this(targetType, AccessTypes.Read)
        {
        }

        public BindValidationContext(Type targetType, AccessTypes access)
        {
            if (targetType == null)
                throw new ArgumentNullException("targetType");
            this.targetType = targetType;
            this.access = access;
        }

        public Type TargetType
        {
            get
            {
                return this.targetType;
            }
        }

        public AccessTypes Access
        {
            get
            {
                return this.access;
            }
        }
    }

    #endregion
}
