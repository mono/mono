//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Runtime;

    internal class FlowchartConnectionPointsAdorner : ConnectionPointsAdorner
    {
        private bool isTextRightToLeft;

        public FlowchartConnectionPointsAdorner(UIElement adornedElement, List<ConnectionPoint> connectionPointsToShow, bool isParentShapeSelected, bool isTextRightToLeft)
            : base(adornedElement, connectionPointsToShow, isParentShapeSelected)
        {
            this.isTextRightToLeft = isTextRightToLeft;
            this.FlowDirection = isTextRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const int textCulture = 9;

            ConnectionPoint trueConnectionPoint = null;
            ConnectionPoint falseConnectionPoint = null;

            if (this.connectionPoints.Contains(FlowchartDesigner.GetTrueConnectionPoint(this.AdornedElement)))
            {
                trueConnectionPoint = FlowchartDesigner.GetTrueConnectionPoint(this.AdornedElement);
            }
            if (this.connectionPoints.Contains(FlowchartDesigner.GetFalseConnectionPoint(this.AdornedElement)))
            {
                falseConnectionPoint = FlowchartDesigner.GetFalseConnectionPoint(this.AdornedElement);
            }
            Point actualPoint;
            Point origin = FreeFormPanel.GetLocation(AdornedElement);
            Thickness margin = ((FrameworkElement)AdornedElement).Margin;
            origin.X += margin.Left;
            origin.Y += margin.Top;

            foreach (ConnectionPoint connPoint in this.connectionPoints)
            {
                actualPoint = new Point(connPoint.Location.X - origin.X, connPoint.Location.Y - origin.Y);
                this.DrawConnectionPoint(connPoint, actualPoint, drawingContext);
            }
            
            if (trueConnectionPoint != null)
            {
                string trueLabelText = String.Empty;

                VirtualizedContainerService.VirtualizingContainer virtualizingContainer = (VirtualizedContainerService.VirtualizingContainer)trueConnectionPoint.ParentDesigner;
                if (virtualizingContainer != null && virtualizingContainer.ModelItem != null)
                {
                    trueLabelText = (string)virtualizingContainer.ModelItem.Properties["TrueLabel"].ComputedValue;
                }

                actualPoint = new Point(trueConnectionPoint.Location.X - origin.X, trueConnectionPoint.Location.Y - origin.Y);
                FormattedText trueMarkerFormattedText = new FormattedText(trueLabelText, new System.Globalization.CultureInfo(textCulture),
                    this.FlowDirection, FlowchartDesigner.FlowElementCaptionTypeface, FlowchartDesigner.FlowNodeCaptionFontSize,
                    new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementCaptionColor));
                actualPoint.Y += ConnectionPoint.DrawingLargeSide / 2;
                actualPoint.X -= trueMarkerFormattedText.WidthIncludingTrailingWhitespace;

                DrawtWithTransform(
                    drawingContext,
                    this.isTextRightToLeft,
                    // Mirror the left side text to the right side by using this axis when RTL.
                    actualPoint.X,
                    () =>
                    {
                        drawingContext.DrawText(trueMarkerFormattedText, actualPoint);
                    });
                
            }
            if (falseConnectionPoint != null)
            {
                string falseLabelText = String.Empty;

                VirtualizedContainerService.VirtualizingContainer virtualizingContainer = (VirtualizedContainerService.VirtualizingContainer)falseConnectionPoint.ParentDesigner;
                if (virtualizingContainer != null && virtualizingContainer.ModelItem != null)
                {
                    falseLabelText = (string)virtualizingContainer.ModelItem.Properties["FalseLabel"].ComputedValue;
                }

                actualPoint = new Point(falseConnectionPoint.Location.X - origin.X, falseConnectionPoint.Location.Y - origin.Y);
                actualPoint.Y += ConnectionPoint.DrawingLargeSide / 2;

                FormattedText falseMarkerFormattedText = new FormattedText(falseLabelText, new System.Globalization.CultureInfo(textCulture),
                    this.FlowDirection, FlowchartDesigner.FlowElementCaptionTypeface, FlowchartDesigner.FlowNodeCaptionFontSize,
                    new SolidColorBrush(WorkflowDesignerColors.WorkflowViewElementCaptionColor));

                DrawtWithTransform(
                    drawingContext,
                    this.isTextRightToLeft,
                    // Mirror the left side text to the right side by using this axis when RTL.
                    actualPoint.X,
                    () =>
                    {
                        drawingContext.DrawText(falseMarkerFormattedText, actualPoint);
                    });
            }

            base.OnRender(drawingContext);
        }

        private static void DrawtWithTransform(DrawingContext drawingContext, bool isRightToLeft, double axis, Action doDraw)
        {
            if (isRightToLeft)
            {
                /* We hope to mirror the Text using yAxis as axis, like from (a) to (b)
                 * |                                 |
                 * |    ==>= =>==>                   |     <==<= =<==
                 * |--------|------> x               |----------|--------> x
                 * y       axis    (a)              y           axis      (b)
                 *  
                 * So we do it in three steps:
                 * 1) move text so that axis and y are coincident
                 * |                                            | 
                 * |    ==>= =>==>                          ==>=|=>==>
                 * |--------|------> x                ----------|---------> x
                 * y       axis                                y(axis)
                 * 
                 * 2) mirror
                 *         |                                   | 
                 *     ==>=|=>==>                          <=<=|=<==
                 * --------|------> x                ----------|---------> x
                 *       y(axis)                              y(axis)
                 * 
                 * 3) move back
                 *         |                         |           
                 *     <=<=|=<==                     |     <=<= =<==
                 * --------|------> x                |---------|----------> x
                 *         y                         y        axis 
                 *         
                 * 
                 *              |  1   0   0 |   | -1 0 0 |   |   1    0   0 |   |  -1     0    0 |
                 *  transform = |  0   1   0 | x |  0 1 0 | x |   0    1   0 | = |   0     1    0 |
                 *              | axis 0   1 |   |  0 0 1 |   | -axis  0   0 |   | 2*axis  0    1 |
                 */

                MatrixTransform transform = new MatrixTransform(-1, 0, 0, 1, 2 * axis, 0);
                drawingContext.PushTransform(transform);
                doDraw();
                drawingContext.Pop();
            }
            else
            {
                doDraw();
            }
        }
    }
}
