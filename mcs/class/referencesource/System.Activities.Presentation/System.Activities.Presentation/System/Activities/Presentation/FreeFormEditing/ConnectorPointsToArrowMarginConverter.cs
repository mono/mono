//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "The class is used in xaml.")]
    class ConnectorPointsToArrowMarginConverter : IMultiValueConverter
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The class is only used internally and not accessible externally.")]
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            Thickness margin = new Thickness(0);
            PointCollection points = values[0] as PointCollection;
            RotateTransform transform = values[1] as RotateTransform;
            int offset = int.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            if (points != null && points.Count > 0)
            {
                margin.Left = points[points.Count - 1].X - offset;
                margin.Top = points[points.Count - 1].Y - offset;
            }
            if (transform != null)
            {
                switch ((int)transform.Angle)
                {
                    case 0:
                        margin.Left -= offset;
                        break;
                    case 90:
                        margin.Top -= offset;
                        break;
                    case 180:
                        margin.Left += offset;
                        break;
                    case 270:
                        margin.Top += offset;
                        break;
                }
            }
            return margin;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
