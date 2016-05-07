//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;

    class ConnectionPointConverter : IMultiValueConverter
    {
        //This converter returns the actual location of the connection point on the panel.
        //values[0] is the Size of the shape, values[1] is the Location of the shape.
        //Parameters define at what width/height fraction of the shape, should the connectionpoint be located.
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "The class is only used internally and not accessible externally.")]
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Size shapeSize = (Size)values[0];
            double width = shapeSize.Width;
            double height = shapeSize.Height;
            Point origin = (Point)values[1];
            List<object> parameters = (List<object>)parameter;
            double widthFraction = (double)parameters[0];
            double heightFraction = (double)parameters[1];
            Thickness margin = (Thickness)parameters[2];
            origin.X += margin.Left;
            origin.Y += margin.Top;
            width = width - margin.Left - margin.Right;
            height = height - margin.Top - margin.Bottom;
            Point connectionPointLocation = new Point(width * widthFraction + origin.X, height * heightFraction + origin.Y);
            return connectionPointLocation;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

    }
}
