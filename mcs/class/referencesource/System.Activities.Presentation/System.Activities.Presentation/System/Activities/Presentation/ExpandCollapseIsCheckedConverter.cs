//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows.Data;
    using System.Windows;
    class ExpandCollapseIsCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, global::System.Globalization.CultureInfo culture)
        {
            bool expandState = (bool)values[0];
            bool pinState = (bool)values[1];
            bool showExpanded = false;
            if (values[2] != DependencyProperty.UnsetValue)
            {
                showExpanded = (bool)values[2];
            }
           
            return showExpanded;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, global::System.Globalization.CultureInfo culture)
        {
            //Return ExpandState and PinState.
            return new object[] { value, true };
        }

    }
}
