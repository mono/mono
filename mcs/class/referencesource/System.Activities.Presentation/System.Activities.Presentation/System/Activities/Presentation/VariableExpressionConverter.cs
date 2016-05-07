//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;

    class VariableExpressionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = null;
            if (value != null)
            {
                if (value is ModelItem)
                {
                    convertedValue = ((ModelItem)value).GetCurrentValue();
                }
                else
                {
                    convertedValue = value;
                }
            }
            return convertedValue;
        }
    }
}
