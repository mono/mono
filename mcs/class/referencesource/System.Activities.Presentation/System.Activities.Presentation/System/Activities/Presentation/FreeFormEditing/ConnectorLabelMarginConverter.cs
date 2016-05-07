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
    class ConnectorLabelMarginConverter : IMultiValueConverter
    {
        const double EPS = 1e-6;

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The class is only used internally and not accessible externally.")]
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness margin = new Thickness(0);
            PointCollection connectorPoints = values[0] as PointCollection;
            // 8 and 4 is calcuated from the margin / padding settings related to the label in xaml
            double labelBorderWidth = (double)values[1] + 8;
            double labelBorderHeight = (double)values[2] + 4;
            if (connectorPoints != null)
            {
                int longestSegmentIndex;
                DesignerGeometryHelper.LongestSegmentLength(connectorPoints, out longestSegmentIndex);
                if (longestSegmentIndex >= 0)
                {
                    Point labelLocation = DesignerGeometryHelper.MidPointOfLineSegment(connectorPoints[longestSegmentIndex], connectorPoints[longestSegmentIndex + 1]);
                    labelLocation.X = (int)(labelLocation.X - labelBorderWidth / 2 + EPS);
                    labelLocation.Y = (int)(labelLocation.Y - labelBorderHeight / 2 + EPS);
                    margin.Top = labelLocation.Y;
                    margin.Left = labelLocation.X;
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
