//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml.Linq;


    [SuppressMessage("XAML", "XAML1004",
        Justification = "We want to keep this for internal use by the WorkflowDesigner assemblies.")]
    sealed class XNameConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return typeof(string) == sourceType;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string currentValue = (string)value;
            XName result = null;
            if (null != currentValue)
            {
                result = currentValue;
            }
            return result;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return typeof(string) == destinationType;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            XName currentValue = (XName)value;
            string result = null;
            if (null != currentValue)
            {
                result = currentValue.ToString();
            }
            return result;
        }
    }
}
