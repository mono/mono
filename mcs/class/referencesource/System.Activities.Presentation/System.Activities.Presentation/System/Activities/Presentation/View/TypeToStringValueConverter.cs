//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Runtime;

    class TypeToStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(targetType.Equals(typeof(string)), "TypeToStringValueConverter cannot convert a type to type " + targetType.FullName);
            string target = null;
            if (value != null)
            {
                Fx.Assert(value is Type, string.Format(CultureInfo.InvariantCulture, "TypeToStringValueConverter cannot convert from type {0} to string", value.GetType().FullName));
                Type editedType = (Type)value;
                //handle primitive types
                if (editedType.IsPrimitive || editedType.IsValueType ||
                    editedType == typeof(string) || editedType == typeof(object))
                {
                    target = editedType.Name;
                }
                    //and other ones
                else
                {
                    target = editedType.FullName;
                }
            }
            return target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(targetType.Equals(typeof(Type)), "TypeToStringValueConverter cannot convert string back to type " + targetType.FullName);
            Type target = null;
            string stringValue = value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                // try to get the type from the type name
                target = Type.GetType(stringValue, false, true);
                //handle primitive types
                if (null == target)
                {
                    stringValue = string.Format(CultureInfo.InvariantCulture, "System.{0}", stringValue);
                    target = Type.GetType(stringValue, false, true);
                }
                if (null == target)
                {
                    return Binding.DoNothing;
                }
            }
            return target;
        }
    }
}
