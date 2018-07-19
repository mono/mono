//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Data;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class HintTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // values represented in XAML
            //   values[0] = ModelItem.XXX
            //   values[1] = ModelItem.XXX.DisplayName

            if (values[0] == null || values[1] == DependencyProperty.UnsetValue)
            {
                return SR.AddActivityHintText;
            }
            else
            {
                return (values[1] as string) ?? string.Empty;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
