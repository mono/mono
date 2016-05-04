//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Activities.Presentation;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Media;
    using System.Activities.Presentation.Internal.PropertyEditing;

    internal class ConnectorEditor
    {
        const double EditPointRadius = 4;
        const double EditPointHitTestRadius = 9;
        const int minLengthForSegmentEditPoint = 10;
        EditPoint activeEditPoint;
        AdornerLayer adornerLayer;
        Connector editedConnector;
        List<EditPoint> editPoints;
        FreeFormPanel parentPanel;

        public ConnectorEditor(FreeFormPanel panel, Connector connector)
        {
            if (panel == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("panel"));
            }
            if (connector == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("connector"));
            }
            this.editPoints = new List<EditPoint>();
            this.parentPanel = panel;
            this.editedConnector = connector;
            this.activeEditPoint = null;
            connector.IsSelected = true;
            // When the ConnectorEditor is active, we allow reconnecting the start point of the Connector instead
            // of creating a new transition that shares the same trigger. So we need to disable tooltips and 
            // highlighting effects for all overlapping start dots.
            this.SetIsHitTestVisibleForOverlappingStartDots(false);
            DisplayEditPoints();
        }
        public bool BeingEdited
        {
            get
            {
                return (this.activeEditPoint != null);
            }
        }

        public bool IsConnectorStartBeingMoved
        {
            get
            {
                return (this.BeingEdited && this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint
                    && this.editedConnector.Points[0] != this.EditPoints[0].Location);
            }
        }

        public bool IsConnectorEndBeingMoved
        {
            get
            {
                return (this.BeingEdited && this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint
                    && this.editedConnector.Points[this.editedConnector.Points.Count - 1] != this.EditPoints[this.EditPoints.Count - 1].Location);
            }
        }

        public Connector Connector
        {
            get
            {
                return this.editedConnector;
            }

            set
            {
                this.editedConnector = value;
            }
        }

        List<EditPoint> EditPoints
        {
            get
            {
                return this.editPoints;
            }
        }

        public List<Point> ConnectorEditorLocation
        {
            get
            {
                return this.GetPointsFromEditPoints();
            }
        }

        void SetIsHitTestVisibleForOverlappingStartDots(bool hitTestVisible)
        {
            ConnectionPoint srcConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(this.Connector);
            foreach (Connector overlappingConnector in srcConnectionPoint.AttachedConnectors)
            {
                if (overlappingConnector.StartDot != null)
                {
                    overlappingConnector.StartDot.IsHitTestVisible = hitTestVisible;
                }
            }
        }

        //If the result is true this method also sets the currently active edit point.
        public bool EditPointsHitTest(Point pt)
        {
            if (this.EditPoints.Count > 0)
            {
                foreach (EditPoint editPoint in this.EditPoints)
                {
                    if (DesignerGeometryHelper.DistanceBetweenPoints(pt, editPoint.Location) <= EditPointHitTestRadius)
                    {
                        this.activeEditPoint = editPoint;
                        return true;
                    }
                }
            }
            return false;
        }

        //Connector editing is completed. This function saves the state of the connectorEditor into the corresponding connector.
        //Returns whether the Editor was persisted or not. It might not be persisted if Connector end points do not lie on a designer.
        public bool Persist(Point finalSnappedPoint)
        {
            List<Point> segments = new List<Point>();
            this.Update(finalSnappedPoint);
            if (this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                return false;
            }
            segments = this.GetPointsFromEditPoints();
            this.parentPanel.UpdateConnectorPoints(Connector, segments);
            this.activeEditPoint = null;
            RemoveAdorners();
            DisplayEditPoints();
            return true;
        }

        //The Connector editor is to be destroyed. Remove the adorners on the editor. activeEditPoint=null sets BeingEdited property to false.
        public void Remove()
        {
            this.activeEditPoint = null;
            RemoveAdorners();
            this.EditPoints.Clear();
            this.Connector.IsSelected = false;
            // Restore the IsHitTestVisible property
            this.SetIsHitTestVisibleForOverlappingStartDots(true);
            this.Connector = null;
            this.parentPanel = null;
        }

        //This method removes the existing adorner on the edited connector, updates the active edit points and creates new adorners.
        public void Update(Point newPoint)
        {
            RemoveAdorners();
            UpdateEditPoints(newPoint);
            Fx.Assert(this.activeEditPoint != null, "activeEditPoint is null");
            adornerLayer.Add(new EditPointAdorner(this, editedConnector, true));
        }

        //Add edit points of specified type
        void AddEditPoints(EditPoint.EditPointTypes editPointType)
        {
            if (editPointType == EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                if (this.editPoints.Count == 0 || !this.editPoints[0].Location.Equals(editedConnector.Points[0]))
                {
                    this.editPoints.Insert(0, new EditPoint(EditPoint.EditPointTypes.ConnectionEditPoint, editedConnector.Points[0]));
                }

                if (this.editPoints.Count < 2 || !this.editPoints[this.editPoints.Count - 1].Equals(editedConnector.Points[editedConnector.Points.Count - 1]))
                {
                    editPoints.Add(new EditPoint(EditPoint.EditPointTypes.ConnectionEditPoint, editedConnector.Points[editedConnector.Points.Count - 1]));
                }
            }
            else if (editPointType == EditPoint.EditPointTypes.MultiSegmentEditPoint)
            {
                if (this.editPoints.Count == 2)
                {
                    List<Point> segments = new List<Point>(this.editedConnector.Points);
                    if (segments.Count > 0)
                    {
                        segments.RemoveAt(0);
                        segments.RemoveAt(segments.Count - 1);
                    }

                    for (int i = 0; i < segments.Count; i++)
                    {
                        this.editPoints.Insert(this.editPoints.Count - 1, new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, segments[i]));
                    }
                }
                else
                {
                    Fx.Assert(false, "EditPoints.Count is not 2.");
                }
            }
        }

        void CreateEditPoints()
        {
            this.editPoints.Clear();

            AddEditPoints(EditPoint.EditPointTypes.ConnectionEditPoint);
            AddEditPoints(EditPoint.EditPointTypes.MultiSegmentEditPoint);

            bool validEditPoints = ValidateEditPoints();
            Fx.Assert(validEditPoints, "Validating EditPoints failed.");
        }

        void DisplayEditPoints()
        {
            CreateEditPoints();
            adornerLayer = AdornerLayer.GetAdornerLayer(editedConnector);
            if (adornerLayer != null)
            {
                adornerLayer.Add(new EditPointAdorner(this, editedConnector, false));
            }
        }

        List<Point> GetPointsFromEditPoints()
        {
            List<Point> segments = new List<Point>();
            //Connection end points will never be moved/removed in following two function calls. Hence passing null as pointsToRetain.
            RemoveEditPointSegmentsWithinTolerance(null);
            RemoveCoincidingEditPoints(null);
            for (int i = 0; i < this.EditPoints.Count; i++)
            {
                segments.Add(this.EditPoints[i].Location);
            }
            return segments;
        }

        void RemoveAdorners()
        {
            if (adornerLayer != null && editedConnector != null)
            {
                Adorner[] adorners = adornerLayer.GetAdorners(editedConnector);
                if (adorners != null)
                {
                    foreach (Adorner adorner in adorners)
                    {
                        adornerLayer.Remove(adorner);
                    }
                }
            }
        }

        //Remove points with the same slope
        void RemoveCoincidingEditPoints()
        {
            if (this.editPoints.Count < 2 ||
                this.editPoints[0].Type != EditPoint.EditPointTypes.ConnectionEditPoint ||
                this.editPoints[this.editPoints.Count - 1].Type != EditPoint.EditPointTypes.ConnectionEditPoint ||
                (this.activeEditPoint != null && this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint))
            {
                return;
            }

            //Create list of points to retain
            List<EditPoint> editPointsToRetain = new List<EditPoint>(this.editPoints.Count);
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                if (this.editPoints[i].Type != EditPoint.EditPointTypes.MultiSegmentEditPoint ||
                    this.editPoints[i] == this.activeEditPoint)
                {
                    editPointsToRetain.Add(this.editPoints[i]);
                }
            }

            //Step1: Get rid of all the line segments which are within tolerance range
            RemoveEditPointSegmentsWithinTolerance(editPointsToRetain);

            //Step2: We should make sure that the active edit point is always retained but those points which are coincidental are always removed
            RemoveCoincidingEditPoints(editPointsToRetain);

            //Step3: Go through each segment and ensure that all the segments are either vertical or horizontal
            for (int i = 0; i < this.editPoints.Count - 1; i++)
            {
                EditPoint current = this.editPoints[i];
                EditPoint next = this.editPoints[i + 1];

                double slope = DesignerGeometryHelper.SlopeOfLineSegment(current.Location, next.Location);
                if (slope != 0 && slope != double.MaxValue)
                {
                    Point location = (slope < 1) ? new Point(next.Location.X, current.Location.Y) : new Point(current.Location.X, next.Location.Y);
                    this.editPoints.Insert(i + 1, new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, location));
                }
            }
        }

        void RemoveEditPointSegmentsWithinTolerance(List<EditPoint> pointsToRetain)
        {
            for (int i = 1; i < this.editPoints.Count - 1; i++)
            {
                EditPoint previous = this.editPoints[i - 1];
                EditPoint current = this.editPoints[i];
                EditPoint next = this.editPoints[i + 1];

                if (pointsToRetain == null || !pointsToRetain.Contains(current))
                {
                    double distance = DesignerGeometryHelper.DistanceOfLineSegments(new Point[] { previous.Location, current.Location });
                    if (distance < ConnectorEditor.EditPointRadius && next.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                    {
                        double slope = DesignerGeometryHelper.SlopeOfLineSegment(current.Location, next.Location);
                        next.Location = (slope < 1) ? new Point(next.Location.X, previous.Location.Y) : new Point(previous.Location.X, next.Location.Y);
                        this.editPoints.Remove(current);
                        i -= 1;
                    }
                    else
                    {
                        distance = DesignerGeometryHelper.DistanceOfLineSegments(new Point[] { current.Location, next.Location });
                        if (distance < ConnectorEditor.EditPointRadius && previous.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                        {
                            double slope = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, current.Location);
                            previous.Location = (slope < 1) ? new Point(previous.Location.X, next.Location.Y) : new Point(next.Location.X, previous.Location.Y);
                            this.editPoints.Remove(current);
                            i--;
                        }
                    }
                }
            }

        }

        void RemoveCoincidingEditPoints(List<EditPoint> pointsToRetain)
        {
            for (int i = 1; i < this.EditPoints.Count - 1; i++)
            {
                EditPoint current = this.EditPoints[i];
                if (pointsToRetain == null || !pointsToRetain.Contains(current))
                {
                    EditPoint previous = this.EditPoints[i - 1];
                    EditPoint next = this.EditPoints[i + 1];
                    double slope1 = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, current.Location);
                    double slope2 = DesignerGeometryHelper.SlopeOfLineSegment(current.Location, next.Location);
                    if (Math.Abs(slope1) == Math.Abs(slope2))
                    {
                        this.EditPoints.Remove(current);
                        i -= 1;
                    }
                }
            }
        }

        //Remove edit points of specified type
        //This method does not remove this.activeEditPoint.
        void RemoveEditPoints(EditPoint.EditPointTypes editPointType)
        {
            List<EditPoint> editPointsToRemove = new List<EditPoint>();
            for (int i = 0; i < this.editPoints.Count; i++)
            {
                EditPoint editPoint = this.editPoints[i];
                if (editPoint.Type == editPointType)
                {
                    editPointsToRemove.Add(editPoint);
                }
            }

            for (int i = 0; i < editPointsToRemove.Count; i++)
            {
                EditPoint editPoint = editPointsToRemove[i];
                if (editPoint != this.activeEditPoint)
                {
                    this.editPoints.Remove(editPoint);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This is a legacy algorithm.")]
        void UpdateEditPoints(Point newPoint)
        {
            if (this.editPoints.Count < 2 ||
                this.editPoints[0].Type != EditPoint.EditPointTypes.ConnectionEditPoint ||
                this.editPoints[this.editPoints.Count - 1].Type != EditPoint.EditPointTypes.ConnectionEditPoint)
            {
                Fx.Assert(false, "EditPoints are invalid");
                return;
            }

            if (this.activeEditPoint != null)
            {
                int activeEditPointIndex = this.editPoints.IndexOf(this.activeEditPoint);
                EditPoint previous = (activeEditPointIndex > 0) ? this.editPoints[activeEditPointIndex - 1] : null;
                EditPoint next = (activeEditPointIndex < this.editPoints.Count - 1) ? this.editPoints[activeEditPointIndex + 1] : null;

                //Note that extra edit points are only added if we are connected to connection point
                if (previous != null && previous.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    double slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, this.activeEditPoint.Location);
                    Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;

                    int editPointOffset = Convert.ToInt32(DesignerGeometryHelper.DistanceBetweenPoints(previous.Location, (next != null) ? next.Location : this.activeEditPoint.Location)) / 4;
                    if (orientation == Orientation.Horizontal)
                    {
                        editPointOffset *= (previous.Location.X < this.activeEditPoint.Location.X) ? 1 : -1;
                    }
                    else
                    {
                        editPointOffset *= (previous.Location.Y < this.activeEditPoint.Location.Y) ? 1 : -1;
                    }

                    activeEditPointIndex = this.editPoints.IndexOf(this.activeEditPoint);
                    Point editPointLocation = (orientation == Orientation.Horizontal) ? new Point(previous.Location.X + editPointOffset, previous.Location.Y) : new Point(previous.Location.X, previous.Location.Y + editPointOffset);
                    previous = new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation);
                    this.editPoints.InsertRange(activeEditPointIndex, new EditPoint[] { new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation), previous });
                }

                if (next != null && next.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    double slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(this.activeEditPoint.Location, next.Location);
                    Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;

                    int editPointOffset = Convert.ToInt32(DesignerGeometryHelper.DistanceBetweenPoints((previous != null) ? previous.Location : this.activeEditPoint.Location, next.Location)) / 4;
                    if (orientation == Orientation.Horizontal)
                    {
                        editPointOffset *= (this.activeEditPoint.Location.X < next.Location.X) ? -1 : 1;
                    }
                    else
                    {
                        editPointOffset *= (this.activeEditPoint.Location.Y < next.Location.Y) ? -1 : 1;
                    }

                    activeEditPointIndex = this.editPoints.IndexOf(this.activeEditPoint);
                    Point editPointLocation = (orientation == Orientation.Horizontal) ? new Point(next.Location.X + editPointOffset, next.Location.Y) : new Point(next.Location.X, next.Location.Y + editPointOffset);
                    next = new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation);
                    this.editPoints.InsertRange(activeEditPointIndex + 1, new EditPoint[] { next, new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, editPointLocation) });
                }

                if (this.activeEditPoint.Type == EditPoint.EditPointTypes.ConnectionEditPoint)
                {
                    Fx.Assert(this.editPoints[0].Type == EditPoint.EditPointTypes.ConnectionEditPoint, "EditPoint type is wrong.");
                    Fx.Assert(this.editPoints[editPoints.Count - 1].Type == EditPoint.EditPointTypes.ConnectionEditPoint, "EditPoint type is wrong.");
                    this.activeEditPoint.Location = newPoint;

                    Fx.Assert(this.editPoints.Count > 0, "Some edit point should exist");
                    ConnectionPoint targetConnPt = null;
                    Point[] points = null;
                    Point begin = this.editPoints[0].Location;
                    Point end = this.editPoints[this.editPoints.Count - 1].Location;

                    if (typeof(ConnectionPointsAdorner).IsAssignableFrom(Mouse.DirectlyOver.GetType()))
                    {
                        ConnectionPointsAdorner connPtsAdorner = Mouse.DirectlyOver as ConnectionPointsAdorner;
                        targetConnPt = FreeFormPanel.ConnectionPointHitTest(newPoint, connPtsAdorner);
                    }

                    if (activeEditPointIndex == 0)
                    {
                        // We are dragging the source point of a connector.
                        ConnectionPoint destConnPt = FreeFormPanel.GetDestinationConnectionPoint(this.editedConnector);
                        if (targetConnPt != null)
                        {
                            points = ConnectorRouter.Route(parentPanel, targetConnPt, destConnPt);
                            this.activeEditPoint.Location = targetConnPt.Location;
                        }
                        else
                        {
                            points = ConnectorRouter.Route(parentPanel, begin, destConnPt);
                        }
                    }
                    else
                    {
                        // We are dragging the destination point of a connector.
                        ConnectionPoint srcConnPt = FreeFormPanel.GetSourceConnectionPoint(this.editedConnector);
                        if (targetConnPt != null)
                        {
                            points = ConnectorRouter.Route(parentPanel, srcConnPt, targetConnPt);
                            this.activeEditPoint.Location = targetConnPt.Location;
                        }
                        else
                        {
                            points = ConnectorRouter.Route(parentPanel, srcConnPt, end);
                        }
                    }

                    //When we start editing the end point we need to clear the slate and start over
                    List<EditPoint> newEditPoints = new List<EditPoint>();
                    if (points != null && points.Length > 1)
                    {
                        RemoveEditPoints(EditPoint.EditPointTypes.MultiSegmentEditPoint);
                        for (int i = 1; i < points.Length - 1; ++i)
                        {
                            newEditPoints.Add(new EditPoint(EditPoint.EditPointTypes.MultiSegmentEditPoint, points[i]));
                        }
                        this.editPoints.InsertRange(1, newEditPoints.ToArray());
                    }
                }
                else if (this.activeEditPoint.Type == EditPoint.EditPointTypes.MultiSegmentEditPoint)
                {
                    if (previous != null && previous.Type != EditPoint.EditPointTypes.ConnectionEditPoint && next != null && next.Type != EditPoint.EditPointTypes.ConnectionEditPoint)
                    {
                        //Update the previous point
                        double slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(previous.Location, this.activeEditPoint.Location);
                        Orientation orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;
                        previous.Location = (orientation == Orientation.Horizontal) ? new Point(previous.Location.X, newPoint.Y) : new Point(newPoint.X, previous.Location.Y);

                        //Update the next point
                        slopeOfLine = DesignerGeometryHelper.SlopeOfLineSegment(this.activeEditPoint.Location, next.Location);
                        orientation = (Math.Abs(slopeOfLine) < 1) ? Orientation.Horizontal : Orientation.Vertical;
                        next.Location = (orientation == Orientation.Horizontal) ? new Point(next.Location.X, newPoint.Y) : new Point(newPoint.X, next.Location.Y);

                        //Update the current point
                        this.activeEditPoint.Location = newPoint;
                    }
                    else
                    {
                        Fx.Assert(false, "Should not be here. UpdateEditPoints failed.");
                    }
                }
            }

            // Remove all the redundant edit points
            RemoveCoincidingEditPoints();

            bool validEditPoints = ValidateEditPoints();
            Fx.Assert(validEditPoints, "Validating EditPoints failed.");
        }

        bool ValidateEditPoints()
        {
            if (this.editPoints.Count < 2)
            {
                return false;
            }

            return true;
        }

        class EditPoint
        {
            EditPointTypes editPointType;
            Point point;

            public EditPoint(EditPointTypes editPointType, Point point)
            {
                this.editPointType = editPointType;
                this.point = point;
            }

            public Point Location
            {
                get
                {
                    return this.point;
                }

                set
                {
                    this.point = value;
                }
            }

            public EditPointTypes Type
            {
                get
                {
                    return this.editPointType;
                }
            }

            public enum EditPointTypes
            {
                ConnectionEditPoint = 1, MultiSegmentEditPoint
            }
        }

        sealed class EditPointAdorner : Adorner
        {
            ConnectorEditor adornedEditor;
            bool drawLines;

            public EditPointAdorner(ConnectorEditor cEditor, UIElement adornedElement, bool shouldDrawLines)
                : base(adornedElement)
            {
                Fx.Assert(adornedElement != null, "Adorned element is null.");
                adornedEditor = cEditor;
                this.IsHitTestVisible = false;
                this.drawLines = shouldDrawLines;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                if (drawingContext != null)
                {
                    int i = 0;
                    SolidColorBrush renderBrush = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColor);
                    renderBrush.Opacity = FreeFormPanel.ConnectorEditorOpacity;
                    Pen renderPen = new Pen(new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementSelectedBorderColor), FreeFormPanel.ConnectorEditorThickness);
                    double renderRadius = ConnectorEditor.EditPointRadius;
                    for (i = 0; i < adornedEditor.EditPoints.Count - 1; i++)
                    {
                        drawingContext.DrawEllipse(renderBrush, renderPen, adornedEditor.EditPoints[i].Location, renderRadius, renderRadius);
                        if (drawLines)
                        {
                            drawingContext.DrawLine(renderPen, adornedEditor.EditPoints[i].Location, adornedEditor.EditPoints[i + 1].Location);
                        }
                    }
                    drawingContext.DrawEllipse(renderBrush, renderPen, adornedEditor.EditPoints[i].Location, renderRadius, renderRadius);
                }
                base.OnRender(drawingContext);
                Keyboard.Focus(adornedEditor.Connector);
            }
        }
    }
}
