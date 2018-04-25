//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.Activities.Core.Presentation
{
    using System.Windows.Media;
    using System.Windows.Data;
    using System.Windows;
    using System.Globalization;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;

    class ExpressionToExpressionTextConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string convertedValue = null;
            ModelItem valueMI = value as ModelItem;
            if (valueMI != null)
            {
                convertedValue = ExpressionHelper.GetExpressionString(valueMI.GetCurrentValue() as Activity, valueMI.Parent);
            }
            return convertedValue;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

    }
}
