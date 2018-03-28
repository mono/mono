namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Collections;
    using System.Resources;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using Microsoft.CSharp;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Reflection;
    using System.Diagnostics;
    #region ModifyActivitiesPropertyDescriptor

    internal class ModifyActivitiesPropertyDescriptor : PropertyDescriptor
    {
        private PropertyInfo propInfo = null;

        public ModifyActivitiesPropertyDescriptor(PropertyInfo propInfo)
            : base("CanModifyActivities", new Attribute[0])
        {
            this.propInfo = propInfo;
        }

        public override bool CanResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(CompositeActivity);
            }
        }

        public override object GetValue(object component)
        {
            return this.propInfo.GetValue(component, null);
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(bool);
            }
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            this.propInfo.SetValue(component, true, null);
            // Design time, allow changes
            if (component is CompositeActivity)
                (component as CompositeActivity).SetValue(CompositeActivity.CustomActivityProperty, false);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
    #endregion
}
