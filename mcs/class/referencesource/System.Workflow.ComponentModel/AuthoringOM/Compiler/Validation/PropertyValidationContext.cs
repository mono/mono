namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;

    #region PropertyValidationContext
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class PropertyValidationContext
    {
        private string propertyName = string.Empty;
        private object propertyOwner = null;
        private object propertyInfo = null;

        public PropertyValidationContext(object propertyOwner, PropertyInfo propertyInfo, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");
            if (propertyOwner == null)
                throw new ArgumentNullException("propertyOwner");

            this.propertyOwner = propertyOwner;
            this.propertyName = propertyName;
            this.propertyInfo = propertyInfo;
        }

        public PropertyValidationContext(object propertyOwner, DependencyProperty dependencyProperty)
        {
            if (propertyOwner == null)
                throw new ArgumentNullException("propertyOwner");

            this.propertyOwner = propertyOwner;
            this.propertyInfo = dependencyProperty;
        }

        public string PropertyName
        {
            get
            {
                if (this.propertyInfo is DependencyProperty)
                    return ((DependencyProperty)this.propertyInfo).Name;
                else
                    return this.propertyName;
            }
        }

        public object PropertyOwner
        {
            get
            {
                return this.propertyOwner;
            }
        }

        public object Property
        {
            get
            {
                return this.propertyInfo;
            }
        }
    }
    #endregion
}
