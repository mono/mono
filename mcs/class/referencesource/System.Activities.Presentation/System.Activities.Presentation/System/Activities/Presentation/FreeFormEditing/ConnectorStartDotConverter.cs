//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "The class is used in xaml.")]
    class ConnectorStartDotConverter : IValueConverter
    {
        const double Radius = 5.0;
        const double RadiusForHitTest = 10.0;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Point point = (Point)value;
            bool[] parameters = (bool[])parameter;
            double radius = Radius;
            if (parameters[1])
            {
                radius = RadiusForHitTest;
            }
            if (parameters[0]) // left point
            {
                return new Point((point.X - radius), point.Y);
            }
            else // right point
            {
                return new Point((point.X + radius), point.Y);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
