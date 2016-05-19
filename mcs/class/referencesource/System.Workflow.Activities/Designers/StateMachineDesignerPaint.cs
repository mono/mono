namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization;

    #region StateMachineDesignerPaint

    internal static class StateMachineDesignerPaint
    {
        // same as AmbientTheme.FadeBrush
        internal static readonly Brush FadeBrush = new SolidBrush(Color.FromArgb(120, 255, 255, 255));

        internal static Size MeasureString(Graphics graphics, Font font, string text, StringAlignment alignment, Size maxSize)
        {
            // copied from DesignerHelpers.cs
            SizeF textSize = SizeF.Empty;
            if (maxSize.IsEmpty)
            {
                textSize = graphics.MeasureString(text, font);
            }
            else
            {
                StringFormat format = new StringFormat();
                format.Alignment = alignment;
                format.LineAlignment = StringAlignment.Center;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags = StringFormatFlags.LineLimit;
                textSize = graphics.MeasureString(text, font, new SizeF(maxSize.Width, maxSize.Height), format);
            }

            return new Size(Convert.ToInt32(Math.Ceiling(textSize.Width)), Convert.ToInt32(Math.Ceiling(textSize.Height)));
        }
        /// <summary>
        /// Makes sure that rectangle is completely contained in the bounds rectangle
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        internal static Rectangle TrimRectangle(Rectangle rectangle, Rectangle bounds)
        {
            int left = rectangle.Left;
            int top = rectangle.Top;
            int width = rectangle.Width;
            int height = rectangle.Height;

            if (left < bounds.Left)
                left = bounds.Left;

            if (top < bounds.Top)
                top = bounds.Top;

            if ((left + width) > bounds.Right)
                width -= rectangle.Right - bounds.Right;

            if ((top + height) > bounds.Bottom)
                height -= rectangle.Bottom - bounds.Bottom;

            return new Rectangle(left, top, width, height);
        }

        private static Point[] OptimizeConnectorPoints(Point[] points)
        {
            Debug.Assert(points.Length >= 2);
            List<Point> optimized = new List<Point>();
            optimized.Add(points[0]);
            Point p1;
            Point p2 = points[0];
            Point p3 = points[1];

            if ((p2.X != p3.X) && (p2.Y != p3.Y))
                optimized.Add(new Point(p3.X, p2.Y));

            for (int i = 2; i < points.Length; i++)
            {
                p1 = p2;
                p2 = p3;
                p3 = points[i];
                if ((p1.X == p2.X) && (p2.X == p3.X) ||
                    (p1.Y == p2.Y) && (p2.Y == p3.Y))
                    continue;

                optimized.Add(p2);
                if ((p2.X != p3.X) && (p2.Y != p3.Y))
                    optimized.Add(new Point(p3.X, p2.Y));
            }

            optimized.Add(points[points.Length - 1]);

            return optimized.ToArray();
        }

        internal static void DrawConnector(Graphics graphics, Pen pen, Point[] points, Size connectorCapSize, Size maxCapSize, LineAnchor startConnectorCap, LineAnchor endConnectorCap)
        {
            if (points.GetLength(0) < 2)
                return;

            points = OptimizeConnectorPoints(points);

            //First we start with drawing start cap
            GraphicsPath startCap = null;
            float startCapInset = 0.0f;
            if (startConnectorCap != LineAnchor.None)
            {
                Point[] startSegment = new Point[] { points[0], points[1] };
                int capSize = (startSegment[0].Y == startSegment[1].Y) ? connectorCapSize.Width : connectorCapSize.Height;
                capSize += (capSize % 2);
                capSize = Math.Min(Math.Min(capSize, maxCapSize.Width), maxCapSize.Height);
                startCap = GetLineCap(startConnectorCap, capSize, out startCapInset);

                //Now if user has requested us to fill the line cap then we do so
                //THIS IS A WORKAROUND IN FILLING THE CUSTOM CAPS AS GDI+ HAS A 
                bool fill = (startCap != null && (((int)startConnectorCap % 2) == 0) && (startSegment[0].X == startSegment[1].X || startSegment[0].Y == startSegment[1].Y));
                if (fill)
                {
                    Matrix oldTransform = graphics.Transform;
                    graphics.TranslateTransform(startSegment[0].X, startSegment[0].Y);
                    if (startSegment[0].Y == startSegment[1].Y)
                        graphics.RotateTransform((startSegment[0].X < startSegment[1].X) ? 90.0f : 270.0f);
                    else
                        graphics.RotateTransform((startSegment[0].Y < startSegment[1].Y) ? 180.0f : 0.0f);
                    using (Brush penBrush = new SolidBrush(pen.Color))
                        graphics.FillPath(penBrush, startCap);
                    graphics.Transform = (oldTransform != null) ? oldTransform : new Matrix();
                }
            }

            GraphicsPath endCap = null;
            float endCapInset = 0.0f;
            if (endConnectorCap != LineAnchor.None)
            {
                Point[] endSegment = new Point[] { points[points.GetLength(0) - 2], points[points.GetLength(0) - 1] };
                int capSize = (endSegment[0].Y == endSegment[1].Y) ? connectorCapSize.Width : connectorCapSize.Height;
                capSize += (capSize % 2);
                capSize = Math.Min(Math.Min(capSize, maxCapSize.Width), maxCapSize.Height);
                endCap = GetLineCap(endConnectorCap, capSize, out endCapInset);

                //Now if user has requested us to fill the line cap then we do so,
                //THIS IS A WORKAROUND IN FILLING THE CUSTOM CAPS AS GDI+ HAS A 
                bool fill = (endCap != null && (((int)endConnectorCap % 2) == 0) && (endSegment[0].X == endSegment[1].X || endSegment[0].Y == endSegment[1].Y));
                if (fill)
                {
                    Matrix oldTransform = graphics.Transform;
                    graphics.TranslateTransform(endSegment[1].X, endSegment[1].Y);
                    if (endSegment[0].Y == endSegment[1].Y)
                        graphics.RotateTransform((endSegment[0].X < endSegment[1].X) ? 270.0f : 90.0f);
                    else
                        graphics.RotateTransform((endSegment[0].Y < endSegment[1].Y) ? 0.0f : 180.0f);
                    using (Brush penBrush = new SolidBrush(pen.Color))
                        graphics.FillPath(penBrush, endCap);
                    graphics.Transform = (oldTransform != null) ? oldTransform : new Matrix();
                }
            }

            if (startCap != null)
            {
                CustomLineCap customStartCap = new CustomLineCap(null, startCap);
                customStartCap.WidthScale = 1.0f / pen.Width;
                customStartCap.BaseInset = startCapInset;
                pen.CustomStartCap = customStartCap;
            }

            if (endCap != null)
            {
                CustomLineCap customEndCap = new CustomLineCap(null, endCap);
                customEndCap.WidthScale = 1.0f / pen.Width;
                customEndCap.BaseInset = endCapInset;
                pen.CustomEndCap = customEndCap;
            }

            using (GraphicsPath path = GetRoundedPath(points, StateDesignerConnector.ConnectorPadding / 2))
            {
                graphics.DrawPath(pen, path);
            }

            if (startCap != null)
            {
                CustomLineCap disposableLineCap = pen.CustomStartCap;
                pen.StartCap = LineCap.Flat;
                disposableLineCap.Dispose();
            }

            if (endCap != null)
            {
                CustomLineCap disposableLineCap = pen.CustomEndCap;
                pen.EndCap = LineCap.Flat;
                disposableLineCap.Dispose();
            }
        }

        private static GraphicsPath GetRoundedPath(Point[] points, int radius)
        {
            Debug.Assert(points.Length >= 2);
            GraphicsPath path = new GraphicsPath();
            if (points.Length == 2)
            {
                path.AddLine(points[0], points[1]);
                return path;
            }

            int diameter = radius * 2;

            Point p1 = points[0];
            Point p2 = points[1];
            Point p3 = points[2];
            int previousConnectorSize;
            int currentConnectorSize = GetDistance(p1, p2);
            int nextConnectorSize = GetDistance(p2, p3);
            ArrowDirection direction1 = GetDirection(p1, p2);
            ArrowDirection direction2 = GetDirection(p2, p3);

            if (currentConnectorSize < diameter || nextConnectorSize < diameter)
            {
                AddSegment(path, radius, p1, p2, false, false, direction1);
            }
            else
            {
                AddSegment(path, radius, p1, p2, false, true, direction1);
                AddRoundedCorner(path, diameter, p2, direction1, direction2);
            }
            int i = 2;
            while (i < (points.Length - 1))
            {
                previousConnectorSize = currentConnectorSize;
                currentConnectorSize = nextConnectorSize;
                direction1 = direction2;
                p1 = p2;
                p2 = p3;
                p3 = points[i + 1];
                direction2 = GetDirection(p2, p3);
                nextConnectorSize = GetDistance(p2, p3);
                if (currentConnectorSize >= diameter && nextConnectorSize >= diameter)
                {
                    AddSegment(path, radius, p1, p2, (previousConnectorSize >= diameter), true, direction1);
                    AddRoundedCorner(path, diameter, p2, direction1, direction2);
                }
                else
                {
                    AddSegment(path, radius, p1, p2, (previousConnectorSize >= diameter), false, direction1);
                }

                i++;
            }

            AddSegment(path, radius, p2, p3,
                (currentConnectorSize >= diameter && nextConnectorSize >= diameter),
                false, direction2);

            return path;
        }

        private static int GetDistance(Point p1, Point p2)
        {
            if (p1.X == p2.X)
                return Math.Abs(p1.Y - p2.Y);
            else
                return Math.Abs(p1.X - p2.X);
        }

        private static void AddSegment(GraphicsPath path, int radius, Point p1, Point p2, bool roundP1, bool roundP2, ArrowDirection direction)
        {
            if (roundP1)
            {
                switch (direction)
                {
                    case ArrowDirection.Down:
                        p1.Y += radius;
                        break;
                    case ArrowDirection.Up:
                        p1.Y -= radius;
                        break;
                    case ArrowDirection.Left:
                        p1.X -= radius;
                        break;
                    default:
                        p1.X += radius;
                        break;
                }
            }
            if (roundP2)
            {
                switch (direction)
                {
                    case ArrowDirection.Down:
                        p2.Y -= radius;
                        break;
                    case ArrowDirection.Up:
                        p2.Y += radius;
                        break;
                    case ArrowDirection.Left:
                        p2.X += radius;
                        break;
                    default:
                        p2.X -= radius;
                        break;
                }
            }
            path.AddLine(p1, p2);
        }

        private static void AddRoundedCorner(GraphicsPath path, int diameter, Point midPoint, ArrowDirection direction1, ArrowDirection direction2)
        {
            switch (direction1)
            {
                case ArrowDirection.Left:
                    if (direction2 == ArrowDirection.Down)
                        path.AddArc(midPoint.X, midPoint.Y, diameter, diameter, 270f, -90f);
                    else
                        path.AddArc(midPoint.X, midPoint.Y - diameter, diameter, diameter, 90f, 90f);
                    break;
                case ArrowDirection.Right:
                    if (direction2 == ArrowDirection.Down)
                        path.AddArc(midPoint.X - diameter, midPoint.Y, diameter, diameter, 270f, 90f);
                    else
                        path.AddArc(midPoint.X - diameter, midPoint.Y - diameter, diameter, diameter, 90f, -90f);
                    break;
                case ArrowDirection.Up:
                    if (direction2 == ArrowDirection.Left)
                        path.AddArc(midPoint.X - diameter, midPoint.Y, diameter, diameter, 0f, -90f);
                    else
                        path.AddArc(midPoint.X, midPoint.Y, diameter, diameter, 180f, 90f);
                    break;
                default:
                    if (direction2 == ArrowDirection.Left)
                        path.AddArc(midPoint.X - diameter, midPoint.Y - diameter, diameter, diameter, 0f, 90f);
                    else
                        path.AddArc(midPoint.X, midPoint.Y - diameter, diameter, diameter, 180f, -90f);
                    break;
            }
        }

        private static ArrowDirection GetDirection(Point start, Point end)
        {
            // we only support vertical or horizotal lines. No diagonals
            Debug.Assert(start.X == end.X || start.Y == end.Y);

            if (start.X == end.X)
                // vertical
                if (start.Y < end.Y)
                    // top to bottom
                    return ArrowDirection.Down;
                else
                    // Bottom to Top
                    return ArrowDirection.Up;
            else
                // horizontal
                if (start.X < end.X)
                    // left to right
                    return ArrowDirection.Right;
                else
                    // right to left
                    return ArrowDirection.Left;
        }



        //

        internal static GraphicsPath GetLineCap(LineAnchor lineCap, int capsize, out float capinset)
        {
            //WE DO NOT SUPPORT ARROWCAPS FOR ANGULAR CONNECTORS FOR NOW
            capinset = 0.0f;
            capinset = (float)capsize / 2;
            Size capSize = new Size(capsize, capsize);

            GraphicsPath lineCapPath = new GraphicsPath();
            switch (lineCap)
            {
                case LineAnchor.Arrow:
                case LineAnchor.ArrowAnchor:
                    int arcRadius = capSize.Height / 3;
                    lineCapPath.AddLine(capSize.Width / 2, -capSize.Height, 0, 0);
                    lineCapPath.AddLine(0, 0, -capSize.Width / 2, -capSize.Height);
                    lineCapPath.AddLine(-capSize.Width / 2, -capSize.Height, 0, -capSize.Height + arcRadius);
                    lineCapPath.AddLine(0, -capSize.Height + arcRadius, capSize.Width / 2, -capSize.Height);
                    capinset = capSize.Height - arcRadius;
                    break;

                case LineAnchor.Diamond:
                case LineAnchor.DiamondAnchor:
                    lineCapPath.AddLine(0, -capSize.Height, capSize.Width / 2, -capSize.Height / 2);
                    lineCapPath.AddLine(capSize.Width / 2, -capSize.Height / 2, 0, 0);
                    lineCapPath.AddLine(0, 0, -capSize.Width / 2, -capSize.Height / 2);
                    lineCapPath.AddLine(-capSize.Width / 2, -capSize.Height / 2, 0, -capSize.Height);
                    break;

                case LineAnchor.Round:
                case LineAnchor.RoundAnchor:
                    lineCapPath.AddEllipse(new Rectangle(-capSize.Width / 2, -capSize.Height, capSize.Width, capSize.Height));
                    break;

                case LineAnchor.Rectangle:
                case LineAnchor.RectangleAnchor:
                    lineCapPath.AddRectangle(new Rectangle(-capSize.Width / 2, -capSize.Height, capSize.Width, capSize.Height));
                    break;

                case LineAnchor.RoundedRectangle:
                case LineAnchor.RoundedRectangleAnchor:
                    arcRadius = capSize.Height / 4;
                    lineCapPath.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(new Rectangle(-capSize.Width / 2, -capSize.Height, capSize.Width, capSize.Height), arcRadius), true);
                    break;
            }

            lineCapPath.CloseFigure();
            return lineCapPath;
        }

        internal static GraphicsPath GetDesignerPath(ActivityDesigner designer, Rectangle bounds, ActivityDesignerTheme designerTheme)
        {
            GraphicsPath designerPath = new GraphicsPath();

            if (designer == GetSafeRootDesigner(designer.Activity.Site) && ((IWorkflowRootDesigner)designer).InvokingDesigner == null)
            {
                designerPath.AddRectangle(bounds);
            }
            else
            {
                // Work around: This should come from AmbientTheme.ArcDiameter
                // but it is internal
                int arcDiameter = 8;
                if (designerTheme != null && designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle)
                    designerPath.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(bounds, arcDiameter), true);
                else
                    designerPath.AddRectangle(bounds);
            }

            return designerPath;
        }

        internal static ActivityDesigner GetSafeRootDesigner(IServiceProvider serviceProvider)
        {
            return (serviceProvider != null) ? ActivityDesigner.GetRootDesigner(serviceProvider) : null;
        }
    }
    #endregion
}
