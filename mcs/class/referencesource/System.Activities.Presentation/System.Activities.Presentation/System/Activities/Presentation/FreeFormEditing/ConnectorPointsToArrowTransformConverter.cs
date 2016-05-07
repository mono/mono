//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "The class is used in xaml.")]
    class ConnectorPointsToArrowTransformConverter : IValueConverter
    {
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The class is only used internally and not accessible externally.")]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            RotateTransform rotateTransform = new RotateTransform(0, double.Parse(parameter.ToString(), CultureInfo.InvariantCulture), double.Parse(parameter.ToString(), CultureInfo.InvariantCulture));
            PointCollection points = value as PointCollection;
            if (points != null && points.Count >= 2)
            {
                rotateTransform.Angle = (int)GetArrowOrientation(points);
            }
            return rotateTransform;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        static ArrowOrientation GetArrowOrientation(PointCollection points)
        {
            Fx.Assert(points.Count >= 2, "Invalid connector");
            ArrowOrientation orientation;
            Point srcPoint = points[points.Count - 2];
            Point destPoint = points[points.Count - 1];

            if (srcPoint.X == destPoint.X)
            {
                orientation = ArrowOrientation.Top;
                if (destPoint.Y > srcPoint.Y)
                {
                    orientation = ArrowOrientation.Bottom;
                }
            }
            else
            {
                orientation = ArrowOrientation.Left;
                if (destPoint.X > srcPoint.X)
                {
                    orientation = ArrowOrientation.Right;
                }
            }
            return orientation;
        }

        enum ArrowOrientation
        {
            Right = 0, Bottom = 90, Left = 180, Top = 270
        };

    }
}
