//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal sealed class HintTextMaxWidthConverter : IValueConverter
    {
        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            // If o == null, this means the Activity inside Default/Cases is null.
            // We need to show SR.ClickToAdd instruction as a whole string without trim.
            // So we need to set the MaxWidth to double.PositiveInfinity. Otherwise, we
            // set it according to the parameter and TextBlock will do proper trimming.
            double maxWidth = double.Parse(parameter as string, CultureInfo.InvariantCulture.NumberFormat);
            return o != null ? maxWidth : DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

    }
}
