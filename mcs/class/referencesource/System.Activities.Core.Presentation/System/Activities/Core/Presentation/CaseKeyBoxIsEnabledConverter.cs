//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.Activities.Core.Presentation
{
    using System.Windows.Data;
    using System.Windows;
    using System.Globalization;

    class CaseKeyBoxIsEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isReadOnly = (bool)values[0];
            bool showExpanded = (bool)values[1];

            if (isReadOnly)
            {
                return false;
            }
            else
            {
                return showExpanded;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
