//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "The class is used in xaml.")]
    class ConnectorPointsToSegmentsConverter : IValueConverter
    {
        const double RoundRadius = 4.0;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PathSegmentCollection retVal = new PathSegmentCollection();
            PointCollection pointCollection = value as PointCollection;
            if (RoundRadius > 0)
            {
                if (pointCollection != null && pointCollection.Count > 0)
                {
                    retVal.Add(new LineSegment(pointCollection[0], true));
                    double curSegmentArcUsed = 0;
                    for (int i = 1; i < pointCollection.Count - 1; i++)
                    {
                        double dist1 = DesignerGeometryHelper.DistanceBetweenPoints(pointCollection[i - 1], pointCollection[i]);
                        double dist2 = DesignerGeometryHelper.DistanceBetweenPoints(pointCollection[i], pointCollection[i + 1]);
                        if (dist1 - curSegmentArcUsed > RoundRadius &&
                            dist2 > RoundRadius)
                        {
                            //build rounded arc at line join.
                            curSegmentArcUsed = RoundRadius;
                            Vector firstSegmentPointingVector = new Vector(pointCollection[i].X - pointCollection[i - 1].X, pointCollection[i].Y - pointCollection[i - 1].Y);
                            Vector secondSegmentPointingVector = new Vector(pointCollection[i + 1].X - pointCollection[i].X, pointCollection[i + 1].Y - pointCollection[i].Y);
                            firstSegmentPointingVector.Normalize();
                            secondSegmentPointingVector.Normalize();
                            Point turningPoint1 = Point.Add(pointCollection[i - 1], Vector.Multiply(dist1 - RoundRadius, firstSegmentPointingVector));
                            Point turningPoint2 = Point.Add(pointCollection[i], Vector.Multiply(RoundRadius, secondSegmentPointingVector));
                            double crossProductZ = firstSegmentPointingVector.X * secondSegmentPointingVector.Y - firstSegmentPointingVector.Y * secondSegmentPointingVector.X;
                            retVal.Add(new LineSegment(turningPoint1, true));
                            retVal.Add(new ArcSegment(turningPoint2, new Size(RoundRadius, RoundRadius), 0, false, crossProductZ > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
                        }
                        else
                        {
                            curSegmentArcUsed = 0;
                            retVal.Add(new LineSegment(pointCollection[i], true));
                        }
                    }
                    retVal.Add(new LineSegment(pointCollection[pointCollection.Count - 1], true));
                }
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

    }
}
