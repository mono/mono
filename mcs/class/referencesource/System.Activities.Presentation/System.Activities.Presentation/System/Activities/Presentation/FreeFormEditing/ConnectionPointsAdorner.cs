//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Runtime;

    internal class ConnectionPointsAdorner : Adorner
    {
        SolidColorBrush renderBrush;
        Pen renderPen;
        protected List<ConnectionPoint> connectionPoints;

        internal List<ConnectionPoint> ConnectionPoints
        {
            get { return this.connectionPoints; }
        }

        public ConnectionPointsAdorner(UIElement adornedElement, List<ConnectionPoint> connectionPointsToShow, bool isParentShapeSelected)
            : base(adornedElement)
        {
            Fx.Assert(adornedElement != null, "adornedElement is null");
            this.connectionPoints = connectionPointsToShow;
            if (isParentShapeSelected)
            {
                this.renderBrush = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementSelectedBackgroundColor);
                this.renderPen = new Pen(new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementSelectedBorderColor), 1.0);
            }
            else
            {
                this.renderBrush = new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementBackgroundColor);
                this.renderPen = new Pen(new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementBorderColor), 1.0);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.IsHitTestVisible = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            AdornedElement.RaiseEvent(e);
        }

        protected void DrawConnectionPoint(ConnectionPoint connPoint, Point actualLocation, DrawingContext drawingContext)
        {
            if (connPoint.IsEnabled)
            {
                // actualLocation is the point on the Edge with respect to the coordinate system defined by the top left corner of the adorned element
                // We will need this transparent rectangle to make sure OnMouseOver event can be triggered, for hit test.
                drawingContext.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Transparent, 0),
                    new Rect(actualLocation + connPoint.HitTestOffset, connPoint.HitTestSize));
                drawingContext.DrawRectangle(this.renderBrush, this.renderPen,
                    new Rect(actualLocation + connPoint.DrawingOffset, connPoint.DrawingSize));
            }
        }
    }
}
