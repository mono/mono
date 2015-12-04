//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Statements;

    //Returns the maximum of input values. Input values should be of type double.
    class MaxValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double maxValue = double.MinValue;
            foreach (object value in values)
            {
                double val = (double)value;
                if (!double.IsNaN(val) && val > maxValue)
                {
                    maxValue = val;
                }
            }
            if (maxValue == double.MinValue)
            {
                maxValue = double.NaN;
            }
            return maxValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
