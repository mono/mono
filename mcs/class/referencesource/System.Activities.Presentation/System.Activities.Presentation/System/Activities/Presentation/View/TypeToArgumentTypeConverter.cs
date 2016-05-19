//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Runtime;

    // This converter converts from InArgument<T>, OutArgument<T>, Activity<T> to T
    // this does not support convert back.
    internal sealed class TypeToArgumentTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = null;
            Type type = value as Type;
            if (type != null)
            {
                if (type.GetGenericArguments().Length > 0)
                {
                    convertedValue = type.GetGenericArguments()[0];
                }
            }
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
