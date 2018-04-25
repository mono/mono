//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.ComponentModel;

    class WorkflowServiceAttributesTypeConverter : TypeConverter
    {

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            // TypeDescriptor.GetProperties is used here to get the sub properties of the property that we want to be able
            // to be expandable in the property browser
            PropertyDescriptorCollection subProperties = TypeDescriptor.GetProperties(value, new Attribute[] { new BrowsableAttribute(true) });
            return subProperties;
        }
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            // This is to make the property expandable into sub properties int he property browser
            return true;
        }
    }
}
