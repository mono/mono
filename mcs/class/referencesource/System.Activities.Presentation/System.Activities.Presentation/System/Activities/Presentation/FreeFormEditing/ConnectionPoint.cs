//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Runtime;
    class ConnectionPoint : UIElement
    {
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(Point), typeof(ConnectionPoint));

        private List<Connector> attachedConnectors;
        private UIElement parentDesigner;
        private ConnectionPointKind pointType;

        // Size constants for the rectangle drawn
        internal const double DrawingSmallSide = 4;
        internal const double DrawingLargeSide = 10;

        // Size constants for the hit test area.
        internal const double HitTestSmallSide = 14;
        internal const double HitTestLargeSide = 20;

        public ConnectionPoint()
        {
            pointType = ConnectionPointKind.Default;
            attachedConnectors = new List<Connector>();
            this.parentDesigner = null;
        }

        public List<Connector> AttachedConnectors
        {
            get
            {
                return this.attachedConnectors;
            }
        }

        public Point Location
        {
            get { return (Point)GetValue(ConnectionPoint.LocationProperty); }
            set { SetValue(ConnectionPoint.LocationProperty, value); }
        }

        // This is the vector from the point on the Edge to the top left of the rectangle being drawn.
        public Vector DrawingOffset
        {
            get
            {
                return GetOffset(DrawingSmallSide, DrawingLargeSide);
            }
        }

        // This is the vector from the point on the Edge to the top left of the rectangle being used for hit test.
        public Vector HitTestOffset
        {
            get
            {
                return GetOffset(HitTestSmallSide, HitTestLargeSide);
            }
        }

        // This is the size for the rectangle drawn (size is independent of coordinate system)
        public Size DrawingSize
        {
            get
            {
                return this.GetSize(ConnectionPoint.DrawingSmallSide, ConnectionPoint.DrawingLargeSide);
            }
        }

        // This is the size for the hit test area (size is independent of coordinate system)
        public Size HitTestSize
        {
            get
            {
                return this.GetSize(ConnectionPoint.HitTestSmallSide, ConnectionPoint.HitTestLargeSide);
            }
        }

        public UIElement ParentDesigner
        {
            get
            {
                return this.parentDesigner;
            }
            set
            {
                this.parentDesigner = value;
            }
        }

        public ConnectionPointKind PointType
        {
            get
            {
                return this.pointType;
            }
            set
            {
                this.pointType = value;
            }
        }

        // The list of Points representing the edge of the parent designer where this ConnectionPoint locates with respect to (0,0) of the FreeFormPanel.
        public List<Point> Edge
        {
            get
            {
                FrameworkElement parent = this.ParentDesigner as FrameworkElement;
                Fx.Assert(parent != null, "shape should be a FrameworkElement");
                Point topLeft = FreeFormPanel.GetLocation(parent);
                topLeft.Offset(parent.Margin.Left, parent.Margin.Top);
                double parentWidth = parent.DesiredSize.Width - parent.Margin.Left - parent.Margin.Right;
                double parentHeight = parent.DesiredSize.Height - parent.Margin.Top - parent.Margin.Bottom;
                if (this.Location.X == topLeft.X)
                { //Left Edge
                    return new List<Point> { topLeft, new Point(topLeft.X, topLeft.Y + parentHeight) };
                }
                else if (this.Location.X == topLeft.X + parentWidth)
                { //Right edge
                    return new List<Point> { new Point(topLeft.X + parentWidth, topLeft.Y), new Point(topLeft.X + parentWidth, topLeft.Y + parentHeight) };
                }
                else if (this.Location.Y == topLeft.Y)
                { //Top Edge
                    return new List<Point> { topLeft, new Point(topLeft.X + parentWidth, topLeft.Y) };
                }
                else if (this.Location.Y == topLeft.Y + parentHeight)
                { //Bottom edge
                    return new List<Point> { new Point(topLeft.X, topLeft.Y + parentHeight), new Point(topLeft.X + parentWidth, topLeft.Y + parentHeight) };
                }
                return null;
            }
        }

        public EdgeLocation EdgeLocation
        {
            get;
            set;
        }

        public static ConnectionPoint GetClosestConnectionPoint(List<ConnectionPoint> connectionPoints, Point refPoint, out double minDist)
        {
            minDist = double.PositiveInfinity;
            if (connectionPoints == null || connectionPoints.Count == 0)
            {
                return null;
            }
            double dist = 0;
            ConnectionPoint closestPoint = null;
            foreach (ConnectionPoint point in connectionPoints)
            {
                dist = DesignerGeometryHelper.DistanceBetweenPoints(refPoint, point.Location);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        // This is the vector from the point on the Edge to the top left of a rectangle with a particular (small, large) pair.
        Vector GetOffset(double small, double large)
        {
            return this.EdgeToDrawnMidPointOffset() + this.MidPointToTopLeftOffset(small, large);
        }

        // This is the vector from the point on the Edge to the midpoint of the "drawn" rectangle.
        Vector EdgeToDrawnMidPointOffset()
        {
            double small = ConnectionPoint.DrawingSmallSide;
            switch (this.EdgeLocation)
            {
                case EdgeLocation.Left: return new Vector(-small / 2, 0);
                case EdgeLocation.Right: return new Vector(small / 2, 0);
                case EdgeLocation.Top: return new Vector(0, -small / 2);
                case EdgeLocation.Bottom: return new Vector(0, small / 2);
            }
            Fx.Assert("There is no other possibilities for EdgeDirections");
            // To please compiler
            return new Vector();
        }

        // This is the vector from the midpoint of the rectangle to the top left of the rectangle with a particular (small, large) pair.
        Vector MidPointToTopLeftOffset(double small, double large)
        {
            Size rectSize = GetSize(small, large);
            return new Vector(-rectSize.Width / 2, -rectSize.Height / 2);
        }

        // This is the size for the rectangle with a particular (small, large) pair
        Size GetSize(double small, double large)
        {
            if (this.EdgeLocation == EdgeLocation.Left || this.EdgeLocation == EdgeLocation.Right)
            {
                return new Size(small, large);
            }
            else
            {
                return new Size(large, small);
            }
        }
    }

    enum EdgeLocation
    {
        Left,
        Right,
        Top,
        Bottom
    }
}
