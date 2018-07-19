//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Windows.Data;
    using System.Runtime;
    using System.Globalization;

    public sealed class GenericTypeArgumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(targetType == typeof(Type), "GenericTypeArgumentConverter is expected to be a Type to Type converter with an integer parameter");
            Fx.Assert(value is Type, "GenericTypeArgumentConverter is expected to be a Type to Type converter with an integer parameter");
            Type source = value as Type;
            return source.GetGenericArguments()[Int32.Parse(parameter.ToString(), culture)];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // It is theoretically impossible to convert from a generic type argument back to its original type.
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
