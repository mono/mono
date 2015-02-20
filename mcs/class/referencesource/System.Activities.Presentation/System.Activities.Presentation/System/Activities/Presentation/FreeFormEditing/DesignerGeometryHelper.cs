//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    internal static class DesignerGeometryHelper
    {
        public const double EPS = 1e-6;

        public static double ManhattanDistanceBetweenPoints(Point begin, Point end)
        {
            return Math.Abs(begin.X - end.X) + Math.Abs(begin.Y - end.Y);
        }

        public static double DistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        //This function calculates the total length of line segments by adding individual lengths
        public static double DistanceOfLineSegments(Point[] segments)
        {
            double distance = 0;
            for (int i = 1; i < segments.Length; i++)
            {
                distance += DistanceBetweenPoints(segments[i - 1], segments[i]);
            }
            return distance;
        }

        public static Point MidPointOfLineSegment(Point point1, Point point2)
        {
            return new Point(Math.Round((point1.X + point2.X) / 2), Math.Round((point1.Y + point2.Y) / 2));
        }

        public static double SlopeOfLineSegment(Point start, Point end)
        {
            //If line is vertical then the slope is infinite
            if (start.X == end.X)
            {
                return double.MaxValue;
            }

            //If the line is horizontal then slope is 0
            if (start.Y == end.Y)
            {
                return 0;
            }

            return ((end.Y - start.Y) / (end.X - start.X));
        }


        //This function returns the length of the longest segment in a PointsCollection.
        //The segments are assumed to be HORIZONTAL or VERTICAL.
        //the out parameter returns the start point of the longest segment.
        //We always choose the first segment among the segments with max length.
        public static double LongestSegmentLength(PointCollection points, out int longestSegmentIndex)
        {
            double maxLength = 0;
            longestSegmentIndex = -1;
            for (int i = 0; i < points.Count - 1; i++)
            {
                double length = Math.Abs((points[i].X == points[i + 1].X) ? points[i].Y - points[i + 1].Y : points[i].X - points[i + 1].X);
                if (!length.IsEqualTo(maxLength) && length > maxLength) 
                {
                    maxLength = length;
                    longestSegmentIndex = i;
                }
            }
            
            return maxLength;
        }
    }

}
