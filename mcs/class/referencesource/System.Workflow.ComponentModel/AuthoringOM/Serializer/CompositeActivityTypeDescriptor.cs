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

    #region CompositeActivityTypeDescriptor

    internal class CompositeActivityTypeDescriptor : CustomTypeDescriptor
    {
        ICustomTypeDescriptor realTypeDescriptor = null;
        public CompositeActivityTypeDescriptor(ICustomTypeDescriptor realTypeDescriptor)
            : base(realTypeDescriptor)
        {
            this.realTypeDescriptor = realTypeDescriptor;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = base.GetProperties(attributes);
            if (attributes != null && attributes.Length == 1 && attributes[0] is DesignOnlyAttribute && !(attributes[0] as DesignOnlyAttribute).IsDesignOnly)
            {
                ArrayList readonlyProperties = new ArrayList();
                foreach (PropertyDescriptor property in properties)
                    readonlyProperties.Add(property);

                PropertyInfo propInfo = typeof(CompositeActivity).GetProperty("CanModifyActivities", BindingFlags.NonPublic | BindingFlags.Instance);
                readonlyProperties.Add(new ModifyActivitiesPropertyDescriptor(propInfo));
                return new PropertyDescriptorCollection((PropertyDescriptor[])readonlyProperties.ToArray(typeof(PropertyDescriptor)));
            }
            return properties;
        }
    }


    #endregion
}
